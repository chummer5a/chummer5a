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
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            foreach (ToolStripItem objItem in cmsContact.Items)
            {
                LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
            }
            MoveControls();
        }

        private void PetControl_Load(object sender, EventArgs e)
        {
            Width = cmdDelete.Left + cmdDelete.Width;

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

        private void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x == _objContact.LinkedCharacter);
                Cursor = Cursors.WaitCursor;
                if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                {
                    objOpenCharacter = Program.MainForm.LoadCharacter(_objContact.LinkedCharacter.FileName);
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
                        MessageBox.Show(string.Format(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language), _objContact.FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };
            if (!string.IsNullOrEmpty(_objContact.FileName) && File.Exists(_objContact.FileName))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(_objContact.FileName);
                openFileDialog.FileName = Path.GetFileName(_objContact.FileName);
            }
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                _objContact.FileName = openFileDialog.FileName;
                imgLink.SetToolTip(LanguageManager.GetString("Tip_Contact_OpenFile", GlobalOptions.Language));

                // Set the relative path.
                Uri uriApplication = new Uri(Utils.GetStartupPath);
                Uri uriFile = new Uri(_objContact.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objContact.RelativeFileName = "../" + uriRelative.ToString();

                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
                Cursor = Cursors.Default;
            }
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (MessageBox.Show(LanguageManager.GetString("Message_RemoveCharacterAssociation", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_RemoveCharacterAssociation", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                imgLink.SetToolTip(LanguageManager.GetString("Tip_Contact_LinkFile", GlobalOptions.Language));
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmContactNotes = new frmNotes
            {
                Notes = _objContact.Notes
            };
            frmContactNotes.ShowDialog(this);

            if (frmContactNotes.DialogResult == DialogResult.OK && _objContact.Notes != frmContactNotes.Notes)
            {
                _objContact.Notes = frmContactNotes.Notes;

                string strTooltip = LanguageManager.GetString("Tip_Contact_EditNotes", GlobalOptions.Language);
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap(100));
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
            }
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            txtContactName.Left = lblName.Left + lblName.Width + 6;
            lblMetatypeLabel.Left = txtContactName.Left + txtContactName.Width + 16;
            cboMetatype.Left = lblMetatypeLabel.Left + lblMetatypeLabel.Width + 6;
            cboMetatype.Width = imgLink.Left - 6 - cboMetatype.Left;
        }

        private void LoadContactList()
        {
            List<ListItem> lstMetatypes = new List<ListItem>
            {
                ListItem.Blank
            };
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            using (XmlNodeList xmlMetatypesList = XmlManager.Load("critters.xml").SelectNodes("/chummer/metatypes/metatype"))
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
                                    lstMetatypes.Add(new ListItem(strMetavariantName, strMetatypeDisplay + strSpaceCharacter + '(' + (objXmlMetavariantNode["translate"]?.InnerText ?? strMetavariantName) + ')'));
                            }
                    }

            lstMetatypes.Sort(CompareListItems.CompareNames);

            cboMetatype.BeginUpdate();
            cboMetatype.ValueMember = "Value";
            cboMetatype.DisplayMember = "Name";
            cboMetatype.DataSource = lstMetatypes;
            cboMetatype.EndUpdate();
        }

        private void DoDataBindings()
        {
            cboMetatype.DataBindings.Add("Text", _objContact, nameof(_objContact.DisplayMetatype), false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtContactName.DataBindings.Add("Text", _objContact, nameof(_objContact.Name), false,
                DataSourceUpdateMode.OnPropertyChanged);
            DataBindings.Add("BackColor", _objContact, nameof(_objContact.PreferredColor), false,
                DataSourceUpdateMode.OnPropertyChanged);

            // Properties controllable by the character themselves
            txtContactName.DataBindings.Add("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter), false,
                DataSourceUpdateMode.OnPropertyChanged);
            cboMetatype.DataBindings.Add("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter), false,
                DataSourceUpdateMode.OnPropertyChanged);
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
