// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityModel.OidcClient.Browser
{
    /// <summary>
    /// Options for the browser used for login.
    /// </summary>
    public class BrowserOptions
    {
        /// <summary>
        /// Gets the start URL.
        /// </summary>
        /// <value>
        /// The start URL.
        /// </value>
        public string StartUrl { get; }

        /// <summary>
        /// Gets the end URL.
        /// </summary>
        /// <value>
        /// The end URL.
        /// </value>
        public string EndUrl { get; }

        /// <summary>
        /// Gets or sets the browser display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Visible;

        /// <summary>
        /// Gets or sets the browser timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserOptions"/> class.
        /// </summary>
        /// <param name="startUrl">The start URL.</param>
        /// <param name="endUrl">The end URL.</param>
        public BrowserOptions(string startUrl, string endUrl)
        {
            StartUrl = startUrl;
            EndUrl = endUrl;
        }
    }
}
