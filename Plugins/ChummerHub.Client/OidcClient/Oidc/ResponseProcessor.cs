// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel.Client;
using IdentityModel.OidcClient.Infrastructure;
using IdentityModel.OidcClient.Results;
using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.OidcClient
{
    internal class ResponseProcessor
    {
        private readonly OidcClientOptions _options;
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly CryptoHelper _crypto;
        private readonly Func<CancellationToken, Task> _refreshKeysAsync;

        public ResponseProcessor(OidcClientOptions options, Func<CancellationToken, Task> refreshKeysAsync)
        {
            _options = options;
            _refreshKeysAsync = refreshKeysAsync;
            //Log = options.LoggerFactory.CreateLogger<ResponseProcessor>();
            
            _crypto = new CryptoHelper(options);
        }

        public Task<ResponseValidationResult> ProcessResponseAsync(
            AuthorizeResponse authorizeResponse, 
            AuthorizeState state,
            Parameters backChannelParameters, 
            CancellationToken cancellationToken = default)
        {
            Log.Trace("ProcessResponseAsync");

            //////////////////////////////////////////////////////
            // validate common front-channel parameters
            //////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(authorizeResponse.Code))
            {
                return Task.FromResult(new ResponseValidationResult("Missing authorization code."));
            }

            if (string.IsNullOrEmpty(authorizeResponse.State))
            {
                return Task.FromResult(new ResponseValidationResult("Missing state."));
            }

            if (!string.Equals(state.State, authorizeResponse.State, StringComparison.Ordinal))
            {
                return Task.FromResult(new ResponseValidationResult("Invalid state."));
            }

            return ProcessCodeFlowResponseAsync(authorizeResponse, state, backChannelParameters, cancellationToken);
        }

        private async Task<ResponseValidationResult> ProcessCodeFlowResponseAsync(
            AuthorizeResponse authorizeResponse, 
            AuthorizeState state, 
            Parameters backChannelParameters, 
            CancellationToken cancellationToken)
        {
            Log.Trace("ProcessCodeFlowResponseAsync");

            //////////////////////////////////////////////////////
            // process back-channel response
            //////////////////////////////////////////////////////

            // redeem code for tokens
            TokenResponse tokenResponse = await RedeemCodeAsync(authorizeResponse.Code, state, backChannelParameters, cancellationToken);
            if (tokenResponse.IsError)
            {
                return new ResponseValidationResult($"Error redeeming code: {tokenResponse.Error ?? "no error code"} / {tokenResponse.ErrorDescription ?? "no description"}");
            }
            if (tokenResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return new ResponseValidationResult($"Error redeeming code: {tokenResponse.Raw}");
            }

            // validate token response
            TokenResponseValidationResult tokenResponseValidationResult = await ValidateTokenResponseAsync(tokenResponse, state, requireIdentityToken:false, cancellationToken: cancellationToken);
            if (tokenResponseValidationResult.IsError)
            {
                return new ResponseValidationResult($"Error validating token response: {tokenResponseValidationResult.Error}");
            }

            return new ResponseValidationResult
            {
                AuthorizeResponse = authorizeResponse,
                TokenResponse = tokenResponse,
                User = tokenResponseValidationResult.IdentityTokenValidationResult?.User ?? Principal.Create(_options.Authority)
            };
        }

        internal async Task<TokenResponseValidationResult> ValidateTokenResponseAsync(TokenResponse response, AuthorizeState state, bool requireIdentityToken, CancellationToken cancellationToken = default)
        {
            Log.Trace("ValidateTokenResponse");

            // token response must contain an access token
            if (response.AccessToken.IsMissing())
            {
                return new TokenResponseValidationResult("Access token is missing on token response.");
            }

            if (requireIdentityToken && response.IdentityToken.IsMissing())
            {
                // token response must contain an identity token (openid scope is mandatory)
                return new TokenResponseValidationResult("Identity token is missing on token response.");
            }

            if (response.IdentityToken.IsPresent())
            {
                IIdentityTokenValidator validator;
                if (_options.IdentityTokenValidator == null)
                {
                    if (_options.Policy.RequireIdentityTokenSignature == false)
                    {
                        validator = new NoValidationIdentityTokenValidator();
                    }
                    else
                    {
                        throw new InvalidOperationException("No IIdentityTokenValidator is configured. Either explicitly set a validator on the options, or set OidcClientOptions.Policy.RequireIdentityTokenSignature to false to skip validation.");
                    }
                }
                else
                {
                    validator = _options.IdentityTokenValidator;
                }
                
                IdentityTokenValidationResult validationResult = await validator.ValidateAsync(response.IdentityToken, _options, cancellationToken);

                if (validationResult.Error == "invalid_signature")
                {
                    await _refreshKeysAsync(cancellationToken);
                    validationResult = _options.IdentityTokenValidator != null
                        ? await _options.IdentityTokenValidator.ValidateAsync(response.IdentityToken, _options, cancellationToken)
                        : default;
                }
                
                if (validationResult?.IsError != false)
                {
                    return new TokenResponseValidationResult(validationResult?.Error ?? "Identity token validation error");
                }

                // validate at_hash
                if (!string.Equals(validationResult.SignatureAlgorithm, "none", StringComparison.OrdinalIgnoreCase))
                {
                    Claim atHash = validationResult.User.FindFirst(JwtClaimTypes.AccessTokenHash);
                    if (atHash == null)
                    {
                        if (_options.Policy.RequireAccessTokenHash)
                        {
                            return new TokenResponseValidationResult("at_hash is missing.");
                        }
                    }
                    else
                    {
                        if (!_crypto.ValidateHash(response.AccessToken, atHash.Value, validationResult.SignatureAlgorithm))
                        {
                            return new TokenResponseValidationResult("Invalid access token hash.");
                        }
                    }    
                }
                
                return new TokenResponseValidationResult(validationResult);
            }

            return new TokenResponseValidationResult((IdentityTokenValidationResult)null);
        }

        private async Task<TokenResponse> RedeemCodeAsync(string code, AuthorizeState state, Parameters backChannelParameters, CancellationToken cancellationToken)
        {
            Log.Trace("RedeemCodeAsync");

            HttpClient client = _options.CreateClient();
            TokenResponse tokenResult = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            {
                Address = _options.ProviderInformation.TokenEndpoint,

                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                ClientAssertion = _options.ClientAssertion,
                ClientCredentialStyle = _options.TokenClientCredentialStyle,

                Code = code,
                RedirectUri = state.RedirectUri,
                CodeVerifier = state.CodeVerifier,
                Parameters = backChannelParameters ?? new Parameters()
            }, cancellationToken).ConfigureAwait(false);

            return tokenResult;
        }
    }
}
