using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Model;
using Microsoft.Rest;
using SINners;
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

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        internal async static Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(ConcurrentDictionary<string, CharacterCache> CharCache)
        {
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
                    MyOnlineTreeNode.Nodes.Clear();
                }
                if (response == null || response.Body?.Any() == false)
                {
                    return new List<TreeNode>() { MyOnlineTreeNode }; 
                }
                foreach (var sinner in response.Body)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(sinner.JsonSummary))
                            continue;
                        CharacterCache objCache = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterCache>(sinner.JsonSummary);
                        SetEventHandlers(sinner, objCache);

                        TreeNode objNode = new TreeNode
                        {
                            Text = objCache.CalculatedName(),
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

                        MyOnlineTreeNode.Nodes.Add(objNode);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceWarning("Could not deseialize CharacterCache-Object: " + sinner.JsonSummary);
                    }
                }
                if (MyOnlineTreeNode.Nodes.Count > 0)
                    return new List<TreeNode>() { MyOnlineTreeNode };
                else
                    return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

        //private void streamToFile(SINners.Models.Stream fileStream, string filePath)
        //{
        //    using (FileStream writeStream = new FileStream(filePath,
        //                                    FileMode.Create,
        //                                    FileAccess.Write))
        //    {
        //        int length = 1024;
        //        Byte[] buffer = new Byte[length];
        //        int bytesRead = fileStream.Read(buffer, 0, length);
        //        while (bytesRead > 0)
        //        {
        //            writeStream.Write(buffer, 0, bytesRead);
        //            bytesRead = fileStream.Read(buffer, 0, length);
        //        }
        //        fileStream.Close();
        //        writeStream.Close();
        //    }
        //}

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
            objCache.OnDoubleClick -= objCache.OnDefaultDoubleClick;
            objCache.OnDoubleClick += (sender, e) =>
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
                Character objOpenCharacter = PluginHandler.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == loadFilePath);
                if (objOpenCharacter == null || !PluginHandler.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                {
                    if (String.IsNullOrEmpty(loadFilePath))
                    {
                        try
                        {
                            PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                            string zippedFile = Path.Combine(System.IO.Path.GetTempPath(), "SINner", sinner.Id.Value + ".chum5z");
                            if (File.Exists(zippedFile))
                                File.Delete(zippedFile);
                            var filestream = StaticUtils.Client.GetDownloadFile(sinner.Id.Value);
                            //var action = StaticUtils.Client.GetDownloadFileWithHttpMessagesAsync();
                            //action.RunSynchronously();
                            //var download = action.Result;
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
                            objCache.ErrorText = ex.Message;
                        }
                        finally
                        {
                            PluginHandler.MainForm.Cursor = Cursors.Default;
                        }
                    }

                    objOpenCharacter = PluginHandler.MainForm.LoadCharacter(loadFilePath);
                    PluginHandler.MainForm.OpenCharacter(objOpenCharacter);


                }
            };
            objCache.OnAfterSelect -= objCache.OnDefaultAfterSelect;
            objCache.OnAfterSelect += (sender, treeViewEventArgs) =>
            {
                //download Char in background...
            };
            objCache.OnKeyDown -= objCache.OnDefaultKeyDown;
            objCache.OnKeyDown += (sender, args) =>
            {
                try
                {
                    PluginHandler.MainForm.Cursor = Cursors.WaitCursor;
                    if (args.Item1.KeyCode == Keys.Delete)
                    {
                        StaticUtils.Client.Delete(sinner.Id.Value);
                        objCache.ErrorText = "deleted!";
                        PluginHandler.MainForm.Invoke(new MethodInvoker(() =>
                        {
                            PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                        }));
                        
                    }
                    PluginHandler.MainForm.Cursor = Cursors.Default;
                }
                catch (Exception e)
                {
                    objCache.ErrorText = e.Message;
                }
                finally
                {
                    PluginHandler.MainForm.Cursor = Cursors.Default;
                }
            };
        }
    }
}
