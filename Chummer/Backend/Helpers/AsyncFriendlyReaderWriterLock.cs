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

// Uncomment this define to control whether or not write locks check if they have been called from within a non-upgradeable read lock.
#if DEBUG
//#define UPGRADEABLEREADLOCKCHECK
#endif

using System;

#if DEBUG

using System.Diagnostics;

#endif

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
        // Because .NET Framework doesn't have dictionary optimizations for dealing with multiple AsyncLocals stored per context, we need scrape together something similar.
        // Therefore, we store a nested tuple where the first element is the number of active local readers and the second element is the tuple containing our writer lock semaphores
        // TODO: Revert this cursed bodge once we migrate to a version of .NET that has these AsyncLocal optimizations
        private readonly AsyncLocal<Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>> _objAsyncLocalCurrentsContainer = new AsyncLocal<Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>>();

        private readonly DebuggableSemaphoreSlim _objTopLevelWriterSemaphore = new DebuggableSemaphoreSlim();

#if UPGRADEABLEREADLOCKCHECK
        private readonly AsyncLocal<bool> _objInNonUpgradeableReadLock = new AsyncLocal<bool>();
#endif

        private int _intCountActiveReaders;
        private int _intDisposedStatus;

        /// <summary>
        /// Try to synchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public IDisposable EnterWriteLock(CancellationToken token = default)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));

            token.ThrowIfCancellationRequested();
#if UPGRADEABLEREADLOCKCHECK
            // We are trying to acquire a write lock from inside a non-upgradeable read lock. This is bad! Make sure this doesn't happen!
            if (_objReaderSemaphore.CurrentCount == 0 && _objInNonUpgradeableReadLock.Value)
                Utils.BreakIfDebug();
#endif
            int intCountLocalReaders = 0;
            DebuggableSemaphoreSlim objLastSemaphore = null;
            DebuggableSemaphoreSlim objCurrentSemaphore = _objTopLevelWriterSemaphore;
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objLastSemaphore, objCurrentSemaphore) = objAsyncLocals;
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentSemaphore || objNextSemaphore == objLastSemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                    intCountLocalReaders, objCurrentSemaphore, objNextSemaphore);
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objLastSemaphore, objCurrentSemaphore, objNextSemaphore, this);
            try
            {
                objCurrentSemaphore.SafeWait(token);
            }
            catch
            {
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                        intCountLocalReaders, objLastSemaphore, objCurrentSemaphore);
                Utils.SemaphorePool.Return(ref objNextSemaphore);
                throw;
            }
            try
            {
                // Wait for existing reader locks to finish and exit
                if (intCountLocalReaders == 0)
                {
                    _objReaderSemaphore.SafeWait(token);
                    _objReaderSemaphore.Release();
                }
                else
                {
                    // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                    while (intCountLocalReaders < _intCountActiveReaders)
                        Utils.SafeSleep(token);
#if DEBUG
                    if (intCountLocalReaders > _intCountActiveReaders)
                        Utils.BreakIfDebug();
#endif
                }
            }
            catch
            {
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                        intCountLocalReaders, objLastSemaphore, objCurrentSemaphore);
                try
                {
                    // ReSharper disable once MethodSupportsCancellation
                    objNextSemaphore.SafeWait(CancellationToken.None);
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
        /// Try to asynchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public Task<IAsyncDisposable> EnterWriteLockAsync()
        {
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
#if UPGRADEABLEREADLOCKCHECK
            // We are trying to acquire a write lock from inside a non-upgradeable read lock. This is bad! Make sure this doesn't happen!
            if (_objReaderSemaphore.CurrentCount == 0 && _objInNonUpgradeableReadLock.Value)
                Utils.BreakIfDebug();
#endif
            int intCountLocalReaders = 0;
            DebuggableSemaphoreSlim objLastSemaphore = null;
            DebuggableSemaphoreSlim objCurrentSemaphore = _objTopLevelWriterSemaphore;
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objLastSemaphore, objCurrentSemaphore) = objAsyncLocals;
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentSemaphore || objNextSemaphore == objLastSemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                    intCountLocalReaders, objCurrentSemaphore, objNextSemaphore);
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objLastSemaphore, objCurrentSemaphore, objNextSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentSemaphore, objRelease, intCountLocalReaders);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterWriteLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            if (token.IsCancellationRequested)
                return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));
