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
			try
			{
				CrashDumper dmper = new CrashDumper(obj[0]);
				Application.Run(new frmCrashReporter(dmper));
			}
			catch
			{
				
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{ 
			if (args.Length == 0) return;

			if (_functions.ContainsKey(args[0]))
			{
				_functions[args[0]](args.Skip(1).ToArray());
			}
			



			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new frmCrashReporter());


		}
	}
}
