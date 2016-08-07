/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;
using System.Reflection;
﻿using System.Windows;
﻿using Application = System.Windows.Forms.Application;
﻿using MessageBox = System.Windows.Forms.MessageBox;

namespace Chummer
{
	
	public partial class frmUpdate : Form
	{

		private bool _blnSilentMode;
		private bool _blnSilentCheck;
		private bool _blnDownloaded = false;
		private bool _blnUnBlocked = false;
		private string strDownloadFile = "";
		private string strLatestVersion = "";
		private string strTempPath = "";
        private readonly string strAppPath = Application.StartupPath;
		private readonly GlobalOptions _objGlobalOptions = GlobalOptions.Instance;
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
			_blnUnBlocked = CheckConnection("https://raw.githubusercontent.com/chummer5a/chummer5a/master/Chummer/changelog.txt");

			if (_blnUnBlocked)
			{
				GetChummerVersion();
				if (!_blnSilentMode)
				{
					WebClient wc = new WebClient();
					IWebProxy wp = WebRequest.DefaultWebProxy;
					wp.Credentials = CredentialCache.DefaultCredentials;
					wc.Proxy = wp;
					wc.Encoding = Encoding.UTF8;
					Log.Info("Download the changelog");
					wc.DownloadFile("https://raw.githubusercontent.com/chummer5a/chummer5a/" + LatestVersion + "/Chummer/changelog.txt",
						Path.Combine(Application.StartupPath, "changelog.txt"));
					webNotes.DocumentText = "<font size=\"-1\" face=\"Courier New,Serif\">" +
					                        File.ReadAllText(Path.Combine(Application.StartupPath, "changelog.txt"))
						                        .Replace("&", "&amp;")
						                        .Replace("<", "&lt;")
						                        .Replace(">", "&gt;")
						                        .Replace("\n", "<br />") + "</font>";
				}

				Log.Info("intCount = " + intCount.ToString());
				// If there is more than 1 instance running, do not let the application be updated.
				if (intCount > 1)
				{
					Log.Info("More than one instance, exiting");
					if (!_blnSilentMode && !_blnSilentCheck)
						MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_MultipleInstances"),
							LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Log.Info("frmUpdate_Load");
					this.Close();
				}
			}
			else
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Warning_Update_CouldNotConnect"), "Chummer5",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.Close();
				Log.Exit("frmUpdate_Load");
			}
			Log.Exit("frmUpdate_Load");
		}

		private bool CheckConnection(String URL)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

				//if (request.Proxy != null)
					//request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
				request.Timeout = 5000;
				request.Credentials = CredentialCache.DefaultNetworkCredentials;
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				return response.StatusCode == HttpStatusCode.OK;
			}
			catch
			{
				return false;
			}
		}
		public void GetChummerVersion()
		{
			if (_blnUnBlocked)
			{
				string strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases/latest";
				if (_objGlobalOptions.PreferNightlyBuilds)
				{
					strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases";
				}
				HttpWebRequest request =
					(HttpWebRequest) WebRequest.Create(strUpdateLocation);
				request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
				request.Accept = "application/json";
				// Get the response.

				HttpWebResponse response = (HttpWebResponse) request.GetResponse();

				// Get the stream containing content returned by the server.
				Stream dataStream = response.GetResponseStream();
				// Open the stream using a StreamReader for easy access.
				StreamReader reader = new StreamReader(dataStream);
				// Read the content.

				string responseFromServer = reader.ReadToEnd();
				string[] stringSeparators = new string[] {","};
				string[] result;
				result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

				bool blnFoundTag = false;
				bool blnFoundArchive = false;
				foreach (string line in result)
				{
					if (line.Contains("tag_name"))
					{
						strLatestVersion = line.Split(':')[1];
						strLatestVersion = strLatestVersion.Split('}')[0].Replace("\"", string.Empty);
						blnFoundTag = true;
					}
					if (line.Contains("browser_download_url"))
					{
						strDownloadFile = line.Split(':')[2];
						strDownloadFile = strDownloadFile.Substring(2);
						strDownloadFile = strDownloadFile.Split('}')[0].Replace("\"", string.Empty);
						strDownloadFile = "https://" + strDownloadFile;
						blnFoundArchive = true;
					}
					if (blnFoundArchive && blnFoundTag)
					{
						break;
					}
				}
				// Cleanup the streams and the response.
				reader.Close();
				dataStream.Close();
				response.Close();
			}
			else
			{
				strLatestVersion = LanguageManager.Instance.GetString("String_No_Update_Found");
			}
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
		/// Latest release build number located on Github.
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



		/// <summary>
		/// Latest release build number located on Github.
		/// </summary>
		public string CurrentVersion
		{
			get
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				string strCurrentVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
				return strCurrentVersion;
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
			strTempPath = Path.Combine(Path.GetTempPath(), ("chummer"+LatestVersion+".zip"));
			
			WebClient Client = new WebClient();
			Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
			Client.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadCompleted);
			Client.DownloadFileAsync(new Uri(strDownloadFile), strTempPath);
			cmdUpdate.Enabled = false;
		}
		
		#region AsyncDownload Events
		/// <summary>
		/// Update the download progress for the file.
		/// </summary>
		private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			double bytesIn = double.Parse(e.BytesReceived.ToString());
			double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
			double percentage = bytesIn / totalBytes * 100;
			pgbOverallProgress.Value = int.Parse(Math.Truncate(percentage).ToString());
		}


		/// <summary>
		/// The EXE file is down downloading, so replace the old file with the new one.
		/// </summary>
		private void wc_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			cmdUpdate.Text = "Restart";
			cmdUpdate.Enabled = true;
			Log.Info("wc_DownloadExeFileCompleted");
			
				_blnDownloaded = true;
			
				try
				{
				//Create a backup file in the temp directory. 
				string zipPath = Path.Combine(Path.GetTempPath(), ("chummer"+ CurrentVersion +".zip"));
				Log.Info("Creating archive from application path: ", strAppPath);
					if (!File.Exists(zipPath))
					{
						ZipFile.CreateFromDirectory(strAppPath, zipPath, CompressionLevel.Fastest, true);
					}
				// Delete the old Chummer5 executable.
				File.Delete(strAppPath + "\\Chummer5.exe.old");
				// Rename the current Chummer5 executable.
				File.Move(strAppPath+"\\Chummer5.exe", strAppPath + "\\Chummer5.exe.old");
				
				// Copy over the archive from the temp directory.
				Log.Info("Extracting downloaded archive into application path: ", zipPath);
					using (ZipArchive archive = ZipFile.OpenRead(strTempPath))
					{
						foreach (ZipArchiveEntry entry in archive.Entries)
						{
							entry.ExtractToFile(Path.Combine(strAppPath, entry.FullName), true);
						}
					}
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
