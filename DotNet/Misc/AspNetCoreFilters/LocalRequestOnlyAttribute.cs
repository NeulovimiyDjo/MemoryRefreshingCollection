using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MyProject
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LocalRequestOnlyAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public int Order { get; set; }


        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!IPAddress.IsLoopback(context.HttpContext.Connection.RemoteIpAddress))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                _logger.Warn($"Access forbidden: request is not local to method {context.HttpContext.Request.Path}");
            }
        }
    }
}
