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
using System.Threading.Tasks;

namespace Chummer
{
    public partial class frmSelectGear : Form
    {
        private string _strSelectedGear = string.Empty;
        private string _strSelectedCategory = string.Empty;
        private int _intSelectedRating = 0;
        private decimal _decSelectedQty = 1;
        private decimal _decMarkup = 0;

        private int _intAvailModifier = 0;
        private int _intCostMultiplier = 1;

        private string _strAllowedCategories = string.Empty;
        private XmlNode _objParentNode = null;
        private decimal _decMaximumCapacity = -1;
        private bool _blnAddAgain = false;
        private static string _strSelectCategory = string.Empty;
        private bool _blnShowPositiveCapacityOnly = false;
        private bool _blnShowNegativeCapacityOnly = false;
        private bool _blnShowArmorCapacityOnly = false;
        private bool _blnBlackMarketDiscount;
        private CapacityStyle _objCapacityStyle = CapacityStyle.Standard;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectGear(Character objCharacter, bool blnCareer = false, int intAvailModifier = 0, int intCostMultiplier = 1, XmlNode objParentNode = null)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _intAvailModifier = intAvailModifier;
            _intCostMultiplier = intCostMultiplier;
            _objCharacter = objCharacter;
            _objParentNode = objParentNode;
            // Stack Checkbox is only available in Career Mode.
            if (!_objCharacter.Created)
            {
                chkStack.Checked = false;
                chkStack.Visible = false;
            }

            MoveControls();
            // Load the Gear information.
            _objXmlDocument = XmlManager.Load("gear.xml");
        }

