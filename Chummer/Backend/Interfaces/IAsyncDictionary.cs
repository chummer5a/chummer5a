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
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Chummer
{
    public interface IAsyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IAsyncCollection<KeyValuePair<TKey, TValue>>
    {
        ValueTask<ICollection<TKey>> GetKeysAsync(CancellationToken token = default);

        ValueTask<ICollection<TValue>> GetValuesAsync(CancellationToken token = default);

        ValueTask<TValue> GetValueAtAsync(TKey key, CancellationToken token = default);

        ValueTask SetValueAtAsync(TKey key, TValue value, CancellationToken token = default);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey" />
        ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken token = default);

        ValueTask AddAsync(TKey key, TValue value, CancellationToken token = default);

        ValueTask<bool> RemoveAsync(TKey key, CancellationToken token = default);

        ValueTask<Tuple<bool, TValue>> TryGetValueAsync(TKey key, CancellationToken token = default);
    }
}
