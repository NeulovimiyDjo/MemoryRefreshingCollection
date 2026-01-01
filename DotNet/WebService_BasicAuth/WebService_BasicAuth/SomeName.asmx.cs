using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using System.Xml.Serialization;

namespace WebService_BasicAuth
{
    /// <summary>
    /// Summary description for SomeName
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService_BasicAuth : System.Web.Services.WebService
    {
        private static readonly Logger logger = CommonStaticVariables.logger;
		private static readonly HttpClient client = new HttpClient() { BaseAddress = new Uri("SomeAddress") };


        [WebMethod]
        public string SomeWebMethod(string param)
        {
            logger.Info($"SomeWebMethod executing");
			
			var param = new Dictionary<string, string> { { "ParamName", "ParamValue" } };
			var encodedParam = new FormUrlEncodedContent(parameters);

			HttpResponseMessage response = client.PostAsync("SomeControllerName/SomeMethodName", encodedParam).GetAwaiter().GetResult();
			response.EnsureSuccessStatusCode();
			
			return response.Something;
        }


		[WebMethod]
        public static string GetSomeExternalService2Data(string param)
        {
            EndpointAddress endpointAdress = new EndpointAddress(SomeExternalService2Address);
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport; // Required for ssl
            var client = new SomeExternalService2(binding, endpointAdress);

            if (Global.IgnoreCertificate == true)
            {
                client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = // For development only
                    new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
            }


            using (var scope = new OperationContextScope(client.InnerChannel))
            {
                // Add a basic-auth HTTP Header to an outgoing request
                string auth = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
                HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["Authorization"] = auth;
                if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;
                }
                else
                {
                    OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, requestMessage);
                }


                var result = client.SomeExternalService2Method(param);

                return result;
            }
        }

		[WebMethod]
        public static string GetSomeExternalServiceData(string param)
        {
            var client = new SomeExternalService();
            client.Url = SomeExternalServiceAddress;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            client.PreAuthenticate = true;


            var result = client.SomeExternalServiceMethod(param);

            return result;
        }
		
		
		private static string SerializeToString(object obj)
        {
            var messageType = obj.GetType();
            var serializer = new XmlSerializer(messageType);
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }
    }
}
