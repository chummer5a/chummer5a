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
using Microsoft.ApplicationInsights.Channel;
using NLog;

namespace Chummer.Plugins
{
    [InheritedExport(typeof(IPlugin))]
    public interface IPlugin
    {
        void CustomInitialize(frmChummerMain mainControl);

        IEnumerable<TabPage> GetTabPages(frmCareer input);
        IEnumerable<TabPage> GetTabPages(frmCreate input);
        IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem menu);
        ITelemetry SetTelemetryInitialize(ITelemetry telemetry);

        Task<IEnumerable<TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate);

        UserControl GetOptionsControl();

        string GetSaveToFileElement(Character input);
        void LoadFileElement(Character input, string fileElement);

        void SetIsUnitTest(bool isUnitTest);

        Assembly GetPluginAssembly();
    }


    public class PluginControl
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static CompositionContainer container = null;
        public static CompositionContainer Container { get { return container; } }
        public string PathToPlugins { get; set; }
        private static AggregateCatalog catalog;
        private static DirectoryCatalog myDirectoryCatalog = null;

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
                if (myDirectoryCatalog == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                    if (!Directory.Exists("Plugins"))
                    {
                        Log.Warn("Directory " + path + " not found. No Plugins will be available.");
                        MyPlugins = new List<IPlugin>();
                        return;
                    }
                    myDirectoryCatalog = new DirectoryCatalog(path: path, searchPattern: "*.dll");
                }
                
                catalog = new AggregateCatalog();
                Log.Info("Searching for dlls in path " + myDirectoryCatalog.FullPath);
                catalog.Catalogs.Add(myDirectoryCatalog);
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
                        Log.Error(e);
                    }
                }
                Log.Info("Initializing Plugins finished.");
            }
            catch(System.Security.SecurityException e)
            {
                string msg = "Well, the Plugin wanted to do something that requires Admin rights. Let's just ignore this: " + Environment.NewLine + Environment.NewLine;
                msg += e.ToString();
                Log.Warn(e, msg);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
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
                if (MyPlugins == null)
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

        internal void LoadPlugins(CustomActivity parentActivity)
        {
            try
            {
                using (var op_plugin = Timekeeper.StartSyncron("LoadPlugins", parentActivity, CustomActivity.OperationType.DependencyOperation, myDirectoryCatalog.FullPath))
                { 
                    this.Initialize();
                }
            }
            catch(System.Security.SecurityException e)
            {
                string msg = "Well, something went wrong probably because we are not Admins. Let's just ignore it and move on." + Environment.NewLine + Environment.NewLine;
                Console.WriteLine(msg + e.Message);
                System.Diagnostics.Trace.TraceWarning(msg + e.Message);
                return;
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
                Log.Warn(e, msg);
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
                Log.Error(e, msg);
            }
            catch (Exception e)
            {
                string msg = "Exception loading plugins: " + Environment.NewLine;
                msg += Environment.NewLine;
                msg += e.ToString();
                Log.Error(e, msg);
            }
        }

        internal void CallPlugins(frmCareer frmCareer, CustomActivity parentActivity)
        {
            foreach(var plugin in MyActivePlugins)
            {
                using (var op_plugin = Timekeeper.StartSyncron("load_plugin_GetTabPage_Career_" + plugin.ToString(),
                    parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
                {
                    var pages = plugin.GetTabPages(frmCareer);
                    if (pages == null)
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
        }

        internal void CallPlugins(frmCreate frmCreate, CustomActivity parentActivity)
        {
            foreach (var plugin in MyActivePlugins)
            {
                using (var op_plugin = Timekeeper.StartSyncron("load_plugin_GetTabPage_Create_" + plugin.ToString(), parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
                {
                    var pages = plugin.GetTabPages(frmCreate);
                    if (pages == null)
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
        }

        internal void CallPlugins(ToolStripMenuItem menu, CustomActivity parentActivity)
        {
            foreach (var plugin in MyActivePlugins)
            {
                using (var op_plugin = Timekeeper.StartSyncron("load_plugin_GetMenuItems_" + plugin.ToString(),
                    parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
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
}
