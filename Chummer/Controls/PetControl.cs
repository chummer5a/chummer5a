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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class PetControl : UserControl
    {
        private readonly Contact _objContact;
        private bool _blnLoading = true;

        // Events.
        public event TextEventHandler ContactDetailChanged;
        public event EventHandler DeleteContact;

        #region Control Events
        public PetControl(Contact objContact)
        {
            _objContact = objContact;
            InitializeComponent();
            this.TranslateWinForm();
            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void PetControl_Load(object sender, EventArgs e)
        {
            LoadContactList();

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

        private void cboMetatype_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Metatype"));
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact?.Invoke(this, e);
        }

        private void imgLink_Click(object sender, EventArgs e)
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
            cmsContact.Show(imgLink, imgLink.Left - 700, imgLink.Top);
        }

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x == _objContact.LinkedCharacter);
                Cursor = Cursors.WaitCursor;
                if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                {
                    objOpenCharacter = await Program.MainForm.LoadCharacter(_objContact.LinkedCharacter.FileName).ConfigureAwait(true);
                    Program.MainForm.OpenCharacter(objOpenCharacter);
                }
                Cursor = Cursors.Default;
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
                        Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), _objContact.FileName), LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objContact.RelativeFileName) : _objContact.FileName;
                System.Diagnostics.Process.Start(strFile);
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' + LanguageManager.GetString("DialogFilter_All")
            })
            {
                if (!string.IsNullOrEmpty(_objContact.FileName) && File.Exists(_objContact.FileName))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(_objContact.FileName);
                    openFileDialog.FileName = Path.GetFileName(_objContact.FileName);
                }

                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                Cursor = Cursors.WaitCursor;
                _objContact.FileName = openFileDialog.FileName;
                imgLink.SetToolTip(LanguageManager.GetString("Tip_Contact_OpenFile"));

                // Set the relative path.
                Uri uriApplication = new Uri(Utils.GetStartupPath);
                Uri uriFile = new Uri(_objContact.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objContact.RelativeFileName = "../" + uriRelative;

                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
                Cursor = Cursors.Default;
            }
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_RemoveCharacterAssociation"), LanguageManager.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                imgLink.SetToolTip(LanguageManager.GetString("Tip_Contact_LinkFile"));
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            string strOldValue = _objContact.Notes;
            using (frmNotes frmContactNotes = new frmNotes { Notes = strOldValue })
            {
                frmContactNotes.ShowDialog(this);
                if (frmContactNotes.DialogResult != DialogResult.OK)
                    return;
                frmContactNotes.ShowDialog(this);

                _objContact.Notes = frmContactNotes.Notes;
                if (strOldValue == _objContact.Notes)
                    return;
            }

            string strTooltip = LanguageManager.GetString("Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
            imgNotes.SetToolTip(strTooltip.WordWrap());
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
        }
        #endregion

        #region Methods
        private void LoadContactList()
        {
            List<ListItem> lstMetatypes = new List<ListItem> (30)
            {
                ListItem.Blank
            };
            string strSpace = LanguageManager.GetString("String_Space");
            using (XmlNodeList xmlMetatypesList = _objContact.CharacterObject.LoadData("critters.xml").SelectNodes("/chummer/metatypes/metatype"))
                if (xmlMetatypesList != null)
                    foreach (XmlNode xmlMetatypeNode in xmlMetatypesList)
                    {
                        string strName = xmlMetatypeNode["name"]?.InnerText;
                        string strMetatypeDisplay = xmlMetatypeNode["translate"]?.InnerText ?? strName;
                        lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                        XmlNodeList xmlMetavariantsList = xmlMetatypeNode.SelectNodes("metavariants/metavariant");
                        if (xmlMetavariantsList != null)
                            foreach (XmlNode objXmlMetavariantNode in xmlMetavariantsList)
                            {
                                string strMetavariantName = objXmlMetavariantNode["name"]?.InnerText;
                                if (lstMetatypes.All(x => x.Value.ToString() != strMetavariantName))
                                    lstMetatypes.Add(new ListItem(strMetavariantName, strMetatypeDisplay + strSpace + '(' + (objXmlMetavariantNode["translate"]?.InnerText ?? strMetavariantName) + ')'));
                            }
                    }

            lstMetatypes.Sort(CompareListItems.CompareNames);

            cboMetatype.BeginUpdate();
            cboMetatype.ValueMember = nameof(ListItem.Value);
            cboMetatype.DisplayMember = nameof(ListItem.Name);
            cboMetatype.DataSource = lstMetatypes;
            cboMetatype.EndUpdate();
        }

        private void DoDataBindings()
        {
            cboMetatype.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayMetatype));
            txtContactName.DoDatabinding("Text", _objContact, nameof(_objContact.Name));
            this.DoOneWayDataBinding("BackColor", _objContact, nameof(_objContact.PreferredColor));

            // Properties controllable by the character themselves
            txtContactName.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
            cboMetatype.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;

        #endregion
    }
}
