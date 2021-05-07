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
 using System.Linq;
 using System.Windows.Forms;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmLifestyleNuyen : Form
    {
        readonly Character _objCharacter;
        private Lifestyle _objLifestyle;

        #region Control Events
        public frmLifestyleNuyen(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        public frmLifestyleNuyen()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void frmLifestyleNuyen_Load(object sender, EventArgs e)
        {
            RefreshCboSelectLifestyle();
            RefreshCalculation(sender, e);
        }

        private void nudDiceResult_ValueChanged(object sender, EventArgs e)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            lblResult.Text = strSpace + '+' + strSpace + Extra.ToString("#,0", GlobalOptions.CultureInfo) + ')' + strSpace + '×'
                             + strSpace + SelectedLifestyle.Multiplier.ToString(_objCharacter.Options.NuyenFormat + '¥', GlobalOptions.CultureInfo)
                             + strSpace + '=' + strSpace + StartingNuyen.ToString(_objCharacter.Options.NuyenFormat + '¥', GlobalOptions.CultureInfo);
        }

        private void cboSelectLifestyle_SelectionChanged(object sender, EventArgs e)
        {
            _objLifestyle = (Lifestyle) cboSelectLifestyle.SelectedItem;
            RefreshCalculation(sender, e);
            lblDice.Text = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Label_LifestyleNuyen_ResultOf"), SelectedLifestyle.Dice.ToString(GlobalOptions.CultureInfo));
        }

        private void RefreshCboSelectLifestyle()
        {
            List<Lifestyle> lstLifestyles = _objCharacter.Lifestyles.ToList();

            lstLifestyles.Sort((y,x) => x.ExpectedValue.CompareTo(y.ExpectedValue));

            cboSelectLifestyle.ValueMember = nameof(ListItem.Value);
            cboSelectLifestyle.DisplayMember = nameof(ListItem.Name);
            cboSelectLifestyle.DataSource = lstLifestyles;
        }

        private void RefreshCalculation(object sender, EventArgs e)
        {
            nudDiceResult.Maximum = SelectedLifestyle.Dice * 6;
            nudDiceResult.Minimum = SelectedLifestyle.Dice;
            nudDiceResult_ValueChanged(sender, e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Number of dice that are rolled for the lifestyle.
        /// </summary>
        public int Dice { get; set; }

        /// <summary>
        /// Extra number that is added to the dice roll.
        /// </summary>
        public decimal Extra { get; set; }

        /// <summary>
        /// D6 multiplier for the Lifestyle.
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// The total amount of Nuyen resulting from the dice roll.
        /// </summary>
        public decimal StartingNuyen => ((nudDiceResult.Value + Extra) * SelectedLifestyle.Multiplier);

        /// <summary>
        /// The currently selected lifestyl
        /// </summary>
        public Lifestyle SelectedLifestyle => _objLifestyle;

        #endregion
    }
}
