using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Properties;
using ChummerHub.Client.Sinners;
using ChummerHub.Client.UI;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Rest;
using Newtonsoft.Json;
using NLog;

namespace ChummerHub.Client.Backend
{
    public static class StaticUtils
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        public static Type GetListType(object someList)
        {
            if (someList == null)
                throw new ArgumentNullException(nameof(someList));
            Type result;
            Type type = someList.GetType();

            if (!type.IsGenericType)
                throw new ArgumentException("Type must be List<>, but was " + type.FullName, nameof(someList));
            try
            {
                result = type.GetGenericArguments()[0];
            }
            catch (Exception e)
            {
                ArgumentException ex = new ArgumentException("Type must be List<>, but was " + type.FullName, nameof(someList), e);
                throw ex;
            }

            return result;
        }


        public static readonly Utils MyUtils = new Utils();
        static StaticUtils()
        {
        }

        public static bool IsUnitTest => MyUtils.IsUnitTest;

        private static CookieContainer _AuthorizationCookieContainer;

        private static List<string> _userRoles;
        public static List<string> UserRoles
        {
            get
            {
                if (_userRoles == null)
                {
                    using (new CursorWait(PluginHandler.MainForm))
                    {
                        int counter = 0;
                        //just wait until the task from the startup finishes...
                        while (_userRoles == null)
                        {
                            ++counter;
                            if (counter > 10 * 5)
                            {
                                _userRoles = new List<string>
                                {
                                    "none"
                                };
                                break;
                            }
                            Chummer.Utils.SafeSleep();
                        }
                    }
                }
                return _userRoles;
            }
            set => _userRoles = new List<string>(value);
        }

        private static List<string> _possibleRoles;
        public static List<string> PossibleRoles
        {
            get
            {
                if (_possibleRoles == null)
                {
                    using (new CursorWait(PluginHandler.MainForm))
                    {
                        int counter = 0;
                        //just wait until the task from the startup finishes...
                        while (_possibleRoles == null)
                        {
                            counter++;
                            if (counter > 10 * 5)
                            {
                                _possibleRoles = new List<string> { "none" };
                                break;
                            }
                            Chummer.Utils.SafeSleep();
                        }
                    }
                }
                return _possibleRoles;
            }
            set => _possibleRoles = new List<string>(value);
        }

