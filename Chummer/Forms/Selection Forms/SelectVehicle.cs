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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectVehicle : Form
    {
        private string _strSelectedVehicle = string.Empty;
        private bool _blnUsedVehicle;
        private string _strUsedAvail = string.Empty;
        private decimal _decUsedCost;
        private decimal _decMarkup;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private readonly HashSet<string> _setDealerConnectionMaps = Utils.StringHashSetPool.Get();
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private bool _blnBlackMarketDiscount;

        #region Control Events

        public SelectVehicle(Character objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            Disposed += (sender, args) =>
            {
                Utils.ListItemListPool.Return(_lstCategory);
                Utils.StringHashSetPool.Return(_setDealerConnectionMaps);
                Utils.StringHashSetPool.Return(_setBlackMarketMaps);
            };
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode));

            if (_objCharacter.DealerConnectionDiscount)
            {
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.DealerConnection))
                {
                    _setDealerConnectionMaps.Add(objImprovement.UniqueName);
                }
            }
        }

        private async void SelectVehicle_Load(object sender, EventArgs e)
        {
            DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.TopRight,
                Format = _objCharacter.Settings.NuyenFormat + await LanguageManager.GetStringAsync("String_NuyenSymbol").ConfigureAwait(false),
                NullValue = null
            };
            dgvc_Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;
            if (_objCharacter.Created)
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                }).ConfigureAwait(false);
            }
            else
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(
                        GlobalSettings.CultureInfo, x.Text,
                        _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount).ConfigureAwait(false);

            // Populate the Vehicle Category list.
            string strFilterPrefix = "vehicles/vehicle[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and category = ";
            foreach (XPathNavigator objXmlCategory in await _xmlBaseVehicleDataNode.SelectAndCacheExpressionAsync("categories/category").ConfigureAwait(false))
            {
                string strInnerText = objXmlCategory.Value;
                if (_xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + ']') != null)
                {
                    _lstCategory.Add(new ListItem(strInnerText,
                        (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate").ConfigureAwait(false))?.Value ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }

            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);
            _blnLoading = false;
            // Select the first Category in the list.
            await cboCategory.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedIndex = 0;
                else
                    x.SelectedValue = _strSelectCategory;
                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
        }

        private async void lstVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSelectedVehicle().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm().ConfigureAwait(false);
        }

        private async void ProcessVehicleCostsChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await RefreshList().ConfigureAwait(false);
            await UpdateSelectedVehicleCost().ConfigureAwait(false);
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await RefreshList().ConfigureAwait(false);
            await UpdateSelectedVehicleCost().ConfigureAwait(false);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstVehicle.SelectedIndex + 1 < lstVehicle.Items.Count:
                    lstVehicle.SelectedIndex++;
                    break;

                case Keys.Down:
                    {
                        if (lstVehicle.Items.Count > 0)
                        {
                            lstVehicle.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstVehicle.SelectedIndex - 1 >= 0:
                    lstVehicle.SelectedIndex--;
                    break;

                case Keys.Up:
                    {
                        if (lstVehicle.Items.Count > 0)
                        {
                            lstVehicle.SelectedIndex = lstVehicle.Items.Count - 1;
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
        /// Name of Vehicle that was selected in the dialogue.
        /// </summary>
        public string SelectedVehicle => _strSelectedVehicle;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool UsedVehicle => _blnUsedVehicle;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// Cost of the Vehicle's cost by when it is used.
        /// </summary>
        public decimal UsedCost => _decUsedCost;

        /// <summary>
        /// Vehicle's Availability when it is used.
        /// </summary>
        public string UsedAvail => _strUsedAvail;

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Refresh the information for the selected Vehicle.
        /// </summary>
        private async ValueTask UpdateSelectedVehicle(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlVehicle == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
            try
            {
                string strHandling = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("handling", token))?.Value;
                await lblVehicleHandling.DoThreadSafeAsync(x => x.Text = strHandling, token: token).ConfigureAwait(false);
                string strAccel = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("accel", token))?.Value;
                await lblVehicleAccel.DoThreadSafeAsync(x => x.Text = strAccel, token: token).ConfigureAwait(false);
                string strSpeed = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("speed", token))?.Value;
                await lblVehicleSpeed.DoThreadSafeAsync(x => x.Text = strSpeed, token: token).ConfigureAwait(false);
                string strPilot = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("pilot", token))?.Value;
                await lblVehiclePilot.DoThreadSafeAsync(x => x.Text = strPilot, token: token).ConfigureAwait(false);
                string strBody = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("body", token))?.Value;
                await lblVehicleBody.DoThreadSafeAsync(x => x.Text = strBody, token: token).ConfigureAwait(false);
                string strArmor = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("armor", token))?.Value;
                await lblVehicleArmor.DoThreadSafeAsync(x => x.Text = strArmor, token: token).ConfigureAwait(false);
                string strSeats = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("seats", token))?.Value;
                await lblVehicleSeats.DoThreadSafeAsync(x => x.Text = strSeats, token: token).ConfigureAwait(false);
                string strSensor = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("sensor", token))?.Value;
                await lblVehicleSensor.DoThreadSafeAsync(x => x.Text = strSensor, token: token).ConfigureAwait(false);
                await lblVehicleHandlingLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strHandling), token: token).ConfigureAwait(false);
                await lblVehicleAccelLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAccel), token: token).ConfigureAwait(false);
                await lblVehicleSpeedLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSpeed), token: token).ConfigureAwait(false);
                await lblVehiclePilotLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strPilot), token: token).ConfigureAwait(false);
                await lblVehicleBodyLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strBody), token: token).ConfigureAwait(false);
                await lblVehicleArmorLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strArmor), token: token).ConfigureAwait(false);
                await lblVehicleSeatsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSeats), token: token).ConfigureAwait(false);
                await lblVehicleSensorLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSensor), token: token).ConfigureAwait(false);
                AvailabilityValue objTotalAvail
                    = new AvailabilityValue(0, (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("avail", token))?.Value,
                                            await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) ? -4 : 0);
                string strAvail = objTotalAvail.ToString();
                await lblVehicleAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
                await lblVehicleAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token).ConfigureAwait(false);

                bool blnCanBlackMarketDiscount
                    = _setBlackMarketMaps.Contains((await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("category", token))?.Value);
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

                await UpdateSelectedVehicleCost(token).ConfigureAwait(false);

                string strSource = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("source", token))?.Value
                                   ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                string strPage = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token).ConfigureAwait(false))?.Value
                                 ?? (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("page", token))?.Value
                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                SourceString objSource = await SourceString.GetSourceStringAsync(
                    strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter, token: token).ConfigureAwait(false);
                await objSource.SetControlAsync(lblSource, token: token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()), token: token).ConfigureAwait(false);
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout(), token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refresh the cost information for the selected Vehicle.
        /// </summary>
        private async ValueTask UpdateSelectedVehicleCost(CancellationToken token = default)
        {
            string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlVehicle == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                return;
            }

            // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            string strCost = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("cost", token))?.Value ?? string.Empty;
            if (strCost.StartsWith("Variable", StringComparison.Ordinal))
            {
                strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                await lblVehicleCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                await lblVehicleCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strCost), token: token).ConfigureAwait(false);
                await lblTest.DoThreadSafeAsync(x => x.Text = string.Empty, token: token).ConfigureAwait(false);
                await lblTestLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
            }
            else
            {
                decimal decCost = 0.0m;
                if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && decimal.TryParse(strCost, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decTmp))
                {
                    decCost = decTmp;

                    if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        decCost *= 1.0m - (await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m);
                    // Apply the markup if applicable.
                    decCost *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m);

                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        decCost *= 0.9m;
                    if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps, objXmlVehicle.SelectSingleNode("category")?.Value))
                        decCost *= 0.9m;
                }

                strCost = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                await lblVehicleCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                await lblVehicleCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strCost), token: token).ConfigureAwait(false);
                string strTest = await _objCharacter.AvailTestAsync(decCost, await lblVehicleAvail.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token).ConfigureAwait(false);
            }
        }

        private ValueTask RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || txtSearch.TextLength == 0))
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                else
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
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                        }
                    }
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }
            return BuildVehicleList(_xmlBaseVehicleDataNode.Select("vehicles/vehicle" + strFilter));
        }

        private async ValueTask BuildVehicleList(XPathNodeIterator objXmlVehicleList)
        {
            await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                int intOverLimit = 0;
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                bool blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                decimal decBaseCostMultiplier = 1.0m;
                if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    decBaseCostMultiplier -= (await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false) / 100.0m);
                decBaseCostMultiplier *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false) / 100.0m);
                bool blnHasSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength != 0).ConfigureAwait(false);
                if (await tabViews.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false) == 1)
                {
                    XmlDocument dummy = new XmlDocument {XmlResolver = null};
                    DataTable tabVehicles = new DataTable("vehicles");
                    tabVehicles.Columns.Add("VehicleGuid");
                    tabVehicles.Columns.Add("VehicleName");
                    tabVehicles.Columns.Add("Accel");
                    tabVehicles.Columns.Add("Armor");
                    tabVehicles.Columns.Add("Body");
                    tabVehicles.Columns.Add("Handling");
                    tabVehicles.Columns.Add("Pilot");
                    tabVehicles.Columns.Add("Sensor");
                    tabVehicles.Columns.Add("Speed");
                    tabVehicles.Columns.Add("Seats");
                    tabVehicles.Columns.Add("Gear");
                    tabVehicles.Columns.Add("Mods");
                    tabVehicles.Columns.Add("Weapons");
                    tabVehicles.Columns.Add("WeaponMounts");
                    tabVehicles.Columns.Add("Avail", typeof(AvailabilityValue));
                    tabVehicles.Columns.Add("Source", typeof(SourceString));
                    tabVehicles.Columns.Add("Cost", typeof(NuyenString));

                    foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
                    {
                        if (blnHideOverAvailLimit && !await objXmlVehicle.CheckAvailRestrictionAsync(_objCharacter).ConfigureAwait(false))
                        {
                            ++intOverLimit;
                            continue;
                        }

                        if (!blnFreeItem && blnShowOnlyAffordItems)
                        {
                            decimal decCostMultiplier = decBaseCostMultiplier;
                            if (blnBlackMarketDiscount
                                && _setBlackMarketMaps.Contains((await objXmlVehicle
                                                                       .SelectSingleNodeAndCacheExpressionAsync(
                                                                           "category").ConfigureAwait(false))
                                                                ?.Value))
                                decCostMultiplier *= 0.9m;
                            if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps,
                                                                  (await objXmlVehicle
                                                                         .SelectSingleNodeAndCacheExpressionAsync(
                                                                             "category").ConfigureAwait(false))
                                                                  ?.Value))
                                decCostMultiplier *= 0.9m;
                            if (!await objXmlVehicle.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier).ConfigureAwait(false))
                            {
                                ++intOverLimit;
                                continue;
                            }
                        }

                        using (Vehicle objVehicle = new Vehicle(_objCharacter))
                        {
                            objVehicle.Create(objXmlVehicle.ToXmlNode(dummy), true, true, false, true);
                            string strID = objVehicle.SourceIDString;
                            string strVehicleName = objVehicle.CurrentDisplayName;
                            string strAccel = objVehicle.TotalAccel;
                            string strArmor = objVehicle.TotalArmor.ToString(GlobalSettings.CultureInfo);
                            string strBody = objVehicle.TotalBody.ToString(GlobalSettings.CultureInfo);
                            string strHandling = objVehicle.TotalHandling;
                            string strPilot = objVehicle.Pilot.ToString(GlobalSettings.CultureInfo);
                            string strSensor = objVehicle.CalculatedSensor.ToString(GlobalSettings.CultureInfo);
                            string strSpeed = objVehicle.TotalSpeed;
                            string strSeats = objVehicle.TotalSeats.ToString(GlobalSettings.CultureInfo);
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdGear))
                            {
                                foreach (Gear objGear in objVehicle.GearChildren)
                                {
                                    sbdGear.AppendLine(objGear.CurrentDisplayName);
                                }

                                if (sbdGear.Length > 0)
                                    sbdGear.Length -= Environment.NewLine.Length;

                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdMods))
                                {
                                    foreach (VehicleMod objMod in objVehicle.Mods)
                                    {
                                        sbdMods.AppendLine(objMod.CurrentDisplayName);
                                    }

                                    if (sbdMods.Length > 0)
                                        sbdMods.Length -= Environment.NewLine.Length;
                                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdWeapons))
                                    {
                                        if (sbdWeapons.Length > 0)
                                            sbdWeapons.Length -= Environment.NewLine.Length;
                                        foreach (Weapon objWeapon in objVehicle.Weapons)
                                        {
                                            sbdWeapons.AppendLine(objWeapon.CurrentDisplayName);
                                        }

                                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                   out StringBuilder sbdWeaponMounts))
                                        {
                                            foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                                            {
                                                sbdWeaponMounts.AppendLine(objWeaponMount.CurrentDisplayName);
                                            }

                                            if (sbdWeaponMounts.Length > 0)
                                                sbdWeaponMounts.Length -= Environment.NewLine.Length;

                                            AvailabilityValue objAvail = objVehicle.TotalAvailTuple();
                                            SourceString strSource = await SourceString.GetSourceStringAsync(
                                                objVehicle.Source,
                                                objVehicle.DisplayPage(GlobalSettings.Language),
                                                GlobalSettings.Language, GlobalSettings.CultureInfo,
                                                _objCharacter).ConfigureAwait(false);
                                            NuyenString strCost =
                                                new NuyenString(
                                                    objVehicle.TotalCost.ToString(GlobalSettings.CultureInfo));

                                            tabVehicles.Rows.Add(strID, strVehicleName, strAccel, strArmor, strBody,
                                                                 strHandling, strPilot, strSensor, strSpeed, strSeats,
                                                                 sbdGear.ToString(), sbdMods.ToString(),
                                                                 sbdWeapons.ToString(), sbdWeaponMounts.ToString(),
                                                                 objAvail, strSource, strCost);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await dgvVehicles.DoThreadSafeAsync(x =>
                    {
                        x.Columns[0].Visible = false;
                        x.Columns[13].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                        x.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                        DataSet set = new DataSet("vehicles");
                        set.Tables.Add(tabVehicles);
                        x.DataSource = set;
                        x.DataMember = "vehicles";
                    }).ConfigureAwait(false);
                }
                else
                {
                    string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstVehicles))
                    {
                        foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
                        {
                            if (blnHideOverAvailLimit && !await objXmlVehicle.CheckAvailRestrictionAsync(_objCharacter).ConfigureAwait(false))
                            {
                                ++intOverLimit;
                                continue;
                            }

                            if (!blnFreeItem && blnShowOnlyAffordItems)
                            {
                                decimal decCostMultiplier = decBaseCostMultiplier;
                                if (blnBlackMarketDiscount
                                    && _setBlackMarketMaps.Contains((await objXmlVehicle
                                                                           .SelectSingleNodeAndCacheExpressionAsync(
                                                                               "category").ConfigureAwait(false))
                                                                    ?.Value))
                                    decCostMultiplier *= 0.9m;
                                if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps,
                                                                      (await objXmlVehicle
                                                                             .SelectSingleNodeAndCacheExpressionAsync(
                                                                                 "category").ConfigureAwait(false))
                                                                      ?.Value))
                                    decCostMultiplier *= 0.9m;
                                if (!await objXmlVehicle.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier).ConfigureAwait(false))
                                {
                                    ++intOverLimit;
                                    continue;
                                }
                            }

                            string strDisplayname
                                = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                  ?? (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false))?.Value
                                  ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);

                            if (!GlobalSettings.SearchInCategoryOnly && blnHasSearch)
                            {
                                string strCategory
                                    = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("category").ConfigureAwait(false))?.Value;
                                if (!string.IsNullOrEmpty(strCategory))
                                {
                                    ListItem objFoundItem
                                        = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                                    if (!string.IsNullOrEmpty(objFoundItem.Name))
                                    {
                                        strDisplayname += strSpace + '[' + objFoundItem.Name + ']';
                                    }
                                }
                            }

                            lstVehicles.Add(new ListItem(
                                                (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("id").ConfigureAwait(false))
                                                ?.Value ?? string.Empty,
                                                strDisplayname));
                        }

                        lstVehicles.Sort(CompareListItems.CompareNames);
                        if (intOverLimit > 0)
                        {
                            // Add after sort so that it's always at the end
                            lstVehicles.Add(new ListItem(string.Empty,
                                                         string.Format(GlobalSettings.CultureInfo,
                                                                       await LanguageManager.GetStringAsync(
                                                                           "String_RestrictedItemsHidden").ConfigureAwait(false),
                                                                       intOverLimit)));
                        }

                        string strOldSelected = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
                        _blnLoading = true;
                        await lstVehicle.PopulateWithListItemsAsync(lstVehicles).ConfigureAwait(false);
                        _blnLoading = false;
                        await lstVehicle.DoThreadSafeAsync(x =>
                        {
                            if (string.IsNullOrEmpty(strOldSelected))
                                x.SelectedIndex = -1;
                            else
                                x.SelectedValue = strOldSelected;
                        }).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            XPathNavigator xmlVehicle = null;
            switch (tabViews.SelectedIndex)
            {
                case 0:
                    string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        xmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + ']');
                        if (xmlVehicle != null)
                        {
                            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0)
                                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false)
                                : (await xmlVehicle.SelectSingleNodeAndCacheExpressionAsync("category", token))?.Value;
                            _strSelectedVehicle = (await xmlVehicle.SelectSingleNodeAndCacheExpressionAsync("id", token))?.Value;
                            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                            _blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);

                            await this.DoThreadSafeAsync(x =>
                            {
                                x.DialogResult = DialogResult.OK;
                                x.Close();
                            }, token: token).ConfigureAwait(false);
                        }
                    }

                    break;

                case 1:
                    if (await dgvVehicles.DoThreadSafeFuncAsync(x => x.SelectedRows.Count, token: token).ConfigureAwait(false) == 1)
                    {
                        string strWeapon = await dgvVehicles.DoThreadSafeFuncAsync(x => x.SelectedRows[0].Cells[0].Value.ToString(), token: token).ConfigureAwait(false);
                        xmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("/chummer/vehicles/vehicle[id = " + strWeapon.CleanXPath() + ']');
                        if (xmlVehicle != null)
                        {
                            _strSelectCategory
                                = (GlobalSettings.SearchInCategoryOnly
                                   || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0)
                                    ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false)
                                    : (await xmlVehicle.SelectSingleNodeAndCacheExpressionAsync("category", token))?.Value;
                            _strSelectedVehicle = (await xmlVehicle.SelectSingleNodeAndCacheExpressionAsync("id", token))?.Value;
                        }
                        _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

                        await this.DoThreadSafeAsync(x =>
                        {
                            x.DialogResult = DialogResult.OK;
                            x.Close();
                        }, token: token).ConfigureAwait(false);
                    }
                    break;
            }

            if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
            {
                decimal decCost = xmlVehicle != null
                    ? Convert.ToDecimal((await xmlVehicle.SelectSingleNodeAndCacheExpressionAsync("cost", token))?.Value, GlobalSettings.InvariantCultureInfo)
                    : 0;
                decCost *= 1 - (await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m);

                _blnUsedVehicle = true;
                _strUsedAvail = (await lblVehicleAvail.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false))
                                .Replace(await LanguageManager.GetStringAsync("String_AvailRestricted", token: token).ConfigureAwait(false), "R")
                                .Replace(await LanguageManager.GetStringAsync("String_AvailForbidden", token: token).ConfigureAwait(false), "F");
                _decUsedCost = decCost;
            }

            _blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            _strSelectCategory = GlobalSettings.SearchInCategoryOnly
                                 || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token)
                                                   .ConfigureAwait(false) == 0
                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                   .ConfigureAwait(false)
                : xmlVehicle != null
                    ? (await xmlVehicle.SelectSingleNodeAndCacheExpressionAsync("category", token))?.Value
                    : string.Empty;
            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
