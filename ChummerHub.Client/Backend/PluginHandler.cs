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
        private static Dictionary<int, CharacterExtended> MyCharExtendedDic = new Dictionary<int, CharacterExtended>();

        public static CharacterExtended GetCharExtended(Character c, string fileElement)
        {
            CharacterExtended ce;
            if (!MyCharExtendedDic.TryGetValue(c.GetHashCode(), out ce))
            {
                ce = new CharacterExtended(c, fileElement);
                MyCharExtendedDic.Add(ce.GetHashCode(), ce);
            }
            return ce;    
            
        }

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
            var ce = GetCharExtended(input.CharacterObject, null);
            SINnersUserControl uc = new SINnersUserControl();
            uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCreate input)
        {
            var ce = GetCharExtended(input.CharacterObject, null);
            SINnersUserControl uc = new SINnersUserControl();
            uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        string IPlugin.GetSaveToFileElement(Character input)
        {
            var ce = GetCharExtended(input, null);
            if (SINnersOptions.UploadOnSave == true)
            {
                input.OnSaveCompleted += (async (sender, e) =>
                {
                    ce.PrepareModel(false);
                    await Utils.PostSINnerAsync(ce);
                    await Utils.UploadChummerFileAsync(ce);
                });
            }
            return JsonConvert.SerializeObject(ce.MySINnerFile);
        }

        

        void IPlugin.LoadFileElement(Character input, string fileElement)
        {
            var ce = GetCharExtended(input, fileElement);

        }

        IEnumerable<ToolStripMenuItem> IPlugin.GetMenuItems(ToolStripMenuItem input)
        {
            ToolStripMenuItem mnuSINners = new ToolStripMenuItem();
            mnuSINners.Name = "mnuSINners";
            mnuSINners.Text = "&SINners";
            mnuSINners.Click += new System.EventHandler(mnuSINners_Click);
            mnuSINners.Image = ChummerHub.Client.Properties.Resources.group;
            mnuSINners.ImageTransparentColor = System.Drawing.Color.Black;
            mnuSINners.Size = new System.Drawing.Size(148, 22);
            mnuSINners.Tag = "Menu_SINners";
            return new List<ToolStripMenuItem>() { mnuSINners };
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
