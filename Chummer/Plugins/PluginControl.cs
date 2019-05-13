using System;
using System.Collections.Concurrent;
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
        void CustomInitialize(frmChummerMain mainControl);

        IEnumerable<TabPage> GetTabPages(frmCareer input);
        IEnumerable<TabPage> GetTabPages(frmCreate input);
        IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem menu);

        Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate);

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
            try
            {
                if (GlobalOptions.PluginsEnabled == false)
                {
                    Log.Info("Plugins are globally disabled - exiting PluginControl.Initialize()");
                    return;
                }
                Log.Info("Plugins are globally enabled - entering PluginControl.Initialize()");
                catalog = new AggregateCatalog();
                var execat = new DirectoryCatalog(path: "Plugins", searchPattern: "*.exe");
                Log.Info("Searching for exes in path " + execat.FullPath);
                catalog.Catalogs.Add(execat);
                catalog.Catalogs.Add(new DirectoryCatalog(path: "Plugins", searchPattern: "*.dll"));
                container = new CompositionContainer(catalog);
                //Fill the imports of this object
                StartWatch();
                container.ComposeParts(this);

                Log.Info("Plugins found: " + MyPlugins.Count());
                Log.Info("Plugins active: " + MyActivePlugins.Count());
                foreach (var plugin in MyActivePlugins)
                {
                    try
                    {
                        Log.Info("Initializing Plugin " + plugin.ToString());
                        plugin.CustomInitialize(Program.MainForm);
                        plugin.SetIsUnitTest(Utils.IsUnitTest);
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                }
                Log.Info("Initializing Plugins finished.");
            }
            catch (Exception e)
            {
                Log.Exception(e);
                throw;
            }
            
        }

        [ImportMany(typeof(IPlugin))]
        public IEnumerable<IPlugin> MyPlugins { get; set; }

        public IEnumerable<IPlugin> MyActivePlugins
        { get
          {
                List<IPlugin> result = new List<IPlugin>();
                if (GlobalOptions.PluginsEnabled == false)
                    return result;
                var list = MyPlugins.ToList();
                foreach(var plugin in list)
                {
                    bool enabled = false;
                    if (GlobalOptions.PluginsEnabledDic.TryGetValue(plugin.ToString(), out enabled))
                    {
                        if (enabled)
                            result.Add(plugin);
                    }
                }
                return result;
          }
        }


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
                Log.Exception(e, msg);
            }
            catch (CompositionException e)
            {

                string msg = "Exception loading plugins: " + Environment.NewLine;

                foreach (var exp in e.Errors)
                {
                    msg += exp.Exception + Environment.NewLine;
                }

                msg += Environment.NewLine;
                msg += e.ToString();
                Log.Exception(e, msg);
            }
            catch (Exception e)
            {
                string msg = "Exception loading plugins: " + Environment.NewLine;

                msg += Environment.NewLine;
                msg += e.ToString();
                Log.Exception(e, msg);
            }
        }

        internal void CallPlugins(frmCareer frmCareer)
        {
            foreach(var plugin in MyActivePlugins)
            {
                var pages = plugin.GetTabPages(frmCareer);
                if(pages == null)
                    continue;
                foreach (TabPage page in pages)
                {
                    if (page != null)
                    {
                        if (!frmCareer.TabCharacterTabs.TabPages.Contains(page))
                            frmCareer.TabCharacterTabs.TabPages.Add(page);
                    }
                }
            }
        }

        internal void CallPlugins(frmCreate frmCreate)
        {
            foreach (var plugin in MyActivePlugins)
            {
                var pages = plugin.GetTabPages(frmCreate);
                if(pages == null)
                    continue;
                foreach (TabPage page in pages)
                {
                    if (page != null)
                    {
                        if (!frmCreate.TabCharacterTabs.TabPages.Contains(page))
                            frmCreate.TabCharacterTabs.TabPages.Add(page);
                    }
                }
            }
        }

        internal void CallPlugins(ToolStripMenuItem menu)
        {
            foreach (var plugin in MyActivePlugins)
            {
                var menuitems = plugin.GetMenuItems(menu);
                if (menuitems == null)
                    continue;
                foreach (ToolStripMenuItem plugInMenu in menuitems)
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
