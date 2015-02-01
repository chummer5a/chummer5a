using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
        public static frmPlayerDashboard Instance
        {
            get
            {
                if (frmPlayerDashboard._instance == null)
                    frmPlayerDashboard._instance = new frmPlayerDashboard();
                return frmPlayerDashboard._instance;
            }
        }

        protected frmPlayerDashboard()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
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

        public DiceRollerControl DiceRoller
        {
            get { return this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl; }
        }
        #endregion

        #region Events
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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

            #region Dice Roller
            DiceRollerControl dice =
                this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;
            //dice.NumberOfEdge = this.CurrentNPC.EDG;    // todo figure out number of edge dice
            #endregion
        }
        #endregion
    }
}
