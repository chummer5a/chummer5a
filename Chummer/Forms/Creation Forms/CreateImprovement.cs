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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Skills;
using Microsoft.IO;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class CreateImprovement : Form, IHasCharacterObject
    {
        private readonly Character _objCharacter;
        private readonly XPathNavigator _objImprovementsDocumentImprovementsNode;
        private string _strSelect = string.Empty;
        private readonly string _strCustomGroup;

        public Character CharacterObject => _objCharacter;

        #region Control Events

        public CreateImprovement(Character objCharacter, string strCustomGroup = "")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            _strCustomGroup = strCustomGroup;
            _objImprovementsDocumentImprovementsNode = objCharacter.LoadDataXPath("improvements.xml").SelectSingleNode("/chummer/improvements");
        }

        private async void CreateImprovement_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            {
                // Populate the Improvement Type list.
                foreach (XPathNavigator objXmlImprovement in _objImprovementsDocumentImprovementsNode.Select("improvement"))
                {
                    string strId = objXmlImprovement.SelectSingleNodeAndCacheExpression("id")?.Value;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        lstTypes.Add(new ListItem(strId,
                                                  objXmlImprovement
                                                      .SelectSingleNodeAndCacheExpression("translate")?.Value
                                                  ?? objXmlImprovement
                                                      .SelectSingleNodeAndCacheExpression("name")?.Value
                                                  ?? await LanguageManager.GetStringAsync("String_Unknown")
                                                                          .ConfigureAwait(false)));
                    }
                }

                lstTypes.Sort(CompareListItems.CompareNames);
                await cboImprovemetType.PopulateWithListItemsAsync(lstTypes).ConfigureAwait(false);

                // Load the information from the passed Improvement if one has been given.
                if (EditImprovementObject != null)
                {
                    await cboImprovemetType.DoThreadSafeAsync(x => x.SelectedValue = EditImprovementObject.CustomId).ConfigureAwait(false);
                    await txtName.DoThreadSafeAsync(x => x.Text = EditImprovementObject.CustomName).ConfigureAwait(false);
                    await nudMax.DoThreadSafeAsync(x =>
                    {
                        if (x.Visible)
                            x.Value = EditImprovementObject.Maximum;
                    }).ConfigureAwait(false);
                    await nudMin.DoThreadSafeAsync(x =>
                    {
                        if (x.Visible)
                            x.Value = EditImprovementObject.Minimum;
                    }).ConfigureAwait(false);
                    await nudVal.DoThreadSafeAsync(x =>
                    {
                        if (x.Visible)
                        {
                            switch (EditImprovementObject.ImproveType)
                            {
                                // Attribute improvements store the Value in Augmented instead.
                                case Improvement.ImprovementType.Attribute:
                                    x.Value = EditImprovementObject.Augmented;
                                    break;
                                // Adept power level improvements store the Value in Rating instead.
                                case Improvement.ImprovementType.AdeptPowerFreeLevels:
                                    x.Value = EditImprovementObject.Rating;
                                    break;
                                default:
                                    x.Value = EditImprovementObject.Value;
                                    break;
                            }
                        }
                    }).ConfigureAwait(false);

                    await chkApplyToRating.DoThreadSafeAsync(x => x.Checked = x.Visible && EditImprovementObject.AddToRating).ConfigureAwait(false);
                    if (await txtTranslateSelection.DoThreadSafeFuncAsync(x => x.Visible).ConfigureAwait(false))
                    {
                        await txtSelect.DoThreadSafeAsync(x => x.Text = EditImprovementObject.ImprovedName).ConfigureAwait(false);
                        // get the selection type of improvement and generate translation
                        XPathNavigator xmlImprovementNode
                            = _objImprovementsDocumentImprovementsNode.TryGetNodeByNameOrId(
                                "improvement",
                                await cboImprovemetType.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false), blnIdIsGuid: false);
                        XPathNavigator objFetchNode = xmlImprovementNode?.SelectSingleNodeAndCacheExpression("fields/field");
                        string strText
                            = await TranslateField(objFetchNode?.Value, EditImprovementObject.ImprovedName).ConfigureAwait(false);
                        await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);
                    }
                }
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboImprovemetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            XPathNavigator objFetchNode = _objImprovementsDocumentImprovementsNode.TryGetNodeByNameOrId("improvement", cboImprovemetType.SelectedValue.ToString(), blnIdIsGuid: false);

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

            if (objFetchNode == null)
                return;
            foreach (XPathNavigator objNode in objFetchNode.Select("fields/field"))
            {
                switch (objNode.Value)
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
                        if (objNode.Value.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                        {
                            lblSelect.Visible = true;
                            txtTranslateSelection.Visible = true;
                            cmdChangeSelection.Visible = true;
                            _strSelect = objNode.Value;
                        }
                        break;
                }
            }

            // Display the help information.
            txtHelp.Text = objFetchNode.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objFetchNode.SelectSingleNodeAndCacheExpression("page")?.Value;
            chkIgnoreLimits.Visible = _strSelect == "SelectAdeptPower";
        }

        private async void cmdChangeSelection_Click(object sender, EventArgs e)
        {
            switch (_strSelect)
            {
                case "SelectActionDicePool":
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstActions))
                    {
                        foreach (XPathNavigator xmlAction in (await _objCharacter.LoadDataXPathAsync("actions.xml").ConfigureAwait(false))
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

                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectAction").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectItem> frmSelectAction = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem()).ConfigureAwait(false))
                        {
                            await frmSelectAction.MyForm.DoThreadSafeAsync(x => x.Description = strDescription).ConfigureAwait(false);
                            frmSelectAction.MyForm.SetDropdownItemsMode(lstActions);

                            if (await frmSelectAction.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = await frmSelectAction.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName).ConfigureAwait(false);
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }

                    break;

                case "SelectAttribute":
                    {
                        List<string> lstAbbrevs = new List<string>(Backend.Attributes.AttributeSection.AttributeStrings);

                        lstAbbrevs.Remove("ESS");
                        if (!await _objCharacter.GetMAGEnabledAsync().ConfigureAwait(false))
                        {
                            lstAbbrevs.Remove("MAG");
                            lstAbbrevs.Remove("MAGAdept");
                        }
                        else if (!await _objCharacter.GetIsMysticAdeptAsync().ConfigureAwait(false) || !await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync().ConfigureAwait(false))
                            lstAbbrevs.Remove("MAGAdept");
                        if (!await _objCharacter.GetRESEnabledAsync().ConfigureAwait(false))
                            lstAbbrevs.Remove("RES");
                        if (!await _objCharacter.GetDEPEnabledAsync().ConfigureAwait(false))
                            lstAbbrevs.Remove("DEP");

                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectAttribute").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectAttribute> frmPickAttribute = await ThreadSafeForm<SelectAttribute>.GetAsync(
                                   () => new SelectAttribute(lstAbbrevs.ToArray())
                                   {
                                       Description = strDescription
                                   }).ConfigureAwait(false))
                        {
                            if (await frmPickAttribute.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = frmPickAttribute.MyForm.SelectedAttribute;
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }
                    break;

                case "SelectEcho":
                    {
                        InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1, Technomancer = true };
                        using (ThreadSafeForm<SelectMetamagic> frmPickMetamagic =
                               await ThreadSafeForm<SelectMetamagic>.GetAsync(() =>
                                                                                  new SelectMetamagic(_objCharacter, objGrade)).ConfigureAwait(false))
                        {
                            if (await frmPickMetamagic.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelectedId = frmPickMetamagic.MyForm.SelectedMetamagic;
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    XPathNavigator xmlEchoNode
                                        = (await _objCharacter.LoadDataXPathAsync("echoes.xml").ConfigureAwait(false))
                                        .TryGetNodeByNameOrId(
                                            "/chummer/echoes/echo", strSelectedId);
                                    string strEchoName = xmlEchoNode != null
                                        ? xmlEchoNode.SelectSingleNodeAndCacheExpression("name")?.Value
                                        : string.Empty;
                                    if (!string.IsNullOrEmpty(strEchoName))
                                    {
                                        await txtSelect.DoThreadSafeAsync(x => x.Text = strEchoName).ConfigureAwait(false);
                                        string strTranslateSelection = await TranslateField(_strSelect, strEchoName).ConfigureAwait(false);
                                        await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await txtSelect.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                        await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    await txtSelect.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                    await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    break;

                case "SelectMetamagic":
                    {
                        InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1 };
                        using (ThreadSafeForm<SelectMetamagic> frmPickMetamagic =
                               await ThreadSafeForm<SelectMetamagic>.GetAsync(() =>
                                                                                  new SelectMetamagic(_objCharacter, objGrade)).ConfigureAwait(false))
                        {
                            if (await frmPickMetamagic.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelectedId = frmPickMetamagic.MyForm.SelectedMetamagic;
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    XPathNavigator xmlMetamagicNode
                                        = (await _objCharacter.LoadDataXPathAsync("metamagic.xml").ConfigureAwait(false))
                                        .TryGetNodeByNameOrId(
                                            "/chummer/metamagics/metamagic", strSelectedId);
                                    string strMetamagicName = xmlMetamagicNode != null
                                        ? xmlMetamagicNode.SelectSingleNodeAndCacheExpression("name")?.Value
                                        : string.Empty;
                                    if (!string.IsNullOrEmpty(strMetamagicName))
                                    {
                                        await txtSelect.DoThreadSafeAsync(x => x.Text = strMetamagicName).ConfigureAwait(false);
                                        string strTranslateSelection = await TranslateField(_strSelect, strMetamagicName).ConfigureAwait(false);
                                        await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await txtSelect.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                        await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    await txtSelect.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                    await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    break;

                case "SelectMentalAttribute":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectAttribute").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectAttribute> frmPickAttribute =
                           await ThreadSafeForm<SelectAttribute>.GetAsync(() =>
                                                                              new SelectAttribute(Backend.Attributes.AttributeSection.MentalAttributes.ToArray())
                                                                                  { Description = strDescription }).ConfigureAwait(false))
                    {
                        if (await frmPickAttribute.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickAttribute.MyForm.SelectedAttribute;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
                case "SelectPhysicalAttribute":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectAttribute").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectAttribute> frmPickAttribute =
                           await ThreadSafeForm<SelectAttribute>.GetAsync(() =>
                                                                              new SelectAttribute(Backend.Attributes.AttributeSection.PhysicalAttributes.ToArray())
                                                                                  { Description = strDescription }).ConfigureAwait(false))
                    {
                        if (await frmPickAttribute.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickAttribute.MyForm.SelectedAttribute;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
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
                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectAttribute").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectAttribute> frmPickAttribute = await ThreadSafeForm<SelectAttribute>.GetAsync(() => new SelectAttribute(lstAbbrevs.ToArray())
                               {
                                   Description = strDescription
                               }).ConfigureAwait(false))
                        {
                            if (await frmPickAttribute.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = frmPickAttribute.MyForm.SelectedAttribute;
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }
                    break;

                case "SelectSkill":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectSkill").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectSkill> frmPickSkill =
                           await ThreadSafeForm<SelectSkill>.GetAsync(() => new SelectSkill(_objCharacter)).ConfigureAwait(false))
                    {
                        await frmPickSkill.MyForm.DoThreadSafeAsync(x => x.Description = strDescription).ConfigureAwait(false);
                        if (await frmPickSkill.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickSkill.MyForm.SelectedSkill;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
                case "SelectKnowSkill":
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                               out List<ListItem> lstDropdownItems))
                    {
                        string strFilter = string.Empty;
                        using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                   out HashSet<string> setProcessedSkillNames))
                        {
                            await _objCharacter.SkillsSection.KnowledgeSkills.ForEachAsync(async objKnowledgeSkill =>
                            {
                                lstDropdownItems.Add(
                                    new ListItem(objKnowledgeSkill.Name,
                                                 await objKnowledgeSkill.GetCurrentDisplayNameAsync()
                                                                        .ConfigureAwait(false)));
                                setProcessedSkillNames.Add(objKnowledgeSkill.Name);
                            }).ConfigureAwait(false);

                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                       out StringBuilder sbdFilter))
                            {
                                if (setProcessedSkillNames.Count > 0)
                                {
                                    sbdFilter.Append("not(");
                                    foreach (string strName in setProcessedSkillNames)
                                    {
                                        sbdFilter.Append("name = ", strName.CleanXPath(), " or ");
                                    }

                                    sbdFilter.Length -= 4;
                                    sbdFilter.Append(')');
                                }

                                if (sbdFilter.Length > 0)
                                    strFilter = sbdFilter.Insert(0, '[').Append(']').ToString();
                            }
                        }

                        foreach (XPathNavigator xmlSkill in (await _objCharacter.LoadDataXPathAsync("skills.xml").ConfigureAwait(false))
                                 .Select("/chummer/knowledgeskills/skill[not(hide)]"
                                         + strFilter))
                        {
                            string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstDropdownItems.Add(
                                    new ListItem(
                                        strName,
                                        xmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value ??
                                        strName));
                        }

                        lstDropdownItems.Sort(CompareListItems.CompareNames);

                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectSkill").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectItem> frmPickSkill = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem()).ConfigureAwait(false))
                        {
                            await frmPickSkill.MyForm.DoThreadSafeAsync(x => x.Description = strDescription).ConfigureAwait(false);
                            frmPickSkill.MyForm.SetDropdownItemsMode(lstDropdownItems);
                            if (await frmPickSkill.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = await frmPickSkill.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem).ConfigureAwait(false);
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }

                    break;
                }
                case "SelectSkillCategory":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectSkillCategory").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectSkillCategory> frmPickSkillCategory = await ThreadSafeForm<SelectSkillCategory>.GetAsync(() => new SelectSkillCategory(_objCharacter) { Description = strDescription }).ConfigureAwait(false))
                    {
                        if (await frmPickSkillCategory.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickSkillCategory.MyForm.SelectedCategory;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
                case "SelectSkillGroup":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectSkillGroup").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectSkillGroup> frmPickSkillGroup = await ThreadSafeForm<SelectSkillGroup>.GetAsync(() => new SelectSkillGroup(_objCharacter) { Description = strDescription }).ConfigureAwait(false))
                    {
                        if (await frmPickSkillGroup.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickSkillGroup.MyForm.SelectedSkillGroup;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
                case "SelectComplexForm":
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                               out List<ListItem> lstComplexForms))
                    {
                        foreach (XPathNavigator xmlSpell in (await _objCharacter.LoadDataXPathAsync(
                                                                      "complexforms.xml").ConfigureAwait(false))
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

                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectComplexForm").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectItem> selectComplexForm = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem()).ConfigureAwait(false))
                        {
                            await selectComplexForm.MyForm.DoThreadSafeAsync(x => x.Description = strDescription).ConfigureAwait(false);
                            selectComplexForm.MyForm.SetDropdownItemsMode(lstComplexForms);

                            if (await selectComplexForm.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = await selectComplexForm.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName).ConfigureAwait(false);
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }

                    break;
                }
                case "SelectSpell":
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                               out List<ListItem> lstSpells))
                    {
                        foreach (XPathNavigator xmlSpell in (await _objCharacter.LoadDataXPathAsync("spells.xml").ConfigureAwait(false))
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

                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectSpell").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectItem> selectSpell = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem()).ConfigureAwait(false))
                        {
                            await selectSpell.MyForm.DoThreadSafeAsync(x => x.Description = strDescription).ConfigureAwait(false);
                            selectSpell.MyForm.SetDropdownItemsMode(lstSpells);

                            if (await selectSpell.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = await selectSpell.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName).ConfigureAwait(false);
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }

                    break;
                }
                case "SelectWeaponCategory":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectWeaponCategory").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectWeaponCategory> frmPickWeaponCategory = await ThreadSafeForm<SelectWeaponCategory>.GetAsync(() => new SelectWeaponCategory(_objCharacter) { Description = strDescription }).ConfigureAwait(false))
                    {
                        if (await frmPickWeaponCategory.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickWeaponCategory.MyForm.SelectedCategory;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
                case "SelectSpellCategory":
                {
                    string strDescription = await LanguageManager.GetStringAsync("Title_SelectSpellCategory").ConfigureAwait(false);
                    using (ThreadSafeForm<SelectSpellCategory> frmPickSpellCategory = await ThreadSafeForm<SelectSpellCategory>.GetAsync(() => new SelectSpellCategory(_objCharacter) { Description = strDescription }).ConfigureAwait(false))
                    {
                        if (await frmPickSpellCategory.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelect = frmPickSpellCategory.MyForm.SelectedCategory;
                            await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                            string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                            await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                        }
                    }

                    break;
                }
                case "SelectAdeptPower":
                    using (ThreadSafeForm<SelectPower> frmPickPower = await ThreadSafeForm<SelectPower>.GetAsync(() => new SelectPower(_objCharacter) { IgnoreLimits = chkIgnoreLimits.Checked }).ConfigureAwait(false))
                    {
                        if (await frmPickPower.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                        {
                            string strSelectedId = frmPickPower.MyForm.SelectedPower;
                            if (!string.IsNullOrEmpty(strSelectedId))
                            {
                                string strPowerName
                                    = (await _objCharacter.LoadDataXPathAsync("powers.xml").ConfigureAwait(false))
                                      .TryGetNodeByNameOrId(
                                          "/chummer/powers/power", frmPickPower.MyForm.SelectedPower)
                                      ?.SelectSingleNode("name")
                                      ?.Value;
                                if (!string.IsNullOrEmpty(strPowerName))
                                {
                                    await txtSelect.DoThreadSafeAsync(x => x.Text = strPowerName).ConfigureAwait(false);
                                    string strTranslateSelection = await TranslateField(_strSelect, strPowerName).ConfigureAwait(false);
                                    await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                                }
                                else
                                {
                                    await txtSelect.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                    await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await txtSelect.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                            }
                        }
                    }
                    break;
                case "SelectXPath":
                {
                    // Get the current improvement type and find the SelectXPath field node
                    string strSelectedType = cboImprovemetType.SelectedValue?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(strSelectedType))
                    {
                        await Program.ShowScrollableMessageBoxAsync(this,
                            await LanguageManager.GetStringAsync("Message_Improvement_SelectXPath_Configuration", token: default).ConfigureAwait(false) ?? "SelectXPath field requires xml and xpath attributes.",
                            await LanguageManager.GetStringAsync("MessageTitle_Improvement_SelectXPath_Configuration", token: default).ConfigureAwait(false) ?? "Configuration Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                        break;
                    }

                    XPathNavigator xmlImprovementNode = _objImprovementsDocumentImprovementsNode.TryGetNodeByNameOrId("improvement", strSelectedType, blnIdIsGuid: false);
                    XPathNavigator xmlSelectXPathField = xmlImprovementNode?.SelectSingleNodeAndCacheExpression("fields/field[. = 'SelectXPath']");
                        
                    string strSelectXPathXml = xmlSelectXPathField?.SelectSingleNodeAndCacheExpression("@xml")?.Value ?? string.Empty;
                    string strSelectXPathExpression = xmlSelectXPathField?.SelectSingleNodeAndCacheExpression("@xpath")?.Value ?? string.Empty;

                    if (string.IsNullOrEmpty(strSelectXPathXml) || string.IsNullOrEmpty(strSelectXPathExpression))
                    {
                        await Program.ShowScrollableMessageBoxAsync(this,
                            await LanguageManager.GetStringAsync("Message_Improvement_SelectXPath_Configuration", token: default).ConfigureAwait(false) ?? "SelectXPath field requires xml and xpath attributes.",
                            await LanguageManager.GetStringAsync("MessageTitle_Improvement_SelectXPath_Configuration", token: default).ConfigureAwait(false) ?? "Configuration Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                        break;
                    }

                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                               out List<ListItem> lstItems))
                    {
                        // Load items from the specified XML file using the xpath expression
                        foreach (XPathNavigator xmlItem in (await _objCharacter.LoadDataXPathAsync(strSelectXPathXml).ConfigureAwait(false))
                                                                  .SelectAndCacheExpression(strSelectXPathExpression))
                        {
                            string strName = xmlItem.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstItems.Add(new ListItem(
                                    strName,
                                    xmlItem.SelectSingleNodeAndCacheExpression("translate")?.Value
                                    ?? strName));
                        }

                        string strDescription = await LanguageManager.GetStringAsync("Title_SelectItem").ConfigureAwait(false);
                        using (ThreadSafeForm<SelectItem> selectItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem()).ConfigureAwait(false))
                        {
                            await selectItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription).ConfigureAwait(false);
                            selectItem.MyForm.SetDropdownItemsMode(lstItems);

                            if (await selectItem.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.OK)
                            {
                                string strSelect = await selectItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName).ConfigureAwait(false);
                                await txtSelect.DoThreadSafeAsync(x => x.Text = strSelect).ConfigureAwait(false);
                                string strTranslateSelection = await TranslateField(_strSelect, strSelect).ConfigureAwait(false);
                                await txtTranslateSelection.DoThreadSafeAsync(x => x.Text = strTranslateSelection).ConfigureAwait(false);
                            }
                        }
                    }

                    break;
                }
            }
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Accept the values on the Form and create the required XML data.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            // Make sure a value has been selected if necessary.
            if (await txtTranslateSelection.DoThreadSafeFuncAsync(x => x.Visible, token: token).ConfigureAwait(false) && string.IsNullOrEmpty(await txtSelect.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false)))
            {
                await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_SelectItem", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SelectItem", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                return;
            }

            string strName = await txtName.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            // Make sure a value has been provided for the name.
            if (string.IsNullOrEmpty(strName))
            {
                await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_ImprovementName", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_ImprovementName", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                await txtName.DoThreadSafeAsync(x => x.Focus(), token: token).ConfigureAwait(false);
                return;
            }

            string strSelectedType = await cboImprovemetType
                                           .DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token)
                                           .ConfigureAwait(false);

            string strGuid;
            using (new FetchSafelyFromSafeObjectPool<XmlDocument>(Utils.XmlDocumentPool, out XmlDocument objBonusXml))
            {
                using (RecyclableMemoryStream objStream = new RecyclableMemoryStream(Utils.MemoryStreamManager))
                {
                    using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                    {
                        // Build the XML for the Improvement.
                        XPathNavigator objFetchNode = _objImprovementsDocumentImprovementsNode.TryGetNodeByNameOrId("improvement", strSelectedType, blnIdIsGuid: false);
                        if (objFetchNode == null)
                            return;
                        string strInternal = objFetchNode.SelectSingleNodeAndCacheExpression("internal", token)?.Value;
                        if (string.IsNullOrEmpty(strInternal))
                            return;
                        await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);
                        // <bonus>
                        XmlElementWriteHelper objBonusNode = await objWriter.StartElementAsync("bonus", token: token).ConfigureAwait(false);
                        try
                        {
                            // <whatever element>
                            XmlElementWriteHelper objInternalNode = await objWriter.StartElementAsync(strInternal, token: token).ConfigureAwait(false);
                            try
                            {
                                // Retrieve the XML data from the document and replace the values as necessary.
                                foreach (XPathNavigator xmlAttribute in objFetchNode.SelectAndCacheExpression("xml/@*", token))
                                {
                                    await objWriter.WriteAttributeStringAsync(
                                        xmlAttribute.LocalName, xmlAttribute.Value, token: token).ConfigureAwait(false);
                                }

                                // ReSharper disable once PossibleNullReferenceException
                                string strXml
                                    = await objFetchNode.SelectSingleNodeAndCacheExpression("xml", token).Value
                                        .CheapReplaceAsync("{val}",
                                                           () => nudVal.DoThreadSafeFuncAsync(
                                                               x => x.Value.ToString(
                                                                   GlobalSettings.InvariantCultureInfo), token: token),
                                                           token: token)
                                        .CheapReplaceAsync("{min}",
                                                           () => nudMin.DoThreadSafeFuncAsync(
                                                               x => x.Value.ToString(
                                                                   GlobalSettings.InvariantCultureInfo), token: token),
                                                           token: token)
                                        .CheapReplaceAsync("{max}",
                                                           () => nudMax.DoThreadSafeFuncAsync(
                                                               x => x.Value.ToString(
                                                                   GlobalSettings.InvariantCultureInfo), token: token),
                                                           token: token)
                                        .CheapReplaceAsync("{aug}",
                                                           () => nudAug.DoThreadSafeFuncAsync(
                                                               x => x.Value.ToString(
                                                                   GlobalSettings.InvariantCultureInfo), token: token),
                                                           token: token)
                                        .CheapReplaceAsync("{free}",
                                                           () =>
                                                               chkFree.DoThreadSafeFuncAsync(
                                                                   x => x.Checked.ToString(
                                                                       GlobalSettings
                                                                           .InvariantCultureInfo), token: token),
                                                           token: token)
                                        .CheapReplaceAsync("{select}",
                                                           () => txtSelect
                                                               .DoThreadSafeFuncAsync(
                                                                   x => x.Text, token: token), token: token)
                                        .CheapReplaceAsync(
                                            "{applytorating}",
                                            async () =>
                                                await chkApplyToRating
                                                      .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                      .ConfigureAwait(false)
                                                    ? "<applytorating>True</applytorating>"
                                                    : string.Empty, token: token).ConfigureAwait(false);
                                await objWriter.WriteRawAsync(strXml).ConfigureAwait(false);

                                // Write the rest of the document.
                            }
                            finally
                            {
                                // </whatever element>
                                await objInternalNode.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            // </bonus>
                            await objBonusNode.DisposeAsync().ConfigureAwait(false);
                        }
                        await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                        await objWriter.FlushAsync().ConfigureAwait(false);
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
                strGuid = Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo);
                await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Custom, strGuid, objNode, strFriendlyName: strName, token: token).ConfigureAwait(false);
            }

            // If an Improvement was passed in, remove it from the character.
            string strNotes = string.Empty;
            int intOrder = 0;
            if (EditImprovementObject != null)
            {
                // Copy the notes over to the new item.
                strNotes = await EditImprovementObject.GetNotesAsync(token).ConfigureAwait(false);
                intOrder = EditImprovementObject.SortOrder;
                await ImprovementManager.RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Custom, EditImprovementObject.SourceName, token).ConfigureAwait(false);
            }

            // Find the newly-created Improvement and attach its custom name.
            Improvement objImprovement = await _objCharacter.Improvements.FirstOrDefaultAsync(imp => imp.SourceName == strGuid, token).ConfigureAwait(false);
            if (objImprovement != null)
            {
                objImprovement.CustomName = strName;
                objImprovement.CustomId = strSelectedType;
                objImprovement.Custom = true;
                await objImprovement.SetNotesAsync(strNotes, token).ConfigureAwait(false);
                objImprovement.SortOrder = intOrder;
                objImprovement.CustomGroup = _strCustomGroup;
                NewImprovement = objImprovement;
            }
            else { Utils.BreakIfDebug(); }

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a current language translation given an improvement name.
        /// </summary>
        /// <param name="strImprovementType"> The selector for the target translation. Often just _strSelect. </param>
        /// <param name="strToTranslate"> The string which to translate. Usually name. Guid in the case of adept powers.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        private async Task<string> TranslateField(string strImprovementType, string strToTranslate, CancellationToken token = default)
        {
            XPathNavigator objXmlNode;
            switch (strImprovementType)
            {
                case "SelectAttribute":
                case "SelectPhysicalAttribute":
                case "SelectMentalAttribute":
                case "SelectSpecialAttribute":
                    return strToTranslate == "MAGAdept"
                    ? await LanguageManager.GetStringAsync("String_AttributeMAGShort", token: token).ConfigureAwait(false) + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + "(" + await LanguageManager.GetStringAsync("String_DescAdept", token: token).ConfigureAwait(false) + ")"
                    : await LanguageManager.GetStringAsync("String_Attribute" + strToTranslate + "Short", token: token).ConfigureAwait(false);

                case "SelectSkill":
                    if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strToTranslate, token).ConfigureAwait(false))
                    {
                        string[] astrToTranslateParts = strToTranslate.SplitFixedSizePooledArray('(', 2, StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            astrToTranslateParts[0] = astrToTranslateParts[0].Trim();
                            astrToTranslateParts[1] = astrToTranslateParts[1].Substring(0, astrToTranslateParts[1].Length - 1);

                            objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/skills/skill", astrToTranslateParts[0]);
                            string strFirstPartTranslated = objXmlNode != null
                                ? objXmlNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                  ?? objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                  ?? astrToTranslateParts[0]
                                : astrToTranslateParts[0];

                            return strFirstPartTranslated + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + "(" + await _objCharacter.TranslateExtraAsync(astrToTranslateParts[1], token: token).ConfigureAwait(false) + ")";
                        }
                        finally
                        {
                            ArrayPool<string>.Shared.Return(astrToTranslateParts);
                        }
                    }

                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/skills/skill", strToTranslate);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? strToTranslate;

                case "SelectKnowSkill":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/knowledgeskills/skill", strToTranslate);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? strToTranslate;

                case "SelectSkillCategory":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + "]", token: token);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("@translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression(".", token: token)?.Value ?? strToTranslate;

                case "SelectSkillGroup":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer/skillgroups/name[. = " + strToTranslate.CleanXPath() + "]", token: token);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("@translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression(".", token: token)?.Value ?? strToTranslate;

                case "SelectWeaponCategory":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("weapons.xml", token: token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + "]", token: token);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("@translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression(".", token: token)?.Value ?? strToTranslate;

                case "SelectSpellCategory":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("spells.xml", token: token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + strToTranslate.CleanXPath() + "]", token: token);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("@translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression(".", token: token)?.Value ?? strToTranslate;

                case "SelectAdeptPower":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("powers.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/powers/power", strToTranslate);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? strToTranslate;

                case "SelectMetamagic":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("metamagic.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/metamagics/metamagic", strToTranslate);
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? strToTranslate;

                case "SelectEcho":
                    objXmlNode = (await _objCharacter.LoadDataXPathAsync("echoes.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/echoes/echo", strToTranslate.CleanXPath());
                    if (objXmlNode == null)
                        return strToTranslate;
                    return objXmlNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? strToTranslate;

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
