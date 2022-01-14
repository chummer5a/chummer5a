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
    internal static class ReadOnlyListExtensions
    {
        public static int BinarySearch<T>(this IReadOnlyList<T> lstCollection, T objItem) where T : IComparable
        {
            int intLastIntervalBounds = lstCollection.Count - 1;
            int intBase = 0;
            for (int i = intLastIntervalBounds / 2; i > 0; i = intLastIntervalBounds / 2)
            {
                int intLoopIndex = intBase + i;
                int intCompareResult = objItem.CompareTo(lstCollection[intLoopIndex]);
                if (intCompareResult == 0)
                    return intLoopIndex;
                if (intCompareResult > 0)
                {
                    intBase += intLastIntervalBounds - i;
                    intLastIntervalBounds -= i; // Makes sure that for odd sizes, we end up spanning every item
                }
                else
                {
                    intLastIntervalBounds = i;
                }
            }

            return ~(intBase + 1); // Bitwise complement of next item larger than this one, just like List.BinarySearch
        }

        public static int BinarySearch<T>(this IReadOnlyList<T> lstCollection, T objItem, IComparer<T> comparer)
        {
            int intLastIntervalBounds = lstCollection.Count - 1;
            int intBase = 0;
            for (int i = intLastIntervalBounds / 2; i > 0; i = intLastIntervalBounds / 2)
            {
                int intLoopIndex = intBase + i;
                int intCompareResult = comparer.Compare(objItem, lstCollection[intLoopIndex]);
                if (intCompareResult == 0)
                    return intLoopIndex;
                if (intCompareResult > 0)
                {
                    intBase += intLastIntervalBounds - i;
                    intLastIntervalBounds -= i; // Makes sure that for odd sizes, we end up spanning every item
                }
                else
                {
                    intLastIntervalBounds = i;
                }
            }

            return ~(intBase + 1); // Bitwise complement of next item larger than this one, just like List.BinarySearch
        }

        public static int BinarySearch<T>(this IReadOnlyList<T> lstCollection, int index, int count, T objItem, IComparer<T> comparer)
        {
            int intLastIntervalBounds = count - 1;
            int intBase = index;
            for (int i = intLastIntervalBounds / 2; i > 0; i = intLastIntervalBounds / 2)
            {
                int intLoopIndex = intBase + i;
                int intCompareResult = comparer.Compare(objItem, lstCollection[intLoopIndex]);
                if (intCompareResult == 0)
                    return intLoopIndex;
                if (intCompareResult > 0)
                {
                    intBase += intLastIntervalBounds - i;
                    intLastIntervalBounds -= i; // Makes sure that for odd sizes, we end up spanning every item
                }
                else
                {
                    intLastIntervalBounds = i;
                }
            }

            return ~(intBase + 1); // Bitwise complement of next item larger than this one, just like List.BinarySearch
        }

        public static int FindIndex<T>(this IReadOnlyList<T> lstCollection, Predicate<T> predicate)
        {
            for (int i = 0; i < lstCollection.Count; ++i)
            {
                if (predicate(lstCollection[i]))
                    return i;
            }
            return -1;
        }

        public static int FindIndex<T>(this IReadOnlyList<T> lstCollection, int startIndex, Predicate<T> predicate)
        {
            for (int i = startIndex; i < lstCollection.Count; ++i)
            {
                if (predicate(lstCollection[i]))
                    return i;
            }
            return -1;
        }

        public static int FindIndex<T>(this IReadOnlyList<T> lstCollection, int startIndex, int count, Predicate<T> predicate)
        {
            int intUpperBounds = count - startIndex;
            for (int i = startIndex; i < Math.Min(lstCollection.Count, intUpperBounds); ++i)
            {
                if (predicate(lstCollection[i]))
                    return i;
            }
            return -1;
        }

        public static int FindLastIndex<T>(this IReadOnlyList<T> lstCollection, Predicate<T> predicate)
        {
            for (int i = lstCollection.Count - 1; i >= 0; --i)
            {
                if (predicate(lstCollection[i]))
                    return i;
            }
            return -1;
        }

        public static int FindLastIndex<T>(this IReadOnlyList<T> lstCollection, int startIndex, Predicate<T> predicate)
        {
            for (int i = startIndex; i >= 0; --i)
            {
                if (predicate(lstCollection[i]))
                    return i;
            }
            return -1;
        }

        public static int FindLastIndex<T>(this IReadOnlyList<T> lstCollection, int startIndex, int count, Predicate<T> predicate)
        {
            int intLowerBounds = startIndex - count;
            for (int i = startIndex; i >= Math.Max(0, intLowerBounds); --i)
            {
                if (predicate(lstCollection[i]))
                    return i;
            }
            return -1;
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> lstCollection, T objItem)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (objItem == null)
                throw new ArgumentNullException(nameof(objItem));
            for (int i = lstCollection.Count - 1; i >= 0; --i)
            {
                if (lstCollection[i].Equals(objItem))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> lstCollection, T objItem)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (objItem == null)
                throw new ArgumentNullException(nameof(objItem));
            for (int i = 0; i < lstCollection.Count; ++i)
            {
                if (lstCollection[i].Equals(objItem))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
