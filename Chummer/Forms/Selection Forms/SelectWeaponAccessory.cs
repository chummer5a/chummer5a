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
    public partial class SelectWeaponAccessory : Form
    {
        private string _strSelectedAccessory;
        private decimal _decMarkup;
        private bool _blnFreeCost;
        private int _intSelectedRating;

        private bool _blnLoading = true;
        private readonly List<string> _lstAllowedMounts = new List<string>();
        private Weapon _objParentWeapon;
        private bool _blnIsParentWeaponBlackMarketAllowed;
        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseChummerNode;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private HashSet<string> _setBlackMarketMaps;

        #region Control Events

        public SelectWeaponAccessory(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Prevent Enter key from closing the form when NumericUpDown controls have focus
            nudMinimumCost.KeyDown += NumericUpDown_KeyDown;
            nudMaximumCost.KeyDown += NumericUpDown_KeyDown;
            nudExactCost.KeyDown += NumericUpDown_KeyDown;
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            // Load the Weapon information.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("weapons.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseChummerNode));
        }

        private async void SelectWeaponAccessory_Load(object sender, EventArgs e)
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
                    x.Text = string.Format(GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                    x.Visible = true;
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }

            _blnLoading = false;
            await RefreshList().ConfigureAwait(false);
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private async Task RefreshList(CancellationToken token = default)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstAccessories))
            {
                // Populate the Accessory list.
                string strFilter = string.Empty;
                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    sbdFilter.Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false));
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
                        // StringBuilder.Insert can be slow because of in-place replaces, so use concat instead
                        strFilter = string.Concat("[", sbdFilter.Append(']').ToString());
                }

                int intOverLimit = 0;
                int intMountRestricted = 0;
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                decimal decBaseCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                decimal decNuyen = blnFreeItem || !blnShowOnlyAffordItems ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                foreach (XPathNavigator objXmlAccessory in _xmlBaseChummerNode.Select(
                             "accessories/accessory" + strFilter))
                {
                    string strId = objXmlAccessory.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;

                    // Check mount requirements first
                    bool blnMeetsMountRequirements = await _objParentWeapon.CheckAccessoryRequirementsAsync(objXmlAccessory, token).ConfigureAwait(false);
                    if (!blnMeetsMountRequirements)
                    {
                        ++intMountRestricted;
                        continue;
                    }

                    decimal decCostMultiplier = decBaseCostMultiplier;
                    if (_blnIsParentWeaponBlackMarketAllowed)
                        decCostMultiplier *= 0.9m;
                    if (!blnHideOverAvailLimit || await objXmlAccessory.CheckAvailRestrictionAsync(_objCharacter, intAvailModifier: (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlAccessory.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false)
                        && (blnFreeItem || !blnShowOnlyAffordItems
                                        || await objXmlAccessory.CheckNuyenRestrictionAsync(
                                            _objCharacter, decNuyen, decCostMultiplier, token: token).ConfigureAwait(false)))
                    {
                        lstAccessories.Add(new ListItem(
                                               strId,
                                               objXmlAccessory.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                               ?? objXmlAccessory.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                               ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                    }
                    else
                        ++intOverLimit;
                }

                lstAccessories.Sort(CompareListItems.CompareNames);
                if (intOverLimit > 0)
                {
                    // Add after sort so that it's always at the end
                    lstAccessories.Add(new ListItem(string.Empty, string.Format(
                                                        GlobalSettings.CultureInfo,
                                                        await LanguageManager.GetStringAsync("String_RestrictedItemsHidden", token: token).ConfigureAwait(false),
                                                        intOverLimit)));
                }
                if (intMountRestricted > 0 && !string.IsNullOrEmpty(strSearch))
                {
                    // Add after sort so that it's always at the end
                    lstAccessories.Add(new ListItem(string.Empty, string.Format(
                                                        GlobalSettings.CultureInfo,
                                                        await LanguageManager.GetStringAsync("String_RestrictedItemsHiddenMount", token: token).ConfigureAwait(false),
                                                        intMountRestricted)));
                }

                string strOldSelected = await lstAccessory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                _blnLoading = true;
                await lstAccessory.PopulateWithListItemsAsync(lstAccessories, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstAccessory.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        private async void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
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

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await RefreshList().ConfigureAwait(false);
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo().ConfigureAwait(false);
        }

        private async void cboMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            await this.DoThreadSafeAsync(x => x.UpdateMountFields(true)).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                await UpdateGearInfo(false).ConfigureAwait(false);
        }

        private async void cboExtraMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            await this.DoThreadSafeAsync(x => x.UpdateMountFields(false)).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                await UpdateGearInfo(false).ConfigureAwait(false);
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

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Name of Accessory that was selected in the dialogue.
        /// </summary>
        public string SelectedAccessory => _strSelectedAccessory;

        /// <summary>
        /// Mount that was selected in the dialogue.
        /// </summary>
        public ValueTuple<string, string> SelectedMount => new ValueTuple<string, string>(cboMount.SelectedItem?.ToString(), cboExtraMount.SelectedItem?.ToString());

        /// <summary>
        /// Rating of the Accessory.
        /// </summary>
        public int SelectedRating => _intSelectedRating;

        /// <summary>
        /// The current weapon for which the accessory is being selected
        /// </summary>
        public async Task SetWeapon(Weapon objWeapon, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.Exchange(ref _objParentWeapon, objWeapon) == objWeapon)
                return;
            _lstAllowedMounts.Clear();
            if (objWeapon != null)
            {
                _lstAllowedMounts.AddRange(await objWeapon.GetAccessoryMountsAsync(token: token).ConfigureAwait(false));

                //TODO: Accessories don't use a category mapping, so we use parent weapon's category instead.
                if (await _objCharacter.GetBlackMarketDiscountAsync(token).ConfigureAwait(false))
                {
                    XPathNavigator xmlWeaponNode = await objWeapon.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                    string strCategory =
                        xmlWeaponNode != null
                            ? xmlWeaponNode.SelectSingleNodeAndCacheExpression("category", token)?.Value ?? string.Empty
                            : string.Empty;
                    _blnIsParentWeaponBlackMarketAllowed = !string.IsNullOrEmpty(strCategory) &&
                                                           _setBlackMarketMaps.Contains(strCategory);
                }
            }
            else
            {
                _blnIsParentWeaponBlackMarketAllowed = false;
            }
        }

        /// <summary>
        /// Whether the item should be added for free.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Whether the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        #endregion Properties

        #region Methods

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
        }

        private void UpdateMountFields(bool boolChangeExtraMountFirst)
        {
            if (cboMount.SelectedItem.ToString() != "None" && cboExtraMount.SelectedItem != null && cboExtraMount.SelectedItem.ToString() != "None"
                && cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString())
            {
                if (boolChangeExtraMountFirst)
                    cboExtraMount.SelectedIndex = 0;
                else
                    cboMount.SelectedIndex = 0;
                while (cboMount.SelectedItem.ToString() != "None" && cboExtraMount.SelectedItem.ToString() != "None" && cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString())
                {
                    if (boolChangeExtraMountFirst)
                        ++cboExtraMount.SelectedIndex;
                    else
                        ++cboMount.SelectedIndex;
                }
            }
        }

        private async Task UpdateGearInfo(bool blnUpdateMountComboBoxes = true, CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            XPathNavigator xmlAccessory = null;
            string strSelectedId = await lstAccessory
                                         .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                         .ConfigureAwait(false);
            // Retrieve the information for the selected Accessory.
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                xmlAccessory
                    = _xmlBaseChummerNode.TryGetNodeByNameOrId("accessories/accessory", strSelectedId);
            }

            if (xmlAccessory == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                return;
            }

            string strRC = xmlAccessory.SelectSingleNodeAndCacheExpression("rc", token)?.Value;
            if (!string.IsNullOrEmpty(strRC))
            {
                await lblRCLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                await lblRC.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.Text = strRC;
                }, token: token).ConfigureAwait(false);
            }
            else
            {
                await lblRC.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblRCLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
            }

            string strExpression = xmlAccessory.SelectSingleNodeAndCacheExpression("rating", token)?.Value ?? string.Empty;
            if (strExpression == "0")
                strExpression = string.Empty;
            int intMaxRating = int.MaxValue;
            if (!string.IsNullOrEmpty(strExpression))
            {
                intMaxRating = (await ProcessInvariantXPathExpression(strExpression, 0, token).ConfigureAwait(false)).StandardRound();
            }

            if (intMaxRating > 0 && intMaxRating != int.MaxValue)
            {
                await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaxRating, token: token).ConfigureAwait(false);
                if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                               .ConfigureAwait(false))
                {
                    int intMinimum = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token)
                                                    .ConfigureAwait(false);
                    int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token)
                                                    .ConfigureAwait(false);
                    while (intMaximum > intMinimum && !await xmlAccessory
                                                             .CheckAvailRestrictionAsync(
                                                                 _objCharacter, intMaximum, (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: xmlAccessory.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token)
                                                             .ConfigureAwait(false))
                    {
                        --intMaximum;
                    }

                    await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                }

                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                .ConfigureAwait(false) && !await chkFreeItem
                        .DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                {
                    decimal decCostMultiplier
                        = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false)
                        / 100.0m;
                    if (_setBlackMarketMaps.Contains(xmlAccessory.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                        decCostMultiplier *= 0.9m;
                    int intMinimum = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token)
                                                    .ConfigureAwait(false);
                    int intMaximum = await nudRating.DoThreadSafeFuncAsync(x => x.MaximumAsInt, token: token)
                                                    .ConfigureAwait(false);
                    decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                    while (intMaximum > intMinimum && !await xmlAccessory
                                                             .CheckNuyenRestrictionAsync(
                                                                 _objCharacter, decNuyen, decCostMultiplier, intMaximum,
                                                                 token).ConfigureAwait(false))
                    {
                        --intMaximum;
                    }

                    await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaximum, token: token).ConfigureAwait(false);
                }

                await nudRating.DoThreadSafeAsync(x =>
                {
                    x.Enabled = x.Maximum != x.Minimum;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
            }
            else
            {
                await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                await nudRating.DoThreadSafeAsync(x =>
                {
                    x.Enabled = false;
                    x.Visible = false;
                }, token: token).ConfigureAwait(false);
                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
            }

            if (blnUpdateMountComboBoxes)
            {
                string strDataMounts = xmlAccessory.SelectSingleNodeAndCacheExpression("mount", token)?.Value;
                List<string> lstMounts = new List<string>(1);
                if (!string.IsNullOrEmpty(strDataMounts))
                {
                    lstMounts.AddRange(strDataMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));
                }

                if (!lstMounts.Contains("None"))
                    lstMounts.Add("None");

                string strSelectedMount = await cboMount.DoThreadSafeFuncAsync(x =>
                {
                    x.Visible = true;
                    x.Items.Clear();
                    foreach (string strCurrentMount in lstMounts)
                    {
                        if (!string.IsNullOrEmpty(strCurrentMount) && _lstAllowedMounts.Contains(strCurrentMount))
                        {
                            x.Items.Add(strCurrentMount);
                        }
                    }

                    x.Enabled = x.Items.Count > 1;
                    x.SelectedIndex = 0;
                    return x.SelectedItem.ToString();
                }, token: token).ConfigureAwait(false);
                await lblMountLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                List<string> lstExtraMounts = new List<string>(1);
                string strExtraMount = xmlAccessory.SelectSingleNodeAndCacheExpression("extramount", token)?.Value;
                if (!string.IsNullOrEmpty(strExtraMount))
                {
                    lstExtraMounts.AddRange(strExtraMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));
                }

                if (!lstExtraMounts.Contains("None"))
                    lstExtraMounts.Add("None");

                bool blnShowExtraMountLabel = await cboExtraMount.DoThreadSafeFuncAsync(x =>
                {
                    x.Items.Clear();
                    foreach (string strCurrentMount in lstExtraMounts)
                    {
                        if (!string.IsNullOrEmpty(strCurrentMount) && _lstAllowedMounts.Contains(strCurrentMount))
                        {
                            x.Items.Add(strCurrentMount);
                        }
                    }

                    x.Enabled = x.Items.Count > 1;
                    x.SelectedIndex = 0;
                    if (strSelectedMount != "None" && x.SelectedItem.ToString() != "None"
                                                   && strSelectedMount == x.SelectedItem.ToString())
                        ++x.SelectedIndex;
                    x.Visible = x.Enabled && x.SelectedItem.ToString() != "None";
                    return x.Visible;
                }, token: token).ConfigureAwait(false);
                await lblExtraMountLabel.DoThreadSafeAsync(x => x.Visible = blnShowExtraMountLabel, token: token)
                                        .ConfigureAwait(false);
            }

            int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token)
                                           .ConfigureAwait(false);

            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail
                = await new AvailabilityValue(intRating, xmlAccessory.SelectSingleNodeAndCacheExpression("avail", token)?.Value,
                (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: xmlAccessory.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound())
                    .ToStringAsync(token).ConfigureAwait(false);
            await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
            await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token)
                               .ConfigureAwait(false);

            string strNuyen = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
            {
                string strCost = "0";
                xmlAccessory.TryGetStringFieldQuickly("cost", ref strCost);

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
                        await lblCost.DoThreadSafeAsync(
                                         x => x.Text = decMin.ToString(strNuyenFormat,
                                                                       GlobalSettings.CultureInfo)
                                                       + strNuyen + "+",
                                         token: token)
                                     .ConfigureAwait(false);
                    }
                    else
                    {
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                               .ConfigureAwait(false);
                        await lblCost.DoThreadSafeAsync(
                                         x => x.Text = decMin.ToString(strNuyenFormat,
                                                                       GlobalSettings.CultureInfo) + strSpace + "-"
                                                       + strSpace + decMax.ToString(strNuyenFormat,
                                                           GlobalSettings.CultureInfo)
                                                       + strNuyen, token: token)
                                     .ConfigureAwait(false);
                    }

                    string strTest = await _objCharacter.AvailTestAsync(decMax, strAvail, token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                }
                else
                {
                    decimal decCost = await ProcessInvariantXPathExpression(strCost, intRating, token).ConfigureAwait(false);
                    
                    // Apply any markup.
                    decCost *= 1
                               + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                   .ConfigureAwait(false) / 100.0m;

                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                    .ConfigureAwait(false))
                        decCost *= 0.9m;
                    decCost *= _objParentWeapon.AccessoryMultiplier;
                    if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                    {
                        string strSelectedMount = await cboMount.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: token).ConfigureAwait(false);
                        string strSelectedExtraMount = await cboExtraMount.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: token).ConfigureAwait(false);
                        bool blnBreakAfterFound = string.IsNullOrEmpty(strSelectedMount) || string.IsNullOrEmpty(strSelectedExtraMount);
                        foreach (string strDoubledCostSlot in _objParentWeapon.DoubledCostModificationSlots.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (strDoubledCostSlot == strSelectedMount || strDoubledCostSlot == strSelectedExtraMount)
                            {
                                decCost *= 2;
                                if (blnBreakAfterFound)
                                    break;
                                else
                                    blnBreakAfterFound = true;
                            }
                        }
                    }

                    string strCostText = decCost.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                        + strNuyen;
                    await lblCost
                          .DoThreadSafeAsync(
                              x => x.Text = strCostText, token: token)
                          .ConfigureAwait(false);
                    string strTest = await _objCharacter.AvailTestAsync(decCost, strAvail, token: token)
                                                        .ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                }
            }
            else
            {
                string strCostText = 0.0m.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                        + strNuyen;
                await lblCost
                      .DoThreadSafeAsync(
                          x => x.Text = strCostText, token: token)
                      .ConfigureAwait(false);
                string strTest = await _objCharacter.AvailTestAsync(0, strAvail, token: token).ConfigureAwait(false);
                await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
            }

            XPathNavigator xmlAccessoryRatingLabel = xmlAccessory.SelectSingleNodeAndCacheExpression("ratinglabel", token);
            string strRatingLabel = xmlAccessoryRatingLabel != null
                ? string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync("Label_RatingFormat", token: token)
                                                     .ConfigureAwait(false),
                                await LanguageManager.GetStringAsync(xmlAccessoryRatingLabel.Value, token: token)
                                                     .ConfigureAwait(false))
                : await LanguageManager.GetStringAsync("Label_Rating", token: token).ConfigureAwait(false);
            await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token).ConfigureAwait(false);
            bool blnShowCost
                = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                                     .ConfigureAwait(false));
            await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);
            bool blnShowTest
                = !string.IsNullOrEmpty(await lblTest.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                                     .ConfigureAwait(false));
            await lblTestLabel.DoThreadSafeAsync(x => x.Visible = blnShowTest, token: token).ConfigureAwait(false);

            await chkBlackMarketDiscount.DoThreadSafeAsync(x =>
            {
                x.Enabled = _blnIsParentWeaponBlackMarketAllowed;
                if (!x.Checked)
                {
                    x.Checked = GlobalSettings.AssumeBlackMarket && _blnIsParentWeaponBlackMarketAllowed;
                }
                else if (!_blnIsParentWeaponBlackMarketAllowed)
                {
                    //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                    x.Checked = false;
                }
            }, token: token).ConfigureAwait(false);

            string strSource = xmlAccessory.SelectSingleNodeAndCacheExpression("source", token)?.Value
                               ?? await LanguageManager.GetStringAsync("String_Unknown", token: token)
                                                       .ConfigureAwait(false);
            string strPage
                = xmlAccessory.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? xmlAccessory.SelectSingleNodeAndCacheExpression("page", token)?.Value
                ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
            SourceString objSourceString = await SourceString
                                                 .GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                       GlobalSettings.CultureInfo, _objCharacter,
                                                                       token: token).ConfigureAwait(false);
            await objSourceString.SetControlAsync(lblSource, this, token: token).ConfigureAwait(false);
            await lblSourceLabel
                  .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSourceString.ToString()), token: token)
                  .ConfigureAwait(false);
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstAccessory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedAccessory = strSelectedId;
                _decMarkup = nudMarkup.Value;
                _blnFreeCost = chkFreeItem.Checked;
                _intSelectedRating = nudRating.Visible ? nudRating.ValueAsInt : 0;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        private async Task<decimal> ProcessInvariantXPathExpression(string strExpression, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            strExpression = strExpression.ProcessFixedValuesString(intRating);
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        if (_objParentWeapon != null)
                        {
                            Microsoft.VisualStudio.Threading.AsyncLazy<int> intParentRating = new Microsoft.VisualStudio.Threading.AsyncLazy<int>(() => _objParentWeapon.GetRatingAsync(token), Utils.JoinableTaskFactory);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                                async () => (await intParentRating.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "Parent Rating",
                                async () => (await intParentRating.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Weapon Rating}",
                                async () => (await intParentRating.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "Weapon Rating",
                                async () => (await intParentRating.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            Microsoft.VisualStudio.Threading.AsyncLazy<decimal> decParentCost = new Microsoft.VisualStudio.Threading.AsyncLazy<decimal>(() => _objParentWeapon.GetOwnCostAsync(token), Utils.JoinableTaskFactory);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Weapon Cost}",
                                async () => (await decParentCost.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "Weapon Cost",
                                async () => (await decParentCost.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            Microsoft.VisualStudio.Threading.AsyncLazy<decimal> decParentTotalCost = new Microsoft.VisualStudio.Threading.AsyncLazy<decimal>(() => _objParentWeapon.MultipliableCostAsync(null, token), Utils.JoinableTaskFactory);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Weapon Total Cost}",
                                async () => (await decParentTotalCost.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "Weapon Total Cost",
                                async () => (await decParentTotalCost.GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await _objParentWeapon.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            sbdValue.Replace("{Parent Rating}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo))
                                .Replace("Parent Rating", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo))
                                .Replace("{Weapon Rating}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo))
                                .Replace("Weapon Rating", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo))
                                .Replace("{Weapon Cost}", "0")
                                .Replace("Weapon Cost", "0")
                                .Replace("{Weapon Total Cost}", "0")
                                .Replace("Weapon Total Cost", "0");
                            Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                            await _objCharacter.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        }
                        sbdValue.Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                            .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        strExpression = sbdValue.ToString();
                    }
                }
                (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                .EvaluateInvariantXPathAsync(strExpression, token)
                                                                .ConfigureAwait(false);
                return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            }
            return decValue;
        }

        #endregion Methods
    }
}
