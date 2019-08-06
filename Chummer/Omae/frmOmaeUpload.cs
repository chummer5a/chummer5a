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
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using Chummer.OmaeService;

namespace Chummer
{
    public sealed partial class frmOmaeUpload : Form
    {
        private readonly Character _objCharacter = new Character();
        private readonly List<ListItem> _lstCharacterTypes = new List<ListItem>();

        // Error message constants.
        private readonly string NO_CONNECTION_MESSAGE = string.Empty;
        private readonly string NO_CONNECTION_TITLE = string.Empty;

        private readonly string _strUserName;
        private string _strCharacterName = string.Empty;
        private string _strMetatype = string.Empty;
        private string _strMetavariant = string.Empty;
        private string _strQualities = string.Empty;
        private readonly int _intCharacterID = 0;
        private readonly int _intCharacterType = 0;
        private int _intCreated = 0;

        #region Control Events
        public frmOmaeUpload(string strUserName, List<ListItem> lstCharacterTypes, int intCharacterType, int intCharacterID = 0, string strDescription = "")
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Instance.Language, this);
            _strUserName = strUserName;

            // Remove the items that cannot actually be uploaded through this window.
            foreach (ListItem objItem in lstCharacterTypes)
            {
                if (objItem.Value.ToString() == "4")
                {
                    lstCharacterTypes.Remove(objItem);
                    break;
                }
            }
            foreach (ListItem objItem in lstCharacterTypes)
            {
                if (objItem.Value.ToString() == "data")
                {
                    lstCharacterTypes.Remove(objItem);
                    break;
                }
            }
            foreach (ListItem objItem in lstCharacterTypes)
            {
                if (objItem.Value.ToString() == "sheets")
                {
                    lstCharacterTypes.Remove(objItem);
                    break;
                }
            }

            _lstCharacterTypes = lstCharacterTypes;
            _intCharacterID = intCharacterID;
            txtDescription.Text = strDescription;
            _intCharacterType = intCharacterType;

            NO_CONNECTION_MESSAGE = LanguageManager.GetString("Message_Omae_CannotConnection", GlobalOptions.Instance.Language);
            NO_CONNECTION_TITLE = LanguageManager.GetString("MessageTitle_Omae_CannotConnection", GlobalOptions.Instance.Language);

            MoveControls();
        }

        private void frmOmaeUpload_Load(object sender, EventArgs e)
        {
            cboCharacterTypes.DataSource = null;
            cboCharacterTypes.DataSource = _lstCharacterTypes;
            cboCharacterTypes.ValueMember = "Value";
            cboCharacterTypes.DisplayMember = "Name";

            string strName = string.Empty;
            foreach (ListItem objItem in _lstCharacterTypes)
            {
                if (objItem.Value.ToString() == _intCharacterType.ToString())
                {
                    strName = objItem.Value.ToString();
                    break;
                }
            }
            cboCharacterTypes.SelectedValue = strName;
        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Instance.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Instance.Language),
                Multiselect = false
            };

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Clear the file path field.
            txtFilePath.Text = string.Empty;

            // Make sure a .chum5 file was selected.
            if (!openFileDialog.FileName.EndsWith(".chum5"))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_CannotUploadFile", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_CannotUploadFile", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;
            // Attempt to load the character and make sure it's a valid character file.
            _objCharacter.FileName = openFileDialog.FileName;
            try
            {
                _objCharacter.Load();
                Cursor = Cursors.Default;
            }
            catch
            {
                Cursor = Cursors.Default;
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_CannotUploadFile", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_CannotUploadFile", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the character is named.
            _strCharacterName = _objCharacter.CharacterName;
            if (string.IsNullOrWhiteSpace(_strCharacterName) || _strCharacterName == LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Instance.Language))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_UnnamedCharacter", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_UnnamedCharacter", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _strMetatype = _objCharacter.Metatype;
            _strMetavariant = _objCharacter.Metavariant;
            _intCreated = Convert.ToInt32(_objCharacter.Created);

            foreach (Quality objQuality in _objCharacter.Qualities)
            {
                _strQualities += objQuality.Name + "|";
            }
            if (!string.IsNullOrEmpty(_strQualities))
                _strQualities = _strQualities.Substring(0, _strQualities.Length - 1);
            // Make sure the Qualities list doesn't exceed 4,000 characters.
            if (_strQualities.Length > 4000)
                _strQualities = _strQualities.Substring(0, 4000);
            // Make sure the character name doesn't exceed 100 characters.
            if (_strCharacterName.Length > 100)
                _strCharacterName = _strCharacterName.Substring(0, 100);

            // If everything checks out, populate the file path filed and character name.
            txtFilePath.Text = openFileDialog.FileName;
            cboCharacterName.Items.Clear();
            if (!string.IsNullOrEmpty(_objCharacter.Name))
                cboCharacterName.Items.Add(_objCharacter.Name);
            if (!string.IsNullOrEmpty(_objCharacter.Alias))
                cboCharacterName.Items.Add(_objCharacter.Alias);
            cboCharacterName.SelectedIndex = 0;
        }

        private void cmdUpload_Click(object sender, EventArgs e)
        {
            bool blnSuccess = false;

            // Make sure a file has been selected.
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_SelectFile", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_SelectFile", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure there is at least some sort of description.
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_CharacterDescription", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_CharacterDescription", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Read the contents of the file into a byte array, the compress it.
            byte[] bytFile = OmaeHelper.Compress(File.ReadAllBytes(txtFilePath.Text));

            // Make sure the file doesn't exceed 500K in size (512,000 bytes).
            if (bytFile.Length > 512000)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_FileTooLarge", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_FileTooLarge", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            omaeSoapClient objService = OmaeHelper.GetOmaeService();
            try
            {
                cmdUpload.Enabled = false;
                txtDescription.Enabled = false;
                string strCharacterName = cboCharacterName.Text;
                if (strCharacterName.Length > 100)
                    strCharacterName = strCharacterName.Substring(0, 100);
                if (objService.UploadCharacter153(_strUserName, _intCharacterID, strCharacterName, txtDescription.Text, _strMetatype, _strMetavariant, _strQualities, Convert.ToInt32(cboCharacterTypes.SelectedValue), _intCreated, bytFile))
                {
                    blnSuccess = true;
                    MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_UploadComplete", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_UploadComplete", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_UploadFailed", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_UploadFailed", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            objService.Close();
            cmdUpload.Enabled = true;
            txtDescription.Enabled = true;

            if (blnSuccess)
                DialogResult = DialogResult.OK;
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            txtFilePath.Left = Math.Max(cboCharacterName.Left, lblFilePathLabel.Left + lblFilePathLabel.Width + 6);
            txtFilePath.Width = cmdBrowse.Left - txtFilePath.Left - 6;
        }
        #endregion
    }
}
