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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectSpell : Form
    {
        private string _strSelectedSpell = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private bool _blnIgnoreRequirements;
        private string _strLimitCategory = string.Empty;
        private string _strForceSpell = string.Empty;
        private bool _blnCanGenericSpellBeFree;
        private bool _blnCanTouchOnlySpellBeFree;
        private static string _strSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseSpellDataNode;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private bool _blnRefresh;

        #region Control Events

        public SelectSpell(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            // Load the Spells information.
            _xmlBaseSpellDataNode = _objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer");
        }

        private async void SelectSpell_Load(object sender, EventArgs e)
        {
            chkLimited.SetToolTip(await LanguageManager.GetStringAsync("Tip_SelectSpell_LimitedSpell"));
            chkExtended.SetToolTip(await LanguageManager.GetStringAsync("Tip_SelectSpell_ExtendedSpell"));
            // If a value is forced, set the name of the spell and accept the form.
            if (!string.IsNullOrEmpty(_strForceSpell))
            {
                _strSelectedSpell = _strForceSpell;
                DialogResult = DialogResult.OK;
            }
            (bool blnCanTouchOnlySpellBeFree, bool blnCanGenericSpellBeFree) = _objCharacter.AllowFreeSpells;
            _blnCanTouchOnlySpellBeFree = blnCanTouchOnlySpellBeFree;
            _blnCanGenericSpellBeFree = blnCanGenericSpellBeFree;
            txtSearch.Text = string.Empty;
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in await _xmlBaseSpellDataNode.SelectAndCacheExpressionAsync(
                         "categories/category"))
            {
                string strCategory = objXmlCategory.Value;
                if (!string.IsNullOrEmpty(_strLimitCategory) && strCategory != _strLimitCategory)
                    continue;
                if (!await AnyItemInList(strCategory))
                    continue;
                _lstCategory.Add(new ListItem(strCategory,
                                              (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value
                                              ?? strCategory));
            }

            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count != 1)
            {
                _lstCategory.Insert(0,
                    new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;
            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            // Don't show the Extended Spell checkbox if the option to Extend any Detection Spell is disabled.
            chkExtended.Visible = _objCharacter.Settings.ExtendAnyDetectionSpell;
            _blnLoading = false;
            await BuildSpellList(cboCategory.SelectedValue?.ToString());
        }

        private async void lstSpells_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSpellInfo();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await BuildSpellList(cboCategory.SelectedValue?.ToString());
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildSpellList(cboCategory.SelectedValue?.ToString());
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstSpells.SelectedIndex == -1 && lstSpells.Items.Count > 0)
            {
                lstSpells.SelectedIndex = 0;
            }
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        int intNewIndex = lstSpells.SelectedIndex + 1;
                        if (intNewIndex >= lstSpells.Items.Count)
                            intNewIndex = 0;
                        if (lstSpells.Items.Count > 0)
                            lstSpells.SelectedIndex = intNewIndex;
                        break;
                    }
                case Keys.Up:
                    {
                        int intNewIndex = lstSpells.SelectedIndex - 1;
                        if (intNewIndex <= 0)
                            intNewIndex = lstSpells.Items.Count - 1;
                        if (lstSpells.Items.Count > 0)
                            lstSpells.SelectedIndex = intNewIndex;
                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private async void chkExtended_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh)
                return;
            await UpdateSpellInfo();
        }

        private async void chkLimited_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh)
                return;
            await UpdateSpellInfo();
        }

        #endregion Control Events

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

        public bool FreeOnly { get; set; }

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

        #endregion Properties

        #region Methods

        private ValueTask<bool> AnyItemInList(string strCategory = "")
        {
            return RefreshList(strCategory, false);
        }

        private ValueTask<bool> BuildSpellList(string strCategory = "")
        {
            return RefreshList(strCategory, true);
        }

        private async ValueTask<bool> RefreshList(string strCategory, bool blnDoUIUpdate)
        {
            if (_blnLoading && blnDoUIUpdate)
                return false;
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    lstSpells.BeginUpdate();
                    lstSpells.PopulateWithListItems(ListItem.Blank.Yield());
                    lstSpells.EndUpdate();
                }
                return false;
            }

            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSpellItems))
            {
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> limitDescriptors))
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> blockDescriptors))
                {
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                        if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                               && (GlobalSettings.SearchInCategoryOnly
                                                                   || txtSearch.TextLength == 0))
                            sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                        else
                        {
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdCategoryFilter))
                            {
                                foreach (string strItem in _lstCategory.Select(x => x.Value))
                                {
                                    if (!string.IsNullOrEmpty(strItem))
                                        sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath())
                                                         .Append(" or ");
                                }

                                if (sbdCategoryFilter.Length > 0)
                                {
                                    sbdCategoryFilter.Length -= 4;
                                    sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                                }
                            }
                        }

                        if (_objCharacter.Settings.ExtendAnyDetectionSpell)
                            sbdFilter.Append(" and ((not(contains(name, \", Extended\"))))");
                        if (!string.IsNullOrEmpty(txtSearch.Text))
                            sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                        // Populate the Spell list.
                        if (!_blnIgnoreRequirements)
                        {
                            foreach (Improvement improvement in ImprovementManager.GetCachedImprovementListForValueOf(
                                         _objCharacter, Improvement.ImprovementType.LimitSpellDescriptor))
                            {
                                limitDescriptors.Add(improvement.ImprovedName);
                            }

                            foreach (Improvement improvement in ImprovementManager.GetCachedImprovementListForValueOf(
                                         _objCharacter, Improvement.ImprovementType.BlockSpellDescriptor))
                            {
                                blockDescriptors.Add(improvement.ImprovedName);
                            }
                        }

                        if (sbdFilter.Length > 0)
                            strFilter = '[' + sbdFilter.ToString() + ']';
                    }

                    foreach (XPathNavigator objXmlSpell in _xmlBaseSpellDataNode.Select("spells/spell" + strFilter))
                    {
                        string strSpellCategory = objXmlSpell.SelectSingleNode("category")?.Value ?? string.Empty;
                        if (!_blnIgnoreRequirements)
                        {
                            if (!objXmlSpell.RequirementsMet(_objCharacter))
                                continue;
                            
                            if (ImprovementManager.GetCachedImprovementListForValueOf(
                                    _objCharacter, Improvement.ImprovementType.AllowSpellCategory,
                                    strSpellCategory).Count != 0)
                            {
                                if (!blnDoUIUpdate)
                                    return true;
                                await AddSpell(objXmlSpell, strSpellCategory);
                                continue;
                            }

                            string strRange = objXmlSpell.SelectSingleNode("range")?.Value ?? string.Empty;
                            if (ImprovementManager.GetCachedImprovementListForValueOf(
                                    _objCharacter, Improvement.ImprovementType.AllowSpellRange,
                                    strRange).Count != 0)
                            {
                                if (!blnDoUIUpdate)
                                    return true;
                                await AddSpell(objXmlSpell, strSpellCategory);
                                continue;
                            }

                            string strDescriptor
                                = objXmlSpell.SelectSingleNode("descriptor")?.Value ?? string.Empty;

                            if (limitDescriptors.Count != 0
                                && !limitDescriptors.Any(l => strDescriptor.Contains(l)))
                                continue;

                            if (blockDescriptors.Count != 0 && blockDescriptors.Any(l => strDescriptor.Contains(l)))
                                continue;

                            if (ImprovementManager
                                .GetCachedImprovementListForValueOf(_objCharacter,
                                                                    Improvement.ImprovementType.LimitSpellCategory)
                                .Any(x => x.ImprovedName != strSpellCategory))
                            {
                                continue;
                            }
                        }
                        
                        if (!blnDoUIUpdate)
                            return true;
                        await AddSpell(objXmlSpell, strSpellCategory);
                    }
                }

                if (blnDoUIUpdate)
                {
                    lstSpellItems.Sort(CompareListItems.CompareNames);
                    string strOldSelected = lstSpells.SelectedValue?.ToString();
                    _blnLoading = true;
                    lstSpells.BeginUpdate();
                    lstSpells.PopulateWithListItems(lstSpellItems);
                    _blnLoading = false;
                    if (!string.IsNullOrEmpty(strOldSelected))
                        lstSpells.SelectedValue = strOldSelected;
                    else
                        lstSpells.SelectedIndex = -1;
                    lstSpells.EndUpdate();
                }

                async ValueTask AddSpell(XPathNavigator objXmlSpell, string strSpellCategory)
                {
                    string strDisplayName = (await objXmlSpell.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ??
                                            objXmlSpell.SelectSingleNode("name")?.Value ??
                                            await LanguageManager.GetStringAsync("String_Unknown");
                    if (!GlobalSettings.SearchInCategoryOnly && txtSearch.TextLength != 0
                                                             && !string.IsNullOrEmpty(strSpellCategory))
                    {
                        ListItem objFoundItem
                            = _lstCategory.Find(objFind => objFind.Value.ToString() == strSpellCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += strSpace + '[' + objFoundItem.Name + ']';
                        }
                    }

                    lstSpellItems.Add(new ListItem(objXmlSpell.SelectSingleNode("id")?.Value ?? string.Empty,
                                                   strDisplayName));
                }

                return lstSpellItems.Count > 0;
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm()
        {
            string strSelectedItem = lstSpells.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedItem))
                return;

            // Display the Spell information.
            XPathNavigator objXmlSpell = _xmlBaseSpellDataNode.SelectSingleNode("spells/spell[id = " + strSelectedItem.CleanXPath() + ']');
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
                            Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit"), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (objXmlSpell?.SelectSingleNode("category")?.Value == "Rituals")
                    {
                        if (intRitualCount >= intSpellLimit)
                        {
                            Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit"), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (intSpellCount >= intSpellLimit)
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit"),
                            await LanguageManager.GetStringAsync("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                if (!objXmlSpell.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync("String_DescSpell")))
                {
                    return;
                }
            }

            _strSelectedSpell = strSelectedItem;
            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0)
                ? cboCategory.SelectedValue?.ToString()
                : _xmlBaseSpellDataNode.SelectSingleNode("/chummer/spells/spell[id = " + _strSelectedSpell.CleanXPath() + "]/category")?.Value ?? string.Empty;
            FreeBonus = chkFreeBonus.Checked;
            DialogResult = DialogResult.OK;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        private async ValueTask UpdateSpellInfo()
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
                xmlSpell = _xmlBaseSpellDataNode.SelectSingleNode("/chummer/spells/spell[id = " + strSelectedSpellId.CleanXPath() + ']');
            }
            if (xmlSpell == null)
            {
                tlpRight.Visible = false;
                return;
            }

            string strSpace = await LanguageManager.GetStringAsync("String_Space");

            bool blnExtendedFound = false;
            bool blnAlchemicalFound = false;
            string strDescriptors = xmlSpell.SelectSingleNode("descriptor")?.Value;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDescriptors))
            {
                if (!string.IsNullOrEmpty(strDescriptors))
                {
                    foreach (string strDescriptor in strDescriptors.SplitNoAlloc(
                                 ',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        switch (strDescriptor.Trim())
                        {
                            case "Alchemical Preparation":
                                blnAlchemicalFound = true;
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescAlchemicalPreparation"));
                                break;

                            case "Extended Area":
                                blnExtendedFound = true;
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescExtendedArea"));
                                break;

                            case "Material Link":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescMaterialLink"));
                                break;

                            case "Multi-Sense":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescMultiSense"));
                                break;

                            case "Organic Link":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescOrganicLink"));
                                break;

                            case "Single-Sense":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescSingleSense"));
                                break;

                            default:
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_Desc" + strDescriptor.Trim()));
                                break;
                        }

                        sbdDescriptors.Append(',').Append(strSpace);
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
                    sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescExtendedArea")).Append(',')
                                  .Append(strSpace);
                }

                if (chkAlchemical.Checked && !blnAlchemicalFound)
                {
                    sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescAlchemicalPreparation")).Append(',')
                                  .Append(strSpace);
                }

                // Remove the trailing comma.
                if (sbdDescriptors.Length > 2)
                    sbdDescriptors.Length -= 2;
                lblDescriptors.Text = sbdDescriptors.ToString();
            }

            if (string.IsNullOrEmpty(lblDescriptors.Text))
                lblDescriptors.Text = await LanguageManager.GetStringAsync("String_None");
            lblDescriptorsLabel.Visible = !string.IsNullOrEmpty(lblDescriptors.Text);

            switch (xmlSpell.SelectSingleNode("type")?.Value)
            {
                case "M":
                    lblType.Text = await LanguageManager.GetStringAsync("String_SpellTypeMana");
                    break;

                default:
                    lblType.Text = await LanguageManager.GetStringAsync("String_SpellTypePhysical");
                    break;
            }
            lblTypeLabel.Visible = !string.IsNullOrEmpty(lblType.Text);

            switch (xmlSpell.SelectSingleNode("duration")?.Value)
            {
                case "P":
                    lblDuration.Text = await LanguageManager.GetStringAsync("String_SpellDurationPermanent");
                    break;

                case "S":
                    lblDuration.Text = await LanguageManager.GetStringAsync("String_SpellDurationSustained");
                    break;

                default:
                    lblDuration.Text = await LanguageManager.GetStringAsync("String_SpellDurationInstant");
                    break;
            }
            lblDurationLabel.Visible = !string.IsNullOrEmpty(lblDuration.Text);

            if (blnExtendedFound)
            {
                chkExtended.Visible = true;
                chkExtended.Checked = true;
                chkExtended.Enabled = false;
            }
            else if (_objCharacter.Settings.ExtendAnyDetectionSpell && xmlSpell.SelectSingleNode("category")?.Value == "Detection")
            {
                chkExtended.Visible = true;
                if (!chkExtended.Enabled) // Resets this checkbox if we just selected an Extended Area spell
                    chkExtended.Checked = false;
                chkExtended.Enabled = true;
            }
            else
            {
                chkExtended.Checked = false;
                chkExtended.Visible = false;
            }

            string strRange = xmlSpell.SelectSingleNode("range")?.Value ?? string.Empty;
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strRange = await strRange
                    .CheapReplaceAsync("Self", () => LanguageManager.GetStringAsync("String_SpellRangeSelf"))
                    .CheapReplaceAsync("LOS", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfSight"))
                    .CheapReplaceAsync("LOI", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfInfluence"))
                    .CheapReplaceAsync("Touch", () => LanguageManager.GetStringAsync("String_SpellRangeTouchLong"))
                    .CheapReplaceAsync("T", () => LanguageManager.GetStringAsync("String_SpellRangeTouch"))
                    .CheapReplaceAsync("(A)", async () => '(' + await LanguageManager.GetStringAsync("String_SpellRangeArea") + ')')
                    .CheapReplaceAsync("MAG", () => LanguageManager.GetStringAsync("String_AttributeMAGShort"));
            }
            lblRange.Text = strRange;
            lblRangeLabel.Visible = !string.IsNullOrEmpty(lblRange.Text);

            switch (xmlSpell.SelectSingleNode("damage")?.Value)
            {
                case "P":
                    lblDamageLabel.Visible = true;
                    lblDamage.Text = await LanguageManager.GetStringAsync("String_DamagePhysical");
                    break;

                case "S":
                    lblDamageLabel.Visible = true;
                    lblDamage.Text = await LanguageManager.GetStringAsync("String_DamageStun");
                    break;

                default:
                    lblDamageLabel.Visible = false;
                    lblDamage.Text = string.Empty;
                    break;
            }

            string strDV = xmlSpell.SelectSingleNode("dv")?.Value.Replace('/', '÷').Replace('*', '×') ?? string.Empty;
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strDV = await strDV.CheapReplaceAsync("F", () => LanguageManager.GetStringAsync("String_SpellForce"))
                    .CheapReplaceAsync("Overflow damage", () => LanguageManager.GetStringAsync("String_SpellOverflowDamage"))
                    .CheapReplaceAsync("Damage Value", () => LanguageManager.GetStringAsync("String_SpellDamageValue"))
                    .CheapReplaceAsync("Toxin DV", () => LanguageManager.GetStringAsync("String_SpellToxinDV"))
                    .CheapReplaceAsync("Disease DV", () => LanguageManager.GetStringAsync("String_SpellDiseaseDV"))
                    .CheapReplaceAsync("Radiation Power",
                        () => LanguageManager.GetStringAsync("String_SpellRadiationPower"));
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
            if (Extended && !blnExtendedFound)
            {
                strDV += " + 2";
            }
            object xprResult = CommonFunctions.EvaluateInvariantXPath(strDV.TrimStart('+'), out bool blnIsSuccess);
            if (blnIsSuccess)
            {
                if (force)
                {
                    strDV = string.Format(GlobalSettings.CultureInfo, "F{0:+0;-0;}", xprResult);
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

            string strSource = xmlSpell.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            string strPage = (await xmlSpell.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? xmlSpell.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                             GlobalSettings.CultureInfo, _objCharacter);
            lblSource.Text = objSource.ToString();
            lblSource.SetToolTip(objSource.LanguageBookTooltip);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            tlpRight.Visible = true;
            _blnRefresh = false;
        }

        #endregion Methods
    }
}
