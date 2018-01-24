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
        private XmlDocument _xmlDoc;

        public WeaponMount WeaponMount
        {
            get => _objMount;
        }

        public frmCreateWeaponMount(Vehicle objVehicle, Character objCharacter, WeaponMount objWeaponMount = null)
		{
            _xmlDoc = XmlManager.Load("vehicles.xml");
		    _objVehicle = objVehicle;
		    _objMount = objWeaponMount;
		    _objCharacter = objCharacter;
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
            foreach (XmlNode xmlSizeNode in _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount[" + strSizeFilter + "]"))
            {
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

                lstSize.Add(new ListItem(xmlSizeNode["id"].InnerText, xmlSizeNode["translate"]?.InnerText ?? xmlSizeNode["name"].InnerText));
            }

            cboSize.BeginUpdate();
            cboSize.ValueMember = "Value";
            cboSize.DisplayMember = "Name";
            cboSize.DataSource = lstSize;
            cboSize.Enabled = lstSize.Count > 1;
            cboSize.EndUpdate();

            if (_objMount != null)
            {
                TreeNode objModsParentNode = new TreeNode
                {
                    Tag = "Node_AdditionalMods",
                    Text = LanguageManager.GetString("Node_AdditionalMods", GlobalOptions.Language)
                };
                treMods.Nodes.Add(objModsParentNode);
                objModsParentNode.Expand();
                foreach (VehicleMod objMod in _objMount.Mods)
                {
                    objModsParentNode.Nodes.Add(objMod.CreateTreeNode(null, null, null, null, null, null));
                }
                _lstMods.AddRange(_objMount.Mods);

                cboSize.SelectedValue = _objMount.SourceId;
            }
            if (cboSize.SelectedIndex == -1)
                if (lstSize.Count > 0)
                    cboSize.SelectedIndex = 0;
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
                    string strLoopId = objExistingOption.SourceId;
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
                    foreach (XmlNode xmlLoopNode in xmlForbiddenNode.SelectNodes("control"))
                        if (xmlLoopNode.InnerText == strStringToCheck)
                            return;

                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                    foreach (XmlNode xmlLoopNode in xmlForbiddenNode.SelectNodes("flexibility"))
                        if (xmlLoopNode.InnerText == strStringToCheck)
                            return;

                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                    foreach (XmlNode xmlLoopNode in xmlForbiddenNode.SelectNodes("visibility"))
                        if (xmlLoopNode.InnerText == strStringToCheck)
                            return;
            }
            XmlNode xmlRequiredNode = xmlSelectedMount["required"];
            if (xmlRequiredNode != null)
            {
                bool blnRequirementsMet = true;
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("control"))
                    {
                        blnRequirementsMet = false;
                        if (xmlLoopNode.InnerText == strStringToCheck)
                        {
                            blnRequirementsMet = true;
                            break;
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;

                blnRequirementsMet = true;
                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("flexibility"))
                    {
                        blnRequirementsMet = false;
                        if (xmlLoopNode.InnerText == strStringToCheck)
                        {
                            blnRequirementsMet = true;
                            break;
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;

                blnRequirementsMet = true;
                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("visibility"))
                    {
                        blnRequirementsMet = false;
                        if (xmlLoopNode.InnerText == strStringToCheck)
                        {
                            blnRequirementsMet = true;
                            break;
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
            else if (_objMount.SourceId != strSelectedMount)
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

	    public bool AllowDiscounts { get; set; } = false;
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
            if (!string.IsNullOrEmpty(strSelectedModId) && strSelectedModId.IsGuid())
            {
                VehicleMod objMod = _lstMods.FirstOrDefault(x => x.InternalId == strSelectedModId);
                if (objMod != null)
                {
                    cmdDeleteMod.Enabled = !objMod.IncludedInVehicle;
                    lblSlots.Text = objMod.CalculatedSlots.ToString();
                    lblAvailability.Text = objMod.TotalAvail(GlobalOptions.Language);
                    
                    if (chkFreeItem.Checked)
                    {
                        lblCost.Text = 0.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    }
                    else
                    {
                        int intTotalSlots = Convert.ToInt32(xmlSelectedMount?["slots"]?.InnerText);
                        for (int i = 0; i < astrSelectedValues.Length; ++i)
                        {
                            string strSelectedId = astrSelectedValues[i];
                            if (!string.IsNullOrEmpty(strSelectedId))
                            {
                                XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedId + "\"]");
                                if (xmlLoopNode != null)
                                {
                                    intTotalSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText);
                                }
                            }
                        }
                        foreach (VehicleMod objLoopMod in _lstMods)
                        {
                            intTotalSlots += objMod.CalculatedSlots;
                        }
                        lblCost.Text = (objMod.TotalCostInMountCreation(intTotalSlots) * (1 + (nudMarkup.Value / 100.0m))).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    }

                    string strModPage = objMod.Page(GlobalOptions.Language);
                    lblSource.Text = CommonFunctions.LanguageBookShort(objMod.Source, GlobalOptions.Language) + ' ' + strModPage;

                    tipTooltip.SetToolTip(lblSource,
                        CommonFunctions.LanguageBookLong(objMod.Source, GlobalOptions.Language) + ' ' +
                        LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strModPage);
                    return;
                }
            }
            
            if (xmlSelectedMount == null)
            {
                lblCost.Text = string.Empty;
                lblSlots.Text = string.Empty;
                lblAvailability.Text = string.Empty;
                return;
            }
	        decimal decCost = !chkFreeItem.Checked ? Convert.ToDecimal(xmlSelectedMount["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo) : 0;
            int intSlots = Convert.ToInt32(xmlSelectedMount["slots"]?.InnerText);

            string strAvail = xmlSelectedMount["avail"]?.InnerText ?? string.Empty;
            char chrAvailSuffix = strAvail.Length > 0 ? strAvail[strAvail.Length - 1] : ' ';
            if (chrAvailSuffix == 'F' || chrAvailSuffix == 'R')
                strAvail = strAvail.Substring(0, strAvail.Length - 1);
            else
                chrAvailSuffix = ' ';
            int intAvail = Convert.ToInt32(strAvail);
            
	        for(int i = 0; i < astrSelectedValues.Length; ++i)
	        {
                string strSelectedId = astrSelectedValues[i];
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedId + "\"]");
                    if (xmlLoopNode != null)
                    {
                        if (!chkFreeItem.Checked)
                            decCost += Convert.ToInt32(xmlLoopNode["cost"]?.InnerText);

                        intSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText);

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
                        intAvail += Convert.ToInt32(strLoopAvail);
                    }
                }
	        }
            foreach (VehicleMod objMod in _lstMods)
            {
                intSlots += objMod.CalculatedSlots;
                string strLoopAvail = objMod.TotalAvail(GlobalOptions.DefaultLanguage);
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
                intAvail += Convert.ToInt32(strLoopAvail);
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
                strAvailText += LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
            else if (chrAvailSuffix == 'R')
                strAvailText += LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);

	        decCost *= 1 + (nudMarkup.Value / 100.0m);
	        lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
	        lblSlots.Text = intSlots.ToString();
	        lblAvailability.Text = strAvailText;

            string strSource = xmlSelectedMount["source"].InnerText;
            string strPage = xmlSelectedMount["altpage"]?.InnerText ?? xmlSelectedMount["page"].InnerText;
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

            tipTooltip.SetToolTip(lblSource,
                CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' +
                LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
        }

        private void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedMount + "\"]");

            int intSlots = Convert.ToInt32(xmlSelectedMount?["slots"]?.InnerText);

            string[] astrSelectedValues = { cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString() };
            for (int i = 0; i < astrSelectedValues.Length; ++i)
            {
                string strSelectedId = astrSelectedValues[i];
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedId + "\"]");
                    if (xmlLoopNode != null)
                    {
                        intSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText);
                    }
                }
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                intSlots += objMod.CalculatedSlots;
            }

            TreeNode objModsParentNode = treMods.FindNode("Node_AdditionalMods");
            do
            {
                frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(_objCharacter)
                {
                    // Pass the selected vehicle on to the form.
                    SelectedVehicle = _objVehicle,
                    VehicleMountMods = true,
                    WeaponMountSlots = intSlots
                };
                if (_objMount != null)
                {
                    frmPickVehicleMod.InstalledMods = _objMount.Mods;
                }

                frmPickVehicleMod.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                {
                    frmPickVehicleMod.Dispose();
                    break;
                }
                blnAddAgain = frmPickVehicleMod.AddAgain;
                XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");
                XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/weaponmountmods/mod[id = \"" + frmPickVehicleMod.SelectedMod + "\"]");

                VehicleMod objMod = new VehicleMod(_objCharacter);
                objMod.Create(objXmlMod, frmPickVehicleMod.SelectedRating, _objVehicle, frmPickVehicleMod.Markup);
                // Check the item's Cost and make sure the character can afford it.
                decimal decOriginalCost = _objVehicle.TotalCost;
                if (frmPickVehicleMod.FreeCost)
                    objMod.Cost = "0";
                frmPickVehicleMod.Dispose();

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
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }
                }
                if (_objCharacter.Created)
                {
                    decimal decCost = _objVehicle.TotalCost - decOriginalCost;

                    // Multiply the cost if applicable.
                    string strAvail = objMod.TotalAvail(GlobalOptions.DefaultLanguage);
                    if (strAvail.EndsWith(LanguageManager.GetString("String_AvailRestricted",
                            GlobalOptions.DefaultLanguage)) && _objCharacter.Options.MultiplyRestrictedCost)
                        decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                    if (strAvail.EndsWith(LanguageManager.GetString("String_AvailForbidden",
                            GlobalOptions.DefaultLanguage)) && _objCharacter.Options.MultiplyForbiddenCost)
                        decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                    if (decCost > _objCharacter.Nuyen)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language),
                            LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1,
                        LanguageManager.GetString("String_ExpensePurchaseVehicleMod", GlobalOptions.Language) +
                        ' ' + objMod.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.Add(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMountMod, objMod.InternalId);
                    objExpense.Undo = objUndo;
                }
                _lstMods.Add(objMod);
                intSlots += objMod.CalculatedSlots;

                // Check for Improved Sensor bonus.
                if (objMod.Bonus?["selecttext"] != null)
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", objMod.DisplayNameShort(GlobalOptions.Language))
                    };
                    frmPickText.ShowDialog(this);
                    objMod.Extra = frmPickText.SelectedValue;
                    frmPickText.Dispose();
                }

                TreeNode objNewNode = objMod.CreateTreeNode(null, null, null, null, null, null);

                if (objModsParentNode == null)
                {
                    objModsParentNode = new TreeNode
                    {
                        Tag = "Node_AdditionalMods",
                        Text = LanguageManager.GetString("Node_AdditionalMods", GlobalOptions.Language)
                    };
                    treMods.Nodes.Add(objModsParentNode);
                    objModsParentNode.Expand();
                }

                objModsParentNode.Nodes.Add(objNewNode);
                treMods.SelectedNode = objNewNode;
            }
            while (blnAddAgain);
        }

        private void cmdDeleteMod_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treMods.SelectedNode;
            if (objSelectedNode != null)
            {
                string strSelectedId = objSelectedNode.Tag.ToString();
                if (!string.IsNullOrEmpty(strSelectedId) && strSelectedId.IsGuid())
                {
                    VehicleMod objMod = _lstMods.FirstOrDefault(x => x.InternalId == strSelectedId);
                    if (objMod != null && !objMod.IncludedInVehicle)
                    {
                        if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle", GlobalOptions.Language)))
                            return;

                        _lstMods.Remove(objMod);
                        foreach (Weapon objLoopWeapon in objMod.Weapons)
                        {
                            objLoopWeapon.DeleteWeapon(null, null);
                        }
                        foreach (Cyberware objLoopCyberware in objMod.Cyberware)
                        {
                            objLoopCyberware.DeleteCyberware(null, null);
                        }
                        TreeNode objParentNode = objSelectedNode.Parent;
                        objSelectedNode.Remove();
                        if (objParentNode.Nodes.Count == 0)
                            objParentNode.Remove();
                    }
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
            foreach (XmlNode xmlWeaponMountOptionNode in _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount[" + strFilter + "]"))
            {
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

                string strName = xmlWeaponMountOptionNode["name"].InnerText;
                bool blnAddItem = true;
                switch (xmlWeaponMountOptionNode["category"]?.InnerText)
                {
                    case "Visibility":
                        if (xmlForbiddenNode != null)
                        {
                            foreach (XmlNode xmlLoopNode in xmlForbiddenNode.SelectNodes("visibility"))
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
                            foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("visibility"))
                            {
                                if (xmlLoopNode.InnerText == strName)
                                {
                                    blnAddItem = true;
                                    break;
                                }
                            }
                        }
                        if (blnAddItem)
                            lstVisibility.Add(new ListItem(xmlWeaponMountOptionNode["id"].InnerText, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                        break;
                    case "Flexibility":
                        if (xmlForbiddenNode != null)
                        {
                            foreach (XmlNode xmlLoopNode in xmlForbiddenNode.SelectNodes("flexibility"))
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
                            foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("flexibility"))
                            {
                                if (xmlLoopNode.InnerText == strName)
                                {
                                    blnAddItem = true;
                                    break;
                                }
                            }
                        }
                        if (blnAddItem)
                            lstFlexibility.Add(new ListItem(xmlWeaponMountOptionNode["id"].InnerText, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                        break;
                    case "Control":
                        if (xmlForbiddenNode != null)
                        {
                            foreach (XmlNode xmlLoopNode in xmlForbiddenNode.SelectNodes("control"))
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
                            foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("control"))
                            {
                                if (xmlLoopNode.InnerText == strName)
                                {
                                    blnAddItem = true;
                                    break;
                                }
                            }
                        }
                        if (blnAddItem)
                            lstControl.Add(new ListItem(xmlWeaponMountOptionNode["id"].InnerText, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
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
            cboVisibility.ValueMember = "Value";
            cboVisibility.DisplayMember = "Name";
            cboVisibility.DataSource = lstVisibility;
            cboVisibility.Enabled = lstVisibility.Count > 1;
            if (!string.IsNullOrEmpty(strOldVisibility))
                cboVisibility.SelectedValue = strOldVisibility;
            if (cboVisibility.SelectedIndex == -1 && lstVisibility.Count > 0)
                cboVisibility.SelectedIndex = 0;
            cboVisibility.EndUpdate();

            cboFlexibility.BeginUpdate();
            cboFlexibility.ValueMember = "Value";
            cboFlexibility.DisplayMember = "Name";
            cboFlexibility.DataSource = lstFlexibility;
            cboFlexibility.Enabled = lstFlexibility.Count > 1;
            if (!string.IsNullOrEmpty(strOldFlexibility))
                cboFlexibility.SelectedValue = strOldFlexibility;
            if (cboFlexibility.SelectedIndex == -1 && lstFlexibility.Count > 0)
                cboFlexibility.SelectedIndex = 0;
            cboFlexibility.EndUpdate();

            cboControl.BeginUpdate();
            cboControl.ValueMember = "Value";
            cboControl.DisplayMember = "Name";
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
