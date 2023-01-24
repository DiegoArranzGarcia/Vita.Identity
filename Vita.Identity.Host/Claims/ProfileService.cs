using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Vita.Identity.Application.Query.Users;

namespace Vita.Identity.Host.Claims
{
    public class ProfileService : IProfileService
    {
        private readonly IUserQueryStore _usersQueryStore;

        public ProfileService(IUserQueryStore usersQueryStore)
        {
            _usersQueryStore = usersQueryStore;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null)
                throw new Exception("No subject Id claim present");

            if (!Guid.TryParse(sub, out Guid id))
                throw new Exception("Invalid subject Id claim");

            var user = await _usersQueryStore.GetUserById(id);
            if (user == null)
                return;

            var claims = new List<Claim>
            {
                new Claim(type: JwtClaimTypes.Email, value: user.Email),
                new Claim(type: JwtClaimTypes.GivenName, value: user.GivenName),
                new Claim(type: JwtClaimTypes.FamilyName, value: user.FamilyName),
            };

            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null) throw new Exception("No subject Id claim present");
            if (!Guid.TryParse(sub, out Guid id)) throw new Exception("Invalid subject Id claim");

            var user = await _usersQueryStore.GetUserById(id);

            //if (user == null)
            //{
            //    Logger?.LogWarning("No user found matching subject Id: {0}", sub);
            //}

            context.IsActive = user != null;
        }
    }
}
