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
        private readonly Improvement.ImprovementSource _eSource;
        private readonly string _strType;

        #region Control Events

        public CreateCyberwareSuite(Character objCharacter, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Cyberware)
        {
            InitializeComponent();
            _eSource = eSource;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;

            if (_eSource == Improvement.ImprovementSource.Cyberware)
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
            string strName = await txtName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strName))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareSuite_SuiteName").ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_SuiteName").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strFileName = await txtFileName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strFileName))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareSuite_FileName").ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_FileName").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the file name starts with custom and ends with _cyberware.xml.
            if (!strFileName.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) || !strFileName.EndsWith('_' + _strType + ".xml", StringComparison.OrdinalIgnoreCase))
            {
                Program.ShowScrollableMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CyberwareSuite_InvalidFileName").ConfigureAwait(false), _strType),
                    await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_InvalidFileName").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // See if a Suite with this name already exists for the Custom category.
            // This was originally done without the XmlManager, but because amends and overrides and toggling custom data directories can change names, we need to use it.
            if ((await _objCharacter.LoadDataXPathAsync(_strType + ".xml").ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/suites/suite", strName) != null)
            {
                Program.ShowScrollableMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CyberwareSuite_DuplicateName").ConfigureAwait(false), strName),
                    await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_DuplicateName").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    await objXmlCurrentDocument.LoadStandardAsync(strPath).ConfigureAwait(false);
                }
                catch (IOException ex)
                {
                    Program.ShowScrollableMessageBox(this, ex.ToString());
                    return;
                }
                catch (XmlException ex)
                {
                    Program.ShowScrollableMessageBox(this, ex.ToString());
                    return;
                }
            }

            using (FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                    // <chummer>
                    await objWriter.WriteStartElementAsync("chummer").ConfigureAwait(false);
                    if (!blnNewFile)
                    {
                        // <cyberwares>
                        await objWriter.WriteStartElementAsync(_strType + 's').ConfigureAwait(false);
                        using (XmlNodeList xmlCyberwareList = objXmlCurrentDocument.SelectNodes("/chummer/" + _strType + 's'))
                            if (xmlCyberwareList?.Count > 0)
                                foreach (XmlNode xmlCyberware in xmlCyberwareList)
                                    xmlCyberware.WriteContentTo(objWriter);
                        // </cyberwares>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // <suites>
                    await objWriter.WriteStartElementAsync("suites").ConfigureAwait(false);

                    // If this is not a new file, write out the current contents.
                    if (!blnNewFile)
                    {
                        using (XmlNodeList xmlCyberwareList = objXmlCurrentDocument.SelectNodes("/chummer/suites"))
                            if (xmlCyberwareList?.Count > 0)
                                foreach (XmlNode xmlCyberware in xmlCyberwareList)
                                    xmlCyberware.WriteContentTo(objWriter);
                    }

                    // Determine the Grade of Cyberware.
                    Cyberware objFirstWare = await _objCharacter.Cyberware.FirstOrDefaultAsync(x => x.SourceType == _eSource)
                        .ConfigureAwait(false);
                    string strGrade
                        = objFirstWare != null ? (await objFirstWare.GetGradeAsync().ConfigureAwait(false)).Name : string.Empty;

                    // <suite>
                    await objWriter.WriteStartElementAsync("suite").ConfigureAwait(false);
                    // <name />
                    await objWriter.WriteElementStringAsync("id", Guid.NewGuid().ToString()).ConfigureAwait(false);
                    // <name />
                    await objWriter.WriteElementStringAsync("name", txtName.Text).ConfigureAwait(false);
                    // <grade />
                    await objWriter.WriteElementStringAsync("grade", strGrade).ConfigureAwait(false);
                    // <cyberwares>
                    await objWriter.WriteStartElementAsync(_strType + 's').ConfigureAwait(false);

                    // Write out the Cyberware.
                    await _objCharacter.Cyberware.ForEachAsync(async objCyberware =>
                    {
                        if (objCyberware.SourceType != _eSource)
                            return;

                        // <cyberware>
                        // ReSharper disable AccessToDisposedClosure
                        await objWriter.WriteStartElementAsync(_strType).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("name", objCyberware.Name).ConfigureAwait(false);
                        int intRating = await objCyberware.GetRatingAsync().ConfigureAwait(false);
                        if (intRating > 0)
                            await objWriter
                                  .WriteElementStringAsync(
                                      "rating", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                                  .ConfigureAwait(false);
                        // Write out child items.
                        if (await objCyberware.Children.GetCountAsync().ConfigureAwait(false) > 0)
                        {
                            // <cyberwares>
                            await objWriter.WriteStartElementAsync(_strType + 's').ConfigureAwait(false);
                            await objCyberware.Children.ForEachAsync(async objChild =>
                            {
                                // Do not include items that come with the base item by default.
                                if (objChild.Capacity != "[*]")
                                {
                                    await objWriter.WriteStartElementAsync(_strType).ConfigureAwait(false);
                                    await objWriter.WriteElementStringAsync("name", objChild.Name)
                                                   .ConfigureAwait(false);
                                    int intChildRating = await objChild.GetRatingAsync().ConfigureAwait(false);
                                    if (intChildRating > 0)
                                        await objWriter
                                              .WriteElementStringAsync(
                                                  "rating",
                                                  intChildRating.ToString(GlobalSettings.InvariantCultureInfo))
                                              .ConfigureAwait(false);
                                    // </cyberware>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                }
                            }).ConfigureAwait(false);

                            // </cyberwares>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }

                        // </cyberware>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure
                    }).ConfigureAwait(false);

                    // </cyberwares>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    // </suite>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    // </chummer>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                }
            }

            Program.ShowScrollableMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CyberwareSuite_SuiteCreated").ConfigureAwait(false), strName),
                await LanguageManager.GetStringAsync("MessageTitle_CyberwareSuite_SuiteCreated").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
