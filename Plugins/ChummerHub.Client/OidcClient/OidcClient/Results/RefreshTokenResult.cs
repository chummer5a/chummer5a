// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityModel.OidcClient.Results
{
    /// <summary>
    /// The result of a refresh token request.
    /// </summary>
    /// <seealso cref="IdentityModel.OidcClient.Result" />
    public class RefreshTokenResult : Result
    {
        /// <summary>
        /// Gets or sets the identity token.
        /// </summary>
        /// <value>
        /// The identity token.
        /// </value>
        public virtual string IdentityToken { get; internal set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public virtual string AccessToken { get; internal set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public virtual string RefreshToken { get; internal set; }

        /// <summary>
        /// Gets or sets the expires in (seconds).
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        public virtual int ExpiresIn { get; internal set; }
        
        /// <summary>
        /// Gets or sets the access token expiration.
        /// </summary>
        /// <value>
        /// The access token expiration.
        /// </value>
        public virtual DateTimeOffset AccessTokenExpiration { get; internal set; }

    }
}