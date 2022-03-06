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
        // Needed because the pools are not necessarily initialized in static classes
        private readonly bool _blnTopLevelSemaphoreFromPool;
        // In order to properly allow async lock to be recursive but still make them work properly as locks, we need to set up something
        // that is a bit like a singly-linked list. Each lock creates a disposable SafeSemaphoreRelease, and only disposing it frees the lock.
        private readonly AsyncLocal<SemaphoreSlim> _objCurrentSemaphore = new AsyncLocal<SemaphoreSlim>();
        private readonly SemaphoreSlim _objTopLevelSemaphore;
        private bool _blnIsDisposed;

        public AsyncLock()
        {
            if (Utils.SemaphorePool != null)
            {
                _objTopLevelSemaphore = Utils.SemaphorePool.Get();
                _blnTopLevelSemaphoreFromPool = true;
            }
            else
            {
                _objTopLevelSemaphore = new SemaphoreSlim(1, 1);
                _blnTopLevelSemaphoreFromPool = false;
            }
        }

        public Task<IAsyncDisposable> TakeLockAsync()
        {
            if (_blnIsDisposed)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncLock)));
            SemaphoreSlim objCurrentSemaphore = _objCurrentSemaphore.Value ?? _objTopLevelSemaphore;
            SemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentSemaphore.Value = objNextSemaphore;
            SafeSemaphoreRelease objRelease = new SafeSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeLockCoreAsync(objCurrentSemaphore, objRelease);
        }

        private static async Task<IAsyncDisposable> TakeLockCoreAsync(SemaphoreSlim objCurrentSemaphore, SafeSemaphoreRelease objRelease)
        {
            await objCurrentSemaphore.WaitAsync();
            return objRelease;
        }

        public IDisposable TakeLock()
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncLock));
            SemaphoreSlim objCurrentSemaphore = _objCurrentSemaphore.Value ?? _objTopLevelSemaphore;
            SemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentSemaphore.Value = objNextSemaphore;
            if (Utils.EverDoEvents)
            {
                while (!objCurrentSemaphore.Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe();
            }
            else
                objCurrentSemaphore.Wait();
            return new SafeSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
        }

        /// <summary>
        /// Are there any async locks held below this one?
        /// </summary>
        public bool IsLockHeldRecursively => IsLockHeld && _objCurrentSemaphore.Value != null && _objCurrentSemaphore.Value.CurrentCount == 0;

        /// <summary>
        /// Is there anything holding the lock?
        /// </summary>
        public bool IsLockHeld => !IsDisposed && _objTopLevelSemaphore.CurrentCount == 0;

        /// <summary>
        /// Is the locker object already disposed and its allocatable semaphores returned to the semaphore pool?
        /// </summary>
        public bool IsDisposed => _blnIsDisposed;

        public void Dispose()
        {
            if (_blnIsDisposed)
                return;

            _blnIsDisposed = true;
            // Ensure the lock isn't held. If it is, wait for it to be released
            // before completing the dispose.
            if (Utils.EverDoEvents)
            {
                while (!_objTopLevelSemaphore.Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe();
            }
            else
                _objTopLevelSemaphore.Wait();
            _objTopLevelSemaphore.Release();
            if (_blnTopLevelSemaphoreFromPool)
                Utils.SemaphorePool.Return(_objTopLevelSemaphore);
            else
                _objTopLevelSemaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_blnIsDisposed)
                return;

            _blnIsDisposed = true;
            // Ensure the lock isn't held. If it is, wait for it to be released
            // before completing the dispose.
            await _objTopLevelSemaphore.WaitAsync();
            _objTopLevelSemaphore.Release();
            if (_blnTopLevelSemaphoreFromPool)
                Utils.SemaphorePool.Return(_objTopLevelSemaphore);
            else
                _objTopLevelSemaphore.Dispose();
        }

        private readonly struct SafeSemaphoreRelease : IAsyncDisposable, IDisposable
        {
            private readonly SemaphoreSlim _objCurrentSemaphore;
            private readonly SemaphoreSlim _objNextSemaphore;
            private readonly AsyncLock _objAsyncLock;

            public SafeSemaphoreRelease(SemaphoreSlim objCurrentSemaphore, SemaphoreSlim objNextSemaphore, AsyncLock objAsyncLock)
            {
                _objCurrentSemaphore = objCurrentSemaphore;
                _objNextSemaphore = objNextSemaphore;
                _objAsyncLock = objAsyncLock;
            }

            public ValueTask DisposeAsync()
            {
                if (_objNextSemaphore != _objAsyncLock._objCurrentSemaphore.Value)
                {
                    if (_objAsyncLock._objCurrentSemaphore.Value == _objCurrentSemaphore)
                        throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    if (_objAsyncLock._objCurrentSemaphore.Value == null)
                        throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                                                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                                                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore");
                }

                // Update _objAsyncLock._objCurrentSemaphore in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the
                // update will happen in a copy of the ExecutionContext and the caller
                // won't see the changes.
                _objAsyncLock._objCurrentSemaphore.Value = _objCurrentSemaphore == _objAsyncLock._objTopLevelSemaphore
                    ? null
                    : _objCurrentSemaphore;

                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                await _objNextSemaphore.WaitAsync();
                _objCurrentSemaphore.Release();
                _objNextSemaphore.Release();
                Utils.SemaphorePool.Return(_objNextSemaphore);
            }

            public void Dispose()
            {
                if (_objNextSemaphore != _objAsyncLock._objCurrentSemaphore.Value)
                {
                    if (_objAsyncLock._objCurrentSemaphore.Value == _objCurrentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    if (_objAsyncLock._objCurrentSemaphore.Value == null)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore");
                }

                _objAsyncLock._objCurrentSemaphore.Value
                    = _objCurrentSemaphore == _objAsyncLock._objTopLevelSemaphore ? null : _objCurrentSemaphore;
                if (Utils.EverDoEvents)
                {
                    while (!_objNextSemaphore.Wait(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe();
                }
                else
                    _objNextSemaphore.Wait();
                _objCurrentSemaphore.Release();
                _objNextSemaphore.Release();
                Utils.SemaphorePool.Return(_objNextSemaphore);
            }
        }
    }
}
