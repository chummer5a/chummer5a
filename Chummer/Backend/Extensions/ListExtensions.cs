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
            int intTargetIndex = 0;
            for (; intTargetIndex < lstCollection.Count; ++intTargetIndex)
            {
                int intCompareResult = lstCollection[intTargetIndex].CompareTo(objNewItem);
                if (intCompareResult == 0 || (intCompareResult > 0) != blnReverse)
                {
                    break;
                }
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }

        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, IComparer<T> comparer, bool blnReverse = false)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            int intTargetIndex = 0;
            for (; intTargetIndex < lstCollection.Count; ++intTargetIndex)
            {
                int intCompareResult = comparer.Compare(lstCollection[intTargetIndex], objNewItem);
                if (intCompareResult == 0 || (intCompareResult > 0) != blnReverse)
                {
                    break;
                }
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }

        public static void AddRange<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd)
        {
            foreach (T objItem in lstToAdd)
                lstCollection.Add(objItem);
        }
    }
}
