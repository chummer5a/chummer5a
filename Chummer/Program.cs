using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			
			//Log exceptions that is caught. Wanting to know about this cause of performance
	        AppDomain.CurrentDomain.FirstChanceException += Log.FirstChanceException;

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
    }
}