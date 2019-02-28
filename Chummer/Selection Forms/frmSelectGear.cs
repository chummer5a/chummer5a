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
 using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using System.Text;
using System.Globalization;

namespace Chummer
{
    public partial class frmSelectGear : Form
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
        private static string s_StrSelectCategory = string.Empty;
        private bool _blnShowPositiveCapacityOnly;
        private bool _blnShowNegativeCapacityOnly;
        private bool _blnBlackMarketDiscount;
        private CapacityStyle _eCapacityStyle = CapacityStyle.Standard;

        private readonly XPathNavigator _xmlBaseGearDataNode;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setAllowedCategories = new HashSet<string>();
        private readonly HashSet<string> _setBlackMarketMaps;

        #region Control Events
        public frmSelectGear(Character objCharacter, int intAvailModifier = 0, int intCostMultiplier = 1, object objGearParent = null, string strAllowedCategories = "")
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _intAvailModifier = intAvailModifier;
            _intCostMultiplier = intCostMultiplier;
            _objCharacter = objCharacter;
            _objGearParent = objGearParent;
            _objParentNode = (_objGearParent as IHasXmlNode)?.GetNode()?.CreateNavigator();
            // Stack Checkbox is only available in Career Mode.
            if (!_objCharacter.Created)
            {
                chkStack.Checked = false;
                chkStack.Visible = false;
            }
            
