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
 using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
 using Chummer.Backend.Equipment;
 using Chummer.Backend.Skills;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Text;

namespace Chummer
{
    public partial class frmKarmaMetatype : Form
    {
        private bool _blnLoading = true;

        private readonly Character _objCharacter;
        private string _strCurrentPossessionMethod;

        private readonly XPathNavigator _xmlBaseMetatypeDataNode;
        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XmlNode _xmlSkillsDocumentKnowledgeSkillsNode;
        private readonly XmlNode _xmlMetatypeDocumentMetatypesNode;
        private readonly XmlNode _xmlQualityDocumentQualitiesNode;
        private readonly XmlNode _xmlCritterPowerDocumentPowersNode;
        
        #region Form Events
        public frmKarmaMetatype(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            XmlDocument xmlMetatypeDoc = XmlManager.Load(strXmlFile);
            _xmlMetatypeDocumentMetatypesNode = xmlMetatypeDoc.SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = xmlMetatypeDoc.GetFastNavigator().SelectSingleNode("/chummer");
            _xmlSkillsDocumentKnowledgeSkillsNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/knowledgeskills");
            XmlDocument xmlQualityDoc = XmlManager.Load("qualities.xml");
            _xmlQualityDocumentQualitiesNode = xmlQualityDoc.SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = xmlQualityDoc.GetFastNavigator().SelectSingleNode("/chummer");
            _xmlCritterPowerDocumentPowersNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers");
        }
        
