using System.Security.Principal;

namespace Freedom.Domain.Services.Security
{
    public class FreedomIdentity : IIdentity
    {
        public FreedomIdentity(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string AuthenticationType => nameof(FreedomIdentity);

        public bool IsAuthenticated => true;

        public bool ForcePasswordChange { get; set; }
    }
}