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
        public static async Task RefreshChildrenGearsClearBindings(this IHasInternalId objParent, TreeView treGear, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (objParent == null || treGear == null)
                return;

            foreach (Gear objGear in e.OldItems)
            {
                await objGear.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token).ConfigureAwait(false);
            }
        }

        public static async Task RefreshChildrenGears(this IHasInternalId objParent, TreeView treGear, ContextMenuStrip cmsGear, ContextMenuStrip cmsCustomGear, Func<Task<int>> funcOffset, NotifyCollectionChangedEventArgs e, AsyncNotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (e == null || objParent == null || treGear == null)
                return;

            TreeNode nodParent = await treGear.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
            if (nodParent == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Gear objGear in e.NewItems)
                        {
                            await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                            await objGear.SetupChildrenGearsCollectionChangedAsync(true, treGear, cmsGear, cmsCustomGear, funcMakeDirty, token).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Gear objGear in e.OldItems)
                        {
                            await objGear.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token).ConfigureAwait(false);
                            await treGear.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objGear)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        break;
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId
                            = (await treGear.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId
                              ?? string.Empty;
                        foreach (Gear objGear in e.OldItems)
                        {
                            await objGear.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token).ConfigureAwait(false);
                            await treGear.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objGear)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Gear objGear in e.NewItems)
                        {
                            await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                            await objGear.SetupChildrenGearsCollectionChangedAsync(true, treGear, cmsGear, cmsCustomGear, funcMakeDirty, token).ConfigureAwait(false);
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
                            foreach (Gear objGear in e.OldItems)
                            {
                                nodParent.FindNodeByTag(objGear)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Gear objGear in e.NewItems)
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
                TreeNode objNode = await objGear.CreateTreeNode(cmsGear, cmsCustomGear, token).ConfigureAwait(false);
                if (objNode != null)
                {
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
                        await treGear.DoThreadSafeAsync(x => x.SelectedNode = objNode, token: token)
                            .ConfigureAwait(false);
                }
            }
        }

        public static async Task RefreshChildrenCyberwareClearBindings(this IHasInternalId objParent, TreeView treCyberware, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (objParent == null || treCyberware == null)
                return;

            foreach (Cyberware objCyberware in e.OldItems)
            {
                await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware, token: token).ConfigureAwait(false);
            }
        }

        public static async Task RefreshChildrenCyberware(this IHasInternalId objParent, TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, Func<Task<int>> funcOffset, NotifyCollectionChangedEventArgs e, AsyncNotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (e == null || objParent == null || treCyberware == null)
                return;

            TreeNode nodParent = await treCyberware.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
            if (nodParent == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Cyberware objCyberware in e.NewItems)
                        {
                            await AddToTree(objCyberware, intNewIndex).ConfigureAwait(false);
                            await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treCyberware, cmsCyberware, cmsCyberwareGear, funcMakeDirty, token).ConfigureAwait(false);
                            ++intNewIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Cyberware objCyberware in e.OldItems)
                        {
                            await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware, token: token).ConfigureAwait(false);
                            await treCyberware.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objCyberware)?.Remove(), token: token).ConfigureAwait(false);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treCyberware.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (Cyberware objCyberware in e.OldItems)
                        {
                            await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware, token: token).ConfigureAwait(false);
                            await treCyberware.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objCyberware)?.Remove(), token: token).ConfigureAwait(false);
                        }
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Cyberware objCyberware in e.NewItems)
                        {
                            await AddToTree(objCyberware, intNewIndex).ConfigureAwait(false);
                            await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treCyberware, cmsCyberware, cmsCyberwareGear, funcMakeDirty, token).ConfigureAwait(false);
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
                            foreach (Cyberware objCyberware in e.OldItems)
                            {
                                nodParent.FindNodeByTag(objCyberware)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Cyberware objCyberware in e.NewItems)
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

            async ValueTask AddToTree(Cyberware objCyberware, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = await objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear, token).ConfigureAwait(false);
                if (objNode != null)
                {
                    await treCyberware.DoThreadSafeAsync(x =>
                    {
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

        public static async Task RefreshChildrenWeaponsClearBindings(this IHasInternalId objParent, TreeView treWeapons, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (objParent == null || treWeapons == null)
                return;

            foreach (Weapon objWeapon in e.OldItems)
            {
                await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token).ConfigureAwait(false);
            }
        }

        public static async Task RefreshChildrenWeapons(this IHasInternalId objParent, TreeView treWeapons, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, Func<Task<int>> funcOffset, NotifyCollectionChangedEventArgs e, AsyncNotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (e == null || objParent == null || treWeapons == null)
                return;

            TreeNode nodParent = await treWeapons.DoThreadSafeFuncAsync(x => x.FindNode(objParent.InternalId), token: token).ConfigureAwait(false);
            if (nodParent == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Weapon objWeapon in e.NewItems)
                        {
                            await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                            await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treWeapons, cmsWeapon, cmsWeaponAccessory,
                                cmsWeaponAccessoryGear, funcMakeDirty, token).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Weapon objWeapon in e.OldItems)
                        {
                            await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token).ConfigureAwait(false);
                            await treWeapons.DoThreadSafeAsync(() => nodParent.FindNode(objWeapon.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId
                            = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)
                            ?.InternalId ?? string.Empty;
                        foreach (Weapon objWeapon in e.OldItems)
                        {
                            await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token).ConfigureAwait(false);
                            await treWeapons.DoThreadSafeAsync(() => nodParent.FindNode(objWeapon.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (Weapon objWeapon in e.NewItems)
                        {
                            await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                            await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treWeapons, cmsWeapon, cmsWeaponAccessory,
                                cmsWeaponAccessoryGear, funcMakeDirty, token).ConfigureAwait(false);
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
                        foreach (Weapon objWeapon in e.OldItems)
                        {
                            await treWeapons.DoThreadSafeAsync(() => nodParent.FindNode(objWeapon.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = e.NewStartingIndex;
                        foreach (Weapon objWeapon in e.NewItems)
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

            async ValueTask AddToTree(Weapon objWeapon, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = await objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objNode != null)
                {
                    await treWeapons.DoThreadSafeAsync(x =>
                    {
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

        public static async Task RefreshWeaponAccessoriesClearBindings(this IHasInternalId objParent, TreeView treWeapons, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (objParent == null || treWeapons == null)
                return;

            foreach (WeaponAccessory objWeaponAccessory in e.OldItems)
            {
                await objWeaponAccessory.GearChildren.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                await objWeaponAccessory.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                await objWeaponAccessory.GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treWeapons, token: token), token).ConfigureAwait(false);
            }
        }

        public static async Task RefreshWeaponAccessories(this IHasInternalId objParent, TreeView treWeapons, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, Func<Task<int>> funcOffset, NotifyCollectionChangedEventArgs e, AsyncNotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (e == null || objParent == null || treWeapons == null)
                return;

            TreeNode nodParent = await treWeapons.DoThreadSafeFuncAsync(x => x.FindNode(objParent.InternalId), token: token).ConfigureAwait(false);
            if (nodParent == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (WeaponAccessory objWeaponAccessory in e.NewItems)
                        {
                            await AddToTree(objWeaponAccessory, intNewIndex).ConfigureAwait(false);

                            Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponAccessory.RefreshChildrenGearsClearBindings(treWeapons, y, innerToken);

                            Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                objWeaponAccessory.RefreshChildrenGears(
                                    treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty, token: innerToken);

                            objWeaponAccessory.GearChildren.AddTaggedBeforeClearCollectionChanged(
                                treWeapons, FuncDelegateBeforeClearToAdd);
                            objWeaponAccessory.GearChildren.AddTaggedCollectionChanged(
                                treWeapons, FuncDelegateToAdd);
                            if (funcMakeDirty != null)
                                objWeaponAccessory.GearChildren.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                            await objWeaponAccessory.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                objGear.SetupChildrenGearsCollectionChangedAsync(true, treWeapons, cmsWeaponAccessoryGear, null, funcMakeDirty, token), token).ConfigureAwait(false);
                            ++intNewIndex;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponAccessory objWeaponAccessory in e.OldItems)
                        {
                            await objWeaponAccessory.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                            await objWeaponAccessory.GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treWeapons, token: token), token).ConfigureAwait(false);
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
                        foreach (WeaponAccessory objWeaponAccessory in e.OldItems)
                        {
                            await objWeaponAccessory.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                            await objWeaponAccessory.GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treWeapons, token: token), token).ConfigureAwait(false);
                            await treWeapons.DoThreadSafeAsync(
                                () => nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove(), token: token).ConfigureAwait(false);
                        }

                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (WeaponAccessory objWeaponAccessory in e.NewItems)
                        {
                            await AddToTree(objWeaponAccessory, intNewIndex).ConfigureAwait(false);

                            Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponAccessory.RefreshChildrenGearsClearBindings(treWeapons, y, innerToken);

                            Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                objWeaponAccessory.RefreshChildrenGears(
                                    treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty, token: innerToken);

                            objWeaponAccessory.GearChildren.AddTaggedBeforeClearCollectionChanged(
                                treWeapons, FuncDelegateBeforeClearToAdd);
                            objWeaponAccessory.GearChildren.AddTaggedCollectionChanged(
                                treWeapons, FuncDelegateToAdd);
                            if (funcMakeDirty != null)
                                objWeaponAccessory.GearChildren.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                            await objWeaponAccessory.GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.SetupChildrenGearsCollectionChangedAsync(true, treWeapons, cmsWeaponAccessoryGear, null, funcMakeDirty, token), token).ConfigureAwait(false);
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
                        foreach (WeaponAccessory objWeaponAccessory in e.OldItems)
                        {
                            nodParent.FindNode(objWeaponAccessory.InternalId)?.Remove();
                        }

                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (WeaponAccessory objWeaponAccessory in e.NewItems)
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

            async ValueTask AddToTree(WeaponAccessory objWeaponAccessory, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = await objWeaponAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objNode != null)
                {
                    await treWeapons.DoThreadSafeAsync(x =>
                    {
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

        public static async Task RefreshVehicleModsClearBindings(this IHasInternalId objParent, TreeView treVehicles, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (objParent == null || treVehicles == null)
                return;

            foreach (VehicleMod objVehicleMod in e.OldItems)
            {
                await objVehicleMod.Cyberware.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objVehicleMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objVehicleMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                await objVehicleMod.Weapons.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objVehicleMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objVehicleMod.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
            }
        }

        public static async Task RefreshVehicleMods(this IHasInternalId objParent, TreeView treVehicles, ContextMenuStrip cmsVehicleMod, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, Func<Task<int>> funcOffset, NotifyCollectionChangedEventArgs e, AsyncNotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (treVehicles == null || e == null)
                return;

            TreeNode nodParent = await treVehicles.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
            if (nodParent == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (VehicleMod objVehicleMod in e.NewItems)
                        {
                            await AddToTree(objVehicleMod, intNewIndex).ConfigureAwait(false);

                            Task FuncVehicleModCyberwareBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                            Task FuncVehicleModCyberwareToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenCyberware(
                                    treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty, token: innerToken);

                            Task FuncVehicleModWeaponsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                            Task FuncVehicleModWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.GetCountAsync(innerToken), y, funcMakeDirty, token: innerToken);

                            objVehicleMod.Cyberware.AddTaggedBeforeClearCollectionChanged(treVehicles, FuncVehicleModCyberwareBeforeClearToAdd);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, FuncVehicleModCyberwareToAdd);
                            objVehicleMod.Weapons.AddTaggedBeforeClearCollectionChanged(treVehicles, FuncVehicleModWeaponsBeforeClearToAdd);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, FuncVehicleModWeaponsToAdd);
                            if (funcMakeDirty != null)
                            {
                                objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, funcMakeDirty);
                                objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, funcMakeDirty);
                            }
                            await objVehicleMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty, token), token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty, token), token).ConfigureAwait(false);
                            ++intNewIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (VehicleMod objVehicleMod in e.OldItems)
                        {
                            await objVehicleMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objVehicleMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                            await treVehicles.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objVehicleMod)?.Remove(), token: token).ConfigureAwait(false);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in e.OldItems)
                        {
                            await objVehicleMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objVehicleMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                            await treVehicles.DoThreadSafeAsync(() => nodParent.FindNodeByTag(objVehicleMod)?.Remove(), token: token).ConfigureAwait(false);
                        }
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (VehicleMod objVehicleMod in e.NewItems)
                        {
                            await AddToTree(objVehicleMod, intNewIndex).ConfigureAwait(false);

                            Task FuncVehicleModCyberwareBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                            Task FuncVehicleModCyberwareToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenCyberware(
                                    treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty, token: innerToken);

                            Task FuncVehicleModWeaponsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                            Task FuncVehicleModWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                objVehicleMod.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.GetCountAsync(innerToken), y, funcMakeDirty, token: innerToken);

                            objVehicleMod.Cyberware.AddTaggedBeforeClearCollectionChanged(treVehicles, FuncVehicleModCyberwareBeforeClearToAdd);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, FuncVehicleModCyberwareToAdd);
                            objVehicleMod.Weapons.AddTaggedBeforeClearCollectionChanged(treVehicles, FuncVehicleModWeaponsBeforeClearToAdd);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, FuncVehicleModWeaponsToAdd);
                            if (funcMakeDirty != null)
                            {
                                objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, funcMakeDirty);
                                objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, funcMakeDirty);
                            }
                            await objVehicleMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty, token), token).ConfigureAwait(false);
                            await objVehicleMod.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty, token), token).ConfigureAwait(false);
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
                            foreach (VehicleMod objVehicleMod in e.OldItems)
                            {
                                nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += await funcOffset.Invoke().ConfigureAwait(false);
                        foreach (VehicleMod objVehicleMod in e.NewItems)
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

            async ValueTask AddToTree(VehicleMod objVehicleMod, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = await objVehicleMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objNode != null)
                {
                    await treVehicles.DoThreadSafeAsync(x =>
                    {
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

        public static async Task RefreshVehicleWeaponMountsClearBindings(this IHasInternalId objParent, TreeView treVehicles, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (objParent == null || treVehicles == null)
                return;

            foreach (WeaponMount objWeaponMount in e.OldItems)
            {
                await objWeaponMount.Mods.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objWeaponMount.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objWeaponMount.Weapons.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objWeaponMount.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                await objWeaponMount.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                await objWeaponMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                {
                    await objMod.Cyberware.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                    await objMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                        .ConfigureAwait(false);
                    await objMod.Cyberware.ForEachWithSideEffectsAsync(
                        objCyberware =>
                            objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treVehicles,
                                token: token), token).ConfigureAwait(false);
                    await objMod.Weapons.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                    await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                        .ConfigureAwait(false);
                    await objMod.Weapons.ForEachWithSideEffectsAsync(
                        objWeapon =>
                            objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles,
                                token: token), token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            }
        }

        public static async Task RefreshVehicleWeaponMounts(this IHasInternalId objParent, TreeView treVehicles, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod, Func<Task<int>> funcOffset, NotifyCollectionChangedEventArgs e, AsyncNotifyCollectionChangedEventHandler funcMakeDirty, CancellationToken token = default)
        {
            if (treVehicles == null || e == null)
                return;

            TreeNode nodVehicleParent = await treVehicles.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objParent), token: token).ConfigureAwait(false);
            if (nodVehicleParent == null)
                return;
            TreeNode nodParent = await treVehicles.DoThreadSafeFuncAsync(() => nodVehicleParent.FindNode("String_WeaponMounts", false), token: token).ConfigureAwait(false);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in e.NewItems)
                        {
                            await AddToTree(objWeaponMount, intNewIndex).ConfigureAwait(false);

                            Task FuncWeaponMountVehicleModBeforeClearToAdd(object x,
                                NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                            Task FuncWeaponMountVehicleModToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshVehicleMods(treVehicles, cmsVehicleMod, cmsCyberware,
                                    cmsCyberwareGear, cmsVehicleWeapon,
                                    cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, null, y, funcMakeDirty, token: innerToken);

                            Task FuncWeaponMountWeaponBeforeClearToAdd(object x,
                                NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                            Task FuncWeaponMountWeaponToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.GetCountAsync(innerToken), y, funcMakeDirty,
                                    token: innerToken);

                            objWeaponMount.Mods.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                FuncWeaponMountVehicleModBeforeClearToAdd);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                FuncWeaponMountVehicleModToAdd);
                            objWeaponMount.Weapons.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                FuncWeaponMountWeaponBeforeClearToAdd);
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(
                                treVehicles, FuncWeaponMountWeaponToAdd);
                            if (funcMakeDirty != null)
                            {
                                objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                    funcMakeDirty);
                                objWeaponMount.Weapons.AddTaggedCollectionChanged(
                                    treVehicles, funcMakeDirty);
                            }

                            await objWeaponMount.Weapons
                                .ForEachWithSideEffectsAsync(
                                    objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treVehicles,
                                        cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                        funcMakeDirty, token), token).ConfigureAwait(false);
                            await objWeaponMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                            {
                                Task FuncWeaponMountVehicleModCyberwareBeforeClearToAdd(object x,
                                    NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                                Task FuncWeaponMountVehicleModCyberwareToAdd(
                                    object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenCyberware(
                                        treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty,
                                        token: innerToken);

                                Task FuncWeaponMountVehicleModWeaponsBeforeClearToAdd(object x,
                                    NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                Task FuncWeaponMountVehicleModWeaponsToAdd(
                                    object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                        cmsVehicleWeaponAccessory,
                                        cmsVehicleWeaponAccessoryGear,
                                        () => objMod.Cyberware.GetCountAsync(innerToken), y, funcMakeDirty, token: innerToken);

                                objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                    FuncWeaponMountVehicleModCyberwareBeforeClearToAdd);
                                objMod.Cyberware.AddTaggedCollectionChanged(
                                    treVehicles, FuncWeaponMountVehicleModCyberwareToAdd);
                                objMod.Weapons.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                    FuncWeaponMountVehicleModWeaponsBeforeClearToAdd);
                                objMod.Weapons.AddTaggedCollectionChanged(
                                    treVehicles, FuncWeaponMountVehicleModWeaponsToAdd);
                                if (funcMakeDirty != null)
                                {
                                    objMod.Cyberware.AddTaggedCollectionChanged(
                                        treVehicles, funcMakeDirty);
                                    objMod.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, funcMakeDirty);
                                }

                                await objMod.Cyberware.ForEachWithSideEffectsAsync(
                                        objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true,
                                            treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty, token), token)
                                    .ConfigureAwait(false);
                                await objMod.Weapons.ForEachWithSideEffectsAsync(
                                    objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treVehicles,
                                        cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                        funcMakeDirty, token), token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);
                            ++intNewIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponMount objWeaponMount in e.OldItems)
                        {
                            await objWeaponMount.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                            await objWeaponMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                            {
                                await objMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Cyberware.ForEachWithSideEffectsAsync(
                                    objCyberware =>
                                        objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treVehicles,
                                            token: token), token).ConfigureAwait(false);
                                await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Weapons.ForEachWithSideEffectsAsync(
                                    objWeapon =>
                                        objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles,
                                            token: token), token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);
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
                        foreach (WeaponMount objWeaponMount in e.OldItems)
                        {
                            await objWeaponMount.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token).ConfigureAwait(false);
                            await objWeaponMount.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles, token: token), token).ConfigureAwait(false);
                            await objWeaponMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                            {
                                await objMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Cyberware.ForEachWithSideEffectsAsync(
                                    objCyberware =>
                                        objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treVehicles,
                                            token: token), token).ConfigureAwait(false);
                                await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Weapons.ForEachWithSideEffectsAsync(
                                    objWeapon =>
                                        objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treVehicles,
                                            token: token), token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);

                            await treVehicles.DoThreadSafeAsync(() => nodParent?.FindNodeByTag(objWeaponMount)?.Remove(), token: token).ConfigureAwait(false);
                        }
                        int intNewIndex = e.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in e.NewItems)
                        {
                            await AddToTree(objWeaponMount, intNewIndex).ConfigureAwait(false);

                            Task FuncWeaponMountVehicleModBeforeClearToAdd(object x,
                                NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                            Task FuncWeaponMountVehicleModToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshVehicleMods(treVehicles, cmsVehicleMod, cmsCyberware,
                                    cmsCyberwareGear, cmsVehicleWeapon,
                                    cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, null, y, funcMakeDirty, token: innerToken);

                            Task FuncWeaponMountWeaponBeforeClearToAdd(object x,
                                NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                            Task FuncWeaponMountWeaponToAdd(object x, NotifyCollectionChangedEventArgs y,
                                CancellationToken innerToken = default) =>
                                objWeaponMount.RefreshChildrenWeapons(
                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.GetCountAsync(innerToken), y, funcMakeDirty,
                                    token: innerToken);

                            objWeaponMount.Mods.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                FuncWeaponMountVehicleModBeforeClearToAdd);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                FuncWeaponMountVehicleModToAdd);
                            objWeaponMount.Weapons.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                FuncWeaponMountWeaponBeforeClearToAdd);
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(
                                treVehicles, FuncWeaponMountWeaponToAdd);
                            if (funcMakeDirty != null)
                            {
                                objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                    funcMakeDirty);
                                objWeaponMount.Weapons.AddTaggedCollectionChanged(
                                    treVehicles, funcMakeDirty);
                            }
                            await objWeaponMount.Weapons.ForEachWithSideEffectsAsync(objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, funcMakeDirty, token), token).ConfigureAwait(false);
                            await objWeaponMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                            {
                                Task FuncWeaponMountVehicleModCyberwareBeforeClearToAdd(object x,
                                    NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                                Task FuncWeaponMountVehicleModCyberwareToAdd(
                                    object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenCyberware(
                                        treVehicles, cmsCyberware, cmsCyberwareGear, null, y, funcMakeDirty,
                                        token: innerToken);

                                Task FuncWeaponMountVehicleModWeaponsBeforeClearToAdd(object x,
                                    NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                Task FuncWeaponMountVehicleModWeaponsToAdd(
                                    object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                        cmsVehicleWeaponAccessory,
                                        cmsVehicleWeaponAccessoryGear,
                                        () => objMod.Cyberware.GetCountAsync(innerToken), y, funcMakeDirty, token: innerToken);

                                objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                    FuncWeaponMountVehicleModCyberwareBeforeClearToAdd);
                                objMod.Cyberware.AddTaggedCollectionChanged(
                                    treVehicles, FuncWeaponMountVehicleModCyberwareToAdd);
                                objMod.Weapons.AddTaggedBeforeClearCollectionChanged(treVehicles,
                                    FuncWeaponMountVehicleModWeaponsBeforeClearToAdd);
                                objMod.Weapons.AddTaggedCollectionChanged(
                                    treVehicles, FuncWeaponMountVehicleModWeaponsToAdd);
                                if (funcMakeDirty != null)
                                {
                                    objMod.Cyberware.AddTaggedCollectionChanged(
                                        treVehicles, funcMakeDirty);
                                    objMod.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, funcMakeDirty);
                                }

                                await objMod.Cyberware.ForEachWithSideEffectsAsync(
                                    objCyberware => objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true,
                                        treVehicles, cmsCyberware, cmsCyberwareGear, funcMakeDirty, token), token).ConfigureAwait(false);
                                await objMod.Weapons.ForEachWithSideEffectsAsync(
                                    objWeapon => objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treVehicles,
                                        cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                        funcMakeDirty, token), token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);
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
                            foreach (WeaponMount objWeaponMount in e.OldItems)
                            {
                                nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                            }
                        }, token: token).ConfigureAwait(false);
                        int intNewIndex = e.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in e.NewItems)
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
                TreeNode objNode = await objWeaponMount.CreateTreeNode(cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicleMod, token).ConfigureAwait(false);
                if (objNode == null)
                    return;

                if (nodParent == null)
                {
                    nodParent = new TreeNode
                    {
                        Tag = "String_WeaponMounts",
                        Text = await LanguageManager.GetStringAsync("String_WeaponMounts", token: token).ConfigureAwait(false)
                    };
                    int intOffset = funcOffset != null ? await funcOffset.Invoke().ConfigureAwait(false) : 0;
                    await treVehicles.DoThreadSafeAsync(() =>
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        nodVehicleParent.Nodes.Insert(intOffset, nodParent);
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
