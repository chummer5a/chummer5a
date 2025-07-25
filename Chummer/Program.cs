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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Backend;
using Chummer.Forms;
using Chummer.Plugins;
using Chummer.Properties;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Metrics;
using Microsoft.ApplicationInsights.NLogTarget;
using NLog;
using NLog.Config;

[assembly: CLSCompliant(true)]

namespace Chummer
{
    public static class Program
    {
        private static Logger Log;
        private const string ChummerGuid = "eb0759c1-3599-495e-8bc5-57c8b3e1b31c";

        private static readonly Lazy<Process> s_objMyProcess = new Lazy<Process>(Process.GetCurrentProcess);
        public static Process MyProcess => s_objMyProcess.Value;

        private static Lazy<TelemetryConfiguration> s_objActiveTelemetryConfiguration = new Lazy<TelemetryConfiguration>(TelemetryConfiguration.CreateDefault);
        [CLSCompliant(false)]
        public static TelemetryConfiguration ActiveTelemetryConfiguration => s_objActiveTelemetryConfiguration?.Value;

        private static readonly Lazy<CustomTelemetryInitializer> s_objTelemetryInitializer
            = new Lazy<CustomTelemetryInitializer>(() => new CustomTelemetryInitializer());

        [CLSCompliant(false)]
        public static Lazy<TelemetryClient> ChummerTelemetryClient { get; } = new Lazy<TelemetryClient>(
            () =>
            {
                if (IsMono
                    || Utils.IsUnitTest
                    || GlobalSettings.UseLoggingApplicationInsights == UseAILogging.OnlyLocal)
                    return null;

                TelemetryConfiguration objActiveConfiguration = ActiveTelemetryConfiguration;
                if (objActiveConfiguration == null)
                    return null;
                objActiveConfiguration.ConnectionString = "InstrumentationKey=012fd080-80dc-4c10-97df-4f2cf8c805d5;IngestionEndpoint=https://westeurope-0.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";
#if DEBUG
                //If you set true as DeveloperMode (see above), you can see the sending telemetry in the debugging output window in IDE.
                objActiveConfiguration.TelemetryChannel.DeveloperMode = true;
#else
                objActiveConfiguration.TelemetryChannel.DeveloperMode = false;
#endif
                objActiveConfiguration.TelemetryInitializers.Add(s_objTelemetryInitializer.Value);
                objActiveConfiguration.TelemetryProcessorChainBuilder.Use(next =>
                                                                              new TranslateExceptionTelemetryProcessor(
                                                                                  next));
                objActiveConfiguration.TelemetryProcessorChainBuilder.Use(next =>
                                                                              new DropUserdataTelemetryProcessor(
                                                                                  next, Environment.UserName));
                objActiveConfiguration.TelemetryProcessorChainBuilder.Build();
                return new TelemetryClient(objActiveConfiguration);
            });

        private static PluginControl _objPluginLoader;
        public static PluginControl PluginLoader => _objPluginLoader = _objPluginLoader ?? new PluginControl();

        internal static readonly IntPtr CommandLineArgsDataTypeId = (IntPtr)7593599;

        /// <summary>
        /// Check this to see if we are currently in the Main Thread.
        /// </summary>
        [ThreadStatic]
        // ReSharper disable once ThreadStaticFieldHasInitializer
        // ReSharper disable once ConvertToConstant.Global
#pragma warning disable CA2019
        public static readonly bool IsMainThread = true;
#pragma warning restore CA2019

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            // Set DPI Stuff before anything else
            SetProcessDPI(GlobalSettings.DpiScalingMethodSetting);
            if (IsMainThread)
                SetThreadDPI(GlobalSettings.DpiScalingMethodSetting);
            Utils.CreateSynchronizationContext();

