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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectArmorMod : Form
    {
        private bool _blnLoading = true;
        private decimal _decArmorCapacity;
        private decimal _decArmorCost;
        private CapacityStyle _eCapacityStyle = CapacityStyle.Standard;
        private readonly XPathNavigator _objParentNode;
        private readonly XPathNavigator _xmlBaseDataNode;
        private readonly Character _objCharacter;
        private readonly Armor _objArmor;
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectArmorMod(Character objCharacter, Armor objParentNode = null)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            // Load the Armor information.
            _xmlBaseDataNode = _objCharacter.LoadDataXPath("armor.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _objArmor = objParentNode;
            _objParentNode = _objArmor?.GetNodeXPath();
            if (_xmlBaseDataNode != null)
                _setBlackMarketMaps.AddRange(
                    _objCharacter.GenerateBlackMarketMappings(
                        _xmlBaseDataNode.SelectSingleNodeAndCacheExpression("modcategories")));
        }

        private async void SelectArmorMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                });
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = true);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true);
            }
            else
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(
                        GlobalSettings.CultureInfo, x.Text,
                        _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                });
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false);
            }
            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);
            _blnLoading = false;
            await RefreshList();
        }

        private async void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSelectedArmor();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateSelectedArmor();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await RefreshList();
            }
            await UpdateSelectedArmor();
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await RefreshList();
            }
            await UpdateSelectedArmor();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public decimal ArmorCost
        {
            set => _decArmorCost = value;
        }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public decimal ArmorCapacity
        {
            set => _decArmorCapacity = value;
        }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Name of Accessory that was selected in the dialogue.
        /// </summary>
        public string SelectedArmorMod { get; private set; } = string.Empty;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => nudRating.ValueAsInt;

        /// <summary>
        /// Categories that the Armor allows to be used.
        /// </summary>
        public string AllowedCategories { get; set; } = string.Empty;

        /// <summary>
        /// Whether or not the General category should be included.
        /// </summary>
        public bool ExcludeGeneralCategory { get; set; }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup { get; private set; }

        /// <summary>
        /// Capacity display style.
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            set => _eCapacityStyle = value;
        }

        #endregion Properties

        #region Methods

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateSelectedArmor();
        }

        /// <summary>
        /// Update the information for the selected Armor Mod.
        /// </summary>
        private async ValueTask UpdateSelectedArmor()
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstMod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            XPathNavigator objXmlMod = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlMod = _xmlBaseDataNode.SelectSingleNode("/chummer/mods/mod[id = " + strSelectedId.CleanXPath() + ']');
            if (objXmlMod == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                return;
            }
            await this.DoThreadSafeAsync(x => x.SuspendLayout());
            try
            {
                // Extract the Avil and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.
                string strArmor = objXmlMod.SelectSingleNode("armor")?.Value;
                await lblA.DoThreadSafeAsync(x => x.Text = strArmor);
                await lblALabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strArmor));

                string strRatingLabel = objXmlMod.SelectSingleNode("ratinglabel")?.Value;
                strRatingLabel = !string.IsNullOrEmpty(strRatingLabel)
                    ? string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Label_RatingFormat"),
                                    await LanguageManager.GetStringAsync(strRatingLabel))
                    : await LanguageManager.GetStringAsync("Label_Rating");
                await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel);
                decimal decMaximum = Convert.ToDecimal(objXmlMod.SelectSingleNode("maxrating")?.Value,
                                                       GlobalSettings.InvariantCultureInfo);
                await nudRating.DoThreadSafeAsync(x => x.Maximum = decMaximum);
                if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked))
                {
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        while (x.Maximum > 1 && !objXmlMod.CheckAvailRestriction(_objCharacter, x.MaximumAsInt))
                        {
                            --x.Maximum;
                        }
                    });
                }

                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                {
                    decimal decCostMultiplier = 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                    if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        while (x.Maximum > 1 && !objXmlMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, x.MaximumAsInt))
                        {
                            --x.Maximum;
                        }
                    });
                }

                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true);
                if (nudRating.Maximum <= 1)
                {
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true);
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Enabled = false;
                    });
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
                    });
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false);
                }

                string strAvail = new AvailabilityValue(Convert.ToInt32(nudRating.Value),
                                                        objXmlMod.SelectSingleNode("avail")?.Value).ToString();
                await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail);
                await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail));

                // Cost.
                bool blnCanBlackMarketDiscount
                    = _setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")?.Value);
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
                });

                object objProcess;
                bool blnIsSuccess;
                if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                {
                    await lblCost.DoThreadSafeAsync(x => x.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                                                  + LanguageManager.GetString("String_NuyenSymbol"));
                    await lblTest.DoThreadSafeAsync(x => x.Text = _objCharacter.AvailTest(0, strAvail));
                }
                else
                {
                    string strCostElement = objXmlMod.SelectSingleNode("cost")?.Value ?? string.Empty;
                    if (strCostElement.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string strSuffix = string.Empty;
                        if (!strCostElement.EndsWith(')'))
                        {
                            strSuffix = strCostElement.Substring(strCostElement.LastIndexOf(')') + 1);
                            strCostElement = strCostElement.TrimEndOnce(strSuffix);
                        }

                        string[] strValues = strCostElement.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                           .Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strCostElement
                            = strValues[Math.Max(Math.Min(Convert.ToInt32(nudRating.Value), strValues.Length) - 1, 0)];
                        strCostElement += strSuffix;
                    }

                    if (strCostElement.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        string strCost = strCostElement.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        strCost = decMax == decimal.MaxValue
                            ? decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol") + '+'
                            : decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                              + await LanguageManager.GetStringAsync("String_Space") + '-'
                              + await LanguageManager.GetStringAsync("String_Space")
                              + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol");
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost);
                        await lblTest.DoThreadSafeAsync(x => x.Text = _objCharacter.AvailTest(decMin, strAvail));
                    }
                    else
                    {
                        string strCost = await (await strCostElement.CheapReplaceAsync(
                                "Rating", () => nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)))
                            .CheapReplaceAsync("Armor Cost",
                                               () => _decArmorCost.ToString(GlobalSettings.InvariantCultureInfo));

                        // Apply any markup.
                        (blnIsSuccess, objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost);
                        decimal decCost = blnIsSuccess
                            ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo)
                            : 0;
                        decCost *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);

                        await lblCost.DoThreadSafeAsync(x => x.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                                                      + LanguageManager.GetString("String_NuyenSymbol"));

                        await lblTest.DoThreadSafeAsync(x => x.Text = _objCharacter.AvailTest(decCost, strAvail));
                    }
                }

                await lblCost.DoThreadSafeFuncAsync(x => x.Text)
                             .ContinueWith(
                                 y => lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result)))
                             .Unwrap();
                await lblTest.DoThreadSafeFuncAsync(x => x.Text)
                             .ContinueWith(
                                 y => lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result)))
                             .Unwrap();

                // Capacity.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objXmlMod.SelectSingleNode("armorcapacity")?.Value;

                // Handle YNT Softweave
                if (_eCapacityStyle == CapacityStyle.Zero || string.IsNullOrEmpty(strCapacity))
                    await lblCapacity.DoThreadSafeAsync(x => x.Text = '[' + 0.ToString(GlobalSettings.CultureInfo) + ']');
                else
                {
                    if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strCapacity = strValues[nudRating.ValueAsInt - 1];
                    }

                    strCapacity = await (await strCapacity.CheapReplaceAsync(
                            "Capacity", () => _decArmorCapacity.ToString(GlobalSettings.InvariantCultureInfo)))
                        .CheapReplaceAsync(
                            "Rating", () => nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo));
                    bool blnSquareBrackets = strCapacity.StartsWith('[');
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                    (blnIsSuccess, objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCapacity);
                    string strReturn = blnIsSuccess
                        ? ((double) objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                        : strCapacity;
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';

                    await lblCapacity.DoThreadSafeAsync(x => x.Text = strReturn);
                }

                await lblCapacity.DoThreadSafeFuncAsync(x => x.Text)
                                 .ContinueWith(
                                     y => lblCapacityLabel.DoThreadSafeAsync(
                                         x => x.Visible = !string.IsNullOrEmpty(y.Result))).Unwrap();

                string strSource = objXmlMod.SelectSingleNode("source")?.Value
                                   ?? await LanguageManager.GetStringAsync("String_Unknown");
                string strPage = (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value
                                 ?? objXmlMod.SelectSingleNode("page")?.Value
                                 ?? await LanguageManager.GetStringAsync("String_Unknown");
                SourceString objSource = await SourceString.GetSourceStringAsync(
                    strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                await objSource.SetControlAsync(lblSource);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()));
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true);
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout());
            }
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList();
        }

        /// <summary>
        ///
        /// </summary>
        private async ValueTask RefreshList()
        {
            string strFilter = string.Empty;
            // Populate the Mods list.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstMods))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                    using (new FetchSafelyFromPool<StringBuilder>(
                               Utils.StringBuilderPool, out StringBuilder sbdCategoryFilter))
                    {
                        if (!ExcludeGeneralCategory)
                            sbdCategoryFilter.Append("category = \"General\" or ");
                        foreach (string strCategory in AllowedCategories.SplitNoAlloc(
                                     ',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!string.IsNullOrEmpty(strCategory))
                                sbdCategoryFilter.Append("category = ").Append(strCategory.CleanXPath()).Append(" or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                        }
                    }

                    if (!string.IsNullOrEmpty(txtSearch.Text))
                        sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                    if (sbdFilter.Length > 0)
                        strFilter = '[' + sbdFilter.ToString() + ']';
                }

                int intOverLimit = 0;
                XPathNodeIterator objXmlModList = _xmlBaseDataNode.Select("/chummer/mods/mod" + strFilter);
                if (objXmlModList.Count > 0)
                {
                    bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked);
                    bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked);
                    bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked);
                    foreach (XPathNavigator objXmlMod in objXmlModList)
                    {
                        XPathNavigator xmlTestNode
                            = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("forbidden/parentdetails");
                        if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("required/parentdetails");
                        if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        string strId = (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value;
                        if (string.IsNullOrEmpty(strId)) continue;
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(
                                (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("category"))?.Value))
                            decCostMultiplier *= 0.9m;
                        if (!blnHideOverAvailLimit || objXmlMod.CheckAvailRestriction(_objCharacter) &&
                            (blnFreeItem || !blnShowOnlyAffordItems ||
                             objXmlMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier))
                            && objXmlMod.RequirementsMet(_objCharacter, _objArmor))
                        {
                            lstMods.Add(new ListItem(
                                            strId,
                                            (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                            ?? (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                            ?? await LanguageManager.GetStringAsync("String_Unknown")));
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
                                                           await LanguageManager.GetStringAsync("String_RestrictedItemsHidden"),
                                                           intOverLimit)));
                }

                string strOldSelected = await lstMod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                _blnLoading = true;
                await lstMod.PopulateWithListItemsAsync(lstMods);
                _blnLoading = false;
                await lstMod.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                });
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
                Markup = nudMarkup.Value;
                BlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
