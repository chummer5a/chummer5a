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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using Application = System.Windows.Forms.Application;

namespace Chummer
{
    public partial class ChummerUpdater : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private bool _blnSilentMode;
        private bool _blnSilentModeUpdateWasDenied;
        private string _strDownloadFile = string.Empty;
        private string _strLatestVersion = string.Empty;
        private string _strTempLatestVersionZipPath = string.Empty;
        private readonly string _strTempLatestVersionChangelogPath;
        private readonly string _strAppPath = Utils.GetStartupPath;
        private readonly bool _blnPreferNightly;
        private bool _blnIsConnected;
        private Task _tskConnectionLoader;
        private CancellationTokenSource _objConnectionLoaderCancellationTokenSource;
        private readonly WebClient _clientDownloader;
        private readonly WebClient _clientChangelogDownloader;
        private string _strExceptionString;

        public ChummerUpdater()
        {
            Log.Info("ChummerUpdater");
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            CurrentVersion = Utils.CurrentChummerVersion.ToString(3);
            _blnPreferNightly = GlobalSettings.PreferNightlyBuilds;
            _strTempLatestVersionChangelogPath = Path.Combine(Utils.GetTempPath(), "changelog.txt");
            _clientChangelogDownloader = new WebClient { Proxy = WebRequest.DefaultWebProxy, Encoding = Encoding.UTF8, Credentials = CredentialCache.DefaultNetworkCredentials };
            _clientDownloader = new WebClient { Proxy = WebRequest.DefaultWebProxy, Encoding = Encoding.UTF8, Credentials = CredentialCache.DefaultNetworkCredentials };
            _clientDownloader.DownloadFileCompleted += wc_DownloadCompleted;
            _clientDownloader.DownloadProgressChanged += wc_DownloadProgressChanged;
        }

        private async void ChummerUpdater_Load(object sender, EventArgs e)
        {
            Log.Info("ChummerUpdater_Load enter");
            Log.Info("Check Global Mutex for duplicate");
            bool blnHasDuplicate;
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
            Log.Info("blnHasDuplicate = " + blnHasDuplicate.ToString(GlobalSettings.InvariantCultureInfo));
            // If there is more than 1 instance running, do not let the application be updated.
            if (blnHasDuplicate)
            {
                Log.Info("More than one instance, exiting");
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Update_MultipleInstances"), await LanguageManager.GetStringAsync("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Log.Info("ChummerUpdater_Load exit");
                Close();
            }
            if (_tskConnectionLoader == null || (_tskConnectionLoader.IsCompleted && (_tskConnectionLoader.IsCanceled ||
                _tskConnectionLoader.IsFaulted)))
            {
                await DownloadChangelog();
            }
            if (_blnIsConnected && SilentMode && !_blnSilentModeUpdateWasDenied)
            {
                await DownloadUpdates();
            }
            Log.Info("ChummerUpdater_Load exit");
        }

        private bool _blnFormClosing;

