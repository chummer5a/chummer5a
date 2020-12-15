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

namespace Chummer
{
    public static class ListExtensions
    {
        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, bool blnReverse = false) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            // Binary search for the place where item should be inserted
            int intIntervalStart = 0;
            int intIntervalEnd = lstCollection.Count - 1;
            int intTargetIndex = intIntervalEnd / 2;
            for (; intIntervalStart <= intIntervalEnd; intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
            {
                int intCompareResult = lstCollection[intTargetIndex].CompareTo(objNewItem);
                if (intCompareResult == 0)
                {
                    // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                    int intAdjuster = blnReverse ? -1 : 1;
                    for (int i = intTargetIndex + intAdjuster; blnReverse ? i >= 0 : i < lstCollection.Count; i += intAdjuster)
                    {
                        if (lstCollection[i].CompareTo(objNewItem) == 0)
                            intTargetIndex += intAdjuster;
                        else
                            break;
                    }
                    break;
                }
                if (intIntervalStart == intIntervalEnd)
                {
                    if ((intCompareResult > 0) != blnReverse)
                        intTargetIndex += 1;
                    break;
                }
                if ((intCompareResult > 0) != blnReverse)
                {
                    intIntervalStart = intTargetIndex + 1;
                }
                else
                    intIntervalEnd = intTargetIndex - 1;
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }

        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, IComparer<T> comparer, bool blnReverse = false)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            // Binary search for the place where item should be inserted
            int intIntervalStart = 0;
            int intIntervalEnd = lstCollection.Count - 1;
            int intTargetIndex = intIntervalEnd / 2;
            for (; intIntervalStart <= intIntervalEnd; intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
            {
                int intCompareResult = comparer.Compare(lstCollection[intTargetIndex], objNewItem);
                if (intCompareResult == 0)
                {
                    // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                    int intAdjuster = blnReverse ? -1 : 1;
                    for (int i = intTargetIndex + intAdjuster; blnReverse ? i >= 0 : i < lstCollection.Count; i += intAdjuster)
                    {
                        if (comparer.Compare(lstCollection[i], objNewItem) == 0)
                            intTargetIndex += intAdjuster;
                        else
                            break;
                    }
                    break;
                }
                if (intIntervalStart == intIntervalEnd)
                {
                    if ((intCompareResult > 0) != blnReverse)
                        intTargetIndex += 1;
                    break;
                }
                if ((intCompareResult > 0) != blnReverse)
                    intIntervalStart = intTargetIndex + 1;
                else
                    intIntervalEnd = intTargetIndex - 1;
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }

        public static void AddRange<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                lstCollection.Add(objItem);
        }

        public static void AddRangeWithSort<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd, IComparer<T> comparer, bool blnReverse = false)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            foreach (T objItem in lstToAdd)
                AddWithSort(lstCollection, objItem, comparer, blnReverse);
        }

        public static void RemoveRange<T>(this IList<T> lstCollection, int index, int count)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (lstCollection.Count == 0)
                return;
            if (index >= lstCollection.Count)
                throw new ArgumentException(nameof(index));
            if (count == 0)
                throw new ArgumentException(nameof(count));
            for (int i = Math.Min(index + count - 1, lstCollection.Count); i >= index; --i)
                lstCollection.RemoveAt(i);
        }
    }
}
