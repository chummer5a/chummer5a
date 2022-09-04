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
    public interface IAsyncCollection<T> : ICollection<T>, IAsyncEnumerable<T>
    {
        ValueTask<int> GetCountAsync(CancellationToken token = default);
        ValueTask AddAsync(T item, CancellationToken token = default);
        ValueTask ClearAsync(CancellationToken token = default);
        ValueTask<bool> ContainsAsync(T item, CancellationToken token = default);
        ValueTask CopyToAsync(T[] array, int index, CancellationToken token = default);
        ValueTask<bool> RemoveAsync(T item, CancellationToken token = default);
    }

    public static class AsyncCollectionExtensions
    {
        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                await lstCollection.AddAsync(objItem, token);
        }

        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IAsyncEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    await lstCollection.AddAsync(objEnumerator.Current, token);
                }
            }
        }
    }
}
