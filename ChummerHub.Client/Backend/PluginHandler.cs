using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Model;
using ChummerHub.Client.UI;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Rest;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using System.Windows.Threading;
using Chummer.Properties;
using NLog;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using SINners;
using Formatting = Newtonsoft.Json.Formatting;
using MessageBox = System.Windows.MessageBox;
using TabControl = System.Windows.Forms.TabControl;

namespace Chummer.Plugins
{
    [Export(typeof(IPlugin))]
    //[ExportMetadata("Name", "SINners")]
    //[ExportMetadata("frmCareer", "true")]
    public class PluginHandler : IPlugin
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static UploadClient MyUploadClient = null;

        public static frmChummerMain MainForm = null;

        [ImportingConstructor]
        public PluginHandler()
        {
            if (ChummerHub.Client.Properties.Settings.Default.UpgradeRequired)
            {
                ChummerHub.Client.Properties.Settings.Default.Upgrade();
                ChummerHub.Client.Properties.Settings.Default.UpgradeRequired = false;
                ChummerHub.Client.Properties.Settings.Default.Save();
            }
            System.Diagnostics.Trace.TraceInformation("Plugin ChummerHub.Client importing (Constructor).");
            MyUploadClient = new UploadClient();
            if (Properties.Settings.Default.UploadClientId == Guid.Empty)
            {
                Properties.Settings.Default.UploadClientId = Guid.NewGuid();
                Properties.Settings.Default.Save();
            }
            MyUploadClient.Id = Properties.Settings.Default.UploadClientId;
        }



        public override string ToString()
        {
            return "SINners";
        }

        bool IPlugin.SetCharacterRosterNode(TreeNode objNode)
        {
            if (objNode?.ContextMenuStrip == null)
                return false;
            ToolStripMenuItem newShare = new ToolStripMenuItem("Share")
            {
                Name = "tsShareChummer",
                Tag = "Menu_ShareChummer",
                Text = "Share chummer",
                Size = new System.Drawing.Size(177, 22),
                Image = global::Chummer.Properties.Resources.link_add
            };
            newShare.Click += NewShareOnClick;
            objNode.ContextMenuStrip.Items.Add(newShare);
            LanguageManager.TranslateWinForm(GlobalOptions.Language, objNode.ContextMenuStrip);
            return true;
        }

        public ITelemetry SetTelemetryInitialize(ITelemetry telemetry)
        {
            //We should maybe add an option in the plugin-option dialog to give the user the opportunity to enable this again.
            //if (!String.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.UserEmail))
            //{
            //    if (telemetry?.Context?.User != null)
            //        telemetry.Context.User.AccountId = ChummerHub.Client.Properties.Settings.Default.UserEmail;
            //}
            return telemetry;
        }

        bool IPlugin.ProcessCommandLine(string parameter)
        {
            Log.Debug("ChummerHub.Client.PluginHandler ProcessCommandLine: " + parameter);
            string argument = "";
            string onlyparameter = parameter;
            if (parameter.Contains(':'))
            {
                argument = parameter.Substring(parameter.IndexOf(':'));
                argument = argument.TrimStart(':');
                onlyparameter = parameter.Substring(0, parameter.IndexOf(':'));
            }
            switch (onlyparameter)
            {
                case "RegisterUriScheme":
                    if (StaticUtils.RegisterChummerProtocol(argument))
                        Environment.ExitCode = -1;
                    else
                        Environment.ExitCode = 0;
                    return false;
                    break;
                case "Load":
                    return HandleLoadCommand(argument);
                    break;
                default:
                    Log.Warn("Unknown command line parameter: " + parameter);
                    return true;
                    break;
            }
            return true;
        }

        void IPlugin.Dispose()
        {
            if (PipeManager != null)
            {
                //only stop the server if this is the last instance!
                if (!BlnHasDuplicate)
                    PipeManager.StopServer();
            }
                
        }