#if UPGRADEABLEREADLOCKCHECK
            // We are trying to acquire a write lock from inside a non-upgradeable read lock. This is bad! Make sure this doesn't happen!
            if (_objReaderSemaphore.CurrentCount == 0 && _objInNonUpgradeableReadLock.Value)
                Utils.BreakIfDebug();
#endif
            int intCountLocalReaders = 0;
            DebuggableSemaphoreSlim objLastSemaphore = null;
            DebuggableSemaphoreSlim objCurrentSemaphore = _objTopLevelWriterSemaphore;
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objLastSemaphore, objCurrentSemaphore) = objAsyncLocals;
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentSemaphore || objNextSemaphore == objLastSemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                    intCountLocalReaders, objCurrentSemaphore, objNextSemaphore);
            SafeWriterSemaphoreRelease objRelease = new SafeWriterSemaphoreRelease(objLastSemaphore, objCurrentSemaphore, objNextSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentSemaphore, objRelease, intCountLocalReaders, token);
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, SafeWriterSemaphoreRelease objRelease, int intCountLocalReaders)
        {
            await objCurrentSemaphore.WaitAsync().ConfigureAwait(false);
            // Wait for existing reader locks to finish and exit
            if (intCountLocalReaders == 0)
            {
                await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                _objReaderSemaphore.Release();
            }
            else
            {
                // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                while (intCountLocalReaders < _intCountActiveReaders)
                    await Utils.SafeSleepAsync().ConfigureAwait(false);
#if DEBUG
                if (intCountLocalReaders > _intCountActiveReaders)
                    Utils.BreakIfDebug();
#endif
            }
            return objRelease;
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, SafeWriterSemaphoreRelease objRelease, int intCountLocalReaders, CancellationToken token)
        {
            try
            {
                await objCurrentSemaphore.WaitAsync(token).ConfigureAwait(false);
                // Wait for existing reader locks to finish and exit
                if (intCountLocalReaders == 0)
                {
                    await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                    _objReaderSemaphore.Release();
                }
                else
                {
                    // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                    while (intCountLocalReaders < _intCountActiveReaders)
                        await Utils.SafeSleepAsync(token).ConfigureAwait(false);
#if DEBUG
                    if (intCountLocalReaders > _intCountActiveReaders)
                        Utils.BreakIfDebug();
#endif
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
            }
            return objRelease;
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// </summary>
        public IDisposable EnterUpgradeableReadLock(CancellationToken token = default)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine("Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return null;
            }
            token.ThrowIfCancellationRequested();
            int intCountLocalReaders = 0;
            DebuggableSemaphoreSlim objLastSemaphore = null;
            DebuggableSemaphoreSlim objCurrentSemaphore = _objTopLevelWriterSemaphore;
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objLastSemaphore, objCurrentSemaphore) = objAsyncLocals;
            token.ThrowIfCancellationRequested();
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (objCurrentSemaphore.CurrentCount != 0)
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

                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(intCountLocalReaders + 1,
                        objLastSemaphore, objCurrentSemaphore);
                return new SafeUpgradeableReaderSemaphoreRelease(this);
            }

            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
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

                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(intCountLocalReaders + 1,
                        objLastSemaphore, objCurrentSemaphore);
                return new SafeUpgradeableReaderSemaphoreRelease(this);
            }
            finally
            {
                objCurrentSemaphore.Release();
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// </summary>
        public Task<IDisposable> EnterUpgradeableReadLockAsync()
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine("Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }
            // Because of shenanigens around AsyncLocal, we need to set the local readers count in this method instead of any of the async ones
            int intCountLocalReaders = 0;
            DebuggableSemaphoreSlim objLastSemaphore = null;
            DebuggableSemaphoreSlim objCurrentSemaphore = _objTopLevelWriterSemaphore;
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objLastSemaphore, objCurrentSemaphore) = objAsyncLocals;
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(intCountLocalReaders + 1,
                    objLastSemaphore, objCurrentSemaphore);
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            return objCurrentSemaphore.CurrentCount != 0
                ? TakeUpgradeableReadLockCoreLightAsync()
                : TakeUpgradeableReadLockCoreAsync(objCurrentSemaphore);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// </summary>
        public Task<IDisposable> EnterUpgradeableReadLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine("Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }
            if (token.IsCancellationRequested)
                return Task.FromException<IDisposable>(new OperationCanceledException(token));
            // Because of shenanigens around AsyncLocal, we need to set the local readers count in this method instead of any of the async ones
            // To undo this change in case the request is canceled, we will register a callback that will only be disposed at the end of the async methods
            int intCountLocalReaders = 0;
            DebuggableSemaphoreSlim objLastSemaphore = null;
            DebuggableSemaphoreSlim objCurrentSemaphore = _objTopLevelWriterSemaphore;
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objLastSemaphore, objCurrentSemaphore) = objAsyncLocals;
            CancellationTokenRegistration objLocalReaderUndo = token.Register(() =>
            {
                Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals2 = _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals2 != null)
                {
                    (int intCountLocalReaders2, DebuggableSemaphoreSlim objLastSemaphore2,
                        DebuggableSemaphoreSlim objCurrentSemaphore2) = objAsyncLocals2;
                    _objAsyncLocalCurrentsContainer.Value =
                        new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(intCountLocalReaders2 - 1,
                            objLastSemaphore2, objCurrentSemaphore2);
                }
            }, true);
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(intCountLocalReaders + 1,
                    objLastSemaphore, objCurrentSemaphore);
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            return objCurrentSemaphore.CurrentCount != 0
                ? TakeUpgradeableReadLockCoreLightAsync(objLocalReaderUndo, token)
                : TakeUpgradeableReadLockCoreAsync(objCurrentSemaphore, objLocalReaderUndo, token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore)
        {
            await objCurrentSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intCountActiveReaders);
                        throw;
                    }
                }
                return new SafeUpgradeableReaderSemaphoreRelease(this);
            }
            finally
            {
                objCurrentSemaphore.Release();
            }
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, CancellationTokenRegistration objLocalReaderUndo, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
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

                    return new SafeUpgradeableReaderSemaphoreRelease(this);
                }
                finally
                {
                    objCurrentSemaphore.Release();
                }
            }
            finally
            {
                objLocalReaderUndo.Dispose();
            }
        }

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreLightAsync()
        {
            if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
            {
                try
                {
                    await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                }
                catch
                {
                    Interlocked.Decrement(ref _intCountActiveReaders);
                    throw;
                }
            }
            return new SafeUpgradeableReaderSemaphoreRelease(this);
        }

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreLightAsync(CancellationTokenRegistration objLocalReaderUndo, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
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

                return new SafeUpgradeableReaderSemaphoreRelease(this);
            }
            finally
            {
                objLocalReaderUndo.Dispose();
            }
        }

        /// <summary>
        /// Release a lock held for reading that could have been upgraded to a write lock.
        /// </summary>
        private void ExitUpgradeableReadLock()
        {
            if (_intDisposedStatus > 1)
            {
#if DEBUG
                Debug.WriteLine("Exiting a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return;
            }
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0)
                _objReaderSemaphore.Release();
            Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim> objAsyncLocals = _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (int intCountLocalReaders, DebuggableSemaphoreSlim objLastSemaphore,
                    DebuggableSemaphoreSlim objCurrentSemaphore) = objAsyncLocals;
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(intCountLocalReaders - 1,
                        objLastSemaphore, objCurrentSemaphore);
            }
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// </summary>
        public IDisposable EnterReadLock(CancellationToken token = default)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine("Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return null;
            }
            token.ThrowIfCancellationRequested();
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (_objTopLevelWriterSemaphore.CurrentCount != 0)
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
#if UPGRADEABLEREADLOCKCHECK
                _objInNonUpgradeableReadLock.Value = true;
