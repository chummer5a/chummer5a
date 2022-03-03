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
using System.Runtime.CompilerServices;
using System.Threading;

namespace Chummer
{
    /// <summary>
    /// Syntactic Sugar for wrapping a AsyncFriendlyReaderWriterLock's EnterWriteLock() and ExitWriteLock() methods into something that hooks into `using`
    /// </summary>
    public readonly struct EnterWriteLock : IDisposable
    {
        private readonly AsyncFriendlyReaderWriterLock _rwlMyLock;
        private readonly IDisposable _rwlMyWriteRelease;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnterWriteLock Enter(AsyncFriendlyReaderWriterLock rwlMyLock, CancellationToken token = default)
        {
            IDisposable rwlMyWriteRelease = rwlMyLock.EnterWriteLock(token);
            return new EnterWriteLock(rwlMyLock, rwlMyWriteRelease);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnterWriteLock(AsyncFriendlyReaderWriterLock rwlMyLock, IDisposable rwlMyWriteRelease)
        {
            _rwlMyLock = rwlMyLock;
            _rwlMyWriteRelease = rwlMyWriteRelease;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _rwlMyLock.ExitWriteLock(_rwlMyWriteRelease);
        }
    }
}
