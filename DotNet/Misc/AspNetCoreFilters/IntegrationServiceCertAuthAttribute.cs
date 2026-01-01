using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace MyProject
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CertAuthAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
    {
        private readonly AuthTicketChecker _authTicketChecker = new AuthTicketChecker();

        public int Order { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            IHeaderDictionary headers = context.HttpContext.Request.Headers;
            if (headers.ContainsKey("CertAuthTicket")
                && _authTicketChecker.ValidateAuthTicket(headers["CertAuthTicket"]))
            {
                return;
            }

            ForbidAccess(context);
        }


        private static void ForbidAccess(AuthorizationFilterContext context)
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            _logger.Warn($"Access forbidden: failed to authenticate the caller to method {context.HttpContext.Request.Path} using cert auth.");
        }


        private class AuthTicketChecker
        {
            private readonly CertSignHelper _certSignHelper = new CertSignHelper("key");

            public bool ValidateAuthTicket(string ticket)
            {
                byte[] ticketBytes = Convert.FromBase64String(ticket);
                bool isValidTicket = _certSignHelper.Validate(ticketBytes);
                return isValidTicket;
            }
        }
    }
}
