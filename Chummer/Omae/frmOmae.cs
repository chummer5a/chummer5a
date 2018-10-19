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
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.OmaeService;
using Chummer.TranslationService;
using Microsoft.Win32;

namespace Chummer
{
    public enum OmaeMode
    {
        Character = 1,
        Data = 2,
        Sheets = 3,
    }

    public partial class frmOmae : Form
    {
        // Error message constants.
        private readonly string NO_CONNECTION_MESSAGE = string.Empty;
        private readonly string NO_CONNECTION_TITLE = string.Empty;
        
        private readonly List<ListItem> _lstCharacterTypes = new List<ListItem>();

        private bool _blnLoggedIn = false;
        private string _strUserName = string.Empty;
        private readonly frmChummerMain _frmMain;
        private OmaeMode _objMode = OmaeMode.Character;

        #region Helper Methods
        /// <summary>
        /// Populate the sort order list.
        /// </summary>
        public void PopulateSortOrder()
        {
            List<ListItem> lstSort = new List<ListItem>
            {
                new ListItem("0", LanguageManager.GetString("String_Name", GlobalOptions.Language)),
                new ListItem("1", "Most Recent"),
                new ListItem("2", "Most Downloaded")
            };

            cboSortOrder.DataSource = lstSort;
            cboSortOrder.ValueMember = "Value";
            cboSortOrder.DisplayMember = "Name";
        }

