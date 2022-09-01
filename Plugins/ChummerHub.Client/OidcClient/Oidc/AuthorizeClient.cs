// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.Infrastructure;
using IdentityModel.OidcClient.Results;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.OidcClient
{
    internal class AuthorizeClient
    {
        private readonly CryptoHelper _crypto;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly OidcClientOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public AuthorizeClient(OidcClientOptions options)
        {
            _options = options;
            //_logger = options.LoggerFactory.CreateLogger<AuthorizeClient>();
            _crypto = new CryptoHelper(options);
        }

        public async Task<AuthorizeResult> AuthorizeAsync(AuthorizeRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.Trace("AuthorizeAsync");

            if (_options.Browser == null)
            {
                throw new InvalidOperationException("No browser configured.");
            }

            AuthorizeResult result = new AuthorizeResult
            {
                State = CreateAuthorizeState(request.ExtraParameters)
            };

            BrowserOptions browserOptions = new BrowserOptions(result.State.StartUrl, _options.RedirectUri)
            {
                Timeout = TimeSpan.FromSeconds(request.Timeout),
                DisplayMode = request.DisplayMode,
                
            };

            BrowserResult browserResult = await _options.Browser.InvokeAsync(browserOptions, cancellationToken);

            if (browserResult.ResultType == BrowserResultType.Success)
            {
                result.Data = browserResult.Response;
                return result;
            }

            result.Error = browserResult.Error ?? browserResult.ResultType.ToString();
            return result;
        }

        public Task<BrowserResult> EndSessionAsync(LogoutRequest request,
            CancellationToken cancellationToken = default)
        {
            string endpoint = _options.ProviderInformation.EndSessionEndpoint;
            if (endpoint.IsMissing())
            {
                throw new InvalidOperationException("Discovery document has no end session endpoint");
            }

            string url = CreateEndSessionUrl(endpoint, request);

            BrowserOptions browserOptions = new BrowserOptions(url, _options.PostLogoutRedirectUri ?? string.Empty)
            {
                Timeout = TimeSpan.FromSeconds(request.BrowserTimeout),
                DisplayMode = request.BrowserDisplayMode
            };

            return _options.Browser.InvokeAsync(browserOptions, cancellationToken);
        }

        public AuthorizeState CreateAuthorizeState(Parameters frontChannelParameters)
        {
            _logger.Trace("CreateAuthorizeStateAsync");

            CryptoHelper.Pkce pkce = _crypto.CreatePkceData();

            AuthorizeState state = new AuthorizeState
            {
                State = _crypto.CreateState(_options.StateLength),
                RedirectUri = _options.RedirectUri,
                CodeVerifier = pkce.CodeVerifier,
            };

            state.StartUrl = CreateAuthorizeUrl(state.State, pkce.CodeChallenge, frontChannelParameters);

            _logger.Debug(LogSerializer.Serialize(state));

            return state;
        }

        internal string CreateAuthorizeUrl(string state, string codeChallenge,
            Parameters frontChannelParameters)
        {
            _logger.Trace("CreateAuthorizeUrl");

            Parameters parameters = CreateAuthorizeParameters(state, codeChallenge, frontChannelParameters);
            RequestUrl request = new RequestUrl(_options.ProviderInformation.AuthorizeEndpoint);

            return request.Create(parameters);
        }

        internal string CreateEndSessionUrl(string endpoint, LogoutRequest request)
        {
            _logger.Trace("CreateEndSessionUrl");

            return new RequestUrl(endpoint).CreateEndSessionUrl(
                idTokenHint: request.IdTokenHint,
                postLogoutRedirectUri: _options.PostLogoutRedirectUri,
                state: request.State);
        }

        internal Parameters CreateAuthorizeParameters(
            string state,
            string codeChallenge,
            Parameters frontChannelParameters)
        {
            _logger.Trace("CreateAuthorizeParameters");

            Parameters parameters = new Parameters
            {
                { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
                { OidcConstants.AuthorizeRequest.State, state },
                { OidcConstants.AuthorizeRequest.CodeChallenge, codeChallenge },
                { OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
            };

            if (_options.ClientId.IsPresent())
            {
                parameters.Add(OidcConstants.AuthorizeRequest.ClientId, _options.ClientId);
            }

            if (_options.Scope.IsPresent())
            {
                parameters.Add(OidcConstants.AuthorizeRequest.Scope, _options.Scope);
            }

            if (_options.Resource.Count > 0)
            {
                foreach (string resource in _options.Resource)
                {
                    parameters.Add(OidcConstants.AuthorizeRequest.Resource, resource);
                }
            }

            if (_options.RedirectUri.IsPresent())
            {
                parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, _options.RedirectUri);
            }

            if (frontChannelParameters != null)
            {
                foreach (KeyValuePair<string, string> entry in frontChannelParameters)
                {
                    parameters.Add(entry.Key, entry.Value);
                }
            }

            return parameters;
        }
    }
}
