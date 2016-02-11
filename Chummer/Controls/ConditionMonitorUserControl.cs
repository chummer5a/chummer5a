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
        private int _modifier;
        private int currentOverflow;

        #region Properties

        public delegate void DamagedEventHandler(object sender, EventArgs e);

        public event DamagedEventHandler CurrentCharacterDamageApplied;

        // the maximum physical overflow damage of the current character //BODY
        private int maxOverflow;

        public virtual void OnDamage(EventArgs e)
        {
            CurrentCharacterDamageApplied?.Invoke(this, e);
        }

        /// <summary>
        /// The HP of the character
        /// </summary>
        public int MaxPhysical
        {
            get { return this._progressBarPhysical.Maximum; }
            set
            {
                this._progressBarPhysical.Maximum = value;
                _lblPhysical.Text = $"Physical: {Physical}/{MaxPhysical}";
            }
        }

        /// <summary>
        /// The stun chart of the character
        /// </summary>
        public int MaxStun
        {
            get { return this._progressBarStun.Maximum; }
            set
            {
                this._progressBarStun.Maximum = value;
                _lblStun.Text = $"Stun: {Stun}/{MaxStun}";
            }
        }

        /// <summary>
        /// The physical health of the current npc
        /// </summary>
        public int Physical 
        {
            get{ return this._progressBarPhysical.Value; }
            set
            {
                this._progressBarPhysical.Value = value;
                _lblPhysical.Text = $"{Physical}:{MaxPhysical}";
            }
        }

        /// <summary>
        /// The stun health of the current npc
        /// </summary>
        public int Stun 
        {
            get { return this._progressBarStun.Value; }
            set
            {
                this._progressBarStun.Value = value;
                _lblStun.Text = $"Stun: {Stun}/{MaxStun}";
                lblKnockOut.Visible = Stun >= MaxStun;
            }

        }
        #endregion

        public int Modifier
        {
            get { return _modifier; }
            set
            {
                _modifier = value;
                _lblModifier.Text =Modifier!=0? $"Pool: {Modifier}":string.Empty;
            }
        }
       
        // the overflow damage of the current Character
        public int CurrentOverflow
        {
            get
            {
                return currentOverflow;
            }

            set
            {
                currentOverflow = value;
                this.lblOverflowValue.Text = $"Overflow Damage : {value}";
                ApplyOverflow(currentOverflow);
                SetOverflowVisible(value > 0);
                lblDead.Visible = currentOverflow >= maxOverflow;
            }
        }

        private void ApplyOverflow(int overflow)
        {
            if (overflow > 0)
            {
                _pbOverflow.Value = overflow >= maxOverflow ? maxOverflow : overflow;
            }
        }

        public int MaxOverflow
        {
            get
            {
                return maxOverflow;
            }

            set
            {
                maxOverflow = value;
                _pbOverflow.Maximum = value;
                lblMaxOverflow.Text = $"Dead at {maxOverflow}";
            }
        }

        private void SetOverflowVisible(bool show)
        {
            this.lblOverflowValue.Visible = show;
            this.lblMaxOverflow.Visible = show;
            this._pbOverflow.Visible = show;
        }

        public ConditionMonitorUserControl()
        {
            InitializeComponent();
            _lblModifier.Text = Modifier != 0 ? $"Pool: {Modifier}" : string.Empty;
        }

        private void ApplyPhysicalDamage(int val)
        {
            if (val == 0) return;
            var wayToDeath = (val + _progressBarPhysical.Value - _progressBarPhysical.Maximum);
            if (CurrentOverflow > 0 )
            {
                CurrentOverflow += wayToDeath;
                if (CurrentOverflow < 0)
                {
                    var heal = CurrentOverflow;
                    CurrentOverflow = 0;
                    ApplyPhysicalDamage(heal);
                    
                }
                val = 0;
                wayToDeath = 0;
            }
            if (val + this._progressBarPhysical.Value < 0)
            {
                this._progressBarPhysical.Value = 0;
            }
            else if (val + this._progressBarPhysical.Value > this._progressBarPhysical.Maximum)
            {
                this._progressBarPhysical.Value = this._progressBarPhysical.Maximum;

            }
            else
            {
                //this._progressBarPhysical.Value += val > this._progressBarPhysical.Maximum
                //    ? this._progressBarPhysical.Maximum
                //    : val;
                this._progressBarPhysical.Value += val;
            }
            if (_progressBarPhysical.Value >= _progressBarPhysical.Maximum)
            {
                CurrentOverflow += wayToDeath;
            }
        }

        private void ApplyStunDamage(int val)
        {
            if (val == 0) return;
            var stunoverflow = (val + _progressBarStun.Value - _progressBarStun.Maximum)/2;
            if (val + this._progressBarStun.Value < 0)
            {
                this._progressBarStun.Value = 0;
            }
            else if (val + this._progressBarStun.Value > this._progressBarStun.Maximum)
            {

                this._progressBarStun.Value = this._progressBarStun.Maximum;
                //stun overflow becomes physical Damage
                ApplyPhysicalDamage(stunoverflow);
            }

            else
            {
                this._progressBarStun.Value += val > this._progressBarStun.Maximum
                    ? this._progressBarStun.Maximum
                    : val;
            }
        }

        private void _btnApplyStun_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(this.nudStun.Value);
            ApplyStunDamage(val);
            OnDamage(EventArgs.Empty);
            nudStun.Value = 0;
        }

        private void _btnHealStun_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(this.nudRecoverStun.Value);
            ApplyStunDamage(-val);
            OnDamage(EventArgs.Empty);
            nudRecoverStun.Value = 0;
        }

        private void _btnHealPhysical_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(this.nudHeal.Value);
            ApplyPhysicalDamage(-val);
            OnDamage(EventArgs.Empty);
            nudHeal.Value = 0;
        }

        private void _btnPhysical_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(this._nudPhysical.Value);
            ApplyPhysicalDamage(val);
            OnDamage(EventArgs.Empty);
            _nudPhysical.Value = 0;
        }
    }
}
