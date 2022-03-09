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
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectGear : Form
    {
        private bool _blnLoading = true;
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

        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private readonly HashSet<string> _setAllowedCategories = Utils.StringHashSetPool.Get();
        private readonly HashSet<string> _setAllowedNames = Utils.StringHashSetPool.Get();
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectGear(Character objCharacter, int intAvailModifier = 0, int intCostMultiplier = 1, object objGearParent = null, string strAllowedCategories = "", string strAllowedNames = "")
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _intAvailModifier = intAvailModifier;
            _intCostMultiplier = intCostMultiplier;
            _objCharacter = objCharacter;
            _objGearParent = objGearParent;
            _objParentNode = (_objGearParent as IHasXmlDataNode)?.GetNodeXPath();
            // Stack Checkbox is only available in Career Mode.
            if (!_objCharacter.Created)
            {
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

            XPathNodeIterator objXmlCategoryList;

            // Populate the Gear Category list.
            if (_setAllowedCategories.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMount))
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
                objXmlCategoryList = await _xmlBaseGearDataNode.SelectAndCacheExpressionAsync("categories/category");
            }

            foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
            {
                string strCategory = objXmlCategory.Value;
                // Make sure the Category isn't in the exclusion list.
                if (!_setAllowedCategories.Contains(strCategory) && (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@show"))?.Value == bool.FalseString)
                {
                    continue;
                }
                if (_lstCategory.All(x => x.Value.ToString() != strCategory) && await AnyItemInList(strCategory))
                {
                    _lstCategory.Add(new ListItem(strCategory, (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? strCategory));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            cboCategory.EndUpdate();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                txtSearch.Text = DefaultSearchText;
                txtSearch.Enabled = false;
            }

            _blnLoading = false;
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedValue = _strSelectCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;
            else
                await RefreshList();

            if (!string.IsNullOrEmpty(_strSelectedGear))
                lstGear.SelectedValue = _strSelectedGear;
            // Make sure right-side controls are properly updated depending on how the selections above worked out
            await UpdateGearInfo();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            // Show the Do It Yourself CheckBox if the Commlink Upgrade category is selected.
            if (cboCategory.SelectedValue?.ToString() == "Commlink Upgrade")
                chkDoItYourself.Visible = true;
            else
            {
                chkDoItYourself.Visible = false;
                chkDoItYourself.Checked = false;
            }

            await RefreshList();
        }

        private async void lstGear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstGear.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Gear.
                XPathNavigator objXmlGear = _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = " + strSelectedId.CleanXPath() + ']');

                if (objXmlGear != null)
                {
                    string strName = objXmlGear.SelectSingleNode("name")?.Value ?? string.Empty;

                    // Quantity.
                    nudGearQty.Enabled = true;
                    nudGearQty.Minimum = 1;
                    string strCostFor = objXmlGear.SelectSingleNode("costfor")?.Value;
                    if (!string.IsNullOrEmpty(strCostFor))
                    {
                        nudGearQty.Value = Convert.ToDecimal(strCostFor, GlobalSettings.InvariantCultureInfo);
                        nudGearQty.Increment = Convert.ToDecimal(strCostFor, GlobalSettings.InvariantCultureInfo);
                    }
                    else
                    {
                        nudGearQty.Value = 1;
                        nudGearQty.Increment = 1;
                    }
                    if (strName.StartsWith("Nuyen", StringComparison.Ordinal))
                    {
                        int intDecimalPlaces = _objCharacter.Settings.MaxNuyenDecimals;
                        if (intDecimalPlaces <= 0)
                        {
                            nudGearQty.DecimalPlaces = 0;
                            nudGearQty.Minimum = 1.0m;
                        }
                        else
                        {
                            nudGearQty.DecimalPlaces = intDecimalPlaces;
                            decimal decMinimum = 1.0m;
                            // Need a for loop instead of a power system to maintain exact precision
                            for (int i = 0; i < intDecimalPlaces; ++i)
                                decMinimum /= 10.0m;
                            nudGearQty.Minimum = decMinimum;
                        }
                    }
                    else if (objXmlGear.SelectSingleNode("category")?.Value == "Currency")
                    {
                        nudGearQty.DecimalPlaces = 2;
                        nudGearQty.Minimum = 0.01m;
                    }
                    else
                    {
                        nudGearQty.DecimalPlaces = 0;
                        nudGearQty.Minimum = 1.0m;
                    }

                    nudGearQty.Visible = true;
                    lblGearQtyLabel.Visible = true;
                    chkStack.Visible = _objCharacter.Created;

                    string strRatingLabel = objXmlGear.SelectSingleNode("ratinglabel")?.Value;
                    lblRatingLabel.Text = !string.IsNullOrEmpty(strRatingLabel)
                        ? string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Label_RatingFormat"),
                            await LanguageManager.GetStringAsync(strRatingLabel))
                        : await LanguageManager.GetStringAsync("Label_Rating");
                }
                else
                {
                    nudGearQty.Visible = false;
                    nudGearQty.Enabled = false;
                    nudGearQty.Value = 1;
                    lblGearQtyLabel.Visible = false;
                    chkStack.Visible = false;
                }
            }
            else
            {
                nudGearQty.Visible = false;
                nudGearQty.Enabled = false;
                nudGearQty.Value = 1;
                lblGearQtyLabel.Visible = false;
                chkStack.Visible = false;
            }

            await UpdateGearInfo();
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private async void nudGearQty_ValueChanged(object sender, EventArgs e)
        {
            await UpdateGearInfo();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                await RefreshList();
            }
            await UpdateGearInfo();
        }

        private async void chkDoItYourself_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                await RefreshList();
            }
            await UpdateGearInfo();
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                await RefreshList();
            }
            await UpdateGearInfo();
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList();
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
                case Keys.Up when lstGear.SelectedIndex - 1 >= 0:
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
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
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
            set
            {
                _decMaximumCapacity = value;
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed") + LanguageManager.GetString("String_Space") + _decMaximumCapacity.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Whether or not the item's cost should be cut in half for being a Do It Yourself component/upgrade.
        /// </summary>
        public bool DoItYourself => chkDoItYourself.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Whether or not the Gear should stack with others if possible.
        /// </summary>
        public bool Stack => chkStack.Checked;

        /// <summary>
        /// Whether or not the Stack Checkbox should be shown (default true).
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
        /// Whether or not the selected Vehicle is used.
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
        private async ValueTask UpdateGearInfo()
        {
            if (_blnLoading)
            {
                tlpRight.Visible = false;
                return;
            }

            string strSelectedId = lstGear.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
            {
                tlpRight.Visible = false;
                return;
            }

            // Retrieve the information for the selected piece of Gear.
            XPathNavigator objXmlGear = _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = " + strSelectedId.CleanXPath() + ']');

            if (objXmlGear == null)
            {
                tlpRight.Visible = false;
                return;
            }

            SuspendLayout();
            // Retrieve the information for the selected piece of Cyberware.
            string strDeviceRating = objXmlGear.SelectSingleNode("devicerating")?.Value ?? string.Empty;
            lblGearDeviceRating.Text = strDeviceRating;
            lblGearDeviceRatingLabel.Visible = !string.IsNullOrEmpty(strDeviceRating);

            string strSource = objXmlGear.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            string strPage = (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? objXmlGear.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                             GlobalSettings.CultureInfo, _objCharacter);
            lblSource.Text = objSource.ToString();
            lblSource.SetToolTip(objSource.LanguageBookTooltip);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            lblAvail.Text = new AvailabilityValue(Convert.ToInt32(nudRating.Value), objXmlGear.SelectSingleNode("avail")?.Value).ToString();
            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

            decimal decMultiplier = nudGearQty.Value / nudGearQty.Increment;
            if (chkDoItYourself.Checked)
                decMultiplier *= 0.5m;

            // Cost.
            bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(objXmlGear.SelectSingleNode("category")?.Value);
            chkBlackMarketDiscount.Enabled = blnCanBlackMarketDiscount;
            if (!chkBlackMarketDiscount.Checked)
            {
                chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
            }
            else if (!blnCanBlackMarketDiscount)
            {
                //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                chkBlackMarketDiscount.Checked = false;
            }

            decimal decItemCost = 0.0m;
            bool blnIsSuccess;
            object objProcess;
            if (chkFreeItem.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            }
            else
            {
                XPathNavigator objCostNode = objXmlGear.SelectSingleNode("cost");
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
                    for (int i = nudRating.ValueAsInt; i <= intHighestCostNode; ++i)
                    {
                        XPathNavigator objLoopNode = objXmlGear.SelectSingleNode("cost" + i.ToString(GlobalSettings.InvariantCultureInfo));
                        if (objLoopNode != null)
                        {
                            objCostNode = objLoopNode;
                            break;
                        }
                    }
                }
                if (objCostNode != null)
                {
                    try
                    {
                        objProcess = CommonFunctions.EvaluateInvariantXPath(objCostNode.Value.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out blnIsSuccess);
                        decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) * decMultiplier : 0;
                        decCost *= 1 + (nudMarkup.Value / 100.0m);
                        if (chkBlackMarketDiscount.Checked)
                            decCost *= 0.9m;
                        lblCost.Text = (decCost * _intCostMultiplier).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        decItemCost = decCost;
                    }
                    catch (XPathException)
                    {
                        lblCost.Text = objCostNode.Value;
                        if (decimal.TryParse(objCostNode.Value, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decTemp))
                        {
                            decItemCost = decTemp;
                            lblCost.Text = (decItemCost * _intCostMultiplier).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        }
                    }

                    if (objCostNode.Value.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string strCost = "0";
                        if (nudRating.Value > 0)
                        {
                            strCost = objCostNode.Value.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                 .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .ElementAt(nudRating.ValueAsInt - 1).Trim('[', ']');
                        }

                        decimal decCost = Convert.ToDecimal(strCost, GlobalSettings.InvariantCultureInfo) * decMultiplier;
                        decCost *= 1 + (nudMarkup.Value / 100.0m);
                        if (chkBlackMarketDiscount.Checked)
                            decCost *= 0.9m;
                        lblCost.Text = (decCost * _intCostMultiplier).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+";
                        decItemCost = decCost;
                    }
                    else if (objCostNode.Value.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        string strCost = objCostNode.Value.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        if (decMax == decimal.MaxValue)
                            lblCost.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+";
                        else
                        {
                            string strSpace = await LanguageManager.GetStringAsync("String_Space");
                            lblCost.Text = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                           + strSpace + '-' + strSpace
                                           + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        }

                        decItemCost = decMin;
                    }
                }
            }
            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);

            // Update the Avail Test Label.
            lblTest.Text = _objCharacter.AvailTest(decItemCost * _intCostMultiplier, lblAvail.Text);
            lblTestLabel.Visible = true;

            // Capacity.

            if (_eCapacityStyle == CapacityStyle.Zero)
                lblCapacity.Text = '[' + 0.ToString(GlobalSettings.CultureInfo) + ']';
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
                        string strSecondHalf = strCapacityText.Substring(intPos + 1, strCapacityText.Length - intPos - 1);

                        if (strFirstHalf == "[*]")
                            lblCapacity.Text = "*";
                        else
                        {
                            bool blnSquareBrackets = strFirstHalf.StartsWith('[');
                            strCapacity = strFirstHalf;
                            if (blnSquareBrackets && strCapacity.Length > 2)
                                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                            if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                            {
                                string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                                if (strValues.Length >= nudRating.ValueAsInt)
                                    lblCapacity.Text = strValues[nudRating.ValueAsInt - 1];
                                else
                                {
                                    try
                                    {
                                        objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out blnIsSuccess);
                                        lblCapacity.Text = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo) : strCapacity;
                                    }
                                    catch (XPathException)
                                    {
                                        lblCapacity.Text = strCapacity;
                                    }
                                    catch (OverflowException) // Result is text and not a double
                                    {
                                        lblCapacity.Text = strCapacity;
                                    }
                                    catch (InvalidCastException) // Result is text and not a double
                                    {
                                        lblCapacity.Text = strCapacity;
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out blnIsSuccess);
                                    lblCapacity.Text = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo) : strCapacity;
                                }
                                catch (XPathException)
                                {
                                    lblCapacity.Text = strCapacity;
                                }
                                catch (OverflowException) // Result is text and not a double
                                {
                                    lblCapacity.Text = strCapacity;
                                }
                                catch (InvalidCastException) // Result is text and not a double
                                {
                                    lblCapacity.Text = strCapacity;
                                }
                            }

                            if (blnSquareBrackets)
                                lblCapacity.Text = '[' + lblCapacity.Text + ']';
                        }

                        lblCapacity.Text += '/' + strSecondHalf;
                    }
                    else if (strCapacityText == "[*]")
                        lblCapacity.Text = "*";
                    else
                    {
                        bool blnSquareBrackets = strCapacityText.StartsWith('[');
                        strCapacity = strCapacityText;
                        if (blnSquareBrackets && strCapacity.Length > 2)
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacityText.StartsWith("FixedValues(", StringComparison.Ordinal))
                        {
                            string[] strValues = strCapacityText.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                            lblCapacity.Text = strValues[Math.Max(Math.Min(nudRating.ValueAsInt, strValues.Length) - 1, 0)];
                        }
                        else
                        {
                            try
                            {
                                objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out blnIsSuccess);
                                lblCapacity.Text = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo) : strCapacity;
                            }
                            catch (OverflowException) // Result is text and not a double
                            {
                                lblCapacity.Text = strCapacity;
                            }
                            catch (InvalidCastException) // Result is text and not a double
                            {
                                lblCapacity.Text = strCapacity;
                            }
                        }
                        if (blnSquareBrackets)
                            lblCapacity.Text = '[' + lblCapacity.Text + ']';
                    }
                }
                else
                {
                    lblCapacity.Text = 0.ToString(GlobalSettings.CultureInfo);
                }
            }
            lblCapacityLabel.Visible = !string.IsNullOrEmpty(lblCapacity.Text);

            // Rating.
            string strExpression = objXmlGear.SelectSingleNode("rating")?.Value ?? string.Empty;
            if (strExpression == "0")
                strExpression = string.Empty;
            int intRating = int.MaxValue;
            if (!string.IsNullOrEmpty(strExpression))
            {
                if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strExpression = strValues[Math.Max(Math.Min(nudRating.ValueAsInt, strValues.Length) - 1, 0)].Trim('[', ']');
                }

                if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                {
                    using (new FetchSafelyFromPool<StringBuilder>(
                               Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        sbdValue.Replace(
                            "{Rating}", nudRating.ValueAsInt.ToString(GlobalSettings.InvariantCultureInfo));
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                            () => (_objGearParent as IHasRating)?.Rating.ToString(
                                      GlobalSettings.InvariantCultureInfo)
                                  ?? int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        objProcess = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString(), out blnIsSuccess);
                        intRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                    }
                }
                else if (!int.TryParse(strExpression, out intRating))
                    intRating = 0;
            }

            if (intRating > 0 && intRating != int.MaxValue)
            {
                nudRating.Maximum = intRating;
                XPathNavigator xmlMinRatingNode = objXmlGear.SelectSingleNode("minrating");
                if (xmlMinRatingNode != null)
                {
                    decimal decOldMinimum = nudRating.Minimum;
                    strExpression = xmlMinRatingNode.Value;
                    int intMinimumRating = 0;
                    if (!string.IsNullOrEmpty(strExpression))
                    {
                        if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                        {
                            string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                            strExpression = strValues[Math.Max(Math.Min(nudRating.ValueAsInt, strValues.Length) - 1, 0)].Trim('[', ']');
                        }

                        if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                        {
                            using (new FetchSafelyFromPool<StringBuilder>(
                                       Utils.StringBuilderPool, out StringBuilder sbdValue))
                            {
                                sbdValue.Append(strExpression);
                                sbdValue.Replace(
                                    "{Rating}", nudRating.ValueAsInt.ToString(GlobalSettings.InvariantCultureInfo));
                                await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                                    () => (_objGearParent as IHasRating)?.Rating.ToString(
                                        GlobalSettings.InvariantCultureInfo) ?? "0");
                                _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                                objProcess = CommonFunctions.EvaluateInvariantXPath(
                                    sbdValue.ToString(), out blnIsSuccess);
                                intMinimumRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                            }
                        }
                        else if (!int.TryParse(strExpression, out intMinimumRating))
                            intMinimumRating = 0;
                    }
                    nudRating.Minimum = intMinimumRating;
                    if (decOldMinimum > nudRating.Minimum)
                    {
                        nudRating.Value -= decOldMinimum - nudRating.Minimum;
                    }
                }
                else
                {
                    nudRating.Minimum = 1;
                }
                if (chkHideOverAvailLimit.Checked)
                {
                    while (nudRating.Maximum > nudRating.Minimum && !objXmlGear.CheckAvailRestriction(_objCharacter, nudRating.MaximumAsInt, _intAvailModifier))
                    {
                        --nudRating.Maximum;
                    }
                }

                if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                {
                    decimal decCostMultiplier = nudGearQty.Value / nudGearQty.Increment;
                    if (chkDoItYourself.Checked)
                        decCostMultiplier *= 0.5m;
                    decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains(objXmlGear.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    while (nudRating.Maximum > nudRating.Minimum && !objXmlGear.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                    {
                        --nudRating.Maximum;
                    }
                }

                lblRatingLabel.Visible = true;
                nudRating.Enabled = nudRating.Minimum != nudRating.Maximum;
                nudRating.Visible = true;
                lblRatingNALabel.Visible = false;
            }
            else
            {
                lblRatingLabel.Visible = true;
                lblRatingNALabel.Visible = true;
                nudRating.Minimum = 0;
                nudRating.Maximum = 0;
                nudRating.Enabled = false;
                nudRating.Visible = false;
            }
            tlpRight.Visible = true;
            ResumeLayout();
        }

        private ValueTask<bool> AnyItemInList(string strCategory = "")
        {
            return RefreshList(strCategory, false);
        }

        private ValueTask<bool> RefreshList(string strCategory = "")
        {
            return RefreshList(strCategory, true);
        }

        private async ValueTask<bool> RefreshList(string strCategory, bool blnDoUIUpdate)
        {
            bool blnAnyItem = false;
            if (string.IsNullOrEmpty(strCategory))
                strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');

                // Only add in category filter if we either are not searching or we have the option set to only search in categories
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || txtSearch.TextLength == 0))
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                else if (_setAllowedCategories.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value))
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
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
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
                if (!string.IsNullOrEmpty(txtSearch.Text))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            int intOverLimit = 0;
            List<ListItem> lstGears = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                foreach (XPathNavigator objXmlGear in _xmlBaseGearDataNode.Select("gears/gear" + strFilter))
                {
                    XPathNavigator xmlTestNode
                        = await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("forbidden/parentdetails");
                    if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("required/parentdetails");
                    if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("forbidden/geardetails");
                    if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("required/geardetails");
                    if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    if (!objXmlGear.RequirementsMet(_objCharacter))
                        continue;

                    if (!blnDoUIUpdate)
                    {
                        blnAnyItem = true;
                        break;
                    }

                    decimal decCostMultiplier = nudGearQty.Value / nudGearQty.Increment;
                    if (chkDoItYourself.Checked)
                        decCostMultiplier *= 0.5m;
                    decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains((await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("category"))?.Value))
                        decCostMultiplier *= 0.9m;
                    if (!chkHideOverAvailLimit.Checked
                              || objXmlGear.CheckAvailRestriction(_objCharacter, 1, _intAvailModifier)
                              && (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked
                                                      || objXmlGear.CheckNuyenRestriction(
                                                          _objCharacter.Nuyen, decCostMultiplier)))
                    {
                        blnAnyItem = true;
                        string strDisplayName = (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                ?? (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                                ?? await LanguageManager.GetStringAsync("String_Unknown");
                        lstGears.Add(new ListItem(
                                         (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value ?? string.Empty,
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
                        string strSelectCategory = cboCategory.SelectedValue?.ToString();
                        if (!GlobalSettings.SearchInCategoryOnly || string.IsNullOrEmpty(strSelectCategory) ||
                            strSelectCategory == "Show All")
                        {
                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string> setDuplicateNames))
                            {
                                for (int i = 0; i < lstGears.Count - 1; ++i)
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
                                    string strSpace = await LanguageManager.GetStringAsync("String_Space");
                                    for (int i = 0; i < lstGears.Count; ++i)
                                    {
                                        ListItem objLoopItem = lstGears[i];
                                        if (!setDuplicateNames.Contains(objLoopItem.Name))
                                            continue;
                                        XPathNavigator objXmlGear
                                            = _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = " +
                                                objLoopItem.Value.ToString().CleanXPath() +
                                                "]");
                                        if (objXmlGear == null)
                                            continue;
                                        string strLoopCategory
                                            = (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("category"))?.Value;
                                        if (string.IsNullOrEmpty(strLoopCategory))
                                            continue;
                                        ListItem objFoundItem
                                            = _lstCategory.Find(objFind => objFind.Value.ToString() == strLoopCategory);
                                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                                        {
                                            lstGears[i] = new ListItem(objLoopItem.Value
                                                                       , objLoopItem.Name + strSpace + '[' + objFoundItem.Name + ']');
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
                                                                    "String_RestrictedItemsHidden"),
                                                                intOverLimit)));
                    }

                    lstGear.BeginUpdate();
                    string strOldSelected = lstGear.SelectedValue?.ToString();
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    lstGear.PopulateWithListItems(lstGears);
                    _blnLoading = blnOldLoading;
                    if (string.IsNullOrEmpty(strOldSelected))
                        lstGear.SelectedIndex = -1;
                    else
                        lstGear.SelectedValue = strOldSelected;
                    lstGear.EndUpdate();
                }
            }
            finally
            {
                if (lstGears != null)
                    Utils.ListItemListPool.Return(lstGears);
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
                _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0)
                    ? cboCategory.SelectedValue?.ToString()
                    : _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = " + strSelectedId.CleanXPath() + "]/category")?.Value;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                _intSelectedRating = nudRating.ValueAsInt;
                _decSelectedQty = nudGearQty.Value;
                _decMarkup = nudMarkup.Value;

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
