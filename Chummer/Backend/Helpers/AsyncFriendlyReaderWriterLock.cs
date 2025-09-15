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

#if DEBUG
// Uncomment this define to control whether stacktraces should be saved every time a read lock is acquired (helpful for debugging cases where a non-read lock is attempted to be acquired inside a read lock)
//#define READERLOCKSTACKTRACEDEBUG
// Uncomment this define to control whether stacktraces should be saved to the AsyncLocal every time it is explicitly written to (helpful to try to track down where disposed helpers are staying as part of the local)
//#define ASYNCLOCALWRITEDEBUG
// Uncomment this define if you are having a weird deadlock or crash that you cannot diagnose, which can often be caused by an improperly set or unset AsyncLocal earlier in the code
//#define DEBUGBREAKONIMPROPERLOCALUNSET
#endif

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;

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
    /// IMPORTANT NOTE:
    /// Because of our reliance on AsyncLocal to make this work, we need to be A LOT more careful with any method or call that would create a copy of the ExecutionContext,
    /// because it can end up creating a memory leak. The two most common methods that will do this are Task.Run and CancellationToken.Register.
    /// </summary>
    public sealed class AsyncFriendlyReaderWriterLock : IAsyncDisposable, IDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        // In order to properly allow async lock to be recursive but still make them work properly as locks, we need to set up something
        // that is a bit like a singly-linked list, but as a tree graph. Each lock creates a disposable release object of some kind, and only disposing it frees the lock.
        // Because .NET Framework doesn't have dictionary optimizations for dealing with multiple AsyncLocals stored per context, we need scrape together something similar.
        // Therefore, we store a tuple where the first element is the current helper to be used and the next two elements are the topmost held upgradeable reader and writer helpers.
        // TODO: Revert this cursed bodge once we migrate to a version of .NET that has these AsyncLocal optimizations
#if ASYNCLOCALWRITEDEBUG
        private readonly AsyncLocal<
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>>
            _objAsyncLocalCurrentsContainer =
                new AsyncLocal<ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper,
                    string>>(ValueChangedHandler);

        private static void ValueChangedHandler(AsyncLocalValueChangedArgs<ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>> objArgs)
        {
            // We should never be setting the local to a value where the current helper is disposed, because disposal will always happen after a new value is set
            if (!objArgs.ThreadContextChanged && objArgs.CurrentValue != null && objArgs.CurrentValue.Item1?.IsDisposed == true)
                Utils.BreakIfDebug();
        }
#else
        private readonly AsyncLocal<
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>>
            _objAsyncLocalCurrentsContainer =
                new AsyncLocal<ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>>();
#endif
#if READERLOCKSTACKTRACEDEBUG
        private readonly AsyncLocal<string> _objIsInReadLockContainer = new AsyncLocal<string>();
#else
        private readonly AsyncLocal<bool> _objIsInReadLockContainer = new AsyncLocal<bool>();
#endif

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

#if READERLOCKSTACKTRACEDEBUG
        public bool IsInNonUpgradeableReadLock => !string.IsNullOrEmpty(_objIsInReadLockContainer.Value);
#else
        public bool IsInNonUpgradeableReadLock => _objIsInReadLockContainer.Value;
