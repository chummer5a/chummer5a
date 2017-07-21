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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectWeaponCategory : Form
    {
        private string _strSelectedCategory = string.Empty;
        private string _strForceCategory = string.Empty;

        public string WeaponType { get; set; }

        private XmlDocument _objXmlDocument = new XmlDocument();

        #region Control Events
        public frmSelectWeaponCategory()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void frmSelectWeaponCategory_Load(object sender, EventArgs e)
        {
            _objXmlDocument = XmlManager.Instance.Load("weapons.xml");

            // Build a list of Weapon Categories found in the Weapons file.
            XmlNodeList objXmlCategoryList;
            List<ListItem> lstCategory = new List<ListItem>();
            if (!string.IsNullOrEmpty(_strForceCategory))
            {
                objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category[. = \"" + _strForceCategory + "\"]");
            }
            else
            {
                objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            }

            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                if (WeaponType != null)
                {
                    if (objXmlCategory.Attributes["type"] == null)
                        continue;

                    if (objXmlCategory.Attributes["type"].Value != WeaponType)
                        continue;
                }

                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes["translate"]?.InnerText ?? objXmlCategory.InnerText;
                lstCategory.Add(objItem);
            }

            // Add the Cyberware Category.
            if (/*string.IsNullOrEmpty(_strForceCategory) ||*/ _strForceCategory == "Cyberware")
            {
                ListItem objItem = new ListItem();
                objItem.Value = "Cyberware";
                objItem.Name = "Cyberware";
                lstCategory.Add(objItem);
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstCategory;

            // Select the first Skill in the list.
            if (cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            if (cboCategory.Items.Count == 1)
                cmdOK_Click(sender, e);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strSelectedCategory = cboCategory.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapon Category that was selected in the dialogue.
        /// </summary>
        public string SelectedCategory
        {
            get
            {
                return _strSelectedCategory;
            }
        }

        /// <summary>
        /// Description to show in the window.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        /// <summary>
        /// Restrict the list to only a single Category.
        /// </summary>
        public string OnlyCategory
        {
            set
            {
                _strForceCategory = value;
            }
        }
        #endregion
    }
}
