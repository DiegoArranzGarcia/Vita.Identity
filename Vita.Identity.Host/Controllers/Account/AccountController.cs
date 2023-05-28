using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServerHost.Quickstart.UI;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vita.Identity.Application.Commands.Users;
using Vita.Identity.Application.Query.Users;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.ValueObjects;
using Vita.Identity.Host.Shared;

namespace Vita.Identity.Host.Controllers.Account;

[SecurityHeaders]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IEventService _events;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly Domain.Services.Authentication.IAuthenticationService _authenticationService;

    public AccountController(IIdentityServerInteractionService interaction,
                             IClientStore clientStore,
                             IAuthenticationSchemeProvider schemeProvider,
                             IEventService events,
                             IMediator mediator,
                             IUserRepository userRepository,
                             Domain.Services.Authentication.IAuthenticationService authenticationService,
                             IConfiguration configuration)
    {
        _interaction = interaction;
        _clientStore = clientStore;
        _schemeProvider = schemeProvider;
        _events = events;
        _mediator = mediator;
        _configuration = configuration;
        _userRepository = userRepository;
        _authenticationService = authenticationService;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
        //string accessToken = await HttpContext.GetTokenAsync("access_token");
        // build a model so we know what to show on the login page
        var vm = await BuildLoginViewModelAsync(returnUrl);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model, string button)
    {
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

        if (ModelState.IsValid)
        {
            Email email = new(model.Email);

            if (await _authenticationService.AuthenticateUser(email, model.Password))
            {
                User user = await _userRepository.FindByEmailAsync(email);

                await _events.RaiseAsync(new UserLoginSuccessEvent(username: user.UserName, subjectId: user.Id.ToString(), name: user.GivenName, clientId: context?.Client?.ClientId));

                AuthenticationProperties props = null;
                if (model.RememberLogin)
                {
                    props = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                    };
                }

                var isuser = new IdentityServerUser(user.Id.ToString())
                {
                    DisplayName = $"{user.GivenName} {user.FamilyName}".Trim(),
                };

                await HttpContext.SignInAsync(isuser, props);

                if (context != null)
                {
                    if (await _clientStore.IsPkceClientAsync(context?.Client?.ClientId))
                        return this.LoadingPage("Redirect", model.ReturnUrl);

                    return Redirect(model.ReturnUrl);
                }

                if (Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return Redirect("~/");

                throw new Exception("invalid return URL");
            }

            await _events.RaiseAsync(new UserLoginFailureEvent(model.Email, "invalid credentials", clientId: context?.Client?.ClientId));
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
        }

        var vm = await BuildLoginViewModelAsync(model);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SignUp([FromBody] CreateUserCommand createUserCommand)
    {
        if (_configuration["AllowedUserCreation"] == null || !bool.Parse(_configuration["AllowedUserCreation"]))
            return Forbid();

        var user = await _mediator.Send(createUserCommand);
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        var vm = await BuildLogoutViewModelAsync(logoutId);

        if (!vm.ShowLogoutPrompt)
            return await Logout(vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutInputModel model)
    {
        var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

        if (User.Identity.IsAuthenticated)
        {
            await HttpContext.SignOutAsync();
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
        }

        if (vm.TriggerExternalSignout)
        {
            string url = Url.Action("Logout", new { logoutId = vm.LogoutId });
            return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
        }

        return View("LoggedOut", vm);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

            var vm = new LoginViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                Email = context?.LoginHint,
            };

            return vm;
        }

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ExternalProvider
            {
                DisplayName = x.DisplayName ?? x.Name,
                AuthenticationScheme = x.Name
            }).ToList();

        var allowLocal = true;
        if (context?.Client?.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context?.Client?.ClientId);
            if (client != null)
                allowLocal = client.EnableLocalLogin;
        }

        return new LoginViewModel
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal,
            ReturnUrl = returnUrl,
            Email = context?.LoginHint,
            ExternalProviders = providers.ToArray(),
        };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
    {
        var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
        vm.Email = model.Email;
        vm.RememberLogin = model.RememberLogin;
        return vm;
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
    {
        var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

        if (User?.Identity.IsAuthenticated != true)
        {
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        var context = await _interaction.GetLogoutContextAsync(logoutId);
        if (context?.ShowSignoutPrompt == false)
        {
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        return vm;
    }

    private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
    {
        var logout = await _interaction.GetLogoutContextAsync(logoutId);

        var vm = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            SignOutIframeUrl = logout?.SignOutIFrameUrl,
            LogoutId = logoutId
        };

        if (User?.Identity.IsAuthenticated == true)
        {
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                if (providerSupportsSignout)
                {
                    if (vm.LogoutId == null)
                        vm.LogoutId = await _interaction.CreateLogoutContextAsync();

                    vm.ExternalAuthenticationScheme = idp;
                }
            }
        }

        return vm;
    }
}
