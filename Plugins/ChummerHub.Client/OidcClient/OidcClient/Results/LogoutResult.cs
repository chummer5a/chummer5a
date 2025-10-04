// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityModel.OidcClient
{
    /// <summary>
    /// Models the result of a logout
    /// </summary>
    /// <seealso cref="IdentityModel.OidcClient.Result" />
    public class LogoutResult : Result
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutResult"/> class.
        /// </summary>
        public LogoutResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutResult"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public LogoutResult(string error)
        {
            Error = error;
        }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public string Response { get; set; }
    }
}
