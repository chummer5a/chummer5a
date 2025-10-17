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
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectVehicleMod : Form
    {
        private readonly Vehicle _objVehicle;
        private int _intWeaponMountSlots;
        private decimal _decMarkup;
        private bool _blnFreeCost;
        private bool _blnLoading = true;
        private bool _blnSkipUpdate;
        private static string _strSelectCategory = string.Empty;

        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private readonly string _strLimitToCategories = string.Empty;
        private List<ListItem> _lstCategory;
        private HashSet<string> _setBlackMarketMaps;

        #region Control Events

        public SelectVehicleMod(Character objCharacter, Vehicle objVehicle)
        {
            _objVehicle = objVehicle ?? throw new ArgumentNullException(nameof(objVehicle));
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Prevent Enter key from closing the form when NumericUpDown controls have focus
            nudMinimumCost.KeyDown += NumericUpDown_KeyDown;
            nudMaximumCost.KeyDown += NumericUpDown_KeyDown;
            nudExactCost.KeyDown += NumericUpDown_KeyDown;
            _lstCategory = Utils.ListItemListPool.Get();
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            if (_xmlBaseVehicleDataNode != null)
            {
                _setBlackMarketMaps.AddRange(
                    _objCharacter.GenerateBlackMarketMappings(
                        _xmlBaseVehicleDataNode.SelectSingleNodeAndCacheExpression("modcategories")));
            }
        }

        private async void SelectVehicleMod_Load(object sender, EventArgs e)
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
                    x.Text = string.Format( GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                    x.Visible = true;
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }

            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string>
                                                                                setValues))
            {
                foreach (string strCategory in _strLimitToCategories.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    setValues.Add(strCategory);

                // Populate the Category list.
                string strFilterPrefix = (VehicleMountMods
                    ? "weaponmountmods/mod[("
                    : "mods/mod[(") + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync().ConfigureAwait(false) + ") and category = ";
                foreach (XPathNavigator objXmlCategory in _xmlBaseVehicleDataNode.SelectAndCacheExpression("modcategories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    if ((string.IsNullOrEmpty(_strLimitToCategories) || setValues.Contains(strInnerText))
                        && _xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + "]") != null)
                    {
                        _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
                    }
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }
            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);
            await cboCategory.DoThreadSafeAsync(x =>
            {
                // Select the first Category in the list.
                if (!string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedValue = _strSelectCategory;
                if (x.SelectedIndex == -1 && _lstCategory.Count > 0)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);

            _blnLoading = false;
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private async void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo().ConfigureAwait(false);
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

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            _strSelectCategory = string.Empty;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await RefreshList().ConfigureAwait(false);
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await RefreshList().ConfigureAwait(false);
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstMod.SelectedIndex + 1 < lstMod.Items.Count:
                    lstMod.SelectedIndex++;
                    break;

                case Keys.Down:
                    {
                        if (lstMod.Items.Count > 0)
                        {
                            lstMod.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstMod.SelectedIndex >= 1:
                    lstMod.SelectedIndex--;
                    break;

                case Keys.Up:
                    {
                        if (lstMod.Items.Count > 0)
                        {
                            lstMod.SelectedIndex = lstMod.Items.Count - 1;
                        }

                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.TextLength, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// The slots taken up by a weapon mount to which the vehicle mod might be being added
        /// </summary>
        public int WeaponMountSlots
        {
            set => _intWeaponMountSlots = value;
        }

        /// <summary>
        /// Name of the Mod that was selected in the dialogue.
        /// </summary>
        public string SelectedMod { get; private set; } = string.Empty;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating { get; private set; }

        /// <summary>
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Is the mod being added to a vehicle weapon mount?
        /// </summary>
        public bool VehicleMountMods { get; set; }

        /// <summary>
        /// If the mod is being added to a vehicle weapon mount, the (prospective) cost of the weapon mount without any additional mods.
        /// </summary>
        public decimal ParentWeaponMountOwnCost { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Build the list of Mods.
        /// </summary>
        private async Task RefreshList(CancellationToken token = default)
        {
            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            string strFilter = "(" + await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false) + ")";
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (string.IsNullOrWhiteSpace(strSearch) || GlobalSettings.SearchInCategoryOnly))
                strFilter += " and category = " + strCategory.CleanXPath();
            /*
            else if (!string.IsNullOrEmpty(AllowedCategories))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdCategoryFilter))
                {
                    foreach (string strItem in _lstCategory.Select(x => x.Value))
                    {
                        if (!string.IsNullOrEmpty(strItem))
                            sbdCategoryFilter.Append("category = ", strItem.CleanXPath(), " or ");
                    }
                    if (sbdCategoryFilter.Length > 0)
                    {
                        sbdCategoryFilter.Length -= 4;
                        strFilter = sbdCategoryFilter.Insert(0, strFilter, " and (", ')').ToString();
                    }
                }
            }
            */

            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            // Retrieve the list of Mods for the selected Category.
            XPathNodeIterator objXmlModList = VehicleMountMods
                ? _xmlBaseVehicleDataNode.Select("weaponmountmods/mod[" + strFilter + "]")
                : _xmlBaseVehicleDataNode.Select("mods/mod[" + strFilter + "]");
            // Update the list of Mods based on the selected Category.
            int intOverLimit = 0;
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMods))
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                decimal decBaseCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                decimal decNuyen = blnFreeItem || !blnShowOnlyAffordItems ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                foreach (XPathNavigator objXmlMod in objXmlModList)
                {
                    if (!await _objVehicle.CheckModRequirementsAsync(objXmlMod, token).ConfigureAwait(false))
                        continue;

                    int intMinRating = 0;
                    string strMinRating = objXmlMod.SelectSingleNodeAndCacheExpression("minrating", token: token)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strMinRating))
                    {
                        intMinRating = (await ProcessInvariantXPathExpression(strMinRating, 0, token: token).ConfigureAwait(false)).Item1.StandardRound();
                    }

                    string strRating = objXmlMod.SelectSingleNodeAndCacheExpression("rating", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                        if (strRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                        {
                            intMinRating = Math.Min(intMinRating, Vehicle.MaxWheels);
                        }
                        //Used for the Armor modifications.
                        else if (strRating.Equals("body", StringComparison.OrdinalIgnoreCase))
                        {
                            intMinRating = Math.Min(intMinRating, await _objVehicle.GetTotalBodyAsync(token).ConfigureAwait(false));
                        }
                        //Used for Metahuman Adjustments.
                        else if (strRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                        {
                            intMinRating = Math.Min(intMinRating, await _objVehicle.GetTotalSeatsAsync(token).ConfigureAwait(false));
                        }
                        else
                        {
                            int intMaxRating = int.MaxValue;
                            if (!string.IsNullOrEmpty(strRating))
                                intMaxRating = (await ProcessInvariantXPathExpression(strRating, intMinRating, 0, token).ConfigureAwait(false)).Item1.StandardRound();
                            if (intMaxRating > 0 && intMaxRating != int.MaxValue)
                                intMinRating = Math.Min(intMinRating, intMaxRating);
                            else
                                intMinRating = 0;
                        }
                    }

                    decimal decCostMultiplier = decBaseCostMultiplier;

                    // Apply cost filtering
                    decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                    decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                    decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

                    if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNodeAndCacheExpression("category", token: token)?.Value))
                        decCostMultiplier *= 0.9m;
                    if (!blnHideOverAvailLimit || await objXmlMod.CheckAvailRestrictionAsync(_objCharacter, intMinRating, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlMod.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
                    {
                        // Use unified cost filtering with parent context
                        if (blnFreeItem || !blnShowOnlyAffordItems || await objXmlMod.CheckNuyenRestrictionAsync(
                                _objCharacter, decNuyen, decCostMultiplier, intMinRating, _objVehicle, 
                                decMinimumCost, decMaximumCost, decExactCost, token).ConfigureAwait(false))
                        {
                            lstMods.Add(new ListItem(objXmlMod.SelectSingleNodeAndCacheExpression("id", token: token)?.Value,
                                                     objXmlMod.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                     ?? objXmlMod.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                     ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                        }
                    }
                    else
                        ++intOverLimit;
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
                    if (string.IsNullOrEmpty(strOldSelected))
                        x.SelectedIndex = -1;
                    else
                        x.SelectedValue = strOldSelected;
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
                XPathNavigator xmlVehicleMod = _xmlBaseVehicleDataNode.TryGetNodeByNameOrId((VehicleMountMods ? "weaponmountmods" : "mods") + "/mod", strSelectedId);
                if (xmlVehicleMod != null)
                {
                    SelectedMod = strSelectedId;
                    SelectedRating = nudRating.ValueAsInt;
                    _decMarkup = nudMarkup.Value;
                    _blnFreeCost = chkFreeItem.Checked;
                    _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                    _strSelectCategory = GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0
                        ? cboCategory.SelectedValue?.ToString()
                        : xmlVehicleMod.SelectSingleNodeAndCacheExpression("category")?.Value;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        /// <summary>
        /// Update the Mod's information based on the Mod selected and current Rating.
        /// </summary>
        private async Task UpdateGearInfo(CancellationToken token = default)
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            _blnSkipUpdate = true;
            try
            {
                XPathNavigator xmlVehicleMod = null;
                string strSelectedId = await lstMod
                                             .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                             .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    // Retireve the information for the selected Mod.
                    // Filtering is also done on the Category in case there are non-unique names across categories.
                    xmlVehicleMod = _xmlBaseVehicleDataNode.TryGetNodeByNameOrId((VehicleMountMods ? "weaponmountmods" : "mods") + "/mod", strSelectedId);
                }

                if (xmlVehicleMod != null)
                {
                    bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(
                        xmlVehicleMod.SelectSingleNodeAndCacheExpression("category", token)?.Value);
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

                    // Slots (part 1, if we don't need a Rating)
                    int intExtraSlots = 0;
                    string strSlots
                        = xmlVehicleMod.SelectSingleNodeAndCacheExpression("slots", token)?.Value ?? string.Empty;
                    if (!strSlots.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        intExtraSlots = (await ProcessInvariantXPathExpression(strSlots, 0, token: token).ConfigureAwait(false)).Item1.StandardRound();
                        string strInnerText = intExtraSlots.ToString(GlobalSettings.CultureInfo);
                        await lblSlots.DoThreadSafeAsync(x => x.Text = strInnerText, token: token).ConfigureAwait(false);
                        await lblSlotsLabel
                              .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strInnerText), token: token)
                              .ConfigureAwait(false);
                    }

                    // Extract the Avail and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                    // This is done using XPathExpression.

                    int intMinRating = 0;
                    string strMinRating = xmlVehicleMod.SelectSingleNodeAndCacheExpression("minrating", token: token)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strMinRating))
                    {
                        intMinRating = (await ProcessInvariantXPathExpression(strMinRating, 0, token: token).ConfigureAwait(false)).Item1.StandardRound();
                    }

                    await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                    string strRating = xmlVehicleMod.SelectSingleNodeAndCacheExpression("rating", token)?.Value;
                    if (string.IsNullOrEmpty(strRating))
                    {
                        string strRatingLabel = await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                                                     .ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                        if (intMinRating > 0)
                        {
                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Minimum = intMinRating;
                                x.Maximum = intMinRating;
                                x.Visible = true;
                                x.Enabled = false;
                            }, token: token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                        }
                        else
                        {
                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Minimum = 0;
                                x.Maximum = 0;
                                x.Visible = false;
                            }, token: token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                        }
                    }
                    // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                    else if (strRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRatingLabel = await LanguageManager.GetStringAsync("Label_Qty", token: token)
                                                                     .ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Maximum = Vehicle.MaxWheels;
                            x.Visible = true;
                        }, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                              .ConfigureAwait(false);
                    }
                    //Used for the Armor modifications.
                    else if (strRating.Equals("body", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRatingLabel = await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                                                     .ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                        int intMaximum = await _objVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Maximum = intMaximum;
                            x.Visible = true;
                        }, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                              .ConfigureAwait(false);
                    }
                    //Used for Metahuman Adjustments.
                    else if (strRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRatingLabel = await LanguageManager.GetStringAsync("Label_Seats", token: token)
                                                                     .ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                        int intTotalSeats = await _objVehicle.GetTotalSeatsAsync(token).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Maximum = intTotalSeats;
                            x.Visible = true;
                        }, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                              .ConfigureAwait(false);
                    }
                    else
                    {
                        string strRatingLabel = await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                                                     .ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                        int intTempRating = int.MaxValue;
                        if (!string.IsNullOrEmpty(strRating))
                            intTempRating = (await ProcessInvariantXPathExpression(strRating, intMinRating, intExtraSlots, token).ConfigureAwait(false)).Item1.StandardRound();
                        if (intTempRating > 0 && intTempRating != int.MaxValue)
                        {
                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Maximum = intTempRating;
                                x.Visible = true;
                            }, token: token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                                  .ConfigureAwait(false);
                        }
                        else
                        {
                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Minimum = 0;
                                x.Maximum = 0;
                                x.Visible = false;
                            }, token: token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token)
                                                  .ConfigureAwait(false);
                        }
                    }

                    if (await nudRating.DoThreadSafeFuncAsync(x => x.Maximum, token: token).ConfigureAwait(false) != 0)
                    {
                        int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token)
                                                            .ConfigureAwait(false);

                        if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                       .ConfigureAwait(false))
                        {
                            while (intMaximum > intMinRating && !await xmlVehicleMod
                                                                       .CheckAvailRestrictionAsync(
                                                                           _objCharacter, intMaximum, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: xmlVehicleMod.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token)
                                                                       .ConfigureAwait(false))
                            {
                                --intMaximum;
                            }
                        }

                        if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                        .ConfigureAwait(false) && !await chkFreeItem
                                .DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            decimal decCostMultiplier
                                = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                    .ConfigureAwait(false) / 100.0m;
                            if (_setBlackMarketMaps.Contains(
                                    xmlVehicleMod.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                                decCostMultiplier *= 0.9m;
                            decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                            while (intMaximum > 1 && !await xmlVehicleMod
                                                            .CheckNuyenRestrictionAsync(
                                                                _objCharacter, decNuyen, decCostMultiplier, intMaximum,
                                                                token).ConfigureAwait(false))
                            {
                                --intMaximum;
                            }
                        }

                        int intMinimum = intMinRating;
                        if (intMinimum <= 0)
                            intMinimum = Math.Min(1, intMaximum);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = intMinimum;
                            x.Maximum = Math.Max(intMinimum, intMaximum);
                            x.Enabled = x.Maximum != x.Minimum;
                        }, token: token).ConfigureAwait(false);
                    }

                    int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token)
                                                   .ConfigureAwait(false);

                    // Slots (part 2, if we do need a rating)
                    if (strSlots.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        intExtraSlots = (await ProcessInvariantXPathExpression(strSlots, intRating, token: token).ConfigureAwait(false)).Item1.StandardRound();
                        string strInnerText = intExtraSlots.ToString(GlobalSettings.CultureInfo);
                        await lblSlots.DoThreadSafeAsync(x => x.Text = strInnerText, token: token).ConfigureAwait(false);
                        await lblSlotsLabel
                              .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strInnerText), token: token)
                              .ConfigureAwait(false);
                    }

                    // Avail.
                    string strAvail
                        = await new AvailabilityValue(
                                intRating,
                                xmlVehicleMod.SelectSingleNodeAndCacheExpression("avail", token)?.Value ?? string.Empty,
                                (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: xmlVehicleMod.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound())
                            .ToStringAsync(token).ConfigureAwait(false);
                    await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
                    await lblAvailLabel
                          .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token)
                          .ConfigureAwait(false);

                    // Cost.
                    string strNuyen = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                    decimal decItemCost = 0;
                    if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    {
                        string strCost = 0.0m.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                            + strNuyen;
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        string strCost
                            = xmlVehicleMod.SelectSingleNodeAndCacheExpression("cost", token)?.Value ?? string.Empty;
                        strCost = strCost.ProcessFixedValuesString(intRating);
                        if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                        {
                            string strFirstHalf = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
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
                            if (decMax == decimal.MaxValue)
                            {
                                string strText =
                                    decMin.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + strNuyen + "+";
                                await lblCost.DoThreadSafeAsync(x => x.Text = strText, token: token)
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                    .ConfigureAwait(false);
                                string strText =
                                    decMin.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + strSpace + "-" + strSpace +
                                    decMax.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + strNuyen;
                                await lblCost.DoThreadSafeAsync(x => x.Text = strText, token: token)
                                    .ConfigureAwait(false);
                            }

                            strCost = decMin.ToString(GlobalSettings.InvariantCultureInfo);
                        }

                        decItemCost = (await ProcessInvariantXPathExpression(strCost, intRating, intExtraSlots, token).ConfigureAwait(false)).Item1;

                        // Apply any markup.
                        decItemCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                            .ConfigureAwait(false) / 100.0m;

                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                        .ConfigureAwait(false))
                        {
                            decItemCost *= 0.9m;
                        }

                        string strText2 = decItemCost.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false),
                                                                     GlobalSettings.CultureInfo)
                                                + strNuyen;
                        await lblCost
                              .DoThreadSafeAsync(
                                  x => x.Text = strText2, token: token)
                              .ConfigureAwait(false);
                    }

                    await lblCostLabel
                          .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblCost.Text), token: token)
                          .ConfigureAwait(false);

                    // Update the Avail Test Label.
                    string strTest = await _objCharacter.AvailTestAsync(decItemCost, strAvail, token)
                                                        .ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token)
                                      .ConfigureAwait(false);

                    string strCategory
                        = xmlVehicleMod.SelectSingleNodeAndCacheExpression("category", token)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strCategory))
                    {
                        if (Vehicle.ModCategoryStrings.Contains(strCategory))
                        {
                            await lblVehicleCapacityLabel.DoThreadSafeAsync(x => x.Visible = true, token: token)
                                                         .ConfigureAwait(false);
                            int intSlots = (await ProcessInvariantXPathExpression(strSlots, intRating, intExtraSlots, token).ConfigureAwait(false)).Item1.StandardRound();
                            string strCapacity = await GetRemainingModCapacity(strCategory, intSlots, token).ConfigureAwait(false);
                            await lblVehicleCapacity.DoThreadSafeAsync(x =>
                            {
                                x.Visible = true;
                                x.Text = strCapacity;
                            }, token: token).ConfigureAwait(false);
                            await lblVehicleCapacityLabel
                                  .SetToolTipTextAsync(
                                      await LanguageManager
                                            .GetStringAsync("Tip_RemainingVehicleModCapacity", token: token)
                                            .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblVehicleCapacityLabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                                         .ConfigureAwait(false);
                            await lblVehicleCapacity.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                                    .ConfigureAwait(false);
                        }

                        if (strCategory == "Weapon Mod")
                        {
                            string strCategoryLabel = await LanguageManager
                                                            .GetStringAsync("String_WeaponModification", token: token)
                                                            .ConfigureAwait(false);
                            await lblCategory.DoThreadSafeAsync(x => x.Text = strCategoryLabel, token: token)
                                             .ConfigureAwait(false);
                        }
                        // Translate the Category if possible.
                        else if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                 StringComparison.OrdinalIgnoreCase))
                        {
                            XPathNavigator objXmlCategoryTranslate
                                = _xmlBaseVehicleDataNode.SelectSingleNode(
                                    "modcategories/category[. = " + strCategory.CleanXPath() + "]/@translate");
                            await lblCategory
                                  .DoThreadSafeAsync(x => x.Text = objXmlCategoryTranslate?.Value ?? strCategory,
                                                     token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblCategory.DoThreadSafeAsync(x => x.Text = strCategory, token: token)
                                             .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await lblCategory.DoThreadSafeAsync(x => x.Text = strCategory, token: token)
                                         .ConfigureAwait(false);
                        await lblVehicleCapacityLabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                                     .ConfigureAwait(false);
                        await lblVehicleCapacity.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                                .ConfigureAwait(false);
                    }

                    bool blnShowCategory = !string.IsNullOrEmpty(
                        await lblCategory.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                    await lblCategoryLabel.DoThreadSafeAsync(x => x.Visible = blnShowCategory, token: token)
                                          .ConfigureAwait(false);

                    string strLimit = xmlVehicleMod.SelectSingleNodeAndCacheExpression("limit", token)?.Value;
                    if (!string.IsNullOrEmpty(strLimit))
                    {
                        // Translate the Limit if possible.
                        if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                            StringComparison.OrdinalIgnoreCase))
                        {
                            XPathNavigator objXmlLimit
                                = _xmlBaseVehicleDataNode.SelectSingleNode(
                                    "limits/limit[. = " + strLimit.CleanXPath() + "]/@translate");
                            strLimit = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                            .ConfigureAwait(false) + "(" + objXmlLimit?.Value
                                       ?? strLimit + ")";
                        }
                        else
                        {
                            strLimit = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                            .ConfigureAwait(false) + "(" + strLimit + ")";
                        }

                        await lblLimit.DoThreadSafeAsync(x => x.Text = strLimit, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblLimit.DoThreadSafeAsync(x => x.Text = string.Empty, token: token)
                                      .ConfigureAwait(false);
                    }

                    if (xmlVehicleMod.SelectSingleNodeAndCacheExpression("ratinglabel", token) != null)
                    {
                        string strRatingLabel = string.Format(GlobalSettings.CultureInfo,
                                                              await LanguageManager
                                                                    .GetStringAsync("Label_RatingFormat", token: token)
                                                                    .ConfigureAwait(false),
                                                              await LanguageManager
                                                                    .GetStringAsync(
                                                                        xmlVehicleMod
                                                                            .SelectSingleNodeAndCacheExpression(
                                                                                "ratinglabel", token).Value,
                                                                        token: token).ConfigureAwait(false));
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                    }

                    string strSource
                        = xmlVehicleMod.SelectSingleNodeAndCacheExpression("source", token)?.Value ?? await LanguageManager
                            .GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    string strPage
                        = xmlVehicleMod.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value
                          ?? xmlVehicleMod.SelectSingleNodeAndCacheExpression("page", token)?.Value
                          ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    SourceString objSourceString = await SourceString
                                                         .GetSourceStringAsync(
                                                             strSource, strPage, GlobalSettings.Language,
                                                             GlobalSettings.CultureInfo, _objCharacter, token: token)
                                                         .ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource, this, token: token).ConfigureAwait(false);
                    await lblSourceLabel
                          .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSourceString.ToString()),
                                             token: token).ConfigureAwait(false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                _blnSkipUpdate = false;
            }
        }

        private Task<string> GetRemainingModCapacity(string strCategory, int intModSlots, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            switch (strCategory.ToUpperInvariant())
            {
                case "POWERTRAIN":
                    return _objVehicle.PowertrainModSlotsUsedAsync(intModSlots, token);

                case "PROTECTION":
                    return _objVehicle.ProtectionModSlotsUsedAsync(intModSlots, token);

                case "WEAPONS":
                    return _objVehicle.WeaponModSlotsUsedAsync(intModSlots, token);

                case "BODY":
                    return _objVehicle.BodyModSlotsUsedAsync(intModSlots, token);

                case "ELECTROMAGNETIC":
                    return _objVehicle.ElectromagneticModSlotsUsedAsync(intModSlots, token);

                case "COSMETIC":
                    return _objVehicle.CosmeticModSlotsUsedAsync(intModSlots, token);

                default:
                    return Task.FromResult(string.Empty);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        private async Task<ValueTuple<decimal, bool>> ProcessInvariantXPathExpression(string strExpression, int intRating, int intExtraSlots = 0, CancellationToken token = default)
        {
            return await CommonFunctions.ProcessInvariantXPathExpressionAsync(
                strExpression, 
                intRating, 
                _objCharacter, 
                _objVehicle, 
                intExtraSlots: intExtraSlots, 
                token: token).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