        private void frmMetatype_Load(object sender, EventArgs e)
        {
            // Populate the Metatype Category list.
            List<ListItem> lstCategories = new List<ListItem>();

            // Create a list of Categories.
            XPathNavigator xmlMetatypesNode = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes");
            if (xmlMetatypesNode != null)
            {
                HashSet<string> lstAlreadyProcessed = new HashSet<string>();
                foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.Select("categories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    if (!lstAlreadyProcessed.Contains(strInnerText))
                    {
                        lstAlreadyProcessed.Add(strInnerText);
                        if (null != xmlMetatypesNode.SelectSingleNode("metatype[category = \"" + strInnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")]"))
                        {
                            lstCategories.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
                        }
                    }
                }
            }

            lstCategories.Sort(CompareListItems.CompareNames);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstCategories;

            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            cboCategory.SelectedValue = _objCharacter.MetatypeCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();
            
            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            chkPossessionBased.SetToolTip(LanguageManager.GetString("Tip_Metatype_PossessionTradition", GlobalOptions.Language));
            chkBloodSpirit.SetToolTip(LanguageManager.GetString("Tip_Metatype_BloodSpirit", GlobalOptions.Language));

            List<ListItem> lstMethods = new List<ListItem>
            {
                new ListItem("Possession", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText ?? "Possession"),
                new ListItem("Inhabitation", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")?.InnerText ?? "Inhabitation")
            };
            
            lstMethods.Sort(CompareListItems.CompareNames);
            
            foreach (CritterPower objPower in _objCharacter.CritterPowers)
            {
                string strPowerName = objPower.Name;
                if (lstMethods.Any(x => x.Value.ToString() == strPowerName))
                {
                    _strCurrentPossessionMethod = strPowerName;
                    break;
                }
            }
            
            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.ValueMember = "Value";
            cboPossessionMethod.DisplayMember = "Name";
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.EndUpdate();
            
            PopulateMetatypes();
            PopulateMetavariants();
            RefreshSelectedMetavariant();

            _blnLoading = false;
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            PopulateMetavariants();
        }

        private void lstMetatypes_DoubleClick(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            RefreshSelectedMetavariant();
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
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private void MetatypeSelected()
        {
            Cursor = Cursors.WaitCursor;
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();
                string strSelectedMetavariant = cboMetavariant.SelectedValue.ToString();

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                    strSelectedMetavariant = "Human";

                XmlNode objXmlMetatype = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode("metatype[name = \"" + strSelectedMetatype + "\"]");
                if (objXmlMetatype == null)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectMetatype", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Cursor = Cursors.Default;
                    return;
                }
                XmlNode objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
                int intForce = 0;
                if (nudForce.Visible)
                    intForce = decimal.ToInt32(nudForce.Value);

                // Set Metatype information.
                int intMinModifier = 0;
                int intMaxModifier = 0;
                XmlNode charNode = strSelectedMetatypeCategory == "Shapeshifter" ? objXmlMetatype : objXmlMetavariant ?? objXmlMetatype;
                _objCharacter.AttributeSection.Create(charNode, intForce, intMinModifier, intMaxModifier);
                _objCharacter.Metatype = strSelectedMetatype;
                _objCharacter.MetatypeCategory = strSelectedMetatypeCategory;
                _objCharacter.MetatypeBP = Convert.ToInt32(lblKarma.Text);
                _objCharacter.Metavariant = strSelectedMetavariant == "None" ? string.Empty : strSelectedMetavariant;

                // We only reverted to the base metatype to get the attributes.
                if (strSelectedMetatypeCategory == "Shapeshifter")
                {
                    charNode = objXmlMetavariant ?? objXmlMetatype;
                }

                string strMovement = objXmlMetatype["movement"]?.InnerText;
                if (!string.IsNullOrEmpty(strMovement))
                    _objCharacter.Movement = strMovement;

                // Determine if the Metatype has any bonuses.
                XmlNode xmlBonusNode = charNode.SelectSingleNode("bonus");
                if (xmlBonusNode != null)
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, strSelectedMetatype, xmlBonusNode, false, 1, strSelectedMetatype);

                List<Weapon> lstWeapons = new List<Weapon>();
                // Create the Qualities that come with the Metatype.
                using (XmlNodeList xmlQualityList = charNode.SelectNodes("qualities/*/quality"))
                    if (xmlQualityList != null)
                        foreach (XmlNode objXmlQualityItem in xmlQualityList)
                        {
                            XmlNode objXmlQuality = _xmlQualityDocumentQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                            Quality objQuality = new Quality(_objCharacter);
                            string strForceValue = objXmlQualityItem.Attributes["select"]?.InnerText ?? string.Empty;
                            QualitySource objSource = objXmlQualityItem.Attributes["removable"]?.InnerText == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
                            objQuality.Create(objXmlQuality, objSource, lstWeapons, strForceValue);
                            objQuality.ContributeToLimit = false;
                            _objCharacter.Qualities.Add(objQuality);
                        }

                //Load any critter powers the character has.
                foreach (XmlNode objXmlPower in charNode.SelectNodes("powers/power"))
                {
                    XmlNode objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"" + objXmlPower.InnerText + "\"]");
                    CritterPower objPower = new CritterPower(_objCharacter);
                    string strForcedValue = objXmlPower.Attributes["select"]?.InnerText ?? string.Empty;
                    int intRating = Convert.ToInt32(objXmlPower.Attributes["rating"]?.InnerText);

                    objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                    ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                    ImprovementManager.Commit(_objCharacter);
                }

                //Load any natural weapons the character has.
                foreach (XmlNode objXmlNaturalWeapon in charNode.SelectNodes("nautralweapons/naturalweapon"))
                {
                    Weapon objWeapon = new Weapon(_objCharacter)
                    {
                        Name = objXmlNaturalWeapon["name"].InnerText,
                        Category = LanguageManager.GetString("Tab_Critter", GlobalOptions.Language),
                        WeaponType = "Melee",
                        Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"]?.InnerText),
                        Damage = objXmlNaturalWeapon["damage"].InnerText,
                        AP = objXmlNaturalWeapon["ap"]?.InnerText ?? "0",
                        Mode = "0",
                        RC = "0",
                        Concealability = 0,
                        Avail = "0",
                        Cost = "0",
                        UseSkill = objXmlNaturalWeapon["useskill"]?.InnerText,
                        Source = objXmlNaturalWeapon["source"].InnerText,
                        Page = objXmlNaturalWeapon["page"].InnerText
                    };

                    _objCharacter.Weapons.Add(objWeapon);
                }

