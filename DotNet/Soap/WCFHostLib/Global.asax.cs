using System;
using System.Net.Security;

namespace WCFHostLib
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (request, certificate, chain, error) =>
                {
                    bool isLocal = ((System.Net.HttpWebRequest)request).Address.IsLoopback == true;
                    if (isLocal)
                    {
                        return true;
                    }

                    return error == SslPolicyErrors.None;
                };
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}