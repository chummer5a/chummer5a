// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityModel.OidcClient.Results
{
    /// <summary>
    /// The result of a userinfo request.
    /// </summary>
    /// <seealso cref="IdentityModel.OidcClient.Result" />
    public class UserInfoResult : Result
    {
        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public virtual IEnumerable<Claim> Claims { get; internal set; }
    }
}
