using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Http;

namespace NeonJungleLC.GoogleSheets
{
    public class ProxySupportedHttpClientFactory : HttpClientFactory
    {
        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
        {
            IWebProxy iProxy = WebRequest.DefaultWebProxy;
            WebProxy myProxy = new WebProxy(iProxy.GetProxy(new Uri("https://www.google.com")));
            // potentially, setup credentials on the proxy here
            myProxy.Credentials = CredentialCache.DefaultCredentials;
            myProxy.UseDefaultCredentials = true;
            
            //var proxy = new WebProxy("http://proxyserver:8080", true, null, null);

            var webRequestHandler = new WebRequestHandler()
            {
                UseProxy = true,
                Proxy = myProxy,
                UseCookies = false
            };

            return webRequestHandler;
        }
    }
}
