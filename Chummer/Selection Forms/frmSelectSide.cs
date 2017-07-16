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

namespace Chummer
{
    public partial class frmSelectSide : Form
    {
        private string _strSelectedSide = string.Empty;

        #region Control Events
        public frmSelectSide()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            // Create a list for the sides.
            List<ListItem> lstSides = new List<ListItem>();
            ListItem objLeft = new ListItem();
            objLeft.Value = "Left";
            objLeft.Name = LanguageManager.Instance.GetString("String_Improvement_SideLeft");

            ListItem objRight = new ListItem();
            objRight.Value = "Right";
            objRight.Name = LanguageManager.Instance.GetString("String_Improvement_SideRight");

            lstSides.Add(objLeft);
            lstSides.Add(objRight);

            cboSide.BeginUpdate();
            cboSide.ValueMember = "Value";
            cboSide.DisplayMember = "Name";
            cboSide.DataSource = lstSides;
            cboSide.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strSelectedSide = cboSide.Text;
            DialogResult = DialogResult.OK;
        }

        private void frmSelectSide_Load(object sender, EventArgs e)
        {
            // Select the first item in the list.
            cboSide.SelectedIndex = 0;
        }
        #endregion

        #region Properties
        // Description to show in the window.
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        /// <summary>
        /// Side that was selected in the dialogue.
        /// </summary>
        public string SelectedSide
        {
            get
            {
                return _strSelectedSide;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Force a particular value to be selected in the window.
        /// </summary>
        /// <param name="strSide">Value to force.</param>
        public void ForceValue(string strSide)
        {
            cboSide.SelectedValue = strSide;
            cboSide.Text = strSide;
            cmdOK_Click(this, null);
        }
        #endregion
    }
}