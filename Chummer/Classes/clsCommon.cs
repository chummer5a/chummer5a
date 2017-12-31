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
        #region XPath Evaluators
        // TODO: implement a sane expression evaluator
        // A single instance of an XmlDocument and its corresponding XPathNavigator helps reduce overhead of evaluating XPaths that just contain mathematical operations
        private static readonly XmlDocument s_ObjXPathNavigatorDocument = new XmlDocument();
        private static readonly XPathNavigator s_ObjXPathNavigator = s_ObjXPathNavigatorDocument.CreateNavigator();

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="strXPath">String as XPath Expression to evaluate</param>
        /// <returns>System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object EvaluateInvariantXPath(string strXPath)
        {
            return s_ObjXPathNavigator.Evaluate(strXPath);
        }

        /// <summary>
        /// Evaluate an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="objXPath">XPath Expression to evaluate</param>
        /// <returns>System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object EvaluateInvariantXPath(XPathExpression objXPath)
        {
            return s_ObjXPathNavigator.Evaluate(objXPath);
        }
        #endregion

        #region Find Functions
        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Gear FindVehicleGear(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            return FindVehicleGear(strGuid, lstVehicles, out Vehicle objFoundVehicle, out WeaponAccessory objFoundWeaponAccessory, out Cyberware objFoundCyberware);
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
                    objReturn = objVehicle.Gear.DeepFindById(strGuid);
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
                        objReturn = FindWeaponGear(strGuid, objMod.Weapons, out WeaponAccessory objAccessory);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponAccessory = objAccessory;
                            objFoundCyberware = null;
                            return objReturn;
                        }

                        // Cyberware.
                        objReturn = FindCyberwareGear(strGuid, objMod.Cyberware, out Cyberware objCyberware);

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
            return FindVehicleMod(strGuid, lstVehicles, out Vehicle objFoundVehicle);
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the VehicleMod to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the VehicleMod was found in.</param>
        public static VehicleMod FindVehicleMod(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        if (objMod.InternalId == strGuid)
                        {
                            objFoundVehicle = objVehicle;
                            return objMod;
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
            return FindVehicleWeapon(strGuid, lstVehicles, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
        {
            return FindVehicleWeapon(strGuid, lstVehicles, out objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod);
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
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                Weapon objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    objReturn = objVehicle.Weapons.DeepFindById(strGuid);
                    if (objReturn != null)
                    {
                        objFoundVehicle = objVehicle;
                        objFoundWeaponMount = null;
                        objFoundVehicleMod = null;
                        return objReturn;
                    }

                    foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                    {
                        objReturn = objMod.Weapons.DeepFindById(strGuid);
                        if (objReturn != null)
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponMount = objMod;
                            objFoundVehicleMod = null;
                            return objReturn;
                        }
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objReturn = objMod.Weapons.DeepFindById(strGuid);
                        if (objReturn != null)
                        {
                            objFoundVehicle = objVehicle;
                            objFoundVehicleMod = objMod;
                            objFoundWeaponMount = null;
                            return objReturn;
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
        internal static WeaponMount FindVehicleWeaponMount(string strGuid, IEnumerable<Vehicle> lstVehicles, out Vehicle outVehicle)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                    {
                        if (objMod.InternalId == strGuid)
                        {
                            outVehicle = objVehicle;
                            return objMod;
                        }
                    }
                }
            }
            outVehicle = null;
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strGuid"></param>
        /// <param name="lstVehicles"></param>
        /// <returns></returns>
        internal static VehicleMod FindVehicleWeaponMountMod(string strGuid, IEnumerable<Vehicle> lstVehicles, out WeaponMount outMount)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                    {
                        foreach (VehicleMod objVehicleMod in objWeaponMount.Mods)
                        {
                            if (objVehicleMod.InternalId == strGuid)
                            {
                                outMount = objWeaponMount;
                                return objVehicleMod;
                            }
                        }
                    }
                }
            }
            outMount = null;
            return null;
        }
        
        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static WeaponAccessory FindVehicleWeaponAccessory(string strGuid, IEnumerable<Vehicle> lstVehicles)
        {
            return FindVehicleWeaponAccessory(strGuid, lstVehicles, out Weapon objFoundWeapon);
        }

        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static WeaponAccessory FindVehicleWeaponAccessory(string strGuid, IEnumerable<Vehicle> lstVehicles, out Weapon objFoundWeapon)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                WeaponAccessory objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    objReturn = FindWeaponAccessory(strGuid, objVehicle.Weapons, out objFoundWeapon);
                    if (objReturn != null)
                        return objReturn;

                    foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                    {
                        objReturn = FindWeaponAccessory(strGuid, objMod.Weapons, out objFoundWeapon);
                        if (objReturn != null)
                        {
                            return objReturn;
                        }
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objReturn = FindWeaponAccessory(strGuid, objMod.Weapons, out objFoundWeapon);
                        if (objReturn != null)
                        {
                            return objReturn;
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
            return FindVehicleCyberware(strGuid, lstVehicles, out VehicleMod objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Cyberware to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
        public static Cyberware FindVehicleCyberware(string strGuid, IEnumerable<Vehicle> lstVehicles, out VehicleMod objFoundVehicleMod)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                Cyberware objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objReturn = objMod.Cyberware.DeepFindById(strGuid);
                        if (objReturn != null)
                        {
                            objFoundVehicleMod = objMod;
                            return objReturn;
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
            return FindArmorGear(strGuid, lstArmors, out Armor objFoundArmor, out ArmorMod objFoundArmorMod);
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
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Armor objArmor in lstArmors)
                {
                    objReturn = objArmor.Gear.DeepFindById(strGuid);
                    if (objReturn != null)
                    {
                        objFoundArmor = objArmor;
                        objFoundArmorMod = null;
                        return objReturn;
                    }

                    foreach (ArmorMod objMod in objArmor.ArmorMods)
                    {
                        objReturn = objMod.Gear.DeepFindById(strGuid);
                        if (objReturn != null)
                        {
                            objFoundArmor = objArmor;
                            objFoundArmorMod = objMod;
                            return objReturn;
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
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                foreach (Armor objArmor in lstArmors)
                {
                    foreach (ArmorMod objMod in objArmor.ArmorMods)
                    {
                        if (objMod.InternalId == strGuid)
                            return objMod;
                    }
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
            return FindCyberwareGear(strGuid, lstCyberware, out Cyberware objFoundCyberware);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public static Gear FindCyberwareGear(string strGuid, IEnumerable<Cyberware> lstCyberware, out Cyberware objFoundCyberware)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Cyberware objCyberware in lstCyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                {
                    objReturn = objCyberware.Gear.DeepFindById(strGuid);

                    if (objReturn != null)
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
            return FindWeaponAccessory(strGuid, lstWeapons, out Weapon objFoundWeapon);
        }

        /// <summary>
        /// Locate a WeaponAccessory within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the WeaponAccessory to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundWeapon">Weapon in which the Accesory was found.</param>
        public static WeaponAccessory FindWeaponAccessory(string strGuid, IEnumerable<Weapon> lstWeapons, out Weapon objFoundWeapon)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
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
            return FindWeaponGear(strGuid, lstWeapons, out WeaponAccessory objFoundAccessory);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundAccessory">WeaponAccessory that the Gear was found in.</param>
        public static Gear FindWeaponGear(string strGuid, IEnumerable<Weapon> lstWeapons, out WeaponAccessory objFoundAccessory)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        objReturn = objAccessory.Gear.DeepFindById(strGuid);

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
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
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
            return FindMartialArtAdvantage(strGuid, lstMartialArts, out MartialArt objFoundMartialArt);
        }

        /// <summary>
        /// Locate a Martial Art Advantage within the character's Martial Arts.
        /// </summary>
        /// <param name="strGuid">InternalId of the Martial Art Advantage to find.</param>
        /// <param name="lstMartialArts">List of Martial Arts to search.</param>
        /// <param name="objFoundMartialArt">MartialArt the Advantage was found in.</param>
        public static MartialArtAdvantage FindMartialArtAdvantage(string strGuid, IEnumerable<MartialArt> lstMartialArts, out MartialArt objFoundMartialArt)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && strGuid != Guid.Empty.ToString())
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
        #endregion

        #region Add Improvements Functions
        public static void ReaddGearImprovements(Character objCharacter, Gear objGear, TreeView treGears, ref string strOutdatedItems, ICollection<string> lstInternalIdFilter, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Gear, bool blnStackEquipped = true)
        {
            // We're only re-apply improvements a list of items, not all of them
            if (lstInternalIdFilter == null || lstInternalIdFilter.Contains(objGear.InternalId))
            {
                XmlNode objNode = objGear.GetNode();
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
                            ImprovementManager.CreateImprovements(objCharacter, eSource, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                objGear.Extra = ImprovementManager.SelectedValue;
                                TreeNode objGearNode = treGears.FindNode(objGear.InternalId);
                                if (objGearNode != null)
                                    objGearNode.Text = objGear.DisplayName(GlobalOptions.Language);
                            }
                        }
                        if (objGear.WirelessOn && objGear.WirelessBonus != null)
                        {
                            ImprovementManager.ForcedValue = objGear.Extra;
                            ImprovementManager.CreateImprovements(objCharacter, eSource, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                objGear.Extra = ImprovementManager.SelectedValue;
                                TreeNode objGearNode = treGears.FindNode(objGear.InternalId);
                                if (objGearNode != null)
                                    objGearNode.Text = objGear.DisplayName(GlobalOptions.Language);
                            }
                        }
                    }

                }
                else
                {
                    strOutdatedItems += objGear.DisplayName(GlobalOptions.Language) + "\n";
                }
            }
            foreach (Gear objChild in objGear.Children)
                ReaddGearImprovements(objCharacter, objChild, treGears, ref strOutdatedItems, lstInternalIdFilter, eSource, blnStackEquipped);
        }
        #endregion

        #region TreeNode Creation Methods
        /// <summary>
        /// Build up the Tree for the current piece of Cyberware and all of its children.
        /// </summary>
        /// <param name="objCyberware">Cyberware to iterate through.</param>
        /// <param name="objParentNode">TreeNode to append to.</param>
        /// <param name="objMenu">ContextMenuStrip that the new Cyberware TreeNodes should use.</param>
        /// <param name="objGearMenu">ContextMenuStrip that the new Gear TreeNodes should use.</param>
        public static void CreateCyberwareTreeNode(Cyberware objCyberware, TreeNode objParentNode, ContextMenuStrip objMenu, ContextMenuStrip objGearMenu)
        {
            TreeNode objNode = new TreeNode
            {
                Text = objCyberware.DisplayName(GlobalOptions.Language),
                Tag = objCyberware.InternalId
            };
            if (!string.IsNullOrEmpty(objCyberware.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (!string.IsNullOrEmpty(objCyberware.ParentID))
                objNode.ForeColor = SystemColors.GrayText;
            objNode.ToolTipText = objCyberware.Notes;
            objNode.ContextMenuStrip = objMenu;

            objParentNode.Nodes.Add(objNode);
            objParentNode.Expand();

            foreach (Cyberware objChild in objCyberware.Children)
                CreateCyberwareTreeNode(objChild, objNode, objMenu, objGearMenu);

            foreach (Gear objGear in objCyberware.Gear)
            {
                TreeNode objGearNode = new TreeNode
                {
                    Text = objGear.DisplayName(GlobalOptions.Language),
                    Tag = objGear.InternalId
                };
                if (!string.IsNullOrEmpty(objGear.Notes))
                    objGearNode.ForeColor = Color.SaddleBrown;
                else if (objGear.IncludedInParent)
                    objGearNode.ForeColor = SystemColors.GrayText;
                objGearNode.ToolTipText = objGear.Notes;
                objGearNode.ContextMenuStrip = objGearMenu;

                objGear.BuildGearTree(objGearNode, objGearMenu);

                objNode.Nodes.Add(objGearNode);
                objNode.Expand();
            }

        }

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
            TreeNode objNode = new TreeNode
            {
                Text = objArmor.DisplayName(GlobalOptions.Language),
                Tag = objArmor.InternalId
            };
            if (!string.IsNullOrEmpty(objArmor.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            objNode.ToolTipText = objArmor.Notes;

            foreach (ArmorMod objMod in objArmor.ArmorMods)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = objMod.DisplayName(GlobalOptions.Language),
                    Tag = objMod.InternalId,
                    ContextMenuStrip = string.IsNullOrEmpty(objMod.GearCapacity) ? cmsArmorMod : cmsArmorGear
                };
                if (!string.IsNullOrEmpty(objMod.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                else if (objMod.IncludedInArmor)
                    objChild.ForeColor = SystemColors.GrayText;
                objChild.ToolTipText = objMod.Notes;
                foreach (Gear objGear in objMod.Gear)
                {
                    TreeNode objChildGear = new TreeNode
                    {
                        Text = objGear.DisplayName(GlobalOptions.Language),
                        Tag = objGear.InternalId
                    };
                    if (!string.IsNullOrEmpty(objGear.Notes))
                        objChildGear.ForeColor = Color.SaddleBrown;
                    else if (objGear.IncludedInParent)
                        objChildGear.ForeColor = SystemColors.GrayText;
                    objChildGear.ToolTipText = objGear.Notes;

                    objGear.BuildGearTree(objChildGear, cmsArmorGear);

                    objChildGear.ContextMenuStrip = cmsArmorGear;
                    objChild.Nodes.Add(objChildGear);
                    objChild.Expand();
                }
                objNode.Nodes.Add(objChild);
                objNode.Expand();
            }

            foreach (Gear objGear in objArmor.Gear)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = objGear.DisplayName(GlobalOptions.Language),
                    Tag = objGear.InternalId
                };
                if (!string.IsNullOrEmpty(objGear.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                else if (objGear.IncludedInParent)
                    objChild.ForeColor = SystemColors.GrayText;
                objChild.ToolTipText = objGear.Notes;

                objGear.BuildGearTree(objChild, cmsArmorGear);

                objChild.ContextMenuStrip = cmsArmorGear;
                objNode.Nodes.Add(objChild);
                objNode.Expand();
            }

            TreeNode objParent = treArmor.Nodes[0];
            if (!string.IsNullOrEmpty(objArmor.Location))
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
            TreeNode objNode = new TreeNode
            {
                Text = objVehicle.DisplayName(GlobalOptions.Language),
                Tag = objVehicle.InternalId
            };
            if (!string.IsNullOrEmpty(objVehicle.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (!string.IsNullOrEmpty(objVehicle.ParentID))
                objNode.ForeColor = SystemColors.GrayText;
            objNode.ToolTipText = objVehicle.Notes;

            // Populate the list of Vehicle Locations.
            foreach (string strLocation in objVehicle.Locations)
            {
                TreeNode objLocation = new TreeNode
                {
                    Tag = strLocation,
                    Text = strLocation,
                    ContextMenuStrip = cmsVehicleLocation
                };
                objNode.Nodes.Add(objLocation);
            }

            // VehicleMods.
            foreach (VehicleMod objMod in objVehicle.Mods)
            {
                TreeNode objChildNode = new TreeNode
                {
                    Text = objMod.DisplayName(GlobalOptions.Language),
                    Tag = objMod.InternalId
                };
                if (!string.IsNullOrEmpty(objMod.Notes))
                    objChildNode.ForeColor = Color.SaddleBrown;
                else if (objMod.IncludedInVehicle)
                    objChildNode.ForeColor = SystemColors.GrayText;
                objChildNode.ToolTipText = objMod.Notes;

                // Cyberware.
                foreach (Cyberware objCyberware in objMod.Cyberware)
                {
                    TreeNode objCyberwareNode = new TreeNode
                    {
                        Text = objCyberware.DisplayName(GlobalOptions.Language),
                        Tag = objCyberware.InternalId
                    };
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
                TreeNode mountsNode = new TreeNode
                {
                    Tag = "String_WeaponMounts",
                    Text = LanguageManager.GetString("String_WeaponMounts", GlobalOptions.Language)
                };
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
                TreeNode objGearNode = new TreeNode
                {
                    Text = objGear.DisplayName(GlobalOptions.Language),
                    Tag = objGear.InternalId
                };
                if (!string.IsNullOrEmpty(objGear.Notes))
                    objGearNode.ForeColor = Color.SaddleBrown;
                else if (objGear.IncludedInParent)
                    objGearNode.ForeColor = SystemColors.GrayText;
                objGearNode.ToolTipText = objGear.Notes;

                objGear.BuildGearTree(objGearNode, cmsVehicleGear);

                objGearNode.ContextMenuStrip = cmsVehicleGear;

                TreeNode objParent = objNode;
                if (!string.IsNullOrEmpty(objGear.Location))
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
            TreeNode objNode = new TreeNode
            {
                Text = wm.DisplayName(GlobalOptions.Language),
                Tag = wm.InternalId,
                ContextMenuStrip = cmsVehicleWeaponMount
            };
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
            TreeNode objNode = new TreeNode
            {
                Text = objWeapon.DisplayName(GlobalOptions.Language),
                Tag = WeaponID ?? objWeapon.InternalId
            };
            if (!string.IsNullOrEmpty(objWeapon.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || WeaponID != null || !string.IsNullOrEmpty(objWeapon.ParentID))
                objNode.ForeColor = SystemColors.GrayText;

            objNode.ToolTipText = objWeapon.Notes;

            // Add attached Weapon Accessories.
            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = objAccessory.DisplayName(GlobalOptions.Language),
                    Tag = objAccessory.InternalId,
                    ContextMenuStrip = cmsWeaponAccessory
                };
                if (!string.IsNullOrEmpty(objAccessory.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                else if (objAccessory.IncludedInWeapon)
                    objChild.ForeColor = SystemColors.GrayText;
                objChild.ToolTipText = objAccessory.Notes;

                // Add any Gear attached to the Weapon Accessory.
                foreach (Gear objGear in objAccessory.Gear)
                {
                    TreeNode objGearChild = new TreeNode
                    {
                        Text = objGear.DisplayName(GlobalOptions.Language),
                        Tag = objGear.InternalId
                    };
                    if (!string.IsNullOrEmpty(objGear.Notes))
                        objGearChild.ForeColor = Color.SaddleBrown;
                    else if (objGear.IncludedInParent)
                        objGearChild.ForeColor = SystemColors.GrayText;
                    objGearChild.ToolTipText = objGear.Notes;

                    objGear.BuildGearTree(objGearChild, cmsWeaponAccessoryGear);

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
            Gear objGear = objCharacter.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());

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
                objGear.Parent.RefreshMatrixAttributeArray();
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
                Gear objParent = objCharacter.Gear.DeepFindById(objDestination.Tag.ToString());

                // Add the Gear as a child of the destination Node and clear its location.
                objParent.Children.Add(objGear);
                objGear.Location = string.Empty;
                objGear.Parent = objParent;
                objParent.RefreshMatrixAttributeArray();
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
            if (objNewParent.Text == LanguageManager.GetString("Node_SelectedGear", GlobalOptions.Language))
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
            Armor objArmor = objCharacter.Armor.FindById(treArmor.SelectedNode.Tag.ToString());

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
            if (objNewParent.Text == LanguageManager.GetString("Node_SelectedArmor", GlobalOptions.Language))
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
            if (objNewParent.Text == LanguageManager.GetString("Node_SelectedWeapons", GlobalOptions.Language))
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
            Cyberware objCyberware = objCharacter.Cyberware.DeepFindById(objCyberwareNode.Tag.ToString());
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
            Vehicle objDestinationVehicle = objCharacter.Vehicles.FindById(objVehicleNode.Tag.ToString());

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
            Gear objGear = FindVehicleGear(treVehicles.SelectedNode.Tag.ToString(), objCharacter.Vehicles, out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);

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
                objGear.Parent.RefreshMatrixAttributeArray();
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
                objDestinationGear.RefreshMatrixAttributeArray();
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

        /// <summary>
        /// Convert a book code into the full name.
        /// </summary>
        /// <param name="strCode">Book code to convert.</param>
        public static string BookFromCode(string strCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strCode))
            {
                XmlNode objXmlBook = XmlManager.Load("books.xml", strLanguage)?.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
                string strReturn = objXmlBook?["name"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strReturn))
                    return strReturn;
            }
            return string.Empty;
        }

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        public static string LanguageBookShort(string strCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strCode))
            {
                XmlNode objXmlBook = XmlManager.Load("books.xml", strLanguage)?.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
                string strReturn = objXmlBook?["altcode"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strReturn))
                    return strReturn;
                return strCode;
            }
            return string.Empty;
        }

        /// <summary>
        /// Determine the book's original code by using the alternate code.
        /// </summary>
        /// <param name="strCode">Alternate code to look for.</param>
        public static string BookFromAltCode(string strCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strCode))
            {
                XmlNode objXmlBook = XmlManager.Load("books.xml", strLanguage)?.SelectSingleNode("/chummer/books/book[altcode = \"" + strCode + "\"]");
                string strReturn = objXmlBook?["code"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strReturn))
                    return strReturn;
                return strCode;
            }
            return string.Empty;
        }

        /// <summary>
        /// Book name (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        public static string LanguageBookLong(string strCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strCode))
            {
                XmlNode objXmlBook = XmlManager.Load("books.xml", strLanguage)?.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
                if (objXmlBook != null)
                {
                    string strReturn = objXmlBook["translate"]?.InnerText;
                    if (!string.IsNullOrWhiteSpace(strReturn))
                        return strReturn;
                    strReturn = objXmlBook["name"]?.InnerText;
                    if (!string.IsNullOrWhiteSpace(strReturn))
                        return strReturn;
                }
            }
            return string.Empty;
        }

        #region PDF Functions
        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book coode and page number to open.</param>
        public static void OpenPDFFromControl(object sender, EventArgs e)
        {
            if (sender is Control objControl)
                OpenPDF(objControl.Text);
        }
        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book coode and page number to open.</param>
        public static void OpenPDF(string strSource, string strPDFParamaters = "", string strPDFAppPath = "")
        {
            if (string.IsNullOrEmpty(strPDFParamaters))
                strPDFParamaters = GlobalOptions.PDFParameters;
            // The user must have specified the arguments of their PDF application in order to use this functionality.
            if (string.IsNullOrWhiteSpace(strPDFParamaters))
                return;

            if (string.IsNullOrEmpty(strPDFAppPath))
                strPDFAppPath = GlobalOptions.PDFAppPath;
            // The user must have specified the arguments of their PDF application in order to use this functionality.
            if (string.IsNullOrWhiteSpace(strPDFAppPath))
                return;

            string[] strTemp = strSource.Split(' ');
            if (strTemp.Length < 2)
                return;
            if (!int.TryParse(strTemp[1], out int intPage))
                return;

            // Make sure the page is actually a number that we can use as well as being 1 or higher.
            if (intPage < 1)
                return;

            // Revert the sourcebook code to the one from the XML file if necessary.
            string strBook = LanguageBookShort(strTemp[0], GlobalOptions.Language);

            // Retrieve the sourcebook information including page offset and PDF application name.
            SourcebookInfo objBookInfo = GlobalOptions.SourcebookInfo.FirstOrDefault(objInfo => objInfo.Code == strBook && !string.IsNullOrEmpty(objInfo.Path));
            // If the sourcebook was not found, we can't open anything.
            if (objBookInfo == null)
                return;

            Uri uriPath = new Uri(objBookInfo.Path);
            intPage += objBookInfo.Offset;

            string strParams = strPDFParamaters;
            strParams = strParams.Replace("{page}", intPage.ToString());
            strParams = strParams.Replace("{localpath}", uriPath.LocalPath);
            strParams = strParams.Replace("{absolutepath}", uriPath.AbsolutePath);
            ProcessStartInfo objProgress = new ProcessStartInfo
            {
                FileName = strPDFAppPath,
                Arguments = strParams
            };
            Process.Start(objProgress);
        }
        #endregion
    }
}
