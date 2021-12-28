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
    public partial class frmSelectVehicle : Form
    {
        private string _strSelectedVehicle = string.Empty;
        private bool _blnUsedVehicle;
        private string _strUsedAvail = string.Empty;
        private decimal _decUsedCost;
        private decimal _decMarkup;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setDealerConnectionMaps = new HashSet<string>();
        private readonly HashSet<string> _setBlackMarketMaps;
        private bool _blnBlackMarketDiscount;

        #region Control Events

        public frmSelectVehicle(Character objCharacter)
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
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode);

            if (_objCharacter.DealerConnectionDiscount)
            {
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.DealerConnection))
                {
                    _setDealerConnectionMaps.Add(objImprovement.UniqueName);
                }
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
                chkHideOverAvailLimit.Text = string.Format(GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
            }

            // Populate the Vehicle Category list.
            string strFilterPrefix = "vehicles/vehicle[(" + _objCharacter.Settings.BookXPath() + ") and category = ";
            foreach (XPathNavigator objXmlCategory in _xmlBaseVehicleDataNode.SelectAndCacheExpression("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if (_xmlBaseVehicleDataNode.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + "]") != null)
                {
                    _lstCategory.Add(new ListItem(strInnerText,
                        objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            _blnLoading = false;
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();
        }

        private void RefreshCurrentList(object sender, EventArgs e)
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

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void ProcessVehicleCostsChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                RefreshList();
            UpdateSelectedVehicleCost();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
                RefreshList();
            UpdateSelectedVehicleCost();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstVehicle.SelectedIndex + 1 < lstVehicle.Items.Count:
                    lstVehicle.SelectedIndex++;
                    break;

                case Keys.Down:
                    {
                        if (lstVehicle.Items.Count > 0)
                        {
                            lstVehicle.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstVehicle.SelectedIndex - 1 >= 0:
                    lstVehicle.SelectedIndex--;
                    break;

                case Keys.Up:
                    {
                        if (lstVehicle.Items.Count > 0)
                        {
                            lstVehicle.SelectedIndex = lstVehicle.Items.Count - 1;
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
        /// Vehicle's Availability when it is used.
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

        #endregion Properties

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
                // Retrieve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + "]");
            }
            if (objXmlVehicle == null)
            {
                tlpRight.Visible = false;
                return;
            }

            SuspendLayout();
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
            AvailabilityValue objTotalAvail = new AvailabilityValue(0, objXmlVehicle.SelectSingleNode("avail")?.Value, chkUsedVehicle.Checked ? -4 : 0);
            lblVehicleAvail.Text = objTotalAvail.ToString();
            lblVehicleAvailLabel.Visible = !string.IsNullOrEmpty(lblVehicleAvail.Text);

            bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNode("category")?.Value);
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

            UpdateSelectedVehicleCost();

            string strSource = objXmlVehicle.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
            string strPage = objXmlVehicle.SelectSingleNode("altpage")?.Value ?? objXmlVehicle.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
            SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language,
                GlobalSettings.CultureInfo, _objCharacter);
            lblSource.Text = objSource.ToString();
            lblSource.SetToolTip(objSource.LanguageBookTooltip);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            tlpRight.Visible = true;
            ResumeLayout();
        }

        /// <summary>
        /// Refresh the cost information for the selected Vehicle.
        /// </summary>
        private void UpdateSelectedVehicleCost()
        {
            string strSelectedId = lstVehicle.SelectedValue?.ToString();
            XPathNavigator objXmlVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected Vehicle.
                objXmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + "]");
            }
            if (objXmlVehicle == null)
            {
                tlpRight.Visible = false;
                return;
            }

            // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            string strCost = objXmlVehicle.SelectSingleNode("cost")?.Value ?? string.Empty;
            if (strCost.StartsWith("Variable", StringComparison.Ordinal))
            {
                lblVehicleCost.Text = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                lblVehicleCostLabel.Visible = !string.IsNullOrEmpty(lblVehicleCost.Text);
                lblTest.Text = string.Empty;
                lblTestLabel.Visible = false;
            }
            else
            {
                decimal decCost = 0.0m;
                if (!chkFreeItem.Checked && decimal.TryParse(strCost, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decTmp))
                {
                    decCost = decTmp;

                    if (chkUsedVehicle.Checked)
                        decCost *= 1.0m - (nudUsedVehicleDiscount.Value / 100.0m);
                    // Apply the markup if applicable.
                    decCost *= 1 + (nudMarkup.Value / 100.0m);

                    if (chkBlackMarketDiscount.Checked)
                        decCost *= 0.9m;
                    if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps, objXmlVehicle.SelectSingleNode("category")?.Value))
                        decCost *= 0.9m;
                }

                lblVehicleCost.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                lblVehicleCostLabel.Visible = !string.IsNullOrEmpty(lblVehicleCost.Text);
                lblTest.Text = _objCharacter.AvailTest(decCost, lblVehicleAvail.Text);
                lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);
            }
        }

        private void RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || txtSearch.TextLength == 0))
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                else
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value))
                        {
                            if (!string.IsNullOrEmpty(strItem))
                                sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                        }
                    }
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }
            BuildVehicleList(_xmlBaseVehicleDataNode.Select("vehicles/vehicle" + strFilter));
        }

        private void BuildVehicleList(XPathNodeIterator objXmlVehicleList)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            int intOverLimit = 0;
            List<ListItem> lstVehicles = new List<ListItem>();
            foreach (XPathNavigator objXmlVehicle in objXmlVehicleList)
            {
                if (chkHideOverAvailLimit.Checked && !objXmlVehicle.CheckAvailRestriction(_objCharacter))
                {
                    ++intOverLimit;
                    continue;
                }
                if (!chkFreeItem.Checked && chkShowOnlyAffordItems.Checked)
                {
                    decimal decCostMultiplier = 1.0m;
                    if (chkUsedVehicle.Checked)
                        decCostMultiplier -= (nudUsedVehicleDiscount.Value / 100.0m);
                    decCostMultiplier *= 1 + (nudMarkup.Value / 100.0m);
                    if (chkBlackMarketDiscount.Checked && _setBlackMarketMaps.Contains(objXmlVehicle.SelectSingleNodeAndCacheExpression("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    if (Vehicle.DoesDealerConnectionApply(_setDealerConnectionMaps, objXmlVehicle.SelectSingleNodeAndCacheExpression("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    if (!objXmlVehicle.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier))
                    {
                        ++intOverLimit;
                        continue;
                    }
                }

                string strDisplayname = objXmlVehicle.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlVehicle.SelectSingleNodeAndCacheExpression("name")?.Value ?? LanguageManager.GetString("String_Unknown");

                if (!GlobalSettings.SearchInCategoryOnly && txtSearch.TextLength != 0)
                {
                    string strCategory = objXmlVehicle.SelectSingleNodeAndCacheExpression("category")?.Value;
                    if (!string.IsNullOrEmpty(strCategory))
                    {
                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayname += strSpace + '[' + objFoundItem.Name + ']';
                        }
                    }
                }
                lstVehicles.Add(new ListItem(objXmlVehicle.SelectSingleNodeAndCacheExpression("id")?.Value ?? string.Empty, strDisplayname));
            }
            lstVehicles.Sort(CompareListItems.CompareNames);
            if (intOverLimit > 0)
            {
                // Add after sort so that it's always at the end
                lstVehicles.Add(new ListItem(string.Empty,
                    string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_RestrictedItemsHidden"),
                        intOverLimit)));
            }
            string strOldSelected = lstVehicle.SelectedValue?.ToString();
            _blnLoading = true;
            lstVehicle.BeginUpdate();
            lstVehicle.PopulateWithListItems(lstVehicles);
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
                xmlVehicle = _xmlBaseVehicleDataNode.SelectSingleNode("vehicles/vehicle[id = " + strSelectedId.CleanXPath() + "]");
            }
            if (xmlVehicle == null)
                return;

            if (chkUsedVehicle.Checked)
            {
                decimal decCost = Convert.ToDecimal(xmlVehicle.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                decCost *= 1 - (nudUsedVehicleDiscount.Value / 100.0m);

                _blnUsedVehicle = true;
                _strUsedAvail = lblVehicleAvail.Text.Replace(LanguageManager.GetString("String_AvailRestricted"), "R").Replace(LanguageManager.GetString("String_AvailForbidden"), "F");
                _decUsedCost = decCost;
            }

            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlVehicle.SelectSingleNode("category")?.Value;
            _strSelectedVehicle = strSelectedId;
            _decMarkup = nudMarkup.Value;

            DialogResult = DialogResult.OK;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
