using Certes;
using Certes.Acme;
using System;
using System.IO;
using System.Linq;

namespace Cert_requester
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Avaliable commands: getnew getold validate generate exit\n");

      bool shouldExit = false;
      while (!shouldExit)
      {
        string command = Console.ReadLine();

        switch (command)
        {
          case "getnew":
            GetChallengeNewAcc();
            break;
          case "getold":
            GetChallengeOldAcc();
            break;
          case "validate":
            ValidateChallenge();
            break;
          case "generate":
            GenerateCert();
            break;
          case "exit":
            shouldExit = true;
            break;
          default:
            break;
        }
      }
    }

    static async void GetChallengeNewAcc()
    {
      var acme = new AcmeContext(WellKnownServers.LetsEncryptV2);
      var account = await acme.NewAccount("restgogi0@mail.ru", true);
      var pemKey = acme.AccountKey.ToPem();
      File.WriteAllText("pem_key.txt", pemKey);

      var order = await acme.NewOrder(new[] { "mrwarrior-001-site1.dtempurl.com" });
      var orderUri = order.Location;
      File.WriteAllText("orderUri.txt", orderUri.ToString());



      var authz = (await order.Authorizations()).First();
      var httpChallenge = await authz.Http();
      var keyAuthz = httpChallenge.KeyAuthz;
      File.WriteAllText(httpChallenge.Token, keyAuthz);
    }

    static async void GetChallengeOldAcc()
    {
      var pemKey = File.ReadAllText("pem_key.txt");
      var accountKey = KeyFactory.FromPem(pemKey);
      var acme = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
      var account = await acme.Account();

      var stringOrderUri = File.ReadAllText("orderUri.txt");
      var orderUri = new Uri(stringOrderUri);
      var order = acme.Order(orderUri);



      var authz = (await order.Authorizations()).First();
      var httpChallenge = await authz.Http();
      var keyAuthz = httpChallenge.KeyAuthz;
      File.WriteAllText(httpChallenge.Token, keyAuthz);
    }

    static async void ValidateChallenge()
    {
      var pemKey = File.ReadAllText("pem_key.txt");
      var accountKey = KeyFactory.FromPem(pemKey);
      var acme = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
      var account = await acme.Account();

      var stringOrderUri = File.ReadAllText("orderUri.txt");
      var orderUri = new Uri(stringOrderUri);
      var order = acme.Order(orderUri);



      var authz = (await order.Authorizations()).First();
      var httpChallenge = await authz.Http();
      var keyAuthz = httpChallenge.KeyAuthz;
      File.WriteAllText(httpChallenge.Token, keyAuthz);


      await httpChallenge.Validate();
    }


    static async void GenerateCert()
    {
      var pemKey = File.ReadAllText("pem_key.txt");
      var accountKey = KeyFactory.FromPem(pemKey);
      var acme = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
      var account = await acme.Account();

      var stringOrderUri = File.ReadAllText("orderUri.txt");
      var orderUri = new Uri(stringOrderUri);
      var order = acme.Order(orderUri);



      var authz = (await order.Authorizations()).First();
      var httpChallenge = await authz.Http();
      var keyAuthz = httpChallenge.KeyAuthz;
      File.WriteAllText(httpChallenge.Token, keyAuthz);



      var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
      var cert = await order.Generate(new CsrInfo
      {
        CountryName = "RU",
        State = "MSK",
        Locality = "MSK",
        Organization = "ORG",
        OrganizationUnit = "ORGUNIT",
        CommonName = "mrwarrior-001-site1.dtempurl.com",
      }, privateKey);

      var pfxBuilder = cert.ToPfx(privateKey);
      var pfx = pfxBuilder.Build("my-cert", "abcd1234");
      File.WriteAllBytes("cert.pfx", pfx);
    }
  }
}
