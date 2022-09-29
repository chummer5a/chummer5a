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
using Chummer.Annotations;

namespace Chummer
{
    public interface IAsyncEnumerable<T> : IEnumerable<T>
    {
        ValueTask<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default);
    }

    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case IAsyncReadOnlyCollection<T> lstAsyncReadOnlyEnumerable:
                    lstReturn = new List<T>(await lstAsyncReadOnlyEnumerable.GetCountAsync(token));
                    break;

                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lstReturn.Add(objEnumerator.Current);
                }
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case IAsyncReadOnlyCollection<T> lstAsyncReadOnlyEnumerable:
                    lstReturn = new List<T>(await lstAsyncReadOnlyEnumerable.GetCountAsync(token));
                    break;

                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        lstReturn.Add(objEnumerator.Current);
                }
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case IAsyncReadOnlyCollection<T> lstAsyncReadOnlyEnumerable:
                    lstReturn = new List<T>(await lstAsyncReadOnlyEnumerable.GetCountAsync(token));
                    break;

                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        lstReturn.Add(objEnumerator.Current);
                }
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        lstReturn.Add(objEnumerator.Current);
                }
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            return await ToListAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await ToListAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await ToListAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                if (objEnumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, CancellationToken token = default)
        {
            return await AnyAsync(await tskEnumerable, token);
        }

        public static async Task<bool> AnyAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            return await AnyAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<bool> AnyAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await AnyAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<bool> AnyAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await AnyAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<bool> AllAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!await funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!await funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            return await AllAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<bool> AllAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await AllAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<bool> AllAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await AllAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, CancellationToken token = default)
        {
            return await CountAsync(await tskEnumerable, token);
        }

        public static async Task<int> CountAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            return await CountAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<int> CountAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await CountAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<int> CountAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await CountAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop;
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<T> AggregateAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, T, T> funcAggregator, CancellationToken token = default)
        {
            T objReturn;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                if (!objEnumerator.MoveNext())
                {
                    throw new ArgumentException("Enumerable has no elements", nameof(objEnumerable));
                }

                objReturn = objEnumerator.Current;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<T> AggregateAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, T, Task<T>> funcAggregator, CancellationToken token = default)
        {
            T objReturn;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                if (!objEnumerator.MoveNext())
                {
                    throw new ArgumentException("Enumerable has no elements", nameof(objEnumerable));
                }

                objReturn = objEnumerator.Current;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = await funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<T> AggregateAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, T, T> funcAggregator, CancellationToken token = default)
        {
            return await AggregateAsync(await tskEnumerable, funcAggregator, token);
        }

        public static async Task<T> AggregateAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, T, Task<T>> funcAggregator, CancellationToken token = default)
        {
            return await AggregateAsync(await tskEnumerable, funcAggregator, token);
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this IAsyncEnumerable<T> objEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, TAccumulate> funcAggregator, CancellationToken token = default)
        {
            TAccumulate objReturn = objSeed;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this IAsyncEnumerable<T> objEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, Task<TAccumulate>> funcAggregator, CancellationToken token = default)
        {
            TAccumulate objReturn = objSeed;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = await funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this Task<IAsyncEnumerable<T>> tskEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, TAccumulate> funcAggregator, CancellationToken token = default)
        {
            return await AggregateAsync(await tskEnumerable, objSeed, funcAggregator, token);
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this Task<IAsyncEnumerable<T>> tskEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, Task<TAccumulate>> funcAggregator, CancellationToken token = default)
        {
            return await AggregateAsync(await tskEnumerable, objSeed, funcAggregator, token);
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Max(intReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MaxAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Max(lngReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Max(fltReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Max(dblReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Max(decReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Max(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Max(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Max(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Max(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Max(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Max(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Max(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Max(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Max(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Max(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Max(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Max(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Max(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Max(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Max(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Max(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Max(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Max(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Max(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Max(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Max(intReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Max(lngReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Max(fltReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Max(dblReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Max(decReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Max(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Max(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Max(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Max(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Max(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Max(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Max(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Max(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Max(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Max(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Max(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Max(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Max(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Max(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Max(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Max(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Max(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Max(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Max(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Max(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn = Math.Max(intReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Max(lngReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Max(fltReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Max(dblReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn = Math.Max(decReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Max(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Max(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Max(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Max(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Max(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Max(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Max(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Max(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Max(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Max(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Max(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Max(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Max(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Max(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Max(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Max(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Max(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Max(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Max(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Max(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MaxParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Min(intReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Min(lngReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Min(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Min(fltReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Min(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Min(dblReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Min(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Min(decReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Min(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Min(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Min(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Min(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Min(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Min(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Min(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Min(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Min(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<long> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Min(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Min(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Min(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Min(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<float> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Min(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Min(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Min(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Min(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<double> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Min(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Min(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Min(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Min(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<decimal> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Min(intReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Min(lngReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Min(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Min(fltReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Min(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Min(dblReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Min(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Min(decReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Min(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Min(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Min(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Min(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Min(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Min(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Min(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Min(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Min(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Min(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Min(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Min(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Min(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Min(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Min(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Min(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Min(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Min(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Min(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Min(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Min(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn = Math.Min(intReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Min(lngReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Min(lngReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return lngReturn;
        }

        public static async Task<long> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Min(fltReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Min(fltReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return fltReturn;
        }

        public static async Task<float> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Min(dblReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Min(dblReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return dblReturn;
        }

        public static async Task<double> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn = Math.Min(decReturn, funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn = Math.Min(decReturn, await funcSelector.Invoke(objEnumerator.Current));
                }
            }
            return decReturn;
        }

        public static async Task<decimal> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Min(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Min(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn = Math.Min(intReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn = Math.Min(intReturn, await tskLoop);
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<int> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Min(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Min(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn = Math.Min(lngReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn = Math.Min(lngReturn, await tskLoop);
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<long> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Min(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Min(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn = Math.Min(fltReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn = Math.Min(fltReturn, await tskLoop);
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<float> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Min(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Min(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn = Math.Min(dblReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn = Math.Min(dblReturn, await tskLoop);
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<double> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Min(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Min(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn = Math.Min(decReturn, await tskLoop);
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn = Math.Min(decReturn, await tskLoop);
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<decimal> MinParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await MinParallelAsync(await tskEnumerable, funcPredicate, funcSelector, token);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, CancellationToken token = default)
        {
            return await FirstOrDefaultAsync(await tskEnumerable, token);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            return await FirstOrDefaultAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await FirstOrDefaultAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, CancellationToken token = default)
        {
            return await LastOrDefaultAsync(await tskEnumerable, token);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            return await LastOrDefaultAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await LastOrDefaultAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await FirstOrDefaultAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }
        
        public static async Task<T> LastOrDefaultAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            return await LastOrDefaultAsync(await tskEnumerable, funcPredicate, token);
        }

        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objFuncToRun.Invoke(objEnumerator.Current);
                }
            }
        }

        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    await objFuncToRun.Invoke(objEnumerator.Current);
                }
            }
        }

        public static async Task ForEachAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            await ForEachAsync(await tskEnumerable, objFuncToRun, token);
        }

        public static async Task ForEachAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            await ForEachAsync(await tskEnumerable, objFuncToRun, token);
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                        return;
                }
            }
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                        return;
                }
            }
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate, token);
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate, token);
        }

        public static async Task ForEachParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task>(Utils.MaxParallelBatchSize);
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objFuncToRun.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
        }

        public static async Task ForEachParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token)))
                : new List<Task>(Utils.MaxParallelBatchSize);
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token))
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objFuncToRun.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks);
                        token.ThrowIfCancellationRequested();
                        lstTasks.Clear();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks);
        }

        public static async Task ForEachParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            await ForEachParallelAsync(await tskEnumerable, objFuncToRun, token);
        }

        public static async Task ForEachParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            await ForEachParallelAsync(await tskEnumerable, objFuncToRun, token);
        }

        public static async Task ForEachParallelWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            // ReSharper disable once AccessToDisposedClosure
            using (token.Register(() => objSource.Cancel(false)))
            {
                CancellationToken objToken = objSource.Token;
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(objToken)))
                    : new List<Task>(Utils.MaxParallelBatchSize);
                using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(objToken))
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent), objToken).ContinueWith(
                                             async x =>
                                             {
                                                 if (await x)
                                                     // ReSharper disable once AccessToDisposedClosure
                                                     objSource.Cancel(false);
                                             }, objToken).Unwrap());
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                await Task.WhenAll(lstTasks);
                            }
                            catch (OperationCanceledException)
                            {
                                token.ThrowIfCancellationRequested();
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            lstTasks.Clear();
                        }
                    }
                }

                try
                {
                    await Task.WhenAll(lstTasks);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public static async Task ForEachParallelWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            // ReSharper disable once AccessToDisposedClosure
            using (token.Register(() => objSource.Cancel(false)))
            {
                CancellationToken objToken = objSource.Token;
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(objToken)))
                    : new List<Task>(Utils.MaxParallelBatchSize);
                using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(objToken))
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent), objToken).ContinueWith(
                                             async x =>
                                             {
                                                 if (await x)
                                                     // ReSharper disable once AccessToDisposedClosure
                                                     objSource.Cancel(false);
                                             }, objToken).Unwrap());
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                await Task.WhenAll(lstTasks);
                            }
                            catch (OperationCanceledException)
                            {
                                token.ThrowIfCancellationRequested();
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            lstTasks.Clear();
                        }
                    }
                }

                try
                {
                    await Task.WhenAll(lstTasks);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public static async Task ForEachParallelWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachParallelWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate, token);
        }

        public static async Task ForEachParallelWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachParallelWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate, token);
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<TSource> DeepAggregateAsync<TSource>(this IAsyncEnumerable<TSource> objParentList, Func<TSource, IAsyncEnumerable<TSource>> funcGetChildrenMethod, Func<TSource, TSource, TSource> funcAggregate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return Task.FromResult<TSource>(default);
            return objParentList.AggregateAsync<TSource, TSource>(default,
                                                                  async (current, objLoopChild) => funcAggregate(
                                                                      funcAggregate(current, objLoopChild),
                                                                      await funcGetChildrenMethod(objLoopChild)
                                                                          .DeepAggregateAsync(
                                                                              funcGetChildrenMethod, funcAggregate, token)), token);
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<TAccumulate> DeepAggregateAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> objParentList, Func<TSource, IAsyncEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objParentList == null
                ? seed
                : await objParentList.AggregateAsync(seed,
                    (current, objLoopChild) => funcGetChildrenMethod(objLoopChild).DeepAggregateAsync(funcGetChildrenMethod,
                        funcAggregate(current, objLoopChild), funcAggregate, token), token);
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<TResult> DeepAggregateAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> objParentList, Func<TSource, IAsyncEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, Func<TAccumulate, TResult> resultSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return resultSelector == null
                ? default
                : resultSelector(await objParentList.DeepAggregateAsync(funcGetChildrenMethod, seed, funcAggregate, token));
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAllAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AllAsync(async objLoopChild =>
                predicate(objLoopChild) &&
                await funcGetChildrenMethod(objLoopChild).DeepAllAsync(funcGetChildrenMethod, predicate, token), token);
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAnyAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AnyAsync(async objLoopChild =>
                predicate(objLoopChild) ||
                await funcGetChildrenMethod(objLoopChild).DeepAnyAsync(funcGetChildrenMethod, predicate, token), token);
        }

        /// <summary>
        /// Similar to LINQ's Count(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        ++intReturn;
                    intReturn += await funcGetChildrenMethod(objLoopChild).DeepCountAsync(funcGetChildrenMethod, predicate, token);
                }
            }
            return intReturn;
        }

        /// <summary>
        /// Similar to LINQ's Count() without predicate, but deep searches the list, counting up the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    intReturn += 1 + await funcGetChildrenMethod(objEnumerator.Current).DeepCountAsync(funcGetChildrenMethod, token);
                }
            }
            return intReturn;
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    try
                    {
                        return await funcGetChildrenMethod(objLoopChild)
                            .DeepFirstAsync(funcGetChildrenMethod, predicate, token);
                    }
                    catch (InvalidOperationException)
                    {
                        //swallow this
                    }
                }
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate, token);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                throw new InvalidOperationException();
            bool blnFoundValue = false;
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                    {
                        objReturn = objLoopChild;
                        blnFoundValue = true;
                    }
                    T objTemp;
                    try
                    {
                        objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastAsync(funcGetChildrenMethod, predicate, token);
                        blnFoundValue = true;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                    objReturn = objTemp;
                }
            }
            if (blnFoundValue)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                throw new InvalidOperationException();
            bool blnFoundValue = false;
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    blnFoundValue = true;
                    objReturn = objEnumerator.Current;
                }
                try
                {
                    T objTemp = await funcGetChildrenMethod(objReturn).DeepLastAsync(funcGetChildrenMethod, token);
                    objReturn = objTemp;
                }
                catch (InvalidOperationException)
                {
                    //swallow this
                }
            }
            if (blnFoundValue)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        objReturn = objLoopChild;
                    T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate, token);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token))
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild))
                        objReturn = objLoopChild;
                    T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate, token);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = await objParentList.LastOrDefaultAsync(token);
            if (funcGetChildrenMethod != null)
            {
                List<T> lstChildren = await funcGetChildrenMethod(objReturn).ToListAsync(token);
                if (lstChildren.Count > 0)
                {
                    T objTemp = lstChildren.DeepLastOrDefault(funcGetChildrenMethod);
                    if (objTemp?.Equals(default(T)) == false)
                        return objTemp;
                }
            }
            return objReturn;
        }
    }
}
