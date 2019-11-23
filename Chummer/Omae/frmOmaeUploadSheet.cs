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
    public sealed partial class frmOmaeUploadSheet : Form
    {
        // Error message constants.
        private readonly string NO_CONNECTION_MESSAGE = string.Empty;
        private readonly string NO_CONNECTION_TITLE = string.Empty;

        private readonly string _strUserName;
        private readonly int _intSheetID = 0;
        private readonly List<string> _lstFiles = new List<string>();

        #region Control Events
        public frmOmaeUploadSheet(string strUserName, int intSheetID = 0, string strDescription = "", string strName = "")
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _strUserName = strUserName;
            _intSheetID = intSheetID;
            txtDescription.Text = strDescription;
            txtName.Text = strName;

            NO_CONNECTION_MESSAGE = LanguageManager.GetString("Message_Omae_CannotConnection", GlobalOptions.Language);
            NO_CONNECTION_TITLE = LanguageManager.GetString("MessageTitle_Omae_CannotConnection", GlobalOptions.Language);

            MoveControls();
        }

        private void frmOmaeUploadSheet_Load(object sender, EventArgs e)
        {

        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XSL Files (*.xsl; *.xslt)|*.xsl;*.xslt",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Clear the file path field.
            txtFilePath.Text = string.Empty;
            _lstFiles.Clear();

            // Make sure valid files were selected.
            foreach (string strFile in openFileDialog.FileNames)
            {
                if (!strFile.EndsWith(".xsl") && !strFile.EndsWith(".xslt"))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_CannotUploadSheet", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_CannotUploadFile", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            string[] strFileArray = openFileDialog.FileNames;

            // Conver the array to a List.
            for (int i = 0; i <= strFileArray.Length - 1; i++)
            {
                _lstFiles.Add(strFileArray[i]);
                txtFilePath.Text += strFileArray[i] + ", ";
            }
            if (!string.IsNullOrEmpty(txtFilePath.Text))
                txtFilePath.Text = txtFilePath.Text.Substring(0, txtFilePath.Text.Length - 2);
        }

        private void cmdUpload_Click(object sender, EventArgs e)
        {
            // Make sure a name has been entered.
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_SheetName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_SheetName", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure there is at least some sort of description.
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OameUpload_SheetDescription", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_SheetDescription", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure at least 1 file was selected.
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_SheetSelectFiles", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_SelectFile", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnSuccess = false;

            // Compress the files.
            byte[] bytFile = OmaeHelper.CompressMutiple(_lstFiles);

            // Make sure the file doesn't exceed 250K in size (256,000 bytes).
            if (bytFile.Length > 256000)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_FileTooLarge", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_FileTooLarge", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Upload the file.
            omaeSoapClient objService = OmaeHelper.GetOmaeService();
            try
            {
                cmdUpload.Enabled = false;
                txtDescription.Enabled = false;
                txtName.Enabled = false;
                if (objService.UploadSheet(_strUserName, _intSheetID, txtName.Text, txtDescription.Text, bytFile))
                {
                    blnSuccess = true;
                    MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_UploadComplete", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_UploadComplete", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(LanguageManager.GetString("Message_OmaeUpload_UploadFailed", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OmaeUpload_UploadFailed", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            objService.Close();
            cmdUpload.Enabled = true;
            txtDescription.Enabled = true;
            txtName.Enabled = true;

            if (blnSuccess)
                DialogResult = DialogResult.OK;
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            int intWidth = Math.Max(lblDescriptionLabel.Width, lblNameLabel.Width);
            intWidth = Math.Max(intWidth, lblFilePathLabel.Width);

            txtName.Left = lblNameLabel.Left + intWidth + 6;
            txtName.Width = Width - txtName.Left - 16;
            txtDescription.Left = lblDescriptionLabel.Left + intWidth + 6;
            txtDescription.Width = Width - txtDescription.Left - 16;
        }
        #endregion
    }
}
