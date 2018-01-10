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
		private readonly List<object> _lstVisibility;
		private readonly List<object> _lstFlexibility;
		private readonly List<object> _lstControl;
		private readonly List<object> _lstSize;
	    private readonly List<VehicleMod> _lstMods = new List<VehicleMod>();
		private bool _loading = true;
	    private readonly Vehicle _vehicle;
	    private readonly Character _objCharacter;
	    private WeaponMount _mount;
		private XmlDocument _xmlDoc;

        public WeaponMount WeaponMount { get; internal set; }

        public frmCreateWeaponMount(Vehicle vehicle, Character character, WeaponMount wm = null)
		{
			_lstControl = new List<object>();
			_lstFlexibility = new List<object>();
			_lstSize = new List<object>();
			_lstVisibility = new List<object>();
		    _vehicle = vehicle;
		    _mount = wm;
		    if (_mount != null)
		    {
		        _lstMods.AddRange(_mount.Mods);
		    }
		    _objCharacter = character;
			InitializeComponent();
		}

        private void frmCreateWeaponMount_Load(object sender, EventArgs e)
        {
            _xmlDoc = XmlManager.Load("vehicles.xml");

            // Populate the Armor Category list.
            var nodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount");
            
            if (nodeList != null)
                foreach (XmlNode node in nodeList)
                {
                    var add = !(node["optionaldrone"] != null && !_vehicle.IsDrone);
                    if (!add) continue;
                    var objItem = new ListItem(node["id"]?.InnerText, node.Attributes?["translate"]?.InnerText ?? node["name"]?.InnerText);
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
            cboSize.EndUpdate();

            cboVisibility.BeginUpdate();
            cboVisibility.ValueMember = "Value";
            cboVisibility.DisplayMember = "Name";
            cboVisibility.DataSource = _lstVisibility;
            cboVisibility.EndUpdate();

            cboFlexibility.BeginUpdate();
            cboFlexibility.ValueMember = "Value";
            cboFlexibility.DisplayMember = "Name";
            cboFlexibility.DataSource = _lstFlexibility;
            cboFlexibility.EndUpdate();

            cboControl.BeginUpdate();
            cboControl.ValueMember = "Value";
            cboControl.DisplayMember = "Name";
            cboControl.DataSource = _lstControl;
            cboControl.EndUpdate();
            nudMarkup.Visible = AllowDiscounts;
            lblMarkupLabel.Visible = AllowDiscounts;
            lblMarkupPercentLabel.Visible = AllowDiscounts;

            if (_mount != null)
            {
                foreach (var m in _mount.Mods)
                {
                    var n = new TreeNode
                    {
                        Text = m.DisplayName(GlobalOptions.Language),
                        Tag = m
                    };
                    treMods.Nodes[0].Nodes.Add(n);
                }
                _lstMods.AddRange(_mount.Mods);
            }
            _loading = false;
            comboBox_SelectedIndexChanged(null, null);
        }

		private void cmdOK_Click(object sender, EventArgs e)
		{
            var tree = new TreeNode();
            //TODO: THIS IS UGLY AS SHIT, FIX BETTER
            var node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboSize.SelectedValue + "\"]");
            if (node?["forbidden"] != null)
            {
                var list = node.SelectNodes("/forbidden/control");
                var check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboControl.SelectedValue + "\"]");
                if (list != null && list.Cast<XmlNode>().Any(n => n.InnerText == check?["name"]?.InnerText))
                    return;
                list = node.SelectNodes("/forbidden/flexibility");
                check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboFlexibility.SelectedValue + "\"]");
                if (list != null && list.Cast<XmlNode>().Any(n => n.InnerText == check?["name"]?.InnerText))
                    return;
                list = node.SelectNodes("/forbidden/visibility");
                check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboVisibility.SelectedValue + "\"]");
                if (list != null && list.Cast<XmlNode>().Any(n => n.InnerText == check?["name"]?.InnerText))
                    return;
            }
            if (node["required"] != null)
            {
                bool requirementsMet = true;
                XmlNodeList list = node.SelectNodes("/required/control");
                XmlNode check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboControl.SelectedValue + "\"]");
                if (list != null && list.Count > 0)
                {
                    requirementsMet = requirementsMet && list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText);
                    if (!requirementsMet)
                        return;
                }
                list = node.SelectNodes("/required/flexibility");
                if (list != null && list.Count > 0)
                {
                    check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboFlexibility.SelectedValue + "\"]");
                    requirementsMet = requirementsMet && list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText);
                    if (!requirementsMet)
                        return;
                }
                list = node.SelectNodes("/required/visibility");
                if (list != null && list.Count > 0)
                {
                    check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboVisibility.SelectedValue + "\"]");
                    requirementsMet = requirementsMet && list.Cast<XmlNode>().Any(n => n.InnerText == check?["name"]?.InnerText);
                    if (!requirementsMet)
                        return;
                }
            }
		    if (_mount == null)
		    {
		        _mount = new WeaponMount(_objCharacter, _vehicle);
		        _mount.Create(node, tree);
		    }
		    else
		    {
		        _mount.WeaponMountOptions.Clear();
		    }
            var option = new WeaponMountOption(_objCharacter);
            option.Create(cboControl.SelectedValue.ToString(), _mount.WeaponMountOptions);
            option = new WeaponMountOption(_objCharacter);
            option.Create(cboFlexibility.SelectedValue.ToString(), _mount.WeaponMountOptions);
            option = new WeaponMountOption(_objCharacter);
            option.Create(cboVisibility.SelectedValue.ToString(), _mount.WeaponMountOptions);
            WeaponMount = _mount;
            WeaponMount.Mods.Clear();
            foreach (var v in _lstMods)
		    {
		        WeaponMount.Mods.Add(v);
		    }
            tree.Text = _mount.DisplayName(GlobalOptions.Language);
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
	        if (_loading) return;
	        XmlNode node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboSize.SelectedValue + "\"]");
	        decimal cost = 0;
	        if (!chkFreeItem.Checked) cost = Convert.ToInt32(node?["cost"]?.InnerText);
	        int avail = 0;
	        string availSuffix = string.Empty;
	        int slots = Convert.ToInt32(node?["slots"]?.InnerText);

            string strAvail = node["avail"]?.InnerText ?? string.Empty;
            if (strAvail.EndsWith('F', 'R'))
	        {
	            availSuffix = strAvail.Substring(strAvail.Length - 1, 1);
	            avail = Convert.ToInt32(strAvail.Substring(0, strAvail.Length - 1));
	        }
	        List<object> boxes = new List<object>
	        {
	            cboVisibility.SelectedValue,
	            cboFlexibility.SelectedValue,
	            cboControl.SelectedValue
	        };
	        foreach (object box in boxes)
	        {
	            node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + box + "\"]");
	            if (node == null) continue;
	            avail += Convert.ToInt32(node["avail"]?.InnerText);
	            if (!chkFreeItem.Checked) cost += Convert.ToInt32(node["cost"]?.InnerText);
	            slots += Convert.ToInt32(node["slots"]?.InnerText);
	        }
	        cost *= 1 + (nudMarkup.Value / 100.0m);
	        lblCost.Text = cost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
	        lblSlots.Text = slots.ToString();
	        lblAvailability.Text = $@"{avail}{availSuffix}";
        }

        private void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            do
            {
                frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(_objCharacter)
                {
                    // Pass the selected vehicle on to the form.
                    SelectedVehicle = _vehicle,
                    VehicleMountMods = true
                };
                if (_mount != null)
                {
                    frmPickVehicleMod.InstalledMods = _mount.Mods;
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

                TreeNode objNode = new TreeNode();
                VehicleMod objMod = new VehicleMod(_objCharacter);
                objMod.Create(objXmlMod, objNode, frmPickVehicleMod.SelectedRating, _vehicle, frmPickVehicleMod.Markup);
                // Check the item's Cost and make sure the character can afford it.
                decimal decOriginalCost = _vehicle.TotalCost;
                if (frmPickVehicleMod.FreeCost)
                    objMod.Cost = "0";

                // Do not allow the user to add a new Vehicle Mod if the Vehicle's Capacity has been reached.
                if (_objCharacter.Options.EnforceCapacity)
                {
                    bool blnOverCapacity = false;
                    if (_objCharacter.Options.BookEnabled("R5"))
                    {
                        if (_vehicle.IsDrone && GlobalOptions.Dronemods)
                        {
                            if (_vehicle.DroneModSlotsUsed > _vehicle.DroneModSlots)
                                blnOverCapacity = true;
                        }
                        else
                        {
                            int intUsed = _vehicle.CalcCategoryUsed(objMod.Category);
                            int intAvail = _vehicle.CalcCategoryAvail(objMod.Category);
                            if (intUsed > intAvail)
                                blnOverCapacity = true;
                        }
                    }
                    else if (_vehicle.Slots < _vehicle.SlotsUsed)
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
                    decimal decCost = _vehicle.TotalCost - decOriginalCost;

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
                treMods.Nodes[0].Nodes.Add(objNode);
                treMods.Nodes[0].Expand();

                // Check for Improved Sensor bonus.
                if (objMod.Bonus?["selecttext"] != null)
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", objMod.DisplayNameShort(GlobalOptions.Language))
                    };
                    frmPickText.ShowDialog(this);
                    objMod.Extra = frmPickText.SelectedValue;
                    objNode.Text = objMod.DisplayName(GlobalOptions.Language);
                    frmPickText.Dispose();
                }
                frmPickVehicleMod.Dispose();
            }
            while (blnAddAgain);
        }
    }
}
