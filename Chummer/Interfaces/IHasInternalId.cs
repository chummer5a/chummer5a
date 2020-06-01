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
                        nodParent.Nodes.Clear();
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
                        nodParent.Nodes.Clear();
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
                            objWeaponAccessory.Gear.AddTaggedCollectionChanged(treWeapons, (x, y) => objWeaponAccessory.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, y));
                            foreach (Gear objGear in objWeaponAccessory.Gear)
                                objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponAccessory.Gear.RemoveTaggedCollectionChanged(treWeapons);
                            foreach (Gear objGear in objWeaponAccessory.Gear)
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
                            objWeaponAccessory.Gear.RemoveTaggedCollectionChanged(treWeapons);
                            foreach (Gear objGear in objWeaponAccessory.Gear)
                                objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                            nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponAccessory, intNewIndex);
                            objWeaponAccessory.Gear.AddTaggedCollectionChanged(treWeapons, (x, y) => objWeaponAccessory.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, y));
                            foreach (Gear objGear in objWeaponAccessory.Gear)
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
                        nodParent.Nodes.Clear();
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
    }
}
