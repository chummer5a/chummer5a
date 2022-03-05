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
using System.Threading.Tasks;

namespace Chummer
{
    public interface IAsyncEnumerable<T> : IEnumerable<T>
    {
        ValueTask<IEnumerator<T>> GetEnumeratorAsync();
    }

    public static class AsyncEnumerableExtensions
    {
        public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable)
        {
            List<T> lstReturn = objEnumerable is ICollection<T> lstEnumerable
                ? new List<T>(lstEnumerable.Count)
                : new List<T>();
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    lstReturn.Add(objEnumerator.Current);
            }
            return lstReturn;
        }

        public static async ValueTask ForEachAsync<T>(this IAsyncEnumerable<T> objEnumerable, Action<T> objFuncToRun)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    objFuncToRun.Invoke(objEnumerator.Current);
            }
        }

        public static async ValueTask ForEachAsync<T>(this IAsyncEnumerable<T> objEnumerable, Func<T, Task> objFuncToRun)
        {
            using (IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    await objFuncToRun.Invoke(objEnumerator.Current);
            }
        }

        public static async ValueTask ForEachAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, Action<T> objFuncToRun)
        {
            using (IEnumerator<T> objEnumerator = await (await tskEnumerable).GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    objFuncToRun.Invoke(objEnumerator.Current);
            }
        }

        public static async ValueTask ForEachAsync<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, Func<T, Task> objFuncToRun)
        {
            using (IEnumerator<T> objEnumerator = await (await tskEnumerable).GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                    await objFuncToRun.Invoke(objEnumerator.Current);
            }
        }

        public static async ValueTask ForEachWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, Func<T, bool> objFuncToRunWithPossibleTerminate)
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

        public static async ValueTask ForEachWithBreak<T>(this IAsyncEnumerable<T> objEnumerable, Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
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

        public static async ValueTask ForEachWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            using (IEnumerator<T> objEnumerator = await (await tskEnumerable).GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (!objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                        return;
                }
            }
        }

        public static async ValueTask ForEachWithBreak<T>(this Task<IAsyncEnumerable<T>> tskEnumerable, Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            using (IEnumerator<T> objEnumerator = await (await tskEnumerable).GetEnumeratorAsync())
            {
                while (objEnumerator.MoveNext())
                {
                    if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                        return;
                }
            }
        }
    }
}
