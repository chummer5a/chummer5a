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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class PetControl : UserControl
    {
        private readonly Contact _objContact;
        private bool _blnLoading = true;

        // Events.
        public event EventHandler<TextEventArgs> ContactDetailChanged;

        public event EventHandler DeleteContact;

        #region Control Events

        public PetControl(Contact objContact)
        {
            _objContact = objContact;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private async void PetControl_Load(object sender, EventArgs e)
        {
            await LoadContactList();

            DoDataBindings();

            _blnLoading = false;
        }

        public void UnbindPetControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Name"));
        }

        private void UpdateMetatype(object sender, EventArgs e)
        {
            if (_blnLoading || _objContact.DisplayMetatype == cboMetatype.Text)
                return;
            _objContact.DisplayMetatype = cboMetatype.Text;
            if (_objContact.DisplayMetatype != cboMetatype.Text)
            {
                _blnLoading = true;
                cboMetatype.Text = _objContact.DisplayMetatype;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Metatype"));
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact?.Invoke(this, e);
        }

        private void cmdLink_Click(object sender, EventArgs e)
        {
            // Determine which options should be shown based on the FileName value.
            if (!string.IsNullOrEmpty(_objContact.FileName))
            {
                tsAttachCharacter.Visible = false;
                tsContactOpen.Visible = true;
                tsRemoveCharacter.Visible = true;
            }
            else
            {
                tsAttachCharacter.Visible = true;
                tsContactOpen.Visible = false;
                tsRemoveCharacter.Visible = false;
            }
            cmsContact.Show(cmdLink, cmdLink.Left - 700, cmdLink.Top);
        }

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = await Program.OpenCharacters.ContainsAsync(_objContact.LinkedCharacter)
                    ? _objContact.LinkedCharacter
                    : null;
                using (new CursorWait(this))
                {
                    if (objOpenCharacter == null)
                        objOpenCharacter = await Program.LoadCharacterAsync(_objContact.LinkedCharacter.FileName);
                    if (!Program.SwitchToOpenCharacter(objOpenCharacter))
                        await Program.OpenCharacter(objOpenCharacter);
                }
            }
            else
            {
                bool blnUseRelative = false;

                // Make sure the file still exists before attempting to load it.
                if (!File.Exists(_objContact.FileName))
                {
                    bool blnError = false;
                    // If the file doesn't exist, use the relative path if one is available.
                    if (string.IsNullOrEmpty(_objContact.RelativeFileName))
                        blnError = true;
                    else if (!File.Exists(Path.GetFullPath(_objContact.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;

                    if (blnError)
                    {
                        Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_FileNotFound"), _objContact.FileName), await LanguageManager.GetStringAsync("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objContact.RelativeFileName) : _objContact.FileName;
                System.Diagnostics.Process.Start(strFile);
            }
        }

        private async void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' + await LanguageManager.GetStringAsync("DialogFilter_All")
            })
            {
                if (!string.IsNullOrEmpty(_objContact.FileName) && File.Exists(_objContact.FileName))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(_objContact.FileName);
                    openFileDialog.FileName = Path.GetFileName(_objContact.FileName);
                }

                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                using (new CursorWait(this))
                {
                    _objContact.FileName = openFileDialog.FileName;
                    cmdLink.ToolTipText = await LanguageManager.GetStringAsync("Tip_Contact_OpenFile");

                    // Set the relative path.
                    Uri uriApplication = new Uri(Utils.GetStartupPath);
                    Uri uriFile = new Uri(_objContact.FileName);
                    Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                    _objContact.RelativeFileName = "../" + uriRelative;

                    ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
                }
            }
        }

        private async void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_RemoveCharacterAssociation"), await LanguageManager.GetStringAsync("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                cmdLink.ToolTipText = await LanguageManager.GetStringAsync("Tip_Contact_LinkFile");
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private async void cmdNotes_Click(object sender, EventArgs e)
        {
            using (EditNotes frmContactNotes = new EditNotes(_objContact.Notes, _objContact.NotesColor))
            {
                await frmContactNotes.ShowDialogSafeAsync(this);
                if (frmContactNotes.DialogResult != DialogResult.OK)
                    return;
                _objContact.Notes = frmContactNotes.Notes;
            }

            string strTooltip = await LanguageManager.GetStringAsync("Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
            cmdNotes.ToolTipText = strTooltip.WordWrap();
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
        }

        #endregion Control Events

        #region Methods

        private async ValueTask LoadContactList()
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            {
                lstMetatypes.Add(ListItem.Blank);
                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                foreach (XPathNavigator xmlMetatypeNode in await (await _objContact.CharacterObject.LoadDataXPathAsync("critters.xml"))
                             .SelectAndCacheExpressionAsync(
                                 "/chummer/metatypes/metatype"))
                {
                    string strName = (await xmlMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                    string strMetatypeDisplay = (await xmlMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                ?? strName;
                    lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                    XPathNodeIterator xmlMetavariantsList
                        = await xmlMetatypeNode.SelectAndCacheExpressionAsync("metavariants/metavariant");
                    if (xmlMetavariantsList.Count > 0)
                    {
                        string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                        foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                        {
                            string strMetavariantName
                                = (await objXmlMetavariantNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                  ?? string.Empty;
                            if (lstMetatypes.All(
                                    x => strMetavariantName.Equals(x.Value.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase)))
                                lstMetatypes.Add(new ListItem(strMetavariantName,
                                                              string.Format(
                                                                  GlobalSettings.CultureInfo, strMetavariantFormat,
                                                                  (await objXmlMetavariantNode
                                                                      .SelectSingleNodeAndCacheExpressionAsync("translate"))
                                                                      ?.Value ?? strMetavariantName)));
                        }
                    }
                }

                lstMetatypes.Sort(CompareListItems.CompareNames);

                cboMetatype.BeginUpdate();
                cboMetatype.PopulateWithListItems(lstMetatypes);
                cboMetatype.EndUpdate();
            }
        }

        private void DoDataBindings()
        {
            cboMetatype.SelectedValue = _objContact.Metatype;
            if (cboMetatype.SelectedIndex < 0)
                cboMetatype.Text = _objContact.DisplayMetatype;
            txtContactName.DoDataBinding("Text", _objContact, nameof(_objContact.Name));
            this.DoOneWayDataBinding("BackColor", _objContact, nameof(_objContact.PreferredColor));

            // Properties controllable by the character themselves
            txtContactName.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
            cboMetatype.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;

        #endregion Properties
    }
}
