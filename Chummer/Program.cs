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
using System.Xml;
using Chummer.Backend;
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
        internal static readonly Process MyProcess = s_objMyProcess.Value;

        [CLSCompliant(false)]
        public static TelemetryClient ChummerTelemetryClient { get; } = new TelemetryClient();
        private static PluginControl _objPluginLoader;
        public static PluginControl PluginLoader => _objPluginLoader = _objPluginLoader ?? new PluginControl();

        internal static readonly IntPtr CommandLineArgsDataTypeId = (IntPtr)7593599;

        /// <summary>
        /// Check this to see if we are currently in the Main Thread.
        /// </summary>
        [ThreadStatic]
        // ReSharper disable once ThreadStaticFieldHasInitializer
        public static readonly bool IsMainThread = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Set DPI Stuff before anything else, even the mutex
            SetProcessDPI(GlobalSettings.DpiScalingMethodSetting);
            if (IsMainThread)
                SetThreadDPI(GlobalSettings.DpiScalingMethodSetting);
            using (GlobalChummerMutex = new Mutex(false, @"Global\" + ChummerGuid, out bool blnIsNewInstance))
            {
                try
                {
                    try
                    {
                        // Chummer instance already exists, so switch to it instead of opening a new instance
                        if (!blnIsNewInstance || !GlobalChummerMutex.WaitOne(TimeSpan.FromSeconds(2), false))
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
                                    string.Join("<>", Environment.GetCommandLineArgs());
                                NativeMethods.CopyDataStruct objData = new NativeMethods.CopyDataStruct();
                                IntPtr ptrCommandLineArguments = IntPtr.Zero;
                                try
                                {
                                    // Allocate memory for the data and copy
                                    objData = NativeMethods.CopyDataFromString(CommandLineArgsDataTypeId, strCommandLineArgumentsJoined);
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
                    }
                    catch (AbandonedMutexException e)
                    {
                        Log.Info(e);
                    }

                    //for some fun try out this command line parameter: chummer://plugin:SINners:Load:5ff55b9d-7d1c-4067-a2f5-774127346f4e
                    PageViewTelemetry pvt = null;
                    DateTimeOffset startTime = DateTimeOffset.UtcNow;
                    // Set default cultures based on the currently set language
                    CultureInfo.DefaultThreadCurrentCulture = GlobalSettings.CultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = GlobalSettings.CultureInfo;
                    string strPostErrorMessage = string.Empty;
                    string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");
                    if (!Directory.Exists(settingsDirectoryPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(settingsDirectoryPath);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            string strMessage = LanguageManager.GetString("Message_Insufficient_Permissions_Warning",
                                                                          GlobalSettings.Language, false);
                            if (string.IsNullOrEmpty(strMessage))
                                strMessage = ex.ToString();
                            strPostErrorMessage = strMessage;
                        }
                        catch (Exception ex)
                        {
                            strPostErrorMessage = ex.ToString();
                        }
                    }

                    IsMono = Type.GetType("Mono.Runtime") != null;
                    // Delete old ProfileOptimization file because we don't want it anymore, instead we restart profiling for each newly generated assembly
                    Utils.SafeDeleteFile(Path.Combine(Utils.GetStartupPath, "chummerprofile"));
                    // We avoid weird issues with ProfileOptimization pointing JIT to the wrong place by checking for and removing all profile optimization files that
                    // were made in an older version (i.e. an older assembly)
                    string strProfileOptimizationName = "chummerprofile_" + Utils.CurrentChummerVersion + ".profile";
                    foreach (string strProfileFile in Directory.GetFiles(Utils.GetStartupPath, "*.profile", SearchOption.TopDirectoryOnly))
                        if (!string.Equals(strProfileFile, strProfileOptimizationName,
                                           StringComparison.OrdinalIgnoreCase))
                            Utils.SafeDeleteFile(strProfileFile);
                    // Mono, non-Windows native stuff, and Win11 don't always play nice with ProfileOptimization, so it's better to just not bother with it when running under them
                    if (!IsMono && Utils.HumanReadableOSVersion.StartsWith("Windows") && !Utils.HumanReadableOSVersion.StartsWith("Windows 11"))
                    {
                        ProfileOptimization.SetProfileRoot(Utils.GetStartupPath);
                        ProfileOptimization.StartProfile(strProfileOptimizationName);
                    }

                    Stopwatch sw = Stopwatch.StartNew();
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

                    string strInfo =
                        string.Format(GlobalSettings.InvariantCultureInfo,
                            "Application Chummer5a build {0} started at {1} with command line arguments {2}",
                            Utils.CurrentChummerVersion, DateTime.UtcNow,
                            Environment.CommandLine);
                    sw.TaskEnd("infogen");

                    sw.TaskEnd("infoprnt");

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    sw.TaskEnd("languagefreestartup");

                    void HandleCrash(object o, UnhandledExceptionEventArgs exa)
                    {
                        if (exa.ExceptionObject is Exception ex)
                        {
                            try
                            {
                                if (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.Crashes
                                    && ChummerTelemetryClient != null
                                    && !Utils.IsMilestoneVersion)
                                {
                                    ExceptionTelemetry et = new ExceptionTelemetry(ex)
                                    {
                                        SeverityLevel = SeverityLevel.Critical
                                    };
                                    //we have to enable the uploading of THIS message, so it isn't filtered out in the DropUserdataTelemetryProcessos
                                    foreach (DictionaryEntry d in ex.Data)
                                    {
                                        if ((d.Key != null) && (d.Value != null))
                                            et.Properties.Add(d.Key.ToString(), d.Value.ToString());
                                    }

                                    et.Properties.Add("IsCrash", bool.TrueString);
                                    CustomTelemetryInitializer ti = new CustomTelemetryInitializer();
                                    ti.Initialize(et);

                                    ChummerTelemetryClient.TrackException(et);
                                    ChummerTelemetryClient.Flush();
                                }
                            }
                            catch (Exception ex1)
                            {
                                Log.Error(ex1);
                            }
#if !DEBUG
                            CrashHandler.WebMiniDumpHandler(ex);
#endif
                        }
                    }

                    AppDomain.CurrentDomain.UnhandledException += HandleCrash;

                    sw.TaskEnd("Startup");

                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

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
                        TelemetryConfiguration.Active.InstrumentationKey = "012fd080-80dc-4c10-97df-4f2cf8c805d5";
                        LogManager.ThrowExceptions = true;
                        if (IsMono)
                        {
                            //Mono Crashes because of Application Insights. Set Logging to local, when Mono Runtime is detected
                            GlobalSettings.UseLoggingApplicationInsights = UseAILogging.OnlyLocal;
                        }
                        if (GlobalSettings.UseLoggingApplicationInsights > UseAILogging.OnlyMetric)
                        {
                            ConfigurationItemFactory.Default.Targets.RegisterDefinition(
                                "ApplicationInsightsTarget",
                                typeof(ApplicationInsightsTarget)
                            );
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

                        if (!Utils.IsUnitTest && GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.OnlyMetric)
                        {
#if DEBUG
                            //If you set true as DeveloperMode (see above), you can see the sending telemetry in the debugging output window in IDE.
                            TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
#else
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = false;
#endif
                            TelemetryConfiguration.Active.TelemetryInitializers.Add(new CustomTelemetryInitializer());
                            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next =>
                                new TranslateExceptionTelemetryProcessor(next));
                            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next =>
                                new DropUserdataTelemetryProcessor(next, Environment.UserName));
                            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Build();
                            //for now lets disable live view.We may make another GlobalOption to enable it at a later stage...
                            //var live = new LiveStreamProvider(ApplicationInsightsConfig);
                            //live.Enable();

                            //Log an Event with AssemblyVersion and CultureInfo
                            MetricIdentifier objMetricIdentifier = new MetricIdentifier("Chummer", "Program Start",
                                "Version", "Culture", "AISetting", "OSVersion");
                            string strOSVersion = Utils.HumanReadableOSVersion;
                            Metric objMetric = ChummerTelemetryClient.GetMetric(objMetricIdentifier);
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

                            UploadObjectAsMetric.UploadObject(ChummerTelemetryClient, typeof(GlobalSettings));
                        }
                        else
                        {
                            TelemetryConfiguration.Active.DisableTelemetry = true;
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
                                Log.Warn("Files could not be unblocked in " + AppDomain.CurrentDomain.BaseDirectory);
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
                                    ShowMessageBox(strMessage, "Plugins not enabled", MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
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
                                            ShowMessageBox(strMessage, strWhatPlugin + " not enabled",
                                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                            ChummerTelemetryClient?.TrackException(ex);
                            Log.Warn(e);
                        }
                    }

                    // Delete the old executable if it exists (created by the update process).
                    Utils.SafeClearDirectory(Utils.GetStartupPath, "*.old");
                    // Purge the temporary directory
                    Utils.SafeClearDirectory(Utils.GetTempPath());

                    if (showMainForm)
                    {
                        // Attempt to cache all XML files that are used the most.
                        using (_ = Timekeeper.StartSyncron("cache_load", null, CustomActivity.OperationType.DependencyOperation, Utils.CurrentChummerVersion.ToString(3)))
                        using (MainProgressBar
                                   = CreateAndShowProgressBar(
                                       Application.ProductName, Utils.BasicDataFileNames.Count))
                        {
                            Task[] tskCachingTasks = new Task[Utils.BasicDataFileNames.Count];
                            for (int i = 0; i < tskCachingTasks.Length; ++i)
                            {
                                string strLoopFile = Utils.BasicDataFileNames[i];
                                tskCachingTasks[i] = Task.Run(() => CacheCommonFile(strLoopFile));
                            }

                            async Task CacheCommonFile(string strFile)
                            {
                                // Load default language data first for performance reasons
                                if (!GlobalSettings.Language.Equals(
                                        GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                                {
                                    await XmlManager.LoadXPathAsync(strFile, null, GlobalSettings.DefaultLanguage);
                                }
                                await XmlManager.LoadXPathAsync(strFile);
                                MainProgressBar.PerformStep(
                                    Application.ProductName,
                                    LoadingBar.ProgressBarTextPatterns.Initializing);
                            }

                            Utils.RunWithoutThreadLock(() => Task.WhenAll(tskCachingTasks));
                        }

                        MainForm.MyStartupPvt = pvt;
                        Application.Run(MainForm);
                    }

                    PluginLoader?.Dispose();
                    Log.Info(ExceptionHeatMap.GenerateInfo());
                    if (GlobalSettings.UseLoggingApplicationInsights > UseAILogging.OnlyLocal
                        && ChummerTelemetryClient != null)
                    {
                        ChummerTelemetryClient.Flush();
                        //we have to wait a bit to give it time to upload the data
                        Console.WriteLine("Waiting a bit to flush logging data...");
                        Utils.SafeSleep(TimeSpan.FromSeconds(2));
                    }
                }
                finally
                {
                    GlobalChummerMutex.ReleaseMutex();
                }
            }
        }

        private static bool UnblockPath(string strPath)
        {
            bool blnAllUnblocked = true;

            foreach (string strFile in Directory.GetFiles(strPath))
            {
                if (!UnblockFile(strFile))
                {
                    // Get the last error and display it.
                    int intError = Marshal.GetLastWin32Error();
                    Win32Exception exception = new Win32Exception(intError, "Error while unblocking " + strFile + '.');
                    switch (exception.NativeErrorCode)
                    {
                        case 2://file not found - that means the alternate data-stream is not present.
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

            foreach (string strDir in Directory.GetDirectories(strPath))
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

        private static void SetProcessDPI(DpiScalingMethod eMethod)
        {
            switch (eMethod)
            {
                case DpiScalingMethod.None:
                    if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext
                            NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.ContextDpiAwareness.Unaware);
                        else
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.Unaware);
                    }
                    else
                        return;
                    break;
                // System
                case DpiScalingMethod.Zoom:
                    if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext
                            NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.ContextDpiAwareness.System);
                        else
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.System);
                    }
                    else
                        NativeMethods.SetProcessDPIAware();
                    break;
                // PerMonitor/PerMonitorV2
                case DpiScalingMethod.Rescale:
                    if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext and PerMonitorV2
                            NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.ContextDpiAwareness.PerMonitorV2);
                        else
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.PerMonitor);
                    }
                    else
                        NativeMethods.SetProcessDPIAware(); // System as backup, because it's better than remaining unaware if we want PerMonitor/PerMonitorV2
                    break;
                // System (Enhanced)
                case DpiScalingMethod.SmartZoom:
                    if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // Windows 8.1 added SetProcessDpiAwareness
                    {
                        if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // Windows 10 Creators Update added SetProcessDpiAwarenessContext
                        {
                            NativeMethods.SetProcessDpiAwarenessContext(
                                Environment.OSVersion.Version >= new Version(10, 0, 17763)
                                    ? NativeMethods.ContextDpiAwareness.UnawareGdiScaled // Windows 10 Version 1809 Added GDI+ Scaling
                                    : NativeMethods.ContextDpiAwareness.System); // System as backup, because it's better than remaining unaware if we want GDI+ Scaling
                        }
                        else
                            NativeMethods.SetProcessDpiAwareness(NativeMethods.ProcessDpiAwareness.System);
                    }
                    else
                        NativeMethods.SetProcessDPIAware(); // System as backup, because it's better than remaining unaware if we want GDI+ Scaling
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eMethod), eMethod, null);
            }

            Utils.BreakOnErrorIfDebug();
        }

        private static void SetThreadDPI(DpiScalingMethod eMethod)
        {
            if (Environment.OSVersion.Version <
                new Version(10, 0, 15063)) // Windows 10 Creators Update added SetThreadDpiAwarenessContext
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
                        Environment.OSVersion.Version >= new Version(10, 0, 17763)
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
                if (_frmMainForm == value)
                    return;
                _frmMainForm = value;
                if (value != null)
                {
                    foreach (Action<ChummerMainForm> funcToRun in MainFormOnAssignActions)
                        funcToRun(value);
                    MainFormOnAssignActions.Clear();
                    foreach (Func<ChummerMainForm, Task> funcAsyncToRun in MainFormOnAssignAsyncActions)
                        Task.Run(() => funcAsyncToRun(value));
                    MainFormOnAssignAsyncActions.Clear();
                }
            }
        }

