using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChummerHub.Services
{
    using ChummerHub.Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;

    public sealed class ClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        public ClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
                : base(userManager, roleManager, optionsAccessor)
        {
        }

        /// <inheritdoc/>
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user).ConfigureAwait(false);

            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.Subject))
            {
                var sub = user.Id.ToString();
                var claim = new Claim(JwtClaimTypes.Subject, sub);
                identity.AddClaim(claim);
            }
            return identity;
        }
    }
}
