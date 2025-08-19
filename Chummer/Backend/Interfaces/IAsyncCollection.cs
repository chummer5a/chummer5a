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
    public interface IAsyncCollection<T> : ICollection<T>, IAsyncEnumerable<T>
    {
        Task<int> GetCountAsync(CancellationToken token = default);

        Task AddAsync(T item, CancellationToken token = default);

        Task ClearAsync(CancellationToken token = default);

        Task<bool> ContainsAsync(T item, CancellationToken token = default);

        Task CopyToAsync(T[] array, int index, CancellationToken token = default);

        Task<bool> RemoveAsync(T item, CancellationToken token = default);
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
                await lstCollection.AddAsync(objItem, token).ConfigureAwait(false);
        }

        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IAsyncEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    await lstCollection.AddAsync(objEnumerator.Current, token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }
    }
}