#endif

        public bool IsInUpgradeableReadLock => _objAsyncLocalCurrentsContainer.Value.Item2 != null;

        public bool IsInReadLock => IsInNonUpgradeableReadLock || IsInUpgradeableReadLock;

        public bool IsInWriteLock => _objAsyncLocalCurrentsContainer.Value.Item3 != null;

        public bool IsInPotentialWriteLock
        {
            get
            {
                if (IsInNonUpgradeableReadLock)
                    return false;
#if ASYNCLOCALWRITEDEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
#else
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
#endif
                    objAsyncLocals =
                        _objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default)
                {
                    return objAsyncLocals.Item2 != null || objAsyncLocals.Item3 != null;
                }
                return false;
            }
        }

        private LinkedAsyncRWLockHelper GetReadLockHelper(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            LinkedAsyncRWLockHelper objCurrentHelper = _objTopLevelHelper;
#if ASYNCLOCALWRITEDEBUG
            ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
#else
            ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
#endif
                objAsyncLocals =
                    _objAsyncLocalCurrentsContainer.Value;
            if (objAsyncLocals != default)
                objCurrentHelper = objAsyncLocals.Item1;
            return objCurrentHelper;
        }

        private ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper,
                    LinkedAsyncRWLockHelper> GetHelpers(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            LinkedAsyncRWLockHelper objCurrentHelper = _objTopLevelHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader = null;
            LinkedAsyncRWLockHelper objTopMostHeldWriter = null;
            LinkedAsyncRWLockHelper objNextHelper;
#if ASYNCLOCALWRITEDEBUG
            string strLastWriteStacktrace = string.Empty;
#endif
            // Loop is a hacky fix for weird cases where another locker changes our AsyncLocal semaphores in between us obtaining them and us checking them
            int intLoopCount = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                {
                    // Emergency exit for odd cases where, for some reason, AsyncLocal assignment does not happen (in the right place?) when a locker release is disposed
                    // Let's just get the first ancestor lock that is not disposed. If this causes problems, it's because of the above-mentioned comment around AsyncLocal assignment
#if DEBUGBREAKONIMPROPERLOCALUNSET
                    // If you are breaking here because of a mysterious crash or deadlock you cannot find the source of, check for the following:
                    // - Synchronous upgradeable read or write locks acquired inside a scope where some locks are acquired asynchronously. These won't always cause issues, but often will. Remember that using a foreach on a locking collection will acquire a lock synchronously (normally a non-upgradeable read lock, upgradeable read lock if already inside an upgradeable read lock or write lock).
                    // - Disposal/unsetting of a locker (to release a lock) that is not in the same scope where it was set/created. Being forced to dispose lockers in the same scope is not ideal, but necessary to make sure the AsyncLocals that power this entire system update themselves properly.
                    // - Forgetting to dispose/unset a locker that has been set. If it is an asynchronous locker, it needs to have a try-finally disposal immediately after it is set, even if (and especially if) the next line is cancellation token check, otherwise AsyncLocals won't update themselves properly even after a cancellation.
                    Utils.BreakIfDebug();
#elif DEBUG
                    Log.Warn("Ran into an improperly set AsyncLocal that needs to be reset, location: " + Environment.NewLine + EnhancedStackTrace.Current());
#endif
                    while (objCurrentHelper != null && objCurrentHelper.IsDisposed)
                        objCurrentHelper = objCurrentHelper.ParentLinkedHelper;
                    // Held helpers need to actually be held at the moment, otherwise we have a problem
                    while (objTopMostHeldUReader != null && (objTopMostHeldUReader.IsDisposed
                                                             || objTopMostHeldUReader.ActiveUpgradeableReaderSemaphore
                                                                 .CurrentCount != 0))
                        objTopMostHeldUReader = objTopMostHeldUReader.ParentLinkedHelper;
                    while (objTopMostHeldWriter != null && (objTopMostHeldWriter.IsDisposed
                                                            || objTopMostHeldWriter.ActiveUpgradeableReaderSemaphore
                                                                .CurrentCount != 0
                                                            || objTopMostHeldWriter.ActiveWriterSemaphore
                                                                .CurrentCount != 0))
                        objTopMostHeldWriter = objTopMostHeldWriter.ParentLinkedHelper;

#if ASYNCLOCALWRITEDEBUG
                    string strStackTrace;
#endif
                    _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                        new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, strStackTrace = EnhancedStackTrace.Current().ToString());
                    strLastWriteStacktrace = strStackTrace;
#else
                        new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter);
#endif
                }
                else
                {
#if ASYNCLOCALWRITEDEBUG
                    ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
#else
                    ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
#endif
                        objAsyncLocals = _objAsyncLocalCurrentsContainer.Value;
                    if (objAsyncLocals != default)
                    {
#if ASYNCLOCALWRITEDEBUG
                        (objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, strLastWriteStacktrace)
#else
                        (objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter)
#endif
                            = objAsyncLocals;
                    }
                    else
                    {
                        objCurrentHelper = _objTopLevelHelper;
                        objTopMostHeldUReader = null;
                        objTopMostHeldWriter = null;
                    }
                }

                if (objCurrentHelper?.IsDisposed != false)
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
            return new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper,
                LinkedAsyncRWLockHelper>(objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                objTopMostHeldWriter);
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
#if READERLOCKSTACKTRACEDEBUG
            string strReadLockStacktrace = _objIsInReadLockContainer.Value;
            bool blnIsInReadLock = !string.IsNullOrEmpty(strReadLockStacktrace);
#else
            bool blnIsInReadLock = _objIsInReadLockContainer.Value;
