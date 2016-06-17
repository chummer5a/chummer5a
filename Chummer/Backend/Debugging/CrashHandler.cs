using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Chummer.Backend.Debugging
{
	internal class CrashHandler
	{
		private class DumpData
		{
			public DumpData()
			{
				AddDefaultInfo();
			}

			public List<string> capturefiles = new List<string>();
			public Dictionary<string, string> pretendfiles = new Dictionary<string, string>();
			public Dictionary<string, string> attributes = new Dictionary<string, string>();
			public int processid = Process.GetCurrentProcess().Id;

			void AddDefaultInfo()
			{
				//Crash handler will make visible-{whatever} visible in the upload while the rest will exists in a file named attributes.txt
				attributes.Add("visible-crash-id", Guid.NewGuid().ToString());

				attributes.Add("visible-build-type",
					#if DEBUG
					"DEBUG"
#else
					"RELEASE"
#endif
					);
				attributes.Add("commandline", Environment.CommandLine);
				attributes.Add("visible-version", Application.ProductVersion);
				
				try
				{
					RegistryKey cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

					if (!cv.GetValueNames().Contains("ProductId"))
					{
						//on 32 bit builds?
						//cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion");

						cv = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
					}

					String[] keys = cv.GetValueNames();
					attributes.Add("machine-id", cv.GetValue("ProductId").ToString());
					
				}
				catch{ }

				attributes.Add("machine-name", Environment.MachineName);
				attributes.Add("current-dir", Environment.CurrentDirectory);
				attributes.Add("application-dir", Application.ExecutablePath);

				attributes.Add("visible-error-friendly", "No description available");
			}

			public void AddException(Exception ex)
			{
				pretendfiles.Add("exception.txt", ex.ToString());

				attributes["visible-error-friendly"] = ex.Message;
			}

			public string SerializeBase64()
			{
				string altson = new JavaScriptSerializer().Serialize(this);
				return Convert.ToBase64String(Encoding.UTF8.GetBytes(altson));
			}

			public void AddFile(string file)
			{
				capturefiles.Add(file);
			}

			public void AddPrentendFile(string filename, string contents)
			{
				pretendfiles.Add(filename, contents);
			}
		}

		internal static void WebMiniDumpHandler(Exception ex)
		{

			if (MessageBox.Show("Chummer5a crashed.\nDo you want to send a crash report to the developer?", "Crash!", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					DumpData dump = new DumpData();
					
					dump.AddException(ex);
					dump.AddFile(Path.Combine(Environment.CurrentDirectory, "settings", "default.xml"));
					dump.AddFile(Path.Combine(Environment.CurrentDirectory, "chummerlog.txt"));
					

					Process crashHandler = Process.Start("crashhandler", "crash " + dump.SerializeBase64());

					crashHandler.WaitForExit();
				}
				catch(Exception nex)
				{
					MessageBox.Show("Failed to create crash report.\nMake sure your system is connected to the internet.\n" +nex.ToString());
				}
			}

		}
	}
}