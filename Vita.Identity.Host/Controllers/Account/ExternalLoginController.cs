using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.ValueObjects;
using Vita.Identity.Host.Shared;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalLoginController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly ILogger<ExternalLoginController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IEventService _events;

        public ExternalLoginController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            ILogger<ExternalLoginController> logger,
            IUserRepository userRepository)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _logger = logger;
            _userRepository = userRepository;
            _events = events;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                }
            };

            return Challenge(props, scheme);

        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
                throw new Exception("External authentication error");

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            var (user, loginProvider, claims) = await FindUserFromExternalProvider(result);

            if (user is null)
                user = await AutoProvisionUser(loginProvider, claims);
            else
                await UpdateLoginProviderInfo(user, loginProvider);

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            var isuser = new IdentityServerUser(user.Id.ToString())
            {
                DisplayName = user.UserName,
                IdentityProvider = loginProvider.Name,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            await _events.RaiseAsync(new UserLoginSuccessEvent(loginProvider.Name, loginProvider.ExternalUserId, user.Id.ToString(), user.UserName, true, context?.Client.ClientId));

            return Redirect(returnUrl);
        }

        private Task UpdateLoginProviderInfo(User user, Vita.Identity.Domain.Aggregates.Users.LoginProvider loginProvider)
        {
            Vita.Identity.Domain.Aggregates.Users.LoginProvider storedLoginProvider = user.LoginProviders.Single(u => u.Name == loginProvider.Name);
            storedLoginProvider.UpdateAccessToken(loginProvider.Token);

            _userRepository.Update(user);
            return _userRepository.UnitOfWork.SaveEntitiesAsync();
        }

        private async Task<(User user, LoginProvider loginProvider, IEnumerable<Claim> claims)> FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            string provider = result.Properties.Items["scheme"];
            string providerUserId = userIdClaim.Value;
            string accessToken = result.Properties.GetTokenValue("access_token");

            LoginProvider loginProvider = new(provider, providerUserId, accessToken);

            // find external user
            var user = await _userRepository.FindByLoginProvider(loginProvider.Name, loginProvider.ExternalUserId);

            return (user, loginProvider, claims);
        }

        private async Task<User> AutoProvisionUser(LoginProvider loginProvider, IEnumerable<Claim> claims)
        {
            Claim emailClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            Email email = new(emailClaim.Value);

            User user = await _userRepository.FindByEmailAsync(email);

            if (user is null)
            {
                string givenName = claims.First(x => x.Type == ClaimTypes.GivenName).Value;
                string familyName = claims.First(x => x.Type == ClaimTypes.Surname).Value;

                user = Vita.Identity.Domain.Aggregates.Users.User.CreateWithLoginProvider(email, givenName, familyName, loginProvider);
                await _userRepository.Add(user);
            }
            else
            {
                LoginProvider currentProvider = user.LoginProviders.FirstOrDefault(x => x.Name == loginProvider.Name);

                if (currentProvider is null)
                {
                    user.AssociateExternalLoginProvider(loginProvider);
                    await _userRepository.Update(user);
                }
            }
            
            await _userRepository.UnitOfWork.SaveEntitiesAsync();            

            return user;
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        private static void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
    }
}