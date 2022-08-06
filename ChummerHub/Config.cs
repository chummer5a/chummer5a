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
using ChummerHub.Services;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ChummerHub
{
    public class JwtTokenClass
    {
        public static ILogger _logger;
        private IConfiguration _configuration;

        public JwtTokenClass(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            TokenTimeoutMinutes = 60 * 24 * 365;
            KeyVault keyVault = new KeyVault(logger);
            
            SigningKey = keyVault.GetSecret("JwtSigningKey");  //"MySecretChummer5aKey";
            if (Debugger.IsAttached)
            {
                Issuer = "https://localhost:64939";
            }
            else
            {
                Issuer = "https://chummer.azurewebsites.net/";
                try
                {
                    var issuerstring = _configuration["JWTIssuer"];
                    //string issuerstring = Startup.AppSettings["JWTIssuer"];
                    _logger.LogInformation("JWTIssuer: " + issuerstring);
                    Issuer = issuerstring;
                }
                catch (Exception e)
                {
                    _logger?.LogError("Could not get JWTIssuer: " + e);
                }
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
                        JwtClaimTypes.EmailVerified,
                        JwtClaimTypes.IdentityProvider
                    }
                },
                new IdentityResource("roles", "Your role(s)", new List<string>() { API.Authorization.Constants.UserRolePublicAccess })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("ChummerHubV1", "ChummerHub API")
        };

        public static IEnumerable<Client> Clients
        {
            get
            {
                List<string> grantTypes = new List<string>();
                grantTypes.AddRange(GrantTypes.DeviceFlow);
                grantTypes.AddRange(GrantTypes.CodeAndClientCredentials);
                var result = new List<Client>()
                {
                      new Client
                      {
                            ClientId = "Chummer5a",
                            ClientSecrets = { new Secret("secret".Sha256()) },
                            RequireClientSecret = false,
                            RequirePkce = true,
                            IdentityTokenLifetime = 60*5*24*365,
                            AccessTokenLifetime = 60*5*24*365,
                            AbsoluteRefreshTokenLifetime = 60*5*24*365,
                            DeviceCodeLifetime = 60*5*24*365,
                            AllowRememberConsent = true,
                            AlwaysIncludeUserClaimsInIdToken = true,
                            AlwaysSendClientClaims = true,
                            AuthorizationCodeLifetime = 60*5*24*365,
                            RefreshTokenExpiration = TokenExpiration.Sliding,
                            AllowedGrantTypes = grantTypes,
                            AllowAccessTokensViaBrowser = true,
                            AllowOfflineAccess = true,

                            Claims = new List<ClientClaim>()
                            {
                                new ClientClaim("idp", "local"),
                                new ClientClaim("iss", "local")
                            },
                            AccessTokenType = AccessTokenType.Jwt,
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
                                "http://127.0.0.1:64888",
                                "http://127.0.0.1:5013",
                                "http://127.0.0.1:62777",
                                "http://localhost:64888",
                                "http://localhost:5013",
                                "http://localhost:62777",
                                "http://127.0.0.1:64888/signin-oidc",
                                "http://127.0.0.1:5013/signin-oidc",
                                "http://127.0.0.1:62777/signin-oidc",
                                "http://localhost:64888/signin-oidc",
                                "http://localhost:5013/signin-oidc",
                                "http://localhost:62777/signin-oidc",

                            },

                            // where to redirect after logout
                            //PostLogoutRedirectUris = { "http://localhost:64939/signout-callback-oidc" },
                            
                            AllowedScopes = new List<string>
                            {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                IdentityServerConstants.StandardScopes.OfflineAccess,
                                "api",
                                "verification",
                                "roles"
                            }
                            
                      }
                };
                return result;
            }
               
        }

        public static string ApplicationName {
            get { return "ChummerHub"; }
            }

        private static JwtTokenClass _jwtToken = null;

        //public JwtTokenClass JwtToken(ILogger logger, IConfiguration configuration)
        //{
            
        //        if (_jwtToken == null)
        //        {
        //            _jwtToken = new JwtTokenClass(logger, configuration);
        //        }
        //        return _jwtToken;
        //}

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
