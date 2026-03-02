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
        private DebuggableSemaphoreSlim _objCancelledWriterSemaphore; // We need this semaphore to prevent race conditions during cancellations
        private DebuggableSemaphoreSlim _objPendingWriterSemaphore;
        private DebuggableSemaphoreSlim _objActiveUpgradeableReaderSemaphore;
        private DebuggableSemaphoreSlim _objActiveWriterSemaphore;
        private long _lngNumReaders;
        private long _lngPendingCountForWriter;
        private int _intNumChildren;
        private DebuggableSemaphoreSlim _objHasChildrenSemaphore; // Used to prevent disposing a helper until it has no more children left

        private readonly LinkedAsyncRWLockHelper _objParentLinkedHelper;
        private readonly CancellationTokenSource _objDisposalTokenSource;
        private readonly CancellationToken _objDisposalToken;

        public DebuggableSemaphoreSlim PendingWriterSemaphore => _objPendingWriterSemaphore;
        public DebuggableSemaphoreSlim ActiveUpgradeableReaderSemaphore => _objActiveUpgradeableReaderSemaphore;
        public DebuggableSemaphoreSlim ActiveWriterSemaphore => _objActiveWriterSemaphore;

        public LinkedAsyncRWLockHelper ParentLinkedHelper => _objParentLinkedHelper;

        private static readonly SafeObjectPool<Stack<LinkedAsyncRWLockHelper>> s_objHelperStackPool =
            new SafeObjectPool<Stack<LinkedAsyncRWLockHelper>>(() => new Stack<LinkedAsyncRWLockHelper>(8), x => x.Clear());

        private static readonly SafeObjectPool<Stack<DebuggableSemaphoreSlim>> s_objSemaphoreStackPool =
            new SafeObjectPool<Stack<DebuggableSemaphoreSlim>>(() => new Stack<DebuggableSemaphoreSlim>(8), x => x.Clear());

        private static readonly SafeObjectPool<Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>
            s_objWriterLockHelperStackPool =
                new SafeObjectPool<Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>(() =>
                    new Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>(8), x => x.Clear());

        public bool IsDisposed => _intDisposedStatus > 0;

        public LinkedAsyncRWLockHelper(LinkedAsyncRWLockHelper objParent, bool blnGetFromPool = true)
        {
#if LINKEDSEMAPHOREDEBUG
            objParent?.AddChild(this);
#else
            objParent?.AddChild();
#endif
            _objDisposalTokenSource = new CancellationTokenSource();
            _objDisposalToken = _objDisposalTokenSource.Token;
            if (blnGetFromPool)
            {
                _blnSemaphoreIsPooled = true;
                LinkedAsyncRWLockHelper objGrandParent = objParent?.ParentLinkedHelper;

                _objCancelledWriterSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objPendingWriterSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objActiveUpgradeableReaderSemaphore = safelyGetNewPooledSemaphoreSlim();
                _objActiveWriterSemaphore = safelyGetNewPooledSemaphoreSlim();
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
                                   || objNewSemaphore == objParent.ActiveUpgradeableReaderSemaphore
                                   || objNewSemaphore == objParent.ActiveWriterSemaphore
                                   || objNewSemaphore == objGrandParent.PendingWriterSemaphore
                                   || objNewSemaphore == objGrandParent.ActiveUpgradeableReaderSemaphore
                                   || objNewSemaphore == objGrandParent.ActiveWriterSemaphore)
                            {
                                objNewSemaphore = Utils.SemaphorePool.Get();
                            }
                        }
                        else
                        {
                            while (objNewSemaphore == objParent.PendingWriterSemaphore
                                   || objNewSemaphore == objParent.ActiveUpgradeableReaderSemaphore
                                   || objNewSemaphore == objParent.ActiveWriterSemaphore)
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
                _objCancelledWriterSemaphore = new DebuggableSemaphoreSlim();
                _objPendingWriterSemaphore = new DebuggableSemaphoreSlim();
                _objActiveUpgradeableReaderSemaphore = new DebuggableSemaphoreSlim();
                _objActiveWriterSemaphore = new DebuggableSemaphoreSlim();
                _objHasChildrenSemaphore = new DebuggableSemaphoreSlim();
            }

            _objParentLinkedHelper = objParent;

#if LINKEDSEMAPHOREDEBUG
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();
#endif
        }

