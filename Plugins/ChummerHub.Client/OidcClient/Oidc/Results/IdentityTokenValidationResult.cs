// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Security.Claims;

namespace IdentityModel.OidcClient.Results
{
    /// <summary>
    /// Identity token validation result
    /// </summary>
    public class IdentityTokenValidationResult : Result
    {
        /// <summary>
        /// The user represented by the identity token
        /// </summary>
        public ClaimsPrincipal User { get; set; }
        
        /// <summary>
        /// The signature algorithm of the identity token 
        /// </summary>
        public string SignatureAlgorithm { get; set; }
    }
}