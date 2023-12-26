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

        private CancellationTokenSource _objUpdateInfoCancellationTokenSource;
        private CancellationTokenSource _objRefreshComboBoxesCancellationTokenSource;
        private CancellationTokenSource _objAddModCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        public WeaponMount WeaponMount => _objMount;

        public CreateWeaponMount(Vehicle objVehicle, Character objCharacter, WeaponMount objWeaponMount = null)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateInfoCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshComboBoxesCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objAddModCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericCancellationTokenSource.Dispose();
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
            };
            _objVehicle = objVehicle;
            _objMount = objWeaponMount;
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
            try
            {
                XPathNavigator xmlVehicleNode = await _objVehicle.GetNodeXPathAsync(_objGenericToken).ConfigureAwait(false);
                // Populate the Weapon Mount Category list.
                string strSizeFilter = "category = \'Size\' and " + await _objCharacter.Settings.BookXPathAsync(token: _objGenericToken).ConfigureAwait(false);
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
                            string strId = xmlSizeNode.SelectSingleNodeAndCacheExpression("id", _objGenericToken)?.Value;
                            if (string.IsNullOrEmpty(strId))
                                continue;

                            XPathNavigator xmlTestNode = xmlSizeNode.SelectSingleNodeAndCacheExpression("forbidden/vehicledetails", _objGenericToken);
                            if (xmlTestNode != null && await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, _objGenericToken).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlSizeNode.SelectSingleNodeAndCacheExpression("required/vehicledetails", _objGenericToken);
                            if (xmlTestNode != null && !await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, _objGenericToken).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            lstSize.Add(new ListItem(
                                            strId,
                                            xmlSizeNode.SelectSingleNodeAndCacheExpression("translate", _objGenericToken)?.Value
                                            ?? xmlSizeNode.SelectSingleNodeAndCacheExpression("name", _objGenericToken)?.Value
                                            ?? await LanguageManager.GetStringAsync("String_Unknown", token: _objGenericToken).ConfigureAwait(false)));
                        }
                    }

                    await cboSize.PopulateWithListItemsAsync(lstSize, _objGenericToken).ConfigureAwait(false);
                    await cboSize.DoThreadSafeAsync(x => x.Enabled = _blnAllowEditOptions && lstSize.Count > 1, _objGenericToken).ConfigureAwait(false);
                }

                if (_objMount != null)
                {
                    TreeNode objModsParentNode = new TreeNode
                    {
                        Tag = "Node_AdditionalMods",
                        Text = await LanguageManager.GetStringAsync("Node_AdditionalMods", token: _objGenericToken).ConfigureAwait(false)
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
                    }, _objGenericToken).ConfigureAwait(false);
                    _lstMods.AddRange(_objMount.Mods);

                    await cboSize.DoThreadSafeAsync(x => x.SelectedValue = _objMount.SourceIDString, _objGenericToken).ConfigureAwait(false);
                }
                if (await cboSize.DoThreadSafeFuncAsync(x => x.SelectedIndex, _objGenericToken).ConfigureAwait(false) == -1)
                {
                    await cboSize.DoThreadSafeAsync(x =>
                    {
                        if (x.Items.Count > 0)
                            x.SelectedIndex = 0;
                    }, _objGenericToken).ConfigureAwait(false);
                }
                else
                    await RefreshComboBoxes(_objGenericToken).ConfigureAwait(false);

                await nudMarkup.DoThreadSafeAsync(x =>
                {
                    x.Visible = AllowDiscounts || _objMount?.Markup != 0;
                    x.Enabled = AllowDiscounts;
                    lblMarkupLabel.DoThreadSafe(y => y.Visible = x.Visible);
                    lblMarkupPercentLabel.DoThreadSafe(y => y.Visible = x.Visible);
                }, _objGenericToken).ConfigureAwait(false);

                if (_objMount != null)
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstVisibility))
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFlexibility))
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstControl))
                    {
                        lstVisibility.AddRange(await cboVisibility.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>(), _objGenericToken).ConfigureAwait(false));
                        lstFlexibility.AddRange(await cboFlexibility.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>(), _objGenericToken).ConfigureAwait(false));
                        lstControl.AddRange(await cboControl.DoThreadSafeFuncAsync(x => x.Items.Cast<ListItem>(), _objGenericToken).ConfigureAwait(false));
                        foreach (string strLoopId in _objMount.WeaponMountOptions.Select(x => x.SourceIDString))
                        {
                            if (lstVisibility.Any(x => x.Value.ToString() == strLoopId))
                                await cboVisibility.DoThreadSafeAsync(x => x.SelectedValue = strLoopId, _objGenericToken).ConfigureAwait(false);
                            else if (lstFlexibility.Any(x => x.Value.ToString() == strLoopId))
                                await cboFlexibility.DoThreadSafeAsync(x => x.SelectedValue = strLoopId, _objGenericToken).ConfigureAwait(false);
                            else if (lstControl.Any(x => x.Value.ToString() == strLoopId))
                                await cboControl.DoThreadSafeAsync(x => x.SelectedValue = strLoopId, _objGenericToken).ConfigureAwait(false);
                        }
                    }
                }

                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount, _objGenericToken).ConfigureAwait(false);

                Interlocked.Decrement(ref _intLoading);
                await UpdateInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void CreateWeaponMount_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateInfoCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshComboBoxesCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objAddModCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                //TODO: THIS IS UGLY AS SHIT, FIX BETTER

                string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedMount))
                    return;
                string strSelectedControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedControl))
                    return;
                string strSelectedFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedFlexibility))
                    return;
                string strSelectedVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), _objGenericToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedVisibility))
                    return;

                XmlNode xmlSelectedMount = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedMount);
                if (xmlSelectedMount == null)
                    return;
                XmlNode xmlSelectedControl = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedControl);
                if (xmlSelectedControl == null)
                    return;
                XmlNode xmlSelectedFlexibility = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedFlexibility);
                if (xmlSelectedFlexibility == null)
                    return;
                XmlNode xmlSelectedVisibility = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedVisibility);
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
                        _objMount.Create(xmlSelectedMount, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false));
                    }
                    else if (_objMount.SourceIDString != strSelectedMount)
                    {
                        _objMount.Create(xmlSelectedMount, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false));
                    }

                    _objMount.DiscountCost = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);

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
                    = await _objMount.Mods.ToListAsync(x => !x.IncludedInVehicle && !_lstMods.Contains(x), _objGenericToken).ConfigureAwait(false);
                await _objMount.Mods.RemoveAllAsync(x => lstOldRemovedVehicleMods.Contains(x), _objGenericToken).ConfigureAwait(false);
                List<VehicleMod> lstNewVehicleMods = new List<VehicleMod>(_lstMods.Count);
                foreach (VehicleMod objMod in _lstMods)
                {
                    if (await _objMount.Mods.ContainsAsync(objMod, _objGenericToken).ConfigureAwait(false))
                        continue;
                    lstNewVehicleMods.Add(objMod);
                    await _objMount.Mods.AddAsync(objMod, _objGenericToken).ConfigureAwait(false);
                }

                if (_objCharacter.Created)
                {
                    bool blnRemoveMountAfterCheck = false;
                    // Check the item's Cost and make sure the character can afford it.
                    if (!await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                    {
                        // New mount, so temporarily add it to parent vehicle to make sure we can capture everything
                        if (_objMount.Parent == null)
                        {
                            blnRemoveMountAfterCheck = true;
                            await _objVehicle.WeaponMounts.AddAsync(_objMount, _objGenericToken).ConfigureAwait(false);
                        }

                        decimal decCost = await _objVehicle.GetTotalCostAsync(_objGenericToken).ConfigureAwait(false) - _decOldBaseCost;

                        // Multiply the cost if applicable.
                        switch ((await _objMount.TotalAvailTupleAsync(token: _objGenericToken).ConfigureAwait(false)).Suffix)
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
                            Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen", token: _objGenericToken).ConfigureAwait(false),
                                                             await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen", token: _objGenericToken).ConfigureAwait(false),
                                                             MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (blnRemoveMountAfterCheck)
                            {
                                await _objVehicle.WeaponMounts.RemoveAsync(_objMount, _objGenericToken).ConfigureAwait(false);
                                _objMount = null;
                            }
                            else
                            {
                                await _objMount.Mods.RemoveAllAsync(x => lstNewVehicleMods.Contains(x), _objGenericToken).ConfigureAwait(false);
                                await _objMount.Mods.AddRangeAsync(lstOldRemovedVehicleMods, _objGenericToken).ConfigureAwait(false);
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
                            await _objVehicle.WeaponMounts.AddAsync(_objMount, _objGenericToken).ConfigureAwait(false);
                        }
                        bool blnOverCapacity;
                        if (await _objCharacter.Settings.BookEnabledAsync("R5", _objGenericToken).ConfigureAwait(false))
                        {
                            if (_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                                blnOverCapacity
                                    = await _objVehicle.GetDroneModSlotsUsedAsync(_objGenericToken)
                                                       .ConfigureAwait(false) > await _objVehicle
                                        .GetDroneModSlotsAsync(_objGenericToken).ConfigureAwait(false);
                            else
                                blnOverCapacity = await _objVehicle.OverR5CapacityAsync("Weapons", _objGenericToken)
                                                                   .ConfigureAwait(false);
                        }
                        else
                            blnOverCapacity = await _objVehicle.GetSlotsAsync(_objGenericToken).ConfigureAwait(false)
                                              < await _objVehicle.GetSlotsUsedAsync(_objGenericToken)
                                                                 .ConfigureAwait(false);

                        if (blnOverCapacity)
                        {
                            Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached", token: _objGenericToken).ConfigureAwait(false),
                                                             await LanguageManager.GetStringAsync("MessageTitle_CapacityReached", token: _objGenericToken).ConfigureAwait(false),
                                                             MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (blnRemoveMountAfterCheck)
                            {
                                await _objVehicle.WeaponMounts.RemoveAsync(_objMount, _objGenericToken).ConfigureAwait(false);
                                _objMount = null;
                            }
                            else
                            {
                                await _objMount.Mods.RemoveAllAsync(x => lstNewVehicleMods.Contains(x), _objGenericToken).ConfigureAwait(false);
                                await _objMount.Mods.AddRangeAsync(lstOldRemovedVehicleMods, _objGenericToken).ConfigureAwait(false);
                            }
                            return;
                        }
                    }

                    if (blnRemoveMountAfterCheck)
                    {
                        await _objVehicle.WeaponMounts.RemoveAsync(_objMount, _objGenericToken).ConfigureAwait(false);
                    }
                }

                _objMount.FreeCost = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false);

                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await RefreshComboBoxes(_objGenericToken).ConfigureAwait(false);
                await treMods.DoThreadSafeAsync(x => x.SelectedNode = null, _objGenericToken).ConfigureAwait(false);
                await UpdateInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await treMods.DoThreadSafeAsync(x => x.SelectedNode = null, _objGenericToken).ConfigureAwait(false);
                await UpdateInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void DoUpdateInfo(object sender, EventArgs e)
        {
            try
            {
                await UpdateInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        public bool AllowDiscounts { get; set; }

        private async Task UpdateInfo(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateInfoCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                XPathNavigator xmlSelectedMount = null;
                string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedMount))
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                else
                {
                    xmlSelectedMount = _xmlDocXPath.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedMount);
                    if (xmlSelectedMount == null)
                        await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    else
                    {
                        string strSelectedControl = await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strSelectedControl))
                            await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                        else if (_xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedControl) == null)
                            await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                        else
                        {
                            string strSelectedFlexibility = await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                            if (string.IsNullOrEmpty(strSelectedFlexibility))
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                            else if (_xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedFlexibility) == null)
                                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                            else
                            {
                                string strSelectedVisibility = await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                                if (string.IsNullOrEmpty(strSelectedVisibility))
                                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                                else if (_xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedVisibility) == null)
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
                        string strModAvail = await objMod.GetDisplayTotalAvailAsync(token).ConfigureAwait(false);
                        await lblAvailability.DoThreadSafeAsync(x => x.Text = strModAvail, token).ConfigureAwait(false);

                        if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                        {
                            string strCostInner = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                            await lblCost.DoThreadSafeAsync(x => x.Text = strCostInner, token).ConfigureAwait(false);
                        }
                        else
                        {
                            int intTotalSlots = 0;
                            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intTotalSlots);
                            foreach (string strSelectedId in astrSelectedValues)
                            {
                                if (!string.IsNullOrEmpty(strSelectedId))
                                {
                                    XmlNode xmlLoopNode = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedId);
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
                            string strCostInner = (await objMod.TotalCostInMountCreation(intTotalSlots, token).ConfigureAwait(false) * (1 + decMarkup / 100.0m))
                                    .ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                    + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                            await lblCost.DoThreadSafeAsync(x => x.Text = strCostInner, token).ConfigureAwait(false);
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
                        = _setBlackMarketMaps.Contains(
                            xmlSelectedMount.SelectSingleNodeAndCacheExpression("category", token: token)?.Value ?? string.Empty);
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

                string strAvail = xmlSelectedMount.SelectSingleNodeAndCacheExpression("avail", token)?.Value ?? string.Empty;
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
                    XmlNode xmlLoopNode = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedId);
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
                    AvailabilityValue objLoopAvail = await objMod.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
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
                        decCost += await objMod.TotalCostInMountCreation(intSlots, token).ConfigureAwait(false);
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

                string strCost = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token).ConfigureAwait(false);
                await lblSlots.DoThreadSafeAsync(x => x.Text = intSlots.ToString(GlobalSettings.CultureInfo), token).ConfigureAwait(false);
                await lblAvailability.DoThreadSafeAsync(x => x.Text = strAvailText, token).ConfigureAwait(false);
                string strSource
                    = xmlSelectedMount.SelectSingleNodeAndCacheExpression("source", token)?.Value ?? await LanguageManager
                        .GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                string strPage
                    = xmlSelectedMount.SelectSingleNodeAndCacheExpression("altpage", token)?.Value
                      ?? xmlSelectedMount.SelectSingleNodeAndCacheExpression("page", token)?.Value ?? await LanguageManager
                          .GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
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
        }

        private async void cmdAddMod_Click(object sender, EventArgs e)
        {
            try
            {
                CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
                CancellationToken objNewToken = objNewCancellationTokenSource.Token;
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objAddModCancellationTokenSource, objNewCancellationTokenSource);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_objGenericToken, objNewToken))
                {
                    CancellationToken token = objJoinedCancellationTokenSource.Token;
                    bool blnAddAgain;

                    XPathNavigator xmlSelectedMount = null;
                    string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedMount))
                        xmlSelectedMount = _xmlDocXPath.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedMount);

                    int intSlots = 0;
                    xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

                    string[] astrSelectedValues =
                    {
                    await cboVisibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false),
                    await cboFlexibility.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false),
                    await cboControl.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                };

                    foreach (string strSelectedId in astrSelectedValues)
                    {
                        if (!string.IsNullOrEmpty(strSelectedId))
                        {
                            XmlNode xmlLoopNode = _xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedId);
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

                    TreeNode objModsParentNode = await treMods.DoThreadSafeFuncAsync(x => x.FindNode("Node_AdditionalMods"), token).ConfigureAwait(false);
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
                                                                                   }, token).ConfigureAwait(false))
                        {
                            // Make sure the dialogue window was not canceled.
                            if (await frmPickVehicleMod.ShowDialogSafeAsync(this, token).ConfigureAwait(false) == DialogResult.Cancel)
                                break;

                            blnAddAgain = frmPickVehicleMod.MyForm.AddAgain;
                            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("vehicles.xml", token: token).ConfigureAwait(false);
                            XmlNode objXmlMod = objXmlDocument.TryGetNodeByNameOrId("/chummer/weaponmountmods/mod", frmPickVehicleMod.MyForm.SelectedMod);

                            VehicleMod objMod = new VehicleMod(_objCharacter)
                            {
                                DiscountCost = frmPickVehicleMod.MyForm.BlackMarketDiscount
                            };
                            objMod.Create(objXmlMod, frmPickVehicleMod.MyForm.SelectedRating, _objVehicle, frmPickVehicleMod.MyForm.Markup);
                            if (frmPickVehicleMod.MyForm.FreeCost)
                                objMod.Cost = "0";
                            if (_objMount != null)
                                await _objMount.Mods.AddAsync(objMod, token).ConfigureAwait(false);
                            _lstMods.Add(objMod);
                            intSlots += objMod.CalculatedSlots;

                            TreeNode objNewNode = objMod.CreateTreeNode(null, null, null, null, null, null);

                            if (objModsParentNode == null)
                            {
                                objModsParentNode = new TreeNode
                                {
                                    Tag = "Node_AdditionalMods",
                                    Text = await LanguageManager.GetStringAsync("Node_AdditionalMods", token: token).ConfigureAwait(false)
                                };
                                TreeNode objLoopNode = objModsParentNode;
                                await treMods.DoThreadSafeAsync(x =>
                                {
                                    x.Nodes.Add(objLoopNode);
                                    objLoopNode.Expand();
                                }, token).ConfigureAwait(false);
                            }

                            TreeNode objLoopNode2 = objModsParentNode;
                            await treMods.DoThreadSafeAsync(x =>
                            {
                                objLoopNode2.Nodes.Add(objNewNode);
                                x.SelectedNode = objNewNode;
                            }, token).ConfigureAwait(false);
                        }
                    }
                    while (blnAddAgain);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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

        private async Task RefreshComboBoxes(CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objRefreshComboBoxesCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                XPathNavigator xmlRequiredNode = null;
                XPathNavigator xmlForbiddenNode = null;
                string strSelectedMount = await cboSize.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedMount))
                {
                    XPathNavigator xmlSelectedMount = _xmlDocXPath.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSelectedMount);
                    if (xmlSelectedMount != null)
                    {
                        xmlForbiddenNode = xmlSelectedMount.SelectSingleNodeAndCacheExpression("forbidden/weaponmountdetails", token: token);
                        xmlRequiredNode = xmlSelectedMount.SelectSingleNodeAndCacheExpression("required/weaponmountdetails", token: token);
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
                            string strId = xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("id", token)?.Value;
                            if (string.IsNullOrEmpty(strId))
                                continue;

                            XPathNavigator xmlTestNode = xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("forbidden/vehicledetails", token: token);
                            if (xmlTestNode != null && await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("required/vehicledetails", token: token);
                            if (xmlTestNode != null && !await xmlVehicleNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            string strName = xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                             ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            bool blnAddItem = true;
                            switch (xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("category", token: token)?.Value)
                            {
                                case "Visibility":
                                    {
                                        XPathNodeIterator xmlNodeList = xmlForbiddenNode?.SelectAndCacheExpression("visibility", token);
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
                                            xmlNodeList = xmlRequiredNode.SelectAndCacheExpression("visibility", token);
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
                                                    strId, xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? strName));
                                    }
                                    break;

                                case "Flexibility":
                                    {
                                        if (xmlForbiddenNode != null)
                                        {
                                            XPathNodeIterator xmlNodeList
                                                = xmlForbiddenNode.SelectAndCacheExpression("flexibility", token: token);
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
                                            XPathNodeIterator xmlNodeList = xmlRequiredNode.SelectAndCacheExpression("flexibility", token: token);
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
                                                    strId, xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? strName));
                                    }
                                    break;

                                case "Control":
                                    {
                                        if (xmlForbiddenNode != null)
                                        {
                                            XPathNodeIterator xmlNodeList
                                                = xmlForbiddenNode.SelectAndCacheExpression("control", token: token);
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
                                            XPathNodeIterator xmlNodeList = xmlRequiredNode.SelectAndCacheExpression("control", token: token);
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
                                                    strId, xmlWeaponMountOptionNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? strName));
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
        }

        private async void lblSource_Click(object sender, EventArgs e)
        {
            try
            {
                await CommonFunctions.OpenPdfFromControl(sender, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }
    }
}
