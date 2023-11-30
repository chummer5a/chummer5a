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
        // In order to properly allow async lock to be recursive but still make them work properly as locks, we need to set up something
        // that is a bit like a singly-linked list, but as a tree graph. Each lock creates a disposable release object of some kind, and only disposing it frees the lock.
        // Because .NET Framework doesn't have dictionary optimizations for dealing with multiple AsyncLocals stored per context, we need scrape together something similar.
        // Therefore, we store a nested tuple where the first element is the number of active local readers and the second element is the tuple containing our writer lock semaphores
        // TODO: Revert this cursed bodge once we migrate to a version of .NET that has these AsyncLocal optimizations
        private readonly AsyncLocal<Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>>
            _objAsyncLocalCurrentsContainer = new AsyncLocal<Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>>();

        private readonly LinkedSemaphoreSlim _objTopLevelWriterSemaphore = new LinkedSemaphoreSlim(null);

        private int _intCountActiveHiPrioReaders;
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
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            DebuggableSemaphoreSlim objNextSemaphore = null;
            LinkedSemaphoreSlim objNextLinkedSemaphore;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    throw new TimeoutException();
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                    _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                {
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                    if (intCountLocalReaders == int.MinValue)
                        throw new InvalidOperationException(
                            "Write lock was attempted to be acquired inside a non-upgradeable read lock.");
                }

                if (objNextSemaphore == null)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                       objNextSemaphore == objCurrentLinkedSemaphore.ParentLinkedSemaphore?.MySemaphore)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                objNextLinkedSemaphore =
                    new LinkedSemaphoreSlim(objCurrentLinkedSemaphore, objNextSemaphore, true);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            // While we are attempting to acquire the write lock, act as if all previous upgradeable readers have been turned into writers
            ChangeNumActiveReaders(-intCountLocalReaders);
            try
            {
                objCurrentLinkedSemaphore.SafeWaitAll(token, objTopMostHeldWriterSemaphore);
            }
            catch
            {
                ChangeNumActiveReaders(intCountLocalReaders);
                throw;
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                    0, objNextLinkedSemaphore, objCurrentLinkedSemaphore);

            SafeWriterSemaphoreRelease objRelease =
                new SafeWriterSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);

            try
            {
                // Wait for existing reader locks to finish and exit
                // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                while (_intCountActiveHiPrioReaders > 0 || _intCountActiveReaders > 0)
                    Utils.SafeSleep(token);
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
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            DebuggableSemaphoreSlim objNextSemaphore = null;
            LinkedSemaphoreSlim objNextLinkedSemaphore;
            SafeWriterSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IAsyncDisposable>(new TimeoutException());
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
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

                if (objNextSemaphore == null)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                       objNextSemaphore == objCurrentLinkedSemaphore.ParentLinkedSemaphore?.MySemaphore)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                objNextLinkedSemaphore =
                    new LinkedSemaphoreSlim(objCurrentLinkedSemaphore, objNextSemaphore, true);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            objRelease =
                new SafeWriterSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                    0, objNextLinkedSemaphore, objCurrentLinkedSemaphore);

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
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            DebuggableSemaphoreSlim objNextSemaphore = null;
            LinkedSemaphoreSlim objNextLinkedSemaphore;
            SafeWriterSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IAsyncDisposable>(new TimeoutException());
                if (token.IsCancellationRequested)
                    return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
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

                if (objNextSemaphore == null)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                       objNextSemaphore == objCurrentLinkedSemaphore.ParentLinkedSemaphore?.MySemaphore)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                objNextLinkedSemaphore =
                    new LinkedSemaphoreSlim(objCurrentLinkedSemaphore, objNextSemaphore, true);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            objRelease =
                new SafeWriterSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                    0, objNextLinkedSemaphore, objCurrentLinkedSemaphore);

            return TakeWriteLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease,
                intCountLocalReaders, token);
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(LinkedSemaphoreSlim objCurrentSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeWriterSemaphoreRelease objRelease,
            int intCountLocalReaders)
        {
            // While we are attempting to acquire the write lock, act as if all previous upgradeable readers have been turned into writers
            ChangeNumActiveReaders(-intCountLocalReaders);
            await objCurrentSemaphore.WaitAllAsync(objTopMostHeldWriterSemaphore).ConfigureAwait(false);
            // Wait for existing reader locks to finish and exit
            // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
            while (_intCountActiveHiPrioReaders > 0 || _intCountActiveReaders > 0)
                await Utils.SafeSleepAsync().ConfigureAwait(false);

            return objRelease;
        }

        private async Task<IAsyncDisposable> TakeWriteLockCoreAsync(LinkedSemaphoreSlim objCurrentSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeWriterSemaphoreRelease objRelease,
            int intCountLocalReaders, CancellationToken token)
        {
            // While we are attempting to acquire the write lock, act as if all previous upgradeable readers have been turned into writers
            ChangeNumActiveReaders(-intCountLocalReaders);
            if (token.IsCancellationRequested)
                return objRelease;
            try
            {
                await objCurrentSemaphore.WaitAllAsync(token, objTopMostHeldWriterSemaphore).ConfigureAwait(false);
                // Wait for existing reader locks to finish and exit
                // It's OK that this isn't (inter)locked because we should already handle race condition issues by having acquired the writer lock
                while (_intCountActiveHiPrioReaders > 0 || _intCountActiveReaders > 0)
                    await Utils.SafeSleepAsync(token).ConfigureAwait(false);
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
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return null;
            }

            token.ThrowIfCancellationRequested();
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            DebuggableSemaphoreSlim objNextSemaphore = null;
            LinkedSemaphoreSlim objNextLinkedSemaphore;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    throw new TimeoutException();
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
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
                if (objNextSemaphore == null)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                       objNextSemaphore == objCurrentLinkedSemaphore.ParentLinkedSemaphore?.MySemaphore)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                objNextLinkedSemaphore =
                    new LinkedSemaphoreSlim(objCurrentLinkedSemaphore, objNextSemaphore, true);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount != 0)
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                        objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
                return new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }

            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
            objCurrentLinkedSemaphore.MySemaphore.SafeWait(token);
            try
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                        objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
                return new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }
            finally
            {
                objCurrentLinkedSemaphore.MySemaphore.Release();
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
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return Task.FromResult<IDisposable>(null);
            }

            // Because of shenanigens around AsyncLocal, we need to set the local readers count in this method instead of any of the async ones
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            DebuggableSemaphoreSlim objNextSemaphore = null;
            LinkedSemaphoreSlim objNextLinkedSemaphore;
            SafeUpgradeableReaderSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IDisposable>(new TimeoutException());
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
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

                if (objNextSemaphore == null)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                       objNextSemaphore == objCurrentLinkedSemaphore.ParentLinkedSemaphore?.MySemaphore)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                objNextLinkedSemaphore =
                    new LinkedSemaphoreSlim(objCurrentLinkedSemaphore, objNextSemaphore, true);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            objRelease =
                new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);

            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount != 0)
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                        objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
                return Task.FromResult<IDisposable>(objRelease);
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                    objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);

            return TakeUpgradeableReadLockCoreAsync(objCurrentLinkedSemaphore, objRelease);
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
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return Task.FromResult<IDisposable>(null);
            }

            if (token.IsCancellationRequested)
                return Task.FromException<IDisposable>(new OperationCanceledException(token));
            // Because of shenanigens around AsyncLocal, we need to set the local readers count in this method instead of any of the async ones
            // To undo this change in case the request is canceled, we will register a callback that will only be disposed at the end of the async methods
            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            DebuggableSemaphoreSlim objNextSemaphore = null;
            LinkedSemaphoreSlim objNextLinkedSemaphore;
            SafeUpgradeableReaderSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IDisposable>(new TimeoutException());
                if (token.IsCancellationRequested)
                    return Task.FromException<IDisposable>(new OperationCanceledException(token));
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
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

                if (objNextSemaphore == null)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                while (objNextSemaphore == objCurrentLinkedSemaphore.MySemaphore ||
                       objNextSemaphore == objCurrentLinkedSemaphore.ParentLinkedSemaphore?.MySemaphore)
                    objNextSemaphore = Utils.SemaphorePool.Get();
                objNextLinkedSemaphore =
                    new LinkedSemaphoreSlim(objCurrentLinkedSemaphore, objNextSemaphore, true);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            objRelease = new SafeUpgradeableReaderSemaphoreRelease(intCountLocalReaders, objNextLinkedSemaphore,
                objTopMostHeldWriterSemaphore, this);

            // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
            if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount != 0)
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                        objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);
                return Task.FromResult<IDisposable>(objRelease);
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(intCountLocalReaders + 1,
                    objNextLinkedSemaphore, objTopMostHeldWriterSemaphore);

            return TakeUpgradeableReadLockCoreAsync(objCurrentLinkedSemaphore, objRelease, token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeUpgradeableReadLockCoreAsync(LinkedSemaphoreSlim objCurrentSemaphore,
            SafeUpgradeableReaderSemaphoreRelease objRelease, CancellationToken token = default)
        {
            try
            {
                await objCurrentSemaphore.MySemaphore.WaitAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
                ChangeNumActiveReaders(1); // We always need to increase active readers because count always gets decreased when release is disposed
                return objRelease;
            }

            try
            {
                ChangeNumActiveReaders(1);
            }
            finally
            {
                objCurrentSemaphore.MySemaphore.Release();
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
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return null;
            }

            token.ThrowIfCancellationRequested();

            ChangeNumActiveReaders(1); // Temporarily increase active reader count to avoid race conditions 
            try
            {
                if (_intCountActiveHiPrioReaders > 0)
                {
                    ChangeNumActiveReaders(1);
                    return new SafeFastReaderSemaphoreRelease(this);
                }
            }
            finally
            {
                ChangeNumActiveReaders(-1);
            }

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    throw new TimeoutException();
                token.ThrowIfCancellationRequested();
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                    _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;

                // Only do the complicated steps if any write lock is currently being held, otherwise skip it and just process the read lock
                if (intCountLocalReaders == int.MinValue)
                {
                    ChangeNumActiveReaders(1);
                    _objAsyncLocalCurrentsContainer.Value =
                        new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                            objCurrentLinkedSemaphore,
                            objTopMostHeldWriterSemaphore);
                    return new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore, this);
                }
            } while (objCurrentLinkedSemaphore.IsDisposed);

            if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount != 0)
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }

            token.ThrowIfCancellationRequested();

            // Temporarily acquiring a write lock just to mess with the read locks is a bottleneck, so don't do any such setting unless we need it
            objCurrentLinkedSemaphore.MySemaphore.SafeWait(token);
            try
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return new SafeReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }
            finally
            {
                objCurrentLinkedSemaphore.MySemaphore.Release();
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
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return Task.FromResult<IDisposable>(null);
            }

            ChangeNumActiveReaders(1); // Temporarily increase active reader count to avoid race conditions 
            try
            {
                if (_intCountActiveHiPrioReaders > 0)
                {
                    ChangeNumActiveReaders(1);
                    return Task.FromResult<IDisposable>(new SafeFastReaderSemaphoreRelease(this));
                }
            }
            finally
            {
                ChangeNumActiveReaders(-1);
            }

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            SafeReaderSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IDisposable>(new TimeoutException());
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals = _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;

                objRelease = new SafeReaderSemaphoreRelease(intCountLocalReaders,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
                if (intCountLocalReaders == int.MinValue)
                {
                    ChangeNumActiveReaders(1);
                    _objAsyncLocalCurrentsContainer.Value =
                        new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                            objCurrentLinkedSemaphore,
                            objTopMostHeldWriterSemaphore);
                    return Task.FromResult<IDisposable>(objRelease);
                }
            } while (objCurrentLinkedSemaphore.IsDisposed);

            if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount != 0)
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return Task.FromResult<IDisposable>(objRelease);
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore);
            return TakeReadLockCoreAsync(objCurrentLinkedSemaphore, objRelease);
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
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return Task.FromResult<IDisposable>(null);
            }

            if (token.IsCancellationRequested)
                return Task.FromException<IDisposable>(new OperationCanceledException(token));

            ChangeNumActiveReaders(1); // Temporarily increase active reader count to avoid race conditions 
            try
            {
                if (_intCountActiveHiPrioReaders > 0)
                {
                    ChangeNumActiveReaders(1);
                    return Task.FromResult<IDisposable>(new SafeFastReaderSemaphoreRelease(this));
                }
            }
            finally
            {
                ChangeNumActiveReaders(-1);
            }

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            SafeReaderSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IDisposable>(new TimeoutException());
                if (token.IsCancellationRequested)
                    return Task.FromException<IDisposable>(new OperationCanceledException(token));
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals = _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                {
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                }

                objRelease = new SafeReaderSemaphoreRelease(intCountLocalReaders,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
                if (intCountLocalReaders == int.MinValue)
                {
                    ChangeNumActiveReaders(1);
                    _objAsyncLocalCurrentsContainer.Value =
                        new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                            objCurrentLinkedSemaphore,
                            objTopMostHeldWriterSemaphore);
                    return Task.FromResult<IDisposable>(objRelease);
                }
            } while (objCurrentLinkedSemaphore.IsDisposed);

            if (objCurrentLinkedSemaphore.MySemaphore.CurrentCount != 0)
            {
                ChangeNumActiveReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return Task.FromResult<IDisposable>(objRelease);
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore);
            return TakeReadLockCoreAsync(objCurrentLinkedSemaphore, objRelease, token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(LinkedSemaphoreSlim objCurrentLinkedSemaphore, SafeReaderSemaphoreRelease objRelease)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return objRelease;
            }
            await objCurrentLinkedSemaphore.MySemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                ChangeNumActiveReaders(1);
            }
            finally
            {
                objCurrentLinkedSemaphore.MySemaphore.Release();
            }

            return objRelease;
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IDisposable> TakeReadLockCoreAsync(LinkedSemaphoreSlim objCurrentLinkedSemaphore, SafeReaderSemaphoreRelease objRelease, CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return objRelease;
            }
            try
            {
                await objCurrentLinkedSemaphore.MySemaphore.WaitAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
                ChangeNumActiveReaders(1); // We always need to increase active readers because count always gets decreased when release is disposed
                return objRelease;
            }

            try
            {
                ChangeNumActiveReaders(1);
            }
            finally
            {
                objCurrentLinkedSemaphore.MySemaphore.Release();
            }

            return objRelease;
        }

        /// <summary>
        /// Try to synchronously obtain a high-priority lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// Useful if we know we are going to try to acquire a read lock a whole bunch of times and don't want to deal with AsyncLocal's overhead each time.
        /// </summary>
        public IDisposable EnterHiPrioReadLock(CancellationToken token = default)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a high-priority read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return null;
            }

            token.ThrowIfCancellationRequested();

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    throw new TimeoutException();
                token.ThrowIfCancellationRequested();
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                    _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
            } while (objCurrentLinkedSemaphore.IsDisposed);

            // Because we are a high-priority reader, we *must* temporarily acquire a write lock
            objCurrentLinkedSemaphore.SafeWaitAll(token, objTopMostHeldWriterSemaphore);
            try
            {
                ChangeNumActiveHiPrioReaders(1);
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                        objCurrentLinkedSemaphore,
                        objTopMostHeldWriterSemaphore);
                return new SafeHiPrioReaderSemaphoreRelease(intCountLocalReaders, objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            }
            finally
            {
                objCurrentLinkedSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a high-priority lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// Useful if we know we are going to try to acquire a read lock a whole bunch of times and don't want to deal with AsyncLocal's overhead each time.
        /// </summary>
        public Task<IAsyncDisposable> EnterHiPrioReadLockAsync()
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return Task.FromResult<IAsyncDisposable>(null);
            }

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            SafeHiPrioReaderSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IAsyncDisposable>(new TimeoutException());
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                    _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                objRelease = new SafeHiPrioReaderSemaphoreRelease(intCountLocalReaders,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore);

            return TakeHiPrioReadLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease);
        }

        /// <summary>
        /// Try to asynchronously obtain a high-priority lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// Useful if we know we are going to try to acquire a read lock a whole bunch of times and don't want to deal with AsyncLocal's overhead each time.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterHiPrioReadLockAsync(CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return Task.FromResult<IAsyncDisposable>(null);
            }

            if (token.IsCancellationRequested)
                return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));

            int intCountLocalReaders = 0;
            LinkedSemaphoreSlim objCurrentLinkedSemaphore;
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore = null;
            SafeHiPrioReaderSemaphoreRelease objRelease;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            do
            {
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                    return Task.FromException<IAsyncDisposable>(new TimeoutException());
                if (token.IsCancellationRequested)
                    return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));
                objCurrentLinkedSemaphore = _objTopLevelWriterSemaphore;
                Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim> objAsyncLocals =
                    _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                    (intCountLocalReaders, objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore) = objAsyncLocals;
                objRelease = new SafeHiPrioReaderSemaphoreRelease(intCountLocalReaders,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore, this);
            } while (objCurrentLinkedSemaphore.IsDisposed);

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(int.MinValue,
                    objCurrentLinkedSemaphore,
                    objTopMostHeldWriterSemaphore);

            return TakeHiPrioReadLockCoreAsync(objCurrentLinkedSemaphore, objTopMostHeldWriterSemaphore, objRelease, token);
        }

        /// <summary>
        /// Heavier read lock entrant, used if a write lock is already being held somewhere
        /// </summary>
        private async Task<IAsyncDisposable> TakeHiPrioReadLockCoreAsync(LinkedSemaphoreSlim objCurrentLinkedSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeHiPrioReaderSemaphoreRelease objRelease)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return objRelease;
            }
            await objCurrentLinkedSemaphore.WaitAllAsync(objTopMostHeldWriterSemaphore).ConfigureAwait(false);
            try
            {
                ChangeNumActiveHiPrioReaders(1);
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
        private async Task<IAsyncDisposable> TakeHiPrioReadLockCoreAsync(LinkedSemaphoreSlim objCurrentLinkedSemaphore,
            LinkedSemaphoreSlim objTopMostHeldWriterSemaphore, SafeHiPrioReaderSemaphoreRelease objRelease,
            CancellationToken token)
        {
            if (_intDisposedStatus != 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return objRelease;
            }
            try
            {
                await objCurrentLinkedSemaphore.WaitAllAsync(token, objTopMostHeldWriterSemaphore)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this because it must be handled as a disposal in the original ExecutionContext
                ChangeNumActiveHiPrioReaders(1); // We always need to increase active readers because count always gets decreased when release is disposed
                return objRelease;
            }

            try
            {
                ChangeNumActiveHiPrioReaders(1);
            }
            finally
            {
                objCurrentLinkedSemaphore.ReleaseAll(objTopMostHeldWriterSemaphore);
            }

            return objRelease;
        }

        private void ChangeNumActiveReaders(int intDiff)
        {
            if (_intDisposedStatus > 1)
            {
                if (intDiff <= 0)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Decreasing the number of active readers in a reader-writer lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(EnhancedStackTrace.Current().ToString());
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
                    Interlocked.Decrement(ref _intCountActiveReaders);
                    break;
                case 1:
                    Interlocked.Increment(ref _intCountActiveReaders);

                    break;
                default:
                    Interlocked.Add(ref _intCountActiveReaders, intDiff);

                    break;
            }
        }

        private void ChangeNumActiveHiPrioReaders(int intDiff)
        {
            if (_intDisposedStatus > 1)
            {
                if (intDiff <= 0)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Decreasing the number of active readers in a reader-writer lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(EnhancedStackTrace.Current().ToString());
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
                    Interlocked.Decrement(ref _intCountActiveHiPrioReaders);
                    break;
                case 1:
                    Interlocked.Increment(ref _intCountActiveHiPrioReaders);
                    break;
                default:
                    Interlocked.Add(ref _intCountActiveHiPrioReaders, intDiff);
                    break;
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
                while (_intCountActiveHiPrioReaders > 0 || _intCountActiveReaders > 0)
                    Utils.SafeSleep();
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
                while (_intCountActiveHiPrioReaders > 0 || _intCountActiveReaders > 0)
                    await Utils.SafeSleepAsync().ConfigureAwait(false);
                _objTopLevelWriterSemaphore.MySemaphore.Release();
                await _objTopLevelWriterSemaphore.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        private readonly struct SafeFastReaderSemaphoreRelease : IDisposable
        {
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeFastReaderSemaphoreRelease(AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                _objReaderWriterLock = objReaderWriterLock;
            }

            public void Dispose()
            {
                _objReaderWriterLock.ChangeNumActiveReaders(-1);
            }
        }

        private readonly struct SafeHiPrioReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly int _intOldCountLocalReaders;
            private readonly LinkedSemaphoreSlim _objNextLinkedSemaphore;
            private readonly LinkedSemaphoreSlim _objPreviousTopMostHeldWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeHiPrioReaderSemaphoreRelease(int intOldCountLocalReaders, LinkedSemaphoreSlim objNextLinkedSemaphore,
                LinkedSemaphoreSlim objPreviousTopMostHeldWriterSemaphore,
                AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objNextLinkedSemaphore == null)
                    throw new ArgumentNullException(nameof(objNextLinkedSemaphore));
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentLinkedSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentLinkedSemaphore;
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

                if (_intOldCountLocalReaders != int.MinValue)
                {
                    // Wait for all other readers to exit before exiting ourselves
                    while (_intOldCountLocalReaders < _objReaderWriterLock._intCountActiveReaders)
                        Utils.SafeSleep();
                }

                _objReaderWriterLock.ChangeNumActiveHiPrioReaders(-1);
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
                if (_intOldCountLocalReaders != int.MinValue)
                {
                    // Wait for all other readers to exit before exiting ourselves
                    while (_intOldCountLocalReaders < _objReaderWriterLock._intCountActiveReaders)
                        await Utils.SafeSleepAsync().ConfigureAwait(false);
                }

                _objReaderWriterLock.ChangeNumActiveHiPrioReaders(-1);
            }
        }

        private readonly struct SafeReaderSemaphoreRelease : IDisposable
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
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentLinkedSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentLinkedSemaphore;
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
        }

        private readonly struct SafeUpgradeableReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly int _intOldCountLocalReaders;
            private readonly LinkedSemaphoreSlim _objNextLinkedSemaphore;
            private readonly LinkedSemaphoreSlim _objPreviousTopMostHeldWriterSemaphore;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public SafeUpgradeableReaderSemaphoreRelease(int intOldCountLocalReaders,
                LinkedSemaphoreSlim objNextLinkedSemaphore,
                LinkedSemaphoreSlim objPreviousTopMostHeldWriterSemaphore,
                AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objNextLinkedSemaphore == null)
                    throw new ArgumentNullException(nameof(objNextLinkedSemaphore));
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentLinkedSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentLinkedSemaphore;
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
                    Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                    return;
                }

