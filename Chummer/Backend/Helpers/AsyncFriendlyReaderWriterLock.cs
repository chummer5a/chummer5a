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
        // that is a bit like a singly-linked list, but as a tree graph. Each lock creates a disposable release object of some kind, and only disposing it frees the lock.
        // Because .NET Framework doesn't have dictionary optimizations for dealing with multiple AsyncLocals stored per context, we need scrape together something similar.
        // Therefore, we store a nested tuple where the first element is the number of active local readers and the second element is the tuple containing our writer lock semaphores
        // TODO: Revert this cursed bodge once we migrate to a version of .NET that has these AsyncLocal optimizations
        private readonly AsyncLocal<Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>>
            _objAsyncLocalCurrentsContainer = new AsyncLocal<Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>>();

        private readonly LinkedSemaphoreSlim _objTopLevelWriterSemaphore = new LinkedSemaphoreSlim();

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
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                if (intCountLocalReaders == int.MinValue)
                    throw new InvalidOperationException(
                        "Write lock was attempted to be acquired inside a non-upgradeable read lock.");
            }

            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                   objNextSemaphore == objCurrentLinkedSemaphore.ParentSemaphore?.MySemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            LinkedSemaphoreSlim objNextLinkedSemaphore = new LinkedSemaphoreSlim(objNextSemaphore, true)
            {
                ParentSemaphore = objCurrentLinkedSemaphore
            };

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                    0, objNextLinkedSemaphore, objCurrentLinkedSemaphore);
            SafeWriterSemaphoreRelease objRelease =
                new SafeWriterSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);

            // While we are attempting to acquire the write lock, act as if all previous upgradeable readers have been turned into writers
            ChangeNumActiveReaders(-intCountLocalReaders, token: token);
            try
            {
                objCurrentLinkedSemaphore.SafeWaitAll(token, objTopMostHeldWriterSemaphore);
            }
            catch
            {
                ChangeNumActiveReaders(intCountLocalReaders, false, CancellationToken.None);
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
                    while (_intCountActiveReaders > 0)
                        Utils.SafeSleep(token);
#if DEBUG
                    if (_intCountActiveReaders < 0)
                        Utils.BreakIfDebug();
#endif
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
        public Task<IAsyncDisposable> EnterWriteLockAsync()
        {
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(
                    new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                if (intCountLocalReaders == int.MinValue)
                    return Task.FromException<IAsyncDisposable>(
                        new InvalidOperationException(
                            "Write lock was attempted to be acquired inside a non-upgradeable read lock."));
            }

            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                   objNextSemaphore == objCurrentLinkedSemaphore.ParentSemaphore?.MySemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            LinkedSemaphoreSlim objNextLinkedSemaphore = new LinkedSemaphoreSlim(objNextSemaphore, true)
            {
                ParentSemaphore = objCurrentLinkedSemaphore
            };
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                    0, objNextLinkedSemaphore, objCurrentLinkedSemaphore);
            SafeWriterSemaphoreRelease objRelease =
                new SafeWriterSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease,
                intCountLocalReaders);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterWriteLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(
                    new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));
            if (token.IsCancellationRequested)
                return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                if (intCountLocalReaders == int.MinValue)
                    return Task.FromException<IAsyncDisposable>(
                        new InvalidOperationException(
                            "Write lock was attempted to be acquired inside a non-upgradeable read lock."));
            }

            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                   objNextSemaphore == objCurrentLinkedSemaphore.ParentSemaphore?.MySemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            LinkedSemaphoreSlim objNextLinkedSemaphore = new LinkedSemaphoreSlim(objNextSemaphore, true)
            {
                ParentSemaphore = objCurrentLinkedSemaphore
            };
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                    0, objNextLinkedSemaphore, objCurrentLinkedSemaphore);
            SafeWriterSemaphoreRelease objRelease =
                new SafeWriterSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            return TakeWriteLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease,
                intCountLocalReaders, token);
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(LinkedSemaphoreSlim objCurrentSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeWriterSemaphoreRelease objRelease,
            int intCountLocalReaders)
        {
            try
            {
                // While we are attempting to acquire the write lock, act as if all previous upgradeable readers have been turned into writers
                await ChangeNumActiveReadersAsync(-intCountLocalReaders, false).ConfigureAwait(false);
                await objCurrentSemaphore.WaitAllAsync(objTopMostHeldWriterSemaphore).ConfigureAwait(false);
                // Wait for existing reader locks to finish and exit
                if (intCountLocalReaders == 0)
                {
                    await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                    _objReaderSemaphore.Release();
                }
                else
                {
                    // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                    while (_intCountActiveReaders > 0)
                        await Utils.SafeSleepAsync().ConfigureAwait(false);
#if DEBUG
                    if (_intCountActiveReaders < 0)
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

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(LinkedSemaphoreSlim objCurrentSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeWriterSemaphoreRelease objRelease,
            int intCountLocalReaders, CancellationToken token)
        {
            try
            {
                // While we are attempting to acquire the write lock, act as if all previous upgradeable readers have been turned into writers
                await ChangeNumActiveReadersAsync(-intCountLocalReaders, false, token).ConfigureAwait(false);
                await objCurrentSemaphore.WaitAllAsync(token, objTopMostHeldWriterSemaphore).ConfigureAwait(false);
                // Wait for existing reader locks to finish and exit
                if (intCountLocalReaders == 0)
                {
                    await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                    _objReaderSemaphore.Release();
                }
                else
                {
                    // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                    while (_intCountActiveReaders > 0)
                        await Utils.SafeSleepAsync(token).ConfigureAwait(false);
#if DEBUG
                    if (_intCountActiveReaders < 0)
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
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return null;
            }

            token.ThrowIfCancellationRequested();
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                if (intCountLocalReaders == int.MinValue)
                    throw new InvalidOperationException(
                        "Upgradeable read lock was attempted to be acquired inside a non-upgradeable read lock.");
            }

            token.ThrowIfCancellationRequested();
            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                   objNextSemaphore == objCurrentLinkedSemaphore.ParentSemaphore?.MySemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            LinkedSemaphoreSlim objNextLinkedSemaphore = new LinkedSemaphoreSlim(objNextSemaphore, true)
            {
                ParentSemaphore = objCurrentLinkedSemaphore
            };
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (_objTopLevelWriterSemaphore.MySemaphore.CurrentCount == 0)
            {
                ChangeNumActiveReaders(1, token: token);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                        objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
                return new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }

            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
            objCurrentLinkedSemaphore.SafeWaitAll(token, objTopMostHeldWriterSemaphore);
            try
            {
                ChangeNumActiveReaders(1, token: token);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                        objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
            }
            finally
            {
                objCurrentLinkedSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
            }

            return new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                objTopMostHeldWriterSemaphore, this);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// </summary>
        public Task<IDisposable> EnterUpgradeableReadLockAsync()
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }

            // Because of shenanigens around AsyncLocal, we need to set the local readers count in this method instead of any of the async ones
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                if (intCountLocalReaders == int.MinValue)
                    return Task.FromException<IDisposable>(
                        new InvalidOperationException(
                            "Upgradeable read lock was attempted to be acquired inside a non-upgradeable read lock."));
            }

            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                   objNextSemaphore == objCurrentLinkedSemaphore.ParentSemaphore?.MySemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            LinkedSemaphoreSlim objNextLinkedSemaphore = new LinkedSemaphoreSlim(objNextSemaphore, true)
            {
                ParentSemaphore = objCurrentLinkedSemaphore
            };
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                    objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
            SafeUpgradeableReaderSemaphoreRelease objRelease =
                new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            return _objTopLevelWriterSemaphore.MySemaphore.CurrentCount == 0
                ? TakeUpgradeableReadLockCoreLightAsync(objRelease)
                : TakeUpgradeableReadLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore,
                    objRelease);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IDisposable> EnterUpgradeableReadLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }

            if (token.IsCancellationRequested)
                return Task.FromException<IDisposable>(new OperationCanceledException(token));
            // Because of shenanigens around AsyncLocal, we need to set the local readers count in this method instead of any of the async ones
            // To undo this change in case the request is canceled, we will register a callback that will only be disposed at the end of the async methods
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
            {
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                if (intCountLocalReaders == int.MinValue)
                    return Task.FromException<IDisposable>(
                        new InvalidOperationException(
                            "Upgradeable read lock was attempted to be acquired inside a non-upgradeable read lock."));
            }

            DebuggableSemaphoreSlim objNextSemaphore = Utils.SemaphorePool.Get();
            // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
            // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
            // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
            // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
            while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                   objNextSemaphore == objCurrentLinkedSemaphore.ParentSemaphore?.MySemaphore)
                objNextSemaphore = Utils.SemaphorePool.Get();
            LinkedSemaphoreSlim objNextLinkedSemaphore = new LinkedSemaphoreSlim(objNextSemaphore, true)
            {
                ParentSemaphore = objCurrentLinkedSemaphore
            };
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                    objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
            SafeUpgradeableReaderSemaphoreRelease objRelease =
                new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            return _objTopLevelWriterSemaphore.MySemaphore.CurrentCount == 0
                ? TakeUpgradeableReadLockCoreLightAsync(objRelease, token)
                : TakeUpgradeableReadLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease,
                    token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreAsync(LinkedSemaphoreSlim objCurrentSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeUpgradeableReaderSemaphoreRelease objRelease,
            CancellationToken token = default)
        {
            try
            {
                await objCurrentSemaphore.WaitAllAsync(token, objTopMostHeldWriterSemaphore).ConfigureAwait(false);
                try
                {
                    await ChangeNumActiveReadersAsync(1, false, token).ConfigureAwait(false);
                }
                finally
                {
                    objCurrentSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
            }

            return objRelease;
        }

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreLightAsync(
            SafeUpgradeableReaderSemaphoreRelease objRelease, CancellationToken token = default)
        {
            try
            {
                await ChangeNumActiveReadersAsync(1, false, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
            }

            return objRelease;
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// </summary>
        public IDisposable EnterReadLock(CancellationToken token = default)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return null;
            }

            token.ThrowIfCancellationRequested();
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;

            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (_objTopLevelWriterSemaphore.MySemaphore.CurrentCount != 0 || intCountLocalReaders == int.MinValue)
            {
                ChangeNumActiveReaders(1, token: token);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }

            token.ThrowIfCancellationRequested();

            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
            objCurrentLinkedSemaphore.SafeWaitAll(token, objTopMostHeldWriterSemaphore);
            try
            {
                ChangeNumActiveReaders(1, token: token);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }
            finally
            {
                objCurrentLinkedSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
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
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore);
            SafeReaderSemaphoreRelease objRelease = new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                objTopMostHeldWriterSemaphore, this);
            return _objTopLevelWriterSemaphore.MySemaphore.CurrentCount != 0 || intCountLocalReaders == int.MinValue
                ? TakeReadLockCoreLightAsync(objRelease)
                : TakeReadLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IDisposable> EnterReadLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(Environment.StackTrace);
#endif
                return Task.FromResult<IDisposable>(null);
            }

            if (token.IsCancellationRequested)
                return Task.FromException<IDisposable>(new OperationCanceledException(token));
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != null)
                (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore);
            SafeReaderSemaphoreRelease objRelease = new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                objTopMostHeldWriterSemaphore, this);
            return _objTopLevelWriterSemaphore.MySemaphore.CurrentCount != 0 || intCountLocalReaders == int.MinValue
                ? TakeReadLockCoreLightAsync(objRelease, token)
                : TakeReadLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease, token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(LinkedSemaphoreSlim objCurrentLinkedSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeReaderSemaphoreRelease objRelease)
        {
            await objCurrentLinkedSemaphore.WaitAllAsync(objTopMostHeldWriterSemaphore).ConfigureAwait(false);
            try
            {
                await ChangeNumActiveReadersAsync(1).ConfigureAwait(false);
            }
            finally
            {
                objCurrentLinkedSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
            }

            return objRelease;
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(LinkedSemaphoreSlim objCurrentLinkedSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeReaderSemaphoreRelease objRelease,
            CancellationToken token)
        {
            try
            {
                await objCurrentLinkedSemaphore.WaitAllAsync(token, objTopMostHeldWriterSemaphore)
                    .ConfigureAwait(false);
                try
                {
                    await ChangeNumActiveReadersAsync(1, false, token: token).ConfigureAwait(false);
                }
                finally
                {
                    objCurrentLinkedSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
            }

            return objRelease;
        }

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreLightAsync(SafeReaderSemaphoreRelease objRelease)
        {
            await ChangeNumActiveReadersAsync(1).ConfigureAwait(false);
            return objRelease;
        }

        /// <summary>
        /// Lighter read lock entrant, used if no write locks are being held and we just want to worry about the read lock
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreLightAsync(SafeReaderSemaphoreRelease objRelease,
            CancellationToken token)
        {
            try
            {
                await ChangeNumActiveReadersAsync(1, false, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
            }

            return objRelease;
        }

        private void ChangeNumActiveReaders(int intDiff, bool blnUndoOnCancel = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (_intDisposedStatus > 1)
            {
                if (intDiff <= 0)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Decreasing the number of active readers in a reader-writer lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(Environment.StackTrace);
#endif
                    return;
                }

                throw new ObjectDisposedException(ToString());
            }

            if (intDiff == 0)
                return;

            switch (intDiff)
            {
                case -1:
                    if (Interlocked.Decrement(ref _intCountActiveReaders) == 0)
                        _objReaderSemaphore.Release();
                    break;
                case 1:
                    if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                    {
                        try
                        {
                            _objReaderSemaphore.SafeWait(token);
                        }
                        catch (OperationCanceledException)
                        {
                            if (blnUndoOnCancel)
                                Interlocked.Decrement(ref _intCountActiveReaders);
                            throw;
                        }
                        catch
                        {
                            Interlocked.Decrement(ref _intCountActiveReaders);
                            throw;
                        }
                    }

                    break;
                default:
                    int intNewValue = Interlocked.Add(ref _intCountActiveReaders, intDiff);
                    if (intNewValue == intDiff)
                    {
                        try
                        {
                            _objReaderSemaphore.SafeWait(token);
                        }
                        catch (OperationCanceledException)
                        {
                            if (blnUndoOnCancel)
                                Interlocked.Add(ref _intCountActiveReaders, -intDiff);
                            throw;
                        }
                        catch
                        {
                            Interlocked.Add(ref _intCountActiveReaders, -intDiff);
                            throw;
                        }
                    }
                    else if (intNewValue == 0)
                        _objReaderSemaphore.Release();

                    break;
            }
        }

        private Task ChangeNumActiveReadersAsync(int intDiff, bool blnUndoOnCancel = true,
            CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);

            if (_intDisposedStatus > 1)
            {
                if (intDiff <= 0)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Decreasing the number of active readers in a reader-writer lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(Environment.StackTrace);
#endif
                    return Task.CompletedTask;
                }

                return Task.FromException(new ObjectDisposedException(ToString()));
            }

            if (intDiff == 0)
                return Task.CompletedTask;

            switch (intDiff)
            {
                case -1:
                    if (Interlocked.Decrement(ref _intCountActiveReaders) == 0)
                        _objReaderSemaphore.Release();
                    return Task.CompletedTask;
                case 1:
                    if (Interlocked.Increment(ref _intCountActiveReaders) == 1)
                        return WaitReaderSemaphore1();

                    async Task WaitReaderSemaphore1()
                    {
                        try
                        {
                            await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            if (blnUndoOnCancel)
                                Interlocked.Decrement(ref _intCountActiveReaders);
                            throw;
                        }
                        catch
                        {
                            Interlocked.Decrement(ref _intCountActiveReaders);
                            throw;
                        }
                    }

                    return Task.CompletedTask;
                default:
                    int intNewValue = Interlocked.Add(ref _intCountActiveReaders, intDiff);
                    if (intNewValue == intDiff)
                    {
                        return WaitReaderSemaphore2();

                        async Task WaitReaderSemaphore2()
                        {
                            try
                            {
                                await _objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                if (blnUndoOnCancel)
                                    Interlocked.Add(ref _intCountActiveReaders, -intDiff);
                                throw;
                            }
                            catch
                            {
                                Interlocked.Add(ref _intCountActiveReaders, -intDiff);
                                throw;
                            }
                        }
                    }

                    if (intNewValue == 0)
                        _objReaderSemaphore.Release();

                    return Task.CompletedTask;
            }
        }

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
                _objTopLevelWriterSemaphore.MySemaphore.SafeWait();
                _objReaderSemaphore.SafeWait();
                _objReaderSemaphore.Release();
                _objReaderSemaphore.Dispose();
                _objTopLevelWriterSemaphore.MySemaphore.Release();
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
                await _objTopLevelWriterSemaphore.MySemaphore.WaitAsync().ConfigureAwait(false);
                await _objReaderSemaphore.WaitAsync().ConfigureAwait(false);
                _objReaderSemaphore.Release();
                _objReaderSemaphore.Dispose();
                _objTopLevelWriterSemaphore.MySemaphore.Release();
                _objTopLevelWriterSemaphore.Dispose();
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        private readonly struct SafeReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly int _intOldCountLocalReaders;
            private readonly LinkedSemaphoreSlim _objNextLinkedSemaphore;
            private readonly LinkedSemaphoreSlim _objPreviousTopMostHeldWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeReaderSemaphoreRelease(int intOldCountLocalReaders, LinkedSemaphoreSlim objNextLinkedSemaphore,
                LinkedSemaphoreSlim objPreviousTopMostHeldWriterSemaphore,
                AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objNextLinkedSemaphore == null)
                    throw new ArgumentNullException(nameof(objNextLinkedSemaphore));
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentSemaphore;
                    if (objLastLinkedSemaphore != null)
                    {
                        if (objLastLinkedSemaphore.MySemaphore == objCurrentLinkedSemaphore.MySemaphore)
                            throw new InvalidOperationException(
                                "Last and current semaphores are identical, this should not happen.");
                        if (objLastLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                            throw new InvalidOperationException(
                                "Last and next semaphores are identical, this should not happen.");
                    }
                }

                _intOldCountLocalReaders = intOldCountLocalReaders;
                _objNextLinkedSemaphore = objNextLinkedSemaphore;
                _objPreviousTopMostHeldWriterSemaphore = objPreviousTopMostHeldWriterSemaphore;
                _objReaderWriterLock = objReaderWriterLock;
            }

            public void Dispose()
            {
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(_intOldCountLocalReaders,
                        _objNextLinkedSemaphore, _objPreviousTopMostHeldWriterSemaphore);

                _objReaderWriterLock.ChangeNumActiveReaders(-1);
            }

            public ValueTask DisposeAsync()
            {
                // Update _objReaderWriterLock._objAsyncLocalCurrentsContainer in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a
                // copy of the ExecutionContext and the caller won't see the changes.
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(_intOldCountLocalReaders,
                        _objNextLinkedSemaphore, _objPreviousTopMostHeldWriterSemaphore);

                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                await _objReaderWriterLock.ChangeNumActiveReadersAsync(-1).ConfigureAwait(false);
            }
        }

        private readonly struct SafeUpgradeableReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly int _intOldCountLocalReaders;
            private readonly LinkedSemaphoreSlim _objNextLinkedSemaphore;
            private readonly LinkedSemaphoreSlim _objPreviousTopMostHeldWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeUpgradeableReaderSemaphoreRelease(int intOldCountLocalReaders, LinkedSemaphoreSlim objNextLinkedSemaphore,
                LinkedSemaphoreSlim objPreviousTopMostHeldWriterSemaphore,
                AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objNextLinkedSemaphore == null)
                    throw new ArgumentNullException(nameof(objNextLinkedSemaphore));
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentSemaphore;
                    if (objLastLinkedSemaphore != null)
                    {
                        if (objLastLinkedSemaphore.MySemaphore == objCurrentLinkedSemaphore.MySemaphore)
                            throw new InvalidOperationException(
                                "Last and current semaphores are identical, this should not happen.");
                        if (objLastLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                            throw new InvalidOperationException(
                                "Last and next semaphores are identical, this should not happen.");
                    }
                }

                _intOldCountLocalReaders = intOldCountLocalReaders;
                _objNextLinkedSemaphore = objNextLinkedSemaphore;
                _objPreviousTopMostHeldWriterSemaphore = objPreviousTopMostHeldWriterSemaphore;
                _objReaderWriterLock = objReaderWriterLock;
            }

            public void Dispose()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Exiting a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(Environment.StackTrace);
