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
    public partial class DiceRoller : Form
    {
        private readonly ChummerMainForm _frmMain;
        private readonly List<DiceRollerListViewItem> _lstResults = new List<DiceRollerListViewItem>(40);

        #region Control Events

        public DiceRoller(ChummerMainForm frmMainForm, IEnumerable<Quality> lstQualities = null, int intDice = 1)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _frmMain = frmMainForm;
            nudDice.Value = intDice;
            ProcessGremlins(lstQualities);

            lblResultsLabel.Visible = false;
            lblResults.Text = string.Empty;
        }

        private async void DiceRoller_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMethod))
            {
                lstMethod.Add(new ListItem("Standard", await LanguageManager.GetStringAsync("String_DiceRoller_Standard")));
                lstMethod.Add(new ListItem("Large", await LanguageManager.GetStringAsync("String_DiceRoller_Large")));
                lstMethod.Add(new ListItem("ReallyLarge", await LanguageManager.GetStringAsync("String_DiceRoller_ReallyLarge")));

                cboMethod.BeginUpdate();
                cboMethod.PopulateWithListItems(lstMethod);
                cboMethod.SelectedIndex = 0;
                cboMethod.EndUpdate();
            }
        }

        private async void cmdRollDice_Click(object sender, EventArgs e)
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
                        intResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    int intResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
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

            string strSpace = await LanguageManager.GetStringAsync("String_Space");

            if (chkBubbleDie.Checked
                && (chkVariableGlitch.Checked
                    || (intGlitchCount == intGlitchThreshold - 1
                        && (nudDice.ValueAsInt & 1) == 0)))
            {
                int intBubbleDieResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intBubbleDieResult, intTarget, intGlitchMin, true);
                if (lviCur.IsGlitch)
                    ++intGlitchCount;
                _lstResults.Add(lviCur);
            }

            lblResultsLabel.Visible = true;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdResults))
            {
                if (intGlitchCount >= intGlitchThreshold)
                {
                    if (intHitCount > 0)
                    {
                        if (nudThreshold.Value > 0)
                        {
                            sbdResults.Append(await LanguageManager.GetStringAsync(
                                intHitCount >= nudThreshold.Value
                                    ? "String_DiceRoller_Success"
                                    : "String_DiceRoller_Failure")).Append(strSpace).Append('(');
                        }

                        sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync("String_DiceRoller_Glitch"), intHitCount);
                        if (nudThreshold.Value > 0)
                            sbdResults.Append(')');
                    }
                    else
                        sbdResults.Append(await LanguageManager.GetStringAsync("String_DiceRoller_CriticalGlitch"));
                }
                else if (nudThreshold.Value > 0)
                {
                    sbdResults
                        .Append(await LanguageManager.GetStringAsync(intHitCount >= nudThreshold.Value
                            ? "String_DiceRoller_Success"
                            : "String_DiceRoller_Failure")).Append(strSpace)
                        .Append('(')
                        .AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_DiceRoller_Hits"),
                                      intHitCount + ')');
                }
                else
                    sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("String_DiceRoller_Hits"), intHitCount);

                sbdResults.AppendLine().AppendLine().Append(await LanguageManager.GetStringAsync("Label_DiceRoller_Sum"))
                          .Append(strSpace).Append(_lstResults.Sum(x => x.Result).ToString(GlobalSettings.CultureInfo));
                lblResults.Text = sbdResults.ToString();
            }

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

        private void DiceRoller_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove the Main window's reference to this form.
            _frmMain.RollerWindow = null;
        }

        private async void cmdReroll_Click(object sender, EventArgs e)
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
                MessageBox.Show(await LanguageManager.GetStringAsync("String_NoDiceLeft_Text"), await LanguageManager.GetStringAsync("String_NoDiceLeft_Title"));
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
                        intLoopResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
                        lstRandom.Add(intLoopResult);
                    } while (intLoopResult == 6);
                }
                else
                {
                    int intLoopResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
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

            string strSpace = await LanguageManager.GetStringAsync("String_Space");

            if (chkBubbleDie.Checked
                && (chkVariableGlitch.Checked
                    || (intGlitchCount == intGlitchThreshold - 1
                        && (nudDice.ValueAsInt & 1) == 0)))
            {
                int intBubbleDieResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intBubbleDieResult, intTarget, intGlitchMin, true);
                if (lviCur.IsGlitch)
                    ++intGlitchCount;
                _lstResults.Add(lviCur);
            }

            lblResultsLabel.Visible = true;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdResults))
            {
                if (intGlitchCount >= intGlitchThreshold)
                {
                    if (intHitCount > 0)
                    {
                        if (nudThreshold.Value > 0)
                        {
                            sbdResults.Append(await LanguageManager.GetStringAsync(
                                intHitCount >= nudThreshold.Value
                                    ? "String_DiceRoller_Success"
                                    : "String_DiceRoller_Failure")).Append(strSpace).Append('(');
                        }

                        sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync("String_DiceRoller_Glitch"), intHitCount);
                        if (nudThreshold.Value > 0)
                            sbdResults.Append(')');
                    }
                    else
                        sbdResults.Append(await LanguageManager.GetStringAsync("String_DiceRoller_CriticalGlitch"));
                }
                else if (nudThreshold.Value > 0)
                {
                    sbdResults
                        .Append(await LanguageManager.GetStringAsync(intHitCount >= nudThreshold.Value
                            ? "String_DiceRoller_Success"
                            : "String_DiceRoller_Failure")).Append(strSpace)
                        .Append('(')
                        .AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_DiceRoller_Hits"),
                                      intHitCount).Append(')');
                }
                else
                    sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("String_DiceRoller_Hits"), intHitCount);

                sbdResults.AppendLine().AppendLine().Append(await LanguageManager.GetStringAsync("Label_DiceRoller_Sum"))
                          .Append(strSpace).Append(_lstResults.Sum(x => x.Result).ToString(GlobalSettings.CultureInfo));
                lblResults.Text = sbdResults.ToString();
            }

            lstResults.BeginUpdate();
            lstResults.Items.Clear();
            foreach (DiceRollerListViewItem objItem in _lstResults)
            {
                lstResults.Items.Add(objItem);
            }
            lstResults.EndUpdate();
        }

        #endregion Control Events

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
        /// Process whether or not a character has Gremlins and at which Rating.
        /// </summary>
        /// <param name="lstQualities">Qualities that the character has.</param>
        public void ProcessGremlins(IEnumerable<Quality> lstQualities)
        {
            Quality objGremlinsQuality
                = lstQualities?.FirstOrDefault(x => x.Name.StartsWith("Gremlins", StringComparison.Ordinal));
            nudGremlins.Value = objGremlinsQuality?.Levels ?? 0;
        }

        #endregion Properties
    }
}
