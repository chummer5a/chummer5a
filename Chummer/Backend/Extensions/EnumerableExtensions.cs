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
                return default(T);
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
                return default(T);
            }

            return lstHaystack.FirstOrDefault(x => x.InternalId == strGuid);
        }
    }
}
