// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityModel.OidcClient.Results
{
    internal class TokenResponseValidationResult : Result
    {
        public TokenResponseValidationResult(string error)
        {
            Error = error;
        }

        public TokenResponseValidationResult(IdentityTokenValidationResult result)
        {
            IdentityTokenValidationResult = result;
        }

        public virtual IdentityTokenValidationResult IdentityTokenValidationResult { get; set; }
    }
}