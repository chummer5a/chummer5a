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
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmDiceRoller : Form
    {
        private readonly frmMain _frmMain;

        #region Control Events
        public frmDiceRoller(frmMain frmMainForm, List<Quality> lstQualities = null, int intDice = 1)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _frmMain = frmMainForm;
            nudDice.Value = intDice;
            if (lstQualities != null)
            {
                foreach (Quality objQuality in lstQualities)
                {
                    if (objQuality.Name.StartsWith("Gremlins"))
                    {
                        int intRating = Convert.ToInt32(objQuality.Name.Substring(objQuality.Name.Length - 2, 1));
                        nudGremlins.Value = intRating;
                    }
                }
            }
            MoveControls();

            List<ListItem> lstMethod = new List<ListItem>();
            ListItem itmStandard = new ListItem();
            itmStandard.Value = "Standard";
            itmStandard.Name = LanguageManager.Instance.GetString("String_DiceRoller_Standard");

            ListItem itmLarge = new ListItem();
            itmLarge.Value = "Large";
            itmLarge.Name = LanguageManager.Instance.GetString("String_DiceRoller_Large");

            ListItem itmReallyLarge = new ListItem();
            itmReallyLarge.Value = "ReallyLarge";
            itmReallyLarge.Name = LanguageManager.Instance.GetString("String_DiceRoller_ReallyLarge");

            lstMethod.Add(itmStandard);
            lstMethod.Add(itmLarge);
            lstMethod.Add(itmReallyLarge);

            cboMethod.BeginUpdate();
            cboMethod.ValueMember = "Value";
            cboMethod.DisplayMember = "Name";
            cboMethod.DataSource = lstMethod;
            cboMethod.SelectedIndex = 0;
            cboMethod.EndUpdate();
        }

        private void cmdRollDice_Click(object sender, EventArgs e)
        {
            Random objRandom = new Random();
            List<int> lstRandom = new List<int>();
            int intHitCount = 0;
            int intGlitchCount = 0;
            int intGlitchThreshold = 0;
            int intGlitchMin = 1;

            intGlitchThreshold = Convert.ToInt32(Math.Ceiling(nudDice.Value / 2.0m));
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= Convert.ToInt32(nudGremlins.Value);
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            // If Rushed Job is checked, the minimum die result for a Glitch becomes 2.
            if (chkRushJob.Checked)
                intGlitchMin = 2;
            else
                intGlitchMin = 1;

            for (int intCounter = 1; intCounter <= nudDice.Value; intCounter++)
            {
                if (chkRuleOf6.Checked)
                {
                    int intResult = 0;
                    do
                    {
                        intResult = objRandom.Next(1, 7);
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    int intResult = objRandom.Next(1, 7);
                    lstRandom.Add(intResult);
                }
            }

            lstResults.Items.Clear();
            foreach (int intResult in lstRandom)
            {
                lstResults.Items.Add(intResult);

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

            lblResults.Text = LanguageManager.Instance.GetString("Label_DiceRoller_Result") + " ";
            if (intGlitchCount >= intGlitchThreshold)
            {
                if (intHitCount > 0)
                {
                    if (nudThreshold.Value > 0)
                    {
                        if (intHitCount >= nudThreshold.Value)
                            lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Success") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString()) + ")";
                        else
                            lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Failure") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString()) + ")";
                    }
                    else
                        lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString());
                }
                else
                    lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_CriticalGlitch");
            }
            else
            {
                if (nudThreshold.Value > 0)
                {
                    if (intHitCount >= nudThreshold.Value)
                        lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Success") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Hits").Replace("{0}", intHitCount.ToString()) + ")";
                    else
                        lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Failure") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Hits").Replace("{0}", intHitCount.ToString()) + ")";
                }
                else
                    lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Hits").Replace("{0}", intHitCount.ToString());
            }
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

            List<int> lstHits = new List<int>();
            foreach (int intResult in lstResults.Items)
            {
                if (intResult >= intKeepThreshold)
                    lstHits.Add(intResult);
            }

            Random objRandom = new Random();
            List<int> lstRandom = new List<int>();
            int intHitCount = lstHits.Count;
            int intGlitchCount = 0;
            int intGlitchThreshold = 0;

            intGlitchThreshold = Convert.ToInt32(Math.Ceiling(nudDice.Value / 2.0m));
            // Deduct the Gremlins Rating from the Glitch Threshold.
            intGlitchThreshold -= Convert.ToInt32(nudGremlins.Value);
            if (intGlitchThreshold < 1)
                intGlitchThreshold = 1;

            lstResults.Items.Clear();

            // Add any kept Hits back.
            foreach (int intResult in lstHits)
                lstResults.Items.Add(intResult);

            for (int intCounter = 1; intCounter <= nudDice.Value - lstHits.Count; intCounter++)
            {
                if (chkRuleOf6.Checked)
                {
                    int intResult = 0;
                    do
                    {
                        intResult = objRandom.Next(1, 7);
                        lstRandom.Add(intResult);
                    } while (intResult == 6);
                }
                else
                {
                    int intResult = objRandom.Next(1, 7);
                    lstRandom.Add(intResult);
                }
            }

            foreach (int intResult in lstRandom)
            {
                lstResults.Items.Add(intResult);

                if (cboMethod.SelectedValue.ToString() == "Standard")
                {
                    int intTarget = 5;
                    // If Cinematic Gameplay is turned on, Hits occur on 4, 5, or 6 instead.
                    if (chkCinematicGameplay.Checked)
                        intTarget = 4;

                    if (intResult >= intTarget)
                        intHitCount++;
                    if (intResult == 1)
                        intGlitchCount++;
                }
                else if (cboMethod.SelectedValue.ToString() == "Large")
                {
                    if (intResult >= 3)
                        intHitCount++;
                    if (intResult == 1)
                        intGlitchCount++;
                }
                else if (cboMethod.SelectedValue.ToString() == "ReallyLarge")
                {
                    intHitCount += intResult;
                }
            }

            lblResults.Text = LanguageManager.Instance.GetString("Label_DiceRoller_Result") + " ";
            if (intGlitchCount >= intGlitchThreshold)
            {
                if (intHitCount > 0)
                {
                    if (nudThreshold.Value > 0)
                    {
                        if (intHitCount >= nudThreshold.Value)
                            lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Success") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString()) + ")";
                        else
                            lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Failure") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString()) + ")";
                    }
                    else
                        lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString());
                }
                else
                    lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_CriticalGlitch");
            }
            else
            {
                if (nudThreshold.Value > 0)
                {
                    if (intHitCount >= nudThreshold.Value)
                        lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Success") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Hits").Replace("{0}", intHitCount.ToString()) + ")";
                    else
                        lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Failure") + " (" + LanguageManager.Instance.GetString("String_DiceRoller_Glitch").Replace("{0}", intHitCount.ToString()) + ")";
                }
                else
                    lblResults.Text += LanguageManager.Instance.GetString("String_DiceRoller_Hits").Replace("{0}", intHitCount.ToString());
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Number of dice to roll.
        /// </summary>
        public int Dice
        {
            set
            {
                nudDice.Value = value;
            }
        }

        /// <summary>
        /// List of Qualities the character has to determine whether or not they have Gremlins and at which Rating.
        /// </summary>
        public List<Quality> Qualities
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

        #region Methods
        private void MoveControls()
        {
            nudDice.Left = lblRoll.Left + lblRoll.Width + 6;
            lblD6.Left = nudDice.Left + nudDice.Width + 6;
            cboMethod.Left = lblD6.Left + lblD6.Width + 6;
            cmdRollDice.Left = cboMethod.Left + cboMethod.Width + 6;
            cmdReroll.Left = cmdRollDice.Left;

            int intMax = Math.Max(lblThreshold.Width, lblGremlins.Width);
            nudThreshold.Left = lblThreshold.Left + intMax + 6;
            nudGremlins.Left = lblGremlins.Left + intMax + 6;
            Width = cmdReroll.Left + cmdReroll.Width + 16;
        }
        #endregion
    }
}