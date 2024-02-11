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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Special class that should be used instead of GetEnumerator for collections with an internal locking object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class LockingEnumerator<T> : IEnumerator<T>, IAsyncDisposable
    {
        private readonly IDisposable _objMyRelease;
        private readonly IAsyncDisposable _objMyReleaseAsync;

        private IEnumerator<T> _objInternalEnumerator;

        public static LockingEnumerator<T> Get(IHasLockObject objMyParent, CancellationToken token = default)
        {
            IDisposable objMyRelease = objMyParent.LockObject.EnterReadLock(token);
            return new LockingEnumerator<T>(objMyRelease);
        }

        public static async Task<LockingEnumerator<T>> GetAsync(IHasLockObject objMyParent, CancellationToken token = default)
        {
            IAsyncDisposable objMyRelease = await objMyParent.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                await objMyRelease.DisposeAsync().ConfigureAwait(false);
                throw;
            }
            return new LockingEnumerator<T>(objMyRelease);
        }

        public static LockingEnumerator<T> GetWithSideEffects(IHasLockObject objMyParent, CancellationToken token = default)
        {
            IDisposable objMyRelease = objMyParent.LockObject.EnterReadLockWithUpgradeableParent(token);
            return new LockingEnumerator<T>(objMyRelease);
        }

        public static async Task<LockingEnumerator<T>> GetWithSideEffectsAsync(IHasLockObject objMyParent, CancellationToken token = default)
        {
            IAsyncDisposable objMyRelease = await objMyParent.LockObject.EnterReadLockWithUpgradeableParentAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                await objMyRelease.DisposeAsync().ConfigureAwait(false);
                throw;
            }
            return new LockingEnumerator<T>(objMyRelease);
        }

        private LockingEnumerator(IDisposable objMyRelease)
        {
            _objMyRelease = objMyRelease;
        }

        private LockingEnumerator(IAsyncDisposable objMyReleaseAsync)
        {
            _objMyReleaseAsync = objMyReleaseAsync;
        }

        public void SetEnumerator(IEnumerator<T> objInternalEnumerator)
        {
            if (_objInternalEnumerator != null)
                throw new ArgumentException(null, nameof(objInternalEnumerator));
            _objInternalEnumerator = objInternalEnumerator;
        }

        /// <inheritdoc />
        public void Dispose()
        {
#if DEBUG
            if (_objMyReleaseAsync != null)
            {
                // Tried to synchronously dispose an enumerator that was created asynchronously, sign of bad code.
                Utils.BreakIfDebug();
            }
#endif
            _objInternalEnumerator.Dispose();
            _objMyRelease?.Dispose();
            if (_objMyReleaseAsync != null)
            {
                // We need to create the task first before awaiting it because the actual assignment of AsyncLocals must happen in the right place (outside of the safe-awaiter function)
                Task tskDispose = _objMyReleaseAsync.DisposeAsync().AsTask();
                Utils.SafelyRunSynchronously(() => tskDispose);
            }
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _objInternalEnumerator.Dispose();
            _objMyRelease?.Dispose();
            return _objMyReleaseAsync?.DisposeAsync() ?? default;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            return _objInternalEnumerator.MoveNext();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _objInternalEnumerator.Reset();
        }

        /// <inheritdoc />
        public T Current => _objInternalEnumerator.Current;

        /// <inheritdoc />
        object IEnumerator.Current => Current;
    }
}
