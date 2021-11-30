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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmMasterIndex : Form
    {
        private bool _blnSkipRefresh = true;
        private CharacterSettings _objSelectedSetting = SettingsManager.LoadedCharacterSettings[GlobalSettings.DefaultMasterIndexSetting];
        private readonly List<ListItem> _lstFileNamesWithItems;
        private readonly LockingDictionary<MasterIndexEntry, string> _dicCachedNotes = new LockingDictionary<MasterIndexEntry, string>();
        private readonly List<ListItem> _lstItems = new List<ListItem>(short.MaxValue);

        private readonly List<string> _lstFileNames = new List<string>
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
        };

        public frmMasterIndex()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _lstFileNamesWithItems = new List<ListItem>(_lstFileNames.Count);
            PopulateCharacterSettings();
        }

        private void PopulateCharacterSettings()
        {
            // Populate the Character Settings list.
            List<ListItem> lstCharacterSettings = new List<ListItem>(SettingsManager.LoadedCharacterSettings.Count);
            foreach (KeyValuePair<string, CharacterSettings> objLoopOptions in SettingsManager.LoadedCharacterSettings)
            {
                lstCharacterSettings.Add(new ListItem(objLoopOptions.Key, objLoopOptions.Value.DisplayName));
            }
            lstCharacterSettings.Sort(CompareListItems.CompareNames);

            string strOldSetting = cboCharacterSetting.SelectedValue?.ToString();

            bool blnOldSkipRefresh = _blnSkipRefresh;
            _blnSkipRefresh = true;

            cboCharacterSetting.BeginUpdate();
            cboCharacterSetting.PopulateWithListItems(lstCharacterSettings);
            if (!string.IsNullOrEmpty(strOldSetting) && SettingsManager.LoadedCharacterSettings.ContainsKey(strOldSetting))
                cboCharacterSetting.SelectedValue = SettingsManager.LoadedCharacterSettings[strOldSetting];
            cboCharacterSetting.EndUpdate();

            _blnSkipRefresh = blnOldSkipRefresh;

            if (cboCharacterSetting.SelectedIndex != -1)
                return;
            cboCharacterSetting.SelectedValue = SettingsManager.LoadedCharacterSettings[GlobalSettings.DefaultMasterIndexSetting];
            if (cboCharacterSetting.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                cboCharacterSetting.SelectedIndex = 0;
        }

        private async void frmMasterIndex_Load(object sender, EventArgs e)
        {
            await LoadContent();
            _objSelectedSetting.PropertyChanged += OnSelectedSettingChanged;
        }

        private async void OnSelectedSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CharacterSettings.Books) && e.PropertyName != nameof(CharacterSettings.EnabledCustomDataDirectoryPaths))
                return;
            using (new CursorWait(this))
                await LoadContent();
        }

        private async void cboCharacterSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strSelectedSetting = cboCharacterSetting.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedSetting) || !SettingsManager.LoadedCharacterSettings.ContainsKey(strSelectedSetting))
                strSelectedSetting = GlobalSettings.DefaultMasterIndexSetting;

            if (strSelectedSetting == _objSelectedSetting.DictionaryKey)
                return;

            _objSelectedSetting.PropertyChanged -= OnSelectedSettingChanged;
            _objSelectedSetting = SettingsManager.LoadedCharacterSettings[strSelectedSetting];
            _objSelectedSetting.PropertyChanged += OnSelectedSettingChanged;

            using (new CursorWait(this))
                await LoadContent();
        }

        private async Task LoadContent()
        {
            using (CustomActivity opLoadFrmMasterindex = Timekeeper.StartSyncron("op_load_frm_masterindex", null,
                CustomActivity.OperationType.RequestOperation, null))
            {
                _dicCachedNotes.Clear();
                _lstItems.Clear();
                _lstFileNamesWithItems.Clear();

                HashSet<string> setValidCodes = new HashSet<string>();
                foreach (XPathNavigator xmlBookNode in (await XmlManager.LoadXPathAsync("books.xml", _objSelectedSetting.EnabledCustomDataDirectoryPaths))
                    .SelectAndCacheExpression("/chummer/books/book/code"))
                {
                    setValidCodes.Add(xmlBookNode.Value);
                }

                setValidCodes.IntersectWith(_objSelectedSetting.Books);

                string strSourceFilter = setValidCodes.Count > 0
                    ? '(' + string.Join(" or ", setValidCodes.Select(x => "source = \'" + x + "\'")) + ')'
                    : "source";

                ConcurrentBag<ListItem> lstItemsForLoading = new ConcurrentBag<ListItem>();
                ConcurrentBag<ListItem> lstFileNamesWithItemsForLoading = new ConcurrentBag<ListItem>();
                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_load_entries", opLoadFrmMasterindex))
                {
                    // Prevents locking the UI thread while still benefitting from static scheduling of Parallel.ForEach
                    await Task.WhenAll(_lstFileNames.Select(strFileName => Task.Run(async () =>
                    {
                        XPathNavigator xmlBaseNode = await XmlManager.LoadXPathAsync(strFileName, _objSelectedSetting.EnabledCustomDataDirectoryPaths);
                        xmlBaseNode = xmlBaseNode.SelectSingleNodeAndCacheExpression("/chummer");
                        if (xmlBaseNode == null)
                            return;
                        bool blnLoopFileNameHasItems = false;
                        foreach (XPathNavigator xmlItemNode in xmlBaseNode.SelectAndCacheExpression(".//*[page and " +
                            strSourceFilter + ']'))
                        {
                            blnLoopFileNameHasItems = true;
                            string strName = xmlItemNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                            string strDisplayName = xmlItemNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                    ?? strName
                                                    ?? xmlItemNode.SelectSingleNodeAndCacheExpression("id")?.Value
                                                    ?? LanguageManager.GetString("String_Unknown");
                            string strSource = xmlItemNode.SelectSingleNodeAndCacheExpression("source")?.Value;
                            string strPage = xmlItemNode.SelectSingleNodeAndCacheExpression("page")?.Value;
                            string strDisplayPage = xmlItemNode.SelectSingleNodeAndCacheExpression("altpage")?.Value
                                                    ?? strPage;
                            string strEnglishNameOnPage = xmlItemNode.SelectSingleNodeAndCacheExpression("nameonpage")?.Value
                                                          ?? strName;
                            string strTranslatedNameOnPage =
                                xmlItemNode.SelectSingleNodeAndCacheExpression("altnameonpage")?.Value
                                ?? strDisplayName;
                            string strNotes = xmlItemNode.SelectSingleNodeAndCacheExpression("altnotes")?.Value
                                              ?? xmlItemNode.SelectSingleNodeAndCacheExpression("notes")?.Value;
                            MasterIndexEntry objEntry = new MasterIndexEntry(
                                strDisplayName,
                                strFileName,
                                new SourceString(strSource, strPage, GlobalSettings.DefaultLanguage,
                                    GlobalSettings.InvariantCultureInfo),
                                new SourceString(strSource, strDisplayPage, GlobalSettings.Language,
                                    GlobalSettings.CultureInfo),
                                strEnglishNameOnPage,
                                strTranslatedNameOnPage);
                            lstItemsForLoading.Add(new ListItem(objEntry, strDisplayName));
                            if (!string.IsNullOrEmpty(strNotes))
                                _dicCachedNotes.TryAdd(objEntry, strNotes);
                        }

                        if (blnLoopFileNameHasItems)
                            lstFileNamesWithItemsForLoading.Add(new ListItem(strFileName, strFileName));
                    })));
                }

                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_populate_entries", opLoadFrmMasterindex))
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    string strFormat = "{0}" + strSpace + "[{1}]";
                    Dictionary<string, List<ListItem>> dicHelper = new Dictionary<string, List<ListItem>>(lstItemsForLoading.Count);
                    foreach (ListItem objItem in lstItemsForLoading)
                    {
                        MasterIndexEntry objEntry = (MasterIndexEntry)objItem.Value;
                        string strKey = objEntry.DisplayName.ToUpperInvariant();
                        if (dicHelper.TryGetValue(strKey, out List<ListItem> lstExistingItems))
                        {
                            ListItem objExistingItem = lstExistingItems.Find(x => objEntry.DisplaySource.Equals(((MasterIndexEntry)x.Value).DisplaySource));
                            if (objExistingItem.Value != null)
                            {
                                ((MasterIndexEntry)objExistingItem.Value).FileNames.UnionWith(objEntry.FileNames);
                            }
                            else
                            {
                                List<ListItem> lstItemsNeedingNameChanges = lstExistingItems.FindAll(x => !objEntry.FileNames.IsSubsetOf(((MasterIndexEntry)x.Value).FileNames));
                                if (lstItemsNeedingNameChanges.Count == 0)
                                {
                                    _lstItems.Add(objItem); // Not using AddRange because of potential memory issues
                                    lstExistingItems.Add(objItem);
                                }
                                else
                                {
                                    ListItem objItemToAdd = new ListItem(objItem.Value, string.Format(GlobalSettings.CultureInfo,
                                        strFormat, objItem.Name, string.Join(',' + strSpace, objEntry.FileNames)));
                                    _lstItems.Add(objItemToAdd); // Not using AddRange because of potential memory issues
                                    lstExistingItems.Add(objItemToAdd);

                                    foreach (ListItem objToRename in lstItemsNeedingNameChanges)
                                    {
                                        _lstItems.Remove(objToRename);
                                        lstExistingItems.Remove(objToRename);

                                        MasterIndexEntry objExistingEntry = (MasterIndexEntry)objToRename.Value;
                                        objItemToAdd = new ListItem(objToRename.Value, string.Format(GlobalSettings.CultureInfo,
                                            strFormat, objExistingEntry.DisplayName, string.Join(',' + strSpace, objExistingEntry.FileNames)));
                                        _lstItems.Add(objItemToAdd); // Not using AddRange because of potential memory issues
                                        lstExistingItems.Add(objItemToAdd);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _lstItems.Add(objItem); // Not using AddRange because of potential memory issues
                            dicHelper.Add(strKey, new List<ListItem>(objItem.Yield()));
                        }
                    }
                    _lstFileNamesWithItems.AddRange(lstFileNamesWithItemsForLoading);
                }

                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_sort_entries", opLoadFrmMasterindex))
                {
                    _lstItems.Sort(CompareListItems.CompareNames);
                    _lstFileNamesWithItems.Sort(CompareListItems.CompareNames);
                }

                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_populate_controls", opLoadFrmMasterindex))
                {
                    _lstFileNamesWithItems.Insert(0, new ListItem(string.Empty, LanguageManager.GetString("String_All")));

                    int intOldSelectedIndex = cboFile.SelectedIndex;

                    cboFile.BeginUpdate();
                    cboFile.PopulateWithListItems(_lstFileNamesWithItems);
                    try
                    {
                        cboFile.SelectedIndex = Math.Max(intOldSelectedIndex, 0);
                    }
                    // For some reason, some unit tests will fire this exception even when _lstFileNamesWithItems is explicitly checked for having enough items
                    catch (ArgumentOutOfRangeException)
                    {
                        cboFile.SelectedIndex = -1;
                    }
                    cboFile.EndUpdate();

                    lstItems.BeginUpdate();
                    lstItems.PopulateWithListItems(_lstItems);
                    lstItems.SelectedIndex = -1;
                    lstItems.EndUpdate();

                    _blnSkipRefresh = false;
                }
            }
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        private void RefreshList(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            using (new CursorWait(this))
            {
                List<ListItem> lstFilteredItems;
                if (txtSearch.TextLength == 0 && string.IsNullOrEmpty(cboFile.SelectedValue?.ToString()))
                {
                    lstFilteredItems = _lstItems;
                }
                else
                {
                    lstFilteredItems = new List<ListItem>(_lstItems.Count);
                    string strFileFilter = cboFile.SelectedValue?.ToString() ?? string.Empty;
                    string strSearchFilter = txtSearch.Text;
                    foreach (ListItem objItem in _lstItems)
                    {
                        MasterIndexEntry objItemEntry = (MasterIndexEntry)objItem.Value;
                        if (!string.IsNullOrEmpty(strFileFilter) && !objItemEntry.FileNames.Contains(strFileFilter))
                            continue;
                        if (!string.IsNullOrEmpty(strSearchFilter))
                        {
                            string strDisplayNameNoFile = objItemEntry.DisplayName;
                            if (strDisplayNameNoFile.EndsWith(".xml]", StringComparison.OrdinalIgnoreCase))
                                strDisplayNameNoFile = strDisplayNameNoFile.Substring(0, strDisplayNameNoFile.LastIndexOf('[')).Trim();
                            if (strDisplayNameNoFile.IndexOf(strSearchFilter, StringComparison.OrdinalIgnoreCase) == -1)
                                continue;
                        }

                        lstFilteredItems.Add(objItem);
                    }
                }

                object objOldSelectedValue = lstItems.SelectedValue;
                lstItems.BeginUpdate();
                _blnSkipRefresh = true;
                lstItems.PopulateWithListItems(lstFilteredItems);
                _blnSkipRefresh = false;
                if (objOldSelectedValue != null)
                {
                    MasterIndexEntry objOldSelectedEntry = (MasterIndexEntry)objOldSelectedValue;
                    lstItems.SelectedIndex = lstFilteredItems.FindIndex(x => ((MasterIndexEntry)x.Value).Equals(objOldSelectedEntry));
                }
                else
                    lstItems.SelectedIndex = -1;

                lstItems.EndUpdate();
            }
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            using (new CursorWait(this))
            {
                if (lstItems.SelectedValue is MasterIndexEntry objEntry)
                {
                    lblSourceLabel.Visible = true;
                    lblSource.Visible = true;
                    lblSourceClickReminder.Visible = true;
                    lblSource.Text = objEntry.DisplaySource.ToString();
                    lblSource.ToolTipText = objEntry.DisplaySource.LanguageBookTooltip;
                    if (!_dicCachedNotes.TryGetValue(objEntry, out string strNotes))
                    {
                        strNotes = CommonFunctions.GetTextFromPdf(objEntry.Source.ToString(), objEntry.EnglishNameOnPage);

                        if (string.IsNullOrEmpty(strNotes)
                            && !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                            && (objEntry.TranslatedNameOnPage != objEntry.EnglishNameOnPage
                                || objEntry.Source.Page != objEntry.DisplaySource.Page))
                        {
                            // don't check again it is not translated
                            strNotes = CommonFunctions.GetTextFromPdf(objEntry.DisplaySource.ToString(), objEntry.TranslatedNameOnPage);
                        }

                        _dicCachedNotes.TryAdd(objEntry, strNotes);
                    }

                    txtNotes.Text = strNotes;
                    txtNotes.Visible = true;
                }
                else
                {
                    lblSourceLabel.Visible = false;
                    lblSource.Visible = false;
                    lblSourceClickReminder.Visible = false;
                    txtNotes.Visible = false;
                }
            }
        }

        private readonly struct MasterIndexEntry
        {
            public MasterIndexEntry(string strDisplayName, string strFileName, SourceString objSource, SourceString objDisplaySource, string strEnglishNameOnPage, string strTranslatedNameOnPage)
            {
                DisplayName = strDisplayName;
                FileNames = new HashSet<string>
                {
                    strFileName
                };
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
        }

        private void cmdEditCharacterSetting_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                using (frmCharacterSettings frmOptions = new frmCharacterSettings(cboCharacterSetting.SelectedValue as CharacterSettings))
                    frmOptions.ShowDialog(this);
                // Do not repopulate the character settings list because that will happen from frmCharacterSettings where appropriate
            }
        }

        public void ForceRepopulateCharacterSettings()
        {
            using (new CursorWait(this))
            {
                SuspendLayout();
                PopulateCharacterSettings();
                ResumeLayout();
            }
        }
    }
}
