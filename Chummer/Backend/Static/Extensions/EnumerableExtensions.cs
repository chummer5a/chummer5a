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
using System.Linq;
using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Locate an object (Needle) within a list and its children (Haystack) based on GUID match.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T DeepFindById<T>(this IEnumerable<T> lstHaystack, string strGuid) where T : IHasChildren<T>, IHasInternalId
        {
            if (lstHaystack == null || string.IsNullOrWhiteSpace(strGuid) || strGuid.IsEmptyGuid())
            {
                return default;
            }

            return lstHaystack.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGuid);
        }

        /// <summary>
        /// Locate an object (Needle) within a list (Haystack) based on GUID match.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T FindById<T>(this IEnumerable<T> lstHaystack, string strGuid) where T : IHasInternalId
        {
            if (lstHaystack == null || string.IsNullOrWhiteSpace(strGuid) || strGuid.IsEmptyGuid())
            {
                return default;
            }

            return lstHaystack.FirstOrDefault(x => x.InternalId == strGuid);
        }

        /// <summary>
        /// Get a HashCode representing the contents of an enumerable (instead of just of the pointer to the location where the enumerable would start)
        /// </summary>
        /// <typeparam name="T">The type for which GetHashCode() will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static int GetEnsembleHashCode<T>(this IEnumerable<T> lstItems)
        {
            if (lstItems == null)
                return 0;
            // uint to prevent overflows
            return (int)lstItems.Aggregate<T, uint>(19, (current, objItem) => current * 31 + (uint)objItem.GetHashCode());
        }

        /// <summary>
        /// Get a HashCode representing the contents of an enumerable (instead of just of the pointer to the location where the enumerable would start) in a way where the order of the items is irrelevant
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which GetHashCode() will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static int GetOrderInvariantEnsembleHashCode<T>(this IEnumerable<T> lstItems)
        {
            if (lstItems == null)
                return 0;
            // uint to prevent overflows
            return (int)(19 + lstItems.Aggregate<T, uint>(0, (current, obj) => current + (uint)obj.GetHashCode()) * 31);
        }

        /// <summary>
        /// Syntactic sugar to wraps this object instance into an IEnumerable consisting of a single item.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="objItem">The instance that will be wrapped. </param>
        /// <returns>An IEnumerable consisting of just <paramref name="objItem"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Yield<T>(this T objItem)
        {
            return ToEnumerable(objItem); // stealth array allocation through params is still faster than yield return
        }

        /// <summary>
        /// Making use of params for syntactic sugar, wraps a list of objects into an IEnumerable consisting of them.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="lstItems">The list of objects that will be wrapped. </param>
        /// <returns>An IEnumerable consisting of <paramref name="lstItems"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerable<T>(params T[] lstItems)
        {
            return lstItems; // faster and lighter on memory than yield return
        }

        /// <summary>
        /// Syntactic sugar for LINQ Any() call that will use List::Exists() if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool Exists<T>(this IEnumerable<T> lstCollection, Predicate<T> predicate)
        {
            if (lstCollection is List<T> lstCastedCollection)
                return lstCastedCollection.Exists(predicate);
            return lstCollection.Any(x => predicate(x));
        }

        /// <summary>
        /// Syntactic sugar for LINQ FirstOrDefault call that will use List::Find() if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T Find<T>(this IEnumerable<T> lstCollection, Predicate<T> predicate)
        {
            if (lstCollection is List<T> lstCastedCollection)
                return lstCastedCollection.Find(predicate);
            return lstCollection.FirstOrDefault(x => predicate(x));
        }

        /// <summary>
        /// Syntactic sugar for LINQ LastOrDefault call that will use List::FindLast() if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T FindLast<T>(this IEnumerable<T> lstCollection, Predicate<T> predicate)
        {
            if (lstCollection is List<T> lstCastedCollection)
                return lstCastedCollection.FindLast(predicate);
            return lstCollection.LastOrDefault(x => predicate(x));
        }

        /// <summary>
        /// Syntactic sugar for LINQ Where().ToList() call that will use List::FindAll() if possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstCollection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<T> FindAll<T>(this IEnumerable<T> lstCollection, Predicate<T> predicate)
        {
            if (lstCollection is List<T> lstCastedCollection)
                return lstCastedCollection.FindAll(predicate);
            return lstCollection.Where(x => predicate(x)).ToList();
        }
    }
}
