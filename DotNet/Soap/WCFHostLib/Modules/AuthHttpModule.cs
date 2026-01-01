using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Configuration;
using WCFHostLib.Modules;
using WCFHostLib.Validators;
using NLog;

namespace WCFHostLib.Modules
{
    public class AuthHttpModule : IHttpModule
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ICertificateValidator certificateValidator = new CertificateValidator();


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
                case "~/test_testservice.svc":
                    {
                        try
                        {
                            this.RequireCertAuth(app);
                        }
                        catch (Exception e)
                        {
                            this.HandleAuthException(app, e);
                        }

                        break;
                    }
                default:
                    {
                        this.ForbidAccess(app, "Invalid endpoint.");
                        break;
                    }
            }
        }
		

        private void RequireCertAuth(HttpApplication app)
        {
            if (app.Request.ClientCertificate.Certificate == null || app.Request.ClientCertificate.Certificate.Length == 0)
            {
                this.ForbidAccess(app, "Client certificate is absent.");
                return;
            }

            X509Certificate2 cert = new X509Certificate2(app.Request.ClientCertificate.Certificate);
            if (!this.certificateValidator.Validate(cert, out string error) || !app.Request.ClientCertificate.IsValid)
            {
                this.ForbidAccess(app, $"Client certificate is not valid:\n{error}");
                return;
            }

            app.Request.Headers.Add("CertInfo", cert.Subject);
        }

        private void ForbidAccess(HttpApplication app, string msg)
        {
            if (app.Response.StatusCode == 403)
                return;

            app.Response.StatusCode = 403;
            app.Response.StatusDescription = "Forbidden";
            app.Response.Write("403 Forbidden");
            app.CompleteRequest();

            logger.Warn($"Access Denied to {app.Request.AppRelativeCurrentExecutionFilePath}: {msg}");
        }

        private void HandleAuthException(HttpApplication app, Exception ex)
        {
            app.Response.StatusCode = 500;
            app.Response.StatusDescription = "Internal service error";
            app.Response.Write($"Error while trying to authenticate.");
            app.CompleteRequest();

            logger.Error($"Error while trying to authenticate: {ex.Message}\n {ex.StackTrace}");
        }
    }
}