#if LINKEDSEMAPHOREDEBUG
        private readonly string _strGuid = Guid.NewGuid().ToString();

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string RecordedStackTrace { get; set; }

        private readonly ConcurrentHashSet<LinkedAsyncRWLockHelper> _setChildren = new ConcurrentHashSet<LinkedAsyncRWLockHelper>();

        private void AddChild(LinkedAsyncRWLockHelper objChild)
#else
        private void AddChild()
#endif
        {
            if (IsDisposed || _objDisposalToken.IsCancellationRequested)
                throw new  ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            if (Interlocked.Increment(ref _intNumChildren) == 1)
            {
                try
                {
                    _objHasChildrenSemaphore.SafeWait(_objDisposalToken);
                }
                catch (OperationCanceledException)
                {
                    Interlocked.Decrement(ref _intNumChildren);
                    throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
                }
                catch
                {
                    Interlocked.Decrement(ref _intNumChildren);
                    throw;
                }
            }
#if LINKEDSEMAPHOREDEBUG
            _setChildren.TryAdd(objChild);
#endif
        }

#if LINKEDSEMAPHOREDEBUG
        private async Task AddChildAsync(LinkedAsyncRWLockHelper objChild, CancellationToken token = default)
#else
        private async Task AddChildAsync(CancellationToken token = default)
#endif
        {
            token.ThrowIfCancellationRequested();
            if (IsDisposed || _objDisposalToken.IsCancellationRequested)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            if (Interlocked.Increment(ref _intNumChildren) == 1)
            {
                using (CancellationTokenSource objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                {
                    try
                    {
                        await _objHasChildrenSemaphore.WaitAsync(objSource.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Interlocked.Decrement(ref _intNumChildren);
                        if (_objDisposalToken.IsCancellationRequested)
                            throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
                        throw;
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _intNumChildren);
                        throw;
                    }
                }
            }
#if LINKEDSEMAPHOREDEBUG
            _setChildren.TryAdd(objChild);
#endif
        }

#if LINKEDSEMAPHOREDEBUG
        private void RemoveChild(LinkedAsyncRWLockHelper objChild)
#else
        private void RemoveChild()
#endif
        {
#if LINKEDSEMAPHOREDEBUG
            _setChildren.Remove(objChild);
#endif
            if (Interlocked.Decrement(ref _intNumChildren) == 0)
                _objHasChildrenSemaphore.Release();
        }

        public void Dispose()
        {
            if (_objDisposalToken.IsCancellationRequested)
                return;
            // Acquire all of our locks
            DebuggableSemaphoreSlim objHasChildrenSemaphore = _objHasChildrenSemaphore;
            if (objHasChildrenSemaphore != null)
            {
                try
                {
                    objHasChildrenSemaphore.SafeWait(_objDisposalToken);
                }
                catch (OperationCanceledException)
                {
                    if (_objDisposalToken.IsCancellationRequested)
                        return;
                    throw;
                }
            }
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) != 0)
                return;
            _objDisposalTokenSource.Cancel();
#if LINKEDSEMAPHOREDEBUG
            if (!_setChildren.IsEmpty)
                Utils.BreakIfDebug();
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            DebuggableSemaphoreSlim objActiveUpgradeableReaderSemaphore = Interlocked.Exchange(ref _objActiveUpgradeableReaderSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objActiveUpgradeableReaderSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objActiveWriterSemaphore = Interlocked.Exchange(ref _objActiveWriterSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objActiveWriterSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objPendingWriterSemaphore = Interlocked.Exchange(ref _objPendingWriterSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objPendingWriterSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objCancelledWriterSemaphore = Interlocked.Exchange(ref _objCancelledWriterSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objCancelledWriterSemaphore?.SafeWait();

#if LINKEDSEMAPHOREDEBUG
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            Interlocked.Increment(ref _intDisposedStatus);
            _objHasChildrenSemaphore = null;
#if LINKEDSEMAPHOREDEBUG
            _objParentLinkedHelper?.RemoveChild(this);
#else
            _objParentLinkedHelper?.RemoveChild();
#endif

            // Progressively release and dispose of our locks
            if (objCancelledWriterSemaphore != null)
            {
                objCancelledWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objCancelledWriterSemaphore);
                else
                    objCancelledWriterSemaphore.Dispose();
            }

            if (objPendingWriterSemaphore != null)
            {
                objPendingWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objPendingWriterSemaphore);
                else
                    objPendingWriterSemaphore.Dispose();
            }

            if (objActiveWriterSemaphore != null)
            {
                objActiveWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objActiveWriterSemaphore);
                else
                    objActiveWriterSemaphore.Dispose();
            }

            if (objActiveUpgradeableReaderSemaphore != null)
            {
                objActiveUpgradeableReaderSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objActiveUpgradeableReaderSemaphore);
                else
                    objActiveUpgradeableReaderSemaphore.Dispose();
            }

            if (objHasChildrenSemaphore != null)
            {
                objHasChildrenSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objHasChildrenSemaphore);
                else
                    objHasChildrenSemaphore.Dispose();
            }

            _objDisposalTokenSource.Dispose();
        }

