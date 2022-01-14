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
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;

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
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _xmlMetatypeDocumentMetatypesNode = _objCharacter.LoadData(strXmlFile).SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = _objCharacter.LoadDataXPath(strXmlFile).SelectSingleNodeAndCacheExpression("/chummer");
            _xmlSkillsDocumentKnowledgeSkillsNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/knowledgeskills");
            _xmlQualityDocumentQualitiesNode = _objCharacter.LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlCritterPowerDocumentPowersNode = _objCharacter.LoadData("critterpowers.xml").SelectSingleNode("/chummer/powers");
        }

        private void frmMetatype_Load(object sender, EventArgs e)
        {
            // Populate the Metatype Category list.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategories))
            {
                // Create a list of Categories.
                XPathNavigator xmlMetatypesNode
                    = _xmlBaseMetatypeDataNode.SelectSingleNodeAndCacheExpression("metatypes");
                if (xmlMetatypesNode != null)
                {
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setAlreadyProcessed))
                    {
                        foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.SelectAndCacheExpression(
                                     "categories/category"))
                        {
                            string strInnerText = objXmlCategory.Value;
                            if (setAlreadyProcessed.Contains(strInnerText))
                                continue;
                            setAlreadyProcessed.Add(strInnerText);
                            if (xmlMetatypesNode.SelectSingleNode("metatype[category = " + strInnerText.CleanXPath()
                                                                  + " and (" + _objCharacter.Settings.BookXPath()
                                                                  + ")]")
                                != null)
                            {
                                lstCategories.Add(new ListItem(strInnerText,
                                                               objXmlCategory
                                                                   .SelectSingleNodeAndCacheExpression("@translate")
                                                                   ?.Value
                                                               ?? strInnerText));
                            }
                        }
                    }
                }

                lstCategories.Sort(CompareListItems.CompareNames);
                lstCategories.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));

                cboCategory.BeginUpdate();
                cboCategory.PopulateWithListItems(lstCategories);
                // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
                cboCategory.SelectedValue = _objCharacter.MetatypeCategory;
                if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                    cboCategory.SelectedIndex = 0;

                cboCategory.EndUpdate();
            }

            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            chkPossessionBased.SetToolTip(LanguageManager.GetString("Tip_Metatype_PossessionTradition"));

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMethods))
            {
                lstMethods.Add(new ListItem("Possession",
                                            _xmlCritterPowerDocumentPowersNode
                                                ?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText
                                            ?? "Possession"));
                lstMethods.Add(new ListItem("Inhabitation",
                                            _xmlCritterPowerDocumentPowersNode
                                                ?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")?.InnerText
                                            ?? "Inhabitation"));
                lstMethods.Sort(CompareListItems.CompareNames);

                _strCurrentPossessionMethod = _objCharacter.CritterPowers.Select(x => x.Name)
                                                           .FirstOrDefault(
                                                               y => lstMethods.Any(
                                                                   x => y.Equals(
                                                                       x.Value.ToString(),
                                                                       StringComparison.OrdinalIgnoreCase)));

                cboPossessionMethod.BeginUpdate();
                cboPossessionMethod.PopulateWithListItems(lstMethods);
                cboPossessionMethod.EndUpdate();
            }

            PopulateMetatypes();
            PopulateMetavariants();
            RefreshSelectedMetavariant();

            _blnLoading = false;
        }

        #endregion Form Events

        #region Control Events

        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            PopulateMetavariants();
            ResumeLayout();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SuspendLayout();
            RefreshSelectedMetavariant();
            ResumeLayout();
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
            SuspendLayout();
            PopulateMetatypes();
            ResumeLayout();
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }

        #endregion Control Events

        #region Custom Methods

        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private void MetatypeSelected()
        {
            using (new CursorWait(this))
            {
                string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strSelectedMetatype))
                {
                    string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();
                    string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString() ?? Guid.Empty.ToString();

                    XmlNode objXmlMetatype = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode("metatype[id = " + strSelectedMetatype.CleanXPath() + "]");
                    if (objXmlMetatype == null)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Remove all priority-given qualities (relevant when switching from Priority/Sum-to-Ten to Karma)
                    _objCharacter.Qualities.RemoveAll(x => x.OriginSource == QualitySource.Heritage);

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == Guid.Empty.ToString())
                        strSelectedMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"Human\"]/id")?.InnerText ?? "None";
                    if (_objCharacter.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetatype
                        || _objCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetavariant)
                    {
                        // Remove qualities that require the old metatype
                        List<Quality> lstQualitiesToCheck = new List<Quality>(_objCharacter.Qualities.Count);
                        foreach (Quality objQuality in _objCharacter.Qualities)
                        {
                            if (objQuality.OriginSource == QualitySource.Improvement
                                || objQuality.OriginSource == QualitySource.Metatype
                                || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                || objQuality.OriginSource == QualitySource.MetatypeRemovedAtChargen)
                                continue;
                            XmlNode xmlRestrictionNode = objQuality.GetNode()?["required"];
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNode(".//metatype") != null || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode = objQuality.GetNode()?["forbidden"];
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNode(".//metatype") != null || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                                {
                                    lstQualitiesToCheck.Add(objQuality);
                                }
                            }
                        }
                        int intForce = nudForce.Visible ? nudForce.ValueAsInt : 0;
                        _objCharacter.Create(strSelectedMetatypeCategory, strSelectedMetatype, strSelectedMetavariant,
                            objXmlMetatype, intForce, _xmlQualityDocumentQualitiesNode,
                            _xmlCritterPowerDocumentPowersNode, _xmlSkillsDocumentKnowledgeSkillsNode, chkPossessionBased.Checked ? cboPossessionMethod.SelectedValue?.ToString() : string.Empty);
                        foreach (Quality objQuality in lstQualitiesToCheck)
                        {
                            if (objQuality.GetNode()?.CreateNavigator().RequirementsMet(_objCharacter) == false)
                                _objCharacter.Qualities.Remove(objQuality);
                        }
                    }

                    // Flip all attribute, skill, and skill group points to karma levels (relevant when switching from Priority/Sum-to-Ten to Karma)
                    foreach (CharacterAttrib objAttrib in _objCharacter.AttributeSection.Attributes)
                    {
                        // This ordering makes sure data bindings to numeric up-downs with maxima don't get broken
                        int intBase = objAttrib.Base;
                        objAttrib.Base = 0;
                        objAttrib.Karma += intBase;
                    }
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        // This ordering makes sure data bindings to numeric up-downs with maxima don't get broken
                        int intBase = objSkill.BasePoints;
                        objSkill.BasePoints = 0;
                        objSkill.KarmaPoints += intBase;
                    }
                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                    {
                        // This ordering makes sure data bindings to numeric up-downs with maxima don't get broken
                        int intBase = objGroup.BasePoints;
                        objGroup.BasePoints = 0;
                        objGroup.KarmaPoints += intBase;
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RefreshSelectedMetavariant()
        {
            string strSpace = LanguageManager.GetString("String_Space");
            XPathNavigator objXmlMetatype = null;
            XPathNavigator objXmlMetavariant = null;
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[id = " + strSelectedMetatype.CleanXPath() + "]");
                string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
                if (objXmlMetatype != null && !string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                {
                    objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[id = " + strSelectedMetavariant.CleanXPath() + "]");
                }
            }

            if (objXmlMetavariant != null)
            {
                cmdOK.Enabled = true;
                if (objXmlMetatype.SelectSingleNodeAndCacheExpression("forcecreature") == null)
                {
                    lblBOD.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("bodmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblAGI.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("agimax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("agiaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblREA.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("reamax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("reaaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblSTR.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("strmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("straug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblCHA.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("chamax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("chaaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblINT.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("intmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("intaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblLOG.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("logmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("logaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblWIL.Text = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetavariant
                                          .SelectSingleNodeAndCacheExpression("wilmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                }
                else
                {
                    lblBOD.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblAGI.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblREA.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblSTR.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblCHA.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblINT.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblLOG.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblWIL.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmin")?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin")?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                }

                // ReSharper disable once IdentifierTypo
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdQualities))
                {
                    // Build a list of the Metavariant's Qualities.
                    foreach (XPathNavigator objXmlQuality in objXmlMetavariant.SelectAndCacheExpression(
                                 "qualities/*/quality"))
                    {
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            sbdQualities.Append(
                                _xmlBaseQualityDataNode
                                    .SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath()
                                                      + "]/translate")?.Value ?? objXmlQuality.Value);

                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(')
                                            .Append(_objCharacter.TranslateExtra(strSelect)).Append(')');
                            }
                        }
                        else
                        {
                            sbdQualities.Append(objXmlQuality.Value);
                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(').Append(strSelect).Append(')');
                            }
                        }

                        sbdQualities.Append(Environment.NewLine);
                    }

                    lblQualities.Text = sbdQualities.Length == 0
                        ? LanguageManager.GetString("String_None")
                        : sbdQualities.ToString();
                }

                lblKarma.Text = objXmlMetavariant.SelectSingleNodeAndCacheExpression("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetavariant.SelectSingleNodeAndCacheExpression("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetavariant.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetavariant.SelectSingleNodeAndCacheExpression("page")?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                        lblSource.Text = objSource.ToString();
                        lblSource.SetToolTip(objSource.LanguageBookTooltip);
                    }
                    else
                    {
                        string strUnknown = LanguageManager.GetString("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                }
                else
                {
                    string strUnknown = LanguageManager.GetString("String_Unknown");
                    lblSource.Text = strUnknown;
                    lblSource.SetToolTip(strUnknown);
                }
            }
            else if (objXmlMetatype != null)
            {
                cmdOK.Enabled = true;
                if (objXmlMetatype.SelectSingleNodeAndCacheExpression("forcecreature") == null)
                {
                    lblBOD.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("bodmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("bodaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblAGI.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("agimax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("agiaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblREA.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("reamax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("reaaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblSTR.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("strmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("straug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblCHA.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("chamax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("chaaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblINT.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("intmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("intaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblLOG.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("logmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("logaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    lblWIL.Text = (objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin")?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + (objXmlMetatype
                                          .SelectSingleNodeAndCacheExpression("wilmax")?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("wilaug")?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                }
                else
                {
                    lblBOD.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblAGI.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblREA.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblSTR.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblCHA.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblINT.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblLOG.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblWIL.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                }

                // ReSharper disable once IdentifierTypo
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdQualities))
                {
                    // Build a list of the Metatype's Positive Qualities.
                    foreach (XPathNavigator objXmlQuality in objXmlMetatype.SelectAndCacheExpression(
                                 "qualities/*/quality"))
                    {
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            sbdQualities.Append(
                                _xmlBaseQualityDataNode
                                    .SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath()
                                                      + "]/translate")?.Value ?? objXmlQuality.Value);

                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(')
                                            .Append(_objCharacter.TranslateExtra(strSelect))
                                            .Append(')');
                            }
                        }
                        else
                        {
                            sbdQualities.Append(objXmlQuality.Value);
                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(').Append(strSelect).Append(')');
                            }
                        }

                        sbdQualities.Append(Environment.NewLine);
                    }

                    lblQualities.Text = sbdQualities.Length == 0
                        ? LanguageManager.GetString("String_None")
                        : sbdQualities.ToString();
                }

                lblKarma.Text = objXmlMetatype.SelectSingleNodeAndCacheExpression("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetatype.SelectSingleNodeAndCacheExpression("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetatype.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("page")?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                        lblSource.Text = objSource.ToString();
                        lblSource.SetToolTip(objSource.LanguageBookTooltip);
                    }
                    else
                    {
                        string strUnknown = LanguageManager.GetString("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                }
                else
                {
                    string strUnknown = LanguageManager.GetString("String_Unknown");
                    lblSource.Text = strUnknown;
                    lblSource.SetToolTip(strUnknown);
                }
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

                lblQualities.Text = string.Empty;

                lblKarma.Text = string.Empty;
                lblSource.Text = string.Empty;

                cmdOK.Enabled = false;
            }

            if (objXmlMetatype.SelectSingleNode("category").InnerXml.EndsWith("Spirits", StringComparison.Ordinal))
            {
                if (!chkPossessionBased.Visible && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                {
                    chkPossessionBased.Checked = true;
                    cboPossessionMethod.SelectedValue = _strCurrentPossessionMethod;
                }
                chkPossessionBased.Visible = true;
                cboPossessionMethod.Visible = true;
            }
            else
            {
                chkPossessionBased.Visible = false;
                chkPossessionBased.Checked = false;
                cboPossessionMethod.Visible = false;
            }

            // If the Metatype has Force enabled, show the Force NUD.
            string strEssMax = objXmlMetatype.SelectSingleNodeAndCacheExpression("essmax")?.Value ?? string.Empty;
            int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
            if (objXmlMetatype.SelectSingleNodeAndCacheExpression("forcecreature") != null || intPos != -1)
            {
                if (intPos != -1)
                {
                    if (intPos > 0)
                    {
                        --intPos;
                        lblForceLabel.Text = strEssMax.Substring(intPos, 3).Replace("D6", LanguageManager.GetString("String_D6"));
                        nudForce.Maximum = Convert.ToInt32(strEssMax[intPos], GlobalSettings.InvariantCultureInfo) * 6;
                    }
                    else
                    {
                        lblForceLabel.Text = 1.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_D6");
                        nudForce.Maximum = 6;
                    }
                }
                else
                {
                    // TODO: Unhardcode whether Force is called "Force" or "Level"
                    lblForceLabel.Text = LanguageManager.GetString(objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmax")?.Value == "0" &&
                                                                   objXmlMetatype.SelectSingleNodeAndCacheExpression("agimax")?.Value == "0" &&
                                                                   objXmlMetatype.SelectSingleNodeAndCacheExpression("reamax")?.Value == "0" &&
                                                                   objXmlMetatype.SelectSingleNodeAndCacheExpression("strmax")?.Value == "0" &&
                                                                   objXmlMetatype.SelectSingleNodeAndCacheExpression("magmin")?.Value.Contains('F') != true
                        ? "String_Level"
                        : "String_Force");
                    nudForce.Maximum = 100;
                }
                lblForceLabel.Visible = true;
                nudForce.Visible = true;
            }
            else
            {
                lblForceLabel.Visible = false;
                nudForce.Visible = false;
            }

            lblBODLabel.Visible = !string.IsNullOrEmpty(lblBOD.Text);
            lblAGILabel.Visible = !string.IsNullOrEmpty(lblAGI.Text);
            lblREALabel.Visible = !string.IsNullOrEmpty(lblREA.Text);
            lblSTRLabel.Visible = !string.IsNullOrEmpty(lblSTR.Text);
            lblCHALabel.Visible = !string.IsNullOrEmpty(lblCHA.Text);
            lblINTLabel.Visible = !string.IsNullOrEmpty(lblINT.Text);
            lblLOGLabel.Visible = !string.IsNullOrEmpty(lblLOG.Text);
            lblWILLabel.Visible = !string.IsNullOrEmpty(lblWIL.Text);
            lblQualitiesLabel.Visible = !string.IsNullOrEmpty(lblQualities.Text);
            lblKarmaLabel.Visible = !string.IsNullOrEmpty(lblKarma.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            XPathNavigator objXmlMetatype = null;
            if (!string.IsNullOrEmpty(strSelectedMetatype))
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[id = " + strSelectedMetatype.CleanXPath() + "]");
            // Don't attempt to do anything if nothing is selected.
            if (objXmlMetatype != null)
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetavariants))
                {
                    lstMetavariants.Add(new ListItem(Guid.Empty, LanguageManager.GetString("String_None")));
                    foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select(
                                 "metavariants/metavariant[" + _objCharacter.Settings.BookXPath() + "]"))
                    {
                        string strId = objXmlMetavariant.SelectSingleNodeAndCacheExpression("id")?.Value;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            lstMetavariants.Add(new ListItem(strId,
                                                             objXmlMetavariant
                                                                 .SelectSingleNodeAndCacheExpression("translate")?.Value
                                                             ?? objXmlMetavariant
                                                                .SelectSingleNodeAndCacheExpression("name")?.Value
                                                             ?? LanguageManager.GetString("String_Unknown")));
                        }
                    }

                    // Retrieve the list of Metavariants for the selected Metatype.

                    bool blnOldLoading = _blnLoading;
                    string strOldSelectedValue = cboMetavariant.SelectedValue?.ToString()
                                                 ?? _objCharacter?.MetavariantGuid.ToString(
                                                     "D", GlobalSettings.InvariantCultureInfo);
                    _blnLoading = true;
                    cboMetavariant.BeginUpdate();
                    cboMetavariant.PopulateWithListItems(lstMetavariants);
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
                }

                lblMetavariantLabel.Visible = true;
                cboMetavariant.Visible = true;
            }
            else
            {
                lblMetavariantLabel.Visible = false;
                cboMetavariant.Visible = false;
                // Clear the Metavariant list if nothing is currently selected.
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                cboMetavariant.BeginUpdate();
                cboMetavariant.PopulateWithListItems(new ListItem(Guid.Empty, LanguageManager.GetString("String_None")).Yield());
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
            string strSearchText       = txtSearch.Text;
            string strFilter = string.Empty;
            if (!string.IsNullOrEmpty(strSelectedCategory) || !string.IsNullOrEmpty(strSearchText))
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstMetatypeItems))
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                        if (!string.IsNullOrEmpty(strSelectedCategory) && strSelectedCategory != "Show All"
                                                                       && (GlobalSettings.SearchInCategoryOnly
                                                                           || strSearchText.Length == 0))
                            sbdFilter.Append(" and category = ").Append(strSelectedCategory.CleanXPath());
                        
                        if (!string.IsNullOrEmpty(txtSearch.Text))
                            sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearchText));
                        
                        if (sbdFilter.Length > 0)
                            strFilter = '[' + sbdFilter.ToString() + ']';
                    }

                    foreach (XPathNavigator xmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                 "metatypes/metatype" + strFilter))
                    {
                        string strId = xmlMetatype.SelectSingleNodeAndCacheExpression("id")?.Value;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            lstMetatypeItems.Add(new ListItem(strId,
                                                              xmlMetatype
                                                                  .SelectSingleNodeAndCacheExpression("translate")
                                                                  ?.Value
                                                              ?? xmlMetatype.SelectSingleNodeAndCacheExpression("name")
                                                                            ?.Value
                                                              ?? LanguageManager.GetString("String_Unknown")));
                        }
                    }

                    lstMetatypeItems.Sort(CompareListItems.CompareNames);

                    bool blnOldLoading = _blnLoading;
                    string strOldSelected = lstMetatypes.SelectedValue?.ToString()
                                            ?? _objCharacter?.MetatypeGuid.ToString(
                                                "D", GlobalSettings.InvariantCultureInfo);
                    if (strOldSelected == Guid.Empty.ToString("D", GlobalSettings.InvariantCultureInfo))
                        strOldSelected = _objCharacter.GetNode(true)?.SelectSingleNodeAndCacheExpression("id")?.Value
                                         ?? string.Empty;
                    _blnLoading = true;
                    lstMetatypes.BeginUpdate();
                    lstMetatypes.PopulateWithListItems(lstMetatypeItems);
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
                }
            }
            else
            {
                lstMetatypes.BeginUpdate();
                lstMetatypes.DataSource = null;
                lstMetatypes.EndUpdate();

                chkPossessionBased.Visible = false;
                chkPossessionBased.Checked = false;
                cboPossessionMethod.Visible = false;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Custom Methods

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            PopulateMetatypes();
        }
    }
}
