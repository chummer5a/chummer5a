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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public sealed partial class DataExporter : Form
    {
        private int _intLoading = 1;

        private CancellationTokenSource _objProcessCharacterSettingIndexChangedCancellationTokenSource;
        private CancellationTokenSource _objRepopulateCharacterSettingsCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;
        private readonly ConcurrentDictionary<string, string> _dicCachedLanguageDocumentNames =
            new ConcurrentDictionary<string, string>();
        private readonly DebuggableSemaphoreSlim _objExportSemaphore = new DebuggableSemaphoreSlim();

        #region Control Events

        public DataExporter()
        {
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessCharacterSettingIndexChangedCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objRepopulateCharacterSettingsCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericCancellationTokenSource.Dispose();
                dlgSaveFile?.Dispose();
                _objExportSemaphore?.Dispose();
            };
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cmdEditCharacterOption_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                        try
                        {
                            CharacterSettings objOldSelected = await cboCharacterSetting
                                                                     .DoThreadSafeFuncAsync(x => x.SelectedValue, _objGenericToken)
                                                                     .ConfigureAwait(false) as CharacterSettings;
                            using (ThreadSafeForm<EditCharacterSettings> frmOptions
                                   = await ThreadSafeForm<EditCharacterSettings>.GetAsync(
                                                                                    () => new EditCharacterSettings(
                                                                                        objOldSelected), _objGenericToken)
                                                                                .ConfigureAwait(false))
                                await frmOptions.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false);

                            await RepopulateCharacterSettings(token: _objGenericToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void DataExporter_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await RefreshLanguageDocumentNames(_objGenericToken).ConfigureAwait(false);
                        await PopulateLanguageList(_objGenericToken).ConfigureAwait(false);
                        await RepopulateCharacterSettings(token: _objGenericToken).ConfigureAwait(false);
                        await pgbExportProgress.DoThreadSafeAsync(x => x.Maximum = Utils.BasicDataFileNames.Count, _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }

                Interlocked.Decrement(ref _intLoading);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void DataExporter_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessCharacterSettingIndexChangedCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objRepopulateCharacterSettingsCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async Task RefreshLanguageDocumentNames(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _dicCachedLanguageDocumentNames.Clear();
            token.ThrowIfCancellationRequested();
            foreach (string strFilePath in Directory.EnumerateFiles(Utils.GetLanguageFolderPath, "*.xml"))
            {
                token.ThrowIfCancellationRequested();
                if (strFilePath.EndsWith("_data.xml"))
                    continue;
                string strLanguageName = await LanguageManager.GetLanguageNameFromFileNameAsync(strFilePath, token: token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strLanguageName))
                    continue;
                token.ThrowIfCancellationRequested();
                _dicCachedLanguageDocumentNames.AddOrUpdate(Path.GetFileNameWithoutExtension(strFilePath), x => strLanguageName, (x, y) => strLanguageName);
            }
        }

        private async Task PopulateLanguageList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstLanguages))
            {
                foreach (KeyValuePair<string, string> kvpLanguages in _dicCachedLanguageDocumentNames)
                {
                    token.ThrowIfCancellationRequested();
                    lstLanguages.Add(new ListItem(kvpLanguages.Key, kvpLanguages.Value));
                }

                token.ThrowIfCancellationRequested();
                lstLanguages.Sort(CompareListItems.CompareNames);
                await cboLanguage.PopulateWithListItemsAsync(lstLanguages, token).ConfigureAwait(false);
                await cboLanguage.DoThreadSafeAsync(x =>
                {
                    x.SelectedValue = GlobalSettings.Language;
                    if (x.SelectedIndex == -1)
                        x.SelectedValue = GlobalSettings.DefaultLanguage;
                }, token).ConfigureAwait(false);
            }
        }

        private async void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await pgbExportProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                string strLanguage = await cboLanguage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                await imgSheetLanguageFlag.DoThreadSafeAsync(x => x.Image = FlagImageGetter.GetFlagFromCountryCode(
                    strLanguage.Substring(3, 2),
                    Math.Min(x.Width, x.Height)), token: _objGenericToken).ConfigureAwait(false);
                await ValidateIfExportPossible(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task RepopulateCharacterSettings(CharacterSettings objSelected = null,
                                                            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objRepopulateCharacterSettingsCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        object objOldSelected = objSelected ?? await cboCharacterSetting
                                                                     .DoThreadSafeFuncAsync(
                                                                         x => x.SelectedValue, token: token)
                                                                     .ConfigureAwait(false);
                        // Populate the Gameplay Settings list.
                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstCharacterSettings))
                        {
                            IReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings
                                = await SettingsManager.GetLoadedCharacterSettingsAsync(_objGenericToken).ConfigureAwait(false);
                            bool blnSuccess = dicCharacterSettings.TryGetValue(
                                GlobalSettings.DefaultCharacterSetting, out CharacterSettings objSetting);
                            await dicCharacterSettings.ForEachAsync(async x =>
                            {
                                lstCharacterSettings.Add(new ListItem(x.Value,
                                                                      await x.Value
                                                                             .GetCurrentDisplayNameAsync(token)
                                                                             .ConfigureAwait(false)));
                            }, token).ConfigureAwait(false);

                            lstCharacterSettings.Sort(CompareListItems.CompareNames);
                            await cboCharacterSetting.PopulateWithListItemsAsync(lstCharacterSettings, token: token)
                                                     .ConfigureAwait(false);
                            await cboCharacterSetting.DoThreadSafeAsync(x =>
                            {
                                if (objOldSelected != null)
                                    x.SelectedValue = objOldSelected;
                                if (objOldSelected == null || x.SelectedIndex == -1)
                                {
                                    if (blnSuccess)
                                        x.SelectedValue = objSetting;
                                    if (x.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                                    {
                                        x.SelectedIndex = 0;
                                    }
                                }
                            }, token: token).ConfigureAwait(false);
                        }

                        await ProcessCharacterSettingIndexChanged(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async void cboCharacterSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await ProcessCharacterSettingIndexChanged(_objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task ProcessCharacterSettingIndexChanged(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessCharacterSettingIndexChangedCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                // Load the Priority information.
                if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, token).ConfigureAwait(false) is CharacterSettings objSelectedGameplayOption)
                {
                    string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
                    await pgbExportProgress.DoThreadSafeAsync(x => x.Value = 0, token).ConfigureAwait(false);
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdCustomDataDirectories))
                    {
                        foreach (CustomDataDirectoryInfo objLoopInfo in await objSelectedGameplayOption
                                     .GetEnabledCustomDataDirectoryInfosAsync(token).ConfigureAwait(false))
                            sbdCustomDataDirectories.AppendLine(await objLoopInfo.GetDisplayNameAsync(token)
                                .ConfigureAwait(false));

                        await lblCustomData.DoThreadSafeAsync(x =>
                        {
                            x.Text = sbdCustomDataDirectories.ToString();
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strNone;
                        }, token).ConfigureAwait(false);
                    }
                }
                await ValidateIfExportPossible(token).ConfigureAwait(false);
            }
        }

        private async Task ValidateIfExportPossible(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_objExportSemaphore.CurrentCount == 0)
            {
                // An export is currently underway, do not allow any additional exports until the current one is finished (just in case)
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, _objGenericToken).ConfigureAwait(false);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, _objGenericToken).ConfigureAwait(false);
            }
            string strLanguage = await cboLanguage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                // For English, only allow export if we have any custom data
                bool blnValidExport = false;
                if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, token).ConfigureAwait(false) is CharacterSettings objSettings)
                {
                    blnValidExport = (await objSettings.GetEnabledCustomDataDirectoryInfosAsync(token).ConfigureAwait(false)).Count > 0;
                }
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = blnValidExport, _objGenericToken).ConfigureAwait(false);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = blnValidExport, _objGenericToken).ConfigureAwait(false);
            }
            else
            {
                // Non-English languages are always valid for export because the translations can modify the base data files
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken).ConfigureAwait(false);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken).ConfigureAwait(false);
            }
        }

        private async Task DoExport(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, token).ConfigureAwait(false) is CharacterSettings objSettings))
                return;
            dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Zip", token: token).ConfigureAwait(false) + '|' + await LanguageManager.GetStringAsync("DialogFilter_All", token: token).ConfigureAwait(false);
            dlgSaveFile.Title = await LanguageManager.GetStringAsync("Button_Export_SaveDataAs", token: token).ConfigureAwait(false);
            DialogResult eResult = await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token).ConfigureAwait(false);
            if (eResult == DialogResult.Cancel)
                throw new OperationCanceledException();
            if (eResult != DialogResult.OK)
                return;
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                if (!await _objExportSemaphore.WaitAsync(Utils.WaitEmergencyReleaseMaxTicks, token).ConfigureAwait(false))
                    throw new OperationCanceledException();
                try
                {
                    await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    await pgbExportProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    string strLanguage = await cboLanguage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strLanguage))
                        strLanguage = GlobalSettings.DefaultLanguage;
                    string strSaveArchive = dlgSaveFile.FileName;
                    if (string.IsNullOrEmpty(strSaveArchive))
                        return;
                    if (!strSaveArchive.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        strSaveArchive += ".zip";
                    await Utils.SafeDeleteDirectoryAsync(strSaveArchive, token: token).ConfigureAwait(false);
                    IAsyncDisposable objLocker = await objSettings.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            using (FileStream objZipFileStream = new FileStream(strSaveArchive, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                token.ThrowIfCancellationRequested();
                                using (ZipArchive zipNewArchive = new ZipArchive(objZipFileStream, ZipArchiveMode.Create))
                                {
                                    token.ThrowIfCancellationRequested();
                                    foreach (string strFileName in Utils.BasicDataFileNames)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        XmlDocument xmlDocument = await objSettings.LoadDataAsync(strFileName, strLanguage, token: token).ConfigureAwait(false);
                                        ZipArchiveEntry objEntry = zipNewArchive.CreateEntry(Path.GetFileName(strFileName));
                                        token.ThrowIfCancellationRequested();
                                        using (Stream objStream = objEntry.Open())
                                        {
                                            token.ThrowIfCancellationRequested();
                                            await Task.Run(() => xmlDocument.Save(objStream), token).ConfigureAwait(false);
                                        }
                                        await pgbExportProgress.DoThreadSafeAsync(x => ++x.Value, _objGenericToken).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // ReSharper disable once MethodSupportsCancellation
                            await Utils.SafeDeleteDirectoryAsync(strSaveArchive, token: CancellationToken.None).ConfigureAwait(false);
                            throw;
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    _objExportSemaphore.Release();
                    await ValidateIfExportPossible(token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Control Events

        private async void cmdExport_Click(object sender, EventArgs e)
        {
            try
            {
                await DoExport(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdExportClose_Click(object sender, EventArgs e)
        {
            try
            {
                await DoExport(_objGenericToken).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }
    }
}
