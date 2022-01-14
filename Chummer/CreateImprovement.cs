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
using System.Xml.XPath;
using Chummer.Backend.Skills;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class CreateImprovement : Form
    {
        private readonly Character _objCharacter;
        private readonly XmlDocument _objDocument;
        private string _strSelect = string.Empty;
        private readonly string _strCustomGroup;

        #region Control Events

        public CreateImprovement(Character objCharacter, string strCustomGroup = "")
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
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            {
                // Populate the Improvement Type list.
                XmlNodeList objXmlImprovementList = _objDocument.SelectNodes("/chummer/improvements/improvement");
                if (objXmlImprovementList?.Count > 0)
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
                cboImprovemetType.PopulateWithListItems(lstTypes);

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
                        nudVal.Value = EditImprovementObject.CustomId == "specificattribute"
                            ? EditImprovementObject.Augmented
                            : EditImprovementObject.Value;
                    }

                    chkApplyToRating.Checked = chkApplyToRating.Visible && EditImprovementObject.AddToRating;
                    if (txtTranslateSelection.Visible)
                    {
                        txtSelect.Text = EditImprovementObject.ImprovedName;
                        // get the selection type of improvement and generate translation
                        XmlNode objFetchNode = _objDocument.SelectSingleNode(
                            "/chummer/improvements/improvement[id = "
                            + cboImprovemetType.SelectedValue.ToString().CleanXPath() + "]/fields/field");
                        txtTranslateSelection.Text
                            = TranslateField(objFetchNode?.InnerText, EditImprovementObject.ImprovedName);
                    }
                }

                cboImprovemetType.EndUpdate();
            }
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
            XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = " + cboImprovemetType.SelectedValue.ToString().CleanXPath() + "]");

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
                            if (objNode.InnerText.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
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
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstActions))
                    {
                        foreach (XPathNavigator xmlAction in _objCharacter.LoadDataXPath("actions.xml")
                                                                          .SelectAndCacheExpression(
                                                                              "/chummer/actions/action"))
                        {
                            string strName = xmlAction.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstActions.Add(new ListItem(
                                                   strName,
                                                   xmlAction.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                   ?? strName));
                        }

                        using (frmSelectItem frmSelectAction = new frmSelectItem
                               {
                                   Description = LanguageManager.GetString("Title_SelectAction")
                               })
                        {
                            frmSelectAction.SetDropdownItemsMode(lstActions);
                            frmSelectAction.ShowDialogSafe(this);

                            if (frmSelectAction.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmSelectAction.SelectedName;
                                txtTranslateSelection.Text = TranslateField(_strSelect, frmSelectAction.SelectedName);
                            }
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
                        else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
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
                            frmPickAttribute.ShowDialogSafe(this);

                            if (frmPickAttribute.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickAttribute.SelectedAttribute;
                                txtTranslateSelection.Text = TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                            }
                        }
                    }
                    break;

                case "SelectEcho":
                    {
                        InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1, Technomancer = true };
                        using (frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(_objCharacter, objGrade))
                        {
                            frmPickMetamagic.ShowDialogSafe(this);
                            if (frmPickMetamagic.DialogResult == DialogResult.OK)
                            {
                                string strSelectedId = frmPickMetamagic.SelectedMetamagic;
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    string strEchoName = _objCharacter.LoadDataXPath("echoes.xml")
                                                                             .SelectSingleNode(
                                                                                 "/chummer/echoes/echo[id = " + strSelectedId.CleanXPath() + "]/name")?.Value;
                                    if (!string.IsNullOrEmpty(strEchoName))
                                    {
                                        txtSelect.Text = strEchoName;
                                        txtTranslateSelection.Text = TranslateField(_strSelect, strEchoName);
                                    }
                                    else
                                    {
                                        txtSelect.Text = string.Empty;
                                        txtTranslateSelection.Text = string.Empty;
                                    }
                                }
                                else
                                {
                                    txtSelect.Text = string.Empty;
                                    txtTranslateSelection.Text = string.Empty;
                                }
                            }
                        }
                    }
                    break;

                case "SelectMetamagic":
                    {
                        InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1 };
                        using (frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(_objCharacter, objGrade))
                        {
                            frmPickMetamagic.ShowDialogSafe(this);
                            if (frmPickMetamagic.DialogResult == DialogResult.OK)
                            {
                                string strSelectedId = frmPickMetamagic.SelectedMetamagic;
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    string strEchoName = _objCharacter.LoadDataXPath("metamagic.xml")
                                                                      .SelectSingleNode(
                                                                          "/chummer/metamagics/metamagic[id = " + strSelectedId.CleanXPath() + "]/name")?.Value;
                                    if (!string.IsNullOrEmpty(strEchoName))
                                    {
                                        txtSelect.Text = strEchoName;
                                        txtTranslateSelection.Text = TranslateField(_strSelect, strEchoName);
                                    }
                                    else
                                    {
                                        txtSelect.Text = string.Empty;
                                        txtTranslateSelection.Text = string.Empty;
                                    }
                                }
                                else
                                {
                                    txtSelect.Text = string.Empty;
                                    txtTranslateSelection.Text = string.Empty;
                                }
                            }
                        }
                    }
                    break;

                case "SelectMentalAttribute":
                    using (frmSelectAttribute frmPickAttribute = new frmSelectAttribute(Backend.Attributes.AttributeSection.MentalAttributes.ToArray()))
                    {
                        frmPickAttribute.Description = LanguageManager.GetString("Title_SelectAttribute");
                        frmPickAttribute.ShowDialogSafe(this);

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
                        frmPickAttribute.ShowDialogSafe(this);

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
                        else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
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
                            frmPickAttribute.ShowDialogSafe(this);

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
                        frmPickSkill.ShowDialogSafe(this);

                        if (frmPickSkill.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkill.SelectedSkill;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkill.SelectedSkill);
                        }
                    }
                    break;

                case "SelectKnowSkill":
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstDropdownItems))
                    {
                        string strFilter = string.Empty;
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setProcessedSkillNames))
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                            {
                                lstDropdownItems.Add(
                                    new ListItem(objKnowledgeSkill.Name, objKnowledgeSkill.CurrentDisplayName));
                                setProcessedSkillNames.Add(objKnowledgeSkill.Name);
                            }

                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdFilters))
                            {
                                if (setProcessedSkillNames.Count > 0)
                                {
                                    sbdFilters.Append("not(");
                                    foreach (string strName in setProcessedSkillNames)
                                    {
                                        sbdFilters.Append("name = ").Append(strName.CleanXPath()).Append(" or ");
                                    }

                                    sbdFilters.Length -= 4;
                                    sbdFilters.Append(')');
                                }

                                if (sbdFilters.Length > 0)
                                    strFilter = '[' + sbdFilters.ToString() + ']';
                            }
                        }

                        foreach (XPathNavigator xmlSkill in _objCharacter.LoadDataXPath("skills.xml")
                                                                         .Select("/chummer/knowledgeskills/skill"
                                                                             + strFilter))
                        {
                            string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstDropdownItems.Add(
                                    new ListItem(
                                        strName,
                                        xmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                        }

                        lstDropdownItems.Sort(CompareListItems.CompareNames);

                        using (frmSelectItem frmPickSkill = new frmSelectItem
                               {
                                   Description = LanguageManager.GetString("Title_SelectSkill")
                               })
                        {
                            frmPickSkill.SetDropdownItemsMode(lstDropdownItems);
                            frmPickSkill.ShowDialogSafe(this);

                            if (frmPickSkill.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickSkill.SelectedItem;
                                txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkill.SelectedItem);
                            }
                        }
                    }
                }
                    break;

                case "SelectSkillCategory":
                    using (frmSelectSkillCategory frmPickSkillCategory = new frmSelectSkillCategory(_objCharacter))
                    {
                        frmPickSkillCategory.Description = LanguageManager.GetString("Title_SelectSkillCategory");
                        frmPickSkillCategory.ShowDialogSafe(this);

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
                        frmPickSkillGroup.ShowDialogSafe(this);

                        if (frmPickSkillGroup.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkillGroup.SelectedSkillGroup;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickSkillGroup.SelectedSkillGroup);
                        }
                    }
                    break;

                case "SelectComplexForm":
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstComplexForms))
                    {
                        foreach (XPathNavigator xmlSpell in _objCharacter.LoadDataXPath("complexforms.xml")
                                                                         .SelectAndCacheExpression(
                                                                             "/chummer/complexforms/complexform"))
                        {
                            string strName = xmlSpell.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstComplexForms.Add(new ListItem(
                                                        strName,
                                                        xmlSpell.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                        ?? strName));
                        }

                        using (frmSelectItem selectComplexForm = new frmSelectItem
                               {
                                   Description = LanguageManager.GetString("Title_SelectComplexForm")
                               })
                        {
                            selectComplexForm.SetDropdownItemsMode(lstComplexForms);
                            selectComplexForm.ShowDialogSafe(this);

                            if (selectComplexForm.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = selectComplexForm.SelectedName;
                                txtTranslateSelection.Text = TranslateField(_strSelect, selectComplexForm.SelectedName);
                            }
                        }
                    }

                    break;

                case "SelectSpell":
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstSpells))
                    {
                        foreach (XPathNavigator xmlSpell in _objCharacter.LoadDataXPath("spells.xml")
                                                                         .SelectAndCacheExpression(
                                                                             "/chummer/spells/spell"))
                        {
                            string strName = xmlSpell.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstSpells.Add(new ListItem(
                                                  strName,
                                                  xmlSpell.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                  ?? strName));
                        }

                        using (frmSelectItem selectSpell = new frmSelectItem
                               {
                                   Description = LanguageManager.GetString("Title_SelectSpell")
                               })
                        {
                            selectSpell.SetDropdownItemsMode(lstSpells);
                            selectSpell.ShowDialogSafe(this);

                            if (selectSpell.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = selectSpell.SelectedName;
                                txtTranslateSelection.Text = TranslateField(_strSelect, selectSpell.SelectedName);
                            }
                        }
                    }

                    break;

                case "SelectWeaponCategory":
                    using (frmSelectWeaponCategory frmPickWeaponCategory = new frmSelectWeaponCategory(_objCharacter))
                    {
                        frmPickWeaponCategory.Description = LanguageManager.GetString("Title_SelectWeaponCategory");
                        frmPickWeaponCategory.ShowDialogSafe(this);

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
                        frmPickSpellCategory.ShowDialogSafe(this);

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
                        frmPickPower.ShowDialogSafe(this);

                        if (frmPickPower.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = _objCharacter.LoadDataXPath("powers.xml").SelectSingleNode("/chummer/powers/power[id = " + frmPickPower.SelectedPower.CleanXPath() + "]/name")?.Value;
                            txtTranslateSelection.Text = TranslateField(_strSelect, frmPickPower.SelectedPower);
                        }
                    }
                    break;
            }
        }

        #endregion Control Events

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

            XmlDocument objBonusXml = new XmlDocument { XmlResolver = null };
            using (MemoryStream objStream = new MemoryStream())
            {
                // Here instead of later because objWriter.Close() needs Stream to not be disposed, but StreamReader.Close() will dispose the Stream.
                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                {
                    using (XmlWriter objWriter = XmlWriter.Create(objStream))
                    {
                        // Build the XML for the Improvement.
                        XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = " + cboImprovemetType.SelectedValue.ToString().CleanXPath() + "]");
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
                            .Replace("{val}", nudVal.Value.ToString(GlobalSettings.InvariantCultureInfo))
                            .Replace("{min}", nudMin.Value.ToString(GlobalSettings.InvariantCultureInfo))
                            .Replace("{max}", nudMax.Value.ToString(GlobalSettings.InvariantCultureInfo))
                            .Replace("{aug}", nudAug.Value.ToString(GlobalSettings.InvariantCultureInfo))
                            .Replace("{free}", chkFree.Checked.ToString(GlobalSettings.InvariantCultureInfo).ToLowerInvariant())
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
                        using (XmlReader objXmlReader = XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                            objBonusXml.Load(objXmlReader);
                    }
                }
            }

            // Pluck out the bonus information.
            XmlNode objNode = objBonusXml.SelectSingleNode("/bonus");

            // Pass it to the Improvement Manager so that it can be added to the character.
            string strGuid = Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo);
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
            else { Utils.BreakIfDebug(); }

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
            XPathNavigator objXmlNode;
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
                    if (ExoticSkill.IsExoticSkillName(strToTranslate))
                    {
                        string[] astrToTranslateParts = strToTranslate.Split('(', StringSplitOptions.RemoveEmptyEntries);
                        astrToTranslateParts[0] = astrToTranslateParts[0].Trim();
                        astrToTranslateParts[1] = astrToTranslateParts[1].Substring(0, astrToTranslateParts[1].Length - 1);

                        objXmlNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/skills/skill[name = " + astrToTranslateParts[0].CleanXPath() + "]");
                        string strFirstPartTranslated = objXmlNode?.SelectSingleNode("translate")?.Value ?? objXmlNode?.SelectSingleNode("name")?.Value ?? astrToTranslateParts[0];

                        return strFirstPartTranslated + LanguageManager.GetString("String_Space") + '(' + _objCharacter.TranslateExtra(astrToTranslateParts[1]) + ')';
                    }
                    else
                    {
                        objXmlNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/skills/skill[name = " + strToTranslate.CleanXPath() + "]");
                        return objXmlNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression("name")?.Value ?? strToTranslate;
                    }

                case "SelectKnowSkill":
                    objXmlNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode?.SelectSingleNode("name")?.Value ?? strToTranslate;

                case "SelectSkillCategory":
                    objXmlNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression(".")?.Value ?? strToTranslate;

                case "SelectSkillGroup":
                    objXmlNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/skillgroups/name[. = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression(".")?.Value ?? strToTranslate;

                case "SelectWeaponCategory":
                    objXmlNode = _objCharacter.LoadDataXPath("weapons.xml").SelectSingleNode("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression(".")?.Value ?? strToTranslate;

                case "SelectSpellCategory":
                    objXmlNode = _objCharacter.LoadDataXPath("spells.xml").SelectSingleNode("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression(".")?.Value ?? strToTranslate;

                case "SelectAdeptPower":
                    objXmlNode = _objCharacter.LoadDataXPath("powers.xml").SelectSingleNode("/chummer/powers/power[id = " + strToTranslate.CleanXPath() + " or name = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression("name")?.Value ?? strToTranslate;

                case "SelectMetamagic":
                    objXmlNode = _objCharacter.LoadDataXPath("metamagic.xml").SelectSingleNode("/chummer/metamagics/metamagic[name = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression("name")?.Value ?? strToTranslate;

                case "SelectEcho":
                    objXmlNode = _objCharacter.LoadDataXPath("echoes.xml").SelectSingleNode("/chummer/echoes/echo[name = " + strToTranslate.CleanXPath() + "]");
                    return objXmlNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode?.SelectSingleNodeAndCacheExpression("name")?.Value ?? strToTranslate;

                default:
                    return strToTranslate;
            }
        }

        #endregion Methods

        #region Properties

        public Improvement NewImprovement { get; private set; }

        /// <summary>
        /// Set Improvement object to edit.
        /// </summary>
        public Improvement EditImprovementObject { get; set; }

        #endregion Properties
    }
}
