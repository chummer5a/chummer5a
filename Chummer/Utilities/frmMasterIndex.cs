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
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmMasterIndex : Form
    {
        private bool _blnSkipRefresh = true;
        private readonly List<ListItem> _lstFileNamesWithItems;
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
            this.TranslateWinForm();

            _lstFileNamesWithItems = new List<ListItem>(_lstFileNames.Count);
        }

        private void frmMasterIndex_Load(object sender, EventArgs e)
        {
            using (var op_load_frm_masterindex = Timekeeper.StartSyncron("op_load_frm_masterindex", null, CustomActivity.OperationType.RequestOperation, null))
            {
                using (_ = Timekeeper.StartSyncron("load_frm_masterindex_populate_entries", op_load_frm_masterindex))
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    foreach (string strFileName in _lstFileNames)
                    {
                        string strDisplayNameSuffix = string.Concat(strSpace, "[", strFileName, "]");
                        XPathNavigator xmlBaseNode = XmlManager.Load(strFileName).GetFastNavigator().SelectSingleNode("/chummer");
                        if (xmlBaseNode != null)
                        {
                            bool blnLoopFileNameHasItems = false;
                            foreach (XPathNavigator xmlItemNode in xmlBaseNode.Select("//[source and page]"))
                            {
                                blnLoopFileNameHasItems = true;
                                string strDisplayName = xmlItemNode.SelectSingleNode("translate")?.Value
                                                        ?? xmlItemNode.SelectSingleNode("name")?.Value
                                                        ?? xmlItemNode.SelectSingleNode("id")?.Value
                                                        ?? LanguageManager.GetString("String_Unknown");
                                string strSource = xmlItemNode.SelectSingleNode("altsource")?.Value
                                                   ?? xmlItemNode.SelectSingleNode("source")?.Value;
                                string strPage = xmlItemNode.SelectSingleNode("altpage")?.Value
                                                 ?? xmlItemNode.SelectSingleNode("page")?.Value;
                                string strNameOnPage = xmlItemNode.SelectSingleNode("altnameonpage")?.Value
                                                       ?? xmlItemNode.SelectSingleNode("nameonpage")?.Value
                                                       ?? strDisplayName;
                                MasterIndexEntry objEntry = new MasterIndexEntry(
                                    strDisplayName,
                                    strFileName,
                                    new SourceString(strSource, strPage, GlobalOptions.Language),
                                    CommonFunctions.GetTextFromPDF(strSource + ' ' + strPage, strNameOnPage));
                                _lstItems.Add(new ListItem(objEntry, strDisplayName + strDisplayNameSuffix));
                            }

                            if (blnLoopFileNameHasItems)
                                _lstFileNamesWithItems.Add(new ListItem(strFileName, strFileName));
                        }
                    }
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
                    lstItems.EndUpdate();

                    _blnSkipRefresh = false;
                }
            }
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }

        private void RefreshList(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
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
                    if (!string.IsNullOrEmpty(strFileFilter) && !objItemEntry.FileName.Equals(strFileFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!string.IsNullOrEmpty(strSearchFilter) && objItemEntry.DisplayName.IndexOf(strSearchFilter, StringComparison.OrdinalIgnoreCase) == -1)
                        continue;
                    lstFilteredItems.Add(objItem);
                }
            }

            string strOldSelectedValue = ((MasterIndexEntry)lstItems.SelectedValue).Id;
            lstItems.BeginUpdate();
            _blnSkipRefresh = true;
            lstItems.DataSource = lstFilteredItems;
            _blnSkipRefresh = false;
            ListItem objSelectedItem = !string.IsNullOrEmpty(strOldSelectedValue)
                ? _lstItems.FirstOrDefault(x => ((MasterIndexEntry)x.Value).Id.Equals(strOldSelectedValue))
                : ListItem.Blank;
            if (!objSelectedItem.Equals(ListItem.Blank))
                lstItems.SelectedItem = objSelectedItem;
            else
                lstItems_SelectedIndexChanged(sender, e);
            lstItems.EndUpdate();
            Cursor = objOldCursor;
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            SuspendLayout();
            if (lstItems.SelectedValue is MasterIndexEntry objEntry)
            {
                lblSourceLabel.Visible = true;
                lblSource.Visible = true;
                lblSourceClickReminder.Visible = true;
                lblSource.Text = objEntry.Source.ToString();
                lblSource.ToolTipText = objEntry.Source.LanguageBookTooltip;
                rtbNotes.Text = objEntry.Notes;
            }
            else
            {
                lblSourceLabel.Visible = false;
                lblSource.Visible = false;
                lblSourceClickReminder.Visible = false;
                rtbNotes.Text = string.Empty;
            }
            ResumeLayout();
            Cursor = objOldCursor;
        }

        private void rtbNotes_TextChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            rtbNotes.Visible = rtbNotes.TextLength > 0;
        }

        private readonly struct MasterIndexEntry
        {
            public MasterIndexEntry(string strDisplayName, string strFileName, SourceString objSource, string strNotes)
            {
                Id = Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo);
                DisplayName = strDisplayName;
                FileName = strFileName;
                Source = objSource;
                Notes = strNotes;
            }

            internal string Id { get; }
            internal string DisplayName { get; }
            internal string FileName { get; }
            internal SourceString Source { get; }
            internal string Notes { get; }
        }
    }
}
