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
    /// Async/await-friendly version of ReaderWriterLockSlim that works off of SemaphoreSlim instead.
    /// ReaderWriterLockSlim's locks have thread affinity and so have problems when a lock is entered on one thread, an async/await Task engine is created and executed, and then
    /// the code resumes on a different thread (which can happen with async/await) and tries releasing the lock.
    /// Internals and method heavily inspired by the code in the pull request mentioned in this StackOverflow comment:
    /// https://stackoverflow.com/questions/19659387/readerwriterlockslim-and-async-await#comment120825654_64757462
    /// Features allowing the lock to be recursive are taken from here (because otherwise, doing recursive locks that work with async/await is impossible, see the second link):
    /// https://github.com/dotnet/wcf/blob/main/src/System.Private.ServiceModel/src/Internals/System/Runtime/AsyncLock.cs
    /// https://itnext.io/reentrant-recursive-async-lock-is-impossible-in-c-e9593f4aa38a
    /// </summary>
    public sealed class AsyncFriendlyReaderWriterLock : IAsyncDisposable, IDisposable
    {
        // Needed because the pools are not necessarily initialized in static classes
        private readonly bool _blnSemaphoresFromPool;
        // Because readers are always recursive and it's fine that way, we only need to deploy complicated stuff on the writer side
        private readonly SemaphoreSlim _objReaderSemaphore;
        // In order to properly allow async lock to be recursive but still make them work properly as locks, we need to set up something
        // that is a bit like a singly-linked list. Each lock creates a disposable SafeWriterSemaphoreRelease, and only disposing it frees the lock.
        private readonly AsyncLocal<SemaphoreSlim> _objCurrentWriterSemaphore = new AsyncLocal<SemaphoreSlim>();
        private readonly SemaphoreSlim _objTopLevelWriterSemaphore;
        private int _intCountActiveReaders;
        private bool _blnIsDisposed;

        public AsyncFriendlyReaderWriterLock()
        {
            if (Utils.SemaphorePool != null)
            {
                _blnSemaphoresFromPool = true;
                _objReaderSemaphore = Utils.SemaphorePool.Get();
                _objTopLevelWriterSemaphore = Utils.SemaphorePool.Get();
            }
            else
            {
                _blnSemaphoresFromPool = false;
                _objReaderSemaphore = new SemaphoreSlim(1, 1);
                _objTopLevelWriterSemaphore = new SemaphoreSlim(1, 1);
            }
        }

        /// <summary>
        /// Try to synchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public IDisposable EnterWriteLock(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            SemaphoreSlim objCurrentSemaphore = _objCurrentWriterSemaphore.Value ?? _objTopLevelWriterSemaphore;
            SemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = objNextSemaphore;
            IDisposable objReturn = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            try
            {
                if (Utils.EverDoEvents)
                {
                    while (!objCurrentSemaphore.Wait(Utils.DefaultSleepDuration, token))
                        Utils.DoEventsSafe();
                }
                else
                    objCurrentSemaphore.Wait(token);
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && !IsWriteLockHeldRecursively)
                {
                    try
                    {
                        if (Utils.EverDoEvents)
                        {
                            while (!_objReaderSemaphore.Wait(Utils.DefaultSleepDuration, token))
                                Utils.DoEventsSafe();
                        }
                        else
                            _objReaderSemaphore.Wait(token);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intCountActiveReaders);
                        throw;
                    }
                }
            }
            catch
            {
                objReturn.Dispose();
                throw;
            }
            return objReturn;
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public Task<IAsyncDisposable> EnterWriteLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            SemaphoreSlim objCurrentSemaphore = _objCurrentWriterSemaphore.Value ?? _objTopLevelWriterSemaphore;
            SemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = objNextSemaphore;
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentSemaphore, objRelease, token);
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(SemaphoreSlim objCurrentSemaphore, SafeWriterSemaphoreRelease objRelease, CancellationToken token)
        {
            try
            {
                await objCurrentSemaphore.WaitAsync(token).ConfigureAwait(false);
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && !IsWriteLockHeldRecursively)
                {
                    try
                    {
                        await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intCountActiveReaders);
                        throw;
                    }
                }
            }
            catch
            {
                await objRelease.DisposeAsync().ConfigureAwait(false);
                throw;
            }
            return objRelease;
        }

        /// <summary>
        /// Synchronously release a lock held for writing.
        /// Use the SafeSemaphoreWriterRelease object gotten after obtaining a write lock as this method's argument.
        /// </summary>
        public void ExitWriteLock(IDisposable objRelease)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0 && !IsWriteLockHeldRecursively)
                _objReaderSemaphore.Release();
            objRelease.Dispose();
        }

        /// <summary>
        /// Asynchronously release a lock held for writing.
        /// Use the SafeSemaphoreWriterRelease object gotten after obtaining a write lock as this method's argument.
        /// </summary>
        public ValueTask ExitWriteLockAsync(IAsyncDisposable objRelease)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0 && !IsWriteLockHeldRecursively)
                _objReaderSemaphore.Release();
            return objRelease.DisposeAsync();
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading.
        /// </summary>
        public void EnterReadLock(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            SemaphoreSlim objCurrentSemaphore = _objCurrentWriterSemaphore.Value ?? _objTopLevelWriterSemaphore;
            SemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = objNextSemaphore;
            using (new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this))
            {
                if (Utils.EverDoEvents)
                {
                    while (!objCurrentSemaphore.Wait(Utils.DefaultSleepDuration, token))
                        Utils.DoEventsSafe();
                }
                else
                    objCurrentSemaphore.Wait(token);
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        if (Utils.EverDoEvents)
                        {
                            while (!_objReaderSemaphore.Wait(Utils.DefaultSleepDuration, token))
                                Utils.DoEventsSafe();
                        }
                        else
                            _objReaderSemaphore.Wait(token);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intCountActiveReaders);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading.
        /// </summary>
        public Task EnterReadLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                return Task.FromException(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            SemaphoreSlim objCurrentSemaphore = _objCurrentWriterSemaphore.Value ?? _objTopLevelWriterSemaphore;
            SemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = objNextSemaphore;
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeReadLockCoreAsync(objCurrentSemaphore, objRelease, token);
        }

        private async Task TakeReadLockCoreAsync(SemaphoreSlim objCurrentSemaphore, SafeWriterSemaphoreRelease objRelease, CancellationToken token = default)
        {
            await objCurrentSemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intCountActiveReaders);
                        throw;
                    }
                }
            }
            finally
            {
                await objRelease.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Release a lock held for reading.
        /// </summary>
        public void ExitReadLock()
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0)
            {
                _objReaderSemaphore.Release();
            }
        }

        /// <summary>
        /// Is there anything holding the read lock? Note that write locks will also cause this to return true.
        /// </summary>
        public bool IsReadLockHeld => !IsDisposed && _intCountActiveReaders > 0;

        /// <summary>
        /// Is there anything holding the write lock?
        /// </summary>
        public bool IsWriteLockHeld => !IsDisposed && _objTopLevelWriterSemaphore.CurrentCount == 0;

        /// <summary>
        /// Are there any async locks held below this one?
        /// </summary>
        public bool IsWriteLockHeldRecursively => IsWriteLockHeld && _objCurrentWriterSemaphore.Value != null && _objCurrentWriterSemaphore.Value.CurrentCount == 0;

        /// <summary>
        /// Is the locker object already disposed and its allocatable semaphores returned to the semaphore pool?
        /// </summary>
        public bool IsDisposed => _blnIsDisposed;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_blnIsDisposed)
                return;

            _blnIsDisposed = true;
            // Ensure the locks aren't held. If they are, wait for them to be released
            // before completing the dispose.
            if (Utils.EverDoEvents)
            {
                while (!_objTopLevelWriterSemaphore.Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe();
            }
            else
                _objTopLevelWriterSemaphore.Wait();
            _objTopLevelWriterSemaphore.Release();
            if (_blnSemaphoresFromPool)
                Utils.SemaphorePool.Return(_objTopLevelWriterSemaphore);
            else
                _objTopLevelWriterSemaphore.Dispose();
            if (Utils.EverDoEvents)
            {
                while (!_objReaderSemaphore.Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe();
            }
            else
                _objReaderSemaphore.Wait();
            _objReaderSemaphore.Release();
            if (_blnSemaphoresFromPool)
                Utils.SemaphorePool.Return(_objReaderSemaphore);
            else
                _objReaderSemaphore.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_blnIsDisposed)
                return;

            _blnIsDisposed = true;
            
            // Ensure the locks aren't held. If they are, wait for them to be released
            // before completing the dispose.
            await _objTopLevelWriterSemaphore.WaitAsync().ConfigureAwait(false);
            ConfiguredTaskAwaitable tskReaderLock = _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
            _objTopLevelWriterSemaphore.Release();
            if (_blnSemaphoresFromPool)
                Utils.SemaphorePool.Return(_objTopLevelWriterSemaphore);
            else
                _objTopLevelWriterSemaphore.Dispose();
            await tskReaderLock;
            _objReaderSemaphore.Release();
            if (_blnSemaphoresFromPool)
                Utils.SemaphorePool.Return(_objReaderSemaphore);
            else
                _objReaderSemaphore.Dispose();
        }

        private readonly struct SafeWriterSemaphoreRelease : IAsyncDisposable, IDisposable
        {
            private readonly SemaphoreSlim _objCurrentWriterSemaphore;
            private readonly SemaphoreSlim _objNextWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeWriterSemaphoreRelease(SemaphoreSlim objCurrentWriterSemaphore, SemaphoreSlim objNextWriterSemaphore, AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                _objCurrentWriterSemaphore = objCurrentWriterSemaphore;
                _objNextWriterSemaphore = objNextWriterSemaphore;
                _objReaderWriterLock = objReaderWriterLock;
            }

            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock.IsDisposed)
                    throw new InvalidOperationException(
                        "Lock object was disposed before a writer lock release object assigned to it");
                if (_objNextWriterSemaphore != _objReaderWriterLock._objCurrentWriterSemaphore.Value)
                {
                    if (_objReaderWriterLock._objCurrentWriterSemaphore.Value == _objCurrentWriterSemaphore)
                        throw new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    if (_objReaderWriterLock._objCurrentWriterSemaphore.Value == null)
                        throw new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                                                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                                                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore");
                }

                // Update _objReaderWriterLock._objCurrentWriterSemaphore in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the
                // update will happen in a copy of the ExecutionContext and the caller
                // won't see the changes.
                _objReaderWriterLock._objCurrentWriterSemaphore.Value = _objCurrentWriterSemaphore
                                                                        == _objReaderWriterLock
                                                                            ._objTopLevelWriterSemaphore
                    ? null
                    : _objCurrentWriterSemaphore;

                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                await _objNextWriterSemaphore.WaitAsync().ConfigureAwait(false);
                _objCurrentWriterSemaphore.Release();
                _objNextWriterSemaphore.Release();
                Utils.SemaphorePool.Return(_objNextWriterSemaphore);
            }

            public void Dispose()
            {
                if (_objReaderWriterLock.IsDisposed)
                    throw new InvalidOperationException(
                        "Lock object was disposed before a writer lock release object assigned to it");
                if (_objNextWriterSemaphore != _objReaderWriterLock._objCurrentWriterSemaphore.Value)
                {
                    if (_objReaderWriterLock._objCurrentWriterSemaphore.Value == _objCurrentWriterSemaphore)
                        throw new InvalidOperationException(
                            "_objNextWriterSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    if (_objReaderWriterLock._objCurrentWriterSemaphore.Value == null)
                        throw new InvalidOperationException(
                            "_objNextWriterSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore");
                }

                _objReaderWriterLock._objCurrentWriterSemaphore.Value
                    = _objCurrentWriterSemaphore == _objReaderWriterLock._objTopLevelWriterSemaphore
                        ? null
                        : _objCurrentWriterSemaphore;
                if (Utils.EverDoEvents)
                {
                    while (!_objNextWriterSemaphore.Wait(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe();
                }
                else
                    _objNextWriterSemaphore.Wait();
                _objCurrentWriterSemaphore.Release();
                _objNextWriterSemaphore.Release();
                Utils.SemaphorePool.Return(_objNextWriterSemaphore);
            }
        }
    }
}