#endif
                    return;
                }

#if DEBUG
                (_, LinkedSemaphoreSlim objNextLinkedSemaphore, _) = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (_objNextLinkedSemaphore != objNextLinkedSemaphore)
                {
                    if (objNextLinkedSemaphore == null)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException(
                        "_objNextLinkedSemaphore was expected to be the current semaphore.");
                }
#endif

                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                        _intOldCountLocalReaders, _objNextLinkedSemaphore.ParentSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);
                _objReaderWriterLock.ChangeNumActiveReaders(-1);
                _objNextLinkedSemaphore.Dispose();
            }

            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Exiting a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(Environment.StackTrace);
#endif
                    return new ValueTask(Task.CompletedTask);
                }

#if DEBUG
                (_, LinkedSemaphoreSlim objNextLinkedSemaphore, _) = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (_objNextLinkedSemaphore != objNextLinkedSemaphore)
                {
                    if (objNextLinkedSemaphore == null)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException(
                        "_objNextLinkedSemaphore was expected to be the current semaphore.");
                }
#endif

                // Update _objReaderWriterLock._objAsyncLocalCurrentsContainer in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a
                // copy of the ExecutionContext and the caller won't see the changes.
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                        _intOldCountLocalReaders, _objNextLinkedSemaphore.ParentSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);
                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                await _objReaderWriterLock.ChangeNumActiveReadersAsync(-1).ConfigureAwait(false);
                _objNextLinkedSemaphore.Dispose();
            }
        }

        private readonly struct SafeWriterSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly int _intOldCountLocalReaders;
            private readonly LinkedSemaphoreSlim _objNextLinkedSemaphore;
            private readonly LinkedSemaphoreSlim _objPreviousTopMostHeldWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeWriterSemaphoreRelease(int intOldCountLocalReaders, LinkedSemaphoreSlim objNextLinkedSemaphore,
                LinkedSemaphoreSlim objPreviousTopMostHeldWriterSemaphore,
                AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objNextLinkedSemaphore == null)
                    throw new ArgumentNullException(nameof(objNextLinkedSemaphore));
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentSemaphore;
                    if (objLastLinkedSemaphore != null)
                    {
                        if (objLastLinkedSemaphore.MySemaphore == objCurrentLinkedSemaphore.MySemaphore)
                            throw new InvalidOperationException(
                                "Last and current semaphores are identical, this should not happen.");
                        if (objLastLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                            throw new InvalidOperationException(
                                "Last and next semaphores are identical, this should not happen.");
                    }
                }

                _intOldCountLocalReaders = intOldCountLocalReaders;
                _objNextLinkedSemaphore = objNextLinkedSemaphore;
                _objPreviousTopMostHeldWriterSemaphore = objPreviousTopMostHeldWriterSemaphore;
                _objReaderWriterLock = objReaderWriterLock;
            }

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
#if DEBUG
                (int intCountLocalReaders, LinkedSemaphoreSlim objNextLinkedSemaphore,
                        _) =
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (intCountLocalReaders != 0)
                {
                    throw new InvalidOperationException(
                        "intCountLocalReaders was expected to be zero but isn't, a reader lock is still active within the call stack.");
                }
                if (_objNextLinkedSemaphore != objNextLinkedSemaphore)
                {
                    if (objNextLinkedSemaphore == null)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException(
                        "_objNextLinkedSemaphore was expected to be the current semaphore.");
                }
