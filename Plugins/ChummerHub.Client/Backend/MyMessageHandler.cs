/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using NLog;

namespace ChummerHub.Client.Backend
{
    public class MyMessageHandler : HttpClientHandler
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        public MyMessageHandler()
        {
            Proxy = WebRequest.DefaultWebProxy;
            // new WebProxy("http://localhost:8888"),
            UseProxy = true;
            Credentials = CredentialCache.DefaultCredentials;
            PreAuthenticate = true;
            CookieContainer = new CookieContainer();
            UseDefaultCredentials = true;
            Proxy.Credentials = CredentialCache.DefaultCredentials;
        }

        private static int requestCounter;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            int myCounter = ++requestCounter;
            string msg = "Process request " + myCounter + ": " + request.RequestUri;
            Log.Debug<object>(msg);
            try
            {
                if (!string.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.BearerToken))
                {
                    AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Bearer", ChummerHub.Client.Properties.Settings.Default.BearerToken);
                    request.Headers.Authorization = auth;
                    //request.Headers.Add("Bearer", ChummerHub.Client.Properties.Settings.Default.AccessToken);
                }
                else if (!String.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.IdentityToken))
                {
                    AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Bearer", ChummerHub.Client.Properties.Settings.Default.IdentityToken);
                    request.Headers.Authorization = auth;
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                // Call the inner handler.
                request.Headers.TryAddWithoutValidation("ContentType", "application/json");
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                sw.Stop();
                msg = "Process response " + myCounter + " (" + (sw.ElapsedMilliseconds / 1000.0d).ToString(CultureInfo.InvariantCulture) + "): " + response.StatusCode;
                Log.Debug<object>(msg);
                return response;
            }
            catch(Exception e)
            {
                e.Data.Add("request", request.AsFormattedString());
                Log.Error(e);
                throw;
            }
        }

       
    }
}
