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
    public partial class frmInitRoller : Form
    {
        private int dice = 0;

        #region Control Events
        public frmInitRoller()
        {
            InitializeComponent();
            //LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            CenterToParent();
        }

        private void frmInitRoller_Load(object sender, EventArgs e)
        {
            lblDice.Text += " " + dice.ToString() + "D6: ";
            nudDiceResult.Maximum = dice * 6;
            nudDiceResult.Minimum = dice;
            MoveControls();
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
        public int Dice
        {
            get
            {
                return dice;
            }
            set
            {
                dice = value;
            }
        }

        /// <summary>
        /// Window title.
        /// </summary>
        public string Title
        {
            set
            {
                Text = value;
            }
        }

        /// <summary>
        /// Description text.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        /// <summary>
        /// Dice roll result.
        /// </summary>
        public int Result
        {
            get
            {
                return Convert.ToInt32(nudDiceResult.Value);
            }
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            nudDiceResult.Left = lblDice.Left + lblDice.Width + 6;
            lblResult.Left = nudDiceResult.Left + nudDiceResult.Width + 6;
        }
        #endregion
    }
}
