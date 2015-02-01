using System;
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
