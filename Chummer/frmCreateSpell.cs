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
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmCreateSpell : Form
    {
        private readonly XmlDocument _objXmlDocument = new XmlDocument();
        private bool _blnLoading = false;
        private bool _blnSkipRefresh = false;
        private Spell _objSpell;

        #region Control Events
        public frmCreateSpell(Character objCharacter)
        {
            _objSpell = new Spell(objCharacter);
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objXmlDocument = XmlManager.Instance.Load("spells.xml");
            MoveControls();
        }

        private void frmCreateSpell_Load(object sender, EventArgs e)
        {
            _blnLoading = true;
            lblDV.Text = "0";

            List<ListItem> lstCategory = new List<ListItem>();

            // Populate the list of Spell Categories.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                lstCategory.Add(objItem);
            }
            cboCategory.BeginUpdate();
            cboType.BeginUpdate();
            cboRange.BeginUpdate();
            cboDuration.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstCategory;
            cboCategory.SelectedIndex = 0;

            // Populate the list of Spell Types.
            ListItem itmPhysical = new ListItem();
            itmPhysical.Value = "P";
            itmPhysical.Name = LanguageManager.Instance.GetString("String_DescPhysical");
            ListItem itmMana = new ListItem();
            itmMana.Value = "M";
            itmMana.Name = LanguageManager.Instance.GetString("String_DescMana");
            List<ListItem> lstTypes = new List<ListItem>();
            lstTypes.Add(itmPhysical);
            lstTypes.Add(itmMana);
            cboType.ValueMember = "Value";
            cboType.DisplayMember = "Name";
            cboType.DataSource = lstTypes;
            cboType.SelectedIndex = 0;

            // Populate the list of Ranges.
            ListItem itmTouch = new ListItem();
            itmTouch.Value = "T";
            itmTouch.Name = LanguageManager.Instance.GetString("String_SpellRangeTouchLong");
            ListItem itmLOS = new ListItem();
            itmLOS.Value = "LOS";
            itmLOS.Name = LanguageManager.Instance.GetString("String_SpellRangeLineOfSight");
            List<ListItem> lstRanges = new List<ListItem>();
            lstRanges.Add(itmTouch);
            lstRanges.Add(itmLOS);
            cboRange.ValueMember = "Value";
            cboRange.DisplayMember = "Name";
            cboRange.DataSource = lstRanges;
            cboRange.SelectedIndex = 0;

            // Populate the list of Durations.
            ListItem itmInstant = new ListItem();
            itmInstant.Value = "I";
            itmInstant.Name = LanguageManager.Instance.GetString("String_SpellDurationInstantLong");
            ListItem itmPermanent = new ListItem();
            itmPermanent.Value = "P";
            itmPermanent.Name = LanguageManager.Instance.GetString("String_SpellDurationPermanentLong");
            ListItem itmSustained = new ListItem();
            itmSustained.Value = "S";
            itmSustained.Name = LanguageManager.Instance.GetString("String_SpellDurationSustainedLong");
            List<ListItem> lstDurations = new List<ListItem>();
            lstDurations.Add(itmInstant);
            lstDurations.Add(itmPermanent);
            lstDurations.Add(itmSustained);
            cboDuration.ValueMember = "Value";
            cboDuration.DisplayMember = "Name";
            cboDuration.DataSource = lstDurations;
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

            if (cboCategory.SelectedValue.ToString() == "Combat")
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
            }
            else if (cboCategory.SelectedValue.ToString() == "Detection")
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
            }
            else if (cboCategory.SelectedValue.ToString() == "Health")
            {
                // Nothing special for Health Spells.
            }
            else if (cboCategory.SelectedValue.ToString() == "Illusion")
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
            }
            else if (cboCategory.SelectedValue.ToString() == "Manipulation")
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
            }

            CalculateDrain();
        }

        private void chkRestricted_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRestricted.Checked)
                chkVeryRestricted.Enabled = false;
            else
                chkVeryRestricted.Enabled = true;

            CalculateDrain();
            txtRestriction.Enabled = chkRestricted.Checked || chkVeryRestricted.Checked;
            if (!txtRestriction.Enabled)
                txtRestriction.Text = string.Empty;
        }

        private void chkVeryRestricted_CheckedChanged(object sender, EventArgs e)
        {
            if (chkVeryRestricted.Checked)
                chkRestricted.Enabled = false;
            else
                chkRestricted.Enabled = true;

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
        #endregion

        #region Methods
        /// <summary>
        /// Re-calculate the Drain modifiers based on the currently-selected options.
        /// </summary>
        private void ChangeModifiers()
        {
            foreach (CheckBox chkCheckbox in panModifiers.Controls.OfType<CheckBox>())
            {
                chkCheckbox.Enabled = true;
                chkCheckbox.Checked = false;
                chkCheckbox.Text = string.Empty;
                chkCheckbox.Tag = string.Empty;
                chkCheckbox.Visible = false;
            }
            nudNumberOfEffects.Visible = false;
            nudNumberOfEffects.Enabled = true;

            switch (cboCategory.SelectedValue.ToString())
            {
                case "Detection":
                    chkModifier1.Tag = "+0";
                    chkModifier1.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell2");
                    chkModifier3.Tag = "+0";
                    chkModifier3.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell3");
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell4");
                    chkModifier5.Tag = "+0";
                    chkModifier5.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell5");
                    chkModifier6.Tag = "+0";
                    chkModifier6.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell6");
                    chkModifier7.Tag = "+1";
                    chkModifier7.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell7");
                    chkModifier8.Tag = "+1";
                    chkModifier8.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell8");
                    chkModifier9.Tag = "+2";
                    chkModifier9.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell9");
                    chkModifier10.Tag = "+4";
                    chkModifier10.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell10");
                    chkModifier11.Tag = "+1";
                    chkModifier11.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell11");
                    chkModifier12.Tag = "+2";
                    chkModifier12.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell12");
                    chkModifier13.Tag = "+4";
                    chkModifier13.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell13");
                    chkModifier14.Tag = "+2";
                    chkModifier14.Text = LanguageManager.Instance.GetString("Checkbox_DetectionSpell14");
                    break;
                case "Health":
                    chkModifier1.Tag = "+0";
                    chkModifier1.Text = LanguageManager.Instance.GetString("Checkbox_HealthSpell1");
                    chkModifier2.Tag = "+4";
                    chkModifier2.Text = LanguageManager.Instance.GetString("Checkbox_HealthSpell2");
                    chkModifier3.Tag = "-2";
                    chkModifier3.Text = LanguageManager.Instance.GetString("Checkbox_HealthSpell3");
                    chkModifier4.Tag = "+2";
                    chkModifier4.Text = LanguageManager.Instance.GetString("Checkbox_HealthSpell4");
                    chkModifier5.Tag = "-2";
                    chkModifier5.Text = LanguageManager.Instance.GetString("Checkbox_HealthSpell5");
                    break;
                case "Illusion":
                    chkModifier1.Tag = "-1";
                    chkModifier1.Text = LanguageManager.Instance.GetString("Checkbox_IllusionSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.Instance.GetString("Checkbox_IllusionSpell2");
                    chkModifier3.Tag = "-2";
                    chkModifier3.Text = LanguageManager.Instance.GetString("Checkbox_IllusionSpell3");
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.Instance.GetString("Checkbox_IllusionSpell4");
                    chkModifier5.Tag = "+2";
                    chkModifier5.Text = LanguageManager.Instance.GetString("Checkbox_IllusionSpell5");
                    break;
                case "Manipulation":
                    chkModifier1.Tag = "-2";
                    chkModifier1.Text = LanguageManager.Instance.GetString("Checkbox_ManipulationSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.Instance.GetString("Checkbox_ManipulationSpell2");
                    chkModifier3.Tag = "+0";
                    chkModifier3.Text = LanguageManager.Instance.GetString("Checkbox_ManipulationSpell3");
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.Instance.GetString("Checkbox_ManipulationSpell4");
                    chkModifier5.Tag = "+2";
                    chkModifier5.Text = LanguageManager.Instance.GetString("Checkbox_ManipulationSpell5");
                    chkModifier6.Tag = "+2";
                    chkModifier6.Text = LanguageManager.Instance.GetString("Checkbox_ManipulationSpell6");
                    nudNumberOfEffects.Visible = true;
                    nudNumberOfEffects.Top = chkModifier6.Top - 1;
                    break;
                default:
                    // Combat.
                    chkModifier1.Tag = "+0";
                    chkModifier1.Text = LanguageManager.Instance.GetString("Checkbox_CombatSpell1");
                    chkModifier2.Tag = "+0";
                    chkModifier2.Text = LanguageManager.Instance.GetString("Checkbox_CombatSpell2");
                    chkModifier3.Tag = "+2";
                    chkModifier3.Text = LanguageManager.Instance.GetString("Checkbox_CombatSpell3");
                    nudNumberOfEffects.Visible = true;
                    nudNumberOfEffects.Top = chkModifier3.Top - 1;
                    chkModifier4.Tag = "+0";
                    chkModifier4.Text = LanguageManager.Instance.GetString("Checkbox_CombatSpell4");
                    chkModifier5.Tag = "-1";
                    chkModifier5.Text = LanguageManager.Instance.GetString("Checkbox_CombatSpell5");
                    break;
            }

            foreach (CheckBox chkCheckbox in panModifiers.Controls.OfType<CheckBox>())
            {
                if (!string.IsNullOrEmpty(chkCheckbox.Text))
                {
                    chkCheckbox.Visible = true;
                    chkCheckbox.Text += " (" + chkCheckbox.Tag + ")";
                }
            }

            if (nudNumberOfEffects.Visible)
            {
                if (cboCategory.SelectedValue.ToString() == "Combat")
                    nudNumberOfEffects.Left = chkModifier3.Left + chkModifier3.Width + 6;
                else if (cboCategory.SelectedValue.ToString() == "Manipulation")
                    nudNumberOfEffects.Left = chkModifier6.Left + chkModifier6.Width + 6;
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
            if (cboType.SelectedValue.ToString() == "M")
                intDV += 0;
            else
                intDV += 1;

            // Range DV.
            switch (cboRange.SelectedValue.ToString())
            {
                case "T":
                    intDV -= 2;
                    break;
                default:
                    // LOS
                    intDV += 0;
                    break;
            }
            if (chkArea.Checked)
                intDV += 2;

            // Restriction DV.
            if (chkRestricted.Checked)
                intDV -= 1;
            if (chkVeryRestricted.Checked)
                intDV -= 2;

            // Duration DV.
            if (cboDuration.SelectedValue.ToString() == "P")
            {
                // Curative Health Spells do not have a modifier for Permanant duration.
                if (cboCategory.SelectedValue.ToString() == "Health" && chkModifier1.Checked)
                    intDV += 0;
                else
                    intDV += 2;
            }
            else
                intDV += 0;

            // Include any checked modifiers.
            foreach (CheckBox chkModifier in panModifiers.Controls.OfType<CheckBox>())
            {
                if (chkModifier.Visible && chkModifier.Checked)
                {
                    if (chkModifier == chkModifier3 && cboCategory.SelectedValue.ToString() == "Combat")
                        intDV += (Convert.ToInt32(chkModifier.Tag.ToString()) * Convert.ToInt32(nudNumberOfEffects.Value));
                    else if (chkModifier == chkModifier6 && cboCategory.SelectedValue.ToString() == "Manipulation")
                        intDV += (Convert.ToInt32(chkModifier.Tag.ToString()) * Convert.ToInt32(nudNumberOfEffects.Value));
                    else
                        intDV += Convert.ToInt32(chkModifier.Tag.ToString());
                }
            }

            string strBase = string.Empty;
            if (cboCategory.SelectedValue.ToString() == "Health" && chkModifier1.Checked)
            {
                // Health Spells use (Damage Value) as their base.
                strBase = "(" + LanguageManager.Instance.GetString("String_SpellDamageValue") + ")";
            }
            else
            {
                // All other spells use (F/2) as their base.
                strBase = "(F/2)";
            }

            string strDV = intDV.ToString();
            if (intDV > 0)
                strDV = "+" + strDV;
            if (intDV == 0)
                strDV = string.Empty;
            lblDV.Text = (strBase + strDV).Replace("/", "÷");
            lblDV.Text = lblDV.Text.Replace("F", LanguageManager.Instance.GetString("String_SpellForce"));
            lblDV.Text = lblDV.Text.Replace("Damage Value", LanguageManager.Instance.GetString("String_SpellDamageValue"));

            return strBase + strDV;
        }

        /// <summary>
        /// Accept the values of the form.
        /// </summary>
        private void AcceptForm()
        {
            string strMessage = string.Empty;
            // Make sure a name has been provided.
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += '\n';
                strMessage += LanguageManager.Instance.GetString("Message_SpellName");
            }

            // Make sure a Restricted value if the field is enabled.
            if (txtRestriction.Enabled && string.IsNullOrEmpty(txtRestriction.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += '\n';
                strMessage += LanguageManager.Instance.GetString("Message_SpellRestricted");
            }

            // Make sure the Spell has met all of its requirements.
            if (cboCategory.SelectedValue.ToString() == "Combat")
            {
                // Either Direct or Indirect must be selected.
                if (!chkModifier1.Checked && !chkModifier2.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_CombatSpellRequirement1");
                }

                // Either Physical damage or Stun damage must be selected.
                if (!chkModifier4.Checked && !chkModifier5.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_CombatSpellRequirement2");
                }
            }
            else if (cboCategory.SelectedValue.ToString() == "Detection")
            {
                // Either Directional, Area, or Psychic must be selected.
                if (!chkModifier1.Checked && !chkModifier2.Checked && !chkModifier3.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_DetectionSpellRequirement1");
                }

                // Either Active or Passive must be selected.
                if (!chkModifier4.Checked && !chkModifier5.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_DetectionSpellRequirement2");
                }
            }
            else if (cboCategory.SelectedValue.ToString() == "Health")
            {
                // Nothing special.
            }
            else if (cboCategory.SelectedValue.ToString() == "Illusion")
            {
                // Either Obvious or Realistic must be selected.
                if (!chkModifier1.Checked && !chkModifier2.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_IllusionSpellRequirement1");
                }

                // Either Single-Sense or Multi-Sense must be selected.
                if (!chkModifier3.Checked && !chkModifier4.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_IllusionSpellRequirement2");
                }
            }
            else if (cboCategory.SelectedValue.ToString() == "Manipulation")
            {
                // Either Environmental, Mental, or Physical must be selected.
                if (!chkModifier1.Checked && !chkModifier2.Checked && !chkModifier3.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_ManipulationSpellRequirement1");
                }

                // Either Minor Change or Major Change must be selected.
                if (!chkModifier4.Checked && !chkModifier5.Checked)
                {
                    if (!string.IsNullOrEmpty(strMessage))
                        strMessage += '\n';
                    strMessage += LanguageManager.Instance.GetString("Message_ManipulationSpellRequirement2");
                }
            }

            // Show the message if necessary.
            if (!string.IsNullOrEmpty(strMessage))
            {
                MessageBox.Show(strMessage, LanguageManager.Instance.GetString("Title_CreateSpell"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                            strDescriptors += "Extended Area";
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
                    if (cboRange.SelectedValue.ToString() == "T")
                        strDescriptors += "Touch, ";
                    if (cboRange.SelectedValue.ToString() == "A")
                        strDescriptors += "Area, ";
                    if (chkModifier3.Checked)
                        strDescriptors += "Elemental, ";
                    break;
            }

            string strRange = cboRange.SelectedValue.ToString();
            if (chkArea.Checked)
                strRange += " (A)";

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
                if (chkModifier4.Checked)
                    _objSpell.Damage = "P";
                else
                    _objSpell.Damage = "S";
            }
            _objSpell.DV = CalculateDrain();
            if (!string.IsNullOrEmpty(txtRestriction.Text))
                _objSpell.Extra = txtRestriction.Text;
            _objSpell.Duration = cboDuration.SelectedValue.ToString();

            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblCategory.Width, lblType.Width);
            intWidth = Math.Max(intWidth, lblRange.Width);
            intWidth = Math.Max(intWidth, lblDuration.Width);
            intWidth = Math.Max(intWidth, lblSpellOptions.Width);
            intWidth = Math.Max(intWidth, lblName.Width);
            txtName.Left = lblName.Left + intWidth + 6;
            cboCategory.Left = lblCategory.Left + intWidth + 6;
            cboType.Left = lblType.Left + intWidth + 6;
            cboRange.Left = lblRange.Left + intWidth + 6;
            cboDuration.Left = lblDuration.Left + intWidth + 6;
            panModifiers.Left = lblSpellOptions.Left + intWidth + 6;

            chkVeryRestricted.Left = chkRestricted.Left + chkRestricted.Width + 6;
            txtRestriction.Left = chkVeryRestricted.Left + chkVeryRestricted.Width + 6;

            chkArea.Left = cboRange.Left + cboRange.Width + 6;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Spell that was created in the dialogue.
        /// </summary>
        public Spell SelectedSpell
        {
            get
            {
                return _objSpell;
            }
        }
        #endregion
    }
}