#endif
            if (blnIsInReadLock)
                throw new InvalidOperationException(
                    "Attempted to take a write lock while inside of a non-upgradeable read lock.");

            (LinkedAsyncRWLockHelper objCurrentHelper, LinkedAsyncRWLockHelper objNextHelper,
                    LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter) =
                GetHelpers(token: token);
            try
            {
                objCurrentHelper.TakeWriteLock(objTopMostHeldWriter, token);
                try
                {
                    _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                        new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                            objNextHelper, objTopMostHeldUReader, objCurrentHelper, EnhancedStackTrace.Current().ToString());
#else
                        new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                            objNextHelper, objTopMostHeldUReader, objCurrentHelper);
#endif
                    try
                    {
                        if (_objParentLock == null)
                            return new SafeWriterSemaphoreRelease(objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter, this);
                        IDisposable objParentRelease = _blnLockReadOnlyForParent
                            ? _objParentLock.EnterReadLock(token)
                            : _objParentLock.EnterUpgradeableReadLock(token);
                        try
                        {
                            return new SafeWriterSemaphoreRelease(objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter, this,
                                objParentRelease: objParentRelease);
                        }
                        catch
                        {
                            objParentRelease.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                            new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                                objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
                            new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                                objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter);
#endif
                        throw;
                    }
                }
                catch
                {
                    objCurrentHelper.ReleaseWriteLock(objTopMostHeldUReader, objTopMostHeldWriter);
                    throw;
                }
            }
            catch
            {
                objNextHelper.Dispose();
                throw;
            }
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
                return Task.FromCanceled<IAsyncDisposable>(token);
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(
                    new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));

#if READERLOCKSTACKTRACEDEBUG
            string strReadLockStacktrace = _objIsInReadLockContainer.Value;
            bool blnIsInReadLock = !string.IsNullOrEmpty(strReadLockStacktrace);
#else
            bool blnIsInReadLock = _objIsInReadLockContainer.Value;
#endif
            if (blnIsInReadLock)
            {
                return Task.FromException<IAsyncDisposable>(
#if READERLOCKSTACKTRACEDEBUG
                    new InvalidOperationException(
                        "Attempted to take a write lock while inside of a non-upgradeable read lock. Read lock stacktrace:" +
                        Environment.NewLine + strReadLockStacktrace));
#else
                    new InvalidOperationException(
                        "Attempted to take a write lock while inside of a non-upgradeable read lock."));
#endif
            }

            LinkedAsyncRWLockHelper objCurrentHelper;
            LinkedAsyncRWLockHelper objNextHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader;
            LinkedAsyncRWLockHelper objTopMostHeldWriter;
            try
            {
                (objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter) =
                    GetHelpers(token);
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled<IAsyncDisposable>(e.CancellationToken);
            }
            catch (Exception e)
            {
                Utils.BreakIfDebug();
                return Task.FromException<IAsyncDisposable>(e);
            }

            _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                    objNextHelper, objTopMostHeldUReader, objCurrentHelper, EnhancedStackTrace.Current().ToString());
#else
                new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                    objNextHelper, objTopMostHeldUReader, objCurrentHelper);
#endif

            if (_objParentLock != null)
            {
                // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
                Task<IAsyncDisposable> tskParent = _blnLockReadOnlyForParent
                    ? _objParentLock.EnterReadLockAsync(token)
                    : _objParentLock.EnterUpgradeableReadLockAsync(token);
                return InnerAsync(objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                    tskParent, token);
                async Task<IAsyncDisposable> InnerAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                    LinkedAsyncRWLockHelper objInnerNextHelper,
                    LinkedAsyncRWLockHelper objInnerTopMostHeldUReader,
                    LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                    Task<IAsyncDisposable> tskInnerParent,
                    CancellationToken innerToken = default)
                {
                    IAsyncDisposable objParentRelease = null;
                    try
                    {
                        objParentRelease = await tskInnerParent.ConfigureAwait(false);
                    }
#if DEBUG
                    catch (Exception e)
                    {
                        // swallow all exceptions because we need to be able to properly unset AsyncLocals when we release
                        if (!(e is OperationCanceledException))
                            Utils.BreakIfDebug();
                    }
#else
                    catch
                    {
                        // swallow all exceptions because we need to be able to properly unset AsyncLocals when we release
                    }
#endif
                    return await TakeWriteLockCoreAsync(
                        objInnerCurrentHelper, objInnerNextHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, objParentRelease, innerToken).ConfigureAwait(false);
                }
            }

            return TakeWriteLockCoreAsync(objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                innerToken: token);

            async Task<IAsyncDisposable> TakeWriteLockCoreAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                LinkedAsyncRWLockHelper objInnerNextHelper,
                LinkedAsyncRWLockHelper objInnerTopMostHeldUReader, LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                IAsyncDisposable objParentReleaseAsync = null, CancellationToken innerToken = default)
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
                        objInnerTopMostHeldWriter, this, true, objParentReleaseAsync: objParentReleaseAsync);
                }

                return new SafeWriterSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                    objInnerTopMostHeldWriter, this, objParentReleaseAsync: objParentReleaseAsync);
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
#if READERLOCKSTACKTRACEDEBUG
            string strReadLockStacktrace = _objIsInReadLockContainer.Value;
            bool blnIsInReadLock = !string.IsNullOrEmpty(strReadLockStacktrace);
