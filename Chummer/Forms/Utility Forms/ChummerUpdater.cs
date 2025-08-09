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
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private int _intSilentMode;
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
            Disposed += (sender, args) =>
            {
                _objGenericFormClosingCancellationTokenSource.Dispose();
                _clientChangelogDownloader.Dispose();
                _clientDownloader.Dispose();
                _objConnectionLoaderCancellationTokenSource?.Dispose();
            };
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
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            this,
                            await LanguageManager.GetStringAsync("Message_Update_MultipleInstances",
                                token: _objGenericToken).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("Title_Update", token: _objGenericToken)
                                .ConfigureAwait(false),
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation, token: _objGenericToken).ConfigureAwait(false);
                    }

                    Log.Info("ChummerUpdater_Load exit");
                    await this.DoThreadSafeAsync(x => x.Close(), _objGenericToken).ConfigureAwait(false);
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
            Task tskOld = Interlocked.Exchange(ref _tskChangelogDownloader, null);
            try
            {
                if (tskOld?.IsCompleted == false)
                    await tskOld.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            Task tskConnectionLoader = _tskConnectionLoader;
            if (tskConnectionLoader == null || (tskConnectionLoader.IsCompleted && (tskConnectionLoader.IsCanceled ||
                tskConnectionLoader.IsFaulted)))
            {
                CancellationToken objToken = objNewChangelogSource.Token;
                Task tskNew = Task.Run(() => DownloadChangelog(objToken), objToken);
                if (Interlocked.CompareExchange(ref _tskChangelogDownloader, tskNew, null) != null)
                {
                    Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                            objNewChangelogSource);
                    try
                    {
                        objNewChangelogSource.Cancel(false);
                    }
                    finally
                    {
                        objNewChangelogSource.Dispose();
                    }
                }
                try
                {
                    await tskNew.ConfigureAwait(false);
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
                    await DownloadUpdates(objNewUpdatesSource.Token).ConfigureAwait(false);
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
                try
                {
                    if (sender is Control ctlSender)
                        await ctlSender.DoThreadSafeAsync(x => x.Hide(), _objGenericToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
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
            Task tskOld = Interlocked.Exchange(ref _tskChangelogDownloader, null);
            if (tskOld != null)
            {
                try
                {
                    await tskOld.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
            tskOld = Interlocked.Exchange(ref _tskConnectionLoader, null);
            if (tskOld != null)
            {
                try
                {
                    await tskOld.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
            _objGenericFormClosingCancellationTokenSource?.Cancel(false);
        }

        private async Task DownloadChangelog(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationToken objToken = objNewSource.Token;
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

            Task tskOld = Interlocked.Exchange(ref _tskConnectionLoader, null);
            try
            {
                if (tskOld?.IsCompleted == false)
                    await tskOld.ConfigureAwait(false);
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
            Task tskNew = Task.Run(async () =>
            {
                await LoadConnection(objToken).ConfigureAwait(false);
                await PopulateChangelog(objToken).ConfigureAwait(false);
            }, objToken);
            if (Interlocked.CompareExchange(ref _tskConnectionLoader, tskNew, null) != null)
            {
                Interlocked.CompareExchange(ref _objConnectionLoaderCancellationTokenSource, null, objNewSource);
                try
                {
                    objNewSource.Cancel(false);
                }
                finally
                {
                    objNewSource.Dispose();
                }
                try
                {
                    await tskNew.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private async Task PopulateChangelog(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!_blnFormClosing)
            {
                if (!_clientDownloader.IsBusy)
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (File.Exists(_strTempLatestVersionChangelogPath))
                {
                    string strDocumentText = "<font size=\"-1\" face=\"Courier New,Serif\">"
                                             + (await FileExtensions
                                                      .ReadAllTextAsync(_strTempLatestVersionChangelogPath, token)
                                                      .ConfigureAwait(false)).CleanForHtml()
                                             + "</font>";
                    token.ThrowIfCancellationRequested();
                    await webNotes.DoThreadSafeAsync(x => x.DocumentText = strDocumentText, token)
                                  .ConfigureAwait(false);
                }
                token.ThrowIfCancellationRequested();
                await DoVersionTextUpdate(token).ConfigureAwait(false);
            }
        }

        private async Task LoadConnection(CancellationToken token = default)
        {
            while (_clientChangelogDownloader.IsBusy)
            {
                token.ThrowIfCancellationRequested();
                await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            }
            _blnIsConnected = false;
            token.ThrowIfCancellationRequested();
            bool blnChummerVersionGotten = true;
            string strError = (await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false)).Trim();
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
                    response = await request.GetResponseAsync().ConfigureAwait(false) as HttpWebResponse;

                    token.ThrowIfCancellationRequested();

                    // Get the stream containing content returned by the server.
                    await using (Stream dataStream = response?.GetResponseStream())
                    {
                        if (dataStream == null)
                            blnChummerVersionGotten = false;
                        if (blnChummerVersionGotten)
                        {
                            token.ThrowIfCancellationRequested();

                            // Open the stream using a StreamReader for easy access.
                            string responseFromServer;
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                            {
                                token.ThrowIfCancellationRequested();
                                using (StreamReader objReader = new StreamReader(dataStream, Encoding.UTF8, true))
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (string strLine = await objReader.ReadLineAsync(token).ConfigureAwait(false);
                                         strLine != null;
                                         strLine = await objReader.ReadLineAsync(token).ConfigureAwait(false))
                                    {
                                        token.ThrowIfCancellationRequested();
                                        if (!string.IsNullOrEmpty(strLine))
                                            sbdReturn.AppendLine(strLine);
                                    }
                                }

                                responseFromServer = sbdReturn.ToString();
                            }

                            token.ThrowIfCancellationRequested();

                            bool blnFoundTag = false;
                            bool blnFoundArchive = false;
                            foreach (string strLine in responseFromServer.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                token.ThrowIfCancellationRequested();

                                if (!blnFoundTag && strLine.Contains("tag_name"))
                                {
                                    LatestVersion = (_strLatestVersion = strLine.SplitNoAlloc(':').ElementAtOrDefault(1))
                                        .SplitNoAlloc('}').FirstOrDefault().FastEscape('\"').Trim();
                                    blnFoundTag = true;
                                    if (blnFoundArchive)
                                        break;
                                }

                                if (!blnFoundArchive && strLine.Contains("browser_download_url"))
                                {
                                    _strDownloadFile = "https://" +
                                                       (strLine.SplitNoAlloc(':').ElementAtOrDefault(2) ?? string.Empty)
                                                       .Substring(2).SplitNoAlloc('}').FirstOrDefault()
                                                       .FastEscape('\"');
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
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.IsNullOrEmpty(_strExceptionString)
                            ? await LanguageManager
                                .GetStringAsync(
                                    "Warning_Update_CouldNotConnect", token: token)
                                .ConfigureAwait(false)
                            : string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager
                                    .GetStringAsync(
                                        "Warning_Update_CouldNotConnectException",
                                        token: token).ConfigureAwait(false),
                                _strExceptionString),
                        Application.ProductName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                }

                return;
            }

            if (File.Exists(_strTempLatestVersionChangelogPath))
            {
                if (!await FileExtensions.SafeDeleteAsync(_strTempLatestVersionChangelogPath + ".old", !SilentMode, token: token)
                                         .ConfigureAwait(false))
                    return;
                File.Move(_strTempLatestVersionChangelogPath, _strTempLatestVersionChangelogPath + ".old");
            }

            string strUrl = "https://raw.githubusercontent.com/chummer5a/chummer5a/" + LatestVersion + "/Chummer/changelog.txt";
            try
            {
                Uri uriConnectionAddress = new Uri(strUrl);
                if (!await FileExtensions.SafeDeleteAsync(_strTempLatestVersionChangelogPath + ".tmp", !SilentMode, token: token)
                                         .ConfigureAwait(false))
                    return;
                await _clientChangelogDownloader
                      .DownloadFileTaskAsync(uriConnectionAddress, _strTempLatestVersionChangelogPath + ".tmp")
                      .ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                File.Move(_strTempLatestVersionChangelogPath + ".tmp", _strTempLatestVersionChangelogPath);
            }
            catch (WebException ex)
            {
                if (File.Exists(_strTempLatestVersionChangelogPath + ".old"))
                    File.Move(_strTempLatestVersionChangelogPath + ".old", _strTempLatestVersionChangelogPath);
                string strException = ex.ToString();
                int intNewLineLocation = strException.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (intNewLineLocation == -1)
                    intNewLineLocation = strException.IndexOf('\n');
                if (intNewLineLocation != -1)
                    strException = strException.Substring(0, intNewLineLocation);
                _strExceptionString = strException;
                if (!SilentMode)
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager
                                .GetStringAsync(
                                    "Warning_Update_CouldNotConnectException",
                                    token: token).ConfigureAwait(false),
                            strException),
                        Application.ProductName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                }

                return;
            }
            catch (UriFormatException ex)
            {
                if (File.Exists(_strTempLatestVersionChangelogPath + ".old"))
                    File.Move(_strTempLatestVersionChangelogPath + ".old", _strTempLatestVersionChangelogPath);
                string strException = ex.ToString();
                int intNewLineLocation = strException.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (intNewLineLocation == -1)
                    intNewLineLocation = strException.IndexOf('\n');
                if (intNewLineLocation != -1)
                    strException = strException.Substring(0, intNewLineLocation);
                _strExceptionString = strException;
                if (!SilentMode)
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager
                                .GetStringAsync(
                                    "Warning_Update_CouldNotConnectException",
                                    token: token).ConfigureAwait(false),
                            strException),
                        Application.ProductName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                }

                return;
            }
            catch
            {
                if (File.Exists(_strTempLatestVersionChangelogPath + ".old"))
                    File.Move(_strTempLatestVersionChangelogPath + ".old", _strTempLatestVersionChangelogPath);
                throw;
            }

            _blnIsConnected = true;
        }

        /// <summary>
        /// When running in silent mode, the update window will not be shown.
        /// </summary>
        public bool SilentMode
        {
            get => _intSilentMode > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intSilentMode, intNewValue) == intNewValue)
                    return;
                if (value)
                {
                    CancellationTokenSource objNewSource = new CancellationTokenSource();
                    CancellationToken objToken = objNewSource.Token;
                    CancellationTokenSource objTemp
                        = Interlocked.Exchange(ref _objChangelogDownloaderCancellationTokenSource, objNewSource);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }
                    Task tskConnectionLoader = _tskConnectionLoader;
                    if (tskConnectionLoader == null || (tskConnectionLoader.IsCompleted && (tskConnectionLoader.IsCanceled || tskConnectionLoader.IsFaulted)))
                    {
                        Task tskOld = Interlocked.Exchange(ref _tskChangelogDownloader, null);
                        if (tskOld?.IsCompleted == false)
                        {
                            Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                                    objNewSource);
                            objNewSource.Dispose();
                            return;
                        }
                        Task tskNew = Task.Run(() => DownloadChangelog(objToken), objToken);
                        if (Interlocked.CompareExchange(ref _tskChangelogDownloader, tskNew, null) != null)
                        {
                            Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                                    objNewSource);
                            try
                            {
                                objNewSource.Cancel(false);
                            }
                            finally
                            {
                                objNewSource.Dispose();
                            }
                            try
                            {
                                Utils.SafelyRunSynchronously(() => tskNew);
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        }
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
                if (Interlocked.Exchange(ref _strLatestVersion, value) == value)
                    return;
                _strTempLatestVersionZipPath
                    = Path.Combine(Utils.GetTempPath(), "chummer" + value + ".zip");
            }
        }

        /// <summary>
        /// Latest release build number located on Github.
        /// </summary>
        public string CurrentVersion { get; }

        public async Task DoVersionTextUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strLatestVersion = LatestVersion.TrimStartOnce("Nightly-v");
            await lblUpdaterStatus.DoThreadSafeAsync(x => x.Left = x.Left + x.Width + 6, token).ConfigureAwait(false);
            if (!_blnIsConnected || strLatestVersion == (await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false)).Trim())
            {
                token.ThrowIfCancellationRequested();
                string strText = string.IsNullOrEmpty(_strExceptionString)
                    ? await LanguageManager.GetStringAsync(
                        "Warning_Update_CouldNotConnect", token: token).ConfigureAwait(false)
                    : string.Format(
                        GlobalSettings.CultureInfo,
                        (await LanguageManager.GetStringAsync(
                            "Warning_Update_CouldNotConnectException", token: token).ConfigureAwait(false))
                        .NormalizeWhiteSpace(),
                        _strExceptionString);
                string strReconnect = await LanguageManager.GetStringAsync("Button_Reconnect", token: token).ConfigureAwait(false);
                await lblUpdaterStatus.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                await cmdUpdate.DoThreadSafeFuncAsync(x => x.Text = strReconnect, token).ConfigureAwait(false);
                await cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                return;
            }

            int intResult = 0;
            if (ValueVersion.TryParse(strLatestVersion, out ValueVersion objLatestVersion))
                intResult = objLatestVersion.CompareTo(Utils.CurrentChummerVersion);
            token.ThrowIfCancellationRequested();
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strStatusText;
            if (intResult > 0)
            {
                strStatusText = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Update_Available", token: token).ConfigureAwait(false), strLatestVersion) + strSpace +
                                string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Currently_Installed_Version", token: token).ConfigureAwait(false), CurrentVersion);
            }
            else
            {
                strStatusText = await LanguageManager.GetStringAsync("String_Up_To_Date", token: token).ConfigureAwait(false) + strSpace +
                                string.Format(GlobalSettings.CultureInfo,
                                              await LanguageManager.GetStringAsync("String_Currently_Installed_Version", token: token).ConfigureAwait(false),
                                              CurrentVersion) + strSpace + string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("String_Latest_Version", token: token).ConfigureAwait(false),
                                    await LanguageManager.GetStringAsync(_blnPreferNightly
                                                                             ? "String_Nightly"
                                                                             : "String_Stable", token: token).ConfigureAwait(false), strLatestVersion);
                if (intResult < 0)
                {
                    token.ThrowIfCancellationRequested();
                    string strText = await LanguageManager.GetStringAsync("Button_Up_To_Date", token: token).ConfigureAwait(false);
                    await cmdRestart.DoThreadSafeAsync(x =>
                    {
                        x.Text = strText;
                        x.Enabled = false;
                    }, token).ConfigureAwait(false);
                }
            }
            if (_blnPreferNightly)
                strStatusText += strSpace + await LanguageManager.GetStringAsync("String_Nightly_Changelog_Warning", token: token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            string strUpdateString
                = await LanguageManager.GetStringAsync(intResult > 0 ? "Button_Download" : "Button_Redownload", token: token).ConfigureAwait(false);
            await lblUpdaterStatus.DoThreadSafeAsync(x => x.Text = strStatusText, token).ConfigureAwait(false);
            await cmdUpdate.DoThreadSafeFuncAsync(x => x.Text = strUpdateString, token).ConfigureAwait(false);
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
            Task tskOld = Interlocked.Exchange(ref _tskChangelogDownloader, null);
            try
            {
                if (tskOld?.IsCompleted == false)
                    await tskOld.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
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
                    await DownloadUpdates(objNewUpdatesSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
            else
            {
                Task tskConnectionLoader = _tskConnectionLoader;
                if (tskConnectionLoader == null || (tskConnectionLoader.IsCompleted
                                                      && (tskConnectionLoader.IsCanceled ||
                                                          tskConnectionLoader.IsFaulted)))
                {
                    CancellationToken objToken = objNewChangelogSource.Token;
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, objToken).ConfigureAwait(false);
                    Task tskNew = Task.Run(() => DownloadChangelog(objToken), objToken);
                    if (Interlocked.CompareExchange(ref _tskChangelogDownloader, tskNew, null) != null)
                    {
                        Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                                objNewChangelogSource);
                        try
                        {
                            objNewChangelogSource.Cancel(false);
                        }
                        finally
                        {
                            objNewChangelogSource.Dispose();
                        }
                    }
                    try
                    {
                        await tskNew.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                }
                else
                {
                    Interlocked.CompareExchange(ref _objChangelogDownloaderCancellationTokenSource, null,
                                                objNewChangelogSource);
                    objNewChangelogSource.Dispose();
                }
            }
        }

        private async void cmdRestart_Click(object sender, EventArgs e)
        {
            Log.Info("cmdRestart_Click");
            try
            {
                await DoUpdate(_objGenericToken).ConfigureAwait(false);
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
                await DoCleanReinstall(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task<bool> CreateBackupZip(CancellationToken token = default)
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
                        await using (FileStream objZipFileStream = new FileStream(strBackupZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            token.ThrowIfCancellationRequested();
                            using (ZipArchive zipNewArchive = new ZipArchive(objZipFileStream, ZipArchiveMode.Create))
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (string strFile in Directory.EnumerateFiles(_strAppPath))
                                {
                                    token.ThrowIfCancellationRequested();
                                    ZipArchiveEntry objEntry = zipNewArchive.CreateEntry(Path.GetFileName(strFile));
                                    objEntry.LastWriteTime = File.GetLastWriteTime(strFile);
                                    await using (FileStream objFileStream = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        token.ThrowIfCancellationRequested();
                                        await using (Stream objStream = objEntry.Open())
                                        {
                                            //magic number default buffer size, no overload that is only stream+token
                                            await objFileStream.CopyToAsync(objStream, 81920, token).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // ReSharper disable once MethodSupportsCancellation
                        await Utils.SafeDeleteDirectoryAsync(strBackupZipPath, token: CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                token.ThrowIfCancellationRequested();
                if (!SilentMode)
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                return false;
            }
            catch (IOException)
            {
                token.ThrowIfCancellationRequested();
                if (!SilentMode)
                    await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token).ConfigureAwait(false), Path.GetFileName(strBackupZipPath)), token: token).ConfigureAwait(false);
                return false;
            }
            catch (NotSupportedException)
            {
                token.ThrowIfCancellationRequested();
                if (!SilentMode)
                    await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token).ConfigureAwait(false), Path.GetFileName(strBackupZipPath)), token: token).ConfigureAwait(false);
                return false;
            }
            return true;
        }

        private async Task DoUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempLatestVersionZipPath))
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    await cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    if (!await CreateBackupZip(token).ConfigureAwait(false))
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
                            || strFileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase)
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

                    await InstallUpdateFromZip(_strTempLatestVersionZipPath, lstFilesToDelete, token).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task DoCleanReinstall(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_Updater_CleanReinstallPrompt", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_Updater_CleanReinstallPrompt", token: token).ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: token).ConfigureAwait(false) != DialogResult.Yes)
                return;
            if (Directory.Exists(_strAppPath) && File.Exists(_strTempLatestVersionZipPath))
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    await cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    if (!await CreateBackupZip(token).ConfigureAwait(false))
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
                            || strFileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
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

                    await InstallUpdateFromZip(_strTempLatestVersionZipPath, lstFilesToDelete, token).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task InstallUpdateFromZip(string strZipPath, List<string> lstFilesToDelete, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnDoRestart = true;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                List<string> lstPathsToRestoreOnFail = new List<string>(lstFilesToDelete.Count);
                List<string> lstPathsToDeleteOnFail = new List<string>(lstFilesToDelete.Count);
                // Copy over the archive from the temp directory.
                Log.Info("Extracting downloaded archive into application path: " + strZipPath);
                try
                {
                    using (ZipArchive zipArchive
                           = ZipFile.Open(strZipPath, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (ZipArchiveEntry objEntry in zipArchive.Entries)
                        {
                            token.ThrowIfCancellationRequested();
                            string strFullName = objEntry.FullName;
                            // Skip directories because they already get handled with Directory.CreateDirectory
                            if (strFullName.Length > 0 && strFullName[strFullName.Length - 1] == '/')
                                continue;
                            if (objEntry.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                                continue;
                            string strLoopPath = Path.Combine(_strAppPath, strFullName);
                            if (strLoopPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                                continue;
                            // Test to make sure we have no sneaky writes to outside our intended directory
                            strLoopPath = Path.GetFullPath(strLoopPath);
                            if (!strLoopPath.StartsWith(Path.GetFullPath(_strAppPath + Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                                continue;
                            try
                            {
                                string strLoopDirectory = Path.GetDirectoryName(strLoopPath);
                                if (!string.IsNullOrEmpty(strLoopDirectory))
                                    Directory.CreateDirectory(strLoopDirectory);
                                if (File.Exists(strLoopPath))
                                {
                                    if (!await FileExtensions.SafeDeleteAsync(
                                            strLoopPath + ".old", !SilentMode, token: token).ConfigureAwait(false))
                                    {
                                        blnDoRestart = false;
                                        break;
                                    }

                                    File.Move(strLoopPath, strLoopPath + ".old");
                                    lstPathsToRestoreOnFail.Add(strLoopPath + ".old");
                                }
                                else
                                    lstPathsToDeleteOnFail.Add(strLoopPath);

                                objEntry.ExtractToFile(strLoopPath, true);
                            }
                            catch (IOException)
                            {
                                if (!SilentMode)
                                {
                                    await Program.ShowScrollableMessageBoxAsync(
                                        this,
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync(
                                                    "Message_File_Cannot_Be_Accessed",
                                                    token: token)
                                                .ConfigureAwait(false),
                                            Path.GetFileName(strLoopPath)), token: token).ConfigureAwait(false);
                                }

                                blnDoRestart = false;
                                break;
                            }
                            catch (NotSupportedException)
                            {
                                if (!SilentMode)
                                {
                                    await Program.ShowScrollableMessageBoxAsync(
                                        this,
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync(
                                                    "Message_File_Cannot_Be_Accessed",
                                                    token: token)
                                                .ConfigureAwait(false),
                                            Path.GetFileName(strLoopPath)), token: token).ConfigureAwait(false);
                                }

                                blnDoRestart = false;
                                break;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                if (!SilentMode)
                                {
                                    await Program.ShowScrollableMessageBoxAsync(
                                        this,
                                        await LanguageManager.GetStringAsync(
                                                "Message_Insufficient_Permissions_Warning",
                                                token: token)
                                            .ConfigureAwait(false), token: token).ConfigureAwait(false);
                                }

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
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager
                                    .GetStringAsync("Message_File_Cannot_Be_Accessed", token: token)
                                    .ConfigureAwait(false),
                                strZipPath), token: token).ConfigureAwait(false);
                    }

                    blnDoRestart = false;
                }
                catch (NotSupportedException)
                {
                    if (!SilentMode)
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager
                                    .GetStringAsync("Message_File_Cannot_Be_Accessed", token: token)
                                    .ConfigureAwait(false),
                                strZipPath), token: token).ConfigureAwait(false);
                    }

                    blnDoRestart = false;
                }
                catch (UnauthorizedAccessException)
                {
                    if (!SilentMode)
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            this,
                            await LanguageManager
                                .GetStringAsync("Message_Insufficient_Permissions_Warning", token: token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    }

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
                        dicTasks.Add(strFileToDelete, FileExtensions.SafeDeleteAsync(strFileToDelete, token: token));
                        if (++intCounter != Utils.MaxParallelBatchSize)
                            continue;
                        await Task.WhenAll(dicTasks.Values).ConfigureAwait(false);
                        foreach (KeyValuePair<string, Task<bool>> kvpTaskPair in dicTasks)
                        {
                            if (!await kvpTaskPair.Value.ConfigureAwait(false))
                                lstBlocked.Add(kvpTaskPair.Key);
                        }

                        dicTasks.Clear();
                        intCounter = 0;
                    }

                    await Task.WhenAll(dicTasks.Values).ConfigureAwait(false);
                    foreach (KeyValuePair<string, Task<bool>> kvpTaskPair in dicTasks)
                    {
                        if (!await kvpTaskPair.Value.ConfigureAwait(false))
                            lstBlocked.Add(kvpTaskPair.Key);
                    }

                    foreach (string strBlockedFile in lstBlocked.ToList())
                    {
                        try
                        {
                            if (File.Exists(strBlockedFile + ".old")
                                && !await FileExtensions.SafeDeleteAsync(strBlockedFile + ".old", !SilentMode, token: token)
                                               .ConfigureAwait(false))
                            {
                                continue;
                            }

                            File.Move(strBlockedFile, strBlockedFile + ".old");
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                        catch (NotSupportedException)
                        {
                            continue;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            continue;
                        }

                        lstBlocked.Remove(strBlockedFile);
                    }

                    if (lstBlocked.Count > 0)
                    {
                        Utils.BreakIfDebug();
                        if (!SilentMode)
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdOutput))
                            {
                                sbdOutput.Append(
                                    await LanguageManager
                                          .GetStringAsync("Message_Files_Cannot_Be_Removed", token: token)
                                          .ConfigureAwait(false));
                                foreach (string strFile in lstBlocked)
                                {
                                    sbdOutput.AppendLine().Append(strFile);
                                }

                                await Program.ShowScrollableMessageBoxAsync(this, sbdOutput.ToString(), null, MessageBoxButtons.OK,
                                    MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string strToDelete in lstPathsToDeleteOnFail)
                    {
                        try
                        {
                            await FileExtensions.SafeDeleteAsync(strToDelete, token: token).ConfigureAwait(false);
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
                    foreach (string strBackupFileName in lstPathsToRestoreOnFail)
                    {
                        try
                        {
                            string strOriginal = Path.GetFileNameWithoutExtension(strBackupFileName);
                            if (File.Exists(strOriginal)
                                && !await FileExtensions.SafeDeleteAsync(strOriginal, token: token)
                                               .ConfigureAwait(false))
                                continue;
                            File.Move(strBackupFileName, strOriginal);
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
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            if (blnDoRestart)
                await Utils.RestartApplication(token: token).ConfigureAwait(false);
        }

        private async Task DownloadUpdates(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Uri.TryCreate(_strDownloadFile, UriKind.Absolute, out Uri uriDownloadFileAddress))
            {
                Log.Debug("DownloadUpdates");
                token.ThrowIfCancellationRequested();
                await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdRestart.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                if (File.Exists(_strTempLatestVersionZipPath))
                {
                    File.Move(_strTempLatestVersionZipPath, _strTempLatestVersionZipPath + ".old");
                }
                token.ThrowIfCancellationRequested();
                try
                {
                    using (token.Register(() => _clientDownloader.CancelAsync()))
                        await _clientDownloader.DownloadFileTaskAsync(uriDownloadFileAddress, _strTempLatestVersionZipPath).ConfigureAwait(false);
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
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync(
                                        "Warning_Update_CouldNotConnectException",
                                        token: token)
                                    .ConfigureAwait(false),
                                strException), Application.ProductName, MessageBoxButtons.OK,
                            MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                    }

                    await cmdUpdate.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
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
                    await pgbOverallProgress.DoThreadSafeAsync(x => x.Value = intTmp, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
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
                if (File.Exists(_strTempLatestVersionZipPath + ".old"))
                {
                    if (e.Cancelled || e.Error != null)
                    {
                        await FileExtensions.SafeDeleteAsync(_strTempLatestVersionZipPath, !SilentMode,
                                                        token: _objGenericToken).ConfigureAwait(false);
                        File.Move(_strTempLatestVersionZipPath + ".old", _strTempLatestVersionZipPath);
                    }
                    else
                    {
                        await FileExtensions.SafeDeleteAsync(_strTempLatestVersionZipPath + ".old", !SilentMode,
                                                        token: _objGenericToken).ConfigureAwait(false);
                    }
                }

                string strText1 = await LanguageManager.GetStringAsync("Button_Redownload", token: _objGenericToken).ConfigureAwait(false);
                string strText2 = await LanguageManager.GetStringAsync("Button_Up_To_Date", token: _objGenericToken).ConfigureAwait(false);
                await cmdUpdate.DoThreadSafeAsync(x =>
                {
                    x.Text = strText1;
                    x.Enabled = true;
                }, _objGenericToken).ConfigureAwait(false);
                await cmdRestart.DoThreadSafeAsync(x =>
                {
                    if (_blnIsConnected && x.Text != strText2)
                        x.Enabled = true;
                }, _objGenericToken).ConfigureAwait(false);
                await cmdCleanReinstall.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken).ConfigureAwait(false);
                Log.Info("wc_DownloadExeFileCompleted exit");
                if (SilentMode)
                {
                    if (await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_Update_CloseForms", token: _objGenericToken).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync(
                                "Title_Update", token: _objGenericToken).ConfigureAwait(false), MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question, token: _objGenericToken).ConfigureAwait(false) ==
                        DialogResult.Yes)
                    {
                        await DoUpdate(_objGenericToken).ConfigureAwait(false);
                    }
                    else
                    {
                        _blnSilentModeUpdateWasDenied
                            = true; // only actually go through with the attempt in Silent Mode once, don't prompt user any more if they canceled out once already
                        _blnIsConnected = false;
                        await this.DoThreadSafeAsync(x => x.Close(), _objGenericToken).ConfigureAwait(false);
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
