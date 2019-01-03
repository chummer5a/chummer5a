using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{
    public class MyMessageHandler : DelegatingHandler
    {
        public MyMessageHandler()
        {
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = WebRequest.DefaultWebProxy,// new WebProxy("http://localhost:8888"),
                UseProxy = true,
               
                Credentials = CredentialCache.DefaultCredentials
            };
            httpClientHandler.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            httpClientHandler.PreAuthenticate = true;
            httpClientHandler.CookieContainer = new CookieContainer();
            httpClientHandler.UseDefaultCredentials = true;
            httpClientHandler.Credentials = System.Net.CredentialCache.DefaultCredentials;

            this.InnerHandler = httpClientHandler;   
        }
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine("Process request");
            // Call the inner handler.
            request.Headers.TryAddWithoutValidation("ContentType", "application/json");
            var response = await base.SendAsync(request, cancellationToken);
            Debug.WriteLine("Process response");
            return response;
        }
    }
}
