using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.UI;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.Plugins
{
    [Export(typeof(IPlugin))]
    //[ExportMetadata("Name", "SINners")]
    //[ExportMetadata("frmCareer", "true")]
    public class PluginHandler : IPlugin
    {
        [ImportingConstructor]
        public PluginHandler()
        {
            System.Diagnostics.Trace.TraceInformation("Plugin ChummerHub.Client importing (Constructor).");
        }

        TabPage IPlugin.GetTabPage(frmCareer input)
        {
            SINnersUserControl uc = new SINnersUserControl();
            uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return page;
        }

        TabPage IPlugin.GetTabPage(frmCreate input)
        {
            SINnersUserControl uc = new SINnersUserControl();
            uc.SetCharacterFrom(input);
            TabPage page = new TabPage("SINners");
            page.Controls.Add(uc);
            return page;
        }
    }
}
