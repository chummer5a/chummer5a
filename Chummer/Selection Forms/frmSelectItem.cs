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
using System.Xml;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectItem : Form
    {
        private List<Gear> _lstGear = new List<Gear>();
        private List<Vehicle> _lstVehicles = new List<Vehicle>();
        private List<ListItem> _lstGeneralItems = new List<ListItem>();
        private string _strMode = "General";
        private Character _objCharacter;
        private bool _blnAllowAutoSelect = true;
        private string _strForceItem = string.Empty;
        private string _strSelectItemOnLoad = string.Empty;

        #region Control Events
        public frmSelectItem()
        {
            InitializeComponent();
            this.TranslateWinForm();
        }

        private void frmSelectItem_Load(object sender, EventArgs e)
        {
            List<ListItem> lstItems = new List<ListItem>();

            if (_strMode == "Gear")
            {
                string strSpace = LanguageManager.GetString("String_Space");
                cboAmmo.DropDownStyle = ComboBoxStyle.DropDownList;
                // Add each of the items to a new List since we need to also grab their plugin information.
                foreach (Gear objGear in _lstGear)
                {
                    StringBuilder sbdAmmoName = new StringBuilder(objGear.DisplayNameShort(GlobalOptions.Language));
                    // Retrieve the plugin information if it has any.
                    if (objGear.Children.Count > 0)
                    {
                        // Append the plugin information to the name.
                        sbdAmmoName.Append(strSpace).Append('[')
                            .AppendJoin(',' + strSpace, objGear.Children.Select(x => x.DisplayNameShort(GlobalOptions.Language)))
                            .Append(']');
                    }
                    if (objGear.Rating > 0)
                        sbdAmmoName.Append(strSpace).Append('(').Append(LanguageManager.GetString(objGear.RatingLabel))
                            .Append(strSpace).Append(objGear.Rating.ToString(GlobalOptions.CultureInfo)).Append(')');
                    sbdAmmoName.Append(strSpace).Append('x').Append(objGear.Quantity.ToString(GlobalOptions.InvariantCultureInfo));
                    lstItems.Add(new ListItem(objGear.InternalId, sbdAmmoName.ToString()));
                }
            }
            else if (_strMode == "Vehicles")
            {
                cboAmmo.DropDownStyle = ComboBoxStyle.DropDownList;
                // Add each of the items to a new List.
                foreach (Vehicle objVehicle in _lstVehicles)
                {
                    lstItems.Add(new ListItem(objVehicle.InternalId, objVehicle.CurrentDisplayName));
                }
            }
            else if (_strMode == "General")
            {
                cboAmmo.DropDownStyle = ComboBoxStyle.DropDownList;
                lstItems = _lstGeneralItems;
            }
            else if (_strMode == "Dropdown")
            {
                cboAmmo.DropDownStyle = ComboBoxStyle.DropDown;
                lstItems = _lstGeneralItems;
            }
            else if (_strMode == "Restricted")
            {
                cboAmmo.DropDownStyle = ComboBoxStyle.DropDown;

                if (!_objCharacter.Options.LicenseRestricted)
                {
                    using (XmlNodeList objXmlList = XmlManager.Load("licenses.xml").SelectNodes("/chummer/licenses/license"))
                    {
                        if (objXmlList != null)
                        {
                            foreach (XmlNode objNode in objXmlList)
                            {
                                string strInnerText = objNode.InnerText;
                                lstItems.Add(new ListItem(strInnerText, objNode.Attributes?["translate"]?.InnerText ?? strInnerText));
                            }
                        }
                    }
                }
                else
                {
                    // Cyberware/Bioware.
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        if (objCyberware.TotalAvailTuple(false).Suffix == 'R')
                        {
                            lstItems.Add(new ListItem(objCyberware.InternalId, objCyberware.CurrentDisplayName));
                        }
                        foreach (Gear objGear in objCyberware.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                        {
                            lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                        }
                    }

                    // Armor.
                    foreach (Armor objArmor in _objCharacter.Armor)
                    {
                        if (objArmor.TotalAvailTuple(false).Suffix == 'R')
                        {
                            lstItems.Add(new ListItem(objArmor.InternalId, objArmor.CurrentDisplayName));
                        }
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.TotalAvailTuple(false).Suffix == 'R')
                            {
                                lstItems.Add(new ListItem(objMod.InternalId, objMod.CurrentDisplayName));
                            }
                            foreach (Gear objGear in objMod.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                            {
                                lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                            }
                        }
                        foreach (Gear objGear in objArmor.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                        {
                            lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                        }
                    }

                    // Weapons.
                    foreach (Weapon objWeapon in _objCharacter.Weapons.GetAllDescendants(x => x.Children))
                    {
                        if (objWeapon.TotalAvailTuple(false).Suffix == 'R')
                        {
                            lstItems.Add(new ListItem(objWeapon.InternalId, objWeapon.CurrentDisplayName));
                        }
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            if (!objAccessory.IncludedInWeapon && objAccessory.TotalAvailTuple(false).Suffix == 'R')
                            {
                                lstItems.Add(new ListItem(objAccessory.InternalId, objAccessory.CurrentDisplayName));
                            }
                            foreach (Gear objGear in objAccessory.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                            {
                                lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                            }
                        }
                    }

                    // Gear.
                    foreach (Gear objGear in _objCharacter.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                    {
                        lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                    }

                    // Vehicles.
                    foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                    {
                        if (objVehicle.TotalAvailTuple(false).Suffix == 'R')
                        {
                            lstItems.Add(new ListItem(objVehicle.InternalId, objVehicle.CurrentDisplayName));
                        }
                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            if (!objMod.IncludedInVehicle && objMod.TotalAvailTuple(false).Suffix == 'R')
                            {
                                lstItems.Add(new ListItem(objMod.InternalId, objMod.CurrentDisplayName));
                            }
                            foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children))
                            {
                                if (objWeapon.TotalAvailTuple(false).Suffix == 'R')
                                {
                                    lstItems.Add(new ListItem(objWeapon.InternalId, objWeapon.CurrentDisplayName));
                                }
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    if (!objAccessory.IncludedInWeapon && objAccessory.TotalAvailTuple(false).Suffix == 'R')
                                    {
                                        lstItems.Add(new ListItem(objAccessory.InternalId, objAccessory.CurrentDisplayName));
                                    }
                                    foreach (Gear objGear in objAccessory.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                                    {
                                        lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                                    }
                                }
                            }
                        }
                        foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                        {
                            if (!objWeaponMount.IncludedInVehicle && objWeaponMount.TotalAvailTuple(false).Suffix == 'R')
                            {
                                lstItems.Add(new ListItem(objWeaponMount.InternalId, objWeaponMount.CurrentDisplayName));
                            }
                            foreach (Weapon objWeapon in objWeaponMount.Weapons.GetAllDescendants(x => x.Children))
                            {
                                if (objWeapon.TotalAvailTuple(false).Suffix == 'R')
                                {
                                    lstItems.Add(new ListItem(objWeapon.InternalId, objWeapon.CurrentDisplayName));
                                }
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    if (!objAccessory.IncludedInWeapon && objAccessory.TotalAvailTuple(false).Suffix == 'R')
                                    {
                                        lstItems.Add(new ListItem(objAccessory.InternalId, objAccessory.CurrentDisplayName));
                                    }
                                    foreach (Gear objGear in objAccessory.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                                    {
                                        lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                                    }
                                }
                            }
                        }
                        foreach (Gear objGear in objVehicle.Gear.DeepWhere(x => x.Children, x => x.TotalAvailTuple(false).Suffix == 'R'))
                        {
                            lstItems.Add(new ListItem(objGear.InternalId, objGear.CurrentDisplayName));
                        }
                    }
                }
            }

            // Populate the lists.
            cboAmmo.BeginUpdate();
            cboAmmo.ValueMember = nameof(ListItem.Value);
            cboAmmo.DisplayMember = nameof(ListItem.Name);
            cboAmmo.DataSource = lstItems;

            // If there's only 1 value in the list, the character doesn't have a choice, so just accept it.
            if (cboAmmo.Items.Count == 1 && _blnAllowAutoSelect)
                AcceptForm();

            if (!string.IsNullOrEmpty(_strForceItem))
            {
                cboAmmo.SelectedIndex = cboAmmo.FindStringExact(_strForceItem);
                if (cboAmmo.SelectedIndex != -1)
                    AcceptForm();
            }
            if (!string.IsNullOrEmpty(_strSelectItemOnLoad))
            {
                if (cboAmmo.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    string strOldSelected = cboAmmo.SelectedValue?.ToString();
                    cboAmmo.SelectedValue = _strSelectItemOnLoad;
                    if (cboAmmo.SelectedIndex == -1 && !string.IsNullOrEmpty(strOldSelected))
                        cboAmmo.SelectedValue = strOldSelected;
                }
                else
                    cboAmmo.Text = _strSelectItemOnLoad;
            }
            cboAmmo.EndUpdate();

            if (cboAmmo.Items.Count < 0)
                cmdOK.Enabled = false;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal ID of the item that was selected.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                if (cboAmmo == null)
                    return null;
                if (cboAmmo.DropDownStyle == ComboBoxStyle.DropDownList && cboAmmo.SelectedValue != null)
                    return cboAmmo.SelectedValue.ToString();
                return cboAmmo.Text;
            }
            set => _strSelectItemOnLoad = value;
        }

        /// <summary>
        /// Name of the item that was selected.
        /// </summary>
        public string SelectedName => cboAmmo.Text;

        /// <summary>
        /// Whether or not the Form should be accepted if there is only one item left in the list.
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
        #endregion

        #region Methods
        /// <summary>
        /// List of Gear that the user can select.
        /// </summary>
        public void SetGearMode(IEnumerable<Gear> lstGears)
        {
            _lstGear = new List<Gear>(lstGears);
            _strMode = "Gear";
        }

        /// <summary>
        /// List of Vehicles that the user can select.
        /// </summary>
        public void SetVehiclesMode(IEnumerable<Vehicle> lstVehicles)
        {
            _lstVehicles = new List<Vehicle>(lstVehicles);
            _strMode = "Vehicles";
        }

        /// <summary>
        /// List of general items that the user can select.
        /// </summary>
        public void SetGeneralItemsMode(IEnumerable<ListItem> lstItems)
        {
            _lstGeneralItems = new List<ListItem>(lstItems);
            _strMode = "General";
        }

        /// <summary>
        /// List of general items that the user can select.
        /// </summary>
        public void SetDropdownItemsMode(IEnumerable<ListItem> lstItems)
        {
            _lstGeneralItems = new List<ListItem>(lstItems);
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
            DialogResult = DialogResult.OK;
        }
        #endregion
    }
}
