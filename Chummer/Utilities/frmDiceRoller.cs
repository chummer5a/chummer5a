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
 using System.Text;
 using System.Windows.Forms;

namespace Chummer
{
    public partial class frmDiceRoller : Form
    {
        private readonly frmChummerMain _frmMain;
        private readonly List<DiceRollerListViewItem> _lstResults = new List<DiceRollerListViewItem>(40);

        #region Control Events
        public frmDiceRoller(frmChummerMain frmMainForm, IEnumerable<Quality> lstQualities = null, int intDice = 1)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _frmMain = frmMainForm;
            nudDice.Value = intDice;
            if (lstQualities != null)
            {
                int intGremlinsRating = lstQualities.Count(x => x.Name == "Gremlins");
                if (intGremlinsRating > 0)
                    nudGremlins.Value = intGremlinsRating;
            }

            List<ListItem> lstMethod = new List<ListItem>(3)
            {
                new ListItem("Standard", LanguageManager.GetString("String_DiceRoller_Standard")),
                new ListItem("Large", LanguageManager.GetString("String_DiceRoller_Large")),
                new ListItem("ReallyLarge", LanguageManager.GetString("String_DiceRoller_ReallyLarge"))
            };

            cboMethod.BeginUpdate();
            cboMethod.PopulateWithListItems(lstMethod);
            cboMethod.SelectedIndex = 0;
            cboMethod.EndUpdate();

            lblResultsLabel.Visible = false;
            lblResults.Text = string.Empty;
        }

        private void cmdRollDice_Click(object sender, EventArgs e)
        {
            List<int> lstRandom = new List<int>(nudDice.ValueAsInt);
            int intGlitchMin = 1;

            // If Rushed Job is checked, the minimum die result for a Glitch becomes 2.
            if (chkRushJob.Checked)
                intGlitchMin = 2;

            int intTarget = 5;
            // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
            if (chkCinematicGameplay.Checked)
                intTarget = 4;
            switch (cboMethod.SelectedValue.ToString())
            {
                case "Large":
                {
                    intTarget = 3;
                    break;
                }
                case "ReallyLarge":
                {
                    intTarget = 1;
                    intGlitchMin = 7;
                    break;
                }
            }

            for (int intCounter = 1; intCounter <= nudDice.Value; intCounter++)
            {
                if (chkRuleOf6.Checked)
                {
                    int intResult;
                    do
                    {
                        intResult = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    int intResult = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    lstRandom.Add(intResult);
                }
            }

            _lstResults.Clear();
            foreach (int intResult in lstRandom)
            {
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intResult, intTarget, intGlitchMin);
                _lstResults.Add(lviCur);
            }

            int intHitCount = cboMethod.SelectedValue?.ToString() == "ReallyLarge" ? _lstResults.Sum(x => x.Result) : _lstResults.Count(x => x.IsHit);
            int intGlitchCount = _lstResults.Count(x => x.IsGlitch);

            int intGlitchThreshold = chkVariableGlitch.Checked
                ? intHitCount + 1
                : ((nudDice.Value + 1) / 2).StandardRound();
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= nudGremlins.ValueAsInt;
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            string strSpace = LanguageManager.GetString("String_Space");

            if (chkBubbleDie.Checked
                && (chkVariableGlitch.Checked
                    || (intGlitchCount == intGlitchThreshold - 1
                        && (nudDice.ValueAsInt & 1) == 0)))
            {
                int intBubbleDieResult = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intBubbleDieResult, intTarget, intGlitchMin, true);
                if (lviCur.IsGlitch)
                    intGlitchCount += 1;
                _lstResults.Add(lviCur);
            }

