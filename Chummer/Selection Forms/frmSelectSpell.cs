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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend;

namespace Chummer
{
    public partial class frmSelectSpell : Form
    {
        private string _strSelectedSpell = string.Empty;

        private bool _blnAddAgain;
        private bool _blnIgnoreRequirements;
        private string _strLimitCategory = string.Empty;
        private string _strForceSpell = string.Empty;
        private bool _blnCanGenericSpellBeFree = false;
        private bool _blnCanTouchOnlySpellBeFree = false;
        private static string s_StrSelectCategory = string.Empty;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectSpell(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;

            tipTooltip.SetToolTip(chkLimited, LanguageManager.GetString("Tip_SelectSpell_LimitedSpell", GlobalOptions.Language));
            tipTooltip.SetToolTip(chkExtended, LanguageManager.GetString("Tip_SelectSpell_ExtendedSpell", GlobalOptions.Language));

            MoveControls();
            // Load the Spells information.
            _objXmlDocument = XmlManager.Load("spells.xml");
        }

        private void frmSelectSpell_Load(object sender, EventArgs e)
        {
            // If a value is forced, set the name of the spell and accept the form.
            if (!string.IsNullOrEmpty(_strForceSpell))
            {
                _strSelectedSpell = _strForceSpell;
                DialogResult = DialogResult.OK;
            }

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            //Free Spells (typically from Dedicated Spellslinger or custom Improvements) are only handled manually
            //in Career Mode. Create mode manages itself.
            int intFreeGenericSpells = ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.FreeSpells);
            int intFreeTouchOnlySpells = 0;
            foreach (Improvement imp in _objCharacter.Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.FreeSpellsATT))
            {
                if (imp.ImproveType == Improvement.ImprovementType.FreeSpellsATT)
                {
                    int intAttValue = _objCharacter.GetAttribute(imp.ImprovedName).TotalValue;
                    if (imp.UniqueName.Contains("half"))
                        intAttValue = (intAttValue + 1) / 2;
                    if (imp.UniqueName.Contains("touchonly"))
                        intFreeTouchOnlySpells += intAttValue;
                    else
                        intFreeGenericSpells += intAttValue;
                }
                else if (imp.ImproveType == Improvement.ImprovementType.FreeSpellsSkill)
                {
                    int intSkillValue = _objCharacter.SkillsSection.GetActiveSkill(imp.ImprovedName).TotalBaseRating;
                    if (imp.UniqueName.Contains("half"))
                        intSkillValue = (intSkillValue + 1) / 2;
                    if (imp.UniqueName.Contains("touchonly"))
                        intFreeTouchOnlySpells += intSkillValue;
                    else
                        intFreeGenericSpells += intSkillValue;
                }
            }
            int intTotalFreeNonTouchSpellsCount = _objCharacter.Spells.Count(spell => spell.FreeBonus && spell.Range != "T");
            int intTotalFreeTouchOnlySpellsCount = _objCharacter.Spells.Count(spell => spell.FreeBonus && spell.Range == "T");
            if (intFreeTouchOnlySpells > intTotalFreeTouchOnlySpellsCount)
            {
                _blnCanTouchOnlySpellBeFree = true;
            }
            if (intFreeGenericSpells > intTotalFreeNonTouchSpellsCount + Math.Max(intTotalFreeTouchOnlySpellsCount - intFreeTouchOnlySpells, 0))
            {
                _blnCanGenericSpellBeFree = true;
            }

            txtSearch.Text = string.Empty;

