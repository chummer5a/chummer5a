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
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
    public partial class PetControl : UserControl
    {
        private readonly Contact _objContact;

        // Events.
        public Action<object> DeleteContact { get; set; }
        public Action<object> FileNameChanged { get; set; }

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
            lblMetatype.DataBindings.Add("Text", _objContact, nameof(_objContact.DisplayMetatype), false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtContactName.DataBindings.Add("Text", _objContact, nameof(_objContact.Name), false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtContactName.DataBindings.Add("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter), false,
                DataSourceUpdateMode.OnPropertyChanged);
            this.DataBindings.Add("BackColor", _objContact, nameof(_objContact.Colour), false,
                DataSourceUpdateMode.OnPropertyChanged);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact(this);
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
                        MessageBox.Show(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language).Replace("{0}", _objContact.FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Filter = "Chummer Files (*.chum5)|*.chum5|All Files (*.*)|*.*"
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
                tipTooltip.SetToolTip(imgLink, LanguageManager.GetString("Tip_Contact_OpenFile", GlobalOptions.Language));

                // Set the relative path.
                Uri uriApplication = new Uri(@Application.StartupPath);
                Uri uriFile = new Uri(@_objContact.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objContact.RelativeFileName = "../" + uriRelative.ToString();

                FileNameChanged(this);
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
                tipTooltip.SetToolTip(imgLink, LanguageManager.GetString("Tip_Contact_LinkFile", GlobalOptions.Language));
                lblMetatype.Text = string.Empty;
                FileNameChanged(this);
            }
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmContactNotes = new frmNotes
            {
                Notes = _objContact.Notes
            };
            frmContactNotes.ShowDialog(this);

            if (frmContactNotes.DialogResult == DialogResult.OK)
                _objContact.Notes = frmContactNotes.Notes;

            string strTooltip = LanguageManager.GetString("Tip_Contact_EditNotes", GlobalOptions.Language);
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += "\n\n" + _objContact.Notes;
            tipTooltip.SetToolTip(imgNotes, strTooltip.WordWrap(100));
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
                _objContact.Name = value;
            }
        }
        #endregion
    }
}
