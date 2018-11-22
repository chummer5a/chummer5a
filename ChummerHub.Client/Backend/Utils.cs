using Microsoft.Rest;
using SINners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{
    public static class StaticUtils
    {
        public static Utils MyUtils = new Utils();
        static StaticUtils()
        {
        }
        public static bool IsUnitTest { get { return MyUtils.IsUnitTest; } }

        private static CookieContainer _AuthorizationCookieContainer = null;

        public static CookieContainer AuthorizationCookieContainer
        {
            get
            {
                Properties.Settings.Default.Reload();
                if ((_AuthorizationCookieContainer == null)
                    || (String.IsNullOrEmpty(Properties.Settings.Default.CookieData)))
                {
                    Uri uri = new Uri(Properties.Settings.Default.SINnerUrl);
                    string cookieData = Properties.Settings.Default.CookieData;
                    _AuthorizationCookieContainer = GetUriCookieContainer(uri, cookieData);
                }
                return _AuthorizationCookieContainer;
            }
            set
            {
                _AuthorizationCookieContainer = value;
                if (value == null)
                {
                    Properties.Settings.Default.CookieData = null;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static CookieContainer GetUriCookieContainer(Uri uri, string cookieData)
        {
            CookieContainer cookies = null;
            if (String.IsNullOrEmpty(cookieData))
                cookieData = GetUriCookieData(uri);
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData);
                Properties.Settings.Default.CookieData = cookieData;
                int i = uri.AbsoluteUri.IndexOf(uri.AbsolutePath);
                
                Properties.Settings.Default.SINnerUrl = uri.AbsoluteUri.Substring(0, i);
                if (Properties.Settings.Default.SINnerUrl.Length < 7)
                    Properties.Settings.Default.SINnerUrl = uri.AbsoluteUri;
                Properties.Settings.Default.Save();
            }
            return cookies;
        }

        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
                                        string url,
                                        string cookieName,
                                        StringBuilder cookieData,
                                        ref int size,
                                        Int32 dwFlags,
                                        IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;



        /// <summary>
        /// Gets the actual Cookie data
        /// </summary>
        public static string GetUriCookieData(Uri uri)
        {
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            return cookieData.ToString().Replace(';', ',');
        }

        private static SINnersClient _client = null;
        public static SINnersClient Client
        {
            get
            {
                if (_client == null)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(Properties.Settings.Default.SINnerUrl))
                            return null;
                        ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        //ServiceClientCredentials credentials = new TokenCredentials("Bearer");
                        Uri baseUri = new Uri(Properties.Settings.Default.SINnerUrl);
                        Microsoft.Rest.ServiceClientCredentials credentials = new MyCredentials();
                        //ServiceClientCredentials creds = new ServiceClientCredentials();
                        DelegatingHandler delegatingHandler = new MyMessageHandler();
                        HttpClientHandler httpClientHandler = new HttpClientHandler();
                        httpClientHandler.CookieContainer = AuthorizationCookieContainer;
                        _client = new SINnersClient(baseUri, credentials, httpClientHandler, delegatingHandler);

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        throw;
                    }
                }
                return _client;
            }
            set
            {
                _client = value;
            }

        }
    }

    public class Utils
    {
        public Utils()
        {
            IsUnitTest = false;
        }

        public bool IsUnitTest { get; set; }
    }
}
