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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Microsoft.VisualStudio.Threading;
using IAsyncDisposable = System.IAsyncDisposable;

namespace Chummer
{
    public partial class SelectMetatypeKarma : Form, IHasCharacterObject
    {
        private int _intLoading = 1;

        private readonly Character _objCharacter;
        private string _strCurrentPossessionMethod;

        private readonly XPathNavigator _xmlBaseMetatypeDataNode;
        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XmlNode _xmlSkillsDocumentKnowledgeSkillsNode;
        private readonly XmlNode _xmlMetatypeDocumentMetatypesNode;
        private readonly XmlNode _xmlQualityDocumentQualitiesNode;
        private readonly AsyncLazy<XmlNode> _xmlCritterPowerDocumentPowersNode;

        private CancellationTokenSource _objPopulateMetatypesCancellationTokenSource;
        private CancellationTokenSource _objPopulateMetavariantsCancellationTokenSource;
        private CancellationTokenSource _objRefreshSelectedMetavariantCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        public Character CharacterObject => _objCharacter;

        #region Form Events

        public SelectMetatypeKarma(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            string strXmlFile = _objCharacter.IsCritter ? "critters.xml" : "metatypes.xml";
            _xmlMetatypeDocumentMetatypesNode = _objCharacter.LoadData(strXmlFile).SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = _objCharacter.LoadDataXPath(strXmlFile).SelectSingleNodeAndCacheExpression("/chummer");
            _xmlSkillsDocumentKnowledgeSkillsNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/knowledgeskills");
            _xmlQualityDocumentQualitiesNode = _objCharacter.LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlCritterPowerDocumentPowersNode
                = new AsyncLazy<XmlNode>(
                    async () => (await _objCharacter.LoadDataAsync("critterpowers.xml").ConfigureAwait(false))
                        .SelectSingleNode("/chummer/powers"), Utils.JoinableTaskFactory);
        }

