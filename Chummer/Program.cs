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
 using System.ComponentModel;
 using System.Diagnostics;
 using System.Globalization;
 using System.IO;
using System.Linq;
 using System.Reflection;
 using System.Runtime;
 using System.Runtime.InteropServices;
 using System.Threading;
 using System.Windows.Forms;
 using Chummer.Backend;
 using Chummer.Plugins;
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
        private const string strChummerGuid = "eb0759c1-3599-495e-8bc5-57c8b3e1b31c";
        internal static readonly Process MyProcess = Process.GetCurrentProcess();

        public static TelemetryClient ChummerTelemetryClient { get; } = new TelemetryClient();
        private static PluginControl _objPluginLoader;
        public static PluginControl PluginLoader => _objPluginLoader = _objPluginLoader ?? new PluginControl();


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //for some fun try out this command line parameter: chummer://plugin:SINners:Load:5ff55b9d-7d1c-4067-a2f5-774127346f4e
            PageViewTelemetry pvt = null;
            var startTime = DateTimeOffset.UtcNow;
            using (GlobalChummerMutex = new Mutex(false, @"Global\" + strChummerGuid))
            {
                // Set default cultures based on the currently set language
                CultureInfo.DefaultThreadCurrentCulture = GlobalOptions.CultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = GlobalOptions.CultureInfo;
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
                        string strMessage = LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language, false);
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
                // Mono doesn't always play nice with ProfileOptimization, so it's better to just not bother with it when running under Mono
                if (!IsMono)
                {
                    ProfileOptimization.SetProfileRoot(Utils.GetStartupPath);
                    ProfileOptimization.StartProfile("chummerprofile");
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
                    string.Format(GlobalOptions.InvariantCultureInfo, "Application Chummer5a build {0} started at {1} with command line arguments {2}",
                        Assembly.GetExecutingAssembly().GetName().Version, DateTime.UtcNow, Environment.CommandLine);
                sw.TaskEnd("infogen");

                sw.TaskEnd("infoprnt");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                sw.TaskEnd("languagefreestartup");
#if !DEBUG
                AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                        CrashHandler.WebMiniDumpHandler(ex);

                    //main.Hide();
                    //main.ShowInTaskbar = false;
                };
#else
                AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                {
                    try
                    {
                        if (e.ExceptionObject is Exception myException)
                        {
                            myException.Data.Add("IsCrash", bool.TrueString);
                            ExceptionTelemetry et = new ExceptionTelemetry(myException)
                            {
                                SeverityLevel = SeverityLevel.Critical
                            };
                            //we have to enable the uploading of THIS message, so it isn't filtered out in the DropUserdataTelemetryProcessos
                            foreach (DictionaryEntry d in myException.Data)
                            {
                                if (d.Key != null && d.Value != null)
                                    et.Properties.Add(d.Key.ToString(), d.Value.ToString());
                            }
                            ChummerTelemetryClient.TrackException(myException);
                            ChummerTelemetryClient.Flush();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                };
#endif

                sw.TaskEnd("Startup");

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                if (!string.IsNullOrEmpty(LanguageManager.ManagerErrorMessage))
                {
                    // MainForm is null at the moment, so we have to show error box manually
                    MessageBox.Show(LanguageManager.ManagerErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(GlobalOptions.ErrorMessage))
                {
                    // MainForm is null at the moment, so we have to show error box manually
                    MessageBox.Show(GlobalOptions.ErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(strPostErrorMessage))
                {
                    // MainForm is null at the moment, so we have to show error box manually
                    MessageBox.Show(strPostErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    TelemetryConfiguration.Active.InstrumentationKey = "012fd080-80dc-4c10-97df-4f2cf8c805d5";
                    LogManager.ThrowExceptions = true;
                    if (GlobalOptions.UseLoggingApplicationInsights > UseAILogging.OnlyMetric)
                    {
                        ConfigurationItemFactory.Default.Targets.RegisterDefinition(
                            "ApplicationInsightsTarget",
                            typeof(ApplicationInsightsTarget)
                        );
                    }

                    LogManager.ThrowExceptions = false;
                    Log = LogManager.GetCurrentClassLogger();
                    if (GlobalOptions.UseLogging)
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

                    if (Properties.Settings.Default.UploadClientId == Guid.Empty)
                    {
                        Properties.Settings.Default.UploadClientId = Guid.NewGuid();
                        Properties.Settings.Default.Save();
                    }

                    if (!Utils.IsUnitTest && GlobalOptions.UseLoggingApplicationInsights >= UseAILogging.OnlyMetric)
                    {

#if DEBUG
                        //If you set true as DeveloperMode (see above), you can see the sending telemetry in the debugging output window in IDE.
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
#else
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = false;
#endif
                        TelemetryConfiguration.Active.TelemetryInitializers.Add(new CustomTelemetryInitializer());
                        TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next => new TranslateExceptionTelemetryProcessor(next));
                        TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next => new DropUserdataTelemetryProcessor(next, Environment.UserName));
                        TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Build();
                        //for now lets disable live view.We may make another GlobalOption to enable it at a later stage...
                        //var live = new LiveStreamProvider(ApplicationInsightsConfig);
                        //live.Enable();

                        //Log an Event with AssemblyVersion and CultureInfo
                        MetricIdentifier objMetricIdentifier = new MetricIdentifier("Chummer", "Program Start", "Version", "Culture", dimension3Name:"AISetting", dimension4Name:"OSVersion");
                        string strOSVersion = helpers.Application_Insights.OSVersion.GetOSInfo();
                        Metric objMetric = ChummerTelemetryClient.GetMetric(objMetricIdentifier);
                        objMetric.TrackValue(1,
                            Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                            GlobalOptions.UseLoggingApplicationInsights.ToString(),
                            strOSVersion);

                        //Log a page view:
                        pvt = new PageViewTelemetry("frmChummerMain()")
                        {
                            Name = "Chummer Startup: " +
                                   Assembly.GetExecutingAssembly().GetName().Version,
                            Id = Properties.Settings.Default.UploadClientId.ToString(),
                            Timestamp = startTime
                        };
                        pvt.Context.Operation.Name = "Operation Program.Main()";
                        pvt.Properties.Add("parameters", Environment.CommandLine);

                        UploadObjectAsMetric.UploadObject(ChummerTelemetryClient, typeof(GlobalOptions));
                    }
                    else
                    {
                        TelemetryConfiguration.Active.DisableTelemetry = true;
                    }

                    Log.Info(strInfo);
                    Log.Info("Logging options are set to " + GlobalOptions.UseLogging + " and Upload-Options are set to "
                             + GlobalOptions.UseLoggingApplicationInsights + " (Installation-Id: "
                             + Properties.Settings.Default.UploadClientId.ToString("D", GlobalOptions.InvariantCultureInfo) + ").");

                    //make sure the Settings are upgraded/preserved after an upgrade
                    //see for details: https://stackoverflow.com/questions/534261/how-do-you-keep-user-config-settings-across-different-assembly-versions-in-net/534335#534335
                    if (Properties.Settings.Default.UpgradeRequired)
                    {
                        if (UnblockPath(AppDomain.CurrentDomain.BaseDirectory))
                        {
                            Properties.Settings.Default.Upgrade();
                            Properties.Settings.Default.UpgradeRequired = false;
                            Properties.Settings.Default.Save();
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
                // Make sure the default language has been loaded before attempting to open the Main Form.
                LanguageManager.LoadLanguage(GlobalOptions.Language);
                MainForm = new frmChummerMain();
                try
                {
                    PluginLoader.LoadPlugins(null);
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
                            if (!GlobalOptions.PluginsEnabled)
                            {
                                string strMessage = "Please enable Plugins to use command-line arguments invoking specific plugin-functions!";
                                Log.Warn(strMessage);
                                MainForm.ShowMessageBox(strMessage, "Plugins not enabled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                string strWhatPlugin = strArg.Substring(strArg.IndexOf("/plugin", StringComparison.Ordinal) + 8);
                                //some external apps choose to add a '/' before a ':' even in the middle of an url...
                                strWhatPlugin = strWhatPlugin.TrimStart(':');
                                int intEndPlugin = strWhatPlugin.IndexOf(':');
                                string strParameter = strWhatPlugin.Substring(intEndPlugin + 1);
                                strWhatPlugin = strWhatPlugin.Substring(0, intEndPlugin);
                                IPlugin objActivePlugin = PluginLoader.MyActivePlugins.FirstOrDefault(a => a.ToString() == strWhatPlugin);
                                if (objActivePlugin == null)
                                {
                                    if (PluginLoader.MyPlugins.All(a => a.ToString() != strWhatPlugin))
                                    {
                                        string strMessage = "Plugin " + strWhatPlugin + " is not enabled in the options!" + Environment.NewLine
                                                            + "If you want to use command-line arguments, please enable this plugin and restart the program.";
                                        Log.Warn(strMessage);
                                        MainForm.ShowMessageBox(strMessage, strWhatPlugin + " not enabled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                if (showMainForm)
                {
                    MainForm.MyStartupPVT = pvt;
                    Application.Run(MainForm);
                }
                PluginLoader?.Dispose();
                Log.Info(ExceptionHeatMap.GenerateInfo());
                if (GlobalOptions.UseLoggingApplicationInsights > UseAILogging.OnlyLocal
                    && ChummerTelemetryClient != null)
                {
                    ChummerTelemetryClient.Flush();
                    //we have to wait a bit to give it time to upload the data
                    Console.WriteLine("Waiting a bit to flush logging data...");
                    Thread.Sleep(2000);
                }
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        private static bool UnblockPath(string strPath)
        {
            bool blnAllUnblocked = true;

            foreach (string strFile in Directory.GetFiles(strPath))
            {
                if (!UnblockFile(strFile))
                {
                    // Get the last error and display it.
                    int intError = Marshal.GetLastWin32Error();
                    Win32Exception exception = new Win32Exception(intError, "Error while unblocking " + strFile + ".");
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
            return DeleteFile(strFileName + ":Zone.Identifier");
        }


        /// <summary>
        /// Main application form.
        /// </summary>
        public static frmChummerMain MainForm
        {
            get;
            set;
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
