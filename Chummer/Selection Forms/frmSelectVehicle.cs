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

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private static string s_StrSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setDealerConnectionMaps = new HashSet<string>();
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
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = XmlManager.Load("vehicles.xml").GetFastNavigator().SelectSingleNode("/chummer");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode);

            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(imp =>
                imp.Enabled && imp.ImproveType == Improvement.ImprovementType.DealerConnection))
            {
                _setDealerConnectionMaps.Add(objImprovement.UniqueName);
            }
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
                chkHideOverAvailLimit.Text = string.Format(chkHideOverAvailLimit.Text, _objCharacter.MaximumAvailability.ToString(GlobalOptions.CultureInfo));
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
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            _blnLoading = false;
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();
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
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                RefreshList();
            UpdateSelectedVehicle();
        }

        private void nudUsedVehicleDiscount_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                RefreshList();
            UpdateSelectedVehicle();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
                RefreshList();
            UpdateSelectedVehicle();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                RefreshList();
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

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void chkShowOnlyAffordItems_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList();
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
            if (_blnLoading)
                return;
            
            string strSelectedId = lstVehicle.SelectedValue?.ToString();
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = \"" + strSelectedId + "\"]");
            }
            if (objXmlVehicle == null)
            {
                lblVehicleHandlingLabel.Visible = false;
                lblVehicleAccelLabel.Visible = false;
                lblVehicleSpeedLabel.Visible = false;
                lblVehiclePilotLabel.Visible = false;
                lblVehicleBodyLabel.Visible = false;
                lblVehicleArmorLabel.Visible = false;
                lblVehicleSeatsLabel.Visible = false;
                lblVehicleSensorLabel.Visible = false;
                lblVehicleAvailLabel.Visible = false;
                lblSourceLabel.Visible = false;
                lblVehicleCostLabel.Visible = false;
                lblTestLabel.Visible = false;
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
                lblSource.SetToolTip(string.Empty);
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
            lblVehicleHandlingLabel.Visible = !string.IsNullOrEmpty(lblVehicleHandling.Text);
            lblVehicleAccelLabel.Visible = !string.IsNullOrEmpty(lblVehicleAccel.Text);
            lblVehicleSpeedLabel.Visible = !string.IsNullOrEmpty(lblVehicleSpeed.Text);
            lblVehiclePilotLabel.Visible = !string.IsNullOrEmpty(lblVehiclePilot.Text);
            lblVehicleBodyLabel.Visible = !string.IsNullOrEmpty(lblVehicleBody.Text);
            lblVehicleArmorLabel.Visible = !string.IsNullOrEmpty(lblVehicleArmor.Text);
            lblVehicleSeatsLabel.Visible = !string.IsNullOrEmpty(lblVehicleSeats.Text);
            lblVehicleSensorLabel.Visible = !string.IsNullOrEmpty(lblVehicleSensor.Text);
            AvailabilityValue objTotalAvail = new AvailabilityValue(0, objXmlVehicle.SelectSingleNode("avail")?.Value);

            if (chkUsedVehicle.Checked)
            {
                objTotalAvail.Value -= 4;
            }
            lblVehicleAvail.Text = objTotalAvail.ToString();
            lblVehicleAvailLabel.Visible = !string.IsNullOrEmpty(lblVehicleAvail.Text);

            chkBlackMarketDiscount.Enabled = _objCharacter.BlackMarketDiscount;

            if (!chkBlackMarketDiscount.Checked)
            {
                chkBlackMarketDiscount.Checked = GlobalOptions.AssumeBlackMarket &&
                                                 _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNode("category")
                                                     ?.Value);
            }
            else if (!_setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNode("category")?.Value))
            {
                //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                chkBlackMarketDiscount.Checked = false;
            }

            // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            string strCost = objXmlVehicle.SelectSingleNode("cost")?.Value ?? string.Empty;
            if (strCost.StartsWith("Variable"))
            {
                lblVehicleCost.Text = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                lblVehicleCostLabel.Visible = !string.IsNullOrEmpty(lblVehicleCost.Text);
                lblTest.Text = string.Empty;
                lblTestLabel.Visible = false;
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
                    if (_setDealerConnectionMaps != null)
                    {
                        if (_setDealerConnectionMaps.Any(set => objXmlVehicle.SelectSingleNode("category")?.Value.StartsWith(set) == true))
                        {
                            decCost *= 0.9m;
                        }
                    }
                }

                lblVehicleCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleCostLabel.Visible = !string.IsNullOrEmpty(lblVehicleCost.Text);
                lblTest.Text = _objCharacter.AvailTest(decCost, lblVehicleAvail.Text);
                lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);
            }


            string strSource = objXmlVehicle.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objXmlVehicle.SelectSingleNode("altpage")?.Value ?? objXmlVehicle.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
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
                if (chkHideOverAvailLimit.Checked && !SelectionShared.CheckAvailRestriction(objXmlVehicle, _objCharacter))
                    continue;
                if (!chkFreeItem.Checked && chkShowOnlyAffordItems.Checked)
                {
                    decimal decCostMultiplier = 1.0m;
                    if (chkUsedVehicle.Checked)
                        decCostMultiplier -= (nudUsedVehicleDiscount.Value / 100.0m);
                    decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    if (_setDealerConnectionMaps?.Any(set => objXmlVehicle.SelectSingleNode("category")?.Value.StartsWith(set) == true) == true)
                        decCostMultiplier *= 0.9m;
                    if (!SelectionShared.CheckNuyenRestriction(objXmlVehicle, _objCharacter.Nuyen, decCostMultiplier))
                        continue;
                }

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
            lstVehicles.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstVehicle.SelectedValue?.ToString();
            _blnLoading = true;
            lstVehicle.BeginUpdate();
            lstVehicle.ValueMember = "Value";
            lstVehicle.DisplayMember = "Name";
            lstVehicle.DataSource = lstVehicles;
            _blnLoading = false;
            if (string.IsNullOrEmpty(strOldSelected))
                lstVehicle.SelectedIndex = -1;
            else
                lstVehicle.SelectedValue = strOldSelected;
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

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }

        #endregion
    }
}
