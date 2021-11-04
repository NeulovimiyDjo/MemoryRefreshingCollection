using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WCFServices
{
    public static class HttpClientExtensions
    {
        public async static Task<HttpResponseMessage> PostStringContentAsync(
            this HttpClient client, string requestUri, StringContent content, Dictionary<string, string> headers = null)
        {
            if (headers != null)
                foreach (var header in headers)
                    content.Headers.Add(header.Key, header.Value);

            var response = await client.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public async static Task<HttpResponseMessage> PostAsJsonAsync<RequestType>(
            this HttpClient client, string requestUri, RequestType requestData, Dictionary<string, string> headers = null)
        {
            var requestJson = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            if (headers != null)
                foreach (var header in headers)
                    content.Headers.Add(header.Key, header.Value);

            var response = await client.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public async static Task<string> PostAsJsonAndGetContentStringAsync<RequestType>(
            this HttpClient client, string requestUri, RequestType requestData, Dictionary<string, string> headers = null)
        {
            var response = await client.PostAsJsonAsync(requestUri, requestData, headers);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public async static Task<ResponseType> PostAsJsonAndDeserializeAsync<ResponseType, RequestType>(
            this HttpClient client, string requestUri, RequestType requestData, Dictionary<string, string> headers = null)
        {
            var responseJson = await client.PostAsJsonAndGetContentStringAsync(requestUri, requestData, headers);
            ResponseType res = JsonConvert.DeserializeObject<ResponseType>(responseJson);
            return res;
        }
    }
}
