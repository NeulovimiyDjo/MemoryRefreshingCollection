using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;

namespace WebService_BasicAuth
{
    public class Global : System.Web.HttpApplication
    {

        public static readonly bool IgnoreCertificate = WebConfigurationManager.AppSettings["SomeKey"].ToLower() == "true";

        protected void Application_Start(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (request, certificate, chain, error) =>
                {
                    bool isLocal = ((System.Net.HttpWebRequest)request).Address.IsLoopback == true;
                    if (isLocal || IgnoreCertificate)
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