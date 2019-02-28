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
    public partial class frmInitRoller : Form
    {
        #region Control Events
        public frmInitRoller()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
        }

        private void frmInitRoller_Load(object sender, EventArgs e)
        {
            lblDice.Text = string.Format(LanguageManager.GetString("Label_LifestyleNuyen_ResultOf"), Dice);
            nudDiceResult.Maximum = Dice * 6;
            nudDiceResult.Minimum = Dice;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Number of dice that are rolled for the lifestyle.
        /// </summary>
        public int Dice { get; set; }

        /// <summary>
        /// Dice roll result.
        /// </summary>
        public int Result => decimal.ToInt32(nudDiceResult.Value);
        #endregion
    }
}
