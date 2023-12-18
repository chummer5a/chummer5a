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

// Uncomment this define to control whether stacktraces should be saved every time a linked semaphore is successfully disposed.
#if DEBUG
//#define LINKEDSEMAPHOREDEBUG
#endif

using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public sealed class LinkedAsyncRWLockHelper : IDisposable, IAsyncDisposable
    {
        private int _intDisposedStatus;
        private readonly bool _blnSemaphoreIsPooled;
        private const long MaxReaderCount = long.MaxValue >> 2;
        private DebuggableSemaphoreSlim _objWriterCancellationSemaphore; // We need this semaphore to prevent race conditions during cancellations
        private DebuggableSemaphoreSlim _objPendingWriterSemaphore;
        private DebuggableSemaphoreSlim _objUpgradeableReaderSemaphore;
        private DebuggableSemaphoreSlim _objReaderSemaphore;
        private long _lngNumReaders;
        private long _lngPendingCountForWriter;

        private LinkedAsyncRWLockHelper _objParentLinkedHelper;
        private readonly CancellationTokenSource _objDisposalTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objDisposalToken;

        public DebuggableSemaphoreSlim PendingWriterSemaphore => _objPendingWriterSemaphore;
        public DebuggableSemaphoreSlim UpgradeableReaderSemaphore => _objUpgradeableReaderSemaphore;
        public DebuggableSemaphoreSlim ReaderSemaphore => _objReaderSemaphore;

        public LinkedAsyncRWLockHelper ParentLinkedHelper => _objParentLinkedHelper;

        private static readonly SafeObjectPool<Stack<LinkedAsyncRWLockHelper>> s_objHelperStackPool =
            new SafeObjectPool<Stack<LinkedAsyncRWLockHelper>>(() => new Stack<LinkedAsyncRWLockHelper>(8), x => x.Clear());

        private static readonly SafeObjectPool<Stack<DebuggableSemaphoreSlim>> s_objSemaphoreStackPool =
            new SafeObjectPool<Stack<DebuggableSemaphoreSlim>>(() => new Stack<DebuggableSemaphoreSlim>(8), x => x.Clear());

        private static readonly SafeObjectPool<Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>
            s_objWriterLockHelperStackPool =
                new SafeObjectPool<Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>(() =>
                    new Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>(8), x => x.Clear());

        public bool IsDisposed => _intDisposedStatus > 0;

        public LinkedAsyncRWLockHelper(LinkedAsyncRWLockHelper objParent, bool blnGetFromPool = true)
        {
            _objDisposalToken = _objDisposalTokenSource.Token;
            if (objParent?.IsDisposed == false)
            {
                try
                {
#if LINKEDSEMAPHOREDEBUG
                    objParent.AddChild(this);
#else
                    objParent.AddChild();
#endif
                }
                catch (ObjectDisposedException)
                {
                    objParent = null;
                }
            }
            else
                objParent = null;
            if (blnGetFromPool)
            {
                _blnSemaphoreIsPooled = true;
                LinkedAsyncRWLockHelper objGrandParent = objParent?.ParentLinkedHelper;

                _objWriterCancellationSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objPendingWriterSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objUpgradeableReaderSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objReaderSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objHasChildrenSemaphore = safelyGetNewPooledSemaphoreSlim();

                DebuggableSemaphoreSlim safelyGetNewPooledSemaphoreSlim()
                {
                    DebuggableSemaphoreSlim objNewSemaphore = Utils.SemaphorePool.Get();
                    // Extremely hacky solution to buggy semaphore (re)cycling in AsyncLocal
                    // TODO: Fix this properly. The problem is that after an AsyncLocal shallow-copy in a different context, the semaphores can get returned in the copy without altering the original AsyncLocal
                    // This problem happens when the UI thread is safe-waiting on a semaphore and then gets an Application.DoEvents call (from a Utils.RunWithoutThreadLock) that includes a semaphore release.
                    // The ideal solution *should* be to refactor the entire codebase so that those kinds of situations can't happen in the first place, but that requires monstrous effort, and I'm too tired to fix that properly.
                    if (objParent != null)
                    {
                        if (objGrandParent != null)
                        {
                            while (objNewSemaphore == objParent.PendingWriterSemaphore
                                   || objNewSemaphore == objParent.UpgradeableReaderSemaphore
                                   || objNewSemaphore == objParent.ReaderSemaphore
                                   || objNewSemaphore == objGrandParent.PendingWriterSemaphore
                                   || objNewSemaphore == objGrandParent.UpgradeableReaderSemaphore
                                   || objNewSemaphore == objGrandParent.ReaderSemaphore)
                            {
                                objNewSemaphore = Utils.SemaphorePool.Get();
                            }
                        }
                        else
                        {
                            while (objNewSemaphore == objParent.PendingWriterSemaphore
                                   || objNewSemaphore == objParent.UpgradeableReaderSemaphore
                                   || objNewSemaphore == objParent.ReaderSemaphore)
                            {
                                objNewSemaphore = Utils.SemaphorePool.Get();
                            }
                        }
                    }

                    return objNewSemaphore;
                }
            }
            else
            {
                _objWriterCancellationSemaphore = new DebuggableSemaphoreSlim();
                _objPendingWriterSemaphore = new DebuggableSemaphoreSlim();
                _objUpgradeableReaderSemaphore = new DebuggableSemaphoreSlim();
                _objReaderSemaphore = new DebuggableSemaphoreSlim();
                _objHasChildrenSemaphore = new DebuggableSemaphoreSlim();
            }

            _objParentLinkedHelper = objParent;

#if LINKEDSEMAPHOREDEBUG
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();
#endif
        }

        private int _intNumChildren;

        private DebuggableSemaphoreSlim _objHasChildrenSemaphore;

#if LINKEDSEMAPHOREDEBUG
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string RecordedStackTrace { get; set; }

        private readonly ConcurrentHashSet<LinkedAsyncRWLockHelper> _setChildren = new ConcurrentHashSet<LinkedAsyncRWLockHelper>();

        private void AddChild(LinkedAsyncRWLockHelper objChild)
#else
        private void AddChild()
#endif
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            if (Interlocked.Increment(ref _intNumChildren) == 1)
                _objHasChildrenSemaphore.SafeWait(_objDisposalToken);
#if LINKEDSEMAPHOREDEBUG
            _setChildren.TryAdd(objChild);
#endif
        }

