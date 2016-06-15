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
	public class MainController
	{
		private Character _objCharacter;
		private CommonFunctions _objFunctions;
		private ImprovementManager _objImprovementManager;

		public MainController(Character objCharacter)
		{
			_objCharacter = objCharacter;
			_objImprovementManager = new ImprovementManager(_objCharacter);
			_objFunctions = new CommonFunctions(_objCharacter);
		}

		#region Enums
		public enum MentorType
		{
			Mentor = 0,
			Paragon = 1
		}
		#endregion

		#region Move TreeNodes
		/// <summary>
		/// Move a Gear TreeNode after Drag and Drop, changing its parent.
		/// </summary>
		/// <param name="intNewIndex">Node's new idnex.</param>
		/// <param name="objDestination">Destination Node.</param>
		public void MoveGearParent(int intNewIndex, TreeNode objDestination, TreeView treGear, ContextMenuStrip cmsGear)
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
			Gear objGear = new Gear(_objCharacter);
			objGear = _objFunctions.FindGear(treGear.SelectedNode.Tag.ToString(), _objCharacter.Gear);

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
				_objCharacter.Gear.Remove(objGear);
			else
				objGear.Parent.Children.Remove(objGear);

			if (objDestination.Level == 0)
			{
				// The Gear was moved to a location, so add it to the character instead.
				_objCharacter.Gear.Add(objGear);
				objGear.Location = objDestination.Text;
				objGear.Parent = null;
			}
			else
			{
				Gear objParent = new Gear(_objCharacter);
				// Locate the Gear that the item was dropped on.
				objParent = _objFunctions.FindGear(objDestination.Tag.ToString(), _objCharacter.Gear);

				// Add the Gear as a child of the destination Node and clear its location.
				objParent.Children.Add(objGear);
				objGear.Location = "";
				objGear.Parent = objParent;
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
		public void MoveGearNode(int intNewIndex, TreeNode objDestination, TreeView treGear)
		{
			Gear objGear = new Gear(_objCharacter);
			// Locate the currently selected piece of Gear.
			foreach (Gear objCharacterGear in _objCharacter.Gear)
			{
				if (objCharacterGear.InternalId == treGear.SelectedNode.Tag.ToString())
				{
					objGear = objCharacterGear;
					break;
				}
			}
			_objCharacter.Gear.Remove(objGear);
			if (intNewIndex > _objCharacter.Gear.Count)
				_objCharacter.Gear.Add(objGear);
			else
				_objCharacter.Gear.Insert(intNewIndex, objGear);

			TreeNode objNewParent = objDestination;
			while (objNewParent.Level > 0)
				objNewParent = objNewParent.Parent;

			TreeNode objOldParent = treGear.SelectedNode;
			while (objOldParent.Level > 0)
				objOldParent = objOldParent.Parent;

			// Change the Location on the Gear item.
			if (objNewParent.Text == LanguageManager.Instance.GetString("Node_SelectedGear"))
				objGear.Location = "";
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
		public void MoveGearRoot(int intNewIndex, TreeNode objDestination, TreeView treGear)
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

			string strLocation = "";
			// Locate the currently selected Location.
			foreach (string strCharacterLocation in _objCharacter.Locations)
			{
				if (strCharacterLocation == treGear.SelectedNode.Tag.ToString())
				{
					strLocation = strCharacterLocation;
					break;
				}
			}
			_objCharacter.Locations.Remove(strLocation);

			if (intNewIndex - 1 > _objCharacter.Locations.Count)
				_objCharacter.Locations.Add(strLocation);
			else
				_objCharacter.Locations.Insert(intNewIndex - 1, strLocation);

			TreeNode nodOldNode = treGear.SelectedNode;
			treGear.Nodes.Remove(nodOldNode);
			treGear.Nodes.Insert(intNewIndex, nodOldNode);
		}

		/// <summary>
		/// Move a Lifestyle TreeNode after Drag and Drop.
		/// </summary>
		/// <param name="intNewIndex">Node's new index.</param>
		/// <param name="objDestination">Destination Node.</param>
		public void MoveLifestyleNode(int intNewIndex, TreeNode objDestination, TreeView treLifestyles)
		{
			Lifestyle objLifestyle = new Lifestyle(_objCharacter);
			// Locate the currently selected Lifestyle.
			foreach (Lifestyle objCharacterLifestyle in _objCharacter.Lifestyles)
			{
				if (objCharacterLifestyle.Name == treLifestyles.SelectedNode.Tag.ToString())
				{
					objLifestyle = objCharacterLifestyle;
					break;
				}
			}
			_objCharacter.Lifestyles.Remove(objLifestyle);
			if (intNewIndex > _objCharacter.Lifestyles.Count)
				_objCharacter.Lifestyles.Add(objLifestyle);
			else
				_objCharacter.Lifestyles.Insert(intNewIndex, objLifestyle);

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
		public void MoveArmorNode(int intNewIndex, TreeNode objDestination, TreeView treArmor)
		{
			// Locate the currently selected Armor.
			Armor objArmor = _objFunctions.FindArmor(treArmor.SelectedNode.Tag.ToString(), _objCharacter.Armor);

			_objCharacter.Armor.Remove(objArmor);
			if (intNewIndex > _objCharacter.Armor.Count)
				_objCharacter.Armor.Add(objArmor);
			else
				_objCharacter.Armor.Insert(intNewIndex, objArmor);

			TreeNode objNewParent = objDestination;
			while (objNewParent.Level > 0)
				objNewParent = objNewParent.Parent;

			TreeNode objOldParent = treArmor.SelectedNode;
			while (objOldParent.Level > 0)
				objOldParent = objOldParent.Parent;

			// Change the Location on the Armor item.
			if (objNewParent.Text == LanguageManager.Instance.GetString("Node_SelectedArmor"))
				objArmor.Location = "";
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
		public void MoveArmorRoot(int intNewIndex, TreeNode objDestination, TreeView treArmor)
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

			string strLocation = "";
			// Locate the currently selected Location.
			foreach (string strCharacterLocation in _objCharacter.ArmorBundles)
			{
				if (strCharacterLocation == treArmor.SelectedNode.Tag.ToString())
				{
					strLocation = strCharacterLocation;
					break;
				}
			}
			_objCharacter.ArmorBundles.Remove(strLocation);

			if (intNewIndex - 1 > _objCharacter.ArmorBundles.Count)
				_objCharacter.ArmorBundles.Add(strLocation);
			else
				_objCharacter.ArmorBundles.Insert(intNewIndex - 1, strLocation);

			TreeNode nodOldNode = treArmor.SelectedNode;
			treArmor.Nodes.Remove(nodOldNode);
			treArmor.Nodes.Insert(intNewIndex, nodOldNode);
		}

		/// <summary>
		/// Move a Weapon TreeNode after Drag and Drop.
		/// </summary>
		/// <param name="intNewIndex">Node's new index.</param>
		/// <param name="objDestination">Destination Node.</param>
		public void MoveWeaponNode(int intNewIndex, TreeNode objDestination, TreeView treWeapons)
		{
			Weapon objWeapon = new Weapon(_objCharacter);
			// Locate the currently selected Weapon.
			foreach (Weapon objCharacterWeapon in _objCharacter.Weapons)
			{
				if (objCharacterWeapon.InternalId == treWeapons.SelectedNode.Tag.ToString())
				{
					objWeapon = objCharacterWeapon;
					break;
				}
			}
			_objCharacter.Weapons.Remove(objWeapon);
			if (intNewIndex > _objCharacter.Weapons.Count)
				_objCharacter.Weapons.Add(objWeapon);
			else
				_objCharacter.Weapons.Insert(intNewIndex, objWeapon);

			TreeNode objNewParent = objDestination;
			while (objNewParent.Level > 0)
				objNewParent = objNewParent.Parent;

			TreeNode objOldParent = treWeapons.SelectedNode;
			while (objOldParent.Level > 0)
				objOldParent = objOldParent.Parent;

			// Change the Location of the Weapon.
			if (objNewParent.Text == LanguageManager.Instance.GetString("Node_SelectedWeapons"))
				objWeapon.Location = "";
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
		public void MoveWeaponRoot(int intNewIndex, TreeNode objDestination, TreeView treWeapons)
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

			string strLocation = "";
			// Locate the currently selected Location.
			foreach (string strCharacterLocation in _objCharacter.WeaponLocations)
			{
				if (strCharacterLocation == treWeapons.SelectedNode.Tag.ToString())
				{
					strLocation = strCharacterLocation;
					break;
				}
			}
			_objCharacter.Locations.Remove(strLocation);

			if (intNewIndex - 1 > _objCharacter.WeaponLocations.Count)
				_objCharacter.WeaponLocations.Add(strLocation);
			else
				_objCharacter.WeaponLocations.Insert(intNewIndex - 1, strLocation);

			TreeNode nodOldNode = treWeapons.SelectedNode;
			treWeapons.Nodes.Remove(nodOldNode);
			treWeapons.Nodes.Insert(intNewIndex, nodOldNode);
		}

		/// <summary>
		/// Move a Vehicle TreeNode after Drag and Drop.
		/// </summary>
		/// <param name="intNewIndex">Node's new index.</param>
		/// <param name="objDestination">Destination Node.</param>
		public void MoveVehicleNode(int intNewIndex, TreeNode objDestination, TreeView treVehicles)
		{
			Vehicle objVehicle = new Vehicle(_objCharacter);
			// Locate the currently selected Vehicle.
			foreach (Vehicle objCharacterVehicle in _objCharacter.Vehicles)
			{
				if (objCharacterVehicle.InternalId == treVehicles.SelectedNode.Tag.ToString())
				{
					objVehicle = objCharacterVehicle;
					break;
				}
			}
			_objCharacter.Vehicles.Remove(objVehicle);
			if (intNewIndex > _objCharacter.Weapons.Count)
				_objCharacter.Vehicles.Add(objVehicle);
			else
				_objCharacter.Vehicles.Insert(intNewIndex, objVehicle);

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
		/// <param name="intNewIndex">Node's new index.</param>
		/// <param name="objDestination">Destination Node.</param>
		public void MoveVehicleGearParent(int intNewIndex, TreeNode objDestination, TreeView treVehicles, ContextMenuStrip cmsVehicleGear)
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
			Vehicle objDestinationVehicle = new Vehicle(_objCharacter);
			foreach (Vehicle objCharacterVehicle in _objCharacter.Vehicles)
			{
				if (objCharacterVehicle.InternalId == objVehicleNode.Tag.ToString())
				{
					objDestinationVehicle = objCharacterVehicle;
					break;
				}
			}

			// Make sure the destination is another piece of Gear or a Location.
			bool blnDestinationGear = true;
			bool blnDestinationLocation = false;
			Vehicle objTempVehicle = new Vehicle(_objCharacter);
			Gear objDestinationGear = _objFunctions.FindVehicleGear(objDestination.Tag.ToString(), _objCharacter.Vehicles, out objTempVehicle);
			if (objDestinationGear == null)
				blnDestinationGear = false;

			// Determine if this is a Location in the destination Vehicle.
			string strDestinationLocation = "";
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
			Vehicle objVehicle = new Vehicle(_objCharacter);
			Gear objGear = _objFunctions.FindVehicleGear(treVehicles.SelectedNode.Tag.ToString(), _objCharacter.Vehicles, out objVehicle);

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
				objVehicle.Gear.Remove(objGear);
			else
				objGear.Parent.Children.Remove(objGear);

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
				objGear.Location = "";
				objGear.Parent = objDestinationGear;
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
		public void MoveImprovementNode(int intNewIndex, TreeNode objDestination, TreeView treImprovements)
		{
			Improvement objImprovement = new Improvement();
			// Locate the currently selected Lifestyle.
			foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
			{
				if (objCharacterImprovement.SourceName == treImprovements.SelectedNode.Tag.ToString())
				{
					objImprovement = objCharacterImprovement;
					break;
				}
			}

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
				foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
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
		public void MoveImprovementRoot(int intNewIndex, TreeNode objDestination, TreeView treImprovements)
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

			string strLocation = "";
			// Locate the currently selected Group.
			foreach (string strCharacterGroup in _objCharacter.ImprovementGroups)
			{
				if (strCharacterGroup == treImprovements.SelectedNode.Tag.ToString())
				{
					strLocation = strCharacterGroup;
				}
			}
			_objCharacter.ImprovementGroups.Remove(strLocation);

			if (intNewIndex - 1 > _objCharacter.ImprovementGroups.Count)
				_objCharacter.ImprovementGroups.Add(strLocation);
			else
				_objCharacter.ImprovementGroups.Insert(intNewIndex - 1, strLocation);

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
		public void ClearSpellTab(TreeView treSpells)
		{
			// Run through all of the Spells and remove their Improvements.
			foreach (Spell objSpell in _objCharacter.Spells)
				_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Spell, objSpell.InternalId);

			// Clear the list of Spells.
			foreach (TreeNode objNode in treSpells.Nodes)
				objNode.Nodes.Clear();

			_objCharacter.Spells.Clear();
			_objCharacter.Spirits.Clear();

		}

		/// <summary>
		/// Clear all Adept tab elements from the character.
		/// </summary>
		public void ClearAdeptTab()
		{
			// Run through all of the Powers and remove their Improvements.
			foreach (Power objPower in _objCharacter.Powers)
				_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Power, objPower.InternalId);

			_objCharacter.Powers.Clear();
		}

		/// <summary>
		/// Clear all Technomancer tab elements from the character.
		/// </summary>
		public void ClearTechnomancerTab(TreeView treComplexForms)
		{
			// Run through all of the Complex Forms and remove their Improvements.
            foreach (ComplexForm objProgram in _objCharacter.ComplexForms)
				_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.ComplexForm, objProgram.InternalId);

			// Clear the list of Complex Forms.
			foreach (TreeNode objNode in treComplexForms.Nodes)
				objNode.Nodes.Clear();

			_objCharacter.Spirits.Clear();
            _objCharacter.ComplexForms.Clear();
		}

		/// <summary>
		/// Clear all Critter tab elements from the character.
		/// </summary>
		public void ClearCritterTab(TreeView treCritterPowers)
		{
			// Run through all of the Critter Powers and remove their Improvements.
			foreach (CritterPower objPower in _objCharacter.CritterPowers)
				_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.CritterPower, objPower.InternalId);

			// Clear the list of Critter Powers.
			foreach (TreeNode objNode in treCritterPowers.Nodes)
				objNode.Nodes.Clear();

			_objCharacter.CritterPowers.Clear();
		}

		/// <summary>
		/// Clera all Initiation tab elements from the character.
		/// </summary>
		public void ClearInitiationTab(TreeView treMetamagic)
		{
			// Remove any Metamagic/Echo Improvements.
			foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
			{
				_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Echo, objMetamagic.InternalId);
				_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Metamagic, objMetamagic.InternalId);
			}

			_objCharacter.InitiateGrade = 0;
			_objCharacter.SubmersionGrade = 0;
			_objCharacter.Metamagics.Clear();
			_objCharacter.InitiationGrades.Clear();

			treMetamagic.Nodes.Clear();
		}
		#endregion

		/// <summary>
		/// Populate the list of Bonded Foci.
		/// </summary>
		public void PopulateFocusList(TreeView treFoci)
		{
			treFoci.Nodes.Clear();
			int intFociTotal = 0;
			bool blnWarned = false;

			foreach (Gear objGear in _objCharacter.Gear)
			{
				if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci")
				{
					TreeNode objNode = new TreeNode();
					objNode.Text = objGear.DisplayName.Replace(LanguageManager.Instance.GetString("String_Rating"), LanguageManager.Instance.GetString("String_Force"));
					objNode.Tag = objGear.InternalId;
					foreach (Focus objFocus in _objCharacter.Foci)
					{
						if (objFocus.GearId == objGear.InternalId)
						{
							objNode.Checked = true;
							objFocus.Rating = objGear.Rating;
							intFociTotal += objFocus.Rating;
							// Do not let the number of BP spend on bonded Foci exceed MAG * 5.
							if (intFociTotal > _objCharacter.MAG.TotalValue * 5 && !_objCharacter.IgnoreRules)
							{
								// Mark the Gear a Bonded.
								foreach (Gear objCharacterGear in _objCharacter.Gear)
								{
									if (objCharacterGear.InternalId == objFocus.GearId)
										objCharacterGear.Bonded = false;
								}

								_objCharacter.Foci.Remove(objFocus);
								if (!blnWarned)
								{
									objNode.Checked = false;
									MessageBox.Show(LanguageManager.Instance.GetString("Message_FocusMaximumForce"), LanguageManager.Instance.GetString("MessageTitle_FocusMaximum"), MessageBoxButtons.OK, MessageBoxIcon.Information);
									blnWarned = true;
									break;
								}
							}
						}
					}
					treFoci.Nodes.Add(objNode);
				}
			}

			// Add Stacked Foci.
			foreach (Gear objGear in _objCharacter.Gear)
			{
				if (objGear.Category == "Stacked Focus")
				{
					foreach (StackedFocus objStack in _objCharacter.StackedFoci)
					{
						if (objStack.GearId == objGear.InternalId)
						{
							TreeNode objNode = new TreeNode();
							objNode.Text = LanguageManager.Instance.GetString("String_StackedFocus") + ": " + objStack.Name;
							objNode.Tag = objStack.InternalId;

							_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

							if (objStack.Bonded)
							{
								foreach (Gear objFociGear in objStack.Gear)
								{
									if (objFociGear.Extra != string.Empty)
										_objImprovementManager.ForcedValue = objFociGear.Extra;
									_objImprovementManager.CreateImprovements(Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort);
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
		public string CalculateFreeSpiritPowerPoints()
		{
			string strReturn = "";

			if (_objCharacter.Metatype == "Free Spirit" && !_objCharacter.IsCritter)
			{
				// PC Free Spirit.
				double dblPowerPoints = 0;

				foreach (CritterPower objPower in _objCharacter.CritterPowers)
				{
					if (objPower.CountTowardsLimit)
						dblPowerPoints += objPower.PowerPoints;
				}

				int intPowerPoints = _objCharacter.EDG.TotalValue + _objImprovementManager.ValueOf(Improvement.ImprovementType.FreeSpiritPowerPoints);

				// If the house rule to base Power Points on the character's MAG value instead, use the character's MAG.
				if (_objCharacter.Options.FreeSpiritPowerPointsMAG)
					intPowerPoints = _objCharacter.MAG.TotalValue + _objImprovementManager.ValueOf(Improvement.ImprovementType.FreeSpiritPowerPoints);

				strReturn = String.Format("{1} ({0} " + LanguageManager.Instance.GetString("String_Remaining") + ")", intPowerPoints - dblPowerPoints, intPowerPoints);
			}
			else
			{
				int intPowerPoints = 0;

				if (_objCharacter.Metatype == "Free Spirit")
				{
					// Critter Free Spirits have a number of Power Points equal to their EDG plus any Free Spirit Power Points Improvements.
					intPowerPoints = _objCharacter.EDG.Value + _objImprovementManager.ValueOf(Improvement.ImprovementType.FreeSpiritPowerPoints); ;
				}
				else if (_objCharacter.Metatype == "Ally Spirit")
				{
					// Ally Spirits get a number of Power Points equal to their MAG.
					intPowerPoints = _objCharacter.MAG.TotalValue;
				}
				else
				{
					// Spirits get 1 Power Point for every 3 full points of Force (MAG) they possess.
					double dblMAG = Convert.ToDouble(_objCharacter.MAG.TotalValue, GlobalOptions.Instance.CultureInfo);
					intPowerPoints = Convert.ToInt32(Math.Floor(dblMAG / 3.0));
				}

				int intUsed = 0;// _objCharacter.CritterPowers.Count - intExisting;
				foreach (CritterPower objPower in _objCharacter.CritterPowers)
				{
					if (objPower.Category != "Weakness" && objPower.CountTowardsLimit)
						intUsed++;
				}

				strReturn = String.Format("{1} ({0} " + LanguageManager.Instance.GetString("String_Remaining") + ")", intPowerPoints - intUsed, intPowerPoints);
			}

			return strReturn;
		}

		/// <summary>
		/// Calculate the number of Free Sprite Power Points used.
		/// </summary>
		public string CalculateFreeSpritePowerPoints()
		{
			string strReturn = "";

			// Free Sprite Power Points.
			double dblPowerPoints = 0;

			foreach (CritterPower objPower in _objCharacter.CritterPowers)
			{
				if (objPower.CountTowardsLimit)
					dblPowerPoints += 1;
			}

			int intPowerPoints = _objCharacter.EDG.TotalValue + _objImprovementManager.ValueOf(Improvement.ImprovementType.FreeSpiritPowerPoints);

			strReturn = String.Format("{1} ({0} " + LanguageManager.Instance.GetString("String_Remaining") + ")", intPowerPoints - dblPowerPoints, intPowerPoints);

			return strReturn;
		}

		/// <summary>
		/// Retrieve the information for the Mentor Spirit or Paragon the character might have.
		/// </summary>
		/// <param name="objMentorType">Type of feature to check for, either Mentor Spirit or Paragon.</param>
		public MentorSpirit MentorInformation(MentorType objMentorType)
		{
			MentorSpirit objReturn = new MentorSpirit();
			string strMentorSpirit = "";

			Quality objMentorQuality = new Quality(_objCharacter);

			// Look for the Mentor Spirit or Paragon Quality based on the type chosen.
			foreach (Quality objQuality in _objCharacter.Qualities)
			{
				if (objMentorType == MentorType.Mentor && objQuality.Name == "Mentor Spirit")
				{
					strMentorSpirit = objQuality.Extra;
					objMentorQuality = objQuality;
					break;
				}
				if (objMentorType == MentorType.Paragon && objQuality.Name == "Paragon")
				{
					strMentorSpirit = objQuality.Extra;
					objMentorQuality = objQuality;
					break;
				}
			}

			if (strMentorSpirit != "")
			{
				string strAdvantage = "";
				string strDisadvantage = "";

				XmlDocument objXmlDocument = new XmlDocument();
				XmlNode objXmlMentor;
				if (strMentorSpirit != "")
				{
					// Load the appropriate XML document.
					if (objMentorType == MentorType.Mentor)
						objXmlDocument = XmlManager.Instance.Load("mentors.xml");
					else
						objXmlDocument = XmlManager.Instance.Load("paragons.xml");

					objXmlMentor = objXmlDocument.SelectSingleNode("/chummer/mentors/mentor[name = \"" + strMentorSpirit + "\"]");

					// Build the list of advantages gained through the Mentor Spirit.
					if (objXmlMentor["altadvantage"] != null)
						strAdvantage = objXmlMentor["altadvantage"].InnerText;
					else
						strAdvantage = objXmlMentor["advantage"].InnerText;

					foreach (Improvement objImprovement in _objCharacter.Improvements)
					{
						if (objImprovement.SourceName == objMentorQuality.InternalId)
						{
							if (objImprovement.Notes != string.Empty)
								strAdvantage += " " + LanguageManager.Instance.TranslateExtra(objImprovement.Notes) + ".";
						}
					}

					// Build the list of disadvantages gained through the Mentor Spirit.
					if (objXmlMentor["altdisadvantage"] != null)
						strDisadvantage = objXmlMentor["altdisadvantage"].InnerText;
					else
						strDisadvantage = objXmlMentor["disadvantage"].InnerText;

					// Populate the Mentor Spirit object.
					objReturn.Name = strMentorSpirit;
					objReturn.Advantages = LanguageManager.Instance.GetString("Label_SelectMentorSpirit_Advantage") + " " + strAdvantage + "\n\n" + LanguageManager.Instance.GetString("Label_SelectMetamagic_Disadvantage") + " " + strDisadvantage;
				}
			}
			else
				return null;

			return objReturn;
		}

		/// <summary>
		/// Change the Equipped status of a piece of Gear and all of its children.
		/// </summary>
		/// <param name="objGear">Gear object to change.</param>
		/// <param name="blnEquipped">Whether or not the Gear should be marked as Equipped.</param>
		public void ChangeGearEquippedStatus(Gear objGear, bool blnEquipped)
		{
			if (blnEquipped)
			{
				// Add any Improvements from the Gear.
				if (objGear.Bonus != null)
				{
					bool blnAddImprovement = true;
					// If this is a Focus which is not bonded, don't do anything.
					if (objGear.Category != "Stacked Focus")
					{
						if (objGear.Category.EndsWith("Foci"))
							blnAddImprovement = objGear.Bonded;

						if (blnAddImprovement)
						{
							if (objGear.Extra != string.Empty)
								_objImprovementManager.ForcedValue = objGear.Extra;
							_objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort);
						}
					}
					else
					{
						// Stacked Foci need to be handled a little differently.
						foreach (StackedFocus objStack in _objCharacter.StackedFoci)
						{
							if (objStack.GearId == objGear.InternalId)
							{
								if (objStack.Bonded)
								{
									foreach (Gear objFociGear in objStack.Gear)
									{
										if (objFociGear.Extra != string.Empty)
											_objImprovementManager.ForcedValue = objFociGear.Extra;
										_objImprovementManager.CreateImprovements(Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort);
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
				if (objGear.Bonus != null)
				{
					if (objGear.Category != "Stacked Focus")
						_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Gear, objGear.InternalId);
					else
					{
						// Stacked Foci need to be handled a little differetnly.
						foreach (StackedFocus objStack in _objCharacter.StackedFoci)
						{
							if (objStack.GearId == objGear.InternalId)
							{
								foreach (Gear objFociGear in objStack.Gear)
									_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
							}
						}
					}
				}
			}

			if (objGear.Children.Count > 0)
				ChangeGearEquippedStatus(objGear.Children, blnEquipped);
		}

		/// <summary>
		/// Change the Equipped status of all Gear plugins. This should only be called from the other ChangeGearEquippedStatus and never used directly.
		/// </summary>
		/// <param name="lstGear">List of child Gear to change.</param>
		/// <param name="blnEquipped">Whether or not the children should be marked as Equipped.</param>
		public void ChangeGearEquippedStatus(List<Gear> lstGear, bool blnEquipped)
		{
			foreach (Gear objGear in lstGear)
			{
				ChangeGearEquippedStatus(objGear, blnEquipped);
			}
		}
	}
}