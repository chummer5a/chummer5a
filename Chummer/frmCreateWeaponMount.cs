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
using Chummer.Backend.Equipment;

namespace Chummer
{
	public partial class frmCreateWeaponMount : Form
	{
        private readonly List<VehicleMod> _lstMods = new List<VehicleMod>();
		private bool _blnLoading = true;
	    private readonly Vehicle _objVehicle;
	    private readonly Character _objCharacter;
	    private WeaponMount _objMount;
        private readonly XmlDocument _xmlDoc;

        public WeaponMount WeaponMount => _objMount;

	    public frmCreateWeaponMount(Vehicle objVehicle, Character objCharacter, WeaponMount objWeaponMount = null)
		{
            _objVehicle = objVehicle;
		    _objMount = objWeaponMount;
		    _objCharacter = objCharacter;
            _xmlDoc = _objCharacter.LoadData("vehicles.xml");
            InitializeComponent();
		}

        private void frmCreateWeaponMount_Load(object sender, EventArgs e)
        {
            XmlNode xmlVehicleNode = _objVehicle.GetNode();
            List<ListItem> lstSize = new List<ListItem>();
            // Populate the Weapon Mount Category list.
            string strSizeFilter = "category = \"Size\" and " + _objCharacter.Options.BookXPath();
            if (!_objVehicle.IsDrone && GlobalOptions.Dronemods)
                strSizeFilter += " and not(optionaldrone)";
            using (XmlNodeList xmlSizeNodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount[" + strSizeFilter + "]"))
                if (xmlSizeNodeList?.Count > 0)
                    foreach (XmlNode xmlSizeNode in xmlSizeNodeList)
                    {
                        string strId = xmlSizeNode["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XmlNode xmlTestNode = xmlSizeNode.SelectSingleNode("forbidden/vehicledetails");
                        if (xmlTestNode != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                continue;
                            }
                        }
                        xmlTestNode = xmlSizeNode.SelectSingleNode("required/vehicledetails");
                        if (xmlTestNode != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (!xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                continue;
                            }
                        }

                        lstSize.Add(new ListItem(strId, xmlSizeNode["translate"]?.InnerText ?? xmlSizeNode["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown")));
                    }

            cboSize.BeginUpdate();
            cboSize.ValueMember = nameof(ListItem.Value);
            cboSize.DisplayMember = nameof(ListItem.Name);
            cboSize.DataSource = lstSize;
            cboSize.Enabled = lstSize.Count > 1;
            cboSize.EndUpdate();

            if (_objMount != null)
            {
                TreeNode objModsParentNode = new TreeNode
                {
                    Tag = "Node_AdditionalMods",
                    Text = LanguageManager.GetString("Node_AdditionalMods")
                };
                treMods.Nodes.Add(objModsParentNode);
                objModsParentNode.Expand();
                foreach (VehicleMod objMod in _objMount.Mods)
                {
                    TreeNode objLoopNode = objMod.CreateTreeNode(null, null, null, null, null, null);
                    if (objLoopNode != null)
                        objModsParentNode.Nodes.Add(objLoopNode);
                }
                _lstMods.AddRange(_objMount.Mods);

                cboSize.SelectedValue = _objMount.SourceIDString;
            }
            if (cboSize.SelectedIndex == -1)
            {
                if (lstSize.Count > 0)
                    cboSize.SelectedIndex = 0;
            }
            else
                RefreshCBOs();

            nudMarkup.Visible = AllowDiscounts;
            lblMarkupLabel.Visible = AllowDiscounts;
            lblMarkupPercentLabel.Visible = AllowDiscounts;

            if (_objMount != null)
            {
                List<ListItem> lstVisibility = cboVisibility.Items.Cast<ListItem>().ToList();
                List<ListItem> lstFlexibility = cboFlexibility.Items.Cast<ListItem>().ToList();
                List<ListItem> lstControl = cboControl.Items.Cast<ListItem>().ToList();
                foreach (WeaponMountOption objExistingOption in _objMount.WeaponMountOptions)
                {
                    string strLoopId = objExistingOption.SourceIDString;
                    if (lstVisibility.Any(x => x.Value.ToString() == strLoopId))
                        cboVisibility.SelectedValue = strLoopId;
                    else if (lstFlexibility.Any(x => x.Value.ToString() == strLoopId))
                        cboFlexibility.SelectedValue = strLoopId;
                    else if (lstControl.Any(x => x.Value.ToString() == strLoopId))
                        cboControl.SelectedValue = strLoopId;
                }
            }

            _blnLoading = false;
            UpdateInfo();
            this.TranslateWinForm();
        }

		private void cmdOK_Click(object sender, EventArgs e)
		{
            //TODO: THIS IS UGLY AS SHIT, FIX BETTER

            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedMount))
                return;
            string strSelectedControl = cboControl.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedControl))
                return;
            string strSelectedFlexibility = cboFlexibility.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedFlexibility))
                return;
            string strSelectedVisibility = cboVisibility.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedVisibility))
                return;

            XmlNode xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedMount + "\"]");
            if (xmlSelectedMount == null)
                return;
            XmlNode xmlSelectedControl = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedControl + "\"]");
            if (xmlSelectedControl == null)
                return;
            XmlNode xmlSelectedFlexibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedFlexibility + "\"]");
            if (xmlSelectedFlexibility == null)
                return;
            XmlNode xmlSelectedVisibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedVisibility + "\"]");
            if (xmlSelectedVisibility == null)
                return;

            XmlNode xmlForbiddenNode = xmlSelectedMount["forbidden"];
            if (xmlForbiddenNode != null)
            {
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                    using (XmlNodeList xmlControlNodeList = xmlForbiddenNode.SelectNodes("control"))
                        if (xmlControlNodeList?.Count > 0)
                            foreach (XmlNode xmlLoopNode in xmlControlNodeList)
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;

                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlFlexibilityNodeList = xmlForbiddenNode.SelectNodes("flexibility"))
                        if (xmlFlexibilityNodeList?.Count > 0)
                            foreach (XmlNode xmlLoopNode in xmlFlexibilityNodeList)
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                }

                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlVisibilityNodeList = xmlForbiddenNode.SelectNodes("visibility"))
                        if (xmlVisibilityNodeList?.Count > 0)
                            foreach (XmlNode xmlLoopNode in xmlVisibilityNodeList)
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                }
            }
            XmlNode xmlRequiredNode = xmlSelectedMount["required"];
            if (xmlRequiredNode != null)
            {
                bool blnRequirementsMet = true;
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlControlNodeList = xmlRequiredNode.SelectNodes("control"))
                    {
                        if (xmlControlNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlControlNodeList)
                            {
                                blnRequirementsMet = false;
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                {
                                    blnRequirementsMet = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;

                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlFlexibilityNodeList = xmlRequiredNode.SelectNodes("flexibility"))
                    {
                        if (xmlFlexibilityNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlFlexibilityNodeList)
                            {
                                blnRequirementsMet = false;
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                {
                                    blnRequirementsMet = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;

                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlVisibilityNodeList = xmlRequiredNode.SelectNodes("visibility"))
                    {
                        if (xmlVisibilityNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlVisibilityNodeList)
                            {
                                blnRequirementsMet = false;
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                {
                                    blnRequirementsMet = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;
            }
		    if (_objMount == null)
		    {
		        _objMount = new WeaponMount(_objCharacter, _objVehicle);
		        _objMount.Create(xmlSelectedMount);
		    }
            else if (_objMount.SourceIDString != strSelectedMount)
            {
                _objMount.Create(xmlSelectedMount);
            }

            WeaponMountOption objControlOption = new WeaponMountOption(_objCharacter);
            WeaponMountOption objFlexibilityOption = new WeaponMountOption(_objCharacter);
            WeaponMountOption objVisibilityOption = new WeaponMountOption(_objCharacter);
            if (objControlOption.Create(xmlSelectedControl) &&
                objFlexibilityOption.Create(xmlSelectedFlexibility) &&
                objVisibilityOption.Create(xmlSelectedVisibility))
            {
                _objMount.WeaponMountOptions.Clear();
                _objMount.WeaponMountOptions.Add(objControlOption);
                _objMount.WeaponMountOptions.Add(objFlexibilityOption);
                _objMount.WeaponMountOptions.Add(objVisibilityOption);
            }

            _objMount.Mods.Clear();
            foreach (VehicleMod objMod in _lstMods)
		    {
                objMod.WeaponMountParent = _objMount;
                _objMount.Mods.Add(objMod);
		    }
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshCBOs();
            treMods.SelectedNode = null;
            UpdateInfo();
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            treMods.SelectedNode = null;
            UpdateInfo();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

	    public bool FreeCost => chkFreeItem.Checked;

	    public decimal Markup => nudMarkup.Value;

	    public bool AllowDiscounts { get; set; }
        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

	    private void UpdateInfo()
	    {
	        if (_blnLoading)
                return;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedMount))
                cmdOK.Enabled = false;
            else
            {
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedMount + "\"]");
                if (xmlSelectedMount == null)
                    cmdOK.Enabled = false;
                else
                {
                    string strSelectedControl = cboControl.SelectedValue?.ToString();
                    if (string.IsNullOrEmpty(strSelectedControl))
                        cmdOK.Enabled = false;
                    else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedControl + "\"]") == null)
                        cmdOK.Enabled = false;
                    else
                    {
                        string strSelectedFlexibility = cboFlexibility.SelectedValue?.ToString();
                        if (string.IsNullOrEmpty(strSelectedFlexibility))
                            cmdOK.Enabled = false;
                        else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedFlexibility + "\"]") == null)
                            cmdOK.Enabled = false;
                        else
                        {
                            string strSelectedVisibility = cboVisibility.SelectedValue?.ToString();
                            if (string.IsNullOrEmpty(strSelectedVisibility))
                                cmdOK.Enabled = false;
                            else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedVisibility + "\"]") == null)
                                cmdOK.Enabled = false;
                            else
                                cmdOK.Enabled = true;
                        }
                    }
                }
            }

            string[] astrSelectedValues = { cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString() };

            cmdDeleteMod.Enabled = false;
            string strSelectedModId = treMods.SelectedNode?.Tag.ToString();
            string strSpace = LanguageManager.GetString("String_Space");
            if (!string.IsNullOrEmpty(strSelectedModId) && strSelectedModId.IsGuid())
            {
                VehicleMod objMod = _lstMods.FirstOrDefault(x => x.InternalId == strSelectedModId);
                if (objMod != null)
                {
                    cmdDeleteMod.Enabled = !objMod.IncludedInVehicle;
                    lblSlots.Text = objMod.CalculatedSlots.ToString(GlobalOptions.InvariantCultureInfo);
                    lblAvailability.Text = objMod.DisplayTotalAvail;

                    if (chkFreeItem.Checked)
                    {
                        lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    }
                    else
                    {
                        int intTotalSlots = Convert.ToInt32(xmlSelectedMount?["slots"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                        foreach (string strSelectedId in astrSelectedValues)
                        {
                            if (!string.IsNullOrEmpty(strSelectedId))
                            {
                                XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedId + "\"]");
                                if (xmlLoopNode != null)
                                {
                                    intTotalSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                                }
                            }
                        }
                        foreach (VehicleMod objLoopMod in _lstMods)
                        {
                            intTotalSlots += objLoopMod.CalculatedSlots;
                        }
                        lblCost.Text = (objMod.TotalCostInMountCreation(intTotalSlots) * (1 + (nudMarkup.Value / 100.0m))).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    }

                    objMod.SetSourceDetail(lblSource);
                    lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
                    lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);
                    lblAvailabilityLabel.Visible = !string.IsNullOrEmpty(lblAvailability.Text);
                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                    return;
                }
            }

            if (xmlSelectedMount == null)
            {
                lblCost.Text = string.Empty;
                lblSlots.Text = string.Empty;
                lblAvailability.Text = string.Empty;
                lblCostLabel.Visible = false;
                lblSlotsLabel.Visible = false;
                lblAvailabilityLabel.Visible = false;
                return;
            }
	        decimal decCost = !chkFreeItem.Checked ? Convert.ToDecimal(xmlSelectedMount["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo) : 0;
            int intSlots = Convert.ToInt32(xmlSelectedMount["slots"]?.InnerText, GlobalOptions.InvariantCultureInfo);

            string strAvail = xmlSelectedMount["avail"]?.InnerText ?? string.Empty;
            char chrAvailSuffix = strAvail.Length > 0 ? strAvail[strAvail.Length - 1] : ' ';
            if (chrAvailSuffix == 'F' || chrAvailSuffix == 'R')
                strAvail = strAvail.Substring(0, strAvail.Length - 1);
            else
                chrAvailSuffix = ' ';
            int intAvail = Convert.ToInt32(strAvail, GlobalOptions.InvariantCultureInfo);

            foreach (string strSelectedId in astrSelectedValues)
            {
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedId + "\"]");
                    if (xmlLoopNode != null)
                    {
                        if (!chkFreeItem.Checked)
                            decCost += Convert.ToInt32(xmlLoopNode["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);

                        intSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText, GlobalOptions.InvariantCultureInfo);

                        string strLoopAvail = xmlLoopNode["avail"]?.InnerText ?? string.Empty;
                        char chrLoopAvailSuffix = strLoopAvail.Length > 0 ? strLoopAvail[strLoopAvail.Length - 1] : ' ';
                        if (chrLoopAvailSuffix == 'F')
                        {
                            strLoopAvail = strLoopAvail.Substring(0, strLoopAvail.Length - 1);
                            chrAvailSuffix = 'F';
                        }
                        else if (chrLoopAvailSuffix == 'R')
                        {
                            strLoopAvail = strLoopAvail.Substring(0, strLoopAvail.Length - 1);
                            if (chrAvailSuffix == ' ')
                                chrAvailSuffix = 'R';
                        }
                        intAvail += Convert.ToInt32(strLoopAvail, GlobalOptions.InvariantCultureInfo);
                    }
                }
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                intSlots += objMod.CalculatedSlots;
                AvailabilityValue objLoopAvail = objMod.TotalAvailTuple();
                char chrLoopAvailSuffix = objLoopAvail.Suffix;
                if (chrLoopAvailSuffix == 'F')
                    chrAvailSuffix = 'F';
                else if (chrAvailSuffix != 'F' &&chrLoopAvailSuffix == 'R')
                    chrAvailSuffix = 'R';
                intAvail += objLoopAvail.Value;
            }
            if (!chkFreeItem.Checked)
            {
                foreach (VehicleMod objMod in _lstMods)
                {
                    decCost += objMod.TotalCostInMountCreation(intSlots);
                }
            }

            string strAvailText = intAvail.ToString(GlobalOptions.CultureInfo);
            if (chrAvailSuffix == 'F')
                strAvailText += LanguageManager.GetString("String_AvailForbidden");
            else if (chrAvailSuffix == 'R')
                strAvailText += LanguageManager.GetString("String_AvailRestricted");

	        decCost *= 1 + (nudMarkup.Value / 100.0m);
	        lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
	        lblSlots.Text = intSlots.ToString(GlobalOptions.CultureInfo);
	        lblAvailability.Text = strAvailText;
	        lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
	        lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);
	        lblAvailabilityLabel.Visible = !string.IsNullOrEmpty(lblAvailability.Text);

            string strSource = xmlSelectedMount["source"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
            string strPage = xmlSelectedMount["altpage"]?.InnerText ?? xmlSelectedMount["page"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
            lblSource.Text = _objCharacter.LanguageBookShort(strSource) + strSpace + strPage;
            lblSource.SetToolTip(_objCharacter.LanguageBookLong(strSource) + strSpace +
                                 LanguageManager.GetString("String_Page") + strSpace + strPage);
	        lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
	    }

        private void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedMount + "\"]");

            int intSlots = Convert.ToInt32(xmlSelectedMount?["slots"]?.InnerText, GlobalOptions.InvariantCultureInfo);

            string[] astrSelectedValues = { cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString() };
            foreach (string strSelectedId in astrSelectedValues)
            {
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedId + "\"]");
                    if (xmlLoopNode != null)
                    {
                        intSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                    }
                }
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                intSlots += objMod.CalculatedSlots;
            }

            string strSpace = LanguageManager.GetString("String_Space");
            TreeNode objModsParentNode = treMods.FindNode("Node_AdditionalMods");
            do
            {
                using (frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(_objCharacter, _objVehicle, _objMount?.Mods)
                {
                    // Pass the selected vehicle on to the form.
                    VehicleMountMods = true,
                    WeaponMountSlots = intSlots
                })
                {
                    frmPickVehicleMod.ShowDialog(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                        break;

                blnAddAgain = frmPickVehicleMod.AddAgain;
                XmlDocument objXmlDocument = _objCharacter.LoadData("vehicles.xml");
                XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/weaponmountmods/mod[id = \"" + frmPickVehicleMod.SelectedMod + "\"]");

                    VehicleMod objMod = new VehicleMod(_objCharacter)
                    {
                        DiscountCost = frmPickVehicleMod.BlackMarketDiscount
                    };
                    objMod.Create(objXmlMod, frmPickVehicleMod.SelectedRating, _objVehicle, frmPickVehicleMod.Markup);
                    // Check the item's Cost and make sure the character can afford it.
                    decimal decOriginalCost = _objVehicle.TotalCost;
                    if (frmPickVehicleMod.FreeCost)
                        objMod.Cost = "0";

                    // Do not allow the user to add a new Vehicle Mod if the Vehicle's Capacity has been reached.
                    if (_objCharacter.Options.EnforceCapacity)
                    {
                        bool blnOverCapacity = false;
                        if (_objCharacter.Options.BookEnabled("R5"))
                        {
                            if (_objVehicle.IsDrone && GlobalOptions.Dronemods)
                            {
                                if (_objVehicle.DroneModSlotsUsed > _objVehicle.DroneModSlots)
                                    blnOverCapacity = true;
                            }
                            else
                            {
                                int intUsed = _objVehicle.CalcCategoryUsed(objMod.Category);
                                int intAvail = _objVehicle.CalcCategoryAvail(objMod.Category);
                                if (intUsed > intAvail)
                                    blnOverCapacity = true;
                            }
                        }
                        else if (_objVehicle.Slots < _objVehicle.SlotsUsed)
                        {
                            blnOverCapacity = true;
                        }

                        if (blnOverCapacity)
                        {
                            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CapacityReached"), LanguageManager.GetString("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }
                    }

                    if (_objCharacter.Created)
                    {
                        decimal decCost = _objVehicle.TotalCost - decOriginalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objMod.TotalAvailTuple().Suffix;
                        if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                            decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                        if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                            decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                        if (decCost > _objCharacter.Nuyen)
                        {
                            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"),
                                LanguageManager.GetString("MessageTitle_NotEnoughNuyen"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                        objExpense.Create(decCost * -1,
                            LanguageManager.GetString("String_ExpensePurchaseVehicleMod") +
                            strSpace + objMod.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                        _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                        _objCharacter.Nuyen -= decCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMountMod, objMod.InternalId);
                        objExpense.Undo = objUndo;
                    }

                    _lstMods.Add(objMod);
                    intSlots += objMod.CalculatedSlots;

                    TreeNode objNewNode = objMod.CreateTreeNode(null, null, null, null, null, null);

                    if (objModsParentNode == null)
                    {
                        objModsParentNode = new TreeNode
                        {
                            Tag = "Node_AdditionalMods",
                            Text = LanguageManager.GetString("Node_AdditionalMods")
                        };
                        treMods.Nodes.Add(objModsParentNode);
                        objModsParentNode.Expand();
                    }

                    objModsParentNode.Nodes.Add(objNewNode);
                    treMods.SelectedNode = objNewNode;
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteMod_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treMods.SelectedNode;
            string strSelectedId = objSelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId) && strSelectedId.IsGuid())
            {
                VehicleMod objMod = _lstMods.FirstOrDefault(x => x.InternalId == strSelectedId);
                if (objMod != null && !objMod.IncludedInVehicle)
                {
                    if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle")))
                        return;

                    _lstMods.Remove(objMod);
                    foreach (Weapon objLoopWeapon in objMod.Weapons)
                    {
                        objLoopWeapon.DeleteWeapon();
                    }
                    foreach (Cyberware objLoopCyberware in objMod.Cyberware)
                    {
                        objLoopCyberware.DeleteCyberware();
                    }
                    TreeNode objParentNode = objSelectedNode.Parent;
                    objSelectedNode.Remove();
                    if (objParentNode.Nodes.Count == 0)
                        objParentNode.Remove();
                }
            }
        }

        private void treMods_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateInfo();
        }

        private void RefreshCBOs()
        {
            XmlNode xmlRequiredNode = null;
            XmlNode xmlForbiddenNode = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
            {
                XmlNode xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedMount + "\"]");
                if (xmlSelectedMount != null)
                {
                    xmlForbiddenNode = xmlSelectedMount.SelectSingleNode("forbidden/weaponmountdetails");
                    xmlRequiredNode = xmlSelectedMount.SelectSingleNode("required/weaponmountdetails");
                }
            }

            XmlNode xmlVehicleNode = _objVehicle.GetNode();
            List<ListItem> lstVisibility = new List<ListItem>();
            List<ListItem> lstFlexibility = new List<ListItem>();
            List<ListItem> lstControl = new List<ListItem>();
            // Populate the Weapon Mount Category list.
            string strFilter = "category != \"Size\" and not(hide)";
            if (!_objVehicle.IsDrone || !GlobalOptions.Dronemods)
                strFilter += " and not(optionaldrone)";
            using (XmlNodeList xmlWeaponMountOptionNodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount[" + strFilter + "]"))
                if (xmlWeaponMountOptionNodeList?.Count > 0)
                    foreach (XmlNode xmlWeaponMountOptionNode in xmlWeaponMountOptionNodeList)
                    {
                        string strId = xmlWeaponMountOptionNode["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XmlNode xmlTestNode = xmlWeaponMountOptionNode.SelectSingleNode("forbidden/vehicledetails");
                        if (xmlTestNode != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                continue;
                            }
                        }
                        xmlTestNode = xmlWeaponMountOptionNode.SelectSingleNode("required/vehicledetails");
                        if (xmlTestNode != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (!xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                continue;
                            }
                        }

                        string strName = xmlWeaponMountOptionNode["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                        bool blnAddItem = true;
                        switch (xmlWeaponMountOptionNode["category"]?.InnerText)
                        {
                            case "Visibility":
                            {
                                XmlNodeList xmlNodeList = xmlForbiddenNode?.SelectNodes("visibility");
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlNodeList)
                                    {
                                        if (xmlLoopNode.InnerText == strName)
                                        {
                                            blnAddItem = false;
                                            break;
                                        }
                                    }
                                }

                                if (xmlRequiredNode != null)
                                {
                                    blnAddItem = false;
                                    xmlNodeList = xmlRequiredNode.SelectNodes("visibility");
                                    if (xmlNodeList?.Count > 0)
                                        foreach (XmlNode xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.InnerText == strName)
                                            {
                                                blnAddItem = true;
                                                break;
                                            }
                                        }
                                }

                                if (blnAddItem)
                                    lstVisibility.Add(new ListItem(strId, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                            }
                                break;
                            case "Flexibility":
                            {
                                XmlNodeList xmlNodeList = xmlForbiddenNode?.SelectNodes("flexibility");
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlNodeList)
                                    {
                                        if (xmlLoopNode.InnerText == strName)
                                        {
                                            blnAddItem = false;
                                            break;
                                        }
                                    }
                                }

                                if (xmlRequiredNode != null)
                                {
                                    blnAddItem = false;
                                    xmlNodeList = xmlRequiredNode.SelectNodes("flexibility");
                                    if (xmlNodeList?.Count > 0)
                                        foreach (XmlNode xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.InnerText == strName)
                                            {
                                                blnAddItem = true;
                                                break;
                                            }
                                        }
                                }

                                if (blnAddItem)
                                    lstFlexibility.Add(new ListItem(strId, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                            }
                                break;
                            case "Control":
                            {
                                XmlNodeList xmlNodeList = xmlForbiddenNode?.SelectNodes("control");
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlNodeList)
                                    {
                                        if (xmlLoopNode.InnerText == strName)
                                        {
                                            blnAddItem = false;
                                            break;
                                        }
                                    }
                                }

                                if (xmlRequiredNode != null)
                                {
                                    blnAddItem = false;
                                    xmlNodeList = xmlRequiredNode.SelectNodes("control");
                                    if (xmlNodeList?.Count > 0)
                                        foreach (XmlNode xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.InnerText == strName)
                                            {
                                                blnAddItem = true;
                                                break;
                                            }
                                        }
                                }

                                if (blnAddItem)
                                    lstControl.Add(new ListItem(strId, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                            }
                                break;
                            default:
                                Utils.BreakIfDebug();
                                break;
                        }
                    }

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            string strOldVisibility = cboVisibility.SelectedValue?.ToString();
            string strOldFlexibility = cboFlexibility.SelectedValue?.ToString();
            string strOldControl = cboControl.SelectedValue?.ToString();
            cboVisibility.BeginUpdate();
            cboVisibility.ValueMember = nameof(ListItem.Value);
            cboVisibility.DisplayMember = nameof(ListItem.Name);
            cboVisibility.DataSource = lstVisibility;
            cboVisibility.Enabled = lstVisibility.Count > 1;
            if (!string.IsNullOrEmpty(strOldVisibility))
                cboVisibility.SelectedValue = strOldVisibility;
            if (cboVisibility.SelectedIndex == -1 && lstVisibility.Count > 0)
                cboVisibility.SelectedIndex = 0;
            cboVisibility.EndUpdate();

            cboFlexibility.BeginUpdate();
            cboFlexibility.ValueMember = nameof(ListItem.Value);
            cboFlexibility.DisplayMember = nameof(ListItem.Name);
            cboFlexibility.DataSource = lstFlexibility;
            cboFlexibility.Enabled = lstFlexibility.Count > 1;
            if (!string.IsNullOrEmpty(strOldFlexibility))
                cboFlexibility.SelectedValue = strOldFlexibility;
            if (cboFlexibility.SelectedIndex == -1 && lstFlexibility.Count > 0)
                cboFlexibility.SelectedIndex = 0;
            cboFlexibility.EndUpdate();

            cboControl.BeginUpdate();
            cboControl.ValueMember = nameof(ListItem.Value);
            cboControl.DisplayMember = nameof(ListItem.Name);
            cboControl.DataSource = lstControl;
            cboControl.Enabled = lstControl.Count > 1;
            if (!string.IsNullOrEmpty(strOldControl))
                cboControl.SelectedValue = strOldControl;
            if (cboControl.SelectedIndex == -1 && lstControl.Count > 0)
                cboControl.SelectedIndex = 0;
            cboControl.EndUpdate();

            _blnLoading = blnOldLoading;
        }
    }
}
