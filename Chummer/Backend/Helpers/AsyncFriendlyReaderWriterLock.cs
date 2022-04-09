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
        // Because readers are always recursive and it's fine that way, we only need to deploy complicated stuff on the writer side
        private readonly DebuggableSemaphoreSlim _objReaderSemaphore = new DebuggableSemaphoreSlim();
        // In order to properly allow async lock to be recursive but still make them work properly as locks, we need to set up something
        // that is a bit like a singly-linked list. Each lock creates a disposable SafeWriterSemaphoreRelease, and only disposing it frees the lock.
        private readonly AsyncLocal<DebuggableSemaphoreSlim> _objCurrentWriterSemaphore = new AsyncLocal<DebuggableSemaphoreSlim>();
        private readonly ReaderWriterLockSlim _objAsyncLocalWriterReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly DebuggableSemaphoreSlim _objTopLevelWriterSemaphore = new DebuggableSemaphoreSlim();
        private int _intCountActiveReaders;
        private bool _blnIsDisposed;
        private bool _blnIsDisposing;

        /// <summary>
        /// Try to synchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public IDisposable EnterWriteLock(CancellationToken token = default)
        {
            if (_blnIsDisposed || _blnIsDisposing)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            token.ThrowIfCancellationRequested();
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore;
            try
            {
                if (Utils.EverDoEvents)
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                        Utils.DoEventsSafe();
                    }
                }
                else
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch
            {
                Utils.SemaphorePool.Return(objNextSemaphore);
                throw;
            }
            try
            {
                objCurrentSemaphore = _objCurrentWriterSemaphore.Value;
                _objCurrentWriterSemaphore.Value = objNextSemaphore;
            }
            finally
            {
                _objAsyncLocalWriterReaderWriterLockSlim.ExitWriteLock();
            }
            if (objCurrentSemaphore == null)
                objCurrentSemaphore = _objTopLevelWriterSemaphore;
            SafeWriterSemaphoreRelease objReturn = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            try
            {
                objCurrentSemaphore.SafeWait(token);
            }
            catch
            {
                objReturn.DoRelease(false);
                throw;
            }
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                objReturn.DoRelease();
                throw;
            }
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && !IsWriteLockHeldRecursively)
                {
                    _objReaderSemaphore.SafeWait(token);
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
            if (_blnIsDisposed || _blnIsDisposing)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            token.ThrowIfCancellationRequested();
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore;
            try
            {
                if (Utils.EverDoEvents)
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                        Utils.DoEventsSafe();
                    }
                }
                else
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.SemaphorePool.Return(objNextSemaphore);
                return Task.FromException<IAsyncDisposable>(ex);
            }
            try
            {
                objCurrentSemaphore = _objCurrentWriterSemaphore.Value;
                _objCurrentWriterSemaphore.Value = objNextSemaphore;
            }
            finally
            {
                _objAsyncLocalWriterReaderWriterLockSlim.ExitWriteLock();
            }
            if (objCurrentSemaphore == null)
                objCurrentSemaphore = _objTopLevelWriterSemaphore;
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentSemaphore, objRelease, token);
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, SafeWriterSemaphoreRelease objRelease, CancellationToken token)
        {
            try
            {
                await objCurrentSemaphore.WaitAsync(token).ConfigureAwait(false);
            }
            catch
            {
                await objRelease.DoReleaseAsync(false);
                throw;
            }
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                await objRelease.DoReleaseAsync();
                throw;
            }
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && !IsWriteLockHeldRecursively)
                {
                    await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false); // Decrement on error already happens in Dispose
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
            if (!(objRelease is SafeWriterSemaphoreRelease objReleaseCast))
                throw new ArgumentException("Argument is not a " + nameof(SafeWriterSemaphoreRelease),
                                            nameof(objRelease));
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0 && !IsWriteLockHeldRecursively)
                _objReaderSemaphore.Release();
            objReleaseCast.DoRelease();
        }

        /// <summary>
        /// Asynchronously release a lock held for writing.
        /// Use the SafeSemaphoreWriterRelease object gotten after obtaining a write lock as this method's argument.
        /// </summary>
        public Task ExitWriteLockAsync(IAsyncDisposable objRelease)
        {
            if (_blnIsDisposed)
                return Task.FromException(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            if (!(objRelease is SafeWriterSemaphoreRelease objReleaseCast))
                return Task.FromException(new ArgumentException(
                                              "Argument is not a " + nameof(SafeWriterSemaphoreRelease),
                                              nameof(objRelease)));
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0 && !IsWriteLockHeldRecursively)
                _objReaderSemaphore.Release();
            return objReleaseCast.DoReleaseAsync();
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading.
        /// </summary>
        public void EnterReadLock(CancellationToken token = default)
        {
            if (_blnIsDisposed || _blnIsDisposing)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            token.ThrowIfCancellationRequested();
            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
            bool blnDoWriterLock = false;
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore;
            try
            {
                if (Utils.EverDoEvents)
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                        Utils.DoEventsSafe();
                    }
                }
                else
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch
            {
                Utils.SemaphorePool.Return(objNextSemaphore);
                throw;
            }
            try
            {
                objCurrentSemaphore = _objCurrentWriterSemaphore.Value;
                _objCurrentWriterSemaphore.Value = objNextSemaphore;
            }
            finally
            {
                _objAsyncLocalWriterReaderWriterLockSlim.ExitWriteLock();
            }
            if (objCurrentSemaphore == null)
                objCurrentSemaphore = _objTopLevelWriterSemaphore;
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            try
            {
                blnDoWriterLock = _objTopLevelWriterSemaphore.CurrentCount == 0;
                if (blnDoWriterLock)
                {
                    blnDoWriterLock = false; // Makes sure we don't try to release semaphore if we cancel out of acquiring its lock
                    objCurrentSemaphore.SafeWait(token);
                    blnDoWriterLock = true;
                }

                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        _objReaderSemaphore.SafeWait(token);
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
                // Deliberately DoRelease to not decrement the active reader count
                objRelease.DoRelease(blnDoWriterLock);
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading.
        /// </summary>
        public Task EnterReadLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed || _blnIsDisposing)
                return Task.FromException(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            token.ThrowIfCancellationRequested();
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            DebuggableSemaphoreSlim objCurrentSemaphore;
            try
            {
                if (Utils.EverDoEvents)
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                        Utils.DoEventsSafe();
                    }
                }
                else
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.SemaphorePool.Return(objNextSemaphore);
                return Task.FromException(ex);
            }
            try
            {
                objCurrentSemaphore = _objCurrentWriterSemaphore.Value;
                _objCurrentWriterSemaphore.Value = objNextSemaphore;
            }
            finally
            {
                _objAsyncLocalWriterReaderWriterLockSlim.ExitWriteLock();
            }
            if (objCurrentSemaphore == null)
                objCurrentSemaphore = _objTopLevelWriterSemaphore;
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objCurrentSemaphore, objNextSemaphore, this);
            return TakeReadLockCoreAsync(objCurrentSemaphore, objRelease, token);
        }

        private async Task TakeReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, SafeWriterSemaphoreRelease objRelease, CancellationToken token = default)
        {
            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
            bool blnDoWriterLock = _objTopLevelWriterSemaphore.CurrentCount == 0;
            if (blnDoWriterLock)
            {
                try
                {
                    await objCurrentSemaphore.WaitAsync(token).ConfigureAwait(false);
                }
                catch
                {
                    await objRelease.DoReleaseAsync(false).ConfigureAwait(false);
                    throw;
                }
            }
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
                // Deliberately DoRelease to not decrement the active reader count
                await objRelease.DoReleaseAsync(blnDoWriterLock).ConfigureAwait(false);
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
        public bool IsWriteLockHeld => !IsDisposed && _objTopLevelWriterSemaphore != null && _objTopLevelWriterSemaphore.CurrentCount == 0;

        /// <summary>
        /// Are there any async locks held below this one?
        /// </summary>
        public bool IsWriteLockHeldRecursively
        {
            get
            {
                if (!IsWriteLockHeld)
                    return false;
                DebuggableSemaphoreSlim objSemaphore;
                if (Utils.EverDoEvents)
                {
                    while (!_objAsyncLocalWriterReaderWriterLockSlim.TryEnterReadLock(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe();
                }
                else
                    _objAsyncLocalWriterReaderWriterLockSlim.EnterReadLock();
                try
                {
                    objSemaphore = _objCurrentWriterSemaphore.Value;
                }
                finally
                {
                    _objAsyncLocalWriterReaderWriterLockSlim.ExitReadLock();
                }
                return objSemaphore != null && objSemaphore.CurrentCount == 0;
            }
        }

        /// <summary>
        /// Is the locker object already disposed and its allocatable semaphores returned to the semaphore pool?
        /// </summary>
        public bool IsDisposed => _blnIsDisposed;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_blnIsDisposed || _blnIsDisposing)
                return;

            _blnIsDisposing = true;
            try
            {
                // Ensure the locks aren't held. If they are, wait for them to be released
                // before completing the dispose.
                _objTopLevelWriterSemaphore.SafeWait();
                _objReaderSemaphore.SafeWait();
                _objReaderSemaphore.Release();
                _objReaderSemaphore.Dispose();
                _objTopLevelWriterSemaphore.Release();
                _objTopLevelWriterSemaphore.Dispose();
                _objAsyncLocalWriterReaderWriterLockSlim.Dispose();
                _blnIsDisposed = true;
            }
            finally
            {
                _blnIsDisposing = false;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_blnIsDisposed || _blnIsDisposing)
                return;

            _blnIsDisposing = true;
            try
            {
                // Ensure the locks aren't held. If they are, wait for them to be released
                // before completing the dispose.
                await _objTopLevelWriterSemaphore.WaitAsync().ConfigureAwait(false);
                await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                _objReaderSemaphore.Release();
                _objReaderSemaphore.Dispose();
                _objTopLevelWriterSemaphore.Release();
                _objTopLevelWriterSemaphore.Dispose();
                _objAsyncLocalWriterReaderWriterLockSlim.Dispose();
                _blnIsDisposed = true;
            }
            finally
            {
                _blnIsDisposing = false;
            }
        }

        private readonly struct SafeWriterSemaphoreRelease : IAsyncDisposable, IDisposable
        {
            private readonly DebuggableSemaphoreSlim _objCurrentWriterSemaphore;
            private readonly DebuggableSemaphoreSlim _objNextWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeWriterSemaphoreRelease(DebuggableSemaphoreSlim objCurrentWriterSemaphore, DebuggableSemaphoreSlim objNextWriterSemaphore, AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                _objCurrentWriterSemaphore = objCurrentWriterSemaphore;
                _objNextWriterSemaphore = objNextWriterSemaphore;
                _objReaderWriterLock = objReaderWriterLock;
            }

            public Task DoReleaseAsync(bool blnReleaseLock = true)
            {
                if (_objReaderWriterLock.IsDisposed)
                    return Task.FromException(new InvalidOperationException(
                                                  "Lock object was disposed before a writer lock release object assigned to it"));
                if (Utils.EverDoEvents)
                {
                    while (!_objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.TryEnterUpgradeableReadLock(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe();
                }
                else
                    _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (_objNextWriterSemaphore != _objReaderWriterLock._objCurrentWriterSemaphore.Value)
                    {
                        if (_objReaderWriterLock._objCurrentWriterSemaphore.Value == _objCurrentWriterSemaphore)
                            return Task.FromException(new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset."));
                        if (_objReaderWriterLock._objCurrentWriterSemaphore.Value == null)
                            return Task.FromException(new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                                                          + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                                                          + "the lock inside a function and then leaving the function before exiting the lock can produce this situation."));
                        return Task.FromException(new InvalidOperationException("_objNextWriterSemaphore was expected to be the current semaphore"));
                    }

                    // Update _objReaderWriterLock._objCurrentWriterSemaphore in the calling ExecutionContext
                    // and defer any awaits to DoReleaseCoreAsync(). If this isn't done, the
                    // update will happen in a copy of the ExecutionContext and the caller
                    // won't see the changes.
                    if (Utils.EverDoEvents)
                    {
                        while (!_objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                            Utils.DoEventsSafe();
                    }
                    else
                        _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.EnterWriteLock();
                    try
                    {
                        _objReaderWriterLock._objCurrentWriterSemaphore.Value = _objCurrentWriterSemaphore
                            == _objReaderWriterLock
                                ._objTopLevelWriterSemaphore
                                ? null
                                : _objCurrentWriterSemaphore;
                    }
                    finally
                    {
                        _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.ExitWriteLock();
                    }
                }
                finally
                {
                    _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.ExitUpgradeableReadLock();
                }

                return DoReleaseCoreAsync(blnReleaseLock);
            }

            private async Task DoReleaseCoreAsync(bool blnReleaseLock = true)
            {
                try
                {
                    if (blnReleaseLock)
                    {
                        await _objNextWriterSemaphore.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            _objCurrentWriterSemaphore.Release();
                        }
                        finally
                        {
                            _objNextWriterSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    Utils.SemaphorePool.Return(_objNextWriterSemaphore);
                }
            }

            public void DoRelease(bool blnReleaseLock = true)
            {
                if (_objReaderWriterLock.IsDisposed)
                    throw new InvalidOperationException(
                        "Lock object was disposed before a writer lock release object assigned to it");
                if (Utils.EverDoEvents)
                {
                    while (!_objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.TryEnterUpgradeableReadLock(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe();
                }
                else
                    _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
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

                    if (Utils.EverDoEvents)
                    {
                        while (!_objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.TryEnterWriteLock(Utils.DefaultSleepDuration))
                            Utils.DoEventsSafe();
                    }
                    else
                        _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.EnterWriteLock();
                    try
                    {
                        _objReaderWriterLock._objCurrentWriterSemaphore.Value
                            = _objCurrentWriterSemaphore == _objReaderWriterLock._objTopLevelWriterSemaphore
                                ? null
                                : _objCurrentWriterSemaphore;
                    }
                    finally
                    {
                        _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.ExitWriteLock();
                    }
                }
                finally
                {
                    _objReaderWriterLock._objAsyncLocalWriterReaderWriterLockSlim.ExitUpgradeableReadLock();
                }

                try
                {
                    if (blnReleaseLock)
                    {
                        _objNextWriterSemaphore.SafeWait();
                        try
                        {
                            _objCurrentWriterSemaphore.Release();
                        }
                        finally
                        {
                            _objNextWriterSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    Utils.SemaphorePool.Return(_objNextWriterSemaphore);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _objReaderWriterLock.ExitWriteLock(this);
            }

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                return new ValueTask(_objReaderWriterLock.ExitWriteLockAsync(this));
            }
        }
    }
}
