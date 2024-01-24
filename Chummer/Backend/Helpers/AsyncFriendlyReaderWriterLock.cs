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
        private readonly AsyncLocal<
                Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>>
            _objAsyncLocalCurrentsContainer =
                new AsyncLocal<Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper,
                    bool>>();

        private readonly LinkedAsyncRWLockHelper _objTopLevelHelper = new LinkedAsyncRWLockHelper(null, false);

        private AsyncFriendlyReaderWriterLock _objParentLock;
        private bool _blnLockReadOnlyForParent;

        private int _intDisposedStatus;

        public AsyncFriendlyReaderWriterLock(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            if (objParentLock?.IsDisposed == false)
            {
                _objParentLock = objParentLock;
                _blnLockReadOnlyForParent = blnLockReadOnlyForParent;
            }
        }

        public void SetParent(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _objTopLevelHelper.TakeSingleWriteLock(token);
            try
            {
                token.ThrowIfCancellationRequested();
                _objParentLock = objParentLock;
                _blnLockReadOnlyForParent = blnLockReadOnlyForParent;
            }
            finally
            {
                _objTopLevelHelper.ReleaseSingleWriteLock();
            }
        }

        public async Task SetParentAsync(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await _objTopLevelHelper.TakeSingleWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _objParentLock = objParentLock;
                _blnLockReadOnlyForParent = blnLockReadOnlyForParent;
            }
            finally
            {
                _objTopLevelHelper.ReleaseSingleWriteLock();
            }
        }

        private Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper,
            LinkedAsyncRWLockHelper, bool> GetHelpers(bool blnForReadLock = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            LinkedAsyncRWLockHelper objCurrentHelper = _objTopLevelHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader = null;
            LinkedAsyncRWLockHelper objTopMostHeldWriter = null;
            bool blnIsInReadLock = false;
            LinkedAsyncRWLockHelper objNextHelper = null;
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            if (blnForReadLock)
            {
                token.ThrowIfCancellationRequested();
                Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>
                    objAsyncLocals =
                        _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null)
                    (objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, blnIsInReadLock) =
                        objAsyncLocals;
            }
            else
            {
                int intLoopCount = 0;
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                        throw new TimeoutException();
                    Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>
                        objAsyncLocals =
                            _objAsyncLocalCurrentsContainer.Value;
                    if (objAsyncLocals != null)
                    {
                        (objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, blnIsInReadLock) =
                            objAsyncLocals;
                    }
                    else
                    {
                        objCurrentHelper = _objTopLevelHelper;
                        objTopMostHeldUReader = null;
                        objTopMostHeldWriter = null;
                        blnIsInReadLock = false;
                    }

                    if (objCurrentHelper.IsDisposed)
                        continue;
                    try
                    {
                        // Setting the helper here makes sure we prevent the current helper from being disposed in-between
                        objNextHelper = new LinkedAsyncRWLockHelper(objCurrentHelper);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Current helper got disposed in-between, so swallow this
                        continue;
                    }

                    break;
                }
            }

            return new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper,
                LinkedAsyncRWLockHelper, bool>(objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                objTopMostHeldWriter, blnIsInReadLock);
        }

        /// <summary>
        /// Try to synchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// </summary>
        public IDisposable EnterWriteLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            (LinkedAsyncRWLockHelper objCurrentHelper, LinkedAsyncRWLockHelper objNextHelper,
                    LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter,
                    bool blnIsInReadLock) =
                GetHelpers(token: token);

            if (blnIsInReadLock)
                throw new InvalidOperationException(
                    "Attempted to take a write lock while inside of a non-upgradeable read lock.");

            try
            {
                objCurrentHelper.TakeWriteLock(objTopMostHeldWriter, token);
            }
            catch
            {
                objNextHelper.Dispose();
                throw;
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                    objNextHelper, objTopMostHeldUReader, objCurrentHelper, false);

            if (_objParentLock == null)
                return new SafeWriterSemaphoreRelease(objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter, this);
            IDisposable objParentRelease = _blnLockReadOnlyForParent
                ? _objParentLock.EnterReadLock(token)
                : _objParentLock.EnterUpgradeableReadLock(token);
            return new SafeWriterSemaphoreRelease(objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter, this,
                objParentRelease: objParentRelease);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for writing.
        /// The returned SafeSemaphoreWriterRelease must be stored for when the write lock is to be released.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterWriteLockAsync(CancellationToken token = default)
        {
            // This method is set up to return a Task because we need to make sure to manipulate AsyncLocals before the async engine is initialized
            if (token.IsCancellationRequested)
                return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(
                    new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));

            LinkedAsyncRWLockHelper objCurrentHelper;
            LinkedAsyncRWLockHelper objNextHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader;
            LinkedAsyncRWLockHelper objTopMostHeldWriter;
            bool blnIsInReadLock;
            try
            {
                (objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter, blnIsInReadLock) =
                    GetHelpers(token: token);
            }
            catch (Exception e)
            {
                Utils.BreakIfDebug();
                return Task.FromException<IAsyncDisposable>(e);
            }

            if (blnIsInReadLock)
            {
                return Task.FromException<IAsyncDisposable>(
                    new InvalidOperationException(
                        "Attempted to take a write lock while inside of a non-upgradeable read lock."));
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                    objNextHelper, objTopMostHeldUReader, objCurrentHelper, false);

            if (_objParentLock != null)
            {
                return (_blnLockReadOnlyForParent
                    ? _objParentLock.EnterReadLockAsync(token).ContinueWith(async x => await TakeWriteLockCoreAsync(
                        objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                        objTopMostHeldWriter, null, await x.ConfigureAwait(false), token).ConfigureAwait(false), TaskScheduler.Current)
                    : _objParentLock.EnterUpgradeableReadLockAsync(token).ContinueWith(async x =>
                        await TakeWriteLockCoreAsync(
                            objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                            objTopMostHeldWriter, null, await x.ConfigureAwait(false), token).ConfigureAwait(false), TaskScheduler.Current)).Unwrap();
            }

            return TakeWriteLockCoreAsync(objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                innerToken: token);

            async Task<IAsyncDisposable> TakeWriteLockCoreAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                LinkedAsyncRWLockHelper objInnerNextHelper,
                LinkedAsyncRWLockHelper objInnerTopMostHeldUReader, LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                IDisposable objParentRelease = null, IAsyncDisposable objParentReleaseAsync = null, CancellationToken innerToken = default)
            {
                try
                {
                    await objInnerCurrentHelper.TakeWriteLockAsync(objInnerTopMostHeldWriter, innerToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this because unsetting the AsyncLocal must be handled as a disposal in the original ExecutionContext
                    return new SafeWriterSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, this, true, objParentRelease, objParentReleaseAsync);
                }

                return new SafeWriterSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                    objInnerTopMostHeldWriter, this, false, objParentRelease, objParentReleaseAsync);
            }
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// </summary>
        public IDisposable EnterUpgradeableReadLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock));
            (LinkedAsyncRWLockHelper objCurrentHelper, LinkedAsyncRWLockHelper objNextHelper,
                    LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter,
                    bool blnIsInReadLock) =
                GetHelpers(token: token);

            if (blnIsInReadLock)
            {
                throw new InvalidOperationException(
                    "Attempted to take an upgradeable read lock while inside of a non-upgradeable read lock.");
            }

            try
            {
                objCurrentHelper.TakeUpgradeableReadLock(token);
            }
            catch
            {
                objNextHelper.Dispose();
                throw;
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                    objNextHelper, objCurrentHelper, objTopMostHeldWriter, false);

            if (_objParentLock == null)
                return new SafeUpgradeableReaderSemaphoreRelease(objNextHelper, objTopMostHeldUReader,
                    objTopMostHeldWriter, this);
            IDisposable objParentRelease = _blnLockReadOnlyForParent
                ? _objParentLock.EnterReadLock(token)
                : _objParentLock.EnterUpgradeableReadLock(token);
            return new SafeUpgradeableReaderSemaphoreRelease(objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                this, objParentRelease: objParentRelease);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterUpgradeableReadLockAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromException<IAsyncDisposable>(new OperationCanceledException(token));
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(
                    new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));

            LinkedAsyncRWLockHelper objCurrentHelper;
            LinkedAsyncRWLockHelper objNextHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader;
            LinkedAsyncRWLockHelper objTopMostHeldWriter;
            bool blnIsInReadLock;
            try
            {
                (objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter, blnIsInReadLock) =
                    GetHelpers(token: token);
            }
            catch (Exception e)
            {
                Utils.BreakIfDebug();
                return Task.FromException<IAsyncDisposable>(e);
            }

            if (blnIsInReadLock)
            {
                return Task.FromException<IAsyncDisposable>(
                    new InvalidOperationException(
                        "Attempted to take an upgradeable read lock while inside of a non-upgradeable read lock."));
            }

            _objAsyncLocalCurrentsContainer.Value =
                new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                    objNextHelper, objCurrentHelper, objTopMostHeldWriter, false);

            if (_objParentLock != null)
            {
                return (_blnLockReadOnlyForParent
                    ? _objParentLock.EnterReadLockAsync(token).ContinueWith(async x =>
                        await TakeUpgradeableReadLockCoreAsync(
                            objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                            objTopMostHeldWriter, null, await x.ConfigureAwait(false), token).ConfigureAwait(false), TaskScheduler.Current)
                    : _objParentLock.EnterUpgradeableReadLockAsync(token).ContinueWith(async x =>
                        await TakeUpgradeableReadLockCoreAsync(
                            objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                            objTopMostHeldWriter, null, await x.ConfigureAwait(false), token).ConfigureAwait(false), TaskScheduler.Current)).Unwrap();
            }

            return TakeUpgradeableReadLockCoreAsync(objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                objTopMostHeldWriter, innerToken: token);

            async Task<IAsyncDisposable> TakeUpgradeableReadLockCoreAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                LinkedAsyncRWLockHelper objInnerNextHelper,
                LinkedAsyncRWLockHelper objInnerTopMostHeldUReader, LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                IDisposable objParentRelease = null, IAsyncDisposable objParentReleaseAsync = null, CancellationToken innerToken = default)
            {
                try
                {
                    await objInnerCurrentHelper.TakeUpgradeableReadLockAsync(innerToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this because unsetting the AsyncLocal must be handled as a disposal in the original ExecutionContext
                    return new SafeUpgradeableReaderSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, this, true, objParentRelease, objParentReleaseAsync);
                }

                return new SafeUpgradeableReaderSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                    objInnerTopMostHeldWriter, this, false, objParentRelease, objParentReleaseAsync);
            }
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// </summary>
        public IDisposable EnterReadLock(CancellationToken token = default)
        {
            return EnterReadLock(false, token);
        }

        /// <summary>
        /// Try to synchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// This version will set the lock's parent to an upgradeable read lock instead of a non-upgradeable one
        /// </summary>
        public IDisposable EnterReadLockWithUpgradeableParent(CancellationToken token = default)
        {
            return EnterReadLock(true, token);
        }

        private IDisposable EnterReadLock(bool blnParentLockIsUpgradeable, CancellationToken token = default)
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

            (LinkedAsyncRWLockHelper objCurrentHelper, LinkedAsyncRWLockHelper _,
                    LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter,
                    bool blnIsInReadLock) =
                GetHelpers(true, token);

            if (objCurrentHelper.IsDisposed)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                if (!blnIsInReadLock)
                {
                    _objAsyncLocalCurrentsContainer.Value =
                        new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                            objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, true);
                }

                if (_objParentLock == null)
                    return new SafeReaderSemaphoreRelease(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                        blnIsInReadLock, this, true);
                IDisposable objParentRelease2 = blnParentLockIsUpgradeable
                    ? _objParentLock.EnterUpgradeableReadLockAsync(token)
                    : _objParentLock.EnterReadLock(token);
                return new SafeReaderSemaphoreRelease(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                    blnIsInReadLock, this, objCurrentHelper.IsDisposed, objParentRelease2);
            }

            objCurrentHelper.TakeReadLock(blnIsInReadLock, token);

            if (!blnIsInReadLock)
            {
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, true);
            }

            if (_objParentLock == null)
                return new SafeReaderSemaphoreRelease(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                    blnIsInReadLock, this, objCurrentHelper.IsDisposed);
            IDisposable objParentRelease = blnParentLockIsUpgradeable
                ? _objParentLock.EnterUpgradeableReadLockAsync(token)
                : _objParentLock.EnterReadLock(token);
            return new SafeReaderSemaphoreRelease(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                blnIsInReadLock, this, objCurrentHelper.IsDisposed, objParentRelease);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterReadLockAsync(CancellationToken token = default)
        {
            return EnterReadLockAsync(false, token);
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// This version will set the lock's parent to an upgradeable read lock instead of a non-upgradeable one
        /// </summary>
        public Task<IAsyncDisposable> EnterReadLockWithUpgradeableParentAsync(CancellationToken token = default)
        {
            return EnterReadLockAsync(true, token);
        }

        private Task<IAsyncDisposable> EnterReadLockAsync(bool blnParentLockIsUpgradeable, CancellationToken token = default)
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

            LinkedAsyncRWLockHelper objCurrentHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader;
            LinkedAsyncRWLockHelper objTopMostHeldWriter;
            bool blnIsInReadLock;
            try
            {
                (objCurrentHelper, _, objTopMostHeldUReader, objTopMostHeldWriter, blnIsInReadLock) =
                    GetHelpers(true, token);
            }
            catch (Exception e)
            {
                Utils.BreakIfDebug();
                return Task.FromException<IAsyncDisposable>(e);
            }

            if (!blnIsInReadLock)
            {
                _objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, true);
            }

            if (_objParentLock != null)
            {
                return blnParentLockIsUpgradeable
                    ? _objParentLock.EnterUpgradeableReadLockAsync(token).ContinueWith(async x => await TakeReadLockCoreAsync(
                            objCurrentHelper, objTopMostHeldUReader,
                            objTopMostHeldWriter, blnIsInReadLock, null, await x.ConfigureAwait(false), token)
                        .ConfigureAwait(false), TaskScheduler.Current).Unwrap()
                    : _objParentLock.EnterReadLockAsync(token).ContinueWith(async x => await TakeReadLockCoreAsync(
                            objCurrentHelper, objTopMostHeldUReader,
                            objTopMostHeldWriter, blnIsInReadLock, null, await x.ConfigureAwait(false), token)
                        .ConfigureAwait(false), TaskScheduler.Current).Unwrap();
            }

            return TakeReadLockCoreAsync(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, blnIsInReadLock,
                innerToken: token);

            async Task<IAsyncDisposable> TakeReadLockCoreAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                LinkedAsyncRWLockHelper objInnerTopMostHeldUReader, LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                bool blnInnerIsInReadLock, IDisposable objParentRelease = null, IAsyncDisposable objParentReleaseAsync = null, CancellationToken innerToken = default)
            {
                if (_intDisposedStatus != 0 || objCurrentHelper.IsDisposed)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                    return new SafeReaderSemaphoreRelease(objInnerCurrentHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, blnInnerIsInReadLock, this, true, objParentRelease, objParentReleaseAsync);
                }

                try
                {
                    await objInnerCurrentHelper.TakeReadLockAsync(blnInnerIsInReadLock, innerToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this because unsetting the AsyncLocal must be handled as a disposal in the original ExecutionContext
                    return new SafeReaderSemaphoreRelease(objInnerCurrentHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, blnInnerIsInReadLock, this, true, objParentRelease, objParentReleaseAsync);
                }

                return new SafeReaderSemaphoreRelease(objInnerCurrentHelper, objInnerTopMostHeldUReader,
                    objInnerTopMostHeldWriter, blnInnerIsInReadLock, this, objCurrentHelper.IsDisposed, objParentRelease, objParentReleaseAsync);
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
                _objTopLevelHelper.Dispose();
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
                await _objTopLevelHelper.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                Interlocked.CompareExchange(ref _intDisposedStatus, 2, 1);
            }
        }

        private readonly struct SafeReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly LinkedAsyncRWLockHelper _objCurrentHelper;
            private readonly LinkedAsyncRWLockHelper _objTopMostHeldUReader;
            private readonly LinkedAsyncRWLockHelper _objTopMostHeldWriter;
            private readonly bool _blnOldIsInReadLock;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;
            private readonly bool _blnSkipUnlockOnDispose;
            private readonly IDisposable _objParentRelease;
            private readonly IAsyncDisposable _objParentReleaseAsync;

            public SafeReaderSemaphoreRelease(LinkedAsyncRWLockHelper objCurrentHelper,
                LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter,
                bool blnIsInReadLock, AsyncFriendlyReaderWriterLock objReaderWriterLock,
                bool blnSkipUnlockOnDispose = false, IDisposable objParentRelease = null, IAsyncDisposable objParentReleaseAsync = null)
            {
                if (objCurrentHelper == null)
                    throw new ArgumentNullException(nameof(objCurrentHelper));
#if DEBUG
                LinkedAsyncRWLockHelper objLastHelper = objCurrentHelper.ParentLinkedHelper;
                if (objLastHelper != null && objLastHelper == objCurrentHelper)
                    throw new InvalidOperationException(
                        "Last and current helpers are identical, this should not happen.");
#endif

                _objCurrentHelper = objCurrentHelper;
                _objTopMostHeldUReader = objTopMostHeldUReader;
                _objTopMostHeldWriter = objTopMostHeldWriter;
                _blnOldIsInReadLock = blnIsInReadLock;
                _objReaderWriterLock = objReaderWriterLock;
                _blnSkipUnlockOnDispose = blnSkipUnlockOnDispose;
                _objParentRelease = objParentRelease;
                _objParentReleaseAsync = objParentReleaseAsync;
            }

            public void Dispose()
            {
                if (_objParentReleaseAsync != null)
                    throw new InvalidOperationException(
                        "Tried to synchronously dispose a lock with a parent that was acquired asynchronously.");
                _objParentRelease?.Dispose();

                if (_objReaderWriterLock._intDisposedStatus > 1)
                    return;

                if (!_blnOldIsInReadLock)
                {
                    _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                        new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                            _objCurrentHelper, _objTopMostHeldUReader, _objTopMostHeldWriter, false);
                }

                if (!_blnSkipUnlockOnDispose)
                    _objCurrentHelper.ReleaseReadLock();
            }

            public ValueTask DisposeAsync()
            {
                DisposeAsyncPre();
                return _objParentReleaseAsync == null
                       && (_objReaderWriterLock._intDisposedStatus > 1 || _blnSkipUnlockOnDispose)
                    ? default
                    : DisposeCoreAsync();
            }

            public void DisposeAsyncPre()
            {
                // Update _objReaderWriterLock._objAsyncLocalCurrentsContainer in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a
                // copy of the ExecutionContext and the caller won't see the changes.
                _objParentRelease?.Dispose();
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        objCastReleaseWriter.DisposeAsyncPre();
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        objCastReleaseUReader.DisposeAsyncPre();
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        objCastReleaseReader.DisposeAsyncPre();
                        break;
                }

                if (_objReaderWriterLock._intDisposedStatus > 1)
                    return;
                if (_blnOldIsInReadLock)
                    return;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        _objCurrentHelper, _objTopMostHeldUReader, _objTopMostHeldWriter, false);
            }

            public async ValueTask DisposeCoreAsync()
            {
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        await objCastReleaseWriter.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        await objCastReleaseUReader.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        await objCastReleaseReader.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                }
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    return;
                if (!_blnSkipUnlockOnDispose)
                    await _objCurrentHelper.ReleaseReadLockAsync().ConfigureAwait(false);
            }
        }

        private readonly struct SafeUpgradeableReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly LinkedAsyncRWLockHelper _objNextHelper;
            private readonly LinkedAsyncRWLockHelper _objPreviousTopMostHeldUReader;
            private readonly LinkedAsyncRWLockHelper _objPreviousTopMostHeldWriter;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;
            private readonly bool _blnSkipUnlockOnDispose;
            private readonly IDisposable _objParentRelease;
            private readonly IAsyncDisposable _objParentReleaseAsync;

            public SafeUpgradeableReaderSemaphoreRelease(LinkedAsyncRWLockHelper objNextHelper,
                LinkedAsyncRWLockHelper objPreviousTopMostHeldUReader,
                LinkedAsyncRWLockHelper objPreviousTopMostHeldWriter, AsyncFriendlyReaderWriterLock objReaderWriterLock,
                bool blnSkipUnlockOnDispose = false, IDisposable objParentRelease = null, IAsyncDisposable objParentReleaseAsync = null)
            {
                if (objNextHelper == null)
                    throw new ArgumentNullException(nameof(objNextHelper));
#if DEBUG
                LinkedAsyncRWLockHelper objCurrentHelper = objNextHelper.ParentLinkedHelper;
                if (objCurrentHelper != null)
                {
                    if (objCurrentHelper == objNextHelper)
                        throw new InvalidOperationException(
                            "Current and next helpers are identical, this should not happen.");
                    LinkedAsyncRWLockHelper objLastHelper = objCurrentHelper.ParentLinkedHelper;
                    if (objLastHelper != null)
                    {
                        if (objLastHelper == objCurrentHelper)
                            throw new InvalidOperationException(
                                "Last and current helpers are identical, this should not happen.");
                        if (objLastHelper == objNextHelper)
                            throw new InvalidOperationException(
                                "Last and next helpers are identical, this should not happen.");
                    }
                }
#endif

                _objNextHelper = objNextHelper;
                _objPreviousTopMostHeldUReader = objPreviousTopMostHeldUReader;
                _objPreviousTopMostHeldWriter = objPreviousTopMostHeldWriter;
                _objReaderWriterLock = objReaderWriterLock;
                _blnSkipUnlockOnDispose = blnSkipUnlockOnDispose;
                _objParentRelease = objParentRelease;
                _objParentReleaseAsync = objParentReleaseAsync;
            }

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));

                DisposeAsyncPre();

                return _blnSkipUnlockOnDispose && _objParentReleaseAsync == null
                    ? _objNextHelper.DisposeAsync()
                    : DisposeCoreAsync();
            }

            public void DisposeAsyncPre()
            {
                // Update _objReaderWriterLock._objAsyncLocalCurrentsContainer in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a
                // copy of the ExecutionContext and the caller won't see the changes.
                _objParentRelease?.Dispose();
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        objCastReleaseWriter.DisposeAsyncPre();
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        objCastReleaseUReader.DisposeAsyncPre();
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        objCastReleaseReader.DisposeAsyncPre();
                        break;
                }
                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader,
                        _objPreviousTopMostHeldWriter, false);
            }

            public async ValueTask DisposeCoreAsync()
            {
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        await objCastReleaseWriter.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        await objCastReleaseUReader.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        await objCastReleaseReader.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                }

                if (_blnSkipUnlockOnDispose)
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                bool blnDoUnlock;
                try
                {
                    blnDoUnlock = objCurrentHelper.ActiveUpgradeableReaderSemaphore.CurrentCount == 0;
                }
                catch
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                try
                {
                    if (blnDoUnlock)
                    {
                        try
                        {
                            await _objNextHelper.TakeSingleWriteLockAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            objCurrentHelper.ReleaseUpgradeableReadLock();
                            throw;
                        }

                        try
                        {
                            objCurrentHelper.ReleaseUpgradeableReadLock();
                        }
                        finally
                        {
                            _objNextHelper.ReleaseSingleWriteLock();
                        }
                    }
                }
                finally
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
                if (_objParentReleaseAsync != null)
                    throw new InvalidOperationException(
                        "Tried to synchronously dispose a lock with a parent that was acquired asynchronously.");
                _objParentRelease?.Dispose();

                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader,
                        _objPreviousTopMostHeldWriter, false);

                bool blnDoUnlock;
                try
                {
                    blnDoUnlock = !_blnSkipUnlockOnDispose &&
                                  objCurrentHelper.ActiveUpgradeableReaderSemaphore.CurrentCount == 0;
                }
                catch
                {
                    _objNextHelper.Dispose();
                    return;
                }

                try
                {
                    if (blnDoUnlock)
                    {
                        try
                        {
                            _objNextHelper.TakeSingleWriteLock();
                        }
                        catch
                        {
                            objCurrentHelper.ReleaseUpgradeableReadLock();
                            throw;
                        }

                        try
                        {
                            objCurrentHelper.ReleaseUpgradeableReadLock();
                        }
                        finally
                        {
                            _objNextHelper.ReleaseSingleWriteLock();
                        }
                    }
                }
                finally
                {
                    _objNextHelper.Dispose();
                }
            }
        }

        private readonly struct SafeWriterSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly LinkedAsyncRWLockHelper _objNextHelper;
            private readonly LinkedAsyncRWLockHelper _objPreviousTopMostHeldUReader;
            private readonly LinkedAsyncRWLockHelper _objPreviousTopMostHeldWriter;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;
            private readonly bool _blnSkipUnlockOnDispose;
            private readonly IDisposable _objParentRelease;
            private readonly IAsyncDisposable _objParentReleaseAsync;

            public SafeWriterSemaphoreRelease(LinkedAsyncRWLockHelper objNextHelper,
                LinkedAsyncRWLockHelper objPreviousTopMostHeldUReader,
                LinkedAsyncRWLockHelper objPreviousTopMostHeldWriter, AsyncFriendlyReaderWriterLock objReaderWriterLock,
                bool blnSkipUnlockOnDispose = false, IDisposable objParentRelease = null, IAsyncDisposable objParentReleaseAsync = null)
            {
                if (objNextHelper == null)
                    throw new ArgumentNullException(nameof(objNextHelper));
#if DEBUG
                LinkedAsyncRWLockHelper objCurrentHelper = objNextHelper.ParentLinkedHelper;
                if (objCurrentHelper != null)
                {
                    if (objCurrentHelper == objNextHelper)
                        throw new InvalidOperationException(
                            "Current and next helpers are identical, this should not happen.");
                    LinkedAsyncRWLockHelper objLastHelper = objCurrentHelper.ParentLinkedHelper;
                    if (objLastHelper != null)
                    {
                        if (objLastHelper == objCurrentHelper)
                            throw new InvalidOperationException(
                                "Last and current helpers are identical, this should not happen.");
                        if (objLastHelper == objNextHelper)
                            throw new InvalidOperationException(
                                "Last and next helpers are identical, this should not happen.");
                    }
                }
#endif

                _objNextHelper = objNextHelper;
                _objPreviousTopMostHeldUReader = objPreviousTopMostHeldUReader;
                _objPreviousTopMostHeldWriter = objPreviousTopMostHeldWriter;
                _objReaderWriterLock = objReaderWriterLock;
                _blnSkipUnlockOnDispose = blnSkipUnlockOnDispose;
                _objParentRelease = objParentRelease;
                _objParentReleaseAsync = objParentReleaseAsync;
            }

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));

                DisposeAsyncPre();

                return _blnSkipUnlockOnDispose && _objParentReleaseAsync == null
                    ? _objNextHelper.DisposeAsync()
                    : DisposeCoreAsync();
            }

            public void DisposeAsyncPre()
            {
                // Update _objReaderWriterLock._objAsyncLocalCurrentsContainer in the calling ExecutionContext
                // and defer any awaits to DisposeCoreAsync(). If this isn't done, the update will happen in a
                // copy of the ExecutionContext and the caller won't see the changes.
                _objParentRelease?.Dispose();
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        objCastReleaseWriter.DisposeAsyncPre();
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        objCastReleaseUReader.DisposeAsyncPre();
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        objCastReleaseReader.DisposeAsyncPre();
                        break;
                }
                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader,
                        _objPreviousTopMostHeldWriter, false);
            }

            public async ValueTask DisposeCoreAsync()
            {
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        await objCastReleaseWriter.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        await objCastReleaseUReader.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        await objCastReleaseReader.DisposeCoreAsync().ConfigureAwait(false);
                        break;
                }

                if (_blnSkipUnlockOnDispose)
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                    return;
                }
                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                bool blnDoUnlock;
                try
                {
                    blnDoUnlock = objCurrentHelper.ActiveWriterSemaphore.CurrentCount == 0;
                }
                catch
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                try
                {
                    if (blnDoUnlock)
                    {
                        try
                        {
                            await _objNextHelper.TakeSingleWriteLockAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            objCurrentHelper.ReleaseWriteLock(_objPreviousTopMostHeldUReader,
                                _objPreviousTopMostHeldWriter);
                            throw;
                        }

                        try
                        {
                            objCurrentHelper.ReleaseWriteLock(_objPreviousTopMostHeldUReader,
                                _objPreviousTopMostHeldWriter);
                        }
                        finally
                        {
                            _objNextHelper.ReleaseSingleWriteLock();
                        }
                    }
                }
                finally
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
                if (_objParentReleaseAsync != null)
                    throw new InvalidOperationException(
                        "Tried to synchronously dispose a lock with a parent that was acquired asynchronously.");
                _objParentRelease?.Dispose();

                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new Tuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, bool>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader,
                        _objPreviousTopMostHeldWriter, false);

                bool blnDoUnlock;
                try
                {
                    blnDoUnlock = !_blnSkipUnlockOnDispose && objCurrentHelper.ActiveWriterSemaphore.CurrentCount == 0;
                }
                catch
                {
                    _objNextHelper.Dispose();
                    return;
                }

                try
                {
                    if (blnDoUnlock)
                    {
                        try
                        {
                            _objNextHelper.TakeSingleWriteLock();
                        }
                        catch
                        {
                            objCurrentHelper.ReleaseWriteLock(_objPreviousTopMostHeldUReader,
                                _objPreviousTopMostHeldWriter);
                            throw;
                        }

                        try
                        {
                            objCurrentHelper.ReleaseWriteLock(_objPreviousTopMostHeldUReader,
                                _objPreviousTopMostHeldWriter);
                        }
                        finally
                        {
                            _objNextHelper.ReleaseSingleWriteLock();
                        }
                    }
                }
                finally
                {
                    _objNextHelper.Dispose();
                }
            }
        }
    }
}
