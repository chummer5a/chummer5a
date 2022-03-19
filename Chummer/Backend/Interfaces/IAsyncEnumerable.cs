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
        ValueTask<IEnumerator<T>> GetEnumeratorAsync();
    }

    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable)
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
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    lstReturn.Add(objEnumerator.Current);
            }
            return lstReturn;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                if (objEnumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable)
        {
            return await AnyAsync(await tskEnumerable);
        }

        public static async Task<bool> AnyAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            return await AnyAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<bool> AnyAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            return await AnyAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<bool> AllAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (!funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (!await funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            return await AllAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<bool> AllAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            return await AllAsync(await tskEnumerable, funcPredicate);
        }
        
        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        ++intReturn;
                }
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable)
        {
            return await CountAsync(await tskEnumerable);
        }

        public static async Task<int> CountAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            return await CountAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<int> CountAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            return await CountAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    intReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }
        
        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    lngReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }
        
        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    fltReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    dblReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }
        
        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }
        
        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    decReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcSelector);
        }
        
        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(await objTemp.CountAsync)
                : new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            int intReturn = 0;
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(await objTemp.CountAsync)
                : new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            int intReturn = 0;
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }
        
        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(await objTemp.CountAsync)
                : new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            long lngReturn = 0;
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(await objTemp.CountAsync)
                : new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            long lngReturn = 0;
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }
        

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }
        
        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(await objTemp.CountAsync)
                : new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            float fltReturn = 0;
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(await objTemp.CountAsync)
                : new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            float fltReturn = 0;
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(await objTemp.CountAsync)
                : new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            double dblReturn = 0;
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(await objTemp.CountAsync)
                : new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            double dblReturn = 0;
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(await objTemp.CountAsync)
                : new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            decimal decReturn = 0;
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(await objTemp.CountAsync)
                : new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
            decimal decReturn = 0;
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcSelector);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(await objTemp.CountAsync)
                : new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            int intReturn = 0;
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(await objTemp.CountAsync)
                : new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            int intReturn = 0;
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(await objTemp.CountAsync)
                : new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            long lngReturn = 0;
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(await objTemp.CountAsync)
                : new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            long lngReturn = 0;
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }


        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(await objTemp.CountAsync)
                : new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            float fltReturn = 0;
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(await objTemp.CountAsync)
                : new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            float fltReturn = 0;
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(await objTemp.CountAsync)
                : new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            double dblReturn = 0;
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(await objTemp.CountAsync)
                : new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            double dblReturn = 0;
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(await objTemp.CountAsync)
                : new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            decimal decReturn = 0;
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(await objTemp.CountAsync)
                : new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            decimal decReturn = 0;
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        intReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return intReturn;
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            long lngReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        lngReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return lngReturn;
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            float fltReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        fltReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return fltReturn;
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            double dblReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        dblReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return dblReturn;
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            decimal decReturn = 0;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate(objEnumerator.Current))
                        decReturn += await funcSelector.Invoke(objEnumerator.Current);
                }
            }
            return decReturn;
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            return await SumAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(await objTemp.CountAsync)
                : new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            int intReturn = 0;
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(await objTemp.CountAsync)
                : new List<Task<int>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            int intReturn = 0;
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop;
            return intReturn;
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<int> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(await objTemp.CountAsync)
                : new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            long lngReturn = 0;
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(await objTemp.CountAsync)
                : new List<Task<long>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            long lngReturn = 0;
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop;
            return lngReturn;
        }


        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<long> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(await objTemp.CountAsync)
                : new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            float fltReturn = 0;
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(await objTemp.CountAsync)
                : new List<Task<float>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            float fltReturn = 0;
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop;
            return fltReturn;
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<float> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(await objTemp.CountAsync)
                : new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            double dblReturn = 0;
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(await objTemp.CountAsync)
                : new List<Task<double>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            double dblReturn = 0;
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop;
            return dblReturn;
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<double> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(await objTemp.CountAsync)
                : new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            decimal decReturn = 0;
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(await objTemp.CountAsync)
                : new List<Task<decimal>>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(async () => await funcPredicate(objCurrent) ? await funcSelector.Invoke(objCurrent) : 0));
                }
            }
            await Task.WhenAll(lstTasks);
            decimal decReturn = 0;
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop;
            return decReturn;
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<decimal> SumParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector)
        {
            return await SumParallelAsync(await tskEnumerable, funcPredicate, funcSelector);
        }

        public static async Task<T> AggregateAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, T, T> funcAggregator)
        {
            T objReturn;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                if (!objEnumerator.MoveNext())
                {
                    throw new ArgumentException("Enumerable has no elements", nameof(objEnumerable));
                }

                objReturn = objEnumerator.Current;
                while (objEnumerator.MoveNext())
                {
                    objReturn = funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<T> AggregateAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, T, Task<T>> funcAggregator)
        {
            T objReturn;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                if (!objEnumerator.MoveNext())
                {
                    throw new ArgumentException("Enumerable has no elements", nameof(objEnumerable));
                }

                objReturn = objEnumerator.Current;
                while (objEnumerator.MoveNext())
                {
                    objReturn = await funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }
        public static async Task<T> AggregateAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, T, T> funcAggregator)
        {
            return await AggregateAsync(await tskEnumerable, funcAggregator);
        }

        public static async Task<T> AggregateAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, T, Task<T>> funcAggregator)
        {
            return await AggregateAsync(await tskEnumerable, funcAggregator);
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this IAsyncEnumerable<T> objEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, TAccumulate> funcAggregator)
        {
            TAccumulate objReturn = objSeed;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    objReturn = funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this IAsyncEnumerable<T> objEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, Task<TAccumulate>> funcAggregator)
        {
            TAccumulate objReturn = objSeed;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    objReturn = await funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            return objReturn;
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this Task<IAsyncEnumerable<T>> tskEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, TAccumulate> funcAggregator)
        {
            return await AggregateAsync(await tskEnumerable, objSeed, funcAggregator);
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this Task<IAsyncEnumerable<T>> tskEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, Task<TAccumulate>> funcAggregator)
        {
            return await AggregateAsync(await tskEnumerable, objSeed, funcAggregator);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                if (objEnumerator.MoveNext())
                {
                    return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        return objEnumerator.Current;
                }
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable)
        {
            return await FirstOrDefaultAsync(await tskEnumerable);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            return await FirstOrDefaultAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            return await FirstOrDefaultAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (await funcPredicate.Invoke(objEnumerator.Current))
                        objReturn = objEnumerator.Current;
                }
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable)
        {
            return await LastOrDefaultAsync(await tskEnumerable);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate)
        {
            return await LastOrDefaultAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate)
        {
            return await LastOrDefaultAsync(await tskEnumerable, funcPredicate);
        }

        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Action<T> objFuncToRun)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    objFuncToRun.Invoke(objEnumerator.Current);
            }
        }

        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    await objFuncToRun.Invoke(objEnumerator.Current);
            }
        }

        public static async Task ForEachAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Action<T> objFuncToRun)
        {
            await ForEachAsync(await tskEnumerable, objFuncToRun);
        }

        public static async Task ForEachAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun)
        {
            await ForEachAsync(await tskEnumerable, objFuncToRun);
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        public static async Task ForEachWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
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
        public static async Task ForEachWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
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
        public static async Task ForEachWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            await ForEachWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate);
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        public static async Task ForEachWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            await ForEachWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate);
        }

        public static async Task ForEachParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Action<T> objFuncToRun)
        {
            List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task>(await objTemp.CountAsync)
                : new List<Task>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => objFuncToRun.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
        }

        public static async Task ForEachParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun)
        {
            List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task>(await objTemp.CountAsync)
                : new List<Task>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objCurrent = objEnumerator.Current;
                    lstTasks.Add(Task.Run(() => objFuncToRun.Invoke(objCurrent)));
                }
            }
            await Task.WhenAll(lstTasks);
        }

        public static async Task ForEachParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Action<T> objFuncToRun)
        {
            await ForEachParallelAsync(await tskEnumerable, objFuncToRun);
        }

        public static async Task ForEachParallelAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun)
        {
            await ForEachParallelAsync(await tskEnumerable, objFuncToRun);
        }

        public static async Task ForEachParallelWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            {
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(await objTemp.CountAsync)
                    : new List<Task>();
                CancellationToken objToken = objSource.Token;
                using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
                {
                    while (objEnumerator.MoveNext())
                    {
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent), objToken).ContinueWith(
                                         x =>
                                         {
                                             if (x.Result)
                                                 // ReSharper disable once AccessToDisposedClosure
                                                 objSource.Cancel(false);
                                         }, objToken));
                    }
                }

                try
                {
                    await Task.WhenAll(lstTasks);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }
        }

        public static async Task ForEachParallelWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            {
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(await objTemp.CountAsync)
                    : new List<Task>();
                CancellationToken objToken = objSource.Token;
                using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
                {
                    while (objEnumerator.MoveNext())
                    {
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent),
                                              objToken).ContinueWith(
                                         x =>
                                         {
                                             if (x.Result)
                                                 // ReSharper disable once AccessToDisposedClosure
                                                 objSource.Cancel(false);
                                         }, objToken));
                    }
                }

                try
                {
                    await Task.WhenAll(lstTasks);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }
        }

        public static async Task ForEachParallelWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            await ForEachParallelWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate);
        }

        public static async Task ForEachParallelWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            await ForEachParallelWithBreak(await tskEnumerable, objFuncToRunWithPossibleTerminate);
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<TSource> DeepAggregateAsync<TSource>(this IAsyncEnumerable<TSource> objParentList, Func<TSource, IAsyncEnumerable<TSource>> funcGetChildrenMethod, Func<TSource, TSource, TSource> funcAggregate)
        {
            if (objParentList == null)
                return Task.FromResult<TSource>(default);
            return objParentList.AggregateAsync<TSource, TSource>(default,
                                                                  async (current, objLoopChild) => funcAggregate(
                                                                      funcAggregate(current, objLoopChild),
                                                                      await funcGetChildrenMethod(objLoopChild)
                                                                          .DeepAggregateAsync(
                                                                              funcGetChildrenMethod, funcAggregate)));
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<TAccumulate> DeepAggregateAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> objParentList, Func<TSource, IAsyncEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate)
        {
            return objParentList == null
                ? seed
                : await objParentList.AggregateAsync(seed,
                    (current, objLoopChild) => funcGetChildrenMethod(objLoopChild).DeepAggregateAsync(funcGetChildrenMethod,
                        funcAggregate(current, objLoopChild), funcAggregate));
        }

        /// <summary>
        /// Similar to LINQ's Aggregate(), but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<TResult> DeepAggregateAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> objParentList, Func<TSource, IAsyncEnumerable<TSource>> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, Func<TAccumulate, TResult> resultSelector)
        {
            return resultSelector == null
                ? default
                : resultSelector(await objParentList.DeepAggregateAsync(funcGetChildrenMethod, seed, funcAggregate));
        }

        /// <summary>
        /// Similar to LINQ's All(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAllAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            return objParentList.AllAsync(async objLoopChild =>
                predicate(objLoopChild) &&
                await funcGetChildrenMethod(objLoopChild).DeepAllAsync(funcGetChildrenMethod, predicate));
        }

        /// <summary>
        /// Similar to LINQ's Any(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAnyAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            return objParentList.AnyAsync(async objLoopChild =>
                predicate(objLoopChild) ||
                await funcGetChildrenMethod(objLoopChild).DeepAnyAsync(funcGetChildrenMethod, predicate));
        }

        /// <summary>
        /// Similar to LINQ's Count(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        ++intReturn;
                    intReturn += await funcGetChildrenMethod(objLoopChild).DeepCountAsync(funcGetChildrenMethod, predicate);
                }
            }
            return intReturn;
        }

        /// <summary>
        /// Similar to LINQ's Count() without predicate, but deep searches the list, counting up the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod)
        {
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    intReturn += 1 + await funcGetChildrenMethod(objEnumerator.Current).DeepCountAsync(funcGetChildrenMethod);
                }
            }
            return intReturn;
        }

        /// <summary>
        /// Similar to LINQ's First(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's FirstOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            return default;
        }

        /// <summary>
        /// Similar to LINQ's Last(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            T objReturn = await objParentList.DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's Last() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod)
        {
            T objReturn = await objParentList.DeepLastOrDefaultAsync(funcGetChildrenMethod);
            if (objReturn?.Equals(default(T)) == false)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, bool> predicate)
        {
            if (objParentList == null)
                return default;
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        objReturn = objLoopChild;
                    T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to LINQ's LastOrDefault(), but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod, Func<T, Task<bool>> predicate)
        {
            if (objParentList == null)
                return default;
            T objReturn = default;
            using (IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild))
                        objReturn = objLoopChild;
                    T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            return objReturn;
        }
        
        

        /// <summary>
        /// Similar to LINQ's LastOrDefault() without a predicate, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T>(this IAsyncEnumerable<T> objParentList, Func<T, IAsyncEnumerable<T>> funcGetChildrenMethod)
        {
            if (objParentList == null)
                return default;
            T objReturn = await objParentList.LastOrDefaultAsync();
            if (funcGetChildrenMethod != null)
            {
                List<T> lstChildren = await funcGetChildrenMethod(objReturn).ToListAsync();
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
