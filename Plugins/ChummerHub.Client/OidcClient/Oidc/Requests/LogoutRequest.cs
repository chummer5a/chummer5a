// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel.OidcClient.Browser;

namespace IdentityModel.OidcClient
{
    /// <summary>
    /// A logout request
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Gets or sets the browser display mode.
        /// </summary>
        /// <value>
        /// The browser display mode.
        /// </value>
        public DisplayMode BrowserDisplayMode { get; set; } = DisplayMode.Visible;

        /// <summary>
        /// Gets or sets the browser timeout.
        /// </summary>
        /// <value>
        /// The browser timeout.
        /// </value>
        public int BrowserTimeout { get; set; } = 300;

        /// <summary>
        /// Gets or sets the id_token_hint.
        /// </summary>
        /// <value>
        /// The identifier token hint.
        /// </value>
        public string IdTokenHint { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state value passed back to client as query string on post_logout_redirect_uri
        /// </value>
        public string State { get; set; }
    }
}