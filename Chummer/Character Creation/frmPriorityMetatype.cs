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
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmPriorityMetatype : Form
    {
        private readonly Character _objCharacter;

        private int _intBuildMethod;
        private bool _blnLoading = true;
        private readonly List<string> _lstPrioritySkills;

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
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _lstPrioritySkills = new List<string>(objCharacter.PriorityBonusSkillList);
            XmlDocument xmlMetatypeDoc = XmlManager.Load(strXmlFile);
            _xmlMetatypeDocumentMetatypesNode = xmlMetatypeDoc.SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = xmlMetatypeDoc.GetFastNavigator().SelectSingleNode("/chummer");
            _xmlBasePriorityDataNode = XmlManager.Load("priorities.xml").GetFastNavigator().SelectSingleNode("/chummer");
            _xmlBaseSkillDataNode = XmlManager.Load("skills.xml").GetFastNavigator().SelectSingleNode("/chummer");
            XmlDocument xmlQualityDoc = XmlManager.Load("qualities.xml");
            _xmlQualityDocumentQualitiesNode = xmlQualityDoc.SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = xmlQualityDoc.GetFastNavigator().SelectSingleNode("/chummer");
            _xmlCritterPowerDocumentPowersNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers");
        }

        private void frmPriorityMetatype_Load(object sender, EventArgs e)
        {
            // Load the Priority information.
            if (string.IsNullOrEmpty(_objCharacter.GameplayOption))
                _objCharacter.GameplayOption = "Standard";

            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                _intBuildMethod = 1;
                lblSumtoTen.Visible = true;
            }
            List<string> objPriorities = new List<string>(5);
            if (_objCharacter.PriorityArray != string.Empty)
            {
                foreach (char c in _objCharacter.PriorityArray)
                {
                    switch (c)
                    {
                        case 'A':
                            objPriorities.Add("A,4");
                            break;
                        case 'B':
                            objPriorities.Add("B,3");
                            break;
                        case 'C':
                            objPriorities.Add("C,2");
                            break;
                        case 'D':
                            objPriorities.Add("D,1");
                            break;
                        case 'E':
                            objPriorities.Add("E,0");
                            break;
                    }
                }
            }
            else
            {
                objPriorities = new List<string> { "A,4", "B,3", "C,2", "D,1", "E,0" };
            }

            // Populate the Priority Category list.
            XPathNavigator xmlBasePrioritiesNode = _xmlBasePriorityDataNode.SelectSingleNode("priorities");
            if (xmlBasePrioritiesNode != null)
            {
                foreach (XPathNavigator objXmlPriorityCategory in _xmlBasePriorityDataNode.Select("categories/category"))
                {
                    XPathNodeIterator objItems = xmlBasePrioritiesNode.Select("priority[category = \"" + objXmlPriorityCategory.Value + "\" and gameplayoption = \"" + _objCharacter.GameplayOption + "\"]");

                    if (objItems.Count == 0)
                    {
                        objItems = xmlBasePrioritiesNode.Select("priority[category = \"" + objXmlPriorityCategory.Value + "\" and not(gameplayoption)]");
                    }

                    if (objItems.Count > 0)
                    {
                        List<ListItem> lstItems = new List<ListItem>();

                        foreach (string s in objPriorities)
                        {
                            foreach (XPathNavigator objXmlPriority in objItems)
                            {
                                if (objXmlPriority.SelectSingleNode("value")?.Value == s)
                                {
                                    lstItems.Add(new ListItem(
                                        objXmlPriority.SelectSingleNode("value")?.Value ?? string.Empty,
                                        objXmlPriority.SelectSingleNode("translate")?.Value ??
                                        objXmlPriority.SelectSingleNode("name")?.Value ??
                                        LanguageManager.GetString("String_Unknown", GlobalOptions.Language)));
                                    break;
                                }
                            }
                        }

                        lstItems.Sort(CompareListItems.CompareNames);
                        switch (objXmlPriorityCategory.Value)
                        {
                            case "Heritage":
                                cboHeritage.BeginUpdate();
                                cboHeritage.ValueMember = "Value";
                                cboHeritage.DisplayMember = "Name";
                                cboHeritage.DataSource = lstItems;
                                cboHeritage.EndUpdate();
                                break;
                            case "Talent":
                                cboTalent.BeginUpdate();
                                cboTalent.ValueMember = "Value";
                                cboTalent.DisplayMember = "Name";
                                cboTalent.DataSource = lstItems;
                                cboTalent.EndUpdate();
                                break;
                            case "Attributes":
                                cboAttributes.BeginUpdate();
                                cboAttributes.ValueMember = "Value";
                                cboAttributes.DisplayMember = "Name";
                                cboAttributes.DataSource = lstItems;
                                cboAttributes.EndUpdate();
                                break;
                            case "Skills":
                                cboSkills.BeginUpdate();
                                cboSkills.ValueMember = "Value";
                                cboSkills.DisplayMember = "Name";
                                cboSkills.DataSource = lstItems;
                                cboSkills.EndUpdate();
                                break;
                            case "Resources":
                                cboResources.BeginUpdate();
                                cboResources.ValueMember = "Value";
                                cboResources.DisplayMember = "Name";
                                cboResources.DataSource = lstItems;
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
                cboAttributes.SelectedIndex = cboAttributes.FindString(_objCharacter.AttributesPriority[0].ToString());
                //Heritage (Metatype)
                cboHeritage.SelectedIndex = cboHeritage.FindString(_objCharacter.MetatypePriority[0].ToString());
                //Resources
                cboResources.SelectedIndex = cboResources.FindString(_objCharacter.ResourcesPriority[0].ToString());
                //Skills
                cboSkills.SelectedIndex = cboSkills.FindString(_objCharacter.SkillsPriority[0].ToString());
                //Magical/Resonance Talent
                cboTalent.SelectedIndex = cboTalent.FindString(_objCharacter.SpecialPriority[0].ToString());

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
                    cboSkill1.SelectedValue = strSkill;
                }
                strSkill = _lstPrioritySkills.ElementAtOrDefault(1);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    cboSkill2.SelectedValue = strSkill;
                }
                strSkill = _lstPrioritySkills.ElementAtOrDefault(2);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    cboSkill3.SelectedValue = strSkill;
                }
                cboTalents_SelectedIndexChanged(null, EventArgs.Empty);
            }
            else
            {
                cboHeritage.SelectedIndex = 0;
                cboTalent.SelectedIndex = 1;
                cboAttributes.SelectedIndex = 2;
                cboSkills.SelectedIndex = 3;
                cboResources.SelectedIndex = 4;
                LoadMetatypes();
                PopulateMetatypes();
                PopulateMetavariants();
                PopulateTalents();
                RefreshSelectedMetatype();
            }

            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboHeritage);
                ManagePriorityItems(cboAttributes);
                ManagePriorityItems(cboTalent);
                ManagePriorityItems(cboSkills);
                ManagePriorityItems(cboResources);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }

            _blnLoading = false;
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
            PopulateMetavariants();
            RefreshSelectedMetatype();
        }

        private void lstMetatypes_DoubleClick(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cboTalents_SelectedIndexChanged(object sender, EventArgs e)
        {
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
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = \"" + (cboTalent.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        xmlTalentNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + strSelectedTalents + "\"]");
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
                                xmlSkillsList = GetActiveSkillList(xmlSkillTypeNode.SelectSingleNode("@xpath")?.Value);
                                strSkillType = "active";
                                break;
                            default:
                                xmlSkillsList = GetActiveSkillList();
                                break;
                        }

                        if (intSkillCount > 0)
                        {
                            List<ListItem> lstSkills = new List<ListItem>();
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
                                    string strName = objXmlSkill.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                                    lstSkills.Add(new ListItem(strName, objXmlSkill.SelectSingleNode("translate")?.Value ?? strName));
                                }
                            }
                            lstSkills.Sort(CompareListItems.CompareNames);
                            bool blnOldLoading = _blnLoading;
                            int intOldSelectedIndex = cboSkill1.SelectedIndex;
                            int intOldDataSourceSize = cboSkill1.Items.Count;
                            cboSkill1.ValueMember = "Value";
                            cboSkill1.DisplayMember = "Name";
                            cboSkill1.DataSource = lstSkills;
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
                                cboSkill2.BindingContext = new BindingContext();
                                cboSkill2.ValueMember = "Value";
                                cboSkill2.DisplayMember = "Name";
                                cboSkill2.DataSource = lstSkills;
                                cboSkill2.Visible = true;
                                if (intOldDataSourceSize == cboSkill2.Items.Count)
                                {
                                    _blnLoading = true;
                                    cboSkill2.SelectedIndex = intOldSelectedIndex;
                                    _blnLoading = blnOldLoading;
                                }
                                if (cboSkill2.SelectedIndex == cboSkill1.SelectedIndex)
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
                                    cboSkill3.BindingContext = new BindingContext();
                                    cboSkill3.ValueMember = "Value";
                                    cboSkill3.DisplayMember = "Name";
                                    cboSkill3.DataSource = lstSkills;
                                    cboSkill3.Visible = true;
                                    if (intOldDataSourceSize == cboSkill3.Items.Count)
                                    {
                                        _blnLoading = true;
                                        cboSkill3.SelectedIndex = intOldSelectedIndex;
                                        _blnLoading = blnOldLoading;
                                    }
                                    if (cboSkill3.SelectedIndex == cboSkill1.SelectedIndex || cboSkill3.SelectedIndex == cboSkill2.SelectedIndex)
                                    {
                                        int intNewIndex = cboSkill3.SelectedIndex;
                                        do
                                        {
                                            intNewIndex += 1;
                                            if (intNewIndex >= cboSkill3.Items.Count)
                                                intNewIndex = 0;
                                        }
                                        while ((intNewIndex == cboSkill1.SelectedIndex || intNewIndex == cboSkill2.SelectedIndex) && intNewIndex != cboSkill3.SelectedIndex);
                                        cboSkill3.SelectedIndex = intNewIndex;
                                    }
                                }
                            }
                            string strMetamagicSkillSelection = string.Format(LanguageManager.GetString("String_MetamagicSkillBase", GlobalOptions.Language),
                                LanguageManager.GetString("String_MetamagicSkills", GlobalOptions.Language));
                            // strSkillType can have the following values: magic, resonance, matrix, active, specific, grouped
                            // So the language file should contain each of those like String_MetamagicSkillType_magic
                            lblMetatypeSkillSelection.Text = string.Format(strMetamagicSkillSelection, strSkillCount, LanguageManager.GetString("String_MetamagicSkillType_"+strSkillType, GlobalOptions.Language), strSkillVal);
                            lblMetatypeSkillSelection.Visible = true;
                        }

                        int intSpecialAttribPoints = 0;

                        string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
                        string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();

                        if (!string.IsNullOrEmpty(strSelectedMetatype))
                        {
                            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = \"" + (cboHeritage.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                            {
                                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("gameplayoption") != null)
                                {
                                    XPathNavigator objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                                    if (!string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                                        objXmlMetatypePriorityNode = objXmlMetatypePriorityNode?.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
                                    if (int.TryParse(objXmlMetatypePriorityNode?.SelectSingleNode("value")?.Value, out int intTemp))
                                        intSpecialAttribPoints += intTemp;
                                    break;
                                }
                            }
                        }

                        if (int.TryParse(xmlTalentNode.SelectSingleNode("specialattribpoints")?.Value, out int intTalentSpecialAttribPoints))
                            intSpecialAttribPoints += intTalentSpecialAttribPoints;

                        lblSpecialAttributes.Text = intSpecialAttribPoints.ToString();
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
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
        }

        private void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            RefreshSelectedMetatype();
            PopulateTalents();
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
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
            PopulateMetatypes();
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
        }

        private void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboHeritage);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
            LoadMetatypes();
            PopulateMetatypes();
            PopulateMetavariants();
            RefreshSelectedMetatype();
        }

        private void cboTalent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboTalent);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
            PopulateTalents();
        }

        private void cboAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboAttributes);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
        }

        private void cboSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboSkills);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
        }

        private void cboResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboResources);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        void MetatypeSelected()
        {
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                int intSumToTen = SumtoTen(false);
                if (intSumToTen != _objCharacter.SumtoTen)
                {
                    Program.MainForm.ShowMessageBox(string.Format(LanguageManager.GetString("Message_SumtoTen", GlobalOptions.Language), _objCharacter.SumtoTen.ToString(GlobalOptions.CultureInfo), intSumToTen.ToString(GlobalOptions.CultureInfo)));
                    return;
                }
            }
            if (cboTalents.SelectedIndex == -1)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Metatype_SelectTalent", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectTalent", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSkill1 = cboSkill1.SelectedValue?.ToString();
            string strSkill2 = cboSkill2.SelectedValue?.ToString();
            string strSkill3 = cboSkill3.SelectedValue?.ToString();

            if ((cboSkill1.Visible && string.IsNullOrEmpty(strSkill1)) ||
                (cboSkill2.Visible && string.IsNullOrEmpty(strSkill2)) ||
                (cboSkill3.Visible && string.IsNullOrEmpty(strSkill3)))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Metatype_SelectSkill", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectSkill", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if ((cboSkill1.Visible && cboSkill2.Visible && strSkill1 == strSkill2) ||
                (cboSkill1.Visible && cboSkill3.Visible && strSkill1 == strSkill3) ||
                (cboSkill2.Visible && cboSkill3.Visible && strSkill2 == strSkill3))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Metatype_Duplicate", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_Duplicate", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;

            bool boolHalveAttributePriorityPoints = false;

            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedMetavariant = cboMetavariant.SelectedValue.ToString();
                string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                    strSelectedMetavariant = "Human";

                XmlNode objXmlMetatype = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode("metatype[name = \"" + strSelectedMetatype + "\"]");
                XmlNode objXmlMetavariant = objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");

                int intForce = nudForce.Visible ? decimal.ToInt32(nudForce.Value) : 0;

                int intMinModifier = 0;
                int intMaxModifier = 0;
                //TODO: What the hell is this for?
                /*if (_strXmlFile == "critters.xml")
                {
                    if (strSelectedMetatypeCategory == "Technocritters")
                    {
                        intMinModifier = -1;
                        intMaxModifier = 1;
                    }
                    else
                    {
                        intMinModifier = -3;
                        intMaxModifier = 3;
                    }
                }*/
                XmlNode charNode = strSelectedMetatypeCategory == "Shapeshifter" ? objXmlMetatype : objXmlMetavariant ?? objXmlMetatype;
                if (charNode == null)
                    return;
                _objCharacter.Create(strSelectedMetatypeCategory, objXmlMetatype["id"].InnerText, objXmlMetavariant["id"].InnerText, charNode, intForce, _xmlQualityDocumentQualitiesNode, _xmlCritterPowerDocumentPowersNode, XmlManager.Load("skills.xml").SelectSingleNode("/chummer/knowledgeskills"));
                
                // begin priority based character settings
                // Load the Priority information.

                // Set the character priority selections
                _objCharacter.MetatypeBP = Convert.ToInt32(lblMetavariantKarma.Text);
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
                XPathNodeIterator xmlResourcesPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Resources\" and value = \"" + _objCharacter.ResourcesPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlResourcesPriority in xmlResourcesPriorityList)
                {
                    if (xmlResourcesPriorityList.Count == 1 || xmlResourcesPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        decimal decResources = 0;
                        if (xmlResourcesPriority.TryGetDecFieldQuickly("resources", ref decResources))
                            _objCharacter.StartingNuyen = _objCharacter.Nuyen = decResources;
                        break;
                    }
                }


                if ("Aspected Magician".Equals(cboTalents.SelectedValue))
                {
                    _objCharacter.Pushtext.Push(strSkill1);
                }
                else if ("Enchanter".Equals(cboTalents.SelectedValue))
                {
                    _objCharacter.Pushtext.Push(strSkill1);
                }

                List<Weapon> lstWeapons = new List<Weapon>();

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = \"" + _objCharacter.SpecialPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        XPathNavigator xmlTalentPriorityNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + _objCharacter.TalentPriority + "\"]");

                        if (xmlTalentPriorityNode != null)
                        {
                            // Create the Qualities that come with the Talent.
                            foreach (XPathNavigator objXmlQualityItem in xmlTalentPriorityNode.Select("qualities/quality"))
                            {
                                XmlNode objXmlQuality = _xmlQualityDocumentQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlQualityItem.Value + "\"]");
                                Quality objQuality = new Quality(_objCharacter);
                                string strForceValue = objXmlQualityItem.SelectSingleNode("@select")?.Value ?? string.Empty;
                                QualitySource objSource = objXmlQualityItem.SelectSingleNode("@removable")?.Value == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
                                objQuality.Create(objXmlQuality, objSource, lstWeapons, strForceValue);
                                _objCharacter.Qualities.Add(objQuality);
                            }

                            // Set starting magic
                            int intTemp = 0;
                            _objCharacter.MAG.MetatypeMinimum =
                                xmlTalentPriorityNode.TryGetInt32FieldQuickly("magic", ref intTemp) ? intTemp : 1;
                            _objCharacter.FreeSpells = xmlTalentPriorityNode.TryGetInt32FieldQuickly("spells", ref intTemp) ? intTemp : 0;
                            _objCharacter.MAG.MetatypeMaximum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxmagic", ref intTemp) ? intTemp : Convert.ToInt32(CommonFunctions.ExpressionToString(charNode["magmax"]?.InnerText, intForce, intMaxModifier));
                            // Set starting resonance
                            _objCharacter.RES.MetatypeMinimum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("resonance", ref intTemp) ? intTemp : 1;
                            _objCharacter.CFPLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("cfp", ref intTemp) ? intTemp : 0;
                            _objCharacter.RES.MetatypeMaximum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxresonance", ref intTemp) ? intTemp : Convert.ToInt32(CommonFunctions.ExpressionToString(charNode["resmax"]?.InnerText, intForce, intMaxModifier));
                            // Set starting depth
                            _objCharacter.DEP.MetatypeMinimum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("depth", ref intTemp) ? intTemp : 1;
                            _objCharacter.AINormalProgramLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("ainormalprogramlimit", ref intTemp) ? intTemp : 0;
                            _objCharacter.AIAdvancedProgramLimit = xmlTalentPriorityNode.TryGetInt32FieldQuickly("aiadvancedprogramlimit", ref intTemp) ? intTemp : 0;
                            _objCharacter.DEP.MetatypeMaximum = xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxdepth", ref intTemp) ? intTemp : Convert.ToInt32(CommonFunctions.ExpressionToString(charNode["depmax"]?.InnerText, intForce, intMaxModifier));

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
                            AddFreeSkills(intFreeLevels, eType);
                        }
                        break;
                    }
                }

                // Set Special Attributes
                _objCharacter.Special = Convert.ToInt32(lblSpecialAttributes.Text);
                _objCharacter.TotalSpecial = _objCharacter.Special;

                // Set Attributes
                XPathNodeIterator objXmlAttributesPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Attributes\" and value = \"" + _objCharacter.AttributesPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator objXmlAttributesPriority in objXmlAttributesPriorityList)
                {
                    if (objXmlAttributesPriorityList.Count == 1 || objXmlAttributesPriority.SelectSingleNode("gameplayoption") != null)
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
                XPathNodeIterator objXmlSkillsPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Skills\" and value = \"" + _objCharacter.SkillsPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator objXmlSkillsPriority in objXmlSkillsPriorityList)
                {
                    if (objXmlSkillsPriorityList.Count == 1 || objXmlSkillsPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        int intTemp = 0;
                        if (objXmlSkillsPriority.TryGetInt32FieldQuickly("skills", ref intTemp))
                            _objCharacter.SkillsSection.SkillPointsMaximum = intTemp;
                        if (objXmlSkillsPriority.TryGetInt32FieldQuickly("skillgroups", ref intTemp))
                            _objCharacter.SkillsSection.SkillGroupPointsMaximum = intTemp;
                        break;
                    }
                }

                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in lstWeapons)
                    _objCharacter.Weapons.Add(objWeapon);

                // Sprites can never have Physical Attributes
                if (_objCharacter.DEPEnabled || strSelectedMetatype.EndsWith("Sprite"))
                {
                    _objCharacter.BOD.AssignLimits("0", "0", "0");
                    _objCharacter.AGI.AssignLimits("0", "0", "0");
                    _objCharacter.REA.AssignLimits("0", "0", "0");
                    _objCharacter.STR.AssignLimits("0", "0", "0");
                    _objCharacter.MAG.AssignLimits("0", "0", "0");
                    _objCharacter.MAGAdept.AssignLimits("0", "0", "0");
                }

                // Load the Priority information.
                XmlNode objXmlGameplayOption = XmlManager.Load("gameplayoptions.xml").SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + _objCharacter.GameplayOption + "\"]");
                if (objXmlGameplayOption != null)
                {
                    _objCharacter.MaxKarma = Convert.ToInt32(objXmlGameplayOption["karma"].InnerText);
                    _objCharacter.MaxNuyen = Convert.ToInt32(objXmlGameplayOption["maxnuyen"].InnerText);
                    _objCharacter.ContactMultiplier = Convert.ToInt32(objXmlGameplayOption["contactmultiplier"].InnerText);
                }

                // Set free contact points
                _objCharacter.OnPropertyChanged(nameof(Character.ContactPoints));

                // Set starting karma
                _objCharacter.BuildKarma = _objCharacter.MaxKarma;

                // Set limit for qualities
                _objCharacter.GameplayOptionQualityLimit = _objCharacter.MaxKarma;

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Metatype_SelectMetatype", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Cursor = Cursors.Default;
        }

        private void AddFreeSkills(int intFreeLevels, Improvement.ImprovementType type)
        {
            if (intFreeLevels != 0)
            {
                bool blnCommit = false;
                if (cboSkill1.Visible)
                {
                    string strSkill = cboSkill1.SelectedValue.ToString();
                    if (!string.IsNullOrEmpty(strSkill))
                    {
                        blnCommit = true;
                        ImprovementManager.CreateImprovement(_objCharacter, strSkill, Improvement.ImprovementSource.Heritage, string.Empty,
                            type, string.Empty, intFreeLevels);
                    }
                }

                if (cboSkill2.Visible)
                {
                    string strSkill = cboSkill2.SelectedValue.ToString();
                    if (!string.IsNullOrEmpty(strSkill))
                    {
                        blnCommit = true;
                        ImprovementManager.CreateImprovement(_objCharacter, strSkill, Improvement.ImprovementSource.Heritage, string.Empty,
                            type, string.Empty, intFreeLevels);
                    }
                }

                if (cboSkill3.Visible)
                {
                    string strSkill = cboSkill3.SelectedValue.ToString();
                    if (!string.IsNullOrEmpty(strSkill))
                    {
                        blnCommit = true;
                        ImprovementManager.CreateImprovement(_objCharacter, strSkill, Improvement.ImprovementSource.Heritage, string.Empty,
                            type, string.Empty, intFreeLevels);
                    }
                }

                if (blnCommit)
                    ImprovementManager.Commit(_objCharacter);
            }
        }

        /// <summary>
        /// Manages adjusting priority selections to prevent doubling up in Priority mode.
        /// </summary>
        /// <param name="comboBox"></param>
        private void ManagePriorityItems(ComboBox comboBox)
        {
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                List<string> objPriorities = new List<string>(5);
                if (_objCharacter.PriorityArray != string.Empty)
                {
                    foreach (char c in _objCharacter.PriorityArray)
                    {
                        switch (c)
                        {
                            case 'A':
                                objPriorities.Add("A,4");
                                break;
                            case 'B':
                                objPriorities.Add("B,3");
                                break;
                            case 'C':
                                objPriorities.Add("C,2");
                                break;
                            case 'D':
                                objPriorities.Add("D,1");
                                break;
                            case 'E':
                                objPriorities.Add("E,0");
                                break;
                        }
                    }
                }
                else
                {
                    objPriorities = new List<string> { "A,4", "B,3", "C,2", "D,1", "E,0" };
                }

                string strHeritageSelected = cboHeritage.SelectedValue.ToString();
                string strTalentSelected = cboTalent.SelectedValue.ToString();
                string strAttributesSelected = cboAttributes.SelectedValue.ToString();
                string strSkillsSelected = cboSkills.SelectedValue.ToString();
                string strResourcesSelected = cboResources.SelectedValue.ToString();

                // Discover which priority rating is not currently assigned
                objPriorities.Remove(strHeritageSelected);
                objPriorities.Remove(strTalentSelected);
                objPriorities.Remove(strAttributesSelected);
                objPriorities.Remove(strSkillsSelected);
                objPriorities.Remove(strResourcesSelected);
                if (objPriorities.Count == 0)
                    return;

                string strComboBoxSelected = comboBox.SelectedValue.ToString();

                string strMissing = objPriorities.First();

                // Find the combo with the same value as this one and change it to the missing value.
                //_blnInitializing = true;
                if (strTalentSelected == strComboBoxSelected && comboBox.Name != cboTalent.Name)
                    cboTalent.SelectedValue = strMissing;
                else if (strHeritageSelected == strComboBoxSelected && comboBox.Name != cboHeritage.Name)
                    cboHeritage.SelectedValue = strMissing;
                else if (strSkillsSelected == strComboBoxSelected && comboBox.Name != cboSkills.Name)
                    cboSkills.SelectedValue = strMissing;
                else if (strResourcesSelected == strComboBoxSelected && comboBox.Name != cboResources.Name)
                    cboResources.SelectedValue = strMissing;
                else if (strAttributesSelected == strComboBoxSelected && comboBox.Name != cboAttributes.Name)
                    cboAttributes.SelectedValue = strMissing;
            }
        }

        private int SumtoTen(bool blnDoUIUpdate = true)
        {
            int value = Convert.ToInt32(cboHeritage.SelectedValue.ToString().Split(',')[_intBuildMethod]);
            value += Convert.ToInt32(cboTalent.SelectedValue.ToString().Split(',')[_intBuildMethod]);
            value += Convert.ToInt32(cboAttributes.SelectedValue.ToString().Split(',')[_intBuildMethod]);
            value += Convert.ToInt32(cboSkills.SelectedValue.ToString().Split(',')[_intBuildMethod]);
            value += Convert.ToInt32(cboResources.SelectedValue.ToString().Split(',')[_intBuildMethod]);

            if (blnDoUIUpdate)
                lblSumtoTen.Text = value.ToString() + '/' + _objCharacter.SumtoTen.ToString();

            return value;
        }

        void RefreshSelectedMetatype()
        {
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
            string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

            XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
            XPathNavigator objXmlMetavariant = string.IsNullOrEmpty(strSelectedMetavariant) || strSelectedMetavariant == "None" ? null : objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
            XPathNavigator objXmlMetatypePriorityNode = null;
            XPathNavigator objXmlMetavariantPriorityNode = null;
            XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = \"" + strSelectedHeritage + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
            foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
            {
                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("gameplayoption") != null)
                {
                    objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                    objXmlMetavariantPriorityNode = objXmlMetavariant != null ? objXmlMetatypePriorityNode.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]") : null;
                    break;
                }
            }

            if (objXmlMetavariant != null)
            {
                if (objXmlMetavariantPriorityNode == null)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("String_NotSupported", GlobalOptions.Language), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmdOK.Enabled = false;
                }
                else
                {
                    cmdOK.Enabled = true;
                }

                lblBOD.Text = $"{objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblAGI.Text = $"{objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblREA.Text = $"{objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblSTR.Text = $"{objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblCHA.Text = $"{objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblINT.Text = $"{objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblLOG.Text = $"{objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblWIL.Text = $"{objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblINI.Text = $"{objXmlMetavariant.SelectSingleNode("inimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetavariant.SelectSingleNode("inimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetavariant.SelectSingleNode("iniaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";

                lblMetavariantKarma.Text = objXmlMetavariantPriorityNode.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);

                // Set the special attributes label.
                int.TryParse(objXmlMetavariantPriorityNode?.SelectSingleNode("value")?.Value, out int intSpecialAttribPoints);

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = \"" + (cboTalent.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + (cboTalents.SelectedValue?.ToString() ?? string.Empty) + "\"]");
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNode("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString();

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetavariant.Select("qualities/*/quality"))
                {
                    string strQuality;
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        strQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = \"" + objXmlQuality.Value + "\"]/translate")?.Value ?? objXmlQuality.Value;

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language) + ')';
                    }
                    else
                    {
                        strQuality = objXmlQuality.Value;
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpaceCharacter + '(' + strSelect + ')';
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
                            strQualities.Append(strSpaceCharacter);
                            strQualities.Append(objLoopQuality.Value.ToString(GlobalOptions.CultureInfo));
                        }
                        strQualities.Append(',' + strSpaceCharacter);
                    }
                    strQualities.Length -= 2;
                    lblMetavariantQualities.Text = strQualities.ToString();
                }
                else
                {
                    lblMetavariantQualities.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                }
            }
            else if (objXmlMetatype != null)
            {
                cmdOK.Enabled = true;
                lblBOD.Text = $"{objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblAGI.Text = $"{objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblREA.Text = $"{objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblSTR.Text = $"{objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblCHA.Text = $"{objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblINT.Text = $"{objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblLOG.Text = $"{objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblWIL.Text = $"{objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";
                lblINI.Text = $"{objXmlMetatype.SelectSingleNode("inimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}/{objXmlMetatype.SelectSingleNode("inimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}({objXmlMetatype.SelectSingleNode("iniaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo)})";

                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metatype's Qualities.
                foreach (XPathNavigator xmlQuality in objXmlMetatype.Select("qualities/*/quality"))
                {
                    string strQuality;
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XPathNavigator objQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = \"" + xmlQuality.Value + "\"]");
                        strQuality = objQuality.SelectSingleNode("translate")?.Value ?? xmlQuality.Value;

                        string strSelect = xmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language) + ')';
                    }
                    else
                    {
                        strQuality = xmlQuality.Value;
                        string strSelect = xmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += strSpaceCharacter + '(' + strSelect + ')';
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
                            strQualities.Append(strSpaceCharacter);
                            strQualities.Append(objLoopQuality.Value.ToString(GlobalOptions.CultureInfo));
                        }
                        strQualities.Append(',' + strSpaceCharacter);
                    }
                    strQualities.Length -= 2;
                    lblMetavariantQualities.Text = strQualities.ToString();
                }
                else
                {
                    lblMetavariantQualities.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                }

                lblMetavariantKarma.Text = objXmlMetatypePriorityNode.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                // Set the special attributes label.
                int.TryParse(objXmlMetatypePriorityNode.SelectSingleNode("value")?.Value, out int intSpecialAttribPoints);

                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = \"" + (cboTalent.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + (cboTalents.SelectedValue?.ToString() ?? string.Empty) + "\"]");
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNode("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString();
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
                lblINI.Text = string.Empty;

                int intSpecialAttribPoints = 0;
                XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = \"" + (cboTalent.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                    {
                        XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + (cboTalents.SelectedValue?.ToString() ?? string.Empty) + "\"]");
                        if (int.TryParse(objXmlTalentsNode?.SelectSingleNode("specialattribpoints")?.Value, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecialAttributes.Text = intSpecialAttribPoints.ToString();

                lblMetavariantQualities.Text = string.Empty;
                lblMetavariantKarma.Text = string.Empty;
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
            lblINILabel.Visible = !string.IsNullOrEmpty(lblINI.Text);
            lblSpecialAttributesLabel.Visible = !string.IsNullOrEmpty(lblSpecialAttributes.Text);
            lblMetavariantQualitiesLabel.Visible = !string.IsNullOrEmpty(lblMetavariantQualities.Text);
            lblMetavariantKarmaLabel.Visible = !string.IsNullOrEmpty(lblMetavariantKarma.Text);
        }

        void PopulateTalents()
        {
            // Load the Priority information.
            List<ListItem> lstTalent = new List<ListItem>();

            // Populate the Priority Category list.
            XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Talent\" and value = \"" + (cboTalent.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
            foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
            {
                if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority.SelectSingleNode("gameplayoption") != null)
                {
                    foreach (XPathNavigator objXmlPriorityTalent in xmlBaseTalentPriority.Select("talents/talent"))
                    {
                        XPathNavigator xmlQualitiesNode = objXmlPriorityTalent.SelectSingleNode("qualities");
                        if (xmlQualitiesNode != null)
                        {
                            bool blnFoundUnavailableQuality = false;

                            foreach (XPathNavigator xmlQuality in xmlQualitiesNode.Select("quality"))
                            {
                                if (_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[" + _objCharacter.Options.BookXPath() + " and name = \"" + xmlQuality.Value + "\"]") == null)
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
                                    if (objXmlForbidden.Name == "metatype")
                                    {
                                        // Check the Metatype restriction.
                                        if (objXmlForbidden.Value == lstMetatypes.SelectedValue?.ToString())
                                        {
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                        }
                                    }
                                    else if (objXmlForbidden.Name == "metatypecategory")
                                    {
                                        // Check the Metatype Category restriction.
                                        if (objXmlForbidden.Value == cboCategory.SelectedValue?.ToString())
                                        {
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                        }
                                    }
                                    else if (objXmlForbidden.Name == "metavariant")
                                    {
                                        // Check the Metavariant restriction.
                                        if (objXmlForbidden.Value == cboMetavariant.SelectedValue?.ToString())
                                        {
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                        }
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
                                    if (objXmlRequired.Name == "metatype")
                                    {
                                        // Check the Metatype restriction.
                                        if (objXmlRequired.Value == lstMetatypes.SelectedValue?.ToString())
                                        {
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                        }
                                    }
                                    else if (objXmlRequired.Name == "metatypecategory")
                                    {
                                        // Check the Metatype Category restriction.
                                        if (objXmlRequired.Value == cboCategory.SelectedValue?.ToString())
                                        {
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                        }
                                    }
                                    else if (objXmlRequired.Name == "metavariant")
                                    {
                                        // Check the Metavariant restriction.
                                        if (objXmlRequired.Value == cboMetavariant.SelectedValue?.ToString())
                                        {
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                        }
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
                            LanguageManager.GetString("String_Unknown", GlobalOptions.Language)));
                    }
                    break;
                }
            }

            lstTalent.Sort(CompareListItems.CompareNames);
            int intOldSelectedIndex = cboTalents.SelectedIndex;
            int intOldDataSourceSize = cboTalents.Items.Count;
            cboTalents.BeginUpdate();
            cboTalents.DataSource = null;
            cboTalents.ValueMember = "Value";
            cboTalents.DisplayMember = "Name";
            cboTalents.DataSource = lstTalent;
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

        void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();

            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

                XPathNavigator objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                XPathNavigator objXmlMetatypeBP = null;
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = \"" + strSelectedHeritage + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("gameplayoption") != null)
                    {
                        objXmlMetatypeBP = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                        break;
                    }
                }

                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                if (objXmlMetatype != null && objXmlMetatypeBP != null)
                {
                    // Retrieve the list of Metavariants for the selected Metatype.
                    foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]"))
                    {
                        string strName = objXmlMetavariant.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                        lstMetavariants.Add(new ListItem(strName, objXmlMetavariant.SelectSingleNode("translate")?.Value ?? strName));
                    }

                    string strOldSelectedValue = cboMetavariant.SelectedValue?.ToString() ?? _objCharacter.Metavariant;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    cboMetavariant.BeginUpdate();
                    cboMetavariant.ValueMember = "Value";
                    cboMetavariant.DisplayMember = "Name";
                    cboMetavariant.DataSource = lstMetavariants;
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
                        lblForceLabel.Visible = true;
                        nudForce.Visible = true;

                        if (intPos != -1)
                        {
                            if (intPos > 0)
                            {
                                intPos -= 1;
                                lblForceLabel.Text = strEssMax.Substring(intPos, 3).Replace("D6", LanguageManager.GetString("String_D6", GlobalOptions.Language));
                                nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1)) * 6;
                            }
                            else
                            {
                                lblForceLabel.Text = 1.ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_D6", GlobalOptions.Language);
                                nudForce.Maximum = 6;
                            }
                        }
                        else
                        {
                            lblForceLabel.Text = LanguageManager.GetString("String_Force", GlobalOptions.Language);
                            nudForce.Maximum = 100;
                        }
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
                    cboMetavariant.ValueMember = "Value";
                    cboMetavariant.DisplayMember = "Name";
                    cboMetavariant.DataSource = lstMetavariants;
                    cboMetavariant.Enabled = false;
                    cboMetavariant.EndUpdate();

                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
            }
            else
            {
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = false;
                cboMetavariant.EndUpdate();

                lblForceLabel.Visible = false;
                nudForce.Visible = false;
            }
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        void PopulateMetatypes()
        {
            string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatypeCategory))
            {
                List<ListItem> lstMetatype = new List<ListItem>();

                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = \"" + (cboHeritage.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("gameplayoption") != null)
                    {
                        foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select("metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + strSelectedMetatypeCategory + "\"]"))
                        {
                            string strName = objXmlMetatype.SelectSingleNode("name")?.Value;
                            if (!string.IsNullOrEmpty(strName) && null != xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strName + "\"]"))
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
                lstMetatypes.ValueMember = "Value";
                lstMetatypes.DisplayMember = "Name";
                lstMetatypes.DataSource = lstMetatype;
                _blnLoading = blnOldLoading;
                if (!string.IsNullOrEmpty(strOldSelectedValue))
                    lstMetatypes.SelectedValue = strOldSelectedValue;
                if (lstMetatypes.SelectedIndex == -1 && lstMetatype.Count > 0)
                    lstMetatypes.SelectedIndex = 0;
                lstMetatypes.EndUpdate();
            }
        }

        private void LoadMetatypes()
        {
            List<ListItem> lstCategory = new List<ListItem>();

            // Populate the Metatype Category list.
            // Create a list of any Categories that should not be in the list.
            HashSet<string> lstRemoveCategory = new HashSet<string>();
            foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.Select("categories/category"))
            {
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select("priorities/priority[category = \"Heritage\" and value = \"" + (cboHeritage.SelectedValue?.ToString() ?? string.Empty) + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority.SelectSingleNode("gameplayoption") != null)
                    {
                        foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select("metatypes/metatype[category = \"" + objXmlCategory.Value + "\" and (" + _objCharacter.Options.BookXPath() + ")]"))
                        {
                            if (null != xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + objXmlMetatype.SelectSingleNode("name")?.Value + "\"]"))
                            {
                                goto NextItem;
                            }
                        }
                        break;
                    }
                }
                // Remove metatypes not covered by heritage
                lstRemoveCategory.Add(objXmlCategory.Value);
                NextItem:;
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
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstCategory;
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
            if (string.IsNullOrEmpty(strXPathFilter))
                return _xmlBaseSkillDataNode.Select("skills/skill");
            return _xmlBaseSkillDataNode.Select("skills/skill[" + strXPathFilter + ']');
        }

        private XPathNodeIterator BuildSkillCategoryList(XPathNodeIterator objSkillList)
        {
            StringBuilder strGroups = new StringBuilder("skillgroups/name");
            if (objSkillList.Count > 0)
            {
                strGroups.Append('[');

                foreach (XPathNavigator xmlSkillGroup in objSkillList)
                {
                    strGroups.Append(". = \"");
                    strGroups.Append(xmlSkillGroup.Value);
                    strGroups.Append("\" or ");
                }

                strGroups.Length -= 4;

                strGroups.Append(']');
            }

            return _xmlBaseSkillDataNode.Select(strGroups.ToString());
        }

        private XPathNodeIterator BuildSkillList(XPathNodeIterator objSkillList)
        {
            StringBuilder strGroups = new StringBuilder("skills/skill");
            if (objSkillList.Count > 0)
            {
                strGroups.Append('[');

                foreach (XPathNavigator xmlSkillGroup in objSkillList)
                {
                    strGroups.Append("name = \"");
                    strGroups.Append(xmlSkillGroup.Value);
                    strGroups.Append("\" or ");
                }

                strGroups.Length -= 4;

                strGroups.Append(']');
            }
            return _xmlBaseSkillDataNode.Select(strGroups.ToString());
        }
        #endregion
    }
}

