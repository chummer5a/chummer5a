// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.Infrastructure;
using IdentityModel.OidcClient.Results;
using NLog;

namespace IdentityModel.OidcClient
{
    /// <summary>
    /// OpenID Connect client
    /// </summary>
    public class OidcClient
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        private readonly AuthorizeClient _authorizeClient;

        private readonly bool _useDiscovery;
        private readonly ResponseProcessor _processor;

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public OidcClientOptions Options { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OidcClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public OidcClient(OidcClientOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ProviderInformation == null)
            {
                if (options.Authority.IsMissing()) throw new ArgumentException("No authority specified", nameof(Options.Authority));
                _useDiscovery = true;
            }

            Options = options;
            //_logger = options.LoggerFactory.CreateLogger<OidcClient>();
            _authorizeClient = new AuthorizeClient(options);
            _processor = new ResponseProcessor(options, EnsureProviderInformationAsync);
        }

        /// <summary>
        /// Starts a login.
        /// </summary>
        /// <param name="request">The login request.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the request</param>
        /// <returns></returns>
        public virtual async Task<LoginResult> LoginAsync(LoginRequest request = null, CancellationToken cancellationToken = default)
        {
            //_logger.LogTrace("LoginAsync");
            //_logger.LogInformation("Starting authentication request.");

            if (request == null) request = new LoginRequest();

            await EnsureConfigurationAsync(cancellationToken);

            AuthorizeResult authorizeResult = await _authorizeClient.AuthorizeAsync(new AuthorizeRequest
            {
                DisplayMode = request.BrowserDisplayMode,
                Timeout = request.BrowserTimeout,
                ExtraParameters = request.FrontChannelExtraParameters
            }, cancellationToken);

            if (authorizeResult.IsError)
            {
                return new LoginResult(authorizeResult.Error, authorizeResult.ErrorDescription);
            }

            LoginResult result = await ProcessResponseAsync(
                authorizeResult.Data,
                authorizeResult.State,
                request.BackChannelExtraParameters,
                cancellationToken);

            if (!result.IsError)
            {
               
                Settings.Default.IdentityToken = result.IdentityToken;
                Settings.Default.AccessToken = result.AccessToken;
                //BearerToken
                Settings.Default.Save();
            }

            return result;
        }

