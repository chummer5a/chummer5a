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
    public partial class SmallSkillControl : UserControl
    {
        private Skill _objSkill;
        private Form _objParent;
        
        /// <summary>
        /// Fired when the user decides to roll the dice
        /// </summary>
        public event EventHandler DiceClick
        {
            add { this.cmdRoll.Click += value; }
            remove { this.cmdRoll.Click -= value; }
        }

        /// <summary>
        /// The skill information
        /// </summary>
        public Skill Skill 
        {
            get { return this._objSkill; }
            set
            {
                this._objSkill = value;
                this.lblSkillName.Text = this.Skill.Name + " : " + this.Skill.TotalRating.ToString();
            }
        }

        public bool ShowSpecialization
        {
            get 
            {
                return chkUseSpecial.Visible;
            }
            set 
            {
                chkUseSpecial.Visible = value;
                if (chkUseSpecial.Visible)
                    this.Width = chkUseSpecial.Left + chkUseSpecial.Width;
                else if (cmdRoll.Visible)
                    this.Width = cmdRoll.Left + cmdRoll.Width;
                else
                    this.Width = lblSkillName.Left + lblSkillName.Width;
            }
        }

        public bool ShowDiceRoller
        {
            get
            {
                return cmdRoll.Visible;
            }
            set
            {
                cmdRoll.Visible = value;
                if (chkUseSpecial.Visible)
                    this.Width = chkUseSpecial.Left + chkUseSpecial.Width;
                else if (cmdRoll.Visible)
                    this.Width = cmdRoll.Left + cmdRoll.Width;
                else
                    this.Width = lblSkillName.Left + lblSkillName.Width;
            }
        }

        public SmallSkillControl(Form parent)
        {
            this._objParent = parent;
            InitializeComponent();
            // setup controls
        }

        private void cmdRoll_Click(object sender, EventArgs e)
        {
            // pass the appropriate information onto the dice roller
            if (this._objParent is frmGMDashboard)
            {
                frmGMDashboard dash = this._objParent as frmGMDashboard;
                dash.DiceRoller.NumberOfDice = this.chkUseSpecial.Checked ? Skill.TotalRating + 2 : Skill.TotalRating;
                // apply appropriate limit here
                dash.DiceRoller.EdgeUse = DiceRollerControl.EdgeUses.None;
                dash.DiceRoller.NumberOfEdge = Convert.ToInt32(((Attribute)dash.CurrentNPC.EDG).TotalValue);
            }
            else
            {
                // we have the individual player's skill's
                frmPlayerDashboard dash = this._objParent as frmPlayerDashboard;
                dash.DiceRoller.NumberOfDice = this.chkUseSpecial.Checked ? Skill.TotalRating + 2 : Skill.TotalRating;
                dash.DiceRoller.EdgeUse = DiceRollerControl.EdgeUses.None;
                dash.DiceRoller.NumberOfEdge = Convert.ToInt32(dash.CurrentNPC.EDG);
            }
        }

        private void lblSkillName_Click(object sender, EventArgs e)
        {
            string strBook = _objSkill.Source + " " + _objSkill.Page;
            CommonFunctions objCommon = new CommonFunctions();
            objCommon.OpenPDF(strBook);
        }
    }
}
