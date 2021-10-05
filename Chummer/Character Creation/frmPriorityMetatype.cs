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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class frmPriorityMetatype : Form
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

        public frmPriorityMetatype(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            if (string.IsNullOrEmpty(_objCharacter.SettingsKey))
                _objCharacter.SettingsKey = GlobalSettings.DefaultCharacterSetting;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _lstPrioritySkills = new List<string>(objCharacter.PriorityBonusSkillList);
            _xmlMetatypeDocumentMetatypesNode = _objCharacter.LoadData(strXmlFile).SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = _objCharacter.LoadDataXPath(strXmlFile).SelectSingleNode("/chummer");
            _xmlBasePriorityDataNode = _objCharacter.LoadDataXPath("priorities.xml").SelectSingleNode("/chummer");
            _xmlBaseSkillDataNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer");
            _xmlQualityDocumentQualitiesNode = _objCharacter.LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNode("/chummer");
            _xmlCritterPowerDocumentPowersNode = _objCharacter.LoadData("critterpowers.xml").SelectSingleNode("/chummer/powers");
            _dicSumtoTenValues = new Dictionary<string, int>(5);
            if (_xmlBasePriorityDataNode != null)
            {
                foreach (XPathNavigator xmlNode in _xmlBasePriorityDataNode.Select("priortysumtotenvalues/*"))
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

        private void frmPriorityMetatype_Load(object sender, EventArgs e)
        {
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                lblSumtoTen.Visible = true;
            }

            // Populate the Priority Category list.
            XPathNavigator xmlBasePrioritiesNode = _xmlBasePriorityDataNode.SelectSingleNode("priorities");
            if (xmlBasePrioritiesNode != null)
            {
                foreach (XPathNavigator objXmlPriorityCategory in _xmlBasePriorityDataNode.Select("categories/category"))
                {
                    XPathNodeIterator objItems = xmlBasePrioritiesNode.Select("priority[category = " + objXmlPriorityCategory.Value.CleanXPath() + " and prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + "]");

                    if (objItems.Count == 0)
                    {
                        objItems = xmlBasePrioritiesNode.Select("priority[category = " + objXmlPriorityCategory.Value.CleanXPath() + " and not(prioritytable)]");
                    }

                    if (objItems.Count > 0)
                    {
                        List<ListItem> lstItems = new List<ListItem>(objItems.Count);

                        foreach (XPathNavigator objXmlPriority in objItems)
                        {
                            string strValue = objXmlPriority.SelectSingleNode("value")?.Value;
                            if (!string.IsNullOrEmpty(strValue) && _lstPriorities.Contains(strValue))
                            {
                                lstItems.Add(new ListItem(
                                    strValue,
                                    objXmlPriority.SelectSingleNode("translate")?.Value ??
                                    objXmlPriority.SelectSingleNode("name")?.Value ??
                                    LanguageManager.GetString("String_Unknown")));
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
                RefreshSelectedMetatype();

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
                RefreshSelectedMetatype();
                if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                    SumToTen();
            }

            // Set up possession boxes
            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            chkPossessionBased.SetToolTip(LanguageManager.GetString("Tip_Metatype_PossessionTradition"));

            List<ListItem> lstMethods = new List<ListItem>(2)
            {
                new ListItem("Possession", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText ?? "Possession"),
                new ListItem("Inhabitation", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")?.InnerText ?? "Inhabitation")
            };

            lstMethods.Sort(CompareListItems.CompareNames);

            foreach (CritterPower objPower in _objCharacter.CritterPowers)
            {
                string strPowerName = objPower.Name;
                if (lstMethods.Any(x => strPowerName.Equals(x.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    _strCurrentPossessionMethod = strPowerName;
                    break;
                }
            }

            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.PopulateWithListItems(lstMethods);
            cboPossessionMethod.EndUpdate();

            _blnLoading = false;
        }

        #endregion Form Events

        #region Control Events

        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumToTen();
            }
            PopulateMetavariants();
            RefreshSelectedMetatype();
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
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("prioritytable") != null)
                    {
                        xmlTalentNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + strSelectedTalents.CleanXPath() + "]");
                        break;
                    }
                }

                if (xmlTalentNode != null)
                {
                    string strSkillCount = xmlTalentNode.SelectSingleNode("skillqty")?.Value ?? xmlTalentNode.SelectSingleNode("skillgroupqty")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSkillCount) && int.TryParse(strSkillCount, out int intSkillCount))
                    {
                        XPathNavigator xmlSkillTypeNode = xmlTalentNode.SelectSingleNode("skilltype") ?? xmlTalentNode.SelectSingleNode("skillgrouptype");
                        string strSkillType = xmlSkillTypeNode?.Value ?? string.Empty;
                        string strSkillVal = xmlTalentNode.SelectSingleNode("skillval")?.Value ?? xmlTalentNode.SelectSingleNode("skillgroupval")?.Value;
                        XPathNodeIterator objNodeList = xmlTalentNode.Select("skillgroupchoices/skillgroup");
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
                                xmlSkillsList = BuildSkillList(xmlTalentNode.Select("skillchoices/skill"));
                                break;

                            case "xpath":
                                xmlSkillsList = GetActiveSkillList(xmlSkillTypeNode?.SelectSingleNode("@xpath")?.Value);
                                strSkillType = "active";
                                break;

                            default:
                                xmlSkillsList = GetActiveSkillList();
                                break;
                        }

                        if (intSkillCount > 0)
                        {
                            List<ListItem> lstSkills = new List<ListItem>(intSkillCount);
                            if (objNodeList.Count > 0)
                            {
                                foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                {
                                    string strInnerText = objXmlSkill.Value;
                                    lstSkills.Add(new ListItem(strInnerText, objXmlSkill.SelectSingleNode("@translate")?.Value ?? strInnerText));
                                }
                            }
                            else
                            {
                                foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                {
                                    string strName = objXmlSkill.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown");
                                    lstSkills.Add(new ListItem(strName, objXmlSkill.SelectSingleNode("translate")?.Value ?? strName));
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
                                if (cboSkill2.SelectedIndex == cboSkill1.SelectedIndex && ExoticSkill.IsExoticSkillName(cboSkill2.SelectedValue?.ToString()))
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
                                        ExoticSkill.IsExoticSkillName(cboSkill3.SelectedValue?.ToString()))
                                    {
                                        int intNewIndex = cboSkill3.SelectedIndex;
                                        do
                                        {
                                            intNewIndex += 1;
                                            if (intNewIndex >= cboSkill3.Items.Count)
                                                intNewIndex = 0;
                                        }
                                        while ((intNewIndex == cboSkill1.SelectedIndex || intNewIndex == cboSkill2.SelectedIndex) && intNewIndex != cboSkill3.SelectedIndex &&
                                               !ExoticSkill.IsExoticSkillName(((ListItem)cboSkill3.Items[intNewIndex]).Value.ToString()));
                                        cboSkill3.SelectedIndex = intNewIndex;
                                    }
                                }
                            }
                            string strMetamagicSkillSelection = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_MetamagicSkillBase"),
                                LanguageManager.GetString("String_MetamagicSkills"));
                            // strSkillType can have the following values: magic, resonance, matrix, active, specific, grouped
                            // So the language file should contain each of those like String_MetamagicSkillType_magic
                            lblMetatypeSkillSelection.Text = string.Format(GlobalSettings.CultureInfo, strMetamagicSkillSelection, strSkillCount, LanguageManager.GetString("String_MetamagicSkillType_" + strSkillType), strSkillVal);
                            lblMetatypeSkillSelection.Visible = true;
                        }

                        int intSpecialAttribPoints = 0;

                        string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
                        string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();

                        if (!string.IsNullOrEmpty(strSelectedMetatype))
                        {
                            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + (cboHeritage.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                            {
                                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("prioritytable") != null)
                                {
                                    XPathNavigator objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + "]");
                                    if (!string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                                        objXmlMetatypePriorityNode = objXmlMetatypePriorityNode?.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + "]");
                                    if (int.TryParse(objXmlMetatypePriorityNode?.SelectSingleNode("value")?.Value, out int intTemp))
                                        intSpecialAttribPoints += intTemp;
                                    break;
                                }
                            }
                        }

                        if (int.TryParse(xmlTalentNode.SelectSingleNode("specialattribpoints")?.Value, out int intTalentSpecialAttribPoints))
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

        private void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            RefreshSelectedMetatype();
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

        private void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
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
            RefreshSelectedMetatype();
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
                using (frmSelectExoticSkill frmSelectExotic = new frmSelectExoticSkill(_objCharacter))
                {
                    frmSelectExotic.ForceSkill(strSkill1);
                    if (frmSelectExotic.ShowDialog(this) != DialogResult.OK)
                        return;
                    strSkill1 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }
            if (ExoticSkill.IsExoticSkillName(strSkill2))
            {
                using (frmSelectExoticSkill frmSelectExotic = new frmSelectExoticSkill(_objCharacter))
                {
                    frmSelectExotic.ForceSkill(strSkill2);
                    if (frmSelectExotic.ShowDialog(this) != DialogResult.OK)
                        return;
                    strSkill2 += " (" + frmSelectExotic.SelectedExoticSkillSpecialisation + ')';
                }
            }
            if (ExoticSkill.IsExoticSkillName(strSkill3))
            {
                using (frmSelectExoticSkill frmSelectExotic = new frmSelectExoticSkill(_objCharacter))
                {
                    frmSelectExotic.ForceSkill(strSkill3);
                    if (frmSelectExotic.ShowDialog(this) != DialogResult.OK)
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
                    XmlNode objXmlMetatype = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode("metatype[name = " + strSelectedMetatype.CleanXPath() + "]");
                    if (objXmlMetatype == null)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Clear out all priority-only qualities that the character bought normally (relevant when switching from Karma to Priority/Sum-to-Ten)
                    _objCharacter.Qualities.RemoveAll(x => x.OriginSource == QualitySource.Selected && x.GetNode()?["onlyprioritygiven"] != null);

                    string strSelectedMetavariant = cboMetavariant.SelectedValue.ToString();
                    string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                        strSelectedMetavariant = "Human";
                    XmlNode objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + "]");
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
                            XmlNode xmlRestrictionNode = objQuality.GetNode()?["required"];
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNode("//metatype") != null || xmlRestrictionNode.SelectSingleNode("//metavariant") != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode = objQuality.GetNode()?["forbidden"];
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNode("//metatype") != null || xmlRestrictionNode.SelectSingleNode("//metavariant") != null))
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
                            if (objQuality.GetNode()?.CreateNavigator().RequirementsMet(_objCharacter) == false)
                                _objCharacter.Qualities.Remove(objQuality);
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
                            (xmlResourcesPriority.SelectSingleNode("prioritytable") != null &&
                             xmlResourcesPriority.SelectSingleNode("gameplayoption") != null))
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
                            if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                            {
                                XPathNavigator xmlTalentPriorityNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + _objCharacter.TalentPriority.CleanXPath() + "]");

                                if (xmlTalentPriorityNode != null)
                                {
                                    // Create the Qualities that come with the Talent.
                                    foreach (XPathNavigator objXmlQualityItem in xmlTalentPriorityNode.Select("qualities/quality"))
                                    {
                                        XmlNode objXmlQuality = _xmlQualityDocumentQualitiesNode.SelectSingleNode("quality[name = " + objXmlQualityItem.Value.CleanXPath() + "]");
                                        Quality objQuality = new Quality(_objCharacter);
                                        string strForceValue = objXmlQualityItem.SelectSingleNode("@select")?.Value ?? string.Empty;
                                        objQuality.Create(objXmlQuality, QualitySource.Heritage, lstWeapons, strForceValue);
                                        Quality objExistingQuality = lstOldPriorityQualities.FirstOrDefault(x => x.SourceIDString == objQuality.SourceIDString && x.Extra == objQuality.Extra && x.Type == objQuality.Type);
                                        if (objExistingQuality != null)
                                            lstOldPriorityQualities.Remove(objExistingQuality);
                                        else
                                            _objCharacter.Qualities.Add(objQuality);
                                    }

                                    foreach (Quality objQuality in lstOldPriorityQualities)
                                    {
                                        ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                                        _objCharacter.Qualities.Remove(objQuality);
                                    }

                                    // Set starting magic
                                    int intTemp = 0;
                                    _objCharacter.MAG.MetatypeMinimum =
                                        xmlTalentPriorityNode.TryGetInt32FieldQuickly("magic", ref intTemp) ? intTemp : 1;
                                    _objCharacter.FreeSpells = xmlTalentPriorityNode.TryGetInt32FieldQuickly("spells", ref intTemp) ? intTemp : 0;
                                    _objCharacter.MAG.MetatypeMaximum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxmagic", ref intTemp)
                                        ? intTemp
                                        : CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intForce);
                                    // Set starting resonance
                                    _objCharacter.RES.MetatypeMinimum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("resonance", ref intTemp) ? intTemp : 1;
                                    _objCharacter.CFPLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("cfp", ref intTemp) ? intTemp : 0;
                                    _objCharacter.RES.MetatypeMaximum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxresonance", ref intTemp)
                                        ? intTemp
                                        : CommonFunctions.ExpressionToInt(charNode["resmax"]?.InnerText, intForce);
                                    // Set starting depth
                                    _objCharacter.DEP.MetatypeMinimum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("depth", ref intTemp) ? intTemp : 1;
                                    _objCharacter.AINormalProgramLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("ainormalprogramlimit", ref intTemp) ? intTemp : 0;
                                    _objCharacter.AIAdvancedProgramLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("aiadvancedprogramlimit", ref intTemp) ? intTemp : 0;
                                    _objCharacter.DEP.MetatypeMaximum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxdepth", ref intTemp)
                                        ? intTemp
                                        : CommonFunctions.ExpressionToInt(charNode["depmax"]?.InnerText, intForce);

                                    // Set Free Skills/Skill Groups
                                    int intFreeLevels = 0;
                                    Improvement.ImprovementType eType = Improvement.ImprovementType.SkillBase;
                                    XPathNavigator objTalentSkillValNode = xmlTalentPriorityNode.SelectSingleNode("skillval");
                                    if (objTalentSkillValNode == null || !int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                    {
                                        objTalentSkillValNode = xmlTalentPriorityNode.SelectSingleNode("skillgroupval");
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
                            (objXmlAttributesPriority.SelectSingleNode("prioritytable") != null &&
                             objXmlAttributesPriority.SelectSingleNode("gameplayoption") != null))
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
                            (objXmlSkillsPriority.SelectSingleNode("prioritytable") != null &&
                             objXmlSkillsPriority.SelectSingleNode("gameplayoption") != null))
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
            List<Improvement> lstOldFreeSkillImprovements = _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Heritage
                                                                                                  && x.ImproveType == type).ToList();
            if (intFreeLevels != 0)
            {
                bool blnCommit = false;
                if (!string.IsNullOrEmpty(strSkill1))
                {
                    Improvement objOldSkillImprovement = lstOldFreeSkillImprovements.FirstOrDefault(x => x.ImprovedName == strSkill1 && x.Value == intFreeLevels);
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
                    Improvement objOldSkillImprovement = lstOldFreeSkillImprovements.FirstOrDefault(x => x.ImprovedName == strSkill2 && x.Value == intFreeLevels);
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
                    Improvement objOldSkillImprovement = lstOldFreeSkillImprovements.FirstOrDefault(x => x.ImprovedName == strSkill3 && x.Value == intFreeLevels);
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
            if (lstCurrentPriorities.Count <= 0)
                return;
            string strComboBoxSelected = comboBox.SelectedValue.ToString();

            string strMissing = lstCurrentPriorities.First();

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
                if (lstCurrentPriorities.Count <= 0) // Just in case
                    return;

                strMissing = lstCurrentPriorities.First();

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

        private void RefreshSelectedMetatype()
        {
            string strSpace = LanguageManager.GetString("String_Space");
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
            string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

            XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + "]");
            XPathNavigator objXmlMetavariant = string.IsNullOrEmpty(strSelectedMetavariant) || strSelectedMetavariant == "None" ? null : objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + "]");
            XPathNavigator objXmlMetatypePriorityNode = null;
            XPathNavigator objXmlMetavariantPriorityNode = null;
            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
            {
                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("prioritytable") != null)
                {
                    objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + "]");
                    objXmlMetavariantPriorityNode = objXmlMetavariant != null ? objXmlMetatypePriorityNode.SelectSingleNode("metavariants/metavariant[name = " + strSelectedMetavariant.CleanXPath() + "]") : null;
                    break;
                }
            }

            string strAttributeFormat = "{0}/{1}" + strSpace + "({2})";
            if (objXmlMetavariant != null)
            {
                if (objXmlMetavariantPriorityNode == null)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("String_NotSupported"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmdOK.Enabled = false;
                }
                else
                {
                    cmdOK.Enabled = true;
                }

                lblBOD.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblAGI.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblREA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblSTR.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblCHA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblINT.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblLOG.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblWIL.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetavariant.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetavariant.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));

                lblMetavariantKarma.Text = objXmlMetavariantPriorityNode.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetavariant.SelectSingleNode("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetavariant.SelectSingleNode("altpage")?.Value ?? objXmlMetavariant.SelectSingleNode("page")?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                        lblSource.Text = objSource.ToString();
                        lblSource.SetToolTip(objSource.LanguageBookTooltip);
                    }
                    else
                    {
                        string strUnknown = LanguageManager.GetString("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                }
                else
                {
                    string strUnknown = LanguageManager.GetString("String_Unknown");
                    lblSource.Text = strUnknown;
                    lblSource.SetToolTip(strUnknown);
                }

                // Set the special attributes label.
                if (!int.TryParse(objXmlMetavariantPriorityNode?.SelectSingleNode("value")?.Value, NumberStyles.Any,
                    GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                    intSpecialAttribPoints = 0;

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("prioritytable") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + (cboTalents.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + "]");
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNode("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo);

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetavariant.Select("qualities/*/quality"))
                {
                    string strQuality;
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        strQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath() + "]/translate")?.Value ?? objXmlQuality.Value;

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + _objCharacter.TranslateExtra(strSelect) + ')';
                    }
                    else
                    {
                        strQuality = objXmlQuality.Value;
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + strSelect + ')';
                    }
                    if (dicQualities.ContainsKey(strQuality))
                    {
                        dicQualities[strQuality] += 1;
                    }
                    else
                        dicQualities.Add(strQuality, 1);
                }

                if (dicQualities.Count > 0)
                {
                    StringBuilder strQualities = new StringBuilder();
                    foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                    {
                        strQualities.Append(objLoopQuality.Key);
                        if (objLoopQuality.Value > 1)
                        {
                            strQualities.Append(strSpace);
                            strQualities.Append(objLoopQuality.Value.ToString(GlobalSettings.CultureInfo));
                        }
                        strQualities.Append(',' + strSpace);
                    }
                    strQualities.Length -= 2;
                    lblMetavariantQualities.Text = strQualities.ToString();
                }
                else
                {
                    lblMetavariantQualities.Text = LanguageManager.GetString("String_None");
                }
            }
            else if (objXmlMetatype != null)
            {
                cmdOK.Enabled = true;
                lblBOD.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblAGI.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblREA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblSTR.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblCHA.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblINT.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblLOG.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                lblWIL.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                    objXmlMetatype.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), objXmlMetatype.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metatype's Qualities.
                foreach (XPathNavigator xmlQuality in objXmlMetatype.Select("qualities/*/quality"))
                {
                    string strQuality;
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = " + xmlQuality.Value.CleanXPath() + "]");
                        strQuality = objQuality.SelectSingleNode("translate")?.Value ?? xmlQuality.Value;

                        string strSelect = xmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + _objCharacter.TranslateExtra(strSelect) + ')';
                    }
                    else
                    {
                        strQuality = xmlQuality.Value;
                        string strSelect = xmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpace + '(' + strSelect + ')';
                    }
                    if (dicQualities.ContainsKey(strQuality))
                    {
                        dicQualities[strQuality] += 1;
                    }
                    else
                        dicQualities.Add(strQuality, 1);
                }

                if (dicQualities.Count > 0)
                {
                    StringBuilder strQualities = new StringBuilder();
                    foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                    {
                        strQualities.Append(objLoopQuality.Key);
                        if (objLoopQuality.Value > 1)
                        {
                            strQualities.Append(strSpace);
                            strQualities.Append(objLoopQuality.Value.ToString(GlobalSettings.CultureInfo));
                        }
                        strQualities.Append(',' + strSpace);
                    }
                    strQualities.Length -= 2;
                    lblMetavariantQualities.Text = strQualities.ToString();
                }
                else
                {
                    lblMetavariantQualities.Text = LanguageManager.GetString("String_None");
                }

                lblMetavariantKarma.Text = objXmlMetatypePriorityNode.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetatype.SelectSingleNode("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetatype.SelectSingleNode("altpage")?.Value ?? objXmlMetatype.SelectSingleNode("page")?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                        lblSource.Text = objSource.ToString();
                        lblSource.SetToolTip(objSource.LanguageBookTooltip);
                    }
                    else
                    {
                        string strUnknown = LanguageManager.GetString("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                }
                else
                {
                    string strUnknown = LanguageManager.GetString("String_Unknown");
                    lblSource.Text = strUnknown;
                    lblSource.SetToolTip(strUnknown);
                }

                // Set the special attributes label.
                if (!int.TryParse(objXmlMetatypePriorityNode.SelectSingleNode("value")?.Value, NumberStyles.Any,
                    GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                    intSpecialAttribPoints = 0;

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("prioritytable") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + (cboTalents.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + "]");
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNode("specialattribpoints")?.Value, out int intTemp))
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
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("prioritytable") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = " + (cboTalents.SelectedValue?.ToString() ?? string.Empty).CleanXPath() + "]");
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNode("specialattribpoints")?.Value, out int intTemp))
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
            List<ListItem> lstTalent = new List<ListItem>(5);

            // Populate the Priority Category list.
            XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = " + (cboTalent.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
            foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
            {
                if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("prioritytable") != null)
                {
                    foreach (XPathNavigator objXmlPriorityTalent in xmlBaseTalentPriority.Select("talents/talent"))
                    {
                        XPathNavigator xmlQualitiesNode = objXmlPriorityTalent.SelectSingleNode("qualities");
                        if (xmlQualitiesNode != null)
                        {
                            bool blnFoundUnavailableQuality = false;

                            foreach (XPathNavigator xmlQuality in xmlQualitiesNode.Select("quality"))
                            {
                                if (_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[" + _objCharacter.Settings.BookXPath() + " and name = " + xmlQuality.Value.CleanXPath() + "]") == null)
                                {
                                    blnFoundUnavailableQuality = true;
                                    break;
                                }
                            }

                            if (blnFoundUnavailableQuality)
                                continue;
                        }
                        XPathNavigator xmlForbiddenNode = objXmlPriorityTalent.SelectSingleNode("forbidden");
                        if (xmlForbiddenNode != null)
                        {
                            bool blnRequirementForbidden = false;

                            // Loop through the oneof requirements.
                            XPathNodeIterator objXmlForbiddenList = xmlForbiddenNode.Select("oneof");
                            foreach (XPathNavigator objXmlOneOf in objXmlForbiddenList)
                            {
                                XPathNodeIterator objXmlOneOfList = objXmlOneOf.SelectChildren(XPathNodeType.Element);

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
                                        case "metavariant" when objXmlForbidden.Value == cboMetavariant.SelectedValue?.ToString():
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                    }
                                }
                            }
                        EndForbiddenLoop:
                            if (blnRequirementForbidden)
                                continue;
                        }
                        XPathNavigator xmlRequiredNode = objXmlPriorityTalent.SelectSingleNode("required");
                        if (xmlRequiredNode != null)
                        {
                            bool blnRequirementMet = false;

                            // Loop through the oneof requirements.
                            XPathNodeIterator objXmlForbiddenList = xmlRequiredNode.Select("oneof");
                            foreach (XPathNavigator objXmlOneOf in objXmlForbiddenList)
                            {
                                XPathNodeIterator objXmlOneOfList = objXmlOneOf.SelectChildren(XPathNodeType.Element);

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
                                        case "metavariant" when objXmlRequired.Value == cboMetavariant.SelectedValue?.ToString():
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                    }
                                }
                            }
                        EndRequiredLoop:
                            if (!blnRequirementMet)
                                continue;
                        }
                        lstTalent.Add(new ListItem(objXmlPriorityTalent.SelectSingleNode("value")?.Value,
                            objXmlPriorityTalent.SelectSingleNode("translate")?.Value ??
                            objXmlPriorityTalent.SelectSingleNode("name")?.Value ??
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

        private void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();

            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

                XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + "]");
                XPathNavigator objXmlMetatypeBP = null;
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("prioritytable") != null)
                    {
                        objXmlMetatypeBP = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strSelectedMetatype.CleanXPath() + "]");
                        break;
                    }
                }

                List<ListItem> lstMetavariants = new List<ListItem>(5)
                {
                    new ListItem("None", LanguageManager.GetString("String_None"))
                };

                if (objXmlMetatype != null && objXmlMetatypeBP != null)
                {
                    // Retrieve the list of Metavariants for the selected Metatype.
                    foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select("metavariants/metavariant[" + _objCharacter.Settings.BookXPath() + "]"))
                    {
                        string strName = objXmlMetavariant.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown");
                        lstMetavariants.Add(new ListItem(strName, objXmlMetavariant.SelectSingleNode("translate")?.Value ?? strName));
                    }

                    string strOldSelectedValue = cboMetavariant.SelectedValue?.ToString() ?? _objCharacter.Metavariant;
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
                    string strEssMax = objXmlMetatype.SelectSingleNode("essmax")?.Value ?? string.Empty;
                    int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                    if (objXmlMetatype.SelectSingleNode("forcecreature") != null || intPos != -1)
                    {
                        if (intPos != -1)
                        {
                            if (intPos > 0)
                            {
                                intPos -= 1;
                                lblForceLabel.Text = strEssMax.Substring(intPos, 3).Replace("D6", LanguageManager.GetString("String_D6"));
                                nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1), GlobalSettings.InvariantCultureInfo) * 6;
                            }
                            else
                            {
                                lblForceLabel.Text = 1.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_D6");
                                nudForce.Maximum = 6;
                            }
                        }
                        else
                        {
                            // TODO: Unhardcode whether Force is called "Force" or "Level"
                            lblForceLabel.Text = LanguageManager.GetString(objXmlMetatype.SelectSingleNode("bodmax")?.Value == "0" &&
                                                                           objXmlMetatype.SelectSingleNode("agimax")?.Value == "0" &&
                                                                           objXmlMetatype.SelectSingleNode("reamax")?.Value == "0" &&
                                                                           objXmlMetatype.SelectSingleNode("strmax")?.Value == "0" &&
                                                                           objXmlMetatype.SelectSingleNode("magmin")?.Value.Contains('F') != true
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
                else
                {
                    cboMetavariant.BeginUpdate();
                    cboMetavariant.PopulateWithListItems(lstMetavariants);
                    cboMetavariant.Enabled = false;
                    cboMetavariant.EndUpdate();

                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
            }
            else
            {
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>(5)
                {
                    new ListItem("None", LanguageManager.GetString("String_None"))
                };

                cboMetavariant.BeginUpdate();
                cboMetavariant.PopulateWithListItems(lstMetavariants);
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
                List<ListItem> lstMetatype = new List<ListItem>(10);

                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + (cboHeritage.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("prioritytable") != null)
                    {
                        foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select("metatypes/metatype[(" + _objCharacter.Settings.BookXPath() + ") and category = " + strSelectedCategory.CleanXPath() + "]"))
                        {
                            string strName = objXmlMetatype.SelectSingleNode("name")?.Value;
                            if (!string.IsNullOrEmpty(strName) && null != xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + strName.CleanXPath() + "]"))
                            {
                                lstMetatype.Add(new ListItem(strName, objXmlMetatype.SelectSingleNode("translate")?.Value ?? strName));
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
            List<ListItem> lstCategory = new List<ListItem>(3);

            // Populate the Metatype Category list.
            // Create a list of any Categories that should not be in the list.
            HashSet<string> lstRemoveCategory = new HashSet<string>();
            foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.Select("categories/category"))
            {
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = " + (cboHeritage.SelectedValue?.ToString() ?? string.Empty).CleanXPath()
                    + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                bool blnRemoveCategory = true;
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("prioritytable") != null)
                    {
                        foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select("metatypes/metatype[category = " + objXmlCategory.Value.CleanXPath() + " and (" + _objCharacter.Settings.BookXPath() + ")]"))
                        {
                            if (null != xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = " + (objXmlMetatype.SelectSingleNode("name")?.Value ?? string.Empty).CleanXPath() + "]"))
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
                    lstRemoveCategory.Add(objXmlCategory.Value);
            }

            foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.Select("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;

                // Make sure the Category isn't in the exclusion list.
                if (!lstRemoveCategory.Contains(strInnerText) &&
                    // Also make sure it is not already in the Category list.
                    lstCategory.All(objItem => objItem.Value.ToString() != strInnerText))
                {
                    lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
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

        private XPathNodeIterator GetMatrixSkillList()
        {
            return _xmlBaseSkillDataNode.Select("skills/skill[skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
        }

        private XPathNodeIterator GetMagicalSkillList()
        {
            return _xmlBaseSkillDataNode.Select("skills/skill[category = \"Magical Active\" or category = \"Pseudo-Magical Active\"]");
        }

        private XPathNodeIterator GetResonanceSkillList()
        {
            return _xmlBaseSkillDataNode.Select("skills/skill[category = \"Resonance Active\" or skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
        }

        private XPathNodeIterator GetActiveSkillList(string strXPathFilter = "")
        {
            return _xmlBaseSkillDataNode.Select(!string.IsNullOrEmpty(strXPathFilter)
                ? "skills/skill[" + strXPathFilter + ']'
                : "skills/skill");
        }

        private XPathNodeIterator BuildSkillCategoryList(XPathNodeIterator objSkillList)
        {
            StringBuilder sbdGroups = new StringBuilder("skillgroups/name");
            if (objSkillList.Count > 0)
            {
                sbdGroups.Append('[');
                foreach (XPathNavigator xmlSkillGroup in objSkillList)
                {
                    sbdGroups.Append(". = " + xmlSkillGroup.Value.CleanXPath() + " or ");
                }
                sbdGroups.Length -= 4;
                sbdGroups.Append(']');
            }

            return _xmlBaseSkillDataNode.Select(sbdGroups.ToString());
        }

        private XPathNodeIterator BuildSkillList(XPathNodeIterator objSkillList)
        {
            StringBuilder sbdGroups = new StringBuilder("skills/skill");
            if (objSkillList.Count > 0)
            {
                sbdGroups.Append('[');
                foreach (XPathNavigator xmlSkillGroup in objSkillList)
                {
                    sbdGroups.Append("name = " + xmlSkillGroup.Value.CleanXPath() + " or ");
                }
                sbdGroups.Length -= 4;
                sbdGroups.Append(']');
            }
            return _xmlBaseSkillDataNode.Select(sbdGroups.ToString());
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Custom Methods
    }
}
