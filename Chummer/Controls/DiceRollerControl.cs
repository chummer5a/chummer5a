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
                return Convert.ToInt32(nudThreshold.Value);
            }
            set
            {
                nudThreshold.Value = value;
            }
        }

        /// <summary>
        /// Applies the gremlins affect (by rating)
        /// </summary>
        public int Gremlins 
        {
            get
            {
                return Convert.ToInt32(nudGremlins.Value);
            }
            set
            {
                nudGremlins.Value = value;
            }
        }

        /// <summary>
        /// The number of dice to roll
        /// </summary>
        public int NumberOfDice 
        {
            get
            {
                return Convert.ToInt32(nudDice.Value);
            }
            set
            {
                nudDice.Value = value;
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
                return (EdgeUses)cboEdgeUse.SelectedItem == default(EdgeUses) ? EdgeUses.None :
                (EdgeUses)cboEdgeUse.SelectedItem;
            }
            set
            {
                switch (value)
                {
                    case EdgeUses.CloseCall: break;
                    case EdgeUses.SeizeTheInit: break;
                    default: cboEdgeUse.SelectedItem = value; break;
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
                return Convert.ToInt32(nudLimit.Value);
            }
            set
            {
                nudLimit.Value = value;
            }
        }

        #endregion

        public DiceRollerControl()
        {
            InitializeComponent();
            nudDice.Maximum = Int32.MaxValue;
            nudGremlins.Maximum = Int32.MaxValue;
            nudLimit.Maximum = Int32.MaxValue;
            nudThreshold.Maximum = Int32.MaxValue;
            List<EdgeUses> edge = new List<EdgeUses>() 
            { 
                EdgeUses.None, 
                EdgeUses.PushTheLimit, 
                EdgeUses.SecondChance, 
                EdgeUses.Blitz, 
                EdgeUses.DeadManTrigger
            };
            cboEdgeUse.BeginUpdate();
            cboEdgeUse.DataSource = edge;
            cboEdgeUse.EndUpdate();
            EdgeUse = EdgeUses.None;
        }

        #region Event Handling
        private void cmdRollDice_Click(object sender, EventArgs e)
        {
            // TODO roll the dice
            List<int> results = new List<int>();
            for (int i = 0; i < NumberOfDice; i++)
            {
                int val = _random.Next(1, 7);
                results.Add(val);

                // check for pushing the limit
                if ((EdgeUse == EdgeUses.PushTheLimit || chkRuleOf6.Checked) && val == 6)
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
            if (chkRushJob.Checked)
                results.ForEach(x => { if (x == 1 || x == 2) glitches++;});
            else
                results.ForEach(x => { if (x == 1) glitches++;});

            // calculate if we glitched or critically glitched (using gremlins)
            bool glitch = false, criticalGlitch = false;
            glitch = glitches + Gremlins > 0 && results.Count / (glitches + Gremlins) < 2;

            if (glitch && hits == 0)
                criticalGlitch = true;
            int limitAppliedHits = hits;
            if (Limit > 0 && !(EdgeUse == EdgeUses.PushTheLimit))
                limitAppliedHits = hits > Limit ? Limit : hits;
            
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

            if (Threshold > 0)
                if (hits >= Threshold || limitAppliedHits >= Threshold)
                    lblThreshold.Text = "Success! Threshold:";   // we succeded on the threshold test...


            lblResults.Text = sb.ToString();

            // now populate the text box
            sb = new StringBuilder();
            // apply limit modifiers
            results.ForEach(x => { sb.Append(x.ToString()); sb.Append(", "); });
            sb.Length -= 2; // remove trailing comma
            txtResults.Text = sb.ToString();
        }

        private void nudDice_ValueChanged(object sender, EventArgs e)
        {
            NumberOfDice = Convert.ToInt32(nudDice.Value);
        }

        private void nudThreshold_ValueChanged(object sender, EventArgs e)
        {
            Threshold = Convert.ToInt32(nudThreshold.Value);
        }

        private void nudGremlins_ValueChanged(object sender, EventArgs e)
        {
            Gremlins = Convert.ToInt32(nudGremlins.Value);
        }

        private void nudLimit_ValueChanged(object sender, EventArgs e)
        {
            Limit = Convert.ToInt32(nudLimit.Value);
        }
        #endregion
    }
}
