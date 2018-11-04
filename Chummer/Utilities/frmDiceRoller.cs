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
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmDiceRoller : Form
    {
        private readonly frmChummerMain _frmMain;
        private readonly List<ListItem> _lstResults = new List<ListItem>(40);

        #region Control Events
        public frmDiceRoller(frmChummerMain frmMainForm, ICollection<Quality> lstQualities = null, int intDice = 1)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _frmMain = frmMainForm;
            nudDice.Value = intDice;
            if (lstQualities != null)
            {
                int intGremlinsRating = lstQualities.Count(x => x.Name == "Gremlins");
                if (intGremlinsRating > 0)
                    nudGremlins.Value = intGremlinsRating;
            }

            List<ListItem> lstMethod = new List<ListItem>
            {
                new ListItem("Standard", LanguageManager.GetString("String_DiceRoller_Standard", GlobalOptions.Language)),
                new ListItem("Large", LanguageManager.GetString("String_DiceRoller_Large", GlobalOptions.Language)),
                new ListItem("ReallyLarge", LanguageManager.GetString("String_DiceRoller_ReallyLarge", GlobalOptions.Language))
            };

            cboMethod.BeginUpdate();
            cboMethod.ValueMember = "Value";
            cboMethod.DisplayMember = "Name";
            cboMethod.DataSource = lstMethod;
            cboMethod.SelectedIndex = 0;
            cboMethod.EndUpdate();

            lblResultsLabel.Visible = false;
            lblResults.Text = string.Empty;
        }

        private void cmdRollDice_Click(object sender, EventArgs e)
        {
            List<int> lstRandom = new List<int>();
            int intHitCount = 0;
            int intGlitchCount = 0;
            int intGlitchMin = 1;
            
            // If Rushed Job is checked, the minimum die result for a Glitch becomes 2.
            if (chkRushJob.Checked)
                intGlitchMin = 2;

            for (int intCounter = 1; intCounter <= nudDice.Value; intCounter++)
            {
                if (chkRuleOf6.Checked)
                {
                    int intResult;
                    do
                    {
                        intResult = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    int intResult = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    lstRandom.Add(intResult);
                }
            }

            _lstResults.Clear();
            foreach (int intResult in lstRandom)
            {
                _lstResults.Add(new ListItem(intResult.ToString(), intResult.ToString()));

                if (cboMethod.SelectedValue.ToString() == "Standard")
                {
                    int intTarget = 5;
                    // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
                    if (chkCinematicGameplay.Checked)
                        intTarget = 4;

                    if (intResult >= intTarget)
                        intHitCount++;
                    if (intResult <= intGlitchMin)
                        intGlitchCount++;
                }
                else if (cboMethod.SelectedValue.ToString() == "Large")
                {
                    if (intResult >= 3)
                        intHitCount++;
                    if (intResult <= intGlitchMin)
                        intGlitchCount++;
                }
                else if (cboMethod.SelectedValue.ToString() == "ReallyLarge")
                {
                    intHitCount += intResult;
                }
            }

            int intGlitchThreshold = chkVariableGlitch.Checked
                ? intHitCount + 1
                : decimal.ToInt32(decimal.Ceiling((nudDice.Value + 1.0m) / 2.0m));
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= decimal.ToInt32(nudGremlins.Value);
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);

            if (chkBubbleDie.Checked)
            {
                if (chkVariableGlitch.Checked || (intGlitchCount == intGlitchThreshold - 1 && (decimal.ToInt32(nudDice.Value) & 1) == 0))
                {
                    int intBubbleDieResult = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    _lstResults.Add(new ListItem(intBubbleDieResult.ToString(GlobalOptions.InvariantCultureInfo), LanguageManager.GetString("String_BubbleDie", GlobalOptions.Language) +
                                                                                                                  strSpaceCharacter + '(' + intBubbleDieResult.ToString(GlobalOptions.CultureInfo) + ')'));
                    if (cboMethod.SelectedValue.ToString() == "Standard" || cboMethod.SelectedValue.ToString() == "Large")
                    {
                        if (intBubbleDieResult <= intGlitchMin)
                            intGlitchCount++;
                    }
                }
            }

            lblResultsLabel.Visible = true;
            lblResults.Text = string.Empty;
            if (intGlitchCount >= intGlitchThreshold)
            {
                if (intHitCount > 0)
                {
                    if (nudThreshold.Value > 0)
                    {
                        lblResults.Text += LanguageManager.GetString(intHitCount >= nudThreshold.Value ? "String_DiceRoller_Success" : "String_DiceRoller_Failure", GlobalOptions.Language) +
                                           strSpaceCharacter + '(' + string.Format(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo)) + ')';
                    }
                    else
                        lblResults.Text += string.Format(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language), intHitCount.ToString());
                }
                else
                    lblResults.Text += LanguageManager.GetString("String_DiceRoller_CriticalGlitch", GlobalOptions.Language);
            }
            else if (nudThreshold.Value > 0)
            {
                lblResults.Text += LanguageManager.GetString(intHitCount >= nudThreshold.Value ? "String_DiceRoller_Success" : "String_DiceRoller_Failure", GlobalOptions.Language) +
                                   strSpaceCharacter + '(' + string.Format(LanguageManager.GetString("String_DiceRoller_Hits", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo)) + ')';
            }
            else
                lblResults.Text += string.Format(LanguageManager.GetString("String_DiceRoller_Hits", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo));

            lblResults.Text += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("Label_DiceRoller_Sum", GlobalOptions.Language) + strSpaceCharacter + lstRandom.Sum().ToString(GlobalOptions.CultureInfo);
            lstResults.BeginUpdate();
            lstResults.DataSource = null;
            lstResults.ValueMember = "Value";
            lstResults.DisplayMember = "Name";
            lstResults.DataSource = _lstResults;
            lstResults.EndUpdate();
        }

        private void cboMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboMethod.SelectedValue.ToString() == "Standard")
                chkRuleOf6.Enabled = true;
            else
            {
                chkRuleOf6.Enabled = false;
                chkRuleOf6.Checked = false;
            }
        }

        private void frmDiceRoller_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove the Main window's reference to this form.
            _frmMain.RollerWindow = null;
        }

        private void cmdReroll_Click(object sender, EventArgs e)
        {
            int intKeepThreshold = 5;
            if (cboMethod.SelectedValue.ToString() == "Standard")
            {
                // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
                if (chkCinematicGameplay.Checked)
                    intKeepThreshold = 4;
            }

            int intKeepSum = 0;
            int intResult;
            // Remove everything that is not a hit
            for (int i = _lstResults.Count - 1; i >= 0; --i)
            {
                if (!int.TryParse(_lstResults[i].Value.ToString(), out intResult) || intResult < intKeepThreshold)
                {
                    _lstResults.RemoveAt(i);
                }
                else
                    intKeepSum += intResult;
            }

            List<int> lstRandom = new List<int>();
            int intHitCount = _lstResults.Count;
            if (cboMethod.SelectedValue.ToString() == "ReallyLarge")
                intHitCount = intKeepSum;
            int intGlitchCount = 0;

            // If Rushed Job is checked, the minimum die result for a Glitch becomes 2.
            int intGlitchMin = 1;
            if (chkRushJob.Checked)
                intGlitchMin = 2;

            for (int intCounter = 1; intCounter <= nudDice.Value - intHitCount; intCounter++)
            {
                if (chkRuleOf6.Checked)
                {
                    do
                    {
                        intResult = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    intResult = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    lstRandom.Add(intResult);
                }
            }

            foreach (int intLoopResult in lstRandom)
            {
                _lstResults.Add(new ListItem(intLoopResult.ToString(), intLoopResult.ToString()));

                if (cboMethod.SelectedValue.ToString() == "Standard")
                {
                    if (intLoopResult >= intKeepThreshold)
                        intHitCount++;
                    if (intLoopResult <= intGlitchMin)
                        intGlitchCount++;
                }
                else if (cboMethod.SelectedValue.ToString() == "Large")
                {
                    if (intLoopResult >= 3)
                        intHitCount++;
                    if (intLoopResult <= intGlitchMin)
                        intGlitchCount++;
                }
                else if (cboMethod.SelectedValue.ToString() == "ReallyLarge")
                {
                    intHitCount += intLoopResult;
                }
            }

            int intGlitchThreshold = chkVariableGlitch.Checked
                ? intHitCount + 1
                : decimal.ToInt32(decimal.Ceiling((nudDice.Value + 1.0m) / 2.0m));
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= decimal.ToInt32(nudGremlins.Value);
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);

            if (chkBubbleDie.Checked)
            {
                if (chkVariableGlitch.Checked || (intGlitchCount == intGlitchThreshold - 1 && (decimal.ToInt32(nudDice.Value) & 1) == 0))
                {
                    int intBubbleDieResult = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    _lstResults.Add(new ListItem(intBubbleDieResult.ToString(), LanguageManager.GetString("String_BubbleDie", GlobalOptions.Language)
                                                                                + strSpaceCharacter + '(' + intBubbleDieResult.ToString(GlobalOptions.CultureInfo) + ')'));

                    if (cboMethod.SelectedValue.ToString() == "Standard" || cboMethod.SelectedValue.ToString() == "Large")
                    {
                        if (intBubbleDieResult <= intGlitchMin)
                            intGlitchCount++;
                    }
                }
            }

            
            lblResultsLabel.Visible = true;
            lblResults.Text = string.Empty;
            if (intGlitchCount >= intGlitchThreshold)
            {
                if (intHitCount > 0)
                {
                    if (nudThreshold.Value > 0)
                    {
                        if (intHitCount >= nudThreshold.Value)
                            lblResults.Text += LanguageManager.GetString("String_DiceRoller_Success", GlobalOptions.Language) + strSpaceCharacter + '(' + string.Format(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo)) + ')';
                        else
                            lblResults.Text += LanguageManager.GetString("String_DiceRoller_Failure", GlobalOptions.Language) + strSpaceCharacter + '(' + string.Format(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo)) + ')';
                    }
                    else
                        lblResults.Text += string.Format(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo));
                }
                else
                    lblResults.Text += LanguageManager.GetString("String_DiceRoller_CriticalGlitch", GlobalOptions.Language);
            }
            else if (nudThreshold.Value > 0)
            {
                if (intHitCount >= nudThreshold.Value)
                    lblResults.Text += LanguageManager.GetString("String_DiceRoller_Success", GlobalOptions.Language) + strSpaceCharacter + '(' + string.Format(LanguageManager.GetString("String_DiceRoller_Hits", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo)) + ')';
                else
                    lblResults.Text += LanguageManager.GetString("String_DiceRoller_Failure", GlobalOptions.Language) + strSpaceCharacter + '(' + string.Format(LanguageManager.GetString("String_DiceRoller_Glitch", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo)) + ')';
            }
            else
                lblResults.Text += string.Format(LanguageManager.GetString("String_DiceRoller_Hits", GlobalOptions.Language), intHitCount.ToString(GlobalOptions.CultureInfo));

            lblResults.Text += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("Label_DiceRoller_Sum", GlobalOptions.Language) + strSpaceCharacter + (lstRandom.Sum() + intKeepSum).ToString(GlobalOptions.CultureInfo);
            lstResults.BeginUpdate();
            lstResults.DataSource = null;
            lstResults.ValueMember = "Value";
            lstResults.DisplayMember = "Name";
            lstResults.DataSource = _lstResults;
            lstResults.EndUpdate();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Number of dice to roll.
        /// </summary>
        public int Dice
        {
            set => nudDice.Value = value;
        }

        /// <summary>
        /// List of Qualities the character has to determine whether or not they have Gremlins and at which Rating.
        /// </summary>
        public IEnumerable<Quality> Qualities
        {
            set
            {
                nudGremlins.Value = 0;
                if (value != null)
                {
                    foreach (Quality objQuality in value)
                    {
                        if (objQuality.Name.StartsWith("Gremlins"))
                        {
                            int intRating = Convert.ToInt32(objQuality.Name.Substring(objQuality.Name.Length - 2, 1));
                            nudGremlins.Value = intRating;
                        }
                    }
                }
            }
        }
        #endregion

        private void chkVariableGlitch_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkBubbleDie_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
