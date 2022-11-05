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
                lstMethod.Add(new ListItem("Standard", await LanguageManager.GetStringAsync("String_DiceRoller_Standard").ConfigureAwait(false)));
                lstMethod.Add(new ListItem("Large", await LanguageManager.GetStringAsync("String_DiceRoller_Large").ConfigureAwait(false)));
                lstMethod.Add(new ListItem("ReallyLarge", await LanguageManager.GetStringAsync("String_DiceRoller_ReallyLarge").ConfigureAwait(false)));

                await cboMethod.PopulateWithListItemsAsync(lstMethod).ConfigureAwait(false);
                await cboMethod.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);
            }
        }

        private async void cmdRollDice_Click(object sender, EventArgs e)
        {
            List<int> lstRandom = new List<int>(await nudDice.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false));
            int intGlitchMin = 1;

            // If Rushed Job is checked, the minimum die result for a Glitch becomes 2.
            if (await chkRushJob.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                intGlitchMin = 2;

            int intTarget = 5;
            // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
            if (await chkCinematicGameplay.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                intTarget = 4;
            switch (await cboMethod.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false))
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

            int intDice = await nudDice.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            for (int intCounter = 1; intCounter <= intDice; intCounter++)
            {
                if (await chkRuleOf6.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                {
                    int intResult;
                    do
                    {
                        intResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    int intResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                    lstRandom.Add(intResult);
                }
            }

            _lstResults.Clear();
            foreach (int intResult in lstRandom)
            {
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intResult, intTarget, intGlitchMin);
                _lstResults.Add(lviCur);
            }

            int intHitCount = await cboMethod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) == "ReallyLarge" ? _lstResults.Sum(x => x.Result) : _lstResults.Count(x => x.IsHit);
            int intGlitchCount = _lstResults.Count(x => x.IsGlitch);

            int intGlitchThreshold = await chkVariableGlitch.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                ? intHitCount + 1
                : (intDice + 1) / 2;
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= await nudGremlins.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);

            if (await chkBubbleDie.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                && (await chkVariableGlitch.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                    || (intGlitchCount == intGlitchThreshold - 1
                        && (intDice & 1) == 0)))
            {
                int intBubbleDieResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intBubbleDieResult, intTarget, intGlitchMin, true);
                if (lviCur.IsGlitch)
                    ++intGlitchCount;
                _lstResults.Add(lviCur);
            }

            await lblResultsLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdResults))
            {
                if (intGlitchCount >= intGlitchThreshold)
                {
                    if (intHitCount > 0)
                    {
                        int intThreshold = await nudThreshold.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
                        if (intThreshold > 0)
                        {
                            sbdResults.Append(await LanguageManager.GetStringAsync(
                                                  intHitCount >= intThreshold
                                                      ? "String_DiceRoller_Success"
                                                      : "String_DiceRoller_Failure").ConfigureAwait(false)).Append(strSpace).Append('(');
                        }

                        sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync(intHitCount == 1 ? "String_DiceRoller_Glitch_Singular" : "String_DiceRoller_Glitch").ConfigureAwait(false), intHitCount);
                        if (intThreshold > 0)
                            sbdResults.Append(')');
                    }
                    else
                        sbdResults.Append(await LanguageManager.GetStringAsync("String_DiceRoller_CriticalGlitch").ConfigureAwait(false));
                }
                else
                {
                    int intThreshold = await nudThreshold.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
                    if (intThreshold > 0)
                    {
                        sbdResults
                            .Append(await LanguageManager.GetStringAsync(intHitCount >= intThreshold
                                                                             ? "String_DiceRoller_Success"
                                                                             : "String_DiceRoller_Failure").ConfigureAwait(false)).Append(strSpace)
                            .Append('(')
                            .AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync(intHitCount == 1 ? "String_DiceRoller_Hit" : "String_DiceRoller_Hits").ConfigureAwait(false),
                                          intHitCount + ')');
                    }
                    else
                        sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync(intHitCount == 1 ? "String_DiceRoller_Hit" : "String_DiceRoller_Hits").ConfigureAwait(false), intHitCount);
                }

                sbdResults.AppendLine().AppendLine().Append(await LanguageManager.GetStringAsync("Label_DiceRoller_Sum").ConfigureAwait(false))
                          .Append(strSpace).Append(_lstResults.Sum(x => x.Result).ToString(GlobalSettings.CultureInfo));
                await lblResults.DoThreadSafeAsync(x => x.Text = sbdResults.ToString()).ConfigureAwait(false);
            }

            await lstResults.DoThreadSafeAsync(x =>
            {
                x.BeginUpdate();
                try
                {
                    x.Items.Clear();
                    foreach (DiceRollerListViewItem objItem in _lstResults)
                    {
                        x.Items.Add(objItem);
                    }
                }
                finally
                {
                    x.EndUpdate();
                }
            }).ConfigureAwait(false);
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
            if (await chkRushJob.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                intGlitchMin = 2;

            int intTarget = 5;
            // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
            if (await chkCinematicGameplay.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                intTarget = 4;
            switch (await cboMethod.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false))
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
                await objItem.SetTargetAsync(intTarget);
                await objItem.SetGlitchMinAsync(intGlitchMin);
            }

            // Remove everything that is not a hit
            int intNewDicePool = _lstResults.Count(x => !x.IsHit);
            _lstResults.RemoveAll(x => !x.IsHit);

            if (intNewDicePool == 0)
            {
                MessageBox.Show(await LanguageManager.GetStringAsync("String_NoDiceLeft_Text").ConfigureAwait(false), await LanguageManager.GetStringAsync("String_NoDiceLeft_Title").ConfigureAwait(false));
                return;
            }

            int intHitCount = await cboMethod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) == "ReallyLarge" ? _lstResults.Sum(x => x.Result) : _lstResults.Count(x => x.IsHit);
            int intGlitchCount = _lstResults.Count(x => x.IsGlitch);
            List<int> lstRandom = new List<int>(intNewDicePool);

            for (int intCounter = 1; intCounter <= intNewDicePool; intCounter++)
            {
                if (await chkRuleOf6.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                {
                    int intLoopResult;
                    do
                    {
                        intLoopResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                        lstRandom.Add(intLoopResult);
                    } while (intLoopResult == 6);
                }
                else
                {
                    int intLoopResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                    lstRandom.Add(intLoopResult);
                }
            }

            foreach (int intResult in lstRandom)
            {
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intResult, intTarget, intGlitchMin);
                _lstResults.Add(lviCur);
            }

            int intGlitchThreshold = await chkVariableGlitch.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                ? intHitCount + 1
                : (_lstResults.Count + 1) / 2;
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= await nudGremlins.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);

            if (await chkBubbleDie.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                && (await chkVariableGlitch.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                    || (intGlitchCount == intGlitchThreshold - 1
                        && (_lstResults.Count & 1) == 0)))
            {
                int intBubbleDieResult = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                DiceRollerListViewItem lviCur = new DiceRollerListViewItem(intBubbleDieResult, intTarget, intGlitchMin, true);
                if (lviCur.IsGlitch)
                    ++intGlitchCount;
                _lstResults.Add(lviCur);
            }

            await lblResultsLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdResults))
            {
                if (intGlitchCount >= intGlitchThreshold)
                {
                    if (intHitCount > 0)
                    {
                        int intThreshold = await nudThreshold.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
                        if (intThreshold > 0)
                        {
                            sbdResults.Append(await LanguageManager.GetStringAsync(
                                                  intHitCount >= intThreshold
                                                      ? "String_DiceRoller_Success"
                                                      : "String_DiceRoller_Failure").ConfigureAwait(false)).Append(strSpace).Append('(');
                        }

                        sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync(intHitCount == 1 ? "String_DiceRoller_Glitch_Singular" : "String_DiceRoller_Glitch").ConfigureAwait(false), intHitCount);
                        if (intThreshold > 0)
                            sbdResults.Append(')');
                    }
                    else
                        sbdResults.Append(await LanguageManager.GetStringAsync("String_DiceRoller_CriticalGlitch").ConfigureAwait(false));
                }
                else
                {
                    int intThreshold = await nudThreshold.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
                    if (intThreshold > 0)
                    {
                        sbdResults
                            .Append(await LanguageManager.GetStringAsync(intHitCount >= intThreshold
                                                                             ? "String_DiceRoller_Success"
                                                                             : "String_DiceRoller_Failure").ConfigureAwait(false)).Append(strSpace)
                            .Append('(')
                            .AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync(intHitCount == 1 ? "String_DiceRoller_Hit" : "String_DiceRoller_Hits").ConfigureAwait(false),
                                          intHitCount).Append(')');
                    }
                    else
                        sbdResults.AppendFormat(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync(intHitCount == 1 ? "String_DiceRoller_Hit" : "String_DiceRoller_Hits").ConfigureAwait(false), intHitCount);
                }

                sbdResults.AppendLine().AppendLine().Append(await LanguageManager.GetStringAsync("Label_DiceRoller_Sum").ConfigureAwait(false))
                          .Append(strSpace).Append(_lstResults.Sum(x => x.Result).ToString(GlobalSettings.CultureInfo));
                await lblResults.DoThreadSafeAsync(x => x.Text = sbdResults.ToString()).ConfigureAwait(false);
            }

            await lstResults.DoThreadSafeAsync(x =>
            {
                x.BeginUpdate();
                try
                {
                    x.Items.Clear();
                    foreach (DiceRollerListViewItem objItem in _lstResults)
                    {
                        x.Items.Add(objItem);
                    }
                }
                finally
                {
                    x.EndUpdate();
                }
            }).ConfigureAwait(false);
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
            nudGremlins.DoThreadSafe(x => x.ValueAsInt = objGremlinsQuality?.Levels ?? 0);
        }

        #endregion Properties
    }
}
