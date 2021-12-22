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
using System.ComponentModel;

namespace Chummer
{
    public static class BindingListExtensions
    {
        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified
        /// System.Collections.Generic.IComparer`1 generic interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection">The ObservableCollection to sort.</param>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="objComparer">The System.Collections.Generic.IComparer`1 generic interface
        /// implementation to use when comparing elements, or null to use the System.IComparable`1 generic
        /// interface implementation of each element.</param>
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
            Array.Sort(aobjSorted, objComparer);
            bool blnOldRaiseListChangedEvents = lstCollection.RaiseListChangedEvents;
            // Not BitArray because read/write performance is much more important here than memory footprint
            bool[] ablnItemChanged = blnOldRaiseListChangedEvents ? new bool[aobjSorted.Length] : null;
            // We're going to disable events while we work with the list, then call them all at once at the end
            lstCollection.RaiseListChangedEvents = false;
            for (int i = 0; i < aobjSorted.Length; ++i)
            {
                T objLoop = aobjSorted[i];
                int intOldIndex = lstCollection.IndexOf(objLoop);
                int intNewIndex = index + i;
                if (intOldIndex == intNewIndex)
                    continue;
                if (intOldIndex > intNewIndex)
                {
                    // Account for removal happening before removal
                    --intOldIndex;
                    if (blnOldRaiseListChangedEvents)
                    {
                        for (int j = intNewIndex; j <= intOldIndex; ++j)
                            ablnItemChanged[j] = true;
                    }
                }
                else
                {
                    // Account for removal happening before removal
                    --intNewIndex;
                    if (blnOldRaiseListChangedEvents)
                    {
                        for (int j = intOldIndex; j <= intNewIndex; ++j)
                            ablnItemChanged[j] = true;
                    }
                }
                lstCollection.RemoveAt(intOldIndex);
                lstCollection.Insert(intNewIndex, objLoop);
            }
            lstCollection.RaiseListChangedEvents = blnOldRaiseListChangedEvents;
            if (!blnOldRaiseListChangedEvents)
                return;
            // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
            int intResetThreshold = ablnItemChanged.Length / 2;
            int intCountTrues = 0;
            for (int i = 0; i < ablnItemChanged.Length; ++i)
            {
                if (ablnItemChanged[i])
                {
                    ++intCountTrues;
                    if (intCountTrues >= intResetThreshold)
                    {
                        lstCollection.ResetBindings();
                        return;
                    }
                }
            }
            for (int i = 0; i < ablnItemChanged.Length; ++i)
            {
                if (ablnItemChanged[i])
                    lstCollection.ResetItem(i);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified System.Comparison`1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection">The ObservableCollection to sort.</param>
        /// <param name="funcComparison">The System.Comparison`1 to use when comparing elements.</param>
        public static void Sort<T>(this BindingList<T> lstCollection, Comparison<T> funcComparison)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            T[] aobjSorted = new T[lstCollection.Count];
            for (int i = 0; i < lstCollection.Count; ++i)
                aobjSorted[i] = lstCollection[i];
            Array.Sort(aobjSorted, funcComparison);
            bool blnOldRaiseListChangedEvents = lstCollection.RaiseListChangedEvents;
            // Not BitArray because read/write performance is much more important here than memory footprint
            bool[] ablnItemChanged = blnOldRaiseListChangedEvents ? new bool[aobjSorted.Length] : null;
            // We're going to disable events while we work with the list, then call them all at once at the end
            lstCollection.RaiseListChangedEvents = false;
            for (int i = 0; i < aobjSorted.Length; ++i)
            {
                T objLoop = aobjSorted[i];
                int intOldIndex = lstCollection.IndexOf(objLoop);
                int intNewIndex = i;
                if (intOldIndex == intNewIndex)
                    continue;
                if (intOldIndex > intNewIndex)
                {
                    // Account for removal happening before removal
                    --intOldIndex;
                    if (blnOldRaiseListChangedEvents)
                    {
                        for (int j = intNewIndex; j <= intOldIndex; ++j)
                            ablnItemChanged[j] = true;
                    }
                }
                else
                {
                    // Account for removal happening before removal
                    --intNewIndex;
                    if (blnOldRaiseListChangedEvents)
                    {
                        for (int j = intOldIndex; j <= intNewIndex; ++j)
                            ablnItemChanged[j] = true;
                    }
                }
                lstCollection.RemoveAt(intOldIndex);
                lstCollection.Insert(intNewIndex, objLoop);
            }
            lstCollection.RaiseListChangedEvents = blnOldRaiseListChangedEvents;
            if (!blnOldRaiseListChangedEvents)
                return;
            // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
            int intResetThreshold = ablnItemChanged.Length / 2;
            int intCountTrues = 0;
            for (int i = 0; i < ablnItemChanged.Length; ++i)
            {
                if (ablnItemChanged[i])
                {
                    ++intCountTrues;
                    if (intCountTrues >= intResetThreshold)
                    {
                        lstCollection.ResetBindings();
                        return;
                    }
                }
            }
            for (int i = 0; i < ablnItemChanged.Length; ++i)
            {
                if (ablnItemChanged[i])
                    lstCollection.ResetItem(i);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified
        /// System.Collections.Generic.IComparer`1 generic interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection">The ObservableCollection to sort.</param>
        /// <param name="objComparer">The System.Collections.Generic.IComparer`1 generic interface
        /// implementation to use when comparing elements, or null to use the System.IComparable`1 generic
        /// interface implementation of each element.</param>
        public static void Sort<T>(this BindingList<T> lstCollection, IComparer<T> objComparer = null) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            T[] aobjSorted = new T[lstCollection.Count];
            for (int i = 0; i < lstCollection.Count; ++i)
                aobjSorted[i] = lstCollection[i];
            Array.Sort(aobjSorted, objComparer);
            bool blnOldRaiseListChangedEvents = lstCollection.RaiseListChangedEvents;
            // Not BitArray because read/write performance is much more important here than memory footprint
            bool[] ablnItemChanged = blnOldRaiseListChangedEvents ? new bool[aobjSorted.Length] : null;
            // We're going to disable events while we work with the list, then call them all at once at the end
            lstCollection.RaiseListChangedEvents = false;
            for (int i = 0; i < aobjSorted.Length; ++i)
            {
                T objLoop = aobjSorted[i];
                int intOldIndex = lstCollection.IndexOf(objLoop);
                int intNewIndex = i;
                if (intOldIndex == intNewIndex)
                    continue;
                if (intOldIndex > intNewIndex)
                {
                    // Account for removal happening before removal
                    --intOldIndex;
                    if (blnOldRaiseListChangedEvents)
                    {
                        for (int j = intNewIndex; j <= intOldIndex; ++j)
                            ablnItemChanged[j] = true;
                    }
                }
                else
                {
                    // Account for removal happening before removal
                    --intNewIndex;
                    if (blnOldRaiseListChangedEvents)
                    {
                        for (int j = intOldIndex; j <= intNewIndex; ++j)
                            ablnItemChanged[j] = true;
                    }
                }
                lstCollection.RemoveAt(intOldIndex);
                lstCollection.Insert(intNewIndex, objLoop);
            }
            lstCollection.RaiseListChangedEvents = blnOldRaiseListChangedEvents;
            if (!blnOldRaiseListChangedEvents)
                return;
            // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
            int intResetThreshold = ablnItemChanged.Length / 2;
            int intCountTrues = 0;
            for (int i = 0; i < ablnItemChanged.Length; ++i)
            {
                if (ablnItemChanged[i])
                {
                    ++intCountTrues;
                    if (intCountTrues >= intResetThreshold)
                    {
                        lstCollection.ResetBindings();
                        return;
                    }
                }
            }
            for (int i = 0; i < ablnItemChanged.Length; ++i)
            {
                if (ablnItemChanged[i])
                    lstCollection.ResetItem(i);
            }
        }
    }
}
