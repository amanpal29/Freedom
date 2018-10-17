using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Freedom.WebApi.Infrastructure;
using Freedom.WebApi.Results;
using Freedom.Domain.Services.Security;

namespace Freedom.WebApi.Filters
{
    // Note: There are two layers of authentication in Freedom
    //
    // The first layer handles authentication of the users credentials using whatever authentication scheme 
    // is configured and generates an IIdentity.  This can be done with host level authentication, or with the
    // Freedom's own Basic authentication via the HedgehogBasicAuthenticationAttribute Filter
    //
    // The second layer ensures that the user is in the Freedom database and generates a IPrinciple with
    // "roles" (i.e. SystemPermissions) that can then be users with an Authorize attribute.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class FreedomAuthenticationAttribute : FilterAttribute, IAuthenticationFilter
    {
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            IIdentity identity = context.Principal?.Identity;

            if (identity == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", context.Request);
                return;
            }

            if (identity.IsAuthenticated == false)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", context.Request);
                return;
            }

            IFreedomUserCache freedomUserCache = IoC.Get<IFreedomUserCache>();

            FreedomUser user = await freedomUserCache.GetFreedomUserAsync(identity.Name);

            // The user was successfully authenticated, but not found in the database.
            if (user == null)
            {
                context.ErrorResult = new ForbiddenResult(context.Request);
                return;
            }

            // Ok this user has been authenticated, and was found in the database.
            FreedomPrincipal principal = new FreedomPrincipal(identity);

            principal.UserId = user.Id;
            principal.DisplayName = !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : user.UserName;
            principal.IsAdministrator = user.IsAdministrator;
            principal.Permissions = user.Permissions;
            principal.SymmetricKey = user.GetSymmetricKeyBytes();

            context.Principal = principal;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}