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
        private bool _blnFreeCost;

        private int _intLoading = 1;
        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;
        private List<ListItem> _lstCategory;
        private HashSet<string> _setDealerConnectionMaps;
        private HashSet<string> _setBlackMarketMaps;
        private bool _blnBlackMarketDiscount;

        private CancellationTokenSource _objUpdateSelectedVehicleCancellationTokenSource;
        private CancellationTokenSource _objUpdateSelectedVehicleCostCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource;
        private readonly CancellationToken _objGenericToken;

        #region Control Events

        public SelectVehicle(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            tabViews.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _lstCategory = Utils.ListItemListPool.Get();
            _setDealerConnectionMaps = Utils.StringHashSetPool.Get();
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            _objGenericCancellationTokenSource = new CancellationTokenSource();
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateSelectedVehicleCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateSelectedVehicleCostCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericCancellationTokenSource.Dispose();
                Utils.ListItemListPool.Return(ref _lstCategory);
                Utils.StringHashSetPool.Return(ref _setDealerConnectionMaps);
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
            };
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode));
        }

        private async void SelectVehicle_Load(object sender, EventArgs e)
        {
            try
            {
                if (await _objCharacter.GetDealerConnectionDiscountAsync(_objGenericToken).ConfigureAwait(false))
                {
                    foreach (Improvement objImprovement in await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                     Improvement.ImprovementType.DealerConnection, token: _objGenericToken)
                                 .ConfigureAwait(false))
                    {
                        _setDealerConnectionMaps.Add(objImprovement.UniqueName);
                    }
                }

                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false);
                DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.TopRight,
                    Format = await objSettings.GetNuyenFormatAsync(_objGenericToken).ConfigureAwait(false) +
                             await LanguageManager.GetStringAsync("String_NuyenSymbol", token: _objGenericToken)
                                 .ConfigureAwait(false),
                    NullValue = null
                };
                dgvc_Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

                bool blnBlackMarketDiscount = await _objCharacter.GetBlackMarketDiscountAsync(_objGenericToken).ConfigureAwait(false);
                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = blnBlackMarketDiscount, _objGenericToken).ConfigureAwait(false);

                if (await _objCharacter.GetCreatedAsync(_objGenericToken).ConfigureAwait(false))
                {
                    await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                    await nudMarkup.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                    await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    await nudMarkup.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    int intMaxAvail = await objSettings.GetMaximumAvailabilityAsync(_objGenericToken).ConfigureAwait(false);
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Text = string.Format(GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                        x.Visible = true;
                        x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                    }, _objGenericToken).ConfigureAwait(false);
                }

                // Populate the Vehicle Category list.
                string strFilterPrefix = "vehicles/vehicle[(" + await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).BookXPathAsync(token: _objGenericToken).ConfigureAwait(false) + ") and category = ";
                foreach (XPathNavigator objXmlCategory in _xmlBaseVehicleDataNode.SelectAndCacheExpression("categories/category", _objGenericToken))
                {
                    string strInnerText = objXmlCategory.Value;
                    if (_xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + ']') != null)
                    {
                        _lstCategory.Add(new ListItem(strInnerText,
                            objXmlCategory.SelectSingleNodeAndCacheExpression("@translate", _objGenericToken)?.Value ?? strInnerText));
                    }
                }
                _lstCategory.Sort(CompareListItems.CompareNames);

                if (_lstCategory.Count > 0)
                {
                    _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: _objGenericToken).ConfigureAwait(false)));
                }

                await cboCategory.PopulateWithListItemsAsync(_lstCategory, _objGenericToken).ConfigureAwait(false);
                Interlocked.Decrement(ref _intLoading);
                // Select the first Category in the list.
                await cboCategory.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrEmpty(_strSelectCategory))
                        x.SelectedIndex = 0;
                    else
                        x.SelectedValue = _strSelectCategory;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SelectVehicle_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateSelectedVehicleCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateSelectedVehicleCostCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void lstVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await UpdateSelectedVehicle(_objGenericToken).ConfigureAwait(false);
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
                _blnAddAgain = false;
                await AcceptForm(_objGenericToken).ConfigureAwait(false);
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

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            try
            {
                _blnAddAgain = true;
                await AcceptForm(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ProcessVehicleCostsChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false)
                    && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                    await RefreshList(_objGenericToken).ConfigureAwait(false);
                await UpdateSelectedVehicleCost(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                    await RefreshList(_objGenericToken).ConfigureAwait(false);
                await UpdateSelectedVehicleCost(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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
                case Keys.Up when lstVehicle.SelectedIndex >= 1:
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
                txtSearch.Select(txtSearch.TextLength, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Name of Vehicle that was selected in the dialogue.
        /// </summary>
        public string SelectedVehicle => _strSelectedVehicle;

        /// <summary>
        /// Whether the selected Vehicle is used.
        /// </summary>
        public bool UsedVehicle => _blnUsedVehicle;

        /// <summary>
        /// Whether the selected Vehicle is used.
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
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Refresh the information for the selected Vehicle.
        /// </summary>
        private async Task UpdateSelectedVehicle(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;

            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateSelectedVehicleCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                XPathNavigator objXmlVehicle = null;
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    // Retrieve the information for the selected Vehicle.
                    objXmlVehicle = _xmlBaseVehicleDataNode.TryGetNodeByNameOrId("vehicles/vehicle", strSelectedId);
                }
                if (objXmlVehicle == null)
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    return;
                }

                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                try
                {
                    string strHandling = objXmlVehicle.SelectSingleNodeAndCacheExpression("handling", token)?.Value;
                    await lblVehicleHandling.DoThreadSafeAsync(x => x.Text = strHandling, token: token).ConfigureAwait(false);
                    string strAccel = objXmlVehicle.SelectSingleNodeAndCacheExpression("accel", token)?.Value;
                    await lblVehicleAccel.DoThreadSafeAsync(x => x.Text = strAccel, token: token).ConfigureAwait(false);
                    string strSpeed = objXmlVehicle.SelectSingleNodeAndCacheExpression("speed", token)?.Value;
                    await lblVehicleSpeed.DoThreadSafeAsync(x => x.Text = strSpeed, token: token).ConfigureAwait(false);
                    string strPilot = objXmlVehicle.SelectSingleNodeAndCacheExpression("pilot", token)?.Value;
                    await lblVehiclePilot.DoThreadSafeAsync(x => x.Text = strPilot, token: token).ConfigureAwait(false);
                    string strBody = objXmlVehicle.SelectSingleNodeAndCacheExpression("body", token)?.Value;
                    await lblVehicleBody.DoThreadSafeAsync(x => x.Text = strBody, token: token).ConfigureAwait(false);
                    string strArmor = objXmlVehicle.SelectSingleNodeAndCacheExpression("armor", token)?.Value;
                    await lblVehicleArmor.DoThreadSafeAsync(x => x.Text = strArmor, token: token).ConfigureAwait(false);
                    string strSeats = objXmlVehicle.SelectSingleNodeAndCacheExpression("seats", token)?.Value;
                    await lblVehicleSeats.DoThreadSafeAsync(x => x.Text = strSeats, token: token).ConfigureAwait(false);
                    string strSensor = objXmlVehicle.SelectSingleNodeAndCacheExpression("sensor", token)?.Value;
                    await lblVehicleSensor.DoThreadSafeAsync(x => x.Text = strSensor, token: token).ConfigureAwait(false);
                    await lblVehicleHandlingLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strHandling), token: token).ConfigureAwait(false);
                    await lblVehicleAccelLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAccel), token: token).ConfigureAwait(false);
                    await lblVehicleSpeedLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSpeed), token: token).ConfigureAwait(false);
                    await lblVehiclePilotLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strPilot), token: token).ConfigureAwait(false);
                    await lblVehicleBodyLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strBody), token: token).ConfigureAwait(false);
                    await lblVehicleArmorLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strArmor), token: token).ConfigureAwait(false);
                    await lblVehicleSeatsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSeats), token: token).ConfigureAwait(false);
                    await lblVehicleSensorLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSensor), token: token).ConfigureAwait(false);
                    int intAvailModifier = (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlVehicle.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound();
                    if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        intAvailModifier -= 4;
                    AvailabilityValue objTotalAvail
                        = new AvailabilityValue(0, objXmlVehicle.SelectSingleNodeAndCacheExpression("avail", token)?.Value,
                                                intAvailModifier);
                    string strAvail = await objTotalAvail.ToStringAsync(token).ConfigureAwait(false);
                    await lblVehicleAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
                    await lblVehicleAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token).ConfigureAwait(false);

                    bool blnCanBlackMarketDiscount
                        = _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNodeAndCacheExpression("category", token)?.Value);
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

                    string strSource = objXmlVehicle.SelectSingleNodeAndCacheExpression("source", token)?.Value
                                       ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    string strPage = objXmlVehicle.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value
                                     ?? objXmlVehicle.SelectSingleNodeAndCacheExpression("page", token)?.Value
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
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Refresh the cost information for the selected Vehicle.
        /// </summary>
        private async Task UpdateSelectedVehicleCost(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateSelectedVehicleCostCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strSelectedId = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                XPathNavigator objXmlVehicle = null;
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    // Retrieve the information for the selected Vehicle.
                    objXmlVehicle = _xmlBaseVehicleDataNode.TryGetNodeByNameOrId("vehicles/vehicle", strSelectedId);
                }
                if (objXmlVehicle == null)
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    return;
                }

                // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
                string strCost = objXmlVehicle.SelectSingleNodeAndCacheExpression("cost", token)?.Value ?? string.Empty;
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
                            decCost *= 1.0m - await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                        // Apply the markup if applicable.
                        decCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;

                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            decCost *= 0.9m;
                        if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps, objXmlVehicle.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                            decCost *= 0.9m;
                    }

                    strCost = decCost.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                    await lblVehicleCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                    await lblVehicleCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strCost), token: token).ConfigureAwait(false);
                    string strTest = await _objCharacter.AvailTestAsync(decCost, await lblVehicleAvail.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token).ConfigureAwait(false);
                }
            }
        }

        private async Task RefreshList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string strCategory = await cboCategory
                        .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? string.Empty, token)
                        .ConfigureAwait(false);
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append('(')
                            .Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false))
                            .Append(')');
                        if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                               && (GlobalSettings.SearchInCategoryOnly
                                                                   || txtSearch.TextLength == 0))
                            sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                        else
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                       out StringBuilder sbdCategoryFilter))
                            {
                                foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                                {
                                    if (!string.IsNullOrEmpty(strItem))
                                        sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath())
                                            .Append(" or ");
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

                    await BuildVehicleList(_xmlBaseVehicleDataNode.Select("vehicles/vehicle" + strFilter), token)
                        .ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task BuildVehicleList(XPathNodeIterator objXmlVehicleList, CancellationToken token = default)
        {
            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
            try
            {
                int intOverLimit = 0;
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                bool blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                decimal decBaseCostMultiplier = 1.0m;
                if (await chkUsedVehicle.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                    decBaseCostMultiplier -= await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false) / 100.0m;
                decBaseCostMultiplier *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false) / 100.0m;
                bool blnHasSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength != 0, token).ConfigureAwait(false);
                if (await tabViews.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false) == 1)
                {
                    IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
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

                        decimal decNuyen = blnFreeItem || !blnShowOnlyAffordItems
                            ? decimal.MaxValue
                            : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                        using (new FetchSafelyFromSafeObjectPool<XmlDocument>(Utils.XmlDocumentPool, out XmlDocument dummy))
                        {
                            foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
                            {
                                if (blnHideOverAvailLimit && !await objXmlVehicle
                                        .CheckAvailRestrictionAsync(_objCharacter, intAvailModifier: (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlVehicle.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
                                {
                                    ++intOverLimit;
                                    continue;
                                }

                                if (!blnFreeItem && blnShowOnlyAffordItems)
                                {
                                    decimal decCostMultiplier = decBaseCostMultiplier;
                                    if (blnBlackMarketDiscount
                                        && _setBlackMarketMaps.Contains(objXmlVehicle
                                            .SelectSingleNodeAndCacheExpression(
                                                "category", token)
                                            ?.Value))
                                        decCostMultiplier *= 0.9m;
                                    if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps,
                                            objXmlVehicle
                                                .SelectSingleNodeAndCacheExpression(
                                                    "category", token)
                                                ?.Value))
                                        decCostMultiplier *= 0.9m;
                                    if (!await objXmlVehicle
                                            .CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier, token: token)
                                            .ConfigureAwait(false))
                                    {
                                        ++intOverLimit;
                                        continue;
                                    }
                                }

                                Vehicle objVehicle = new Vehicle(_objCharacter);
                                try
                                {
                                    dummy.RemoveAll();
                                    await objVehicle
                                        .CreateAsync(objXmlVehicle.ToXmlNode(dummy), true, true, false, true, true, token)
                                        .ConfigureAwait(false);
                                    string strID = objVehicle.SourceIDString;
                                    string strVehicleName =
                                        await objVehicle.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    string strAccel = await objVehicle.GetTotalAccelAsync(token).ConfigureAwait(false);
                                    string strArmor =
                                        (await objVehicle.GetTotalArmorAsync(token).ConfigureAwait(false)).ToString(
                                            GlobalSettings.CultureInfo);
                                    string strBody =
                                        (await objVehicle.GetTotalBodyAsync(token).ConfigureAwait(false)).ToString(
                                            GlobalSettings.CultureInfo);
                                    string strHandling =
                                        await objVehicle.GetTotalHandlingAsync(token).ConfigureAwait(false);
                                    string strPilot =
                                        (await objVehicle.GetPilotAsync(token).ConfigureAwait(false)).ToString(
                                            GlobalSettings
                                                .CultureInfo);
                                    string strSensor =
                                        (await objVehicle.GetCalculatedSensorAsync(token).ConfigureAwait(false)).ToString(
                                            GlobalSettings.CultureInfo);
                                    string strSpeed = await objVehicle.GetTotalSpeedAsync(token).ConfigureAwait(false);
                                    string strSeats =
                                        (await objVehicle.GetTotalSeatsAsync(token).ConfigureAwait(false)).ToString(
                                            GlobalSettings.CultureInfo);

                                    string strGear;
                                    string strMods;
                                    string strWeapons;
                                    string strMounts;
                                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdGear))
                                    {
                                        foreach (Gear objGear in objVehicle.GearChildren)
                                        {
                                            sbdGear.AppendLine(await objGear.GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false));
                                        }

                                        if (sbdGear.Length > 0)
                                            sbdGear.Length -= Environment.NewLine.Length;

                                        strGear = sbdGear.ToString();
                                    }

                                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdMods))
                                    {
                                        await objVehicle.Mods.ForEachAsync(async objMod =>
                                        {
                                            sbdMods.AppendLine(await objMod.GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false));
                                        }, token).ConfigureAwait(false);

                                        if (sbdMods.Length > 0)
                                            sbdMods.Length -= Environment.NewLine.Length;

                                        strMods = sbdMods.ToString();
                                    }

                                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdWeapons))
                                    {
                                        if (sbdWeapons.Length > 0)
                                            sbdWeapons.Length -= Environment.NewLine.Length;
                                        await objVehicle.Weapons.ForEachAsync(async objWeapon =>
                                        {
                                            sbdWeapons.AppendLine(await objWeapon.GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false));
                                        }, token).ConfigureAwait(false);

                                        strWeapons = sbdWeapons.ToString();
                                    }

                                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdWeaponMounts))
                                    {
                                        await objVehicle.WeaponMounts.ForEachAsync(async objWeaponMount =>
                                        {
                                            sbdWeaponMounts.AppendLine(await objWeaponMount
                                                .GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false));
                                        }, token).ConfigureAwait(false);

                                        if (sbdWeaponMounts.Length > 0)
                                            sbdWeaponMounts.Length -= Environment.NewLine.Length;

                                        strMounts = sbdWeaponMounts.ToString();
                                    }

                                    AvailabilityValue objAvail =
                                        await objVehicle.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                                    SourceString strSource = await SourceString.GetSourceStringAsync(
                                        objVehicle.Source,
                                        await objVehicle.DisplayPageAsync(GlobalSettings.Language, token)
                                            .ConfigureAwait(false),
                                        GlobalSettings.Language, GlobalSettings.CultureInfo,
                                        _objCharacter, token).ConfigureAwait(false);
                                    NuyenString strCost =
                                        await NuyenString.GetNuyenStringAsync(
                                            (await objVehicle.GetTotalCostAsync(token).ConfigureAwait(false)).ToString(
                                                GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                                    tabVehicles.Rows.Add(strID, strVehicleName, strAccel, strArmor, strBody,
                                        strHandling, strPilot, strSensor, strSpeed, strSeats,
                                        strGear, strMods,
                                        strWeapons, strMounts,
                                        objAvail, strSource, strCost);
                                }
                                finally
                                {
                                    await objVehicle.DisposeAsync().ConfigureAwait(false);
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
                        }, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    decimal decNuyen = blnFreeItem || !blnShowOnlyAffordItems ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstVehicles))
                    {
                        foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
                        {
                            if (blnHideOverAvailLimit && !await objXmlVehicle.CheckAvailRestrictionAsync(_objCharacter, intAvailModifier: (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlVehicle.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
                            {
                                ++intOverLimit;
                                continue;
                            }

                            if (!blnFreeItem && blnShowOnlyAffordItems)
                            {
                                decimal decCostMultiplier = decBaseCostMultiplier;
                                if (blnBlackMarketDiscount
                                    && _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNodeAndCacheExpression(
                                            "category", token)
                                                                    ?.Value))
                                    decCostMultiplier *= 0.9m;
                                if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps,
                                                                      objXmlVehicle
                                                                          .SelectSingleNodeAndCacheExpression(
                                                                              "category", token)
                                                                      ?.Value))
                                    decCostMultiplier *= 0.9m;
                                if (!await objXmlVehicle.CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier, token: token).ConfigureAwait(false))
                                {
                                    ++intOverLimit;
                                    continue;
                                }
                            }

                            string strDisplayname
                                = objXmlVehicle.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                  ?? objXmlVehicle.SelectSingleNodeAndCacheExpression("name", token)?.Value
                                  ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);

                            if (!GlobalSettings.SearchInCategoryOnly && blnHasSearch)
                            {
                                string strCategory
                                    = objXmlVehicle.SelectSingleNodeAndCacheExpression("category", token)?.Value;
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
                                                objXmlVehicle.SelectSingleNodeAndCacheExpression("id", token)?.Value ?? string.Empty,
                                                strDisplayname));
                        }

                        lstVehicles.Sort(CompareListItems.CompareNames);
                        if (intOverLimit > 0)
                        {
                            // Add after sort so that it's always at the end
                            lstVehicles.Add(new ListItem(string.Empty,
                                                         string.Format(GlobalSettings.CultureInfo,
                                                                       await LanguageManager.GetStringAsync(
                                                                           "String_RestrictedItemsHidden", token: token).ConfigureAwait(false),
                                                                       intOverLimit)));
                        }

                        string strOldSelected = await lstVehicle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await lstVehicle.PopulateWithListItemsAsync(lstVehicles, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }
                        await lstVehicle.DoThreadSafeAsync(x =>
                        {
                            if (string.IsNullOrEmpty(strOldSelected))
                                x.SelectedIndex = -1;
                            else
                                x.SelectedValue = strOldSelected;
                        }, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            XPathNavigator xmlVehicle = null;
            switch (tabViews.SelectedIndex)
            {
                case 0:
                    string strSelectedId = await lstVehicle
                                                 .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                                 .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        xmlVehicle = _xmlBaseVehicleDataNode.TryGetNodeByNameOrId("vehicles/vehicle", strSelectedId);
                        if (xmlVehicle != null)
                        {
                            _strSelectCategory = GlobalSettings.SearchInCategoryOnly
                                                 || await txtSearch
                                                     .DoThreadSafeFuncAsync(x => x.TextLength, token: token)
                                                     .ConfigureAwait(false) == 0
                                ? await cboCategory
                                        .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                        .ConfigureAwait(false)
                                : xmlVehicle.SelectSingleNodeAndCacheExpression("category", token)?.Value;
                            _strSelectedVehicle = xmlVehicle.SelectSingleNodeAndCacheExpression("id", token)?.Value;
                            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                                        .ConfigureAwait(false);
                            _blnFreeCost = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                            _blnBlackMarketDiscount = await chkBlackMarketDiscount
                                                            .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                            .ConfigureAwait(false);

                            await this.DoThreadSafeAsync(x =>
                            {
                                x.DialogResult = DialogResult.OK;
                                x.Close();
                            }, token: token).ConfigureAwait(false);
                        }
                    }

                    break;

                case 1:
                    if (await dgvVehicles.DoThreadSafeFuncAsync(x => x.SelectedRows.Count, token: token)
                                         .ConfigureAwait(false) == 1)
                    {
                        string strWeapon = await dgvVehicles
                                                 .DoThreadSafeFuncAsync(
                                                     x => x.SelectedRows[0].Cells[0].Value.ToString(), token: token)
                                                 .ConfigureAwait(false);
                        xmlVehicle = _xmlBaseVehicleDataNode.TryGetNodeByNameOrId("/chummer/vehicles/vehicle", strWeapon);
                        if (xmlVehicle != null)
                        {
                            _strSelectCategory
                                = GlobalSettings.SearchInCategoryOnly
                                  || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token)
                                      .ConfigureAwait(false) == 0
                                    ? await cboCategory
                                            .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                            .ConfigureAwait(false)
                                    : xmlVehicle.SelectSingleNodeAndCacheExpression("category", token)?.Value;
                            _strSelectedVehicle = xmlVehicle.SelectSingleNodeAndCacheExpression("id", token)?.Value;
                        }

                        _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                                    .ConfigureAwait(false);
                        _blnFreeCost = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);

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
                decimal.TryParse(xmlVehicle?.SelectSingleNodeAndCacheExpression("cost", token)?.Value,
                    NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decCost);
                decCost *= 1 - await nudUsedVehicleDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token)
                    .ConfigureAwait(false) / 100.0m;

                _blnUsedVehicle = true;
                _strUsedAvail = (await lblVehicleAvail.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                                      .ConfigureAwait(false))
                                .Replace(
                                    await LanguageManager.GetStringAsync("String_AvailRestricted", token: token)
                                                         .ConfigureAwait(false), "R")
                                .Replace(
                                    await LanguageManager.GetStringAsync("String_AvailForbidden", token: token)
                                                         .ConfigureAwait(false), "F");
                _decUsedCost = decCost;
            }

            _blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                                  .ConfigureAwait(false);
            if (GlobalSettings.SearchInCategoryOnly
                || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token)
                                  .ConfigureAwait(false) == 0)
            {
                _strSelectCategory = await cboCategory
                                           .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                           .ConfigureAwait(false);
            }
            else if (xmlVehicle != null)
                _strSelectCategory = xmlVehicle.SelectSingleNodeAndCacheExpression("category", token)?.Value ??
                                     string.Empty;
            else
                _strSelectCategory = string.Empty;
            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
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

        #endregion Methods
    }
}
