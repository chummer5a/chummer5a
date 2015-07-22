using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bootstrap
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{

				String file = Path.Combine(Environment.CurrentDirectory, "Version",
#if DEBUG_NEVERDEFINEDCONSTANTFORTESTINGREMOVEBEFORERELEASE
"Debug",
#else
"Release",
#endif
					 "Chummer5.exe");
                if (!File.Exists(file))
				{
					MessageBox.Show("Main Chummer5 executeable not found. Please reinstall Chummer5a");
					return;
				}

				ProcessStartInfo startinfo = new ProcessStartInfo(file);


				startinfo.WorkingDirectory = Environment.CurrentDirectory;
				Regex exefilter = new Regex(".*\\.exe\"? ");
				String filtered = exefilter.Replace(Environment.CommandLine, "");
				startinfo.Arguments = filtered;

				Process p = Process.Start(startinfo);
				
				//Assembly asm = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "Version", "Release", "Chummer5.exe"));
				//asm.EntryPoint.Invoke(null, null);

			}
			catch (Win32Exception)
			{
				try
				{
					//DialogResult result =MessageBox.Show(
					//	"Launching chummer5a gave an error.\n" +
					//	"Your computer most likely don'tsupport a required version of the .Net Framework\n" +
					//	"You can try to run the legacy version of Chummer5a but the Chummer5a team won't provide support",
					//	"Startup error", MessageBoxButtons.OKCancel);

					//if (result == DialogResult.OK)
					//{

					ProcessStartInfo startinfo = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, "Version", "Legacy", "Chummer5.exe"));


					startinfo.WorkingDirectory = Environment.CurrentDirectory;
					Regex exefilter = new Regex(".*\\.exe\"? ");
					String filtered = exefilter.Replace(Environment.CommandLine, "");
					startinfo.Arguments = filtered;

					Process p = Process.Start(startinfo);
					//}
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						"Something bad happened. As this is legacy mode, we don't provide support.\nMake sure you are running a supported version of the net framework\n" +
						ex);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unspecified error happened" + ex);
			}
		}
	}
}
