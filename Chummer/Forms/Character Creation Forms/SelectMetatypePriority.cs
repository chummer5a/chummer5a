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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class SelectMetatypePriority : Form
    {
        private readonly Character _objCharacter;
        private string _strCurrentPossessionMethod;

        private bool _blnLoading = true;
        private readonly Dictionary<string, int> _dicSumtoTenValues;
        private readonly List<string> _lstPrioritySkills;
        private readonly List<string> _lstPriorities;

        private readonly XPathNavigator _xmlBasePriorityDataNode;
        private readonly XPathNavigator _xmlBaseMetatypeDataNode;
        private readonly XPathNavigator _xmlBaseSkillDataNode;
        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XmlNode _xmlMetatypeDocumentMetatypesNode;
        private readonly XmlNode _xmlQualityDocumentQualitiesNode;
        private readonly XmlNode _xmlCritterPowerDocumentPowersNode;

        #region Form Events

        public SelectMetatypePriority(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            if (string.IsNullOrEmpty(_objCharacter.SettingsKey))
                _objCharacter.SettingsKey = GlobalSettings.DefaultCharacterSetting;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _lstPrioritySkills = new List<string>(objCharacter.PriorityBonusSkillList);
            _xmlMetatypeDocumentMetatypesNode = _objCharacter.LoadData(strXmlFile).SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = _objCharacter.LoadDataXPath(strXmlFile).SelectSingleNodeAndCacheExpression("/chummer");
            _xmlBasePriorityDataNode = _objCharacter.LoadDataXPath("priorities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlBaseSkillDataNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlQualityDocumentQualitiesNode = _objCharacter.LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlCritterPowerDocumentPowersNode = _objCharacter.LoadData("critterpowers.xml").SelectSingleNode("/chummer/powers");
            _dicSumtoTenValues = new Dictionary<string, int>(5);
            if (_xmlBasePriorityDataNode != null)
            {
                foreach (XPathNavigator xmlNode in _xmlBasePriorityDataNode.SelectAndCacheExpression("priortysumtotenvalues/*"))
                {
                    _dicSumtoTenValues.Add(xmlNode.Name, xmlNode.ValueAsInt);
                }
            }

            if (_dicSumtoTenValues.Count == 0)
            {
                _dicSumtoTenValues.Add("A", 4);
                _dicSumtoTenValues.Add("B", 3);
                _dicSumtoTenValues.Add("C", 2);
                _dicSumtoTenValues.Add("D", 1);
                _dicSumtoTenValues.Add("E", 0);
            }

            if (!string.IsNullOrEmpty(_objCharacter.Settings.PriorityArray))
            {
                _lstPriorities = new List<string>(5);
                foreach (char c in _objCharacter.Settings.PriorityArray)
                {
                    _lstPriorities.Add(c.ToString());
                }
            }
            else
            {
                _lstPriorities = new List<string> { "A", "B", "C", "D", "E" };
            }

            foreach (string strPriority in _lstPriorities)
                if (!_dicSumtoTenValues.ContainsKey(strPriority))
                    _dicSumtoTenValues.Add(strPriority, 0);
        }

        private async void SelectMetatypePriority_Load(object sender, EventArgs e)
        {
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                lblSumtoTen.Visible = true;
            }

            // Populate the Priority Category list.
            XPathNavigator xmlBasePrioritiesNode = _xmlBasePriorityDataNode.SelectSingleNodeAndCacheExpression("priorities");
            if (xmlBasePrioritiesNode != null)
            {
                foreach (XPathNavigator objXmlPriorityCategory in _xmlBasePriorityDataNode.SelectAndCacheExpression("categories/category"))
                {
                    XPathNodeIterator objItems = xmlBasePrioritiesNode.Select("priority[category = " + objXmlPriorityCategory.Value.CleanXPath() + " and prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ']');

                    if (objItems.Count == 0)
                    {
                        objItems = xmlBasePrioritiesNode.Select("priority[category = " + objXmlPriorityCategory.Value.CleanXPath() + " and not(prioritytable)]");
                    }

                    if (objItems.Count > 0)
                    {
                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstItems))
                        {
                            foreach (XPathNavigator objXmlPriority in objItems)
                            {
                                string strValue = objXmlPriority.SelectSingleNodeAndCacheExpression("value")?.Value;
                                if (!string.IsNullOrEmpty(strValue) && _lstPriorities.Contains(strValue))
                                {
                                    lstItems.Add(new ListItem(
                                                     strValue,
                                                     objXmlPriority.SelectSingleNodeAndCacheExpression("translate")
                                                                   ?.Value ??
                                                     objXmlPriority.SelectSingleNodeAndCacheExpression("name")?.Value ??
                                                     await LanguageManager.GetStringAsync("String_Unknown")));
                                }
                            }

                            lstItems.Sort(CompareListItems.CompareNames);
                            switch (objXmlPriorityCategory.Value)
                            {
                                case "Heritage":
                                    cboHeritage.BeginUpdate();
                                    cboHeritage.PopulateWithListItems(lstItems);
                                    cboHeritage.EndUpdate();
                                    break;

                                case "Talent":
                                    cboTalent.BeginUpdate();
                                    cboTalent.PopulateWithListItems(lstItems);
                                    cboTalent.EndUpdate();
                                    break;

                                case "Attributes":
                                    cboAttributes.BeginUpdate();
                                    cboAttributes.PopulateWithListItems(lstItems);
                                    cboAttributes.EndUpdate();
                                    break;

                                case "Skills":
                                    cboSkills.BeginUpdate();
                                    cboSkills.PopulateWithListItems(lstItems);
                                    cboSkills.EndUpdate();
                                    break;

                                case "Resources":
                                    cboResources.BeginUpdate();
                                    cboResources.PopulateWithListItems(lstItems);
                                    cboResources.EndUpdate();
                                    break;
                            }
                        }
                    }
                }
            }

            // Set Priority defaults.
            if (!string.IsNullOrEmpty(_objCharacter.TalentPriority))
            {
                //Attributes
                cboAttributes.SelectedIndex = cboAttributes.FindString(_objCharacter.AttributesPriority[0].ToString(GlobalSettings.InvariantCultureInfo));
                //Heritage (Metatype)
                cboHeritage.SelectedIndex = cboHeritage.FindString(_objCharacter.MetatypePriority[0].ToString(GlobalSettings.InvariantCultureInfo));
                //Resources
                cboResources.SelectedIndex = cboResources.FindString(_objCharacter.ResourcesPriority[0].ToString(GlobalSettings.InvariantCultureInfo));
                //Skills
                cboSkills.SelectedIndex = cboSkills.FindString(_objCharacter.SkillsPriority[0].ToString(GlobalSettings.InvariantCultureInfo));
                //Magical/Resonance Talent
                cboTalent.SelectedIndex = cboTalent.FindString(_objCharacter.SpecialPriority[0].ToString(GlobalSettings.InvariantCultureInfo));

                LoadMetatypes();
                PopulateMetatypes();
                PopulateMetavariants();
                PopulateTalents();
                await RefreshSelectedMetatype();

                //Magical/Resonance Type
                cboTalents.SelectedValue = _objCharacter.TalentPriority;
                if (cboTalents.SelectedIndex == -1 && cboTalents.Items.Count > 1)
                    cboTalents.SelectedIndex = 0;
                //Selected Magical Bonus Skill
                string strSkill = _lstPrioritySkills.ElementAtOrDefault(0);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    if (ExoticSkill.IsExoticSkillName(strSkill))
                    {
                        int intParenthesesIndex = strSkill.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex > 0)
                            strSkill = strSkill.Substring(0, intParenthesesIndex);
                    }
                    cboSkill1.SelectedValue = strSkill;
                }
                strSkill = _lstPrioritySkills.ElementAtOrDefault(1);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    if (ExoticSkill.IsExoticSkillName(strSkill))
                    {
                        int intParenthesesIndex = strSkill.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex > 0)
                            strSkill = strSkill.Substring(0, intParenthesesIndex);
                    }
                    cboSkill2.SelectedValue = strSkill;
                }
                strSkill = _lstPrioritySkills.ElementAtOrDefault(2);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    if (ExoticSkill.IsExoticSkillName(strSkill))
                    {
                        int intParenthesesIndex = strSkill.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex > 0)
                            strSkill = strSkill.Substring(0, intParenthesesIndex);
                    }
                    cboSkill3.SelectedValue = strSkill;
                }
                cboTalents_SelectedIndexChanged(null, EventArgs.Empty);

                switch (_objCharacter.EffectiveBuildMethod)
                {
                    case CharacterBuildMethod.Priority:
                        ManagePriorityItems(cboHeritage);
                        ManagePriorityItems(cboAttributes);
                        ManagePriorityItems(cboTalent);
                        ManagePriorityItems(cboSkills);
                        ManagePriorityItems(cboResources);
                        break;

                    case CharacterBuildMethod.SumtoTen:
                        SumToTen();
                        break;
                }
            }
            else
            {
                cboHeritage.SelectedIndex = 0;
                cboTalent.SelectedIndex = 0;
                cboAttributes.SelectedIndex = 0;
                cboSkills.SelectedIndex = 0;
                cboResources.SelectedIndex = 0;
                ManagePriorityItems(cboHeritage, true);
                ManagePriorityItems(cboAttributes, true);
                ManagePriorityItems(cboTalent, true);
                ManagePriorityItems(cboSkills, true);
                ManagePriorityItems(cboResources, true);
                LoadMetatypes();
                PopulateMetatypes();
                PopulateMetavariants();
                PopulateTalents();
                await RefreshSelectedMetatype();
                if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                    SumToTen();
            }

            // Set up possession boxes
            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            chkPossessionBased.SetToolTip(await LanguageManager.GetStringAsync("Tip_Metatype_PossessionTradition"));

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMethods))
            {
                lstMethods.Add(new ListItem("Possession",
                                            _xmlCritterPowerDocumentPowersNode
                                                ?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText
                                            ?? "Possession"));
                lstMethods.Add(new ListItem("Inhabitation",
                                            _xmlCritterPowerDocumentPowersNode
                                                ?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")
                                                ?.InnerText
                                            ?? "Inhabitation"));
                lstMethods.Sort(CompareListItems.CompareNames);

                _strCurrentPossessionMethod = _objCharacter.CritterPowers.Select(x => x.Name)
                                                           .FirstOrDefault(
                                                               y => lstMethods.Any(
                                                                   x => y.Equals(
                                                                       x.Value.ToString(),
                                                                       StringComparison.OrdinalIgnoreCase)));

                cboPossessionMethod.BeginUpdate();
                cboPossessionMethod.PopulateWithListItems(lstMethods);
                cboPossessionMethod.EndUpdate();
            }

            _blnLoading = false;
        }

        #endregion Form Events

        #region Control Events

        private async void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumToTen();
            }
            PopulateMetavariants();
            await RefreshSelectedMetatype();
            PopulateTalents();
            ResumeLayout();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cboTalents_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuspendLayout();
            cboSkill1.BeginUpdate();
            cboSkill2.BeginUpdate();
            cboSkill3.BeginUpdate();

            cboSkill1.Visible = false;
            cboSkill2.Visible = false;
            cboSkill3.Visible = false;
            lblMetatypeSkillSelection.Visible = false;

            string strSelectedTalents = cboTalents.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedTalents))
            {
                XPathNavigator xmlTalentNode = null;
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                    {
                        xmlTalentNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + strSelectedTalents.CleanXPath() + ']');
                        break;
                    }
                }

                if (xmlTalentNode != null)
                {
                    string strSkillCount = xmlTalentNode.SelectSingleNodeAndCacheExpression("skillqty")?.Value ?? xmlTalentNode.SelectSingleNodeAndCacheExpression("skillgroupqty")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSkillCount) && int.TryParse(strSkillCount, out int intSkillCount))
                    {
                        XPathNavigator xmlSkillTypeNode = xmlTalentNode.SelectSingleNodeAndCacheExpression("skilltype") ?? xmlTalentNode.SelectSingleNodeAndCacheExpression("skillgrouptype");
                        string strSkillType = xmlSkillTypeNode?.Value ?? string.Empty;
                        XPathNodeIterator objNodeList = xmlTalentNode.SelectAndCacheExpression("skillgroupchoices/skillgroup");
                        XPathNodeIterator xmlSkillsList;
                        switch (strSkillType)
                        {
                            case "magic":
                                xmlSkillsList = GetMagicalSkillList();
                                break;

                            case "resonance":
                                xmlSkillsList = GetResonanceSkillList();
                                break;

                            case "matrix":
                                xmlSkillsList = GetMatrixSkillList();
                                break;

                            case "grouped":
                                xmlSkillsList = BuildSkillCategoryList(objNodeList);
                                break;

                            case "specific":
                                xmlSkillsList = BuildSkillList(xmlTalentNode.SelectAndCacheExpression("skillchoices/skill"));
                                break;

                            case "xpath":
                                xmlSkillsList = GetActiveSkillList(xmlSkillTypeNode?.SelectSingleNodeAndCacheExpression("@xpath")?.Value);
                                strSkillType = "active";
                                break;

                            default:
                                xmlSkillsList = GetActiveSkillList();
                                break;
                        }

                        if (intSkillCount > 0)
                        {
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstSkills))
                            {
                                if (objNodeList.Count > 0)
                                {
                                    foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                    {
                                        string strInnerText = objXmlSkill.Value;
                                        lstSkills.Add(new ListItem(strInnerText,
                                                                   objXmlSkill.SelectSingleNodeAndCacheExpression(
                                                                       "@translate")?.Value ?? strInnerText));
                                    }
                                }
                                else
                                {
                                    foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                    {
                                        string strName = objXmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value
                                                         ?? LanguageManager.GetString("String_Unknown");
                                        lstSkills.Add(
                                            new ListItem(
                                                strName,
                                                objXmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                ?? strName));
                                    }
                                }

                                lstSkills.Sort(CompareListItems.CompareNames);
                                bool blnOldLoading = _blnLoading;
                                int intOldSelectedIndex = cboSkill1.SelectedIndex;
                                int intOldDataSourceSize = cboSkill1.Items.Count;
                                cboSkill1.PopulateWithListItems(lstSkills);
                                cboSkill1.Visible = true;
                                if (intOldDataSourceSize == cboSkill1.Items.Count)
                                {
                                    _blnLoading = true;
                                    cboSkill1.SelectedIndex = intOldSelectedIndex;
                                    _blnLoading = blnOldLoading;
                                }

                                if (intSkillCount > 1)
                                {
                                    intOldSelectedIndex = cboSkill2.SelectedIndex;
                                    intOldDataSourceSize = cboSkill2.Items.Count;
                                    cboSkill2.PopulateWithListItems(lstSkills);
                                    cboSkill2.Visible = true;
                                    if (intOldDataSourceSize == cboSkill2.Items.Count)
                                    {
                                        _blnLoading = true;
                                        cboSkill2.SelectedIndex = intOldSelectedIndex;
                                        _blnLoading = blnOldLoading;
                                    }

                                    if (cboSkill2.SelectedIndex == cboSkill1.SelectedIndex
                                        && !ExoticSkill.IsExoticSkillName(cboSkill2.SelectedValue?.ToString()))
                                    {
                                        if (cboSkill2.SelectedIndex + 1 >= cboSkill2.Items.Count)
                                            cboSkill2.SelectedIndex = 0;
                                        else
                                            cboSkill2.SelectedIndex = cboSkill1.SelectedIndex + 1;
                                    }

                                    if (intSkillCount > 2)
                                    {
                                        intOldSelectedIndex = cboSkill3.SelectedIndex;
                                        intOldDataSourceSize = cboSkill3.Items.Count;
                                        cboSkill3.PopulateWithListItems(lstSkills);
                                        cboSkill3.Visible = true;
                                        if (intOldDataSourceSize == cboSkill3.Items.Count)
                                        {
                                            _blnLoading = true;
                                            cboSkill3.SelectedIndex = intOldSelectedIndex;
                                            _blnLoading = blnOldLoading;
                                        }

                                        if ((cboSkill3.SelectedIndex == cboSkill1.SelectedIndex ||
                                             cboSkill3.SelectedIndex == cboSkill2.SelectedIndex) &&
                                            !ExoticSkill.IsExoticSkillName(cboSkill3.SelectedValue?.ToString()))
                                        {
                                            int intNewIndex = cboSkill3.SelectedIndex;
                                            do
                                            {
                                                ++intNewIndex;
                                                if (intNewIndex >= cboSkill3.Items.Count)
                                                    intNewIndex = 0;
                                            } while ((intNewIndex == cboSkill1.SelectedIndex
                                                      || intNewIndex == cboSkill2.SelectedIndex)
                                                     && intNewIndex != cboSkill3.SelectedIndex &&
                                                     !ExoticSkill.IsExoticSkillName(
                                                         ((ListItem) cboSkill3.Items[intNewIndex]).Value.ToString()));

                                            cboSkill3.SelectedIndex = intNewIndex;
                                        }
                                    }
                                }

                                string strMetamagicSkillSelection = string.Format(
                                    GlobalSettings.CultureInfo, LanguageManager.GetString("String_MetamagicSkillBase"),
                                    LanguageManager.GetString("String_MetamagicSkills"));
                                // strSkillType can have the following values: magic, resonance, matrix, active, specific, grouped
                                // So the language file should contain each of those like String_MetamagicSkillType_magic
                                string strSkillVal = xmlTalentNode.SelectSingleNodeAndCacheExpression("skillval")?.Value
                                                     ?? xmlTalentNode
                                                        .SelectSingleNodeAndCacheExpression("skillgroupval")
                                                        ?.Value;
                                lblMetatypeSkillSelection.Text = string.Format(
                                    GlobalSettings.CultureInfo, strMetamagicSkillSelection, strSkillCount,
                                    LanguageManager.GetString("String_MetamagicSkillType_" + strSkillType),
                                    strSkillVal);
                                lblMetatypeSkillSelection.Visible = true;
                            }
                        }

                        int intSpecialAttribPoints = 0;

                        string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
                        if (!string.IsNullOrEmpty(strSelectedMetatype))
                        {
                            string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
                            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + (cboHeritage.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                            {
                                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                                {
                                    XPathNavigator objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                                    if (!string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                                        objXmlMetatypePriorityNode = objXmlMetatypePriorityNode?.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + ']');
                                    if (int.TryParse(objXmlMetatypePriorityNode?.SelectSingleNodeAndCacheExpression("value")?.Value, out int intTemp))
                                        intSpecialAttribPoints += intTemp;
                                    break;
                                }
                            }
                        }

                        if (int.TryParse(xmlTalentNode.SelectSingleNodeAndCacheExpression("specialattribpoints")?.Value, out int intTalentSpecialAttribPoints))
                            intSpecialAttribPoints += intTalentSpecialAttribPoints;

                        lblSpecialAttributes.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo);
                    }
                }
            }
            else
            {
                cboTalents.SelectedIndex = 0;
            }
            cboSkill1.EndUpdate();
            cboSkill2.EndUpdate();
            cboSkill3.EndUpdate();
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumToTen();
            }
            ResumeLayout();
        }

        private async void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            await RefreshSelectedMetatype();
            PopulateTalents();
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumToTen();
            }
            ResumeLayout();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            PopulateMetatypes();
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumToTen();
            }
            ResumeLayout();
        }

        private async void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            switch (_objCharacter.EffectiveBuildMethod)
            {
                case CharacterBuildMethod.Priority:
                    ManagePriorityItems(cboHeritage);
                    break;

                case CharacterBuildMethod.SumtoTen:
                    SumToTen();
                    break;
            }
            LoadMetatypes();
            PopulateMetatypes();
            PopulateMetavariants();
            await RefreshSelectedMetatype();
            ResumeLayout();
        }

        private void cboTalent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            switch (_objCharacter.EffectiveBuildMethod)
            {
                case CharacterBuildMethod.Priority:
                    ManagePriorityItems(cboTalent);
                    break;

                case CharacterBuildMethod.SumtoTen:
                    SumToTen();
                    break;
            }
            PopulateTalents();
            ResumeLayout();
        }

        private void cboAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            switch (_objCharacter.EffectiveBuildMethod)
            {
                case CharacterBuildMethod.Priority:
                    ManagePriorityItems(cboAttributes);
                    break;

                case CharacterBuildMethod.SumtoTen:
                    SumToTen();
                    break;
            }
            ResumeLayout();
        }

        private void cboSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            switch (_objCharacter.EffectiveBuildMethod)
            {
                case CharacterBuildMethod.Priority:
                    ManagePriorityItems(cboSkills);
                    break;

                case CharacterBuildMethod.SumtoTen:
                    SumToTen();
                    break;
            }
            ResumeLayout();
        }

        private void cboResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            switch (_objCharacter.EffectiveBuildMethod)
            {
                case CharacterBuildMethod.Priority:
                    ManagePriorityItems(cboResources);
                    break;

                case CharacterBuildMethod.SumtoTen:
                    SumToTen();
                    break;
            }
            ResumeLayout();
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }

        #endregion Control Events

        #region Custom Methods

        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private void MetatypeSelected()
        {
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                int intSumToTen = SumToTen(false);
                if (intSumToTen != _objCharacter.Settings.SumtoTen)
                {
                    Program.MainForm.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_SumtoTen"),
                        _objCharacter.Settings.SumtoTen.ToString(GlobalSettings.CultureInfo), intSumToTen.ToString(GlobalSettings.CultureInfo)));
                    return;
                }
            }
            if (cboTalents.SelectedIndex == -1)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectTalent"), LanguageManager.GetString("MessageTitle_Metatype_SelectTalent"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSkill1 = cboSkill1.SelectedValue?.ToString();
            string strSkill2 = cboSkill2.SelectedValue?.ToString();
            string strSkill3 = cboSkill3.SelectedValue?.ToString();

            if ((cboSkill1.Visible && string.IsNullOrEmpty(strSkill1))
                || (cboSkill2.Visible && string.IsNullOrEmpty(strSkill2))
                || (cboSkill3.Visible && string.IsNullOrEmpty(strSkill3)))
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectSkill"), LanguageManager.GetString("MessageTitle_Metatype_SelectSkill"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (ExoticSkill.IsExoticSkillName(strSkill1))
            {
                using (SelectExoticSkill frmSelectExotic = new SelectExoticSkill(_objCharacter))
                {
                    frmSelectExotic.ForceSkill(strSkill1);
                    if (frmSelectExotic.ShowDialogSafe(this) != DialogResult.OK)
                        return;
                    strSkill1 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }
            if (ExoticSkill.IsExoticSkillName(strSkill2))
            {
                using (SelectExoticSkill frmSelectExotic = new SelectExoticSkill(_objCharacter))
                {
                    frmSelectExotic.ForceSkill(strSkill2);
                    if (frmSelectExotic.ShowDialogSafe(this) != DialogResult.OK)
                        return;
                    strSkill2 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }
            if (ExoticSkill.IsExoticSkillName(strSkill3))
            {
                using (SelectExoticSkill frmSelectExotic = new SelectExoticSkill(_objCharacter))
                {
                    frmSelectExotic.ForceSkill(strSkill3);
                    if (frmSelectExotic.ShowDialogSafe(this) != DialogResult.OK)
                        return;
                    strSkill3 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }

            if ((cboSkill1.Visible && cboSkill2.Visible && strSkill1 == strSkill2)
                || (cboSkill1.Visible && cboSkill3.Visible && strSkill1 == strSkill3)
                || (cboSkill2.Visible && cboSkill3.Visible && strSkill2 == strSkill3))
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_Duplicate"), LanguageManager.GetString("MessageTitle_Metatype_Duplicate"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (new CursorWait(this))
            {
                string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strSelectedMetatype))
                {
                    XmlNode objXmlMetatype = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode("metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                    if (objXmlMetatype == null)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Clear out all priority-only qualities that the character bought normally (relevant when switching from Karma to Priority/Sum-to-Ten)
                    for (int i = _objCharacter.Qualities.Count - 1; i >= 0; --i)
                    {
                        if (i >= _objCharacter.Qualities.Count)
                            continue;
                        Quality objQuality = _objCharacter.Qualities[i];
                        if (objQuality.OriginSource == QualitySource.Selected
                            && objQuality.GetNodeXPath()?.SelectSingleNode("onlyprioritygiven") != null)
                            objQuality.DeleteQuality();
                    }

                    string strSelectedMetavariant = cboMetavariant.SelectedValue.ToString();
                    string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                        strSelectedMetavariant = "Human";
                    XmlNode objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + ']');
                    strSelectedMetavariant = objXmlMetavariant?["id"]?.InnerText ?? Guid.Empty.ToString();
                    int intForce = nudForce.Visible ? nudForce.ValueAsInt : 0;

                    if (_objCharacter.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetatype
                        || _objCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetavariant)
                    {
                        // Remove qualities that require the old metatype
                        List<Quality> lstQualitiesToCheck = new List<Quality>(_objCharacter.Qualities.Count);
                        foreach (Quality objQuality in _objCharacter.Qualities)
                        {
                            if (objQuality.OriginSource == QualitySource.Improvement
                                || objQuality.OriginSource == QualitySource.Heritage
                                || objQuality.OriginSource == QualitySource.Metatype
                                || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                || objQuality.OriginSource == QualitySource.MetatypeRemovedAtChargen)
                                continue;
                            XPathNavigator xmlRestrictionNode = objQuality.GetNodeXPath()?.SelectSingleNode("required");
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNode(".//metatype") != null || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode = objQuality.GetNodeXPath()?.SelectSingleNode("forbidden");
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNode(".//metatype") != null || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                                {
                                    lstQualitiesToCheck.Add(objQuality);
                                }
                            }
                        }
                        _objCharacter.Create(strSelectedMetatypeCategory, objXmlMetatype["id"]?.InnerText,
                            strSelectedMetavariant, objXmlMetatype, intForce, _xmlQualityDocumentQualitiesNode,
                            _xmlCritterPowerDocumentPowersNode, null, chkPossessionBased.Checked ? cboPossessionMethod.SelectedValue?.ToString() : string.Empty);
                        foreach (Quality objQuality in lstQualitiesToCheck)
                        {
                            // Set strIgnoreQuality to quality's name to make sure limit counts are not an issue
                            if (objQuality.GetNodeXPath()?.RequirementsMet(_objCharacter, strIgnoreQuality: objQuality.Name) == false)
                            {
                                objQuality.DeleteQuality();
                            }
                        }
                    }

                    string strOldSpecialPriority = _objCharacter.SpecialPriority;
                    string strOldTalentPriority = _objCharacter.TalentPriority;

                    // begin priority based character settings
                    // Load the Priority information.

                    // Set the character priority selections
                    _objCharacter.MetatypeBP = Convert.ToInt32(lblMetavariantKarma.Text, GlobalSettings.CultureInfo);
                    _objCharacter.MetatypePriority = cboHeritage.SelectedValue.ToString();
                    _objCharacter.AttributesPriority = cboAttributes.SelectedValue.ToString();
                    _objCharacter.SpecialPriority = cboTalent.SelectedValue.ToString();
                    _objCharacter.SkillsPriority = cboSkills.SelectedValue.ToString();
                    _objCharacter.ResourcesPriority = cboResources.SelectedValue.ToString();
                    _objCharacter.TalentPriority = cboTalents.SelectedValue.ToString();
                    _objCharacter.PriorityBonusSkillList.Clear();
                    if (cboSkill1.Visible)
                        _objCharacter.PriorityBonusSkillList.Add(strSkill1);
                    if (cboSkill2.Visible)
                        _objCharacter.PriorityBonusSkillList.Add(strSkill2);
                    if (cboSkill3.Visible)
                        _objCharacter.PriorityBonusSkillList.Add(strSkill3);

                    // Set starting nuyen
                    XPathNodeIterator xmlResourcesPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Resources\" and value = " + _objCharacter.ResourcesPriority.CleanXPath() +
                                                                                                 " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlResourcesPriority in xmlResourcesPriorityList)
                    {
                        if (xmlResourcesPriorityList.Count == 1 ||
                            (xmlResourcesPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null &&
                             xmlResourcesPriority.SelectSingleNodeAndCacheExpression("gameplayoption") != null))
                        {
                            decimal decResources = 0;
                            if (xmlResourcesPriority.TryGetDecFieldQuickly("resources", ref decResources))
                                _objCharacter.StartingNuyen = _objCharacter.Nuyen = decResources;
                            break;
                        }
                    }

                    switch (cboTalents.SelectedValue)
                    {
                        case "Aspected Magician":
                        case "Enchanter":
                            _objCharacter.Pushtext.Push(strSkill1);
                            break;
                    }

                    XmlNode charNode =
                        strSelectedMetatypeCategory == "Shapeshifter" || strSelectedMetavariant == Guid.Empty.ToString()
                            ? objXmlMetatype
                            : objXmlMetavariant ?? objXmlMetatype;

                    bool boolHalveAttributePriorityPoints = charNode.NodeExists("halveattributepoints");
                    if (strOldSpecialPriority != _objCharacter.SpecialPriority || strOldTalentPriority != _objCharacter.SpecialPriority)
                    {
                        List<Quality> lstOldPriorityQualities = _objCharacter.Qualities.Where(x => x.OriginSource == QualitySource.Heritage).ToList();
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        bool blnRemoveFreeSkills = true;
                        XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + _objCharacter.SpecialPriority.CleanXPath() +
                                                                                                      " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                        foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                        {
                            if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("gameplayoption") != null)
                            {
                                XPathNavigator xmlTalentPriorityNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + _objCharacter.TalentPriority.CleanXPath() + ']');

                                if (xmlTalentPriorityNode != null)
                                {
                                    // Create the Qualities that come with the Talent.
                                    foreach (XPathNavigator objXmlQualityItem in xmlTalentPriorityNode.SelectAndCacheExpression("qualities/quality"))
                                    {
                                        XmlNode objXmlQuality = _xmlQualityDocumentQualitiesNode.SelectSingleNode("quality[name = " + objXmlQualityItem.Value.CleanXPath() + ']');
                                        Quality objQuality = new Quality(_objCharacter);
                                        string strForceValue = objXmlQualityItem.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                                        objQuality.Create(objXmlQuality, QualitySource.Heritage, lstWeapons, strForceValue);
                                        Quality objExistingQuality = lstOldPriorityQualities.Find(x => x.SourceIDString == objQuality.SourceIDString && x.Extra == objQuality.Extra && x.Type == objQuality.Type);
                                        if (objExistingQuality != null)
                                            lstOldPriorityQualities.Remove(objExistingQuality);
                                        else
                                            _objCharacter.Qualities.Add(objQuality);
                                    }

                                    foreach (Quality objQuality in lstOldPriorityQualities)
                                    {
                                        objQuality.DeleteQuality();
                                    }

                                    // Set starting magic
                                    int intTemp = 1;
                                    int intMax = 0;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("magic", ref intTemp))
                                        intTemp = 1;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxmagic", ref intMax))
                                        intMax = Math.Max(CommonFunctions.ExpressionToInt(
                                                              charNode["magmax"]?.InnerText, intForce), intTemp);
                                    _objCharacter.MAG.AssignLimits(intTemp, intMax, intMax);
                                    _objCharacter.FreeSpells = xmlTalentPriorityNode.TryGetInt32FieldQuickly("spells", ref intTemp) ? intTemp : 0;
                                    // Set starting resonance
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("resonance", ref intTemp))
                                        intTemp = 1;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxresonance", ref intMax))
                                        intMax = Math.Max(CommonFunctions.ExpressionToInt(
                                                              charNode["resmax"]?.InnerText, intForce), intTemp);
                                    _objCharacter.RES.AssignLimits(intTemp, intMax, intMax);
                                    _objCharacter.CFPLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("cfp", ref intTemp) ? intTemp : 0;
                                    // Set starting depth
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("depth", ref intTemp))
                                        intTemp = 1;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxdepth", ref intMax))
                                        intMax = Math.Max(CommonFunctions.ExpressionToInt(
                                                              charNode["depmax"]?.InnerText, intForce), intTemp);
                                    _objCharacter.DEP.AssignLimits(intTemp, intMax, intMax);
                                    _objCharacter.AINormalProgramLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("ainormalprogramlimit", ref intTemp) ? intTemp : 0;
                                    _objCharacter.AIAdvancedProgramLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("aiadvancedprogramlimit", ref intTemp) ? intTemp : 0;

                                    // Set Free Skills/Skill Groups
                                    int intFreeLevels = 0;
                                    Improvement.ImprovementType eType = Improvement.ImprovementType.SkillBase;
                                    XPathNavigator objTalentSkillValNode = xmlTalentPriorityNode.SelectSingleNodeAndCacheExpression("skillval");
                                    if (objTalentSkillValNode == null || !int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                    {
                                        objTalentSkillValNode = xmlTalentPriorityNode.SelectSingleNodeAndCacheExpression("skillgroupval");
                                        if (objTalentSkillValNode != null && int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                        {
                                            eType = Improvement.ImprovementType.SkillGroupBase;
                                        }
                                    }

                                    blnRemoveFreeSkills = false;
                                    AddFreeSkills(intFreeLevels, eType, strSkill1, strSkill2, strSkill3);
                                }

                                break;
                            }
                        }

                        if (blnRemoveFreeSkills)
                            ImprovementManager.RemoveImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Heritage
                                                                                                                       && (x.ImproveType == Improvement.ImprovementType.SkillBase
                                                                                                                           || x.ImproveType == Improvement.ImprovementType.SkillGroupBase)).ToList());
                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in lstWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }

                    // Set Special Attributes
                    _objCharacter.Special = Convert.ToInt32(lblSpecialAttributes.Text, GlobalSettings.CultureInfo);
                    _objCharacter.TotalSpecial = _objCharacter.Special;

                    // Set Attributes
                    XPathNodeIterator objXmlAttributesPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Attributes\" and value = " + _objCharacter.AttributesPriority.CleanXPath() +
                                                                                                     " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator objXmlAttributesPriority in objXmlAttributesPriorityList)
                    {
                        if (objXmlAttributesPriorityList.Count == 1 ||
                            (objXmlAttributesPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null &&
                             objXmlAttributesPriority.SelectSingleNodeAndCacheExpression("gameplayoption") != null))
                        {
                            int intAttributes = 0;
                            objXmlAttributesPriority.TryGetInt32FieldQuickly("attributes", ref intAttributes);
                            if (boolHalveAttributePriorityPoints)
                                intAttributes /= 2;
                            _objCharacter.TotalAttributes = _objCharacter.Attributes = intAttributes;
                            break;
                        }
                    }

                    // Set Skills and Skill Groups
                    XPathNodeIterator objXmlSkillsPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Skills\" and value = " + _objCharacter.SkillsPriority.CleanXPath() +
                                                                                                 " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator objXmlSkillsPriority in objXmlSkillsPriorityList)
                    {
                        if (objXmlSkillsPriorityList.Count == 1 ||
                            (objXmlSkillsPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null &&
                             objXmlSkillsPriority.SelectSingleNodeAndCacheExpression("gameplayoption") != null))
                        {
                            int intTemp = 0;
                            if (objXmlSkillsPriority.TryGetInt32FieldQuickly("skills", ref intTemp))
                                _objCharacter.SkillsSection.SkillPointsMaximum = intTemp;
                            if (objXmlSkillsPriority.TryGetInt32FieldQuickly("skillgroups", ref intTemp))
                                _objCharacter.SkillsSection.SkillGroupPointsMaximum = intTemp;
                            break;
                        }
                    }

                    // Sprites can never have Physical Attributes
                    if (_objCharacter.DEPEnabled || strSelectedMetatype.EndsWith("Sprite", StringComparison.Ordinal))
                    {
                        _objCharacter.BOD.AssignLimits(0, 0, 0);
                        _objCharacter.AGI.AssignLimits(0, 0, 0);
                        _objCharacter.REA.AssignLimits(0, 0, 0);
                        _objCharacter.STR.AssignLimits(0, 0, 0);
                        _objCharacter.MAG.AssignLimits(0, 0, 0);
                        _objCharacter.MAGAdept.AssignLimits(0, 0, 0);
                    }

                    // Set free contact points
                    _objCharacter.OnPropertyChanged(nameof(Character.ContactPoints));

                    // If we suspect the character converted from Karma to Priority/Sum-to-Ten, try to convert their Attributes, Skills, and Skill Groups to using points as efficiently as possible
                    bool blnDoSwitch = false;
                    foreach (CharacterAttrib objAttribute in _objCharacter.AttributeSection.AttributeList)
                    {
                        if (objAttribute.Base > 0)
                        {
                            blnDoSwitch = false;
                            break;
                        }
                        if (objAttribute.Karma > 0)
                            blnDoSwitch = true;
                    }
                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.TotalAttributes)
                        {
                            CharacterAttrib objAttributeToShift = null;
                            foreach (CharacterAttrib objAttribute in _objCharacter.AttributeSection.AttributeList)
                            {
                                if (objAttribute.Karma > 0 && (objAttributeToShift == null || objAttributeToShift.Value < objAttribute.Value))
                                {
                                    objAttributeToShift = objAttribute;
                                }
                            }
                            if (objAttributeToShift == null)
                                break;
                            int intKarma = Math.Min(objAttributeToShift.Karma, _objCharacter.TotalAttributes - intPointsSpent);
                            objAttributeToShift.Karma -= intKarma;
                            objAttributeToShift.Base += intKarma;
                            intPointsSpent += intKarma;
                        }
                    }
                    blnDoSwitch = false;
                    foreach (CharacterAttrib objAttribute in _objCharacter.AttributeSection.SpecialAttributeList)
                    {
                        if (objAttribute.Base > 0)
                        {
                            blnDoSwitch = false;
                            break;
                        }
                        if (objAttribute.Karma > 0)
                            blnDoSwitch = true;
                    }
                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.TotalSpecial)
                        {
                            CharacterAttrib objAttributeToShift = null;
                            foreach (CharacterAttrib objAttribute in _objCharacter.AttributeSection.SpecialAttributeList)
                            {
                                if (objAttribute.Karma > 0 && (objAttributeToShift == null || objAttributeToShift.Value < objAttribute.Value))
                                {
                                    objAttributeToShift = objAttribute;
                                }
                            }
                            if (objAttributeToShift == null)
                                break;
                            int intKarma = Math.Min(objAttributeToShift.Karma, _objCharacter.TotalSpecial - intPointsSpent);
                            objAttributeToShift.Karma -= intKarma;
                            objAttributeToShift.Base += intKarma;
                            intPointsSpent += intKarma;
                        }
                    }
                    blnDoSwitch = false;
                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                    {
                        if (objGroup.Base > 0)
                        {
                            blnDoSwitch = false;
                            break;
                        }
                        if (objGroup.Karma > 0)
                            blnDoSwitch = true;
                    }
                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.SkillsSection.SkillGroupPointsMaximum)
                        {
                            SkillGroup objGroupToShift = null;
                            foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                            {
                                if (objGroup.Karma > 0 && (objGroupToShift == null || objGroupToShift.Rating < objGroup.Rating))
                                {
                                    objGroupToShift = objGroup;
                                }
                            }
                            if (objGroupToShift == null)
                                break;
                            int intKarma = Math.Min(objGroupToShift.Karma, _objCharacter.SkillsSection.SkillGroupPointsMaximum - intPointsSpent);
                            objGroupToShift.Karma -= intKarma;
                            objGroupToShift.Base += intKarma;
                            intPointsSpent += intKarma;
                        }
                    }
                    blnDoSwitch = false;
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objSkill.Base > 0)
                        {
                            blnDoSwitch = false;
                            break;
                        }
                        if (objSkill.Karma > 0)
                            blnDoSwitch = true;
                    }
                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.SkillsSection.SkillGroupPointsMaximum)
                        {
                            Skill objSkillToShift = null;
                            foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                            {
                                if (objSkill.Karma > 0 && (objSkillToShift == null || objSkillToShift.Rating < objSkill.Rating))
                                {
                                    objSkillToShift = objSkill;
                                }
                            }
                            if (objSkillToShift == null)
                                break;
                            int intKarma = Math.Min(objSkillToShift.Karma, _objCharacter.SkillsSection.SkillGroupPointsMaximum - intPointsSpent);
                            objSkillToShift.Karma -= intKarma;
                            objSkillToShift.Base += intKarma;
                            intPointsSpent += intKarma;
                        }
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void AddFreeSkills(int intFreeLevels, Improvement.ImprovementType type, string strSkill1, string strSkill2, string strSkill3)
        {
            List<Improvement> lstOldFreeSkillImprovements
                = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, type);
            lstOldFreeSkillImprovements.RemoveAll(x => x.ImproveSource != Improvement.ImprovementSource.Heritage);
            if (intFreeLevels != 0)
            {
                bool blnCommit = false;
                if (!string.IsNullOrEmpty(strSkill1))
                {
                    Improvement objOldSkillImprovement = lstOldFreeSkillImprovements.Find(x => x.ImprovedName == strSkill1 && x.Value == intFreeLevels);
                    if (objOldSkillImprovement != null)
                        lstOldFreeSkillImprovements.Remove(objOldSkillImprovement);
                    else
                    {
                        blnCommit = true;
                        AddExoticSkillIfNecessary(strSkill1);
                        ImprovementManager.CreateImprovement(_objCharacter, strSkill1, Improvement.ImprovementSource.Heritage, string.Empty,
                            type, string.Empty, intFreeLevels);
                    }
                }

                if (!string.IsNullOrEmpty(strSkill2))
                {
                    Improvement objOldSkillImprovement = lstOldFreeSkillImprovements.Find(x => x.ImprovedName == strSkill2 && x.Value == intFreeLevels);
                    if (objOldSkillImprovement != null)
                        lstOldFreeSkillImprovements.Remove(objOldSkillImprovement);
                    else
                    {
                        blnCommit = true;
                        AddExoticSkillIfNecessary(strSkill2);
                        ImprovementManager.CreateImprovement(_objCharacter, strSkill2, Improvement.ImprovementSource.Heritage, string.Empty,
                            type, string.Empty, intFreeLevels);
                    }
                }

                if (!string.IsNullOrEmpty(strSkill3))
                {
                    Improvement objOldSkillImprovement = lstOldFreeSkillImprovements.Find(x => x.ImprovedName == strSkill3 && x.Value == intFreeLevels);
                    if (objOldSkillImprovement != null)
                        lstOldFreeSkillImprovements.Remove(objOldSkillImprovement);
                    else
                    {
                        blnCommit = true;
                        AddExoticSkillIfNecessary(strSkill3);
                        ImprovementManager.CreateImprovement(_objCharacter, strSkill3, Improvement.ImprovementSource.Heritage, string.Empty,
                            type, string.Empty, intFreeLevels);
                    }
                }

                if (lstOldFreeSkillImprovements.Count > 0)
                    ImprovementManager.RemoveImprovements(_objCharacter, lstOldFreeSkillImprovements);
                if (blnCommit)
                    ImprovementManager.Commit(_objCharacter);

                void AddExoticSkillIfNecessary(string strDictionaryKey)
                {
                    // Add exotic skills if we are increasing their base level
                    if (!ExoticSkill.IsExoticSkillName(strDictionaryKey) ||
                        _objCharacter.SkillsSection.GetActiveSkill(strDictionaryKey) != null)
                        return;
                    string strSkillName = strDictionaryKey;
                    string strSkillSpecific = string.Empty;
                    int intParenthesesIndex = strSkillName.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                    if (intParenthesesIndex > 0)
                    {
                        strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                        strSkillName = strSkillName.Substring(0, intParenthesesIndex);
                    }
                    _objCharacter.SkillsSection.AddExoticSkill(strSkillName, strSkillSpecific);
                }
            }
            else
                ImprovementManager.RemoveImprovements(_objCharacter, lstOldFreeSkillImprovements);
        }

        /// <summary>
        /// Manages adjusting priority selections to prevent doubling up in Priority mode.
        /// </summary>
        /// <param name="comboBox"></param>
        /// <param name="blnForce"></param>
        private void ManagePriorityItems(ComboBox comboBox, bool blnForce = false)
        {
            if (!blnForce && _objCharacter.EffectiveBuildMethod != CharacterBuildMethod.Priority)
                return;
            List<string> lstCurrentPriorities = new List<string>(_lstPriorities);
            string strHeritageSelected = cboHeritage.SelectedValue.ToString();
            string strAttributesSelected = cboAttributes.SelectedValue.ToString();
            string strTalentSelected = cboTalent.SelectedValue.ToString();
            string strSkillsSelected = cboSkills.SelectedValue.ToString();
            string strResourcesSelected = cboResources.SelectedValue.ToString();

            // Discover which priority rating is not currently assigned
            lstCurrentPriorities.Remove(strHeritageSelected);
            lstCurrentPriorities.Remove(strAttributesSelected);
            lstCurrentPriorities.Remove(strTalentSelected);
            lstCurrentPriorities.Remove(strSkillsSelected);
            lstCurrentPriorities.Remove(strResourcesSelected);
            if (lstCurrentPriorities.Count == 0)
                return;
            string strComboBoxSelected = comboBox.SelectedValue.ToString();

            string strMissing = lstCurrentPriorities[0];

            // Find the combo with the same value as this one and change it to the missing value.
            //_blnInitializing = true;
            if (strHeritageSelected == strComboBoxSelected && comboBox.Name != cboHeritage.Name)
                cboHeritage.SelectedValue = strMissing;
            else if (strAttributesSelected == strComboBoxSelected && comboBox.Name != cboAttributes.Name)
                cboAttributes.SelectedValue = strMissing;
            else if (strTalentSelected == strComboBoxSelected && comboBox.Name != cboTalent.Name)
                cboTalent.SelectedValue = strMissing;
            else if (strSkillsSelected == strComboBoxSelected && comboBox.Name != cboSkills.Name)
                cboSkills.SelectedValue = strMissing;
            else if (strResourcesSelected == strComboBoxSelected && comboBox.Name != cboResources.Name)
                cboResources.SelectedValue = strMissing;

            if (lstCurrentPriorities.Count <= 1)
                return;
            do
            {
                lstCurrentPriorities.Clear();
                lstCurrentPriorities.AddRange(_lstPriorities);
                strHeritageSelected = cboHeritage.SelectedValue.ToString();
                strAttributesSelected = cboAttributes.SelectedValue.ToString();
                strTalentSelected = cboTalent.SelectedValue.ToString();
                strSkillsSelected = cboSkills.SelectedValue.ToString();
                strResourcesSelected = cboResources.SelectedValue.ToString();

                // Discover which priority rating is not currently assigned
                lstCurrentPriorities.Remove(strHeritageSelected);
                lstCurrentPriorities.Remove(strAttributesSelected);
                lstCurrentPriorities.Remove(strTalentSelected);
                lstCurrentPriorities.Remove(strSkillsSelected);
                lstCurrentPriorities.Remove(strResourcesSelected);
                if (lstCurrentPriorities.Count == 0) // Just in case
                    return;

                strMissing = lstCurrentPriorities[0];

                // Find the combo with the same value as this one and change it to the missing value.
                //_blnInitializing = true;
                if (strHeritageSelected == strComboBoxSelected && comboBox.Name != cboHeritage.Name)
                    cboHeritage.SelectedValue = strMissing;
                else if (strAttributesSelected == strComboBoxSelected && comboBox.Name != cboAttributes.Name)
                    cboAttributes.SelectedValue = strMissing;
                else if (strTalentSelected == strComboBoxSelected && comboBox.Name != cboTalent.Name)
                    cboTalent.SelectedValue = strMissing;
                else if (strSkillsSelected == strComboBoxSelected && comboBox.Name != cboSkills.Name)
                    cboSkills.SelectedValue = strMissing;
                else if (strResourcesSelected == strComboBoxSelected && comboBox.Name != cboResources.Name)
                    cboResources.SelectedValue = strMissing;
            } while (lstCurrentPriorities.Count > 1);
        }

        private int SumToTen(bool blnDoUIUpdate = true)
        {
            int value = _dicSumtoTenValues[cboHeritage.SelectedValue.ToString()];
            value += _dicSumtoTenValues[cboAttributes.SelectedValue.ToString()];
            value += _dicSumtoTenValues[cboTalent.SelectedValue.ToString()];
            value += _dicSumtoTenValues[cboSkills.SelectedValue.ToString()];
            value += _dicSumtoTenValues[cboResources.SelectedValue.ToString()];

            if (blnDoUIUpdate)
                lblSumtoTen.Text = value.ToString(GlobalSettings.CultureInfo) + '/' + _objCharacter.Settings.SumtoTen.ToString(GlobalSettings.CultureInfo);

            return value;
        }

        private async Task RefreshSelectedMetatype()
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
            string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

            XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
            XPathNavigator objXmlMetavariant = string.IsNullOrEmpty(strSelectedMetavariant) || strSelectedMetavariant == "None" ? null : objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + ']');
            XPathNavigator objXmlMetatypePriorityNode = null;
            XPathNavigator objXmlMetavariantPriorityNode = null;
            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
            {
                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                {
                    objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                    objXmlMetavariantPriorityNode = objXmlMetavariant != null ? objXmlMetatypePriorityNode.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + ']') : null;
                    break;
                }
            }

            string strAttributeFormat = "{0}/{1}" + strSpace + "({2})";
            if (objXmlMetavariant != null)
            {
                if (objXmlMetavariantPriorityNode == null)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("String_NotSupported"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmdOK.Enabled = false;
                }
                else
                {
                    cmdOK.Enabled = true;
                }

                lblBOD.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblAGI.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("agiaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblREA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("reaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblSTR.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("straug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblCHA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("chaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblINT.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("intaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblLOG.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("logaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblWIL.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));

                lblMetavariantKarma.Text = objXmlMetavariantPriorityNode.SelectSingleNodeAndCacheExpression("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetavariant.SelectSingleNodeAndCacheExpression("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetavariant.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetavariant.SelectSingleNodeAndCacheExpression("page")?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                        lblSource.Text = objSource.ToString();
                        lblSource.SetToolTip(objSource.LanguageBookTooltip);
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                }
                else
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                    lblSource.Text = strUnknown;
                    lblSource.SetToolTip(strUnknown);
                }

                // Set the special attributes label.
                if (!int.TryParse(objXmlMetavariantPriorityNode?.SelectSingleNodeAndCacheExpression("value")?.Value, NumberStyles.Any,
                    GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                    intSpecialAttribPoints = 0;

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + (cboTalents.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + ']');
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNodeAndCacheExpression("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo);

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetavariant.SelectAndCacheExpression("qualities/*/quality"))
                {
                    string strQuality;
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        strQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath() + "]/translate")?.Value ?? objXmlQuality.Value;

                        string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect) + ')';
                    }
                    else
                    {
                        strQuality = objXmlQuality.Value;
                        string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + strSelect + ')';
                    }
                    if (dicQualities.TryGetValue(strQuality, out int intExistingRating))
                        dicQualities[strQuality] = intExistingRating + 1;
                    else
                        dicQualities.Add(strQuality, 1);
                }

                if (dicQualities.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdQualities))
                    {
                        foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                        {
                            sbdQualities.Append(objLoopQuality.Key);
                            if (objLoopQuality.Value > 1)
                            {
                                sbdQualities.Append(strSpace)
                                            .Append(objLoopQuality.Value.ToString(GlobalSettings.CultureInfo));
                            }

                            sbdQualities.Append(',').Append(strSpace);
                        }

                        sbdQualities.Length -= 2;
                        lblMetavariantQualities.Text = sbdQualities.ToString();
                    }
                }
                else
                {
                    lblMetavariantQualities.Text = await LanguageManager.GetStringAsync("String_None");
                }
            }
            else if (objXmlMetatype != null)
            {
                cmdOK.Enabled = true;
                lblBOD.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("bodaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblAGI.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("agimax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("agiaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblREA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("reamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("reaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblSTR.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("strmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("straug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblCHA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("chamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("chaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblINT.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("intmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("intaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblLOG.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("logmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("logaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblWIL.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNodeAndCacheExpression("wilaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metatype's Qualities.
                foreach (XPathNavigator xmlQuality in objXmlMetatype.SelectAndCacheExpression("qualities/*/quality"))
                {
                    string strQuality;
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = " + xmlQuality.Value.CleanXPath() + ']');
                        strQuality = objQuality.SelectSingleNodeAndCacheExpression("translate")?.Value ?? xmlQuality.Value;

                        string strSelect = xmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect) + ')';
                    }
                    else
                    {
                        strQuality = xmlQuality.Value;
                        string strSelect = xmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + strSelect + ')';
                    }
                    if (dicQualities.TryGetValue(strQuality, out int intExistingRating))
                        dicQualities[strQuality] = intExistingRating + 1;
                    else
                        dicQualities.Add(strQuality, 1);
                }

                if (dicQualities.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdQualities))
                    {
                        foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                        {
                            sbdQualities.Append(objLoopQuality.Key);
                            if (objLoopQuality.Value > 1)
                            {
                                sbdQualities.Append(strSpace)
                                            .Append(objLoopQuality.Value.ToString(GlobalSettings.CultureInfo));
                            }

                            sbdQualities.Append(',').Append(strSpace);
                        }

                        sbdQualities.Length -= 2;
                        lblMetavariantQualities.Text = sbdQualities.ToString();
                    }
                }
                else
                {
                    lblMetavariantQualities.Text = await LanguageManager.GetStringAsync("String_None");
                }

                lblMetavariantKarma.Text = objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetatype.SelectSingleNodeAndCacheExpression("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetatype.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("page")?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                        lblSource.Text = objSource.ToString();
                        lblSource.SetToolTip(objSource.LanguageBookTooltip);
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                }
                else
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                    lblSource.Text = strUnknown;
                    lblSource.SetToolTip(strUnknown);
                }

                // Set the special attributes label.
                if (!int.TryParse(objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression("value")?.Value, NumberStyles.Any,
                    GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                    intSpecialAttribPoints = 0;

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + (cboTalents.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + ']');
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNodeAndCacheExpression("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo);
            }
            else
            {
                lblBOD.Text = string.Empty;
                lblAGI.Text = string.Empty;
                lblREA.Text = string.Empty;
                lblSTR.Text = string.Empty;
                lblCHA.Text = string.Empty;
                lblINT.Text = string.Empty;
                lblLOG.Text = string.Empty;
                lblWIL.Text = string.Empty;

                int intSpecialAttribPoints = 0;
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + (cboTalents.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + ']');
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNodeAndCacheExpression("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo);

                lblMetavariantQualities.Text = string.Empty;
                lblMetavariantKarma.Text = string.Empty;
                lblSource.Text = string.Empty;
                cmdOK.Enabled = false;
            }

            lblBODLabel.Visible = !string.IsNullOrEmpty(lblBOD.Text);
            lblAGILabel.Visible = !string.IsNullOrEmpty(lblAGI.Text);
            lblREALabel.Visible = !string.IsNullOrEmpty(lblREA.Text);
            lblSTRLabel.Visible = !string.IsNullOrEmpty(lblSTR.Text);
            lblCHALabel.Visible = !string.IsNullOrEmpty(lblCHA.Text);
            lblINTLabel.Visible = !string.IsNullOrEmpty(lblINT.Text);
            lblLOGLabel.Visible = !string.IsNullOrEmpty(lblLOG.Text);
            lblWILLabel.Visible = !string.IsNullOrEmpty(lblWIL.Text);
            lblSpecialAttributesLabel.Visible = !string.IsNullOrEmpty(lblSpecialAttributes.Text);
            lblMetavariantQualitiesLabel.Visible = !string.IsNullOrEmpty(lblMetavariantQualities.Text);
            lblMetavariantKarmaLabel.Visible = !string.IsNullOrEmpty(lblMetavariantKarma.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private void PopulateTalents()
        {
            // Load the Priority information.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTalent))
            {
                // Populate the Priority Category list.
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Talent\" and value = "
                    + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath()
                    + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1
                        || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                    {
                        foreach (XPathNavigator objXmlPriorityTalent in xmlBaseTalentPriority.SelectAndCacheExpression(
                                     "talents/talent"))
                        {
                            XPathNavigator xmlQualitiesNode
                                = objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("qualities");
                            if (xmlQualitiesNode != null)
                            {
                                bool blnFoundUnavailableQuality = false;

                                foreach (XPathNavigator xmlQuality in xmlQualitiesNode.SelectAndCacheExpression(
                                             "quality"))
                                {
                                    if (_xmlBaseQualityDataNode.SelectSingleNode(
                                            "qualities/quality[" + _objCharacter.Settings.BookXPath() + " and name = "
                                            + xmlQuality.Value.CleanXPath() + ']') == null)
                                    {
                                        blnFoundUnavailableQuality = true;
                                        break;
                                    }
                                }

                                if (blnFoundUnavailableQuality)
                                    continue;
                            }

                            XPathNavigator xmlForbiddenNode
                                = objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("forbidden");
                            if (xmlForbiddenNode != null)
                            {
                                bool blnRequirementForbidden = false;

                                // Loop through the oneof requirements.
                                XPathNodeIterator objXmlForbiddenList
                                    = xmlForbiddenNode.SelectAndCacheExpression("oneof");
                                foreach (XPathNavigator objXmlOneOf in objXmlForbiddenList)
                                {
                                    XPathNodeIterator objXmlOneOfList
                                        = objXmlOneOf.SelectChildren(XPathNodeType.Element);

                                    foreach (XPathNavigator objXmlForbidden in objXmlOneOfList)
                                    {
                                        switch (objXmlForbidden.Name)
                                        {
                                            case "metatype":
                                            {
                                                // Check the Metatype restriction.
                                                if (objXmlForbidden.Value == lstMetatypes.SelectedValue?.ToString())
                                                {
                                                    blnRequirementForbidden = true;
                                                    goto EndForbiddenLoop;
                                                }

                                                break;
                                            }
                                            // Check the Metavariant restriction.
                                            case "metatypecategory":
                                            {
                                                // Check the Metatype Category restriction.
                                                if (objXmlForbidden.Value == cboCategory.SelectedValue?.ToString())
                                                {
                                                    blnRequirementForbidden = true;
                                                    goto EndForbiddenLoop;
                                                }

                                                break;
                                            }
                                            case "metavariant" when objXmlForbidden.Value
                                                                    == cboMetavariant.SelectedValue?.ToString():
                                                blnRequirementForbidden = true;
                                                goto EndForbiddenLoop;
                                        }
                                    }
                                }

                                EndForbiddenLoop:
                                if (blnRequirementForbidden)
                                    continue;
                            }

                            XPathNavigator xmlRequiredNode
                                = objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("required");
                            if (xmlRequiredNode != null)
                            {
                                bool blnRequirementMet = false;

                                // Loop through the oneof requirements.
                                XPathNodeIterator objXmlForbiddenList
                                    = xmlRequiredNode.SelectAndCacheExpression("oneof");
                                foreach (XPathNavigator objXmlOneOf in objXmlForbiddenList)
                                {
                                    XPathNodeIterator objXmlOneOfList
                                        = objXmlOneOf.SelectChildren(XPathNodeType.Element);

                                    foreach (XPathNavigator objXmlRequired in objXmlOneOfList)
                                    {
                                        switch (objXmlRequired.Name)
                                        {
                                            case "metatype":
                                            {
                                                // Check the Metatype restriction.
                                                if (objXmlRequired.Value == lstMetatypes.SelectedValue?.ToString())
                                                {
                                                    blnRequirementMet = true;
                                                    goto EndRequiredLoop;
                                                }

                                                break;
                                            }
                                            // Check the Metavariant restriction.
                                            case "metatypecategory":
                                            {
                                                // Check the Metatype Category restriction.
                                                if (objXmlRequired.Value == cboCategory.SelectedValue?.ToString())
                                                {
                                                    blnRequirementMet = true;
                                                    goto EndRequiredLoop;
                                                }

                                                break;
                                            }
                                            case "metavariant" when objXmlRequired.Value
                                                                    == cboMetavariant.SelectedValue?.ToString():
                                                blnRequirementMet = true;
                                                goto EndRequiredLoop;
                                        }
                                    }
                                }

                                EndRequiredLoop:
                                if (!blnRequirementMet)
                                    continue;
                            }

                            lstTalent.Add(new ListItem(
                                              objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("value")?.Value,
                                              objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("translate")
                                                                  ?.Value ??
                                              objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("name")?.Value ??
                                              LanguageManager.GetString("String_Unknown")));
                        }

                        break;
                    }
                }

                lstTalent.Sort(CompareListItems.CompareNames);
                int intOldSelectedIndex = cboTalents.SelectedIndex;
                int intOldDataSourceSize = cboTalents.Items.Count;
                cboTalents.BeginUpdate();
                cboTalents.PopulateWithListItems(lstTalent);
                if (intOldDataSourceSize == cboTalents.Items.Count)
                {
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    cboTalents.SelectedIndex = intOldSelectedIndex;
                    _blnLoading = blnOldLoading;
                }

                cboTalents.Enabled = cboTalents.Items.Count > 1;
                cboTalents.EndUpdate();
            }
        }

        private void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();

            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

                XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                XPathNavigator objXmlMetatypeBP = null;
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                    {
                        objXmlMetatypeBP = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                        break;
                    }
                }
                
                if (objXmlMetatype != null && objXmlMetatypeBP != null)
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstMetavariants))
                    {
                        lstMetavariants.Add(new ListItem(Guid.Empty, LanguageManager.GetString("String_None")));
                        // Retrieve the list of Metavariants for the selected Metatype.
                        foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select(
                                     "metavariants/metavariant[" + _objCharacter.Settings.BookXPath() + ']'))
                        {
                            string strName = objXmlMetavariant.SelectSingleNodeAndCacheExpression("name")?.Value
                                             ?? LanguageManager.GetString("String_Unknown");
                            if (objXmlMetatypeBP.SelectSingleNode(
                                    "metavariants/metavariant[name = " + strName.CleanXPath() + ']') != null)
                            {
                                lstMetavariants.Add(new ListItem(
                                                        strName,
                                                        objXmlMetavariant
                                                            .SelectSingleNodeAndCacheExpression("translate")?.Value
                                                        ?? strName));
                            }
                        }

                        string strOldSelectedValue
                            = cboMetavariant.SelectedValue?.ToString() ?? _objCharacter.Metavariant;
                        bool blnOldLoading = _blnLoading;
                        _blnLoading = true;
                        cboMetavariant.BeginUpdate();
                        cboMetavariant.PopulateWithListItems(lstMetavariants);
                        cboMetavariant.Enabled = lstMetavariants.Count > 1;
                        _blnLoading = blnOldLoading;
                        if (!string.IsNullOrEmpty(strOldSelectedValue))
                            cboMetavariant.SelectedValue = strOldSelectedValue;
                        if (cboMetavariant.SelectedIndex == -1)
                            cboMetavariant.SelectedIndex = 0;
                        cboMetavariant.EndUpdate();

                        // If the Metatype has Force enabled, show the Force NUD.
                        string strEssMax = objXmlMetatype.SelectSingleNodeAndCacheExpression("essmax")?.Value
                                           ?? string.Empty;
                        int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                        if (objXmlMetatype.SelectSingleNodeAndCacheExpression("forcecreature") != null || intPos != -1)
                        {
                            if (intPos != -1)
                            {
                                if (intPos > 0)
                                {
                                    --intPos;
                                    lblForceLabel.Text = strEssMax.Substring(intPos, 3)
                                                                  .Replace(
                                                                      "D6", LanguageManager.GetString("String_D6"));
                                    nudForce.Maximum
                                        = Convert.ToInt32(strEssMax[intPos], GlobalSettings.InvariantCultureInfo) * 6;
                                }
                                else
                                {
                                    lblForceLabel.Text = 1.ToString(GlobalSettings.CultureInfo)
                                                         + LanguageManager.GetString("String_D6");
                                    nudForce.Maximum = 6;
                                }
                            }
                            else
                            {
                                lblForceLabel.Text = LanguageManager.GetString(
                                    objXmlMetatype.SelectSingleNodeAndCacheExpression("forceislevels") != null
                                        ? "String_Level"
                                        : "String_Force");
                                nudForce.Maximum = 100;
                            }

                            lblForceLabel.Visible = true;
                            nudForce.Visible = true;
                        }
                        else
                        {
                            lblForceLabel.Visible = false;
                            nudForce.Visible = false;
                        }
                    }
                }
                else
                {
                    cboMetavariant.BeginUpdate();
                    cboMetavariant.PopulateWithListItems(new ListItem("None", LanguageManager.GetString("String_None")).Yield());
                    cboMetavariant.Enabled = false;
                    cboMetavariant.EndUpdate();

                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
            }
            else
            {
                // Clear the Metavariant list if nothing is currently selected.
                cboMetavariant.BeginUpdate();
                cboMetavariant.PopulateWithListItems(new ListItem("None", LanguageManager.GetString("String_None")).Yield());
                cboMetavariant.Enabled = false;
                cboMetavariant.EndUpdate();

                lblForceLabel.Visible = false;
                nudForce.Visible = false;
            }
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        private void PopulateMetatypes()
        {
            string strSelectedCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedCategory))
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatype))
                {
                    XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Heritage\" and value = "
                        + (cboHeritage.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                    {
                        if (xmlBaseMetatypePriorityList.Count == 1
                            || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                        {
                            foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                         "metatypes/metatype[(" + _objCharacter.Settings.BookXPath()
                                                                + ") and category = " + strSelectedCategory.CleanXPath()
                                                                + ']'))
                            {
                                string strName = objXmlMetatype.SelectSingleNodeAndCacheExpression("name")?.Value;
                                if (!string.IsNullOrEmpty(strName)
                                    && xmlBaseMetatypePriority.SelectSingleNode(
                                        "metatypes/metatype[name = " + strName.CleanXPath() + ']') != null)
                                {
                                    lstMetatype.Add(new ListItem(
                                                        strName,
                                                        objXmlMetatype.SelectSingleNodeAndCacheExpression("translate")
                                                                      ?.Value ?? strName));
                                }
                            }

                            break;
                        }
                    }

                    lstMetatype.Sort(CompareListItems.CompareNames);
                    string strOldSelectedValue = lstMetatypes.SelectedValue?.ToString() ?? _objCharacter.Metatype;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    lstMetatypes.BeginUpdate();
                    lstMetatypes.PopulateWithListItems(lstMetatype);
                    _blnLoading = blnOldLoading;
                    if (!string.IsNullOrEmpty(strOldSelectedValue))
                        lstMetatypes.SelectedValue = strOldSelectedValue;
                    if (lstMetatypes.SelectedIndex == -1 && lstMetatype.Count > 0)
                        lstMetatypes.SelectedIndex = 0;
                    lstMetatypes.EndUpdate();
                }

                if (strSelectedCategory.EndsWith("Spirits", StringComparison.Ordinal))
                {
                    if (!chkPossessionBased.Visible && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                    {
                        chkPossessionBased.Checked = true;
                        cboPossessionMethod.SelectedValue = _strCurrentPossessionMethod;
                    }
                    chkPossessionBased.Visible = true;
                    cboPossessionMethod.Visible = true;
                }
                else
                {
                    chkPossessionBased.Visible = false;
                    chkPossessionBased.Checked = false;
                    cboPossessionMethod.Visible = false;
                }
            }
            else
            {
                lstMetatypes.BeginUpdate();
                lstMetatypes.DataSource = null;
                lstMetatypes.EndUpdate();

                chkPossessionBased.Visible = false;
                chkPossessionBased.Checked = false;
                cboPossessionMethod.Visible = false;
            }
        }

        private void LoadMetatypes()
        {
            // Create a list of any Categories that should not be in the list.
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setRemoveCategories))
            {
                foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.SelectAndCacheExpression(
                             "categories/category"))
                {
                    XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Heritage\" and value = "
                        + (cboHeritage.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    bool blnRemoveCategory = true;
                    foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                    {
                        if (xmlBaseMetatypePriorityList.Count == 1
                            || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable") != null)
                        {
                            foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                         "metatypes/metatype[category = " + objXmlCategory.Value.CleanXPath() + " and ("
                                         + _objCharacter.Settings.BookXPath() + ")]"))
                            {
                                if (xmlBaseMetatypePriority.SelectSingleNode(
                                        "metatypes/metatype[name = "
                                        + (objXmlMetatype.SelectSingleNodeAndCacheExpression("name")?.Value
                                           ?? string.Empty).CleanXPath() + ']') != null)
                                {
                                    blnRemoveCategory = false;
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    // Remove metatypes not covered by heritage
                    if (blnRemoveCategory)
                        setRemoveCategories.Add(objXmlCategory.Value);
                }

                // Populate the Metatype Category list.
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
                {
                    foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.SelectAndCacheExpression(
                                 "categories/category"))
                    {
                        string strInnerText = objXmlCategory.Value;

                        // Make sure the Category isn't in the exclusion list.
                        if (!setRemoveCategories.Contains(strInnerText) &&
                            // Also make sure it is not already in the Category list.
                            lstCategory.All(objItem => objItem.Value.ToString() != strInnerText))
                        {
                            lstCategory.Add(new ListItem(strInnerText,
                                                         objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")
                                                                       ?.Value ?? strInnerText));
                        }
                    }

                    lstCategory.Sort(CompareListItems.CompareNames);
                    string strOldSelected = cboCategory.SelectedValue?.ToString() ?? _objCharacter.MetatypeCategory;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    cboCategory.BeginUpdate();
                    cboCategory.PopulateWithListItems(lstCategory);
                    _blnLoading = blnOldLoading;
                    if (!string.IsNullOrEmpty(strOldSelected))
                        cboCategory.SelectedValue = strOldSelected;
                    if (cboCategory.SelectedIndex == -1 && lstCategory.Count > 0)
                        cboCategory.SelectedIndex = 0;
                    cboCategory.EndUpdate();
                }
            }
        }

        private XPathNodeIterator GetMatrixSkillList()
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression("skills/skill[skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
        }

        private XPathNodeIterator GetMagicalSkillList()
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression("skills/skill[category = \"Magical Active\" or category = \"Pseudo-Magical Active\"]");
        }

        private XPathNodeIterator GetResonanceSkillList()
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression("skills/skill[category = \"Resonance Active\" or skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
        }

        private XPathNodeIterator GetActiveSkillList(string strXPathFilter = "")
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression(!string.IsNullOrEmpty(strXPathFilter)
                ? "skills/skill[" + strXPathFilter + ']'
                : "skills/skill");
        }

        private XPathNodeIterator BuildSkillCategoryList(XPathNodeIterator objSkillList)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdGroups))
            {
                sbdGroups.Append("skillgroups/name");
                if (objSkillList.Count > 0)
                {
                    sbdGroups.Append('[');
                    foreach (XPathNavigator xmlSkillGroup in objSkillList)
                    {
                        sbdGroups.Append(". = ").Append(xmlSkillGroup.Value.CleanXPath()).Append(" or ");
                    }

                    sbdGroups.Length -= 4;
                    sbdGroups.Append(']');
                }

                return _xmlBaseSkillDataNode.Select(sbdGroups.ToString());
            }
        }

        private XPathNodeIterator BuildSkillList(XPathNodeIterator objSkillList)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdGroups))
            {
                sbdGroups.Append("skills/skill");
                if (objSkillList.Count > 0)
                {
                    sbdGroups.Append('[');
                    foreach (XPathNavigator xmlSkillGroup in objSkillList)
                    {
                        sbdGroups.Append("name = ").Append(xmlSkillGroup.Value.CleanXPath()).Append(" or ");
                    }

                    sbdGroups.Length -= 4;
                    sbdGroups.Append(']');
                }

                return _xmlBaseSkillDataNode.Select(sbdGroups.ToString());
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Custom Methods
    }
}
