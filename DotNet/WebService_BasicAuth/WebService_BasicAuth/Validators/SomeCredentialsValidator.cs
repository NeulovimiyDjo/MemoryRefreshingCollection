using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace WebService_BasicAuth.Validators
{
    public class SomeCredentialsValidator : ICredentialsValidator
    {
        private static readonly string RequiredUsername = "Login";
        private static readonly string RequiredPassword = "Password";

        public bool Validate(string username, string password)
        {
            if (username == RequiredUsername && password == RequiredPassword)
            {
                return true;
            }

            return false;
        }
    }
}