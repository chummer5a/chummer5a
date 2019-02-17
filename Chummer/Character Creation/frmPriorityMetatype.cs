using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using Chummer.Skills;

namespace Chummer
{
    public partial class frmPriorityMetatype : Form
    {
        private readonly Character _objCharacter;

        private string _strXmlFile = "metatypes.xml";
        private string _strPrioritiesXmlFile = "priorities.xml";
        private string _strMetatype = string.Empty;
        private string _strAttributes = string.Empty;
        private string _strSpecial = string.Empty;
        private string _strSkills = string.Empty;
        private string _strResources = string.Empty;
        private string _strSelectedMetatype = string.Empty;
        private string _strSelectedMetavariant = string.Empty;
        private string _strSelectedMetatypeCategory = string.Empty;
        private string _strSelectedTalent = string.Empty;
        private int intBuildMethod = 0;
        private List<ListItem> _lstCategory = new List<ListItem>();
        private bool _blnInitializing = false;
        private List<string> _lstPrioritySkills = new List<string>();

        #region Character Events
        private void objCharacter_MAGEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_RESEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_DEPEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_AdeptTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_MagicianTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_TechnomancerTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_AdvancedProgramsTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_CyberwareTabDisabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_InitiationTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_CritterTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }
        #endregion

        #region Properties
        /// <summary>
        /// XML file to read Metatype/Critter information from.
        /// </summary>
        public string XmlFile
        {
            set
            {
                _strXmlFile = value;
            }
        }
        public string Metatype
        {
            get
            {
                return _strMetatype;
            }
            set
            {
                _strMetatype = value;
            }
        }
        public string Resources
        {
            get
            {
                return _strResources;
            }
            set
            {
                _strResources = value;
            }
        }
        public string Skills
        {
            get
            {
                return _strSkills;
            }
            set
            {
                _strSkills = value;
            }
        }
        public string Attributes
        {
            get
            {
                return _strAttributes;
            }
            set
            {
                _strAttributes = value;
            }
        }
        public string Special
        {
            get
            {
                return _strSpecial;
            }
            set
            {
                _strSpecial = value;
            }
        }
        public string SelectedMetatype
        {
            get
            {
                return _strSelectedMetatype;
            }
            set
            {
                _strSelectedMetatype = value;
            }
        }
        public string SelectedMetatypeCategory
        {
            get
            {
                return _strSelectedMetatypeCategory;
            }
            set
            {
                _strSelectedMetatypeCategory = value;
            }
        }
        public string SelectedTalent
        {
            get
            {
                return _strSelectedTalent;
            }
            set
            {
                _strSelectedTalent = value;
            }
        }
        public List<string> PriorityBonusSkillList
        {
            get
            {
                return _lstPrioritySkills;
            }
            set
            {
                _lstPrioritySkills = value;
            }
        }

        public string SelectedMetavariant
        {
            get
            {
                return _strSelectedMetavariant;
            }
            set
            {
                _strSelectedMetavariant = value;
            }
        }

        #endregion

        #region Form Events
        public frmPriorityMetatype(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);

