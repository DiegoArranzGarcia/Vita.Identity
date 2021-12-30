using System;

namespace Vita.Identity.Host.Controllers.Account
{
    public static class AccountOptions
    {
        public const bool AllowLocalLogin = true;
        public const bool AllowRememberLogin = true;
        public readonly static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public const bool ShowLogoutPrompt = true;
        public const bool AutomaticRedirectAfterSignOut = false;

        public const string WindowsAuthenticationSchemeName = Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme;
        public const bool IncludeWindowsGroups = false;

        public const string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}
