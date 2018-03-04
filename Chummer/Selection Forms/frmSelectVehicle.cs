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

namespace Chummer
{
    public partial class frmSelectVehicle : Form
    {
        private string _strSelectedVehicle = string.Empty;
        private bool _blnUsedVehicle;
        private string _strUsedAvail = string.Empty;
        private decimal _decUsedCost;
        private decimal _decMarkup;

        private bool _blnAddAgain;
        private static string s_StrSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setBlackMarketMaps;
        private bool _blnBlackMarketDiscount;

        #region Control Events
        public frmSelectVehicle(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = XmlManager.Load("vehicles.xml").GetFastNavigator().SelectSingleNode("/chummer");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode);
        }

        private void frmSelectVehicle_Load(object sender, EventArgs e)
        {
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

            // Populate the Vehicle Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseVehicleDataNode.Select("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
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

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedVehicle();
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstVehicle_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void chkUsedVehicle_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedVehicle();
        }

        private void nudUsedVehicleDiscount_ValueChanged(object sender, EventArgs e)
        {
            UpdateSelectedVehicle();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedVehicle();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateSelectedVehicle();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedVehicle();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstVehicle.SelectedIndex + 1 < lstVehicle.Items.Count)
                {
                    lstVehicle.SelectedIndex++;
                }
                else if (lstVehicle.Items.Count > 0)
                {
                    lstVehicle.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstVehicle.SelectedIndex - 1 >= 0)
                {
                    lstVehicle.SelectedIndex--;
                }
                else if (lstVehicle.Items.Count > 0)
                {
                    lstVehicle.SelectedIndex = lstVehicle.Items.Count - 1;
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
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Name of Vehicle that was selected in the dialogue.
        /// </summary>
        public string SelectedVehicle => _strSelectedVehicle;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool UsedVehicle => _blnUsedVehicle;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// Cost of the Vehicle's cost by when it is used.
        /// </summary>
        public decimal UsedCost => _decUsedCost;

        /// <summary>
        /// Vehicle's Aavailability when it is used.
        /// </summary>
        public string UsedAvail => _strUsedAvail;

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        #endregion

        #region Methods
        /// <summary>
        /// Refresh the information for the selected Vehicle.
        /// </summary>
        private void UpdateSelectedVehicle()
        {
            string strSelectedId = lstVehicle.SelectedValue?.ToString();
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = \"" + strSelectedId + "\"]");
            }
            if (objXmlVehicle == null)
            {
                lblVehicleHandling.Text = string.Empty;
                lblVehicleAccel.Text = string.Empty;
                lblVehicleSpeed.Text = string.Empty;
                lblVehiclePilot.Text = string.Empty;
                lblVehicleBody.Text = string.Empty;
                lblVehicleArmor.Text = string.Empty;
                lblVehicleSeats.Text = string.Empty;
                lblVehicleSensor.Text = string.Empty;
                lblVehicleAvail.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblVehicleCost.Text = string.Empty;
                lblTest.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
                return;
            }

            decimal decCostModifier = 1.0m;
            if (chkUsedVehicle.Checked)
                decCostModifier -= (nudUsedVehicleDiscount.Value / 100.0m);

            lblVehicleHandling.Text = objXmlVehicle.SelectSingleNode("handling")?.Value;
            lblVehicleAccel.Text = objXmlVehicle.SelectSingleNode("accel")?.Value;
            lblVehicleSpeed.Text = objXmlVehicle.SelectSingleNode("speed")?.Value;
            lblVehiclePilot.Text = objXmlVehicle.SelectSingleNode("pilot")?.Value;
            lblVehicleBody.Text = objXmlVehicle.SelectSingleNode("body")?.Value;
            lblVehicleArmor.Text = objXmlVehicle.SelectSingleNode("armor")?.Value;
            lblVehicleSeats.Text = objXmlVehicle.SelectSingleNode("seats")?.Value;
            lblVehicleSensor.Text = objXmlVehicle.SelectSingleNode("sensor")?.Value;

