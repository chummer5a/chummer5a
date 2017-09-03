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
﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Chummer
{
    public partial class PetControl : UserControl
    {
        private Contact _objContact;

        // Events.
        public Action<object> DeleteContact;
        public Action<object> FileNameChanged;

        #region Control Events
        public PetControl()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void PetControl_Load(object sender, EventArgs e)
        {
            Width = cmdDelete.Left + cmdDelete.Width;
            lblMetatype.Text = string.Empty;

            if (!string.IsNullOrEmpty(_objContact.FileName))
            {
                // Load the character to get their Metatype.
                Character objPet = new Character();
                objPet.FileName = _objContact.FileName;
                objPet.Load();
                lblMetatype.Text = objPet.Metatype;
                if (!string.IsNullOrEmpty(objPet.Metavariant))
                    lblMetatype.Text += " (" + objPet.Metavariant + ")";
                objPet = null;
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact(this);
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            _objContact.Name = txtContactName.Text;
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
            bool blnError = false;
            bool blnUseRelative = false;

            // Make sure the file still exists before attempting to load it.
            if (!File.Exists(_objContact.FileName))
            {
                // If the file doesn't exist, use the relative path if one is available.
                if (string.IsNullOrEmpty(_objContact.RelativeFileName))
                    blnError = true;
                else
                {
                    MessageBox.Show(Path.GetFullPath(_objContact.RelativeFileName));
                    if (!File.Exists(Path.GetFullPath(_objContact.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;
                }

                if (blnError)
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("Message_FileNotFound").Replace("{0}", _objContact.FileName), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if (Path.GetExtension(_objContact.FileName) == "chum5")
            {
                if (!blnUseRelative)
                    GlobalOptions.Instance.MainForm.LoadCharacter(_objContact.FileName, false);
                else
                {
                    string strFile = Path.GetFullPath(_objContact.RelativeFileName);
                    GlobalOptions.Instance.MainForm.LoadCharacter(strFile, false);
                }
            }
            else
            {
                if (!blnUseRelative)
                    System.Diagnostics.Process.Start(_objContact.FileName);
                else
                {
                    string strFile = Path.GetFullPath(_objContact.RelativeFileName);
                    System.Diagnostics.Process.Start(strFile);
                }
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Chummer Files (*.chum5)|*.chum5|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objContact.FileName = openFileDialog.FileName;
                tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_OpenFile"));

                // Load the character to get their Metatype.
                Character objPet = new Character();
                objPet.FileName = _objContact.FileName;
                objPet.Load();
                lblMetatype.Text = objPet.Metatype;
                if (!string.IsNullOrEmpty(objPet.Metavariant))
                    lblMetatype.Text += " (" + objPet.Metavariant + ")";
                objPet = null;

                // Set the relative path.
                Uri uriApplication = new Uri(@Application.StartupPath);
                Uri uriFile = new Uri(@_objContact.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objContact.RelativeFileName = "../" + uriRelative.ToString();

                FileNameChanged(this);
            }
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (MessageBox.Show(LanguageManager.Instance.GetString("Message_RemoveCharacterAssociation"), LanguageManager.Instance.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_LinkFile"));
                lblMetatype.Text = string.Empty;
                FileNameChanged(this);
            }
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmContactNotes = new frmNotes();
            frmContactNotes.Notes = _objContact.Notes;
            frmContactNotes.ShowDialog(this);

            if (frmContactNotes.DialogResult == DialogResult.OK)
                _objContact.Notes = frmContactNotes.Notes;

            string strTooltip = string.Empty;
            strTooltip = LanguageManager.Instance.GetString("Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += "\n\n" + _objContact.Notes;
            tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
        }

        private void cmsContact_Opening(object sender, CancelEventArgs e)
        {
            foreach (ToolStripItem objItem in ((ContextMenuStrip)sender).Items)
            {
                if (objItem.Tag != null)
                {
                    objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
                }
            }
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            txtContactName.Left = lblName.Left + lblName.Width + 6;
            lblMetatypeLabel.Left = txtContactName.Left + txtContactName.Width + 16;
            lblMetatype.Left = lblMetatypeLabel.Left + lblMetatypeLabel.Width + 6;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject
        {
            get
            {
                return _objContact;
            }
            set
            {
                _objContact = value;
            }
        }

        /// <summary>
        /// Contact name.
        /// </summary>
        public string ContactName
        {
            get
            {
                return _objContact.Name;
            }
            set
            {
                txtContactName.Text = value;
                _objContact.Name = value;
            }
        }
        #endregion
    }
}