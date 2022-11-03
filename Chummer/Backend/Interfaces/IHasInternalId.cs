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
using System.Threading;
using System.Threading.Tasks;
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
        public static async ValueTask RefreshChildrenGears(this IHasInternalId objParent, TreeView treGear, ContextMenuStrip cmsGear, ContextMenuStrip cmsCustomGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treGear == null)
                return;

            TreeNode nodParent = await treGear.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
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
                            await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                            objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear, cmsCustomGear, funcMakeDirty);
                            ++intNewIndex;
                        }

                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objGear.SetupChildrenGearsCollectionChanged(false, treGear);
                            await treGear.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objGear)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        break;
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId
                            = (await treGear.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId
                              ?? string.Empty;
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objGear.SetupChildrenGearsCollectionChanged(false, treGear);
                            await treGear.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objGear)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                            objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear, cmsCustomGear, funcMakeDirty);
                            ++intNewIndex;
                        }

                        await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }

                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId
                            = (await treGear.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId
                              ?? string.Empty;
                        await treGear.DoThreadSafeAsync(() =>
                        {
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodParent.FindNodeByTag(objGear)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }

                case NotifyCollectionChangedAction.Reset:
                    {
                        await treGear.DoThreadSafeAsync(() =>
                        {
                            for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodParent.Nodes[i];
                                if (objNode.Tag is Gear objNodeGear && !ReferenceEquals(objNodeGear.Parent, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
            }

            async ValueTask AddToTree(Gear objGear, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objGear.CreateTreeNode(cmsCustomGear != null && objGear.AllowRename ? cmsCustomGear : cmsGear);
                if (objNode == null)
                    return;
                if (objGear.Location == null)
                {
                    await treGear.DoThreadSafeAsync(() =>
                    {
                        if (intIndex >= 0)
                            nodParent.Nodes.Insert(intIndex, objNode);
                        else
                            nodParent.Nodes.Add(objNode);
                        nodParent.Expand();
                    }, token: token).ConfigureAwait(false);
                }
                else
                {
                    await treGear.DoThreadSafeAsync(() =>
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
                    }, token: token).ConfigureAwait(false);
                }
                if (blnSingleAdd)
                    await treGear.DoThreadSafeAsync(x => x.SelectedNode = objNode, token: token).ConfigureAwait(false);
            }
        }

        public static async ValueTask RefreshChildrenCyberware(this IHasInternalId objParent, TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treCyberware == null)
                return;

            TreeNode nodParent = await treCyberware.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
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
                            await AddToTree(objCyberware, intNewIndex).ConfigureAwait(false);
                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear, funcMakeDirty);
                            ++intNewIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                            await treCyberware.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objCyberware)?.Remove(), token: token).ConfigureAwait(false);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treCyberware.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                            await treCyberware.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objCyberware)?.Remove(), token: token).ConfigureAwait(false);
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objCyberware, intNewIndex).ConfigureAwait(false);
                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear, funcMakeDirty);
                            ++intNewIndex;
                        }

                        await treCyberware.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (await treCyberware.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId ?? string.Empty;
                        await treCyberware.DoThreadSafeAsync(() =>
                        {
                            foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodParent.FindNodeByTag(objCyberware)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objCyberware, intNewIndex).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        await treCyberware.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    {
                        VehicleMod objParentAsVehicleMod = objParent as VehicleMod;
                        await treCyberware.DoThreadSafeAsync(() =>
                        {
                            for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodParent.Nodes[i];
                                if (objNode.Tag is Cyberware objNodeCyberware
                                    && !ReferenceEquals(objNodeCyberware.Parent, objParent)
                                    && !ReferenceEquals(objNodeCyberware.ParentVehicle, objParent)
                                    && objParentAsVehicleMod?.Cyberware.Contains(objNodeCyberware) != true)
                                {
                                    objNode.Remove();
                                }
                            }
                        }, token: token).ConfigureAwait(false);
                    }
                    break;
            }

            Task AddToTree(Cyberware objCyberware, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear);
                if (objNode == null)
                    return Task.CompletedTask;

                return treCyberware.DoThreadSafeAsync(x =>
                {
                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        x.SelectedNode = objNode;
                }, token: token);
            }
        }

        public static async ValueTask RefreshChildrenWeapons(this IHasInternalId objParent, TreeView treWeapons, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treWeapons == null)
                return;

            TreeNode nodParent = await treWeapons.DoThreadSafeFuncAsync(x => x.FindNode(objParent.InternalId), token: token).ConfigureAwait(false);
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
                            await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory,
                                                                            cmsWeaponAccessoryGear, funcMakeDirty);
                            ++intNewIndex;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                            await treWeapons.DoThreadSafeAsync(() => nodParent.FindNode(objWeapon.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId
                            = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)
                            ?.InternalId ?? string.Empty;
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                            await treWeapons.DoThreadSafeAsync(() => nodParent.FindNode(objWeapon.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory,
                                                                            cmsWeaponAccessoryGear, funcMakeDirty);
                            ++intNewIndex;
                        }

                        await treWeapons.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId
                            = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)
                            ?.InternalId ?? string.Empty;
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await treWeapons.DoThreadSafeAsync(() => nodParent.FindNode(objWeapon.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        await treWeapons.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        await treWeapons.DoThreadSafeAsync(() =>
                        {
                            nodParent.Nodes.Clear();
                            for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodParent.Nodes[i];
                                if (objNode.Tag is Weapon objNodeWeapon
                                    && !ReferenceEquals(objNodeWeapon.Parent, objParent)
                                    && !ReferenceEquals(objNodeWeapon.ParentMount, objParent)
                                    && !ReferenceEquals(objNodeWeapon.ParentVehicle, objParent)
                                    && !ReferenceEquals(objNodeWeapon.ParentVehicleMod, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
            }

            Task AddToTree(Weapon objWeapon, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objNode == null)
                    return Task.CompletedTask;

                return treWeapons.DoThreadSafeAsync(x =>
                {
                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        x.SelectedNode = objNode;
                }, token: token);
            }
        }

        public static async ValueTask RefreshWeaponAccessories(this IHasInternalId objParent, TreeView treWeapons, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (notifyCollectionChangedEventArgs == null || objParent == null || treWeapons == null)
                return;

            TreeNode nodParent = await treWeapons.DoThreadSafeFuncAsync(x => x.FindNode(objParent.InternalId), token: token).ConfigureAwait(false);
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
                            await AddToTree(objWeaponAccessory, intNewIndex).ConfigureAwait(false);

                            async void FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objWeaponAccessory.RefreshChildrenGears(
                                    treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            await objWeaponAccessory.GearChildren.AddTaggedCollectionChangedAsync(
                                treWeapons, FuncDelegateToAdd, token).ConfigureAwait(false);
                            if (funcMakeDirty != null)
                                await objWeaponAccessory.GearChildren.AddTaggedCollectionChangedAsync(treWeapons, funcMakeDirty, token).ConfigureAwait(false);
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear, null, funcMakeDirty);
                            ++intNewIndex;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await objWeaponAccessory.GearChildren.RemoveTaggedCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                            await treWeapons.DoThreadSafeAsync(
                                () => nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId
                            = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)
                            ?.InternalId ?? string.Empty;
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await objWeaponAccessory.GearChildren.RemoveTaggedCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                            await treWeapons.DoThreadSafeAsync(
                                () => nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeaponAccessory, intNewIndex).ConfigureAwait(false);

                            async void FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objWeaponAccessory.RefreshChildrenGears(
                                    treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            await objWeaponAccessory.GearChildren.AddTaggedCollectionChangedAsync(
                                treWeapons, FuncDelegateToAdd, token).ConfigureAwait(false);
                            if (funcMakeDirty != null)
                                await objWeaponAccessory.GearChildren.AddTaggedCollectionChangedAsync(treWeapons, funcMakeDirty, token).ConfigureAwait(false);
                            foreach (Gear objGear in objWeaponAccessory.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear, null, funcMakeDirty);
                            ++intNewIndex;
                        }

                        await treWeapons.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId
                            = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)
                            ?.InternalId ?? string.Empty;
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove();
                        }

                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (WeaponAccessory objWeaponAccessory in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeaponAccessory, intNewIndex).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        await treWeapons.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        await treWeapons.DoThreadSafeAsync(() =>
                        {
                            for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodParent.Nodes[i];
                                if (objNode.Tag is WeaponAccessory objNodeAccessory
                                    && !ReferenceEquals(objNodeAccessory.Parent, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
            }

            Task AddToTree(WeaponAccessory objWeaponAccessory, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeaponAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objNode == null)
                    return Task.CompletedTask;

                return treWeapons.DoThreadSafeAsync(x =>
                {
                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        x.SelectedNode = objNode;
                }, token: token);
            }
        }

        public static async ValueTask RefreshVehicleMods(this IHasInternalId objParent, TreeView treVehicles, ContextMenuStrip cmsVehicleMod, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (treVehicles == null || notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodParent = await treVehicles.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
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
                            await AddToTree(objVehicleMod, intNewIndex).ConfigureAwait(false);

                            async void FuncVehicleModCyberwareToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objVehicleMod.RefreshChildrenCyberware(
                                    treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            async void FuncVehicleModWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objVehicleMod.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            await objVehicleMod.Cyberware.AddTaggedCollectionChangedAsync(treVehicles, FuncVehicleModCyberwareToAdd, token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.AddTaggedCollectionChangedAsync(treVehicles, FuncVehicleModWeaponsToAdd, token).ConfigureAwait(false);
                            if (funcMakeDirty != null)
                            {
                                await objVehicleMod.Cyberware.AddTaggedCollectionChangedAsync(treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                                await objVehicleMod.Weapons.AddTaggedCollectionChangedAsync(treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                            }
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty);
                            ++intNewIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await objVehicleMod.Cyberware.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                            await objVehicleMod.Weapons.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            await treVehicles.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objVehicleMod)?.Remove(), token: token).ConfigureAwait(false);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await objVehicleMod.Cyberware.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                            await objVehicleMod.Weapons.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            await treVehicles.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objVehicleMod)?.Remove(), token: token).ConfigureAwait(false);
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objVehicleMod, intNewIndex).ConfigureAwait(false);

                            async void FuncVehicleModCyberwareToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objVehicleMod.RefreshChildrenCyberware(
                                    treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            async void FuncVehicleModWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objVehicleMod.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            await objVehicleMod.Cyberware.AddTaggedCollectionChangedAsync(treVehicles, FuncVehicleModCyberwareToAdd, token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.AddTaggedCollectionChangedAsync(treVehicles, FuncVehicleModWeaponsToAdd, token).ConfigureAwait(false);
                            if (funcMakeDirty != null)
                            {
                                await objVehicleMod.Cyberware.AddTaggedCollectionChangedAsync(treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                                await objVehicleMod.Weapons.AddTaggedCollectionChangedAsync(treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                            }
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty);
                            ++intNewIndex;
                        }

                        await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId
                            = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId ?? string.Empty;
                        await treVehicles.DoThreadSafeAsync(() =>
                        {
                            foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objVehicleMod, intNewIndex).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        await treVehicles.DoThreadSafeAsync(() =>
                        {
                            for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodParent.Nodes[i];
                                if (objNode.Tag is VehicleMod objNodeMod
                                    && !ReferenceEquals(objNodeMod.Parent, objParent)
                                    && !ReferenceEquals(objNodeMod.WeaponMountParent, objParent))
                                {
                                    objNode.Remove();
                                }
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
            }

            Task AddToTree(VehicleMod objVehicleMod, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objVehicleMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                if (objNode == null)
                    return Task.CompletedTask;

                return treVehicles.DoThreadSafeAsync(x =>
                {
                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        x.SelectedNode = objNode;
                }, token: token);
            }
        }

        public static async ValueTask RefreshVehicleWeaponMounts(this IHasInternalId objParent, TreeView treVehicles, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, NotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (treVehicles == null || notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodVehicleParent = await treVehicles.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
            if (nodVehicleParent == null)
                return;
            TreeNode nodParent = await treVehicles.DoThreadSafeFuncAsync(() => nodVehicleParent.FindNode("String_WeaponMounts", false), token: token).ConfigureAwait(false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeaponMount, intNewIndex).ConfigureAwait(false);

                            async void FuncWeaponMountVehicleModToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objWeaponMount.RefreshVehicleMods(treVehicles, cmsVehicleMod, cmsCyberware,
                                                                        cmsCyberwareGear, cmsVehicleWeapon,
                                                                        cmsVehicleWeaponAccessory,
                                                                        cmsVehicleWeaponAccessoryGear, null, y, funcMakeDirty, token: token).ConfigureAwait(false);
                            async void FuncWeaponMountWeaponToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objWeaponMount.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            await objWeaponMount.Mods.AddTaggedCollectionChangedAsync(treVehicles,
                                FuncWeaponMountVehicleModToAdd, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.AddTaggedCollectionChangedAsync(
                                treVehicles, FuncWeaponMountWeaponToAdd, token).ConfigureAwait(false);
                            if (funcMakeDirty != null)
                            {
                                await objWeaponMount.Mods.AddTaggedCollectionChangedAsync(treVehicles,
                                    funcMakeDirty, token).ConfigureAwait(false);
                                await objWeaponMount.Weapons.AddTaggedCollectionChangedAsync(
                                    treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                            }
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                async void FuncWeaponMountVehicleModCyberwareToAdd(
                                    object x, NotifyCollectionChangedEventArgs y) =>
                                    await objMod.RefreshChildrenCyberware(
                                        treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                                async void FuncWeaponMountVehicleModWeaponsToAdd(
                                    object x, NotifyCollectionChangedEventArgs y) =>
                                    await objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                                        cmsVehicleWeaponAccessory,
                                                                        cmsVehicleWeaponAccessoryGear,
                                                                        () => objMod.Cyberware.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                                await objMod.Cyberware.AddTaggedCollectionChangedAsync(
                                    treVehicles, FuncWeaponMountVehicleModCyberwareToAdd, token).ConfigureAwait(false);
                                await objMod.Weapons.AddTaggedCollectionChangedAsync(
                                    treVehicles, FuncWeaponMountVehicleModWeaponsToAdd, token).ConfigureAwait(false);
                                if (funcMakeDirty != null)
                                {
                                    await objMod.Cyberware.AddTaggedCollectionChangedAsync(
                                        treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                                    await objMod.Weapons.AddTaggedCollectionChangedAsync(
                                        treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                                }
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty);
                            }
                            ++intNewIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await objWeaponMount.Mods.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                await objMod.Cyberware.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                await objMod.Weapons.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            if (nodParent != null)
                            {
                                await treVehicles.DoThreadSafeAsync(() =>
                                {
                                    nodParent.FindNodeByTag(objWeaponMount)?.Remove();
                                    if (nodParent.Nodes.Count == 0)
                                        nodParent.Remove();
                                }, token: token).ConfigureAwait(false);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            await objWeaponMount.Mods.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                await objMod.Cyberware.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                await objMod.Weapons.RemoveTaggedCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }

                            await treVehicles.DoThreadSafeAsync(() => nodParent?.FindNodeByTag(objWeaponMount)?.Remove(), token: token).ConfigureAwait(false);
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeaponMount, intNewIndex).ConfigureAwait(false);

                            async void FuncWeaponMountVehicleModToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objWeaponMount.RefreshVehicleMods(treVehicles, cmsVehicleMod, cmsCyberware,
                                                                        cmsCyberwareGear, cmsVehicleWeapon,
                                                                        cmsVehicleWeaponAccessory,
                                                                        cmsVehicleWeaponAccessoryGear, null, y, funcMakeDirty, token: token).ConfigureAwait(false);
                            async void FuncWeaponMountWeaponToAdd(object x, NotifyCollectionChangedEventArgs y) =>
                                await objWeaponMount.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                            await objWeaponMount.Mods.AddTaggedCollectionChangedAsync(treVehicles,
                                FuncWeaponMountVehicleModToAdd, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.AddTaggedCollectionChangedAsync(
                                treVehicles, FuncWeaponMountWeaponToAdd, token).ConfigureAwait(false);
                            if (funcMakeDirty != null)
                            {
                                await objWeaponMount.Mods.AddTaggedCollectionChangedAsync(treVehicles,
                                    funcMakeDirty, token).ConfigureAwait(false);
                                await objWeaponMount.Weapons.AddTaggedCollectionChangedAsync(
                                    treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                            }
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                async void FuncWeaponMountVehicleModCyberwareToAdd(
                                    object x, NotifyCollectionChangedEventArgs y) =>
                                    await objMod.RefreshChildrenCyberware(
                                        treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                                async void FuncWeaponMountVehicleModWeaponsToAdd(
                                    object x, NotifyCollectionChangedEventArgs y) =>
                                    await objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                                        cmsVehicleWeaponAccessory,
                                                                        cmsVehicleWeaponAccessoryGear,
                                                                        () => objMod.Cyberware.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                                await objMod.Cyberware.AddTaggedCollectionChangedAsync(
                                    treVehicles, FuncWeaponMountVehicleModCyberwareToAdd, token).ConfigureAwait(false);
                                await objMod.Weapons.AddTaggedCollectionChangedAsync(
                                    treVehicles, FuncWeaponMountVehicleModWeaponsToAdd, token).ConfigureAwait(false);
                                if (funcMakeDirty != null)
                                {
                                    await objMod.Cyberware.AddTaggedCollectionChangedAsync(
                                        treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                                    await objMod.Weapons.AddTaggedCollectionChangedAsync(
                                        treVehicles, funcMakeDirty, token).ConfigureAwait(false);
                                }
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty);
                            }
                            ++intNewIndex;
                        }
                        await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        await treVehicles.DoThreadSafeAsync(() =>
                        {
                            foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            await AddToTree(objWeaponMount, intNewIndex).ConfigureAwait(false);
                            ++intNewIndex;
                        }
                        await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    {
                        if (nodParent != null)
                        {
                            await treVehicles.DoThreadSafeAsync(() =>
                            {
                                for (int i = nodParent.Nodes.Count - 1; i >= 0; --i)
                                {
                                    TreeNode objNode = nodParent.Nodes[i];
                                    if (objNode.Tag is WeaponMount objNodeWeaponMount
                                        && !ReferenceEquals(objNodeWeaponMount.Parent, objParent))
                                    {
                                        objNode.Remove();
                                    }
                                }
                            }, token: token).ConfigureAwait(false);
                        }
                    }
                    break;
            }

            async ValueTask AddToTree(WeaponMount objWeaponMount, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeaponMount.CreateTreeNode(cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicleMod);
                if (objNode == null)
                    return;

                if (nodParent == null)
                {
                    nodParent = new TreeNode
                    {
                        Tag = "String_WeaponMounts",
                        Text = await LanguageManager.GetStringAsync("String_WeaponMounts", token: token).ConfigureAwait(false)
                    };
                    await treVehicles.DoThreadSafeAsync(() =>
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        nodVehicleParent.Nodes.Insert(funcOffset?.Invoke() ?? 0, nodParent);
                        nodParent.Expand();
                    }, token: token).ConfigureAwait(false);
                }

                await treVehicles.DoThreadSafeAsync(x =>
                {
                    if (nodParent == null)
                        return;
                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        x.SelectedNode = objNode;
                }, token: token).ConfigureAwait(false);
            }
        }
    }
}
