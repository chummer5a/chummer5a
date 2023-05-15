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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public sealed partial class SelectBuildMethod : Form
    {
        private readonly Character _objCharacter;
        private readonly CharacterBuildMethod _eStartingBuildMethod;
        private readonly bool _blnForExistingCharacter;
        private int _intLoading = 1;

        private CancellationTokenSource _objProcessCharacterSettingIndexChangedCancellationTokenSource;
        private CancellationTokenSource _objRepopulateCharacterSettingsCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        #region Control Events

        public SelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
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
            };
            _eStartingBuildMethod = _objCharacter.Settings.BuildMethod;
            _blnForExistingCharacter = blnUseCurrentValues;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, _objGenericToken).ConfigureAwait(false) is CharacterSettings objSelectedGameplayOption))
                    return;
                CharacterBuildMethod eSelectedBuildMethod = objSelectedGameplayOption.BuildMethod;
                if (_blnForExistingCharacter && !_objCharacter.Created && _objCharacter.Settings.BuildMethod == _objCharacter.EffectiveBuildMethod && eSelectedBuildMethod != _eStartingBuildMethod)
                {
                    if (Program.ShowScrollableMessageBox(this,
                                                         string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_SelectBP_SwitchBuildMethods", token: _objGenericToken).ConfigureAwait(false),
                                                                       await LanguageManager.GetStringAsync("String_" + eSelectedBuildMethod, token: _objGenericToken).ConfigureAwait(false), await LanguageManager.GetStringAsync("String_" + _eStartingBuildMethod, token: _objGenericToken).ConfigureAwait(false)).WordWrap(),
                                                         await LanguageManager.GetStringAsync("MessageTitle_SelectBP_SwitchBuildMethods", token: _objGenericToken).ConfigureAwait(false), MessageBoxButtons.YesNo,
                                                         MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;
                    string strOldCharacterSettingsKey = await _objCharacter.GetSettingsKeyAsync(_objGenericToken).ConfigureAwait(false);
                    await _objCharacter.SetSettingsKeyAsync((await (await SettingsManager.GetLoadedCharacterSettingsAsync(_objGenericToken).ConfigureAwait(false))
                                                                   .FirstOrDefaultAsync(x => ReferenceEquals(x.Value, objSelectedGameplayOption), _objGenericToken).ConfigureAwait(false)).Key, _objGenericToken).ConfigureAwait(false);
                    // If the character is loading, make sure we only switch build methods after we've loaded, otherwise we might cause all sorts of nastiness
                    if (_objCharacter.IsLoading)
                        await _objCharacter.EnqueuePostLoadAsyncMethodAsync(x => _objCharacter.SwitchBuildMethods(_eStartingBuildMethod, eSelectedBuildMethod, strOldCharacterSettingsKey, x), _objGenericToken).ConfigureAwait(false);
                    else if (!await _objCharacter.SwitchBuildMethods(_eStartingBuildMethod, eSelectedBuildMethod, strOldCharacterSettingsKey, _objGenericToken).ConfigureAwait(false))
                        return;
                }
                else
                {
                    await _objCharacter.SetSettingsKeyAsync((await (await SettingsManager.GetLoadedCharacterSettingsAsync(_objGenericToken).ConfigureAwait(false))
                                                                   .FirstOrDefaultAsync(
                                                                       x => ReferenceEquals(
                                                                           x.Value, objSelectedGameplayOption), _objGenericToken).ConfigureAwait(false)).Key, _objGenericToken).ConfigureAwait(false);
                }
                _objCharacter.IgnoreRules = await chkIgnoreRules.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);
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

        private async void SelectBuildMethod_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        CharacterSettings objSelectSettings = null;
                        if (_blnForExistingCharacter)
                        {
                            IAsyncReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings
                                = await SettingsManager.GetLoadedCharacterSettingsAsync(_objGenericToken).ConfigureAwait(false);
                            (bool blnSuccess, CharacterSettings objSetting)
                                = await dicCharacterSettings.TryGetValueAsync(
                                    await _objCharacter.GetSettingsKeyAsync(_objGenericToken).ConfigureAwait(false), _objGenericToken).ConfigureAwait(false);
                            if (blnSuccess)
                                objSelectSettings = objSetting;
                        }

                        await RepopulateCharacterSettings(objSelectSettings, _objGenericToken).ConfigureAwait(false);
                        await chkIgnoreRules.SetToolTipAsync(
                                                await LanguageManager.GetStringAsync("Tip_SelectKarma_IgnoreRules", token: _objGenericToken)
                                                                     .ConfigureAwait(false), _objGenericToken)
                                            .ConfigureAwait(false);
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

        private void SelectBuildMethod_Closing(object sender, FormClosingEventArgs e)
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

        private async ValueTask RepopulateCharacterSettings(CharacterSettings objSelected = null,
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
                        using (new FetchSafelyFromPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstCharacterSettings))
                        {
                            IAsyncReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings
                                = await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false);
                            (bool blnSuccess, CharacterSettings objSetting)
                                = await dicCharacterSettings.TryGetValueAsync(
                                    GlobalSettings.DefaultCharacterSetting, token).ConfigureAwait(false);
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

        private async ValueTask ProcessCharacterSettingIndexChanged(CancellationToken token = default)
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
                    string strText = await LanguageManager.GetStringAsync("String_" + objSelectedGameplayOption.BuildMethod, token: token).ConfigureAwait(false);
                    await lblBuildMethod.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    switch (objSelectedGameplayOption.BuildMethod)
                    {
                        case CharacterBuildMethod.Priority:
                            {
                                string strText2 = await LanguageManager.GetStringAsync("Label_SelectBP_Priorities", token: token)
                                                               .ConfigureAwait(false);
                                await lblBuildMethodParamLabel.DoThreadSafeAsync(x =>
                                {
                                    x.Text = strText2;
                                    x.Visible = true;
                                }, token).ConfigureAwait(false);
                                await lblBuildMethodParam.DoThreadSafeAsync(x =>
                                {
                                    x.Text = objSelectedGameplayOption.PriorityArray;
                                    x.Visible = true;
                                }, token).ConfigureAwait(false);
                                break;
                            }
                        case CharacterBuildMethod.SumtoTen:
                            {
                                string strText2 = await LanguageManager.GetStringAsync("String_SumtoTen", token: token)
                                                               .ConfigureAwait(false);
                                await lblBuildMethodParamLabel.DoThreadSafeAsync(x =>
                                {
                                    x.Text = strText2;
                                    x.Visible = true;
                                }, token).ConfigureAwait(false);
                                await lblBuildMethodParam.DoThreadSafeAsync(x =>
                                {
                                    x.Text = objSelectedGameplayOption.SumtoTen.ToString(GlobalSettings.CultureInfo);
                                    x.Visible = true;
                                }, token).ConfigureAwait(false);
                                break;
                            }
                        default:
                            await lblBuildMethodParamLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                            await lblBuildMethodParam.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                            break;
                    }

                    string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);

                    await lblMaxAvail.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.MaximumAvailability.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);
                    await lblKarma.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.BuildKarma.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);
                    await lblMaxNuyen.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.NuyenMaximumBP.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);
                    await lblQualityKarma.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.QualityKarmaLimit.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                    string strBookList = await _objCharacter.TranslatedBookListAsync(string.Join(";",
                        objSelectedGameplayOption.Books), token: token).ConfigureAwait(false);
                    await lblBooks.DoThreadSafeAsync(x =>
                    {
                        x.Text = strBookList;
                        if (string.IsNullOrEmpty(x.Text))
                            x.Text = strNone;
                    }, token).ConfigureAwait(false);

                    List<string> lstCustomData = new List<string>();
                    foreach (CustomDataDirectoryInfo objLoopInfo in objSelectedGameplayOption
                                 .EnabledCustomDataDirectoryInfos)
                        lstCustomData.Add(objLoopInfo.DisplayName);
                    lstCustomData.Sort();
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCustomDataDirectories))
                    {
                        foreach (string strName in lstCustomData)
                            sbdCustomDataDirectories.AppendLine(strName);

                        await lblCustomData.DoThreadSafeAsync(x =>
                        {
                            x.Text = sbdCustomDataDirectories.ToString();
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strNone;
                        }, token).ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion Control Events
    }
}
