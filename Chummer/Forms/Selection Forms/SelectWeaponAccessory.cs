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
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectWeaponAccessory(Character objCharacter)
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
                });
            }
            else
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(GlobalSettings.CultureInfo, x.Text,
                                           _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                });
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            _blnLoading = false;
            await RefreshList();
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private async ValueTask RefreshList()
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstAccessories))
            {
                // Populate the Accessory list.
                string strFilter = string.Empty;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath())
                             .Append(
                                 ") and (contains(mount, \"Internal\") or contains(mount, \"None\") or mount = \"\"");
                    foreach (string strAllowedMount in _lstAllowedMounts.Where(
                                 strAllowedMount => !string.IsNullOrEmpty(strAllowedMount)))
                    {
                        sbdFilter.Append(" or contains(mount, ").Append(strAllowedMount.CleanXPath()).Append(')');
                    }

                    sbdFilter.Append(')');
                    string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text);
                    if (!string.IsNullOrEmpty(strSearch))
                        sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                    if (sbdFilter.Length > 0)
                        strFilter = '[' + sbdFilter.ToString() + ']';
                }

                int intOverLimit = 0;
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked);
                decimal decBaseCostMultiplier = 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                foreach (XPathNavigator objXmlAccessory in _xmlBaseChummerNode.Select(
                             "accessories/accessory" + strFilter))
                {
                    string strId = (await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;
                    if (!_objParentWeapon.CheckAccessoryRequirements(objXmlAccessory))
                        continue;

                    decimal decCostMultiplier = decBaseCostMultiplier;
                    if (_blnIsParentWeaponBlackMarketAllowed)
                        decCostMultiplier *= 0.9m;
                    if (!blnHideOverAvailLimit || objXmlAccessory.CheckAvailRestriction(_objCharacter)
                        && (blnFreeItem || !blnShowOnlyAffordItems
                                        || objXmlAccessory.CheckNuyenRestriction(
                                            _objCharacter.Nuyen, decCostMultiplier)))
                    {
                        lstAccessories.Add(new ListItem(
                                               strId,
                                               (await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                               ?? (await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                               ?? await LanguageManager.GetStringAsync("String_Unknown")));
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
                                                        await LanguageManager.GetStringAsync("String_RestrictedItemsHidden"),
                                                        intOverLimit)));
                }

                string strOldSelected = await lstAccessory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                _blnLoading = true;
                await lstAccessory.PopulateWithListItemsAsync(lstAccessories);
                _blnLoading = false;
                await lstAccessory.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                });
            }
        }

        private async void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                await RefreshList();
            await UpdateGearInfo();
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private async void cboMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(true);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                await UpdateGearInfo(false);
        }

        private async void cboExtraMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(false);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                await UpdateGearInfo(false);
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
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
        /// GUID of the current weapon for which the accessory is being selected
        /// </summary>
        public Weapon ParentWeapon
        {
            set
            {
                _objParentWeapon = value;
                _lstAllowedMounts.Clear();
                if (value != null)
                {
                    foreach (XPathNavigator objXmlMount in _xmlBaseChummerNode.Select("weapons/weapon[id = " + value.SourceIDString.CleanXPath() + "]/accessorymounts/mount"))
                    {
                        string strLoopMount = objXmlMount.Value;
                        // Run through the Weapon's current Accessories and filter out any used up Mount points.
                        if (!_objParentWeapon.WeaponAccessories.Any(objMod =>
                            objMod.Mount == strLoopMount || objMod.ExtraMount == strLoopMount))
                        {
                            _lstAllowedMounts.Add(strLoopMount);
                        }
                    }
                }

                //TODO: Accessories don't use a category mapping, so we use parent weapon's category instead.
                if (_objCharacter.BlackMarketDiscount && value != null)
                {
                    string strCategory = value.GetNodeXPath()?.SelectSingleNode("category")?.Value ?? string.Empty;
                    _blnIsParentWeaponBlackMarketAllowed = !string.IsNullOrEmpty(strCategory) && _setBlackMarketMaps.Contains(strCategory);
                }
                else
                {
                    _blnIsParentWeaponBlackMarketAllowed = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
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
            await RefreshList();
        }

        private void UpdateMountFields(bool boolChangeExtraMountFirst)
        {
            if ((cboMount.SelectedItem.ToString() != "None") && cboExtraMount.SelectedItem != null && (cboExtraMount.SelectedItem.ToString() != "None")
                && (cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString()))
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

        private async ValueTask UpdateGearInfo(bool blnUpdateMountComboBoxes = true)
        {
            if (_blnLoading)
                return;

            XPathNavigator xmlAccessory = null;
            string strSelectedId = await lstAccessory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            // Retrieve the information for the selected Accessory.
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlAccessory = _xmlBaseChummerNode.SelectSingleNode("accessories/accessory[id = " + strSelectedId.CleanXPath() + ']');
            if (xmlAccessory == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                return;
            }

            string strRC = xmlAccessory.SelectSingleNode("rc")?.Value;
            if (!string.IsNullOrEmpty(strRC))
            {
                await lblRCLabel.DoThreadSafeAsync(x => x.Visible = true);
                await lblRC.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.Text = strRC;
                });
            }
            else
            {
                await lblRC.DoThreadSafeAsync(x => x.Visible = false);
                await lblRCLabel.DoThreadSafeAsync(x => x.Visible = false);
            }
            if (int.TryParse(xmlAccessory.SelectSingleNode("rating")?.Value, out int intMaxRating) && intMaxRating > 0)
            {
                await nudRating.DoThreadSafeAsync(x => x.Maximum = intMaxRating);
                if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked))
                {
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        while (x.Maximum > x.Minimum
                               && !xmlAccessory.CheckAvailRestriction(_objCharacter, x.MaximumAsInt))
                        {
                            --x.Maximum;
                        }
                    });
                }
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                {
                    decimal decCostMultiplier = 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                    if (_setBlackMarketMaps.Contains(xmlAccessory.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        while (x.Maximum > x.Minimum
                               && !xmlAccessory.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier,
                                                                      x.MaximumAsInt))
                        {
                            --x.Maximum;
                        }
                    });
                }

                await nudRating.DoThreadSafeAsync(x =>
                {
                    x.Enabled = nudRating.Maximum != nudRating.Minimum;
                    x.Visible = true;
                });
                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true);
                await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false);
            }
            else
            {
                await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true);
                await nudRating.DoThreadSafeAsync(x =>
                {
                    x.Enabled = false;
                    x.Visible = false;
                });
                await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true);
            }

            if (blnUpdateMountComboBoxes)
            {
                string strDataMounts = xmlAccessory.SelectSingleNode("mount")?.Value;
                List<string> lstMounts = new List<string>(1);
                if (!string.IsNullOrEmpty(strDataMounts))
                {
                    lstMounts.AddRange(strDataMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));
                }

                lstMounts.Add("None");

                List<string> strAllowed = new List<string>(_lstAllowedMounts) { "None" };
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
                });
                await lblMountLabel.DoThreadSafeAsync(x => x.Visible = true);

                List<string> lstExtraMounts = new List<string>(1);
                string strExtraMount = xmlAccessory.SelectSingleNode("extramount")?.Value;
                if (!string.IsNullOrEmpty(strExtraMount))
                {
                    lstExtraMounts.AddRange(strExtraMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));
                }

                lstExtraMounts.Add("None");

                await cboExtraMount.DoThreadSafeFuncAsync(x =>
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
                }).ContinueWith(y => lblExtraMountLabel.DoThreadSafeAsync(x => x.Visible = y.Result)).Unwrap();
            }

            int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt);

            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail
                = new AvailabilityValue(intRating, xmlAccessory.SelectSingleNode("avail")?.Value)
                    .ToString();
            await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail);
            await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail));

            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
            {
                string strCost = "0";
                if (xmlAccessory.TryGetStringFieldQuickly("cost", ref strCost))
                    strCost = (await strCost.CheapReplaceAsync("Weapon Cost",
                                () => _objParentWeapon.OwnCost.ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplaceAsync("Weapon Total Cost",
                                () => _objParentWeapon.MultipliableCost(null)
                                    .ToString(GlobalSettings.InvariantCultureInfo)))
                        .Replace("Rating", intRating.ToString(GlobalSettings.CultureInfo));
                if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decimal.TryParse(strValues[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                        decimal.TryParse(strValues[1], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
                    }
                    else
                        decimal.TryParse(strCost.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

                    if (decMax == decimal.MaxValue)
                    {
                        await lblCost.DoThreadSafeAsync(
                            x => x.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                          GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol") + '+');
                    }
                    else
                    {
                        string strSpace = await LanguageManager.GetStringAsync("String_Space");
                        await lblCost.DoThreadSafeAsync(
                            x => x.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                          GlobalSettings.CultureInfo) + strSpace + '-'
                                          + strSpace + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                                       GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"));
                    }

                    await lblTest.DoThreadSafeAsync(x => x.Text = _objCharacter.AvailTest(decMax, strAvail));
                }
                else
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                    decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) : 0;

                    // Apply any markup.
                    decCost *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);

                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked))
                        decCost *= 0.9m;
                    decCost *= _objParentWeapon.AccessoryMultiplier;
                    if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                    {
                        string[] astrParentDoubledCostModificationSlots = _objParentWeapon.DoubledCostModificationSlots.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (astrParentDoubledCostModificationSlots.Contains(await cboMount.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString())) ||
                            astrParentDoubledCostModificationSlots.Contains(await cboExtraMount.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString())))
                        {
                            decCost *= 2;
                        }
                    }

                    await lblCost.DoThreadSafeAsync(x => x.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"));
                    await lblTest.DoThreadSafeAsync(x => x.Text = _objCharacter.AvailTest(decCost, strAvail));
                }
            }
            else
            {
                await lblCost.DoThreadSafeAsync(x => x.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"));
                await lblTest.DoThreadSafeAsync(x => x.Text = _objCharacter.AvailTest(0, strAvail));
            }

            XPathNavigator xmlAccessoryRatingLabel = xmlAccessory.SelectSingleNode("ratinglabel");
            string strRatingLabel = xmlAccessoryRatingLabel != null
                ? string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Label_RatingFormat"),
                                await LanguageManager.GetStringAsync(xmlAccessoryRatingLabel.Value))
                : await LanguageManager.GetStringAsync("Label_Rating");
            await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel);
            await lblCost.DoThreadSafeFuncAsync(x => x.Text)
                   .ContinueWith(y => lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result)))
                   .Unwrap();
            await lblTest.DoThreadSafeFuncAsync(x => x.Text)
                         .ContinueWith(y => lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result)))
                         .Unwrap();

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
            });

            string strSource = xmlAccessory.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            string strPage = (await xmlAccessory.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? xmlAccessory.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
            await objSourceString.SetControlAsync(lblSource);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSourceString.ToString()));
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true);
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
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
