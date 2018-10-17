namespace Freedom.Domain.Services.Security
{
    public class FreedomCredentials : FreedomIdentity
    {
        public FreedomCredentials(string name, string password)
            : base(name)
        {
            Password = password;
        }

        public string Password { get; }
    }
}
