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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static CompositionContainer container = null;
        public static CompositionContainer Container => container;
        public string PathToPlugins { get; set; }
        private static AggregateCatalog catalog;
        private static DirectoryCatalog myDirectoryCatalog;

        public PluginControl()
        {
        }

        ~PluginControl()
        {
            foreach (var plugin in this.MyActivePlugins)
            {
                plugin.Dispose();
            }
        }

        //the level-argument is only to absolutely make sure to not spawn processes uncontrolled
        public static bool RegisterChummerProtocol()
        {
            string startupExe = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            RegistryKey key = Registry.ClassesRoot.OpenSubKey("Chummer"); //open myApp protocol's subkey
            bool reregisterKey = false;
            if (key != null)
            {
                if (key.GetValue(string.Empty)?.ToString() != "URL: Chummer Protocol")
                    reregisterKey = true;
                if (key.GetValue("URL Protocol")?.ToString() != string.Empty)
                    reregisterKey = true;
                key = key.OpenSubKey(@"shell\open\command");
                if (key == null)
                    reregisterKey = true;
                else
                {
                    if (key.GetValue(string.Empty)?.ToString() != startupExe + " " + "%1")
                        reregisterKey = true;
                    key.Close();
                }
            }
            else
            {
                reregisterKey = true;
            }

            if (reregisterKey == false)
            {
                Log.Info("Url Protocol Handler for Chummer was already registered!");
                return true;
            }

            try
            {
                System.AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
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
            RegistryKey Software = Registry.CurrentUser.OpenSubKey("Software");  //open myApp protocol's subkey
            // Just in case there's something super-weird going on
            if (Software == null)
            {
                try
                {
                    Software = Registry.CurrentUser.CreateSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                catch (System.UnauthorizedAccessException e)
                {
                    Log.Error(e);
                    return false;
                }
            }
            RegistryKey Classes = Software.OpenSubKey("Classes", true);
            if (Classes == null)
            {
                try
                {
                    Classes = Software.CreateSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                catch (System.UnauthorizedAccessException e)
                {
                    Log.Error(e);
                    return false;
                }
            }

            RegistryKey key = Classes.OpenSubKey("Chummer", true) //open myApp protocol's subkey
                              //if the protocol is not registered yet...we register it
                              ?? Classes.CreateSubKey("Chummer", RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(string.Empty, "URL: Chummer Protocol");
            key.SetValue("URL Protocol", string.Empty);

            RegistryKey shell = key.OpenSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadWriteSubTree)
                                ?? key.CreateSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
            shell.SetValue(string.Empty, myAppPath + " " + "%1");
            //%1 represents the argument - this tells windows to open this program with an argument / parameter
            shell.Close();
            key.Close();
            Classes.Close();
            Software.Close();
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
                    MyPlugins = new List<IPlugin>();
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
                    //search for a textfile, that tells me what dll to parse
                    string infofile = Path.Combine(plugindir, "plugin.txt");
                    if (File.Exists(infofile))
                    {
                        Log.Trace(infofile +  " found: parsing it!");

                        System.IO.StreamReader file = new System.IO.StreamReader(infofile);
                        string line;
                        while ((line = file.ReadLine()) != null)
                        {
                            string plugindll = Path.Combine(plugindir, line);
                            Log.Trace(infofile + " containes line: " + plugindll + " - trying to find it...");
                            if (File.Exists(plugindll))
                            {
                                FileInfo fi = new FileInfo(plugindll);
                                myDirectoryCatalog = new DirectoryCatalog(path: plugindir, searchPattern: fi.Name);
                                Log.Info("Searching for plugin-interface in dll: " + plugindll);
                                catalog.Catalogs.Add(myDirectoryCatalog);
                            }
                            else
                            {
                                Log.Warn("Could not find dll from " + infofile + ": " + plugindll); myDirectoryCatalog = new DirectoryCatalog(path: plugindir, searchPattern: "*.dll");
                                myDirectoryCatalog = new DirectoryCatalog(path: plugindir, searchPattern: "*.dll");
                                Log.Info("Searching for dlls in path " + myDirectoryCatalog?.FullPath);
                                catalog.Catalogs.Add(myDirectoryCatalog);
                            }
                        }
                        file.Close();
                    }
                    else
                    {
                        myDirectoryCatalog = new DirectoryCatalog(path: plugindir, searchPattern: "*.dll");
                        Log.Info("Searching for dlls in path " + myDirectoryCatalog?.FullPath);
                        catalog.Catalogs.Add(myDirectoryCatalog);
                    }
                }

                container = new CompositionContainer(catalog);

                //Fill the imports of this object
                StartWatch();
                container.ComposeParts(this);

                Log.Info("Plugins found: " + MyPlugins.Count());
                if (!MyPlugins.Any())
                {
                    throw new ArgumentException("No plugins found in " + path + ".");
                }
                Log.Info("Plugins active: " + MyActivePlugins.Count());
                foreach (var plugin in MyActivePlugins)
                {
                    try
                    {
                        Log.Info("Initializing Plugin " + plugin.ToString());
                        plugin.SetIsUnitTest(Utils.IsUnitTest);
                        plugin.CustomInitialize(Program.MainForm);
                    }
                    catch (ApplicationException e)
                    {
                        throw;
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
                if (e is ApplicationException)
                    throw;
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
                    if (!GlobalOptions.PluginsEnabledDic.TryGetValue(plugin.ToString(), out bool enabled) || enabled)
                        result.Add(plugin);
                }
                return result;
          }
        }


        private static void StartWatch()
        {
            var watcher = new FileSystemWatcher
            {
                Path = ".",
                NotifyFilter = NotifyFilters.LastWrite
            };
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
            foreach (DirectoryCatalog dCatalog in catalog.Catalogs.Cast<DirectoryCatalog>())
                dCatalog.Refresh();
        }

        internal void LoadPlugins(CustomActivity parentActivity)
        {
            try
            {
                using (var op_plugin = Timekeeper.StartSyncron("LoadPlugins", parentActivity,
                    CustomActivity.OperationType.DependencyOperation, myDirectoryCatalog?.FullPath))
                {
                    this.Initialize();
                }
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
            catch (ApplicationException e)
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


        public void Dispose()
        {
            foreach (var plugin in MyActivePlugins)
                plugin.Dispose();
        }
    }
}
