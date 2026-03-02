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
    public static class TaskExtensions
    {
        /// <summary>
        /// Version of <see cref="Task.Run(Action)"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task RunWithoutEC(Action func)
        {
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func));
        }

        /// <summary>
        /// Version of <see cref="Task.Run(Action, CancellationToken)"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task RunWithoutEC(Action func, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func, token));
        }

        /// <summary>
        /// Version of <see cref="Task.Run{TResult}(Func{TResult})"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task<T> RunWithoutEC<T>(Func<T> func)
        {
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func));
        }

        /// <summary>
        /// Version of <see cref="Task.Run{TResult}(Func{TResult}, CancellationToken)"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task<T> RunWithoutEC<T>(Func<T> func, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T>(token);
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func, token));
        }

        /// <summary>
        /// Version of <see cref="Task.Run(Func{Task})"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task RunWithoutEC(Func<Task> func)
        {
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func));
        }

        /// <summary>
        /// Version of <see cref="Task.Run(Func{Task}, CancellationToken)"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task RunWithoutEC(Func<Task> func, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func, token));
        }

        /// <summary>
        /// Version of <see cref="Task.Run{TResult}(Func{Task{TResult}})"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task<T> RunWithoutEC<T>(Func<Task<T>> func)
        {
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func));
        }

        /// <summary>
        /// Version of <see cref="Task.Run{TResult}(Func{Task{TResult}}, CancellationToken)"/> that runs in an empty execution context as a way to avoid leaking memory of AsyncLocals
        /// </summary>
        public static Task<T> RunWithoutEC<T>(Func<Task<T>> func, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T>(token);
            return Utils.RunInEmptyExecutionContext(() => Task.Run(func, token));
        }
    }
}
