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
using System.Threading.Tasks;
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

        private async void CreateImprovement_Load(object sender, EventArgs e)
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
                                                      ?? await LanguageManager.GetStringAsync("String_Unknown")));
                        }
                    }
                }

                lstTypes.Sort(CompareListItems.CompareNames);
                await cboImprovemetType.PopulateWithListItemsAsync(lstTypes);

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
                            = await TranslateField(objFetchNode?.InnerText, EditImprovementObject.ImprovedName);
                    }
                }
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cboImprovemetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = " + cboImprovemetType.SelectedValue.ToString().CleanXPath() + ']');

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

        private async void cmdChangeSelection_Click(object sender, EventArgs e)
        {
            switch (_strSelect)
            {
                case "SelectActionDicePool":
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstActions))
                    {
                        foreach (XPathNavigator xmlAction in await (await _objCharacter.LoadDataXPathAsync("actions.xml"))
                                     .SelectAndCacheExpressionAsync(
                                         "/chummer/actions/action"))
                        {
                            string strName = (await xmlAction.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstActions.Add(new ListItem(
                                                   strName,
                                                   (await xmlAction.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                   ?? strName));
                        }

                        using (SelectItem frmSelectAction = new SelectItem
                               {
                                   Description = await LanguageManager.GetStringAsync("Title_SelectAction")
                               })
                        {
                            frmSelectAction.SetDropdownItemsMode(lstActions);
                            await frmSelectAction.ShowDialogSafeAsync(this);

                            if (frmSelectAction.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmSelectAction.SelectedName;
                                txtTranslateSelection.Text = await TranslateField(_strSelect, frmSelectAction.SelectedName);
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
                        using (SelectAttribute frmPickAttribute = new SelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = await LanguageManager.GetStringAsync("Title_SelectAttribute")
                        })
                        {
                            await frmPickAttribute.ShowDialogSafeAsync(this);

                            if (frmPickAttribute.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickAttribute.SelectedAttribute;
                                txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                            }
                        }
                    }
                    break;

                case "SelectEcho":
                    {
                        InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1, Technomancer = true };
                        using (SelectMetamagic frmPickMetamagic = new SelectMetamagic(_objCharacter, objGrade))
                        {
                            await frmPickMetamagic.ShowDialogSafeAsync(this);
                            if (frmPickMetamagic.DialogResult == DialogResult.OK)
                            {
                                string strSelectedId = frmPickMetamagic.SelectedMetamagic;
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    string strEchoName = (await _objCharacter.LoadDataXPathAsync("echoes.xml"))
                                                                             .SelectSingleNode(
                                                                                 "/chummer/echoes/echo[id = " + strSelectedId.CleanXPath() + "]/name")?.Value;
                                    if (!string.IsNullOrEmpty(strEchoName))
                                    {
                                        txtSelect.Text = strEchoName;
                                        txtTranslateSelection.Text = await TranslateField(_strSelect, strEchoName);
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
                        using (SelectMetamagic frmPickMetamagic = new SelectMetamagic(_objCharacter, objGrade))
                        {
                            await frmPickMetamagic.ShowDialogSafeAsync(this);
                            if (frmPickMetamagic.DialogResult == DialogResult.OK)
                            {
                                string strSelectedId = frmPickMetamagic.SelectedMetamagic;
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    string strEchoName = (await _objCharacter.LoadDataXPathAsync("metamagic.xml"))
                                                                      .SelectSingleNode(
                                                                          "/chummer/metamagics/metamagic[id = " + strSelectedId.CleanXPath() + "]/name")?.Value;
                                    if (!string.IsNullOrEmpty(strEchoName))
                                    {
                                        txtSelect.Text = strEchoName;
                                        txtTranslateSelection.Text = await TranslateField(_strSelect, strEchoName);
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
                    using (SelectAttribute frmPickAttribute = new SelectAttribute(Backend.Attributes.AttributeSection.MentalAttributes.ToArray()))
                    {
                        frmPickAttribute.Description = await LanguageManager.GetStringAsync("Title_SelectAttribute");
                        await frmPickAttribute.ShowDialogSafeAsync(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                        }
                    }
                    break;

                case "SelectPhysicalAttribute":
                    using (SelectAttribute frmPickAttribute = new SelectAttribute(Backend.Attributes.AttributeSection.PhysicalAttributes.ToArray()))
                    {
                        frmPickAttribute.Description = await LanguageManager.GetStringAsync("Title_SelectAttribute");
                        await frmPickAttribute.ShowDialogSafeAsync(this);

                        if (frmPickAttribute.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickAttribute.SelectedAttribute;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
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
                        using (SelectAttribute frmPickAttribute = new SelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = await LanguageManager.GetStringAsync("Title_SelectAttribute")
                        })
                        {
                            await frmPickAttribute.ShowDialogSafeAsync(this);

                            if (frmPickAttribute.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickAttribute.SelectedAttribute;
                                txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickAttribute.SelectedAttribute);
                            }
                        }
                    }
                    break;

                case "SelectSkill":
                    using (SelectSkill frmPickSkill = new SelectSkill(_objCharacter))
                    {
                        frmPickSkill.Description = await LanguageManager.GetStringAsync("Title_SelectSkill");
                        await frmPickSkill.ShowDialogSafeAsync(this);

                        if (frmPickSkill.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkill.SelectedSkill;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickSkill.SelectedSkill);
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

                        foreach (XPathNavigator xmlSkill in (await _objCharacter.LoadDataXPathAsync("skills.xml"))
                                                                         .Select("/chummer/knowledgeskills/skill"
                                                                             + strFilter))
                        {
                            string strName = (await xmlSkill.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstDropdownItems.Add(
                                    new ListItem(
                                        strName,
                                        (await xmlSkill.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? strName));
                        }

                        lstDropdownItems.Sort(CompareListItems.CompareNames);

                        using (SelectItem frmPickSkill = new SelectItem
                               {
                                   Description = await LanguageManager.GetStringAsync("Title_SelectSkill")
                               })
                        {
                            frmPickSkill.SetDropdownItemsMode(lstDropdownItems);
                            await frmPickSkill.ShowDialogSafeAsync(this);

                            if (frmPickSkill.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = frmPickSkill.SelectedItem;
                                txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickSkill.SelectedItem);
                            }
                        }
                    }
                }
                    break;

                case "SelectSkillCategory":
                    using (SelectSkillCategory frmPickSkillCategory = new SelectSkillCategory(_objCharacter))
                    {
                        frmPickSkillCategory.Description = await LanguageManager.GetStringAsync("Title_SelectSkillCategory");
                        await frmPickSkillCategory.ShowDialogSafeAsync(this);

                        if (frmPickSkillCategory.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkillCategory.SelectedCategory;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickSkillCategory.SelectedCategory);
                        }
                    }
                    break;

                case "SelectSkillGroup":
                    using (SelectSkillGroup frmPickSkillGroup = new SelectSkillGroup(_objCharacter))
                    {
                        frmPickSkillGroup.Description = await LanguageManager.GetStringAsync("Title_SelectSkillGroup");
                        await frmPickSkillGroup.ShowDialogSafeAsync(this);

                        if (frmPickSkillGroup.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSkillGroup.SelectedSkillGroup;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickSkillGroup.SelectedSkillGroup);
                        }
                    }
                    break;

                case "SelectComplexForm":
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstComplexForms))
                    {
                        foreach (XPathNavigator xmlSpell in await (await _objCharacter.LoadDataXPathAsync("complexforms.xml"))
                                     .SelectAndCacheExpressionAsync(
                                         "/chummer/complexforms/complexform"))
                        {
                            string strName = (await xmlSpell.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstComplexForms.Add(new ListItem(
                                                        strName,
                                                        (await xmlSpell.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                        ?? strName));
                        }

                        using (SelectItem selectComplexForm = new SelectItem
                               {
                                   Description = await LanguageManager.GetStringAsync("Title_SelectComplexForm")
                               })
                        {
                            selectComplexForm.SetDropdownItemsMode(lstComplexForms);
                            await selectComplexForm.ShowDialogSafeAsync(this);

                            if (selectComplexForm.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = selectComplexForm.SelectedName;
                                txtTranslateSelection.Text = await TranslateField(_strSelect, selectComplexForm.SelectedName);
                            }
                        }
                    }

                    break;

                case "SelectSpell":
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstSpells))
                    {
                        foreach (XPathNavigator xmlSpell in await (await _objCharacter.LoadDataXPathAsync("spells.xml"))
                                     .SelectAndCacheExpressionAsync(
                                         "/chummer/spells/spell"))
                        {
                            string strName = (await xmlSpell.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstSpells.Add(new ListItem(
                                                  strName,
                                                  (await xmlSpell.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                  ?? strName));
                        }

                        using (SelectItem selectSpell = new SelectItem
                               {
                                   Description = await LanguageManager.GetStringAsync("Title_SelectSpell")
                               })
                        {
                            selectSpell.SetDropdownItemsMode(lstSpells);
                            await selectSpell.ShowDialogSafeAsync(this);

                            if (selectSpell.DialogResult == DialogResult.OK)
                            {
                                txtSelect.Text = selectSpell.SelectedName;
                                txtTranslateSelection.Text = await TranslateField(_strSelect, selectSpell.SelectedName);
                            }
                        }
                    }

                    break;

                case "SelectWeaponCategory":
                    using (SelectWeaponCategory frmPickWeaponCategory = new SelectWeaponCategory(_objCharacter))
                    {
                        frmPickWeaponCategory.Description = await LanguageManager.GetStringAsync("Title_SelectWeaponCategory");
                        await frmPickWeaponCategory.ShowDialogSafeAsync(this);

                        if (frmPickWeaponCategory.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickWeaponCategory.SelectedCategory;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickWeaponCategory.SelectedCategory);
                        }
                    }
                    break;

                case "SelectSpellCategory":
                    using (SelectSpellCategory frmPickSpellCategory = new SelectSpellCategory(_objCharacter))
                    {
                        frmPickSpellCategory.Description = await LanguageManager.GetStringAsync("Title_SelectSpellCategory");
                        await frmPickSpellCategory.ShowDialogSafeAsync(this);

                        if (frmPickSpellCategory.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = frmPickSpellCategory.SelectedCategory;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickSpellCategory.SelectedCategory);
                        }
                    }
                    break;

                case "SelectAdeptPower":
                    using (SelectPower frmPickPower = new SelectPower(_objCharacter))
                    {
                        frmPickPower.IgnoreLimits = chkIgnoreLimits.Checked;
                        await frmPickPower.ShowDialogSafeAsync(this);

                        if (frmPickPower.DialogResult == DialogResult.OK)
                        {
                            txtSelect.Text = (await _objCharacter.LoadDataXPathAsync("powers.xml")).SelectSingleNode("/chummer/powers/power[id = " + frmPickPower.SelectedPower.CleanXPath() + "]/name")?.Value;
                            txtTranslateSelection.Text = await TranslateField(_strSelect, frmPickPower.SelectedPower);
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
        private async ValueTask AcceptForm()
        {
            // Make sure a value has been selected if necessary.
            if (await txtTranslateSelection.DoThreadSafeFuncAsync(x => x.Visible) && string.IsNullOrEmpty(await txtSelect.DoThreadSafeFuncAsync(x => x.Text)))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectItem"), await LanguageManager.GetStringAsync("MessageTitle_SelectItem"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Make sure a value has been provided for the name.
            if (string.IsNullOrEmpty(await txtName.DoThreadSafeFuncAsync(x => x.Text)))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ImprovementName"), await LanguageManager.GetStringAsync("MessageTitle_ImprovementName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                await txtName.DoThreadSafeAsync(x => x.Focus());
                return;
            }

            XmlDocument objBonusXml = new XmlDocument { XmlResolver = null };
            using (MemoryStream objStream = new MemoryStream())
            {
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    // Build the XML for the Improvement.
                    XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = " + (await cboImprovemetType.DoThreadSafeFuncAsync(x => x.SelectedValue)).ToString().CleanXPath() + ']');
                    string strInternal = objFetchNode?["internal"]?.InnerText;
                    if (string.IsNullOrEmpty(strInternal))
                        return;
                    await objWriter.WriteStartDocumentAsync();
                    // <bonus>
                    XmlElementWriteHelper objBonusNode = await objWriter.StartElementAsync("bonus");
                    try
                    {
                        // <whatever element>
                        XmlElementWriteHelper objInternalNode = await objWriter.StartElementAsync(strInternal);
                        try
                        {
                            // Retrieve the XML data from the document and replace the values as necessary.
                            XmlAttributeCollection xmlAttributeCollection = objFetchNode["xml"]?.Attributes;
                            if (xmlAttributeCollection != null)
                            {
                                foreach (XmlAttribute xmlAttribute in xmlAttributeCollection)
                                {
                                    await objWriter.WriteAttributeStringAsync(
                                        xmlAttribute.LocalName, xmlAttribute.Value);
                                }
                            }

                            // ReSharper disable once PossibleNullReferenceException
                            string strXml = await objFetchNode["xml"].InnerText
                                                                     .CheapReplaceAsync("{val}",
                                                                         async () => await nudVal.DoThreadSafeFuncAsync(
                                                                             x => x.Value.ToString(
                                                                                 GlobalSettings.InvariantCultureInfo)))
                                                                     .CheapReplaceAsync("{min}",
                                                                         async () => await nudMin.DoThreadSafeFuncAsync(
                                                                             x => x.Value.ToString(
                                                                                 GlobalSettings.InvariantCultureInfo)))
                                                                     .CheapReplaceAsync("{max}",
                                                                         async () => await nudMax.DoThreadSafeFuncAsync(
                                                                             x => x.Value.ToString(
                                                                                 GlobalSettings.InvariantCultureInfo)))
                                                                     .CheapReplaceAsync("{aug}",
                                                                         async () => await nudAug.DoThreadSafeFuncAsync(
                                                                             x => x.Value.ToString(
                                                                                 GlobalSettings.InvariantCultureInfo)))
                                                                     .CheapReplaceAsync("{free}",
                                                                         async () =>
                                                                             await chkFree.DoThreadSafeFuncAsync(
                                                                                 x => x.Checked.ToString(
                                                                                     GlobalSettings
                                                                                         .InvariantCultureInfo)))
                                                                     .CheapReplaceAsync("{select}",
                                                                         async () => await txtSelect
                                                                             .DoThreadSafeFuncAsync(
                                                                                 x => x.Text))
                                                                     .CheapReplaceAsync(
                                                                         "{applytorating}",
                                                                         async () =>
                                                                             await chkApplyToRating
                                                                                 .DoThreadSafeFuncAsync(x => x.Checked)
                                                                                 ? "<applytorating>True</applytorating>"
                                                                                 : string.Empty);
                            await objWriter.WriteRawAsync(strXml);

                            // Write the rest of the document.
                        }
                        finally
                        {
                            // </whatever element>
                            await objInternalNode.DisposeAsync();
                        }
                    }
                    finally
                    {
                        // </bonus>
                        await objBonusNode.DisposeAsync();
                    }
                    await objWriter.WriteEndDocumentAsync();
                    await objWriter.FlushAsync();
                }

                objStream.Position = 0;

                    // Read it back in as an XmlDocument.
                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                using (XmlReader objXmlReader = XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                    objBonusXml.Load(objXmlReader);
            }

            // Pluck out the bonus information.
            XmlNode objNode = objBonusXml.SelectSingleNode("/bonus");

            // Pass it to the Improvement Manager so that it can be added to the character.
            string strGuid = Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo);
            await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Custom, strGuid, objNode, 1, txtName.Text);

            // If an Improvement was passed in, remove it from the character.
            string strNotes = string.Empty;
            int intOrder = 0;
            if (EditImprovementObject != null)
            {
                // Copy the notes over to the new item.
                strNotes = EditImprovementObject.Notes;
                intOrder = EditImprovementObject.SortOrder;
                await ImprovementManager.RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Custom, EditImprovementObject.SourceName);
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
        private async ValueTask<string> TranslateField(string strImprovementType, string strToTranslate)
        {
            XPathNavigator objXmlNode;
            switch (strImprovementType)
            {
                case "SelectAttribute":
                case "SelectPhysicalAttribute":
                case "SelectMentalAttribute":
                case "SelectSpecialAttribute":
                    return strToTranslate == "MAGAdept"
                    ? await LanguageManager.GetStringAsync("String_AttributeMAGShort") + await LanguageManager.GetStringAsync("String_Space") + '(' + await LanguageManager.GetStringAsync("String_DescAdept") + ')'
                    : await LanguageManager.GetStringAsync("String_Attribute" + strToTranslate + "Short");

                case "SelectSkill":
                    if (ExoticSkill.IsExoticSkillName(strToTranslate))
                    {
                        string[] astrToTranslateParts = strToTranslate.Split('(', StringSplitOptions.RemoveEmptyEntries);
                        astrToTranslateParts[0] = astrToTranslateParts[0].Trim();
                        astrToTranslateParts[1] = astrToTranslateParts[1].Substring(0, astrToTranslateParts[1].Length - 1);

                        objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml")).SelectSingleNode("/chummer/skills/skill[name = " + astrToTranslateParts[0].CleanXPath() + ']');
                        string strFirstPartTranslated = objXmlNode != null
                            ? ((await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                               ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                               ?? astrToTranslateParts[0])
                            : astrToTranslateParts[0];

                        return strFirstPartTranslated + await LanguageManager.GetStringAsync("String_Space") + '(' + await _objCharacter.TranslateExtraAsync(astrToTranslateParts[1]) + ')';
                    }
                    else
                    {
                        objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml")).SelectSingleNode("/chummer/skills/skill[name = " + strToTranslate.CleanXPath() + ']');
                        if (objXmlNode == null)
                            return strToTranslate;
                        return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value ?? strToTranslate;
                    }

                case "SelectKnowSkill":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml")).SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value ?? strToTranslate;

                case "SelectSkillCategory":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml")).SelectSingleNode("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("."))?.Value ?? strToTranslate;

                case "SelectSkillGroup":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml")).SelectSingleNode("/chummer/skillgroups/name[. = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("."))?.Value ?? strToTranslate;

                case "SelectWeaponCategory":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("weapons.xml")).SelectSingleNode("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("."))?.Value ?? strToTranslate;

                case "SelectSpellCategory":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("spells.xml")).SelectSingleNode("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("."))?.Value ?? strToTranslate;

                case "SelectAdeptPower":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("powers.xml")).SelectSingleNode("/chummer/powers/power[id = " + strToTranslate.CleanXPath() + " or name = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value ?? strToTranslate;

                case "SelectMetamagic":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("metamagic.xml")).SelectSingleNode("/chummer/metamagics/metamagic[name = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value ?? strToTranslate;

                case "SelectEcho":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("echoes.xml")).SelectSingleNode("/chummer/echoes/echo[name = " + strToTranslate.CleanXPath() + ']');
                    if (objXmlNode == null)
                        return strToTranslate;
                    return (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? (await objXmlNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value ?? strToTranslate;

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
