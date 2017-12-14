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
 using System.Globalization;
 using System.IO;
 using System.IO.Compression;
 using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
﻿using System.Windows;
﻿using Application = System.Windows.Forms.Application;
﻿using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Generic;

namespace Chummer
{
    public partial class frmUpdate : Form
    {

        private bool _blnSilentMode;
        private bool _blnSilentCheck;
        private string _strDownloadFile = string.Empty;
        private string _strLatestVersion = string.Empty;
        private string _strCurrentVersion = string.Empty;
        private Version _objCurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private string _strTempPath = string.Empty;
        private string _strTempUpdatePath = string.Empty;
        private readonly string _strAppPath = Application.StartupPath;
        private bool _blnPreferNightly = false;
        private bool _blnIsConnected = true;
        private bool _blnChangelogDownloaded = false;
        private readonly BackgroundWorker _workerConnectionLoader = new BackgroundWorker();
        private readonly WebClient _clientDownloader = new WebClient();
        private readonly WebClient _clientChangelogDownloader = new WebClient();

        public frmUpdate()
        {
            Log.Info("frmUpdate");
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            _strCurrentVersion = $"{_objCurrentVersion.Major}.{_objCurrentVersion.Minor}.{_objCurrentVersion.Build}";
            _blnPreferNightly = GlobalOptions.PreferNightlyBuilds;
            _strTempUpdatePath = Path.Combine(Path.GetTempPath(), "changelog.txt");

            _workerConnectionLoader.WorkerReportsProgress = false;
            _workerConnectionLoader.WorkerSupportsCancellation = true;
            _workerConnectionLoader.DoWork += LoadConnection;
            _workerConnectionLoader.RunWorkerCompleted += PopulateChangelog;

            _clientDownloader.DownloadFileCompleted += wc_DownloadCompleted;
            _clientDownloader.DownloadProgressChanged += wc_DownloadProgressChanged;

            IWebProxy wp = WebRequest.DefaultWebProxy;
            wp.Credentials = CredentialCache.DefaultCredentials;
            _clientChangelogDownloader.Proxy = WebRequest.DefaultWebProxy;
            _clientChangelogDownloader.Encoding = Encoding.UTF8;
        }

        private void frmUpdate_Load(object sender, EventArgs e)
        {
            if (!_blnIsConnected)
            {
                Close();
                return;
            }
            Log.Info("frmUpdate_Load");
            Log.Info("Check Global Mutex for duplicate");
            bool blnHasDuplicate = !Program.GlobalChummerMutex.WaitOne(0, false);
            Log.Info("blnHasDuplicate = " + blnHasDuplicate.ToString());
            // If there is more than 1 instance running, do not let the application be updated.
            if (blnHasDuplicate)
            {
                Log.Info("More than one instance, exiting");
                if (!_blnSilentMode && !_blnSilentCheck)
                    MessageBox.Show(LanguageManager.GetString("Message_Update_MultipleInstances"), LanguageManager.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Log.Info("frmUpdate_Load");
                Close();
            }
            if (!_blnChangelogDownloaded && !_workerConnectionLoader.IsBusy)
            {
                _workerConnectionLoader.RunWorkerAsync();
            }
            Log.Exit("frmUpdate_Load");
        }

        private void frmUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            _workerConnectionLoader.CancelAsync();
            _clientDownloader.CancelAsync();
            _clientChangelogDownloader.CancelAsync();
        }

        private void PopulateChangelog(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!_clientDownloader.IsBusy)
                cmdUpdate.Enabled = true;
            if (_blnSilentMode)
            {
                cmdDownload_Click(sender, e);
            }
            if (File.Exists(_strTempUpdatePath))
            {
                string strUpdateLog = File.ReadAllText(_strTempUpdatePath);
                webNotes.DocumentText = "<font size=\"-1\" face=\"Courier New,Serif\">" +
                                                strUpdateLog
                                                    .Replace("&", "&amp;")
                                                    .Replace("<", "&lt;")
                                                    .Replace(">", "&gt;")
                                                    .Replace("\n", "<br />") + "</font>";
            }
            DoVersionTextUpdate();
        }

