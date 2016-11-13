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
		private Character _objCharacter;

        public CommonFunctions()
        {
        }
        
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
		/// Locate a piece of Gear.
		/// </summary>
		/// <param name="strGuid">InternalId of the Gear to find.</param>
		/// <param name="lstGear">List of Gear to search.</param>
		public Gear FindGear(string strGuid, List<Gear> lstGear)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Gear objGear in lstGear)
			{
				if (objGear.InternalId == strGuid)
					objReturn = objGear;
				else
				{
					if (objGear.Children.Count > 0)
						objReturn = FindGear(strGuid, objGear.Children);
				}

				if (objReturn != null)
				{
					if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						return objReturn;
				}
			}

			objReturn = null;
			return objReturn;
		}
		/// <summary>
		/// Locate a Commlink.
		/// </summary>
		/// <param name="strGuid">InternalId of the Gear to find.</param>
		/// <param name="lstCommlink">List of Commlinks to search.</param>
		public Commlink FindCommlink(string strGuid, List<Gear> lstCommlink)
		{
			Commlink objReturn = new Commlink(_objCharacter);
			List<Gear> lstCheckGear = new List<Gear>();

			foreach (Gear objGear in lstCommlink)
			{
				if (objGear.Category == "Commlinks")
				{
					lstCheckGear.Add(objGear);
				}
			}
			foreach (Commlink objCommlink in lstCheckGear)
			{
				if (objCommlink.InternalId == strGuid)
					objReturn = objCommlink;
				else
				{
					if (objCommlink.Children.Count > 0)
						objReturn = FindCommlink(strGuid, objCommlink.Children);
				}

				if (objReturn != null)
				{
					if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						return objReturn;
				}
			}

			objReturn = null;
			return objReturn;
		}

		/// <summary>
		/// Locate a piece of Gear by matching on its Weapon ID.
		/// </summary>
		/// <param name="strGuid">InternalId of the Weapon to find.</param>
		/// <param name="lstGear">List of Gear to search.</param>
		public Gear FindGearByWeaponID(string strGuid, List<Gear> lstGear)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Gear objGear in lstGear)
			{
				if (objGear.WeaponID == strGuid)
					objReturn = objGear;
				else
				{
					if (objGear.Children.Count > 0)
						objReturn = FindGearByWeaponID(strGuid, objGear.Children);
				}

				if (objReturn != null)
				{
					if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						return objReturn;
				}
			}

			objReturn = null;
			return objReturn;
		}

		/// <summary>
		/// Locate a piece of Gear within the character's Vehicles.
		/// </summary>
		/// <param name="strGuid">InternalId of the Gear to find.</param>
		/// <param name="lstVehicles">List of Vehicles to search.</param>
		/// <param name="objFoundVehicle">Vehicle that the Gear was found in.</param>
		public Gear FindVehicleGear(string strGuid, List<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Vehicle objVehicle in lstVehicles)
			{
				objReturn = FindGear(strGuid, objVehicle.Gear);

				if (objReturn != null)
				{
					if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
					{
						objFoundVehicle = objVehicle;
						return objReturn;
					}
				}

				// Look for any Gear that might be attached to this Vehicle through Weapon Accessories or Cyberware.
				foreach (VehicleMod objMod in objVehicle.Mods)
				{
					// Weapon Accessories.
					WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
					objReturn = FindWeaponGear(strGuid, objMod.Weapons, out objAccessory);

					if (objReturn != null)
					{
						if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						{
							objFoundVehicle = objVehicle;
							return objReturn;
						}
					}

					// Cyberware.
					Cyberware objCyberware = new Cyberware(_objCharacter);
					objReturn = FindCyberwareGear(strGuid, objMod.Cyberware, out objCyberware);

					if (objReturn != null)
					{
						if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						{
							objFoundVehicle = objVehicle;
							return objReturn;
						}
					}
				}
			}

			objFoundVehicle = null;
			objReturn = null;
			return objReturn;
		}

		/// <summary>
		/// Locate a Vehicle within the character's Vehicles.
		/// </summary>
		/// <param name="strGuid">InternalId of the Vehicle to Find.</param>
		/// <param name="lstArmors">List of Vehicles to search.</param>
		public Vehicle FindVehicle(string strGuid, List<Vehicle> lstVehicles)
		{
			foreach (Vehicle objVehicle in lstVehicles)
			{
				if (objVehicle.InternalId == strGuid)
					return objVehicle;
			}

			return null;
		}

		/// <summary>
		/// Locate a VehicleMod within the character's Vehicles.
		/// </summary>
		/// <param name="strGuid">InternalId of the VehicleMod to find.</param>
		/// <param name="lstArmors">List of Vehicles to search.</param>
		/// <param name="objFoundArmor">Vehicle that the VehicleMod was found in.</param>
		public VehicleMod FindVehicleMod(string strGuid, List<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
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

			objFoundVehicle = null;
			return null;
		}

		/// <summary>
		/// Locate a Weapon within the character's Vehicles.
		/// </summary>
		/// <param name="strGuid">InteralId of the Weapon to find.</param>
		/// <param name="lstVehicles">List of Vehicles to search.</param>
		/// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
		public Weapon FindVehicleWeapon(string strGuid, List<Vehicle> lstVehicles, out Vehicle objFoundVehicle)
		{
			Weapon objReturn = new Weapon(_objCharacter);
			foreach (Vehicle objVehicle in lstVehicles)
			{
				objReturn = FindWeapon(strGuid, objVehicle.Weapons);
				if (objReturn != null)
				{
					objFoundVehicle = objVehicle;
					return objReturn;
				}

				foreach (VehicleMod objMod in objVehicle.Mods)
				{
					objReturn = FindWeapon(strGuid, objMod.Weapons);
					if (objReturn != null)
					{
						objFoundVehicle = objVehicle;
						return objReturn;
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
		public WeaponAccessory FindVehicleWeaponAccessory(string strGuid, List<Vehicle> lstVehicles)
		{
			WeaponAccessory objReturn = new WeaponAccessory(_objCharacter);
			foreach (Vehicle objVehicle in lstVehicles)
			{
				objReturn = FindWeaponAccessory(strGuid, objVehicle.Weapons);
				if (objReturn != null)
					return objReturn;

				foreach (VehicleMod objMod in objVehicle.Mods)
				{
					objReturn = FindWeaponAccessory(strGuid, objMod.Weapons);
					if (objReturn != null)
						return objReturn;
				}
			}

			return null;
		}

		/// <summary>
		/// Locate a piece of Cyberware within the character's Vehicles.
		/// </summary>
		/// <param name="strGuid">InternalId of the Cyberware to find.</param>
		/// <param name="lstVehicles">List of Vehicles to search.</param>
		public Cyberware FindVehicleCyberware(string strGuid, List<Vehicle> lstVehicles)
		{
			Cyberware objReturn = new Cyberware(_objCharacter);
			foreach (Vehicle objVehicle in lstVehicles)
			{
				foreach (VehicleMod objMod in objVehicle.Mods)
				{
					objReturn = FindCyberware(strGuid, objMod.Cyberware);
					if (objReturn != null)
						return objReturn;
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
		public Gear FindArmorGear(string strGuid, List<Armor> lstArmors, out Armor objFoundArmor)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Armor objArmor in lstArmors)
			{
				objReturn = FindGear(strGuid, objArmor.Gear);

				if (objReturn != null)
				{
					if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
					{
						objFoundArmor = objArmor;
						return objReturn;
					}
				}
			}

			objFoundArmor = null;
			return objReturn;
		}

		/// <summary>
		/// Locate a piece of Armor within the character's Armors.
		/// </summary>
		/// <param name="strGuid">InternalId of the Armor to Find.</param>
		/// <param name="lstArmors">List of Armors to search.</param>
		public Armor FindArmor(string strGuid, List<Armor> lstArmors)
		{
			foreach (Armor objArmor in lstArmors)
			{
				if (objArmor.InternalId == strGuid)
					return objArmor;
			}

			return null;
		}

		/// <summary>
		/// Locate an Armor Mod within the character's Armors.
		/// </summary>
		/// <param name="strGuid">InternalId of the ArmorMod to Find.</param>
		/// <param name="lstArmors">List of Armors to search.</param>
		public ArmorMod FindArmorMod(string strGuid, List<Armor> lstArmors)
		{
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
		public Gear FindCyberwareGear(string strGuid, List<Cyberware> lstCyberware, out Cyberware objFoundCyberware)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Cyberware objCyberware in lstCyberware)
			{
				objReturn = FindGear(strGuid, objCyberware.Gear);

				if (objReturn != null)
				{
					if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
					{
						objFoundCyberware = objCyberware;
						return objReturn;
					}
				}

				if (objCyberware.Children.Count > 0)
				{
					objReturn = FindCyberwareGear(strGuid, objCyberware.Children, out objFoundCyberware);
					if (objReturn != null)
					{
						if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						{
							objFoundCyberware = objCyberware;
							return objReturn;
						}
					}
				}
			}

			objFoundCyberware = null;
			return objReturn;
		}

		/// <summary>
		/// Locate a Weapon within the character's Weapons.
		/// </summary>
		/// <param name="strGuid">InternalId of the Weapon to find.</param>
		/// <param name="lstWeaopns">List of Weapons to search.</param>
		public Weapon FindWeapon(string strGuid, List<Weapon> lstWeaopns)
		{
			Weapon objReturn = new Weapon(_objCharacter);
			foreach (Weapon objWeapon in lstWeaopns)
			{
				if (objWeapon.InternalId == strGuid)
					return objWeapon;

				// Look within Underbarrel Weapons.
				objReturn = FindWeapon(strGuid, objWeapon.UnderbarrelWeapons);
				if (objReturn != null)
					return objReturn;
			}

			return null;
		}

		/// <summary>
		/// Locate a WeaponAccessory within the character's Weapons.
		/// </summary>
		/// <param name="strGuid">InternalId of the WeaponAccessory to find.</param>
		/// <param name="lstWeapons">List of Weapons to search.</param>
		public WeaponAccessory FindWeaponAccessory(string strGuid, List<Weapon> lstWeapons)
		{
			WeaponAccessory objReturn = new WeaponAccessory(_objCharacter);
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

			return null;
		}

		/// <summary>
		/// Locate a piece of Gear within the character's Weapons.
		/// </summary>
		/// <param name="strGuid">InternalId of the Gear to find.</param>
		/// <param name="lstWeapons">List of Weapons to search.</param>
		/// <param name="objFoundAccessory">WeaponAccessory that the Gear was found in.</param>
		public Gear FindWeaponGear(string strGuid, List<Weapon> lstWeapons, out WeaponAccessory objFoundAccessory)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Weapon objWeapon in lstWeapons)
			{
				foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
				{
					objReturn = FindGear(strGuid, objAccessory.Gear);

					if (objReturn != null)
					{
						if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
						{
							objFoundAccessory = objAccessory;
							return objReturn;
						}
					}
				}

				if (objWeapon.UnderbarrelWeapons.Count > 0)
				{
					objReturn = FindWeaponGear(strGuid, objWeapon.UnderbarrelWeapons, out objFoundAccessory);

					if (objReturn != null)
					{
						if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
							return objReturn;
					}
				}
			}

			objFoundAccessory = null;
			return objReturn;
		}

		/// <summary>
		/// Locate a Lifestyle within the character's Lifestyles.
		/// </summary>
		/// <param name="strGuid">InternalId of the Lifestyle to find.</param>
		/// <param name="lstLifestyles">List of Lifestyles to search.</param>
		public Lifestyle FindLifestyle(string strGuid, List<Lifestyle> lstLifestyles)
		{
			foreach (Lifestyle objLifestyle in lstLifestyles)
			{
				if (objLifestyle.InternalId == strGuid)
					return objLifestyle;
			}

			return null;
		}

		/// <summary>
		/// Locate a LifestyleQuality within the character's Lifestyles.
		/// </summary>
		/// <param name="strGuid">InternalId of the Lifestyle Quality to find.</param>
		/// <param name="lstLifestyleQualities">List of Lifestyle Qualities to search.</param>
		public LifestyleQuality FindLifestyleQuality(string strGuid, List<LifestyleQuality> lstLifestyleQualities)
		{
			foreach (LifestyleQuality objLifestyleQuality in lstLifestyleQualities)
			{
				if (objLifestyleQuality.InternalId == strGuid)
					return objLifestyleQuality;
			}

			return null;
		}
		/// <summary>
		/// Locate a piece of Cyberware within the character's Cyberware.
		/// </summary>
		/// <param name="strGuid">InternalId of the Cyberware to find.</param>
		/// <param name="lstCyberware">List of Cyberware to search.</param>
		public Cyberware FindCyberware(string strGuid, List<Cyberware> lstCyberware)
		{
			Cyberware objReturn = new Cyberware(_objCharacter);
			foreach (Cyberware objCyberware in lstCyberware)
			{
				if (objCyberware.InternalId == strGuid)
					return objCyberware;

				objReturn = FindCyberware(strGuid, objCyberware.Children);
				if (objReturn != null)
					return objReturn;
			}

			return null;
		}

		/// <summary>
		/// Locate a Complex Form within the character's Complex Forms.
		/// </summary>
		/// <param name="strGuid">InternalId of the Complex Form to find.</param>
		/// <param name="lstPrograms">List of Complex Forms to search.</param>
        public ComplexForm FindComplexForm(string strGuid, List<ComplexForm> lstPrograms)
		{
            foreach (ComplexForm objProgram in lstPrograms)
			{
				if (objProgram.InternalId == strGuid)
					return objProgram;
			}

			return null;
		}

		/// <summary>
		/// Locate a Spell within the character's Spells.
		/// </summary>
		/// <param name="strGuid">InternalId of the Spell to find.</param>
		/// <param name="lstSpells">List of Spells to search.</param>
		public Spell FindSpell(string strGuid, List<Spell> lstSpells)
		{
			foreach (Spell objSpell in lstSpells)
			{
				if (objSpell.InternalId == strGuid)
					return objSpell;
			}

			return null;
		}

		/// <summary>
		/// Locate a Critter Power within the character's Critter Powers.
		/// </summary>
		/// <param name="strGuid">InternalId of the Critter Power to find.</param>
		/// <param name="lstCritterPowers">List of Critter Powers to search.</param>
		public CritterPower FindCritterPower(string strGuid, List<CritterPower> lstCritterPowers)
		{
			foreach (CritterPower objPower in lstCritterPowers)
			{
				if (objPower.InternalId == strGuid)
					return objPower;
			}

			return null;
		}

		/// <summary>
		/// Locate a Quality within the character's Qualities.
		/// </summary>
		/// <param name="strGuid">InternalId of the Quality to find.</param>
		/// <param name="lstQualities">List of Qualities to search.</param>
		public Quality FindQuality(string strGuid, List<Quality> lstQualities)
		{
			foreach (Quality objQuality in lstQualities)
			{
				if (objQuality.InternalId == strGuid)
					return objQuality;
			}

			return null;
		}

		/// <summary>
		/// Locate a Metamagic within the character's Metamagics.
		/// </summary>
		/// <param name="strGuid">InternalId of the Metamagic to find.</param>
		/// <param name="lstMetamagics">List of Metamagics to search.</param>
		public Metamagic FindMetamagic(string strGuid, List<Metamagic> lstMetamagics)
		{
			foreach (Metamagic objMetamagic in lstMetamagics)
			{
				if (objMetamagic.InternalId == strGuid)
					return objMetamagic;
			}

			return null;
		}

        /// <summary>
        /// Locate a InitiationGrade within the character's InitiationGrades.
        /// </summary>
        /// <param name="strGuid">InternalId of the InitiationGrade to find.</param>
        /// <param name="lstInitiationGrades">List of InitiationGrades to search.</param>
        public InitiationGrade FindInitiationGrade(string strGuid, List<InitiationGrade> lstInitiationGrades)
        {
            foreach (InitiationGrade objInitiationGrade in lstInitiationGrades)
            {
                if (objInitiationGrade.InternalId == strGuid)
                    return objInitiationGrade;
            }

            return null;
        }

        /// <summary>
        /// Locate a Art within the character's Arts.
        /// </summary>
        /// <param name="strGuid">InternalId of the Art to find.</param>
        /// <param name="lstArts">List of Arts to search.</param>
        public Art FindArt(string strGuid, List<Art> lstArts)
        {
            foreach (Art objArt in lstArts)
            {
                if (objArt.InternalId == strGuid)
                    return objArt;
            }

            return null;
        }

        /// <summary>
        /// Locate an Enhancement within the character's Enhancements.
        /// </summary>
        /// <param name="strGuid">InternalId of the Art to find.</param>
        /// <param name="objCharacter">The character to search.</param>
        public Enhancement FindEnhancement(string strGuid, Character objCharacter)
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
            return null;
        }

        /// <summary>
        /// Locate a LimitModifier within the character's Limit Modifiers.
        /// </summary>
        /// <param name="strGuid">InternalId of the Metamagic to find.</param>
        /// <param name="lstMetamagics">List of Metamagics to search.</param>
        public LimitModifier FindLimitModifier(string strGuid, List<LimitModifier> lstLimitModifiers)
        {
            foreach (LimitModifier objLimitModifier in lstLimitModifiers)
            {
                if (objLimitModifier.InternalId == strGuid)
                    return objLimitModifier;
            }

            return null;
        }

		/// <summary>
		/// Locate a Martial Art within the character's Martial Arts.
		/// </summary>
		/// <param name="strName">Name of the Martial Art to find.</param>
		/// <param name="lstMartialArts">List of Martial Arts to search.</param>
		public MartialArt FindMartialArt(string strName, List<MartialArt> lstMartialArts)
		{
			foreach (MartialArt objArt in lstMartialArts)
			{
				if (objArt.Name == strName)
					return objArt;
			}

			return null;
		}

		/// <summary>
		/// Locate a Martial Art Advantage within the character's Martial Arts.
		/// </summary>
		/// <param name="strGuid">InternalId of the Martial Art Advantage to find.</param>
		/// <param name="lstMartialArts">List of Martial Arts to search.</param>
		/// <param name="objFoundMartialArt">MartialArt the Advantage was found in.</param>
		public MartialArtAdvantage FindMartialArtAdvantage(string strGuid, List<MartialArt> lstMartialArts, out MartialArt objFoundMartialArt)
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

			objFoundMartialArt = null;
			return null;
		}

		/// <summary>
		/// Locate a Martial Art Maneuver within the character's Martial Art Maneuvers.
		/// </summary>
		/// <param name="strGuid">InternalId of the Martial Art Maneuver to find.</param>
		/// <param name="lstManeuvers">List of Martial Art Maneuvers to search.</param>
		public MartialArtManeuver FindMartialArtManeuver(string strGuid, List<MartialArtManeuver> lstManeuvers)
		{
			foreach (MartialArtManeuver objManeuver in lstManeuvers)
			{
				if (objManeuver.InternalId == strGuid)
					return objManeuver;
			}

			return null;
		}

		/// <summary>
		/// Find a TreeNode in a TreeView based on its Tag.
		/// </summary>
		/// <param name="strGuid">InternalId of the Node to find.</param>
		/// <param name="treTree">TreeView to search.</param>
		public TreeNode FindNode(string strGuid, TreeView treTree)
		{
			TreeNode objFound = new TreeNode();
			foreach (TreeNode objNode in treTree.Nodes)
			{
				if (objNode.Tag.ToString() == strGuid)
					return objNode;
				else
				{
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
		public TreeNode FindNode(string strGuid, TreeNode objNode)
		{
			TreeNode objFound = new TreeNode();
			foreach (TreeNode objChild in objNode.Nodes)
			{
				if (objChild.Tag.ToString() == strGuid)
					return objChild;
				else
				{
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
		public List<Commlink> FindCharacterCommlinks(List<Gear> lstGear)
		{
			List<Commlink> lstReturn = new List<Commlink>();
			foreach (Gear objGear in lstGear)
			{
				if (objGear.GetType() == typeof(Commlink))
					lstReturn.Add((Commlink)objGear);

				if (objGear.Children.Count > 0)
				{
					// Retrieve the list of Commlinks in child items.
					List<Commlink> lstAppend = FindCharacterCommlinks(objGear.Children);
					if (lstAppend.Count > 0)
					{
						// Append the entries to the current list.
						foreach (Commlink objCommlink in lstAppend)
							lstReturn.Add(objCommlink);
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
		public void ReplaceHomeNodes(string strGuid, List<Gear>lstGear, List<Vehicle> lstVehicles)
		{
			foreach (Commlink objGear in lstGear)
			{
				if (objGear.HomeNode && (objGear.InternalId.ToString() != strGuid))
				{
					objGear.HomeNode = false;
				}
			}
			foreach (Vehicle objVehicle in lstVehicles)
			{
				if (objVehicle.HomeNode && (objVehicle.InternalId.ToString() != strGuid))
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
				TreeNode objRemoveNode = new TreeNode();
				foreach (TreeNode objWeaponNode in treWeapons.Nodes[0].Nodes)
				{
					if (objWeaponNode.Tag.ToString() == objGear.WeaponID)
						objRemoveNode = objWeaponNode;
				}
				treWeapons.Nodes.Remove(objRemoveNode);

				// Remove the Weapon from the Character.
				Weapon objRemoveWeapon = new Weapon(_objCharacter);
				foreach (Weapon objWeapon in _objCharacter.Weapons)
				{
					if (objWeapon.InternalId == objGear.WeaponID)
						objRemoveWeapon = objWeapon;
				}
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
							objPower.FreeLevels -= (objFocus.Rating / 4);
						}
					}
					_objCharacter.Foci.Remove(objFocus);
				}
			}

			// If a Stacked Focus is being removed, make sure the Stacked Foci and its bonuses are being removed.
			if (objGear.Category == "Stacked Focus")
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
							objRemoveNode = objWeaponNode;
					}
					objVehicleNode.Nodes.Remove(objRemoveNode);
				}
				
				// Remove the Weapon from the Vehicle.
				Weapon objRemoveWeapon = new Weapon(_objCharacter);
				foreach (Weapon objWeapon in objVehicle.Weapons)
				{
					if (objWeapon.InternalId == objGear.WeaponID)
						objRemoveWeapon = objWeapon;
				}
				objVehicle.Weapons.Remove(objRemoveWeapon);
			}
		}

		/// <summary>
		/// Verify that the user wants to delete an item.
		/// </summary>
		public bool ConfirmDelete(string strMessage)
		{
			if (!_objCharacter.Options.ConfirmDelete)
				return true;
			else
			{
				if (MessageBox.Show(strMessage, LanguageManager.Instance.GetString("MessageTitle_Delete"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
					return false;
				else
					return true;
			}
		}
		#endregion

		#region Tree Functions


		/// <summary>
		/// Build up the Tree for the current piece of Gear and all of its children.
		/// </summary>
		/// <param name="objGear">Gear to iterate through.</param>
		/// <param name="objNode">TreeNode to append to.</param>
		/// <param name="objMenu">ContextMenuStrip that the new TreeNodes should use.</param>
		public void BuildGearTree(Gear objGear, TreeNode objNode, ContextMenuStrip objMenu)
		{
			foreach (Gear objChild in objGear.Children)
			{
				TreeNode objChildNode = new TreeNode();
				objChildNode.Text = objChild.DisplayName;
				objChildNode.Tag = objChild.InternalId;
				objChildNode.ContextMenuStrip = objMenu;
				if (objChild.Notes != string.Empty)
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
		public void BuildCyberwareTree(Cyberware objCyberware, TreeNode objParentNode, ContextMenuStrip objMenu, ContextMenuStrip objGearMenu)
		{
				TreeNode objNode = new TreeNode();
				objNode.Text = objCyberware.DisplayName;
				objNode.Tag = objCyberware.InternalId;
				if (objCyberware.Notes != string.Empty)
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
					if (objGear.Notes != string.Empty)
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
		public void CreateArmorTreeNode(Armor objArmor, TreeView treArmor, ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
		{
			TreeNode objNode = new TreeNode();
			objNode.Text = objArmor.DisplayName;
			objNode.Tag = objArmor.InternalId;
			if (objArmor.Notes != string.Empty)
				objNode.ForeColor = Color.SaddleBrown;
			objNode.ToolTipText = objArmor.Notes;

			foreach (ArmorMod objMod in objArmor.ArmorMods)
			{
				TreeNode objChild = new TreeNode();
				objChild.Text = objMod.DisplayName;
				objChild.Tag = objMod.InternalId;
				objChild.ContextMenuStrip = cmsArmorMod;
				if (objMod.Notes != string.Empty)
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
				if (objGear.Notes != string.Empty)
					objChild.ForeColor = Color.SaddleBrown;
				objChild.ToolTipText = objGear.Notes;

				BuildGearTree(objGear, objChild, cmsArmorGear);

				objChild.ContextMenuStrip = cmsArmorGear;
				objNode.Nodes.Add(objChild);
				objNode.Expand();
			}

			TreeNode objParent = new TreeNode();
			if (objArmor.Location == "")
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
		/// <param name="cmsVehicleGear">ContextMenuStrip for Vehicle Gear Nodes.</param>
		public void CreateVehicleTreeNode(Vehicle objVehicle, TreeView treVehicles, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear)
		{
			TreeNode objNode = new TreeNode();
			objNode.Text = objVehicle.DisplayName;
			objNode.Tag = objVehicle.InternalId;
			if (objVehicle.Notes != string.Empty)
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
				if (objMod.Notes != string.Empty)
					objChildNode.ForeColor = Color.SaddleBrown;
				objChildNode.ToolTipText = objMod.Notes;

				// Cyberware.
				foreach (Cyberware objCyberware in objMod.Cyberware)
				{
					TreeNode objCyberwareNode = new TreeNode();
					objCyberwareNode.Text = objCyberware.DisplayName;
					objCyberwareNode.Tag = objCyberware.InternalId;
					if (objCyberware.Notes != string.Empty)
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
				if (objGear.Notes != string.Empty)
					objGearNode.ForeColor = Color.SaddleBrown;
				objGearNode.ToolTipText = objGear.Notes;

				BuildGearTree(objGear, objGearNode, cmsVehicleGear);

				objGearNode.ContextMenuStrip = cmsVehicleGear;

				TreeNode objParent = new TreeNode();
				if (objGear.Location == "")
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
		public void CreateWeaponTreeNode(Weapon objWeapon, TreeNode objWeaponsNode, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, string WeaponID = null)
		{
			TreeNode objNode = new TreeNode();
			objNode.Text = objWeapon.DisplayName;
			objNode.Tag = WeaponID ?? objWeapon.InternalId;
			if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || WeaponID != null)
				objNode.ForeColor = SystemColors.GrayText;
			if (objWeapon.Notes != string.Empty)
				objNode.ForeColor = Color.SaddleBrown;
			objNode.ToolTipText = objWeapon.Notes;

			// Add attached Weapon Accessories.
			foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
			{
				TreeNode objChild = new TreeNode();
				objChild.Text = objAccessory.DisplayName;
				objChild.Tag = objAccessory.InternalId;
				objChild.ContextMenuStrip = cmsWeaponAccessory;
				if (objAccessory.Notes != string.Empty)
					objChild.ForeColor = Color.SaddleBrown;
				objChild.ToolTipText = objAccessory.Notes;

				// Add any Gear attached to the Weapon Accessory.
				foreach (Gear objGear in objAccessory.Gear)
				{
					TreeNode objGearChild = new TreeNode();
					objGearChild.Text = objGear.DisplayName;
					objGearChild.Tag = objGear.InternalId;
					if (objGear.Notes != string.Empty)
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
			if (!objWeapon.IsUnderbarrelWeapon && objWeapon.Location != string.Empty)
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
			// The user must have specified the arguments of their PDF application in order to use this functionality.
			if (string.IsNullOrWhiteSpace(GlobalOptions.Instance.PDFParameters))
				return;

			string[] strTemp = strSource.Split(' ');
			string strBook = "";
			Uri uriPath = null;
			int intPage = 0;

			try
			{
				strBook = strTemp[0];
				string strPage = strTemp[1];

				// Make sure the page is actually a number that we can use as well as being 1 or higher.
				if (Convert.ToInt32(strPage) < 1)
					return;
				intPage = Convert.ToInt32(strPage);
			}
			catch
			{
				return;
			}

			// Revert the sourcebook code to the one from the XML file if necessary.
			if (_objCharacter != null)
				strBook = _objCharacter.Options.BookFromAltCode(strBook);

			// Retrieve the sourcebook information including page offset and PDF application name.
			bool blnFound = false;
			foreach (SourcebookInfo objInfo in GlobalOptions.Instance.SourcebookInfo.Where(objInfo => objInfo.Code == strBook).Where(objInfo => objInfo.Path != string.Empty))
			{
				blnFound = true;
				uriPath = new Uri(objInfo.Path);
				intPage += objInfo.Offset;
			}

			// If the sourcebook was not found, we can't open anything.
            if (!blnFound)
				return;

			string strParams = GlobalOptions.Instance.PDFParameters;
			strParams = strParams.Replace("{page}", intPage.ToString());
			strParams = strParams.Replace("{localpath}", uriPath.LocalPath);
			strParams = strParams.Replace("{absolutepath}", uriPath.AbsolutePath);
			ProcessStartInfo objProgress = new ProcessStartInfo();
			objProgress.FileName = GlobalOptions.Instance.PDFAppPath;
			objProgress.Arguments = strParams;
			Process.Start(objProgress);
		}
		#endregion

        #region Logging Functions
		[Obsolete("Use Log.Info()")]
        public void LogWrite(LogType logType, string strClass, string strLine)
        {
	        Log.Info(new object[] {logType, strLine}, "LEGACY_LOG_CALL", strClass);
			
        } 
        #endregion


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
                int eol = text.IndexOf(Environment.NewLine, pos);
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
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;

            // If no whitespace found, break at maximum length
            if (i < 0)
                return max;

            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }
	}
}