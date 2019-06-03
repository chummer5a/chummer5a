using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ChummerHub.Client.Backend
{
    public class MyMessageHandler : DelegatingHandler
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
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

        public static int requestCounter = 0;

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int myCounter = requestCounter++;
                string msg = "Process request " + myCounter + ": " + request.RequestUri;
                Log.Debug((object)msg);
                // Call the inner handler.
                request.Headers.TryAddWithoutValidation("ContentType", "application/json");
                var response = await base.SendAsync(request, cancellationToken);
                msg = "Process response " + myCounter + " (" + (((double)sw.ElapsedMilliseconds)/1000) + "): " + response.StatusCode;
                Log.Debug((object)msg);
                return response;
            }
            catch(Exception e)
            {
                Log.Error(e);
                throw;
            }
            
        }
    }
}
