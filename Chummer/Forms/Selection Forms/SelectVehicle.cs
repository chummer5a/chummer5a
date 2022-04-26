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
                Format = _objCharacter.Settings.NuyenFormat + await LanguageManager.GetStringAsync("String_NuyenSymbol"),
                NullValue = null
            };
            dgvc_Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;
            if (_objCharacter.Created)
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                });
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
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            // Populate the Vehicle Category list.
            string strFilterPrefix = "vehicles/vehicle[(" + _objCharacter.Settings.BookXPath() + ") and category = ";
            foreach (XPathNavigator objXmlCategory in await _xmlBaseVehicleDataNode.SelectAndCacheExpressionAsync("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if (_xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + ']') != null)
                {
                    _lstCategory.Add(new ListItem(strInnerText,
                        (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll")));
            }

            await cboCategory.PopulateWithListItemsAsync(_lstCategory);
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
            });
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private async void lstVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSelectedVehicle();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm();
        }

        private async void ProcessVehicleCostsChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                await RefreshList();
            await UpdateSelectedVehicleCost();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked))
                await RefreshList();
            await UpdateSelectedVehicleCost();
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
        private async ValueTask UpdateSelectedVehicle()
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlVehicle == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout());
            try
            {
                string strHandling = objXmlVehicle.SelectSingleNode("handling")?.Value;
                await lblVehicleHandling.DoThreadSafeAsync(x => x.Text = strHandling);
                string strAccel = objXmlVehicle.SelectSingleNode("accel")?.Value;
                await lblVehicleAccel.DoThreadSafeAsync(x => x.Text = strAccel);
                string strSpeed = objXmlVehicle.SelectSingleNode("speed")?.Value;
                await lblVehicleSpeed.DoThreadSafeAsync(x => x.Text = strSpeed);
                string strPilot = objXmlVehicle.SelectSingleNode("pilot")?.Value;
                await lblVehiclePilot.DoThreadSafeAsync(x => x.Text = strPilot);
                string strBody = objXmlVehicle.SelectSingleNode("body")?.Value;
                await lblVehicleBody.DoThreadSafeAsync(x => x.Text = strBody);
                string strArmor = objXmlVehicle.SelectSingleNode("armor")?.Value;
                await lblVehicleArmor.DoThreadSafeAsync(x => x.Text = strArmor);
                string strSeats = objXmlVehicle.SelectSingleNode("seats")?.Value;
                await lblVehicleSeats.DoThreadSafeAsync(x => x.Text = strSeats);
                string strSensor = objXmlVehicle.SelectSingleNode("sensor")?.Value;
                await lblVehicleSensor.DoThreadSafeAsync(x => x.Text = strSensor);
                await lblVehicleHandlingLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strHandling));
                await lblVehicleAccelLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAccel));
                await lblVehicleSpeedLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSpeed));
                await lblVehiclePilotLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strPilot));
                await lblVehicleBodyLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strBody));
                await lblVehicleArmorLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strArmor));
                await lblVehicleSeatsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSeats));
                await lblVehicleSensorLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSensor));
                AvailabilityValue objTotalAvail
                    = new AvailabilityValue(0, objXmlVehicle.SelectSingleNode("avail")?.Value,
                                            await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked) ? -4 : 0);
                string strAvail = objTotalAvail.ToString();
                await lblVehicleAvail.DoThreadSafeAsync(x => x.Text = strAvail);
                await lblVehicleAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail));

                bool blnCanBlackMarketDiscount
                    = _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNode("category")?.Value);
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

                await UpdateSelectedVehicleCost();

                string strSource = objXmlVehicle.SelectSingleNode("source")?.Value
                                   ?? await LanguageManager.GetStringAsync("String_Unknown");
                string strPage = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value
                                 ?? objXmlVehicle.SelectSingleNode("page")?.Value
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

        /// <summary>
        /// Refresh the cost information for the selected Vehicle.
        /// </summary>
        private async ValueTask UpdateSelectedVehicleCost()
        {
            string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlVehicle == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                return;
            }

            // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            string strCost = objXmlVehicle.SelectSingleNode("cost")?.Value ?? string.Empty;
            if (strCost.StartsWith("Variable", StringComparison.Ordinal))
            {
                strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                await lblVehicleCost.DoThreadSafeAsync(x => x.Text = strCost);
                await lblVehicleCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strCost));
                await lblTest.DoThreadSafeAsync(x => x.Text = string.Empty);
                await lblTestLabel.DoThreadSafeAsync(x => x.Visible = false);
            }
            else
            {
                decimal decCost = 0.0m;
                if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked) && decimal.TryParse(strCost, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decTmp))
                {
                    decCost = decTmp;

                    if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked))
                        decCost *= 1.0m - (await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                    // Apply the markup if applicable.
                    decCost *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);

                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked))
                        decCost *= 0.9m;
                    if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps, objXmlVehicle.SelectSingleNode("category")?.Value))
                        decCost *= 0.9m;
                }

                strCost = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol");
                await lblVehicleCost.DoThreadSafeAsync(x => x.Text = strCost);
                await lblVehicleCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strCost));
                string strTest = _objCharacter.AvailTest(decCost, await lblVehicleAvail.DoThreadSafeFuncAsync(x => x.Text));
                await lblTest.DoThreadSafeAsync(x => x.Text = strTest);
                await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest));
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
            await this.DoThreadSafeAsync(x => x.SuspendLayout());
            try
            {
                int intOverLimit = 0;
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked);
                decimal decBaseCostMultiplier = 1.0m;
                if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked))
                    decBaseCostMultiplier -= (await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                decBaseCostMultiplier *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                bool blnHasSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength != 0);
                if (await tabViews.DoThreadSafeFuncAsync(x => x.SelectedIndex) == 1)
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
                        if (blnHideOverAvailLimit && !objXmlVehicle.CheckAvailRestriction(_objCharacter))
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
                                                                        "category"))
                                                                ?.Value))
                                decCostMultiplier *= 0.9m;
                            if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps,
                                                                  (await objXmlVehicle
                                                                      .SelectSingleNodeAndCacheExpressionAsync(
                                                                          "category"))
                                                                  ?.Value))
                                decCostMultiplier *= 0.9m;
                            if (!objXmlVehicle.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier))
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
                                                _objCharacter);
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
                    });
                }
                else
                {
                    string strSpace = await LanguageManager.GetStringAsync("String_Space");
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstVehicles))
                    {
                        foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
                        {
                            if (blnHideOverAvailLimit && !objXmlVehicle.CheckAvailRestriction(_objCharacter))
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
                                                                            "category"))
                                                                    ?.Value))
                                    decCostMultiplier *= 0.9m;
                                if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps,
                                                                      (await objXmlVehicle
                                                                          .SelectSingleNodeAndCacheExpressionAsync(
                                                                              "category"))
                                                                      ?.Value))
                                    decCostMultiplier *= 0.9m;
                                if (!objXmlVehicle.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier))
                                {
                                    ++intOverLimit;
                                    continue;
                                }
                            }

                            string strDisplayname
                                = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                  ?? await LanguageManager.GetStringAsync("String_Unknown");

                            if (!GlobalSettings.SearchInCategoryOnly && blnHasSearch)
                            {
                                string strCategory
                                    = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("category"))?.Value;
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
                                                (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("id"))
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
                                                                           "String_RestrictedItemsHidden"),
                                                                       intOverLimit)));
                        }

                        string strOldSelected = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                        _blnLoading = true;
                        await lstVehicle.PopulateWithListItemsAsync(lstVehicles);
                        _blnLoading = false;
                        await lstVehicle.DoThreadSafeAsync(x =>
                        {
                            if (string.IsNullOrEmpty(strOldSelected))
                                x.SelectedIndex = -1;
                            else
                                x.SelectedValue = strOldSelected;
                        });
                    }
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout());
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm()
        {
            XPathNavigator xmlVehicle = null;
            switch (tabViews.SelectedIndex)
            {
                case 0:
                    string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        xmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + ']');
                        if (xmlVehicle != null)
                        {
                            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength) == 0)
                                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                : xmlVehicle.SelectSingleNode("category")?.Value;
                            _strSelectedVehicle = xmlVehicle.SelectSingleNode("id")?.Value;
                            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value);
                            _blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked);

                            await this.DoThreadSafeAsync(x =>
                            {
                                x.DialogResult = DialogResult.OK;
                                x.Close();
                            });
                        }
                    }

                    break;

                case 1:
                    if (await dgvVehicles.DoThreadSafeFuncAsync(x => x.SelectedRows.Count) == 1)
                    {
                        string strWeapon = await dgvVehicles.DoThreadSafeFuncAsync(x => x.SelectedRows[0].Cells[0].Value.ToString());
                        if (!string.IsNullOrEmpty(strWeapon))
                            strWeapon = strWeapon.Substring(0, strWeapon.LastIndexOf('(') - 1);
                        xmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("/chummer/vehicles/vehicle[id = " + strWeapon.CleanXPath() + ']');
                        if (xmlVehicle != null)
                        {
                            _strSelectCategory
                                = (GlobalSettings.SearchInCategoryOnly
                                   || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength) == 0)
                                    ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                    : xmlVehicle.SelectSingleNode("category")?.Value;
                            _strSelectedVehicle = xmlVehicle.SelectSingleNode("id")?.Value;
                        }
                        _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value);

                        await this.DoThreadSafeAsync(x =>
                        {
                            x.DialogResult = DialogResult.OK;
                            x.Close();
                        });
                    }
                    break;
            }

            if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked))
            {
                decimal decCost = Convert.ToDecimal(xmlVehicle?.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                decCost *= 1 - (await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);

                _blnUsedVehicle = true;
                _strUsedAvail = (await lblVehicleAvail.DoThreadSafeFuncAsync(x => x.Text))
                                .Replace(await LanguageManager.GetStringAsync("String_AvailRestricted"), "R")
                                .Replace(await LanguageManager.GetStringAsync("String_AvailForbidden"), "F");
                _decUsedCost = decCost;
            }

            _blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked);
            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength) == 0)
                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                : xmlVehicle?.SelectSingleNode("category")?.Value;
            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            });
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
