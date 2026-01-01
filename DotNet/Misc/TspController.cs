using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using Attribute = Org.BouncyCastle.Asn1.Cms.Attribute;

namespace TimeStampService
{
    [AllowAnonymous]
    [ApiController]
    [Route("api")]
    public sealed class TspController : Controller
    {
        [HttpPost("tsp")]
        public async Task<IActionResult> Tsp(CancellationToken cancellationToken = default)
        {
            TspResponder tsResponder = new(
                System.IO.File.ReadAllBytes("timestamping_cert.crt"),
                System.IO.File.ReadAllBytes("timestamping_cert.key"),
                "SHA256");

            byte[] bRequest = await Request.Body.ReadAllBytesAsync();
            byte[] bResponse = tsResponder.GenResponse(bRequest, DateTime.UtcNow);

            Response.StatusCode = 200;
            Response.ContentType = "application/timestamp-reply";
            await Response.BodyWriter.WriteAsync(bResponse, cancellationToken);
            return new EmptyResult();
        }
    }
	
	public class TspResponder
    {
        private readonly X509Certificate x509Cert;
        private readonly AsymmetricKeyParameter priKey;
        private readonly IX509Store x509Store;
        private readonly string hashAlg;

        public TspResponder(byte[] x509Cert, byte[] priKey, string hashAlg)
        {
            this.x509Cert = new X509CertificateParser().ReadCertificate(x509Cert);

            object keyObject = new PemReader(new StreamReader(new MemoryStream(priKey))).ReadObject();
            this.priKey = (AsymmetricKeyParameter)keyObject;

            List<object> certList = new();
            foreach (var cert in new X509CertificateParser().ReadCertificates(x509Cert))
                certList.Add(cert);
            this.x509Store = X509StoreFactory.Create("Certificate/Collection", new X509CollectionStoreParameters(certList));

            this.hashAlg = hashAlg;
        }

        public byte[] GenResponse(byte[] bRequest, DateTime signTime)
        {
            byte[] bSerial = new byte[16];
            new Random().NextBytes(bSerial);
            BigInteger biSerial = new(1, bSerial);
            return RFC3161(bRequest, signTime, biSerial);
        }

        private byte[] RFC3161(byte[] bRequest, DateTime signTime, BigInteger biSerial)
        {
            TimeStampRequest timeStampRequest = new(bRequest);

            //Asn1EncodableVector signedAttributes = new();
            //signedAttributes.Add(new Attribute(CmsAttributes.ContentType, new DerSet(new DerObjectIdentifier("1.2.840.113549.1.7.1"))));
            //signedAttributes.Add(new Attribute(CmsAttributes.SigningTime, new DerSet(new DerUtcTime(signTime))));
            //AttributeTable signedAttributesTable = new(signedAttributes);
            //signedAttributesTable.ToAsn1EncodableVector();

            TimeStampTokenGenerator timeStampTokenGenerator = new(
                priKey,
                x509Cert,
                new DefaultDigestAlgorithmIdentifierFinder().find(hashAlg).Algorithm.Id,
                "1.3.6.1.4.1.13762.3");
            timeStampTokenGenerator.SetCertificates(x509Store);
            timeStampTokenGenerator.SetTsa(new GeneralName(x509Cert.SubjectDN));

            TimeStampResponseGenerator timeStampResponseGenerator = new(
                timeStampTokenGenerator,
                TspAlgorithms.Allowed);

            TimeStampResponse timeStampResponse = timeStampResponseGenerator.Generate(timeStampRequest, biSerial, signTime);
            byte[] result = timeStampResponse.GetEncoded();
            return result;
        }
    }
}
