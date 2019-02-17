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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
 using Chummer.Backend.Equipment;
// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class frmSelectArmor : Form
    {
        private string _strSelectedArmor = string.Empty;

        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;
        private decimal _decMarkup;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory = new List<ListItem>();
        private int _intRating;
        private bool _blnBlackMarketDiscount;

        #region Control Events
        public frmSelectArmor(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Armor information.
            _objXmlDocument = XmlManager.Load("armor.xml");
        }

        private void frmSelectArmor_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }
            chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}",
                    _objCharacter.MaximumAvailability.ToString());
            chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;

            // Populate the Armor Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            if (objXmlCategoryList != null)
                foreach (XmlNode objXmlCategory in objXmlCategoryList)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlCategory.InnerText;
                    objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                    _lstCategory.Add(objItem);
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
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void lstArmor_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstArmor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstArmor.Text))
                return;

            // Get the information for the selected piece of Armor.
            XmlNode objXmlArmor = _objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = \"" + lstArmor.SelectedValue + "\"]");
            if (objXmlArmor == null) return;
            // Create the Armor so we can show its Total Avail (some Armor includes Chemical Seal which adds +6 which wouldn't be factored in properly otherwise).
            Armor objArmor = new Armor(_objCharacter);
            TreeNode objNode = new TreeNode();
            List<Weapon> lstWeapons = new List<Weapon>();
            objArmor.Create(objXmlArmor, objNode, null, 0, lstWeapons, true, true, true);

            lblArmor.Text = objXmlArmor["translate"]?.InnerText ?? objXmlArmor["name"].InnerText;
            lblArmorValue.Text = objXmlArmor["armor"]?.InnerText;
            lblAvail.Text = objArmor.TotalAvail;

            if (objXmlArmor["rating"] != null)
            {
                lblRatingLabel.Visible = true;
                nudRating.Visible = true;
                nudRating.Enabled = true;
                nudRating.Maximum = Convert.ToInt32(objXmlArmor["rating"].InnerText);
                nudRating.Minimum = 1;
                nudRating.Value = 1;
            }
            else
            {
                lblRatingLabel.Visible = false;
                nudRating.Visible = false;
                nudRating.Enabled = false;
                nudRating.Minimum = 0;
                nudRating.Value = 0;
            }
            UpdateArmorInfo();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch_TextChanged(sender, e);
                return;
            }
            string strSelectedCategoryPath = string.Empty;
            // If category selected is "Show All", we show all items regardless of category, otherwise we set the category string to filter for the selected category
            if (cboCategory.SelectedValue != null && cboCategory.SelectedValue.ToString() != "Show All")
            {
                strSelectedCategoryPath = "category = \"" + cboCategory.SelectedValue + "\" and ";
            }
            XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes("/chummer/armors/armor[" + strSelectedCategoryPath + "(" + _objCharacter.Options.BookXPath() + ")]");
            BuildArmorList(objXmlArmorList);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }

            string strCategoryFilter = string.Empty;

            if (cboCategory.SelectedValue != null && cboCategory.SelectedValue.ToString() != "Show All")
                strCategoryFilter = "category = \"" + cboCategory.SelectedValue + "\"";
            else
            {
                foreach (object objListItem in cboCategory.Items)
                {
                    ListItem objItem = (ListItem)objListItem;
                    if (!string.IsNullOrEmpty(objItem.Value))
                        strCategoryFilter += "category = \"" + objItem.Value + "\" or ";
                }
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
            if (!string.IsNullOrEmpty(strCategoryFilter))
                strSearch += " and (" + strCategoryFilter + ")";
            // Remove the trailing " or )";
            if (strSearch.EndsWith(" or )"))
            {
                strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
            }
            strSearch += "]";

            XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes(strSearch);
            BuildArmorList(objXmlArmorList);
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateArmorInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateArmorInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateArmorInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateArmorInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstArmor.SelectedIndex + 1 < lstArmor.Items.Count)
                {
                    lstArmor.SelectedIndex++;
                }
                else if (lstArmor.Items.Count > 0)
                {
                    lstArmor.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstArmor.SelectedIndex - 1 >= 0)
                {
                    lstArmor.SelectedIndex--;
                }
                else if (lstArmor.Items.Count > 0)
                {
                    lstArmor.SelectedIndex = lstArmor.Items.Count - 1;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void tmrSearch_Tick(object sender, EventArgs e)
        {
            tmrSearch.Stop();
            tmrSearch.Enabled = false;

            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }

            string strCategoryFilter = string.Empty;

            foreach (object objListItem in cboCategory.Items)
            {
                ListItem objItem = (ListItem)objListItem;
                if (!string.IsNullOrEmpty(objItem.Value))
                    strCategoryFilter += "category = \"" + objItem.Value + "\" or ";
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
            if (!string.IsNullOrEmpty(strCategoryFilter))
                strSearch += " and (" + strCategoryFilter + ")";
            // Remove the trailing " or )";
            if (strSearch.EndsWith(" or )"))
            {
                strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
            }
            strSearch += "]";

            XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes(strSearch);

            BuildArmorList(objXmlArmorList);
        }

        private void dgvArmor_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void dgvArmor_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Index == 1)
            {
                int intResult = 1;
                int intTmp1;
                int intTmp2;
                if (int.TryParse(e.CellValue1.ToString(), out intTmp1) &&
                        int.TryParse(e.CellValue2.ToString(), out intTmp2) &&
                        intTmp1 < intTmp2)
                    intResult = -1;

                e.SortResult = intResult;
                e.Handled = true;
            }
        }
        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text, _objCharacter);
        }

        private void dgvArmor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            AcceptForm();
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
        /// Armor that was selected in the dialogue.
        /// </summary>
        public string SelectedArmor
        {
            get
            {
                return _strSelectedArmor;
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
        /// Markup percentage.
        /// </summary>
        public int Rating
        {
            get
            {
                return _intRating;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Builds the list of Armors to render in the active tab.
        /// </summary>
        /// <param name="objXmlArmorList">XmlNodeList of Armors to render.</param>
        private void BuildArmorList(XmlNodeList objXmlArmorList)
        {
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    DataTable tabArmor = new DataTable("armor");
                    tabArmor.Columns.Add("ArmorGuid");
                    tabArmor.Columns.Add("ArmorName");
                    tabArmor.Columns.Add("Armor");
                    tabArmor.Columns["Armor"].DataType = typeof(Int32);
                    tabArmor.Columns.Add("Capacity");
                    tabArmor.Columns["Capacity"].DataType = typeof(Int32);
                    tabArmor.Columns.Add("Avail");
                    tabArmor.Columns.Add("Special");
                    tabArmor.Columns.Add("Source");
                    tabArmor.Columns.Add("Cost");
                    tabArmor.Columns["Cost"].DataType = typeof(Int32);

                    // Populate the Armor list.
                    foreach (XmlNode objXmlArmor in objXmlArmorList)
                    {
                        if (Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter, chkHideOverAvailLimit.Checked,
                            Convert.ToInt32(nudRating.Value)))
                        {
                            TreeNode objNode = new TreeNode();
                            Armor objArmor = new Armor(_objCharacter);
                            List<Weapon> lstWeapons = new List<Weapon>();
                            objArmor.Create(objXmlArmor, objNode, null, 0, lstWeapons, true, true, true);

                            string strArmorGuid = objArmor.SourceID.ToString();
                            string strArmorName = objArmor.DisplayName;
                            int intArmor = objArmor.TotalArmor;
                            decimal decCapacity = Convert.ToDecimal(objArmor.CalculatedCapacity, GlobalOptions.CultureInfo);
                            string strAvail = objArmor.Avail;
                            string strAccessories = string.Empty;
                            foreach (ArmorMod objMod in objArmor.ArmorMods)
                            {
                                if (strAccessories.Length > 0)
                                    strAccessories += "\n";
                                strAccessories += objMod.DisplayName;
                            }
                            foreach (Gear objGear in objArmor.Gear)
                            {
                                if (strAccessories.Length > 0)
                                    strAccessories += "\n";
                                strAccessories += objGear.DisplayName;
                            }
                            string strSource = objArmor.Source + " " + objArmor.Page;
                            decimal decCost = objArmor.Cost;

                            tabArmor.Rows.Add(strArmorGuid, strArmorName, intArmor, decCapacity, strAvail, strAccessories,
                                strSource, decCost);
                        }
                    }

                    DataSet set = new DataSet("armor");
                    set.Tables.Add(tabArmor);

                    dgvArmor.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    dgvArmor.DataSource = set;
                    dgvArmor.DataMember = "armor";
                    break;
                default:
                    List<ListItem> lstArmors = new List<ListItem>();
                    foreach (XmlNode objXmlArmor in objXmlArmorList)
                    {
                        if (Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter, chkHideOverAvailLimit.Checked,
                            Convert.ToInt32(nudRating.Value)))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlArmor["id"]?.InnerText;
                            objItem.Name = objXmlArmor["translate"]?.InnerText ?? objXmlArmor["name"]?.InnerText;

                            if (objXmlArmor["category"] != null)
                            {
                                ListItem objFoundItem =
                                    _lstCategory.Find(objFind => objFind.Value == objXmlArmor["category"].InnerText);
                                if (objFoundItem != null)
                                {
                                    objItem.Name += " [" + objFoundItem.Name + "]";
                                }
                                lstArmors.Add(objItem);
                            }
                        }
                    }
                    SortListItem objSort = new SortListItem();
                    lstArmors.Sort(objSort.Compare);
                    lstArmor.BeginUpdate();
                    lstArmor.DataSource = null;
                    lstArmor.ValueMember = "Value";
                    lstArmor.DisplayMember = "Name";
                    lstArmor.DataSource = lstArmors;
                    lstArmor.EndUpdate();
                    break;
            }
        }
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            XmlNode objNode = null;
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    objNode =
                        _objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = \"" + lstArmor.SelectedValue + "\"]");
                    break;
                case 1:
                    objNode =
                        _objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = \"" +
                                                         dgvArmor.SelectedRows[0].Cells[0].Value + "\"]");
                    break;
            }
            if (objNode != null)
            {
                _strSelectCategory = objNode["category"]?.InnerText;
                _strSelectedArmor = objNode["name"]?.InnerText;
                _decMarkup = nudMarkup.Value;
                _intRating = Convert.ToInt32(nudRating.Value);
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

                DialogResult = DialogResult.OK;
            }
        }

        private void MoveControls()
        {
            int intWidth = lblArmorLabel.Width;
            intWidth = Math.Max(intWidth, lblCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);

            lblArmor.Left = lblArmorLabel.Left + intWidth + 6;
            lblCapacity.Left = lblCapacityLabel.Left + intWidth + 6;
            lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }

        private void UpdateArmorInfo()
        {
            // Get the information for the selected piece of Armor.
            XmlNode objXmlArmor = _objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = \"" + lstArmor.SelectedValue + "\"]");
            if (objXmlArmor == null) return;
            // Create the Armor so we can show its Total Avail (some Armor includes Chemical Seal which adds +6 which wouldn't be factored in properly otherwise).
            Armor objArmor = new Armor(_objCharacter);
            TreeNode objNode = new TreeNode();
            List<Weapon> lstWeapons = new List<Weapon>();
            objArmor.Create(objXmlArmor, objNode, null, 0, lstWeapons, true, true, true);

            // Check for a Variable Cost.
            XmlElement xmlCostElement = objXmlArmor["cost"];
            if (xmlCostElement != null)
            {
                decimal decItemCost = 0.0m;
                if (chkFreeItem.Checked)
                {
                    lblCost.Text = $"{0:###,###,##0.##¥}";
                    decItemCost = 0;
                }
                else if (xmlCostElement.InnerText.StartsWith("Variable"))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    string strCost = xmlCostElement.InnerText.TrimStart("Variable", true).Trim("()".ToCharArray());
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMax == decimal.MaxValue)
                    {
                        lblCost.Text = $"{decMin:###,###,##0.##¥+}";
                    }
                    else
                        lblCost.Text = $"{decMin:###,###,##0.##} - {decMax:###,###,##0.##¥}";

                    decItemCost = decMin;
                }
                else if (xmlCostElement.InnerText.Contains("Rating"))
                {
                    XPathNavigator nav = _objXmlDocument.CreateNavigator();
                    XPathExpression xprCost =
                        nav.Compile(xmlCostElement.InnerText.Replace("Rating",
                            nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                    decItemCost = (Convert.ToDecimal(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo));
                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);
                    if (chkBlackMarketDiscount.Checked)
                    {
                        decItemCost *= 0.9m;
                    }
                    lblCost.Text = $"{decItemCost:###,###,##0.##¥}";
                }
                else
                {
                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);
                    if (chkBlackMarketDiscount.Checked)
                    {
                        decItemCost *= 0.9m;
                    }
                    lblCost.Text = $"{decItemCost:###,###,##0.##¥}";
                }

                lblCapacity.Text = objXmlArmor["armorcapacity"]?.InnerText;

                lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);

                string strBook = _objCharacter.Options.LanguageBookShort(objXmlArmor["source"]?.InnerText);
                string strPage = objXmlArmor["page"]?.InnerText;
                if (objXmlArmor["altpage"] != null)
                    strPage = objXmlArmor["altpage"].InnerText;
                lblSource.Text = strBook + " " + strPage;


                tipTooltip.SetToolTip(lblSource,
                    _objCharacter.Options.LanguageBookLong(objXmlArmor["source"]?.InnerText) + " " +
                    LanguageManager.GetString("String_Page") + " " + strPage);
            }
        }
        #endregion
    }
}