        private bool HandleLoadCommand(string argument)
        {
            //check global mutex
            bool blnHasDuplicate = false;
            try
            {
                blnHasDuplicate = !Program.GlobalChummerMutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException ex)
            {
                Log.Error(ex);
                Utils.BreakIfDebug();
                blnHasDuplicate = true;
            }
            
            var thread = new Thread((myargument) =>
            {
                if (!blnHasDuplicate)
                {
                    var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    if (uptime < TimeSpan.FromSeconds(2))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                if (PipeManager != null)
                {
                    string msg = "Load:" + myargument;
                    Log.Trace("Sending argument to Pipeserver: " + msg);
                    PipeManager.Write(msg);
                }
            });
            thread.Start(argument);
            thread.Join();
            if (blnHasDuplicate)
            {
                Environment.ExitCode = -1;
                return false;
            }
            else
                return true;
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCareer input)
        {
            if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == false)
                return null;
            ucSINnersUserControl uc = new ucSINnersUserControl();
            var ce = uc.SetCharacterFrom(input);
            if (ce.Status == TaskStatus.Faulted)
            {
                ChummerHub.Client.Backend.Utils.HandleError(ce.Exception);
                return new List<TabPage>();
            }
            TabPage page = new TabPage("SINners");
            page.Name = "SINners";
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCreate input)
        {
            if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == false)
                return null;
            ucSINnersUserControl uc = new ucSINnersUserControl();
            var ce = uc.SetCharacterFrom(input);
            if (ce.Status == TaskStatus.Faulted)
            {
                ChummerHub.Client.Backend.Utils.HandleError(ce.Exception);
                return new List<TabPage>();
            }
            TabPage page = new TabPage("SINners");
            page.Name = "SINners";
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        private static bool _isSaving = false;

        public static SINner MySINnerLoading { get; internal set; }
        public NamedPipeManager PipeManager { get; private set; }

        string IPlugin.GetSaveToFileElement(Character input)
        {
            CharacterExtended ce = GetMyCe(input);
            
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = jsonResolver,
                
            };
            //remove the reflection tag - no need to save it
            Tag refTag = null;
            string returnme = null;
            if (ce?.MySINnerFile?.SiNnerMetaData?.Tags != null)
            {
                var reflectionseq =
                    (from a in ce.MySINnerFile.SiNnerMetaData.Tags where a != null && a.TagName == "Reflection" select a);
                if (reflectionseq?.Any() == true)
                {
                    refTag = reflectionseq.FirstOrDefault();
                    ce.MySINnerFile.SiNnerMetaData.Tags.Remove(refTag);
                }
                returnme = JsonConvert.SerializeObject(ce.MySINnerFile, Formatting.Indented, settings);
                ce.MySINnerFile.SiNnerMetaData.Tags.Add(refTag);
                return returnme;
            }
            else
            {
                if (ce != null)
                    returnme = JsonConvert.SerializeObject(ce.MySINnerFile, Formatting.Indented, settings);
            }

