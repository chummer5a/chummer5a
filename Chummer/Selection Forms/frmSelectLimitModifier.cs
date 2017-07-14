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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectLimitModifier : Form
    {
        private string _strReturnName = string.Empty;
        private int _intBonus = 1;
        private string _strCondition = string.Empty;

        #region Control Events
        public frmSelectLimitModifier(LimitModifier objLimitModifier = null)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            if (objLimitModifier != null)
            {
                txtName.Text = objLimitModifier.Name;
                _intBonus = objLimitModifier.Bonus;
                txtCondition.Text = objLimitModifier.Condition;
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnName = txtName.Text;
            _intBonus = Convert.ToInt32(nudBonus.Value);
            _strCondition = txtCondition.Text;
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void frmSelectText_Shown(object sender, EventArgs e)
        {
            // If the field is pre-populated, immediately click OK.
            if (!string.IsNullOrEmpty(txtName.Text))
                cmdOK_Click(sender, e);
        }        
        #endregion

        #region Properties
        /// <summary>
        /// Modifier name that was entered in the dialogue.
        /// </summary>
        public string SelectedName
        {
            get
            {
                return _strReturnName;
            }
            set
            {
                txtName.Text = value;
            }
        }

        /// <summary>
        /// Modifier condition that was entered in the dialogue.
        /// </summary>
        public string SelectedCondition
        {
            get
            {
                return _strCondition;
            }
            set
            {
                txtCondition.Text = value;
            }
        }

        /// <summary>
        /// Modifier Bonus that was entered in the dialogue.
        /// </summary>
        public int SelectedBonus
        {
            get
            {
                return _intBonus;
            }
            set
            {
                nudBonus.Value = value;
            }
        }

        #endregion

    }
}
