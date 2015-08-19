using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
			//If debuging and launched from other place (Bootstrap), launch debugger
			if (Environment.GetCommandLineArgs().Contains("/debug") && !Debugger.IsAttached)
			{
				Debugger.Launch();
			}

			//Various init stuff (that mostly "can" be removed as they serve 
			//debugging more than function

			//crash handler that will offer to send a mail
			AppDomain.CurrentDomain.UnhandledException += CrashReport.BuildFromException;
			
			//Needs to be called before Log is setup, as it moves where log might be.
	        FixCwd();


			//Log exceptions that is caught. Wanting to know about this cause of performance
	        AppDomain.CurrentDomain.FirstChanceException += Log.FirstChanceException;

			Log.Info(String.Format("Application Chummer5a build {0} started at {1} with command line arguments {2}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), DateTime.UtcNow, Environment.CommandLine) );

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


			

#if LEGACY
	        DialogResult result =
		        MessageBox.Show(
			        "Chummer5a is currently running in legacy mode.\n While this is possible, the Chummer5a team won't provide support if anything goes wrong\n This feature may be removed without warning",
			        "Legacy mode", MessageBoxButtons.OKCancel);

	        if (result == DialogResult.Cancel)
	        {
		        Application.Exit();
	        }
#endif


			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
			// Make sure the default language has been loaded before attempting to open the Main Form.
			if (LanguageManager.Instance.Loaded)
				Application.Run(new frmMain());
			else
				Application.Exit();
        }

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