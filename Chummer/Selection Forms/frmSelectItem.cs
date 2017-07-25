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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectItem : Form
    {
        private List<Gear> _lstGear = new List<Gear>();
        private List<Vehicle> _lstVehicles = new List<Vehicle>();
        private List<VehicleMod> _lstVehicleMods = new List<VehicleMod>();
        private List<ListItem> _lstGeneralItems = new List<ListItem>();
        private string _strMode = "Gear";
        private Character _objCharacter;
        private bool _blnAllowAutoSelect = true;
        private string _strForceItem = string.Empty;

        #region Control Events
        public frmSelectItem()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void frmSelectItem_Load(object sender, EventArgs e)
        {
            List<ListItem> lstItems = new List<ListItem>();

            if (_strMode == "Gear")
            {
                // Add each of the items to a new List since we need to also grab their plugin information.
                foreach (Gear objGear in _lstGear)
                {
                    ListItem objAmmo = new ListItem();
                    objAmmo.Value = objGear.InternalId;
                    objAmmo.Name = objGear.DisplayNameShort;
                    // Retrieve the plugin information if it has any.
                    if (objGear.Children.Count > 0)
                    {
                        string strPlugins = string.Empty;
                        foreach (Gear objChild in objGear.Children)
                        {
                            strPlugins += objChild.DisplayNameShort + ", ";
                        }
                        // Remove the trailing comma.
                        strPlugins = strPlugins.Substring(0, strPlugins.Length - 2);
                        // Append the plugin information to the name.
                        objAmmo.Name += " [" + strPlugins + "]";
                    }
                    if (objGear.Rating > 0)
                        objAmmo.Name += " (" + LanguageManager.Instance.GetString("String_Rating") + " " + objGear.Rating.ToString() + ")";
                    objAmmo.Name += " x" + objGear.Quantity.ToString();
                    lstItems.Add(objAmmo);
                }
            }
            else if (_strMode == "Vehicles")
            {
                // Add each of the items to a new List.
                foreach (Vehicle objVehicle in _lstVehicles)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objVehicle.InternalId;
                    objItem.Name = objVehicle.DisplayName;
                    lstItems.Add(objItem);
                }
            }
            else if (_strMode == "VehicleMods")
            {
                // Add each of the items to a new List.
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objMod.InternalId;
                    objItem.Name = objMod.DisplayName;
                    lstItems.Add(objItem);
                }
            }
            else if (_strMode == "General")
            {
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
                    XmlDocument objXmlDocument = new XmlDocument();
                    objXmlDocument = XmlManager.Instance.Load("licenses.xml");
                    XmlNodeList objXmlList = objXmlDocument.SelectNodes("/chummer/licenses/license");

                    foreach (XmlNode objNode in objXmlList)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objNode.InnerText;
                        objItem.Name = objNode.Attributes?["translate"]?.InnerText ?? objNode.InnerText;
                        lstItems.Add(objItem);
                    }
                }
                else
                {
                    // Cyberware/Bioware.
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objCyberware.DisplayNameShort;
                            objItem.Name = objCyberware.DisplayNameShort;
                            lstItems.Add(objItem);
                        }
                        foreach (Cyberware objChild in objCyberware.Children)
                        {
                            if (objChild.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objChild.DisplayNameShort;
                                objItem.Name = objChild.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                        }
                    }

                    // Armor.
                    foreach (Armor objArmor in _objCharacter.Armor)
                    {
                        if (objArmor.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objArmor.DisplayNameShort;
                            objItem.Name = objArmor.DisplayNameShort;
                            lstItems.Add(objItem);
                        }
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objMod.DisplayNameShort;
                                objItem.Name = objMod.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                        }
                        foreach (Gear objGear in objArmor.Gear)
                        {
                            if (objGear.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objGear.DisplayNameShort;
                                objItem.Name = objGear.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                        }
                    }

                    // Weapons.
                    foreach (Weapon objWeapon in _objCharacter.Weapons)
                    {
                        if (objWeapon.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objWeapon.DisplayNameShort;
                            objItem.Name = objWeapon.DisplayNameShort;
                            lstItems.Add(objItem);
                        }
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            if (objAccessory.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")) && !objAccessory.IncludedInWeapon)
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objAccessory.DisplayNameShort;
                                objItem.Name = objAccessory.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                        }
                        if (objWeapon.UnderbarrelWeapons.Count > 0)
                        {
                            foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                            {
                                if (objUnderbarrelWeapon.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                                {
                                    ListItem objItem = new ListItem();
                                    objItem.Value = objUnderbarrelWeapon.DisplayNameShort;
                                    objItem.Name = objUnderbarrelWeapon.DisplayNameShort;
                                    lstItems.Add(objItem);
                                }
                                foreach (WeaponAccessory objAccessory in objUnderbarrelWeapon.WeaponAccessories)
                                {
                                    if (objAccessory.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")) && !objAccessory.IncludedInWeapon)
                                    {
                                        ListItem objItem = new ListItem();
                                        objItem.Value = objAccessory.DisplayNameShort;
                                        objItem.Name = objAccessory.DisplayNameShort;
                                        lstItems.Add(objItem);
                                    }
                                }
                            }
                        }
                    }

                    // Gear.
                    foreach (Gear objGear in _objCharacter.Gear)
                    {
                        if (objGear.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objGear.DisplayNameShort;
                            objItem.Name = objGear.DisplayNameShort;
                            lstItems.Add(objItem);
                        }
                        foreach (Gear objChild in objGear.Children)
                        {
                            if (objChild.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objChild.DisplayNameShort;
                                objItem.Name = objChild.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                            foreach (Gear objSubChild in objChild.Children)
                            {
                                if (objSubChild.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                                {
                                    ListItem objItem = new ListItem();
                                    objItem.Value = objSubChild.DisplayNameShort;
                                    objItem.Name = objSubChild.DisplayNameShort;
                                    lstItems.Add(objItem);
                                }
                            }
                        }
                    }

                    // Vehicles.
                    foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                    {
                        if (objVehicle.CalculatedAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objVehicle.DisplayNameShort;
                            objItem.Name = objVehicle.DisplayNameShort;
                            lstItems.Add(objItem);
                        }
                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            if (objMod.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")) && !objMod.IncludedInVehicle)
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objMod.DisplayNameShort;
                                objItem.Name = objMod.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                            foreach (Weapon objWeapon in objMod.Weapons)
                            {
                                if (objWeapon.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                                {
                                    ListItem objItem = new ListItem();
                                    objItem.Value = objWeapon.DisplayNameShort;
                                    objItem.Name = objWeapon.DisplayNameShort;
                                    lstItems.Add(objItem);
                                }
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    if (objAccessory.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")) && !objAccessory.IncludedInWeapon)
                                    {
                                        ListItem objItem = new ListItem();
                                        objItem.Value = objAccessory.DisplayNameShort;
                                        objItem.Name = objAccessory.DisplayNameShort;
                                        lstItems.Add(objItem);
                                    }
                                }
                                if (objWeapon.UnderbarrelWeapons.Count > 0)
                                {
                                    foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                                    {
                                        if (objUnderbarrelWeapon.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                                        {
                                            ListItem objItem = new ListItem();
                                            objItem.Value = objUnderbarrelWeapon.DisplayNameShort;
                                            objItem.Name = objUnderbarrelWeapon.DisplayNameShort;
                                            lstItems.Add(objItem);
                                        }
                                        foreach (WeaponAccessory objAccessory in objUnderbarrelWeapon.WeaponAccessories)
                                        {
                                            if (objAccessory.TotalAvail.EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")) && !objAccessory.IncludedInWeapon)
                                            {
                                                ListItem objItem = new ListItem();
                                                objItem.Value = objAccessory.DisplayNameShort;
                                                objItem.Name = objAccessory.DisplayNameShort;
                                                lstItems.Add(objItem);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        foreach (Gear objGear in objVehicle.Gear)
                        {
                            if (objGear.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objGear.DisplayNameShort;
                                objItem.Name = objGear.DisplayNameShort;
                                lstItems.Add(objItem);
                            }
                            foreach (Gear objChild in objGear.Children)
                            {
                                if (objChild.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                                {
                                    ListItem objItem = new ListItem();
                                    objItem.Value = objChild.DisplayNameShort;
                                    objItem.Name = objChild.DisplayNameShort;
                                    lstItems.Add(objItem);
                                }
                                foreach (Gear objSubChild in objChild.Children)
                                {
                                    if (objSubChild.TotalAvail().EndsWith(LanguageManager.Instance.GetString("String_AvailRestricted")))
                                    {
                                        ListItem objItem = new ListItem();
                                        objItem.Value = objSubChild.DisplayNameShort;
                                        objItem.Name = objSubChild.DisplayNameShort;
                                        lstItems.Add(objItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Populate the lists.
            cboAmmo.BeginUpdate();
            cboAmmo.ValueMember = "Value";
            cboAmmo.DisplayMember = "Name";
            cboAmmo.DataSource = lstItems;

            // If there's only 1 value in the list, the character doesn't have a choice, so just accept it.
            if (cboAmmo.Items.Count == 1 && _blnAllowAutoSelect)
                AcceptForm();

            if (!string.IsNullOrEmpty(_strForceItem))
            {
                cboAmmo.SelectedIndex = cboAmmo.FindStringExact(_strForceItem);
                if (cboAmmo.SelectedIndex != -1)
                    AcceptForm();
                else
                {
                    cboAmmo.DataSource = null;
                    List<ListItem> lstSingle = new List<ListItem>();
                    ListItem objItem = new ListItem();
                    objItem.Value = _strForceItem;
                    objItem.Name = _strForceItem;
                    lstSingle.Add(objItem);
                    cboAmmo.ValueMember = "Value";
                    cboAmmo.DisplayMember = "Name";
                    cboAmmo.DataSource = lstSingle;
                    cboAmmo.SelectedIndex = 0;
                    AcceptForm();
                }
            }
            cboAmmo.EndUpdate();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cboAmmo_DropDown(object sender, EventArgs e)
        {
            // Resize the width of the DropDown so that the longest name fits.
            ComboBox objSender = (ComboBox)sender;
            int intWidth = objSender.DropDownWidth;
            Graphics objGraphics = objSender.CreateGraphics();
            Font objFont = objSender.Font;
            int intScrollWidth = (objSender.Items.Count > objSender.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;
            int intNewWidth;
            foreach (ListItem objItem in ((ComboBox)sender).Items)
            {
                intNewWidth = (int)objGraphics.MeasureString(objItem.Name, objFont).Width + intScrollWidth;
                if (intWidth < intNewWidth)
                {
                    intWidth = intNewWidth;
                }
            }
            objSender.DropDownWidth = intWidth;
        }
        #endregion

        #region Properties
        /// <summary>
        /// List of Gear that the user can select.
        /// </summary>
        public List<Gear> Gear
        {
            set
            {
                _lstGear = value;
                _strMode = "Gear";
            }
        }

        /// <summary>
        /// List of Vehicles that the user can selet.
        /// </summary>
        public List<Vehicle> Vehicles
        {
            set
            {
                _lstVehicles = value;
                _strMode = "Vehicles";
            }
        }

        /// <summary>
        /// List of Vehicle Mods that the user can select.
        /// </summary>
        public List<VehicleMod> VehicleMods
        {
            set
            {
                _lstVehicleMods = value;
                _strMode = "VehicleMods";
            }
        }

        /// <summary>
        /// List of general items that the user can select.
        /// </summary>
        public List<ListItem> GeneralItems
        {
            set
            {
                _lstGeneralItems = value;
                _strMode = "General";
            }
        }

        /// <summary>
        /// List of general items that the user can select.
        /// </summary>
        public List<ListItem> DropdownItems
        {
            set
            {
                _lstGeneralItems = value;
                _strMode = "Dropdown";
            }
        }

        /// <summary>
        /// Character object to search for Restricted items.
        /// </summary>
        public Character Character
        {
            set
            {
                _objCharacter = value;
                _strMode = "Restricted";
            }
        }

        /// <summary>
        /// Name of the item that was selected.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                if (cboAmmo.SelectedValue != null)
                {
                    return cboAmmo.SelectedValue.ToString();
                }
                else
                {
                    return cboAmmo.Text;
                }
            }
        }

        /// <summary>
        /// Whether or not the Form should be accepted if there is only one item left in the list.
        /// </summary>
        public bool AllowAutoSelect
        {
            get
            {
                return _blnAllowAutoSelect;
            }
            set
            {
                _blnAllowAutoSelect = value;
            }
        }

        /// <summary>
        /// Description to show in the window.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        /// <summary>
        /// Force the window to select a value.
        /// </summary>
        public string ForceItem
        {
            set
            {
                _strForceItem = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            cboAmmo.Left = lblAmmoLabel.Left + lblAmmoLabel.Width + 6;
            cboAmmo.Width = Width - cboAmmo.Left - 19;
        }
        #endregion
    }
}
