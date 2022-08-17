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
        private Task _tskChangelogDownloader;
        private CancellationTokenSource _objChangelogDownloaderCancellationTokenSource;
        private CancellationTokenSource _objConnectionLoaderCancellationTokenSource;
        private CancellationTokenSource _objUpdatesDownloaderCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;
        private readonly WebClient _clientDownloader;
        private readonly WebClient _clientChangelogDownloader;
        private string _strExceptionString;

        public ChummerUpdater()
        {
            Log.Info("ChummerUpdater");
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
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
                try
                {
                    Log.Info("More than one instance, exiting");
                    if (!SilentMode)
                        Program.ShowMessageBox(
                            this,
                            await LanguageManager.GetStringAsync("Message_Update_MultipleInstances",
                                                                 token: _objGenericToken),
                            await LanguageManager.GetStringAsync("Title_Update", token: _objGenericToken),
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Log.Info("ChummerUpdater_Load exit");
                    await this.DoThreadSafeAsync(x => x.Close(), _objGenericToken);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                    return;
                }
            }

            CancellationTokenSource objNewChangelogSource = new CancellationTokenSource();
            CancellationTokenSource objTemp
                = Interlocked.Exchange(ref _objChangelogDownloaderCancellationTokenSource, objNewChangelogSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            try
            {
                if (_tskChangelogDownloader?.IsCompleted == false)
                    await _tskChangelogDownloader;
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            if (_tskConnectionLoader == null || (_tskConnectionLoader.IsCompleted && (_tskConnectionLoader.IsCanceled ||
                _tskConnectionLoader.IsFaulted)))
            {
                CancellationToken objToken = objNewChangelogSource.Token;
                _tskChangelogDownloader = Task.Run(() => DownloadChangelog(objToken), objToken);
                try
                {
                    await _tskChangelogDownloader;
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }
            else
            {
                Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                            objNewChangelogSource);
                objNewChangelogSource.Dispose();
            }
            if (_blnIsConnected && SilentMode && !_blnSilentModeUpdateWasDenied)
            {
                CancellationTokenSource objNewUpdatesSource = new CancellationTokenSource();
                objTemp = Interlocked.Exchange(ref _objUpdatesDownloaderCancellationTokenSource, objNewUpdatesSource);
                if (objTemp?.IsCancellationRequested == false)
                {
                    objTemp.Cancel(false);
                    objTemp.Dispose();
                }
                try
                {
                    await DownloadUpdates(objNewUpdatesSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }
            Log.Info("ChummerUpdater_Load exit");
        }

        private bool _blnFormClosing;

        private async void ChummerUpdater_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If we have automatic updates on, make sure we don't close the updater, just hide it
            if (e.CloseReason == CloseReason.UserClosing && GlobalSettings.AutomaticUpdate)
            {
                e.Cancel = true;
                await this.DoThreadSafeAsync(x => x.Hide(), _objGenericToken);
                SilentMode = true;
                return;
            }
            _blnFormClosing = true;
            CancellationTokenSource objTemp
                = Interlocked.Exchange(ref _objConnectionLoaderCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            objTemp = Interlocked.Exchange(ref _objChangelogDownloaderCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            objTemp = Interlocked.Exchange(ref _objUpdatesDownloaderCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            _clientDownloader.CancelAsync();
            _clientChangelogDownloader.CancelAsync();
            try
            {
                await _tskChangelogDownloader;
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            try
            {
                await _tskConnectionLoader;
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            _objGenericFormClosingCancellationTokenSource?.Cancel(false);
        }

        private async Task DownloadChangelog(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationTokenSource objTemp
                = Interlocked.Exchange(ref _objConnectionLoaderCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                Interlocked.CompareExchange(ref _objConnectionLoaderCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }
            try
            {
                if (_tskConnectionLoader?.IsCompleted == false)
                    await _tskConnectionLoader;
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            catch
            {
                Interlocked.CompareExchange(ref _objConnectionLoaderCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }
            CancellationToken objToken = objNewSource.Token;
            _tskConnectionLoader = Task.Run(async () =>
            {
                await LoadConnection(objToken);
                await PopulateChangelog(objToken);
            }, objToken);
        }

        private async ValueTask PopulateChangelog(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!_blnFormClosing)
            {
                if (!_clientDownloader.IsBusy)
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = true, token);
                token.ThrowIfCancellationRequested();
                if (File.Exists(_strTempLatestVersionChangelogPath))
                {
                    string strUpdateLog = File.ReadAllText(_strTempLatestVersionChangelogPath).CleanForHtml();
                    token.ThrowIfCancellationRequested();
                    await webNotes.DoThreadSafeAsync(x => x.DocumentText
                                                         = "<font size=\"-1\" face=\"Courier New,Serif\">"
                                                           + strUpdateLog + "</font>", token);
                }
                token.ThrowIfCancellationRequested();
                await DoVersionTextUpdate(token);
            }
        }

        private async ValueTask LoadConnection(CancellationToken token = default)
        {
            while (_clientChangelogDownloader.IsBusy)
            {
                token.ThrowIfCancellationRequested();
                await Utils.SafeSleepAsync(token);
            }
            _blnIsConnected = false;
            token.ThrowIfCancellationRequested();
            bool blnChummerVersionGotten = true;
            string strError = (await LanguageManager.GetStringAsync("String_Error", token: token)).Trim();
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

                    token.ThrowIfCancellationRequested();

                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response?.GetResponseStream())
                    {
                        if (dataStream == null)
                            blnChummerVersionGotten = false;
                        if (blnChummerVersionGotten)
                        {
                            token.ThrowIfCancellationRequested();

                            // Open the stream using a StreamReader for easy access.
                            string responseFromServer;
                            using (StreamReader reader = new StreamReader(dataStream, Encoding.UTF8, true))
                                responseFromServer = await reader.ReadToEndAsync();

                            token.ThrowIfCancellationRequested();

                            bool blnFoundTag = false;
                            bool blnFoundArchive = false;
                            foreach (string line in responseFromServer.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                token.ThrowIfCancellationRequested();

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
                    Program.ShowMessageBox(this,
                    string.IsNullOrEmpty(_strExceptionString)
                        ? await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnect", token: token)
                        : string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException", token: token), _strExceptionString),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(_strTempLatestVersionChangelogPath))
            {
                if (!await Utils.SafeDeleteFileAsync(_strTempLatestVersionChangelogPath + ".old", !SilentMode, token: token))
                    return;
                File.Move(_strTempLatestVersionChangelogPath, _strTempLatestVersionChangelogPath + ".old");
            }
            string strUrl = "https://raw.githubusercontent.com/chummer5a/chummer5a/" + LatestVersion + "/Chummer/changelog.txt";
            try
            {
                Uri uriConnectionAddress = new Uri(strUrl);
                if (!await Utils.SafeDeleteFileAsync(_strTempLatestVersionChangelogPath + ".tmp", !SilentMode, token: token))
                    return;
                await _clientChangelogDownloader.DownloadFileTaskAsync(uriConnectionAddress, _strTempLatestVersionChangelogPath + ".tmp");
                token.ThrowIfCancellationRequested();
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
                    Program.ShowMessageBox(this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException", token: token), strException),
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
                    Program.ShowMessageBox(this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Warning_Update_CouldNotConnectException", token: token), strException),
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
                if (value)
                {
                    CancellationTokenSource objNewSource = new CancellationTokenSource();
                    CancellationTokenSource objTemp
                        = Interlocked.Exchange(ref _objChangelogDownloaderCancellationTokenSource, objNewSource);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }
                    if ((_tskConnectionLoader == null || (_tskConnectionLoader.IsCompleted && (_tskConnectionLoader.IsCanceled ||
                                                                                               _tskConnectionLoader.IsFaulted))) && _tskChangelogDownloader?.IsCompleted != false)
                    {
                        CancellationToken objToken = objNewSource.Token;
                        _tskChangelogDownloader = Task.Run(() => DownloadChangelog(objToken), objToken);
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                                    objNewSource);
                        objNewSource.Dispose();
                    }
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

        public async ValueTask DoVersionTextUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strLatestVersion = LatestVersion.TrimStartOnce("Nightly-v");
            await lblUpdaterStatus.DoThreadSafeAsync(x => x.Left = x.Left + x.Width + 6, token);
            if (!_blnIsConnected || strLatestVersion == (await LanguageManager.GetStringAsync("String_Error", token: token)).Trim())
            {
                token.ThrowIfCancellationRequested();
                string strText = string.IsNullOrEmpty(_strExceptionString)
                    ? await LanguageManager.GetStringAsync(
                        "Warning_Update_CouldNotConnect", token: token)
                    : string.Format(
                        GlobalSettings.CultureInfo,
                        (await LanguageManager.GetStringAsync(
                            "Warning_Update_CouldNotConnectException", token: token))
                        .NormalizeWhiteSpace(),
                        _strExceptionString);
                await Task.WhenAll(lblUpdaterStatus.DoThreadSafeAsync(x => x.Text = strText, token),
                                   LanguageManager.GetStringAsync("Button_Reconnect", token: token)
                                                  .ContinueWith(
                                                      y => cmdUpdate.DoThreadSafeFuncAsync(
                                                          x => x.Text = y.Result, token), token).Unwrap(),
                                   cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token));
                return;
            }

            int intResult = 0;
            if (VersionExtensions.TryParse(strLatestVersion, out Version objLatestVersion))
                intResult = objLatestVersion?.CompareTo(Utils.CurrentChummerVersion) ?? 0;
            token.ThrowIfCancellationRequested();
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
            string strStatusText;
            if (intResult > 0)
            {
                strStatusText = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Update_Available", token: token), strLatestVersion) + strSpace +
                                string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Currently_Installed_Version", token: token), CurrentVersion);
            }
            else
            {
                strStatusText = await LanguageManager.GetStringAsync("String_Up_To_Date", token: token) + strSpace +
                                string.Format(GlobalSettings.CultureInfo,
                                              await LanguageManager.GetStringAsync("String_Currently_Installed_Version", token: token),
                                              CurrentVersion) + strSpace + string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("String_Latest_Version", token: token),
                                    await LanguageManager.GetStringAsync(_blnPreferNightly
                                                                             ? "String_Nightly"
                                                                             : "String_Stable", token: token), strLatestVersion);
                if (intResult < 0)
                {
                    token.ThrowIfCancellationRequested();
                    string strText = await LanguageManager.GetStringAsync("Button_Up_To_Date", token: token);
                    await cmdRestart.DoThreadSafeAsync(x =>
                    {
                        x.Text = strText;
                        x.Enabled = false;
                    }, token);
                }
            }
            if (_blnPreferNightly)
                strStatusText += strSpace + await LanguageManager.GetStringAsync("String_Nightly_Changelog_Warning", token: token);
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lblUpdaterStatus.DoThreadSafeAsync(x => x.Text = strStatusText, token),
                               LanguageManager.GetStringAsync(intResult > 0 ? "Button_Download" : "Button_Redownload", token: token)
                                              .ContinueWith(
                                                  y => cmdUpdate.DoThreadSafeFuncAsync(x => x.Text = y.Result, token),
                                                  token).Unwrap());
            token.ThrowIfCancellationRequested();
        }

        private async void cmdUpdate_Click(object sender, EventArgs e)
        {
            Log.Info("cmdUpdate_Click");
            CancellationTokenSource objNewChangelogSource = new CancellationTokenSource();
            CancellationTokenSource objTemp
                = Interlocked.Exchange(ref _objChangelogDownloaderCancellationTokenSource, objNewChangelogSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            try
            {
                if (_tskChangelogDownloader?.IsCompleted == false)
                    await _tskChangelogDownloader;
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            if (_blnIsConnected)
            {
                Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                            objNewChangelogSource);
                objNewChangelogSource.Dispose();
                CancellationTokenSource objNewUpdatesSource = new CancellationTokenSource();
                objTemp = Interlocked.Exchange(ref _objUpdatesDownloaderCancellationTokenSource, objNewUpdatesSource);
                if (objTemp?.IsCancellationRequested == false)
                {
                    objTemp.Cancel(false);
                    objTemp.Dispose();
                }
                try
                {
                    await DownloadUpdates(objNewUpdatesSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }
            else if (_tskConnectionLoader == null || (_tskConnectionLoader.IsCompleted
                                                      && (_tskConnectionLoader.IsCanceled ||
                                                          _tskConnectionLoader.IsFaulted)))
            {
                CancellationToken objToken = objNewChangelogSource.Token;
                await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, objToken);
                _tskChangelogDownloader = Task.Run(() => DownloadChangelog(objToken), objToken);
                try
                {
                    await _tskChangelogDownloader;
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }
            else
            {
                Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                            objNewChangelogSource);
                objNewChangelogSource.Dispose();
            }
        }

        private async void cmdRestart_Click(object sender, EventArgs e)
        {
            Log.Info("cmdRestart_Click");
            try
            {
                await DoUpdate(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdCleanReinstall_Click(object sender, EventArgs e)
        {
            Log.Info("cmdCleanReinstall_Click");
            try
            {
                await DoCleanReinstall(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async ValueTask<bool> CreateBackupZip(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Create a backup file in the temp directory.
            string strBackupZipPath = Path.Combine(Utils.GetTempPath(), "chummer" + CurrentVersion + ".zip");
            Log.Info("Creating archive from application path: " + _strAppPath);
            try
            {
                if (!File.Exists(strBackupZipPath))
                {
                    try
                    {
                        using (var zipToOpen = new FileStream(strBackupZipPath, FileMode.Create))
                        using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                        {
                            foreach (var file in Directory.GetFiles(_strAppPath))
                            {
                                var entry = archive.CreateEntry(Path.GetFileName(file));
                                entry.LastWriteTime = File.GetLastWriteTime(file);
                                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read,
                                           FileShare.ReadWrite))
                                using (var stream = entry.Open())
                                {
                                    //magic number default buffer size, no overload that is only stream+token
                                    await fs.CopyToAsync(stream, 81920, token);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // ReSharper disable once MethodSupportsCancellation
                        await Utils.SafeDeleteDirectoryAsync(strBackupZipPath, token: CancellationToken.None);
                        throw;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                token.ThrowIfCancellationRequested();
                if (!SilentMode)
                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning", token: token));
                return false;
            }
            catch (IOException)
            {
                token.ThrowIfCancellationRequested();
                if (!SilentMode)
                    Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token), Path.GetFileName(strBackupZipPath)));
                return false;
            }
            catch (NotSupportedException)
            {
                token.ThrowIfCancellationRequested();
                if (!SilentMode)
                    Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token), Path.GetFileName(strBackupZipPath)));
                return false;
            }
            return true;
        }

        private async ValueTask DoUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempLatestVersionZipPath))
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
                try
                {
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, token);
                    await cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token);
                    await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token);
                    if (!(await CreateBackupZip(token)))
                        return;

                    List<string> lstFilesToDelete = new List<string>(byte.MaxValue);
                    foreach (string strFileToDelete in Directory.EnumerateFiles(
                                 _strAppPath, "*", SearchOption.AllDirectories))
                    {
                        token.ThrowIfCancellationRequested();
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

                    await InstallUpdateFromZip(_strTempLatestVersionZipPath, lstFilesToDelete, token);
                }
                finally
                {
                    await objCursorWait.DisposeAsync();
                }
            }
        }

        private async ValueTask DoCleanReinstall(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Updater_CleanReinstallPrompt", token: token),
                                       await LanguageManager.GetStringAsync("MessageTitle_Updater_CleanReinstallPrompt", token: token), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempLatestVersionZipPath))
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
                try
                {
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, token);
                    await cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token);
                    await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token);
                    if (!await CreateBackupZip(token))
                        return;

                    List<string> lstFilesToDelete = new List<string>(byte.MaxValue);
                    foreach (string strFileToDelete in Directory.EnumerateFiles(
                                 _strAppPath, "*", SearchOption.AllDirectories))
                    {
                        token.ThrowIfCancellationRequested();
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

                    await InstallUpdateFromZip(_strTempLatestVersionZipPath, lstFilesToDelete, token);
                }
                finally
                {
                    await objCursorWait.DisposeAsync();
                }
            }
        }

        private async ValueTask InstallUpdateFromZip(string strZipPath, ICollection<string> lstFilesToDelete, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnDoRestart = true;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                // Copy over the archive from the temp directory.
                Log.Info("Extracting downloaded archive into application path: " + strZipPath);
                try
                {
                    using (ZipArchive archive
                           = ZipFile.Open(strZipPath, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            token.ThrowIfCancellationRequested();
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
                                    if (!await Utils.SafeDeleteFileAsync(
                                            strLoopPath + ".old", !SilentMode, token: token))
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
                                    Program.ShowMessageBox(
                                        this,
                                        string.Format(GlobalSettings.CultureInfo,
                                                      await LanguageManager.GetStringAsync(
                                                          "Message_File_Cannot_Be_Accessed", token: token),
                                                      Path.GetFileName(strLoopPath)));
                                blnDoRestart = false;
                                break;
                            }
                            catch (NotSupportedException)
                            {
                                if (!SilentMode)
                                    Program.ShowMessageBox(
                                        this,
                                        string.Format(GlobalSettings.CultureInfo,
                                                      await LanguageManager.GetStringAsync(
                                                          "Message_File_Cannot_Be_Accessed", token: token),
                                                      Path.GetFileName(strLoopPath)));
                                blnDoRestart = false;
                                break;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                if (!SilentMode)
                                    Program.ShowMessageBox(
                                        this,
                                        await LanguageManager.GetStringAsync(
                                            "Message_Insufficient_Permissions_Warning", token: token));
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
                        Program.ShowMessageBox(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token),
                                          strZipPath));
                    blnDoRestart = false;
                }
                catch (NotSupportedException)
                {
                    if (!SilentMode)
                        Program.ShowMessageBox(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token),
                                          strZipPath));
                    blnDoRestart = false;
                }
                catch (UnauthorizedAccessException)
                {
                    if (!SilentMode)
                        Program.ShowMessageBox(
                            this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning", token: token));
                    blnDoRestart = false;
                }

                if (blnDoRestart)
                {
                    List<string> lstBlocked = new List<string>(lstFilesToDelete.Count);
                    Dictionary<string, Task<bool>> dicTasks
                        = new Dictionary<string, Task<bool>>(Utils.MaxParallelBatchSize);
                    int intCounter = 0;
                    foreach (string strFileToDelete in lstFilesToDelete)
                    {
                        dicTasks.Add(strFileToDelete, Utils.SafeDeleteFileAsync(strFileToDelete, token: token));
                        if (++intCounter != Utils.MaxParallelBatchSize)
                            continue;
                        await Task.WhenAll(dicTasks.Values);
                        foreach (KeyValuePair<string, Task<bool>> kvpTaskPair in dicTasks)
                        {
                            if (!await kvpTaskPair.Value)
                                lstBlocked.Add(kvpTaskPair.Key);
                        }

                        dicTasks.Clear();
                        intCounter = 0;
                    }

                    await Task.WhenAll(dicTasks.Values);
                    foreach (KeyValuePair<string, Task<bool>> kvpTaskPair in dicTasks)
                    {
                        if (!await kvpTaskPair.Value)
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
                                sbdOutput.Append(
                                    await LanguageManager.GetStringAsync("Message_Files_Cannot_Be_Removed", token: token));
                                foreach (string strFile in lstBlocked)
                                {
                                    sbdOutput.AppendLine().Append(strFile);
                                }

                                Program.ShowMessageBox(this, sbdOutput.ToString(), null, MessageBoxButtons.OK,
                                                       MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string strBackupFileName in Directory.GetFiles(
                                 _strAppPath, "*.old", SearchOption.AllDirectories))
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
            finally
            {
                await objCursorWait.DisposeAsync();
            }

            if (blnDoRestart)
                await Utils.RestartApplication(token: token);
        }

        private async ValueTask DownloadUpdates(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Uri.TryCreate(_strDownloadFile, UriKind.Absolute, out Uri uriDownloadFileAddress))
            {
                Log.Debug("DownloadUpdates");
                token.ThrowIfCancellationRequested();
                await Task.WhenAll(cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   Utils.SafeDeleteFileAsync(_strTempLatestVersionZipPath, !SilentMode, token: token));
                token.ThrowIfCancellationRequested();
                try
                {
                    using (token.Register(() => _clientDownloader.CancelAsync()))
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
                    token.ThrowIfCancellationRequested();
                    // Show the warning even if we're in silent mode, because the user should still know that the update check could not be performed
                    if (!SilentMode)
                        Program.ShowMessageBox(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager.GetStringAsync(
                                              "Warning_Update_CouldNotConnectException", token: token),
                                          strException), Application.ProductName, MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = true, token);
                }
            }
        }

        #region AsyncDownload Events

        /// <summary>
        /// Update the download progress for the file.
        /// </summary>
        private async void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                if (int.TryParse(
                        (e.BytesReceived * 100 / e.TotalBytesToReceive).ToString(GlobalSettings.InvariantCultureInfo),
                        out int intTmp))

                    await pgbOverallProgress.DoThreadSafeAsync(x => x.Value = intTmp, _objGenericToken);
            }
            catch (TaskCanceledException)
            {
                //Swallow this. Closing the form cancels the download which is a change of download progress so token is already cancelled. 
            }
        }

        /// <summary>
        /// The EXE file is down downloading, so replace the old file with the new one.
        /// </summary>
        private async void wc_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log.Info("wc_DownloadExeFileCompleted enter");
            try
            {
                string strText1 = await LanguageManager.GetStringAsync("Button_Redownload", token: _objGenericToken);
                string strText2 = await LanguageManager.GetStringAsync("Button_Up_To_Date", token: _objGenericToken);
                await Task.WhenAll(cmdUpdate.DoThreadSafeAsync(x =>
                                   {
                                       x.Text = strText1;
                                       x.Enabled = true;
                                   }, _objGenericToken),
                                   cmdRestart.DoThreadSafeAsync(x =>
                                   {
                                       if (_blnIsConnected && x.Text != strText2)
                                           x.Enabled = true;
                                   }, _objGenericToken),
                                   cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken));
                Log.Info("wc_DownloadExeFileCompleted exit");
                if (SilentMode)
                {
                    if (Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Update_CloseForms", token: _objGenericToken),
                                               await LanguageManager.GetStringAsync(
                                                   "Title_Update", token: _objGenericToken), MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Question) ==
                        DialogResult.Yes)
                    {
                        await DoUpdate(_objGenericToken);
                    }
                    else
                    {
                        _blnSilentModeUpdateWasDenied
                            = true; // only actually go through with the attempt in Silent Mode once, don't prompt user any more if they canceled out once already
                        _blnIsConnected = false;
                        await this.DoThreadSafeAsync(x => x.Close(), _objGenericToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        #endregion AsyncDownload Events
    }
}
