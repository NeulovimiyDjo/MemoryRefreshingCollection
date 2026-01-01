namespace WebService_BasicAuth.Validators
{
    public interface ICredentialsValidator
    {
        bool Validate(string username, string password);
    }
}