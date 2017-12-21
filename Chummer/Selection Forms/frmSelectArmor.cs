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
using System.Text;
// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class frmSelectArmor : Form
    {
        private string _strSelectedArmor = string.Empty;

        private bool _blnAddAgain;
        private static string s_StrSelectCategory = string.Empty;
        private decimal _decMarkup;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private int _intRating;
        private bool _blnBlackMarketDiscount;

        #region Control Events
        public frmSelectArmor(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Armor information.
            _objXmlDocument = XmlManager.Load("armor.xml");
        }

        private void frmSelectArmor_Load(object sender, EventArgs e)
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

            DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.TopRight,
                Format = _objCharacter.Options.NuyenFormat + '¥',
                NullValue = null
            };
            dataGridViewTextBoxColumn7.DefaultCellStyle = dataGridViewNuyenCellStyle;
            Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

            // Populate the Armor Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                string strInnerText = objXmlCategory.InnerText;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

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
            objArmor.Create(objXmlArmor, objNode, null, null, 0, lstWeapons, true, true, true);

            lblArmor.Text = objXmlArmor["translate"]?.InnerText ?? objXmlArmor["name"].InnerText;
            lblArmorValue.Text = objXmlArmor["armor"]?.InnerText;
            lblAvail.Text = objArmor.TotalAvail;

            if (objXmlArmor["rating"] != null)
            {
                nudRating.Maximum = Convert.ToInt32(objXmlArmor["rating"].InnerText);
                while (nudRating.Maximum > 1 && !Backend.SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum)))
                {
                    nudRating.Maximum -= 1;
                }
                lblRatingLabel.Visible = true;
                nudRating.Visible = true;
                nudRating.Enabled = true;
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
            RefreshList();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
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

            RefreshList();
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
                if (int.TryParse(e.CellValue1.ToString(), out int intTmp1) &&
                        int.TryParse(e.CellValue2.ToString(), out int intTmp2) &&
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
        /// Refreshes the displayed lists
        /// </summary>
        private void RefreshList()
        {
            string strFilter = "(" + _objCharacter.Options.BookXPath() + ")";

            string strCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + "\"";
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ")";
                }
            }

            if (txtSearch.TextLength != 0)
            {
                // Treat everything as being uppercase so the search is case-insensitive.
                string strSearchText = txtSearch.Text.ToUpper();
                strFilter += " and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\"))";
            }

            XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes("/chummer/armors/armor[" + strFilter + "]");
            BuildArmorList(objXmlArmorList);
        }

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
                        if (Backend.SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter, chkHideOverAvailLimit.Checked))
                        {
                            TreeNode objNode = new TreeNode();
                            Armor objArmor = new Armor(_objCharacter);
                            List<Weapon> lstWeapons = new List<Weapon>();
                            objArmor.Create(objXmlArmor, objNode, null, null, 0, lstWeapons, true, true, true);

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
                        if (Backend.SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter, chkHideOverAvailLimit.Checked))
                        {
                            string strDisplayName = objXmlArmor["translate"]?.InnerText ?? objXmlArmor["name"].InnerText;
                            if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                            {
                                string strCategory = objXmlArmor["category"]?.InnerText;
                                if (!string.IsNullOrEmpty(strCategory))
                                {
                                    ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == strCategory);
                                    if (!string.IsNullOrEmpty(objFoundItem.Name))
                                    {
                                        strDisplayName += " [" + objFoundItem.Name + "]";
                                    }
                                }
                            }

                            lstArmors.Add(new ListItem(objXmlArmor["id"]?.InnerText, strDisplayName));
                        }
                    }
                    lstArmors.Sort(CompareListItems.CompareNames);
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
                s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
                _strSelectedArmor = objNode["name"]?.InnerText;
                _decMarkup = nudMarkup.Value;
                _intRating = decimal.ToInt32(nudRating.Value);
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
            objArmor.Create(objXmlArmor, objNode, null, null, 0, lstWeapons, true, true, true);

            // Check for a Variable Cost.
            XmlElement xmlCostElement = objXmlArmor["cost"];
            if (xmlCostElement != null)
            {
                decimal decItemCost = 0.0m;
                if (chkFreeItem.Checked)
                {
                    lblCost.Text = 0.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
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
                        lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                    }
                    else
                        lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    decItemCost = decMin;
                }
                else if (xmlCostElement.InnerText.Contains("Rating"))
                {
                    decItemCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(xmlCostElement.InnerText.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))), GlobalOptions.InvariantCultureInfo);
                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);
                    if (chkBlackMarketDiscount.Checked)
                    {
                        decItemCost *= 0.9m;
                    }
                    lblCost.Text = decItemCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                }
                else
                {
                    decItemCost = Convert.ToDecimal(xmlCostElement.InnerText, GlobalOptions.InvariantCultureInfo);
                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);
                    if (chkBlackMarketDiscount.Checked)
                    {
                        decItemCost *= 0.9m;
                    }
                    lblCost.Text = decItemCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
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