            // Attach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged += objCharacter_MAGEnabledChanged;
            _objCharacter.RESEnabledChanged += objCharacter_RESEnabledChanged;
            _objCharacter.DEPEnabledChanged += objCharacter_DEPEnabledChanged;
            _objCharacter.AdeptTabEnabledChanged += objCharacter_AdeptTabEnabledChanged;
            _objCharacter.MagicianTabEnabledChanged += objCharacter_MagicianTabEnabledChanged;
            _objCharacter.TechnomancerTabEnabledChanged += objCharacter_TechnomancerTabEnabledChanged;
            _objCharacter.AdvancedProgramsTabEnabledChanged += objCharacter_AdvancedProgramsTabEnabledChanged;
            _objCharacter.CyberwareTabDisabledChanged += objCharacter_CyberwareTabDisabledChanged;
            _objCharacter.InitiationTabEnabledChanged += objCharacter_InitiationTabEnabledChanged;
            _objCharacter.CritterTabEnabledChanged += objCharacter_CritterTabEnabledChanged;
        }

        private void frmPriorityMetatype_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Detach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged -= objCharacter_MAGEnabledChanged;
            _objCharacter.RESEnabledChanged -= objCharacter_RESEnabledChanged;
            _objCharacter.DEPEnabledChanged -= objCharacter_DEPEnabledChanged;
            _objCharacter.AdeptTabEnabledChanged -= objCharacter_AdeptTabEnabledChanged;
            _objCharacter.MagicianTabEnabledChanged -= objCharacter_MagicianTabEnabledChanged;
            _objCharacter.TechnomancerTabEnabledChanged -= objCharacter_TechnomancerTabEnabledChanged;
            _objCharacter.AdvancedProgramsTabEnabledChanged -= objCharacter_AdvancedProgramsTabEnabledChanged;
            _objCharacter.CyberwareTabDisabledChanged -= objCharacter_CyberwareTabDisabledChanged;
            _objCharacter.InitiationTabEnabledChanged -= objCharacter_InitiationTabEnabledChanged;
            _objCharacter.CritterTabEnabledChanged -= objCharacter_CritterTabEnabledChanged;
        }

        private void frmPriorityMetatype_Load(object sender, EventArgs e)
        {
            // Load the Priority information.
            XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);
            if (string.IsNullOrEmpty(_objCharacter.GameplayOption))
                _objCharacter.GameplayOption = "Standard";

            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                intBuildMethod = 1;
                lblSumtoTen.Visible = true;
            }
            // Populate the Priority Category list.
            _blnInitializing = true;
            XmlNodeList objXmlPriorityCategoryList = objXmlDocumentPriority.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlPriorityCategory in objXmlPriorityCategoryList)
            {
                string strXPath = string.Empty;
                strXPath = "/chummer/priorities/priority[category = \"" + objXmlPriorityCategory.InnerText +
                              "\" and gameplayoption = \"" + _objCharacter.GameplayOption + "\"]";
                XmlNodeList objItems = objXmlDocumentPriority.SelectNodes(strXPath);

                if (objItems != null && objItems.Count == 0)
                {
                    strXPath = "/chummer/priorities/priority[category = \"" + objXmlPriorityCategory.InnerText +
                               "\" and not (gameplayoption)]";
                    objItems = objXmlDocumentPriority.SelectNodes(strXPath);
                }

                if (objItems.Count > 0)
                {
                    List<ListItem> lstItems = new List<ListItem>();
                    // lstItems.Add(new ListItem());
                    foreach (XmlNode objXmlPriority in objItems)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objXmlPriority["value"].InnerText;
                        objItem.Name = objXmlPriority["translate"]?.InnerText ?? objXmlPriority["name"].InnerText;
                        lstItems.Add(objItem);
                    }
                    SortListItem objPrioritySort = new SortListItem();
                    lstItems.Sort(objPrioritySort.Compare);
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

            // Load Metatypes
            LoadMetatypes();
            PopulateTalents();

            // Set Priority defaults.
            if (!string.IsNullOrEmpty(_strAttributes))
            {
                int index = 0;
                //Attributes
                index = cboAttributes.FindString(_strAttributes);
                cboAttributes.SelectedIndex = index;
                //Heritage (Metatype)
                index = cboHeritage.FindString(_strMetatype);
                cboHeritage.SelectedIndex = index;
                //Resources
                index = cboResources.FindString(_strResources);
                cboResources.SelectedIndex = index;
                //Skills
                index = cboSkills.FindString(_strSkills);
                cboSkills.SelectedIndex = index;
                //Magical/Resonance Talent
                index = cboTalent.FindString(_strSpecial);
                cboTalent.SelectedIndex = index;
                //Magical/Resonance Type
                index = cboTalents.FindString(_strSelectedTalent);
                cboTalents.SelectedIndex = index;
                //Selected Magical Bonus Skill
                string strSkill = _lstPrioritySkills.ElementAtOrDefault(0);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    index = cboSkill1.FindString(strSkill);
                    cboSkill1.SelectedIndex = index;
                }
                else
                    cboSkill1.Visible = false;
                strSkill = _lstPrioritySkills.ElementAtOrDefault(1);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    index = cboSkill2.FindString(strSkill);
                    cboSkill2.SelectedIndex = index;
                }
                else
                    cboSkill2.Visible = false;
                strSkill = _lstPrioritySkills.ElementAtOrDefault(2);
                if (!string.IsNullOrEmpty(strSkill))
                {
                    index = cboSkill3.FindString(strSkill);
                    cboSkill3.SelectedIndex = index;
                }
                else
                    cboSkill3.Visible = false;
                //Selected Category of Metatype
                index = cboCategory.FindString(_strSelectedMetatypeCategory);
                cboCategory.SelectedIndex = index;
                //Selected Metatype
                index = lstMetatypes.FindString(_strSelectedMetatype);
                lstMetatypes.SelectedIndex = index;
                //Selected Metavariant
                index = cboMetavariant.FindString(_strSelectedMetavariant);
                cboMetavariant.SelectedIndex = index;
            }
            else
            {
                cboHeritage.SelectedIndex = 0;
                cboTalent.SelectedIndex = 1;
                cboAttributes.SelectedIndex = 2;
                cboSkills.SelectedIndex = 3;
                cboResources.SelectedIndex = 4;
                lstMetatypes.SelectedIndex = 0;
            }
            _blnInitializing = false;
            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                SumtoTen();
            }

            // Make sure lists are properly populated so that you can't e.g. select Magician if you're reprioritizing a Mundane
            string strMetatype = string.Empty;
            string strMetavariant = string.Empty;
            if (lstMetatypes.SelectedIndex >= 0)
            {
                strMetatype = lstMetatypes.SelectedValue.ToString();
                strMetavariant = cboMetavariant.SelectedValue.ToString();
            }
            LoadMetatypes();
            if (lstMetatypes.SelectedIndex >= 0)
            {
                lstMetatypes.SelectedValue = strMetatype;
                cboMetavariant.SelectedValue = strMetavariant;
            }
            PopulateTalents();


            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            tipTooltip.SetToolTip(chkPossessionBased, LanguageManager.GetString("Tip_Metatype_PossessionTradition"));
            tipTooltip.SetToolTip(chkBloodSpirit, LanguageManager.GetString("Tip_Metatype_BloodSpirit"));

            XmlDocument objXmlDocument = XmlManager.Load("critterpowers.xml");
            XmlNode objXmlPossession = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Possession\"]");
            XmlNode objXmlInhabitation = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Inhabitation\"]");
            List<ListItem> lstMethods = new List<ListItem>();

            ListItem objPossession = new ListItem();
            objPossession.Value = "Possession";
            objPossession.Name = objXmlPossession["translate"]?.InnerText ?? objXmlPossession["name"].InnerText;

            ListItem objInhabitation = new ListItem();
            objInhabitation.Value = "Inhabitation";
            objInhabitation.Name = objXmlInhabitation["translate"]?.InnerText ?? objXmlInhabitation["name"].InnerText;

            lstMethods.Add(objInhabitation);
            lstMethods.Add(objPossession);

            SortListItem objSortPossession = new SortListItem();
            lstMethods.Sort(objSortPossession.Compare);
            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.ValueMember = "Value";
            cboPossessionMethod.DisplayMember = "Name";
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.SelectedIndex = cboPossessionMethod.FindStringExact(objPossession.Name);
            cboPossessionMethod.EndUpdate();
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Priority information.
            XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);

            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(lstMetatypes.Text))
            {
                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]");
                XmlNode objXmlMetatypeBP = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");

                if (objXmlMetatypeBP["karma"] != null)
                {
                    lblMetavariantBP.Text = objXmlMetatypeBP["karma"].InnerText;
                }
                else
                {
                    lblMetavariantBP.Text = "0";
                }

                if (objXmlMetatype["forcecreature"] == null)
                {
                    lblBOD.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["bodmin"].InnerText, objXmlMetatype["bodmax"].InnerText, objXmlMetatype["bodaug"].InnerText);
                    lblAGI.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["agimin"].InnerText, objXmlMetatype["agimax"].InnerText, objXmlMetatype["agiaug"].InnerText);
                    lblREA.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["reamin"].InnerText, objXmlMetatype["reamax"].InnerText, objXmlMetatype["reaaug"].InnerText);
                    lblSTR.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["strmin"].InnerText, objXmlMetatype["strmax"].InnerText, objXmlMetatype["straug"].InnerText);
                    lblCHA.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["chamin"].InnerText, objXmlMetatype["chamax"].InnerText, objXmlMetatype["chaaug"].InnerText);
                    lblINT.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["intmin"].InnerText, objXmlMetatype["intmax"].InnerText, objXmlMetatype["intaug"].InnerText);
                    lblLOG.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["logmin"].InnerText, objXmlMetatype["logmax"].InnerText, objXmlMetatype["logaug"].InnerText);
                    lblWIL.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["wilmin"].InnerText, objXmlMetatype["wilmax"].InnerText, objXmlMetatype["wilaug"].InnerText);
                    lblINI.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["inimin"].InnerText, objXmlMetatype["inimax"].InnerText, objXmlMetatype["iniaug"].InnerText);
                }
                else
                {
                    lblBOD.Text = objXmlMetatype["bodmin"].InnerText;
                    lblAGI.Text = objXmlMetatype["agimin"].InnerText;
                    lblREA.Text = objXmlMetatype["reamin"].InnerText;
                    lblSTR.Text = objXmlMetatype["strmin"].InnerText;
                    lblCHA.Text = objXmlMetatype["chamin"].InnerText;
                    lblINT.Text = objXmlMetatype["intmin"].InnerText;
                    lblLOG.Text = objXmlMetatype["logmin"].InnerText;
                    lblWIL.Text = objXmlMetatype["wilmin"].InnerText;
                    lblINI.Text = objXmlMetatype["inimin"].InnerText;
                }

                List<ListItem> lstMetavariants = new List<ListItem>();
                ListItem objNone = new ListItem();
                objNone.Value = "None";
                objNone.Name = LanguageManager.GetString("String_None");
                lstMetavariants.Add(objNone);

                // Retrieve the list of Metavariants for the selected Metatype.
                XmlNodeList objXmlMetavariantList = objXmlMetatype.SelectNodes("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]");
                foreach (XmlNode objXmlMetavariant in objXmlMetavariantList)
                {
                    ListItem objMetavariant = new ListItem();
                    objMetavariant.Value = objXmlMetavariant["name"].InnerText;
                    objMetavariant.Name = objXmlMetavariant["translate"]?.InnerText ?? objXmlMetavariant["name"].InnerText;
                    lstMetavariants.Add(objMetavariant);
                }

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;

                // Select the None item.
                cboMetavariant.SelectedIndex = 0;
                cboMetavariant.Enabled = lstMetavariants.Count > 1;
                cboMetavariant.EndUpdate();

                // Set the special attributes label.
                string strMetavariantString = string.Empty;
                if (cboMetavariant.SelectedValue != null && cboMetavariant.SelectedValue.ToString() != "None" && lstMetatypes.SelectedValue != null && cboHeritage.SelectedValue != null)
                    strMetavariantString = "/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]";
                XmlNode objXmlMetatypeNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]" + strMetavariantString);
                int intSpecialAttribPoints = 0;
                int.TryParse(objXmlMetatypeNode?["value"]?.InnerText, out intSpecialAttribPoints);
                XmlNode objXmlTalentsNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[name = \"" + cboTalents.SelectedValue + "\"]");
                int intTalentSpecialAttribPoints = 0;
                if (int.TryParse(objXmlTalentsNode?["specialattribpoints"]?.InnerText.ToString(), out intTalentSpecialAttribPoints))
                    intSpecialAttribPoints += intTalentSpecialAttribPoints;
                lblSpecial.Text = intSpecialAttribPoints.ToString();

                // If the Metatype has Force enabled, show the Force NUD.
                if (objXmlMetatype["forcecreature"] != null || objXmlMetatype["essmax"].InnerText.Contains("D6"))
                {
                    lblForceLabel.Visible = true;
                    nudForce.Visible = true;

                    if (objXmlMetatype["essmax"].InnerText.Contains("D6"))
                    {
                        int intPos = objXmlMetatype["essmax"].InnerText.IndexOf("D6") - 1;
                        lblForceLabel.Text = objXmlMetatype["essmax"].InnerText.Substring(intPos, 3);
                        nudForce.Maximum = Convert.ToInt32(objXmlMetatype["essmax"].InnerText.Substring(intPos, 1)) * 6;
                    }
                    else
                    {
                        lblForceLabel.Text = LanguageManager.GetString("String_Force");
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
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>();
                ListItem objNone = new ListItem();
                objNone.Value = "None";
                objNone.Name = LanguageManager.GetString("String_None");
                lstMetavariants.Add(objNone);

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = lstMetavariants.Count > 1;
                cboMetavariant.EndUpdate();
            }
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
            if (cboTalents.SelectedIndex >= 0 && cboTalents.SelectedValue != null)
            {
                XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);
                XmlNode objTalentsNode =
                    objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Talent\" and value = \"" +
                                                            cboTalent.SelectedValue + "\"]/talents/talent[value = \"" +
                                                            cboTalents.SelectedValue + "\"]");
                if (objTalentsNode != null)
                {
                    string strSkillCount = (objTalentsNode.SelectSingleNode("skillqty")?.InnerText ??
                                            objTalentsNode.SelectSingleNode("skillgroupqty")?.InnerText) ?? string.Empty;
                    int intSkillCount;
                    if (!string.IsNullOrEmpty(strSkillCount) && int.TryParse(strSkillCount, out intSkillCount))
                    {
                        string strSkillType = (objTalentsNode.SelectSingleNode("skilltype")?.InnerText ??
                                               objTalentsNode.SelectSingleNode("skillgrouptype")?.InnerText);
                        string strSkillVal = (objTalentsNode.SelectSingleNode("skillval")?.InnerText ??
                                              objTalentsNode.SelectSingleNode("skillgroupval")?.InnerText);
                        XmlNodeList objNodeList = (objTalentsNode.SelectNodes("skillgroupchoices/skillgroup"));
                        XmlNodeList skillNodeList = (objTalentsNode.SelectNodes("skillchoices/skill"));
                        string strLabel = LanguageManager.GetString("String_MetamagicSkillBase");
                        strLabel = string.Format(strLabel, LanguageManager.GetString("String_MetamagicSkills"));
                        strLabel = string.Format(strLabel, strSkillCount, strSkillType, strSkillVal);
                        lblMetatypeSkillSelection.Text = strLabel;
                        XmlNodeList objXmlSkillsList;
                        switch (strSkillType)
                        {
                            case "magic":
                                {
                                    objXmlSkillsList = GetMagicalSkillList();
                                    break;
                                }
                            case "resonance":
                                {
                                    objXmlSkillsList = GetResonanceSkillList();
                                    break;
                                }
                            case "matrix":
                                {
                                    objXmlSkillsList = GetMatrixSkillList();
                                    break;
                                }
                            case "grouped":
                                {
                                    objXmlSkillsList = BuildSkillCategoryList(objNodeList);
                                    break;
                                }
                            case "specific":
                                {
                                    objXmlSkillsList = BuildSkillList(skillNodeList);
                                    break;
                                }
                            default:
                                {
                                    objXmlSkillsList = GetActiveSkillList();
                                    break;
                                }
                        }

                        if (intSkillCount > 0)
                        {
                            List<ListItem> lstSkills = new List<ListItem>();
                            if (objNodeList.Count > 0)
                            {
                                lstSkills.AddRange(from XmlNode objXmlSkill in objXmlSkillsList
                                                   select new ListItem
                                                   {
                                                       Value = objXmlSkill.InnerText,
                                                       Name = objXmlSkill.Attributes["translate"]?.InnerText ?? objXmlSkill.InnerText
                                                   });
                            }
                            else
                            {
                                lstSkills.AddRange(from XmlNode objXmlSkill in objXmlSkillsList
                                                   select new ListItem
                                                   {
                                                       Value = objXmlSkill["name"]?.InnerText,
                                                       Name = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText
                                                   });
                            }
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
                            lblMetatypeSkillSelection.Visible = true;
                        }
                        else
                        {
                            lblMetatypeSkillSelection.Visible = false;
                        }

                        string strMetavariantString = string.Empty;
                        if (cboMetavariant.SelectedValue != null && cboMetavariant.SelectedValue.ToString() != "None" && lstMetatypes.SelectedValue != null && cboHeritage.SelectedValue != null)
                            strMetavariantString = "/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]";
                        XmlNode objXmlMetatypePriorityNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]" + strMetavariantString);
                        int intSpecialAttribPoints = 0;
                        int.TryParse(objXmlMetatypePriorityNode?["value"]?.InnerText, out intSpecialAttribPoints);
                        int intTalentSpecialAttribPoints = 0;
                        if (int.TryParse(objTalentsNode["specialattribpoints"]?.InnerText, out intTalentSpecialAttribPoints))
                            intSpecialAttribPoints += intTalentSpecialAttribPoints;
                        lblSpecial.Text = intSpecialAttribPoints.ToString();
                    }
                }
            }
            else
            {
                cboTalents.SelectedIndex = 0;
                lblMetatypeSkillSelection.Visible = false;
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
            XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);

            if (cboMetavariant.SelectedValue.ToString() != "None")
            {
                XmlNode objXmlMetavariant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]");
                XmlNode objXmlMetavariantBP = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue.ToString() + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]");
                if (objXmlMetavariantBP == null)
                {
                    MessageBox.Show(LanguageManager.GetString("String_NotSupported"), "Chummer5",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                if (objXmlMetavariantBP["karma"] != null)
                {
                    lblMetavariantBP.Text = objXmlMetavariantBP["karma"].InnerText;
                }
                else
                {
                    lblMetavariantBP.Text = "0";
                }

                // Set the special attributes label.
                int intSpecialAttribPoints = 0;
                int.TryParse(objXmlMetavariantBP?["value"]?.InnerText, out intSpecialAttribPoints);
                XmlNode objXmlTalentPriorityNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[name = \"" + cboTalents.SelectedValue + "\"]");
                int intTalentSpecialAttribPoints = 0;
                if (int.TryParse(objXmlTalentPriorityNode?["specialattribpoints"]?.InnerText, out intTalentSpecialAttribPoints))
                    intSpecialAttribPoints += intTalentSpecialAttribPoints;
                lblSpecial.Text = intSpecialAttribPoints.ToString();

                string strQuality = string.Empty;
                Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                // Build a list of the Metavariant's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/positive/quality"))
                {
                    strQuality = string.Empty;
                    if (GlobalOptions.Language != "en-us")
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strQuality += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            strQuality += " (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                    }
                    else
                    {
                        strQuality += objXmlQuality.InnerText;
                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            strQuality += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                    }
                    if (dicQualities.ContainsKey(strQuality))
                    {
                        dicQualities[strQuality] += 1;
                    }
                    else
                        dicQualities.Add(strQuality, 1);
                }
                // Build a list of the Metavariant's Negative Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/negative/quality"))
                {
                    strQuality = string.Empty;
                    if (GlobalOptions.Language != "en-us")
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strQuality += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            strQuality += " (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                    }
                    else
                    {
                        strQuality += objXmlQuality.InnerText;
                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            strQuality += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                    }
                    if (dicQualities.ContainsKey(strQuality))
                    {
                        dicQualities[strQuality] += 1;
                    }
                    else
                        dicQualities.Add(strQuality, 1);
                }
                string strQualities = string.Empty;
                if (dicQualities.Count > 0)
                {
                    bool blnFirst = true;
                    foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                    {
                        if (blnFirst)
                            blnFirst = false;
                        else
                            strQualities += ", ";
                        strQualities += objLoopQuality.Key;
                        if (objLoopQuality.Value > 1)
                            strQualities += " " + objLoopQuality.Value.ToString();
                    }
                }
                else
                    strQualities = LanguageManager.GetString("String_None");
                lblMetavariantQualities.Text = strQualities;
            }
            else
            {
                // Set the special attributes label.
                if (lstMetatypes.SelectedItem != null)
                {
                    cmdOK.Enabled = true;
                    XmlNode objXmlMetatypePriorityNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue.ToString() + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]");
                    XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
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
                    // Build a list of the Metavariant's Positive Qualities.
                    foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                    {
                        strQuality = string.Empty;
                        if (GlobalOptions.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQuality += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQuality += " (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                        }
                        else
                        {
                            strQuality += objXmlQuality.InnerText;
                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQuality += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                        }
                        if (dicQualities.ContainsKey(strQuality))
                        {
                            dicQualities[strQuality] += 1;
                        }
                        else
                            dicQualities.Add(strQuality, 1);
                    }
                    // Build a list of the Metavariant's Negative Qualities.
                    foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                    {
                        strQuality = string.Empty;
                        if (GlobalOptions.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQuality += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQuality += " (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                        }
                        else
                        {
                            strQuality += objXmlQuality.InnerText;
                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQuality += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                        }
                        if (dicQualities.ContainsKey(strQuality))
                        {
                            dicQualities[strQuality] += 1;
                        }
                        else
                            dicQualities.Add(strQuality, 1);
                    }

                    string strQualities = string.Empty;
                    if (dicQualities.Count > 0)
                    {
                        bool blnFirst = true;
                        foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                        {
                            if (blnFirst)
                                blnFirst = false;
                            else
                                strQualities += ", ";
                            strQualities += objLoopQuality.Key;
                            if (objLoopQuality.Value > 1)
                                strQualities += " " + objLoopQuality.Value.ToString();
                        }
                    }
                    else
                        strQualities = LanguageManager.GetString("String_None");
                    lblMetavariantQualities.Text = strQualities;

                    lblMetavariantBP.Text = objXmlMetatypePriorityNode["karma"]?.InnerText;
                    // Set the special attributes label.
                    int intSpecialAttribPoints = 0;
                    int.TryParse(objXmlMetatypePriorityNode["value"]?.InnerText, out intSpecialAttribPoints);
                    XmlNode objXmlTalentPriorityNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[name = \"" + cboTalents.SelectedValue + "\"]");
                    int intTalentSpecialAttribPoints = 0;
                    if (int.TryParse(objXmlTalentPriorityNode?["specialattribpoints"]?.InnerText, out intTalentSpecialAttribPoints))
                        intSpecialAttribPoints += intTalentSpecialAttribPoints;
                    lblSpecial.Text = intSpecialAttribPoints.ToString();
                }
            }
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
            string strMetatype = string.Empty;
            if (lstMetatypes.SelectedIndex >= 0)
                strMetatype = lstMetatypes.SelectedValue.ToString();
            LoadMetatypes();
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

            if (_objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen && (SumtoTen() != _objCharacter.SumtoTen))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SumtoTen").Replace("{0}", (_objCharacter.SumtoTen.ToString())).Replace("{1}", (SumtoTen().ToString())));
                return;
            }
            if (cboTalents.SelectedIndex == -1)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectTalent"), LanguageManager.GetString("MessageTitle_Metatype_SelectTalent"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if ((cboSkill1.SelectedIndex == -1 && cboSkill1.Visible) || (cboSkill2.SelectedIndex == -1 && cboSkill2.Visible) || (cboSkill3.SelectedIndex == -1 && cboSkill3.Visible))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectSkill"), LanguageManager.GetString("MessageTitle_Metatype_SelectSkill"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if ((cboSkill1.Visible && cboSkill2.Visible && cboSkill1.SelectedValue.ToString() == cboSkill2.SelectedValue.ToString()) ||
                (cboSkill1.Visible && cboSkill3.Visible && cboSkill1.SelectedValue.ToString() == cboSkill3.SelectedValue.ToString()) ||
                (cboSkill2.Visible && cboSkill3.Visible && cboSkill2.SelectedValue.ToString() == cboSkill3.SelectedValue.ToString()))
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_Duplicate"), LanguageManager.GetString("MessageTitle_Metatype_Duplicate"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool boolHalveAttributePriorityPoints = false;

            if (!string.IsNullOrEmpty(lstMetatypes.Text))
            {
                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
                
                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (cboCategory.SelectedValue.ToString() == "Shapeshifter" && cboMetavariant.SelectedValue.ToString() == "None")
                    cboMetavariant.SelectedValue = "Human";

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
                XmlNode objXmlMetavariant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");

                int intForce = 0;
                if (nudForce.Visible)
                    intForce = Convert.ToInt32(nudForce.Value);

                _objCharacter.MetatypeBP = Convert.ToInt32(lblMetavariantBP.Text);

                int intMinModifier = 0;
                int intMaxModifier = 0;
                //TODO: What the hell is this for?
                /*if (_strXmlFile == "critters.xml")
                {
                    if (cboCategory.SelectedValue.ToString() == "Technocritters")
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
                _objCharacter.RES.AssignLimits(ExpressionToString(charNode["resmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["resmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["resaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.EDG.AssignLimits(ExpressionToString(charNode["edgmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["edgmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["edgaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.ESS.AssignLimits(ExpressionToString(charNode["essmin"]?.InnerText, intForce, 0),              ExpressionToString(charNode["essmax"]?.InnerText, intForce, 0),              ExpressionToString(charNode["essaug"]?.InnerText, intForce, 0));
                _objCharacter.DEP.AssignLimits(ExpressionToString(charNode["depmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["depmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["depaug"]?.InnerText, intForce, intMaxModifier));
                if (charNode["halveattributepoints"] != null)
                    boolHalveAttributePriorityPoints = true;

                _objCharacter.Metatype = lstMetatypes.SelectedValue.ToString();
                _objCharacter.MetatypeCategory = cboCategory.SelectedValue.ToString();
                _objCharacter.Metavariant = cboMetavariant.SelectedValue.ToString() == "None" ? string.Empty : cboMetavariant.SelectedValue.ToString();

                // Load the Qualities file.
                XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

                // Determine if the Metatype has any bonuses.
                if (charNode?.InnerXml.Contains("bonus") == true)
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, lstMetatypes.SelectedValue.ToString(), charNode.SelectSingleNode("bonus"), false, 1, lstMetatypes.SelectedValue.ToString());

                List<Weapon> objWeapons = new List<Weapon>();

                // Create the Qualities that come with the Metatype.
                foreach (XmlNode objXmlQualityItem in charNode?.SelectNodes("qualities/positive/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = string.Empty;
                    if (objXmlQualityItem.Attributes["select"] != null)
                        strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                    QualitySource objSource = QualitySource.Metatype;
                    if (objXmlQualityItem.Attributes["removable"] != null)
                        objSource = QualitySource.MetatypeRemovable;
                    objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                    objQuality.ContributeToLimit = false;
                    _objCharacter.Qualities.Add(objQuality);
                }
                //Load any negative quality the character has. 
                foreach (XmlNode objXmlQualityItem in charNode.SelectNodes("qualities/negative/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = string.Empty;
                    if (objXmlQualityItem.Attributes["select"] != null)
                        strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                    QualitySource objSource = new QualitySource();
                    objSource = QualitySource.Metatype;
                    if (objXmlQualityItem.Attributes["removable"] != null)
                        objSource = QualitySource.MetatypeRemovable;
                    objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                    objQuality.ContributeToLimit = false;
                    _objCharacter.Qualities.Add(objQuality);
                }

                //Load any critter powers the character has. 
                objXmlDocument = XmlManager.Load("critterpowers.xml");
                foreach (XmlNode objXmlPower in charNode.SelectNodes("powers/power"))
                {
                    XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    CritterPower objPower = new CritterPower(_objCharacter);
                    string strForcedValue = string.Empty;
                    int intRating = 0;

                    if (objXmlPower.Attributes["rating"] != null)
                        intRating = Convert.ToInt32(objXmlPower.Attributes["rating"].InnerText);
                    if (objXmlPower.Attributes["select"] != null)
                        strForcedValue = objXmlPower.Attributes["select"].InnerText;

                    objPower.Create(objXmlCritterPower, objNode, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                //Load any natural weapons the character has. 
                foreach (XmlNode objXmlNaturalWeapon in charNode["naturalweapons"]?.SelectNodes("naturalweapon"))
                {
                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Name = objXmlNaturalWeapon["name"].InnerText;
                    objWeapon.Category = LanguageManager.GetString("Tab_Critter");
                    objWeapon.WeaponType = "Melee";
                    objWeapon.Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText);
                    objWeapon.Damage = objXmlNaturalWeapon["damage"].InnerText;
                    objWeapon.AP = objXmlNaturalWeapon["ap"].InnerText;
                    objWeapon.Mode = "0";
                    objWeapon.RC = "0";
                    objWeapon.Concealability = 0;
                    objWeapon.Avail = "0";
                    objWeapon.Cost = 0;
                    objWeapon.UseSkill = objXmlNaturalWeapon["useskill"].InnerText;
                    objWeapon.Source = objXmlNaturalWeapon["source"].InnerText;
                    objWeapon.Page = objXmlNaturalWeapon["page"].InnerText;

                    _objCharacter.Weapons.Add(objWeapon);
                }

                // begin priority based character settings
                // Load the Priority information.
                XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);

                // Set the character priority selections
                _objCharacter.MetatypePriority = cboHeritage.SelectedValue.ToString();
                _objCharacter.AttributesPriority = cboAttributes.SelectedValue.ToString();
                _objCharacter.SpecialPriority = cboTalent.SelectedValue.ToString();
                _objCharacter.SkillsPriority = cboSkills.SelectedValue.ToString();
                _objCharacter.ResourcesPriority = cboResources.SelectedValue.ToString();
                _objCharacter.TalentPriority = cboTalents.SelectedValue.ToString();
                _objCharacter.PriorityBonusSkillList.Clear();
                if (cboSkill1.Visible && cboSkill1.SelectedValue != null)
                {
                    _objCharacter.PriorityBonusSkillList.Add(cboSkill1.SelectedValue.ToString());
                }
                if (cboSkill2.Visible && cboSkill2.SelectedValue != null)
                {
                    _objCharacter.PriorityBonusSkillList.Add(cboSkill2.SelectedValue.ToString());
                }
                if (cboSkill3.Visible && cboSkill3.SelectedValue != null)
                {
                    _objCharacter.PriorityBonusSkillList.Add(cboSkill3.SelectedValue.ToString());
                }

                // Set starting nuyen
                XmlNode objXmResourceNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Resources\" and gameplayoption = \"" + _objCharacter.GameplayOption + "\" and value = \"" + cboResources.SelectedValue + "\"]");
                if (objXmResourceNode?["resources"] != null)
                {
                    _objCharacter.Nuyen = Convert.ToInt32(objXmResourceNode["resources"].InnerText);
                    _objCharacter.StartingNuyen = _objCharacter.Nuyen;
                }

                if ("Aspected Magician".Equals(cboTalents.SelectedValue))
                {
                    _objCharacter.Pushtext.Push((string)cboSkill1.SelectedValue);
                }

                if ("Enchanter".Equals(cboTalents.SelectedValue))
                {
                    _objCharacter.Pushtext.Push((string)cboSkill1.SelectedValue);
                }
                // Set starting positive qualities
                foreach (XmlNode objXmlQualityItem in objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[value = \"" + cboTalents.SelectedValue + "\"]/qualities/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = string.Empty;
                    if (objXmlQualityItem.Attributes["select"] != null)
                        strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                    QualitySource objSource = new QualitySource();
                    objSource = QualitySource.Metatype;
                    if (objXmlQualityItem.Attributes["removable"] != null)
                        objSource = QualitySource.MetatypeRemovable;
                    objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                    _objCharacter.Qualities.Add(objQuality);
                }

                XmlNode objXmlTalentPriorityNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[value = \"" + cboTalents.SelectedValue + "\"]");
                // Set starting magic
                if (objXmlTalentPriorityNode["magic"] != null)
                    _objCharacter.MAG.MetatypeMinimum = Convert.ToInt32(objXmlTalentPriorityNode["magic"].InnerText);
                if (objXmlTalentPriorityNode["spells"] != null)
                    _objCharacter.SpellLimit = Convert.ToInt32(objXmlTalentPriorityNode["spells"].InnerText);
                if (objXmlTalentPriorityNode["maxmagic"] != null)
                    _objCharacter.MAG.MetatypeMaximum = Convert.ToInt32(objXmlTalentPriorityNode["magic"].InnerText);
                // Set starting resonance
                if (objXmlTalentPriorityNode["resonance"] != null)
                    _objCharacter.RES.MetatypeMinimum = Convert.ToInt32(objXmlTalentPriorityNode["resonance"].InnerText);
                if (objXmlTalentPriorityNode["cfp"] != null)
                    _objCharacter.CFPLimit = Convert.ToInt32(objXmlTalentPriorityNode["cfp"].InnerText);
                if (objXmlTalentPriorityNode["maxresonance"] != null)
                    _objCharacter.RES.MetatypeMaximum = Convert.ToInt32(objXmlTalentPriorityNode["maxresonance"].InnerText);
                // Set starting depth
                if (objXmlTalentPriorityNode["depth"] != null)
                    _objCharacter.DEP.MetatypeMinimum = Convert.ToInt32(objXmlTalentPriorityNode["depth"].InnerText);
                if (objXmlTalentPriorityNode["ainormalprogramlimit"] != null)
                    _objCharacter.AINormalProgramLimit = Convert.ToInt32(objXmlTalentPriorityNode["ainormalprogramlimit"].InnerText);
                if (objXmlTalentPriorityNode["ainormalprogramlimit"] != null)
                    _objCharacter.AIAdvancedProgramLimit = Convert.ToInt32(objXmlTalentPriorityNode["aiadvancedprogramlimit"].InnerText);
                if (objXmlTalentPriorityNode["maxdepth"] != null)
                    _objCharacter.DEP.MetatypeMaximum = Convert.ToInt32(objXmlTalentPriorityNode["maxdepth"].InnerText);

                // Set Free Skills/Skill Groups
                XmlNode objTalentsNode = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Talent\" and value = \"" +
                                                            cboTalent.SelectedValue + "\"]/talents/talent[value = \"" +
                                                            cboTalents.SelectedValue + "\"]");
                if (objTalentsNode != null)
                {
                    int intFreeLevels = 0;
                    Improvement.ImprovementType type = Improvement.ImprovementType.SkillBase;
                    XmlNode objTalentSkillValNode = objTalentsNode.SelectSingleNode("skillval");
                    if (objTalentSkillValNode == null || !int.TryParse(objTalentSkillValNode.InnerText, out intFreeLevels))
                    {
                        objTalentSkillValNode = objTalentsNode.SelectSingleNode("skillgroupval");
                        if (objTalentSkillValNode != null && int.TryParse(objTalentSkillValNode.InnerText, out intFreeLevels))
                        {
                            type = Improvement.ImprovementType.SkillGroupBase;
                        }
                    }
                    AddFreeSkills(intFreeLevels, type);
                }

                // Set Special Attributes
                _objCharacter.Special = Convert.ToInt32(lblSpecial.Text);
                _objCharacter.TotalSpecial = _objCharacter.Special;

                // Set Attributes
                XmlNode objXmlAttributesPriority = objXmlDocumentPriority.SelectSingleNode("/chummer/priorities/priority[category = \"Attributes\" and value = \"" + cboAttributes.SelectedValue + "\"]");
                if (objXmlAttributesPriority?["attributes"] != null)
                {
                    _objCharacter.Attributes = Convert.ToInt32(objXmlAttributesPriority["attributes"].InnerText);
                    _objCharacter.TotalAttributes = _objCharacter.Attributes;
                    if (boolHalveAttributePriorityPoints)
                    {
                        _objCharacter.Attributes /= 2;
                        _objCharacter.TotalAttributes /= 2;
                    }
                }

                // Set Skills and Skill Groups
                XmlNodeList objXmlPriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Skills\" and value = \"" + cboSkills.SelectedValue + "\"]");
                foreach (XmlNode objXmlNode in objXmlPriorityList)
                {
                    if (objXmlNode["gameplayoption"] != null &&
                        objXmlNode["gameplayoption"].InnerText != _objCharacter.GameplayOption)
                    {
                        continue;
                    }
                    if (objXmlNode["skills"] != null)
                    {
                        _objCharacter.SkillsSection.SkillPointsMaximum =
                            Convert.ToInt32(objXmlNode["skills"].InnerText);
                        _objCharacter.SkillsSection.SkillGroupPointsMaximum =
                            Convert.ToInt32(objXmlNode["skillgroups"].InnerText);
                        break;
                    }
                }
                
                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in objWeapons)
                    _objCharacter.Weapons.Add(objWeapon);

                // Sprites can never have Physical Attributes
                if (_objCharacter.DEPEnabled || lstMetatypes.SelectedValue.ToString().EndsWith("Sprite"))
                {
                    _objCharacter.BOD.AssignLimits("0", "0", "0");
                    _objCharacter.AGI.AssignLimits("0", "0", "0");
                    _objCharacter.REA.AssignLimits("0", "0", "0");
                    _objCharacter.STR.AssignLimits("0", "0", "0");
                    _objCharacter.MAG.AssignLimits("0", "0", "0");
                }

                // Load the Priority information.
                XmlDocument objXmlDocumentGameplayOptions = XmlManager.Load("gameplayoptions.xml");
                XmlNode objXmlGameplayOption = objXmlDocumentGameplayOptions.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + _objCharacter.GameplayOption + "\"]");
                string strKarma = objXmlGameplayOption["karma"].InnerText;
                string strNuyen = objXmlGameplayOption["maxnuyen"].InnerText;
                string strContactMultiplier = objXmlGameplayOption["contactmultiplier"].InnerText;
                _objCharacter.MaxKarma = Convert.ToInt32(strKarma);
                _objCharacter.MaxNuyen = Convert.ToInt32(strNuyen);
                _objCharacter.ContactMultiplier = Convert.ToInt32(strContactMultiplier);

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
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddFreeSkills(int intFreeLevels, Improvement.ImprovementType type = Improvement.ImprovementType.SkillGroupBase)
        {
            if (cboSkill1.Visible && cboSkill1.SelectedValue != null)
            {
                if ("Aware" == cboTalents.SelectedValue.ToString())
                {
                    SkillsSection.FilterOptions skills = SkillsSection.FilterOptions.Name;
                    _objCharacter.SkillsSection.AddSkills(skills, cboSkill1.SelectedValue.ToString());
                    ImprovementManager.CreateImprovement(_objCharacter, skills.ToString(), Improvement.ImprovementSource.Heritage, "Heritage",
                        Improvement.ImprovementType.SpecialSkills, string.Empty);
                }
                ImprovementManager.CreateImprovement(_objCharacter, cboSkill1.SelectedValue.ToString(), Improvement.ImprovementSource.Heritage, "Heritage",
                    type, string.Empty, intFreeLevels);
            }

            if (cboSkill2.Visible && cboSkill2.SelectedValue != null)
            {
                ImprovementManager.CreateImprovement(_objCharacter, cboSkill2.SelectedValue.ToString(), Improvement.ImprovementSource.Heritage, "Heritage",
                    type, string.Empty, intFreeLevels);
            }

            if (cboSkill3.Visible && cboSkill3.SelectedValue != null)
            {
                ImprovementManager.CreateImprovement(_objCharacter, cboSkill3.SelectedValue.ToString(), Improvement.ImprovementSource.Heritage, "Heritage",
                    type, string.Empty, intFreeLevels);
            }

            ImprovementManager.Commit(_objCharacter);
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

                // Discover which priority rating is not currently assigned
                objPriorities.Remove(cboHeritage.SelectedValue.ToString());
                objPriorities.Remove(cboTalent.SelectedValue.ToString());
                objPriorities.Remove(cboAttributes.SelectedValue.ToString());
                objPriorities.Remove(cboSkills.SelectedValue.ToString());
                objPriorities.Remove(cboResources.SelectedValue.ToString());
                if (objPriorities.Count == 0)
                    return;

                string strMissing = objPriorities.First();

                // Find the combo with the same value as this one and change it to the missing value.
                //_blnInitializing = true;
                if (comboBox.Name != cboTalent.Name &&
                    cboTalent.SelectedValue.ToString() == comboBox.SelectedValue.ToString())
                    cboTalent.SelectedValue = strMissing;

                else if (comboBox.Name != cboHeritage.Name &&
                    cboHeritage.SelectedValue.ToString() == comboBox.SelectedValue.ToString())
                    cboHeritage.SelectedValue = strMissing;

                else if (comboBox.Name != cboSkills.Name &&
                    cboSkills.SelectedValue.ToString() == comboBox.SelectedValue.ToString())
                    cboSkills.SelectedValue = strMissing;

                else if (comboBox.Name != cboResources.Name &&
                    cboResources.SelectedValue.ToString() == comboBox.SelectedValue.ToString())
                    cboResources.SelectedValue = strMissing;

                else if (comboBox.Name != cboAttributes.Name &&
                    cboAttributes.SelectedValue.ToString() == comboBox.SelectedValue.ToString())
                    cboAttributes.SelectedValue = strMissing;
            }
        }

        private int SumtoTen()
        {
            int value = 0;
            value += Convert.ToInt32(cboHeritage.SelectedValue.ToString().Split(',')[intBuildMethod]);
            value += Convert.ToInt32(cboTalent.SelectedValue.ToString().Split(',')[intBuildMethod]);
            value += Convert.ToInt32(cboAttributes.SelectedValue.ToString().Split(',')[intBuildMethod]);
            value += Convert.ToInt32(cboSkills.SelectedValue.ToString().Split(',')[intBuildMethod]);
            value += Convert.ToInt32(cboResources.SelectedValue.ToString().Split(',')[intBuildMethod]);
            lblSumtoTen.Text = (value.ToString() + '/' + _objCharacter.SumtoTen.ToString());
            return value;
        }
        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        private string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            if (string.IsNullOrWhiteSpace(strIn))
                return intOffset.ToString();
            int intValue = 1;
            XmlDocument objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();
            XPathExpression xprAttribute = nav.Compile(strIn.Replace("/", " div ").Replace("F", intForce.ToString()).Replace("1D6", intForce.ToString()).Replace("2D6", intForce.ToString()));
            object xprEvaluateResult = null;
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                xprEvaluateResult = nav.Evaluate(xprAttribute);
            }
            catch (XPathException) { }
            if (xprEvaluateResult is double)
                intValue = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(xprEvaluateResult.ToString(), GlobalOptions.InvariantCultureInfo)));
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
            XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);

            List<ListItem> lstTalent = new List<ListItem>();

            // Populate the Priority Category list.
            XmlNodeList objXmlPriorityTalentList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent");
            foreach (XmlNode objXmlPriorityTalent in objXmlPriorityTalentList)
            {
                if (objXmlPriorityTalent.InnerXml.Contains("forbidden"))
                {
                    bool blnRequirementForbidden = false;

                    // Loop through the oneof requirements.
                    XmlNodeList objXmlForbiddenList = objXmlPriorityTalent.SelectNodes("forbidden/oneof");
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
                if (objXmlPriorityTalent.InnerXml.Contains("required"))
                {
                    bool blnRequirementMet = false;

                    // Loop through the oneof requirements.
                    XmlNodeList objXmlForbiddenList = objXmlPriorityTalent.SelectNodes("required/oneof");
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
                ListItem objItem = new ListItem();
                objItem.Value = objXmlPriorityTalent["value"].InnerText;
                objItem.Name = objXmlPriorityTalent["translate"]?.InnerText ?? objXmlPriorityTalent["name"].InnerText;
                lstTalent.Add(objItem);
            }

            SortListItem objSort = new SortListItem();
            lstTalent.Sort(objSort.Compare);
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

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        void PopulateMetatypes()
        {
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
            // Load the Priority information.
            XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);

            List<ListItem> lstMetatype = new List<ListItem>();

            XmlNodeList objXmlMetatypeList = objXmlDocument.SelectNodes("/chummer/metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + cboCategory.SelectedValue + "\"]");

            foreach (XmlNode objXmlMetatype in objXmlMetatypeList)
            {
                XmlNodeList objXmlMetatypePriorityList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + objXmlMetatype["name"]?.InnerText + "\"]");
                if (objXmlMetatypePriorityList.Count > 0)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlMetatype["name"]?.InnerText;
                    objItem.Name = objXmlMetatype["translate"]?.InnerText ?? objXmlMetatype["name"]?.InnerText;
                    lstMetatype.Add(objItem);
                }
            }
            SortListItem objSort = new SortListItem();
            lstMetatype.Sort(objSort.Compare);
            int intOldSelectedIndex = lstMetatypes.SelectedIndex;
            int intOldDataSourceSize = lstMetatypes.Items.Count;
            lstMetatypes.BeginUpdate();
            lstMetatypes.DataSource = null;
            lstMetatypes.ValueMember = "Value";
            lstMetatypes.DisplayMember = "Name";
            lstMetatypes.DataSource = lstMetatype;
            bool blnOldInitializing = _blnInitializing;
            _blnInitializing = true;
            if (intOldDataSourceSize == lstMetatypes.Items.Count)
            {
                lstMetatypes.SelectedIndex = intOldSelectedIndex;
            }
            else
                lstMetatypes.SelectedIndex = 0;
            lstMetatypes.EndUpdate();
            _blnInitializing = blnOldInitializing;

            if (cboCategory.SelectedValue.ToString().EndsWith("Spirits"))
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
            _lstCategory = new List<ListItem>();

            // Load the Metatype information.
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

            // Load the Priority information.
            XmlDocument objXmlDocumentPriority = XmlManager.Load(_strPrioritiesXmlFile);

            // Populate the Metatype Category list.
            XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category");

            // Create a list of any Categories that should not be in the list.
            List<string> lstRemoveCategory = new List<string>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                bool blnRemoveItem = true;

                string strXPath = "/chummer/metatypes/metatype[category = \"" + objXmlCategory.InnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")]";

                XmlNodeList objItems = objXmlDocument.SelectNodes(strXPath);

                if (objItems.Count > 0)
                {
                    blnRemoveItem = true;
                    // Remove metatypes not covered by heritage
                    foreach (XmlNode objItem in objItems)
                    {
                        XmlNodeList objXmlMetatypeList = objXmlDocumentPriority.SelectNodes("/chummer/priorities/priority[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + objItem["name"]?.InnerText + "\"]");
                        if (objXmlMetatypeList.Count > 0)
                            blnRemoveItem = false;
                    }
                }

                if (blnRemoveItem)
                    lstRemoveCategory.Add(objXmlCategory.InnerText);
            }

            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                // Make sure the Category isn't in the exclusion list.
                bool blnAddItem = true;
                foreach (string strCategory in lstRemoveCategory)
                {
                    if (strCategory == objXmlCategory.InnerText)
                        blnAddItem = false;
                }
                // Also make sure it is not already in the Category list.
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == objXmlCategory.InnerText)
                        blnAddItem = false;
                }

                if (blnAddItem)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlCategory.InnerText;
                    objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                    _lstCategory.Add(objItem);
                }
            }

            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            if (cboCategory.Items.Contains("Metahuman"))
            {
                cboCategory.SelectedValue = "Metahuman";
            }
            else
            {
                cboCategory.SelectedIndex = 0;
            }
            cboCategory.EndUpdate();

            Height = cmdOK.Bottom + 40;
            lstMetatypes.Height = cmdOK.Bottom - lstMetatypes.Top;

            PopulateMetatypes();
        }

        private XmlNode GetSpecificSkill(string strSkill)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            var objXmlSkill = objXmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + strSkill + "\"]");
            return objXmlSkill;
        }

        private XmlNodeList GetMatrixSkillList()
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            var objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill[skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
            return objXmlSkillList;
        }

        private XmlNode GetSpecificSkillGroup(string strSkill)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            var objXmlSkill = objXmlSkillsDocument.SelectSingleNode("/chummer/skillgroups/name[. = \"" + strSkill + "\"]");
            return objXmlSkill;
        }

        private XmlNodeList GetMagicalSkillList(XmlNodeList objNodeList = null)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            var objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill[category = \"Magical Active\" or category = \"Pseudo-Magical Active\"]");
            return objXmlSkillList;
        }

        private XmlNodeList GetResonanceSkillList()
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            var objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill[category = \"Resonance Active\" or skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]");
            return objXmlSkillList;
        }

        private XmlNodeList GetActiveSkillList()
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            var objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill");
            return objXmlSkillList;
        }

        private XmlNodeList BuildSkillCategoryList(XmlNodeList objSkillList)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            string strGroups = "/chummer/skillgroups/name[. = \"" + objSkillList[0].InnerText + "\"";
            for (int i = 1; i < objSkillList.Count; i++)
            {
                strGroups += " or . = \"" + objSkillList[i].InnerText + "\"";
            }
            strGroups += "]";
            var objXmlSkillList = objXmlSkillsDocument.SelectNodes(strGroups);
            return objXmlSkillList;
        }

        private XmlNodeList BuildSkillList(XmlNodeList objSkillList)
        {
            XmlDocument objXmlSkillsDocument = XmlManager.Load("skills.xml");
            string strGroups = "/chummer/skills/skill[name = \"" + objSkillList[0].InnerText + "\"";
            for (int i = 1; i < objSkillList.Count; i++)
            {
                strGroups += " or name = \"" + objSkillList[i].InnerText + "\"";
            }
            strGroups += "]";
            var objXmlSkillList = objXmlSkillsDocument.SelectNodes(strGroups);
            return objXmlSkillList;
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion
    }
}

