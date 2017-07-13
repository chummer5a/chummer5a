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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectSetting : Form
    {
        private string _strSettingsFile = "default.xml";

        #region Control Events
        public frmSelectSetting()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void frmSelectSetting_Load(object sender, EventArgs e)
        {
            // Build the list of XML files found in the settings directory.
            List<ListItem> lstSettings = new List<ListItem>();
            string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
            foreach (string strFileName in Directory.GetFiles(settingsDirectoryPath, "*.xml"))
            {
                // Load the file so we can get the Setting name.
                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(strFileName);
                string strSettingsName = objXmlDocument.SelectSingleNode("/settings/name").InnerText;

                ListItem objItem = new ListItem();
                objItem.Value = Path.GetFileName(strFileName);
                objItem.Name = strSettingsName;

                lstSettings.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            lstSettings.Sort(objSort.Compare);
            cboSetting.BeginUpdate();
            cboSetting.ValueMember = "Value";
            cboSetting.DisplayMember = "Name";
            cboSetting.DataSource = lstSettings;
            cboSetting.EndUpdate();

            // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
            cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");
            if (cboSetting.SelectedIndex == -1)
                cboSetting.SelectedIndex = 0;
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
        #endregion

        #region Methods
        private void MoveControls()
        {
            cboSetting.Left = lblSetting.Left + lblSetting.Width + 6;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Settings file that was selected in the dialogue.
        /// </summary>
        public string SettingsFile
        {
            get
            {
                return _strSettingsFile;
            }
        }
        #endregion
    }
}