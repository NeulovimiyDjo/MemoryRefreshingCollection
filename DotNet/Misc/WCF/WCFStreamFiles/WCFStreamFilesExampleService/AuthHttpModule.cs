using System;
using System.Text;
using System.Web;

namespace WCFStreamFilesExampleService
{
    public class AuthHttpModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += new EventHandler(this.OnAuthenticateRequest);
        }


        public void OnAuthenticateRequest(object source, EventArgs eventArgs)
        {
            HttpApplication app = (HttpApplication)source;

            string relPath = app.Request.AppRelativeCurrentExecutionFilePath.ToLower();
            switch (relPath)
            {
                case "~/someservice.svc":
                    {
                        try
                        {
                            this.RequireCertAuth(app);
                        }
                        catch (Exception e)
                        {
                            HandleAuthException(app, e);
                        }

                        break;
                    }
                default:
                    {
                        this.DenyAccess(app);

                        break;
                    }
            }
        }

        private static void HandleAuthException(HttpApplication app, Exception e)
        {
            app.Response.StatusCode = 500;
            app.Response.StatusDescription = "Internal service error";
            app.Response.Write($"Error while trying to authenticate.");
            app.CompleteRequest();
        }

        private void RequireCertAuth(HttpApplication app)
        {
            if (app.Request.ClientCertificate.Certificate == null || app.Request.ClientCertificate.Certificate.Length == 0 || !app.Request.ClientCertificate.IsValid)
            {
                this.DenyAccess(app);
            }
        }

        private void DenyAccess(HttpApplication app)
        {
            app.Response.StatusCode = 401;
            app.Response.StatusDescription = "Unauthorized";
            app.Response.Write("401 Unauthorized");
            app.CompleteRequest();
        }
    }
}