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

        private List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private HashSet<string> _setAllowedCategories = Utils.StringHashSetPool.Get();
        private HashSet<string> _setAllowedNames = Utils.StringHashSetPool.Get();
        private HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        private CancellationTokenSource _objUpdateGearInfoCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private CancellationTokenSource _objGearSelectedIndexChangedCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        #region Control Events

        public SelectGear(Character objCharacter, int intAvailModifier = 0, int intCostMultiplier = 1, object objGearParent = null, string strAllowedCategories = "", string strAllowedNames = "")
        {
            Disposed += (sender, args) =>
            {
                Utils.ListItemListPool.Return(ref _lstCategory);
                Utils.StringHashSetPool.Return(ref _setAllowedCategories);
                Utils.StringHashSetPool.Return(ref _setAllowedNames);
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
            };
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
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
                _objGenericCancellationTokenSource.Dispose();
            };
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _intAvailModifier = intAvailModifier;
            _intCostMultiplier = intCostMultiplier;
            _objGearParent = objGearParent;
            _objParentNode = (_objGearParent as IHasXmlDataNode)?.GetNodeXPath();
            // Stack Checkbox is only available in Career Mode.
            if (!_objCharacter.Created)
            {
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
                chkStack.Checked = false;
                chkStack.Visible = false;
            }

            // Load the Gear information.
            _xmlBaseGearDataNode = objCharacter.LoadDataXPath("gear.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseGearDataNode));
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
        }

        private async void SelectGear_Load(object sender, EventArgs e)
        {
            try
            {
                if (_objCharacter.Created)
                {
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Text = string.Format(
                            GlobalSettings.CultureInfo, x.Text,
                            _objCharacter.Settings.MaximumAvailability);
                        x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                    }, _objGenericToken).ConfigureAwait(false);
                }

                XPathNodeIterator objXmlCategoryList;

                // Populate the Gear Category list.
                if (_setAllowedCategories.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMount))
                    {
                        foreach (string strAllowedMount in _setAllowedCategories)
                        {
                            if (!string.IsNullOrEmpty(strAllowedMount))
                                sbdMount.Append(". = ").Append(strAllowedMount.CleanXPath()).Append(" or ");
                        }

                        sbdMount.Append(". = \"General\"");
                        objXmlCategoryList = _xmlBaseGearDataNode.Select("categories/category[" + sbdMount + ']');
                    }
                }
                else
                {
                    objXmlCategoryList = _xmlBaseGearDataNode.SelectAndCacheExpression("categories/category", _objGenericToken);
                }

                foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
                {
                    string strCategory = objXmlCategory.Value;
                    // Make sure the Category isn't in the exclusion list.
                    if (!_setAllowedCategories.Contains(strCategory) && objXmlCategory.SelectSingleNodeAndCacheExpression("@show", _objGenericToken)?.Value == bool.FalseString)
                    {
                        continue;
                    }
                    if (_lstCategory.TrueForAll(x => x.Value.ToString() != strCategory) && await AnyItemInList(strCategory, _objGenericToken).ConfigureAwait(false))
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

                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount, _objGenericToken).ConfigureAwait(false);

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
                            }, token).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(strCostFor))
                            {
                                decimal decCostFor = Convert.ToDecimal(strCostFor, GlobalSettings.InvariantCultureInfo);
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
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed") + LanguageManager.GetString("String_Space") + _decMaximumCapacity.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Whether the item's cost should be cut in half for being a Do It Yourself component/upgrade.
        /// </summary>
        public bool DoItYourself => chkDoItYourself.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Whether the Gear should stack with others if possible.
        /// </summary>
        public bool Stack => chkStack.Checked;

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
                    int intRatingValue = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
                    // Retrieve the information for the selected piece of Cyberware.
                    string strDeviceRating = objXmlGear.SelectSingleNodeAndCacheExpression("devicerating", token)?.Value ?? string.Empty;
                    if (strDeviceRating.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        strDeviceRating = await strDeviceRating.CheapReplaceAsync("{Rating}", () => intRatingValue.ToString(), token: token).ConfigureAwait(false);
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                            strDeviceRating, token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            strDeviceRating = ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo);
                    }
                    else
                        strDeviceRating = decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
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
                    await objSource.SetControlAsync(lblSource, token: token).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()), token: token).ConfigureAwait(false);
                    string strAvail = await new AvailabilityValue(intRatingValue,
                        objXmlGear.SelectSingleNodeAndCacheExpression("avail", token)?.Value).ToStringAsync(token).ConfigureAwait(false);
                    await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
                    await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token).ConfigureAwait(false);

                    decimal decMultiplier = await nudGearQty.DoThreadSafeFuncAsync(x => x.Value / x.Increment, token: token).ConfigureAwait(false);
                    if (await chkDoItYourself.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        decMultiplier *= 0.5m;

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
                        string strCost = 0.0m.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
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

                            objCostNode = objXmlGear.SelectSingleNode("cost" + intHighestCostNode);
                            for (int i = intRatingValue; i <= intHighestCostNode; ++i)
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
                            string strCost = objCostNode.Value;

                            if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal) && intRatingValue > 0)
                            {
                                strCost = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                 .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .ElementAt(intRatingValue - 1).Trim('[', ']');
                            }

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
                                    strCost = decMin.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false),
                                                              GlobalSettings.CultureInfo)
                                              + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                     .ConfigureAwait(false) + '+';
                                }
                                else
                                {
                                    string strFormat = await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false);
                                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                                    strCost = decMin.ToString(strFormat, GlobalSettings.CultureInfo)
                                                                          + strSpace + '-' + strSpace
                                                                          + decMax.ToString(strFormat, GlobalSettings.CultureInfo)
                                                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                }
                                await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);

                                decItemCost = decMin;
                            }
                            else
                            {
                                try
                                {
                                    if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decCost))
                                    {
                                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                            strCost.Replace(
                                                "Rating", intRatingValue.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                                        decCost = blnIsSuccess
                                            ? Convert.ToDecimal((double)objProcess) * decMultiplier
                                            : 0;
                                    }
                                    decCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                                        decCost *= 0.9m;
                                    strCost = (decCost * _intCostMultiplier).ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                        + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                    decItemCost = decCost;
                                }
                                catch (XPathException)
                                {
                                    if (decimal.TryParse(strCost, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decTemp))
                                    {
                                        decItemCost = decTemp;
                                        strCost = (decItemCost * _intCostMultiplier).ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                            + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                    }
                                }
                                await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);
                            }
                        }
                    }

                    bool blnShowCost = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);

                    // Update the Avail Test Label.
                    string strTest = await _objCharacter.AvailTestAsync(decItemCost * _intCostMultiplier, strAvail, token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token).ConfigureAwait(false);

                    // Capacity.

                    if (_eCapacityStyle == CapacityStyle.Zero)
                        await lblCapacity.DoThreadSafeAsync(x => x.Text = '[' + 0.ToString(GlobalSettings.CultureInfo) + ']', token: token).ConfigureAwait(false);
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

                                    if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                                    {
                                        string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true)
                                                                        .TrimEndOnce(')')
                                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        if (strValues.Length >= intRatingValue)
                                            await lblCapacity.DoThreadSafeAsync(x => x.Text = strValues[intRatingValue - 1], token: token).ConfigureAwait(false);
                                        else if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decCapacity))
                                        {
                                            try
                                            {
                                                (bool blnIsSuccess2, object objProcess2) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                                    strCapacity.Replace(
                                                        "Rating",
                                                        intRatingValue.ToString(GlobalSettings.InvariantCultureInfo)), token: token).ConfigureAwait(false);
                                                await lblCapacity.DoThreadSafeAsync(x => x.Text = blnIsSuccess2
                                                    ? ((double)objProcess2).ToString("#,0.##", GlobalSettings.CultureInfo)
                                                    : strCapacity, token: token).ConfigureAwait(false);
                                            }
                                            catch (XPathException)
                                            {
                                                await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                            }
                                            catch (OverflowException) // Result is text and not a double
                                            {
                                                await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                            }
                                            catch (InvalidCastException) // Result is text and not a double
                                            {
                                                await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                            }
                                        }
                                        else
                                            await lblCapacity.DoThreadSafeAsync(x => x.Text = decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                                    }
                                    else if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decCapacity))
                                    {
                                        try
                                        {
                                            (bool blnIsSuccess2, object objProcess2) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                                strCapacity.Replace(
                                                    "Rating",
                                                    intRatingValue.ToString(GlobalSettings.InvariantCultureInfo)), token: token).ConfigureAwait(false);
                                            await lblCapacity.DoThreadSafeAsync(x => x.Text = blnIsSuccess2
                                                                                    ? ((double)objProcess2).ToString("#,0.##", GlobalSettings.CultureInfo)
                                                                                    : strCapacity, token: token).ConfigureAwait(false);
                                        }
                                        catch (XPathException)
                                        {
                                            await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                        }
                                        catch (OverflowException) // Result is text and not a double
                                        {
                                            await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                        }
                                        catch (InvalidCastException) // Result is text and not a double
                                        {
                                            await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                        }
                                    }
                                    else
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);

                                    if (blnSquareBrackets)
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = '[' + x.Text + ']', token: token).ConfigureAwait(false);
                                }

                                await lblCapacity.DoThreadSafeAsync(x => x.Text += '/' + strSecondHalf, token: token).ConfigureAwait(false);
                            }
                            else if (strCapacityText == "[*]")
                                await lblCapacity.DoThreadSafeAsync(x => x.Text = "*", token: token).ConfigureAwait(false);
                            else
                            {
                                bool blnSquareBrackets = strCapacityText.StartsWith('[');
                                strCapacity = strCapacityText;
                                if (blnSquareBrackets && strCapacity.Length > 2)
                                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                if (strCapacityText.StartsWith("FixedValues(", StringComparison.Ordinal))
                                {
                                    string[] strValues = strCapacityText.TrimStartOnce("FixedValues(", true)
                                                                        .TrimEndOnce(')')
                                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    lblCapacity.Text
                                        = strValues[Math.Max(Math.Min(intRatingValue, strValues.Length) - 1, 0)];
                                }
                                else if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decCapacity))
                                {
                                    try
                                    {
                                        (bool blnIsSuccess2, object objProcess2) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                            strCapacity.Replace(
                                                "Rating", intRatingValue.ToString(GlobalSettings.InvariantCultureInfo)), token: token).ConfigureAwait(false);
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = blnIsSuccess2
                                                                                ? ((double)objProcess2).ToString("#,0.##", GlobalSettings.CultureInfo)
                                                                                : strCapacity, token: token).ConfigureAwait(false);
                                    }
                                    catch (OverflowException) // Result is text and not a double
                                    {
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                    }
                                    catch (InvalidCastException) // Result is text and not a double
                                    {
                                        await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token: token).ConfigureAwait(false);
                                    }
                                }
                                else
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);

                                if (blnSquareBrackets)
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = '[' + lblCapacity.Text + ']', token: token).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await lblCapacity.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                        }
                    }

                    bool blnShowCapacity = !string.IsNullOrEmpty(await lblCapacity.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                    await lblCapacityLabel.DoThreadSafeAsync(x => x.Visible = blnShowCapacity, token: token).ConfigureAwait(false);

                    // Rating.
                    string strExpression = objXmlGear.SelectSingleNodeAndCacheExpression("rating", token)?.Value ?? string.Empty;
                    if (strExpression == "0")
                        strExpression = string.Empty;
                    int intRating = int.MaxValue;
                    if (!string.IsNullOrEmpty(strExpression))
                    {
                        if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                        {
                            string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                              .Split(',', StringSplitOptions.RemoveEmptyEntries);
                            strExpression = strValues[Math.Max(Math.Min(intRatingValue, strValues.Length) - 1, 0)]
                                .Trim('[', ']');
                        }

                        if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decRating))
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(
                                       Utils.StringBuilderPool, out StringBuilder sbdValue))
                            {
                                sbdValue.Append(strExpression);
                                await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                                                                 async () =>
                                                                 {
                                                                     if (_objGearParent is IHasRating objParentCast)
                                                                     {
                                                                         return (await objParentCast.GetRatingAsync(token).ConfigureAwait(false))
                                                                            .ToString(GlobalSettings.InvariantCultureInfo);
                                                                     }
                                                                     return "0";
                                                                 }, token: token).ConfigureAwait(false);
                                sbdValue.Replace(
                                    "{Rating}", intRatingValue.ToString(GlobalSettings.InvariantCultureInfo));
                                await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);

                                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                                intRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                            }
                        }
                        else
                            intRating = decRating.StandardRound();
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
                                if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                                {
                                    string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                                      .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    strExpression
                                        = strValues[Math.Max(Math.Min(intRatingValue, strValues.Length) - 1, 0)]
                                            .Trim('[', ']');
                                }

                                if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decMinRating))
                                {
                                    using (new FetchSafelyFromObjectPool<StringBuilder>(
                                               Utils.StringBuilderPool, out StringBuilder sbdValue))
                                    {
                                        sbdValue.Append(strExpression);
                                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                                                                         async () =>
                                                                         {
                                                                             if (_objGearParent is IHasRating objParentCast)
                                                                             {
                                                                                 return (await objParentCast.GetRatingAsync(token).ConfigureAwait(false))
                                                                                    .ToString(GlobalSettings.InvariantCultureInfo);
                                                                             }
                                                                             return "0";
                                                                         }, token: token).ConfigureAwait(false);
                                        sbdValue.Replace("{Rating}", intRatingValue.ToString(GlobalSettings.InvariantCultureInfo));
                                        await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);

                                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                            sbdValue.ToString(), token).ConfigureAwait(false);
                                        intMinimumRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                                    }
                                }
                                else
                                    intMinimumRating = decMinRating.StandardRound();
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
                            while (intMaximum > intMinimum && !await objXmlGear.CheckAvailRestrictionAsync(_objCharacter, intMaximum, _intAvailModifier, token: token).ConfigureAwait(false))
                            {
                                --intMaximum;
                            }
                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                        }

                        if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                        {
                            decimal decCostMultiplier = await nudGearQty.DoThreadSafeFuncAsync(x => x.Value / x.Increment, token: token).ConfigureAwait(false);
                            if (await chkDoItYourself.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                                decCostMultiplier *= 0.5m;
                            decCostMultiplier *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                            if (_setBlackMarketMaps.Contains(objXmlGear.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                                decCostMultiplier *= 0.9m;
                            int intMinimum = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token).ConfigureAwait(false);
                            int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token).ConfigureAwait(false);
                            decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                            while (intMaximum > intMinimum && !await objXmlGear.CheckNuyenRestrictionAsync(decNuyen, decCostMultiplier, intMaximum, token).ConfigureAwait(false))
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
                sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');

                // Only add in category filter if we either are not searching or we have the option set to only search in categories
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0))
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                else if (_setAllowedCategories.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                        {
                            sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
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
                            sbdNameFilter.Append("name = ").Append(strItem.CleanXPath()).Append(" or ");
                        }

                        if (sbdNameFilter.Length > 0)
                        {
                            sbdNameFilter.Length -= 4;
                            sbdFilter.Append(" and (").Append(sbdNameFilter).Append(')');
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
                    sbdFilter.Append(" and ammoforweapontype = ").Append(ForceItemAmmoForWeaponType.CleanXPath());

                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            int intOverLimit = 0;
            List<ListItem> lstGears = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                decimal decBaseCostMultiplier = await nudGearQty.DoThreadSafeFuncAsync(x => x.Value / x.Increment, token: token).ConfigureAwait(false);
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

                    if (!await objXmlGear.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
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
                              || await objXmlGear.CheckAvailRestrictionAsync(_objCharacter, 1, _intAvailModifier, token).ConfigureAwait(false)
                              && (blnFreeItem || !blnShowOnlyAffordItems
                                              || await objXmlGear.CheckNuyenRestrictionAsync(decNuyen, decCostMultiplier, token: token).ConfigureAwait(false)))
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
                                                                       objLoopItem.Name + strSpace + '[' + objFoundItem.Name + ']');
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

        #endregion Methods
    }
}
