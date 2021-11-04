using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WCFServices
{
    public class Test_TestService : WCFServiceBase, ITest_TestService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private IClient client => ClientFactory.GetInstance();
        public Test_TestService(IClientFactory clientFactory) : base(clientFactory) { }


        public string TestMethod1(string param)
        {
            return "Hello " + param;
        }


        public async Task<string> TestMethod2(string param)
        {
            try
            {
                logger.Info($"Request TestMethod2: {CommonFunctions.SerializeAsXmlString(param)}");

                var content = new StringContent(param, Encoding.UTF8, "text/plain");
                var response = await client.PostAsync("TestService/TestMethod2", content);
                response.EnsureSuccessStatusCode();

                var TestMethod2Response = await response.Content.ReadAsStringAsync();
                logger.Info($"Response TestMethod2: {CommonFunctions.SerializeAsXmlString(TestMethod2Response)}");
                return TestMethod2Response;
            }
            catch (Exception ex)
            {
                logger.Error($"Error TestMethod2: {ex.Message}\n {ex.StackTrace}");
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }


        public async Task<string> TestMethod3(Test_TestServiceRequest3 Test_TestServiceRequest3)
        {
            try
            {
                logger.Info($"Request TestMethod3: {CommonFunctions.SerializeAsXmlString(Test_TestServiceRequest3)}");

                var json = JsonConvert.SerializeObject(Test_TestServiceRequest3);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("TestService/TestMethod3", content);
                response.EnsureSuccessStatusCode();

                var TestMethod3Response = await response.Content.ReadAsStringAsync();
                logger.Info($"Response TestMethod3: {CommonFunctions.SerializeAsXmlString(TestMethod3Response)}");
                return TestMethod3Response;
            }
            catch (Exception ex)
            {
                logger.Error($"Error TestMethod3: {ex.Message}\n {ex.StackTrace}");
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }
    }
}
