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

        #region Control Events
        public frmCreateImprovement(Character objCharacter, string strCustomGroup = "")
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _strCustomGroup = strCustomGroup;
            _objDocument = objCharacter.LoadData("improvements.xml");
        }

        private void frmCreateImprovement_Load(object sender, EventArgs e)
        {
            List<ListItem> lstTypes = new List<ListItem>((int)Improvement.ImprovementType.NumImprovementTypes);

            // Populate the Improvement Type list.
            XmlNodeList objXmlImprovementList = _objDocument.SelectNodes("/chummer/improvements/improvement");
            if (objXmlImprovementList != null)
            {
                foreach (XmlNode objXmlImprovement in objXmlImprovementList)
                {
                    string strId = objXmlImprovement["id"]?.InnerText;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        lstTypes.Add(new ListItem(strId,
                            objXmlImprovement["translate"]?.InnerText
                            ?? objXmlImprovement["name"]?.InnerText
                            ?? LanguageManager.GetString("String_Unknown")));
                    }
                }
            }

            lstTypes.Sort(CompareListItems.CompareNames);
            cboImprovemetType.BeginUpdate();
            cboImprovemetType.ValueMember = nameof(ListItem.Value);
            cboImprovemetType.DisplayMember = nameof(ListItem.Name);
            cboImprovemetType.DataSource = lstTypes;

            // Load the information from the passed Improvement if one has been given.
            if (EditImprovementObject != null)
            {
                cboImprovemetType.SelectedValue = EditImprovementObject.CustomId;
                txtName.Text = EditImprovementObject.CustomName;
                if (nudMax.Visible)
                    nudMax.Value = EditImprovementObject.Maximum;
                if (nudMin.Visible)
                    nudMin.Value = EditImprovementObject.Minimum;
                if (nudVal.Visible)
                {
                    // specificattribute stores the Value in Augmented instead.
                    nudVal.Value = EditImprovementObject.CustomId == "specificattribute" ? EditImprovementObject.Augmented : EditImprovementObject.Value;
                }
                chkApplyToRating.Checked = chkApplyToRating.Visible && EditImprovementObject.AddToRating;
                if (txtTranslateSelection.Visible)
                {
                    txtSelect.Text = EditImprovementObject.ImprovedName;
                    // get the selection type of improvement and generate translation
                    XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = \"" + cboImprovemetType.SelectedValue + "\"]/fields/field");
                    txtTranslateSelection.Text = TranslateField(objFetchNode?.InnerText, EditImprovementObject.ImprovedName);
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
                            if (objNode.InnerText.StartsWith("Select", StringComparison.Ordinal))
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
            txtHelp.Text = objFetchNode["altpage"]?.InnerText ?? objFetchNode["page"]?.InnerText;
            chkIgnoreLimits.Visible = _strSelect == "SelectAdeptPower";
        }

        private void cmdChangeSelection_Click(object sender, EventArgs e)
        {
            switch (_strSelect)
            {
                case "SelectActionDicePool":
                    List<ListItem> lstActions;
                    using (XmlNodeList xmlActionList = _objCharacter.LoadData("actions.xml").SelectNodes("/chummer/actions/action"))
                    {
                        lstActions = new List<ListItem>(xmlActionList?.Count ?? 0);
                        if (xmlActionList?.Count > 0)
                        {
                            foreach (XmlNode xmlAction in xmlActionList)
                            {
                                string strName = xmlAction["name"]?.InnerText;
                                if (!string.IsNullOrEmpty(strName))
                                    lstActions.Add(new ListItem(strName, xmlAction["translate"]?.InnerText ?? strName));
                            }
                        }
                    }

                    using (frmSelectItem frmSelectAction = new frmSelectItem
                    {
                        Description = LanguageManager.GetString("Title_SelectAction")
                    })
                    {
                        frmSelectAction.SetDropdownItemsMode(lstActions);
                        frmSelectAction.ShowDialog(this);

                        if (frmSelectAction.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmSelectAction.SelectedName;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmSelectAction.SelectedName);
                        }
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
                        using (frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = LanguageManager.GetString("Title_SelectAttribute")
                        })
                        {
                            frmPickAttribute.ShowDialog(this);

                            if (frmPickAttribute.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickAttribute.SelectedAttribute;
                                txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                            }
                        }
                    }
                    break;
                case "SelectEcho":
                    using (frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(_objCharacter, frmSelectMetamagic.Mode.Echo))
                    {
                        frmPickMetamagic.ShowDialog(this);
                        if (frmPickMetamagic.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickMetamagic.SelectedMetamagic;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickMetamagic.SelectedMetamagic);
                        }
                    }
                    break;
                case "SelectMetamagic":
                    using (frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(_objCharacter, frmSelectMetamagic.Mode.Metamagic))
                    {
                        frmPickMetamagic.ShowDialog(this);
                        if (frmPickMetamagic.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickMetamagic.SelectedMetamagic;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickMetamagic.SelectedMetamagic);
                        }
                    }
                    break;
                case "SelectMentalAttribute":
                    using (frmSelectAttribute frmPickAttribute = new frmSelectAttribute(Backend.Attributes.AttributeSection.MentalAttributes.ToArray()))
                    {
                        frmPickAttribute.Description = LanguageManager.GetString("Title_SelectAttribute");
                        frmPickAttribute.ShowDialog(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                        }
                    }
                    break;
                case "SelectPhysicalAttribute":
                    using (frmSelectAttribute frmPickAttribute = new frmSelectAttribute(Backend.Attributes.AttributeSection.PhysicalAttributes.ToArray()))
                    {
                        frmPickAttribute.Description = LanguageManager.GetString("Title_SelectAttribute");
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
                        using (frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = LanguageManager.GetString("Title_SelectAttribute")
                        })
                        {
                            frmPickAttribute.ShowDialog(this);

                            if (frmPickAttribute.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickAttribute.SelectedAttribute;
                                txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                            }
                        }
                    }
                    break;
                case "SelectSkill":
                    using (frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter))
                    {
                        frmPickSkill.Description = LanguageManager.GetString("Title_SelectSkill");
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
                        List<ListItem> lstDropdownItems = new List<ListItem>(_objCharacter.SkillsSection.KnowledgeSkills.Count);
                        HashSet<string> setProcessedSkillNames = new HashSet<string>();
                        foreach (KnowledgeSkill objKnowledgeSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            lstDropdownItems.Add(new ListItem(objKnowledgeSkill.Name, objKnowledgeSkill.CurrentDisplayName));
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
                        using (XmlNodeList xmlSkillList = _objCharacter.LoadData("skills.xml").SelectNodes("/chummer/knowledgeskills/skill" + strFilter))
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

                        using (frmSelectItem frmPickSkill = new frmSelectItem
                        {
                            Description = LanguageManager.GetString("Title_SelectSkill")
                        })
                        {
                            frmPickSkill.SetDropdownItemsMode(lstDropdownItems);
                            frmPickSkill.ShowDialog(this);

                            if (frmPickSkill.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickSkill.SelectedItem;
                                txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkill.SelectedItem);
                            }
                        }
                    }
                    break;
                case "SelectSkillCategory":
                    using (frmSelectSkillCategory frmPickSkillCategory = new frmSelectSkillCategory(_objCharacter))
                    {
                        frmPickSkillCategory.Description = LanguageManager.GetString("Title_SelectSkillCategory");
                        frmPickSkillCategory.ShowDialog(this);

                        if (frmPickSkillCategory.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkillCategory.SelectedCategory;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkillCategory.SelectedCategory);
                        }
                    }
                    break;
                case "SelectSkillGroup":
                    using (frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup(_objCharacter))
                    {
                        frmPickSkillGroup.Description = LanguageManager.GetString("Title_SelectSkillGroup");
                        frmPickSkillGroup.ShowDialog(this);

                        if (frmPickSkillGroup.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkillGroup.SelectedSkillGroup;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkillGroup.SelectedSkillGroup);
                        }
                    }
                    break;
                case "SelectSpell":
                    List<ListItem> lstSpells;
                    using (XmlNodeList xmlSpellList = _objCharacter.LoadData("spells.xml").SelectNodes("/chummer/spells/spell"))
                    {
                        lstSpells = new List<ListItem>(xmlSpellList?.Count ?? 0);
                        if (xmlSpellList?.Count > 0)
                        {
                            foreach (XmlNode xmlSpell in xmlSpellList)
                            {
                                string strName = xmlSpell["name"]?.InnerText;
                                if (!string.IsNullOrEmpty(strName))
                                    lstSpells.Add(new ListItem(strName, xmlSpell["translate"]?.InnerText ?? strName));
                            }
                        }
                    }

                    using (frmSelectItem selectSpell = new frmSelectItem
                    {
                        Description = LanguageManager.GetString("Title_SelectSpell")
                    })
                    {
                        selectSpell.SetDropdownItemsMode(lstSpells);
                        selectSpell.ShowDialog(this);

                        if (selectSpell.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = selectSpell.SelectedName;
                            txtTranslateSelection.Text = TranslateField(_strSelect, selectSpell.SelectedName);
                        }
                    }
                    break;
                case "SelectWeaponCategory":
                    using (frmSelectWeaponCategory frmPickWeaponCategory = new frmSelectWeaponCategory(_objCharacter))
                    {
                        frmPickWeaponCategory.Description = LanguageManager.GetString("Title_SelectWeaponCategory");
                        frmPickWeaponCategory.ShowDialog(this);

                        if (frmPickWeaponCategory.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickWeaponCategory.SelectedCategory;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickWeaponCategory.SelectedCategory);
                        }
                    }
                    break;
                case "SelectSpellCategory":
                    using (frmSelectSpellCategory frmPickSpellCategory = new frmSelectSpellCategory(_objCharacter))
                    {
                        frmPickSpellCategory.Description = LanguageManager.GetString("Title_SelectSpellCategory");
                        frmPickSpellCategory.ShowDialog(this);

                        if (frmPickSpellCategory.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSpellCategory.SelectedCategory;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSpellCategory.SelectedCategory);
                        }
                    }
                    break;
                case "SelectAdeptPower":
                    using (frmSelectPower frmPickPower = new frmSelectPower(_objCharacter))
                    {
                        frmPickPower.IgnoreLimits = chkIgnoreLimits.Checked;
                        frmPickPower.ShowDialog(this);

                        if (frmPickPower.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = _objCharacter.LoadData("powers.xml").SelectSingleNode("/chummer/powers/power[id = \"" + frmPickPower.SelectedPower + "\"]/name")?.InnerText;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickPower.SelectedPower);
                        }
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
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_SelectItem"), LanguageManager.GetString("MessageTitle_SelectItem"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Make sure a value has been provided for the name.
            if (string.IsNullOrEmpty(txtName.Text))
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_ImprovementName"), LanguageManager.GetString("MessageTitle_ImprovementName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            XmlDocument objBonusXml = new XmlDocument
            {
                XmlResolver = null
            };
            using (MemoryStream objStream = new MemoryStream())
            {
                // Here instead of later because objWriter.Close() needs Stream to not be disposed, but StreamReader.Close() will dispose the Stream.
                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                {
                    using (XmlWriter objWriter = XmlWriter.Create(objStream))
                    {
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
                        XmlAttributeCollection xmlAttributeCollection = objFetchNode["xml"]?.Attributes;
                        if (xmlAttributeCollection != null)
                        {
                            foreach (XmlAttribute xmlAttribute in xmlAttributeCollection)
                            {
                                objWriter.WriteAttributeString(xmlAttribute.LocalName, xmlAttribute.Value);
                            }
                        }
                        // ReSharper disable once PossibleNullReferenceException
                        string strXml = objFetchNode["xml"].InnerText
                            .Replace("{val}", nudVal.Value.ToString(GlobalOptions.InvariantCultureInfo))
                            .Replace("{min}", nudMin.Value.ToString(GlobalOptions.InvariantCultureInfo))
                            .Replace("{max}", nudMax.Value.ToString(GlobalOptions.InvariantCultureInfo))
                            .Replace("{aug}", nudAug.Value.ToString(GlobalOptions.InvariantCultureInfo))
                            .Replace("{free}", chkFree.Checked.ToString(GlobalOptions.InvariantCultureInfo).ToLowerInvariant())
                            .Replace("{select}", txtSelect.Text)
                            .Replace("{applytorating}", strRating);
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
                        using (XmlReader objXmlReader = XmlReader.Create(objReader, GlobalOptions.SafeXmlReaderSettings))
                            objBonusXml.Load(objXmlReader);
                    }
                }
            }

            // Pluck out the bonus information.
            XmlNode objNode = objBonusXml.SelectSingleNode("/bonus");

            // Pass it to the Improvement Manager so that it can be added to the character.
            string strGuid = Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo);
            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Custom, strGuid, objNode, 1, txtName.Text);

            // If an Improvement was passed in, remove it from the character.
            string strNotes = string.Empty;
            int intOrder = 0;
            if (EditImprovementObject != null)
            {
                // Copy the notes over to the new item.
                strNotes = EditImprovementObject.Notes;
                intOrder = EditImprovementObject.SortOrder;
                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Custom, EditImprovementObject.SourceName);
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
                    ? LanguageManager.GetString("String_AttributeMAGShort") + LanguageManager.GetString("String_Space") + '(' + LanguageManager.GetString("String_DescAdept") + ')'
                    : LanguageManager.GetString("String_Attribute" + strToTranslate + "Short");

                case "SelectSkill":
                    if (strToTranslate.Contains("Exotic Melee Weapon") ||
                        strToTranslate.Contains("Exotic Ranged Weapon") ||
                        strToTranslate.Contains("Pilot Exotic Vehicle"))
                    {
                        string[] astrToTranslateParts = strToTranslate.Split('(', StringSplitOptions.RemoveEmptyEntries);
                        astrToTranslateParts[0] = astrToTranslateParts[0].Trim();
                        astrToTranslateParts[1] = astrToTranslateParts[1].Substring(0, astrToTranslateParts[1].Length - 1);

                        objXmlNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/skills/skill[name = \"" + astrToTranslateParts[0] + "\"]");
                        string strFirstPartTranslated = objXmlNode?.SelectSingleNode("translate")?.InnerText ?? objXmlNode?.SelectSingleNode("name")?.InnerText ?? astrToTranslateParts[0];

                        return strFirstPartTranslated + LanguageManager.GetString("String_Space") + '(' + _objCharacter.TranslateExtra(astrToTranslateParts[1]) + ')';
                    }
                    else
                    {
                        objXmlNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/skills/skill[name = \"" + strToTranslate + "\"]");
                        return objXmlNode?.SelectSingleNode("translate")?.InnerText ?? objXmlNode?.SelectSingleNode("name")?.InnerText ?? strToTranslate;
                    }

                case "SelectKnowSkill":
                    objXmlNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + strToTranslate + "\"]");
                    return objXmlNode?.SelectSingleNode("translate")?.InnerText ?? objXmlNode?.SelectSingleNode("name")?.InnerText ?? strToTranslate;

                case "SelectSkillCategory":
                    objXmlNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/categories/category[. = \"" + strToTranslate + "\"]");
                    return objXmlNode?.Attributes?["translate"]?.InnerText ?? objXmlNode?.SelectSingleNode(".")?.InnerText ?? strToTranslate;

                case "SelectSkillGroup":
                    objXmlNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/skillgroups/name[. = \"" + strToTranslate + "\"]");
                    return objXmlNode?.Attributes?["translate"]?.InnerText ?? objXmlNode?.SelectSingleNode(".")?.InnerText ?? strToTranslate;

                case "SelectWeaponCategory":
                    objXmlNode = _objCharacter.LoadData("weapons.xml").SelectSingleNode("/chummer/categories/category[. = \"" + strToTranslate + "\"]");
                    return objXmlNode?.Attributes?["translate"]?.InnerText ?? objXmlNode?.SelectSingleNode(".")?.InnerText ?? strToTranslate;

                case "SelectSpellCategory":
                    objXmlNode = _objCharacter.LoadData("spells.xml").SelectSingleNode("/chummer/categories/category[. = \"" + strToTranslate + "\"]");
                    return objXmlNode?.Attributes?["translate"]?.InnerText ?? objXmlNode?.SelectSingleNode(".")?.InnerText ?? strToTranslate;

                case "SelectAdeptPower":
                    objXmlNode = _objCharacter.LoadData("powers.xml").SelectSingleNode("/chummer/powers/power[id = \"" + strToTranslate + "\" or name = \"" + strToTranslate + "\"]");
                    return objXmlNode?.SelectSingleNode("translate")?.InnerText ?? objXmlNode?.SelectSingleNode("name")?.InnerText ?? strToTranslate;

                default:
                    return strToTranslate;
            }
        }
        #endregion

        #region Properties

        public Improvement NewImprovement { get; private set; }

        /// <summary>
        /// Set Improvement object to edit.
        /// </summary>
        public Improvement EditImprovementObject { get; set; }

        #endregion
    }
}
