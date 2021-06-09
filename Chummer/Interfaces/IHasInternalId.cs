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
using System.Collections.Specialized;
using System.Windows.Forms;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public interface IHasInternalId
    {
        string InternalId { get; }
    }

    public static class InternalId
    {
        public static void RefreshChildrenGears(this IHasInternalId objParent, TreeView treGear, ContextMenuStrip cmsGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treGear == null)
                return;

            TreeNode nodParent = treGear.FindNodeByTag(objParent);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objGear, intNewIndex);
                            objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objGear.SetupChildrenGearsCollectionChanged(false, treGear);
                            nodParent.FindNodeByTag(objGear)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objGear.SetupChildrenGearsCollectionChanged(false, treGear);
                            nodParent.FindNodeByTag(objGear)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objGear, intNewIndex);
                            objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                            intNewIndex += 1;
                        }
                        treGear.SelectedNode = treGear.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNodeByTag(objGear)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objGear, intNewIndex);
                            intNewIndex += 1;
                        }
                        treGear.SelectedNode = treGear.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                        {
                            TreeNode objNode = nodParent.Nodes[i];
                            if (objNode.Tag is Gear objNodeGear)
                            {
                                if (!ReferenceEquals(objNodeGear.Parent, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }
                    }
                    break;
            }

            void AddToTree(Gear objGear, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objGear.CreateTreeNode(cmsGear);
                if (objNode == null)
                    return;
                if (objGear.Location == null)
                {
                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                }
                else
                {
                    TreeNode nodLocation = nodParent.FindNodeByTag(objGear.Location, false);
                    if (nodLocation != null)
                    {
                        if (intIndex >= 0)
                            nodLocation.Nodes.Insert(intIndex, objNode);
                        else
                            nodLocation.Nodes.Add(objNode);
                        nodLocation.Expand();
                    }
                    // Location Updating should be part of a separate method, so just add to parent instead
                    else
                    {
                        if (intIndex >= 0)
                            nodParent.Nodes.Insert(intIndex, objNode);
                        else
                            nodParent.Nodes.Add(objNode);
                        nodParent.Expand();
                    }
                }
                if (blnSingleAdd)
                    treGear.SelectedNode = objNode;
            }
        }

        public static void RefreshChildrenCyberware(this IHasInternalId objParent, TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treCyberware == null)
                return;

            TreeNode nodParent = treCyberware.FindNodeByTag(objParent);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objCyberware, intNewIndex);
                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                            nodParent.FindNodeByTag(objCyberware)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treCyberware.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                            nodParent.FindNodeByTag(objCyberware)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objCyberware, intNewIndex);
                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear);
                            intNewIndex += 1;
                        }
                        treCyberware.SelectedNode = treCyberware.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treCyberware.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNodeByTag(objCyberware)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objCyberware, intNewIndex);
                            intNewIndex += 1;
                        }
                        treCyberware.SelectedNode = treCyberware.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        VehicleMod objParentAsVehicleMod = objParent as VehicleMod;
                        for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                        {
                            TreeNode objNode = nodParent.Nodes[i];
                            if (objNode.Tag is Cyberware objNodeCyberware)
                            {
                                if (!ReferenceEquals(objNodeCyberware.Parent, objParent) && !ReferenceEquals(objNodeCyberware.ParentVehicle, objParent)
                                    && objParentAsVehicleMod?.Cyberware.Contains(objNodeCyberware) != true)
                                {
                                    objNode.Remove();
                                }
                            }
                        }
                    }
                    break;
            }

            void AddToTree(Cyberware objCyberware, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear);
                if (objNode == null)
                    return;

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treCyberware.SelectedNode = objNode;
            }
        }

        public static void RefreshChildrenWeapons(this IHasInternalId objParent, TreeView treWeapons, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treWeapons == null)
                return;

            TreeNode nodParent = treWeapons.FindNode(objParent.InternalId);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeapon, intNewIndex);
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                            nodParent.FindNode(objWeapon.InternalId)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                            nodParent.FindNode(objWeapon.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeapon, intNewIndex);
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                        treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNode(objWeapon.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeapon, intNewIndex);
                            intNewIndex += 1;
                        }
                        treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        nodParent.Nodes.Clear();
                        for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                        {
                            TreeNode objNode = nodParent.Nodes[i];
                            if (objNode.Tag is Weapon objNodeWeapon)
                            {
                                if (!ReferenceEquals(objNodeWeapon.Parent, objParent)
                                    && !ReferenceEquals(objNodeWeapon.ParentMount, objParent)
                                    && !ReferenceEquals(objNodeWeapon.ParentVehicle, objParent)
                                    && !ReferenceEquals(objNodeWeapon.ParentVehicleMod, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }
                    }
                    break;
            }

            void AddToTree(Weapon objWeapon, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objNode == null)
                    return;
                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treWeapons.SelectedNode = objNode;
            }
        }

        public static void RefreshWeaponAccessories(this IHasInternalId objParent, TreeView treWeapons, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treWeapons == null)
                return;

            TreeNode nodParent = treWeapons.FindNode(objParent.InternalId);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponAccessory, intNewIndex);
                            objWeaponAccessory.GearChildren.AddTaggedCollectionChanged(treWeapons, (x, y) => objWeaponAccessory.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, y));
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponAccessory.GearChildren.RemoveTaggedCollectionChanged(treWeapons);
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                            nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponAccessory.GearChildren.RemoveTaggedCollectionChanged(treWeapons);
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                            nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponAccessory, intNewIndex);
                            objWeaponAccessory.GearChildren.AddTaggedCollectionChanged(treWeapons, (x, y) => objWeaponAccessory.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, y));
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                        treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponAccessory, intNewIndex);
                            intNewIndex += 1;
                        }
                        treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                        {
                            TreeNode objNode = nodParent.Nodes[i];
                            if (objNode.Tag is WeaponAccessory objNodeAccessory)
                            {
                                if (!ReferenceEquals(objNodeAccessory.Parent, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }
                    }
                    break;
            }

            void AddToTree(WeaponAccessory objWeaponAccessory, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeaponAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objNode == null)
                    return;
                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treWeapons.SelectedNode = objNode;
            }
        }

        public static void RefreshVehicleMods(this IHasInternalId objParent, TreeView treVehicles, ContextMenuStrip cmsVehicleMod, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treVehicles == null || notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodParent = treVehicles.FindNodeByTag(objParent);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objVehicleMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles);
                            objVehicleMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objVehicleMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                            objVehicleMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                        {
                            TreeNode objNode = nodParent.Nodes[i];
                            if (objNode.Tag is VehicleMod objNodeMod)
                            {
                                if (!ReferenceEquals(objNodeMod.Parent, objParent) && !ReferenceEquals(objNodeMod.WeaponMountParent, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }
                    }
                    break;
            }

            void AddToTree(VehicleMod objVehicleMod, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objVehicleMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                if (objNode == null)
                    return;

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }

        public static void RefreshVehicleWeaponMounts(this IHasInternalId objParent, TreeView treVehicles, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treVehicles == null || notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodVehicleParent = treVehicles.FindNodeByTag(objParent);
            if (nodVehicleParent == null)
                return;
            TreeNode nodParent = nodVehicleParent.FindNode("String_WeaponMounts", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => objWeaponMount.RefreshVehicleMods(treVehicles, cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objWeaponMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y));
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                            objWeaponMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles);
                                objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            if (nodParent != null)
                            {
                                nodParent.FindNodeByTag(objWeaponMount)?.Remove();
                                if (nodParent.Nodes.Count == 0)
                                    nodParent.Remove();
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                            objWeaponMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => objWeaponMount.RefreshVehicleMods(treVehicles, cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objWeaponMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y));
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        if (nodParent != null)
                        {
                            for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodParent.Nodes[i];
                                if (objNode.Tag is WeaponMount objNodeWeaponMount)
                                {
                                    if (!ReferenceEquals(objNodeWeaponMount.Parent, objParent))
                                    {
                                        objNode.Remove();
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            void AddToTree(WeaponMount objWeaponMount, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeaponMount.CreateTreeNode(cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicleMod);
                if (objNode == null)
                    return;

                if (nodParent == null)
                {
                    nodParent = new TreeNode
                    {
                        Tag = "String_WeaponMounts",
                        Text = LanguageManager.GetString("String_WeaponMounts")
                    };
                    nodVehicleParent.Nodes.Insert(funcOffset?.Invoke() ?? 0, nodParent);
                    nodParent.Expand();
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }
    }
}
