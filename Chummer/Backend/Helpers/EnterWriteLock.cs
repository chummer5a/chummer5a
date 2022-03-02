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
    public readonly struct EnterWriteLock : IDisposable, IAsyncDisposable
    {
        private readonly AsyncFriendlyReaderWriterLock _rwlMyLock;
        private readonly IDisposable _rwlMyWriteRelease;
        private readonly IAsyncDisposable _rwlMyAsyncWriteRelease;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnterWriteLock Enter(AsyncFriendlyReaderWriterLock rwlMyLock, CancellationToken token = default)
        {
            IDisposable rwlMyWriteRelease = rwlMyLock.EnterWriteLock(token);
            return new EnterWriteLock(rwlMyLock, rwlMyWriteRelease);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnterWriteLock Enter(IHasLockObject rwlMyLockCarrier, CancellationToken token = default)
        {
            AsyncFriendlyReaderWriterLock rwlMyLock = rwlMyLockCarrier.LockObject;
            return Enter(rwlMyLock, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ValueTask<EnterWriteLock> EnterAsync(AsyncFriendlyReaderWriterLock rwlMyLock, CancellationToken token = default)
        {
            IAsyncDisposable rwlMyWriteRelease = await rwlMyLock.EnterWriteLockAsync(token);
            return new EnterWriteLock(rwlMyLock, rwlMyWriteRelease);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<EnterWriteLock> EnterAsync(IHasLockObject rwlMyLockCarrier, CancellationToken token = default)
        {
            AsyncFriendlyReaderWriterLock rwlMyLock = rwlMyLockCarrier.LockObject;
            return EnterAsync(rwlMyLock, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnterWriteLock(AsyncFriendlyReaderWriterLock rwlMyLock, IDisposable rwlMyWriteRelease)
        {
            _rwlMyLock = rwlMyLock;
            _rwlMyWriteRelease = rwlMyWriteRelease;
            _rwlMyAsyncWriteRelease = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnterWriteLock(AsyncFriendlyReaderWriterLock rwlMyLock, IAsyncDisposable rwlMyAsyncWriteRelease)
        {
            _rwlMyLock = rwlMyLock;
            _rwlMyWriteRelease = null;
            _rwlMyAsyncWriteRelease = rwlMyAsyncWriteRelease;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_rwlMyWriteRelease == null)
                throw new InvalidOperationException("Write lock was created synchronously, it must be disposed of synchronously");
            _rwlMyLock.ExitWriteLock(_rwlMyWriteRelease);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask DisposeAsync()
        {
            if (_rwlMyAsyncWriteRelease == null)
                throw new InvalidOperationException("Write lock was created asynchronously, it must be disposed of asynchronously");
            return _rwlMyLock.ExitWriteLockAsync(_rwlMyAsyncWriteRelease);
        }
    }
}
