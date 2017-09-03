using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrashHandler
{
	static class Program
	{
		static Dictionary<string, Action<string[]>> _functions = new Dictionary<string, Action<string[]>>()
		{
			{"crash", ShowCrashReport }
		};

		private static void ShowCrashReport(string[] obj)
		{
			CrashDumper dmper = null;
			try
			{
				dmper = new CrashDumper(obj[0]);
				frmCrashReporter reporter = new frmCrashReporter(dmper);

				Application.Run(reporter);

				if (reporter.DialogResult == DialogResult.OK)
				{
					Application.Run(new frmNoMoreUserInput(dmper));
				}
			}
			finally 
			{
				//Last ditch attempt at closing chummer if not done yet
				try
				{
					dmper?.Process?.Kill();
				} catch { }
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{ 
			if (args.Length == 0) return;

		    Action<string[]> actCachedAction;
			if (_functions.TryGetValue(args[0], out actCachedAction))
			{
                actCachedAction(args.Skip(1).ToArray());
			}

			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new frmCrashReporter());
		}
	}
}
