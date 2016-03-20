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
using System.Threading;
using System.Windows.Forms;
using Chummer.Skills;

namespace Chummer
{
    public partial class frmGMDashboard : Form
    {
        private frmInitiative frmInitative;
        private enum DashBoardPages { CM, Skills, Vassels, Vehicles, Dice, TempBonus }

        #region Singleton

        private static frmGMDashboard _instance;
        /// <summary>
        /// The singleton instance of this object.
        /// </summary>
        public static frmGMDashboard Instance
        {
            get
            {
                if (frmGMDashboard._instance == null)
                    frmGMDashboard._instance = new frmGMDashboard();
                return frmGMDashboard._instance;
            }
        }

        protected frmGMDashboard()
        {
            InitializeComponent();
            this.UpdateTabs();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            this.frmInitative = new frmInitiative();
            this.frmInitative.Hide();
            this.VisibleChanged += frmGMDashboard_VisibleChanged;
            this.frmInitative.InitUC.CurrentCharacterChanged += InitUC_CurrentCharacterChanged;
            this.frmInitative.FormClosing += frmInitative_FormClosing;
            this.FormClosing += frmGMDashboard_FormClosing;
            this.frmInitative.TopMost = true;
            this.CenterToParent();
            // auto hide the form at creation
            this.Hide();
        }

        #endregion

        #region Properties
        /// <summary>
        /// The current NPC that the GM is controlling
        /// </summary>
        public Character CurrentNPC
        {
            get;
            set;
        }

        /// <summary>
        /// The dice roller for applying skill checks
        /// </summary>
        public DiceRollerControl DiceRoller
        {
            get { return this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl; }
        }
        #endregion

        #region Events

        /*
         * When the user attempts to close the main Dashboard, hide 
         * it instead
         */
        void frmGMDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.exitToolStripMenuItem_Click(sender, null);
            e.Cancel = true;
        }

        /*
         * When the user is attempting to close the initiative control,
         * simply "hide" it.
         */
        void frmInitative_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.frmInitative.Hide();
            e.Cancel = true;
        }

        /*
         * User closed the initiative window so "show" it
         */
        private void initativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.frmInitative.Show();
        }

        /*
         * Exits the program
         */
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.frmInitative.Hide();
            this.Hide();
        }

        /// <summary>
        /// When the visibility has been changed to "visible" need to show the init form as well
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmGMDashboard_VisibleChanged(object sender, EventArgs e)
        {
            this.frmInitative.Visible = this.Visible;
        }

        /// <summary>
        /// When the current character has been changed, we need to change all values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InitUC_CurrentCharacterChanged(object sender, EventArgs e)
        {
            this.CurrentNPC = this.frmInitative.InitUC.CurrentCharacter;

            this.UpdateControls();
        }

        /// <summary>
        /// When some form wants the dice roller to be shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DiceClick_Clicked(object sender, EventArgs e)
        {
            this.tabControl.SelectedIndex = (int)DashBoardPages.Dice;
        }
        #endregion

        #region Private helper methods

        /*
         * Updates the tabs 
         */
        private void UpdateTabs()
        {
            this.tabControl.TabPages.Add(DashBoardPages.CM.ToString());
            this.tabControl.TabPages.Add(DashBoardPages.Skills.ToString());
            this.tabControl.TabPages.Add(DashBoardPages.Vassels.ToString());
            this.tabControl.TabPages.Add(DashBoardPages.Vehicles.ToString());
            this.tabControl.TabPages.Add(DashBoardPages.Dice.ToString());
            this.tabControl.TabPages.Add(DashBoardPages.TempBonus.ToString());

            // setup the controls for each tab
            this.tabControl.TabPages[(int)DashBoardPages.CM].Controls.Add(new ConditionMonitorUserControl());
            this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls.Add(new DiceRollerControl());
        }

        private void UpdateControls()
        {
            // tosses the character information relevant to each character
        #region Condition Monitor
            ConditionMonitorUserControl uc = 
                this.tabControl.TabPages[(int)DashBoardPages.CM].Controls[0] as ConditionMonitorUserControl;
            uc.MaxPhysical = this.CurrentNPC.PhysicalCM;
            uc.MaxStun = this.CurrentNPC.StunCM;
            uc.Physical = uc.MaxPhysical;
            uc.Stun = uc.MaxStun;
        #endregion

        #region Skill tab
            //TODO fix this
        #endregion

        #region Dice Roller
            DiceRollerControl dice = 
                this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;
            //dice.NumberOfEdge = this.CurrentNPC.EDG;    // todo figure out number of edge dice
        #endregion
        }
        #endregion

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO
        }
    }
}