                if (strSelectedMetatypeCategory == "Spirits")
                {
                    XmlNode xmlOptionalPowersNode = charNode["optionalpowers"];
                    if (xmlOptionalPowersNode != null)
                    {
                        //For every 3 full points of Force a spirit has, it may gain one Optional Power.
                        for (int i = intForce -3; i >= 0; i -= 3)
                        {
                            XmlDocument objDummyDocument = new XmlDocument();
                            XmlNode bonusNode = objDummyDocument.CreateNode(XmlNodeType.Element, "bonus", null);
                            objDummyDocument.AppendChild(bonusNode);
                            XmlNode powerNode = objDummyDocument.ImportNode(xmlOptionalPowersNode.CloneNode(true),true);
                            objDummyDocument.ImportNode(powerNode, true);
                            bonusNode.AppendChild(powerNode);
                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, strSelectedMetatype, bonusNode, false, 1, strSelectedMetatype);
                        }
                    }
                    //If this is a Blood Spirit, add their free Critter Powers.
                    if (chkBloodSpirit.Checked)
                    {
                        XmlNode objXmlCritterPower;
                        CritterPower objPower;

                        //Energy Drain.
                        if (_objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Energy Drain"))
                        {
                            objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Energy Drain\"]");
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                            ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                            ImprovementManager.Commit(_objCharacter);
                        }

                        // Fear.
                        if (_objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Fear"))
                        {
                            objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Fear\"]");
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                            ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                            ImprovementManager.Commit(_objCharacter);
                        }

                        // Natural Weapon.
                        objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Natural Weapon\"]");
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, 0, "DV " + intForce.ToString() + "P, AP 0");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                        ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                        ImprovementManager.Commit(_objCharacter);

