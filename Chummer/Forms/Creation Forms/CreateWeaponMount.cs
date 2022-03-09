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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class CreateWeaponMount : Form
    {
        private readonly List<VehicleMod> _lstMods = new List<VehicleMod>(1);
        private bool _blnLoading = true;
        private readonly bool _blnAllowEditOptions;
        private readonly Vehicle _objVehicle;
        private readonly Character _objCharacter;
        private WeaponMount _objMount;
        private readonly XmlDocument _xmlDoc;
        private readonly XPathNavigator _xmlDocXPath;
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private readonly decimal _decOldBaseCost;

        public WeaponMount WeaponMount => _objMount;

        public CreateWeaponMount(Vehicle objVehicle, Character objCharacter, WeaponMount objWeaponMount = null)
        {
            _objVehicle = objVehicle;
            _objMount = objWeaponMount;
            _objCharacter = objCharacter;
            // Needed for cost calculations
            _decOldBaseCost = _objCharacter.Created ? _objVehicle.TotalCost : 0;
            _blnAllowEditOptions = _objMount == null || (!_objMount.IncludedInVehicle && !_objCharacter.Created);
            _xmlDoc = _objCharacter.LoadData("vehicles.xml");
            _xmlDocXPath = _objCharacter.LoadDataXPath("vehicles.xml");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(
                                             _xmlDocXPath.SelectSingleNodeAndCacheExpression(
                                                              "/chummer/weaponmountcategories")));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void CreateWeaponMount_Load(object sender, EventArgs e)
        {
            XPathNavigator xmlVehicleNode = await _objVehicle.GetNodeXPathAsync();
            // Populate the Weapon Mount Category list.
            string strSizeFilter = "category = \"Size\" and " + _objCharacter.Settings.BookXPath();
            if (!_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                strSizeFilter += " and not(optionaldrone)";
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSize))
            {
                XPathNodeIterator xmlSizeNodeList
                    = _xmlDocXPath.Select("/chummer/weaponmounts/weaponmount[" + strSizeFilter + ']');
                if (xmlSizeNodeList.Count > 0)
                {
                    foreach (XPathNavigator xmlSizeNode in xmlSizeNodeList)
                    {
                        string strId = (await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XPathNavigator xmlTestNode = await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("forbidden/vehicledetails");
                        if (xmlTestNode != null && await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("required/vehicledetails");
                        if (xmlTestNode != null && !await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        lstSize.Add(new ListItem(
                                        strId,
                                        (await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                        ?? (await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                        ?? await LanguageManager.GetStringAsync("String_Unknown")));
                    }
                }

                cboSize.BeginUpdate();
                cboSize.PopulateWithListItems(lstSize);
                cboSize.Enabled = _blnAllowEditOptions && lstSize.Count > 1;
                cboSize.EndUpdate();
            }

            if (_objMount != null)
            {
                TreeNode objModsParentNode = new TreeNode
                {
                    Tag = "Node_AdditionalMods",
                    Text = await LanguageManager.GetStringAsync("Node_AdditionalMods")
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
                if (cboSize.Items.Count > 0)
                    cboSize.SelectedIndex = 0;
            }
            else
                await RefreshComboBoxes();

            nudMarkup.Visible = AllowDiscounts || _objMount?.Markup != 0;
            nudMarkup.Enabled = AllowDiscounts;
            lblMarkupLabel.Visible = nudMarkup.Visible;
            lblMarkupPercentLabel.Visible = nudMarkup.Visible;

            if (_objMount != null)
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstVisibility))
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFlexibility))
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstControl))
                {
                    lstVisibility.AddRange(cboVisibility.Items.Cast<ListItem>());
                    lstFlexibility.AddRange(cboFlexibility.Items.Cast<ListItem>());
                    lstControl.AddRange(cboControl.Items.Cast<ListItem>());
                    foreach (string strLoopId in _objMount.WeaponMountOptions.Select(x => x.SourceIDString))
                    {
                        if (lstVisibility.Any(x => x.Value.ToString() == strLoopId))
                            cboVisibility.SelectedValue = strLoopId;
                        else if (lstFlexibility.Any(x => x.Value.ToString() == strLoopId))
                            cboFlexibility.SelectedValue = strLoopId;
                        else if (lstControl.Any(x => x.Value.ToString() == strLoopId))
                            cboControl.SelectedValue = strLoopId;
                    }
                }
            }

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            _blnLoading = false;
            await UpdateInfo();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
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

            XmlNode xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');
            if (xmlSelectedMount == null)
                return;
            XmlNode xmlSelectedControl = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedControl.CleanXPath() + ']');
            if (xmlSelectedControl == null)
                return;
            XmlNode xmlSelectedFlexibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedFlexibility.CleanXPath() + ']');
            if (xmlSelectedFlexibility == null)
                return;
            XmlNode xmlSelectedVisibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedVisibility.CleanXPath() + ']');
            if (xmlSelectedVisibility == null)
                return;

            XmlNode xmlForbiddenNode = xmlSelectedMount["forbidden"];
            if (xmlForbiddenNode != null)
            {
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlControlNodeList = xmlForbiddenNode.SelectNodes("control"))
                    {
                        if (xmlControlNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlControlNodeList)
                            {
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                            }
                        }
                    }
                }

                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlFlexibilityNodeList = xmlForbiddenNode.SelectNodes("flexibility"))
                    {
                        if (xmlFlexibilityNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlFlexibilityNodeList)
                            {
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                            }
                        }
                    }
                }

                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlVisibilityNodeList = xmlForbiddenNode.SelectNodes("visibility"))
                    {
                        if (xmlVisibilityNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlVisibilityNodeList)
                            {
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                            }
                        }
                    }
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

            if (_blnAllowEditOptions)
            {
                if (_objMount == null)
                {
                    _objMount = new WeaponMount(_objCharacter, _objVehicle);
                    _objMount.Create(xmlSelectedMount, nudMarkup.Value);
                }
                else if (_objMount.SourceIDString != strSelectedMount)
                {
                    _objMount.Create(xmlSelectedMount, nudMarkup.Value);
                }

                _objMount.DiscountCost = chkBlackMarketDiscount.Checked;

                WeaponMountOption objControlOption = new WeaponMountOption(_objCharacter);
                if (objControlOption.Create(xmlSelectedControl))
                {
                    _objMount.WeaponMountOptions.RemoveAll(x => x.Category == "Control");
                    _objMount.WeaponMountOptions.Add(objControlOption);
                }

                WeaponMountOption objFlexibilityOption = new WeaponMountOption(_objCharacter);
                if (objFlexibilityOption.Create(xmlSelectedFlexibility))
                {
                    _objMount.WeaponMountOptions.RemoveAll(x => x.Category == "Flexibility");
                    _objMount.WeaponMountOptions.Add(objFlexibilityOption);
                }

                WeaponMountOption objVisibilityOption = new WeaponMountOption(_objCharacter);
                if (objVisibilityOption.Create(xmlSelectedVisibility))
                {
                    _objMount.WeaponMountOptions.RemoveAll(x => x.Category == "Visibility");
                    _objMount.WeaponMountOptions.Add(objVisibilityOption);
                }
            }

            List<VehicleMod> lstOldRemovedVehicleMods
                = _objMount.Mods.Where(x => !x.IncludedInVehicle && !_lstMods.Contains(x)).ToList();
            _objMount.Mods.RemoveAll(x => lstOldRemovedVehicleMods.Contains(x));
            List<VehicleMod> lstNewVehicleMods = new List<VehicleMod>(_lstMods.Count);
            foreach (VehicleMod objMod in _lstMods)
            {
                if (await _objMount.Mods.ContainsAsync(objMod))
                    continue;
                lstNewVehicleMods.Add(objMod);
                _objMount.Mods.Add(objMod);
            }

            if (_objCharacter.Created)
            {
                bool blnRemoveMountAfterCheck = false;
                // Check the item's Cost and make sure the character can afford it.
                if (!chkFreeItem.Checked)
                {
                    // New mount, so temporarily add it to parent vehicle to make sure we can capture everything
                    if (_objMount.Parent == null)
                    {
                        blnRemoveMountAfterCheck = true;
                        _objVehicle.WeaponMounts.Add(_objMount);
                    }

                    decimal decCost = _objVehicle.TotalCost - _decOldBaseCost;

                    // Multiply the cost if applicable.
                    switch (_objMount.TotalAvailTuple().Suffix)
                    {
                        case 'R' when _objCharacter.Settings.MultiplyRestrictedCost:
                            decCost *= _objCharacter.Settings.RestrictedCostMultiplier;
                            break;

                        case 'F' when _objCharacter.Settings.MultiplyForbiddenCost:
                            decCost *= _objCharacter.Settings.ForbiddenCostMultiplier;
                            break;
                    }

                    if (decCost > _objCharacter.Nuyen)
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"),
                                                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"),
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (blnRemoveMountAfterCheck)
                        {
                            _objVehicle.WeaponMounts.Remove(_objMount);
                            _objMount = null;
                        }
                        else
                        {
                            _objMount.Mods.RemoveAll(x => lstNewVehicleMods.Contains(x));
                            _objMount.Mods.AddRange(lstOldRemovedVehicleMods);
                        }
                        return;
                    }
                }

                // Do not allow the user to add a new Vehicle Mod if the Vehicle's Capacity has been reached.
                if (_objCharacter.Settings.EnforceCapacity)
                {
                    // New mount, so temporarily add it to parent vehicle to make sure we can capture everything
                    if (_objMount.Parent == null)
                    {
                        blnRemoveMountAfterCheck = true;
                        _objVehicle.WeaponMounts.Add(_objMount);
                    }
                    bool blnOverCapacity;
                    if (_objCharacter.Settings.BookEnabled("R5"))
                    {
                        if (_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                            blnOverCapacity = _objVehicle.DroneModSlotsUsed > _objVehicle.DroneModSlots;
                        else
                            blnOverCapacity = _objVehicle.OverR5Capacity("Weapons");
                    }
                    else
                        blnOverCapacity = _objVehicle.Slots < _objVehicle.SlotsUsed;

                    if (blnOverCapacity)
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"),
                                                        await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"),
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (blnRemoveMountAfterCheck)
                        {
                            _objVehicle.WeaponMounts.Remove(_objMount);
                            _objMount = null;
                        }
                        else
                        {
                            _objMount.Mods.RemoveAll(x => lstNewVehicleMods.Contains(x));
                            _objMount.Mods.AddRange(lstOldRemovedVehicleMods);
                        }
                        return;
                    }
                }

                if (blnRemoveMountAfterCheck)
                {
                    _objVehicle.WeaponMounts.Remove(_objMount);
                }
            }

            _objMount.FreeCost = chkFreeItem.Checked;

            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void cboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RefreshComboBoxes();
            treMods.SelectedNode = null;
            await UpdateInfo();
        }

        private async void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            treMods.SelectedNode = null;
            await UpdateInfo();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateInfo();
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateInfo();
        }

        public bool FreeCost => chkFreeItem.Checked;

        public decimal Markup => nudMarkup.Value;

        public bool AllowDiscounts { get; set; }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            await UpdateInfo();
        }

        private async ValueTask UpdateInfo()
        {
            if (_blnLoading)
                return;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedMount))
                cmdOK.Enabled = false;
            else
            {
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');
                if (xmlSelectedMount == null)
                    cmdOK.Enabled = false;
                else
                {
                    string strSelectedControl = cboControl.SelectedValue?.ToString();
                    if (string.IsNullOrEmpty(strSelectedControl))
                        cmdOK.Enabled = false;
                    else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedControl.CleanXPath() + ']') == null)
                        cmdOK.Enabled = false;
                    else
                    {
                        string strSelectedFlexibility = cboFlexibility.SelectedValue?.ToString();
                        if (string.IsNullOrEmpty(strSelectedFlexibility))
                            cmdOK.Enabled = false;
                        else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedFlexibility.CleanXPath() + ']') == null)
                            cmdOK.Enabled = false;
                        else
                        {
                            string strSelectedVisibility = cboVisibility.SelectedValue?.ToString();
                            if (string.IsNullOrEmpty(strSelectedVisibility))
                                cmdOK.Enabled = false;
                            else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedVisibility.CleanXPath() + ']') == null)
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
                VehicleMod objMod = _lstMods.Find(x => x.InternalId == strSelectedModId);
                if (objMod != null)
                {
                    cmdDeleteMod.Enabled = !objMod.IncludedInVehicle;
                    lblSlots.Text = objMod.CalculatedSlots.ToString(GlobalSettings.InvariantCultureInfo);
                    lblAvailability.Text = objMod.DisplayTotalAvail;

                    if (chkFreeItem.Checked)
                    {
                        lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                    }
                    else
                    {
                        int intTotalSlots = 0;
                        xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intTotalSlots);
                        foreach (string strSelectedId in astrSelectedValues)
                        {
                            if (!string.IsNullOrEmpty(strSelectedId))
                            {
                                XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedId.CleanXPath() + ']');
                                if (xmlLoopNode == null)
                                    continue;
                                int intLoopSlots = 0;
                                if (xmlLoopNode.TryGetInt32FieldQuickly("slots", ref intLoopSlots))
                                {
                                    intTotalSlots += intLoopSlots;
                                }
                            }
                        }
                        foreach (VehicleMod objLoopMod in _lstMods)
                        {
                            if (objMod.IncludedInVehicle)
                                continue;
                            intTotalSlots += objLoopMod.CalculatedSlots;
                        }
                        lblCost.Text = (objMod.TotalCostInMountCreation(intTotalSlots) * (1 + (nudMarkup.Value / 100.0m))).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
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
            // Cost.
            if (_blnAllowEditOptions)
            {
                bool blnCanBlackMarketDiscount
                    = _setBlackMarketMaps.Contains(xmlSelectedMount.SelectSingleNode("category")?.Value);
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
            }
            else
                chkBlackMarketDiscount.Enabled = false;

            decimal decCost = 0;
            if (!chkFreeItem.Checked && _blnAllowEditOptions)
                xmlSelectedMount.TryGetDecFieldQuickly("cost", ref decCost);
            int intSlots = 0;
            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

            string strAvail = xmlSelectedMount["avail"]?.InnerText ?? string.Empty;
            char chrAvailSuffix = strAvail.Length > 0 ? strAvail[strAvail.Length - 1] : ' ';
            if (chrAvailSuffix == 'F' || chrAvailSuffix == 'R')
                strAvail = strAvail.Substring(0, strAvail.Length - 1);
            else
                chrAvailSuffix = ' ';
            int.TryParse(strAvail, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intAvail);

            foreach (string strSelectedId in astrSelectedValues)
            {
                if (string.IsNullOrEmpty(strSelectedId))
                    continue;
                XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedId.CleanXPath() + ']');
                if (xmlLoopNode == null)
                    continue;
                if (!chkFreeItem.Checked && _blnAllowEditOptions)
                {
                    decimal decLoopCost = 0;
                    if (xmlLoopNode.TryGetDecFieldQuickly("cost", ref decLoopCost))
                        decCost += decLoopCost;
                }

                int intLoopSlots = 0;
                if (xmlLoopNode.TryGetInt32FieldQuickly("slots", ref intLoopSlots))
                    intSlots += intLoopSlots;

                string strLoopAvail = xmlLoopNode["avail"]?.InnerText ?? string.Empty;
                char chrLoopAvailSuffix = strLoopAvail.Length > 0 ? strLoopAvail[strLoopAvail.Length - 1] : ' ';
                switch (chrLoopAvailSuffix)
                {
                    case 'F':
                        strLoopAvail = strLoopAvail.Substring(0, strLoopAvail.Length - 1);
                        chrAvailSuffix = 'F';
                        break;

                    case 'R':
                        {
                            strLoopAvail = strLoopAvail.Substring(0, strLoopAvail.Length - 1);
                            if (chrAvailSuffix == ' ')
                                chrAvailSuffix = 'R';
                            break;
                        }
                }
                if (int.TryParse(strLoopAvail, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intLoopAvail))
                    intAvail += intLoopAvail;
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                if (objMod.IncludedInVehicle)
                    continue;
                intSlots += objMod.CalculatedSlots;
                AvailabilityValue objLoopAvail = objMod.TotalAvailTuple();
                char chrLoopAvailSuffix = objLoopAvail.Suffix;
                if (chrLoopAvailSuffix == 'F')
                    chrAvailSuffix = 'F';
                else if (chrAvailSuffix != 'F' && chrLoopAvailSuffix == 'R')
                    chrAvailSuffix = 'R';
                intAvail += objLoopAvail.Value;
            }
            if (!chkFreeItem.Checked)
            {
                foreach (VehicleMod objMod in _lstMods)
                {
                    if (objMod.IncludedInVehicle)
                        continue;
                    decCost += objMod.TotalCostInMountCreation(intSlots);
                }
            }

            if (chkBlackMarketDiscount.Checked)
                decCost *= 0.9m;

            string strAvailText = intAvail.ToString(GlobalSettings.CultureInfo);
            switch (chrAvailSuffix)
            {
                case 'F':
                    strAvailText += await LanguageManager.GetStringAsync("String_AvailForbidden");
                    break;

                case 'R':
                    strAvailText += await LanguageManager.GetStringAsync("String_AvailRestricted");
                    break;
            }

            decCost *= 1 + (nudMarkup.Value / 100.0m);

            lblCost.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            lblSlots.Text = intSlots.ToString(GlobalSettings.CultureInfo);
            lblAvailability.Text = strAvailText;
            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
            lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);
            lblAvailabilityLabel.Visible = !string.IsNullOrEmpty(lblAvailability.Text);

            string strSource = xmlSelectedMount["source"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown");
            string strPage = xmlSelectedMount["altpage"]?.InnerText ?? xmlSelectedMount["page"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown");
            SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
            objSourceString.SetControl(lblSource);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private async void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');

            int intSlots = 0;
            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

            string[] astrSelectedValues = { cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString() };
            foreach (string strSelectedId in astrSelectedValues)
            {
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedId.CleanXPath() + ']');
                    if (xmlLoopNode != null)
                    {
                        intSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText, GlobalSettings.InvariantCultureInfo);
                    }
                }
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                if (objMod.IncludedInVehicle)
                    continue;
                intSlots += objMod.CalculatedSlots;
            }
            
            TreeNode objModsParentNode = treMods.FindNode("Node_AdditionalMods");
            do
            {
                using (SelectVehicleMod frmPickVehicleMod = new SelectVehicleMod(_objCharacter, _objVehicle, _objMount?.Mods)
                {
                    // Pass the selected vehicle on to the form.
                    VehicleMountMods = true,
                    WeaponMountSlots = intSlots
                })
                {
                    await frmPickVehicleMod.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                        break;

                    blnAddAgain = frmPickVehicleMod.AddAgain;
                    XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("vehicles.xml");
                    XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/weaponmountmods/mod[id = " + frmPickVehicleMod.SelectedMod.CleanXPath() + ']');

                    VehicleMod objMod = new VehicleMod(_objCharacter)
                    {
                        DiscountCost = frmPickVehicleMod.BlackMarketDiscount
                    };
                    objMod.Create(objXmlMod, frmPickVehicleMod.SelectedRating, _objVehicle, frmPickVehicleMod.Markup);
                    if (frmPickVehicleMod.FreeCost)
                        objMod.Cost = "0";
                    _objMount?.Mods.Add(objMod);
                    _lstMods.Add(objMod);
                    intSlots += objMod.CalculatedSlots;

                    TreeNode objNewNode = objMod.CreateTreeNode(null, null, null, null, null, null);

                    if (objModsParentNode == null)
                    {
                        objModsParentNode = new TreeNode
                        {
                            Tag = "Node_AdditionalMods",
                            Text = await LanguageManager.GetStringAsync("Node_AdditionalMods")
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
            if (objSelectedNode == null)
                return;
            string strSelectedId = objSelectedNode.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId) && strSelectedId.IsGuid())
            {
                VehicleMod objMod = _lstMods.Find(x => x.InternalId == strSelectedId);
                if (objMod?.IncludedInVehicle != false)
                    return;
                if (!objMod.Remove())
                    return;
                _lstMods.Remove(objMod);
                TreeNode objParentNode = objSelectedNode.Parent;
                objSelectedNode.Remove();
                if (objParentNode.Nodes.Count == 0)
                    objParentNode.Remove();
            }
        }

        private async void treMods_AfterSelect(object sender, TreeViewEventArgs e)
        {
            await UpdateInfo();
        }

        private async ValueTask RefreshComboBoxes()
        {
            XPathNavigator xmlRequiredNode = null;
            XPathNavigator xmlForbiddenNode = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
            {
                XPathNavigator xmlSelectedMount = _xmlDocXPath.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');
                if (xmlSelectedMount != null)
                {
                    xmlForbiddenNode = await xmlSelectedMount.SelectSingleNodeAndCacheExpressionAsync("forbidden/weaponmountdetails");
                    xmlRequiredNode = await xmlSelectedMount.SelectSingleNodeAndCacheExpressionAsync("required/weaponmountdetails");
                }
            }

            XPathNavigator xmlVehicleNode = await _objVehicle.GetNodeXPathAsync();
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstVisibility))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFlexibility))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstControl))
            {
                // Populate the Weapon Mount Category list.
                string strFilter = "category != \"Size\" and " + _objCharacter.Settings.BookXPath();
                if (!_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                    strFilter += " and not(optionaldrone)";
                XPathNodeIterator xmlWeaponMountOptionNodeList
                    = _xmlDocXPath.Select("/chummer/weaponmounts/weaponmount[" + strFilter + ']');
                if (xmlWeaponMountOptionNodeList.Count > 0)
                {
                    foreach (XPathNavigator xmlWeaponMountOptionNode in xmlWeaponMountOptionNodeList)
                    {
                        string strId = xmlWeaponMountOptionNode.SelectSingleNode("id")?.Value;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XPathNavigator xmlTestNode = await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("forbidden/vehicledetails");
                        if (xmlTestNode != null && await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("required/vehicledetails");
                        if (xmlTestNode != null && !await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        string strName = (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                         ?? await LanguageManager.GetStringAsync("String_Unknown");
                        bool blnAddItem = true;
                        switch ((await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("category"))?.Value)
                        {
                            case "Visibility":
                            {
                                XPathNodeIterator xmlNodeList = xmlForbiddenNode?.Select("visibility");
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XPathNavigator xmlLoopNode in xmlNodeList)
                                    {
                                        if (xmlLoopNode.Value == strName)
                                        {
                                            blnAddItem = false;
                                            break;
                                        }
                                    }
                                }

                                if (xmlRequiredNode != null)
                                {
                                    blnAddItem = false;
                                    xmlNodeList = xmlRequiredNode.Select("visibility");
                                    if (xmlNodeList.Count > 0)
                                    {
                                        foreach (XPathNavigator xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.Value == strName)
                                            {
                                                blnAddItem = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (blnAddItem)
                                    lstVisibility.Add(
                                        new ListItem(
                                            strId, (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? strName));
                            }
                                break;

                            case "Flexibility":
                            {
                                if (xmlForbiddenNode != null)
                                {
                                    XPathNodeIterator xmlNodeList
                                        = await xmlForbiddenNode.SelectAndCacheExpressionAsync("flexibility");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XPathNavigator xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.Value == strName)
                                            {
                                                blnAddItem = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (xmlRequiredNode != null)
                                {
                                    blnAddItem = false;
                                    XPathNodeIterator xmlNodeList = await xmlRequiredNode.SelectAndCacheExpressionAsync("flexibility");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XPathNavigator xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.Value == strName)
                                            {
                                                blnAddItem = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (blnAddItem)
                                    lstFlexibility.Add(
                                        new ListItem(
                                            strId, (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? strName));
                            }
                                break;

                            case "Control":
                            {
                                if (xmlForbiddenNode != null)
                                {
                                    XPathNodeIterator xmlNodeList
                                        = await xmlForbiddenNode.SelectAndCacheExpressionAsync("control");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XPathNavigator xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.Value == strName)
                                            {
                                                blnAddItem = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (xmlRequiredNode != null)
                                {
                                    blnAddItem = false;
                                    XPathNodeIterator xmlNodeList = await xmlRequiredNode.SelectAndCacheExpressionAsync("control");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XPathNavigator xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.Value == strName)
                                            {
                                                blnAddItem = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (blnAddItem)
                                    lstControl.Add(
                                        new ListItem(
                                            strId, (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? strName));
                            }
                                break;

                            default:
                                Utils.BreakIfDebug();
                                break;
                        }
                    }
                }

                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                string strOldVisibility = cboVisibility.SelectedValue?.ToString();
                string strOldFlexibility = cboFlexibility.SelectedValue?.ToString();
                string strOldControl = cboControl.SelectedValue?.ToString();
                cboVisibility.BeginUpdate();
                cboVisibility.PopulateWithListItems(lstVisibility);
                cboVisibility.Enabled = _blnAllowEditOptions && lstVisibility.Count > 1;
                if (!string.IsNullOrEmpty(strOldVisibility))
                    cboVisibility.SelectedValue = strOldVisibility;
                if (cboVisibility.SelectedIndex == -1 && lstVisibility.Count > 0)
                    cboVisibility.SelectedIndex = 0;
                cboVisibility.EndUpdate();

                cboFlexibility.BeginUpdate();
                cboFlexibility.PopulateWithListItems(lstFlexibility);
                cboFlexibility.Enabled = _blnAllowEditOptions && lstFlexibility.Count > 1;
                if (!string.IsNullOrEmpty(strOldFlexibility))
                    cboFlexibility.SelectedValue = strOldFlexibility;
                if (cboFlexibility.SelectedIndex == -1 && lstFlexibility.Count > 0)
                    cboFlexibility.SelectedIndex = 0;
                cboFlexibility.EndUpdate();

                cboControl.BeginUpdate();
                cboControl.PopulateWithListItems(lstControl);
                cboControl.Enabled = _blnAllowEditOptions && lstControl.Count > 1;
                if (!string.IsNullOrEmpty(strOldControl))
                    cboControl.SelectedValue = strOldControl;
                if (cboControl.SelectedIndex == -1 && lstControl.Count > 0)
                    cboControl.SelectedIndex = 0;
                cboControl.EndUpdate();

                _blnLoading = blnOldLoading;
            }
        }

        private async void lblSource_Click(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }
    }
}