#else
            bool blnIsInReadLock = _objIsInReadLockContainer.Value;
#endif
            if (blnIsInReadLock)
                throw new InvalidOperationException(
                    "Attempted to take an upgradeable read lock while inside of a non-upgradeable read lock.");

            (LinkedAsyncRWLockHelper objCurrentHelper, LinkedAsyncRWLockHelper objNextHelper,
                    LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter) =
                GetHelpers(token: token);

            try
            {
                objCurrentHelper.TakeUpgradeableReadLock(token);
                try
                {
                    _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                        new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                            objNextHelper, objCurrentHelper, objTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
                        new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                            objNextHelper, objCurrentHelper, objTopMostHeldWriter);
#endif
                    try
                    {
                        if (_objParentLock == null)
                            return new SafeUpgradeableReaderSemaphoreRelease(objNextHelper, objTopMostHeldUReader,
                                objTopMostHeldWriter, this);
                        IDisposable objParentRelease = _blnLockReadOnlyForParent
                            ? _objParentLock.EnterReadLock(token)
                            : _objParentLock.EnterUpgradeableReadLock(token);
                        try
                        {
                            return new SafeUpgradeableReaderSemaphoreRelease(objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                                this, objParentRelease: objParentRelease);
                        }
                        catch
                        {
                            objParentRelease.Dispose();
                            throw;
                        }
                    }
                    catch
                    {
                        _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                            new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                                objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
                            new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                                objCurrentHelper, objTopMostHeldUReader, objTopMostHeldWriter);
#endif
                        throw;
                    }
                }
                catch
                {
                    objCurrentHelper.ReleaseUpgradeableReadLock();
                    throw;
                }
            }
            catch
            {
                objNextHelper.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Try to asynchronously obtain a lock for reading (that can be upgraded to a write lock) and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// </summary>
        public Task<IAsyncDisposable> EnterUpgradeableReadLockAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<IAsyncDisposable>(token);
            if (_intDisposedStatus != 0)
                return Task.FromException<IAsyncDisposable>(
                    new ObjectDisposedException(nameof(AsyncFriendlyReaderWriterLock)));

#if READERLOCKSTACKTRACEDEBUG
            string strReadLockStacktrace = _objIsInReadLockContainer.Value;
            bool blnIsInReadLock = !string.IsNullOrEmpty(strReadLockStacktrace);
#else
            bool blnIsInReadLock = _objIsInReadLockContainer.Value;
#endif
            if (blnIsInReadLock)
            {
                return Task.FromException<IAsyncDisposable>(
#if READERLOCKSTACKTRACEDEBUG
                    new InvalidOperationException(
                        "Attempted to take an upgradeable read lock while inside of a non-upgradeable read lock. Read lock stacktrace:" +
                        Environment.NewLine + strReadLockStacktrace));
#else
                    new InvalidOperationException(
                        "Attempted to take an upgradeable read lock while inside of a non-upgradeable read lock."));
#endif
            }

            LinkedAsyncRWLockHelper objCurrentHelper;
            LinkedAsyncRWLockHelper objNextHelper;
            LinkedAsyncRWLockHelper objTopMostHeldUReader;
            LinkedAsyncRWLockHelper objTopMostHeldWriter;
            try
            {
                (objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter) =
                    GetHelpers(token);
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled<IAsyncDisposable>(e.CancellationToken);
            }
            catch (Exception e)
            {
                Utils.BreakIfDebug();
                return Task.FromException<IAsyncDisposable>(e);
            }

            _objAsyncLocalCurrentsContainer.Value =
#if ASYNCLOCALWRITEDEBUG
                new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                    objNextHelper, objCurrentHelper, objTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
                new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                    objNextHelper, objCurrentHelper, objTopMostHeldWriter);
