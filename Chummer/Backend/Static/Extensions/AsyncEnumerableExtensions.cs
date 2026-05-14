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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    internal static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Syntactic sugar for collecting the elements of a <see cref="IAsyncEnumerable{T}"/> into a list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                lstReturn.Add(objItem);
            }
            return lstReturn;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objItem))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (await funcPredicate(objItem).ConfigureAwait(false))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objItem, token))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (await funcPredicate(objItem, token).ConfigureAwait(false))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!funcPredicate(objItem))
                    return false;
            }
            return true;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await funcPredicate(objItem).ConfigureAwait(false))
                    return false;
            }
            return true;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!funcPredicate(objItem, token))
                    return false;
            }
            return true;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await funcPredicate(objItem, token).ConfigureAwait(false))
                    return false;
            }
            return true;
        }
    }
}
