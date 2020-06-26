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
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            chkLimited.SetToolTip(LanguageManager.GetString("Tip_SelectSpell_LimitedSpell"));
            chkExtended.SetToolTip(LanguageManager.GetString("Tip_SelectSpell_ExtendedSpell"));

            // Load the Spells information.
            _xmlBaseSpellDataNode = _objCharacter.LoadDataXPath("spells.xml").CreateNavigator().SelectSingleNode("/chummer");
        }

        private void frmSelectSpell_Load(object sender, EventArgs e)
        {
            // If a value is forced, set the name of the spell and accept the form.
            if (!string.IsNullOrEmpty(_strForceSpell))
            {
                _strSelectedSpell = _strForceSpell;
                DialogResult = DialogResult.OK;
            }

            _blnCanGenericSpellBeFree = _objCharacter.AllowFreeSpells.Item2;
            _blnCanTouchOnlySpellBeFree = _objCharacter.AllowFreeSpells.Item1;
            txtSearch.Text = string.Empty;
            // Populate the Category list.
            HashSet<string> limit = new HashSet<string>();
            foreach (Improvement improvement in _objCharacter.Improvements.Where(x =>
                (x.ImproveType == Improvement.ImprovementType.LimitSpellCategory ||
                 x.ImproveType == Improvement.ImprovementType.AllowSpellCategory) && x.Enabled))
            {
                limit.Add(improvement.ImprovedName);
            }

            foreach (XPathNavigator objXmlCategory in _xmlBaseSpellDataNode.Select("categories/category"))
            {
                string strCategory = objXmlCategory.Value;
                if (!_blnIgnoreRequirements)
                {
                    foreach (Improvement improvement in _objCharacter.Improvements.Where(x =>
                        (x.ImproveType == Improvement.ImprovementType.AllowSpellRange ||
                         x.ImproveType == Improvement.ImprovementType.LimitSpellRange) && x.Enabled))
                    {
                        if (_xmlBaseSpellDataNode
                                .Select(
                                    "spells/spell[category = \"" + strCategory + "\" and range = \"" + improvement.ImprovedName + "\"]")
                                .Count > 0)
                        {
                            limit.Add(strCategory);
                        }
                    }

                    if (limit.Count != 0 && !limit.Contains(strCategory)) continue;
                    if (!string.IsNullOrEmpty(_strLimitCategory) && _strLimitCategory != strCategory) continue;
                }

                _lstCategory.Add(new ListItem(strCategory,
                    objXmlCategory.SelectSingleNode("@translate")?.Value ?? strCategory));
            }

            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count != 1)
            {
                _lstCategory.Insert(0,
                    new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = nameof(ListItem.Value);
            cboCategory.DisplayMember = nameof(ListItem.Name);
            cboCategory.DataSource = _lstCategory;
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory)) cboCategory.SelectedIndex = 0;
            else cboCategory.SelectedValue = s_StrSelectCategory;
            if (cboCategory.SelectedIndex == -1) cboCategory.SelectedIndex = 0;
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

        public bool FreeOnly;

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
            string strSpace = LanguageManager.GetString("String_Space");
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0))
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
                string strRange = objXmlSpell.SelectSingleNode("range")?.Value ?? string.Empty;
                if (!_blnIgnoreRequirements)
                {
                    if (!objXmlSpell.RequirementsMet(_objCharacter)) continue;
                    HashSet<string> limitDescriptors = new HashSet<string>();
                    foreach (Improvement improvement in _objCharacter.Improvements.Where(x =>
                        x.ImproveType == Improvement.ImprovementType.LimitSpellDescriptor && x.Enabled))
                    {
                        limitDescriptors.Add(improvement.ImprovedName);
                    }

                    if (limitDescriptors.Count != 0 && !limitDescriptors.Any(l => strDescriptor.Contains(l))) continue;
                    HashSet<string> blockDescriptors = new HashSet<string>();
                    foreach (Improvement improvement in _objCharacter.Improvements.Where(x =>
                        x.ImproveType == Improvement.ImprovementType.BlockSpellDescriptor && x.Enabled))
                    {
                        blockDescriptors.Add(improvement.ImprovedName);
                    }

                    if (blockDescriptors.Count != 0 && blockDescriptors.Any(l => strDescriptor.Contains(l))) continue;
                    if (_objCharacter.Improvements.Any(objImprovement =>
                        objImprovement.ImproveType == Improvement.ImprovementType.AllowSpellCategory &&
                        objImprovement.ImprovedName == strSpellCategory))
                    {
                        AddSpell(objXmlSpell, strSpellCategory);
                        continue;
                    }

                    if (_objCharacter.Improvements.Any(objImprovement =>
                        objImprovement.ImproveType == Improvement.ImprovementType.LimitSpellCategory &&
                        objImprovement.ImprovedName == strSpellCategory))
                    {
                        AddSpell(objXmlSpell, strSpellCategory);
                        continue;
                    }

                    if (_objCharacter.Improvements.Any(objImprovement =>
                        objImprovement.ImproveType == Improvement.ImprovementType.AllowSpellRange &&
                        objImprovement.ImprovedName == strRange))
                    {
                        AddSpell(objXmlSpell, strSpellCategory);
                        continue;
                    }

                    if (_objCharacter.Improvements.Any(objImprovement =>
                        objImprovement.ImproveType == Improvement.ImprovementType.LimitSpellCategory &&
                        objImprovement.ImprovedName != strSpellCategory))
                    {
                        continue;
                    }

                    if (_objCharacter.Improvements.Any(objImprovement =>
                            objImprovement.ImproveType == Improvement.ImprovementType.LimitSpellRange) &&
                        !_objCharacter.Improvements.Any(objImprovement =>
                            objImprovement.ImproveType == Improvement.ImprovementType.LimitSpellRange &&
                            objImprovement.ImprovedName == strRange))
                    {
                        continue;
                    }
                }

                AddSpell(objXmlSpell, strSpellCategory);
            }

            lstSpellItems.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstSpells.SelectedValue?.ToString();
            _blnLoading = true;
            lstSpells.BeginUpdate();
            lstSpells.ValueMember = nameof(ListItem.Value);
            lstSpells.DisplayMember = nameof(ListItem.Name);
            lstSpells.DataSource = lstSpellItems;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstSpells.SelectedValue = strOldSelected;
            else
                lstSpells.SelectedIndex = -1;
            lstSpells.EndUpdate();

            void AddSpell(XPathNavigator objXmlSpell, string strSpellCategory)
            {
                string strDisplayName = objXmlSpell.SelectSingleNode("translate")?.Value ??
                                        objXmlSpell.SelectSingleNode("name")?.Value ??
                                        LanguageManager.GetString("String_Unknown");
                if (!GlobalOptions.SearchInCategoryOnly && txtSearch.TextLength != 0)
                {
                    if (!string.IsNullOrEmpty(strSpellCategory))
                    {
                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strSpellCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += strSpace + '[' + objFoundItem.Name + ']';
                        }
                    }
                }

                lstSpellItems.Add(new ListItem(objXmlSpell.SelectSingleNode("id")?.Value ?? string.Empty, strDisplayName));
            }
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
                            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SpellLimit"), LanguageManager.GetString("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (objXmlSpell?.SelectSingleNode("category")?.Value == "Rituals")
                    {
                        if (intRitualCount >= intSpellLimit)
                        {
                            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SpellLimit"), LanguageManager.GetString("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (intSpellCount >= intSpellLimit)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SpellLimit"),
                            LanguageManager.GetString("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                if (!objXmlSpell.RequirementsMet(_objCharacter, null, LanguageManager.GetString("String_DescSpell")))
                {
                    return;
                }
            }

            _strSelectedSpell = strSelectedItem;
            s_StrSelectCategory = (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0)
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

            string strSpace = LanguageManager.GetString("String_Space");
            string[] strDescriptorsIn = xmlSpell.SelectSingleNode("descriptor")?.Value.Split(',') ?? new string[] {};

            StringBuilder objDescriptors = new StringBuilder();
            bool blnExtendedFound = false;
            bool blnAlchemicalFound = false;
            if (!string.IsNullOrEmpty(xmlSpell.SelectSingleNode("descriptor")?.Value))
            {
                foreach (string strDescriptor in strDescriptorsIn)
                {
                    switch (strDescriptor.Trim())
                    {
                        case "Alchemical Preparation":
                            blnAlchemicalFound = true;
                            objDescriptors.Append(LanguageManager.GetString("String_DescAlchemicalPreparation"));
                            break;
                        case "Extended Area":
                            blnExtendedFound = true;
                            objDescriptors.Append(LanguageManager.GetString("String_DescExtendedArea"));
                            break;
                        case "Material Link":
                            objDescriptors.Append(LanguageManager.GetString("String_DescMaterialLink"));
                            break;
                        case "Multi-Sense":
                            objDescriptors.Append(LanguageManager.GetString("String_DescMultiSense"));
                            break;
                        case "Organic Link":
                            objDescriptors.Append(LanguageManager.GetString("String_DescOrganicLink"));
                            break;
                        case "Single-Sense":
                            objDescriptors.Append(LanguageManager.GetString("String_DescSingleSense"));
                            break;
                        default:
                            objDescriptors.Append(LanguageManager.GetString("String_Desc" + strDescriptor.Trim()));
                            break;
                    }
                    objDescriptors.Append(',' + strSpace);
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
                objDescriptors.Append(LanguageManager.GetString("String_DescExtendedArea") + ',' + strSpace);
            }

            if (chkAlchemical.Checked && !blnAlchemicalFound)
            {
                objDescriptors.Append(LanguageManager.GetString("String_DescAlchemicalPreparation") + ',' + strSpace);
            }

            // Remove the trailing comma.
            if (objDescriptors.Length > 2)
                objDescriptors.Length -= 2;
            lblDescriptors.Text = objDescriptors.ToString();
            if (string.IsNullOrEmpty(lblDescriptors.Text))
                lblDescriptors.Text = LanguageManager.GetString("String_None");
            lblDescriptorsLabel.Visible = !string.IsNullOrEmpty(lblDescriptors.Text);

            switch (xmlSpell.SelectSingleNode("type")?.Value)
            {
                case "M":
                    lblType.Text = LanguageManager.GetString("String_SpellTypeMana");
                    break;
                default:
                    lblType.Text = LanguageManager.GetString("String_SpellTypePhysical");
                    break;
            }
            lblTypeLabel.Visible = !string.IsNullOrEmpty(lblType.Text);

            switch (xmlSpell.SelectSingleNode("duration")?.Value)
            {
                case "P":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationPermanent");
                    break;
                case "S":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationSustained");
                    break;
                default:
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationInstant");
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
                strRange = strRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf"))
                    .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight"))
                    .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence"))
                    .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch"))
                    .CheapReplace("(A)", () => '(' + LanguageManager.GetString("String_SpellRangeArea") + ')')
                    .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort"));
            }
            lblRange.Text = strRange;
            lblRangeLabel.Visible = !string.IsNullOrEmpty(lblRange.Text);

            switch (xmlSpell.SelectSingleNode("damage")?.Value)
            {
                case "P":
                    lblDamageLabel.Visible = true;
                    lblDamage.Text = LanguageManager.GetString("String_DamagePhysical");
                    break;
                case "S":
                    lblDamageLabel.Visible = true;
                    lblDamage.Text = LanguageManager.GetString("String_DamageStun");
                    break;
                default:
                    lblDamageLabel.Visible = false;
                    lblDamage.Text = string.Empty;
                    break;
            }

            string strDV = xmlSpell.SelectSingleNode("dv")?.Value.Replace('/', '÷') ?? string.Empty;
            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                strDV = strDV.CheapReplace("F", () => LanguageManager.GetString("String_SpellForce"))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage"))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue"))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV"))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV"))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower"));
            }

            bool force = strDV.StartsWith('F');
            strDV = strDV.TrimStartOnce('F');
            //Navigator can't do math on a single value, so inject a mathable value.
            if (string.IsNullOrEmpty(strDV))
            {
                strDV = "0";
            }
            else
            {
                int intPos = strDV.IndexOf('-');
                if (intPos != -1)
                {
                    strDV = strDV.Substring(intPos);
                }
                else
                {
                    intPos = strDV.IndexOf('+');
                    if (intPos != -1)
                    {
                        strDV = strDV.Substring(intPos);
                    }
                }
            }

            if (Limited)
            {
                strDV += " + -2";
            }
            if (Extended && !Name.EndsWith("Extended", StringComparison.Ordinal))
            {
                strDV += " + 2";
            }
            object xprResult = CommonFunctions.EvaluateInvariantXPath(strDV.TrimStart('+'), out bool blnIsSuccess);
            if (blnIsSuccess)
            {
                if (force)
                {
                    strDV = string.Format(GlobalOptions.CultureInfo, "F{0:+0;-0;}", xprResult);
                }
                else if (xprResult.ToString() != "0")
                {
                    strDV += xprResult;
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
                chkFreeBonus.Checked = FreeOnly;
                chkFreeBonus.Visible = _blnCanGenericSpellBeFree || (_blnCanTouchOnlySpellBeFree && xmlSpell.SelectSingleNode("range")?.Value == "T");
                chkFreeBonus.Enabled = FreeOnly;
            }

            string strSource = xmlSpell.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
            string strPage = xmlSpell.SelectSingleNode("altpage")?.Value ?? xmlSpell.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
            lblSource.Text = _objCharacter.LanguageBookShort(strSource) + strSpace + strPage;
            lblSource.SetToolTip(_objCharacter.LanguageBookLong(strSource) + strSpace +
                                 LanguageManager.GetString("String_Page") + strSpace + strPage);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            _blnRefresh = false;
        }
        #endregion
    }
}