#endif
                return new SafeReaderSemaphoreRelease(this);
            }
            token.ThrowIfCancellationRequested();
            // Check the top writer first to avoid unnecessary AsyncLocal copy-on-write calls
            DebuggableSemaphoreSlim objCurrentSemaphore = _objAsyncLocalCurrentsContainer.Value?.Item3 ?? _objTopLevelWriterSemaphore;
            if (objCurrentSemaphore.CurrentCount != 0)
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
#if UPGRADEABLEREADLOCKCHECK
                _objInNonUpgradeableReadLock.Value = true;
#endif
                return new SafeReaderSemaphoreRelease(this);
            }

            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
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
#if UPGRADEABLEREADLOCKCHECK
                _objInNonUpgradeableReadLock.Value = true;
#endif
                return new SafeReaderSemaphoreRelease(this);
            }
            finally
            {
                objCurrentSemaphore.Release();
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// </summary>
        public Task<IDisposable> EnterReadLockAsync()
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine("Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }
#if UPGRADEABLEREADLOCKCHECK
            _objInNonUpgradeableReadLock.Value = true;
#endif
            if (_objTopLevelWriterSemaphore.CurrentCount != 0)
                return TakeReadLockCoreLightAsync();
            // Check the top writer first to avoid unnecessary AsyncLocal copy-on-write calls
            DebuggableSemaphoreSlim objCurrentSemaphore = _objAsyncLocalCurrentsContainer.Value?.Item3 ?? _objTopLevelWriterSemaphore;
            return objCurrentSemaphore.CurrentCount != 0
                ? TakeReadLockCoreLightAsync()
                : TakeReadLockCoreAsync(objCurrentSemaphore);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// </summary>
        public Task<IDisposable> EnterReadLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine("Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }
            if (token.IsCancellationRequested)
                return Task.FromException<IDisposable>(new OperationCanceledException(token));
#if UPGRADEABLEREADLOCKCHECK
            _objInNonUpgradeableReadLock.Value = true;
            CancellationTokenRegistration objUndoFlag = token.Register(() => _objInNonUpgradeableReadLock.Value = false, true);
            DebuggableSemaphoreSlim objCurrentSemaphore = _objAsyncLocalCurrentsContainer.Value?.Item3 ?? _objTopLevelWriterSemaphore;
            return objCurrentSemaphore.CurrentCount != 0
                ? TakeReadLockCoreLightAsync(objUndoFlag, token)
                : TakeReadLockCoreAsync(objCurrentSemaphore, objUndoFlag, token);
#else
            if (_objTopLevelWriterSemaphore.CurrentCount != 0)
                return TakeReadLockCoreLightAsync(token);
            // Check the top writer first to avoid unnecessary AsyncLocal copy-on-write calls
            DebuggableSemaphoreSlim objCurrentSemaphore = _objAsyncLocalCurrentsContainer.Value?.Item3 ?? _objTopLevelWriterSemaphore;
            return objCurrentSemaphore.CurrentCount != 0
                ? TakeReadLockCoreLightAsync(token)
                : TakeReadLockCoreAsync(objCurrentSemaphore, token);
#endif
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore)
        {
            await objCurrentSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                {
                    try
                    {
                        await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intCountActiveReaders);
                        throw;
                    }
                }

                return new SafeReaderSemaphoreRelease(this);
            }
            finally
            {
                objCurrentSemaphore.Release();
            }
        }

