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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectWeaponAccessoryTarget : Form
    {
        private Weapon _objSelectedWeapon;
        private WeaponAccessory _objSelectedAccessory;
        private readonly WeaponAccessory _objSourceAccessory;
        private readonly Character _objCharacter;

        public SelectWeaponAccessoryTarget(Character objCharacter, WeaponAccessory objSourceAccessory)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objSourceAccessory = objSourceAccessory ?? throw new ArgumentNullException(nameof(objSourceAccessory));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
        }

        private async void SelectWeaponAccessoryTarget_Load(object sender, EventArgs e)
        {
            await PopulateTargets().ConfigureAwait(false);
        }

        private async Task PopulateTargets(CancellationToken token = default)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTargets))
            {
                string strSourceMount = _objSourceAccessory.Mount;
                if (string.IsNullOrEmpty(strSourceMount))
                    return;

                // Find all character weapons and accessories that have compatible mounts
                await PopulateWeaponTargets(_objCharacter.Weapons, "Character", lstTargets, strSourceMount, token).ConfigureAwait(false);

                // Find all vehicle weapons and accessories that have compatible mounts
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    // Check direct vehicle weapons
                    await PopulateWeaponTargets(objVehicle.Weapons, $"Vehicle: {await objVehicle.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)}", 
                        lstTargets, strSourceMount, token).ConfigureAwait(false);
                    
                    // Check weapons in weapon mounts
                    foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                    {
                        await PopulateWeaponTargets(objWeaponMount.Weapons, $"Vehicle: {await objVehicle.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)} (Mount)", 
                            lstTargets, strSourceMount, token).ConfigureAwait(false);
                    }
                }

                lstTargets.Sort(CompareListItems.CompareNames);
                string strOldSelected = await this.lstTargets.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                await this.lstTargets.PopulateWithListItemsAsync(lstTargets, token: token).ConfigureAwait(false);
                await this.lstTargets.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        private async Task PopulateWeaponTargets(IEnumerable<Weapon> lstWeapons, string strCategory, List<ListItem> lstTargets, 
            string strSourceMount, CancellationToken token = default)
        {
            foreach (Weapon objWeapon in lstWeapons)
            {
                // For vehicle weapons, check if they allow accessories instead of just equipped status
                bool blnShouldInclude = await objWeapon.GetEquippedAsync(token).ConfigureAwait(false);
                if (!blnShouldInclude && objWeapon.ParentVehicle != null)
                {
                    // Vehicle weapons are included if they allow accessories
                    blnShouldInclude = objWeapon.AllowAccessory;
                }
                
                if (blnShouldInclude)
                {
                    // Check if weapon has compatible mounts
                    List<string> lstWeaponMounts = await objWeapon.GetAccessoryMountsAsync(token: token).ConfigureAwait(false);
                    
                    
                    if (lstWeaponMounts.Any(x => strSourceMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).Contains(x)))
                    {
                        // Get the mount information for display
                        string strMountInfo = string.Join("/", lstWeaponMounts.Where(x => strSourceMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).Contains(x)));
                        lstTargets.Add(new ListItem(objWeapon.InternalId, 
                            $"{strCategory} - {await objWeapon.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)} - [{strMountInfo}]"));
                    }

                    // Check accessories on this weapon
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        if (objAccessory.Equipped && !string.IsNullOrEmpty(objAccessory.AddMount))
                        {
                            string strAccessoryMount = objAccessory.Mount;
                            if (!string.IsNullOrEmpty(strAccessoryMount))
                            {
                                bool blnCompatible = false;
                                
                                // Check if AddMount contains specific mount types that match the source
                                foreach (string strAddMount in objAccessory.AddMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (strAddMount.Equals("Passthrough", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Passthrough: accessory provides the same mount type as its own mount
                                        // The source accessory must be compatible with the slide mount's mount type
                                        if (strAccessoryMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries)
                                            .Any(x => strSourceMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).Contains(x)))
                                        {
                                            blnCompatible = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // Specific mount type: check if it matches the source accessory's mount
                                        if (strSourceMount.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).Contains(strAddMount))
                                        {
                                            blnCompatible = true;
                                            break;
                                        }
                                    }
                                }
                                
                                if (blnCompatible)
                                {
                                    // Format: Character - Weapon Name - MountType[AccessoryName]
                                    string strWeaponName = await objWeapon.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    string strAccessoryName = await objAccessory.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    lstTargets.Add(new ListItem(objAccessory.InternalId, 
                                        $"{strCategory} - {strWeaponName} - {strAccessoryMount}[{strAccessoryName}]"));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            string strSelectedId = lstTargets.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Find the selected weapon or accessory in character weapons
                _objSelectedWeapon = _objCharacter.Weapons.FirstOrDefault(x => x.InternalId.ToString() == strSelectedId);
                if (_objSelectedWeapon == null)
                {
                    // Look for accessory in character weapons
                    foreach (Weapon objWeapon in _objCharacter.Weapons)
                    {
                        _objSelectedAccessory = objWeapon.WeaponAccessories.FirstOrDefault(x => x.InternalId.ToString() == strSelectedId);
                        if (_objSelectedAccessory != null)
                            break;
                    }
                }

                // If not found in character weapons, look in vehicle weapons
                if (_objSelectedWeapon == null && _objSelectedAccessory == null)
                {
                    foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                    {
                        // Check direct vehicle weapons
                        _objSelectedWeapon = objVehicle.Weapons.FirstOrDefault(x => x.InternalId.ToString() == strSelectedId);
                        if (_objSelectedWeapon == null)
                        {
                            // Look for accessory in direct vehicle weapons
                            foreach (Weapon objWeapon in objVehicle.Weapons)
                            {
                                _objSelectedAccessory = objWeapon.WeaponAccessories.FirstOrDefault(x => x.InternalId.ToString() == strSelectedId);
                                if (_objSelectedAccessory != null)
                                    break;
                            }
                        }
                        
                        // If still not found, check weapons in weapon mounts
                        if (_objSelectedWeapon == null && _objSelectedAccessory == null)
                        {
                            foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                            {
                                _objSelectedWeapon = objWeaponMount.Weapons.FirstOrDefault(x => x.InternalId.ToString() == strSelectedId);
                                if (_objSelectedWeapon == null)
                                {
                                    // Look for accessory in weapon mount weapons
                                    foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                    {
                                        _objSelectedAccessory = objWeapon.WeaponAccessories.FirstOrDefault(x => x.InternalId.ToString() == strSelectedId);
                                        if (_objSelectedAccessory != null)
                                            break;
                                    }
                                }
                                if (_objSelectedWeapon != null || _objSelectedAccessory != null)
                                    break;
                            }
                        }
                        
                        if (_objSelectedWeapon != null || _objSelectedAccessory != null)
                            break;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public Weapon SelectedWeapon => _objSelectedWeapon;
        public WeaponAccessory SelectedAccessory => _objSelectedAccessory;
    }
}
