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
        // Needed because the pools are not necessarily initialized in static classes
        private readonly bool _blnReaderSemaphoreFromPool;
        // Because readers are always recursive and it's fine that way, we only need to deploy complicated stuff on the writer side
        private readonly SemaphoreSlim _objReaderSemaphore;
        // Write lock is set up as a generic AsyncLock that also fiddles with reader lock access
        private readonly AsyncLock _objWriterLock = new AsyncLock();
        private int _intCountActiveReaders;
        private bool _blnIsDisposed;

        public AsyncFriendlyReaderWriterLock()
        {
            if (Utils.SemaphorePool != null)
            {
                _blnReaderSemaphoreFromPool = true;
                _objReaderSemaphore = Utils.SemaphorePool.Get();
            }
            else
            {
                _blnReaderSemaphoreFromPool = false;
                _objReaderSemaphore = new SemaphoreSlim(1, 1);
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

            IDisposable objReturn = _objWriterLock.TakeLock();
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && !_objWriterLock.IsLockHeldRecursively)
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
        public async Task<IAsyncDisposable> EnterWriteLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            IAsyncDisposable objReturn = await _objWriterLock.TakeLockAsync();
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1 && !_objWriterLock.IsLockHeldRecursively)
                {
                    try
                    {
                        await _objReaderSemaphore.WaitAsync(token);
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
                await objReturn.DisposeAsync();
                throw;
            }
            return objReturn;
        }

        /// <summary>
        /// Synchronously release a lock held for writing.
        /// Use the SafeSemaphoreWriterRelease object gotten after obtaining a write lock as this method's argument.
        /// </summary>
        public void ExitWriteLock(IDisposable objRelease)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0 && !_objWriterLock.IsLockHeldRecursively)
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
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0 && !_objWriterLock.IsLockHeldRecursively)
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
            
            using (_objWriterLock.TakeLock())
            {
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
        public async Task EnterReadLockAsync(CancellationToken token = default)
        {
            if (_blnIsDisposed)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            IAsyncDisposable objRelease = await _objWriterLock.TakeLockAsync();
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        await _objReaderSemaphore.WaitAsync(token);
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
                await objRelease.DisposeAsync();
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
        public bool IsWriteLockHeld => !IsDisposed && _objWriterLock.IsLockHeld;

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
            _objWriterLock.Dispose();
            if (Utils.EverDoEvents)
            {
                while (!_objReaderSemaphore.Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe();
            }
            else
                _objReaderSemaphore.Wait();
            _objReaderSemaphore.Release();
            if (_blnReaderSemaphoreFromPool)
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
            await _objWriterLock.DisposeAsync();
            await _objReaderSemaphore.WaitAsync();
            _objReaderSemaphore.Release();
            if (_blnReaderSemaphoreFromPool)
                Utils.SemaphorePool.Return(_objReaderSemaphore);
            else
                _objReaderSemaphore.Dispose();
        }
    }
}
