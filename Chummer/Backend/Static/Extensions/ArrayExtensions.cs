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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class ArrayExtensions
    {
        public static Task SortAsync<T>(T[] keys, int index, int length, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            return length >= 2
                ? IntroSortAsync(keys, index, length + index - 1, 2 * keys.Length.FloorLog2(), comparer, token)
                : Task.CompletedTask;
        }

        private static async Task IntroSortAsync<T>(T[] keys, int lo, int hi, int depthLimit, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int num1;
            for (; hi > lo; hi = num1 - 1)
            {
                token.ThrowIfCancellationRequested();
                int num2 = hi - lo + 1;
                if (num2 <= 16)
                {
                    if (num2 == 1)
                        break;
                    if (num2 == 2)
                    {
                        await SwapIfGreaterAsync(keys, comparer, lo, hi, token).ConfigureAwait(false);
                        break;
                    }
                    if (num2 == 3)
                    {
                        await SwapIfGreaterAsync(keys, comparer, lo, hi - 1, token).ConfigureAwait(false);
                        await SwapIfGreaterAsync(keys, comparer, lo, hi, token).ConfigureAwait(false);
                        await SwapIfGreaterAsync(keys, comparer, hi - 1, hi, token).ConfigureAwait(false);
                        break;
                    }
                    await InsertionSortAsync(keys, lo, hi, comparer, token).ConfigureAwait(false);
                    break;
                }
                if (depthLimit == 0)
                {
                    await HeapsortAsync(keys, lo, hi, comparer, token).ConfigureAwait(false);
                    break;
                }
                --depthLimit;
                num1 = await PickPivotAndPartitionAsync(keys, lo, hi, comparer, token).ConfigureAwait(false);
                await IntroSortAsync(keys, num1 + 1, hi, depthLimit, comparer, token).ConfigureAwait(false);
            }
        }

        private static async Task SwapIfGreaterAsync<T>(T[] keys, Func<T, T, Task<int>> comparer, int a, int b, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (a != b && await comparer(keys[a], keys[b]).ConfigureAwait(false) > 0)
                (keys[a], keys[b]) = (keys[b], keys[a]);
        }

        private static Task SwapAsync<T>(T[] a, int i, int j, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (i != j)
                (a[i], a[j]) = (a[j], a[i]);
            return Task.CompletedTask;
        }

        private static async Task<int> PickPivotAndPartitionAsync<T>(T[] keys, int lo, int hi, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int index = lo + (hi - lo) / 2;
            await SwapIfGreaterAsync(keys, comparer, lo, index, token).ConfigureAwait(false);
            await SwapIfGreaterAsync(keys, comparer, lo, hi, token).ConfigureAwait(false);
            await SwapIfGreaterAsync(keys, comparer, index, hi, token).ConfigureAwait(false);
            T key = keys[index];
            await SwapAsync(keys, index, hi - 1, token).ConfigureAwait(false);
            int i = lo;
            int j = hi - 1;
            while (i < j)
            {
                do
                {
                    ++i;
                } while (await comparer(keys[i], key).ConfigureAwait(false) < 0);

                do
                {
                    --j;
                } while (await comparer(key, keys[j]).ConfigureAwait(false) < 0);

                if (i < j)
                    await SwapAsync(keys, i, j, token).ConfigureAwait(false);
                else
                    break;
            }

            await SwapAsync(keys, i, hi - 1, token).ConfigureAwait(false);
            return i;
        }

        private static async Task HeapsortAsync<T>(T[] keys, int lo, int hi, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; --i)
                await DownHeapAsync(keys, i, n, lo, comparer, token).ConfigureAwait(false);
            for (int index = n; index > 1; --index)
            {
                await SwapAsync(keys, lo, lo + index - 1, token).ConfigureAwait(false);
                await DownHeapAsync(keys, 1, index - 1, lo, comparer, token).ConfigureAwait(false);
            }
        }

        private static async Task DownHeapAsync<T>(T[] keys, int i, int n, int lo, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T key = keys[lo + i - 1];
            int num;
            for (; i <= n / 2; i = num)
            {
                token.ThrowIfCancellationRequested();
                num = 2 * i;
                if (num < n && await comparer(keys[lo + num - 1], keys[lo + num]).ConfigureAwait(false) < 0)
                    ++num;
                if (await comparer(key, keys[lo + num - 1]).ConfigureAwait(false) < 0)
                    keys[lo + i - 1] = keys[lo + num - 1];
                else
                    break;
            }
            keys[lo + i - 1] = key;
        }

        private static async Task InsertionSortAsync<T>(T[] keys, int lo, int hi, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (int index1 = lo; index1 < hi; ++index1)
            {
                token.ThrowIfCancellationRequested();
                int index2 = index1;
                T key;
                for (key = keys[index1 + 1]; index2 >= lo && await comparer(key, keys[index2]).ConfigureAwait(false) < 0; --index2)
                {
                    token.ThrowIfCancellationRequested();
                    keys[index2 + 1] = keys[index2];
                }

                keys[index2 + 1] = key;
            }
        }
    }
}
