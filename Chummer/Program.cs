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
 using System.Diagnostics;
using System.IO;
using System.Linq;
 using System.Net;
 using System.Net.Sockets;
 using System.Runtime;
 using System.Threading;
 using System.Windows.Forms;
﻿using Chummer.Backend;
 using Microsoft.ApplicationInsights;
 using Microsoft.ApplicationInsights.DataContracts;
 using Microsoft.ApplicationInsights.Extensibility;
 using Microsoft.ApplicationInsights.NLogTarget;
 using NLog;
 using NLog.Config;

[assembly: CLSCompliant(true)]
namespace Chummer
{
    internal static class Program
    {
        private static Logger Log = null;
        private const string strChummerGuid = "eb0759c1-3599-495e-8bc5-57c8b3e1b31c";
        //public static TelemetryConfiguration ApplicationInsightsConfig = new TelemetryConfiguration
        //{
        //    InstrumentationKey = "012fd080-80dc-4c10-97df-4f2cf8c805d5"
        //};
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();
        

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            PageViewTelemetry pvt = null;
            var startTime = DateTimeOffset.UtcNow;
            using (GlobalChummerMutex = new Mutex(false, @"Global\" + strChummerGuid))
            {
                IsMono = Type.GetType("Mono.Runtime") != null;
                // Mono doesn't always play nice with ProfileOptimization, so it's better to just not bother with it when running under Mono
                if (!IsMono)
                {
                    ProfileOptimization.SetProfileRoot(Utils.GetStartupPath);
                    ProfileOptimization.StartProfile("chummerprofile");
                }

                Stopwatch sw = Stopwatch.StartNew();
                //If debuging and launched from other place (Bootstrap), launch debugger
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
                //Log exceptions that is caught. Wanting to know about this cause of performance
                AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
                {
                    //Console.WriteLine(e.Exception.ToString());
                };
                AppDomain.CurrentDomain.FirstChanceException += ExceptionHeatmap.OnException;

                sw.TaskEnd("appdomain 2");

                string strInfo =
                    $"Application Chummer5a build {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} started at {DateTime.UtcNow} with command line arguments {Environment.CommandLine}";
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
#endif

                sw.TaskEnd("Startup");

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                if (!string.IsNullOrEmpty(LanguageManager.ManagerErrorMessage))
                {
                    MessageBox.Show(LanguageManager.ManagerErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(GlobalOptions.ErrorMessage))
                {
                    MessageBox.Show(GlobalOptions.ErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    LogManager.ThrowExceptions = true;
                    ConfigurationItemFactory.Default.Targets.RegisterDefinition(
                        "ApplicationInsightsTarget",
                        typeof(Microsoft.ApplicationInsights.NLogTarget.ApplicationInsightsTarget)
                    );
                    Log = NLog.LogManager.GetCurrentClassLogger();
                    if (GlobalOptions.UseLogging)
                    {
                        foreach (var rule in NLog.LogManager.Configuration.LoggingRules.ToList())
                        {
                            //only change the loglevel, if it's off - otherwise it has been changed manually
                            if (rule.Levels.Count == 0)
                                rule.EnableLoggingForLevels(LogLevel.Debug, LogLevel.Fatal);
                        }
                    }
                    Log.Info(strInfo);
                    Log.Info("NLog initialized");
                    if (GlobalOptions.UseLoggingApplicationInsights)
                    {
#if DEBUG
                        //If you set true as DeveloperMode (see above), you can see the sending telemetry in the debugging output window in IDE.
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
#else
                        TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = false;
#endif
                        TelemetryConfiguration.Active.TelemetryInitializers.Add(new CustomTelemetryInitializer());
                        //for now lets disable live view. We may make another GlobalOption to enable it at a later stage...
                        //var live = new LiveStreamProvider(ApplicationInsightsConfig);
                        //live.Enable();

                        // Log a page view:
                        pvt = new PageViewTelemetry("Program.Main()");
                        pvt.Id = "Program.Main()";
                        pvt.Name = strInfo;
                        pvt.Context.Operation.Name = "Operation Program.Main()";
                        pvt.Properties.Add("parameters", Environment.CommandLine);
                        pvt.Timestamp = startTime;
                    }
                    else
                    {
                        TelemetryConfiguration.Active.DisableTelemetry = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                //make sure the Settings are upgraded/preserved after an upgrade
                //see for details: https://stackoverflow.com/questions/534261/how-do-you-keep-user-config-settings-across-different-assembly-versions-in-net/534335#534335
                if (Properties.Settings.Default.UpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeRequired = false;
                    Properties.Settings.Default.Save();
                }

                // Make sure the default language has been loaded before attempting to open the Main Form.
                LanguageManager.TranslateWinForm(GlobalOptions.Language, null);

                MainForm = new frmChummerMain(false, pvt);
                Application.Run(MainForm);
                Log.Info(ExceptionHeatmap.GenerateInfo());
                if (GlobalOptions.UseLoggingApplicationInsights)
                {
                    if (TelemetryClient != null)
                    {
                        //if (pvt != null)
                        //{
                        //    pvt.Duration = DateTimeOffset.UtcNow - pvt.Timestamp;
                        //    ApplicationInsightsTelemetryClient.TrackPageView(pvt);
                        //}
                        TelemetryClient.Flush();
                        //we have to wait a bit to give it time to upload the data
                        Console.WriteLine("Waiting a bit to flush logging data...");
                        Thread.Sleep(5000);
                    }

                }
                
            }
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
