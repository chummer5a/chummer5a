// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel.OidcClient.Browser;
using IdentityModel.Client;

namespace IdentityModel.OidcClient
{
    /// <summary>
    /// A login request.
    /// </summary>
    public class LoginRequest
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
        /// Gets or sets extra parameters for the front-channel request.
        /// </summary>
        /// <value>
        /// The extra parameters.
        /// </value>
        public Parameters FrontChannelExtraParameters { get; set; } = new Parameters();
        
        /// <summary>
        /// Gets or sets extra parameters for the back-channel request.
        /// </summary>
        /// <value>
        /// The extra parameters.
        /// </value>
        public Parameters BackChannelExtraParameters { get; set; } = new Parameters();
    }
}
