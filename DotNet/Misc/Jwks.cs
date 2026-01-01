using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace MyJwks;

public class JwkKeys : IDisposable
{
    private readonly ECDsa _privateKey;
    private readonly ECParameters _ecParameters;

    public ECDsaSecurityKey SecurityKey { get; private set; }
    public JwkPubKey PubKey { get; private set; }

    public JwkKeys()
    {
        _privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _ecParameters = _privateKey.ExportParameters(false);
        SecurityKey = new ECDsaSecurityKey(_privateKey) { KeyId = Guid.NewGuid().ToString() };
        PubKey = CreateJwkEs256PubKey(_ecParameters, SecurityKey);
    }

    public void Dispose()
    {
        _privateKey.Dispose();
    }

    private JwkPubKey CreateJwkEs256PubKey(ECParameters ecParameters, ECDsaSecurityKey securityKey) => new()
    {
        kid = securityKey.KeyId,
        kty = "EC",
        crv = "P-256",
        x = Base64UrlEncode(ecParameters.Q.X ?? Guid.NewGuid().ToByteArray()),
        y = Base64UrlEncode(ecParameters.Q.Y ?? Guid.NewGuid().ToByteArray()),
        use = "sig",
        alg = "ES256",
    };

    private static string Base64UrlEncode(byte[] input)
    {
        string base64 = Convert.ToBase64String(input);
        base64 = base64.Replace('+', '-').Replace('/', '_');
        return base64.TrimEnd('=');
    }

    public class JwkPubKey
    {
        public string kty { get; set; }
        public string crv { get; set; }
        public string x { get; set; }
        public string y { get; set; }
        public string use { get; set; }
        public string alg { get; set; }
        public string kid { get; set; }
    }
}

[Controller]
[Route("pubkeys")]
public class PubKeysController(JwkKeys jwkKeys) : Controller
{
    private readonly JwkKeys _jwkKeys = jwkKeys;

    [HttpGet("jwks")]
    public IActionResult GetJwks()
    {
        return Ok(new { keys = new List<JwkKeys.JwkPubKey> { _jwkKeys.PubKey } });
    }
}

public class JwkTokenHelper(JwkKeys jwkKeys)
{
    private readonly JwkKeys _jwkKeys = jwkKeys;

    public string CreateToken(string login)
    {
        DateTimeOffset exp = DateTimeOffset.UtcNow.AddMinutes(1);

        List<Claim> claims = new()
        {
            new("iss", "my_issuer"),
            new("aud", "my_audience"),
            new("sub", login),
            new("exp", exp.ToUnixTimeSeconds().ToString()),
        };

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = exp.DateTime,
            SigningCredentials = new SigningCredentials(_jwkKeys.SecurityKey, SecurityAlgorithms.EcdsaSha256),
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    public bool ValidateToken(string tokenStr, string login, out string error)
    {
        error = null;

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(tokenStr);

        if (string.IsNullOrWhiteSpace(token.Payload.Iss) || string.IsNullOrWhiteSpace(token.Payload.Sub))
            return false;
        if (token.Payload.IssuedAt > DateTime.UtcNow)
            return false;
        if (DateTimeOffset.FromUnixTimeSeconds((long)token.Payload.Expiration).UtcDateTime < DateTime.UtcNow)
            return false;
        if (token.Payload.Sub != login)
            return false;

        JwkKeys.JwkPubKey pubkey = [_jwkKeys.PubKey]
            .FirstOrDefault(x => x.alg == token.Header.Alg && x.kid == token.Header.Kid);
        string jwkJson = JsonConvert.SerializeObject(pubkey);

        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = token.Issuer,
            ValidateAudience = true,
            ValidAudience = token.Audiences.First(),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new JsonWebKey(jwkJson)
        };

        try
        {
            ClaimsPrincipal _ = handler.ValidateToken(tokenStr, validationParameters, out SecurityToken _);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
