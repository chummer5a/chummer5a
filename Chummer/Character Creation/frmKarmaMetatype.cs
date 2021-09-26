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
            _xmlBaseMetatypeDataNode = _objCharacter.LoadDataXPath(strXmlFile).SelectSingleNode("/chummer");
            _xmlSkillsDocumentKnowledgeSkillsNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/knowledgeskills");
            _xmlQualityDocumentQualitiesNode = _objCharacter.LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNode("/chummer");
            _xmlCritterPowerDocumentPowersNode = _objCharacter.LoadData("critterpowers.xml").SelectSingleNode("/chummer/powers");
        }

        private void frmMetatype_Load(object sender, EventArgs e)
        {
            // Populate the Metatype Category list.
            List<ListItem> lstCategories = new List<ListItem>(3);

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
                        if (null != xmlMetatypesNode.SelectSingleNode(string.Format(GlobalSettings.InvariantCultureInfo,
                            "metatype[category = {0} and ({1})]",
                            strInnerText.CleanXPath(), _objCharacter.Settings.BookXPath())))
                        {
                            lstCategories.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value
                                                                         ?? strInnerText));
                        }
                    }
                }
            }

            lstCategories.Sort(CompareListItems.CompareNames);
            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(lstCategories);
            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            cboCategory.SelectedValue = _objCharacter.MetatypeCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            chkPossessionBased.SetToolTip(LanguageManager.GetString("Tip_Metatype_PossessionTradition"));

            List<ListItem> lstMethods = new List<ListItem>(2)
            {
                new ListItem("Possession", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText ?? "Possession"),
                new ListItem("Inhabitation", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")?.InnerText ?? "Inhabitation")
            };

            lstMethods.Sort(CompareListItems.CompareNames);

            foreach (CritterPower objPower in _objCharacter.CritterPowers)
            {
                string strPowerName = objPower.Name;
                if (lstMethods.Any(x => strPowerName.Equals(x.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    _strCurrentPossessionMethod = strPowerName;
                    break;
                }
            }

            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.PopulateWithListItems(lstMethods);
            cboPossessionMethod.EndUpdate();

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

                    int intForce = nudForce.Visible ? nudForce.ValueAsInt : 0;

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
                                (xmlRestrictionNode.SelectSingleNode("//metatype") != null || xmlRestrictionNode.SelectSingleNode("//metavariant") != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode = objQuality.GetNode()?["forbidden"];
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNode("//metatype") != null || xmlRestrictionNode.SelectSingleNode("//metavariant") != null))
                                {
                                    lstQualitiesToCheck.Add(objQuality);
                                }
                            }
                        }
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
                if (objXmlMetatype.SelectSingleNode("forcecreature") == null)
                {
                    lblBOD.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblAGI.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblREA.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblSTR.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblCHA.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblINT.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblLOG.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblWIL.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                }
                else
                {
                    lblBOD.Text = objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblAGI.Text = objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblREA.Text = objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblSTR.Text = objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblCHA.Text = objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblINT.Text = objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblLOG.Text = objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblWIL.Text = objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                }

                // ReSharper disable once IdentifierTypo
                StringBuilder sbdQualities = new StringBuilder();
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetavariant.Select("qualities/*/quality"))
                {
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        sbdQualities.Append(_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath() + "]/translate")?.Value ?? objXmlQuality.Value);

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(_objCharacter.TranslateExtra(strSelect));
                            sbdQualities.Append(')');
                        }
                    }
                    else
                    {
                        sbdQualities.Append(objXmlQuality.Value);
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(strSelect);
                            sbdQualities.Append(')');
                        }
                    }
                    sbdQualities.Append(Environment.NewLine);
                }

                lblQualities.Text = sbdQualities.Length == 0 ? LanguageManager.GetString("String_None") : sbdQualities.ToString();

                lblKarma.Text = objXmlMetavariant.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetavariant.SelectSingleNode("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetavariant.SelectSingleNode("altpage")?.Value ?? objXmlMetavariant.SelectSingleNode("page")?.Value;
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
                if (objXmlMetatype.SelectSingleNode("forcecreature") == null)
                {
                    lblBOD.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblAGI.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblREA.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblSTR.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblCHA.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblINT.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblLOG.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                    lblWIL.Text = string.Format(GlobalSettings.CultureInfo, "{0}/{1}{2}({3})", objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo),
                        objXmlMetatype.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalSettings.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalSettings.CultureInfo));
                }
                else
                {
                    lblBOD.Text = objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblAGI.Text = objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblREA.Text = objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblSTR.Text = objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblCHA.Text = objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblINT.Text = objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblLOG.Text = objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    lblWIL.Text = objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                }

                // ReSharper disable once IdentifierTypo
                StringBuilder sbdQualities = new StringBuilder();
                // Build a list of the Metatype's Positive Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetatype.Select("qualities/*/quality"))
                {
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        sbdQualities.Append(_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath() + "]/translate")?.Value ?? objXmlQuality.Value);

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(_objCharacter.TranslateExtra(strSelect));
                            sbdQualities.Append(')');
                        }
                    }
                    else
                    {
                        sbdQualities.Append(objXmlQuality.Value);
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(strSelect);
                            sbdQualities.Append(')');
                        }
                    }
                    sbdQualities.Append(Environment.NewLine);
                }

                lblQualities.Text = sbdQualities.Length == 0 ? LanguageManager.GetString("String_None") : sbdQualities.ToString();

                lblKarma.Text = objXmlMetatype.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalSettings.CultureInfo);

                string strSource = objXmlMetatype.SelectSingleNode("source")?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = objXmlMetatype.SelectSingleNode("altpage")?.Value ?? objXmlMetatype.SelectSingleNode("page")?.Value;
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
                List<ListItem> lstMetavariants = new List<ListItem>(5)
                {
                    new ListItem(Guid.Empty, LanguageManager.GetString("String_None"))
                };
                foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select("metavariants/metavariant[" + _objCharacter.Settings.BookXPath() + "]"))
                {
                    string strId = objXmlMetavariant.SelectSingleNode("id")?.Value;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        lstMetavariants.Add(new ListItem(strId,
                            objXmlMetavariant.SelectSingleNode("translate")?.Value
                            ?? objXmlMetavariant.SelectSingleNode("name")?.Value
                            ?? LanguageManager.GetString("String_Unknown")));
                    }
                }

                // Retrieve the list of Metavariants for the selected Metatype.

                bool blnOldLoading = _blnLoading;
                string strOldSelectedValue = cboMetavariant.SelectedValue?.ToString() ?? _objCharacter?.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo);
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

                // If the Metatype has Force enabled, show the Force NUD.
                string strEssMax = objXmlMetatype.SelectSingleNode("essmax")?.Value ?? string.Empty;
                int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                if (objXmlMetatype.SelectSingleNode("forcecreature") != null || intPos != -1)
                {
                    if (intPos != -1)
                    {
                        if (intPos > 0)
                        {
                            intPos -= 1;
                            lblForceLabel.Text = strEssMax.Substring(intPos, 3).Replace("D6", LanguageManager.GetString("String_D6"));
                            nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1), GlobalSettings.InvariantCultureInfo) * 6;
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
                        lblForceLabel.Text = LanguageManager.GetString(objXmlMetatype.SelectSingleNode("bodmax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("agimax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("reamax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("strmax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("magmin")?.Value.Contains('F') != true
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
                lblMetavariantLabel.Visible = true;
                cboMetavariant.Visible = true;
            }
            else
            {
                lblMetavariantLabel.Visible = false;
                cboMetavariant.Visible = false;
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>(5)
                {
                    new ListItem(Guid.Empty, LanguageManager.GetString("String_None"))
                };

                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                cboMetavariant.BeginUpdate();
                cboMetavariant.PopulateWithListItems(lstMetavariants);
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
                foreach (XPathNavigator xmlMetatype in _xmlBaseMetatypeDataNode.Select(
                    string.Format(GlobalSettings.InvariantCultureInfo, "metatypes/metatype[({0}) and category = {1}]",
                        _objCharacter.Settings.BookXPath(), strSelectedCategory.CleanXPath())))
                {
                    string strId = xmlMetatype.SelectSingleNode("id")?.Value;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        lstMetatypeItems.Add(new ListItem(strId,
                            xmlMetatype.SelectSingleNode("translate")?.Value
                            ?? xmlMetatype.SelectSingleNode("name")?.Value
                            ?? LanguageManager.GetString("String_Unknown")));
                    }
                }

                lstMetatypeItems.Sort(CompareListItems.CompareNames);

                bool blnOldLoading = _blnLoading;
                string strOldSelected = lstMetatypes.SelectedValue?.ToString() ?? _objCharacter?.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo);
                if (strOldSelected == Guid.Empty.ToString("D", GlobalSettings.InvariantCultureInfo))
                    strOldSelected = _objCharacter.GetNode(true)?.SelectSingleNode("id")?.Value ?? string.Empty;
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

                if (strSelectedCategory.EndsWith("Spirits", StringComparison.Ordinal))
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
    }
}
