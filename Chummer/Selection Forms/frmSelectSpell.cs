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
using System.Xml.XPath;
 using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class frmSelectSpell : Form
    {
        private string _strSelectedSpell = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private bool _blnIgnoreRequirements;
        private string _strLimitCategory = string.Empty;
        private string _strForceSpell = string.Empty;
        private bool _blnCanGenericSpellBeFree;
        private bool _blnCanTouchOnlySpellBeFree;
        private static string s_StrSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseSpellDataNode;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private bool _blnRefresh;

        #region Control Events
        public frmSelectSpell(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            chkLimited.SetToolTip(LanguageManager.GetString("Tip_SelectSpell_LimitedSpell", GlobalOptions.Language));
            chkExtended.SetToolTip(LanguageManager.GetString("Tip_SelectSpell_ExtendedSpell", GlobalOptions.Language));
            
            // Load the Spells information.
            _xmlBaseSpellDataNode = XmlManager.Load("spells.xml").GetFastNavigator().SelectSingleNode("/chummer");
        }

        private void frmSelectSpell_Load(object sender, EventArgs e)
        {
            // If a value is forced, set the name of the spell and accept the form.
            if (!string.IsNullOrEmpty(_strForceSpell))
            {
                _strSelectedSpell = _strForceSpell;
                DialogResult = DialogResult.OK;
            }

            //Free Spells (typically from Dedicated Spellslinger or custom Improvements) are only handled manually
            //in Career Mode. Create mode manages itself.
            int intFreeGenericSpells = ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.FreeSpells);
            int intFreeTouchOnlySpells = 0;
            foreach (Improvement imp in _objCharacter.Improvements.Where(i => (i.ImproveType == Improvement.ImprovementType.FreeSpellsATT || i.ImproveType == Improvement.ImprovementType.FreeSpellsSkill) && i.Enabled))
            {
                switch (imp.ImproveType)
                {
                    case Improvement.ImprovementType.FreeSpellsATT:
                        int intAttValue = _objCharacter.GetAttribute(imp.ImprovedName).TotalValue;
                        if (imp.UniqueName.Contains("half"))
                            intAttValue = (intAttValue + 1) / 2;
                        if (imp.UniqueName.Contains("touchonly"))
                            intFreeTouchOnlySpells += intAttValue;
                        else
                            intFreeGenericSpells += intAttValue;
                        break;
                    case Improvement.ImprovementType.FreeSpellsSkill:
                        Skill skill = _objCharacter.SkillsSection.GetActiveSkill(imp.ImprovedName);
                        int intSkillValue = _objCharacter.SkillsSection.GetActiveSkill(imp.ImprovedName).TotalBaseRating;
                        if (imp.UniqueName.Contains("half"))
                            intSkillValue = (intSkillValue + 1) / 2;
                        if (imp.UniqueName.Contains("touchonly"))
                            intFreeTouchOnlySpells += intSkillValue;
                        else
                            intFreeGenericSpells += intSkillValue;
                        //TODO: I don't like this being hardcoded, even though I know full well CGL are never going to reuse this.
                        foreach (SkillSpecialization spec in skill.Specializations)
                        {
                            if (_objCharacter.Spells.Any(spell => spell.Category == spec.Name && !spell.FreeBonus))
                            {
                                intFreeGenericSpells++;
                            }
                        }

                        break;
                }
            }
            int intTotalFreeNonTouchSpellsCount = _objCharacter.Spells.Count(spell => spell.FreeBonus && (spell.Range != "T" && spell.Range != "T (A)"));
            int intTotalFreeTouchOnlySpellsCount = _objCharacter.Spells.Count(spell => spell.FreeBonus && (spell.Range == "T" || spell.Range == "T (A)"));
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
            HashSet<string> limit = new HashSet<string>();
            foreach (Improvement improvement in _objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LimitSpellCategory && x.Enabled))
            {
                limit.Add(improvement.ImprovedName);
            }
            foreach (XPathNavigator objXmlCategory in _xmlBaseSpellDataNode.Select("categories/category"))
            {
                string strCategory = objXmlCategory.Value;
                if (limit.Count != 0 && !limit.Contains(strCategory))
                    continue;
                if (!string.IsNullOrEmpty(_strLimitCategory) && _strLimitCategory != strCategory)
                    continue;
                _lstCategory.Add(new ListItem(strCategory, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strCategory));
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

            // Don't show the Extended Spell checkbox if the option to Extend any Detection Spell is disabled.
            chkExtended.Visible = _objCharacter.Options.ExtendAnyDetectionSpell;
            _blnLoading = false;
            BuildSpellList();
        }

        private void lstSpells_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSpellInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void treSpells_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
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
            AcceptForm();
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
            if (_blnRefresh) return;
            UpdateSpellInfo();
        }
        private void chkLimited_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh) return;
            UpdateSpellInfo();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not a Limited version of the Spell was selected.
        /// </summary>
        public bool Limited => chkLimited.Checked;

        /// <summary>
        /// Whether or not an Extended version of the Spell was selected.
        /// </summary>
        public bool Extended => chkExtended.Checked;

        /// <summary>
        /// Whether or not a Alchemical version of the Spell was selected.
        /// </summary>
        public bool Alchemical => chkAlchemical.Checked;

        /// <summary>
        /// Limit the Spell list to a particular Category.
        /// </summary>
        public string LimitCategory
        {
            set => _strLimitCategory = value;
        }

        /// <summary>
        /// Force a particular Spell to be selected.
        /// </summary>
        public string ForceSpellName
        {
            set => _strForceSpell = value;
        }

        /// <summary>
        /// Spell that was selected in the dialogue.
        /// </summary>
        public string SelectedSpell => _strSelectedSpell;

        public bool IgnoreRequirements
        {
            get => _blnIgnoreRequirements;
            set => _blnIgnoreRequirements = value;
        }

        public bool FreeBonus { get; set; }
        #endregion

        #region Methods
        private void BuildSpellList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + '\"';
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
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                }
            }
            if (_objCharacter.Options.ExtendAnyDetectionSpell)
                strFilter += " and ((not(contains(name, \", Extended\"))))";

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Populate the Spell list.
            List<ListItem> lstSpellItems = new List<ListItem>();
            foreach (XPathNavigator objXmlSpell in _xmlBaseSpellDataNode.Select("spells/spell[" + strFilter + "]"))
            {
                string strSpellCategory = objXmlSpell.SelectSingleNode("category")?.Value ?? string.Empty;
                string strDescriptor = objXmlSpell.SelectSingleNode("descriptor")?.Value ?? string.Empty;
                if (!_blnIgnoreRequirements)
                {
                    if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                    {
                        if (!((strSpellCategory == "Rituals" && !strDescriptor.Contains("Spell")) ||
                            (_blnCanTouchOnlySpellBeFree && (objXmlSpell.SelectSingleNode("range")?.Value == "T") || objXmlSpell.SelectSingleNode("range")?.Value == "T (A)")))
                            continue;
                    }
                    else if (!_objCharacter.AdeptEnabled && strDescriptor.Contains("Adept"))
                    {
                        continue;
                    }
                    if (!objXmlSpell.RequirementsMet(_objCharacter))
                        continue;
                }
                HashSet<string> limit = new HashSet<string>();
                foreach (Improvement improvement in _objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LimitSpellDescriptor && x.Enabled))
                {
                    limit.Add(improvement.ImprovedName);
                }

                if (limit.Count != 0 && limit.Any(l => strDescriptor.Contains(l)))
                    continue;
                string strDisplayName = objXmlSpell.SelectSingleNode("translate")?.Value ?? objXmlSpell.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                {
                    if (!string.IsNullOrEmpty(strSpellCategory))
                    {
                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strSpellCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += " [" + objFoundItem.Name + "]";
                        }
                    }
                }
                lstSpellItems.Add(new ListItem(objXmlSpell.SelectSingleNode("id")?.Value ?? string.Empty, strDisplayName));
            }

            lstSpellItems.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstSpells.SelectedValue?.ToString();
            _blnLoading = true;
            lstSpells.BeginUpdate();
            lstSpells.ValueMember = "Value";
            lstSpells.DisplayMember = "Name";
            lstSpells.DataSource = lstSpellItems;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstSpells.SelectedValue = strOldSelected;
            else
                lstSpells.SelectedIndex = -1;
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

            // Display the Spell information.
            XPathNavigator objXmlSpell = _xmlBaseSpellDataNode.SelectSingleNode("spells/spell[id = \"" + strSelectedItem + "\"]");
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
                    else if (objXmlSpell?.SelectSingleNode("category")?.Value == "Rituals")
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
                if (!objXmlSpell.RequirementsMet(_objCharacter, null, LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)))
                {
                    return;
                }
            }

            _strSelectedSpell = strSelectedItem;
            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0)
                ? cboCategory.SelectedValue?.ToString()
                : _xmlBaseSpellDataNode.SelectSingleNode("/chummer/spells/spell[id = \"" + _strSelectedSpell + "\"]/category")?.Value ?? string.Empty;
            FreeBonus = chkFreeBonus.Checked;
            DialogResult = DialogResult.OK;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }

        private void UpdateSpellInfo()
        {
            if (_blnLoading)
                return;
            
            XPathNavigator xmlSpell = null;
            string strSelectedSpellId = lstSpells.SelectedValue?.ToString();
            _blnRefresh = true;
            if (!chkExtended.Visible && chkExtended.Checked)
            {
                chkExtended.Checked = false;
            }
            if (!string.IsNullOrEmpty(strSelectedSpellId))
            {
                xmlSpell = _xmlBaseSpellDataNode.SelectSingleNode("/chummer/spells/spell[id = \"" + strSelectedSpellId + "\"]");
            }
            if (xmlSpell == null)
            {
                lblDescriptorsLabel.Visible = false;
                chkAlchemical.Visible = false;
                lblTypeLabel.Visible = false;
                lblDurationLabel.Visible = false;
                chkExtended.Visible = false;
                lblRangeLabel.Visible = false;
                lblDamageLabel.Visible = false;
                lblDVLabel.Visible = false;
                chkFreeBonus.Visible = false;
                lblSourceLabel.Visible = false;
                lblDescriptors.Text = string.Empty;
                chkAlchemical.Checked = false;
                lblType.Text = string.Empty;
                lblDuration.Text = string.Empty;
                chkExtended.Checked = false;
                lblRange.Text = string.Empty;
                lblDamage.Text = string.Empty;
                lblDV.Text = string.Empty;
                chkFreeBonus.Checked = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                return;
            }

            string[] strDescriptorsIn = xmlSpell.SelectSingleNode("descriptor")?.Value.Split(',') ?? new string[] {};

            StringBuilder objDescriptors = new StringBuilder();
            bool blnExtendedFound = false;
            bool blnAlchemicalFound = false;
            foreach (string strDescriptor in strDescriptorsIn)
            {
                switch (strDescriptor.Trim())
                {
                    case "Active":
                        objDescriptors.Append(LanguageManager.GetString("String_DescActive", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Adept":
                        objDescriptors.Append(LanguageManager.GetString("String_DescAdept", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Alchemical Preparation":
                        blnAlchemicalFound = true;
                        objDescriptors.Append(LanguageManager.GetString("String_DescAlchemicalPreparation", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Area":
                        objDescriptors.Append(LanguageManager.GetString("String_DescArea", GlobalOptions.Language));
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
                    case "Direct":
                        objDescriptors.Append(LanguageManager.GetString("String_DescDirect", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Directional":
                        objDescriptors.Append(LanguageManager.GetString("String_DescDirectional", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Elemental":
                        objDescriptors.Append(LanguageManager.GetString("String_DescElemental", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Environmental":
                        objDescriptors.Append(LanguageManager.GetString("String_DescEnvironmental", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Extended Area":
                        blnExtendedFound = true;
                        objDescriptors.Append(LanguageManager.GetString("String_DescExtendedArea", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Geomancy":
                        objDescriptors.Append(LanguageManager.GetString("String_DescGeomancy", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Indirect":
                        objDescriptors.Append(LanguageManager.GetString("String_DescIndirect", GlobalOptions.Language));
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
                    case "Mental":
                        objDescriptors.Append(LanguageManager.GetString("String_DescMental", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Minion":
                        objDescriptors.Append(LanguageManager.GetString("String_DescMinion", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Multi-Sense":
                        objDescriptors.Append(LanguageManager.GetString("String_DescMultiSense", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Negative":
                        objDescriptors.Append(LanguageManager.GetString("String_DescNegative", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Obvious":
                        objDescriptors.Append(LanguageManager.GetString("String_DescObvious", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Organic Link":
                        objDescriptors.Append(LanguageManager.GetString("String_DescOrganicLink", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Passive":
                        objDescriptors.Append(LanguageManager.GetString("String_DescPassive", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Physical":
                        objDescriptors.Append(LanguageManager.GetString("String_DescPhysical", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Psychic":
                        objDescriptors.Append(LanguageManager.GetString("String_DescPsychic", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Realistic":
                        objDescriptors.Append(LanguageManager.GetString("String_DescRealistic", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Single-Sense":
                        objDescriptors.Append(LanguageManager.GetString("String_DescSingleSense", GlobalOptions.Language));
                        objDescriptors.Append(", ");
                        break;
                    case "Touch":
                        objDescriptors.Append(LanguageManager.GetString("String_DescTouch", GlobalOptions.Language));
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

            if (blnAlchemicalFound)
            {
                chkAlchemical.Visible = true;
                chkAlchemical.Enabled = false;
                chkAlchemical.Checked = true;
            }
            else if (xmlSpell.SelectSingleNode("category")?.Value == "Rituals")
            {
                chkAlchemical.Visible = false;
                chkAlchemical.Checked = false;
            }
            else
            {
                chkAlchemical.Visible = true;
                chkAlchemical.Enabled = true;
            }

            // If Extended Area was not found and the Extended checkbox is checked, add Extended Area to the list of Descriptors.
            if (chkExtended.Checked && !blnExtendedFound)
            {
                objDescriptors.Append(LanguageManager.GetString("String_DescExtendedArea", GlobalOptions.Language));
                objDescriptors.Append(", ");
            }

            if (chkAlchemical.Checked && !blnAlchemicalFound)
            {
                objDescriptors.Append(LanguageManager.GetString("String_DescAlchemicalPreparation", GlobalOptions.Language));
                objDescriptors.Append(", ");
            }

            // Remove the trailing comma.
            if (objDescriptors.Length > 2)
                objDescriptors.Length -= 2;
            lblDescriptors.Text = objDescriptors.ToString();
            lblDescriptorsLabel.Visible = !string.IsNullOrEmpty(lblDescriptors.Text);

            switch (xmlSpell.SelectSingleNode("type")?.Value)
            {
                case "M":
                    lblType.Text = LanguageManager.GetString("String_SpellTypeMana", GlobalOptions.Language);
                    break;
                default:
                    lblType.Text = LanguageManager.GetString("String_SpellTypePhysical", GlobalOptions.Language);
                    break;
            }
            lblTypeLabel.Visible = !string.IsNullOrEmpty(lblType.Text);

            switch (xmlSpell.SelectSingleNode("duration")?.Value)
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
            lblDurationLabel.Visible = !string.IsNullOrEmpty(lblDuration.Text);

            if (blnExtendedFound)
            {
                chkExtended.Visible = true;
                chkExtended.Checked = true;
                chkExtended.Enabled = false;
            }
            else if (xmlSpell.SelectSingleNode("category")?.Value == "Detection")
            {
                chkExtended.Visible = true;
                chkExtended.Enabled = true;
            }
            else
            {
                chkExtended.Checked = false;
                chkExtended.Visible = false;
            }

            string strRange = xmlSpell.SelectSingleNode("range")?.Value ?? string.Empty;
            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                strRange = strRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", GlobalOptions.Language))
                    .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", GlobalOptions.Language))
                    .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", GlobalOptions.Language))
                    .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", GlobalOptions.Language))
                    .CheapReplace("(A)", () => '(' + LanguageManager.GetString("String_SpellRangeArea", GlobalOptions.Language) + ')')
                    .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language));
            }
            lblRange.Text = strRange;
            lblRangeLabel.Visible = !string.IsNullOrEmpty(lblRange.Text);

            switch (xmlSpell.SelectSingleNode("damage")?.Value)
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

            string strDV = xmlSpell.SelectSingleNode("dv")?.Value.Replace('/', '÷') ?? string.Empty;
            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                strDV = strDV.CheapReplace("F", () => LanguageManager.GetString("String_SpellForce", GlobalOptions.Language))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", GlobalOptions.Language))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", GlobalOptions.Language))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", GlobalOptions.Language))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", GlobalOptions.Language))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", GlobalOptions.Language));
            }

            if (chkExtended.Checked && !blnExtendedFound)
            {
                // Add +2 to the DV value if Extended is selected.
                int intPos = strDV.IndexOf(')') + 1;
                string strAfter;
                if (intPos > 0)
                {
                    strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    if (string.IsNullOrEmpty(strAfter))
                        strAfter = "+2";
                    else
                    {
                        int intValue = Convert.ToInt32(strAfter) + 2;
                        if (intValue == 0)
                            strAfter = string.Empty;
                        else if (intValue > 0)
                            strAfter = '+' + intValue.ToString();
                        else
                            strAfter = intValue.ToString();
                    }
                }
                else
                {
                    strAfter = "+2";
                }
                
                strDV += strAfter;
            }

            if (chkLimited.Checked)
            {
                int intPos = strDV.IndexOf('-');
                if (intPos != -1)
                {
                    intPos = intPos + 1;
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter = Convert.ToInt32(strAfter);
                    intAfter += 2;
                    strDV += intAfter.ToString();
                }
                else
                {
                    intPos = strDV.IndexOf('+');
                    if (intPos != -1)
                    {
                        string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                        strDV = strDV.Substring(0, intPos);
                        int intAfter = Convert.ToInt32(strAfter);
                        intAfter -= 2;
                        if (intAfter > 0)
                            strDV += '+' + intAfter.ToString();
                        else if (intAfter < 0)
                            strDV += intAfter.ToString();
                    }
                    else
                    {
                        strDV += "-2";
                    }
                }
            }

            lblDV.Text = strDV;
            lblDVLabel.Visible = !string.IsNullOrEmpty(lblDV.Text);

            if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled && _blnCanTouchOnlySpellBeFree && xmlSpell.SelectSingleNode("range")?.Value == "T")
            {
                chkFreeBonus.Checked = true;
                chkFreeBonus.Visible = true;
                chkFreeBonus.Enabled = false;
            }
            else
            {
                chkFreeBonus.Checked = false;
                chkFreeBonus.Visible = _blnCanGenericSpellBeFree || (_blnCanTouchOnlySpellBeFree && xmlSpell.SelectSingleNode("range")?.Value == "T");
                chkFreeBonus.Enabled = true;
            }

            string strSource = xmlSpell.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = xmlSpell.SelectSingleNode("altpage")?.Value ?? xmlSpell.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter +
                                 LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            _blnRefresh = false;
        }
        #endregion
    }
}