#endif

            if (_objParentLock != null)
            {
                // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
                Task<IAsyncDisposable> tskParent = _blnLockReadOnlyForParent
                    ? _objParentLock.EnterReadLockAsync(token)
                    : _objParentLock.EnterUpgradeableReadLockAsync(token);
                return InnerAsync(objCurrentHelper, objNextHelper, objTopMostHeldUReader, objTopMostHeldWriter,
                    tskParent, token);
                async Task<IAsyncDisposable> InnerAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                    LinkedAsyncRWLockHelper objInnerNextHelper,
                    LinkedAsyncRWLockHelper objInnerTopMostHeldUReader,
                    LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                    Task<IAsyncDisposable> tskInnerParent,
                    CancellationToken innerToken = default)
                {
                    IAsyncDisposable objParentRelease = null;
                    try
                    {
                        objParentRelease = await tskInnerParent.ConfigureAwait(false);
                    }
#if DEBUG
                    catch (Exception e)
                    {
                        // swallow all exceptions because we need to be able to properly unset AsyncLocals when we release
                        if (!(e is OperationCanceledException))
                            Utils.BreakIfDebug();
                    }
#else
                    catch
                    {
                        // swallow all exceptions because we need to be able to properly unset AsyncLocals when we release
                    }
#endif
                    return await TakeUpgradeableReadLockCoreAsync(
                        objInnerCurrentHelper, objInnerNextHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, objParentRelease, innerToken).ConfigureAwait(false);
                }
            }

            return TakeUpgradeableReadLockCoreAsync(objCurrentHelper, objNextHelper, objTopMostHeldUReader,
                objTopMostHeldWriter, innerToken: token);

            async Task<IAsyncDisposable> TakeUpgradeableReadLockCoreAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                LinkedAsyncRWLockHelper objInnerNextHelper,
                LinkedAsyncRWLockHelper objInnerTopMostHeldUReader, LinkedAsyncRWLockHelper objInnerTopMostHeldWriter,
                IAsyncDisposable objParentReleaseAsync = null, CancellationToken innerToken = default)
            {
                try
                {
                    await objInnerCurrentHelper.TakeUpgradeableReadLockAsync(innerToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this because unsetting the AsyncLocal must be handled as a disposal in the original ExecutionContext
                    return new SafeUpgradeableReaderSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                        objInnerTopMostHeldWriter, this, true, objParentReleaseAsync: objParentReleaseAsync);
                }

                return new SafeUpgradeableReaderSemaphoreRelease(objInnerNextHelper, objInnerTopMostHeldUReader,
                    objInnerTopMostHeldWriter, this, objParentReleaseAsync: objParentReleaseAsync);
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

        /// <summary>
        /// Try to synchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// This version will set the lock's parent to an upgradeable read lock instead of a non-upgradeable one if the lock's parent is in a write lock or potential write lock.
        /// </summary>
        public IDisposable EnterReadLockWithMatchingParentLock(CancellationToken token = default)
        {
            return EnterReadLock(_objParentLock?.IsInPotentialWriteLock == true, token);
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
                return new DisposedReaderDummySemaphoreRelease(this);
            }

            token.ThrowIfCancellationRequested();

#if READERLOCKSTACKTRACEDEBUG
            string strReadLockStacktrace = _objIsInReadLockContainer.Value;
            bool blnIsInReadLock = !string.IsNullOrEmpty(strReadLockStacktrace);
#else
            bool blnIsInReadLock = _objIsInReadLockContainer.Value;
#endif

            LinkedAsyncRWLockHelper objCurrentHelper = GetReadLockHelper(token);

            if (objCurrentHelper.IsDisposed)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif

                if (!blnIsInReadLock)
                {
#if READERLOCKSTACKTRACEDEBUG
                    _objIsInReadLockContainer.Value = EnhancedStackTrace.Current().ToString();
#else
                    _objIsInReadLockContainer.Value = true;
#endif
                }

                try
                {
                    if (_objParentLock == null)
                        return new SafeReaderSemaphoreRelease(objCurrentHelper, blnIsInReadLock, this, true);
                    IDisposable objParentRelease2 = !_blnLockReadOnlyForParent && blnParentLockIsUpgradeable
                        ? _objParentLock.EnterUpgradeableReadLock(token)
                        : _objParentLock.EnterReadLock(token);
                    try
                    {
                        return new SafeReaderSemaphoreRelease(objCurrentHelper, blnIsInReadLock, this, objCurrentHelper.IsDisposed, objParentRelease2);
                    }
                    catch
                    {
                        objParentRelease2.Dispose();
                        throw;
                    }
                }
                catch
                {
                    if (!blnIsInReadLock)
                    {
#if READERLOCKSTACKTRACEDEBUG
                        _objIsInReadLockContainer.Value = string.Empty;
#else
                        _objIsInReadLockContainer.Value = false;
#endif
                    }
                    throw;
                }
            }

            objCurrentHelper.TakeReadLock(blnIsInReadLock, token);

            try
            {
                if (!blnIsInReadLock)
                {
#if READERLOCKSTACKTRACEDEBUG
                    _objIsInReadLockContainer.Value = EnhancedStackTrace.Current().ToString();
#else
                    _objIsInReadLockContainer.Value = true;
#endif
                }

                try
                {
                    if (_objParentLock == null)
                        return new SafeReaderSemaphoreRelease(objCurrentHelper, blnIsInReadLock, this, objCurrentHelper.IsDisposed);
                    IDisposable objParentRelease = !_blnLockReadOnlyForParent && blnParentLockIsUpgradeable
                        ? _objParentLock.EnterUpgradeableReadLock(token)
                        : _objParentLock.EnterReadLock(token);
                    try
                    {
                        return new SafeReaderSemaphoreRelease(objCurrentHelper, blnIsInReadLock, this, objCurrentHelper.IsDisposed, objParentRelease);
                    }
                    catch
                    {
                        objParentRelease.Dispose();
                        throw;
                    }
                }
                catch
                {
                    if (!blnIsInReadLock)
                    {
#if READERLOCKSTACKTRACEDEBUG
                        _objIsInReadLockContainer.Value = string.Empty;
#else
                        _objIsInReadLockContainer.Value = false;
#endif
                    }
                    throw;
                }
            }
            catch
            {
                objCurrentHelper.ReleaseReadLock();
                throw;
            }
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

        /// <summary>
        /// Try to asynchronously obtain a lock for reading and only reading and return a disposable that exits the read lock when disposed.
        /// NOTE: Ensure that you are separately handling OperationCanceledException in the calling context and disposing of this result if the token is canceled!
        /// This version will set the lock's parent to an upgradeable read lock instead of a non-upgradeable one if the lock's parent is in a write lock or potential write lock.
        /// </summary>
        public Task<IAsyncDisposable> EnterReadLockWithMatchingParentLockAsync(CancellationToken token = default)
        {
            return EnterReadLockAsync(_objParentLock?.IsInPotentialWriteLock == true, token);
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
                return Task.FromResult<IAsyncDisposable>(new DisposedReaderDummySemaphoreRelease(this));
            }

            if (token.IsCancellationRequested)
                return Task.FromCanceled<IAsyncDisposable>(token);

