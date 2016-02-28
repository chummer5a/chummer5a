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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class ConditionMonitorUserControl : UserControl
    {
        #region Properties

        /// <summary>
        /// The HP of the character
        /// </summary>
        public int MaxPhysical
        {
            get { return this._progressBarPhysical.Maximum; }
            set { this._progressBarPhysical.Maximum = value; }
        }

        /// <summary>
        /// The stun chart of the character
        /// </summary>
        public int MaxStun
        {
            get { return this._progressBarStun.Maximum; }
            set { this._progressBarStun.Maximum = value; }
        }

        /// <summary>
        /// The physical health of the current npc
        /// </summary>
        public int Physical 
        {
            get{ return this._progressBarPhysical.Value; }
            set { this._progressBarPhysical.Value = value; }
        }

        /// <summary>
        /// The stun health of the current npc
        /// </summary>
        public int Stun 
        {
            get { return this._progressBarStun.Value; }
            set { this._progressBarStun.Value = value; }
        }
        #endregion

        public ConditionMonitorUserControl()
        {
            InitializeComponent();
        }

        private void _btnPhysical_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(this._nudPhysical.Value);
            if (val + this._progressBarPhysical.Value < 0)
                this._progressBarPhysical.Value = 0;
            else if (val + this._progressBarPhysical.Value > this._progressBarPhysical.Maximum)
                this._progressBarPhysical.Value = this._progressBarPhysical.Maximum;
            else
                this._progressBarPhysical.Value += val > this._progressBarPhysical.Maximum ?
                    this._progressBarPhysical.Maximum : val;
        }

        private void _btnApplyStun_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(this.nudStun.Value);
            if (val + this._progressBarStun.Value < 0)
                this._progressBarStun.Value = 0;
            else if (val + this._progressBarStun.Value > this._progressBarStun.Value)
                this._progressBarStun.Value = this._progressBarStun.Maximum;
            else
                this._progressBarStun.Value += val > this._progressBarStun.Maximum ?
                    this._progressBarStun.Maximum : val;
        }

    }
}
