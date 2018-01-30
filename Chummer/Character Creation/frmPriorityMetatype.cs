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
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class frmPriorityMetatype : Form
    {
        private readonly Character _objCharacter;

        private readonly string _strXmlFile = "metatypes.xml";
        private int _intBuildMethod = 0;
        private bool _blnInitializing = true;
        private readonly List<string> _lstPrioritySkills = null;

        #region Character Events
        private void DoNothing(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }
        #endregion
        
        #region Form Events
        public frmPriorityMetatype(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter;
            _strXmlFile = strXmlFile;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            // Attach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged += DoNothing;
            _objCharacter.RESEnabledChanged += DoNothing;
            _objCharacter.DEPEnabledChanged += DoNothing;
            _objCharacter.AdeptTabEnabledChanged += DoNothing;
            _objCharacter.MagicianTabEnabledChanged += DoNothing;
            _objCharacter.TechnomancerTabEnabledChanged += DoNothing;
            _objCharacter.AdvancedProgramsTabEnabledChanged += DoNothing;
            _objCharacter.CyberwareTabDisabledChanged += DoNothing;
            _objCharacter.InitiationTabEnabledChanged += DoNothing;
            _objCharacter.CritterTabEnabledChanged += DoNothing;

            _lstPrioritySkills = new List<string>(objCharacter?.PriorityBonusSkillList);

            Height = cmdOK.Bottom + 40;
            lstMetatypes.Height = cmdOK.Bottom - lstMetatypes.Top;
        }

        private void frmPriorityMetatype_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Detach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged -= DoNothing;
            _objCharacter.RESEnabledChanged -= DoNothing;
            _objCharacter.DEPEnabledChanged -= DoNothing;
            _objCharacter.AdeptTabEnabledChanged -= DoNothing;
            _objCharacter.MagicianTabEnabledChanged -= DoNothing;
            _objCharacter.TechnomancerTabEnabledChanged -= DoNothing;
            _objCharacter.AdvancedProgramsTabEnabledChanged -= DoNothing;
            _objCharacter.CyberwareTabDisabledChanged -= DoNothing;
            _objCharacter.InitiationTabEnabledChanged -= DoNothing;
            _objCharacter.CritterTabEnabledChanged -= DoNothing;
        }

        private void frmPriorityMetatype_Load(object sender, EventArgs e)
        {
            // Load the Priority information.
            XmlDocument objXmlDocumentPriority = XmlManager.Load("priorities.xml");
            if (string.IsNullOrEmpty(_objCharacter.GameplayOption))
                _objCharacter.GameplayOption = "Standard";

            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                _intBuildMethod = 1;
                lblSumtoTen.Visible = true;
            }
            // Populate the Priority Category list.
            XmlNode xmlBasePrioritiesNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities");
            foreach (XmlNode objXmlPriorityCategory in objXmlDocumentPriority.SelectNodes("/chummer/categories/category"))
            {
                XmlNodeList objItems = xmlBasePrioritiesNode.SelectNodes("priority[category = \"" + objXmlPriorityCategory.InnerText + "\" and gameplayoption = \"" + _objCharacter.GameplayOption + "\"]");

                if (objItems.Count == 0)
                {
                    objItems = xmlBasePrioritiesNode.SelectNodes("priority[category = \"" + objXmlPriorityCategory.InnerText + "\" and not(gameplayoption)]");
                }

                if (objItems.Count > 0)
                {
                    List<ListItem> lstItems = new List<ListItem>();
                    // lstItems.Add(new ListItem());
                    foreach (XmlNode objXmlPriority in objItems)
                    {
                        lstItems.Add(new ListItem(objXmlPriority["value"].InnerText, objXmlPriority["translate"]?.InnerText ?? objXmlPriority["name"].InnerText));
                    }
                    lstItems.Sort(CompareListItems.CompareNames);
                    switch (objXmlPriorityCategory.InnerText)
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
                        default:
                            break;
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
                else
                    cboSkill1.Visible = false;
                strSkill = _lstPrioritySkills.ElementAtOrDefault(1);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    cboSkill2.SelectedValue = strSkill;
                }
                else
                    cboSkill2.Visible = false;
                strSkill = _lstPrioritySkills.ElementAtOrDefault(2);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    cboSkill3.SelectedValue = strSkill;
                }
                else
                    cboSkill3.Visible = false;
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


            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            tipTooltip.SetToolTip(chkPossessionBased, LanguageManager.GetString("Tip_Metatype_PossessionTradition", GlobalOptions.Language));
            tipTooltip.SetToolTip(chkBloodSpirit, LanguageManager.GetString("Tip_Metatype_BloodSpirit", GlobalOptions.Language));
            
            XmlNode objXmlPowersNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers");
            List<ListItem> lstMethods = new List<ListItem>
            {
                new ListItem("Possession", objXmlPowersNode?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText ?? "Possession"),
                new ListItem("Inhabitation", objXmlPowersNode?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")?.InnerText ?? "Inhabitation")
            };
            
            lstMethods.Sort(CompareListItems.CompareNames);
            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.ValueMember = "Value";
            cboPossessionMethod.DisplayMember = "Name";
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.EndUpdate();

            _blnInitializing = false;
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;
            PopulateMetavariants();
            PopulateTalents();
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
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
                XmlDocument objXmlDocumentPriority = XmlManager.Load("priorities.xml");
                XmlNode objXmlTalentNode = null;
                XmlNodeList xmlBaseTalentPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue?.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority["gameplayoption"] != null)
                    {
                        objXmlTalentNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + strSelectedTalents + "\"]");
                        break;
                    }
                }
                
                if (objXmlTalentNode != null)
                {
                    string strSkillCount = objXmlTalentNode.SelectSingleNode("skillqty")?.InnerText ?? objXmlTalentNode.SelectSingleNode("skillgroupqty")?.InnerText ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSkillCount) && int.TryParse(strSkillCount, out int intSkillCount))
                    {
                        string strSkillType = objXmlTalentNode.SelectSingleNode("skilltype")?.InnerText ?? objXmlTalentNode.SelectSingleNode("skillgrouptype")?.InnerText;
                        string strSkillVal = objXmlTalentNode.SelectSingleNode("skillval")?.InnerText ?? objXmlTalentNode.SelectSingleNode("skillgroupval")?.InnerText;
                        XmlNodeList objNodeList = objXmlTalentNode.SelectNodes("skillgroupchoices/skillgroup");
                        XmlNodeList objXmlSkillsList;
                        switch (strSkillType)
                        {
                            case "magic":
                                objXmlSkillsList = GetMagicalSkillList();
                                break;
                            case "resonance":
                                objXmlSkillsList = GetResonanceSkillList();
                                break;
                            case "matrix":
                                objXmlSkillsList = GetMatrixSkillList();
                                break;
                            case "grouped":
                                objXmlSkillsList = BuildSkillCategoryList(objNodeList);
                                break;
                            case "specific":
                                objXmlSkillsList = BuildSkillList(objXmlTalentNode.SelectNodes("skillchoices/skill"));
                                break;
                            default:
                                objXmlSkillsList = GetActiveSkillList();
                                break;
                        }

                        if (intSkillCount > 0)
                        {
                            List<ListItem> lstSkills = new List<ListItem>();
                            if (objNodeList.Count > 0)
                            {
                                foreach (XmlNode objXmlSkill in objXmlSkillsList)
                                {
                                    string strInnerText = objXmlSkill.InnerText;
                                    lstSkills.Add(new ListItem(strInnerText, objXmlSkill.Attributes["translate"]?.InnerText ?? strInnerText));
                                }
                            }
                            else
                            {
                                foreach (XmlNode objXmlSkill in objXmlSkillsList)
                                {
                                    string strName = objXmlSkill["name"]?.InnerText ?? string.Empty;
                                    lstSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
                                }
                            }
                            lstSkills.Sort(CompareListItems.CompareNames);
                            bool blnOldInitializing = _blnInitializing;
                            int intOldSelectedIndex = cboSkill1.SelectedIndex;
                            int intOldDataSourceSize = cboSkill1.Items.Count;
                            cboSkill1.ValueMember = "Value";
                            cboSkill1.DisplayMember = "Name";
                            cboSkill1.DataSource = lstSkills;
                            cboSkill1.Visible = true;
                            if (intOldDataSourceSize == cboSkill1.Items.Count)
                            {
                                _blnInitializing = true;
                                cboSkill1.SelectedIndex = intOldSelectedIndex;
                                _blnInitializing = blnOldInitializing;
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
                                    _blnInitializing = true;
                                    cboSkill2.SelectedIndex = intOldSelectedIndex;
                                    _blnInitializing = blnOldInitializing;
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
                                        _blnInitializing = true;
                                        cboSkill3.SelectedIndex = intOldSelectedIndex;
                                        _blnInitializing = blnOldInitializing;
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
                            XmlNodeList xmlBaseMetatypePriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue?.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                            foreach (XmlNode xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                            {
                                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority["gameplayoption"] != null)
                                {
                                    XmlNode objXmlMetatypePriorityNode = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                                    if (!string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                                        objXmlMetatypePriorityNode = objXmlMetatypePriorityNode.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
                                    if (int.TryParse(objXmlMetatypePriorityNode?["value"]?.InnerText, out int intTemp))
                                        intSpecialAttribPoints += intTemp;
                                    break;
                                }
                            }
                        }
                        
                        if (int.TryParse(objXmlTalentNode["specialattribpoints"]?.InnerText, out int intTalentSpecialAttribPoints))
                            intSpecialAttribPoints += intTalentSpecialAttribPoints;

                        lblSpecial.Text = intSpecialAttribPoints.ToString();
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
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
            XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");
            XmlDocument objXmlDocumentPriority = XmlManager.Load("priorities.xml");

            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
            string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();

            XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
            XmlNode objXmlMetavariant = string.IsNullOrEmpty(strSelectedMetavariant) || strSelectedMetavariant == "None" ? null : objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
            XmlNode objXmlMetatypePriorityNode = null;
            XmlNode objXmlMetavariantPriorityNode = null;
            XmlNodeList xmlBaseMetatypePriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + strSelectedHeritage + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
            foreach (XmlNode xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
            {
                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority["gameplayoption"] != null)
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
                    MessageBox.Show(LanguageManager.GetString("String_NotSupported", GlobalOptions.Language), "Chummer5", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmdOK.Enabled = false;
                }
                else
                {
                    cmdOK.Enabled = true;
                }

                lblBOD.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["bodmin"].InnerText, objXmlMetavariant["bodmax"].InnerText, objXmlMetavariant["bodaug"].InnerText);
                lblAGI.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["agimin"].InnerText, objXmlMetavariant["agimax"].InnerText, objXmlMetavariant["agiaug"].InnerText);
                lblREA.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["reamin"].InnerText, objXmlMetavariant["reamax"].InnerText, objXmlMetavariant["reaaug"].InnerText);
                lblSTR.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["strmin"].InnerText, objXmlMetavariant["strmax"].InnerText, objXmlMetavariant["straug"].InnerText);
                lblCHA.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["chamin"].InnerText, objXmlMetavariant["chamax"].InnerText, objXmlMetavariant["chaaug"].InnerText);
                lblINT.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["intmin"].InnerText, objXmlMetavariant["intmax"].InnerText, objXmlMetavariant["intaug"].InnerText);
                lblLOG.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["logmin"].InnerText, objXmlMetavariant["logmax"].InnerText, objXmlMetavariant["logaug"].InnerText);
                lblWIL.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["wilmin"].InnerText, objXmlMetavariant["wilmax"].InnerText, objXmlMetavariant["wilaug"].InnerText);
                lblINI.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["inimin"].InnerText, objXmlMetavariant["inimax"].InnerText, objXmlMetavariant["iniaug"].InnerText);

                lblMetavariantBP.Text = objXmlMetavariantPriorityNode["karma"]?.InnerText ?? "0";

                // Set the special attributes label.
                int.TryParse(objXmlMetavariantPriorityNode?["value"]?.InnerText, out int intSpecialAttribPoints);

                XmlNodeList xmlBaseTalentPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue?.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority["gameplayoption"] != null)
                    {
                        XmlNode objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + cboTalents.SelectedValue?.ToString() + "\"]");
                        if (int.TryParse(objXmlTalentsNode?["specialattribpoints"]?.InnerText, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecial.Text = intSpecialAttribPoints.ToString();

                string strQuality = string.Empty;
                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metavariant's Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strQuality = objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += " (" + LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language) + ')';
                    }
                    else
                    {
                        strQuality = objXmlQuality.InnerText;
                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += " (" + strSelect + ')';
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
                            strQualities.Append(' ');
                            strQualities.Append(objLoopQuality.Value.ToString());
                        }
                        strQualities.Append(", ");
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
                lblBOD.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["bodmin"].InnerText, objXmlMetatype["bodmax"].InnerText, objXmlMetatype["bodaug"].InnerText);
                lblAGI.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["agimin"].InnerText, objXmlMetatype["agimax"].InnerText, objXmlMetatype["agiaug"].InnerText);
                lblREA.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["reamin"].InnerText, objXmlMetatype["reamax"].InnerText, objXmlMetatype["reaaug"].InnerText);
                lblSTR.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["strmin"].InnerText, objXmlMetatype["strmax"].InnerText, objXmlMetatype["straug"].InnerText);
                lblCHA.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["chamin"].InnerText, objXmlMetatype["chamax"].InnerText, objXmlMetatype["chaaug"].InnerText);
                lblINT.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["intmin"].InnerText, objXmlMetatype["intmax"].InnerText, objXmlMetatype["intaug"].InnerText);
                lblLOG.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["logmin"].InnerText, objXmlMetatype["logmax"].InnerText, objXmlMetatype["logaug"].InnerText);
                lblWIL.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["wilmin"].InnerText, objXmlMetatype["wilmax"].InnerText, objXmlMetatype["wilaug"].InnerText);
                lblINI.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["inimin"].InnerText, objXmlMetatype["inimax"].InnerText, objXmlMetatype["iniaug"].InnerText);

                string strQuality = string.Empty;
                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metatype's Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strQuality = objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += " (" + LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language) + ')';
                    }
                    else
                    {
                        strQuality = objXmlQuality.InnerText;
                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                            strQuality += " (" + strSelect + ')';
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
                            strQualities.Append(' ');
                            strQualities.Append(objLoopQuality.Value.ToString());
                        }
                        strQualities.Append(", ");
                    }
                    strQualities.Length -= 2;
                    lblMetavariantQualities.Text = strQualities.ToString();
                }
                else
                {
                    lblMetavariantQualities.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                }

                lblMetavariantBP.Text = objXmlMetatypePriorityNode["karma"]?.InnerText;
                // Set the special attributes label.
                int.TryParse(objXmlMetatypePriorityNode["value"]?.InnerText, out int intSpecialAttribPoints);

                XmlNodeList xmlBaseTalentPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue?.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority["gameplayoption"] != null)
                    {
                        XmlNode objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + cboTalents.SelectedValue?.ToString() + "\"]");
                        if (int.TryParse(objXmlTalentsNode?["specialattribpoints"]?.InnerText, out int intTemp))
                            intSpecialAttribPoints += intTemp;
                        break;
                    }
                }

                lblSpecial.Text = intSpecialAttribPoints.ToString();
            }
            else
                cmdOK.Enabled = false;
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
            if (_blnInitializing)
                return;
            PopulateMetatypes();
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
        }

        private void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                ManagePriorityItems(cboHeritage);
            }
            else if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }
            string strMetatype = lstMetatypes.SelectedValue?.ToString();
            LoadMetatypes();
            if (string.IsNullOrEmpty(strMetatype))
                lstMetatypes.SelectedIndex = -1;
            else
                lstMetatypes.SelectedValue = strMetatype;
        }

        private void cboTalent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
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
            if (_blnInitializing)
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
            if (_blnInitializing)
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
            if (_blnInitializing)
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
                    MessageBox.Show(LanguageManager.GetString("Message_SumtoTen", GlobalOptions.Language).Replace("{0}", _objCharacter.SumtoTen.ToString()).Replace("{1}", intSumToTen.ToString()));
                    return;
                }
            }
            if (cboTalents.SelectedIndex == -1)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectTalent", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectTalent", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSkill1 = cboSkill1.SelectedValue?.ToString();
            string strSkill2 = cboSkill2.SelectedValue?.ToString();
            string strSkill3 = cboSkill3.SelectedValue?.ToString();

            if ((cboSkill1.Visible && string.IsNullOrEmpty(strSkill1)) ||
                (cboSkill2.Visible && string.IsNullOrEmpty(strSkill2)) ||
                (cboSkill3.Visible && string.IsNullOrEmpty(strSkill3)))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectSkill", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectSkill", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if ((cboSkill1.Visible && cboSkill2.Visible && strSkill1 == strSkill2) ||
                (cboSkill1.Visible && cboSkill3.Visible && strSkill1 == strSkill3) ||
                (cboSkill2.Visible && cboSkill3.Visible && strSkill2 == strSkill3))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_Duplicate", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_Duplicate", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;

            bool boolHalveAttributePriorityPoints = false;

            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedMetavariant = cboMetavariant.SelectedValue.ToString();
                string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();
                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
                
                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                    strSelectedMetavariant = "Human";

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                XmlNode objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");

                int intForce = nudForce.Visible ? decimal.ToInt32(nudForce.Value) : 0;

                _objCharacter.MetatypeBP = Convert.ToInt32(lblMetavariantBP.Text);

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
                XmlNode charNode = objXmlMetavariant ?? objXmlMetatype;
                if (strSelectedMetatypeCategory == "Shapeshifter")
                {
                    charNode = objXmlMetatype;
                }

                // Set Metatype information.
                _objCharacter.BOD.AssignLimits(ExpressionToString(charNode["bodmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["bodmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["bodaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.AGI.AssignLimits(ExpressionToString(charNode["agimin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["agimax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["agiaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.REA.AssignLimits(ExpressionToString(charNode["reamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["reamax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["reaaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.STR.AssignLimits(ExpressionToString(charNode["strmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["strmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["straug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.CHA.AssignLimits(ExpressionToString(charNode["chamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["chamax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["chaaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.INT.AssignLimits(ExpressionToString(charNode["intmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["intmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["intaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.LOG.AssignLimits(ExpressionToString(charNode["logmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["logmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["logaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.WIL.AssignLimits(ExpressionToString(charNode["wilmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["wilmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["wilaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.MAG.AssignLimits(ExpressionToString(charNode["magmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["magmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["magaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.MAGAdept.AssignLimits(ExpressionToString(charNode["magmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["magmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["magaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.RES.AssignLimits(ExpressionToString(charNode["resmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["resmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["resaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.EDG.AssignLimits(ExpressionToString(charNode["edgmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["edgmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["edgaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.ESS.AssignLimits(ExpressionToString(charNode["essmin"]?.InnerText, intForce, 0),              ExpressionToString(charNode["essmax"]?.InnerText, intForce, 0),              ExpressionToString(charNode["essaug"]?.InnerText, intForce, 0));
                _objCharacter.DEP.AssignLimits(ExpressionToString(charNode["depmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["depmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["depaug"]?.InnerText, intForce, intMaxModifier));
                if (charNode["halveattributepoints"] != null)
                    boolHalveAttributePriorityPoints = true;

                _objCharacter.Metatype = strSelectedMetatype;
                _objCharacter.MetatypeCategory = strSelectedMetatypeCategory;
                _objCharacter.Metavariant = strSelectedMetavariant == "None" ? string.Empty : strSelectedMetavariant;

                // Load the Qualities file.
                XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

                // Determine if the Metatype has any bonuses.
                XmlNode xmlBonusNode = charNode.SelectSingleNode("bonus");
                if (xmlBonusNode != null)
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, strSelectedMetatype, xmlBonusNode, false, 1, strSelectedMetatype);

                List<Weapon> lstWeapons = new List<Weapon>();

                // Create the Qualities that come with the Metatype.
                foreach (XmlNode objXmlQualityItem in charNode?.SelectNodes("qualities/*/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = objXmlQualityItem.Attributes?["select"]?.InnerText ?? string.Empty;
                    QualitySource objSource = objXmlQualityItem.Attributes["removable"]?.InnerText == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
                    objQuality.Create(objXmlQuality, objSource, lstWeapons, strForceValue);
                    objQuality.ContributeToLimit = false;
                    _objCharacter.Qualities.Add(objQuality);
                }

                //Load any critter powers the character has. 
                objXmlDocument = XmlManager.Load("critterpowers.xml");
                foreach (XmlNode objXmlPower in charNode.SelectNodes("powers/power"))
                {
                    XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                    CritterPower objPower = new CritterPower(_objCharacter);
                    string strForcedValue = objXmlPower.Attributes?["select"]?.InnerText ?? string.Empty;
                    int intRating = Convert.ToInt32(objXmlPower.Attributes?["rating"]?.InnerText);

                    objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                //Load any natural weapons the character has. 
                foreach (XmlNode objXmlNaturalWeapon in charNode.SelectNodes("nautralweapons/naturalweapon"))
                {
                    Weapon objWeapon = new Weapon(_objCharacter)
                    {
                        Name = objXmlNaturalWeapon["name"].InnerText,
                        Category = LanguageManager.GetString("Tab_Critter", GlobalOptions.Language),
                        WeaponType = "Melee",
                        Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText),
                        Damage = objXmlNaturalWeapon["damage"].InnerText,
                        AP = objXmlNaturalWeapon["ap"].InnerText,
                        Mode = "0",
                        RC = "0",
                        Concealability = 0,
                        Avail = "0",
                        Cost = "0",
                        UseSkill = objXmlNaturalWeapon["useskill"].InnerText,
                        Source = objXmlNaturalWeapon["source"].InnerText,
                        Page = objXmlNaturalWeapon["page"].InnerText
                    };

                    _objCharacter.Weapons.Add(objWeapon);
                }

                // begin priority based character settings
                // Load the Priority information.
                XmlDocument objXmlDocumentPriority = XmlManager.Load("priorities.xml");

                // Set the character priority selections
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
                XmlNodeList xmlResourcesPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Resources\" and value = \"" + _objCharacter.ResourcesPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlResourcesPriority in xmlResourcesPriorityList)
                {
                    if (xmlResourcesPriorityList.Count == 1 || xmlResourcesPriority["gameplayoption"] != null)
                    {
                        _objCharacter.StartingNuyen = _objCharacter.Nuyen = Convert.ToInt32(xmlResourcesPriority["resources"]?.InnerText);
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

                XmlNodeList xmlBaseTalentPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + _objCharacter.SpecialPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                {
                    if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority["gameplayoption"] != null)
                    {
                        XmlNode xmlTalentPriorityNode = xmlBaseTalentPriority.SelectSingleNode("talents/talent[value = \"" + _objCharacter.TalentPriority + "\"]");

                        if (xmlTalentPriorityNode != null)
                        {
                            // Create the Qualities that come with the Talent.
                            foreach (XmlNode objXmlQualityItem in xmlTalentPriorityNode.SelectNodes("qualities/quality"))
                            {
                                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                                Quality objQuality = new Quality(_objCharacter);
                                string strForceValue = objXmlQualityItem.Attributes?["select"]?.InnerText ?? string.Empty;
                                QualitySource objSource = objXmlQualityItem.Attributes["removable"]?.InnerText == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
                                objQuality.Create(objXmlQuality, objSource, lstWeapons, strForceValue);
                                _objCharacter.Qualities.Add(objQuality);
                            }

                            // Set starting magic
                            if (xmlTalentPriorityNode["magic"] != null)
                                _objCharacter.MAG.MetatypeMinimum = Convert.ToInt32(xmlTalentPriorityNode["magic"].InnerText);
                            if (xmlTalentPriorityNode["spells"] != null)
                                _objCharacter.SpellLimit = Convert.ToInt32(xmlTalentPriorityNode["spells"].InnerText);
                            if (xmlTalentPriorityNode["maxmagic"] != null)
                                _objCharacter.MAG.MetatypeMaximum = Convert.ToInt32(xmlTalentPriorityNode["magic"].InnerText);
                            // Set starting resonance
                            if (xmlTalentPriorityNode["resonance"] != null)
                                _objCharacter.RES.MetatypeMinimum = Convert.ToInt32(xmlTalentPriorityNode["resonance"].InnerText);
                            if (xmlTalentPriorityNode["cfp"] != null)
                                _objCharacter.CFPLimit = Convert.ToInt32(xmlTalentPriorityNode["cfp"].InnerText);
                            if (xmlTalentPriorityNode["maxresonance"] != null)
                                _objCharacter.RES.MetatypeMaximum = Convert.ToInt32(xmlTalentPriorityNode["maxresonance"].InnerText);
                            // Set starting depth
                            if (xmlTalentPriorityNode["depth"] != null)
                                _objCharacter.DEP.MetatypeMinimum = Convert.ToInt32(xmlTalentPriorityNode["depth"].InnerText);
                            if (xmlTalentPriorityNode["ainormalprogramlimit"] != null)
                                _objCharacter.AINormalProgramLimit = Convert.ToInt32(xmlTalentPriorityNode["ainormalprogramlimit"].InnerText);
                            if (xmlTalentPriorityNode["ainormalprogramlimit"] != null)
                                _objCharacter.AIAdvancedProgramLimit = Convert.ToInt32(xmlTalentPriorityNode["aiadvancedprogramlimit"].InnerText);
                            if (xmlTalentPriorityNode["maxdepth"] != null)
                                _objCharacter.DEP.MetatypeMaximum = Convert.ToInt32(xmlTalentPriorityNode["maxdepth"].InnerText);

                            // Set Free Skills/Skill Groups
                            int intFreeLevels = 0;
                            Improvement.ImprovementType eType = Improvement.ImprovementType.SkillBase;
                            XmlNode objTalentSkillValNode = xmlTalentPriorityNode.SelectSingleNode("skillval");
                            if (objTalentSkillValNode == null || !int.TryParse(objTalentSkillValNode.InnerText, out intFreeLevels))
                            {
                                objTalentSkillValNode = xmlTalentPriorityNode.SelectSingleNode("skillgroupval");
                                if (objTalentSkillValNode != null && int.TryParse(objTalentSkillValNode.InnerText, out intFreeLevels))
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
                _objCharacter.Special = Convert.ToInt32(lblSpecial.Text);
                _objCharacter.TotalSpecial = _objCharacter.Special;

                // Set Attributes
                XmlNodeList objXmlAttributesPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Attributes\" and value = \"" + _objCharacter.AttributesPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode objXmlAttributesPriority in objXmlAttributesPriorityList)
                {
                    if (objXmlAttributesPriorityList.Count == 1 || objXmlAttributesPriority["gameplayoption"] != null)
                    {
                        int intAttributes = Convert.ToInt32(objXmlAttributesPriority["attributes"]?.InnerText);
                        if (boolHalveAttributePriorityPoints)
                            intAttributes /= 2;
                        _objCharacter.TotalAttributes = _objCharacter.Attributes = intAttributes;
                        break;
                    }
                }

                // Set Skills and Skill Groups
                XmlNodeList objXmlSkillsPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Skills\" and value = \"" + _objCharacter.SkillsPriority + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode objXmlSkillsPriority in objXmlSkillsPriorityList)
                {
                    if (objXmlSkillsPriorityList.Count == 1 || objXmlSkillsPriority["gameplayoption"] != null)
                    {
                        if (objXmlSkillsPriority["skills"] != null)
                        {
                            _objCharacter.SkillsSection.SkillPointsMaximum = Convert.ToInt32(objXmlSkillsPriority["skills"].InnerText);
                            _objCharacter.SkillsSection.SkillGroupPointsMaximum = Convert.ToInt32(objXmlSkillsPriority["skillgroups"].InnerText);
                            break;
                        }
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
                    string strKarma = objXmlGameplayOption["karma"].InnerText;
                    string strNuyen = objXmlGameplayOption["maxnuyen"].InnerText;
                    string strContactMultiplier = objXmlGameplayOption["contactmultiplier"].InnerText;
                    _objCharacter.MaxKarma = Convert.ToInt32(strKarma);
                    _objCharacter.MaxNuyen = Convert.ToInt32(strNuyen);
                    _objCharacter.ContactMultiplier = Convert.ToInt32(strContactMultiplier);
                }

                // Set free contact points
                _objCharacter.ContactPoints = _objCharacter.CHA.Value * _objCharacter.ContactMultiplier;

                // Set starting karma
                _objCharacter.BuildKarma = _objCharacter.MaxKarma;

                // Set limit for qualities
                _objCharacter.GameplayOptionQualityLimit = _objCharacter.MaxKarma;

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectMetatype", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                List<string> objPriorities = new List<string>() { "A,4", "B,3", "C,2", "D,1", "E,0" };

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
        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        private static string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            if (string.IsNullOrWhiteSpace(strIn))
                return intOffset.ToString();
            int intValue = 1;
            string strForce = intForce.ToString();
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                intValue = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strIn.Replace("/", " div ").Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce))));
            }
            catch (XPathException) { }
            catch (OverflowException) { } // Result is text and not a double
            catch (InvalidCastException) { }
            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < 1)
                    intValue = 1;
            }
            else if (intValue < 0)
                intValue = 0;
            return intValue.ToString();
        }

        void PopulateTalents()
        {
            // Load the Priority information.
            List<ListItem> lstTalent = new List<ListItem>();

            XmlDocument xmlQualitiesDocument = XmlManager.Load("qualities.xml");
            // Populate the Priority Category list.
            XmlNodeList xmlBaseTalentPriorityList = XmlManager.Load("priorities.xml").SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
            foreach (XmlNode xmlBaseTalentPriority in xmlBaseTalentPriorityList)
            {
                if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority["gameplayoption"] != null)
                {
                    foreach (XmlNode objXmlPriorityTalent in xmlBaseTalentPriority.SelectNodes("talents/talent"))
                    {
                        XmlNode xmlQualitiesNode = objXmlPriorityTalent["qualities"];
                        if (xmlQualitiesNode != null)
                        {
                            bool blnFoundUnavailableQuality = false;

                            foreach (XmlNode xmlQuality in xmlQualitiesNode.SelectNodes("quality"))
                            {
                                if (xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[" + _objCharacter.Options.BookXPath() + " and name = \"" + xmlQuality.InnerText + "\"]") == null)
                                {
                                    blnFoundUnavailableQuality = true;
                                    break;
                                }
                            }

                            if (blnFoundUnavailableQuality)
                                continue;
                        }
                        XmlNode xmlForbiddenNode = objXmlPriorityTalent["forbidden"];
                        if (xmlForbiddenNode != null)
                        {
                            bool blnRequirementForbidden = false;

                            // Loop through the oneof requirements.
                            XmlNodeList objXmlForbiddenList = xmlForbiddenNode.SelectNodes("oneof");
                            foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
                            {
                                XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

                                foreach (XmlNode objXmlForbidden in objXmlOneOfList)
                                {
                                    if (objXmlForbidden.Name == "metatype")
                                    {
                                        // Check the Metatype restriction.
                                        if (objXmlForbidden.InnerText == lstMetatypes.SelectedValue?.ToString())
                                        {
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                        }
                                    }
                                    else if (objXmlForbidden.Name == "metatypecategory")
                                    {
                                        // Check the Metatype Category restriction.
                                        if (objXmlForbidden.InnerText == cboCategory.SelectedValue?.ToString())
                                        {
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                        }
                                    }
                                    else if (objXmlForbidden.Name == "metavariant")
                                    {
                                        // Check the Metavariant restriction.
                                        if (objXmlForbidden.InnerText == cboMetavariant.SelectedValue?.ToString())
                                        {
                                            blnRequirementForbidden = true;
                                            goto EndForbiddenLoop;
                                        }
                                    }
                                }
                            }
                            EndForbiddenLoop:;
                            if (blnRequirementForbidden)
                                continue;
                        }
                        XmlNode xmlRequiredNode = objXmlPriorityTalent["required"];
                        if (xmlRequiredNode != null)
                        {
                            bool blnRequirementMet = false;

                            // Loop through the oneof requirements.
                            XmlNodeList objXmlForbiddenList = xmlRequiredNode.SelectNodes("oneof");
                            foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
                            {
                                XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

                                foreach (XmlNode objXmlRequired in objXmlOneOfList)
                                {
                                    if (objXmlRequired.Name == "metatype")
                                    {
                                        // Check the Metatype restriction.
                                        if (objXmlRequired.InnerText == lstMetatypes.SelectedValue?.ToString())
                                        {
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                        }
                                    }
                                    else if (objXmlRequired.Name == "metatypecategory")
                                    {
                                        // Check the Metatype Category restriction.
                                        if (objXmlRequired.InnerText == cboCategory.SelectedValue?.ToString())
                                        {
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                        }
                                    }
                                    else if (objXmlRequired.Name == "metavariant")
                                    {
                                        // Check the Metavariant restriction.
                                        if (objXmlRequired.InnerText == cboMetavariant.SelectedValue?.ToString())
                                        {
                                            blnRequirementMet = true;
                                            goto EndRequiredLoop;
                                        }
                                    }
                                }
                            }
                            EndRequiredLoop:;
                            if (!blnRequirementMet)
                                continue;
                        }
                        lstTalent.Add(new ListItem(objXmlPriorityTalent["value"].InnerText, objXmlPriorityTalent["translate"]?.InnerText ?? objXmlPriorityTalent["name"].InnerText));
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
                bool blnOldInitializing = _blnInitializing;
                _blnInitializing = true;
                cboTalents.SelectedIndex = intOldSelectedIndex;
                _blnInitializing = blnOldInitializing;
            }
            cboTalents.EndUpdate();
        }

        void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();

            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedHeritage = cboHeritage.SelectedValue?.ToString();
                // Load the Priority information.
                XmlDocument objXmlDocumentPriority = XmlManager.Load("priorities.xml");
                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                XmlNode objXmlMetatypeBP = null;
                XmlNodeList xmlBaseMetatypePriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + strSelectedHeritage + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority["gameplayoption"] != null)
                    {
                        objXmlMetatypeBP = xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                        break;
                    }
                }

                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                if (objXmlMetatypeBP != null)
                {
                    // Retrieve the list of Metavariants for the selected Metatype.
                    XmlNodeList objXmlMetavariantList = objXmlMetatype.SelectNodes("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]");
                    foreach (XmlNode objXmlMetavariant in objXmlMetavariantList)
                    {
                        string strName = objXmlMetavariant["name"].InnerText;
                        lstMetavariants.Add(new ListItem(strName, objXmlMetavariant["translate"]?.InnerText ?? strName));
                    }

                    string strOldSelectedValue = lstMetatypes.SelectedValue?.ToString() ?? _objCharacter.Metavariant;
                    bool blnOldInitializing = _blnInitializing;
                    _blnInitializing = true;
                    cboMetavariant.BeginUpdate();
                    cboMetavariant.ValueMember = "Value";
                    cboMetavariant.DisplayMember = "Name";
                    cboMetavariant.DataSource = lstMetavariants;
                    cboMetavariant.Enabled = lstMetavariants.Count > 1;
                    _blnInitializing = blnOldInitializing;
                    if (!string.IsNullOrEmpty(strOldSelectedValue))
                        cboMetavariant.SelectedValue = strOldSelectedValue;
                    if (cboMetavariant.SelectedIndex == -1)
                        cboMetavariant.SelectedIndex = 0;
                    cboMetavariant.EndUpdate();

                    // If the Metatype has Force enabled, show the Force NUD.
                    string strEssMax = objXmlMetatype["essmax"]?.InnerText ?? string.Empty;
                    if (objXmlMetatype["forcecreature"] != null || strEssMax.Contains("D6"))
                    {
                        lblForceLabel.Visible = true;
                        nudForce.Visible = true;

                        if (strEssMax.Contains("D6"))
                        {
                            int intPos = strEssMax.IndexOf("D6") - 1;
                            lblForceLabel.Text = strEssMax.Substring(intPos, 3);
                            nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1)) * 6;
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
            string strSelectedMetatypeCategory = cboCategory.SelectedValue.ToString();
            List<ListItem> lstMetatype = new List<ListItem>();

            XmlNodeList objXmlMetatypeList = XmlManager.Load(_strXmlFile).SelectNodes("/chummer/metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + strSelectedMetatypeCategory + "\"]");

            XmlNodeList xmlBaseMetatypePriorityList = XmlManager.Load("priorities.xml").SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
            foreach (XmlNode xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
            {
                if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority["gameplayoption"] != null)
                {
                    foreach (XmlNode objXmlMetatype in objXmlMetatypeList)
                    {
                        string strName = objXmlMetatype["name"]?.InnerText ?? string.Empty;
                        if (null != xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + strName + "\"]"))
                        {
                            lstMetatype.Add(new ListItem(strName, objXmlMetatype["translate"]?.InnerText ?? strName));
                        }
                    }
                    break;
                }
            }
            
            lstMetatype.Sort(CompareListItems.CompareNames);
            string strOldSelectedValue = lstMetatypes.SelectedValue?.ToString() ?? _objCharacter.Metatype;
            bool blnOldInitializing = _blnInitializing;
            _blnInitializing = true;
            lstMetatypes.BeginUpdate();
            lstMetatypes.ValueMember = "Value";
            lstMetatypes.DisplayMember = "Name";
            lstMetatypes.DataSource = lstMetatype;
            _blnInitializing = blnOldInitializing;
            if (!string.IsNullOrEmpty(strOldSelectedValue))
                lstMetatypes.SelectedValue = strOldSelectedValue;
            if (lstMetatypes.SelectedIndex == -1 && lstMetatype.Count > 0)
                lstMetatypes.SelectedIndex = 0;
            lstMetatypes.EndUpdate();

            if (strSelectedMetatypeCategory.EndsWith("Spirits"))
            {
                chkBloodSpirit.Visible = true;
                chkPossessionBased.Visible = true;
                cboPossessionMethod.Visible = true;
            }
            else
            {
                chkBloodSpirit.Checked = false;
                chkBloodSpirit.Visible = false;
                chkPossessionBased.Visible = false;
                chkPossessionBased.Checked = false;
                cboPossessionMethod.Visible = false;
            }
        }

        private void LoadMetatypes()
        {
            List<ListItem> lstCategory = new List<ListItem>();

            // Load the Metatype information.
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

            // Populate the Metatype Category list.
            XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category");

            // Create a list of any Categories that should not be in the list.
            HashSet<string> lstRemoveCategory = new HashSet<string>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                XmlNodeList xmlBaseMetatypePriorityList = XmlManager.Load("priorities.xml").SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue.ToString() + "\" and (not(gameplayoption) or gameplayoption = \"" + _objCharacter.GameplayOption + "\")]");
                foreach (XmlNode xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority["gameplayoption"] != null)
                    {
                        foreach (XmlNode objXmlMetatype in objXmlDocument.SelectNodes("/chummer/metatypes/metatype[category = \"" + objXmlCategory.InnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")]"))
                        {
                            if (null != xmlBaseMetatypePriority.SelectSingleNode("metatypes/metatype[name = \"" + objXmlMetatype["name"]?.InnerText + "\"]"))
                            {
                                goto NextItem;
                            }
                        }
                        break;
                    }
                }
                // Remove metatypes not covered by heritage
                lstRemoveCategory.Add(objXmlCategory.InnerText);
                NextItem:;
            }

            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                string strInnerText = objXmlCategory.InnerText ?? string.Empty;

                // Make sure the Category isn't in the exclusion list.
                if (!lstRemoveCategory.Contains(strInnerText) &&
                    // Also make sure it is not already in the Category list.
                    !lstCategory.Any(objItem => objItem.Value.ToString() == strInnerText))
                {
                    lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
            }
            
            lstCategory.Sort(CompareListItems.CompareNames);
            string strOldSelected = cboCategory.SelectedValue?.ToString() ?? _objCharacter.MetatypeCategory;
            bool blnOldInitializing = _blnInitializing;
            _blnInitializing = true;
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstCategory;
            _blnInitializing = blnOldInitializing;
            if (!string.IsNullOrEmpty(strOldSelected))
                cboCategory.SelectedValue = strOldSelected;
            if (cboCategory.SelectedIndex == -1 && lstCategory.Count > 0)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();
        }

        private static XmlNode GetSpecificSkill(string strSkill)
        {
            return XmlManager.Load("skills.xml").SelectSingleNode("/chummer/skills/skill[name = \"" + strSkill + "\"]");
        }

        private static XmlNodeList GetMatrixSkillList()
        {
            return XmlManager.Load("skills.xml").SelectNodes("/chummer/skills/skill[skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
        }

        private static XmlNode GetSpecificSkillGroup(string strSkill)
        {
            return XmlManager.Load("skills.xml").SelectSingleNode("/chummer/skillgroups/name[. = \"" + strSkill + "\"]");
        }

        private static XmlNodeList GetMagicalSkillList(XmlNodeList objNodeList = null)
        {
            return XmlManager.Load("skills.xml").SelectNodes("/chummer/skills/skill[category = \"Magical Active\" or category = \"Pseudo-Magical Active\"]");
        }

        private static XmlNodeList GetResonanceSkillList()
        {
            return XmlManager.Load("skills.xml").SelectNodes("/chummer/skills/skill[category = \"Resonance Active\" or skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
        }

        private static XmlNodeList GetActiveSkillList()
        {
            return XmlManager.Load("skills.xml").SelectNodes("/chummer/skills/skill");
        }

        private static XmlNodeList BuildSkillCategoryList(XmlNodeList objSkillList)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            StringBuilder strGroups = new StringBuilder("/chummer/skillgroups/name[. = \"");
            strGroups.Append(objSkillList[0].InnerText);
            strGroups.Append('\"');
            for (int i = 1; i < objSkillList.Count; i++)
            {
                strGroups.Append(" or . = \"");
                strGroups.Append(objSkillList[i].InnerText);
                strGroups.Append('\"');
            }
            strGroups.Append(']');
            return objXmlSkillsDocument.SelectNodes(strGroups.ToString());
        }

        private static XmlNodeList BuildSkillList(XmlNodeList objSkillList)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            StringBuilder strGroups = new StringBuilder("/chummer/skills/skill[name = \"");
            strGroups.Append(objSkillList[0].InnerText);
            strGroups.Append('\"');
            for (int i = 1; i < objSkillList.Count; i++)
            {
                strGroups.Append(" or name = \"");
                strGroups.Append(objSkillList[i].InnerText);
                strGroups.Append('\"');
            }
            strGroups.Append(']');
            return objXmlSkillsDocument.SelectNodes(strGroups.ToString());
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion
    }
}

