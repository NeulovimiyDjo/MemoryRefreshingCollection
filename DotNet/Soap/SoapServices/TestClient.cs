using System;
using System.Net.Http;
using System.Threading.Tasks;
using WCFServices;

namespace SoapServices
{
    public class TestClient : IClient
    {
        private readonly HttpClient client = new HttpClient() { BaseAddress = new Uri("http://localhost:5000") };

        public TestClient() { }


        public async Task<HttpResponseMessage> PostAsync(string requestUri, StringContent content)
        {
            var responseMessage = await client.PostStringContentAsync(requestUri, content);
            return responseMessage;
        }

        public async Task<string> PostAsJsonAndGetContentStringAsync<RequestType>(string requestUri, RequestType requestData)
        {
            var responseString = await client.PostAsJsonAndGetContentStringAsync(requestUri, requestData);
            return responseString;
        }

        public async Task<ResponseType> PostAsJsonAndDeserializeAsync<ResponseType, RequestType>(string requestUri, RequestType requestData)
        {
            ResponseType res = await client.PostAsJsonAndDeserializeAsync<ResponseType, RequestType>(requestUri, requestData);
            return res;
        }
    }
}
