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
using System.Threading;
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
                
                await cboSize.PopulateWithListItemsAsync(lstSize);
                await cboSize.DoThreadSafeAsync(x => x.Enabled = _blnAllowEditOptions && lstSize.Count > 1);
            }

            if (_objMount != null)
            {
                TreeNode objModsParentNode = new TreeNode
                {
                    Tag = "Node_AdditionalMods",
                    Text = await LanguageManager.GetStringAsync("Node_AdditionalMods")
                };
                await treMods.DoThreadSafeAsync(x =>
                {
                    x.Nodes.Add(objModsParentNode);
                    objModsParentNode.Expand();
                    foreach (VehicleMod objMod in _objMount.Mods)
                    {
                        TreeNode objLoopNode = objMod.CreateTreeNode(null, null, null, null, null, null);
                        if (objLoopNode != null)
                            objModsParentNode.Nodes.Add(objLoopNode);
                    }
                });
                _lstMods.AddRange(_objMount.Mods);

                await cboSize.DoThreadSafeAsync(x => x.SelectedValue = _objMount.SourceIDString);
            }
            if (await cboSize.DoThreadSafeFuncAsync(x => x.SelectedIndex) == -1)
            {
                await cboSize.DoThreadSafeAsync(x =>
                {
                    if (x.Items.Count > 0)
                        x.SelectedIndex = 0;
                });
            }
            else
                await RefreshComboBoxes();

            await nudMarkup.DoThreadSafeAsync(x =>
            {
                x.Visible = AllowDiscounts || _objMount?.Markup != 0;
                x.Enabled = AllowDiscounts;
                lblMarkupLabel.DoThreadSafe(y => y.Visible = x.Visible);
                lblMarkupPercentLabel.DoThreadSafe(y => y.Visible = x.Visible);
            });

            if (_objMount != null)
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstVisibility))
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFlexibility))
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstControl))
                {
                    lstVisibility.AddRange(await cboVisibility.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>()));
                    lstFlexibility.AddRange(await cboFlexibility.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>()));
                    lstControl.AddRange(await cboControl.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>()));
                    foreach (string strLoopId in _objMount.WeaponMountOptions.Select(x => x.SourceIDString))
                    {
                        if (lstVisibility.Any(x => x.Value.ToString() == strLoopId))
                            await cboVisibility.DoThreadSafeAsync(x => x.SelectedValue = strLoopId);
                        else if (lstFlexibility.Any(x => x.Value.ToString() == strLoopId))
                            await cboFlexibility.DoThreadSafeAsync(x => x.SelectedValue = strLoopId);
                        else if (lstControl.Any(x => x.Value.ToString() == strLoopId))
                            await cboControl.DoThreadSafeAsync(x => x.SelectedValue = strLoopId);
                    }
                }
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            _blnLoading = false;
            await UpdateInfo();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            //TODO: THIS IS UGLY AS SHIT, FIX BETTER

            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (string.IsNullOrEmpty(strSelectedMount))
                return;
            string strSelectedControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (string.IsNullOrEmpty(strSelectedControl))
                return;
            string strSelectedFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (string.IsNullOrEmpty(strSelectedFlexibility))
                return;
            string strSelectedVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
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
                    _objMount.Create(xmlSelectedMount, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value));
                }
                else if (_objMount.SourceIDString != strSelectedMount)
                {
                    _objMount.Create(xmlSelectedMount, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value));
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
                if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
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
            await treMods.DoThreadSafeAsync(x => x.SelectedNode = null);
            await UpdateInfo();
        }

        private async void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await treMods.DoThreadSafeAsync(x => x.SelectedNode = null);
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

        private async ValueTask UpdateInfo(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_blnLoading)
                return;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
            if (string.IsNullOrEmpty(strSelectedMount))
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
            else
            {
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');
                if (xmlSelectedMount == null)
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false);
                else
                {
                    string strSelectedControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
                    if (string.IsNullOrEmpty(strSelectedControl))
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                    else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedControl.CleanXPath() + ']') == null)
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                    else
                    {
                        string strSelectedFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
                        if (string.IsNullOrEmpty(strSelectedFlexibility))
                            await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                        else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedFlexibility.CleanXPath() + ']') == null)
                            await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                        else
                        {
                            string strSelectedVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
                            if (string.IsNullOrEmpty(strSelectedVisibility))
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                            else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedVisibility.CleanXPath() + ']') == null)
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                            else
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token);
                        }
                    }
                }
            }

            string[] astrSelectedValues =
            {
                await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token),
                await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token),
                await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
            };

            string strLoop;
            await cmdDeleteMod.DoThreadSafeAsync(x => x.Enabled = false, token);
            string strSelectedModId = await treMods.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag.ToString(), token);
            if (!string.IsNullOrEmpty(strSelectedModId) && strSelectedModId.IsGuid())
            {
                VehicleMod objMod = _lstMods.Find(x => x.InternalId == strSelectedModId);
                if (objMod != null)
                {
                    await cmdDeleteMod.DoThreadSafeAsync(x => x.Enabled = !objMod.IncludedInVehicle, token);
                    await lblSlots.DoThreadSafeAsync(x => x.Text = objMod.CalculatedSlots.ToString(GlobalSettings.InvariantCultureInfo), token);
                    await lblAvailability.DoThreadSafeAsync(x => x.Text = objMod.DisplayTotalAvail, token);

                    if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token))
                    {
                        await lblCost.DoThreadSafeAsync(x => x.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥', token);
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

                        decimal decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token);
                        await lblCost.DoThreadSafeAsync(
                            x => x.Text
                                = (objMod.TotalCostInMountCreation(intTotalSlots) * (1 + decMarkup / 100.0m))
                                .ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥', token);
                    }

                    await objMod.SetSourceDetailAsync(lblSource, token);
                    strLoop = await lblCost.DoThreadSafeFuncAsync(x => x.Text, token);
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
                    strLoop = await lblSlots.DoThreadSafeFuncAsync(x => x.Text, token);
                    await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
                    strLoop = await lblAvailability.DoThreadSafeFuncAsync(x => x.Text, token);
                    await lblAvailabilityLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
                    strLoop = await lblSource.DoThreadSafeFuncAsync(x => x.Text, token);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
                    return;
                }
            }

            if (xmlSelectedMount == null)
            {
                await lblCost.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblSlots.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblAvailability.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                await lblCostLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                await lblAvailabilityLabel.DoThreadSafeAsync(x => x.Visible = false, token);
                return;
            }
            // Cost.
            if (_blnAllowEditOptions)
            {
                bool blnCanBlackMarketDiscount
                    = _setBlackMarketMaps.Contains(xmlSelectedMount.SelectSingleNode("category")?.Value);
                await chkBlackMarketDiscount.DoThreadSafeAsync(x =>
                {
                    x.Enabled = blnCanBlackMarketDiscount;
                    if (!x.Checked)
                    {
                        x.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                    }
                    else if (!blnCanBlackMarketDiscount)
                    {
                        //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                        x.Checked = false;
                    }
                }, token);
            }
            else
                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Enabled = false, token);

            decimal decCost = 0;
            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token) && _blnAllowEditOptions)
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
                if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token) && _blnAllowEditOptions)
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
            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token))
            {
                foreach (VehicleMod objMod in _lstMods)
                {
                    if (objMod.IncludedInVehicle)
                        continue;
                    decCost += objMod.TotalCostInMountCreation(intSlots);
                }
            }

            if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token))
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

            decCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token) / 100.0m;

            await lblCost.DoThreadSafeAsync(x => x.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥', token);
            await lblSlots.DoThreadSafeAsync(x => x.Text = intSlots.ToString(GlobalSettings.CultureInfo), token);
            await lblAvailability.DoThreadSafeAsync(x => x.Text = strAvailText, token);
            string strSource = xmlSelectedMount["source"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown");
            string strPage = xmlSelectedMount["altpage"]?.InnerText ?? xmlSelectedMount["page"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown");
            SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
            await objSourceString.SetControlAsync(lblSource, token);
            strLoop = await lblCost.DoThreadSafeFuncAsync(x => x.Text, token);
            await lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
            strLoop = await lblSlots.DoThreadSafeFuncAsync(x => x.Text, token);
            await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
            strLoop = await lblAvailability.DoThreadSafeFuncAsync(x => x.Text, token);
            await lblAvailabilityLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
            strLoop = await lblSource.DoThreadSafeFuncAsync(x => x.Text, token);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop), token);
        }

        private async void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (!string.IsNullOrEmpty(strSelectedMount))
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');

            int intSlots = 0;
            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

            string[] astrSelectedValues =
            {
                await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()),
                await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()),
                await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
            };

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
            
            TreeNode objModsParentNode = await treMods.DoThreadSafeFuncAsync(x => x.FindNode("Node_AdditionalMods"));
            do
            {
                using (SelectVehicleMod frmPickVehicleMod = await this.DoThreadSafeFuncAsync(() => new SelectVehicleMod(_objCharacter, _objVehicle, _objMount?.Mods)
                {
                    // Pass the selected vehicle on to the form.
                    VehicleMountMods = true,
                    WeaponMountSlots = intSlots
                }))
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
                        await treMods.DoThreadSafeAsync(x =>
                        {
                            x.Nodes.Add(objModsParentNode);
                            objModsParentNode.Expand();
                        });
                    }

                    await treMods.DoThreadSafeAsync(x =>
                    {
                        objModsParentNode.Nodes.Add(objNewNode);
                        x.SelectedNode = objNewNode;
                    });
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
            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
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
                string strOldVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                string strOldFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                string strOldControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                await cboVisibility.PopulateWithListItemsAsync(lstVisibility);
                await cboVisibility.DoThreadSafeAsync(x =>
                {
                    x.Enabled = _blnAllowEditOptions && lstVisibility.Count > 1;
                    if (!string.IsNullOrEmpty(strOldVisibility))
                        x.SelectedValue = strOldVisibility;
                    if (x.SelectedIndex == -1 && lstVisibility.Count > 0)
                        x.SelectedIndex = 0;
                });
                await cboFlexibility.PopulateWithListItemsAsync(lstFlexibility);
                await cboFlexibility.DoThreadSafeAsync(x =>
                {
                    x.Enabled = _blnAllowEditOptions && lstFlexibility.Count > 1;
                    if (!string.IsNullOrEmpty(strOldFlexibility))
                        x.SelectedValue = strOldFlexibility;
                    if (x.SelectedIndex == -1 && lstFlexibility.Count > 0)
                        x.SelectedIndex = 0;
                });
                await cboControl.PopulateWithListItemsAsync(lstControl);
                await cboControl.DoThreadSafeAsync(x =>
                {
                    x.Enabled = _blnAllowEditOptions && lstControl.Count > 1;
                    if (!string.IsNullOrEmpty(strOldControl))
                        x.SelectedValue = strOldControl;
                    if (x.SelectedIndex == -1 && lstControl.Count > 0)
                        x.SelectedIndex = 0;
                });

                _blnLoading = blnOldLoading;
            }
        }

        private async void lblSource_Click(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }
    }
}
