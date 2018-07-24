using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]
namespace CrashHandler
{
	static class Program
	{
		static readonly Dictionary<string, Action<string[]>> s_DictionaryFunctions = new Dictionary<string, Action<string[]>>()
		{
			{"crash", ShowCrashReport }
		};

		private static void ShowCrashReport(string[] args)
		{
            if (args.Contains("--debug") && Debugger.IsAttached == false)
		    {
		        Debugger.Launch();
		    }
			CrashDumper dmper = null;
			try
			{
				dmper = new CrashDumper(args[0]);
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
				}
			    catch
			    {
			        // ignored
			    }
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            for (int i = 0; i < args.Length - 1; ++i)
            {
                if (s_DictionaryFunctions.TryGetValue(args[i], out Action<string[]> actCachedAction))
                {
                    actCachedAction(args.Skip(i + 1).ToArray());
                    break;
                }
            }

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new frmCrashReporter());
        }
	}
}