#if READERLOCKSTACKTRACEDEBUG
            string strReadLockStacktrace = _objIsInReadLockContainer.Value;
            bool blnIsInReadLock = !string.IsNullOrEmpty(strReadLockStacktrace);
#else
            bool blnIsInReadLock = _objIsInReadLockContainer.Value;
#endif

            LinkedAsyncRWLockHelper objCurrentHelper;
            try
            {
                objCurrentHelper = GetReadLockHelper(token);
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled<IAsyncDisposable>(e.CancellationToken);
            }
            catch (Exception e)
            {
                Utils.BreakIfDebug();
                return Task.FromException<IAsyncDisposable>(e);
            }

            if (!blnIsInReadLock)
            {
#if READERLOCKSTACKTRACEDEBUG
                _objIsInReadLockContainer.Value = EnhancedStackTrace.Current().ToString();
#else
                _objIsInReadLockContainer.Value = true;
#endif
            }

            if (_objParentLock != null)
            {
                // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
                Task<IAsyncDisposable> tskParent = !_blnLockReadOnlyForParent && blnParentLockIsUpgradeable
                    ? _objParentLock.EnterUpgradeableReadLockAsync(token)
                    : _objParentLock.EnterReadLockAsync(token);
                return InnerAsync(objCurrentHelper, blnIsInReadLock,
                    tskParent, token);
                async Task<IAsyncDisposable> InnerAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                    bool blnInnerIsInReadLock, Task<IAsyncDisposable> tskInnerParent,
                    CancellationToken innerToken = default)
                {
                    IAsyncDisposable objParentRelease = null;
                    try
                    {
                        objParentRelease = await tskInnerParent.ConfigureAwait(false);
                    }
#if DEBUG
                    catch (Exception e)
                    {
                        // swallow all exceptions because we need to be able to properly unset AsyncLocals when we release
                        if (!(e is OperationCanceledException))
                            Utils.BreakIfDebug();
                    }
#else
                    catch
                    {
                        // swallow all exceptions because we need to be able to properly unset AsyncLocals when we release
                    }
#endif
                    return await TakeReadLockCoreAsync(
                        objInnerCurrentHelper, blnInnerIsInReadLock, objParentRelease, innerToken).ConfigureAwait(false);
                }
            }

            return TakeReadLockCoreAsync(objCurrentHelper, blnIsInReadLock, innerToken: token);

            async Task<IAsyncDisposable> TakeReadLockCoreAsync(LinkedAsyncRWLockHelper objInnerCurrentHelper,
                bool blnInnerIsInReadLock, IAsyncDisposable objParentRelease = null, CancellationToken innerToken = default)
            {
                if (_intDisposedStatus != 0 || objInnerCurrentHelper.IsDisposed)
                {
#if DEBUG
                    Debug.WriteLine(
                        "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                    Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                    return new SafeReaderSemaphoreRelease(objInnerCurrentHelper, blnInnerIsInReadLock, this, true, objParentReleaseAsync: objParentRelease);
                }

                try
                {
                    await objInnerCurrentHelper.TakeReadLockAsync(blnInnerIsInReadLock, innerToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this because unsetting the AsyncLocal must be handled as a disposal in the original ExecutionContext
                    return new SafeReaderSemaphoreRelease(objInnerCurrentHelper, blnInnerIsInReadLock, this, true, objParentReleaseAsync: objParentRelease);
                }

                return new SafeReaderSemaphoreRelease(objInnerCurrentHelper, blnInnerIsInReadLock, this, objInnerCurrentHelper.IsDisposed, objParentReleaseAsync: objParentRelease);
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

        private readonly struct DisposedReaderDummySemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;

            public DisposedReaderDummySemaphoreRelease(AsyncFriendlyReaderWriterLock objReaderWriterLock)
            {
                if (objReaderWriterLock != null && objReaderWriterLock._intDisposedStatus == 0)
                    throw new InvalidOperationException("Cannot assign dummy release to a non-disposed lock");
                _objReaderWriterLock = objReaderWriterLock;
            }

            public void Dispose()
            {
                if (_objReaderWriterLock != null && _objReaderWriterLock._intDisposedStatus == 0)
                    throw new InvalidOperationException("Cannot assign dummy release to a non-disposed lock");
            }

            public ValueTask DisposeAsync()
            {
                if (_objReaderWriterLock != null && _objReaderWriterLock._intDisposedStatus == 0)
                    throw new InvalidOperationException("Cannot assign dummy release to a non-disposed lock");
                return default;
            }
        }

        private readonly struct SafeReaderSemaphoreRelease : IDisposable, IAsyncDisposable
        {
            private readonly LinkedAsyncRWLockHelper _objCurrentHelper;
            private readonly bool _blnOldIsInReadLock;
            private readonly AsyncFriendlyReaderWriterLock _objReaderWriterLock;
            private readonly bool _blnSkipUnlockOnDispose;
            private readonly IDisposable _objParentRelease;
            private readonly IAsyncDisposable _objParentReleaseAsync;

            public SafeReaderSemaphoreRelease(LinkedAsyncRWLockHelper objCurrentHelper,
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
                _blnOldIsInReadLock = blnIsInReadLock;
                _objReaderWriterLock = objReaderWriterLock;
                _blnSkipUnlockOnDispose = blnSkipUnlockOnDispose;
                _objParentRelease = objParentRelease;
                _objParentReleaseAsync = objParentReleaseAsync;
            }

            public void Dispose()
            {
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

                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        Utils.SafelyRunSynchronously(() => objCastReleaseWriter.DisposeCoreAsync().AsTask());
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        Utils.SafelyRunSynchronously(() => objCastReleaseUReader.DisposeCoreAsync().AsTask());
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        Utils.SafelyRunSynchronously(() => objCastReleaseReader.DisposeCoreAsync().AsTask());
                        break;
                }

                if (!_blnOldIsInReadLock)
                {
#if READERLOCKSTACKTRACEDEBUG
                    _objReaderWriterLock._objIsInReadLockContainer.Value = string.Empty;
#else
                    _objReaderWriterLock._objIsInReadLockContainer.Value = false;
#endif
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

                if (_objReaderWriterLock._intDisposedStatus > 1 || _blnOldIsInReadLock)
                    return;
#if READERLOCKSTACKTRACEDEBUG
                _objReaderWriterLock._objIsInReadLockContainer.Value = string.Empty;
#else
                _objReaderWriterLock._objIsInReadLockContainer.Value = false;
#endif
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
                DisposeAsyncPre();

                return _blnSkipUnlockOnDispose && _objParentReleaseAsync == null
                    ? _objNextHelper.DisposeAsync()
                    : DisposeCoreAsync();
            }

            public void DisposeAsyncPre()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));

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

#if ASYNCLOCALWRITEDEBUG
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                        _objNextHelper.ParentLinkedHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                        _objNextHelper.ParentLinkedHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter);
#endif
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

