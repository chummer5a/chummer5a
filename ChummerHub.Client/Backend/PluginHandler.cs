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
using MessageBox = System.Windows.MessageBox;

namespace Chummer.Plugins
{
    [Export(typeof(IPlugin))]
    //[ExportMetadata("Name", "SINners")]
    //[ExportMetadata("frmCareer", "true")]
    public class PluginHandler : IPlugin
    {
        //public static CharacterExtended MyCharacterExtended = null;
        //public static Dictionary<string, CharacterExtended> MyCharExtendedDic = new Dictionary<string, CharacterExtended>();

        //public static CharacterExtended GetCharExtended(Character c, string fileElement)
        //{
        //    CharacterExtended ce;
        //    if (!MyCharExtendedDic.TryGetValue(c.GetHashCode(), out ce))
        //    {
        //        ce = new CharacterExtended(c, fileElement);
        //        MyCharExtendedDic.Add(ce.GetHashCode(), ce);
        //    }
        //    return ce;    
            
        //}

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

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCareer input)
        {
            SINnersUserControl uc = new SINnersUserControl();
            var ce = uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Name = "SINners";
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCreate input)
        {
            SINnersUserControl uc = new SINnersUserControl();
            var ce = uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Name = "SINners";
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        private static bool IsSaving = false;

        public static SINner MySINnerLoading { get; internal set; }

        string IPlugin.GetSaveToFileElement(Character input)
        {
            CharacterExtended ce;
            //if (MyCharExtendedDic.ContainsKey(input.FileName))
            //{
            //    if (!MyCharExtendedDic.TryGetValue(input.FileName, out ce))
            //        throw new ArgumentException("Could not load char from Dic!", nameof(input));
            //}
            //else
            //{
                
            //}
            ce = new CharacterExtended(input, null);
            if ((SINnersOptions.UploadOnSave == true) && (IsSaving == false))
            {
                IsSaving = true;
                //removing a handler that is not registered is legal - that way only one handler is registered EVER!
                try
                {
                    input.OnSaveCompleted -= MyOnSaveUpload;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceInformation(e.ToString());
                }
                input.OnSaveCompleted += MyOnSaveUpload;
            }
            return JsonConvert.SerializeObject(ce.MySINnerFile.SiNnerMetaData);
        }

        public async static void MyOnSaveUpload(object sender, Character input)
        {
            try
            {
                input.OnSaveCompleted -= MyOnSaveUpload;
                using (new CursorWait(true, MainForm))
                {
                    CharacterExtended ce;
                    //if (MyCharExtendedDic.ContainsKey(input.FileName))
                    //{
                    //    MyCharExtendedDic.TryGetValue(input.FileName, out ce);
                    //}
                    //else
                    //{
                    ce = new CharacterExtended(input, null);
                    //    MyCharExtendedDic.Add(input.FileName, ce);
                    //}

                    if (!ce.MySINnerFile.SiNnerMetaData.Tags.Any(a => a.TagName == "Reflection"))
                    {
                        ce.MySINnerFile.SiNnerMetaData.Tags = ce.PopulateTags();
                    }

                    await ce.Upload();
                    var found = (from a in MainForm.OpenCharacterForms where a.CharacterObject == input select a)
                        .FirstOrDefault();
                    if (found == null)
                    {
                        return;
                    }

                    TabPage tabPage = null;
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
                        var sb = uc as SINnersBasic;
                        if (sb != null)
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
                IsSaving = false;
            }
        }

        void IPlugin.LoadFileElement(Character input, string fileElement)
        {
            try
            {
                CharacterExtended ce;
                //if (MyCharExtendedDic.TryGetValue(input.FileName, out ce))
                //{
                //    ce.MyCharacter = input;
                //}
                //else
                //{
                ce = new CharacterExtended(input, fileElement, PluginHandler.MySINnerLoading);
                //}
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
                Console.Write(e.ToString());
#if DEBUG
                throw;
#endif
            }
            
        }

        IEnumerable<ToolStripMenuItem> IPlugin.GetMenuItems(ToolStripMenuItem input)
        {
            var list = new List<ToolStripMenuItem>();
#if DEBUG
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

#endif
            ToolStripMenuItem mnuNPCs = new ToolStripMenuItem
            {
                Name = "mnuNPCs",
                Text = "&NPCs"
            };
            mnuNPCs.Click += new System.EventHandler(mnuNPCs_Click);
            mnuNPCs.Image = ChummerHub.Client.Properties.Resources.group;
            mnuNPCs.ImageTransparentColor = System.Drawing.Color.Black;
            mnuNPCs.Size = new System.Drawing.Size(148, 22);
            mnuNPCs.Tag = "Menu_Main_NPCs";
            list.Add(mnuNPCs);
            return list;
        }

        private void mnuSINners_Click(object sender, EventArgs e)
        {
            frmSINnerSearch search = new frmSINnerSearch();
            search.Show();
        }

        public static ConcurrentDictionary<string, TreeNode> MyTreeNodes2Add = new ConcurrentDictionary<string, TreeNode>();

        private async void mnuNPCs_Click(object sender, EventArgs ea)
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
                }
                else
                {
                    TreeNode node = new TreeNode("Error: " + e.Message) { ToolTipText = e.ToString(), Tag = e };
                }
            }
            catch (Exception e)
            {
                TreeNode node = new TreeNode("SINners Error: please log in") { ToolTipText = e.ToString(), Tag = e };
            }
        }

        
        public Assembly GetPluginAssembly()
        {
            return typeof(SINnersUserControl).Assembly;
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
            return new SINnersOptions();
        }

        public async Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate)
        {
            try
            {
                using (new CursorWait(true, frmCharRoster))
                {
                    Func<Task<HttpOperationResponse<ResultAccountGetSinnersByAuthorization>>> myMethodName = async () =>
                    {
                        var client = await StaticUtils.GetClient();
                        var ret = await client.GetSINnersByAuthorizationWithHttpMessagesAsync();
                        return ret;
                    }; 
                    var res = await ChummerHub.Client.Backend.Utils.GetCharacterRosterTreeNode(forceUpdate, myMethodName);
                    if (res == null)
                    {
                        throw new ArgumentException("Could not load owned SINners from WebService.");
                    }
                    var list = res.ToList();
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
                    return new List<TreeNode>() { node };
                }
                else
                {
                    TreeNode node = new TreeNode("Error: " + e.Message) {ToolTipText = e.ToString(), Tag = e};
                    return new List<TreeNode>() { node };
                }
            }
            catch(Exception e)
            {
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
            MainForm = mainControl;
            //Not necessary anymore - see GetSINnerByAuthorization
            //Task.Factory.StartNew(async () =>
            //{
            //    try
            //    {
            //        using (new CursorWait(true, MainForm))
            //        {
            //            await Task.Delay(1000 * 10);
            //            var client = await StaticUtils.GetClient();
            //            if (client != null)
            //            {
            //                var res = await client.GetRolesWithHttpMessagesAsync();
            //                if (res != null)
            //                {
            //                    StaticUtils.UserRoles = res.Body.ToList();
            //                }
            //                else
            //                    StaticUtils.UserRoles = new List<string>() {"none "};
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        System.Diagnostics.Trace.TraceError(e.ToString());
            //    }
            //});
        }
    }
}
