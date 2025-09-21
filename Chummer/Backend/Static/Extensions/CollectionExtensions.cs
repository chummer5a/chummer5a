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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> lstCollection, IEnumerable<T> lstToAdd)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                lstCollection.Add(objItem);
        }

        /// <summary>
        /// Get a HashCode representing the contents of a collection in a way where the order of the items is irrelevant.
        /// Uses the parallel option for large enough collections where it could potentially be faster
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which <see cref="object.GetHashCode"/> will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOrderInvariantEnsembleHashCodeSmart<T>(this IReadOnlyCollection<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return lstItems.Count > ushort.MaxValue
                ? lstItems.GetOrderInvariantEnsembleHashCodeParallel(token)
                : lstItems.GetOrderInvariantEnsembleHashCode(token);
        }

        /// <summary>
        /// Get a HashCode representing the contents of a collection in a way where the order of the items is irrelevant
        /// This is a parallelized version of GetOrderInvariantEnsembleHashCode meant to be used for large collections
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which <see cref="object.GetHashCode"/> will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static int GetOrderInvariantEnsembleHashCodeParallel<T>(this IReadOnlyCollection<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstItems == null)
                return 0;
            // uint to prevent overflows
            unchecked
            {
                uint result = 0;
                Parallel.For(0, lstItems.Count, () => 0, (i, state, local) =>
                {
                    if (token.IsCancellationRequested)
                        state.Stop();
                    return state.IsStopped ? 0 : lstItems.ElementAtBetter(i).GetHashCode();
                }, localResult => result += (uint)localResult);
                token.ThrowIfCancellationRequested();
                return (int)(19 + result * 31);
            }
        }
    }
}