#if LINKEDSEMAPHOREDEBUG
        public ValueTask DisposeAsync()
        {
            if (_objDisposalToken.IsCancellationRequested)
                return default;
            // Acquire all of our locks
            DebuggableSemaphoreSlim objHasChildrenSemaphore = _objHasChildrenSemaphore;
            if (objHasChildrenSemaphore != null)
            {
                try
                {
                    objHasChildrenSemaphore.SafeWait(_objDisposalToken);
                }
                catch (OperationCanceledException)
                {
                    if (_objDisposalToken.IsCancellationRequested)
                        return default;
                    throw;
                }
            }
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) != 0)
                return default;
            _objDisposalTokenSource.Cancel();
            if (!_setChildren.IsEmpty)
                Utils.BreakIfDebug();
            // Need to separate out the async part to make sure the stacktrace is being recorded in the context of the task's creation, not its execution
            RecordedStackTrace = EnhancedStackTrace.Current().ToString();

            return Inner();

            async ValueTask Inner()
            {
#else
        public async ValueTask DisposeAsync()
        {
            if (_objDisposalToken.IsCancellationRequested)
                return;
            // Acquire all of our locks
            DebuggableSemaphoreSlim objHasChildrenSemaphore = _objHasChildrenSemaphore;
            if (objHasChildrenSemaphore != null)
            {
                try
                {
                    await objHasChildrenSemaphore.WaitAsync(_objDisposalToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    if (_objDisposalToken.IsCancellationRequested)
                        return;
                    throw;
                }
            }
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) != 0)
                return;
            _objDisposalTokenSource.Cancel();
#endif
            DebuggableSemaphoreSlim objActiveUpgradeableReaderSemaphore = Interlocked.Exchange(ref _objActiveUpgradeableReaderSemaphore, null);
            if (objActiveUpgradeableReaderSemaphore != null)
                // ReSharper disable once MethodSupportsCancellation
                await objActiveUpgradeableReaderSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objActiveWriterSemaphore = Interlocked.Exchange(ref _objActiveWriterSemaphore, null);
            if (objActiveWriterSemaphore != null)
                // ReSharper disable once MethodSupportsCancellation
                await objActiveWriterSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objPendingWriterSemaphore = Interlocked.Exchange(ref _objPendingWriterSemaphore, null);
            if (objPendingWriterSemaphore != null)
                // ReSharper disable once MethodSupportsCancellation
                await objPendingWriterSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objCancelledWriterSemaphore = Interlocked.Exchange(ref _objCancelledWriterSemaphore, null);
            // ReSharper disable once MethodSupportsCancellation
            objCancelledWriterSemaphore?.SafeWait();

            Interlocked.Increment(ref _intDisposedStatus);
            _objHasChildrenSemaphore = null;
#if LINKEDSEMAPHOREDEBUG
            _objParentLinkedHelper?.RemoveChild(this);
#else
            _objParentLinkedHelper?.RemoveChild();
#endif

            // Progressively release and dispose of our locks
            if (objCancelledWriterSemaphore != null)
            {
                objCancelledWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objCancelledWriterSemaphore);
                else
                    objCancelledWriterSemaphore.Dispose();
            }

            if (objPendingWriterSemaphore != null)
            {
                objPendingWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objPendingWriterSemaphore);
                else
                    objPendingWriterSemaphore.Dispose();
            }

            if (objActiveWriterSemaphore != null)
            {
                objActiveWriterSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objActiveWriterSemaphore);
                else
                    objActiveWriterSemaphore.Dispose();
            }

            if (objActiveUpgradeableReaderSemaphore != null)
            {
                objActiveUpgradeableReaderSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objActiveUpgradeableReaderSemaphore);
                else
                    objActiveUpgradeableReaderSemaphore.Dispose();
            }

            if (objHasChildrenSemaphore != null)
            {
                objHasChildrenSemaphore.Release();
                if (_blnSemaphoreIsPooled)
                    Utils.SemaphorePool.Return(ref objHasChildrenSemaphore);
                else
                    objHasChildrenSemaphore.Dispose();
            }

            _objDisposalTokenSource.Dispose();
