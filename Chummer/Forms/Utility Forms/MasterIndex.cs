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
        private bool _blnSkipRefresh = true;
        private CharacterSettings _objSelectedSetting = GetInitialSetting();
        private readonly LockingDictionary<MasterIndexEntry, Task<string>> _dicCachedNotes = new LockingDictionary<MasterIndexEntry, Task<string>>();
        private readonly List<ListItem> _lstFileNamesWithItems = Utils.ListItemListPool.Get();
        private readonly List<ListItem> _lstItems = Utils.ListItemListPool.Get();
        private readonly CancellationTokenSource _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        private static CharacterSettings GetInitialSetting()
        {
            if (SettingsManager.LoadedCharacterSettings.TryGetValue(GlobalSettings.DefaultMasterIndexSetting,
                                                                    out CharacterSettings objReturn))
                return objReturn;
            return SettingsManager.LoadedCharacterSettings.TryGetValue(
                GlobalSettings.DefaultMasterIndexSettingDefaultValue,
                out objReturn)
                ? objReturn
                : SettingsManager.LoadedCharacterSettings.Values.First();
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
                foreach (CharacterSettings objLoopSettings in SettingsManager.LoadedCharacterSettings.Select(
                             x => x.Value))
                {
                    lstCharacterSettings.Add(new ListItem(objLoopSettings, objLoopSettings.DisplayName));
                }

                lstCharacterSettings.Sort(CompareListItems.CompareNames);

                string strOldSettingKey = (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, token) as CharacterSettings)?.DictionaryKey
                                          ?? _objSelectedSetting?.DictionaryKey;

                bool blnOldSkipRefresh = _blnSkipRefresh;
                _blnSkipRefresh = true;
                CharacterSettings objSettings;
                bool blnSuccess;
                
                await cboCharacterSetting.PopulateWithListItemsAsync(lstCharacterSettings, token);
                if (!string.IsNullOrEmpty(strOldSettingKey))
                {
                    (blnSuccess, objSettings)
                        = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(strOldSettingKey);
                    if (blnSuccess)
                        await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSettings, token);
                }

                _blnSkipRefresh = blnOldSkipRefresh;

                if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) != -1)
                    return;
                (blnSuccess, objSettings)
                    = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(GlobalSettings.DefaultMasterIndexSetting);
                if (blnSuccess)
                    await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSettings, token);
                else
                {
                    (blnSuccess, objSettings)
                        = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(GlobalSettings.DefaultMasterIndexSettingDefaultValue);
                    if (blnSuccess)
                        await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedValue = objSettings, token);
                }
                if (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) == -1 && lstCharacterSettings.Count > 0)
                    await cboCharacterSetting.DoThreadSafeAsync(x => x.SelectedIndex = 0, token);
            }
        }

        private async void MasterIndex_Load(object sender, EventArgs e)
        {
            try
            {
                using (CursorWait.New(this))
                {
                    await PopulateCharacterSettings(_objGenericToken);
                    await LoadContent(_objGenericToken).AsTask()
                                                       .ContinueWith(x => IsFinishedLoading = true, _objGenericToken);
                    _objSelectedSetting.PropertyChanged += OnSelectedSettingChanged;
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void MasterIndex_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objSelectedSetting.PropertyChanged -= OnSelectedSettingChanged;
            _objGenericFormClosingCancellationTokenSource?.Cancel(false);
            foreach (Task<string> tskLoop in _dicCachedNotes.Values)
            {
                try
                {
                    await tskLoop;
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
                using (CursorWait.New(this))
                {
                    try
                    {
                        await LoadContent(_objGenericToken);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                }
            }
        }

        private async void cboCharacterSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            using (CursorWait.New(this))
            {
                try
                {
                    string strSelectedSetting
                        = (await cboCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue, _objGenericToken) as
                            CharacterSettings)?.DictionaryKey;
                    CharacterSettings objSettings = null;
                    bool blnSuccess = false;
                    if (!string.IsNullOrEmpty(strSelectedSetting))
                    {
                        (blnSuccess, objSettings)
                            = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(strSelectedSetting);
                    }

                    if (!blnSuccess)
                    {
                        (blnSuccess, objSettings)
                            = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                GlobalSettings.DefaultMasterIndexSetting);
                        if (!blnSuccess)
                        {
                            (blnSuccess, objSettings)
                                = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                    GlobalSettings.DefaultMasterIndexSettingDefaultValue);
                            if (!blnSuccess)
                                objSettings = SettingsManager.LoadedCharacterSettings.Values.First();
                        }
                    }

                    if (objSettings != _objSelectedSetting)
                    {
                        _objSelectedSetting.PropertyChanged -= OnSelectedSettingChanged;
                        _objSelectedSetting = objSettings;
                        _objSelectedSetting.PropertyChanged += OnSelectedSettingChanged;

                        await LoadContent(_objGenericToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private async ValueTask LoadContent(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CustomActivity opLoadMasterindex = await Timekeeper.StartSyncronAsync("op_load_frm_masterindex", null,
                       CustomActivity.OperationType.RequestOperation, null))
            {
                bool blnOldIsFinishedLoading = IsFinishedLoading;
                try
                {
                    IsFinishedLoading = false;
                    await _dicCachedNotes.ClearAsync();
                    foreach (MasterIndexEntry objExistingEntry in _lstItems.Select(x => x.Value))
                        objExistingEntry.Dispose();
                    _lstItems.Clear();
                    _lstFileNamesWithItems.Clear();
                    string strSourceFilter;
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setValidCodes))
                    {
                        foreach (XPathNavigator xmlBookNode in await (await XmlManager.LoadXPathAsync(
                                         "books.xml", _objSelectedSetting.EnabledCustomDataDirectoryPaths))
                                     .SelectAndCacheExpressionAsync("/chummer/books/book/code"))
                        {
                            setValidCodes.Add(xmlBookNode.Value);
                        }

                        setValidCodes.IntersectWith(_objSelectedSetting.Books);

                        strSourceFilter = setValidCodes.Count > 0
                            ? '(' + string.Join(" or ", setValidCodes.Select(x => "source = " + x.CleanXPath())) + ')'
                            : "source";
                    }

                    using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_load_andpopulate_entries",
                                                                  opLoadMasterindex))
                    {
                        ConcurrentBag<ListItem> lstItemsForLoading = new ConcurrentBag<ListItem>();
                        using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_load_entries", opLoadMasterindex))
                        {
                            ConcurrentBag<ListItem> lstFileNamesWithItemsForLoading = new ConcurrentBag<ListItem>();
                            // Prevents locking the UI thread while still benefiting from static scheduling of Parallel.ForEach
                            await Task.WhenAll(_astrFileNames.Select(strFileName => Task.Run(async () =>
                            {
                                XPathNavigator xmlBaseNode
                                    = await XmlManager.LoadXPathAsync(strFileName,
                                                                      _objSelectedSetting
                                                                          .EnabledCustomDataDirectoryPaths);
                                xmlBaseNode = await xmlBaseNode.SelectSingleNodeAndCacheExpressionAsync("/chummer");
                                if (xmlBaseNode == null)
                                    return;
                                bool blnLoopFileNameHasItems = false;
                                foreach (XPathNavigator xmlItemNode in xmlBaseNode.Select(
                                             ".//*[page and " + strSourceFilter + ']'))
                                {
                                    blnLoopFileNameHasItems = true;
                                    string strName = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("name"))
                                        ?.Value;
                                    string strDisplayName
                                        = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("translate"))
                                          ?.Value
                                          ?? strName
                                          ?? (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value
                                          ?? await LanguageManager.GetStringAsync("String_Unknown");
                                    string strSource
                                        = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("source"))?.Value;
                                    string strPage = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("page"))
                                        ?.Value;
                                    string strDisplayPage
                                        = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value
                                          ?? strPage;
                                    string strEnglishNameOnPage
                                        = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("nameonpage"))
                                          ?.Value
                                          ?? strName;
                                    string strTranslatedNameOnPage =
                                        (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("altnameonpage"))
                                        ?.Value
                                        ?? strDisplayName;
                                    string strNotes
                                        = (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("altnotes"))?.Value
                                          ?? (await xmlItemNode.SelectSingleNodeAndCacheExpressionAsync("notes"))
                                          ?.Value;
                                    MasterIndexEntry objEntry = new MasterIndexEntry(
                                        strDisplayName,
                                        strFileName,
                                        await SourceString.GetSourceStringAsync(
                                            strSource, strPage, GlobalSettings.DefaultLanguage,
                                            GlobalSettings.InvariantCultureInfo),
                                        await SourceString.GetSourceStringAsync(
                                            strSource, strDisplayPage, GlobalSettings.Language,
                                            GlobalSettings.CultureInfo),
                                        strEnglishNameOnPage,
                                        strTranslatedNameOnPage);
                                    lstItemsForLoading.Add(new ListItem(objEntry, strDisplayName));
                                    if (!string.IsNullOrEmpty(strNotes))
                                        await _dicCachedNotes.TryAddAsync(objEntry, Task.FromResult(strNotes));
                                }

                                if (blnLoopFileNameHasItems)
                                    lstFileNamesWithItemsForLoading.Add(new ListItem(strFileName, strFileName));
                            }, token)));
                            _lstFileNamesWithItems.AddRange(lstFileNamesWithItemsForLoading);
                        }

                        using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_populate_entries", opLoadMasterindex))
                        {
                            string strSpace = await LanguageManager.GetStringAsync("String_Space");
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

                                                        if (!(objToRename.Value is MasterIndexEntry objExistingEntry))
                                                            continue;
                                                        objItemToAdd = new ListItem(objToRename.Value, string.Format(
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
                                        _lstItems.Add(objItem); // Not using AddRange because of potential memory issues
                                        List<ListItem> lstHelperItems = Utils.ListItemListPool.Get();
                                        lstHelperItems.Add(objItem);
                                        dicHelper.Add(strKey, lstHelperItems);
                                    }
                                }
                            }
                            finally
                            {
                                foreach (List<ListItem> lstHelperItems in dicHelper.Values)
                                    Utils.ListItemListPool.Return(lstHelperItems);
                            }
                        }
                    }

                    using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_sort_entries", opLoadMasterindex))
                    {
                        _lstItems.Sort(CompareListItems.CompareNames);
                        _lstFileNamesWithItems.Sort(CompareListItems.CompareNames);
                    }

                    using (_ = await Timekeeper.StartSyncronAsync("load_frm_masterindex_populate_controls", opLoadMasterindex))
                    {
                        _lstFileNamesWithItems.Insert(
                            0, new ListItem(string.Empty, await LanguageManager.GetStringAsync("String_All")));

                        int intOldSelectedIndex = cboFile.DoThreadSafeFunc(x => x.SelectedIndex, token);
                        await Task.WhenAll(
                            cboFile.PopulateWithListItemsAsync(_lstFileNamesWithItems, token).ContinueWith(
                                y => cboFile.DoThreadSafe
                                (x =>
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
                                }), token),
                            lstItems.PopulateWithListItemsAsync(_lstItems, token).ContinueWith(y => lstItems.DoThreadSafe(x => x.SelectedIndex = -1), token));
                    }
                }
                finally
                {
                    _blnSkipRefresh = false;
                    IsFinishedLoading = blnOldIsFinishedLoading;
                }
            }
        }

        private async void lblSource_Click(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        private async void RefreshList(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            using (CursorWait.New(this))
            {
                bool blnCustomList = !(txtSearch.TextLength == 0 && string.IsNullOrEmpty(cboFile.SelectedValue?.ToString()));
                List<ListItem> lstFilteredItems = blnCustomList ? Utils.ListItemListPool.Get() : _lstItems;
                try
                {
                    if (blnCustomList)
                    {
                        string strFileFilter = cboFile.SelectedValue?.ToString() ?? string.Empty;
                        string strSearchFilter = txtSearch.Text;
                        foreach (ListItem objItem in _lstItems)
                        {
                            if (!(objItem.Value is MasterIndexEntry objItemEntry))
                                continue;
                            if (!string.IsNullOrEmpty(strFileFilter) && !objItemEntry.FileNames.Contains(strFileFilter))
                                continue;
                            if (!string.IsNullOrEmpty(strSearchFilter))
                            {
                                string strDisplayNameNoFile = objItemEntry.DisplayName;
                                if (strDisplayNameNoFile.EndsWith(".xml]", StringComparison.OrdinalIgnoreCase))
                                    strDisplayNameNoFile = strDisplayNameNoFile
                                                           .Substring(0, strDisplayNameNoFile.LastIndexOf('[')).Trim();
                                if (strDisplayNameNoFile.IndexOf(strSearchFilter, StringComparison.OrdinalIgnoreCase)
                                    == -1)
                                    continue;
                            }

                            lstFilteredItems.Add(objItem);
                        }
                    }

                    object objOldSelectedValue = lstItems.SelectedValue;
                    _blnSkipRefresh = true;
                    await lstItems.PopulateWithListItemsAsync(lstFilteredItems, _objGenericToken);
                    _blnSkipRefresh = false;
                    if (objOldSelectedValue is MasterIndexEntry objOldSelectedEntry)
                        lstItems.SelectedIndex
                            = lstFilteredItems.FindIndex(x => objOldSelectedEntry.Equals(x.Value as MasterIndexEntry));
                    else
                        lstItems.SelectedIndex = -1;
                }
                finally
                {
                    if (blnCustomList)
                        Utils.ListItemListPool.Return(lstFilteredItems);
                }
            }
        }

        private async void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            using (CursorWait.New(this))
            {
                try
                {
                    if (lstItems.DoThreadSafeFunc(x => x.SelectedValue, _objGenericToken) is MasterIndexEntry objEntry)
                    {
                        await Task.WhenAll(lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken),
                                           lblSourceClickReminder.DoThreadSafeAsync(
                                               x => x.Visible = true, _objGenericToken),
                                           lblSource.DoThreadSafeAsync(
                                               x =>
                                               {
                                                   x.Visible = true;
                                                   x.Text = objEntry.DisplaySource.ToString();
                                               }, _objGenericToken),
                                           lblSource.SetToolTipAsync(objEntry.DisplaySource.LanguageBookTooltip,
                                                                     _objGenericToken));
                        (bool blnSuccess, Task<string> tskNotes) = await _dicCachedNotes.TryGetValueAsync(objEntry);
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
                                        objEntry.EnglishNameOnPage);
                                    if (string.IsNullOrEmpty(strReturn))
                                        strReturn = await CommonFunctions.GetTextFromPdfAsync(
                                            objEntry.DisplaySource.ToString(), objEntry.TranslatedNameOnPage);
                                    return strReturn;
                                }, _objGenericToken);
                            }
                            else
                            {
                                tskNotes = Task.Run(() =>
                                                        CommonFunctions.GetTextFromPdfAsync(objEntry.Source.ToString(),
                                                            objEntry.EnglishNameOnPage), _objGenericToken);
                            }

                            await _dicCachedNotes.TryAddAsync(objEntry, tskNotes);
                        }

                        string strNotes = await tskNotes;
                        await txtNotes.DoThreadSafeAsync(x =>
                        {
                            x.Text = strNotes;
                            x.Visible = true;
                        }, _objGenericToken);
                    }
                    else
                    {
                        await Task.WhenAll(lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken),
                                           lblSource.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken),
                                           lblSourceClickReminder.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken),
                                           txtNotes.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken));
                    }
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private sealed class MasterIndexEntry : IDisposable
        {
            public MasterIndexEntry(string strDisplayName, string strFileName, SourceString objSource, SourceString objDisplaySource, string strEnglishNameOnPage, string strTranslatedNameOnPage)
            {
                DisplayName = strDisplayName;
                FileNames = Utils.StringHashSetPool.Get();
                FileNames.Add(strFileName);
                Source = objSource;
                DisplaySource = objDisplaySource;
                EnglishNameOnPage = strEnglishNameOnPage;
                TranslatedNameOnPage = strTranslatedNameOnPage;
            }

            internal string DisplayName { get; }
            internal HashSet<string> FileNames { get; }
            internal SourceString Source { get; }
            internal SourceString DisplaySource { get; }
            internal string EnglishNameOnPage { get; }
            internal string TranslatedNameOnPage { get; }

            /// <inheritdoc />
            public void Dispose()
            {
                Utils.StringHashSetPool.Return(FileNames);
            }
        }

        private async void cmdEditCharacterSetting_Click(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
            {
                using (EditCharacterSettings frmOptions
                       = new EditCharacterSettings(cboCharacterSetting.SelectedValue as CharacterSettings))
                    await frmOptions.ShowDialogSafeAsync(this);
                // Do not repopulate the character settings list because that will happen from frmCharacterSettings where appropriate
            }
        }

        public async ValueTask ForceRepopulateCharacterSettings(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token);
                try
                {
                    await PopulateCharacterSettings(token);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), token);
                }
            }
        }

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading { get; private set; }
    }
}
