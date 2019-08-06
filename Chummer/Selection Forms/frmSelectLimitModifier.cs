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
    public sealed partial class frmSelectLimitModifier : Form
    {
        private string _strReturnName = string.Empty;
        private int _intBonus = 1;
        private string _strCondition = string.Empty;
        private string _strLimitType = string.Empty;

        #region Control Events
        public frmSelectLimitModifier(LimitModifier objLimitModifier = null, params string[] lstLimits)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Instance.Language, this);

            // Build the list of Limits.
            List<ListItem> lstLimitItems = new List<ListItem>();
            foreach (string strLimit in lstLimits)
            {
                lstLimitItems.Add(new ListItem(strLimit, LanguageManager.GetString("String_Limit" + strLimit + "Short", GlobalOptions.Instance.Language)));
            }

            cboLimit.BeginUpdate();
            cboLimit.ValueMember = "Value";
            cboLimit.DisplayMember = "Name";
            cboLimit.DataSource = lstLimitItems;
            if (lstLimitItems.Count >= 1)
                cboLimit.SelectedIndex = 0;
            else
                cmdOK.Enabled = false;
            cboLimit.EndUpdate();

            if (objLimitModifier != null)
            {
                cboLimit.SelectedValue = objLimitModifier.Limit;
                txtName.Text = objLimitModifier.Name;
                _intBonus = objLimitModifier.Bonus;
                txtCondition.Text = objLimitModifier.Condition;
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (txtName.TextLength > 0)
            {
                string strLimitType = cboLimit.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strLimitType))
                {
                    _strReturnName = txtName.Text;
                    _intBonus = decimal.ToInt32(nudBonus.Value);
                    _strCondition = txtCondition.Text;
                    _strLimitType = cboLimit.SelectedValue?.ToString();
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Modifier name that was entered in the dialogue.
        /// </summary>
        public string SelectedName => _strReturnName;

        /// <summary>
        /// Modifier condition that was entered in the dialogue.
        /// </summary>
        public string SelectedCondition => _strCondition;

        /// <summary>
        /// Modifier Bonus that was entered in the dialogue.
        /// </summary>
        public int SelectedBonus => _intBonus;

        /// <summary>
        /// Modifier limit type that was entered in the dialogue.
        /// </summary>
        public string SelectedLimitType => _strLimitType;
        #endregion

        private void ToggleOKEnabled(object sender, EventArgs e)
        {
            cmdOK.Enabled = cboLimit.Items.Count > 0 && txtName.TextLength > 0 && !string.IsNullOrEmpty(cboLimit.SelectedValue?.ToString());
        }
    }
}
