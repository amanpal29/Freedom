using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Xml.Serialization;
using Freedom.Domain.Model;

namespace Freedom.Domain.Services.Security
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class FreedomPrincipal : IPrincipal
    {
        public const string Administrators = "Administrators";

        public FreedomPrincipal()
        {
        }

        public FreedomPrincipal(IIdentity identity)
        {
            Identity = identity;
        }

        public FreedomPrincipal(IIdentity identity, FreedomPrincipal principal)
        {
            Identity = identity;
            UserId = principal.UserId;
            DisplayName = principal.DisplayName;
            IsAdministrator = principal.IsAdministrator;
            Permissions = principal.Permissions;
            SymmetricKey = principal.SymmetricKey;
        }

        [XmlIgnore]
        public IIdentity Identity { get; }

        public bool IsInRole(string role)
        {
            if (role == Administrators)
                return IsAdministrator;

            return Permissions?.Contains(role) ?? false;
        }

        public bool HasPermission(SystemPermission permission)
        {
            return Permissions?.Contains(permission) ?? false;
        }

        [DataMember(EmitDefaultValue = false)]
        public Guid UserId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DisplayName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsAdministrator { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public FreedomPermissionSet Permissions { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public byte[] SymmetricKey { get; set; }
    }

    public static class PrincipalExtensions
    {
        public static bool IsFreedomPrincipal(this IPrincipal principal)
        {
            return principal is FreedomPrincipal;
        }

        public static bool IsFreedomAdministrator(this IPrincipal principal)
        {
            return (principal as FreedomPrincipal)?.IsAdministrator ?? false;
        }

        public static bool HasPermission(this IPrincipal principal, SystemPermission permission)
        {
            return (principal as FreedomPrincipal)?.HasPermission(permission) ?? false;
        }

        public static bool HasAllPermissions(this IPrincipal principal, params SystemPermission[] permissions)
        {
            return HasAllPermissions(principal, (IEnumerable<SystemPermission>) permissions);
        }

        public static bool HasAllPermissions(this IPrincipal principal, IEnumerable<SystemPermission> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            FreedomPrincipal FreedomPrincipal = principal as FreedomPrincipal;

            if (FreedomPrincipal == null)
                return !permissions.Any();

            return permissions.All(p => FreedomPrincipal.HasPermission(p));
        }
    }
}