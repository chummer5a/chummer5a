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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Chummer.Plugins
{
    [Export(typeof(IPlugin))]
    //[ExportMetadata("Name", "SINners")]
    //[ExportMetadata("frmCareer", "true")]
    public class PluginHandler : IPlugin
    {
        //public static CharacterExtended MyCharacterExtended = null;
        private static Dictionary<string, CharacterExtended> MyCharExtendedDic = new Dictionary<string, CharacterExtended>();

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
#if DEBUG
            SINnersUserControl uc = new SINnersUserControl();
            var ce = uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
#else
            return null;
#endif
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCreate input)
        {
#if DEBUG
            SINnersUserControl uc = new SINnersUserControl();
            var ce = uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
#else
            return null;
#endif
        }

        private bool IsSaving = false;

        string IPlugin.GetSaveToFileElement(Character input)
        {
            CharacterExtended ce;
            if(MyCharExtendedDic.ContainsKey(input.FileName))
                MyCharExtendedDic.Remove(input.FileName);
            ce = new CharacterExtended(input, null);
            MyCharExtendedDic.Add(ce.MyCharacter.FileName, ce);
            if((SINnersOptions.UploadOnSave == true) && (IsSaving == false))
            {
                IsSaving = true;
                //removing a handler that is not registered is legal - that way only one handler is registered EVER!
                input.OnSaveCompleted -= MyOnSaveUpload;
                input.OnSaveCompleted += MyOnSaveUpload;
            }
            return JsonConvert.SerializeObject(ce.MySINnerFile.SiNnerMetaData);
        }

        private async void MyOnSaveUpload(object sender, Character input)
        {
            try
            {
                input.OnSaveCompleted -= MyOnSaveUpload;
                CharacterExtended ce = new CharacterExtended(input, null);
                var found = await StaticUtils.Client.GetByIdWithHttpMessagesAsync(ce.MySINnerFile.Id.Value);
                if(found.Response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var sinjson = await found.Response.Content.ReadAsStringAsync();
                    var foundobj = Newtonsoft.Json.JsonConvert.DeserializeObject<SINner>(sinjson);
                    SINner foundsin = foundobj as SINner;
                    if(foundsin.LastChange >= ce.MyCharacter.FileLastWriteTime)
                    {
                        //is already up to date!
                        return;
                    }

                }
                ce.MySINnerFile.SiNnerMetaData.Tags = ce.PopulateTags();
                ce.PrepareModel();
                await Utils.PostSINnerAsync(ce);
                await Utils.UploadChummerFileAsync(ce);
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
            CharacterExtended ce;
            if(MyCharExtendedDic.TryGetValue(input.FileName, out ce))
            {
                ce.MyCharacter = input;
            }
            else
                ce = new CharacterExtended(input, fileElement);
            
        }

        IEnumerable<ToolStripMenuItem> IPlugin.GetMenuItems(ToolStripMenuItem input)
        {
#if DEBUG
            ToolStripMenuItem mnuSINners = new ToolStripMenuItem();
            mnuSINners.Name = "mnuSINners";
            mnuSINners.Text = "&SINners";
            mnuSINners.Click += new System.EventHandler(mnuSINners_Click);
            mnuSINners.Image = ChummerHub.Client.Properties.Resources.group;
            mnuSINners.ImageTransparentColor = System.Drawing.Color.Black;
            mnuSINners.Size = new System.Drawing.Size(148, 22);
            mnuSINners.Tag = "Menu_SINners";
            return new List<ToolStripMenuItem>() { mnuSINners };
#else
            return null;
#endif
        }

        private void mnuSINners_Click(object sender, EventArgs e)
        {
            frmSINnerSearch search = new frmSINnerSearch();
            search.Show();
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

        public async Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(ConcurrentDictionary<string, frmCharacterRoster.CharacterCache> CharDic, bool forceUpdate)
        {
            return await Utils.GetCharacterRosterTreeNode(CharDic, forceUpdate);
        }

        public void CustomInitialize(frmChummerMain mainControl)
        {
            MainForm = mainControl;
        }
    }
}
