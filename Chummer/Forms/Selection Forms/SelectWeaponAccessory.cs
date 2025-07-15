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
        private int _intSelectedRating;

        private bool _blnLoading = true;
        private readonly List<string> _lstAllowedMounts = new List<string>();
        private Weapon _objParentWeapon;
        private bool _blnIsParentWeaponBlackMarketAllowed;
        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseChummerNode;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectWeaponAccessory(Character objCharacter)
        {
            Disposed += (sender, args) => Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            // Load the Weapon information.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("weapons.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseChummerNode));
        }

        private async void SelectWeaponAccessory_Load(object sender, EventArgs e)
        {
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
                    x.Text = string.Format(GlobalSettings.CultureInfo, x.Text,
                                           _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount).ConfigureAwait(false);

            _blnLoading = false;
            await RefreshList().ConfigureAwait(false);
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private async Task RefreshList(CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstAccessories))
            {
                // Populate the Accessory list.
                string strFilter = string.Empty;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false))
                             .Append(
                                 ") and (contains(mount, \"Internal\") or contains(mount, \"None\") or mount = \"\"");
                    foreach (string strAllowedMount in _lstAllowedMounts.Where(
                                 strAllowedMount => !string.IsNullOrEmpty(strAllowedMount)))
                    {
                        sbdFilter.Append(" or contains(mount, ").Append(strAllowedMount.CleanXPath()).Append(')');
                    }

                    sbdFilter.Append(')');
                    string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSearch))
                        sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                    if (sbdFilter.Length > 0)
                        strFilter = '[' + sbdFilter.ToString() + ']';
                }

                int intOverLimit = 0;
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
                    if (!await _objParentWeapon.CheckAccessoryRequirementsAsync(objXmlAccessory, token).ConfigureAwait(false))
                        continue;

                    decimal decCostMultiplier = decBaseCostMultiplier;
                    if (_blnIsParentWeaponBlackMarketAllowed)
                        decCostMultiplier *= 0.9m;
                    if (!blnHideOverAvailLimit || await objXmlAccessory.CheckAvailRestrictionAsync(_objCharacter, token: token).ConfigureAwait(false)
                        && (blnFreeItem || !blnShowOnlyAffordItems
                                        || await objXmlAccessory.CheckNuyenRestrictionAsync(
                                            decNuyen, decCostMultiplier, token: token).ConfigureAwait(false)))
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
            UpdateMountFields(true);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                await UpdateGearInfo(false).ConfigureAwait(false);
        }

        private async void cboExtraMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(false);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                await UpdateGearInfo(false).ConfigureAwait(false);
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
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
        public Tuple<string, string> SelectedMount => new Tuple<string, string>(cboMount.SelectedItem?.ToString(), cboExtraMount.SelectedItem?.ToString());

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
                XPathNavigator xmlThisWeaponDataNode
                    = _xmlBaseChummerNode.TryGetNodeById("weapons/weapon", objWeapon.SourceID);
                if (xmlThisWeaponDataNode != null)
                {
                    foreach (XPathNavigator objXmlMount in xmlThisWeaponDataNode.Select("accessorymounts/mount"))
                    {
                        string strLoopMount = objXmlMount.Value;
                        // Run through the Weapon's current Accessories and filter out any used up Mount points.
                        if (!await _objParentWeapon.WeaponAccessories.AnyAsync(objMod =>
                                objMod.Mount == strLoopMount
                                || objMod.ExtraMount == strLoopMount, token: token).ConfigureAwait(false))
                        {
                            _lstAllowedMounts.Add(strLoopMount);
                        }
                    }
                }

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
        public bool FreeCost => chkFreeItem.Checked;

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

            if (int.TryParse(xmlAccessory.SelectSingleNodeAndCacheExpression("rating", token)?.Value, out int intMaxRating) && intMaxRating > 0)
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
                                                                 _objCharacter, intMaximum, token: token)
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
                                                                 decNuyen, decCostMultiplier, intMaximum,
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

                lstMounts.Add("None");

                List<string> strAllowed = new List<string>(_lstAllowedMounts) {"None"};
                string strSelectedMount = await cboMount.DoThreadSafeFuncAsync(x =>
                {
                    x.Visible = true;
                    x.Items.Clear();
                    foreach (string strCurrentMount in lstMounts)
                    {
                        if (!string.IsNullOrEmpty(strCurrentMount))
                        {
                            foreach (string strAllowedMount in strAllowed)
                            {
                                if (strCurrentMount == strAllowedMount)
                                {
                                    x.Items.Add(strCurrentMount);
                                }
                            }
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

                lstExtraMounts.Add("None");

                bool blnShowExtraMountLabel = await cboExtraMount.DoThreadSafeFuncAsync(x =>
                {
                    x.Items.Clear();
                    foreach (string strCurrentMount in lstExtraMounts)
                    {
                        if (!string.IsNullOrEmpty(strCurrentMount))
                        {
                            foreach (string strAllowedMount in strAllowed)
                            {
                                if (strCurrentMount == strAllowedMount)
                                {
                                    x.Items.Add(strCurrentMount);
                                }
                            }
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
                = await new AvailabilityValue(intRating, xmlAccessory.SelectSingleNodeAndCacheExpression("avail", token)?.Value)
                    .ToStringAsync(token).ConfigureAwait(false);
            await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
            await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token)
                               .ConfigureAwait(false);

            string strNuyen = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
            {
                string strCost = "0";
                if (xmlAccessory.TryGetStringFieldQuickly("cost", ref strCost))
                {
                    strCost = (await strCost.CheapReplaceAsync("Weapon Cost",
                                                               async () => (await _objParentWeapon.GetOwnCostAsync(token).ConfigureAwait(false)).ToString(
                                                                   GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Weapon Total Cost",
                                                               async () => (await _objParentWeapon.MultipliableCostAsync(null, token).ConfigureAwait(false))
                                                                   .ToString(GlobalSettings.InvariantCultureInfo),
                                                               token: token).ConfigureAwait(false))
                        .Replace("Rating", intRating.ToString(GlobalSettings.CultureInfo));
                }

                if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decimal.TryParse(strValues[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out decMin);
                        decimal.TryParse(strValues[1], NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out decMax);
                    }
                    else
                    {
                        decimal.TryParse(strCost.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out decMin);
                    }

                    if (decMax == decimal.MaxValue)
                    {
                        await lblCost.DoThreadSafeAsync(
                                         x => x.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                       GlobalSettings.CultureInfo)
                                                       + strNuyen + '+',
                                         token: token)
                                     .ConfigureAwait(false);
                    }
                    else
                    {
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                               .ConfigureAwait(false);
                        await lblCost.DoThreadSafeAsync(
                                         x => x.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                       GlobalSettings.CultureInfo) + strSpace + '-'
                                                       + strSpace + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                           GlobalSettings.CultureInfo)
                                                       + strNuyen, token: token)
                                     .ConfigureAwait(false);
                    }

                    string strTest = await _objCharacter.AvailTestAsync(decMax, strAvail, token).ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                }
                else
                {
                    if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decCost))
                    {
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                       .EvaluateInvariantXPathAsync(strCost, token)
                                                                       .ConfigureAwait(false);
                        decCost = blnIsSuccess
                            ? Convert.ToDecimal((double)objProcess)
                            : 0;
                    }

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
                        string[] astrParentDoubledCostModificationSlots
                            = _objParentWeapon.DoubledCostModificationSlots.Split(
                                '/', StringSplitOptions.RemoveEmptyEntries);
                        if (astrParentDoubledCostModificationSlots.Contains(
                                await cboMount.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: token)
                                              .ConfigureAwait(false)) ||
                            astrParentDoubledCostModificationSlots.Contains(
                                await cboExtraMount.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: token)
                                                   .ConfigureAwait(false)))
                        {
                            decCost *= 2;
                        }
                    }

                    await lblCost
                          .DoThreadSafeAsync(
                              x => x.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat,
                                                             GlobalSettings.CultureInfo)
                                            + strNuyen, token: token)
                          .ConfigureAwait(false);
                    string strTest = await _objCharacter.AvailTestAsync(decCost, strAvail, token: token)
                                                        .ConfigureAwait(false);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                }
            }
            else
            {
                await lblCost
                      .DoThreadSafeAsync(
                          x => x.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                        + strNuyen, token: token)
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
            await objSourceString.SetControlAsync(lblSource, token: token).ConfigureAwait(false);
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

        #endregion Methods
    }
}
