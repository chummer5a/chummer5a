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
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectItem : Form
    {
        private readonly List<Gear> _lstGear = new List<Gear>();
        private readonly List<Vehicle> _lstVehicles = new List<Vehicle>();
        private List<ListItem> _lstGeneralItems;
        private string _strMode = "General";
        private Character _objCharacter;
        private string _strSelectedItem = string.Empty;
        private string _strSelectedName = string.Empty;
        private bool _blnAllowAutoSelect = true;
        private string _strForceItem = string.Empty;

        #region Control Events

        public SelectItem()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _lstGeneralItems = Utils.ListItemListPool.Get();
            Disposed += (sender, args) => Utils.ListItemListPool.Return(ref _lstGeneralItems);
        }

        private async void SelectItem_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstItems))
            {
                switch (_strMode)
                {
                    case "Gear":
                    {
                        string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
                        await cboAmmo.DoThreadSafeAsync(x => x.DropDownStyle = ComboBoxStyle.DropDownList).ConfigureAwait(false);
                        // Add each of the items to a new List since we need to also grab their plugin information.
                        foreach (Gear objGear in _lstGear)
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(
                                       Utils.StringBuilderPool, out StringBuilder sbdAmmoName))
                            {
                                sbdAmmoName.Append(await objGear.GetCurrentDisplayNameAsync().ConfigureAwait(false));
                                // Retrieve the plugin information if it has any.
                                if (await objGear.Children.GetCountAsync().ConfigureAwait(false) > 0)
                                {
                                    // Append the plugin information to the name.
                                    (await sbdAmmoName.Append(strSpace).Append('[')
                                                      .AppendJoinAsync(',' + strSpace,
                                                                       objGear.Children.Select(
                                                                           x => x.GetCurrentDisplayNameShortAsync())).ConfigureAwait(false))
                                        .Append(']');
                                }

                                int intRating = await objGear.GetRatingAsync().ConfigureAwait(false);
                                if (intRating > 0)
                                {
                                    sbdAmmoName.Append(strSpace).Append('(')
                                               .AppendFormat(GlobalSettings.CultureInfo,
                                                             await LanguageManager.GetStringAsync("Label_RatingFormat")
                                                                 .ConfigureAwait(false),
                                                             await LanguageManager.GetStringAsync(objGear.RatingLabel)
                                                                 .ConfigureAwait(false)).Append(strSpace)
                                               .Append(intRating.ToString(GlobalSettings.CultureInfo)).Append(')');
                                }

                                sbdAmmoName.Append(strSpace).Append('×')
                                           .Append(objGear.Quantity.ToString(GlobalSettings.InvariantCultureInfo));
                                lstItems.Add(new ListItem(objGear.InternalId, sbdAmmoName.ToString()));
                            }
                        }

                        break;
                    }
                    case "Vehicles":
                    {
                        await cboAmmo.DoThreadSafeAsync(x => x.DropDownStyle = ComboBoxStyle.DropDownList).ConfigureAwait(false);
                        // Add each of the items to a new List.
                        foreach (Vehicle objVehicle in _lstVehicles)
                        {
                            lstItems.Add(new ListItem(objVehicle.InternalId, await objVehicle.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                        }

                        break;
                    }
                    case "General":
                        await cboAmmo.DoThreadSafeAsync(x => x.DropDownStyle = ComboBoxStyle.DropDownList).ConfigureAwait(false);
                        lstItems.AddRange(_lstGeneralItems);
                        break;

                    case "Dropdown":
                        await cboAmmo.DoThreadSafeAsync(x =>
                        {
                            x.DropDownStyle = ComboBoxStyle.DropDown;
                            x.AutoCompleteMode = AutoCompleteMode.Suggest;
                        }).ConfigureAwait(false);
                        lstItems.AddRange(_lstGeneralItems);
                        break;

                    case "Restricted":
                    {
                        await cboAmmo.DoThreadSafeAsync(x =>
                        {
                            x.DropDownStyle = ComboBoxStyle.DropDown;
                            x.AutoCompleteMode = AutoCompleteMode.Suggest;
                        }).ConfigureAwait(false);
                        if (!(await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).LicenseRestricted)
                        {
                            foreach (XPathNavigator objNode in (await _objCharacter.LoadDataXPathAsync("licenses.xml").ConfigureAwait(false))
                                                                     .SelectAndCacheExpression(
                                                                         "/chummer/licenses/license"))
                            {
                                string strInnerText = objNode.Value;
                                if (!string.IsNullOrEmpty(strInnerText))
                                {
                                    lstItems.Add(new ListItem(strInnerText,
                                        objNode
                                            .SelectSingleNodeAndCacheExpression(
                                                "@translate")
                                            ?.Value ?? strInnerText));
                                }
                            }
                        }
                        else
                        {
                            // Cyberware/Bioware.
                            foreach (Cyberware objCyberware in await (await _objCharacter.GetCyberwareAsync().ConfigureAwait(false)).GetAllDescendantsAsync(
                                         x => x.GetChildrenAsync()).ConfigureAwait(false))
                            {
                                if ((await objCyberware.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                {
                                    lstItems.Add(
                                        new ListItem(objCyberware.InternalId, await objCyberware.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                                }

                                foreach (Gear objGear in await objCyberware.GearChildren.GetAllDescendantsAsync(
                                             x => x.Children).ConfigureAwait(false))
                                {
                                    if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                            lstItems.Add(new ListItem(objGear.InternalId, await objGear.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                                }
                            }

                            // Armor.
                            await (await _objCharacter.GetArmorAsync().ConfigureAwait(false)).ForEachAsync(async objArmor =>
                            {
                                if ((await objArmor.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                {
                                    lstItems.Add(new ListItem(objArmor.InternalId,
                                                              await objArmor.GetCurrentDisplayNameAsync()
                                                                  .ConfigureAwait(false)));
                                }

                                await objArmor.ArmorMods.ForEachAsync(async objMod =>
                                {
                                    if ((await objMod.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                    {
                                        lstItems.Add(new ListItem(objMod.InternalId,
                                                                  await objMod.GetCurrentDisplayNameAsync()
                                                                      .ConfigureAwait(false)));
                                    }

                                    foreach (Gear objGear in await objMod.GearChildren.GetAllDescendantsAsync(
                                                 x => x.Children).ConfigureAwait(false))
                                    {
                                        if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                            lstItems.Add(new ListItem(objGear.InternalId,
                                                                  await objGear.GetCurrentDisplayNameAsync()
                                                                      .ConfigureAwait(false)));
                                    }
                                }).ConfigureAwait(false);

                                foreach (Gear objGear in await objArmor.GearChildren.GetAllDescendantsAsync(
                                             x => x.Children).ConfigureAwait(false))
                                {
                                    if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                        lstItems.Add(new ListItem(objGear.InternalId,
                                                              await objGear.GetCurrentDisplayNameAsync()
                                                                           .ConfigureAwait(false)));
                                }
                            }).ConfigureAwait(false);

                            // Weapons.
                            foreach (Weapon objWeapon in await (await _objCharacter.GetWeaponsAsync().ConfigureAwait(false)).GetAllDescendantsAsync(x => x.Children).ConfigureAwait(false))
                            {
                                if ((await objWeapon.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                {
                                    lstItems.Add(new ListItem(objWeapon.InternalId, await objWeapon.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                                }

                                await objWeapon.WeaponAccessories.ForEachAsync(async objAccessory =>
                                {
                                    if (!objAccessory.IncludedInWeapon
                                        && (await objAccessory.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                    {
                                        lstItems.Add(
                                            new ListItem(objAccessory.InternalId,
                                                         await objAccessory.GetCurrentDisplayNameAsync()
                                                                           .ConfigureAwait(false)));
                                    }

                                    foreach (Gear objGear in await objAccessory.GearChildren.GetAllDescendantsAsync(
                                                 x => x.Children).ConfigureAwait(false))
                                    {
                                        if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                            lstItems.Add(new ListItem(objGear.InternalId,
                                                                  await objGear.GetCurrentDisplayNameAsync()
                                                                               .ConfigureAwait(false)));
                                    }
                                }).ConfigureAwait(false);
                            }

                            // Gear.
                            foreach (Gear objGear in await (await _objCharacter.GetGearAsync().ConfigureAwait(false)).GetAllDescendantsAsync(x => x.Children).ConfigureAwait(false))
                            {
                                if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                    lstItems.Add(new ListItem(objGear.InternalId, await objGear.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                            }

                            // Vehicles.
                            await (await _objCharacter.GetVehiclesAsync().ConfigureAwait(false)).ForEachAsync(async objVehicle =>
                            {
                                if ((await objVehicle.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                {
                                    lstItems.Add(new ListItem(objVehicle.InternalId,
                                                              await objVehicle.GetCurrentDisplayNameAsync()
                                                                              .ConfigureAwait(false)));
                                }

                                await objVehicle.Mods.ForEachAsync(async objMod =>
                                {
                                    if (!objMod.IncludedInVehicle && (await objMod.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                    {
                                        lstItems.Add(new ListItem(objMod.InternalId,
                                                                  await objMod.GetCurrentDisplayNameAsync()
                                                                              .ConfigureAwait(false)));
                                    }

                                    foreach (Weapon objWeapon in await objMod.Weapons.GetAllDescendantsAsync(x => x.Children).ConfigureAwait(false))
                                    {
                                        if ((await objWeapon.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                        {
                                            lstItems.Add(
                                                new ListItem(objWeapon.InternalId,
                                                             await objWeapon.GetCurrentDisplayNameAsync()
                                                                            .ConfigureAwait(false)));
                                        }

                                        await objWeapon.WeaponAccessories.ForEachAsync(async objAccessory =>
                                        {
                                            if (!objAccessory.IncludedInWeapon
                                                && (await objAccessory.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                            {
                                                lstItems.Add(new ListItem(objAccessory.InternalId,
                                                                          await objAccessory
                                                                              .GetCurrentDisplayNameAsync()
                                                                              .ConfigureAwait(false)));
                                            }

                                            foreach (Gear objGear in await objAccessory.GearChildren.GetAllDescendantsAsync(
                                                         x => x.Children).ConfigureAwait(false))
                                            {
                                                if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                                    lstItems.Add(
                                                    new ListItem(objGear.InternalId,
                                                                 await objGear.GetCurrentDisplayNameAsync()
                                                                              .ConfigureAwait(false)));
                                            }
                                        }).ConfigureAwait(false);
                                    }
                                }).ConfigureAwait(false);

                                await objVehicle.WeaponMounts.ForEachAsync(async objWeaponMount =>
                                {
                                    if (!objWeaponMount.IncludedInVehicle
                                        && (await objWeaponMount.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                    {
                                        lstItems.Add(new ListItem(objWeaponMount.InternalId,
                                                                  await objWeaponMount.GetCurrentDisplayNameAsync()
                                                                      .ConfigureAwait(false)));
                                    }

                                    foreach (Weapon objWeapon in await objWeaponMount.Weapons.GetAllDescendantsAsync(
                                                 x => x.Children).ConfigureAwait(false))
                                    {
                                        if ((await objWeapon.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                        {
                                            lstItems.Add(
                                                new ListItem(objWeapon.InternalId,
                                                             await objWeapon.GetCurrentDisplayNameAsync()
                                                                            .ConfigureAwait(false)));
                                        }

                                        await objWeapon.WeaponAccessories.ForEachAsync(async objAccessory =>
                                        {
                                            if (!objAccessory.IncludedInWeapon
                                                && (await objAccessory.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                            {
                                                lstItems.Add(new ListItem(objAccessory.InternalId,
                                                                          await objAccessory
                                                                              .GetCurrentDisplayNameAsync()
                                                                              .ConfigureAwait(false)));
                                            }

                                            foreach (Gear objGear in await objAccessory.GearChildren.GetAllDescendantsAsync(
                                                         x => x.Children).ConfigureAwait(false))
                                            {
                                                if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                                    lstItems.Add(
                                                        new ListItem(objGear.InternalId,
                                                                     await objGear.GetCurrentDisplayNameAsync()
                                                                         .ConfigureAwait(false)));
                                            }
                                        }).ConfigureAwait(false);
                                    }
                                }).ConfigureAwait(false);

                                foreach (Gear objGear in await objVehicle.GearChildren.GetAllDescendantsAsync(
                                             x => x.Children).ConfigureAwait(false))
                                {
                                    if ((await objGear.TotalAvailTupleAsync(false).ConfigureAwait(false)).Suffix == 'R')
                                        lstItems.Add(new ListItem(objGear.InternalId,
                                                                  await objGear.GetCurrentDisplayNameAsync()
                                                                               .ConfigureAwait(false)));
                                }
                            }).ConfigureAwait(false);
                        }

                        break;
                    }
                }

                lstItems.Sort(CompareListItems.CompareNames);

                // Populate the lists.
                await cboAmmo.PopulateWithListItemsAsync(lstItems).ConfigureAwait(false);

                // If there's only 1 value in the list, the character doesn't have a choice, so just accept it.
                if (await cboAmmo.DoThreadSafeFuncAsync(x => x.Items.Count).ConfigureAwait(false) == 1 && _blnAllowAutoSelect)
                    AcceptForm();

                if (!string.IsNullOrEmpty(_strForceItem))
                {
                    await cboAmmo.DoThreadSafeAsync(x => x.SelectedIndex = x.FindStringExact(_strForceItem)).ConfigureAwait(false);
                    if (await cboAmmo.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false) != -1)
                        AcceptForm();
                }

                if (!string.IsNullOrEmpty(_strSelectedItem))
                {
                    await cboAmmo.DoThreadSafeAsync(x =>
                    {
                        if (x.DropDownStyle == ComboBoxStyle.DropDownList || x.DropDownStyle == ComboBoxStyle.DropDown)
                        {
                            string strOldSelected = x.SelectedValue?.ToString();
                            x.SelectedValue = _strSelectedItem;
                            if (x.SelectedIndex == -1 && !string.IsNullOrEmpty(strOldSelected))
                                x.SelectedValue = strOldSelected;
                        }
                        else
                            x.Text = _strSelectedItem;
                    }).ConfigureAwait(false);
                }
            }

            if (await cboAmmo.DoThreadSafeFuncAsync(x => x.Items.Count).ConfigureAwait(false) < 0)
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Internal ID of the item that was selected.
        /// </summary>
        public string SelectedItem
        {
            get => _strSelectedItem;
            set => _strSelectedItem = value;
        }

        /// <summary>
        /// Name of the item that was selected.
        /// </summary>
        public string SelectedName => _strSelectedName;

        /// <summary>
        /// Whether the Form should be accepted if there is only one item left in the list.
        /// </summary>
        public bool AllowAutoSelect
        {
            get => _blnAllowAutoSelect;
            set => _blnAllowAutoSelect = value;
        }

        /// <summary>
        /// Description to show in the window.
        /// </summary>
        public string Description
        {
            get => lblDescription.Text;
            set => lblDescription.Text = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// List of Gear that the user can select.
        /// </summary>
        public void SetGearMode(IEnumerable<Gear> lstGears)
        {
            _lstGear.Clear();
            _lstGear.AddRange(lstGears);
            _strMode = "Gear";
        }

        /// <summary>
        /// List of Vehicles that the user can select.
        /// </summary>
        public void SetVehiclesMode(IEnumerable<Vehicle> lstVehicles)
        {
            _lstVehicles.Clear();
            _lstVehicles.AddRange(lstVehicles);
            _strMode = "Vehicles";
        }

        /// <summary>
        /// List of general items that the user can select.
        /// </summary>
        public void SetGeneralItemsMode(IEnumerable<ListItem> lstItems)
        {
            _lstGeneralItems.Clear();
            _lstGeneralItems.AddRange(lstItems);
            _strMode = "General";
        }

        /// <summary>
        /// List of general items that the user can select.
        /// </summary>
        public void SetDropdownItemsMode(IEnumerable<ListItem> lstItems)
        {
            _lstGeneralItems.Clear();
            _lstGeneralItems.AddRange(lstItems);
            _strMode = "Dropdown";
        }

        /// <summary>
        /// Character object to search for Restricted items.
        /// </summary>
        public void SetRestrictedMode(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _strMode = "Restricted";
        }

        /// <summary>
        /// Force the window to select a value.
        /// </summary>
        public void ForceItem(string strForceItem)
        {
            _strForceItem = strForceItem;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedName = cboAmmo.Text;
            if (cboAmmo == null)
                _strSelectedItem = string.Empty;
            else if (cboAmmo.DropDownStyle == ComboBoxStyle.DropDownList)
                _strSelectedItem = cboAmmo.SelectedValue?.ToString() ?? _strSelectedName;
            else
                _strSelectedItem = _strSelectedName;
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion Methods
    }
}
