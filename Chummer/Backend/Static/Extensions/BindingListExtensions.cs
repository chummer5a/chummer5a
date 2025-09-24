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
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chummer
{
    public static class BindingListExtensions
    {
        /// <summary>
        /// Sorts the elements in a range of elements in a <see cref="System.ComponentModel.BindingList{T}"/> using the specified <see cref="System.Collections.Generic.IComparer{T}"> generic interface.
        /// If more than half of the list is changed, a <see cref="System.ComponentModel.ListChangedType.Reset"/> event is fired, otherwise it will be a series of <see cref="System.ComponentModel.ListChangedType.ItemChanged"/> events.
        /// </summary>
        /// <param name="lstCollection">The list to sort.</param>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="objComparer">The <see cref="System.Collections.Generic.IComparer{T}"> generic interface implementation to use when comparing elements, or null to use the <see cref="System.IComparable{T}"> generic interface implementation of each element.</param>
        public static void Sort<T>(this BindingList<T> lstCollection, int index, int length, IComparer<T> objComparer = null) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (index + length > lstCollection.Count)
                throw new InvalidOperationException(nameof(length));
            if (length == 0)
                return;
            T[] aobjSorted = new T[length];
            for (int i = 0; i < length; ++i)
                aobjSorted[i] = lstCollection[index + i];
            Array.Sort(aobjSorted, 0, length, objComparer);
            if (!lstCollection.RaiseListChangedEvents)
            {
                for (int i = 0; i < length; ++i)
                {
                    lstCollection[index + i] = aobjSorted[i];
                }

                return;
            }
            // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
            int intResetThreshold = length / 2;
            int intCountChanges = 0;
            // Not BitArray because read/write performance is much more important here than memory footprint
            bool[] ablnItemChanged = length > Utils.MaxStackLimit8BitTypes
                ? ArrayPool<bool>.Shared.Rent(length)
                : null;
            try
            {
#pragma warning disable IDE0029 // Use coalesce expression
                Span<bool> pblnItemChanged = ablnItemChanged != null
                    ? ablnItemChanged
                    : stackalloc bool[length];
#pragma warning restore IDE0029 // Use coalesce expression
                // We're going to disable events while we work with the list, then call them all at once at the end
                lstCollection.RaiseListChangedEvents = false;
                try
                {
                    for (int i = 0; i < length; ++i)
                    {
                        T objLoop = aobjSorted[i];
                        if (ReferenceEquals(objLoop, lstCollection[index + i]))
                            continue;
                        pblnItemChanged[i] = true;
                        ++intCountChanges;
                        lstCollection[index + i] = objLoop;
                    }
                }
                finally
                {
                    lstCollection.RaiseListChangedEvents = true;
                }

                if (intCountChanges >= intResetThreshold)
                {
                    lstCollection.ResetBindings();
                }
                else
                {
                    for (int i = 0; i < length; ++i)
                    {
                        if (pblnItemChanged[i])
                            lstCollection.ResetItem(index + i);
                    }
                }
            }
            finally
            {
                if (ablnItemChanged != null)
                    ArrayPool<bool>.Shared.Return(ablnItemChanged);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in a <see cref="System.ComponentModel.BindingList{T}"/> using the specified <see cref="System.Comparison{T}">.
        /// If more than half of the list is changed, a <see cref="System.ComponentModel.ListChangedType.Reset"/> event is fired, otherwise it will be a series of <see cref="System.ComponentModel.ListChangedType.ItemChanged"/> events.
        /// </summary>
        /// <param name="lstCollection">The list to sort.</param>
        /// <param name="funcComparison">The <see cref="System.Comparison{T}"> to use when comparing elements.</param>
        public static void Sort<T>(this BindingList<T> lstCollection, Comparison<T> funcComparison)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            int intCollectionSize = lstCollection.Count;
            T[] aobjSorted = new T[intCollectionSize];
            for (int i = 0; i < intCollectionSize; ++i)
                aobjSorted[i] = lstCollection[i];
            Array.Sort(aobjSorted,funcComparison);
            if (!lstCollection.RaiseListChangedEvents)
            {
                for (int i = 0; i < intCollectionSize; ++i)
                {
                    lstCollection[i] = aobjSorted[i];
                }

                return;
            }
            // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
            int intResetThreshold = intCollectionSize / 2;
            int intCountChanges = 0;
            // Not BitArray because read/write performance is much more important here than memory footprint
            bool[] ablnItemChanged = intCollectionSize > Utils.MaxStackLimit8BitTypes
                ? ArrayPool<bool>.Shared.Rent(intCollectionSize)
                : null;
            try
            {
#pragma warning disable IDE0029 // Use coalesce expression
                Span<bool> pblnItemChanged = ablnItemChanged != null
                    ? ablnItemChanged
                    : stackalloc bool[intCollectionSize];
#pragma warning restore IDE0029 // Use coalesce expression
                // We're going to disable events while we work with the list, then call them all at once at the end
                lstCollection.RaiseListChangedEvents = false;
                try
                {
                    for (int i = 0; i < intCollectionSize; ++i)
                    {
                        T objLoop = aobjSorted[i];
                        if (ReferenceEquals(objLoop, lstCollection[i]))
                            continue;
                        pblnItemChanged[i] = true;
                        ++intCountChanges;
                        lstCollection[i] = objLoop;
                    }
                }
                finally
                {
                    lstCollection.RaiseListChangedEvents = true;
                }

                if (intCountChanges >= intResetThreshold)
                {
                    lstCollection.ResetBindings();
                }
                else
                {
                    for (int i = 0; i < intCollectionSize; ++i)
                    {
                        if (pblnItemChanged[i])
                            lstCollection.ResetItem(i);
                    }
                }
            }
            finally
            {
                if (ablnItemChanged != null)
                    ArrayPool<bool>.Shared.Return(ablnItemChanged);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in a <see cref="System.ComponentModel.BindingList{T}"/> using the specified <see cref="System.Collections.Generic.IComparer{T}"> generic interface.
        /// If more than half of the list is changed, a <see cref="System.ComponentModel.ListChangedType.Reset"/> event is fired, otherwise it will be a series of <see cref="System.ComponentModel.ListChangedType.ItemChanged"/> events.
        /// </summary>
        /// <param name="lstCollection">The list to sort.</param>
        /// <param name="objComparer">The <see cref="System.Collections.Generic.IComparer{T}"> generic interface implementation to use when comparing elements, or null to use the <see cref="System.IComparable{T}"> generic interface implementation of each element.</param>
        public static void Sort<T>(this BindingList<T> lstCollection, IComparer<T> objComparer = null) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            int intCollectionSize = lstCollection.Count;
            T[] aobjSorted = new T[intCollectionSize];
            for (int i = 0; i < intCollectionSize; ++i)
                aobjSorted[i] = lstCollection[i];
            Array.Sort(aobjSorted, 0, intCollectionSize, objComparer);
            if (!lstCollection.RaiseListChangedEvents)
            {
                for (int i = 0; i < intCollectionSize; ++i)
                {
                    lstCollection[i] = aobjSorted[i];
                }

                return;
            }
            // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
            int intResetThreshold = intCollectionSize / 2;
            int intCountChanges = 0;
            // Not BitArray because read/write performance is much more important here than memory footprint
            bool[] ablnItemChanged = intCollectionSize > Utils.MaxStackLimit8BitTypes
                ? ArrayPool<bool>.Shared.Rent(intCollectionSize)
                : null;
            try
            {
#pragma warning disable IDE0029 // Use coalesce expression
                Span<bool> pblnItemChanged = ablnItemChanged != null
                    ? ablnItemChanged
                    : stackalloc bool[intCollectionSize];
#pragma warning restore IDE0029 // Use coalesce expression
                // We're going to disable events while we work with the list, then call them all at once at the end
                lstCollection.RaiseListChangedEvents = false;
                try
                {
                    for (int i = 0; i < intCollectionSize; ++i)
                    {
                        T objLoop = aobjSorted[i];
                        if (ReferenceEquals(objLoop, lstCollection[i]))
                            continue;
                        pblnItemChanged[i] = true;
                        ++intCountChanges;
                        lstCollection[i] = objLoop;
                    }
                }
                finally
                {
                    lstCollection.RaiseListChangedEvents = true;
                }

                if (intCountChanges >= intResetThreshold)
                {
                    lstCollection.ResetBindings();
                }
                else
                {
                    for (int i = 0; i < intCollectionSize; ++i)
                    {
                        if (pblnItemChanged[i])
                            lstCollection.ResetItem(i);
                    }
                }
            }
            finally
            {
                if (ablnItemChanged != null)
                    ArrayPool<bool>.Shared.Return(ablnItemChanged);
            }
        }

        /// <summary>
        /// Move a single item in a <see cref="System.ComponentModel.BindingList{T}"/> from one index to another.
        /// If more than half of the list is changed, a <see cref="System.ComponentModel.ListChangedType.Reset"/> event is fired, otherwise it will be a series of <see cref="System.ComponentModel.ListChangedType.ItemChanged"/> events.
        /// We cannot fire a <see cref="System.ComponentModel.ListChangedType.ItemMoved"/> because we do not have access to the internal methods required for us to be able to fire it instead of the others.
        /// </summary>
        /// <param name="lstCollection">The list in which the item is to be moved.</param>
        /// <param name="intOldIndex">The current index where the item to be moved is located.</param>
        /// <param name="intNewIndex">The new index to which the selected item should be moved.</param>
        public static void Move<T>(this BindingList<T> lstCollection, int intOldIndex, int intNewIndex)
        {
            int intParity = intOldIndex < intNewIndex ? 1 : -1;
            bool blnOldRaiseListChangedEvents = lstCollection.RaiseListChangedEvents;
            try
            {
                lstCollection.RaiseListChangedEvents = false;
                for (int i = intOldIndex; i != intNewIndex; i += intParity)
                {
                    (lstCollection[intOldIndex + intParity], lstCollection[intOldIndex]) = (lstCollection[intOldIndex], lstCollection[intOldIndex + intParity]);
                }
            }
            finally
            {
                lstCollection.RaiseListChangedEvents = blnOldRaiseListChangedEvents;
            }

            if (Math.Abs(intOldIndex - intNewIndex) * 2 >= lstCollection.Count)
            {
                lstCollection.ResetBindings();
            }
            else
            {
                for (int i = intOldIndex; i != intNewIndex; i += intParity)
                {
                    lstCollection.ResetItem(i);
                }
            }
        }
    }
}