#endif

                // Update _objReaderWriterLock._objAsyncLocalCurrentsContainer in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a
                // copy of the ExecutionContext and the caller won't see the changes.
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                        _intOldCountLocalReaders, _objNextLinkedSemaphore.ParentSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);

                return DisposeCoreAsync(_intOldCountLocalReaders);
            }

            private async ValueTask DisposeCoreAsync(int intCountLocalReaders)
            {
                await _objReaderWriterLock.ChangeNumActiveReadersAsync(intCountLocalReaders).ConfigureAwait(false);
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objNextLinkedSemaphore.ParentSemaphore;
                if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount == 0)
                {
                    try
                    {
                        await _objNextLinkedSemaphore.MySemaphore.WaitAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        objCurrentLinkedSemaphore.ReleaseAll(_objPreviousTopMostHeldWriterSemaphore);
                        throw;
                    }

                    try
                    {
                        objCurrentLinkedSemaphore.ReleaseAll(_objPreviousTopMostHeldWriterSemaphore);
                    }
                    finally
                    {
                        _objNextLinkedSemaphore.MySemaphore.Release();
                    }
                }

                _objNextLinkedSemaphore.Dispose();
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
#if DEBUG
                (int intCountLocalReaders, LinkedSemaphoreSlim objNextLinkedSemaphore,
                        _) =
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (intCountLocalReaders != 0)
                {
                    throw new InvalidOperationException(
                        "intCountLocalReaders was expected to be zero but isn't, a reader lock is still active within the call stack.");
                }
                if (_objNextLinkedSemaphore != objNextLinkedSemaphore)
                {
                    if (objNextLinkedSemaphore == null)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentSemaphore)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException(
                        "_objNextLinkedSemaphore was expected to be the current semaphore.");
                }
#endif

                LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objNextLinkedSemaphore.ParentSemaphore;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                        _intOldCountLocalReaders, objCurrentLinkedSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);
                _objReaderWriterLock.ChangeNumActiveReaders(_intOldCountLocalReaders);

                if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount == 0)
                {
                    try
                    {
                        _objNextLinkedSemaphore.MySemaphore.SafeWait();
                    }
                    catch
                    {
                        objCurrentLinkedSemaphore.ReleaseAll(_objPreviousTopMostHeldWriterSemaphore);
                        throw;
                    }

                    try
                    {
                        objCurrentLinkedSemaphore.ReleaseAll(_objPreviousTopMostHeldWriterSemaphore);
                    }
                    finally
                    {
                        _objNextLinkedSemaphore.MySemaphore.Release();
                    }
                }

                _objNextLinkedSemaphore.Dispose();
            }
        }
    }
}
