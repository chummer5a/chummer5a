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
    public partial class frmPlayerDashboard : Form
    {
        private enum DashBoardPages { CM, Skills, Vassels, Vehicles, Dice }

        #region Singleton

        private static frmPlayerDashboard _instance;
        /// <summary>
        /// The singleton instance of this object.
        /// </summary>
        public static frmPlayerDashboard Instance => _instance ?? (_instance = new frmPlayerDashboard());

        protected frmPlayerDashboard()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            CenterToParent();
            // auto hide the form at creation
            Hide();
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

        public DiceRollerControl DiceRoller => tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;

        #endregion

        #region Events
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

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

            // setup the controls for each tab
            tabControl.TabPages[(int)DashBoardPages.CM].Controls.Add(new ConditionMonitorUserControl());
            tabControl.TabPages[(int)DashBoardPages.Dice].Controls.Add(new DiceRollerControl());
        }

        private void UpdateControls()
        {
            // tosses the character information relevant to each character
            #region Condition Monitor

            if (tabControl.TabPages[(int) DashBoardPages.CM].Controls[0] is ConditionMonitorUserControl uc)
            {
                uc.MaxPhysical = CurrentNPC.PhysicalCM;
                uc.MaxStun = CurrentNPC.StunCM;
                uc.Physical = uc.MaxPhysical;
                uc.Stun = uc.MaxStun;
            }

            #endregion

            #region Dice Roller
            /*
            DiceRollerControl dice =
                tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;
            dice.NumberOfEdge = this.CurrentNPC.EDG;    // todo figure out number of edge dice
            */
            #endregion
        }
        #endregion
    }
}
