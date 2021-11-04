using WebService_BasicAuth.Validators;
using NLog;
using System;
using System.Text;
using System.Web;

namespace WebService_BasicAuth
{
    public class BasicAuthHttpModule : IHttpModule
    {
        private static readonly Logger logger = CommonStaticVariables.logger;

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
                case "~/SomeName.asmx":
                    {
                        try // Require authentication for SomeNameService.
                        {
                            this.RequireBasicAuth(app, new SomeCredentialsValidator());
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

            logger.Error($"Error while trying to authenticate: {e.Message}, {e.StackTrace}");
        }

        private void RequireBasicAuth(HttpApplication app, ICredentialsValidator credentialsValidator)
        {
            string authHeader = app.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authHeader))
            {
                (string username, string password) = this.GetCredentialsFromBasicAuthHeader(authHeader);

                if (credentialsValidator.Validate(username, password))
                {
                    return;
                }
            }

            this.DenyAccess(app);
        }

        private void DenyAccess(HttpApplication app)
        {
            app.Response.StatusCode = 401;
            app.Response.StatusDescription = "Access Denied";
            app.Response.Write("401 Access Denied");
            app.Response.AddHeader("WWW-Authenticate", "Basic");
            app.CompleteRequest();

            logger.Info($"Access Denied: failed to authenticate using 'basic authentication'.");
        }

        private (string, string) GetCredentialsFromBasicAuthHeader(string authHeader)
        {
            authHeader = authHeader.Trim();

            if (authHeader.IndexOf("Basic", 0) != 0)
            {
                throw new Exception("Invalid basic authentication header: header doesn't start with 'Basic'.");
            }

            string encodedCredentials = authHeader.Substring(6);

            byte[] decodedBytes = Convert.FromBase64String(encodedCredentials);
            string s = Encoding.UTF8.GetString(decodedBytes);

            string[] credentials = s.Split(new char[] { ':' });
            if (credentials.Length != 2)
            {
                throw new Exception("Invalid basic authentication header: failed to get username:password pair.");
            }

            string username = credentials[0];
            string password = credentials[1];

            return (username, password);
        }
    }
}