using IdentityServer4.Models;
using System.Collections.Generic;

namespace Vita.Identity.Host.Config
{
    public class ApiScopeConfiguration
    {
        public const string GoalsApiScope = "goals";
        public const string CalendarsApiScope = "calendars";
        public const string IdentityApiScope = "identity";

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope(name: "goals", displayName: "Full Access to Goals API"),
                new ApiScope(name: "calendars", displayName: "Full Access to Calendars API"),
                new ApiScope(name: "identity", displayName: "Full Access to Identity API")
            };
        }

    }
}