            // Populate the Category list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            HashSet<string> limit = new HashSet<string>();
            foreach (Improvement improvement in _objCharacter.Improvements.Where(improvement => improvement.ImproveType == Improvement.ImprovementType.LimitSpellCategory))
            {
                limit.Add(improvement.ImprovedName);
            }
            foreach (XmlNode objXmlCategory in objXmlNodeList)
            {
                string strCategory = objXmlCategory.InnerText;
                if (limit.Count != 0 && !limit.Contains(strCategory))
                    continue;
                if (!string.IsNullOrEmpty(_strLimitCategory) && _strLimitCategory != strCategory)
                    continue;
                _lstCategory.Add(new ListItem(strCategory, objXmlCategory.Attributes?["translate"]?.InnerText ?? strCategory));
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            // Don't show the Extended Spell checkbox if the option to Extend any Detection Spell is diabled.
            chkExtended.Visible = _objCharacter.Options.ExtendAnyDetectionSpell;
            BuildSpellList();
        }

        private void lstSpells_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSpellInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (lstSpells.SelectedValue != null)
            {
                // Display the Spell information.
                XmlNode objXmlSpell = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + lstSpells.SelectedValue.ToString() + "\"]");
                // Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
                // The maximum number of Spells a character can start with is 2 x (highest of Spellcasting or Ritual Spellcasting Skill).
                int intSpellCount = 0;
                int intRitualCount = 0;
                int intAlchPrepCount = 0;

