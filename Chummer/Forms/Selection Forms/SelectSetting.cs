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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectSetting : Form
    {
        private string _strSettingsFile = "default.xml";

        #region Control Events

        public SelectSetting()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void SelectSetting_Load(object sender, EventArgs e)
        {
            // Build the list of XML files found in the settings directory.
            string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSettings))
            {
                foreach (string strFileName in Directory.GetFiles(settingsDirectoryPath, "*.xml"))
                {
                    // Load the file so we can get the Setting name.
                    XPathDocument objXmlDocument;
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFileName, Encoding.UTF8, true))
                        using (XmlReader objXmlReader
                               = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                            objXmlDocument = new XPathDocument(objXmlReader);
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    catch (XmlException)
                    {
                        continue;
                    }

                    lstSettings.Add(new ListItem(Path.GetFileName(strFileName),
                                                 objXmlDocument.CreateNavigator().SelectSingleNode("/settings/name")
                                                               ?.Value ?? await LanguageManager.GetStringAsync("String_Unknown")));
                }

                lstSettings.Sort(CompareListItems.CompareNames);
                cboSetting.BeginUpdate();
                cboSetting.PopulateWithListItems(lstSettings);
                // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
                cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");
                if (cboSetting.SelectedIndex == -1)
                    cboSetting.SelectedIndex = 0;
                cboSetting.EndUpdate();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strSettingsFile = cboSetting.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Settings file that was selected in the dialogue.
        /// </summary>
        public string SettingsFile => _strSettingsFile;

        #endregion Properties
    }
}