        /// <summary>
        /// Populate the Created Status list.
        /// </summary>
        public void PopulateMode()
        {
            List<ListItem> lstMode = new List<ListItem>
            {
                new ListItem("-1", "Any Mode"),
                new ListItem("0", "Create Mode"),
                new ListItem("1", "Career Mode")
            };

            cboFilterMode.DataSource = lstMode;
            cboFilterMode.ValueMember = "Value";
            cboFilterMode.DisplayMember = "Name";
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        public void PopulateMetatypes()
        {
            XmlDocument objXmlDocument = XmlManager.Load("metatypes.xml");

            foreach (XmlNode objNode in objXmlDocument.SelectNodes("/chummer/metatypes/metatype"))
                cboFilterMetatype.Items.Add(objNode["name"].InnerText);

            objXmlDocument = XmlManager.Load("critters.xml");

            foreach (XmlNode objNode in objXmlDocument.SelectNodes("/chummer/metatypes/metatype"))
                cboFilterMetatype.Items.Add(objNode["name"].InnerText);
        }

        /// <summary>
        /// Populate the list of Qualities.
        /// </summary>
        public void PopulateQualities()
        {
            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");

            foreach (XmlNode objNode in objXmlDocument.SelectNodes("/chummer/qualities/quality"))
            {
                cboFilterQuality1.Items.Add(objNode["name"].InnerText);
                cboFilterQuality2.Items.Add(objNode["name"].InnerText);
                cboFilterQuality3.Items.Add(objNode["name"].InnerText);
            }
        }
        
        /// <summary>
        /// Remove unsafe path characters from the file name.
        /// </summary>
        /// <param name="strValue">File name to parse.</param>
        private static string FileSafe(string strValue)
        {
            return strValue.FastEscape(' ', '_', '/', ':', '*', '?', '<', '>', '|', '\\');
        }

        private void MoveControls()
        {
            cboCharacterTypes.Left = lblSearchFor.Left + lblSearchFor.Width + 6;
            lblSortedBy.Left = cboCharacterTypes.Left + cboCharacterTypes.Width + 6;
            cboSortOrder.Left = lblSortedBy.Left + lblSortedBy.Width + 6;
            cmdSearch.Left = cboSortOrder.Left + cboSortOrder.Width + 6;
            cmdFilterToggle.Left = lblFilter.Left + lblFilter.Width + 6;
            cmdFilterClear.Left = cmdFilterToggle.Left + cmdFilterToggle.Width + 6;
        }
        #endregion

        #region Omae Methods
        /// <summary>
        /// Attempt to get the list of character types.
        /// </summary>
        public bool GetCharacterTypes()
        {
            omaeSoapClient objService = OmaeHelper.GetOmaeService();

            try
            {
                MemoryStream objStream = new MemoryStream();
                XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

                objService.GetCharacterTypes().WriteTo(objWriter);
                // Flush the output.
                objWriter.Flush();

                XmlDocument objXmlDocument = OmaeHelper.XmlDocumentFromStream(objStream);

                // Close everything now that we're done.
                objWriter.Close();

                // Stuff all of the items into a ListItem List.
                foreach (XmlNode objNode in objXmlDocument.SelectNodes("/types/type"))
                {
                    _lstCharacterTypes.Add(new ListItem(objNode["id"].InnerText, objNode["name"].InnerText));
                }
                _lstCharacterTypes.Add(new ListItem("4", "Official NPC Packs"));
                _lstCharacterTypes.Add(new ListItem("data", "Data"));
                _lstCharacterTypes.Add(new ListItem("sheets", "Character Sheets"));

                cboCharacterTypes.Items.Clear();
                cboCharacterTypes.DataSource = _lstCharacterTypes;
                cboCharacterTypes.ValueMember = "Value";
                cboCharacterTypes.DisplayMember = "Name";
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            objService.Close();

            return false;
        }

        /// <summary>
        /// Show the Upload Language button if the user is allowed to upload languages.
        /// </summary>
        private void CheckUploadLanguagePermission()
        {
            if (!_blnLoggedIn)
                cmdUploadLanguage.Visible = false;
            else
            {
                translationSoapClient objService = OmaeHelper.GetTranslationService();
                try
                {
                    cmdUploadLanguage.Visible = objService.CanUploadLanguage(_strUserName);
                }
                catch
                {
                    cmdUploadLanguage.Visible = false;
                }
                objService.Close();
            }
        }
        #endregion

        #region OmaeRecord Events
        private void objRecord_OmaeDownloadClicked(Object sender)
        {
            // Setup the web service.
            OmaeRecord objRecord = (OmaeRecord)sender;
            omaeSoapClient objService = OmaeHelper.GetOmaeService();

            if (_objMode == OmaeMode.Character)
            {
                if (objRecord.CharacterType != 4)
                {
                    // Download the selected character.
                    string strFileName = objRecord.CharacterName + ".chum5";
                    strFileName = FileSafe(strFileName);

                    // If the Omae save directory does not yet exist, create it.
                    string strSavePath = Path.Combine(Utils.GetStartupPath, "saves");
                    if (!Directory.Exists(strSavePath))
                        Directory.CreateDirectory(strSavePath);
                    string omaeDirectoryPath = Path.Combine(strSavePath, "omae");
                    if (!Directory.Exists(omaeDirectoryPath))
                        Directory.CreateDirectory(omaeDirectoryPath);

                    // See if there is already a file with the character's name in the Downloads directory.
                    string strFullPath = Path.Combine(omaeDirectoryPath, strFileName);
                    if (File.Exists(strFullPath))
                    {
                        if (MessageBox.Show(LanguageManager.GetString("Message_Omae_FileExists", GlobalOptions.Language).Replace("{0}", strFileName), LanguageManager.GetString("MessageTitle_Omae_FileExists", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            return;
                    }

                    try
                    {
                        // Download the compressed file.
                        byte[] bytFile = objService.DownloadCharacter(objRecord.CharacterID);

                        if (bytFile.Length == 0)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_Omae_CannotFindCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_CannotFindCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            objService.Close();
                            return;
                        }

                        // Decompress the byte array and write it to a file.
                        bytFile = OmaeHelper.Decompress(bytFile);
                        File.WriteAllBytes(strFullPath, bytFile);
                        if (MessageBox.Show(LanguageManager.GetString("Message_Omae_CharacterDownloaded", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_CharacterDownloaded", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Cursor = Cursors.WaitCursor;
                            Character objOpenCharacter = Program.MainForm.LoadCharacter(strFullPath);
                            Cursor = Cursors.Default;
                            _frmMain.OpenCharacter(objOpenCharacter);
                        }
                    }
                    catch (EndpointNotFoundException)
                    {
                        MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Download the selected NPC pack.
                    string strFileName = objRecord.CharacterName + ".chum5";
                    strFileName = FileSafe(strFileName);

                    // If the Omae save directory does not yet exist, create it.
                    string strSavePath = Path.Combine(Utils.GetStartupPath, "saves");
                    if (!Directory.Exists(strSavePath))
                        Directory.CreateDirectory(strSavePath);

                    try
                    {
                        // Download the compressed file.
                        byte[] bytFile = objService.DownloadCharacter(objRecord.CharacterID);

                        if (bytFile.Length == 0)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_Omae_CannotFindCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_CannotFindCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            objService.Close();
                            return;
                        }

                        // Decompress the byte array and write it to a file.
                        OmaeHelper.DecompressNPCs(bytFile);
                        MessageBox.Show(LanguageManager.GetString("Message_Omae_NPCPackDownloaded", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_CharacterDownloaded", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (EndpointNotFoundException)
                    {
                        MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (_objMode == OmaeMode.Data)
            {
                try
                {
                    // Download the compressed file.
                    byte[] bytFile = objService.DownloadDataFile(objRecord.CharacterID);

                    if (bytFile.Length == 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Omae_CannotFindData", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_CannotFindData", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        objService.Close();
                        return;
                    }

                    // Decompress the byte array and write it to a file.
                    OmaeHelper.DecompressDataFile(bytFile, objRecord.CharacterID.ToString());
                    // Show a message saying everything is done.
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_DataDownloaded", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DataDownloaded", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (EndpointNotFoundException)
                {
                    MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (_objMode == OmaeMode.Sheets)
            {
                // If the Omae sheets directory does not yet exist, create it.
                string strSheetsPath = Path.Combine(Utils.GetStartupPath, "sheets", "omae");
                if (!Directory.Exists(strSheetsPath))
                    Directory.CreateDirectory(strSheetsPath);

                try
                {
                    // Download the compressed file.
                    byte[] bytFile = objService.DownloadSheet(objRecord.CharacterID);

                    if (bytFile.Length == 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Omae_CannotFindSheet", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_CannotFindSheet", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        objService.Close();
                        return;
                    }

                    // Decompress the byte array and write it to a file.
                    OmaeHelper.DecompressCharacterSheet(bytFile);
                    // Show a message saying everything is done.
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_SheetDownloaded", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_SheetDownloaded", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (EndpointNotFoundException)
                {
                    MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Close the service now that we're done with it.
            objService.Close();
        }

        private void objRecord_OmaePostUpdateClicked(Object sender)
        {
            OmaeRecord objRecord = (OmaeRecord)sender;
            if (_objMode == OmaeMode.Character)
            {
                frmOmaeUpload frmUpload = new frmOmaeUpload(txtUserName.Text, _lstCharacterTypes, objRecord.CharacterType, objRecord.CharacterID, objRecord.Description);
                frmUpload.ShowDialog(this);
            }
            if (_objMode == OmaeMode.Data)
            {
                frmOmaeUploadData frmUpload = new frmOmaeUploadData(txtUserName.Text, objRecord.CharacterID, objRecord.Description, objRecord.CharacterName);
                frmUpload.ShowDialog(this);
            }
            if (_objMode == OmaeMode.Sheets)
            {
                frmOmaeUploadSheet frmUpload = new frmOmaeUploadSheet(txtUserName.Text, objRecord.CharacterID, objRecord.Description, objRecord.CharacterName);
                frmUpload.ShowDialog(this);
            }
        }

        private void objRecord_OmaeDeleteClicked(Object sender)
        {
            if (_objMode == OmaeMode.Character)
            {
                // Make sure the user wants to delete the character.
                if (MessageBox.Show(LanguageManager.GetString("Message_Omae_ConfirmDelete", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteCharacter", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                // Delete the character.
                OmaeRecord objRecord = (OmaeRecord) sender;
                omaeSoapClient objService = OmaeHelper.GetOmaeService();

                if (objService.DeleteCharacter(objRecord.CharacterID))
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_CharacterDeleted", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_CharacterDeleteError", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (_objMode == OmaeMode.Data)
            {
                // Make sure the user wants to delete the data.
                if (MessageBox.Show(LanguageManager.GetString("Message_Omae_ConfirmData", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteData", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                // Delete the data.
                OmaeRecord objRecord = (OmaeRecord)sender;
                omaeSoapClient objService = OmaeHelper.GetOmaeService();

                if (objService.DeleteDataFile(objRecord.CharacterID))
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_DataDeleted", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteData", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_DataDeleteError", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteData", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (_objMode == OmaeMode.Sheets)
            {
                // Make sure the user wants to delete the character sheet.
                if (MessageBox.Show(LanguageManager.GetString("Message_Omae_ConfirmSheet", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteData", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                // Delete the data.
                OmaeRecord objRecord = (OmaeRecord)sender;
                omaeSoapClient objService = OmaeHelper.GetOmaeService();

                if (objService.DeleteSheet(objRecord.CharacterID))
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_SheetDeleted", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteData", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_SheetDeleteError", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_DeleteData", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Control Events
        public frmOmae(frmChummerMain frmMainForm)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _frmMain = frmMainForm;

            NO_CONNECTION_MESSAGE = LanguageManager.GetString("Message_Omae_CannotConnection", GlobalOptions.Language);
            NO_CONNECTION_TITLE = LanguageManager.GetString("MessageTitle_Omae_CannotConnection", GlobalOptions.Language);
        }

        private void frmOmae_Load(object sender, EventArgs e)
        {
            _strUserName = GlobalOptions.OmaeUserName;
            txtUserName.Text = _strUserName;
            if (!string.IsNullOrEmpty(txtUserName.Text))
                txtPassword.Focus();
            else
                txtUserName.Focus();

            if (GlobalOptions.OmaeAutoLogin)
            {
                chkAutoLogin.Checked = true;
                txtPassword.Text = OmaeHelper.Base64Decode(GlobalOptions.OmaePassword);
                cmdLogin_Click(sender, e);
            }

            PopulateSortOrder();
            PopulateMetatypes();
            PopulateQualities();
            PopulateMode();
            GetCharacterTypes();
            MoveControls();

            if (txtUserName.Text == "Nebular" && _blnLoggedIn)
            {
                cmdCompress.Visible = true;
                cmdCompress.Text = "Compress Files";
                cmdCompressData.Visible = true;
                cmdCompressData.Text = "Compress Data";
            }

            CheckUploadLanguagePermission();
        }

        private void cmdRegister_Click(object sender, EventArgs e)
        {
            // Make sure User Name and Password are provided.
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Omae_ChooseUsername", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_ChooseUsername", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Omae_ChoosePassword", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_ChoosePassword", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            omaeSoapClient objService = OmaeHelper.GetOmaeService();
            try
            {
                int intResult = objService.RegisterUser(txtUserName.Text, OmaeHelper.Base64Encode(txtPassword.Text));

                if (intResult == 0)
                {
                    // Registered successfully.
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_AccountCreated", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_Registration", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (intResult == -1)
                {
                    // Username already exists.
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_AccountExists", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_Registration", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            objService.Close();
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            // Make sure User Name and Password are provided.
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Omae_EnterUsername", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_ChooseUsername", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Omae_EnterPassword", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_ChoosePassword", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            omaeSoapClient objService = OmaeHelper.GetOmaeService();
            try
            {
                bool blnResult = objService.Login(txtUserName.Text, OmaeHelper.Base64Encode(txtPassword.Text));

                if (blnResult)
                {
                    _strUserName = txtUserName.Text;
                    _blnLoggedIn = true;
                    lblLoggedIn.Text = LanguageManager.GetString("Label_Omae_LoggedInAs", GlobalOptions.Language).Replace("{0}", txtUserName.Text);
                    panLogin.Visible = false;
                    panLoggedIn.Visible = true;

                    // Save the settings.
                    RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
                    GlobalOptions.OmaeUserName = txtUserName.Text;
                    objRegistry.SetValue("omaeusername", txtUserName.Text);
                    if (chkAutoLogin.Checked)
                    {
                        GlobalOptions.OmaePassword = OmaeHelper.Base64Encode(txtPassword.Text);
                        objRegistry.SetValue("omaepassword", OmaeHelper.Base64Encode(txtPassword.Text));
                        GlobalOptions.OmaeAutoLogin = chkAutoLogin.Checked;
                        objRegistry.SetValue("omaeautologin", chkAutoLogin.Checked.ToString());
                    }
                    else
                    {
                        GlobalOptions.OmaePassword = string.Empty;
                        objRegistry.SetValue("omaepassword", string.Empty);
                        GlobalOptions.OmaeAutoLogin = chkAutoLogin.Checked;
                        objRegistry.SetValue("omaeautologin", chkAutoLogin.Checked.ToString());
                    }
                    objRegistry.Close();
                }
                else
                    MessageBox.Show(LanguageManager.GetString("Message_Omae_CannotLogin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_OmaeLogin", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            objService.Close();
            CheckUploadLanguagePermission();
        }

        private void cmdUpload_Click(object sender, EventArgs e)
        {
            if (!_blnLoggedIn)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Omae_MustLogin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_OmaeLogin", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            OmaeMode objMode = OmaeMode.Character;
            switch (cboCharacterTypes.SelectedValue.ToString())
            {
                case "data":
                    objMode = OmaeMode.Data;
                    break;
                case "sheets":
                    objMode = OmaeMode.Sheets;
                    break;
                default:
                    objMode = OmaeMode.Character;
                    break;
            }

            if (objMode == OmaeMode.Character)
            {
                frmOmaeUpload frmUpload = new frmOmaeUpload(txtUserName.Text, _lstCharacterTypes, Convert.ToInt32(cboCharacterTypes.SelectedValue));
                frmUpload.ShowDialog(this);
            }
            else if (objMode == OmaeMode.Data)
            {
                frmOmaeUploadData frmUpload = new frmOmaeUploadData(txtUserName.Text);
                frmUpload.ShowDialog(this);
            }
            else if (objMode == OmaeMode.Sheets)
            {
                frmOmaeUploadSheet frmUpload = new frmOmaeUploadSheet(txtUserName.Text);
                frmUpload.ShowDialog(this);
            }
            return;
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            omaeSoapClient objService = OmaeHelper.GetOmaeService();

            // Clear the current contents of the Omae Panel. Detach the events before clearing it.
            foreach (OmaeRecord objRecord in panOmae.Controls.OfType<OmaeRecord>())
            {
                objRecord.OmaeDownloadClicked -= objRecord_OmaeDownloadClicked;
                objRecord.OmaePostUpdateClicked -= objRecord_OmaePostUpdateClicked;
                objRecord.OmaeDeleteClicked -= objRecord_OmaeDeleteClicked;
            }
            panOmae.Controls.Clear();

            // Set the current operating mode.
            switch (cboCharacterTypes.SelectedValue.ToString())
            {
                case "data":
                    _objMode = OmaeMode.Data;
                    break;
                case "sheets":
                    _objMode = OmaeMode.Sheets;
                    break;
                default:
                    _objMode = OmaeMode.Character;
                    break;
            }

            // Search for characters.
            if (_objMode == OmaeMode.Character)
            {
                try
                {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

                    objService.FetchCharacters153(Convert.ToInt32(cboCharacterTypes.SelectedValue), 
                                                  Convert.ToInt32(cboSortOrder.SelectedValue), cboFilterMetatype.Text,
                                                  cboFilterMetavariant.Text, Convert.ToInt32(cboFilterMode.SelectedValue),
                                                  txtFilterUser.Text, cboFilterQuality1.Text, cboFilterQuality2.Text,
                                                  cboFilterQuality3.Text).WriteTo(objWriter);
                    // Flush the output.
                    objWriter.Flush();

                    XmlDocument objXmlDocument = OmaeHelper.XmlDocumentFromStream(objStream);

                    // Close everything now that we're done.
                    objWriter.Close();

                    if (objXmlDocument.SelectNodes("/characters/character").Count == 0)
                    {
                        Label lblResults = new Label
                        {
                            Text = LanguageManager.GetString("String_Omae_NoCharacters", GlobalOptions.Language),
                            Width = 200
                        };
                        panOmae.Controls.Add(lblResults);
                    }
                    else
                    {
                        int intCounter = -1;
                        foreach (XmlNode objNode in objXmlDocument.SelectNodes("/characters/character"))
                        {
                            intCounter++;
                            OmaeRecord objRecord = new OmaeRecord(objNode, Convert.ToInt32(cboCharacterTypes.SelectedValue), OmaeMode.Character);
                            objRecord.OmaeDownloadClicked += objRecord_OmaeDownloadClicked;
                            objRecord.OmaePostUpdateClicked += objRecord_OmaePostUpdateClicked;
                            objRecord.OmaeDeleteClicked += objRecord_OmaeDeleteClicked;
                            if ((objRecord.UserName == txtUserName.Text || txtUserName.Text == "Nebular") && _blnLoggedIn)
                                objRecord.OwnedByUser = true;
                            objRecord.Top = intCounter*88;
                            panOmae.Controls.Add(objRecord);
                        }
                    }
                    objService.Close();
                }
                catch (EndpointNotFoundException)
                {
                    MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Search for data.
            else if (_objMode == OmaeMode.Data)
            {
                try
                {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

                    objService.FetchDataFiles(Convert.ToInt32(cboSortOrder.SelectedValue), string.Empty, txtFilterUser.Text).WriteTo(objWriter);
                    // Flush the output.
                    objWriter.Flush();

                    XmlDocument objXmlDocument = OmaeHelper.XmlDocumentFromStream(objStream);

                    // Close everything now that we're done.
                    objWriter.Close();

                    if (objXmlDocument.SelectNodes("/datas/data").Count == 0)
                    {
                        Label lblResults = new Label
                        {
                            Text = LanguageManager.GetString("String_Omae_NoData", GlobalOptions.Language),
                            Width = 200
                        };
                        panOmae.Controls.Add(lblResults);
                    }
                    else
                    {
                        int intCounter = -1;
                        foreach (XmlNode objNode in objXmlDocument.SelectNodes("/datas/data"))
                        {
                            intCounter++;
                            OmaeRecord objRecord = new OmaeRecord(objNode, 0, OmaeMode.Data);
                            objRecord.OmaeDownloadClicked += objRecord_OmaeDownloadClicked;
                            objRecord.OmaePostUpdateClicked += objRecord_OmaePostUpdateClicked;
                            objRecord.OmaeDeleteClicked += objRecord_OmaeDeleteClicked;
                            if ((objRecord.UserName == txtUserName.Text || txtUserName.Text == "Nebular") && _blnLoggedIn)
                                objRecord.OwnedByUser = true;
                            objRecord.Top = intCounter * 88;
                            panOmae.Controls.Add(objRecord);
                        }
                    }
                    objService.Close();
                }
                catch (EndpointNotFoundException)
                {
                    MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Search for character sheets.
            else if (_objMode == OmaeMode.Sheets)
            {
                try
                {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

                    objService.FetchSheets(Convert.ToInt32(cboSortOrder.SelectedValue), txtFilterUser.Text).WriteTo(objWriter);
                    // Flush the output.
                    objWriter.Flush();

                    XmlDocument objXmlDocument = OmaeHelper.XmlDocumentFromStream(objStream);

                    // Close everything now that we're done.
                    objWriter.Close();

                    if (objXmlDocument.SelectNodes("/sheets/sheet").Count == 0)
                    {
                        Label lblResults = new Label
                        {
                            Text = LanguageManager.GetString("String_Omae_NoSheets", GlobalOptions.Language),
                            Width = 200
                        };
                        panOmae.Controls.Add(lblResults);
                    }
                    else
                    {
                        int intCounter = -1;
                        foreach (XmlNode objNode in objXmlDocument.SelectNodes("/sheets/sheet"))
                        {
                            intCounter++;
                            OmaeRecord objRecord = new OmaeRecord(objNode, 0, OmaeMode.Sheets);
                            objRecord.OmaeDownloadClicked += objRecord_OmaeDownloadClicked;
                            objRecord.OmaePostUpdateClicked += objRecord_OmaePostUpdateClicked;
                            objRecord.OmaeDeleteClicked += objRecord_OmaeDeleteClicked;
                            if ((objRecord.UserName == txtUserName.Text || txtUserName.Text == "Nebular") && _blnLoggedIn)
                                objRecord.OwnedByUser = true;
                            objRecord.Top = intCounter * 88;
                            panOmae.Controls.Add(objRecord);
                        }
                    }
                    objService.Close();
                }
                catch (EndpointNotFoundException)
                {
                    MessageBox.Show(NO_CONNECTION_MESSAGE, NO_CONNECTION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cmdFilterToggle_Click(object sender, EventArgs e)
        {
            // Toggle the height of the Filter Panel.
            if (panFilter.Height == 31)
            {
                panFilter.Height = 120;
                cmdFilterToggle.Text = LanguageManager.GetString("Button_Omae_HideFilter", GlobalOptions.Language);
            }
            else
            {
                panFilter.Height = 31;
                cmdFilterToggle.Text = LanguageManager.GetString("Button_Omae_ShowFilter", GlobalOptions.Language);
            }
            flowLayoutPanel1_Resize(sender, e);
        }

        private void cmdFilterClear_Click(object sender, EventArgs e)
        {
            // Clear the contents of the Filter controls.
            cboFilterMetatype.Text = string.Empty;
            cboFilterMetavariant.Text = string.Empty;
            txtFilterUser.Text = string.Empty;
            cboFilterQuality1.Text = string.Empty;
            cboFilterQuality2.Text = string.Empty;
            cboFilterQuality3.Text = string.Empty;
            cboFilterMode.Text = "Any Mode";
        }

        private void frmOmae_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove the Main window's reference to this form.
            _frmMain.OmaeWindow = null;
        }

        private void cmdPasswordReset_Click(object sender, EventArgs e)
        {
            omaeSoapClient objService = OmaeHelper.GetOmaeService();
            if (!objService.ResetPassword(txtUserName.Text))
                MessageBox.Show(LanguageManager.GetString("Message_Omae_PasswordNoEmail", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_PasswordReset", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(LanguageManager.GetString("Message_Omae_PasswordReset", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_PasswordReset", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cmdMyAccount_Click(object sender, EventArgs e)
        {
            if (!_blnLoggedIn)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Omae_InfoRequiresLogin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Omae_OmaeLogin", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmOmaeAccount frmMyAccount = new frmOmaeAccount(_strUserName);
            frmMyAccount.ShowDialog(this);
        }

        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            panOmae.Height = flowLayoutPanel1.Height - panOmae.Top;
        }

        private void frmOmae_Resize(object sender, EventArgs e)
        {
            Width = 702;
        }

        private void cmdUploadLanguage_Click(object sender, EventArgs e)
        {
            frmOmaeUploadLanguage frmUpload = new frmOmaeUploadLanguage(_strUserName);
            frmUpload.ShowDialog(this);
        }

        private void cboCharacterTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Disable the Quality, Mode, and Metavariant fields if we're not searching for characters since these don't apply.
                bool blnEnabled = (cboCharacterTypes.SelectedValue.ToString() != "data" && cboCharacterTypes.SelectedValue.ToString() != "sheets");
                lblFilterMetatype.Enabled = blnEnabled;
                lblFilterQuality1.Enabled = blnEnabled;
                lblFilterQuality2.Enabled = blnEnabled;
                lblFilterQuality3.Enabled = blnEnabled;
                lblFilterMetavariant.Enabled = blnEnabled;
                lblFilterMode.Enabled = blnEnabled;
                cboFilterMetatype.Enabled = blnEnabled;
                cboFilterQuality1.Enabled = blnEnabled;
                cboFilterQuality2.Enabled = blnEnabled;
                cboFilterQuality3.Enabled = blnEnabled;
                cboFilterMetavariant.Enabled = blnEnabled;
                cboFilterMode.Enabled = blnEnabled;

                if (cboCharacterTypes.SelectedValue.ToString() == "data" || cboCharacterTypes.SelectedValue.ToString() == "sheets")
                    cmdUpload.Text = LanguageManager.GetString("Button_Omae_UploadData", GlobalOptions.Language);
                else
                    cmdUpload.Text = LanguageManager.GetString("Button_Omae_Upload", GlobalOptions.Language);
            }
            catch
            {
            }
        }

        private void cmdCompress_Click(object sender, EventArgs e)
        {
            frmOmaeCompress frmCompress = new frmOmaeCompress();
            frmCompress.ShowDialog(this);
        }

        private void cmdCompressData_Click(object sender, EventArgs e)
        {
            foreach (string strFile in Directory.GetFiles(Path.Combine(Utils.GetStartupPath, "data"), "*.xml"))
            {
                byte[] bytFile = File.ReadAllBytes(strFile);
                bytFile = OmaeHelper.Compress(bytFile);
                File.WriteAllBytes(strFile.Replace(".xml", ".zip"), bytFile);
            }

            MessageBox.Show("Done");
        }
        #endregion
    }
}
