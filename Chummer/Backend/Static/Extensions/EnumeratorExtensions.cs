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
    public static class EnumeratorExtensions
    {
        public static void ForEach<T>(this IEnumerator<T> objEnumerator, Action<T> objFuncToRun)
        {
            while (objEnumerator.MoveNext())
                objFuncToRun.Invoke(objEnumerator.Current);
        }

        public static async ValueTask ForEachAsync<T>(this Task<IEnumerator<T>> tskEnumerator, Action<T> objFuncToRun)
        {
            IEnumerator<T> objEnumerator = await tskEnumerator;
            while (objEnumerator.MoveNext())
                objFuncToRun.Invoke(objEnumerator.Current);
        }

        public static async ValueTask ForEachAsync<T>(this IEnumerator<T> objEnumerator, Func<T, Task> objFuncToRun)
        {
            while (objEnumerator.MoveNext())
                await objFuncToRun.Invoke(objEnumerator.Current);
        }

        public static async ValueTask ForEachAsync<T>(this Task<IEnumerator<T>> tskEnumerator, Func<T, Task> objFuncToRun)
        {
            IEnumerator<T> objEnumerator = await tskEnumerator;
            while (objEnumerator.MoveNext())
                await objFuncToRun.Invoke(objEnumerator.Current);
        }

        public static void ForEachWithBreak<T>(this IEnumerator<T> objEnumerator, Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            while (objEnumerator.MoveNext())
            {
                if (!objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                    return;
            }
        }

        public static async ValueTask ForEachWithBreakAsync<T>(this Task<IEnumerator<T>> tskEnumerator, Func<T, bool> objFuncToRunWithPossibleTerminate)
        {
            IEnumerator<T> objEnumerator = await tskEnumerator;
            while (objEnumerator.MoveNext())
            {
                if (!objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                    return;
            }
        }

        public static async ValueTask ForEachWithBreakAsync<T>(this IEnumerator<T> objEnumerator, Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            while (objEnumerator.MoveNext())
            {
                if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                    return;
            }
        }

        public static async ValueTask ForEachWithBreakAsync<T>(this Task<IEnumerator<T>> tskEnumerator, Func<T, Task<bool>> objFuncToRunWithPossibleTerminate)
        {
            IEnumerator<T> objEnumerator = await tskEnumerator;
            while (objEnumerator.MoveNext())
            {
                if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                    return;
            }
        }
    }
}
