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
    /// Syntactic Sugar for wrapping a AsyncFriendlyReaderWriterLock's EnterReadLock() and ExitReadLock() methods into something that hooks into `using`
    /// </summary>
    public readonly struct EnterReadLock : IDisposable, IEquatable<EnterReadLock>
    {
        private readonly AsyncFriendlyReaderWriterLock _rwlMyLock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnterReadLock Enter(AsyncFriendlyReaderWriterLock rwlMyLock, CancellationToken token = default)
        {
            rwlMyLock.EnterReadLock(token);
            return new EnterReadLock(rwlMyLock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnterReadLock Enter(IHasLockObject rwlMyLockCarrier, CancellationToken token = default)
        {
            AsyncFriendlyReaderWriterLock rwlMyLock = rwlMyLockCarrier.LockObject;
            return Enter(rwlMyLock, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ValueTask<EnterReadLock> EnterAsync(AsyncFriendlyReaderWriterLock rwlMyLock, CancellationToken token = default)
        {
            await rwlMyLock.EnterReadLockAsync(token).ConfigureAwait(false);
            return new EnterReadLock(rwlMyLock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<EnterReadLock> EnterAsync(IHasLockObject rwlMyLockCarrier, CancellationToken token = default)
        {
            AsyncFriendlyReaderWriterLock rwlMyLock = rwlMyLockCarrier.LockObject;
            return EnterAsync(rwlMyLock, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnterReadLock(AsyncFriendlyReaderWriterLock rwlMyLock)
        {
            _rwlMyLock = rwlMyLock;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _rwlMyLock.ExitReadLock();
        }

        /// <inheritdoc />
        public bool Equals(EnterReadLock other)
        {
            return _rwlMyLock.Equals(other._rwlMyLock);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EnterReadLock other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _rwlMyLock.GetHashCode();
        }
    }
}
