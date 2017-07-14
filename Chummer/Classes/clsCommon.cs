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

namespace Chummer
{
    public class CommonFunctions
    {
        #region Constructor and Instance
        private readonly Character _objCharacter;

        public CommonFunctions(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        public enum LogType
        {
            Message = 0,
            Alert = 1,
            Error = 2,
            Content = 3,
            Entering = 4,
            Exiting = 5,
        }
        #endregion

        #region Find Functions
        /// <summary>
        /// Locate an object (Needle) within a list (Haystack) based on only a GUID match.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T FindById<T>(string strGuid, List<T> lstHaystack) where T : IItemWithGuid
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (T objNeedle in lstHaystack)
                {
                    if (objNeedle.InternalId == strGuid)
                        return objNeedle;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Locate an object (Needle) within a list (Haystack) based on GUID match and non-zero name.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T FindByIdWithNameCheck<T>(string strGuid, List<T> lstHaystack) where T : INamedItemWithGuid
        {
            if (strGuid != Guid.Empty.ToString())
            {
                foreach (T objNeedle in lstHaystack)
                {
                    if (objNeedle.InternalId == strGuid && !string.IsNullOrEmpty(objNeedle.Name))
                        return objNeedle;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Locate an object (Needle) within a list and its children (Haystack) based on GUID match and non-zero name.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T DeepFindById<T>(string strGuid, List<T> lstHaystack) where T : INamedParentWithGuid<T>
        {
            if (strGuid != Guid.Empty.ToString())
            {
                T objNeedle;
                foreach (T objLoop in lstHaystack)
                {
                    if (objLoop.InternalId == strGuid)
                    {
                        objNeedle = objLoop;
                        if (!string.IsNullOrEmpty(objNeedle.Name))
                            return objNeedle;
                    }
                    if (objLoop.Children.Count > 0)
                    {
                        objNeedle = DeepFindById(strGuid, objLoop.Children);
                        if (!string.IsNullOrEmpty(objNeedle?.Name))
                            return objNeedle;
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Locate a Commlink.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCommlink">List of Commlinks to search.</param>
        public static Commlink FindCommlink(string strGuid, List<Gear> lstCommlink)
        {
            return DeepFindById(strGuid, lstCommlink) as Commlink;
        }

        /// <summary>
        /// Locate a piece of Gear by matching on its Weapon ID.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon to find.</param>
        /// <param name="lstGear">List of Gear to search.</param>
        public static Gear FindGearByWeaponID(string strGuid, List<Gear> lstGear)
        {
            if (strGuid == Guid.Empty.ToString())
                return null;
            Gear objReturn = null;
            foreach (Gear objGear in lstGear)
            {
                if (!string.IsNullOrEmpty(objGear.Name))
                {
                    if (objGear.WeaponID == strGuid)
                        objReturn = objGear;
                    else if (objGear.Children.Count > 0)
                        objReturn = FindGearByWeaponID(strGuid, objGear.Children);

                    if (!string.IsNullOrEmpty(objReturn?.Name))
                    {
                        return objReturn;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Gear was found in.</param>
        public static Gear FindVehicleGear(string strGuid, List<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
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
                            return objReturn;
                        }

                        // Cyberware.
                        Cyberware objCyberware;
                        objReturn = FindCyberwareGear(strGuid, objMod.Cyberware, out objCyberware);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundVehicle = objVehicle;
                            return objReturn;
                        }
                    }
                }
            }

            objFoundVehicle = null;
            return null;
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the VehicleMod to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the VehicleMod was found in.</param>
        public static VehicleMod FindVehicleMod(string strGuid, List<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
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
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(string strGuid, List<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
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
                            return objReturn;
                        }

                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            objReturn = DeepFindById(strGuid, objMod.Weapons);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
                            {
                                objFoundVehicle = objVehicle;
                                return objReturn;
                            }
                        }
                    }
                }
            }

            objFoundVehicle = null;
            return null;
        }

        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static WeaponAccessory FindVehicleWeaponAccessory(string strGuid, List<Vehicle> lstVehicles)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                WeaponAccessory objReturn;
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    if (!string.IsNullOrEmpty(objVehicle.Name))
                    {
                        objReturn = FindWeaponAccessory(strGuid, objVehicle.Weapons);
                        if (!string.IsNullOrEmpty(objReturn?.Name))
                            return objReturn;

                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            objReturn = FindWeaponAccessory(strGuid, objMod.Weapons);
                            if (!string.IsNullOrEmpty(objReturn?.Name))
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
        public static Cyberware FindVehicleCyberware(string strGuid, List<Vehicle> lstVehicles)
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
                                return objReturn;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="objFoundArmor">Armor that the Gear was found in.</param>
        public static Gear FindArmorGear(string strGuid, List<Armor> lstArmors, out Armor objFoundArmor)
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
                            return objReturn;
                        }
                    }
                }
            }

            objFoundArmor = null;
            return null;
        }

        /// <summary>
        /// Locate an Armor Mod within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the ArmorMod to Find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        public static ArmorMod FindArmorMod(string strGuid, List<Armor> lstArmors)
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
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public static Gear FindCyberwareGear(string strGuid, List<Cyberware> lstCyberware, out Cyberware objFoundCyberware)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Cyberware objCyberware in lstCyberware)
                {
                    objReturn = DeepFindById(strGuid, objCyberware.Gear);

                    if (!string.IsNullOrEmpty(objReturn?.Name))
                    {
                        objFoundCyberware = objCyberware;
                        return objReturn;
                    }

                    if (objCyberware.Children.Count > 0)
                    {
                        objReturn = FindCyberwareGear(strGuid, objCyberware.Children, out objFoundCyberware);
                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundCyberware = objCyberware;
                            return objReturn;
                        }
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
        public static WeaponAccessory FindWeaponAccessory(string strGuid, List<Weapon> lstWeapons)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                WeaponAccessory objReturn;
                foreach (Weapon objWeapon in lstWeapons)
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        if (objAccessory.InternalId == strGuid)
                            return objAccessory;
                    }