            return returnme;

        }

        public static async void MyOnSaveUpload(object sender, Character input)
        {
            try
            {
                if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == false)
                {
                    string msg = "Public Mode currently does not save to the SINners Plugin by default, even if \"onlinemode\" is enabled!" + Environment.NewLine;
                    msg += "If you want to use SINners as online store, please register!";
                    Log.Warn(msg);
                    return;
                }
                input.OnSaveCompleted = null;
                using (new CursorWait(true, MainForm))
                {
                    var ce = GetMyCe(input);
                    //ce = new CharacterExtended(input, null);
                    if (ce.MySINnerFile.SiNnerMetaData.Tags.Any(a => a != null && a.TagName == "Reflection") == false)
                    {
                        ce.MySINnerFile.SiNnerMetaData.Tags = ce.PopulateTags();
                    }

                    await ce.Upload();
                    

                    TabPage tabPage = null;
                    var found = (from a in MainForm.OpenCharacterForms where a.CharacterObject == input select a)
                        .FirstOrDefault();
                    if ((found is frmCreate frm) && (frm.TabCharacterTabs.TabPages.ContainsKey("SINners")))
                    {
                        var index = frm.TabCharacterTabs.TabPages.IndexOfKey("SINners");
                        tabPage = frm.TabCharacterTabs.TabPages[index];
                    }


                    if ((found is frmCareer frm2) && (frm2.TabCharacterTabs.TabPages.ContainsKey("SINners")))
                    {
                        var index = frm2.TabCharacterTabs.TabPages.IndexOfKey("SINners");
                        tabPage = frm2.TabCharacterTabs.TabPages[index];
                    }

                    if (tabPage == null)
                        return;
                    var ucseq = tabPage.Controls.Find("SINnersBasic", true);
                    foreach (var uc in ucseq)
                    {
                        if (uc is ucSINnersBasic sb)
                            await sb?.CheckSINnerStatus();
                    }

                    var ucseq2 = tabPage.Controls.Find("SINnersAdvanced", true);
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
            }
            finally
            {
                input.OnSaveCompleted += MyOnSaveUpload;
                _isSaving = false;
            }
        }

        private static CharacterExtended GetMyCe(Character input)
        {
            CharacterShared found = null;
            if (MainForm?.OpenCharacterForms != null)
                foreach (CharacterShared a in (MainForm?.OpenCharacterForms))
                {
                    if (a?.CharacterObject != input) continue;
                    found = a;
                    break;
                }
            TabPage sinnertab = null;
            if (found != null)
            {



                
                TabControl.TabPageCollection myCollection = null;
                switch (found)
                {
                    case frmCreate foundcreate:
                        myCollection = foundcreate.TabCharacterTabs.TabPages;
                        break;
                    case frmCareer foundcareer:
                        myCollection = foundcareer.TabCharacterTabs.TabPages;
                        break;
                }

                if (myCollection == null)
                    return null;

                foreach (TabPage tab in myCollection)
                {
                    if (tab.Name == "SINners")
                    {
                        sinnertab = tab;
                        break;
                    }
                }
            }

            CharacterExtended ce;
            frmCharacterRoster.CharacterCache myCharacterCache = new frmCharacterRoster.CharacterCache(input?.FileName);
            if (sinnertab == null)
            {
                ce = new CharacterExtended(input, null, null, myCharacterCache);
            }
            else
            {
                ucSINnersUserControl myUcSIN = null;
                foreach (ucSINnersUserControl ucSIN in sinnertab.Controls)
                {
                    myUcSIN = ucSIN;
                    break;
                }

                ce = myUcSIN == null ? new CharacterExtended(input, null, null, myCharacterCache) : myUcSIN.MyCE;
            }
            return ce;
        }

        void IPlugin.LoadFileElement(Character input, string fileElement)
        {
            try
            {
                CharacterExtended ce;
                ce = new CharacterExtended(input, fileElement, PluginHandler.MySINnerLoading);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                throw;
#endif
            }
            
        }

        IEnumerable<ToolStripMenuItem> IPlugin.GetMenuItems(ToolStripMenuItem input)
        {
            var list = new List<ToolStripMenuItem>();

#if DEBUG
            if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == true)
            {
                ToolStripMenuItem mnuSINnerSearchs = new ToolStripMenuItem
                {
                    Name = "mnuSINSearch",
                    Text = "&SINner Search"
                };
                mnuSINnerSearchs.Click += new System.EventHandler(mnuSINnerSearchs_Click);
                mnuSINnerSearchs.Image = ChummerHub.Client.Properties.Resources.group;
                mnuSINnerSearchs.ImageTransparentColor = System.Drawing.Color.Black;
                mnuSINnerSearchs.Size = new System.Drawing.Size(148, 22);
                mnuSINnerSearchs.Tag = "Menu_Tools_SINnerSearch";
                list.Add(mnuSINnerSearchs);
            }
