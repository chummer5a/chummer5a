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
    public partial class frmGMDashboard : Form
    {
        private readonly frmInitiative frmInitative;
        private enum DashBoardPages { CM, Skills, Vassels, Vehicles, Dice, TempBonus }

        #region Singleton

        private static frmGMDashboard _instance;
        /// <summary>
        /// The singleton instance of this object.
        /// </summary>
        public static frmGMDashboard Instance => _instance ?? (_instance = new frmGMDashboard());

        protected frmGMDashboard()
        {
            InitializeComponent();
            UpdateTabs();
            this.TranslateWinForm();
            frmInitative = new frmInitiative();
            frmInitative.Hide();
            VisibleChanged += frmGMDashboard_VisibleChanged;
            frmInitative.InitUC.CurrentCharacterChanged += InitUC_CurrentCharacterChanged;
            frmInitative.FormClosing += frmInitative_FormClosing;
            FormClosing += frmGMDashboard_FormClosing;
            frmInitative.TopMost = true;
            CenterToParent();
            // auto hide the form at creation
            Hide();
        }

        #endregion Singleton

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
        public DiceRollerControl DiceRoller => tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;

        #endregion Properties

        #region Events

        /*
         * When the user attempts to close the main Dashboard, hide
         * it instead
         */
        void frmGMDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            exitToolStripMenuItem_Click(sender, null);
            e.Cancel = true;
        }

        /*
         * When the user is attempting to close the initiative control,
         * simply "hide" it.
         */
        void frmInitative_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmInitative.Hide();
            e.Cancel = true;
        }

        /*
         * User closed the initiative window so "show" it
         */
        private void initativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInitative.Show();
        }

        /*
         * Exits the program
         */
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInitative.Hide();
            Hide();
        }

        /// <summary>
        /// When the visibility has been changed to "visible" need to show the init form as well
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmGMDashboard_VisibleChanged(object sender, EventArgs e)
        {
            frmInitative.Visible = Visible;
        }

        /// <summary>
        /// When the current character has been changed, we need to change all values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InitUC_CurrentCharacterChanged(object sender, EventArgs e)
        {
            CurrentNPC = frmInitative.InitUC.CurrentCharacter;

            UpdateControls();
        }

        /// <summary>
        /// When some form wants the dice roller to be shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DiceClick_Clicked(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = (int)DashBoardPages.Dice;
        }

        #endregion Events

        #region Private helper methods

        /*
         * Updates the tabs
         */
        private void UpdateTabs()
        {
            tabControl.TabPages.Add(DashBoardPages.CM.ToString());
            tabControl.TabPages.Add(DashBoardPages.Skills.ToString());
            tabControl.TabPages.Add(DashBoardPages.Vassels.ToString());
            tabControl.TabPages.Add(DashBoardPages.Vehicles.ToString());
            tabControl.TabPages.Add(DashBoardPages.Dice.ToString());
            tabControl.TabPages.Add(DashBoardPages.TempBonus.ToString());

            // setup the controls for each tab
            tabControl.TabPages[(int)DashBoardPages.CM].Controls.Add(new ConditionMonitorUserControl());
            tabControl.TabPages[(int)DashBoardPages.Dice].Controls.Add(new DiceRollerControl());
        }

        private void UpdateControls()
        {
            // tosses the character information relevant to each character

            #region Condition Monitor

            if (tabControl.TabPages[(int)DashBoardPages.CM].Controls[0] is ConditionMonitorUserControl uc)
            {
                uc.MaxPhysical = CurrentNPC.PhysicalCM;
                uc.MaxStun = CurrentNPC.StunCM;
                uc.Physical = uc.MaxPhysical;
                uc.Stun = uc.MaxStun;
            }

            #endregion Condition Monitor

            #region Skill tab

            //TODO fix this

            #endregion Skill tab

            #region Dice Roller

            /*
            DiceRollerControl dice =
                tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;
            dice.NumberOfEdge = this.CurrentNPC.EDG;    // todo figure out number of edge dice
            */

            #endregion Dice Roller
        }

        #endregion Private helper methods

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO
        }
    }
}
