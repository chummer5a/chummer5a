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
using Chummer.Backend.Enums;
using NLog;

namespace Chummer
{
    public partial class EditCharacterSettings : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly CharacterSettings _objCharacterSettings;
        private CharacterSettings _objReferenceCharacterSettings;
        private List<ListItem> _lstSettings;

        // List of custom data directory infos on the character, in load order. If the character has a directory name for which we have no info, key will be a string instead of an info
        private readonly LockingTypedOrderedDictionary<string, bool> _dicEnabledCharacterCustomDataDirectorys;

        private int _intLoading = 1;
        private int _intSkipLimbCountUpdate;
        private int _intDirty;
        private bool _blnSourcebookToggle = true;
        private bool _blnWasRenamed;
        private int _intSuspendLayoutCount;
        private bool _blnForceMasterIndexRepopulateOnClose;

        // Used to revert to old selected setting if user cancels out of selecting a different one
        private int _intOldSelectedSettingIndex = -1;

        private HashSet<string> _setPermanentSourcebooks;

        #region Form Events

        public EditCharacterSettings(CharacterSettings objExistingSettings = null)
        {
            InitializeComponent();
            tabOptions.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
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

            _dicEnabledCharacterCustomDataDirectorys = new LockingTypedOrderedDictionary<string, bool>();
            _objCharacterSettings = new CharacterSettings(_objReferenceCharacterSettings);
            _objCharacterSettings.MultiplePropertiesChangedAsync += SettingsChanged;
            _lstSettings = Utils.ListItemListPool.Get();
            _setPermanentSourcebooks = Utils.StringHashSetPool.Get();
        }