            lblResultsLabel.Visible = true;
            StringBuilder sbdResults = new StringBuilder();
            if (intGlitchCount >= intGlitchThreshold)
            {
                if (intHitCount > 0)
                {
                    if (nudThreshold.Value > 0)
                    {
                        sbdResults.Append(LanguageManager.GetString(intHitCount >= nudThreshold.Value ? "String_DiceRoller_Success" : "String_DiceRoller_Failure") + strSpace + '(');
                    }
                    sbdResults.AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("String_DiceRoller_Glitch"), intHitCount);
                    if (nudThreshold.Value > 0)
                        sbdResults.Append(')');
                }
                else
                    sbdResults.Append(LanguageManager.GetString("String_DiceRoller_CriticalGlitch"));
            }
            else if (nudThreshold.Value > 0)
            {
                sbdResults.Append(LanguageManager.GetString(intHitCount >= nudThreshold.Value ? "String_DiceRoller_Success" : "String_DiceRoller_Failure")
                                  + strSpace + '(' + string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_DiceRoller_Hits"), intHitCount + ')'));
            }
            else
                sbdResults.AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("String_DiceRoller_Hits"), intHitCount);

            sbdResults.Append(Environment.NewLine + Environment.NewLine +
                              LanguageManager.GetString("Label_DiceRoller_Sum") + strSpace +
                              _lstResults.Sum(x => x.Result).ToString(GlobalOptions.CultureInfo));
            lblResults.Text = sbdResults.ToString();

            lstResults.BeginUpdate();
            lstResults.Items.Clear();
            foreach (DiceRollerListViewItem objItem in _lstResults)
            {
                lstResults.Items.Add(objItem);
            }
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
            //Remove the BubbleDie (it is always at the end)
            _lstResults.RemoveAll(x => x.BubbleDie);

            // If Rushed Job is checked, the minimum die result for a Glitch becomes 2.
            int intGlitchMin = 1;
            if (chkRushJob.Checked)
                intGlitchMin = 2;

            int intTarget = 5;
            // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
            if (chkCinematicGameplay.Checked)
                intTarget = 4;
            switch (cboMethod.SelectedValue.ToString())
            {
                case "Large":
                {
                    intTarget = 3;
                    break;
                }
                case "ReallyLarge":
                {
                    intTarget = 1;
                    intGlitchMin = 7;
                    break;
                }
            }

            foreach (DiceRollerListViewItem objItem in _lstResults)
            {
                objItem.Target = intTarget;
                objItem.GlitchMin = intGlitchMin;
            }

            // Remove everything that is not a hit
            int intNewDicePool = _lstResults.Count(x => !x.IsHit);
            _lstResults.RemoveAll(x => !x.IsHit);

            if (intNewDicePool == 0)
            {
                MessageBox.Show(LanguageManager.GetString("String_NoDiceLeft_Text"), LanguageManager.GetString("String_NoDiceLeft_Title"));
                return;
            }

            int intHitCount = cboMethod.SelectedValue?.ToString() == "ReallyLarge" ? _lstResults.Sum(x => x.Result) : _lstResults.Count(x => x.IsHit);
            int intGlitchCount = _lstResults.Count(x => x.IsGlitch);
            List<int> lstRandom = new List<int>(intNewDicePool);

            for (int intCounter = 1; intCounter <= intNewDicePool; intCounter++)
            {
                if (chkRuleOf6.Checked)
                {
                    int intLoopResult;
                    do
                    {
                        intLoopResult = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                        lstRandom.Add(intLoopResult);
                    } while (intLoopResult == 6);
                }
                else
                {
                    int intLoopResult = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    lstRandom.Add(intLoopResult);
                }
            }

            foreach (int intResult in lstRandom)
            {
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intResult, intTarget, intGlitchMin);
                _lstResults.Add(lviCur);
            }

            int intGlitchThreshold = chkVariableGlitch.Checked
                ? intHitCount + 1
                : ((nudDice.Value + 1) / 2).StandardRound();
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= nudGremlins.ValueAsInt;
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            string strSpace = LanguageManager.GetString("String_Space");

            if (chkBubbleDie.Checked
                && (chkVariableGlitch.Checked
                    || (intGlitchCount == intGlitchThreshold - 1
                        && (nudDice.ValueAsInt & 1) == 0)))
            {
                int intBubbleDieResult = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intBubbleDieResult, intTarget, intGlitchMin, true);
                if (lviCur.IsGlitch)
                    intGlitchCount += 1;
                _lstResults.Add(lviCur);
            }

            lblResultsLabel.Visible = true;
            StringBuilder sbdResults = new StringBuilder();
            if (intGlitchCount >= intGlitchThreshold)
            {
                if (intHitCount > 0)
                {
                    if (nudThreshold.Value > 0)
                    {
                        sbdResults.Append(LanguageManager.GetString(intHitCount >= nudThreshold.Value ? "String_DiceRoller_Success" : "String_DiceRoller_Failure")
                                          + strSpace + '(');
                    }
                    sbdResults.AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("String_DiceRoller_Glitch"), intHitCount);
                    if (nudThreshold.Value > 0)
                        sbdResults.Append(')');
                }
                else
                    sbdResults.Append(LanguageManager.GetString("String_DiceRoller_CriticalGlitch"));
            }
            else if (nudThreshold.Value > 0)
            {
                sbdResults.Append(LanguageManager.GetString(intHitCount >= nudThreshold.Value ? "String_DiceRoller_Success" : "String_DiceRoller_Failure")
                                  + strSpace + '(' + string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_DiceRoller_Hits"), intHitCount) + ')');
            }
            else
                sbdResults.AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("String_DiceRoller_Hits"), intHitCount);

            sbdResults.Append(Environment.NewLine + Environment.NewLine +
                              LanguageManager.GetString("Label_DiceRoller_Sum") + strSpace +
                              _lstResults.Sum(x => x.Result).ToString(GlobalOptions.CultureInfo));
            lblResults.Text = sbdResults.ToString();

            lstResults.BeginUpdate();
            lstResults.Items.Clear();
            foreach (DiceRollerListViewItem objItem in _lstResults)
            {
                lstResults.Items.Add(objItem);
            }
            lstResults.EndUpdate();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Number of dice to roll.
        /// </summary>
        public int Dice
        {
            get => nudDice.ValueAsInt;
            set => nudDice.ValueAsInt = value;
        }

        /// <summary>
        /// List of Qualities the character has to determine whether or not they have Gremlins and at which Rating.
        /// </summary>
        public IEnumerable<Quality> Qualities
        {
            set
            {
                nudGremlins.Value = 0;
                if (value == null)
                    return;
                foreach (Quality objQuality in value)
                {
                    if (objQuality.Name.StartsWith("Gremlins", StringComparison.Ordinal))
                    {
                        nudGremlins.Value = objQuality.Levels;
                    }
                }
            }
        }
        #endregion

    }
}
