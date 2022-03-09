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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public sealed partial class CreateCyberwareSuite : Form
    {
        private readonly Character _objCharacter;
        private readonly Improvement.ImprovementSource _objSource;
        private readonly string _strType;

        #region Control Events

        public CreateCyberwareSuite(Character objCharacter, Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Cyberware)
        {
            InitializeComponent();
            _objSource = objSource;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;

            if (_objSource == Improvement.ImprovementSource.Cyberware)
                _strType = "cyberware";
            else
            {
                _strType = "bioware";
                Text = LanguageManager.GetString("Title_CreateBiowareSuite");
            }

            txtFileName.Text = "custom_" + _strType + ".xml";
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the suite and file name fields are populated.
            if (string.IsNullOrEmpty(txtName.Text))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareSuite_SuiteName"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_SuiteName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareSuite_FileName"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_FileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the file name starts with custom and ends with _cyberware.xml.
            if (!txtFileName.Text.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) || !txtFileName.Text.EndsWith('_' + _strType + ".xml", StringComparison.OrdinalIgnoreCase))
            {
                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CyberwareSuite_InvalidFileName"), _strType),
                    await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_InvalidFileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // See if a Suite with this name already exists for the Custom category.
            // This was originally done without the XmlManager, but because amends and overrides and toggling custom data directories can change names, we need to use it.
            string strName = txtName.Text;
            if ((await _objCharacter.LoadDataXPathAsync(_strType + ".xml")).SelectSingleNode("/chummer/suites/suite[name = " + strName.CleanXPath() + ']') != null)
            {
                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CyberwareSuite_DuplicateName"), strName),
                    await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_DuplicateName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strPath = Path.Combine(Utils.GetStartupPath, "data", txtFileName.Text);

            bool blnNewFile = !File.Exists(strPath);

            // If this is not a new file, read in the existing contents.
            XmlDocument objXmlCurrentDocument = new XmlDocument { XmlResolver = null };
            if (!blnNewFile)
            {
                try
                {
                    objXmlCurrentDocument.LoadStandard(strPath);
                }
                catch (IOException ex)
                {
                    Program.ShowMessageBox(this, ex.ToString());
                    return;
                }
                catch (XmlException ex)
                {
                    Program.ShowMessageBox(this, ex.ToString());
                    return;
                }
            }

            using (FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    await objWriter.WriteStartDocumentAsync();

                    // <chummer>
                    await objWriter.WriteStartElementAsync("chummer");
                    if (!blnNewFile)
                    {
                        // <cyberwares>
                        await objWriter.WriteStartElementAsync(_strType + 's');
                        using (XmlNodeList xmlCyberwareList = objXmlCurrentDocument.SelectNodes("/chummer/" + _strType + 's'))
                            if (xmlCyberwareList?.Count > 0)
                                foreach (XmlNode xmlCyberware in xmlCyberwareList)
                                    xmlCyberware.WriteContentTo(objWriter);
                        // </cyberwares>
                        await objWriter.WriteEndElementAsync();
                    }

                    // <suites>
                    await objWriter.WriteStartElementAsync("suites");

                    // If this is not a new file, write out the current contents.
                    if (!blnNewFile)
                    {
                        using (XmlNodeList xmlCyberwareList = objXmlCurrentDocument.SelectNodes("/chummer/suites"))
                            if (xmlCyberwareList?.Count > 0)
                                foreach (XmlNode xmlCyberware in xmlCyberwareList)
                                    xmlCyberware.WriteContentTo(objWriter);
                    }

                    string strGrade = string.Empty;
                    // Determine the Grade of Cyberware.
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.SourceType == _objSource)
                        {
                            strGrade = objCyberware.Grade.Name;
                            break;
                        }
                    }

                    // <suite>
                    await objWriter.WriteStartElementAsync("suite");
                    // <name />
                    await objWriter.WriteElementStringAsync("id", Guid.NewGuid().ToString());
                    // <name />
                    await objWriter.WriteElementStringAsync("name", txtName.Text);
                    // <grade />
                    await objWriter.WriteElementStringAsync("grade", strGrade);
                    // <cyberwares>
                    await objWriter.WriteStartElementAsync(_strType + 's');

                    // Write out the Cyberware.
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.SourceType == _objSource)
                        {
                            // <cyberware>
                            await objWriter.WriteStartElementAsync(_strType);
                            await objWriter.WriteElementStringAsync("name", objCyberware.Name);
                            if (objCyberware.Rating > 0)
                                await objWriter.WriteElementStringAsync("rating", objCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            // Write out child items.
                            if (objCyberware.Children.Count > 0)
                            {
                                // <cyberwares>
                                await objWriter.WriteStartElementAsync(_strType + 's');
                                foreach (Cyberware objChild in objCyberware.Children)
                                {
                                    // Do not include items that come with the base item by default.
                                    if (objChild.Capacity != "[*]")
                                    {
                                        await objWriter.WriteStartElementAsync(_strType);
                                        await objWriter.WriteElementStringAsync("name", objChild.Name);
                                        if (objChild.Rating > 0)
                                            await objWriter.WriteElementStringAsync("rating", objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                        // </cyberware>
                                        await objWriter.WriteEndElementAsync();
                                    }
                                }

                                // </cyberwares>
                                await objWriter.WriteEndElementAsync();
                            }

                            // </cyberware>
                            await objWriter.WriteEndElementAsync();
                        }
                    }

                    // </cyberwares>
                    await objWriter.WriteEndElementAsync();
                    // </suite>
                    await objWriter.WriteEndElementAsync();
                    // </chummer>
                    await objWriter.WriteEndElementAsync();

                    await objWriter.WriteEndDocumentAsync();
                }
            }

            Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CyberwareSuite_SuiteCreated"), txtName.Text),
                await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_SuiteCreated"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void CreateCyberwareSuite_Load(object sender, EventArgs e)
        {
            txtName.Left = lblName.Left + lblName.Width + 6;
            txtName.Width = Width - txtName.Left - 19;
            txtFileName.Left = txtName.Left;
            txtFileName.Width = txtName.Width;
        }

        #endregion Control Events
    }
}
