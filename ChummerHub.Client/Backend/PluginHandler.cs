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
using Chummer.Properties;
using NLog;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
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
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
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
            return "SINners (Cloud)";
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

        private static bool IsSaving = false;

        public static SINner MySINnerLoading { get; internal set; }

        string IPlugin.GetSaveToFileElement(Character input)
        {
            CharacterExtended ce = GetMyCe(input);
            
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            //jsonResolver.IgnoreProperty(typeof(String), "MugshotBase64");
            //jsonResolver.IgnoreProperty(typeof(SINnerExtended), "jsonSummary");
            //jsonResolver.RenameProperty(typeof(Person), "FirstName", "firstName");
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
                returnme = JsonConvert.SerializeObject(ce.MySINnerFile, Formatting.Indented, settings);
            }

            return returnme;

        }

        public static async void MyOnSaveUpload(object sender, Character input)
        {
            try
            {
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
                IsSaving = false;
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

            if (found == null)
            {
                return null;
            }

            TabPage sinnertab = null;
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
            CharacterExtended ce;
            if (sinnertab == null)
            {
                ce = new CharacterExtended(input, null);
            }
            else
            {
                ucSINnersUserControl myUcSIN = null;
                foreach (ucSINnersUserControl ucSIN in sinnertab.Controls)
                {
                    myUcSIN = ucSIN;
                    break;
                }

                ce = myUcSIN == null ? new CharacterExtended(input, null) : myUcSIN.MyCE;
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
                mnuSINnerSearchs.Tag = "Menu_Main_SINnerSearch";
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
            mnuSINnersArchetypes.Tag = "Menu_Main_SINnersArchetypes";
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
                mnuSINners.Tag = "Menu_Main_SINners";
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
                        PluginHandler.MainForm.DoThreadSafe(() =>
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
                                    PluginHandler.MainForm.CharacterRoster.BringToFront();
                                    MessageBox.Show("Archetypes loaded to character roster!");
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
                List<TreeNode> list = new List<TreeNode>();
                using (new CursorWait(true, frmCharRoster))
                {
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

        public void CustomInitialize(frmChummerMain mainControl)
        {
            Log.Info("CustomInitialize for Plugin ChummerHub.Client entered.");
            MainForm = mainControl;
            if (String.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.TempDownloadPath))
            {
                ChummerHub.Client.Properties.Settings.Default.TempDownloadPath = Path.GetTempPath();
            }
        }
    }
}
