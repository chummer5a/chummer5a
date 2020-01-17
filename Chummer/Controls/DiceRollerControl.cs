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
using System.Collections.Generic;
 using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class DiceRollerControl : UserControl
    {
        private static readonly Random s_ObjRandom = MersenneTwister.SfmtRandom.Create();
        private int _intModuloTemp;

        #region Properties
        private enum EdgeUses
        {
            None = 0,
            PushTheLimit,
            SecondChance,
            SeizeTheInit,
            Blitz,
            CloseCall,
            DeadManTrigger,
        }

        /// <summary>
        /// The threshold that the character needs in order to pass
        /// </summary>
        public int Threshold 
        {
            get => decimal.ToInt32(nudThreshold.Value);
            set => nudThreshold.Value = value;
        }

        /// <summary>
        /// Applies the gremlins affect (by rating)
        /// </summary>
        public int Gremlins 
        {
            get => decimal.ToInt32(nudGremlins.Value);
            set => nudGremlins.Value = value;
        }

        /// <summary>
        /// The number of dice to roll
        /// </summary>
        public int NumberOfDice 
        {
            get => decimal.ToInt32(nudDice.Value);
            set => nudDice.Value = value;
        }

        /// <summary>
        /// True iff the character is using edge to re-roll sixes
        /// </summary>
        public bool UseEdge { get; set; }

        /// <summary>
        /// What the character is using for it's edge case
        /// </summary>
        private EdgeUses EdgeUse 
        {
            get
            {
                EdgeUses eSelectedItem = (EdgeUses)cboEdgeUse.SelectedItem;
                return eSelectedItem == default(EdgeUses) ? EdgeUses.None : eSelectedItem;
            }
            set
            {
                switch (value)
                {
                    case EdgeUses.CloseCall: break;
                    case EdgeUses.SeizeTheInit: break;
                    default:cboEdgeUse.SelectedItem = value; break;
                }
            }
        }

        public int NumberOfEdge { get; set; }

        /// <summary>
        /// The maximum number of hits allowed on this roll
        /// </summary>
        public int Limit 
        {
            get => decimal.ToInt32(nudLimit.Value);
            set => nudLimit.Value = value;
        }

        #endregion

        public DiceRollerControl()
        {
            InitializeComponent();
            nudDice.Maximum = int.MaxValue;
            nudGremlins.Maximum = int.MaxValue;
            nudLimit.Maximum = int.MaxValue;
            nudThreshold.Maximum = int.MaxValue;
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
                do
                {
                    _intModuloTemp = s_ObjRandom.Next();
                }
                while (_intModuloTemp >= int.MaxValue - 1); // Modulo bias removal for 1d6
                int val = 1 + _intModuloTemp % 6;
                results.Add(val);

                // check for pushing the limit
                if (EdgeUse == EdgeUses.PushTheLimit || chkRuleOf6.Checked)
                {
                    while (val == 6)
                    {
                        do
                        {
                            _intModuloTemp = s_ObjRandom.Next();
                        }
                        while (_intModuloTemp >= int.MaxValue - 1); // Modulo bias removal for 1d6
                        val = 1 + _intModuloTemp % 6;
                        results.Add(val);
                    }
                }
            }

            // populate the text box
            StringBuilder sb = new StringBuilder();
            // show the number of hits
            int hits = 0;
            // calculate the 1 (and 2's)
            int glitches = 0;
            foreach (int intResult in results)
            {
                if (intResult == 5 || intResult == 6)
                    hits += 1;
                else if (intResult == 1 || (chkRushJob.Checked && intResult == 2))
                    glitches += 1;
                sb.Append(intResult.ToString());
                sb.Append(", ");
            }
            if (sb.Length > 0)
                sb.Length -= 2; // remove trailing comma

            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            if (chkBubbleDie.Checked && (results.Count & 1) == 0 && results.Count / 2 == glitches + Gremlins)
            {
                do
                {
                    _intModuloTemp = s_ObjRandom.Next();
                }
                while (_intModuloTemp >= int.MaxValue - 1); // Modulo bias removal for 1d6
                int intBubbleDieResult = 1 + _intModuloTemp % 6;
                sb.Append(',' + strSpace + LanguageManager.GetString("String_BubbleDie", GlobalOptions.Language) + strSpace + '(' + intBubbleDieResult.ToString(GlobalOptions.Instance.CultureInfo) + ')');
                if (intBubbleDieResult == 1 || (chkRushJob.Checked && intBubbleDieResult == 2))
                {
                    glitches++;
                }
            }
            txtResults.Text = sb.ToString();

            // calculate if we glitched or critically glitched (using gremlins)
            bool glitch = glitches + Gremlins > 0 && results.Count / (glitches + Gremlins) < 2;
            
            int limitAppliedHits = hits;
            string strLimitString = string.Empty;
            if (limitAppliedHits > Limit && EdgeUse != EdgeUses.PushTheLimit)
            {
                limitAppliedHits = Limit;
                strLimitString = ',' + strSpace + LanguageManager.GetString("String_Limit", GlobalOptions.Language) + strSpace + limitAppliedHits.ToString();
            }

            // show the results
            // we have not gone over our limit
            sb = new StringBuilder(LanguageManager.GetString("Label_DiceRoller_Result", GlobalOptions.Language) + ' ');
            if (glitch)
            {
                if (hits > 0)
                {
                    if (Threshold > 0)
                    {
                        sb.AppendFormat(LanguageManager.GetString(hits >= Threshold || limitAppliedHits >= Threshold ? "String_DiceRoller_Success" : "String_DiceRoller_Failure", GlobalOptions.Language) +
                                        strSpace + '(' + LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language) + strLimitString + ')', hits.ToString(GlobalOptions.Instance.CultureInfo));
                    }
                    else
                    {
                        sb.AppendFormat(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language) + strLimitString, hits.ToString(GlobalOptions.Instance.CultureInfo));
                    }
                }
                else
                {
                    sb.Append(LanguageManager.GetString("String_DiceRoller_CriticalGlitch", GlobalOptions.Language));
                }
            }
            else if (Threshold > 0)
            {
                sb.AppendFormat(LanguageManager.GetString(hits >= Threshold || limitAppliedHits >= Threshold ? "String_DiceRoller_Success" : "String_DiceRoller_Failure", GlobalOptions.Language) +
                                strSpace + '(' + LanguageManager.GetString("String_DiceRoller_Hits", GlobalOptions.Language) + strLimitString + ')', hits.ToString(GlobalOptions.Instance.CultureInfo));
            }
            else
            {
                sb.AppendFormat(LanguageManager.GetString("String_DiceRoller_Hits", GlobalOptions.Language) + strLimitString, hits.ToString(GlobalOptions.Instance.CultureInfo));
            }

            lblResults.Text = sb.ToString();
        }

        private void nudDice_ValueChanged(object sender, EventArgs e)
        {
            NumberOfDice = decimal.ToInt32(nudDice.Value);
        }

        private void nudThreshold_ValueChanged(object sender, EventArgs e)
        {
            Threshold = decimal.ToInt32(nudThreshold.Value);
        }

        private void nudGremlins_ValueChanged(object sender, EventArgs e)
        {
            Gremlins = decimal.ToInt32(nudGremlins.Value);
        }

        private void nudLimit_ValueChanged(object sender, EventArgs e)
        {
            Limit = decimal.ToInt32(nudLimit.Value);
        }
        #endregion
    }
}
