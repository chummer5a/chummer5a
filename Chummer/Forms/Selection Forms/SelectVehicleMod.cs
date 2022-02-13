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

        private void SelectVehicleMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
                chkHideOverAvailLimit.Text = string.Format(GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
            }
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            string[] strValues = _strLimitToCategories.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Populate the Category list.
            string strFilterPrefix = (VehicleMountMods
                ? "weaponmountmods/mod[("
                : "mods/mod[(") + _objCharacter.Settings.BookXPath() + ") and category = ";
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
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }
            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedValue = _strSelectCategory;
            if (cboCategory.SelectedIndex == -1 && _lstCategory.Count > 0)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            _blnLoading = false;
            UpdateGearInfo();
        }

        private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void RefreshCurrentList(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            _strSelectCategory = string.Empty;
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
                RefreshList();
            UpdateGearInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                RefreshList();
            UpdateGearInfo();
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
        private void RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Settings.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (string.IsNullOrWhiteSpace(txtSearch.Text) || GlobalSettings.SearchInCategoryOnly))
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

            if (!string.IsNullOrEmpty(txtSearch.Text))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Retrieve the list of Mods for the selected Category.
            XPathNodeIterator objXmlModList = VehicleMountMods
                ? _xmlBaseVehicleDataNode.Select("weaponmountmods/mod[" + strFilter + ']')
                : _xmlBaseVehicleDataNode.Select("mods/mod[" + strFilter + ']');
            // Update the list of Mods based on the selected Category.
            int intOverLimit = 0;
            XPathNavigator objXmlVehicleNode = _objVehicle.GetNodeXPath();
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMods))
            {
                foreach (XPathNavigator objXmlMod in objXmlModList)
                {
                    XPathNavigator xmlTestNode
                        = objXmlMod.SelectSingleNodeAndCacheExpression("forbidden/vehicledetails");
                    if (xmlTestNode != null && objXmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = objXmlMod.SelectSingleNodeAndCacheExpression("required/vehicledetails");
                    if (xmlTestNode != null && !objXmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = objXmlMod.SelectSingleNodeAndCacheExpression("forbidden/oneof");
                    if (xmlTestNode != null)
                    {
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setForbiddenAccessory))
                        {
                            foreach (XPathNavigator node in xmlTestNode.SelectAndCacheExpression("mods"))
                            {
                                setForbiddenAccessory.Add(node.Value);
                            }

                            if (_lstMods.Any(objAccessory => setForbiddenAccessory.Contains(objAccessory.Name)))
                            {
                                continue;
                            }
                        }
                    }

                    xmlTestNode = objXmlMod.SelectSingleNodeAndCacheExpression("required/oneof");
                    if (xmlTestNode != null)
                    {
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setRequiredAccessory))
                        {
                            foreach (XPathNavigator node in xmlTestNode.SelectAndCacheExpression("mods"))
                            {
                                setRequiredAccessory.Add(node.Value);
                            }

                            if (!_lstMods.Any(objAccessory => setRequiredAccessory.Contains(objAccessory.Name)))
                            {
                                continue;
                            }
                        }
                    }

                    xmlTestNode = objXmlMod.SelectSingleNodeAndCacheExpression("requires");
                    if (xmlTestNode != null && _objVehicle.Seats
                        < (xmlTestNode.SelectSingleNodeAndCacheExpression("seats")?.ValueAsInt ?? 0))
                    {
                        continue;
                    }

                    int intMinRating = 1;
                    string strMinRating = objXmlMod.SelectSingleNodeAndCacheExpression("minrating")?.Value;
                    if (strMinRating?.Length > 0)
                    {
                        strMinRating = ReplaceStrings(strMinRating);
                        object objTempProcess
                            = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnTempIsSuccess);
                        if (blnTempIsSuccess)
                            intMinRating = ((double) objTempProcess).StandardRound();
                    }

                    string strRating = objXmlMod.SelectSingleNodeAndCacheExpression("rating")?.Value;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                        if (strRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                        {
                            intMinRating = Math.Min(intMinRating, 20);
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

                    decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNodeAndCacheExpression("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    if ((!chkHideOverAvailLimit.Checked || objXmlMod.CheckAvailRestriction(_objCharacter, intMinRating))
                        &&
                        (!chkShowOnlyAffordItems.Checked || chkFreeItem.Checked
                                                         || objXmlMod.CheckNuyenRestriction(
                                                             _objCharacter.Nuyen, decCostMultiplier, intMinRating)))
                    {
                        lstMods.Add(new ListItem(objXmlMod.SelectSingleNodeAndCacheExpression("id")?.Value,
                                                 objXmlMod.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                 ?? objXmlMod.SelectSingleNodeAndCacheExpression("name")?.Value
                                                 ?? LanguageManager.GetString("String_Unknown")));
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
                                                           LanguageManager.GetString("String_RestrictedItemsHidden"),
                                                           intOverLimit)));
                }

                string strOldSelected = lstMod.SelectedValue?.ToString();
                _blnLoading = true;
                lstMod.BeginUpdate();
                lstMod.PopulateWithListItems(lstMods);
                _blnLoading = false;
                if (string.IsNullOrEmpty(strOldSelected))
                    lstMod.SelectedIndex = -1;
                else
                    lstMod.SelectedValue = strOldSelected;
                lstMod.EndUpdate();
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
                    _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlVehicleMod.SelectSingleNodeAndCacheExpression("category")?.Value;
                    DialogResult = DialogResult.OK;
                }
            }
        }

        /// <summary>
        /// Update the Mod's information based on the Mod selected and current Rating.
        /// </summary>
        private void UpdateGearInfo()
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            _blnSkipUpdate = true;
            XPathNavigator xmlVehicleMod = null;
            string strSelectedId = lstMod.SelectedValue?.ToString();
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
                chkBlackMarketDiscount.Enabled = blnCanBlackMarketDiscount;
                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                }
                else if (!blnCanBlackMarketDiscount)
                {
                    //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                int intMinRating = 1;
                string strMinRating = xmlVehicleMod.SelectSingleNode("minrating")?.Value;
                if (strMinRating?.Length > 0)
                {
                    strMinRating = ReplaceStrings(strMinRating);
                    object objTempProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnTempIsSuccess);
                    if (blnTempIsSuccess)
                        intMinRating = ((double)objTempProcess).StandardRound();
                }
                lblRatingLabel.Visible = true;
                string strRating = xmlVehicleMod.SelectSingleNode("rating")?.Value;
                if (string.IsNullOrEmpty(strRating))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                    nudRating.Minimum = 0;
                    nudRating.Maximum = 0;
                    nudRating.Visible = false;
                    lblRatingNALabel.Visible = true;
                }
                // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                else if (strRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Qty");
                    nudRating.Maximum = Vehicle.MaxWheels;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                //Used for the Armor modifications.
                else if (strRating.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Body");
                    nudRating.Maximum = _objVehicle.Body;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                //Used for Metahuman Adjustments.
                else if (strRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Seats");
                    nudRating.Maximum = _objVehicle.TotalSeats;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                else
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                    if (int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intRating) && intRating > 0)
                    {
                        nudRating.Maximum = intRating;
                        nudRating.Visible = true;
                        lblRatingNALabel.Visible = false;
                    }
                    else
                    {
                        nudRating.Minimum = 0;
                        nudRating.Maximum = 0;
                        lblRatingNALabel.Visible = true;
                        nudRating.Visible = false;
                    }
                }
                if (nudRating.Maximum != 0)
                {
                    if (chkHideOverAvailLimit.Checked)
                    {
                        while (nudRating.Maximum > intMinRating && !xmlVehicleMod.CheckAvailRestriction(_objCharacter, nudRating.MaximumAsInt))
                        {
                            --nudRating.Maximum;
                        }
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")?.Value))
                            decCostMultiplier *= 0.9m;
                        while (nudRating.Maximum > intMinRating && !xmlVehicleMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                        {
                            --nudRating.Maximum;
                        }
                    }
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = nudRating.Maximum != nudRating.Minimum;
                }

                // Slots.

                string strSlots = xmlVehicleMod.SelectSingleNode("slots")?.Value ?? string.Empty;
                if (strSlots.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strSlots.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strSlots = strValues[nudRating.ValueAsInt - 1];
                }
                int.TryParse(strSlots, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intExtraSlots);
                strSlots = ReplaceStrings(strSlots, intExtraSlots);
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSlots, out bool blnIsSuccess);
                lblSlots.Text = blnIsSuccess ? ((double)objProcess).StandardRound().ToString(GlobalSettings.CultureInfo) : strSlots;
                lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);

                int.TryParse(lblSlots.Text, NumberStyles.Any, GlobalSettings.CultureInfo, out intExtraSlots);

                // Avail.
                lblAvail.Text = new AvailabilityValue(Convert.ToInt32(nudRating.Value), xmlVehicleMod.SelectSingleNode("avail")?.Value).ToString();
                lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

                // Cost.
                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                    lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
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
                            lblCost.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+";
                        else
                        {
                            string strSpace = LanguageManager.GetString("String_Space");
                            lblCost.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                           + strSpace + '-' + strSpace
                                           + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        }

                        strCost = decMin.ToString(GlobalSettings.InvariantCultureInfo);
                    }
                    else if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        int intRating = nudRating.ValueAsInt - 1;
                        strCost = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                        string[] strValues = strCost.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        if (intRating < 0 || intRating > strValues.Length)
                        {
                            intRating = 0;
                        }
                        strCost = strValues[intRating];
                    }
                    strCost = ReplaceStrings(strCost, intExtraSlots);

                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out blnIsSuccess);
                    if (blnIsSuccess)
                        decItemCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);

                    // Apply any markup.
                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                    if (chkBlackMarketDiscount.Checked)
                    {
                        decItemCost *= 0.9m;
                    }

                    lblCost.Text = decItemCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                }
                lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);

                // Update the Avail Test Label.
                lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);
                lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);

                string strCategory = xmlVehicleMod.SelectSingleNode("category")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strCategory))
                {
                    if (Vehicle.ModCategoryStrings.Contains(strCategory))
                    {
                        lblVehicleCapacityLabel.Visible = true;
                        lblVehicleCapacity.Visible = true;
                        int.TryParse(lblSlots.Text, NumberStyles.Any, GlobalSettings.CultureInfo, out int intSlots);
                        lblVehicleCapacity.Text = GetRemainingModCapacity(strCategory, intSlots);
                        lblVehicleCapacityLabel.SetToolTip(LanguageManager.GetString("Tip_RemainingVehicleModCapacity"));
                    }
                    else
                    {
                        lblVehicleCapacityLabel.Visible = false;
                        lblVehicleCapacity.Visible = false;
                    }

                    if (strCategory == "Weapon Mod")
                        lblCategory.Text = LanguageManager.GetString("String_WeaponModification");
                    // Translate the Category if possible.
                    else if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objXmlCategoryTranslate = _xmlBaseVehicleDataNode.SelectSingleNode("modcategories/category[. = " + strCategory.CleanXPath() + "]/@translate");
                        lblCategory.Text = objXmlCategoryTranslate?.Value ?? strCategory;
                    }
                    else
                        lblCategory.Text = strCategory;
                }
                else
                {
                    lblCategory.Text = strCategory;
                    lblVehicleCapacityLabel.Visible = false;
                    lblVehicleCapacity.Visible = false;
                }
                lblCategoryLabel.Visible = !string.IsNullOrEmpty(lblCategory.Text);

                string strLimit = xmlVehicleMod.SelectSingleNode("limit")?.Value;
                if (!string.IsNullOrEmpty(strLimit))
                {
                    // Translate the Limit if possible.
                    if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objXmlLimit = _xmlBaseVehicleDataNode.SelectSingleNode("limits/limit[. = " + strLimit.CleanXPath() + "]/@translate");
                        lblLimit.Text = LanguageManager.GetString("String_Space") + '(' + objXmlLimit?.Value ?? strLimit + ')';
                    }
                    else
                        lblLimit.Text = LanguageManager.GetString("String_Space") + '(' + strLimit + ')';
                }
                else
                    lblLimit.Text = string.Empty;

                if (xmlVehicleMod.SelectSingleNode("ratinglabel") != null)
                    lblRatingLabel.Text = string.Format(GlobalSettings.CultureInfo,
                        LanguageManager.GetString("Label_RatingFormat"),
                        LanguageManager.GetString(xmlVehicleMod.SelectSingleNode("ratinglabel").Value));

                string strSource = xmlVehicleMod.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
                string strPage = xmlVehicleMod.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? xmlVehicleMod.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
                SourceString objSourceString = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                objSourceString.SetControl(lblSource);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                tlpRight.Visible = true;
            }
            else
            {
                tlpRight.Visible = false;
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

        private string ReplaceStrings(string strInput, int intExtraSlots = 0)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInput))
            {
                sbdInput.Append(strInput);
                sbdInput.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo));
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
            await CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
