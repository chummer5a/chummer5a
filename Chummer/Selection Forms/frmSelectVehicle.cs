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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectVehicle : Form
    {
        private string _strSelectedVehicle = string.Empty;
        private bool _blnUsedVehicle = false;
        private string _strUsedAvail = string.Empty;
        private decimal _decUsedCost = 0;
        private decimal _decMarkup = 0;

        private bool _blnAddAgain = false;
        private static string s_StrSelectCategory = string.Empty;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private bool _blnBlackMarketDiscount;

        #region Control Events
        public frmSelectVehicle(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Vehicle information.
            _objXmlDocument = XmlManager.Load("vehicles.xml");
        }

        private void frmSelectVehicle_Load(object sender, EventArgs e)
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

            // Populate the Vehicle Category list.
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
            if (!string.IsNullOrEmpty(lstVehicle.Text))
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
            cmdOK_Click(sender, e);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
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
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Name of Vehicle that was selected in the dialogue.
        /// </summary>
        public string SelectedVehicle
        {
            get
            {
                return _strSelectedVehicle;
            }
        }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool UsedVehicle
        {
            get
            {
                return _blnUsedVehicle;
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
        /// Cost of the Vehicle's cost by when it is used.
        /// </summary>
        public decimal UsedCost
        {
            get
            {
                return _decUsedCost;
            }
        }

        /// <summary>
        /// Vehicle's Aavailability when it is used.
        /// </summary>
        public string UsedAvail
        {
            get
            {
                return _strUsedAvail;
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
        #endregion

        #region Methods
        /// <summary>
        /// Refresh the information for the selected Vehicle.
        /// </summary>
        private void UpdateSelectedVehicle()
        {
            if (string.IsNullOrEmpty(lstVehicle.Text))
                return;

            decimal decCostModifier = 1.0m;

            // Retireve the information for the selected Vehicle.
            XmlNode objXmlVehicle = _objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + lstVehicle.SelectedValue + "\"]");
            if (objXmlVehicle == null)
                return;

            if (chkUsedVehicle.Checked)
                decCostModifier = 1.0m - (nudUsedVehicleDiscount.Value / 100.0m);

            lblVehicleHandling.Text = objXmlVehicle["handling"]?.InnerText;
            lblVehicleAccel.Text = objXmlVehicle["accel"]?.InnerText;
            lblVehicleSpeed.Text = objXmlVehicle["speed"]?.InnerText;
            lblVehiclePilot.Text = objXmlVehicle["pilot"]?.InnerText;
            lblVehicleBody.Text = objXmlVehicle["body"]?.InnerText;
            lblVehicleArmor.Text = objXmlVehicle["armor"]?.InnerText;
            lblVehicleSeats.Text = objXmlVehicle["seats"]?.InnerText;
            lblVehicleSensor.Text = objXmlVehicle["sensor"]?.InnerText;

            string strAvail = objXmlVehicle["avail"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(strAvail))
            {
                if (chkUsedVehicle.Checked)
                {
                    string strSuffix = string.Empty;
                    if (strAvail.EndsWith('R') || strAvail.EndsWith('F'))
                    {
                        strSuffix = strAvail.Substring(strAvail.Length - 1, 1);
                        // Translate the Avail string.
                        if (strSuffix == "R")
                            strSuffix = LanguageManager.GetString("String_AvailRestricted");
                        else if (strSuffix == "F")
                            strSuffix = LanguageManager.GetString("String_AvailForbidden");
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }
                    if (int.TryParse(strAvail, out int intTmp))
                        strAvail = (intTmp + 4).ToString() + strSuffix;
                }
            }
            lblVehicleAvail.Text = strAvail;

            // Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            if (objXmlVehicle["cost"]?.InnerText.StartsWith("Variable") == true)
            {
                lblVehicleCost.Text = objXmlVehicle["cost"].InnerText;
                lblTest.Text = string.Empty;
            }
            else
            {
                decimal decCost = 0.0m;
                if (!chkFreeItem.Checked)
                {
                    objXmlVehicle.TryGetDecFieldQuickly("cost", ref decCost);

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


            string strBook = _objCharacter.Options.LanguageBookShort(objXmlVehicle["source"]?.InnerText);
            string strPage = objXmlVehicle["page"]?.InnerText;
            if (objXmlVehicle["altpage"] != null)
                strPage = objXmlVehicle["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlVehicle["source"]?.InnerText) + " " + LanguageManager.GetString("String_Page") + " " + strPage);
        }

        private void RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = "(" + _objCharacter.Options.BookXPath() + ")";
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

            BuildVehicleList(_objXmlDocument.SelectNodes("/chummer/vehicles/vehicle[" + strFilter + "]"));
        }

        private void BuildVehicleList(XmlNodeList objXmlVehicleList)
        {
            List<ListItem> lstVehicles = new List<ListItem>();
            foreach (XmlNode objXmlVehicle in objXmlVehicleList)
            {
                if (Backend.SelectionShared.CheckAvailRestriction(objXmlVehicle, _objCharacter, chkHideOverAvailLimit.Checked))
                {
                    string strName = objXmlVehicle["name"]?.InnerText;
                    string strDisplayname = objXmlVehicle["translate"]?.InnerText ?? strName;

                    if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                    {
                        string strCategory = objXmlVehicle["category"]?.InnerText;
                        if (!string.IsNullOrEmpty(strCategory))
                        {
                            ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == strCategory);
                            if (!string.IsNullOrEmpty(objFoundItem.Name))
                            {
                                strDisplayname += " [" + objFoundItem.Name + "]";
                            }
                        }
                    }
                    lstVehicles.Add(new ListItem(strName, strDisplayname));
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
            XmlNode objXmlVehicle = _objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + lstVehicle.SelectedValue + "\"]");
            if (objXmlVehicle == null)
                return;

            if (chkUsedVehicle.Checked)
            {
                decimal decCostModifier = 1 - (nudUsedVehicleDiscount.Value / 100.0m);
                decimal decCost = Convert.ToDecimal(objXmlVehicle["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                decCost *= decCostModifier;

                _blnUsedVehicle = true;
                _strUsedAvail = lblVehicleAvail.Text.Replace(LanguageManager.GetString("String_AvailRestricted"), "R").Replace(LanguageManager.GetString("String_AvailForbidden"), "F");
                _decUsedCost = decCost;
            }

            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objXmlVehicle["category"]?.InnerText;
            _strSelectedVehicle = objXmlVehicle["name"]?.InnerText;
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

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
