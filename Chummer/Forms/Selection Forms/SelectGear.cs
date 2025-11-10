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
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Enums;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectGear : Form
    {
        private int _intLoading = 1;
        private string _strSelectedGear = string.Empty;
        private int _intSelectedRating;
        private decimal _decSelectedQty = 1;
        private decimal _decMarkup;
        private bool _blnFreeCost;
        private bool _blnDoItYourself;
        private bool _blnStack;

        private readonly int _intAvailModifier;
        private readonly int _intCostMultiplier;

        private readonly object _objGearParent;
        private readonly XPathNavigator _objParentNode;
        private decimal _decMaximumCapacity = -1;
        private static string _strSelectCategory = string.Empty;
        private bool _blnShowPositiveCapacityOnly;
        private bool _blnShowNegativeCapacityOnly;
        private bool _blnBlackMarketDiscount;
        private CapacityStyle _eCapacityStyle = CapacityStyle.Standard;

        private readonly XPathNavigator _xmlBaseGearDataNode;
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory;
        private HashSet<string> _setAllowedCategories;
        private HashSet<string> _setAllowedNames;
        private HashSet<string> _setBlackMarketMaps;

        private CancellationTokenSource _objUpdateGearInfoCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private CancellationTokenSource _objGearSelectedIndexChangedCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource;
        private readonly CancellationToken _objGenericToken;

        // Shopping cart for multiple purchases
        private readonly List<CartItem> _lstShoppingCart = new List<CartItem>();
        private bool _blnShoppingCartMode = false;

        /// <summary>
        /// Represents an item in the shopping cart
        /// </summary>
        public class CartItem
        {
            public string GearId { get; set; }
            public string GearName { get; set; }
            public int Rating { get; set; }
            public decimal Quantity { get; set; }
            public decimal Markup { get; set; }
            public bool FreeCost { get; set; }
            public bool DoItYourself { get; set; }
            public bool BlackMarketDiscount { get; set; }
            public decimal CapacityUsed { get; set; }

            public override string ToString()
            {
                return $"{GearName} (Rating {Rating}, Qty {Quantity})";
            }
        }

        /// <summary>
        /// Get all items in the shopping cart
        /// </summary>
        public List<CartItem> ShoppingCartItems => new List<CartItem>(_lstShoppingCart);

        #region Control Events

        public SelectGear(Character objCharacter, int intAvailModifier = 0, int intCostMultiplier = 1, object objGearParent = null, string strAllowedCategories = "", string strAllowedNames = "")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            _intAvailModifier = intAvailModifier;
            _intCostMultiplier = intCostMultiplier;
            _objGearParent = objGearParent;
            _objParentNode = (_objGearParent as IHasXmlDataNode)?.GetNodeXPath();
            _lstCategory = Utils.ListItemListPool.Get();
            _setAllowedCategories = Utils.StringHashSetPool.Get();
            _setAllowedNames = Utils.StringHashSetPool.Get();
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            _objGenericCancellationTokenSource = new CancellationTokenSource();
            _objGenericToken = _objGenericCancellationTokenSource.Token;

            // Load the Gear information.
            _xmlBaseGearDataNode = objCharacter.LoadDataXPath("gear.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseGearDataNode));

            // Prevent Enter key from closing the form when NumericUpDown controls have focus
            nudMinimumCost.KeyDown += NumericUpDown_KeyDown;
            nudMaximumCost.KeyDown += NumericUpDown_KeyDown;
            nudExactCost.KeyDown += NumericUpDown_KeyDown;

            foreach (string strCategory in strAllowedCategories.TrimEndOnce(',').SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(strCategory))
                    _setAllowedCategories.Add(strCategory.Trim());
            }

            foreach (string strName in strAllowedNames.TrimEndOnce(',').SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(strName))
                    _setAllowedNames.Add(strName.Trim());
            }

            // Shopping cart is always enabled
            _blnShoppingCartMode = true;
        }

        private async void SelectGear_Load(object sender, EventArgs e)
        {
            try
            {
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
                    await chkStack.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    await nudMarkup.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    int intMaxAvail = await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).GetMaximumAvailabilityAsync(_objGenericToken).ConfigureAwait(false);
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Text = string.Format(GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                        x.Visible = true;
                        x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                    }, _objGenericToken).ConfigureAwait(false);
                    await chkStack.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, _objGenericToken).ConfigureAwait(false);
                }

                XPathNodeIterator objXmlCategoryList;

                // Populate the Gear Category list.
                if (_setAllowedCategories.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMount))
                    {
                        sbdMount.Append("categories/category[");
                        foreach (string strAllowedMount in _setAllowedCategories)
                        {
                            if (!string.IsNullOrEmpty(strAllowedMount))
                                sbdMount.Append(". = ", strAllowedMount.CleanXPath(), " or ");
                        }

                        sbdMount.Append(". = \"General\"]");
                        objXmlCategoryList = _xmlBaseGearDataNode.Select(sbdMount.ToString());
                    }
                }
                else
                {
                    objXmlCategoryList = _xmlBaseGearDataNode.SelectAndCacheExpression("categories/category", _objGenericToken);
                }

                foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
                {
                    string strCategory = objXmlCategory.Value;
                    if (string.IsNullOrEmpty(strCategory))
                        continue;
                    // Make sure the Category isn't in the exclusion list.
                    if (!_setAllowedCategories.Contains(strCategory) && objXmlCategory.SelectSingleNodeAndCacheExpression("@show", _objGenericToken)?.Value == bool.FalseString)
                    {
                        continue;
                    }
                    if ((_lstCategory.Count == 0 || _lstCategory.TrueForAll(x => x.Value?.ToString() != strCategory))
                        && await AnyItemInList(strCategory, _objGenericToken).ConfigureAwait(false))
                    {
                        _lstCategory.Add(new ListItem(strCategory, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate", _objGenericToken)?.Value ?? strCategory));
                    }
                }
                _lstCategory.Sort(CompareListItems.CompareNames);

                if (_lstCategory.Count > 0)
                {
                    _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: _objGenericToken).ConfigureAwait(false)));
                }

                await cboCategory.PopulateWithListItemsAsync(_lstCategory, _objGenericToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(DefaultSearchText))
                {
                    await txtSearch.DoThreadSafeAsync(x =>
                    {
                        x.Text = DefaultSearchText;
                        x.Enabled = false;
                    }, _objGenericToken).ConfigureAwait(false);
                }

                Interlocked.Decrement(ref _intLoading);
                // Select the first Category in the list.
                bool blnRefreshList = false;
                await cboCategory.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(_strSelectCategory))
                        x.SelectedValue = _strSelectCategory;
                    if (x.SelectedIndex == -1 && x.Items.Count > 0)
                        x.SelectedIndex = 0;
                    else
                        blnRefreshList = true;
                }, _objGenericToken).ConfigureAwait(false);
                if (blnRefreshList)
                    await RefreshList(token: _objGenericToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(_strSelectedGear))
                    await lstGear.DoThreadSafeAsync(x => x.SelectedValue = _strSelectedGear, _objGenericToken).ConfigureAwait(false);
                // Make sure right-side controls are properly updated depending on how the selections above worked out
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);

                // Enable shopping cart (always enabled)
                if (_blnShoppingCartMode)
                {
                    // Ensure maximum capacity is set from parent gear if not already set
                    if (_decMaximumCapacity < 0 && _objGearParent is Gear objParentGearForCart)
                    {
                        decimal decRemainingCapacity = await objParentGearForCart.GetCapacityRemainingAsync(_objGenericToken).ConfigureAwait(false);
                        await SetMaximumCapacityAsync(decRemainingCapacity, _objGenericToken).ConfigureAwait(false);
                    }

                    await gpbShoppingCart.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                    await cmdAddToCart.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                    await cmdOK.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    await cmdOKAdd.DoThreadSafeAsync(x => x.Visible = false, _objGenericToken).ConfigureAwait(false);
                    
                    // Only show capacity remaining if there's a parent object
                    bool blnShowCapacity = _objGearParent is Gear;
                    await lblCartCapacityRemainingLabel.DoThreadSafeAsync(x => x.Visible = blnShowCapacity, _objGenericToken).ConfigureAwait(false);
                    await lblCartCapacityRemaining.DoThreadSafeAsync(x => x.Visible = blnShowCapacity, _objGenericToken).ConfigureAwait(false);
                    
                    await UpdateShoppingCartDisplayAsync(_objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SelectGear_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateGearInfoCancellationTokenSource, null);
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
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objGearSelectedIndexChangedCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;

            try
            {
                // Show the Do It Yourself CheckBox if the Commlink Upgrade category is selected.
                if (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false) == "Commlink Upgrade")
                    await chkDoItYourself.DoThreadSafeAsync(x => x.Visible = true, _objGenericToken).ConfigureAwait(false);
                else
                {
                    await chkDoItYourself.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, token: _objGenericToken).ConfigureAwait(false);
                }

                await RefreshList(token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void lstGear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objGearSelectedIndexChangedCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_objGenericToken, objNewToken))
            {
                CancellationToken token = objJoinedCancellationTokenSource.Token;
                try
                {
                    string strSelectedId = await lstGear.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        // Retrieve the information for the selected piece of Gear.
                        XPathNavigator objXmlGear = _xmlBaseGearDataNode.TryGetNodeByNameOrId("gears/gear",strSelectedId);

                        if (objXmlGear != null)
                        {
                            string strName = objXmlGear.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;

                            // Quantity.
                            string strCostFor = objXmlGear.SelectSingleNodeAndCacheExpression("costfor", token)?.Value;
                            await nudGearQty.DoThreadSafeAsync(x =>
                            {
                                x.Enabled = true;
                                x.Minimum = 1;
                                x.Maximum = 1000000000;
                            }, token).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(strCostFor))
                            {
                                decimal.TryParse(strCostFor, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decCostFor);
                                await nudGearQty.DoThreadSafeAsync(x =>
                                {
                                    x.Value = decCostFor;
                                    x.Increment = decCostFor;
                                }, token).ConfigureAwait(false);
                            }
                            else
                            {
                                await nudGearQty.DoThreadSafeAsync(x =>
                                {
                                    x.Value = 1;
                                    x.Increment = 1;
                                }, token).ConfigureAwait(false);
                            }
                            if (strName.StartsWith("Nuyen", StringComparison.Ordinal))
                            {
                                int intDecimalPlaces = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaxNuyenDecimalsAsync(token).ConfigureAwait(false);
                                if (intDecimalPlaces <= 0)
                                {
                                    await nudGearQty.DoThreadSafeAsync(x =>
                                    {
                                        x.DecimalPlaces = 0;
                                        x.Minimum = 1.0m;
                                    }, token).ConfigureAwait(false);
                                }
                                else
                                {
                                    decimal decMinimum = 1.0m;
                                    // Need a for loop instead of a power system to maintain exact precision
                                    for (int i = 0; i < intDecimalPlaces; ++i)
                                        decMinimum /= 10.0m;
                                    await nudGearQty.DoThreadSafeAsync(x =>
                                    {
                                        x.Minimum = decMinimum;
                                        x.DecimalPlaces = intDecimalPlaces;
                                    }, token).ConfigureAwait(false);
                                }
                            }
                            else if (objXmlGear.SelectSingleNodeAndCacheExpression("category", token)?.Value == "Currency")
                            {
                                await nudGearQty.DoThreadSafeAsync(x =>
                                {
                                    x.DecimalPlaces = 2;
                                    x.Minimum = 0.01m;
                                }, token).ConfigureAwait(false);
                            }
                            else
                            {
                                await nudGearQty.DoThreadSafeAsync(x =>
                                {
                                    x.DecimalPlaces = 0;
                                    x.Minimum = 1.0m;
                                }, token).ConfigureAwait(false);
                            }

                            await nudGearQty.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await lblGearQtyLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await chkStack.DoThreadSafeAsync(x => x.Visible = _objCharacter.Created, token: token).ConfigureAwait(false);

                            string strRatingLabel = objXmlGear.SelectSingleNodeAndCacheExpression("ratinglabel", token)?.Value;
                            strRatingLabel = !string.IsNullOrEmpty(strRatingLabel)
                                ? string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Label_RatingFormat", token: token).ConfigureAwait(false),
                                                await LanguageManager.GetStringAsync(strRatingLabel, token: token).ConfigureAwait(false))
                                : await LanguageManager.GetStringAsync("Label_Rating", token: token).ConfigureAwait(false);
                            await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token).ConfigureAwait(false);
                        }
                        else
                        {
                            await nudGearQty.DoThreadSafeAsync(x =>
                            {
                                x.Minimum = 1;
                                x.Maximum = 1;
                                x.Visible = false;
                                x.Enabled = false;
                                x.Value = 1;
                            }, token).ConfigureAwait(false);
                            await lblGearQtyLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                            await chkStack.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await nudGearQty.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = 1;
                            x.Maximum = 1;
                            x.Visible = false;
                            x.Enabled = false;
                            x.Value = 1;
                        }, token).ConfigureAwait(false);
                        await lblGearQtyLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                        await chkStack.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    }

                    await UpdateGearInfo(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private async void nudGearQty_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);
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
                {
                    await RefreshList(token: _objGenericToken).ConfigureAwait(false);
                }
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void chkDoItYourself_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false)
                    && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(token: _objGenericToken).ConfigureAwait(false);
                }
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false)
                    && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(token: _objGenericToken).ConfigureAwait(false);
                }
                await UpdateGearInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void CostFilter(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;

            try
            {
                _intLoading = 1;
                
                await nudMinimumCost.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudMaximumCost.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudExactCost.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);

                decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                
                // If exact cost is specified, clear range values
                if (decExactCost > 0)
                {
                    await nudMinimumCost.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    await nudMaximumCost.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                }
                // If range values are specified, clear exact cost
                else if (decMinimumCost > 0 || decMaximumCost > 0)
                {
                    await nudExactCost.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    
                    // Ensure maximum is not less than minimum
                    if (decMaximumCost < decMinimumCost)
                    {
                        if (sender == nudMaximumCost)
                            await nudMinimumCost.DoThreadSafeAsync(x => x.SetValueSafely(decMaximumCost), _objGenericToken).ConfigureAwait(false);
                        else
                            await nudMaximumCost.DoThreadSafeAsync(x => x.SetValueSafely(decMinimumCost), _objGenericToken).ConfigureAwait(false);
                    }
                }

                _intLoading = 0;

                await RefreshList(token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Form is being closed or operation was cancelled, ignore
                _intLoading = 0;
            }
        }

        private void NumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstGear.SelectedIndex + 1 < lstGear.Items.Count:
                    ++lstGear.SelectedIndex;
                    break;

                case Keys.Down:
                    {
                        if (lstGear.Items.Count > 0)
                        {
                            lstGear.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstGear.SelectedIndex >= 1:
                    --lstGear.SelectedIndex;
                    break;

                case Keys.Up:
                    {
                        if (lstGear.Items.Count > 0)
                        {
                            lstGear.SelectedIndex = lstGear.Items.Count - 1;
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

        private async void cmdAddToCart_Click(object sender, EventArgs e)
        {
            try
            {
                string strSelectedId = await lstGear.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedId))
                    return;

                XPathNavigator objXmlGear = _xmlBaseGearDataNode.TryGetNodeByNameOrId("gears/gear", strSelectedId);
                if (objXmlGear == null)
                    return;

                string strGearName = objXmlGear.SelectSingleNodeAndCacheExpression("name", _objGenericToken)?.Value ?? string.Empty;
                int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, _objGenericToken).ConfigureAwait(false);
                decimal decQty = await nudGearQty.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                bool blnFreeCost = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);
                bool blnDoItYourself = await chkDoItYourself.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);
                bool blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);

                // Calculate capacity used (only if there's a parent object)
                decimal decCapacityUsed = 0;
                if (_objGearParent is Gear objParentGear)
                {
                    string strCapacity = objXmlGear.SelectSingleNodeAndCacheExpression("capacity", _objGenericToken)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strCapacity))
                    {
                        // Parse capacity (format is usually [Rating] or [1] for sensor functions)
                        if (strCapacity.Contains('['))
                        {
                            string strCapacityValue = strCapacity.GetStringBetween('[', ']');
                            if (!string.IsNullOrEmpty(strCapacityValue))
                            {
                                if (int.TryParse(strCapacityValue, out int intCapacity))
                                {
                                    decCapacityUsed = intCapacity;
                                }
                                else if (strCapacityValue == "Rating")
                                {
                                    decCapacityUsed = intRating;
                                }
                            }
                        }
                    }

                    // Check if we have enough capacity (only check if there's a parent)
                    decimal decRemainingCapacity = _decMaximumCapacity;
                    if (decRemainingCapacity < 0)
                    {
                        decRemainingCapacity = await objParentGear.GetCapacityRemainingAsync(_objGenericToken).ConfigureAwait(false);
                    }
                    decimal decUsedCapacity = _lstShoppingCart.Sum(x => x.CapacityUsed);
                    if (decRemainingCapacity - decUsedCapacity < decCapacityUsed)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this,
                            await LanguageManager.GetStringAsync("Message_CapacityReached", token: _objGenericToken).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageTitle_CapacityReached", token: _objGenericToken).ConfigureAwait(false),
                            MessageBoxButtons.OK, MessageBoxIcon.Information, token: _objGenericToken).ConfigureAwait(false);
                        return;
                    }
                }

                CartItem objCartItem = new CartItem
                {
                    GearId = strSelectedId,
                    GearName = strGearName,
                    Rating = intRating,
                    Quantity = decQty,
                    Markup = decMarkup,
                    FreeCost = blnFreeCost,
                    DoItYourself = blnDoItYourself,
                    BlackMarketDiscount = blnBlackMarketDiscount,
                    CapacityUsed = decCapacityUsed
                };

                _lstShoppingCart.Add(objCartItem);
                await UpdateShoppingCartDisplayAsync(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdRemoveFromCart_Click(object sender, EventArgs e)
        {
            try
            {
                int intSelectedIndex = await lstShoppingCart.DoThreadSafeFuncAsync(x => x.SelectedIndex, _objGenericToken).ConfigureAwait(false);
                if (intSelectedIndex >= 0 && intSelectedIndex < _lstShoppingCart.Count)
                {
                    _lstShoppingCart.RemoveAt(intSelectedIndex);
                    await UpdateShoppingCartDisplayAsync(_objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdPurchaseAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lstShoppingCart.Count == 0)
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        await LanguageManager.GetStringAsync("Message_ShoppingCartEmpty", token: _objGenericToken).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_ShoppingCartEmpty", token: _objGenericToken).ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: _objGenericToken).ConfigureAwait(false);
                    return;
                }

                // Set the first item as selected and mark that we're using shopping cart
                if (_lstShoppingCart.Count > 0)
                {
                    CartItem objFirstItem = _lstShoppingCart[0];
                    _strSelectedGear = objFirstItem.GearId;
                    _intSelectedRating = objFirstItem.Rating;
                    _decSelectedQty = objFirstItem.Quantity;
                    _decMarkup = objFirstItem.Markup;
                    _blnFreeCost = objFirstItem.FreeCost;
                    _blnDoItYourself = objFirstItem.DoItYourself;
                    _blnStack = false;
                    _blnBlackMarketDiscount = objFirstItem.BlackMarketDiscount;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task UpdateShoppingCartDisplayAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await lstShoppingCart.DoThreadSafeAsync(x =>
            {
                x.BeginUpdate();
                try
                {
                    x.Items.Clear();
                    foreach (CartItem objItem in _lstShoppingCart)
                    {
                        x.Items.Add(objItem);
                    }
                }
                finally
                {
                    x.EndUpdate();
                }
            }, token).ConfigureAwait(false);

            // Update capacity remaining (only if there's a parent object)
            if (_objGearParent is Gear)
            {
                // If maximum capacity is not set, try to get it from parent gear
                decimal decRemainingCapacity = _decMaximumCapacity;
                if (decRemainingCapacity < 0 && _objGearParent is Gear objParentGearForDisplay)
                {
                    decRemainingCapacity = await objParentGearForDisplay.GetCapacityRemainingAsync(token).ConfigureAwait(false);
                }
                
                decimal decUsedCapacity = _lstShoppingCart.Sum(x => x.CapacityUsed);
                decimal decRemaining = decRemainingCapacity >= 0 ? decRemainingCapacity - decUsedCapacity : 0;

                await lblCartCapacityRemaining.DoThreadSafeAsync(x =>
                {
                    x.Text = decRemainingCapacity >= 0
                        ? decRemaining.ToString("#,0.##", GlobalSettings.CultureInfo)
                        : "0";
                }, token).ConfigureAwait(false);
            }
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Only items that grant Capacity should be shown.
        /// </summary>
        public bool ShowPositiveCapacityOnly
        {
            get => _blnShowPositiveCapacityOnly;
            set
            {
                _blnShowPositiveCapacityOnly = value;
                if (value)
                    _blnShowNegativeCapacityOnly = false;
            }
        }

        /// <summary>
        /// Only items that consume Capacity should be shown.
        /// </summary>
        public bool ShowNegativeCapacityOnly
        {
            get => _blnShowNegativeCapacityOnly;
            set
            {
                _blnShowNegativeCapacityOnly = value;
                if (value)
                    _blnShowPositiveCapacityOnly = false;
            }
        }

        /// <summary>
        /// Only items that consume Armor Capacity should be shown.
        /// </summary>
        public bool ShowArmorCapacityOnly { get; set; }

        /// <summary>
        /// Only items that are marked as being flechette ammo should be shown.
        /// </summary>
        public bool ShowFlechetteAmmoOnly { get; set; }

        /// <summary>
        /// Guid of Gear that was selected in the dialogue.
        /// </summary>
        public string SelectedGear
        {
            get => _strSelectedGear;
            set => _strSelectedGear = value;
        }

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => _intSelectedRating;

        /// <summary>
        /// Quantity that was selected in the dialogue.
        /// </summary>
        public decimal SelectedQty => _decSelectedQty;

        /// <summary>
        /// Set the maximum Capacity the piece of Gear is allowed to be.
        /// </summary>
        public decimal MaximumCapacity
        {
            get => _decMaximumCapacity;
            set
            {
                _decMaximumCapacity = value;
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed") + LanguageManager.GetString("String_Space") + value.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// Set the maximum Capacity the piece of Gear is allowed to be.
        /// </summary>
        public async Task SetMaximumCapacityAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _decMaximumCapacity = value;
            string strText = await LanguageManager.GetStringAsync("Label_MaximumCapacityAllowed", token: token).ConfigureAwait(false)
                                + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false)
                                + value.ToString("#,0.##", GlobalSettings.CultureInfo);
            await lblMaximumCapacity.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Whether the item's cost should be cut in half for being a Do It Yourself component/upgrade.
        /// </summary>
        public bool DoItYourself => _blnDoItYourself;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Whether the Gear should stack with others if possible.
        /// </summary>
        public bool Stack => _blnStack;

        /// <summary>
        /// Whether the Stack Checkbox should be shown (default true).
        /// </summary>
        public bool EnableStack
        {
            set
            {
                chkStack.Visible = value;
                if (!value)
                    chkStack.Checked = false;
            }
        }

        /// <summary>
        /// Capacity display style.
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            set => _eCapacityStyle = value;
        }

        /// <summary>
        /// Whether the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// Default text string to filter by.
        /// </summary>
        public string DefaultSearchText { get; set; }

        /// <summary>
        /// What weapon type is our gear allowed to have
        /// </summary>
        public string ForceItemAmmoForWeaponType { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the Gear's information based on the Gear selected and current Rating.
        /// </summary>
        private async Task UpdateGearInfo(CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateGearInfoCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                if (_intLoading > 0)
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    return;
                }

                string strSelectedId = await lstGear.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedId))
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    return;
                }

                // Retrieve the information for the selected piece of Gear.
                XPathNavigator objXmlGear = _xmlBaseGearDataNode.TryGetNodeByNameOrId("gears/gear", strSelectedId);

                if (objXmlGear == null)
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    return;
                }

                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                try
                {
                    decimal decQuantityMultiplier = await nudGearQty.DoThreadSafeFuncAsync(x => x.Value / x.Increment, token: token).ConfigureAwait(false);
                    decimal decCostMultiplier = _intCostMultiplier;
                    if (await chkDoItYourself.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        decCostMultiplier *= 0.5m;
                    decCostMultiplier *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                    if (_setBlackMarketMaps.Contains(objXmlGear.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                        decCostMultiplier *= 0.9m;

                    // Rating first (since we need it).
                    string strExpression = objXmlGear.SelectSingleNodeAndCacheExpression("rating", token)?.Value ?? string.Empty;
                    if (strExpression == "0")
                        strExpression = string.Empty;
                    int intRating = int.MaxValue;
                    if (!string.IsNullOrEmpty(strExpression))
                    {
                        intRating = (await ProcessInvariantXPathExpression(strExpression, 0, token).ConfigureAwait(false)).Item1.StandardRound();
                    }

                    if (intRating > 0 && intRating != int.MaxValue)
                    {
                        await nudRating.DoThreadSafeAsync(x => x.Maximum = intRating, token: token).ConfigureAwait(false);
                        XPathNavigator xmlMinRatingNode = objXmlGear.SelectSingleNodeAndCacheExpression("minrating", token);
                        if (xmlMinRatingNode != null)
                        {
                            decimal decOldMinimum = await nudRating.DoThreadSafeFuncAsync(x => x.Minimum, token: token).ConfigureAwait(false);
                            strExpression = xmlMinRatingNode.Value;
                            int intMinimumRating = 0;
                            if (!string.IsNullOrEmpty(strExpression))
                            {
                                intMinimumRating = (await ProcessInvariantXPathExpression(strExpression, intRating, token).ConfigureAwait(false)).Item1.StandardRound();
                            }

                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Minimum = intMinimumRating;
                                if (decOldMinimum > x.Minimum)
                                {
                                    x.Value -= decOldMinimum - x.Minimum;
                                }
                            }, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await nudRating.DoThreadSafeAsync(x => x.Minimum = 1, token: token).ConfigureAwait(false);
                        }

                        if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            int intMinimum = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token).ConfigureAwait(false);
                            int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token).ConfigureAwait(false);
                            while (intMaximum > intMinimum && !await objXmlGear.CheckAvailRestrictionAsync(_objCharacter, intMaximum, _intAvailModifier + (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlGear.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
                            {
                                --intMaximum;
                            }
                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                        }

                        if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                        {
                            int intMinimum = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token).ConfigureAwait(false);
                            int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token).ConfigureAwait(false);
                            decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                            decimal decQtyMultiplierMinimum = await nudGearQty.DoThreadSafeFuncAsync(x => x.Minimum / x.Increment, token).ConfigureAwait(false);
                            while (decQuantityMultiplier > decQtyMultiplierMinimum && !await objXmlGear.CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier * decQuantityMultiplier, intMaximum, token).ConfigureAwait(false))
                            {
                                decQuantityMultiplier = Math.Max(decQuantityMultiplier - 1, decQtyMultiplierMinimum);
                            }
                            await nudGearQty.DoThreadSafeAsync(x => x.Maximum = decQuantityMultiplier * x.Increment, token).ConfigureAwait(false);
                            while (intMaximum > intMinimum && !await objXmlGear.CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier * decQuantityMultiplier, intMaximum, token).ConfigureAwait(false))
                            {
                                --intMaximum;
                            }
                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                        }

                        await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Enabled = x.Minimum != x.Maximum;
                            x.Visible = true;
                        }, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = 0;
                            x.Maximum = 0;
                            x.Enabled = false;
                            x.Visible = false;
                        }, token: token).ConfigureAwait(false);
                    }
                    intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);

                    // Retrieve the information for the selected piece of Cyberware.
                    string strDeviceRating = objXmlGear.SelectSingleNodeAndCacheExpression("devicerating", token)?.Value ?? string.Empty;
                    strDeviceRating = (await ProcessInvariantXPathExpression(strDeviceRating, intRating, token).ConfigureAwait(false)).Item1.ToString("#,0.##", GlobalSettings.CultureInfo);
                    await lblGearDeviceRating.DoThreadSafeFuncAsync(x => x.Text = strDeviceRating, token: token).ConfigureAwait(false);
                    await lblGearDeviceRatingLabel.DoThreadSafeFuncAsync(x => x.Visible = !string.IsNullOrEmpty(strDeviceRating), token: token).ConfigureAwait(false);

                    string strSource = objXmlGear.SelectSingleNodeAndCacheExpression("source", token)?.Value
                                       ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    string strPage = objXmlGear.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value
                                     ?? objXmlGear.SelectSingleNodeAndCacheExpression("page", token)?.Value
                                     ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    SourceString objSource = await SourceString.GetSourceStringAsync(
                        strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter, token: token).ConfigureAwait(false);
                    await objSource.SetControlAsync(lblSource, this, token: token).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()), token: token).ConfigureAwait(false);
                    string strAvail = await new AvailabilityValue(intRating,
                        objXmlGear.SelectSingleNodeAndCacheExpression("avail", token)?.Value, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlGear.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound()).ToStringAsync(token).ConfigureAwait(false);
                    await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
                    await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token).ConfigureAwait(false);

                    // Cost.
                    bool blnCanBlackMarketDiscount
                        = _setBlackMarketMaps.Contains(objXmlGear.SelectSingleNodeAndCacheExpression("category", token)?.Value);
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

                    decimal decItemCost = 0.0m;
                    if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    {
                        string strCost = 0.0m.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        XPathNavigator objCostNode = objXmlGear.SelectSingleNodeAndCacheExpression("cost", token);
                        if (objCostNode == null)
                        {
                            int intHighestCostNode = 0;
                            foreach (XmlNode objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                            {
                                if (!objLoopNode.Name.StartsWith("cost", StringComparison.Ordinal))
                                    continue;
                                string strLoopCostString = objLoopNode.Name.Substring(4);
                                if (int.TryParse(strLoopCostString, out int intTmp))
                                {
                                    intHighestCostNode = Math.Max(intHighestCostNode, intTmp);
                                }
                            }

                            objCostNode = objXmlGear.SelectSingleNode("cost" + intHighestCostNode.ToString(GlobalSettings.InvariantCultureInfo));
                            for (int i = intRating; i <= intHighestCostNode; ++i)
                            {
                                XPathNavigator objLoopNode
                                    = objXmlGear.SelectSingleNode("cost" + i.ToString(GlobalSettings.InvariantCultureInfo));
                                if (objLoopNode != null)
                                {
                                    objCostNode = objLoopNode;
                                    break;
                                }
                            }
                        }

                        if (objCostNode != null)
                        {
                            string strCost = objCostNode.Value.ProcessFixedValuesString(intRating);
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
                                    if (decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin))
                                        decMin *= decCostMultiplier;
                                    if (decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax))
                                        decMax *= decCostMultiplier;
                                }
                                else if (decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin))
                                    decMin *= decCostMultiplier;

                                string strFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                                if (decMax == decimal.MaxValue)
                                {
                                    strCost = (decMin * decQuantityMultiplier).ToString(strFormat,
                                                              GlobalSettings.CultureInfo)
                                              + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                     .ConfigureAwait(false) + "+";
                                }
                                else
                                {
                                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                                    strCost = (decMin * decQuantityMultiplier).ToString(strFormat, GlobalSettings.CultureInfo)
                                                                          + strSpace + "-" + strSpace
                                                                          + (decMax * decQuantityMultiplier).ToString(strFormat, GlobalSettings.CultureInfo)
                                                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                }
                                await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);

                                decItemCost = decMin;
                            }
                            else
                            {
                                (decimal decCost, bool blnIsSuccess) = await ProcessInvariantXPathExpression(strCost, intRating, token).ConfigureAwait(false);
                                if (blnIsSuccess || decimal.TryParse(strCost, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decCost))
                                {
                                    decItemCost = decCost * decCostMultiplier;
                                    strCost = (decItemCost * decQuantityMultiplier).ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                        + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                }
                                await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                            }
                        }
                    }

                    bool blnShowCost = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);

                    // Update the Avail Test Label.
                    string strTest = await _objCharacter.AvailTestAsync(decItemCost, strAvail, token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token).ConfigureAwait(false);

                    // Capacity.

                    if (_eCapacityStyle == CapacityStyle.Zero)
                        await lblCapacity.DoThreadSafeAsync(x => x.Text = "[" + 0.ToString(GlobalSettings.CultureInfo) + "]", token: token).ConfigureAwait(false);
                    else
                    {
                        // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                        string strCapacityField = ShowArmorCapacityOnly ? "armorcapacity" : "capacity";
                        string strCapacityText = objXmlGear.SelectSingleNode(strCapacityField)?.Value;
                        if (!string.IsNullOrEmpty(strCapacityText))
                        {
                            int intPos = strCapacityText.IndexOf("/[", StringComparison.Ordinal);
                            string strCapacity;
                            if (intPos != -1)
                            {
                                string strFirstHalf = strCapacityText.Substring(0, intPos);
                                string strSecondHalf = strCapacityText.Substring(intPos + 1);

                                if (strFirstHalf == "[*]")
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = "*", token: token).ConfigureAwait(false);
                                else
                                {
                                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');
                                    strCapacity = strFirstHalf;
                                    if (blnSquareBrackets && strCapacity.Length > 2)
                                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                    (decimal decCapacity, bool blnIsSuccess) = await ProcessInvariantXPathExpression(strCapacity, intRating, token).ConfigureAwait(false);
                                    if (blnIsSuccess)
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                                    else
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);

                                    if (blnSquareBrackets)
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = "[" + x.Text + "]", token: token).ConfigureAwait(false);
                                }

                                await lblCapacity.DoThreadSafeAsync(x => x.Text += "/" + strSecondHalf, token: token).ConfigureAwait(false);
                            }
                            else if (strCapacityText == "[*]")
                                await lblCapacity.DoThreadSafeAsync(x => x.Text = "*", token: token).ConfigureAwait(false);
                            else
                            {
                                bool blnSquareBrackets = strCapacityText.StartsWith('[');
                                strCapacity = strCapacityText;
                                if (blnSquareBrackets && strCapacity.Length > 2)
                                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                (decimal decCapacity, bool blnIsSuccess) = await ProcessInvariantXPathExpression(strCapacity, intRating, token).ConfigureAwait(false);
                                if (blnIsSuccess)
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                                else
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);

                                if (blnSquareBrackets)
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = "[" + x.Text + "]", token: token).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await lblCapacity.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                        }
                    }

                    bool blnShowCapacity = !string.IsNullOrEmpty(await lblCapacity.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                    await lblCapacityLabel.DoThreadSafeAsync(x => x.Visible = blnShowCapacity, token: token).ConfigureAwait(false);

                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                }
            }
        }

        private Task<bool> AnyItemInList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, false, token);
        }

        private async Task<bool> RefreshList(string strCategory = "", CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
                return await RefreshList(strCategory, true, objJoinedCancellationTokenSource.Token).ConfigureAwait(false);
        }

        private async Task<bool> RefreshList(string strCategory, bool blnDoUIUpdate, CancellationToken token = default)
        {
            bool blnAnyItem = false;
            if (string.IsNullOrEmpty(strCategory))
                strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            string strFilter = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false));

                // Only add in category filter if we either are not searching or we have the option set to only search in categories
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0))
                    sbdFilter.Append(" and category = ", strCategory.CleanXPath());
                else if (_setAllowedCategories.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                        {
                            sbdCategoryFilter.Append("category = ", strItem.CleanXPath(), " or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (", sbdCategoryFilter.ToString(), ')');
                        }
                    }
                }

                if (_setAllowedNames.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdNameFilter))
                    {
                        foreach (string strItem in _setAllowedNames)
                        {
                            sbdNameFilter.Append("name = ", strItem.CleanXPath(), " or ");
                        }

                        if (sbdNameFilter.Length > 0)
                        {
                            sbdNameFilter.Length -= 4;
                            sbdFilter.Append(" and (", sbdNameFilter.ToString(), ')');
                        }
                    }
                }

                if (ShowArmorCapacityOnly)
                    sbdFilter.Append(" and (contains(armorcapacity, \"[\") or category = \"Custom\")");
                else if (ShowPositiveCapacityOnly)
                    sbdFilter.Append(" and (not(contains(capacity, \"[\")) or category = \"Custom\")");
                else if (ShowNegativeCapacityOnly)
                    sbdFilter.Append(" and (contains(capacity, \"[\") or category = \"Custom\")");
                if (ShowFlechetteAmmoOnly)
                    sbdFilter.Append(" and isflechetteammo = 'True'");
                if (_objGearParent == null)
                    sbdFilter.Append(" and not(requireparent)");
                if (!string.IsNullOrEmpty(ForceItemAmmoForWeaponType))
                    sbdFilter.Append(" and ammoforweapontype = ", ForceItemAmmoForWeaponType.CleanXPath());

                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ", CommonFunctions.GenerateSearchXPath(strSearch));

                // Apply cost filtering
                decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                
                if (decExactCost > 0)
                {
                    // Exact cost filtering
                    sbdFilter.Append(" and (cost = ", decExactCost.ToString(GlobalSettings.InvariantCultureInfo), ')');
                }
                else if (decMinimumCost != 0 || decMaximumCost != 0)
                {
                    // Range cost filtering
                    sbdFilter.Append(" and ", CommonFunctions.GenerateNumericRangeXPath(decMaximumCost, decMinimumCost, "cost"));
                }

                if (sbdFilter.Length > 0)
                    strFilter = sbdFilter.Insert(0, '[').Append(']').ToString();
            }

            int intOverLimit = 0;
            List<ListItem> lstGears = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                decimal decBaseCostMultiplier = _intCostMultiplier * await nudGearQty.DoThreadSafeFuncAsync(x => x.Minimum / x.Increment, token: token).ConfigureAwait(false);
                if (await chkDoItYourself.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    decBaseCostMultiplier *= 0.5m;
                decBaseCostMultiplier *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                decimal decNuyen = blnFreeItem || !blnShowOnlyAffordItems ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                foreach (XPathNavigator objXmlGear in _xmlBaseGearDataNode.Select("gears/gear" + strFilter))
                {
                    XPathNavigator xmlTestNode
                        = objXmlGear.SelectSingleNodeAndCacheExpression("forbidden/parentdetails", token: token);
                    if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = objXmlGear.SelectSingleNodeAndCacheExpression("required/parentdetails", token: token);
                    if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = objXmlGear.SelectSingleNodeAndCacheExpression("forbidden/geardetails", token: token);
                    if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = objXmlGear.SelectSingleNodeAndCacheExpression("required/geardetails", token: token);
                    if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    if (!await objXmlGear.RequirementsMetAsync(_objCharacter, _objGearParent, token: token).ConfigureAwait(false))
                        continue;

                    if (!blnDoUIUpdate)
                    {
                        blnAnyItem = true;
                        break;
                    }

                    decimal decCostMultiplier = decBaseCostMultiplier;
                    if (_setBlackMarketMaps.Contains(objXmlGear.SelectSingleNodeAndCacheExpression("category", token: token)?.Value))
                        decCostMultiplier *= 0.9m;
                    if (!blnHideOverAvailLimit
                              || await objXmlGear.CheckAvailRestrictionAsync(_objCharacter, 1, _intAvailModifier + (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlGear.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token).ConfigureAwait(false)
                              && (blnFreeItem || !blnShowOnlyAffordItems
                                              || await objXmlGear.CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier, token: token).ConfigureAwait(false)))
                    {
                        blnAnyItem = true;
                        string strDisplayName = objXmlGear.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                ?? objXmlGear.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        lstGears.Add(new ListItem(
                                         objXmlGear.SelectSingleNodeAndCacheExpression("id", token: token)?.Value ?? string.Empty,
                                         strDisplayName));
                    }
                    else
                        ++intOverLimit;
                }

                if (blnDoUIUpdate)
                {
                    // Find all entries that have duplicate names so that we can add category labels next to them
                    // But only if it's even possible for the list to have multiple items from different categories
                    if (lstGears.Count > 1)
                    {
                        string strSelectCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                        if (!GlobalSettings.SearchInCategoryOnly || string.IsNullOrEmpty(strSelectCategory) ||
                            strSelectCategory == "Show All")
                        {
                            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string> setDuplicateNames))
                            {
                                for (int i = 0; i + 1 < lstGears.Count; ++i)
                                {
                                    string strLoopName = lstGears[i].Name;
                                    if (setDuplicateNames.Contains(strLoopName))
                                        continue;
                                    for (int j = i + 1; j < lstGears.Count; ++j)
                                    {
                                        if (strLoopName.Equals(lstGears[j].Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            setDuplicateNames.Add(strLoopName);
                                            break;
                                        }
                                    }
                                }

                                if (setDuplicateNames.Count > 0)
                                {
                                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                                    for (int i = 0; i < lstGears.Count; ++i)
                                    {
                                        ListItem objLoopItem = lstGears[i];
                                        if (!setDuplicateNames.Contains(objLoopItem.Name))
                                            continue;
                                        XPathNavigator objXmlGear = _xmlBaseGearDataNode.TryGetNodeByNameOrId("gears/gear", objLoopItem.Value.ToString());
                                        if (objXmlGear == null)
                                            continue;
                                        string strLoopCategory
                                            = objXmlGear.SelectSingleNodeAndCacheExpression("category", token: token)?.Value;
                                        if (string.IsNullOrEmpty(strLoopCategory))
                                            continue;
                                        ListItem objFoundItem
                                            = _lstCategory.Find(objFind => objFind.Value.ToString() == strLoopCategory);
                                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                                        {
                                            lstGears[i] = new ListItem(objLoopItem.Value,
                                                                       objLoopItem.Name + strSpace + "[" + objFoundItem.Name + "]");
                                        }
                                    }
                                }
                            }
                        }

                        lstGears.Sort(CompareListItems.CompareNames);
                    }

                    if (intOverLimit > 0)
                    {
                        // Add after sort so that it's always at the end
                        lstGears.Add(new ListItem(string.Empty,
                                                  string.Format(GlobalSettings.CultureInfo,
                                                                await LanguageManager.GetStringAsync(
                                                                    "String_RestrictedItemsHidden", token: token).ConfigureAwait(false),
                                                                intOverLimit)));
                    }

                    string strOldSelected = await lstGear.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await lstGear.PopulateWithListItemsAsync(lstGears, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }

                    await lstGear.DoThreadSafeAsync(x =>
                    {
                        if (string.IsNullOrEmpty(strOldSelected))
                            x.SelectedIndex = -1;
                        else
                            x.SelectedValue = strOldSelected;
                    }, token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (lstGears != null)
                    Utils.ListItemListPool.Return(ref lstGears);
            }

            return blnAnyItem;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstGear.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedGear = strSelectedId;
                _strSelectCategory = GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0
                    ? cboCategory.SelectedValue?.ToString()
                    : _xmlBaseGearDataNode.TryGetNodeByNameOrId("gears/gear", strSelectedId)?.SelectSingleNodeAndCacheExpression("category", _objGenericToken)?.Value ?? string.Empty;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                _intSelectedRating = nudRating.ValueAsInt;
                _decSelectedQty = nudGearQty.Value;
                _decMarkup = nudMarkup.Value;
                _blnFreeCost = chkFreeItem.Checked;
                _blnDoItYourself = chkDoItYourself.Checked;
                _blnStack = chkStack.Checked;

                DialogResult = DialogResult.OK;
                Close();
            }
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

        private async Task<ValueTuple<decimal, bool>> ProcessInvariantXPathExpression(string strExpression, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = true;
            strExpression = strExpression.ProcessFixedValuesString(intRating);
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnSuccess = false;
                // Cannot also process without curly brackets because Device Rating and Parent Rating both exist
                strExpression = strExpression.Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        if (_objGearParent is IHasRating objCastParent)
                        {
                            await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                                async () => (await objCastParent.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            sbdValue.Replace("{Parent Rating}", 0.ToString(GlobalSettings.InvariantCultureInfo));
                        }
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            await sbdValue.CheapReplaceAsync(strExpression, "{Gear " + strMatrixAttribute + "}",
                                () => (_objGearParent as IHasMatrixAttributes)?.GetBaseMatrixAttribute(
                                        strMatrixAttribute).ToString(GlobalSettings.InvariantCultureInfo) ?? "0"
                                    , token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Parent " + strMatrixAttribute + "}",
                                () => (_objGearParent as IHasMatrixAttributes)?.GetMatrixAttributeString(
                                    strMatrixAttribute) ?? "0", token: token).ConfigureAwait(false);
                        }
                        await sbdValue.CheapReplaceAsync(strExpression, "Rating", () => intRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        object objLoopParent = _objGearParent;
                        while (objLoopParent is Gear objLoopParentGear)
                            objLoopParent = objLoopParentGear.Parent;
                        if (objLoopParent is Cyberware objCyberwareParent)
                            await objCyberwareParent.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        else if (objLoopParent is WeaponAccessory objAccessoryParent)
                        {
                            Weapon objWeaponParent = objAccessoryParent.Parent;
                            if (objWeaponParent != null)
                            {
                                if (objWeaponParent.Cyberware)
                                {
                                    string strCyberwareId = objAccessoryParent.Parent.ParentID;
                                    objCyberwareParent = await _objCharacter.Cyberware.FindByIdAsync(strCyberwareId, token).ConfigureAwait(false)
                                        ?? (await _objCharacter.Vehicles.FindVehicleCyberwareAsync(x => strCyberwareId == x.InternalId, token).ConfigureAwait(false)).Item1;
                                    await objCyberwareParent.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                                }
                                else if (objWeaponParent.ParentVehicle != null)
                                    await objWeaponParent.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                                else
                                {
                                    Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                                    await _objCharacter.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                                await _objCharacter.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                            }
                        }
                        else if (objLoopParent is Vehicle objVehicleParent)
                            await objVehicleParent.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        else
                        {
                            Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                            await _objCharacter.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        }
                        strExpression = sbdValue.ToString();
                    }
                }
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(strExpression, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    return new ValueTuple<decimal, bool>(Convert.ToDecimal((double)objProcess), true);
            }

            return new ValueTuple<decimal, bool>(decValue, blnSuccess);
        }

        #endregion Methods
    }
}
