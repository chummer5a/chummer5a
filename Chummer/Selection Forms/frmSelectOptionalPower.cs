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
﻿using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectOptionalPower : Form
    {
        private string _strReturnPower = string.Empty;
        private string _strReturnExtra = string.Empty;
        private readonly List<ListItem> _lstPowerItems = new List<ListItem>();

        #region Control Events
        public frmSelectOptionalPower(params Tuple<string, string>[] lstPowerExtraPairs)
        {
            InitializeComponent();
            this.TranslateWinForm();

            foreach (Tuple<string, string> lstObject in lstPowerExtraPairs)
            {
                string strName = LanguageManager.TranslateExtra(lstObject.Item1);
                if (!string.IsNullOrEmpty(lstObject.Item2))
                {
                    strName += LanguageManager.GetString("String_Space") + '(' + LanguageManager.TranslateExtra(lstObject.Item2) + ')';
                }
                _lstPowerItems.Add(new ListItem(lstObject, strName));
            }
            cboPower.BeginUpdate();
            cboPower.ValueMember = "Value";
            cboPower.DisplayMember = "Name";
            cboPower.DataSource = _lstPowerItems;
            if (_lstPowerItems.Count >= 1)
                cboPower.SelectedIndex = 0;
            else
                cmdOK.Enabled = false;
            cboPower.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (cboPower.SelectedValue is Tuple<string, string> objSelectedItem)
            {
                _strReturnPower = objSelectedItem.Item1;
                _strReturnExtra = objSelectedItem.Item2;
                DialogResult = DialogResult.OK;
            }
        }

        private void frmSelectOptionalPower_Load(object sender, EventArgs e)
        {
            if (_lstPowerItems.Count == 1)
                cmdOK_Click(sender, e);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower => _strReturnPower;

        public string SelectedPowerExtra => _strReturnExtra;

        /// <summary>
        /// Description to display on the form.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }
        #endregion
    }
}
