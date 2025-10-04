// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient.Infrastructure;
using IdentityModel.OidcClient.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.OidcClient
{
    internal class AuthorizeClient
    {
        private readonly CryptoHelper _crypto;
        private readonly ILogger<AuthorizeClient> _logger;
        private readonly OidcClientOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public AuthorizeClient(OidcClientOptions options)
        {
            _options = options;
            _logger = options.LoggerFactory.CreateLogger<AuthorizeClient>();
            _crypto = new CryptoHelper(options);
        }

        public async Task<AuthorizeResult> AuthorizeAsync(AuthorizeRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("AuthorizeAsync");

            if (_options.Browser == null)
            {
                throw new InvalidOperationException("No browser configured.");
            }

            AuthorizeResult result = new AuthorizeResult
            {
                State = CreateAuthorizeState(request.ExtraParameters)
            };

            var browserOptions = new BrowserOptions(result.State.StartUrl, _options.RedirectUri)
            {
                Timeout = TimeSpan.FromSeconds(request.Timeout),
                DisplayMode = request.DisplayMode
            };

            var browserResult = await _options.Browser.InvokeAsync(browserOptions, cancellationToken);

            if (browserResult.ResultType == BrowserResultType.Success)
            {
                result.Data = browserResult.Response;
                return result;
            }

            result.Error = browserResult.Error ?? browserResult.ResultType.ToString();
            return result;
        }

        public async Task<BrowserResult> EndSessionAsync(LogoutRequest request,
            CancellationToken cancellationToken = default)
        {
            var endpoint = _options.ProviderInformation.EndSessionEndpoint;
            if (endpoint.IsMissing())
            {
                throw new InvalidOperationException("Discovery document has no end session endpoint");
            }

            var url = CreateEndSessionUrl(endpoint, request);

            var browserOptions = new BrowserOptions(url, _options.PostLogoutRedirectUri ?? string.Empty)
            {
                Timeout = TimeSpan.FromSeconds(request.BrowserTimeout),
                DisplayMode = request.BrowserDisplayMode
            };

            return await _options.Browser.InvokeAsync(browserOptions, cancellationToken);
        }

        public AuthorizeState CreateAuthorizeState(Parameters frontChannelParameters)
        {
            _logger.LogTrace("CreateAuthorizeStateAsync");

            var pkce = _crypto.CreatePkceData();

            var state = new AuthorizeState
            {
                State = _crypto.CreateState(_options.StateLength),
                RedirectUri = _options.RedirectUri,
                CodeVerifier = pkce.CodeVerifier,
            };

            state.StartUrl = CreateAuthorizeUrl(state.State, pkce.CodeChallenge, frontChannelParameters);

            _logger.LogDebug(LogSerializer.Serialize(state));

            return state;
        }

        internal string CreateAuthorizeUrl(string state, string codeChallenge,
            Parameters frontChannelParameters)
        {
            _logger.LogTrace("CreateAuthorizeUrl");

            var parameters = CreateAuthorizeParameters(state, codeChallenge, frontChannelParameters);
            var request = new RequestUrl(_options.ProviderInformation.AuthorizeEndpoint);

            return request.Create(parameters);
        }

        internal string CreateEndSessionUrl(string endpoint, LogoutRequest request)
        {
            _logger.LogTrace("CreateEndSessionUrl");

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
            _logger.LogTrace("CreateAuthorizeParameters");

            var parameters = new Parameters
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

            if (_options.Resource.Any())
            {
                foreach (var resource in _options.Resource)
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
                foreach (var entry in frontChannelParameters)
                {
                    parameters.Add(entry.Key, entry.Value);
                }
            }

            return parameters;
        }
    }
}
