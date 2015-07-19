using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSumtoTenMetatype : Form
    {
        private readonly Character _objCharacter;

        private string _strXmlFile = "metatypes.xml";
        private string _strSumtoTenXmlFile = "SumtoTen.xml";

        private List<ListItem> _lstCategory = new List<ListItem>();
        private bool _blnInitializing = false;

        #region Character Events
        private void objCharacter_MAGEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_RESEnabledChanged(object sender)
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
        #endregion

        #region Form Events
        public frmSumtoTenMetatype(Character objCharacter)
        {

            _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            // Attach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged += objCharacter_MAGEnabledChanged;
            _objCharacter.RESEnabledChanged += objCharacter_RESEnabledChanged;
            _objCharacter.AdeptTabEnabledChanged += objCharacter_AdeptTabEnabledChanged;
            _objCharacter.MagicianTabEnabledChanged += objCharacter_MagicianTabEnabledChanged;
            _objCharacter.TechnomancerTabEnabledChanged += objCharacter_TechnomancerTabEnabledChanged;
            _objCharacter.InitiationTabEnabledChanged += objCharacter_InitiationTabEnabledChanged;
            _objCharacter.CritterTabEnabledChanged += objCharacter_CritterTabEnabledChanged;
        }

        private void frmSumtoTenMetatype_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Detach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged -= objCharacter_MAGEnabledChanged;
            _objCharacter.RESEnabledChanged -= objCharacter_RESEnabledChanged;
            _objCharacter.AdeptTabEnabledChanged -= objCharacter_AdeptTabEnabledChanged;
            _objCharacter.MagicianTabEnabledChanged -= objCharacter_MagicianTabEnabledChanged;
            _objCharacter.TechnomancerTabEnabledChanged -= objCharacter_TechnomancerTabEnabledChanged;
            _objCharacter.InitiationTabEnabledChanged -= objCharacter_InitiationTabEnabledChanged;
            _objCharacter.CritterTabEnabledChanged -= objCharacter_CritterTabEnabledChanged;
        }

        private void frmSumtoTenMetatype_Load(object sender, EventArgs e)
        {

            // Load the SumtoTen information.
            XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

            if (_objCharacter.GameplayOption == "")
                _objCharacter.GameplayOption = "Standard";

            // Populate the SumtoTen Category list.
            _blnInitializing = true;
            XmlNodeList objXmlSumtoTenCategoryList = objXmlDocumentSumtoTen.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlSumtoTenCategory in objXmlSumtoTenCategoryList)
            {
                string strXPath = "";
                if (objXmlSumtoTenCategory.InnerText == "Resources")
                    strXPath = "/chummer/sum10priorities/SumtoTen[category = \"" + objXmlSumtoTenCategory.InnerText + "\" and gameplayoption = \"" + _objCharacter.GameplayOption + "\"]";
                else
                    strXPath = "/chummer/sum10priorities/SumtoTen[category = \"" + objXmlSumtoTenCategory.InnerText + "\"]";
                XmlNodeList objItems = objXmlDocumentSumtoTen.SelectNodes(strXPath);
                if (objItems.Count > 0)
                {
                    List<ListItem> lstItems = new List<ListItem>();
                    // lstItems.Add(new ListItem());
                    foreach (XmlNode objXmlSumtoTen in objItems)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objXmlSumtoTen["value"].InnerText;
                        objItem.Name = objXmlSumtoTen["name"].InnerText;
                        lstItems.Add(objItem);
                    }
                    SortListItem objSumtoTenSort = new SortListItem();
                    lstItems.Sort(objSumtoTenSort.Compare);
                    switch (objXmlSumtoTenCategory.InnerText)
                    {
                        case "Heritage":
                            cboHeritage.ValueMember = "Value";
                            cboHeritage.DisplayMember = "Name";
                            cboHeritage.DataSource = lstItems;
                            break;
                        case "Talent":
                            cboTalent.ValueMember = "Value";
                            cboTalent.DisplayMember = "Name";
                            cboTalent.DataSource = lstItems;
                            break;
                        case "Attributes":
                            cboAttributes.ValueMember = "Value";
                            cboAttributes.DisplayMember = "Name";
                            cboAttributes.DataSource = lstItems;
                            break;
                        case "Skills":
                            cboSkills.ValueMember = "Value";
                            cboSkills.DisplayMember = "Name";
                            cboSkills.DataSource = lstItems;
                            break;
                        case "Resources":
                            cboResources.ValueMember = "Value";
                            cboResources.DisplayMember = "Name";
                            cboResources.DataSource = lstItems;
                            break;
                        default:
                            break;
                    }
                }
            }

            // Set SumtoTen defaults.
            cboHeritage.SelectedIndex = 4;
            cboTalent.SelectedIndex = 3;
            cboAttributes.SelectedIndex = 2;
            cboSkills.SelectedIndex = 1;
            cboResources.SelectedIndex = 0;
            _blnInitializing = false;

            // Load Metatypes
            LoadMetatypes();
            PopulateTalents();
            lstMetatypes.SelectedIndex = 0;

            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            tipTooltip.SetToolTip(chkPossessionBased, LanguageManager.Instance.GetString("Tip_Metatype_PossessionTradition"));
            tipTooltip.SetToolTip(chkBloodSpirit, LanguageManager.Instance.GetString("Tip_Metatype_BloodSpirit"));

            XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
            XmlNode objXmlPossession = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Possession\"]");
            XmlNode objXmlInhabitation = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Inhabitation\"]");
            List<ListItem> lstMethods = new List<ListItem>();

            ListItem objPossession = new ListItem();
            objPossession.Value = "Possession";
            if (objXmlPossession["translate"] != null)
                objPossession.Name = objXmlPossession["translate"].InnerText;
            else
                objPossession.Name = objXmlPossession["name"].InnerText;

            ListItem objInhabitation = new ListItem();
            objInhabitation.Value = "Inhabitation";
            if (objXmlInhabitation["translate"] != null)
                objInhabitation.Name = objXmlInhabitation["translate"].InnerText;
            else
                objInhabitation.Name = objXmlInhabitation["name"].InnerText;

            lstMethods.Add(objInhabitation);
            lstMethods.Add(objPossession);

            SortListItem objSortPossession = new SortListItem();
            lstMethods.Sort(objSortPossession.Compare);
            cboPossessionMethod.ValueMember = "Value";
            cboPossessionMethod.DisplayMember = "Name";
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.SelectedIndex = cboPossessionMethod.FindStringExact(objPossession.Name);
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the SumtoTen information.
            XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

            // Don't attempt to do anything if nothing is selected.
            if (lstMetatypes.Text != "")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
				XmlNode objXmlMetatypeBP = objXmlDocumentSumtoTen.SelectSingleNode("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");

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
                objNone.Name = LanguageManager.Instance.GetString("String_None");
                lstMetavariants.Add(objNone);

                // Retrieve the list of Metavariants for the selected Metatype.
                XmlNodeList objXmlMetavariantList = objXmlMetatype.SelectNodes("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]");
                foreach (XmlNode objXmlMetavariant in objXmlMetavariantList)
                {
                    ListItem objMetavariant = new ListItem();
                    objMetavariant.Value = objXmlMetavariant["name"].InnerText;
                    if (objXmlMetavariant["translate"] != null)
                        objMetavariant.Name = objXmlMetavariant["translate"].InnerText;
                    else
                        objMetavariant.Name = objXmlMetavariant["name"].InnerText;
                    lstMetavariants.Add(objMetavariant);
                }

                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;

                // Select the None item.
                cboMetavariant.SelectedIndex = 0;

                // Set the special attributes label.
                XmlNodeList objXmlMetatypeList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]");
                lblSpecial.Text = objXmlMetatypeList[0]["value"].InnerText.ToString();

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
                        lblForceLabel.Text = LanguageManager.Instance.GetString("String_Force");
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
                objNone.Name = LanguageManager.Instance.GetString("String_None");
                lstMetavariants.Add(objNone);

                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
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


            string strLabel = LanguageManager.Instance.GetString("String_MetamagicSkillBase");

            if (cboTalents.SelectedIndex >= 0)
            {
                if (cboTalent.SelectedValue.ToString() == "4")
                {
                    if (cboTalents.SelectedValue.ToString() == "Magician" || cboTalents.SelectedValue.ToString() == "Mystic Adept")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillMagicianA"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();
                        List<ListItem> lstSkills2 = new List<ListItem>();
                        XmlNodeList objXmlMagicalSkills = GetMagicalSkillList();
                        foreach (XmlNode objXmlSkill in objXmlMagicalSkills)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlSkill["name"].InnerText;
                            objItem.Name = objXmlSkill["name"].InnerText;
                            lstSkills1.Add(objItem);
                            lstSkills2.Add(objItem);
                        }
                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        cboSkill2.ValueMember = "Value";
                        cboSkill2.DisplayMember = "Name";
                        cboSkill2.DataSource = lstSkills2;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = true;
                    }
                    else if (cboTalents.SelectedValue.ToString() == "Technomancer")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillTechnomancerA"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();
                        List<ListItem> lstSkills2 = new List<ListItem>();
                        XmlNodeList objXmlResonanceSkills = GetResonanceSkillList();
                        foreach (XmlNode objXmlSkill in objXmlResonanceSkills)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlSkill["name"].InnerText;
                            objItem.Name = objXmlSkill["name"].InnerText;
                            lstSkills1.Add(objItem);
                            lstSkills2.Add(objItem);
                        }
                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        cboSkill2.ValueMember = "Value";
                        cboSkill2.DisplayMember = "Name";
                        cboSkill2.DataSource = lstSkills2;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = true;
                    }
                    else
                    {
                        lblMetatypeSkillSelection.Visible = false;
                        cboSkill1.Visible = false;
                        cboSkill2.Visible = false;
                    }
                }
                else if (cboTalent.SelectedValue.ToString() == "3")
                {
                    if (cboTalents.SelectedValue.ToString() == "Magician" || cboTalents.SelectedValue.ToString() == "Mystic Adept")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillMagicianB"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();
                        List<ListItem> lstSkills2 = new List<ListItem>();
                        XmlNodeList objXmlMagicalSkills = GetMagicalSkillList();
                        foreach (XmlNode objXmlSkill in objXmlMagicalSkills)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlSkill["name"].InnerText;
                            objItem.Name = objXmlSkill["name"].InnerText;
                            lstSkills1.Add(objItem);
                            lstSkills2.Add(objItem);
                        }
                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        cboSkill2.ValueMember = "Value";
                        cboSkill2.DisplayMember = "Name";
                        cboSkill2.DataSource = lstSkills2;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = true;
                    }
                    else if (cboTalents.SelectedValue.ToString() == "Technomancer")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillTechnomancerB"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();
                        List<ListItem> lstSkills2 = new List<ListItem>();
                        XmlNodeList objXmlResonanceSkills = GetResonanceSkillList();
                        foreach (XmlNode objXmlSkill in objXmlResonanceSkills)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlSkill["name"].InnerText;
                            objItem.Name = objXmlSkill["name"].InnerText;
                            lstSkills1.Add(objItem);
                            lstSkills2.Add(objItem);
                        }
                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        cboSkill2.ValueMember = "Value";
                        cboSkill2.DisplayMember = "Name";
                        cboSkill2.DataSource = lstSkills2;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = true;
                    }
                    else if (cboTalents.SelectedValue.ToString() == "Adept")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillAdeptB"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();
                        XmlNodeList objXmlActiveSkills = GetActiveSkillList();
                        foreach (XmlNode objXmlSkill in objXmlActiveSkills)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlSkill["name"].InnerText;
                            objItem.Name = objXmlSkill["name"].InnerText;
                            lstSkills1.Add(objItem);
                        }
                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = false;
                    }
                    else if (cboTalents.SelectedValue.ToString() == "Aspected Magician")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillAspectedB"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();

                        ListItem objItem = new ListItem();
                        objItem.Value = "Conjuring";
                        objItem.Name = "Conjuring";
                        lstSkills1.Add(objItem);

                        objItem = new ListItem();
                        objItem.Value = "Enchanting";
                        objItem.Name = "Enchanting";
                        lstSkills1.Add(objItem);

                        objItem = new ListItem();
                        objItem.Value = "Sorcery";
                        objItem.Name = "Sorcery";
                        lstSkills1.Add(objItem);

                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = false;
                    }
                    else
                    {
                        lblMetatypeSkillSelection.Visible = false;
                        cboSkill1.Visible = false;
                        cboSkill2.Visible = false;
                    }
                }
                else if (cboTalent.SelectedValue.ToString() == "2")
                {
                    if (cboTalents.SelectedValue.ToString() == "Adept")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillAdeptC"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();
                        XmlNodeList objXmlActiveSkills = GetActiveSkillList();
                        foreach (XmlNode objXmlSkill in objXmlActiveSkills)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlSkill["name"].InnerText;
                            objItem.Name = objXmlSkill["name"].InnerText;
                            lstSkills1.Add(objItem);
                        }
                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = false;
                    }
                    else if (cboTalents.SelectedValue.ToString() == "Aspected Magician")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillAspectedC"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();

                        ListItem objItem = new ListItem();
                        objItem.Value = "Conjuring";
                        objItem.Name = "Conjuring";
                        lstSkills1.Add(objItem);

                        objItem = new ListItem();
                        objItem.Value = "Enchanting";
                        objItem.Name = "Enchanting";
                        lstSkills1.Add(objItem);

                        objItem = new ListItem();
                        objItem.Value = "Sorcery";
                        objItem.Name = "Sorcery";
                        lstSkills1.Add(objItem);

                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = false;
                    }
                    else
                    {
                        lblMetatypeSkillSelection.Visible = false;
                        cboSkill1.Visible = false;
                        cboSkill2.Visible = false;
                    }
                }
                else if (cboTalent.SelectedValue.ToString() == "1")
                {
                    if (cboTalents.SelectedValue.ToString() == "Aspected Magician")
                    {
                        strLabel = String.Format(strLabel, LanguageManager.Instance.GetString("String_MetamagicSkillAspectedD"));
                        lblMetatypeSkillSelection.Text = strLabel;

                        List<ListItem> lstSkills1 = new List<ListItem>();

                        ListItem objItem = new ListItem();
                        objItem.Value = "Conjuring";
                        objItem.Name = "Conjuring";
                        lstSkills1.Add(objItem);

                        objItem = new ListItem();
                        objItem.Value = "Enchanting";
                        objItem.Name = "Enchanting";
                        lstSkills1.Add(objItem);

                        objItem = new ListItem();
                        objItem.Value = "Sorcery";
                        objItem.Name = "Sorcery";
                        lstSkills1.Add(objItem);

                        cboSkill1.ValueMember = "Value";
                        cboSkill1.DisplayMember = "Name";
                        cboSkill1.DataSource = lstSkills1;

                        lblMetatypeSkillSelection.Visible = true;
                        cboSkill1.Visible = true;
                        cboSkill2.Visible = false;
                    }
                    else
                    {
                        lblMetatypeSkillSelection.Visible = false;
                        cboSkill1.Visible = false;
                        cboSkill2.Visible = false;
                    }
                }
                else
                {
                    lblMetatypeSkillSelection.Visible = false;
                    cboSkill1.Visible = false;
                    cboSkill2.Visible = false;
                }
            }
            else
            {
                lblMetatypeSkillSelection.Visible = false;
                cboSkill1.Visible = false;
                cboSkill2.Visible = false;
            }
        }

        private void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);
            XmlDocument objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");
            XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

            if (cboMetavariant.SelectedValue.ToString() != "None")
            {
                XmlNode objXmlMetavariant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");
                XmlNode objXmlMetavariantBP = objXmlDocumentSumtoTen.SelectSingleNode("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]");
                lblMetavariantBP.Text = objXmlMetavariantBP["karma"].InnerText;
                lblBOD.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["bodmin"].InnerText, objXmlMetavariant["bodmax"].InnerText, objXmlMetavariant["bodaug"].InnerText);
                lblAGI.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["agimin"].InnerText, objXmlMetavariant["agimax"].InnerText, objXmlMetavariant["agiaug"].InnerText);
                lblREA.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["reamin"].InnerText, objXmlMetavariant["reamax"].InnerText, objXmlMetavariant["reaaug"].InnerText);
                lblSTR.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["strmin"].InnerText, objXmlMetavariant["strmax"].InnerText, objXmlMetavariant["straug"].InnerText);
                lblCHA.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["chamin"].InnerText, objXmlMetavariant["chamax"].InnerText, objXmlMetavariant["chaaug"].InnerText);
                lblINT.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["intmin"].InnerText, objXmlMetavariant["intmax"].InnerText, objXmlMetavariant["intaug"].InnerText);
                lblLOG.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["logmin"].InnerText, objXmlMetavariant["logmax"].InnerText, objXmlMetavariant["logaug"].InnerText);
                lblWIL.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["wilmin"].InnerText, objXmlMetavariant["wilmax"].InnerText, objXmlMetavariant["wilaug"].InnerText);
                lblINI.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["inimin"].InnerText, objXmlMetavariant["inimax"].InnerText, objXmlMetavariant["iniaug"].InnerText);
                // Set the special attributes label.
                int intSpecial = 0;
                //XmlNode objXmlMetavariantList = objXmlDocumentSumtoTen.SelectSingleNode("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]" + "/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]");
                if (cboHeritage.SelectedIndex < 4)
                {
                    try
                    {
                        XmlNodeList objXmlMetavariantList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue.ToString() + "\"]");
                        intSpecial = Convert.ToInt32(objXmlMetavariantList[0]["value"].InnerText);
                        lblSpecial.Text = objXmlMetavariantList[0]["value"].InnerText.ToString();
                    }
                    catch { }
                }


                string strQualities = "";
                // Build a list of the Metavariant's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/positive/quality"))
                {
                    try
                    {
                        if (GlobalOptions.Instance.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objQuality["translate"] != null)
                                strQualities += objQuality["translate"].InnerText;
                            else
                                strQualities += objXmlQuality.InnerText;

                            if (objXmlQuality.Attributes["select"].InnerText != "")
                                strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                        }
                        else
                        {
                            strQualities += objXmlQuality.InnerText;
                            if (objXmlQuality.Attributes["select"].InnerText != "")
                                strQualities += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                        }
                    }
                    catch
                    {
                    }
                    strQualities += "\n";
                }
                // Build a list of the Metavariant's Negative Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/negative/quality"))
                {
                    try
                    {
                        if (GlobalOptions.Instance.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objQuality["translate"] != null)
                                strQualities += objQuality["translate"].InnerText;
                            else
                                strQualities += objXmlQuality.InnerText;

                            if (objXmlQuality.Attributes["select"].InnerText != "")
                                strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                        }
                        else
                        {
                            strQualities += objXmlQuality.InnerText;
                            if (objXmlQuality.Attributes["select"].InnerText != "")
                                strQualities += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                        }
                    }
                    catch
                    {
                    }
                    strQualities += "\n";
                }
                lblMetavariantQualities.Text = strQualities;
            }
            else
            {
                // Set the special attributes label.
                if (lstMetatypes.SelectedItem != null)
                {
                    XmlNodeList objXmlMetatypeList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue.ToString() + "\"]");
                    lblSpecial.Text = objXmlMetatypeList[0]["value"].InnerText.ToString();
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
                    lblMetavariantQualities.Text = "None";
                }
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private int SumtoTen()
        {
            int value = 0;
            value += Convert.ToInt32(cboHeritage.SelectedValue.ToString());
            value += Convert.ToInt32(cboTalent.SelectedValue.ToString());
            value += Convert.ToInt32(cboAttributes.SelectedValue.ToString());
            value += Convert.ToInt32(cboSkills.SelectedValue.ToString());
            value += Convert.ToInt32(cboResources.SelectedValue.ToString());
            return value;
        } 

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateMetatypes();
        }

        private void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;
            
            _blnInitializing = false;

            string strMetatype = "";
            if (lstMetatypes.SelectedIndex >= 0)
                strMetatype = lstMetatypes.SelectedValue.ToString();
            LoadMetatypes();
            lstMetatypes.SelectedValue = strMetatype;
            PopulateTalents();
            SumtoTen();

            if (cboTalent.SelectedValue.ToString() == "0")
                cboTalents.SelectedIndex = 0;
        }

        private void cboTalent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;
            _blnInitializing = false;
            string strMetatype = "";
            if (lstMetatypes.SelectedIndex >= 0)
                strMetatype = lstMetatypes.SelectedValue.ToString();
            LoadMetatypes();
            lstMetatypes.SelectedValue = strMetatype;
            PopulateTalents();
            SumtoTen();
            cboTalents.SelectedIndex = -1;

            if (cboTalent.SelectedValue.ToString() == "0")
                cboTalents.SelectedIndex = 0;
        }

        private void cboAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;

            _blnInitializing = false;

            string strMetatype = "";
            if (lstMetatypes.SelectedIndex >= 0)
                strMetatype = lstMetatypes.SelectedValue.ToString();
            LoadMetatypes();
            SumtoTen();
            lstMetatypes.SelectedValue = strMetatype;
            PopulateTalents();

            if (cboTalent.SelectedValue.ToString() == "0")
                cboTalents.SelectedIndex = 0;
        }

        private void cboSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;
            _blnInitializing = false;
            string strMetatype = "";
            if (lstMetatypes.SelectedIndex >= 0)
                strMetatype = lstMetatypes.SelectedValue.ToString();
            LoadMetatypes();
            SumtoTen();
            lstMetatypes.SelectedValue = strMetatype;
            PopulateTalents();

            if (cboTalent.SelectedValue.ToString() == "0")
                cboTalents.SelectedIndex = 0;
        }

        private void cboResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnInitializing)
                return;
            string strMetatype = "";
            if (lstMetatypes.SelectedIndex >= 0)
                strMetatype = lstMetatypes.SelectedValue.ToString();
            LoadMetatypes();
            SumtoTen();
            lstMetatypes.SelectedValue = strMetatype;
            PopulateTalents();

            if (cboTalent.SelectedValue.ToString() == "0")
                cboTalents.SelectedIndex = 0;
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        void MetatypeSelected()
        {
            if (SumtoTen() != _objCharacter.SumtoTen) 
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_SumtoTen").Replace("{0}", (_objCharacter.SumtoTen.ToString())).Replace("{1}", (SumtoTen().ToString())));
                return;
            }
            if (cboTalents.SelectedIndex == -1)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_Metatype_SelectTalent"), LanguageManager.Instance.GetString("MessageTitle_Metatype_SelectTalent"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if ((cboSkill1.SelectedIndex == -1 && cboSkill1.Visible) || (cboSkill2.SelectedIndex == -1 && cboSkill2.Visible))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_Metatype_SelectSkill"), LanguageManager.Instance.GetString("MessageTitle_Metatype_SelectSkill"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (cboSkill1.Visible && cboSkill2.Visible && cboSkill1.SelectedValue.ToString() == cboSkill2.SelectedValue.ToString())
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_Metatype_Duplicate"), LanguageManager.Instance.GetString("MessageTitle_Metatype_Duplicate"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (lstMetatypes.Text != "")
            {
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
                XmlNode objXmlMetavariant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");

                int intForce = 0;
                if (nudForce.Visible)
                    intForce = Convert.ToInt32(nudForce.Value);

				_objCharacter.MetatypeBP = Convert.ToInt32(lblMetavariantBP.Text);

				// Set Metatype information.
				if (cboMetavariant.SelectedValue.ToString() != "None")
                {
                    _objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetavariant["bodmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["bodmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["bodaug"].InnerText, intForce, 0));
                    _objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetavariant["agimin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["agimax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["agiaug"].InnerText, intForce, 0));
                    _objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetavariant["reamin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["reamax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["reaaug"].InnerText, intForce, 0));
                    _objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetavariant["strmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["strmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["straug"].InnerText, intForce, 0));
                    _objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetavariant["chamin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["chamax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["chaaug"].InnerText, intForce, 0));
                    _objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetavariant["intmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["intmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["intaug"].InnerText, intForce, 0));
                    _objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetavariant["logmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["logmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["logaug"].InnerText, intForce, 0));
                    _objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetavariant["wilmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["wilmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["wilaug"].InnerText, intForce, 0));
                    _objCharacter.INI.AssignLimits(ExpressionToString(objXmlMetavariant["inimin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["inimax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["iniaug"].InnerText, intForce, 0));
                    _objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetavariant["magmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["magmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["magaug"].InnerText, intForce, 0));
                    _objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetavariant["resmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["resmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["resaug"].InnerText, intForce, 0));
                    _objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetavariant["edgmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["edgmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["edgaug"].InnerText, intForce, 0));
                    _objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetavariant["essmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["essmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["essaug"].InnerText, intForce, 0));
                }
                else if (_strXmlFile != "critters.xml" || lstMetatypes.SelectedValue.ToString() == "Ally Spirit")
                {
                    _objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodaug"].InnerText, intForce, 0));
                    _objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agimax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agiaug"].InnerText, intForce, 0));
                    _objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reamax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reaaug"].InnerText, intForce, 0));
                    _objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["strmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["straug"].InnerText, intForce, 0));
                    _objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chamax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chaaug"].InnerText, intForce, 0));
                    _objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intaug"].InnerText, intForce, 0));
                    _objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logaug"].InnerText, intForce, 0));
                    _objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilaug"].InnerText, intForce, 0));
                    _objCharacter.INI.AssignLimits(ExpressionToString(objXmlMetatype["inimin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["inimax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["iniaug"].InnerText, intForce, 0));
                    _objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magaug"].InnerText, intForce, 0));
                    _objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resaug"].InnerText, intForce, 0));
                    _objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgaug"].InnerText, intForce, 0));
                    _objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"].InnerText, intForce, 0));
                }
                else
                {
                    int intMinModifier = -3;
                    if (cboCategory.SelectedValue.ToString() == "Mutant Critters")
                        intMinModifier = 0;
                    _objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 3));
                    _objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 3));
                    _objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 3));
                    _objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 3));
                    _objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 3));
                    _objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 3));
                    _objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 3));
                    _objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 3));
                    _objCharacter.INI.AssignLimits(ExpressionToString(objXmlMetatype["inimin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["inimax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["iniaug"].InnerText, intForce, 0));
                    _objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 3));
                    _objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 3));
                    _objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 3));
                    _objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"].InnerText, intForce, 0));
                }

                // If we're working with a Critter, set the Attributes to their default values.
                if (_strXmlFile == "critters.xml")
                {
                    _objCharacter.BOD.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 0));
                    _objCharacter.AGI.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 0));
                    _objCharacter.REA.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 0));
                    _objCharacter.STR.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 0));
                    _objCharacter.CHA.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 0));
                    _objCharacter.INT.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 0));
                    _objCharacter.LOG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 0));
                    _objCharacter.WIL.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 0));
                    _objCharacter.MAG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 0));
                    _objCharacter.RES.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 0));
                    _objCharacter.EDG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 0));
                    _objCharacter.ESS.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0));
                }

                // Sprites can never have Physical Attributes or WIL.
                if (lstMetatypes.SelectedValue.ToString().EndsWith("Sprite"))
                {
                    _objCharacter.BOD.AssignLimits("0", "0", "0");
                    _objCharacter.AGI.AssignLimits("0", "0", "0");
                    _objCharacter.REA.AssignLimits("0", "0", "0");
                    _objCharacter.STR.AssignLimits("0", "0", "0");
                    _objCharacter.WIL.AssignLimits("0", "0", "0");
                    _objCharacter.INI.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["inimax"].InnerText, intForce, 0));
                    _objCharacter.INI.MetatypeMaximum = Convert.ToInt32(ExpressionToString(objXmlMetatype["inimax"].InnerText, intForce, 0));
                }

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (cboCategory.SelectedValue.ToString() == "Shapeshifter" && cboMetavariant.SelectedValue.ToString() == "None")
                    cboMetavariant.SelectedValue = "Human";

                _objCharacter.Metatype = lstMetatypes.SelectedValue.ToString();
                _objCharacter.MetatypeCategory = cboCategory.SelectedValue.ToString();
                if (cboMetavariant.SelectedValue.ToString() == "None")
                {
                    _objCharacter.Metavariant = "";
                }
                else
                {
                    _objCharacter.Metavariant = cboMetavariant.SelectedValue.ToString();
                }

                if (objXmlMetatype["movement"] != null) // TODO: Replace with Walk/Run
                    _objCharacter.Movement = objXmlMetatype["movement"].InnerText;

                // Load the Qualities file.
                XmlDocument objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");

                if (cboMetavariant.SelectedValue.ToString() == "None")
                {
                    // Determine if the Metatype has any bonuses.
                    if (objXmlMetatype.InnerXml.Contains("bonus"))
                        objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, lstMetatypes.SelectedValue.ToString(), objXmlMetatype.SelectSingleNode("bonus"), false, 1, lstMetatypes.SelectedValue.ToString());

                    // Create the Qualities that come with the Metatype.
                    foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        Quality objQuality = new Quality(_objCharacter);
                        string strForceValue = "";
                        if (objXmlQualityItem.Attributes["select"] != null)
                            strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                        QualitySource objSource = new QualitySource();
                        objSource = QualitySource.Metatype;
                        if (objXmlQualityItem.Attributes["removable"] != null)
                            objSource = QualitySource.MetatypeRemovable;
                        objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                        objQuality.ContributeToLimit = false;
                        _objCharacter.Qualities.Add(objQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                    foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        Quality objQuality = new Quality(_objCharacter);
                        string strForceValue = "";
                        if (objXmlQualityItem.Attributes["select"] != null)
                            strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                        QualitySource objSource = new QualitySource();
                        objSource = QualitySource.Metatype;
                        if (objXmlQualityItem.Attributes["removable"] != null)
                            objSource = QualitySource.MetatypeRemovable;
                        objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                        objQuality.ContributeToLimit = false;
                        _objCharacter.Qualities.Add(objQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                }

                // If a Metavariant has been selected, locate it in the file.
                if (cboMetavariant.SelectedValue.ToString() != "None")
                {
                    lblMetavariantBP.Text = objXmlMetavariant["karma"].InnerText;
                    lblBOD.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["bodmin"].InnerText, objXmlMetavariant["bodmax"].InnerText, objXmlMetavariant["bodaug"].InnerText);
                    lblAGI.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["agimin"].InnerText, objXmlMetavariant["agimax"].InnerText, objXmlMetavariant["agiaug"].InnerText);
                    lblREA.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["reamin"].InnerText, objXmlMetavariant["reamax"].InnerText, objXmlMetavariant["reaaug"].InnerText);
                    lblSTR.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["strmin"].InnerText, objXmlMetavariant["strmax"].InnerText, objXmlMetavariant["straug"].InnerText);
                    lblCHA.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["chamin"].InnerText, objXmlMetavariant["chamax"].InnerText, objXmlMetavariant["chaaug"].InnerText);
                    lblINT.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["intmin"].InnerText, objXmlMetavariant["intmax"].InnerText, objXmlMetavariant["intaug"].InnerText);
                    lblLOG.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["logmin"].InnerText, objXmlMetavariant["logmax"].InnerText, objXmlMetavariant["logaug"].InnerText);
                    lblWIL.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["wilmin"].InnerText, objXmlMetavariant["wilmax"].InnerText, objXmlMetavariant["wilaug"].InnerText);
                    lblINI.Text = string.Format("{0}/{1} ({2})", objXmlMetavariant["inimin"].InnerText, objXmlMetavariant["inimax"].InnerText, objXmlMetavariant["iniaug"].InnerText);
                    // Determine if the Metavariant has any bonuses.
                    if (objXmlMetavariant.InnerXml.Contains("bonus"))
                        objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metavariant, cboMetavariant.SelectedValue.ToString(), objXmlMetavariant.SelectSingleNode("bonus"), false, 1, cboMetavariant.SelectedValue.ToString());

                    // Create the Qualities that come with the Metatype.
                    foreach (XmlNode objXmlQualityItem in objXmlMetavariant.SelectNodes("qualities/positive/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        Quality objQuality = new Quality(_objCharacter);
                        string strForceValue = "";
                        if (objXmlQualityItem.Attributes["select"] != null)
                            strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                        QualitySource objSource = new QualitySource();
                        objSource = QualitySource.Metatype;
                        if (objXmlQualityItem.Attributes["removable"] != null)
                            objSource = QualitySource.MetatypeRemovable;
                        objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                        objQuality.ContributeToLimit = false;
                        _objCharacter.Qualities.Add(objQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                    foreach (XmlNode objXmlQualityItem in objXmlMetavariant.SelectNodes("qualities/negative/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        Quality objQuality = new Quality(_objCharacter);
                        string strForceValue = "";
                        if (objXmlQualityItem.Attributes["select"] != null)
                            strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                        QualitySource objSource = new QualitySource();
                        objSource = QualitySource.Metatype;
                        if (objXmlQualityItem.Attributes["removable"] != null)
                            objSource = QualitySource.MetatypeRemovable;
                        objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                        objQuality.ContributeToLimit = false;
                        _objCharacter.Qualities.Add(objQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                }
                else
                {
                    lblMetavariantBP.Text = "0";
                    lblMetavariantQualities.Text = "None";
                }
                // Run through the character's Attributes one more time and make sure their value matches their minimum value.
                if (_strXmlFile == "metatypes.xml")
                {
                    _objCharacter.BOD.Value = _objCharacter.BOD.TotalMinimum;
                    _objCharacter.AGI.Value = _objCharacter.AGI.TotalMinimum;
                    _objCharacter.REA.Value = _objCharacter.REA.TotalMinimum;
                    _objCharacter.STR.Value = _objCharacter.STR.TotalMinimum;
                    _objCharacter.CHA.Value = _objCharacter.CHA.TotalMinimum;
                    _objCharacter.INT.Value = _objCharacter.INT.TotalMinimum;
                    _objCharacter.LOG.Value = _objCharacter.LOG.TotalMinimum;
                    _objCharacter.WIL.Value = _objCharacter.WIL.TotalMinimum;

                    _objCharacter.BOD.Base = _objCharacter.BOD.TotalMinimum;
                    _objCharacter.AGI.Base = _objCharacter.AGI.TotalMinimum;
                    _objCharacter.REA.Base = _objCharacter.REA.TotalMinimum;
                    _objCharacter.STR.Base = _objCharacter.STR.TotalMinimum;
                    _objCharacter.CHA.Base = _objCharacter.CHA.TotalMinimum;
                    _objCharacter.INT.Base = _objCharacter.INT.TotalMinimum;
                    _objCharacter.LOG.Base = _objCharacter.LOG.TotalMinimum;
                    _objCharacter.WIL.Base = _objCharacter.WIL.TotalMinimum;

                    _objCharacter.BOD.Karma = 0;
                    _objCharacter.AGI.Karma = 0;
                    _objCharacter.REA.Karma = 0;
                    _objCharacter.STR.Karma = 0;
                    _objCharacter.CHA.Karma = 0;
                    _objCharacter.INT.Karma = 0;
                    _objCharacter.LOG.Karma = 0;
                    _objCharacter.WIL.Karma = 0;
                    _objCharacter.EDG.Karma = 0;
                    _objCharacter.MAG.Karma = 0;
                    _objCharacter.RES.Karma = 0;
                }

                // Add any Critter Powers the Metatype/Critter should have.
                XmlNode objXmlCritter = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");

                objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
                foreach (XmlNode objXmlPower in objXmlCritter.SelectNodes("powers/power"))
                {
                    XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    CritterPower objPower = new CritterPower(_objCharacter);
                    string strForcedValue = "";
                    int intRating = 0;

                    if (objXmlPower.Attributes["rating"] != null)
                        intRating = Convert.ToInt32(objXmlPower.Attributes["rating"].InnerText);
                    if (objXmlPower.Attributes["select"] != null)
                        strForcedValue = objXmlPower.Attributes["select"].InnerText;

                    objPower.Create(objXmlCritterPower, _objCharacter, objNode, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                // Add any Critter Powers the Metavariant should have.
                if (cboMetavariant.SelectedValue.ToString() != "None")
                {
                    foreach (XmlNode objXmlPower in objXmlMetavariant.SelectNodes("powers/power"))
                    {
                        XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        CritterPower objPower = new CritterPower(_objCharacter);
                        string strForcedValue = "";
                        int intRating = 0;

                        if (objXmlPower.Attributes["rating"] != null)
                            intRating = Convert.ToInt32(objXmlPower.Attributes["rating"].InnerText);
                        if (objXmlPower.Attributes["select"] != null)
                            strForcedValue = objXmlPower.Attributes["select"].InnerText;

                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, intRating, strForcedValue);
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }
                }

                // If this is a Blood Spirit, add their free Critter Powers.
                if (chkBloodSpirit.Checked)
                {
                    XmlNode objXmlCritterPower;
                    TreeNode objNode;
                    CritterPower objPower;
                    bool blnAddPower = true;

                    // Energy Drain.
                    foreach (CritterPower objFindPower in _objCharacter.CritterPowers)
                    {
                        if (objFindPower.Name == "Energy Drain")
                        {
                            blnAddPower = false;
                            break;
                        }
                    }
                    if (blnAddPower)
                    {
                        objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Energy Drain\"]");
                        objNode = new TreeNode();
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, "");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }

                    // Fear.
                    blnAddPower = true;
                    foreach (CritterPower objFindPower in _objCharacter.CritterPowers)
                    {
                        if (objFindPower.Name == "Fear")
                        {
                            blnAddPower = false;
                            break;
                        }
                    }
                    if (blnAddPower)
                    {
                        objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Fear\"]");
                        objNode = new TreeNode();
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, "");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }

                    // Natural Weapon.
                    objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Natural Weapon\"]");
                    objNode = new TreeNode();
                    objPower = new CritterPower(_objCharacter);
                    objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, "DV " + intForce.ToString() + "P, AP 0");
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);

                    // Evanescence.
                    blnAddPower = true;
                    foreach (CritterPower objFindPower in _objCharacter.CritterPowers)
                    {
                        if (objFindPower.Name == "Evanescence")
                        {
                            blnAddPower = false;
                            break;
                        }
                    }
                    if (blnAddPower)
                    {
                        objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Evanescence\"]");
                        objNode = new TreeNode();
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, "");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }
                }

                // Remove the Critter's Materialization Power if they have it. Add the Possession or Inhabitation Power if the Possession-based Tradition checkbox is checked.
                if (chkPossessionBased.Checked)
                {
                    foreach (CritterPower objCritterPower in _objCharacter.CritterPowers)
                    {
                        if (objCritterPower.Name == "Materialization")
                        {
                            _objCharacter.CritterPowers.Remove(objCritterPower);
                            break;
                        }
                    }

                    // Add the selected Power.
                    XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + cboPossessionMethod.SelectedValue.ToString() + "\"]");
                    TreeNode objNode = new TreeNode();
                    CritterPower objPower = new CritterPower(_objCharacter);
                    objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, "");
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                // Set the Skill Ratings for the Critter.
                foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/skill"))
                {
                    if (objXmlSkill.InnerText.Contains("Exotic"))
                    {
                        Skill objExotic = new Skill(_objCharacter);
                        objExotic.ExoticSkill = true;
                        objExotic.Attribute = "AGI";
                        if (objXmlSkill.Attributes["spec"] != null)
                        {
                            SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerText);
                            objExotic.Specializations.Add(objSpec);
                        }
                        if (Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0)) > 6)
                            objExotic.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                        objExotic.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                        objExotic.Name = objXmlSkill.InnerText;
                        _objCharacter.Skills.Add(objExotic);
                    }
                    else
                    {
                        foreach (Skill objSkill in _objCharacter.Skills)
                        {
                            if (objSkill.Name == objXmlSkill.InnerText)
                            {
                                if (objXmlSkill.Attributes["spec"] != null)
                                {
                                    SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerText);
                                    objSkill.Specializations.Add(objSpec);
                                }
                                if (Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0)) > 6)
                                    objSkill.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                                objSkill.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                                break;
                            }
                        }
                    }
                }

                // Set the Skill Group Ratings for the Critter.
                foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/group"))
                {
                    foreach (SkillGroup objSkill in _objCharacter.SkillGroups)
                    {
                        if (objSkill.Name == objXmlSkill.InnerText)
                        {
                            objSkill.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                            objSkill.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                            break;
                        }
                    }
                }

                // Set the Knowledge Skill Ratings for the Critter.
                foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/knowledge"))
                {
                    Skill objKnowledge = new Skill(_objCharacter);
                    objKnowledge.Name = objXmlSkill.InnerText;
                    objKnowledge.KnowledgeSkill = true;
                    if (objXmlSkill.Attributes["spec"] != null)
                    {
                        SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerText);
                        objKnowledge.Specializations.Add(objSpec);
                    }
                    objKnowledge.SkillCategory = objXmlSkill.Attributes["category"].InnerText;
                    if (Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText) > 6)
                        objKnowledge.RatingMaximum = Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                    objKnowledge.Rating = Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                    _objCharacter.Skills.Add(objKnowledge);
                }

                // If this is a Critter with a Force (which dictates their Skill Rating/Maximum Skill Rating), set their Skill Rating Maximums.
                if (intForce > 0)
                {
                    int intMaxRating = intForce;
                    // Determine the highest Skill Rating the Critter has.
                    foreach (Skill objSkill in _objCharacter.Skills)
                    {
                        if (objSkill.RatingMaximum > intMaxRating)
                            intMaxRating = objSkill.RatingMaximum;
                    }

                    // Now that we know the upper limit, set all of the Skill Rating Maximums to match.
                    foreach (Skill objSkill in _objCharacter.Skills)
                        objSkill.RatingMaximum = intMaxRating;
                    foreach (SkillGroup objGroup in _objCharacter.SkillGroups)
                        objGroup.RatingMaximum = intMaxRating;

                    // Set the MaxSkillRating for the character so it can be used later when they add new Knowledge Skills or Exotic Skills.
                    _objCharacter.MaxSkillRating = intMaxRating;
                }

                // Add any Complex Forms the Critter comes with (typically Sprites)
                XmlDocument objXmlProgramDocument = XmlManager.Instance.Load("complexforms.xml");
                foreach (XmlNode objXmlComplexForm in objXmlCritter.SelectNodes("complexforms/complexform"))
                {
                    string strForceValue = "";
                    if (objXmlComplexForm.Attributes["select"] != null)
                        strForceValue = objXmlComplexForm.Attributes["select"].InnerText;
                    XmlNode objXmlProgram = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlComplexForm.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    ComplexForm objProgram = new ComplexForm(_objCharacter);
                    objProgram.Create(objXmlProgram, _objCharacter, objNode, strForceValue);
                    _objCharacter.ComplexForms.Add(objProgram);
                }

                // Add any Gear the Critter comes with (typically Programs for A.I.s)
                XmlDocument objXmlGearDocument = XmlManager.Instance.Load("gear.xml");
                foreach (XmlNode objXmlGear in objXmlCritter.SelectNodes("gears/gear"))
                {
                    int intRating = 0;
                    if (objXmlGear.Attributes["rating"] != null)
                        intRating = Convert.ToInt32(ExpressionToString(objXmlGear.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                    string strForceValue = "";
                    if (objXmlGear.Attributes["select"] != null)
                        strForceValue = objXmlGear.Attributes["select"].InnerText;
                    XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    Gear objGear = new Gear(_objCharacter);
                    List<Weapon> lstWeapons = new List<Weapon>();
                    List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                    objGear.Create(objXmlGearItem, _objCharacter, objNode, intRating, lstWeapons, lstWeaponNodes, strForceValue);
                    objGear.Cost = "0";
                    objGear.Cost3 = "0";
                    objGear.Cost6 = "0";
                    objGear.Cost10 = "0";
                    _objCharacter.Gear.Add(objGear);
                }

                // If this is a Mutant Critter, count up the number of Skill points they start with.
                if (_objCharacter.MetatypeCategory == "Mutant Critters")
                {
                    foreach (Skill objSkill in _objCharacter.Skills)
                        _objCharacter.MutantCritterBaseSkills += objSkill.Rating;
                }

                // begin SumtoTen based character settings
                // Load the SumtoTen information.
                XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

                // Set the character SumtoTen selections
                _objCharacter.MetatypePriority = cboHeritage.Text.ToString();
                _objCharacter.AttributesPriority = cboAttributes.Text.ToString();
                _objCharacter.SpecialPriority = cboTalent.Text.ToString();
                _objCharacter.SkillsPriority = cboSkills.Text.ToString();
                _objCharacter.ResourcesPriority = cboResources.Text.ToString();

                // Set starting nuyen
                XmlNodeList objXmResourceList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Resources\" and gameplayoption = \"" + _objCharacter.GameplayOption + "\" and value = \"" + cboResources.SelectedValue + "\"]");
                if (objXmResourceList.Count > 0)
                {
                    _objCharacter.Nuyen = Convert.ToInt32(objXmResourceList[0]["resources"].InnerText.ToString());
                    _objCharacter.StartingNuyen = _objCharacter.Nuyen;
                }

                // Set starting positive qualities
                foreach (XmlNode objXmlQualityItem in objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[value = \"" + cboTalents.SelectedValue + "\"]/qualities/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<Weapon> objWeapons = new List<Weapon>();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = "";
                    if (objXmlQualityItem.Attributes["select"] != null)
                        strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                    QualitySource objSource = new QualitySource();
                    objSource = QualitySource.Metatype;
                    if (objXmlQualityItem.Attributes["removable"] != null)
                        objSource = QualitySource.MetatypeRemovable;
                    objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                    _objCharacter.Qualities.Add(objQuality);

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in objWeapons)
                        _objCharacter.Weapons.Add(objWeapon);
                }

                // Set starting magic
                XmlNodeList objXmlTalentList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[value = \"" + cboTalents.SelectedValue + "\"]");
                if (objXmlTalentList[0]["magic"] != null)
                {
                    _objCharacter.MAG.Value = Convert.ToInt32(objXmlTalentList[0]["magic"].InnerText);
                    _objCharacter.MAG.MetatypeMinimum = Convert.ToInt32(objXmlTalentList[0]["magic"].InnerText);
                    if (_objCharacter.MAG.Value > 0)
                        _objCharacter.MAGEnabled = true;
                    if (objXmlTalentList[0]["spells"] != null)
                    {
                        _objCharacter.SpellLimit = Convert.ToInt32(objXmlTalentList[0]["spells"].InnerText);
                    }
                    else
                    {
                        _objCharacter.SpellLimit = 0;
                    }
                }

                if (objXmlTalentList[0]["maxmagic"] != null)
                    _objCharacter.MAG.MetatypeMaximum = Convert.ToInt32(objXmlTalentList[0]["magic"].InnerText);

                // Set starting resonance
                objXmlTalentList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent[value = \"" + cboTalents.SelectedValue + "\"]");
                if (objXmlTalentList[0]["resonance"] != null)
                {
                    _objCharacter.RES.Value = Convert.ToInt32(objXmlTalentList[0]["resonance"].InnerText);
                    _objCharacter.RES.MetatypeMinimum = Convert.ToInt32(objXmlTalentList[0]["resonance"].InnerText);
                    _objCharacter.RESEnabled = true;
                    _objCharacter.CFPLimit = Convert.ToInt32(objXmlTalentList[0]["cfp"].InnerText);
                }

                if (objXmlTalentList[0]["maxresonance"] != null)
                    _objCharacter.RES.MetatypeMaximum = Convert.ToInt32(objXmlTalentList[0]["resonance"].InnerText);

                // Set starting talent tabs
                switch (cboTalents.SelectedValue.ToString())
                {
                    case "Magician":
                        _objCharacter.MagicianEnabled = true;
                        break;
                    case "Aspected Magician":
                        _objCharacter.MagicianEnabled = true;
                        break;
                    case "Adept":
                        _objCharacter.AdeptEnabled = true;
                        break;
                    case "Mystic Adept":
                        _objCharacter.MagicianEnabled = true;
                        _objCharacter.AdeptEnabled = true;
                        break;
                    case "Technomancer":
                        _objCharacter.TechnomancerEnabled = true;
                        break;
                    default:
                        break;
                }

                // Set Free Skills/Skill Groups
                int intFreeLevels = 0;
                bool blnGroup = (cboTalents.SelectedValue.ToString() == "Aspected Magician");
                if (cboTalent.SelectedValue.ToString() == "4")
                    intFreeLevels = 5;
                else if (cboTalent.SelectedValue.ToString() == "3")
                    intFreeLevels = 4;
                else if (cboTalent.SelectedValue.ToString() == "2")
                    intFreeLevels = 2;
                foreach (Skill objSkill in _objCharacter.Skills)
                {
                    if (cboSkill1.Visible && objSkill.Name == cboSkill1.Text && !blnGroup)
                    {
                        objSkill.FreeLevels = intFreeLevels;
                        if (objSkill.Rating < intFreeLevels)
                            objSkill.Rating = intFreeLevels;
                        _objCharacter.PriorityBonusSkill1 = cboSkill1.Text.ToString();
                    }
                    else if (cboSkill2.Visible && objSkill.Name == cboSkill2.Text && !blnGroup)
                    {
                        objSkill.FreeLevels = intFreeLevels;
                        if (objSkill.Rating < intFreeLevels)
                            objSkill.Rating = intFreeLevels;
                        _objCharacter.PriorityBonusSkill2 = cboSkill2.Text.ToString();
                    }
                    else
                    {
                        objSkill.FreeLevels = 0;
                        if (blnGroup)
                        {
                            // if this skill is a magical skill not belonging to the selected group, reduce the skill maximum to 0
                            if (objSkill.SkillGroup == "Conjuring" || objSkill.SkillGroup == "Enchanting" || objSkill.SkillGroup == "Sorcery")
                            {
                                if (objSkill.SkillGroup != cboSkill1.SelectedValue.ToString())
                                    objSkill.RatingMaximum = 0;
                                else
                                {
                                    if (_objCharacter.IgnoreRules)
                                        objSkill.RatingMaximum = 12;
                                    else
                                        objSkill.RatingMaximum = 6;
                                }
                                _objCharacter.PriorityBonusSkillGroup = cboSkill1.Text.ToString();
                            }
                        }
                    }
                }
                foreach (SkillGroup objSkillGroup in _objCharacter.SkillGroups)
                {
                    if (cboSkill1.Visible && objSkillGroup.Name == cboSkill1.Text && blnGroup)
                    {
                        objSkillGroup.FreeLevels = intFreeLevels;
                        if (objSkillGroup.Base < intFreeLevels)
                            objSkillGroup.Base = intFreeLevels;
                        _objCharacter.PriorityBonusSkillGroup = cboSkill1.Text.ToString();
                    }
                    else
                        objSkillGroup.FreeLevels = 0;

                    if (blnGroup)
                    {
                        // if this skill is a magical skill not belonging to the selected group, reduce the skill maximum to 0
                        if (objSkillGroup.Name == "Conjuring" || objSkillGroup.Name == "Enchanting" || objSkillGroup.Name == "Sorcery")
                        {
                            if (objSkillGroup.Name != cboSkill1.SelectedValue.ToString())
                                objSkillGroup.RatingMaximum = 0;
                            else
                            {
                                if (_objCharacter.IgnoreRules)
                                    objSkillGroup.RatingMaximum = 12;
                                else
                                    objSkillGroup.RatingMaximum = 6;
                            }
                        }
                    }
                }

                // Ignore Rules
                if (_objCharacter.IgnoreRules)
                {
                    foreach (Skill objSkill in _objCharacter.Skills)
                    {
                        objSkill.RatingMaximum = 99;
                    }
                    foreach (SkillGroup objSkillGroup in _objCharacter.SkillGroups)
                    {
                        objSkillGroup.RatingMaximum = 99;
                    }
                }

                // Set Special Attributes
                _objCharacter.Special = Convert.ToInt32(lblSpecial.Text);
                _objCharacter.TotalSpecial = Convert.ToInt32(lblSpecial.Text);

                // Set Attributes
                XmlNodeList objXmlSumtoTenList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Attributes\" and value = \"" + cboAttributes.SelectedValue + "\"]");
                if (objXmlSumtoTenList[0]["attributes"] != null)
                {
                    _objCharacter.Attributes = Convert.ToInt32(objXmlSumtoTenList[0]["attributes"].InnerText);
                    _objCharacter.TotalAttributes = _objCharacter.Attributes;
                }

                // Set Skills and Skill Groups
                objXmlSumtoTenList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Skills\" and value = \"" + cboSkills.SelectedValue + "\"]");
                if (objXmlSumtoTenList[0]["skills"] != null)
                {
                    _objCharacter.SkillPoints = Convert.ToInt32(objXmlSumtoTenList[0]["skills"].InnerText);
                    _objCharacter.SkillPointsMaximum = _objCharacter.SkillPoints;
                    _objCharacter.SkillGroupPoints = Convert.ToInt32(objXmlSumtoTenList[0]["skillgroups"].InnerText);
                    _objCharacter.SkillGroupPointsMaximum = _objCharacter.SkillGroupPoints;
                }

				// Load the SumtoTen information.
				XmlDocument objXmlDocumentGameplayOptions = XmlManager.Instance.Load("gameplayoptions.xml");
				XmlNode objXmlGameplayOption = objXmlDocumentGameplayOptions.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + _objCharacter.GameplayOption + "\"]");
				string strKarma = objXmlGameplayOption["karma"].InnerText;
                string strNuyen = objXmlGameplayOption["maxnuyen"].InnerText;
                string strContactMultiplier = objXmlGameplayOption["contactmultiplier"].InnerText;
                _objCharacter.MaxKarma = Convert.ToInt32(strKarma);
                _objCharacter.MaxNuyen = Convert.ToInt32(strNuyen);
                _objCharacter.ContactMultiplier = Convert.ToInt32(strContactMultiplier);

                // Set free contact points
                _objCharacter.ContactPoints = _objCharacter.CHA.Value * _objCharacter.ContactMultiplier;

                // Set starting Karma
                _objCharacter.BuildKarma = _objCharacter.MaxKarma;

                // Set starting movement rate
                _objCharacter.Movement = (_objCharacter.AGI.TotalValue * 2).ToString() + "/" + (_objCharacter.AGI.TotalValue * 4).ToString();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_Metatype_SelectMetatype"), LanguageManager.Instance.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        public string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            int intValue = 0;
            XmlDocument objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();
            XPathExpression xprAttribute = nav.Compile(strIn.Replace("/", " div ").Replace("F", intForce.ToString()).Replace("1D6", intForce.ToString()).Replace("2D6", intForce.ToString()));
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                intValue = Convert.ToInt32(nav.Evaluate(xprAttribute).ToString());
            }
            catch
            {
                intValue = 1;
            }
            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < 1)
                    intValue = 1;
            }
            else
            {
                if (intValue < 0)
                    intValue = 0;
            }
            return intValue.ToString();
        }

        void PopulateTalents()
        {
            // Load the SumtoTen information.
            XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

            List<ListItem> lstTalent = new List<ListItem>();

            // Populate the SumtoTen Category list.
            XmlNodeList objXmlSumtoTenTalentList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Talent\" and value = \"" + cboTalent.SelectedValue + "\"]/talents/talent");
            foreach (XmlNode objXmlSumtoTenTalent in objXmlSumtoTenTalentList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlSumtoTenTalent["value"].InnerText;
                objItem.Name = objXmlSumtoTenTalent["name"].InnerText;
                lstTalent.Add(objItem);
            }

            SortListItem objSort = new SortListItem();
            lstTalent.Sort(objSort.Compare);
            cboTalents.DataSource = null;
            cboTalents.ValueMember = "Value";
            cboTalents.DisplayMember = "Name";
            cboTalents.DataSource = lstTalent;
            cboTalents.SelectedIndex = -1;
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        void PopulateMetatypes()
        {
            XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);
            // Load the SumtoTen information.
            XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

            List<ListItem> lstMetatype = new List<ListItem>();

            XmlNodeList objXmlMetatypeList = objXmlDocument.SelectNodes("/chummer/metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + cboCategory.SelectedValue + "\"]");

            foreach (XmlNode objXmlMetatype in objXmlMetatypeList)
            {
                XmlNodeList objXmlMetatypeSumtoTenList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + objXmlMetatype["name"].InnerText.ToString() + "\"]");
                if (objXmlMetatypeSumtoTenList.Count > 0)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlMetatype["name"].InnerText;
                    if (objXmlMetatype["translate"] != null)
                        objItem.Name = objXmlMetatype["translate"].InnerText;
                    else
                        objItem.Name = objXmlMetatype["name"].InnerText;
                    lstMetatype.Add(objItem);
                }
            }
            SortListItem objSort = new SortListItem();
            lstMetatype.Sort(objSort.Compare);
            lstMetatypes.DataSource = null;
            lstMetatypes.ValueMember = "Value";
            lstMetatypes.DisplayMember = "Name";
            lstMetatypes.DataSource = lstMetatype;
            lstMetatypes.SelectedIndex = -1;

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
            XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);

            // Load the SumtoTen information.
            XmlDocument objXmlDocumentSumtoTen = XmlManager.Instance.Load(_strSumtoTenXmlFile);

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
                        XmlNodeList objXmlMetatypeList = objXmlDocumentSumtoTen.SelectNodes("/chummer/sum10priorities/SumtoTen[category = \"Heritage\" and value = \"" + cboHeritage.SelectedValue + "\"]/metatypes/metatype[name = \"" + objItem["name"].InnerText.ToString() + "\"]");
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
                    if (objXmlCategory.Attributes != null)
                    {
                        if (objXmlCategory.Attributes["translate"] != null)
                            objItem.Name = objXmlCategory.Attributes["translate"].InnerText;
                        else
                            objItem.Name = objXmlCategory.InnerText;
                    }
                    else
                        objItem.Name = objXmlCategory.InnerXml;
                    _lstCategory.Add(objItem);
                }
            }

            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            try
            {
                cboCategory.SelectedValue = "Metahuman";
            }
            catch
            {
                cboCategory.SelectedIndex = 0;
            }
            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;

            this.Height = cmdOK.Bottom + 40;
            lstMetatypes.Height = cmdOK.Bottom - lstMetatypes.Top;

            PopulateMetatypes();
        }

        private XmlNodeList GetMagicalSkillList()
        {
            XmlNodeList objXmlSkillList;
            XmlDocument objXmlSkillsDocument = XmlManager.Instance.Load("skills.xml");
            objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill[category = \"Magical Active\"]");
            return objXmlSkillList;
        }

        private XmlNodeList GetResonanceSkillList()
        {
            XmlNodeList objXmlSkillList;
            XmlDocument objXmlSkillsDocument = XmlManager.Instance.Load("skills.xml");
            objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill[category = \"Resonance Active\"]");
            return objXmlSkillList;
        }

        private XmlNodeList GetActiveSkillList()
        {
            XmlNodeList objXmlSkillList;
            XmlDocument objXmlSkillsDocument = XmlManager.Instance.Load("skills.xml");
            objXmlSkillList = objXmlSkillsDocument.SelectNodes("/chummer/skills/skill");
            return objXmlSkillList;
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion

    }
}
