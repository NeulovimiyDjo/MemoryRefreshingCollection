using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using WCFHostLib.Validators;
using NLog;
using WCFServices;

namespace WCFHostLib
{
    public class AuthClient : IClient
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient client = new HttpClient() { BaseAddress = new Uri(WebConfigurationManager.AppSettings["MainServiceBaseAddress"]) };
        private readonly AuthTicketCreator _authTicketCreator = new AuthTicketCreator();

        public AuthClient(int seconds = 0)
        {
            if (seconds > 0)
                client.Timeout = TimeSpan.FromSeconds(seconds);
        }


        public async Task<HttpResponseMessage> PostAsync(string requestUri, StringContent content)
        {
            Dictionary<string, string> headers = GetAuthTicketHeaderToAdd();
            var responseMessage = await client.PostStringContentAsync(requestUri, content, headers);
            return responseMessage;
        }

        public async Task<string> PostAsJsonAndGetContentStringAsync<RequestType>(string requestUri, RequestType requestData)
        {
            Dictionary<string, string> headers = GetAuthTicketHeaderToAdd();
            var responseString = await client.PostAsJsonAndGetContentStringAsync(requestUri, requestData, headers);
            return responseString;
        }

        public async Task<ResponseType> PostAsJsonAndDeserializeAsync<ResponseType, RequestType>(string requestUri, RequestType requestData)
        {
            Dictionary<string, string> headers = GetAuthTicketHeaderToAdd();
            ResponseType res = await client.PostAsJsonAndDeserializeAsync<ResponseType, RequestType>(requestUri, requestData, headers);
            return res;
        }


        private Dictionary<string, string> GetAuthTicketHeaderToAdd()
        {
            var requestHeaders = WebOperationContext.Current.IncomingRequest.Headers;
            var headers = new Dictionary<string, string>();

            if (requestHeaders.AllKeys.ToList().Contains("CertInfo"))
                headers.Add("CertAuthTicket", _authTicketCreator.CreateAuthTicket(requestHeaders.Get("CertInfo")));

            return headers;
        }


        private class AuthTicketCreator
        {
            private readonly CertSignHelper _certSignHelper = new CertSignHelper("key");

            public string CreateAuthTicket(string callerInfo)
            {
                byte[] ticketBytes = Encoding.UTF8.GetBytes(callerInfo);
                byte[] signedData = _certSignHelper.Sign(ticketBytes);
                string ticket = Convert.ToBase64String(signedData);
                return ticket;
            }
        }
    }
}
