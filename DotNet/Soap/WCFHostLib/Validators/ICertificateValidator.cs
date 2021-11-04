using System.Security.Cryptography.X509Certificates;

namespace WCFHostLib.Validators
{
    public interface ICertificateValidator
    {
        bool Validate(X509Certificate2 cert, out string error);
    }
}