        private void frmSelectGear_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}", _objCharacter.MaximumAvailability.ToString());
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }
            XmlNodeList objXmlCategoryList;

            // Populate the Gear Category list.
            if (!string.IsNullOrEmpty(_strAllowedCategories))
            {
                if (_strAllowedCategories.EndsWith(','))
                    _strAllowedCategories = _strAllowedCategories.Substring(0, _strAllowedCategories.Length - 1);

                string[] strAllowed = _strAllowedCategories.Split(',');
                string strMount = string.Empty;
                foreach (string strAllowedMount in strAllowed)
                {
                    if (!string.IsNullOrEmpty(strAllowedMount))
                        strMount += ". = \"" + strAllowedMount + "\" or ";
                }
                strMount += "category = \"General\"";
                objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category[" + strMount + "]");
            }
            else
            {
                objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            }

            // Create a list of any Categories that should not be in the list.
            List<string> lstRemoveCategory = new List<string>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                bool blnRemoveItem = true;

                if (objXmlCategory.Attributes["show"] != null)
                {
                    if (objXmlCategory.Attributes["show"].InnerText == "false")
                    {
                        string[] strAllowed = _strAllowedCategories.Split(',');
                        foreach (string strAllowedMount in strAllowed)
                        {
                            if (strAllowedMount == objXmlCategory.InnerText)
                                blnRemoveItem = false;
                        }
                    }

                    if (blnRemoveItem)
                        lstRemoveCategory.Add(objXmlCategory.InnerText);
                }

                string strXPath = "/chummer/gears/gear[category = \"" + objXmlCategory.InnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")";
                if (_blnShowArmorCapacityOnly)
                    strXPath += " and contains(armorcapacity, \"[\")";
                else
                {
                    if (_blnShowPositiveCapacityOnly)
                        strXPath += " and not(contains(capacity, \"[\"))";
                }
                if (_objParentNode == null)
                    strXPath += " and not(requireparent)";
                strXPath += "]";

                XmlNodeList objItems = _objXmlDocument.SelectNodes(strXPath);
                if (objItems.Count > 0)
                    blnRemoveItem = false;

                if (blnRemoveItem)
                    lstRemoveCategory.Add(objXmlCategory.InnerText);
            }

            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                // Make sure the Category isn't in the exclusion list.
                bool blnAddItem = true;
                foreach (string strCategory in lstRemoveCategory)
                {
                    if (strCategory == objXmlCategory.InnerText)
                    {
                        blnAddItem = false;
                        break;
                    }
                }
                if (blnAddItem)
                {
                    // Also make sure it is not already in the Category list.
                    foreach (ListItem objItem in _lstCategory)
                    {
                        if (objItem.Value == objXmlCategory.InnerText)
                        {
                            blnAddItem = false;
                            break;
                        }
                    }
                    if (blnAddItem)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objXmlCategory.InnerText;
                        objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                        _lstCategory.Add(objItem);
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);

            if (_lstCategory.Count > 0)
            {
                ListItem objItem = new ListItem();
                objItem.Value = "Show All";
                objItem.Name = LanguageManager.GetString("String_ShowAll");
                _lstCategory.Insert(0, objItem);
            }

            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            if (!string.IsNullOrEmpty(_strSelectedGear))
                lstGear.SelectedValue = _strSelectedGear;
            else
                txtSearch.Text = DefaultSearchText;
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch_TextChanged(sender, e);
                return;
            }
            if (cboCategory.SelectedValue == null)
                return;

            // Update the list of Weapon based on the selected Category.
            XmlNodeList objXmlGearList;
            txtSearch.Text = string.Empty;

            string strSelectedCategoryPath = string.Empty;
            // If category selected is "Show All", we show all items regardless of category, otherwise we set the category string to filter for the selected category
            if (cboCategory.SelectedValue != null && cboCategory.SelectedValue.ToString() != "Show All")
            {
                strSelectedCategoryPath = "category = \"" + cboCategory.SelectedValue + "\" and ";
            }
            else
            {
                if (!string.IsNullOrEmpty(_strAllowedCategories))
                {
                    string[] strAllowed = _strAllowedCategories.Split(',');
                    for (int index = 0; index < strAllowed.Length; index++)
                    {
                        if (index == 0)
                        {
                            strSelectedCategoryPath = $"(category = \"{strAllowed[index]}\"";
                        }
                        string strAllowedMount = strAllowed[index];
                        if (!string.IsNullOrEmpty(strAllowedMount))
                            strSelectedCategoryPath += $" or category = \"{strAllowed[index]}\"";
                    }
                    strSelectedCategoryPath += ") and ";
                }
            }
            // Retrieve the list of Gear for the selected Category.
            if (!_blnShowNegativeCapacityOnly && !_blnShowPositiveCapacityOnly && !_blnShowArmorCapacityOnly)
                objXmlGearList = _objXmlDocument.SelectNodes("/chummer/gears/gear[" + strSelectedCategoryPath + "(" + _objCharacter.Options.BookXPath() + ")]");
            else
            {
                if (_blnShowArmorCapacityOnly)
                    objXmlGearList = _objXmlDocument.SelectNodes("/chummer/gears/gear[" + strSelectedCategoryPath + "(" + _objCharacter.Options.BookXPath() + ") and contains(armorcapacity, \"[\")]");
                else
                {
                    if (_blnShowPositiveCapacityOnly)
                        objXmlGearList = _objXmlDocument.SelectNodes("/chummer/gears/gear[" + strSelectedCategoryPath + "(" + _objCharacter.Options.BookXPath() + ") and not(contains(capacity, \"[\"))]");
                    else
                        objXmlGearList = _objXmlDocument.SelectNodes("/chummer/gears/gear[" + strSelectedCategoryPath + "(" + _objCharacter.Options.BookXPath() + ") and contains(capacity, \"[\")]");
                }
            }

            List<ListItem> lstGears = new List<ListItem>();
            foreach (XmlNode objXmlGear in objXmlGearList)
            {
                if (objXmlGear["requireparent"] == null || _objParentNode != null)
                {
                    if (!_blnShowArmorCapacityOnly || objXmlGear["armorcapacity"] != null)
                    {
                        if (objXmlGear["forbidden"]?["parentdetails"] != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (_objParentNode.ProcessFilterOperationNode(objXmlGear["forbidden"]["parentdetails"], false))
                            {
                                continue;
                            }
                        }
                        if (objXmlGear["required"]?["parentdetails"] != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (!_objParentNode.ProcessFilterOperationNode(objXmlGear["required"]["parentdetails"], false))
                            {
                                continue;
                            }
                        }
                        decimal decCostMultiplier = nudGearQty.Value / nudGearQty.Increment;
                        if (chkDoItYourself.Checked)
                            decCostMultiplier *= 0.5m;
                        decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                        if (chkBlackMarketDiscount.Checked)
                            decCostMultiplier *= 0.9m;
                        if (chkHacked.Checked)
                            decCostMultiplier *= 0.1m;
                        if (Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlGear, _objCharacter, chkHideOverAvailLimit.Checked, 1, _intAvailModifier) &&
                            (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked ||
                            Backend.Shared_Methods.SelectionShared.CheckNuyenRestriction(objXmlGear, _objCharacter, _objCharacter.Nuyen, decCostMultiplier)))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlGear["name"].InnerText;
                            objItem.Name = objXmlGear["translate"]?.InnerText ?? objXmlGear["name"].InnerText;
                            lstGears.Add(objItem);
                        }
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            lstGears.Sort(objSort.Compare);
            lstGear.BeginUpdate();
            lstGear.DataSource = null;
            lstGear.ValueMember = "Value";
            lstGear.DisplayMember = "Name";
            lstGear.DataSource = lstGears;
            lstGear.EndUpdate();

            // Show the Do It Yourself CheckBox if the Commlink Upgrade category is selected.
            if (cboCategory.SelectedValue?.ToString() == "Commlink Upgrade")
                chkDoItYourself.Visible = true;
            else
            {
                chkDoItYourself.Visible = false;
                chkDoItYourself.Checked = false;
            }
        }

        private void lstGear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGear.SelectedValue == null)
                return;

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlGear = null;

            // Filtering is also done on the Category in case there are non-unique names across categories.
            if (lstGear.SelectedValue.ToString().Contains('^'))
            {
                // If the SelectedValue contains ^, then it also includes the English Category name which needs to be extracted.
                int intIndexOf = lstGear.SelectedValue.ToString().IndexOf('^');
                string strValue = lstGear.SelectedValue.ToString().Substring(0, intIndexOf);
                string strCategory = lstGear.SelectedValue.ToString().Substring(intIndexOf + 1, lstGear.SelectedValue.ToString().Length - intIndexOf - 1);
                objXmlGear = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + strValue + "\" and category = \"" + strCategory + "\"]");
            }
            if (objXmlGear == null)
            {
                string strSelectedCategoryPath = string.Empty;
                // If category selected is "Show All", we show all items regardless of category, otherwise we set the category string to filter for the selected category
                if (cboCategory.SelectedValue != null && cboCategory.SelectedValue.ToString() != "Show All")
                {
                    strSelectedCategoryPath = " and category = \"" + cboCategory.SelectedValue + "\"";
                }
                else
                {
                    if (!string.IsNullOrEmpty(_strAllowedCategories))
                    {
                        string[] strAllowed = _strAllowedCategories.Split(',');
                        for (int index = 0; index < strAllowed.Length; index++)
                        {
                            if (index == 0)
                            {
                                strSelectedCategoryPath = $"category = \"{strAllowed[index]}\"";
                            }
                            string strAllowedMount = strAllowed[index];
                            if (!string.IsNullOrEmpty(strAllowedMount))
                                strSelectedCategoryPath += $" or category = \"{strAllowed[index]}\"";
                        }
                        strSelectedCategoryPath = " and (" + strSelectedCategoryPath + ")";
                    }
                }
                objXmlGear = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + lstGear.SelectedValue + "\"" + strSelectedCategoryPath + "]");
            }

            if (objXmlGear == null)
                return;

            // If a Grenade is selected, show the Aerodynamic checkbox.
            if (objXmlGear["name"].InnerText.StartsWith("Grenade:"))
                chkAerodynamic.Visible = true;
            else
            {
                chkAerodynamic.Visible = false;
                chkAerodynamic.Checked = false;
            }

            // Quantity.
            nudGearQty.Minimum = 1;
            if (objXmlGear["costfor"] != null)
            {
                nudGearQty.Value = Convert.ToDecimal(objXmlGear["costfor"].InnerText, GlobalOptions.InvariantCultureInfo);
                nudGearQty.Increment = Convert.ToDecimal(objXmlGear["costfor"].InnerText, GlobalOptions.InvariantCultureInfo);
            }
            else
            {
                nudGearQty.Value = 1;
                nudGearQty.Increment = 1;
            }
            if (objXmlGear["category"].InnerText == "Currency")
            {
                nudGearQty.DecimalPlaces = 2;
                nudGearQty.Minimum = 0.01m;
            }
            else
            {
                nudGearQty.DecimalPlaces = 0;
                nudGearQty.Minimum = 1.0m;
            }

            UpdateGearInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                string strCurrentSelection = lstGear.SelectedValue.ToString();
                cboCategory_SelectedIndexChanged(sender, e);
                lstGear.SelectedValue = strCurrentSelection;
            }
            UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstGear.Text))
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }

            string strCategoryFilter = string.Empty;

            if (cboCategory.SelectedValue != null && (cboCategory.SelectedValue.ToString() != "Show All" && _objCharacter.Options.SearchInCategoryOnly))
            {
                strCategoryFilter = "and category = \"" + cboCategory.SelectedValue.ToString() + "\"";
            }
            else if (!string.IsNullOrEmpty(_strAllowedCategories))
            {
                string[] strAllowed = _strAllowedCategories.Split(',');
                for (int index = 0; index < strAllowed.Length; index++)
                {
                    if (index == 0)
                    {
                        strCategoryFilter = $"category = \"{strAllowed[index]}\"";
                    }
                    string strAllowedMount = strAllowed[index];
                    if (!string.IsNullOrEmpty(strAllowedMount))
                        strCategoryFilter += $" or category = \"{strAllowed[index]}\"";
                }
                strCategoryFilter = " and (" + strCategoryFilter + ")";
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = $"/chummer/gears/gear[({_objCharacter.Options.BookXPath()}) {strCategoryFilter} and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"{txtSearch.Text.ToUpper()}\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"{txtSearch.Text.ToUpper()}\"))]";

            XmlNodeList objXmlGearList = _objXmlDocument.SelectNodes(strSearch);
            List<ListItem> lstGears = new List<ListItem>();
            foreach (XmlNode objXmlGear in objXmlGearList)
            {
                if (objXmlGear["requireparent"] == null || _objParentNode != null)
                {
                    if (!_blnShowArmorCapacityOnly || objXmlGear["armorcapacity"] != null)
                    {
                        if (objXmlGear["forbidden"]?["parentdetails"] != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (_objParentNode.ProcessFilterOperationNode(objXmlGear["forbidden"]["parentdetails"], false))
                            {
                                continue;
                            }
                        }
                        if (objXmlGear["required"]?["parentdetails"] != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (!_objParentNode.ProcessFilterOperationNode(objXmlGear["required"]["parentdetails"], false))
                            {
                                continue;
                            }
                        }
                        // Only add items that appear in the list of Categories.
                        foreach (object objListItem in cboCategory.Items)
                        {
                            ListItem objCategoryItem = (ListItem)objListItem;
                            if (objCategoryItem.Value == objXmlGear["category"].InnerText)
                            {
                                decimal decCostMultiplier = nudGearQty.Value / nudGearQty.Increment;
                                if (chkDoItYourself.Checked)
                                    decCostMultiplier *= 0.5m;
                                decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                                if (chkBlackMarketDiscount.Checked)
                                    decCostMultiplier *= 0.9m;
                                if (chkHacked.Checked)
                                    decCostMultiplier *= 0.1m;
                                if (Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlGear, _objCharacter, chkHideOverAvailLimit.Checked, 1, _intAvailModifier) &&
                                    (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked ||
                                    Backend.Shared_Methods.SelectionShared.CheckNuyenRestriction(objXmlGear, _objCharacter, _objCharacter.Nuyen, decCostMultiplier)))
                                {
                                    ListItem objItem = new ListItem();
                                    // When searching, Category needs to be added to the Value so we can identify the English Category name.
                                    objItem.Value = objXmlGear["name"].InnerText + "^" + objXmlGear["category"].InnerText;
                                    objItem.Name = objXmlGear["translate"]?.InnerText ?? objXmlGear["name"].InnerText;

                                    if (objXmlGear["category"] != null)
                                    {
                                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == objXmlGear["category"].InnerText);

                                        if (objFoundItem != null)
                                        {
                                            objItem.Name += " [" + objFoundItem.Name + "]";
                                        }
                                    }
                                    lstGears.Add(objItem);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            lstGears.Sort(objSort.Compare);
            lstGear.BeginUpdate();
            lstGear.DataSource = null;
            lstGear.ValueMember = "Value";
            lstGear.DisplayMember = "Name";
            lstGear.DataSource = lstGears;
            lstGear.EndUpdate();
        }

        private void lstGear_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstGear.Text))
                AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void nudGearQty_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                string strCurrentSelection = lstGear.SelectedValue.ToString();
                cboCategory_SelectedIndexChanged(sender, e);
                lstGear.SelectedValue = strCurrentSelection;
            }
            UpdateGearInfo();
        }

        private void chkDoItYourself_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                string strCurrentSelection = lstGear.SelectedValue.ToString();
                cboCategory_SelectedIndexChanged(sender, e);
                lstGear.SelectedValue = strCurrentSelection;
            }
            UpdateGearInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                string strCurrentSelection = lstGear.SelectedValue.ToString();
                cboCategory_SelectedIndexChanged(sender, e);
                lstGear.SelectedValue = strCurrentSelection;
            }
            UpdateGearInfo();
        }

        private void chkHacked_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                string strCurrentSelection = lstGear.SelectedValue.ToString();
                cboCategory_SelectedIndexChanged(sender, e);
                lstGear.SelectedValue = strCurrentSelection;
            }
            UpdateGearInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstGear.SelectedIndex + 1 < lstGear.Items.Count)
                {
                    lstGear.SelectedIndex++;
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
                    lstGear.SelectedIndex--;
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
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Only items that grant Capacity should be shown.
        /// </summary>
        public bool ShowPositiveCapacityOnly
        {
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
        public bool ShowArmorCapacityOnly
        {
            set
            {
                _blnShowArmorCapacityOnly = value;
            }
        }

        /// <summary>
        /// Name of Gear that was selected in the dialogue.
        /// </summary>
        public string SelectedGear
        {
            get
            {
                return _strSelectedGear;
            }
            set
            {
                _strSelectedGear = value;
            }
        }

        /// <summary>
        /// Name of the Category the selected piece of Gear belongs to.
        /// </summary>
        public string SelectedCategory
        {
            get
            {
                return _strSelectedCategory;
            }
        }

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating
        {
            get
            {
                return _intSelectedRating;
            }
        }

        /// <summary>
        /// Quantity that was selected in the dialogue.
        /// </summary>
        public decimal SelectedQty
        {
            get
            {
                return _decSelectedQty;
            }
        }

        /// <summary>
        /// Set the maximum Capacity the piece of Gear is allowed to be.
        /// </summary>
        public decimal MaximumCapacity
        {
            set
            {
                _decMaximumCapacity = value;
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed") + " " + _decMaximumCapacity.ToString("#,0.##", GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Categories that the Gear allows to be used.
        /// </summary>
        public string AllowedCategories
        {
            get
            {
                return _strAllowedCategories;
            }
            set
            {
                _strAllowedCategories = value;
            }
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost
        {
            get
            {
                return chkFreeItem.Checked;
            }
        }

        /// <summary>
        /// Whether or not the item's cost should be cut in half for being a Do It Yourself component/upgrade.
        /// </summary>
        public bool DoItYourself
        {
            get
            {
                return chkDoItYourself.Checked;
            }
        }

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup
        {
            get
            {
                return _decMarkup;
            }
        }

        /// <summary>
        /// Whether or not the item was hacked which reduces the cost to 10% of the original value.
        /// </summary>
        public bool Hacked
        {
            get
            {
                return chkHacked.Checked;
            }
        }

        /// <summary>
        /// Whether or not the Gear should stack with others if possible.
        /// </summary>
        public bool Stack
        {
            get
            {
                return chkStack.Checked;
            }
        }

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
            set
            {
                _objCapacityStyle = value;
            }
        }

        /// <summary>
        /// Whether or not the item is an Inherent Program for A.I.s.
        /// </summary>
        public bool InherentProgram
        {
            get
            {
                return chkInherentProgram.Checked;
            }
        }

        /// <summary>
        /// Whether or not a Grenade is Aerodynamic.
        /// </summary>
        public bool Aerodynamic
        {
            get
            {
                return chkAerodynamic.Checked;
            }
        }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get
            {
                return _blnBlackMarketDiscount;
            }
        }

        /// <summary>
        /// Default text string to filter by.
        /// </summary>
        public string DefaultSearchText { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Update the Gear's information based on the Gear selected and current Rating.
        /// </summary>
        private void UpdateGearInfo()
        {
            if (!string.IsNullOrEmpty(lstGear.Text))
            {
                // Retireve the information for the selected piece of Cyberware.
                XmlNode objXmlGear = null;
                decimal decItemCost = 0.0m;

                // Filtering is also done on the Category in case there are non-unique names across categories.
                string strCategory = string.Empty;
                if (lstGear.SelectedValue.ToString().Contains('^'))
                {
                    // If the SelectedValue contains ^, then it also includes the English Category name which needs to be extracted.
                    int intIndexOf = lstGear.SelectedValue.ToString().IndexOf('^');
                    string strValue = lstGear.SelectedValue.ToString().Substring(0, intIndexOf);
                    strCategory = lstGear.SelectedValue.ToString().Substring(intIndexOf + 1, lstGear.SelectedValue.ToString().Length - intIndexOf - 1);
                    objXmlGear = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + strValue + "\" and category = \"" + strCategory + "\"]");
                }
                if (objXmlGear == null)
                {
                    string strSelectedCategoryPath = string.Empty;
                    // If category selected is "Show All", we show all items regardless of category, otherwise we set the category string to filter for the selected category
                   if (cboCategory.SelectedValue != null && cboCategory.SelectedValue.ToString() != "Show All")
                    {
                        strSelectedCategoryPath = " and category = \"" + cboCategory.SelectedValue + "\"";
                    }
                    else
                    {
                        strCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(_strAllowedCategories))
                        {
                            string[] strAllowed = _strAllowedCategories.Split(',');
                            for (int index = 0; index < strAllowed.Length; index++)
                            {
                                if (index == 0)
                                {
                                    strSelectedCategoryPath = $"category = \"{strAllowed[index]}\"";
                                }
                                string strAllowedMount = strAllowed[index];
                                if (!string.IsNullOrEmpty(strAllowedMount))
                                    strSelectedCategoryPath += $" or category = \"{strAllowed[index]}\"";
                            }
                            strSelectedCategoryPath = " and (" + strSelectedCategoryPath + ")";
                        }
                    }
                    objXmlGear = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + lstGear.SelectedValue + "\"" + strSelectedCategoryPath + "]");
                }

                if (_objCharacter.DEPEnabled)
                {
                    if ((strCategory == "Matrix Programs" || strCategory == "Skillsofts" || strCategory == "Autosofts" || strCategory == "Autosofts, Agent" || strCategory == "Autosofts, Drone") && _objCharacter.Options.BookEnabled("UN") && !lstGear.SelectedValue.ToString().StartsWith("Suite:"))
                        chkInherentProgram.Visible = true;
                    else
                        chkInherentProgram.Visible = false;

                    chkInherentProgram.Enabled = !chkHacked.Checked;
                    if (!chkInherentProgram.Enabled)
                        chkInherentProgram.Checked = false;
                }
                else
                    chkInherentProgram.Visible = false;

                if (objXmlGear["devicerating"] != null)
                    lblGearDeviceRating.Text = objXmlGear["devicerating"].InnerText;
                else
                    lblGearDeviceRating.Text = string.Empty;

                /*if (objXmlGear["category"].InnerText.EndsWith("Software") || objXmlGear["category"].InnerText.EndsWith("Programs") || objXmlGear["category"].InnerText == "Program Options" || objXmlGear["category"].InnerText.StartsWith("Autosofts") || objXmlGear["category"].InnerText.StartsWith("Skillsoft") || objXmlGear["category"].InnerText == "Program Packages" || objXmlGear["category"].InnerText == "Software Suites")
                    chkHacked.Visible = true;
                else
                    chkHacked.Visible = false;*/

                string strBook = _objCharacter.Options.LanguageBookShort(objXmlGear["source"].InnerText);
                string strPage = objXmlGear["page"].InnerText;
                if (objXmlGear["altpage"] != null)
                    strPage = objXmlGear["altpage"].InnerText;
                lblSource.Text = strBook + " " + strPage;

                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                // Avail.
                // If avail contains "F" or "R", remove it from the string so we can use the expression.
                string strAvail = string.Empty;
                string strAvailExpr = string.Empty;
                string strPrefix = string.Empty;
                XmlNode objAvailNode = objXmlGear["avail"];
                if (objAvailNode == null)
                {
                    int intHighestAvailNode = 0;
                    foreach (XmlNode objLoopNode in objXmlGear.ChildNodes)
                    {
                        if (objLoopNode.NodeType == XmlNodeType.Element && objLoopNode.Name.StartsWith("avail"))
                        {
                            string strLoopCostString = objLoopNode.Name.Substring(4);
                            int intTmp;
                            if (int.TryParse(strLoopCostString, out intTmp))
                            {
                                intHighestAvailNode = Math.Max(intHighestAvailNode, intTmp);
                            }
                        }
                    }
                    objAvailNode = objXmlGear["avail" + intHighestAvailNode];
                    for (int i = decimal.ToInt32(nudRating.Value); i <= intHighestAvailNode; ++i)
                    {
                        XmlNode objLoopNode = objXmlGear["avail" + i.ToString(GlobalOptions.InvariantCultureInfo)];
                        if (objLoopNode != null)
                        {
                            objAvailNode = objLoopNode;
                            break;
                        }
                    }
                }
                strAvailExpr = objAvailNode.InnerText;
                
                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    if (strAvail == "R")
                        strAvail = LanguageManager.GetString("String_AvailRestricted");
                    else if (strAvail == "F")
                        strAvail = LanguageManager.GetString("String_AvailForbidden");
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                if (strAvailExpr.Substring(0, 1) == "+")
                {
                    strPrefix = "+";
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }

                try
                {
                    lblAvail.Text = strPrefix + (Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)))) + _intAvailModifier).ToString() + strAvail;
                }
                catch (XPathException)
                {
                    lblAvail.Text = strPrefix + strAvailExpr + strAvail;
                }

                decimal decMultiplier = nudGearQty.Value / nudGearQty.Increment;
                if (chkDoItYourself.Checked)
                    decMultiplier *= 0.5m;
                
                // Cost.
                if (chkFreeItem.Checked)
                {
                    lblCost.Text = "0.00¥";
                    decItemCost = 0;
                }
                else
                {
                    XmlNode objCostNode = objXmlGear["cost"];
                    if (objCostNode == null)
                    {
                        int intHighestCostNode = 0;
                        foreach (XmlNode objLoopNode in objXmlGear.ChildNodes)
                        {
                            if (objLoopNode.NodeType == XmlNodeType.Element && objLoopNode.Name.StartsWith("cost"))
                            {
                                string strLoopCostString = objLoopNode.Name.Substring(4);
                                int intTmp;
                                if (int.TryParse(strLoopCostString, out intTmp))
                                {
                                    intHighestCostNode = Math.Max(intHighestCostNode, intTmp);
                                }
                            }
                        }
                        objCostNode = objXmlGear["cost" + intHighestCostNode];
                        for (int i = decimal.ToInt32(nudRating.Value); i <= intHighestCostNode; ++i)
                        {
                            XmlNode objLoopNode = objXmlGear["cost" + i.ToString(GlobalOptions.InvariantCultureInfo)];
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
                            decimal decCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(objCostNode.InnerText.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))), GlobalOptions.InvariantCultureInfo) * decMultiplier;
                            decCost *= 1 + (nudMarkup.Value / 100.0m);
                            if (chkBlackMarketDiscount.Checked)
                                decCost *= 0.9m;
                            if (chkHacked.Checked)
                                decCost *= 0.1m;
                            lblCost.Text = (decCost * _intCostMultiplier).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                            decItemCost = decCost;
                        }
                        catch (XPathException)
                        {
                            lblCost.Text = objCostNode.InnerText;
                            decimal decTemp;
                            if (decimal.TryParse(objCostNode.InnerText, out decTemp))
                            {
                                decItemCost = decTemp;
                                lblCost.Text = (decItemCost * _intCostMultiplier).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                            }
                        }

                        if (objCostNode.InnerText.StartsWith("FixedValues"))
                        {
                            string[] strValues = objCostNode.InnerText.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                            string strCost = "0";
                            if (nudRating.Value > 0)
                                strCost = strValues[decimal.ToInt32(nudRating.Value) - 1].Trim("[]".ToCharArray());
                            decimal decCost = Convert.ToDecimal(strCost, GlobalOptions.InvariantCultureInfo) * decMultiplier;
                            decCost *= 1 + (nudMarkup.Value / 100.0m);
                            if (chkBlackMarketDiscount.Checked)
                                decCost *= 0.9m;
                            if (chkHacked.Checked)
                                decCost *= 0.1m;
                            lblCost.Text = (decCost * _intCostMultiplier).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                            decItemCost = decCost;
                        }
                        else if (objCostNode.InnerText.StartsWith("Variable"))
                        {
                            decimal decMin = 0;
                            decimal decMax = decimal.MaxValue;
                            string strCost = objCostNode.InnerText.TrimStart("Variable", true).Trim("()".ToCharArray());
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

                // Update the Avail Test Label.
                lblTest.Text = _objCharacter.AvailTest(decItemCost * _intCostMultiplier, lblAvail.Text);

                // Capacity.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = "0";
                string strCapacityField = "capacity";
                if (_blnShowArmorCapacityOnly)
                    strCapacityField = "armorcapacity";
                bool blnSquareBrackets = false;

                if (_objCapacityStyle == CapacityStyle.Standard)
                {
                    if (objXmlGear[strCapacityField] != null)
                    {
                        if (objXmlGear[strCapacityField].InnerText.Contains("/["))
                        {
                            int intPos = objXmlGear[strCapacityField].InnerText.IndexOf("/[");
                            string strFirstHalf = objXmlGear[strCapacityField].InnerText.Substring(0, intPos);
                            string strSecondHalf = objXmlGear[strCapacityField].InnerText.Substring(intPos + 1, objXmlGear[strCapacityField].InnerText.Length - intPos - 1);

                            blnSquareBrackets = strFirstHalf.Contains('[');
                            strCapacity = strFirstHalf;
                            if (blnSquareBrackets && strCapacity.Length > 2)
                                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                            if (objXmlGear[strCapacityField].InnerText == "[*]")
                                lblCapacity.Text = "*";
                            else
                            {
                                if (objXmlGear[strCapacityField].InnerText.StartsWith("FixedValues"))
                                {
                                    string[] strValues = objXmlGear[strCapacityField].InnerText.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                                    if (strValues.Length >= decimal.ToInt32(nudRating.Value))
                                        lblCapacity.Text = strValues[decimal.ToInt32(nudRating.Value) - 1];
                                    else
                                    {
                                        try
                                        {
                                            lblCapacity.Text = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)))).ToString("#,0.##", GlobalOptions.CultureInfo);
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
                                        lblCapacity.Text = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)))).ToString("#,0.##", GlobalOptions.CultureInfo);
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
                            if (blnSquareBrackets)
                                lblCapacity.Text = "[" + lblCapacity.Text + "]";

                            lblCapacity.Text += "/" + strSecondHalf;
                        }
                        else
                        {
                            blnSquareBrackets = objXmlGear[strCapacityField].InnerText.Contains('[');
                            strCapacity = objXmlGear[strCapacityField].InnerText;
                            if (blnSquareBrackets && strCapacity.Length > 2)
                                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                            if (objXmlGear[strCapacityField].InnerText == "[*]")
                                lblCapacity.Text = "*";
                            else
                            {
                                if (objXmlGear[strCapacityField].InnerText.StartsWith("FixedValues"))
                                {
                                    string[] strValues = objXmlGear[strCapacityField].InnerText.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                                    if (strValues.Length >= decimal.ToInt32(nudRating.Value))
                                        lblCapacity.Text = strValues[decimal.ToInt32(nudRating.Value) - 1];
                                    else
                                        lblCapacity.Text = "0";
                                }
                                else
                                {
                                    string strCalculatedCapacity = string.Empty;
                                    try
                                    {
                                        strCalculatedCapacity = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)))).ToString("#,0.##", GlobalOptions.CultureInfo);
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
                                    if (!string.IsNullOrEmpty(strCalculatedCapacity))
                                        lblCapacity.Text = strCalculatedCapacity;
                                }
                            }
                            if (blnSquareBrackets)
                                lblCapacity.Text = "[" + lblCapacity.Text + "]";
                        }
                    }
                    else
                    {
                        lblCapacity.Text = "0";
                    }
                }
                else if (_objCapacityStyle == CapacityStyle.Zero)
                    lblCapacity.Text = "[0]";
                else if (_objCapacityStyle == CapacityStyle.PerRating)
                {
                    if (nudRating.Value == 0)
                        lblCapacity.Text = "[1]";
                    else
                        lblCapacity.Text = "[" + nudRating.Value.ToString(GlobalOptions.CultureInfo) + "]";
                }

                // Rating.
                if (Convert.ToInt32(objXmlGear["rating"].InnerText) > 0)
                {
                    nudRating.Maximum = Convert.ToInt32(objXmlGear["rating"].InnerText);
                    if (objXmlGear["minrating"] != null)
                    {
                        decimal decOldMinimum = nudRating.Minimum;
                        nudRating.Minimum = Convert.ToInt32(objXmlGear["minrating"].InnerText);
                        if (decOldMinimum > nudRating.Minimum)
                        {
                            nudRating.Value -= decOldMinimum - nudRating.Minimum;
                        }
                    }
                    else
                    {
                        nudRating.Minimum = 1;
                    }
                    while (nudRating.Maximum > nudRating.Minimum && !Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlGear, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum), _intAvailModifier))
                    {
                        nudRating.Maximum -= 1;
                    }

                    if (nudRating.Minimum == nudRating.Maximum)
                        nudRating.Enabled = false;
                    else
                        nudRating.Enabled = true;
                }
                else
                {
                    nudRating.Minimum = 0;
                    nudRating.Maximum = 0;
                    nudRating.Enabled = false;
                }

                tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlGear["source"].InnerText) + " " + LanguageManager.GetString("String_Page") + " " + strPage);
            }
        }

        /// <summary>
        /// Add a Category to the Category list.
        /// </summary>
        public void AddCategory(string strCategories)
        {
            string[] strCategoryList = strCategories.Split(',');
            foreach (string strCategory in strCategoryList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = strCategory;
                objItem.Name = strCategory;
                _lstCategory.Add(objItem);
            }
            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            cboCategory.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (!string.IsNullOrEmpty(lstGear.Text))
            {
                XmlNode objNode = null;

                if (lstGear.SelectedValue.ToString().Contains('^'))
                {
                    // If the SelectedValue contains ^, then it also includes the English Category name which needs to be extracted.
                    int intIndexOf = lstGear.SelectedValue.ToString().IndexOf('^');
                    string strValue = lstGear.SelectedValue.ToString().Substring(0, intIndexOf);
                    string strCategory = lstGear.SelectedValue.ToString().Substring(intIndexOf + 1, lstGear.SelectedValue.ToString().Length - intIndexOf - 1);
                    objNode = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + strValue + "\" and category = \"" + strCategory + "\"]");
                }
                else if (cboCategory.SelectedValue?.ToString() != "Show All")
                    objNode = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + lstGear.SelectedValue + "\" and category = \"" + cboCategory.SelectedValue + "\"]");
                else
                    objNode = _objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + lstGear.SelectedValue + "\"]");

                _strSelectedGear = objNode["name"].InnerText;
                _strSelectedCategory = objNode["category"].InnerText;
                _strSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : _strSelectedCategory;
            }

            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            _intSelectedRating = decimal.ToInt32(nudRating.Value);
            _decSelectedQty = nudGearQty.Value;
            _decMarkup = nudMarkup.Value;

            if (!chkInherentProgram.Visible || !chkInherentProgram.Enabled)
                chkInherentProgram.Checked = false;

            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblCapacityLabel.Width, lblAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);
            intWidth = Math.Max(intWidth, lblRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblGearQtyLabel.Width);
            intWidth = Math.Max(intWidth, lblMarkupLabel.Width);

            lblCapacity.Left = lblCapacityLabel.Left + intWidth + 6;
            lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
            lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;
            nudRating.Left = lblRatingLabel.Left + intWidth + 6;
            nudGearQty.Left = lblGearQtyLabel.Left + intWidth + 6;
            chkStack.Left = nudGearQty.Left + nudGearQty.Width + 6;
            nudMarkup.Left = lblMarkupLabel.Left + intWidth + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblGearDeviceRating.Left = lblGearDeviceRatingLabel.Left + lblGearDeviceRatingLabel.Width + 6;

            chkDoItYourself.Left = chkFreeItem.Left + chkFreeItem.Width + 6;
            chkHacked.Left = chkDoItYourself.Left + chkDoItYourself.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
