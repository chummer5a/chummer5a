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
using System.IO;
 using System.Linq;
 using System.Text;
 using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Skills;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class frmCreateImprovement : Form
    {
        private readonly Character _objCharacter;
        private readonly XmlDocument _objDocument;
        private string _strSelect = string.Empty;
        private readonly string _strCustomGroup;
        private Improvement _objEditImprovement;

        #region Control Events
        public frmCreateImprovement(Character objCharacter, string strCustomGroup = "")
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _strCustomGroup = strCustomGroup;
            _objDocument = XmlManager.Load("improvements.xml");
        }

        private void frmCreateImprovement_Load(object sender, EventArgs e)
        {
            List<ListItem> lstTypes = new List<ListItem>();

            // Populate the Improvement Type list.
            XmlNodeList objXmlImprovementList = _objDocument.SelectNodes("/chummer/improvements/improvement");
            if (objXmlImprovementList != null)
                lstTypes.AddRange(from XmlNode objXmlImprovement in objXmlImprovementList
                    select new ListItem(objXmlImprovement["id"]?.InnerText, Name = objXmlImprovement["translate"]?.InnerText ?? objXmlImprovement["name"]?.InnerText));

            lstTypes.Sort(CompareListItems.CompareNames);
            cboImprovemetType.BeginUpdate();
            cboImprovemetType.ValueMember = "Value";
            cboImprovemetType.DisplayMember = "Name";
            cboImprovemetType.DataSource = lstTypes;

            // Load the information from the passed Improvement if one has been given.
            if (_objEditImprovement != null)
            {
                cboImprovemetType.SelectedValue = _objEditImprovement.CustomId;
                txtName.Text = _objEditImprovement.CustomName;
                if (nudMax.Visible)
                    nudMax.Value = _objEditImprovement.Maximum;
                if (nudMin.Visible)
                    nudMin.Value = _objEditImprovement.Minimum;
                if (nudVal.Visible)
                {
                    // specificattribute stores the Value in Augmented instead.
                    nudVal.Value = _objEditImprovement.CustomId == "specificattribute" ? _objEditImprovement.Augmented : _objEditImprovement.Value;
                }
                chkApplyToRating.Checked = chkApplyToRating.Visible && _objEditImprovement.AddToRating;
                if (txtTranslateSelection.Visible)
                {
                    txtSelect.Text = _objEditImprovement.ImprovedName;
                    // get the selection type of improvement and generate translation
                    XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = \"" + cboImprovemetType.SelectedValue + "\"]/fields/field");
                    txtTranslateSelection.Text = TranslateField(objFetchNode.InnerText, _objEditImprovement.ImprovedName);
                }
            }
            cboImprovemetType.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cboImprovemetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = \"" + cboImprovemetType.SelectedValue + "\"]");

            lblVal.Visible = false;
            lblMin.Visible = false;
            lblMax.Visible = false;
            lblAug.Visible = false;
            nudVal.Visible = false;
            nudMin.Visible = false;
            nudMax.Visible = false;
            nudAug.Visible = false;
            chkApplyToRating.Visible = false;
            chkApplyToRating.Checked = false;
            chkFree.Visible = false;
            chkFree.Checked = false;

            lblSelect.Visible = false;
            txtSelect.Visible = false;
            txtSelect.Text = string.Empty;
            txtTranslateSelection.Text = string.Empty;
            txtTranslateSelection.Visible = false;
            cmdChangeSelection.Visible = false;
            _strSelect = string.Empty;

            if (objFetchNode == null) return;
            XmlNodeList xmlNodeList = objFetchNode.SelectNodes("fields/field");
            if (xmlNodeList != null)
                foreach (XmlNode objNode in xmlNodeList)
                {
                    switch (objNode.InnerText)
                    {
                        case "val":
                            lblVal.Visible = true;
                            nudVal.Visible = true;
                            break;
                        case "min":
                            lblMin.Visible = true;
                            nudMin.Visible = true;
                            break;
                        case "max":
                            lblMax.Visible = true;
                            nudMax.Visible = true;
                            break;
                        case "aug":
                            lblAug.Visible = true;
                            nudAug.Visible = true;
                            break;
                        case "applytorating":
                            chkApplyToRating.Visible = true;
                            break;
                        case "free":
                            chkFree.Visible = true;
                            break;
                        default:
                            if (objNode.InnerText.StartsWith("Select"))
                            {
                                lblSelect.Visible = true;
                                txtTranslateSelection.Visible = true;
                                cmdChangeSelection.Visible = true;
                                _strSelect = objNode.InnerText;
                            }
                            break;
                    }
                }

            // Display the help information.
            lblHelp.Text = objFetchNode["altpage"]?.InnerText ?? objFetchNode["page"]?.InnerText;
        }

        private void cmdChangeSelection_Click(object sender, EventArgs e)
        {
            switch (_strSelect)
            {
                case "SelectActionDicePool":
                    List<ListItem> lstActions = new List<ListItem>();
                    using (XmlNodeList xmlActionList = XmlManager.Load("actions.xml").SelectNodes("/chummer/actions/action"))
                        if (xmlActionList != null)
                            foreach (XmlNode xmlAction in xmlActionList)
                            {
                                lstActions.Add(new ListItem(xmlAction["name"].InnerText, xmlAction["translate"]?.InnerText ?? xmlAction["name"]?.InnerText));
                            }

                    frmSelectItem select = new frmSelectItem
                    {
                        Description = LanguageManager.GetString("Title_SelectAction", GlobalOptions.Language),
                        DropdownItems = lstActions
                    };
                    select.ShowDialog(this);

                    if (select.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = select.SelectedName;
                        txtTranslateSelection.Text = TranslateField(_strSelect, select.SelectedName);
                    }
                    break;
                case "SelectAttribute":
                    {
                        List<string> lstAbbrevs = new List<string>(Backend.Attributes.AttributeSection.AttributeStrings);

                        lstAbbrevs.Remove("ESS");
                        if (!_objCharacter.MAGEnabled)
                        {
                            lstAbbrevs.Remove("MAG");
                            lstAbbrevs.Remove("MAGAdept");
                        }
                        else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                            lstAbbrevs.Remove("MAGAdept");

                        if (!_objCharacter.RESEnabled)
                            lstAbbrevs.Remove("RES");
                        if (!_objCharacter.DEPEnabled)
                            lstAbbrevs.Remove("DEP");
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = LanguageManager.GetString("Title_SelectAttribute", GlobalOptions.Language)
                        };
                        frmPickAttribute.ShowDialog(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                        }
                    }
                    break;
                case "SelectMentalAttribute":
                    {
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute(Backend.Attributes.AttributeSection.MentalAttributes.ToArray())
                        {
                            Description = LanguageManager.GetString("Title_SelectAttribute", GlobalOptions.Language)
                        };

                        frmPickAttribute.ShowDialog(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                        }
                    }
                    break;
                case "SelectPhysicalAttribute":
                    {
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute(Backend.Attributes.AttributeSection.PhysicalAttributes.ToArray())
                        {
                            Description = LanguageManager.GetString("Title_SelectAttribute", GlobalOptions.Language)
                        };

                        frmPickAttribute.ShowDialog(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                        }
                    }
                    break;
                case "SelectSpecialAttribute":
                    {
                        List<string> lstAbbrevs = new List<string>(Backend.Attributes.AttributeSection.AttributeStrings);
                        lstAbbrevs.RemoveAll(x => Backend.Attributes.AttributeSection.PhysicalAttributes.Contains(x) || Backend.Attributes.AttributeSection.MentalAttributes.Contains(x));
                        lstAbbrevs.Remove("ESS");
                        /*
                        if (!_objCharacter.MAGEnabled)
                        {
                            lstAbbrevs.Remove("MAG");
                            lstAbbrevs.Remove("MAGAdept");
                        }
                        else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                            lstAbbrevs.Remove("MAGAdept");

                        if (!_objCharacter.RESEnabled)
                            lstAbbrevs.Remove("RES");
                        if (!_objCharacter.DEPEnabled)
                            lstAbbrevs.Remove("DEP");
                            */
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = LanguageManager.GetString("Title_SelectAttribute", GlobalOptions.Language)
                        };

                        frmPickAttribute.ShowDialog(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                        }

                    }
                    break;
                case "SelectSkill":
                    {
                        frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter)
                        {
                            Description = LanguageManager.GetString("Title_SelectSkill", GlobalOptions.Language)
                        };
                        frmPickSkill.ShowDialog(this);

                        if (frmPickSkill.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkill.SelectedSkill;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkill.SelectedSkill);
                        }  
                    }
                    break;
                case "SelectKnowSkill":
                    {
                        List<ListItem> lstDropdownItems = new List<ListItem>();
                        HashSet<string> setProcessedSkillNames = new HashSet<string>();
                        foreach (KnowledgeSkill objKnowledgeSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            lstDropdownItems.Add(new ListItem(objKnowledgeSkill.Name, objKnowledgeSkill.DisplayNameMethod(GlobalOptions.Language)));
                            setProcessedSkillNames.Add(objKnowledgeSkill.Name);
                        }
                        StringBuilder objFilter = new StringBuilder();
                        if (setProcessedSkillNames.Count > 0)
                        {
                            objFilter.Append("not(");
                            foreach (string strName in setProcessedSkillNames)
                            {
                                objFilter.Append("name = \"" + strName + "\" or ");
                            }

                            objFilter.Length -= 4;
                            objFilter.Append(')');
                        }

                        string strFilter = objFilter.Length > 0 ? '[' + objFilter.ToString() + ']' : string.Empty;
                        using (XmlNodeList xmlSkillList = XmlManager.Load("skills.xml", GlobalOptions.Language).SelectNodes("/chummer/knowledgeskills/skill" + strFilter))
                        {
                            if (xmlSkillList?.Count > 0)
                            {
                                foreach (XmlNode xmlSkill in xmlSkillList)
                                {
                                    string strName = xmlSkill["name"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strName))
                                        lstDropdownItems.Add(new ListItem(strName, xmlSkill["translate"]?.InnerText ?? strName));
                                }
                            }
                        }

                        lstDropdownItems.Sort(CompareListItems.CompareNames);

                        frmSelectItem frmPickSkill = new frmSelectItem
                        {
                            DropdownItems = lstDropdownItems,
                            Description = LanguageManager.GetString("Title_SelectSkill", GlobalOptions.Language)
                        };

                        frmPickSkill.ShowDialog(this);

                        if (frmPickSkill.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkill.SelectedItem;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkill.SelectedItem);
                        }
                    }
                    break;
                case "SelectSkillCategory":
                    frmSelectSkillCategory frmPickSkillCategory = new frmSelectSkillCategory
                    {
                        Description = LanguageManager.GetString("Title_SelectSkillCategory", GlobalOptions.Language)
                    };
                    frmPickSkillCategory.ShowDialog(this);

                    if (frmPickSkillCategory.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = frmPickSkillCategory.SelectedCategory;
                        txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkillCategory.SelectedCategory);
                    }
                       
                    break;
                case "SelectSkillGroup":
                    frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup
                    {
                        Description = LanguageManager.GetString("Title_SelectSkillGroup", GlobalOptions.Language)
                    };
                    frmPickSkillGroup.ShowDialog(this);

                    if (frmPickSkillGroup.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = frmPickSkillGroup.SelectedSkillGroup;
                        txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkillGroup.SelectedSkillGroup);
                    }
                        
                    break;
                case "SelectSpell":
                    List<ListItem> lstSpells = new List<ListItem>();
                    using (XmlNodeList xmlSpellList = XmlManager.Load("spells.xml").SelectNodes("/chummer/spells/spell"))
                        if (xmlSpellList != null)
                            foreach (XmlNode xmlSpell in xmlSpellList)
                            {
                                lstSpells.Add(new ListItem(xmlSpell["name"].InnerText, xmlSpell["translate"]?.InnerText ?? xmlSpell["name"]?.InnerText));
                            }

                    frmSelectItem selectSpell = new frmSelectItem
                    {
                        Description = LanguageManager.GetString("Title_SelectSpell", GlobalOptions.Language),
                        DropdownItems = lstSpells
                        
                    };
                    selectSpell.ShowDialog(this);

                    if (selectSpell.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = selectSpell.SelectedName;
                        txtTranslateSelection.Text = TranslateField(_strSelect, selectSpell.SelectedName);
                    }
                    break;
                case "SelectWeaponCategory":
                    frmSelectWeaponCategory frmPickWeaponCategory = new frmSelectWeaponCategory
                    {
                        Description = LanguageManager.GetString("Title_SelectWeaponCategory", GlobalOptions.Language)
                    };
                    frmPickWeaponCategory.ShowDialog(this);

                    if (frmPickWeaponCategory.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = frmPickWeaponCategory.SelectedCategory;
                        txtTranslateSelection.Text = TranslateField(_strSelect, frmPickWeaponCategory.SelectedCategory);
                    }
                    break;
                case "SelectSpellCategory":
                    frmSelectSpellCategory frmPickSpellCategory = new frmSelectSpellCategory
                    {
                        Description = LanguageManager.GetString("Title_SelectSpellCategory", GlobalOptions.Language)
                    };
                    frmPickSpellCategory.ShowDialog(this);

                    if (frmPickSpellCategory.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = frmPickSpellCategory.SelectedCategory;
                        txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSpellCategory.SelectedCategory);
                    }                     
                    break;
                case "SelectAdeptPower":
                    frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
                    frmPickPower.ShowDialog(this);

                    if (frmPickPower.DialogResult == DialogResult.OK)
                    {
                        txtSelect.Text = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[id = \"" + frmPickPower.SelectedPower + "\"]/name")?.InnerText;
                        txtTranslateSelection.Text = TranslateField(_strSelect, frmPickPower.SelectedPower);
                    }
                    break;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the values on the Form and create the required XML data.
        /// </summary>
        private void AcceptForm()
        {
            // Make sure a value has been selected if necessary.
            if (txtTranslateSelection.Visible && string.IsNullOrEmpty(txtSelect.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectItem", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectItem", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Make sure a value has been provided for the name.
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_ImprovementName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ImprovementName", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            MemoryStream objStream = new MemoryStream();
            XmlWriter objWriter = XmlWriter.Create(objStream);

            // Build the XML for the Improvement.
            XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = \"" + cboImprovemetType.SelectedValue + "\"]");
            string strInternal = objFetchNode?["internal"]?.InnerText;
            if (string.IsNullOrEmpty(strInternal))
                return;
            objWriter.WriteStartDocument();
            // <bonus>
            objWriter.WriteStartElement("bonus");
            // <whatever element>
            objWriter.WriteStartElement(strInternal);

            string strRating = string.Empty;
            if (chkApplyToRating.Checked)
                strRating = "<applytorating>True</applytorating>";

            // Retrieve the XML data from the document and replace the values as necessary.
            // ReSharper disable once PossibleNullReferenceException
            string strXml = objFetchNode["xml"].InnerText;
            strXml = strXml.Replace("{val}", nudVal.Value.ToString(GlobalOptions.InvariantCultureInfo));
            strXml = strXml.Replace("{min}", nudMin.Value.ToString(GlobalOptions.InvariantCultureInfo));
            strXml = strXml.Replace("{max}", nudMax.Value.ToString(GlobalOptions.InvariantCultureInfo));
            strXml = strXml.Replace("{aug}", nudAug.Value.ToString(GlobalOptions.InvariantCultureInfo));
            strXml = strXml.Replace("{free}", chkFree.Checked.ToString().ToLower());
            strXml = strXml.Replace("{select}", txtSelect.Text);
            strXml = strXml.Replace("{applytorating}", strRating);
            objWriter.WriteRaw(strXml);

            // Write the rest of the document.
            // </whatever element>
            objWriter.WriteEndElement();
            // </bonus>
            objWriter.WriteEndElement();
            objWriter.WriteEndDocument();
            objWriter.Flush();

            objStream.Position = 0;

            // Read it back in as an XmlDocument.
            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
            XmlDocument objBonusXml = new XmlDocument();
            strXml = objReader.ReadToEnd();
            objBonusXml.LoadXml(strXml);

            objWriter.Close();

            // Pluck out the bonus information.
            XmlNode objNode = objBonusXml.SelectSingleNode("/bonus");

            // Pass it to the Improvement Manager so that it can be added to the character.
            string strGuid = Guid.NewGuid().ToString("D");
            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Custom, strGuid, objNode, false, 1, txtName.Text);

            // If an Improvement was passed in, remove it from the character.
            string strNotes = string.Empty;
            int intOrder = 0;
            if (_objEditImprovement != null)
            {
                // Copy the notes over to the new item.
                strNotes = _objEditImprovement.Notes;
                intOrder = _objEditImprovement.SortOrder;
                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Custom, _objEditImprovement.SourceName);
            }

            // Find the newly-created Improvement and attach its custom name.
            Improvement objImprovement = _objCharacter.Improvements.FirstOrDefault(imp => imp.SourceName == strGuid);
            if (objImprovement != null)
            {
                objImprovement.CustomName = txtName.Text;
                objImprovement.CustomId = cboImprovemetType.SelectedValue.ToString();
                objImprovement.Custom = true;
                objImprovement.Notes = strNotes;
                objImprovement.SortOrder = intOrder;
                objImprovement.CustomGroup = _strCustomGroup;
                NewImprovement = objImprovement;
            }
            else {Utils.BreakIfDebug();}

            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Returns a current language translation given an improvement name.
        /// </summary>
        /// <param name="strImprovementType"> The selector for the target translation. Often just _strSelect. </param>
        /// <param name="strToTranslate"> The string which to translate. Usually name. Guid in the case of adept powers.</param>
        /// <returns></returns>
        private string TranslateField(string strImprovementType, string strToTranslate)
        {
            XmlNode objXmlNode;
            switch (strImprovementType)
            {
                case "SelectAttribute":
                case "SelectPhysicalAttribute":
                case "SelectMentalAttribute":
                case "SelectSpecialAttribute":
                    return strToTranslate == "MAGAdept"
                    ? LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language) + " (" + LanguageManager.GetString("String_DescAdept", GlobalOptions.Language) + ')'
                    : LanguageManager.GetString("String_Attribute" + strToTranslate + "Short", GlobalOptions.Language);

                case "SelectSkill":
                    objXmlNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/skills/skill[name = \"" + strToTranslate + "\"]");
                    return objXmlNode.SelectSingleNode("translate")?.InnerText ?? objXmlNode.SelectSingleNode("name").InnerText;

                case "SelectKnowSkill":
                    objXmlNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + strToTranslate + "\"]");
                    return objXmlNode.SelectSingleNode("translate")?.InnerText ?? objXmlNode.SelectSingleNode("name").InnerText;

                case "SelectSkillCategory":
                    objXmlNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/categories/category[. = \"" + strToTranslate + "\"]");
                    return objXmlNode.Attributes?["translate"]?.InnerText ?? objXmlNode.SelectSingleNode(".").InnerText;

                case "SelectSkillGroup":
                    objXmlNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/skillgroups/name[. = \"" + strToTranslate + "\"]");
                    return objXmlNode.Attributes?["translate"]?.InnerText ?? objXmlNode.SelectSingleNode(".").InnerText;

                case "SelectWeaponCategory":
                    objXmlNode = XmlManager.Load("weapons.xml").SelectSingleNode("/chummer/categories/category[. = \"" + strToTranslate + "\"]");
                    return objXmlNode.Attributes?["translate"]?.InnerText ?? objXmlNode.SelectSingleNode(".").InnerText;

                case "SelectSpellCategory":
                    objXmlNode = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/categories/category[. = \"" + strToTranslate + "\"]");
                    return objXmlNode.Attributes?["translate"]?.InnerText ?? objXmlNode.SelectSingleNode(".").InnerText;

                case "SelectAdeptPower":
                    objXmlNode = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[id = \"" + strToTranslate + "\" or name = \"" + strToTranslate + "\"]");
                    return objXmlNode.SelectSingleNode("translate")?.InnerText ?? objXmlNode.SelectSingleNode("name").InnerText;

                default:
                    return strToTranslate;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Improvement object to edit.
        /// </summary>
        public Improvement EditImprovementObject
        {
            set => _objEditImprovement = value;
        }

        public Improvement NewImprovement { get; set; }

        #endregion
    }
}
