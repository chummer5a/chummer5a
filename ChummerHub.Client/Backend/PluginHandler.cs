using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Model;
using ChummerHub.Client.UI;
using Newtonsoft.Json;
using SINners.Models;
using System;
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
        public static CharacterExtended MyCharacterExtended = null;

        public static UploadClient MyUploadClient = null;

        
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

            MyUploadClient.UploadClientId = Properties.Settings.Default.UploadClientId;
        }

        public override string ToString()
        {
            return "SINners (Cloud)";
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCareer input)
        {
            if (MyCharacterExtended?.MyCharacter != input.CharacterObject)
                MyCharacterExtended = new CharacterExtended(input.CharacterObject, null);
            SINnersUserControl uc = new SINnersUserControl();
            uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        IEnumerable<TabPage> IPlugin.GetTabPages(frmCreate input)
        {
            if (MyCharacterExtended?.MyCharacter != input.CharacterObject)
                MyCharacterExtended = new CharacterExtended(input.CharacterObject, null);
            SINnersUserControl uc = new SINnersUserControl();
            uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return new List<TabPage>() { page };
        }

        string IPlugin.GetSaveToFileElement(Character input)
        {
            if (MyCharacterExtended?.MyCharacter != input)
                MyCharacterExtended = new CharacterExtended(input, null);
            return JsonConvert.SerializeObject(MyCharacterExtended.MySINnerFile);
        }

        void IPlugin.LoadFileElement(Character input, string fileElement)
        {
            if (MyCharacterExtended?.MyCharacter != input)
                MyCharacterExtended = new CharacterExtended(input, fileElement);
            
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
    }
}
