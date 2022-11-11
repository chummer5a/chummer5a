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
        private int _intLoading = 1;
        private readonly bool _blnAllowEditOptions;
        private readonly Vehicle _objVehicle;
        private readonly Character _objCharacter;
        private WeaponMount _objMount;
        private readonly XmlDocument _xmlDoc;
        private readonly XPathNavigator _xmlDocXPath;
        private HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private readonly decimal _decOldBaseCost;

        public WeaponMount WeaponMount => _objMount;

        public CreateWeaponMount(Vehicle objVehicle, Character objCharacter, WeaponMount objWeaponMount = null)
        {
            Disposed += (sender, args) => Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
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
            XPathNavigator xmlVehicleNode = await _objVehicle.GetNodeXPathAsync().ConfigureAwait(false);
            // Populate the Weapon Mount Category list.
            string strSizeFilter = "category = \"Size\" and " + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false);
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
                        string strId = (await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("id").ConfigureAwait(false))?.Value;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XPathNavigator xmlTestNode = await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("forbidden/vehicledetails").ConfigureAwait(false);
                        if (xmlTestNode != null && await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("required/vehicledetails").ConfigureAwait(false);
                        if (xmlTestNode != null && !await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        lstSize.Add(new ListItem(
                                        strId,
                                        (await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                        ?? (await xmlSizeNode.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false))?.Value
                                        ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false)));
                    }
                }

                await cboSize.PopulateWithListItemsAsync(lstSize).ConfigureAwait(false);
                await cboSize.DoThreadSafeAsync(x => x.Enabled = _blnAllowEditOptions && lstSize.Count > 1).ConfigureAwait(false);
            }

            if (_objMount != null)
            {
                TreeNode objModsParentNode = new TreeNode
                {
                    Tag = "Node_AdditionalMods",
                    Text = await LanguageManager.GetStringAsync("Node_AdditionalMods").ConfigureAwait(false)
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
                }).ConfigureAwait(false);
                _lstMods.AddRange(_objMount.Mods);

                await cboSize.DoThreadSafeAsync(x => x.SelectedValue = _objMount.SourceIDString).ConfigureAwait(false);
            }
            if (await cboSize.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false) == -1)
            {
                await cboSize.DoThreadSafeAsync(x =>
                {
                    if (x.Items.Count > 0)
                        x.SelectedIndex = 0;
                }).ConfigureAwait(false);
            }
            else
                await RefreshComboBoxes().ConfigureAwait(false);

            await nudMarkup.DoThreadSafeAsync(x =>
            {
                x.Visible = AllowDiscounts || _objMount?.Markup != 0;
                x.Enabled = AllowDiscounts;
                lblMarkupLabel.DoThreadSafe(y => y.Visible = x.Visible);
                lblMarkupPercentLabel.DoThreadSafe(y => y.Visible = x.Visible);
            }).ConfigureAwait(false);

            if (_objMount != null)
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstVisibility))
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFlexibility))
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstControl))
                {
                    lstVisibility.AddRange(await cboVisibility.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>()).ConfigureAwait(false));
                    lstFlexibility.AddRange(await cboFlexibility.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>()).ConfigureAwait(false));
                    lstControl.AddRange(await cboControl.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>()).ConfigureAwait(false));
                    foreach (string strLoopId in _objMount.WeaponMountOptions.Select(x => x.SourceIDString))
                    {
                        if (lstVisibility.Any(x => x.Value.ToString() == strLoopId))
                            await cboVisibility.DoThreadSafeAsync(x => x.SelectedValue = strLoopId).ConfigureAwait(false);
                        else if (lstFlexibility.Any(x => x.Value.ToString() == strLoopId))
                            await cboFlexibility.DoThreadSafeAsync(x => x.SelectedValue = strLoopId).ConfigureAwait(false);
                        else if (lstControl.Any(x => x.Value.ToString() == strLoopId))
                            await cboControl.DoThreadSafeAsync(x => x.SelectedValue = strLoopId).ConfigureAwait(false);
                    }
                }
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount).ConfigureAwait(false);

            Interlocked.Decrement(ref _intLoading);
            await UpdateInfo().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            //TODO: THIS IS UGLY AS SHIT, FIX BETTER

            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedMount))
                return;
            string strSelectedControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedControl))
                return;
            string strSelectedFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedFlexibility))
                return;
            string strSelectedVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
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
                    _objMount.Create(xmlSelectedMount, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false));
                }
                else if (_objMount.SourceIDString != strSelectedMount)
                {
                    _objMount.Create(xmlSelectedMount, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false));
                }

                _objMount.DiscountCost = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);

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
                = await _objMount.Mods.ToListAsync(x => !x.IncludedInVehicle && !_lstMods.Contains(x)).ConfigureAwait(false);
            await _objMount.Mods.RemoveAllAsync(x => lstOldRemovedVehicleMods.Contains(x)).ConfigureAwait(false);
            List<VehicleMod> lstNewVehicleMods = new List<VehicleMod>(_lstMods.Count);
            foreach (VehicleMod objMod in _lstMods)
            {
                if (await _objMount.Mods.ContainsAsync(objMod).ConfigureAwait(false))
                    continue;
                lstNewVehicleMods.Add(objMod);
                await _objMount.Mods.AddAsync(objMod).ConfigureAwait(false);
            }

            if (_objCharacter.Created)
            {
                bool blnRemoveMountAfterCheck = false;
                // Check the item's Cost and make sure the character can afford it.
                if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                {
                    // New mount, so temporarily add it to parent vehicle to make sure we can capture everything
                    if (_objMount.Parent == null)
                    {
                        blnRemoveMountAfterCheck = true;
                        await _objVehicle.WeaponMounts.AddAsync(_objMount).ConfigureAwait(false);
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
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen").ConfigureAwait(false),
                                                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen").ConfigureAwait(false),
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (blnRemoveMountAfterCheck)
                        {
                            await _objVehicle.WeaponMounts.RemoveAsync(_objMount).ConfigureAwait(false);
                            _objMount = null;
                        }
                        else
                        {
                            await _objMount.Mods.RemoveAllAsync(x => lstNewVehicleMods.Contains(x)).ConfigureAwait(false);
                            await _objMount.Mods.AddRangeAsync(lstOldRemovedVehicleMods).ConfigureAwait(false);
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
                        await _objVehicle.WeaponMounts.AddAsync(_objMount).ConfigureAwait(false);
                    }
                    bool blnOverCapacity;
                    if (await _objCharacter.Settings.BookEnabledAsync("R5").ConfigureAwait(false))
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
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached").ConfigureAwait(false),
                                                        await LanguageManager.GetStringAsync("MessageTitle_CapacityReached").ConfigureAwait(false),
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (blnRemoveMountAfterCheck)
                        {
                            await _objVehicle.WeaponMounts.RemoveAsync(_objMount).ConfigureAwait(false);
                            _objMount = null;
                        }
                        else
                        {
                            await _objMount.Mods.RemoveAllAsync(x => lstNewVehicleMods.Contains(x)).ConfigureAwait(false);
                            await _objMount.Mods.AddRangeAsync(lstOldRemovedVehicleMods).ConfigureAwait(false);
                        }
                        return;
                    }
                }

                if (blnRemoveMountAfterCheck)
                {
                    await _objVehicle.WeaponMounts.RemoveAsync(_objMount).ConfigureAwait(false);
                }
            }

            _objMount.FreeCost = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RefreshComboBoxes().ConfigureAwait(false);
            await treMods.DoThreadSafeAsync(x => x.SelectedNode = null).ConfigureAwait(false);
            await UpdateInfo().ConfigureAwait(false);
        }

        private async void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await treMods.DoThreadSafeAsync(x => x.SelectedNode = null).ConfigureAwait(false);
            await UpdateInfo().ConfigureAwait(false);
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateInfo().ConfigureAwait(false);
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateInfo().ConfigureAwait(false);
        }

        public bool AllowDiscounts { get; set; }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            await UpdateInfo().ConfigureAwait(false);
        }

        private async ValueTask UpdateInfo(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intLoading > 0)
                return;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedMount))
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            else
            {
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');
                if (xmlSelectedMount == null)
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                else
                {
                    string strSelectedControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strSelectedControl))
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedControl.CleanXPath() + ']') == null)
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    else
                    {
                        string strSelectedFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strSelectedFlexibility))
                            await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                        else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedFlexibility.CleanXPath() + ']') == null)
                            await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                        else
                        {
                            string strSelectedVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                            if (string.IsNullOrEmpty(strSelectedVisibility))
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                            else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedVisibility.CleanXPath() + ']') == null)
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                            else
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                        }
                    }
                }
            }

            string[] astrSelectedValues =
            {
                await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false),
                await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false),
                await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
            };

            await cmdDeleteMod.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            string strSelectedModId = await treMods.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedModId) && strSelectedModId.IsGuid())
            {
                VehicleMod objMod = _lstMods.Find(x => x.InternalId == strSelectedModId);
                if (objMod != null)
                {
                    await cmdDeleteMod.DoThreadSafeAsync(x => x.Enabled = !objMod.IncludedInVehicle, token).ConfigureAwait(false);
                    await lblSlots.DoThreadSafeAsync(x => x.Text = objMod.CalculatedSlots.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                    await lblAvailability.DoThreadSafeAsync(x => x.Text = objMod.DisplayTotalAvail, token).ConfigureAwait(false);

                    if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                    {
                        await lblCost.DoThreadSafeAsync(x => x.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token).ConfigureAwait(false);
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

                        decimal decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false);
                        await lblCost.DoThreadSafeAsync(
                            x => x.Text
                                = (objMod.TotalCostInMountCreation(intTotalSlots) * (1 + decMarkup / 100.0m))
                                .ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token).ConfigureAwait(false);
                    }

                    await objMod.SetSourceDetailAsync(lblSource, token).ConfigureAwait(false);
                    string strLoop1 = await lblCost.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop1), token).ConfigureAwait(false);
                    string strLoop2 = await lblSlots.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                    await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop2), token).ConfigureAwait(false);
                    string strLoop3 = await lblAvailability.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                    await lblAvailabilityLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop3), token).ConfigureAwait(false);
                    string strLoop4 = await lblSource.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop4), token).ConfigureAwait(false);
                    return;
                }
            }

            if (xmlSelectedMount == null)
            {
                await lblCost.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblSlots.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblAvailability.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                await lblCostLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblAvailabilityLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
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
                }, token).ConfigureAwait(false);
            }
            else
                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);

            decimal decCost = 0;
            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false) && _blnAllowEditOptions)
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
                if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false) && _blnAllowEditOptions)
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
            if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
            {
                foreach (VehicleMod objMod in _lstMods)
                {
                    if (objMod.IncludedInVehicle)
                        continue;
                    decCost += objMod.TotalCostInMountCreation(intSlots);
                }
            }

            if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                decCost *= 0.9m;

            string strAvailText = intAvail.ToString(GlobalSettings.CultureInfo);
            switch (chrAvailSuffix)
            {
                case 'F':
                    strAvailText += await LanguageManager.GetStringAsync("String_AvailForbidden", token: token).ConfigureAwait(false);
                    break;

                case 'R':
                    strAvailText += await LanguageManager.GetStringAsync("String_AvailRestricted", token: token).ConfigureAwait(false);
                    break;
            }

            decCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false) / 100.0m;

            await lblCost.DoThreadSafeAsync(x => x.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token).ConfigureAwait(false);
            await lblSlots.DoThreadSafeAsync(x => x.Text = intSlots.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);
            await lblAvailability.DoThreadSafeAsync(x => x.Text = strAvailText, token).ConfigureAwait(false);
            string strSource = xmlSelectedMount["source"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
            string strPage = xmlSelectedMount["altpage"]?.InnerText ?? xmlSelectedMount["page"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
            SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
            await objSourceString.SetControlAsync(lblSource, token).ConfigureAwait(false);
            string strLoop5 = await lblCost.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            await lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop5), token).ConfigureAwait(false);
            string strLoop6 = await lblSlots.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            await lblSlotsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop6), token).ConfigureAwait(false);
            string strLoop7 = await lblAvailability.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            await lblAvailabilityLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop7), token).ConfigureAwait(false);
            string strLoop8 = await lblSource.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strLoop8), token).ConfigureAwait(false);
        }

        private async void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedMount))
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');

            int intSlots = 0;
            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

            string[] astrSelectedValues =
            {
                await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false),
                await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false),
                await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)
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

            TreeNode objModsParentNode = await treMods.DoThreadSafeFuncAsync(x => x.FindNode("Node_AdditionalMods")).ConfigureAwait(false);
            do
            {
                int intLoopSlots = intSlots;
                using (ThreadSafeForm<SelectVehicleMod> frmPickVehicleMod =
                       await ThreadSafeForm<SelectVehicleMod>.GetAsync(() =>
                                                                           new SelectVehicleMod(_objCharacter, _objVehicle, _objMount?.Mods)
                                                                           {
                                                                               // Pass the selected vehicle on to the form.
                                                                               VehicleMountMods = true,
                                                                               WeaponMountSlots = intLoopSlots
                                                                           }).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickVehicleMod.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.Cancel)
                        break;

                    blnAddAgain = frmPickVehicleMod.MyForm.AddAgain;
                    XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("vehicles.xml").ConfigureAwait(false);
                    XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/weaponmountmods/mod[id = " + frmPickVehicleMod.MyForm.SelectedMod.CleanXPath() + ']');

                    VehicleMod objMod = new VehicleMod(_objCharacter)
                    {
                        DiscountCost = frmPickVehicleMod.MyForm.BlackMarketDiscount
                    };
                    objMod.Create(objXmlMod, frmPickVehicleMod.MyForm.SelectedRating, _objVehicle, frmPickVehicleMod.MyForm.Markup);
                    if (frmPickVehicleMod.MyForm.FreeCost)
                        objMod.Cost = "0";
                    if (_objMount != null)
                        await _objMount.Mods.AddAsync(objMod).ConfigureAwait(false);
                    _lstMods.Add(objMod);
                    intSlots += objMod.CalculatedSlots;

                    TreeNode objNewNode = objMod.CreateTreeNode(null, null, null, null, null, null);

                    if (objModsParentNode == null)
                    {
                        objModsParentNode = new TreeNode
                        {
                            Tag = "Node_AdditionalMods",
                            Text = await LanguageManager.GetStringAsync("Node_AdditionalMods").ConfigureAwait(false)
                        };
                        TreeNode objLoopNode = objModsParentNode;
                        await treMods.DoThreadSafeAsync(x =>
                        {
                            x.Nodes.Add(objLoopNode);
                            objLoopNode.Expand();
                        }).ConfigureAwait(false);
                    }

                    TreeNode objLoopNode2 = objModsParentNode;
                    await treMods.DoThreadSafeAsync(x =>
                    {
                        objLoopNode2.Nodes.Add(objNewNode);
                        x.SelectedNode = objNewNode;
                    }).ConfigureAwait(false);
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
            await UpdateInfo().ConfigureAwait(false);
        }

        private async ValueTask RefreshComboBoxes(CancellationToken token = default)
        {
            XPathNavigator xmlRequiredNode = null;
            XPathNavigator xmlForbiddenNode = null;
            string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedMount))
            {
                XPathNavigator xmlSelectedMount = _xmlDocXPath.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + ']');
                if (xmlSelectedMount != null)
                {
                    xmlForbiddenNode = await xmlSelectedMount.SelectSingleNodeAndCacheExpressionAsync("forbidden/weaponmountdetails", token: token).ConfigureAwait(false);
                    xmlRequiredNode = await xmlSelectedMount.SelectSingleNodeAndCacheExpressionAsync("required/weaponmountdetails", token: token).ConfigureAwait(false);
                }
            }

            XPathNavigator xmlVehicleNode = await _objVehicle.GetNodeXPathAsync(token: token).ConfigureAwait(false);
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstVisibility))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFlexibility))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstControl))
            {
                // Populate the Weapon Mount Category list.
                string strFilter = "category != \"Size\" and " + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false);
                if (!_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                    strFilter += " and not(optionaldrone)";
                XPathNodeIterator xmlWeaponMountOptionNodeList
                    = _xmlDocXPath.Select("/chummer/weaponmounts/weaponmount[" + strFilter + ']');
                if (xmlWeaponMountOptionNodeList.Count > 0)
                {
                    foreach (XPathNavigator xmlWeaponMountOptionNode in xmlWeaponMountOptionNodeList)
                    {
                        string strId = (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("id", token).ConfigureAwait(false))?.Value;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XPathNavigator xmlTestNode = await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("forbidden/vehicledetails", token: token).ConfigureAwait(false);
                        if (xmlTestNode != null && await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("required/vehicledetails", token: token).ConfigureAwait(false);
                        if (xmlTestNode != null && !await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        string strName = (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value
                                         ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        bool blnAddItem = true;
                        switch ((await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("category", token: token).ConfigureAwait(false))?.Value)
                        {
                            case "Visibility":
                            {
                                XPathNodeIterator xmlNodeList = xmlForbiddenNode != null
                                    ? await xmlForbiddenNode.SelectAndCacheExpressionAsync("visibility", token).ConfigureAwait(false)
                                    : null;
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
                                    xmlNodeList = await xmlRequiredNode.SelectAndCacheExpressionAsync("visibility", token).ConfigureAwait(false);
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
                                            strId, (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value ?? strName));
                            }
                                break;

                            case "Flexibility":
                            {
                                if (xmlForbiddenNode != null)
                                {
                                    XPathNodeIterator xmlNodeList
                                        = await xmlForbiddenNode.SelectAndCacheExpressionAsync("flexibility", token: token).ConfigureAwait(false);
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
                                    XPathNodeIterator xmlNodeList = await xmlRequiredNode.SelectAndCacheExpressionAsync("flexibility", token: token).ConfigureAwait(false);
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
                                            strId, (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value ?? strName));
                            }
                                break;

                            case "Control":
                            {
                                if (xmlForbiddenNode != null)
                                {
                                    XPathNodeIterator xmlNodeList
                                        = await xmlForbiddenNode.SelectAndCacheExpressionAsync("control", token: token).ConfigureAwait(false);
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
                                    XPathNodeIterator xmlNodeList = await xmlRequiredNode.SelectAndCacheExpressionAsync("control", token: token).ConfigureAwait(false);
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
                                            strId, (await xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value ?? strName));
                            }
                                break;

                            default:
                                Utils.BreakIfDebug();
                                break;
                        }
                    }
                }

                Interlocked.Increment(ref _intLoading);
                try
                {
                    string strOldVisibility = await cboVisibility
                                                    .DoThreadSafeFuncAsync(
                                                        x => x.SelectedValue?.ToString(), token: token)
                                                    .ConfigureAwait(false);
                    string strOldFlexibility = await cboFlexibility
                                                     .DoThreadSafeFuncAsync(
                                                         x => x.SelectedValue?.ToString(), token: token)
                                                     .ConfigureAwait(false);
                    string strOldControl = await cboControl
                                                 .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                                 .ConfigureAwait(false);
                    await cboVisibility.PopulateWithListItemsAsync(lstVisibility, token: token).ConfigureAwait(false);
                    await cboVisibility.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = _blnAllowEditOptions && lstVisibility.Count > 1;
                        if (!string.IsNullOrEmpty(strOldVisibility))
                            x.SelectedValue = strOldVisibility;
                        if (x.SelectedIndex == -1 && lstVisibility.Count > 0)
                            x.SelectedIndex = 0;
                    }, token: token).ConfigureAwait(false);
                    await cboFlexibility.PopulateWithListItemsAsync(lstFlexibility, token: token).ConfigureAwait(false);
                    await cboFlexibility.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = _blnAllowEditOptions && lstFlexibility.Count > 1;
                        if (!string.IsNullOrEmpty(strOldFlexibility))
                            x.SelectedValue = strOldFlexibility;
                        if (x.SelectedIndex == -1 && lstFlexibility.Count > 0)
                            x.SelectedIndex = 0;
                    }, token: token).ConfigureAwait(false);
                    await cboControl.PopulateWithListItemsAsync(lstControl, token: token).ConfigureAwait(false);
                    await cboControl.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = _blnAllowEditOptions && lstControl.Count > 1;
                        if (!string.IsNullOrEmpty(strOldControl))
                            x.SelectedValue = strOldControl;
                        if (x.SelectedIndex == -1 && lstControl.Count > 0)
                            x.SelectedIndex = 0;
                    }, token: token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
            }
        }

        private async void lblSource_Click(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }
    }
}
