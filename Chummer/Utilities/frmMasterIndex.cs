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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmMasterIndex : Form
    {
        private bool _blnSkipRefresh = true;
        private readonly List<ListItem> _lstFileNamesWithItems;
        private readonly ConcurrentDictionary<MasterIndexEntry, string> _dicCachedNotes = new ConcurrentDictionary<MasterIndexEntry, string>();
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
        }

        private void frmMasterIndex_Load(object sender, EventArgs e)
        {
            using (var op_load_frm_masterindex = Timekeeper.StartSyncron("op_load_frm_masterindex", null, CustomActivity.OperationType.RequestOperation, null))
            {
                HashSet<string> setValidCodes = new HashSet<string>();
                foreach (XPathNavigator xmlBookNode in XmlManager.LoadXPath("books.xml").Select("/chummer/books/book/code"))
                {
                    setValidCodes.Add(xmlBookNode.Value);
                }

                string strSourceFilter = setValidCodes.Count > 0
                    ? new StringBuilder("(")
                        .AppendJoin(" or ", setValidCodes.Select(x => "source = \'" + x + "\'"))
                        .Append(')').ToString()
                    : "source";

                ConcurrentBag<ListItem> lstItemsForLoading = new ConcurrentBag<ListItem>();
                ConcurrentBag<ListItem> lstFileNamesWithItemsForLoading = new ConcurrentBag<ListItem>();
                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_load_entries", op_load_frm_masterindex))
                {
                    Parallel.ForEach(_lstFileNames, strFileName =>
                    {
                        XPathNavigator xmlBaseNode = XmlManager.LoadXPath(strFileName).SelectSingleNode("/chummer");
                        if (xmlBaseNode != null)
                        {
                            bool blnLoopFileNameHasItems = false;
                            foreach (XPathNavigator xmlItemNode in xmlBaseNode.Select(".//*[page and " + strSourceFilter + ']'))
                            {
                                blnLoopFileNameHasItems = true;
                                string strName = xmlItemNode.SelectSingleNode("name")?.Value;
                                string strDisplayName = xmlItemNode.SelectSingleNode("translate")?.Value
                                                        ?? strName
                                                        ?? xmlItemNode.SelectSingleNode("id")?.Value
                                                        ?? LanguageManager.GetString("String_Unknown");
                                string strSource = xmlItemNode.SelectSingleNode("source")?.Value;
                                string strPage = xmlItemNode.SelectSingleNode("page")?.Value;
                                string strDisplayPage = xmlItemNode.SelectSingleNode("altpage")?.Value
                                                        ?? strPage;
                                string strEnglishNameOnPage = xmlItemNode.SelectSingleNode("nameonpage")?.Value
                                                              ?? strName;
                                string strTranslatedNameOnPage = xmlItemNode.SelectSingleNode("altnameonpage")?.Value
                                                                 ?? strDisplayName;
                                string strNotes = xmlItemNode.SelectSingleNode("altnotes")?.Value
                                                  ?? xmlItemNode.SelectSingleNode("notes")?.Value;
                                MasterIndexEntry objEntry = new MasterIndexEntry(
                                    strDisplayName,
                                    strFileName,
                                    new SourceString(strSource, strPage, GlobalOptions.DefaultLanguage, GlobalOptions.InvariantCultureInfo),
                                    new SourceString(strSource, strDisplayPage, GlobalOptions.Language, GlobalOptions.CultureInfo),
                                    strEnglishNameOnPage,
                                    strTranslatedNameOnPage);
                                lstItemsForLoading.Add(new ListItem(objEntry, strDisplayName));
                                if (!string.IsNullOrEmpty(strNotes))
                                    _dicCachedNotes.TryAdd(objEntry, strNotes);
                            }

                            if (blnLoopFileNameHasItems)
                                lstFileNamesWithItemsForLoading.Add(new ListItem(strFileName, strFileName));
                        }
                    });
                }

                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_populate_entries", op_load_frm_masterindex))
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    Dictionary<string, List<ListItem>> dicHelper = new Dictionary<string, List<ListItem>>(lstItemsForLoading.Count);
                    foreach (ListItem objItem in lstItemsForLoading)
                    {
                        MasterIndexEntry objEntry = (MasterIndexEntry)objItem.Value;
                        string strKey = objEntry.DisplayName.ToUpperInvariant();
                        if (dicHelper.TryGetValue(strKey, out List<ListItem> lstExistingItems))
                        {
                            ListItem objExistingItem = lstExistingItems.FirstOrDefault(x =>
                                objEntry.DisplaySource.Equals(((MasterIndexEntry) x.Value).DisplaySource));
                            if (objExistingItem.Value != null)
                            {
                                ((MasterIndexEntry)objExistingItem.Value).FileNames.UnionWith(objEntry.FileNames);
                            }
                            else
                            {
                                List<ListItem> lstItemsNeedingNameChanges = lstExistingItems.Where(x => !objEntry.FileNames.IsSubsetOf(((MasterIndexEntry) x.Value).FileNames)).ToList();
                                if (lstItemsNeedingNameChanges.Count == 0)
                                {
                                    _lstItems.Add(objItem); // Not using AddRange because of potential memory issues
                                    lstExistingItems.Add(objItem);
                                }
                                else
                                {
                                    ListItem objItemToAdd = new ListItem(objItem.Value,
                                        new StringBuilder(objItem.Name)
                                            .Append(strSpace).Append('[').AppendJoin(',' + strSpace, objEntry.FileNames).Append(']').ToString());
                                    _lstItems.Add(objItemToAdd); // Not using AddRange because of potential memory issues
                                    lstExistingItems.Add(objItemToAdd);

                                    foreach (ListItem objToRename in lstItemsNeedingNameChanges)
                                    {
                                        _lstItems.Remove(objToRename);
                                        lstExistingItems.Remove(objToRename);

                                        MasterIndexEntry objExistingEntry = (MasterIndexEntry)objToRename.Value;
                                        objItemToAdd = new ListItem(objToRename.Value,
                                            new StringBuilder(objExistingEntry.DisplayName)
                                                .Append(strSpace).Append('[').AppendJoin(',' + strSpace, objExistingEntry.FileNames).Append(']').ToString());
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

                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_sort_entries", op_load_frm_masterindex))
                {
                    _lstItems.Sort(CompareListItems.CompareNames);
                    _lstFileNamesWithItems.Sort(CompareListItems.CompareNames);
                }

                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_populate_controls", op_load_frm_masterindex))
                {
                    _lstFileNamesWithItems.Insert(0, new ListItem(string.Empty, LanguageManager.GetString("String_All")));

                    cboFile.BeginUpdate();
                    cboFile.ValueMember = nameof(ListItem.Value);
                    cboFile.DisplayMember = nameof(ListItem.Name);
                    cboFile.DataSource = _lstFileNamesWithItems;
                    cboFile.SelectedIndex = 0;
                    cboFile.EndUpdate();

                    lstItems.BeginUpdate();
                    lstItems.ValueMember = nameof(ListItem.Value);
                    lstItems.DisplayMember = nameof(ListItem.Name);
                    lstItems.DataSource = _lstItems;
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
                        MasterIndexEntry objItemEntry = (MasterIndexEntry) objItem.Value;
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
                lstItems.DataSource = lstFilteredItems;
                _blnSkipRefresh = false;
                if (objOldSelectedValue != null)
                {
                    MasterIndexEntry objOldSelectedEntry = (MasterIndexEntry) objOldSelectedValue;
                    lstItems.SelectedIndex = lstFilteredItems.FindIndex(x => ((MasterIndexEntry) x.Value).Equals(objOldSelectedEntry));
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
                            && GlobalOptions.Language != GlobalOptions.DefaultLanguage
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
    }
}
