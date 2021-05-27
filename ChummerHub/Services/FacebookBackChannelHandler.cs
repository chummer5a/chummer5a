using System;
using System.Net.Http;

namespace ChummerHub.Services
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FacebookBackChannelHandler'
    public class FacebookBackChannelHandler : HttpClientHandler
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FacebookBackChannelHandler'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FacebookBackChannelHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FacebookBackChannelHandler.SendAsync(HttpRequestMessage, CancellationToken)'
            HttpRequestMessage request,
            System.Threading.CancellationToken cancellationToken)
        {
            // Replace the RequestUri so it's not malformed
            if (request.RequestUri?.AbsolutePath.Contains("/oauth") != true)
            {
                request.RequestUri = new Uri(request.RequestUri?.AbsoluteUri.Replace("?access_token", "&access_token") ?? string.Empty);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
