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
    public partial class SelectSpellCategory : Form
    {
        private string _strSelectedCategory = string.Empty;
        private string _strForceCategory    = string.Empty;
        private readonly HashSet<string> _setExcludeCategories = Utils.StringHashSetPool.Get();

        private readonly XPathNavigator _objXmlDocument;

        #region Control Events

        public SelectSpellCategory(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = XmlManager.LoadXPath("spells.xml", objCharacter?.Settings.EnabledCustomDataDirectoryPaths);
        }

        private async void SelectSpellCategory_Load(object sender, EventArgs e)
        {
            // Build the list of Spell Categories from the Spells file.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
            {
                foreach (XPathNavigator objXmlCategory in !string.IsNullOrEmpty(_strForceCategory)
                             ? _objXmlDocument.Select("/chummer/categories/category[. = "
                                                      + _strForceCategory.CleanXPath() + ']')
                             : await _objXmlDocument.SelectAndCacheExpressionAsync("/chummer/categories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    if (_setExcludeCategories.Contains(strInnerText))
                        continue;
                    lstCategory.Add(new ListItem(strInnerText,
                                                 (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value
                                                 ?? strInnerText));
                }
                
                await cboCategory.PopulateWithListItemsAsync(lstCategory);
                // Select the first Skill in the list.
                cboCategory.SelectedIndex = 0;
            }

            if (cboCategory.Items.Count == 1)
                cmdOK_Click(sender, e);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strSelectedCategory = cboCategory.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        #endregion Control Events

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
            set => _strForceCategory = value;
        }

        #endregion Properties

        /// <summary>
        /// Exclude a Category from the list.
        /// </summary>
        public void SetExcludeCategories(IEnumerable<string> lstCategories)
        {
            _setExcludeCategories.Clear();
            if (lstCategories == null)
                return;
            foreach (string strCategory in lstCategories)
                _setExcludeCategories.Add(strCategory);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
