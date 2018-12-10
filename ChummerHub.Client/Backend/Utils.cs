using Chummer;
using Microsoft.Rest;
using SINners;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Chummer.frmCharacterRoster;

namespace ChummerHub.Client.Backend
{
    public static class StaticUtils
    {
        public static Type GetListType(object someList)
        {
            if (someList == null)
                throw new ArgumentNullException("someList");
            Type result;
            var type = someList.GetType();

            var genType = type.GetGenericTypeDefinition();


            if (!type.IsGenericType)
                throw new ArgumentException("someList", "Type must be List<>, but was " + type.FullName);
            try
            {
                result = type.GetGenericArguments()[0];
            }
            catch (Exception e)
            {
                var ex = new ArgumentException("someList", "Type must be List<>, but was " + type.FullName, e);
                throw ex;
            }

            return result;
        }


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

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        internal async static Task<TreeNode> GetCharacterRosterTreeNode(ConcurrentDictionary<string, CharacterCache> CharCache)
        {
            try
            {
                var response = await StaticUtils.Client.GetSINnersByAuthorizationWithHttpMessagesAsync();
                if (response == null || response.Body?.Any() == false)
                {
                    return null;
                }
                TreeNode onlineNode = new TreeNode()
                {
                    Text = "Online"
                };
                foreach (var sinner in response.Body)
                {
                    try
                    {
                        var objCache = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterCache>(sinner.JsonSummary);
                        objCache.OnDoubleClick -= objCache.OnDefaultDoubleClick;
                        objCache.OnDoubleClick += (sender, mainForm) =>
                           {
                               Character objOpenCharacter = mainForm.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FileName);

                               if (objOpenCharacter == null || !mainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                               {
                                   objOpenCharacter = mainForm.LoadCharacter(objCache.FilePath);
                                   mainForm.OpenCharacter(objOpenCharacter);
                               }
                           };
                        TreeNode objNode = new TreeNode
                        {
                            Text = sinner.Id?.ToString(),
                            Tag = sinner.Id.Value.ToString()
                        };
                        if (!string.IsNullOrEmpty(objCache.ErrorText))
                        {
                            objNode.ForeColor = Color.Red;
                            objNode.ToolTipText += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_Error", GlobalOptions.Language)
                                                    + LanguageManager.GetString("String_Colon", GlobalOptions.Language) + Environment.NewLine + objCache.ErrorText;
                        }
                        CharacterCache delObj;
                        CharCache.TryRemove(sinner.Id.Value.ToString(), out delObj);
                        CharCache.TryAdd(sinner.Id.Value.ToString(), objCache);

                        onlineNode.Nodes.Add(objNode);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceWarning("Could not deseialize CharacterCache-Object: " + sinner.JsonSummary);
                    }
                }
                if (onlineNode.Nodes.Count > 0)
                    return onlineNode;
                else
                    return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
            return null;
            //CharacterCache objCache = new CharacterCache(strFile);
            //if (!CharCache.TryAdd(strFile, objCache))
            //    CharCache[strFile] = objCache;

            //TreeNode objNode = new TreeNode
            //{
            //    ContextMenuStrip = cmsRoster,
            //    Text = CalculatedName(objCache),
            //    ToolTipText = objCache.FilePath.CheapReplace(Utils.GetStartupPath, () => '<' + Application.ProductName + '>'),
            //    Tag = strFile
            //};
            //if (!string.IsNullOrEmpty(objCache.ErrorText))
            //{
            //    objNode.ForeColor = Color.Red;
            //    objNode.ToolTipText += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_Error", GlobalOptions.Language)
            //                            + LanguageManager.GetString("String_Colon", GlobalOptions.Language) + Environment.NewLine + objCache.ErrorText;
            //}

            //return objNode;
            
        }
    }
}
