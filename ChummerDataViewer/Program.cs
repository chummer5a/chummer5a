using System;
using System.IO;
using System.Windows.Forms;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
		    if (args.Length > 0)
		    {
		        if (args[0] == "decrypt")
		        {
		            string file = args[1];
		            byte[] fileContents = File.ReadAllBytes(file);
		            byte[] decrypted = DownloaderWorker.Decrypt(args[2], fileContents);
		            string newPath = Path.GetFileNameWithoutExtension(file) + ".zip";
		            File.WriteAllBytes(newPath, decrypted);
		        }
		    }
		    else
		    {
		        Application.EnableVisualStyles();
		        Application.SetCompatibleTextRenderingDefault(false);
		        Application.Run(new Mainform());
		    }
		}
	}
}