#endif
            ToolStripMenuItem mnuSINnersArchetypes = new ToolStripMenuItem
            {
                Name = "mnuSINnersArchetypes",
                Text = "&Archetypes"
            };
            mnuSINnersArchetypes.Click += new System.EventHandler(mnuSINnersArchetypes_Click);
            mnuSINnersArchetypes.Image = ChummerHub.Client.Properties.Resources.group;
            mnuSINnersArchetypes.ImageTransparentColor = System.Drawing.Color.Black;
            mnuSINnersArchetypes.Size = new System.Drawing.Size(148, 22);
            mnuSINnersArchetypes.Tag = "Menu_Tools_SINnersArchetypes";
            list.Add(mnuSINnersArchetypes);

            if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == true)
            {
                ToolStripMenuItem mnuSINners = new ToolStripMenuItem
                {
                    Name = "mnuSINners",
                    Text = "&SINners"
                };
                mnuSINners.Click += new System.EventHandler(mnuSINners_Click);
                mnuSINners.Image = ChummerHub.Client.Properties.Resources.group;
                mnuSINners.ImageTransparentColor = System.Drawing.Color.Black;
                mnuSINners.Size = new System.Drawing.Size(148, 22);
                mnuSINners.Tag = "Menu_Tools_SINners";
                list.Add(mnuSINners);
            }

            return list;
        }

        private void mnuSINnerSearchs_Click(object sender, EventArgs e)
        {
            frmSINnerSearch search = new frmSINnerSearch();
            search.Show();
        }

        private async void mnuSINnersArchetypes_Click(object sender, EventArgs e)
        {
            SINSearchGroupResult ssgr = null;
            HttpOperationResponse<ResultGroupGetSearchGroups> res = null;
            try
            {
                using (new CursorWait(true, MainForm))
                {
                    var client = StaticUtils.GetClient();
                    res = await client.GetPublicGroupWithHttpMessagesAsync("Archetypes", null, null);
                    var result =
                        await ChummerHub.Client.Backend.Utils.HandleError(res, res.Body) as ResultGroupGetSearchGroups;
                    if (result == null)
                        return;
                    if (result.CallSuccess == true)
                    {
                        ssgr = result.MySearchGroupResult;
                        var ssgr1 = ssgr;
                        PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                        {
                            using (new CursorWait(true, MainForm))
                            {
                                if (ssgr1 != null && ssgr1.SinGroups?.Any() == true)
                                {
                                    var list = ssgr1.SinGroups.Where(a => a.Groupname == "Archetypes").ToList();
                                    var nodelist =
                                        ChummerHub.Client.Backend.Utils.CharacterRosterTreeNodifyGroupList(list);
                                    foreach (var node in nodelist)
                                    {
                                        PluginHandler.MyTreeNodes2Add.AddOrUpdate(node.Name, node,
                                            (key, oldValue) => node);
                                    }

                                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                                    PluginHandler.MainForm.CharacterRoster.treCharacterList.SelectedNode =
                                        nodelist.FirstOrDefault(a => a.Name ==  "Archetypes");
                                    PluginHandler.MainForm.BringToFront();
                                }
                                else
                                {
                                    MessageBox.Show("No archetypes found!");
                                }
                            }
                        });
                    }

                    ssgr = null;
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var result =
                       await ChummerHub.Client.Backend.Utils.HandleError(res, res.Body) as ResultGroupGetSearchGroups;
                if (result == null)
                    return;
            }
        }

        public static ConcurrentDictionary<string, TreeNode> MyTreeNodes2Add = new ConcurrentDictionary<string, TreeNode>();

        private async void mnuSINners_Click(object sender, EventArgs ea)
        {
            try
            {
                using (new CursorWait(true, PluginHandler.MainForm))
                {
                    frmSINnerGroupSearch frmSearch = new frmSINnerGroupSearch(null, null);
                    frmSearch.TopMost = true;
                    frmSearch.Show(PluginHandler.MainForm);
                }

            }
            catch (Microsoft.Rest.SerializationException e)
            {
                if (e.Content.Contains("Log in - ChummerHub"))
                {
                    TreeNode node = new TreeNode("Online, but not logged in!")
                    {
                        ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login",
                        Tag = e
                    };
                    Log.Warn("Online, but not logged in!");
                }
                else
                {
                    Log.Warn(e);
                    TreeNode node = new TreeNode("Error: " + e.Message)
                    {
                        ToolTipText = e.ToString(), Tag = e
                    };
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                TreeNode node = new TreeNode("SINners Error: please log in") { ToolTipText = e.ToString(), Tag = e };
            }
        }

        
        public Assembly GetPluginAssembly()
        {
            return typeof(ucSINnersUserControl).Assembly;
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
            StaticUtils.MyUtils.IsUnitTest = isUnitTest;
            if (!StaticUtils.MyUtils.IsUnitTest)
                MyUploadClient.ChummerVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            else
                MyUploadClient.ChummerVersion = System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();

        }

        public System.Windows.Forms.UserControl GetOptionsControl()
        {
            return new ucSINnersOptions();
        }

        
        public async Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate)
        {
            try
            {
                ContextMenuStrip myContextMenuStrip = null;
                List<TreeNode> list = new List<TreeNode>();
                using (new CursorWait(true, frmCharRoster))
                {
                        frmCharRoster.DoThreadSafe(() =>
                        {
                            myContextMenuStrip = frmCharRoster.CreateContextMenuStrip();
                            var menulist = myContextMenuStrip.Items.Cast<ToolStripMenuItem>().ToList();
                            foreach (var item in menulist)
                            {
                                switch (item.Name)
                                {
                                    case "tsToggleFav":
                                        myContextMenuStrip.Items.Remove(item);
                                        break;
                                    case "tsCloseOpenCharacter":
                                        myContextMenuStrip.Items.Remove(item);
                                        break;
                                    case "tsSort":
                                        myContextMenuStrip.Items.Remove(item);
                                        break;
                                    case "tsDelete":
                                        ToolStripMenuItem newDelete = new ToolStripMenuItem(item.Text, item.Image);
                                        newDelete.Click += frmCharRoster.tsDelete_Click;
                                        myContextMenuStrip.Items.Add(newDelete);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            ToolStripMenuItem newShare = new ToolStripMenuItem("Share")
                            {
                                Name = "tsShareChummer",
                                Tag = "Menu_ShareChummer",
                                Text = "Share chummer",
                                Size = new System.Drawing.Size(177, 22),
                                Image = global::Chummer.Properties.Resources.link_add
                            };
                            newShare.Click += NewShareOnClick;
                            myContextMenuStrip.Items.Add(newShare);
                            LanguageManager.TranslateWinForm(GlobalOptions.Language, myContextMenuStrip);
                        });
                    if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == true)
                    {
                        Log.Info("Loading CharacterRoster from SINners...");
                        Func<Task<HttpOperationResponse<ResultAccountGetSinnersByAuthorization>>> myMethodName = async () =>
                        {
                            try
                            {
                                var client = StaticUtils.GetClient();
                                var ret = await client.GetSINnersByAuthorizationWithHttpMessagesAsync();
                                return ret;
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                                throw;
                            }
                        };
                        var res = await ChummerHub.Client.Backend.Utils.GetCharacterRosterTreeNode(forceUpdate, myMethodName);
                        if (res == null)
                        {
                            throw new ArgumentException("Could not load owned SINners from WebService.");
                        }
                        list = res.ToList();
                    }
                    var myadd = MyTreeNodes2Add.ToList();
                    var mysortadd = (from a in myadd orderby a.Value.Text select a).ToList();
                    foreach (var addme in mysortadd)
                    {
                        list.Add(addme.Value);
                    }
                    AddContextMenuStripRecursive(list, myContextMenuStrip);
                    return list;
                }
                    
            }
            catch(Microsoft.Rest.SerializationException e)
            {
                
                if (e.Content.Contains("Log in - ChummerHub"))
                {
                    TreeNode node = new TreeNode("Online, but not logged in!")
                    {
                        ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login", Tag = e
                    };
                    Log.Warn(e, "Online, but not logged in!");
                    return new List<TreeNode>() { node };
                }
                else
                {
                    Log.Error(e);
                    TreeNode node = new TreeNode("Error: " + e.Message) {ToolTipText = e.ToString(), Tag = e};
                    return new List<TreeNode>() { node };
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
                TreeNode node = new TreeNode("SINners Error: please log in") {ToolTipText = e.ToString(), Tag = e};
                var objCache = new frmCharacterRoster.CharacterCache
                {
                    ErrorText = e.ToString()
                };
                node.Tag = objCache;
                return new List<TreeNode>() { node };
            }
        }

        private async void NewShareOnClick(object sender, EventArgs e)
        {
            TreeNode t = PluginHandler.MainForm.CharacterRoster.treCharacterList.SelectedNode;

            if (t?.Tag is frmCharacterRoster.CharacterCache objCache)
            {
                frmSINnerShare share = new frmSINnerShare();
                share.MyUcSINnerShare.MyCharacterCache = objCache;
                share.TopMost = true;
                share.Show(PluginHandler.MainForm);
                await share.MyUcSINnerShare.DoWork();
            }
        }

        private void AddContextMenuStripRecursive(List<TreeNode> list, ContextMenuStrip myCmsRoster)
        {
            foreach (var node in list)
            {
                
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    node.ContextMenuStrip = myCmsRoster;
                });
                
                if (node.Nodes.Count > 0)
                {
                    var myList = node.Nodes.Cast<TreeNode>().ToList();
                    AddContextMenuStripRecursive(myList, myCmsRoster);
                }
            }
        }

        public bool BlnHasDuplicate { get; set; }

        public void CustomInitialize(frmChummerMain mainControl)
        {
            Log.Info("CustomInitialize for Plugin ChummerHub.Client entered.");
            MainForm = mainControl;
            if (String.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.TempDownloadPath))
            {
                ChummerHub.Client.Properties.Settings.Default.TempDownloadPath = Path.GetTempPath();
            }

            //check global mutex
            BlnHasDuplicate = false;
            try
            {
                BlnHasDuplicate = !Program.GlobalChummerMutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException ex)
            {
                Log.Error(ex);
                Utils.BreakIfDebug();
                BlnHasDuplicate = true;
            }
            if (PipeManager == null)
            {
                PipeManager = new NamedPipeManager("Chummer");
                Log.Info("blnHasDuplicate = " + BlnHasDuplicate.ToString());
                // If there is more than 1 instance running, do not let the application start a receiving server.
                if (BlnHasDuplicate)
                {
                    Log.Info("More than one instance, not starting NamedPipe-Server...");
                    throw new ApplicationException("More than one instance is running.");
                }
                else
                {
                    Log.Info("Only one instance, starting NamedPipe-Server...");
                    PipeManager.StartServer();
                    PipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
                }
            }


        }

        private static string fileNameToLoad = "";

        public static async void HandleNamedPipe_OpenRequest(string argument)
        {
                Log.Trace("Pipeserver receiced a request: " + argument);
                if (!string.IsNullOrEmpty(argument))
                {
                    //make sure the mainform is visible ...
                    var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    if (uptime < TimeSpan.FromSeconds(5))
                        Thread.Sleep(TimeSpan.FromSeconds(4));
                    if (PluginHandler.MainForm.Visible == false)
                    {
                        PluginHandler.MainForm.DoThreadSafe(() =>
                        {
                            if (PluginHandler.MainForm.WindowState == FormWindowState.Minimized)
                                PluginHandler.MainForm.WindowState = FormWindowState.Normal;
                            PluginHandler.MainForm.Activate();
                        });
                    }
                var client = StaticUtils.GetClient();
                while (PluginHandler.MainForm.Visible == false)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                if (argument.StartsWith("Load:"))
                {
                    try
                    {
                        string SINnerIdvalue = argument.Substring(5);
                        SINnerIdvalue = SINnerIdvalue.Trim('/');
                        if (Guid.TryParse(SINnerIdvalue, out Guid SINnerId))
                        {
                                
                            var found = await client.GetSINByIdWithHttpMessagesAsync(SINnerId);
                            await ChummerHub.Client.Backend.Utils.HandleError(found, found?.Body);
                            if (found?.Response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                fileNameToLoad = await ChummerHub.Client.Backend.Utils.DownloadFileTask(found.Body.MySINner, null);
                                await MainFormLoadChar(fileNameToLoad);
                            }
                            else if (found?.Response.StatusCode == HttpStatusCode.NotFound)
                            {
                                PluginHandler.MainForm.ShowMessageBox("Could not find a SINner with Id " + SINnerId + " online!");
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        PluginHandler.MainForm.ShowMessageBox("Error loading SINner: " + e.Message);
                    }
                        
                }
                else
                {
                    throw new ArgumentException("Unkown command received: " + argument, nameof(argument));
                }
            }
        }

        private static async Task<Character> MainFormLoadChar(string fileToLoad)
        {
            
                using (frmLoading frmLoadingForm = new frmLoading {CharacterFile = fileToLoad })
                {
                    Character objCharacter = new Character()
                    {
                        FileName = fileToLoad
                    };
                    //already open
                    var foundseq = (from a in PluginHandler.MainForm.OpenCharacters
                        where a.FileName == fileToLoad
                        select a);
                    if (foundseq.Any())
                    {
                        objCharacter = foundseq.FirstOrDefault();
                    }
                    else
                    {
                        frmLoadingForm.Reset(36);
                        frmLoadingForm.TopMost = true;
                        frmLoadingForm.Show();
                        if (await objCharacter.Load(frmLoadingForm, ChummerHub.Client.Properties.Settings.Default.IgnoreWarningsOnOpening))
                            PluginHandler.MainForm.OpenCharacters.Add(objCharacter);
                        else
                            return objCharacter;
                    }
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        var foundform = from a in PluginHandler.MainForm.OpenCharacterForms
                            where a.CharacterObject == objCharacter
                            select a;
                        if (foundform.Any())
                        {
                            PluginHandler.MainForm.SwitchToOpenCharacter(objCharacter, false);
                        }
                        else
                        {
                            PluginHandler.MainForm.OpenCharacter(objCharacter, false);
                        }
                        PluginHandler.MainForm.BringToFront();
                    });
                    return objCharacter;
                }
      
        }
    }
}
