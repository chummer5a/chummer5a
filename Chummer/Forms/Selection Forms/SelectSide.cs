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

namespace Chummer
{
    public partial class SelectSide : Form
    {
        private string _strSelectedSide = string.Empty;

        #region Control Events

        public SelectSide()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strSelectedSide = cboSide.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private async void SelectSide_Load(object sender, EventArgs e)
        {
            // Create a list for the sides.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSides))
            {
                lstSides.Add(new ListItem("Left", await LanguageManager.GetStringAsync("String_Improvement_SideLeft")));
                lstSides.Add(new ListItem("Right", await LanguageManager.GetStringAsync("String_Improvement_SideRight")));
                lstSides.Sort(CompareListItems.CompareNames);

                cboSide.BeginUpdate();
                cboSide.PopulateWithListItems(lstSides);
                cboSide.EndUpdate();
            }

            // Select the first item in the list.
            cboSide.SelectedIndex = 0;
        }

        #endregion Control Events

        #region Properties

        // Description to show in the window.
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Side that was selected in the dialogue.
        /// </summary>
        public string SelectedSide => _strSelectedSide;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Force a particular value to be selected in the window.
        /// </summary>
        /// <param name="strSide">Value to force.</param>
        public void ForceValue(string strSide)
        {
            _strSelectedSide = strSide;
            DialogResult = DialogResult.OK;
        }

        #endregion Methods

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