                foreach (Spell objspell in _objCharacter.Spells)
                {
                    if (objspell.Alchemical)
                    { intAlchPrepCount++; }
                    else if (objspell.Category == "Rituals")
                    { intRitualCount++; }
                    else
                    { intSpellCount++; }
                }
                if (!_objCharacter.IgnoreRules)
                {
                    if (!_objCharacter.Created)
                    {
                        int intSpellLimit = (_objCharacter.MAG.TotalValue * 2);
                        if (chkAlchemical.Checked)
                        {
                            if (intAlchPrepCount >= intSpellLimit)
                            {
                                MessageBox.Show(LanguageManager.GetString("Message_SpellLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SpellLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        else if (objXmlSpell["category"].InnerText == "Rituals")
                        {
                            if (intRitualCount >= intSpellLimit)
                            {
                                MessageBox.Show(LanguageManager.GetString("Message_SpellLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SpellLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        else if (intSpellCount >= intSpellLimit)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_SpellLimit", GlobalOptions.Language),
                                LanguageManager.GetString("MessageTitle_SpellLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    if (!SelectionShared.RequirementsMet(objXmlSpell, true, _objCharacter, null, null, _objXmlDocument, string.Empty, LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)))
                    {
                        return;
                    }
                }
                AcceptForm();
            }
        }

        private void treSpells_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildSpellList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildSpellList();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstSpells.SelectedIndex == -1)
            {
                if (lstSpells.Items.Count > 0)
                    lstSpells.SelectedIndex = 0;
            }
            if (e.KeyCode == Keys.Down)
            {
                int intNewIndex = lstSpells.SelectedIndex + 1;
                if (intNewIndex >= lstSpells.Items.Count)
                    intNewIndex = 0;
                if (lstSpells.Items.Count > 0)
                    lstSpells.SelectedIndex = intNewIndex;
            }
            if (e.KeyCode == Keys.Up)
            {
                int intNewIndex = lstSpells.SelectedIndex - 1;
                if (intNewIndex <= 0)
                    intNewIndex = lstSpells.Items.Count - 1;
                if (lstSpells.Items.Count > 0)
                    lstSpells.SelectedIndex = intNewIndex;
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void chkExtended_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSpellInfo();
        }
        private void chkLimited_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSpellInfo();
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Whether or not a Limited version of the Spell was selected.
        /// </summary>
        public bool Limited
        {
            get
            {
                return chkLimited.Checked;
            }
        }

        /// <summary>
        /// Whether or not an Extended version of the Spell was selected.
        /// </summary>
        public bool Extended
        {
            get
            {
                return chkExtended.Checked;
            }
        }

        /// <summary>
        /// Whether or not a Alchemical version of the Spell was selected.
        /// </summary>
        public bool Alchemical
        {
            get
            {
                return chkAlchemical.Checked;
            }
        }

        /// <summary>
        /// Limit the Spell list to a particular Category.
        /// </summary>
        public string LimitCategory
        {
            set
            {
                _strLimitCategory = value;
            }
        }

        /// <summary>
        /// Force a particular Spell to be selected.
        /// </summary>
        public string ForceSpellName
        {
            set
            {
                _strForceSpell = value;
            }
        }

        /// <summary>
        /// Spell that was selected in the dialogue.
        /// </summary>
        public string SelectedSpell
        {
            get
            {
                return _strSelectedSpell;
            }
        }

        public bool IgnoreRequirements
        {
            get
            {
                return _blnIgnoreRequirements;
            }
            set
            {
                _blnIgnoreRequirements = value;
            }

        }

        public bool FreeBonus { get; set; }
        #endregion

        #region Methods
        private void BuildSpellList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = "(" + _objCharacter.Options.BookXPath() + ")";
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + "\"";
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ")";
                }
            }
            if (txtSearch.TextLength != 0)
            {
                // Treat everything as being uppercase so the search is case-insensitive.
                string strSearchText = txtSearch.Text.ToUpper();
                strFilter += " and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\"))";
            }
            
            if (_objCharacter.Options.ExtendAnyDetectionSpell)
                strFilter += " and ((not(contains(name, \", Extended\"))))";

            // Populate the Spell list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/spells/spell[" + strFilter + "]");

            List<ListItem> lstSpellItems = new List<ListItem>();
            foreach (XmlNode objXmlSpell in objXmlNodeList)
            {
                string strSpellCategory = objXmlSpell["category"]?.InnerText;
                if (!_blnIgnoreRequirements)
                {
                    if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                    {
                        if (!((strSpellCategory == "Rituals" && !objXmlSpell["descriptor"].InnerText.Contains("Spell")) ||
                            (_blnCanTouchOnlySpellBeFree && objXmlSpell["range"].InnerText == "T")))
                            continue;
                    }
                    else if (!_objCharacter.AdeptEnabled && objXmlSpell["descriptor"].InnerText.Contains("Adept"))
                    {
                        continue;
                    }
                    if (!SelectionShared.RequirementsMet(objXmlSpell, false, _objCharacter))
                        continue;
                }

                string strDisplayName = objXmlSpell["translate"]?.InnerText ?? objXmlSpell["name"].InnerText;
                if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                {
                    if (!string.IsNullOrEmpty(strSpellCategory))
                    {
                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == strSpellCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += " [" + objFoundItem.Name + "]";
                        }
                    }
                }
                lstSpellItems.Add(new ListItem(objXmlSpell["id"].InnerText, strDisplayName));
            }
            
            lstSpellItems.Sort(CompareListItems.CompareNames);
            lstSpells.BeginUpdate();
            lstSpells.DataSource = null;
            lstSpells.ValueMember = "Value";
            lstSpells.DisplayMember = "Name";
            lstSpells.DataSource = lstSpellItems;
            lstSpells.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstSpells.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedItem))
                return;
            _strSelectedSpell = strSelectedItem;
            XmlNode objXmlSpell = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + _strSelectedSpell + "\"]");
            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objXmlSpell?["category"]?.InnerText ?? string.Empty;
            FreeBonus = chkFreeBonus.Checked;
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblDescriptorsLabel.Width, lblTypeLabel.Width);
            intWidth = Math.Max(intWidth, lblTypeLabel.Width);
            intWidth = Math.Max(intWidth, lblRangeLabel.Width);
            intWidth = Math.Max(intWidth, lblDamageLabel.Width);
            intWidth = Math.Max(intWidth, lblDurationLabel.Width);
            intWidth = Math.Max(intWidth, lblDVLabel.Width);

            lblDescriptors.Left = lblDescriptorsLabel.Left + intWidth + 6;
            lblType.Left = lblTypeLabel.Left + intWidth + 6;
            lblRange.Left = lblRangeLabel.Left + intWidth + 6;
            lblDamage.Left = lblDamageLabel.Left + intWidth + 6;
            lblDuration.Left = lblDurationLabel.Left + intWidth + 6;
            lblDV.Left = lblDVLabel.Left + intWidth + 6;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }

        private void UpdateSpellInfo()
        {
            string strSelectedSpellId = lstSpells.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedSpellId))
            {
                lblDescriptors.Text = string.Empty;
                chkAlchemical.Enabled = false;
                chkAlchemical.Checked = false;
                lblType.Text = string.Empty;
                lblDuration.Text = string.Empty;
                chkExtended.Checked = false;
                chkExtended.Enabled = false;
                lblRange.Text = string.Empty;
                lblDamage.Text = string.Empty;
                lblDV.Text = string.Empty;
                chkFreeBonus.Checked = false;
                chkFreeBonus.Visible = false;
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
                return;
            }

            // Display the Spell information.
            XmlNode objXmlSpell =
                _objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + strSelectedSpellId + "\"]");

            string[] strDescriptorsIn = objXmlSpell["descriptor"].InnerText.Split(',');

            StringBuilder objDescriptors = new StringBuilder();
            bool blnExtendedFound = false;
            foreach (string strDescriptor in strDescriptorsIn)
            {
                switch (strDescriptor.Trim())
                {
                    case "Adept":
                        objDescriptors.Append(LanguageManager.GetString("String_DescAdept", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Anchored":
                        objDescriptors.Append(LanguageManager.GetString("String_DescAnchored", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Blood":
                        objDescriptors.Append(LanguageManager.GetString("String_DescBlood", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Contractual":
                        objDescriptors.Append(LanguageManager.GetString("String_DescContractual", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Geomancy":
                        objDescriptors.Append(LanguageManager.GetString("String_DescGeomancy", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Mana":
                        objDescriptors.Append(LanguageManager.GetString("String_DescMana", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Material Link":
                        objDescriptors.Append(LanguageManager.GetString("String_DescMaterialLink", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Minion":
                        objDescriptors.Append(LanguageManager.GetString("String_DescMinion", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Organic Link":
                        objDescriptors.Append(LanguageManager.GetString("String_DescOrganicLink", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Spell":
                        objDescriptors.Append(LanguageManager.GetString("String_DescSpell", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Spotter":
                        objDescriptors.Append(LanguageManager.GetString("String_DescSpotter", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                }
            }

            if (objXmlSpell["category"].InnerText == "Rituals")
            {
                chkAlchemical.Enabled = false;
                chkAlchemical.Checked = false;
            }
            else
                chkAlchemical.Enabled = true;

            // If Extended Area was not found and the Extended checkbox is checked, add Extended Area to the list of Descriptors.
            if (chkExtended.Checked && !blnExtendedFound)
            {
                objDescriptors.Append(LanguageManager.GetString("String_DescExtendedArea", GlobalOptions.Language));
                objDescriptors.Append(", ");
            }

            if (chkAlchemical.Checked && !blnExtendedFound)
            {
                objDescriptors.Append(LanguageManager.GetString("String_DescAlchemicalPreparation", GlobalOptions.Language));
                objDescriptors.Append(", ");
            }

            // Remove the trailing comma.
            if (objDescriptors.Length > 2)
                objDescriptors.Length -= 2;
            lblDescriptors.Text = objDescriptors.ToString();

            switch (objXmlSpell["type"].InnerText)
            {
                case "M":
                    lblType.Text = LanguageManager.GetString("String_SpellTypeMana", GlobalOptions.Language);
                    break;
                default:
                    lblType.Text = LanguageManager.GetString("String_SpellTypePhysical", GlobalOptions.Language);
                    break;
            }

            switch (objXmlSpell["duration"].InnerText)
            {
                case "P":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationPermanent", GlobalOptions.Language);
                    break;
                case "S":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationSustained", GlobalOptions.Language);
                    break;
                default:
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationInstant", GlobalOptions.Language);
                    break;
            }

            if (objXmlSpell["category"].InnerText == "Detection")
            {
                chkExtended.Enabled = true;
            }
            else
            {
                chkExtended.Checked = false;
                chkExtended.Enabled = false;
            }

            string strRange = objXmlSpell["range"].InnerText;
            strRange = strRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", GlobalOptions.Language));
            strRange = strRange.CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", GlobalOptions.Language));
            strRange = strRange.CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", GlobalOptions.Language));
            strRange = strRange.CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", GlobalOptions.Language));
            strRange = strRange.CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", GlobalOptions.Language) + ")");
            strRange = strRange.CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language));
            lblRange.Text = strRange;

            switch (objXmlSpell["damage"].InnerText)
            {
                case "P":
                    lblDamageLabel.Visible = true;
                    lblDamage.Text = LanguageManager.GetString("String_DamagePhysical", GlobalOptions.Language);
                    break;
                case "S":
                    lblDamageLabel.Visible = true;
                    lblDamage.Text = LanguageManager.GetString("String_DamageStun", GlobalOptions.Language);
                    break;
                default:
                    lblDamageLabel.Visible = false;
                    lblDamage.Text = string.Empty;
                    break;
            }

            string strDV = objXmlSpell["dv"].InnerText.Replace('/', '÷').CheapReplace("F", () => LanguageManager.GetString("String_SpellForce", GlobalOptions.Language));
            strDV = strDV.CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", GlobalOptions.Language));
            strDV = strDV.CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", GlobalOptions.Language));
            strDV = strDV.CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", GlobalOptions.Language));
            strDV = strDV.CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", GlobalOptions.Language));
            strDV = strDV.CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", GlobalOptions.Language));

            if (chkExtended.Checked)
            {
                // Add +2 to the DV value if Extended is selected.
                int intPos = strDV.IndexOf(')') + 1;
                string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                strDV = strDV.Substring(0, intPos);
                if (string.IsNullOrEmpty(strAfter))
                    strAfter = "+2";
                else
                {
                    int intValue = Convert.ToInt32(strAfter) + 2;
                    if (intValue == 0)
                        strAfter = string.Empty;
                    else if (intValue > 0)
                        strAfter = "+" + intValue.ToString();
                    else
                        strAfter = intValue.ToString();
                }
                strDV += strAfter;
            }

            if (chkLimited.Checked)
            {
                int intPos = 0;
                if (strDV.Contains('-'))
                {
                    intPos = strDV.IndexOf('-') + 1;
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter = Convert.ToInt32(strAfter);
                    intAfter += 2;
                    strDV += intAfter.ToString();
                }
                else if (strDV.Contains('+'))
                {
                    intPos = strDV.IndexOf('+');
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter = Convert.ToInt32(strAfter);
                    intAfter -= 2;
                    if (intAfter > 0)
                        strDV += "+" + intAfter.ToString();
                    else if (intAfter < 0)
                        strDV += intAfter.ToString();
                }
                else
                {
                    strDV += "-2";
                }
            }

            lblDV.Text = strDV;

            if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled && _blnCanTouchOnlySpellBeFree && objXmlSpell["range"].InnerText == "T")
            {
                chkFreeBonus.Checked = true;
                chkFreeBonus.Visible = true;
                chkFreeBonus.Enabled = false;
            }
            else
            {
                chkFreeBonus.Checked = false;
                chkFreeBonus.Visible = _blnCanGenericSpellBeFree || (_blnCanTouchOnlySpellBeFree && objXmlSpell["range"].InnerText == "T");
                chkFreeBonus.Enabled = true;
            }

            string strBook = CommonFunctions.LanguageBookShort(objXmlSpell["source"].InnerText, GlobalOptions.Language);
            string strPage = objXmlSpell["page"].InnerText;
            if (objXmlSpell["altpage"] != null)
                strPage = objXmlSpell["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource,
                CommonFunctions.LanguageBookLong(objXmlSpell["source"].InnerText, GlobalOptions.Language) + " " +
                LanguageManager.GetString("String_Page", GlobalOptions.Language) + " " + strPage);
        }
        #endregion
    }
}
