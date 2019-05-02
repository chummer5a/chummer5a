using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Model;
using Microsoft.Rest;
using SINners;
using SINners.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.UI;
using Newtonsoft.Json;
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

        private static List<string> _userRoles = null;
        public static List<string> UserRoles
        {
            get
            {
                if (_userRoles == null)
                {
                    using (new CursorWait(false))
                    {
                        int counter = 0;
                        //just wait until the task from the startup finishes...
                        while (_userRoles == null)
                        {
                            counter++;
                            if (counter > 10 * 5)
                            {
                                _userRoles = new List<string>() { "none" };
                                break;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                return _userRoles;
            }
            set
            {
                _userRoles = value;
            }
        }

        private static List<string> _possibleRoles = null;
        public static List<string> PossibleRoles
        {
            get
            {
                if (_possibleRoles == null)
                {
                    using (new CursorWait(false))
                    {
                        int counter = 0;
                        //just wait until the task from the startup finishes...
                        while (_possibleRoles == null)
                        {
                            counter++;
                            if (counter > 10 * 5)
                            {
                                _possibleRoles = new List<string>() { "none" };
                                break;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                return _possibleRoles;
            }
            set
            {
                _possibleRoles = value;
            }
        }

        public static CookieContainer AuthorizationCookieContainer
        {
            get
            {
                //Properties.Settings.Default.Reload();
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
                    Uri uri = new Uri(Properties.Settings.Default.SINnerUrl);
                    DeleteUriCookieData(uri);
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
            if (cookieData == null)
                return null;
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

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

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


        /// <summary>
        /// Gets the actual Cookie data
        /// </summary>
        public static bool DeleteUriCookieData(Uri uri)
        {
            Cookie temp1 = new Cookie("KEY1", "VALUE1", "/Path/To/My/App", "/");
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if(!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if(datasize < 0)
                    return false;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if(!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return false;
            }
            if (!InternetSetCookie(uri.ToString(), null, ""))
            {
                return false;
            }

            return true;
        }
        

        private static bool clientErrorShown = false;

        private static bool? _clientNOTworking = null;

        private static SINnersClient _clientTask = null;

        private static SINnersClient _client = null;
        public static SINnersClient GetClient(bool reset = false)
        {
            if (reset)
            {
                _client = null;
                _clientNOTworking = false;
                _clientTask = null;
            }
            if (_client == null)
            {
                if (_clientNOTworking.HasValue == false ||
                    (_clientNOTworking == false))
                {
                    { 
                        _client = GetSINnersClient();
                        if (_client != null)
                            _clientNOTworking = false;
                    }
                }
            }
            return _client;
        }

        private static SINnersClient GetSINnersClient()
        {
            SINnersClient client = null;
            try
            {
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(frmChummerMain));
                if (assembly.GetName().Version.Build == 0)
                {
                    Properties.Settings.Default.SINnerUrl = "https://sinners.azurewebsites.net";
                }
                else
                {
                    Properties.Settings.Default.SINnerUrl = "https://sinners-beta.azurewebsites.net";
                }
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    try
                    {

                        string local = "http://localhost:5000/";
                        var request = WebRequest.Create("http://localhost:5000/");
                        WebResponse response = request.GetResponse();
                        Properties.Settings.Default.SINnerUrl = local;
                        System.Diagnostics.Trace.TraceInformation("Connected to " + local + ".");
                    }
                    catch (Exception e)
                    {
                        Properties.Settings.Default.SINnerUrl = "https://sinners-beta.azurewebsites.net";
                        System.Diagnostics.Trace.TraceInformation("Connected to " + Properties.Settings.Default.SINnerUrl + ".");
                    }
                }
                ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                Uri baseUri = new Uri(Properties.Settings.Default.SINnerUrl);
                Microsoft.Rest.ServiceClientCredentials credentials = new MyCredentials();
                DelegatingHandler delegatingHandler = new MyMessageHandler();
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.CookieContainer = AuthorizationCookieContainer;
                client = new SINnersClient(baseUri, credentials, httpClientHandler, delegatingHandler);
                //double waitTime = 30;
                //var resptask = client.GetVersionWithHttpMessagesAsync().CancelAfter((int)(1000 * waitTime));
                //resptask.ContinueWith((respresult) =>
                //    {
                //        try
                //        {
                //            if (respresult.IsCanceled)
                //            {
                //                System.Diagnostics.Trace.TraceInformation("Could not connected to SINners in " + waitTime + " seconds.");
                //                if (_clientNOTworking.HasValue == false)
                //                    _clientNOTworking = true;
                //                return;
                //            }
                //            var verresp = respresult.Result;
                //            if (verresp.Response.StatusCode == HttpStatusCode.OK)
                //                System.Diagnostics.Trace.TraceInformation("Connected to SINners in version " + verresp.Body.AssemblyVersion + ".");
                //            else if (verresp.Response.StatusCode == HttpStatusCode.Forbidden)
                //            {
                //                if (_clientNOTworking.HasValue == false)
                //                    _clientNOTworking = true;
                //                throw new System.Web.HttpException(403, "WebService disabled by Admin!");
                //            }
                //            _clientNOTworking = false;
                //        }
                //        catch (Exception e)
                //        {
                //            System.Diagnostics.Trace.TraceError(e.ToString());
                //            Console.WriteLine(e);
                //            throw;
                //        }

                //    });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                if (!clientErrorShown)
                {
                    clientErrorShown = true;
                    Exception inner = ex;
                    while (inner.InnerException != null)
                        inner = inner.InnerException;
                    string msg = "Error connecting to SINners: " + Environment.NewLine;
                    msg += "(the complete error description is copied to clipboard)" + Environment.NewLine + Environment.NewLine + inner.ToString();
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        System.Windows.Forms.Clipboard.SetText(ex.ToString());
                    });
                    msg += Environment.NewLine + Environment.NewLine + "Please check the Plugin-Options dialog.";
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                }
            }
            return client;
        }
    }

    public class Utils
    {
       

        public Utils()
        {
            IsUnitTest = false;
        }

        public bool IsUnitTest { get; set; }

        //private static TreeNode MyOnlineTreeNode { get; set; }

        private static List<TreeNode> MyTreeNodeList { get; set; }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        internal async static Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(bool forceUpdate, Func<Task<HttpOperationResponse<ResultAccountGetSinnersByAuthorization>>> myGetSINnersFunction)
        {
            if ((MyTreeNodeList != null) && !forceUpdate)
                return MyTreeNodeList;
            MyTreeNodeList = new List<TreeNode>();
            try
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    MyTreeNodeList.Clear();
                });

                HttpOperationResponse<ResultAccountGetSinnersByAuthorization> response = null;
                try
                {
                    response = await myGetSINnersFunction();
                }
                catch(Microsoft.Rest.SerializationException e)
                {
                    if (e.Content.Contains("Log in - ChummerHub"))
                    {
                        TreeNode node = new TreeNode("Online, not logged in")
                        {
                            ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login"
                        };
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MyTreeNodeList.Add(node);
                        });
                        return MyTreeNodeList;
                    }
                    else
                    {
                        string msg = "Could not load response from SINners:" + Environment.NewLine;
                        msg += e.Message;
                        if (e.InnerException != null)
                        {
                            msg += Environment.NewLine + e.InnerException.Message;
                        }
                        System.Diagnostics.Trace.TraceWarning(msg);
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MyTreeNodeList.Add(new TreeNode()
                            {
                                ToolTipText = msg
                            });
                        });
                        return MyTreeNodeList;
                    }
                }
                if (response == null || (response.Response.StatusCode == HttpStatusCode.BadRequest))
                {
                    string msg = "Could not load online Sinners: " + response?.Response.ReasonPhrase;
                    if (response != null)
                    {
                        var content = await response.Response.Content.ReadAsStringAsync();
                        msg += Environment.NewLine + "Content: " + content;
                    }
                    System.Diagnostics.Trace.TraceWarning(msg);
                   
                    var errornode = new TreeNode()
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
                            System.Windows.Forms.Clipboard.SetText(msg);
                        });
                    };
                    errornode.Tag = errorCache;
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                            MyTreeNodeList.Add(errornode);
                    });
                    return MyTreeNodeList;
                }

                ResultAccountGetSinnersByAuthorization res = response.Body as ResultAccountGetSinnersByAuthorization;
                var result = res.MySINSearchGroupResult;
                if (result?.Roles != null)
                    StaticUtils.UserRoles = result.Roles?.ToList();
                System.Diagnostics.Trace.TraceInformation("Connected to SINners in version " + result?.Version?.AssemblyVersion + ".");
                var groups = result?.SinGroups.ToList();
                MyTreeNodeList = CharacterRosterTreeNodifyGroupList(groups);
                
                return MyTreeNodeList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

        public static async Task<object> HandleError(HttpOperationResponse response)
        {
            return await HandleError(response, null);
        }

        public static async Task<object> HandleError(Exception e)
        {
            ResultBase rb = new ResultBase();
            rb.ErrorText = e.Message;
            rb.MyException = e;
            rb.CallSuccess = false;
            if ((!String.IsNullOrEmpty(rb.ErrorText)
                 || (rb.MyException != null)))
            {
                var frmSIN = new frmSINnerResponse();
                frmSIN.SINnerResponseUI.Result = rb;
                frmSIN.TopMost = true;
                frmSIN.ShowDialog(PluginHandler.MainForm);
            }
            return rb;
        }

        public static async Task<object> HandleError(HttpOperationResponse response,
            object ResponseBody)
        {
            ResultBase rb = null;
            string content = "not set";
            try
            {
                if (ResponseBody == null)
                {
                    content = await response.Response.Content.ReadAsStringAsync();
                    rb = Newtonsoft.Json.JsonConvert.DeserializeObject<ResultBase>(content);
                    ResponseBody = rb;
                }
            }
            catch (Exception e)
            {
                rb = new ResultBase();
                rb.ErrorText = content;
                rb.MyException = e;
                rb.CallSuccess = false;
                ResponseBody = rb;
            }

            try
            {
                if (ResponseBody != null)
                {
                    content = Newtonsoft.Json.JsonConvert.SerializeObject(ResponseBody);
                    rb = Newtonsoft.Json.JsonConvert.DeserializeObject<ResultBase>(content);
                }
            }
            catch (Exception e)
            {
                rb = new ResultBase();
                rb.ErrorText = content;
                rb.MyException = e;
                rb.CallSuccess = false;
                ResponseBody = rb;
            }


            if ((!String.IsNullOrEmpty(rb.ErrorText)
                     || (rb.MyException != null)))
            {
                var frmSIN = new frmSINnerResponse();
                frmSIN.SINnerResponseUI.Result = rb;
                frmSIN.TopMost = true;
                frmSIN.ShowDialog(PluginHandler.MainForm);
            }
            return ResponseBody;
        }


        public static List<TreeNode> CharacterRosterTreeNodifyGroupList(List<SINnerSearchGroup> groups)
        {
            if (groups == null)
            {
                return MyTreeNodeList;
            }

            foreach (var parentlist in groups)
            {
                try
                {
                    //list.SiNner.DownloadedFromSINnersTime = DateTime.Now;
                    var objListNode = GetCharacterRosterTreeNodeRecursive(parentlist);

                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        if (objListNode != null)
                            MyTreeNodeList.Add(objListNode);
                        else
                        {
                            TreeNode node = new TreeNode("no owned chummers found")
                            {
                                Tag = "",
                                Name = "no owned chummers found",
                                ToolTipText =
                                    "To upload a chummer go into the sinners-tabpage after opening a character and press upload (and wait a bit)."
                            };
                            MyTreeNodeList.Add(node);
                        }
                    });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not deserialize CharacterCache-Object: " + e.ToString());
                    TreeNode errorNode = new TreeNode()
                    {
                        Text = "Error loading Char from WebService"
                    };
                    var objCache = new CharacterCache();
                    objCache.ErrorText = e.ToString();
                    errorNode.Tag = objCache;
                    return new List<TreeNode>() {errorNode};
                }
            }

            if (MyTreeNodeList.Count == 0)
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    TreeNode node = new TreeNode("Online, but no chummers uploaded");
                    node.Tag = node.Text;
                    node.ToolTipText =
                        "To upload a chummer, open it go to the sinners-tabpage and click upload (and wait a bit).";
                    MyTreeNodeList.Add(node);
                });
            }

            return MyTreeNodeList;
        }

        private static TreeNode GetCharacterRosterTreeNodeRecursive(SINnerSearchGroup ssg)
        {
            TreeNode objListNode = new TreeNode
            {
                Text = ssg.Groupname,
                Name = ssg.Groupname
            };
            var mlist = (from a in ssg.MyMembers orderby a.Display select a).ToList();
            foreach (var member in mlist)
            {
                var sinner = member.MySINner;
                sinner.DownloadedFromSINnersTime = DateTime.Now;
                CharacterCache objCache = null;
                if (!String.IsNullOrEmpty(sinner?.MyExtendedAttributes?.JsonSummary))
                {
                    objCache =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterCache>(sinner.MyExtendedAttributes
                            .JsonSummary);
                }
                else
                {
                    objCache = new CharacterCache
                    {
                        CharacterName = "pending",
                        CharacterAlias = sinner.Alias,
                        BuildMethod = "online"
                    };
                }
                SetEventHandlers(sinner, objCache);
                TreeNode memberNode = new TreeNode
                {
                    
                    Text = objCache.CalculatedName(),
                    Name = objCache.CalculatedName(),
                    Tag = objCache,
                    ToolTipText = "Last Change: " + sinner.LastChange,
                    ContextMenuStrip = PluginHandler.MainForm.CharacterRoster.ContextMenuStrip
                };
                if (String.IsNullOrEmpty(sinner.DownloadUrl))
                {
                    objCache.ErrorText = "File is not uploaded - only metadata available." + Environment.NewLine
                                      + "Please upload this file again from a client," +
                                      Environment.NewLine
                                      + "that has saved a local copy." +
                                      Environment.NewLine + Environment.NewLine
                                      + "Open the character locally, make sure to have \"online mode\"" +
                                      Environment.NewLine
                                      + "selected in option->plugins->sinner and press the \"save\" symbol.";

                }
                var foundseq = (from a in objListNode.Nodes.Find(memberNode.Name, false) where a.Tag == memberNode.Tag select a).ToList();
                if (foundseq.Any())
                {
                    objListNode.Nodes.Remove(foundseq.FirstOrDefault());
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

                //CharacterCache delObj;
                //if (ssg.MySINSearchGroups != null)
                //{
                //    foreach (var childlist in ssg.MySINSearchGroups)
                //    {
                //        var childnode = GetCharacterRosterTreeNodeRecursive(childlist);
                //        if (childnode != null)
                //        {
                //            var found1seq = (from a in objListNode.Nodes.Find(childnode.Name, false) where a.Tag == childnode.Tag select a).ToList();
                //            if (found1seq.Any())
                //            {
                //                objListNode.Nodes.Remove(found1seq.FirstOrDefault());
                //            }
                //            objListNode.Nodes.Add(childnode);
                //        }
                //    }
                //}
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
                            var list = objListNode.Nodes.Find(childnode.Name, false).ToList();
                            foreach (var mergenode in list)
                            {
                                foreach (TreeNode what in childnode.Nodes)
                                {
                                    if (mergenode.Nodes.ContainsKey(what.Name))
                                    {
                                        var compare = mergenode.Nodes.Find(what.Name, false);
                                        bool found = false;
                                        foreach (var singlecompare in compare)
                                        {
                                            if (singlecompare.Tag == what.Tag)
                                            {
                                                found = true;
                                                mergenode.Nodes.Remove(singlecompare);
                                            }
                                        }
                                        if (!found)
                                            mergenode.Nodes.Add(what);
                                    }
                                    else
                                    {
                                        mergenode.Nodes.Add(what);
                                    }
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

        private static void SetEventHandlers(SINners.Models.SINner sinner, CharacterCache objCache)
        {
            objCache.OnMyDoubleClick -= objCache.OnDefaultDoubleClick;
            objCache.OnMyDoubleClick += (sender, e) => OnMyDoubleClick(sinner, objCache); 
            objCache.OnMyAfterSelect -= objCache.OnDefaultAfterSelect;
            objCache.OnMyAfterSelect += (sender, treeViewEventArgs) => OnMyAfterSelect(sinner, objCache, treeViewEventArgs);
            objCache.OnMyKeyDown -= objCache.OnDefaultKeyDown;
            objCache.OnMyKeyDown += async (sender, args) =>
            {
                try
                {
                    using (new CursorWait(true, PluginHandler.MainForm))
                    {
                        if (args.Item1.KeyCode == Keys.Delete)
                        {
                            var client = StaticUtils.GetClient();
                            client.Delete(sinner.Id.Value);
                            objCache.ErrorText = "deleted!";
                            PluginHandler.MainForm.DoThreadSafe(() =>
                            {
                                PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                            });
                        }
                    }
                }
                catch(HttpOperationException e)
                {
                    objCache.ErrorText = e.Message;
                    objCache.ErrorText += Environment.NewLine + e.Response.Content;
                    System.Diagnostics.Trace.TraceWarning(e.ToString());
                }
                catch (Exception e)
                {
                    objCache.ErrorText = e.Message;
                    System.Diagnostics.Trace.TraceWarning(e.ToString());
                }
            };
        }

        private async static void OnMyAfterSelect(SINner sinner, CharacterCache objCache, TreeViewEventArgs treeViewEventArgs)
        {
            string loadFilePath = null;
            using (new CursorWait(true, PluginHandler.MainForm))
            {
                string zipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SINner", sinner.Id.Value.ToString());
                if (Directory.Exists(zipPath))
                {
                    var files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        DateTime lastwrite = File.GetLastWriteTime(file);
                        if ((lastwrite >= sinner.LastChange)
                            || sinner.LastChange == null)
                        {
                            loadFilePath = file;
                            objCache.FilePath = loadFilePath;
                            break;
                        }
                        File.Delete(file);
                    }
                }
                if (String.IsNullOrEmpty(objCache.FilePath))
                { 
                    objCache.FilePath = await DownloadFileTask(sinner, objCache);

                }

                if (!String.IsNullOrEmpty(objCache.FilePath))
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

        private static void OnMyDoubleClick(SINner sinner, CharacterCache objCache)
        {
            var t = DownloadFileTask(sinner, objCache);
            t.ContinueWith((downloadtask) =>
            {
                PluginHandler.MySINnerLoading = sinner;
                PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers(true);
                string filepath = downloadtask.Result as string;
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    Character c = PluginHandler.MainForm.LoadCharacter(filepath);
                    if (c != null)
                    {
                        SwitchToCharacter(c);
                    }
                    //else
                    //    SwitchToCharacter(objCache);

                    PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers(false);
                    PluginHandler.MySINnerLoading = null;
                });

            });
        }

     
        private static void SwitchToCharacter(Character objOpenCharacter)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                    using (new CursorWait(true, PluginHandler.MainForm))
                    {
                        if (objOpenCharacter == null ||
                        !PluginHandler.MainForm.SwitchToOpenCharacter(objOpenCharacter, false))
                        {
                            PluginHandler.MainForm.OpenCharacter(objOpenCharacter, false);
                        }
                    }
                
            });
        }

        private static void SwitchToCharacter(CharacterCache objCache)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                    using (new CursorWait(true, PluginHandler.MainForm))
                    {
                        Character objOpenCharacter = PluginHandler.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FilePath);
                        if (objOpenCharacter == null)
                            objOpenCharacter = PluginHandler.MainForm.LoadCharacter(objCache.FilePath);
                        SwitchToCharacter(objOpenCharacter);
                    }
            });

        }

        public static async Task<HttpOperationResponse> PostSINnerAsync(CharacterExtended ce)
        {
            HttpOperationResponse res = null;
            try
            {
                UploadInfoObject uploadInfoObject = new UploadInfoObject();
                uploadInfoObject.Client = PluginHandler.MyUploadClient;
                uploadInfoObject.UploadDateTime = DateTime.Now;
                ce.MySINnerFile.UploadDateTime = DateTime.Now;
                uploadInfoObject.SiNners = new List<SINner>() { ce.MySINnerFile };
                System.Diagnostics.Trace.TraceInformation("Posting " + ce.MySINnerFile.Id + "...");
                var client = StaticUtils.GetClient();
                if (!StaticUtils.IsUnitTest)
                {
                    
                    res = await client.PostSINWithHttpMessagesAsync(uploadInfoObject);
                    if ((res.Response.StatusCode != HttpStatusCode.OK)
                        && (res.Response.StatusCode != HttpStatusCode.Accepted)
                        && (res.Response.StatusCode != HttpStatusCode.Created))
                    {
                        var msg = "Post of " + ce.MyCharacter.Alias + " completed with StatusCode: " + res?.Response?.StatusCode;
                        msg += Environment.NewLine + "Reason: " + res?.Response?.ReasonPhrase;
                        var content = await res.Response.Content.ReadAsStringAsync();
                        msg += Environment.NewLine + "Content: " + content;
                        System.Diagnostics.Trace.TraceWarning(msg);
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MessageBox.Show(msg);
                        });
                        throw new ArgumentException(msg);
                    }
                }
                else
                {
                    client.PostSINWithHttpMessagesAsync(uploadInfoObject).RunSynchronously();
                }
                System.Diagnostics.Trace.TraceInformation("Post of " + ce.MySINnerFile.Id + " finished.");
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
            return res;
        }

     

        public static async Task<HttpOperationResponse> UploadChummerFileAsync(CharacterExtended ce)
        {
            HttpOperationResponse res = null;
            try
            {
                if (String.IsNullOrEmpty(ce.ZipFilePath))
                    ce.ZipFilePath = await ce.PrepareModel();

                using (FileStream fs = new FileStream(ce.ZipFilePath, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        var client = StaticUtils.GetClient();
                        if (!StaticUtils.IsUnitTest)
                        {
                            HttpStatusCode myStatus = HttpStatusCode.Unused;
                            res = await client.PutSINWithHttpMessagesAsync(ce.MySINnerFile.Id.Value, fs);
                            //var task = res.ContinueWith((sender) =>
                            //{

                                string msg = "Upload ended with statuscode: ";
                                msg += res?.Response?.StatusCode + Environment.NewLine;
                                msg += res?.Response?.ReasonPhrase;
                                msg += Environment.NewLine + res?.Response?.Content.ReadAsStringAsync().Result;
                                System.Diagnostics.Trace.TraceInformation(msg);
                                myStatus = res.Response.StatusCode;
                                if(!StaticUtils.IsUnitTest)
                                {
                                    PluginHandler.MainForm.DoThreadSafe(() =>
                                    {
                                        if(myStatus != HttpStatusCode.OK)
                                        {
                                            MessageBox.Show(msg);
                                        }
                                        using (new CursorWait(true, PluginHandler.MainForm))
                                        {
                                            Chummer.Plugins.PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                                        }   
                                    });
                                }
                                else
                                {
                                    System.Diagnostics.Trace.TraceInformation(msg);
                                }
                            //});
                        }
                        else
                        {
                            client.PutSIN(ce.MySINnerFile.Id.Value, fs);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError(e.ToString());
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MessageBox.Show(e.Message);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
            return res;
        }

        
        public static async Task<CharacterCache> DownloadSINnerExtended(SINners.Models.SINner sinner, CharacterCache objCache)
        {
            try
            {

                try
                {
                    var client = StaticUtils.GetClient();
                    var onlinesinner = await client.GetSINByIdWithHttpMessagesAsync(sinner.Id.Value);
                    var json = onlinesinner.Body.MySINner.MyExtendedAttributes.JsonSummary;
                    var onlineCache = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterCache>(json);
                    objCache.CharacterAlias = onlineCache.CharacterAlias;
                    objCache.CharacterName = onlineCache.CharacterName;
                    objCache.MugshotBase64 = onlineCache.MugshotBase64;
                    objCache.Background = onlineCache.Background;
                    objCache.CharacterNotes = onlineCache.CharacterNotes;
                    objCache.BuildMethod = onlineCache.BuildMethod;
                    objCache.Concept = onlineCache.Concept;
                    objCache.Created = onlineCache.Created;
                    objCache.Description = onlineCache.Description;
                    objCache.ErrorText = onlineCache.ErrorText;
                    objCache.Essence = onlineCache.Essence;
                    objCache.FileName = onlineCache.FileName;
                    objCache.FilePath = onlineCache.FilePath;
                    objCache.GameNotes = onlineCache.GameNotes;
                    objCache.Karma = onlineCache.Karma;
                    objCache.Metatype = onlineCache.Metatype;
                    objCache.Metavariant = onlineCache.Metavariant;
                    objCache.Mugshot = onlineCache.Mugshot;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    if (objCache != null)
                        objCache.ErrorText = ex.Message;
                }

                return objCache;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message);
                objCache.ErrorText = e.Message;
                throw;
            }
        }


        public static async Task<string> DownloadFile(SINners.Models.SINner sinner, CharacterCache objCache)
        {
            try
            {

                //currently only one chum5-File per chum5z ZipFile is implemented!
                string loadFilePath = null;
                string zipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SINner", sinner.Id.Value.ToString());
                if (Directory.Exists(zipPath))
                {
                    var files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        DateTime lastwrite = File.GetLastWriteTime(file);
                        if ((lastwrite >= sinner.LastChange)
                            || sinner.LastChange == null)
                        {
                            loadFilePath = file;
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
                Character objOpenCharacter = null;
                objOpenCharacter = PluginHandler.MainForm?.OpenCharacters?.FirstOrDefault(x => x.FileName == loadFilePath);
                if ((objOpenCharacter != null))
                {
                    return loadFilePath;
                }
                if (String.IsNullOrEmpty(loadFilePath))
                {
                    try
                    {
                        string zippedFile = Path.Combine(System.IO.Path.GetTempPath(), "SINner", sinner.Id.Value + ".chum5z");
                        if (File.Exists(zippedFile))
                            File.Delete(zippedFile);
                        Exception rethrow = null;
                        bool downloadedFromGoogle = false;
                        try
                        {
                            using (WebClient wc = new WebClient())
                            {
                                //wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                                //wc.DownloadFileCompleted += (sender, e) =>
                                //{
                                //    if (e.Error == null)
                                //    {
                                //        PluginHandler.MainForm.DoThreadSafe(() =>
                                //        {
                                //            PluginHandler.MainForm.Text = PluginHandler.MainForm.MainTitle;
                                //        });
                                //    }
                                //    else
                                //    {
                                //        PluginHandler.MainForm.DoThreadSafe(() =>
                                //        {
                                //            PluginHandler.MainForm.Text =
                                //                PluginHandler.MainForm.MainTitle + "Error while loading " +
                                //                sinner.Alias + " from SINners-WebService";
                                //        });
                                //    }

                                //};
                                wc.DownloadFile(
                                    // Param1 = Link of file
                                    new System.Uri(sinner.DownloadUrl),
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
                                var filestream = client.GetDownloadFile(sinner.Id.Value);
                                var array = ReadFully(filestream);
                                File.WriteAllBytes(zippedFile, array);
                            }
                        }

                        if (!File.Exists(zippedFile))
                        {
                            if (rethrow != null)
                                throw rethrow;
                        }


                        System.IO.Compression.ZipFile.ExtractToDirectory(zippedFile, zipPath);
                        var files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                        foreach (var file in files)
                        {
                            if (sinner.UploadDateTime != null)
                                File.SetCreationTime(file, sinner.UploadDateTime.Value);
                            if (sinner.LastChange != null)
                                File.SetLastWriteTime(file, sinner.LastChange.Value);
                            loadFilePath = file;
                            objCache.FilePath = loadFilePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.Message);
                        if (objCache != null)
                            objCache.ErrorText = ex.Message;
                    }
                }
                return objCache.FilePath;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message);
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

        public static Task<string> DownloadSINnerExtendedTask(SINners.Models.SINner sinner, CharacterCache objCache)
        {
            try
            {
                if ((objCache.DownLoadRunning != null) && (objCache.DownLoadRunning.Status == TaskStatus.Running))
                    return objCache.DownLoadRunning;

                objCache.DownLoadRunning = Task.Factory.StartNew<string>(() =>
                {
                    objCache = DownloadSINnerExtended(sinner, objCache).Result;
                    //objCache.FilePath = filepath;
                    return objCache.ErrorText;
                });
                return objCache.DownLoadRunning;
            }
            catch (Exception ex)
            {
                objCache.ErrorText = ex.ToString();
                throw;
            }


        }

        public static Task<string> DownloadFileTask(SINners.Models.SINner sinner, CharacterCache objCache)
        {
            try
            {
                if ((objCache.DownLoadRunning != null)&& (objCache.DownLoadRunning.Status == TaskStatus.Running))
                    return objCache.DownLoadRunning;

                objCache.DownLoadRunning = Task.Factory.StartNew<string>(() =>
                {
                    string filepath = DownloadFile(sinner, objCache).Result;
                    objCache.FilePath = filepath;
                    return objCache.FilePath;
                });
                return objCache.DownLoadRunning;
            }
             catch(Exception ex)
            {
                objCache.ErrorText = ex.ToString();
                throw;
            }

          
        }


            
        
    }
}
