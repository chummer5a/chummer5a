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
 using System.Threading.Tasks;
 using System.Windows.Forms;
﻿using Chummer.Backend;
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
        public static TelemetryClient ChummerTelemetryClient { get; } = new TelemetryClient();
        private static PluginControl _pluginLoader;
        public static PluginControl PluginLoader
        {
            get => _pluginLoader ?? (_pluginLoader = new PluginControl());
            set => _pluginLoader = value;
        }


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
                        strPostErrorMessage += strMessage;
                    }
                    catch (Exception ex)
                    {
                        strPostErrorMessage += ex.ToString();
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

                AppDomain.CurrentDomain.FirstChanceException += ExceptionHeatmap.OnException;

                sw.TaskEnd("appdomain 2");

                string strInfo =
                    $"Application Chummer5a build {Assembly.GetExecutingAssembly().GetName().Version} started at {DateTime.UtcNow} with command line arguments {Environment.CommandLine}";
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
                                if ((d.Key != null) && (d.Value != null))
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
                    MainForm.ShowMessageBox(LanguageManager.ManagerErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(GlobalOptions.ErrorMessage))
                {
                    MainForm.ShowMessageBox(GlobalOptions.ErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(strPostErrorMessage))
                {
                    MainForm.ShowMessageBox(strPostErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
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
                        foreach (var rule in LogManager.Configuration.LoggingRules.ToList())
                        {
                            //only change the loglevel, if it's off - otherwise it has been changed manually
                            if (rule.Levels.Count == 0)
                                rule.EnableLoggingForLevels(LogLevel.Debug, LogLevel.Fatal);
                        }
                    }

                    if (Properties.Settings.Default.UploadClientId == Guid.Empty)
                    {
                        Properties.Settings.Default.UploadClientId = Guid.NewGuid();
                        Properties.Settings.Default.Save();
                    }

                    if (GlobalOptions.UseLoggingApplicationInsights >= UseAILogging.OnlyMetric)
                    {

#if DEBUG
                        //If you set true as DeveloperMode (see above), you can see the sending telemetry in the debugging output window in IDE.
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
#else
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = false;
#endif
                        TelemetryConfiguration.Active.TelemetryInitializers.Add(new CustomTelemetryInitializer());
                        TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use((next) => new TranslateExceptionTelemetryProcessor(next));
                        var replacePath = Environment.UserName;
                        TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use((next) => new DropUserdataTelemetryProcessor(next, replacePath));
                        TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Build();
                        //for now lets disable live view.We may make another GlobalOption to enable it at a later stage...
                        //var live = new LiveStreamProvider(ApplicationInsightsConfig);
                        //live.Enable();

                        //Log an Event with AssemblyVersion and CultureInfo
                        MetricIdentifier mi = new MetricIdentifier("Chummer", "Program Start", "Version", "Culture", dimension3Name:"AISetting");
                        var metric = ChummerTelemetryClient.GetMetric(mi);
                        metric.TrackValue(1,
                            Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                            GlobalOptions.UseLoggingApplicationInsights.ToString());

                        //Log a page view:
                        pvt = new PageViewTelemetry("frmChummerMain()")
                        {
                            Name = "Chummer Startup: " +
                                   Assembly.GetExecutingAssembly().GetName().Version,
                            Id = Properties.Settings.Default.UploadClientId.ToString()
                        };
                        pvt.Context.Operation.Name = "Operation Program.Main()";
                        pvt.Properties.Add("parameters", Environment.CommandLine);
                        pvt.Timestamp = startTime;

                        UploadObjectAsMetric.UploadObject(ChummerTelemetryClient, typeof(GlobalOptions));
                    }
                    else
                    {
                        TelemetryConfiguration.Active.DisableTelemetry = true;
                    }
                    if (Utils.IsUnitTest)
                        TelemetryConfiguration.Active.DisableTelemetry = true;

                    Log.Info(strInfo);
                    Log.Info("Logging options are set to " + GlobalOptions.UseLogging + " and Upload-Options are set to " + GlobalOptions.UseLoggingApplicationInsights + " (Installation-Id: " + Properties.Settings.Default.UploadClientId + ").");

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
                }

                //load the plugins and maybe work of any command line arguments
                //arguments come in the form of
                //              /plugin:Name:Parameter:Argument
                //              /plugin:SINners:RegisterUriScheme:0
                bool showMainForm = true;
                // Make sure the default language has been loaded before attempting to open the Main Form.
                LanguageManager.TranslateWinForm(GlobalOptions.Language, null);
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
                        var loopResult = Parallel.For(1, strArgs.Length, i =>
                        {
                            if (strArgs[i].Contains("/plugin"))
                            {
                                if (GlobalOptions.PluginsEnabled == false)
                                {
                                    string msg =
                                        "Please enable Plugins to use command-line arguments invoking specific plugin-functions!";
                                    Log.Warn(msg);
                                    MessageBox.Show(msg, "Plugins not enabled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                                else
                                {
                                    string whatplugin = strArgs[i].Substring(strArgs[i].IndexOf("/plugin", StringComparison.Ordinal) + 8);
                                    //some external apps choose to add a '/' before a ':' even in the middle of an url...
                                    whatplugin = whatplugin.TrimStart(':');
                                    int endplugin = whatplugin.IndexOf(':');
                                    string parameter = whatplugin.Substring(endplugin + 1);
                                    whatplugin = whatplugin.Substring(0, endplugin);
                                    var plugin =
                                        PluginLoader.MyActivePlugins.FirstOrDefault(a =>
                                            a.ToString() == whatplugin);
                                    if (plugin == null)
                                    {
                                        if (PluginLoader.MyPlugins.All(a => a.ToString() != whatplugin))
                                        {
                                            string msg = "Plugin " + whatplugin + " is not enabled in the options!" + Environment.NewLine;
                                            msg +=
                                                "If you want to use command-line arguments, please enable this plugin and restart the program.";
                                            Log.Warn(msg);
                                            MessageBox.Show(msg, whatplugin + " not enabled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        showMainForm &= plugin.ProcessCommandLine(parameter);
                                    }
                                }
                            }
                        });
                        if (!loopResult.IsCompleted)
                            Debugger.Break();
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
                    MainForm.FormMainInitialize(pvt);
                    Application.Run(MainForm);
                }
                Log.Info(ExceptionHeatmap.GenerateInfo());
                if (GlobalOptions.UseLoggingApplicationInsights > UseAILogging.OnlyLocal)
                {
                    if (ChummerTelemetryClient != null)
                    {
                        ChummerTelemetryClient.Flush();
                        //we have to wait a bit to give it time to upload the data
                        Console.WriteLine("Waiting a bit to flush logging data...");
                        Thread.Sleep(2000);
                    }
                }
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        public static bool UnblockPath(string path)
        {
            bool allUnblocked = true;
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                if (!UnblockFile(file))
                {
                    // Get the last error and display it.
                    int error = Marshal.GetLastWin32Error();
                    Win32Exception exception = new Win32Exception(error, "Error while unblocking " + file + ".");
                    switch (exception.NativeErrorCode)
                    {
                        case 2://file not found - that means the alternate data-stream is not present.
                            break;
                        case 5: Log.Warn(exception);
                            allUnblocked = false;
                            break;
                        default: Log.Error(exception);
                            allUnblocked = false;
                            break;
                    }
                }
            }

            foreach (string dir in dirs)
            {
                if (!UnblockPath(dir))
                    allUnblocked = false;
            }

            return allUnblocked;

        }

        public static bool UnblockFile(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
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

        private static ExceptionHeatMap ExceptionHeatmap { get; } = new ExceptionHeatMap();

        static void FixCwd()
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