#if LINKEDSEMAPHOREDEBUG
            }
#endif
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
                        DebuggableSemaphoreSlim objLoopSemaphore = _objCancelledWriterSemaphore;
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
                            DebuggableSemaphoreSlim objReaderSemaphore = ActiveWriterSemaphore;
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
                    DebuggableSemaphoreSlim objLoopSemaphore = _objCancelledWriterSemaphore;
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
                    catch (SemaphoreFullException)
                    {
                        Utils.BreakIfDebug();
                        // swallow if full, can happen if we get the timing just wrong on the backup code meant to prevent deadlocks
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
                        DebuggableSemaphoreSlim objLoopSemaphore = _objCancelledWriterSemaphore;
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
                            DebuggableSemaphoreSlim objReaderSemaphore = ActiveWriterSemaphore;
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
                    DebuggableSemaphoreSlim objLoopSemaphore = _objCancelledWriterSemaphore;
                    if (objLoopSemaphore != null)
                        await objLoopSemaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);
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
                    catch (SemaphoreFullException)
                    {
                        Utils.BreakIfDebug();
                        // // swallow if full, can happen if we get the timing just wrong on the backup code meant to prevent deadlocks
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
                DebuggableSemaphoreSlim objLoopSemaphore = _objCancelledWriterSemaphore;
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
                    catch (SemaphoreFullException)
                    {
                        Utils.BreakIfDebug();
                        // // swallow if full, can happen if we get the timing just wrong on the backup code meant to prevent deadlocks
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
                DebuggableSemaphoreSlim objLoopSemaphore = _objCancelledWriterSemaphore;
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
                    catch (SemaphoreFullException)
                    {
                        Utils.BreakIfDebug();
                        // swallow if full, can happen if we get the timing just wrong on the backup code meant to prevent deadlocks
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
                DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
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
                DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
                token.ThrowIfCancellationRequested();
                await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
            }
        }

        public void ReleaseUpgradeableReadLock()
        {
            if (_intDisposedStatus > 1)
                throw new ObjectDisposedException(nameof(LinkedAsyncRWLockHelper));
            // Use locals for thread safety
            DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
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
            using (new FetchSafelyFromSafeObjectPool<Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>(
                       s_objWriterLockHelperStackPool,
                       out Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>> stkUndo))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;
                        // Use locals for thread safety
                        DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
                        // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkUndo.Push(
                            new ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(this, objLoopSemaphore));
                        // Since we can only be re-entrant to an upgradeable reader or a writer, both of these will have the semaphore already set, so we don't need to go up the chain

                        token.ThrowIfCancellationRequested();

                        LinkedAsyncRWLockHelper objLoopHelper = this;
                        while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                        {
                            // Acquire the reader lock to lock out readers
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore = objLoopHelper.ActiveWriterSemaphore;
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore.SafeWait(token);
                            stkUndo.Push(
                                new ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper,
                                    objLoopSemaphore));
                            token.ThrowIfCancellationRequested();
                            // Hold Pending Writer Semaphore before reader lock announcement to prevent edge case weird stuff
                            objLoopSemaphore = objLoopHelper.PendingWriterSemaphore;
                            objLoopSemaphore.SafeWait(token);
                            // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                            long lngNumReaders = Interlocked.Add(ref objLoopHelper._lngNumReaders, -MaxReaderCount) +
                                                 MaxReaderCount;
                            // Wait for existing readers to exit
                            try
                            {
                                if (lngNumReaders != 0)
                                {
                                    long lngPendingCount = Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter,
                                        lngNumReaders);
                                    if (lngPendingCount != lngNumReaders)
                                        Utils.BreakIfDebug();
                                    if (lngPendingCount > 0)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        while (!objLoopSemaphore.SafeWait(Utils.SleepEmergencyReleaseMaxTicks, token))
                                        {
                                            if (_lngPendingCountForWriter <= 0)
                                                break; // Just in case we get a rare deadlock that can happen with read locks releasing at just the wrong time
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 =
                                    objLoopHelper._objCancelledWriterSemaphore;
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
                                if (objLoopSemaphore.CurrentCount == 0)
                                    objLoopSemaphore.Release();
                            }

                            stkUndo.Push(
                                new ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper, null));

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
            using (new FetchSafelyFromSafeObjectPool<Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>>>(
                       s_objWriterLockHelperStackPool,
                       out Stack<ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>> stkUndo))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;
                        // Use locals for thread safety
                        DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkUndo.Push(
                            new ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(this, objLoopSemaphore));
                        // Since we can only be re-entrant to an upgradeable reader or a writer, both of these will have the semaphore already set, so we don't need to go up the chain

                        token.ThrowIfCancellationRequested();

                        LinkedAsyncRWLockHelper objLoopHelper = this;
                        while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                        {
                            // Acquire the reader lock to lock out readers
                            token.ThrowIfCancellationRequested();
                            objLoopSemaphore = objLoopHelper.ActiveWriterSemaphore;
                            token.ThrowIfCancellationRequested();
                            await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                            stkUndo.Push(
                                new ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper,
                                    objLoopSemaphore));
                            token.ThrowIfCancellationRequested();
                            // Hold Pending Writer Semaphore before reader lock announcement to prevent edge case weird stuff
                            objLoopSemaphore = objLoopHelper.PendingWriterSemaphore;
                            await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                            // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                            long lngNumReaders = Interlocked.Add(ref objLoopHelper._lngNumReaders, -MaxReaderCount) +
                                                 MaxReaderCount;
                            // Wait for existing readers to exit
                            try
                            {
                                if (lngNumReaders != 0)
                                {
                                    long lngPendingCount = Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter,
                                        lngNumReaders);
                                    if (lngPendingCount != lngNumReaders)
                                        Utils.BreakIfDebug();
                                    if (lngPendingCount > 0)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        while (!await objLoopSemaphore
                                                   .WaitAsync(Utils.SleepEmergencyReleaseMaxTicks, token)
                                                   .ConfigureAwait(false))
                                        {
                                            if (_lngPendingCountForWriter <= 0)
                                                break;// Just in case we get a rare deadlock that can happen with read locks releasing at just the wrong time
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Use locals for thread safety
                                DebuggableSemaphoreSlim objLoopSemaphore2 = objLoopHelper._objCancelledWriterSemaphore;
                                if (objLoopSemaphore2 != null)
                                    await objLoopSemaphore2.WaitAsync(CancellationToken.None).ConfigureAwait(false);
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
                                if (objLoopSemaphore.CurrentCount == 0)
                                    objLoopSemaphore.Release();
                            }

                            stkUndo.Push(
                                new ValueTuple<LinkedAsyncRWLockHelper, DebuggableSemaphoreSlim>(objLoopHelper, null));

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
            using (new FetchSafelyFromSafeObjectPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
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
                        DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ActiveWriterSemaphore;
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Hold Pending Writer Semaphore before reader lock announcement to prevent edge case weird stuff
                        objLoopSemaphore = PendingWriterSemaphore;
                        objLoopSemaphore.SafeWait(token);

                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                    lngNumReaders);
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    while (!objLoopSemaphore.SafeWait(Utils.SleepEmergencyReleaseMaxTicks, token))
                                    {
                                        if (_lngPendingCountForWriter <= 0)
                                            break;// Just in case we get a rare deadlock that can happen with read locks releasing at just the wrong time
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objCancelledWriterSemaphore;
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
                            if (objLoopSemaphore.CurrentCount == 0)
                                objLoopSemaphore.Release();
                        }
                    }
                }
                catch when (_intDisposedStatus <= 1)
                {
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
            using (new FetchSafelyFromSafeObjectPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
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
                        DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
                        token.ThrowIfCancellationRequested();
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ActiveWriterSemaphore;
                        token.ThrowIfCancellationRequested();
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();
                        // Hold Pending Writer Semaphore before reader lock announcement to prevent edge case weird stuff
                        objLoopSemaphore = PendingWriterSemaphore;
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                    lngNumReaders);
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    while (!await objLoopSemaphore.WaitAsync(Utils.SleepEmergencyReleaseMaxTicks, token).ConfigureAwait(false))
                                    {
                                        if (_lngPendingCountForWriter <= 0)
                                            break; // Just in case we get a rare deadlock that can happen with read locks releasing at just the wrong time
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objCancelledWriterSemaphore;
                            if (objLoopSemaphore2 != null)
                                await objLoopSemaphore2.WaitAsync(CancellationToken.None).ConfigureAwait(false);
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
                            if (objLoopSemaphore.CurrentCount == 0)
                                objLoopSemaphore.Release();
                        }
                    }
                }
                catch when (_intDisposedStatus <= 1)
                {
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
            DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
            if (objLoopSemaphore.CurrentCount != 0)
                throw new InvalidOperationException("Attempting to upgrade to write lock while not inside of an upgradeable read lock");
            using (new FetchSafelyFromSafeObjectPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
                       out Stack<DebuggableSemaphoreSlim> stkLockedSemaphores))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ActiveWriterSemaphore;
                        token.ThrowIfCancellationRequested();
                        objLoopSemaphore.SafeWait(token);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();

                        // Hold Pending Writer Semaphore before reader lock announcement to prevent edge case weird stuff
                        objLoopSemaphore = PendingWriterSemaphore;
                        objLoopSemaphore.SafeWait(token);

                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                    lngNumReaders);
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    while (!objLoopSemaphore.SafeWait(Utils.SleepEmergencyReleaseMaxTicks, token))
                                    {
                                        if (_lngPendingCountForWriter <= 0)
                                            break; // Just in case we get a rare deadlock that can happen with read locks releasing at just the wrong time
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objCancelledWriterSemaphore;
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
                            if (objLoopSemaphore.CurrentCount == 0)
                                objLoopSemaphore.Release();
                        }
                    }
                }
                catch when (_intDisposedStatus <= 1)
                {
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
            DebuggableSemaphoreSlim objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
            if (objLoopSemaphore.CurrentCount != 0)
                throw new InvalidOperationException("Attempting to upgrade to write lock while not inside of an upgradeable read lock");
            using (new FetchSafelyFromSafeObjectPool<Stack<DebuggableSemaphoreSlim>>(s_objSemaphoreStackPool,
                       out Stack<DebuggableSemaphoreSlim> stkLockedSemaphores))
            {
                try
                {
                    using (CancellationTokenSource objNewTokenSource =
                           CancellationTokenSource.CreateLinkedTokenSource(token, _objDisposalToken))
                    {
                        token = objNewTokenSource.Token;

                        // Acquire the reader lock to lock out readers
                        objLoopSemaphore = ActiveWriterSemaphore;
                        token.ThrowIfCancellationRequested();
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        stkLockedSemaphores.Push(objLoopSemaphore);

                        token.ThrowIfCancellationRequested();
                        // Hold Pending Writer Semaphore before reader lock announcement to prevent edge case weird stuff
                        objLoopSemaphore = PendingWriterSemaphore;
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                        long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                             MaxReaderCount;
                        // Wait for existing readers to exit
                        try
                        {
                            if (lngNumReaders != 0)
                            {
                                long lngPendingCount = Interlocked.Add(ref _lngPendingCountForWriter,
                                    lngNumReaders);
                                if (lngPendingCount != lngNumReaders)
                                    Utils.BreakIfDebug();
                                if (lngPendingCount > 0)
                                {
                                    token.ThrowIfCancellationRequested();
                                    while (!await objLoopSemaphore.WaitAsync(Utils.SleepEmergencyReleaseMaxTicks, token).ConfigureAwait(false))
                                    {
                                        if (_lngPendingCountForWriter <= 0)
                                            break; // Just in case we get a rare deadlock that can happen with read locks releasing at just the wrong time
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Use locals for thread safety
                            DebuggableSemaphoreSlim objLoopSemaphore2 = _objCancelledWriterSemaphore;
                            if (objLoopSemaphore2 != null)
                                await objLoopSemaphore2.WaitAsync(CancellationToken.None).ConfigureAwait(false);
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
                            if (objLoopSemaphore.CurrentCount == 0)
                                objLoopSemaphore.Release();
                        }
                    }
                }
                catch when (_intDisposedStatus <= 1)
                {
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
                using (new FetchSafelyFromSafeObjectPool<Stack<LinkedAsyncRWLockHelper>>(s_objHelperStackPool,
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
                        DebuggableSemaphoreSlim objLoopSemaphore2 = objLoopHelper.ActiveWriterSemaphore;
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

            // Use locals for thread safety
            // Release our reader lock, allowing waiting readers to pass through
            DebuggableSemaphoreSlim objLoopSemaphore = ActiveWriterSemaphore;
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
            objLoopSemaphore = ActiveUpgradeableReaderSemaphore;
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
