using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace Vita.Identity.Host.Policies;

public class IdentityApiRequirementHandler : AuthorizationHandler<IdentityApiRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdentityApiRequirement requirement)
    {
        throw new NotImplementedException();
    }
}
