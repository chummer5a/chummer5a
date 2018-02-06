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
using System.Windows.Forms;
using System.IO;
using System.Linq;
using Chummer.Backend.Equipment;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.CompilerServices;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

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
        public static Gear FindVehicleGear(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            return lstVehicles.FindVehicleGear(strGuid, out Vehicle objFoundVehicle, out WeaponAccessory objFoundWeaponAccessory, out Cyberware objFoundCyberware);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Gear was found in.</param>
        public static Gear FindVehicleGear(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle, out WeaponAccessory objFoundWeaponAccessory, out Cyberware objFoundCyberware)
        {
            if (!string.IsNullOrEmpty(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    Gear objReturn = objVehicle.Gear.DeepFindById(strGuid);
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
                        objReturn = objMod.Weapons.FindWeaponGear(strGuid, out WeaponAccessory objAccessory);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponAccessory = objAccessory;
                            objFoundCyberware = null;
                            return objReturn;
                        }

                        // Cyberware.
                        objReturn = objMod.Cyberware.FindCyberwareGear(strGuid, out Cyberware objCyberware);

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
        public static VehicleMod FindVehicleMod(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            return lstVehicles.FindVehicleMod(strGuid, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount);
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the VehicleMod to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the VehicleMod was found in.</param>
        public static VehicleMod FindVehicleMod(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        if (objMod.InternalId == strGuid)
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponMount = null;
                            return objMod;
                        }
                    }
                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (VehicleMod objMod in objMount.Mods)
                        {
                            if (objMod.InternalId == strGuid)
                            {
                                objFoundVehicle = objVehicle;
                                objFoundWeaponMount = objMount;
                                return objMod;
                            }
                        }
                    }
                }
            }

            objFoundVehicle = null;
            objFoundWeaponMount = null;
            return null;
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Weapon FindVehicleWeapon(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            return lstVehicles.FindVehicleWeapon(strGuid, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle)
        {
            return lstVehicles.FindVehicleWeapon(strGuid, out objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InteralId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        /// <param name="objFoundVehicleMod">Vehicle mod that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    Weapon objReturn = objVehicle.Weapons.DeepFindById(strGuid);
                    if (objReturn != null)
                    {
                        objFoundVehicle = objVehicle;
                        objFoundWeaponMount = null;
                        objFoundVehicleMod = null;
                        return objReturn;
                    }

                    foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                    {
                        objReturn = objWeaponMount.Weapons.DeepFindById(strGuid);
                        if (objReturn != null)
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponMount = objWeaponMount;
                            objFoundVehicleMod = null;
                            return objReturn;
                        }

                        foreach (VehicleMod objMod in objWeaponMount.Mods)
                        {
                            objReturn = objMod.Weapons.DeepFindById(strGuid);
                            if (objReturn != null)
                            {
                                objFoundVehicle = objVehicle;
                                objFoundVehicleMod = objMod;
                                objFoundWeaponMount = objWeaponMount;
                                return objReturn;
                            }
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
        public static WeaponMount FindVehicleWeaponMount(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle outVehicle)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
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
        public static VehicleMod FindVehicleWeaponMountMod(this IEnumerable<Vehicle> lstVehicles, string strGuid, out WeaponMount outMount)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
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
        public static WeaponAccessory FindVehicleWeaponAccessory(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    WeaponAccessory objReturn = objVehicle.Weapons.FindWeaponAccessory(strGuid);
                    if (objReturn != null)
                    {
                        return objReturn;
                    }

                    foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                    {
                        objReturn = objMod.Weapons.FindWeaponAccessory(strGuid);
                        if (objReturn != null)
                        {
                            return objReturn;
                        }
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objReturn = objMod.Weapons.FindWeaponAccessory(strGuid);
                        if (objReturn != null)
                        {
                            return objReturn;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Cyberware to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Cyberware FindVehicleCyberware(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            return lstVehicles.FindVehicleCyberware(strGuid, out VehicleMod objFoundVehicleMod);
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Cyberware to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
        public static Cyberware FindVehicleCyberware(this IEnumerable<Vehicle> lstVehicles, string strGuid, out VehicleMod objFoundVehicleMod)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        Cyberware objReturn = objMod.Cyberware.DeepFindById(strGuid);
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
        public static Gear FindArmorGear(this IEnumerable<Armor> lstArmors, string strGuid)
        {
            return lstArmors.FindArmorGear(strGuid, out Armor objFoundArmor, out ArmorMod objFoundArmorMod);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="objFoundArmor">Armor that the Gear was found in.</param>
        /// <param name="objFoundArmorMod">Armor mod that the Gear was found in.</param>
        public static Gear FindArmorGear(this IEnumerable<Armor> lstArmors, string strGuid, out Armor objFoundArmor, out ArmorMod objFoundArmorMod)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Armor objArmor in lstArmors)
                {
                    Gear objReturn = objArmor.Gear.DeepFindById(strGuid);
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
        public static ArmorMod FindArmorMod(this IEnumerable<Armor> lstArmors, string strGuid)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
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
        public static Gear FindCyberwareGear(this IEnumerable<Cyberware> lstCyberware, string strGuid)
        {
            return lstCyberware.FindCyberwareGear(strGuid, out Cyberware objFoundCyberware);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public static Gear FindCyberwareGear(this IEnumerable<Cyberware> lstCyberware, string strGuid, out Cyberware objFoundCyberware)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Cyberware objCyberware in lstCyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                {
                    Gear objReturn = objCyberware.Gear.DeepFindById(strGuid);

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
        public static WeaponAccessory FindWeaponAccessory(this IEnumerable<Weapon> lstWeapons, string strGuid)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Count > 0))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        if (objAccessory.InternalId == strGuid)
                        {
                            return objAccessory;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        public static Gear FindWeaponGear(this IEnumerable<Weapon> lstWeapons, string strGuid)
        {
            return lstWeapons.FindWeaponGear(strGuid, out WeaponAccessory objFoundAccessory);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundAccessory">WeaponAccessory that the Gear was found in.</param>
        public static Gear FindWeaponGear(this IEnumerable<Weapon> lstWeapons, string strGuid, out WeaponAccessory objFoundAccessory)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        Gear objReturn = objAccessory.Gear.DeepFindById(strGuid);

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
        public static Enhancement FindEnhancement(this Character objCharacter, string strGuid)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
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
        /// Locate a Martial Art Technique within the character's Martial Arts.
        /// </summary>
        /// <param name="strGuid">InternalId of the Martial Art Technique to find.</param>
        /// <param name="lstMartialArts">List of Martial Arts to search.</param>
        public static MartialArtTechnique FindMartialArtTechnique(this IEnumerable<MartialArt> lstMartialArts, string strGuid)
        {
            return lstMartialArts.FindMartialArtTechnique(strGuid, out MartialArt objFoundMartialArt);
        }

        /// <summary>
        /// Locate a Martial Art Technique within the character's Martial Arts.
        /// </summary>
        /// <param name="strGuid">InternalId of the Martial Art Advantage to find.</param>
        /// <param name="lstMartialArts">List of Martial Arts to search.</param>
        /// <param name="objFoundMartialArt">MartialArt the Technique was found in.</param>
        public static MartialArtTechnique FindMartialArtTechnique(this IEnumerable<MartialArt> lstMartialArts, string strGuid, out MartialArt objFoundMartialArt)
        {
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (MartialArt objArt in lstMartialArts)
                {
                    foreach (MartialArtTechnique objTechnique in objArt.Techniques)
                    {
                        if (objTechnique.InternalId == strGuid)
                        {
                            objFoundMartialArt = objArt;
                            return objTechnique;
                        }
                    }
                }
            }

            objFoundMartialArt = null;
            return null;
        }
        #endregion

        #region Move TreeNodes
        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop, changing its parent.
        /// </summary>
        /// <param name="intNewIndex">Node's new idnex.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveGearParent(Character objCharacter, TreeNode objDestination, TreeNode objGearNode)
        {
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = objDestination; objCheckNode != null && objCheckNode.Level >= objDestination.Level; objCheckNode = objCheckNode.Parent)
                if (objCheckNode == objGearNode)
                    return;

            string strSelectedId = objGearNode.Tag.ToString();
            // Locate the currently selected piece of Gear.
            Gear objGear = objCharacter.Gear.DeepFindById(strSelectedId);

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
                objGear.Location = objDestination.Text;
                objGear.Parent = null;
                objCharacter.Gear.Add(objGear);
            }
            else
            {
                // Locate the Gear that the item was dropped on.
                Gear objParent = objCharacter.Gear.DeepFindById(objDestination.Tag.ToString());

                // Add the Gear as a child of the destination Node and clear its location.
                objGear.Location = string.Empty;
                objGear.Parent = objParent;
                objParent.Children.Add(objGear);
                objParent.RefreshMatrixAttributeArray();
            }
        }

        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveGearNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode objGearNode)
        {
            string strSelectedId = objGearNode?.Tag.ToString();
            Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.InternalId == strSelectedId);
            if (objGear != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Gear item.
                objGear.Location = objNewParent.Tag.ToString() == "Node_SelectedGear" ? string.Empty : objNewParent.Text;

                objCharacter.Gear.Move(objCharacter.Gear.IndexOf(objGear), intNewIndex);
            }
        }

        /// <summary>
        /// Move a Gear Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveGearRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
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
            
            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.GearLocations.Move(objCharacter.GearLocations.IndexOf(strLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Lifestyle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveLifestyleNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodLifestyleNode)
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

            string strSelectedId = nodLifestyleNode.Tag.ToString();
            Lifestyle objLifestyle = objCharacter.Lifestyles.FirstOrDefault(x => x.InternalId == strSelectedId);
            if (objLifestyle != null)
                objCharacter.Lifestyles.Move(objCharacter.Lifestyles.IndexOf(objLifestyle), intNewIndex);
        }

        /// <summary>
        /// Move an Armor TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveArmorNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodArmorNode)
        {
            string strSelectedId = nodArmorNode?.Tag.ToString();
            // Locate the currently selected Armor.
            Armor objArmor = objCharacter.Armor.FindById(strSelectedId);
            if (objArmor != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Armor item.
                objArmor.Location = objNewParent.Tag.ToString() == "Node_SelectedArmor" ? string.Empty : objNewParent.Text;

                objCharacter.Armor.Move(objCharacter.Armor.IndexOf(objArmor), intNewIndex);
            }
        }

        /// <summary>
        /// Move an Armor Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveArmorRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
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

            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.ArmorLocations.Move(objCharacter.ArmorLocations.IndexOf(strLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Weapon TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveWeaponNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodWeaponNode)
        {
            string strSelectedId = nodWeaponNode?.Tag.ToString();
            // Locate the currently selected Weapon.
            Weapon objWeapon = objCharacter.Weapons.FindById(strSelectedId);
            if (objWeapon != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Armor item.
                objWeapon.Location = objNewParent.Tag.ToString() == "Node_SelectedWeapons" ? string.Empty : objNewParent.Text;

                objCharacter.Weapons.Move(objCharacter.Weapons.IndexOf(objWeapon), intNewIndex);
            }
        }

        /// <summary>
        /// Move a Weapon Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveWeaponRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
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
            
            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.WeaponLocations.Move(objCharacter.WeaponLocations.IndexOf(strLocation), intNewIndex);
        }
        
        /// <summary>
        /// Move a Vehicle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveVehicleNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodVehicleNode)
        {
            string strSelectedId = nodVehicleNode?.Tag.ToString();
            // Locate the currently selected Vehicle.
            Vehicle objVehicle = objCharacter.Vehicles.FindById(strSelectedId);
            if (objVehicle != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Armor item.
                objVehicle.Location = objNewParent.Tag.ToString() == "Node_SelectedVehicles" ? string.Empty : objNewParent.Text;

                objCharacter.Vehicles.Move(objCharacter.Vehicles.IndexOf(objVehicle), intNewIndex);
            }
        }

        /// <summary>
        /// Move a Vehicle Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="nodDestination">Destination Node.</param>
        public static void MoveVehicleGearParent(Character objCharacter, TreeNode nodDestination, TreeNode nodGearNode)
        {
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = nodDestination; objCheckNode != null && objCheckNode.Level >= nodDestination.Level; objCheckNode = objCheckNode.Parent)
                if (objCheckNode == nodGearNode)
                    return;

            // Locate the currently selected piece of Gear.
            Gear objGear = objCharacter.Vehicles.FindVehicleGear(nodGearNode.Tag.ToString(), out Vehicle objOldVehicle, out WeaponAccessory objOldWeaponAccessory, out Cyberware objOldCyberware);

            if (objGear == null)
                return;

            Gear objOldParent = objGear.Parent;
            string strDestinationId = nodDestination.Tag.ToString();
            // Make sure the destination is another piece of Gear or a Location.
            Gear objDestinationGear = objCharacter.Vehicles.FindVehicleGear(strDestinationId);
            if (objDestinationGear != null)
            {
                // Remove the Gear from the Vehicle.
                if (objOldParent != null)
                {
                    objOldParent.Children.Remove(objGear);
                    objOldParent.RefreshMatrixAttributeArray();
                }
                else if (objOldCyberware != null)
                {
                    objOldCyberware.Gear.Remove(objGear);
                    objOldCyberware.RefreshMatrixAttributeArray();
                }
                else if (objOldWeaponAccessory != null)
                    objOldWeaponAccessory.Gear.Remove(objGear);
                else
                {
                    objOldVehicle.Gear.Remove(objGear);
                    objOldVehicle.RefreshMatrixAttributeArray();
                }

                // Add the Gear to its new parent.
                objGear.Location = string.Empty;
                objGear.Parent = objDestinationGear;
                objDestinationGear.Children.Add(objGear);
                objDestinationGear.RefreshMatrixAttributeArray();
            }
            else
            {
                // Determine if this is a Location.
                TreeNode nodVehicleNode = nodDestination;
                do
                {
                    nodVehicleNode = nodVehicleNode.Parent;
                }
                while (nodVehicleNode.Level > 1);

                // Get a reference to the destination Vehicle.
                Vehicle objDestinationVehicle = objCharacter.Vehicles.FindById(nodVehicleNode.Tag.ToString());

                // Determine if this is a Location in the destination Vehicle.
                string strDestinationLocation = objDestinationVehicle.Locations.FirstOrDefault(x => x == strDestinationId);

                if (!string.IsNullOrEmpty(strDestinationLocation))
                {
                    // Remove the Gear from the Vehicle.
                    if (objOldParent != null)
                    {
                        objOldParent.Children.Remove(objGear);
                        objOldParent.RefreshMatrixAttributeArray();
                    }
                    else if (objOldCyberware != null)
                    {
                        objOldCyberware.Gear.Remove(objGear);
                        objOldCyberware.RefreshMatrixAttributeArray();
                    }
                    else if (objOldWeaponAccessory != null)
                        objOldWeaponAccessory.Gear.Remove(objGear);
                    else
                    {
                        objOldVehicle.Gear.Remove(objGear);
                        objOldVehicle.RefreshMatrixAttributeArray();
                    }

                    // Add the Gear to the Vehicle and set its Location.
                    objGear.Location = strDestinationLocation;
                    objGear.Parent = null;
                    objDestinationVehicle.Gear.Add(objGear);
                }
            }
        }

        /// <summary>
        /// Move an Improvement TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveImprovementNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
        {
            string strSelectedId = nodOldNode?.Tag.ToString();
            int intOldIndex = -1;
            for (int i = 0; i < objCharacter.Improvements.Count; ++i)
            {
                if (objCharacter.Improvements[i].SourceName == strSelectedId)
                {
                    intOldIndex = i;
                    break;
                }
            }

            if (intOldIndex != -1)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                Improvement objImprovement = objCharacter.Improvements[intOldIndex];
                // Change the Group on the Custom Improvement.
                objImprovement.CustomGroup = objNewParent.Text;
                objCharacter.Improvements[intOldIndex] = objImprovement;
            }
        }

        /// <summary>
        /// Move an Improvement Group TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveImprovementRoot(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
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
            
            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.ImprovementGroups.Move(objCharacter.ImprovementGroups.IndexOf(strLocation), intNewIndex);
        }
        #endregion

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strAltCode">Book code to search for.</param>
        /// <param name="strLanguage">Language to load.</param>
        public static string LanguageBookCodeFromAltCode(string strAltCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strAltCode))
            {
                XmlNode xmlOriginalCode = XmlManager.Load("books.xml", strLanguage).SelectSingleNode("/chummer/books/book[altcode = \"" + strAltCode + "\"]/code");
                return xmlOriginalCode?.InnerText ?? strAltCode;
            }
            return string.Empty;
        }

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="strLanguage">Language to load.</param>
        public static string LanguageBookShort(string strCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strCode))
            {
                XmlNode xmlAltCode = XmlManager.Load("books.xml", strLanguage).SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]/altcode");
                return xmlAltCode?.InnerText ?? strCode;
            }
            return string.Empty;
        }

        /// <summary>
        /// Book name (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="strLanguage">Language to load.</param>
        public static string LanguageBookLong(string strCode, string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(strCode))
            {
                XmlNode xmlBook = XmlManager.Load("books.xml", strLanguage).SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
                if (xmlBook != null)
                {
                    string strReturn = xmlBook["translate"]?.InnerText ?? xmlBook["name"]?.InnerText;
                    if (!string.IsNullOrWhiteSpace(strReturn))
                        return strReturn;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns an XPath Expression's string that searches an item's name for a string.
        /// </summary>
        /// <param name="strNeedle">String to look for</param>
        /// <param name="strNameElement">Name of the element that corresponds to the item's untranslated name.</param>
        /// <param name="strTranslateElement">Name of the element that corresponds to the item's translated name.</param>
        /// <param name="blnAddAnd">Whether to add " and " to the beginning of the search XPath</param>
        /// <returns></returns>
        public static string GenerateSearchXPath(string strNeedle, string strNameElement = "name", string strTranslateElement = "translate", bool blnAddAnd = true)
        {
            if (string.IsNullOrEmpty(strNeedle))
                return string.Empty;
            string strSearchText = strNeedle.ToUpper();
            // Treat everything as being uppercase so the search is case-insensitive.
            return string.Concat(
                blnAddAnd ? " and ((not(" : "((not(",
                strTranslateElement,
                ") and contains(translate(",
                strNameElement,
                ",'abcdefghijklmnopqrstuvwxyzàáâãäåæăąāçčćđďèéêëěęēėģğıìíîïīįķłĺļñňńņòóôõöőøřŕšśşțťùúûüűůūųẃẁŵẅýỳŷÿžźżß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆĂĄĀÇČĆĐĎÈÉÊËĚĘĒĖĢĞIÌÍÎÏĪĮĶŁĹĻÑŇŃŅÒÓÔÕÖŐØŘŔŠŚŞȚŤÙÚÛÜŰŮŪŲẂẀŴẄÝỲŶŸŽŹŻß'), \"",
                strSearchText,
                "\")) or contains(translate(",
                strTranslateElement,
                ",'abcdefghijklmnopqrstuvwxyzàáâãäåæăąāçčćđďèéêëěęēėģğıìíîïīįķłĺļñňńņòóôõöőøřŕšśşțťùúûüűůūųẃẁŵẅýỳŷÿžźżß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆĂĄĀÇČĆĐĎÈÉÊËĚĘĒĖĢĞIÌÍÎÏĪĮĶŁĹĻÑŇŃŅÒÓÔÕÖŐØŘŔŠŚŞȚŤÙÚÛÜŰŮŪŲẂẀŴẄÝỲŶŸŽŹŻß'), \"",
                strSearchText,
                "\"))");
        }

        #region PDF Functions
        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="sender">Control from which this method was called.</param>
        /// <param name="e">EventArgs used when this method was called.</param>
        public static void OpenPDFFromControl(object sender, EventArgs e)
        {
            if (sender is Control objControl)
                OpenPDF(objControl.Text);
        }
        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book coode and page number to open.</param>
        /// <param name="strPDFParamaters">PDF parameters to use. If empty, use GlobalOptions.PDFParameters.</param>
        /// <param name="strPDFAppPath">PDF parameters to use. If empty, use GlobalOptions.PDFAppPath.</param>
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
            string strBook = LanguageBookCodeFromAltCode(strTemp[0], GlobalOptions.Language);

            // Retrieve the sourcebook information including page offset and PDF application name.
            SourcebookInfo objBookInfo = GlobalOptions.SourcebookInfo.FirstOrDefault(objInfo => objInfo.Code == strBook && !string.IsNullOrEmpty(objInfo.Path));
            // If the sourcebook was not found, we can't open anything.
            if (objBookInfo == null)
                return;

            Uri uriPath = new Uri(objBookInfo.Path);
            // Check if the file actually exists.
            if (!File.Exists(uriPath.LocalPath))
                return;
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

        /// <summary>
        /// Gets a textblock from a given PDF document.
        /// </summary>
        /// <param name="strSource">Formatted Source to search, ie SR5 70</param>
        /// <param name="strText">String to search for as an opener</param>
        /// <returns></returns>
        public static string GetTextFromPDF(string strSource, string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return strText;

            string[] strTemp = strSource.Split(' ');
            if (strTemp.Length < 2)
                return string.Empty;
            if (!int.TryParse(strTemp[1], out int intPage))
                return string.Empty;

            // Make sure the page is actually a number that we can use as well as being 1 or higher.
            if (intPage < 1)
                return string.Empty;

            // Revert the sourcebook code to the one from the XML file if necessary.
            string strBook = LanguageBookCodeFromAltCode(strTemp[0], GlobalOptions.Language);

            // Retrieve the sourcebook information including page offset and PDF application name.
            SourcebookInfo objBookInfo = GlobalOptions.SourcebookInfo.FirstOrDefault(objInfo => objInfo.Code == strBook && !string.IsNullOrEmpty(objInfo.Path));
            // If the sourcebook was not found, we can't open anything.
            if (objBookInfo == null)
                return string.Empty;

            Uri uriPath = new Uri(objBookInfo.Path);
            // Check if the file actually exists.
            if (!File.Exists(uriPath.LocalPath))
                return string.Empty;
            intPage += objBookInfo.Offset;

            // due to the tag <nameonpage> for the qualities those variants are no longer needed,
            // as such the code would run at most half of the comparisons with the variants
            // but to be sure we find everything still strip unnecessary stuff after the ':' and any number in it.
            // PS: does any qualities have numbers on them? Or is that a chummer thing?
            string strTextToSearch = strText.Split(':')[0].Trim().TrimEnd(" I", " II", " III", " IV");

            PdfReader reader = objBookInfo.CachedPdfReader;
            List<string> lstStringFromPDF = new List<string>();
            int intTitleIndex = -1;
            int intBlockEndIndex = -1;
            int intExtraAllCapsInfo = 0;
            bool blnTitleWithColon = false; // it is either an uppercase title or title in a paragraph with a colon
            int intMaxPagesToRead = 3;  // parse at most 3 pages of content
            // Loop through each page, starting at the listed page + offset.
            for (; intPage <= reader.NumberOfPages; ++intPage)
            {
                // failsafe if something goes wrong, I guess no descrition takes more than two full pages?
                if (intMaxPagesToRead-- == 0)
                    break;

                int intProcessedStrings = lstStringFromPDF.Count;
                // each page should have its own text extraction strategy for it to work properly
                // this way we don't need to check for previous page appearing in the current page
                // https://stackoverflow.com/questions/35911062/why-are-gettextfrompage-from-itextsharp-returning-longer-and-longer-strings
                string strPageText = PdfTextExtractor.GetTextFromPage(reader, intPage, new SimpleTextExtractionStrategy());

                // don't trust it to be correct, trim all whitespace and remove empty strings before we even start
                lstStringFromPDF.AddRange(strPageText.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));

                for (int i = intProcessedStrings; i < lstStringFromPDF.Count; i++)
                {
                    // failsafe for languages that doesn't have case distincion (chinese, japanese, etc)
                    // there not much to be done for those languages, so stop after 10 continuous lines of uppercase text after our title
                    if (intExtraAllCapsInfo > 10)
                        break;

                    string strCurrentLine = lstStringFromPDF[i];
                    // we still haven't found anything
                    if (intTitleIndex == -1)
                    {
                        int intTextToSearchLenght = strTextToSearch.Length;
                        int intLineBiggerThanText = lstStringFromPDF[i].Length - intTextToSearchLenght;
                        int intTitleExtraLines = 0;
                        if (strCurrentLine.Length < intTextToSearchLenght)
                        {
                            // if the line is smaller first check if it contains the start of the text, before parsing the rest
                            if (strTextToSearch.StartsWith(strCurrentLine, StringComparison.OrdinalIgnoreCase))
                            {
                                // now just add more lines to it until it is enough
                                while (strCurrentLine.Length < intTextToSearchLenght && (i + intTitleExtraLines + 1) < lstStringFromPDF.Count)
                                {
                                    intTitleExtraLines++;
                                    // add the content plus a space
                                    strCurrentLine += ' ' + lstStringFromPDF[i + intTitleExtraLines];
                                }
                            }
                            else
                            {
                                // just go to the next line
                                continue;
                            }
                        }
                        // now either we have enough text to search or the page doesn't have anymore stuff and must give up
                        if (strCurrentLine.Length < intTextToSearchLenght)
                            break;

                        if (strCurrentLine.StartsWith(strTextToSearch, StringComparison.OrdinalIgnoreCase))
                        {
                            // WE FOUND SOMETHING! lets check what kind block we have
                            // if it is bigger it must have a ':' after the name otherwise it is probably the wrong stuff
                            if (strCurrentLine.Length > intTextToSearchLenght)
                            {
                                if (strCurrentLine[intTextToSearchLenght] == ':')
                                {
                                    intTitleIndex = i;
                                    blnTitleWithColon = true;
                                }
                            }
                            else // if it is not bigger it is the same lenght
                            {
                                // this must be an upper case title
                                if (strCurrentLine.ToUpperInvariant() == strCurrentLine)
                                {
                                    intTitleIndex = i;
                                    blnTitleWithColon = false;
                                }
                            }
                            // if we found the tile lets finish some things before finding the text block
                            if (intTitleIndex != -1)
                            {
                                // if we had to concatenate stuff lets fix the list of strings before continuing
                                if (intTitleExtraLines > 0)
                                {
                                    lstStringFromPDF[i] = strCurrentLine;
                                    lstStringFromPDF.RemoveRange(i + 1, intTitleExtraLines);
                                }
                            }

                        }
                    }
                    else // we already found our title, just go to the end of the block
                    {
                        // it is something in all caps we need to verify what it is
                        if (strCurrentLine.ToUpperInvariant() == strCurrentLine)
                        {
                            // if it is header or footer information just remove it
                            // do we also include lines with just numbers as probably page numbers??
                            if (strCurrentLine.All(c => char.IsDigit(c)) || strCurrentLine.Contains(">>") || strCurrentLine.Contains("<<"))
                            {
                                lstStringFromPDF.RemoveAt(i);
                                // rewind and go again
                                i--;
                                continue;
                            }
                            // if it is a line in all caps following the all caps title just skip it
                            if (!blnTitleWithColon && i == intTitleIndex + intExtraAllCapsInfo + 1)
                            {
                                intExtraAllCapsInfo++;
                                continue;
                            }
                            // if we are here it is the end of the block we found our end, mark it and be done
                            intBlockEndIndex = i;
                            break;
                        }
                        // if it is a title with colon we stop in the next line that has a colon
                        // this is not perfect, if we had bold information we could do more about that
                        if (blnTitleWithColon && strCurrentLine.Contains(':'))
                        {
                            intBlockEndIndex = i;
                            break;
                        }
                    }
                }
                // we scanned the first page and found nothing, just give up
                if (intTitleIndex == -1)
                    return string.Empty;
                // already have our end, quit searching here
                if (intBlockEndIndex != -1)
                    break;
            }

            // we have our textblock, lets format it and be done with it
            if (intBlockEndIndex != -1)
            {
                string[] strArray = lstStringFromPDF.ToArray();
                // if it is a "paragraph title" just concatenate everything
                if (blnTitleWithColon)
                    return string.Join(" ", strArray, intTitleIndex, intBlockEndIndex - intTitleIndex);
                // add the title
                string strResultContent = strArray[intTitleIndex] + '\n';
                // if we have extra info add it keeping the line breaks
                if (intExtraAllCapsInfo > 0)
                    strResultContent += string.Join("\n", strArray, intTitleIndex + 1, intExtraAllCapsInfo) + '\n';
                int intContentStartIndex = intTitleIndex + intExtraAllCapsInfo + 1;
                // this is the best we can do for now, it will still mangle spell blocks a bit
                for (int i = intContentStartIndex; i < intBlockEndIndex; i++)
                {
                    string strContentString = strArray[i];
                    switch (strContentString.Last())
                    {
                        case '-':
                            strResultContent += strContentString.Substring(0, strContentString.Length - 1);
                            break;
                        case '.':
                        case '!':
                        case '?':
                            strResultContent += strContentString + '\n';
                            break;
                        default:
                            strResultContent += strContentString + ' ';
                            break;
                    }
                }
                // In tooltips linebreaks should follow windows style \r\n
                return strResultContent.Replace("\n", Environment.NewLine);
            }
            return string.Empty;
        }
        #endregion
    }
}
