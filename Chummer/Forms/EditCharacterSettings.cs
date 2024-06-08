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
        private readonly LockingTypedOrderedDictionary<object, bool> _dicCharacterCustomDataDirectoryInfos;

        private int _intLoading = 1;
        private int _intSkipLimbCountUpdate;
        private int _intDirty;
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
                    _objReferenceCharacterSettings = SettingsManager.LoadedCharacterSettings.First().Value;
            }

            _dicCharacterCustomDataDirectoryInfos = new LockingTypedOrderedDictionary<object, bool>();
            _objCharacterSettings = new CharacterSettings(_objReferenceCharacterSettings);
            _objCharacterSettings.MultiplePropertiesChangedAsync += SettingsChanged;
            Disposed += (sender, args) =>
            {
                _dicCharacterCustomDataDirectoryInfos.Dispose();
                _objCharacterSettings.MultiplePropertiesChangedAsync -= SettingsChanged;
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
            if (await Program.ShowScrollableMessageBoxAsync(
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_CharacterOptions_ConfirmDelete")
                            .ConfigureAwait(false),
                        await _objReferenceCharacterSettings.GetNameAsync().ConfigureAwait(false)),
                    await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_ConfirmDelete")
                        .ConfigureAwait(false),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning).ConfigureAwait(false) != DialogResult.Yes)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                ConcurrentDictionary<string, CharacterSettings> dicCharacterSettings
                    = await SettingsManager.GetLoadedCharacterSettingsAsModifiableAsync().ConfigureAwait(false);
                if (!dicCharacterSettings.TryRemove(
                        await _objReferenceCharacterSettings.GetDictionaryKeyAsync()
                            .ConfigureAwait(false), out CharacterSettings objDeletedSettings))
                    return;
                try
                {
                    if (!await FileExtensions.SafeDeleteAsync(
                                                 Path.Combine(Utils.GetSettingsFolderPath,
                                                              await _objReferenceCharacterSettings.GetFileNameAsync().ConfigureAwait(false)), true)
                                             .ConfigureAwait(false))
                    {
                        // Revert removal of setting if we cannot delete the file
                        dicCharacterSettings.TryAdd(
                            await objDeletedSettings.GetDictionaryKeyAsync().ConfigureAwait(false), objDeletedSettings);
                        return;
                    }
                }
                catch
                {
                    // Revert removal of setting if we cannot delete the file
                    dicCharacterSettings.TryAdd(
                        await objDeletedSettings.GetDictionaryKeyAsync().ConfigureAwait(false), objDeletedSettings);
                    throw;
                }

                // Force repopulate character settings list in Master Index from here in lieu of event handling for concurrent dictionaries
                _blnForceMasterIndexRepopulateOnClose = true;
                CharacterBuildMethod eReferenceBuildMethod = await _objReferenceCharacterSettings.GetBuildMethodAsync().ConfigureAwait(false);
                KeyValuePair<string, CharacterSettings> kvpReplacementOption
                    = await dicCharacterSettings.FirstOrDefaultAsync(async x =>
                        await x.Value.GetBuiltInOptionAsync().ConfigureAwait(false) &&
                        await x.Value.GetBuildMethodAsync().ConfigureAwait(false) == eReferenceBuildMethod).ConfigureAwait(false);
                string strReferenceFileName = await _objReferenceCharacterSettings.GetFileNameAsync().ConfigureAwait(false);
                await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                {
                    if (await objCharacter.GetSettingsKeyAsync().ConfigureAwait(false) == strReferenceFileName)
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
            ConcurrentDictionary<string, CharacterSettings> dicCharacterSettings
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

                    // ReSharper disable once AccessToModifiedClosure
                    if (await dicCharacterSettings.AnyAsync(async x => await x.Value.GetNameAsync().ConfigureAwait(false) == strSelectedName).ConfigureAwait(false))
                    {
                        DialogResult eCreateDuplicateSetting = await Program.ShowScrollableMessageBoxAsync(
                            string.Format(
                                await LanguageManager.GetStringAsync("Message_CharacterOptions_DuplicateSettingName")
                                    .ConfigureAwait(false),
                                strSelectedName),
                            await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_DuplicateFileName")
                                .ConfigureAwait(false),
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning).ConfigureAwait(false);
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
                        await Program.ShowScrollableMessageBoxAsync(
                            await LanguageManager.GetStringAsync("Message_CharacterOptions_SettingFileNameTooLongError")
                                .ConfigureAwait(false),
                            await LanguageManager
                                .GetStringAsync("MessageTitle_CharacterOptions_SettingFileNameTooLongError")
                                .ConfigureAwait(false),
                            MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
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
                    if (!dicCharacterSettings.TryAdd(strKey, objNewCharacterSettings))
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
                        dicCharacterSettings.TryRemove(strKey, out _);
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                    if (!blnSaveSuccessful)
                    {
                        // Revert addition of settings if we cannot create a file
                        dicCharacterSettings.TryRemove(strKey, out _);
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
                            await Program.ShowScrollableMessageBoxAsync(this,
                                await LanguageManager.GetStringAsync(
                                        "Message_CharacterOptions_OpenCharacterOnBuildMethodChange")
                                    .ConfigureAwait(false)
                                +
                                sbdConflictingCharacters,
                                await LanguageManager.GetStringAsync(
                                        "MessageTitle_CharacterOptions_OpenCharacterOnBuildMethodChange")
                                    .ConfigureAwait(false),
                                MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
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
            if (!(await SettingsManager.GetLoadedCharacterSettingsAsync().ConfigureAwait(false)).TryGetValue(
                    strSelectedFile, out CharacterSettings objNewOption))
                return;

            if (IsDirty)
            {
                string text = await LanguageManager.GetStringAsync("Message_CharacterOptions_UnsavedDirty")
                                                   .ConfigureAwait(false);
                string caption = await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_UnsavedDirty")
                                                      .ConfigureAwait(false);

                if (await Program.ShowScrollableMessageBoxAsync(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question).ConfigureAwait(false) !=
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
            if (await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager.GetStringAsync("Message_Options_RestoreDefaults").ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_Options_RestoreDefaults").ConfigureAwait(false),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question).ConfigureAwait(false) != DialogResult.Yes)
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

        private bool _blnSkipClosing;

        private async void EditCharacterSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_blnSkipClosing) // Needed for weird async FormClosing event issue workaround
                return;
            Form frmSender = sender as Form;
            if (frmSender != null)
            {
                e.Cancel = true; // Always have to cancel because of issues with async FormClosing events
                await frmSender.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false); // Disable the form to make sure we can't interract with it anymore
            }

            try
            {
                // Caller returns and form stays open (weird async FormClosing event issue workaround)
                await Task.Yield();

                if (IsDirty && await Program.ShowScrollableMessageBoxAsync(
                            await LanguageManager.GetStringAsync("Message_CharacterOptions_UnsavedDirty")
                                .ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageTitle_CharacterOptions_UnsavedDirty")
                                .ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        .ConfigureAwait(false)
                    != DialogResult.Yes)
                {
                    return;
                }

                if (_blnForceMasterIndexRepopulateOnClose && Program.MainForm.MasterIndex != null)
                {
                    await Program.MainForm.MasterIndex.ForceRepopulateCharacterSettings().ConfigureAwait(false);
                }

                // Now we close the original caller (weird async FormClosing event issue workaround)
                if (frmSender != null)
                {
                    _blnSkipClosing = true;
                    await frmSender.DoThreadSafeAsync(x => x.Close()).ConfigureAwait(false);
                }
            }
            finally
            {
                if (frmSender != null)
                    await frmSender.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false); // Doesn't matter if we're closed
            }
        }

        private async void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _intLoading);
            try
            {
                foreach (TreeNode objNode in await treSourcebook.DoThreadSafeFuncAsync(x => x.Nodes).ConfigureAwait(false))
                {
                    string strBookCode = await treSourcebook.DoThreadSafeFuncAsync(() => objNode.Tag.ToString()).ConfigureAwait(false);
                    if (!_setPermanentSourcebooks.Contains(strBookCode))
                    {
                        await treSourcebook.DoThreadSafeFuncAsync(() => objNode.Checked = _blnSourcebookToggle).ConfigureAwait(false);
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

            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.Books)).ConfigureAwait(false);
            _blnSourcebookToggle = !_blnSourcebookToggle;
        }

        private async void treSourcebook_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_intLoading > 0)
                return;
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            string strBookCode = await treSourcebook.DoThreadSafeFuncAsync(() => objNode.Tag.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strBookCode)
                || (_setPermanentSourcebooks.Contains(strBookCode) && !await treSourcebook.DoThreadSafeFuncAsync(() => objNode.Checked).ConfigureAwait(false)))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    await treSourcebook.DoThreadSafeFuncAsync(() => objNode.Checked = !objNode.Checked).ConfigureAwait(false);
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
            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.Books)).ConfigureAwait(false);
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
            await _dicCharacterCustomDataDirectoryInfos.ReverseAsync(intIndex - 1, 2).ConfigureAwait(false);
            await _objCharacterSettings.CustomDataDirectoryKeys.ReverseAsync(intIndex - 1, 2).ConfigureAwait(false);
            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
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
            IAsyncDisposable objLocker = await _objCharacterSettings.CustomDataDirectoryKeys.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                IAsyncDisposable objLocker2
                    = await _dicCharacterCustomDataDirectoryInfos.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    for (int i = intIndex; i > 0; --i)
                    {
                        await _dicCharacterCustomDataDirectoryInfos.ReverseAsync(i - 1, 2).ConfigureAwait(false);
                        await _objCharacterSettings.CustomDataDirectoryKeys.ReverseAsync(i - 1, 2).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private async void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedNode)
                                                                 .ConfigureAwait(false);
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= await _dicCharacterCustomDataDirectoryInfos.GetCountAsync().ConfigureAwait(false) - 1)
                return;

            IAsyncDisposable objLocker = await _dicCharacterCustomDataDirectoryInfos.LockObject.EnterUpgradeableReadLockAsync().ConfigureAwait(false);
            try
            {
                if (intIndex >= await _dicCharacterCustomDataDirectoryInfos.GetCountAsync().ConfigureAwait(false) - 1)
                    return;
                IAsyncDisposable objLocker2 = await _dicCharacterCustomDataDirectoryInfos.LockObject
                    .EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    await _dicCharacterCustomDataDirectoryInfos.ReverseAsync(intIndex, 2).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await _objCharacterSettings.CustomDataDirectoryKeys.ReverseAsync(intIndex, 2).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private async void cmdToBottomCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = await treCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedNode)
                                                                 .ConfigureAwait(false);
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= await _dicCharacterCustomDataDirectoryInfos.GetCountAsync().ConfigureAwait(false) - 1)
                    return;

            IAsyncDisposable objLocker = await _dicCharacterCustomDataDirectoryInfos.LockObject.EnterUpgradeableReadLockAsync().ConfigureAwait(false);
            try
            {
                int intCount = await _dicCharacterCustomDataDirectoryInfos.GetCountAsync().ConfigureAwait(false);
                if (intIndex >= intCount - 1)
                    return;
                IAsyncDisposable objLocker2 = await _objCharacterSettings.CustomDataDirectoryKeys.LockObject
                    .EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    IAsyncDisposable objLocker3 = await _dicCharacterCustomDataDirectoryInfos.LockObject
                        .EnterWriteLockAsync().ConfigureAwait(false);
                    try
                    {
                        for (int i = intIndex; i < intCount - 1; ++i)
                        {
                            await _dicCharacterCustomDataDirectoryInfos.ReverseAsync(i, 2).ConfigureAwait(false);
                            await _objCharacterSettings.CustomDataDirectoryKeys.ReverseAsync(i, 2)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker3.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
            await PopulateCustomDataDirectoryTreeView().ConfigureAwait(false);
        }

        private async void treCustomDataDirectories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_intLoading > 0)
                return;
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            int intIndex = objNode.Index;
            bool blnChecked = objNode.Checked;
            await _dicCharacterCustomDataDirectoryInfos.SetValueAtAsync(intIndex, blnChecked).ConfigureAwait(false);
            switch (objNode.Tag)
            {
                case CustomDataDirectoryInfo objCustomDataDirectoryInfo:
                    if (await _objCharacterSettings.CustomDataDirectoryKeys.TryUpdateAsync(objCustomDataDirectoryInfo.CharacterSettingsSaveKey, blnChecked).ConfigureAwait(false))
                        await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
                    break;
                case string strCustomDataDirectoryKey:
                    if (await _objCharacterSettings.CustomDataDirectoryKeys.TryUpdateAsync(strCustomDataDirectoryKey, blnChecked).ConfigureAwait(false))
                        await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
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
            Color objWindowTextColor = ColorManager.WindowText;
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
                    ? ColorManager.WindowText
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
                    ? ColorManager.WindowText
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
                    ? ColorManager.WindowText
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
                    ? ColorManager.WindowText
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
                    ? ColorManager.WindowText
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
                    ? ColorManager.WindowText
                    : ColorManager.ErrorColor;
            await txtEssenceModifierPostExpression.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtLiftLimit_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                    await txtLiftLimit.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false)
                    ? ColorManager.WindowText
                    : ColorManager.ErrorColor;
            await txtLiftLimit.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void txtCarryLimit_TextChanged(object sender, EventArgs e)
        {
            Color objColor
                = await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                    await txtCarryLimit.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false)
                    ? ColorManager.WindowText
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
                    ? ColorManager.WindowText
                    : ColorManager.ErrorColor;
            await txtEncumbranceInterval.DoThreadSafeAsync(x => x.ForeColor = objColor).ConfigureAwait(false);
        }

        private async void chkGrade_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox chkGrade))
                return;

            string strGrade = await chkGrade.DoThreadSafeFuncAsync(x => x.Tag.ToString()).ConfigureAwait(false);
            if (await chkGrade.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                if (_objCharacterSettings.BannedWareGrades.Remove(strGrade))
                {
                    await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.BannedWareGrades)).ConfigureAwait(false);
                }
            }
            else if (_objCharacterSettings.BannedWareGrades.Add(strGrade))
            {
                await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.BannedWareGrades)).ConfigureAwait(false);
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

        private async Task PopulateSourcebookTreeView(CancellationToken token = default)
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
                foreach (XPathNavigator objXmlBook in (await XmlManager.LoadXPathAsync(
                                                                "books.xml",
                                                                _objCharacterSettings.EnabledCustomDataDirectoryPaths,
                                                                token: token).ConfigureAwait(false))
                                                            .SelectAndCacheExpression(
                                                                "/chummer/books/book", token: token))
                {
                    if (objXmlBook.SelectSingleNodeAndCacheExpression("hide", token: token) != null)
                        continue;
                    string strCode = objXmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                    if (string.IsNullOrEmpty(strCode))
                        continue;
                    bool blnChecked = _objCharacterSettings.Books.Contains(strCode);
                    if (objXmlBook.SelectSingleNodeAndCacheExpression("permanent", token: token) != null)
                    {
                        _setPermanentSourcebooks.Add(strCode);
                        if (_objCharacterSettings.BooksWritable.Add(strCode))
                            await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.Books), token).ConfigureAwait(false);
                        blnChecked = true;
                    }

                    string strTranslate
                        = objXmlBook.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value;
                    string strName = objXmlBook.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
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

        private async Task PopulateCustomDataDirectoryTreeView(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            object objOldSelected = await treCustomDataDirectories
                                          .DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false);
            await treCustomDataDirectories.DoThreadSafeAsync(x => x.BeginUpdate(), token).ConfigureAwait(false);
            try
            {
                string strFileNotFound = await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                                                              .ConfigureAwait(false);
                Color objErrorColor = ColorManager.ErrorColor;
                Color objGrayTextColor = ColorManager.GrayText;
                IAsyncDisposable objLocker = await _dicCharacterCustomDataDirectoryInfos.LockObject.EnterUpgradeableReadLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    int intNewCount
                        = await _dicCharacterCustomDataDirectoryInfos.GetCountAsync(token).ConfigureAwait(false);
                    if (intNewCount != await treCustomDataDirectories
                            .DoThreadSafeFuncAsync(x => x.Nodes.Count, token: token)
                            .ConfigureAwait(false))
                    {
                        List<TreeNode> lstNodes = new List<TreeNode>(intNewCount);
                        await _dicCharacterCustomDataDirectoryInfos.ForEachAsync(async kvpInfo =>
                        {
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
                                        objNode.ForeColor = objErrorColor;
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
                        }, token: token).ConfigureAwait(false);

                        await treCustomDataDirectories.DoThreadSafeAsync(x =>
                        {
                            x.Nodes.Clear();
                            foreach (TreeNode objNode in lstNodes)
                                x.Nodes.Add(objNode);
                        }, token).ConfigureAwait(false);
                    }
                    else
                    {
                        Color objWindowTextColor = ColorManager.WindowText;
                        for (int i = 0; i < intNewCount; ++i)
                        {
                            KeyValuePair<object, bool> kvpInfo = await _dicCharacterCustomDataDirectoryInfos
                                .GetValueAtAsync(i, token).ConfigureAwait(false);
                            int i1 = i;
                            TreeNode objNode = await treCustomDataDirectories
                                .DoThreadSafeFuncAsync(x => x.Nodes[i1], token)
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
                                            objNode.ForeColor = objErrorColor;
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
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
        private async Task PopulateOptions(CancellationToken token = default)
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

        private async Task PopulatePriorityTableList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstPriorityTables))
                {
                    foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                       .LoadXPathAsync("priorities.xml",
                                                                           _objCharacterSettings
                                                                               .EnabledCustomDataDirectoryPaths,
                                                                           token: token).ConfigureAwait(false))
                                                                .SelectAndCacheExpression(
                                                                    "/chummer/prioritytables/prioritytable",
                                                                    token: token))
                    {
                        string strName = objXmlNode.Value;
                        if (!string.IsNullOrEmpty(strName))
                            lstPriorityTables.Add(new ListItem(objXmlNode.Value,
                                                               objXmlNode
                                                                   .SelectSingleNodeAndCacheExpression(
                                                                       "@translate", token: token)
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

        private async Task PopulateLimbCountList(CancellationToken token = default)
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
                        foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                           .LoadXPathAsync("options.xml",
                                                                               _objCharacterSettings
                                                                                   .EnabledCustomDataDirectoryPaths,
                                                                               token: token).ConfigureAwait(false))
                                                                    .SelectAndCacheExpression(
                                                                        "/chummer/limbcounts/limb", token: token))
                        {
                            string strExclude
                                = objXmlNode.SelectSingleNodeAndCacheExpression("exclude", token: token)?.Value
                                  ?? string.Empty;
                            if (!string.IsNullOrEmpty(strExclude))
                                strExclude = '<' + strExclude;
                            lstLimbCount.Add(new ListItem(
                                                 objXmlNode
                                                     .SelectSingleNodeAndCacheExpression(
                                                         "limbcount", token: token)
                                                 ?.Value + strExclude,
                                                 objXmlNode
                                                     .SelectSingleNodeAndCacheExpression(
                                                         "translate", token: token)
                                                 ?.Value
                                                 ?? objXmlNode
                                                     .SelectSingleNodeAndCacheExpression(
                                                         "name", token: token)
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

        private async Task PopulateAllowedGrades(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstGrades))
                {
                    foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                       .LoadXPathAsync("bioware.xml",
                                                                           _objCharacterSettings
                                                                               .EnabledCustomDataDirectoryPaths,
                                                                           token: token).ConfigureAwait(false))
                                                                .SelectAndCacheExpression(
                                                                    "/chummer/grades/grade[not(hide)]", token: token))
                    {
                        string strName = objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
                        if (!string.IsNullOrEmpty(strName) && strName != "None")
                        {
                            string strBook = objXmlNode.SelectSingleNodeAndCacheExpression("source", token: token)?.Value;
                            if (!string.IsNullOrEmpty(strBook)
                                && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                                continue;
                            if (strName.ContainsAny(lstGrades.Select(x => x.Value.ToString())))
                                continue;
                            ListItem objExistingCoveredGrade =
                                lstGrades.Find(x => x.Value.ToString().Contains(strName));
                            if (objExistingCoveredGrade.Value != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(
                                              strName,
                                              objXmlNode
                                                  .SelectSingleNodeAndCacheExpression("translate", token: token)
                                              ?.Value
                                              ?? strName));
                        }
                    }

                    foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                       .LoadXPathAsync("cyberware.xml",
                                                                           _objCharacterSettings
                                                                               .EnabledCustomDataDirectoryPaths,
                                                                           token: token).ConfigureAwait(false))
                                                                .SelectAndCacheExpression(
                                                                    "/chummer/grades/grade[not(hide)]", token: token))
                    {
                        string strName = objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
                        if (!string.IsNullOrEmpty(strName) && strName != "None")
                        {
                            string strBook = objXmlNode
                                .SelectSingleNodeAndCacheExpression("source", token: token)
                                ?.Value;
                            if (!string.IsNullOrEmpty(strBook)
                                && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                                continue;
                            if (strName.ContainsAny(lstGrades.Select(x => x.Value.ToString())))
                                continue;
                            ListItem objExistingCoveredGrade =
                                lstGrades.Find(x => x.Value.ToString().Contains(strName));
                            if (objExistingCoveredGrade.Value != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(
                                              strName,
                                              objXmlNode
                                                  .SelectSingleNodeAndCacheExpression("translate", token: token)
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
            using (_dicCharacterCustomDataDirectoryInfos.LockObject.EnterWriteLock())
            {
                _dicCharacterCustomDataDirectoryInfos.Clear();
                _objCharacterSettings.CustomDataDirectoryKeys.ForEach(kvpCustomDataDirectory =>
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
                });
            }
        }

        private async Task RebuildCustomDataDirectoryInfosAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _dicCharacterCustomDataDirectoryInfos.LockObject
                .EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _dicCharacterCustomDataDirectoryInfos.ClearAsync(token).ConfigureAwait(false);
                await _objCharacterSettings.CustomDataDirectoryKeys.ForEachAsync(kvpCustomDataDirectory =>
                    {
                        CustomDataDirectoryInfo objLoopInfo
                            = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(
                                x => x.CharacterSettingsSaveKey == kvpCustomDataDirectory.Key);
                        if (objLoopInfo != default)
                        {
                            return _dicCharacterCustomDataDirectoryInfos.AddAsync(objLoopInfo, kvpCustomDataDirectory.Value, token);
                        }
                        else
                        {
                            return _dicCharacterCustomDataDirectoryInfos.AddAsync(kvpCustomDataDirectory.Key,
                                kvpCustomDataDirectory.Value, token);
                        }
                    }, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task SetToolTips(CancellationToken token = default)
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

        private async Task SetupDataBindings(CancellationToken token = default)
        {
            await cmdRename.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = !y, _objCharacterSettings,
                                                                nameof(CharacterSettings.BuiltInOption),
                                                                x => x.GetBuiltInOptionAsync(token),
                                                                token: token)
                           .ConfigureAwait(false);
            await cmdDelete.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = !y, _objCharacterSettings,
                                                                nameof(CharacterSettings.BuiltInOption),
                                                                x => x.GetBuiltInOptionAsync(token),
                                                                token: token)
                           .ConfigureAwait(false);

            await cboBuildMethod.RegisterAsyncDataBindingWithDelayAsync(
                x => (CharacterBuildMethod)x.SelectedValue,
                (x, y) => x.SelectedValue = y, _objCharacterSettings,
                nameof(CharacterSettings.BuildMethod),
                (x, y) => x.SelectedValueChanged += y,
                x => x.GetBuildMethodAsync(token),
                (x, y) => x.SetBuildMethodAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await lblPriorityTable.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                       nameof(CharacterSettings
                                                                                  .BuildMethodUsesPriorityTables),
                                                                       x => x
                                                                            .GetBuildMethodUsesPriorityTablesAsync(
                                                                                token)
                                                                            , token: token)
                                  .ConfigureAwait(false);
            await cboPriorityTable.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                       nameof(CharacterSettings
                                                                                  .BuildMethodUsesPriorityTables),
                                                                       x => x
                                                                            .GetBuildMethodUsesPriorityTablesAsync(
                                                                                token)
                                                                            , token: token)
                                  .ConfigureAwait(false);
            await lblPriorities.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                    nameof(CharacterSettings.BuildMethodIsPriority),
                                                                    x => x.GetBuildMethodIsPriorityAsync(token)
                                                                          ,
                                                                    token: token).ConfigureAwait(false);
            await txtPriorities.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                    nameof(CharacterSettings.BuildMethodIsPriority),
                                                                    x => x.GetBuildMethodIsPriorityAsync(token)
                                                                          ,
                                                                    token: token).ConfigureAwait(false);
            await txtPriorities.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                _objCharacterSettings,
                nameof(CharacterSettings.PriorityArray),
                (x, y) => x.TextChanged += y,
                x => x.GetPriorityArrayAsync(token),
                (x, y) => x.SetPriorityArrayAsync(y, token),
                1000, token, token).ConfigureAwait(false);
            await lblSumToTen.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                  nameof(CharacterSettings.BuildMethodIsSumtoTen),
                                                                  x => x.GetBuildMethodIsSumtoTenAsync(token),
                                                                  token: token).ConfigureAwait(false);
            await nudSumToTen.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                  nameof(CharacterSettings.BuildMethodIsSumtoTen),
                                                                  x => x.GetBuildMethodIsSumtoTenAsync(token),
                                                                  token: token).ConfigureAwait(false);
            await nudSumToTen.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.SumtoTen),
                (x, y) => x.ValueChanged += y,
                x => x.GetSumtoTenAsync(token),
                (x, y) => x.SetSumtoTenAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudStartingKarma.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.BuildKarma),
                (x, y) => x.ValueChanged += y,
                x => x.GetBuildKarmaAsync(token),
                (x, y) => x.SetBuildKarmaAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxNuyenKarma.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenMaximumBP),
                (x, y) => x.ValueChanged += y,
                x => x.GetNuyenMaximumBPAsync(token),
                (x, y) => x.SetNuyenMaximumBPAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxAvail.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaximumAvailability),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaximumAvailabilityAsync(token),
                (x, y) => x.SetMaximumAvailabilityAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudQualityKarmaLimit.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.QualityKarmaLimit),
                (x, y) => x.ValueChanged += y,
                x => x.GetQualityKarmaLimitAsync(token),
                (x, y) => x.SetQualityKarmaLimitAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudNuyenCarryover.RegisterAsyncDataBindingWithDelayAsync(x => x.Value,
                (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenCarryover),
                (x, y) => x.ValueChanged += y,
                x => x.GetNuyenCarryoverAsync(token),
                (x, y) => x.SetNuyenCarryoverAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudNuyenCarryover
                .RegisterOneWayAsyncDataBindingAsync((x, y) => x.DecimalPlaces = y, _objCharacterSettings,
                    nameof(CharacterSettings.MaxNuyenDecimals), x => x.GetMaxNuyenDecimalsAsync(token), token)
                .ConfigureAwait(false);
            await nudKarmaCarryover.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaCarryover),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaCarryoverAsync(token),
                (x, y) => x.SetKarmaCarryoverAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxNumberMaxAttributes.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxNumberMaxAttributesCreate),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxNumberMaxAttributesCreateAsync(token),
                (x, y) => x.SetMaxNumberMaxAttributesCreateAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxSkillRatingCreate.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxSkillRatingCreate),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxSkillRatingCreateAsync(token),
                (x, y) => x.SetMaxSkillRatingCreateAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxKnowledgeSkillRatingCreate.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxKnowledgeSkillRatingCreateAsync(token),
                (x, y) => x.SetMaxKnowledgeSkillRatingCreateAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxMartialArts.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaximumMartialArts),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaximumMartialArtsAsync(token),
                (x, y) => x.SetMaximumMartialArtsAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxMartialTechniques.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaximumMartialTechniques),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaximumMartialTechniquesAsync(token),
                (x, y) => x.SetMaximumMartialTechniquesAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxSkillRatingCreate
                .RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objCharacterSettings,
                    nameof(CharacterSettings.MaxSkillRating), x => x.GetMaxSkillRatingAsync(token), token)
                .ConfigureAwait(false);
            await nudMaxKnowledgeSkillRatingCreate
                .RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objCharacterSettings,
                    nameof(CharacterSettings.MaxKnowledgeSkillRating), x => x.GetMaxKnowledgeSkillRatingAsync(token), token)
                .ConfigureAwait(false);
            await txtContactPoints.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.ContactPointsExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetContactPointsExpressionAsync(token),
                (x, y) => x.SetContactPointsExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtKnowledgePoints.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.KnowledgePointsExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetKnowledgePointsExpressionAsync(token),
                (x, y) => x.SetKnowledgePointsExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtRegisteredSpriteLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.RegisteredSpriteExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetRegisteredSpriteExpressionAsync(token),
                (x, y) => x.SetRegisteredSpriteExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtBoundSpiritLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.BoundSpiritExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetBoundSpiritExpressionAsync(token),
                (x, y) => x.SetBoundSpiritExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtEssenceModifierPostExpression.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.EssenceModifierPostExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetEssenceModifierPostExpressionAsync(token),
                (x, y) => x.SetEssenceModifierPostExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtLiftLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.LiftLimitExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetLiftLimitExpressionAsync(token),
                (x, y) => x.SetLiftLimitExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtCarryLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.CarryLimitExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetCarryLimitExpressionAsync(token),
                (x, y) => x.SetCarryLimitExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtEncumbranceInterval.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.EncumbranceIntervalExpression),
                (x, y) => x.TextChanged += y,
                x => x.GetEncumbranceIntervalExpressionAsync(token),
                (x, y) => x.SetEncumbranceIntervalExpressionAsync(y, token),
                1000,
                token,
                token).ConfigureAwait(false);
            await nudWeightDecimals.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.WeightDecimals),
                (x, y) => x.ValueChanged += y,
                x => x.GetWeightDecimalsAsync(token),
                (x, y) => x.SetWeightDecimalsAsync(y, token), 250, token, token).ConfigureAwait(false);

            await chkEncumbrancePenaltyPhysicalLimit.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyPhysicalLimit),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDoEncumbrancePenaltyPhysicalLimitAsync(token),
                (x, y) => x.SetDoEncumbrancePenaltyPhysicalLimitAsync(y, token), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyMovementSpeed.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyMovementSpeed),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDoEncumbrancePenaltyMovementSpeedAsync(token),
                (x, y) => x.SetDoEncumbrancePenaltyMovementSpeedAsync(y, token), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyAgility.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyAgility),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDoEncumbrancePenaltyAgilityAsync(token),
                (x, y) => x.SetDoEncumbrancePenaltyAgilityAsync(y, token), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyReaction.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyReaction),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDoEncumbrancePenaltyReactionAsync(token),
                (x, y) => x.SetDoEncumbrancePenaltyReactionAsync(y, token), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyReaction.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyWoundModifier),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDoEncumbrancePenaltyWoundModifierAsync(token),
                (x, y) => x.SetDoEncumbrancePenaltyWoundModifierAsync(y, token), token).ConfigureAwait(false);

            await nudEncumbrancePenaltyPhysicalLimit.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyPhysicalLimit),
                (x, y) => x.ValueChanged += y,
                x => x.GetEncumbrancePenaltyPhysicalLimitAsync(token),
                (x, y) => x.SetEncumbrancePenaltyPhysicalLimitAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyMovementSpeed.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyMovementSpeed),
                (x, y) => x.ValueChanged += y,
                x => x.GetEncumbrancePenaltyMovementSpeedAsync(token),
                (x, y) => x.SetEncumbrancePenaltyMovementSpeedAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyAgility.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyAgility),
                (x, y) => x.ValueChanged += y,
                x => x.GetEncumbrancePenaltyAgilityAsync(token),
                (x, y) => x.SetEncumbrancePenaltyAgilityAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyReaction.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyReaction),
                (x, y) => x.ValueChanged += y,
                x => x.GetEncumbrancePenaltyReactionAsync(token),
                (x, y) => x.SetEncumbrancePenaltyReactionAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyWoundModifier.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyWoundModifier),
                (x, y) => x.ValueChanged += y,
                x => x.GetEncumbrancePenaltyWoundModifierAsync(token),
                (x, y) => x.SetEncumbrancePenaltyWoundModifierAsync(y, token), 250, token, token).ConfigureAwait(false);

            await chkEnforceCapacity.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EnforceCapacity),
                (x, y) => x.CheckedChanged += y,
                x => x.GetEnforceCapacityAsync(token),
                (x, y) => x.SetEnforceCapacityAsync(y, token), token).ConfigureAwait(false);
            await chkLicenseEachRestrictedItem.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.LicenseRestricted),
                (x, y) => x.CheckedChanged += y,
                x => x.GetLicenseRestrictedAsync(token),
                (x, y) => x.SetLicenseRestrictedAsync(y, token), token).ConfigureAwait(false);
            await chkReverseAttributePriorityOrder.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ReverseAttributePriorityOrder),
                (x, y) => x.CheckedChanged += y,
                x => x.GetReverseAttributePriorityOrderAsync(token),
                (x, y) => x.SetReverseAttributePriorityOrderAsync(y, token), token).ConfigureAwait(false);
            await chkDronemods.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneMods),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDroneModsAsync(token),
                (x, y) => x.SetDroneModsAsync(y, token), token).ConfigureAwait(false);
            await chkDronemodsMaximumPilot.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneModsMaximumPilot),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDroneModsMaximumPilotAsync(token),
                (x, y) => x.SetDroneModsMaximumPilotAsync(y, token), token).ConfigureAwait(false);
            await chkRestrictRecoil.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RestrictRecoil),
                (x, y) => x.CheckedChanged += y,
                x => x.GetRestrictRecoilAsync(token),
                (x, y) => x.SetRestrictRecoilAsync(y, token), token).ConfigureAwait(false);
            await chkStrictSkillGroups.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.StrictSkillGroupsInCreateMode),
                (x, y) => x.CheckedChanged += y,
                x => x.GetStrictSkillGroupsInCreateModeAsync(token),
                (x, y) => x.SetStrictSkillGroupsInCreateModeAsync(y, token), token).ConfigureAwait(false);
            await chkAllowPointBuySpecializationsOnKarmaSkills.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowPointBuySpecializationsOnKarmaSkills),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAllowPointBuySpecializationsOnKarmaSkillsAsync(token),
                (x, y) => x.SetAllowPointBuySpecializationsOnKarmaSkillsAsync(y, token), token).ConfigureAwait(false);
            await chkAllowFreeGrids.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowFreeGrids),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAllowFreeGridsAsync(token),
                (x, y) => x.SetAllowFreeGridsAsync(y, token), token).ConfigureAwait(false);

            await chkDontUseCyberlimbCalculation.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontUseCyberlimbCalculation),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDontUseCyberlimbCalculationAsync(token),
                (x, y) => x.SetDontUseCyberlimbCalculationAsync(y, token), token).ConfigureAwait(false);
            await chkCyberlegMovement.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CyberlegMovement),
                (x, y) => x.CheckedChanged += y,
                x => x.GetCyberlegMovementAsync(token),
                (x, y) => x.SetCyberlegMovementAsync(y, token), token).ConfigureAwait(false);
            await chkCyberlimbAttributeBonusCap.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride),
                (x, y) => x.CheckedChanged += y,
                x => x.GetCyberlimbAttributeBonusCapOverrideAsync(token),
                (x, y) => x.SetCyberlimbAttributeBonusCapOverrideAsync(y, token), token).ConfigureAwait(false);
            await nudCyberlimbAttributeBonusCap.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride),
                x => x.GetCyberlimbAttributeBonusCapOverrideAsync(token), token: token).ConfigureAwait(false);
            await nudCyberlimbAttributeBonusCap.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCap),
                (x, y) => x.ValueChanged += y,
                x => x.GetCyberlimbAttributeBonusCapAsync(token),
                (x, y) => x.SetCyberlimbAttributeBonusCapAsync(y, token), 250, token, token).ConfigureAwait(false);
            await chkRedlinerLimbsSkull.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesSkull),
                (x, y) => x.CheckedChanged += y,
                async x => !await x.GetRedlinerExcludesSkullAsync(token).ConfigureAwait(false),
                (x, y) => x.SetRedlinerExcludesSkullAsync(!y, token), token, token).ConfigureAwait(false);
            await chkRedlinerLimbsTorso.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesTorso),
                (x, y) => x.CheckedChanged += y,
                async x => !await x.GetRedlinerExcludesTorsoAsync(token).ConfigureAwait(false),
                (x, y) => x.SetRedlinerExcludesTorsoAsync(!y, token), token, token).ConfigureAwait(false);
            await chkRedlinerLimbsArms.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesArms),
                (x, y) => x.CheckedChanged += y,
                async x => !await x.GetRedlinerExcludesArmsAsync(token).ConfigureAwait(false),
                (x, y) => x.SetRedlinerExcludesArmsAsync(!y, token), token, token).ConfigureAwait(false);
            await chkRedlinerLimbsLegs.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesLegs),
                (x, y) => x.CheckedChanged += y,
                async x => !await x.GetRedlinerExcludesLegsAsync(token).ConfigureAwait(false),
                (x, y) => x.SetRedlinerExcludesLegsAsync(!y, token), token, token).ConfigureAwait(false);

            await nudNuyenDecimalsMaximum.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxNuyenDecimals),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxNuyenDecimalsAsync(token),
                (x, y) => x.SetMaxNuyenDecimalsAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudNuyenDecimalsMinimum.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinNuyenDecimals),
                (x, y) => x.ValueChanged += y,
                x => x.GetMinNuyenDecimalsAsync(token),
                (x, y) => x.SetMinNuyenDecimalsAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudEssenceDecimals.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EssenceDecimals),
                (x, y) => x.ValueChanged += y,
                x => x.GetEssenceDecimalsAsync(token),
                (x, y) => x.SetEssenceDecimalsAsync(y, token), 250, token, token).ConfigureAwait(false);
            await chkDontRoundEssenceInternally.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontRoundEssenceInternally),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDontRoundEssenceInternallyAsync(token),
                (x, y) => x.SetDontRoundEssenceInternallyAsync(y, token), token).ConfigureAwait(false);

            await nudMinInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMinInitiativeDiceAsync(token),
                (x, y) => x.SetMinInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinInitiativeDice),
                x => x.GetMinInitiativeDiceAsync(token), token: token).ConfigureAwait(false);
            await nudMaxInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxInitiativeDiceAsync(token),
                (x, y) => x.SetMaxInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMinAstralInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinAstralInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMinAstralInitiativeDiceAsync(token),
                (x, y) => x.SetMinAstralInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxAstralInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinAstralInitiativeDice),
                x => x.GetMinAstralInitiativeDiceAsync(token), token: token).ConfigureAwait(false);
            await nudMaxAstralInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxAstralInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxAstralInitiativeDiceAsync(token),
                (x, y) => x.SetMaxAstralInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMinColdSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinColdSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMinColdSimInitiativeDiceAsync(token),
                (x, y) => x.SetMinColdSimInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxColdSimInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinColdSimInitiativeDice),
                x => x.GetMinColdSimInitiativeDiceAsync(token), token: token).ConfigureAwait(false);
            await nudMaxColdSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxColdSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxColdSimInitiativeDiceAsync(token),
                (x, y) => x.SetMaxColdSimInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMinHotSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinHotSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMinHotSimInitiativeDiceAsync(token),
                (x, y) => x.SetMinHotSimInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxHotSimInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinHotSimInitiativeDice),
                x => x.GetMinHotSimInitiativeDiceAsync(token), token: token).ConfigureAwait(false);
            await nudMaxHotSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxHotSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxHotSimInitiativeDiceAsync(token),
                (x, y) => x.SetMaxHotSimInitiativeDiceAsync(y, token), 250, token, token).ConfigureAwait(false);

            await chkEnable4eStyleEnemyTracking.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                (x, y) => x.CheckedChanged += y,
                x => x.GetEnableEnemyTrackingAsync(token),
                (x, y) => x.SetEnableEnemyTrackingAsync(y, token), token).ConfigureAwait(false);
            await flpKarmaGainedFromEnemies.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                x => x.GetEnableEnemyTrackingAsync(token), token: token).ConfigureAwait(false);
            await nudKarmaGainedFromEnemies.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaEnemy),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaEnemyAsync(token),
                (x, y) => x.SetKarmaEnemyAsync(y, token), 250, token, token).ConfigureAwait(false);
            await chkEnemyKarmaQualityLimit.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                x => x.GetEnableEnemyTrackingAsync(token), token: token).ConfigureAwait(false);
            await chkEnemyKarmaQualityLimit.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EnemyKarmaQualityLimit),
                (x, y) => x.CheckedChanged += y,
                x => x.GetEnemyKarmaQualityLimitAsync(token),
                (x, y) => x.SetEnemyKarmaQualityLimitAsync(y, token), token).ConfigureAwait(false);
            await chkMoreLethalGameplay.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MoreLethalGameplay),
                (x, y) => x.CheckedChanged += y,
                x => x.GetMoreLethalGameplayAsync(token),
                (x, y) => x.SetMoreLethalGameplayAsync(y, token), token).ConfigureAwait(false);

            await chkNoArmorEncumbrance.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NoArmorEncumbrance),
                (x, y) => x.CheckedChanged += y,
                x => x.GetNoArmorEncumbranceAsync(token),
                (x, y) => x.SetNoArmorEncumbranceAsync(y, token), token).ConfigureAwait(false);
            await chkUncappedArmorAccessoryBonuses.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UncappedArmorAccessoryBonuses),
                (x, y) => x.CheckedChanged += y,
                x => x.GetUncappedArmorAccessoryBonusesAsync(token),
                (x, y) => x.SetUncappedArmorAccessoryBonusesAsync(y, token), token).ConfigureAwait(false);
            await chkIgnoreArt.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.IgnoreArt),
                (x, y) => x.CheckedChanged += y,
                x => x.GetIgnoreArtAsync(token),
                (x, y) => x.SetIgnoreArtAsync(y, token), token).ConfigureAwait(false);
            await chkIgnoreComplexFormLimit.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.IgnoreComplexFormLimit),
                (x, y) => x.CheckedChanged += y,
                x => x.GetIgnoreComplexFormLimitAsync(token),
                (x, y) => x.SetIgnoreComplexFormLimitAsync(y, token), token).ConfigureAwait(false);
            await chkUnarmedSkillImprovements.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UnarmedImprovementsApplyToWeapons),
                (x, y) => x.CheckedChanged += y,
                x => x.GetUnarmedImprovementsApplyToWeaponsAsync(token),
                (x, y) => x.SetUnarmedImprovementsApplyToWeaponsAsync(y, token), token).ConfigureAwait(false);
            await chkMysAdPp.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptAllowPpCareer),
                (x, y) => x.CheckedChanged += y,
                x => x.GetMysAdeptAllowPpCareerAsync(token),
                (x, y) => x.SetMysAdeptAllowPpCareerAsync(y, token), token).ConfigureAwait(false);
            await chkMysAdPp.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = !y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                x => x.GetMysAdeptSecondMAGAttributeAsync(token), token: token).ConfigureAwait(false);
            await chkPrioritySpellsAsAdeptPowers.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.PrioritySpellsAsAdeptPowers),
                (x, y) => x.CheckedChanged += y,
                x => x.GetPrioritySpellsAsAdeptPowersAsync(token),
                (x, y) => x.SetPrioritySpellsAsAdeptPowersAsync(y, token), token).ConfigureAwait(false);
            await chkPrioritySpellsAsAdeptPowers.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = !y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                x => x.GetMysAdeptSecondMAGAttributeAsync(token), token: token).ConfigureAwait(false);
            await chkMysAdeptSecondMAGAttribute.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                (x, y) => x.CheckedChanged += y,
                x => x.GetMysAdeptSecondMAGAttributeAsync(token),
                (x, y) => x.SetMysAdeptSecondMAGAttributeAsync(y, token), token).ConfigureAwait(false);
            await chkMysAdeptSecondMAGAttribute.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttributeEnabled),
                x => x.GetMysAdeptSecondMAGAttributeEnabledAsync(token), token: token).ConfigureAwait(false);
            await chkUsePointsOnBrokenGroups.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UsePointsOnBrokenGroups),
                (x, y) => x.CheckedChanged += y,
                x => x.GetUsePointsOnBrokenGroupsAsync(token),
                (x, y) => x.SetUsePointsOnBrokenGroupsAsync(y, token), token).ConfigureAwait(false);
            await chkSpecialKarmaCost.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.SpecialKarmaCostBasedOnShownValue),
                (x, y) => x.CheckedChanged += y,
                x => x.GetSpecialKarmaCostBasedOnShownValueAsync(token),
                (x, y) => x.SetSpecialKarmaCostBasedOnShownValueAsync(y, token), token).ConfigureAwait(false);
            await chkUseCalculatedPublicAwareness.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UseCalculatedPublicAwareness),
                (x, y) => x.CheckedChanged += y,
                x => x.GetUseCalculatedPublicAwarenessAsync(token),
                (x, y) => x.SetUseCalculatedPublicAwarenessAsync(y, token), token).ConfigureAwait(false);
            await chkAlternateMetatypeAttributeKarma.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AlternateMetatypeAttributeKarma),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAlternateMetatypeAttributeKarmaAsync(token),
                (x, y) => x.SetAlternateMetatypeAttributeKarmaAsync(y, token), token).ConfigureAwait(false);
            await chkCompensateSkillGroupKarmaDifference.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CompensateSkillGroupKarmaDifference),
                (x, y) => x.CheckedChanged += y,
                x => x.GetCompensateSkillGroupKarmaDifferenceAsync(token),
                (x, y) => x.SetCompensateSkillGroupKarmaDifferenceAsync(y, token), token).ConfigureAwait(false);
            await chkFreeMartialArtSpecialization.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.FreeMartialArtSpecialization),
                (x, y) => x.CheckedChanged += y,
                x => x.GetFreeMartialArtSpecializationAsync(token),
                (x, y) => x.SetFreeMartialArtSpecializationAsync(y, token), token).ConfigureAwait(false);
            await chkIncreasedImprovedAbilityModifier.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.IncreasedImprovedAbilityMultiplier),
                (x, y) => x.CheckedChanged += y,
                x => x.GetIncreasedImprovedAbilityMultiplierAsync(token),
                (x, y) => x.SetIncreasedImprovedAbilityMultiplierAsync(y, token), token).ConfigureAwait(false);
            await chkAllowTechnomancerSchooling.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowTechnomancerSchooling),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAllowTechnomancerSchoolingAsync(token),
                (x, y) => x.SetAllowTechnomancerSchoolingAsync(y, token), token).ConfigureAwait(false);
            await chkAllowSkillRegrouping.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowSkillRegrouping),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAllowSkillRegroupingAsync(token),
                (x, y) => x.SetAllowSkillRegroupingAsync(y, token), token).ConfigureAwait(false);
            await chkSpecializationsBreakSkillGroups.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.SpecializationsBreakSkillGroups),
                (x, y) => x.CheckedChanged += y,
                x => x.GetSpecializationsBreakSkillGroupsAsync(token),
                (x, y) => x.SetSpecializationsBreakSkillGroupsAsync(y, token), token).ConfigureAwait(false);
            await chkDontDoubleQualityPurchases.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontDoubleQualityPurchases),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDontDoubleQualityPurchasesAsync(token),
                (x, y) => x.SetDontDoubleQualityPurchasesAsync(y, token), token).ConfigureAwait(false);
            await chkDontDoubleQualityRefunds.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontDoubleQualityRefunds),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDontDoubleQualityRefundsAsync(token),
                (x, y) => x.SetDontDoubleQualityRefundsAsync(y, token), token).ConfigureAwait(false);
            await chkDroneArmorMultiplier.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplierEnabled),
                (x, y) => x.CheckedChanged += y,
                x => x.GetDroneArmorMultiplierEnabledAsync(token),
                (x, y) => x.SetDroneArmorMultiplierEnabledAsync(y, token), token).ConfigureAwait(false);
            await nudDroneArmorMultiplier.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplierEnabled),
                x => x.GetDroneArmorMultiplierEnabledAsync(token), token: token).ConfigureAwait(false);
            await nudDroneArmorMultiplier.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplier),
                (x, y) => x.ValueChanged += y,
                x => x.GetDroneArmorMultiplierAsync(token),
                (x, y) => x.SetDroneArmorMultiplierAsync(y, token), 250, token, token).ConfigureAwait(false);
            await chkESSLossReducesMaximumOnly.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ESSLossReducesMaximumOnly),
                (x, y) => x.CheckedChanged += y,
                x => x.GetESSLossReducesMaximumOnlyAsync(token),
                (x, y) => x.SetESSLossReducesMaximumOnlyAsync(y, token), token).ConfigureAwait(false);
            await chkExceedNegativeQualities.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualities),
                (x, y) => x.CheckedChanged += y,
                x => x.GetExceedNegativeQualitiesAsync(token),
                (x, y) => x.SetExceedNegativeQualitiesAsync(y, token), token).ConfigureAwait(false);
            await chkExceedNegativeQualitiesNoBonus.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualities),
                x => x.GetExceedNegativeQualitiesAsync(token), token: token).ConfigureAwait(false);
            await chkExceedNegativeQualitiesNoBonus.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualitiesNoBonus),
                (x, y) => x.CheckedChanged += y,
                x => x.GetExceedNegativeQualitiesNoBonusAsync(token),
                (x, y) => x.SetExceedNegativeQualitiesNoBonusAsync(y, token), token).ConfigureAwait(false);
            await chkExceedPositiveQualities.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualities),
                (x, y) => x.CheckedChanged += y,
                x => x.GetExceedPositiveQualitiesAsync(token),
                (x, y) => x.SetExceedPositiveQualitiesAsync(y, token), token).ConfigureAwait(false);
            await chkExceedPositiveQualitiesCostDoubled.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualities),
                x => x.GetExceedPositiveQualitiesAsync(token), token: token).ConfigureAwait(false);
            await chkExceedPositiveQualitiesCostDoubled.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualitiesCostDoubled),
                (x, y) => x.CheckedChanged += y,
                x => x.GetExceedPositiveQualitiesCostDoubledAsync(token),
                (x, y) => x.SetExceedPositiveQualitiesCostDoubledAsync(y, token), token).ConfigureAwait(false);
            await chkExtendAnyDetectionSpell.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExtendAnyDetectionSpell),
                (x, y) => x.CheckedChanged += y,
                x => x.GetExtendAnyDetectionSpellAsync(token),
                (x, y) => x.SetExtendAnyDetectionSpellAsync(y, token), token).ConfigureAwait(false);
            await chkAllowCyberwareESSDiscounts.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowCyberwareESSDiscounts),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAllowCyberwareESSDiscountsAsync(token),
                (x, y) => x.SetAllowCyberwareESSDiscountsAsync(y, token), token).ConfigureAwait(false);
            await chkAllowInitiation.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowInitiationInCreateMode),
                (x, y) => x.CheckedChanged += y,
                x => x.GetAllowInitiationInCreateModeAsync(token),
                (x, y) => x.SetAllowInitiationInCreateModeAsync(y, token),
                token,
                token).ConfigureAwait(false);
            await nudMaxSkillRating.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxSkillRating),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxSkillRatingAsync(token),
                (x, y) => x.SetMaxKnowledgeSkillRatingAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudMaxKnowledgeSkillRating.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxKnowledgeSkillRating),
                (x, y) => x.ValueChanged += y,
                x => x.GetMaxKnowledgeSkillRatingAsync(token),
                (x, y) => x.SetMaxKnowledgeSkillRatingAsync(y, token), 250, token, token).ConfigureAwait(false);

            // Karma options.
            await nudMetatypeCostsKarmaMultiplier.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MetatypeCostsKarmaMultiplier),
                (x, y) => x.ValueChanged += y,
                x => x.GetMetatypeCostsKarmaMultiplierAsync(token),
                (x, y) => x.SetMetatypeCostsKarmaMultiplierAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNuyenPerWftM.RegisterAsyncDataBindingWithDelayAsync(x => x.Value,
                (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenPerBPWftM),
                (x, y) => x.ValueChanged += y,
                x => x.GetNuyenPerBPWftMAsync(token),
                (x, y) => x.SetNuyenPerBPWftMAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNuyenPerWftP.RegisterAsyncDataBindingWithDelayAsync(x => x.Value,
                (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenPerBPWftP),
                (x, y) => x.ValueChanged += y,
                x => x.GetNuyenPerBPWftPAsync(token),
                (x, y) => x.SetNuyenPerBPWftPAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaAttribute.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaAttribute),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaAttributeAsync(token),
                (x, y) => x.SetKarmaAttributeAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaQuality.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaQuality),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaQualityAsync(token),
                (x, y) => x.SetKarmaQualityAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpecialization.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpecialization),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSpecializationAsync(token),
                (x, y) => x.SetKarmaSpecializationAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaKnowledgeSpecialization.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaKnowledgeSpecialization),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaKnowledgeSpecializationAsync(token),
                (x, y) => x.SetKarmaKnowledgeSpecializationAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewKnowledgeSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewKnowledgeSkill),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaNewKnowledgeSkillAsync(token),
                (x, y) => x.SetKarmaNewKnowledgeSkillAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewActiveSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewActiveSkill),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaNewActiveSkillAsync(token),
                (x, y) => x.SetKarmaNewActiveSkillAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewSkillGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewSkillGroup),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaNewSkillGroupAsync(token),
                (x, y) => x.SetKarmaNewSkillGroupAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaImproveKnowledgeSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaImproveKnowledgeSkill),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaImproveKnowledgeSkillAsync(token),
                (x, y) => x.SetKarmaImproveKnowledgeSkillAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaImproveActiveSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaImproveActiveSkill),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaImproveActiveSkillAsync(token),
                (x, y) => x.SetKarmaImproveActiveSkillAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaImproveSkillGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaImproveSkillGroup),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaImproveSkillGroupAsync(token),
                (x, y) => x.SetKarmaImproveSkillGroupAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpell.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpell),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSpellAsync(token),
                (x, y) => x.SetKarmaSpellAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewComplexForm.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewComplexForm),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaNewComplexFormAsync(token),
                (x, y) => x.SetKarmaNewComplexFormAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewAIProgram.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewAIProgram),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaNewAIProgramAsync(token),
                (x, y) => x.SetKarmaNewAIProgramAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewAIAdvancedProgram.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewAIAdvancedProgram),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaNewAIAdvancedProgramAsync(token),
                (x, y) => x.SetKarmaNewAIAdvancedProgramAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaMetamagic.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaMetamagic),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaMetamagicAsync(token),
                (x, y) => x.SetKarmaMetamagicAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaContact.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaContact),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaContactAsync(token),
                (x, y) => x.SetKarmaContactAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpirit.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpirit),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSpiritAsync(token),
                (x, y) => x.SetKarmaSpiritAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpiritFettering.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpiritFettering),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSpiritFetteringAsync(token),
                (x, y) => x.SetKarmaSpiritFetteringAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaTechnique.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaTechnique),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaTechniqueAsync(token),
                (x, y) => x.SetKarmaTechniqueAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaInitiation.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaInitiation),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaInitiationAsync(token),
                (x, y) => x.SetKarmaInitiationAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaInitiationFlat.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaInitiationFlat),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaInitiationFlatAsync(token),
                (x, y) => x.SetKarmaInitiationFlatAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaJoinGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaJoinGroup),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaJoinGroupAsync(token),
                (x, y) => x.SetKarmaJoinGroupAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaLeaveGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaLeaveGroup),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaLeaveGroupAsync(token),
                (x, y) => x.SetKarmaLeaveGroupAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaMysticAdeptPowerPoint.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaMysticAdeptPowerPoint),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaMysticAdeptPowerPointAsync(token),
                (x, y) => x.SetKarmaMysticAdeptPowerPointAsync(y, token), 250, token, token).ConfigureAwait(false);

            // Focus costs
            await nudKarmaAlchemicalFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaAlchemicalFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaAlchemicalFocusAsync(token),
                (x, y) => x.SetKarmaAlchemicalFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaBanishingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaBanishingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaBanishingFocusAsync(token),
                (x, y) => x.SetKarmaBanishingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaBindingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaBindingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaBindingFocusAsync(token),
                (x, y) => x.SetKarmaBindingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaCenteringFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaCenteringFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaCenteringFocusAsync(token),
                (x, y) => x.SetKarmaCenteringFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaCounterspellingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaCounterspellingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaCounterspellingFocusAsync(token),
                (x, y) => x.SetKarmaCounterspellingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaDisenchantingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaDisenchantingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaDisenchantingFocusAsync(token),
                (x, y) => x.SetKarmaDisenchantingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaFlexibleSignatureFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaFlexibleSignatureFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaFlexibleSignatureFocusAsync(token),
                (x, y) => x.SetKarmaFlexibleSignatureFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaMaskingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaMaskingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaMaskingFocusAsync(token),
                (x, y) => x.SetKarmaMaskingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaPowerFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaPowerFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaPowerFocusAsync(token),
                (x, y) => x.SetKarmaPowerFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaQiFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaQiFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaQiFocusAsync(token),
                (x, y) => x.SetKarmaQiFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaRitualSpellcastingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaRitualSpellcastingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaRitualSpellcastingFocusAsync(token),
                (x, y) => x.SetKarmaRitualSpellcastingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpellcastingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpellcastingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSpellcastingFocusAsync(token),
                (x, y) => x.SetKarmaSpellcastingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpellShapingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpellShapingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSpellShapingFocusAsync(token),
                (x, y) => x.SetKarmaSpellShapingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSummoningFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSummoningFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSummoningFocusAsync(token),
                (x, y) => x.SetKarmaSummoningFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaSustainingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSustainingFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaSustainingFocusAsync(token),
                (x, y) => x.SetKarmaSustainingFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
            await nudKarmaWeaponFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaWeaponFocus),
                (x, y) => x.ValueChanged += y,
                x => x.GetKarmaWeaponFocusAsync(token),
                (x, y) => x.SetKarmaWeaponFocusAsync(y, token), 250, token, token).ConfigureAwait(false);
        }

        private async Task PopulateSettingsList(CancellationToken token = default)
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

        private async Task SettingsChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                if (Interlocked.CompareExchange(ref _intLoading, 1, 0) == 0)
                {
                    try
                    {
                        await SetIsDirtyAsync(!await _objCharacterSettings
                                                .HasIdenticalSettingsAsync(_objReferenceCharacterSettings, token)
                                                .ConfigureAwait(false), token).ConfigureAwait(false);
                        if (e.PropertyNames.Contains(nameof(CharacterSettings.EnabledCustomDataDirectoryPaths)))
                            await PopulateOptions(token).ConfigureAwait(false);
                        else if (e.PropertyNames.Contains(nameof(CharacterSettings.PriorityTable)))
                            await PopulatePriorityTableList(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                }
                else
                {
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.BuiltInOption)))
                    {
                        bool blnAllTextBoxesLegal = await IsAllTextBoxesLegalAsync(token).ConfigureAwait(false);
                        await cmdSave.DoThreadSafeAsync(
                            x => x.Enabled = IsDirty && blnAllTextBoxesLegal
                                                     && !_objCharacterSettings.BuiltInOption, token: token).ConfigureAwait(false);
                    }
                    else if (e.PropertyNames.Contains(nameof(CharacterSettings.PriorityArray))
                             || e.PropertyNames.Contains(nameof(CharacterSettings.BuildMethod)))
                    {
                        bool blnAllTextBoxesLegal = await IsAllTextBoxesLegalAsync(token).ConfigureAwait(false);
                        await cmdSaveAs.DoThreadSafeAsync(x => x.Enabled = IsDirty && blnAllTextBoxesLegal, token: token)
                            .ConfigureAwait(false);
                        await cmdSave.DoThreadSafeAsync(
                            x => x.Enabled = IsDirty && blnAllTextBoxesLegal
                                                     && !_objCharacterSettings.BuiltInOption, token: token).ConfigureAwait(false);
                    }
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.ChargenKarmaToNuyenExpression)))
                    {
                        string strText = await _objCharacterSettings.GetChargenKarmaToNuyenExpressionAsync(token).ConfigureAwait(false);
                        await txtNuyenExpression.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
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

        private async Task<bool> IsAllTextBoxesLegalAsync(CancellationToken token = default)
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
            get => _intDirty > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intDirty, intNewValue) == intNewValue)
                    return;
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

        private async Task SetIsDirtyAsync(bool value, CancellationToken token = default)
        {
            int intNewValue = value.ToInt32();
            if (Interlocked.Exchange(ref _intDirty, intNewValue) == intNewValue)
                return;
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
