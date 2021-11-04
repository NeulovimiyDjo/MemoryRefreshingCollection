using System.Net.Http;
using System.Threading.Tasks;

namespace WCFServices
{
    public interface IClient
    {
        public Task<ResponseType> PostAsJsonAndDeserializeAsync<ResponseType, RequestType>(string requestUri, RequestType requestData);
        public Task<string> PostAsJsonAndGetContentStringAsync<RequestType>(string requestUri, RequestType requestData);
        public Task<HttpResponseMessage> PostAsync(string requestUri, StringContent content);
    }
}