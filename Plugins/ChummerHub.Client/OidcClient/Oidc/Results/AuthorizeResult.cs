// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityModel.OidcClient.Results
{
    internal class AuthorizeResult : Result
    {
        public virtual string Data { get; set; }
        public virtual AuthorizeState State { get; set; }
    }
}
