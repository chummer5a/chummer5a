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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using Microsoft.VisualStudio.Threading;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class SelectMetatypePriority : Form
    {
        private readonly Character _objCharacter;
        private string _strCurrentPossessionMethod;

        private int _intLoading = 1;
        private readonly Dictionary<string, int> _dicSumtoTenValues;
        private readonly List<string> _lstPrioritySkills;
        private readonly List<string> _lstPriorities;

        private readonly XPathNavigator _xmlBasePriorityDataNode;
        private readonly XPathNavigator _xmlBaseMetatypeDataNode;
        private readonly XPathNavigator _xmlBaseSkillDataNode;
        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XmlNode _xmlMetatypeDocumentMetatypesNode;
        private readonly XmlNode _xmlQualityDocumentQualitiesNode;
        private readonly AsyncLazy<XmlNode> _xmlCritterPowerDocumentPowersNode;

        private CancellationTokenSource _objLoadMetatypesCancellationTokenSource;
        private CancellationTokenSource _objPopulateMetatypesCancellationTokenSource;
        private CancellationTokenSource _objPopulateMetavariantsCancellationTokenSource;
        private CancellationTokenSource _objPopulateTalentsCancellationTokenSource;
        private CancellationTokenSource _objRefreshSelectedMetatypeCancellationTokenSource;
        private CancellationTokenSource _objProcessTalentsIndexChangedCancellationTokenSource;
        private CancellationTokenSource _objManagePriorityItemsCancellationTokenSource;
        private CancellationTokenSource _objSumToTenCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        #region Form Events

        public SelectMetatypePriority(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objLoadMetatypesCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateMetatypesCancellationTokenSource, null);
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
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateTalentsCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshSelectedMetatypeCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessTalentsIndexChangedCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objManagePriorityItemsCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objSumToTenCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericCancellationTokenSource.Dispose();
            };
            if (string.IsNullOrEmpty(_objCharacter.SettingsKey))
                _objCharacter.SettingsKey = GlobalSettings.DefaultCharacterSetting;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _lstPrioritySkills = new List<string>(objCharacter.PriorityBonusSkillList);
            _xmlMetatypeDocumentMetatypesNode = _objCharacter.LoadData(strXmlFile).SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = _objCharacter.LoadDataXPath(strXmlFile).SelectSingleNodeAndCacheExpression("/chummer");
            _xmlBasePriorityDataNode = _objCharacter.LoadDataXPath("priorities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlBaseSkillDataNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlQualityDocumentQualitiesNode = _objCharacter.LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlCritterPowerDocumentPowersNode
                = new AsyncLazy<XmlNode>(
                    async () => (await _objCharacter.LoadDataAsync("critterpowers.xml").ConfigureAwait(false))
                        .SelectSingleNode("/chummer/powers"), Utils.JoinableTaskFactory);
            _dicSumtoTenValues = new Dictionary<string, int>(5);
            if (_xmlBasePriorityDataNode != null)
            {
                foreach (XPathNavigator xmlNode in _xmlBasePriorityDataNode.SelectAndCacheExpression("priortysumtotenvalues/*"))
                {
                    _dicSumtoTenValues.Add(xmlNode.Name, xmlNode.ValueAsInt);
                }
            }

            if (_dicSumtoTenValues.Count == 0)
            {
                _dicSumtoTenValues.Add("A", 4);
                _dicSumtoTenValues.Add("B", 3);
                _dicSumtoTenValues.Add("C", 2);
                _dicSumtoTenValues.Add("D", 1);
                _dicSumtoTenValues.Add("E", 0);
            }

            if (!string.IsNullOrEmpty(_objCharacter.Settings.PriorityArray))
            {
                _lstPriorities = new List<string>(5);
                foreach (char c in _objCharacter.Settings.PriorityArray)
                {
                    _lstPriorities.Add(c.ToString());
                }
            }
            else
            {
                _lstPriorities = new List<string> { "A", "B", "C", "D", "E" };
            }

            foreach (string strPriority in _lstPriorities)
                if (!_dicSumtoTenValues.ContainsKey(strPriority))
                    _dicSumtoTenValues.Add(strPriority, 0);
        }

        private async void SelectMetatypePriority_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                        {
                            await lblSumtoTen.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                        }

                        // Populate the Priority Category list.
                        XPathNavigator xmlBasePrioritiesNode
                            = _xmlBasePriorityDataNode.SelectSingleNodeAndCacheExpression("priorities", _objGenericToken);
                        if (xmlBasePrioritiesNode != null)
                        {
                            foreach (XPathNavigator objXmlPriorityCategory in _xmlBasePriorityDataNode
                                         .SelectAndCacheExpression("categories/category", _objGenericToken))
                            {
                                XPathNodeIterator objItems = xmlBasePrioritiesNode.Select(
                                    "priority[category = " + objXmlPriorityCategory.Value.CleanXPath()
                                                           + " and prioritytable = "
                                                           + _objCharacter.Settings.PriorityTable.CleanXPath() + ']');

                                if (objItems.Count == 0)
                                {
                                    objItems = xmlBasePrioritiesNode.Select(
                                        "priority[category = " + objXmlPriorityCategory.Value.CleanXPath()
                                                               + " and not(prioritytable)]");
                                }

                                if (objItems.Count > 0)
                                {
                                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                                   out List<ListItem> lstItems))
                                    {
                                        foreach (XPathNavigator objXmlPriority in objItems)
                                        {
                                            string strValue
                                                = objXmlPriority.SelectSingleNodeAndCacheExpression("value", _objGenericToken)
                                                ?.Value;
                                            if (!string.IsNullOrEmpty(strValue) && _lstPriorities.Contains(strValue))
                                            {
                                                lstItems.Add(new ListItem(
                                                                 strValue,
                                                                 objXmlPriority
                                                                     .SelectSingleNodeAndCacheExpression(
                                                                         "translate", _objGenericToken)
                                                                 ?.Value ??
                                                                 objXmlPriority
                                                                     .SelectSingleNodeAndCacheExpression(
                                                                         "name", _objGenericToken)?.Value ??
                                                                 await LanguageManager.GetStringAsync("String_Unknown", token: _objGenericToken)
                                                                     .ConfigureAwait(false)));
                                            }
                                        }

                                        lstItems.Sort(CompareListItems.CompareNames);
                                        switch (objXmlPriorityCategory.Value)
                                        {
                                            case "Heritage":
                                                await cboHeritage.PopulateWithListItemsAsync(lstItems, _objGenericToken)
                                                                 .ConfigureAwait(false);
                                                break;

                                            case "Talent":
                                                await cboTalent.PopulateWithListItemsAsync(lstItems, _objGenericToken).ConfigureAwait(false);
                                                break;

                                            case "Attributes":
                                                await cboAttributes.PopulateWithListItemsAsync(lstItems, _objGenericToken)
                                                                   .ConfigureAwait(false);
                                                break;

                                            case "Skills":
                                                await cboSkills.PopulateWithListItemsAsync(lstItems, _objGenericToken).ConfigureAwait(false);
                                                break;

                                            case "Resources":
                                                await cboResources.PopulateWithListItemsAsync(lstItems, _objGenericToken)
                                                                  .ConfigureAwait(false);
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        // Set Priority defaults.
                        if (!string.IsNullOrEmpty(_objCharacter.TalentPriority))
                        {
                            //Attributes
                            await cboAttributes.DoThreadSafeAsync(x => x.SelectedIndex
                                                                      = x.FindString(_objCharacter.AttributesPriority[0]
                                                                          .ToString(GlobalSettings
                                                                              .InvariantCultureInfo)), _objGenericToken)
                                               .ConfigureAwait(false);
                            //Heritage (Metatype)
                            await cboHeritage.DoThreadSafeAsync(x => x.SelectedIndex
                                                                    = x.FindString(_objCharacter.MetatypePriority[0]
                                                                                       .ToString(GlobalSettings
                                                                                           .InvariantCultureInfo)), _objGenericToken)
                                             .ConfigureAwait(false);
                            //Resources
                            await cboResources.DoThreadSafeAsync(x => x.SelectedIndex
                                                                     = x.FindString(_objCharacter.ResourcesPriority[0]
                                                                         .ToString(GlobalSettings
                                                                                       .InvariantCultureInfo)), _objGenericToken)
                                              .ConfigureAwait(false);
                            //Skills
                            await cboSkills.DoThreadSafeAsync(x => x.SelectedIndex
                                                                  = x.FindString(_objCharacter.SkillsPriority[0]
                                                                                     .ToString(GlobalSettings
                                                                                         .InvariantCultureInfo)), _objGenericToken)
                                           .ConfigureAwait(false);
                            //Magical/Resonance Talent
                            await cboTalent.DoThreadSafeAsync(x => x.SelectedIndex
                                                                  = x.FindString(_objCharacter.SpecialPriority[0]
                                                                                     .ToString(
                                                                                         GlobalSettings
                                                                                             .InvariantCultureInfo)), _objGenericToken)
                                           .ConfigureAwait(false);

                            await LoadMetatypes(_objGenericToken).ConfigureAwait(false);
                            await PopulateMetatypes(_objGenericToken).ConfigureAwait(false);
                            await PopulateMetavariants(_objGenericToken).ConfigureAwait(false);
                            await PopulateTalents(_objGenericToken).ConfigureAwait(false);
                            await RefreshSelectedMetatype(_objGenericToken).ConfigureAwait(false);

                            //Magical/Resonance Type
                            await cboTalents.DoThreadSafeAsync(x =>
                            {
                                x.SelectedValue = _objCharacter.TalentPriority;
                                if (x.SelectedIndex == -1 && x.Items.Count > 1)
                                    x.SelectedIndex = 0;
                            }, _objGenericToken).ConfigureAwait(false);
                            //Selected Magical Bonus Skill
                            string strSkill = _lstPrioritySkills.ElementAtOrDefault(0);
                            if (!string.IsNullOrEmpty(strSkill))
                            {
                                if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSkill, _objGenericToken).ConfigureAwait(false))
                                {
                                    int intParenthesesIndex = strSkill.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                                    if (intParenthesesIndex > 0)
                                        strSkill = strSkill.Substring(0, intParenthesesIndex);
                                }

                                await cboSkill1.DoThreadSafeAsync(x => x.SelectedValue = strSkill, _objGenericToken).ConfigureAwait(false);
                            }

                            string strSkill2 = _lstPrioritySkills.ElementAtOrDefault(1);
                            if (!string.IsNullOrEmpty(strSkill2))
                            {
                                if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSkill2, _objGenericToken).ConfigureAwait(false))
                                {
                                    int intParenthesesIndex = strSkill2.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                                    if (intParenthesesIndex > 0)
                                        strSkill2 = strSkill2.Substring(0, intParenthesesIndex);
                                }

                                await cboSkill2.DoThreadSafeAsync(x => x.SelectedValue = strSkill2, _objGenericToken).ConfigureAwait(false);
                            }

                            string strSkill3 = _lstPrioritySkills.ElementAtOrDefault(2);
                            if (!string.IsNullOrEmpty(strSkill3))
                            {
                                if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSkill3, _objGenericToken).ConfigureAwait(false))
                                {
                                    int intParenthesesIndex = strSkill3.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                                    if (intParenthesesIndex > 0)
                                        strSkill3 = strSkill3.Substring(0, intParenthesesIndex);
                                }

                                await cboSkill3.DoThreadSafeAsync(x => x.SelectedValue = strSkill3, _objGenericToken).ConfigureAwait(false);
                            }

                            await ProcessTalentsIndexChanged(_objGenericToken).ConfigureAwait(false);

                            switch (_objCharacter.EffectiveBuildMethod)
                            {
                                case CharacterBuildMethod.Priority:
                                    await ManagePriorityItems(cboHeritage, token: _objGenericToken).ConfigureAwait(false);
                                    await ManagePriorityItems(cboAttributes, token: _objGenericToken).ConfigureAwait(false);
                                    await ManagePriorityItems(cboTalent, token: _objGenericToken).ConfigureAwait(false);
                                    await ManagePriorityItems(cboSkills, token: _objGenericToken).ConfigureAwait(false);
                                    await ManagePriorityItems(cboResources, token: _objGenericToken).ConfigureAwait(false);
                                    break;

                                case CharacterBuildMethod.SumtoTen:
                                    await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                                    break;
                            }
                        }
                        else
                        {
                            await cboHeritage.DoThreadSafeAsync(x => x.SelectedIndex = 0, token: _objGenericToken).ConfigureAwait(false);
                            await cboTalent.DoThreadSafeAsync(x => x.SelectedIndex = 0, token: _objGenericToken).ConfigureAwait(false);
                            await cboAttributes.DoThreadSafeAsync(x => x.SelectedIndex = 0, token: _objGenericToken).ConfigureAwait(false);
                            await cboSkills.DoThreadSafeAsync(x => x.SelectedIndex = 0, token: _objGenericToken).ConfigureAwait(false);
                            await cboResources.DoThreadSafeAsync(x => x.SelectedIndex = 0, token: _objGenericToken).ConfigureAwait(false);
                            await ManagePriorityItems(cboHeritage, true, token: _objGenericToken).ConfigureAwait(false);
                            await ManagePriorityItems(cboAttributes, true, token: _objGenericToken).ConfigureAwait(false);
                            await ManagePriorityItems(cboTalent, true, token: _objGenericToken).ConfigureAwait(false);
                            await ManagePriorityItems(cboSkills, true, token: _objGenericToken).ConfigureAwait(false);
                            await ManagePriorityItems(cboResources, true, token: _objGenericToken).ConfigureAwait(false);
                            await LoadMetatypes(token: _objGenericToken).ConfigureAwait(false);
                            await PopulateMetatypes(token: _objGenericToken).ConfigureAwait(false);
                            await PopulateMetavariants(token: _objGenericToken).ConfigureAwait(false);
                            await PopulateTalents(token: _objGenericToken).ConfigureAwait(false);
                            await RefreshSelectedMetatype(token: _objGenericToken).ConfigureAwait(false);
                            if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                                await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                        }

                        // Set up possession boxes
                        // Add Possession and Inhabitation to the list of Critter Tradition variations.
                        await chkPossessionBased.SetToolTipAsync(
                                                    await LanguageManager.GetStringAsync("Tip_Metatype_PossessionTradition", token: _objGenericToken)
                                                                         .ConfigureAwait(false), _objGenericToken)
                                                .ConfigureAwait(false);

                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
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

                            _strCurrentPossessionMethod = _objCharacter.CritterPowers.Select(x => x.Name)
                                                                       .FirstOrDefault(
                                                                           y => lstMethods.Exists(
                                                                               x => y.Equals(
                                                                                   x.Value.ToString(),
                                                                                   StringComparison.OrdinalIgnoreCase)));

                            await cboPossessionMethod.PopulateWithListItemsAsync(lstMethods, _objGenericToken).ConfigureAwait(false);
                        }

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

        private void SelectMetatypePriority_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objLoadMetatypesCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateMetatypesCancellationTokenSource, null);
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
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateTalentsCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshSelectedMetatypeCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessTalentsIndexChangedCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objManagePriorityItemsCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objSumToTenCancellationTokenSource, null);
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
                        if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                        {
                            await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                        }

                        await PopulateMetavariants(_objGenericToken).ConfigureAwait(false);
                        await RefreshSelectedMetatype(_objGenericToken).ConfigureAwait(false);
                        await PopulateTalents(_objGenericToken).ConfigureAwait(false);
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

        private async void cboTalents_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await ProcessTalentsIndexChanged(_objGenericToken).ConfigureAwait(false);
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

        private async Task ProcessTalentsIndexChanged(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessTalentsIndexChangedCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                await cboSkill1.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await cboSkill2.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await cboSkill3.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblMetatypeSkillSelection.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);

                string strSelectedTalents = await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                            .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedTalents))
                {
                    XPathNavigator xmlTalentNode = null;
                    XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Talent\" and value = "
                        + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                          .ConfigureAwait(false) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath()
                        + ")]");
                    foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                    {
                        if (xmlBaseTalentPriorityList.Count == 1 || xmlBaseTalentPriority
                                                                          .SelectSingleNodeAndCacheExpression(
                                                                              "prioritytable", token)
                            != null)
                        {
                            xmlTalentNode
                                = xmlBaseTalentPriority.SelectSingleNode(
                                    "talents/talent[value = " + strSelectedTalents.CleanXPath() + ']');
                            break;
                        }
                    }

                    if (xmlTalentNode != null)
                    {
                        string strSkillCount =
                            xmlTalentNode.SelectSingleNodeAndCacheExpression("skillqty", token)?.Value ??
                            xmlTalentNode.SelectSingleNodeAndCacheExpression("skillgroupqty", token)?.Value ??
                            string.Empty;
                        if (!string.IsNullOrEmpty(strSkillCount) && int.TryParse(strSkillCount, out int intSkillCount))
                        {
                            XPathNavigator xmlSkillTypeNode =
                                xmlTalentNode.SelectSingleNodeAndCacheExpression("skilltype", token) ??
                                xmlTalentNode.SelectSingleNodeAndCacheExpression("skillgrouptype", token);
                            string strSkillType = xmlSkillTypeNode?.Value ?? string.Empty;
                            XPathNodeIterator objNodeList = xmlTalentNode
                                                                  .SelectAndCacheExpression(
                                                                      "skillgroupchoices/skillgroup", token);
                            XPathNodeIterator xmlSkillsList;
                            switch (strSkillType)
                            {
                                case "magic":
                                    xmlSkillsList = GetMagicalSkillList(token);
                                    break;

                                case "resonance":
                                    xmlSkillsList = GetResonanceSkillList(token);
                                    break;

                                case "matrix":
                                    xmlSkillsList = GetMatrixSkillList(token);
                                    break;

                                case "grouped":
                                    xmlSkillsList = BuildSkillCategoryList(objNodeList);
                                    break;

                                case "specific":
                                    xmlSkillsList
                                        = BuildSkillList(xmlTalentNode
                                                               .SelectAndCacheExpression("skillchoices/skill", token));
                                    break;

                                case "xpath":
                                    xmlSkillsList = GetActiveSkillList(
                                        xmlSkillTypeNode?.SelectSingleNodeAndCacheExpression("@xpath", token)?.Value, token);
                                    strSkillType = "active";
                                    break;

                                default:
                                    xmlSkillsList = GetActiveSkillList(token: token);
                                    break;
                            }

                            if (intSkillCount > 0)
                            {
                                using (new FetchSafelyFromPool<List<ListItem>>(
                                           Utils.ListItemListPool, out List<ListItem> lstSkills))
                                {
                                    if (objNodeList.Count > 0)
                                    {
                                        foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                        {
                                            string strInnerText = objXmlSkill.Value;
                                            lstSkills.Add(new ListItem(strInnerText,
                                                                       objXmlSkill
                                                                           .SelectSingleNodeAndCacheExpression(
                                                                               "@translate", token)?.Value
                                                                       ?? strInnerText));
                                        }
                                    }
                                    else
                                    {
                                        foreach (XPathNavigator objXmlSkill in xmlSkillsList)
                                        {
                                            string strName
                                                = objXmlSkill.SelectSingleNodeAndCacheExpression("name", token)?.Value
                                                  ?? await LanguageManager.GetStringAsync("String_Unknown", token: token)
                                                                          .ConfigureAwait(false);
                                            lstSkills.Add(
                                                new ListItem(
                                                    strName,
                                                    objXmlSkill
                                                        .SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                    ?? strName));
                                        }
                                    }

                                    lstSkills.Sort(CompareListItems.CompareNames);
                                    int intOldSelectedIndex = await cboSkill1
                                                                    .DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                    .ConfigureAwait(false);
                                    int intOldDataSourceSize = await cboSkill1
                                                                     .DoThreadSafeFuncAsync(x => x.Items.Count, token)
                                                                     .ConfigureAwait(false);
                                    await cboSkill1.PopulateWithListItemsAsync(lstSkills, token).ConfigureAwait(false);
                                    await cboSkill1.DoThreadSafeAsync(x =>
                                    {
                                        x.Visible = true;
                                        x.Enabled = lstSkills.Count > 1;
                                    }, token).ConfigureAwait(false);
                                    if (intOldDataSourceSize == lstSkills.Count)
                                    {
                                        Interlocked.Increment(ref _intLoading);
                                        try
                                        {
                                            await cboSkill1
                                                  .DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex, token)
                                                  .ConfigureAwait(false);
                                        }
                                        finally
                                        {
                                            Interlocked.Decrement(ref _intLoading);
                                        }
                                    }

                                    if (intSkillCount > 1)
                                    {
                                        int intOldSelectedIndex2 = await cboSkill2
                                                                         .DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                         .ConfigureAwait(false);
                                        int intOldDataSourceSize2 = await cboSkill2
                                                                          .DoThreadSafeFuncAsync(x => x.Items.Count, token)
                                                                          .ConfigureAwait(false);
                                        await cboSkill2.PopulateWithListItemsAsync(lstSkills, token).ConfigureAwait(false);
                                        await cboSkill2.DoThreadSafeAsync(x =>
                                        {
                                            x.Visible = true;
                                            x.Enabled = lstSkills.Count > 2;
                                        }, token).ConfigureAwait(false);
                                        if (intOldDataSourceSize2 == lstSkills.Count)
                                        {
                                            Interlocked.Increment(ref _intLoading);
                                            try
                                            {
                                                await cboSkill2
                                                      .DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex2, token)
                                                      .ConfigureAwait(false);
                                            }
                                            finally
                                            {
                                                Interlocked.Decrement(ref _intLoading);
                                            }
                                        }

                                        if (await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                           .ConfigureAwait(false) == await cboSkill1
                                                .DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false)
                                            && !await ExoticSkill.IsExoticSkillNameAsync(
                                                _objCharacter, await cboSkill2
                                                                     .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                                     .ConfigureAwait(false), token).ConfigureAwait(false))
                                        {
                                            if (await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                               .ConfigureAwait(false) + 1
                                                >= await cboSkill2.DoThreadSafeFuncAsync(x => x.Items.Count, token)
                                                                  .ConfigureAwait(false))
                                                await cboSkill2.DoThreadSafeAsync(x => x.SelectedIndex = 0, token)
                                                               .ConfigureAwait(false);
                                            else
                                            {
                                                int intSkill1Index
                                                    = await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                     .ConfigureAwait(false);
                                                await cboSkill2
                                                      .DoThreadSafeAsync(x => x.SelectedIndex = intSkill1Index + 1, token)
                                                      .ConfigureAwait(false);
                                            }
                                        }

                                        if (intSkillCount > 2)
                                        {
                                            int intOldSelectedIndex3 = await cboSkill3
                                                                             .DoThreadSafeFuncAsync(
                                                                                 x => x.SelectedIndex, token)
                                                                             .ConfigureAwait(false);
                                            int intOldDataSourceSize3 = await cboSkill3
                                                                              .DoThreadSafeFuncAsync(
                                                                                  x => x.Items.Count, token)
                                                                              .ConfigureAwait(false);
                                            await cboSkill3.PopulateWithListItemsAsync(lstSkills, token)
                                                           .ConfigureAwait(false);
                                            await cboSkill3.DoThreadSafeAsync(x =>
                                            {
                                                x.Visible = true;
                                                x.Enabled = lstSkills.Count > 3;
                                            }, token).ConfigureAwait(false);
                                            if (intOldDataSourceSize3 == lstSkills.Count)
                                            {
                                                Interlocked.Increment(ref _intLoading);
                                                try
                                                {
                                                    await cboSkill3
                                                          .DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex3, token)
                                                          .ConfigureAwait(false);
                                                }
                                                finally
                                                {
                                                    Interlocked.Decrement(ref _intLoading);
                                                }
                                            }

                                            if ((await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                .ConfigureAwait(false)
                                                 == await cboSkill1.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                   .ConfigureAwait(false) ||
                                                 await cboSkill3.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                .ConfigureAwait(false)
                                                 == await cboSkill2.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                   .ConfigureAwait(false)) &&
                                                !await ExoticSkill.IsExoticSkillNameAsync(
                                                    _objCharacter, await cboSkill3
                                                                         .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                                         .ConfigureAwait(false), token).ConfigureAwait(false))
                                            {
                                                int intNewIndex = await cboSkill3
                                                                        .DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                                                                        .ConfigureAwait(false);
                                                do
                                                {
                                                    ++intNewIndex;
                                                    if (intNewIndex >= lstSkills.Count)
                                                        intNewIndex = 0;
                                                } while ((intNewIndex == await cboSkill1
                                                                               .DoThreadSafeFuncAsync(
                                                                                   x => x.SelectedIndex, token)
                                                                               .ConfigureAwait(false)
                                                          || intNewIndex == await cboSkill2
                                                                                  .DoThreadSafeFuncAsync(
                                                                                      x => x.SelectedIndex, token)
                                                                                  .ConfigureAwait(false))
                                                         && intNewIndex != await cboSkill3
                                                                                 .DoThreadSafeFuncAsync(
                                                                                     x => x.SelectedIndex, token)
                                                                                 .ConfigureAwait(false)
                                                         && !await ExoticSkill.IsExoticSkillNameAsync(
                                                             _objCharacter, lstSkills[intNewIndex].Value.ToString(), token).ConfigureAwait(false));

                                                await cboSkill3.DoThreadSafeAsync(x => x.SelectedIndex = intNewIndex, token)
                                                               .ConfigureAwait(false);
                                            }
                                        }
                                    }

                                    string strMetamagicSkillSelection = string.Format(
                                        GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("String_MetamagicSkillBase", token: token)
                                                             .ConfigureAwait(false),
                                        await LanguageManager.GetStringAsync("String_MetamagicSkills", token: token)
                                                             .ConfigureAwait(false));
                                    // strSkillType can have the following values: magic, resonance, matrix, active, specific, grouped
                                    // So the language file should contain each of those like String_MetamagicSkillType_magic
                                    string strSkillVal
                                        = xmlTalentNode.SelectSingleNodeAndCacheExpression("skillval", token)?.Value
                                          ?? xmlTalentNode
                                              .SelectSingleNodeAndCacheExpression("skillgroupval", token)
                                          ?.Value;
                                    string strMetamagicSkillType = await LanguageManager.GetStringAsync(
                                        "String_MetamagicSkillType_" + strSkillType, token: token).ConfigureAwait(false);
                                    await lblMetatypeSkillSelection.DoThreadSafeAsync(x =>
                                    {
                                        x.Text = string.Format(GlobalSettings.CultureInfo, strMetamagicSkillSelection,
                                                               strSkillCount, strMetamagicSkillType, strSkillVal);
                                        x.Visible = true;
                                    }, token).ConfigureAwait(false);
                                }
                            }

                            int intSpecialAttribPoints = 0;

                            string strSelectedMetatype = await lstMetatypes
                                                               .DoThreadSafeFuncAsync(
                                                                   x => x.SelectedValue?.ToString(), token)
                                                               .ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(strSelectedMetatype))
                            {
                                string strSelectedMetavariant = await cboMetavariant
                                                                      .DoThreadSafeFuncAsync(
                                                                          x => x.SelectedValue?.ToString(), token)
                                                                      .ConfigureAwait(false);
                                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                                    "priorities/priority[category = \"Heritage\" and value = "
                                    + (await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                        .ConfigureAwait(false) ?? string.Empty).CleanXPath()
                                    + " and (not(prioritytable) or prioritytable = "
                                    + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                                {
                                    if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority
                                            .SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                                    {
                                        XPathNavigator objXmlMetatypePriorityNode
                                            = xmlBaseMetatypePriority.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                                        if (!string.IsNullOrEmpty(strSelectedMetavariant)
                                            && strSelectedMetavariant != Guid.Empty.ToString("D"))
                                            objXmlMetatypePriorityNode
                                                = objXmlMetatypePriorityNode?.TryGetNodeByNameOrId("metavariants/metavariant", strSelectedMetavariant);
                                        if (objXmlMetatypePriorityNode != null && int.TryParse(
                                                objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression(
                                                    "value", token)?.Value, out int intTemp))
                                            intSpecialAttribPoints += intTemp;
                                        break;
                                    }
                                }
                            }

                            if (int.TryParse(
                                    xmlTalentNode
                                        .SelectSingleNodeAndCacheExpression("specialattribpoints", token)?.Value, out int intTalentSpecialAttribPoints))
                                intSpecialAttribPoints += intTalentSpecialAttribPoints;

                            await lblSpecialAttributes
                                  .DoThreadSafeAsync(
                                      x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token)
                                  .ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await cboTalents.DoThreadSafeAsync(x => x.SelectedIndex = 0, token).ConfigureAwait(false);
                }

                if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                {
                    await SumToTen(true, token).ConfigureAwait(false);
                }
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
                        try
                        {
                            await RefreshSelectedMetatype(_objGenericToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }
                        try
                        {
                            await PopulateTalents(_objGenericToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }
                        if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                        {
                            await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                        }
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

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
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
                        try
                        {
                            await PopulateMetatypes(_objGenericToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }
                        if (_objCharacter.EffectiveBuildMethod == CharacterBuildMethod.SumtoTen)
                        {
                            await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                        }
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

        private async void cboHeritage_SelectedIndexChanged(object sender, EventArgs e)
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
                        try
                        {
                            switch (_objCharacter.EffectiveBuildMethod)
                            {
                                case CharacterBuildMethod.Priority:
                                    await ManagePriorityItems(cboHeritage, token: _objGenericToken).ConfigureAwait(false);
                                    break;

                                case CharacterBuildMethod.SumtoTen:
                                    await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                                    break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }

                        try
                        {
                            await LoadMetatypes(_objGenericToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }
                        try
                        {
                            await PopulateMetatypes(_objGenericToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }
                        try
                        {
                            await PopulateMetavariants(_objGenericToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }
                        await RefreshSelectedMetatype(_objGenericToken).ConfigureAwait(false);
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

        private async void cboTalent_SelectedIndexChanged(object sender, EventArgs e)
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
                        try
                        {
                            switch (_objCharacter.EffectiveBuildMethod)
                            {
                                case CharacterBuildMethod.Priority:
                                    await ManagePriorityItems(cboTalent, token: _objGenericToken).ConfigureAwait(false);
                                    break;

                                case CharacterBuildMethod.SumtoTen:
                                    await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                                    break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this and move on in case this was canceled internally because of a duplicate call
                        }

                        await PopulateTalents(token: _objGenericToken).ConfigureAwait(false);
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

        private async void cboAttributes_SelectedIndexChanged(object sender, EventArgs e)
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
                        switch (_objCharacter.EffectiveBuildMethod)
                        {
                            case CharacterBuildMethod.Priority:
                                await ManagePriorityItems(cboAttributes, token: _objGenericToken).ConfigureAwait(false);
                                break;

                            case CharacterBuildMethod.SumtoTen:
                                await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                                break;
                        }
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

        private async void cboSkills_SelectedIndexChanged(object sender, EventArgs e)
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
                        switch (_objCharacter.EffectiveBuildMethod)
                        {
                            case CharacterBuildMethod.Priority:
                                await ManagePriorityItems(cboSkills, token: _objGenericToken).ConfigureAwait(false);
                                break;

                            case CharacterBuildMethod.SumtoTen:
                                await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                                break;
                        }
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

        private async void cboResources_SelectedIndexChanged(object sender, EventArgs e)
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
                        switch (_objCharacter.EffectiveBuildMethod)
                        {
                            case CharacterBuildMethod.Priority:
                                await ManagePriorityItems(cboResources, token: _objGenericToken).ConfigureAwait(false);
                                break;

                            case CharacterBuildMethod.SumtoTen:
                                await SumToTen(token: _objGenericToken).ConfigureAwait(false);
                                break;
                        }
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
            System.IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _objCharacter.GetEffectiveBuildMethodAsync(token).ConfigureAwait(false) == CharacterBuildMethod.SumtoTen)
                {
                    int intSumToTen = await SumToTen(false, token).ConfigureAwait(false);
                    if (intSumToTen != _objCharacter.Settings.SumtoTen)
                    {
                        Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync(
                                "Message_SumtoTen", token: token).ConfigureAwait(false),
                            _objCharacter.Settings.SumtoTen.ToString(
                                GlobalSettings.CultureInfo),
                            intSumToTen.ToString(GlobalSettings.CultureInfo)));
                        return;
                    }
                }

                if (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false) == -1)
                {
                    Program.ShowScrollableMessageBox(
                        this,
                        await LanguageManager.GetStringAsync("Message_Metatype_SelectTalent", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectTalent", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string strSkill1
                    = await cboSkill1.DoThreadSafeFuncAsync(x => x.Visible ? x.SelectedValue?.ToString() : string.Empty,
                        token).ConfigureAwait(false);
                string strSkill2
                    = await cboSkill2.DoThreadSafeFuncAsync(x => x.Visible ? x.SelectedValue?.ToString() : string.Empty,
                        token).ConfigureAwait(false);
                string strSkill3
                    = await cboSkill3.DoThreadSafeFuncAsync(x => x.Visible ? x.SelectedValue?.ToString() : string.Empty,
                        token).ConfigureAwait(false);

                if ((await cboSkill1.DoThreadSafeFuncAsync(x => x.Visible, token).ConfigureAwait(false) &&
                     string.IsNullOrEmpty(strSkill1))
                    || (await cboSkill2.DoThreadSafeFuncAsync(x => x.Visible, token).ConfigureAwait(false) &&
                        string.IsNullOrEmpty(strSkill2))
                    || (await cboSkill3.DoThreadSafeFuncAsync(x => x.Visible, token).ConfigureAwait(false)
                        && string.IsNullOrEmpty(strSkill3)))
                {
                    Program.ShowScrollableMessageBox(
                        this,
                        await LanguageManager.GetStringAsync("Message_Metatype_SelectSkill", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectSkill", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSkill1, token).ConfigureAwait(false))
                {
                    using (ThreadSafeForm<SelectExoticSkill> frmSelectExotic =
                           await ThreadSafeForm<SelectExoticSkill>.GetAsync(() => new SelectExoticSkill(_objCharacter),
                               token).ConfigureAwait(false))
                    {
                        frmSelectExotic.MyForm.ForceSkill(strSkill1);
                        if (await frmSelectExotic.ShowDialogSafeAsync(this, token).ConfigureAwait(false) !=
                            DialogResult.OK)
                            return;
                        strSkill1 += " (" + frmSelectExotic.MyForm.SelectedExoticSkillSpecialisation + ')';
                    }
                }

                if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSkill2, token).ConfigureAwait(false))
                {
                    using (ThreadSafeForm<SelectExoticSkill> frmSelectExotic =
                           await ThreadSafeForm<SelectExoticSkill>.GetAsync(() => new SelectExoticSkill(_objCharacter),
                               token).ConfigureAwait(false))
                    {
                        frmSelectExotic.MyForm.ForceSkill(strSkill2);
                        if (await frmSelectExotic.ShowDialogSafeAsync(this, token).ConfigureAwait(false) !=
                            DialogResult.OK)
                            return;
                        strSkill2 += " (" + frmSelectExotic.MyForm.SelectedExoticSkillSpecialisation + ')';
                    }
                }

                if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSkill3, token).ConfigureAwait(false))
                {
                    using (ThreadSafeForm<SelectExoticSkill> frmSelectExotic =
                           await ThreadSafeForm<SelectExoticSkill>.GetAsync(() => new SelectExoticSkill(_objCharacter),
                               token).ConfigureAwait(false))
                    {
                        frmSelectExotic.MyForm.ForceSkill(strSkill3);
                        if (await frmSelectExotic.ShowDialogSafeAsync(this, token).ConfigureAwait(false) !=
                            DialogResult.OK)
                            return;
                        strSkill3 += " (" + frmSelectExotic.MyForm.SelectedExoticSkillSpecialisation + ')';
                    }
                }

                if ((!string.IsNullOrEmpty(strSkill1) && (strSkill1 == strSkill2 || strSkill1 == strSkill3))
                    || (!string.IsNullOrEmpty(strSkill2) && strSkill2 == strSkill3))
                {
                    Program.ShowScrollableMessageBox(
                        this,
                        await LanguageManager.GetStringAsync("Message_Metatype_Duplicate", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_Duplicate", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string strSelectedMetatype
                    = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                        .ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedMetatype))
                {
                    Program.ShowScrollableMessageBox(
                        this,
                        await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                XmlNode objXmlMetatype
                    = _xmlMetatypeDocumentMetatypesNode.TryGetNodeByNameOrId("metatype", strSelectedMetatype);
                if (objXmlMetatype == null)
                {
                    Program.ShowScrollableMessageBox(
                        this,
                        await LanguageManager.GetStringAsync("Message_Metatype_SelectMetatype", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_Metatype_SelectMetatype", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                strSelectedMetatype = objXmlMetatype["id"]?.InnerText ?? Guid.Empty.ToString("D");

                System.IAsyncDisposable objLocker2
                    = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Clear out all priority-only qualities that the character bought normally (relevant when switching from Karma to Priority/Sum-to-Ten)
                    for (int i = await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false) - 1;
                         i >= 0;
                         --i)
                    {
                        if (i >= await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false))
                            continue;
                        Quality objQuality = await _objCharacter.Qualities.GetValueAtAsync(i, token)
                            .ConfigureAwait(false);
                        if (objQuality.OriginSource == QualitySource.Selected
                            && (await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false))
                            ?.SelectSingleNode(
                                "onlyprioritygiven")
                            != null)
                            await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                    }

                    string strSelectedMetavariant
                        = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    string strSelectedMetatypeCategory
                        = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                            .ConfigureAwait(false);

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter" &&
                        strSelectedMetavariant == Guid.Empty.ToString("D"))
                        strSelectedMetavariant = "Human";
                    XmlNode objXmlMetavariant
                        = objXmlMetatype.TryGetNodeByNameOrId("metavariants/metavariant", strSelectedMetavariant);
                    strSelectedMetavariant = objXmlMetavariant?["id"]?.InnerText ?? Guid.Empty.ToString();
                    int intForce = await nudForce.DoThreadSafeFuncAsync(x => x.Visible ? x.ValueAsInt : 0, token)
                        .ConfigureAwait(false);

                    if (_objCharacter.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetatype
                        || (await _objCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false)).ToString("D", GlobalSettings.InvariantCultureInfo) !=
                        strSelectedMetavariant)
                    {
                        // Remove qualities that require the old metatype
                        List<Quality> lstQualitiesToCheck =
                            new List<Quality>(await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false));
                        await _objCharacter.Qualities.ForEachAsync(async objQuality =>
                        {
                            QualitySource eSource = await objQuality.GetOriginSourceAsync(token).ConfigureAwait(false);
                            if (eSource == QualitySource.Improvement
                                || eSource == QualitySource.Heritage
                                || eSource == QualitySource.Metatype
                                || eSource == QualitySource.MetatypeRemovable
                                || eSource == QualitySource.MetatypeRemovedAtChargen)
                                return;
                            XPathNavigator xmlBaseNode
                                = await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                            XPathNavigator xmlRestrictionNode
                                = xmlBaseNode?.SelectSingleNodeAndCacheExpression("required", token);
                            if (xmlRestrictionNode != null &&
                                (xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metatype", token) != null
                                 || xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metavariant", token) !=
                                 null))
                            {
                                lstQualitiesToCheck.Add(objQuality);
                            }
                            else
                            {
                                xmlRestrictionNode
                                    = xmlBaseNode?.SelectSingleNodeAndCacheExpression("forbidden", token);
                                if (xmlRestrictionNode != null &&
                                    (xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metatype", token) != null
                                     || xmlRestrictionNode.SelectSingleNodeAndCacheExpression(".//metavariant",
                                         token) != null))
                                {
                                    lstQualitiesToCheck.Add(objQuality);
                                }
                            }
                        }, token).ConfigureAwait(false);

                        await _objCharacter.CreateAsync(strSelectedMetatypeCategory, strSelectedMetatype,
                            strSelectedMetavariant, objXmlMetatype, intForce,
                            _xmlQualityDocumentQualitiesNode,
                            await _xmlCritterPowerDocumentPowersNode.GetValueAsync(token).ConfigureAwait(false),
                            null,
                            await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Checked, token)
                                .ConfigureAwait(false)
                                ? await cboPossessionMethod.DoThreadSafeFuncAsync(
                                    x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                                : string.Empty, token).ConfigureAwait(false);
                        foreach (Quality objQuality in lstQualitiesToCheck)
                        {
                            XPathNavigator objLoopNode
                                = await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                            // Set strIgnoreQuality to quality's name to make sure limit counts are not an issue
                            if (objLoopNode != null && !await objLoopNode
                                    .RequirementsMetAsync(
                                        _objCharacter, strIgnoreQuality: await objQuality.GetNameAsync(token).ConfigureAwait(false),
                                        token: token).ConfigureAwait(false))
                                await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                        }
                    }

                    string strOldSpecialPriority = _objCharacter.SpecialPriority;
                    string strOldTalentPriority = _objCharacter.TalentPriority;

                    // begin priority based character settings
                    // Load the Priority information.

                    // Set the character priority selections
                    _objCharacter.MetatypePriority
                        = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    _objCharacter.AttributesPriority
                        = await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    _objCharacter.SpecialPriority
                        = await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    _objCharacter.SkillsPriority
                        = await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    _objCharacter.ResourcesPriority
                        = await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    _objCharacter.TalentPriority
                        = await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                            .ConfigureAwait(false);
                    await _objCharacter.PriorityBonusSkillList.ClearAsync(token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSkill1))
                        await _objCharacter.PriorityBonusSkillList.AddAsync(strSkill1, token: token)
                            .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSkill2))
                        await _objCharacter.PriorityBonusSkillList.AddAsync(strSkill2, token: token)
                            .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSkill3))
                        await _objCharacter.PriorityBonusSkillList.AddAsync(strSkill3, token: token)
                            .ConfigureAwait(false);

                    // Set starting nuyen
                    XPathNodeIterator xmlResourcesPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Resources\" and value = "
                        + _objCharacter.ResourcesPriority.CleanXPath() +
                        " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath()
                        + ")]");
                    foreach (XPathNavigator xmlResourcesPriority in xmlResourcesPriorityList)
                    {
                        if (xmlResourcesPriorityList.Count == 1 ||
                            (xmlResourcesPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null
                             && xmlResourcesPriority.SelectSingleNodeAndCacheExpression("gameplayoption", token) != null))
                        {
                            decimal decResources = 0;
                            if (xmlResourcesPriority.TryGetDecFieldQuickly("resources", ref decResources))
                            {
                                await _objCharacter.SetNuyenAsync(decResources, token).ConfigureAwait(false);
                                await _objCharacter.SetStartingNuyenAsync(decResources, token).ConfigureAwait(false);
                            }

                            break;
                        }
                    }

                    XmlNode charNode =
                        strSelectedMetatypeCategory == "Shapeshifter"
                        || strSelectedMetavariant == Guid.Empty.ToString("D")
                            ? objXmlMetatype
                            : objXmlMetavariant ?? objXmlMetatype;

                    int intSpecialAttribPoints = 0;
                    bool boolHalveAttributePriorityPoints = charNode.NodeExists("halveattributepoints");
                    if (strOldSpecialPriority != _objCharacter.SpecialPriority
                        || strOldTalentPriority != _objCharacter.TalentPriority)
                    {
                        List<Quality> lstOldPriorityQualities
                            = await (await _objCharacter.GetQualitiesAsync(token).ConfigureAwait(false))
                                .ToListAsync(
                                    x => x.OriginSource == QualitySource.Heritage, token: token)
                                .ConfigureAwait(false);
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        bool blnRemoveFreeSkills = true;
                        XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                            "priorities/priority[category = \"Talent\" and value = "
                            + _objCharacter.SpecialPriority.CleanXPath() +
                            " and (not(prioritytable) or prioritytable = "
                            + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                        string strSkill1XPath = strSkill1.CleanXPath();
                        string strSkill2XPath = strSkill2.CleanXPath();
                        string strSkill3XPath = strSkill3.CleanXPath();
                        foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                        {
                            if (xmlBaseTalentPriorityList.Count == 1
                                || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("gameplayoption", token) != null)
                            {
                                XPathNavigator xmlTalentPriorityNode
                                    = xmlBaseTalentPriority.SelectSingleNode(
                                        "talents/talent[value = " + _objCharacter.TalentPriority.CleanXPath()
                                                                  + ']');

                                if (xmlTalentPriorityNode != null)
                                {
                                    string strUnlockSkillsFilter = string.Empty;
                                    if (!string.IsNullOrEmpty(strSkill1)
                                        || !string.IsNullOrEmpty(strSkill2)
                                        || !string.IsNullOrEmpty(strSkill3))
                                    {
                                        strUnlockSkillsFilter
                                            = "bonus/unlockskills[contains(text(), ',') and (contains(text(), "
                                              + strSkill1XPath;
                                        if (!string.IsNullOrEmpty(strSkill2))
                                            strUnlockSkillsFilter += ") or contains(text(), " + strSkill2XPath;
                                        if (!string.IsNullOrEmpty(strSkill3))
                                            strUnlockSkillsFilter += ") or contains(text(), " + strSkill3XPath;
                                        strUnlockSkillsFilter += "))]";
                                    }

                                    // Create the Qualities that come with the Talent.
                                    foreach (XPathNavigator objXmlQualityItem in xmlTalentPriorityNode
                                                 .SelectAndCacheExpression("qualities/quality", token))
                                    {
                                        XmlNode objXmlQuality
                                            = _xmlQualityDocumentQualitiesNode.TryGetNodeByNameOrId(
                                                "quality", objXmlQualityItem.Value);
                                        Quality objQuality = new Quality(_objCharacter);
                                        bool blnDoRemove = false;
                                        Quality objExistingQuality;
                                        try
                                        {
                                            string strForceValue
                                                = objXmlQualityItem.SelectSingleNodeAndCacheExpression(
                                                        "@select", token)
                                                    ?.Value ?? string.Empty;
                                            if (string.IsNullOrEmpty(strForceValue) &&
                                                !string.IsNullOrEmpty(strUnlockSkillsFilter))
                                            {
                                                // If we have selected a priority entry where we get bonus ranks to a skill or skill group that isn't necessarily given to us,
                                                // then make sure that that skill is what is given to us in the end. Mainly relevant for aspected magicians.
                                                XmlNodeList xmlRelevantUnlocksNodesList =
                                                    objXmlQuality.SelectNodes(strUnlockSkillsFilter);
                                                if (xmlRelevantUnlocksNodesList.Count > 0)
                                                {
                                                    List<string> lstToPush
                                                        = new List<string>(xmlRelevantUnlocksNodesList.Count);
                                                    foreach (XmlNode xmlLoopNode in xmlRelevantUnlocksNodesList)
                                                    {
                                                        string[] astrOptions = xmlLoopNode.InnerText.Split(',');
                                                        if (!string.IsNullOrEmpty(strSkill1) &&
                                                            astrOptions.Contains(strSkill1))
                                                            lstToPush.Add(strSkill1);
                                                        else if (!string.IsNullOrEmpty(strSkill2) &&
                                                                 astrOptions.Contains(strSkill2))
                                                            lstToPush.Add(strSkill2);
                                                        else if (!string.IsNullOrEmpty(strSkill3) &&
                                                                 astrOptions.Contains(strSkill3))
                                                            lstToPush.Add(strSkill3);
                                                    }

                                                    // Reverse order because we process bonus nodes from top to bottom, and this text will be saved in a FILO stack
                                                    lstToPush.Reverse();
                                                    int intNumPushed = 0;
                                                    try
                                                    {
                                                        foreach (string strToPush in lstToPush)
                                                        {
                                                            token.ThrowIfCancellationRequested();
                                                            _objCharacter.PushText.Push(strToPush);
                                                            ++intNumPushed;
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        for (int i = 0; i < intNumPushed; ++i)
                                                        {
                                                            if (!_objCharacter.PushText.TryPop(out _))
                                                                break;
                                                        }

                                                        throw;
                                                    }
                                                }
                                            }

                                            await objQuality.CreateAsync(objXmlQuality, QualitySource.Heritage, lstWeapons,
                                                strForceValue, token: token).ConfigureAwait(false);
                                            // TODO: Do something if we are prompted to select something for a quality and the user cancels out.
                                            // The naive thing would be to just return, but we've already changed too much about the character (and had to in order to allow this quality
                                            // selection's requirements checking to work properly) to undo it at this point.
                                            // The safe thing would probably be to operate on a duplicate of the character and copy over all data after we know it's final, but that needs
                                            // a lot of backend stuff.
                                            QualityType eQualityType = await objQuality.GetTypeAsync(token).ConfigureAwait(false);
                                            objExistingQuality = await lstOldPriorityQualities.FirstOrDefaultAsync(
                                                async x => x.SourceID == objQuality.SourceID
                                                           && x.Extra == objQuality.Extra &&
                                                           await x.GetTypeAsync(token).ConfigureAwait(false) ==
                                                           eQualityType, token: token).ConfigureAwait(false);
                                            if (objExistingQuality == null)
                                                await _objCharacter.Qualities.AddAsync(objQuality, token: token)
                                                    .ConfigureAwait(false);
                                            else
                                            {
                                                blnDoRemove = true;
                                            }
                                        }
                                        catch
                                        {
                                            await objQuality.DisposeAsync().ConfigureAwait(false);
                                            throw;
                                        }

                                        if (blnDoRemove)
                                        {
                                            lstOldPriorityQualities.Remove(objExistingQuality);
                                            await objQuality.DeleteQualityAsync(token: token)
                                                .ConfigureAwait(false);
                                        }
                                    }

                                    foreach (Quality objQuality in lstOldPriorityQualities)
                                    {
                                        await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                                    }

                                    // Set starting magic
                                    int intTemp = 1;
                                    int intMax = 0;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("magic", ref intTemp))
                                        intTemp = 1;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxmagic", ref intMax))
                                        intMax = Math.Max(await CommonFunctions.ExpressionToIntAsync(
                                                charNode["magmax"]?.InnerText, intForce,
                                                token: token).ConfigureAwait(false),
                                            intTemp);
                                    await _objCharacter.MAG.AssignLimitsAsync(intTemp, intMax, intMax, token)
                                        .ConfigureAwait(false);
                                    _objCharacter.FreeSpells
                                        = xmlTalentPriorityNode.TryGetInt32FieldQuickly("spells", ref intTemp)
                                            ? intTemp
                                            : 0;
                                    // Set starting resonance
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("resonance", ref intTemp))
                                        intTemp = 1;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxresonance", ref intMax))
                                        intMax = Math.Max(await CommonFunctions.ExpressionToIntAsync(
                                                charNode["resmax"]?.InnerText, intForce,
                                                token: token).ConfigureAwait(false),
                                            intTemp);
                                    await _objCharacter.RES.AssignLimitsAsync(intTemp, intMax, intMax, token)
                                        .ConfigureAwait(false);
                                    _objCharacter.CFPLimit
                                        = xmlTalentPriorityNode.TryGetInt32FieldQuickly("cfp", ref intTemp)
                                            ? intTemp
                                            : 0;
                                    // Set starting depth
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("depth", ref intTemp))
                                        intTemp = 1;
                                    if (!xmlTalentPriorityNode.TryGetInt32FieldQuickly("maxdepth", ref intMax))
                                        intMax = Math.Max(await CommonFunctions.ExpressionToIntAsync(
                                                charNode["depmax"]?.InnerText, intForce,
                                                token: token).ConfigureAwait(false),
                                            intTemp);
                                    await _objCharacter.DEP.AssignLimitsAsync(intTemp, intMax, intMax, token)
                                        .ConfigureAwait(false);
                                    _objCharacter.AINormalProgramLimit
                                        = xmlTalentPriorityNode.TryGetInt32FieldQuickly(
                                            "ainormalprogramlimit", ref intTemp)
                                            ? intTemp
                                            : 0;
                                    _objCharacter.AIAdvancedProgramLimit
                                        = xmlTalentPriorityNode.TryGetInt32FieldQuickly(
                                            "aiadvancedprogramlimit", ref intTemp)
                                            ? intTemp
                                            : 0;

                                    // Set Free Skills/Skill Groups
                                    int intFreeLevels = 0;
                                    Improvement.ImprovementType eType = Improvement.ImprovementType.SkillBase;
                                    XPathNavigator objTalentSkillValNode
                                        = xmlTalentPriorityNode.SelectSingleNodeAndCacheExpression("skillval", token);
                                    if (objTalentSkillValNode == null
                                        || !int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                    {
                                        objTalentSkillValNode
                                            = xmlTalentPriorityNode.SelectSingleNodeAndCacheExpression("skillgroupval", token);
                                        if (objTalentSkillValNode != null
                                            && int.TryParse(objTalentSkillValNode.Value, out intFreeLevels))
                                        {
                                            eType = Improvement.ImprovementType.SkillGroupBase;
                                        }
                                    }

                                    blnRemoveFreeSkills = false;
                                    await AddFreeSkills(intFreeLevels, eType, strSkill1, strSkill2, strSkill3,
                                        token).ConfigureAwait(false);

                                    if (int.TryParse(
                                            xmlTalentPriorityNode
                                                .SelectSingleNodeAndCacheExpression("specialattribpoints", token)
                                                ?.Value, out int intTalentSpecialAttribPoints))
                                        intSpecialAttribPoints += intTalentSpecialAttribPoints;
                                }

                                break;
                            }
                        }

                        if (blnRemoveFreeSkills)
                            await ImprovementManager.RemoveImprovementsAsync(
                                _objCharacter,
                                await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false))
                                    .ToListAsync(
                                        x => x.ImproveSource == Improvement.ImprovementSource.Heritage
                                             && (x.ImproveType == Improvement.ImprovementType.SkillBase
                                                 || x.ImproveType == Improvement.ImprovementType.SkillGroupBase),
                                        token).ConfigureAwait(false),
                                token: token).ConfigureAwait(false);
                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in lstWeapons)
                            await _objCharacter.Weapons.AddAsync(objWeapon, token: token).ConfigureAwait(false);
                    }

                    // Set Special Attributes and Karma

                    int intMetatypeBP = 0;
                    XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Heritage\" and value = "
                        + _objCharacter.MetatypePriority.CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                    {
                        if (xmlBaseMetatypePriorityList.Count == 1
                            || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                        {
                            XPathNavigator objXmlMetatypePriorityNode
                                = xmlBaseMetatypePriority.TryGetNodeByNameOrId(
                                      "metatypes/metatype", strSelectedMetatype)
                                  ?? xmlBaseMetatypePriority.TryGetNodeByNameOrId(
                                      "metatypes/metatype", await _objCharacter.GetMetatypeAsync(token).ConfigureAwait(false));
                            if (strSelectedMetavariant != Guid.Empty.ToString("D"))
                                objXmlMetatypePriorityNode
                                    = objXmlMetatypePriorityNode?.TryGetNodeByNameOrId(
                                          "metavariants/metavariant", strSelectedMetavariant)
                                      ?? objXmlMetatypePriorityNode?.TryGetNodeByNameOrId(
                                          "metavariants/metavariant", await _objCharacter.GetMetavariantAsync(token).ConfigureAwait(false));
                            if (objXmlMetatypePriorityNode != null)
                            {
                                if (int.TryParse(objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression("value", token)?.Value, out int intTemp))
                                    intSpecialAttribPoints += intTemp;
                                if (int.TryParse(objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression("karma", token)?.Value, out int intTemp2))
                                    intMetatypeBP += intTemp2;
                            }

                            break;
                        }
                    }

                    _objCharacter.Special = intSpecialAttribPoints;
                    _objCharacter.TotalSpecial = _objCharacter.Special;
                    _objCharacter.MetatypeBP = intMetatypeBP;

                    // Set Attributes
                    XPathNodeIterator objXmlAttributesPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Attributes\" and value = "
                        + _objCharacter.AttributesPriority.CleanXPath() +
                        " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath()
                        + ")]");
                    foreach (XPathNavigator objXmlAttributesPriority in objXmlAttributesPriorityList)
                    {
                        if (objXmlAttributesPriorityList.Count == 1 ||
                            (objXmlAttributesPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null
                             && objXmlAttributesPriority.SelectSingleNodeAndCacheExpression("gameplayoption", token) != null))
                        {
                            int intAttributes = 0;
                            objXmlAttributesPriority.TryGetInt32FieldQuickly("attributes", ref intAttributes);
                            if (boolHalveAttributePriorityPoints)
                                intAttributes /= 2;
                            _objCharacter.TotalAttributes = _objCharacter.Attributes = intAttributes;
                            break;
                        }
                    }

                    // Set Skills and Skill Groups
                    XPathNodeIterator objXmlSkillsPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Skills\" and value = "
                        + _objCharacter.SkillsPriority.CleanXPath()
                        +
                        " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable
                            .CleanXPath() + ")]");
                    foreach (XPathNavigator objXmlSkillsPriority in objXmlSkillsPriorityList)
                    {
                        if (objXmlSkillsPriorityList.Count == 1 ||
                            (objXmlSkillsPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null
                             && objXmlSkillsPriority.SelectSingleNodeAndCacheExpression("gameplayoption", token) != null))
                        {
                            int intTemp = 0;
                            if (objXmlSkillsPriority.TryGetInt32FieldQuickly("skills", ref intTemp))
                                _objCharacter.SkillsSection.SkillPointsMaximum = intTemp;
                            if (objXmlSkillsPriority.TryGetInt32FieldQuickly("skillgroups", ref intTemp))
                                _objCharacter.SkillsSection.SkillGroupPointsMaximum = intTemp;
                            break;
                        }
                    }

                    // Sprites can never have Physical Attributes
                    if (await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false)
                        || _objCharacter.Metatype.EndsWith("Sprite", StringComparison.Ordinal))
                    {
                        await _objCharacter.BOD.AssignBaseKarmaLimitsAsync(0, 0, 0, 0, 0, token)
                            .ConfigureAwait(false);
                        await _objCharacter.AGI.AssignBaseKarmaLimitsAsync(0, 0, 0, 0, 0, token)
                            .ConfigureAwait(false);
                        await _objCharacter.REA.AssignBaseKarmaLimitsAsync(0, 0, 0, 0, 0, token)
                            .ConfigureAwait(false);
                        await _objCharacter.STR.AssignBaseKarmaLimitsAsync(0, 0, 0, 0, 0, token)
                            .ConfigureAwait(false);
                        await _objCharacter.MAG.AssignBaseKarmaLimitsAsync(0, 0, 0, 0, 0, token)
                            .ConfigureAwait(false);
                        await _objCharacter.MAGAdept.AssignBaseKarmaLimitsAsync(0, 0, 0, 0, 0, token)
                            .ConfigureAwait(false);
                    }

                    // Set free contact points
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.ContactPoints), token).ConfigureAwait(false);

                    // If we suspect the character converted from Karma to Priority/Sum-to-Ten, try to convert their Attributes, Skills, and Skill Groups to using points as efficiently as possible
                    bool blnDoSwitch = false;
                    await _objCharacter.AttributeSection.AttributeList.ForEachWithBreakAsync(async objAttribute =>
                    {
                        if (await objAttribute.GetBaseAsync(token).ConfigureAwait(false) > 0)
                        {
                            blnDoSwitch = false;
                            return false;
                        }

                        if (await objAttribute.GetKarmaAsync(token).ConfigureAwait(false) > 0)
                            blnDoSwitch = true;
                        return true;
                    }, token).ConfigureAwait(false);

                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.TotalAttributes)
                        {
                            CharacterAttrib objAttributeToShift = null;
                            await _objCharacter.AttributeSection.AttributeList.ForEachWithSideEffectsAsync(async objAttribute =>
                            {
                                if (await objAttribute.GetKarmaAsync(token).ConfigureAwait(false) > 0
                                    && (objAttributeToShift == null
                                        || await objAttributeToShift.GetValueAsync(token).ConfigureAwait(false) < await objAttribute.GetValueAsync(token).ConfigureAwait(false)))
                                {
                                    objAttributeToShift = objAttribute;
                                }
                            }, token).ConfigureAwait(false);

                            if (objAttributeToShift == null)
                                break;
                            int intKarma = Math.Min(await objAttributeToShift.GetKarmaAsync(token).ConfigureAwait(false),
                                _objCharacter.TotalAttributes - intPointsSpent);
                            await objAttributeToShift.ModifyKarmaAsync(-intKarma, token).ConfigureAwait(false);
                            await objAttributeToShift.ModifyBaseAsync(intKarma, token).ConfigureAwait(false);
                            intPointsSpent += intKarma;
                        }
                    }

                    blnDoSwitch = false;
                    await _objCharacter.AttributeSection.SpecialAttributeList.ForEachWithBreakAsync(async objAttribute =>
                    {
                        if (await objAttribute.GetBaseAsync(token).ConfigureAwait(false) > 0)
                        {
                            blnDoSwitch = false;
                            return false;
                        }

                        if (await objAttribute.GetKarmaAsync(token).ConfigureAwait(false) > 0)
                            blnDoSwitch = true;
                        return true;
                    }, token).ConfigureAwait(false);

                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.TotalSpecial)
                        {
                            CharacterAttrib objAttributeToShift = null;
                            await _objCharacter.AttributeSection.SpecialAttributeList.ForEachWithSideEffectsAsync(async objAttribute =>
                            {
                                if (await objAttribute.GetKarmaAsync(token).ConfigureAwait(false) > 0
                                    && (objAttributeToShift == null
                                        || await objAttributeToShift.GetValueAsync(token).ConfigureAwait(false) < await objAttribute.GetValueAsync(token).ConfigureAwait(false)))
                                {
                                    objAttributeToShift = objAttribute;
                                }
                            }, token).ConfigureAwait(false);

                            if (objAttributeToShift == null)
                                break;
                            int intKarma = Math.Min(await objAttributeToShift.GetKarmaAsync(token).ConfigureAwait(false),
                                _objCharacter.TotalAttributes - intPointsSpent);
                            await objAttributeToShift.ModifyKarmaAsync(-intKarma, token).ConfigureAwait(false);
                            await objAttributeToShift.ModifyBaseAsync(intKarma, token).ConfigureAwait(false);
                            intPointsSpent += intKarma;
                        }
                    }

                    blnDoSwitch = false;
                    await (await _objCharacter.SkillsSection.GetSkillGroupsAsync(token).ConfigureAwait(false)).ForEachWithBreakAsync(async objGroup =>
                    {
                        if (await objGroup.GetBaseAsync(token).ConfigureAwait(false) > 0)
                        {
                            blnDoSwitch = false;
                            return false;
                        }

                        if (await objGroup.GetKarmaAsync(token).ConfigureAwait(false) > 0)
                            blnDoSwitch = true;
                        return true;
                    }, token).ConfigureAwait(false);

                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        while (intPointsSpent < _objCharacter.SkillsSection.SkillGroupPointsMaximum)
                        {
                            SkillGroup objGroupToShift = null;
                            await (await _objCharacter.SkillsSection.GetSkillGroupsAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objGroup =>
                            {
                                if (await objGroup.GetKarmaAsync(token).ConfigureAwait(false) > 0
                                    && (objGroupToShift == null || await objGroupToShift.GetRatingAsync(token).ConfigureAwait(false) < await objGroup.GetRatingAsync(token).ConfigureAwait(false)))
                                {
                                    objGroupToShift = objGroup;
                                }
                            }, token).ConfigureAwait(false);

                            if (objGroupToShift == null)
                                break;
                            int intKarma = Math.Min(await objGroupToShift.GetKarmaAsync(token).ConfigureAwait(false),
                                _objCharacter.SkillsSection.SkillGroupPointsMaximum
                                - intPointsSpent);
                            await objGroupToShift.ModifyKarmaAsync(-intKarma, token).ConfigureAwait(false);
                            await objGroupToShift.ModifyBaseAsync(intKarma, token).ConfigureAwait(false);
                            intPointsSpent += intKarma;
                        }
                    }

                    blnDoSwitch = false;
                    await (await _objCharacter.SkillsSection.GetSkillsAsync(token).ConfigureAwait(false)).ForEachWithBreakAsync(async objSkill =>
                    {
                        if (await objSkill.GetBaseAsync(token).ConfigureAwait(false) > 0)
                        {
                            blnDoSwitch = false;
                            return false;
                        }

                        if (await objSkill.GetKarmaAsync(token).ConfigureAwait(false) > 0)
                            blnDoSwitch = true;
                        return true;
                    }, token).ConfigureAwait(false);

                    if (blnDoSwitch)
                    {
                        int intPointsSpent = 0;
                        SkillsSection objSkillsSection
                            = await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                        while (intPointsSpent < objSkillsSection.SkillGroupPointsMaximum)
                        {
                            Skill objSkillToShift = null;
                            int intSkillToShiftKarma = 0;
                            await (await _objCharacter.SkillsSection.GetSkillsAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objSkill =>
                            {
                                int intLoopKarma = await objSkill.GetKarmaAsync(token).ConfigureAwait(false);
                                if (intLoopKarma > 0 && (objSkillToShift == null
                                                         || await objSkillToShift.GetRatingAsync(token)
                                                             .ConfigureAwait(false)
                                                         < await objSkill.GetRatingAsync(token)
                                                             .ConfigureAwait(false)))
                                {
                                    objSkillToShift = objSkill;
                                    intSkillToShiftKarma = intLoopKarma;
                                }
                            }, token).ConfigureAwait(false);

                            if (objSkillToShift == null)
                                break;
                            int intKarma = Math.Min(intSkillToShiftKarma,
                                objSkillsSection.SkillGroupPointsMaximum - intPointsSpent);
                            await objSkillToShift.SetKarmaAsync(intSkillToShiftKarma - intKarma, token)
                                .ConfigureAwait(false);
                            await objSkillToShift
                                .SetBaseAsync(
                                    await objSkillToShift.GetBaseAsync(token).ConfigureAwait(false) + intKarma,
                                    token).ConfigureAwait(false);
                            intPointsSpent += intKarma;
                        }
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task AddFreeSkills(int intFreeLevels, Improvement.ImprovementType type, string strSkill1, string strSkill2, string strSkill3, CancellationToken token = default)
        {
            List<Improvement> lstOldFreeSkillImprovements
                = await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, type, token: token).ConfigureAwait(false);
            lstOldFreeSkillImprovements.RemoveAll(x => x.ImproveSource != Improvement.ImprovementSource.Heritage);
            if (intFreeLevels != 0)
            {
                bool blnCommit = false;
                try
                {
                    if (!string.IsNullOrEmpty(strSkill1))
                    {
                        Improvement objOldSkillImprovement
                            = lstOldFreeSkillImprovements.Find(
                                x => x.ImprovedName == strSkill1 && x.Value == intFreeLevels);
                        if (objOldSkillImprovement != null)
                            lstOldFreeSkillImprovements.Remove(objOldSkillImprovement);
                        else
                        {
                            blnCommit = true;
                            await AddExoticSkillIfNecessary(strSkill1).ConfigureAwait(false);
                            await ImprovementManager.CreateImprovementAsync(
                                _objCharacter, strSkill1, Improvement.ImprovementSource.Heritage, string.Empty,
                                type, string.Empty, intFreeLevels, token: token).ConfigureAwait(false);
                        }
                    }

                    if (!string.IsNullOrEmpty(strSkill2))
                    {
                        Improvement objOldSkillImprovement
                            = lstOldFreeSkillImprovements.Find(
                                x => x.ImprovedName == strSkill2 && x.Value == intFreeLevels);
                        if (objOldSkillImprovement != null)
                            lstOldFreeSkillImprovements.Remove(objOldSkillImprovement);
                        else
                        {
                            blnCommit = true;
                            await AddExoticSkillIfNecessary(strSkill2).ConfigureAwait(false);
                            await ImprovementManager.CreateImprovementAsync(
                                _objCharacter, strSkill2, Improvement.ImprovementSource.Heritage, string.Empty,
                                type, string.Empty, intFreeLevels, token: token).ConfigureAwait(false);
                        }
                    }

                    if (!string.IsNullOrEmpty(strSkill3))
                    {
                        Improvement objOldSkillImprovement
                            = lstOldFreeSkillImprovements.Find(
                                x => x.ImprovedName == strSkill3 && x.Value == intFreeLevels);
                        if (objOldSkillImprovement != null)
                            lstOldFreeSkillImprovements.Remove(objOldSkillImprovement);
                        else
                        {
                            blnCommit = true;
                            await AddExoticSkillIfNecessary(strSkill3).ConfigureAwait(false);
                            await ImprovementManager.CreateImprovementAsync(
                                _objCharacter, strSkill3, Improvement.ImprovementSource.Heritage, string.Empty,
                                type, string.Empty, intFreeLevels, token: token).ConfigureAwait(false);
                        }
                    }

                    if (lstOldFreeSkillImprovements.Count > 0)
                        await ImprovementManager
                              .RemoveImprovementsAsync(_objCharacter, lstOldFreeSkillImprovements, token: token)
                              .ConfigureAwait(false);
                }
                catch
                {
                    if (blnCommit)
                        await ImprovementManager.RollbackAsync(_objCharacter, CancellationToken.None).ConfigureAwait(false);
                    throw;
                }

                if (blnCommit)
                    await ImprovementManager.CommitAsync(_objCharacter, token).ConfigureAwait(false);

                async ValueTask AddExoticSkillIfNecessary(string strDictionaryKey)
                {
                    // Add exotic skills if we are increasing their base level
                    if (!await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strDictionaryKey, token).ConfigureAwait(false) ||
                        await _objCharacter.SkillsSection.GetActiveSkillAsync(strDictionaryKey, token).ConfigureAwait(false) != null)
                        return;
                    string strSkillName = strDictionaryKey;
                    string strSkillSpecific = string.Empty;
                    int intParenthesesIndex = strSkillName.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                    if (intParenthesesIndex > 0)
                    {
                        strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                        strSkillName = strSkillName.Substring(0, intParenthesesIndex);
                    }
                    await _objCharacter.SkillsSection.AddExoticSkillAsync(strSkillName, strSkillSpecific, token).ConfigureAwait(false);
                }
            }
            else
                await ImprovementManager.RemoveImprovementsAsync(_objCharacter, lstOldFreeSkillImprovements, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Manages adjusting priority selections to prevent doubling up in Priority mode.
        /// </summary>
        private async Task ManagePriorityItems(ComboBox comboBox, bool blnForce = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!blnForce && _objCharacter.EffectiveBuildMethod != CharacterBuildMethod.Priority)
                return;
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objManagePriorityItemsCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                List<string> lstCurrentPriorities = new List<string>(_lstPriorities);
                string strHeritageSelected = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                string strAttributesSelected = await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                string strTalentSelected = await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                string strSkillsSelected = await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                string strResourcesSelected = await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);

                // Discover which priority rating is not currently assigned
                lstCurrentPriorities.Remove(strHeritageSelected);
                lstCurrentPriorities.Remove(strAttributesSelected);
                lstCurrentPriorities.Remove(strTalentSelected);
                lstCurrentPriorities.Remove(strSkillsSelected);
                lstCurrentPriorities.Remove(strResourcesSelected);
                if (lstCurrentPriorities.Count == 0)
                    return;
                string strComboBoxSelected = (await comboBox.DoThreadSafeFuncAsync(x => x.SelectedValue, token).ConfigureAwait(false)).ToString();

                string strMissing = lstCurrentPriorities[0];

                // Find the combo with the same value as this one and change it to the missing value.
                //_blnInitializing = true;
                string strMyName = await comboBox.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false);
                if (strHeritageSelected == strComboBoxSelected && strMyName != await cboHeritage.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                    await cboHeritage.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token).ConfigureAwait(false);
                else if (strAttributesSelected == strComboBoxSelected && strMyName != await cboAttributes.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                    await cboAttributes.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token).ConfigureAwait(false);
                else if (strTalentSelected == strComboBoxSelected && strMyName != await cboTalent.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                    await cboTalent.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token).ConfigureAwait(false);
                else if (strSkillsSelected == strComboBoxSelected && strMyName != await cboSkills.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                    await cboSkills.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token).ConfigureAwait(false);
                else if (strResourcesSelected == strComboBoxSelected && strMyName != await cboResources.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                    await cboResources.DoThreadSafeAsync(x => x.SelectedValue = strMissing, token).ConfigureAwait(false);

                if (lstCurrentPriorities.Count <= 1)
                    return;
                do
                {
                    lstCurrentPriorities.Clear();
                    lstCurrentPriorities.AddRange(_lstPriorities);
                    strHeritageSelected = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                    strAttributesSelected = await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                    strTalentSelected = await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                    strSkillsSelected = await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);
                    strResourcesSelected = await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false);

                    // Discover which priority rating is not currently assigned
                    lstCurrentPriorities.Remove(strHeritageSelected);
                    lstCurrentPriorities.Remove(strAttributesSelected);
                    lstCurrentPriorities.Remove(strTalentSelected);
                    lstCurrentPriorities.Remove(strSkillsSelected);
                    lstCurrentPriorities.Remove(strResourcesSelected);
                    if (lstCurrentPriorities.Count == 0) // Just in case
                        return;

                    string strLoopMissing = lstCurrentPriorities[0];

                    // Find the combo with the same value as this one and change it to the missing value.
                    //_blnInitializing = true;
                    strMyName = await comboBox.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false);
                    if (strHeritageSelected == strComboBoxSelected && strMyName != await cboHeritage.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                        await cboHeritage.DoThreadSafeAsync(x => x.SelectedValue = strLoopMissing, token).ConfigureAwait(false);
                    else if (strAttributesSelected == strComboBoxSelected && strMyName != await cboAttributes.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                        await cboAttributes.DoThreadSafeAsync(x => x.SelectedValue = strLoopMissing, token).ConfigureAwait(false);
                    else if (strTalentSelected == strComboBoxSelected && strMyName != await cboTalent.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                        await cboTalent.DoThreadSafeAsync(x => x.SelectedValue = strLoopMissing, token).ConfigureAwait(false);
                    else if (strSkillsSelected == strComboBoxSelected && strMyName != await cboSkills.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                        await cboSkills.DoThreadSafeAsync(x => x.SelectedValue = strLoopMissing, token).ConfigureAwait(false);
                    else if (strResourcesSelected == strComboBoxSelected && strMyName != await cboResources.DoThreadSafeFuncAsync(x => x.Name, token).ConfigureAwait(false))
                        await cboResources.DoThreadSafeAsync(x => x.SelectedValue = strLoopMissing, token).ConfigureAwait(false);
                } while (lstCurrentPriorities.Count > 1);
            }
        }

        private async Task<int> SumToTen(bool blnDoUIUpdate = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn;
            if (blnDoUIUpdate)
            {
                CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
                CancellationToken objNewToken = objNewCancellationTokenSource.Token;
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objSumToTenCancellationTokenSource, objNewCancellationTokenSource);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
                {
                    token = objJoinedCancellationTokenSource.Token;
                    intReturn = _dicSumtoTenValues[await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                    intReturn += _dicSumtoTenValues[await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                    intReturn += _dicSumtoTenValues[await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                    intReturn += _dicSumtoTenValues[await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                    intReturn += _dicSumtoTenValues[await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];

                    string strText = intReturn.ToString(GlobalSettings.CultureInfo) + '/'
                                                                + _objCharacter.Settings.SumtoTen.ToString(GlobalSettings.CultureInfo);
                    await lblSumtoTen.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                }
            }
            else
            {
                intReturn = _dicSumtoTenValues[await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                intReturn += _dicSumtoTenValues[await cboAttributes.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                intReturn += _dicSumtoTenValues[await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                intReturn += _dicSumtoTenValues[await cboSkills.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
                intReturn += _dicSumtoTenValues[await cboResources.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token).ConfigureAwait(false)];
            }
            return intReturn;
        }

        private async Task RefreshSelectedMetatype(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshSelectedMetatypeCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strSelectedMetatype = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                string strSelectedMetavariant = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                string strSelectedHeritage = await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);

                XPathNavigator objXmlMetatype
                    = _xmlBaseMetatypeDataNode.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                XPathNavigator objXmlMetavariant
                    = string.IsNullOrEmpty(strSelectedMetavariant) || strSelectedMetavariant == Guid.Empty.ToString("D")
                        ? null
                        : objXmlMetatype?.TryGetNodeByNameOrId("metavariants/metavariant", strSelectedMetavariant);
                XPathNavigator objXmlMetatypePriorityNode = null;
                XPathNavigator objXmlMetavariantPriorityNode = null;
                XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                    "priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                                                                               + " and (not(prioritytable) or prioritytable = "
                                                                               + _objCharacter.Settings.PriorityTable
                                                                                   .CleanXPath() + ")]");
                foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                {
                    if (xmlBaseMetatypePriorityList.Count == 1
                        || xmlBaseMetatypePriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                    {
                        objXmlMetatypePriorityNode
                            = xmlBaseMetatypePriority.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                        objXmlMetavariantPriorityNode = objXmlMetavariant != null
                            ? objXmlMetatypePriorityNode.TryGetNodeByNameOrId("metavariants/metavariant", strSelectedMetavariant)
                            : null;
                        break;
                    }
                }

                string strAttributeFormat = "{0}/{1}" + strSpace + "({2})";
                if (objXmlMetavariant != null)
                {
                    if (objXmlMetavariantPriorityNode == null)
                    {
                        Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("String_NotSupported", token: token).ConfigureAwait(false),
                                               Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    }

                    string strMin = objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax = objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodmax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug = objXmlMetavariant.SelectSingleNodeAndCacheExpression("bodaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblBOD.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token).ConfigureAwait(false);
                    string strMin2 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimin", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax2 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("agimax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug2 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("agiaug", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblAGI.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin2, strMax2, strAug2), token).ConfigureAwait(false);
                    string strMin3 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamin", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax3 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("reamax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug3 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("reaaug", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblREA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin3, strMax3, strAug3), token).ConfigureAwait(false);
                    string strMin4 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmin", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax4 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("strmax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug4 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("straug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblSTR.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin4, strMax4, strAug4), token).ConfigureAwait(false);
                    string strMin5 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax5 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("chamax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug5 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("chaaug", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblCHA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin5, strMax5, strAug5), token).ConfigureAwait(false);
                    string strMin6 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmin", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax6 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("intmax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug6 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("intaug", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblINT.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin6, strMax6, strAug6), token).ConfigureAwait(false);
                    string strMin7 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmin", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax7 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("logmax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug7 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("logaug", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblLOG.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin7, strMax7, strAug7), token).ConfigureAwait(false);
                    string strMin8 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmin", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax8 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilmax", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug8 = objXmlMetavariant.SelectSingleNodeAndCacheExpression("wilaug", token)?.Value
                                     ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblWIL.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin8, strMax8, strAug8), token).ConfigureAwait(false);

                    string strKarmaText
                        = objXmlMetavariantPriorityNode.SelectSingleNodeAndCacheExpression("karma", token)?.Value
                          ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblMetavariantKarma.DoThreadSafeAsync(x => x.Text = strKarmaText, token).ConfigureAwait(false);

                    string strSource = objXmlMetavariant.SelectSingleNodeAndCacheExpression("source", token)
                        ?.Value;
                    if (!string.IsNullOrEmpty(strSource))
                    {
                        string strPage
                            = objXmlMetavariant.SelectSingleNodeAndCacheExpression("altpage", token)?.Value
                              ?? objXmlMetavariant.SelectSingleNodeAndCacheExpression("page", token)?.Value;
                        if (!string.IsNullOrEmpty(strPage))
                        {
                            SourceString objSource = await SourceString.GetSourceStringAsync(
                                strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo,
                                _objCharacter, token).ConfigureAwait(false);
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

                    // Set the special attributes label.
                    if (objXmlMetavariantPriorityNode == null || !int.TryParse(
                            objXmlMetavariantPriorityNode.SelectSingleNodeAndCacheExpression("value", token)?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                        intSpecialAttribPoints = 0;

                    XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Talent\" and value = "
                        + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                    {
                        if (xmlBaseTalentPriorityList.Count == 1
                            || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                        {
                            XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode(
                                "talents/talent[value = "
                                + (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath() + ']');
                            if (objXmlTalentsNode != null
                                && int.TryParse(
                                    objXmlTalentsNode.SelectSingleNodeAndCacheExpression("specialattribpoints", token)?.Value, out int intTemp))
                                intSpecialAttribPoints += intTemp;
                            break;
                        }
                    }

                    await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                    Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                    // Build a list of the Metavariant's Qualities.
                    foreach (XPathNavigator objXmlQuality in objXmlMetavariant.SelectAndCacheExpression(
                                 "qualities/*/quality", token))
                    {
                        string strQuality;
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            strQuality = _xmlBaseQualityDataNode
                                         .TryGetNodeByNameOrId("qualities/quality", objXmlQuality.Value)
                                         ?.Value
                                         ?? objXmlQuality.Value;

                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select", token)?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strQuality += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect, token: token).ConfigureAwait(false)
                                              + ')';
                        }
                        else
                        {
                            strQuality = objXmlQuality.Value;
                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select", token)?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strQuality += strSpace + '(' + strSelect + ')';
                        }

                        if (dicQualities.TryGetValue(strQuality, out int intExistingRating))
                            dicQualities[strQuality] = intExistingRating + 1;
                        else
                            dicQualities.Add(strQuality, 1);
                    }

                    if (dicQualities.Count > 0)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdQualities))
                        {
                            foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                            {
                                sbdQualities.Append(objLoopQuality.Key);
                                if (objLoopQuality.Value > 1)
                                {
                                    sbdQualities.Append(strSpace)
                                                .Append(objLoopQuality.Value.ToString(GlobalSettings.CultureInfo));
                                }

                                sbdQualities.Append(',').Append(strSpace);
                            }

                            sbdQualities.Length -= 2;
                            await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = sbdQualities.ToString(), token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
                        await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = strNone, token).ConfigureAwait(false);
                    }
                }
                else if (objXmlMetatype != null)
                {
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    string strMin = objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax = objXmlMetatype.SelectSingleNodeAndCacheExpression("bodmax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug = objXmlMetatype.SelectSingleNodeAndCacheExpression("bodaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblBOD.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin, strMax, strAug), token).ConfigureAwait(false);
                    string strMin2 = objXmlMetatype.SelectSingleNodeAndCacheExpression("agimin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax2 = objXmlMetatype.SelectSingleNodeAndCacheExpression("agimax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug2 = objXmlMetatype.SelectSingleNodeAndCacheExpression("agiaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblAGI.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin2, strMax2, strAug2), token).ConfigureAwait(false);
                    string strMin3 = objXmlMetatype.SelectSingleNodeAndCacheExpression("reamin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax3 = objXmlMetatype.SelectSingleNodeAndCacheExpression("reamax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug3 = objXmlMetatype.SelectSingleNodeAndCacheExpression("reaaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblREA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin3, strMax3, strAug3), token).ConfigureAwait(false);
                    string strMin4 = objXmlMetatype.SelectSingleNodeAndCacheExpression("strmin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax4 = objXmlMetatype.SelectSingleNodeAndCacheExpression("strmax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug4 = objXmlMetatype.SelectSingleNodeAndCacheExpression("straug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblSTR.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin4, strMax4, strAug4), token).ConfigureAwait(false);
                    string strMin5 = objXmlMetatype.SelectSingleNodeAndCacheExpression("chamin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax5 = objXmlMetatype.SelectSingleNodeAndCacheExpression("chamax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug5 = objXmlMetatype.SelectSingleNodeAndCacheExpression("chaaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblCHA.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin5, strMax5, strAug5), token).ConfigureAwait(false);
                    string strMin6 = objXmlMetatype.SelectSingleNodeAndCacheExpression("intmin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax6 = objXmlMetatype.SelectSingleNodeAndCacheExpression("intmax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug6 = objXmlMetatype.SelectSingleNodeAndCacheExpression("intaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblINT.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin6, strMax6, strAug6), token).ConfigureAwait(false);
                    string strMin7 = objXmlMetatype.SelectSingleNodeAndCacheExpression("logmin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax7 = objXmlMetatype.SelectSingleNodeAndCacheExpression("logmax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug7 = objXmlMetatype.SelectSingleNodeAndCacheExpression("logaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblLOG.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin7, strMax7, strAug7), token).ConfigureAwait(false);
                    string strMin8 = objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmin", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strMax8 = objXmlMetatype.SelectSingleNodeAndCacheExpression("wilmax", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    string strAug8 = objXmlMetatype.SelectSingleNodeAndCacheExpression("wilaug", token)?.Value
                                    ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblWIL.DoThreadSafeAsync(x => x.Text = string.Format(GlobalSettings.CultureInfo, strAttributeFormat, strMin8, strMax8, strAug8), token).ConfigureAwait(false);

                    string strKarmaText
                        = objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression("karma", token)?.Value
                          ?? 0.ToString(GlobalSettings.CultureInfo);
                    await lblMetavariantKarma.DoThreadSafeAsync(x => x.Text = strKarmaText, token).ConfigureAwait(false);

                    Dictionary<string, int> dicQualities = new Dictionary<string, int>(5);
                    // Build a list of the Metatype's Qualities.
                    foreach (XPathNavigator xmlQuality in objXmlMetatype.SelectAndCacheExpression(
                                 "qualities/*/quality", token))
                    {
                        string strQuality;
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            XPathNavigator objQuality
                                = _xmlBaseQualityDataNode.TryGetNodeByNameOrId("qualities/quality", xmlQuality.Value);
                            strQuality = objQuality.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                         ?? xmlQuality.Value;

                            string strSelect = xmlQuality.SelectSingleNodeAndCacheExpression("@select", token)?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strQuality += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect, token: token).ConfigureAwait(false)
                                              + ')';
                        }
                        else
                        {
                            strQuality = xmlQuality.Value;
                            string strSelect = xmlQuality.SelectSingleNodeAndCacheExpression("@select", token)?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strQuality += strSpace + '(' + strSelect + ')';
                        }

                        if (dicQualities.TryGetValue(strQuality, out int intExistingRating))
                            dicQualities[strQuality] = intExistingRating + 1;
                        else
                            dicQualities.Add(strQuality, 1);
                    }

                    if (dicQualities.Count > 0)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdQualities))
                        {
                            foreach (KeyValuePair<string, int> objLoopQuality in dicQualities)
                            {
                                sbdQualities.Append(objLoopQuality.Key);
                                if (objLoopQuality.Value > 1)
                                {
                                    sbdQualities.Append(strSpace)
                                                .Append(objLoopQuality.Value.ToString(GlobalSettings.CultureInfo));
                                }

                                sbdQualities.Append(',').Append(strSpace);
                            }

                            sbdQualities.Length -= 2;
                            await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = sbdQualities.ToString(), token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
                        await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = strNone, token).ConfigureAwait(false);
                    }

                    string strSource = objXmlMetatype.SelectSingleNodeAndCacheExpression("source", token)
                        ?.Value;
                    if (!string.IsNullOrEmpty(strSource))
                    {
                        string strPage
                            = objXmlMetatype.SelectSingleNodeAndCacheExpression("altpage", token)?.Value
                              ?? objXmlMetatype.SelectSingleNodeAndCacheExpression("page", token)?.Value;
                        if (!string.IsNullOrEmpty(strPage))
                        {
                            SourceString objSource = await SourceString.GetSourceStringAsync(
                                strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo,
                                _objCharacter, token).ConfigureAwait(false);
                            await objSource.SetControlAsync(lblSource, token).ConfigureAwait(false);
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

                    // Set the special attributes label.
                    if (!int.TryParse(
                            objXmlMetatypePriorityNode.SelectSingleNodeAndCacheExpression("value", token)?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out int intSpecialAttribPoints))
                        intSpecialAttribPoints = 0;

                    XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Talent\" and value = "
                        + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                    {
                        if (xmlBaseTalentPriorityList.Count == 1
                            || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                        {
                            XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode(
                                "talents/talent[value = "
                                + (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath() + ']');
                            if (objXmlTalentsNode != null
                                && int.TryParse(
                                    objXmlTalentsNode.SelectSingleNodeAndCacheExpression(
                                            "specialattribpoints", token)
                                    ?.Value, out int intTemp))
                                intSpecialAttribPoints += intTemp;
                            break;
                        }
                    }

                    await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);
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

                    int intSpecialAttribPoints = 0;
                    XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Talent\" and value = "
                        + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = "
                        + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                    {
                        if (xmlBaseTalentPriorityList.Count == 1
                            || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                        {
                            XPathNavigator objXmlTalentsNode = xmlBaseTalentPriority.SelectSingleNode(
                                "talents/talent[value = "
                                + (await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath() + ']');
                            if (objXmlTalentsNode != null
                                && int.TryParse(
                                    objXmlTalentsNode.SelectSingleNodeAndCacheExpression("specialattribpoints", token)
                                    ?.Value, out int intTemp))
                                intSpecialAttribPoints += intTemp;
                            break;
                        }
                    }

                    await lblSpecialAttributes.DoThreadSafeAsync(x => x.Text = intSpecialAttribPoints.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);

                    await lblMetavariantQualities.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblMetavariantKarma.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblSource.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
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
                bool blnVisible9 = await lblSpecialAttributes.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblSpecialAttributesLabel.DoThreadSafeAsync(x => x.Visible = blnVisible9, token).ConfigureAwait(false);
                bool blnVisible10 = await lblMetavariantQualities.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblMetavariantQualitiesLabel.DoThreadSafeAsync(x => x.Visible = blnVisible10, token).ConfigureAwait(false);
                bool blnVisible11 = await lblMetavariantKarma.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblMetavariantKarmaLabel.DoThreadSafeAsync(x => x.Visible = blnVisible11, token).ConfigureAwait(false);
                bool blnVisible12 = await lblSourceLabel.DoThreadSafeFuncAsync(x => !string.IsNullOrEmpty(x.Text), token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnVisible12, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateTalents(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objPopulateTalentsCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                // Load the Priority information.
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTalent))
                {
                    // Populate the Priority Category list.
                    XPathNodeIterator xmlBaseTalentPriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Talent\" and value = "
                        + (await cboTalent.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? string.Empty).CleanXPath()
                        + " and (not(prioritytable) or prioritytable = " + _objCharacter.Settings.PriorityTable.CleanXPath()
                        + ")]");
                    foreach (XPathNavigator xmlBaseTalentPriority in xmlBaseTalentPriorityList)
                    {
                        if (xmlBaseTalentPriorityList.Count == 1
                            || xmlBaseTalentPriority.SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                        {
                            foreach (XPathNavigator objXmlPriorityTalent in xmlBaseTalentPriority.SelectAndCacheExpression(
                                         "talents/talent", token))
                            {
                                XPathNavigator xmlQualitiesNode
                                    = objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("qualities", token);
                                if (xmlQualitiesNode != null)
                                {
                                    bool blnFoundUnavailableQuality = false;

                                    foreach (XPathNavigator xmlQuality in xmlQualitiesNode.SelectAndCacheExpression(
                                                 "quality", token))
                                    {
                                        if (_xmlBaseQualityDataNode.TryGetNodeByNameOrId(
                                                "qualities/quality", xmlQuality.Value, await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)) == null)
                                        {
                                            blnFoundUnavailableQuality = true;
                                            break;
                                        }
                                    }

                                    if (blnFoundUnavailableQuality)
                                        continue;
                                }

                                XPathNavigator xmlForbiddenNode
                                    = objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("forbidden", token);
                                if (xmlForbiddenNode != null)
                                {
                                    bool blnRequirementForbidden = false;

                                    // Loop through the oneof requirements.
                                    XPathNodeIterator objXmlForbiddenList
                                        = xmlForbiddenNode.SelectAndCacheExpression("oneof", token);
                                    foreach (XPathNavigator objXmlOneOf in objXmlForbiddenList)
                                    {
                                        XPathNodeIterator objXmlOneOfList
                                            = objXmlOneOf.SelectChildren(XPathNodeType.Element);

                                        foreach (XPathNavigator objXmlForbidden in objXmlOneOfList)
                                        {
                                            switch (objXmlForbidden.Name)
                                            {
                                                case "metatype":
                                                    {
                                                        // Check the Metatype restriction.
                                                        if (objXmlForbidden.Value == await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false))
                                                        {
                                                            blnRequirementForbidden = true;
                                                            goto EndForbiddenLoop;
                                                        }

                                                        break;
                                                    }
                                                // Check the Metavariant restriction.
                                                case "metatypecategory":
                                                    {
                                                        // Check the Metatype Category restriction.
                                                        if (objXmlForbidden.Value == await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false))
                                                        {
                                                            blnRequirementForbidden = true;
                                                            goto EndForbiddenLoop;
                                                        }

                                                        break;
                                                    }
                                                case "metavariant" when objXmlForbidden.Value == await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false):
                                                    blnRequirementForbidden = true;
                                                    goto EndForbiddenLoop;
                                            }
                                        }
                                    }

                                    EndForbiddenLoop:
                                    if (blnRequirementForbidden)
                                        continue;
                                }

                                XPathNavigator xmlRequiredNode
                                    = objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("required", token);
                                if (xmlRequiredNode != null)
                                {
                                    bool blnRequirementMet = false;

                                    // Loop through the oneof requirements.
                                    XPathNodeIterator objXmlForbiddenList
                                        = xmlRequiredNode.SelectAndCacheExpression("oneof", token);
                                    foreach (XPathNavigator objXmlOneOf in objXmlForbiddenList)
                                    {
                                        XPathNodeIterator objXmlOneOfList
                                            = objXmlOneOf.SelectChildren(XPathNodeType.Element);

                                        foreach (XPathNavigator objXmlRequired in objXmlOneOfList)
                                        {
                                            switch (objXmlRequired.Name)
                                            {
                                                case "metatype":
                                                    {
                                                        // Check the Metatype restriction.
                                                        if (objXmlRequired.Value == await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false))
                                                        {
                                                            blnRequirementMet = true;
                                                            goto EndRequiredLoop;
                                                        }

                                                        break;
                                                    }
                                                // Check the Metavariant restriction.
                                                case "metatypecategory":
                                                    {
                                                        // Check the Metatype Category restriction.
                                                        if (objXmlRequired.Value == await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false))
                                                        {
                                                            blnRequirementMet = true;
                                                            goto EndRequiredLoop;
                                                        }

                                                        break;
                                                    }
                                                case "metavariant" when objXmlRequired.Value == await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false):
                                                    blnRequirementMet = true;
                                                    goto EndRequiredLoop;
                                            }
                                        }
                                    }

                                    EndRequiredLoop:
                                    if (!blnRequirementMet)
                                        continue;
                                }

                                lstTalent.Add(new ListItem(
                                                  objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("value", token)?.Value,
                                                  objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("translate", token)
                                                                      ?.Value ??
                                                  objXmlPriorityTalent.SelectSingleNodeAndCacheExpression("name", token)?.Value ??
                                                  await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                            }

                            break;
                        }
                    }

                    lstTalent.Sort(CompareListItems.CompareNames);
                    int intOldSelectedIndex = await cboTalents.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false);
                    int intOldDataSourceSize = await cboTalents.DoThreadSafeFuncAsync(x => x.Items.Count, token).ConfigureAwait(false);
                    await cboTalents.PopulateWithListItemsAsync(lstTalent, token).ConfigureAwait(false);
                    if (intOldDataSourceSize == await cboTalents.DoThreadSafeFuncAsync(x => x.Items.Count, token).ConfigureAwait(false))
                    {
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await cboTalents.DoThreadSafeAsync(x => x.SelectedIndex = intOldSelectedIndex, token)
                                            .ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }
                    }

                    await cboTalents.DoThreadSafeAsync(x => x.Enabled = x.Items.Count > 1, token).ConfigureAwait(false);
                }
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
                string strSelectedMetatype = await lstMetatypes
                                               .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                               .ConfigureAwait(false);

                // Don't attempt to do anything if nothing is selected.
                if (!string.IsNullOrEmpty(strSelectedMetatype))
                {
                    string strSelectedHeritage = await cboHeritage
                                                       .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                       .ConfigureAwait(false);

                    XPathNavigator objXmlMetatype
                        = _xmlBaseMetatypeDataNode.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                    XPathNavigator objXmlMetatypeBP = null;
                    XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                        "priorities/priority[category = \"Heritage\" and value = " + strSelectedHeritage.CleanXPath()
                                                                                   + " and (not(prioritytable) or prioritytable = "
                                                                                   + _objCharacter.Settings.PriorityTable
                                                                                       .CleanXPath() + ")]");
                    foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                    {
                        if (xmlBaseMetatypePriorityList.Count == 1 || xmlBaseMetatypePriority
                                                                            .SelectSingleNodeAndCacheExpression(
                                                                                "prioritytable", token) != null)
                        {
                            objXmlMetatypeBP
                                = xmlBaseMetatypePriority.TryGetNodeByNameOrId("metatypes/metatype", strSelectedMetatype);
                            break;
                        }
                    }

                    if (objXmlMetatype != null && objXmlMetatypeBP != null)
                    {
                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstMetavariants))
                        {
                            lstMetavariants.Add(new ListItem(
                                                    Guid.Empty.ToString("D"),
                                                    await LanguageManager.GetStringAsync("String_None", token: token)
                                                                         .ConfigureAwait(false)));
                            // Retrieve the list of Metavariants for the selected Metatype.
                            foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select(
                                         "metavariants/metavariant[" + await _objCharacter.Settings
                                             .BookXPathAsync(token: token).ConfigureAwait(false) + ']'))
                            {
                                string strId = objXmlMetavariant
                                    .SelectSingleNodeAndCacheExpression("id", token)?.Value;
                                string strName = objXmlMetavariant
                                    .SelectSingleNodeAndCacheExpression("name", token)?.Value;
                                if (!string.IsNullOrEmpty(strId)
                                    && objXmlMetatypeBP.TryGetNodeByNameOrId(
                                        "metavariants/metavariant", strId) != null)
                                {
                                    if (string.IsNullOrEmpty(strName))
                                        strName = await LanguageManager
                                                        .GetStringAsync("String_Unknown", token: token)
                                                        .ConfigureAwait(false);
                                    lstMetavariants.Add(new ListItem(
                                                            strId,
                                                            objXmlMetavariant
                                                                .SelectSingleNodeAndCacheExpression(
                                                                    "translate", token)
                                                            ?.Value ?? strName));
                                }
                                else if (!string.IsNullOrEmpty(strName)
                                         && objXmlMetatypeBP.TryGetNodeByNameOrId(
                                             "metavariants/metavariant", strName) != null)
                                {
                                    lstMetavariants.Add(new ListItem(
                                                            strName,
                                                            objXmlMetavariant
                                                                .SelectSingleNodeAndCacheExpression(
                                                                    "translate", token)
                                                            ?.Value ?? strName));
                                }
                            }

                            string strOldSelectedValue
                                = await cboMetavariant.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                    .ConfigureAwait(false) ?? (await _objCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false)).ToString("D");
                            Interlocked.Increment(ref _intLoading);
                            try
                            {
                                await cboMetavariant.PopulateWithListItemsAsync(lstMetavariants, token)
                                                    .ConfigureAwait(false);
                                await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = lstMetavariants.Count > 1, token)
                                                    .ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intLoading);
                            }

                            await cboMetavariant.DoThreadSafeAsync(x =>
                            {
                                if (!string.IsNullOrEmpty(strOldSelectedValue))
                                    x.SelectedValue = strOldSelectedValue;
                                if (x.SelectedIndex == -1)
                                    x.SelectedIndex = 0;
                            }, token).ConfigureAwait(false);

                            // If the Metatype has Force enabled, show the Force NUD.
                            string strEssMax
                                = objXmlMetatype.SelectSingleNodeAndCacheExpression("essmax", token)?.Value
                                  ?? string.Empty;
                            int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                            if (objXmlMetatype.SelectSingleNodeAndCacheExpression("forcecreature", token) != null || intPos != -1)
                            {
                                if (intPos != -1)
                                {
                                    string strD6 = await LanguageManager.GetStringAsync("String_D6", token: token)
                                                                        .ConfigureAwait(false);
                                    if (intPos > 0)
                                    {
                                        --intPos;
                                        await lblForceLabel.DoThreadSafeAsync(x => x.Text = strEssMax.Substring(intPos, 3)
                                                                                  .Replace("D6", strD6), token)
                                                           .ConfigureAwait(false);
                                        await nudForce.DoThreadSafeAsync(x => x.Maximum
                                                                             = Convert.ToInt32(
                                                                                 strEssMax[intPos],
                                                                                 GlobalSettings.InvariantCultureInfo) * 6,
                                                                         token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await lblForceLabel
                                              .DoThreadSafeAsync(
                                                  x => x.Text = 1.ToString(GlobalSettings.CultureInfo) + strD6, token)
                                              .ConfigureAwait(false);
                                        await nudForce.DoThreadSafeAsync(x => x.Maximum = 6, token).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    lblForceLabel.Text = await LanguageManager.GetStringAsync(
                                        objXmlMetatype.SelectSingleNodeAndCacheExpression("forceislevels", token) != null
                                            ? "String_Level"
                                            : "String_Force", token: token).ConfigureAwait(false);
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
                        }
                    }
                    else
                    {
                        await cboMetavariant
                              .PopulateWithListItemsAsync(
                                  new ListItem(
                                      Guid.Empty.ToString("D"),
                                      await LanguageManager.GetStringAsync("String_None", token: token)
                                                           .ConfigureAwait(false)).Yield(), token).ConfigureAwait(false);
                        await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);

                        await lblForceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                        await nudForce.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Clear the Metavariant list if nothing is currently selected.
                    await cboMetavariant
                          .PopulateWithListItemsAsync(
                              new ListItem(
                                      Guid.Empty.ToString("D"),
                                      await LanguageManager.GetStringAsync("String_None", token: token)
                                                           .ConfigureAwait(false))
                                  .Yield(), token).ConfigureAwait(false);
                    await cboMetavariant.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);

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
                string strSelectedCategory = await cboCategory
                                               .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                               .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedCategory))
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatype))
                    {
                        XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                            "priorities/priority[category = \"Heritage\" and value = "
                            + (await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                .ConfigureAwait(false) ?? string.Empty).CleanXPath()
                            + " and (not(prioritytable) or prioritytable = "
                            + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                        foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                        {
                            if (xmlBaseMetatypePriorityList.Count == 1
                                || xmlBaseMetatypePriority
                                         .SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                            {
                                foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                             "metatypes/metatype[("
                                             + await _objCharacter.Settings.BookXPathAsync(token: token)
                                                                  .ConfigureAwait(false)
                                             + ") and category = " + strSelectedCategory.CleanXPath()
                                             + ']'))
                                {
                                    string strId = objXmlMetatype
                                        .SelectSingleNodeAndCacheExpression("id", token)?.Value;
                                    string strName = objXmlMetatype
                                        .SelectSingleNodeAndCacheExpression("name", token)?.Value;
                                    if (!string.IsNullOrEmpty(strId)
                                        && xmlBaseMetatypePriority.TryGetNodeByNameOrId(
                                            "metatypes/metatype", strId) != null)
                                    {
                                        if (string.IsNullOrEmpty(strName))
                                            strName = await LanguageManager
                                                            .GetStringAsync("String_Unknown", token: token)
                                                            .ConfigureAwait(false);
                                        lstMetatype.Add(new ListItem(
                                                            strId,
                                                            objXmlMetatype
                                                                .SelectSingleNodeAndCacheExpression(
                                                                    "translate", token)
                                                            ?.Value ?? strName));
                                    }
                                    else if (!string.IsNullOrEmpty(strName)
                                             && xmlBaseMetatypePriority.TryGetNodeByNameOrId(
                                                 "metatypes/metatype", strName) != null)
                                    {
                                        lstMetatype.Add(new ListItem(
                                                            strName,
                                                            objXmlMetatype
                                                                .SelectSingleNodeAndCacheExpression(
                                                                    "translate", token)
                                                            ?.Value ?? strName));
                                    }
                                }

                                break;
                            }
                        }

                        lstMetatype.Sort(CompareListItems.CompareNames);
                        string strOldSelectedValue
                            = await lstMetatypes.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                .ConfigureAwait(false) ?? _objCharacter.Metatype;
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await lstMetatypes.PopulateWithListItemsAsync(lstMetatype, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }

                        await lstMetatypes.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strOldSelectedValue))
                                x.SelectedValue = strOldSelectedValue;
                            if (x.SelectedIndex == -1 && lstMetatype.Count > 0)
                                x.SelectedIndex = 0;
                        }, token).ConfigureAwait(false);
                    }

                    if (strSelectedCategory.EndsWith("Spirits", StringComparison.Ordinal))
                    {
                        if (!await chkPossessionBased.DoThreadSafeFuncAsync(x => x.Visible, token).ConfigureAwait(false)
                            && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                        {
                            await chkPossessionBased.DoThreadSafeAsync(x => x.Checked = true, token).ConfigureAwait(false);
                            await cboPossessionMethod
                                  .DoThreadSafeAsync(x => x.SelectedValue = _strCurrentPossessionMethod, token)
                                  .ConfigureAwait(false);
                        }

                        await chkPossessionBased.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                        await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
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
                }
                else
                {
                    await lstMetatypes.DoThreadSafeAsync(x => x.DataSource = null, token).ConfigureAwait(false);
                    await chkPossessionBased.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, token).ConfigureAwait(false);
                    await cboPossessionMethod.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                }
            }
        }

        private async Task LoadMetatypes(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objLoadMetatypesCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                // Create a list of any Categories that should not be in the list.
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setRemoveCategories))
                {
                    foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.SelectAndCacheExpression(
                                 "categories/category", token))
                    {
                        XPathNodeIterator xmlBaseMetatypePriorityList = _xmlBasePriorityDataNode.Select(
                            "priorities/priority[category = \"Heritage\" and value = "
                            + (await cboHeritage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                .ConfigureAwait(false) ?? string.Empty).CleanXPath()
                            + " and (not(prioritytable) or prioritytable = "
                            + _objCharacter.Settings.PriorityTable.CleanXPath() + ")]");
                        bool blnRemoveCategory = true;
                        foreach (XPathNavigator xmlBaseMetatypePriority in xmlBaseMetatypePriorityList)
                        {
                            if (xmlBaseMetatypePriorityList.Count == 1
                                || xmlBaseMetatypePriority
                                         .SelectSingleNodeAndCacheExpression("prioritytable", token) != null)
                            {
                                foreach (XPathNavigator objXmlMetatype in _xmlBaseMetatypeDataNode.Select(
                                             "metatypes/metatype[category = " + objXmlCategory.Value.CleanXPath() + " and ("
                                             + await _objCharacter.Settings.BookXPathAsync(token: token)
                                                                  .ConfigureAwait(false) + ")]"))
                                {
                                    if (xmlBaseMetatypePriority.TryGetNodeByNameOrId(
                                            "metatypes/metatype", objXmlMetatype
                                                .SelectSingleNodeAndCacheExpression(
                                                    "id", token)?.Value) != null
                                        || xmlBaseMetatypePriority.TryGetNodeByNameOrId(
                                            "metatypes/metatype", objXmlMetatype
                                                .SelectSingleNodeAndCacheExpression(
                                                    "name", token)?.Value) != null)
                                    {
                                        blnRemoveCategory = false;
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        // Remove metatypes not covered by heritage
                        if (blnRemoveCategory)
                            setRemoveCategories.Add(objXmlCategory.Value);
                    }

                    // Populate the Metatype Category list.
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
                    {
                        foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode
                                                                        .SelectAndCacheExpression(
                                                                            "categories/category", token))
                        {
                            string strInnerText = objXmlCategory.Value;

                            // Make sure the Category isn't in the exclusion list.
                            if (!setRemoveCategories.Contains(strInnerText) &&
                                // Also make sure it is not already in the Category list.
                                lstCategory.TrueForAll(objItem => objItem.Value.ToString() != strInnerText))
                            {
                                lstCategory.Add(new ListItem(strInnerText,
                                                             objXmlCategory
                                                                 .SelectSingleNodeAndCacheExpression(
                                                                     "@translate", token)
                                                             ?.Value ?? strInnerText));
                            }
                        }

                        lstCategory.Sort(CompareListItems.CompareNames);
                        string strOldSelected
                            = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                               .ConfigureAwait(false) ?? _objCharacter.MetatypeCategory;
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await cboCategory.PopulateWithListItemsAsync(lstCategory, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }

                        await cboCategory.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strOldSelected))
                                x.SelectedValue = strOldSelected;
                            if (x.SelectedIndex == -1 && lstCategory.Count > 0)
                                x.SelectedIndex = 0;
                        }, token).ConfigureAwait(false);
                    }
                }
            }
        }

        private XPathNodeIterator GetMatrixSkillList(CancellationToken token = default)
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression("skills/skill[skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]", token);
        }

        private XPathNodeIterator GetMagicalSkillList(CancellationToken token = default)
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression("skills/skill[category = \"Magical Active\" or category = \"Pseudo-Magical Active\"]", token);
        }

        private XPathNodeIterator GetResonanceSkillList(CancellationToken token = default)
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression("skills/skill[category = \"Resonance Active\" or skillgroup = \"Cracking\" or skillgroup = \"Electronics\"]", token);
        }

        private XPathNodeIterator GetActiveSkillList(string strXPathFilter = "", CancellationToken token = default)
        {
            return _xmlBaseSkillDataNode.SelectAndCacheExpression(!string.IsNullOrEmpty(strXPathFilter)
                ? "skills/skill[" + strXPathFilter + ']'
                : "skills/skill", token);
        }

        private XPathNodeIterator BuildSkillCategoryList(XPathNodeIterator objSkillList)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdGroups))
            {
                sbdGroups.Append("skillgroups/name");
                if (objSkillList.Count > 0)
                {
                    sbdGroups.Append('[');
                    foreach (XPathNavigator xmlSkillGroup in objSkillList)
                    {
                        sbdGroups.Append(". = ").Append(xmlSkillGroup.Value.CleanXPath()).Append(" or ");
                    }

                    sbdGroups.Length -= 4;
                    sbdGroups.Append(']');
                }

                return _xmlBaseSkillDataNode.Select(sbdGroups.ToString());
            }
        }

        private XPathNodeIterator BuildSkillList(XPathNodeIterator objSkillList)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdGroups))
            {
                sbdGroups.Append("skills/skill");
                if (objSkillList.Count > 0)
                {
                    sbdGroups.Append('[');
                    foreach (XPathNavigator xmlSkillGroup in objSkillList)
                    {
                        sbdGroups.Append("name = ").Append(xmlSkillGroup.Value.CleanXPath()).Append(" or ");
                    }

                    sbdGroups.Length -= 4;
                    sbdGroups.Append(']');
                }

                return _xmlBaseSkillDataNode.Select(sbdGroups.ToString());
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
