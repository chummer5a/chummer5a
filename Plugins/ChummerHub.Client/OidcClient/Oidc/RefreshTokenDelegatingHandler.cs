// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Results;

namespace IdentityModel.OidcClient
{
    /// <summary>
    /// HTTP message delegating handler that encapsulates token handling and refresh
    /// </summary>
    public class RefreshTokenDelegatingHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly OidcClient _oidcClient;

        private string _accessToken;
        private string _refreshToken;
        
        private bool _disposed;

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets the current access token
        /// </summary>
        public string AccessToken
        {
            get
            {
                if (_lock.Wait(Timeout))
                {
                    try
                    {
                        return _accessToken;
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current refresh token
        /// </summary>
        public string RefreshToken
        {
            get
            {
                if (_lock.Wait(Timeout))
                {
                    try
                    {
                        return _refreshToken;
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Occurs when the tokens were refreshed successfully
        /// </summary>
        public event EventHandler<TokenRefreshedEventArgs> TokenRefreshed = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenDelegatingHandler" /> class.
        /// </summary>
        /// <param name="oidcClient">The oidc client.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="innerHandler">The inner handler.</param>
        /// <exception cref="ArgumentNullException">oidcClient</exception>
        public RefreshTokenDelegatingHandler(OidcClient oidcClient, string accessToken, string refreshToken, HttpMessageHandler innerHandler = null)
        {
            _oidcClient = oidcClient ?? throw new ArgumentNullException(nameof(oidcClient));

            if (refreshToken.IsMissing()) throw new ArgumentNullException(nameof(refreshToken));
            _refreshToken = refreshToken;

            if (accessToken.IsMissing()) throw new ArgumentNullException(nameof(accessToken));
            _accessToken = accessToken;

            if (innerHandler != null) InnerHandler = innerHandler;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
        /// </returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string accessToken = await GetAccessTokenAsync(cancellationToken);
            if (accessToken.IsMissing() && await RefreshTokensAsync(cancellationToken) == false)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = request };
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            if (await RefreshTokensAsync(cancellationToken) == false)
            {
                return response;
            }

            response.Dispose(); // This 401 response will not be used for anything so is disposed to unblock the socket.

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Net.Http.DelegatingHandler" />, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _lock.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task<bool> RefreshTokensAsync(CancellationToken cancellationToken)
        {
            if (await _lock.WaitAsync(Timeout, cancellationToken).ConfigureAwait(false))
            {
                if (_refreshToken.IsMissing())
                {
                    return false;
                }

                try
                {
                    RefreshTokenResult response = await _oidcClient.RefreshTokenAsync(_refreshToken, cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (!response.IsError)
                    {
                        _accessToken = response.AccessToken;
                        if (!response.RefreshToken.IsMissing())
                        {
                            _refreshToken = response.RefreshToken;
                        }

                        // This task definitely should not be left unawaited in case there's an exception that needs to be gotten,
                        // but the original code was set up this way so and I have no time to look at how to actually make this safe.
                        // There was a pragma here before that removed the Warning, but I removed it because this is a warning that should not be ignored.
                        Task.Run(() =>
                        {
                            foreach (EventHandler<TokenRefreshedEventArgs> del in TokenRefreshed.GetInvocationList())
                            {
                                try
                                {
                                    del(this, new TokenRefreshedEventArgs(response.AccessToken, response.RefreshToken, response.ExpiresIn));
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }).ConfigureAwait(false);

                        return true;
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            return false;
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (await _lock.WaitAsync(Timeout, cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    return _accessToken;
                }
                finally
                {
                    _lock.Release();
                }
            }

            return null;
        }
    }
}
