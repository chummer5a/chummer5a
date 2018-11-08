using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Plugins
{
    [InheritedExport(typeof(IPlugin))]
    public interface IPlugin
    {
        //UserControl GetUserControl(frmCareer input);
        //UserControl GetUserControl(frmCreate input);

        IEnumerable<TabPage> GetTabPages(frmCareer input);
        IEnumerable<TabPage> GetTabPages(frmCreate input);
        IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem menu);

        UserControl GetOptionsControl();

        string GetSaveToFileElement(Character input);
        void LoadFileElement(Character input, string fileElement);

        void SetIsUnitTest(bool isUnitTest);

        Assembly GetPluginAssembly();
    }


    public class PluginControl
    {
        private static CompositionContainer container = null;
        public static CompositionContainer Container { get { return container; } }

        private static AggregateCatalog catalog;

        public void Initialize()
        {
            catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path: "Plugins", searchPattern: "*.exe"));
            catalog.Catalogs.Add(new DirectoryCatalog(path: "Plugins", searchPattern: "*.dll"));
            container = new CompositionContainer(catalog);
            //Fill the imports of this object
          
            StartWatch();
            container.ComposeParts(this);
            foreach (var plugin in MyPlugins)
            {
                plugin.SetIsUnitTest(Utils.IsUnitTest);
            }
        }

        [ImportMany(typeof(IPlugin))]
        public IEnumerable<IPlugin> MyPlugins { get; set; }


        private static void StartWatch()
        {
            var watcher = new FileSystemWatcher() { Path = ".", NotifyFilter = NotifyFilters.LastWrite };
            watcher.Changed += (s, e) =>
            {
                string lName = e.Name.ToLower();
                if (lName.EndsWith(".dll") || lName.EndsWith(".exe"))
                    Refresh();
            };
            watcher.EnableRaisingEvents = true;
        }

        public static void Refresh()
        {
            foreach (DirectoryCatalog dCatalog in catalog.Catalogs)
                dCatalog.Refresh();
        }

        internal void LoadPlugins()
        {
            try
            {
                this.Initialize();
            }
            catch (ReflectionTypeLoadException e)
            {
                string msg = "Exception loading plugins: " + Environment.NewLine;
                foreach (var exp in e.LoaderExceptions)
                {
                    msg += exp.Message + Environment.NewLine;
                }
                msg += Environment.NewLine;
                msg += e.ToString();
                Console.WriteLine(msg);
                System.Diagnostics.Debug.Write(msg);
            }
        }

        internal void CallPlugins(frmCareer frmCareer)
        {
            foreach(var plugin in MyPlugins)
            {
                foreach (TabPage page in plugin.GetTabPages(frmCareer))
                {
                    if (page != null)
                    {
                        if (!frmCareer.TabCharacterTabs.TabPages.Contains(page))
                            frmCareer.TabCharacterTabs.TabPages.Add(page);
                    }
                }
            }
        }

        internal void CallPlugins(ToolStripMenuItem menu)
        {
            foreach (var plugin in MyPlugins)
            {
                foreach (ToolStripMenuItem plugInMenu in plugin.GetMenuItems(menu))
                {
                    if (plugInMenu != null)
                    {
                        if (!menu.DropDownItems.Contains(plugInMenu))
                            menu.DropDownItems.Add(plugInMenu);
                    }
                }
            }
        }

      
    }
}