#if LINKEDSEMAPHOREDEBUG
        private async Task AddChildAsync(LinkedAsyncRWLockHelper objChild, CancellationToken token = default)
#else
        private Task AddChildAsync(CancellationToken token = default)
#endif
        {
#if LINKEDSEMAPHOREDEBUG
            token.ThrowIfCancellationRequested();
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            if (Interlocked.Increment(ref _intNumChildren) == 1)
                await _objHasChildrenSemaphore.WaitAsync(token).ConfigureAwait(false);
            _setChildren.TryAdd(objChild);
#else
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (IsDisposed)
                return Task.FromException(new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper)));
            return Interlocked.Increment(ref _intNumChildren) == 1
                ? _objHasChildrenSemaphore.WaitAsync(token)
                : Task.CompletedTask;
#endif
        }

#if LINKEDSEMAPHOREDEBUG
        private void RemoveChild(LinkedAsyncRWLockHelper objChild)
#else
        private void RemoveChild()
#endif
        {
            if (Interlocked.Decrement(ref _intNumChildren) == 0)
                _objHasChildrenSemaphore.Release();
#if LINKEDSEMAPHOREDEBUG
            _setChildren.Remove(objChild);
#endif
        }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool blnHasWriteLockAlready)
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;
            _objDisposalTokenSource.Cancel();
            // Acquire all of our locks
            DebuggableSemaphoreSlim objUpgradeableReaderSemaphore = Interlocked.Exchange(ref _objUpgradeableReaderSemaphore, null);
            if (!blnHasWriteLockAlready)
                // ReSharper disable once MethodSupportsCancellation
                objUpgradeableReaderSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objReaderSemaphore = Interlocked.Exchange(ref _objReaderSemaphore, null);
            if (!blnHasWriteLockAlready)
                // ReSharper disable once MethodSupportsCancellation
                objReaderSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objPendingWriterSemaphore = Interlocked.Exchange(ref _objPendingWriterSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objPendingWriterSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objWriterCancellationSemaphore = Interlocked.Exchange(ref _objWriterCancellationSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objWriterCancellationSemaphore?.SafeWait();

#if LINKEDSEMAPHOREDEBUG
            while (!_setChildren.IsEmpty)
                // ReSharper disable once MethodSupportsCancellation
                Utils.SafeSleep();
#endif

            DebuggableSemaphoreSlim objHasChildrenSemaphore = Interlocked.Exchange(ref _objHasChildrenSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objHasChildrenSemaphore?.SafeWait();

#if LINKEDSEMAPHOREDEBUG
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            Interlocked.Increment(ref _intDisposedStatus);
#if LINKEDSEMAPHOREDEBUG
            _objParentLinkedHelper?.RemoveChild(this);
#else
            Interlocked.Exchange(ref _objParentLinkedHelper, null)?.RemoveChild();
#endif

            // Progressively release and dispose of our locks
            if (objHasChildrenSemaphore != null)
            {
                objHasChildrenSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objHasChildrenSemaphore);
                else
                    objHasChildrenSemaphore.Dispose();
            }

            if (objWriterCancellationSemaphore != null)
            {
                objWriterCancellationSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objWriterCancellationSemaphore);
                else
                    objWriterCancellationSemaphore.Dispose();
            }

            if (objPendingWriterSemaphore != null)
            {
                objPendingWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objPendingWriterSemaphore);
                else
                    objPendingWriterSemaphore.Dispose();
            }

            if (objReaderSemaphore != null)
            {
                objReaderSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objReaderSemaphore);
                else
                    objReaderSemaphore.Dispose();
            }

            if (objUpgradeableReaderSemaphore != null)
            {
                objUpgradeableReaderSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objUpgradeableReaderSemaphore);
                else
                    objUpgradeableReaderSemaphore.Dispose();
            }
        }

        public ValueTask DisposeAsync()
        {
            return DisposeAsync(false);
        }

        public async ValueTask DisposeAsync(bool blnHasWriteLockAlready)
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;
            _objDisposalTokenSource.Cancel();
            // Acquire all of our locks
            DebuggableSemaphoreSlim objUpgradeableReaderSemaphore = Interlocked.Exchange(ref _objUpgradeableReaderSemaphore, null);
            if (objUpgradeableReaderSemaphore != null && !blnHasWriteLockAlready)
                // ReSharper disable once MethodSupportsCancellation
                await objUpgradeableReaderSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objReaderSemaphore = Interlocked.Exchange(ref _objReaderSemaphore, null);
            if (objReaderSemaphore != null && !blnHasWriteLockAlready)
                // ReSharper disable once MethodSupportsCancellation
                await objReaderSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objPendingWriterSemaphore = Interlocked.Exchange(ref _objPendingWriterSemaphore, null);
            if (objPendingWriterSemaphore != null)
                // ReSharper disable once MethodSupportsCancellation
                await objPendingWriterSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objWriterCancellationSemaphore = Interlocked.Exchange(ref _objWriterCancellationSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objWriterCancellationSemaphore?.SafeWait();

#if LINKEDSEMAPHOREDEBUG
            while (!_setChildren.IsEmpty)
                // ReSharper disable once MethodSupportsCancellation
                await Utils.SafeSleepAsync().ConfigureAwait(false);
#endif

            DebuggableSemaphoreSlim objHasChildrenSemaphore = Interlocked.Exchange(ref _objHasChildrenSemaphore, null);
            if (objHasChildrenSemaphore != null)
                // ReSharper disable once MethodSupportsCancellation
                await objHasChildrenSemaphore.WaitAsync().ConfigureAwait(false);

#if LINKEDSEMAPHOREDEBUG
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            Interlocked.Increment(ref _intDisposedStatus);
#if LINKEDSEMAPHOREDEBUG
            _objParentLinkedHelper?.RemoveChild(this);
#else
            Interlocked.Exchange(ref _objParentLinkedHelper, null)?.RemoveChild();
#endif

            // Progressively release and dispose of our locks
            if (objHasChildrenSemaphore != null)
            {
                objHasChildrenSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objHasChildrenSemaphore);
                else
                    objHasChildrenSemaphore.Dispose();
            }

            if (objWriterCancellationSemaphore != null)
            {
                objWriterCancellationSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objWriterCancellationSemaphore);
                else
                    objWriterCancellationSemaphore.Dispose();
            }

            if (objPendingWriterSemaphore != null)
            {
                objPendingWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objPendingWriterSemaphore);
                else
                    objPendingWriterSemaphore.Dispose();
            }

            if (objReaderSemaphore != null)
            {
                objReaderSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objReaderSemaphore);
                else
                    objReaderSemaphore.Dispose();
            }

            if (objUpgradeableReaderSemaphore != null)
            {
                objUpgradeableReaderSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objUpgradeableReaderSemaphore);
                else
                    objUpgradeableReaderSemaphore.Dispose();
            }
        }

        public void TakeReadLock(bool blnSkipSemaphore, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus > 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return;
            }
            long lngNumReaders = Interlocked.Increment(ref _lngNumReaders);
            // Negative = there's a writer lock that's waiting or active
            if (lngNumReaders < 0)
            {
                try
                {
                    if (blnSkipSemaphore)
                    {
                        // We aren't blocked because we are a re-entrant reader lock, so we should make sure we up the pending count as well
                        DebuggableSemaphoreSlim objLoopSemaphore = _objWriterCancellationSemaphore;
                        if (objLoopSemaphore != null)
                        {
                            using (CancellationTokenSource objNewTokenSource =
                                   CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                                objLoopSemaphore.SafeWait(objNewTokenSource.Token);
                        }
                        try
                        {
                            if (_lngNumReaders < 0) // Check to make sure we didn't cancel in-between
                                Interlocked.Increment(ref _lngPendingCountForWriter);
                        }
                        finally
                        {
                            objLoopSemaphore?.Release();
                        }
                    }
                    else
                    {
                        using (CancellationTokenSource objNewTokenSource =
                               CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                        {
                            token = objNewTokenSource.Token;
                            // Use local for thread safety
                            DebuggableSemaphoreSlim objReaderSemaphore = ReaderSemaphore;
                            token.ThrowIfCancellationRequested();
                            objReaderSemaphore.SafeWait(token);
                            objReaderSemaphore.Release();
                        }
                    }
                }
                catch
                {
                    lngNumReaders = Interlocked.Decrement(ref _lngNumReaders);
                    if (lngNumReaders >= 0)
                        throw;
                    // Use locals for thread safety
                    DebuggableSemaphoreSlim objLoopSemaphore = _objWriterCancellationSemaphore;
                    // ReSharper disable once MethodSupportsCancellation
                    objLoopSemaphore?.SafeWait();
                    long lngPendingCount;
                    try
                    {
                        if (_lngNumReaders >= 0) // Check to make sure we didn't cancel in-between
                            throw;
                        lngPendingCount = Interlocked.Decrement(ref _lngPendingCountForWriter);
                    }
                    finally
                    {
                        objLoopSemaphore?.Release();
                    }

                    if (lngPendingCount != 0)
                        throw;
                    try
                    {
                        _objPendingWriterSemaphore?.Release();
                    }
                    catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }
                    catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }

                    throw;
                }
            }
        }

        public async Task TakeReadLockAsync(bool blnSkipSemaphore, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus > 0)
            {
#if DEBUG
                Debug.WriteLine(
                    "Entering a read lock after it has been disposed. Not fatal, just potentially a sign of bad code. Stacktrace:");
                Debug.WriteLine(EnhancedStackTrace.Current().ToString());
#endif
                return;
            }
            long lngNumReaders = Interlocked.Increment(ref _lngNumReaders);
            // Negative = there's a writer lock that's waiting or active
            if (lngNumReaders < 0)
            {
                try
                {
                    if (blnSkipSemaphore)
                    {
                        // We aren't blocked because we are a re-entrant reader lock, so we should make sure we up the pending count as well
                        DebuggableSemaphoreSlim objLoopSemaphore = _objWriterCancellationSemaphore;
                        if (objLoopSemaphore != null)
                        {
                            using (CancellationTokenSource objNewTokenSource =
                                   CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                                await objLoopSemaphore.WaitAsync(objNewTokenSource.Token).ConfigureAwait(false);
                        }

                        try
                        {
                            if (_lngNumReaders < 0) // Check to make sure we didn't cancel in-between
                                Interlocked.Increment(ref _lngPendingCountForWriter);
                        }
                        finally
                        {
                            objLoopSemaphore?.Release();
                        }
                    }
                    else
                    {
                        using (CancellationTokenSource objNewTokenSource =
                               CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                        {
                            token = objNewTokenSource.Token;
                            // Use local for thread safety
                            DebuggableSemaphoreSlim objReaderSemaphore = ReaderSemaphore;
                            token.ThrowIfCancellationRequested();
                            await objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                            objReaderSemaphore.Release();
                        }
                    }
                }
                catch
                {
                    lngNumReaders = Interlocked.Decrement(ref _lngNumReaders);
                    if (lngNumReaders >= 0)
                        throw;
                    // Use locals for thread safety
                    DebuggableSemaphoreSlim objLoopSemaphore = _objWriterCancellationSemaphore;
                    if (objLoopSemaphore != null)
                        // ReSharper disable once MethodSupportsCancellation
                        await objLoopSemaphore.WaitAsync().ConfigureAwait(false);
                    long lngPendingCount;
                    try
                    {
                        if (_lngNumReaders >= 0) // Check to make sure we didn't cancel in-between
                            throw;
                        lngPendingCount = Interlocked.Decrement(ref _lngPendingCountForWriter);
                    }
                    finally
                    {
                        objLoopSemaphore?.Release();
                    }

                    if (lngPendingCount != 0)
                        throw;
                    try
                    {
                        _objPendingWriterSemaphore?.Release();
                    }
                    catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }
                    catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }

                    throw;
                }
            }
        }

        public void ReleaseReadLock()
        {
            if (_intDisposedStatus > 1)
                return;
            long lngNumReaders = Interlocked.Decrement(ref _lngNumReaders);
            // A writer is pending, signal them that a reader lock has exited
            if (lngNumReaders < 0)
            {
                // Use locals for thread safety
                DebuggableSemaphoreSlim objLoopSemaphore = _objWriterCancellationSemaphore;
                // ReSharper disable once MethodSupportsCancellation
                objLoopSemaphore?.SafeWait();
                long lngPendingCount;
                try
                {
                    if (_lngNumReaders >= 0) // Check to make sure we didn't cancel in-between
                        return;
                    lngPendingCount = Interlocked.Decrement(ref _lngPendingCountForWriter);
                }
                finally
                {
                    objLoopSemaphore?.Release();
                }
                if (lngPendingCount == 0)
                {
                    try
                    {
                        _objPendingWriterSemaphore?.Release();
                    }
                    catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }
                    catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }
                }
            }
        }

        public async Task ReleaseReadLockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus > 1)
                return;
            long lngNumReaders = Interlocked.Decrement(ref _lngNumReaders);
            // A writer is pending, signal them that a reader lock has exited
            if (lngNumReaders < 0)
            {
                // Use locals for thread safety
                DebuggableSemaphoreSlim objLoopSemaphore = _objWriterCancellationSemaphore;
                if (objLoopSemaphore != null)
                    await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                long lngPendingCount;
                try
                {
                    if (_lngNumReaders >= 0) // Check to make sure we didn't cancel in-between
                        return;
                    lngPendingCount = Interlocked.Decrement(ref _lngPendingCountForWriter);
                }
                finally
                {
                    objLoopSemaphore?.Release();
                }
                if (lngPendingCount == 0)
                {
                    try
                    {
                        _objPendingWriterSemaphore?.Release();
                    }
                    catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }
                    catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                    {
                        // swallow this if we got disposed in-between
                    }
                }
            }
        }

        public void TakeUpgradeableReadLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            using (CancellationTokenSource objNewTokenSource =
                   CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
            {
                token = objNewTokenSource.Token;
                // Use locals for thread safety
                // Acquire the upgradeable read lock that also prevents all other locks besides read locks
                DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                token.ThrowIfCancellationRequested();
                objLoopSemaphore.SafeWait(token);
            }
        }

        public async Task TakeUpgradeableReadLockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            using (CancellationTokenSource objNewTokenSource =
                   CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
            {
                token = objNewTokenSource.Token;
                // Use locals for thread safety
                // Acquire the upgradeable read lock that also prevents all other locks besides read locks
                DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                token.ThrowIfCancellationRequested();
                await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
            }
        }

        public void ReleaseUpgradeableReadLock()
        {
            if (_intDisposedStatus > 1)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            // Use locals for thread safety
            DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
            try
            {
                // Release our upgradeable read lock, allowing pending upgradeable reader locks and writer locks through
                objLoopSemaphore?.Release();
            }
            catch (SemaphoreFullException) when (_intDisposedStatus > 1)
            {
                // swallow this if we got disposed in-between
            }
            catch (ObjectDisposedException) when (_intDisposedStatus > 1)
            {
                // swallow this if we got disposed in-between
            }
        }

        public void TakeWriteLock(LinkedAsyncRWLockHelper objTopMostHeldWriter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            using (new FetchSafelyFromPool<Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>(
                       s_objWriterLockHelperStackPool,
                       out Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>> stkUndo))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;
                        // Use locals for thread safety
                        DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                        // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkUndo.Push(
                            new Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(this, objLoopSemaphore));
                        // Since we can only be re-entrant to an upgradeable reader or a writer, both of these will have the semaphore already set, so we don't need to go up the chain

                        token.ThrowIfCancellationRequested();

                        LinkedAsyncRWLockHelper objLoopHelper = this;
                        while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                        {
                            // Acquire the reader lock to lock out readers
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore = objLoopHelper.ReaderSemaphore;
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore.SafeWait(token);
                            stkUndo.Push(
                                new Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper,
                                    objLoopSemaphore));
                            token.ThrowIfCancellationRequested();
                            // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                            long lngNumReaders = Interlocked.Add(ref objLoopHelper._lngNumReaders, -MaxReaderCount) +
                                                 MaxReaderCount;
                            // Wait for existing readers to exit
                            objLoopSemaphore = objLoopHelper.PendingWriterSemaphore;
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                objLoopSemaphore.SafeWait(token);
                            }
                            catch
                            {
                                Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                                throw;
                            }

                            try
                            {
                                if (lngNumReaders != 0)
                                {
                                    long lngPendingCount;
                                    // Use locals for thread safety
                                    DebuggableSemaphoreSlim objLoopSemaphore2 =
                                        objLoopHelper._objWriterCancellationSemaphore;
                                    objLoopSemaphore2?.SafeWait(token);
                                    try
                                    {
                                        lngPendingCount = Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter,
                                            lngNumReaders);
                                    }
                                    finally
                                    {
                                        objLoopSemaphore2?.Release();
                                    }
                                    if (lngPendingCount != lngNumReaders)
                                        Utils.BreakIfDebug();
                                    if (lngPendingCount > 0)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        objLoopSemaphore.SafeWait(token);
                                    }
                                }
                            }
                            catch
                            {
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 =
                                    objLoopHelper._objWriterCancellationSemaphore;
                                // ReSharper disable once MethodSupportsCancellation
                                objLoopSemaphore2?.SafeWait();
                                try
                                {
                                    Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                                    objLoopHelper._lngPendingCountForWriter = 0;
                                }
                                finally
                                {
                                    objLoopSemaphore2?.Release();
                                }

                                throw;
                            }
                            finally
                            {
                                objLoopSemaphore.Release();
                            }

                            stkUndo.Push(
                                new Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper, null));

                            objLoopHelper = objLoopHelper.ParentLinkedHelper;
                        }
                    }
                }
                catch
                {
                    while (stkUndo.Count > 0)
                    {
                        (LinkedAsyncRWLockHelper objLoopHelper, DebuggableSemaphoreSlim objLoopSemaphore) =
                            stkUndo.Pop();
                        if (objLoopHelper != null)
                        {
                            if (objLoopSemaphore != null)
                            {
                                if (objLoopHelper._intDisposedStatus > 1)
                                    continue;
                                try
                                {
                                    objLoopSemaphore.Release();
                                }
                                catch (SemaphoreFullException) when (objLoopHelper._intDisposedStatus > 1)
                                {
                                    // swallow this if we got disposed in-between
                                }
                                catch (ObjectDisposedException) when (objLoopHelper._intDisposedStatus > 1)
                                {
                                    // swallow this if we got disposed in-between
                                }
                            }
                            else
                            {
                                Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                            }
                        }
                        else
                            Utils.BreakIfDebug(); // This should not happen, we should always have a helper set
                    }

                    throw;
                }
            }
        }

        public async Task TakeWriteLockAsync(LinkedAsyncRWLockHelper objTopMostHeldWriter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            using (new FetchSafelyFromPool<Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>(
                       s_objWriterLockHelperStackPool,
                       out Stack<Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>> stkUndo))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;
                        // Use locals for thread safety
                        DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkUndo.Push(
                            new Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(this, objLoopSemaphore));
                        // Since we can only be re-entrant to an upgradeable reader or a writer, both of these will have the semaphore already set, so we don't need to go up the chain

                        token.ThrowIfCancellationRequested();

                        LinkedAsyncRWLockHelper objLoopHelper = this;
                        while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                        {
                            // Acquire the reader lock to lock out readers
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore = objLoopHelper.ReaderSemaphore;
                            token.ThrowIfCancellationRequested();
                            await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                            stkUndo.Push(
                                new Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper,
                                    objLoopSemaphore));
                            token.ThrowIfCancellationRequested();
                            // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                            long lngNumReaders = Interlocked.Add(ref objLoopHelper._lngNumReaders, -MaxReaderCount) +
                                                 MaxReaderCount;
                            // Wait for existing readers to exit
                            objLoopSemaphore = objLoopHelper.PendingWriterSemaphore;
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                            }
                            catch
                            {
                                Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                                throw;
                            }

                            try
                            {
                                if (lngNumReaders != 0)
                                {
                                    long lngPendingCount;
                                    // Use locals for thread safety
                                    DebuggableSemaphoreSlim objLoopSemaphore2 = objLoopHelper._objWriterCancellationSemaphore;
                                    if (objLoopSemaphore2 != null)
                                        await objLoopSemaphore2.WaitAsync(token).ConfigureAwait(false);
                                    try
                                    {
                                        lngPendingCount = Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter,
                                            lngNumReaders);
                                    }
                                    finally
                                    {
                                        objLoopSemaphore2?.Release();
                                    }
                                    if (lngPendingCount != lngNumReaders)
                                        Utils.BreakIfDebug();
                                    if (lngPendingCount > 0)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                                    }
                                }
                            }
                            catch
                            {
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 = objLoopHelper._objWriterCancellationSemaphore;
                                if (objLoopSemaphore2 != null)
                                    // ReSharper disable once MethodSupportsCancellation
                                    await objLoopSemaphore2.WaitAsync().ConfigureAwait(false);
                                try
                                {
                                    Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                                    objLoopHelper._lngPendingCountForWriter = 0;
                                }
                                finally
                                {
                                    objLoopSemaphore2?.Release();
                                }

                                throw;
                            }
                            finally
                            {
                                objLoopSemaphore.Release();
                            }

                            stkUndo.Push(
                                new Tuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper, null));

                            objLoopHelper = objLoopHelper.ParentLinkedHelper;
                        }
                    }
                }
                catch
                {
                    while (stkUndo.Count > 0)
                    {
                        (LinkedAsyncRWLockHelper objLoopHelper, DebuggableSemaphoreSlim objLoopSemaphore) =
                            stkUndo.Pop();
                        if (objLoopHelper != null)
                        {
                            if (objLoopSemaphore != null)
                            {
                                if (objLoopHelper._intDisposedStatus > 1)
                                    continue;
                                try
                                {
                                    objLoopSemaphore.Release();
                                }
                                catch (SemaphoreFullException) when (objLoopHelper._intDisposedStatus > 1)
                                {
                                    // swallow this if we got disposed in-between
                                }
                                catch (ObjectDisposedException) when (objLoopHelper._intDisposedStatus > 1)
                                {
                                    // swallow this if we got disposed in-between
                                }
                            }
                            else
                            {
                                Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                            }
                        }
                        else
                            Utils.BreakIfDebug(); // This should not happen, we should always have a helper set
                    }

                    throw;
                }
            }
        }

        public void TakeSingleWriteLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            using (new FetchSafelyFromPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
                       out Stack<DebuggableSemaphoreSlim> stkLockedSemaphores))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;
                        // Use locals for thread safety
                        // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                        DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        objLoopSemaphore = PendingWriterSemaphore;
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore.SafeWait(token);
                        }
                        catch
                        {
                            Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                            throw;
                        }

                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount;
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                                objLoopSemaphore2?.SafeWait(token);
                                try
                                {
                                    lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                        lngNumReaders);
                                }
                                finally
                                {
                                    objLoopSemaphore2?.Release();
                                }
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    objLoopSemaphore.SafeWait(token);
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                            // ReSharper disable once MethodSupportsCancellation
                            objLoopSemaphore2?.SafeWait();
                            try
                            {
                                Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                                _lngPendingCountForWriter = 0;
                            }
                            finally
                            {
                                objLoopSemaphore2?.Release();
                            }

                            throw;
                        }
                        finally
                        {
                            objLoopSemaphore.Release();
                        }
                    }
                }
                catch
                {
                    if (_intDisposedStatus > 1)
                        throw;
                    while (stkLockedSemaphores.Count > 0)
                    {
                        try
                        {
                            stkLockedSemaphores.Pop().Release();
                        }
                        catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                        catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                    }

                    throw;
                }
            }
        }

        public async Task TakeSingleWriteLockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            using (new FetchSafelyFromPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
                       out Stack<DebuggableSemaphoreSlim> stkLockedSemaphores))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;
                        // Use locals for thread safety
                        // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                        DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        objLoopSemaphore = PendingWriterSemaphore;
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        }
                        catch
                        {
                            Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                            throw;
                        }

                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount;
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                                if (objLoopSemaphore2 != null)
                                    await objLoopSemaphore2.WaitAsync(token).ConfigureAwait(false);
                                try
                                {
                                    lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                        lngNumReaders);
                                }
                                finally
                                {
                                    objLoopSemaphore2?.Release();
                                }
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                            if (objLoopSemaphore2 != null)
                                // ReSharper disable once MethodSupportsCancellation
                                await objLoopSemaphore2.WaitAsync().ConfigureAwait(false);
                            try
                            {
                                Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                                _lngPendingCountForWriter = 0;
                            }
                            finally
                            {
                                objLoopSemaphore2?.Release();
                            }

                            throw;
                        }
                        finally
                        {
                            objLoopSemaphore.Release();
                        }
                    }
                }
                catch
                {
                    if (_intDisposedStatus > 1)
                        throw;
                    while (stkLockedSemaphores.Count > 0)
                    {
                        try
                        {
                            stkLockedSemaphores.Pop().Release();
                        }
                        catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                        catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                    }

                    throw;
                }
            }
        }

        public void SingleUpgradeToWriteLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            // Use locals for thread safety
            DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
            if (objLoopSemaphore.CurrentCount != 0)
                throw new InvalidOperationException("Attempting to upgrade to write lock while not inside of an upgradeable read lock");
            using (new FetchSafelyFromPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
                       out Stack<DebuggableSemaphoreSlim> stkLockedSemaphores))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        objLoopSemaphore = PendingWriterSemaphore;
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore.SafeWait(token);
                        }
                        catch
                        {
                            Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                            throw;
                        }

                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount;
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                                objLoopSemaphore2?.SafeWait(token);
                                try
                                {
                                    lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                        lngNumReaders);
                                }
                                finally
                                {
                                    objLoopSemaphore2?.Release();
                                }
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    objLoopSemaphore.SafeWait(token);
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                            // ReSharper disable once MethodSupportsCancellation
                            objLoopSemaphore2?.SafeWait();
                            try
                            {
                                Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                                _lngPendingCountForWriter = 0;
                            }
                            finally
                            {
                                objLoopSemaphore2?.Release();
                            }

                            throw;
                        }
                        finally
                        {
                            objLoopSemaphore.Release();
                        }
                    }
                }
                catch
                {
                    if (_intDisposedStatus > 1)
                        throw;
                    while (stkLockedSemaphores.Count > 0)
                    {
                        try
                        {
                            stkLockedSemaphores.Pop().Release();
                        }
                        catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                        catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                    }

                    throw;
                }
            }
        }

        public async Task SingleUpgradeToWriteLockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            // Use locals for thread safety
            DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
            if (objLoopSemaphore.CurrentCount != 0)
                throw new InvalidOperationException("Attempting to upgrade to write lock while not inside of an upgradeable read lock");
            using (new FetchSafelyFromPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
                       out Stack<DebuggableSemaphoreSlim> stkLockedSemaphores))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        objLoopSemaphore = PendingWriterSemaphore;
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        }
                        catch
                        {
                            Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                            throw;
                        }

                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount;
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                                if (objLoopSemaphore2 != null)
                                    await objLoopSemaphore2.WaitAsync(token).ConfigureAwait(false);
                                try
                                {
                                    lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                        lngNumReaders);
                                }
                                finally
                                {
                                    objLoopSemaphore2?.Release();
                                }
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objWriterCancellationSemaphore;
                            if (objLoopSemaphore2 != null)
                                // ReSharper disable once MethodSupportsCancellation
                                await objLoopSemaphore2.WaitAsync().ConfigureAwait(false);
                            try
                            {
                                Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                                _lngPendingCountForWriter = 0;
                            }
                            finally
                            {
                                objLoopSemaphore2?.Release();
                            }

                            throw;
                        }
                        finally
                        {
                            objLoopSemaphore.Release();
                        }
                    }
                }
                catch
                {
                    if (_intDisposedStatus > 1)
                        throw;
                    while (stkLockedSemaphores.Count > 0)
                    {
                        try
                        {
                            stkLockedSemaphores.Pop().Release();
                        }
                        catch (SemaphoreFullException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                        catch (ObjectDisposedException) when (_intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                    }

                    throw;
                }
            }
        }

        public void ReleaseSingleWriteLock()
        {
            ReleaseWriteLock(null, null);
        }

        public void ReleaseWriteLock(LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter)
        {
            if (_intDisposedStatus > 1)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));

            // There are upgradeable readers into which we have re-entered, so go up the chain
            if (objTopMostHeldUReader != null)
            {
                using (new FetchSafelyFromPool<Stack<LinkedAsyncRWLockHelper>>(s_objHelperStackPool,
                           out Stack<LinkedAsyncRWLockHelper> stkLockedUReaderHelpers))
                {
                    // Unlock from bottom upwards to avoid race conditions
                    LinkedAsyncRWLockHelper objLoopHelper = ParentLinkedHelper;
                    while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                    {
                        stkLockedUReaderHelpers.Push(objLoopHelper);
                        objLoopHelper = objLoopHelper.ParentLinkedHelper;
                    }

                    while (stkLockedUReaderHelpers.Count > 0)
                    {
                        objLoopHelper = stkLockedUReaderHelpers.Pop();
                        // Announce to readers that we are no longer active
                        Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);

                        if (objLoopHelper._intDisposedStatus > 1)
                            throw new ObjectDisposedException(nameof(objLoopHelper));
                        // Use locals for thread safety
                        // Release our reader lock, allowing waiting readers to pass through
                        DebuggableSemaphoreSlim objLoopSemaphore2 = objLoopHelper.ReaderSemaphore;
                        try
                        {
                            objLoopSemaphore2?.Release();
                        }
                        catch (SemaphoreFullException) when (objLoopHelper._intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                        catch (ObjectDisposedException) when (objLoopHelper._intDisposedStatus > 1)
                        {
                            // swallow this if we got disposed in-between
                        }
                    }
                }
            }

            // Announce to readers that we are no longer active
            Interlocked.Add(ref _lngNumReaders, MaxReaderCount);

            if (_intDisposedStatus > 1)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));

            // Use locals for thread safety
            // Release our reader lock, allowing waiting readers to pass through
            DebuggableSemaphoreSlim objLoopSemaphore = ReaderSemaphore;
            try
            {
                objLoopSemaphore?.Release();
            }
            catch (SemaphoreFullException) when (_intDisposedStatus > 1)
            {
                // swallow this if we got disposed in-between
            }
            catch (ObjectDisposedException) when (_intDisposedStatus > 1)
            {
                // swallow this if we got disposed in-between
            }

            // Release our upgradeable read lock, allowing pending upgradeable reader locks and writer locks through
            objLoopSemaphore = UpgradeableReaderSemaphore;
            try
            {
                objLoopSemaphore?.Release();
            }
            catch (SemaphoreFullException) when (_intDisposedStatus > 1)
            {
                // swallow this if we got disposed in-between
            }
            catch (ObjectDisposedException) when (_intDisposedStatus > 1)
            {
                // swallow this if we got disposed in-between
            }
        }
    }
}