#if DEBUG
        private static bool _blnShowDevWarningAboutDebuggingOnlyOnce = true;
#endif

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
            if (owner == null)
                owner = MainProgressBar.IsNullOrDisposed() ? MainForm as Control : MainProgressBar;
            if (owner != null)
            {
                if (owner.InvokeRequired)
                {
#if DEBUG
                    if (_blnShowDevWarningAboutDebuggingOnlyOnce && Debugger.IsAttached)
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

                    try
                    {
                        return (DialogResult)owner.Invoke(new PassControlStringStringReturnDialogResultDelegate(ShowMessageBox),
                            owner, message, caption, buttons, icon, defaultButton);
                    }
                    catch (ObjectDisposedException)
                    {
                        //if the main form is disposed, we really don't need to bother anymore...
                    }
                    catch (Exception e)
                    {
                        string msg = "Could not show a MessageBox " + caption + ':' + Environment.NewLine + message
                                     + Environment.NewLine + Environment.NewLine + "Exception: " + e;
                        Log.Fatal(e, msg);
                    }
                }

                return CenterableMessageBox.Show(MainProgressBar.IsNullOrDisposed() ? owner.FindForm() : MainProgressBar, message, caption, buttons, icon, defaultButton);
            }
            MainFormOnAssignActions.Add(x => ShowMessageBox(owner, message, caption, buttons, icon, defaultButton));
            return DialogResult.Cancel;
        }

        private delegate DialogResult PassControlStringStringReturnDialogResultDelegate(
            Control owner, string s1, string s2, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton);

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
            return MainForm.OpenCharacterForms.FirstOrDefault(
                x => ReferenceEquals(x.CharacterObject, objCharacter)) as Form ?? MainForm;
        }

        /// <summary>
        /// List of all currently loaded characters (either in an open form or linked to a character in an open form via contact, spirit, or pet)
        /// </summary>
        public static ThreadSafeObservableCollection<Character> OpenCharacters { get; } = new ThreadSafeObservableCollection<Character>();

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="blnShowProgressBar">Show loading bar for the character.</param>
        public static Character LoadCharacter(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true, bool blnShowProgressBar = true)
        {
            return LoadCharacterCoreAsync(true, strFileName, strNewName, blnClearFileName, blnShowErrors, blnShowProgressBar).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="blnShowProgressBar">Show loading bar for the character.</param>
        public static Task<Character> LoadCharacterAsync(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true, bool blnShowProgressBar = true)
        {
            return LoadCharacterCoreAsync(false, strFileName, strNewName, blnClearFileName, blnShowErrors, blnShowProgressBar);
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="blnShowProgressBar">Show loading bar for the character.</param>
        private static async Task<Character> LoadCharacterCoreAsync(bool blnSync, string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true, bool blnShowProgressBar = true)
        {
            if (string.IsNullOrEmpty(strFileName))
                return null;
            Character objCharacter = null;
            if (File.Exists(strFileName) && strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
            {
                //Timekeeper.Start("loading");
                bool blnLoadAutosave = false;
                string strAutosavesPath = Utils.GetAutosavesFolderPath;
                if (string.IsNullOrEmpty(strNewName) && !blnClearFileName)
                {
                    objCharacter = OpenCharacters.FirstOrDefault(x => x.FileName == strFileName);
                    if (objCharacter != null)
                        return objCharacter;
                }
                objCharacter = new Character
                {
                    FileName = strFileName
                };
                if (blnShowErrors) // Only do the autosave prompt if we will show prompts
                {
                    if (!strFileName.StartsWith(strAutosavesPath))
                    {
                        string strNewAutosaveName = Path.GetFileName(strFileName);
                        if (!string.IsNullOrEmpty(strNewAutosaveName))
                        {
                            strNewAutosaveName = Path.Combine(strAutosavesPath, strNewAutosaveName);
                            if (File.Exists(strNewAutosaveName) && File.GetLastWriteTimeUtc(strNewAutosaveName) > File.GetLastWriteTimeUtc(strFileName))
                            {
                                blnLoadAutosave = true;
                                objCharacter.FileName = strNewAutosaveName;
                            }
                        }

                        if (!blnLoadAutosave && !string.IsNullOrEmpty(strNewName))
                        {
                            string strOldAutosaveName = strNewName;
                            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                            {
                                strOldAutosaveName = strOldAutosaveName.Replace(invalidChar, '_');
                            }

                            if (!string.IsNullOrEmpty(strOldAutosaveName))
                            {
                                strOldAutosaveName = Path.Combine(strAutosavesPath, strOldAutosaveName);
                                if (File.Exists(strOldAutosaveName) && File.GetLastWriteTimeUtc(strOldAutosaveName) > File.GetLastWriteTimeUtc(strFileName))
                                {
                                    blnLoadAutosave = true;
                                    objCharacter.FileName = strOldAutosaveName;
                                }
                            }
                        }
                    }
                    if (blnLoadAutosave && ShowMessageBox(
                        string.Format(GlobalSettings.CultureInfo,
                                      // ReSharper disable once MethodHasAsyncOverload
                                      blnSync ? LanguageManager.GetString("Message_AutosaveFound") : await LanguageManager.GetStringAsync("Message_AutosaveFound"),
                            Path.GetFileName(strFileName),
                            File.GetLastWriteTimeUtc(objCharacter.FileName).ToLocalTime(),
                            File.GetLastWriteTimeUtc(strFileName).ToLocalTime()),
                        // ReSharper disable once MethodHasAsyncOverload
                        blnSync ? LanguageManager.GetString("MessageTitle_AutosaveFound") : await LanguageManager.GetStringAsync("MessageTitle_AutosaveFound"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        blnLoadAutosave = false;
                        objCharacter.FileName = strFileName;
                    }
                }
                if (blnShowProgressBar && MainProgressBar.IsNullOrDisposed())
                {
                    using (MainProgressBar = await CreateAndShowProgressBarAsync(Path.GetFileName(objCharacter.FileName), Character.NumLoadingSections))
                    {
                        OpenCharacters.Add(objCharacter);
                        //Timekeeper.Start("load_file");
                        bool blnLoaded = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.Load(MainProgressBar, blnShowErrors)
                            : await objCharacter.LoadAsync(MainProgressBar, blnShowErrors);
                        //Timekeeper.Finish("load_file");
                        if (!blnLoaded)
                        {
                            OpenCharacters.Remove(objCharacter);
                            return null;
                        }
                    }
                }
                else
                {
                    OpenCharacters.Add(objCharacter);
                    //Timekeeper.Start("load_file");
                    bool blnLoaded;
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        blnLoaded = objCharacter.Load(blnShowProgressBar ? MainProgressBar : null, blnShowErrors);
                    else
                        blnLoaded = await objCharacter.LoadAsync(blnShowProgressBar ? MainProgressBar : null,
                            blnShowErrors);
                    //Timekeeper.Finish("load_file");
                    if (!blnLoaded)
                    {
                        OpenCharacters.Remove(objCharacter);
                        return null;
                    }
                }

                // If a new name is given, set the character's name to match (used in cloning).
                if (!string.IsNullOrEmpty(strNewName))
                    objCharacter.Name = strNewName;
                // Clear the File Name field so that this does not accidentally overwrite the original save file (used in cloning).
                if (blnClearFileName)
                    objCharacter.FileName = string.Empty;
                // Restore original filename if we loaded from an autosave
                if (blnLoadAutosave)
                    objCharacter.FileName = strFileName;
                // Clear out file name if the character's file is in the autosaves folder because we do not want them to be manually saving there.
                if (objCharacter.FileName.StartsWith(strAutosavesPath))
                    objCharacter.FileName = string.Empty;
            }
            else if (blnShowErrors)
            {
                ShowMessageBox(string.Format(GlobalSettings.CultureInfo,
                        blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString("Message_FileNotFound")
                            : await LanguageManager.GetStringAsync("Message_FileNotFound"),
                        strFileName),
                    blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? LanguageManager.GetString("MessageTitle_FileNotFound")
                        : await LanguageManager.GetStringAsync("MessageTitle_FileNotFound"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return objCharacter;
        }

        public static bool SwitchToOpenCharacter(Character objCharacter)
        {
            if (objCharacter == null || MainForm == null)
                return false;
            return MainForm.DoThreadSafeFunc(x => ((ChummerMainForm) x).SwitchToOpenCharacter(objCharacter));
        }

        /// <summary>
        /// Opens the correct window for a single character in the main form, queues the command to open on the main form if it is not assigned (thread-safe).
        /// </summary>
        public static Task OpenCharacter(Character objCharacter, bool blnIncludeInMru = true)
        {
            return objCharacter == null ? Task.CompletedTask : OpenCharacterList(objCharacter.Yield(), blnIncludeInMru);
        }

        /// <summary>
        /// Open the correct windows for a list of characters in the main form, queues the command to open on the main form if it is not assigned (thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        public static Task OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = true)
        {
            if (lstCharacters == null)
                return Task.CompletedTask;
            if (MainForm != null)
                return MainForm.DoThreadSafeFunc(
                    x => ((ChummerMainForm) x).OpenCharacterList(lstCharacters, blnIncludeInMru));
            return Task.Run(() => MainFormOnAssignAsyncActions.Add(
                                x => x.DoThreadSafeFunc(
                                    y => ((ChummerMainForm) y).OpenCharacterList(lstCharacters, blnIncludeInMru))));
        }

        /// <summary>
        /// Open a character's print form up without necessarily opening them up fully for editing.
        /// </summary>
        public static Task OpenCharacterForPrinting(Character objCharacter)
        {
            if (objCharacter == null || MainForm == null)
                return Task.CompletedTask;
            if (MainForm != null)
                return MainForm.DoThreadSafeFunc(x => ((ChummerMainForm) x).OpenCharacterForPrinting(objCharacter));
            return Task.Run(() => MainFormOnAssignAsyncActions.Add(
                                x => x.DoThreadSafeFunc(
                                    y => ((ChummerMainForm) y).OpenCharacterForPrinting(objCharacter))));
        }

        /// <summary>
        /// Open a character for exporting without necessarily opening them up fully for editing.
        /// </summary>
        public static Task OpenCharacterForExport(Character objCharacter)
        {
            if (objCharacter == null || MainForm == null)
                return Task.CompletedTask;
            if (MainForm != null)
                return MainForm.DoThreadSafeFunc(x => ((ChummerMainForm)x).OpenCharacterForExport(objCharacter));
            return Task.Run(() => MainFormOnAssignAsyncActions.Add(
                                x => x.DoThreadSafeFunc(
                                    y => ((ChummerMainForm)y).OpenCharacterForExport(objCharacter))));
        }

        public static LoadingBar MainProgressBar { get; set; }

        /// <summary>
        /// Syntactic sugar for creating and displaying a LoadingBar screen with specific text and progress bar size.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="intCount"></param>
        /// <returns></returns>
        public static LoadingBar CreateAndShowProgressBar(string strFile = "", int intCount = 1)
        {
            LoadingBar frmReturn = new LoadingBar { CharacterFile = strFile };
            if (intCount > 0)
                frmReturn.Reset(intCount);
            frmReturn.Show();
            return frmReturn;
        }

        /// <summary>
        /// Syntactic sugar for creating and displaying a LoadingBar screen with specific text and progress bar size.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="intCount"></param>
        /// <returns></returns>
        public static async ValueTask<LoadingBar> CreateAndShowProgressBarAsync(string strFile = "", int intCount = 1)
        {
            LoadingBar frmReturn = new LoadingBar { CharacterFile = strFile };
            if (intCount > 0)
                await frmReturn.ResetAsync(intCount);
            frmReturn.Show();
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
            //If launched by file assiocation, the cwd is file location.
            //Chummer looks for data in cwd, to be able to move exe (legacy+bootstraper uses this)

            if (Directory.Exists(Path.Combine(Utils.GetStartupPath, "data"))
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
