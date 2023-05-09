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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    public partial class EditCharacterSettings : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly CharacterSettings _objCharacterSettings;
        private CharacterSettings _objReferenceCharacterSettings;
        private List<ListItem> _lstSettings = Utils.ListItemListPool.Get();

        // List of custom data directory infos on the character, in load order. If the character has a directory name for which we have no info, key will be a string instead of an info
        private readonly TypedOrderedDictionary<object, bool> _dicCharacterCustomDataDirectoryInfos
            = new TypedOrderedDictionary<object, bool>();

        private int _intLoading = 1;
        private int _intSkipLimbCountUpdate;
        private bool _blnDirty;
        private bool _blnSourcebookToggle = true;
        private bool _blnWasRenamed;
        private int _intSuspendLayoutCount;
        private bool _blnForceMasterIndexRepopulateOnClose;

        // Used to revert to old selected setting if user cancels out of selecting a different one
        private int _intOldSelectedSettingIndex = -1;

        private HashSet<string> _setPermanentSourcebooks = Utils.StringHashSetPool.Get();

        #region Form Events

        public EditCharacterSettings(CharacterSettings objExistingSettings = null)
        {
            InitializeComponent();
            tabOptions.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objReferenceCharacterSettings = objExistingSettings;
            if (_objReferenceCharacterSettings == null)
            {
                if (SettingsManager.LoadedCharacterSettings.TryGetValue(GlobalSettings.DefaultCharacterSetting,
                                                                        out CharacterSettings objSetting))
                    _objReferenceCharacterSettings = objSetting;
                else if (SettingsManager.LoadedCharacterSettings.TryGetValue(
                             GlobalSettings.DefaultCharacterSettingDefaultValue,
                             out objSetting))
                    _objReferenceCharacterSettings = objSetting;
                else
                    _objReferenceCharacterSettings = SettingsManager.LoadedCharacterSettings.Values.First();
            }

            _objCharacterSettings = new CharacterSettings(_objReferenceCharacterSettings);
            using (_objCharacterSettings.LockObject.EnterWriteLock())
                _objCharacterSettings.PropertyChanged += SettingsChanged;
            Disposed += (sender, args) =>
            {
                using (_objCharacterSettings.LockObject.EnterWriteLock())
                    _objCharacterSettings.PropertyChanged -= SettingsChanged;
                _objCharacterSettings.Dispose();
                Utils.ListItemListPool.Return(ref _lstSettings);
                Utils.StringHashSetPool.Return(ref _setPermanentSourcebooks);
            };
        }

        private async void EditCharacterSettings_Load(object sender, EventArgs e)
        {
            await RebuildCustomDataDirectoryInfosAsync().ConfigureAwait(false);
            await SetToolTips().ConfigureAwait(false);
            await PopulateSettingsList().ConfigureAwait(false);

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstBuildMethods))
            {
                lstBuildMethods.Add(new ListItem(CharacterBuildMethod.Priority,
                                                 await LanguageManager.GetStringAsync("String_Priority")
                                                                      .ConfigureAwait(false)));
                lstBuildMethods.Add(new ListItem(CharacterBuildMethod.SumtoTen,
                                                 await LanguageManager.GetStringAsync("String_SumtoTen")
                                                                      .ConfigureAwait(false)));
                lstBuildMethods.Add(new ListItem(CharacterBuildMethod.Karma,
                                                 await LanguageManager.GetStringAsync("String_Karma")
                                                                      .ConfigureAwait(false)));
                if (GlobalSettings.LifeModuleEnabled)
                    lstBuildMethods.Add(new ListItem(CharacterBuildMethod.LifeModule,
                                                     await LanguageManager.GetStringAsync("String_LifeModule")
                                                                          .ConfigureAwait(false)));

                await cboBuildMethod.PopulateWithListItemsAsync(lstBuildMethods).ConfigureAwait(false);
            }

            await PopulateOptions().ConfigureAwait(false);
            await SetupDataBindings().ConfigureAwait(false);

            await SetIsDirtyAsync(false).ConfigureAwait(false);
            Interlocked.Decrement(ref _intLoading);
        }

        #endregion Form Events

        #region Control Events

        private async void cmdGlobalOptionsCustomData_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                using (ThreadSafeForm<EditGlobalSettings> frmOptions =
                       await ThreadSafeForm<EditGlobalSettings>.GetAsync(() =>
                                                                             new EditGlobalSettings(
                                                                                 "tabCustomDataDirectories"))
                                                               .ConfigureAwait(false))
                    await frmOptions.ShowDialogSafeAsync(this).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdRename_Click(object sender, EventArgs e)
        {
            string strRename = await LanguageManager.GetStringAsync("Message_CharacterOptions_SettingRename")
                                                    .ConfigureAwait(false);
            using (ThreadSafeForm<SelectText> frmSelectName = await ThreadSafeForm<SelectText>.GetAsync(
                       () => new SelectText
                       {
                           DefaultString = _objCharacterSettings.Name,
                           Description = strRename
                       }).ConfigureAwait(false))
            {
                if (await frmSelectName.ShowDialogSafeAsync(this).ConfigureAwait(false) != DialogResult.OK)
                    return;
                _objCharacterSettings.Name = frmSelectName.MyForm.SelectedValue;
            }

            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    int intCurrentSelectedSettingIndex
                        = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false);
                    if (intCurrentSelectedSettingIndex >= 0)
                    {
                        ListItem objNewListItem = new ListItem(_lstSettings[intCurrentSelectedSettingIndex].Value,
                                                               await _objCharacterSettings.GetCurrentDisplayNameAsync().ConfigureAwait(false));
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            _lstSettings[intCurrentSelectedSettingIndex] = objNewListItem;
                            await cboSetting.PopulateWithListItemsAsync(_lstSettings).ConfigureAwait(false);
                            await cboSetting.DoThreadSafeAsync(x => x.SelectedIndex = intCurrentSelectedSettingIndex)
                                            .ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }
                    }

                    _blnWasRenamed = true;
                    await SetIsDirtyAsync(true).ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }

                _intOldSelectedSettingIndex
                    = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to delete this setting
            if (Program.ShowScrollableMessageBox(
                    string.Format(GlobalSettings.CultureInfo,
                                  await LanguageManager.GetStringAsync("Message_CharacterOptions_ConfirmDelete")
                                                       .ConfigureAwait(false),
                                  _objReferenceCharacterSettings.Name),
                    await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_ConfirmDelete")
                                         .ConfigureAwait(false),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                LockingDictionary<string, CharacterSettings> dicCharacterSettings
                    = await SettingsManager.GetLoadedCharacterSettingsAsModifiableAsync().ConfigureAwait(false);
                (bool blnSuccess, CharacterSettings objDeletedSettings)
                    = await dicCharacterSettings.TryRemoveAsync(
                                                    await _objReferenceCharacterSettings.GetDictionaryKeyAsync()
                                                        .ConfigureAwait(false))
                                                .ConfigureAwait(false);
                if (!blnSuccess)
                    return;
                try
                {
                    if (!await Utils.SafeDeleteFileAsync(
                                        Path.Combine(Utils.GetStartupPath, "settings",
                                                     _objReferenceCharacterSettings.FileName), true)
                                    .ConfigureAwait(false))
                    {
                        // Revert removal of setting if we cannot delete the file
                        await dicCharacterSettings.AddAsync(
                                                      await objDeletedSettings.GetDictionaryKeyAsync()
                                                                              .ConfigureAwait(false),
                                                      objDeletedSettings)
                                                  .ConfigureAwait(false);
                        return;
                    }
                }
                catch
                {
                    // Revert removal of setting if we cannot delete the file
                    await dicCharacterSettings.AddAsync(
                                                  await objDeletedSettings.GetDictionaryKeyAsync()
                                                                          .ConfigureAwait(false), objDeletedSettings)
                                              .ConfigureAwait(false);
                    throw;
                }

                // Force repopulate character settings list in Master Index from here in lieu of event handling for concurrent dictionaries
                _blnForceMasterIndexRepopulateOnClose = true;
                KeyValuePair<string, CharacterSettings> kvpReplacementOption
                    = await dicCharacterSettings.FirstOrDefaultAsync(
                                                    x => x.Value.BuiltInOption
                                                         && x.Value.BuildMethod
                                                         == _objReferenceCharacterSettings.BuildMethod)
                                                .ConfigureAwait(false);
                await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                {
                    if (await objCharacter.GetSettingsKeyAsync().ConfigureAwait(false)
                        == _objReferenceCharacterSettings.FileName)
                        await objCharacter.SetSettingsKeyAsync(kvpReplacementOption.Key).ConfigureAwait(false);
                }).ConfigureAwait(false);

                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    _objReferenceCharacterSettings = kvpReplacementOption.Value;
                    await _objCharacterSettings.CopyValuesAsync(_objReferenceCharacterSettings).ConfigureAwait(false);
                    await RebuildCustomDataDirectoryInfosAsync().ConfigureAwait(false);
                    await SetIsDirtyAsync(false).ConfigureAwait(false);
                    await PopulateSettingsList().ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdSaveAs_Click(object sender, EventArgs e)
        {
            string strSelectedName;
            string strSelectedFullFileName;
            string strSelectSettingName
                = await LanguageManager.GetStringAsync("Message_CharacterOptions_SelectSettingName")
                                       .ConfigureAwait(false);
            LockingDictionary<string, CharacterSettings> dicCharacterSettings
                = await SettingsManager.GetLoadedCharacterSettingsAsModifiableAsync().ConfigureAwait(false);
            do
            {
                do
                {
                    using (ThreadSafeForm<SelectText> frmSelectName = await ThreadSafeForm<SelectText>.GetAsync(
                               () => new SelectText
                               {
                                   DefaultString = _objCharacterSettings.BuiltInOption
                                       ? string.Empty
                                       : _objCharacterSettings.FileName.TrimEndOnce(".xml"),
                                   Description = strSelectSettingName
                               }).ConfigureAwait(false))
                    {
                        if (await frmSelectName.ShowDialogSafeAsync(this).ConfigureAwait(false) != DialogResult.OK)
                            return;
                        strSelectedName = frmSelectName.MyForm.SelectedValue;
                    }

                    if (dicCharacterSettings.Any(x => x.Value.Name == strSelectedName))
                    {
                        DialogResult eCreateDuplicateSetting = Program.ShowScrollableMessageBox(
                            string.Format(
                                await LanguageManager.GetStringAsync("Message_CharacterOptions_DuplicateSettingName")
                                                     .ConfigureAwait(false),
                                strSelectedName),
                            await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_DuplicateFileName")
                                                 .ConfigureAwait(false),
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        switch (eCreateDuplicateSetting)
                        {
                            case DialogResult.Cancel:
                                return;

                            case DialogResult.No:
                                strSelectedName = string.Empty;
                                break;
                        }
                    }
                } while (string.IsNullOrWhiteSpace(strSelectedName));

                string strBaseFileName = strSelectedName.CleanForFileName().TrimEndOnce(".xml");
                // Make sure our file name isn't too long, otherwise we run into problems on Windows
                // We can assume that Chummer's startup path plus 16 is within the limit, otherwise the user would have had problems installing Chummer with its data files in the first place
                int intStartupPathLimit = Utils.GetStartupPath.Length + 16;
                if (strBaseFileName.Length > intStartupPathLimit)
                    strBaseFileName = strBaseFileName.Substring(0, intStartupPathLimit);
                strSelectedFullFileName = strBaseFileName + ".xml";
                int intMaxNameLength = char.MaxValue - Utils.GetStartupPath.Length - "settings".Length - 6;
                uint uintAccumulator = 1;
                string strSeparator = "_";
                while (dicCharacterSettings.Any(x => x.Value.FileName == strSelectedFullFileName))
                {
                    strSelectedFullFileName = strBaseFileName + strSeparator
                                                              + uintAccumulator.ToString(
                                                                  GlobalSettings.InvariantCultureInfo) + ".xml";
                    if (strSelectedFullFileName.Length > intMaxNameLength)
                    {
                        Program.ShowScrollableMessageBox(
                            await LanguageManager.GetStringAsync("Message_CharacterOptions_SettingFileNameTooLongError")
                                                 .ConfigureAwait(false),
                            await LanguageManager
                                  .GetStringAsync("MessageTitle_CharacterOptions_SettingFileNameTooLongError")
                                  .ConfigureAwait(false),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        strSelectedName = string.Empty;
                        break;
                    }

                    if (uintAccumulator == uint.MaxValue)
                        uintAccumulator = uint.MinValue;
                    else if (++uintAccumulator == 1)
                        strSeparator += '_';
                }
            } while (string.IsNullOrWhiteSpace(strSelectedName));

            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                _objCharacterSettings.Name = strSelectedName;
                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    CharacterSettings objNewCharacterSettings
                        = new CharacterSettings(_objCharacterSettings, false, strSelectedFullFileName);
                    string strKey = await objNewCharacterSettings.GetDictionaryKeyAsync().ConfigureAwait(false);
                    if (!await dicCharacterSettings.TryAddAsync(strKey, objNewCharacterSettings).ConfigureAwait(false))
                    {
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        return;
                    }

                    bool blnSaveSuccessful;
                    try
                    {
                        blnSaveSuccessful = await _objCharacterSettings.SaveAsync(strSelectedFullFileName, true).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Revert addition of settings if we cannot create a file
                        await dicCharacterSettings.RemoveAsync(strKey).ConfigureAwait(false);
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                    if (!blnSaveSuccessful)
                    {
                        // Revert addition of settings if we cannot create a file
                        await dicCharacterSettings.RemoveAsync(strKey).ConfigureAwait(false);
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        return;
                    }

                    // Force repopulate character settings list in Master Index from here in lieu of event handling for concurrent dictionaries
                    _blnForceMasterIndexRepopulateOnClose = true;
                    _objReferenceCharacterSettings = objNewCharacterSettings;
                    await SetIsDirtyAsync(false).ConfigureAwait(false);
                    await PopulateSettingsList().ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdSave_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                if (_objReferenceCharacterSettings.BuildMethod != _objCharacterSettings.BuildMethod)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdConflictingCharacters))
                    {
                        foreach (Character objCharacter in Program.OpenCharacters)
                        {
                            if (!objCharacter.Created
                                && ReferenceEquals(objCharacter.Settings, _objReferenceCharacterSettings))
                                sbdConflictingCharacters.AppendLine(objCharacter.CharacterName);
                        }

                        if (sbdConflictingCharacters.Length > 0)
                        {
                            Program.ShowScrollableMessageBox(this,
                                                             await LanguageManager.GetStringAsync(
                                                                     "Message_CharacterOptions_OpenCharacterOnBuildMethodChange")
                                                                 .ConfigureAwait(false)
                                                             +
                                                             sbdConflictingCharacters,
                                                             await LanguageManager.GetStringAsync(
                                                                     "MessageTitle_CharacterOptions_OpenCharacterOnBuildMethodChange")
                                                                 .ConfigureAwait(false),
                                                             MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                if (!await _objCharacterSettings.SaveAsync().ConfigureAwait(false))
                    return;
                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    await _objReferenceCharacterSettings.CopyValuesAsync(_objCharacterSettings).ConfigureAwait(false);
                    await SetIsDirtyAsync(false).ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            string strSelectedFile = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                                     .ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedFile))
                return;
            (bool blnSuccess, CharacterSettings objNewOption)
                = await (await SettingsManager.GetLoadedCharacterSettingsAsync().ConfigureAwait(false))
                        .TryGetValueAsync(strSelectedFile).ConfigureAwait(false);
            if (!blnSuccess)
                return;

            if (IsDirty)
            {
                string text = await LanguageManager.GetStringAsync("Message_CharacterOptions_UnsavedDirty")
                                                   .ConfigureAwait(false);
                string caption = await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_UnsavedDirty")
                                                      .ConfigureAwait(false);

                if (Program.ShowScrollableMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                {
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex = _intOldSelectedSettingIndex)
                                        .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }

                    return;
                }

                await SetIsDirtyAsync(false).ConfigureAwait(false);
            }

            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                        await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                    try
                    {
                        if (_blnWasRenamed && _intOldSelectedSettingIndex >= 0)
                        {
                            int intCurrentSelectedSettingIndex
                                = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false);
                            ListItem objNewListItem =
                                new ListItem(_lstSettings[_intOldSelectedSettingIndex].Value,
                                             await _objReferenceCharacterSettings.GetCurrentDisplayNameAsync().ConfigureAwait(false));
                            _lstSettings[_intOldSelectedSettingIndex] = objNewListItem;
                            await cboSetting.PopulateWithListItemsAsync(_lstSettings).ConfigureAwait(false);
                            await cboSetting.DoThreadSafeAsync(x => x.SelectedIndex = intCurrentSelectedSettingIndex)
                                            .ConfigureAwait(false);
                        }

                        _objReferenceCharacterSettings = objNewOption;
                        await _objCharacterSettings.CopyValuesAsync(objNewOption).ConfigureAwait(false);
                        await RebuildCustomDataDirectoryInfosAsync().ConfigureAwait(false);
                        await PopulateOptions().ConfigureAwait(false);
                        await SetIsDirtyAsync(false).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                            await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }

                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdRestoreDefaults_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if (Program.ShowScrollableMessageBox(
                    await LanguageManager.GetStringAsync("Message_Options_RestoreDefaults").ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_Options_RestoreDefaults").ConfigureAwait(false),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                        await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                    try
                    {
                        int intCurrentSelectedSettingIndex
                            = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false);
                        if (_blnWasRenamed && intCurrentSelectedSettingIndex >= 0)
                        {
                            ListItem objNewListItem =
                                new ListItem(_lstSettings[intCurrentSelectedSettingIndex].Value,
                                             await _objReferenceCharacterSettings.GetCurrentDisplayNameAsync().ConfigureAwait(false));
                            _lstSettings[intCurrentSelectedSettingIndex] = objNewListItem;
                            await cboSetting.PopulateWithListItemsAsync(_lstSettings).ConfigureAwait(false);
                            await cboSetting.DoThreadSafeAsync(x => x.SelectedIndex = intCurrentSelectedSettingIndex)
                                            .ConfigureAwait(false);
                        }

                        await _objCharacterSettings.CopyValuesAsync(_objReferenceCharacterSettings)
                                                   .ConfigureAwait(false);
                        await RebuildCustomDataDirectoryInfosAsync().ConfigureAwait(false);
                        await PopulateOptions().ConfigureAwait(false);
                        await SetIsDirtyAsync(false).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                            await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }

                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void cboLimbCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intSkipLimbCountUpdate > 0)
                return;

            string strLimbCount = cboLimbCount.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strLimbCount))
            {
                _objCharacterSettings.LimbCount = 6;
                _objCharacterSettings.ExcludeLimbSlot = string.Empty;
            }
            else
            {
                int intSeparatorIndex = strLimbCount.IndexOf('<');
                if (intSeparatorIndex == -1)
                {
                    if (int.TryParse(strLimbCount, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                     out int intLimbCount))
                        _objCharacterSettings.LimbCount = intLimbCount;
                    else
                    {
                        Utils.BreakIfDebug();
                        _objCharacterSettings.LimbCount = 6;
                    }

                    _objCharacterSettings.ExcludeLimbSlot = string.Empty;
                }
                else
                {
                    if (int.TryParse(strLimbCount.Substring(0, intSeparatorIndex), NumberStyles.Any,
                                     GlobalSettings.InvariantCultureInfo, out int intLimbCount))
                    {
                        _objCharacterSettings.LimbCount = intLimbCount;
                        _objCharacterSettings.ExcludeLimbSlot = intSeparatorIndex + 1 < strLimbCount.Length
                            ? strLimbCount.Substring(intSeparatorIndex + 1)
                            : string.Empty;
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                        _objCharacterSettings.LimbCount = 6;
                        _objCharacterSettings.ExcludeLimbSlot = string.Empty;
                    }
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void EditCharacterSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsDirty && Program.ShowScrollableMessageBox(
                    await LanguageManager.GetStringAsync("Message_CharacterOptions_UnsavedDirty").ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_UnsavedDirty")
                                         .ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes)
            {
                e.Cancel = true;
            }

            if (_blnForceMasterIndexRepopulateOnClose && Program.MainForm.MasterIndex != null)
            {
                await Program.MainForm.MasterIndex.ForceRepopulateCharacterSettings().ConfigureAwait(false);
            }
        }

        private void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _intLoading);
            try
            {
                foreach (TreeNode objNode in treSourcebook.Nodes)
                {
                    string strBookCode = objNode.Tag.ToString();
                    if (!_setPermanentSourcebooks.Contains(strBookCode))
                    {
                        objNode.Checked = _blnSourcebookToggle;
                        if (_blnSourcebookToggle)
                            _objCharacterSettings.BooksWritable.Add(strBookCode);
                        else
                            _objCharacterSettings.BooksWritable.Remove(strBookCode);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref _intLoading);
            }

            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.Books));
            _blnSourcebookToggle = !_blnSourcebookToggle;
        }

        private void treSourcebook_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_intLoading > 0)
                return;
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            string strBookCode = objNode.Tag.ToString();
            if (string.IsNullOrEmpty(strBookCode)
                || (_setPermanentSourcebooks.Contains(strBookCode) && !objNode.Checked))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    objNode.Checked = !objNode.Checked;
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }

                return;
            }

            if (objNode.Checked)
                _objCharacterSettings.BooksWritable.Add(strBookCode);
            else
                _objCharacterSettings.BooksWritable.Remove(strBookCode);
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.Books));
        }

        private async void cmdIncreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedNode)
                                                                 .ConfigureAwait(false);
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            _dicCharacterCustomDataDirectoryInfos.Reverse(intIndex - 1, 2);
            _objCharacterSettings.CustomDataDirectoryKeys.Reverse(intIndex - 1, 2);
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private async void cmdToTopCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedNode)
                                                                 .ConfigureAwait(false);
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            for (int i = intIndex; i > 0; --i)
            {
                _dicCharacterCustomDataDirectoryInfos.Reverse(i - 1, 2);
                _objCharacterSettings.CustomDataDirectoryKeys.Reverse(i - 1, 2);
            }

            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private async void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedNode)
                                                                 .ConfigureAwait(false);
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _dicCharacterCustomDataDirectoryInfos.Count - 1)
                return;
            _dicCharacterCustomDataDirectoryInfos.Reverse(intIndex, 2);
            _objCharacterSettings.CustomDataDirectoryKeys.Reverse(intIndex, 2);
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private async void cmdToBottomCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedNode)
                                                                 .ConfigureAwait(false);
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _dicCharacterCustomDataDirectoryInfos.Count - 1)
                return;
            for (int i = intIndex; i < _dicCharacterCustomDataDirectoryInfos.Count - 1; ++i)
            {
                _dicCharacterCustomDataDirectoryInfos.Reverse(i, 2);
                _objCharacterSettings.CustomDataDirectoryKeys.Reverse(i, 2);
            }

            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private void treCustomDataDirectories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_intLoading > 0)
                return;
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            int intIndex = objNode.Index;
            object objKey = _dicCharacterCustomDataDirectoryInfos[intIndex].Key;
            _dicCharacterCustomDataDirectoryInfos[objKey] = objNode.Checked;
            switch (objNode.Tag)
            {
                case CustomDataDirectoryInfo objCustomDataDirectoryInfo
                    when _objCharacterSettings.CustomDataDirectoryKeys.ContainsKey(
                        objCustomDataDirectoryInfo.CharacterSettingsSaveKey):
                    _objCharacterSettings.CustomDataDirectoryKeys[objCustomDataDirectoryInfo.CharacterSettingsSaveKey]
                        = objNode.Checked;
                    _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
                    break;
                case string strCustomDataDirectoryKey
                    when _objCharacterSettings.CustomDataDirectoryKeys.ContainsKey(strCustomDataDirectoryKey):
                    _objCharacterSettings.CustomDataDirectoryKeys[strCustomDataDirectoryKey] = objNode.Checked;
                    _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
                    break;
            }
        }

        private void txtPriorities_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar)
                        && e.KeyChar != 'A' && e.KeyChar != 'B' && e.KeyChar != 'C' && e.KeyChar != 'D'
                        && e.KeyChar != 'E'
                        && e.KeyChar != 'a' && e.KeyChar != 'b' && e.KeyChar != 'c' && e.KeyChar != 'd'
                        && e.KeyChar != 'e';
            switch (e.KeyChar)
            {
                case 'a':
                    e.KeyChar = 'A';
                    break;

                case 'b':
                    e.KeyChar = 'B';
                    break;

                case 'c':
                    e.KeyChar = 'C';
                    break;

                case 'd':
                    e.KeyChar = 'D';
                    break;

                case 'e':
                    e.KeyChar = 'E';
                    break;
            }
        }

        private async void txtPriorities_TextChanged(object sender, EventArgs e)
        {
            Color objWindowTextColor = await ColorManager.GetWindowTextAsync().ConfigureAwait(false);
            await txtPriorities.DoThreadSafeAsync(x => x.ForeColor
                                                      = x.TextLength == 5
                                                          ? objWindowTextColor
                                                          : ColorManager.ErrorColor).ConfigureAwait(false);
        }

        private async void txtContactPoints_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                           await txtContactPoints.DoThreadSafeFuncAsync(x => x.Text)
                                                                 .ConfigureAwait(false))
                                       .ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtContactPoints.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtKnowledgePoints_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                           await txtKnowledgePoints.DoThreadSafeFuncAsync(x => x.Text)
                                                                   .ConfigureAwait(false))
                                       .ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtKnowledgePoints.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtNuyenExpression_TextChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            string strText = await txtNuyenExpression.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(strText.Replace("{Karma}", "1")
                    .Replace("{PriorityNuyen}", "1")).ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtNuyenExpression.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
            await _objCharacterSettings.SetChargenKarmaToNuyenExpressionAsync(strText)
                                       .ConfigureAwait(false); // Not data-bound so that the setter can be asynchronous
        }

        private async void txtBoundSpiritLimit_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                           await txtBoundSpiritLimit.DoThreadSafeFuncAsync(x => x.Text)
                                                                    .ConfigureAwait(false))
                                       .ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtBoundSpiritLimit.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtRegisteredSpriteLimit_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                           await txtRegisteredSpriteLimit.DoThreadSafeFuncAsync(x => x.Text)
                                                                         .ConfigureAwait(false))
                                       .ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtRegisteredSpriteLimit.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtEssenceModifierPostExpression_TextChanged(object sender, EventArgs e)
        {
            string strText = await txtEssenceModifierPostExpression.DoThreadSafeFuncAsync(x => x.Text)
                                                                   .ConfigureAwait(false);
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                    strText.Replace("{Modifier}", "1.0")).ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtEssenceModifierPostExpression.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtLiftLimit_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                    await txtLiftLimit.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtLiftLimit.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtCarryLimit_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                    await txtCarryLimit.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtCarryLimit.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtEncumbranceInterval_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                           await txtEncumbranceInterval.DoThreadSafeFuncAsync(x => x.Text)
                                                                       .ConfigureAwait(false))
                                       .ConfigureAwait(false)
                    ? await ColorManager.GetWindowTextAsync().ConfigureAwait(false)
                    : ColorManager.ErrorColor;
            await txtEncumbranceInterval.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private void chkGrade_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox chkGrade))
                return;

            string strGrade = chkGrade.Tag.ToString();
            if (chkGrade.Checked)
            {
                if (_objCharacterSettings.BannedWareGrades.Contains(strGrade))
                {
                    _objCharacterSettings.BannedWareGrades.Remove(strGrade);
                    _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.BannedWareGrades));
                }
            }
            else if (!_objCharacterSettings.BannedWareGrades.Contains(strGrade))
            {
                _objCharacterSettings.BannedWareGrades.Add(strGrade);
                _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.BannedWareGrades));
            }
        }

        private void cboPriorityTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            string strNewPriorityTable = cboPriorityTable.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(strNewPriorityTable))
                return;
            _objCharacterSettings.PriorityTable = strNewPriorityTable;
        }

        private async void treCustomDataDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node?.Tag is CustomDataDirectoryInfo objSelected))
            {
                await gpbDirectoryInfo.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            await gpbDirectoryInfo.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                string strDescription = await objSelected.GetDisplayDescriptionAsync(GlobalSettings.Language)
                                                         .ConfigureAwait(false);
                await rtbDirectoryDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
                await lblDirectoryVersion.DoThreadSafeAsync(x => x.Text = objSelected.MyVersion.ToString())
                                         .ConfigureAwait(false);
                string strAuthors = await objSelected
                                          .GetDisplayAuthorsAsync(GlobalSettings.Language, GlobalSettings.CultureInfo)
                                          .ConfigureAwait(false);
                await lblDirectoryAuthors.DoThreadSafeAsync(x => x.Text = strAuthors).ConfigureAwait(false);
                string strName = await objSelected.GetDisplayNameAsync().ConfigureAwait(false);
                await lblDirectoryName.DoThreadSafeAsync(x => x.Text = strName).ConfigureAwait(false);

                if (objSelected.DependenciesList.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDependencies))
                    {
                        foreach (DirectoryDependency dependency in objSelected.DependenciesList)
                            sbdDependencies.AppendLine(await dependency.GetDisplayNameAsync().ConfigureAwait(false));
                        await lblDependencies.DoThreadSafeAsync(x => x.Text = sbdDependencies.ToString())
                                             .ConfigureAwait(false);
                    }
                }
                else
                {
                    //Make sure all old information is discarded
                    await lblDependencies.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                }

                if (objSelected.IncompatibilitiesList.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdIncompatibilities))
                    {
                        foreach (DirectoryDependency exclusivity in objSelected.IncompatibilitiesList)
                            sbdIncompatibilities.AppendLine(
                                await exclusivity.GetDisplayNameAsync().ConfigureAwait(false));
                        await lblIncompatibilities.DoThreadSafeAsync(x => x.Text = sbdIncompatibilities.ToString())
                                                  .ConfigureAwait(false);
                    }
                }
                else
                {
                    //Make sure all old information is discarded
                    await lblIncompatibilities.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                }

                await gpbDirectoryInfo.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }
            finally
            {
                await gpbDirectoryInfo.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
            }
        }

        #endregion Control Events

        #region Methods

        private async ValueTask PopulateSourcebookTreeView(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Load the Sourcebook information.
            // Put the Sourcebooks into a List so they can first be sorted.
            object objOldSelected = await treSourcebook.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                                                       .ConfigureAwait(false);
            await treSourcebook.DoThreadSafeAsync(x => x.BeginUpdate(), token).ConfigureAwait(false);
            try
            {
                await treSourcebook.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);
                _setPermanentSourcebooks.Clear();
                foreach (XPathNavigator objXmlBook in await (await XmlManager.LoadXPathAsync(
                                                                "books.xml",
                                                                _objCharacterSettings.EnabledCustomDataDirectoryPaths,
                                                                token: token).ConfigureAwait(false))
                                                            .SelectAndCacheExpressionAsync(
                                                                "/chummer/books/book", token: token)
                                                            .ConfigureAwait(false))
                {
                    if (await objXmlBook.SelectSingleNodeAndCacheExpressionAsync("hide", token: token)
                                        .ConfigureAwait(false) != null)
                        continue;
                    string strCode = (await objXmlBook.SelectSingleNodeAndCacheExpressionAsync("code", token: token)
                                                      .ConfigureAwait(false))?.Value;
                    if (string.IsNullOrEmpty(strCode))
                        continue;
                    bool blnChecked = _objCharacterSettings.Books.Contains(strCode);
                    if (await objXmlBook.SelectSingleNodeAndCacheExpressionAsync("permanent", token: token)
                                        .ConfigureAwait(false) != null)
                    {
                        _setPermanentSourcebooks.Add(strCode);
                        if (_objCharacterSettings.BooksWritable.Add(strCode))
                            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.Books));
                        blnChecked = true;
                    }

                    string strTranslate
                        = (await objXmlBook.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                           .ConfigureAwait(false))?.Value;
                    string strName = (await objXmlBook.SelectSingleNodeAndCacheExpressionAsync("name", token: token)
                                                      .ConfigureAwait(false))?.Value;
                    await treSourcebook.DoThreadSafeAsync(x =>
                    {
                        TreeNode objNode = new TreeNode
                        {
                            Text = strTranslate ?? strName ?? string.Empty,
                            Tag = strCode,
                            Checked = blnChecked
                        };
                        x.Nodes.Add(objNode);
                    }, token).ConfigureAwait(false);
                }

                await treSourcebook.DoThreadSafeAsync(x =>
                {
                    x.Sort();
                    if (objOldSelected != null)
                        x.SelectedNode = x.FindNodeByTag(objOldSelected);
                }, token).ConfigureAwait(false);
            }
            finally
            {
                await treSourcebook.DoThreadSafeAsync(x => x.EndUpdate(), token).ConfigureAwait(false);
            }
        }

        private async ValueTask PopulateCustomDataDirectoryTreeView(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            object objOldSelected = await treCustomDataDirectories
                                          .DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false);
            await treCustomDataDirectories.DoThreadSafeAsync(x => x.BeginUpdate(), token).ConfigureAwait(false);
            try
            {
                string strFileNotFound = await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                                                              .ConfigureAwait(false);
                Color objGrayTextColor = await ColorManager.GetGrayTextAsync(token).ConfigureAwait(false);
                if (_dicCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
                {
                    List<TreeNode> lstNodes = new List<TreeNode>(_dicCharacterCustomDataDirectoryInfos.Count);
                    foreach (KeyValuePair<object, bool> kvpInfo in _dicCharacterCustomDataDirectoryInfos.OfType<KeyValuePair<object, bool>>())
                    {
                        token.ThrowIfCancellationRequested();
                        TreeNode objNode = new TreeNode
                        {
                            Tag = kvpInfo.Key,
                            Checked = kvpInfo.Value
                        };
                        if (kvpInfo.Key is CustomDataDirectoryInfo objInfo)
                        {
                            objNode.Text = objInfo.DisplayName;
                            if (objNode.Checked)
                            {
                                // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                                string missingDirectories = string.Empty;
                                if (objInfo.DependenciesList.Count > 0)
                                    missingDirectories = await objInfo
                                                               .CheckDependencyAsync(_objCharacterSettings, token)
                                                               .ConfigureAwait(false);

                                string prohibitedDirectories = string.Empty;
                                if (objInfo.IncompatibilitiesList.Count > 0)
                                    prohibitedDirectories = await objInfo
                                                                  .CheckIncompatibilityAsync(
                                                                      _objCharacterSettings, token)
                                                                  .ConfigureAwait(false);

                                if (!string.IsNullOrEmpty(missingDirectories)
                                    || !string.IsNullOrEmpty(prohibitedDirectories))
                                {
                                    objNode.ToolTipText
                                        = await CustomDataDirectoryInfo.BuildIncompatibilityDependencyStringAsync(
                                            missingDirectories, prohibitedDirectories, token).ConfigureAwait(false);
                                    objNode.ForeColor = ColorManager.ErrorColor;
                                }
                            }
                        }
                        else
                        {
                            objNode.Text = kvpInfo.Key.ToString();
                            objNode.ForeColor = objGrayTextColor;
                            objNode.ToolTipText = strFileNotFound;
                        }

                        lstNodes.Add(objNode);
                    }

                    await treCustomDataDirectories.DoThreadSafeAsync(x =>
                    {
                        x.Nodes.Clear();
                        foreach (TreeNode objNode in lstNodes)
                            x.Nodes.Add(objNode);
                    }, token).ConfigureAwait(false);
                }
                else
                {
                    Color objWindowTextColor = await ColorManager.GetWindowTextAsync(token).ConfigureAwait(false);
                    for (int i = 0; i < _dicCharacterCustomDataDirectoryInfos.Count; ++i)
                    {
                        KeyValuePair<object, bool> kvpInfo = _dicCharacterCustomDataDirectoryInfos[i];
                        int i1 = i;
                        TreeNode objNode = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.Nodes[i1], token)
                                                                         .ConfigureAwait(false);
                        await treCustomDataDirectories.DoThreadSafeAsync(() =>
                        {
                            objNode.Tag = kvpInfo.Key;
                            objNode.Checked = kvpInfo.Value;
                        }, token: token).ConfigureAwait(false);
                        if (kvpInfo.Key is CustomDataDirectoryInfo objInfo)
                        {
                            string strText = await objInfo.GetDisplayNameAsync(token).ConfigureAwait(false);
                            await treCustomDataDirectories.DoThreadSafeAsync(() => objNode.Text = strText, token)
                                                          .ConfigureAwait(false);
                            if (objNode.Checked)
                            {
                                // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                                string missingDirectories = string.Empty;
                                if (objInfo.DependenciesList.Count > 0)
                                    missingDirectories = await objInfo
                                                               .CheckDependencyAsync(_objCharacterSettings, token)
                                                               .ConfigureAwait(false);

                                string prohibitedDirectories = string.Empty;
                                if (objInfo.IncompatibilitiesList.Count > 0)
                                    prohibitedDirectories = await objInfo
                                                                  .CheckIncompatibilityAsync(
                                                                      _objCharacterSettings, token)
                                                                  .ConfigureAwait(false);

                                if (!string.IsNullOrEmpty(missingDirectories)
                                    || !string.IsNullOrEmpty(prohibitedDirectories))
                                {
                                    string strToolTip
                                        = await CustomDataDirectoryInfo.BuildIncompatibilityDependencyStringAsync(
                                            missingDirectories, prohibitedDirectories, token).ConfigureAwait(false);
                                    await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                    {
                                        objNode.ToolTipText = strToolTip;
                                        objNode.ForeColor = ColorManager.ErrorColor;
                                    }, token: token).ConfigureAwait(false);
                                }
                                else
                                {
                                    await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                    {
                                        objNode.ToolTipText = string.Empty;
                                        objNode.ForeColor = objWindowTextColor;
                                    }, token: token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                {
                                    objNode.ToolTipText = string.Empty;
                                    objNode.ForeColor = objWindowTextColor;
                                }, token: token).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await treCustomDataDirectories.DoThreadSafeAsync(() =>
                            {
                                objNode.Text = kvpInfo.Key.ToString();
                                objNode.ForeColor = objGrayTextColor;
                                objNode.ToolTipText = strFileNotFound;
                            }, token: token).ConfigureAwait(false);
                        }
                    }

                    if (objOldSelected != null)
                    {
                        await treCustomDataDirectories.DoThreadSafeAsync(x =>
                        {
                            x.SelectedNode = x.FindNodeByTag(objOldSelected);
                            x.ShowNodeToolTips = true;
                        }, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await treCustomDataDirectories.DoThreadSafeAsync(x => x.ShowNodeToolTips = true, token)
                                                      .ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await treCustomDataDirectories.DoThreadSafeAsync(x => x.EndUpdate(), token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private async ValueTask PopulateOptions(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                try
                {
                    await PopulateSourcebookTreeView(token).ConfigureAwait(false);
                    await PopulatePriorityTableList(token).ConfigureAwait(false);
                    await PopulateLimbCountList(token).ConfigureAwait(false);
                    await PopulateAllowedGrades(token).ConfigureAwait(false);
                    await PopulateCustomDataDirectoryTreeView(token).ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), CancellationToken.None).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask PopulatePriorityTableList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstPriorityTables))
                {
                    foreach (XPathNavigator objXmlNode in await (await XmlManager
                                                                       .LoadXPathAsync("priorities.xml",
                                                                           _objCharacterSettings
                                                                               .EnabledCustomDataDirectoryPaths,
                                                                           token: token).ConfigureAwait(false))
                                                                .SelectAndCacheExpressionAsync(
                                                                    "/chummer/prioritytables/prioritytable",
                                                                    token: token).ConfigureAwait(false))
                    {
                        string strName = objXmlNode.Value;
                        if (!string.IsNullOrEmpty(strName))
                            lstPriorityTables.Add(new ListItem(objXmlNode.Value,
                                                               (await objXmlNode
                                                                      .SelectSingleNodeAndCacheExpressionAsync(
                                                                          "@translate", token: token)
                                                                      .ConfigureAwait(false))
                                                               ?.Value ?? strName));
                    }

                    string strOldSelected = _objCharacterSettings.PriorityTable;

                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await cboPriorityTable.PopulateWithListItemsAsync(lstPriorityTables, token)
                                              .ConfigureAwait(false);
                        await cboPriorityTable.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strOldSelected))
                                x.SelectedValue = strOldSelected;
                            if (x.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                                x.SelectedValue = _objReferenceCharacterSettings.PriorityTable;
                            if (x.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                                x.SelectedIndex = 0;
                        }, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                }

                string strSelectedTable
                    = await cboPriorityTable.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                            .ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(strSelectedTable) &&
                    _objCharacterSettings.PriorityTable != strSelectedTable)
                    _objCharacterSettings.PriorityTable = strSelectedTable;
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask PopulateLimbCountList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                Interlocked.Increment(ref _intSkipLimbCountUpdate);
                try
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstLimbCount))
                    {
                        foreach (XPathNavigator objXmlNode in await (await XmlManager
                                                                           .LoadXPathAsync("options.xml",
                                                                               _objCharacterSettings
                                                                                   .EnabledCustomDataDirectoryPaths,
                                                                               token: token).ConfigureAwait(false))
                                                                    .SelectAndCacheExpressionAsync(
                                                                        "/chummer/limbcounts/limb", token: token)
                                                                    .ConfigureAwait(false))
                        {
                            string strExclude
                                = (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("exclude", token: token)
                                                   .ConfigureAwait(false))?.Value
                                  ??
                                  string.Empty;
                            if (!string.IsNullOrEmpty(strExclude))
                                strExclude = '<' + strExclude;
                            lstLimbCount.Add(new ListItem(
                                                 (await objXmlNode
                                                        .SelectSingleNodeAndCacheExpressionAsync(
                                                            "limbcount", token: token).ConfigureAwait(false))
                                                 ?.Value + strExclude,
                                                 (await objXmlNode
                                                        .SelectSingleNodeAndCacheExpressionAsync(
                                                            "translate", token: token).ConfigureAwait(false))
                                                 ?.Value
                                                 ?? (await objXmlNode
                                                           .SelectSingleNodeAndCacheExpressionAsync(
                                                               "name", token: token).ConfigureAwait(false))
                                                 ?.Value
                                                 ?? string.Empty));
                        }

                        string strLimbSlot
                            = _objCharacterSettings.LimbCount.ToString(GlobalSettings.InvariantCultureInfo);
                        if (!string.IsNullOrEmpty(_objCharacterSettings.ExcludeLimbSlot))
                            strLimbSlot += '<' + _objCharacterSettings.ExcludeLimbSlot;

                        await cboLimbCount.PopulateWithListItemsAsync(lstLimbCount, token).ConfigureAwait(false);
                        await cboLimbCount.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strLimbSlot))
                                x.SelectedValue = strLimbSlot;
                            if (x.SelectedIndex == -1 && lstLimbCount.Count > 0)
                                x.SelectedIndex = 0;
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipLimbCountUpdate);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask PopulateAllowedGrades(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstGrades))
                {
                    foreach (XPathNavigator objXmlNode in await (await XmlManager
                                                                       .LoadXPathAsync("bioware.xml",
                                                                           _objCharacterSettings
                                                                               .EnabledCustomDataDirectoryPaths,
                                                                           token: token).ConfigureAwait(false))
                                                                .SelectAndCacheExpressionAsync(
                                                                    "/chummer/grades/grade[not(hide)]", token: token)
                                                                .ConfigureAwait(false))
                    {
                        string strName = (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name", token: token)
                                                          .ConfigureAwait(false))?.Value;
                        if (!string.IsNullOrEmpty(strName) && strName != "None")
                        {
                            string strBook = (await objXmlNode
                                                    .SelectSingleNodeAndCacheExpressionAsync("source", token: token)
                                                    .ConfigureAwait(false))
                                ?.Value;
                            if (!string.IsNullOrEmpty(strBook)
                                && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                                continue;
                            if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                                continue;
                            ListItem objExistingCoveredGrade =
                                lstGrades.Find(x => x.Value.ToString().Contains(strName));
                            if (objExistingCoveredGrade.Value != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(
                                              strName,
                                              (await objXmlNode
                                                     .SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                                     .ConfigureAwait(false))
                                              ?.Value
                                              ?? strName));
                        }
                    }

                    foreach (XPathNavigator objXmlNode in await (await XmlManager
                                                                       .LoadXPathAsync("cyberware.xml",
                                                                           _objCharacterSettings
                                                                               .EnabledCustomDataDirectoryPaths,
                                                                           token: token).ConfigureAwait(false))
                                                                .SelectAndCacheExpressionAsync(
                                                                    "/chummer/grades/grade[not(hide)]", token: token)
                                                                .ConfigureAwait(false))
                    {
                        string strName = (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name", token: token)
                                                          .ConfigureAwait(false))?.Value;
                        if (!string.IsNullOrEmpty(strName) && strName != "None")
                        {
                            string strBook = (await objXmlNode
                                                    .SelectSingleNodeAndCacheExpressionAsync("source", token: token)
                                                    .ConfigureAwait(false))
                                ?.Value;
                            if (!string.IsNullOrEmpty(strBook)
                                && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                                continue;
                            if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                                continue;
                            ListItem objExistingCoveredGrade =
                                lstGrades.Find(x => x.Value.ToString().Contains(strName));
                            if (objExistingCoveredGrade.Value != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(
                                              strName,
                                              (await objXmlNode
                                                     .SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                                     .ConfigureAwait(false))
                                              ?.Value
                                              ?? strName));
                        }
                    }

                    await flpAllowedCyberwareGrades.DoThreadSafeAsync(x =>
                    {
                        x.SuspendLayout();
                        try
                        {
                            x.Controls.Clear();
                            foreach (ListItem objGrade in lstGrades)
                            {
                                ColorableCheckBox chkGrade = new ColorableCheckBox
                                {
                                    UseVisualStyleBackColor = true,
                                    Text = objGrade.Name,
                                    Tag = objGrade.Value,
                                    AutoSize = true,
                                    Anchor = AnchorStyles.Left,
                                    Checked = !_objCharacterSettings.BannedWareGrades.Contains(
                                        objGrade.Value.ToString())
                                };
                                chkGrade.CheckedChanged += chkGrade_CheckedChanged;
                                x.Controls.Add(chkGrade);
                            }
                        }
                        finally
                        {
                            x.ResumeLayout();
                        }
                    }, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void RebuildCustomDataDirectoryInfos()
        {
            _dicCharacterCustomDataDirectoryInfos.Clear();
            foreach (KeyValuePair<string, bool> kvpCustomDataDirectory in _objCharacterSettings.CustomDataDirectoryKeys)
            {
                CustomDataDirectoryInfo objLoopInfo
                    = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(
                        x => x.CharacterSettingsSaveKey == kvpCustomDataDirectory.Key);
                if (objLoopInfo != default)
                {
                    _dicCharacterCustomDataDirectoryInfos.Add(objLoopInfo, kvpCustomDataDirectory.Value);
                }
                else
                {
                    _dicCharacterCustomDataDirectoryInfos.Add(kvpCustomDataDirectory.Key,
                                                              kvpCustomDataDirectory.Value);
                }
            }
        }

        private async ValueTask RebuildCustomDataDirectoryInfosAsync(CancellationToken token = default)
        {
            _dicCharacterCustomDataDirectoryInfos.Clear();
            await _objCharacterSettings.CustomDataDirectoryKeys.ForEachAsync(
                kvpCustomDataDirectory =>
                {
                    CustomDataDirectoryInfo objLoopInfo
                        = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(
                            x => x.CharacterSettingsSaveKey == kvpCustomDataDirectory.Key);
                    if (objLoopInfo != default)
                    {
                        _dicCharacterCustomDataDirectoryInfos.Add(objLoopInfo, kvpCustomDataDirectory.Value);
                    }
                    else
                    {
                        _dicCharacterCustomDataDirectoryInfos.Add(kvpCustomDataDirectory.Key,
                                                                  kvpCustomDataDirectory.Value);
                    }
                }, token: token).ConfigureAwait(false);
        }

        private async ValueTask SetToolTips(CancellationToken token = default)
        {
            await chkUnarmedSkillImprovements
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsUnarmedSkillImprovements", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkIgnoreArt
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsIgnoreArt", token: token).ConfigureAwait(false))
                      .WordWrap(), token).ConfigureAwait(false);
            await chkIgnoreComplexFormLimit
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsIgnoreComplexFormLimit", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkCyberlegMovement
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsCyberlegMovement", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkDontDoubleQualityPurchases
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsDontDoubleQualityPurchases", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkDontDoubleQualityRefunds
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsDontDoubleQualityRefunds", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkStrictSkillGroups
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionStrictSkillGroups", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkAllowInitiation
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsAllowInitiation", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkUseCalculatedPublicAwareness
                  .SetToolTipAsync(
                      (await LanguageManager.GetStringAsync("Tip_PublicAwareness", token: token).ConfigureAwait(false))
                      .WordWrap(), token).ConfigureAwait(false);
        }

        private async ValueTask SetupDataBindings(CancellationToken token = default)
        {
            await cmdRename.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = !y, _objCharacterSettings,
                                                                nameof(CharacterSettings.BuiltInOption),
                                                                x => x.GetBuiltInOptionAsync(token).AsTask(),
                                                                token: token)
                           .ConfigureAwait(false);
            await cmdDelete.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = !y, _objCharacterSettings,
                                                                nameof(CharacterSettings.BuiltInOption),
                                                                x => x.GetBuiltInOptionAsync(token).AsTask(),
                                                                token: token)
                           .ConfigureAwait(false);

            await cboBuildMethod
                  .DoDataBindingAsync("SelectedValue", _objCharacterSettings, nameof(CharacterSettings.BuildMethod),
                                      token).ConfigureAwait(false);
            await lblPriorityTable.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                       nameof(CharacterSettings
                                                                                  .BuildMethodUsesPriorityTables),
                                                                       x => x
                                                                            .GetBuildMethodUsesPriorityTablesAsync(
                                                                                token)
                                                                            .AsTask(), token: token)
                                  .ConfigureAwait(false);
            await cboPriorityTable.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                       nameof(CharacterSettings
                                                                                  .BuildMethodUsesPriorityTables),
                                                                       x => x
                                                                            .GetBuildMethodUsesPriorityTablesAsync(
                                                                                token)
                                                                            .AsTask(), token: token)
                                  .ConfigureAwait(false);
            await lblPriorities.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                    nameof(CharacterSettings.BuildMethodIsPriority),
                                                                    x => x.GetBuildMethodIsPriorityAsync(token)
                                                                          .AsTask(),
                                                                    token: token).ConfigureAwait(false);
            await txtPriorities.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                    nameof(CharacterSettings.BuildMethodIsPriority),
                                                                    x => x.GetBuildMethodIsPriorityAsync(token)
                                                                          .AsTask(),
                                                                    token: token).ConfigureAwait(false);
            await txtPriorities
                  .DoDataBindingAsync("Text", _objCharacterSettings, nameof(CharacterSettings.PriorityArray), token)
                  .ConfigureAwait(false);
            await lblSumToTen.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                  nameof(CharacterSettings.BuildMethodIsSumtoTen),
                                                                  x => x.GetBuildMethodIsSumtoTenAsync(token).AsTask(),
                                                                  token: token).ConfigureAwait(false);
            await nudSumToTen.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                  nameof(CharacterSettings.BuildMethodIsSumtoTen),
                                                                  x => x.GetBuildMethodIsSumtoTenAsync(token).AsTask(),
                                                                  token: token).ConfigureAwait(false);
            await nudSumToTen
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.SumtoTen), token)
                  .ConfigureAwait(false);
            await nudStartingKarma
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.BuildKarma), token)
                  .ConfigureAwait(false);
            await nudMaxNuyenKarma
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.NuyenMaximumBP), token)
                  .ConfigureAwait(false);
            await nudMaxAvail
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaximumAvailability),
                                      token).ConfigureAwait(false);
            await nudQualityKarmaLimit
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.QualityKarmaLimit),
                                      token).ConfigureAwait(false);
            await nudMaxNumberMaxAttributes
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.MaxNumberMaxAttributesCreate), token)
                  .ConfigureAwait(false);
            await nudMaxSkillRatingCreate
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxSkillRatingCreate),
                                      token).ConfigureAwait(false);
            await nudMaxKnowledgeSkillRatingCreate
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate), token)
                  .ConfigureAwait(false);
            await nudMaxMartialArts
                .DoDataBindingAsync("Value", _objCharacterSettings,
                    nameof(CharacterSettings.MaximumMartialArts), token)
                .ConfigureAwait(false);
            await nudMaxMartialTechniques
                .DoDataBindingAsync("Value", _objCharacterSettings,
                    nameof(CharacterSettings.MaximumMartialTechniques), token)
                .ConfigureAwait(false);
            await nudMaxSkillRatingCreate
                  .DoDataBindingAsync("Maximum", _objCharacterSettings, nameof(CharacterSettings.MaxSkillRating), token)
                  .ConfigureAwait(false);
            await nudMaxKnowledgeSkillRatingCreate
                  .DoDataBindingAsync("Maximum", _objCharacterSettings,
                                      nameof(CharacterSettings.MaxKnowledgeSkillRating), token).ConfigureAwait(false);
            await txtContactPoints
                  .DoDataBindingAsync("Text", _objCharacterSettings, nameof(CharacterSettings.ContactPointsExpression),
                                      token).ConfigureAwait(false);
            await txtKnowledgePoints
                  .DoDataBindingAsync("Text", _objCharacterSettings,
                                      nameof(CharacterSettings.KnowledgePointsExpression), token).ConfigureAwait(false);
            await txtRegisteredSpriteLimit
                  .DoDataBindingAsync("Text", _objCharacterSettings,
                                      nameof(CharacterSettings.RegisteredSpriteExpression), token)
                  .ConfigureAwait(false);
            await txtBoundSpiritLimit
                  .DoDataBindingAsync("Text", _objCharacterSettings, nameof(CharacterSettings.BoundSpiritExpression),
                                      token).ConfigureAwait(false);
            await txtEssenceModifierPostExpression
                  .DoDataBindingAsync("Text", _objCharacterSettings,
                                      nameof(CharacterSettings.EssenceModifierPostExpression), token)
                  .ConfigureAwait(false);
            await txtLiftLimit
                  .DoDataBindingAsync("Text", _objCharacterSettings, nameof(CharacterSettings.LiftLimitExpression),
                                      token).ConfigureAwait(false);
            await txtCarryLimit
                  .DoDataBindingAsync("Text", _objCharacterSettings, nameof(CharacterSettings.CarryLimitExpression),
                                      token).ConfigureAwait(false);
            await txtEncumbranceInterval
                  .DoDataBindingAsync("Text", _objCharacterSettings,
                                      nameof(CharacterSettings.EncumbranceIntervalExpression), token)
                  .ConfigureAwait(false);
            await nudWeightDecimals
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.WeightDecimals), token)
                  .ConfigureAwait(false);

            await chkEncumbrancePenaltyPhysicalLimit
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DoEncumbrancePenaltyPhysicalLimit), token)
                  .ConfigureAwait(false);
            await chkEncumbrancePenaltyMovementSpeed
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DoEncumbrancePenaltyMovementSpeed), token)
                  .ConfigureAwait(false);
            await chkEncumbrancePenaltyAgility
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DoEncumbrancePenaltyAgility), token)
                  .ConfigureAwait(false);
            await chkEncumbrancePenaltyReaction
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DoEncumbrancePenaltyReaction), token)
                  .ConfigureAwait(false);
            await chkEncumbrancePenaltyWoundModifier
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DoEncumbrancePenaltyWoundModifier), token)
                  .ConfigureAwait(false);

            await nudEncumbrancePenaltyPhysicalLimit
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.EncumbrancePenaltyPhysicalLimit), token)
                  .ConfigureAwait(false);
            await nudEncumbrancePenaltyMovementSpeed
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.EncumbrancePenaltyMovementSpeed), token)
                  .ConfigureAwait(false);
            await nudEncumbrancePenaltyAgility
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.EncumbrancePenaltyAgility), token).ConfigureAwait(false);
            await nudEncumbrancePenaltyReaction
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.EncumbrancePenaltyReaction), token)
                  .ConfigureAwait(false);
            await nudEncumbrancePenaltyWoundModifier
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.EncumbrancePenaltyWoundModifier), token)
                  .ConfigureAwait(false);

            await chkEnforceCapacity
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.EnforceCapacity),
                                      token).ConfigureAwait(false);
            await chkLicenseEachRestrictedItem
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.LicenseRestricted),
                                      token).ConfigureAwait(false);
            await chkReverseAttributePriorityOrder
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ReverseAttributePriorityOrder), token)
                  .ConfigureAwait(false);
            await chkDronemods
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.DroneMods), token)
                  .ConfigureAwait(false);
            await chkDronemodsMaximumPilot
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.DroneModsMaximumPilot),
                                      token).ConfigureAwait(false);
            await chkRestrictRecoil
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.RestrictRecoil), token)
                  .ConfigureAwait(false);
            await chkStrictSkillGroups
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.StrictSkillGroupsInCreateMode), token)
                  .ConfigureAwait(false);
            await chkAllowPointBuySpecializationsOnKarmaSkills
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.AllowPointBuySpecializationsOnKarmaSkills), token)
                  .ConfigureAwait(false);
            await chkAllowFreeGrids
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowFreeGrids), token)
                  .ConfigureAwait(false);

            await chkDontUseCyberlimbCalculation
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DontUseCyberlimbCalculation), token)
                  .ConfigureAwait(false);
            await chkCyberlegMovement
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.CyberlegMovement),
                                      token).ConfigureAwait(false);
            await chkCyberlimbAttributeBonusCap
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride), token)
                  .ConfigureAwait(false);
            await nudCyberlimbAttributeBonusCap.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride),
                x => x.GetCyberlimbAttributeBonusCapOverrideAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await nudCyberlimbAttributeBonusCap
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.CyberlimbAttributeBonusCap), token)
                  .ConfigureAwait(false);
            await chkRedlinerLimbsSkull
                  .DoNegatableDataBindingAsync("Checked", _objCharacterSettings,
                                               nameof(CharacterSettings.RedlinerExcludesSkull), token)
                  .ConfigureAwait(false);
            await chkRedlinerLimbsTorso
                  .DoNegatableDataBindingAsync("Checked", _objCharacterSettings,
                                               nameof(CharacterSettings.RedlinerExcludesTorso), token)
                  .ConfigureAwait(false);
            await chkRedlinerLimbsArms
                  .DoNegatableDataBindingAsync("Checked", _objCharacterSettings,
                                               nameof(CharacterSettings.RedlinerExcludesArms), token)
                  .ConfigureAwait(false);
            await chkRedlinerLimbsLegs
                  .DoNegatableDataBindingAsync("Checked", _objCharacterSettings,
                                               nameof(CharacterSettings.RedlinerExcludesLegs), token)
                  .ConfigureAwait(false);

            await nudNuyenDecimalsMaximum
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxNuyenDecimals), token)
                  .ConfigureAwait(false);
            await nudNuyenDecimalsMinimum
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MinNuyenDecimals), token)
                  .ConfigureAwait(false);
            await nudEssenceDecimals
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.EssenceDecimals), token)
                  .ConfigureAwait(false);
            await chkDontRoundEssenceInternally
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DontRoundEssenceInternally), token)
                  .ConfigureAwait(false);

            await nudMinInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MinInitiativeDice),
                                      token).ConfigureAwait(false);
            await nudMaxInitiativeDice
                  .DoDataBindingAsync("Minimum", _objCharacterSettings, nameof(CharacterSettings.MinInitiativeDice),
                                      token).ConfigureAwait(false);
            await nudMaxInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxInitiativeDice),
                                      token).ConfigureAwait(false);
            await nudMinAstralInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MinAstralInitiativeDice),
                                      token).ConfigureAwait(false);
            await nudMaxAstralInitiativeDice
                  .DoDataBindingAsync("Minimum", _objCharacterSettings,
                                      nameof(CharacterSettings.MinAstralInitiativeDice), token).ConfigureAwait(false);
            await nudMaxAstralInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxAstralInitiativeDice),
                                      token).ConfigureAwait(false);
            await nudMinColdSimInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.MinColdSimInitiativeDice), token).ConfigureAwait(false);
            await nudMaxColdSimInitiativeDice
                  .DoDataBindingAsync("Minimum", _objCharacterSettings,
                                      nameof(CharacterSettings.MinColdSimInitiativeDice), token).ConfigureAwait(false);
            await nudMaxColdSimInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.MaxColdSimInitiativeDice), token).ConfigureAwait(false);
            await nudMinHotSimInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MinHotSimInitiativeDice),
                                      token).ConfigureAwait(false);
            await nudMaxHotSimInitiativeDice
                  .DoDataBindingAsync("Minimum", _objCharacterSettings,
                                      nameof(CharacterSettings.MinHotSimInitiativeDice), token).ConfigureAwait(false);
            await nudMaxHotSimInitiativeDice
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxHotSimInitiativeDice),
                                      token).ConfigureAwait(false);

            await chkEnable4eStyleEnemyTracking
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.EnableEnemyTracking),
                                      token).ConfigureAwait(false);
            await flpKarmaGainedFromEnemies.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                x => x.GetEnableEnemyTrackingAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await nudKarmaGainedFromEnemies
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaEnemy), token)
                  .ConfigureAwait(false);
            await chkEnemyKarmaQualityLimit.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                x => x.GetEnableEnemyTrackingAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await chkEnemyKarmaQualityLimit
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.EnemyKarmaQualityLimit), token).ConfigureAwait(false);
            await chkMoreLethalGameplay
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.MoreLethalGameplay),
                                      token).ConfigureAwait(false);

            await chkNoArmorEncumbrance
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.NoArmorEncumbrance),
                                      token).ConfigureAwait(false);
            await chkUncappedArmorAccessoryBonuses
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.UncappedArmorAccessoryBonuses), token)
                  .ConfigureAwait(false);
            await chkIgnoreArt
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.IgnoreArt), token)
                  .ConfigureAwait(false);
            await chkIgnoreComplexFormLimit
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.IgnoreComplexFormLimit), token).ConfigureAwait(false);
            await chkUnarmedSkillImprovements
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.UnarmedImprovementsApplyToWeapons), token)
                  .ConfigureAwait(false);
            await chkMysAdPp
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.MysAdeptAllowPpCareer),
                                      token).ConfigureAwait(false);
            await chkMysAdPp.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = !y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                x => x.GetMysAdeptSecondMAGAttributeAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await chkPrioritySpellsAsAdeptPowers
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.PrioritySpellsAsAdeptPowers), token)
                  .ConfigureAwait(false);
            await chkPrioritySpellsAsAdeptPowers.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = !y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                x => x.GetMysAdeptSecondMAGAttributeAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await chkMysAdeptSecondMAGAttribute
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.MysAdeptSecondMAGAttribute), token)
                  .ConfigureAwait(false);
            await chkMysAdeptSecondMAGAttribute.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttributeEnabled),
                x => x.GetMysAdeptSecondMAGAttributeEnabledAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await chkUsePointsOnBrokenGroups
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.UsePointsOnBrokenGroups), token).ConfigureAwait(false);
            await chkSpecialKarmaCost
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.SpecialKarmaCostBasedOnShownValue), token)
                  .ConfigureAwait(false);
            await chkUseCalculatedPublicAwareness
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.UseCalculatedPublicAwareness), token)
                  .ConfigureAwait(false);
            await chkAlternateMetatypeAttributeKarma
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.AlternateMetatypeAttributeKarma), token)
                  .ConfigureAwait(false);
            await chkCompensateSkillGroupKarmaDifference
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.CompensateSkillGroupKarmaDifference), token)
                  .ConfigureAwait(false);
            await chkFreeMartialArtSpecialization
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.FreeMartialArtSpecialization), token)
                  .ConfigureAwait(false);
            await chkIncreasedImprovedAbilityModifier
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.IncreasedImprovedAbilityMultiplier), token)
                  .ConfigureAwait(false);
            await chkAllowTechnomancerSchooling
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.AllowTechnomancerSchooling), token)
                  .ConfigureAwait(false);
            await chkAllowSkillRegrouping
                  .DoDataBindingAsync("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowSkillRegrouping),
                                      token).ConfigureAwait(false);
            await chkSpecializationsBreakSkillGroups
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.SpecializationsBreakSkillGroups), token)
                  .ConfigureAwait(false);
            await chkDontDoubleQualityPurchases
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DontDoubleQualityPurchases), token)
                  .ConfigureAwait(false);
            await chkDontDoubleQualityRefunds
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DontDoubleQualityRefunds), token).ConfigureAwait(false);
            await chkDroneArmorMultiplier
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.DroneArmorMultiplierEnabled), token)
                  .ConfigureAwait(false);
            await nudDroneArmorMultiplier.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplierEnabled),
                x => x.GetDroneArmorMultiplierEnabledAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await nudDroneArmorMultiplier
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.DroneArmorMultiplier),
                                      token).ConfigureAwait(false);
            await chkESSLossReducesMaximumOnly
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ESSLossReducesMaximumOnly), token).ConfigureAwait(false);
            await chkExceedNegativeQualities
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ExceedNegativeQualities), token).ConfigureAwait(false);
            await chkExceedNegativeQualitiesNoBonus.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualities),
                x => x.GetExceedNegativeQualitiesAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await chkExceedNegativeQualitiesNoBonus
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ExceedNegativeQualitiesNoBonus), token)
                  .ConfigureAwait(false);
            await chkExceedPositiveQualities
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ExceedPositiveQualities), token).ConfigureAwait(false);
            await chkExceedPositiveQualitiesCostDoubled.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualities),
                x => x.GetExceedPositiveQualitiesAsync(token).AsTask(), token: token).ConfigureAwait(false);
            await chkExceedPositiveQualitiesCostDoubled
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ExceedPositiveQualitiesCostDoubled), token)
                  .ConfigureAwait(false);
            await chkExtendAnyDetectionSpell
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.ExtendAnyDetectionSpell), token).ConfigureAwait(false);
            await chkAllowCyberwareESSDiscounts
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.AllowCyberwareESSDiscounts), token)
                  .ConfigureAwait(false);
            await chkAllowInitiation
                  .DoDataBindingAsync("Checked", _objCharacterSettings,
                                      nameof(CharacterSettings.AllowInitiationInCreateMode), token)
                  .ConfigureAwait(false);
            await nudMaxSkillRating
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxSkillRating), token)
                  .ConfigureAwait(false);
            await nudMaxKnowledgeSkillRating
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.MaxKnowledgeSkillRating),
                                      token).ConfigureAwait(false);

            // Karma options.
            await nudMetatypeCostsKarmaMultiplier
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.MetatypeCostsKarmaMultiplier), token)
                  .ConfigureAwait(false);
            await nudKarmaNuyenPerWftM
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.NuyenPerBPWftM), token)
                  .ConfigureAwait(false);
            await nudKarmaNuyenPerWftP
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.NuyenPerBPWftP), token)
                  .ConfigureAwait(false);
            await nudKarmaAttribute
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaAttribute), token)
                  .ConfigureAwait(false);
            await nudKarmaQuality
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaQuality), token)
                  .ConfigureAwait(false);
            await nudKarmaSpecialization
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpecialization),
                                      token).ConfigureAwait(false);
            await nudKarmaKnowledgeSpecialization
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaKnowledgeSpecialization), token)
                  .ConfigureAwait(false);
            await nudKarmaNewKnowledgeSkill
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewKnowledgeSkill),
                                      token).ConfigureAwait(false);
            await nudKarmaNewActiveSkill
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewActiveSkill),
                                      token).ConfigureAwait(false);
            await nudKarmaNewSkillGroup
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewSkillGroup),
                                      token).ConfigureAwait(false);
            await nudKarmaImproveKnowledgeSkill
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaImproveKnowledgeSkill), token)
                  .ConfigureAwait(false);
            await nudKarmaImproveActiveSkill
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaImproveActiveSkill),
                                      token).ConfigureAwait(false);
            await nudKarmaImproveSkillGroup
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaImproveSkillGroup),
                                      token).ConfigureAwait(false);
            await nudKarmaSpell
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpell), token)
                  .ConfigureAwait(false);
            await nudKarmaNewComplexForm
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewComplexForm),
                                      token).ConfigureAwait(false);
            await nudKarmaNewAIProgram
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewAIProgram),
                                      token).ConfigureAwait(false);
            await nudKarmaNewAIAdvancedProgram
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaNewAIAdvancedProgram), token).ConfigureAwait(false);
            await nudKarmaMetamagic
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaMetamagic), token)
                  .ConfigureAwait(false);
            await nudKarmaContact
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaContact), token)
                  .ConfigureAwait(false);
            await nudKarmaCarryover
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaCarryover), token)
                  .ConfigureAwait(false);
            await nudKarmaSpirit
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpirit), token)
                  .ConfigureAwait(false);
            await nudKarmaSpiritFettering
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpiritFettering),
                                      token).ConfigureAwait(false);
            await nudKarmaTechnique
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaTechnique), token)
                  .ConfigureAwait(false);
            await nudKarmaInitiation
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaInitiation), token)
                  .ConfigureAwait(false);
            await nudKarmaInitiationFlat
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaInitiationFlat),
                                      token).ConfigureAwait(false);
            await nudKarmaJoinGroup
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaJoinGroup), token)
                  .ConfigureAwait(false);
            await nudKarmaLeaveGroup
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaLeaveGroup), token)
                  .ConfigureAwait(false);
            await nudKarmaMysticAdeptPowerPoint
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaMysticAdeptPowerPoint), token)
                  .ConfigureAwait(false);

            // Focus costs
            await nudKarmaAlchemicalFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaAlchemicalFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaBanishingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaBanishingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaBindingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaBindingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaCenteringFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaCenteringFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaCounterspellingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaCounterspellingFocus), token).ConfigureAwait(false);
            await nudKarmaDisenchantingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaDisenchantingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaFlexibleSignatureFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaFlexibleSignatureFocus), token)
                  .ConfigureAwait(false);
            await nudKarmaMaskingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaMaskingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaPowerFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaPowerFocus), token)
                  .ConfigureAwait(false);
            await nudKarmaQiFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaQiFocus), token)
                  .ConfigureAwait(false);
            await nudKarmaRitualSpellcastingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings,
                                      nameof(CharacterSettings.KarmaRitualSpellcastingFocus), token)
                  .ConfigureAwait(false);
            await nudKarmaSpellcastingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpellcastingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaSpellShapingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpellShapingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaSummoningFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSummoningFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaSustainingFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSustainingFocus),
                                      token).ConfigureAwait(false);
            await nudKarmaWeaponFocus
                  .DoDataBindingAsync("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaWeaponFocus), token)
                  .ConfigureAwait(false);
        }

        private async ValueTask PopulateSettingsList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelect = string.Empty;
                if (_intLoading == 0)
                    strSelect = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                .ConfigureAwait(false);
                _lstSettings.Clear();
                foreach (KeyValuePair<string, CharacterSettings> kvpCharacterSettingsEntry in await SettingsManager
                             .GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false))
                {
                    _lstSettings.Add(new ListItem(kvpCharacterSettingsEntry.Key,
                                                  await kvpCharacterSettingsEntry.Value.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                    if (ReferenceEquals(_objReferenceCharacterSettings, kvpCharacterSettingsEntry.Value))
                        strSelect = kvpCharacterSettingsEntry.Key;
                }

                _lstSettings.Sort(CompareListItems.CompareNames);
                await cboSetting.PopulateWithListItemsAsync(_lstSettings, token).ConfigureAwait(false);
                await cboSetting.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strSelect))
                        x.SelectedValue = strSelect;
                    if (x.SelectedIndex == -1 && _lstSettings.Count > 0)
                        x.SelectedValue = x.FindStringExact(GlobalSettings.DefaultCharacterSetting);
                    if (x.SelectedIndex == -1 && _lstSettings.Count > 0)
                        x.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
                _intOldSelectedSettingIndex = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                              .ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                if (Interlocked.CompareExchange(ref _intLoading, 1, 0) == 0)
                {
                    try
                    {
                        await SetIsDirtyAsync(!await _objCharacterSettings
                                                .HasIdenticalSettingsAsync(_objReferenceCharacterSettings)
                                                .ConfigureAwait(false)).ConfigureAwait(false);
                        switch (e.PropertyName)
                        {
                            case nameof(CharacterSettings.EnabledCustomDataDirectoryPaths):
                                await PopulateOptions().ConfigureAwait(false);
                                break;

                            case nameof(CharacterSettings.PriorityTable):
                                await PopulatePriorityTableList().ConfigureAwait(false);
                                break;
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                }
                else
                {
                    switch (e.PropertyName)
                    {
                        case nameof(CharacterSettings.BuiltInOption):
                        {
                            bool blnAllTextBoxesLegal = await IsAllTextBoxesLegalAsync().ConfigureAwait(false);
                            await cmdSave.DoThreadSafeAsync(
                                x => x.Enabled = IsDirty && blnAllTextBoxesLegal
                                                         && !_objCharacterSettings.BuiltInOption).ConfigureAwait(false);
                            break;
                        }
                        case nameof(CharacterSettings.PriorityArray):
                        case nameof(CharacterSettings.BuildMethod):
                        {
                            bool blnAllTextBoxesLegal = await IsAllTextBoxesLegalAsync().ConfigureAwait(false);
                            await cmdSaveAs.DoThreadSafeAsync(x => x.Enabled = IsDirty && blnAllTextBoxesLegal)
                                           .ConfigureAwait(false);
                            await cmdSave.DoThreadSafeAsync(
                                x => x.Enabled = IsDirty && blnAllTextBoxesLegal
                                                         && !_objCharacterSettings.BuiltInOption).ConfigureAwait(false);
                            break;
                        }
                        case nameof(CharacterSettings.ChargenKarmaToNuyenExpression)
                            : // Not data-bound so that the setter can be asynchronous
                        {
                            await txtNuyenExpression.DoThreadSafeAsync(
                                                        x => x.Text = _objCharacterSettings
                                                            .ChargenKarmaToNuyenExpression)
                                                    .ConfigureAwait(false);
                            break;
                        }
                    }
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private bool IsAllTextBoxesLegal()
        {
            if (_objCharacterSettings.BuildMethod == CharacterBuildMethod.Priority
                && _objCharacterSettings.PriorityArray.Length != 5)
                return false;

            return CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.ContactPointsExpression) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.KnowledgePointsExpression) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.ChargenKarmaToNuyenExpression.Replace("{Karma}", "1")
                                            .Replace("{PriorityNuyen}", "1")) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.EssenceModifierPostExpression.Replace("{Modifier}", "1.0")) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.RegisteredSpriteExpression) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.BoundSpiritExpression) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.LiftLimitExpression) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.CarryLimitExpression) &&
                   CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                       _objCharacterSettings.EncumbranceIntervalExpression);
        }

        private async ValueTask<bool> IsAllTextBoxesLegalAsync(CancellationToken token = default)
        {
            if (_objCharacterSettings.BuildMethod == CharacterBuildMethod.Priority
                && _objCharacterSettings.PriorityArray.Length != 5)
                return false;

            return await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.ContactPointsExpression, token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.KnowledgePointsExpression, token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       (await _objCharacterSettings.GetChargenKarmaToNuyenExpressionAsync(token).ConfigureAwait(false))
                       .Replace("{Karma}", "1")
                       .Replace("{PriorityNuyen}", "1"), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                            _objCharacterSettings.EssenceModifierPostExpression.Replace("{Modifier}",
                                                "1.0"), token: token)
                                        .ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.RegisteredSpriteExpression, token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.BoundSpiritExpression, token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.LiftLimitExpression, token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.CarryLimitExpression, token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       _objCharacterSettings.EncumbranceIntervalExpression, token: token).ConfigureAwait(false);
        }

        private bool IsDirty
        {
            get => _blnDirty;
            set
            {
                if (_blnDirty == value)
                    return;
                _blnDirty = value;
                string strText = LanguageManager.GetString(value ? "String_Cancel" : "String_OK");
                cmdOK.DoThreadSafe(x => x.Text = strText);
                if (value)
                {
                    bool blnIsAllTextBoxesLegal = IsAllTextBoxesLegal();
                    cmdSaveAs.DoThreadSafe(x => x.Enabled = blnIsAllTextBoxesLegal);
                    if (blnIsAllTextBoxesLegal)
                    {
                        bool blnTemp = _objCharacterSettings.BuiltInOption;
                        cmdSave.DoThreadSafe(x => x.Enabled = !blnTemp);
                    }
                    else
                        cmdSave.DoThreadSafe(x => x.Enabled = false);
                }
                else
                {
                    _blnWasRenamed = false;
                    cmdSaveAs.DoThreadSafe(x => x.Enabled = false);
                    cmdSave.DoThreadSafe(x => x.Enabled = false);
                }
            }
        }

        private async ValueTask SetIsDirtyAsync(bool value, CancellationToken token = default)
        {
            if (_blnDirty == value)
                return;
            _blnDirty = value;
            string strText = await LanguageManager.GetStringAsync(value ? "String_Cancel" : "String_OK", token: token)
                                                  .ConfigureAwait(false);
            await cmdOK.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
            if (value)
            {
                bool blnIsAllTextBoxesLegal = await IsAllTextBoxesLegalAsync(token).ConfigureAwait(false);
                await cmdSaveAs.DoThreadSafeAsync(x => x.Enabled = blnIsAllTextBoxesLegal, token).ConfigureAwait(false);
                if (blnIsAllTextBoxesLegal)
                {
                    bool blnTemp = await _objCharacterSettings.GetBuiltInOptionAsync(token).ConfigureAwait(false);
                    await cmdSave.DoThreadSafeAsync(x => x.Enabled = !blnTemp, token).ConfigureAwait(false);
                }
                else
                    await cmdSave.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            }
            else
            {
                _blnWasRenamed = false;
                await cmdSaveAs.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdSave.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            }
        }

        #endregion Methods
    }
}