        private void LoadConnection(object sender, DoWorkEventArgs e)
        {
            if (_clientChangelogDownloader.IsBusy)
                return;
            if (!GetChummerVersion())
            {
                MessageBox.Show(LanguageManager.GetString("Warning_Update_CouldNotConnect"), "Chummer5", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _blnIsConnected = false;
                this.DoThreadSafe(new Action(() => Close()));
            }
            else if (LatestVersion != LanguageManager.GetString("String_No_Update_Found"))
            {
                if (File.Exists(_strTempUpdatePath))
                {
                    if (File.Exists(_strTempUpdatePath + ".old"))
                        File.Delete(_strTempUpdatePath + ".old");
                    File.Move(_strTempUpdatePath, _strTempUpdatePath + ".old");
                }
                string strURL = "https://raw.githubusercontent.com/chummer5a/chummer5a/" + LatestVersion + "/Chummer/changelog.txt";
                if (Uri.TryCreate(strURL, UriKind.Absolute, out Uri uriConnectionAddress))
                {
                    try
                    {
                        if (File.Exists(_strTempUpdatePath + ".tmp"))
                            File.Delete(_strTempUpdatePath + ".tmp");
                        _clientChangelogDownloader.DownloadFile(uriConnectionAddress, _strTempUpdatePath + ".tmp");
                        File.Move(_strTempUpdatePath + ".tmp", _strTempUpdatePath);
                    }
                    catch (WebException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Warning_Update_CouldNotConnect"), "Chummer5", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _blnIsConnected = false;
                        this.DoThreadSafe(new Action(() => Close()));
                    }
                }
                else
                {
                    MessageBox.Show(LanguageManager.GetString("Warning_Update_CouldNotConnect"), "Chummer5", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _blnIsConnected = false;
                    this.DoThreadSafe(new Action(() => Close()));
                }
            }
        }

        private bool GetChummerVersion()
        {
            LatestVersion = LanguageManager.GetString("String_No_Update_Found");
            string strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases/latest";
            if (_blnPreferNightly)
            {
                strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases";
            }
            HttpWebRequest request = null;
            try
            {
                WebRequest objTemp = WebRequest.Create(strUpdateLocation);
                request = objTemp as HttpWebRequest;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
            if (request == null)
                return false;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.Accept = "application/json";
            request.Timeout = 5000;

            // Get the response.
            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException)
            {
                return false;
            }

            // Get the stream containing content returned by the server.
            Stream dataStream = response?.GetResponseStream();
            if (dataStream == null)
                return false;
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.

            string responseFromServer = reader.ReadToEnd();
            string[] stringSeparators = new string[] { "," };
            string[] result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            bool blnFoundTag = false;
            bool blnFoundArchive = false;
            foreach (string line in result)
            {
                if (!blnFoundTag && line.Contains("tag_name"))
                {
                    _strLatestVersion = line.Split(':')[1];
                    LatestVersion = _strLatestVersion.Split('}')[0].FastEscape('\"');
                    blnFoundTag = true;
                    if (blnFoundArchive)
                        break;
                }
                if (!blnFoundArchive && line.Contains("browser_download_url"))
                {
                    _strDownloadFile = line.Split(':')[2];
                    _strDownloadFile = _strDownloadFile.Substring(2);
                    _strDownloadFile = _strDownloadFile.Split('}')[0].FastEscape('\"');
                    _strDownloadFile = "https://" + _strDownloadFile;
                    blnFoundArchive = true;
                    if (blnFoundTag)
                        break;
                }
            }
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
            return true;
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
                if (value)
                {
                    _workerConnectionLoader.RunWorkerAsync();
                }
            }
        }

        /// <summary>
        /// Latest release build number located on Github.
        /// </summary>
        public string LatestVersion
        {
            get
            {
                return _strLatestVersion;
            }
            set
            {
                _strLatestVersion = value;
                _strTempPath = Path.Combine(Path.GetTempPath(), "chummer" + _strLatestVersion + ".zip");
            }
        }

        /// <summary>
        /// Latest release build number located on Github.
        /// </summary>
        public string CurrentVersion
        {
            get
            {
                return _strCurrentVersion;
            }
        }

        public void DoVersionTextUpdate()
        {
            string strLatestVersion = LatestVersion.Trim().TrimStart("Nightly-v");
            lblUpdaterStatus.Left = lblUpdaterStatusLabel.Left + lblUpdaterStatusLabel.Width + 6;
            if (strLatestVersion == LanguageManager.GetString("String_No_Update_Found").Trim())
            {
                lblUpdaterStatus.Text = LanguageManager.GetString("Warning_Update_CouldNotConnect");
                cmdUpdate.Enabled = false;
                return;
            }
            Version.TryParse(strLatestVersion, out Version objLatestVersion);
            int intResult = objLatestVersion?.CompareTo(_objCurrentVersion) ?? 0;

            if (intResult > 0)
            {
                lblUpdaterStatus.Text = LanguageManager.GetString("String_Update_Available").Replace("{0}", strLatestVersion).Replace("{1}", _strCurrentVersion);
            }
            else
            {
                lblUpdaterStatus.Text = LanguageManager.GetString("String_Up_To_Date").Replace("{0}", _strCurrentVersion).Replace("{1}", LanguageManager.GetString(_blnPreferNightly ? "String_Nightly" : "String_Stable")).Replace("{2}", strLatestVersion);
                if (intResult < 0)
                {
                    cmdRestart.Text = LanguageManager.GetString("Button_Up_To_Date");
                    cmdRestart.Enabled = false;
                }
                cmdUpdate.Text = LanguageManager.GetString("Button_Redownload");
            }
            if (_blnPreferNightly)
                lblUpdaterStatus.Text += " " + LanguageManager.GetString("String_Nightly_Changelog_Warning");
        }

        private void cmdDownload_Click(object sender, EventArgs e)
        {
            Log.Info("cmdUpdate_Click");
            Log.Info("Download updates");
            DownloadUpdates();
        }

        private void cmdRestart_Click(object sender, EventArgs e)
        {
            Log.Info("cmdRestart_Click");
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempPath))
            {
                Cursor = Cursors.WaitCursor;
                cmdUpdate.Enabled = false;
                cmdRestart.Enabled = false;
                cmdCleanReinstall.Enabled = false;
                if (!CreateBackupZipAndRenameExes())
                {
                    Cursor = Cursors.Default;
                    return;
                }

                HashSet<string> lstFilesToDelete = new HashSet<string>(Directory.GetFiles(_strAppPath, "*", SearchOption.AllDirectories));
                HashSet<string> lstFilesToNotDelete = new HashSet<string>();
                foreach (string strFileToDelete in lstFilesToDelete)
                {
                    string strFileName = Path.GetFileName(strFileToDelete);
                    string strFilePath = Path.GetDirectoryName(strFileToDelete).TrimStart(_strAppPath);
                    int intSeparatorIndex = strFilePath.LastIndexOf(Path.DirectorySeparatorChar);
                    string strTopLevelFolder = intSeparatorIndex != -1 ? strFilePath.Substring(intSeparatorIndex + 1) : string.Empty;
                    if (strFileName.EndsWith(".old") ||
                        strFileName.StartsWith("custom") ||
                        strFileName.StartsWith("override") ||
                        strFileName.StartsWith("amend") ||
                        strFilePath.Contains("customdata") ||
                        strFilePath.Contains("saves") ||
                        strFilePath.Contains("settings") ||
                        (strFilePath.Contains("sheets") && strTopLevelFolder != "de" && strTopLevelFolder != "fr" && strTopLevelFolder != "jp" && strTopLevelFolder != "zh") ||
                        (strTopLevelFolder == "lang" && strFileName != "de.xml" && strFileName != "fr.xml" && strFileName != "jp.xml" && strFileName != "zh.xml" && strFileName != "de_data.xml" && strFileName != "fr_data.xml" && strFileName != "jp_data.xml" && strFileName != "zh_data.xml"))
                        lstFilesToNotDelete.Add(strFileToDelete);
                }
                lstFilesToDelete.RemoveWhere(x => lstFilesToNotDelete.Contains(x));

                InstallUpdateFromZip(_strTempPath, lstFilesToDelete);
            }
        }

        private void cmdCleanReinstall_Click(object sender, EventArgs e)
        {
            Log.Info("cmdCleanReinstall_Click");
            if (MessageBox.Show(LanguageManager.GetString("Message_Updater_CleanReinstallPrompt"),
                LanguageManager.GetString("MessageTitle_Updater_CleanReinstallPrompt"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempPath))
            {
                Cursor = Cursors.WaitCursor;
                cmdUpdate.Enabled = false;
                cmdRestart.Enabled = false;
                cmdCleanReinstall.Enabled = false;
                if (!CreateBackupZipAndRenameExes())
                {
                    Cursor = Cursors.Default;
                    return;
                }

                HashSet<string> lstFilesToDelete = new HashSet<string>(Directory.GetFiles(_strAppPath, "*", SearchOption.AllDirectories));
                HashSet<string> lstFilesToNotDelete = new HashSet<string>();
                foreach (string strFileToDelete in lstFilesToDelete)
                {
                    string strFileName = Path.GetFileName(strFileToDelete);
                    string strFilePath = Path.GetDirectoryName(strFileToDelete).TrimStart(_strAppPath);
                    int intSeparatorIndex = strFilePath.LastIndexOf(Path.DirectorySeparatorChar);
                    string strTopLevelFolder = intSeparatorIndex != -1 ? strFilePath.Substring(intSeparatorIndex + 1) : string.Empty;
                    if (strFileName.EndsWith(".old") || strFilePath.Contains("saves"))
                        lstFilesToNotDelete.Add(strFileToDelete);
                }
                lstFilesToDelete.RemoveWhere(x => lstFilesToNotDelete.Contains(x));

                InstallUpdateFromZip(_strTempPath, lstFilesToDelete);
            }
        }

        private bool CreateBackupZipAndRenameExes()
        {
            //Create a backup file in the temp directory. 
            string strBackupZipPath = Path.Combine(Path.GetTempPath(), "chummer" + CurrentVersion + ".zip");
            Log.Info("Creating archive from application path: ", _strAppPath);
            try
            {
                if (!File.Exists(strBackupZipPath))
                {
                    ZipFile.CreateFromDirectory(_strAppPath, strBackupZipPath, CompressionLevel.Fastest, true);
                }
                // Delete the old Chummer5 executables, libraries, and other files whose current versions are in use, then rename the current versions.
                foreach (string strLoopExeName in Directory.GetFiles(_strAppPath, "*.exe", SearchOption.AllDirectories))
                {
                    if (File.Exists(strLoopExeName + ".old"))
                        File.Delete(strLoopExeName + ".old");
                    File.Move(strLoopExeName, strLoopExeName + ".old");
                }
                foreach (string strLoopDllName in Directory.GetFiles(_strAppPath, "*.dll", SearchOption.AllDirectories))
                {
                    if (File.Exists(strLoopDllName + ".old"))
                        File.Delete(strLoopDllName + ".old");
                    File.Move(strLoopDllName, strLoopDllName + ".old");
                }
                foreach (string strLoopPdbName in Directory.GetFiles(_strAppPath, "*.pdb", SearchOption.AllDirectories))
                {
                    if (File.Exists(strLoopPdbName + ".old"))
                        File.Delete(strLoopPdbName + ".old");
                    File.Move(strLoopPdbName, strLoopPdbName + ".old");
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                return false;
            }
            return true;
        }

        private void InstallUpdateFromZip(string strZipPath, HashSet<string> lstFilesToDelete)
        {
            bool blnDoRestart = true;
            // Copy over the archive from the temp directory.
            Log.Info("Extracting downloaded archive into application path: ", strZipPath);
            using (ZipArchive archive = ZipFile.Open(strZipPath, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Skip directories because they already get handled with Directory.CreateDirectory
                    if (entry.FullName.Length > 0 && entry.FullName[entry.FullName.Length - 1] == '/')
                        continue;
                    string strLoopPath = Path.Combine(_strAppPath, entry.FullName);
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(strLoopPath));
                        entry.ExtractToFile(strLoopPath, true);
                    }
                    catch (IOException)
                    {
                        try
                        {
                            if (File.Exists(strLoopPath + ".old"))
                                File.Delete(strLoopPath + ".old");
                            File.Move(strLoopPath, strLoopPath + ".old");
                            Directory.CreateDirectory(Path.GetDirectoryName(strLoopPath));
                            entry.ExtractToFile(strLoopPath, true);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed") + "\n\n" + Path.GetFileName(strLoopPath));
                            blnDoRestart = false;
                            break;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                        blnDoRestart = false;
                        break;
                    }
                    lstFilesToDelete.Remove(strLoopPath.Replace('/', Path.DirectorySeparatorChar));
                }
            }
            if (blnDoRestart)
            {
                foreach (string strFileToDelete in lstFilesToDelete)
                {
                    if (File.Exists(strFileToDelete))
                        File.Delete(strFileToDelete);
                }
                Utils.RestartApplication(string.Empty);
            }
        }

        private void DownloadUpdates()
        {
            if (!Uri.TryCreate(_strDownloadFile, UriKind.Absolute, out Uri uriDownloadFileAddress))
                return;
            Log.Enter("DownloadUpdates");
            cmdUpdate.Enabled = false;
            cmdRestart.Enabled = false;
            cmdCleanReinstall.Enabled = false;
            if (File.Exists(_strTempPath))
                File.Delete(_strTempPath);
            try
            {
                _clientDownloader.DownloadFileAsync(uriDownloadFileAddress, _strTempPath);
            }
            catch (WebException)
            {
                // Show the warning even if we're in silent mode, because the user should still know that the update check could not be performed
                MessageBox.Show(LanguageManager.GetString("Warning_Update_CouldNotConnect"), "Chummer5", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmdUpdate.Enabled = true;
            }
        }

        #region AsyncDownload Events
        /// <summary>
        /// Update the download progress for the file.
        /// </summary>
        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (int.TryParse((e.BytesReceived * 100 / e.TotalBytesToReceive).ToString(), out int intTmp))
                pgbOverallProgress.Value = intTmp;
        }


        /// <summary>
        /// The EXE file is down downloading, so replace the old file with the new one.
        /// </summary>
        private void wc_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log.Info("wc_DownloadExeFileCompleted");
            cmdUpdate.Text = LanguageManager.GetString("Button_Redownload");
            cmdUpdate.Enabled = true;
            if (cmdRestart.Text != LanguageManager.GetString("Button_Up_To_Date"))
                cmdRestart.Enabled = true;
            cmdCleanReinstall.Enabled = true;
            Log.Exit("wc_DownloadExeFileCompleted");
            if (_blnSilentMode)
            {
                string text = LanguageManager.GetString("Message_Update_CloseForms");
                string caption = LanguageManager.GetString("Title_Update");

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cmdRestart_Click(sender, e);
                }
                else
                {
                    _blnIsConnected = false;
                    this.DoThreadSafe(new Action(() => Close()));
                }
            }
        }
        #endregion
    }
}
