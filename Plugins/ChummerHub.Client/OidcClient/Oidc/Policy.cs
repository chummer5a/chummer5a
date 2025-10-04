// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel.Client;
using System.Collections.Generic;

namespace IdentityModel.OidcClient
{
    /// <summary>
    /// The validation policy for OidcClient
    /// </summary>
    public class Policy
    {
        /// <summary>
        /// Gets or sets the policy for discovery.
        /// </summary>
        /// <value>
        /// The discovery.
        /// </value>
        public DiscoveryPolicy Discovery { get; set; } = new DiscoveryPolicy();

        /// <summary>
        /// Gets or sets a value indicating whether at_hash is required (defaults to false).
        /// </summary>
        /// <value>
        /// <c>true</c> if at_hash is required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireAccessTokenHash { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether an identity token is required on refresh token responses (defaults to false).
        /// </summary>
        /// <value>
        /// <c>true</c> if [require identity token on refresh token response]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireIdentityTokenOnRefreshTokenResponse { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether an identity token must be signed or not (unsigned identity tokens are only allowed in authorization code flow)
        /// </summary>
        /// <value>
        /// <c>true</c> if identity token must be signed; otherwise, <c>false</c>.
        /// </value>
        public bool RequireIdentityTokenSignature { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the identity token issuer name should match.
        /// </summary>
        /// <value><c>true</c> if identity token issuer name should match; otherwise, <c>false</c>.</value>
        public bool ValidateTokenIssuerName { get; set; } = true;

        /// <summary>
        /// Gets or sets the supported identity token signing algorithms.
        /// </summary>
        /// <value>
        /// The supported algorithms.
        /// </value>
        public ICollection<string> ValidSignatureAlgorithms { get; set; } = new HashSet<string>
        {
            OidcConstants.Algorithms.Asymmetric.RS256,
            OidcConstants.Algorithms.Asymmetric.RS384,
            OidcConstants.Algorithms.Asymmetric.RS512,

            OidcConstants.Algorithms.Asymmetric.PS256,
            OidcConstants.Algorithms.Asymmetric.PS384,
            OidcConstants.Algorithms.Asymmetric.PS512,

            OidcConstants.Algorithms.Asymmetric.ES256,
            OidcConstants.Algorithms.Asymmetric.PS384,
            OidcConstants.Algorithms.Asymmetric.PS512
        };
    }
}