#if DEBUG
#if ASYNCLOCALWRITEDEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
#else
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
#endif
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default && objAsyncLocals.Item1 != _objNextHelper.ParentLinkedHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif

                if (_blnSkipUnlockOnDispose)
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                bool blnDoUnlock;
                try
                {
                    blnDoUnlock = objCurrentHelper != null && objCurrentHelper.ActiveUpgradeableReaderSemaphore.CurrentCount == 0;
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
                _objParentRelease?.Dispose();
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        objCastReleaseWriter.DisposeAsyncPre();
                        Utils.SafelyRunSynchronously(() => objCastReleaseWriter.DisposeCoreAsync().AsTask());
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        objCastReleaseUReader.DisposeAsyncPre();
                        Utils.SafelyRunSynchronously(() => objCastReleaseUReader.DisposeCoreAsync().AsTask());
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        objCastReleaseReader.DisposeAsyncPre();
                        Utils.SafelyRunSynchronously(() => objCastReleaseReader.DisposeCoreAsync().AsTask());
                        break;
                }

                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
#if ASYNCLOCALWRITEDEBUG
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter);
#endif

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
                DisposeAsyncPre();

                return _blnSkipUnlockOnDispose && _objParentReleaseAsync == null
                    ? _objNextHelper.DisposeAsync()
                    : DisposeCoreAsync();
            }

            public void DisposeAsyncPre()
            {
                if (_objReaderWriterLock._intDisposedStatus > 1)
                    throw new ObjectDisposedException(nameof(_objReaderWriterLock));
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

#if ASYNCLOCALWRITEDEBUG
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                        _objNextHelper.ParentLinkedHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                        _objNextHelper.ParentLinkedHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter);
#endif
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

