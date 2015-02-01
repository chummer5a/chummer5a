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
    public partial class DiceRollerControl : UserControl
    {
        private static Random _random = new Random();

        #region Properties
        public enum EdgeUses { None, PushTheLimit, SecondChance, SeizeTheInit, Blitz, CloseCall, DeadManTrigger }

        /// <summary>
        /// The threshold that the character needs in order to pass
        /// </summary>
        public int Threshold 
        {
            get
            {
                return Convert.ToInt32(this.nudThreshold.Value);
            }
            set
            {
                this.nudThreshold.Value = value;
            }
        }

        /// <summary>
        /// Applies the gremlins affect (by rating)
        /// </summary>
        public int Gremlins 
        {
            get
            {
                return Convert.ToInt32(this.nudGremlins.Value);
            }
            set
            {
                this.nudGremlins.Value = value;
            }
        }

        /// <summary>
        /// The number of dice to roll
        /// </summary>
        public int NumberOfDice 
        {
            get
            {
                return Convert.ToInt32(this.nudDice.Value);
            }
            set
            {
                this.nudDice.Value = value;
            }
        }

        /// <summary>
        /// True iff the character is using edge to re-roll sixes
        /// </summary>
        public bool UseEdge { get; set; }

        /// <summary>
        /// What the character is using for it's edge case
        /// </summary>
        public EdgeUses EdgeUse 
        {
            get
            {
                return (EdgeUses)this.cboEdgeUse.SelectedItem == default(EdgeUses) ? EdgeUses.None :
                (EdgeUses)this.cboEdgeUse.SelectedItem;
            }
            set
            {
                switch (value)
                {
                    case EdgeUses.CloseCall: break;
                    case EdgeUses.SeizeTheInit: break;
                    default: this.cboEdgeUse.SelectedItem = value; break;
                }
            }
        }

        public int NumberOfEdge { get; set; }

        /// <summary>
        /// The maximum number of hits allowed on this roll
        /// </summary>
        public int Limit 
        {
            get
            {
                return Convert.ToInt32(this.nudLimit.Value);
            }
            set
            {
                this.nudLimit.Value = value;
            }
        }

        #endregion

        public DiceRollerControl()
        {
            InitializeComponent();
            this.nudDice.Maximum = Int32.MaxValue;
            this.nudGremlins.Maximum = Int32.MaxValue;
            this.nudLimit.Maximum = Int32.MaxValue;
            this.nudThreshold.Maximum = Int32.MaxValue;
            List<EdgeUses> edge = new List<EdgeUses>() 
            { 
                EdgeUses.None, 
                EdgeUses.PushTheLimit, 
                EdgeUses.SecondChance, 
                EdgeUses.Blitz, 
                EdgeUses.DeadManTrigger
            };
            this.cboEdgeUse.DataSource = edge;
            this.EdgeUse = EdgeUses.None;
        }

        #region Event Handling
        private void cmdRollDice_Click(object sender, EventArgs e)
        {
            // TODO roll the dice
            List<int> results = new List<int>();
            for (int i = 0; i < this.NumberOfDice; i++)
            {
                int val = _random.Next(1, 7);
                results.Add(val);

                // check for pushing the limit
                if ((this.EdgeUse == EdgeUses.PushTheLimit || this.chkRuleOf6.Checked) && val == 6)
                {
                    int edgeRoll = _random.Next(1, 7);
                    results.Add(edgeRoll);
                    bool @continue = true;
                    while (@continue)
                    {
                        if (edgeRoll != 6)
                            @continue = false;
                        else
                        {
                            edgeRoll = _random.Next(1, 7);
                            results.Add(edgeRoll);
                        }
                    }
                }
            }

            // show the number of hits
            int hits = 0;
            results.ForEach(x => { if (x == 5 || x == 6) hits++; });

            // calculate the 1 (and 2's)
            int glitches = 0;
            if (this.chkRushJob.Checked)
                results.ForEach(x => { if (x == 1 || x == 2) glitches++;});
            else
                results.ForEach(x => { if (x == 1) glitches++;});

            // calculate if we glitched or critically glitched (using gremlins)
            bool glitch = false, criticalGlitch = false;
            glitch = glitches + this.Gremlins > 0 && results.Count / (glitches + this.Gremlins) < 2;

            if (glitch && hits == 0)
                criticalGlitch = true;
            int limitAppliedHits = hits;
            if (this.Limit > 0 && !(this.EdgeUse == EdgeUses.PushTheLimit))
                limitAppliedHits = hits > this.Limit ? this.Limit : hits;
            
            // show the results
            // we have not gone over our limit
            StringBuilder sb = new StringBuilder();
            if (hits > 0 && limitAppliedHits == hits)
                sb.Append("Results: " + hits + " Hits!");
            if (limitAppliedHits < hits)
                sb.Append("Results: " + limitAppliedHits + " Hits by Limit!");
            if (glitch && !criticalGlitch)
                sb.Append(" Glitch!");   // we glitched though...
            if (criticalGlitch)
                sb.Append("Results: Critical Glitch!");   // we crited!
            if (hits == 0 && !glitch)
                sb.Append("Results: 0 Hits.");   // we have no hits and no glitches

            if (this.Threshold > 0)
                if (hits >= this.Threshold || limitAppliedHits >= this.Threshold)
                    this.lblThreshold.Text = "Success! Threshold:";   // we succeded on the threshold test...


            this.lblResults.Text = sb.ToString();

            // now populate the text box
            sb = new StringBuilder();
            // apply limit modifiers
            results.ForEach(x => { sb.Append(x.ToString()); sb.Append(", "); });
            sb.Length -= 2; // remove trailing comma
            this.txtResults.Text = sb.ToString();
        }

        private void nudDice_ValueChanged(object sender, EventArgs e)
        {
            this.NumberOfDice = Convert.ToInt32(this.nudDice.Value);
        }

        private void nudThreshold_ValueChanged(object sender, EventArgs e)
        {
            this.Threshold = Convert.ToInt32(this.nudThreshold.Value);
        }

        private void nudGremlins_ValueChanged(object sender, EventArgs e)
        {
            this.Gremlins = Convert.ToInt32(this.nudGremlins.Value);
        }

        private void nudLimit_ValueChanged(object sender, EventArgs e)
        {
            this.Limit = Convert.ToInt32(this.nudLimit.Value);
        }
        #endregion
    }
}
