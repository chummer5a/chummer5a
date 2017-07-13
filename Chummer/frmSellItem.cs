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
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSellItem : Form
    {
        #region Control Events
        public frmSellItem()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            nudPercent.Left = lblSellForLabel.Left + lblSellForLabel.Width + 6;
            lblPercentLabel.Left = nudPercent.Left + nudPercent.Width + 6;
            Width = lblPercentLabel.Left + lblPercentLabel.Width + 19;
            if (Width < 185)
                Width = 185;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The percentage the item will be sold at.
        /// </summary>
        public double SellPercent
        {
            get
            {
                return Convert.ToDouble(nudPercent.Value / 100, GlobalOptions.CultureInfo);
            }
        }
        #endregion
    }
}