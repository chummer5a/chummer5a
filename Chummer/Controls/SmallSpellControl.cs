using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.Controls
{
    public partial class SmallSpellControl : UserControl
    {
        private Spell _objSpell;
        private Form _objParent;

        /// <summary>
        /// Fired when the user decides to roll the dice
        /// </summary>
        public event EventHandler DiceClick
        {
            add
            {
                this.cmdRollSpell.Click += value;
                btnDrain.Click += value;
            }
            remove
            {
                this.cmdRollSpell.Click -= value;
                btnDrain.Click -= value;
            }
        }

        public Spell Spell
        {
            get { return this._objSpell; }
            set
            {
                _objSpell = value;
                txtSpellName.Text = $"{_objSpell.DisplayNameShort}: {_objSpell.DicePool}";
                lblDrain.Text = _objSpell.DV;
            }
        }

        public int DrainResist { get; set; }

        public SmallSpellControl(Form parent)
        {
            this._objParent = parent;
            InitializeComponent();
        }

        private void cmdRollSpell_Click(object sender, EventArgs e)
        {
            if (this._objParent is frmGMDashboard)
            {
                frmGMDashboard dash = this._objParent as frmGMDashboard;
                dash.DiceRoller.Clear();
                dash.DiceRoller.NumberOfDice = this.Spell.DicePool;
                dash.DiceRoller.Limit = nudDram.Value > 0 ? (int) nudDram.Value : (int) nudForce.Value;
                dash.DiceRoller.EdgeUse = DiceRollerControl.EdgeUses.None;
                dash.DiceRoller.NumberOfEdge = Convert.ToInt32(((Attribute) dash.CurrentNPC.EDG).TotalValue);
            }
        }

        private void lblDram_Click(object sender, EventArgs e)
        {

        }

        private void btnDrain_Click(object sender, EventArgs e)
        {
            if (_objParent is frmGMDashboard)
            {
                frmGMDashboard dash = this._objParent as frmGMDashboard;
                dash.DiceRoller.Clear();
                dash.DiceRoller.NumberOfDice = DrainResist;
                dash.DiceRoller.Limit = nudDram.Value > 0 ? (int) nudDram.Value : (int) nudForce.Value;
                dash.DiceRoller.EdgeUse = DiceRollerControl.EdgeUses.None;
                dash.DiceRoller.NumberOfEdge = Convert.ToInt32(((Attribute) dash.CurrentNPC.EDG).TotalValue);
                dash.DiceRoller.Threshold = Spell.GetDrainValue((int)nudForce.Value);
            }
        }

        private void nudForce_ValueChanged(object sender, EventArgs e)
        {
            this.lblDrainValue.Text = $"Drain: {Spell.GetDrainValue((int) nudForce.Value)}";
        }
    }
}