        public static void SetCookieContainer(string cookiestring = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cookiestring))
                {
                    Settings.Default.CookieData = null;
                    Settings.Default.Save();
                    CookieCollection cookies = StaticUtils.AuthorizationCookieContainer?.GetCookies(new Uri(Settings.Default.SINnerUrl));
                }
                else
                {
                    //CookieCollection collection = new CookieCollection();
                    Settings.Default.CookieData = cookiestring;
                    Settings.Default.Save();
                    //collection.Add(cookie);
                    StaticUtils.AuthorizationCookieContainer?.SetCookies(new Uri(Settings.Default.SINnerUrl), cookiestring);
                    //StaticUtils.AuthorizationCookieContainer?.GetCookies(new Uri(Settings.Default.SINnerUrl));
                    //Cookie cookie = new Cookie("Cookie", cookiestring, "", Settings.Default.SINnerUrl);
                    //StaticUtils.AuthorizationCookieContainer.Add(cookie);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }



        /// <summary>
        /// Prepares the login request.
        /// </summary>
        /// <param name="frontChannelParameters">extra parameters to send to the authorize endpoint.</param>
        /// /// <param name="cancellationToken">A token that can be used to cancel the request</param>
        /// <returns>State for initiating the authorize request and processing the response</returns>
        public virtual async Task<AuthorizeState> PrepareLoginAsync(Parameters frontChannelParameters = null, CancellationToken cancellationToken = default)
        {
            //_logger.LogTrace("PrepareLoginAsync");

            await EnsureConfigurationAsync(cancellationToken);
            return _authorizeClient.CreateAuthorizeState(frontChannelParameters);
        }

        /// <summary>
        /// Creates a logout URL.
        /// </summary>
        /// <param name="request">The logout request.</param>
        /// /// <param name="cancellationToken">A token that can be used to cancel the request</param>
        /// <returns></returns>
        public virtual async Task<string> PrepareLogoutAsync(LogoutRequest request = default, CancellationToken cancellationToken = default)
        {
            await EnsureConfigurationAsync(cancellationToken);

            string endpoint = Options.ProviderInformation.EndSessionEndpoint;
            if (endpoint.IsMissing())
            {
                throw new InvalidOperationException("Discovery document has no end session endpoint");
            }

            return _authorizeClient.CreateEndSessionUrl(endpoint, request);
        }

        /// <summary>
        /// Starts a logout.
        /// </summary>
        /// <param name="request">The logout request.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the request</param>
        /// <returns></returns>
        public virtual async Task<LogoutResult> LogoutAsync(LogoutRequest request = null, CancellationToken cancellationToken = default)
        {
            if (request == null) request = new LogoutRequest();

            await EnsureConfigurationAsync(cancellationToken);

            BrowserResult result = await _authorizeClient.EndSessionAsync(request, cancellationToken);

            if (result.ResultType != BrowserResultType.Success)
            {
                return new LogoutResult(result.ResultType.ToString())
                {
                    Response = result.Response
                };
            }

            return new LogoutResult
            {
                Response = result.Response
            };
        }

        /// <summary>
        /// Processes the authorize response.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="state">The state.</param>
        /// <param name="backChannelParameters">Parameters for back-channel call</param>
        /// <param name="cancellationToken">A token that can be used to cancel the request</param>
        /// <returns>
        /// Result of the login response validation
        /// </returns>
        public virtual async Task<LoginResult> ProcessResponseAsync(
            string data, 
            AuthorizeState state, 
            Parameters backChannelParameters = null, 
            CancellationToken cancellationToken = default)
        {
            //_logger.LogTrace("ProcessResponseAsync");
            //_logger.LogInformation("Processing response.");

            backChannelParameters = backChannelParameters ?? new Parameters();
            await EnsureConfigurationAsync(cancellationToken);

            //_logger.LogDebug("Authorize response: {response}", data);
            AuthorizeResponse authorizeResponse = new AuthorizeResponse(data);

            if (authorizeResponse.IsError)
            {
                //_logger.LogError(authorizeResponse.Error);
                return new LoginResult(authorizeResponse.Error, authorizeResponse.ErrorDescription);
            }

            ResponseValidationResult result = await _processor.ProcessResponseAsync(authorizeResponse, state, backChannelParameters, cancellationToken);
            if (result.IsError)
            {
                //_logger.LogError(result.Error);
                return new LoginResult(result.Error, result.ErrorDescription);
            }

            IEnumerable<Claim> userInfoClaims = Enumerable.Empty<Claim>();
            if (Options.LoadProfile)
            {
                UserInfoResult userInfoResult = await GetUserInfoAsync(result.TokenResponse.AccessToken, cancellationToken);
                if (userInfoResult.IsError)
                {
                    string error = $"Error contacting userinfo endpoint: {userInfoResult.Error}";
                    //_logger.LogError(error);

                    return new LoginResult(error);
                }

                userInfoClaims = userInfoResult.Claims;

                Claim userInfoSub = userInfoResult.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
                if (userInfoSub == null)
                {
                    string error = "sub claim is missing from userinfo endpoint";
                    //_logger.LogError(error);

                    return new LoginResult(error);
                }

                if (result.TokenResponse.IdentityToken != null && !string.Equals(userInfoSub.Value, result.User.FindFirst(JwtClaimTypes.Subject).Value))
                {
                    string error = "sub claim from userinfo endpoint is different than sub claim from identity token.";
                    //_logger.LogError(error);

                    return new LoginResult(error);
                }
            }

            string authTimeValue = result.User.FindFirst(JwtClaimTypes.AuthenticationTime)?.Value;
            DateTimeOffset? authTime = null;

            if (authTimeValue.IsPresent() && long.TryParse(authTimeValue, out long seconds))
            {
                authTime = DateTimeOffset.FromUnixTimeSeconds(seconds);
            }

            ClaimsPrincipal user = ProcessClaims(result.User, userInfoClaims);

            LoginResult loginResult = new LoginResult
            {
                User = user,
                AccessToken = result.TokenResponse.AccessToken,
                RefreshToken = result.TokenResponse.RefreshToken,
           
                AccessTokenExpiration = DateTimeOffset.Now.AddSeconds(result.TokenResponse.ExpiresIn),
                IdentityToken = result.TokenResponse.IdentityToken,
                AuthenticationTime = authTime,
                TokenResponse = result.TokenResponse // In some cases there is additional custom response data that clients need access to
            };

            if (loginResult.RefreshToken.IsPresent())
            {
                loginResult.RefreshTokenHandler = new RefreshTokenDelegatingHandler(
                    this,
                    loginResult.AccessToken,
                    loginResult.RefreshToken,
                    Options.RefreshTokenInnerHttpHandler);
            }

            return loginResult;
        }

        /// <summary>
        /// Gets the user claims from the userinfo endpoint.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// User claims
        /// </returns>
        /// <exception cref="ArgumentNullException">accessToken</exception>
        /// <exception cref="InvalidOperationException">No userinfo endpoint specified</exception>
        public virtual async Task<UserInfoResult> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            //_logger.LogTrace("GetUserInfoAsync");

            await EnsureConfigurationAsync(cancellationToken);
            if (accessToken.IsMissing()) throw new ArgumentNullException(nameof(accessToken));
            if (!Options.ProviderInformation.SupportsUserInfo) throw new InvalidOperationException("No userinfo endpoint specified");

            HttpClient userInfoClient = Options.CreateClient();

            UserInfoResponse userInfoResponse = await userInfoClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = Options.ProviderInformation.UserInfoEndpoint,
                Token = accessToken
            }, cancellationToken).ConfigureAwait(false);

            if (userInfoResponse.IsError)
            {
                return new UserInfoResult
                {
                    Error = userInfoResponse.Error
                };
            }

            return new UserInfoResult
            {
                Claims = userInfoResponse.Claims
            };
        }

        /// <summary>
        /// Refreshes an access token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="backChannelParameters">Back-channel parameters</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A token response.
        /// </returns>
        public virtual async Task<RefreshTokenResult> RefreshTokenAsync(
            string refreshToken, 
            Parameters backChannelParameters = null, 
            CancellationToken cancellationToken = default)
        {
            //_logger.LogTrace("RefreshTokenAsync");

            await EnsureConfigurationAsync(cancellationToken);
            backChannelParameters = backChannelParameters ?? new Parameters();
            
            HttpClient client = Options.CreateClient();

            TokenResponse response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = Options.ProviderInformation.TokenEndpoint,
                ClientId = Options.ClientId,
                ClientSecret = Options.ClientSecret,
                ClientAssertion = Options.ClientAssertion,
                ClientCredentialStyle = Options.TokenClientCredentialStyle,
                RefreshToken = refreshToken,
                Parameters = backChannelParameters
            }, cancellationToken).ConfigureAwait(false);

            if (response.IsError)
            {
                return new RefreshTokenResult
                {
                    Error = response.Error,
                    ErrorDescription = response.ErrorDescription,
                };
            }

            // validate token response
            TokenResponseValidationResult validationResult = await _processor.ValidateTokenResponseAsync(response, null, requireIdentityToken: Options.Policy.RequireIdentityTokenOnRefreshTokenResponse,
                cancellationToken: cancellationToken);
            if (validationResult.IsError)
            {
                return new RefreshTokenResult { Error = validationResult.Error };
            }

            return new RefreshTokenResult
            {
                IdentityToken = response.IdentityToken,
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                ExpiresIn = response.ExpiresIn,
                AccessTokenExpiration = DateTimeOffset.Now.AddSeconds(response.ExpiresIn)
            };
        }

        internal async Task EnsureConfigurationAsync(CancellationToken cancellationToken)
        {
            await EnsureProviderInformationAsync(cancellationToken);

            Log.Trace("Effective options: " + LogSerializer.Serialize(Options));
        }

        internal async Task EnsureProviderInformationAsync(CancellationToken cancellationToken)
        {
            //_logger.LogTrace("EnsureProviderInformation");

            if (_useDiscovery)
            {
                if (Options.RefreshDiscoveryDocumentForLogin == false && Options.ProviderInformation != null)
                {
                    // discovery document has been loaded before - skip reload
                    Log.Debug("Skipping refresh of discovery document.");

                    return;
                }

                HttpClient discoveryClient = Options.CreateClient();
                DiscoveryDocumentResponse disco = await discoveryClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = Options.Authority,
                    Policy = Options.Policy.Discovery
                }, cancellationToken).ConfigureAwait(false);

                if (disco.IsError)
                {
                    Log.Error("Error loading discovery document: {errorType} - {error}", disco.ErrorType.ToString(), disco.Error);

                    if (disco.ErrorType == ResponseErrorType.Exception)
                    {
                        throw new InvalidOperationException("Error loading discovery document: " + disco.Error, disco.Exception);
                    }

                    throw new InvalidOperationException("Error loading discovery document: " + disco.Error);
                }

                Log.Debug("Successfully loaded discovery document");
                Log.Debug("Loaded keyset from {jwks_uri}", disco.JwksUri);
                IEnumerable<string> kids = disco.KeySet?.Keys?.Select(k => k.Kid);
                if (kids != null)
                {
                    //_logger.LogDebug($"Keyset contains the following kids: {string.Join(",", kids)}");
                }

                Options.ProviderInformation = new ProviderInformation
                {
                    IssuerName = disco.Issuer,
                    KeySet = disco.KeySet,

                    AuthorizeEndpoint = disco.AuthorizeEndpoint,
                    TokenEndpoint = disco.TokenEndpoint,
                    EndSessionEndpoint = disco.EndSessionEndpoint,
                    UserInfoEndpoint = disco.UserInfoEndpoint,
                    TokenEndPointAuthenticationMethods = disco.TokenEndpointAuthenticationMethodsSupported
                };
            }

            if (Options.ProviderInformation.IssuerName.IsMissing())
            {
                string error = "Issuer name is missing in provider information";

                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            if (Options.ProviderInformation.AuthorizeEndpoint.IsMissing())
            {
                string error = "Authorize endpoint is missing in provider information";

                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            if (Options.ProviderInformation.TokenEndpoint.IsMissing())
            {
                string error = "Token endpoint is missing in provider information";

                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            if (Options.ProviderInformation.KeySet == null && Options.Policy.Discovery.RequireKeySet)
            {
                string error = "Key set is missing in provider information";

                Log.Error(error);
                throw new InvalidOperationException(error);
            }
        }

        internal ClaimsPrincipal ProcessClaims(ClaimsPrincipal user, IEnumerable<Claim> userInfoClaims)
        {
            //_logger.LogTrace("ProcessClaims");

            HashSet<Claim> combinedClaims = new HashSet<Claim>(new ClaimComparer(new ClaimComparer.Options { IgnoreIssuer = true }));

            user.Claims.ToList().ForEach(c => combinedClaims.Add(c));
            userInfoClaims.ToList().ForEach(c => combinedClaims.Add(c));

            List<Claim> userClaims = Options.FilterClaims
                ? combinedClaims.Where(c => !Options.FilteredClaims.Contains(c.Type)).ToList()
                : combinedClaims.ToList();

            return new ClaimsPrincipal(new ClaimsIdentity(userClaims, user.Identity.AuthenticationType, user.Identities.First().NameClaimType, user.Identities.First().RoleClaimType));
        }
    }
}
