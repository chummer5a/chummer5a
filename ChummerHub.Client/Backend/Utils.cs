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
                        {
                            if (!StaticUtils.IsUnitTest)
                                return null;
                            Properties.Settings.Default.SINnerUrl = "https://sinners.azurewebsites.net/";
                        }
                            
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
                        var version = _client.GetVersion();
                        //var version = _client.GetVersionWithHttpMessagesAsync().Result;
                        System.Diagnostics.Trace.TraceInformation("Connected to SINners in version " + version.AssemblyVersion + ".");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        if (!clientErrorShown)
                        {
                            Exception inner = ex;
                            while (inner.InnerException != null)
                                inner = inner.InnerException;
                            string msg = "Error connecting to SINners: " + Environment.NewLine + Environment.NewLine + inner.Message;
                            msg += Environment.NewLine + Environment.NewLine + "Please check the Plugin-Options dialog.";
                            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            clientErrorShown = true;
                        }
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

        private static TreeNode MyOnlineTreeNode { get; set; }

        private static List<TreeNode> MyTreeNodeList { get; set; }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        internal async static Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(ConcurrentDictionary<string, CharacterCache> CharCache, bool forceUpdate)
        {
            if ((MyTreeNodeList != null) && !forceUpdate)
                return MyTreeNodeList;
            try
            {
                var response = await StaticUtils.Client.GetSINnersByAuthorizationWithHttpMessagesAsync();

                if (MyOnlineTreeNode == null)
                {
                    MyOnlineTreeNode = new TreeNode()
                    {
                        Text = "Online",
                        Tag = "OnlineTag"
                    };
                }
                else
                {
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        MyOnlineTreeNode.Nodes.Clear();
                    });
                    
                }
                if (response.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string msg = "Could not load online Sinners: " + response.Response.ReasonPhrase;
                    var content = await response.Response.Content.ReadAsStringAsync();
                    msg += Environment.NewLine + "Content: " + content;
                    System.Diagnostics.Trace.TraceWarning(msg);
                    MyOnlineTreeNode.ToolTipText = msg;
                    return new List<TreeNode>() { MyOnlineTreeNode };
                }
                if (response == null || response.Body == null || response.Body?.SinLists == null)
                {
                    return new List<TreeNode>() { MyOnlineTreeNode }; 
                }
                foreach (var list in response.Body.SinLists)
                {
                    try
                    {
                        if(!list.SiNners.Any())
                            continue;
                        TreeNode objListNode = new TreeNode
                        {
                            Text = list.Header,
                            Tag = list,
                            ContextMenuStrip = PluginHandler.MainForm.CharacterRoster.ContextMenuStrip
                        };
                        foreach(var sinner in list.SiNners)
                        {
                            if(String.IsNullOrEmpty(sinner.JsonSummary))
                                continue;
                            CharacterCache objCache = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterCache>(sinner.JsonSummary);
                            SetEventHandlers(sinner, objCache);

                            TreeNode objNode = new TreeNode
                            {
                                Text = objCache.CalculatedName(),
                                Tag = sinner.Id.Value.ToString(),
                                ToolTipText = "Last Change: " + sinner.LastChange,
                                ContextMenuStrip = PluginHandler.MainForm.CharacterRoster.ContextMenuStrip

                            };
                            if(!string.IsNullOrEmpty(objCache.ErrorText))
                            {
                                objNode.ForeColor = Color.Red;
                                objNode.ToolTipText += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_Error", GlobalOptions.Language)
                                                        + LanguageManager.GetString("String_Colon", GlobalOptions.Language) + Environment.NewLine + objCache.ErrorText;
                            }
                            CharacterCache delObj;
                            CharCache.TryRemove(sinner.Id.Value.ToString(), out delObj);
                            CharCache.TryAdd(sinner.Id.Value.ToString(), objCache);
                            PluginHandler.MainForm.DoThreadSafe(() =>
                            {
                                objListNode.Nodes.Add(objNode);
                            });
                        }

                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            MyOnlineTreeNode.Nodes.Add(objListNode);
                        });
                        
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceWarning("Could not deserialize CharacterCache-Object: " + list.Header);
                    }
                }
                if (MyOnlineTreeNode.Nodes.Count > 0)
                    return MyTreeNodeList = new List<TreeNode>() { MyOnlineTreeNode };
                else
                    return MyTreeNodeList = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
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
            objCache.OnMyDoubleClick += (sender, e) =>
            {
               
                    var t = DownloadFileTask(sinner, objCache);
                    t.ContinueWith((downloadtask) =>
                    {
                        PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers(true);
                        Character c = downloadtask.Result as Character;
                        if(c != null)
                        {
                            SwitchToCharacter(c);
                        }
                        SwitchToCharacter(objCache);
                        PluginHandler.MainForm.CharacterRoster.SetMyEventHandlers(false);
                    });
               
                
                
            };
            objCache.OnMyAfterSelect -= objCache.OnDefaultAfterSelect;
            objCache.OnMyAfterSelect += (sender, treeViewEventArgs) =>
            {
                DownloadFileTask(sinner, objCache);
            };
            objCache.OnMyKeyDown -= objCache.OnDefaultKeyDown;
            objCache.OnMyKeyDown += (sender, args) =>
            {
                try
                {
                    PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                    if (args.Item1.KeyCode == Keys.Delete)
                    {
                        StaticUtils.Client.Delete(sinner.Id.Value);
                        objCache.ErrorText = "deleted!";
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                        });
                    }
                    PluginHandler.MainForm.Cursor = Cursors.Default;
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
                finally
                {
                    PluginHandler.MainForm.Cursor = Cursors.Default;
                }
            };
        }

        private static void SwitchToCharacter(Character objOpenCharacter)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                if (objOpenCharacter == null || !PluginHandler.MainForm.SwitchToOpenCharacter(objOpenCharacter, false))
                {
                    PluginHandler.MainForm.OpenCharacter(objOpenCharacter, false);
                }
                PluginHandler.MainForm.Cursor = Cursors.Default;
            });
        }

        private static void SwitchToCharacter(CharacterCache objCache)
        {
            Character objOpenCharacter = PluginHandler.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FilePath);
            if (objOpenCharacter == null) 
                objOpenCharacter = PluginHandler.MainForm.LoadCharacter(objCache.FilePath);
            SwitchToCharacter(objOpenCharacter);
        

        }

        public static async Task<HttpOperationResponse> PostSINnerAsync(CharacterExtended ce)
        {
            HttpOperationResponse res = null;
            try
            {
                
                if (!StaticUtils.IsUnitTest)
                {
                  PluginHandler.MainForm.DoThreadSafe(() =>
                  {
                      PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                  });
                }
                UploadInfoObject uploadInfoObject = new UploadInfoObject();
                uploadInfoObject.Client = PluginHandler.MyUploadClient;
                uploadInfoObject.UploadDateTime = DateTime.Now;
                ce.MySINnerFile.UploadDateTime = DateTime.Now;
                uploadInfoObject.SiNners = new List<SINner>() { ce.MySINnerFile };
                System.Diagnostics.Trace.TraceInformation("Posting " + ce.MySINnerFile.Id + "...");
                if (!StaticUtils.IsUnitTest)
                {
                    res = await StaticUtils.Client.PostSINWithHttpMessagesAsync(uploadInfoObject);
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
                    StaticUtils.Client.PostSINWithHttpMessagesAsync(uploadInfoObject).RunSynchronously();
                }
                System.Diagnostics.Trace.TraceInformation("Post of " + ce.MySINnerFile.Id + " finished.");
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
            finally
            {
                if (!StaticUtils.IsUnitTest)
                {
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        PluginHandler.MainForm.Cursor = Cursors.Default;
                    });
                }
            }
            return res;
        }

        public static async Task<HttpOperationResponse> UploadChummerFileAsync(CharacterExtended ce)
        {
            HttpOperationResponse res = null;
            try
            {
                if (String.IsNullOrEmpty(ce.ZipFilePath))
                    ce.ZipFilePath = ce.PrepareModel();

                using (FileStream fs = new FileStream(ce.ZipFilePath, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        if (!StaticUtils.IsUnitTest)
                        {
                            PluginHandler.MainForm.DoThreadSafe(() =>
                            {
                                PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                            });
                            HttpStatusCode myStatus = HttpStatusCode.Unused;
                            res = await StaticUtils.Client.PutSINWithHttpMessagesAsync(ce.MySINnerFile.Id.Value, fs);
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
                                        Chummer.Plugins.PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                                        PluginHandler.MainForm.Cursor = Cursors.Default;
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
                            StaticUtils.Client.PutSIN(ce.MySINnerFile.Id.Value, fs);
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
                    finally
                    {
                        if (!StaticUtils.IsUnitTest)
                        {
                            PluginHandler.MainForm.DoThreadSafe(() =>
                            {
                                PluginHandler.MainForm.Cursor = Cursors.Default;
                            });
                        }
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

        public static Character DownloadFile(SINners.Models.SINner sinner, CharacterCache objCache)
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
                            break;
                        }
                        //no recent file found - download it (again).
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
                    return objOpenCharacter;
                }
                if (String.IsNullOrEmpty(loadFilePath))
                {
                    try
                    {
                        PluginHandler.MainForm?.DoThreadSafe(() =>
                        {
                            PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                        });
                        string zippedFile = Path.Combine(System.IO.Path.GetTempPath(), "SINner", sinner.Id.Value + ".chum5z");
                        if (File.Exists(zippedFile))
                            File.Delete(zippedFile);
                        var filestream = StaticUtils.Client.GetDownloadFile(sinner.Id.Value);
                        var array = ReadFully(filestream);
                        File.WriteAllBytes(zippedFile, array);
                        System.IO.Compression.ZipFile.ExtractToDirectory(zippedFile, zipPath);
                        var files = Directory.EnumerateFiles(zipPath, "*.chum5", SearchOption.TopDirectoryOnly);
                        foreach (var file in files)
                        {
                            if (sinner.UploadDateTime != null)
                                File.SetCreationTime(file, sinner.UploadDateTime.Value);
                            if (sinner.LastChange != null)
                                File.SetLastWriteTime(file, sinner.LastChange.Value);
                            loadFilePath = file;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.Message);
                        if (objCache != null)
                            objCache.ErrorText = ex.Message;
                    }
                    finally
                    {
                        PluginHandler.MainForm?.DoThreadSafe(() =>
                        {
                            PluginHandler.MainForm.Cursor = Cursors.Default;
                        });
                    }
                    
                }
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message);
                objCache.ErrorText = e.Message;
                throw;
            }
        }

        public static Task<Character> DownloadFileTask(SINners.Models.SINner sinner, CharacterCache objCache)
        {
            try
            {
                if(objCache.DownLoadRunning != null)
                    return objCache.DownLoadRunning;

                objCache.DownLoadRunning = Task.Factory.StartNew<Character>(() =>
                {
                    return DownloadFile(sinner, objCache);
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
