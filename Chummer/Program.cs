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
	        
			//Setup a crash handler that will send a mail
	        AppDomain.CurrentDomain.UnhandledException += CrashReport.BuildFromException;
	        throw new Exception();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
	        if (!Debugger.IsAttached)
	        {
		        Debugger.Launch();
			}
#endif
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