                    // Look within Underbarrel Weapons.
                    objReturn = FindWeaponAccessory(strGuid, objWeapon.UnderbarrelWeapons);
                    if (objReturn != null)
                        return objReturn;
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundAccessory">WeaponAccessory that the Gear was found in.</param>
        public static Gear FindWeaponGear(string strGuid, List<Weapon> lstWeapons, out WeaponAccessory objFoundAccessory)
        {
            if (strGuid != Guid.Empty.ToString())
            {
                Gear objReturn;
                foreach (Weapon objWeapon in lstWeapons)
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        objReturn = DeepFindById(strGuid, objAccessory.Gear);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                        {
                            objFoundAccessory = objAccessory;
                            return objReturn;
                        }
                    }

                    if (objWeapon.UnderbarrelWeapons.Count > 0)
                    {
                        objReturn = FindWeaponGear(strGuid, objWeapon.UnderbarrelWeapons, out objFoundAccessory);

                        if (!string.IsNullOrEmpty(objReturn?.Name))
                            return objReturn;
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
        /// <param name="objFoundMartialArt">MartialArt the Advantage was found in.</param>
        public static MartialArtAdvantage FindMartialArtAdvantage(string strGuid, List<MartialArt> lstMartialArts, out MartialArt objFoundMartialArt)
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
            if (strGuid != Guid.Empty.ToString())
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

        /// <summary>
        /// Find all of the Commlinks carried by the character.
        /// </summary>
        /// <param name="lstGear">List of Gear to search within for Commlinks.</param>
        public static List<Commlink> FindCharacterCommlinks(List<Gear> lstGear)
        {
            List<Commlink> lstReturn = new List<Commlink>();
            foreach (Gear objGear in lstGear)
            {
                if (objGear.GetType() == typeof(Commlink))
                    lstReturn.Add(objGear as Commlink);

                if (objGear.Children.Count > 0)
                {
                    // Retrieve the list of Commlinks in child items.
                    List<Commlink> lstAppend = FindCharacterCommlinks(objGear.Children);

                    if (lstAppend != null)
                    {
                        // Append the entries to the current list.
                        lstReturn.AddRange(lstAppend);
                    }
                }
            }

            return lstReturn;
        }

        /// <summary>
        /// Find and disable any other items selected as a home node.
        /// </summary>
        /// <param name="strGuid">GUID to whitelist when disabling other home nodes.</param>
        /// <param name="lstGear">List of Gear to search within for Home Node status.</param>
        /// <param name="lstVehicles">List of Gear to search within for Home Node status.</param>
        public static void ReplaceHomeNodes(string strGuid, List<Gear>lstGear, List<Vehicle> lstVehicles)
        {
            foreach (Gear objGear in lstGear)
            {
                if (objGear.HomeNode && objGear.InternalId != strGuid)
                {
                    objGear.HomeNode = false;
                }
            }
            foreach (Vehicle objVehicle in lstVehicles)
            {
                if (objVehicle.HomeNode && objVehicle.InternalId != strGuid)
                {
                    objVehicle.HomeNode = false;
                }
            }
        }
        #endregion

        #region Delete Functions
        /// <summary>
        /// Recursive method to delete a piece of Gear and its Improvements from the character.
        /// </summary>
        /// <param name="objGear">Gear to delete.</param>
        /// <param name="treWeapons">TreeView that holds the list of Weapons.</param>
        /// <param name="objImprovementManager">Improvement Manager the character is using.</param>
        public void DeleteGear(Gear objGear, TreeView treWeapons, ImprovementManager objImprovementManager)
        {
            // Remove any children the Gear may have.
            foreach (Gear objChild in objGear.Children)
                DeleteGear(objChild, treWeapons, objImprovementManager);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (objGear.WeaponID != Guid.Empty.ToString())
            {
                // Remove the Weapon from the TreeView.
                TreeNode objRemoveNode = null;
                foreach (TreeNode objWeaponNode in treWeapons.Nodes[0].Nodes)
                {
                    if (objWeaponNode.Tag.ToString() == objGear.WeaponID)
                    {
                        objRemoveNode = objWeaponNode;
                        break;
                    }
                }
                if (objRemoveNode != null)
                    treWeapons.Nodes.Remove(objRemoveNode);

                // Remove the Weapon from the Character.
                Weapon objRemoveWeapon = null;
                foreach (Weapon objWeapon in _objCharacter.Weapons)
                {
                    if (objWeapon.InternalId == objGear.WeaponID)
                    {
                        objRemoveWeapon = objWeapon;
                        break;
                    }
                }
                if (objRemoveWeapon != null)
                    _objCharacter.Weapons.Remove(objRemoveWeapon);
            }

            objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Gear, objGear.InternalId);

            // If a Focus is being removed, make sure the actual Focus is being removed from the character as well.
            if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci")
            {
                List<Focus> lstRemoveFoci = new List<Focus>();
                foreach (Focus objFocus in _objCharacter.Foci)
                {
                    if (objFocus.GearId == objGear.InternalId)
                        lstRemoveFoci.Add(objFocus);
                }
                foreach (Focus objFocus in lstRemoveFoci)
                {
                    foreach (Power objPower in _objCharacter.Powers)
                    {
                        if (objPower.BonusSource == objFocus.GearId)
                        {
                            //objPower.FreeLevels -= (objFocus.Rating / 4);
                        }
                    }
                    _objCharacter.Foci.Remove(objFocus);
                }
            }
            // If a Stacked Focus is being removed, make sure the Stacked Foci and its bonuses are being removed.
            else if (objGear.Category == "Stacked Focus")
            {
                foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                {
                    if (objStack.GearId == objGear.InternalId)
                    {
                        objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                        _objCharacter.StackedFoci.Remove(objStack);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Recursive method to delete a piece of Gear and from a Vehicle.
        /// </summary>
        /// <param name="objGear">Gear to delete.</param>
        /// <param name="treVehicles">TreeView that holds the list of Vehicles.</param>
        /// <param name="objVehicle">Vehicle to remove items from.</param>
        public void DeleteVehicleGear(Gear objGear, TreeView treVehicles, Vehicle objVehicle)
        {
            // Remove any children the Gear may have.
            foreach (Gear objChild in objGear.Children)
                DeleteVehicleGear(objChild, treVehicles, objVehicle);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (objGear.WeaponID != Guid.Empty.ToString())
            {
                // Remove the Weapon from the TreeView.
                TreeNode objRemoveNode = new TreeNode();
                foreach (TreeNode objVehicleNode in treVehicles.Nodes[0].Nodes)
                {
                    foreach (TreeNode objWeaponNode in objVehicleNode.Nodes)
                    {
                        if (objWeaponNode.Tag.ToString() == objGear.WeaponID)
                        {
                            objRemoveNode = objWeaponNode;
                            break;
                        }
                    }
                    objVehicleNode.Nodes.Remove(objRemoveNode);
                }
                // Remove the Weapon from the Vehicle.
                Weapon objRemoveWeapon = null;
                foreach (Weapon objWeapon in objVehicle.Weapons)
                {
                    if (objWeapon.InternalId == objGear.WeaponID)
                    {
                        objRemoveWeapon = objWeapon;
                        break;
                    }
                }
                if (objRemoveWeapon != null)
                    objVehicle.Weapons.Remove(objRemoveWeapon);
            }
        }

        /// <summary>
        /// Method to delete an Armor object.
        /// </summary>
        /// <param name="treArmor"></param>
        /// <param name="treWeapons"></param>
        /// <param name="_objImprovementManager"></param>
        public void DeleteArmor(TreeView treArmor, TreeView treWeapons, ImprovementManager _objImprovementManager)
        {
            if (!ConfirmDelete(LanguageManager.Instance.GetString("Message_DeleteArmor")))
                return;

            TreeNode objSelectedNode = treArmor.SelectedNode;
            TreeNodeCollection objWeaponNodes = treWeapons.Nodes;
            if (objSelectedNode == null)
                return;
            if (objSelectedNode.Level == 1)
            {
                Armor objArmor = FindByIdWithNameCheck(objSelectedNode.Tag.ToString(), _objCharacter.Armor);
                if (objArmor == null)
                    return;
                // Remove any Improvements created by the Armor and its children.
                foreach (ArmorMod objMod in objArmor.ArmorMods)
                {
                    // Remove the Cyberweapon created by the Mod if applicable.
                    if (objMod.WeaponID != Guid.Empty.ToString())
                    {
                        // Remove the Weapon from the Character.
                        foreach (Weapon objWeapon in _objCharacter.Weapons.Where(objWeapon => objWeapon.InternalId == objMod.WeaponID))
                        {
                            _objCharacter.Weapons.Remove(objWeapon);
                            // Remove the Weapon from the TreeView.
                            foreach (TreeNode objWeaponNode in objWeaponNodes.Cast<TreeNode>().Where(objWeaponNode => objWeaponNode.Tag.ToString() == objMod.WeaponID))
                            {
                                objWeaponNodes.Remove(objWeaponNode);
                            }
                        }
                    }

                    _objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                }
                _objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Armor, objArmor.InternalId);

                // Remove any Improvements created by the Armor's Gear.
                foreach (Gear objGear in objArmor.Gear)
                    DeleteGear(objGear, treWeapons, _objImprovementManager);

                List<Weapon> lstRemoveWeapons = new List<Weapon>();
                // Remove the Weapon from the Character.
                foreach (Weapon objWeapon in _objCharacter.Weapons.Where(objWeapon => objWeapon.InternalId == objArmor.WeaponID))
                {
                    lstRemoveWeapons.Add(objWeapon);
                    // Remove the Weapon from the TreeView.
                    foreach (TreeNode objWeaponNode in objWeaponNodes.Cast<TreeNode>().Where(objWeaponNode => objWeaponNode.Tag.ToString() == objArmor.WeaponID))
                    {
                        objWeaponNodes.Remove(objWeaponNode);
                    }
                }
                foreach (Weapon objWeapon in lstRemoveWeapons)
                {
                    _objCharacter.Weapons.Remove(objWeapon);
                }

                _objCharacter.Armor.Remove(objArmor);
            }
            else if (objSelectedNode.Level == 2)
            {
                ArmorMod objMod = FindArmorMod(objSelectedNode.Tag.ToString(), _objCharacter.Armor);
                if (objMod != null)
                {
                    // Remove the Cyberweapon created by the Mod if applicable.
                    if (objMod.WeaponID != Guid.Empty.ToString())
                    {
                        // Remove the Weapon from the TreeView.
                        TreeNode objRemoveNode = null;
                        foreach (TreeNode objWeaponNode in objWeaponNodes)
                        {
                            if (objWeaponNode.Tag.ToString() == objMod.WeaponID)
                            {
                                objRemoveNode = objWeaponNode;
                                break;
                            }
                        }
                        if (objRemoveNode != null)
                            objWeaponNodes.Remove(objRemoveNode);

                        // Remove the Weapon from the Character.
                        Weapon objRemoveWeapon = null;
                        foreach (Weapon objWeapon in _objCharacter.Weapons)
                        {
                            if (objWeapon.InternalId == objMod.WeaponID)
                            {
                                objRemoveWeapon = objWeapon;
                                break;
                            }
                        }
                        if (objRemoveWeapon != null)
                            _objCharacter.Weapons.Remove(objRemoveWeapon);
                    }

                    // Remove any Improvements created by the ArmorMod.
                    _objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                    objMod.Parent.ArmorMods.Remove(objMod);
                }
                else
                {
                    Armor objSelectedArmor;
                    Gear objGear = FindArmorGear(objSelectedNode.Tag.ToString(), _objCharacter.Armor,
                        out objSelectedArmor);
                    if (objGear != null)
                    {
                        DeleteGear(objGear, treWeapons, _objImprovementManager);
                        objSelectedArmor.Gear.Remove(objGear);
                    }
                }
            }
            else if (objSelectedNode.Level > 2)
            {
                Armor objSelectedArmor;
                Gear objGear = FindArmorGear(objSelectedNode.Tag.ToString(), _objCharacter.Armor, out objSelectedArmor);
                if (objGear != null)
                {
                    objGear.Parent.Children.Remove(objGear);
                    DeleteGear(objGear, treWeapons, _objImprovementManager);
                    objSelectedArmor.Gear.Remove(objGear);
                }
            }
            objSelectedNode.Remove();
        }

        /// <summary>
        /// Verify that the user wants to delete an item.
        /// </summary>
        public bool ConfirmDelete(string strMessage)
        {
            return !_objCharacter.Options.ConfirmDelete ||
                   MessageBox.Show(strMessage, LanguageManager.Instance.GetString("MessageTitle_Delete"),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
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
            foreach (Gear objChild in objGear.Children)
            {
                TreeNode objChildNode = new TreeNode();
                objChildNode.Text = objChild.DisplayName;
                objChildNode.Tag = objChild.InternalId;
                objChildNode.ContextMenuStrip = objMenu;
                if (!string.IsNullOrEmpty(objChild.Notes))
                    objChildNode.ForeColor = Color.SaddleBrown;
                objChildNode.ToolTipText = objChild.Notes;

                objNode.Nodes.Add(objChildNode);
                objNode.Expand();

                // Set the Gear's Parent.
                objChild.Parent = objGear;

                BuildGearTree(objChild, objChildNode, objMenu);
            }
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
                objChild.ContextMenuStrip = cmsArmorMod;
                if (!string.IsNullOrEmpty(objMod.Notes))
                    objChild.ForeColor = Color.SaddleBrown;
                objChild.ToolTipText = objMod.Notes;
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
        public static void CreateVehicleTreeNode(Vehicle objVehicle, TreeView treVehicles, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = objVehicle.DisplayName;
            objNode.Tag = objVehicle.InternalId;
            if (!string.IsNullOrEmpty(objVehicle.Notes))
                objNode.ForeColor = Color.SaddleBrown;
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
                if (objMod.IncludedInVehicle)
                    objChildNode.ForeColor = SystemColors.GrayText;
                if (!string.IsNullOrEmpty(objMod.Notes))
                    objChildNode.ForeColor = Color.SaddleBrown;
                objChildNode.ToolTipText = objMod.Notes;

                // Cyberware.
                foreach (Cyberware objCyberware in objMod.Cyberware)
                {
                    TreeNode objCyberwareNode = new TreeNode();
                    objCyberwareNode.Text = objCyberware.DisplayName;
                    objCyberwareNode.Tag = objCyberware.InternalId;
                    if (!string.IsNullOrEmpty(objCyberware.Notes))
                        objCyberwareNode.ForeColor = Color.SaddleBrown;
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
            if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || WeaponID != null)
                objNode.ForeColor = SystemColors.GrayText;
            if (!string.IsNullOrEmpty(objWeapon.Notes))
                objNode.ForeColor = Color.SaddleBrown;
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
                objChild.ToolTipText = objAccessory.Notes;

                // Add any Gear attached to the Weapon Accessory.
                foreach (Gear objGear in objAccessory.Gear)
                {
                    TreeNode objGearChild = new TreeNode();
                    objGearChild.Text = objGear.DisplayName;
                    objGearChild.Tag = objGear.InternalId;
                    if (!string.IsNullOrEmpty(objGear.Notes))
                        objGearChild.ForeColor = Color.SaddleBrown;
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

        #region PDF Functions

        /// <summary>
        /// Open a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book coode and page number to open.</param>
        public void OpenPDF(string strSource)
        {
            StaticOpenPDF(strSource, _objCharacter);
        }

        /// <summary>
        /// Static Function to open a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book coode and page number to open.</param>
        /// <param name="objCharacter">Character from which alternate sources should be fetched.</param>
        public static void StaticOpenPDF(string strSource, Character objCharacter = null)
        {
            // The user must have specified the arguments of their PDF application in order to use this functionality.
            if (string.IsNullOrWhiteSpace(GlobalOptions.Instance.PDFParameters))
                return;

            // The user must have specified the arguments of their PDF application in order to use this functionality.
            if (string.IsNullOrWhiteSpace(GlobalOptions.Instance.PDFAppPath))
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
            SourcebookInfo objBookInfo = GlobalOptions.Instance.SourcebookInfo.FirstOrDefault(
                objInfo => objInfo.Code == strBook && !string.IsNullOrEmpty(objInfo.Path));
            if (objBookInfo != null)
            {
                uriPath = new Uri(objBookInfo.Path);
                intPage += objBookInfo.Offset;
            }
            // If the sourcebook was not found, we can't open anything.
            else
                return;

            string strParams = GlobalOptions.Instance.PDFParameters;
            strParams = strParams.Replace("{page}", intPage.ToString());
            strParams = strParams.Replace("{localpath}", uriPath.LocalPath);
            strParams = strParams.Replace("{absolutepath}", uriPath.AbsolutePath);
            ProcessStartInfo objProgress = new ProcessStartInfo
            {
                FileName = GlobalOptions.Instance.PDFAppPath,
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