using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chummer.Debugging;

namespace Chummer
{
    static class Program
    {
		/// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			Stopwatch sw = Stopwatch.StartNew();
			//If debuging and launched from other place (Bootstrap), launch debugger
			if (Environment.GetCommandLineArgs().Contains("/debug") && !Debugger.IsAttached)
			{
				Debugger.Launch();
			}
	        sw.TaskEnd("dbgchk");
			//Various init stuff (that mostly "can" be removed as they serve 
			//debugging more than function

			//crash handler that will offer to send a mail
			AppDomain.CurrentDomain.UnhandledException += CrashReport.BuildFromException;
			
	        sw.TaskEnd("appdomain 1");

			//Needs to be called before Log is setup, as it moves where log might be.
	        FixCwd();

	        sw.TaskEnd("fixcwd");
			//Log exceptions that is caught. Wanting to know about this cause of performance
	        AppDomain.CurrentDomain.FirstChanceException += Log.FirstChanceException;
			AppDomain.CurrentDomain.FirstChanceException += heatmap.OnException;
			

			sw.TaskEnd("appdomain 2");

	        string info =
		        $"Application Chummer5a build {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} started at {DateTime.UtcNow} with command line arguments {Environment.CommandLine}";

	        sw.TaskEnd("infogen");

			Log.Info( info);
			
	        sw.TaskEnd("infoprnt");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


	        sw.TaskEnd("languagefreestartup");
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
			// Make sure the default language has been loaded before attempting to open the Main Form.

	        sw.TaskEnd("Startup");
			if (LanguageManager.Instance.Loaded)
				Application.Run(new frmMain());
			else
				Application.Exit();

			string ExceptionMap = heatmap.GenerateInfo();
			Log.Info(ExceptionMap);
        }

		static ExceptionHeatMap heatmap = new ExceptionHeatMap();

		static void FixCwd()
		{
			//If launched by file assiocation, the cwd is file location. 
			//Chummer looks for data in cwd, to be able to move exe (legacy+bootstraper uses this)

			if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "data"))
			    && Directory.Exists(Path.Combine(Environment.CurrentDirectory, "lang")))
			{
				//both normally used data dirs present (add file loading abstraction to the list)
				//so do nothing

				return;
			}

			Environment.CurrentDirectory = Application.StartupPath;
		}
    }
}