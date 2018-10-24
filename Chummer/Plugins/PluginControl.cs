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

namespace Chummer.Plugins
{
    [InheritedExport(typeof(IPlugin))]
    public interface IPlugin
    {
        //UserControl GetUserControl(frmCareer input);
        //UserControl GetUserControl(frmCreate input);

        TabPage GetTabPage(frmCareer input);
        TabPage GetTabPage(frmCreate input);

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
                TabPage page = plugin.GetTabPage(frmCareer);
                //UserControl ucChild = plugin.GetUserControl(frmCareer);
                if (page != null)
                {
                    frmCareer.TabCharacterTabs.TabPages.Add(page);
                }
            }
        }
    }
}