        private async void SelectMetatypeKarma_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        // Populate the Metatype Category list.
                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstCategories))
                        {
                            // Create a list of Categories.
                            XPathNavigator xmlMetatypesNode
                                = _xmlBaseMetatypeDataNode.SelectSingleNodeAndCacheExpression("metatypes", _objGenericToken);
                            if (xmlMetatypesNode != null)
                            {
                                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                                out HashSet<string> setAlreadyProcessed))
                                {
                                    foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode
                                                 .SelectAndCacheExpression(
                                                     "categories/category", _objGenericToken))
                                    {
                                        string strInnerText = objXmlCategory.Value;
                                        if (!setAlreadyProcessed.Add(strInnerText))
                                            continue;
                                        if (xmlMetatypesNode.SelectSingleNode(
                                                "metatype[category = " + strInnerText.CleanXPath()
                                                                       + " and " + await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).BookXPathAsync(token: _objGenericToken).ConfigureAwait(false)
                                                                       + "]")
                                            != null)
                                        {
                                            lstCategories.Add(new ListItem(strInnerText,
                                                                           objXmlCategory
                                                                               .SelectSingleNodeAndCacheExpression(
                                                                                   "@translate", _objGenericToken)
                                                                           ?.Value
                                                                           ?? strInnerText));
                                        }
                                    }
                                }
                            }

                            lstCategories.Sort(CompareListItems.CompareNames);
                            lstCategories.Insert(
                                0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: _objGenericToken).ConfigureAwait(false)));

                            await cboCategory.PopulateWithListItemsAsync(lstCategories, _objGenericToken).ConfigureAwait(false);
                            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
                            string strMetatypeCategory = await _objCharacter.GetMetatypeCategoryAsync(_objGenericToken).ConfigureAwait(false);
                            await cboCategory.DoThreadSafeAsync(x =>
                            {
                                x.SelectedValue = strMetatypeCategory;
                                if (x.SelectedIndex == -1 && lstCategories.Count > 0)
                                    x.SelectedIndex = 0;
                            }, _objGenericToken).ConfigureAwait(false);
                        }

                        // Add Possession and Inhabitation to the list of Critter Tradition variations.
                        await chkPossessionBased.SetToolTipTextAsync(
                            await LanguageManager.GetStringAsync("Tip_Metatype_PossessionTradition", token: _objGenericToken).ConfigureAwait(false), _objGenericToken).ConfigureAwait(false);

                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstMethods))
                        {
                            if (GlobalSettings.Language != GlobalSettings.DefaultLanguage)
                            {
                                XmlNode objCritterPowersDataNode = await _xmlCritterPowerDocumentPowersNode
                                                                         .GetValueAsync(_objGenericToken)
                                                                         .ConfigureAwait(false);
                                if (objCritterPowersDataNode != null)
                                {
                                    lstMethods.Add(new ListItem("Possession",
                                                                objCritterPowersDataNode
                                                                    .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                                        "power[name = \"Possession\"]/translate", _objGenericToken)
                                                                ?.Value
                                                                ?? "Possession"));
                                    lstMethods.Add(new ListItem("Inhabitation",
                                                                objCritterPowersDataNode
                                                                    .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                                        "power[name = \"Inhabitation\"]/translate", _objGenericToken)
                                                                ?.Value
                                                                ?? "Inhabitation"));
                                }
                                else
                                {
                                    lstMethods.Add(new ListItem("Possession", "Possession"));
                                    lstMethods.Add(new ListItem("Inhabitation", "Inhabitation"));
                                }
                            }
                            else
                            {
                                lstMethods.Add(new ListItem("Possession", "Possession"));
                                lstMethods.Add(new ListItem("Inhabitation", "Inhabitation"));
                            }

                            lstMethods.Sort(CompareListItems.CompareNames);

                            await (await _objCharacter.GetCritterPowersAsync(_objGenericToken).ConfigureAwait(false)).ForEachWithBreakAsync(y =>
                            {
                                string strLoop = y.Name;
                                if (lstMethods.Exists(x => strLoop.Equals(x.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
                                {
                                    _strCurrentPossessionMethod = strLoop;
                                    return false;
                                }
                                return true;
                            }, _objGenericToken).ConfigureAwait(false);
                            await cboPossessionMethod.PopulateWithListItemsAsync(lstMethods, _objGenericToken).ConfigureAwait(false);
                        }

                        await PopulateMetatypes(_objGenericToken).ConfigureAwait(false);
                        await PopulateMetavariants(_objGenericToken).ConfigureAwait(false);
                        await RefreshSelectedMetavariant(_objGenericToken).ConfigureAwait(false);

                        Interlocked.Decrement(ref _intLoading);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SelectMetatypeKarma_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateMetatypesCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateMetavariantsCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshSelectedMetavariantCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        #endregion Form Events

        #region Control Events

        private async void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await ProcessMetatypeSelectedChanged(_objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private Task ProcessMetatypeSelectedChanged(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return Task.CompletedTask;
            return PopulateMetavariants(token);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await MetatypeSelected(_objGenericToken).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await ProcessMetavariantSelectedChanged(_objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private Task ProcessMetavariantSelectedChanged(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return Task.CompletedTask;
            return RefreshSelectedMetavariant(token);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void RefreshMetatypesControl(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await PopulateMetatypes(_objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    bool blnTemp = await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);
                    await cboPossessionMethod.DoThreadSafeAsync(x => x.Enabled = blnTemp, _objGenericToken).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        #endregion Control Events

        #region Custom Methods

        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private async Task MetatypeSelected(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedMetatypeCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? Utils.GuidEmptyString;

                XmlNode objXmlMetatype
                    = _xmlMetatypeDocumentMetatypesNode.TryGetNodeByNameOrId("metatype", strSelectedMetatype);

                if (objXmlMetatype == null)
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        this, await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return;
                }

                IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Remove all priority-given qualities (relevant when switching from Priority/Sum-to-Ten to Karma)
                    for (int i = await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                    {
                        if (i >= await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false))
                            continue;
                        Quality objQuality = await _objCharacter.Qualities.GetValueAtAsync(i, token).ConfigureAwait(false);
                        if (await objQuality.GetOriginSourceAsync(token).ConfigureAwait(false) == QualitySource.Heritage)
                            await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                    }

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant.IsEmptyGuid())
                        strSelectedMetavariant
                            = objXmlMetatype
                                  .SelectSingleNodeAndCacheExpressionAsNavigator(
                                      "metavariants/metavariant[name = \"Human\"]/id", _objGenericToken)
                              ?.Value
                              ?? Utils.GuidEmptyString;
                    if (_objCharacter.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetatype
                        || _objCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetavariant)
                    {
                        // Remove qualities that require the old metatype
                        List<Quality> lstQualitiesToCheck = new List<Quality>(await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false));
                        await _objCharacter.Qualities.ForEachAsync(async objQuality =>
                        {
                            QualitySource eOriginSource = await objQuality.GetOriginSourceAsync(token).ConfigureAwait(false);
                            if (eOriginSource == QualitySource.Improvement
                                || eOriginSource == QualitySource.QualityLevelImprovement
                                || eOriginSource == QualitySource.Metatype
                                || eOriginSource == QualitySource.MetatypeRemovable
                                || eOriginSource == QualitySource.MetatypeRemovedAtChargen)
                                return;
                            XPathNavigator xmlBaseNode
                                = await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                            XPathNavigator xmlRestrictionNode
                                = xmlBaseNode?.SelectSingleNodeAndCacheExpression("required", token);
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metatype", token) != null
                                 || xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metavariant", token) != null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode
                                    = xmlBaseNode?.SelectSingleNodeAndCacheExpression("forbidden", token);
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metatype", token) != null
                                     || xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metavariant", token) != null))
                                {
                                    lstQualitiesToCheck.Add(objQuality);
                                }
                            }
                        }, token).ConfigureAwait(false);

                        int intForce = await nudForce.DoThreadSafeFuncAsync(x => x.Visible ? x.ValueAsInt : 0, token).ConfigureAwait(false);
                        await _objCharacter.CreateAsync(strSelectedMetatypeCategory, strSelectedMetatype,
                            strSelectedMetavariant,
                            objXmlMetatype, intForce, _xmlQualityDocumentQualitiesNode,
                            await _xmlCritterPowerDocumentPowersNode
                                .GetValueAsync(_objGenericToken)
                                .ConfigureAwait(false), _xmlSkillsDocumentKnowledgeSkillsNode,
                            await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false)
                                ? await cboPossessionMethod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(),
                                    token).ConfigureAwait(false)
                                : string.Empty, token).ConfigureAwait(false);
                        foreach (Quality objQuality in lstQualitiesToCheck)
                        {
                            XPathNavigator objLoopNode
                                = await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                            // Set strIgnoreQuality to quality's name to make sure limit counts are not an issue
                            if (objLoopNode != null && !await objLoopNode.RequirementsMetAsync(_objCharacter, strIgnoreQuality: await objQuality.GetNameAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                                await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                        }
                    }

                    // Flip all attribute, skill, and skill group points to karma levels (relevant when switching from Priority/Sum-to-Ten to Karma)
                    await (await _objCharacter.AttributeSection.GetAttributesAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objAttrib =>
                    {
                        // This ordering makes sure data bindings to numeric up-downs with maxima don't get broken
                        int intBase = await objAttrib.GetBaseAsync(token).ConfigureAwait(false);
                        await objAttrib.SetBaseAsync(0, token).ConfigureAwait(false);
                        await objAttrib.ModifyKarmaAsync(intBase, token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);

                    await (await _objCharacter.SkillsSection.GetSkillsAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objSkill =>
                    {
                        // This ordering makes sure data bindings to numeric up-downs with maxima don't get broken
                        int intBase = await objSkill.GetBasePointsAsync(token).ConfigureAwait(false);
                        await objSkill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                        await objSkill.ModifyKarmaPointsAsync(intBase, token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);

                    await (await _objCharacter.SkillsSection.GetSkillGroupsAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objGroup =>
                    {
                        // This ordering makes sure data bindings to numeric up-downs with maxima don't get broken
                        int intBase = await objGroup.GetBasePointsAsync(token).ConfigureAwait(false);
                        await objGroup.SetBasePointsAsync(0, token).ConfigureAwait(false);
                        await objGroup.ModifyKarmaPointsAsync(intBase, token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
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
                await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype", token: token).ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
            }
        }

        private async Task RefreshSelectedMetavariant(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshSelectedMetavariantCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                XPathNavigator objXmlMetatype = null;
                XPathNavigator objXmlMetavariant = null;
                string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedMetatype))
                {
                    objXmlMetatype = _xmlBaseMetatypeDataNode.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                    string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                    if (objXmlMetatype != null && !string.IsNullOrEmpty(strSelectedMetavariant) && !string.Equals(
                            strSelectedMetavariant, Utils.GuidEmptyString, StringComparison.OrdinalIgnoreCase))
                    {
                        objXmlMetavariant = objXmlMetatype.TryGetNodeByNameOrId("metavariants/metavariant", strSelectedMetavariant);
                    }
                }

                string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
                if (objXmlMetavariant != null)
                {
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    if (objXmlMetavariant.SelectSingleNodeAndCacheExpression("forcecreature", token: token) == null)
                    {
                        string strText = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmin", token: token)?.Value
                                          ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                 .SelectSingleNodeAndCacheExpression("bodmax", token: token)?.Value
                                             ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                         + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodaug", token: token)?.Value
                                            ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                        string strText2 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("agimax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("agiaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblAGI.DoThreadSafeAsync(x => x.Text = strText2, token).ConfigureAwait(false);
                        string strText3 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("reamax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("reaaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblREA.DoThreadSafeAsync(x => x.Text = strText3, token).ConfigureAwait(false);
                        string strText4 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("strmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("straug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblSTR.DoThreadSafeAsync(x => x.Text = strText4, token).ConfigureAwait(false);
                        string strText5 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("chamax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("chaaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblCHA.DoThreadSafeAsync(x => x.Text = strText5, token).ConfigureAwait(false);
                        string strText6 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("intmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("intaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblINT.DoThreadSafeAsync(x => x.Text = strText6, token).ConfigureAwait(false);
                        string strText7 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("logmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("logaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblLOG.DoThreadSafeAsync(x => x.Text = strText7, token).ConfigureAwait(false);
                        string strText8 = (objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetavariant
                                                  .SelectSingleNodeAndCacheExpression("wilmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblWIL.DoThreadSafeAsync(x => x.Text = strText8, token).ConfigureAwait(false);
                    }
                    else
                    {
                        string strText = objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmin", token: token)?.Value
                                         ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin", token: token)?.Value
                                         ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                        string strText2 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblAGI.DoThreadSafeAsync(x => x.Text = strText2, token).ConfigureAwait(false);
                        string strText3 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblREA.DoThreadSafeAsync(x => x.Text = strText3, token).ConfigureAwait(false);
                        string strText4 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblSTR.DoThreadSafeAsync(x => x.Text = strText4, token).ConfigureAwait(false);
                        string strText5 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblCHA.DoThreadSafeAsync(x => x.Text = strText5, token).ConfigureAwait(false);
                        string strText6 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblINT.DoThreadSafeAsync(x => x.Text = strText6, token).ConfigureAwait(false);
                        string strText7 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblLOG.DoThreadSafeAsync(x => x.Text = strText7, token).ConfigureAwait(false);
                        string strText8 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmin", token: token)?.Value
                                  ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin", token: token)?.Value
                                  ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblWIL.DoThreadSafeAsync(x => x.Text = strText8, token).ConfigureAwait(false);
                    }

                    // ReSharper disable once IdentifierTypo
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdQualities))
                    {
                        // Build a list of the Metavariant's Qualities.
                        foreach (XPathNavigator objXmlQuality in objXmlMetavariant.SelectAndCacheExpression(
                                     "qualities/*/quality", token: token))
                        {
                            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                StringComparison.OrdinalIgnoreCase))
                            {
                                XPathNavigator xmlLoopQualityNode = _xmlBaseQualityDataNode.TryGetNodeByNameOrId("qualities/quality", objXmlQuality.Value);

                                sbdQualities.Append(
                                    xmlLoopQualityNode?.SelectSingleNodeAndCacheExpression("translate", token: token)
                                        ?.Value
                                    ?? objXmlQuality.Value);

                                string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select", token: token)?.Value;
                                if (!string.IsNullOrEmpty(strSelect))
                                {
                                    sbdQualities.Append(strSpace, '(')
                                                .Append(await _objCharacter.TranslateExtraAsync(strSelect, token: token).ConfigureAwait(false), ')');
                                }
                            }
                            else
                            {
                                sbdQualities.Append(objXmlQuality.Value);
                                string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select", token: token)?.Value;
                                if (!string.IsNullOrEmpty(strSelect))
                                {
                                    sbdQualities.Append(strSpace, '(').Append(strSelect, ')');
                                }
                            }

                            sbdQualities.Append(Environment.NewLine);
                        }

                        string strText = sbdQualities.Length > 0 ? sbdQualities.ToString() : strNone;
                        await lblQualities.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    }

                    string strKarma = objXmlMetavariant.SelectSingleNodeAndCacheExpression("karma", token: token)?.Value;
                    await lblKarma.DoThreadSafeAsync(x => x.Text = strKarma ?? 0.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                    string strSource = objXmlMetavariant.SelectSingleNodeAndCacheExpression("source", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strSource))
                    {
                        string strPage = objXmlMetavariant.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? objXmlMetavariant.SelectSingleNodeAndCacheExpression("page", token: token)?.Value;
                        if (!string.IsNullOrEmpty(strPage))
                        {
                            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
                            await objSource.SetControlAsync(lblSource, this, token).ConfigureAwait(false);
                        }
                        else
                        {
                            string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                            await lblSource.SetToolTipTextAsync(strUnknown, token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                        await lblSource.SetToolTipTextAsync(strUnknown, token).ConfigureAwait(false);
                    }
                }
                else if (objXmlMetatype != null)
                {
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    if (objXmlMetatype.SelectSingleNodeAndCacheExpression("forcecreature", token: token) == null)
                    {
                        string strText = (objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin", token: token)?.Value
                                          ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                 .SelectSingleNodeAndCacheExpression("bodmax", token: token)?.Value
                                             ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                         + (objXmlMetatype.SelectSingleNodeAndCacheExpression("bodaug", token: token)?.Value
                                            ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                        string strText2 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("agimax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("agiaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblAGI.DoThreadSafeAsync(x => x.Text = strText2, token).ConfigureAwait(false);
                        string strText3 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("reamax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("reaaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblREA.DoThreadSafeAsync(x => x.Text = strText3, token).ConfigureAwait(false);
                        string strText4 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("strmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("straug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblSTR.DoThreadSafeAsync(x => x.Text = strText4, token).ConfigureAwait(false);
                        string strText5 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("chamax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("chaaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblCHA.DoThreadSafeAsync(x => x.Text = strText5, token).ConfigureAwait(false);
                        string strText6 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("intmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("intaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblINT.DoThreadSafeAsync(x => x.Text = strText6, token).ConfigureAwait(false);
                        string strText7 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("logmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("logaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblLOG.DoThreadSafeAsync(x => x.Text = strText7, token).ConfigureAwait(false);
                        string strText8 = (objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin", token: token)?.Value
                                   ?? 0.ToString(GlobalSettings.CultureInfo)) + "/" + (objXmlMetatype
                                                  .SelectSingleNodeAndCacheExpression("wilmax", token: token)?.Value
                                      ?? 0.ToString(GlobalSettings.CultureInfo)) + strSpace + "("
                                  + (objXmlMetatype.SelectSingleNodeAndCacheExpression("wilaug", token: token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo)) + ")";
                        await lblWIL.DoThreadSafeAsync(x => x.Text = strText8, token).ConfigureAwait(false);
                    }
                    else
                    {
                        string strText = objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblBOD.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                        string strText2 = objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblAGI.DoThreadSafeAsync(x => x.Text = strText2, token).ConfigureAwait(false);
                        string strText3 = objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblREA.DoThreadSafeAsync(x => x.Text = strText3, token).ConfigureAwait(false);
                        string strText4 = objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblSTR.DoThreadSafeAsync(x => x.Text = strText4, token).ConfigureAwait(false);
                        string strText5 = objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblCHA.DoThreadSafeAsync(x => x.Text = strText5, token).ConfigureAwait(false);
                        string strText6 = objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblINT.DoThreadSafeAsync(x => x.Text = strText6, token).ConfigureAwait(false);
                        string strText7 = objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblLOG.DoThreadSafeAsync(x => x.Text = strText7, token).ConfigureAwait(false);
                        string strText8 = objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin", token: token)?.Value ?? 0.ToString(GlobalSettings.CultureInfo);
                        await lblWIL.DoThreadSafeAsync(x => x.Text = strText8, token).ConfigureAwait(false);
                    }

                    // ReSharper disable once IdentifierTypo
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdQualities))
                    {
                        // Build a list of the Metatype's Positive Qualities.
                        foreach (XPathNavigator objXmlQuality in objXmlMetatype.SelectAndCacheExpression(
                                     "qualities/*/quality", token: token))
                        {
                            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                StringComparison.OrdinalIgnoreCase))
                            {
                                XPathNavigator xmlLoopQualityNode = _xmlBaseQualityDataNode.TryGetNodeByNameOrId("qualities/quality", objXmlQuality.Value);

                                sbdQualities.Append(
                                    xmlLoopQualityNode?.SelectSingleNodeAndCacheExpression("translate", token: token)
                                        ?.Value
                                    ?? objXmlQuality.Value);

                                string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select", token: token)?.Value;
                                if (!string.IsNullOrEmpty(strSelect))
                                {
                                    sbdQualities.Append(strSpace, '(')
                                                .Append(await _objCharacter.TranslateExtraAsync(strSelect, token: token).ConfigureAwait(false), ')');
                                }
                            }
                            else
                            {
                                sbdQualities.Append(objXmlQuality.Value);
                                string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select", token: token)?.Value;
                                if (!string.IsNullOrEmpty(strSelect))
                                {
                                    sbdQualities.Append(strSpace, '(').Append(strSelect, ')');
                                }
                            }

                            sbdQualities.Append(Environment.NewLine);
                        }

                        string strText = sbdQualities.Length > 0 ? sbdQualities.ToString() : strNone;
                        await lblQualities.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                    }

                    string strKarma = objXmlMetatype.SelectSingleNodeAndCacheExpression("karma", token: token)?.Value;
                    await lblKarma.DoThreadSafeAsync(x => x.Text = strKarma ?? 0.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                    string strSource = objXmlMetatype.SelectSingleNodeAndCacheExpression("source", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strSource))
                    {
                        string strPage = objXmlMetatype.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("page", token: token)?.Value;
                        if (!string.IsNullOrEmpty(strPage))
                        {
                            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
                            await objSource.SetControlAsync(lblSource, this, token).ConfigureAwait(false);
                        }
                        else
                        {
                            string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                            await lblSource.SetToolTipTextAsync(strUnknown, token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        await lblSource.DoThreadSafeAsync(x => x.Text = strUnknown, token).ConfigureAwait(false);
                        await lblSource.SetToolTipTextAsync(strUnknown, token).ConfigureAwait(false);
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

                if (objXmlMetatype != null && objXmlMetatype.SelectSingleNodeAndCacheExpression("category", token)?.InnerXmlViaPool(token).EndsWith("Spirits", StringComparison.Ordinal) == true)
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
                    ? objXmlMetatype.SelectSingleNodeAndCacheExpression("essmax", token: token)?.Value ?? string.Empty
                    : string.Empty;
                int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                if ((objXmlMetatype?.SelectSingleNodeAndCacheExpression("forcecreature", token: token) != null) || intPos != -1)
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
                            objXmlMetatype.SelectSingleNodeAndCacheExpression("forceislevels", token: token) != null
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
                bool blnVisible2 = await lblAGI.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblAGILabel.DoThreadSafeAsync(x => x.Visible = blnVisible2, token).ConfigureAwait(false);
                bool blnVisible3 = await lblREA.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblREALabel.DoThreadSafeAsync(x => x.Visible = blnVisible3, token).ConfigureAwait(false);
                bool blnVisible4 = await lblSTR.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblSTRLabel.DoThreadSafeAsync(x => x.Visible = blnVisible4, token).ConfigureAwait(false);
                bool blnVisible5 = await lblCHA.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblCHALabel.DoThreadSafeAsync(x => x.Visible = blnVisible5, token).ConfigureAwait(false);
                bool blnVisible6 = await lblINT.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblINTLabel.DoThreadSafeAsync(x => x.Visible = blnVisible6, token).ConfigureAwait(false);
                bool blnVisible7 = await lblLOG.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblLOGLabel.DoThreadSafeAsync(x => x.Visible = blnVisible7, token).ConfigureAwait(false);
                bool blnVisible8 = await lblWIL.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblWILLabel.DoThreadSafeAsync(x => x.Visible = blnVisible8, token).ConfigureAwait(false);
                bool blnVisible9 = await lblQualities.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblQualitiesLabel.DoThreadSafeAsync(x => x.Visible = blnVisible9, token).ConfigureAwait(false);
                bool blnVisible10 = await lblKarma.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblKarmaLabel.DoThreadSafeAsync(x => x.Visible = blnVisible10, token).ConfigureAwait(false);
                bool blnVisible11 = await lblSource.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnVisible11, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateMetavariants(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateMetavariantsCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                XPathNavigator objXmlMetatype = null;
                if (!string.IsNullOrEmpty(strSelectedMetatype))
                    objXmlMetatype = _xmlBaseMetatypeDataNode.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                // Don't attempt to do anything if nothing is selected.
                if (objXmlMetatype != null)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetavariants))
                    {
                        lstMetavariants.Add(new ListItem(Guid.Empty, await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false)));
                        foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select(
                                     "metavariants/metavariant[" + await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false) + "]"))
                        {
                            string strId = objXmlMetavariant.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                            if (!string.IsNullOrEmpty(strId))
                            {
                                lstMetavariants.Add(new ListItem(strId,
                                                                 objXmlMetavariant.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                                 ?? objXmlMetavariant.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                            }
                        }

                        // Retrieve the list of Metavariants for the selected Metatype.

                        string strOldSelectedValue
                            = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                              ?? _objCharacter?.MetavariantGuid.ToString(
                                  "D", GlobalSettings.InvariantCultureInfo);
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await cboMetavariant.PopulateWithListItemsAsync(lstMetavariants, token).ConfigureAwait(false);
                            await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = lstMetavariants.Count > 1, token)
                                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }

                        if (!string.IsNullOrEmpty(strOldSelectedValue))
                        {
                            bool blnDoProcess = await cboMetavariant.DoThreadSafeFuncAsync(x =>
                            {
                                if (x.SelectedValue?.ToString() == strOldSelectedValue)
                                    return true;
                                if (!string.IsNullOrEmpty(strOldSelectedValue))
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
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await cboMetavariant
                              .PopulateWithListItemAsync(
                                  new ListItem(
                                      Guid.Empty,
                                      await LanguageManager.GetStringAsync("String_None", token: token)
                                                           .ConfigureAwait(false)), token).ConfigureAwait(false);
                        await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }

                    await cboMetavariant.DoThreadSafeAsync(x => x.SelectedIndex = 0, token).ConfigureAwait(false);

                    await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await nudForce.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        private async Task PopulateMetatypes(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateMetatypesCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strSelectedCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                string strSearchText = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedCategory) || !string.IsNullOrEmpty(strSearchText))
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstMetatypeItems))
                    {
                        string strFilter = string.Empty;
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                        {
                            sbdFilter.Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false));
                            if (!string.IsNullOrEmpty(strSelectedCategory) && strSelectedCategory != "Show All"
                                                                           && (GlobalSettings.SearchInCategoryOnly
                                                                               || strSearchText.Length == 0))
                                sbdFilter.Append(" and category = ", strSelectedCategory.CleanXPath());

                            if (!string.IsNullOrEmpty(txtSearch.Text))
                                sbdFilter.Append(" and ", CommonFunctions.GenerateSearchXPath(strSearchText));

                            if (sbdFilter.Length > 0)
                                // StringBuilder.Insert can be slow because of in-place replaces, so use concat instead
                                strFilter = string.Concat("[", sbdFilter.Append(']').ToString());
                        }

                        foreach (XPathNavigator xmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                     "metatypes/metatype" + strFilter))
                        {
                            string strId = xmlMetatype.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                            if (!string.IsNullOrEmpty(strId))
                            {
                                lstMetatypeItems.Add(new ListItem(strId,
                                                                  xmlMetatype.SelectSingleNodeAndCacheExpression("translate", token: token)
                                                                      ?.Value
                                                                  ?? xmlMetatype.SelectSingleNodeAndCacheExpression("name", token: token)
                                                                                ?.Value
                                                                  ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                            }
                        }

                        lstMetatypeItems.Sort(CompareListItems.CompareNames);

                        string strOldSelected
                            = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                  .ConfigureAwait(false)
                              ?? _objCharacter.MetatypeGuid.ToString(
                                  "D", GlobalSettings.InvariantCultureInfo);
                        if (strOldSelected.IsEmptyGuid())
                        {
                            XPathNavigator objOldMetatypeNode = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);
                            if (objOldMetatypeNode != null)
                                strOldSelected = objOldMetatypeNode.SelectSingleNodeAndCacheExpression("id", token: token)
                                                 ?.Value
                                                 ?? string.Empty;
                            else
                                strOldSelected = string.Empty;
                        }

                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await lstMetatypes.PopulateWithListItemsAsync(lstMetatypeItems, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }

                        // Attempt to select the default Human item. If it could not be found, select the first item in the list instead.
                        if (!string.IsNullOrEmpty(strOldSelected))
                        {
                            bool blnDoProcess = await lstMetatypes.DoThreadSafeFuncAsync(x =>
                            {
                                if (x.SelectedValue?.ToString() == strOldSelected)
                                    return true;
                                if (!string.IsNullOrEmpty(strOldSelected))
                                    x.SelectedValue = strOldSelected;
                                return false;
                            }, token).ConfigureAwait(false);
                            if (blnDoProcess)
                                await ProcessMetatypeSelectedChanged(token).ConfigureAwait(false);
                        }
                        else
                            await ProcessMetatypeSelectedChanged(token).ConfigureAwait(false);
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
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            try
            {
                await CommonFunctions.OpenPdfFromControl(sender, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        #endregion Custom Methods
    }
}
