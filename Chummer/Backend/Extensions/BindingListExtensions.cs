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
        internal static void MergeInto<T>(this BindingList<T> list, IEnumerable<T> items, Comparison<T> comparison, Action<T, T> funcMergeIfEquals = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));
            foreach (T item in items)
            {
                list.MergeInto(item, comparison, funcMergeIfEquals);
            }
        }

        internal static void MergeInto<T>(this BindingList<T> list, T objNewItem, Comparison<T> comparison, Action<T,T> funcMergeIfEquals = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (objNewItem == null)
                throw new ArgumentNullException(nameof(objNewItem));
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));
            //if (list.Count == 0)
            //{
            //    list.Add(item);
            //    return;
            //}
            // Binary search for the place where item should be merged in
            int intIntervalStart = 0;
            int intIntervalEnd = list.Count - 1;
            int intMergeIndex = intIntervalEnd / 2;
            for (; intIntervalStart <= intIntervalEnd; intMergeIndex = (intIntervalStart + intIntervalEnd) / 2)
            {
                T objLoopExistingItem = list[intMergeIndex];
                int intCompareResult = comparison(objLoopExistingItem, objNewItem);
                if (intCompareResult == 0)
                {
                    // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                    for (int i = intMergeIndex + 1; i < list.Count; ++i)
                    {
                        if (comparison(list[i], objNewItem) == 0)
                            intMergeIndex += 1;
                        else
                            break;
                    }
                    funcMergeIfEquals?.Invoke(objLoopExistingItem, objNewItem);
                    return;
                }
                if (intIntervalStart == intIntervalEnd)
                {
                    if (intCompareResult > 0)
                        intMergeIndex += 1;
                    break;
                }
                if (intCompareResult > 0)
                    intIntervalStart = intMergeIndex + 1;
                else
                    intIntervalEnd = intMergeIndex - 1;
            }

            list.Insert(intMergeIndex, objNewItem);
        }

        internal static void RemoveAll<T>(this BindingList<T> list, Predicate<T> predicate)
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        internal static void AddRange<T>(this BindingList<T> list, IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                list.Add(item);
            }
        }
    }
}
