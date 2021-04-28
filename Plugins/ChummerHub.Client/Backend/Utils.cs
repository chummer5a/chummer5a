using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Sinners;
using ChummerHub.Client.Properties;
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static System.Type GetListType(object someList)
        {
            if (someList == null)
                throw new ArgumentNullException(nameof(someList));
            System.Type result;
            var type = someList.GetType();

            if (!type.IsGenericType)
                throw new ArgumentException("Type must be List<>, but was " + type.FullName, nameof(someList));
            try
            {
                result = type.GetGenericArguments()[0];
            }
            catch (Exception e)
            {
                var ex = new ArgumentException("Type must be List<>, but was " + type.FullName, nameof(someList), e);
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
                    using (new CursorWait())
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
                            Thread.Sleep(100);
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
                    using (new CursorWait())
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
                            Thread.Sleep(100);
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
        public static extern bool InternetGetCookieEx(
                                        string url,
                                        string cookieName,
                                        StringBuilder cookieData,
                                        ref int size,
                                        int dwFlags,
                                        System.IntPtr lpReserved);

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
                if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, System.IntPtr.Zero))
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
                        System.IntPtr.Zero))
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
                if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, System.IntPtr.Zero))
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
                        System.IntPtr.Zero))
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

        private static MySinnersClient _clientTask;

        private static MySinnersClient _client;
        public static MySinnersClient GetClient(bool reset = false)
        {
            if (reset)
            {
                _client = null;
                _clientNOTworking = false;
                _clientTask = null;
            }
            if (_client == null)
            {
                if (_clientNOTworking.HasValue == false || _clientNOTworking == false)
                {
                    _client = GetSINnersClient();
                    if (_client != null)
                        _clientNOTworking = false;
                }
            }
            return _client;
        }

        private static MySinnersClient GetSINnersClient()
        {
            MySinnersClient client = null;
            try
            {
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(frmChummerMain));
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
                ServiceClientCredentials credentials = null;
                try
                {
                    credentials = new MyCredentials();
                }
                catch (Exception e)
                {
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    TelemetryClient ct = new TelemetryClient();
                    ct.TrackException(et);
                    Log.Error(e);
                }

                DelegatingHandler delegatingHandler = new MyMessageHandler();
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(delegatingHandler);
                var temp = AuthorizationCookieContainer;
                if (temp != null)
                    httpClientHandler.CookieContainer = temp;
                client = new MySinnersClient(baseUri.ToString(), httpClient);
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
                using (var client = new HttpClient())
                {
                    var uri = new Uri(callback);
                    string baseuri = uri.GetLeftPart(UriPartial.Authority);
                    client.BaseAddress = new Uri(baseuri);
                    using (var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("Progress", progress.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("Text", text)
                    }))
                    {
                        var result = await client.PostAsync(uri, content).ConfigureAwait(true);
                        string resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(true);
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    MyTreeNodeList.Clear();
                });

                ResultAccountGetSinnersByAuthorization response = null;
                try
                {
                    response = await myGetSINnersFunction().ConfigureAwait(true);
                }
                catch(SerializationException e)
                {
                    if (e.Content.Contains("Log in - ChummerHub"))
                    {
                        TreeNode node = new TreeNode("Online, not logged in")
                        {
                            ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login"
                        };
                        Log.Warn(e, "Online, not logged in");
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MyTreeNodeList.Add(node);
                        });
                    }
                    else
                    {
                        string msg = "Could not load response from SINners:" + Environment.NewLine;
                        msg += e.Message;
                        if (e.InnerException != null)
                        {
                            msg += Environment.NewLine + e.InnerException.Message;
                        }
                        Log.Error(e, msg);
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MyTreeNodeList.Add(new TreeNode
                            {
                                ToolTipText = msg
                            });
                        });
                    }
                    return MyTreeNodeList;
                }
                if (response == null || response.CallSuccess == false)
                {
                    string msg = "Could not load online Sinners: " + response.ErrorText;
                    
                    Log.Warn(msg);
                    var errornode = new TreeNode
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
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        MyTreeNodeList.Add(errornode);
                    });
                    return MyTreeNodeList;
                }

                ResultAccountGetSinnersByAuthorization res = response;
                var result = res.MySINSearchGroupResult;
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
        }

        public static async Task<object> HandleError(Object resultBase)
        {
            return await HandleError(resultBase, null).ConfigureAwait(true);
        }

        public static async Task<object> HandleError(Exception e)
        {
            ResultBase rb = new ResultBase
            {
                ErrorText = e?.Message,
                MyException = e,
                CallSuccess = false
            };
            if (!string.IsNullOrEmpty(rb.ErrorText) || rb.MyException != null)
            {
                using (var frmSIN = new frmSINnerResponse
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
                    }
                    else
                    {
                        Log.Error(e, "Response from SINners WebService: ");
                    }

                    frmSIN.ShowDialog(PluginHandler.MainForm);
                }
            }
            return rb;
        }


        public static async Task<object> HandleError(Object objResultBase, Exception e)
        {
            if (objResultBase == null)
                return e;
            ResultBase rb = (ResultBase)objResultBase;
            
            if (!string.IsNullOrEmpty(rb?.ErrorText) || rb?.MyException != null)
            {
                Log.Warn("SINners WebService returned: " + rb.ErrorText);
                await Task.Run(() =>
                {
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        var frmSIN = new frmSINnerResponse
                        {
                            TopMost = true
                        };
                        if (rb?.ErrorText.Length > 600)
                            rb.ErrorText = rb.ErrorText.Substring(0, 598) + "...";
                        frmSIN.SINnerResponseUI.Result = rb;
                        frmSIN.DoThreadSafe(() =>
                        {
                            Log.Trace("Showing Dialog for frmSINnerResponse()");
                            frmSIN.Show();
                        });
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
            foreach (var parentlist in groups)
            {
                TreeNode objListNode;
                try
                {
                    //list.SiNner.DownloadedFromSINnersTime = DateTime.Now;
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
                strReturn = Path.GetFileNameWithoutExtension(objCache.FileName) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + LanguageManager.GetString("String_Error", GlobalOptions.Language) + ')';
            }
            else
            {
                strReturn = objCache.CharacterAlias;
                if (string.IsNullOrEmpty(strReturn))
                {
                    strReturn = objCache.CharacterName;
                    if (string.IsNullOrEmpty(strReturn))
                        strReturn = LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language);
                }
                strReturn += " (online)";
            }
            return strReturn;
        }

        private static TreeNode GetCharacterRosterTreeNodeRecursive(SINnerSearchGroup ssg)
        {
            TreeNode objListNode = new TreeNode
            {
                Text = ssg.Groupname,
                Name = ssg.Groupname,
                Tag = ssg
            };
            foreach (var member in ssg.MyMembers.OrderBy(a => a.Display))
            {
                var sinner = member.MySINner;
                sinner.DownloadedFromSINnersTime = DateTime.Now.ToUniversalTime();
                CharacterCache objCache = sinner.GetCharacterCache()
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
                                      + "selected in option->plugins->sinner and press the \"save\" symbol.";

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
                                               LanguageManager.GetString("String_Error", GlobalOptions.Language)
                                               + LanguageManager.GetString("String_Colon", GlobalOptions.Language) +
                                               Environment.NewLine + objCache.ErrorText;
                }
            }

            if (ssg.MySINSearchGroups != null)
            {
                foreach (var childssg in ssg.MySINSearchGroups)
                {
                    var childnode = GetCharacterRosterTreeNodeRecursive(childssg);
                    if (childnode != null)
                    {
                        if (!objListNode.Nodes.ContainsKey(childnode.Name))
                            objListNode.Nodes.Add(childnode);
                        else
                        {
                            foreach (var mergenode in objListNode.Nodes.Find(childnode.Name, false))
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
                using (frmLoading frmLoadingForm = new frmLoading { CharacterFile = fileName })
                {
                    frmLoadingForm.Reset(36);
                    frmLoadingForm.Show();
                    if (!await objCharacter.Load(frmLoadingForm, false).ConfigureAwait(true))
                        return null;
                    Log.Trace("Character loaded: " + objCharacter.Name);
                }

                ce = new CharacterExtended(objCharacter, null, null, new CharacterCache(fileName));
                await ce.Upload().ConfigureAwait(true);
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
            objCache.OnMyDoubleClick += (sender, e) => OnMyDoubleClick(sinner, objCache);
            objCache.OnMyAfterSelect = null;
            objCache.OnMyAfterSelect += (sender, treeViewEventArgs) => OnMyAfterSelect(sinner, objCache, treeViewEventArgs);
            objCache.OnMyKeyDown = null;
            objCache.OnMyKeyDown += async (sender, args) =>
            {
                try
                {
                    using (new CursorWait(PluginHandler.MainForm, true))
                    {
                        if (args.Item1.KeyCode == Keys.Delete)
                        {
                            var client = StaticUtils.GetClient();
                            if (sinner.Id != null)
                            {
                                await client.DeleteAsync(sinner.Id.Value).ConfigureAwait(false);
                            }
                            objCache.ErrorText = "deleted!";
                            PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                            {
                                PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false);
                            });
                        }
                    }
                }
                catch(HttpOperationException e)
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
            };

            objCache.OnMyContextMenuDeleteClick = null;
            objCache.OnMyContextMenuDeleteClick += async (sender, args) =>
            {
                try
                {
                    if (sinner.Id == null)
                        return;
                    using (new CursorWait(PluginHandler.MainForm, true))
                    {
                        var client = StaticUtils.GetClient();
                        var res = await client.DeleteAsync(sinner.Id.Value).ConfigureAwait(true); ;
                        if (!(await ChummerHub.Client.Backend.Utils.HandleError(res).ConfigureAwait(true) is ResultGroupGetSearchGroups result))
                            return;
                        if (result.CallSuccess == true)
                        {
                            objCache.ErrorText = "deleted!";
                            PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                            {
                                PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false);
                            });
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
            };
        }

        private static async void OnMyAfterSelect(SINner sinner, CharacterCache objCache, TreeViewEventArgs treeViewEventArgs)
        {
            using (new CursorWait(PluginHandler.MainForm, true))
            {
                if (string.IsNullOrEmpty(sinner.FilePath))
                {
                    objCache.FilePath = await DownloadFileTask(sinner, objCache).ConfigureAwait(true);
                }
                if (!string.IsNullOrEmpty(objCache.FilePath))
                {
                    //I copy the values, because I dont know what callbacks are registered...
                    var tempCache = new CharacterCache(objCache.FilePath);
                    objCache.Background = tempCache.Background;
                    objCache.MugshotBase64 = tempCache.MugshotBase64;
                    objCache.BuildMethod = tempCache.BuildMethod;
                    objCache.CharacterAlias = tempCache.CharacterAlias;
                    objCache.CharacterName = tempCache.CharacterName;
                    objCache.CharacterNotes = tempCache.CharacterNotes;
                    objCache.Concept = tempCache.Concept;
                    objCache.Created = tempCache.Created;
                    objCache.Description = tempCache.Description;
                    objCache.Essence = tempCache.Essence;
                    objCache.GameNotes = tempCache.GameNotes;
                    objCache.Karma = tempCache.Karma;
                    objCache.FileName = tempCache.FileName;
                    objCache.Metatype = tempCache.Metatype;
                    objCache.Metavariant = tempCache.Metavariant;
                    objCache.PlayerName = tempCache.PlayerName;
                    objCache.SettingsFile = tempCache.SettingsFile;
                }
                PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                {
                    PluginHandler.MainForm.CharacterRoster.UpdateCharacter(objCache);
                });

                treeViewEventArgs.Node.Text = objCache.CalculatedName();
            }
        }

        private static async void OnMyDoubleClick(SINner sinner, CharacterCache objCache)
        {
            string filepath = await DownloadFileTask(sinner, objCache).ConfigureAwait(true);
            PluginHandler.MySINnerLoading = sinner;
            PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers(true);
            PluginHandler.MainForm.DoThreadSafe(async () =>
            {
                Character c = await PluginHandler.MainForm.LoadCharacter(filepath).ConfigureAwait(true);
                if (c != null)
                {
                    SwitchToCharacter(c);
                }
                PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers();
                PluginHandler.MySINnerLoading = null;
            });
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

        private static async void SwitchToCharacter(CharacterCache objCache)
        {
            using (new CursorWait(PluginHandler.MainForm, true))
            {
                PluginHandler.MainForm.DoThreadSafe(async () =>
                {
                    Character objOpenCharacter = PluginHandler.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FilePath)
                                                 ?? await PluginHandler.MainForm.LoadCharacter(objCache.FilePath).ConfigureAwait(true);
                    SwitchToCharacter(objOpenCharacter);
                });
            }
        }

        public static async Task<ResultSinnerPostSIN> PostSINnerAsync(CharacterExtended ce)
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
                var client = StaticUtils.GetClient();
                if (!StaticUtils.IsUnitTest)
                {
                    res = await client.PostSINAsync(uploadInfoObject).ConfigureAwait(true);
                    if (res != null && res.CallSuccess == false)
                    {
                        var msg = "Post of " + ce.MyCharacter.Alias + " completed with StatusCode: " + res.CallSuccess;
                        msg += Environment.NewLine + "Reason: " + res.ErrorText;
                        
                        Log.Warn(msg);
                        try
                        {
                            await HandleError(res).ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            await HandleError(res, e).ConfigureAwait(true);
                        }

                        return res;
                    }
                }
                else
                {
                    var res2 = await client.PostSINAsync(uploadInfoObject).ConfigureAwait(true);
                    {
                    }
                }
                Log.Info("Post of " + (ce.MySINnerFile.Id != null
                    ? ce.MySINnerFile.Id.Value.ToString("D", GlobalOptions.InvariantCultureInfo)
                    : string.Empty) + " finished.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            return res;
        }

        public static async Task<ResultSinnerPostSIN> PostSINner(CharacterExtended ce)
        {
            if (ce == null)
                throw new ArgumentNullException(nameof(ce));
            ResultSinnerPostSIN res = null;
            try
            {
                UploadInfoObject uploadInfo = new UploadInfoObject
                {
                    Client = PluginHandler.MyUploadClient,
                    UploadDateTime = DateTime.Now
                };
                ce.MySINnerFile.UploadDateTime = DateTime.Now;
                uploadInfo.SiNners = new List<SINner>
                {
                    ce.MySINnerFile
                };
                Log.Info("Posting " + ce.MySINnerFile.Id + "...");
                // This line must be called in UI thread to get correct scheduler
                TaskScheduler scheduler = null;
                Program.MainForm.DoThreadSafe(() =>
                {
                    scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                });

                // this can be called anywhere
                var task = new Task(() =>
                {
                    var client = StaticUtils.GetClient();
                    res = client.PostSINAsync(uploadInfo).Result;
                });

                // also can be called anywhere. Task  will be scheduled for execution.
                // And *IF I'm not mistaken* can be (or even will be executed synchronously)
                // if this call is made from GUI thread. (to be checked)
                task.Start(scheduler);
                //Program.MainForm.DoThreadSafe(async () =>
                //{
                //    var client = StaticUtils.GetClient();
                //    res = await client.PostSINWithHttpMessagesAsync(uploadInfo);
                //});
                task.Wait();
                //res = client.PostSIN(uploadInfo);
                Log.Info("Post of " + ce.MySINnerFile.Id + " finished.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            return res;
        }



        public static async Task<ResultSINnerPut> UploadChummerFileAsync(CharacterExtended ce)
        {
            if (ce == null)
                throw new ArgumentNullException(nameof(ce));
            ResultSINnerPut res = null;
            try
            {
                if (string.IsNullOrEmpty(ce.ZipFilePath))
                    ce.ZipFilePath = await ce.PrepareModel().ConfigureAwait(true);

                using (FileStream fs = new FileStream(ce.ZipFilePath, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        var client = StaticUtils.GetClient();
                        if (!StaticUtils.IsUnitTest)
                        {
                            if (ce.MySINnerFile.Id != null)
                            {
                                FileParameter fp = new FileParameter(fs);
                                res = await client.PutSINAsync(ce.MySINnerFile.Id.Value, fp).ConfigureAwait(true);
                            }
                            string msg = "Upload ended with statuscode: ";
                            msg += res?.CallSuccess + Environment.NewLine;
                            msg += res?.ErrorText;
                            Log.Info(msg);
                            //HttpStatusCode myStatus = res?.Response?.StatusCode ?? HttpStatusCode.NotFound;
                            if(!StaticUtils.IsUnitTest)
                            {
                                if (res.CallSuccess == false)
                                {
                                    Program.MainForm.ShowMessageBox(msg);
                                }
                                using (new CursorWait(PluginHandler.MainForm, true))
                                {
                                    PluginHandler.MainForm.DoThreadSafe(() =>
                                    {
                                        PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false);
                                    });
                                }
                            }
                        }
                        else
                        {
                            FileParameter fp = new FileParameter(fs);
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

        public static ResultSINnerPut UploadChummer(CharacterExtended ce)
        {
            if (ce == null)
                throw new ArgumentNullException(nameof(ce));
            ResultSINnerPut res = null;
            try
            {
                if (string.IsNullOrEmpty(ce.ZipFilePath))
                {
                    ce.ZipFilePath = ce.PrepareModel().Result;
                }

                using (FileStream fs = new FileStream(ce.ZipFilePath, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        var client = StaticUtils.GetClient();
                        if (!StaticUtils.IsUnitTest)
                        {
                            FileParameter fp = new FileParameter(fs);
                            res = client.PutSINAsync(ce.MySINnerFile.Id ?? Guid.Empty, fp).Result;
                            string msg = "Upload ended with statuscode: ";
                            if (res != null)
                            {
                                msg += res.CallSuccess + Environment.NewLine;
                                msg += res.ErrorText;

                                if (!StaticUtils.IsUnitTest)
                                {
                                    using (new CursorWait(PluginHandler.MainForm, true))
                                    {
                                        PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                                        {
                                            PluginHandler.MainForm.CharacterRoster.LoadCharacters(false,
                                                false, false);
                                        });
                                    }
                                }
                            }

                            Log.Info(msg);
                        }
                        else
                        {
                            FileParameter fp = new FileParameter(fs);
                            client.PutSINAsync(ce.MySINnerFile.Id ?? Guid.Empty, fp);
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
                    var files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        if (sinner.LastChange == null || File.GetLastWriteTime(file) >= sinner.LastChange)
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
                                wc.DownloadFile(
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
                            if (!File.Exists(zippedFile))
                            {
                                var client = StaticUtils.GetClient();
                                var filestream = await client.GetDownloadFileAsync(sinner.Id.Value);
                                if (filestream == null)
                                {
                                    throw new ArgumentNullException(nameof(sinner), "Could not download Sinner " +
                                        sinner.Id.Value + " via client.GetDownloadFileAsync()!");
                                }
                                
                                var array = ReadFully(filestream.Stream);
                                File.WriteAllBytes(zippedFile, array);
                            }
                        }

                        if (!File.Exists(zippedFile))
                        {
                            if (rethrow != null)
                                throw rethrow;
                        }


                        ZipFile.ExtractToDirectory(zippedFile, zipPath);
                        var files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                        foreach (var file in files)
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


        private static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                string substring = "Downloading: " + e.ProgressPercentage;
                PluginHandler.MainForm.Text = PluginHandler.MainForm.MainTitle + " " + substring;
            });
        }


        public static Task<string> DownloadFileTask(SINner sinner, CharacterCache objCache)
        {
            try
            {
                if (objCache?.DownLoadRunning != null && objCache.DownLoadRunning.Status == TaskStatus.Running)
                    return objCache.DownLoadRunning;
                Log.Info("Downloading SINner: " + sinner?.Id);
                Task<string> returntask = Task.Run(() =>
                {
                    string filepath = DownloadFile(sinner, objCache).Result;
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
