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
using System.Linq;
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

        #region Control Events

        public SelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _eStartingBuildMethod = _objCharacter.Settings.BuildMethod;
            _blnForExistingCharacter = blnUseCurrentValues;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            if (!(await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue) is CharacterSettings objSelectedGameplayOption))
                return;
            CharacterBuildMethod eSelectedBuildMethod = objSelectedGameplayOption.BuildMethod;
            if (_blnForExistingCharacter && !_objCharacter.Created && _objCharacter.Settings.BuildMethod == _objCharacter.EffectiveBuildMethod && eSelectedBuildMethod != _eStartingBuildMethod)
            {
                if (Program.ShowMessageBox(this,
                    string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_SelectBP_SwitchBuildMethods"),
                        await LanguageManager.GetStringAsync("String_" + eSelectedBuildMethod), await LanguageManager.GetStringAsync("String_" + _eStartingBuildMethod)).WordWrap(),
                    await LanguageManager.GetStringAsync("MessageTitle_SelectBP_SwitchBuildMethods"), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                string strOldCharacterSettingsKey = _objCharacter.SettingsKey;
                _objCharacter.SettingsKey = (await SettingsManager.LoadedCharacterSettings
                    .FirstOrDefaultAsync(x => ReferenceEquals(x.Value, objSelectedGameplayOption))).Key;
                // If the character is loading, make sure we only switch build methods after we've loaded, otherwise we might cause all sorts of nastiness
                if (_objCharacter.IsLoading)
                    await _objCharacter.PostLoadMethodsAsync.EnqueueAsync(() => _objCharacter.SwitchBuildMethods(_eStartingBuildMethod, eSelectedBuildMethod, strOldCharacterSettingsKey));
                else if (!await _objCharacter.SwitchBuildMethods(_eStartingBuildMethod, eSelectedBuildMethod, strOldCharacterSettingsKey))
                    return;
            }
            else
            {
                _objCharacter.SettingsKey = (await SettingsManager.LoadedCharacterSettings
                                                                  .FirstOrDefaultAsync(
                                                                      x => ReferenceEquals(
                                                                          x.Value, objSelectedGameplayOption))).Key;
            }
            _objCharacter.IgnoreRules = chkIgnoreRules.Checked;
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cmdEditCharacterOption_Click(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
            {
                object objOldSelected = await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue);
                using (ThreadSafeForm<EditCharacterSettings> frmOptions
                       = await ThreadSafeForm<EditCharacterSettings>.GetAsync(
                           () => new EditCharacterSettings(objOldSelected as CharacterSettings)))
                    await frmOptions.ShowDialogSafeAsync(this);

                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    // Populate the Gameplay Settings list.
                    using (new FetchSafelyFromPool<List<ListItem>>(
                               Utils.ListItemListPool, out List<ListItem> lstGameplayOptions))
                    {
                        lstGameplayOptions.AddRange(SettingsManager.LoadedCharacterSettings.Values
                                                                   .Select(objLoopOptions =>
                                                                               new ListItem(
                                                                                   objLoopOptions,
                                                                                   objLoopOptions.DisplayName)));
                        lstGameplayOptions.Sort(CompareListItems.CompareNames);
                        await cboCharacterSetting.PopulateWithListItemsAsync(lstGameplayOptions);
                        await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objOldSelected);
                        if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex) == -1
                            && lstGameplayOptions.Count > 0)
                        {
                            (bool blnSuccess, CharacterSettings objSetting)
                                = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                    GlobalSettings.DefaultCharacterSetting);
                            await cboCharacterSetting.DoThreadSafeAsync(x =>
                            {
                                if (blnSuccess)
                                    x.SelectedValue = objSetting;
                                if (x.SelectedIndex == -1 && lstGameplayOptions.Count > 0)
                                {
                                    x.SelectedIndex = 0;
                                }
                            });
                        }
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void SelectBuildMethod_Load(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    // Populate the Character Settings list.
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstCharacterSettings))
                    {
                        foreach (CharacterSettings objLoopSetting in SettingsManager.LoadedCharacterSettings.Values)
                        {
                            lstCharacterSettings.Add(new ListItem(objLoopSetting, objLoopSetting.DisplayName));
                        }

                        lstCharacterSettings.Sort(CompareListItems.CompareNames);
                        await cboCharacterSetting.PopulateWithListItemsAsync(lstCharacterSettings);
                        if (_blnForExistingCharacter)
                        {
                            (bool blnSuccess, CharacterSettings objSetting)
                                = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                    _objCharacter.SettingsKey);
                            if (blnSuccess)
                                await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSetting);
                            if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex) == -1)
                            {
                                CharacterSettings objSetting2;
                                (blnSuccess, objSetting2)
                                    = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                        GlobalSettings.DefaultCharacterSetting);
                                if (blnSuccess)
                                    await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSetting2);
                            }

                            await chkIgnoreRules.DoThreadSafeAsync(x => x.Checked = _objCharacter.IgnoreRules);
                        }
                        else
                        {
                            (bool blnSuccess, CharacterSettings objSetting)
                                = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                    GlobalSettings.DefaultCharacterSetting);
                            if (blnSuccess)
                                await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSetting);
                        }

                        if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex) == -1
                            && lstCharacterSettings.Count > 0)
                        {
                            await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedIndex = 0);
                        }
                    }

                    await chkIgnoreRules.SetToolTipAsync(await LanguageManager.GetStringAsync("Tip_SelectKarma_IgnoreRules"));
                    await ProcessGameplayIndexChanged();
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    await ProcessGameplayIndexChanged();
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async ValueTask ProcessGameplayIndexChanged(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Load the Priority information.
            if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, token) is CharacterSettings objSelectedGameplayOption)
            {
                string strText = await LanguageManager.GetStringAsync("String_" + objSelectedGameplayOption.BuildMethod);
                await lblBuildMethod.DoThreadSafeAsync(x => x.Text = strText, token);
                switch (objSelectedGameplayOption.BuildMethod)
                {
                    case CharacterBuildMethod.Priority:
                        strText = await LanguageManager.GetStringAsync("Label_SelectBP_Priorities");
                        await lblBuildMethodParamLabel.DoThreadSafeAsync(x =>
                        {
                            x.Text = strText;
                            x.Visible = true;
                        }, token);
                        await lblBuildMethodParam.DoThreadSafeAsync(x =>
                        {
                            x.Text = objSelectedGameplayOption.PriorityArray;
                            x.Visible = true;
                        }, token);
                        break;

                    case CharacterBuildMethod.SumtoTen:
                        strText = await LanguageManager.GetStringAsync("String_SumtoTen");
                        await lblBuildMethodParamLabel.DoThreadSafeAsync(x =>
                        {
                            x.Text = strText;
                            x.Visible = true;
                        }, token);
                        await lblBuildMethodParam.DoThreadSafeAsync(x =>
                        {
                            x.Text = objSelectedGameplayOption.SumtoTen.ToString(GlobalSettings.CultureInfo);
                            x.Visible = true;
                        }, token);
                        break;

                    default:
                        await lblBuildMethodParamLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                        await lblBuildMethodParam.DoThreadSafeAsync(x => x.Visible = false, token);
                        break;
                }

                string strNone = await LanguageManager.GetStringAsync("String_None");

                await lblMaxAvail.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.MaximumAvailability.ToString(GlobalSettings.CultureInfo), token);
                await lblKarma.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.BuildKarma.ToString(GlobalSettings.CultureInfo), token);
                await lblMaxNuyen.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.NuyenMaximumBP.ToString(GlobalSettings.CultureInfo), token);
                await lblQualityKarma.DoThreadSafeAsync(x => x.Text = objSelectedGameplayOption.QualityKarmaLimit.ToString(GlobalSettings.CultureInfo), token);

                await lblBooks.DoThreadSafeAsync(x =>
                {
                    x.Text = _objCharacter.TranslatedBookList(string.Join(";",
                                                                          objSelectedGameplayOption.Books));
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strNone;
                }, token);

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdCustomDataDirectories))
                {
                    foreach (CustomDataDirectoryInfo objLoopInfo in objSelectedGameplayOption
                                 .EnabledCustomDataDirectoryInfos)
                        sbdCustomDataDirectories.AppendLine(objLoopInfo.Name);

                    await lblCustomData.DoThreadSafeAsync(x =>
                    {
                        x.Text = sbdCustomDataDirectories.ToString();
                        if (string.IsNullOrEmpty(x.Text))
                            x.Text = strNone;
                    }, token);
                }
            }
        }

        #endregion Control Events
    }
}
