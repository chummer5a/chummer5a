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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public interface IAsyncReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IAsyncReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken token = default);

        ValueTask<Tuple<bool, TValue>> TryGetValueAsync(TKey key, CancellationToken token = default);

        ValueTask<IReadOnlyCollection<TKey>> GetReadOnlyKeysAsync(CancellationToken token = default);

        ValueTask<IReadOnlyCollection<TValue>> GetReadOnlyValuesAsync(CancellationToken token = default);
    }

    public static class AsyncReadOnlyDictionaryExtensions
    {
        public static async ValueTask<bool> EqualsByValueAsync(
            this IAsyncReadOnlyDictionary<object, IComparable> dicLeft,
            IAsyncDictionary<object, IComparable> dicRight, CancellationToken token = default)
        {
            if (await dicLeft.GetCountAsync(token).ConfigureAwait(false)
                != await dicRight.GetCountAsync(token).ConfigureAwait(false))
                return false;
            IEnumerator<KeyValuePair<object, IComparable>> objLeftEnumerator
                = await dicLeft.GetEnumeratorAsync(token).ConfigureAwait(false);
            while (objLeftEnumerator.MoveNext())
            {
                object objKey = objLeftEnumerator.Current.Key;
                if (!await dicRight.ContainsKeyAsync(objKey, token).ConfigureAwait(false))
                    return false;
            }

            IEnumerator<KeyValuePair<object, IComparable>> objRightEnumerator
                = await dicRight.GetEnumeratorAsync(token).ConfigureAwait(false);
            while (objRightEnumerator.MoveNext())
            {
                object objKey = objRightEnumerator.Current.Key;
                (bool blnContains, IComparable objValue)
                    = await dicLeft.TryGetValueAsync(objKey, token).ConfigureAwait(false);
                if (!blnContains)
                    return false;
                if (!objValue.Equals(objRightEnumerator.Current.Value))
                    return false;
            }

            return true;
        }

        public static async ValueTask<bool> EqualsByValueAsync(
            this IAsyncReadOnlyDictionary<object, IComparable> dicLeft,
            IAsyncReadOnlyDictionary<object, IComparable> dicRight, CancellationToken token = default)
        {
            if (await dicLeft.GetCountAsync(token).ConfigureAwait(false)
                != await dicRight.GetCountAsync(token).ConfigureAwait(false))
                return false;
            IEnumerator<KeyValuePair<object, IComparable>> objLeftEnumerator
                = await dicLeft.GetEnumeratorAsync(token).ConfigureAwait(false);
            while (objLeftEnumerator.MoveNext())
            {
                object objKey = objLeftEnumerator.Current.Key;
                if (!await dicRight.ContainsKeyAsync(objKey, token).ConfigureAwait(false))
                    return false;
            }

            IEnumerator<KeyValuePair<object, IComparable>> objRightEnumerator
                = await dicRight.GetEnumeratorAsync(token).ConfigureAwait(false);
            while (objRightEnumerator.MoveNext())
            {
                object objKey = objRightEnumerator.Current.Key;
                (bool blnContains, IComparable objValue)
                    = await dicLeft.TryGetValueAsync(objKey, token).ConfigureAwait(false);
                if (!blnContains)
                    return false;
                if (!objValue.Equals(objRightEnumerator.Current.Value))
                    return false;
            }

            return true;
        }

        public static async ValueTask<bool> EqualsByValueAsync(
            this IAsyncReadOnlyDictionary<object, IComparable> dicLeft,
            IDictionary<object, IComparable> dicRight, CancellationToken token = default)
        {
            if (await dicLeft.GetCountAsync(token).ConfigureAwait(false) != dicRight.Count)
                return false;
            IEnumerator<KeyValuePair<object, IComparable>> objLeftEnumerator
                = await dicLeft.GetEnumeratorAsync(token).ConfigureAwait(false);
            while (objLeftEnumerator.MoveNext())
            {
                object objKey = objLeftEnumerator.Current.Key;
                if (!dicRight.ContainsKey(objKey))
                    return false;
            }

            foreach (KeyValuePair<object, IComparable> kvpRight in dicRight)
            {
                object objKey = kvpRight.Key;
                (bool blnContains, IComparable objValue)
                    = await dicLeft.TryGetValueAsync(objKey, token).ConfigureAwait(false);
                if (!blnContains)
                    return false;
                if (!objValue.Equals(kvpRight.Value))
                    return false;
            }

            return true;
        }

        public static async ValueTask<bool> EqualsByValueAsync(
            this IAsyncReadOnlyDictionary<object, IComparable> dicLeft,
            IReadOnlyDictionary<object, IComparable> dicRight, CancellationToken token = default)
        {
            if (await dicLeft.GetCountAsync(token).ConfigureAwait(false) != dicRight.Count)
                return false;
            IEnumerator<KeyValuePair<object, IComparable>> objLeftEnumerator
                = await dicLeft.GetEnumeratorAsync(token).ConfigureAwait(false);
            while (objLeftEnumerator.MoveNext())
            {
                object objKey = objLeftEnumerator.Current.Key;
                if (!dicRight.ContainsKey(objKey))
                    return false;
            }

            foreach (KeyValuePair<object, IComparable> kvpRight in dicRight)
            {
                object objKey = kvpRight.Key;
                (bool blnContains, IComparable objValue)
                    = await dicLeft.TryGetValueAsync(objKey, token).ConfigureAwait(false);
                if (!blnContains)
                    return false;
                if (!objValue.Equals(kvpRight.Value))
                    return false;
            }

            return true;
        }
    }
}
