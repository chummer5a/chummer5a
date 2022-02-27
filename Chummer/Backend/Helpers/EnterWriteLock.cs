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
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Syntactic Sugar for wrapping a AsyncFriendlyReaderWriterLock's EnterWriteLock() and ExitWriteLock() methods into something that hooks into `using`
    /// </summary>
    public sealed class EnterWriteLock : IDisposable, IAsyncDisposable
    {
        private readonly AsyncFriendlyReaderWriterLock _rwlMyLock;
        private AsyncFriendlyReaderWriterLock.SafeSemaphoreWriterRelease _rwlMyWriteRelease;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnterWriteLock(AsyncFriendlyReaderWriterLock rwlMyLock, bool blnEnterLockSync = true, CancellationToken token = default)
        {
            _rwlMyLock = rwlMyLock;
            if (blnEnterLockSync)
                _rwlMyWriteRelease = _rwlMyLock.EnterWriteLock(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnterWriteLock(IHasLockObject rwlMyLock, bool blnEnterLockSync = true, CancellationToken token = default)
        {
            _rwlMyLock = rwlMyLock.LockObject;
            if (blnEnterLockSync)
                _rwlMyWriteRelease = _rwlMyLock.EnterWriteLock(token);
        }

        public async Task<EnterWriteLock> EnterLockAsync(CancellationToken token = default)
        {
            _rwlMyWriteRelease = await _rwlMyLock.EnterWriteLockAsync(token);
            return this;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _rwlMyLock.ExitWriteLock(_rwlMyWriteRelease);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask DisposeAsync()
        {
            return _rwlMyLock.ExitWriteLockAsync(_rwlMyWriteRelease);
        }
    }
}
