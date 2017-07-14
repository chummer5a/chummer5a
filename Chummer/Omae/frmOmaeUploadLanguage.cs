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
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using System.Xml;
using Chummer.TranslationService;

namespace Chummer
{
    public partial class frmOmaeUploadLanguage : Form
    {
        private readonly OmaeHelper _objOmaeHelper = new OmaeHelper();

        // Error message constants.
        private readonly string NO_CONNECTION_MESSAGE = string.Empty;
        private readonly string NO_CONNECTION_TITLE = string.Empty;
        private const string MESSAGE_SUCCESS = "Language file was successfully uploaded.";
        private const string MESSAGE_UNAUTHORIZED = "You are not authorized to upload files for this language.";
        private const string MESSAGE_INVALID_FILE = "This is not a valid Chummer language file.";

        private const int RESULT_SUCCESS = 0;
        private const int RESULT_UNAUTHORIZED = 1;
        private const int RESULT_INVALID_FILE = 2;

        private string _strUserName;

        #region Control Events
        public frmOmaeUploadLanguage(string strUserName)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
            _strUserName = strUserName;

            NO_CONNECTION_MESSAGE = LanguageManager.Instance.GetString("Message_Omae_CannotConnection");
            NO_CONNECTION_TITLE = LanguageManager.Instance.GetString("MessageTitle_Omae_CannotConnection");
        }

        private void frmOmaeUploadLanguage_Load(object sender, EventArgs e)
        {

        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Clear the file path field.
            txtFilePath.Text = string.Empty;

            // Make sure a .chum5 file was selected.
            if (!openFileDialog.FileName.EndsWith(".xml"))
            {
                MessageBox.Show("You must select a valid XML file to upload.", "Cannot Upload File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Attempt to load the character and make sure it's a valid character file.
            XmlDocument objXmlDocument = new XmlDocument();
            try
            {
                objXmlDocument.Load(openFileDialog.FileName);
            }
            catch
            {
                MessageBox.Show("This is not a valid XML file.", "Invalid XML File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtFilePath.Text = openFileDialog.FileName;
        }

        private void cmdUpload_Click(object sender, EventArgs e)
        {
            // Make sure a file has been selected.
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("Please select a language file to upload.", "No Language File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            byte[] bytFile = File.ReadAllBytes(txtFilePath.Text);
            string strFileName = Path.GetFileName(txtFilePath.Text);

            translationSoapClient objService = _objOmaeHelper.GetTranslationService();
            try
            {
                int intResult = objService.UploadLanguage(_strUserName, strFileName, bytFile);

                if (intResult == RESULT_SUCCESS)
                    MessageBox.Show(MESSAGE_SUCCESS, "File Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (intResult == RESULT_UNAUTHORIZED)
                    MessageBox.Show(MESSAGE_UNAUTHORIZED, "File Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (intResult == RESULT_INVALID_FILE)
                    MessageBox.Show(MESSAGE_INVALID_FILE, "File Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            objService.Close();
        }
        #endregion
    }
}