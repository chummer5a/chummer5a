using System;
using System.Collections.Generic;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
			// Make sure the default language has been loaded before attempting to open the Main Form.
			if (LanguageManager.Instance.Loaded)
				Application.Run(new frmMain());
			else
				Application.Exit();
        }
    }
}