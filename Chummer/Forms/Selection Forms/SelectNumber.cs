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
using System.Windows.Forms;

namespace Chummer
{
    public partial class SelectNumber : Form
    {
        private decimal _decReturnValue;

        #region Control Events

        public SelectNumber(int intDecimalPlaces = 2)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            nudNumber.DecimalPlaces = intDecimalPlaces;
        }

        private void SelectNumber_Shown(object sender, EventArgs e)
        {
            nudNumber.Focus();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _decReturnValue = nudNumber.Value;
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Value that was entered in the dialogue.
        /// </summary>
        public decimal SelectedValue
        {
            get => _decReturnValue;
            set => nudNumber.Value = value;
        }

        /// <summary>
        /// Minimum number.
        /// </summary>
        public decimal Minimum
        {
            set => nudNumber.Minimum = value;
        }

        /// <summary>
        /// Maximum number.
        /// </summary>
        public decimal Maximum
        {
            set => nudNumber.Maximum = value;
        }

        /// <summary>
        /// Description to display in the dialogue.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Whether or not the Cancel button is enabled.
        /// </summary>
        public bool AllowCancel
        {
            set
            {
                cmdCancel.Enabled = value;
                if (!value)
                    ControlBox = false;
            }
        }

        #endregion Properties
    }
}
