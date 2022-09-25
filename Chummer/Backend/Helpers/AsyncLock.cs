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
    /// <summary>
    /// A version of a locker that works recursively and also works with Async/Await.
    /// Taken from the WCF project here:
    /// https://github.com/dotnet/wcf/blob/main/src/System.Private.ServiceModel/src/Internals/System/Runtime/AsyncLock.cs
    /// </summary>
    public sealed class AsyncLock : IDisposable, IAsyncDisposable
    {
        // In order to properly allow async lock to be recursive but still make them work properly as locks, we need to set up something
        // that is a bit like a singly-linked list. Each lock creates a disposable SafeSemaphoreRelease, and only disposing it frees the lock.
        private readonly AsyncLocal<DebuggableSemaphoreSlim> _objCurrentSemaphore = new AsyncLocal<DebuggableSemaphoreSlim>();

        private readonly DebuggableSemaphoreSlim _objTopLevelSemaphore = new DebuggableSemaphoreSlim();
        private int _intDisposedStatus;

        public Task<IAsyncDisposable> TakeLockAsync()
        {
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncLock)));
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore = _objCurrentSemaphore.Value ?? _objTopLevelSemaphore;
            _objCurrentSemaphore.Value = objNextSemaphore;
            SafeSemaphoreRelease objRelease = new SafeSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeLockCoreAsync(objCurrentSemaphore, objRelease);
        }

        /// <summary>
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> TakeLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncLock)));
            token.ThrowIfCancellationRequested();
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore = _objCurrentSemaphore.Value ?? _objTopLevelSemaphore;
            _objCurrentSemaphore.Value = objNextSemaphore;
            SafeSemaphoreRelease objRelease = new SafeSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeLockCoreAsync(objCurrentSemaphore, objRelease, token);
        }

        private static async Task<IAsyncDisposable> TakeLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, SafeSemaphoreRelease objRelease)
        {
            await objCurrentSemaphore.WaitAsync();
            return objRelease;
        }

        private static async Task<IAsyncDisposable> TakeLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, SafeSemaphoreRelease objRelease, CancellationToken token)
        {
            try
            {
                await objCurrentSemaphore.WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
            }
            return objRelease;
        }

        public IDisposable TakeLock(CancellationToken token = default)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(AsyncLock));
            token.ThrowIfCancellationRequested();
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore = _objCurrentSemaphore.Value ?? _objTopLevelSemaphore;
            _objCurrentSemaphore.Value = objNextSemaphore;
            try
            {
                objCurrentSemaphore.SafeWait(token);
            }
            catch
            {
                _objCurrentSemaphore.Value = objCurrentSemaphore;
                Utils.SemaphorePool.Return(ref objNextSemaphore);
                throw;
            }
            return new SafeSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
        }

        /// <summary>
        /// Are there any async locks held below this one?
        /// </summary>
        public bool IsLockHeldRecursively => IsLockHeld && _objCurrentSemaphore.Value != null && _objCurrentSemaphore.Value != _objTopLevelSemaphore;

        /// <summary>
        /// Is there anything holding the lock?
        /// </summary>
        public bool IsLockHeld => _intDisposedStatus == 0 && _objTopLevelSemaphore.CurrentCount == 0;

        /// <summary>
        /// Is the locker object already disposed and its allocatable semaphores returned to the semaphore pool?
        /// </summary>
        public bool IsDisposed => _intDisposedStatus > 1;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) != 0)
                return;

            try
            {
                // Ensure the lock isn't held. If it is, wait for it to be released
                // before completing the dispose.
                _objTopLevelSemaphore.SafeWait();
                _objTopLevelSemaphore.Release();
                _objTopLevelSemaphore.Dispose();
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) != 0)
                return;

            try
            {
                // Ensure the lock isn't held. If it is, wait for it to be released
                // before completing the dispose.
                await _objTopLevelSemaphore.WaitAsync();
                _objTopLevelSemaphore.Release();
                _objTopLevelSemaphore.Dispose();
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        private struct SafeSemaphoreRelease : IAsyncDisposable, IDisposable
        {
            private readonly DebuggableSemaphoreSlim _objCurrentSemaphore;
            private DebuggableSemaphoreSlim _objNextSemaphore;
            private readonly AsyncLock _objAsyncLock;

            public SafeSemaphoreRelease(DebuggableSemaphoreSlim objCurrentSemaphore, DebuggableSemaphoreSlim objNextSemaphore, AsyncLock objAsyncLock)
            {
                _objCurrentSemaphore = objCurrentSemaphore;
                _objNextSemaphore = objNextSemaphore;
                _objAsyncLock = objAsyncLock;
            }

            public ValueTask DisposeAsync()
            {
                if (_objAsyncLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objAsyncLock));
                DebuggableSemaphoreSlim objNextSemaphoreSlim = _objAsyncLock._objCurrentSemaphore.Value;
                if (_objNextSemaphore != objNextSemaphoreSlim)
                {
                    if (objNextSemaphoreSlim == _objCurrentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    if (objNextSemaphoreSlim == null)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore");
                }

                // Update _objAsyncLock._objCurrentSemaphore in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the
                // update will happen in a copy of the ExecutionContext and the caller
                // won't see the changes.
                _objAsyncLock._objCurrentSemaphore.Value = _objCurrentSemaphore;

                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                try
                {
                    if (_objCurrentSemaphore.CurrentCount == 0)
                    {
                        await _objNextSemaphore.WaitAsync();
                        try
                        {
                            _objCurrentSemaphore.Release();
                        }
                        finally
                        {
                            _objNextSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    Utils.SemaphorePool.Return(ref _objNextSemaphore);
                }
            }

            public void Dispose()
            {
                if (_objAsyncLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objAsyncLock));
                DebuggableSemaphoreSlim objNextSemaphoreSlim = _objAsyncLock._objCurrentSemaphore.Value;
                if (_objNextSemaphore != objNextSemaphoreSlim)
                {
                    if (objNextSemaphoreSlim == _objCurrentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    if (objNextSemaphoreSlim == null)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore");
                }

                _objAsyncLock._objCurrentSemaphore.Value = _objCurrentSemaphore;
                try
                {
                    if (_objCurrentSemaphore.CurrentCount == 0)
                    {
                        _objNextSemaphore.SafeWait();
                        try
                        {
                            _objCurrentSemaphore.Release();
                        }
                        finally
                        {
                            _objNextSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    Utils.SemaphorePool.Return(ref _objNextSemaphore);
                }
            }
        }
    }
}
