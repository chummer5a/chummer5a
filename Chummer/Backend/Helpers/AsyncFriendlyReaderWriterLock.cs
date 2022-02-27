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
    /// https://github.com/dotnet/wcf/blob/ce7428c2962e4ea4fce9c9c3e5999758a52bc4b9/src/System.Private.ServiceModel/src/Internals/System/Runtime/AsyncLock.cs
    /// https://itnext.io/reentrant-recursive-async-lock-is-impossible-in-c-e9593f4aa38a
    /// </summary>
    public sealed class AsyncFriendlyReaderWriterLock : IAsyncDisposable, IDisposable
    {
        // Because readers are always recursive and it's fine that way, we only need to deploy complicated stuff on the writer side
        private SemaphoreSlim _objReaderSemaphore = Utils.SemaphorePool.Get();
        // In order to properly allow writers to be recursive but still make them work properly as writer locks, we need to set up something
        // that is a bit like a singly-linked list. Each write lock creates a disposable SafeSemaphoreWriterRelease, and only disposing it
        // frees the write lock.
        private SemaphoreSlim _objTopLevelWriterSemaphore = Utils.SemaphorePool.Get();
        private readonly AsyncLocal<SemaphoreSlim> _objCurrentWriterSemaphore = new AsyncLocal<SemaphoreSlim>();
        private int _intCountActiveReaders;
        private bool _blnIsDisposed;

        /// <summary>
        /// Try to synchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public SafeSemaphoreWriterRelease EnterWriteLock(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            if (_objCurrentWriterSemaphore.Value == null)
                _objCurrentWriterSemaphore.Value = _objTopLevelWriterSemaphore;
            SemaphoreSlim objCurrentWriterSemaphore = _objCurrentWriterSemaphore.Value;

            if (Utils.EverDoEvents)
            {
                while (!objCurrentWriterSemaphore.Wait(Utils.DefaultSleepDuration, token))
                {
                    Utils.DoEventsSafe();
                }
                try
                {
                    if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && objCurrentWriterSemaphore == _objTopLevelWriterSemaphore)
                    {
                        try
                        {
                            while (!_objReaderSemaphore.Wait(Utils.DefaultSleepDuration, token))
                            {
                                Utils.DoEventsSafe();
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
                    _objCurrentWriterSemaphore.Value
                        = objCurrentWriterSemaphore == _objTopLevelWriterSemaphore
                            ? null
                            : objCurrentWriterSemaphore;
                    objCurrentWriterSemaphore.Release();
                    throw;
                }
            }
            else
            {
                objCurrentWriterSemaphore.Wait(token);
                try
                {
                    if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && objCurrentWriterSemaphore == _objTopLevelWriterSemaphore)
                    {
                        try
                        {
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
                    _objCurrentWriterSemaphore.Value
                        = objCurrentWriterSemaphore == _objTopLevelWriterSemaphore
                            ? null
                            : objCurrentWriterSemaphore;
                    objCurrentWriterSemaphore.Release();
                    throw;
                }
            }

            SemaphoreSlim objNextWriterSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = objNextWriterSemaphore;
            return new SafeSemaphoreWriterRelease(objCurrentWriterSemaphore, objNextWriterSemaphore, this);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public Task<SafeSemaphoreWriterRelease> EnterWriteLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (_objCurrentWriterSemaphore.Value == null)
                _objCurrentWriterSemaphore.Value = _objTopLevelWriterSemaphore;
            SemaphoreSlim objCurrentWriterSemaphore = _objCurrentWriterSemaphore.Value;
            SemaphoreSlim objNextWriterSemaphore = Utils.SemaphorePool.Get();
            _objCurrentWriterSemaphore.Value = objNextWriterSemaphore;
            SafeSemaphoreWriterRelease objWriterRelease = new SafeSemaphoreWriterRelease(objCurrentWriterSemaphore, objNextWriterSemaphore, this);
            return TakeWriterLockCoreAsync(objCurrentWriterSemaphore, objWriterRelease, token);
        }

        private async Task<SafeSemaphoreWriterRelease> TakeWriterLockCoreAsync(SemaphoreSlim objCurrentWriterSemaphore, SafeSemaphoreWriterRelease objWriterRelease, CancellationToken token = default)
        {
            try
            {
                await objCurrentWriterSemaphore.WaitAsync(token).ConfigureAwait(false);
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && objCurrentWriterSemaphore == _objTopLevelWriterSemaphore)
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
                if (objWriterRelease.IsMyLock(this))
                    await objWriterRelease.DisposeAsync();
                throw;
            }
            return objWriterRelease;
        }

        /// <summary>
        /// Synchronously release a lock held for writing.
        /// Use the SafeSemaphoreWriterRelease object gotten after obtaining a write lock as this method's argument.
        /// </summary>
        public void ExitWriteLock(SafeSemaphoreWriterRelease objRelease)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (!objRelease.IsMyLock(this))
                throw new ArgumentException("Attempting to exit a write lock that does not belong to us!",
                                            nameof(objRelease));
            objRelease.Dispose();
        }

        /// <summary>
        /// Asynchronously release a lock held for writing.
        /// Use the SafeSemaphoreWriterRelease object gotten after obtaining a write lock as this method's argument.
        /// </summary>
        public ValueTask ExitWriteLockAsync(SafeSemaphoreWriterRelease objRelease)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (!objRelease.IsMyLock(this))
                throw new ArgumentException("Attempting to exit a write lock that does not belong to us!",
                                            nameof(objRelease));
            return objRelease.DisposeAsync();
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading.
        /// </summary>
        public void EnterReadLock(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            if (_objCurrentWriterSemaphore.Value == null)
                _objCurrentWriterSemaphore.Value = _objTopLevelWriterSemaphore;

            SemaphoreSlim objCurrentWriterSemaphore = _objCurrentWriterSemaphore.Value;
            try
            {
                if (Utils.EverDoEvents)
                {
                    while (!objCurrentWriterSemaphore.Wait(Utils.DefaultSleepDuration, token))
                    {
                        Utils.DoEventsSafe();
                    }

                    try
                    {
                        if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                        {
                            try
                            {
                                while (!_objReaderSemaphore.Wait(Utils.DefaultSleepDuration, token))
                                {
                                    Utils.DoEventsSafe();
                                }
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
                        objCurrentWriterSemaphore.Release();
                    }
                }
                else
                {
                    objCurrentWriterSemaphore.Wait(token);
                    try
                    {
                        if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                        {
                            try
                            {
                                _objReaderSemaphore.Wait(token);
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
                        objCurrentWriterSemaphore.Release();
                    }
                }
            }
            finally
            {
                _objCurrentWriterSemaphore.Value
                    = objCurrentWriterSemaphore == _objTopLevelWriterSemaphore
                        ? null
                        : objCurrentWriterSemaphore;
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading.
        /// </summary>
        public async Task EnterReadLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (_objCurrentWriterSemaphore.Value == null)
                _objCurrentWriterSemaphore.Value = _objTopLevelWriterSemaphore;

            SemaphoreSlim objCurrentWriterSemaphore = _objCurrentWriterSemaphore.Value;
            try
            {
                await objCurrentWriterSemaphore.WaitAsync(token).ConfigureAwait(false);
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
                    objCurrentWriterSemaphore.Release();
                }
            }
            finally
            {
                _objCurrentWriterSemaphore.Value
                    = objCurrentWriterSemaphore == _objTopLevelWriterSemaphore
                        ? null
                        : objCurrentWriterSemaphore;
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
        public bool IsReadLockHeld => !IsDisposed && (_intCountActiveReaders > 0 || _objReaderSemaphore.CurrentCount == 0);

        /// <summary>
        /// Is there anything holding the write lock?
        /// </summary>
        public bool IsWriteLockHeld => !IsDisposed && _objTopLevelWriterSemaphore.CurrentCount == 0;

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
            _objTopLevelWriterSemaphore.Wait();
            _objTopLevelWriterSemaphore.Release();
            Utils.SemaphorePool.Return(_objTopLevelWriterSemaphore);
            Utils.SemaphorePool.Return(_objReaderSemaphore);
            _objTopLevelWriterSemaphore = null;
            _objReaderSemaphore = null;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_blnIsDisposed)
                return;

            _blnIsDisposed = true;
            // Ensure the locks aren't held. If they are, wait for them to be released
            // before completing the dispose.
            await _objTopLevelWriterSemaphore.WaitAsync();
            _objTopLevelWriterSemaphore.Release();
            Utils.SemaphorePool.Return(_objTopLevelWriterSemaphore);
            Utils.SemaphorePool.Return(_objReaderSemaphore);
            _objTopLevelWriterSemaphore = null;
            _objReaderSemaphore = null;
        }

        /// <summary>
        /// This class is used to ensure proper tracking and releasing of recursive write locks regardless of which threads start or resume the
        /// tasks handled by async/await operations. An instance is created whenever a write lock is obtained and the same instance should be
        /// disposed to release the aforementioned write lock.
        /// </summary>
        public readonly struct SafeSemaphoreWriterRelease : IAsyncDisposable, IDisposable
        {
            private readonly SemaphoreSlim _objCurrentWriterSemaphore;
            private readonly SemaphoreSlim _objNextWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objMyLock;

            public SafeSemaphoreWriterRelease(SemaphoreSlim objCurrentWriterSemaphore, SemaphoreSlim objNextWriterSemaphore, AsyncFriendlyReaderWriterLock objMyLock)
            {
                _objCurrentWriterSemaphore = objCurrentWriterSemaphore;
                _objNextWriterSemaphore = objNextWriterSemaphore;
                _objMyLock = objMyLock;
            }

            public bool IsMyLock(AsyncFriendlyReaderWriterLock objMyLock)
            {
                return _objMyLock == objMyLock;
            }

            public ValueTask DisposeAsync()
            {
                if (_objNextWriterSemaphore != _objMyLock._objCurrentWriterSemaphore.Value)
                    throw new InvalidOperationException("_objNextWriterSemaphore was expected to by the current writer semaphore");
                // Update _objMyLock._objCurrentWriterSemaphore in the calling ExecutionContext and defer
                // any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a copy
                // of the ExecutionContext and the caller won't see the changes.
                _objMyLock._objCurrentWriterSemaphore.Value
                    = _objCurrentWriterSemaphore == _objMyLock._objTopLevelWriterSemaphore
                        ? null
                        : _objCurrentWriterSemaphore;
                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                await _objNextWriterSemaphore.WaitAsync();
                _objCurrentWriterSemaphore.Release();
                _objNextWriterSemaphore.Release();
                Utils.SemaphorePool.Return(_objNextWriterSemaphore);
                if (Interlocked.Decrement(ref _objMyLock._intCountActiveReaders) == 0
                    && _objCurrentWriterSemaphore == _objMyLock._objTopLevelWriterSemaphore)
                    _objMyLock._objReaderSemaphore.Release();
            }

            public void Dispose()
            {
                if (_objNextWriterSemaphore != _objMyLock._objCurrentWriterSemaphore.Value)
                    throw new InvalidOperationException("_objNextWriterSemaphore was expected to by the current writer semaphore");
                _objMyLock._objCurrentWriterSemaphore.Value
                    = _objCurrentWriterSemaphore == _objMyLock._objTopLevelWriterSemaphore
                        ? null
                        : _objCurrentWriterSemaphore;
                _objNextWriterSemaphore.Wait();
                _objCurrentWriterSemaphore.Release();
                _objNextWriterSemaphore.Release();
                Utils.SemaphorePool.Return(_objNextWriterSemaphore);
                if (Interlocked.Decrement(ref _objMyLock._intCountActiveReaders) == 0
                    && _objCurrentWriterSemaphore == _objMyLock._objTopLevelWriterSemaphore)
                    _objMyLock._objReaderSemaphore.Release();
            }
        }
    }
}
