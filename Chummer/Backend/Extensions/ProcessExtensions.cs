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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="objThis">The process to wait for cancellation.</param>
        /// <param name="objCancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task<int> StartAsync(this Process objThis, CancellationToken objCancellationToken = default)
        {
            TaskCompletionSource<int> objTaskCompletionSource = new TaskCompletionSource<int>();
            objThis.EnableRaisingEvents = true;
            objThis.Exited += (sender, args) => objTaskCompletionSource.TrySetResult(objThis.ExitCode);
            if (objCancellationToken != default)
                objCancellationToken.Register(() => objTaskCompletionSource.TrySetCanceled(objCancellationToken));
            objThis.Start();
            return objTaskCompletionSource.Task;
        }
    }
}
