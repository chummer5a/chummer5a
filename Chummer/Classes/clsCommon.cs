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
using System.Diagnostics;
 using System.Text;
 using System.Windows.Forms;
 using System.Drawing;
 using System.Linq;
 using Chummer.Backend.Equipment;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class CommonFunctions
    {
        #region Enums
        public enum LogType
        {
            Message = 0,
            Alert = 1,
            Error = 2,
            Content = 3,
            Entering = 4,
            Exiting = 5,
        }
        public enum MentorType
        {
            Mentor = 0,
            Paragon = 1
        }
        #endregion

        #region XPath Evaluators
        // TODO: implement a sane expression evaluator
        // A single instance of an XmlDocument and its corresponding XPathNavigator helps reduce overhead of evaluating XPaths that just contain mathematical operations
        static XmlDocument objXPathNavigatorDocument = new XmlDocument();
        static XPathNavigator objXPathNavigator = objXPathNavigatorDocument.CreateNavigator();

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="strXPath">String as XPath Expression to evaluate</param>
        /// <returns>System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object EvaluateInvariantXPath(string strXPath)
        {
            return objXPathNavigator.Evaluate(strXPath);
        }

        /// <summary>
        /// Evaluate an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="objXPath">XPath Expression to evaluate</param>
        /// <returns>System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object EvaluateInvariantXPath(XPathExpression objXPath)
        {
            return objXPathNavigator.Evaluate(objXPath);
        }
        #endregion

        #region Find Functions
        /// <summary>
        /// Locate an object (Needle) within a list (Haystack) based on only a GUID match.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T FindById<T>(string strGuid, IEnumerable<T> lstHaystack) where T : IItemWithGuid
        {
            if (strGuid == Guid.Empty.ToString())
            {
                return default(T);
            }
            
            return lstHaystack.FirstOrDefault(x => x.InternalId == strGuid);
        }

        /// <summary>
        /// Locate an object (Needle) within a list (Haystack) based on GUID match and non-zero name.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T FindByIdWithNameCheck<T>(string strGuid, IEnumerable<T> lstHaystack) where T : INamedItemWithGuid
        {
            if (strGuid == Guid.Empty.ToString())
            {
                return default(T);
            }

            return lstHaystack.FirstOrDefault(x => x.InternalId == strGuid && !string.IsNullOrEmpty(x.Name));
        }

        /// <summary>
        /// Locate an object (Needle) within a list and its children (Haystack) based on GUID match and non-zero name.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T DeepFindById<T>(string strGuid, IEnumerable<T> lstHaystack) where T : INamedParentWithGuid<T>
        {
            if (strGuid == Guid.Empty.ToString())
            {
                return default(T);
            }

            return lstHaystack.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGuid && !string.IsNullOrEmpty(x.Name));
        }

        /// <summary>
        /// Locate an object (Needle) within a list and its children (Haystack) based on name match and non-zero guid.
        /// </summary>
        /// <param name="strName">Name of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T DeepFindByName<T>(string strName, IEnumerable<T> lstHaystack) where T : INamedParentWithGuid<T>
        {
            if (string.IsNullOrEmpty(strName))
            {
                return default(T);
            }

            return lstHaystack.DeepFirstOrDefault(x => x.Children, x => x.Name == strName && x.InternalId != Guid.Empty.ToString());
        }

        /// <summary>
        /// Locate a Commlink.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCommlink">List of Commlinks to search.</param>
        public static Commlink FindCommlink(string strGuid, IEnumerable<Gear> lstCommlink)
        {
            return DeepFindById(strGuid, lstCommlink) as Commlink;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Gear FindVehicleGear(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            Vehicle objFoundVehicle = null;
            WeaponAccessory objFoundWeaponAccessory = null;
            Cyberware objFoundCyberware = null;
            return FindVehicleGear(strGuid,lstVehicles, out objFoundVehicle, out objFoundWeaponAccessory, out objFoundCyberware);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Gear was found in.</param>
        public static Gear FindVehicleGear(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle objFoundVehicle, out WeaponAccessory objFoundWeaponAccessory, out Cyberware objFoundCyberware)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    objReturn = DeepFindById(strGuid, objVehicle.Gear);
                    if (!string.IsNullOrEmpty(objReturn?.Name))
                    {
                        objFoundVehicle = objVehicle;
                        objFoundWeaponAccessory = null;
                        objFoundCyberware = null;
                        return objReturn;
                    }

                    // Look for any Gear that might be attached to this Vehicle through Weapon Accessories or Cyberware.
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        // Weapon Accessories.
                        WeaponAccessory objAccessory;
                        objReturn = FindWeaponGear(strGuid, objMod.Weapons, out objAccessory);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponAccessory = objAccessory;
                            objFoundCyberware = null;
                            return objReturn;
                        }

                        // Cyberware.
                        Cyberware objCyberware;
                        objReturn = FindCyberwareGear(strGuid, objMod.Cyberware, out objCyberware);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponAccessory = null;
                            objFoundCyberware = objCyberware;
                            return objReturn;
                        }
                    }
                }
            }

            objFoundVehicle = null;
            objFoundWeaponAccessory = null;
            objFoundCyberware = null;
            return null;
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the VehicleMod to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static VehicleMod FindVehicleMod(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            Vehicle objFoundVehicle = null;
            return FindVehicleMod(strGuid, lstVehicles, out objFoundVehicle);
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the VehicleMod to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the VehicleMod was found in.</param>
        public static VehicleMod FindVehicleMod(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    if (!string.IsNullOrEmpty(objVehicle.Name))
                    {
                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            if (objMod.InternalId == strGuid && !string.IsNullOrEmpty(objMod.Name))
                            {
                                objFoundVehicle = objVehicle;
                                return objMod;
                            }
                        }
                    }
                }
            }

            objFoundVehicle = null;
            return null;
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Weapon FindVehicleWeapon(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            Vehicle objFoundVehicle = null;
            WeaponMount objFoundWeaponMount = null;
            VehicleMod objFoundVehicleMod = null;
            return FindVehicleWeapon(strGuid, lstVehicles, out objFoundVehicle, out objFoundWeaponMount, out objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicleMod">Vehicle mod that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(string strGuid, IEnumerable<Vehicle> lstVehicles, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod)
        {
            Vehicle objFoundVehicle = null;
            return FindVehicleWeapon(strGuid, lstVehicles, out objFoundVehicle, out objFoundWeaponMount, out objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
        {
            WeaponMount objFoundWeaponMount = null;
            VehicleMod objFoundVehicleMod = null;
            return FindVehicleWeapon(strGuid, lstVehicles, out objFoundVehicle, out objFoundWeaponMount, out objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        /// <param name="objFoundVehicleMod">Vehicle mod that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Weapon objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    if (!string.IsNullOrEmpty(objVehicle.Name))
                    {
                        objReturn = DeepFindById(strGuid, objVehicle.Weapons);
                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponMount = null;
                            objFoundVehicleMod = null;
                            return objReturn;
                        }

                        foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                        {
                            objReturn = DeepFindById(strGuid, objMod.Weapons);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                objFoundVehicle = objVehicle;
                                objFoundWeaponMount = objMod;
                                objFoundVehicleMod = null;
                                return objReturn;
                            }
                        }

                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            objReturn = DeepFindById(strGuid, objMod.Weapons);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                objFoundVehicle = objVehicle;
                                objFoundVehicleMod = objMod;
                                objFoundWeaponMount = null;
                                return objReturn;
                            }
                        }
                    }
                }
            }

            objFoundVehicle = null;
            objFoundWeaponMount = null;
            objFoundVehicleMod = null;
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strGuid"></param>
        /// <param name="lstVehicles"></param>
        /// <returns></returns>
        internal static WeaponMount FindVehicleWeaponMount(string strGuid, List<Vehicle> lstVehicles, out Vehicle outVehicle)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    if (!string.IsNullOrEmpty(objVehicle.Name))
                    {
                        foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                        {
                            if (objMod.InternalId == strGuid && !string.IsNullOrEmpty(objMod.Name))
                            {
                                outVehicle = objVehicle;
                                return objMod;
                            }
                        }
                    }
                }
            }
            outVehicle = null;
            return null;
        }

        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static WeaponAccessory FindVehicleWeaponAccessory(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            Weapon objFoundWeapon = null;
            return FindVehicleWeaponAccessory(strGuid, lstVehicles, out objFoundWeapon);
        }

        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static WeaponAccessory FindVehicleWeaponAccessory(string strGuid, IEnumerable<Vehicle> lstVehicles, out Weapon objFoundWeapon)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                WeaponAccessory objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    if (!string.IsNullOrEmpty(objVehicle.Name))
                    {
                        objReturn = FindWeaponAccessory(strGuid, objVehicle.Weapons, out objFoundWeapon);
                        if (!string.IsNullOrEmpty(objReturn?.Name))
                            return objReturn;

                        foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                        {
                            objReturn = FindWeaponAccessory(strGuid, objMod.Weapons, out objFoundWeapon);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                return objReturn;
                            }
                        }

                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            objReturn = FindWeaponAccessory(strGuid, objMod.Weapons, out objFoundWeapon);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                return objReturn;
                            }
                        }
                    }
                }
            }

            objFoundWeapon = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Cyberware to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
        public static Cyberware FindVehicleCyberware(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            VehicleMod objFoundVehicleMod = null;
            return FindVehicleCyberware(strGuid, lstVehicles, out objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Cyberware to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
        public static Cyberware FindVehicleCyberware(string strGuid, IEnumerable<Vehicle> lstVehicles, out VehicleMod objFoundVehicleMod)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Cyberware objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    if (!string.IsNullOrEmpty(objVehicle.Name))
                    {
                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            objReturn = DeepFindById(strGuid, objMod.Cyberware);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                objFoundVehicleMod = objMod;
                                return objReturn;
                            }
                        }
                    }
                }
            }

            objFoundVehicleMod = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        public static Gear FindArmorGear(string strGuid, IEnumerable<Armor> lstArmors)
        {
            Armor objFoundArmor = null;
            ArmorMod objFoundArmorMod = null;
            return FindArmorGear(strGuid, lstArmors, out objFoundArmor, out objFoundArmorMod);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="objFoundArmor">Armor that the Gear was found in.</param>
        public static Gear FindArmorGear(string strGuid, IEnumerable<Armor> lstArmors, out Armor objFoundArmor)
        {
            ArmorMod objFoundArmorMod = null;
            return FindArmorGear(strGuid, lstArmors, out objFoundArmor, out objFoundArmorMod);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="objFoundArmorMod">Armor mod that the Gear was found in.</param>
        public static Gear FindArmorGear(string strGuid, IEnumerable<Armor> lstArmors, out ArmorMod objFoundArmorMod)
        {
            Armor objFoundArmor = null;
            return FindArmorGear(strGuid, lstArmors, out objFoundArmor, out objFoundArmorMod);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="objFoundArmor">Armor that the Gear was found in.</param>
        /// <param name="objFoundArmorMod">Armor mod that the Gear was found in.</param>
        public static Gear FindArmorGear(string strGuid, IEnumerable<Armor> lstArmors, out Armor objFoundArmor, out ArmorMod objFoundArmorMod)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Armor objArmor in lstArmors)
                {
                    if (!string.IsNullOrEmpty(objArmor.Name))
                    {
                        objReturn = DeepFindById(strGuid, objArmor.Gear);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundArmor = objArmor;
                            objFoundArmorMod = null;
                            return objReturn;
                        }
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            objReturn = DeepFindById(strGuid, objMod.Gear);

                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                objFoundArmor = objArmor;
                                objFoundArmorMod = objMod;
                                return objReturn;
                            }
                        }
                    }
                }
            }

            objFoundArmor = null;
            objFoundArmorMod = null;
            return null;
        }

        /// <summary>
        /// Locate an Armor Mod within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the ArmorMod to Find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        public static ArmorMod FindArmorMod(string strGuid, IEnumerable<Armor> lstArmors)
        {
            if (strGuid == Guid.Empty.ToString())
                return null;
            foreach (Armor objArmor in lstArmors)
            {
                foreach (ArmorMod objMod in objArmor.ArmorMods)
                {
                    if (objMod.InternalId == strGuid)
                        return objMod;
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        public static Gear FindCyberwareGear(string strGuid, IEnumerable<Cyberware> lstCyberware)
        {
            Cyberware objFoundCyberware = null;
            return FindCyberwareGear(strGuid, lstCyberware, out objFoundCyberware);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public static Gear FindCyberwareGear(string strGuid, IEnumerable<Cyberware> lstCyberware, out Cyberware objFoundCyberware)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Cyberware objCyberware in lstCyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                {
                    objReturn = DeepFindById(strGuid, objCyberware.Gear);

                    if (!string.IsNullOrEmpty(objReturn?.Name))
                    {
                        objFoundCyberware = objCyberware;
                        return objReturn;
                    }
                }
            }

            objFoundCyberware = null;
            return null;
        }

        /// <summary>
        /// Locate a WeaponAccessory within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the WeaponAccessory to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        public static WeaponAccessory FindWeaponAccessory(string strGuid, IEnumerable<Weapon> lstWeapons)
        {
            Weapon objFoundWeapon = null;
            return FindWeaponAccessory(strGuid, lstWeapons, out objFoundWeapon);
        }

        /// <summary>
        /// Locate a WeaponAccessory within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the WeaponAccessory to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundWeapon">Weapon in which the Accesory was found.</param>
        public static WeaponAccessory FindWeaponAccessory(string strGuid, IEnumerable<Weapon> lstWeapons, out Weapon objFoundWeapon)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Count > 0))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        if (objAccessory.InternalId == strGuid)
                        {
                            objFoundWeapon = objWeapon;
                            return objAccessory;
                        }
                    }
                }
            }

            objFoundWeapon = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        public static Gear FindWeaponGear(string strGuid, IEnumerable<Weapon> lstWeapons)
        {
            WeaponAccessory objFoundAccessory = null;
            return FindWeaponGear(strGuid, lstWeapons, out objFoundAccessory);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundAccessory">WeaponAccessory that the Gear was found in.</param>
        public static Gear FindWeaponGear(string strGuid, IEnumerable<Weapon> lstWeapons, out WeaponAccessory objFoundAccessory)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        objReturn = DeepFindById(strGuid, objAccessory.Gear);

                        if (objReturn != null)
                        {
                            objFoundAccessory = objAccessory;
                            return objReturn;
                        }
                    }
                }
            }

            objFoundAccessory = null;
            return null;
        }

        /// <summary>
        /// Locate an Enhancement within the character's Enhancements.
        /// </summary>
        /// <param name="strGuid">InternalId of the Art to find.</param>
        /// <param name="objCharacter">The character to search.</param>
        public static Enhancement FindEnhancement(string strGuid, Character objCharacter)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (Enhancement objEnhancement in objCharacter.Enhancements)
                {
                    if (objEnhancement.InternalId == strGuid)
                        return objEnhancement;
                }
                foreach (Power objPower in objCharacter.Powers)
                {
                    foreach (Enhancement objEnhancement in objPower.Enhancements)
                    {
                        if (objEnhancement.InternalId == strGuid)
                            return objEnhancement;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Locate a Martial Art Advantage within the character's Martial Arts.
        /// </summary>
        /// <param name="strGuid">InternalId of the Martial Art Advantage to find.</param>
        /// <param name="lstMartialArts">List of Martial Arts to search.</param>
        public static MartialArtAdvantage FindMartialArtAdvantage(string strGuid, IEnumerable<MartialArt> lstMartialArts)
        {
            MartialArt objFoundMartialArt = null;
            return FindMartialArtAdvantage(strGuid, lstMartialArts, out objFoundMartialArt);
        }

        /// <summary>
        /// Locate a Martial Art Advantage within the character's Martial Arts.
        /// </summary>
        /// <param name="strGuid">InternalId of the Martial Art Advantage to find.</param>
        /// <param name="lstMartialArts">List of Martial Arts to search.</param>
        /// <param name="objFoundMartialArt">MartialArt the Advantage was found in.</param>
        public static MartialArtAdvantage FindMartialArtAdvantage(string strGuid, IEnumerable<MartialArt> lstMartialArts, out MartialArt objFoundMartialArt)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (MartialArt objArt in lstMartialArts)
                {
                    foreach (MartialArtAdvantage objAdvantage in objArt.Advantages)
                    {
                        if (objAdvantage.InternalId == strGuid)
                        {
                            objFoundMartialArt = objArt;
                            return objAdvantage;
                        }
                    }
                }
            }

            objFoundMartialArt = null;
            return null;
        }

        /// <summary>
        /// Find a TreeNode in a TreeView based on its Tag.
        /// </summary>
        /// <param name="strGuid">InternalId of the Node to find.</param>
        /// <param name="treTree">TreeView to search.</param>
        public static TreeNode FindNode(string strGuid, TreeView treTree)
        {
            if (strGuid != Guid.Empty.ToString() && treTree != null)
            {
                TreeNode objFound;
                foreach (TreeNode objNode in treTree.Nodes)
                {
                    if (objNode.Tag.ToString() == strGuid)
                        return objNode;

                    objFound = FindNode(strGuid, objNode);
                    if (objFound != null)
                        return objFound;
                }
            }
            return null;
        }

        /// <summary>
        /// Find a TreeNode in a TreeNode based on its Tag.
        /// </summary>
        /// <param name="strGuid">InternalId of the Node to find.</param>
        /// <param name="objNode">TreeNode to search.</param>
        public static TreeNode FindNode(string strGuid, TreeNode objNode)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                TreeNode objFound;
                foreach (TreeNode objChild in objNode.Nodes)
                {
                    if (objChild.Tag.ToString() == strGuid)
                        return objChild;

                    objFound = FindNode(strGuid, objChild);
                    if (objFound != null)
                        return objFound;
                }
            }
            return null;
        }
        #endregion

        #region Delete Functions
        /// <summary>
        /// Recursive method to delete a piece of Gear and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        /// <param name="objGear">Gear to delete.</param>
        /// <param name="treWeapons">TreeView that holds the list of Weapons.</param>
        /// <param name="objImprovementManager">Improvement Manager the character is using.</param>
        public static decimal DeleteGear(Character objCharacter, Gear objGear, TreeView treWeapons, TreeView treVehicles)
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Gear objChild in objGear.Children)
                decReturn += DeleteGear(objCharacter, objChild, treWeapons, treVehicles);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (objGear.WeaponID != Guid.Empty.ToString())
            {
                List<string> lstNodesToRemoveIds = new List<string>();
                List<Tuple<Weapon, Vehicle, VehicleMod>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod>>();
                foreach (Weapon objWeapon in objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objGear.InternalId))
                {
                    lstNodesToRemoveIds.Add(objWeapon.InternalId);
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, null, null));
                }
                foreach (Vehicle objVehicle in objCharacter.Vehicles)
                {
                    foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objGear.InternalId))
                    {
                        lstNodesToRemoveIds.Add(objWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, null));
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objGear.InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, objMod));
                        }
                    }
                }
                // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
                foreach (Tuple<Weapon, Vehicle, VehicleMod> objLoopTuple in lstWeaponsToDelete)
                {
                    Weapon objWeapon = objLoopTuple.Item1;
                    decReturn += objWeapon.TotalCost + DeleteWeapon(objCharacter, objWeapon, treWeapons, treVehicles);
                    if (objWeapon.Parent != null)
                        objWeapon.Parent.Children.Remove(objWeapon);
                    else if (objLoopTuple.Item3 != null)
                        objLoopTuple.Item3.Weapons.Remove(objWeapon);
                    else if (objLoopTuple.Item2 != null)
                        objLoopTuple.Item2.Weapons.Remove(objWeapon);
                    else
                        objCharacter.Weapons.Remove(objWeapon);
                }
                foreach (string strNodeId in lstNodesToRemoveIds)
                {
                    // Remove the Weapons from the TreeView.
                    TreeNode objLoopNode = FindNode(strNodeId, treWeapons) ?? FindNode(strNodeId, treVehicles);
                    objLoopNode?.Remove();
                }
            }

            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.Gear, objGear.InternalId);

            // If a Focus is being removed, make sure the actual Focus is being removed from the character as well.
            if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci")
            {
                HashSet<Focus> lstRemoveFoci = new HashSet<Focus>();
                foreach (Focus objFocus in objCharacter.Foci)
                {
                    if (objFocus.GearId == objGear.InternalId)
                        lstRemoveFoci.Add(objFocus);
                }
                foreach (Focus objFocus in lstRemoveFoci)
                {
                    /*
                    foreach (Power objPower in objCharacter.Powers)
                    {
                        if (objPower.BonusSource == objFocus.GearId)
                        {
                            //objPower.FreeLevels -= (objFocus.Rating / 4);
                        }
                    }
                    */
                    objCharacter.Foci.Remove(objFocus);
                }
            }
            // If a Stacked Focus is being removed, make sure the Stacked Foci and its bonuses are being removed.
            else if (objGear.Category == "Stacked Focus")
            {
                StackedFocus objStack = objCharacter.StackedFoci.FirstOrDefault(x => x.GearId == objGear.InternalId);
                if (objStack != null)
                {
                    ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                    objCharacter.StackedFoci.Remove(objStack);
                }
            }

            Commlink objCommlink = (objGear as Commlink);
            if (objCommlink != null)
                objCommlink.IsActive = false;
            return decReturn;
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        /// <param name="objGear">Gear to delete.</param>
        /// <param name="treWeapons">TreeView that holds the list of Weapons.</param>
        /// <param name="objImprovementManager">Improvement Manager the character is using.</param>
        public static decimal DeleteCyberware(Character objCharacter, Cyberware objCyberware, TreeView treWeapons, TreeView treVehicles)
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Cyberware objChild in objCyberware.Children)
                decReturn += DeleteCyberware(objCharacter, objChild, treWeapons, treVehicles);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (objCyberware.WeaponID != Guid.Empty.ToString())
            {
                List<string> lstNodesToRemoveIds = new List<string>();
                List<Tuple<Weapon, Vehicle, VehicleMod>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod>>();
                foreach (Weapon objWeapon in objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCyberware.InternalId))
                {
                    lstNodesToRemoveIds.Add(objWeapon.InternalId);
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, null, null));
                }
                foreach (Vehicle objVehicle in objCharacter.Vehicles)
                {
                    foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCyberware.InternalId))
                    {
                        lstNodesToRemoveIds.Add(objWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, null));
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCyberware.InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, objMod));
                        }
                    }
                }
                // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
                foreach (Tuple<Weapon, Vehicle, VehicleMod> objLoopTuple in lstWeaponsToDelete)
                {
                    Weapon objWeapon = objLoopTuple.Item1;
                    decReturn += objWeapon.TotalCost + DeleteWeapon(objCharacter, objWeapon, treWeapons, treVehicles);
                    if (objWeapon.Parent != null)
                        objWeapon.Parent.Children.Remove(objWeapon);
                    else if (objLoopTuple.Item3 != null)
                        objLoopTuple.Item3.Weapons.Remove(objWeapon);
                    else if (objLoopTuple.Item2 != null)
                        objLoopTuple.Item2.Weapons.Remove(objWeapon);
                    else
                        objCharacter.Weapons.Remove(objWeapon);
                }
                foreach (string strNodeId in lstNodesToRemoveIds)
                {
                    // Remove the Weapons from the TreeView.
                    TreeNode objLoopNode = FindNode(strNodeId, treWeapons) ?? FindNode(strNodeId, treVehicles);
                    objLoopNode?.Remove();
                }
            }

            // Remove any Vehicle that the Cyberware created.
            if (objCyberware.VehicleID != Guid.Empty.ToString())
            {
                List<string> lstNodesToRemoveIds = new List<string>();
                List<Vehicle> lstVehiclesToRemove = new List<Vehicle>();
                foreach (Vehicle objLoopVehicle in objCharacter.Vehicles)
                {
                    if (objLoopVehicle.ParentID == objCyberware.InternalId)
                    {
                        lstNodesToRemoveIds.Add(objLoopVehicle.InternalId);
                        lstVehiclesToRemove.Add(objLoopVehicle);
                    }
                }
                foreach (Vehicle objLoopVehicle in lstVehiclesToRemove)
                {
                    decReturn += objLoopVehicle.TotalCost;
                    objCharacter.Vehicles.Remove(objLoopVehicle);
                    foreach (Gear objLoopGear in objLoopVehicle.Gear)
                    {
                        decReturn += DeleteGear(objCharacter, objLoopGear, treWeapons, treVehicles);
                    }
                    foreach (Weapon objLoopWeapon in objLoopVehicle.Weapons)
                    {
                        decReturn += DeleteWeapon(objCharacter, objLoopWeapon, treWeapons, treVehicles);
                    }
                    foreach (VehicleMod objLoopMod in objLoopVehicle.Mods)
                    {
                        foreach (Weapon objLoopWeapon in objLoopMod.Weapons)
                        {
                            decReturn += DeleteWeapon(objCharacter, objLoopWeapon, treWeapons, treVehicles);
                        }
                        foreach (Cyberware objLoopCyberware in objLoopMod.Cyberware)
                        {
                            decReturn += DeleteCyberware(objCharacter, objLoopCyberware, treWeapons, treVehicles);
                        }
                    }
                }
                foreach (string strNodeId in lstNodesToRemoveIds)
                {
                    // Remove the Weapons from the TreeView.
                    FindNode(strNodeId, treVehicles)?.Remove();
                }
            }

            ImprovementManager.RemoveImprovements(objCharacter, objCyberware.SourceType, objCyberware.InternalId);
            if (objCyberware.PairBonus != null)
            {
                List<Cyberware> lstPairableCyberwares = objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Name == objCyberware.Name && x.Extra == objCyberware.Extra && x.IsModularCurrentlyEquipped).ToList();
                int intCyberwaresCount = lstPairableCyberwares.Count - 1;
                if (!string.IsNullOrEmpty(objCyberware.Location))
                {
                    intCyberwaresCount = Math.Min(lstPairableCyberwares.Count(x => x.Location == objCyberware.Location) - 1, lstPairableCyberwares.Count(x => x.Location != objCyberware.Location));
                }
                foreach (Cyberware objLoopCyberware in lstPairableCyberwares.Where(x => x.InternalId != objCyberware.InternalId))
                {
                    ImprovementManager.RemoveImprovements(objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId);
                    if (objLoopCyberware.Bonus != null)
                        ImprovementManager.CreateImprovements(objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.Bonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort);
                    if (objLoopCyberware.WirelessOn && objLoopCyberware.WirelessBonus != null)
                        ImprovementManager.CreateImprovements(objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.WirelessBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort);
                    if (intCyberwaresCount > 0 && intCyberwaresCount % 2 == 0)
                    {
                        ImprovementManager.CreateImprovements(objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort);
                    }
                    intCyberwaresCount -= 1;
                }
            }

            foreach (Gear objLoopGear in objCyberware.Gear)
            {
                decReturn += DeleteGear(objCharacter, objLoopGear, treWeapons, treVehicles);
            }

            return decReturn;
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        /// <param name="objWeapon">Weapon to delete.</param>
        /// <param name="treWeapons">TreeView that holds the list of Weapons.</param>
        /// <param name="treVehicles">TreeView that holds the list of Vehicles.</param>
        public static decimal DeleteWeapon(Character objCharacter, Weapon objWeapon, TreeView treWeapons, TreeView treVehicles)
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Weapon objChild in objWeapon.Children)
                decReturn += DeleteWeapon(objCharacter, objChild, treWeapons, treVehicles);

            foreach (WeaponAccessory objLoopAccessory in objWeapon.WeaponAccessories)
            {
                foreach (Gear objLoopGear in objLoopAccessory.Gear)
                {
                    decReturn += DeleteGear(objCharacter, objLoopGear, treWeapons, treVehicles);
                }
            }

            List<string> lstNodesToRemoveIds = new List<string>();
            List<Tuple<Weapon, Vehicle, VehicleMod>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod>>();
            foreach (Weapon objDeleteWeapon in objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objWeapon.InternalId))
            {
                lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objDeleteWeapon, null, null));
            }
            foreach (Vehicle objVehicle in objCharacter.Vehicles)
            {
                foreach (Weapon objDeleteWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objWeapon.InternalId))
                {
                    lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objDeleteWeapon, objVehicle, null));
                }

                foreach (VehicleMod objMod in objVehicle.Mods)
                {
                    foreach (Weapon objDeleteWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objWeapon.InternalId))
                    {
                        lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objDeleteWeapon, objVehicle, objMod));
                    }
                }
            }
            // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
            foreach (Tuple<Weapon, Vehicle, VehicleMod> objLoopTuple in lstWeaponsToDelete)
            {
                Weapon objDeleteWeapon = objLoopTuple.Item1;
                decReturn += objDeleteWeapon.TotalCost + DeleteWeapon(objCharacter, objDeleteWeapon, treWeapons, treVehicles);
                if (objDeleteWeapon.Parent != null)
                    objDeleteWeapon.Parent.Children.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item3 != null)
                    objLoopTuple.Item3.Weapons.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item2 != null)
                    objLoopTuple.Item2.Weapons.Remove(objDeleteWeapon);
                else
                    objCharacter.Weapons.Remove(objDeleteWeapon);
            }
            foreach (string strNodeId in lstNodesToRemoveIds)
            {
                // Remove the Weapons from the TreeView.
                TreeNode objLoopNode = FindNode(strNodeId, treWeapons) ?? FindNode(strNodeId, treVehicles);
                objLoopNode?.Remove();
            }

            return decReturn;
        }

        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        /// <param name="treArmor"></param>
        /// <param name="treWeapons"></param>
        /// <param name="_objImprovementManager"></param>
        public static decimal DeleteArmor(Character objCharacter, TreeView treArmor, TreeView treWeapons, TreeView treVehicles)
        {
            if (!ConfirmDelete(objCharacter, LanguageManager.GetString("Message_DeleteArmor")))
                return 0.0m;

            TreeNode objSelectedNode = treArmor.SelectedNode;
            TreeNodeCollection objWeaponNodes = treWeapons.Nodes;
            if (objSelectedNode == null)
                return 0.0m;
            decimal decReturn = 0.0m;
            if (objSelectedNode.Level == 1)
            {
                Armor objArmor = FindByIdWithNameCheck(objSelectedNode.Tag.ToString(), objCharacter.Armor);
                if (objArmor == null)
                    return 0.0m;
                // Remove any Improvements created by the Armor and its children.
                foreach (ArmorMod objMod in objArmor.ArmorMods)
                {
                    // Remove the Cyberweapon created by the Mod if applicable.
                    if (objMod.WeaponID != Guid.Empty.ToString())
                    {
                        List<string> lstNodesToRemoveIds = new List<string>();
                        List<Tuple<Weapon, Vehicle, VehicleMod>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod>>();
                        foreach (Weapon objWeapon in objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objMod.InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, null, null));
                        }
                        foreach (Vehicle objVehicle in objCharacter.Vehicles)
                        {
                            foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objMod.InternalId))
                            {
                                lstNodesToRemoveIds.Add(objWeapon.InternalId);
                                lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, null));
                            }

                            foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                            {
                                foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objMod.InternalId))
                                {
                                    lstNodesToRemoveIds.Add(objWeapon.InternalId);
                                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, objVehicleMod));
                                }
                            }
                        }
                        foreach (Tuple<Weapon, Vehicle, VehicleMod> objLoopTuple in lstWeaponsToDelete)
                        {
                            Weapon objDeleteWeapon = objLoopTuple.Item1;
                            decReturn += objDeleteWeapon.TotalCost + DeleteWeapon(objCharacter, objDeleteWeapon, treWeapons, treVehicles);
                            if (objDeleteWeapon.Parent != null)
                                objDeleteWeapon.Parent.Children.Remove(objDeleteWeapon);
                            else if (objLoopTuple.Item3 != null)
                                objLoopTuple.Item3.Weapons.Remove(objDeleteWeapon);
                            else if (objLoopTuple.Item2 != null)
                                objLoopTuple.Item2.Weapons.Remove(objDeleteWeapon);
                            else
                                objCharacter.Weapons.Remove(objDeleteWeapon);
                        }
                        foreach (string strNodeId in lstNodesToRemoveIds)
                        {
                            // Remove the Weapons from the TreeView.
                            TreeNode objLoopNode = FindNode(strNodeId, treWeapons) ?? FindNode(strNodeId, treVehicles);
                            objLoopNode?.Remove();
                        }
                    }

                    ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                    // Remove any Improvements created by the Armor's Gear.
                    foreach (Gear objGear in objMod.Gear)
                        decReturn += DeleteGear(objCharacter, objGear, treWeapons, treVehicles);
                }
                ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.Armor, objArmor.InternalId);

                // Remove any Improvements created by the Armor's Gear.
                foreach (Gear objGear in objArmor.Gear)
                    decReturn += DeleteGear(objCharacter, objGear, treWeapons, treVehicles);

                // Remove the Cyberweapon created by the Mod if applicable.
                if (objArmor.WeaponID != Guid.Empty.ToString())
                {
                    List<string> lstNodesToRemoveIds = new List<string>();
                    List<Tuple<Weapon, Vehicle, VehicleMod>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod>>();
                    foreach (Weapon objWeapon in objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objArmor.InternalId))
                    {
                        lstNodesToRemoveIds.Add(objWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, null, null));
                    }
                    foreach (Vehicle objVehicle in objCharacter.Vehicles)
                    {
                        foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objArmor.InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, null));
                        }

                        foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objArmor.InternalId))
                            {
                                lstNodesToRemoveIds.Add(objWeapon.InternalId);
                                lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, objVehicleMod));
                            }
                        }
                    }
                    foreach (Tuple<Weapon, Vehicle, VehicleMod> objLoopTuple in lstWeaponsToDelete)
                    {
                        Weapon objDeleteWeapon = objLoopTuple.Item1;
                        decReturn += objDeleteWeapon.TotalCost + DeleteWeapon(objCharacter, objDeleteWeapon, treWeapons, treVehicles);
                        if (objDeleteWeapon.Parent != null)
                            objDeleteWeapon.Parent.Children.Remove(objDeleteWeapon);
                        else if (objLoopTuple.Item3 != null)
                            objLoopTuple.Item3.Weapons.Remove(objDeleteWeapon);
                        else if (objLoopTuple.Item2 != null)
                            objLoopTuple.Item2.Weapons.Remove(objDeleteWeapon);
                        else
                            objCharacter.Weapons.Remove(objDeleteWeapon);
                    }
                    foreach (string strNodeId in lstNodesToRemoveIds)
                    {
                        // Remove the Weapons from the TreeView.
                        TreeNode objLoopNode = FindNode(strNodeId, treWeapons) ?? FindNode(strNodeId, treVehicles);
                        objLoopNode?.Remove();
                    }
                }

                objCharacter.Armor.Remove(objArmor);
            }
            else if (objSelectedNode.Level == 2)
            {
                ArmorMod objMod = FindArmorMod(objSelectedNode.Tag.ToString(), objCharacter.Armor);
                if (objMod != null)
                {
                    // Remove the Cyberweapon created by the Mod if applicable.
                    if (objMod.WeaponID != Guid.Empty.ToString())
                    {
                        List<string> lstNodesToRemoveIds = new List<string>();
                        List<Tuple<Weapon, Vehicle, VehicleMod>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod>>();
                        foreach (Weapon objWeapon in objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objMod.InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, null, null));
                        }
                        foreach (Vehicle objVehicle in objCharacter.Vehicles)
                        {
                            foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objMod.InternalId))
                            {
                                lstNodesToRemoveIds.Add(objWeapon.InternalId);
                                lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, null));
                            }

                            foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                            {
                                foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objMod.InternalId))
                                {
                                    lstNodesToRemoveIds.Add(objWeapon.InternalId);
                                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod>(objWeapon, objVehicle, objVehicleMod));
                                }
                            }
                        }
                        foreach (Tuple<Weapon, Vehicle, VehicleMod> objLoopTuple in lstWeaponsToDelete)
                        {
                            Weapon objDeleteWeapon = objLoopTuple.Item1;
                            decReturn += objDeleteWeapon.TotalCost + DeleteWeapon(objCharacter, objDeleteWeapon, treWeapons, treVehicles);
                            if (objDeleteWeapon.Parent != null)
                                objDeleteWeapon.Parent.Children.Remove(objDeleteWeapon);
                            else if (objLoopTuple.Item3 != null)
                                objLoopTuple.Item3.Weapons.Remove(objDeleteWeapon);
                            else if (objLoopTuple.Item2 != null)
                                objLoopTuple.Item2.Weapons.Remove(objDeleteWeapon);
                            else
                                objCharacter.Weapons.Remove(objDeleteWeapon);
                        }
                        foreach (string strNodeId in lstNodesToRemoveIds)
                        {
                            // Remove the Weapons from the TreeView.
                            TreeNode objLoopNode = FindNode(strNodeId, treWeapons) ?? FindNode(strNodeId, treVehicles);
                            objLoopNode?.Remove();
                        }
                    }

                    // Remove any Improvements created by the ArmorMod.
                    ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                    objMod.Parent.ArmorMods.Remove(objMod);
                }
                else
                {
                    Armor objSelectedArmor;
                    ArmorMod objSelectedArmorMod;
                    Gear objGear = FindArmorGear(objSelectedNode.Tag.ToString(), objCharacter.Armor, out objSelectedArmor, out objSelectedArmorMod);
                    if (objGear != null)
                    {
                        Gear objGearParent = objGear.Parent;
                        if (objGearParent != null)
                            objGearParent.Children.Remove(objGear);
                        else if (objSelectedArmorMod != null)
                            objSelectedArmorMod.Gear.Remove(objGear);
                        else
                            objSelectedArmor.Gear.Remove(objGear);
                        decReturn += DeleteGear(objCharacter, objGear, treWeapons, treVehicles);
                    }
                }
            }
            else if (objSelectedNode.Level > 2)
            {
                Armor objSelectedArmor;
                ArmorMod objSelectedArmorMod;
                Gear objGear = FindArmorGear(objSelectedNode.Tag.ToString(), objCharacter.Armor, out objSelectedArmor, out objSelectedArmorMod);
                if (objGear != null)
                {
                    Gear objGearParent = objGear.Parent;
                    if (objGearParent != null)
                        objGearParent.Children.Remove(objGear);
                    else if (objSelectedArmorMod != null)
                        objSelectedArmorMod.Gear.Remove(objGear);
                    else
                        objSelectedArmor.Gear.Remove(objGear);
                    Commlink objCommlink = objGearParent as Commlink;
                    if (objCommlink?.CanSwapAttributes == true)
                    {
                        objCommlink.RefreshCyberdeckArray();
                    }
                    decReturn += DeleteGear(objCharacter, objGear, treWeapons, treVehicles);
                }
            }
            objSelectedNode.Remove();
            return decReturn;
        }

        /// <summary>
        /// Verify that the user wants to delete an item.
        /// </summary>
        public static bool ConfirmDelete(Character objCharacter, string strMessage)
        {
            return !objCharacter.Options.ConfirmDelete ||
                   MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_Delete"),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
        #endregion

        #region Add Improvements Functions
        public static void ReaddGearImprovements(Character objCharacter, Gear objGear, TreeView treGears, ref string strOutdatedItems, List<string> lstInternalIdFilter, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Gear, bool blnStackEquipped = true)
        {
            // We're only re-apply improvements a list of items, not all of them
            if (lstInternalIdFilter != null && !lstInternalIdFilter.Contains(objGear.InternalId))
                return;
            XmlNode objNode = objGear.MyXmlNode;
            if (objNode != null)
            {
                if (objGear.Category == "Stacked Focus")
                {
                    StackedFocus objStack = objCharacter.StackedFoci.FirstOrDefault(x => x.GearId == objGear.InternalId);
                    if (objStack != null)
                    {
                        foreach (Gear objFociGear in objStack.Gear)
                        {
                            ReaddGearImprovements(objCharacter, objFociGear, treGears, ref strOutdatedItems, lstInternalIdFilter, Improvement.ImprovementSource.StackedFocus, blnStackEquipped);
                        }
                    }
                }
                objGear.Bonus = objNode["bonus"];
                objGear.WirelessBonus = objNode["wirelessbonus"];
                if (blnStackEquipped && objGear.Equipped)
                {
                    if (objGear.Bonus != null)
                    {
                        ImprovementManager.ForcedValue = objGear.Extra;
                        ImprovementManager.CreateImprovements(objCharacter, eSource, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort);
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objGear.Extra = ImprovementManager.SelectedValue;
                            TreeNode objGearNode = FindNode(objGear.InternalId, treGears);
                            if (objGearNode != null)
                                objGearNode.Text = objGear.DisplayName;
                        }
                    }
                    if (objGear.WirelessOn && objGear.WirelessBonus != null)
                    {
                        ImprovementManager.ForcedValue = objGear.Extra;
                        ImprovementManager.CreateImprovements(objCharacter, eSource, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort);
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objGear.Extra = ImprovementManager.SelectedValue;
                            TreeNode objGearNode = FindNode(objGear.InternalId, treGears);
                            if (objGearNode != null)
                                objGearNode.Text = objGear.DisplayName;
                        }
                    }
                }
                
            }
            else
            {
                strOutdatedItems += objGear.DisplayName + "\n";
            }
            foreach (Gear objChild in objGear.Children)
                ReaddGearImprovements(objCharacter, objChild, treGears, ref strOutdatedItems, lstInternalIdFilter, eSource, blnStackEquipped);
        }
        #endregion

        #region Tree Functions


        /// <summary>
        /// Build up the Tree for the current piece of Gear and all of its children.
        /// </summary>
        /// <param name="objGear">Gear to iterate through.</param>
        /// <param name="objNode">TreeNode to append to.</param>
        /// <param name="objMenu">ContextMenuStrip that the new TreeNodes should use.</param>
        public static void BuildGearTree(Gear objGear, TreeNode objNode, ContextMenuStrip objMenu)
        {
            bool blnExpandNode = false;
            foreach (Gear objChild in objGear.Children)
            {
                TreeNode objChildNode = new TreeNode();
                objChildNode.Text = objChild.DisplayName;
                objChildNode.Tag = objChild.InternalId;
                objChildNode.ContextMenuStrip = objMenu;
                if (!string.IsNullOrEmpty(objChild.Notes))
                    objChildNode.ForeColor = Color.SaddleBrown;
                else if (objChild.IncludedInParent)
                    objChildNode.ForeColor = SystemColors.GrayText;
                objChildNode.ToolTipText = objChild.Notes;

                objNode.Nodes.Add(objChildNode);
                if (objChild.ParentID != objGear.InternalId || (objGear.MyXmlNode?["gears"]?.Attributes?["startcollapsed"]?.InnerText != "yes"))
                    blnExpandNode = true;

                BuildGearTree(objChild, objChildNode, objMenu);
            }
            if (blnExpandNode)
                objNode.Expand();
        }

        /// <summary>
        /// Build up the Tree for the current piece of Cyberware and all of its children.
        /// </summary>
        /// <param name="objCyberware">Cyberware to iterate through.</param>
        /// <param name="objParentNode">TreeNode to append to.</param>
        /// <param name="objMenu">ContextMenuStrip that the new Cyberware TreeNodes should use.</param>
        /// <param name="objGearMenu">ContextMenuStrip that the new Gear TreeNodes should use.</param>
        public static void BuildCyberwareTree(Cyberware objCyberware, TreeNode objParentNode, ContextMenuStrip objMenu, ContextMenuStrip objGearMenu)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = objCyberware.DisplayName;
            objNode.Tag = objCyberware.InternalId;
            if (!string.IsNullOrEmpty(objCyberware.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (!string.IsNullOrEmpty(objCyberware.ParentID))
                objNode.ForeColor = SystemColors.GrayText;
            objNode.ToolTipText = objCyberware.Notes;
            objNode.ContextMenuStrip = objMenu;

            objParentNode.Nodes.Add(objNode);
            objParentNode.Expand();

            foreach (Cyberware objChild in objCyberware.Children)
                BuildCyberwareTree(objChild, objNode, objMenu, objGearMenu);

            foreach (Gear objGear in objCyberware.Gear)
            {
                TreeNode objGearNode = new TreeNode();
                objGearNode.Text = objGear.DisplayName;
                objGearNode.Tag = objGear.InternalId;
                if (!string.IsNullOrEmpty(objGear.Notes))
                    objGearNode.ForeColor = Color.SaddleBrown;
                else if (objGear.IncludedInParent)
                    objGearNode.ForeColor = SystemColors.GrayText;
                objGearNode.ToolTipText = objGear.Notes;
                objGearNode.ContextMenuStrip = objGearMenu;

                BuildGearTree(objGear, objGearNode, objGearMenu);

                objNode.Nodes.Add(objGearNode);
                objNode.Expand();
            }

        }

        #endregion

        #region TreeNode Creation Methods
        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        /// <param name="objArmor">Armor to add.</param>
        /// <param name="treArmor">Armor TreeView.</param>
        /// <param name="cmsArmor">ContextMenuStrip for the Armor Node.</param>
        /// <param name="cmsArmorMod">ContextMenuStrip for Armor Mod Nodes.</param>
        /// <param name="cmsArmorGear">ContextMenuStrip for Armor Gear Nodes.</param>
        public static void CreateArmorTreeNode(Armor objArmor, TreeView treArmor, ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = objArmor.DisplayName;
            objNode.Tag = objArmor.InternalId;
            if (!string.IsNullOrEmpty(objArmor.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            objNode.ToolTipText = objArmor.Notes;

            foreach (ArmorMod objMod in objArmor.ArmorMods)
            {
                TreeNode objChild = new TreeNode();
                objChild.Text = objMod.DisplayName;
                objChild.Tag = objMod.InternalId;
                objChild.ContextMenuStrip = string.IsNullOrEmpty(objMod.GearCapacity) ? cmsArmorMod : cmsArmorGear;
                if (!string.IsNullOrEmpty(objMod.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                else if (objMod.IncludedInArmor)
                    objChild.ForeColor = SystemColors.GrayText;
                objChild.ToolTipText = objMod.Notes;
                foreach (Gear objGear in objMod.Gear)
                {
                    TreeNode objChildGear = new TreeNode();
                    objChildGear.Text = objGear.DisplayName;
                    objChildGear.Tag = objGear.InternalId;
                    if (!string.IsNullOrEmpty(objGear.Notes))
                        objChildGear.ForeColor = Color.SaddleBrown;
                    else if (objGear.IncludedInParent)
                        objChildGear.ForeColor = SystemColors.GrayText;
                    objChildGear.ToolTipText = objGear.Notes;

                    BuildGearTree(objGear, objChildGear, cmsArmorGear);

                    objChildGear.ContextMenuStrip = cmsArmorGear;
                    objChild.Nodes.Add(objChildGear);
                    objChild.Expand();
                }
                objNode.Nodes.Add(objChild);
                objNode.Expand();
            }

            foreach (Gear objGear in objArmor.Gear)
            {
                TreeNode objChild = new TreeNode();
                objChild.Text = objGear.DisplayName;
                objChild.Tag = objGear.InternalId;
                if (!string.IsNullOrEmpty(objGear.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                else if (objGear.IncludedInParent)
                    objChild.ForeColor = SystemColors.GrayText;
                objChild.ToolTipText = objGear.Notes;

                BuildGearTree(objGear, objChild, cmsArmorGear);

                objChild.ContextMenuStrip = cmsArmorGear;
                objNode.Nodes.Add(objChild);
                objNode.Expand();
            }

            TreeNode objParent = new TreeNode();
            if (string.IsNullOrEmpty(objArmor.Location))
                objParent = treArmor.Nodes[0];
            else
            {
                foreach (TreeNode objFind in treArmor.Nodes)
                {
                    if (objFind.Text == objArmor.Location)
                    {
                        objParent = objFind;
                        break;
                    }
                }
            }

            objNode.ContextMenuStrip = cmsArmor;
            objParent.Nodes.Add(objNode);
            objParent.Expand();
        }

        /// <summary>
        /// Add a Vehicle to the TreeView.
        /// </summary>
        /// <param name="objVehicle">Vehicle to add.</param>
        /// <param name="treVehicles">Vehicle TreeView.</param>
        /// <param name="cmsVehicle">ContextMenuStrip for the Vehicle Node.</param>
        /// <param name="cmsVehicleLocation">ContextMenuStrip for Vehicle Location Nodes.</param>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip for Vehicle Weapon Nodes.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Weapon Accessory Nodes.</param>
        /// <param name="cmsWeaponAccessoryGear"></param>
        /// <param name="cmsVehicleGear">ContextMenuStrip for Vehicle Gear Nodes.</param>
        /// <param name="cmsVehicleWeaponMount">ContextMenuStrip for Vehicle Weapon Mounts.</param>
        public static void CreateVehicleTreeNode(Vehicle objVehicle, TreeView treVehicles, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = objVehicle.DisplayName;
            objNode.Tag = objVehicle.InternalId;
            if (!string.IsNullOrEmpty(objVehicle.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (!string.IsNullOrEmpty(objVehicle.ParentID))
                objNode.ForeColor = SystemColors.GrayText;
            objNode.ToolTipText = objVehicle.Notes;

            // Populate the list of Vehicle Locations.
            foreach (string strLocation in objVehicle.Locations)
            {
                TreeNode objLocation = new TreeNode();
                objLocation.Tag = strLocation;
                objLocation.Text = strLocation;
                objLocation.ContextMenuStrip = cmsVehicleLocation;
                objNode.Nodes.Add(objLocation);
            }

            // VehicleMods.
            foreach (VehicleMod objMod in objVehicle.Mods)
            {
                TreeNode objChildNode = new TreeNode();
                objChildNode.Text = objMod.DisplayName;
                objChildNode.Tag = objMod.InternalId;
                if (!string.IsNullOrEmpty(objMod.Notes))
                    objChildNode.ForeColor = Color.SaddleBrown;
                else if (objMod.IncludedInVehicle)
                    objChildNode.ForeColor = SystemColors.GrayText;
                objChildNode.ToolTipText = objMod.Notes;

                // Cyberware.
                foreach (Cyberware objCyberware in objMod.Cyberware)
                {
                    TreeNode objCyberwareNode = new TreeNode();
                    objCyberwareNode.Text = objCyberware.DisplayName;
                    objCyberwareNode.Tag = objCyberware.InternalId;
                    if (!string.IsNullOrEmpty(objCyberware.Notes))
                        objCyberwareNode.ForeColor = Color.SaddleBrown;
                    else if (!string.IsNullOrEmpty(objCyberware.ParentID))
                        objCyberwareNode.ForeColor = SystemColors.GrayText;
                    objCyberwareNode.ToolTipText = objCyberware.Notes;
                    objChildNode.Nodes.Add(objCyberwareNode);
                    objChildNode.Expand();
                }

                // VehicleWeapons.
                foreach (Weapon objWeapon in objMod.Weapons)
                    CreateWeaponTreeNode(objWeapon, objChildNode, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);

                // Attach the ContextMenuStrip.
                objChildNode.ContextMenuStrip = cmsVehicle;

                objNode.Nodes.Add(objChildNode);
                objNode.Expand();
            }
            if (objVehicle.WeaponMounts.Count > 0)
            {
                TreeNode mountsNode = new TreeNode();
                mountsNode.Tag = "String_WeaponMounts";
                mountsNode.Text = LanguageManager.GetString("String_WeaponMounts");
                objNode.Nodes.Add(mountsNode);
                // Weapon Mounts
                foreach (WeaponMount wm in objVehicle.WeaponMounts)
                    CreateWeaponMountTreeNode(wm, mountsNode, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, cmsVehicleWeaponMount);
            }
            // Vehicle Weapons (not attached to a mount).
            foreach (Weapon objWeapon in objVehicle.Weapons)
                CreateWeaponTreeNode(objWeapon, objNode, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);

            // Vehicle Gear.
            foreach (Gear objGear in objVehicle.Gear)
            {
                TreeNode objGearNode = new TreeNode();
                objGearNode.Text = objGear.DisplayName;
                objGearNode.Tag = objGear.InternalId;
                if (!string.IsNullOrEmpty(objGear.Notes))
                    objGearNode.ForeColor = Color.SaddleBrown;
                else if (objGear.IncludedInParent)
                    objGearNode.ForeColor = SystemColors.GrayText;
                objGearNode.ToolTipText = objGear.Notes;

                BuildGearTree(objGear, objGearNode, cmsVehicleGear);

                objGearNode.ContextMenuStrip = cmsVehicleGear;

                TreeNode objParent = new TreeNode();
                if (string.IsNullOrEmpty(objGear.Location))
                    objParent = objNode;
                else
                {
                    foreach (TreeNode objFind in objNode.Nodes)
                    {
                        if (objFind.Text == objGear.Location)
                        {
                            objParent = objFind;
                            break;
                        }
                    }
                }

                objParent.Nodes.Add(objGearNode);
                objParent.Expand();
            }

            objNode.ContextMenuStrip = cmsVehicle;
            treVehicles.Nodes[0].Nodes.Add(objNode);
            treVehicles.Nodes[0].Expand();
        }

        /// <summary>
        /// Add a Weapon Mount to the TreeView
        /// </summary>
        /// <param name="wm">WeaponMount that we're creating.</param>
        /// <param name="parentNode">Parent treenode to add to.</param>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip for Vehicle Weapons</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Weapon Accessories</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Gear</param>
        /// <param name="cmsVehicleWeaponMount">ContextMenuStrip for Vehicle Weapon Mounts</param>
        public static void CreateWeaponMountTreeNode(WeaponMount wm, TreeNode parentNode, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsVehicleWeaponMount)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = wm.DisplayName;
            objNode.Tag = wm.InternalId;
            objNode.ContextMenuStrip = cmsVehicleWeaponMount;
            if (!string.IsNullOrEmpty(wm.Notes))
            {
                objNode.ToolTipText = wm.Notes;
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (wm.IncludedInVehicle)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            foreach (Weapon w in wm.Weapons)
            {
                CreateWeaponTreeNode(w, objNode, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
            }
            parentNode.Nodes.Add(objNode);
        }

        /// <summary>
        /// Add a Weapon to the TreeView.
        /// </summary>
        /// <param name="objWeapon">Weapon to add.</param>
        /// <param name="objWeaponsNode">Node to append the Weapon Node to.</param>
        /// <param name="cmsWeapon">ContextMenuStrip for the Weapon Node.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Accessory Nodes.</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Accessory Gear Nodes.</param>
        /// <param name="WeaponID">The weapon </param>
        public static void CreateWeaponTreeNode(Weapon objWeapon, TreeNode objWeaponsNode, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, string WeaponID = null)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = objWeapon.DisplayName;
            objNode.Tag = WeaponID ?? objWeapon.InternalId;
            if (!string.IsNullOrEmpty(objWeapon.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || WeaponID != null || !string.IsNullOrEmpty(objWeapon.ParentID))
                objNode.ForeColor = SystemColors.GrayText;

            objNode.ToolTipText = objWeapon.Notes;

            // Add attached Weapon Accessories.
            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
            {
                TreeNode objChild = new TreeNode();
                objChild.Text = objAccessory.DisplayName;
                objChild.Tag = objAccessory.InternalId;
                objChild.ContextMenuStrip = cmsWeaponAccessory;
                if (!string.IsNullOrEmpty(objAccessory.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                else if (objAccessory.IncludedInWeapon)
                    objChild.ForeColor = SystemColors.GrayText;
                objChild.ToolTipText = objAccessory.Notes;

                // Add any Gear attached to the Weapon Accessory.
                foreach (Gear objGear in objAccessory.Gear)
                {
                    TreeNode objGearChild = new TreeNode();
                    objGearChild.Text = objGear.DisplayName;
                    objGearChild.Tag = objGear.InternalId;
                    if (!string.IsNullOrEmpty(objGear.Notes))
                        objGearChild.ForeColor = Color.SaddleBrown;
                    else if (objGear.IncludedInParent)
                        objGearChild.ForeColor = SystemColors.GrayText;
                    objGearChild.ToolTipText = objGear.Notes;

                    BuildGearTree(objGear, objGearChild, cmsWeaponAccessoryGear);

                    objGearChild.ContextMenuStrip = cmsWeaponAccessoryGear;
                    objChild.Nodes.Add(objGearChild);
                    objChild.Expand();
                }

                objNode.Nodes.Add(objChild);
                objNode.Expand();
            }

            // Add Underbarrel Weapons.
            if (objWeapon.UnderbarrelWeapons.Count > 0)
            {
                foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                    CreateWeaponTreeNode(objUnderbarrelWeapon, objNode, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
            }

            // If this is not an Underbarrel Weapon and it has a Location, find the Location Node that this should be attached to instead.
            if (!objWeapon.IsUnderbarrelWeapon && !string.IsNullOrEmpty(objWeapon.Location))
            {
                foreach (TreeNode objLocationNode in objWeaponsNode.TreeView.Nodes)
                {
                    if (objLocationNode.Text == objWeapon.Location)
                    {
                        objWeaponsNode = objLocationNode;
                        break;
                    }
                }
            }

            objNode.ContextMenuStrip = cmsWeapon;
            objWeaponsNode.Nodes.Add(objNode);
            objWeaponsNode.Expand();
        }
        #endregion

        #region Move TreeNodes
        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop, changing its parent.
        /// </summary>
        /// <param name="intNewIndex">Node's new idnex.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveGearParent(Character objCharacter, TreeNode objDestination, TreeView treGear, ContextMenuStrip cmsGear)
        {
            // The item cannot be dropped onto itself.
            if (objDestination == treGear.SelectedNode)
                return;
            // The item cannot be dropped onto one of its children.
            foreach (TreeNode objNode in treGear.SelectedNode.Nodes)
            {
                if (objNode == objDestination)
                    return;
            }

            // Locate the currently selected piece of Gear.
            Gear objGear = DeepFindById(treGear.SelectedNode.Tag.ToString(), objCharacter.Gear);

            // Gear cannot be moved to one if its children.
            bool blnAllowMove = true;
            TreeNode objFindNode = objDestination;
            if (objDestination.Level > 0)
            {
                do
                {
                    objFindNode = objFindNode.Parent;
                    if (objFindNode.Tag.ToString() == objGear.InternalId)
                    {
                        blnAllowMove = false;
                        break;
                    }
                } while (objFindNode.Level > 0);
            }

            if (!blnAllowMove)
                return;

            // Remove the Gear from the character.
            if (objGear.Parent == null)
                objCharacter.Gear.Remove(objGear);
            else
            {
                objGear.Parent.Children.Remove(objGear);
                Commlink objCommlink = objGear.Parent as Commlink;
                if (objCommlink?.CanSwapAttributes == true)
                {
                    objCommlink.RefreshCyberdeckArray();
                }
            }

            if (objDestination.Level == 0)
            {
                // The Gear was moved to a location, so add it to the character instead.
                objCharacter.Gear.Add(objGear);
                objGear.Location = objDestination.Text;
                objGear.Parent = null;
            }
            else
            {
                // Locate the Gear that the item was dropped on.
                Gear objParent = DeepFindById(objDestination.Tag.ToString(), objCharacter.Gear);

                // Add the Gear as a child of the destination Node and clear its location.
                objParent.Children.Add(objGear);
                objGear.Location = string.Empty;
                objGear.Parent = objParent;
                Commlink objCommlink = objParent as Commlink;
                if (objCommlink?.CanSwapAttributes == true)
                {
                    objCommlink.RefreshCyberdeckArray();
                }
            }

            TreeNode objClone = treGear.SelectedNode;
            objClone.ContextMenuStrip = cmsGear;

            // Remove the current Node.
            treGear.SelectedNode.Remove();

            // Add the new Node to the new parent.
            objDestination.Nodes.Add(objClone);
            objDestination.Expand();
        }

        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveGearNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treGear)
        {
            Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.InternalId == treGear.SelectedNode.Tag.ToString());
            objCharacter.Gear.Remove(objGear);
            if (intNewIndex > objCharacter.Gear.Count)
                objCharacter.Gear.Add(objGear);
            else
                objCharacter.Gear.Insert(intNewIndex, objGear);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            TreeNode objOldParent = treGear.SelectedNode;
            while (objOldParent.Level > 0)
                objOldParent = objOldParent.Parent;

            // Change the Location on the Gear item.
            if (objNewParent.Text == LanguageManager.GetString("Node_SelectedGear"))
                objGear.Location = string.Empty;
            else
                objGear.Location = objNewParent.Text;

            TreeNode objClone = treGear.SelectedNode;

            objOldParent.Nodes.Remove(treGear.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move a Gear Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveGearRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treGear)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = string.Empty;
            // Locate the currently selected Location.
            foreach (string strCharacterLocation in objCharacter.GearLocations)
            {
                if (strCharacterLocation == treGear.SelectedNode.Tag.ToString())
                {
                    strLocation = strCharacterLocation;
                    break;
                }
            }
            objCharacter.GearLocations.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.GearLocations.Count)
                objCharacter.GearLocations.Add(strLocation);
            else
                objCharacter.GearLocations.Insert(intNewIndex - 1, strLocation);

            TreeNode nodOldNode = treGear.SelectedNode;
            treGear.Nodes.Remove(nodOldNode);
            treGear.Nodes.Insert(intNewIndex, nodOldNode);
        }

        /// <summary>
        /// Move a Lifestyle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveLifestyleNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treLifestyles)
        {
            Lifestyle objLifestyle = objCharacter.Lifestyles.FirstOrDefault(x => x.Name == treLifestyles.SelectedNode.Tag.ToString());
            objCharacter.Lifestyles.Remove(objLifestyle);
            if (intNewIndex > objCharacter.Lifestyles.Count)
                objCharacter.Lifestyles.Add(objLifestyle);
            else
                objCharacter.Lifestyles.Insert(intNewIndex, objLifestyle);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            TreeNode objOldParent = treLifestyles.SelectedNode;
            while (objOldParent.Level > 0)
                objOldParent = objOldParent.Parent;

            TreeNode objClone = treLifestyles.SelectedNode;

            objOldParent.Nodes.Remove(treLifestyles.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move an Armor TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveArmorNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treArmor)
        {
            // Locate the currently selected Armor.
            Armor objArmor = FindByIdWithNameCheck(treArmor.SelectedNode.Tag.ToString(), objCharacter.Armor);

            objCharacter.Armor.Remove(objArmor);
            if (intNewIndex > objCharacter.Armor.Count)
                objCharacter.Armor.Add(objArmor);
            else
                objCharacter.Armor.Insert(intNewIndex, objArmor);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            TreeNode objOldParent = treArmor.SelectedNode;
            while (objOldParent.Level > 0)
                objOldParent = objOldParent.Parent;

            // Change the Location on the Armor item.
            if (objNewParent.Text == LanguageManager.GetString("Node_SelectedArmor"))
                objArmor.Location = string.Empty;
            else
                objArmor.Location = objNewParent.Text;

            TreeNode objClone = treArmor.SelectedNode;

            objOldParent.Nodes.Remove(treArmor.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move an Armor Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveArmorRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treArmor)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = string.Empty;
            // Locate the currently selected Location.
            foreach (string strCharacterLocation in objCharacter.ArmorLocations)
            {
                if (strCharacterLocation == treArmor.SelectedNode.Tag.ToString())
                {
                    strLocation = strCharacterLocation;
                    break;
                }
            }
            objCharacter.ArmorLocations.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.ArmorLocations.Count)
                objCharacter.ArmorLocations.Add(strLocation);
            else
                objCharacter.ArmorLocations.Insert(intNewIndex - 1, strLocation);

            TreeNode nodOldNode = treArmor.SelectedNode;
            treArmor.Nodes.Remove(nodOldNode);
            treArmor.Nodes.Insert(intNewIndex, nodOldNode);
        }

        /// <summary>
        /// Move a Weapon TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveWeaponNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treWeapons)
        {
            Weapon objWeapon = objCharacter.Weapons.FirstOrDefault(x => x.InternalId == treWeapons.SelectedNode.Tag.ToString());
            objCharacter.Weapons.Remove(objWeapon);
            if (intNewIndex > objCharacter.Weapons.Count)
                objCharacter.Weapons.Add(objWeapon);
            else
                objCharacter.Weapons.Insert(intNewIndex, objWeapon);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            TreeNode objOldParent = treWeapons.SelectedNode;
            while (objOldParent.Level > 0)
                objOldParent = objOldParent.Parent;

            // Change the Location of the Weapon.
            if (objNewParent.Text == LanguageManager.GetString("Node_SelectedWeapons"))
                objWeapon.Location = string.Empty;
            else
                objWeapon.Location = objNewParent.Text;

            TreeNode objClone = treWeapons.SelectedNode;

            objOldParent.Nodes.Remove(treWeapons.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move a Weapon Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveWeaponRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treWeapons)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = string.Empty;
            // Locate the currently selected Location.
            foreach (string strCharacterLocation in objCharacter.WeaponLocations)
            {
                if (strCharacterLocation == treWeapons.SelectedNode.Tag.ToString())
                {
                    strLocation = strCharacterLocation;
                    break;
                }
            }
            objCharacter.GearLocations.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.WeaponLocations.Count)
                objCharacter.WeaponLocations.Add(strLocation);
            else
                objCharacter.WeaponLocations.Insert(intNewIndex - 1, strLocation);

            TreeNode nodOldNode = treWeapons.SelectedNode;
            treWeapons.Nodes.Remove(nodOldNode);
            treWeapons.Nodes.Insert(intNewIndex, nodOldNode);
        }

        /// <summary>
        /// Move a Cyberware TreeNode after Drag and Drop or changing mount.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="lstNewList">New list to which the Cyberware is being moved.</param>
        /// <param name="objDestination">New parent node.</param>
        /// <param name="treOldTreeView">Old tree view from which we are moving the Cyberware.</param>
        public static void MoveCyberwareNode(Character objCharacter, int intNewIndex, IList<Cyberware> lstNewList, TreeNode objDestination, TreeView treOldTreeView)
        {
            TreeNode objCyberwareNode = treOldTreeView.SelectedNode;
            Cyberware objCyberware = DeepFindById(objCyberwareNode.Tag.ToString(), objCharacter.Cyberware);
            VehicleMod objOldParentVehicleMod = null;
            if (objCyberware == null)
            {
                objCyberware = FindVehicleCyberware(objCyberwareNode.Tag.ToString(), objCharacter.Vehicles, out objOldParentVehicleMod);
            }
            Cyberware objOldParentCyberware = objCyberware.Parent;
            if (objOldParentCyberware != null)
                objOldParentCyberware.Children.Remove(objCyberware);
            else if (objOldParentVehicleMod != null)
                objOldParentVehicleMod.Cyberware.Remove(objCyberware);
            else
                objCharacter.Cyberware.Remove(objCyberware);

            if (intNewIndex > lstNewList.Count)
                lstNewList.Add(objCyberware);
            else
                lstNewList.Insert(intNewIndex, objCyberware);

            TreeNode objNewParent = objDestination;

            TreeNode objOldParent = treOldTreeView.SelectedNode.Parent;

            objOldParent.Nodes.Remove(treOldTreeView.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objCyberwareNode);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move a Vehicle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveVehicleNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treVehicles)
        {
            Vehicle objVehicle = objCharacter.Vehicles.FirstOrDefault(x => x.InternalId == treVehicles.SelectedNode.Tag.ToString());
            objCharacter.Vehicles.Remove(objVehicle);
            if (intNewIndex > objCharacter.Vehicles.Count)
                objCharacter.Vehicles.Add(objVehicle);
            else
                objCharacter.Vehicles.Insert(intNewIndex, objVehicle);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            TreeNode objOldParent = treVehicles.SelectedNode;
            while (objOldParent.Level > 0)
                objOldParent = objOldParent.Parent;

            TreeNode objClone = treVehicles.SelectedNode;

            objOldParent.Nodes.Remove(treVehicles.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move a Vehicle Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveVehicleGearParent(Character objCharacter, TreeNode objDestination, TreeView treVehicles, ContextMenuStrip cmsVehicleGear)
        {
            // The item cannot be dropped onto itself.
            if (objDestination == treVehicles.SelectedNode)
                return;
            // The item cannot be dropped onton one of its children.
            foreach (TreeNode objNode in treVehicles.SelectedNode.Nodes)
            {
                if (objNode == objDestination)
                    return;
            }

            // Determine if this is a Location.
            TreeNode objVehicleNode = objDestination;
            do
            {
                objVehicleNode = objVehicleNode.Parent;
            } while (objVehicleNode.Level > 1);

            // Get a reference to the destination Vehicle.
            Vehicle objDestinationVehicle = FindById(objVehicleNode.Tag.ToString(), objCharacter.Vehicles);

            // Make sure the destination is another piece of Gear or a Location.
            bool blnDestinationGear = true;
            bool blnDestinationLocation = false;
            Gear objDestinationGear = FindVehicleGear(objDestination.Tag.ToString(), objCharacter.Vehicles);
            if (objDestinationGear == null)
                blnDestinationGear = false;

            // Determine if this is a Location in the destination Vehicle.
            string strDestinationLocation = string.Empty;
            foreach (string strLocation in objDestinationVehicle.Locations)
            {
                if (strLocation == objDestination.Tag.ToString())
                {
                    strDestinationLocation = strLocation;
                    blnDestinationLocation = true;
                    break;
                }
            }

            if (!blnDestinationLocation && !blnDestinationGear)
                return;

            // Locate the currently selected piece of Gear.
            Vehicle objVehicle = null;
            WeaponAccessory objWeaponAccessory = null;
            Cyberware objCyberware = null;
            Gear objGear = FindVehicleGear(treVehicles.SelectedNode.Tag.ToString(), objCharacter.Vehicles, out objVehicle, out objWeaponAccessory, out objCyberware);

            // Gear cannot be moved to one of its children.
            bool blnAllowMove = true;
            TreeNode objFindNode = objDestination;
            if (objDestination.Level > 0)
            {
                do
                {
                    objFindNode = objFindNode.Parent;
                    if (objFindNode.Tag.ToString() == objGear.InternalId)
                    {
                        blnAllowMove = false;
                        break;
                    }
                } while (objFindNode.Level > 0);
            }

            if (!blnAllowMove)
                return;

            // Remove the Gear from the Vehicle.
            if (objGear.Parent == null)
            {
                if (objCyberware != null)
                    objCyberware.Gear.Remove(objGear);
                else if (objWeaponAccessory != null)
                    objWeaponAccessory.Gear.Remove(objGear);
                else
                    objVehicle.Gear.Remove(objGear);
            }
            else
            {
                objGear.Parent.Children.Remove(objGear);
                Commlink objCommlink = objGear.Parent as Commlink;
                if (objCommlink?.CanSwapAttributes == true)
                {
                    objCommlink.RefreshCyberdeckArray();
                }
            }

            if (blnDestinationLocation)
            {
                // Add the Gear to the Vehicle and set its Location.
                objDestinationVehicle.Gear.Add(objGear);
                objGear.Location = strDestinationLocation;
                objGear.Parent = null;
            }
            else
            {
                // Add the Gear to its new parent.
                objDestinationGear.Children.Add(objGear);
                objGear.Location = string.Empty;
                objGear.Parent = objDestinationGear;
                Commlink objCommlink = objDestinationGear as Commlink;
                if (objCommlink?.CanSwapAttributes == true)
                {
                    objCommlink.RefreshCyberdeckArray();
                }
            }

            TreeNode objClone = treVehicles.SelectedNode;
            objClone.ContextMenuStrip = cmsVehicleGear;

            // Remove the current Node.
            treVehicles.SelectedNode.Remove();

            // Add the new Node to its parent.
            objDestination.Nodes.Add(objClone);
            objDestination.Expand();
        }

        /// <summary>
        /// Move an Improvement TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveImprovementNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treImprovements)
        {
            Improvement objImprovement = objCharacter.Improvements.FirstOrDefault(x => x.SourceName == treImprovements.SelectedNode.Tag.ToString());

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            TreeNode objOldParent = treImprovements.SelectedNode;
            while (objOldParent.Level > 0)
                objOldParent = objOldParent.Parent;

            // Change the Group on the Custom Improvement.
            objImprovement.CustomGroup = objNewParent.Text;

            TreeNode objClone = treImprovements.SelectedNode;

            objOldParent.Nodes.Remove(treImprovements.SelectedNode);
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();

            // Change the sort order for all of the Improvements in the TreeView.
            foreach (TreeNode objNode in treImprovements.Nodes[0].Nodes)
            {
                foreach (Improvement objCharacterImprovement in objCharacter.Improvements)
                {
                    if (objCharacterImprovement.SourceName == objNode.Tag.ToString())
                    {
                        objCharacterImprovement.SortOrder = objNode.Index;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Move an Improvement Group TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveImprovementRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treImprovements)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = string.Empty;
            // Locate the currently selected Group.
            foreach (string strCharacterGroup in objCharacter.ImprovementGroups)
            {
                if (strCharacterGroup == treImprovements.SelectedNode.Tag.ToString())
                {
                    strLocation = strCharacterGroup;
                }
            }
            objCharacter.ImprovementGroups.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.ImprovementGroups.Count)
                objCharacter.ImprovementGroups.Add(strLocation);
            else
                objCharacter.ImprovementGroups.Insert(intNewIndex - 1, strLocation);

            TreeNode nodOldNode = treImprovements.SelectedNode;
            treImprovements.Nodes.Remove(nodOldNode);
            treImprovements.Nodes.Insert(intNewIndex, nodOldNode);
        }
        #endregion

        #region Tab clearing
        /// <summary>
        /// Clear all Spell tab elements from the character.
        /// </summary>
        /// <param name="treSpells"></param>
        public static void ClearSpellTab(Character objCharacter, TreeView treSpells)
        {
            // Run through all of the Spells and remove their Improvements.
            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.Spell, string.Empty);

            // Clear the list of Spells.
            foreach (TreeNode objNode in treSpells.Nodes)
                objNode.Nodes.Clear();

            objCharacter.Spells.Clear();
            objCharacter.Spirits.RemoveAll(x => x.EntityType == SpiritType.Spirit);

        }

        /// <summary>
        /// Clear all Adept tab elements from the character.
        /// </summary>
        public static void ClearAdeptTab(Character objCharacter)
        {
            // Run through all of the Powers and remove their Improvements.
            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.Power, string.Empty);

            objCharacter.Powers.Clear();
        }

        /// <summary>
        /// Clear all Technomancer tab elements from the character.
        /// </summary>
        public static void ClearTechnomancerTab(Character objCharacter, TreeView treComplexForms)
        {
            // Run through all of the Complex Forms and remove their Improvements.
            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.ComplexForm, string.Empty);

            // Clear the list of Complex Forms.
            foreach (TreeNode objNode in treComplexForms.Nodes)
                objNode.Nodes.Clear();

            objCharacter.Spirits.RemoveAll(x => x.EntityType == SpiritType.Sprite);
            objCharacter.ComplexForms.Clear();
        }

        /// <summary>
        /// Clear all Advanced Programs tab elements from the character.
        /// </summary>
        public static void ClearAdvancedProgramsTab(Character objCharacter, TreeView treAIPrograms)
        {
            // Run through all of the Advanced Programs and remove their Improvements.
            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.AIProgram, string.Empty);

            // Clear the list of Advanced Programs.
            foreach (TreeNode objNode in treAIPrograms.Nodes)
                objNode.Nodes.Clear();

            objCharacter.AIPrograms.Clear();
        }

        /// <summary>
        /// Clear all Cyberware tab elements from the character.
        /// </summary>
        public static void ClearCyberwareTab(Character objCharacter, TreeView treCyberware, TreeView treWeapons, TreeView treVehicles)
        {
            foreach (Cyberware objCyberware in objCharacter.Cyberware)
            {
                DeleteCyberware(objCharacter, objCyberware, treWeapons, treVehicles);
            }
            objCharacter.Cyberware.Clear();

            // Clear the list of Advanced Programs.
            // Remove the item from the TreeView.
            foreach (TreeNode objNode in treCyberware.Nodes)
                objNode.Nodes.Clear();
            treCyberware.Nodes.Clear();
        }

        /// <summary>
        /// Clear all Critter tab elements from the character.
        /// </summary>
        public static void ClearCritterTab(Character objCharacter, TreeView treCritterPowers)
        {
            // Run through all of the Critter Powers and remove their Improvements.
            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.CritterPower, string.Empty);

            // Clear the list of Critter Powers.
            foreach (TreeNode objNode in treCritterPowers.Nodes)
                objNode.Nodes.Clear();

            objCharacter.CritterPowers.Clear();
        }

        /// <summary>
        /// Clear all Initiation tab elements from the character that were not added by improvements.
        /// </summary>
        public static void ClearInitiations(Character objCharacter)
        {
            objCharacter.InitiateGrade = 0;
            objCharacter.SubmersionGrade = 0;
            objCharacter.InitiationGrades.Clear();
            // Metamagics/Echoes can add addition bonus metamagics/echoes, so we cannot use foreach or RemoveAll()
            for (int j = objCharacter.Metamagics.Count - 1; j >= 0; j--)
            {
                if (j < objCharacter.Metamagics.Count)
                {
                    Metamagic objToRemove = objCharacter.Metamagics[j];
                    if (objToRemove.Grade >= 0)
                    {
                        // Remove the Improvements created by the Metamagic.
                        ImprovementManager.RemoveImprovements(objCharacter, objToRemove.SourceType, objToRemove.InternalId);
                        objCharacter.Metamagics.Remove(objToRemove);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Populate the list of Bonded Foci.
        /// </summary>
        public static void PopulateFocusList(Character objCharacter, TreeView treFoci)
        {
            treFoci.Nodes.Clear();
            int intFociTotal = 0;
            bool blnWarned = false;

            int intMaxFocusTotal = objCharacter.MAG.TotalValue * 5;
            if (objCharacter.Options.MysAdeptSecondMAGAttribute && objCharacter.IsMysticAdept)
                intMaxFocusTotal = Math.Min(intMaxFocusTotal, objCharacter.MAGAdept.TotalValue * 5);
            foreach (Gear objGear in objCharacter.Gear.Where(objGear => objGear.Category == "Foci" || objGear.Category == "Metamagic Foci"))
            {
                List<Focus> removeFoci = new List<Focus>();
                TreeNode objNode = new TreeNode();
                objNode.Text = objGear.DisplayName.Replace(LanguageManager.GetString("String_Rating"), LanguageManager.GetString("String_Force"));
                objNode.Tag = objGear.InternalId;
                foreach (Focus objFocus in objCharacter.Foci)
                {
                    if (objFocus.GearId == objGear.InternalId)
                    {
                        objNode.Checked = true;
                        objFocus.Rating = objGear.Rating;
                        intFociTotal += objFocus.Rating;
                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                        if (intFociTotal > intMaxFocusTotal && !objCharacter.IgnoreRules)
                        {
                            // Mark the Gear a Bonded.
                            foreach (Gear objCharacterGear in objCharacter.Gear)
                            {
                                if (objCharacterGear.InternalId == objFocus.GearId)
                                    objCharacterGear.Bonded = false;
                            }
                            removeFoci.Add(objFocus);
                            if (!blnWarned)
                            {
                                objNode.Checked = false;
                                MessageBox.Show(LanguageManager.GetString("Message_FocusMaximumForce"), LanguageManager.GetString("MessageTitle_FocusMaximum"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnWarned = true;
                                break;
                            }
                        }
                    }
                }
                foreach (Focus f in removeFoci)
                {
                    objCharacter.Foci.Remove(f);
                }
                treFoci.Nodes.Add(objNode);
            }

            // Add Stacked Foci.
            foreach (Gear objGear in objCharacter.Gear)
            {
                if (objGear.Category == "Stacked Focus")
                {
                    foreach (StackedFocus objStack in objCharacter.StackedFoci)
                    {
                        if (objStack.GearId == objGear.InternalId)
                        {
                            TreeNode objNode = new TreeNode();
                            objNode.Text = LanguageManager.GetString("String_StackedFocus") + ": " + objStack.Name;
                            objNode.Tag = objStack.InternalId;

                            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                            if (objStack.Bonded)
                            {
                                foreach (Gear objFociGear in objStack.Gear)
                                {
                                    if (!string.IsNullOrEmpty(objFociGear.Extra))
                                        ImprovementManager.ForcedValue = objFociGear.Extra;
                                    ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort);
                                    if (objFociGear.WirelessOn)
                                        ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.WirelessBonus, false, objFociGear.Rating, objFociGear.DisplayNameShort);
                                }
                                objNode.Checked = true;
                            }

                            treFoci.Nodes.Add(objNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the number of Free Spirit Power Points used.
        /// </summary>
        public static string CalculateFreeSpiritPowerPoints(Character objCharacter)
        {
            string strReturn;

            if (objCharacter.Metatype == "Free Spirit" && !objCharacter.IsCritter)
            {
                // PC Free Spirit.
                decimal decPowerPoints = 0;

                foreach (CritterPower objPower in objCharacter.CritterPowers)
                {
                    if (objPower.CountTowardsLimit)
                        decPowerPoints += objPower.PowerPoints;
                }

                int intPowerPoints = objCharacter.EDG.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.FreeSpiritPowerPoints);

                // If the house rule to base Power Points on the character's MAG value instead, use the character's MAG.
                if (objCharacter.Options.FreeSpiritPowerPointsMAG)
                    intPowerPoints = objCharacter.MAG.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.FreeSpiritPowerPoints);

                strReturn = string.Format("{1} ({0} " + LanguageManager.GetString("String_Remaining") + ")", intPowerPoints - decPowerPoints, intPowerPoints);
            }
            else
            {
                int intPowerPoints;

                if (objCharacter.Metatype == "Free Spirit")
                {
                    // Critter Free Spirits have a number of Power Points equal to their EDG plus any Free Spirit Power Points Improvements.
                    intPowerPoints = objCharacter.EDG.Value + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.FreeSpiritPowerPoints);
                }
                else if (objCharacter.Metatype == "Ally Spirit")
                {
                    // Ally Spirits get a number of Power Points equal to their MAG.
                    intPowerPoints = objCharacter.MAG.TotalValue;
                }
                else
                {
                    // Spirits get 1 Power Point for every 3 full points of Force (MAG) they possess.
                    intPowerPoints = objCharacter.MAG.TotalValue / 3;
                }

                int intUsed = 0;// _objCharacter.CritterPowers.Count - intExisting;
                foreach (CritterPower objPower in objCharacter.CritterPowers)
                {
                    if (objPower.Category != "Weakness" && objPower.CountTowardsLimit)
                        intUsed++;
                }

                strReturn = string.Format("{1} ({0} " + LanguageManager.GetString("String_Remaining") + ")", intPowerPoints - intUsed, intPowerPoints);
            }

            return strReturn;
        }

        /// <summary>
        /// Calculate the number of Free Sprite Power Points used.
        /// </summary>
        public static string CalculateFreeSpritePowerPoints(Character objCharacter)
        {
            // Free Sprite Power Points.
            int intUsedPowerPoints = 0;

            foreach (CritterPower objPower in objCharacter.CritterPowers)
            {
                if (objPower.CountTowardsLimit)
                    intUsedPowerPoints += 1;
            }

            int intPowerPoints = objCharacter.EDG.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.FreeSpiritPowerPoints);

            return string.Format("{1} ({0} " + LanguageManager.GetString("String_Remaining") + ")", intPowerPoints - intUsedPowerPoints, intPowerPoints);
        }

        /// <summary>
        /// Change the Equipped status of a piece of Gear and all of its children.
        /// </summary>
        /// <param name="objGear">Gear object to change.</param>
        /// <param name="blnEquipped">Whether or not the Gear should be marked as Equipped.</param>
        public static void ChangeGearEquippedStatus(Character objCharacter, Gear objGear, bool blnEquipped)
        {
            if (blnEquipped)
            {
                // Add any Improvements from the Gear.
                if (objGear.Bonus != null || (objGear.WirelessOn && objGear.WirelessBonus != null))
                {
                    bool blnAddImprovement = true;
                    // If this is a Focus which is not bonded, don't do anything.
                    if (objGear.Category != "Stacked Focus")
                    {
                        if (objGear.Category.EndsWith("Foci"))
                            blnAddImprovement = objGear.Bonded;

                        if (blnAddImprovement)
                        {
                            if (!string.IsNullOrEmpty(objGear.Extra))
                                ImprovementManager.ForcedValue = objGear.Extra;
                            if (objGear.Bonus != null)
                            {
                                if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort))
                                {
                                    // Clear created improvements
                                    ChangeGearEquippedStatus(objCharacter, objGear, false);
                                    return;
                                }
                                objGear.Extra = ImprovementManager.SelectedValue;
                            }
                            if (objGear.WirelessOn && objGear.WirelessBonus != null)
                            {
                                if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort))
                                {
                                    // Clear created improvements
                                    ChangeGearEquippedStatus(objCharacter, objGear, false);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Stacked Foci need to be handled a little differently.
                        foreach (StackedFocus objStack in objCharacter.StackedFoci)
                        {
                            if (objStack.GearId == objGear.InternalId && objStack.Bonded)
                            {
                                foreach (Gear objFociGear in objStack.Gear)
                                {
                                    if (!string.IsNullOrEmpty(objFociGear.Extra))
                                        ImprovementManager.ForcedValue = objFociGear.Extra;
                                    if (objGear.Bonus != null)
                                    {
                                        if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort))
                                        {
                                            // Clear created improvements
                                            ChangeGearEquippedStatus(objCharacter, objGear, false);
                                            return;
                                        }
                                        objGear.Extra = ImprovementManager.SelectedValue;
                                    }
                                    if (objGear.WirelessOn && objGear.WirelessBonus != null)
                                    {
                                        if (ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort))
                                        {
                                            // Clear created improvements
                                            ChangeGearEquippedStatus(objCharacter, objGear, false);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Remove any Improvements from the Gear.
                if (objGear.Bonus != null || (objGear.WirelessOn && objGear.WirelessBonus != null))
                {
                    if (objGear.Category != "Stacked Focus")
                        ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.Gear, objGear.InternalId);
                    else
                    {
                        // Stacked Foci need to be handled a little differetnly.
                        foreach (StackedFocus objStack in objCharacter.StackedFoci)
                        {
                            if (objStack.GearId == objGear.InternalId)
                            {
                                foreach (Gear objFociGear in objStack.Gear)
                                    ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                            }
                        }
                    }
                }
            }

            if (objGear.Children.Count > 0)
                ChangeGearEquippedStatus(objCharacter, objGear.Children, blnEquipped);
        }

        /// <summary>
        /// Change the Equipped status of all Gear plugins. This should only be called from the other ChangeGearEquippedStatus and never used directly.
        /// </summary>
        /// <param name="lstGear">List of child Gear to change.</param>
        /// <param name="blnEquipped">Whether or not the children should be marked as Equipped.</param>
        public static void ChangeGearEquippedStatus(Character objCharacter, IEnumerable<Gear> lstGear, bool blnEquipped)
        {
            foreach (Gear objGear in lstGear)
            {
                ChangeGearEquippedStatus(objCharacter, objGear, blnEquipped);
            }
        }

        /// <summary>
        /// Construct a list of possible places to put a piece of modular cyberware. Names are display names of the given items, values are internalIDs of the given items.
        /// </summary>
        /// <param name="objModularCyberware">Cyberware for which to construct the list.</param>
        /// <returns></returns>
        public static List<ListItem> ConstructModularCyberlimbList(Character objCharacter, Cyberware objModularCyberware)
        {
            List<ListItem> lstReturn = new List<ListItem>();

            ListItem liMount = new ListItem();
            liMount.Value = "None";
            liMount.Name = LanguageManager.GetString("String_None");
            lstReturn.Add(liMount);

            foreach (Cyberware objLoopCyberware in objCharacter.Cyberware.GetAllDescendants(x => x.Children))
            {
                // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount && objLoopCyberware.Location == objModularCyberware.Location &&
                    objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name && objLoopCyberware != objModularCyberware)
                {
                    // Make sure it's not the place where the mount is already occupied (either by us or something else)
                    if (!objLoopCyberware.Children.Any(x => x.PlugsIntoModularMount == objLoopCyberware.HasModularMount))
                    {
                        liMount = new ListItem();
                        liMount.Value = objLoopCyberware.InternalId;
                        string strName = string.Empty;
                        if (objLoopCyberware.Parent != null)
                            strName = objLoopCyberware.Parent.DisplayName;
                        else
                            strName = objLoopCyberware.DisplayName;
                        liMount.Name = strName;
                        lstReturn.Add(liMount);
                    }
                }
            }
            foreach (Vehicle objLoopVehicle in objCharacter.Vehicles)
            {
                foreach (VehicleMod objLoopVehicleMod in objLoopVehicle.Mods)
                {
                    foreach (Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                        if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount && objLoopCyberware.Location == objModularCyberware.Location &&
                            objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name && objLoopCyberware != objModularCyberware)
                        {
                            // Make sure it's not the place where the mount is already occupied (either by us or something else)
                            if (!objLoopCyberware.Children.Any(x => x.PlugsIntoModularMount == objLoopCyberware.HasModularMount))
                            {
                                liMount = new ListItem();
                                liMount.Value = objLoopCyberware.InternalId;
                                string strName = objLoopVehicle.DisplayName + " ";
                                if (objLoopCyberware.Parent != null)
                                    strName += objLoopCyberware.Parent.DisplayName;
                                else
                                    strName += objLoopVehicleMod.DisplayName;
                                liMount.Name = strName;
                                lstReturn.Add(liMount);
                            }
                        }
                    }
                }
            }
            return lstReturn;
        }

        /// <summary>
        /// Return a list of CyberwareGrades from XML files.
        /// </summary>
        /// <param name="objSource">Source to load the Grades from, either Bioware or Cyberware.</param>
        public static List<Grade> GetGradeList(Improvement.ImprovementSource objSource, CharacterOptions objCharacterOptions = null)
        {
            List<Grade> lstGrades = new List<Grade>();
            string strXmlFile = objSource == Improvement.ImprovementSource.Bioware ? "bioware.xml" : "cyberware.xml";
            XmlDocument objXMlDocument = XmlManager.Load(strXmlFile);

            string strBookFilter = string.Empty;
            if (objCharacterOptions != null)
                strBookFilter = "[(" + objCharacterOptions.BookXPath() + ")]";
            foreach (XmlNode objNode in objXMlDocument.SelectNodes("/chummer/grades/grade" + strBookFilter))
            {
                Grade objGrade = new Grade();
                objGrade.Load(objNode);
                lstGrades.Add(objGrade);
            }

            return lstGrades;
        }

        #region PDF Functions
        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book coode and page number to open.</param>
        /// <param name="objCharacter">Character from which alternate sources should be fetched.</param>
        public static void OpenPDF(string strSource, Character objCharacter = null)
        {
            // The user must have specified the arguments of their PDF application in order to use this functionality.
            if (string.IsNullOrWhiteSpace(GlobalOptions.PDFParameters))
                return;

            // The user must have specified the arguments of their PDF application in order to use this functionality.
            if (string.IsNullOrWhiteSpace(GlobalOptions.PDFAppPath))
                return;

            string[] strTemp = strSource.Split(' ');
            if (strTemp.Length < 2)
                return;
            int intPage;
            if (!int.TryParse(strTemp[1], out intPage))
                return;

            // Make sure the page is actually a number that we can use as well as being 1 or higher.
            if (intPage < 1)
                return;

            // Revert the sourcebook code to the one from the XML file if necessary.
            string strBook = strTemp[0];
            if (objCharacter != null)
                strBook = objCharacter.Options.LanguageBookShort(strBook);

            // Retrieve the sourcebook information including page offset and PDF application name.
            Uri uriPath;
            SourcebookInfo objBookInfo = GlobalOptions.SourcebookInfo.FirstOrDefault(
                objInfo => objInfo.Code == strBook && !string.IsNullOrEmpty(objInfo.Path));
            if (objBookInfo != null)
            {
                uriPath = new Uri(objBookInfo.Path);
                intPage += objBookInfo.Offset;
            }
            // If the sourcebook was not found, we can't open anything.
            else
                return;

            string strParams = GlobalOptions.PDFParameters;
            strParams = strParams.Replace("{page}", intPage.ToString());
            strParams = strParams.Replace("{localpath}", uriPath.LocalPath);
            strParams = strParams.Replace("{absolutepath}", uriPath.AbsolutePath);
            ProcessStartInfo objProgress = new ProcessStartInfo
            {
                FileName = GlobalOptions.PDFAppPath,
                Arguments = strParams
            };
            Process.Start(objProgress);
        }
        #endregion

        #region Logging Functions
        [Obsolete("Use Log.Info()")]
        public static void LogWrite(LogType logType, string strClass, string strLine)
        {
            Log.Info(new object[] {logType, strLine}, "LEGACY_LOG_CALL", strClass);
        }
        #endregion

        #region Text Functions
        /// <summary>
        /// Word wraps the given text to fit within the specified width.
        /// </summary>
        /// <param name="text">Text to be word wrapped</param>
        /// <param name="width">Width, in characters, to which the text
        /// should be word wrapped</param>
        /// <returns>The modified text</returns>
        public static string WordWrap(string text, int width)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                int eol = text.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + Environment.NewLine.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;
                        if (len > width)
                            len = BreakLine(text, pos, width);
                        sb.Append(text, pos, len);
                        sb.Append(Environment.NewLine);

                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && Char.IsWhiteSpace(text[pos]))
                            pos++;
                    } while (eol > pos);
                }
                else sb.Append(Environment.NewLine); // Empty line
            }
            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
                i--;

            // If no whitespace found, break at maximum length
            if (i < 0)
                return max;

            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }
        #endregion
    }
}