            string strAvail = objXmlVehicle.SelectSingleNode("avail")?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(strAvail))
            {
                string strSuffix = string.Empty;
                char chrLastChar = strAvail.Length > 0 ? strAvail[strAvail.Length - 1] : ' ';
                if (chrLastChar == 'F')
                {
                    strSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }
                else if (chrLastChar == 'R')
                {
                    strSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }
                if (chkUsedVehicle.Checked)
                {
                    if (int.TryParse(strAvail, out int intTmp))
                        strAvail = intTmp + 4.ToString();
                }
                strAvail += strSuffix;
            }
            lblVehicleAvail.Text = strAvail;

            chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNode("category")?.Value);

            // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            string strCost = objXmlVehicle.SelectSingleNode("cost")?.Value ?? string.Empty;
            if (strCost.StartsWith("Variable"))
            {
                lblVehicleCost.Text = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                lblTest.Text = string.Empty;
            }
            else
            {
                decimal decCost = 0.0m;
                if (!chkFreeItem.Checked)
                {
                    if (decimal.TryParse(strCost, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decTmp))
                    {
                        decCost = decTmp;
                    }

                    // Apply the markup if applicable.
                    decCost *= decCostModifier;
                    decCost *= 1 + (nudMarkup.Value / 100.0m);

                    if (chkBlackMarketDiscount.Checked)
                    {
                        decCost *= 0.9m;
                    }
                }

                lblVehicleCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblTest.Text = _objCharacter.AvailTest(decCost, lblVehicleAvail.Text);
            }


            string strSource = objXmlVehicle.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objXmlVehicle.SelectSingleNode("altpage")?.Value ?? objXmlVehicle.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

            tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
        }

        private void RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + '\"';
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
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                }
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            BuildVehicleList(_xmlBaseVehicleDataNode.Select("vehicles/vehicle[" + strFilter + ']'));
        }

        private void BuildVehicleList(XPathNodeIterator objXmlVehicleList)
        {
            List<ListItem> lstVehicles = new List<ListItem>();
            foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
            {
                if (!chkHideOverAvailLimit.Checked || SelectionShared.CheckAvailRestriction(objXmlVehicle, _objCharacter))
                {
                    string strDisplayname = objXmlVehicle.SelectSingleNode("translate")?.Value ?? objXmlVehicle.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);

                    if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                    {
                        string strCategory = objXmlVehicle.SelectSingleNode("category")?.Value;
                        if (!string.IsNullOrEmpty(strCategory))
                        {
                            ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                            if (!string.IsNullOrEmpty(objFoundItem.Name))
                            {
                                strDisplayname += " [" + objFoundItem.Name + ']';
                            }
                        }
                    }
                    lstVehicles.Add(new ListItem(objXmlVehicle.SelectSingleNode("id")?.Value ?? string.Empty, strDisplayname));
                }
            }
            lstVehicles.Sort(CompareListItems.CompareNames);
            lstVehicle.BeginUpdate();
            lstVehicle.DataSource = null;
            lstVehicle.ValueMember = "Value";
            lstVehicle.DisplayMember = "Name";
            lstVehicle.DataSource = lstVehicles;
            lstVehicle.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstVehicle.SelectedValue?.ToString();
            XPathNavigator xmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                xmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = \"" + strSelectedId + "\"]");
            }
            if (xmlVehicle == null)
                return;

            if (chkUsedVehicle.Checked)
            {
                decimal decCost = Convert.ToDecimal(xmlVehicle.SelectSingleNode("cost")?.Value, GlobalOptions.InvariantCultureInfo);
                decCost *= 1 - (nudUsedVehicleDiscount.Value / 100.0m);

                _blnUsedVehicle = true;
                _strUsedAvail = lblVehicleAvail.Text.Replace(LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language), "R").Replace(LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language), "F");
                _decUsedCost = decCost;
            }

            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlVehicle.SelectSingleNode("category")?.Value;
            _strSelectedVehicle = strSelectedId;
            _decMarkup = nudMarkup.Value;

            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblVehicleHandlingLabel.Width, lblVehicleSpeedLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleBodyLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSensorLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleCostLabel.Width);

            lblVehicleHandling.Left = lblVehicleHandlingLabel.Left + intWidth + 6;
            lblVehicleSpeed.Left = lblVehicleSpeedLabel.Left + intWidth + 6;
            lblVehicleBody.Left = lblVehicleBodyLabel.Left + intWidth + 6;
            lblVehicleSensor.Left = lblVehicleSensorLabel.Left + intWidth + 6;
            lblVehicleAvail.Left = lblVehicleAvailLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblVehicleAvail.Left + lblVehicleAvail.Width + 16;
            lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            lblVehicleCost.Left = lblVehicleCostLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblVehicleAccelLabel.Width, lblVehiclePilotLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleArmorLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSeatsLabel.Width);

            lblVehicleAccelLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleAccel.Left = lblVehicleAccelLabel.Left + intWidth + 6;
            lblVehiclePilotLabel.Left = lblVehicleHandling.Left + 60;
            lblVehiclePilot.Left = lblVehiclePilotLabel.Left + intWidth + 6;
            lblVehicleArmorLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleArmor.Left = lblVehicleArmorLabel.Left + intWidth + 6;
            lblVehicleSeatsLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleSeats.Left = lblVehicleSeatsLabel.Left + intWidth + 6;

            lblUsedVehicleDiscountLabel.Left = chkUsedVehicle.Left + chkUsedVehicle.Width + 6;
            nudUsedVehicleDiscount.Left = lblUsedVehicleDiscountLabel.Left + lblUsedVehicleDiscountLabel.Width + 6;
            lblUsedVehicleDiscountPercentLabel.Left = nudUsedVehicleDiscount.Left + nudUsedVehicleDiscount.Width;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width + 6;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion
    }
}
