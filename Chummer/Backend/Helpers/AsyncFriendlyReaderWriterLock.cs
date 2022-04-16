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
        private readonly AsyncLocal<Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>> _objCurrentWriterSemaphore = new AsyncLocal<Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>>();
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
            (DebuggableSemaphoreSlim objLastSemaphore, DebuggableSemaphoreSlim objCurrentSemaphore)
                = _objCurrentWriterSemaphore.Value
                  ?? new Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(null, _objTopLevelWriterSemaphore);
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = new Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(objCurrentSemaphore, objNextSemaphore);
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objLastSemaphore, objCurrentSemaphore, objNextSemaphore, this);
            try
            {
                objCurrentSemaphore.SafeWait(token);
            }
            catch
            {
                objRelease.DoRelease(false);
                throw;
            }
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                objRelease.DoRelease();
                throw;
            }
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1
                    // Wait for the reader lock only if there have been no other write locks before us
                    && (_objTopLevelWriterSemaphore.CurrentCount != 0 || objCurrentSemaphore == _objTopLevelWriterSemaphore))
                {
                    _objReaderSemaphore.SafeWait(token);
                }
            }
            catch
            {
                objRelease.Dispose();
                throw;
            }
            return objRelease;
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
            (DebuggableSemaphoreSlim objLastSemaphore, DebuggableSemaphoreSlim objCurrentSemaphore)
                = _objCurrentWriterSemaphore.Value
                  ?? new Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(null, _objTopLevelWriterSemaphore);
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = new Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(objCurrentSemaphore, objNextSemaphore);
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objLastSemaphore, objCurrentSemaphore, objNextSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentSemaphore, objNextSemaphore, objRelease, token);
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, DebuggableSemaphoreSlim objNextSemaphore, SafeWriterSemaphoreRelease objRelease, CancellationToken token)
        {
            try
            {
                await objCurrentSemaphore.WaitAsync(token).ConfigureAwait(false);
            }
            catch
            {
                // Don't bother with _objCurrentWriterSemaphore assignment because the shallow copy made when executing in this async
                // context gets discarded when the context is exited anyway.
                Utils.SemaphorePool.Return(ref objNextSemaphore);
                throw;
            }
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        // Wait for the reader lock only if there have been no other write locks before us
                        if (_objTopLevelWriterSemaphore.CurrentCount != 0 || objCurrentSemaphore == _objTopLevelWriterSemaphore)
                        {
                            await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                        }
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
                // Don't bother with _objCurrentWriterSemaphore assignment because the shallow copy made when executing in this async
                // context gets discarded when the context is exited anyway.
                try
                {
                    // ReSharper disable once MethodSupportsCancellation
                    await objNextSemaphore.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        objCurrentSemaphore.Release();
                    }
                    finally
                    {
                        objNextSemaphore.Release();
                    }
                }
                finally
                {
                    Utils.SemaphorePool.Return(ref objNextSemaphore);
                }
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
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0
                // Release the reader lock only if there have been no other write locks before us
                && (_objTopLevelWriterSemaphore.CurrentCount != 0 || objReleaseCast._objCurrentSemaphore == _objTopLevelWriterSemaphore))
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
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0
                // Release the reader lock only if there have been no other write locks before us
                && (_objTopLevelWriterSemaphore.CurrentCount != 0 || objReleaseCast._objCurrentSemaphore == _objTopLevelWriterSemaphore))
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
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (_objTopLevelWriterSemaphore.CurrentCount == 0)
            {
                // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
                DebuggableSemaphoreSlim objCurrentSemaphore = _objCurrentWriterSemaphore.Value?.Item2 ?? _objTopLevelWriterSemaphore;
                objCurrentSemaphore.SafeWait(token);
                try
                {
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
                    objCurrentSemaphore.Release();
                }
            }
            else if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
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

        /// <summary>
        /// Try to asynchronously obtain a lock for reading.
        /// </summary>
        public Task EnterReadLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed || _blnIsDisposing)
                return Task.FromException(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            token.ThrowIfCancellationRequested();
            if (_objTopLevelWriterSemaphore.CurrentCount != 0)
                return TakeReadLockCoreLightAsync(token);
            DebuggableSemaphoreSlim objCurrentSemaphore = _objCurrentWriterSemaphore.Value?.Item2 ?? _objTopLevelWriterSemaphore;
            return TakeReadLockCoreAsync(objCurrentSemaphore, token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task TakeReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, CancellationToken token = default)
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
                objCurrentSemaphore.Release();
            }
        }

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task TakeReadLockCoreLightAsync(CancellationToken token = default)
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
                _blnIsDisposed = true;
            }
            finally
            {
                _blnIsDisposing = false;
            }
        }

        private struct SafeWriterSemaphoreRelease : IAsyncDisposable, IDisposable
        {
            public readonly DebuggableSemaphoreSlim _objLastSemaphore;
            public readonly DebuggableSemaphoreSlim _objCurrentSemaphore;
            private DebuggableSemaphoreSlim _objNextSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeWriterSemaphoreRelease(DebuggableSemaphoreSlim objLastSemaphore, DebuggableSemaphoreSlim objCurrentSemaphore, DebuggableSemaphoreSlim objNextSemaphore, AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objCurrentSemaphore == null)
                    throw new ArgumentNullException(nameof(objCurrentSemaphore));
                if (objNextSemaphore == null)
                    throw new ArgumentNullException(nameof(objNextSemaphore));
                if (objLastSemaphore == objCurrentSemaphore)
                    throw new InvalidOperationException(
                        "Last and current semaphores are identical, this should not happen.");
                if (objCurrentSemaphore == objNextSemaphore)
                    throw new InvalidOperationException(
                        "Current and next semaphores are identical, this should not happen.");
                if (objLastSemaphore == objNextSemaphore)
                    throw new InvalidOperationException(
                        "Last and next semaphores are identical, this should not happen.");
                _objLastSemaphore = objLastSemaphore;
                _objCurrentSemaphore = objCurrentSemaphore;
                _objNextSemaphore = objNextSemaphore;
                _objReaderWriterLock = objReaderWriterLock;
            }

            public Task DoReleaseAsync(bool blnReleaseLock = true)
            {
                if (_objReaderWriterLock.IsDisposed)
                    return Task.FromException(new InvalidOperationException(
                                                  "Lock object was disposed before a writer lock release object assigned to it"));
                (DebuggableSemaphoreSlim objCurrentSemaphoreSlim, DebuggableSemaphoreSlim objNextSemaphoreSlim)
                    = _objReaderWriterLock._objCurrentWriterSemaphore.Value;
                if (_objNextSemaphore != objNextSemaphoreSlim)
                {
                    if (objNextSemaphoreSlim == _objCurrentSemaphore)
                        return Task.FromException(new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset."));
                    if (objNextSemaphoreSlim == null)
                        return Task.FromException(new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                                                      + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                                                      + "the lock inside a function and then leaving the function before exiting the lock can produce this situation."));
                    return Task.FromException(new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore"));
                }

                if (objCurrentSemaphoreSlim != _objCurrentSemaphore && (objCurrentSemaphoreSlim != null
                                                                        || _objCurrentSemaphore
                                                                        != _objReaderWriterLock
                                                                            ._objTopLevelWriterSemaphore))
                {
                    if (objCurrentSemaphoreSlim == null)
                        return Task.FromException(new InvalidOperationException("_objCurrentSemaphore was expected to be the previous semaphore. Instead, the previous semaphore is null.\n\n"
                                                      + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                                                      + "the lock inside a function and then leaving the function before exiting the lock can produce this situation."));
                    return Task.FromException(new InvalidOperationException("_objCurrentSemaphore was expected to be the previous semaphore"));
                }
                
                // Update _objReaderWriterLock._objCurrentSemaphore in the calling ExecutionContext
                // and defer any awaits to DoReleaseCoreAsync(). If this isn't done, the
                // update will happen in a copy of the ExecutionContext and the caller
                // won't see the changes.
                _objReaderWriterLock._objCurrentWriterSemaphore.Value = new Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(_objLastSemaphore, objCurrentSemaphoreSlim);

                return DoReleaseCoreAsync(blnReleaseLock);
            }

            private async Task DoReleaseCoreAsync(bool blnReleaseLock)
            {
                try
                {
                    if (blnReleaseLock)
                    {
                        await _objNextSemaphore.WaitAsync().ConfigureAwait(false);
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

            public void DoRelease(bool blnReleaseLock = true)
            {
                if (_objReaderWriterLock.IsDisposed)
                    throw new InvalidOperationException(
                        "Lock object was disposed before a writer lock release object assigned to it");
                (DebuggableSemaphoreSlim objCurrentSemaphoreSlim, DebuggableSemaphoreSlim objNextSemaphoreSlim)
                    = _objReaderWriterLock._objCurrentWriterSemaphore.Value;
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

                if (objCurrentSemaphoreSlim != _objCurrentSemaphore && (objCurrentSemaphoreSlim != null
                                                                        || _objCurrentSemaphore
                                                                        != _objReaderWriterLock
                                                                            ._objTopLevelWriterSemaphore))
                {
                    if (objCurrentSemaphoreSlim == null)
                        throw new InvalidOperationException("_objCurrentSemaphore was expected to be the previous semaphore. Instead, the previous semaphore is null.\n\n"
                                                           + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                                                           + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objCurrentSemaphore was expected to be the previous semaphore");
                }
                
                _objReaderWriterLock._objCurrentWriterSemaphore.Value = new Tuple<DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(_objLastSemaphore, objCurrentSemaphoreSlim);

                try
                {
                    if (blnReleaseLock)
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
