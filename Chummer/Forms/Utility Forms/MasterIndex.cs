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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class MasterIndex : Form
    {
        private int _intSkipRefresh = 1;
        private CharacterSettings _objSelectedSetting;
        private readonly LockingDictionary<MasterIndexEntry, Task<string>> _dicCachedNotes = new LockingDictionary<MasterIndexEntry, Task<string>>();
        private List<ListItem> _lstFileNamesWithItems = Utils.ListItemListPool.Get();
        private List<ListItem> _lstItems = Utils.ListItemListPool.Get();
        private readonly CancellationTokenSource _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        private static async ValueTask<CharacterSettings> GetInitialSetting(CancellationToken token = default)
        {
            IAsyncReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings = await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false);
            (bool blnSuccess, CharacterSettings objReturn)
                = await dicCharacterSettings.TryGetValueAsync(GlobalSettings.DefaultMasterIndexSetting, token).ConfigureAwait(false);
            if (blnSuccess)
                return objReturn;
            (blnSuccess, objReturn)
                = await dicCharacterSettings.TryGetValueAsync(GlobalSettings.DefaultMasterIndexSettingDefaultValue, token).ConfigureAwait(false);
            return blnSuccess
                ? objReturn
                : (await dicCharacterSettings.GetReadOnlyValuesAsync(token).ConfigureAwait(false)).First();
        }

        private static readonly ReadOnlyCollection<string> _astrFileNames = Array.AsReadOnly(new[]
        {
            "actions.xml",
            "armor.xml",
            "bioware.xml",
            "complexforms.xml",
            "critters.xml",
            "critterpowers.xml",
            "cyberware.xml",
            "drugcomponents.xml",
            "echoes.xml",
            "gear.xml",
            "lifemodules.xml",
            "lifestyles.xml",
            "martialarts.xml",
            "mentors.xml",
            "metamagic.xml",
            "metatypes.xml",
            "paragons.xml",
            "powers.xml",
            "programs.xml",
            "qualities.xml",
            "references.xml",
            "skills.xml",
            "spells.xml",
            "spiritpowers.xml",
            "streams.xml",
            "traditions.xml",
            "vehicles.xml",
            "weapons.xml"
        });

        public MasterIndex()
        {
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                _dicCachedNotes.Dispose();
                foreach (ListItem objExistingItem in _lstItems)
                    ((MasterIndexEntry) objExistingItem.Value).Dispose();
                Utils.ListItemListPool.Return(ref _lstFileNamesWithItems);
                Utils.ListItemListPool.Return(ref _lstItems);
                _objGenericFormClosingCancellationTokenSource.Dispose();
            };
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async ValueTask PopulateCharacterSettings(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Populate the Character Settings list.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCharacterSettings))
            {
                IAsyncReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings
                    = await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false);
                await dicCharacterSettings.ForEachAsync(async x =>
                {
                    lstCharacterSettings.Add(
                        new ListItem(x.Value, await x.Value.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                }, token: token).ConfigureAwait(false);

                lstCharacterSettings.Sort(CompareListItems.CompareNames);

                string strOldSettingKey
                    = await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, token).ConfigureAwait(false)
                        is CharacterSettings objOldSettings
                        ? await objOldSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                        : string.Empty;
                if (string.IsNullOrEmpty(strOldSettingKey))
                {
                    if (_objSelectedSetting == null)
                    {
                        Interlocked.CompareExchange(ref _objSelectedSetting,
                                                    await GetInitialSetting(token).ConfigureAwait(false), null);
                    }

                    strOldSettingKey = _objSelectedSetting != null
                        ? await _objSelectedSetting.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                        : string.Empty;
                }

                Interlocked.Increment(ref _intSkipRefresh);
                try
                {
                    await cboCharacterSetting.PopulateWithListItemsAsync(lstCharacterSettings, token)
                                             .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strOldSettingKey))
                    {
                        (bool blnSuccess, CharacterSettings objSettings)
                            = await dicCharacterSettings.TryGetValueAsync(strOldSettingKey, token)
                                                        .ConfigureAwait(false);
                        if (blnSuccess)
                            await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSettings, token)
                                                     .ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipRefresh);
                }

                if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false)
                    != -1)
                    return;
                (bool blnSuccess2, CharacterSettings objSettings2)
                    = await dicCharacterSettings.TryGetValueAsync(GlobalSettings.DefaultMasterIndexSetting, token)
                                                .ConfigureAwait(false);
                if (blnSuccess2)
                    await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSettings2, token)
                                             .ConfigureAwait(false);
                else
                {
                    (bool blnSuccess3, CharacterSettings objSettings3)
                        = await dicCharacterSettings
                                .TryGetValueAsync(GlobalSettings.DefaultMasterIndexSettingDefaultValue, token)
                                .ConfigureAwait(false);
                    if (blnSuccess3)
                        await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSettings3, token)
                                                 .ConfigureAwait(false);
                }

                await cboCharacterSetting.DoThreadSafeAsync(x =>
                {
                    if (x.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                        x.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
            }
        }

        private async void MasterIndex_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await SourceString.Blank.SetControlAsync(lblSource, _objGenericToken).ConfigureAwait(false);
                    await PopulateCharacterSettings(_objGenericToken).ConfigureAwait(false);
                    await LoadContent(_objGenericToken).ConfigureAwait(false);
                    CharacterSettings objSettings = _objSelectedSetting;
                    if (objSettings == null)
                    {
                        CharacterSettings objNewSettings
                            = await GetInitialSetting(_objGenericToken).ConfigureAwait(false);
                        objSettings = Interlocked.CompareExchange(ref _objSelectedSetting, objNewSettings, null)
                                      ?? objNewSettings;
                    }

                    if (objSettings != null)
                    {
                        IAsyncDisposable objLocker = await objSettings.LockObject.EnterWriteLockAsync(_objGenericToken).ConfigureAwait(false);
                        try
                        {
                            objSettings.PropertyChanged += OnSelectedSettingChanged;
                        }
                        finally
                        {
                            await objLocker.DisposeAsync().ConfigureAwait(false);
                        }
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
            finally
            {
                Interlocked.Increment(ref _intIsFinishedLoading);
            }
        }

        private async void MasterIndex_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_objSelectedSetting != null)
            {
                IAsyncDisposable objLocker = await _objSelectedSetting.LockObject.EnterWriteLockAsync(_objGenericToken).ConfigureAwait(false);
                try
                {
                    _objSelectedSetting.PropertyChanged -= OnSelectedSettingChanged;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            _objGenericFormClosingCancellationTokenSource?.Cancel(false);
            // ReSharper disable once MethodSupportsCancellation
            foreach (Task<string> tskLoop in await _dicCachedNotes.GetValuesAsync().ConfigureAwait(false))
            {
                try
                {
                    await tskLoop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private async void OnSelectedSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterSettings.Books)
                || e.PropertyName == nameof(CharacterSettings.EnabledCustomDataDirectoryPaths))
            {
                try
                {
                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await LoadContent(_objGenericToken).ConfigureAwait(false);
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
        }

        private async void cboCharacterSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intSkipRefresh > 0)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    string strSelectedSetting
                        = await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, _objGenericToken)
                                                   .ConfigureAwait(false) is CharacterSettings objOldSettings
                            ? await objOldSettings.GetDictionaryKeyAsync(_objGenericToken).ConfigureAwait(false)
                            : string.Empty;
                    bool blnSuccess = false;
                    CharacterSettings objSettings = null;
                    IAsyncReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings
                        = await SettingsManager.GetLoadedCharacterSettingsAsync(_objGenericToken)
                                               .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedSetting))
                    {
                        (blnSuccess, objSettings)
                            = await dicCharacterSettings.TryGetValueAsync(strSelectedSetting, _objGenericToken)
                                                        .ConfigureAwait(false);
                    }

                    if (!blnSuccess)
                    {
                        (blnSuccess, objSettings)
                            = await dicCharacterSettings.TryGetValueAsync(
                                GlobalSettings.DefaultMasterIndexSetting, _objGenericToken).ConfigureAwait(false);
                        if (!blnSuccess)
                        {
                            (blnSuccess, objSettings)
                                = await dicCharacterSettings.TryGetValueAsync(
                                                                GlobalSettings
                                                                    .DefaultMasterIndexSettingDefaultValue,
                                                                _objGenericToken)
                                                            .ConfigureAwait(false);
                            if (!blnSuccess)
                                objSettings = dicCharacterSettings.Values.First();
                        }
                    }

                    CharacterSettings objPreviousSettings = Interlocked.Exchange(ref _objSelectedSetting, objSettings);
                    if (objPreviousSettings != objSettings)
                    {
                        if (objPreviousSettings != null)
                        {
                            IAsyncDisposable objLocker = await objPreviousSettings.LockObject.EnterWriteLockAsync(_objGenericToken).ConfigureAwait(false);
                            try
                            {
                                objPreviousSettings.PropertyChanged -= OnSelectedSettingChanged;
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        if (objSettings != null)
                        {
                            IAsyncDisposable objLocker = await objSettings.LockObject.EnterWriteLockAsync(_objGenericToken).ConfigureAwait(false);
                            try
                            {
                                objSettings.PropertyChanged += OnSelectedSettingChanged;
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        await LoadContent(_objGenericToken).ConfigureAwait(false);
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

        private async Task LoadContent(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CustomActivity opLoadMasterindex = await Timekeeper.StartSyncronAsync("op_load_frm_masterindex", null,
                       CustomActivity.OperationType.RequestOperation, null, token).ConfigureAwait(false))
            {
                Interlocked.Decrement(ref _intIsFinishedLoading);
                try
                {
                    await _dicCachedNotes.ClearAsync(token).ConfigureAwait(false);
                    foreach (object objUncastedExistingEntry in _lstItems.Select(x => x.Value))
                    {
                        if (objUncastedExistingEntry is MasterIndexEntry objExistingEntry)
                            objExistingEntry.Dispose();
                    }

                    _lstItems.Clear();
                    _lstFileNamesWithItems.Clear();
                    if (_objSelectedSetting == null)
                        Interlocked.CompareExchange(ref _objSelectedSetting,
                                                    await GetInitialSetting(token).ConfigureAwait(false), null);
                    string strSourceFilter;
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setValidCodes))
                    {
                        if (_objSelectedSetting != null)
                        {
                            foreach (XPathNavigator xmlBookNode in await (await XmlManager.LoadXPathAsync(
                                                                             "books.xml", _objSelectedSetting.EnabledCustomDataDirectoryPaths,
                                                                             token: token).ConfigureAwait(false))
                                                                         .SelectAndCacheExpressionAsync("/chummer/books/book/code", token: token).ConfigureAwait(false))
                            {
                                setValidCodes.Add(xmlBookNode.Value);
                            }

                            setValidCodes.IntersectWith(_objSelectedSetting.Books);
                        }

                        strSourceFilter = setValidCodes.Count > 0
                            ? '(' + string.Join(" or ", setValidCodes.Select(x => "source = " + x.CleanXPath())) + ')'
                            : "source";
                    }

                    using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_load_andpopulate_entries",
                                                                  opLoadMasterindex, token).ConfigureAwait(false))
                    {
                        if (_objSelectedSetting != null)
                        {
                            ConcurrentBag<ListItem> lstItemsForLoading = new ConcurrentBag<ListItem>();
                            using (_ = await Timekeeper.StartSyncronAsync(
                                       "load_frm_masterindex_load_entries", opLoadMasterindex, token).ConfigureAwait(false))
                            {
                                ConcurrentBag<ListItem> lstFileNamesWithItemsForLoading = new ConcurrentBag<ListItem>();
                                // Prevents locking the UI thread while still benefiting from static scheduling of Parallel.ForEach
                                await Task.WhenAll(_astrFileNames.Select(strFileName => Task.Run(async () =>
                                {
                                    XPathNavigator xmlBaseNode
                                        = await XmlManager.LoadXPathAsync(strFileName,
                                                                          _objSelectedSetting
                                                                              .EnabledCustomDataDirectoryPaths,
                                                                          token: token).ConfigureAwait(false);
                                    xmlBaseNode
                                        = await xmlBaseNode.SelectSingleNodeAndCacheExpressionAsync(
                                            "/chummer", token: token).ConfigureAwait(false);
                                    if (xmlBaseNode == null)
                                        return;
                                    bool blnLoopFileNameHasItems = false;
                                    foreach (XPathNavigator xmlItemNode in xmlBaseNode.Select(
                                                 ".//*[page and " + strSourceFilter + ']'))
                                    {
                                        blnLoopFileNameHasItems = true;
                                        string strName
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                "name", token: token).ConfigureAwait(false))
                                            ?.Value;
                                        string strDisplayName
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                  "translate", token: token).ConfigureAwait(false))
                                              ?.Value
                                              ?? strName
                                              ?? (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                  "id", token: token).ConfigureAwait(false))?.Value
                                              ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                                        string strSource
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                "source", token: token).ConfigureAwait(false))?.Value;
                                        string strPage
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                "page", token: token).ConfigureAwait(false))
                                            ?.Value;
                                        string strDisplayPage
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                  "altpage", token: token).ConfigureAwait(false))?.Value
                                              ?? strPage;
                                        string strEnglishNameOnPage
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                  "nameonpage", token: token).ConfigureAwait(false))
                                              ?.Value
                                              ?? strName;
                                        string strTranslatedNameOnPage =
                                            (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                "altnameonpage", token: token).ConfigureAwait(false))
                                            ?.Value
                                            ?? strDisplayName;
                                        string strNotes
                                            = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                  "altnotes", token: token).ConfigureAwait(false))?.Value
                                              ?? (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync(
                                                  "notes", token: token).ConfigureAwait(false))
                                              ?.Value;
                                        MasterIndexEntry objEntry = new MasterIndexEntry(
                                            strDisplayName,
                                            strFileName,
                                            await SourceString.GetSourceStringAsync(
                                                strSource, strPage, GlobalSettings.DefaultLanguage,
                                                GlobalSettings.InvariantCultureInfo, token: token).ConfigureAwait(false),
                                            await SourceString.GetSourceStringAsync(
                                                strSource, strDisplayPage, GlobalSettings.Language,
                                                GlobalSettings.CultureInfo, token: token).ConfigureAwait(false),
                                            strEnglishNameOnPage,
                                            strTranslatedNameOnPage);
                                        lstItemsForLoading.Add(new ListItem(objEntry, strDisplayName));
                                        if (!string.IsNullOrEmpty(strNotes))
                                            await _dicCachedNotes.TryAddAsync(
                                                objEntry, Task.FromResult(strNotes), token).ConfigureAwait(false);
                                    }

                                    if (blnLoopFileNameHasItems)
                                        lstFileNamesWithItemsForLoading.Add(new ListItem(strFileName, strFileName));
                                }, token))).ConfigureAwait(false);
                                _lstFileNamesWithItems.AddRange(lstFileNamesWithItemsForLoading);
                            }

                            using (_ = await Timekeeper.StartSyncronAsync(
                                       "load_frm_masterindex_populate_entries", opLoadMasterindex, token).ConfigureAwait(false))
                            {
                                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                                string strFormat = "{0}" + strSpace + "[{1}]";
                                Dictionary<string, List<ListItem>> dicHelper
                                    = new Dictionary<string, List<ListItem>>(lstItemsForLoading.Count);
                                try
                                {
                                    foreach (ListItem objItem in lstItemsForLoading)
                                    {
                                        if (!(objItem.Value is MasterIndexEntry objEntry))
                                            continue;
                                        string strKey = objEntry.DisplayName.ToUpperInvariant();
                                        if (dicHelper.TryGetValue(strKey, out List<ListItem> lstExistingItems))
                                        {
                                            ListItem objExistingItem = lstExistingItems.Find(
                                                x => x.Value is MasterIndexEntry y
                                                     && objEntry.DisplaySource.Equals(y.DisplaySource));
                                            if (objExistingItem.Value is MasterIndexEntry objLoopEntry)
                                            {
                                                objLoopEntry.FileNames.UnionWith(objEntry.FileNames);
                                                objEntry.Dispose();
                                            }
                                            else
                                            {
                                                using (new FetchSafelyFromPool<List<ListItem>>(
                                                           Utils.ListItemListPool,
                                                           out List<ListItem> lstItemsNeedingNameChanges))
                                                {
                                                    lstItemsNeedingNameChanges.AddRange(lstExistingItems.FindAll(
                                                        x => x.Value is MasterIndexEntry y
                                                             && !objEntry.FileNames.IsSubsetOf(y.FileNames)));
                                                    if (lstItemsNeedingNameChanges.Count == 0)
                                                    {
                                                        _lstItems.Add(
                                                            objItem); // Not using AddRange because of potential memory issues
                                                        lstExistingItems.Add(objItem);
                                                    }
                                                    else
                                                    {
                                                        ListItem objItemToAdd = new ListItem(
                                                            objItem.Value, string.Format(GlobalSettings.CultureInfo,
                                                                strFormat, objItem.Name,
                                                                string.Join(
                                                                    ',' + strSpace, objEntry.FileNames)));
                                                        _lstItems.Add(
                                                            objItemToAdd); // Not using AddRange because of potential memory issues
                                                        lstExistingItems.Add(objItemToAdd);

                                                        foreach (ListItem objToRename in lstItemsNeedingNameChanges)
                                                        {
                                                            _lstItems.Remove(objToRename);
                                                            lstExistingItems.Remove(objToRename);

                                                            if (!(objToRename
                                                                    .Value is MasterIndexEntry objExistingEntry))
                                                                continue;
                                                            objItemToAdd = new ListItem(
                                                                objToRename.Value, string.Format(
                                                                    GlobalSettings.CultureInfo,
                                                                    strFormat, objExistingEntry.DisplayName,
                                                                    string.Join(
                                                                        ',' + strSpace,
                                                                        objExistingEntry.FileNames)));
                                                            _lstItems.Add(
                                                                objItemToAdd); // Not using AddRange because of potential memory issues
                                                            lstExistingItems.Add(objItemToAdd);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _lstItems.Add(
                                                objItem); // Not using AddRange because of potential memory issues
                                            List<ListItem> lstHelperItems = Utils.ListItemListPool.Get();
                                            lstHelperItems.Add(objItem);
                                            dicHelper.Add(strKey, lstHelperItems);
                                        }
                                    }
                                }
                                finally
                                {
                                    List<List<ListItem>> lstToReturn = dicHelper.Values.ToList();
                                    for (int i = lstToReturn.Count - 1; i >= 0; --i)
                                    {
                                        List<ListItem> lstLoop = lstToReturn[i];
                                        Utils.ListItemListPool.Return(ref lstLoop);
                                    }
                                }
                            }
                        }
                    }

                    using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_sort_entries", opLoadMasterindex, token).ConfigureAwait(false))
                    {
                        _lstItems.Sort(CompareListItems.CompareNames);
                        _lstFileNamesWithItems.Sort(CompareListItems.CompareNames);
                    }

                    using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_populate_controls", opLoadMasterindex, token).ConfigureAwait(false))
                    {
                        _lstFileNamesWithItems.Insert(
                            0, new ListItem(string.Empty, await LanguageManager.GetStringAsync("String_All", token: token).ConfigureAwait(false)));

                        int intOldSelectedIndex = await cboFile.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false);
                        await cboFile.PopulateWithListItemsAsync(_lstFileNamesWithItems, token).ConfigureAwait(false);
                        await cboFile.DoThreadSafeAsync(x =>
                        {
                            try
                            {
                                x.SelectedIndex = Math.Max(intOldSelectedIndex, 0);
                            }
                            // For some reason, some unit tests will fire this exception even when _lstFileNamesWithItems is explicitly checked for having enough items
                            catch (ArgumentOutOfRangeException)
                            {
                                x.SelectedIndex = -1;
                            }
                        }, token).ConfigureAwait(false);
                        await lstItems.PopulateWithListItemsAsync(_lstItems, token).ConfigureAwait(false);
                        await lstItems.DoThreadSafeAsync(x => x.SelectedIndex = -1, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipRefresh);
                    Interlocked.Increment(ref _intIsFinishedLoading);
                }
            }
        }

        private async void lblSource_Click(object sender, EventArgs e)
        {
            try
            {
                await CommonFunctions.OpenPdfFromControl(sender, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshList(object sender, EventArgs e)
        {
            if (_intSkipRefresh > 0)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    bool blnCustomList
                        = !(await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, _objGenericToken).ConfigureAwait(false) == 0
                            && string.IsNullOrEmpty(
                                await cboFile.DoThreadSafeFuncAsync(
                                    x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false)));
                    List<ListItem> lstFilteredItems = blnCustomList ? Utils.ListItemListPool.Get() : _lstItems;
                    try
                    {
                        if (blnCustomList)
                        {
                            string strFileFilter
                                = await cboFile.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(),
                                                                      _objGenericToken).ConfigureAwait(false) ?? string.Empty;
                            string strSearchFilter
                                = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, _objGenericToken).ConfigureAwait(false);
                            foreach (ListItem objItem in _lstItems)
                            {
                                if (!(objItem.Value is MasterIndexEntry objItemEntry))
                                    continue;
                                if (!string.IsNullOrEmpty(strFileFilter)
                                    && !objItemEntry.FileNames.Contains(strFileFilter))
                                    continue;
                                if (!string.IsNullOrEmpty(strSearchFilter))
                                {
                                    string strDisplayNameNoFile = objItemEntry.DisplayName;
                                    if (strDisplayNameNoFile.EndsWith(".xml]", StringComparison.OrdinalIgnoreCase))
                                        strDisplayNameNoFile = strDisplayNameNoFile
                                                               .Substring(0, strDisplayNameNoFile.LastIndexOf('['))
                                                               .Trim();
                                    if (strDisplayNameNoFile.IndexOf(strSearchFilter,
                                                                     StringComparison.OrdinalIgnoreCase)
                                        == -1)
                                        continue;
                                }

                                lstFilteredItems.Add(objItem);
                            }
                        }

                        object objOldSelectedValue
                            = await lstItems.DoThreadSafeFuncAsync(x => x.SelectedValue, _objGenericToken).ConfigureAwait(false);
                        Interlocked.Increment(ref _intSkipRefresh);
                        try
                        {
                            await lstItems.PopulateWithListItemsAsync(lstFilteredItems, _objGenericToken)
                                          .ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intSkipRefresh);
                        }

                        if (objOldSelectedValue is MasterIndexEntry objOldSelectedEntry)
                            await lstItems.DoThreadSafeFuncAsync(
                                x => x.SelectedIndex
                                    // ReSharper disable once AccessToModifiedClosure
                                    = lstFilteredItems.FindIndex(
                                        y => objOldSelectedEntry.Equals(y.Value as MasterIndexEntry)),
                                _objGenericToken).ConfigureAwait(false);
                        else
                            await lstItems.DoThreadSafeFuncAsync(x => x.SelectedIndex = -1, _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (blnCustomList)
                            Utils.ListItemListPool.Return(ref lstFilteredItems);
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

        private async void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intSkipRefresh > 0)
                return;

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    if (await lstItems.DoThreadSafeFuncAsync(x => x.SelectedValue, _objGenericToken).ConfigureAwait(false) is
                        MasterIndexEntry objEntry)
                    {
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                        await lblSourceClickReminder.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                        await objEntry.DisplaySource.SetControlAsync(lblSource, _objGenericToken).ConfigureAwait(false);
                        (bool blnSuccess, Task<string> tskNotes)
                            = await _dicCachedNotes.TryGetValueAsync(objEntry, _objGenericToken).ConfigureAwait(false);
                        if (!blnSuccess)
                        {
                            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                StringComparison.OrdinalIgnoreCase)
                                && (objEntry.TranslatedNameOnPage != objEntry.EnglishNameOnPage
                                    || objEntry.Source.Page != objEntry.DisplaySource.Page))
                            {
                                // don't check again it is not translated
                                tskNotes = Task.Run(async () =>
                                {
                                    string strReturn = await CommonFunctions.GetTextFromPdfAsync(
                                        objEntry.Source.ToString(),
                                        objEntry.EnglishNameOnPage, token: _objGenericToken).ConfigureAwait(false);
                                    if (string.IsNullOrEmpty(strReturn))
                                        strReturn = await CommonFunctions.GetTextFromPdfAsync(
                                            objEntry.DisplaySource.ToString(), objEntry.TranslatedNameOnPage, token: _objGenericToken).ConfigureAwait(false);
                                    return strReturn;
                                }, _objGenericToken);
                            }
                            else
                            {
                                tskNotes = Task.Run(() =>
                                                        CommonFunctions.GetTextFromPdfAsync(
                                                            objEntry.Source.ToString(),
                                                            objEntry.EnglishNameOnPage, token: _objGenericToken), _objGenericToken);
                            }

                            await _dicCachedNotes.TryAddAsync(objEntry, tskNotes, _objGenericToken).ConfigureAwait(false);
                        }

                        string strNotes = await tskNotes.ConfigureAwait(false);
                        await txtNotes.DoThreadSafeAsync(x =>
                        {
                            x.Text = strNotes;
                            x.Visible = true;
                        }, _objGenericToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                        await lblSourceClickReminder.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                        await SourceString.Blank.SetControlAsync(lblSource, _objGenericToken).ConfigureAwait(false);
                        await txtNotes.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
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

        private sealed class MasterIndexEntry : IDisposable
        {
            private HashSet<string> _setFileNames;

            public MasterIndexEntry(string strDisplayName, string strFileName, SourceString objSource, SourceString objDisplaySource, string strEnglishNameOnPage, string strTranslatedNameOnPage)
            {
                DisplayName = strDisplayName;
                _setFileNames = Utils.StringHashSetPool.Get();
                _setFileNames.Add(strFileName);
                Source = objSource;
                DisplaySource = objDisplaySource;
                EnglishNameOnPage = strEnglishNameOnPage;
                TranslatedNameOnPage = strTranslatedNameOnPage;
            }

            internal string DisplayName { get; }

            internal HashSet<string> FileNames => _setFileNames;

            internal SourceString Source { get; }
            internal SourceString DisplaySource { get; }
            internal string EnglishNameOnPage { get; }
            internal string TranslatedNameOnPage { get; }

            /// <inheritdoc />
            public void Dispose()
            {
                Utils.StringHashSetPool.Return(ref _setFileNames);
            }
        }

        private async void cmdEditCharacterSetting_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    using (ThreadSafeForm<EditCharacterSettings> frmOptions
                           = await ThreadSafeForm<EditCharacterSettings>.GetAsync(
                               () => new EditCharacterSettings(cboCharacterSetting.SelectedValue as CharacterSettings),
                               _objGenericToken).ConfigureAwait(false))
                        await frmOptions.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false);
                    // Do not repopulate the character settings list because that will happen from frmCharacterSettings where appropriate
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

        public async ValueTask ForceRepopulateCharacterSettings(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    await PopulateCharacterSettings(token).ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intIsFinishedLoading;

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading => _intIsFinishedLoading > 0;
    }
}
