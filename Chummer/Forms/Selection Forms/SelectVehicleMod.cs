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
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectVehicleMod : Form
    {
        private readonly Vehicle _objVehicle;
        private int _intWeaponMountSlots;
        private int _intMarkup;
        private bool _blnLoading = true;
        private bool _blnSkipUpdate;
        private static string _strSelectCategory = string.Empty;

        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private readonly string _strLimitToCategories = string.Empty;
        private List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectVehicleMod(Character objCharacter, Vehicle objVehicle)
        {
            Disposed += (sender, args) =>
            {
                Utils.ListItemListPool.Return(ref _lstCategory);
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
            };
            _objVehicle = objVehicle ?? throw new ArgumentNullException(nameof(objVehicle));
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
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
            if (_objCharacter.Created)
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
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(
                        GlobalSettings.CultureInfo, x.Text,
                        _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }
            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount).ConfigureAwait(false);

            string[] strValues = _strLimitToCategories.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Populate the Category list.
            string strFilterPrefix = (VehicleMountMods
                ? "weaponmountmods/mod[("
                : "mods/mod[(") + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and category = ";
            foreach (XPathNavigator objXmlCategory in _xmlBaseVehicleDataNode.SelectAndCacheExpression("modcategories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if ((string.IsNullOrEmpty(_strLimitToCategories) || strValues.Contains(strInnerText))
                    && _xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + ']') != null)
                {
                    _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
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
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public int Markup => _intMarkup;

        /// <summary>
        /// Is the mod being added to a vehicle weapon mount?
        /// </summary>
        public bool VehicleMountMods { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Build the list of Mods.
        /// </summary>
        private async Task RefreshList(CancellationToken token = default)
        {
            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            string strFilter = '(' + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false) + ')';
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (string.IsNullOrWhiteSpace(strSearch) || GlobalSettings.SearchInCategoryOnly))
                strFilter += " and category = " + strCategory.CleanXPath();
            /*
            else if (!string.IsNullOrEmpty(AllowedCategories))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdCategoryFilter))
                {
                    foreach (string strItem in _lstCategory.Select(x => x.Value))
                    {
                        if (!string.IsNullOrEmpty(strItem))
                            sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                    }
                    if (sbdCategoryFilter.Length > 0)
                    {
                        sbdCategoryFilter.Length -= 4;
                        strFilter += " and (" + sbdCategoryFilter.ToString() + ')';
                    }
                }
            }
            */

            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);

            // Retrieve the list of Mods for the selected Category.
            XPathNodeIterator objXmlModList = VehicleMountMods
                ? _xmlBaseVehicleDataNode.Select("weaponmountmods/mod[" + strFilter + ']')
                : _xmlBaseVehicleDataNode.Select("mods/mod[" + strFilter + ']');
            // Update the list of Mods based on the selected Category.
            int intOverLimit = 0;
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMods))
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                decimal decBaseCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                foreach (XPathNavigator objXmlMod in objXmlModList)
                {
                    if (!await _objVehicle.CheckModRequirementsAsync(objXmlMod, token).ConfigureAwait(false))
                        continue;

                    int intMinRating = 1;
                    string strMinRating = objXmlMod.SelectSingleNodeAndCacheExpression("minrating", token: token)?.Value;
                    if (strMinRating?.Length > 0)
                    {
                        strMinRating = await ReplaceStrings(strMinRating, token: token).ConfigureAwait(false);
                        (bool blnTempIsSuccess, object objTempProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating, token).ConfigureAwait(false);
                        if (blnTempIsSuccess)
                            intMinRating = ((double) objTempProcess).StandardRound();
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
                            intMinRating = Math.Min(intMinRating, _objVehicle.Body);
                        }
                        //Used for Metahuman Adjustments.
                        else if (strRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                        {
                            intMinRating = Math.Min(intMinRating, await _objVehicle.GetTotalSeatsAsync(token).ConfigureAwait(false));
                        }
                        else if (int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                              out int intMaxRating))
                        {
                            intMinRating = Math.Min(intMinRating, intMaxRating);
                        }
                    }

                    decimal decCostMultiplier = decBaseCostMultiplier;
                    if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNodeAndCacheExpression("category", token: token)?.Value))
                        decCostMultiplier *= 0.9m;
                    if ((!blnHideOverAvailLimit || await objXmlMod.CheckAvailRestrictionAsync(_objCharacter, intMinRating, token: token).ConfigureAwait(false))
                        &&
                        (!blnShowOnlyAffordItems || blnFreeItem
                                                 || await objXmlMod.CheckNuyenRestrictionAsync(
                                                     _objCharacter.Nuyen, decCostMultiplier, intMinRating, token).ConfigureAwait(false)))
                    {
                        lstMods.Add(new ListItem(objXmlMod.SelectSingleNodeAndCacheExpression("id", token: token)?.Value,
                                                 objXmlMod.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                 ?? objXmlMod.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
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
                    _intMarkup = nudMarkup.ValueAsInt;
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

                    // Extract the Avail and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                    // This is done using XPathExpression.

                    int intMinRating = 1;
                    string strMinRating = xmlVehicleMod.SelectSingleNodeAndCacheExpression("minrating", token)?.Value;
                    if (strMinRating?.Length > 0)
                    {
                        strMinRating = await ReplaceStrings(strMinRating, token: token).ConfigureAwait(false);
                        (bool blnTempIsSuccess, object objTempProcess) = await CommonFunctions
                                                                               .EvaluateInvariantXPathAsync(
                                                                                   strMinRating, token)
                                                                               .ConfigureAwait(false);
                        if (blnTempIsSuccess)
                            intMinRating = ((double) objTempProcess).StandardRound();
                    }

                    await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                    string strRating = xmlVehicleMod.SelectSingleNodeAndCacheExpression("rating", token)?.Value;
                    if (string.IsNullOrEmpty(strRating))
                    {
                        string strRatingLabel = await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                                                     .ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token)
                                            .ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = 0;
                            x.Maximum = 0;
                            x.Visible = false;
                        }, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
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
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Maximum = _objVehicle.Body;
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
                        if (int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out int intTempRating) && intTempRating > 0)
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
                        if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                       .ConfigureAwait(false))
                        {
                            int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token)
                                                            .ConfigureAwait(false);
                            while (intMaximum > intMinRating && !await xmlVehicleMod
                                                                       .CheckAvailRestrictionAsync(
                                                                           _objCharacter, intMaximum, token: token)
                                                                       .ConfigureAwait(false))
                            {
                                --intMaximum;
                            }

                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token)
                                           .ConfigureAwait(false);
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
                            int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token)
                                                            .ConfigureAwait(false);
                            while (intMaximum > 1 && !await xmlVehicleMod
                                                            .CheckNuyenRestrictionAsync(
                                                                _objCharacter.Nuyen, decCostMultiplier, intMaximum,
                                                                token).ConfigureAwait(false))
                            {
                                --intMaximum;
                            }

                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token)
                                           .ConfigureAwait(false);
                        }

                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = intMinRating;
                            x.Enabled = x.Maximum != x.Minimum;
                        }, token: token).ConfigureAwait(false);
                    }

                    int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token)
                                                   .ConfigureAwait(false);

                    // Slots.
                    string strSlots
                        = xmlVehicleMod.SelectSingleNodeAndCacheExpression("slots", token)?.Value ?? string.Empty;
                    if (strSlots.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strSlots.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                     .Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strSlots = strValues[intRating - 1];
                    }

                    int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                 out int intExtraSlots);
                    strSlots = await ReplaceStrings(strSlots, intExtraSlots, token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                   .EvaluateInvariantXPathAsync(strSlots, token)
                                                                   .ConfigureAwait(false);
                    if (blnIsSuccess)
                        strSlots = ((double) objProcess).StandardRound().ToString(GlobalSettings.CultureInfo);
                    await lblSlots.DoThreadSafeAsync(x => x.Text = strSlots, token: token).ConfigureAwait(false);
                    await lblSlotsLabel
                          .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSlots), token: token)
                          .ConfigureAwait(false);

                    int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.CultureInfo, out intExtraSlots);

                    // Avail.
                    string strAvail
                        = await new AvailabilityValue(
                                intRating,
                                xmlVehicleMod.SelectSingleNodeAndCacheExpression("avail", token)?.Value)
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
                        string strCost = 0.0m.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false),
                            GlobalSettings.CultureInfo) + strNuyen;
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        string strCost
                            = xmlVehicleMod.SelectSingleNodeAndCacheExpression("cost", token)?.Value ?? string.Empty;
                        if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                        {
                            decimal decMin;
                            decimal decMax = decimal.MaxValue;
                            strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                            if (strCost.Contains('-'))
                            {
                                string[] strValues = strCost.Split('-');
                                decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                                decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                            }
                            else
                            {
                                decMin = Convert.ToDecimal(strCost.FastEscape('+'),
                                                           GlobalSettings.InvariantCultureInfo);
                            }

                            if (decMax == decimal.MaxValue)
                            {
                                string strText =
                                    decMin.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false),
                                        GlobalSettings.CultureInfo) + strNuyen + '+';
                                await lblCost.DoThreadSafeAsync(x => x.Text = strText, token: token)
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                    .ConfigureAwait(false);
                                string strText =
                                    decMin.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false),
                                        GlobalSettings.CultureInfo) + strSpace + '-' + strSpace +
                                    decMax.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false),
                                        GlobalSettings.CultureInfo) + strNuyen;
                                await lblCost.DoThreadSafeAsync(x => x.Text = strText, token: token)
                                    .ConfigureAwait(false);
                            }

                            strCost = decMin.ToString(GlobalSettings.InvariantCultureInfo);
                        }
                        else if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                        {
                            strCost = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                            string[] strValues = strCost.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (intRating < 1 || intRating > strValues.Length)
                            {
                                intRating = 1;
                            }

                            strCost = strValues[intRating - 1];
                        }

                        strCost = await ReplaceStrings(strCost, intExtraSlots, token).ConfigureAwait(false);

                        (blnIsSuccess, objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token)
                                                                          .ConfigureAwait(false);
                        if (blnIsSuccess)
                            decItemCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);

                        // Apply any markup.
                        decItemCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                            .ConfigureAwait(false) / 100.0m;

                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                        .ConfigureAwait(false))
                        {
                            decItemCost *= 0.9m;
                        }

                        await lblCost
                              .DoThreadSafeAsync(
                                  x => x.Text = decItemCost.ToString(_objCharacter.Settings.NuyenFormat,
                                                                     GlobalSettings.CultureInfo)
                                                + strNuyen, token: token)
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
                            int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.CultureInfo, out int intSlots);
                            string strCapacity = await GetRemainingModCapacity(strCategory, intSlots, token).ConfigureAwait(false);
                            await lblVehicleCapacity.DoThreadSafeAsync(x =>
                            {
                                x.Visible = true;
                                x.Text = strCapacity;
                            }, token: token).ConfigureAwait(false);
                            await lblVehicleCapacityLabel
                                  .SetToolTipAsync(
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
                                                            .ConfigureAwait(false) + '(' + objXmlLimit?.Value
                                       ?? strLimit + ')';
                        }
                        else
                        {
                            strLimit = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                            .ConfigureAwait(false) + '(' + strLimit + ')';
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
                    await objSourceString.SetControlAsync(lblSource, token: token).ConfigureAwait(false);
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
            switch (strCategory)
            {
                case "Powertrain":
                    return _objVehicle.PowertrainModSlotsUsedAsync(intModSlots, token);

                case "Protection":
                    return _objVehicle.ProtectionModSlotsUsedAsync(intModSlots, token);

                case "Weapons":
                    return _objVehicle.WeaponModSlotsUsedAsync(intModSlots, token);

                case "Body":
                    return _objVehicle.BodyModSlotsUsedAsync(intModSlots, token);

                case "Electromagnetic":
                    return _objVehicle.ElectromagneticModSlotsUsedAsync(intModSlots, token);

                case "Cosmetic":
                    return _objVehicle.CosmeticModSlotsUsedAsync(intModSlots, token);

                default:
                    return Task.FromResult(string.Empty);
            }
        }

        private async Task<string> ReplaceStrings(string strInput, int intExtraSlots = 0, CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInput))
            {
                sbdInput.Append(strInput);
                await sbdInput.CheapReplaceAsync(strInput, "Rating", () => nudRating.DoThreadSafeFuncAsync(x => x.Value.ToString(GlobalSettings.InvariantCultureInfo), token: token), token: token).ConfigureAwait(false);
                sbdInput.Replace("Vehicle Cost", _objVehicle.Cost);
                sbdInput.Replace("Weapon Cost", 0.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Total Cost", 0.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Body", _objVehicle.Body.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Handling", _objVehicle.Handling.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Offroad Handling",
                                 _objVehicle.OffroadHandling.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Speed", _objVehicle.Speed.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Offroad Speed",
                                 _objVehicle.OffroadSpeed.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Acceleration", _objVehicle.Accel.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Offroad Acceleration",
                                 _objVehicle.OffroadAccel.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Sensor", _objVehicle.BaseSensor.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace("Armor", _objVehicle.Armor.ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.Replace(
                    "Slots", (_intWeaponMountSlots + intExtraSlots).ToString(GlobalSettings.InvariantCultureInfo));

                return sbdInput.ToString();
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
