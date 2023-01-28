using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Vita.Identity.Host.Config
{
    public static class IdentityServerConfiguration
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
            {
                new Client
                {
                    Enabled = true,
                    ClientId = configuration["Clients:Spa:ClientId"],
                    ClientName = "SPA (Code + PKCE)",

                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = { configuration["Clients:Spa:RedirectUri"] },
                    PostLogoutRedirectUris = { },

                    AllowOfflineAccess = true,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        ApiScopeConfiguration.GoalsApiScope
                    }      
                },
                new Client
                {
                    ClientId= configuration["Clients:Server2Server:ClientId"],
                    ClientName = "Vita Backend Services",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = {
                        ApiScopeConfiguration.IdentityApiScope,
                        ApiScopeConfiguration.GoalsApiScope,
                        ApiScopeConfiguration.CalendarsApiScope
                    },

                    ClientSecrets = { new Secret(configuration["Clients:Server2Server:ClientSecret"].Sha256()) },
                }
            };
        }
    }
}
