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
    public partial class frmLifestyleNuyen : Form
    {
        readonly Character _objCharacter;

        #region Control Events
        public frmLifestyleNuyen(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
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
            lblDice.Text = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Label_LifestyleNuyen_ResultOf"), Dice.ToString(GlobalOptions.CultureInfo));
            nudDiceResult.Maximum = Dice * 6;
            nudDiceResult.Minimum = Dice;
            nudDiceResult_ValueChanged(sender, e);
        }

        private void nudDiceResult_ValueChanged(object sender, EventArgs e)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            lblResult.Text = strSpace + '+' + strSpace + Extra.ToString("#,0", GlobalOptions.CultureInfo) + ')' + strSpace + '×'
                             + strSpace + Multiplier.ToString(_objCharacter.Options.NuyenFormat + '¥', GlobalOptions.CultureInfo)
                             + strSpace + '=' + strSpace + StartingNuyen.ToString(_objCharacter.Options.NuyenFormat + '¥', GlobalOptions.CultureInfo);
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
        public decimal StartingNuyen => ((nudDiceResult.Value + Extra) * Multiplier);

        #endregion
    }
}
