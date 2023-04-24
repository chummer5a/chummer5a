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
using System.Threading;
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

        Task<ICollection<TabPage>> GetTabPages(CharacterCareer input, CancellationToken token = default);

        Task<ICollection<TabPage>> GetTabPages(CharacterCreate input, CancellationToken token = default);

        Task<ICollection<ToolStripMenuItem>> GetMenuItems(ToolStripMenuItem menu, CancellationToken token = default);

        [CLSCompliant(false)]
#pragma warning disable CS3010 // CLS-compliant interfaces must have only CLS-compliant members
        ITelemetry SetTelemetryInitialize(ITelemetry telemetry);
#pragma warning restore CS3010 // CLS-compliant interfaces must have only CLS-compliant members

        bool ProcessCommandLine(string parameter);

        Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(CharacterRoster frmCharRoster, bool forceUpdate, CancellationToken token = default);

        UserControl GetOptionsControl();

        string GetSaveToFileElement(Character input);

        void LoadFileElement(Character input, string fileElement);

        void SetIsUnitTest(bool isUnitTest);

        Assembly GetPluginAssembly();

        bool SetCharacterRosterNode(TreeNode objNode);

        Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs dragEventArgs, TreeView treCharacterList, CancellationToken token = default);
    }

    public class PluginControl : IHasLockObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private static CompositionContainer _container;
        public static CompositionContainer Container => _container;
        public string PathToPlugins { get; set; }
        private static AggregateCatalog _objCatalog;
        private static DirectoryCatalog _objMyDirectoryCatalog;
        private FileSystemWatcher _objWatcher;

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
            using (LockObject.EnterWriteLock())
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

                    string path = Path.Combine(Utils.GetStartupPath, "plugins");
                    path = Path.GetFullPath(path);
                    if (!Directory.Exists(path))
                    {
                        string msg = "Directory " + path + " not found. No Plugins will be available.";
                        throw new ArgumentException(msg);
                    }

                    _objCatalog = new AggregateCatalog();
                    //delete old NeonJungleLC-Plugin
                    Utils.SafeDeleteDirectory(Path.Combine(path, "NeonJungleLC"));

                    bool blnAnyPlugins = false;
                    foreach (string plugindir in Directory.EnumerateDirectories(path))
                    {
                        blnAnyPlugins = true;
                        if (plugindir.Contains("SamplePlugin", StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Warn(
                                "Found an old SamplePlugin (not maintaned anymore) and deleteing it to not mess with the plugin catalog composition.");
                            Utils.SafeDeleteDirectory(plugindir);
                            continue;
                        }

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

                    if (!blnAnyPlugins)
                    {
                        throw new ArgumentException("No Plugin-Subdirectories in " + path + " !");
                    }

                    _container = new CompositionContainer(_objCatalog);

                    //Fill the imports of this object
                    StartWatch();
                    try
                    {
                        _container.ComposeParts(this);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        if (Program.ChummerTelemetryClient != null)
                        {
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdLoaderExceptions))
                            {
                                int counter = 0;
                                foreach (Exception except in e.LoaderExceptions)
                                {
                                    counter++;
                                    sbdLoaderExceptions
                                        .AppendLine().Append("LoaderException ").Append(counter).Append(": ")
                                        .Append(except.Message);
                                    Program.ChummerTelemetryClient.TrackException(except);
                                }

                                Program.ChummerTelemetryClient.Flush();
                                string msg
                                    = "Plugins (at least not all of them) could not be loaded. Logs are uploaded to the ChummerDevs. Maybe ping one of the Devs on Discord and provide your Installation-id: "
                                      + Properties.Settings.Default.UploadClientId + Environment.NewLine + "Exception: "
                                      + Environment.NewLine + Environment.NewLine + e + Environment.NewLine
                                      + Environment.NewLine + "The LoaderExceptions are: " + Environment.NewLine
                                      + sbdLoaderExceptions + Environment.NewLine + Environment.NewLine;

                                Log.Info(e, msg);
                            }
                        }
                        else
                        {
                            Log.Error(
                                e,
                                "Plugins (at least not all of them) could not be loaded. Please allow logging to upload logs.");
                        }
                    }

                    if (MyPlugins.Count == 0)
                    {
                        throw new ArgumentException("No plugins found in " + path + '.');
                    }

                    IReadOnlyList<IPlugin> lstActivePlugins = MyActivePlugins;
                    Log.Info("Plugins found: " + MyPlugins.Count + Environment.NewLine + "Plugins active: "
                             + lstActivePlugins.Count);
                    foreach (IPlugin plugin in lstActivePlugins)
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
                    string msg
                        = "Well, the Plugin wanted to do something that requires Admin rights. Let's just ignore this: "
                          + Environment.NewLine + Environment.NewLine + e;
                    Log.Warn(e, msg);
                }
                catch (Exception e) when (!(e is ApplicationException))
                {
                    Log.Fatal(e);
                    throw;
                }
            }
        }

        [ImportMany(typeof(IPlugin))]
        public ThreadSafeList<IPlugin> MyPlugins
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstMyPlugins;
            }
        }

        public IReadOnlyList<IPlugin> MyActivePlugins
        {
            get
            {
                if (!GlobalSettings.PluginsEnabled)
                    return Array.Empty<IPlugin>();
                using (EnterReadLock.Enter(LockObject))
                {
                    List<IPlugin> result = new List<IPlugin>(MyPlugins.Count);
                    foreach (IPlugin plugin in MyPlugins)
                    {
                        if (!GlobalSettings.PluginsEnabledDic.TryGetValue(plugin.ToString(), out bool enabled)
                            || enabled)
                            result.Add(plugin);
                    }

                    return result;
                }
            }
        }

        public async ValueTask<IReadOnlyList<IPlugin>> GetMyActivePluginsAsync(CancellationToken token = default)
        {
            if (!GlobalSettings.PluginsEnabled)
                return Array.Empty<IPlugin>();
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                List<IPlugin> result = new List<IPlugin>(await MyPlugins.GetCountAsync(token).ConfigureAwait(false));
                await MyPlugins.ForEachAsync(async plugin =>
                {
                    (bool blnSuccess, bool blnEnabled)
                        = await GlobalSettings.PluginsEnabledDic.TryGetValueAsync(plugin.ToString(), token)
                                              .ConfigureAwait(false);
                    if (!blnSuccess || blnEnabled)
                        result.Add(plugin);
                }, token).ConfigureAwait(false);

                return result;
            }
        }

        private void StartWatch()
        {
            FileSystemWatcher objNewWatcher = new FileSystemWatcher
            {
                Path = ".",
                NotifyFilter = NotifyFilters.LastWrite
            };
            if (Interlocked.CompareExchange(ref _objWatcher, objNewWatcher, null) == null)
            {
                objNewWatcher.Changed += (s, e) =>
                {
                    if (e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        Refresh();
                };
                objNewWatcher.EnableRaisingEvents = true;
            }
            else
                objNewWatcher.Dispose();
        }

        public static void Refresh()
        {
            foreach (DirectoryCatalog dCatalog in _objCatalog.Catalogs.Cast<DirectoryCatalog>())
                dCatalog.Refresh();
        }

        internal void LoadPlugins(CustomActivity parentActivity = null)
        {
            using (LockObject.EnterWriteLock())
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
                        "Well, something went wrong probably because we are not Admins. Let's just ignore it and move on."
                        +
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

                        sbdMessage.AppendLine().Append(e);
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

                        sbdMessage.AppendLine().Append(e);
                        Log.Error(e, sbdMessage.ToString());
                    }
                }
                catch (ApplicationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    string msg = "Exception loading plugins: " + Environment.NewLine + Environment.NewLine + e;
                    Log.Error(e, msg);
                }
            }
        }

        internal async Task CallPlugins(CharacterCareer frmCareer, CustomActivity parentActivity, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (IPlugin plugin in await GetMyActivePluginsAsync(token).ConfigureAwait(false))
                {
                    using (_ = Timekeeper.StartSyncron("load_plugin_GetTabPage_Career_" + plugin,
                                                       parentActivity,
                                                       CustomActivity.OperationType.DependencyOperation,
                                                       plugin.ToString()))
                    {
                        ICollection<TabPage> pages = await plugin.GetTabPages(frmCareer, token).ConfigureAwait(false);
                        if (pages == null)
                            continue;
                        foreach (TabPage page in pages)
                        {
                            if (page != null)
                            {
                                await frmCareer.DoThreadSafeAsync(x =>
                                {
                                    if (!x.TabCharacterTabs.TabPages.Contains(page))
                                        x.TabCharacterTabs.TabPages.Add(page);
                                }, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        internal async Task CallPlugins(CharacterCreate frmCreate, CustomActivity parentActivity,
                                        CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (IPlugin plugin in await GetMyActivePluginsAsync(token).ConfigureAwait(false))
                {
                    using (_ = Timekeeper.StartSyncron("load_plugin_GetTabPage_Create_" + plugin, parentActivity,
                                                       CustomActivity.OperationType.DependencyOperation,
                                                       plugin.ToString()))
                    {
                        ICollection<TabPage> pages = await plugin.GetTabPages(frmCreate, token).ConfigureAwait(false);
                        if (pages == null)
                            continue;
                        foreach (TabPage page in pages)
                        {
                            if (page != null)
                            {
                                await frmCreate.DoThreadSafeAsync(x =>
                                {
                                    if (!x.TabCharacterTabs.TabPages.Contains(page))
                                        x.TabCharacterTabs.TabPages.Add(page);
                                }, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        internal async Task CallPlugins(ToolStripMenuItem menu, CustomActivity parentActivity,
                                        CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (IPlugin plugin in await GetMyActivePluginsAsync(token).ConfigureAwait(false))
                {
                    using (_ = Timekeeper.StartSyncron("load_plugin_GetMenuItems_" + plugin,
                                                       parentActivity,
                                                       CustomActivity.OperationType.DependencyOperation,
                                                       plugin.ToString()))
                    {
                        ICollection<ToolStripMenuItem> menuitems
                            = await plugin.GetMenuItems(menu, token).ConfigureAwait(false);
                        if (menuitems == null)
                            continue;
                        foreach (ToolStripMenuItem plugInMenu in menuitems)
                        {

                            if (plugInMenu != null)
                            {
                                await Utils.RunOnMainThreadAsync(() =>
                                {
                                    if (!menu.DropDownItems.Contains(plugInMenu))
                                        menu.DropDownItems.Add(plugInMenu);
                                }, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        private int _intIsDisposed;
        private readonly ThreadSafeList<IPlugin> _lstMyPlugins = new ThreadSafeList<IPlugin>(1);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IPlugin plugin in MyPlugins)
                    plugin.Dispose();
                MyPlugins.Dispose();

                Interlocked.Exchange(ref _objWatcher, null)?.Dispose();
                _container?.Dispose();
                _objCatalog?.Dispose();
                _objMyDirectoryCatalog?.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            using (LockObject.EnterWriteLock())
                Dispose(true);
            LockObject.Dispose();
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await MyPlugins.ForEachAsync(async plugin =>
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (plugin is IAsyncDisposable objAsyncPlugin)
                        await objAsyncPlugin.DisposeAsync().ConfigureAwait(false);
                    else
                        plugin.Dispose();
                }).ConfigureAwait(false);

                await MyPlugins.DisposeAsync().ConfigureAwait(false);

                Interlocked.Exchange(ref _objWatcher, null)?.Dispose();
                _container?.Dispose();
                _objCatalog?.Dispose();
                _objMyDirectoryCatalog?.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await DisposeAsync(true).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await LockObject.DisposeAsync().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
