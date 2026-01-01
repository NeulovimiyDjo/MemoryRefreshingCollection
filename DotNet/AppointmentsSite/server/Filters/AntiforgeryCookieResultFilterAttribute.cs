using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace server.Filters
{
  public class AntiforgeryCookieResultFilterAttribute : ResultFilterAttribute
  {
    protected IAntiforgery _antiforgery;

    public AntiforgeryCookieResultFilterAttribute(IAntiforgery antiforgery) => _antiforgery = antiforgery;

    public override void OnResultExecuting(ResultExecutingContext context)
    {
      string path = context.HttpContext.Request.Path.Value;

      if (string.Equals(path, "/api/account/isLoggedIn", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(path, "/api/account/login", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(path, "/api/account/register", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(path, "/api/account/logout", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = _antiforgery.GetAndStoreTokens(context.HttpContext);
        context.HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
      }
    }
  }
}
