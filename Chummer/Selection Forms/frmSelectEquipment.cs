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
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectEquipment : Form
    {
        private bool _blnLoading = true;
        private decimal _decArmorCapacity;
        private decimal _decArmorCost;
        private CapacityStyle _eCapacityStyle = CapacityStyle.Standard;

        private readonly XPathNavigator _xmlBaseDataNode;
        private readonly object _objParentObject;
        private readonly XPathNavigator _objParentNode;
        private readonly Character _objCharacter;
        private decimal _intCostMultiplier = 1;
        private string _strXmlPath = "";
        private bool _blnShowNegativeCapacityOnly;
        private bool _blnShowPositiveCapacityOnly;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setAllowedCategories = new HashSet<string>();
        private readonly HashSet<string> _setBlackMarketMaps;

        #region Control Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objCharacter">Character Object to load.</param>
        /// <param name="strXmlName">Name of the XML Document to load, ie armor.xml.</param>
        /// <param name="strXmlPath">Path for XML objects, ie mods/mod</param>
        /// <param name="objParentNode">Object to which the item will be a child.</param>
        /// <param name="strAllowedCategories">Permitted categories.</param>
        public frmSelectEquipment(Character objCharacter, string strXmlName, string strXmlPath, object objParentNode = null, string strAllowedCategories = "")
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            // Load the Armor information.
            _xmlBaseDataNode = XmlManager.Load(strXmlName).GetFastNavigator().SelectSingleNode("/chummer");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseDataNode);
            _objParentObject = objParentNode;
            _objParentNode = (_objParentObject as IHasXmlNode)?.GetNode()?.CreateNavigator();
            _strXmlPath = strXmlPath;

            foreach (string strCategory in strAllowedCategories.TrimEndOnce(',').Split(','))
            {
                string strLoop = strCategory.Trim();
                if (!string.IsNullOrEmpty(strLoop))
                    _setAllowedCategories.Add(strLoop);
            }
        }

        private void frmSelectArmorMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
            }
            else
            {
                chkHideOverAvailLimit.Text = string.Format(chkHideOverAvailLimit.Text, _objCharacter.MaximumAvailability.ToString(GlobalOptions.CultureInfo));
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
            }
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

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
                objXmlCategoryList = _xmlBaseDataNode.Select("categories/category[" + strMount.ToString() + "]");
            }
            else
            {
                objXmlCategoryList = _xmlBaseDataNode.Select("categories/category");
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

            _blnLoading = false;
        }

        private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
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

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
        }

        private void lstMod_DoubleClick(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList(cboCategory.SelectedValue.ToString());
            }
            UpdateSelectedItem();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                RefreshList(cboCategory.SelectedValue.ToString());
            }
            UpdateSelectedItem();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; set; }

        /// <summary>
        /// Whether or not the selected object was acquired through Black Market Discount.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Name of Accessory that was selected in the dialogue.
        /// </summary>
        public string SelectedItemName { get; private set; } = string.Empty;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => decimal.ToInt32(nudRating.Value);

        /// <summary>
        /// Whether or not the General category should be included.
        /// </summary>
        public bool ExcludeGeneralCategory { get; set; }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup { get; private set; }

        /// <summary>
        /// Capacity display style.
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            set => _eCapacityStyle = value;
        }
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
        /// What prefixes is our gear allowed to have
        /// </summary>
        public List<string> ForceItemPrefixStrings { get; } = new List<string>();

        #endregion

        #region Methods
        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
        }
        /// <summary>
        /// Update the information for the selected Armor Mod.
        /// </summary>
        private void UpdateSelectedItem()
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstItems.SelectedValue?.ToString();
            XPathNavigator objXmlItem = _xmlBaseDataNode.SelectSingleNode($"{_strXmlPath}[id = \"{strSelectedId}\"]");
            if (objXmlItem == null)
            {
                lblALabel.Visible = false;
                lblA.Text = string.Empty;
                lblRatingLabel.Visible = false;
                lblRatingNALabel.Visible = false;
                nudRating.Enabled = false;
                nudRating.Visible = false;
                lblAvailLabel.Visible = false;
                lblAvail.Text = string.Empty;
                lblCostLabel.Visible = false;
                lblCost.Text = string.Empty;
                chkBlackMarketDiscount.Checked = false;
                lblCapacityLabel.Visible = false;
                lblCapacity.Text = string.Empty;
                lblSourceLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                return;
            }
            // Extract the Avil and Cost values from the object info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            lblA.Text = objXmlItem.SelectSingleNode("armor")?.Value;
            lblALabel.Visible = !string.IsNullOrEmpty(lblA.Text);

            nudRating.Maximum = Convert.ToDecimal(objXmlItem.SelectSingleNode("maxrating")?.Value, GlobalOptions.InvariantCultureInfo);
            if (chkHideOverAvailLimit.Checked)
            {
                while (nudRating.Maximum > 1 && !SelectionShared.CheckAvailRestriction(objXmlItem, _objCharacter, decimal.ToInt32(nudRating.Maximum)))
                {
                    nudRating.Maximum -= 1;
                }
            }

            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                if (_setBlackMarketMaps.Contains(objXmlItem.SelectSingleNode("category")?.Value))
                    decCostMultiplier *= 0.9m;
                while (nudRating.Maximum > 1 && !SelectionShared.CheckNuyenRestriction(objXmlItem, _objCharacter.Nuyen, decCostMultiplier, decimal.ToInt32(nudRating.Maximum)))
                {
                    nudRating.Maximum -= 1;
                }
            }

            lblRatingLabel.Visible = true;
            if (nudRating.Maximum <= 1)
            {
                lblRatingNALabel.Visible = true;
                nudRating.Visible = false;
                nudRating.Enabled = false;
            }
            else
            {
                nudRating.Enabled = nudRating.Minimum != nudRating.Maximum;
                if (nudRating.Minimum == 0)
                {
                    nudRating.Value = 1;
                    nudRating.Minimum = 1;
                }
                nudRating.Visible = true;
                lblRatingNALabel.Visible = false;
            }

            string strAvail = string.Empty;
            string strAvailExpr = objXmlItem.SelectSingleNode("avail")?.Value ?? string.Empty;
            string strPrefix = string.Empty;

            if (!string.IsNullOrEmpty(strAvailExpr))
            {
                if (strAvailExpr.StartsWith("FixedValues("))
                {
                    string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvailExpr = strValues[(int)Math.Max(Math.Min(nudRating.Value, strValues.Length) - 1, 0)];
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
            lblAvail.Text = (blnIsSuccess ? Convert.ToInt32(objProcess).ToString() : strAvailExpr) + strAvail;
            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

            // Cost.
            chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(objXmlItem.SelectSingleNode("category")?.Value);

            decimal decItemCost = 0.0m;
            if (chkFreeItem.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            }
            else
            {
                XPathNavigator objCostNode = objXmlItem.SelectSingleNode("cost");
                if (objCostNode == null)
                {
                    int intHighestCostNode = 0;
                    foreach (XmlNode objLoopNode in objXmlItem.SelectChildren(XPathNodeType.Element))
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
                    objCostNode = objXmlItem.SelectSingleNode("cost" + intHighestCostNode);
                    for (int i = decimal.ToInt32(nudRating.Value); i <= intHighestCostNode; ++i)
                    {
                        XPathNavigator objLoopNode = objXmlItem.SelectSingleNode("cost" + i.ToString(GlobalOptions.InvariantCultureInfo));
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
                        decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
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
                        decimal decCost = Convert.ToDecimal(strCost, GlobalOptions.InvariantCultureInfo);
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

            // Capacity.
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            string strCapacity = objXmlItem.SelectSingleNode("armorcapacity")?.Value;

            // Handle YNT Softweave
            if (_eCapacityStyle == CapacityStyle.Zero || string.IsNullOrEmpty(strCapacity))
                lblCapacity.Text = "[0]";
            else
            {
                if (strCapacity.StartsWith("FixedValues("))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCapacity = strValues[decimal.ToInt32(nudRating.Value) - 1];
                }

                strCapacity = strCapacity.CheapReplace("Capacity", () => _decArmorCapacity.ToString(GlobalOptions.InvariantCultureInfo))
                    .CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
                bool blnSquareBrackets = strCapacity.StartsWith('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out blnIsSuccess);
                string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                lblCapacity.Text = strReturn;
            }

            lblCapacityLabel.Visible = !string.IsNullOrEmpty(lblCapacity.Text);

            string strSource = objXmlItem.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objXmlItem.SelectSingleNode("altpage")?.Value ?? objXmlItem.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private IList<ListItem> BuildItemList(XPathNodeIterator objXmlItemList, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            List<ListItem> lstObjects = new List<ListItem>();
            foreach (XPathNavigator objXmlItem in objXmlItemList)
            {
                XPathNavigator xmlTestNode = objXmlItem.SelectSingleNode("forbidden/parentdetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlItem.SelectSingleNode("required/parentdetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlItem.SelectSingleNode("forbidden/geardetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlItem.SelectSingleNode("required/geardetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }

                if (!objXmlItem.RequirementsMet(_objCharacter))
                    continue;

                if (!blnDoUIUpdate && blnTerminateAfterFirst)
                {
                    lstObjects.Add(new ListItem(string.Empty, string.Empty));
                }

                decimal decCostMultiplier = 1;
                decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                if (_setBlackMarketMaps.Contains(objXmlItem.SelectSingleNode("category")?.Value))
                    decCostMultiplier *= 0.9m;
                if (!blnDoUIUpdate ||
                    ((!chkHideOverAvailLimit.Checked || SelectionShared.CheckAvailRestriction(objXmlItem, _objCharacter, 1, 0) &&
                    (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked ||
                    SelectionShared.CheckNuyenRestriction(objXmlItem, _objCharacter.Nuyen, decCostMultiplier)))))
                {
                    string strDisplayName = objXmlItem.SelectSingleNode("translate")?.Value ?? objXmlItem.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    // When searching, Category needs to be added to the Value so we can identify the English Category name.
                    if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                    {
                        string strCategory = objXmlItem.SelectSingleNode("category")?.Value;
                        if (!string.IsNullOrEmpty(strCategory))
                        {
                            ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                            if (!string.IsNullOrEmpty(objFoundItem.Name))
                                strDisplayName += " [" + objFoundItem.Name + "]";
                        }
                    }
                    lstObjects.Add(new ListItem(objXmlItem.SelectSingleNode("id")?.Value ?? string.Empty, strDisplayName));

                    if (blnTerminateAfterFirst)
                        break;
                }
            }
            if (blnDoUIUpdate)
            {
                lstObjects.Sort(CompareListItems.CompareNames);
                lstItems.BeginUpdate();
                string strOldSelected = lstItems.SelectedValue?.ToString();
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                lstItems.ValueMember = nameof(ListItem.Value);
                lstItems.DisplayMember = nameof(ListItem.Name);
                lstItems.DataSource = lstObjects;
                _blnLoading = blnOldLoading;
                if (string.IsNullOrEmpty(strOldSelected))
                    lstItems.SelectedIndex = -1;
                else
                    lstItems.SelectedValue = strOldSelected;
                lstItems.EndUpdate();
            }

            return lstObjects;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstItems.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                SelectedItemName = strSelectedId;
                Markup = nudMarkup.Value;
                BlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
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
            if (_objParentObject == null)
                strFilter.Append(" and not(requireparent)");
            foreach (string strPrefix in ForceItemPrefixStrings)
                strFilter.Append(" and starts-with(name,\"" + strPrefix + "\")");

            strFilter.Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

            return BuildItemList(_xmlBaseDataNode.Select($"{_strXmlPath}[{strFilter}]"), blnDoUIUpdate, blnTerminateAfterFirst);
        }
        #endregion

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList(cboCategory.SelectedValue.ToString());
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList(cboCategory.SelectedValue.ToString());
        }
    }
}
