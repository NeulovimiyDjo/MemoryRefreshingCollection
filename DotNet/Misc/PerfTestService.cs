using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace PerfTest
{
    public class PerfTestService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public PerfTestService(
            IConfiguration configuration,
            IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _client = clientFactory.CreateClient("HttpClientWithSSLUntrusted");
        }

        public async Task<string> Test(PerfTestData perfTestData)
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(1)))
                throw new Exception("Test is already executing in other thread");

            try
            {
                return await TestImpl(perfTestData);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> TestImpl(PerfTestData perfTestData)
        {
            int totalRequests = int.Parse(perfTestData.TotalRequests);
            int maxThreads = int.Parse(perfTestData.MaxThreads);
            string testType = perfTestData.TestType;

            Func<Task> testRequestAction = testType switch
            {
                "get" => async () => await TestGetRequest(perfTestData.Path, perfTestData.Token),
                _ => throw new Exception($"Invalid test type '{testType}'"),
            };

            string res = await TestRequestsLoad(
                totalRequests,
                maxThreads,
                testRequestAction);
            return res;
        }

        private async Task<string> TestRequestsLoad(
            int totalRequests,
            int maxThreads,
            Func<Task> testRequestAction)
        {
            int requestNum = 0;
            List<Task> testRequestTasks = new();
            List<Exception> exceptions = new();
            int successfulRequests = 0;
            ConcurrentQueue<string> errors = new();

            Stopwatch sw = new();
            sw.Start();
            for (int i = 0; i < totalRequests; i++)
            {
                if (sw.ElapsedMilliseconds > 1000 * 30)
                    throw new Exception("Timeout of 30 seconds exceeded");

                if (testRequestTasks.Any(t => t.Exception is not null))
                    break;

                testRequestTasks.Add(ExecuteTestRequest(requestNum++));
                if (testRequestTasks.Count == maxThreads)
                {
                    await Task.WhenAny(testRequestTasks.ToArray());
                    testRequestTasks.RemoveAll(t => t.IsCompleted && t.Exception is null);
                }
            }

            await Task.WhenAll(testRequestTasks.ToArray()).ContinueWith(t =>
            {
                if (t.Exception is not null)
                    exceptions.AddRange(t.Exception.InnerExceptions);
            });

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions.ToArray());

            long rps = 1000 * totalRequests / sw.ElapsedMilliseconds;
            return $"rps:{rps}\n" +
                $"timeElapsedSeconds:{sw.ElapsedMilliseconds / 1000}.{sw.ElapsedMilliseconds % 1000}\n" +
                $"totalRequests:{totalRequests}\n" +
                $"successfulRequests:{successfulRequests}\n" +
                $"errors:\n" +
                string.Join("\n", errors);

            async Task ExecuteTestRequest(int requestNum)
            {
                if (requestNum <= 20)
                    await Task.Delay(50 * requestNum);

                try
                {
                    await testRequestAction();
                    Interlocked.Increment(ref successfulRequests);
                }
                catch (Exception ex)
                {
                    errors.Enqueue(ex.Message);
                }
            }
        }

        public async Task TestGetRequest(string path, string token)
        {
            string baseAddress = _configuration.GetSection("AppSettings").GetValue<string>("ServiceAddress");
            var url = string.Format("{0}/{1}", baseAddress, path);

            using HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Add("Cookie", $"token={token}");

            using HttpResponseMessage response = await _client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"{response.StatusCode}: {responseContent}");
        }
    }
}
