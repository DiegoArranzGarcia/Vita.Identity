using IdentityServer4;
using IdentityServer4.Models;
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

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "vita.spa",
                    ClientName = "SPA (Code + PKCE)",

                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = { "http://localhost:4200/login" },
                    PostLogoutRedirectUris = { },

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        ApiScopeConfiguration.GoalApiScope
                    },

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,                   
                },
                new Client
                {
                    ClientId= "identity-api-client",
                    ClientName = "Vita Backend Services",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = {
                        ApiScopeConfiguration.IdentityApiScope
                    },

                    ClientSecrets = { new Secret("identity-secret".Sha256()) },
                }
            };
        }
    }
}