        public static CookieContainer AuthorizationCookieContainer
        {
            get
            {
                try
                {
                    if (_AuthorizationCookieContainer == null
                        || string.IsNullOrEmpty(Settings.Default.CookieData))
                    {
                        Uri uri = new Uri(Settings.Default.SINnerUrl);
                        string cookieData = Settings.Default.CookieData;
                        _AuthorizationCookieContainer = GetUriCookieContainer(uri, cookieData);
                    }
                    return _AuthorizationCookieContainer;
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
                return _AuthorizationCookieContainer;

            }
            set
            {
                _AuthorizationCookieContainer = value;
                if (value == null)
                {
                    Uri uri = new Uri(Settings.Default.SINnerUrl);
                    DeleteUriCookieData(uri);
                    Settings.Default.CookieData = null;
                    Settings.Default.Save();
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
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            //if (string.IsNullOrEmpty(cookieData))
            //    throw new ArgumentNullException(nameof(cookieData));
            CookieContainer cookies = null;
            try
            {
                if (string.IsNullOrEmpty(cookieData))
                    cookieData = GetUriCookieData(uri);
                if (cookieData == null)
                    return null;
                if (cookieData.Length > 0)
                {
                    try
                    {
                        cookies = new CookieContainer();
                        cookies.SetCookies(uri, cookieData);
                        Settings.Default.CookieData = cookieData;
                        int i = uri.AbsoluteUri.IndexOf(uri.AbsolutePath, StringComparison.Ordinal);

                        Settings.Default.SINnerUrl = uri.AbsoluteUri.Substring(0, i);
                        if (Settings.Default.SINnerUrl.Length < 7)
                            Settings.Default.SINnerUrl = uri.AbsoluteUri;
                        Settings.Default.Save();
                    }
                    catch(Exception e)
                    {
                        Log.Error(e);
                        throw;
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
                throw;
            }
            return cookies;
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(
                                        string url,
                                        string cookieName,
                                        StringBuilder cookieData,
                                        ref int size,
                                        int dwFlags,
                                        IntPtr lpReserved);

        private const int InternetCookieHttponly = 0x2000;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        /// <summary>
        /// Gets the actual Cookie data
        /// </summary>
        public static string GetUriCookieData(Uri uri)
        {
            if (uri == null)
                return string.Empty;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            try
            {
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
            }
            catch(Exception e)
            {
                Log.Error(e);
                throw;
            }
            return cookieData.Replace(';', ',').ToString();
        }


        /// <summary>
        /// Gets the actual Cookie data
        /// </summary>
        public static bool DeleteUriCookieData(Uri uri)
        {
            if (uri == null)
                return true;
            Cookie temp1 = new Cookie("KEY1", "VALUE1", "/Path/To/My/App", "/");
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            try
            {
                if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
                {
                    if (datasize < 0)
                        return false;
                    // Allocate stringbuilder large enough to hold the cookie
                    cookieData = new StringBuilder(datasize);
                    if (!InternetGetCookieEx(
                        uri.ToString(),
                        null, cookieData,
                        ref datasize,
                        InternetCookieHttponly,
                        IntPtr.Zero))
                        return false;
                }
                if (InternetSetCookie(uri.ToString(), null, ""))
                {
                    return true;
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
            return false;
        }


        private static bool clientErrorShown;

        private static bool? _clientNOTworking;

        private static SinnersClient _clientTask;
        private static SinnersClient _client;
        public static SinnersClient GetClient(bool reset = false)
        {
            if (reset)
            {
                _client = null;
                _clientNOTworking = false;
                _clientTask = null;
            }
            if (_client == null && (!_clientNOTworking.HasValue || _clientNOTworking == false))
            {
                _client = GetSINnersClient();
                if (_client != null)
                    _clientNOTworking = false;
            }
            return _client;
        }

        private static SinnersClient GetSINnersClient()
        {
            SinnersClient client = null;
            try
            {
                Assembly assembly = Assembly.GetAssembly(typeof(ChummerMainForm));
                Settings.Default.SINnerUrl = assembly.GetName().Version.Build == 0
                    ? "https://chummer-stable.azurewebsites.net"
                    : "https://chummer-beta.azurewebsites.net";
                Settings.Default.Save();
                if (Debugger.IsAttached)
                {
                    Settings.Default.SINnerUrl = "https://chummer-beta.azurewebsites.net";
                    //try
                    //{
                    //    string local = "http://localhost:5000/";
                    //    var request = WebRequest.Create(new Uri(local));
                    //    WebResponse response = request.GetResponse();
                    //    Settings.Default.SINnerUrl = local;
                    //    Log.Info("Connected to " + local + ".");
                    //}
                    //catch (Exception)
                    //{


                    //}
                }
                Log.Info("Connected to " + Settings.Default.SINnerUrl + ".");

                ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                Uri baseUri = new Uri(Settings.Default.SINnerUrl);
                try
                {
                    _ = new MyCredentials();
                }
                catch (Exception e)
                {
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    TelemetryClient ct = new TelemetryClient();
                    ct.TrackException(et);
                    Log.Error(e);
                }

                HttpClientHandler delegatingHandler = new MyMessageHandler();
                //HttpClientHandler httpClientHandler = new HttpClientHandler();
                CookieContainer temp = AuthorizationCookieContainer;
                if (temp != null)
                    delegatingHandler.CookieContainer = temp;
                HttpClient httpClient = new HttpClient(delegatingHandler);
                client = new SinnersClient(baseUri.ToString(), httpClient);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                if (!clientErrorShown)
                {
                    clientErrorShown = true;
                    Exception inner = ex;
                    while (inner.InnerException != null)
                        inner = inner.InnerException;
                    string msg = "Error connecting to SINners: " + Environment.NewLine;
                    msg += "(the complete error description is copied to clipboard)" + Environment.NewLine + Environment.NewLine + inner;
                    PluginHandler.MainForm.DoThreadSafe(() => { Clipboard.SetText(ex.ToString()); });
                    msg += Environment.NewLine + Environment.NewLine + "Please check the Plugin-Options dialog.";
                    Program.MainForm.ShowMessageBox(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return client;
        }



        internal static async Task WebCall(string callback, int progress, string text)
        {
            try
            {
                Log.Trace("Posting WebCall " + callback + ": " + text + "(" + progress + ")");
                using (HttpClient client = new HttpClient())
                {
                    Uri uri = new Uri(callback);
                    string baseuri = uri.GetLeftPart(UriPartial.Authority);
                    client.BaseAddress = new Uri(baseuri);
                    using (FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("Progress", progress.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("Text", text)
                    }))
                    {
                        HttpResponseMessage result = await client.PostAsync(uri, content);
                        string resultContent = await result.Content.ReadAsStringAsync();
                        Log.Trace("Result from WebCall " + callback + ": " + resultContent);
                    }
                }
            }
            catch (Exception e)
            {
                string message = "This exception can be ignored! " + e;
                Log.Debug(e, message);
            }
        }
    }

    public class Utils
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        public Utils()
        {
            IsUnitTest = false;
        }

        public bool IsUnitTest { get; set; }

        private static List<TreeNode> _myTreeNodeList;
        private static IList<TreeNode> MyTreeNodeList
        {
            get => _myTreeNodeList ?? (_myTreeNodeList = new List<TreeNode>());
            set => _myTreeNodeList = new List<TreeNode>(value);
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        internal static async Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(bool forceUpdate, Func<Task<ResultAccountGetSinnersByAuthorization>> myGetSINnersFunction)
        {
            if (MyTreeNodeList != null && !forceUpdate)
                return MyTreeNodeList;
            MyTreeNodeList = new List<TreeNode>();
            try
            {
                await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                {
                    MyTreeNodeList.Clear();
                });

                ResultAccountGetSinnersByAuthorization response = null;
                try
                {
                    response = await myGetSINnersFunction();
                }
                catch(ArgumentException e)
                {
                    return await ApiExceptionHandling(e, e.Message);
                }
                catch(SerializationException apie)
                {
                    return await ApiExceptionHandling(apie, apie.Content);
                }
                if (response == null || !response.CallSuccess)
                {
                    string msg = "Could not load online Sinners: " + (response?.ErrorText ?? "Response is null.");
                    
                    Log.Warn(msg);
                    TreeNode errornode = new TreeNode
                    {
                        Text = "Error contacting SINners"
                    };

                    CharacterCache errorCache = new CharacterCache
                    {
                        ErrorText = "Error is copied to clipboard!" + Environment.NewLine + Environment.NewLine + msg,
                        CharacterAlias = "Error loading SINners"
                    };
                    errorCache.OnMyAfterSelect += (sender, args) =>
                    {
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            Clipboard.SetText(msg);
                        });
                    };
                    errornode.Tag = errorCache;
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        MyTreeNodeList.Add(errornode);
                    });
                    return MyTreeNodeList;
                }

                ResultAccountGetSinnersByAuthorization res = response;
                SINSearchGroupResult result = res.MySINSearchGroupResult;
                if (result?.Roles != null)
                    StaticUtils.UserRoles = result.Roles.ToList();

                string info = "Connected to SINners in version " + result?.Version?.AssemblyVersion + ".";
                Log.Info(info);
                MyTreeNodeList.AddRange(CharacterRosterTreeNodifyGroupList(result?.SinGroups));
                return MyTreeNodeList;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

            async Task<ICollection<TreeNode>> ApiExceptionHandling(Exception se, string content)
            {
                if (content.Contains("Log in - ChummerHub") || content == "User not logged in.")
                {
                    TreeNode node = new TreeNode("Online, not logged in")
                    {
                        ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login",
                        Tag = new Action(() =>
                        {
                            using (new CursorWait(Program.MainForm))
                                using (EditGlobalSettings frmOptions = new EditGlobalSettings("tabPlugins"))
                                    frmOptions.ShowDialog(Program.MainForm);
                        })
                    };
                    Log.Warn(se, "Online, not logged in");
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        MyTreeNodeList.Add(node);
                    });
                }
                else
                {
                    string msg = "Could not load response from SINners:" + Environment.NewLine;
                    msg += se.Message;
                    if (se.InnerException != null)
                    {
                        msg += Environment.NewLine + se.InnerException.Message;
                    }
                    Log.Error(se, msg);
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        MyTreeNodeList.Add(new TreeNode
                        {
                            ToolTipText = msg
                        });
                    });
                }
                return MyTreeNodeList;
            }
        }

        public static ResultBase HandleError(Exception e)
        {
            ResultBase rb = new ResultBase
            {
                ErrorText = e?.Message,
                MyException = e,
                CallSuccess = false
            };
            if (!string.IsNullOrEmpty(rb.ErrorText) || rb.MyException != null)
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    using (frmSINnerResponse frmSIN = new frmSINnerResponse
                    {
                        TopMost = true
                    })
                    {
                        frmSIN.SINnerResponseUI.Result = rb;
                        if (rb.MyException != null)
                        {
                            Log.Info(rb.MyException, "The SINners WebService had a problem. This was it's response: ");
                            frmSIN.SINnerResponseUI.Result.ErrorText =
                                "This is NOT an exception from Chummer itself, but from the SINners WebService. This error happend \"in the cloud\": " +
                                rb.ErrorText;
                            if (rb.MyException?.InnerException != null)
                            {
                                frmSIN.SINnerResponseUI.Result.ErrorText += Environment.NewLine + rb.MyException?.InnerException.Message;
                            }

                        }
                        else
                        {
                            Log.Error(e, " Response from SINners WebService: ");
                        }

                        frmSIN.ShowDialog(PluginHandler.MainForm);
                    }
                });
            }
            return rb;
        }

        public static object ShowErrorResponseForm(object objResultBase, Exception e = null)
        {
            return ShowErrorResponseFormCoreAsync(true, objResultBase, e).GetAwaiter().GetResult();
        }

        public static Task<object> ShowErrorResponseFormAsync(object objResultBase, Exception e = null)
        {
            return ShowErrorResponseFormCoreAsync(false, objResultBase, e);
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName)?.GetValue(src, null);
        }

        private static async Task<object> ShowErrorResponseFormCoreAsync(bool blnSync, object objResultBase, Exception e = null)
        {
            if (objResultBase == null)
                return e;

            ResultBase rb = null;
            try
            {
                rb = new ResultBase
                {
                    ErrorText = (string) GetPropValue(objResultBase, "ErrorText"),
                    MyException = (Exception) GetPropValue(objResultBase, "MyException"),
                    CallSuccess = (bool) GetPropValue(objResultBase, "CallSuccess")
                };
            }
            catch(Exception ex)
            {
                rb = new ResultBase
                {
                    ErrorText = "Could not cast " + objResultBase + " to Resultbase." + Environment.NewLine +
                                Environment.NewLine + ex.Message,
                    MyException = ex
                };
                Log.Warn(ex);
            }
            if (string.IsNullOrEmpty(rb.ErrorText) && rb.MyException == null)
                return rb;
            Log.Warn("SINners WebService returned: " + rb.ErrorText);
            if (blnSync)
                ShowResponseForm();
            else
                await Task.Run(ShowResponseForm);
            void ShowResponseForm()
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    frmSINnerResponse frmSIN = new frmSINnerResponse
                    {
                        TopMost = true
                    };
                    if (rb.ErrorText.Length > 600)
                        rb.ErrorText = rb.ErrorText.Substring(0, 598) + "...";
                    frmSIN.SINnerResponseUI.Result = rb;
                    frmSIN.DoThreadSafe(() =>
                    {
                        Log.Trace("Showing Dialog for frmSINnerResponse()");
                        frmSIN.Show();
                    });
                });
            }
            return rb;
        }


        public static IEnumerable<TreeNode> CharacterRosterTreeNodifyGroupList(IEnumerable<SINnerSearchGroup> groups)
        {
            if (groups == null)
            {
                yield break;
            }

            bool bFoundOneChummer = false;
            bool bBreak = false;
            foreach (SINnerSearchGroup parentlist in groups)
            {
                TreeNode objListNode;
                try
                {
                    objListNode = GetCharacterRosterTreeNodeRecursive(parentlist);
                    if (objListNode.Nodes.Count > 0)
                    {
                        bFoundOneChummer = true;
                        objListNode.Tag = PluginHandler.MyPluginHandlerInstance;
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e, "Could not deserialize CharacterCache-Object: ");
                    objListNode = new TreeNode
                    {
                        Text = "Error loading Char from WebService",
                        Tag = new CharacterCache
                        {
                            ErrorText = e.ToString()
                        }
                    };
                    bBreak = true;
                }
                if (bBreak || objListNode.Nodes.Count > 0)
                {
                    yield return objListNode;
                    if (bBreak)
                        yield break;
                }
            }

            if (!bFoundOneChummer)
            {
                TreeNode node = new TreeNode("Online, but no chummers uploaded")
                {
                    Tag = PluginHandler.MyPluginHandlerInstance,
                    ToolTipText = "To upload a chummer, open it go to the sinners-tabpage and click upload (and wait a bit)."
                };
                Log.Info("Online, but no chummers uploaded");
                yield return node;
            }
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="objCache">Cache from which to generate name.</param>
        /// <returns></returns>
        public static string CalculateCharName(CharacterCache objCache)
        {
            if (objCache == null)
                return string.Empty;
            if (objCache.BuildMethod != "online")
                return objCache.CalculatedName(false);
            string strReturn;
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                strReturn = Path.GetFileNameWithoutExtension(objCache.FileName) + LanguageManager.GetString("String_Space", GlobalSettings.Language) + '(' + LanguageManager.GetString("String_Error", GlobalSettings.Language) + ')';
            }
            else
            {
                strReturn = objCache.CharacterAlias;
                if (string.IsNullOrEmpty(strReturn))
                {
                    strReturn = objCache.CharacterName;
                    if (string.IsNullOrEmpty(strReturn))
                        strReturn = LanguageManager.GetString("String_UnnamedCharacter", GlobalSettings.Language);
                }
                strReturn += " (online)";
            }
            return strReturn;
        }

        private static TreeNode GetCharacterRosterTreeNodeRecursive(SINnerSearchGroup ssg)
        {
            return GetCharacterRosterTreeNodeRecursiveCoreAsync(true, ssg).GetAwaiter().GetResult();
        }

        private static Task<TreeNode> GetCharacterRosterTreeNodeRecursiveAsync(SINnerSearchGroup ssg)
        {
            return GetCharacterRosterTreeNodeRecursiveCoreAsync(false, ssg);
        }

        private static async Task<TreeNode> GetCharacterRosterTreeNodeRecursiveCoreAsync(bool blnSync, SINnerSearchGroup ssg)
        {
            TreeNode objListNode = new TreeNode
            {
                Text = ssg.Groupname,
                Name = ssg.Groupname,
                Tag = ssg
            };
            if (ssg.MyMembers.Count == 0 && ssg.MySINSearchGroups.Count == 0)
            {
                string emptystring = LanguageManager.GetString("String_Empty", GlobalSettings.Language);
                TreeNode empty = new TreeNode()
                {
                    Text = emptystring
                };
                objListNode.Nodes.Add(empty);
            }
            foreach (SINnerSearchGroupMember member in ssg.MyMembers.OrderBy(a => a.Display))
            {
                SINner sinner = member.MySINner;
                sinner.DownloadedFromSINnersTime = DateTime.Now.ToUniversalTime();
                CharacterCache objCache = (blnSync
                                              // ReSharper disable once MethodHasAsyncOverload
                                              ? sinner.GetCharacterCache()
                                              : await sinner.GetCharacterCacheAsync())
                                          ?? new CharacterCache
                                          {
                                              CharacterName = "pending",
                                              CharacterAlias = sinner.Alias,
                                              BuildMethod = "online"
                                          };

                if (objCache.MyPluginDataDic.ContainsKey("IsSINnerFavorite"))
                    objCache.MyPluginDataDic.Remove("IsSINnerFavorite");
                objCache.MyPluginDataDic.Add("IsSINnerFavorite", member.IsFavorite);
                SetEventHandlers(sinner, objCache);
                TreeNode memberNode = new TreeNode
                {
                    Text = CalculateCharName(objCache),
                    Name = CalculateCharName(objCache),
                    Tag = objCache,
                    ToolTipText = "Last Change: " + sinner.LastChange,
                };
                if (string.IsNullOrEmpty(sinner.DownloadUrl))
                {
                    objCache.ErrorText = "File is not uploaded - only metadata available." + Environment.NewLine
                                      + "Please upload this file again from a client," +
                                      Environment.NewLine
                                      + "that has saved a local copy." +
                                      Environment.NewLine + Environment.NewLine
                                      + "Open the character locally, make sure to have \"online mode\"" +
                                      Environment.NewLine
                                      + "selected in option->plugins->sinner and press the \"save\" symbol." +
                                      Environment.NewLine + Environment.NewLine
                                      + "You can delete this entry by selecting it and pressing the \"del\" key.";

                    Log.Warn(objCache.ErrorText);
                }
                TreeNode nodExistingMemberNode = objListNode.Nodes.Find(memberNode.Name, false).FirstOrDefault(x => x.Tag == memberNode.Tag);
                if (nodExistingMemberNode != null)
                {
                    objListNode.Nodes.Remove(nodExistingMemberNode);
                }
                objListNode.Nodes.Add(memberNode);

                if (!string.IsNullOrEmpty(objCache.ErrorText))
                {
                    memberNode.ForeColor = Color.Red;
                    memberNode.ToolTipText += Environment.NewLine + Environment.NewLine +
                                               LanguageManager.GetString("String_Error", GlobalSettings.Language)
                                               + LanguageManager.GetString("String_Colon", GlobalSettings.Language) +
                                               Environment.NewLine + objCache.ErrorText;
                }
            }

            if (ssg.MySINSearchGroups != null)
            {
                foreach (SINnerSearchGroup childssg in ssg.MySINSearchGroups)
                {
                    TreeNode childnode = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? GetCharacterRosterTreeNodeRecursive(childssg)
                        : await GetCharacterRosterTreeNodeRecursiveAsync(childssg);
                    if (childnode != null)
                    {
                        if (!objListNode.Nodes.ContainsKey(childnode.Name))
                            objListNode.Nodes.Add(childnode);
                        else
                        {
                            foreach (TreeNode mergenode in objListNode.Nodes.Find(childnode.Name, false))
                            {
                                foreach (TreeNode what in childnode.Nodes)
                                {
                                    if (mergenode.Nodes.ContainsKey(what.Name))
                                    {
                                        TreeNode nodExistingNode = mergenode.Nodes.Find(what.Name, false).FirstOrDefault(x => x.Tag == what.Tag);
                                        if (nodExistingNode != null)
                                            mergenode.Nodes.Remove(nodExistingNode);
                                        else
                                            mergenode.Nodes.Add(what);
                                    }
                                    else
                                        mergenode.Nodes.Add(what);
                                }
                            }
                        }
                    }
                }
            }

            return objListNode;
        }

        private static async Task<SINnerGroup> CreateGroup(SINnerGroup mygroup)
        {
            try
            {
                SinnersClient client = StaticUtils.GetClient();
                await client.PostGroupAsync(null,mygroup);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.Message);
                throw;
            }
            return null;
        }

        internal static async Task<SINnerGroup> CreateGroupOnClickAsync(SINnerGroup group = null, string groupname = "")
        {
            try
            {
                if (group == null)
                {
                    group = new SINnerGroup(null)
                    {
                        Groupname = groupname,
                        IsPublic = false
                    };
                }


                using (frmSINnerGroupEdit ge = new frmSINnerGroupEdit(group, false))
                {
                    DialogResult result = ge.ShowDialog(Program.MainForm);
                    if (result == DialogResult.OK)
                    {
                        group = ge.MySINnerGroupCreate.MyGroup;
                        try
                        {
                            using (new CursorWait(Program.MainForm))
                            {
                                SINnerGroup a = await CreateGroup(ge.MySINnerGroupCreate.MyGroup);
                                return a;
                                
                            }
                        }
                        catch (Exception exception)
                        {
                            Program.MainForm.ShowMessageBox(exception.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.ToString());
            }
            return null;
        }

        public static byte[] ReadFully(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static async Task<CharacterExtended> UploadCharacterFromFile(string fileName)
        {
            CharacterExtended ce = null;
            Character objCharacter = null;
            try
            {
                Log.Trace("Loading: " + fileName);
                objCharacter = new Character {FileName = fileName};
                using (LoadingBar frmLoadingForm = ChummerMainForm.CreateAndShowProgressBar(Path.GetFileName(fileName), Character.NumLoadingSections))
                {
                    if (!await objCharacter.LoadAsync(frmLoadingForm, false))
                        return null;
                    Log.Trace("Character loaded: " + objCharacter.Name);
                }

                CharacterCache objCache = await CharacterCache.CreateFromFileAsync(fileName);
                ce = new CharacterExtended(objCharacter, null, objCache);
                await ce.Upload();
            }
            catch (Exception ex)
            {
                string msg = "Exception while loading " + fileName + ":" + Environment.NewLine + ex;
                Log.Warn(msg);
                /* run your code here */
                Program.MainForm.ShowMessageBox(msg);
            }
            finally
            {
                if (ce == null)
                    objCharacter?.Dispose();
            }

            return ce;
        }

        private static void SetEventHandlers(SINner sinner, CharacterCache objCache)
        {
            if (sinner == null)
                throw new ArgumentNullException(nameof(sinner));
            if (objCache == null)
                throw new ArgumentNullException(nameof(objCache));
            objCache.MyPluginDataDic.Add("SINnerId", sinner.Id);
            objCache.OnMyDoubleClick = null;
            objCache.OnMyDoubleClick += OnObjCacheOnMyDoubleClick;
            async void OnObjCacheOnMyDoubleClick(object sender, EventArgs e) => await OnMyDoubleClick(sinner, objCache);
            objCache.OnMyAfterSelect = null;
            objCache.OnMyAfterSelect += OnObjCacheOnMyAfterSelect;
            async void OnObjCacheOnMyAfterSelect(object sender, TreeViewEventArgs treeViewEventArgs) => await OnMyAfterSelect(sinner, objCache, treeViewEventArgs);
            objCache.OnMyKeyDown = null;
            objCache.OnMyKeyDown += OnObjCacheOnMyKeyDown;

            async void OnObjCacheOnMyKeyDown(object sender, Tuple<KeyEventArgs, TreeNode> args)
            {
                try
                {
                    using (new CursorWait(PluginHandler.MainForm, true))
                    {
                        if (args.Item1.KeyCode == Keys.Delete)
                        {
                            SinnersClient client = StaticUtils.GetClient();
                            if (sinner.Id != null)
                            {
                                await client.DeleteAsync(sinner.Id.Value).ConfigureAwait(false);
                            }

                            objCache.ErrorText = "deleted!";
                            await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodes(PluginHandler.MyPluginHandlerInstance);
                        }
                    }
                }
                catch (HttpOperationException e)
                {
                    objCache.ErrorText = e.Message;
                    objCache.ErrorText += Environment.NewLine + e.Response.Content;
                    Log.Error(e, e.Response.Content);
                }
                catch (Exception e)
                {
                    objCache.ErrorText = e.Message;
                    Log.Error(e);
                }
            }

            objCache.OnMyContextMenuDeleteClick = null;
            objCache.OnMyContextMenuDeleteClick += OnObjCacheOnMyContextMenuDeleteClick;

            async void OnObjCacheOnMyContextMenuDeleteClick(object sender, EventArgs args)
            {
                try
                {
                    if (sinner.Id == null)
                        return;
                    using (new CursorWait(PluginHandler.MainForm, true))
                    {
                        SinnersClient client = StaticUtils.GetClient();
                        ConfiguredTaskAwaitable<ResultSinnerDelete> res = client.DeleteAsync(sinner.Id.Value).ConfigureAwait(false);
                        if (!((await ShowErrorResponseFormAsync(res)) is ResultGroupGetSearchGroups result))
                            return;
                        if (result.CallSuccess)
                        {
                            objCache.ErrorText = "deleted!";
                            await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodes(PluginHandler.MyPluginHandlerInstance);
                        }
                    }
                }
                catch (HttpOperationException ex)
                {
                    objCache.ErrorText = ex.Message;
                    objCache.ErrorText += Environment.NewLine + ex.Response.Content;
                    Log.Error(ex, objCache.ErrorText);
                }
                catch (Exception ex)
                {
                    objCache.ErrorText = ex.Message;
                    Log.Error(ex);
                }
            }
        }

        private static async Task OnMyAfterSelect(SINner sinner, CharacterCache objCache, TreeViewEventArgs treeViewEventArgs)
        {
            using (new CursorWait(PluginHandler.MainForm, true))
            {
                if (string.IsNullOrEmpty(sinner.FilePath))
                {
                    objCache.FilePath = await DownloadFileTask(sinner, objCache);
                }
                if (!string.IsNullOrEmpty(objCache.FilePath))
                {
                    //I copy the values, because I dont know what callbacks are registered...
                    using (CharacterCache tempCache = new CharacterCache())
                    {
                        if (await tempCache.LoadFromFileAsync(objCache.FilePath))
                        {
                            objCache.CopyFrom(tempCache);
                        }
                    }
                }
                await PluginHandler.MainForm.CharacterRoster.DoThreadSafeAsync(() =>
                {
                    PluginHandler.MainForm.CharacterRoster.UpdateCharacter(objCache);
                });

                treeViewEventArgs.Node.Text = objCache.CalculatedName();
            }
        }

        private static async Task OnMyDoubleClick(SINner sinner, CharacterCache objCache)
        {
            string filepath = await DownloadFileTask(sinner, objCache);
            PluginHandler.MySINnerLoading = sinner;
            PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers(true);
            await PluginHandler.MainForm.DoThreadSafeAsync(() =>
            {
                Character c = PluginHandler.MainForm.LoadCharacter(filepath);
                if (c != null)
                {
                    SwitchToCharacter(c);
                }
                PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers();
                PluginHandler.MySINnerLoading = null;
            });
        }

        private static SINnerVisibility s_objDefaultSINnerVisibility;

        public static SINnerVisibility DefaultSINnerVisibility
        {
            get
            {
                if (s_objDefaultSINnerVisibility == null && !string.IsNullOrEmpty(Settings.Default.SINnerVisibility))
                {
                    try
                    {
                        s_objDefaultSINnerVisibility = JsonConvert.DeserializeObject<SINnerVisibility>(Settings.Default.SINnerVisibility);
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e);
                    }
                }
                return s_objDefaultSINnerVisibility;
            }
            set
            {
                s_objDefaultSINnerVisibility = value;
                Settings.Default.SINnerVisibility = JsonConvert.SerializeObject(value);
                Settings.Default.Save();
            }
        }


        private static void SwitchToCharacter(Character objOpenCharacter)
        {
            using (new CursorWait(PluginHandler.MainForm, true))
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    if (objOpenCharacter == null
                        || !PluginHandler.MainForm.SwitchToOpenCharacter(objOpenCharacter, false))
                    {
                        PluginHandler.MainForm.OpenCharacter(objOpenCharacter, false);
                    }
                });
            }
        }

        private static void SwitchToCharacter(CharacterCache objCache)
        {
            using (new CursorWait(PluginHandler.MainForm, true))
            {
                Character objOpenCharacter = PluginHandler.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FilePath)
                                             ?? PluginHandler.MainForm.LoadCharacter(objCache.FilePath);
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    SwitchToCharacter(objOpenCharacter);
                });
            }
        }

        public static ResultSinnerPostSIN PostSINner(CharacterExtended ce)
        {
            return PostSINnerCoreAsync(true, ce).GetAwaiter().GetResult();
        }

        public static Task<ResultSinnerPostSIN> PostSINnerAsync(CharacterExtended ce)
        {
            return PostSINnerCoreAsync(false, ce);
        }

        private static async Task<ResultSinnerPostSIN> PostSINnerCoreAsync(bool blnSync, CharacterExtended ce)
        {
            if (ce == null)
                throw new ArgumentNullException(nameof(ce));
            ResultSinnerPostSIN res = null;
            try
            {
                UploadInfoObject uploadInfoObject = new UploadInfoObject
                {
                    Client = PluginHandler.MyUploadClient,
                    UploadDateTime = DateTime.Now,
                };
                ce.MySINnerFile.UploadDateTime = DateTime.Now;
                uploadInfoObject.SiNners = new List<SINner>
                {
                    ce.MySINnerFile
                };
                Log.Info("Posting " + ce.MySINnerFile.Id + "...");
                TaskScheduler objUIScheduler = null;
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    Program.MainForm.DoThreadSafe(() =>
                    {
                        objUIScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    });
                SinnersClient client = StaticUtils.GetClient();
                if (!StaticUtils.IsUnitTest)
                {
                    if (blnSync)
                    {
                        Task<ResultSinnerPostSIN> objPostTask = client.PostSINAsync(uploadInfoObject);
                        objPostTask.RunSynchronously(objUIScheduler);
                        res = objPostTask.Result;
                    }
                    else
                        res = await client.PostSINAsync(uploadInfoObject);
                    if (res != null && !res.CallSuccess)
                    {
                        string msg = "Post of " + ce.MyCharacter.Alias + " completed with StatusCode: " + res.CallSuccess;
                        msg += Environment.NewLine + "Reason: " + res.ErrorText;
                        
                        Log.Warn(msg);
                        try
                        {
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverload
                                ShowErrorResponseForm(res);
                            else
                                await ShowErrorResponseFormAsync(res);
                        }
                        catch (Exception e)
                        {
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverload
                                ShowErrorResponseForm(res, e);
                            else
                                await ShowErrorResponseFormAsync(res, e);
                        }
                        return res;
                    }
                }
                else if (blnSync)
                {
                    Task<ResultSinnerPostSIN> objPostTask = client.PostSINAsync(uploadInfoObject);
                    objPostTask.RunSynchronously(objUIScheduler);
                }
                else
                {
                    await client.PostSINAsync(uploadInfoObject);
                }
                Log.Info("Post of " + (ce.MySINnerFile.Id != null
                    ? ce.MySINnerFile.Id.Value.ToString("D", GlobalSettings.InvariantCultureInfo)
                    : string.Empty) + " finished.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            return res;
        }

        public static ResultSINnerPut UploadChummerFile(CharacterExtended ce)
        {
            return UploadChummerFileCoreAsync(true, ce).GetAwaiter().GetResult();
        }

        public static Task<ResultSINnerPut> UploadChummerFileAsync(CharacterExtended ce)
        {
            return UploadChummerFileCoreAsync(false, ce);
        }

        private static async Task<ResultSINnerPut> UploadChummerFileCoreAsync(bool blnSync, CharacterExtended ce)
        {
            if (ce == null)
                throw new ArgumentNullException(nameof(ce));
            ResultSINnerPut res = null;
            try
            {
                if (string.IsNullOrEmpty(ce.ZipFilePath))
                    ce.ZipFilePath = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? ce.PrepareModel()
                        : await ce.PrepareModelAsync();

                using (FileStream fs = new FileStream(ce.ZipFilePath, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        SinnersClient client = StaticUtils.GetClient();
                        if (!StaticUtils.IsUnitTest)
                        {
                            if (ce.MySINnerFile.Id != null)
                            {
                                FileParameter fp = new FileParameter(fs);
                                if (blnSync)
                                {
                                    Task<ResultSINnerPut> objPutTask = client.PutSINAsync(ce.MySINnerFile.Id.Value, fp);
                                    if (objPutTask.Status == TaskStatus.Created)
                                        objPutTask.RunSynchronously();
                                    res = objPutTask.Result;
                                }
                                else
                                    res = await client.PutSINAsync(ce.MySINnerFile.Id.Value, fp);
                            }
                            string msg = "Upload ended with statuscode: ";
                            msg += res?.CallSuccess + Environment.NewLine;
                            msg += res?.ErrorText;

                            Log.Info(msg);
                            //HttpStatusCode myStatus = res?.Response?.StatusCode ?? HttpStatusCode.NotFound;
                            if(!StaticUtils.IsUnitTest)
                            {
                                if (res?.CallSuccess == false)
                                {
                                    Program.MainForm.ShowMessageBox(msg);
                                }
                                using (new CursorWait(PluginHandler.MainForm, true))
                                {
                                    await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodes(PluginHandler.MyPluginHandlerInstance);
                                }
                            }
                        }
                        else
                        {
                            FileParameter fp = new FileParameter(fs);
                            if (blnSync)
                                client.PutSINAsync(ce.MySINnerFile.Id ?? Guid.Empty, fp).Wait();
                            else
                                await client.PutSINAsync(ce.MySINnerFile.Id ?? Guid.Empty, fp);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Program.MainForm.ShowMessageBox(e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            return res;
        }


        public static async Task<string> DownloadFile(SINner sinner, CharacterCache objCache)
        {
            if (sinner == null)
                throw new ArgumentNullException(nameof(sinner));
            if (!sinner.Id.HasValue)
            {
                NullReferenceException e = new NullReferenceException("SINner Id is not set!");
                Log.Error(e);
                if (objCache != null)
                    objCache.ErrorText = e.Message;
                return string.Empty;
            }
            try
            {
                string strIdToUse = sinner.Id.Value.ToString();
                //currently only one chum5-File per chum5z ZipFile is implemented!
                string loadFilePath = string.Empty;
                string zipPath = Path.Combine(Path.GetTempPath(), "SINner", strIdToUse);
                if (Directory.Exists(zipPath))
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        if (File.GetLastWriteTime(file) >= sinner.LastChange)
                        {
                            loadFilePath = file;
                            if (objCache != null)
                                objCache.FilePath = loadFilePath;
                            break;
                        }
                        File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(zipPath);
                }
                Character objOpenCharacter = PluginHandler.MainForm?.OpenCharacters?.FirstOrDefault(x => x.FileName == loadFilePath);
                if (objOpenCharacter != null)
                {
                    return loadFilePath;
                }
                if (string.IsNullOrEmpty(loadFilePath))
                {
                    try
                    {
                        string zippedFile = Path.Combine(Path.GetTempPath(), "SINner", strIdToUse + ".chum5z");
                        if (File.Exists(zippedFile))
                            File.Delete(zippedFile);
                        Exception rethrow = null;
                        try
                        {
                            using (WebClient wc = new WebClient())
                            {
                                await wc.DownloadFileTaskAsync(
                                    // Param1 = Link of file
                                    new Uri(sinner.DownloadUrl),
                                    // Param2 = Path to save
                                    zippedFile
                                );
                            }
                        }
                        catch (Exception e)
                        {
                            rethrow = e;
                            FileInfo fi = new FileInfo(zippedFile);
                            if (!File.Exists(zippedFile) || fi.Length == 0)
                            {
                                SinnersClient client = StaticUtils.GetClient();
                                FileResponse filestream = await client.GetDownloadFileAsync(sinner.Id.Value);
                                if (filestream == null)
                                {
                                    throw new ArgumentNullException(nameof(sinner), "Could not download Sinner " +
                                        sinner.Id.Value + " via client.GetDownloadFileAsync()!");
                                }
                                
                                byte[] array = ReadFully(filestream.Stream);
                                File.WriteAllBytes(zippedFile, array);
                            }
                        }

                        if (!File.Exists(zippedFile) && rethrow != null)
                            throw rethrow;


                        ZipFile.ExtractToDirectory(zippedFile, zipPath);
                        IEnumerable<string> files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                        {
                            if (sinner.UploadDateTime != null)
                            {
                                DateTime origDateTime = new DateTime(sinner.UploadDateTime.Value.Ticks, DateTimeKind.Unspecified);
                                File.SetCreationTime(file, origDateTime);
                            }
                            if (sinner.LastChange != null)
                            {
                                DateTime origDateTime = new DateTime(sinner.LastChange.Ticks, DateTimeKind.Unspecified);
                                File.SetLastWriteTime(file, origDateTime);
                            }
                            loadFilePath = file;
                            if (objCache != null)
                                objCache.FilePath = loadFilePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        if (objCache != null)
                            objCache.ErrorText = ex.Message;
                    }
                }
                return loadFilePath;
            }
            catch (Exception e)
            {
                Log.Error(e);
                if (objCache != null)
                    objCache.ErrorText = e.Message;
                throw;
            }
        }


        private static void wc_DownloadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                string substring = "Downloading: " + e.ProgressPercentage;
                PluginHandler.MainForm.Text = PluginHandler.MainForm.MainTitle + ' ' + substring;
            });
        }


        public static Task<string> DownloadFileTask(SINner sinner, CharacterCache objCache)
        {
            try
            {
                if (objCache?.DownLoadRunning != null && objCache.DownLoadRunning.Status == TaskStatus.Running)
                    return objCache.DownLoadRunning;
                Log.Info("Downloading SINner: " + sinner?.Id);
                Task<string> returntask = Task.Run(async () =>
                {
                    string filepath = await DownloadFile(sinner, objCache);
                    if (objCache != null)
                        objCache.FilePath = filepath;
                    return filepath;
                });
                if (objCache != null)
                    objCache.DownLoadRunning = returntask;
                return returntask;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error downloading sinner " + sinner?.Id + ": ");
                if (objCache != null)
                    objCache.ErrorText = ex.ToString();
                throw;
            }
        }
    }
}
