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
using System.Threading.Tasks;

namespace Chummer
{
    internal static class ReadOnlyDictionaryExtensions
    {
        public static bool EqualsByValue(this IReadOnlyDictionary<object, IComparable> dicLeft, IDictionary<object, IComparable> dicRight)
        {
            if (dicLeft.Count != dicRight.Count)
                return false;
            return dicLeft.Keys.All(dicRight.ContainsKey) &&
                   dicRight.Keys.All(x => dicLeft.ContainsKey(x) && dicRight[x].Equals(dicLeft[x]));
        }

        public static bool EqualsByValue(this IReadOnlyDictionary<object, IComparable> dicLeft, IReadOnlyDictionary<object, IComparable> dicRight)
        {
            if (dicLeft.Count != dicRight.Count)
                return false;
            return dicLeft.Keys.All(dicRight.ContainsKey) &&
                   dicRight.Keys.All(x => dicLeft.ContainsKey(x) && dicRight[x].Equals(dicLeft[x]));
        }

        public static async ValueTask<bool> EqualsByValueAsync(this IAsyncReadOnlyDictionary<object, IComparable> dicLeft, IAsyncReadOnlyDictionary<object, IComparable> dicRight)
        {
            if (dicLeft.Count != dicRight.Count)
                return false;
            IEnumerator<KeyValuePair<object, IComparable>> objLeftEnumerator = await dicLeft.GetEnumeratorAsync();
            while (objLeftEnumerator.MoveNext())
            {
                object objKey = objLeftEnumerator.Current.Key;
                if (!await dicRight.ContainsKeyAsync(objKey))
                    return false;
            }
            IEnumerator<KeyValuePair<object, IComparable>> objRightEnumerator = await dicRight.GetEnumeratorAsync();
            while (objRightEnumerator.MoveNext())
            {
                object objKey = objRightEnumerator.Current.Key;
                (bool blnContains, IComparable objValue) = await dicLeft.TryGetValueAsync(objKey);
                if (!blnContains)
                    return false;
                if (!objValue.Equals(objRightEnumerator.Current.Value))
                    return false;
            }
            return true;
        }
    }
}
