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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class SelectArmor : Form
    {
        private string _strSelectedArmor = string.Empty;

        private int _intLoading = 1;
        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;
        private decimal _decMarkup;
        private bool _blnFreeCost;
        private Armor _objSelectedArmor;

        private readonly XmlDocument _objXmlDocument;
        private readonly XPathNavigator _objXmlArmorDocumentChummerNode;
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory;
        private HashSet<string> _setBlackMarketMaps;
        private int _intRating;
        private bool _blnBlackMarketDiscount;

        private CancellationTokenSource _objUpdateArmorInfoCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private CancellationTokenSource _objArmorSelectedIndexChangedCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource;
        private readonly CancellationToken _objGenericToken;

        #region Control Events

        public SelectArmor(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            tabControl.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            // Load the Armor information.
            _objXmlDocument = objCharacter.LoadData("armor.xml");
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            _objXmlArmorDocumentChummerNode = objCharacter.LoadDataXPath("armor.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(objCharacter.GenerateBlackMarketMappings(
                                             objCharacter.LoadDataXPath("armor.xml")
                                                         .SelectSingleNodeAndCacheExpression("/chummer")));
            _lstCategory = Utils.ListItemListPool.Get();
            _objGenericCancellationTokenSource = new CancellationTokenSource();
            _objGenericToken = _objGenericCancellationTokenSource.Token;

            // Prevent Enter key from closing the form when NumericUpDown controls have focus
            nudMinimumCost.KeyDown += NumericUpDown_KeyDown;
            nudMaximumCost.KeyDown += NumericUpDown_KeyDown;
            nudExactCost.KeyDown += NumericUpDown_KeyDown;
        }

        private async void SelectArmor_Load(object sender, EventArgs e)
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
                }

                DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.TopRight,
                    Format = await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).GetNuyenFormatAsync(_objGenericToken).ConfigureAwait(false)
                        + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: _objGenericToken).ConfigureAwait(false),
                    NullValue = null
                };
                Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

                // Populate the Armor Category list.
                if (_objXmlArmorDocumentChummerNode.SelectSingleNodeAndCacheExpression("/chummer/categories/category", _objGenericToken) != null)
                {
                    string strFilterPrefix = "armors/armor[(" + await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).BookXPathAsync(token: _objGenericToken).ConfigureAwait(false) + ") and category = ";
                    foreach (XPathNavigator objXmlCategory in _objXmlArmorDocumentChummerNode.SelectAndCacheExpression("/chummer/categories/category", _objGenericToken))
                    {
                        string strInnerText = objXmlCategory.Value;
                        if (_objXmlArmorDocumentChummerNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + "]") != null)
                        {
                            _lstCategory.Add(new ListItem(strInnerText,
                                objXmlCategory.SelectSingleNodeAndCacheExpression("@translate", _objGenericToken)?.Value ?? strInnerText));
                        }
                    }
                }
                _lstCategory.Sort(CompareListItems.CompareNames);

                if (_lstCategory.Count > 0)
                {
                    _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: _objGenericToken).ConfigureAwait(false)));
                }

                await cboCategory.PopulateWithListItemsAsync(_lstCategory, _objGenericToken).ConfigureAwait(false);
                await cboCategory.DoThreadSafeAsync(x =>
                {
                    // Select the first Category in the list.
                    if (!string.IsNullOrEmpty(_strSelectCategory))
                        x.SelectedValue = _strSelectCategory;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }, _objGenericToken).ConfigureAwait(false);

                Interlocked.Decrement(ref _intLoading);

                await RefreshList(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SelectArmor_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateArmorInfoCancellationTokenSource, null);
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
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objArmorSelectedIndexChangedCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm(_objGenericToken).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void lstArmor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;

            try
            {
                CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
                CancellationToken objNewToken = objNewCancellationTokenSource.Token;
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objArmorSelectedIndexChangedCancellationTokenSource, objNewCancellationTokenSource);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_objGenericToken, objNewToken))
                {
                    CancellationToken token = objJoinedCancellationTokenSource.Token;
                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        XmlNode xmlArmor = null;
                        string strSelectedId = await lstArmor
                            .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(strSelectedId))
                            xmlArmor = _objXmlDocument.TryGetNodeByNameOrId("/chummer/armors/armor", strSelectedId);
                        if (xmlArmor != null)
                        {
                            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                // Create the Armor so we can show its Total Avail (some Armor includes Chemical Seal which adds +6 which wouldn't be factored in properly otherwise).
                                Armor objArmor = new Armor(_objCharacter);
                                try
                                {
                                    List<Weapon> lstWeapons = new List<Weapon>(1);
                                    await objArmor.CreateAsync(xmlArmor, 0, lstWeapons, true, true, true, true, token)
                                        .ConfigureAwait(false);

                                    Armor objOldArmor = Interlocked.Exchange(ref _objSelectedArmor, objArmor);
                                    if (objOldArmor != null)
                                        await objOldArmor.DisposeAsync().ConfigureAwait(false);

                                    int intRating = 0;
                                    if (xmlArmor.TryGetInt32FieldQuickly("rating", ref intRating))
                                    {
                                        await nudRating.DoThreadSafeAsync(x => x.Maximum = intRating, token)
                                            .ConfigureAwait(false);
                                        if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                .ConfigureAwait(false))
                                        {
                                            int intMaximum = await nudRating
                                                .DoThreadSafeFuncAsync(x => x.MaximumAsInt, token)
                                                .ConfigureAwait(false);
                                            while (intMaximum > 1 && !await SelectionShared
                                                       .CheckAvailRestrictionAsync(xmlArmor, _objCharacter, intMaximum,
                                                       (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objArmor.SourceIDString, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(),
                                                           token: token).ConfigureAwait(false))
                                            {
                                                --intMaximum;
                                            }

                                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token)
                                                .ConfigureAwait(false);
                                        }

                                        if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                .ConfigureAwait(false)
                                            && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                .ConfigureAwait(false))
                                        {
                                            decimal decCostMultiplier =
                                                1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token)
                                                    .ConfigureAwait(false) / 100.0m;
                                            if (_setBlackMarketMaps.Contains(xmlArmor["category"]?.InnerTextViaPool(token)))
                                                decCostMultiplier *= 0.9m;
                                            int intMaximum = await nudRating
                                                .DoThreadSafeFuncAsync(x => x.MaximumAsInt, token)
                                                .ConfigureAwait(false);
                                            decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                                            while (intMaximum > 1 && !await SelectionShared
                                                       .CheckNuyenRestrictionAsync(xmlArmor, _objCharacter, decNuyen,
                                                           decCostMultiplier, intMaximum, token).ConfigureAwait(false))
                                            {
                                                --intMaximum;
                                            }

                                            await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token)
                                                .ConfigureAwait(false);
                                        }

                                        await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token)
                                            .ConfigureAwait(false);
                                        await nudRating.DoThreadSafeAsync(x =>
                                        {
                                            x.Minimum = 1;
                                            x.Value = 1;
                                            x.Enabled = x.Minimum != x.Maximum;
                                            x.Visible = true;
                                        }, token).ConfigureAwait(false);
                                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token)
                                            .ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token)
                                            .ConfigureAwait(false);
                                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token)
                                            .ConfigureAwait(false);
                                        await nudRating.DoThreadSafeAsync(x =>
                                        {
                                            x.Minimum = 0;
                                            x.Maximum = 0;
                                            x.Value = 0;
                                            x.Enabled = false;
                                            x.Visible = false;
                                        }, token).ConfigureAwait(false);
                                    }

                                    string strRatingLabel = xmlArmor["ratinglabel"]?.InnerTextViaPool(token);
                                    strRatingLabel = !string.IsNullOrEmpty(strRatingLabel)
                                        ? string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("Label_RatingFormat", token: token)
                                                .ConfigureAwait(false),
                                            await LanguageManager.GetStringAsync(strRatingLabel, token: token)
                                                .ConfigureAwait(false))
                                        : await LanguageManager.GetStringAsync("Label_Rating", token: token)
                                            .ConfigureAwait(false);
                                    await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token)
                                        .ConfigureAwait(false);
                                }
                                catch
                                {
                                    Interlocked.CompareExchange(ref _objSelectedArmor, null, objArmor);
                                    await objArmor.DisposeAsync().ConfigureAwait(false);
                                    throw;
                                }
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            Armor objOldArmor = Interlocked.Exchange(ref _objSelectedArmor, null);
                            if (objOldArmor != null)
                                await objOldArmor.DisposeAsync().ConfigureAwait(false);
                            await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token)
                                .ConfigureAwait(false);
                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Visible = false;
                                x.Enabled = false;
                                x.Minimum = 0;
                                x.Value = 0;
                            }, token).ConfigureAwait(false);
                        }

                        await UpdateArmorInfo(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }
                }
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
                await RefreshList(_objGenericToken).ConfigureAwait(false);
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
                            await nudMinimumCost.DoThreadSafeAsync(x => x.Value = decMaximumCost, _objGenericToken).ConfigureAwait(false);
                        else
                            await nudMaximumCost.DoThreadSafeAsync(x => x.Value = decMinimumCost, _objGenericToken).ConfigureAwait(false);
                    }
                }

                _intLoading = 0;

                await RefreshList(_objGenericToken).ConfigureAwait(false);
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

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm(_objGenericToken).ConfigureAwait(false);
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

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(_objGenericToken).ConfigureAwait(false);
                }
                await UpdateArmorInfo(_objGenericToken).ConfigureAwait(false);
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
                await UpdateArmorInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                await UpdateArmorInfo(_objGenericToken).ConfigureAwait(false);
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
                    await RefreshList(_objGenericToken).ConfigureAwait(false);
                }
                await UpdateArmorInfo(_objGenericToken).ConfigureAwait(false);
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
                case Keys.Down when lstArmor.SelectedIndex + 1 < lstArmor.Items.Count:
                    ++lstArmor.SelectedIndex;
                    break;

                case Keys.Down:
                    {
                        if (lstArmor.Items.Count > 0)
                        {
                            lstArmor.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstArmor.SelectedIndex >= 1:
                    --lstArmor.SelectedIndex;
                    break;

                case Keys.Up:
                    {
                        if (lstArmor.Items.Count > 0)
                        {
                            lstArmor.SelectedIndex = lstArmor.Items.Count - 1;
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

        private async void dgvArmor_DoubleClick(object sender, EventArgs e)
        {
            await AcceptForm(_objGenericToken).ConfigureAwait(false);
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
        /// Armor that was selected in the dialogue.
        /// </summary>
        public string SelectedArmor => _strSelectedArmor;

        /// <summary>
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public int Rating => _intRating;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Refreshes the displayed lists
        /// </summary>
        private async Task RefreshList(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;

            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }

            using (CancellationTokenSource objJoinedCancellationTokenSource =
                   CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append("/chummer/armors/armor[(")
                            .Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false))
                            .Append(')');

                        string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                               && (GlobalSettings.SearchInCategoryOnly
                                                                   || await txtSearch
                                                                       .DoThreadSafeFuncAsync(x => x.TextLength == 0,
                                                                           token: token).ConfigureAwait(false)))
                            sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                        else
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                       out StringBuilder sbdCategoryFilter))
                            {
                                foreach (string strItem in _lstCategory.Select(x => x.Value?.ToString()))
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

                        string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token)
                            .ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(strSearch))
                            sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                        // Apply cost filtering
                        decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                        decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                        decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                        
                        if (decExactCost > 0)
                        {
                            // Exact cost filtering
                            sbdFilter.Append(" and (cost = ").Append(decExactCost.ToString(GlobalSettings.InvariantCultureInfo)).Append(')');
                        }
                        else if (decMinimumCost != 0 || decMaximumCost != 0)
                        {
                            // Range cost filtering
                            sbdFilter.Append(" and (").Append(CommonFunctions.GenerateNumericRangeXPath(decMaximumCost, decMinimumCost, "cost")).Append(')');
                        }

                        await BuildArmorList(_objXmlDocument.SelectNodes(sbdFilter.Append(']').ToString()),
                            token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Builds the list of Armors to render in the active tab.
        /// </summary>
        /// <param name="objXmlArmorList">XmlNodeList of Armors to render.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task BuildArmorList(XmlNodeList objXmlArmorList, CancellationToken token = default)
        {
            decimal decBaseMarkup = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
            bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            decimal decNuyen = !blnFreeItem && blnShowOnlyAffordItems ? await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false) : decimal.MaxValue;
            switch (await tabControl.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false))
            {
                case 1:
                    DataTable tabArmor = new DataTable("armor");
                    tabArmor.Columns.Add("ArmorGuid");
                    tabArmor.Columns.Add("ArmorName");
                    tabArmor.Columns.Add("Armor");
                    tabArmor.Columns["Armor"].DataType = typeof(int);
                    tabArmor.Columns.Add("Capacity");
                    tabArmor.Columns["Capacity"].DataType = typeof(decimal);
                    tabArmor.Columns.Add("Avail");
                    tabArmor.Columns["Avail"].DataType = typeof(AvailabilityValue);
                    tabArmor.Columns.Add("Special");
                    tabArmor.Columns.Add("Source");
                    tabArmor.Columns["Source"].DataType = typeof(SourceString);
                    tabArmor.Columns.Add("Cost");
                    tabArmor.Columns["Cost"].DataType = typeof(NuyenString);

                    IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        // Populate the Armor list.
                        foreach (XmlNode objXmlArmor in objXmlArmorList)
                        {
                            decimal decCostMultiplier = decBaseMarkup;
                            if (_setBlackMarketMaps.Contains(objXmlArmor["category"]?.InnerTextViaPool(token)))
                                decCostMultiplier *= 0.9m;
                            if (!blnHideOverAvailLimit
                                || await SelectionShared
                                    .CheckAvailRestrictionAsync(objXmlArmor, _objCharacter, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlArmor["id"]?.InnerTextViaPool(token), blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token)
                                    .ConfigureAwait(false) && (blnFreeItem
                                                               || !blnShowOnlyAffordItems
                                                               || await SelectionShared.CheckNuyenRestrictionAsync(
                                                                   objXmlArmor, _objCharacter, decNuyen, decCostMultiplier,
                                                                   token: token).ConfigureAwait(false)))
                            {
                                Armor objArmor = new Armor(_objCharacter);
                                try
                                {
                                    List<Weapon> lstWeapons = new List<Weapon>(1);
                                    await objArmor
                                        .CreateAsync(objXmlArmor, 0, lstWeapons, true, true, true, true, token)
                                        .ConfigureAwait(false);

                                    string strArmorGuid = objArmor.SourceIDString;
                                    string strArmorName = await objArmor.GetCurrentDisplayNameAsync(token)
                                        .ConfigureAwait(false);
                                    int intArmor = await objArmor.GetTotalArmorAsync(token: token).ConfigureAwait(false);
                                    decimal.TryParse(await objArmor.CalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false),
                                        System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decCapacity);
                                    AvailabilityValue objAvail = await objArmor.TotalAvailTupleAsync(token: token)
                                        .ConfigureAwait(false);
                                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdAccessories))
                                    {
                                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                                        {
                                            sbdAccessories.AppendLine(await objMod.GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false));
                                        }

                                        foreach (Gear objGear in objArmor.GearChildren)
                                        {
                                            sbdAccessories.AppendLine(await objGear.GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false));
                                        }

                                        if (sbdAccessories.Length > 0)
                                            sbdAccessories.Length -= Environment.NewLine.Length;
                                        SourceString strSource = await SourceString.GetSourceStringAsync(
                                                objArmor.Source,
                                                await objArmor.DisplayPageAsync(GlobalSettings.Language, token)
                                                    .ConfigureAwait(false),
                                                GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter,
                                                token)
                                            .ConfigureAwait(false);
                                        NuyenString strCost =
                                            await NuyenString.GetNuyenStringAsync((await objArmor.DisplayCost(false, token: token).ConfigureAwait(false)).Item1, token: token).ConfigureAwait(false);

                                        tabArmor.Rows.Add(strArmorGuid, strArmorName, intArmor, decCapacity, objAvail,
                                            sbdAccessories.ToString(), strSource, strCost);
                                    }
                                }
                                finally
                                {
                                    await objArmor.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }

                    DataSet set = new DataSet("armor");
                    set.Tables.Add(tabArmor);

                    dgvArmor.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    dgvArmor.DataSource = set;
                    dgvArmor.DataMember = "armor";
                    break;

                default:
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstArmors))
                    {
                        int intOverLimit = 0;
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                        foreach (XmlNode objXmlArmor in objXmlArmorList)
                        {
                            decimal decCostMultiplier = decBaseMarkup;
                            if (_setBlackMarketMaps.Contains(objXmlArmor["category"]?.InnerTextViaPool(token)))
                                decCostMultiplier *= 0.9m;
                            if ((!blnHideOverAvailLimit
                                 || await SelectionShared.CheckAvailRestrictionAsync(objXmlArmor, _objCharacter, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlArmor["id"]?.InnerTextViaPool(token), blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
                                && (blnFreeItem
                                    || !blnShowOnlyAffordItems
                                    || await SelectionShared.CheckNuyenRestrictionAsync(
                                        objXmlArmor, _objCharacter, decNuyen, decCostMultiplier, token: token).ConfigureAwait(false)))
                            {
                                string strDisplayName = objXmlArmor["translate"]?.InnerTextViaPool(token)
                                                        ?? objXmlArmor["name"]?.InnerTextViaPool(token);
                                if (!GlobalSettings.SearchInCategoryOnly && txtSearch.TextLength != 0)
                                {
                                    string strCategory = objXmlArmor["category"]?.InnerTextViaPool(token);
                                    if (!string.IsNullOrEmpty(strCategory))
                                    {
                                        ListItem objFoundItem
                                            = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                                        {
                                            strDisplayName += strSpace + "[" + objFoundItem.Name + "]";
                                        }
                                    }
                                }

                                lstArmors.Add(new ListItem(objXmlArmor["id"]?.InnerTextViaPool(token), strDisplayName));
                            }
                            else
                                ++intOverLimit;
                        }

                        lstArmors.Sort(CompareListItems.CompareNames);
                        if (intOverLimit > 0)
                        {
                            // Add after sort so that it's always at the end
                            lstArmors.Add(new ListItem(string.Empty,
                                                       string.Format(GlobalSettings.CultureInfo,
                                                                     await LanguageManager.GetStringAsync(
                                                                         "String_RestrictedItemsHidden", token: token).ConfigureAwait(false),
                                                                     intOverLimit)));
                        }

                        string strOldSelected = await lstArmor.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await lstArmor.PopulateWithListItemsAsync(lstArmors, token: token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }
                        if (!string.IsNullOrEmpty(strOldSelected))
                            await lstArmor.DoThreadSafeAsync(x => x.SelectedValue = strOldSelected, token: token).ConfigureAwait(false);
                        else
                            await lstArmor.DoThreadSafeAsync(x => x.SelectedIndex = -1, token: token).ConfigureAwait(false);
                        break;
                    }
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strSelectedId = string.Empty;
            CursorWait objCursorWait
                = await CursorWait.NewAsync(this, true, token: token).ConfigureAwait(false);
            try
            {
                switch (await tabControl.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false))
                {
                    case 0:
                        strSelectedId = await lstArmor.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                        break;

                    case 1:
                        strSelectedId = await dgvArmor.DoThreadSafeFuncAsync(x => x.SelectedRows[0].Cells[0].Value?.ToString(), token).ConfigureAwait(false);
                        break;
                }
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    if (!GlobalSettings.SearchInCategoryOnly && await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token).ConfigureAwait(false) != 0)
                    {
                        XPathNavigator objArmorDataNode
                            = _objXmlArmorDocumentChummerNode.TryGetNodeByNameOrId("armors/armor", strSelectedId);
                        if (objArmorDataNode != null)
                            _strSelectCategory = objArmorDataNode
                                                     .SelectSingleNodeAndCacheExpression(
                                                         "category", token)?.Value
                                                 ?? await cboCategory
                                                          .DoThreadSafeFuncAsync(
                                                              x => x.SelectedValue?.ToString(), token)
                                                          .ConfigureAwait(false);
                        else
                            _strSelectCategory = await cboCategory
                                                       .DoThreadSafeFuncAsync(
                                                           x => x.SelectedValue?.ToString(), token)
                                                       .ConfigureAwait(false);
                    }
                    else
                        _strSelectCategory = await cboCategory
                                                   .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(),
                                                                          token).ConfigureAwait(false);

                    _strSelectedArmor = strSelectedId;
                    _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false);
                    _blnFreeCost = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                    _intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);
                    _blnBlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token).ConfigureAwait(false);
        }

        private async Task UpdateArmorInfo(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateArmorInfoCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                Armor objSelectedArmor = _objSelectedArmor;
                if (objSelectedArmor != null)
                {
                    bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(objSelectedArmor.Category);

                    objSelectedArmor.DiscountCost = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x =>
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
                        return x.Checked;
                    }, token).ConfigureAwait(false);
                    await objSelectedArmor.SetRatingAsync(await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false), token).ConfigureAwait(false);

                    await objSelectedArmor.SetSourceDetailAsync(lblSource, token: token).ConfigureAwait(false);
                    bool blnShowSource = !string.IsNullOrEmpty(
                        await lblSource.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnShowSource, token: token)
                                        .ConfigureAwait(false);

                    await lblArmorValueLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                    string strArmorValue = await objSelectedArmor.GetDisplayArmorValueAsync(token).ConfigureAwait(false);
                    await lblArmorValue.DoThreadSafeAsync(x => x.Text = strArmorValue, token).ConfigureAwait(false);
                    await lblCapacityLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                    string strCapacity = await objSelectedArmor.GetDisplayCapacityAsync(token).ConfigureAwait(false);
                    await lblCapacity.DoThreadSafeAsync(x => x.Text = strCapacity, token).ConfigureAwait(false);

                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                    decimal decItemCost = 0;
                    string strCost;
                    if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                    {
                        strCost = 0.0m.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                        + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        (strCost, decItemCost) = await objSelectedArmor.DisplayCost(true,
                            await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false) / 100.0m, token).ConfigureAwait(false);
                    }
                    await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token).ConfigureAwait(false);

                    AvailabilityValue objTotalAvail = await objSelectedArmor.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    string strAvail = await objTotalAvail.ToStringAsync(token).ConfigureAwait(false);
                    await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token)
                                        .ConfigureAwait(false);
                    await lblAvailLabel
                            .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token)
                            .ConfigureAwait(false);
                    string strTest = await _objCharacter.AvailTestAsync(decItemCost, objTotalAvail, token)
                                                        .ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                    await lblTestLabel
                            .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token)
                            .ConfigureAwait(false);
                }
                else
                {
                    await chkBlackMarketDiscount.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        x.Checked = false;
                    }, token).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await lblSource.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblSource.SetToolTipTextAsync(string.Empty, token).ConfigureAwait(false);

                    await lblArmorValueLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await lblArmorValue.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblCapacityLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await lblCapacity.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await lblCost.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await lblAvail.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                }
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
