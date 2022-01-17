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
    public interface IPlugin : IDisposable
    {
        //only very rudimentary initialization should take place here. Make it QUICK.
        void CustomInitialize(ChummerMainForm mainControl);

        IEnumerable<TabPage> GetTabPages(CharacterCareer input);

        IEnumerable<TabPage> GetTabPages(CharacterCreate input);

        IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem menu);

        [CLSCompliant(false)]
#pragma warning disable CS3010 // CLS-compliant interfaces must have only CLS-compliant members
        ITelemetry SetTelemetryInitialize(ITelemetry telemetry);
#pragma warning restore CS3010 // CLS-compliant interfaces must have only CLS-compliant members

        bool ProcessCommandLine(string parameter);

        Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(CharacterRoster frmCharRoster, bool forceUpdate);

        UserControl GetOptionsControl();

        string GetSaveToFileElement(Character input);

        void LoadFileElement(Character input, string fileElement);

        void SetIsUnitTest(bool isUnitTest);

        Assembly GetPluginAssembly();

        bool SetCharacterRosterNode(TreeNode objNode);

        Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs dragEventArgs, TreeView treCharacterList);
    }

    public class PluginControl : IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private static CompositionContainer _container;
        public static CompositionContainer Container => _container;
        public string PathToPlugins { get; set; }
        private static AggregateCatalog _objCatalog;
        private static DirectoryCatalog _objMyDirectoryCatalog;
        private static FileSystemWatcher _objWatcher;

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
                            else if (subkey.GetValue(string.Empty)?.ToString() != startupExe + " %1")
                                reregisterKey = true;
                        }
                    }
                }
                else
                {
                    reregisterKey = true;
                }
            }

            if (!reregisterKey)
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
            RegistryKey objSoftware = null;
            RegistryKey objClasses = null;
            try
            {
                objSoftware = Registry.CurrentUser.OpenSubKey("Software"); //open myApp protocol's subkey
                // Just in case there's something super-weird going on
                if (objSoftware == null)
                {
                    try
                    {
                        objSoftware = Registry.CurrentUser.CreateSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Log.Error(e);
                        return false;
                    }

                    if (objSoftware == null)
                    {
                        Log.Error("Software key was successfully accessed, but somehow the key is null.");
                        return false;
                    }
                }

                objClasses = objSoftware.OpenSubKey("Classes", true);
                if (objClasses == null)
                {
                    try
                    {
                        objClasses = objSoftware.CreateSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Log.Error(e);
                        return false;
                    }

                    if (objClasses == null)
                    {
                        Log.Error("Classes key was successfully accessed, but somehow the key is null.");
                        return false;
                    }
                }

                using (RegistryKey keyChummerKey = objClasses.OpenSubKey("Chummer", true) //open myApp protocol's subkey
                                                                                       //if the protocol is not registered yet...we register it
                                                   ?? objClasses.CreateSubKey("Chummer", RegistryKeyPermissionCheck.ReadWriteSubTree))
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

                        keyShellCommand.SetValue(string.Empty, myAppPath + " %1");
                        //%1 represents the argument - this tells windows to open this program with an argument / parameter
                    }
                }
            }
            finally
            {
                objClasses?.Close();
                objSoftware?.Close();
            }
            Log.Info("Url Protocol Handler for Chummer registered!");
            return true;
        }

        public void Initialize()
        {
            try
            {
                RegisterChummerProtocol();
                if (!GlobalSettings.PluginsEnabled)
                {
                    Log.Info("Plugins are globally disabled - exiting PluginControl.Initialize()");
                    return;
                }
                Log.Info("Plugins are globally enabled - entering PluginControl.Initialize()");

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                path = Path.GetFullPath(path);
                if (!Directory.Exists(path))
                {
                    string msg = "Directory " + path + " not found. No Plugins will be available.";
                    throw new ArgumentException(msg);
                }
                _objCatalog = new AggregateCatalog();
                //delete old NeonJungleLC-Plugin
                string neon = Path.Combine(path, "NeonJungleLC");
                if (Directory.Exists(neon))
                    Directory.Delete(neon, true);
                string[] plugindirectories = Directory.GetDirectories(path);
                if (plugindirectories.Length == 0)
                {
                    throw new ArgumentException("No Plugin-Subdirectories in " + path + " !");
                }

                foreach (string plugindir in plugindirectories)
                {
                    Log.Trace("Searching in " + plugindir + " for plugin.txt or dlls containing the interface.");
                    //search for a text file that tells me what dll to parse
                    string infofile = Path.Combine(plugindir, "plugin.txt");
                    if (File.Exists(infofile))
                    {
                        Log.Trace(infofile + " found: parsing it!");

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
                                    _objMyDirectoryCatalog = new DirectoryCatalog(plugindir, fi.Name);
                                    Log.Info("Searching for plugin-interface in dll: " + plugindll);
                                    _objCatalog.Catalogs.Add(_objMyDirectoryCatalog);
                                }
                                else
                                {
                                    Log.Warn("Could not find dll from " + infofile + ": " + plugindll);
                                    _objMyDirectoryCatalog = new DirectoryCatalog(plugindir, "*.dll");
                                    Log.Info("Searching for dlls in path " + _objMyDirectoryCatalog?.FullPath);
                                    _objCatalog.Catalogs.Add(_objMyDirectoryCatalog);
                                }
                            }
                        }
                    }
                    else
                    {
                        _objMyDirectoryCatalog = new DirectoryCatalog(plugindir, "*.dll");
                        Log.Info("Searching for dlls in path " + _objMyDirectoryCatalog?.FullPath);
                        _objCatalog.Catalogs.Add(_objMyDirectoryCatalog);
                    }
                }

                _container = new CompositionContainer(_objCatalog);

                //Fill the imports of this object
                StartWatch();
                _container.ComposeParts(this);

                Log.Info("Plugins found: " + MyPlugins.Count);
                if (MyPlugins.Count == 0)
                {
                    throw new ArgumentException("No plugins found in " + path + ".");
                }
                Log.Info("Plugins active: " + MyActivePlugins.Count);
                foreach (IPlugin plugin in MyActivePlugins)
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
            catch (System.Security.SecurityException e)
            {
                string msg = "Well, the Plugin wanted to do something that requires Admin rights. Let's just ignore this: " + Environment.NewLine + Environment.NewLine;
                msg += e.ToString();
                Log.Warn(e, msg);
            }
            catch (Exception e) when (!(e is ApplicationException))
            {
                Log.Fatal(e);
                throw;
            }
        }

        [ImportMany(typeof(IPlugin))]
        public List<IPlugin> MyPlugins { get; } = new List<IPlugin>();

        public List<IPlugin> MyActivePlugins
        {
            get
            {
                List<IPlugin> result = new List<IPlugin>(MyPlugins.Count);
                if (!GlobalSettings.PluginsEnabled)
                    return result;
                foreach (IPlugin plugin in MyPlugins)
                {
                    if (!GlobalSettings.PluginsEnabledDic.TryGetValue(plugin.ToString(), out bool enabled) || enabled)
                        result.Add(plugin);
                }
                return result;
            }
        }

        private static void StartWatch()
        {
            if (_objWatcher == null)
            {
                _objWatcher = new FileSystemWatcher
                {
                    Path = ".",
                    NotifyFilter = NotifyFilters.LastWrite
                };
                _objWatcher.Changed += (s, e) =>
                {
                    if (e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        Refresh();
                };
                _objWatcher.EnableRaisingEvents = true;
            }
        }

        public static void Refresh()
        {
            foreach (DirectoryCatalog dCatalog in _objCatalog.Catalogs.Cast<DirectoryCatalog>())
                dCatalog.Refresh();
        }

        internal void LoadPlugins(CustomActivity parentActivity = null)
        {
            try
            {
                using (_ = Timekeeper.StartSyncron("LoadPlugins", parentActivity,
                                                   CustomActivity.OperationType.DependencyOperation,
                                                   _objMyDirectoryCatalog?.FullPath))
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
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdMessage))
                {
                    sbdMessage.AppendLine("Exception loading plugins: ");
                    foreach (Exception exp in e.LoaderExceptions)
                    {
                        sbdMessage.AppendLine(exp.Message);
                    }
                    sbdMessage.AppendLine();
                    sbdMessage.Append(e);
                    Log.Warn(e, sbdMessage.ToString());
                }
            }
            catch (CompositionException e)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdMessage))
                {
                    sbdMessage.AppendLine("Exception loading plugins: ");
                    foreach (CompositionError exp in e.Errors)
                    {
                        sbdMessage.AppendLine(exp.Exception.ToString());
                    }
                    sbdMessage.AppendLine();
                    sbdMessage.Append(e);
                    Log.Error(e, sbdMessage.ToString());
                }
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

        internal void CallPlugins(CharacterCareer frmCareer, CustomActivity parentActivity)
        {
            foreach (IPlugin plugin in MyActivePlugins)
            {
                using (_ = Timekeeper.StartSyncron("load_plugin_GetTabPage_Career_" + plugin,
                    parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
                {
                    IEnumerable<TabPage> pages = plugin.GetTabPages(frmCareer);
                    if (pages == null)
                        continue;
                    foreach (TabPage page in pages)
                    {
                        if (page != null && !frmCareer.TabCharacterTabs.TabPages.Contains(page))
                        {
                            frmCareer.TabCharacterTabs.TabPages.Add(page);
                        }
                    }
                }
            }
        }

        internal void CallPlugins(CharacterCreate frmCreate, CustomActivity parentActivity)
        {
            foreach (IPlugin plugin in MyActivePlugins)
            {
                using (_ = Timekeeper.StartSyncron("load_plugin_GetTabPage_Create_" + plugin, parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
                {
                    IEnumerable<TabPage> pages = plugin.GetTabPages(frmCreate);
                    if (pages == null)
                        continue;
                    foreach (TabPage page in pages)
                    {
                        if (page != null && !frmCreate.TabCharacterTabs.TabPages.Contains(page))
                        {
                            frmCreate.TabCharacterTabs.TabPages.Add(page);
                        }
                    }
                }
            }
        }

        internal void CallPlugins(ToolStripMenuItem menu, CustomActivity parentActivity)
        {
            foreach (IPlugin plugin in MyActivePlugins)
            {
                using (_ = Timekeeper.StartSyncron("load_plugin_GetMenuItems_" + plugin,
                    parentActivity, CustomActivity.OperationType.DependencyOperation, plugin.ToString()))
                {
                    IEnumerable<ToolStripMenuItem> menuitems = plugin.GetMenuItems(menu);
                    if (menuitems == null)
                        continue;
                    foreach (ToolStripMenuItem plugInMenu in menuitems)
                    {
                        if (plugInMenu != null && !menu.DropDownItems.Contains(plugInMenu))
                        {
                            menu.DropDownItems.Add(plugInMenu);
                        }
                    }
                }
            }
        }

        private bool _blnDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_blnDisposed)
                    return;

                _blnDisposed = true;

                foreach (IPlugin plugin in MyActivePlugins)
                    plugin.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