            // Load the Gear information.
            _xmlBaseGearDataNode = XmlManager.Load("gear.xml").GetFastNavigator().SelectSingleNode("/chummer");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseGearDataNode);
            foreach (string strCategory in strAllowedCategories.TrimEndOnce(',').Split(','))
            {
                string strLoop = strCategory.Trim();
                if (!string.IsNullOrEmpty(strLoop))
                    _setAllowedCategories.Add(strLoop);
            }
        }

        private void frmSelectGear_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                chkHideOverAvailLimit.Text = string.Format(chkHideOverAvailLimit.Text, _objCharacter.MaximumAvailability.ToString(GlobalOptions.CultureInfo));
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }

            XPathNodeIterator objXmlCategoryList;

            // Populate the Gear Category list.
            if (_setAllowedCategories.Count > 0)
            {
                StringBuilder strMount = new StringBuilder();
                foreach (string strAllowedMount in _setAllowedCategories)
                {
                    if (!string.IsNullOrEmpty(strAllowedMount))
                        strMount.Append(". = \"" + strAllowedMount + "\" or ");
                }
                strMount.Append(". = \"General\"");
                objXmlCategoryList = _xmlBaseGearDataNode.Select("categories/category[" + strMount.ToString() + "]");
            }
            else
            {
                objXmlCategoryList = _xmlBaseGearDataNode.Select("categories/category");
            }

            foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
            {
                string strCategory = objXmlCategory.Value;
                // Make sure the Category isn't in the exclusion list.
                if (!_setAllowedCategories.Contains(strCategory) && objXmlCategory.SelectSingleNode("@show")?.Value == bool.FalseString)
                {
                    continue;
                }
                if (_lstCategory.All(x => x.Value.ToString() != strCategory) && RefreshList(strCategory, false, true).Count > 0)
                {
                    string strInnerText = strCategory;
                    _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strCategory));
                }
            }
            
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            cboCategory.EndUpdate();

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                txtSearch.Text = DefaultSearchText;
                txtSearch.Enabled = false;
            }

            _blnLoading = false;
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedValue = s_StrSelectCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;
            else
                RefreshList(cboCategory.SelectedValue?.ToString());

            if (!string.IsNullOrEmpty(_strSelectedGear))
                lstGear.SelectedValue = _strSelectedGear;
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedCategory = cboCategory.SelectedValue?.ToString();

            // Show the Do It Yourself CheckBox if the Commlink Upgrade category is selected.
            if (strSelectedCategory == "Commlink Upgrade")
                chkDoItYourself.Visible = true;
            else
            {
                chkDoItYourself.Visible = false;
                chkDoItYourself.Checked = false;
            }

            RefreshList(strSelectedCategory);
        }

        private void lstGear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstGear.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected piece of Gear.
                XPathNavigator objXmlGear = _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = \"" + strSelectedId + "\"]");

                if (objXmlGear != null)
                {
                    string strName = objXmlGear.SelectSingleNode("name")?.Value ?? string.Empty;

                    // Quantity.
                    nudGearQty.Enabled = true;
                    nudGearQty.Minimum = 1;
                    string strCostFor = objXmlGear.SelectSingleNode("costfor")?.Value;
                    if (!string.IsNullOrEmpty(strCostFor))
                    {
                        nudGearQty.Value = Convert.ToDecimal(strCostFor, GlobalOptions.InvariantCultureInfo);
                        nudGearQty.Increment = Convert.ToDecimal(strCostFor, GlobalOptions.InvariantCultureInfo);
                    }
                    else
                    {
                        nudGearQty.Value = 1;
                        nudGearQty.Increment = 1;
                    }
                    if (strName.StartsWith("Nuyen"))
                    {
                        int intDecimalPlaces = _objCharacter.Options.NuyenDecimals;
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

            UpdateGearInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList(cboCategory.SelectedValue?.ToString());
        }

        private void lstGear_DoubleClick(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void nudGearQty_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList(cboCategory.SelectedValue?.ToString());
            }
            UpdateGearInfo();
        }

        private void chkDoItYourself_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                RefreshList(cboCategory.SelectedValue?.ToString());
            }
            UpdateGearInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                RefreshList(cboCategory.SelectedValue?.ToString());
            }
            UpdateGearInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstGear.SelectedIndex + 1 < lstGear.Items.Count)
                {
                    lstGear.SelectedIndex += 1;
                }
                else if (lstGear.Items.Count > 0)
                {
                    lstGear.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstGear.SelectedIndex - 1 >= 0)
                {
                    lstGear.SelectedIndex -= 1;
                }
                else if (lstGear.Items.Count > 0)
                {
                    lstGear.SelectedIndex = lstGear.Items.Count - 1;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }
        #endregion

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
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + _decMaximumCapacity.ToString("#,0.##", GlobalOptions.CultureInfo);
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
        /// What prefixes is our gear allowed to have
        /// </summary>
        public List<string> ForceItemPrefixStrings { get; } = new List<string>();
        #endregion

        #region Methods
        /// <summary>
        /// Update the Gear's information based on the Gear selected and current Rating.
        /// </summary>
        private void UpdateGearInfo()
        {
            string strSelectedId = lstGear.SelectedValue?.ToString();
            if (_blnLoading || string.IsNullOrEmpty(strSelectedId))
            {
                lblGearDeviceRatingLabel.Visible = false;
                lblSourceLabel.Visible = false;
                lblAvailLabel.Visible = false;
                lblCostLabel.Visible = false;
                lblTestLabel.Visible = false;
                lblCapacityLabel.Visible = false;
                lblRatingLabel.Visible = false;
                nudRating.Visible = false;
                lblRatingNALabel.Visible = false;
                lblGearQtyLabel.Visible = false;
                nudGearQty.Visible = false;
                chkStack.Visible = false;
                lblGearDeviceRating.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblAvail.Text = string.Empty;
                lblCost.Text = string.Empty;
                chkBlackMarketDiscount.Checked = false;
                lblTest.Text = string.Empty;
                lblCapacity.Text = string.Empty;
                nudRating.Minimum = 0;
                nudRating.Maximum = 0;
                nudRating.Enabled = false;
                lblSource.SetToolTip(string.Empty);
                return;
            }

            // Retireve the information for the selected piece of Gear.
            XPathNavigator objXmlGear = _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = \"" + strSelectedId + "\"]");

            if (objXmlGear == null)
            {
                lblGearDeviceRatingLabel.Visible = false;
                lblSourceLabel.Visible = false;
                lblAvailLabel.Visible = false;
                lblCostLabel.Visible = false;
                lblTestLabel.Visible = false;
                lblCapacityLabel.Visible = false;
                lblRatingLabel.Visible = false;
                nudRating.Visible = false;
                lblRatingNALabel.Visible = false;
                lblGearQtyLabel.Visible = false;
                nudGearQty.Visible = false;
                chkStack.Visible = false;
                lblGearDeviceRating.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblAvail.Text = string.Empty;
                lblCost.Text = string.Empty;
                chkBlackMarketDiscount.Checked = false;
                lblTest.Text = string.Empty;
                lblCapacity.Text = string.Empty;
                nudRating.Minimum = 0;
                nudRating.Maximum = 0;
                nudRating.Enabled = false;
                lblSource.SetToolTip(string.Empty);
                return;
            }

            // Retrieve the information for the selected piece of Cyberware.
            string strDeviceRating = objXmlGear.SelectSingleNode("devicerating")?.Value ?? string.Empty;
            lblGearDeviceRating.Text = strDeviceRating;
            lblGearDeviceRatingLabel.Visible = !string.IsNullOrEmpty(strDeviceRating);

            string strSource = objXmlGear.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objXmlGear.SelectSingleNode("altpage")?.Value ?? objXmlGear.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

            // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail = string.Empty;
            string strPrefix = string.Empty;
            XPathNavigator objAvailNode = objXmlGear.SelectSingleNode("avail");
            if (objAvailNode == null)
            {
                int intHighestAvailNode = 0;
                foreach (XPathNavigator objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (objLoopNode.Name.StartsWith("avail"))
                    {
                        string strLoopCostString = objLoopNode.Name.Substring(5);
                        if (int.TryParse(strLoopCostString, out int intTmp))
                        {
                            intHighestAvailNode = Math.Max(intHighestAvailNode, intTmp);
                        }
                    }
                }
                objAvailNode = objXmlGear.SelectSingleNode("avail" + intHighestAvailNode);
                for (int i = decimal.ToInt32(nudRating.Value); i <= intHighestAvailNode; ++i)
                {
                    XPathNavigator objLoopNode = objXmlGear.SelectSingleNode("avail" + i.ToString(GlobalOptions.InvariantCultureInfo));
                    if (objLoopNode != null)
                    {
                        objAvailNode = objLoopNode;
                        break;
                    }
                }
            }
            string strAvailExpr = objAvailNode?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(strAvailExpr))
            {
                if (strAvailExpr.StartsWith("FixedValues("))
                {
                    string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvailExpr = strValues[(int) Math.Max(Math.Min(nudRating.Value, strValues.Length) - 1, 0)];
                }

                char chrLastChar = strAvailExpr[strAvailExpr.Length - 1];
                if (chrLastChar == 'R')
                {
                    strAvail = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                else if (chrLastChar == 'F')
                {
                    strAvail = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                if (strAvailExpr[0] == '+')
                {
                    strPrefix = "+";
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }
            }

            object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
            lblAvail.Text = strPrefix + (blnIsSuccess ? (Convert.ToInt32(objProcess) + _intAvailModifier).ToString() : strAvailExpr) + strAvail;
            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);
            
            decimal decMultiplier = nudGearQty.Value / nudGearQty.Increment;
            if (chkDoItYourself.Checked)
                decMultiplier *= 0.5m;

            // Cost.
            chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(objXmlGear.SelectSingleNode("category")?.Value);

            decimal decItemCost = 0.0m;
            if (chkFreeItem.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            }
            else
            {
                XPathNavigator objCostNode = objXmlGear.SelectSingleNode("cost");
                if (objCostNode == null)
                {
                    int intHighestCostNode = 0;
                    foreach (XmlNode objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                    {
                        if (objLoopNode.Name.StartsWith("cost"))
                        {
                            string strLoopCostString = objLoopNode.Name.Substring(4);
                            if (int.TryParse(strLoopCostString, out int intTmp))
                            {
                                intHighestCostNode = Math.Max(intHighestCostNode, intTmp);
                            }
                        }
                    }
                    objCostNode = objXmlGear.SelectSingleNode("cost" + intHighestCostNode);
                    for (int i = decimal.ToInt32(nudRating.Value); i <= intHighestCostNode; ++i)
                    {
                        XPathNavigator objLoopNode = objXmlGear.SelectSingleNode("cost" + i.ToString(GlobalOptions.InvariantCultureInfo));
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
                        objProcess = CommonFunctions.EvaluateInvariantXPath(objCostNode.Value.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)), out blnIsSuccess);
                        decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) * decMultiplier : 0;
                        decCost *= 1 + (nudMarkup.Value / 100.0m);
                        if (chkBlackMarketDiscount.Checked)
                            decCost *= 0.9m;
                        lblCost.Text = (decCost * _intCostMultiplier).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                        decItemCost = decCost;
                    }
                    catch (XPathException)
                    {
                        lblCost.Text = objCostNode.Value;
                        if (decimal.TryParse(objCostNode.Value, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decTemp))
                        {
                            decItemCost = decTemp;
                            lblCost.Text = (decItemCost * _intCostMultiplier).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                        }
                    }

                    if (objCostNode.Value.StartsWith("FixedValues("))
                    {
                        string[] strValues = objCostNode.Value.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                        string strCost = "0";
                        if (nudRating.Value > 0)
                            strCost = strValues[decimal.ToInt32(nudRating.Value) - 1].Trim('[', ']');
                        decimal decCost = Convert.ToDecimal(strCost, GlobalOptions.InvariantCultureInfo) * decMultiplier;
                        decCost *= 1 + (nudMarkup.Value / 100.0m);
                        if (chkBlackMarketDiscount.Checked)
                            decCost *= 0.9m;
                        lblCost.Text = (decCost * _intCostMultiplier).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                        decItemCost = decCost;
                    }
                    else if (objCostNode.Value.StartsWith("Variable("))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        string strCost = objCostNode.Value.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                        if (decMax == decimal.MaxValue)
                            lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                        else
                            lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                        decItemCost = decMin;
                    }
                }
            }
            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
            
            // Update the Avail Test Label.
            lblTest.Text = _objCharacter.AvailTest(decItemCost * _intCostMultiplier, lblAvail.Text);
            lblTestLabel.Visible = true;

            // Capacity.
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            string strCapacityField = ShowArmorCapacityOnly ? "armorcapacity" : "capacity";

            if (_eCapacityStyle == CapacityStyle.Zero)
                lblCapacity.Text = '[' + 0.ToString(GlobalOptions.CultureInfo) + ']';
            else
            {
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

                            if (strCapacity.StartsWith("FixedValues("))
                            {
                                string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                                if (strValues.Length >= decimal.ToInt32(nudRating.Value))
                                    lblCapacity.Text = strValues[decimal.ToInt32(nudRating.Value) - 1];
                                else
                                {
                                    try
                                    {
                                        objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)), out blnIsSuccess);
                                        lblCapacity.Text = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
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
                                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)), out blnIsSuccess);
                                    lblCapacity.Text = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
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
                        if (strCapacityText.StartsWith("FixedValues("))
                        {
                            string[] strValues = strCapacityText.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                            lblCapacity.Text = strValues[Math.Max(Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1, 0)];
                        }
                        else
                        {
                            try
                            {
                                objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)), out blnIsSuccess);
                                lblCapacity.Text = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
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
                    lblCapacity.Text = 0.ToString(GlobalOptions.CultureInfo);
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
                if (strExpression.StartsWith("FixedValues("))
                {
                    string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strExpression = strValues[Math.Max(Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1, 0)].Trim('[', ']');
                }

                if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                {
                    StringBuilder objValue = new StringBuilder(strExpression);
                    objValue.Replace("{Rating}", decimal.ToInt32(nudRating.Value).ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{Parent Rating}", () => (_objGearParent as IHasRating)?.Rating.ToString(GlobalOptions.InvariantCultureInfo) ?? int.MaxValue.ToString(GlobalOptions.InvariantCultureInfo));
                    foreach (string strCharAttributeName in Backend.Attributes.AttributeSection.AttributeStrings)
                    {
                        objValue.CheapReplace(strExpression, '{' + strCharAttributeName + '}', () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString());
                        objValue.CheapReplace(strExpression, '{' + strCharAttributeName + "Base}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString());
                    }

                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out blnIsSuccess);
                    intRating = blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double) objProcess)) : 0;
                }
                else
                    int.TryParse(strExpression, out intRating);
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
                        if (strExpression.StartsWith("FixedValues("))
                        {
                            string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                            strExpression = strValues[Math.Max(Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1, 0)].Trim('[', ']');
                        }

                        if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                        {
                            StringBuilder objValue = new StringBuilder(strExpression);
                            objValue.Replace("{Rating}", decimal.ToInt32(nudRating.Value).ToString(GlobalOptions.InvariantCultureInfo));
                            objValue.CheapReplace(strExpression, "{Parent Rating}", () => (_objGearParent as IHasRating)?.Rating.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                            foreach (string strCharAttributeName in Backend.Attributes.AttributeSection.AttributeStrings)
                            {
                                objValue.CheapReplace(strExpression, '{' + strCharAttributeName + '}', () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString());
                                objValue.CheapReplace(strExpression, '{' + strCharAttributeName + "Base}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString());
                            }

                            // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                            objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out blnIsSuccess);
                            intMinimumRating = blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double)objProcess)) : 0;
                        }
                        else
                            int.TryParse(strExpression, out intMinimumRating);
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
                    while (nudRating.Maximum > nudRating.Minimum && !SelectionShared.CheckAvailRestriction(objXmlGear, _objCharacter, decimal.ToInt32(nudRating.Maximum), _intAvailModifier))
                    {
                        nudRating.Maximum -= 1;
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
                    while (nudRating.Maximum > nudRating.Minimum && !SelectionShared.CheckNuyenRestriction(objXmlGear, _objCharacter.Nuyen, decCostMultiplier, decimal.ToInt32(nudRating.Maximum)))
                    {
                        nudRating.Maximum -= 1;
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
        }

        private IList<ListItem> RefreshList(string strCategory, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            StringBuilder strFilter = new StringBuilder("(" + _objCharacter.Options.BookXPath() + ')');
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter.Append(" and category = \"" + strCategory + '\"');
            else if (_setAllowedCategories.Count > 0)
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter.Append(" and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')');
                }
            }
            if (ShowArmorCapacityOnly)
                strFilter.Append(" and (contains(armorcapacity, \"[\") or category = \"Custom\")");
            else if (ShowPositiveCapacityOnly)
                strFilter.Append(" and (not(contains(capacity, \"[\")) or category = \"Custom\")");
            else if (ShowNegativeCapacityOnly)
                strFilter.Append(" and (contains(capacity, \"[\") or category = \"Custom\")");
            if (_objGearParent == null)
                strFilter.Append(" and not(requireparent)");
            foreach (string strPrefix in ForceItemPrefixStrings)
                strFilter.Append(" and starts-with(name,\"" + strPrefix + "\")");

            strFilter.Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

            return BuildGearList(_xmlBaseGearDataNode.Select("gears/gear[" + strFilter + "]"), blnDoUIUpdate, blnTerminateAfterFirst);
        }

        private IList<ListItem> BuildGearList(XPathNodeIterator objXmlGearList, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            List<ListItem> lstGears = new List<ListItem>();
            foreach (XPathNavigator objXmlGear in objXmlGearList)
            {
                XPathNavigator xmlTestNode = objXmlGear.SelectSingleNode("forbidden/parentdetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlGear.SelectSingleNode("required/parentdetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlGear.SelectSingleNode("forbidden/geardetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlGear.SelectSingleNode("required/geardetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }

                if (!objXmlGear.RequirementsMet(_objCharacter))
                    continue;

                if (!blnDoUIUpdate && blnTerminateAfterFirst)
                {
                    lstGears.Add(new ListItem(string.Empty, string.Empty));
                }

                decimal decCostMultiplier = nudGearQty.Value / nudGearQty.Increment;
                if (chkDoItYourself.Checked)
                    decCostMultiplier *= 0.5m;
                decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                if (_setBlackMarketMaps.Contains(objXmlGear.SelectSingleNode("category")?.Value))
                    decCostMultiplier *= 0.9m;
                if (!blnDoUIUpdate ||
                    ((!chkHideOverAvailLimit.Checked || SelectionShared.CheckAvailRestriction(objXmlGear, _objCharacter, 1, _intAvailModifier) &&
                    (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked ||
                    SelectionShared.CheckNuyenRestriction(objXmlGear, _objCharacter.Nuyen, decCostMultiplier)))))
                {
                    string strDisplayName = objXmlGear.SelectSingleNode("translate")?.Value ?? objXmlGear.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);

                    if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                    {
                        string strCategory = objXmlGear.SelectSingleNode("category")?.Value;
                        if (!string.IsNullOrEmpty(strCategory))
                        {
                            ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                            if (!string.IsNullOrEmpty(objFoundItem.Name))
                                strDisplayName += " [" + objFoundItem.Name + "]";
                        }
                    }
                    // When searching, Category needs to be added to the Value so we can identify the English Category name.
                    lstGears.Add(new ListItem(objXmlGear.SelectSingleNode("id")?.Value ?? string.Empty, strDisplayName));

                    if (blnTerminateAfterFirst)
                        break;
                }
            }
            if (blnDoUIUpdate)
            {
                lstGears.Sort(CompareListItems.CompareNames);
                lstGear.BeginUpdate();
                string strOldSelected = lstGear.SelectedValue?.ToString();
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                lstGear.ValueMember = nameof(ListItem.Value);
                lstGear.DisplayMember = nameof(ListItem.Name);
                lstGear.DataSource = lstGears;
                _blnLoading = blnOldLoading;
                if (string.IsNullOrEmpty(strOldSelected))
                    lstGear.SelectedIndex = -1;
                else
                    lstGear.SelectedValue = strOldSelected;
                lstGear.EndUpdate();
            }

            return lstGears;
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
                s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0)
                    ? cboCategory.SelectedValue?.ToString()
                    : _xmlBaseGearDataNode.SelectSingleNode("gears/gear[id = \"" + strSelectedId + "\"]/category")?.Value;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                _intSelectedRating = decimal.ToInt32(nudRating.Value);
                _decSelectedQty = nudGearQty.Value;
                _decMarkup = nudMarkup.Value;

                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