        private async void EditCharacterSettings_Load(object sender, EventArgs e)
        {
            await RebuildCustomDataDirectoryInfosAsync().ConfigureAwait(false);
            await SetToolTips().ConfigureAwait(false);
            await PopulateSettingsList().ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstBuildMethods))
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
                                GlobalSettings.CultureInfo,
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
                        strSeparator += "_";
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
                    try
                    {
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
                    }
                    catch
                    {
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
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
                if (await _objReferenceCharacterSettings.GetBuildMethodAsync().ConfigureAwait(false) != await _objCharacterSettings.GetBuildMethodAsync().ConfigureAwait(false))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdConflictingCharacters))
                    {
                        await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                        {
                            if (!await objCharacter.GetCreatedAsync().ConfigureAwait(false)
                                && ReferenceEquals(await objCharacter.GetSettingsAsync().ConfigureAwait(false),
                                    _objReferenceCharacterSettings))
                                sbdConflictingCharacters.AppendLine(await objCharacter.GetCharacterNameAsync().ConfigureAwait(false));
                        }).ConfigureAwait(false);

                        if (sbdConflictingCharacters.Length > 0)
                        {
                            await Program.ShowScrollableMessageBoxAsync(this,
                                await LanguageManager.GetStringAsync(
                                        "Message_CharacterOptions_OpenCharacterOnBuildMethodChange")
                                    .ConfigureAwait(false)
                                +
                                sbdConflictingCharacters.ToString(),
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

                _intOldSelectedSettingIndex = await cboSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false);
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

        private async void cboLimbCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intSkipLimbCountUpdate > 0)
                return;

            string strLimbCount = await cboLimbCount.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strLimbCount))
            {
                await _objCharacterSettings.SetLimbCountAsync(6).ConfigureAwait(false);
                await _objCharacterSettings.SetExcludeLimbSlotAsync(string.Empty).ConfigureAwait(false);
            }
            else
            {
                int intSeparatorIndex = strLimbCount.IndexOf('<');
                if (intSeparatorIndex == -1)
                {
                    if (int.TryParse(strLimbCount, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                     out int intLimbCount))
                        await _objCharacterSettings.SetLimbCountAsync(intLimbCount).ConfigureAwait(false);
                    else
                    {
                        Utils.BreakIfDebug();
                        await _objCharacterSettings.SetLimbCountAsync(6).ConfigureAwait(false);
                    }

                    await _objCharacterSettings.SetExcludeLimbSlotAsync(string.Empty).ConfigureAwait(false);
                }
                else
                {
                    if (int.TryParse(strLimbCount.Substring(0, intSeparatorIndex), NumberStyles.Any,
                                     GlobalSettings.InvariantCultureInfo, out int intLimbCount))
                    {
                        await _objCharacterSettings.SetLimbCountAsync(intLimbCount).ConfigureAwait(false);
                        await _objCharacterSettings.SetExcludeLimbSlotAsync(intSeparatorIndex + 1 < strLimbCount.Length
                            ? strLimbCount.Substring(intSeparatorIndex + 1)
                            : string.Empty).ConfigureAwait(false);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                        await _objCharacterSettings.SetLimbCountAsync(6).ConfigureAwait(false);
                        await _objCharacterSettings.SetExcludeLimbSlotAsync(string.Empty).ConfigureAwait(false);
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

                if (_blnForceMasterIndexRepopulateOnClose)
                {
                    MasterIndex frmMasterIndex = Program.MainForm?.MasterIndex;
                    if (frmMasterIndex != null)
                        await frmMasterIndex.ForceRepopulateCharacterSettings().ConfigureAwait(false);
                }

                // Now we close the original caller (weird async FormClosing event issue workaround)
                if (frmSender != null)
                {
                    await frmSender.DoThreadSafeAsync(x =>
                    {
                        x.FormClosing -= EditCharacterSettings_FormClosing;
                        x.Close();
                    }).ConfigureAwait(false);
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
            await _dicEnabledCharacterCustomDataDirectorys.ReverseAsync(intIndex - 1, 2).ConfigureAwait(false);
            await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).ReverseAsync(intIndex - 1, 2).ConfigureAwait(false);
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
            IAsyncDisposable objLocker = await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                IAsyncDisposable objLocker2
                    = await _dicEnabledCharacterCustomDataDirectorys.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    for (int i = intIndex; i > 0; --i)
                    {
                        await _dicEnabledCharacterCustomDataDirectorys.ReverseAsync(i - 1, 2).ConfigureAwait(false);
                        await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).ReverseAsync(i - 1, 2).ConfigureAwait(false);
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
            if (intIndex >= await _dicEnabledCharacterCustomDataDirectorys.GetCountAsync().ConfigureAwait(false) - 1)
                return;

            IAsyncDisposable objLocker = await _dicEnabledCharacterCustomDataDirectorys.LockObject.EnterUpgradeableReadLockAsync().ConfigureAwait(false);
            try
            {
                if (intIndex >= await _dicEnabledCharacterCustomDataDirectorys.GetCountAsync().ConfigureAwait(false) - 1)
                    return;
                IAsyncDisposable objLocker2 = await _dicEnabledCharacterCustomDataDirectorys.LockObject
                    .EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    await _dicEnabledCharacterCustomDataDirectorys.ReverseAsync(intIndex, 2).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).ReverseAsync(intIndex, 2).ConfigureAwait(false);
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
            if (intIndex >= await _dicEnabledCharacterCustomDataDirectorys.GetCountAsync().ConfigureAwait(false) - 1)
                    return;

            IAsyncDisposable objLocker = await _dicEnabledCharacterCustomDataDirectorys.LockObject.EnterUpgradeableReadLockAsync().ConfigureAwait(false);
            try
            {
                int intCount = await _dicEnabledCharacterCustomDataDirectorys.GetCountAsync().ConfigureAwait(false);
                if (intIndex >= intCount - 1)
                    return;
                IAsyncDisposable objLocker2 = await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).LockObject
                    .EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    IAsyncDisposable objLocker3 = await _dicEnabledCharacterCustomDataDirectorys.LockObject
                        .EnterWriteLockAsync().ConfigureAwait(false);
                    try
                    {
                        for (int i = intIndex; i < intCount - 1; ++i)
                        {
                            await _dicEnabledCharacterCustomDataDirectorys.ReverseAsync(i, 2).ConfigureAwait(false);
                            await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).ReverseAsync(i, 2)
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
            await _dicEnabledCharacterCustomDataDirectorys.SetValueAtAsync(intIndex, blnChecked).ConfigureAwait(false);
            switch (objNode.Tag)
            {
                case CustomDataDirectoryInfo objCustomDataDirectoryInfo:
                    if (await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).TryUpdateAsync(objCustomDataDirectoryInfo.CharacterSettingsSaveKey, blnChecked).ConfigureAwait(false))
                        await _objCharacterSettings.OnPropertyChangedAsync(nameof(CharacterSettings.CustomDataDirectoryKeys)).ConfigureAwait(false);
                    break;
                case string strCustomDataDirectoryKey:
                    if (await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync().ConfigureAwait(false)).TryUpdateAsync(strCustomDataDirectoryKey, blnChecked).ConfigureAwait(false))
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

        private void txtGameplayOptionName_TextChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            _objCharacterSettings.GameplayOptionName = txtGameplayOptionName.Text;
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

        private async void cboPriorityTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            string strNewPriorityTable = await cboPriorityTable.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(strNewPriorityTable))
                return;
            await _objCharacterSettings.SetPriorityTableAsync(strNewPriorityTable).ConfigureAwait(false);
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
                string strDescription = await objSelected.GetCurrentDisplayDescriptionAsync().ConfigureAwait(false);
                await rtbDirectoryDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
                await lblDirectoryVersion.DoThreadSafeAsync(x => x.Text = objSelected.MyVersion.ToString())
                                         .ConfigureAwait(false);
                string strAuthors = await objSelected.GetCurrentDisplayAuthorsAsync().ConfigureAwait(false);
                await lblDirectoryAuthors.DoThreadSafeAsync(x => x.Text = strAuthors).ConfigureAwait(false);
                string strName = await objSelected.GetCurrentDisplayNameAsync().ConfigureAwait(false);
                await lblDirectoryName.DoThreadSafeAsync(x => x.Text = strName).ConfigureAwait(false);

                if (objSelected.DependenciesList.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDependencies))
                    {
                        foreach (DirectoryDependency dependency in objSelected.DependenciesList)
                            sbdDependencies.AppendLine(dependency.CurrentDisplayName);
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
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdIncompatibilities))
                    {
                        foreach (DirectoryDependency exclusivity in objSelected.IncompatibilitiesList)
                            sbdIncompatibilities.AppendLine(exclusivity.CurrentDisplayName);
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
                                                                await _objCharacterSettings.GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false),
                                                                token: token).ConfigureAwait(false))
                                                            .SelectAndCacheExpression(
                                                                "/chummer/books/book", token: token))
                {
                    if (objXmlBook.SelectSingleNodeAndCacheExpression("hide", token: token) != null)
                        continue;
                    string strCode = objXmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                    if (string.IsNullOrEmpty(strCode))
                        continue;
                    bool blnChecked = (await _objCharacterSettings.GetBooksAsync(token).ConfigureAwait(false)).Contains(strCode);
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
                IAsyncDisposable objLocker = await _dicEnabledCharacterCustomDataDirectorys.LockObject.EnterUpgradeableReadLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    int intNewCount
                        = await _dicEnabledCharacterCustomDataDirectorys.GetCountAsync(token).ConfigureAwait(false);
                    if (intNewCount != await treCustomDataDirectories
                            .DoThreadSafeFuncAsync(x => x.Nodes.Count, token: token)
                            .ConfigureAwait(false))
                    {
                        // Group by GUID to deduplicate and show only one entry per GUID
                        Dictionary<string, KeyValuePair<string, bool>> dicDeduplicatedByGuid =
                            new Dictionary<string, KeyValuePair<string, bool>>(StringComparer.OrdinalIgnoreCase);
                        await _dicEnabledCharacterCustomDataDirectorys.ForEachAsync(kvpKeyAndEnabled =>
                        {
                            string strGuid = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(kvpKeyAndEnabled.Key);
                            if (string.IsNullOrEmpty(strGuid))
                            {
                                // For entries without GUID, use the key itself
                                if (!dicDeduplicatedByGuid.ContainsKey(kvpKeyAndEnabled.Key))
                                    dicDeduplicatedByGuid.Add(kvpKeyAndEnabled.Key, kvpKeyAndEnabled);
                            }
                            else
                            {
                                // For entries with GUID, keep only one (the highest version is already selected in CharacterSettings)
                                if (!dicDeduplicatedByGuid.TryGetValue(strGuid, out KeyValuePair<string, bool> kvpExisting))
                                {
                                    dicDeduplicatedByGuid.Add(strGuid, kvpKeyAndEnabled);
                                }
                            }
                        }, token).ConfigureAwait(false);

                        List<TreeNode> lstNodes = new List<TreeNode>(dicDeduplicatedByGuid.Count);
                        foreach (KeyValuePair<string, KeyValuePair<string, bool>> kvpDeduplicated in dicDeduplicatedByGuid)
                        {
                            KeyValuePair<string, bool> kvpKeyAndEnabled = kvpDeduplicated.Value;
                            TreeNode objNode = new TreeNode
                            {
                                Checked = kvpKeyAndEnabled.Value
                            };
                            // Use the same logic as RecalculateEnabledCustomDataDirectories to find the actual version being used
                            string strKey = kvpKeyAndEnabled.Key;
                            string strId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(
                                strKey, out ValueVersion objPreferredVersion);
                            CustomDataDirectoryInfo objInfo = null;
                            if (string.IsNullOrEmpty(strId))
                            {
                                // For entries without GUID, find by name and pick highest version
                                objInfo = GlobalSettings.CustomDataDirectoryInfos
                                    .Where(x => x.Name.Equals(strKey, StringComparison.OrdinalIgnoreCase))
                                    .OrderByDescending(x => x.MyVersion)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                // For entries with GUID, use VersionMatchScore to find best matching version
                                objInfo = GlobalSettings.CustomDataDirectoryInfos
                                    .Where(x => x.InternalId.Equals(strId, StringComparison.OrdinalIgnoreCase))
                                    .OrderByDescending(x =>
                                    {
                                        int intReturn = int.MaxValue;
                                        intReturn -= (objPreferredVersion.Build - x.MyVersion.Build).Pow(2) * 16777216;
                                        intReturn -= (objPreferredVersion.Major - x.MyVersion.Major).Pow(2) * 65536;
                                        intReturn -= (objPreferredVersion.Minor - x.MyVersion.Minor).Pow(2) * 256;
                                        intReturn -= (objPreferredVersion.Revision - x.MyVersion.Revision).Pow(2);
                                        return intReturn;
                                    })
                                    .FirstOrDefault();
                            }
                            
                            if (objInfo != null)
                            {
                                objNode.Tag = objInfo;
                                // Always show the actual version being used (from objInfo.MyVersion)
                                string strDisplayName = await objInfo.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);

                                // Check if we're using a higher version than the minimum specified
                                if (objPreferredVersion != default(ValueVersion) && objInfo.MyVersion > objPreferredVersion)
                                {
                                    // We're using a higher version than the minimum specified, indicate this
                                    string strUsingVersion = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                                    // The display name already shows the actual version, so we can add a note about the minimum requirement
                                    strDisplayName += $"{strUsingVersion}(≥{objPreferredVersion})";
                                }

                                objNode.Text = CustomDataDirectoryUpdater.AppendUpdateIndicatorToDisplayName(
                                    strDisplayName, objInfo);
                                if (objNode.Checked)
                                {
                                    // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                                    string missingDirectories = string.Empty;
                                    if (objInfo.DependenciesList.Count > 0)
                                        missingDirectories = await objInfo
                                            .CheckDependencyAsync(_objCharacterSettings, token: token)
                                            .ConfigureAwait(false);

                                    string prohibitedDirectories = string.Empty;
                                    if (objInfo.IncompatibilitiesList.Count > 0)
                                        prohibitedDirectories = await objInfo
                                            .CheckIncompatibilityAsync(
                                                _objCharacterSettings, token: token)
                                            .ConfigureAwait(false);

                                    if (!string.IsNullOrEmpty(missingDirectories)
                                        || !string.IsNullOrEmpty(prohibitedDirectories))
                                    {
                                        objNode.ToolTipText
                                            = await CustomDataDirectoryInfo.BuildIncompatibilityDependencyStringAsync(
                                                missingDirectories, prohibitedDirectories, token: token).ConfigureAwait(false);
                                        objNode.ForeColor = objErrorColor;
                                    }
                                    else if (CustomDataDirectoryUpdater.GetCachedAvailability(objInfo).IsUpdateAvailable)
                                    {
                                        objNode.ForeColor = ColorManager.Highlight;
                                    }
                                }
                            }
                            else
                            {
                                // Try to find any version of this custom data directory by GUID to show a better name
                                string strGuid = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(kvpKeyAndEnabled.Key);
                                string strDisplayText = kvpKeyAndEnabled.Key;
                                if (!string.IsNullOrEmpty(strGuid))
                                {
                                    // Look for any version of this GUID in GlobalSettings
                                    CustomDataDirectoryInfo objAnyVersion = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(
                                        x => x.InternalId.Equals(strGuid, StringComparison.OrdinalIgnoreCase));
                                    if (objAnyVersion != null)
                                    {
                                        // Found a version, show the name with the version from the key
                                        CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(kvpKeyAndEnabled.Key, out ValueVersion objMinimumVersionFromKey);
                                        if (objMinimumVersionFromKey != default(ValueVersion))
                                        {
                                            strDisplayText = $"{objAnyVersion.Name} ({objMinimumVersionFromKey})";
                                        }
                                        else
                                        {
                                            strDisplayText = objAnyVersion.Name;
                                        }
                                    }
                                }
                                
                                objNode.Tag = kvpKeyAndEnabled.Key;
                                objNode.Text = strDisplayText;
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
                        // Group by GUID to deduplicate and show only one entry per GUID
                        Dictionary<string, KeyValuePair<string, bool>> dicDeduplicatedByGuid =
                            new Dictionary<string, KeyValuePair<string, bool>>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < intNewCount; ++i)
                        {
                            KeyValuePair<string, bool> kvpKeyAndEnabled = await _dicEnabledCharacterCustomDataDirectorys
                                .GetValueAtAsync(i, token).ConfigureAwait(false);
                            string strGuid = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(kvpKeyAndEnabled.Key);
                            if (string.IsNullOrEmpty(strGuid))
                            {
                                // For entries without GUID, use the key itself
                                if (!dicDeduplicatedByGuid.ContainsKey(kvpKeyAndEnabled.Key))
                                    dicDeduplicatedByGuid.Add(kvpKeyAndEnabled.Key, kvpKeyAndEnabled);
                            }
                            else
                            {
                                // For entries with GUID, keep only one (the highest version is already selected in CharacterSettings)
                                if (!dicDeduplicatedByGuid.TryGetValue(strGuid, out KeyValuePair<string, bool> kvpExisting))
                                {
                                    dicDeduplicatedByGuid.Add(strGuid, kvpKeyAndEnabled);
                                }
                            }
                        }

                        // If deduplication resulted in fewer entries, rebuild the tree
                        int intDeduplicatedCount = dicDeduplicatedByGuid.Count;
                        int intCurrentNodeCount = await treCustomDataDirectories
                            .DoThreadSafeFuncAsync(x => x.Nodes.Count, token: token).ConfigureAwait(false);
                        if (intDeduplicatedCount != intCurrentNodeCount)
                        {
                            // Count changed, rebuild the tree
                            List<TreeNode> lstNodes = new List<TreeNode>(intDeduplicatedCount);
                            foreach (KeyValuePair<string, KeyValuePair<string, bool>> kvpDeduplicated in dicDeduplicatedByGuid)
                            {
                                KeyValuePair<string, bool> kvpKeyAndEnabled = kvpDeduplicated.Value;
                                TreeNode objNode = new TreeNode
                                {
                                    Checked = kvpKeyAndEnabled.Value
                                };
                                CustomDataDirectoryInfo objInfoRebuild = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(
                                    x => x.CharacterSettingsSaveKey == kvpKeyAndEnabled.Key);
                                if (objInfoRebuild != null)
                                {
                                    objNode.Tag = objInfoRebuild;
                                    string strRebuildDisplayName
                                        = await objInfoRebuild.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    objNode.Text = CustomDataDirectoryUpdater.AppendUpdateIndicatorToDisplayName(
                                        strRebuildDisplayName, objInfoRebuild);
                                    if (objNode.Checked)
                                    {
                                        string missingDirectories = string.Empty;
                                        if (objInfoRebuild.DependenciesList.Count > 0)
                                            missingDirectories = await objInfoRebuild
                                                .CheckDependencyAsync(_objCharacterSettings, token: token)
                                                .ConfigureAwait(false);

                                        string prohibitedDirectories = string.Empty;
                                        if (objInfoRebuild.IncompatibilitiesList.Count > 0)
                                            prohibitedDirectories = await objInfoRebuild
                                                .CheckIncompatibilityAsync(
                                                    _objCharacterSettings, token: token)
                                                .ConfigureAwait(false);

                                        if (!string.IsNullOrEmpty(missingDirectories)
                                            || !string.IsNullOrEmpty(prohibitedDirectories))
                                        {
                                            objNode.ToolTipText
                                                = await CustomDataDirectoryInfo.BuildIncompatibilityDependencyStringAsync(
                                                    missingDirectories, prohibitedDirectories, token: token).ConfigureAwait(false);
                                            objNode.ForeColor = objErrorColor;
                                        }
                                        else if (CustomDataDirectoryUpdater.GetCachedAvailability(objInfoRebuild)
                                                     .IsUpdateAvailable)
                                        {
                                            objNode.ForeColor = ColorManager.Highlight;
                                        }
                                    }
                                }
                                else
                                {
                                    // Try to find any version of this custom data directory by GUID to show a better name
                                    string strGuid = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(kvpKeyAndEnabled.Key);
                                    string strDisplayText = kvpKeyAndEnabled.Key;
                                    if (!string.IsNullOrEmpty(strGuid))
                                    {
                                        // Look for any version of this GUID in GlobalSettings
                                        CustomDataDirectoryInfo objAnyVersion = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(
                                            x => x.InternalId.Equals(strGuid, StringComparison.OrdinalIgnoreCase));
                                        if (objAnyVersion != null)
                                        {
                                            // Found a version, show the name with the version from the key
                                            CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(kvpKeyAndEnabled.Key, out ValueVersion objMinimumVersionFromKeyRebuild);
                                            if (objMinimumVersionFromKeyRebuild != default(ValueVersion))
                                            {
                                                strDisplayText = $"{objAnyVersion.Name} ({objMinimumVersionFromKeyRebuild})";
                                            }
                                            else
                                            {
                                                strDisplayText = objAnyVersion.Name;
                                            }
                                        }
                                    }
                                    
                                    objNode.Tag = kvpKeyAndEnabled.Key;
                                    objNode.Text = strDisplayText;
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

                            // Update tree nodes to match deduplicated entries
                            int intNodeIndex = 0;
                            Color objWindowTextColor = ColorManager.WindowText;
                            foreach (KeyValuePair<string, KeyValuePair<string, bool>> kvpDeduplicated in dicDeduplicatedByGuid)
                            {
                                KeyValuePair<string, bool> kvpKeyAndEnabled = kvpDeduplicated.Value;
                                int i1 = intNodeIndex;
                                TreeNode objNode = await treCustomDataDirectories
                                    .DoThreadSafeFuncAsync(x =>
                                    {
                                        if (i1 >= x.Nodes.Count)
                                            return null;
                                        TreeNode objReturn = x.Nodes[i1];
                                        objReturn.Checked = kvpKeyAndEnabled.Value;
                                        return objReturn;
                                    }, token)
                                    .ConfigureAwait(false);
                                if (objNode == null)
                                    break;
                                
                                // Use the same logic as RecalculateEnabledCustomDataDirectories to find the actual version being used
                                string strKey = kvpKeyAndEnabled.Key;
                                string strId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(
                                    strKey, out ValueVersion objPreferredVersion);
                                CustomDataDirectoryInfo objInfo = null;
                                if (string.IsNullOrEmpty(strId))
                                {
                                    // For entries without GUID, find by name and pick highest version
                                    objInfo = GlobalSettings.CustomDataDirectoryInfos
                                        .Where(x => x.Name.Equals(strKey, StringComparison.OrdinalIgnoreCase))
                                        .OrderByDescending(x => x.MyVersion)
                                        .FirstOrDefault();
                                }
                                else
                                {
                                    // For entries with GUID, use VersionMatchScore to find best matching version
                                    objInfo = GlobalSettings.CustomDataDirectoryInfos
                                        .Where(x => x.InternalId.Equals(strId, StringComparison.OrdinalIgnoreCase))
                                        .OrderByDescending(x =>
                                        {
                                            int intReturn = int.MaxValue;
                                            intReturn -= (objPreferredVersion.Build - x.MyVersion.Build).Pow(2) * 16777216;
                                            intReturn -= (objPreferredVersion.Major - x.MyVersion.Major).Pow(2) * 65536;
                                            intReturn -= (objPreferredVersion.Minor - x.MyVersion.Minor).Pow(2) * 256;
                                            intReturn -= (objPreferredVersion.Revision - x.MyVersion.Revision).Pow(2);
                                            return intReturn;
                                        })
                                        .FirstOrDefault();
                                }
                                
                                if (objInfo != null)
                                {
                                    // Always show the actual version being used (from objInfo.MyVersion)
                                    string strText = await objInfo.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    
                                    // Check if we're using a higher version than the minimum specified
                                    if (objPreferredVersion != default(ValueVersion) && objInfo.MyVersion > objPreferredVersion)
                                    {
                                        // We're using a higher version than the minimum specified, indicate this
                                        string strUsingVersion = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                                        strText += $"{strUsingVersion}(≥{objPreferredVersion})";
                                    }

                                    strText = CustomDataDirectoryUpdater.AppendUpdateIndicatorToDisplayName(strText, objInfo);
                                    
                                    await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                        {
                                            objNode.Tag = objInfo;
                                            objNode.Text = strText;
                                        }, token)
                                        .ConfigureAwait(false);
                                    if (objNode.Checked)
                                    {
                                        // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                                        string missingDirectories = string.Empty;
                                        if (objInfo.DependenciesList.Count > 0)
                                            missingDirectories = await objInfo
                                                .CheckDependencyAsync(_objCharacterSettings, token: token)
                                                .ConfigureAwait(false);

                                        string prohibitedDirectories = string.Empty;
                                        if (objInfo.IncompatibilitiesList.Count > 0)
                                            prohibitedDirectories = await objInfo
                                                .CheckIncompatibilityAsync(
                                                    _objCharacterSettings, token: token)
                                                .ConfigureAwait(false);

                                        if (!string.IsNullOrEmpty(missingDirectories)
                                            || !string.IsNullOrEmpty(prohibitedDirectories))
                                        {
                                            string strToolTip
                                                = await CustomDataDirectoryInfo.BuildIncompatibilityDependencyStringAsync(
                                                    missingDirectories, prohibitedDirectories, token: token).ConfigureAwait(false);
                                            await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                            {
                                                objNode.ToolTipText = strToolTip;
                                                objNode.ForeColor = objErrorColor;
                                            }, token: token).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            Color objForeColor = CustomDataDirectoryUpdater.GetCachedAvailability(objInfo)
                                                                     .IsUpdateAvailable
                                                ? ColorManager.Highlight
                                                : objWindowTextColor;
                                            await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                            {
                                                objNode.ToolTipText = string.Empty;
                                                objNode.ForeColor = objForeColor;
                                            }, token: token).ConfigureAwait(false);
                                        }
                                    }
                                    else
                                    {
                                        Color objForeColor = CustomDataDirectoryUpdater.GetCachedAvailability(objInfo)
                                                                 .IsUpdateAvailable
                                            ? ColorManager.Highlight
                                            : objWindowTextColor;
                                        await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                        {
                                            objNode.ToolTipText = string.Empty;
                                            objNode.ForeColor = objForeColor;
                                        }, token: token).ConfigureAwait(false);
                                    }
                                    
                                    intNodeIndex++;
                                }
                                else
                                {
                                    await treCustomDataDirectories.DoThreadSafeAsync(() =>
                                    {
                                        objNode.Tag = kvpKeyAndEnabled.Key;
                                        objNode.Text = kvpKeyAndEnabled.Key;
                                        objNode.ForeColor = objGrayTextColor;
                                        objNode.ToolTipText = strFileNotFound;
                                    }, token: token).ConfigureAwait(false);
                                    intNodeIndex++;
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

            await UpdateCustomDataTabTitleAsync(token).ConfigureAwait(false);
        }

        private async Task UpdateCustomDataTabTitleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strTabText = await LanguageManager.GetStringAsync("Tab_Options_CustomData", token: token)
                                                      .ConfigureAwait(false);
            if (GlobalSettings.CustomDataDirectoryInfos.Any(x =>
                    CustomDataDirectoryUpdater.GetCachedAvailability(x).IsUpdateAvailable))
            {
                strTabText += await LanguageManager
                                   .GetStringAsync("String_CustomData_UpdatesAvailableMenuSuffix", token: token)
                                   .ConfigureAwait(false);
            }

            await tabCustomData.DoThreadSafeAsync(x => x.Text = strTabText, token).ConfigureAwait(false);
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
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstPriorityTables))
                {
                    foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                       .LoadXPathAsync("priorities.xml",
                                                                           await _objCharacterSettings
                                                                               .GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false),
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

                    string strOldSelected = await _objCharacterSettings.GetPriorityTableAsync(token).ConfigureAwait(false);
                    string strDefaultSelected = await _objReferenceCharacterSettings.GetPriorityTableAsync(token).ConfigureAwait(false);
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
                                x.SelectedValue = strDefaultSelected;
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
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstLimbCount))
                    {
                        foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                           .LoadXPathAsync("options.xml",
                                                                               await _objCharacterSettings
                                                                                   .GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false),
                                                                               token: token).ConfigureAwait(false))
                                                                    .SelectAndCacheExpression(
                                                                        "/chummer/limbcounts/limb", token: token))
                        {
                            string strExclude
                                = objXmlNode.SelectSingleNodeAndCacheExpression("exclude", token: token)?.Value
                                  ?? string.Empty;
                            if (!string.IsNullOrEmpty(strExclude))
                                strExclude = "<" + strExclude;
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
                            = (await _objCharacterSettings.GetLimbCountAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                        string strExcludeLimbSlot = await _objCharacterSettings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(strExcludeLimbSlot))
                            strLimbSlot += "<" + strExcludeLimbSlot;

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
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstGrades))
                {
                    foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                       .LoadXPathAsync("bioware.xml",
                                                                           await _objCharacterSettings
                                                                               .GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false),
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
                                                                           await _objCharacterSettings
                                                                               .GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false),
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
            using (_dicEnabledCharacterCustomDataDirectorys.LockObject.EnterWriteLock())
            {
                _dicEnabledCharacterCustomDataDirectorys.Clear();
                _objCharacterSettings.CustomDataDirectoryKeys.ForEach(kvpCustomDataDirectory =>
                {
                    _dicEnabledCharacterCustomDataDirectorys.Add(kvpCustomDataDirectory.Key,
                        kvpCustomDataDirectory.Value);
                });
            }
        }

        private async Task RebuildCustomDataDirectoryInfosAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _dicEnabledCharacterCustomDataDirectorys.LockObject
                .EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _dicEnabledCharacterCustomDataDirectorys.ClearAsync(token).ConfigureAwait(false);
                await (await _objCharacterSettings.GetCustomDataDirectoryKeysAsync(token).ConfigureAwait(false))
                    .ForEachAsync((kvpCustomDataDirectory, t) => _dicEnabledCharacterCustomDataDirectorys.AddAsync(
                        kvpCustomDataDirectory.Key,
                        kvpCustomDataDirectory.Value, t), token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task SetToolTips(CancellationToken token = default)
        {
            await chkUnarmedSkillImprovements
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsUnarmedSkillImprovements", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkIgnoreArt
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsIgnoreArt", token: token).ConfigureAwait(false))
                      .WordWrap(), token).ConfigureAwait(false);
            await chkIgnoreComplexFormLimit
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsIgnoreComplexFormLimit", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkCyberlegMovement
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsCyberlegMovement", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkDontDoubleQualityPurchases
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsDontDoubleQualityPurchases", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkDontDoubleQualityRefunds
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsDontDoubleQualityRefunds", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkStrictSkillGroups
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionStrictSkillGroups", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkAllowInitiation
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_OptionsAllowInitiation", token: token)
                                            .ConfigureAwait(false)).WordWrap(), token).ConfigureAwait(false);
            await chkUseCalculatedPublicAwareness
                  .SetToolTipTextAsync(
                      (await LanguageManager.GetStringAsync("Tip_PublicAwareness", token: token).ConfigureAwait(false))
                      .WordWrap(), token).ConfigureAwait(false);
        }

        private async Task SetupDataBindings(CancellationToken token = default)
        {
            await cmdRename.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = !y, _objCharacterSettings,
                                                                nameof(CharacterSettings.BuiltInOption),
                                                                (x, t) => x.GetBuiltInOptionAsync(t),
                                                                token: token)
                           .ConfigureAwait(false);
            await cmdDelete.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = !y, _objCharacterSettings,
                                                                nameof(CharacterSettings.BuiltInOption),
                                                                (x, t) => x.GetBuiltInOptionAsync(t),
                                                                token: token)
                           .ConfigureAwait(false);

            await cboBuildMethod.RegisterAsyncDataBindingWithDelayAsync(
                x => (CharacterBuildMethod)x.SelectedValue,
                (x, y) => x.SelectedValue = y, _objCharacterSettings,
                nameof(CharacterSettings.BuildMethod),
                (x, y) => x.SelectedValueChanged += y,
                (x, t) => x.GetBuildMethodAsync(t),
                (x, y, t) => x.SetBuildMethodAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await lblPriorityTable.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                       nameof(CharacterSettings
                                                                                  .BuildMethodUsesPriorityTables),
                                                                       (x, t) => x
                                                                            .GetBuildMethodUsesPriorityTablesAsync(
                                                                                t)
                                                                            , token: token)
                                  .ConfigureAwait(false);
            await cboPriorityTable.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                       nameof(CharacterSettings
                                                                                  .BuildMethodUsesPriorityTables),
                                                                       (x, t) => x
                                                                            .GetBuildMethodUsesPriorityTablesAsync(
                                                                                t)
                                                                            , token: token)
                                  .ConfigureAwait(false);
            await lblPriorities.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                    nameof(CharacterSettings.BuildMethodIsPriority),
                                                                    (x, t) => x.GetBuildMethodIsPriorityAsync(t)
                                                                          ,
                                                                    token: token).ConfigureAwait(false);
            await txtPriorities.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                    nameof(CharacterSettings.BuildMethodIsPriority),
                                                                    (x, t) => x.GetBuildMethodIsPriorityAsync(t)
                                                                          ,
                                                                    token: token).ConfigureAwait(false);
            await txtPriorities.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                _objCharacterSettings,
                nameof(CharacterSettings.PriorityArray),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetPriorityArrayAsync(t),
                (x, y, t) => x.SetPriorityArrayAsync(y, t),
                1000, token, token).ConfigureAwait(false);
            await lblSumToTen.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                  nameof(CharacterSettings.BuildMethodIsSumtoTen),
                                                                  (x, t) => x.GetBuildMethodIsSumtoTenAsync(t),
                                                                  token: token).ConfigureAwait(false);
            await nudSumToTen.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacterSettings,
                                                                  nameof(CharacterSettings.BuildMethodIsSumtoTen),
                                                                  (x, t) => x.GetBuildMethodIsSumtoTenAsync(t),
                                                                  token: token).ConfigureAwait(false);
            await nudSumToTen.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.SumtoTen),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetSumtoTenAsync(t),
                (x, y, t) => x.SetSumtoTenAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudStartingKarma.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.BuildKarma),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetBuildKarmaAsync(t),
                (x, y, t) => x.SetBuildKarmaAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxNuyenKarma.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenMaximumBP),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetNuyenMaximumBPAsync(t),
                (x, y, t) => x.SetNuyenMaximumBPAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxAvail.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaximumAvailability),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaximumAvailabilityAsync(t),
                (x, y, t) => x.SetMaximumAvailabilityAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudQualityKarmaLimit.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.QualityKarmaLimit),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetQualityKarmaLimitAsync(t),
                (x, y, t) => x.SetQualityKarmaLimitAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudNuyenCarryover.RegisterAsyncDataBindingWithDelayAsync(x => x.Value,
                (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenCarryover),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetNuyenCarryoverAsync(t),
                (x, y, t) => x.SetNuyenCarryoverAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudNuyenCarryover
                .RegisterOneWayAsyncDataBindingAsync((x, y) => x.DecimalPlaces = y, _objCharacterSettings,
                    nameof(CharacterSettings.MaxNuyenDecimals), (x, t) => x.GetMaxNuyenDecimalsAsync(t), token)
                .ConfigureAwait(false);
            await nudKarmaCarryover.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaCarryover),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaCarryoverAsync(t),
                (x, y, t) => x.SetKarmaCarryoverAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxNumberMaxAttributes.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxNumberMaxAttributesCreate),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxNumberMaxAttributesCreateAsync(t),
                (x, y, t) => x.SetMaxNumberMaxAttributesCreateAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxSkillRatingCreate.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxSkillRatingCreate),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxSkillRatingCreateAsync(t),
                (x, y, t) => x.SetMaxSkillRatingCreateAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxKnowledgeSkillRatingCreate.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxKnowledgeSkillRatingCreateAsync(t),
                (x, y, t) => x.SetMaxKnowledgeSkillRatingCreateAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxMartialArts.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaximumMartialArts),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaximumMartialArtsAsync(t),
                (x, y, t) => x.SetMaximumMartialArtsAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxMartialTechniques.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaximumMartialTechniques),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaximumMartialTechniquesAsync(t),
                (x, y, t) => x.SetMaximumMartialTechniquesAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxSkillRatingCreate
                .RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objCharacterSettings,
                    nameof(CharacterSettings.MaxSkillRating), (x, t) => x.GetMaxSkillRatingAsync(t), token)
                .ConfigureAwait(false);
            await nudMaxKnowledgeSkillRatingCreate
                .RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objCharacterSettings,
                    nameof(CharacterSettings.MaxKnowledgeSkillRating), (x, t) => x.GetMaxKnowledgeSkillRatingAsync(t), token)
                .ConfigureAwait(false);
            await txtContactPoints.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.ContactPointsExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetContactPointsExpressionAsync(t),
                (x, y, t) => x.SetContactPointsExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtGameplayOptionName.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.GameplayOptionName),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetGameplayOptionNameAsync(t),
                (x, y, t) => x.SetGameplayOptionNameAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtKnowledgePoints.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.KnowledgePointsExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetKnowledgePointsExpressionAsync(t),
                (x, y, t) => x.SetKnowledgePointsExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtRegisteredSpriteLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.RegisteredSpriteExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetRegisteredSpriteExpressionAsync(t),
                (x, y, t) => x.SetRegisteredSpriteExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtBoundSpiritLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.BoundSpiritExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetBoundSpiritExpressionAsync(t),
                (x, y, t) => x.SetBoundSpiritExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtEssenceModifierPostExpression.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.EssenceModifierPostExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetEssenceModifierPostExpressionAsync(t),
                (x, y, t) => x.SetEssenceModifierPostExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtLiftLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.LiftLimitExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetLiftLimitExpressionAsync(t),
                (x, y, t) => x.SetLiftLimitExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtCarryLimit.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.CarryLimitExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetCarryLimitExpressionAsync(t),
                (x, y, t) => x.SetCarryLimitExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await txtEncumbranceInterval.RegisterAsyncDataBindingWithDelayAsync(
                x => x.Text,
                (x, y) => x.Text = y, _objCharacterSettings,
                nameof(CharacterSettings.EncumbranceIntervalExpression),
                (x, y) => x.TextChanged += y,
                (x, t) => x.GetEncumbranceIntervalExpressionAsync(t),
                (x, y, t) => x.SetEncumbranceIntervalExpressionAsync(y, t),
                1000,
                token,
                token).ConfigureAwait(false);
            await nudWeightDecimals.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.WeightDecimals),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetWeightDecimalsAsync(t),
                (x, y, t) => x.SetWeightDecimalsAsync(y, t), 250, token, token).ConfigureAwait(false);

            await chkEncumbrancePenaltyPhysicalLimit.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyPhysicalLimit),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDoEncumbrancePenaltyPhysicalLimitAsync(t),
                (x, y, t) => x.SetDoEncumbrancePenaltyPhysicalLimitAsync(y, t), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyMovementSpeed.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyMovementSpeed),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDoEncumbrancePenaltyMovementSpeedAsync(t),
                (x, y, t) => x.SetDoEncumbrancePenaltyMovementSpeedAsync(y, t), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyAgility.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyAgility),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDoEncumbrancePenaltyAgilityAsync(t),
                (x, y, t) => x.SetDoEncumbrancePenaltyAgilityAsync(y, t), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyReaction.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyReaction),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDoEncumbrancePenaltyReactionAsync(t),
                (x, y, t) => x.SetDoEncumbrancePenaltyReactionAsync(y, t), token).ConfigureAwait(false);
            await chkEncumbrancePenaltyReaction.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DoEncumbrancePenaltyWoundModifier),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDoEncumbrancePenaltyWoundModifierAsync(t),
                (x, y, t) => x.SetDoEncumbrancePenaltyWoundModifierAsync(y, t), token).ConfigureAwait(false);

            await nudEncumbrancePenaltyPhysicalLimit.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyPhysicalLimit),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetEncumbrancePenaltyPhysicalLimitAsync(t),
                (x, y, t) => x.SetEncumbrancePenaltyPhysicalLimitAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyMovementSpeed.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyMovementSpeed),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetEncumbrancePenaltyMovementSpeedAsync(t),
                (x, y, t) => x.SetEncumbrancePenaltyMovementSpeedAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyAgility.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyAgility),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetEncumbrancePenaltyAgilityAsync(t),
                (x, y, t) => x.SetEncumbrancePenaltyAgilityAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyReaction.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyReaction),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetEncumbrancePenaltyReactionAsync(t),
                (x, y, t) => x.SetEncumbrancePenaltyReactionAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudEncumbrancePenaltyWoundModifier.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EncumbrancePenaltyWoundModifier),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetEncumbrancePenaltyWoundModifierAsync(t),
                (x, y, t) => x.SetEncumbrancePenaltyWoundModifierAsync(y, t), 250, token, token).ConfigureAwait(false);

            await chkEnforceCapacity.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EnforceCapacity),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetEnforceCapacityAsync(t),
                (x, y, t) => x.SetEnforceCapacityAsync(y, t), token).ConfigureAwait(false);
            await chkLicenseEachRestrictedItem.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.LicenseRestricted),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetLicenseRestrictedAsync(t),
                (x, y, t) => x.SetLicenseRestrictedAsync(y, t), token).ConfigureAwait(false);
            await chkReverseAttributePriorityOrder.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ReverseAttributePriorityOrder),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetReverseAttributePriorityOrderAsync(t),
                (x, y, t) => x.SetReverseAttributePriorityOrderAsync(y, t), token).ConfigureAwait(false);
            await chkDronemods.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneMods),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDroneModsAsync(t),
                (x, y, t) => x.SetDroneModsAsync(y, t), token).ConfigureAwait(false);
            await chkDronemodsMaximumPilot.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneModsMaximumPilot),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDroneModsMaximumPilotAsync(t),
                (x, y, t) => x.SetDroneModsMaximumPilotAsync(y, t), token).ConfigureAwait(false);
            await chkRestrictRecoil.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RestrictRecoil),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetRestrictRecoilAsync(t),
                (x, y, t) => x.SetRestrictRecoilAsync(y, t), token).ConfigureAwait(false);
            await chkStrictSkillGroups.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.StrictSkillGroupsInCreateMode),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetStrictSkillGroupsInCreateModeAsync(t),
                (x, y, t) => x.SetStrictSkillGroupsInCreateModeAsync(y, t), token).ConfigureAwait(false);
            await chkAllowPointBuySpecializationsOnKarmaSkills.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowPointBuySpecializationsOnKarmaSkills),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowPointBuySpecializationsOnKarmaSkillsAsync(t),
                (x, y, t) => x.SetAllowPointBuySpecializationsOnKarmaSkillsAsync(y, t), token).ConfigureAwait(false);
            await chkAllowFreeGrids.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowFreeGrids),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowFreeGridsAsync(t),
                (x, y, t) => x.SetAllowFreeGridsAsync(y, t), token).ConfigureAwait(false);

            await chkDontUseCyberlimbCalculation.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontUseCyberlimbCalculation),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDontUseCyberlimbCalculationAsync(t),
                (x, y, t) => x.SetDontUseCyberlimbCalculationAsync(y, t), token).ConfigureAwait(false);
            await chkCyberlegMovement.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CyberlegMovement),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetCyberlegMovementAsync(t),
                (x, y, t) => x.SetCyberlegMovementAsync(y, t), token).ConfigureAwait(false);
            await chkCyberlimbAttributeBonusCap.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetCyberlimbAttributeBonusCapOverrideAsync(t),
                (x, y, t) => x.SetCyberlimbAttributeBonusCapOverrideAsync(y, t), token).ConfigureAwait(false);
            await nudCyberlimbAttributeBonusCap.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride),
                (x, t) => x.GetCyberlimbAttributeBonusCapOverrideAsync(t), token: token).ConfigureAwait(false);
            await nudCyberlimbAttributeBonusCap.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CyberlimbAttributeBonusCap),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetCyberlimbAttributeBonusCapAsync(t),
                (x, y, t) => x.SetCyberlimbAttributeBonusCapAsync(y, t), 250, token, token).ConfigureAwait(false);
            await chkRedlinerLimbsSkull.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesSkull),
                (x, y) => x.CheckedChanged += y,
                async (x, t) => !await x.GetRedlinerExcludesSkullAsync(t).ConfigureAwait(false),
                (x, y, t) => x.SetRedlinerExcludesSkullAsync(!y, t), token, token).ConfigureAwait(false);
            await chkRedlinerLimbsTorso.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesTorso),
                (x, y) => x.CheckedChanged += y,
                async (x, t) => !await x.GetRedlinerExcludesTorsoAsync(t).ConfigureAwait(false),
                (x, y, t) => x.SetRedlinerExcludesTorsoAsync(!y, t), token, token).ConfigureAwait(false);
            await chkRedlinerLimbsArms.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesArms),
                (x, y) => x.CheckedChanged += y,
                async (x, t) => !await x.GetRedlinerExcludesArmsAsync(t).ConfigureAwait(false),
                (x, y, t) => x.SetRedlinerExcludesArmsAsync(!y, t), token, token).ConfigureAwait(false);
            await chkRedlinerLimbsLegs.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.RedlinerExcludesLegs),
                (x, y) => x.CheckedChanged += y,
                async (x, t) => !await x.GetRedlinerExcludesLegsAsync(t).ConfigureAwait(false),
                (x, y, t) => x.SetRedlinerExcludesLegsAsync(!y, t), token, token).ConfigureAwait(false);

            await nudNuyenDecimalsMaximum.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxNuyenDecimals),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxNuyenDecimalsAsync(t),
                (x, y, t) => x.SetMaxNuyenDecimalsAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudNuyenDecimalsMinimum.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinNuyenDecimals),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMinNuyenDecimalsAsync(t),
                (x, y, t) => x.SetMinNuyenDecimalsAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudEssenceDecimals.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EssenceDecimals),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetEssenceDecimalsAsync(t),
                (x, y, t) => x.SetEssenceDecimalsAsync(y, t), 250, token, token).ConfigureAwait(false);
            await chkDontRoundEssenceInternally.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontRoundEssenceInternally),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDontRoundEssenceInternallyAsync(t),
                (x, y, t) => x.SetDontRoundEssenceInternallyAsync(y, t), token).ConfigureAwait(false);

            await nudMinInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMinInitiativeDiceAsync(t),
                (x, y, t) => x.SetMinInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinInitiativeDice),
                (x, t) => x.GetMinInitiativeDiceAsync(t), token: token).ConfigureAwait(false);
            await nudMaxInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxInitiativeDiceAsync(t),
                (x, y, t) => x.SetMaxInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMinAstralInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinAstralInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMinAstralInitiativeDiceAsync(t),
                (x, y, t) => x.SetMinAstralInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxAstralInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinAstralInitiativeDice),
                (x, t) => x.GetMinAstralInitiativeDiceAsync(t), token: token).ConfigureAwait(false);
            await nudMaxAstralInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxAstralInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxAstralInitiativeDiceAsync(t),
                (x, y, t) => x.SetMaxAstralInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMinColdSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinColdSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMinColdSimInitiativeDiceAsync(t),
                (x, y, t) => x.SetMinColdSimInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxColdSimInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinColdSimInitiativeDice),
                (x, t) => x.GetMinColdSimInitiativeDiceAsync(t), token: token).ConfigureAwait(false);
            await nudMaxColdSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxColdSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxColdSimInitiativeDiceAsync(t),
                (x, y, t) => x.SetMaxColdSimInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMinHotSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MinHotSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMinHotSimInitiativeDiceAsync(t),
                (x, y, t) => x.SetMinHotSimInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxHotSimInitiativeDice.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Minimum = y, _objCharacterSettings,
                nameof(CharacterSettings.MinHotSimInitiativeDice),
                (x, t) => x.GetMinHotSimInitiativeDiceAsync(t), token: token).ConfigureAwait(false);
            await nudMaxHotSimInitiativeDice.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxHotSimInitiativeDice),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxHotSimInitiativeDiceAsync(t),
                (x, y, t) => x.SetMaxHotSimInitiativeDiceAsync(y, t), 250, token, token).ConfigureAwait(false);

            await chkEnable4eStyleEnemyTracking.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetEnableEnemyTrackingAsync(t),
                (x, y, t) => x.SetEnableEnemyTrackingAsync(y, t), token).ConfigureAwait(false);
            await flpKarmaGainedFromEnemies.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                (x, t) => x.GetEnableEnemyTrackingAsync(t), token: token).ConfigureAwait(false);
            await nudKarmaGainedFromEnemies.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaEnemy),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaEnemyAsync(t),
                (x, y, t) => x.SetKarmaEnemyAsync(y, t), 250, token, token).ConfigureAwait(false);
            await chkEnemyKarmaQualityLimit.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.EnableEnemyTracking),
                (x, t) => x.GetEnableEnemyTrackingAsync(t), token: token).ConfigureAwait(false);
            await chkEnemyKarmaQualityLimit.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.EnemyKarmaQualityLimit),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetEnemyKarmaQualityLimitAsync(t),
                (x, y, t) => x.SetEnemyKarmaQualityLimitAsync(y, t), token).ConfigureAwait(false);
            await chkMoreLethalGameplay.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MoreLethalGameplay),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetMoreLethalGameplayAsync(t),
                (x, y, t) => x.SetMoreLethalGameplayAsync(y, t), token).ConfigureAwait(false);

            await chkNoArmorEncumbrance.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NoArmorEncumbrance),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetNoArmorEncumbranceAsync(t),
                (x, y, t) => x.SetNoArmorEncumbranceAsync(y, t), token).ConfigureAwait(false);
            await chkUncappedArmorAccessoryBonuses.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UncappedArmorAccessoryBonuses),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetUncappedArmorAccessoryBonusesAsync(t),
                (x, y, t) => x.SetUncappedArmorAccessoryBonusesAsync(y, t), token).ConfigureAwait(false);
            await chkIgnoreArt.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.IgnoreArt),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetIgnoreArtAsync(t),
                (x, y, t) => x.SetIgnoreArtAsync(y, t), token).ConfigureAwait(false);
            await chkIgnoreComplexFormLimit.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.IgnoreComplexFormLimit),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetIgnoreComplexFormLimitAsync(t),
                (x, y, t) => x.SetIgnoreComplexFormLimitAsync(y, t), token).ConfigureAwait(false);
            await chkUnarmedSkillImprovements.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UnarmedImprovementsApplyToWeapons),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetUnarmedImprovementsApplyToWeaponsAsync(t),
                (x, y, t) => x.SetUnarmedImprovementsApplyToWeaponsAsync(y, t), token).ConfigureAwait(false);
            await chkMysAdPp.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptAllowPpCareer),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetMysAdeptAllowPpCareerAsync(t),
                (x, y, t) => x.SetMysAdeptAllowPpCareerAsync(y, t), token).ConfigureAwait(false);
            await chkMysAdPp.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = !y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                (x, t) => x.GetMysAdeptSecondMAGAttributeAsync(t), token: token).ConfigureAwait(false);
            await chkPrioritySpellsAsAdeptPowers.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.PrioritySpellsAsAdeptPowers),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetPrioritySpellsAsAdeptPowersAsync(t),
                (x, y, t) => x.SetPrioritySpellsAsAdeptPowersAsync(y, t), token).ConfigureAwait(false);
            await chkPrioritySpellsAsAdeptPowers.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = !y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                (x, t) => x.GetMysAdeptSecondMAGAttributeAsync(t), token: token).ConfigureAwait(false);
            await chkMysAdeptSecondMAGAttribute.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttribute),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetMysAdeptSecondMAGAttributeAsync(t),
                (x, y, t) => x.SetMysAdeptSecondMAGAttributeAsync(y, t), token).ConfigureAwait(false);
            await chkMysAdeptSecondMAGAttribute.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.MysAdeptSecondMAGAttributeEnabled),
                (x, t) => x.GetMysAdeptSecondMAGAttributeEnabledAsync(t), token: token).ConfigureAwait(false);
            await chkUsePointsOnBrokenGroups.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UsePointsOnBrokenGroups),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetUsePointsOnBrokenGroupsAsync(t),
                (x, y, t) => x.SetUsePointsOnBrokenGroupsAsync(y, t), token).ConfigureAwait(false);
            await chkSpecialKarmaCost.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.SpecialKarmaCostBasedOnShownValue),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetSpecialKarmaCostBasedOnShownValueAsync(t),
                (x, y, t) => x.SetSpecialKarmaCostBasedOnShownValueAsync(y, t), token).ConfigureAwait(false);
            await chkUseCalculatedPublicAwareness.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.UseCalculatedPublicAwareness),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetUseCalculatedPublicAwarenessAsync(t),
                (x, y, t) => x.SetUseCalculatedPublicAwarenessAsync(y, t), token).ConfigureAwait(false);
            await chkAlternateMetatypeAttributeKarma.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AlternateMetatypeAttributeKarma),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAlternateMetatypeAttributeKarmaAsync(t),
                (x, y, t) => x.SetAlternateMetatypeAttributeKarmaAsync(y, t), token).ConfigureAwait(false);
            await chkCompensateSkillGroupKarmaDifference.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.CompensateSkillGroupKarmaDifference),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetCompensateSkillGroupKarmaDifferenceAsync(t),
                (x, y, t) => x.SetCompensateSkillGroupKarmaDifferenceAsync(y, t), token).ConfigureAwait(false);
            await chkFreeMartialArtSpecialization.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.FreeMartialArtSpecialization),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetFreeMartialArtSpecializationAsync(t),
                (x, y, t) => x.SetFreeMartialArtSpecializationAsync(y, t), token).ConfigureAwait(false);
            await chkIncreasedImprovedAbilityModifier.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.IncreasedImprovedAbilityMultiplier),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetIncreasedImprovedAbilityMultiplierAsync(t),
                (x, y, t) => x.SetIncreasedImprovedAbilityMultiplierAsync(y, t), token).ConfigureAwait(false);
            await chkAllowTechnomancerSchooling.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowTechnomancerSchooling),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowTechnomancerSchoolingAsync(t),
                (x, y, t) => x.SetAllowTechnomancerSchoolingAsync(y, t), token).ConfigureAwait(false);
            await chkAllowSkillRegrouping.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowSkillRegrouping),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowSkillRegroupingAsync(t),
                (x, y, t) => x.SetAllowSkillRegroupingAsync(y, t), token).ConfigureAwait(false);
            await chkSpecializationsBreakSkillGroups.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.SpecializationsBreakSkillGroups),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetSpecializationsBreakSkillGroupsAsync(t),
                (x, y, t) => x.SetSpecializationsBreakSkillGroupsAsync(y, t), token).ConfigureAwait(false);
            await chkDontDoubleQualityPurchases.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontDoubleQualityPurchases),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDontDoubleQualityPurchasesAsync(t),
                (x, y, t) => x.SetDontDoubleQualityPurchasesAsync(y, t), token).ConfigureAwait(false);
            await chkDontDoubleQualityRefunds.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DontDoubleQualityRefunds),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDontDoubleQualityRefundsAsync(t),
                (x, y, t) => x.SetDontDoubleQualityRefundsAsync(y, t), token).ConfigureAwait(false);
            await chkDroneArmorMultiplier.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplierEnabled),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetDroneArmorMultiplierEnabledAsync(t),
                (x, y, t) => x.SetDroneArmorMultiplierEnabledAsync(y, t), token).ConfigureAwait(false);
            await nudDroneArmorMultiplier.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplierEnabled),
                (x, t) => x.GetDroneArmorMultiplierEnabledAsync(t), token: token).ConfigureAwait(false);
            await nudDroneArmorMultiplier.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.DroneArmorMultiplier),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetDroneArmorMultiplierAsync(t),
                (x, y, t) => x.SetDroneArmorMultiplierAsync(y, t), 250, token, token).ConfigureAwait(false);
            await chkESSLossReducesMaximumOnly.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ESSLossReducesMaximumOnly),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetESSLossReducesMaximumOnlyAsync(t),
                (x, y, t) => x.SetESSLossReducesMaximumOnlyAsync(y, t), token).ConfigureAwait(false);
            await chkExceedNegativeQualities.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualities),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetExceedNegativeQualitiesAsync(t),
                (x, y, t) => x.SetExceedNegativeQualitiesAsync(y, t), token).ConfigureAwait(false);
            await chkExceedNegativeQualitiesNoBonus.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualities),
                (x, t) => x.GetExceedNegativeQualitiesAsync(t), token: token).ConfigureAwait(false);
            await chkExceedNegativeQualitiesNoBonus.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedNegativeQualitiesNoBonus),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetExceedNegativeQualitiesNoBonusAsync(t),
                (x, y, t) => x.SetExceedNegativeQualitiesNoBonusAsync(y, t), token).ConfigureAwait(false);
            await chkExceedPositiveQualities.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualities),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetExceedPositiveQualitiesAsync(t),
                (x, y, t) => x.SetExceedPositiveQualitiesAsync(y, t), token).ConfigureAwait(false);
            await chkExceedPositiveQualitiesCostDoubled.RegisterOneWayAsyncDataBindingAsync(
                (x, y) => x.Enabled = y, _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualities),
                (x, t) => x.GetExceedPositiveQualitiesAsync(t), token: token).ConfigureAwait(false);
            await chkExceedPositiveQualitiesCostDoubled.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExceedPositiveQualitiesCostDoubled),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetExceedPositiveQualitiesCostDoubledAsync(t),
                (x, y, t) => x.SetExceedPositiveQualitiesCostDoubledAsync(y, t), token).ConfigureAwait(false);
            await chkExtendAnyDetectionSpell.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.ExtendAnyDetectionSpell),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetExtendAnyDetectionSpellAsync(t),
                (x, y, t) => x.SetExtendAnyDetectionSpellAsync(y, t), token).ConfigureAwait(false);
            await chkAllowLimitedSpellsForBareHandedAdept.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowLimitedSpellsForBareHandedAdept),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowLimitedSpellsForBareHandedAdeptAsync(t),
                (x, y, t) => x.SetAllowLimitedSpellsForBareHandedAdeptAsync(y, t), token).ConfigureAwait(false);
            await chkAllowCyberwareESSDiscounts.RegisterAsyncDataBindingAsync(x => x.Checked,
                (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowCyberwareESSDiscounts),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowCyberwareESSDiscountsAsync(t),
                (x, y, t) => x.SetAllowCyberwareESSDiscountsAsync(y, t), token).ConfigureAwait(false);
            await chkAllowInitiation.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                _objCharacterSettings,
                nameof(CharacterSettings.AllowInitiationInCreateMode),
                (x, y) => x.CheckedChanged += y,
                (x, t) => x.GetAllowInitiationInCreateModeAsync(t),
                (x, y, t) => x.SetAllowInitiationInCreateModeAsync(y, t),
                token,
                token).ConfigureAwait(false);
            await nudMaxSkillRating.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxSkillRating),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxSkillRatingAsync(t),
                (x, y, t) => x.SetMaxKnowledgeSkillRatingAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudMaxKnowledgeSkillRating.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MaxKnowledgeSkillRating),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMaxKnowledgeSkillRatingAsync(t),
                (x, y, t) => x.SetMaxKnowledgeSkillRatingAsync(y, t), 250, token, token).ConfigureAwait(false);

            // Karma options.
            await nudMetatypeCostsKarmaMultiplier.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.MetatypeCostsKarmaMultiplier),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetMetatypeCostsKarmaMultiplierAsync(t),
                (x, y, t) => x.SetMetatypeCostsKarmaMultiplierAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNuyenPerWftM.RegisterAsyncDataBindingWithDelayAsync(x => x.Value,
                (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenPerBPWftM),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetNuyenPerBPWftMAsync(t),
                (x, y, t) => x.SetNuyenPerBPWftMAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNuyenPerWftP.RegisterAsyncDataBindingWithDelayAsync(x => x.Value,
                (x, y) => x.Value = y,
                _objCharacterSettings,
                nameof(CharacterSettings.NuyenPerBPWftP),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetNuyenPerBPWftPAsync(t),
                (x, y, t) => x.SetNuyenPerBPWftPAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaAttribute.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaAttribute),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaAttributeAsync(t),
                (x, y, t) => x.SetKarmaAttributeAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaQuality.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaQuality),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaQualityAsync(t),
                (x, y, t) => x.SetKarmaQualityAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpecialization.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpecialization),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSpecializationAsync(t),
                (x, y, t) => x.SetKarmaSpecializationAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaKnowledgeSpecialization.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaKnowledgeSpecialization),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaKnowledgeSpecializationAsync(t),
                (x, y, t) => x.SetKarmaKnowledgeSpecializationAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewKnowledgeSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewKnowledgeSkill),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaNewKnowledgeSkillAsync(t),
                (x, y, t) => x.SetKarmaNewKnowledgeSkillAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewActiveSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewActiveSkill),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaNewActiveSkillAsync(t),
                (x, y, t) => x.SetKarmaNewActiveSkillAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewSkillGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewSkillGroup),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaNewSkillGroupAsync(t),
                (x, y, t) => x.SetKarmaNewSkillGroupAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaImproveKnowledgeSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaImproveKnowledgeSkill),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaImproveKnowledgeSkillAsync(t),
                (x, y, t) => x.SetKarmaImproveKnowledgeSkillAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaImproveActiveSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaImproveActiveSkill),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaImproveActiveSkillAsync(t),
                (x, y, t) => x.SetKarmaImproveActiveSkillAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaImproveSkillGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaImproveSkillGroup),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaImproveSkillGroupAsync(t),
                (x, y, t) => x.SetKarmaImproveSkillGroupAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpell.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpell),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSpellAsync(t),
                (x, y, t) => x.SetKarmaSpellAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewComplexForm.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewComplexForm),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaNewComplexFormAsync(t),
                (x, y, t) => x.SetKarmaNewComplexFormAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewAIProgram.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewAIProgram),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaNewAIProgramAsync(t),
                (x, y, t) => x.SetKarmaNewAIProgramAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaNewAIAdvancedProgram.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaNewAIAdvancedProgram),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaNewAIAdvancedProgramAsync(t),
                (x, y, t) => x.SetKarmaNewAIAdvancedProgramAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaMetamagic.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaMetamagic),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaMetamagicAsync(t),
                (x, y, t) => x.SetKarmaMetamagicAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaContact.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaContact),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaContactAsync(t),
                (x, y, t) => x.SetKarmaContactAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpirit.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpirit),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSpiritAsync(t),
                (x, y, t) => x.SetKarmaSpiritAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpiritFettering.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpiritFettering),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSpiritFetteringAsync(t),
                (x, y, t) => x.SetKarmaSpiritFetteringAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaTechnique.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaTechnique),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaTechniqueAsync(t),
                (x, y, t) => x.SetKarmaTechniqueAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaInitiation.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaInitiation),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaInitiationAsync(t),
                (x, y, t) => x.SetKarmaInitiationAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaInitiationFlat.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaInitiationFlat),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaInitiationFlatAsync(t),
                (x, y, t) => x.SetKarmaInitiationFlatAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaJoinGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaJoinGroup),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaJoinGroupAsync(t),
                (x, y, t) => x.SetKarmaJoinGroupAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaLeaveGroup.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaLeaveGroup),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaLeaveGroupAsync(t),
                (x, y, t) => x.SetKarmaLeaveGroupAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaMysticAdeptPowerPoint.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaMysticAdeptPowerPoint),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaMysticAdeptPowerPointAsync(t),
                (x, y, t) => x.SetKarmaMysticAdeptPowerPointAsync(y, t), 250, token, token).ConfigureAwait(false);

            // Focus costs
            await nudKarmaAlchemicalFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaAlchemicalFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaAlchemicalFocusAsync(t),
                (x, y, t) => x.SetKarmaAlchemicalFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaBanishingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaBanishingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaBanishingFocusAsync(t),
                (x, y, t) => x.SetKarmaBanishingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaBindingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaBindingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaBindingFocusAsync(t),
                (x, y, t) => x.SetKarmaBindingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaCenteringFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaCenteringFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaCenteringFocusAsync(t),
                (x, y, t) => x.SetKarmaCenteringFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaCounterspellingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaCounterspellingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaCounterspellingFocusAsync(t),
                (x, y, t) => x.SetKarmaCounterspellingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaDisenchantingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaDisenchantingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaDisenchantingFocusAsync(t),
                (x, y, t) => x.SetKarmaDisenchantingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaFlexibleSignatureFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaFlexibleSignatureFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaFlexibleSignatureFocusAsync(t),
                (x, y, t) => x.SetKarmaFlexibleSignatureFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaMaskingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaMaskingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaMaskingFocusAsync(t),
                (x, y, t) => x.SetKarmaMaskingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaPowerFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaPowerFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaPowerFocusAsync(t),
                (x, y, t) => x.SetKarmaPowerFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaQiFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaQiFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaQiFocusAsync(t),
                (x, y, t) => x.SetKarmaQiFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaRitualSpellcastingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaRitualSpellcastingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaRitualSpellcastingFocusAsync(t),
                (x, y, t) => x.SetKarmaRitualSpellcastingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpellcastingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpellcastingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSpellcastingFocusAsync(t),
                (x, y, t) => x.SetKarmaSpellcastingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSpellShapingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSpellShapingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSpellShapingFocusAsync(t),
                (x, y, t) => x.SetKarmaSpellShapingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSummoningFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSummoningFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSummoningFocusAsync(t),
                (x, y, t) => x.SetKarmaSummoningFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaSustainingFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaSustainingFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaSustainingFocusAsync(t),
                (x, y, t) => x.SetKarmaSustainingFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
            await nudKarmaWeaponFocus.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt,
                (x, y) => x.ValueAsInt = y,
                _objCharacterSettings,
                nameof(CharacterSettings.KarmaWeaponFocus),
                (x, y) => x.ValueChanged += y,
                (x, t) => x.GetKarmaWeaponFocusAsync(t),
                (x, y, t) => x.SetKarmaWeaponFocusAsync(y, t), 250, token, token).ConfigureAwait(false);
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
            if (await _objCharacterSettings.GetBuildMethodAsync(token).ConfigureAwait(false) == CharacterBuildMethod.Priority
                && (await _objCharacterSettings.GetPriorityArrayAsync(token).ConfigureAwait(false)).Length != 5)
                return false;

            return await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetContactPointsExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetKnowledgePointsExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       (await _objCharacterSettings.GetChargenKarmaToNuyenExpressionAsync(token).ConfigureAwait(false))
                       .Replace("{Karma}", "1")
                       .Replace("{PriorityNuyen}", "1"), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                                            (await _objCharacterSettings.GetEssenceModifierPostExpressionAsync(token).ConfigureAwait(false))
                                            .Replace("{Modifier}", "1.0"), token: token)
                                        .ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetRegisteredSpriteExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetBoundSpiritExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetLiftLimitExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetCarryLimitExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) &&
                   await CommonFunctions.IsCharacterAttributeXPathValidOrNullAsync(
                       await _objCharacterSettings.GetEncumbranceIntervalExpressionAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
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
