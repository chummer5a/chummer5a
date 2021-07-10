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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Starts a process and returns an await task linked to it.
        /// </summary>
        /// <param name="objProcess">The process to start.</param>
        /// <param name="objCancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
        /// <returns>A Task linked to the process' status code that will complete when the process exits.</returns>
        public static Task<int> StartAsync(this Process objProcess, CancellationToken objCancellationToken = default)
        {
            TaskCompletionSource<int> objTaskCompletionSource = new TaskCompletionSource<int>();
            objProcess.EnableRaisingEvents = true;
            objProcess.Exited += (sender, args) => objTaskCompletionSource.TrySetResult(objProcess.ExitCode);
            if (objCancellationToken != default)
                objCancellationToken.Register(() => objTaskCompletionSource.TrySetCanceled(objCancellationToken));
            objProcess.Start();
            return objTaskCompletionSource.Task;
        }

        /// <summary>
        /// Starts a process and returns an await task linked to it.
        /// </summary>
        /// <param name="objStartInfo">The process start info to use.</param>
        /// <param name="objCancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
        /// <returns>A Task linked to the process' status code that will complete when the process exits.</returns>
        public static Task<int> StartAsync(this ProcessStartInfo objStartInfo, CancellationToken objCancellationToken = default)
        {
            Process objProcess = new Process { StartInfo = objStartInfo };
            TaskCompletionSource<int> objTaskCompletionSource = new TaskCompletionSource<int>();
            objProcess.EnableRaisingEvents = true;
            objProcess.Exited += (sender, args) => objTaskCompletionSource.TrySetResult(objProcess.ExitCode);
            if (objCancellationToken != default)
                objCancellationToken.Register(() => objTaskCompletionSource.TrySetCanceled(objCancellationToken));
            objProcess.Start();
            return objTaskCompletionSource.Task;
        }

        /// <summary>
        /// Syntactic sugar for starting a process based on a process start info.
        /// </summary>
        /// <param name="objStartInfo">Info for process to start</param>
        /// <returns>Running process started from <paramref name="objStartInfo"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Process Start(this ProcessStartInfo objStartInfo)
        {
            return Process.Start(objStartInfo);
        }
    }
}