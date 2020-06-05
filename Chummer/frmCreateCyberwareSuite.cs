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
using System.Text;
using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public sealed partial class frmCreateCyberwareSuite : Form
    {
        private readonly Character _objCharacter;
        private readonly Improvement.ImprovementSource _objSource;
        private readonly string _strType;

        #region Control Events
        public frmCreateCyberwareSuite(Character objCharacter, Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Cyberware)
        {
            InitializeComponent();
            _objSource = objSource;
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

        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the suite and file name fields are populated.
            if (string.IsNullOrEmpty(txtName.Text))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CyberwareSuite_SuiteName"), LanguageManager.GetString("MessageTitle_CyberwareSuite_SuiteName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CyberwareSuite_FileName"), LanguageManager.GetString("MessageTitle_CyberwareSuite_FileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the file name starts with custom and ends with _cyberware.xml.
            if (!txtFileName.Text.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) || !txtFileName.Text.EndsWith('_' + _strType + ".xml", StringComparison.OrdinalIgnoreCase))
            {
                Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_CyberwareSuite_InvalidFileName"), _strType),
                    LanguageManager.GetString("MessageTitle_CyberwareSuite_InvalidFileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // See if a Suite with this name already exists for the Custom category.
            // This was originally done without the XmlManager, but because amends and overrides and toggling custom data directories can change names, we need to use it.
            string strName = txtName.Text;
            if (XmlManager.Load(_strType + ".xml").SelectSingleNode("/chummer/suites/suite[name = \"" + strName + "\"]") != null)
            {
                Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_CyberwareSuite_DuplicateName"), strName),
                    LanguageManager.GetString("MessageTitle_CyberwareSuite_DuplicateName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strPath = Path.Combine(Utils.GetStartupPath, "data", txtFileName.Text);

            bool blnNewFile = !File.Exists(strPath);

            // If this is not a new file, read in the existing contents.
            XmlDocument objXmlCurrentDocument = new XmlDocument
            {
                XmlResolver = null
            };
            if (!blnNewFile)
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strPath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, new XmlReaderSettings {XmlResolver = null}))
                            objXmlCurrentDocument.Load(objXmlReader);
                }
                catch (IOException ex)
                {
                    Program.MainForm.ShowMessageBox(ex.ToString());
                    return;
                }
                catch (XmlException ex)
                {
                    Program.MainForm.ShowMessageBox(ex.ToString());
                    return;
                }
            }

            using (FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 1,
                    IndentChar = '\t'
                })
                {
                    objWriter.WriteStartDocument();

                    // <chummer>
                    objWriter.WriteStartElement("chummer");
                    if (!blnNewFile)
                    {
                        // <cyberwares>
                        objWriter.WriteStartElement(_strType + "s");
                        using (XmlNodeList xmlCyberwareList = objXmlCurrentDocument.SelectNodes("/chummer/" + _strType + "s"))
                            if (xmlCyberwareList?.Count > 0)
                                foreach (XmlNode xmlCyberware in xmlCyberwareList)
                                    xmlCyberware.WriteContentTo(objWriter);
                        // </cyberwares>
                        objWriter.WriteEndElement();
                    }

                    // <suites>
                    objWriter.WriteStartElement("suites");

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
                    objWriter.WriteStartElement("suite");
                    // <name />
                    objWriter.WriteElementString("id", Guid.NewGuid().ToString());
                    // <name />
                    objWriter.WriteElementString("name", txtName.Text);
                    // <grade />
                    objWriter.WriteElementString("grade", strGrade);
                    // <cyberwares>
                    objWriter.WriteStartElement(_strType + "s");

                    // Write out the Cyberware.
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.SourceType == _objSource)
                        {
                            // <cyberware>
                            objWriter.WriteStartElement(_strType);
                            objWriter.WriteElementString("name", objCyberware.Name);
                            if (objCyberware.Rating > 0)
                                objWriter.WriteElementString("rating", objCyberware.Rating.ToString(GlobalOptions.InvariantCultureInfo));
                            // Write out child items.
                            if (objCyberware.Children.Count > 0)
                            {
                                // <cyberwares>
                                objWriter.WriteStartElement(_strType + "s");
                                foreach (Cyberware objChild in objCyberware.Children)
                                {
                                    // Do not include items that come with the base item by default.
                                    if (objChild.Capacity != "[*]")
                                    {
                                        objWriter.WriteStartElement(_strType);
                                        objWriter.WriteElementString("name", objChild.Name);
                                        if (objChild.Rating > 0)
                                            objWriter.WriteElementString("rating", objChild.Rating.ToString(GlobalOptions.InvariantCultureInfo));
                                        // </cyberware>
                                        objWriter.WriteEndElement();
                                    }
                                }

                                // </cyberwares>
                                objWriter.WriteEndElement();
                            }

                            // </cyberware>
                            objWriter.WriteEndElement();
                        }
                    }

                    // </cyberwares>
                    objWriter.WriteEndElement();
                    // </suite>
                    objWriter.WriteEndElement();
                    // </chummer>
                    objWriter.WriteEndElement();

                    objWriter.WriteEndDocument();
                }
            }

            Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_CyberwareSuite_SuiteCreated"), txtName.Text),
                LanguageManager.GetString("MessageTitle_CyberwareSuite_SuiteCreated"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void frmCreateCyberwareSuite_Load(object sender, EventArgs e)
        {
            txtName.Left = lblName.Left + lblName.Width + 6;
            txtName.Width = Width - txtName.Left - 19;
            txtFileName.Left = txtName.Left;
            txtFileName.Width = txtName.Width;
        }
        #endregion
    }
}
