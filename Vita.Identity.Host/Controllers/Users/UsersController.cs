using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vita.Identity.Application.Query.Users;

namespace Vita.Identity.Host.Controllers.Users
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Startup.IdentityApiScopePolicy)]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserQueryStore _userQueryStore;

        public UsersController(IUserQueryStore userQueryStore)
        {
            _userQueryStore = userQueryStore;
        }

        [HttpGet]
        [Route("{id}/login-providers")]

        public async Task<IActionResult> GetUserExternalLoginProviders(Guid id)
        {
            IEnumerable<ExternalLoginProviderDto> externalLoginProviders = await _userQueryStore.GetUserExternalLoginProviders(id);

            return Ok(externalLoginProviders);
        }

        [HttpGet]
        [Route("{id}/login-providers/{loginProviderId}/access-token")]
        public async Task<IActionResult> GetLoginProviderUserAccessToken(Guid id, Guid loginProviderId)
        {
            AccessTokenDto accessToken = await _userQueryStore.GetUserAccessToken(id, loginProviderId);

            return Ok(accessToken);
        }
    }
}