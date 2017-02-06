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
            get { return _progressBarPhysical.Maximum; }
            set { _progressBarPhysical.Maximum = value; }
        }

        /// <summary>
        /// The stun chart of the character
        /// </summary>
        public int MaxStun
        {
            get { return _progressBarStun.Maximum; }
            set { _progressBarStun.Maximum = value; }
        }

        /// <summary>
        /// The physical health of the current npc
        /// </summary>
        public int Physical 
        {
            get{ return _progressBarPhysical.Value; }
            set { _progressBarPhysical.Value = value; }
        }

        /// <summary>
        /// The stun health of the current npc
        /// </summary>
        public int Stun 
        {
            get { return _progressBarStun.Value; }
            set { _progressBarStun.Value = value; }
        }
        #endregion

        public ConditionMonitorUserControl()
        {
            InitializeComponent();
        }

        private void _btnPhysical_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(_nudPhysical.Value);
            if (val + _progressBarPhysical.Value < 0)
                _progressBarPhysical.Value = 0;
            else if (val + _progressBarPhysical.Value > _progressBarPhysical.Maximum)
                _progressBarPhysical.Value = _progressBarPhysical.Maximum;
            else
                _progressBarPhysical.Value += val > _progressBarPhysical.Maximum ?
                    _progressBarPhysical.Maximum : val;
        }

        private void _btnApplyStun_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(nudStun.Value);
            if (val + _progressBarStun.Value < 0)
                _progressBarStun.Value = 0;
            else if (val + _progressBarStun.Value > _progressBarStun.Value)
                _progressBarStun.Value = _progressBarStun.Maximum;
            else
                _progressBarStun.Value += val > _progressBarStun.Maximum ?
                    _progressBarStun.Maximum : val;
        }

    }
}