#if DEBUG
                (_, LinkedSemaphoreSlim objNextLinkedSemaphore, _) =
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (_objNextLinkedSemaphore != objNextLinkedSemaphore)
                {
                    if (objNextLinkedSemaphore == null)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentLinkedSemaphore)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException(
                        "_objNextLinkedSemaphore was expected to be the current semaphore.");
                }
#endif

                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                        _intOldCountLocalReaders, _objNextLinkedSemaphore.ParentLinkedSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);
                _objNextLinkedSemaphore.Dispose();
                _objReaderWriterLock.ChangeNumActiveReaders(-1);
            }

            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Exiting a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                    return new ValueTask(Task.CompletedTask);
                }

#if DEBUG
                (_, LinkedSemaphoreSlim objNextLinkedSemaphore, _) =
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (_objNextLinkedSemaphore != objNextLinkedSemaphore)
                {
                    if (objNextLinkedSemaphore == null)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the current semaphore is null.\n\n"
                            + "This may be because AsyncLocal's control flow is the inverse of what one expects, so acquiring "
                            + "the lock inside a function and then leaving the function before exiting the lock can produce this situation.");
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentLinkedSemaphore)
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
                        _intOldCountLocalReaders, _objNextLinkedSemaphore.ParentLinkedSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);
                return DisposeCoreAsync();
            }

            private async ValueTask DisposeCoreAsync()
            {
                await _objNextLinkedSemaphore.DisposeAsync().ConfigureAwait(false);
                _objReaderWriterLock.ChangeNumActiveReaders(-1);
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
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = objNextLinkedSemaphore.ParentLinkedSemaphore;
                if (objCurrentLinkedSemaphore != null)
                {
                    if (objCurrentLinkedSemaphore.MySemaphore == objNextLinkedSemaphore.MySemaphore)
                        throw new InvalidOperationException(
                            "Current and next semaphores are identical, this should not happen.");
                    LinkedSemaphoreSlim objLastLinkedSemaphore = objCurrentLinkedSemaphore.ParentLinkedSemaphore;
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
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentLinkedSemaphore)
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
                        _intOldCountLocalReaders, _objNextLinkedSemaphore.ParentLinkedSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);

                return DisposeCoreAsync(_intOldCountLocalReaders);
            }

            private async ValueTask DisposeCoreAsync(int intCountLocalReaders)
            {
                // Wait for all other readers to exit before exiting ourselves
                while (_objReaderWriterLock._intCountActiveReaders > 0 &&
                       _objReaderWriterLock._intCountActiveHiPrioReaders > 0)
                    await Utils.SafeSleepAsync().ConfigureAwait(false);
                LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objNextLinkedSemaphore.ParentLinkedSemaphore;
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

                await _objNextLinkedSemaphore.DisposeAsync().ConfigureAwait(false);

                _objReaderWriterLock.ChangeNumActiveReaders(intCountLocalReaders);
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
                    if (objNextLinkedSemaphore == _objNextLinkedSemaphore.ParentLinkedSemaphore)
                        throw new InvalidOperationException(
                            "_objNextLinkedSemaphore was expected to be the current semaphore. Instead, the old semaphore was never unset.");
                    throw new InvalidOperationException(
                        "_objNextLinkedSemaphore was expected to be the current semaphore.");
                }
#endif

                LinkedSemaphoreSlim objCurrentLinkedSemaphore = _objNextLinkedSemaphore.ParentLinkedSemaphore;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<int, LinkedSemaphoreSlim, LinkedSemaphoreSlim>(
                        _intOldCountLocalReaders, objCurrentLinkedSemaphore,
                        _objPreviousTopMostHeldWriterSemaphore);
                // Wait for all other readers to exit before exiting ourselves
                while (_objReaderWriterLock._intCountActiveReaders > 0 &&
                       _objReaderWriterLock._intCountActiveHiPrioReaders > 0)
                    Utils.SafeSleep();

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

                _objReaderWriterLock.ChangeNumActiveReaders(_intOldCountLocalReaders);
            }
        }
    }
}
