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
        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private readonly List<VehicleMod> _lstMods = new List<VehicleMod>();

        #region Control Events

        public SelectVehicleMod(Character objCharacter, Vehicle objVehicle, IEnumerable<VehicleMod> lstExistingMods = null)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objVehicle = objVehicle ?? throw new ArgumentNullException(nameof(objVehicle));
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            if (_xmlBaseVehicleDataNode != null)
                _setBlackMarketMaps.AddRange(
                    _objCharacter.GenerateBlackMarketMappings(
                        _xmlBaseVehicleDataNode.SelectSingleNodeAndCacheExpression("modcategories")));
            if (lstExistingMods != null)
                _lstMods.AddRange(lstExistingMods);
        }

        private async void SelectVehicleMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = true);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                });
            }
            else
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(
                        GlobalSettings.CultureInfo, x.Text,
                        _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                });
            }
            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            string[] strValues = _strLimitToCategories.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Populate the Category list.
            string strFilterPrefix = (VehicleMountMods
                ? "weaponmountmods/mod[("
                : "mods/mod[(") + await _objCharacter.Settings.BookXPathAsync() + ") and category = ";
            foreach (XPathNavigator objXmlCategory in await _xmlBaseVehicleDataNode.SelectAndCacheExpressionAsync("modcategories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if ((string.IsNullOrEmpty(_strLimitToCategories) || strValues.Contains(strInnerText))
                    && _xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + ']') != null)
                {
                    _lstCategory.Add(new ListItem(strInnerText, (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll")));
            }
            await cboCategory.PopulateWithListItemsAsync(_lstCategory);
            await cboCategory.DoThreadSafeAsync(x =>
            {
                // Select the first Category in the list.
                if (!string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedValue = _strSelectCategory;
                if (x.SelectedIndex == -1 && _lstCategory.Count > 0)
                    x.SelectedIndex = 0;
            });

            _blnLoading = false;
            await UpdateGearInfo();
        }

        private async void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
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
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked))
                await RefreshList();
            await UpdateGearInfo();
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                await RefreshList();
            await UpdateGearInfo();
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
                case Keys.Up when lstMod.SelectedIndex - 1 >= 0:
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
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
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
        /// Whether or not the item should be added for free.
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
        private async ValueTask RefreshList(CancellationToken token = default)
        {
            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            string strFilter = '(' + await _objCharacter.Settings.BookXPathAsync(token: token) + ')';
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token);
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
            XPathNavigator objXmlVehicleNode = await _objVehicle.GetNodeXPathAsync(token: token);
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMods))
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                decimal decBaseCostMultiplier = 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m);
                foreach (XPathNavigator objXmlMod in objXmlModList)
                {
                    XPathNavigator xmlTestNode
                        = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("forbidden/vehicledetails", token: token);
                    if (xmlTestNode != null && await objXmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("required/vehicledetails", token: token);
                    if (xmlTestNode != null && !await objXmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("forbidden/oneof", token: token);
                    if (xmlTestNode != null)
                    {
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setForbiddenAccessory))
                        {
                            foreach (XPathNavigator node in await xmlTestNode.SelectAndCacheExpressionAsync("mods", token: token))
                            {
                                setForbiddenAccessory.Add(node.Value);
                            }

                            if (_lstMods.Any(objAccessory => setForbiddenAccessory.Contains(objAccessory.Name)))
                            {
                                continue;
                            }
                        }
                    }

                    xmlTestNode = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("required/oneof", token: token);
                    if (xmlTestNode != null)
                    {
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setRequiredAccessory))
                        {
                            foreach (XPathNavigator node in await xmlTestNode.SelectAndCacheExpressionAsync("mods", token: token))
                            {
                                setRequiredAccessory.Add(node.Value);
                            }

                            if (!_lstMods.Any(objAccessory => setRequiredAccessory.Contains(objAccessory.Name)))
                            {
                                continue;
                            }
                        }
                    }

                    xmlTestNode = await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("requires", token: token);
                    if (xmlTestNode != null && _objVehicle.Seats
                        < ((await xmlTestNode.SelectSingleNodeAndCacheExpressionAsync("seats", token: token))?.ValueAsInt ?? 0))
                    {
                        continue;
                    }

                    int intMinRating = 1;
                    string strMinRating = (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("minrating", token: token))?.Value;
                    if (strMinRating?.Length > 0)
                    {
                        strMinRating = await ReplaceStrings(strMinRating, token: token);
                        object objTempProcess
                            = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnTempIsSuccess);
                        if (blnTempIsSuccess)
                            intMinRating = ((double) objTempProcess).StandardRound();
                    }

                    string strRating = (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("rating", token: token))?.Value;
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
                            intMinRating = Math.Min(intMinRating, _objVehicle.TotalSeats);
                        }
                        else if (int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                              out int intMaxRating))
                        {
                            intMinRating = Math.Min(intMinRating, intMaxRating);
                        }
                    }

                    decimal decCostMultiplier = decBaseCostMultiplier;
                    if (_setBlackMarketMaps.Contains((await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("category", token: token))?.Value))
                        decCostMultiplier *= 0.9m;
                    if ((!blnHideOverAvailLimit || await objXmlMod.CheckAvailRestrictionAsync(_objCharacter, intMinRating, token: token))
                        &&
                        (!blnShowOnlyAffordItems || blnFreeItem
                                                 || await objXmlMod.CheckNuyenRestrictionAsync(
                                                     _objCharacter.Nuyen, decCostMultiplier, intMinRating, token)))
                    {
                        lstMods.Add(new ListItem((await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("id", token: token))?.Value,
                                                 (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("translate", token: token))?.Value
                                                 ?? (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("name", token: token))?.Value
                                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token)));
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
                                                           await LanguageManager.GetStringAsync("String_RestrictedItemsHidden", token: token),
                                                           intOverLimit)));
                }

                string strOldSelected = await lstMod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                _blnLoading = true;
                await lstMod.PopulateWithListItemsAsync(lstMods, token: token);
                _blnLoading = false;
                await lstMod.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrEmpty(strOldSelected))
                        x.SelectedIndex = -1;
                    else
                        x.SelectedValue = strOldSelected;
                }, token: token);
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
                XPathNavigator xmlVehicleMod = _xmlBaseVehicleDataNode.SelectSingleNode((VehicleMountMods ? "weaponmountmods" : "mods") + "/mod[id = " + strSelectedId.CleanXPath() + ']');
                if (xmlVehicleMod != null)
                {
                    SelectedMod = strSelectedId;
                    SelectedRating = nudRating.ValueAsInt;
                    _intMarkup = nudMarkup.ValueAsInt;
                    _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                    _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0)
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
        private async ValueTask UpdateGearInfo(CancellationToken token = default)
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            _blnSkipUpdate = true;
            XPathNavigator xmlVehicleMod = null;
            string strSelectedId = await lstMod.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected Mod.
                // Filtering is also done on the Category in case there are non-unique names across categories.
                xmlVehicleMod = VehicleMountMods
                    ? _xmlBaseVehicleDataNode.SelectSingleNode("weaponmountmods/mod[id = " + strSelectedId.CleanXPath() + ']')
                    : _xmlBaseVehicleDataNode.SelectSingleNode("mods/mod[id = " + strSelectedId.CleanXPath() + ']');
            }

            if (xmlVehicleMod != null)
            {
                bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")?.Value);
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
                }, token: token);

                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                int intMinRating = 1;
                string strMinRating = xmlVehicleMod.SelectSingleNode("minrating")?.Value;
                if (strMinRating?.Length > 0)
                {
                    strMinRating = await ReplaceStrings(strMinRating, token: token);
                    object objTempProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnTempIsSuccess);
                    if (blnTempIsSuccess)
                        intMinRating = ((double)objTempProcess).StandardRound();
                }
                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                string strRating = xmlVehicleMod.SelectSingleNode("rating")?.Value;
                if (string.IsNullOrEmpty(strRating))
                {
                    await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                         .ContinueWith(y => lblRatingLabel.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Minimum = 0;
                        x.Maximum = 0;
                        x.Visible = false;
                    }, token: token);
                    lblRatingNALabel.Visible = true;
                }
                // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                else if (strRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                {
                    await LanguageManager.GetStringAsync("Label_Qty", token: token)
                                         .ContinueWith(y => lblRatingLabel.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Maximum = Vehicle.MaxWheels;
                        x.Visible = true;
                    }, token: token);
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                }
                //Used for the Armor modifications.
                else if (strRating.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    await LanguageManager.GetStringAsync("Label_Body", token: token)
                                         .ContinueWith(y => lblRatingLabel.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Maximum = _objVehicle.Body;
                        x.Visible = true;
                    }, token: token);
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                }
                //Used for Metahuman Adjustments.
                else if (strRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                {
                    await LanguageManager.GetStringAsync("Label_Seats", token: token)
                                         .ContinueWith(y => lblRatingLabel.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Maximum = _objVehicle.TotalSeats;
                        x.Visible = true;
                    }, token: token);
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                }
                else
                {
                    await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                         .ContinueWith(y => lblRatingLabel.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    if (int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intTempRating) && intTempRating > 0)
                    {
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Maximum = intTempRating;
                            x.Visible = true;
                        }, token: token);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                    }
                    else
                    {
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = 0;
                            x.Maximum = 0;
                            x.Visible = false;
                        }, token: token);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    }
                }
                if (await nudRating.DoThreadSafeFuncAsync(x => x.Maximum, token: token) != 0)
                {
                    if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token);
                        while (intMaximum > intMinRating && !await xmlVehicleMod.CheckAvailRestrictionAsync(_objCharacter, intMaximum, token: token))
                        {
                            --intMaximum;
                        }
                        await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token);
                    }

                    if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        decimal decCostMultiplier = 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m);
                        if (_setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")?.Value))
                            decCostMultiplier *= 0.9m;
                        int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token);
                        while (intMaximum > 1 && !await xmlVehicleMod.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier, intMaximum, token))
                        {
                            --intMaximum;
                        }
                        await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token);
                    }

                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Minimum = intMinRating;
                        x.Enabled = x.Maximum != x.Minimum;
                    }, token: token);
                }

                int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token);

                // Slots.
                string strSlots = xmlVehicleMod.SelectSingleNode("slots")?.Value ?? string.Empty;
                if (strSlots.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strSlots.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strSlots = strValues[intRating - 1];
                }
                int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intExtraSlots);
                strSlots = await ReplaceStrings(strSlots, intExtraSlots, token);
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSlots, out bool blnIsSuccess);
                if (blnIsSuccess)
                    strSlots = ((double) objProcess).StandardRound().ToString(GlobalSettings.CultureInfo);
                await lblSlots.DoThreadSafeAsync(x => x.Text = strSlots, token: token);
                await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSlots), token: token);

                int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.CultureInfo, out intExtraSlots);

                // Avail.
                string strAvail = new AvailabilityValue(intRating, xmlVehicleMod.SelectSingleNode("avail")?.Value)
                    .ToString();
                await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token);
                await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token);

                // Cost.
                decimal decItemCost = 0;
                if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    await lblCost.DoThreadSafeAsync(x => x.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                else
                {
                    string strCost = xmlVehicleMod.SelectSingleNode("cost")?.Value ?? string.Empty;
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
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        if (decMax == decimal.MaxValue)
                            await lblCost.DoThreadSafeAsync(
                                x => x.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                              GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol") + '+', token: token);
                        else
                        {
                            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
                            await lblCost.DoThreadSafeAsync(
                                x => x.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                              GlobalSettings.CultureInfo)
                                              + strSpace + '-' + strSpace
                                              + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                                GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
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
                    strCost = await ReplaceStrings(strCost, intExtraSlots, token);

                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out blnIsSuccess);
                    if (blnIsSuccess)
                        decItemCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);

                    // Apply any markup.
                    decItemCost *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m);

                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        decItemCost *= 0.9m;
                    }

                    await lblCost.DoThreadSafeAsync(x => x.Text = decItemCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                }
                await lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblCost.Text), token: token);

                // Update the Avail Test Label.
                string strTest = await _objCharacter.AvailTestAsync(decItemCost, strAvail, token);
                await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token);
                await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token);

                string strCategory = xmlVehicleMod.SelectSingleNode("category")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strCategory))
                {
                    if (Vehicle.ModCategoryStrings.Contains(strCategory))
                    {
                        await lblVehicleCapacityLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                        int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.CultureInfo, out int intSlots);
                        await lblVehicleCapacity.DoThreadSafeAsync(x =>
                        {
                            x.Visible = true;
                            x.Text = GetRemainingModCapacity(strCategory, intSlots);
                        }, token: token);
                        await lblVehicleCapacityLabel.SetToolTipAsync(await LanguageManager.GetStringAsync("Tip_RemainingVehicleModCapacity", token: token), token: token);
                    }
                    else
                    {
                        await lblVehicleCapacityLabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                        await lblVehicleCapacity.DoThreadSafeAsync(x => x.Visible = false, token: token);
                    }

                    if (strCategory == "Weapon Mod")
                        await LanguageManager.GetStringAsync("String_WeaponModification", token: token)
                                             .ContinueWith(y => lblCategory.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                             .Unwrap();
                    // Translate the Category if possible.
                    else if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                             StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objXmlCategoryTranslate
                            = _xmlBaseVehicleDataNode.SelectSingleNode(
                                "modcategories/category[. = " + strCategory.CleanXPath() + "]/@translate");
                        await lblCategory.DoThreadSafeAsync(x => x.Text = objXmlCategoryTranslate?.Value ?? strCategory, token: token);
                    }
                    else
                        await lblCategory.DoThreadSafeAsync(x => x.Text = strCategory, token: token);
                }
                else
                {
                    await lblCategory.DoThreadSafeAsync(x => x.Text = strCategory, token: token);
                    await lblVehicleCapacityLabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                    await lblVehicleCapacity.DoThreadSafeAsync(x => x.Visible = false, token: token);
                }

                await lblCategory.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                 .ContinueWith(
                                     y => lblCategoryLabel.DoThreadSafeAsync(
                                         x => x.Visible = !string.IsNullOrEmpty(y.Result), token: token), token).Unwrap();

                string strLimit = xmlVehicleMod.SelectSingleNode("limit")?.Value;
                if (!string.IsNullOrEmpty(strLimit))
                {
                    // Translate the Limit if possible.
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objXmlLimit = _xmlBaseVehicleDataNode.SelectSingleNode("limits/limit[. = " + strLimit.CleanXPath() + "]/@translate");
                        strLimit = await LanguageManager.GetStringAsync("String_Space", token: token) + '(' + objXmlLimit?.Value ?? strLimit + ')';
                    }
                    else
                    {
                        strLimit = await LanguageManager.GetStringAsync("String_Space", token: token) + '(' + strLimit + ')';
                    }
                    await lblLimit.DoThreadSafeAsync(x => x.Text = strLimit, token: token);
                }
                else
                    await lblLimit.DoThreadSafeAsync(x => x.Text = string.Empty, token: token);

                if (xmlVehicleMod.SelectSingleNode("ratinglabel") != null)
                {
                    string strRatingLabel = string.Format(GlobalSettings.CultureInfo,
                                                          await LanguageManager.GetStringAsync("Label_RatingFormat", token: token),
                                                          await LanguageManager.GetStringAsync(xmlVehicleMod.SelectSingleNode("ratinglabel").Value, token: token));
                    await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token);
                }

                string strSource = xmlVehicleMod.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
                string strPage = (await xmlVehicleMod.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token))?.Value
                                 ?? xmlVehicleMod.SelectSingleNode("page")?.Value
                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
                SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter, token: token);
                await objSourceString.SetControlAsync(lblSource, token: token);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSourceString.ToString()), token: token);
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token);
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token);
            }
            _blnSkipUpdate = false;
        }

        private string GetRemainingModCapacity(string strCategory, int intModSlots)
        {
            switch (strCategory)
            {
                case "Powertrain":
                    return _objVehicle.PowertrainModSlotsUsed(intModSlots);

                case "Protection":
                    return _objVehicle.ProtectionModSlotsUsed(intModSlots);

                case "Weapons":
                    return _objVehicle.WeaponModSlotsUsed(intModSlots);

                case "Body":
                    return _objVehicle.BodyModSlotsUsed(intModSlots);

                case "Electromagnetic":
                    return _objVehicle.ElectromagneticModSlotsUsed(intModSlots);

                case "Cosmetic":
                    return _objVehicle.CosmeticModSlotsUsed(intModSlots);

                default:
                    return string.Empty;
            }
        }

        private async ValueTask<string> ReplaceStrings(string strInput, int intExtraSlots = 0, CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInput))
            {
                sbdInput.Append(strInput);
                await sbdInput.CheapReplaceAsync("Rating", strInput, () => nudRating.DoThreadSafeFuncAsync(x => x.Value.ToString(GlobalSettings.InvariantCultureInfo), token: token), token: token);
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
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
