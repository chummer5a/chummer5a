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
using System.Threading;
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
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                    {
                        await lblSumtoTen.DoThreadSafeAsync(x => x.Visible = true);
                    }

                    // Populate the Priority Category list.
                    XPathNavigator xmlBasePrioritiesNode
                        = await _xmlBasePriorityDataNode.SelectSingleNodeAndCacheExpressionAsync("priorities");
                    if (xmlBasePrioritiesNode != null)
                    {
                        foreach (XPathNavigator objXmlPriorityCategory in await _xmlBasePriorityDataNode
                                     .SelectAndCacheExpressionAsync("categories/category"))
                        {
                            XPathNodeIterator objItems = xmlBasePrioritiesNode.Select(
                                "priority[category = " + objXmlPriorityCategory.Value.CleanXPath()
                                                       + " and prioritytable = "
                                                       + _objCharacter.Settings.PriorityTable.CleanXPath() + ']');

                            if (objItems.Count == 0)
                            {
                                objItems = xmlBasePrioritiesNode.Select(
                                    "priority[category = " + objXmlPriorityCategory.Value.CleanXPath()
                                                           + " and not(prioritytable)]");
                            }

                            if (objItems.Count > 0)
                            {
                                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                               out List<ListItem> lstItems))
                                {
                                    foreach (XPathNavigator objXmlPriority in objItems)
                                    {
                                        string strValue
                                            = (await objXmlPriority.SelectSingleNodeAndCacheExpressionAsync("value"))
                                            ?.Value;
                                        if (!string.IsNullOrEmpty(strValue) && _lstPriorities.Contains(strValue))
                                        {
                                            lstItems.Add(new ListItem(
                                                             strValue,
                                                             (await objXmlPriority
                                                                 .SelectSingleNodeAndCacheExpressionAsync(
                                                                     "translate"))
                                                             ?.Value ??
                                                             (await objXmlPriority
                                                                 .SelectSingleNodeAndCacheExpressionAsync(
                                                                     "name"))?.Value ??
                                                             await LanguageManager.GetStringAsync("String_Unknown")));
                                        }
                                    }

                                    lstItems.Sort(CompareListItems.CompareNames);
                                    switch (objXmlPriorityCategory.Value)
                                    {
                                        case "Heritage":
                                            await cboHeritage.PopulateWithListItemsAsync(lstItems);
                                            break;

                                        case "Talent":
                                            await cboTalent.PopulateWithListItemsAsync(lstItems);
                                            break;

                                        case "Attributes":
                                            await cboAttributes.PopulateWithListItemsAsync(lstItems);
                                            break;

                                        case "Skills":
                                            await cboSkills.PopulateWithListItemsAsync(lstItems);
                                            break;

                                        case "Resources":
                                            await cboResources.PopulateWithListItemsAsync(lstItems);
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
                        await cboAttributes.DoThreadSafeAsync(x => x.SelectedIndex
                                                                  = x.FindString(_objCharacter.AttributesPriority[0]
                                                                      .ToString(GlobalSettings
                                                                          .InvariantCultureInfo)));
                        //Heritage (Metatype)
                        await cboHeritage.DoThreadSafeAsync(x => x.SelectedIndex
                                                                = x.FindString(_objCharacter.MetatypePriority[0]
                                                                                   .ToString(GlobalSettings
                                                                                       .InvariantCultureInfo)));
                        //Resources
                        await cboResources.DoThreadSafeAsync(x => x.SelectedIndex
                                                                 = x.FindString(_objCharacter.ResourcesPriority[0]
                                                                     .ToString(GlobalSettings
                                                                                   .InvariantCultureInfo)));
                        //Skills
                        await cboSkills.DoThreadSafeAsync(x => x.SelectedIndex
                                                              = x.FindString(_objCharacter.SkillsPriority[0]
                                                                                 .ToString(GlobalSettings
                                                                                     .InvariantCultureInfo)));
                        //Magical/Resonance Talent
                        await cboTalent.DoThreadSafeAsync(x => x.SelectedIndex
                                                              = x.FindString(_objCharacter.SpecialPriority[0]
                                                                                 .ToString(
                                                                                     GlobalSettings
                                                                                         .InvariantCultureInfo)));

                        await LoadMetatypes();
                        await PopulateMetatypes();
                        await PopulateMetavariants();
                        await PopulateTalents();
                        await RefreshSelectedMetatype();

                        //Magical/Resonance Type
                        await cboTalents.DoThreadSafeAsync(x =>
                        {
                            x.SelectedValue = _objCharacter.TalentPriority;
                            if (x.SelectedIndex == -1 && x.Items.Count > 1)
                                x.SelectedIndex = 0;
                        });
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

                            await cboSkill1.DoThreadSafeAsync(x => x.SelectedValue = strSkill);
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

                            await cboSkill2.DoThreadSafeAsync(x => x.SelectedValue = strSkill);
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

                            await cboSkill3.DoThreadSafeAsync(x => x.SelectedValue = strSkill);
                        }

                        await ProcessTalentsIndexChanged();

                        switch (_objCharacter.EffectiveBuildMethod)
                        {
                            case CharacterBuildMethod.Priority:
                                await ManagePriorityItems(cboHeritage);
                                await ManagePriorityItems(cboAttributes);
                                await ManagePriorityItems(cboTalent);
                                await ManagePriorityItems(cboSkills);
                                await ManagePriorityItems(cboResources);
                                break;

                            case CharacterBuildMethod.SumtoTen:
                                await SumToTen();
                                break;
                        }
                    }
                    else
                    {
                        await cboHeritage.DoThreadSafeAsync(x => x.SelectedIndex = 0);
                        await cboTalent.DoThreadSafeAsync(x => x.SelectedIndex = 0);
                        await cboAttributes.DoThreadSafeAsync(x => x.SelectedIndex = 0);
                        await cboSkills.DoThreadSafeAsync(x => x.SelectedIndex = 0);
                        await cboResources.DoThreadSafeAsync(x => x.SelectedIndex = 0);
                        await ManagePriorityItems(cboHeritage, true);
                        await ManagePriorityItems(cboAttributes, true);
                        await ManagePriorityItems(cboTalent, true);
                        await ManagePriorityItems(cboSkills, true);
                        await ManagePriorityItems(cboResources, true);
                        await LoadMetatypes();
                        await PopulateMetatypes();
                        await PopulateMetavariants();
                        await PopulateTalents();
                        await RefreshSelectedMetatype();
                        if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                            await SumToTen();
                    }

                    // Set up possession boxes
                    // Add Possession and Inhabitation to the list of Critter Tradition variations.
                    await chkPossessionBased.SetToolTipAsync(
                        await LanguageManager.GetStringAsync("Tip_Metatype_PossessionTradition"));

                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstMethods))
                    {
                        lstMethods.Add(new ListItem("Possession",
                                                    _xmlCritterPowerDocumentPowersNode
                                                        ?.SelectSingleNode("power[name = \"Possession\"]/translate")
                                                        ?.InnerText
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
                        
                        await cboPossessionMethod.PopulateWithListItemsAsync(lstMethods);
                    }

                    _blnLoading = false;
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        #endregion Form Events

        #region Control Events

        private async void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                    {
                        await SumToTen();
                    }

                    await PopulateMetavariants();
                    await RefreshSelectedMetatype();
                    await PopulateTalents();
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
            {
                await MetatypeSelected();
            }
        }

        private async void cboTalents_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    await ProcessTalentsIndexChanged();
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async ValueTask ProcessTalentsIndexChanged(CancellationToken token = default)
        {
            await cboSkill1.DoThreadSafeAsync(x => x.Visible = false, token);
            await cboSkill2.DoThreadSafeAsync(x => x.Visible = false, token);
            await cboSkill3.DoThreadSafeAsync(x => x.Visible = false, token);
            await lblMetatypeSkillSelection.DoThreadSafeAsync(x => x.Visible = false, token);

            string strSelectedTalents = await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            if (!string.IsNullOrEmpty(strSelectedTalents))
            {
                XPathNavigator xmlTalentNode = null;
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath() + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || await xmlBaseTalentPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null)
                    {
                        xmlTalentNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + strSelectedTalents.CleanXPath() + ']');
                        break;
                    }
                }

                if (xmlTalentNode != null)
                {
                    string strSkillCount = (await xmlTalentNode.SelectSingleNodeAndCacheExpressionAsync("skillqty"))?.Value ?? (await xmlTalentNode.SelectSingleNodeAndCacheExpressionAsync("skillgroupqty"))?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSkillCount) && int.TryParse(strSkillCount, out int intSkillCount))
                    {
                        XPathNavigator xmlSkillTypeNode = await xmlTalentNode.SelectSingleNodeAndCacheExpressionAsync("skilltype") ?? await xmlTalentNode.SelectSingleNodeAndCacheExpressionAsync("skillgrouptype");
                        string strSkillType = xmlSkillTypeNode?.Value ?? string.Empty;
                        XPathNodeIterator objNodeList = await xmlTalentNode.SelectAndCacheExpressionAsync("skillgroupchoices/skillgroup");
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
                                xmlSkillsList = BuildSkillList(await xmlTalentNode.SelectAndCacheExpressionAsync("skillchoices/skill"));
                                break;

                            case "xpath":
                                xmlSkillsList = GetActiveSkillList(
                                    xmlSkillTypeNode != null
                                        ? (await xmlSkillTypeNode.SelectSingleNodeAndCacheExpressionAsync("@xpath"))
                                        ?.Value
                                        : null);
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
                                                                   (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync(
                                                                       "@translate"))?.Value ?? strInnerText));
                                    }
                                }
                                else
                                {
                                    foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                    {
                                        string strName = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                                         ?? await LanguageManager.GetStringAsync("String_Unknown");
                                        lstSkills.Add(
                                            new ListItem(
                                                strName,
                                                (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                ?? strName));
                                    }
                                }

                                lstSkills.Sort(CompareListItems.CompareNames);
                                bool blnOldLoading = _blnLoading;
                                int intOldSelectedIndex = await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token);
                                int intOldDataSourceSize = await cboSkill1.DoThreadSafeFuncAsync(x => x.Items.Count, token);
                                await cboSkill1.PopulateWithListItemsAsync(lstSkills, token);
                                await cboSkill1.DoThreadSafeAsync(x => x.Visible = true, token);
                                if (intOldDataSourceSize == cboSkill1.Items.Count)
                                {
                                    _blnLoading = true;
                                    await cboSkill1.DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex, token);
                                    _blnLoading = blnOldLoading;
                                }

                                if (intSkillCount > 1)
                                {
                                    intOldSelectedIndex = await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token);
                                    intOldDataSourceSize = await cboSkill2.DoThreadSafeFuncAsync(x => x.Items.Count, token);
                                    await cboSkill2.PopulateWithListItemsAsync(lstSkills, token);
                                    await cboSkill2.DoThreadSafeAsync(x => x.Visible = true, token);
                                    if (intOldDataSourceSize == cboSkill2.Items.Count)
                                    {
                                        _blnLoading = true;
                                        await cboSkill2.DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex, token);
                                        _blnLoading = blnOldLoading;
                                    }

                                    if (await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) == await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                        && !ExoticSkill.IsExoticSkillName(
                                            await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)))
                                    {
                                        if (await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) + 1 >= await cboSkill2.DoThreadSafeFuncAsync(x => x.Items.Count, token))
                                            await cboSkill2.DoThreadSafeAsync(x => x.SelectedIndex = 0, token);
                                        else
                                        {
                                            int intSkill1Index
                                                = await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token);
                                            await cboSkill2.DoThreadSafeAsync(x => x.SelectedIndex = intSkill1Index + 1, token);
                                        }
                                    }

                                    if (intSkillCount > 2)
                                    {
                                        intOldSelectedIndex = await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token);
                                        intOldDataSourceSize = await cboSkill3.DoThreadSafeFuncAsync(x => x.Items.Count, token);
                                        await cboSkill3.PopulateWithListItemsAsync(lstSkills, token);
                                        await cboSkill3.DoThreadSafeAsync(x => x.Visible = true, token);
                                        if (intOldDataSourceSize == cboSkill3.Items.Count)
                                        {
                                            _blnLoading = true;
                                            await cboSkill3.DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex, token);
                                            _blnLoading = blnOldLoading;
                                        }

                                        if ((await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) == await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) ||
                                             await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) == await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)) &&
                                            !ExoticSkill.IsExoticSkillName(await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)))
                                        {
                                            int intNewIndex = await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token);
                                            do
                                            {
                                                ++intNewIndex;
                                                if (intNewIndex >= await cboSkill3.DoThreadSafeFuncAsync(x => x.Items.Count, token))
                                                    intNewIndex = 0;
                                            } while ((intNewIndex == await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                      || intNewIndex == await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token))
                                                     && intNewIndex != await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) &&
                                                     !ExoticSkill.IsExoticSkillName(
                                                         ((ListItem) await cboSkill3.DoThreadSafeFuncAsync(x => x.Items[intNewIndex], token)).Value.ToString()));

                                            await cboSkill3.DoThreadSafeAsync(x => x.SelectedIndex = intNewIndex, token);
                                        }
                                    }
                                }

                                string strMetamagicSkillSelection = string.Format(
                                    GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_MetamagicSkillBase"),
                                    await LanguageManager.GetStringAsync("String_MetamagicSkills"));
                                // strSkillType can have the following values: magic, resonance, matrix, active, specific, grouped
                                // So the language file should contain each of those like String_MetamagicSkillType_magic
                                string strSkillVal = (await xmlTalentNode.SelectSingleNodeAndCacheExpressionAsync("skillval"))?.Value
                                                     ?? (await xmlTalentNode
                                                         .SelectSingleNodeAndCacheExpressionAsync("skillgroupval"))
                                                        ?.Value;
                                string strMetamagicSkillType = await LanguageManager.GetStringAsync(
                                    "String_MetamagicSkillType_" + strSkillType);
                                await lblMetatypeSkillSelection.DoThreadSafeAsync(x =>
                                {
                                    x.Text = string.Format(GlobalSettings.CultureInfo, strMetamagicSkillSelection,
                                                           strSkillCount, strMetamagicSkillType, strSkillVal);
                                    x.Visible = true;
                                }, token);
                            }
                        }

                        int intSpecialAttribPoints = 0;

                        string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
                        if (!string.IsNullOrEmpty(strSelectedMetatype))
                        {
                            string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
                            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + (await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath() + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                            {
                                if (xmlBaseMetatypePriorityList.Count == 1 || await xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null)
                                {
                                    XPathNavigator objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                                    if (!string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                                        objXmlMetatypePriorityNode = objXmlMetatypePriorityNode?.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + ']');
                                    if (objXmlMetatypePriorityNode != null && int.TryParse(
                                            (await objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpressionAsync(
                                                "value"))?.Value, out int intTemp))
                                        intSpecialAttribPoints += intTemp;
                                    break;
                                }
                            }
                        }

                        if (int.TryParse((await xmlTalentNode.SelectSingleNodeAndCacheExpressionAsync("specialattribpoints"))?.Value, out int intTalentSpecialAttribPoints))
                            intSpecialAttribPoints += intTalentSpecialAttribPoints;

                        await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token);
                    }
                }
            }
            else
            {
                await cboTalents.DoThreadSafeAsync(x => x.SelectedIndex = 0, token);
            }
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                await SumToTen(true, token);
            }
        }

        private async void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    await RefreshSelectedMetatype();
                    await PopulateTalents();
                    if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                    {
                        await SumToTen();
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    await PopulateMetatypes();
                    if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                    {
                        await SumToTen();
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    switch (_objCharacter.EffectiveBuildMethod)
                    {
                        case CharacterBuildMethod.Priority:
                            await ManagePriorityItems(cboHeritage);
                            break;

                        case CharacterBuildMethod.SumtoTen:
                            await SumToTen();
                            break;
                    }

                    await LoadMetatypes();
                    await PopulateMetatypes();
                    await PopulateMetavariants();
                    await RefreshSelectedMetatype();
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cboTalent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    switch (_objCharacter.EffectiveBuildMethod)
                    {
                        case CharacterBuildMethod.Priority:
                            await ManagePriorityItems(cboTalent);
                            break;

                        case CharacterBuildMethod.SumtoTen:
                            await SumToTen();
                            break;
                    }

                    await PopulateTalents();
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cboAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    switch (_objCharacter.EffectiveBuildMethod)
                    {
                        case CharacterBuildMethod.Priority:
                            await ManagePriorityItems(cboAttributes);
                            break;

                        case CharacterBuildMethod.SumtoTen:
                            await SumToTen();
                            break;
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cboSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    switch (_objCharacter.EffectiveBuildMethod)
                    {
                        case CharacterBuildMethod.Priority:
                            await ManagePriorityItems(cboSkills);
                            break;

                        case CharacterBuildMethod.SumtoTen:
                            await SumToTen();
                            break;
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private async void cboResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (CursorWait.New(this))
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout());
                try
                {
                    switch (_objCharacter.EffectiveBuildMethod)
                    {
                        case CharacterBuildMethod.Priority:
                            await ManagePriorityItems(cboResources);
                            break;

                        case CharacterBuildMethod.SumtoTen:
                            await SumToTen();
                            break;
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout());
                }
            }
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            using (CursorWait.New(this))
                cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }

        #endregion Control Events

        #region Custom Methods

        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private async ValueTask MetatypeSelected(CancellationToken token = default)
        {
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                int intSumToTen = await SumToTen(false, token);
                if (intSumToTen != _objCharacter.Settings.SumtoTen)
                {
                    Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_SumtoTen"),
                        _objCharacter.Settings.SumtoTen.ToString(GlobalSettings.CultureInfo), intSumToTen.ToString(GlobalSettings.CultureInfo)));
                    return;
                }
            }
            if (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedIndex, token) == -1)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Metatype_SelectTalent"), await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectTalent"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSkill1 = await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            string strSkill2 = await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            string strSkill3 = await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);

            if ((await cboSkill1.DoThreadSafeFuncAsync(x => x.Visible, token) && string.IsNullOrEmpty(strSkill1))
                || (await cboSkill2.DoThreadSafeFuncAsync(x => x.Visible, token) && string.IsNullOrEmpty(strSkill2))
                || (await cboSkill3.DoThreadSafeFuncAsync(x => x.Visible, token) && string.IsNullOrEmpty(strSkill3)))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Metatype_SelectSkill"), await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectSkill"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (ExoticSkill.IsExoticSkillName(strSkill1))
            {
                using (SelectExoticSkill frmSelectExotic = await this.DoThreadSafeFuncAsync(() => new SelectExoticSkill(_objCharacter), token))
                {
                    frmSelectExotic.ForceSkill(strSkill1);
                    if (await frmSelectExotic.ShowDialogSafeAsync(this) != DialogResult.OK)
                        return;
                    strSkill1 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }
            if (ExoticSkill.IsExoticSkillName(strSkill2))
            {
                using (SelectExoticSkill frmSelectExotic = await this.DoThreadSafeFuncAsync(() => new SelectExoticSkill(_objCharacter), token))
                {
                    frmSelectExotic.ForceSkill(strSkill2);
                    if (await frmSelectExotic.ShowDialogSafeAsync(this) != DialogResult.OK)
                        return;
                    strSkill2 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }
            if (ExoticSkill.IsExoticSkillName(strSkill3))
            {
                using (SelectExoticSkill frmSelectExotic = await this.DoThreadSafeFuncAsync(() => new SelectExoticSkill(_objCharacter), token))
                {
                    frmSelectExotic.ForceSkill(strSkill3);
                    if (await frmSelectExotic.ShowDialogSafeAsync(this) != DialogResult.OK)
                        return;
                    strSkill3 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }

            if ((await cboSkill1.DoThreadSafeFuncAsync(x => x.Visible, token) && await cboSkill2.DoThreadSafeFuncAsync(x => x.Visible && strSkill1 == strSkill2, token))
                || (await cboSkill1.DoThreadSafeFuncAsync(x => x.Visible, token) && await cboSkill3.DoThreadSafeFuncAsync(x => x.Visible && strSkill1 == strSkill3, token))
                || (await cboSkill2.DoThreadSafeFuncAsync(x => x.Visible, token) && await cboSkill3.DoThreadSafeFuncAsync(x => x.Visible && strSkill2 == strSkill3, token)))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Metatype_Duplicate"), await LanguageManager.GetStringAsync("MessageTitle_Metatype_Duplicate"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                XmlNode objXmlMetatype
                    = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode(
                        "metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                if (objXmlMetatype == null)
                {
                    Program.ShowMessageBox(
                        this, await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype"),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                strSelectedMetatype = objXmlMetatype["id"]?.InnerText ?? Guid.Empty.ToString("D");

                // Clear out all priority-only qualities that the character bought normally (relevant when switching from Karma to Priority/Sum-to-Ten)
                for (int i = _objCharacter.Qualities.Count - 1; i >= 0; --i)
                {
                    if (i >= _objCharacter.Qualities.Count)
                        continue;
                    Quality objQuality = _objCharacter.Qualities[i];
                    if (objQuality.OriginSource == QualitySource.Selected
                        && (await objQuality.GetNodeXPathAsync())?.SelectSingleNode("onlyprioritygiven") != null)
                        objQuality.DeleteQuality();
                }

                string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                string strSelectedMetatypeCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                    strSelectedMetavariant = "Human";
                XmlNode objXmlMetavariant
                    = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = "
                                                      + strSelectedMetavariant.CleanXPath() + ']');
                strSelectedMetavariant = objXmlMetavariant?["id"]?.InnerText ?? Guid.Empty.ToString();
                int intForce = await nudForce.DoThreadSafeFuncAsync(x => x.Visible ? x.ValueAsInt : 0, token);

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
                        XPathNavigator xmlRestrictionNode
                            = (await objQuality.GetNodeXPathAsync())?.SelectSingleNode("required");
                        if (xmlRestrictionNode != null &&
                            (xmlRestrictionNode.SelectSingleNode(".//metatype") != null
                             || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                        {
                            lstQualitiesToCheck.Add(objQuality);
                        }
                        else
                        {
                            xmlRestrictionNode = (await objQuality.GetNodeXPathAsync())?.SelectSingleNode("forbidden");
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNode(".//metatype") != null
                                 || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                        }
                    }

                    _objCharacter.Create(strSelectedMetatypeCategory, strSelectedMetatype,
                                         strSelectedMetavariant, objXmlMetatype, intForce,
                                         _xmlQualityDocumentQualitiesNode,
                                         _xmlCritterPowerDocumentPowersNode, null,
                                         await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Checked, token)
                                             ? await cboPossessionMethod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                             : string.Empty);
                    foreach (Quality objQuality in lstQualitiesToCheck)
                    {
                        // Set strIgnoreQuality to quality's name to make sure limit counts are not an issue
                        if ((await objQuality.GetNodeXPathAsync())?.RequirementsMet(
                                _objCharacter, strIgnoreQuality: objQuality.Name) == false)
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
                _objCharacter.MetatypeBP = Convert.ToInt32(await lblMetavariantKarma.DoThreadSafeFuncAsync(x => x.Text, token), GlobalSettings.CultureInfo);
                _objCharacter.MetatypePriority = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                _objCharacter.AttributesPriority = await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                _objCharacter.SpecialPriority = await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                _objCharacter.SkillsPriority = await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                _objCharacter.ResourcesPriority = await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                _objCharacter.TalentPriority = await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                await _objCharacter.PriorityBonusSkillList.ClearAsync();
                if (await cboSkill1.DoThreadSafeFuncAsync(x => x.Visible, token))
                    await _objCharacter.PriorityBonusSkillList.AddAsync(strSkill1);
                if (await cboSkill2.DoThreadSafeFuncAsync(x => x.Visible, token))
                    await _objCharacter.PriorityBonusSkillList.AddAsync(strSkill2);
                if (await cboSkill3.DoThreadSafeFuncAsync(x => x.Visible, token))
                    await _objCharacter.PriorityBonusSkillList.AddAsync(strSkill3);

                // Set starting nuyen
                XPathNodeIterator xmlResourcesPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Resources\" and value = "
                    + _objCharacter.ResourcesPriority.CleanXPath() +
                    " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath()
                    + ")]");
                foreach (XPathNavigator xmlResourcesPriority in xmlResourcesPriorityList)
                {
                    if (xmlResourcesPriorityList.Count == 1 ||
                        (await xmlResourcesPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null &&
                         await xmlResourcesPriority.SelectSingleNodeAndCacheExpressionAsync("gameplayoption") != null))
                    {
                        decimal decResources = 0;
                        if (xmlResourcesPriority.TryGetDecFieldQuickly("resources", ref decResources))
                            _objCharacter.StartingNuyen = _objCharacter.Nuyen = decResources;
                        break;
                    }
                }

                switch (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token))
                {
                    case "Aspected Magician":
                    case "Enchanter":
                        await _objCharacter.PushText.PushAsync(strSkill1);
                        break;
                }

                XmlNode charNode =
                    strSelectedMetatypeCategory == "Shapeshifter" || strSelectedMetavariant == Guid.Empty.ToString()
                        ? objXmlMetatype
                        : objXmlMetavariant ?? objXmlMetatype;

                int intSpecialAttribPoints = 0;
                bool boolHalveAttributePriorityPoints = charNode.NodeExists("halveattributepoints");
                if (strOldSpecialPriority != _objCharacter.SpecialPriority
                    || strOldTalentPriority != _objCharacter.SpecialPriority)
                {
                    List<Quality> lstOldPriorityQualities = _objCharacter.Qualities
                                                                         .Where(x => x.OriginSource
                                                                             == QualitySource.Heritage).ToList();
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    bool blnRemoveFreeSkills = true;
                    XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Talent\" and value = "
                        + _objCharacter.SpecialPriority.CleanXPath() +
                        " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                    {
                        if (xmlBaseTalentPriorityList.Count == 1
                            || await xmlBaseTalentPriority.SelectSingleNodeAndCacheExpressionAsync("gameplayoption")
                            != null)
                        {
                            XPathNavigator xmlTalentPriorityNode
                                = xmlBaseTalentPriority.SelectSingleNode(
                                    "talents/talent[value = " + _objCharacter.TalentPriority.CleanXPath() + ']');

                            if (xmlTalentPriorityNode != null)
                            {
                                // Create the Qualities that come with the Talent.
                                foreach (XPathNavigator objXmlQualityItem in await xmlTalentPriorityNode
                                             .SelectAndCacheExpressionAsync("qualities/quality"))
                                {
                                    XmlNode objXmlQuality
                                        = _xmlQualityDocumentQualitiesNode.SelectSingleNode(
                                            "quality[name = " + objXmlQualityItem.Value.CleanXPath() + ']');
                                    Quality objQuality = new Quality(_objCharacter);
                                    string strForceValue
                                        = (await objXmlQualityItem.SelectSingleNodeAndCacheExpressionAsync("@select"))
                                        ?.Value ?? string.Empty;
                                    objQuality.Create(objXmlQuality, QualitySource.Heritage, lstWeapons, strForceValue);
                                    Quality objExistingQuality = lstOldPriorityQualities.Find(
                                        x => x.SourceIDString == objQuality.SourceIDString
                                             && x.Extra == objQuality.Extra && x.Type == objQuality.Type);
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
                                _objCharacter.FreeSpells
                                    = xmlTalentPriorityNode.TryGetInt32FieldQuickly("spells", ref intTemp)
                                        ? intTemp
                                        : 0;
                                // Set starting resonance
                                if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("resonance", ref intTemp))
                                    intTemp = 1;
                                if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxresonance", ref intMax))
                                    intMax = Math.Max(CommonFunctions.ExpressionToInt(
                                                          charNode["resmax"]?.InnerText, intForce), intTemp);
                                _objCharacter.RES.AssignLimits(intTemp, intMax, intMax);
                                _objCharacter.CFPLimit
                                    = xmlTalentPriorityNode.TryGetInt32FieldQuickly("cfp", ref intTemp) ? intTemp : 0;
                                // Set starting depth
                                if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("depth", ref intTemp))
                                    intTemp = 1;
                                if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxdepth", ref intMax))
                                    intMax = Math.Max(CommonFunctions.ExpressionToInt(
                                                          charNode["depmax"]?.InnerText, intForce), intTemp);
                                _objCharacter.DEP.AssignLimits(intTemp, intMax, intMax);
                                _objCharacter.AINormalProgramLimit
                                    = xmlTalentPriorityNode.TryGetInt32FieldQuickly("ainormalprogramlimit", ref intTemp)
                                        ? intTemp
                                        : 0;
                                _objCharacter.AIAdvancedProgramLimit
                                    = xmlTalentPriorityNode.TryGetInt32FieldQuickly(
                                        "aiadvancedprogramlimit", ref intTemp)
                                        ? intTemp
                                        : 0;

                                // Set Free Skills/Skill Groups
                                int intFreeLevels = 0;
                                Improvement.ImprovementType eType = Improvement.ImprovementType.SkillBase;
                                XPathNavigator objTalentSkillValNode
                                    = await xmlTalentPriorityNode.SelectSingleNodeAndCacheExpressionAsync("skillval");
                                if (objTalentSkillValNode == null
                                    || !int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                {
                                    objTalentSkillValNode
                                        = await xmlTalentPriorityNode.SelectSingleNodeAndCacheExpressionAsync(
                                            "skillgroupval");
                                    if (objTalentSkillValNode != null
                                        && int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                    {
                                        eType = Improvement.ImprovementType.SkillGroupBase;
                                    }
                                }

                                blnRemoveFreeSkills = false;
                                AddFreeSkills(intFreeLevels, eType, strSkill1, strSkill2, strSkill3);

                                if (int.TryParse(
                                        (await xmlTalentPriorityNode.SelectSingleNodeAndCacheExpressionAsync(
                                            "specialattribpoints"))?.Value, out int intTalentSpecialAttribPoints))
                                    intSpecialAttribPoints += intTalentSpecialAttribPoints;
                            }

                            break;
                        }
                    }

                    if (blnRemoveFreeSkills)
                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter, _objCharacter.Improvements
                                                                             .Where(x => x.ImproveSource
                                                                                 == Improvement.ImprovementSource
                                                                                     .Heritage
                                                                                 && (x.ImproveType
                                                                                     == Improvement.ImprovementType
                                                                                         .SkillBase
                                                                                     || x.ImproveType
                                                                                     == Improvement.ImprovementType
                                                                                         .SkillGroupBase))
                                                                             .ToList());
                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in lstWeapons)
                        _objCharacter.Weapons.Add(objWeapon);
                }

                // Set Special Attributes

                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Heritage\" and value = "
                    + _objCharacter.MetatypePriority.CleanXPath()
                    + " and (not(prioritytable) or prioritytable = "
                    + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1
                        || await xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable")
                        != null)
                    {
                        XPathNavigator objXmlMetatypePriorityNode
                            = xmlBaseMetatypePriority.SelectSingleNode(
                                "metatypes/metatype[name = " + _objCharacter.Metatype.CleanXPath() + ']');
                        if (strSelectedMetavariant != Guid.Empty.ToString())
                            objXmlMetatypePriorityNode = objXmlMetatypePriorityNode?.SelectSingleNode(
                                "metavariants/metavariant[name = " + _objCharacter.Metavariant.CleanXPath() + ']');
                        if (objXmlMetatypePriorityNode != null && int.TryParse(
                                (await objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpressionAsync("value"))
                                ?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                _objCharacter.Special = intSpecialAttribPoints;
                _objCharacter.TotalSpecial = _objCharacter.Special;

                // Set Attributes
                XPathNodeIterator objXmlAttributesPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Attributes\" and value = "
                    + _objCharacter.AttributesPriority.CleanXPath() +
                    " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath()
                    + ")]");
                foreach (XPathNavigator objXmlAttributesPriority in objXmlAttributesPriorityList)
                {
                    if (objXmlAttributesPriorityList.Count == 1 ||
                        (await objXmlAttributesPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null
                         &&
                         await objXmlAttributesPriority.SelectSingleNodeAndCacheExpressionAsync("gameplayoption")
                         != null))
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
                XPathNodeIterator objXmlSkillsPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Skills\" and value = " + _objCharacter.SkillsPriority.CleanXPath()
                                                                             +
                                                                             " and (not(prioritytable) or prioritytable = "
                                                                             + _objCharacter.Settings.PriorityTable
                                                                                 .CleanXPath() + ")]");
                foreach (XPathNavigator objXmlSkillsPriority in objXmlSkillsPriorityList)
                {
                    if (objXmlSkillsPriorityList.Count == 1 ||
                        (await objXmlSkillsPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null &&
                         await objXmlSkillsPriority.SelectSingleNodeAndCacheExpressionAsync("gameplayoption") != null))
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
                if (_objCharacter.DEPEnabled || _objCharacter.Metatype.EndsWith("Sprite", StringComparison.Ordinal))
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
                            if (objAttribute.Karma > 0 && (objAttributeToShift == null
                                                           || objAttributeToShift.Value < objAttribute.Value))
                            {
                                objAttributeToShift = objAttribute;
                            }
                        }

                        if (objAttributeToShift == null)
                            break;
                        int intKarma = Math.Min(objAttributeToShift.Karma,
                                                _objCharacter.TotalAttributes - intPointsSpent);
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
                            if (objAttribute.Karma > 0 && (objAttributeToShift == null
                                                           || objAttributeToShift.Value < objAttribute.Value))
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
                            if (objGroup.Karma > 0
                                && (objGroupToShift == null || objGroupToShift.Rating < objGroup.Rating))
                            {
                                objGroupToShift = objGroup;
                            }
                        }

                        if (objGroupToShift == null)
                            break;
                        int intKarma = Math.Min(objGroupToShift.Karma,
                                                _objCharacter.SkillsSection.SkillGroupPointsMaximum - intPointsSpent);
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
                            if (objSkill.Karma > 0
                                && (objSkillToShift == null || objSkillToShift.Rating < objSkill.Rating))
                            {
                                objSkillToShift = objSkill;
                            }
                        }

                        if (objSkillToShift == null)
                            break;
                        int intKarma = Math.Min(objSkillToShift.Karma,
                                                _objCharacter.SkillsSection.SkillGroupPointsMaximum - intPointsSpent);
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
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype"),
                                       await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype"),
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private async ValueTask ManagePriorityItems(ComboBox comboBox, bool blnForce = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!blnForce && _objCharacter.EffectiveBuildMethod != CharacterBuildMethod.Priority)
                return;
            List<string> lstCurrentPriorities = new List<string>(_lstPriorities);
            string strHeritageSelected = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
            string strAttributesSelected = await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
            string strTalentSelected = await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
            string strSkillsSelected = await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
            string strResourcesSelected = await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);

            // Discover which priority rating is not currently assigned
            lstCurrentPriorities.Remove(strHeritageSelected);
            lstCurrentPriorities.Remove(strAttributesSelected);
            lstCurrentPriorities.Remove(strTalentSelected);
            lstCurrentPriorities.Remove(strSkillsSelected);
            lstCurrentPriorities.Remove(strResourcesSelected);
            if (lstCurrentPriorities.Count == 0)
                return;
            string strComboBoxSelected = comboBox.DoThreadSafeFunc(x => x.SelectedValue).ToString();

            string strMissing = lstCurrentPriorities[0];

            // Find the combo with the same value as this one and change it to the missing value.
            //_blnInitializing = true;
            string strMyName = await comboBox.DoThreadSafeFuncAsync(x => x.Name, token);
            if (strHeritageSelected == strComboBoxSelected && strMyName != await cboHeritage.DoThreadSafeFuncAsync(x => x.Name, token))
                await cboHeritage.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
            else if (strAttributesSelected == strComboBoxSelected && strMyName != await cboAttributes.DoThreadSafeFuncAsync(x => x.Name, token))
                await cboAttributes.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
            else if (strTalentSelected == strComboBoxSelected && strMyName != await cboTalent.DoThreadSafeFuncAsync(x => x.Name, token))
                await cboTalent.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
            else if (strSkillsSelected == strComboBoxSelected && strMyName != await cboSkills.DoThreadSafeFuncAsync(x => x.Name, token))
                await cboSkills.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
            else if (strResourcesSelected == strComboBoxSelected && strMyName != await cboResources.DoThreadSafeFuncAsync(x => x.Name, token))
                await cboResources.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);

            if (lstCurrentPriorities.Count <= 1)
                return;
            do
            {
                lstCurrentPriorities.Clear();
                lstCurrentPriorities.AddRange(_lstPriorities);
                strHeritageSelected = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                strAttributesSelected = await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                strTalentSelected = await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                strSkillsSelected = await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);
                strResourcesSelected = await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token);

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
                strMyName = await comboBox.DoThreadSafeFuncAsync(x => x.Name, token);
                if (strHeritageSelected == strComboBoxSelected && strMyName != await cboHeritage.DoThreadSafeFuncAsync(x => x.Name, token))
                    await cboHeritage.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
                else if (strAttributesSelected == strComboBoxSelected && strMyName != await cboAttributes.DoThreadSafeFuncAsync(x => x.Name, token))
                    await cboAttributes.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
                else if (strTalentSelected == strComboBoxSelected && strMyName != await cboTalent.DoThreadSafeFuncAsync(x => x.Name, token))
                    await cboTalent.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
                else if (strSkillsSelected == strComboBoxSelected && strMyName != await cboSkills.DoThreadSafeFuncAsync(x => x.Name, token))
                    await cboSkills.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
                else if (strResourcesSelected == strComboBoxSelected && strMyName != await cboResources.DoThreadSafeFuncAsync(x => x.Name, token))
                    await cboResources.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token);
            } while (lstCurrentPriorities.Count > 1);
        }

        private async ValueTask<int> SumToTen(bool blnDoUIUpdate = true, CancellationToken token = default)
        {
            int value = _dicSumtoTenValues[await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)];
            value += _dicSumtoTenValues[await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)];
            value += _dicSumtoTenValues[await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)];
            value += _dicSumtoTenValues[await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)];
            value += _dicSumtoTenValues[await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)];

            if (blnDoUIUpdate)
                await lblSumtoTen.DoThreadSafeAsync(x => x.Text = value.ToString(GlobalSettings.CultureInfo) + '/'
                                                        + _objCharacter.Settings.SumtoTen.ToString(GlobalSettings.CultureInfo), token);

            return value;
        }

        private async ValueTask RefreshSelectedMetatype(CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            string strSelectedHeritage = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);

            XPathNavigator objXmlMetatype
                = _xmlBaseMetatypeDataNode.SelectSingleNode(
                    "metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
            XPathNavigator objXmlMetavariant
                = string.IsNullOrEmpty(strSelectedMetavariant) || strSelectedMetavariant == "None"
                    ? null
                    : objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = "
                                                       + strSelectedMetavariant.CleanXPath() + ']');
            XPathNavigator objXmlMetatypePriorityNode = null;
            XPathNavigator objXmlMetavariantPriorityNode = null;
            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                "priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                                                                           + " and (not(prioritytable) or prioritytable = "
                                                                           + _objCharacter.Settings.PriorityTable
                                                                               .CleanXPath() + ")]");
            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
            {
                if (xmlBaseMetatypePriorityList.Count == 1
                    || await xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable")
                    != null)
                {
                    objXmlMetatypePriorityNode
                        = xmlBaseMetatypePriority.SelectSingleNode(
                            "metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                    objXmlMetavariantPriorityNode = objXmlMetavariant != null
                        ? objXmlMetatypePriorityNode.SelectSingleNode(
                            "metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + ']')
                        : null;
                    break;
                }
            }

            string strAttributeFormat = "{0}/{1}" + strSpace + "({2})";
            if (objXmlMetavariant != null)
            {
                if (objXmlMetavariantPriorityNode == null)
                {
                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("String_NotSupported"),
                                           Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                }
                else
                {
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token);
                }

                string strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("bodmin"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                string strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("bodmax"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                string strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("bodaug"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblBOD.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("agimin"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("agimax"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("agiaug"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblAGI.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("reamin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("reamax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("reaaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblREA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("strmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("strmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("straug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblSTR.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("chamin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("chamax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("chaaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblCHA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("intmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("intmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("intaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblINT.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("logmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("logmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("logaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblLOG.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("wilmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("wilmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("wilaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblWIL.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);

                string strKarmaText
                    = (await objXmlMetavariantPriorityNode.SelectSingleNodeAndCacheExpressionAsync("karma"))
                      ?.Value
                      ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblMetavariantKarma.DoThreadSafeAsync(x => x.Text = strKarmaText, token);

                string strSource = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("source"))
                    ?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage
                        = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value
                          ?? (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("page"))?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = await SourceString.GetSourceStringAsync(
                            strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo,
                            _objCharacter);
                        await lblSource.DoThreadSafeAsync(x => x.Text = objSource.ToString(), token);
                        await lblSource.SetToolTipAsync(objSource.LanguageBookTooltip, token);
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                        await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token);
                        await lblSource.SetToolTipAsync(strUnknown, token);
                    }
                }
                else
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                    await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token);
                    await lblSource.SetToolTipAsync(strUnknown, token);
                }

                // Set the special attributes label.
                if (objXmlMetavariantPriorityNode == null || !int.TryParse(
                        (await objXmlMetavariantPriorityNode.SelectSingleNodeAndCacheExpressionAsync("value"))
                        ?.Value,
                        NumberStyles.Any,
                        GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                    intSpecialAttribPoints = 0;

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Talent\" and value = "
                    + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = "
                    + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1
                        || await xmlBaseTalentPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable")
                        != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode(
                            "talents/talent[value = "
                            + (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath() + ']');
                        if (objXmlTalentsNode != null
                            && int.TryParse(
                                (await objXmlTalentsNode.SelectSingleNodeAndCacheExpressionAsync(
                                    "specialattribpoints"))
                                ?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token);

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in await objXmlMetavariant.SelectAndCacheExpressionAsync(
                             "qualities/*/quality"))
                {
                    string strQuality;
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                        StringComparison.OrdinalIgnoreCase))
                    {
                        strQuality = _xmlBaseQualityDataNode
                                     .SelectSingleNode("qualities/quality[name = "
                                                       + objXmlQuality.Value.CleanXPath() + "]/translate")
                                     ?.Value
                                     ?? objXmlQuality.Value;

                        string strSelect
                            = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select"))
                            ?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect)
                                          + ')';
                    }
                    else
                    {
                        strQuality = objXmlQuality.Value;
                        string strSelect
                            = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select"))
                            ?.Value;
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
                        await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = sbdQualities.ToString(), token);
                    }
                }
                else
                {
                    string strNone = await LanguageManager.GetStringAsync("String_None");
                    await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = strNone, token);
                }
            }
            else if (objXmlMetatype != null)
            {
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token);
                string strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodmin"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                string strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodmax"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                string strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodaug"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblBOD.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agimin"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agimax"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agiaug"))?.Value
                                ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblAGI.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reamin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reamax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reaaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblREA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("strmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("strmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("straug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblSTR.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chamin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chamax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chaaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblCHA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblINT.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblLOG.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);
                strMin = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilmin"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilmax"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                strAug = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilaug"))?.Value
                         ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblWIL.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token);

                string strKarmaText
                    = (await objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpressionAsync("karma"))?.Value
                      ?? 0.ToString(GlobalSettings.CultureInfo);
                await lblMetavariantKarma.DoThreadSafeAsync(x => x.Text = strKarmaText, token);

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metatype's Qualities.
                foreach (XPathNavigator xmlQuality in await objXmlMetatype.SelectAndCacheExpressionAsync(
                             "qualities/*/quality"))
                {
                    string strQuality;
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                        StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objQuality
                            = _xmlBaseQualityDataNode.SelectSingleNode(
                                "qualities/quality[name = " + xmlQuality.Value.CleanXPath() + ']');
                        strQuality = (await objQuality.SelectSingleNodeAndCacheExpressionAsync("translate"))
                                     ?.Value
                                     ?? xmlQuality.Value;

                        string strSelect = (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select"))
                            ?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect)
                                          + ')';
                    }
                    else
                    {
                        strQuality = xmlQuality.Value;
                        string strSelect = (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select"))
                            ?.Value;
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
                        await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = sbdQualities.ToString(), token);
                    }
                }
                else
                {
                    string strNone = await LanguageManager.GetStringAsync("String_None");
                    await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = strNone, token);
                }

                string strSource = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("source"))
                    ?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage
                        = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value
                          ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("page"))?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = await SourceString.GetSourceStringAsync(
                            strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo,
                            _objCharacter);
                        lblSource.Text = objSource.ToString();
                        await lblSource.SetToolTipAsync(objSource.LanguageBookTooltip, token);
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                        await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token);
                        await lblSource.SetToolTipAsync(strUnknown, token);
                    }
                }
                else
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                    await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token);
                    await lblSource.SetToolTipAsync(strUnknown, token);
                }

                // Set the special attributes label.
                if (!int.TryParse(
                        (await objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpressionAsync("value"))
                        ?.Value,
                        NumberStyles.Any,
                        GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                    intSpecialAttribPoints = 0;

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Talent\" and value = "
                    + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = "
                    + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1
                        || await xmlBaseTalentPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable")
                        != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode(
                            "talents/talent[value = "
                            + (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath() + ']');
                        if (objXmlTalentsNode != null
                            && int.TryParse(
                                (await objXmlTalentsNode.SelectSingleNodeAndCacheExpressionAsync(
                                    "specialattribpoints"))
                                ?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token);
            }
            else
            {
                await lblBOD.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblAGI.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblREA.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblSTR.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblCHA.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblINT.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblLOG.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblWIL.DoThreadSafeAsync(x => x.Text = string.Empty, token);

                int intSpecialAttribPoints = 0;
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Talent\" and value = "
                    + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = "
                    + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1
                        || await xmlBaseTalentPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable")
                        != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode(
                            "talents/talent[value = "
                            + (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath() + ']');
                        if (objXmlTalentsNode != null
                            && int.TryParse(
                                (await objXmlTalentsNode.SelectSingleNodeAndCacheExpressionAsync(
                                    "specialattribpoints"))
                                ?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token);

                await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblMetavariantKarma.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblSource.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
            }

            bool blnVisible = await lblBOD.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblBODLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblAGI.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblAGILabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblREA.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblREALabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblSTR.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblSTRLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblCHA.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblCHALabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblINT.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblINTLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblLOG.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblLOGLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblWIL.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblWILLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblSpecialAttributes.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblSpecialAttributesLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblMetavariantQualities.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblMetavariantQualitiesLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblMetavariantKarma.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblMetavariantKarmaLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
            blnVisible = await lblSourceLabel.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token);
        }

        private async ValueTask PopulateTalents(CancellationToken token = default)
        {
            // Load the Priority information.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTalent))
            {
                // Populate the Priority Category list.
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Talent\" and value = "
                    + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath()
                    + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1
                        || await xmlBaseTalentPriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null)
                    {
                        foreach (XPathNavigator objXmlPriorityTalent in await xmlBaseTalentPriority.SelectAndCacheExpressionAsync(
                                     "talents/talent"))
                        {
                            XPathNavigator xmlQualitiesNode
                                = await objXmlPriorityTalent.SelectSingleNodeAndCacheExpressionAsync("qualities");
                            if (xmlQualitiesNode != null)
                            {
                                bool blnFoundUnavailableQuality = false;

                                foreach (XPathNavigator xmlQuality in await xmlQualitiesNode.SelectAndCacheExpressionAsync(
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
                                = await objXmlPriorityTalent.SelectSingleNodeAndCacheExpressionAsync("forbidden");
                            if (xmlForbiddenNode != null)
                            {
                                bool blnRequirementForbidden = false;

                                // Loop through the oneof requirements.
                                XPathNodeIterator objXmlForbiddenList
                                    = await xmlForbiddenNode.SelectAndCacheExpressionAsync("oneof");
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
                                                if (objXmlForbidden.Value == await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token))
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
                                                if (objXmlForbidden.Value == await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token))
                                                {
                                                    blnRequirementForbidden = true;
                                                    goto EndForbiddenLoop;
                                                }

                                                break;
                                            }
                                            case "metavariant" when objXmlForbidden.Value == await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token):
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
                                = await objXmlPriorityTalent.SelectSingleNodeAndCacheExpressionAsync("required");
                            if (xmlRequiredNode != null)
                            {
                                bool blnRequirementMet = false;

                                // Loop through the oneof requirements.
                                XPathNodeIterator objXmlForbiddenList
                                    = await xmlRequiredNode.SelectAndCacheExpressionAsync("oneof");
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
                                                if (objXmlRequired.Value == await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token))
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
                                                if (objXmlRequired.Value == await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token))
                                                {
                                                    blnRequirementMet = true;
                                                    goto EndRequiredLoop;
                                                }

                                                break;
                                            }
                                            case "metavariant" when objXmlRequired.Value == await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token):
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
                                              (await objXmlPriorityTalent.SelectSingleNodeAndCacheExpressionAsync("value"))?.Value,
                                              (await objXmlPriorityTalent.SelectSingleNodeAndCacheExpressionAsync("translate"))
                                                                  ?.Value ??
                                              (await objXmlPriorityTalent.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value ??
                                              await LanguageManager.GetStringAsync("String_Unknown")));
                        }

                        break;
                    }
                }

                lstTalent.Sort(CompareListItems.CompareNames);
                int intOldSelectedIndex = await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedIndex, token);
                int intOldDataSourceSize = await cboTalents.DoThreadSafeFuncAsync(x => x.Items.Count, token);
                await cboTalents.PopulateWithListItemsAsync(lstTalent, token);
                if (intOldDataSourceSize == await cboTalents.DoThreadSafeFuncAsync(x => x.Items.Count, token))
                {
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    await cboTalents.DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex, token);
                    _blnLoading = blnOldLoading;
                }

                await cboTalents.DoThreadSafeAsync(x => x.Enabled = x.Items.Count > 1, token);
            }
        }

        private async ValueTask PopulateMetavariants(CancellationToken token = default)
        {
            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);

            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedHeritage = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);

                XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + ']');
                XPathNavigator objXmlMetatypeBP = null;
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || await xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null)
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
                        lstMetavariants.Add(new ListItem(Guid.Empty, await LanguageManager.GetStringAsync("String_None")));
                        // Retrieve the list of Metavariants for the selected Metatype.
                        foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select(
                                     "metavariants/metavariant[" + _objCharacter.Settings.BookXPath() + ']'))
                        {
                            string strName = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            if (objXmlMetatypeBP.SelectSingleNode(
                                    "metavariants/metavariant[name = " + strName.CleanXPath() + ']') != null)
                            {
                                lstMetavariants.Add(new ListItem(
                                                        strName,
                                                        (await objXmlMetavariant
                                                            .SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                        ?? strName));
                            }
                        }

                        string strOldSelectedValue
                            = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? _objCharacter.Metavariant;
                        bool blnOldLoading = _blnLoading;
                        _blnLoading = true;
                        await cboMetavariant.PopulateWithListItemsAsync(lstMetavariants, token);
                        await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = lstMetavariants.Count > 1, token);
                        _blnLoading = blnOldLoading;
                        await cboMetavariant.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strOldSelectedValue))
                                x.SelectedValue = strOldSelectedValue;
                            if (x.SelectedIndex == -1)
                                x.SelectedIndex = 0;
                        }, token);

                        // If the Metatype has Force enabled, show the Force NUD.
                        string strEssMax = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("essmax"))?.Value
                                           ?? string.Empty;
                        int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                        if (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("forcecreature") != null || intPos != -1)
                        {
                            if (intPos != -1)
                            {
                                string strD6 = await LanguageManager.GetStringAsync("String_D6");
                                if (intPos > 0)
                                {
                                    --intPos;
                                    await lblForceLabel.DoThreadSafeAsync(x => x.Text = strEssMax.Substring(intPos, 3)
                                                                              .Replace("D6", strD6), token);
                                    await nudForce.DoThreadSafeAsync(x => x.Maximum
                                                                         = Convert.ToInt32(
                                                                             strEssMax[intPos],
                                                                             GlobalSettings.InvariantCultureInfo) * 6, token);
                                }
                                else
                                {
                                    await lblForceLabel.DoThreadSafeAsync(x => x.Text = 1.ToString(GlobalSettings.CultureInfo) + strD6, token);
                                    await nudForce.DoThreadSafeAsync(x => x.Maximum = 6, token);
                                }
                            }
                            else
                            {
                                lblForceLabel.Text = await LanguageManager.GetStringAsync(
                                    await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("forceislevels") != null
                                        ? "String_Level"
                                        : "String_Force");
                                await nudForce.DoThreadSafeAsync(x => x.Maximum = 100, token);
                            }

                            await lblForceLabel.DoThreadSafeAsync(x => x.Visible = true, token);
                            await nudForce.DoThreadSafeAsync(x => x.Visible = true, token);
                        }
                        else
                        {
                            await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                            await nudForce.DoThreadSafeAsync(x => x.Visible = false, token);
                        }
                    }
                }
                else
                {
                    await cboMetavariant.PopulateWithListItemsAsync(new ListItem("None", await LanguageManager.GetStringAsync("String_None")).Yield(), token);
                    await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = false, token);

                    await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                    await nudForce.DoThreadSafeAsync(x => x.Visible = false, token);
                }
            }
            else
            {
                // Clear the Metavariant list if nothing is currently selected.
                await cboMetavariant.PopulateWithListItemsAsync(new ListItem("None", await LanguageManager.GetStringAsync("String_None")).Yield(), token);
                await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = false, token);

                await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                await nudForce.DoThreadSafeAsync(x => x.Visible = false, token);
            }
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        private async ValueTask PopulateMetatypes(CancellationToken token = default)
        {
            string strSelectedCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            if (!string.IsNullOrEmpty(strSelectedCategory))
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatype))
                {
                    XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Heritage\" and value = "
                        + (await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                    {
                        if (xmlBaseMetatypePriorityList.Count == 1
                            || await xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null)
                        {
                            foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                         "metatypes/metatype[(" + _objCharacter.Settings.BookXPath()
                                                                + ") and category = " + strSelectedCategory.CleanXPath()
                                                                + ']'))
                            {
                                string strName = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                                if (!string.IsNullOrEmpty(strName)
                                    && xmlBaseMetatypePriority.SelectSingleNode(
                                        "metatypes/metatype[name = " + strName.CleanXPath() + ']') != null)
                                {
                                    lstMetatype.Add(new ListItem(
                                                        strName,
                                                        (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("translate"))
                                                                      ?.Value ?? strName));
                                }
                            }

                            break;
                        }
                    }

                    lstMetatype.Sort(CompareListItems.CompareNames);
                    string strOldSelectedValue = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? _objCharacter.Metatype;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    await lstMetatypes.PopulateWithListItemsAsync(lstMetatype, token);
                    _blnLoading = blnOldLoading;
                    await lstMetatypes.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelectedValue))
                            x.SelectedValue = strOldSelectedValue;
                        if (x.SelectedIndex == -1 && lstMetatype.Count > 0)
                            x.SelectedIndex = 0;
                    }, token);
                }

                if (strSelectedCategory.EndsWith("Spirits", StringComparison.Ordinal))
                {
                    if (!await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Visible, token) && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                    {
                        await chkPossessionBased.DoThreadSafeAsync(x => x.Checked = true, token);
                        await cboPossessionMethod.DoThreadSafeAsync(x => x.SelectedValue = _strCurrentPossessionMethod, token);
                    }
                    await chkPossessionBased.DoThreadSafeAsync(x => x.Visible = true, token);
                    await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token);
                }
                else
                {
                    await chkPossessionBased.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, token);
                    await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token);
                }
            }
            else
            {
                await lstMetatypes.DoThreadSafeAsync(x => x.DataSource = null, token);
                await chkPossessionBased.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                }, token);
                await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token);
            }
        }

        private async ValueTask LoadMetatypes(CancellationToken token = default)
        {
            // Create a list of any Categories that should not be in the list.
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setRemoveCategories))
            {
                foreach (XPathNavigator objXmlCategory in await _xmlBaseMetatypeDataNode.SelectAndCacheExpressionAsync(
                             "categories/category"))
                {
                    XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Heritage\" and value = "
                        + (await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    bool blnRemoveCategory = true;
                    foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                    {
                        if (xmlBaseMetatypePriorityList.Count == 1
                            || await xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpressionAsync("prioritytable") != null)
                        {
                            foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                         "metatypes/metatype[category = " + objXmlCategory.Value.CleanXPath() + " and ("
                                         + _objCharacter.Settings.BookXPath() + ")]"))
                            {
                                if (xmlBaseMetatypePriority.SelectSingleNode(
                                        "metatypes/metatype[name = "
                                        + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
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
                    foreach (XPathNavigator objXmlCategory in await _xmlBaseMetatypeDataNode.SelectAndCacheExpressionAsync(
                                 "categories/category"))
                    {
                        string strInnerText = objXmlCategory.Value;

                        // Make sure the Category isn't in the exclusion list.
                        if (!setRemoveCategories.Contains(strInnerText) &&
                            // Also make sure it is not already in the Category list.
                            lstCategory.All(objItem => objItem.Value.ToString() != strInnerText))
                        {
                            lstCategory.Add(new ListItem(strInnerText,
                                                         (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))
                                                                       ?.Value ?? strInnerText));
                        }
                    }

                    lstCategory.Sort(CompareListItems.CompareNames);
                    string strOldSelected = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? _objCharacter.MetatypeCategory;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    await cboCategory.PopulateWithListItemsAsync(lstCategory, token);
                    _blnLoading = blnOldLoading;
                    await cboCategory.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        if (x.SelectedIndex == -1 && lstCategory.Count > 0)
                            x.SelectedIndex = 0;
                    }, token);
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

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Custom Methods
    }
}
