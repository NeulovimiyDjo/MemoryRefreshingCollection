using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NLog;

namespace BaseRateLimitedControllerTest
{
    [RequestSizeLimit(100_000_000)]
    public abstract class BaseRateLimitedController : ControllerBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly RateLimitHelper _rateLimitHelper = new();

        protected async Task<IActionResult> ExecuteProcessRequestFunc<TRequest, TResponse>(
            Func<TRequest, Task<TResponse>> processRequestFunc)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            if (await _rateLimitHelper.RequestsLimitExceeded())
            {
                _logger.Error($"Requests limit exceeded, terminating request {typeof(TRequest)}");
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            using Activity activity = MyTracer.CreateNewHttpInActivity(GetUrlPath());
            try
            {
                (TRequest request, Exception ex) = await ParseRequest<TRequest>(Request.Body);
                if (ex != null)
                    throw ex;

                activity?.EnrichDataWithRequest(request);
                TResponse response = await processRequestFunc(request);
                activity?.EnrichDataWithResponse(response);

                activity?.SetSuccess(true);
                return Content(response, MediaTypeNames.Text.Xml);
            }
            catch (Exception ex)
            {
                activity?.AddError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private string GetUrlPath()
        {
            return "/" + (HttpContext.GetEndpoint() as RouteEndpoint).RoutePattern.RawText;
        }
    }

    public class RateLimitHelper
    {
        private static readonly long WindowSizeSeconds = 1;
        private static readonly long MaxRequestsPerWindow = 10;
        private static readonly int CleanupOldDbRecordsMinIntervalMilliseconds = 5000;

        private static DateTime _lastOldDbRecordsCleanupTime = DateTime.MinValue;
        // This only prevents concurrent access within one server, but will still help on spikes.
        private static readonly SemaphoreSlim _dbOpSemaphore = new(1, 1);
        // This is only to prevent accessing DB when requests limit is greatly exceeded.
        private static readonly ConcurrentQueue<DateTime> _perServerRecentRequests = new();

        public async Task<bool> RequestsLimitExceeded()
        {
            DateTime now = DateTime.UtcNow;

            if (GetRecentRequestsCountPerServer() + 1 > MaxRequestsPerWindow)
                return true;
            AddRecentRequestPerServer();

            await _dbOpSemaphore.WaitAsync();
            try
            {
                if (await GetRecentRequestsCountFromDb() + 1 > MaxRequestsPerWindow)
                    return true;
                await AddRecentRequestToDb();
            }
            finally
            {
                _dbOpSemaphore.Release();
            }

            return false;

            int GetRecentRequestsCountPerServer()
            {
                return _perServerRecentRequests
                    .Where(x => x > now.AddSeconds(-WindowSizeSeconds))
                    .Count();
            }

            void AddRecentRequestPerServer()
            {
                _perServerRecentRequests.Enqueue(now);
                while (_perServerRecentRequests.TryPeek(out DateTime time) && time < now.AddSeconds(-WindowSizeSeconds))
                    _perServerRecentRequests.TryDequeue(out var _);
            }

            async Task<int> GetRecentRequestsCountFromDb()
            {
                int count = await GetCountFromSharedStorage();
                return count;
            }

            async Task AddRecentRequestToDb(DbManager db)
            {
                await AddRecordToSharedStorage();

                if (now.AddSeconds(-CleanupOldDbRecordsMinIntervalMilliseconds) > _lastOldDbRecordsCleanupTime)
                {
                    _lastOldDbRecordsCleanupTime = now;
                    await AddOldRecordsFromSharedStorage();
                }
            }
        }
}
