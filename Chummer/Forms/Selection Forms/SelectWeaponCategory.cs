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
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectWeaponCategory : Form
    {
        private string _strSelectedCategory = string.Empty;
        private string _strForceCategory = string.Empty;

        public string WeaponType { get; set; }

        private readonly XPathNavigator _objXmlDocument;

        #region Control Events

        public SelectWeaponCategory(Character objCharacter)
        {
            _objXmlDocument =
                XmlManager.LoadXPath("weapons.xml", objCharacter?.Settings.EnabledCustomDataDirectoryPaths);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void SelectWeaponCategory_Load(object sender, EventArgs e)
        {
            // Build a list of Weapon Categories found in the Weapons file.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
            {
                foreach (XPathNavigator objXmlCategory in !string.IsNullOrEmpty(_strForceCategory)
                             ? _objXmlDocument.Select("/chummer/categories/category[. = "
                                                      + _strForceCategory.CleanXPath() + ']')
                             : _objXmlDocument.SelectAndCacheExpression("/chummer/categories/category"))
                {
                    if (!string.IsNullOrEmpty(WeaponType) && objXmlCategory.Value != "Exotic Ranged Weapons")
                    {
                        string strType = objXmlCategory.SelectSingleNodeAndCacheExpression("@type")?.Value;
                        if (string.IsNullOrEmpty(strType) || strType != WeaponType)
                            continue;
                    }

                    string strInnerText = objXmlCategory.Value;
                    lstCategory.Add(new ListItem(strInnerText,
                                                 objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                 ?? strInnerText));
                }

                // Add the Cyberware Category.
                if ( /*string.IsNullOrEmpty(_strForceCategory) ||*/ _strForceCategory == "Cyberware")
                {
                    lstCategory.Add(new ListItem("Cyberware", LanguageManager.GetString("String_Cyberware")));
                }

                switch (lstCategory.Count)
                {
                    case 0:
                        ConfirmSelection(string.Empty);
                        break;

                    case 1:
                        ConfirmSelection(lstCategory[0].Value.ToString());
                        break;
                }

                cboCategory.BeginUpdate();
                cboCategory.PopulateWithListItems(lstCategory);
                // Select the first Skill in the list.
                if (cboCategory.Items.Count > 0)
                    cboCategory.SelectedIndex = 0;
                cboCategory.EndUpdate();
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            ConfirmSelection(cboCategory.SelectedValue.ToString());
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        private void ConfirmSelection(string strSelection)
        {
            _strSelectedCategory = strSelection;
            DialogResult = DialogResult.OK;
            Close();
        }

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

        #endregion Properties
    }
}
