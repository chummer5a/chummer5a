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

using System.Collections.Generic;
using System.Linq;

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

            int intHash = 19;
            foreach (T objItem in lstItems)
            {
                intHash = intHash * 31 + objItem.GetHashCode();
            }
            return intHash;
        }
    }
}
