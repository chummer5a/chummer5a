using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmGMDashboard : Form
    {
        private frmInitiative frmInitative;
        private enum DashBoardPages { CM, Skills, Vassels, Vehicles, Dice, TempBonus }

        private ConditionMonitorUserControl _conditionMonitorUserControl;

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
            this.CenterToParent();
            this.Location = new Point(this.Location.X - Width/2, Location.Y);
            this.frmInitative.Location = new Point(this.Location.X + Width, Location.Y);
                     

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
            this.CurrentNPC = this.frmInitative.InitUC.SelectedCharacter;

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
            this.tabControl.TabPages.Add(DashBoardPages.Vassels.ToString()); //todo: find out what Vassels are...
            this.tabControl.TabPages.Add(DashBoardPages.Vehicles.ToString()); //todo implement vehicles
            this.tabControl.TabPages.Add(DashBoardPages.Dice.ToString());
            this.tabControl.TabPages.Add(DashBoardPages.TempBonus.ToString()); //todo implement temp boni

            // setup the controls for each tab
            _conditionMonitorUserControl = new ConditionMonitorUserControl();
            _conditionMonitorUserControl.CurrentCharacterDamageApplied += _conditionMonitorUserControl_CurrentCharacterDamageApplied; ;
            this.tabControl.TabPages[(int)DashBoardPages.CM].Controls.Add(_conditionMonitorUserControl);
            this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls.Add(new DiceRollerControl());
            this.tabControl.TabPages[(int) DashBoardPages.Vassels].Enabled = false;
            this.tabControl.TabPages[(int)DashBoardPages.Vehicles].Enabled = false;
            this.tabControl.TabPages[(int)DashBoardPages.TempBonus].Enabled = false;

        }

        private void _conditionMonitorUserControl_CurrentCharacterDamageApplied(object sender, EventArgs e)
        {
            
            if (CurrentNPC == null)
            {
                // if the there is no selected character in the initiative control
                return;
            }
            // apply the damagevalues to the current selected char
            CurrentNPC.PhysicalCMFilled = _conditionMonitorUserControl.Physical;
            CurrentNPC.StunCMFilled = _conditionMonitorUserControl.Stun;
            // safe old damagemodifier for applying init changes
            var oldModifier= CurrentNPC.DamageInitModifier;
            // set new Damagemodifier for current selected char
            CurrentNPC.DamageInitModifier = CalculateDamageModifier();
            _conditionMonitorUserControl.Modifier = CurrentNPC.DamageInitModifier;
            // apply initchanges 
            frmInitative.InitUC.ApplyDamage(CurrentNPC,oldModifier);

        }

        private int CalculateDamageModifier()
        {
            var physDamage = CurrentNPC.PhysicalCMFilled;
            var stunDamage = CurrentNPC.StunCMFilled;

            ImprovementManager man = new ImprovementManager(CurrentNPC);
            var ignoredPhys=  man.ValueOf(Improvement.ImprovementType.IgnoreCMPenaltyPhysical);
            var ignoredStun = man.ValueOf(Improvement.ImprovementType.IgnoreCMPenaltyStun);
            //todo Add other types of improvements (get a list of damage modifier changing improvements) 
            var returnValue = 0;
            returnValue += physDamage > ignoredPhys ? physDamage/3 : 0;
            returnValue += stunDamage > ignoredStun ? stunDamage/3 : 0;

            return -returnValue;
        }

        private void UpdateControls()
        {
            if (CurrentNPC == null) return;
            // tosses the character information relevant to each character
        #region Condition Monitor
            ConditionMonitorUserControl uc = 
                this.tabControl.TabPages[(int)DashBoardPages.CM].Controls[0] as ConditionMonitorUserControl;
            uc.MaxPhysical = this.CurrentNPC.PhysicalCM;
            uc.MaxStun = this.CurrentNPC.StunCM;
            uc.Physical = CurrentNPC.PhysicalCMFilled;
            uc.Stun = CurrentNPC.StunCMFilled;
            uc.Modifier = CurrentNPC.DamageInitModifier;
            #endregion

        #region Skill tab
            this.tabControl.TabPages[(int)DashBoardPages.Skills].Controls.Clear();
            FlowLayoutPanel panel = new FlowLayoutPanel();
            foreach (Skill skill in this.CurrentNPC.Skills.Where(s=>s.Rating>0))
            {
                if (skill.KnowledgeSkill)
                    continue;   // improvement for knowledge skills goes here
                // insert new skill control
                SmallSkillControl ucSkill = new SmallSkillControl(this);
                ucSkill.Skill = skill;
                ucSkill.DiceClick += DiceClick_Clicked;
                // add the skill to the collection of skills to show
                panel.Controls.Add(ucSkill);
            }
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.MouseEnter += (object sender, EventArgs e) => { panel.Focus(); };
            this.tabControl.TabPages[(int)DashBoardPages.Skills].Controls.Add(panel);
        #endregion

        #region Dice Roller
            DiceRollerControl dice = 
                this.tabControl.TabPages[(int)DashBoardPages.Dice].Controls[0] as DiceRollerControl;
            //dice.NumberOfEdge = this.CurrentNPC.EDG;    // todo figure out number of edge dice
        #endregion
        
            //todo other possible tabs
        }
        #endregion

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO
        }
    }
}
