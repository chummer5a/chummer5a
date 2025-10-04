// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Net.Http;

namespace IdentityModel.OidcClient.Infrastructure
{
    internal static class OidcClientOptionsExtensions
    {
        public static HttpClient CreateClient(this OidcClientOptions options)
        {
            if (options.HttpClientFactory != null)
            {
                return options.HttpClientFactory(options);
            }
            
            HttpClient client;

            if (options.BackchannelHandler != null)
            {
                client = new HttpClient(options.BackchannelHandler);
            }
            else
            {
                client = new HttpClient();
            }

            client.Timeout = options.BackchannelTimeout;
            return client;
        }
    }
}