                        // Evanescence.
                        if (_objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Evanescence"))
                        {
                            objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Evanescence\"]");
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                            ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                            ImprovementManager.Commit(_objCharacter);
                        }
                    }

                    HashSet<string> lstPossessionMethods = new HashSet<string>();
                    foreach (ListItem objPossessionMethodItem in cboPossessionMethod.Items)
                        lstPossessionMethods.Add(objPossessionMethodItem.Value.ToString());

                    // Remove the Critter's Materialization Power if they have it. Add the Possession or Inhabitation Power if the Possession-based Tradition checkbox is checked.
                    if (chkPossessionBased.Checked)
                    {
                        string strSelectedPossessionMethod = cboPossessionMethod.SelectedValue?.ToString();
                        CritterPower objMaterializationPower = _objCharacter.CritterPowers.FirstOrDefault(x => x.Name == "Materialization");
                        if (objMaterializationPower != null)
                            _objCharacter.CritterPowers.Remove(objMaterializationPower);

                        if (_objCharacter.CritterPowers.All(x => x.Name != strSelectedPossessionMethod))
                        {
                            // Add the selected Power.
                            XmlNode objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"" + strSelectedPossessionMethod + "\"]");
                            if (objXmlCritterPower != null)
                            {
                                CritterPower objPower = new CritterPower(_objCharacter);
                                objPower.Create(objXmlCritterPower, 0, string.Empty);
                                objPower.CountTowardsLimit = false;
                                _objCharacter.CritterPowers.Add(objPower);

                                ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                                ImprovementManager.Commit(_objCharacter);
                            }
                        }
                    }
                    else if (_objCharacter.CritterPowers.All(x => x.Name != "Materialization"))
                    {
                        // Add the Materialization Power.
                        XmlNode objXmlCritterPower = _xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Materialization\"]");
                        if (objXmlCritterPower != null)
                        {
                            CritterPower objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);

                            ImprovementManager.CreateImprovement(_objCharacter, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                            ImprovementManager.Commit(_objCharacter);
                        }
                    }
                }

                //Set the Active Skill Ratings for the Critter.
                foreach (XmlNode xmlSkill in charNode.SelectNodes("skills/skill"))
                {
                    string strRating = xmlSkill.Attributes?["rating"]?.InnerText;

                    if (!string.IsNullOrEmpty(strRating))
                    {
                        ImprovementManager.CreateImprovement(_objCharacter, xmlSkill.InnerText, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillLevel, string.Empty, strRating == "F" ? intForce : Convert.ToInt32(strRating));
                        ImprovementManager.Commit(_objCharacter);
                    }
                }
                //Set the Skill Group Ratings for the Critter.
                foreach (XmlNode xmlSkillGroup in charNode.SelectNodes("skills/group"))
                {
                    string strRating = xmlSkillGroup.Attributes?["rating"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        ImprovementManager.CreateImprovement(_objCharacter, xmlSkillGroup.InnerText, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillGroupLevel, string.Empty, strRating == "F" ? intForce : Convert.ToInt32(strRating));
                        ImprovementManager.Commit(_objCharacter);
                    }
                }

                //Set the Knowledge Skill Ratings for the Critter.
                foreach (XmlNode xmlSkill in charNode.SelectNodes("skills/knowledge"))
                {
                    string strRating = xmlSkill.Attributes?["rating"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        if (_objCharacter.SkillsSection.KnowledgeSkills.All(x => x.Name != xmlSkill.InnerText))
                        {
                            XmlNode objXmlSkillNode = _xmlSkillsDocumentKnowledgeSkillsNode.SelectSingleNode("skill[name = \"" + xmlSkill.InnerText + "\"]");
                            if (objXmlSkillNode != null)
                            {
                                KnowledgeSkill objSkill = Skill.FromData(objXmlSkillNode, _objCharacter) as KnowledgeSkill;
                                _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                            }
                            else
                            {
                                KnowledgeSkill objSkill = new KnowledgeSkill(_objCharacter, xmlSkill.InnerText, true)
                                {
                                    Type = xmlSkill.Attributes?["category"]?.InnerText
                                };
                                _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                            }
                        }
                        ImprovementManager.CreateImprovement(_objCharacter, xmlSkill.InnerText, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillLevel, string.Empty, strRating == "F" ? intForce : Convert.ToInt32(strRating));
                        ImprovementManager.Commit(_objCharacter);
                    }
                }

                // Add any Complex Forms the Critter comes with (typically Sprites)
                XmlDocument xmlComplexFormDocument = XmlManager.Load("complexforms.xml");
                foreach (XmlNode xmlComplexForm in charNode.SelectNodes("complexforms/complexform"))
                {
                    XmlNode xmlComplexFormData = xmlComplexFormDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + xmlComplexForm.InnerText + "\"]");
                    if (xmlComplexFormData == null)
                        continue;

                    // Check for SelectText.
                    string strExtra = xmlComplexForm.Attributes?["select"]?.InnerText ?? string.Empty;
                    XmlNode xmlSelectText = xmlComplexFormData.SelectSingleNode("bonus/selecttext");
                    if (xmlSelectText != null && !string.IsNullOrWhiteSpace(strExtra))
                    {
                        frmSelectText frmPickText = new frmSelectText
                        {
                            Description = string.Format(LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language), xmlComplexFormData["translate"]?.InnerText ?? xmlComplexFormData["name"].InnerText)
                        };
                        frmPickText.ShowDialog();
                        // Make sure the dialogue window was not canceled.
                        if (frmPickText.DialogResult == DialogResult.Cancel)
                            continue;
                        strExtra = frmPickText.SelectedValue;
                    }

                    ComplexForm objComplexform = new ComplexForm(_objCharacter);
                    objComplexform.Create(xmlComplexFormData, strExtra);
                    if (objComplexform.InternalId.IsEmptyGuid())
                        continue;
                    objComplexform.Grade = -1;

                    _objCharacter.ComplexForms.Add(objComplexform);

                    ImprovementManager.CreateImprovement(_objCharacter, objComplexform.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.ComplexForm, string.Empty);
                    ImprovementManager.Commit(_objCharacter);
                }

                // Add any Advanced Programs the Critter comes with (typically A.I.s)
                XmlDocument xmlAIProgramDocument = XmlManager.Load("programs.xml");
                foreach (XmlNode xmlAIProgram in charNode.SelectNodes("programs/program"))
                {
                    XmlNode xmlAIProgramData = xmlAIProgramDocument.SelectSingleNode("/chummer/programs/program[name = \"" + xmlAIProgram.InnerText + "\"]");
                    if (xmlAIProgramData == null)
                        continue;

                    // Check for SelectText.
                    string strExtra = xmlAIProgram.Attributes?["select"]?.InnerText ?? string.Empty;
                    XmlNode xmlSelectText = xmlAIProgramData.SelectSingleNode("bonus/selecttext");
                    if (xmlSelectText != null && !string.IsNullOrWhiteSpace(strExtra))
                    {
                        frmSelectText frmPickText = new frmSelectText
                        {
                            Description = string.Format(LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language), xmlAIProgramData["translate"]?.InnerText ?? xmlAIProgramData["name"].InnerText)
                        };
                        frmPickText.ShowDialog();
                        // Make sure the dialogue window was not canceled.
                        if (frmPickText.DialogResult == DialogResult.Cancel)
                            continue;
                        strExtra = frmPickText.SelectedValue;
                    }

                    AIProgram objAIProgram = new AIProgram(_objCharacter);
                    objAIProgram.Create(xmlAIProgram, strExtra, false);
                    if (objAIProgram.InternalId.IsEmptyGuid())
                        continue;

                    _objCharacter.AIPrograms.Add(objAIProgram);

                    ImprovementManager.CreateImprovement(_objCharacter, objAIProgram.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.AIProgram, string.Empty);
                    ImprovementManager.Commit(_objCharacter);
                }

                // Add any Gear the Critter comes with (typically Programs for A.I.s)
                XmlDocument xmlGearDocument = XmlManager.Load("gear.xml");
                foreach (XmlNode xmlGear in charNode.SelectNodes("gears/gear"))
                {
                    XmlNode xmlGearData = xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + xmlGear["name"].InnerText.CleanXPath() + " and category = " + xmlGear["category"].InnerText.CleanXPath() + "]");
                    if (xmlGearData == null)
                        continue;

                    int intRating = 1;
                    if (xmlGear["rating"] != null)
                        intRating = Convert.ToInt32(xmlGear["rating"].InnerText);
                    decimal decQty = 1.0m;
                    if (xmlGear["quantity"] != null)
                        decQty = Convert.ToDecimal(xmlGear["quantity"].InnerText, GlobalOptions.InvariantCultureInfo);
                    string strForceValue = xmlGear.Attributes?["select"]?.InnerText ?? string.Empty;

                    Gear objGear = new Gear(_objCharacter);
                    objGear.Create(xmlGearData, intRating, lstWeapons, strForceValue);

                    if (objGear.InternalId.IsEmptyGuid())
                        continue;

                    objGear.Quantity = decQty;

                    // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                    if (_objCharacter.ActiveCommlink == null && objGear.IsCommlink)
                    {
                        objGear.SetActiveCommlink(_objCharacter, true);
                    }

                    objGear.Cost = "0";
                    // Create any Weapons that came with this Gear.
                    foreach (Weapon objWeapon in lstWeapons)
                        _objCharacter.Weapons.Add(objWeapon);

                    objGear.ParentID = Guid.NewGuid().ToString();

                    _objCharacter.Gear.Add(objGear);

                    ImprovementManager.CreateImprovement(_objCharacter, objGear.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.Gear, string.Empty);
                    ImprovementManager.Commit(_objCharacter);
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

                /*
                int x;
                int.TryParse(lblBP.Text, out x);
                _objCharacter.BuildKarma = _objCharacter.BuildKarma - x;
                */

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectMetatype", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Cursor = Cursors.Default;
        }

        private void RefreshSelectedMetavariant()
        {
            XPathNavigator objXmlMetatype = null;
            XPathNavigator objXmlMetavariant = null;
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
                if (objXmlMetatype != null && !string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                {
                    objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
                }
            }

            if (objXmlMetavariant != null)
            {
                cmdOK.Enabled = true;
                if (objXmlMetatype.SelectSingleNode("forcecreature") == null)
                {
                    lblBOD.Text = $"{objXmlMetavariant.SelectSingleNode("bodmin")?.Value}/{objXmlMetavariant.SelectSingleNode("bodmax")?.Value} ({objXmlMetavariant.SelectSingleNode("bodaug")?.Value})";
                    lblAGI.Text = $"{objXmlMetavariant.SelectSingleNode("agimin")?.Value}/{objXmlMetavariant.SelectSingleNode("agimax")?.Value} ({objXmlMetavariant.SelectSingleNode("agiaug")?.Value})";
                    lblREA.Text = $"{objXmlMetavariant.SelectSingleNode("reamin")?.Value}/{objXmlMetavariant.SelectSingleNode("reamax")?.Value} ({objXmlMetavariant.SelectSingleNode("reaaug")?.Value})";
                    lblSTR.Text = $"{objXmlMetavariant.SelectSingleNode("strmin")?.Value}/{objXmlMetavariant.SelectSingleNode("strmax")?.Value} ({objXmlMetavariant.SelectSingleNode("straug")?.Value})";
                    lblCHA.Text = $"{objXmlMetavariant.SelectSingleNode("chamin")?.Value}/{objXmlMetavariant.SelectSingleNode("chamax")?.Value} ({objXmlMetavariant.SelectSingleNode("chaaug")?.Value})";
                    lblINT.Text = $"{objXmlMetavariant.SelectSingleNode("intmin")?.Value}/{objXmlMetavariant.SelectSingleNode("intmax")?.Value} ({objXmlMetavariant.SelectSingleNode("intaug")?.Value})";
                    lblLOG.Text = $"{objXmlMetavariant.SelectSingleNode("logmin")?.Value}/{objXmlMetavariant.SelectSingleNode("logmax")?.Value} ({objXmlMetavariant.SelectSingleNode("logaug")?.Value})";
                    lblWIL.Text = $"{objXmlMetavariant.SelectSingleNode("wilmin")?.Value}/{objXmlMetavariant.SelectSingleNode("wilmax")?.Value} ({objXmlMetavariant.SelectSingleNode("wilaug")?.Value})";
                    lblINI.Text = $"{objXmlMetavariant.SelectSingleNode("inimin")?.Value}/{objXmlMetavariant.SelectSingleNode("inimax")?.Value} ({objXmlMetavariant.SelectSingleNode("iniaug")?.Value})";
                }
                else
                {
                    lblBOD.Text = objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? "0";
                    lblAGI.Text = objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? objXmlMetatype.SelectSingleNode("agimin")?.Value ?? "0";
                    lblREA.Text = objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? objXmlMetatype.SelectSingleNode("reamin")?.Value ?? "0";
                    lblSTR.Text = objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? objXmlMetatype.SelectSingleNode("strmin")?.Value ?? "0";
                    lblCHA.Text = objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? objXmlMetatype.SelectSingleNode("chamin")?.Value ?? "0";
                    lblINT.Text = objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? objXmlMetatype.SelectSingleNode("intmin")?.Value ?? "0";
                    lblLOG.Text = objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? objXmlMetatype.SelectSingleNode("logmin")?.Value ?? "0";
                    lblWIL.Text = objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? "0";
                    lblINI.Text = objXmlMetavariant.SelectSingleNode("inimin")?.Value ?? objXmlMetatype.SelectSingleNode("inimin")?.Value ?? "0";
                }

                StringBuilder strbldQualities = new StringBuilder();
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetavariant.Select("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        strbldQualities.Append(_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = \"" + objXmlQuality.Value + "\"]/translate")?.Value ?? objXmlQuality.Value);

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(');
                            strbldQualities.Append(LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language));
                            strbldQualities.Append(')');
                        }
                    }
                    else
                    {
                        strbldQualities.Append(objXmlQuality.Value);
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(');
                            strbldQualities.Append(strSelect);
                            strbldQualities.Append(')');
                        }
                    }
                    strbldQualities.Append(Environment.NewLine);
                }

                lblQualities.Text = strbldQualities.Length == 0 ? LanguageManager.GetString("String_None", GlobalOptions.Language) : strbldQualities.ToString();

                lblKarma.Text = objXmlMetavariant.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
            }
            else if (objXmlMetatype != null)
            {
                cmdOK.Enabled = true;
                if (objXmlMetatype.SelectSingleNode("forcecreature") == null)
                {
                    lblBOD.Text = $"{objXmlMetatype.SelectSingleNode("bodmin")?.Value}/{objXmlMetatype.SelectSingleNode("bodmax")?.Value} ({objXmlMetatype.SelectSingleNode("bodaug")?.Value})";
                    lblAGI.Text = $"{objXmlMetatype.SelectSingleNode("agimin")?.Value}/{objXmlMetatype.SelectSingleNode("agimax")?.Value} ({objXmlMetatype.SelectSingleNode("agiaug")?.Value})";
                    lblREA.Text = $"{objXmlMetatype.SelectSingleNode("reamin")?.Value}/{objXmlMetatype.SelectSingleNode("reamax")?.Value} ({objXmlMetatype.SelectSingleNode("reaaug")?.Value})";
                    lblSTR.Text = $"{objXmlMetatype.SelectSingleNode("strmin")?.Value}/{objXmlMetatype.SelectSingleNode("strmax")?.Value} ({objXmlMetatype.SelectSingleNode("straug")?.Value})";
                    lblCHA.Text = $"{objXmlMetatype.SelectSingleNode("chamin")?.Value}/{objXmlMetatype.SelectSingleNode("chamax")?.Value} ({objXmlMetatype.SelectSingleNode("chaaug")?.Value})";
                    lblINT.Text = $"{objXmlMetatype.SelectSingleNode("intmin")?.Value}/{objXmlMetatype.SelectSingleNode("intmax")?.Value} ({objXmlMetatype.SelectSingleNode("intaug")?.Value})";
                    lblLOG.Text = $"{objXmlMetatype.SelectSingleNode("logmin")?.Value}/{objXmlMetatype.SelectSingleNode("logmax")?.Value} ({objXmlMetatype.SelectSingleNode("logaug")?.Value})";
                    lblWIL.Text = $"{objXmlMetatype.SelectSingleNode("wilmin")?.Value}/{objXmlMetatype.SelectSingleNode("wilmax")?.Value} ({objXmlMetatype.SelectSingleNode("wilaug")?.Value})";
                    lblINI.Text = $"{objXmlMetatype.SelectSingleNode("inimin")?.Value}/{objXmlMetatype.SelectSingleNode("inimax")?.Value} ({objXmlMetatype.SelectSingleNode("iniaug")?.Value})";
                }
                else
                {
                    lblBOD.Text = objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? "0";
                    lblAGI.Text = objXmlMetatype.SelectSingleNode("agimin")?.Value ?? "0";
                    lblREA.Text = objXmlMetatype.SelectSingleNode("reamin")?.Value ?? "0";
                    lblSTR.Text = objXmlMetatype.SelectSingleNode("strmin")?.Value ?? "0";
                    lblCHA.Text = objXmlMetatype.SelectSingleNode("chamin")?.Value ?? "0";
                    lblINT.Text = objXmlMetatype.SelectSingleNode("intmin")?.Value ?? "0";
                    lblLOG.Text = objXmlMetatype.SelectSingleNode("logmin")?.Value ?? "0";
                    lblWIL.Text = objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? "0";
                    lblINI.Text = objXmlMetatype.SelectSingleNode("inimin")?.Value ?? "0";
                }

                StringBuilder strbldQualities = new StringBuilder();
                // Build a list of the Metatype's Positive Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetatype.Select("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        strbldQualities.Append(_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = \"" + objXmlQuality.Value + "\"]/translate")?.Value ?? objXmlQuality.Value);

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(');
                            strbldQualities.Append(LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language));
                            strbldQualities.Append(')');
                        }
                    }
                    else
                    {
                        strbldQualities.Append(objXmlQuality.Value);
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(');
                            strbldQualities.Append(strSelect);
                            strbldQualities.Append(')');
                        }
                    }
                    strbldQualities.Append(Environment.NewLine);
                }

                lblQualities.Text = strbldQualities.Length == 0 ? LanguageManager.GetString("String_None", GlobalOptions.Language) : strbldQualities.ToString();

                lblKarma.Text = objXmlMetatype.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
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

                lblQualities.Text = string.Empty;

                lblKarma.Text = string.Empty;

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
            lblQualitiesLabel.Visible = !string.IsNullOrEmpty(lblQualities.Text);
            lblKarma.Visible = !string.IsNullOrEmpty(lblKarma.Text);
        }

        private void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            XPathNavigator objXmlMetatype = null;
            if (!string.IsNullOrEmpty(strSelectedMetatype))
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
            // Don't attempt to do anything if nothing is selected.
            if (objXmlMetatype != null)
            {
                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                // Retrieve the list of Metavariants for the selected Metatype.
                foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]"))
                {
                    string strName = objXmlMetavariant.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstMetavariants.Add(new ListItem(strName, objXmlMetavariant.SelectSingleNode("translate")?.Value ?? strName));
                }

                bool blnOldLoading = _blnLoading;
                string strOldSelectedValue = cboMetavariant.SelectedValue?.ToString() ?? _objCharacter?.Metavariant;
                _blnLoading = true;
                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = lstMetavariants.Count > 1;
                _blnLoading = blnOldLoading;
                if (!string.IsNullOrEmpty(strOldSelectedValue))
                {
                    if (cboMetavariant.SelectedValue?.ToString() == strOldSelectedValue)
                        cboMetavariant_SelectedIndexChanged(null, null);
                    else
                        cboMetavariant.SelectedValue = strOldSelectedValue;
                }
                if (cboMetavariant.SelectedIndex == -1 && lstMetavariants.Count > 0)
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
                            lblForceLabel.Text = strEssMax.Substring(intPos, 3);
                            nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1)) * 6;
                        }
                        else
                        {
                            lblForceLabel.Text = "1D6";
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
                                                                       objXmlMetatype.SelectSingleNode("magmin")?.Value.Contains('F') != true ? "String_Level" : "String_Force", GlobalOptions.Language);
                        nudForce.Maximum = 100;
                    }
                }
                else
                {
                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
                lblMetavariantLabel.Visible = true;
                cboMetavariant.Visible = true;
            }
            else
            {
                lblMetavariantLabel.Visible = false;
                cboMetavariant.Visible = false;
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = false;
                _blnLoading = blnOldLoading;
                cboMetavariant.SelectedIndex = 0;
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
                List<ListItem> lstMetatypeItems = new List<ListItem>();

                foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select("metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + strSelectedCategory + "\"]"))
                {
                    string strName = objXmlMetatype.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstMetatypeItems.Add(new ListItem(strName, objXmlMetatype.SelectSingleNode("translate")?.Value ?? strName));
                }

                lstMetatypeItems.Sort(CompareListItems.CompareNames);

                bool blnOldLoading = _blnLoading;
                string strOldSelected = lstMetatypes.SelectedValue?.ToString() ?? _objCharacter?.Metatype;
                _blnLoading = true;
                lstMetatypes.BeginUpdate();
                lstMetatypes.ValueMember = "Value";
                lstMetatypes.DisplayMember = "Name";
                lstMetatypes.DataSource = lstMetatypeItems;
                _blnLoading = blnOldLoading;
                // Attempt to select the default Human item. If it could not be found, select the first item in the list instead.
                if (!string.IsNullOrEmpty(strOldSelected))
                {
                    if (lstMetatypes.SelectedValue?.ToString() == strOldSelected)
                        lstMetatypes_SelectedIndexChanged(this, EventArgs.Empty);
                    else
                        lstMetatypes.SelectedValue = strOldSelected;
                }
                if (lstMetatypes.SelectedIndex == -1 && lstMetatypeItems.Count > 0)
                    lstMetatypes.SelectedIndex = 0;

                lstMetatypes.EndUpdate();

                if (strSelectedCategory.EndsWith("Spirits"))
                {
                    if (!chkPossessionBased.Visible && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                    {
                        chkPossessionBased.Checked = true;
                        cboPossessionMethod.SelectedValue = _strCurrentPossessionMethod;
                    }
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
            else
            {
                lstMetatypes.BeginUpdate();
                lstMetatypes.DataSource = null;
                lstMetatypes.EndUpdate();

                chkBloodSpirit.Checked = false;
                chkBloodSpirit.Visible = false;
                chkPossessionBased.Visible = false;
                chkPossessionBased.Checked = false;
                cboPossessionMethod.Visible = false;
            }
        }
        #endregion
    }
}
