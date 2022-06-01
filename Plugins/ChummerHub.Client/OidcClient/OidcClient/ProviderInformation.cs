// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityModel.Jwk;

namespace IdentityModel.OidcClient
{
    /// <summary>
    /// Information about an OpenID Connect provider
    /// </summary>
    public class ProviderInformation
    {
        /// <summary>
        /// Gets or sets the name of the issuer.
        /// </summary>
        /// <value>
        /// The name of the issuer.
        /// </value>
        public string IssuerName { get; set; }

        /// <summary>
        /// Gets or sets the key set.
        /// </summary>
        /// <value>
        /// The key set.
        /// </value>
        public JsonWebKeySet KeySet { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint.
        /// </summary>
        /// <value>
        /// The token endpoint.
        /// </value>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the authorize endpoint.
        /// </summary>
        /// <value>
        /// The authorize endpoint.
        /// </value>
        public string AuthorizeEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the end session endpoint.
        /// </summary>
        /// <value>
        /// The end session endpoint.
        /// </value>
        public string EndSessionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the user information endpoint.
        /// </summary>
        /// <value>
        /// The user information endpoint.
        /// </value>
        public string UserInfoEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the token end point authentication methods.
        /// </summary>
        /// <value>
        /// The token end point authentication methods.
        /// </value>
        public IEnumerable<string> TokenEndPointAuthenticationMethods { get; set; } = new string[] { };


        /// <summary>
        /// Gets a value indicating whether [supports user information].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports user information]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsUserInfo => UserInfoEndpoint.IsPresent();
        
        /// <summary>
        /// Gets a value indicating whether [supports end session].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports end session]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsEndSession => EndSessionEndpoint.IsPresent();
    }
}