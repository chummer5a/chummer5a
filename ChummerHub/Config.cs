/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ChummerHub
{
    public class JwtTokenClass
    {
        public JwtTokenClass()
        {
            TokenTimeoutMinutes = 60 * 24 * 365;
            SigningKey = "MySecretChummer5aKey";
            if (Debugger.IsAttached)
            {
                Issuer = "https://localhost:64939";
            }
            else
            {
                Issuer = "https://chummer.azurewebsites.net/";
            }
            Audience = "";
        }
        public string Issuer { get; internal set; }
        public string Audience { get; internal set; }
        public string SigningKey { get; internal set; }
        public double TokenTimeoutMinutes { get; internal set; }
    }

    public class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                 new IdentityResource()
                {
                    Name = "verification",
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Email,
                        JwtClaimTypes.EmailVerified
                    }
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("ChummerHubV1", "ChummerHub API")
        };

        public static IEnumerable<Client> Clients =>
       new List<Client>
       {
           // JWT-based client authentication sample
                new Client
                {
                    ClientId = "Chummer5a",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    
                    // this client uses an RSA key as client secret
                    // and https://docs.duendesoftware.com/identityserver/v5/tokens/authentication/jwt/
                    ClientSecrets =
                    {
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                            Value = "{'e':'AQAB','kid':'ZzAjSnraU3bkWGnnAqLapYGpTyNfLbjbzgAPbbW2GEA','kty':'RSA','n':'wWwQFtSzeRjjerpEM5Rmqz_DsNaZ9S1Bw6UbZkDLowuuTCjBWUax0vBMMxdy6XjEEK4Oq9lKMvx9JzjmeJf1knoqSNrox3Ka0rnxXpNAz6sATvme8p9mTXyp0cX4lF4U2J54xa2_S9NF5QWvpXvBeC4GAJx7QaSw4zrUkrc6XyaAiFnLhQEwKJCwUw4NOqIuYvYp_IXhw-5Ti_icDlZS-282PcccnBeOcX7vc21pozibIdmZJKqXNsL1Ibx5Nkx1F1jLnekJAmdaACDjYRLL_6n3W4wUp19UvzB1lGtXcJKLLkqB6YDiZNu16OSiSprfmrRXvYmvD8m6Fnl5aetgKw'}"
                        }
                    },

                    AllowedScopes = { "ChummerHubV1" }
                },
            //new Client
            //{
            //    ClientId = "Chummer5a",

            //    // no interactive user, use the clientid/secret for authentication
            //    AllowedGrantTypes = GrantTypes.DeviceFlow,

            //    // secret for authentication
            //    ClientSecrets =
            //    {
            //        new Secret("secret".Sha256())
            //    },

            //    // where to redirect to after login
            //    RedirectUris = { "https://localhost:64939/signin-oidc" },

            //    // where to redirect to after logout
            //    PostLogoutRedirectUris = { "https://localhost:63939/signout-callback-oidc" },

            //    AllowOfflineAccess = true,

            //    // scopes that client has access to
            //    AllowedScopes =
            //    {
            //        IdentityServerConstants.StandardScopes.OpenId,
            //        IdentityServerConstants.StandardScopes.Profile,
            //        "ChummerHubV1"
            //    }
            //},
            new Client
            {
                ClientId = "interactive.public",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                

                // where to redirect after login
                RedirectUris = new List<string>()
                {
                    "https://127.0.0.1:64888",
                    "https://127.0.0.1:5013",
                    "https://127.0.0.1:62777",
                    "https://localhost:64888",
                    "https://localhost:5013",
                    "https://localhost:62777",
                    "https://127.0.0.1:64888/signin-oidc",
                    "https://127.0.0.1:5013/signin-oidc",
                    "https://127.0.0.1:62777/signin-oidc",
                    "https://localhost:64888/signin-oidc",
                    "https://localhost:5013/signin-oidc",
                    "https://localhost:62777/signin-oidc",
                    
                },

                // where to redirect after logout
                //PostLogoutRedirectUris = { "https://localhost:64939/signout-callback-oidc" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "api",
                    "verification"
                }
            }
       };

        public static string ApplicationName {
            get { return "ChummerHub"; }
            }

        private static JwtTokenClass _jwtToken = null;

        public static JwtTokenClass JwtToken
        {
            get
            {
                if (_jwtToken == null)
                {
                    _jwtToken = new JwtTokenClass();
                }
                return _jwtToken;
            }
        }

        internal static List<ApplicationUser> GetAdminUsers()
        {
            var a = new ApplicationUser()
            {
                Email = "archon.megalon@gmail.com",
                EmailConfirmed = true,
                LockoutEnabled = false,
                NormalizedEmail = "ARCHON.MEGALON@GMAIL.COM",
                NormalizedUserName = "archon",
                UserName = "Archon",
                Groupname = "MyPlayGroup1",
                Id = Guid.Parse("9FC744C1-FC22-4EDA-6A05-08D64B08AE81"),
            };
            var b = new ApplicationUser()
            {
                Email = "chummer5isalive@gmail.com",
                EmailConfirmed = true,
                LockoutEnabled = false,
                NormalizedEmail = "CHUMMER5ISALIVE@GMAIL.COM",
                NormalizedUserName = "chummer",
                UserName = "Chummer",
                Groupname = "MyPlayGroup2",
                Id = Guid.Parse("AFC744C1-FC22-4EDA-6A05-08D64B08AE81"),
            };
            var list = new List<ApplicationUser>() { a, b };
            return list;
        }
    }
}