#if DEBUG
#if ASYNCLOCALWRITEDEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
#else
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
#endif
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default && objAsyncLocals.Item1 != _objNextHelper.ParentLinkedHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif

                if (_blnSkipUnlockOnDispose)
                {
                    await _objNextHelper.DisposeAsync().ConfigureAwait(false);
                    return;
                }
                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
                bool blnDoUnlock;
                try
                {
                    blnDoUnlock = objCurrentHelper != null && objCurrentHelper.ActiveWriterSemaphore.CurrentCount == 0;
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
                _objParentRelease?.Dispose();
                switch (_objParentReleaseAsync)
                {
                    case SafeWriterSemaphoreRelease objCastReleaseWriter:
                        objCastReleaseWriter.DisposeAsyncPre();
                        Utils.SafelyRunSynchronously(() => objCastReleaseWriter.DisposeCoreAsync().AsTask());
                        break;
                    case SafeUpgradeableReaderSemaphoreRelease objCastReleaseUReader:
                        objCastReleaseUReader.DisposeAsyncPre();
                        Utils.SafelyRunSynchronously(() => objCastReleaseUReader.DisposeCoreAsync().AsTask());
                        break;
                    case SafeReaderSemaphoreRelease objCastReleaseReader:
                        objCastReleaseReader.DisposeAsyncPre();
                        Utils.SafelyRunSynchronously(() => objCastReleaseReader.DisposeCoreAsync().AsTask());
                        break;
                }

                LinkedAsyncRWLockHelper objCurrentHelper = _objNextHelper.ParentLinkedHelper;
#if ASYNCLOCALWRITEDEBUG
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != null && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, string>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter, EnhancedStackTrace.Current().ToString());
#else
#if DEBUG
                ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>
                    objAsyncLocals = _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value;
                if (objAsyncLocals != default && objAsyncLocals.Item1 != _objNextHelper)
                {
                    Utils.BreakIfDebug();
                }
#endif
                _objReaderWriterLock._objAsyncLocalCurrentsContainer.Value =
                    new ValueTuple<LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper, LinkedAsyncRWLockHelper>(
                        objCurrentHelper, _objPreviousTopMostHeldUReader, _objPreviousTopMostHeldWriter);
#endif

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
