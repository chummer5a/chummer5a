// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityModel.OidcClient.Browser
{
    /// <summary>
    /// Possible browser results.
    /// </summary>
    public enum BrowserResultType
    {
        /// <summary>
        /// success
        /// </summary>
        Success,
        /// <summary>
        /// HTTP error
        /// </summary>
        HttpError,
        /// <summary>
        /// user cancel
        /// </summary>
        UserCancel,
        /// <summary>
        /// timeout
        /// </summary>
        Timeout,
        /// <summary>
        /// unknown error
        /// </summary>
        UnknownError
    }
}