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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class SelectMetatypeKarma : Form
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

        public SelectMetatypeKarma(Character objCharacter, string strXmlFile = "metatypes.xml")
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

        private async void SelectMetatypeKarma_Load(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    // Populate the Metatype Category list.
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstCategories))
                    {
                        // Create a list of Categories.
                        XPathNavigator xmlMetatypesNode
                            = await _xmlBaseMetatypeDataNode.SelectSingleNodeAndCacheExpressionAsync("metatypes").ConfigureAwait(false);
                        if (xmlMetatypesNode != null)
                        {
                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string> setAlreadyProcessed))
                            {
                                foreach (XPathNavigator objXmlCategory in await _xmlBaseMetatypeDataNode
                                             .SelectAndCacheExpressionAsync(
                                                 "categories/category").ConfigureAwait(false))
                                {
                                    string strInnerText = objXmlCategory.Value;
                                    if (setAlreadyProcessed.Contains(strInnerText))
                                        continue;
                                    setAlreadyProcessed.Add(strInnerText);
                                    if (xmlMetatypesNode.SelectSingleNode(
                                            "metatype[category = " + strInnerText.CleanXPath()
                                                                   + " and (" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false)
                                                                   + ")]")
                                        != null)
                                    {
                                        lstCategories.Add(new ListItem(strInnerText,
                                                                       (await objXmlCategory
                                                                              .SelectSingleNodeAndCacheExpressionAsync(
                                                                                  "@translate").ConfigureAwait(false))
                                                                       ?.Value
                                                                       ?? strInnerText));
                                    }
                                }
                            }
                        }

                        lstCategories.Sort(CompareListItems.CompareNames);
                        lstCategories.Insert(
                            0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));

                        await cboCategory.PopulateWithListItemsAsync(lstCategories).ConfigureAwait(false);
                        // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
                        await cboCategory.DoThreadSafeAsync(x =>
                        {
                            x.SelectedValue = _objCharacter.MetatypeCategory;
                            if (x.SelectedIndex == -1 && lstCategories.Count > 0)
                                x.SelectedIndex = 0;
                        }).ConfigureAwait(false);
                    }

                    // Add Possession and Inhabitation to the list of Critter Tradition variations.
                    await chkPossessionBased.SetToolTipAsync(
                        await LanguageManager.GetStringAsync("Tip_Metatype_PossessionTradition").ConfigureAwait(false)).ConfigureAwait(false);

                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstMethods))
                    {
                        lstMethods.Add(new ListItem("Possession",
                                                    _xmlCritterPowerDocumentPowersNode
                                                        ?.SelectSingleNode("power[name = \"Possession\"]/translate")
                                                        ?.InnerText
                                                    ?? "Possession"));
                        lstMethods.Add(new ListItem("Inhabitation",
                                                    _xmlCritterPowerDocumentPowersNode
                                                        ?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")
                                                        ?.InnerText
                                                    ?? "Inhabitation"));
                        lstMethods.Sort(CompareListItems.CompareNames);

                        _strCurrentPossessionMethod = _objCharacter.CritterPowers.Select(x => x.Name)
                                                                   .FirstOrDefault(
                                                                       y => lstMethods.Any(
                                                                           x => y.Equals(
                                                                               x.Value.ToString(),
                                                                               StringComparison.OrdinalIgnoreCase)));

                        await cboPossessionMethod.PopulateWithListItemsAsync(lstMethods).ConfigureAwait(false);
                    }

                    await PopulateMetatypes().ConfigureAwait(false);
                    await PopulateMetavariants().ConfigureAwait(false);
                    await RefreshSelectedMetavariant().ConfigureAwait(false);

                    _blnLoading = false;
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Form Events

        #region Control Events

        private async void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    await ProcessMetatypeSelectedChanged().ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask ProcessMetatypeSelectedChanged(CancellationToken token = default)
        {
            if (_blnLoading)
                return;
            await PopulateMetavariants(token).ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await MetatypeSelected().ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    await ProcessMetavariantSelectedChanged().ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask ProcessMetavariantSelectedChanged(CancellationToken token = default)
        {
            if (_blnLoading)
                return;
            await RefreshSelectedMetavariant(token).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    await PopulateMetatypes().ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                bool blnTemp = await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                await cboPossessionMethod.DoThreadSafeAsync(x => x.Enabled = blnTemp).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Control Events

        #region Custom Methods

        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private async ValueTask MetatypeSelected(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedMetatypeCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? Guid.Empty.ToString();

                XmlNode objXmlMetatype
                    = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode(
                        "metatype[id = " + strSelectedMetatype.CleanXPath() + ']');
                if (objXmlMetatype == null)
                {
                    Program.ShowMessageBox(
                        this, await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    // Remove all priority-given qualities (relevant when switching from Priority/Sum-to-Ten to Karma)
                    for (int i = _objCharacter.Qualities.Count - 1; i >= 0; --i)
                    {
                        if (i >= _objCharacter.Qualities.Count)
                            continue;
                        Quality objQuality = _objCharacter.Qualities[i];
                        if (objQuality.OriginSource == QualitySource.Heritage)
                            objQuality.DeleteQuality();
                    }

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter"
                        && strSelectedMetavariant == Guid.Empty.ToString())
                        strSelectedMetavariant
                            = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"Human\"]/id")
                                            ?.InnerText
                              ?? "None";
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
                            XPathNavigator xmlRestrictionNode
                                = (await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false))?.SelectSingleNode("required");
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNode(".//metatype") != null
                                 || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode
                                    = (await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false))?.SelectSingleNode("forbidden");
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNode(".//metatype") != null
                                     || xmlRestrictionNode.SelectSingleNode(".//metavariant") != null))
                                {
                                    lstQualitiesToCheck.Add(objQuality);
                                }
                            }
                        }

                        int intForce = nudForce.Visible ? nudForce.ValueAsInt : 0;
                        _objCharacter.Create(strSelectedMetatypeCategory, strSelectedMetatype, strSelectedMetavariant,
                                             objXmlMetatype, intForce, _xmlQualityDocumentQualitiesNode,
                                             _xmlCritterPowerDocumentPowersNode, _xmlSkillsDocumentKnowledgeSkillsNode,
                                             chkPossessionBased.Checked
                                                 ? cboPossessionMethod.SelectedValue?.ToString()
                                                 : string.Empty, token);
                        foreach (Quality objQuality in lstQualitiesToCheck)
                        {
                            if ((await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false))?.RequirementsMet(_objCharacter)
                                == false)
                                objQuality.DeleteQuality();
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
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, token).ConfigureAwait(false);
            }
            else
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                                       await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async ValueTask RefreshSelectedMetavariant(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            XPathNavigator objXmlMetatype = null;
            XPathNavigator objXmlMetavariant = null;
            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[id = " + strSelectedMetatype.CleanXPath() + ']');
                string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                if (objXmlMetatype != null && !string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                {
                    objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[id = " + strSelectedMetavariant.CleanXPath() + ']');
                }
            }

            string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
            if (objXmlMetavariant != null)
            {
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                if (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("forcecreature", token: token).ConfigureAwait(false) == null)
                {
                    string strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("bodmin", token: token).ConfigureAwait(false))?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                             .SelectSingleNodeAndCacheExpressionAsync("bodmax", token: token).ConfigureAwait(false))?.Value
                                         ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                     + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("bodaug", token: token).ConfigureAwait(false))?.Value
                                        ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("agimin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("agimax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("agiaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblAGI.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("reamin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("reamax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("reaaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblREA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("strmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("strmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("straug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblSTR.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("chamin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("chamax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("chaaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblCHA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("intmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("intmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("intaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblINT.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("logmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("logmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("logaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblLOG.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("wilmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetavariant
                                      .SelectSingleNodeAndCacheExpressionAsync("wilmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("wilaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblWIL.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                }
                else
                {
                    string strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("bodmin", token: token).ConfigureAwait(false))?.Value
                                     ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodmin", token: token).ConfigureAwait(false))?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("agimin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agimin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblAGI.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("reamin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reamin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblREA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("strmin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("strmin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblSTR.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("chamin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chamin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblCHA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("intmin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intmin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblINT.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("logmin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logmin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblLOG.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("wilmin", token: token).ConfigureAwait(false))?.Value
                              ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilmin", token: token).ConfigureAwait(false))?.Value
                              ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblWIL.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                }

                // ReSharper disable once IdentifierTypo
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdQualities))
                {
                    // Build a list of the Metavariant's Qualities.
                    foreach (XPathNavigator objXmlQuality in await objXmlMetavariant.SelectAndCacheExpressionAsync(
                                 "qualities/*/quality", token: token).ConfigureAwait(false))
                    {
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            sbdQualities.Append(
                                _xmlBaseQualityDataNode
                                    .SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath()
                                                      + "]/translate")?.Value ?? objXmlQuality.Value);

                            string strSelect = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select", token: token).ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(')
                                            .Append(await _objCharacter.TranslateExtraAsync(strSelect, token: token).ConfigureAwait(false)).Append(')');
                            }
                        }
                        else
                        {
                            sbdQualities.Append(objXmlQuality.Value);
                            string strSelect = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select", token: token).ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(').Append(strSelect).Append(')');
                            }
                        }

                        sbdQualities.Append(Environment.NewLine);
                    }

                    await lblQualities.DoThreadSafeAsync(x => x.Text = sbdQualities.Length == 0
                                                             ? strNone
                                                             : sbdQualities.ToString(), token).ConfigureAwait(false);
                }

                string strKarma = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("karma", token: token).ConfigureAwait(false))?.Value;
                await lblKarma.DoThreadSafeAsync(x => x.Text = strKarma ?? 0.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                string strSource = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("source", token: token).ConfigureAwait(false))?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token).ConfigureAwait(false))?.Value ?? (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("page", token: token).ConfigureAwait(false))?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
                        await lblSource.DoThreadSafeAsync(x => x.Text = objSource.ToString(), token).ConfigureAwait(false);
                        await lblSource.SetToolTipAsync(objSource.LanguageBookTooltip, token).ConfigureAwait(false);
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                        await lblSource.SetToolTipAsync(strUnknown, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                    await lblSource.SetToolTipAsync(strUnknown, token).ConfigureAwait(false);
                }
            }
            else if (objXmlMetatype != null)
            {
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                if (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("forcecreature", token: token).ConfigureAwait(false) == null)
                {
                    string strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodmin", token: token).ConfigureAwait(false))?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                             .SelectSingleNodeAndCacheExpressionAsync("bodmax", token: token).ConfigureAwait(false))?.Value
                                         ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                                     + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodaug", token: token).ConfigureAwait(false))?.Value
                                        ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agimin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("agimax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agiaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblAGI.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reamin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("reamax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reaaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblREA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("strmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("strmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("straug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblSTR.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chamin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("chamax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chaaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblCHA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("intmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblINT.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("logmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblLOG.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilmin", token: token).ConfigureAwait(false))?.Value
                               ?? 0.ToString(GlobalSettings.CultureInfo)) + '/' + ((await objXmlMetatype
                                      .SelectSingleNodeAndCacheExpressionAsync("wilmax", token: token).ConfigureAwait(false))?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + '('
                              + ((await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilaug", token: token).ConfigureAwait(false))?.Value
                                 ?? 0.ToString(GlobalSettings.CultureInfo)) + ')';
                    await lblWIL.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                }
                else
                {
                    string strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("bodmin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("agimin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblAGI.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("reamin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblREA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("strmin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblSTR.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("chamin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblCHA.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("intmin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblINT.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("logmin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblLOG.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    strText = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("wilmin", token: token).ConfigureAwait(false))?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblWIL.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                }

                // ReSharper disable once IdentifierTypo
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdQualities))
                {
                    // Build a list of the Metatype's Positive Qualities.
                    foreach (XPathNavigator objXmlQuality in await objXmlMetatype.SelectAndCacheExpressionAsync(
                                 "qualities/*/quality", token: token).ConfigureAwait(false))
                    {
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            sbdQualities.Append(
                                _xmlBaseQualityDataNode
                                    .SelectSingleNode("qualities/quality[name = " + objXmlQuality.Value.CleanXPath()
                                                      + "]/translate")?.Value ?? objXmlQuality.Value);

                            string strSelect = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select", token: token).ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(')
                                            .Append(await _objCharacter.TranslateExtraAsync(strSelect, token: token).ConfigureAwait(false))
                                            .Append(')');
                            }
                        }
                        else
                        {
                            sbdQualities.Append(objXmlQuality.Value);
                            string strSelect = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select", token: token).ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                            {
                                sbdQualities.Append(strSpace).Append('(').Append(strSelect).Append(')');
                            }
                        }

                        sbdQualities.Append(Environment.NewLine);
                    }

                    await lblQualities.DoThreadSafeAsync(x => x.Text = sbdQualities.Length == 0
                                                             ? strNone
                                                             : sbdQualities.ToString(), token).ConfigureAwait(false);
                }

                string strKarma = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("karma", token: token).ConfigureAwait(false))?.Value;
                await lblKarma.DoThreadSafeAsync(x => x.Text = strKarma ?? 0.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                string strSource = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("source", token: token).ConfigureAwait(false))?.Value;
                if (!string.IsNullOrEmpty(strSource))
                {
                    string strPage = (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token).ConfigureAwait(false))?.Value ?? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("page", token: token).ConfigureAwait(false))?.Value;
                    if (!string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
                        await lblSource.DoThreadSafeAsync(x => x.Text = objSource.ToString(), token).ConfigureAwait(false);
                        await lblSource.SetToolTipAsync(objSource.LanguageBookTooltip, token).ConfigureAwait(false);
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                        await lblSource.SetToolTipAsync(strUnknown, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                    await lblSource.SetToolTipAsync(strUnknown, token).ConfigureAwait(false);
                }
            }
            else
            {
                await lblBOD.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblAGI.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblREA.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblSTR.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblCHA.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblINT.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblLOG.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblWIL.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);

                await lblQualities.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);

                await lblKarma.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);

                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            }

            if (objXmlMetatype?.SelectSingleNode("category")?.InnerXml.EndsWith("Spirits", StringComparison.Ordinal) == true)
            {
                if (!await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Visible, token).ConfigureAwait(false) && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                {
                    await chkPossessionBased.DoThreadSafeAsync(x => x.Checked = true, token).ConfigureAwait(false);
                    await cboPossessionMethod.DoThreadSafeAsync(x => x.SelectedValue = _strCurrentPossessionMethod, token).ConfigureAwait(false);
                }
                await chkPossessionBased.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
            }
            else
            {
                await chkPossessionBased.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                }, token).ConfigureAwait(false);
                await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
            }

            // If the Metatype has Force enabled, show the Force NUD.
            string strEssMax = objXmlMetatype != null
                ? (await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("essmax", token: token).ConfigureAwait(false))?.Value ?? string.Empty
                : string.Empty;
            int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
            if ((objXmlMetatype != null && await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("forcecreature", token: token).ConfigureAwait(false) != null) || intPos != -1)
            {
                if (intPos != -1)
                {
                    string strD6 = await LanguageManager.GetStringAsync("String_D6", token: token).ConfigureAwait(false);
                    if (intPos > 0)
                    {
                        --intPos;
                        await lblForceLabel.DoThreadSafeAsync(x => x.Text = strEssMax.Substring(intPos, 3).Replace("D6", strD6), token).ConfigureAwait(false);
                        await nudForce.DoThreadSafeAsync(x => x.Maximum = Convert.ToInt32(strEssMax[intPos], GlobalSettings.InvariantCultureInfo) * 6, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblForceLabel.DoThreadSafeAsync(x => x.Text = 1.ToString(GlobalSettings.CultureInfo) + strD6, token).ConfigureAwait(false);
                        await nudForce.DoThreadSafeAsync(x => x.Maximum = 6, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    string strText = await LanguageManager.GetStringAsync(
                        await objXmlMetatype.SelectSingleNodeAndCacheExpressionAsync("forceislevels", token: token).ConfigureAwait(false) != null
                            ? "String_Level"
                            : "String_Force", token: token).ConfigureAwait(false);
                    await lblForceLabel.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    await nudForce.DoThreadSafeAsync(x => x.Maximum = 100, token).ConfigureAwait(false);
                }
                await lblForceLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await nudForce.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
            }
            else
            {
                await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await nudForce.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
            }

            bool blnVisible = await lblBOD.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblBODLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblAGI.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblAGILabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblREA.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblREALabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblSTR.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblSTRLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblCHA.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblCHALabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblINT.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblINTLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblLOG.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblLOGLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblWIL.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblWILLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblQualities.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblQualitiesLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblKarma.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblKarmaLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            blnVisible = await lblSource.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
        }

        private async ValueTask PopulateMetavariants(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            XPathNavigator objXmlMetatype = null;
            if (!string.IsNullOrEmpty(strSelectedMetatype))
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[id = " + strSelectedMetatype.CleanXPath() + ']');
            // Don't attempt to do anything if nothing is selected.
            if (objXmlMetatype != null)
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetavariants))
                {
                    lstMetavariants.Add(new ListItem(Guid.Empty, await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false)));
                    foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select(
                                 "metavariants/metavariant[" + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false) + ']'))
                    {
                        string strId = (await objXmlMetavariant.SelectSingleNodeAndCacheExpressionAsync("id", token: token).ConfigureAwait(false))?.Value;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            lstMetavariants.Add(new ListItem(strId,
                                                             (await objXmlMetavariant
                                                                    .SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value
                                                             ?? (await objXmlMetavariant
                                                                       .SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value
                                                             ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                        }
                    }

                    // Retrieve the list of Metavariants for the selected Metatype.

                    bool blnOldLoading = _blnLoading;
                    string strOldSelectedValue
                        = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                          ?? _objCharacter?.MetavariantGuid.ToString(
                              "D", GlobalSettings.InvariantCultureInfo);
                    _blnLoading = true;
                    await cboMetavariant.PopulateWithListItemsAsync(lstMetavariants, token).ConfigureAwait(false);
                    await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = lstMetavariants.Count > 1, token).ConfigureAwait(false);
                    _blnLoading = blnOldLoading;
                    if (!string.IsNullOrEmpty(strOldSelectedValue))
                    {
                        bool blnDoProcess = await cboMetavariant.DoThreadSafeFuncAsync(x =>
                        {
                            if (x.SelectedValue?.ToString() == strOldSelectedValue)
                                return true;
                            x.SelectedValue = strOldSelectedValue;
                            return false;
                        }, token).ConfigureAwait(false);
                        if (blnDoProcess)
                            await ProcessMetavariantSelectedChanged(token).ConfigureAwait(false);
                    }
                    await cboMetavariant.DoThreadSafeAsync(x =>
                    {
                        if (x.SelectedIndex == -1 && lstMetavariants.Count > 0)
                            x.SelectedIndex = 0;
                    }, token).ConfigureAwait(false);
                }

                await lblMetavariantLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await cboMetavariant.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
            }
            else
            {
                await lblMetavariantLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await cboMetavariant.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                // Clear the Metavariant list if nothing is currently selected.
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                await cboMetavariant.PopulateWithListItemsAsync(new ListItem(Guid.Empty, await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false)).Yield(), token).ConfigureAwait(false);
                await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                _blnLoading = blnOldLoading;
                await cboMetavariant.DoThreadSafeAsync(x => x.SelectedIndex = 0, token).ConfigureAwait(false);

                await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await nudForce.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        private async ValueTask PopulateMetatypes(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSelectedCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            string strSearchText       = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedCategory) || !string.IsNullOrEmpty(strSearchText))
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstMetatypeItems))
                {
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');
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
                        string strId = (await xmlMetatype.SelectSingleNodeAndCacheExpressionAsync("id", token: token).ConfigureAwait(false))?.Value;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            lstMetatypeItems.Add(new ListItem(strId,
                                                              (await xmlMetatype
                                                                     .SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))
                                                                  ?.Value
                                                              ?? (await xmlMetatype.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))
                                                                            ?.Value
                                                              ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                        }
                    }

                    lstMetatypeItems.Sort(CompareListItems.CompareNames);

                    bool blnOldLoading = _blnLoading;
                    string strOldSelected
                        = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                          ?? _objCharacter?.MetatypeGuid.ToString(
                              "D", GlobalSettings.InvariantCultureInfo);
                    if (strOldSelected == Guid.Empty.ToString("D", GlobalSettings.InvariantCultureInfo))
                    {
                        XPathNavigator objOldMetatypeNode = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);
                        if (objOldMetatypeNode != null)
                            strOldSelected = (await objOldMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("id", token: token).ConfigureAwait(false))
                                             ?.Value
                                             ?? string.Empty;
                        else
                            strOldSelected = string.Empty;
                    }

                    _blnLoading = true;
                    await lstMetatypes.PopulateWithListItemsAsync(lstMetatypeItems, token).ConfigureAwait(false);
                    _blnLoading = blnOldLoading;
                    // Attempt to select the default Human item. If it could not be found, select the first item in the list instead.
                    if (!string.IsNullOrEmpty(strOldSelected))
                    {
                        bool blnDoProcess = await lstMetatypes.DoThreadSafeFuncAsync(x =>
                        {
                            if (x.SelectedValue?.ToString() == strOldSelected)
                                return true;
                            x.SelectedValue = strOldSelected;
                            return false;
                        }, token).ConfigureAwait(false);
                        if (blnDoProcess)
                            await ProcessMetatypeSelectedChanged(token).ConfigureAwait(false);
                    }
                    await lstMetatypes.DoThreadSafeAsync(x =>
                    {
                        if (x.SelectedIndex == -1 && lstMetatypeItems.Count > 0)
                            x.SelectedIndex = 0;
                    }, token).ConfigureAwait(false);
                }
            }
            else
            {
                await lstMetatypes.DoThreadSafeAsync(x =>
                {
                    x.BeginUpdate();
                    try
                    {
                        x.DataSource = null;
                    }
                    finally
                    {
                        x.EndUpdate();
                    }
                }, token).ConfigureAwait(false);

                await chkPossessionBased.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                }, token).ConfigureAwait(false);
                await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Custom Methods

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
                try
                {
                    await PopulateMetatypes().ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