        private void ChummerUpdater_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If we have automatic updates on, make sure we don't close the updater, just hide it
            if (e.CloseReason == CloseReason.UserClosing && GlobalSettings.AutomaticUpdate)
            {
                e.Cancel = true;
                Hide();
                SilentMode = true;
                return;
            }
            _blnFormClosing = true;
            _objConnectionLoaderCancellationTokenSource?.Cancel(false);
            _clientDownloader.CancelAsync();
            _clientChangelogDownloader.CancelAsync();
        }

        private async Task DownloadChangelog()
        {
            if (_objConnectionLoaderCancellationTokenSource != null)
            {
                _objConnectionLoaderCancellationTokenSource.Cancel(false);
                _objConnectionLoaderCancellationTokenSource.Dispose();
            }
            _objConnectionLoaderCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskConnectionLoader?.IsCompleted == false)
                    await _tskConnectionLoader;
            }
            catch (TaskCanceledException)
            {
                // Swallow this
            }
            _tskConnectionLoader = Task.Run(async () =>
            {
                await LoadConnection();
                if (!_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                    await PopulateChangelog();
            }, _objConnectionLoaderCancellationTokenSource.Token);
        }

        private async Task PopulateChangelog()
        {
            if (!_blnFormClosing)
            {
                if (!_clientDownloader.IsBusy)
                    await cmdUpdate.DoThreadSafeAsync(() => cmdUpdate.Enabled = true);
                if (File.Exists(_strTempLatestVersionChangelogPath))
                {
                    string strUpdateLog = File.ReadAllText(_strTempLatestVersionChangelogPath).CleanForHtml();
                    await webNotes.DoThreadSafeAsync(() => webNotes.DocumentText
                                                         = "<font size=\"-1\" face=\"Courier New,Serif\">"
                                                           + strUpdateLog + "</font>");
                }
                await this.DoThreadSafeAsync(DoVersionTextUpdate);
            }
        }

        private async Task LoadConnection()
        {
            while (_clientChangelogDownloader.IsBusy)
                await Utils.SafeSleepAsync();
            _blnIsConnected = false;
            if (_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                return;
            bool blnChummerVersionGotten = true;
            string strError = (await LanguageManager.GetStringAsync("String_Error")).Trim();
            _strExceptionString = string.Empty;
            LatestVersion = strError;
            Uri uriUpdateLocation = new Uri(_blnPreferNightly
                ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
                : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest");

            HttpWebRequest request = null;
            try
            {
                WebRequest objTemp = WebRequest.Create(uriUpdateLocation);
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
                    response = await request.GetResponseAsync() as HttpWebResponse;

                    if (_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                        return;

                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response?.GetResponseStream())
                    {
                        if (dataStream == null)
                            blnChummerVersionGotten = false;
                        if (blnChummerVersionGotten)
                        {
                            if (_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                                return;

                            // Open the stream using a StreamReader for easy access.
                            string responseFromServer;
                            using (StreamReader reader = new StreamReader(dataStream, Encoding.UTF8, true))
                                responseFromServer = await reader.ReadToEndAsync();

                            if (_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                                return;

                            bool blnFoundTag = false;
                            bool blnFoundArchive = false;
                            foreach (string line in responseFromServer.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                                    return;

                                if (!blnFoundTag && line.Contains("tag_name"))
                                {
                                    _strLatestVersion = line.SplitNoAlloc(':').ElementAtOrDefault(1);
                                    LatestVersion = _strLatestVersion.SplitNoAlloc('}').FirstOrDefault().FastEscape('\"').Trim();
                                    blnFoundTag = true;
                                    if (blnFoundArchive)
                                        break;
                                }

                                if (!blnFoundArchive && line.Contains("browser_download_url"))
                                {
                                    _strDownloadFile = line.SplitNoAlloc(':').ElementAtOrDefault(2) ?? string.Empty;
                                    _strDownloadFile = _strDownloadFile.Substring(2);
                                    _strDownloadFile = _strDownloadFile.SplitNoAlloc('}').FirstOrDefault().FastEscape('\"');
                                    _strDownloadFile = "https://" + _strDownloadFile;
                                    blnFoundArchive = true;
                                    if (blnFoundTag)
                                        break;
                                }
                            }

                            if (!blnFoundArchive || !blnFoundTag)
                                blnChummerVersionGotten = false;
                        }
                    }
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
                finally
                {
                    response?.Close();
                }
            }
            if (!blnChummerVersionGotten || LatestVersion == strError)
            {
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this,
                    string.IsNullOrEmpty(_strExceptionString)
                        ? await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnect")
                        : string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException"), _strExceptionString),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(_strTempLatestVersionChangelogPath))
            {
                if (!await Utils.SafeDeleteFileAsync(_strTempLatestVersionChangelogPath + ".old", !SilentMode))
                    return;
                File.Move(_strTempLatestVersionChangelogPath, _strTempLatestVersionChangelogPath + ".old");
            }
            string strUrl = "https://raw.githubusercontent.com/chummer5a/chummer5a/" + LatestVersion + "/Chummer/changelog.txt";
            try
            {
                Uri uriConnectionAddress = new Uri(strUrl);
                if (!await Utils.SafeDeleteFileAsync(_strTempLatestVersionChangelogPath + ".tmp", !SilentMode))
                    return;
                await _clientChangelogDownloader.DownloadFileTaskAsync(uriConnectionAddress, _strTempLatestVersionChangelogPath + ".tmp");
                if (_objConnectionLoaderCancellationTokenSource.IsCancellationRequested)
                    return;
                File.Move(_strTempLatestVersionChangelogPath + ".tmp", _strTempLatestVersionChangelogPath);
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
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException"), strException),
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException"), strException),
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _blnIsConnected = true;
        }

        /// <summary>
        /// When running in silent mode, the update window will not be shown.
        /// </summary>
        public bool SilentMode
        {
            get => _blnSilentMode;
            set
            {
                if (_blnSilentMode == value)
                    return;
                _blnSilentMode = value;
                if (value && (_tskConnectionLoader == null || (_tskConnectionLoader.IsCompleted && (_tskConnectionLoader.IsCanceled ||
                    _tskConnectionLoader.IsFaulted))))
                {
                    Utils.RunWithoutThreadLock(DownloadChangelog);
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
                _strTempLatestVersionZipPath = Path.Combine(Utils.GetTempPath(), "chummer" + _strLatestVersion + ".zip");
            }
        }

        /// <summary>
        /// Latest release build number located on Github.
        /// </summary>
        public string CurrentVersion { get; }

        public async Task DoVersionTextUpdate()
        {
            string strLatestVersion = LatestVersion.TrimStartOnce("Nightly-v");
            lblUpdaterStatus.Left = lblUpdaterStatusLabel.Left + lblUpdaterStatusLabel.Width + 6;
            if (!_blnIsConnected || strLatestVersion == (await LanguageManager.GetStringAsync("String_Error")).Trim())
            {
                lblUpdaterStatus.Text = string.IsNullOrEmpty(_strExceptionString)
                    ? await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnect")
                    : string.Format(GlobalSettings.CultureInfo, (await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException")).NormalizeWhiteSpace(), _strExceptionString);
                cmdUpdate.Text = await LanguageManager.GetStringAsync("Button_Reconnect");
                cmdRestart.Enabled = false;
                cmdCleanReinstall.Enabled = false;
                return;
            }

            int intResult = 0;
            if (VersionExtensions.TryParse(strLatestVersion, out Version objLatestVersion))
                intResult = objLatestVersion?.CompareTo(Utils.CurrentChummerVersion) ?? 0;

            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            if (intResult > 0)
            {
                lblUpdaterStatus.Text = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Update_Available"), strLatestVersion) + strSpace +
                                        string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Currently_Installed_Version"), CurrentVersion);
                cmdUpdate.Text = await LanguageManager.GetStringAsync("Button_Download");
            }
            else
            {
                lblUpdaterStatus.Text = await LanguageManager.GetStringAsync("String_Up_To_Date") + strSpace +
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("String_Currently_Installed_Version"),
                                            CurrentVersion) + strSpace + string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("String_Latest_Version"),
                                            await LanguageManager.GetStringAsync(_blnPreferNightly
                                                ? "String_Nightly"
                                                : "String_Stable"), strLatestVersion);
                if (intResult < 0)
                {
                    cmdRestart.Text = await LanguageManager.GetStringAsync("Button_Up_To_Date");
                    cmdRestart.Enabled = false;
                }
                cmdUpdate.Text = await LanguageManager.GetStringAsync("Button_Redownload");
            }
            if (_blnPreferNightly)
                lblUpdaterStatus.Text += strSpace + await LanguageManager.GetStringAsync("String_Nightly_Changelog_Warning");
        }

        private async void cmdUpdate_Click(object sender, EventArgs e)
        {
            Log.Info("cmdUpdate_Click");
            if (_blnIsConnected)
                await DownloadUpdates();
            else if (_tskConnectionLoader == null || (_tskConnectionLoader.IsCompleted && (_tskConnectionLoader.IsCanceled ||
                _tskConnectionLoader.IsFaulted)))
            {
                cmdUpdate.Enabled = false;
                await DownloadChangelog();
            }
        }

        private async void cmdRestart_Click(object sender, EventArgs e)
        {
            Log.Info("cmdRestart_Click");
            await DoUpdate();
        }

        private async void cmdCleanReinstall_Click(object sender, EventArgs e)
        {
            Log.Info("cmdCleanReinstall_Click");
            await DoCleanReinstall();
        }

        private async Task<bool> CreateBackupZip()
        {
            //Create a backup file in the temp directory.
            string strBackupZipPath = Path.Combine(Utils.GetTempPath(), "chummer" + CurrentVersion + ".zip");
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
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                return false;
            }
            catch (IOException)
            {
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"), Path.GetFileName(strBackupZipPath)));
                return false;
            }
            catch (NotSupportedException)
            {
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"), Path.GetFileName(strBackupZipPath)));
                return false;
            }
            return true;
        }

        private async Task DoUpdate()
        {
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempLatestVersionZipPath))
            {
                using (new CursorWait())
                {
                    cmdUpdate.Enabled = false;
                    cmdRestart.Enabled = false;
                    cmdCleanReinstall.Enabled = false;
                    if (!(await CreateBackupZip()))
                        return;

                    string[] astrAllFiles = Directory.GetFiles(_strAppPath, "*", SearchOption.AllDirectories);
                    List<string> lstFilesToDelete = new List<string>(astrAllFiles.Length);
                    foreach (string strFileToDelete in astrAllFiles)
                    {
                        string strFileName = Path.GetFileName(strFileToDelete);
                        if (string.IsNullOrEmpty(strFileName)
                            || strFileName.EndsWith(".old", StringComparison.OrdinalIgnoreCase)
                            || strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFileName.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                            || strFileName.StartsWith("override_", StringComparison.OrdinalIgnoreCase)
                            || strFileName.StartsWith("amend_", StringComparison.OrdinalIgnoreCase))
                            continue;
                        string strFilePath = Path.GetDirectoryName(strFileToDelete).TrimStartOnce(_strAppPath);
                        if (!strFilePath.StartsWith("data", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("export", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("lang", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("saves", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("sheets", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("Utils", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(strFilePath.TrimEndOnce(strFileName)))
                            continue;
                        int intSeparatorIndex = strFilePath.LastIndexOf(Path.DirectorySeparatorChar);
                        string strTopLevelFolder = intSeparatorIndex != -1
                            ? strFilePath.Substring(intSeparatorIndex + 1)
                            : string.Empty;
                        if (strFilePath.Contains("sheets", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strTopLevelFolder, "sheets", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strTopLevelFolder, "de-de", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strTopLevelFolder, "fr-fr", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strTopLevelFolder, "ja-jp", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strTopLevelFolder, "pt-br", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strTopLevelFolder, "zh-cn", StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (string.Equals(strTopLevelFolder, "lang", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "en-us.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "de-de.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "fr-fr.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "ja-jp.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "pt-br.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "zh-cn.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "de-de_data.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "fr-fr_data.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "ja-jp_data.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "pt-br_data.xml", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(strFileName, "zh-cn_data.xml", StringComparison.OrdinalIgnoreCase))
                            continue;
                        lstFilesToDelete.Add(strFileToDelete);
                    }

                    await InstallUpdateFromZip(_strTempLatestVersionZipPath, lstFilesToDelete);
                }
            }
        }

        private async Task DoCleanReinstall()
        {
            if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Updater_CleanReinstallPrompt"),
                await LanguageManager.GetStringAsync("MessageTitle_Updater_CleanReinstallPrompt"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempLatestVersionZipPath))
            {
                using (new CursorWait())
                {
                    cmdUpdate.Enabled = false;
                    cmdRestart.Enabled = false;
                    cmdCleanReinstall.Enabled = false;
                    if (!(await CreateBackupZip()))
                        return;

                    string[] astrAllFiles = Directory.GetFiles(_strAppPath, "*", SearchOption.AllDirectories);
                    List<string> lstFilesToDelete = new List<string>(astrAllFiles.Length);
                    foreach (string strFileToDelete in astrAllFiles)
                    {
                        string strFileName = Path.GetFileName(strFileToDelete);
                        if (string.IsNullOrEmpty(strFileName)
                            || strFileName.EndsWith(".old", StringComparison.OrdinalIgnoreCase)
                            || strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                            continue;
                        string strFilePath = Path.GetDirectoryName(strFileToDelete).TrimStartOnce(_strAppPath);
                        if (!strFilePath.StartsWith("customdata", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("data", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("export", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("lang", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("saves", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("settings", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("sheets", StringComparison.OrdinalIgnoreCase)
                            && !strFilePath.StartsWith("Utils", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(strFilePath.TrimEndOnce(strFileName)))
                            continue;
                        lstFilesToDelete.Add(strFileToDelete);
                    }

                    await InstallUpdateFromZip(_strTempLatestVersionZipPath, lstFilesToDelete);
                }
            }
        }

        private async Task InstallUpdateFromZip(string strZipPath, ICollection<string> lstFilesToDelete)
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
                                if (!await Utils.SafeDeleteFileAsync(strLoopPath + ".old", !SilentMode))
                                {
                                    blnDoRestart = false;
                                    break;
                                }

                                File.Move(strLoopPath, strLoopPath + ".old");
                            }

                            entry.ExtractToFile(strLoopPath, false);
                        }
                        catch (IOException)
                        {
                            if (!SilentMode)
                                Program.MainForm.ShowMessageBox(
                                    this,
                                    string.Format(GlobalSettings.CultureInfo,
                                                  await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"),
                                                  Path.GetFileName(strLoopPath)));
                            blnDoRestart = false;
                            break;
                        }
                        catch (NotSupportedException)
                        {
                            if (!SilentMode)
                                Program.MainForm.ShowMessageBox(
                                    this,
                                    string.Format(GlobalSettings.CultureInfo,
                                                  await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"),
                                                  Path.GetFileName(strLoopPath)));
                            blnDoRestart = false;
                            break;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (!SilentMode)
                                Program.MainForm.ShowMessageBox(
                                    this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                            blnDoRestart = false;
                            break;
                        }

                        lstFilesToDelete.Remove(strLoopPath.Replace('/', Path.DirectorySeparatorChar));
                    }
                }
            }
            catch (IOException)
            {
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                                      await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"), strZipPath));
                blnDoRestart = false;
            }
            catch (NotSupportedException)
            {
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                                      await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"), strZipPath));
                blnDoRestart = false;
            }
            catch (UnauthorizedAccessException)
            {
                if (!SilentMode)
                    Program.MainForm.ShowMessageBox(
                        this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                blnDoRestart = false;
            }
            if (blnDoRestart)
            {
                Dictionary<string, Task<bool>> dicTasks = new Dictionary<string, Task<bool>>(lstFilesToDelete.Count);
                foreach (string strFileToDelete in lstFilesToDelete)
                {
                    dicTasks.Add(strFileToDelete, Utils.SafeDeleteFileAsync(strFileToDelete));
                }
                await Task.WhenAll(dicTasks.Values);
                List<string> lstBlocked = new List<string>(lstFilesToDelete.Count);
                foreach (KeyValuePair<string, Task<bool>> kvpTaskPair in dicTasks)
                {
                    if (!kvpTaskPair.Value.Result)
                        lstBlocked.Add(kvpTaskPair.Key);
                }

                if (lstBlocked.Count > 0)
                {
                    Utils.BreakIfDebug();
                    if (!SilentMode)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdOutput))
                        {
                            sbdOutput.Append(await LanguageManager.GetStringAsync("Message_Files_Cannot_Be_Removed"));
                            foreach (string strFile in lstBlocked)
                            {
                                sbdOutput.AppendLine().Append(strFile);
                            }

                            Program.MainForm.ShowMessageBox(this, sbdOutput.ToString(), null, MessageBoxButtons.OK,
                                                            MessageBoxIcon.Information);
                        }
                    }
                }
                await Utils.RestartApplication();
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
                        // Swallow this
                    }
                    catch (NotSupportedException)
                    {
                        // Swallow this
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Swallow this
                    }
                }
            }
        }

        private async Task DownloadUpdates()
        {
            if (Uri.TryCreate(_strDownloadFile, UriKind.Absolute, out Uri uriDownloadFileAddress))
            {
                Log.Debug("DownloadUpdates");
                await Task.WhenAll(cmdUpdate.DoThreadSafeAsync(() => cmdUpdate.Enabled = false),
                                   cmdRestart.DoThreadSafeAsync(() => cmdRestart.Enabled = false),
                                   cmdCleanReinstall.DoThreadSafeAsync(() => cmdCleanReinstall.Enabled = false),
                                   Utils.SafeDeleteFileAsync(_strTempLatestVersionZipPath, !SilentMode));
                try
                {
                    await _clientDownloader.DownloadFileTaskAsync(uriDownloadFileAddress, _strTempLatestVersionZipPath);
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
                    if (!SilentMode)
                        Program.MainForm.ShowMessageBox(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException"),
                                          strException), Application.ProductName, MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    await cmdUpdate.DoThreadSafeAsync(() => cmdUpdate.Enabled = true);
                }
            }
        }

        #region AsyncDownload Events

        /// <summary>
        /// Update the download progress for the file.
        /// </summary>
        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (int.TryParse((e.BytesReceived * 100 / e.TotalBytesToReceive).ToString(GlobalSettings.InvariantCultureInfo), out int intTmp))
                pgbOverallProgress.Value = intTmp;
        }

        /// <summary>
        /// The EXE file is down downloading, so replace the old file with the new one.
        /// </summary>
        private async void wc_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log.Info("wc_DownloadExeFileCompleted enter");
            cmdUpdate.QueueThreadSafe(async () =>
            {
                cmdUpdate.Text = await LanguageManager.GetStringAsync("Button_Redownload");
                cmdUpdate.Enabled = true;
            });
            cmdRestart.QueueThreadSafe(async () =>
            {
                if (_blnIsConnected && cmdRestart.Text != await LanguageManager.GetStringAsync("Button_Up_To_Date"))
                    cmdRestart.Enabled = true;
            });
            cmdCleanReinstall.QueueThreadSafe(() => cmdCleanReinstall.Enabled = true);
            Log.Info("wc_DownloadExeFileCompleted exit");
            if (SilentMode)
            {
                if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Update_CloseForms"),
                        await LanguageManager.GetStringAsync("Title_Update"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.Yes)
                {
                    await DoUpdate();
                }
                else
                {
                    _blnSilentModeUpdateWasDenied = true; // only actually go through with the attempt in Silent Mode once, don't prompt user any more if they canceled out once already
                    _blnIsConnected = false;
                    this.QueueThreadSafe(Close);
                }
            }
        }

        #endregion AsyncDownload Events
    }
}
