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
		private readonly List<ListItem> _lstVisibility = new List<ListItem>();
        private readonly List<ListItem> _lstFlexibility = new List<ListItem>();
        private readonly List<ListItem> _lstControl = new List<ListItem>();
        private readonly List<ListItem> _lstSize = new List<ListItem>();
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
		    if (_objMount != null)
		    {
		        _lstMods.AddRange(_objMount.Mods);
		    }
		    _objCharacter = objCharacter;
			InitializeComponent();
		}

        private void frmCreateWeaponMount_Load(object sender, EventArgs e)
        {
            // Populate the Weapon Mount Category list.
            foreach (XmlNode node in _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount"))
            {
                if (node["optionaldrone"] != null && !_objVehicle.IsDrone)
                    continue;
                ListItem objItem = new ListItem(node["id"]?.InnerText, node.Attributes?["translate"]?.InnerText ?? node["name"]?.InnerText ?? string.Empty);
                switch (node["category"]?.InnerText)
                {
                    case "Visibility":
                        _lstVisibility.Add(objItem);
                        break;
                    case "Flexibility":
                        _lstFlexibility.Add(objItem);
                        break;
                    case "Control":
                        _lstControl.Add(objItem);
                        break;
                    case "Size":
                        _lstSize.Add(objItem);
                        break;
                    default:
                        Utils.BreakIfDebug();
                        break;
                }
            }

            cboSize.BeginUpdate();
            cboSize.ValueMember = "Value";
            cboSize.DisplayMember = "Name";
            cboSize.DataSource = _lstSize;
            cboSize.Enabled = _lstSize.Count > 1;
            cboSize.EndUpdate();

            cboVisibility.BeginUpdate();
            cboVisibility.ValueMember = "Value";
            cboVisibility.DisplayMember = "Name";
            cboVisibility.DataSource = _lstVisibility;
            cboVisibility.Enabled = _lstVisibility.Count > 1;
            cboVisibility.EndUpdate();

            cboFlexibility.BeginUpdate();
            cboFlexibility.ValueMember = "Value";
            cboFlexibility.DisplayMember = "Name";
            cboFlexibility.DataSource = _lstFlexibility;
            cboFlexibility.Enabled = _lstFlexibility.Count > 1;
            cboFlexibility.EndUpdate();

            cboControl.BeginUpdate();
            cboControl.ValueMember = "Value";
            cboControl.DisplayMember = "Name";
            cboControl.DataSource = _lstControl;
            cboControl.Enabled = _lstControl.Count > 1;
            cboControl.EndUpdate();

            nudMarkup.Visible = AllowDiscounts;
            lblMarkupLabel.Visible = AllowDiscounts;
            lblMarkupPercentLabel.Visible = AllowDiscounts;

            if (_objMount != null)
            {
                foreach (VehicleMod objMod in _objMount.Mods)
                {
                    TreeNode n = new TreeNode
                    {
                        Text = objMod.DisplayName(GlobalOptions.Language),
                        Tag = objMod
                    };
                    treMods.Nodes[0].Nodes.Add(n);
                }
                _lstMods.AddRange(_objMount.Mods);

                cboSize.SelectedValue = _objMount.SourceId;
                foreach (WeaponMountOption objExistingOption in _objMount.WeaponMountOptions)
                {
                    string strLoopId = objExistingOption.SourceId;
                    if (_lstVisibility.Any(x => x.Value == strLoopId))
                        cboVisibility.SelectedValue = strLoopId;
                    else if (_lstFlexibility.Any(x => x.Value == strLoopId))
                        cboFlexibility.SelectedValue = strLoopId;
                    else if (_lstControl.Any(x => x.Value == strLoopId))
                        cboControl.SelectedValue = strLoopId;
                }

                if (cboSize.SelectedIndex == -1 && _lstSize.Count > 0)
                    cboSize.SelectedIndex = 0;
                if (cboVisibility.SelectedIndex == -1 && _lstVisibility.Count > 0)
                    cboVisibility.SelectedIndex = 0;
                if (cboFlexibility.SelectedIndex == -1 && _lstFlexibility.Count > 0)
                    cboFlexibility.SelectedIndex = 0;
                if (cboControl.SelectedIndex == -1 && _lstControl.Count > 0)
                    cboControl.SelectedIndex = 0;
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
            if (xmlSelectedMount == null)
                return;
            XmlNode xmlSelectedFlexibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedFlexibility + "\"]");
            if (xmlSelectedMount == null)
                return;
            XmlNode xmlSelectedVisibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedVisibility + "\"]");
            if (xmlSelectedMount == null)
                return;

            XmlNode xmlForbiddenNode = xmlSelectedMount["forbidden"];
            if (xmlSelectedMount["forbidden"] != null)
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
                bool requirementsMet = false;
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("control"))
                    {
                        if (xmlLoopNode.InnerText == strStringToCheck)
                        {
                            requirementsMet = true;
                            break;
                        }
                    }
                }
                if (!requirementsMet)
                    return;

                requirementsMet = false;
                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("flexibility"))
                    {
                        if (xmlLoopNode.InnerText == strStringToCheck)
                        {
                            requirementsMet = true;
                            break;
                        }
                    }
                }
                if (!requirementsMet)
                    return;

                requirementsMet = false;
                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    foreach (XmlNode xmlLoopNode in xmlRequiredNode.SelectNodes("visibility"))
                    {
                        if (xmlLoopNode.InnerText == strStringToCheck)
                        {
                            requirementsMet = true;
                            break;
                        }
                    }
                }
                if (!requirementsMet)
                    return;
            }
		    if (_objMount == null)
		    {
		        _objMount = new WeaponMount(_objCharacter, _objVehicle);
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
                _objMount.Mods.Add(objMod);
		    }
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
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

            XmlNode xmlMountNode = null;
            string strSelectedMountId = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMountId))
            {
                xmlMountNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + strSelectedMountId + "\"]");
            }
            if (xmlMountNode == null)
            {
                lblCost.Text = string.Empty;
                lblSlots.Text = string.Empty;
                lblAvailability.Text = string.Empty;
                return;
            }
	        decimal decCost = !chkFreeItem.Checked ? Convert.ToDecimal(xmlMountNode["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo) : 0;
            int intSlots = Convert.ToInt32(xmlMountNode["slots"]?.InnerText);

            string strAvail = xmlMountNode["avail"]?.InnerText ?? string.Empty;
            char chrAvailSuffix = strAvail.Length > 0 ? strAvail[strAvail.Length - 1] : ' ';
            if (chrAvailSuffix == 'F' || chrAvailSuffix == 'R')
                strAvail = strAvail.Substring(0, strAvail.Length - 1);
            else
                chrAvailSuffix = ' ';
            int intAvail = Convert.ToInt32(strAvail);

            string[] astrSelectedValues = {cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString()};
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

            string strAvailText = intAvail.ToString(GlobalOptions.Language);
            if (chrAvailSuffix == 'F')
                strAvailText += LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
            else if (chrAvailSuffix == 'R')
                strAvailText += LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);

	        decCost *= 1 + (nudMarkup.Value / 100.0m);
	        lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
	        lblSlots.Text = intSlots.ToString();
	        lblAvailability.Text = strAvailText;
        }

        private void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            do
            {
                frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(_objCharacter)
                {
                    // Pass the selected vehicle on to the form.
                    SelectedVehicle = _objVehicle,
                    VehicleMountMods = true
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
                        _lstMods.Remove(objMod);
                        frmPickVehicleMod.Dispose();
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
                        _lstMods.Remove(objMod);
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language),
                            LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickVehicleMod.Dispose();
                        continue;
                    }
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1,
                        LanguageManager.GetString("String_ExpensePurchaseVehicleMod", GlobalOptions.Language) +
                        " " + objMod.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.Add(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMountMod, objMod.InternalId);
                    objExpense.Undo = objUndo;
                }
                _lstMods.Add(objMod);

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

                treMods.Nodes[0].Nodes.Add(objMod.CreateTreeNode(null, null, null, null, null, null));
                treMods.Nodes[0].Expand();
                frmPickVehicleMod.Dispose();
            }
            while (blnAddAgain);
        }
    }
}