            using (GlobalChummerMutex = new Mutex(false, @"Global\" + ChummerGuid, out bool blnIsNewInstance))
            {
                bool blnReleaseMutex = false;
                try
                {
                    try
                    {
                        if (blnIsNewInstance)
                            blnReleaseMutex
                                = Utils.RunOnMainThread(
                                    () => GlobalChummerMutex.WaitOne(TimeSpan.FromSeconds(2), false));
                        // Chummer instance already exists, so switch to it instead of opening a new instance
                        if (!blnIsNewInstance || !blnReleaseMutex)
                        {
                            // Try to get the main chummer process by fetching the Chummer process with the earliest start time
                            Process objMainChummerProcess = MyProcess;
                            foreach (Process objLoopProcess in Process.GetProcessesByName(MyProcess.ProcessName))
                            {
                                if (objLoopProcess.StartTime.Ticks < objMainChummerProcess.StartTime.Ticks)
                                    objMainChummerProcess = objLoopProcess;
                            }

                            if (objMainChummerProcess != MyProcess)
                            {
                                NativeMethods.SendMessage(objMainChummerProcess.MainWindowHandle,
                                                          NativeMethods.WM_SHOWME, 0, IntPtr.Zero);

                                string strCommandLineArgumentsJoined =
                                    string.Join("<|>", Environment.GetCommandLineArgs());
                                NativeMethods.CopyDataStruct objData = default;
                                IntPtr ptrCommandLineArguments = IntPtr.Zero;
                                try
                                {
                                    // Allocate memory for the data and copy
                                    objData = NativeMethods.CopyDataFromString(
                                        CommandLineArgsDataTypeId, strCommandLineArgumentsJoined);
                                    ptrCommandLineArguments = Marshal.AllocCoTaskMem(Marshal.SizeOf(objData));
                                    Marshal.StructureToPtr(objData, ptrCommandLineArguments, false);
                                    // Send the message
                                    NativeMethods.SendMessage(objMainChummerProcess.MainWindowHandle,
                                                              NativeMethods.WM_COPYDATA, 0, ptrCommandLineArguments);
                                }
                                finally
                                {
                                    // Free the allocated memory after the control has been returned
                                    if (ptrCommandLineArguments != IntPtr.Zero)
                                        Marshal.FreeCoTaskMem(ptrCommandLineArguments);
                                    if (objData.lpData != IntPtr.Zero)
                                        Marshal.FreeHGlobal(objData.lpData);
                                }
                            }

                            return;
                        }

                        //for some fun try out this command line parameter: chummer://plugin:SINners:Load:5ff55b9d-7d1c-4067-a2f5-774127346f4e
                        PageViewTelemetry pvt = null;
                        DateTimeOffset startTime = DateTimeOffset.UtcNow;
                        // Set default cultures based on the currently set language
                        CultureInfo.DefaultThreadCurrentCulture = GlobalSettings.CultureInfo;
                        CultureInfo.DefaultThreadCurrentUICulture = GlobalSettings.CultureInfo;
                        string strPostErrorMessage = string.Empty;
                        if (!Directory.Exists(Utils.GetSettingsFolderPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(Utils.GetSettingsFolderPath);
                            }
                            catch (Exception ex)
                            {
                                strPostErrorMessage = ex.ToString();
                            }
                        }
                        if (!Directory.Exists(Utils.GetPacksFolderPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(Utils.GetPacksFolderPath);
                            }
                            catch (Exception ex)
                            {
                                strPostErrorMessage = ex.ToString();
                            }
                        }

                        IsMono = Type.GetType("Mono.Runtime") != null;

                        string strInfo;

                        using (new FetchSafelyFromSafeObjectPool<Stopwatch>(Utils.StopwatchPool, out Stopwatch sw))
                        {
                            sw.Start();
                            //If debugging and launched from other place (Bootstrap), launch debugger
                            if (Environment.GetCommandLineArgs().Contains("/debug") && !Debugger.IsAttached)
                            {
                                Debugger.Launch();
                            }

                            sw.TaskEnd("dbgchk");
                            //Various init stuff (that mostly "can" be removed as they serve
                            //debugging more than function

                            //Needs to be called before Log is setup, as it moves where log might be.
                            FixCwd();

                            sw.TaskEnd("fixcwd");

                            AppDomain.CurrentDomain.FirstChanceException += ExceptionHeatMap.OnException;

                            sw.TaskEnd("appdomain 2");

                            strInfo =
                                string.Format(GlobalSettings.InvariantCultureInfo,
                                    "Application Chummer5a build {0} started at {1} with command line arguments {2}",
                                    Utils.CurrentChummerVersion, DateTime.UtcNow,
                                    Environment.CommandLine);
                            sw.TaskEnd("infogen");

                            sw.TaskEnd("infoprnt");

                            sw.TaskEnd("languagefreestartup");

                            if (!Utils.IsUnitTest)
                                AppDomain.CurrentDomain.UnhandledException += HandleCrash;

                            sw.TaskEnd("Startup");
                        }

                        void HandleCrash(object o, UnhandledExceptionEventArgs exa)
                        {
                            DateTime datCrashDateTime = DateTime.UtcNow;
                            if (!(exa.ExceptionObject is Exception ex))
                                return;
                            ex = ex.Demystify();
                            if (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.Crashes
                                && !Utils.IsMilestoneVersion)
                            {
                                TelemetryClient objLocalTelemetryClient = ChummerTelemetryClient.Value;
                                if (objLocalTelemetryClient != null)
                                {
                                    try
                                    {
                                        ExceptionTelemetry et = new ExceptionTelemetry(ex)
                                        {
                                            SeverityLevel = SeverityLevel.Critical
                                        };
                                        //we have to enable the uploading of THIS message, so it isn't filtered out in the DropUserdataTelemetryProcessos
                                        foreach (DictionaryEntry d in ex.Data)
                                        {
                                            object objValue = d.Value;
                                            if (objValue != null)
                                            {
                                                object objKey = d.Key;
                                                et.Properties.Add(objKey.ToString(), objValue.ToString());
                                            }
                                        }

                                        et.Properties.Add("IsCrash", exa.IsTerminating.ToString());
                                        s_objTelemetryInitializer.Value.Initialize(et);

                                        objLocalTelemetryClient.TrackException(et);
                                        try
                                        {
                                            using (CancellationTokenSource objTimeout
                                                   = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                                            {
                                                CancellationToken objTimeoutToken = objTimeout.Token;
                                                Utils.SafelyRunSynchronously(
                                                    () => objLocalTelemetryClient.FlushAsync(objTimeoutToken),
                                                    objTimeoutToken);
                                            }
                                        }
                                        catch (OperationCanceledException)
                                        {
                                            //swallow this, we timed out on the flush
                                            Utils.BreakIfDebug();
                                        }
                                    }
                                    catch (Exception ex1)
                                    {
                                        Log.Error(ex1);
                                    }
                                }
                            }
#if DEBUG
                            if (!Debugger.IsAttached)
                                Debugger.Launch();
#endif
                            Utils.BreakIfDebug();
                            CrashHandler.WebMiniDumpHandler(ex, datCrashDateTime);
                        }

                        // Delete old ProfileOptimization file because we don't want it anymore, instead we restart profiling for each newly generated assembly
                        FileExtensions.SafeDelete(Path.Combine(Utils.GetStartupPath, "chummerprofile"));
                        // We avoid weird issues with ProfileOptimization pointing JIT to the wrong place by checking for and removing all profile optimization files that
                        // were made in an older version (i.e. an older assembly)
                        string strProfileOptimizationName
                            = "chummerprofile_" + Utils.CurrentChummerVersion + ".profile";
                        foreach (string strProfileFile in Directory.GetFiles(Utils.GetStartupPath, "*.profile"))
                        {
                            if (!string.Equals(strProfileFile, strProfileOptimizationName,
                                               StringComparison.OrdinalIgnoreCase))
                                FileExtensions.SafeDelete(strProfileFile);
                        }

                        // Mono, non-Windows native stuff, and Win11 don't always play nice with ProfileOptimization, so it's better to just not bother with it when running under them
                        if (!IsMono && Utils.HumanReadableOSVersion.StartsWith(
                                        "Windows", StringComparison.OrdinalIgnoreCase)
                                    && !Utils.HumanReadableOSVersion.StartsWith(
                                        "Windows 11", StringComparison.OrdinalIgnoreCase))
                        {
                            ProfileOptimization.SetProfileRoot(Utils.GetStartupPath);
                            ProfileOptimization.StartProfile(strProfileOptimizationName);
                        }

                        if (!string.IsNullOrEmpty(LanguageManager.ManagerErrorMessage))
                        {
                            // MainForm is null at the moment, so we have to show error box manually
                            MessageBox.Show(LanguageManager.ManagerErrorMessage, Application.ProductName,
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (!string.IsNullOrEmpty(GlobalSettings.ErrorMessage))
                        {
                            // MainForm is null at the moment, so we have to show error box manually
                            MessageBox.Show(GlobalSettings.ErrorMessage, Application.ProductName, MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                            return;
                        }

                        if (!string.IsNullOrEmpty(strPostErrorMessage))
                        {
                            // MainForm is null at the moment, so we have to show error box manually
                            MessageBox.Show(strPostErrorMessage, Application.ProductName, MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                            return;
                        }

                        try
                        {
                            LogManager.ThrowExceptions = true;
                            if (IsMono)
                            {
                                //Mono Crashes because of Application Insights. Set Logging to local, when Mono Runtime is detected
                                GlobalSettings.UseLoggingApplicationInsights = UseAILogging.OnlyLocal;
                            }

                            if (GlobalSettings.UseLoggingApplicationInsights > UseAILogging.OnlyMetric)
                            {
                                LogManager.Setup()
                                          .SetupExtensions(
                                              ext => ext.RegisterTarget<ApplicationInsightsTarget>(
                                                  "ApplicationInsightsTarget"));
                            }

                            LogManager.ThrowExceptions = false;
                            Log = LogManager.GetCurrentClassLogger();
                            if (GlobalSettings.UseLogging)
                            {
                                foreach (LoggingRule objRule in LogManager.Configuration.LoggingRules)
                                {
#if DEBUG
                                    //enable logging to EventLog when Debugging
                                    if (objRule.Levels.Count == 0 && objRule.RuleName == "ELChummer")
                                        objRule.EnableLoggingForLevels(LogLevel.Trace, LogLevel.Fatal);
#endif
                                    //only change the loglevel, if it's off - otherwise it has been changed manually
                                    if (objRule.Levels.Count == 0)
                                        objRule.EnableLoggingForLevels(LogLevel.Debug, LogLevel.Fatal);
                                }
                            }

                            if (Settings.Default.UploadClientId == Guid.Empty)
                            {
                                Settings.Default.UploadClientId = Guid.NewGuid();
                                Settings.Default.Save();
                            }

                            if (!Utils.IsUnitTest
                                && GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.OnlyMetric)
                            {
                                TelemetryClient objLocalTelemetryClient = ChummerTelemetryClient.Value;
                                if (objLocalTelemetryClient != null)
                                {
                                    //for now lets disable live view.We may make another GlobalOption to enable it at a later stage...
                                    //var live = new LiveStreamProvider(ApplicationInsightsConfig);
                                    //live.Enable();

                                    //Log an Event with AssemblyVersion and CultureInfo
                                    MetricIdentifier objMetricIdentifier = new MetricIdentifier(
                                        "Chummer", "Program Start",
                                        "Version", "Culture", "AISetting", "OSVersion");
                                    string strOSVersion = Utils.HumanReadableOSVersion;
                                    Metric objMetric = objLocalTelemetryClient.GetMetric(objMetricIdentifier);
                                    objMetric.TrackValue(1,
                                                         Utils.CurrentChummerVersion.ToString(),
                                                         CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                                                         GlobalSettings.UseLoggingApplicationInsights.ToString(),
                                                         strOSVersion);

                                    //Log a page view:
                                    pvt = new PageViewTelemetry("frmChummerMain()")
                                    {
                                        Name = "Chummer Startup: " +
                                               Utils.CurrentChummerVersion,
                                        Id = Settings.Default.UploadClientId.ToString(),
                                        Timestamp = startTime
                                    };
                                    pvt.Context.Operation.Name = "Operation Program.Main()";
                                    pvt.Properties.Add("parameters", Environment.CommandLine);

                                    UploadObjectAsMetric.UploadObject(objLocalTelemetryClient, typeof(GlobalSettings));
                                }
                            }

                            Log.Info(strInfo);
                            Log.Info("Logging options are set to " + GlobalSettings.UseLogging +
                                     " and Upload-Options are set to "
                                     + GlobalSettings.UseLoggingApplicationInsights + " (Installation-Id: "
                                     + Settings.Default.UploadClientId.ToString("D",
                                         GlobalSettings.InvariantCultureInfo) + ").");

                            //make sure the Settings are upgraded/preserved after an upgrade
                            //see for details: https://stackoverflow.com/questions/534261/how-do-you-keep-user-config-settings-across-different-assembly-versions-in-net/534335#534335
                            if (Settings.Default.UpgradeRequired)
                            {
                                if (UnblockPath(AppDomain.CurrentDomain.BaseDirectory))
                                {
                                    Settings.Default.Upgrade();
                                    Settings.Default.UpgradeRequired = false;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Warn("Files could not be unblocked in "
                                             + AppDomain.CurrentDomain.BaseDirectory);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Log.Error(e);
#if DEBUG
                            throw;
#endif
                        }

                        //load the plugins and maybe work of any command line arguments
                        //arguments come in the form of
                        //              /plugin:Name:Parameter:Argument
                        //              /plugin:SINners:RegisterUriScheme:0
                        bool showMainForm = !Utils.IsUnitTest;
                        bool blnRestoreDefaultLanguage;
                        try
                        {
                            // Make sure the default language has been loaded before attempting to open the Main Form.
                            blnRestoreDefaultLanguage = !LanguageManager.LoadLanguage(GlobalSettings.Language);
                        }
                        // This to catch and handle an extremely strange issue where Chummer tries to load a language it shouldn't and ends up
                        // dereferencing a null value that should be impossible by static code analysis. This code here is a failsafe so that
                        // it at least keeps working in English instead of crashing.
                        catch (NullReferenceException)
                        {
                            Utils.BreakIfDebug();
                            blnRestoreDefaultLanguage = true;
                        }

                        // Restore Chummer's language to en-US if we failed to load the default one.
                        if (blnRestoreDefaultLanguage)
                            GlobalSettings.Language = GlobalSettings.DefaultLanguage;

                        OpenCharacters.BeforeClearCollectionChangedAsync += OpenCharactersOnBeforeClearCollectionChanged;
                        OpenCharacters.CollectionChangedAsync += OpenCharactersOnCollectionChanged;

                        MainForm = new ChummerMainForm();
                        try
                        {
                            PluginLoader.LoadPlugins();
                        }
                        catch (ApplicationException)
                        {
                            showMainForm = false;
                        }

                        if (!Utils.IsUnitTest)
                        {
                            string[] strArgs = Environment.GetCommandLineArgs();
                            try
                            {
                                // Process plugin args synchronously because plugin load order can end up mattering
                                foreach (string strArg in strArgs)
                                {
                                    if (!strArg.Contains("/plugin"))
                                        continue;
                                    if (!GlobalSettings.PluginsEnabled)
                                    {
                                        const string strMessage =
                                            "Please enable Plugins to use command-line arguments invoking specific plugin-functions!";
                                        Log.Warn(strMessage);
                                        ShowScrollableMessageBox(strMessage, "Plugins not enabled", icon: MessageBoxIcon.Exclamation);
                                    }
                                    else
                                    {
                                        string strWhatPlugin =
                                            strArg.Substring(strArg.IndexOf("/plugin", StringComparison.Ordinal) + 8);
                                        //some external apps choose to add a '/' before a ':' even in the middle of an url...
                                        strWhatPlugin = strWhatPlugin.TrimStart(':');
                                        int intEndPlugin = strWhatPlugin.IndexOf(':');
                                        string strParameter = strWhatPlugin.Substring(intEndPlugin + 1);
                                        strWhatPlugin = strWhatPlugin.Substring(0, intEndPlugin);
                                        IPlugin objActivePlugin =
                                            PluginLoader.MyActivePlugins.Find(a => a.ToString() == strWhatPlugin);
                                        if (objActivePlugin == null)
                                        {
                                            if (PluginLoader.MyPlugins.All(a => a.ToString() != strWhatPlugin))
                                            {
                                                string strMessage =
                                                    "Plugin " + strWhatPlugin + " is not enabled in the options!" +
                                                    Environment.NewLine
                                                    + "If you want to use command-line arguments, please enable this plugin and restart the program.";
                                                Log.Warn(strMessage);
                                                ShowScrollableMessageBox(strMessage, strWhatPlugin + " not enabled",
                                                                         MessageBoxButtons.OK,
                                                                         MessageBoxIcon.Exclamation);
                                            }
                                        }
                                        else
                                        {
                                            showMainForm &= objActivePlugin.ProcessCommandLine(strParameter);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionTelemetry ex = new ExceptionTelemetry(e)
                                {
                                    SeverityLevel = SeverityLevel.Warning
                                };
                                ChummerTelemetryClient.Value?.TrackException(ex);
                                Log.Warn(e);
                            }
                        }

                        // Delete the old executable if it exists (created by the update process).
                        Utils.SafeClearDirectory(Utils.GetStartupPath, "*.old");
                        // Purge the temporary directory
                        Utils.SafeClearDirectory(Utils.GetTempPath());
                        // Fix any misplaced custom data files
                        Utils.MoveMisplacedCustomDataFiles();

                        if (showMainForm)
                        {
                            // Attempt to cache all XML files that are used the most.
                            using (Timekeeper.StartSyncron("cache_load", null,
                                                           CustomActivity.OperationType.DependencyOperation,
                                                           Utils.CurrentChummerVersion.ToString(3)))
                            using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                   = CreateAndShowProgressBar(Application.ProductName, Utils.BasicDataFileNames.Count))
                            {
                                List<Task> lstCachingTasks = new List<Task>(Utils.MaxParallelBatchSize);
                                int intCounter = 0;
                                foreach (string strLoopFile in Utils.BasicDataFileNames)
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    lstCachingTasks.Add(
                                        Task.Run(() => CacheCommonFile(strLoopFile, frmLoadingBar.MyForm)));
                                    if (++intCounter != Utils.MaxParallelBatchSize)
                                        continue;
                                    Utils.RunWithoutThreadLock(() => Task.WhenAll(lstCachingTasks));
                                    lstCachingTasks.Clear();
                                    intCounter = 0;
                                }

                                Utils.RunWithoutThreadLock(() => Task.WhenAll(lstCachingTasks));

                                async Task CacheCommonFile(string strFile, LoadingBar frmLoadingBarInner)
                                {
                                    // Load default language data first for performance reasons
                                    if (!GlobalSettings.Language.Equals(
                                            GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                                    {
                                        await XmlManager.LoadXPathAsync(strFile, null, GlobalSettings.DefaultLanguage)
                                                        .ConfigureAwait(false);
                                    }

                                    await XmlManager.LoadXPathAsync(strFile).ConfigureAwait(false);
                                    if (strFile == "settings.xml")
                                    {
                                        _ = await SettingsManager.GetLoadedCharacterSettingsAsync().ConfigureAwait(false);
                                    }
                                    await frmLoadingBarInner.PerformStepAsync(
                                        Application.ProductName,
                                        LoadingBar.ProgressBarTextPatterns.Initializing).ConfigureAwait(false);
                                }
                            }

                            MainForm.MyStartupPvt = pvt;
                            Application.Run(MainForm);
                        }

                        OpenCharacters.Clear();
                        OpenCharacters.BeforeClearCollectionChangedAsync -= OpenCharactersOnBeforeClearCollectionChanged;
                        OpenCharacters.CollectionChangedAsync -= OpenCharactersOnCollectionChanged;

                        PluginLoader?.Dispose();
                        Log.Info(ExceptionHeatMap.GenerateInfo());
                        TelemetryClient objTelemetryClient = ChummerTelemetryClient.Value;
                        if (objTelemetryClient != null)
                        {
                            try
                            {
                                // Note: Flush is now synchronous and blocking. If we want to flush without blocking or with a timeout (like here), we need FlushAsync instead
                                using (CancellationTokenSource objTimeout
                                       = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                                {
                                    CancellationToken objTimeoutToken = objTimeout.Token;
                                    Utils.SafelyRunSynchronously(() => objTelemetryClient.FlushAsync(objTimeoutToken),
                                                                 objTimeoutToken);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this, we timed out on the flush
                                Utils.BreakIfDebug();
                            }
                        }
                    }
                    finally
                    {
                        // Manually dispose of the active telemetry configuration to also flush its metric managers
                        Lazy<TelemetryConfiguration> objOldConfiguration
                            = Interlocked.Exchange(ref s_objActiveTelemetryConfiguration, null);
                        if (objOldConfiguration?.IsValueCreated == true)
                            objOldConfiguration.Value.Dispose();
                    }
                }
                finally
                {
                    if (blnReleaseMutex)
                        Utils.RunOnMainThread(() => GlobalChummerMutex.ReleaseMutex());
                }
            }
        }

        private static bool UnblockPath(string strPath)
        {
            bool blnAllUnblocked = true;

            foreach (string strFile in Directory.EnumerateFiles(strPath))
            {
                if (!UnblockFile(strFile))
                {
                    // Get the last error and display it.
                    int intError = Marshal.GetLastWin32Error();
                    Win32Exception exception = new Win32Exception(intError, "Error while unblocking " + strFile + '.');
                    switch (exception.NativeErrorCode)
                    {
                        //file not found - that means the alternate data-stream is not present.
                        case 2:
                            break;

                        case 5:
                            Log.Warn(exception);
                            blnAllUnblocked = false;
                            break;

                        default:
                            Log.Error(exception);
                            blnAllUnblocked = false;
                            break;
                    }
                }
            }

            foreach (string strDir in Directory.EnumerateDirectories(strPath))
            {
                if (!UnblockPath(strDir))
                    blnAllUnblocked = false;
            }

            return blnAllUnblocked;
        }

        private static bool UnblockFile(string strFileName)
        {
            return NativeMethods.DeleteFile(strFileName + ":Zone.Identifier");
        }

        public static void SetProcessDPI(DpiScalingMethod eMethod)
        {
            OperatingSystem objOSInfo = Environment.OSVersion;
            Version objOSInfoVersion = objOSInfo.Version;
            switch (eMethod)
            {
                case DpiScalingMethod.None:
                    if (objOSInfo.Platform == PlatformID.Win32NT && objOSInfoVersion >= new ValueVersion(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (objOSInfoVersion >= new ValueVersion(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext
                        {
                            NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.ContextDpiAwareness.Unaware);
                            if (Marshal.GetLastWin32Error() != 0)
                                NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.Unaware);
                        }
                        else
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.Unaware);
                    }
                    else
                        return;
                    break;
                // System
                case DpiScalingMethod.Zoom:
                    if (objOSInfo.Platform == PlatformID.Win32NT && objOSInfoVersion >= new ValueVersion(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (objOSInfoVersion >= new ValueVersion(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext
                        {
                            NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.ContextDpiAwareness.System);
                            if (Marshal.GetLastWin32Error() != 0)
                            {
                                NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.System);
                                if (Marshal.GetLastWin32Error() != 0)
                                    NativeMethods.SetProcessDPIAware();
                            }
                        }
                        else
                        {
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.System);
                            if (Marshal.GetLastWin32Error() != 0)
                                NativeMethods.SetProcessDPIAware();
                        }
                    }
                    else
                        NativeMethods.SetProcessDPIAware();
                    break;
                // PerMonitor/PerMonitorV2
                case DpiScalingMethod.Rescale:
                    if (objOSInfo.Platform == PlatformID.Win32NT && objOSInfoVersion >= new ValueVersion(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (objOSInfoVersion >= new ValueVersion(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext and PerMonitorV2
                        {
                            NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.ContextDpiAwareness.PerMonitorV2);
                            if (Marshal.GetLastWin32Error() != 0)
                            {
                                NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.PerMonitor);
                                if (Marshal.GetLastWin32Error() != 0)
                                    NativeMethods.SetProcessDPIAware();
                            }
                        }
                        else
                        {
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.PerMonitor);
                            if (Marshal.GetLastWin32Error() != 0)
                                NativeMethods.SetProcessDPIAware();
                        }
                    }
                    else
                        NativeMethods.SetProcessDPIAware(); // System as backup, because it's better than remaining unaware if we want PerMonitor/PerMonitorV2
                    break;
                // System (Enhanced)
                case DpiScalingMethod.SmartZoom:
                    if (objOSInfo.Platform == PlatformID.Win32NT && objOSInfoVersion >= new ValueVersion(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (objOSInfoVersion >= new ValueVersion(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext
                        {
                            NativeMethods.SetProcessDpiAwarenessContext(
                                objOSInfoVersion >= new ValueVersion(10, 0, 17763)
                                    ? NativeMethods.ContextDpiAwareness.UnawareGdiScaled // Windows 10 Version 1809 Added GDI+ Scaling
                                    : NativeMethods.ContextDpiAwareness.System); // System as backup, because it's better than remaining unaware if we want GDI+ Scaling
                            if (Marshal.GetLastWin32Error() != 0)
                            {
                                NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.System);
                                if (Marshal.GetLastWin32Error() != 0)
                                    NativeMethods.SetProcessDPIAware();
                            }
                        }
                        else
                        {
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.System);
                            if (Marshal.GetLastWin32Error() != 0)
                                NativeMethods.SetProcessDPIAware();
                        }
                    }
                    else
                        NativeMethods.SetProcessDPIAware(); // System as backup, because it's better than remaining unaware if we want GDI+ Scaling
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eMethod), eMethod, null);
            }

            Utils.BreakOnErrorIfDebug();
        }

        public static void SetThreadDPI(DpiScalingMethod eMethod)
        {
            OperatingSystem objOSInfo = Environment.OSVersion;
            if (objOSInfo.Platform != PlatformID.Win32NT)
            {
                SetProcessDPI(eMethod);
                return;
            }
            Version objOSInfoVersion = objOSInfo.Version;
            // Windows 10 Creators Update added SetThreadDpiAwarenessContext
            if (objOSInfoVersion < new ValueVersion(10, 0, 15063))
            {
                SetProcessDPI(eMethod);
                return;
            }

            switch (eMethod)
            {
                case DpiScalingMethod.None:
                    NativeMethods.SetThreadDpiAwarenessContext(NativeMethods.ContextDpiAwareness.Unaware);
                    break;
                // System
                case DpiScalingMethod.Zoom:
                    NativeMethods.SetThreadDpiAwarenessContext(NativeMethods.ContextDpiAwareness.System);
                    break;
                // PerMonitor/PerMonitorV2
                case DpiScalingMethod.Rescale:
                    NativeMethods.SetThreadDpiAwarenessContext(NativeMethods.ContextDpiAwareness.PerMonitorV2);
                    break;
                // System (Enhanced)
                case DpiScalingMethod.SmartZoom:
                    NativeMethods.SetThreadDpiAwarenessContext(
                        objOSInfoVersion >= new ValueVersion(10, 0, 17763)
                            ? NativeMethods.ContextDpiAwareness.UnawareGdiScaled // Windows 10 Version 1809 Added GDI+ Scaling
                            : NativeMethods.ContextDpiAwareness.System); // System as backup, because it's better than remaining unaware if we want GDI+ Scaling
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eMethod), eMethod, null);
            }

            Utils.BreakOnErrorIfDebug();
        }

        private static ChummerMainForm _frmMainForm;

        /// <summary>
        /// Main application form.
        /// </summary>
        public static ChummerMainForm MainForm
        {
            get => _frmMainForm;
            set
            {
                if (Interlocked.Exchange(ref _frmMainForm, value) == value)
                    return;
                if (value == null)
                    return;
                // Set up this way instead of using foreach because on-assign actions can add more on-assign actions
                int i = 0;
                int j = 0;
                while (i < MainFormOnAssignActions.Count || j < MainFormOnAssignAsyncActions.Count)
                {
                    for (; i < MainFormOnAssignActions.Count; ++i)
                        MainFormOnAssignActions[i](value);
                    for (; j < MainFormOnAssignAsyncActions.Count; ++j)
                    {
                        int j1 = j;
                        Utils.SafelyRunSynchronously(() => MainFormOnAssignAsyncActions[j1](value));
                    }
                }
                MainFormOnAssignActions.Clear();
                MainFormOnAssignAsyncActions.Clear();
            }
        }

#if DEBUG
        private static bool _blnShowDevWarningAboutDebuggingOnlyOnce = true;
#endif

        /// <summary>
        /// Shows a dialog box with vertical scrollbars for text that is too long centered on the Chummer main form window, or otherwise queues up such a box to be displayed
        /// </summary>
        public static DialogResult ShowScrollableMessageBox(string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            return ShowScrollableMessageBox(null, message, caption, buttons, icon, defaultButton);
        }

        /// <summary>
        /// Shows a dialog box with vertical scrollbars for text that is too long centered on the a window containing a WinForms control, or otherwise queues up such a box to be displayed
        /// </summary>
        public static DialogResult ShowScrollableMessageBox(Control owner, string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            if (Utils.IsUnitTest)
            {
                if (icon == MessageBoxIcon.Error || buttons != MessageBoxButtons.OK)
                {
                    Utils.BreakIfDebug();
                    string strMessage = "We don't want to see MessageBoxes in Unit Tests!" + Environment.NewLine +
                                        "Caption: " + caption + Environment.NewLine + "Message: " + message;
                    throw new InvalidOperationException(strMessage);
                }
                return DialogResult.OK;
            }
            Form frmOwnerForm = owner as Form ?? owner?.FindForm();
            if (frmOwnerForm.IsNullOrDisposed())
            {
                frmOwnerForm = TopMostLoadingBar;
                if (frmOwnerForm.IsNullOrDisposed())
                {
                    frmOwnerForm = MainForm;
                }
            }

            if (frmOwnerForm != null)
            {
#if DEBUG
                if (frmOwnerForm.InvokeRequired && _blnShowDevWarningAboutDebuggingOnlyOnce && Debugger.IsAttached)
                {
                    _blnShowDevWarningAboutDebuggingOnlyOnce = false;
                    //it works on my installation even in the debugger, so maybe we can ignore that...
                    //WARNING from the link above (you can edit that out if it's not causing problem):
                    //
                    //BUT ALSO KEEP IN MIND: when debugging a multi-threaded GUI app, and you're debugging in a thread
                    //other than the main/application thread, YOU NEED TO TURN OFF
                    //the "Enable property evaluation and other implicit function calls" option, or else VS will
                    //automatically fetch the values of local/global GUI objects FROM THE CURRENT THREAD, which will
                    //cause your application to crash/fail in strange ways. Go to Tools->Options->Debugging to turn
                    //that setting off.
                    Debugger.Break();
                }
#endif

                return frmOwnerForm.DoThreadSafeFunc(x => ScrollableMessageBox.Show(x, message, caption, buttons, icon, defaultButton));
            }
            MainFormOnAssignActions.Add(x => ShowScrollableMessageBox(owner, message, caption, buttons, icon, defaultButton));
            return DialogResult.Cancel;
        }

        /// <summary>
        /// Shows a dialog box with vertical scrollbars for text that is too long centered on the Chummer main form window, or otherwise queues up such a box to be displayed
        /// </summary>
        public static Task<DialogResult> ShowScrollableMessageBoxAsync(string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, CancellationToken token = default)
        {
            return ShowScrollableMessageBoxAsync(null, message, caption, buttons, icon, defaultButton, token);
        }

        /// <summary>
        /// Shows a dialog box with vertical scrollbars for text that is too long centered on the a window containing a WinForms control, or otherwise queues up such a box to be displayed
        /// </summary>
        public static async Task<DialogResult> ShowScrollableMessageBoxAsync(Control owner, string message,
            string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, CancellationToken token = default)
        {
            if (Utils.IsUnitTest)
            {
                if (icon == MessageBoxIcon.Error || buttons != MessageBoxButtons.OK)
                {
                    Utils.BreakIfDebug();
                    string strMessage = "We don't want to see MessageBoxes in Unit Tests!" + Environment.NewLine +
                                        "Caption: " + caption + Environment.NewLine + "Message: " + message;
                    throw new InvalidOperationException(strMessage);
                }

                return DialogResult.OK;
            }

            Form frmOwnerForm = owner as Form ?? owner?.FindForm();
            if (frmOwnerForm.IsNullOrDisposed())
            {
                frmOwnerForm = TopMostLoadingBar;
                if (frmOwnerForm.IsNullOrDisposed())
                {
                    frmOwnerForm = MainForm;
                }
            }

            if (frmOwnerForm != null)
            {
#if DEBUG
                if (frmOwnerForm.InvokeRequired && _blnShowDevWarningAboutDebuggingOnlyOnce && Debugger.IsAttached)
                {
                    _blnShowDevWarningAboutDebuggingOnlyOnce = false;
                    //it works on my installation even in the debugger, so maybe we can ignore that...
                    //WARNING from the link above (you can edit that out if it's not causing problem):
                    //
                    //BUT ALSO KEEP IN MIND: when debugging a multi-threaded GUI app, and you're debugging in a thread
                    //other than the main/application thread, YOU NEED TO TURN OFF
                    //the "Enable property evaluation and other implicit function calls" option, or else VS will
                    //automatically fetch the values of local/global GUI objects FROM THE CURRENT THREAD, which will
                    //cause your application to crash/fail in strange ways. Go to Tools->Options->Debugging to turn
                    //that setting off.
                    Debugger.Break();
                }
#endif

                return await frmOwnerForm
                    .DoThreadSafeFuncAsync(
                        x => ScrollableMessageBox.ShowAsync(x, message, caption, buttons, icon, defaultButton,
                            token: token), token: token).Unwrap().ConfigureAwait(false);
            }

            MainFormOnAssignAsyncActions.Add(x =>
                ShowScrollableMessageBoxAsync(owner, message, caption, buttons, icon, defaultButton, token));
            return DialogResult.Cancel;
        }

        /// <summary>
        /// Shows a dialog box centered on the Chummer main form window, or otherwise queues up such a box to be displayed
        /// </summary>
        public static DialogResult ShowMessageBox(string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            return ShowMessageBox(null, message, caption, buttons, icon, defaultButton);
        }

        /// <summary>
        /// Shows a dialog box centered on the a window containing a WinForms control, or otherwise queues up such a box to be displayed
        /// </summary>
        public static DialogResult ShowMessageBox(Control owner, string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            if (Utils.IsUnitTest)
            {
                if (icon == MessageBoxIcon.Error || buttons != MessageBoxButtons.OK)
                {
                    Utils.BreakIfDebug();
                    string strMessage = "We don't want to see MessageBoxes in Unit Tests!" + Environment.NewLine +
                                        "Caption: " + caption + Environment.NewLine + "Message: " + message;
                    throw new InvalidOperationException(strMessage);
                }
                return DialogResult.OK;
            }
            Form frmOwnerForm = owner as Form ?? owner?.FindForm();
            if (frmOwnerForm.IsNullOrDisposed())
            {
                frmOwnerForm = TopMostLoadingBar;
                if (frmOwnerForm.IsNullOrDisposed())
                {
                    frmOwnerForm = MainForm;
                }
            }

            if (frmOwnerForm != null)
            {
#if DEBUG
                if (frmOwnerForm.InvokeRequired && _blnShowDevWarningAboutDebuggingOnlyOnce && Debugger.IsAttached)
                {
                    _blnShowDevWarningAboutDebuggingOnlyOnce = false;
                    //it works on my installation even in the debugger, so maybe we can ignore that...
                    //WARNING from the link above (you can edit that out if it's not causing problem):
                    //
                    //BUT ALSO KEEP IN MIND: when debugging a multi-threaded GUI app, and you're debugging in a thread
                    //other than the main/application thread, YOU NEED TO TURN OFF
                    //the "Enable property evaluation and other implicit function calls" option, or else VS will
                    //automatically fetch the values of local/global GUI objects FROM THE CURRENT THREAD, which will
                    //cause your application to crash/fail in strange ways. Go to Tools->Options->Debugging to turn
                    //that setting off.
                    Debugger.Break();
                }
#endif

                return frmOwnerForm.DoThreadSafeFunc(x => CenterableMessageBox.Show(x, message, caption, buttons, icon, defaultButton));
            }
            MainFormOnAssignActions.Add(x => ShowMessageBox(owner, message, caption, buttons, icon, defaultButton));
            return DialogResult.Cancel;
        }

        /// <summary>
        /// Shows a dialog box centered on the Chummer main form window, or otherwise queues up such a box to be displayed
        /// </summary>
        public static Task<DialogResult> ShowMessageBoxAsync(string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, CancellationToken token = default)
        {
            return ShowMessageBoxAsync(null, message, caption, buttons, icon, defaultButton, token);
        }

        /// <summary>
        /// Shows a dialog box centered on the a window containing a WinForms control, or otherwise queues up such a box to be displayed
        /// </summary>
        public static async Task<DialogResult> ShowMessageBoxAsync(Control owner, string message, string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, CancellationToken token = default)
        {
            if (Utils.IsUnitTest)
            {
                if (icon == MessageBoxIcon.Error || buttons != MessageBoxButtons.OK)
                {
                    Utils.BreakIfDebug();
                    string strMessage = "We don't want to see MessageBoxes in Unit Tests!" + Environment.NewLine +
                                        "Caption: " + caption + Environment.NewLine + "Message: " + message;
                    throw new InvalidOperationException(strMessage);
                }

                return DialogResult.OK;
            }

            Form frmOwnerForm = owner as Form ?? owner?.FindForm();
            if (frmOwnerForm.IsNullOrDisposed())
            {
                frmOwnerForm = TopMostLoadingBar;
                if (frmOwnerForm.IsNullOrDisposed())
                {
                    frmOwnerForm = MainForm;
                }
            }

            if (frmOwnerForm != null)
            {
#if DEBUG
                if (frmOwnerForm.InvokeRequired && _blnShowDevWarningAboutDebuggingOnlyOnce && Debugger.IsAttached)
                {
                    _blnShowDevWarningAboutDebuggingOnlyOnce = false;
                    //it works on my installation even in the debugger, so maybe we can ignore that...
                    //WARNING from the link above (you can edit that out if it's not causing problem):
                    //
                    //BUT ALSO KEEP IN MIND: when debugging a multi-threaded GUI app, and you're debugging in a thread
                    //other than the main/application thread, YOU NEED TO TURN OFF
                    //the "Enable property evaluation and other implicit function calls" option, or else VS will
                    //automatically fetch the values of local/global GUI objects FROM THE CURRENT THREAD, which will
                    //cause your application to crash/fail in strange ways. Go to Tools->Options->Debugging to turn
                    //that setting off.
                    Debugger.Break();
                }
#endif

                return await frmOwnerForm
                    .DoThreadSafeFuncAsync(
                        x => CenterableMessageBox.Show(x, message, caption, buttons, icon, defaultButton), token)
                    .ConfigureAwait(false);
            }

            MainFormOnAssignAsyncActions.Add(x =>
                ShowMessageBoxAsync(owner, message, caption, buttons, icon, defaultButton, token));
            return DialogResult.Cancel;
        }

        /// <summary>
        /// Queue of Actions to run after MainForm is assigned
        /// </summary>
        public static List<Action<ChummerMainForm>> MainFormOnAssignActions { get; } = new List<Action<ChummerMainForm>>();

        /// <summary>
        /// Queue of Async Actions to run after MainForm is assigned
        /// </summary>
        public static List<Func<ChummerMainForm, Task>> MainFormOnAssignAsyncActions { get; } = new List<Func<ChummerMainForm, Task>>();

        /// <summary>
        /// Gets the form to use for creating sub-forms and displaying them as dialogs
        /// </summary>
        /// <param name="objCharacter">If this character's file is open, use their open form as the one we want for showing dialogs.</param>
        /// <returns></returns>
        public static Form GetFormForDialog(Character objCharacter = null)
        {
            if (MainForm == null)
                return null;
            if (objCharacter == null)
                return MainForm;
            return MainForm.OpenCharacterEditorForms?.FirstOrDefault(
                       x => ReferenceEquals(x.CharacterObject, objCharacter)) as Form
                   ?? MainForm.OpenCharacterSheetViewers?.FirstOrDefault(x => x.CharacterObjects.Contains(objCharacter))
                       as Form
                   ?? MainForm.OpenCharacterExportForms?.FirstOrDefault(
                           x => ReferenceEquals(x.CharacterObject, objCharacter))
                       as Form
                   ?? MainForm;
        }

        /// <summary>
        /// Gets the form to use for creating sub-forms and displaying them as dialogs
        /// </summary>
        /// <param name="objCharacter">If this character's file is open, use their open form as the one we want for showing dialogs.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<Form> GetFormForDialogAsync(Character objCharacter = null, CancellationToken token = default)
        {
            if (MainForm == null)
                return Task.FromResult<Form>(null);
            return objCharacter == null ? Task.FromResult<Form>(MainForm) : InnerMethod();
            async Task<Form> InnerMethod()
            {
                Form frmReturn;
                ThreadSafeObservableCollection<CharacterShared> lstForms1 = MainForm.OpenCharacterEditorForms;
                if (lstForms1 != null)
                {
                    frmReturn = await lstForms1.FirstOrDefaultAsync(
                        x => ReferenceEquals(x.CharacterObject, objCharacter), token: token).ConfigureAwait(false);
                    if (frmReturn != null)
                        return frmReturn;
                }
                ThreadSafeObservableCollection<CharacterSheetViewer> lstForms2 = MainForm.OpenCharacterSheetViewers;
                if (lstForms2 != null)
                {
                    frmReturn = await lstForms2.FirstOrDefaultAsync(
                        x => x.CharacterObjects.Contains(objCharacter), token: token).ConfigureAwait(false);
                    if (frmReturn != null)
                        return frmReturn;
                }
                ThreadSafeObservableCollection<ExportCharacter> lstForms3 = MainForm.OpenCharacterExportForms;
                if (lstForms3 != null)
                {
                    frmReturn = await lstForms1.FirstOrDefaultAsync(
                        x => ReferenceEquals(x.CharacterObject, objCharacter), token: token).ConfigureAwait(false);
                    if (frmReturn != null)
                        return frmReturn;
                }
                return MainForm;
            }
        }

        /// <summary>
        /// List of all currently loaded characters (either in an open form or linked to a character in an open form via contact, spirit, or pet)
        /// </summary>
        public static ThreadSafeObservableCollection<Character> OpenCharacters { get; } = new ThreadSafeObservableCollection<Character>();

        private static async Task OpenCharactersOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (Character objCharacter in e.OldItems)
            {
                await objCharacter.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static async Task OpenCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Character objCharacter in e.OldItems)
                        {
                            await objCharacter.DisposeAsync().ConfigureAwait(false);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Character objCharacter in e.OldItems)
                        {
                            if (!e.NewItems.Contains(objCharacter))
                                await objCharacter.DisposeAsync().ConfigureAwait(false);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="frmLoadingBar">If not null, show and use this loading bar for the character.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Character LoadCharacter(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true, LoadingBar frmLoadingBar = null, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(
                () => LoadCharacterCoreAsync(true, strFileName, strNewName, blnClearFileName, blnShowErrors,
                                             frmLoadingBar, token), token);
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="frmLoadingBar">If not null, show and use this loading bar for the character.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<Character> LoadCharacterAsync(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true, LoadingBar frmLoadingBar = null, CancellationToken token = default)
        {
            return LoadCharacterCoreAsync(false, strFileName, strNewName, blnClearFileName, blnShowErrors, frmLoadingBar, token);
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="frmLoadingBar">If not null, show and use this loading bar for the character.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static async Task<Character> LoadCharacterCoreAsync(bool blnSync, string strFileName,
                                                                    string strNewName = "",
                                                                    bool blnClearFileName = false,
                                                                    bool blnShowErrors = true,
                                                                    LoadingBar frmLoadingBar = null,
                                                                    CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strFileName))
                return null;
            Character objCharacter = null;
            if (File.Exists(strFileName) && (strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                                             || strFileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase)))
            {
                //Timekeeper.Start("loading");
                if (string.IsNullOrEmpty(strNewName) && !blnClearFileName)
                {
                    objCharacter = blnSync
                        ? OpenCharacters.FirstOrDefault(x => x.FileName == strFileName)
                        : await OpenCharacters.FirstOrDefaultAsync(async x => await x.GetFileNameAsync(token).ConfigureAwait(false) == strFileName, token)
                                              .ConfigureAwait(false);
                    if (objCharacter != null)
                        return objCharacter;
                }

                objCharacter = new Character
                {
                    FileName = strFileName
                };
                string strAutosavesPath = Utils.GetAutosavesFolderPath;
                string strAutosaveName = string.Empty;
                if (blnShowErrors) // Only do the autosave prompt if we will show prompts
                {
                    if (!strFileName.StartsWith(strAutosavesPath, StringComparison.OrdinalIgnoreCase))
                    {
                        string strBaseFileName = Path.GetFileNameWithoutExtension(strFileName);
                        if (!string.IsNullOrEmpty(strBaseFileName))
                        {
                            strAutosaveName = Path.Combine(strAutosavesPath, strBaseFileName) + ".chum5";
                            if (File.Exists(strAutosaveName))
                            {
                                if (File.GetLastWriteTimeUtc(strAutosaveName) <= File.GetLastWriteTimeUtc(strFileName))
                                {
                                    strAutosaveName = string.Empty;
                                }
                            }
                            else
                                strAutosaveName = string.Empty;
                            if (string.IsNullOrEmpty(strAutosaveName))
                            {
                                strAutosaveName = Path.Combine(strAutosavesPath, strBaseFileName) + ".chum5lz";
                                if (File.Exists(strAutosaveName))
                                {
                                    if (File.GetLastWriteTimeUtc(strAutosaveName)
                                        <= File.GetLastWriteTimeUtc(strFileName))
                                    {
                                        strAutosaveName = string.Empty;
                                    }
                                }
                                else
                                    strAutosaveName = string.Empty;
                            }
                        }

                        if (string.IsNullOrEmpty(strAutosaveName) && !string.IsNullOrEmpty(strNewName))
                        {
                            strBaseFileName = Path.GetFileNameWithoutExtension(strNewName);
                            if (!string.IsNullOrEmpty(strBaseFileName))
                            {
                                strAutosaveName = Path.Combine(strAutosavesPath, strBaseFileName) + ".chum5";
                                if (File.Exists(strAutosaveName))
                                {
                                    if (File.GetLastWriteTimeUtc(strAutosaveName) <= File.GetLastWriteTimeUtc(strFileName))
                                    {
                                        strAutosaveName = string.Empty;
                                    }
                                }
                                else
                                    strAutosaveName = string.Empty;
                                if (string.IsNullOrEmpty(strAutosaveName))
                                {
                                    strAutosaveName = Path.Combine(strAutosavesPath, strBaseFileName) + ".chum5lz";
                                    if (File.Exists(strAutosaveName))
                                    {
                                        if (File.GetLastWriteTimeUtc(strAutosaveName)
                                            <= File.GetLastWriteTimeUtc(strFileName))
                                        {
                                            strAutosaveName = string.Empty;
                                        }
                                    }
                                    else
                                        strAutosaveName = string.Empty;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(strAutosaveName))
                    {
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            if (ShowScrollableMessageBox(
                                    string.Format(GlobalSettings.CultureInfo,
                                        // ReSharper disable once MethodHasAsyncOverload
                                        LanguageManager.GetString("Message_AutosaveFound", token: token),
                                        Path.GetFileName(strFileName),
                                        File.GetLastWriteTimeUtc(strAutosaveName).ToLocalTime(),
                                        File.GetLastWriteTimeUtc(strFileName).ToLocalTime()),
                                    // ReSharper disable once MethodHasAsyncOverload
                                    LanguageManager.GetString("MessageTitle_AutosaveFound", token: token),
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            {
                                strAutosaveName = string.Empty;
                            }
                        }
                        else if (await ShowScrollableMessageBoxAsync(
                                     string.Format(GlobalSettings.CultureInfo,
                                         await LanguageManager
                                             .GetStringAsync("Message_AutosaveFound", token: token)
                                             .ConfigureAwait(false),
                                         Path.GetFileName(strFileName),
                                         File.GetLastWriteTimeUtc(strAutosaveName).ToLocalTime(),
                                         File.GetLastWriteTimeUtc(strFileName).ToLocalTime()),
                                     await LanguageManager
                                         .GetStringAsync("MessageTitle_AutosaveFound", token: token)
                                         .ConfigureAwait(false),
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: token).ConfigureAwait(false) != DialogResult.Yes)
                        {
                            strAutosaveName = string.Empty;
                        }
                    }
                }

                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    OpenCharacters.Add(objCharacter);
                else
                    await OpenCharacters.AddAsync(objCharacter, token).ConfigureAwait(false);
                string strFileToLoad = string.IsNullOrEmpty(strAutosaveName) ? strFileName : strAutosaveName;
                //Timekeeper.Start("load_file");
                bool blnLoaded = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? objCharacter.Load(strFileToLoad, frmLoadingBar, blnShowErrors, token)
                    : await objCharacter.LoadAsync(strFileToLoad, frmLoadingBar, blnShowErrors, token).ConfigureAwait(false);
                //Timekeeper.Finish("load_file");
                if (!blnLoaded)
                {
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        OpenCharacters.Remove(objCharacter);
                    else
                        await OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                    return null;
                }

                // If a new name is given, set the character's name to match (used in cloning).
                if (!string.IsNullOrEmpty(strNewName))
                    objCharacter.Name = strNewName;
                // Clear the File Name field so that this does not accidentally overwrite the original save file (used in cloning).
                if (blnClearFileName
                    // Clear out file name if the character's file is in the autosaves folder because we do not want them to be manually saving there.
                    || objCharacter.FileName.StartsWith(strAutosavesPath, StringComparison.OrdinalIgnoreCase))
                    objCharacter.FileName = string.Empty;
            }
            else if (blnShowErrors)
            {
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                            // ReSharper disable once MethodHasAsyncOverload
                            LanguageManager.GetString("Message_FileNotFound", token: token),
                            strFileName),
                        // ReSharper disable once MethodHasAsyncOverload
                        LanguageManager.GetString("MessageTitle_FileNotFound", token: token),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    await ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager
                                .GetStringAsync("Message_FileNotFound", token: token)
                                .ConfigureAwait(false),
                            strFileName),
                        await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                }
            }

            return objCharacter;
        }

        public static Task<bool> SwitchToOpenCharacter(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null || MainForm == null)
                return Task.FromResult(false);
            return MainForm.SwitchToOpenCharacter(objCharacter, token);
        }

        /// <summary>
        /// Opens the correct window for a single character in the main form, queues the command to open on the main form if it is not assigned (thread-safe).
        /// </summary>
        public static Task OpenCharacter(Character objCharacter, bool blnIncludeInMru = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objCharacter == null ? Task.CompletedTask : OpenCharacterList(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open the correct windows for a list of characters in the main form, queues the command to open on the main form if it is not assigned (thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = true, CancellationToken token = default)
        {
            if (lstCharacters == null)
                return Task.CompletedTask;
            if (MainForm != null)
                return MainForm.OpenCharacterList(lstCharacters, blnIncludeInMru, token);
            return Task.Run(() => MainFormOnAssignAsyncActions.Add(
                                x => x.OpenCharacterList(lstCharacters, blnIncludeInMru, token)), token);
        }

        public static Task<bool> SwitchToOpenPrintCharacter(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null || MainForm == null)
                return Task.FromResult(false);
            return MainForm.SwitchToOpenPrintCharacter(objCharacter, token);
        }

        /// <summary>
        /// Open a character's print form up without necessarily opening them up fully for editing.
        /// </summary>
        public static Task OpenCharacterForPrinting(Character objCharacter, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objCharacter == null ? Task.CompletedTask : OpenCharacterListForPrinting(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open print forms for a list of characters (thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task OpenCharacterListForPrinting(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            if (lstCharacters == null)
                return Task.CompletedTask;
            if (MainForm != null)
                return MainForm.OpenCharacterListForPrinting(lstCharacters, blnIncludeInMru, token);
            return Task.Run(() => MainFormOnAssignAsyncActions.Add(
                                x => x.OpenCharacterListForPrinting(lstCharacters, blnIncludeInMru, token)), token);
        }

        public static Task<bool> SwitchToOpenExportCharacter(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null || MainForm == null)
                return Task.FromResult(false);
            return MainForm.SwitchToOpenExportCharacter(objCharacter, token);
        }

        /// <summary>
        /// Open a character for exporting without necessarily opening them up fully for editing.
        /// </summary>
        public static Task OpenCharacterForExport(Character objCharacter, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objCharacter == null ? Task.CompletedTask : OpenCharacterListForExport(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open export forms for a list of characters (thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task OpenCharacterListForExport(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            if (lstCharacters == null)
                return Task.CompletedTask;
            if (MainForm != null)
                return MainForm.OpenCharacterListForExport(lstCharacters, blnIncludeInMru, token);
            return Task.Run(() => MainFormOnAssignAsyncActions.Add(
                                x => x.OpenCharacterListForExport(lstCharacters, blnIncludeInMru, token)), token);
        }

        public static LoadingBar TopMostLoadingBar => s_frmTopMostLoadingBar;

        private static LoadingBar s_frmTopMostLoadingBar;

        private static readonly ConcurrentHashSet<LoadingBar> s_setLoadingBars = new ConcurrentHashSet<LoadingBar>();

        /// <summary>
        /// Syntactic sugar for creating and displaying a LoadingBar screen with specific text and progress bar size.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="intCount"></param>
        /// <returns></returns>
        public static ThreadSafeForm<LoadingBar> CreateAndShowProgressBar(string strFile = "", int intCount = 1)
        {
            ThreadSafeForm<LoadingBar> frmReturn = ThreadSafeForm<LoadingBar>.Get(() => new LoadingBar());
            frmReturn.MyForm.CharacterFile = strFile;
            if (intCount > 0)
                frmReturn.MyForm.Reset(intCount);
            frmReturn.MyForm.DoThreadSafe(x =>
            {
                x.Closed += (sender, args) =>
                {
                    s_setLoadingBars.Remove(x);
                    Interlocked.CompareExchange(ref s_frmTopMostLoadingBar, null, x);
                };
                x.Show();
            });
            Interlocked.CompareExchange(ref s_frmTopMostLoadingBar, frmReturn.MyForm, null);
            if (!s_setLoadingBars.TryAdd(frmReturn.MyForm))
                throw new InvalidOperationException();
            return frmReturn;
        }

        /// <summary>
        /// Syntactic sugar for creating and displaying a LoadingBar screen with specific text and progress bar size.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="intCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<ThreadSafeForm<LoadingBar>> CreateAndShowProgressBarAsync(string strFile = "", int intCount = 1, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            ThreadSafeForm<LoadingBar> frmReturn = await ThreadSafeForm<LoadingBar>.GetAsync(() => new LoadingBar(), token).ConfigureAwait(false);
            await frmReturn.MyForm.SetCharacterFileAsync(strFile, token).ConfigureAwait(false);
            if (intCount > 0)
                await frmReturn.MyForm.ResetAsync(intCount, token).ConfigureAwait(false);
            await frmReturn.MyForm.DoThreadSafeAsync(x =>
            {
                x.Closed += (sender, args) =>
                {
                    s_setLoadingBars.Remove(x);
                    Interlocked.CompareExchange(ref s_frmTopMostLoadingBar, null, x);
                };
                x.Show();
            }, token).ConfigureAwait(false);
            Interlocked.CompareExchange(ref s_frmTopMostLoadingBar, frmReturn.MyForm, null);
            if (!s_setLoadingBars.TryAdd(frmReturn.MyForm))
                throw new InvalidOperationException();
            return frmReturn;
        }

        /// <summary>
        /// Whether the application is running under Mono (true) or .NET (false)
        /// </summary>
        public static bool IsMono
        {
            get;
            private set;
        }

        private static ExceptionHeatMap ExceptionHeatMap { get; } = new ExceptionHeatMap();

        private static void FixCwd()
        {
            //If launched by file association, the cwd is file location.
            //Chummer looks for data in cwd, to be able to move exe (legacy+bootstrapper uses this)

            if (Directory.Exists(Utils.GetDataFolderPath)
                && Directory.Exists(Path.Combine(Utils.GetStartupPath, "lang")))
            {
                //both normally used data dirs present (add file loading abstraction to the list)
                //so do nothing

                return;
            }

            Environment.CurrentDirectory = Utils.GetStartupPath;
        }

        public static Mutex GlobalChummerMutex
        {
            get;
            private set;
        }
    }
}
