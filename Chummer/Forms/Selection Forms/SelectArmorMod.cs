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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Enums;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectArmorMod : Form
    {
        private bool _blnLoading = true;
        private readonly XPathNavigator _objParentNode;
        private readonly XPathNavigator _xmlBaseDataNode;
        private readonly Character _objCharacter;
        private readonly Armor _objArmor;
        private HashSet<string> _setBlackMarketMaps;

        private decimal _decMarkup;
        private bool _blnFreeCost;

        #region Control Events

        public SelectArmorMod(Character objCharacter, Armor objParentNode = null)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Prevent Enter key from closing the form when NumericUpDown controls have focus
            nudMinimumCost.KeyDown += NumericUpDown_KeyDown;
            nudMaximumCost.KeyDown += NumericUpDown_KeyDown;
            nudExactCost.KeyDown += NumericUpDown_KeyDown;
            // Load the Armor information.
            _xmlBaseDataNode = _objCharacter.LoadDataXPath("armor.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _objArmor = objParentNode;
            if (_objArmor != null)
            {
                _strCachedParentRating = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(
                    async () => (await _objArmor.GetRatingAsync().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                    Utils.JoinableTaskFactory);
                _strCachedParentOwnCost = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(
                    async () => (await _objArmor.GetOwnCostAsync().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                    Utils.JoinableTaskFactory);
                _strCachedParentOwnWeight = new Lazy<string>(() => _objArmor.OwnWeight.ToString(GlobalSettings.InvariantCultureInfo));
                _strCachedParentCapacity = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(
                    () => _objArmor.TotalArmorCapacityAsync(GlobalSettings.InvariantCultureInfo), Utils.JoinableTaskFactory);
                _objParentNode = _objArmor.GetNodeXPath();
            }
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            if (_xmlBaseDataNode != null)
            {
                _setBlackMarketMaps.AddRange(
                    _objCharacter.GenerateBlackMarketMappings(
                        _xmlBaseDataNode.SelectSingleNodeAndCacheExpression("modcategories")));
            }
        }

        private async void SelectArmorMod_Load(object sender, EventArgs e)
        {
            bool blnBlackMarketDiscount = await _objCharacter.GetBlackMarketDiscountAsync().ConfigureAwait(false);
            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = blnBlackMarketDiscount).ConfigureAwait(false);

            if (await _objCharacter.GetCreatedAsync().ConfigureAwait(false))
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                }).ConfigureAwait(false);
            }
            else
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                int intMaxAvail = await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).GetMaximumAvailabilityAsync().ConfigureAwait(false);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                    x.Visible = true;
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }
            _blnLoading = false;
            await RefreshList().ConfigureAwait(false);
        }

        private async void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSelectedArmor().ConfigureAwait(false);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateSelectedArmor().ConfigureAwait(false);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                await RefreshList().ConfigureAwait(false);
            }
            await UpdateSelectedArmor().ConfigureAwait(false);
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                await RefreshList().ConfigureAwait(false);
            }
            await UpdateSelectedArmor().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Whether the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Name of Accessory that was selected in the dialogue.
        /// </summary>
        public string SelectedArmorMod { get; private set; } = string.Empty;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating { get; private set; }

        /// <summary>
        /// Categories that the Armor allows to be used.
        /// </summary>
        public string AllowedCategories { get; set; } = string.Empty;

        /// <summary>
        /// Whether the General category should be included.
        /// </summary>
        public bool ExcludeGeneralCategory { get; set; }

        /// <summary>
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        #endregion Properties

        #region Methods

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateSelectedArmor().ConfigureAwait(false);
        }

        /// <summary>
        /// Update the information for the selected Armor Mod.
        /// </summary>
        private async Task UpdateSelectedArmor(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstMod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            XPathNavigator objXmlMod = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlMod = _xmlBaseDataNode.TryGetNodeByNameOrId("/chummer/mods/mod", strSelectedId);
            if (objXmlMod == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                return;
            }
            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
            try
            {
                // Extract the Avil and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.
                string strArmor = objXmlMod.SelectSingleNodeAndCacheExpression("armor", token)?.Value;
                await lblA.DoThreadSafeAsync(x => x.Text = strArmor, token: token).ConfigureAwait(false);
                await lblALabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strArmor), token: token).ConfigureAwait(false);

                string strRatingLabel = objXmlMod.SelectSingleNodeAndCacheExpression("ratinglabel", token)?.Value;
                strRatingLabel = !string.IsNullOrEmpty(strRatingLabel)
                    ? string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Label_RatingFormat", token: token).ConfigureAwait(false),
                                    await LanguageManager.GetStringAsync(strRatingLabel, token: token).ConfigureAwait(false))
                    : await LanguageManager.GetStringAsync("Label_Rating", token: token).ConfigureAwait(false);
                await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token).ConfigureAwait(false);

                string strExpression = objXmlMod.SelectSingleNodeAndCacheExpression("maxrating", token)?.Value ?? string.Empty;
                if (strExpression == "0")
                    strExpression = string.Empty;
                int intMaxRating = int.MaxValue;
                if (!string.IsNullOrEmpty(strExpression))
                {
                    intMaxRating = (await ProcessInvariantXPathExpression(strExpression, 0, token).ConfigureAwait(false)).Item1.StandardRound();
                }

                if (intMaxRating > 0 && intMaxRating != int.MaxValue)
                {
                    await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaxRating, token: token).ConfigureAwait(false);
                    if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    {
                        int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token).ConfigureAwait(false);
                        while (intMaximum > 1 && !await objXmlMod.CheckAvailRestrictionAsync(_objCharacter, intMaximum, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlMod.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
                        {
                            --intMaximum;
                        }
                        await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                    }

                    if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    {
                        decimal decCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                        if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                            decCostMultiplier *= 0.9m;
                        int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token).ConfigureAwait(false);
                        decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                        while (intMaximum > 1 && !await objXmlMod.CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier, intMaximum, token: token).ConfigureAwait(false))
                        {
                            --intMaximum;
                        }
                        await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                    }

                    if (await nudRating.DoThreadSafeFuncAsync(x => x.Maximum, token: token).ConfigureAwait(false) <= 1)
                    {
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Visible = false;
                            x.Enabled = false;
                        }, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Enabled = x.Minimum != x.Maximum;
                            if (x.Minimum == 0)
                            {
                                x.Value = 1;
                                x.Minimum = 1;
                            }
                            x.Visible = true;
                        }, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    }
                }
                else
                {
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Enabled = false;
                    }, token: token).ConfigureAwait(false);
                }
                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);
                string strAvail = await new AvailabilityValue(intRating,
                    objXmlMod.SelectSingleNodeAndCacheExpression("avail", token)?.Value,
                    (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlMod.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound()).ToStringAsync(token).ConfigureAwait(false);
                await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
                await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token).ConfigureAwait(false);

                // Cost.
                bool blnCanBlackMarketDiscount
                    = _setBlackMarketMaps.Contains(objXmlMod.SelectSingleNodeAndCacheExpression("category", token)?.Value);
                await chkBlackMarketDiscount.DoThreadSafeAsync(x =>
                {
                    x.Enabled = blnCanBlackMarketDiscount;
                    if (!x.Checked)
                    {
                        x.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                    }
                    else if (!blnCanBlackMarketDiscount)
                    {
                        //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                        x.Checked = false;
                    }
                }, token: token).ConfigureAwait(false);

                string strNuyenSymbol = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                {
                    string strCost = 0.0m.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo) + strNuyenSymbol;
                    await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                    string strTest = await _objCharacter.AvailTestAsync(0, strAvail, token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                }
                else
                {
                    string strCostElement = objXmlMod.SelectSingleNodeAndCacheExpression("cost", token)?.Value ?? string.Empty;
                    strCostElement = strCostElement.ProcessFixedValuesString(intRating);
                    if (strCostElement.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        string strFirstHalf = strCostElement.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        string strSecondHalf = string.Empty;
                        int intHyphenIndex = strFirstHalf.IndexOf('-');
                        if (intHyphenIndex != -1)
                        {
                            if (intHyphenIndex + 1 < strFirstHalf.Length)
                                strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                            strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                        }
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        if (intHyphenIndex != -1)
                        {
                            decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                            decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
                        }
                        else
                            decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

                        string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                        string strCost = decMax == decimal.MaxValue
                            ? decMin.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false) + "+"
                            : decMin.ToString(strNuyenFormat, GlobalSettings.CultureInfo)
                              + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + "-"
                              + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false)
                              + decMax.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                        string strTest = await _objCharacter.AvailTestAsync(decMin, strAvail, token).ConfigureAwait(false);
                        await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        decimal decCost = (await ProcessInvariantXPathExpression(strCostElement, intRating, token).ConfigureAwait(false)).Item1;
                        decCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                        string strCost = decCost.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo) + strNuyenSymbol;
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                        string strTest = await _objCharacter.AvailTestAsync(decCost, strAvail, token).ConfigureAwait(false);
                        await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    }
                }

                bool blnShowCost = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);
                bool blnShowTest = !string.IsNullOrEmpty(await lblTest.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                await lblTestLabel.DoThreadSafeAsync(x => x.Visible = blnShowTest, token: token).ConfigureAwait(false);

                // Capacity.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objXmlMod.SelectSingleNodeAndCacheExpression("armorcapacity", token)?.Value;

                // Handle YNT Softweave
                if (string.IsNullOrEmpty(strCapacity) || _objArmor?.CapacityDisplayStyle != CapacityStyle.Standard)
                    await lblCapacity.DoThreadSafeAsync(x => x.Text = "[" + 0.ToString(GlobalSettings.CultureInfo) + "]", token: token).ConfigureAwait(false);
                else
                {
                    strCapacity = strCapacity.ProcessFixedValuesString(intRating);

                    bool blnSquareBrackets = strCapacity.StartsWith('[');
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(strCapacity, intRating, token).ConfigureAwait(false);
                    string strReturn = blnIsSuccess ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo) : strCapacity;
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";

                    await lblCapacity.DoThreadSafeAsync(x => x.Text = strReturn, token: token).ConfigureAwait(false);
                }

                bool blnShowCapacity = !string.IsNullOrEmpty(await lblCapacity.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                await lblCapacityLabel.DoThreadSafeAsync(x => x.Visible = blnShowCapacity, token: token).ConfigureAwait(false);

                string strSource = objXmlMod.SelectSingleNodeAndCacheExpression("source", token)?.Value
                                   ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                string strPage = objXmlMod.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value
                                 ?? objXmlMod.SelectSingleNodeAndCacheExpression("page", token)?.Value
                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                SourceString objSource = await SourceString.GetSourceStringAsync(
                    strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter, token: token).ConfigureAwait(false);
                await objSource.SetControlAsync(lblSource, this, token: token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()), token: token).ConfigureAwait(false);
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout(), CancellationToken.None).ConfigureAwait(false);
            }
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
        }

        private async void CostFilter(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            await RefreshList().ConfigureAwait(false);
        }

        private void NumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private async Task RefreshList(CancellationToken token = default)
        {
            string strFilter = string.Empty;
            // Populate the Mods list.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstMods))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    sbdFilter.Append('(',
                        await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false), ')');
                    using (new FetchSafelyFromObjectPool<StringBuilder>(
                               Utils.StringBuilderPool, out StringBuilder sbdCategoryFilter))
                    {
                        if (!ExcludeGeneralCategory)
                            sbdCategoryFilter.Append("category = \"General\" or ");
                        foreach (string strCategory in AllowedCategories.SplitNoAlloc(
                                     ',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!string.IsNullOrEmpty(strCategory))
                                sbdCategoryFilter.Append("category = ", strCategory.CleanXPath(), " or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (", sbdCategoryFilter.ToString(), ')');
                        }
                    }

                    if (!string.IsNullOrEmpty(txtSearch.Text))
                        sbdFilter.Append(" and ", CommonFunctions.GenerateSearchXPath(txtSearch.Text));
                    if (sbdFilter.Length > 0)
                        strFilter = sbdFilter.Insert(0, '[').Append(']').ToString();
                }

                int intOverLimit = 0;
                XPathNodeIterator objXmlModList = _xmlBaseDataNode.Select("/chummer/mods/mod" + strFilter);
                if (objXmlModList.Count > 0)
                {
                    bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                    bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                    bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                    decimal decNuyen = blnFreeItem || !blnShowOnlyAffordItems ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                    foreach (XPathNavigator objXmlMod in objXmlModList)
                    {
                        XPathNavigator xmlTestNode
                            = objXmlMod.SelectSingleNodeAndCacheExpression("forbidden/parentdetails", token: token);
                        if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = objXmlMod.SelectSingleNodeAndCacheExpression("required/parentdetails", token: token);
                        if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        string strId = objXmlMod.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                        if (string.IsNullOrEmpty(strId)) continue;
                        decimal decCostMultiplier = 1 + nudMarkup.Value / 100.0m;
                        if (_setBlackMarketMaps.Contains(
                                objXmlMod.SelectSingleNodeAndCacheExpression("category", token: token)?.Value))
                            decCostMultiplier *= 0.9m;
                        if (!blnHideOverAvailLimit || await objXmlMod.CheckAvailRestrictionAsync(_objCharacter, intAvailModifier: (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlMod.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false) &&
                            (blnFreeItem || !blnShowOnlyAffordItems ||
                             await objXmlMod.CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier, token: token).ConfigureAwait(false))
                            && await objXmlMod.RequirementsMetAsync(_objCharacter, _objArmor, token: token).ConfigureAwait(false))
                        {
                            // Apply post-processing cost filtering to handle complex cost formulas
                            bool blnPassesCostFilter = true;
                            decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                            decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                            decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

                            // Check cost filters using the centralized method
                            blnPassesCostFilter = await CommonFunctions.CheckCostFilterAsync(objXmlMod, _objCharacter, 
                                _objArmor, decCostMultiplier, 1, decMinimumCost, decMaximumCost, decExactCost, token).ConfigureAwait(false);

                            if (blnPassesCostFilter)
                            {
                                lstMods.Add(new ListItem(
                                                strId,
                                                objXmlMod.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                ?? objXmlMod.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                            }
                        }
                        else
                            ++intOverLimit;
                    }
                }

                lstMods.Sort(CompareListItems.CompareNames);
                if (intOverLimit > 0)
                {
                    // Add after sort so that it's always at the end
                    lstMods.Add(new ListItem(string.Empty,
                                             string.Format(GlobalSettings.CultureInfo,
                                                           await LanguageManager.GetStringAsync("String_RestrictedItemsHidden", token: token).ConfigureAwait(false),
                                                           intOverLimit)));
                }

                string strOldSelected = await lstMod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                _blnLoading = true;
                await lstMod.PopulateWithListItemsAsync(lstMods, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstMod.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstMod.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                SelectedArmorMod = strSelectedId;
                SelectedRating = nudRating.ValueAsInt;
                _decMarkup = nudMarkup.Value;
                _blnFreeCost = chkFreeItem.Checked;
                BlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        private readonly Microsoft.VisualStudio.Threading.AsyncLazy<string> _strCachedParentRating;
        private readonly Microsoft.VisualStudio.Threading.AsyncLazy<string> _strCachedParentOwnCost;
        private readonly Lazy<string> _strCachedParentOwnWeight;
        private readonly Microsoft.VisualStudio.Threading.AsyncLazy<string> _strCachedParentCapacity;

        private async Task<ValueTuple<decimal, bool>> ProcessInvariantXPathExpression(string strExpression, int intRating, CancellationToken token = default)
        {
            return await CommonFunctions.ProcessInvariantXPathExpressionAsync(
                strExpression, 
                intRating, 
                _objCharacter, 
                _objArmor, 
                token: token).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
