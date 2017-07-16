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
﻿using System.Linq;
﻿using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectText : Form
    {
        private string _strReturnValue = string.Empty;

        #region Control Events
        public frmSelectText()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (PreventXPathErrors && txtValue.Text.Contains('"'))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_InvalidCharacters"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                _strReturnValue = txtValue.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void frmSelectText_Shown(object sender, EventArgs e)
        {
            if (DefaultString != null)
            {
                txtValue.Text = DefaultString;
            }
        }        
        #endregion

        #region Properties
        /// <summary>
        /// Value that was entered in the dialogue.
        /// </summary>
        public string SelectedValue
        {
            get
            {
                return _strReturnValue;
            }
            set
            {
                txtValue.Text = value;
            }
        }

        /// <summary>
        /// Description to display in the dialogue.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        public bool PreventXPathErrors { get; internal set; }
        public string DefaultString { get; internal set; }
        #endregion
    }
}