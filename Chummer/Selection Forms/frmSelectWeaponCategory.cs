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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectWeaponCategory : Form
    {
        private string _strSelectedCategory = string.Empty;
        private string _strForceCategory = string.Empty;

        public string WeaponType { get; set; }

        private readonly XmlDocument _objXmlDocument;

        #region Control Events
        public frmSelectWeaponCategory()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objXmlDocument = XmlManager.Load("weapons.xml");
        }

        private void frmSelectWeaponCategory_Load(object sender, EventArgs e)
        {
            // Build a list of Weapon Categories found in the Weapons file.
            List<ListItem> lstCategory = new List<ListItem>();
            using (XmlNodeList objXmlCategoryList = !string.IsNullOrEmpty(_strForceCategory)
                ? _objXmlDocument.SelectNodes("/chummer/categories/category[. = \"" + _strForceCategory + "\"]")
                : _objXmlDocument.SelectNodes("/chummer/categories/category"))
                if (objXmlCategoryList != null)
                    foreach (XmlNode objXmlCategory in objXmlCategoryList)
                    {
                        if (WeaponType != null && _strForceCategory != "Exotic Ranged Weapons")
                        {
                            string strType = objXmlCategory.Attributes?["type"]?.Value;
                            if (string.IsNullOrEmpty(strType) || strType != WeaponType)
                                continue;
                        }

                        string strInnerText = objXmlCategory.InnerText;
                        lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                    }

            // Add the Cyberware Category.
            if (/*string.IsNullOrEmpty(_strForceCategory) ||*/ _strForceCategory == "Cyberware")
            {
                lstCategory.Add(new ListItem("Cyberware", LanguageManager.GetString("String_Cyberware", GlobalOptions.Language)));
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
        public string SelectedCategory => _strSelectedCategory;

        /// <summary>
        /// Description to show in the window.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Restrict the list to only a single Category.
        /// </summary>
        public string OnlyCategory
        {
            set
            {
                _strForceCategory = value;
                if (value == "Cyberware")
                    _strForceCategory = "Cyberweapon";
            }
        }
        #endregion
    }
}
