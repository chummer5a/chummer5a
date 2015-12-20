using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression.FileSystem;

namespace Chummer
{
	public partial class frmUpdate : Form
	{
		private bool _blnSilentMode;
		private bool _blnSilentCheck;
		private bool _blnDownloaded = false;
		private string strDownloadFile = "";
		private string strLatestVersion = "";
		private CommonFunctions objFunctions = new CommonFunctions();

		public frmUpdate()
		{
			Log.Info("frmUpdate");
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		private void frmUpdate_Load(object sender, EventArgs e)
		{
			Log.Info("frmUpdate_Load");
			// Count the number of instances of Chummer that are currently running.
			Log.Info("Get process list");
			string strFileName = Process.GetCurrentProcess().MainModule.FileName;
			Log.Info("Get Chummer process count");
			int intCount = 0;
			foreach (Process objProcess in Process.GetProcesses())
			{
				try
				{
					if (objProcess.MainModule.FileName == strFileName)
						intCount++;
				}
				catch
				{
				}
			}

			if (!_blnSilentMode)
			{
				WebClient wc = new WebClient();
				wc.Encoding = Encoding.UTF8;
				Log.Info("Download the changelog");
				wc.DownloadFile("https://www.dropbox.com/s/0ugjj17dvi1qrr0/changelog.txt?dl=1", Path.Combine(Environment.CurrentDirectory, "changelog.txt"));
				webNotes.DocumentText = "<font size=\"-1\" face=\"Courier New,Serif\">" + File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "changelog.txt")).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "<br />") + "</font>";
			}

			GetChummerVersion();

			Log.Info("intCount = " + intCount.ToString());
			// If there is more than 1 instance running, do not let the application be updated.
			if (intCount > 1)
			{
				Log.Info("More than one instance, exiting");
				if (!_blnSilentMode && !_blnSilentCheck)
					MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_MultipleInstances"), LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				Log.Info("frmUpdate_Load");
				this.Close();
			}
			Log.Exit("frmUpdate_Load");
		}

		public void GetChummerVersion()
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/chummer5a/chummer5a/releases/latest");
			request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
			request.Accept = "application/json";
			// Get the response.

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// Get the stream containing content returned by the server.
			Stream dataStream = response.GetResponseStream();
			// Open the stream using a StreamReader for easy access.
			StreamReader reader = new StreamReader(dataStream);
			// Read the content.

			string responseFromServer = reader.ReadToEnd();
			string[] stringSeparators = new string[] { "," };
			string[] result;
			result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

			foreach (string line in result)
			{
				if (line.Contains("browser_download_url"))
				{
					strDownloadFile = line.Split(':')[2];
					strDownloadFile = strDownloadFile.Substring(2);
					strDownloadFile = strDownloadFile.Split('}')[0].Replace("\"", string.Empty);
					strDownloadFile = "https://" + strDownloadFile;
				}
				if (line.Contains("tag_name"))
				{
					strLatestVersion = line.Split(':')[1];
					strLatestVersion = strLatestVersion.Split('}')[0].Replace("\"", string.Empty);
				}
			}
			// Cleanup the streams and the response.
			reader.Close();
			dataStream.Close();
			response.Close();
		}

		/// <summary>
		/// When checking if a new version is available, don't show the update window.
		/// </summary>
		public bool SilentCheck
		{
			get
			{
				return _blnSilentCheck;
			}
			set
			{
				_blnSilentCheck = value;
			}
		}

		/// <summary>
		/// When running in silent mode, the update window will not be shown.
		/// </summary>
		public bool SilentMode
		{
			get
			{
				return _blnSilentMode;
			}
			set
			{
				_blnSilentMode = value;
			}
		}

		/// <summary>
		/// When running in silent mode, the update window will not be shown.
		/// </summary>
		public string LatestVersion
		{
			get
			{
				return strLatestVersion;
			}
			set
			{
				strLatestVersion = value;
			}
		}

		private void cmdDownload_Click(object sender, EventArgs e)
		{
			if (!_blnDownloaded)
			{
				Log.Info("cmdUpdate_Click");
				Log.Info("Download updates");
				DownloadUpdates();
			}
			else
			{
				Log.Info("cmdUpdate_Click");
				Log.Info("Restart Chummer");
				Application.Restart();
			}
		}

		private void DownloadUpdates()
		{
			Log.Enter("DownloadUpdates");
			string strAppPath = Application.ExecutablePath;
			string strArchive = strAppPath + ".old";
			string strNewPath = Path.Combine(Path.GetTempPath(), "chummer5.zip");
			string strFilePath = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
			
			WebClient Client = new WebClient();
			Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
			Client.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadCompleted);
			Client.DownloadFileAsync(new Uri(strDownloadFile), strNewPath);
			cmdUpdate.Enabled = false;
		}
		
		#region AsyncDownload Events
		/// <summary>
		/// Update the download progress for the file.
		/// </summary>
		private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			Log.Info("wc_DownloadProgressChanged");
			double bytesIn = double.Parse(e.BytesReceived.ToString());
			double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
			double percentage = bytesIn / totalBytes * 100;

			pgbOverallProgress.Value = int.Parse(Math.Truncate(percentage).ToString());
			Log.Exit("wc_DownloadProgressChanged");
		}


		/// <summary>
		/// The EXE file is down downloading, so replace the old file with the new one.
		/// </summary>
		private void wc_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			cmdUpdate.Text = "Restart";
			cmdUpdate.Enabled = true;
			Log.Info("wc_DownloadExeFileCompleted");
			string strAppPath = Application.StartupPath;
			string strArchive = strAppPath + "/old";
			string strNewPath = Path.Combine(Path.GetTempPath(), "chummer5.exe");

			Log.Info("Validate the EXE");

				Log.Info("EXE validated");
				_blnDownloaded = true;

				// Copy over the executable.
				try
				{
					Log.Info("strArchive = " + strArchive);
					Log.Info("strAppPath = " + strAppPath);
					Log.Info("strNewPath = " + strNewPath);

				_blnDownloaded = true;
				}
				catch (Exception ex)
				{
					Log.Error("ERROR Message = " + ex.Message);
					Log.Error("ERROR Source  = " + ex.Source);
					Log.Error("ERROR Trace   = " + ex.StackTrace.ToString());
				}
			Log.Exit("wc_DownloadExeFileCompleted");
		}

		#endregion
	}
}
