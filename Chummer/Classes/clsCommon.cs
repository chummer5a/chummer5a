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
                WeaponAccessory objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    objReturn = objVehicle.Weapons.FindWeaponAccessory(strGuid);
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
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
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
        public static void MoveGearParent(Character objCharacter, TreeNode objDestination, TreeView treGear)
        {
            TreeNode objClone = treGear.SelectedNode;
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = objDestination; objCheckNode != null && objCheckNode.Level >= objDestination.Level; objCheckNode = objCheckNode.Parent)
                if (objCheckNode == objClone)
                    return;

            string strSelectedId = objClone.Tag.ToString();
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

            // Remove the current Node.
            objClone.Remove();

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
            TreeNode objClone = treGear.SelectedNode;
            string strSelectedId = objClone.Tag.ToString();
            Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.InternalId == strSelectedId);
            objCharacter.Gear.Remove(objGear);
            if (intNewIndex > objCharacter.Gear.Count)
                objCharacter.Gear.Add(objGear);
            else
                objCharacter.Gear.Insert(intNewIndex, objGear);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;
            
            // Change the Location on the Gear item.
            if (objNewParent.Tag.ToString() == "Node_SelectedGear")
                objGear.Location = string.Empty;
            else
                objGear.Location = objNewParent.Text;

            objClone.Remove();
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

            TreeNode nodOldNode = treGear.SelectedNode;
            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.GearLocations.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.GearLocations.Count)
                objCharacter.GearLocations.Add(strLocation);
            else
                objCharacter.GearLocations.Insert(intNewIndex - 1, strLocation);

            nodOldNode.Remove();
            treGear.Nodes.Insert(intNewIndex, nodOldNode);
        }

        /// <summary>
        /// Move a Lifestyle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveLifestyleNode(Character objCharacter, int intNewIndex, TreeNode objDestination, TreeView treLifestyles)
        {
            TreeNode objClone = treLifestyles.SelectedNode;
            string strSelectedId = objClone.Tag.ToString();
            Lifestyle objLifestyle = objCharacter.Lifestyles.FirstOrDefault(x => x.Name == strSelectedId);
            objCharacter.Lifestyles.Remove(objLifestyle);
            if (intNewIndex > objCharacter.Lifestyles.Count)
                objCharacter.Lifestyles.Add(objLifestyle);
            else
                objCharacter.Lifestyles.Insert(intNewIndex, objLifestyle);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            objClone.Remove();
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
            TreeNode objClone = treArmor.SelectedNode;
            string strSelectedId = objClone.Tag.ToString();
            // Locate the currently selected Armor.
            Armor objArmor = objCharacter.Armor.FindById(strSelectedId);

            objCharacter.Armor.Remove(objArmor);
            if (intNewIndex > objCharacter.Armor.Count)
                objCharacter.Armor.Add(objArmor);
            else
                objCharacter.Armor.Insert(intNewIndex, objArmor);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            // Change the Location on the Armor item.
            if (objNewParent.Tag.ToString() == "Node_SelectedArmor")
                objArmor.Location = string.Empty;
            else
                objArmor.Location = objNewParent.Text;

            objClone.Remove();
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

            TreeNode nodOldNode = treArmor.SelectedNode;
            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.ArmorLocations.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.ArmorLocations.Count)
                objCharacter.ArmorLocations.Add(strLocation);
            else
                objCharacter.ArmorLocations.Insert(intNewIndex - 1, strLocation);
            
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
            TreeNode objClone = treWeapons.SelectedNode;
            string strSelectedId = objClone.Tag.ToString();
            Weapon objWeapon = objCharacter.Weapons.FirstOrDefault(x => x.InternalId == strSelectedId);
            objCharacter.Weapons.Remove(objWeapon);
            if (intNewIndex > objCharacter.Weapons.Count)
                objCharacter.Weapons.Add(objWeapon);
            else
                objCharacter.Weapons.Insert(intNewIndex, objWeapon);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            // Change the Location of the Weapon.
            if (objNewParent.Tag.ToString() == "Node_SelectedWeapons")
                objWeapon.Location = string.Empty;
            else
                objWeapon.Location = objNewParent.Text;

            objClone.Remove();
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

            TreeNode nodOldNode = treWeapons.SelectedNode;
            string strLocation = nodOldNode.Tag.ToString();
            objCharacter.GearLocations.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.WeaponLocations.Count)
                objCharacter.WeaponLocations.Add(strLocation);
            else
                objCharacter.WeaponLocations.Insert(intNewIndex - 1, strLocation);
            
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
            string strSelectedId = objCyberwareNode.Tag.ToString();
            Cyberware objCyberware = objCharacter.Cyberware.DeepFindById(strSelectedId);
            VehicleMod objOldParentVehicleMod = null;
            if (objCyberware == null)
            {
                objCyberware = objCharacter.Vehicles.FindVehicleCyberware(strSelectedId, out objOldParentVehicleMod);
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

            objCyberwareNode.Remove();
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
            TreeNode objClone = treVehicles.SelectedNode;
            string strSelectedId = objClone.Tag.ToString();
            Vehicle objVehicle = objCharacter.Vehicles.FirstOrDefault(x => x.InternalId == strSelectedId);
            objCharacter.Vehicles.Remove(objVehicle);
            if (intNewIndex > objCharacter.Vehicles.Count)
                objCharacter.Vehicles.Add(objVehicle);
            else
                objCharacter.Vehicles.Insert(intNewIndex, objVehicle);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            // Change the Location on the Gear item.
            if (objNewParent.Tag.ToString() == "Node_SelectedVehicles")
                objVehicle.Location = string.Empty;
            else
                objVehicle.Location = objNewParent.Text;

            objClone.Remove();
            objNewParent.Nodes.Insert(intNewIndex, objClone);
            objNewParent.Expand();
        }

        /// <summary>
        /// Move a Vehicle Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="objDestination">Destination Node.</param>
        public static void MoveVehicleGearParent(Character objCharacter, TreeNode objDestination, TreeView treVehicles)
        {
            TreeNode objClone = treVehicles.SelectedNode;
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = objDestination; objCheckNode != null && objCheckNode.Level >= objDestination.Level; objCheckNode = objCheckNode.Parent)
                if (objCheckNode == objClone)
                    return;

            // Determine if this is a Location.
            TreeNode objVehicleNode = objDestination;
            do
            {
                objVehicleNode = objVehicleNode.Parent;
            } while (objVehicleNode.Level > 1);

            // Get a reference to the destination Vehicle.
            Vehicle objDestinationVehicle = objCharacter.Vehicles.FindById(objVehicleNode.Tag.ToString());

            // Make sure the destination is another piece of Gear or a Location.
            Gear objDestinationGear = objCharacter.Vehicles.FindVehicleGear(objDestination.Tag.ToString());

            // Determine if this is a Location in the destination Vehicle.
            string strDestinationLocation = objDestinationVehicle.Locations.FirstOrDefault(x => x == objDestination.Tag.ToString());

            if (string.IsNullOrEmpty(strDestinationLocation) && objDestinationGear == null)
                return;

            // Locate the currently selected piece of Gear.
            Gear objGear = objCharacter.Vehicles.FindVehicleGear(objClone.Tag.ToString(), out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);

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
            if (objGear.Parent != null)
            {
                objGear.Parent.Children.Remove(objGear);
                objGear.Parent.RefreshMatrixAttributeArray();
            }
            else if (objCyberware != null)
            {
                objCyberware.Gear.Remove(objGear);
                objCyberware.RefreshMatrixAttributeArray();
            }
            else if (objWeaponAccessory != null)
                objWeaponAccessory.Gear.Remove(objGear);
            else
            {
                objVehicle.Gear.Remove(objGear);
                objVehicle.RefreshMatrixAttributeArray();
            }

            if (!string.IsNullOrEmpty(strDestinationLocation))
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

            // Remove the current Node.
            objClone.Remove();

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
            TreeNode objClone = treImprovements.SelectedNode;
            string strSelectedId = objClone?.Tag.ToString();
            Improvement objImprovement = objCharacter.Improvements.FirstOrDefault(x => x.SourceName == strSelectedId);

            TreeNode objNewParent = objDestination;
            while (objNewParent.Level > 0)
                objNewParent = objNewParent.Parent;

            // Change the Group on the Custom Improvement.
            objImprovement.CustomGroup = objNewParent.Text;

            objClone.Remove();
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

            TreeNode nodOldNode = treImprovements.SelectedNode;
            string strLocation = nodOldNode.Tag.ToString();
            // Locate the currently selected Group.
            objCharacter.ImprovementGroups.Remove(strLocation);

            if (intNewIndex - 1 > objCharacter.ImprovementGroups.Count)
                objCharacter.ImprovementGroups.Add(strLocation);
            else
                objCharacter.ImprovementGroups.Insert(intNewIndex - 1, strLocation);

            nodOldNode.Remove();
            treImprovements.Nodes.Insert(intNewIndex, nodOldNode);
        }
        #endregion

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
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
            
            List<string> lstTextsToSearch = new List<string>();
            foreach (string strLoopText in strText.Split(':'))
            {
                string strLoopTextTrimmed = strLoopText.Trim();
                lstTextsToSearch.Add(strLoopTextTrimmed.ToUpperInvariant());
                lstTextsToSearch.Add($"{strLoopTextTrimmed}:");
                string strTextWithoutNumerals = strLoopTextTrimmed.TrimEnd(" I", " II", " III", " IV");
                if (strTextWithoutNumerals != strLoopTextTrimmed)
                {
                    lstTextsToSearch.Add(strTextWithoutNumerals.ToUpperInvariant());
                    lstTextsToSearch.Add($"{strTextWithoutNumerals}:");
                }
            }

            StringBuilder strbldReturn = new StringBuilder();
            PdfReader reader = objBookInfo.CachedPdfReader;
            ITextExtractionStrategy its = new SimpleTextExtractionStrategy();
            string strOldPageText = string.Empty;
            // Loop through each page, starting at the listed page + offset.
            for (; intPage <= reader.NumberOfPages; ++intPage)
            {
                string strPageText = PdfTextExtractor.GetTextFromPage(reader, intPage, its);

                strPageText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(strPageText))).TrimStart(strOldPageText);

                strOldPageText = strPageText;

                // Sometimes names are split across multiple lines. This shimmer removes the newline characters between words that are written in all caps.
                for (int intNewlineIndex = strPageText.IndexOf('\n'); intNewlineIndex != -1; intNewlineIndex = intNewlineIndex + 1 < strPageText.Length ? strPageText.IndexOf('\n', intNewlineIndex + 1) : -1)
                {
                    string strFirstHalf = strPageText.Substring(0, intNewlineIndex).TrimEnd();
                    int intLastWhitespace = Math.Max(strFirstHalf.LastIndexOf(' '), strFirstHalf.LastIndexOf('\n'));
                    string strFirstHalfLastWord = strFirstHalf.Substring(intLastWhitespace + 1);
                    if (strFirstHalfLastWord == strFirstHalfLastWord.ToUpperInvariant())
                    {
                        string strSecondHalf = intNewlineIndex < strPageText.Length ? strPageText.Substring(intNewlineIndex + 1).TrimStart() : string.Empty;
                        intLastWhitespace = Math.Min(strSecondHalf.IndexOf(' '), strSecondHalf.IndexOf('\n'));
                        string strSecondHalfFirstWord = intLastWhitespace == -1 ? strSecondHalf : strSecondHalf.Substring(0, intLastWhitespace);
                        if (!strSecondHalfFirstWord.StartsWith("BONUS") && strSecondHalfFirstWord == strSecondHalfFirstWord.ToUpperInvariant())
                        {
                            strPageText = strFirstHalf + ' ' + strSecondHalf;
                        }
                    }
                }
                
                // If our string builder is empty, look for our text to know where to start, otherwise we're continuing our textblock from a previous page
                if (strbldReturn.Length == 0)
                {
                    int intTextLocation = -1;
                    foreach (string strNeedle in lstTextsToSearch)
                    {
                        intTextLocation = strPageText.IndexOf(strNeedle, StringComparison.Ordinal);
                        if (intTextLocation != -1)
                            break;
                    }
                    // Listed start page didn't contain the text we're looking for and we're not looking at a second page. Fail out (relatively) early.
                    if (intTextLocation == -1)
                    {
                        goto EndPdfReading;
                    }
                    // Found the relevant string, so trim everything before it. 
                    strPageText = strPageText.Substring(intTextLocation).TrimStart(lstTextsToSearch.ToArray());
                }

                string[] astrOut = strPageText.Split('\n');
                string strStack = string.Empty;
                for (int i = 0; i < astrOut.Length; i++)
                {
                    string strLoop = astrOut[i];
                    if (string.IsNullOrWhiteSpace(strLoop))
                        continue;
                    bool blnIsBonusLine = strLoop.StartsWith("BONUS") || strLoop.StartsWith("COST");
                    // We found an ALLCAPS string element that isn't the title. We've found our full textblock.
                    if (!blnIsBonusLine && strLoop == strLoop.ToUpperInvariant())
                    {
                        // The ALLCAPS element is actually a table or the bottom of the page, so continue fetching text from the next page instead of terminating
                        if (strLoop.Contains("TABLE") || strLoop.Contains(">>"))
                            break;
                        else
                            goto EndPdfReading;
                    }

                    // Add to the existing string. TODO: Something to preserve newlines that we actually want?
                    strbldReturn.Append(strLoop);
                    if (strLoop.EndsWith('-'))
                        strbldReturn.Length -= 1;
                    else if (blnIsBonusLine)
                        strbldReturn.Append('\n');
                    else
                        strbldReturn.Append(' ');
                }
            }
            EndPdfReading:
            return strbldReturn.ToString().NormalizeWhiteSpace();
        }
        #endregion
    }
}
