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
    public partial class frmSelectLimit : Form
    {
        private string _strReturnValue = string.Empty;
        private string _strSelectedDisplayLimit = string.Empty;

        #region Control Events
        public frmSelectLimit(params string[] lstLimits)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            // Build the list of Limits.
            List<ListItem> lstLimitItems = new List<ListItem>();
            foreach (string strLimit in lstLimits)
            {
                lstLimitItems.Add(new ListItem(strLimit, LanguageManager.GetString("String_Limit" + strLimit + "Short", GlobalOptions.Language)));
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
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            string strSelectedLimit = cboLimit.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedLimit))
            {
                _strReturnValue = strSelectedLimit;
                _strSelectedDisplayLimit = ((ListItem)cboLimit.SelectedItem).Name;
                DialogResult = DialogResult.OK;
            }
        }

        private void frmSelectLimit_Load(object sender, EventArgs e)
        {
            if (cboLimit.Items.Count == 1)
            {
                cmdOK_Click(sender, e);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Limit that was selected in the dialogue.
        /// </summary>
        public string SelectedLimit => _strReturnValue;

        /// <summary>
        /// Limit that was selected in the dialogue.
        /// </summary>
        public string SelectedDisplayLimit => _strSelectedDisplayLimit;

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
