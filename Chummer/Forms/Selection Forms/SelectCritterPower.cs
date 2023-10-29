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
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectCritterPower : Form
    {
        private string _strSelectedPower = string.Empty;
        private int _intSelectedRating;
        private static string _strSelectCategory = string.Empty;
        private decimal _decPowerPoints;
        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseCritterPowerDataNode;
        private readonly XPathNavigator _xmlMetatypeDataNode;
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory = Utils.ListItemListPool.Get();

        #region Control Events

        public SelectCritterPower(Character objCharacter)
        {
            Disposed += (sender, args) => Utils.ListItemListPool.Return(ref _lstCategory);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _xmlBaseCritterPowerDataNode = _objCharacter.LoadDataXPath("critterpowers.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlMetatypeDataNode = _objCharacter.GetNodeXPath();

            if (_xmlMetatypeDataNode == null || _objCharacter.MetavariantGuid == Guid.Empty) return;
            XPathNavigator xmlMetavariantNode = _xmlMetatypeDataNode.TryGetNodeById("metavariants/metavariant", _objCharacter.MetavariantGuid);
            if (xmlMetavariantNode != null)
                _xmlMetatypeDataNode = xmlMetavariantNode;
        }

        private async void SelectCritterPower_Load(object sender, EventArgs e)
        {
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseCritterPowerDataNode.SelectAndCacheExpression("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if (strInnerText.ContainsAny((await ImprovementManager
                                                    .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                        Improvement.ImprovementType.AllowCritterPowerCategory)
                                                    .ConfigureAwait(false))
                                             .Select(imp => imp.ImprovedName))
                    && (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@whitelist")
                                            .ConfigureAwait(false))?.Value == bool.TrueString
                    || strInnerText.ContainsAny((await ImprovementManager
                                                       .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                           Improvement.ImprovementType
                                                                      .LimitCritterPowerCategory)
                                                       .ConfigureAwait(false))
                                                .Select(imp => imp.ImprovedName)))
                {
                    _lstCategory.Add(new ListItem(strInnerText,
                                                  (await objXmlCategory
                                                         .SelectSingleNodeAndCacheExpressionAsync("@translate")
                                                         .ConfigureAwait(false))?.Value ?? strInnerText));
                    continue;
                }

                if ((await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                                    Improvement.ImprovementType.LimitCritterPowerCategory).ConfigureAwait(false))
                    .Any(imp => !strInnerText.Contains(imp.ImprovedName)))
                {
                    continue;
                }
                _lstCategory.Add(new ListItem(strInnerText,
                    objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
            }

            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }

            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else if (cboCategory.Items.Contains(_strSelectCategory))
            {
                cboCategory.SelectedValue = _strSelectCategory;
            }

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void trePowers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            await lblPowerPoints.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            await lblPowerPointsLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            string strSelectedPower = await trePowers.DoThreadSafeFuncAsync(x => x.SelectedNode.Tag?.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedPower))
            {
                XPathNavigator objXmlPower = _xmlBaseCritterPowerDataNode.TryGetNodeByNameOrId("powers/power", strSelectedPower);
                if (objXmlPower != null)
                {
                    string strCategory = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("category").ConfigureAwait(false))?.Value
                                         ?? string.Empty;
                    await lblCritterPowerCategory.DoThreadSafeAsync(x => x.Text = strCategory).ConfigureAwait(false);

                    string strType = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("type").ConfigureAwait(false))?.Value
                                     ?? string.Empty;
                    switch (strType)
                    {
                        case "M":
                            strType = await LanguageManager.GetStringAsync("String_SpellTypeMana").ConfigureAwait(false);
                            break;

                        case "P":
                            strType = await LanguageManager.GetStringAsync("String_SpellTypePhysical").ConfigureAwait(false);
                            break;
                    }
                    await lblCritterPowerType.DoThreadSafeAsync(x => x.Text = strType).ConfigureAwait(false);

                    string strAction = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("action").ConfigureAwait(false))?.Value ?? string.Empty;
                    switch (strAction)
                    {
                        case "Auto":
                            strAction = await LanguageManager.GetStringAsync("String_ActionAutomatic").ConfigureAwait(false);
                            break;

                        case "Free":
                            strAction = await LanguageManager.GetStringAsync("String_ActionFree").ConfigureAwait(false);
                            break;

                        case "Simple":
                            strAction = await LanguageManager.GetStringAsync("String_ActionSimple").ConfigureAwait(false);
                            break;

                        case "Complex":
                            strAction = await LanguageManager.GetStringAsync("String_ActionComplex").ConfigureAwait(false);
                            break;

                        case "Special":
                            strAction = await LanguageManager.GetStringAsync("String_SpellDurationSpecial").ConfigureAwait(false);
                            break;
                    }
                    await lblCritterPowerAction.DoThreadSafeAsync(x => x.Text = strAction).ConfigureAwait(false);

                    string strRange = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("range").ConfigureAwait(false))?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strRange))
                    {
                        strRange = await strRange.CheapReplaceAsync("Self",
                                                                    () => LanguageManager.GetStringAsync("String_SpellRangeSelf"))
                                                 .CheapReplaceAsync("Special",
                                                                    () => LanguageManager.GetStringAsync("String_SpellDurationSpecial"))
                                                 .CheapReplaceAsync("LOS", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfSight"))
                                                 .CheapReplaceAsync("LOI",
                                                                    () => LanguageManager.GetStringAsync("String_SpellRangeLineOfInfluence"))
                                                 .CheapReplaceAsync("Touch", () => LanguageManager.GetStringAsync("String_SpellRangeTouchLong"))
                                                 .CheapReplaceAsync("T", () => LanguageManager.GetStringAsync("String_SpellRangeTouch"))
                                                 .CheapReplaceAsync("(A)", async () => '(' + await LanguageManager.GetStringAsync("String_SpellRangeArea").ConfigureAwait(false) + ')')
                                                 .CheapReplaceAsync("MAG", () => LanguageManager.GetStringAsync("String_AttributeMAGShort")).ConfigureAwait(false);
                    }
                    await lblCritterPowerRange.DoThreadSafeAsync(x => x.Text = strRange).ConfigureAwait(false);

                    string strDuration = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("duration").ConfigureAwait(false))?.Value ?? string.Empty;
                    switch (strDuration)
                    {
                        case "Instant":
                            strDuration = await LanguageManager.GetStringAsync("String_SpellDurationInstantLong").ConfigureAwait(false);
                            break;

                        case "Sustained":
                            strDuration = await LanguageManager.GetStringAsync("String_SpellDurationSustained").ConfigureAwait(false);
                            break;

                        case "Always":
                            strDuration = await LanguageManager.GetStringAsync("String_SpellDurationAlways").ConfigureAwait(false);
                            break;

                        case "Special":
                            strDuration = await LanguageManager.GetStringAsync("String_SpellDurationSpecial").ConfigureAwait(false);
                            break;
                    }
                    await lblCritterPowerDuration.DoThreadSafeAsync(x => x.Text = strDuration).ConfigureAwait(false);

                    string strSource = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("source").ConfigureAwait(false))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strPage = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("altpage").ConfigureAwait(false))?.Value ?? (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("page").ConfigureAwait(false))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSource.SetControlAsync(lblCritterPowerSource).ConfigureAwait(false);

                    bool blnVisible = await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false) != null;
                    await nudCritterPowerRating.DoThreadSafeAsync(x => { x.Visible = blnVisible; x.Enabled = blnVisible; }).ConfigureAwait(false);

                    string strKarma = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("karma").ConfigureAwait(false))?.Value
                                      ?? "0";
                    await lblKarma.DoThreadSafeAsync(x => x.Text = strKarma).ConfigureAwait(false);

                    // If the character is a Free Spirit, populate the Power Points Cost as well.
                    if (_objCharacter.Metatype == "Free Spirit")
                    {
                        XPathNavigator xmlOptionalPowerCostNode = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers/power[. = " + (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false))?.Value.CleanXPath() + "]/@cost");
                        if (xmlOptionalPowerCostNode != null)
                        {
                            await lblPowerPoints.DoThreadSafeAsync(x =>
                            {
                                x.Text = xmlOptionalPowerCostNode.Value;
                                x.Visible = true;
                            }).ConfigureAwait(false);
                            await lblPowerPointsLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                        }
                    }

                    await lblCritterPowerTypeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strType)).ConfigureAwait(false);
                    await lblCritterPowerActionLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAction)).ConfigureAwait(false);
                    await lblCritterPowerRangeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strRange)).ConfigureAwait(false);
                    await lblCritterPowerDurationLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDuration)).ConfigureAwait(false);
                    await lblCritterPowerSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString())).ConfigureAwait(false);
                    await lblKarmaLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strKarma)).ConfigureAwait(false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            }
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RefreshCategory().ConfigureAwait(false);
        }

        private async ValueTask RefreshCategory(CancellationToken token = default)
        {
            await trePowers.DoThreadSafeAsync(x => x.Nodes.Clear(), token: token).ConfigureAwait(false);

            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);

            List<string> lstPowerWhitelist = new List<string>(10);

            // If the Critter is only allowed certain Powers, display only those.
            XPathNavigator xmlOptionalPowers = await _xmlMetatypeDataNode.SelectSingleNodeAndCacheExpressionAsync("optionalpowers", token: token).ConfigureAwait(false);
            if (xmlOptionalPowers != null)
            {
                foreach (XPathNavigator xmlNode in xmlOptionalPowers.SelectAndCacheExpression("power", token: token))
                    lstPowerWhitelist.Add(xmlNode.Value);

                // Determine if the Critter has a physical presence Power (Materialization, Possession, or Inhabitation).
                bool blnPhysicalPresence = await (await _objCharacter.GetCritterPowersAsync(token).ConfigureAwait(false))
                                                 .AnyAsync(
                                                     x => x.Name == "Materialization" || x.Name == "Possession"
                                                         || x.Name == "Inhabitation", token).ConfigureAwait(false);

                // Add any Critter Powers the Critter comes with that have been manually deleted so they can be re-added.
                foreach (XPathNavigator objXmlCritterPower in _xmlMetatypeDataNode.SelectAndCacheExpression("powers/power", token: token))
                {
                    bool blnAddPower = true;
                    // Make sure the Critter doesn't already have the Power.
                    await (await _objCharacter.GetCritterPowersAsync(token).ConfigureAwait(false)).ForEachWithBreakAsync(
                        objCheckPower =>
                        {
                            string strCheckPowerName = objCheckPower.Name;
                            if (strCheckPowerName == objXmlCritterPower.Value)
                            {
                                blnAddPower = false;
                                return false;
                            }

                            if ((strCheckPowerName == "Materialization" || strCheckPowerName == "Possession"
                                                                        || strCheckPowerName == "Inhabitation")
                                && blnPhysicalPresence)
                            {
                                blnAddPower = false;
                                return false;
                            }

                            return true;
                        }, token).ConfigureAwait(false);

                    if (blnAddPower)
                    {
                        lstPowerWhitelist.Add(objXmlCritterPower.Value);

                        // If Manifestation is one of the Powers, also include Inhabitation and Possess if they're not already in the list.
                        if (!blnPhysicalPresence && objXmlCritterPower.Value == "Materialization")
                        {
                            bool blnFoundPossession = false;
                            bool blnFoundInhabitation = false;
                            foreach (string strCheckPower in lstPowerWhitelist)
                            {
                                switch (strCheckPower)
                                {
                                    case "Possession":
                                        blnFoundPossession = true;
                                        break;

                                    case "Inhabitation":
                                        blnFoundInhabitation = true;
                                        break;
                                }

                                if (blnFoundInhabitation && blnFoundPossession)
                                    break;
                            }
                            if (!blnFoundPossession)
                            {
                                lstPowerWhitelist.Add("Possession");
                            }
                            if (!blnFoundInhabitation)
                            {
                                lstPowerWhitelist.Add("Inhabitation");
                            }
                        }
                    }
                }
            }

            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All")
                {
                    sbdFilter.Append(" and (contains(category,").Append(strCategory.CleanXPath()).Append("))");
                }
                else
                {
                    bool blnHasToxic = false;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                        {
                            if (!string.IsNullOrEmpty(strItem))
                            {
                                sbdCategoryFilter.Append("(contains(category,").Append(strItem.CleanXPath())
                                                 .Append(")) or ");
                                if (strItem == "Toxic Critter Powers")
                                {
                                    sbdCategoryFilter.Append("toxic = ").Append(bool.TrueString.CleanXPath()).Append(" or ");
                                    blnHasToxic = true;
                                }
                            }
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                        }
                    }

                    if (!blnHasToxic)
                        sbdFilter.Append(" and (not(toxic) or toxic != ").Append(bool.TrueString.CleanXPath()).Append(')');
                }

                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            foreach (XPathNavigator objXmlPower in _xmlBaseCritterPowerDataNode.Select("powers/power" + strFilter))
            {
                string strPowerName = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                if (!lstPowerWhitelist.Contains(strPowerName) && lstPowerWhitelist.Count != 0)
                    continue;
                if (!await objXmlPower.RequirementsMetAsync(_objCharacter, string.Empty, string.Empty, token: token).ConfigureAwait(false)) continue;
                TreeNode objNode = new TreeNode
                {
                    Tag = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("id", token: token).ConfigureAwait(false))?.Value ?? string.Empty,
                    Text = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value ?? strPowerName
                };
                await trePowers.DoThreadSafeAsync(x => x.Nodes.Add(objNode), token: token).ConfigureAwait(false);
            }
            await trePowers.DoThreadSafeAsync(x => x.Sort(), token: token).ConfigureAwait(false);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshCategory().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedPower = trePowers.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedPower))
                return;

            XPathNavigator objXmlPower = _xmlBaseCritterPowerDataNode.TryGetNodeByNameOrId("powers/power", strSelectedPower);
            if (objXmlPower == null)
                return;

            if (nudCritterPowerRating.Visible)
                _intSelectedRating = nudCritterPowerRating.ValueAsInt;

            _strSelectCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
            _strSelectedPower = strSelectedPower;

            // If the character is a Free Spirit (PC, not the Critter version), populate the Power Points Cost as well.
            if (_objCharacter.Metatype == "Free Spirit" && !_objCharacter.IsCritter)
            {
                string strName = objXmlPower.SelectSingleNodeAndCacheExpression("name")?.Value;
                if (!string.IsNullOrEmpty(strName))
                {
                    XPathNavigator objXmlOptionalPowerCost = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers/power[. = " + strName.CleanXPath() + "]/@cost");
                    if (objXmlOptionalPowerCost != null)
                        _decPowerPoints = Convert.ToDecimal(objXmlOptionalPowerCost.Value, GlobalSettings.InvariantCultureInfo);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Criter Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower => _strSelectedPower;

        /// <summary>
        /// Rating for the Critter Power that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => _intSelectedRating;

        /// <summary>
        /// Power Point cost for the Critter Power (only applies to Free Spirits).
        /// </summary>
        public decimal PowerPoints => _decPowerPoints;

        #endregion Properties
    }
}
