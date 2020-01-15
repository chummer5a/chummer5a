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
 using System;
using System.ComponentModel;
 using System.IO;
 using System.IO.Compression;
 using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
 using Application = System.Windows.Forms.Application;
 using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Generic;
 using System.Linq;
 using System.Threading;
 using NLog;

namespace Chummer
{
    public partial class frmUpdate : Form
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private bool _blnSilentMode;
        private string _strDownloadFile = string.Empty;
        private string _strLatestVersion = string.Empty;
        private readonly Version _objCurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private string _strTempPath = string.Empty;
        private readonly string _strTempUpdatePath;
        private readonly string _strAppPath = Utils.GetStartupPath;
        private readonly bool _blnPreferNightly;
        private bool _blnIsConnected = true;
        private readonly bool _blnChangelogDownloaded = false;
        private readonly BackgroundWorker _workerConnectionLoader = new BackgroundWorker();
        private readonly WebClient _clientDownloader = new WebClient();
        private readonly WebClient _clientChangelogDownloader = new WebClient();
        private string _strExceptionString;

        public frmUpdate()
        {
            Log.Info("frmUpdate");
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            CurrentVersion = $"{_objCurrentVersion.Major}.{_objCurrentVersion.Minor}.{_objCurrentVersion.Build}";
            _blnPreferNightly = GlobalOptions.Instance.PreferNightlyBuilds;
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
            Log.Info("frmUpdate_Load enter");
            Log.Info("Check Global Mutex for duplicate");
            bool blnHasDuplicate = false;
            try
            {
                blnHasDuplicate = !Program.GlobalChummerMutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException ex)
            {
                Log.Error(ex);
                Utils.BreakIfDebug();
                blnHasDuplicate = true;
            }
            Log.Info("blnHasDuplicate = " + blnHasDuplicate.ToString());
            // If there is more than 1 instance running, do not let the application be updated.
            if (blnHasDuplicate)
            {
                Log.Info("More than one instance, exiting");
                if (!SilentMode)
                    MessageBox.Show(LanguageManager.GetString("Message_Update_MultipleInstances", GlobalOptions.Language), LanguageManager.GetString("Title_Update", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Log.Info("frmUpdate_Load exit");
                Close();
            }
            if (!_blnChangelogDownloaded && !_workerConnectionLoader.IsBusy)
            {
                _workerConnectionLoader.RunWorkerAsync();
            }
            Log.Info("frmUpdate_Load exit");
        }

        private bool _blnIsClosing;
        private void frmUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            _blnIsClosing = true;
            _workerConnectionLoader.CancelAsync();
            _clientDownloader.CancelAsync();
            _clientChangelogDownloader.CancelAsync();
        }

        private void PopulateChangelog(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                if (!_blnIsClosing)
                    Close();
                return;
            }
            if (!_clientDownloader.IsBusy)
                cmdUpdate.Enabled = true;
            if (SilentMode)
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
                                                    .Replace(Environment.NewLine, "<br />")
                                                    .Replace("\n", "<br />") + "</font>";
            }
            DoVersionTextUpdate();
        }

        private void LoadConnection(object sender, DoWorkEventArgs e)
        {
            if (_clientChangelogDownloader.IsBusy)
                return;
            bool blnChummerVersionGotten = true;
            string strError = LanguageManager.GetString("String_Error", GlobalOptions.Language).Trim();
            _strExceptionString = string.Empty;
            LatestVersion = strError;
            string strUpdateLocation = _blnPreferNightly
                ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
                : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest";

            HttpWebRequest request = null;
            try
            {
                WebRequest objTemp = WebRequest.Create(strUpdateLocation);
                request = objTemp as HttpWebRequest;
            }
            catch (System.Security.SecurityException)
            {
                blnChummerVersionGotten = false;
            }
            if (request == null)
                blnChummerVersionGotten = false;
            if (blnChummerVersionGotten)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
                request.Accept = "application/json";
                request.Timeout = 5000;

                // Get the response.
                HttpWebResponse response = null;
                try
                {
                    response = request.GetResponse() as HttpWebResponse;
                }
                catch (WebException ex)
                {
                    blnChummerVersionGotten = false;
                    string strException = ex.ToString();
                    int intNewLineLocation = strException.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (intNewLineLocation == -1)
                        intNewLineLocation = strException.IndexOf('\n');
                    if (intNewLineLocation != -1)
                        strException = strException.Substring(0, intNewLineLocation);
                    _strExceptionString = strException;
                }

                if (_workerConnectionLoader.CancellationPending)
                {
                    e.Cancel = true;
                    response?.Close();
                    return;
                }

                // Get the stream containing content returned by the server.
                Stream dataStream = response?.GetResponseStream();
                if (dataStream == null)
                    blnChummerVersionGotten = false;
                if (blnChummerVersionGotten)
                {
                    if (_workerConnectionLoader.CancellationPending)
                    {
                        e.Cancel = true;
                        dataStream.Close();
                        response.Close();
                        return;
                    }
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream, Encoding.UTF8, true);
                    // Read the content.

                    if (_workerConnectionLoader.CancellationPending)
                    {
                        e.Cancel = true;
                        reader.Close();
                        response.Close();
                        return;
                    }

                    string responseFromServer = reader.ReadToEnd();

                    if (_workerConnectionLoader.CancellationPending)
                    {
                        e.Cancel = true;
                        reader.Close();
                        response.Close();
                        return;
                    }

                    string[] stringSeparators = { "," };

                    if (_workerConnectionLoader.CancellationPending)
                    {
                        e.Cancel = true;
                        reader.Close();
                        response.Close();
                        return;
                    }

                    string[] result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    bool blnFoundTag = false;
                    bool blnFoundArchive = false;
                    foreach (string line in result)
                    {
                        if (_workerConnectionLoader.CancellationPending)
                        {
                            e.Cancel = true;
                            reader.Close();
                            response.Close();
                            return;
                        }
                        if (!blnFoundTag && line.Contains("tag_name"))
                        {
                            _strLatestVersion = line.Split(':')[1];
                            LatestVersion = _strLatestVersion.Split('}')[0].FastEscape('\"').Trim();
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
                    if (!blnFoundArchive || !blnFoundTag)
                        blnChummerVersionGotten = false;
                    // Cleanup the streams and the response.
                    reader.Close();
                }
                dataStream?.Close();
                response?.Close();
            }
            if (!blnChummerVersionGotten || LatestVersion == strError)
            {
                MessageBox.Show(
                    string.IsNullOrEmpty(_strExceptionString)
                        ? LanguageManager.GetString("Warning_Update_CouldNotConnect", GlobalOptions.Language)
                        : string.Format(LanguageManager.GetString("Warning_Update_CouldNotConnectException", GlobalOptions.Language), _strExceptionString), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _blnIsConnected = false;
                e.Cancel = true;
            }
            else
            {
                if (File.Exists(_strTempUpdatePath))
                {
                    if (File.Exists(_strTempUpdatePath + ".old"))
                        File.Delete(_strTempUpdatePath + ".old");
                    File.Move(_strTempUpdatePath, _strTempUpdatePath + ".old");
                }
                string strURL = "https://raw.githubusercontent.com/chummer5a/chummer5a/" + LatestVersion + "/Chummer/changelog.txt";
                try
                {
                    Uri uriConnectionAddress = new Uri(strURL);
                    if (File.Exists(_strTempUpdatePath + ".tmp"))
                        File.Delete(_strTempUpdatePath + ".tmp");
                    _clientChangelogDownloader.DownloadFileAsync(uriConnectionAddress, _strTempUpdatePath + ".tmp");
                    while (_clientChangelogDownloader.IsBusy)
                    {
                        if (_workerConnectionLoader.CancellationPending)
                        {
                            _clientChangelogDownloader.CancelAsync();
                            e.Cancel = true;
                            return;
                        }
                    }
                    File.Move(_strTempUpdatePath + ".tmp", _strTempUpdatePath);
                }
                catch (WebException ex)
                {
                    string strException = ex.ToString();
                    int intNewLineLocation = strException.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (intNewLineLocation == -1)
                        intNewLineLocation = strException.IndexOf('\n');
                    if (intNewLineLocation != -1)
                        strException = strException.Substring(0, intNewLineLocation);
                    _strExceptionString = strException;
                    MessageBox.Show(string.Format(LanguageManager.GetString("Warning_Update_CouldNotConnectException", GlobalOptions.Language), strException), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _blnIsConnected = false;
                    e.Cancel = true;
                }
                catch (UriFormatException ex)
                {
                    string strException = ex.ToString();
                    int intNewLineLocation = strException.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (intNewLineLocation == -1)
                        intNewLineLocation = strException.IndexOf('\n');
                    if (intNewLineLocation != -1)
                        strException = strException.Substring(0, intNewLineLocation);
                    _strExceptionString = strException;
                    MessageBox.Show(string.Format(LanguageManager.GetString("Warning_Update_CouldNotConnectException", GlobalOptions.Language), strException), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _blnIsConnected = false;
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// When running in silent mode, the update window will not be shown.
        /// </summary>
        public bool SilentMode
        {
            get => _blnSilentMode;
            set
            {
                _blnSilentMode = value;
                if (value && !_workerConnectionLoader.IsBusy)
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
            get => _strLatestVersion;
            set
            {
                _strLatestVersion = value;
                _strTempPath = Path.Combine(Path.GetTempPath(), "chummer" + _strLatestVersion + ".zip");
            }
        }

        /// <summary>
        /// Latest release build number located on Github.
        /// </summary>
        public string CurrentVersion { get; }

        public void DoVersionTextUpdate()
        {
            string strLatestVersion = LatestVersion.TrimStartOnce("Nightly-v");
            lblUpdaterStatus.Left = lblUpdaterStatusLabel.Left + lblUpdaterStatusLabel.Width + 6;
            if (strLatestVersion == LanguageManager.GetString("String_Error", GlobalOptions.Language).Trim())
            {
                lblUpdaterStatus.Text = string.IsNullOrEmpty(_strExceptionString)
                    ? LanguageManager.GetString("Warning_Update_CouldNotConnect", GlobalOptions.Language)
                    : string.Format(LanguageManager.GetString("Warning_Update_CouldNotConnectException", GlobalOptions.Language).NormalizeWhiteSpace(), _strExceptionString);
                cmdUpdate.Enabled = false;
                return;
            }
            Version.TryParse(strLatestVersion, out Version objLatestVersion);
            int intResult = objLatestVersion?.CompareTo(_objCurrentVersion) ?? 0;

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            if (intResult > 0)
            {
                lblUpdaterStatus.Text = string.Format(LanguageManager.GetString("String_Update_Available", GlobalOptions.Language), strLatestVersion) + strSpaceCharacter +
                                        string.Format(LanguageManager.GetString("String_Currently_Installed_Version", GlobalOptions.Language), CurrentVersion);
            }
            else
            {
                lblUpdaterStatus.Text = LanguageManager.GetString("String_Up_To_Date", GlobalOptions.Language) + strSpaceCharacter +
                                        string.Format(LanguageManager.GetString("String_Currently_Installed_Version", GlobalOptions.Language), CurrentVersion) + strSpaceCharacter +
                                        string.Format(LanguageManager.GetString("String_Latest_Version", GlobalOptions.Language), LanguageManager.GetString(_blnPreferNightly ? "String_Nightly" : "String_Stable", GlobalOptions.Language), strLatestVersion);
                if (intResult < 0)
                {
                    cmdRestart.Text = LanguageManager.GetString("Button_Up_To_Date", GlobalOptions.Language);
                    cmdRestart.Enabled = false;
                }
                cmdUpdate.Text = LanguageManager.GetString("Button_Redownload", GlobalOptions.Language);
            }
            if (_blnPreferNightly)
                lblUpdaterStatus.Text += strSpaceCharacter + LanguageManager.GetString("String_Nightly_Changelog_Warning", GlobalOptions.Language);
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
                if (!CreateBackupZip())
                {
                    Cursor = Cursors.Default;
                    return;
                }

                HashSet<string> lstFilesToDelete = new HashSet<string>(Directory.GetFiles(_strAppPath, "*", SearchOption.AllDirectories));
                HashSet<string> lstFilesToNotDelete = new HashSet<string>();
                foreach (string strFileToDelete in lstFilesToDelete)
                {
                    string strFileName = Path.GetFileName(strFileToDelete);
                    string strFilePath = Path.GetDirectoryName(strFileToDelete).TrimStartOnce(_strAppPath);
                    int intSeparatorIndex = strFilePath.LastIndexOf(Path.DirectorySeparatorChar);
                    string strTopLevelFolder = intSeparatorIndex != -1 ? strFilePath.Substring(intSeparatorIndex + 1) : string.Empty;
                    if ((!strFilePath.StartsWith("data") && !strFilePath.StartsWith("export") &&
                         !strFilePath.StartsWith("lang") && !strFilePath.StartsWith("sheets") &&
                         !strFilePath.StartsWith("saves") && !strFilePath.StartsWith("Utils") &&
                         !string.IsNullOrEmpty(strFilePath.TrimEndOnce(strFileName))) ||
                        strFileName?.EndsWith(".old") != false || strFileName.EndsWith(".chum5") ||
                        strFileName.StartsWith("custom") || strFileName.StartsWith("override") ||
                        strFileName.StartsWith("amend") ||
                        (strFilePath.Contains("sheets") && strTopLevelFolder != "de" && strTopLevelFolder != "fr" &&
                         strTopLevelFolder != "jp" && strTopLevelFolder != "zh") || (strTopLevelFolder == "lang" &&
                                                                                     strFileName != "de.xml" &&
                                                                                     strFileName != "fr.xml" &&
                                                                                     strFileName != "jp.xml" &&
                                                                                     strFileName != "zh.xml" &&
                                                                                     strFileName != "de_data.xml" &&
                                                                                     strFileName != "fr_data.xml" &&
                                                                                     strFileName != "jp_data.xml" &&
                                                                                     strFileName != "zh_data.xml"))
                        lstFilesToNotDelete.Add(strFileToDelete);
                }
                lstFilesToDelete.RemoveWhere(x => lstFilesToNotDelete.Contains(x));

                InstallUpdateFromZip(_strTempPath, lstFilesToDelete);
            }
        }

        private void cmdCleanReinstall_Click(object sender, EventArgs e)
        {
            Log.Info("cmdCleanReinstall_Click");
            if (MessageBox.Show(LanguageManager.GetString("Message_Updater_CleanReinstallPrompt", GlobalOptions.Language),
                LanguageManager.GetString("MessageTitle_Updater_CleanReinstallPrompt", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempPath))
            {
                Cursor = Cursors.WaitCursor;
                cmdUpdate.Enabled = false;
                cmdRestart.Enabled = false;
                cmdCleanReinstall.Enabled = false;
                if (!CreateBackupZip())
                {
                    Cursor = Cursors.Default;
                    return;
                }

                HashSet<string> lstFilesToDelete = new HashSet<string>(Directory.GetFiles(_strAppPath, "*", SearchOption.AllDirectories));
                HashSet<string> lstFilesToNotDelete = new HashSet<string>();
                foreach (string strFileToDelete in lstFilesToDelete)
                {
                    string strFileName = Path.GetFileName(strFileToDelete);
                    string strFilePath = Path.GetDirectoryName(strFileToDelete).TrimStartOnce(_strAppPath);
                    if (!strFilePath.StartsWith("customdata") &&
                        !strFilePath.StartsWith("data") &&
                        !strFilePath.StartsWith("export") &&
                        !strFilePath.StartsWith("lang") &&
                        !strFilePath.StartsWith("saves") &&
                        !strFilePath.StartsWith("settings") &&
                        !strFilePath.StartsWith("sheets") &&
                        !strFilePath.StartsWith("Utils") &&
                        !string.IsNullOrEmpty(strFilePath.TrimEndOnce(strFileName)) ||
                        strFileName?.EndsWith(".old") != false || strFileName.EndsWith(".chum5"))
                        lstFilesToNotDelete.Add(strFileToDelete);
                }
                lstFilesToDelete.RemoveWhere(x => lstFilesToNotDelete.Contains(x));

                InstallUpdateFromZip(_strTempPath, lstFilesToDelete);
            }
        }

        private bool CreateBackupZip()
        {
            //Create a backup file in the temp directory.
            string strBackupZipPath = Path.Combine(Path.GetTempPath(), "chummer" + CurrentVersion + ".zip");
            Log.Info("Creating archive from application path: " + _strAppPath);
            try
            {
                if (!File.Exists(strBackupZipPath))
                {
                    ZipFile.CreateFromDirectory(_strAppPath, strBackupZipPath, CompressionLevel.Fastest, true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                return false;
            }
            catch (IOException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + Environment.NewLine + Environment.NewLine + Path.GetFileName(strBackupZipPath));
                return false;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + Environment.NewLine + Environment.NewLine + Path.GetFileName(strBackupZipPath));
                return false;
            }
            return true;
        }

        private void InstallUpdateFromZip(string strZipPath, HashSet<string> lstFilesToDelete)
        {
            bool blnDoRestart = true;
            // Copy over the archive from the temp directory.
            Log.Info("Extracting downloaded archive into application path: " + strZipPath);
            try
            {
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
                            string strLoopDirectory = Path.GetDirectoryName(strLoopPath);
                            if (!string.IsNullOrEmpty(strLoopDirectory))
                                Directory.CreateDirectory(strLoopDirectory);
                            if (File.Exists(strLoopPath))
                            {
                                if (File.Exists(strLoopPath + ".old"))
                                    File.Delete(strLoopPath + ".old");
                                File.Move(strLoopPath, strLoopPath + ".old");
                            }
                            entry.ExtractToFile(strLoopPath, false);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + Environment.NewLine + Environment.NewLine + Path.GetFileName(strLoopPath));
                            blnDoRestart = false;
                            break;
                        }
                        catch (NotSupportedException)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + Environment.NewLine + Environment.NewLine + Path.GetFileName(strLoopPath));
                            blnDoRestart = false;
                            break;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                            blnDoRestart = false;
                            break;
                        }
                        lstFilesToDelete.Remove(strLoopPath.Replace('/', Path.DirectorySeparatorChar));
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + Environment.NewLine + Environment.NewLine + strZipPath);
                blnDoRestart = false;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + Environment.NewLine + Environment.NewLine + strZipPath);
                blnDoRestart = false;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                blnDoRestart = false;
            }
            if (blnDoRestart)
            {
                List<string> lstBlocked = new List<string>();
                foreach (var strFileToDelete in lstFilesToDelete)
                {
                    //TODO: This will quite likely leave some wreckage behind. Introduce a sleep and scream after x seconds. 
                    if (!IsFileLocked(strFileToDelete))
                        try
                        {
                            File.Delete(strFileToDelete);
                        }
                        catch (IOException)
                        {
                            lstBlocked.Add(strFileToDelete);
                        }
                    else
                        Utils.BreakIfDebug();
                }

                /*TODO: It seems like the most likely cause here is that the ChummerHub plugin is holding onto the REST API dlls.
                //      Investigate a solution for this; possibly do something to shut down plugins while updating.
                //      Likely best option is a helper exe that caches opened characters and other relevant variables, relaunching after update is complete. 
                 if (lstBlocked.Count > 0)
                {
                    var output = LanguageManager.GetString("Message_Files_Cannot_Be_Removed",
                        GlobalOptions.Language);
                    output = lstBlocked.Aggregate(output, (current, s) => current + Environment.NewLine + s);

                    MessageBox.Show(output);
                }*/
                Utils.RestartApplication(GlobalOptions.Language, string.Empty);
            }
            else
            {
                foreach (string strBackupFileName in Directory.GetFiles(_strAppPath, "*.old", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Move(strBackupFileName, strBackupFileName.Substring(0, strBackupFileName.Length - 4));
                    }
                    catch (IOException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }
        }

        private void DownloadUpdates()
        {
            if (!Uri.TryCreate(_strDownloadFile, UriKind.Absolute, out Uri uriDownloadFileAddress))
                return;
            Log.Debug("DownloadUpdates");
            cmdUpdate.Enabled = false;
            cmdRestart.Enabled = false;
            cmdCleanReinstall.Enabled = false;
            if (File.Exists(_strTempPath))
                File.Delete(_strTempPath);
            try
            {
                _clientDownloader.DownloadFileAsync(uriDownloadFileAddress, _strTempPath);
            }
            catch (WebException ex)
            {
                string strException = ex.ToString();
                int intNewLineLocation = strException.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (intNewLineLocation == -1)
                    intNewLineLocation = strException.IndexOf('\n');
                if (intNewLineLocation != -1)
                    strException = strException.Substring(0, intNewLineLocation);
                // Show the warning even if we're in silent mode, because the user should still know that the update check could not be performed
                MessageBox.Show(string.Format(LanguageManager.GetString("Warning_Update_CouldNotConnectException", GlobalOptions.Language), strException), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmdUpdate.Enabled = true;
            }
        }

        /// <summary>
        /// Test if the file at a given path is accessible to write operations. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns>File is locked if True.</returns>
        protected virtual bool IsFileLocked(string path)
        {
            try
            {
                File.Open(path, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                // File doesn't exist. 
                return true;
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            catch (Exception)
            {
                Utils.BreakIfDebug();
                return true;
            }

            //file is not locked
            return false;
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
            Log.Info("wc_DownloadExeFileCompleted enter");
            cmdUpdate.Text = LanguageManager.GetString("Button_Redownload", GlobalOptions.Language);
            cmdUpdate.Enabled = true;
            if (cmdRestart.Text != LanguageManager.GetString("Button_Up_To_Date", GlobalOptions.Language))
                cmdRestart.Enabled = true;
            cmdCleanReinstall.Enabled = true;
            Log.Info("wc_DownloadExeFileCompleted exit");
            if (SilentMode)
            {
                string text = LanguageManager.GetString("Message_Update_CloseForms", GlobalOptions.Language);
                string caption = LanguageManager.GetString("Title_Update", GlobalOptions.Language);

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cmdRestart_Click(sender, e);
                }
                else
                {
                    _blnIsConnected = false;
                    this.DoThreadSafe(Close);
                }
            }
        }
        #endregion
    }
}
