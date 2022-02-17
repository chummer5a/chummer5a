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

        private void SelectWeaponAccessory_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                chkHideOverAvailLimit.Text = string.Format(GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
            }

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            _blnLoading = false;
            RefreshList();
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private void RefreshList()
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
                    if (!string.IsNullOrEmpty(txtSearch.Text))
                        sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                    if (sbdFilter.Length > 0)
                        strFilter = '[' + sbdFilter.ToString() + ']';
                }

                int intOverLimit = 0;
                foreach (XPathNavigator objXmlAccessory in _xmlBaseChummerNode.Select(
                             "accessories/accessory" + strFilter))
                {
                    string strId = objXmlAccessory.SelectSingleNodeAndCacheExpression("id")?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;
                    if (!_objParentWeapon.CheckAccessoryRequirements(objXmlAccessory))
                        continue;

                    decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                    if (_blnIsParentWeaponBlackMarketAllowed)
                        decCostMultiplier *= 0.9m;
                    if (!chkHideOverAvailLimit.Checked || objXmlAccessory.CheckAvailRestriction(_objCharacter)
                        && (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked
                                                || objXmlAccessory.CheckNuyenRestriction(
                                                    _objCharacter.Nuyen, decCostMultiplier)))
                    {
                        lstAccessories.Add(new ListItem(
                                               strId,
                                               objXmlAccessory.SelectSingleNodeAndCacheExpression("translate")?.Value
                                               ?? objXmlAccessory.SelectSingleNodeAndCacheExpression("name")?.Value
                                               ?? LanguageManager.GetString("String_Unknown")));
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
                                                        LanguageManager.GetString("String_RestrictedItemsHidden"),
                                                        intOverLimit)));
                }

                string strOldSelected = lstAccessory.SelectedValue?.ToString();
                _blnLoading = true;
                lstAccessory.BeginUpdate();
                lstAccessory.PopulateWithListItems(lstAccessories);
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    lstAccessory.SelectedValue = strOldSelected;
                else
                    lstAccessory.SelectedIndex = -1;
                lstAccessory.EndUpdate();
            }
        }

        private void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
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

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
                RefreshList();
            UpdateGearInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                RefreshList();
            UpdateGearInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cboMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(true);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                UpdateGearInfo(false);
        }

        private void cboExtraMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(false);
            if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                UpdateGearInfo(false);
        }

        private void RefreshCurrentList(object sender, EventArgs e)
        {
            RefreshList();
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
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

        private void UpdateGearInfo(bool blnUpdateMountComboBoxes = true)
        {
            if (_blnLoading)
                return;

            XPathNavigator xmlAccessory = null;
            string strSelectedId = lstAccessory.SelectedValue?.ToString();
            // Retrieve the information for the selected Accessory.
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlAccessory = _xmlBaseChummerNode.SelectSingleNode("accessories/accessory[id = " + strSelectedId.CleanXPath() + ']');
            if (xmlAccessory == null)
            {
                tlpRight.Visible = false;
                return;
            }

            string strRC = xmlAccessory.SelectSingleNode("rc")?.Value;
            if (!string.IsNullOrEmpty(strRC))
            {
                lblRC.Visible = true;
                lblRCLabel.Visible = true;
                lblRC.Text = strRC;
            }
            else
            {
                lblRC.Visible = false;
                lblRCLabel.Visible = false;
            }
            if (int.TryParse(xmlAccessory.SelectSingleNode("rating")?.Value, out int intMaxRating) && intMaxRating > 0)
            {
                nudRating.Maximum = intMaxRating;
                if (chkHideOverAvailLimit.Checked)
                {
                    while (nudRating.Maximum > nudRating.Minimum && !xmlAccessory.CheckAvailRestriction(_objCharacter, nudRating.MaximumAsInt))
                    {
                        --nudRating.Maximum;
                    }
                }
                if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                {
                    decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains(xmlAccessory.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    while (nudRating.Maximum > nudRating.Minimum && !xmlAccessory.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                    {
                        --nudRating.Maximum;
                    }
                }
                nudRating.Enabled = nudRating.Maximum != nudRating.Minimum;
                nudRating.Visible = true;
                lblRatingLabel.Visible = true;
                lblRatingNALabel.Visible = false;
            }
            else
            {
                lblRatingNALabel.Visible = true;
                nudRating.Enabled = false;
                nudRating.Visible = false;
                lblRatingLabel.Visible = true;
            }

            if (blnUpdateMountComboBoxes)
            {
                string strDataMounts = xmlAccessory.SelectSingleNode("mount")?.Value;
                List<string> strMounts = new List<string>(1);
                if (!string.IsNullOrEmpty(strDataMounts))
                {
                    strMounts.AddRange(strDataMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));
                }

                strMounts.Add("None");

                List<string> strAllowed = new List<string>(_lstAllowedMounts) { "None" };
                cboMount.Visible = true;
                cboMount.Items.Clear();
                foreach (string strCurrentMount in strMounts)
                {
                    if (!string.IsNullOrEmpty(strCurrentMount))
                    {
                        foreach (string strAllowedMount in strAllowed)
                        {
                            if (strCurrentMount == strAllowedMount)
                            {
                                cboMount.Items.Add(strCurrentMount);
                            }
                        }
                    }
                }

                cboMount.Enabled = cboMount.Items.Count > 1;
                cboMount.SelectedIndex = 0;
                lblMountLabel.Visible = true;

                List<string> strExtraMounts = new List<string>(1);
                string strExtraMount = xmlAccessory.SelectSingleNode("extramount")?.Value;
                if (!string.IsNullOrEmpty(strExtraMount))
                {
                    foreach (string strItem in strExtraMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        strExtraMounts.Add(strItem);
                    }
                }

                strExtraMounts.Add("None");

                cboExtraMount.Items.Clear();
                foreach (string strCurrentMount in strExtraMounts)
                {
                    if (!string.IsNullOrEmpty(strCurrentMount))
                    {
                        foreach (string strAllowedMount in strAllowed)
                        {
                            if (strCurrentMount == strAllowedMount)
                            {
                                cboExtraMount.Items.Add(strCurrentMount);
                            }
                        }
                    }
                }

                cboExtraMount.Enabled = cboExtraMount.Items.Count > 1;
                cboExtraMount.SelectedIndex = 0;
                if (cboMount.SelectedItem.ToString() != "None" && cboExtraMount.SelectedItem.ToString() != "None"
                                                               && cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString())
                    ++cboExtraMount.SelectedIndex;
                cboExtraMount.Visible = cboExtraMount.Enabled && cboExtraMount.SelectedItem.ToString() != "None";
                lblExtraMountLabel.Visible = cboExtraMount.Visible;
            }

            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            lblAvail.Text = new AvailabilityValue(Convert.ToInt32(nudRating.Value), xmlAccessory.SelectSingleNode("avail")?.Value).ToString();
            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

            if (!chkFreeItem.Checked)
            {
                string strCost = "0";
                if (xmlAccessory.TryGetStringFieldQuickly("cost", ref strCost))
                    strCost = strCost.CheapReplace("Weapon Cost", () => _objParentWeapon.OwnCost.ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("Weapon Total Cost", () => _objParentWeapon.MultipliableCost(null).ToString(GlobalSettings.InvariantCultureInfo))
                        .Replace("Rating", nudRating.Value.ToString(GlobalSettings.CultureInfo));
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
                        lblCost.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+";
                    }
                    else
                    {
                        string strSpace = LanguageManager.GetString("String_Space");
                        lblCost.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + strSpace + '-'
                                       + strSpace + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                    }

                    lblTest.Text = _objCharacter.AvailTest(decMax, lblAvail.Text);
                }
                else
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                    decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) : 0;

                    // Apply any markup.
                    decCost *= 1 + (nudMarkup.Value / 100.0m);

                    if (chkBlackMarketDiscount.Checked)
                        decCost *= 0.9m;
                    decCost *= _objParentWeapon.AccessoryMultiplier;
                    if (!string.IsNullOrEmpty(_objParentWeapon.DoubledCostModificationSlots))
                    {
                        string[] astrParentDoubledCostModificationSlots = _objParentWeapon.DoubledCostModificationSlots.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (astrParentDoubledCostModificationSlots.Contains(cboMount.SelectedItem?.ToString()) ||
                            astrParentDoubledCostModificationSlots.Contains(cboExtraMount.SelectedItem?.ToString()))
                        {
                            decCost *= 2;
                        }
                    }

                    lblCost.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                    lblTest.Text = _objCharacter.AvailTest(decCost, lblAvail.Text);
                }
            }
            else
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                lblTest.Text = _objCharacter.AvailTest(0, lblAvail.Text);
            }

            lblRatingLabel.Text = xmlAccessory.SelectSingleNode("ratinglabel") != null
                ? string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(xmlAccessory.SelectSingleNode("ratinglabel").Value))
                : LanguageManager.GetString("Label_Rating");
            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
            lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);

            chkBlackMarketDiscount.Enabled = _blnIsParentWeaponBlackMarketAllowed;
            if (!chkBlackMarketDiscount.Checked)
            {
                chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && _blnIsParentWeaponBlackMarketAllowed;
            }
            else if (!_blnIsParentWeaponBlackMarketAllowed)
            {
                //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                chkBlackMarketDiscount.Checked = false;
            }

            string strSource = xmlAccessory.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
            string strPage = xmlAccessory.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? xmlAccessory.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
            SourceString objSourceString = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
            objSourceString.SetControl(lblSource);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            tlpRight.Visible = true;
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
