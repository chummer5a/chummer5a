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
using System.Xml.XPath;

namespace Chummer
{
    public partial class CreateSpell : Form
    {
        private readonly XPathNavigator _objXmlDocument;
        private bool _blnLoading;
        private bool _blnSkipRefresh;
        private readonly Spell _objSpell;

        #region Control Events

        public CreateSpell(Character objCharacter)
        {
            _objSpell = new Spell(objCharacter);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = objCharacter.LoadDataXPath("spells.xml");
        }

        private void CreateSpell_Load(object sender, EventArgs e)
        {
            _blnLoading = true;
            lblDV.Text = 0.ToString(GlobalSettings.CultureInfo);

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCategory))
            {
                // Populate the list of Spell Categories.
                foreach (XPathNavigator objXmlCategory in _objXmlDocument.SelectAndCacheExpression(
                             "/chummer/categories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    lstCategory.Add(new ListItem(strInnerText,
                                                 objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                 ?? strInnerText));
                }

                cboCategory.BeginUpdate();
                cboType.BeginUpdate();
                cboRange.BeginUpdate();
                cboDuration.BeginUpdate();
                cboCategory.PopulateWithListItems(lstCategory);
                cboCategory.SelectedIndex = 0;
            }

            // Populate the list of Spell Types.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            {
                lstTypes.Add(new ListItem("P", LanguageManager.GetString("String_DescPhysical")));
                lstTypes.Add(new ListItem("M", LanguageManager.GetString("String_DescMana")));
                cboType.PopulateWithListItems(lstTypes);
            }

            cboType.SelectedIndex = 0;

            // Populate the list of Ranges.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstRanges))
            {
                lstRanges.Add(new ListItem("T", LanguageManager.GetString("String_SpellRangeTouchLong")));
                lstRanges.Add(new ListItem("LOS", LanguageManager.GetString("String_SpellRangeLineOfSight")));
                cboRange.PopulateWithListItems(lstRanges);
            }
            cboRange.SelectedIndex = 0;

            // Populate the list of Durations.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDurations))
            {
                lstDurations.Add(new ListItem("I", LanguageManager.GetString("String_SpellDurationInstantLong")));
                lstDurations.Add(new ListItem("P", LanguageManager.GetString("String_SpellDurationPermanentLong")));
                lstDurations.Add(new ListItem("S", LanguageManager.GetString("String_SpellDurationSustainedLong")));
                cboDuration.PopulateWithListItems(lstDurations);
            }

            cboDuration.SelectedIndex = 0;
            _blnLoading = false;
            cboCategory.EndUpdate();
            cboType.EndUpdate();
            cboRange.EndUpdate();
            cboDuration.EndUpdate();

            CalculateDrain();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCategory.SelectedValue.ToString() == "Health")
            {
                chkArea.Checked = false;
                chkArea.Enabled = false;
            }
            else
                chkArea.Enabled = true;

            ChangeModifiers();
            CalculateDrain();
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateDrain();
        }

        private void cboRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateDrain();
        }

        private void cboDuration_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateDrain();
        }

        private void chkModifier_CheckedChanged(object sender, EventArgs e)
        {
            cboType.Enabled = true;
            if (_blnSkipRefresh)
                return;

            switch (cboCategory.SelectedValue.ToString())
            {
                case "Combat":
                    {
                        // Direct and Indirect cannot be selected at the same time.
                        if (chkModifier1.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier2.Checked = false;
                            chkModifier2.Enabled = false;
                            chkModifier3.Checked = false;
                            chkModifier3.Enabled = false;
                            nudNumberOfEffects.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier2.Enabled = true;
                            chkModifier3.Enabled = true;
                            nudNumberOfEffects.Enabled = true;
                        }

                        // Indirect Combat Spells must always be physical. Direct and Indirect cannot be selected at the same time.
                        if (chkModifier2.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier1.Checked = false;
                            chkModifier1.Enabled = false;
                            cboType.SelectedValue = "P";
                            cboType.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier1.Enabled = true;
                        }

                        // Elemental effect spells must be Indirect (and consequently physical as well).
                        if (chkModifier3.Checked)
                        {
                            chkModifier2.Checked = true;
                        }

                        // Physical damage and Stun damage cannot be selected at the same time.
                        if (chkModifier4.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier5.Checked = false;
                            chkModifier5.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier5.Enabled = true;
                        }
                        if (chkModifier5.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier4.Checked = false;
                            chkModifier4.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier4.Enabled = true;
                        }

                        break;
                    }
                case "Detection":
                    {
                        // Directional, and Area cannot be selected at the same time.
                        if (chkModifier1.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier2.Checked = false;
                            chkModifier2.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        if (chkModifier2.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier1.Checked = false;
                            chkModifier1.Enabled = false;
                            _blnSkipRefresh = false;
                        }

                        // Active and Passive cannot be selected at the same time.
                        if (chkModifier4.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier5.Checked = false;
                            chkModifier5.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier5.Enabled = true;
                        }
                        if (chkModifier5.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier4.Checked = false;
                            chkModifier4.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier4.Enabled = true;
                        }

                        // If Extended Area is selected, Area must also be selected.
                        if (chkModifier14.Checked)
                        {
                            chkModifier1.Checked = false;
                            chkModifier3.Checked = false;
                            chkModifier2.Checked = true;
                        }

                        break;
                    }
                case "Health":
                    // Nothing special for Health Spells.
                    break;

                case "Illusion":
                    {
                        // Obvious and Realistic cannot be selected at the same time.
                        if (chkModifier1.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier2.Checked = false;
                            chkModifier2.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier2.Enabled = true;
                        }
                        if (chkModifier2.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier1.Checked = false;
                            chkModifier1.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier1.Enabled = true;
                        }

                        // Single-Sense and Multi-Sense cannot be selected at the same time.
                        if (chkModifier3.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier4.Checked = false;
                            chkModifier4.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier4.Enabled = true;
                        }
                        if (chkModifier4.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier3.Checked = false;
                            chkModifier3.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier3.Enabled = true;
                        }

                        break;
                    }
                case "Manipulation":
                    {
                        // Environmental, Mental, and Physical cannot be selected at the same time.
                        if (chkModifier1.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier2.Checked = false;
                            chkModifier2.Enabled = false;
                            chkModifier3.Checked = false;
                            chkModifier3.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        if (chkModifier2.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier1.Checked = false;
                            chkModifier1.Enabled = false;
                            chkModifier3.Checked = false;
                            chkModifier3.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        if (chkModifier3.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier1.Checked = false;
                            chkModifier1.Enabled = false;
                            chkModifier2.Checked = false;
                            chkModifier2.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        chkModifier1.Enabled = (!chkModifier2.Checked && !chkModifier3.Checked);
                        chkModifier2.Enabled = (!chkModifier1.Checked && !chkModifier3.Checked);
                        chkModifier3.Enabled = (!chkModifier1.Checked && !chkModifier2.Checked);

                        // Minor Change and Major Change cannot be selected at the same time.
                        if (chkModifier4.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier5.Checked = false;
                            chkModifier5.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier5.Enabled = true;
                        }
                        if (chkModifier5.Checked)
                        {
                            _blnSkipRefresh = true;
                            chkModifier4.Checked = false;
                            chkModifier4.Enabled = false;
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            chkModifier4.Enabled = true;
                        }

                        break;
                    }
            }

            CalculateDrain();
        }

        private void chkRestricted_CheckedChanged(object sender, EventArgs e)
        {
            chkVeryRestricted.Enabled = !chkRestricted.Checked;

            CalculateDrain();
            txtRestriction.Enabled = chkRestricted.Checked || chkVeryRestricted.Checked;
            if (!txtRestriction.Enabled)
                txtRestriction.Text = string.Empty;
        }

        private void chkVeryRestricted_CheckedChanged(object sender, EventArgs e)
        {
            chkRestricted.Enabled = !chkVeryRestricted.Checked;

            CalculateDrain();
            txtRestriction.Enabled = chkRestricted.Checked || chkVeryRestricted.Checked;
            if (!txtRestriction.Enabled)
                txtRestriction.Text = string.Empty;
        }

        private void nudNumberOfEffects_ValueChanged(object sender, EventArgs e)
        {
            CalculateDrain();
        }

        private void chkArea_CheckedChanged(object sender, EventArgs e)
        {
            CalculateDrain();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Re-calculate the Drain modifiers based on the currently-selected options.
        /// </summary>
        private void ChangeModifiers()
        {
            foreach (CheckBox chkCheckbox in flpModifiers.Controls.OfType<CheckBox>())
            {
                chkCheckbox.Enabled = true;
                chkCheckbox.Checked = false;
                chkCheckbox.Text = string.Empty;
                chkCheckbox.Tag = string.Empty;
                chkCheckbox.Visible = false;
            }
            foreach (Panel panChild in flpModifiers.Controls.OfType<Panel>())
            {
                foreach (CheckBox chkCheckbox in panChild.Controls.OfType<CheckBox>())
                {
                    chkCheckbox.Enabled = true;
                    chkCheckbox.Checked = false;
                    chkCheckbox.Text = string.Empty;
                    chkCheckbox.Tag = string.Empty;
                    chkCheckbox.Visible = false;
                }
            }
            nudNumberOfEffects.Visible = false;
            nudNumberOfEffects.Enabled = true;

            switch (cboCategory.SelectedValue.ToString())
            {
                case "Detection":
                    chkModifier1.Tag = "+0";
                    chkModifier1.Text = LanguageManager.GetString("Checkbox_DetectionSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.GetString("Checkbox_DetectionSpell2");
                    chkModifier3.Tag = "+0";
                    chkModifier3.Text = LanguageManager.GetString("Checkbox_DetectionSpell3");
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.GetString("Checkbox_DetectionSpell4");
                    chkModifier5.Tag = "+0";
                    chkModifier5.Text = LanguageManager.GetString("Checkbox_DetectionSpell5");
                    chkModifier6.Tag = "+0";
                    chkModifier6.Text = LanguageManager.GetString("Checkbox_DetectionSpell6");
                    chkModifier7.Tag = "+1";
                    chkModifier7.Text = LanguageManager.GetString("Checkbox_DetectionSpell7");
                    chkModifier8.Tag = "+1";
                    chkModifier8.Text = LanguageManager.GetString("Checkbox_DetectionSpell8");
                    chkModifier9.Tag = "+2";
                    chkModifier9.Text = LanguageManager.GetString("Checkbox_DetectionSpell9");
                    chkModifier10.Tag = "+4";
                    chkModifier10.Text = LanguageManager.GetString("Checkbox_DetectionSpell10");
                    chkModifier11.Tag = "+1";
                    chkModifier11.Text = LanguageManager.GetString("Checkbox_DetectionSpell11");
                    chkModifier12.Tag = "+2";
                    chkModifier12.Text = LanguageManager.GetString("Checkbox_DetectionSpell12");
                    chkModifier13.Tag = "+4";
                    chkModifier13.Text = LanguageManager.GetString("Checkbox_DetectionSpell13");
                    chkModifier14.Tag = "+2";
                    chkModifier14.Text = LanguageManager.GetString("Checkbox_DetectionSpell14");
                    break;

                case "Health":
                    chkModifier1.Tag = "+0";
                    chkModifier1.Text = LanguageManager.GetString("Checkbox_HealthSpell1");
                    chkModifier2.Tag = "+4";
                    chkModifier2.Text = LanguageManager.GetString("Checkbox_HealthSpell2");
                    chkModifier3.Tag = "-2";
                    chkModifier3.Text = LanguageManager.GetString("Checkbox_HealthSpell3");
                    chkModifier4.Tag = "+2";
                    chkModifier4.Text = LanguageManager.GetString("Checkbox_HealthSpell4");
                    chkModifier5.Tag = "-2";
                    chkModifier5.Text = LanguageManager.GetString("Checkbox_HealthSpell5");
                    break;

                case "Illusion":
                    chkModifier1.Tag = "-1";
                    chkModifier1.Text = LanguageManager.GetString("Checkbox_IllusionSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.GetString("Checkbox_IllusionSpell2");
                    chkModifier3.Tag = "-2";
                    chkModifier3.Text = LanguageManager.GetString("Checkbox_IllusionSpell3");
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.GetString("Checkbox_IllusionSpell4");
                    chkModifier5.Tag = "+2";
                    chkModifier5.Text = LanguageManager.GetString("Checkbox_IllusionSpell5");
                    break;

                case "Manipulation":
                    chkModifier1.Tag = "-2";
                    chkModifier1.Text = LanguageManager.GetString("Checkbox_ManipulationSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.GetString("Checkbox_ManipulationSpell2");
                    chkModifier3.Tag = "+0";
                    chkModifier3.Text = LanguageManager.GetString("Checkbox_ManipulationSpell3");
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.GetString("Checkbox_ManipulationSpell4");
                    chkModifier5.Tag = "+2";
                    chkModifier5.Text = LanguageManager.GetString("Checkbox_ManipulationSpell5");
                    chkModifier6.Tag = "+2";
                    chkModifier6.Text = LanguageManager.GetString("Checkbox_ManipulationSpell6");
                    nudNumberOfEffects.Visible = true;
                    nudNumberOfEffects.Top = chkModifier6.Top - 1;
                    break;

                default:
                    // Combat.
                    chkModifier1.Tag = "+0";
                    chkModifier1.Text = LanguageManager.GetString("Checkbox_CombatSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.GetString("Checkbox_CombatSpell2");
                    chkModifier3.Tag = "+2";
                    chkModifier3.Text = LanguageManager.GetString("Checkbox_CombatSpell3");
                    nudNumberOfEffects.Visible = true;
                    nudNumberOfEffects.Top = chkModifier3.Top - 1;
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.GetString("Checkbox_CombatSpell4");
                    chkModifier5.Tag = "-1";
                    chkModifier5.Text = LanguageManager.GetString("Checkbox_CombatSpell5");
                    break;
            }

            string strCheckBoxFormat = LanguageManager.GetString("String_Space") + "({0})";
            foreach (Control objControl in flpModifiers.Controls)
            {
                switch (objControl)
                {
                    case CheckBox chkCheckbox:
                    {
                        if (!string.IsNullOrEmpty(chkCheckbox.Text))
                        {
                            chkCheckbox.Visible = true;
                            chkCheckbox.Text += string.Format(GlobalSettings.CultureInfo, strCheckBoxFormat, chkCheckbox.Tag);
                        }

                        break;
                    }
                    case Panel pnlControl:
                    {
                        foreach (CheckBox chkInnerCheckbox in pnlControl.Controls.OfType<CheckBox>())
                        {
                            if (!string.IsNullOrEmpty(chkInnerCheckbox.Text))
                            {
                                chkInnerCheckbox.Visible = true;
                                chkInnerCheckbox.Text += string.Format(GlobalSettings.CultureInfo, strCheckBoxFormat, chkInnerCheckbox.Tag);
                            }
                        }

                        break;
                    }
                }
            }

            if (nudNumberOfEffects.Visible)
            {
                switch (cboCategory.SelectedValue.ToString())
                {
                    case "Combat":
                        nudNumberOfEffects.Left = chkModifier3.Left + chkModifier3.Width + 6;
                        break;

                    case "Manipulation":
                        nudNumberOfEffects.Left = chkModifier6.Left + chkModifier6.Width + 6;
                        break;
                }
            }
        }

        /// <summary>
        /// Calculate the Spell's Drain Value based on the currently-selected options.
        /// </summary>
        private string CalculateDrain()
        {
            if (_blnLoading)
                return string.Empty;

            int intDV = 0;

            // Type DV.
            if (cboType.SelectedValue.ToString() != "M")
                ++intDV;

            // Range DV.
            if (cboRange.SelectedValue.ToString() == "T")
                intDV -= 2;

            if (chkArea.Checked)
                intDV += 2;

            // Restriction DV.
            if (chkRestricted.Checked)
                --intDV;
            if (chkVeryRestricted.Checked)
                intDV -= 2;

            // Duration DV.
            // Curative Health Spells do not have a modifier for Permanent duration.
            if (cboDuration.SelectedValue.ToString() == "P" && (cboCategory.SelectedValue.ToString() != "Health" || !chkModifier1.Checked))
                intDV += 2;

            // Include any checked modifiers.
            foreach (CheckBox chkModifier in flpModifiers.Controls.OfType<CheckBox>())
            {
                if (chkModifier.Visible && chkModifier.Checked)
                {
                    if (chkModifier == chkModifier3 && cboCategory.SelectedValue.ToString() == "Combat")
                        intDV += (Convert.ToInt32(chkModifier.Tag.ToString(), GlobalSettings.InvariantCultureInfo) * nudNumberOfEffects.ValueAsInt);
                    else if (chkModifier == chkModifier6 && cboCategory.SelectedValue.ToString() == "Manipulation")
                        intDV += (Convert.ToInt32(chkModifier.Tag.ToString(), GlobalSettings.InvariantCultureInfo) * nudNumberOfEffects.ValueAsInt);
                    else
                        intDV += Convert.ToInt32(chkModifier.Tag.ToString(), GlobalSettings.InvariantCultureInfo);
                }
            }
            foreach (Panel panChild in flpModifiers.Controls.OfType<Panel>())
            {
                foreach (CheckBox chkModifier in panChild.Controls.OfType<CheckBox>())
                {
                    if (chkModifier.Visible && chkModifier.Checked)
                    {
                        if (chkModifier == chkModifier3 && cboCategory.SelectedValue.ToString() == "Combat")
                            intDV += (Convert.ToInt32(chkModifier.Tag.ToString(), GlobalSettings.InvariantCultureInfo) * nudNumberOfEffects.ValueAsInt);
                        else if (chkModifier == chkModifier6 && cboCategory.SelectedValue.ToString() == "Manipulation")
                            intDV += (Convert.ToInt32(chkModifier.Tag.ToString(), GlobalSettings.InvariantCultureInfo) * nudNumberOfEffects.ValueAsInt);
                        else
                            intDV += Convert.ToInt32(chkModifier.Tag.ToString(), GlobalSettings.InvariantCultureInfo);
                    }
                }
            }

            string strBase;
            if (cboCategory.SelectedValue.ToString() == "Health" && chkModifier1.Checked)
            {
                // Health Spells use (Damage Value) as their base.
                strBase = '(' + LanguageManager.GetString("String_SpellDamageValue") + ')';
            }
            else
            {
                // All other spells use (F/2) as their base.
                strBase = "(F/2)";
            }

            string strDV = intDV.ToString(GlobalSettings.InvariantCultureInfo);
            if (intDV > 0)
                strDV = '+' + strDV;
            if (intDV == 0)
                strDV = string.Empty;
            lblDV.Text = (strBase + strDV).Replace('/', '÷').Replace('*', '×')
                .CheapReplace("F", () => LanguageManager.GetString("String_SpellForce"))
                .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue"));

            return strBase + strDV;
        }

        /// <summary>
        /// Accept the values of the form.
        /// </summary>
        private void AcceptForm()
        {
            string strMessage = string.Empty;
            // Make sure a name has been provided.
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += Environment.NewLine;
                strMessage += LanguageManager.GetString("Message_SpellName");
            }

            // Make sure a Restricted value if the field is enabled.
            if (txtRestriction.Enabled && string.IsNullOrWhiteSpace(txtRestriction.Text))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += Environment.NewLine;
                strMessage += LanguageManager.GetString("Message_SpellRestricted");
            }

            switch (cboCategory.SelectedValue.ToString())
            {
                // Make sure the Spell has met all of its requirements.
                case "Combat":
                    {
                        // Either Direct or Indirect must be selected.
                        if (!chkModifier1.Checked && !chkModifier2.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_CombatSpellRequirement1");
                        }

                        // Either Physical damage or Stun damage must be selected.
                        if (!chkModifier4.Checked && !chkModifier5.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_CombatSpellRequirement2");
                        }

                        break;
                    }
                case "Detection":
                    {
                        // Either Directional, Area, or Psychic must be selected.
                        if (!chkModifier1.Checked && !chkModifier2.Checked && !chkModifier3.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_DetectionSpellRequirement1");
                        }

                        // Either Active or Passive must be selected.
                        if (!chkModifier4.Checked && !chkModifier5.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_DetectionSpellRequirement2");
                        }

                        break;
                    }
                case "Health":
                    // Nothing special.
                    break;

                case "Illusion":
                    {
                        // Either Obvious or Realistic must be selected.
                        if (!chkModifier1.Checked && !chkModifier2.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_IllusionSpellRequirement1");
                        }

                        // Either Single-Sense or Multi-Sense must be selected.
                        if (!chkModifier3.Checked && !chkModifier4.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_IllusionSpellRequirement2");
                        }

                        break;
                    }
                case "Manipulation":
                    {
                        // Either Environmental, Mental, or Physical must be selected.
                        if (!chkModifier1.Checked && !chkModifier2.Checked && !chkModifier3.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_ManipulationSpellRequirement1");
                        }

                        // Either Minor Change or Major Change must be selected.
                        if (!chkModifier4.Checked && !chkModifier5.Checked)
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += LanguageManager.GetString("Message_ManipulationSpellRequirement2");
                        }

                        break;
                    }
            }

            // Show the message if necessary.
            if (!string.IsNullOrEmpty(strMessage))
            {
                Program.MainForm.ShowMessageBox(this, strMessage, LanguageManager.GetString("Title_CreateSpell"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string strRange = cboRange.SelectedValue.ToString();
            if (chkArea.Checked)
                strRange += "(A)";

            // If we're made it this far, everything is OK, so create the Spell.
            string strDescriptors = string.Empty;
            switch (cboCategory.SelectedValue.ToString())
            {
                case "Detection":
                    if (chkModifier4.Checked)
                        strDescriptors += "Active, ";
                    if (chkModifier5.Checked)
                        strDescriptors += "Passive, ";
                    if (chkModifier1.Checked)
                        strDescriptors += "Directional, ";
                    if (chkModifier3.Checked)
                        strDescriptors += "Psychic, ";
                    if (chkModifier2.Checked)
                    {
                        if (!chkModifier14.Checked)
                            strDescriptors += "Area, ";
                        else
                            strDescriptors += "Extended Area, ";
                    }
                    break;

                case "Health":
                    if (chkModifier4.Checked)
                        strDescriptors += "Negative, ";
                    break;

                case "Illusion":
                    if (chkModifier1.Checked)
                        strDescriptors += "Obvious, ";
                    if (chkModifier2.Checked)
                        strDescriptors += "Realistic, ";
                    if (chkModifier3.Checked)
                        strDescriptors += "Single-Sense, ";
                    if (chkModifier4.Checked)
                        strDescriptors += "Multi-Sense, ";
                    if (chkArea.Checked)
                        strDescriptors += "Area, ";
                    break;

                case "Manipulation":
                    if (chkModifier1.Checked)
                        strDescriptors += "Environmental, ";
                    if (chkModifier2.Checked)
                        strDescriptors += "Mental, ";
                    if (chkModifier3.Checked)
                        strDescriptors += "Physical, ";
                    if (chkArea.Checked)
                        strDescriptors += "Area, ";
                    break;

                default:
                    // Combat.
                    if (chkModifier1.Checked)
                        strDescriptors += "Direct, ";
                    if (chkModifier2.Checked)
                        strDescriptors += "Indirect, ";
                    if (cboRange.SelectedValue.ToString().Contains("(A)"))
                        strDescriptors += "Area, ";
                    if (chkModifier3.Checked)
                        strDescriptors += "Elemental, ";
                    break;
            }

            // Remove the trailing ", " from the Descriptors string.
            if (!string.IsNullOrEmpty(strDescriptors))
                strDescriptors = strDescriptors.Substring(0, strDescriptors.Length - 2);

            _objSpell.Name = txtName.Text;
            _objSpell.Source = "SM";
            _objSpell.Page = "159";
            _objSpell.Category = cboCategory.SelectedValue.ToString();
            _objSpell.Descriptors = strDescriptors;
            _objSpell.Range = strRange;
            _objSpell.Type = cboType.SelectedValue.ToString();
            _objSpell.Limited = chkLimited.Checked;
            if (cboCategory.SelectedValue.ToString() == "Combat")
            {
                _objSpell.Damage = chkModifier4.Checked ? "P" : "S";
            }
            _objSpell.DvBase = CalculateDrain();
            if (!string.IsNullOrEmpty(txtRestriction.Text))
                _objSpell.Extra = txtRestriction.Text;
            _objSpell.Duration = cboDuration.SelectedValue.ToString();

            DialogResult = DialogResult.OK;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Spell that was created in the dialogue.
        /// </summary>
        public Spell SelectedSpell => _objSpell;

        #endregion Properties
    }
}