#if UPGRADEABLEREADLOCKCHECK
        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, CancellationTokenRegistration objUndo, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
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

                    return new SafeReaderSemaphoreRelease(this);
                }
                finally
                {
                    objCurrentSemaphore.Release();
                }
            }
            finally
            {
                objUndo.Dispose();
            }
        }
#else
        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(DebuggableSemaphoreSlim objCurrentSemaphore, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
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

                return new SafeReaderSemaphoreRelease(this);
            }
            finally
            {
                objCurrentSemaphore.Release();
            }
        }
#endif

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreLightAsync()
        {
            if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
            {
                try
                {
                    await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                }
                catch
                {
                    Interlocked.Decrement(ref _intCountActiveReaders);
                    throw;
                }
            }

            return new SafeReaderSemaphoreRelease(this);
        }

#if UPGRADEABLEREADLOCKCHECK
        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreLightAsync(CancellationTokenRegistration objUndo, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
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

                return new SafeReaderSemaphoreRelease(this);
            }
            finally
            {
                objUndo.Dispose();
            }
        }
#else
        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreLightAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
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

            return new SafeReaderSemaphoreRelease(this);
        }
#endif

        /// <summary>
        /// Release a lock held for only reading.
        /// </summary>
        private void ExitReadLock()
        {
            if (_intDisposedStatus > 1)
            {
#if DEBUG
                Debug.WriteLine("Exiting a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return;
            }
            if (Interlocked.Decrement(ref _intCountActiveReaders) == 0)
                _objReaderSemaphore.Release();
#if UPGRADEABLEREADLOCKCHECK
            _objInNonUpgradeableReadLock.Value = false;
#endif
        }

        /// <summary>
        /// Are there any async locks held below this one?
        /// </summary>
        public bool IsWriteLockHeldRecursively => IsWriteLockHeld && _objAsyncLocalCurrentsContainer.Value?.Item2 != null;

        /// <summary>
        /// Is there anything holding the lock?
        /// </summary>
        public bool IsWriteLockHeld => _intDisposedStatus == 0 && _objTopLevelWriterSemaphore.CurrentCount == 0;

        /// <summary>
        /// Is there anything holding the reader lock?
        /// </summary>
        public bool IsReadLockHeld => _intDisposedStatus == 0 && _objReaderSemaphore.CurrentCount == 0;

        /// <summary>
        /// Is the locker object already disposed and its allocatable semaphores returned to the semaphore pool?
        /// </summary>
        public bool IsDisposed => _intDisposedStatus > 1;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;

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
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;

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
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        private readonly struct SafeReaderSemaphoreRelease : IDisposable
        {
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeReaderSemaphoreRelease(AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                _objReaderWriterLock = objReaderWriterLock;
            }

            public void Dispose()
            {
                _objReaderWriterLock.ExitReadLock();
            }
        }

        private readonly struct SafeUpgradeableReaderSemaphoreRelease : IDisposable
        {
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeUpgradeableReaderSemaphoreRelease(AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                _objReaderWriterLock = objReaderWriterLock;
            }

            public void Dispose()
            {
                _objReaderWriterLock.ExitUpgradeableReadLock();
            }
        }

        private struct SafeWriterSemaphoreRelease : IAsyncDisposable, IDisposable
        {
            private readonly DebuggableSemaphoreSlim _objLastSemaphore;
            private readonly DebuggableSemaphoreSlim _objCurrentSemaphore;
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

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
                (int intCountLocalReaders, DebuggableSemaphoreSlim objCurrentSemaphoreSlim,
                        DebuggableSemaphoreSlim objNextSemaphoreSlim) =
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (_objNextSemaphore != objNextSemaphoreSlim)
                {
                    if (objNextSemaphoreSlim == null)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextSemaphoreSlim == _objCurrentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore");
                }

                if (objCurrentSemaphoreSlim != _objCurrentSemaphore && (objCurrentSemaphoreSlim != null
                                                                        || _objCurrentSemaphore
                                                                        != _objReaderWriterLock
                                                                            ._objTopLevelWriterSemaphore))
                {
                    if (objCurrentSemaphoreSlim == null)
                        throw new InvalidOperationException(
                            "_objCurrentSemaphore was expected to be the previous semaphore. Instead, the previous semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException("_objCurrentSemaphore was expected to be the previous semaphore");
                }

                // Update _objReaderWriterLock._objCurrentSemaphore in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the
                // update will happen in a copy of the ExecutionContext and the caller
                // won't see the changes.
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                        intCountLocalReaders, _objLastSemaphore, objCurrentSemaphoreSlim);

                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                if (_objCurrentSemaphore.CurrentCount == 0)
                {
                    try
                    {
                        await _objNextSemaphore.WaitAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        _objCurrentSemaphore.Release();
                        throw;
                    }
                    try
                    {
                        _objCurrentSemaphore.Release();
                    }
                    finally
                    {
                        _objNextSemaphore.Release();
                    }
                }
                Utils.SemaphorePool.Return(ref _objNextSemaphore);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
                (int intCountLocalReaders, DebuggableSemaphoreSlim objCurrentSemaphoreSlim,
                        DebuggableSemaphoreSlim objNextSemaphoreSlim) =
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (_objNextSemaphore != objNextSemaphoreSlim)
                {
                    if (objNextSemaphoreSlim == null)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextSemaphoreSlim == _objCurrentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException("_objNextSemaphore was expected to be the current semaphore");
                }

                if (objCurrentSemaphoreSlim != _objCurrentSemaphore && (objCurrentSemaphoreSlim != null
                                                                        || _objCurrentSemaphore
                                                                        != _objReaderWriterLock
                                                                            ._objTopLevelWriterSemaphore))
                {
                    if (objCurrentSemaphoreSlim == null)
                        throw new InvalidOperationException(
                            "_objCurrentSemaphore was expected to be the previous semaphore. Instead, the previous semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    throw new InvalidOperationException(
                        "_objCurrentSemaphore was expected to be the previous semaphore");
                }

                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, DebuggableSemaphoreSlim, DebuggableSemaphoreSlim>(
                        intCountLocalReaders, _objLastSemaphore, objCurrentSemaphoreSlim);

                if (_objCurrentSemaphore.CurrentCount == 0)
                {
                    try
                    {
                        _objNextSemaphore.SafeWait();
                    }
                    catch
                    {
                        _objCurrentSemaphore.Release();
                        throw;
                    }
                    try
                    {
                        _objCurrentSemaphore.Release();
                    }
                    finally
                    {
                        _objNextSemaphore.Release();
                    }
                }
                Utils.SemaphorePool.Return(ref _objNextSemaphore);
            }
        }
    }
}
