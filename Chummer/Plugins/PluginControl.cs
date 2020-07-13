/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Win32;
using NLog;

namespace Chummer.Plugins
{
    [InheritedExport(typeof(IPlugin))]
    public interface IPlugin
    {
        //only very rudimentary initialization should take place here. Make it QUICK.
        void CustomInitialize(frmChummerMain mainControl);

        IEnumerable<TabPage> GetTabPages(frmCareer input);
        IEnumerable<TabPage> GetTabPages(frmCreate input);
        IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem menu);
        ITelemetry SetTelemetryInitialize(ITelemetry telemetry);
        bool ProcessCommandLine(string parameter);

        Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate);
        UserControl GetOptionsControl();

        string GetSaveToFileElement(Character input);
        void LoadFileElement(Character input, string fileElement);

        void SetIsUnitTest(bool isUnitTest);

        Assembly GetPluginAssembly();
        void Dispose();
        bool SetCharacterRosterNode(TreeNode objNode);
        Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs dragEventArgs, TreeView treCharacterList);
    }


    public class PluginControl : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static CompositionContainer container;
        public static CompositionContainer Container => container;
        public string PathToPlugins { get; set; }
        private static AggregateCatalog catalog;
        private static DirectoryCatalog myDirectoryCatalog;
        private static FileSystemWatcher watcher;

        //the level-argument is only to absolutely make sure to not spawn processes uncontrolled
        public static bool RegisterChummerProtocol()
        {
            string startupExe = Assembly.GetEntryAssembly()?.Location;
            bool reregisterKey = false;
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("Chummer")) //open myApp protocol's subkey
            {
                if (key != null)
                {
                    if (key.GetValue(string.Empty)?.ToString() != "URL: Chummer Protocol")
                        reregisterKey = true;
                    else if (!string.IsNullOrEmpty(key.GetValue("URL Protocol")?.ToString()))
                        reregisterKey = true;
                    else
                    {
                        using (RegistryKey subkey = key.OpenSubKey(@"shell\open\command"))
                        {
                            if (subkey == null)
                                reregisterKey = true;
                            else if (subkey.GetValue(string.Empty)?.ToString() != startupExe + " " + "%1")
                                reregisterKey = true;
                        }
                    }
                }
                else
                {
                    reregisterKey = true;
                }
            }

            if (reregisterKey == false)
            {
                Log.Info("Url Protocol Handler for Chummer was already registered!");
                return true;
            }

            try
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                return RegisterMyProtocol(startupExe);
            }
            catch (System.Security.SecurityException se)
            {
                Log.Warn(se);
            }
            return true;
        }


        public static bool RegisterMyProtocol(string myAppPath)  //myAppPath = full path to your application
        {
            RegistryKey Software = null;
            RegistryKey Classes = null;
            try
            {
                Software = Registry.CurrentUser.OpenSubKey("Software"); //open myApp protocol's subkey
                // Just in case there's something super-weird going on
                if (Software == null)
                {
                    try
                    {
                        Software = Registry.CurrentUser.CreateSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Log.Error(e);
                        return false;
                    }

                    if (Software == null)
                    {
                        Log.Error("Software key was successfully accessed, but somehow the key is null.");
                        return false;
                    }
                }

                Classes = Software.OpenSubKey("Classes", true);
                if (Classes == null)
                {
                    try
                    {
                        Classes = Software.CreateSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Log.Error(e);
                        return false;
                    }

                    if (Classes == null)
                    {
                        Log.Error("Classes key was successfully accessed, but somehow the key is null.");
                        return false;
                    }
                }

                using (RegistryKey keyChummerKey = Classes.OpenSubKey("Chummer", true) //open myApp protocol's subkey
                                                   //if the protocol is not registered yet...we register it
                                                   ?? Classes.CreateSubKey("Chummer", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (keyChummerKey == null)
                    {
                        Log.Error("Chummer key was successfully accessed, but somehow the key is null.");
                        return false;
                    }

                    keyChummerKey.SetValue(string.Empty, "URL: Chummer Protocol");
                    keyChummerKey.SetValue("URL Protocol", string.Empty);

                    using (RegistryKey keyShellCommand = keyChummerKey.OpenSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadWriteSubTree)
                                                         ?? keyChummerKey.CreateSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (keyShellCommand == null)
                        {
                            Log.Error("Shell command key was successfully accessed, but somehow the key is null.");
                            return false;
                        }

                        keyShellCommand.SetValue(string.Empty, myAppPath + " " + "%1");
                        //%1 represents the argument - this tells windows to open this program with an argument / parameter
                    }
                }
            }
            finally
            {
                Classes?.Close();
                Software?.Close();
            }
            Log.Info("Url Protocol Handler for Chummer registered!");
            return true;
        }

        public void Initialize()
        {
            try
            {
                RegisterChummerProtocol();
                if (!GlobalOptions.PluginsEnabled)
                {
                    Log.Info("Plugins are globally disabled - exiting PluginControl.Initialize()");
                    return;
                }
                Log.Info("Plugins are globally enabled - entering PluginControl.Initialize()");

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                if (!Directory.Exists(path))
                {
                    string msg = "Directory " + path + " not found. No Plugins will be available.";
                    throw new ArgumentException(msg);
                }
                catalog = new AggregateCatalog();

                var plugindirectories = Directory.GetDirectories(path);
                if (!plugindirectories.Any())
                {
                    throw new ArgumentException("No Plugin-Subdirectories in " + path + " !");
                }

                foreach (var plugindir in plugindirectories)
                {
                    Log.Trace("Searching in " + plugindir + " for plugin.txt or dlls containing the interface.");
                    //search for a text file that tells me what dll to parse
                    string infofile = Path.Combine(plugindir, "plugin.txt");
                    if (File.Exists(infofile))
                    {
                        Log.Trace(infofile +  " found: parsing it!");

                        using (StreamReader file = new StreamReader(infofile))
                        {
                            string line;
                            while ((line = file.ReadLine()) != null)
                            {
                                string plugindll = Path.Combine(plugindir, line);
                                Log.Trace(infofile + " containes line: " + plugindll + " - trying to find it...");
                                if (File.Exists(plugindll))
                                {
                                    FileInfo fi = new FileInfo(plugindll);
                                    myDirectoryCatalog = new DirectoryCatalog(plugindir, fi.Name);
                                    Log.Info("Searching for plugin-interface in dll: " + plugindll);
                                    catalog.Catalogs.Add(myDirectoryCatalog);
                                }
                                else
                                {
                                    Log.Warn("Could not find dll from " + infofile + ": " + plugindll);
                                    myDirectoryCatalog = new DirectoryCatalog(plugindir, "*.dll");
                                    Log.Info("Searching for dlls in path " + myDirectoryCatalog?.FullPath);
                                    catalog.Catalogs.Add(myDirectoryCatalog);
                                }
                            }
                        }
                    }
                    else
                    {
                        myDirectoryCatalog = new DirectoryCatalog(plugindir, "*.dll");
                        Log.Info("Searching for dlls in path " + myDirectoryCatalog?.FullPath);
                        catalog.Catalogs.Add(myDirectoryCatalog);
                    }
                }

                container = new CompositionContainer(catalog);

                //Fill the imports of this object
                StartWatch();
                container.ComposeParts(this);

                Log.Info("Plugins found: " + MyPlugins.Count);
                if (MyPlugins.Count == 0)
                {
                    throw new ArgumentException("No plugins found in " + path + ".");
                }
                Log.Info("Plugins active: " + MyActivePlugins.Count);
                foreach (var plugin in MyActivePlugins)
                {
                    try
                    {
                        Log.Info("Initializing Plugin " + plugin);
                        plugin.SetIsUnitTest(Utils.IsUnitTest);
                        plugin.CustomInitialize(Program.MainForm);
                    }
                    catch (ApplicationException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
#if DEBUG
                        throw;
#endif
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
                if (e is ApplicationException)
                    throw;
                Log.Fatal(e);
                throw;
            }
        }

        [ImportMany(typeof(IPlugin))]
        public ICollection<IPlugin> MyPlugins { get; } = new List<IPlugin>();

        public ICollection<IPlugin> MyActivePlugins
        {
            get
            {
                List<IPlugin> result = new List<IPlugin>();
                if (GlobalOptions.PluginsEnabled == false)
                    return result;
                foreach(IPlugin plugin in MyPlugins)
                {
                    if (!GlobalOptions.PluginsEnabledDic.TryGetValue(plugin.ToString(), out bool enabled) || enabled)
                        result.Add(plugin);
                }
                return result;
            }
        }


        private static void StartWatch()
        {
            if (watcher == null)
            {
                watcher = new FileSystemWatcher
                {
                    Path = ".",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                watcher.Changed += (s, e) =>
                {
                    if (e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        Refresh();
                };
                watcher.EnableRaisingEvents = true;
            }
        }

        public static void Refresh()
        {
            foreach (DirectoryCatalog dCatalog in catalog.Catalogs.Cast<DirectoryCatalog>())
                dCatalog.Refresh();
        }

        internal void LoadPlugins(CustomActivity parentActivity)
        {
            try
            {
                using (_ = Timekeeper.StartSyncron("LoadPlugins", parentActivity,
                    CustomActivity.OperationType.DependencyOperation, myDirectoryCatalog?.FullPath))
                    Initialize();
            }
            catch (System.Security.SecurityException e)
            {
                string msg =
                    "Well, something went wrong probably because we are not Admins. Let's just ignore it and move on." +
                    Environment.NewLine + Environment.NewLine;
                Log.Warn(e, msg);
            }
            catch (ReflectionTypeLoadException e)
            {
                StringBuilder msg = new StringBuilder("Exception loading plugins: " + Environment.NewLine);
                foreach (var exp in e.LoaderExceptions)
                {
                    msg.AppendLine(exp.Message);
                }
                msg.AppendLine();
                msg.Append(e);
                Log.Warn(e, msg.ToString());
            }
            catch (CompositionException e)
            {
                StringBuilder msg = new StringBuilder("Exception loading plugins: " + Environment.NewLine);
                foreach (var exp in e.Errors)
                {
                    msg.AppendLine(exp.Exception.ToString());
                }
                msg.AppendLine();
                msg.Append(e);

                Log.Error(e, msg.ToString());
            }
            catch (ApplicationException)
            {
                throw;
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
            foreach(IPlugin plugin in MyActivePlugins)
            {
                using (_ = Timekeeper.StartSyncron("load_plugin_GetTabPage_Career_" + plugin,
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
                using (_ = Timekeeper.StartSyncron("load_plugin_GetTabPage_Create_" + plugin, parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
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
                using (_ = Timekeeper.StartSyncron("load_plugin_GetMenuItems_" + plugin,
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

        private bool _blnDisposed;

        public void Dispose()
        {
            if (_blnDisposed)
                return;

            foreach (IPlugin plugin in MyActivePlugins)
                plugin.Dispose();

            _blnDisposed = true;
        }
    }
}
