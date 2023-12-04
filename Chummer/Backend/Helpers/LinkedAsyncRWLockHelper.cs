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

// Uncomment this define to control whether or not stacktraces should be saved every time a linked semaphore is successfully disposed.
#if DEBUG
//#define LINKEDSEMAPHOREDEBUG
#endif

using System;
using System.Collections.Generic;
#if LINKEDSEMAPHOREDEBUG
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
        private const long MaxReaderCount = int.MaxValue;
        private DebuggableSemaphoreSlim _objPendingWriterSemaphore;
        private DebuggableSemaphoreSlim _objUpgradeableReaderSemaphore;
        private DebuggableSemaphoreSlim _objReaderSemaphore;
        private long _lngNumReaders;
        private long _lngPendingCountForWriter;

        private LinkedAsyncRWLockHelper _objParentLinkedHelper;

        public DebuggableSemaphoreSlim PendingWriterSemaphore => _objPendingWriterSemaphore;
        public DebuggableSemaphoreSlim UpgradeableReaderSemaphore => _objUpgradeableReaderSemaphore;
        public DebuggableSemaphoreSlim ReaderSemaphore => _objReaderSemaphore;

        public LinkedAsyncRWLockHelper ParentLinkedHelper => _objParentLinkedHelper;

        private static readonly SafeObjectPool<Stack<LinkedAsyncRWLockHelper>> s_objHelperStackPool =
            new SafeObjectPool<Stack<LinkedAsyncRWLockHelper>>(() => new Stack<LinkedAsyncRWLockHelper>(8));

        private static readonly SafeObjectPool<Stack<DebuggableSemaphoreSlim>> s_objSemaphoreStackPool =
            new SafeObjectPool<Stack<DebuggableSemaphoreSlim>>(() => new Stack<DebuggableSemaphoreSlim>(8));

        private static readonly SafeObjectPool<Stack<Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>>>
            s_objWriterLockHelperStackPool =
                new SafeObjectPool<Stack<Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>>>(() =>
                    new Stack<Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>>(8));

        public bool IsDisposed => _intDisposedStatus > 0;

        public LinkedAsyncRWLockHelper(LinkedAsyncRWLockHelper objParent, bool blnGetFromPool = true)
        {
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
                _objHasChildrenSemaphore.SafeWait();
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
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;
            // Acquire all of our locks
            DebuggableSemaphoreSlim objUpgradeableReaderSemaphore = Interlocked.Exchange(ref _objUpgradeableReaderSemaphore, null);
            objUpgradeableReaderSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objReaderSemaphore = Interlocked.Exchange(ref _objReaderSemaphore, null);
            objReaderSemaphore?.SafeWait();
            DebuggableSemaphoreSlim objPendingWriterSemaphore = Interlocked.Exchange(ref _objPendingWriterSemaphore, null);
            objPendingWriterSemaphore?.SafeWait();
#if LINKEDSEMAPHOREDEBUG
            while (!_setChildren.IsEmpty)
                Utils.SafeSleep();
#endif

            DebuggableSemaphoreSlim objHasChildrenSemaphore = Interlocked.Exchange(ref _objHasChildrenSemaphore, null);
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

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;
            // Acquire all of our locks
            DebuggableSemaphoreSlim objUpgradeableReaderSemaphore = Interlocked.Exchange(ref _objUpgradeableReaderSemaphore, null);
            if (objUpgradeableReaderSemaphore != null)
                await objUpgradeableReaderSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objReaderSemaphore = Interlocked.Exchange(ref _objReaderSemaphore, null);
            if (objReaderSemaphore != null)
                await objReaderSemaphore.WaitAsync().ConfigureAwait(false);
            DebuggableSemaphoreSlim objPendingWriterSemaphore = Interlocked.Exchange(ref _objPendingWriterSemaphore, null);
            if (objPendingWriterSemaphore != null)
                await objPendingWriterSemaphore.WaitAsync().ConfigureAwait(false);
#if LINKEDSEMAPHOREDEBUG
            while (!_setChildren.IsEmpty)
                Utils.SafeSleep();
#endif

            DebuggableSemaphoreSlim objHasChildrenSemaphore = Interlocked.Exchange(ref _objHasChildrenSemaphore, null);
            if (objHasChildrenSemaphore != null)
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
                Utils.SemaphorePool.Return(ref objHasChildrenSemaphore);
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
            long lngNumReaders = Interlocked.Increment(ref _lngNumReaders);
            // Negative = there's a writer lock that's waiting or active
            if (lngNumReaders < 0)
            {
                if (blnSkipSemaphore)
                {
                    // We aren't blocked because we are a re-entrant reader lock, so we should make sure we up the pending count as well
                    Interlocked.Increment(ref _lngPendingCountForWriter);
                }
                else
                {
                    // Use local for thread safety
                    DebuggableSemaphoreSlim objReaderSemaphore = ReaderSemaphore;
                    try
                    {
                        objReaderSemaphore.SafeWait(token);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _lngNumReaders);
                        throw;
                    }

                    objReaderSemaphore.Release();
                }
            }
        }

        public async Task TakeReadLockAsync(bool blnSkipSemaphore, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngNumReaders = Interlocked.Increment(ref _lngNumReaders);
            // Negative = there's a writer lock that's waiting or active
            if (lngNumReaders < 0)
            {
                if (blnSkipSemaphore)
                {
                    // We aren't blocked because we are a re-entrant reader lock, so we should make sure we up the pending count as well
                    Interlocked.Increment(ref _lngPendingCountForWriter);
                }
                else
                {
                    // Use local for thread safety
                    DebuggableSemaphoreSlim objReaderSemaphore = ReaderSemaphore;
                    try
                    {
                        await objReaderSemaphore.WaitAsync(token).ConfigureAwait(false);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _lngNumReaders);
                        throw;
                    }
                    objReaderSemaphore.Release();
                }
            }
        }

        public void ReleaseReadLock()
        {
            long lngNumReaders = Interlocked.Decrement(ref _lngNumReaders);
            // A writer is pending, signal them that a reader lock has exited
            if (lngNumReaders < 0 && Interlocked.Decrement(ref _lngPendingCountForWriter) == 0)
            {
                try
                {
                    _objPendingWriterSemaphore.Release();
                }
                catch (SemaphoreFullException)
                {
                    // swallow this, we somehow triggered multiple releases in succession
                }
            }
        }

        public void TakeUpgradeableReadLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Use locals for thread safety
            // Acquire the upgradeable read lock that also prevents all other locks besides read locks
            DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
            objLoopSemaphore.SafeWait(token);
        }

        public Task TakeUpgradeableReadLockAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            // Use locals for thread safety
            // Acquire the upgradeable read lock that also prevents all other locks besides read locks
            DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
            return objLoopSemaphore.WaitAsync(token);
        }

        public void ReleaseUpgradeableReadLock()
        {
            // Use locals for thread safety
            DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
            try
            {
                // Release our upgradeable read lock, allowing pending upgradeable reader locks and writer locks through
                objLoopSemaphore?.Release();
            }
            catch (SemaphoreFullException)
            {
                // swallow this
            }
        }

        public void TakeWriteLock(LinkedAsyncRWLockHelper objTopMostHeldWriter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Stack<Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>> stkUndo = s_objWriterLockHelperStackPool.Get();
            try
            {
                // Use locals for thread safety
                DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                objLoopSemaphore.SafeWait(token);
                stkUndo.Push(new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(null, 0, objLoopSemaphore));
                // Since we can only be re-entrant to an upgradeable reader or a writer, both of these will have the semaphore already set, so we don't need to go up the chain

                token.ThrowIfCancellationRequested();

                LinkedAsyncRWLockHelper objLoopHelper = this;
                while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                {
                    // Acquire the reader lock to lock out readers
                    token.ThrowIfCancellationRequested();
                    objLoopSemaphore = objLoopHelper.ReaderSemaphore;
                    objLoopSemaphore.SafeWait(token);
                    stkUndo.Push(
                        new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(null, 0, objLoopSemaphore));
                    token.ThrowIfCancellationRequested();
                    // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                    long lngNumReaders = Interlocked.Add(ref objLoopHelper._lngNumReaders, -MaxReaderCount) +
                                         MaxReaderCount;
                    if (lngNumReaders != 0 &&
                        Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter, lngNumReaders) != 0)
                    {
                        // Wait for existing readers to exit
                        objLoopSemaphore = objLoopHelper.PendingWriterSemaphore;
                        try
                        {
                            objLoopSemaphore.SafeWait(token);
                        }
                        catch
                        {
                            stkUndo.Push(
                                new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(objLoopHelper,
                                    lngNumReaders, null));
                            throw;
                        }

                        stkUndo.Push(new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(objLoopHelper,
                            lngNumReaders, objLoopSemaphore));
                    }
                    else
                        stkUndo.Push(
                            new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(objLoopHelper,
                                lngNumReaders, null));

                    objLoopHelper = objLoopHelper.ParentLinkedHelper;
                }
            }
            catch
            {
                while (stkUndo.Count > 0)
                {
                    (LinkedAsyncRWLockHelper objLoopHelper, long lngNumReaders, DebuggableSemaphoreSlim objLoopSemaphore) = stkUndo.Pop();
                    try
                    {
                        objLoopSemaphore?.Release();
                        if (objLoopHelper != null)
                            Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter, -lngNumReaders);
                    }
                    catch (SemaphoreFullException)
                    {
                        // swallow this
                        if (objLoopHelper != null)
                            objLoopHelper._lngPendingCountForWriter = 0;
                    }

                    if (objLoopHelper != null)
                        Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                }

                throw;
            }
            finally
            {
                s_objWriterLockHelperStackPool.Return(ref stkUndo);
            }
        }

        public async Task TakeWriteLockAsync(LinkedAsyncRWLockHelper objTopMostHeldWriter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Stack<Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>> stkUndo = s_objWriterLockHelperStackPool.Get();
            try
            {
                // Use locals for thread safety
                DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                stkUndo.Push(new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(null, 0, objLoopSemaphore));
                // Since we can only be re-entrant to an upgradeable reader or a writer, both of these will have the semaphore already set, so we don't need to go up the chain

                token.ThrowIfCancellationRequested();

                LinkedAsyncRWLockHelper objLoopHelper = this;
                while (objLoopHelper != null && objLoopHelper != objTopMostHeldWriter)
                {
                    // Acquire the reader lock to lock out readers
                    token.ThrowIfCancellationRequested();
                    objLoopSemaphore = objLoopHelper.ReaderSemaphore;
                    await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                    stkUndo.Push(
                        new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(null, 0, objLoopSemaphore));
                    token.ThrowIfCancellationRequested();
                    // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                    long lngNumReaders = Interlocked.Add(ref objLoopHelper._lngNumReaders, -MaxReaderCount) +
                                         MaxReaderCount;
                    if (lngNumReaders != 0 &&
                        Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter, lngNumReaders) != 0)
                    {
                        // Wait for existing readers to exit
                        objLoopSemaphore = objLoopHelper.PendingWriterSemaphore;
                        try
                        {
                            await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                        }
                        catch
                        {
                            stkUndo.Push(
                                new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(objLoopHelper,
                                    lngNumReaders, null));
                            throw;
                        }

                        stkUndo.Push(new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(objLoopHelper,
                            lngNumReaders, objLoopSemaphore));
                    }
                    else
                        stkUndo.Push(
                            new Tuple<LinkedAsyncRWLockHelper, long, DebuggableSemaphoreSlim>(objLoopHelper,
                                lngNumReaders, null));

                    objLoopHelper = objLoopHelper.ParentLinkedHelper;
                }
            }
            catch
            {
                while (stkUndo.Count > 0)
                {
                    (LinkedAsyncRWLockHelper objLoopHelper, long lngNumReaders, DebuggableSemaphoreSlim objLoopSemaphore) = stkUndo.Pop();
                    try
                    {
                        objLoopSemaphore?.Release();
                        if (objLoopHelper != null)
                            Interlocked.Add(ref objLoopHelper._lngPendingCountForWriter, -lngNumReaders);
                    }
                    catch (SemaphoreFullException)
                    {
                        // swallow this
                        if (objLoopHelper != null)
                            objLoopHelper._lngPendingCountForWriter = 0;
                    }

                    if (objLoopHelper != null)
                        Interlocked.Add(ref objLoopHelper._lngNumReaders, MaxReaderCount);
                }

                throw;
            }
            finally
            {
                s_objWriterLockHelperStackPool.Return(ref stkUndo);
            }
        }

        public void TakeSingleWriteLock(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Stack<DebuggableSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
            try
            {
                // Use locals for thread safety
                // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                objLoopSemaphore.SafeWait(token);
                stkLockedSemaphores.Push(objLoopSemaphore);

                token.ThrowIfCancellationRequested();

                // Acquire the reader lock to lock out readers
                objLoopSemaphore = ReaderSemaphore;
                objLoopSemaphore.SafeWait(token);
                stkLockedSemaphores.Push(objLoopSemaphore);

                token.ThrowIfCancellationRequested();

                // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                     MaxReaderCount;
                if (lngNumReaders != 0 &&
                    Interlocked.Add(ref _lngPendingCountForWriter, lngNumReaders) != 0)
                {
                    // Wait for existing readers to exit
                    objLoopSemaphore = PendingWriterSemaphore;
                    try
                    {
                        objLoopSemaphore.SafeWait(token);
                    }
                    catch
                    {
                        Interlocked.Add(ref _lngPendingCountForWriter, -lngNumReaders);
                        Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                        throw;
                    }
                }
            }
            catch
            {
                while (stkLockedSemaphores.Count > 0)
                {
                    try
                    {
                        stkLockedSemaphores.Pop().Release();
                    }
                    catch (SemaphoreFullException)
                    {
                        // swallow this
                    }
                }

                throw;
            }
            finally
            {
                s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
            }
        }

        public async Task TakeSingleWriteLockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Stack<DebuggableSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
            try
            {
                // Use locals for thread safety
                // First lock to prevent any more upgradeable read locks (also makes sure only one writer lock is queued at a time)
                DebuggableSemaphoreSlim objLoopSemaphore = UpgradeableReaderSemaphore;
                await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                stkLockedSemaphores.Push(objLoopSemaphore);

                token.ThrowIfCancellationRequested();

                // Acquire the reader lock to lock out readers
                objLoopSemaphore = ReaderSemaphore;
                await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                stkLockedSemaphores.Push(objLoopSemaphore);

                token.ThrowIfCancellationRequested();

                // Announce that we have acquired the reader lock, preventing new reader locks from being acquired
                long lngNumReaders = Interlocked.Add(ref _lngNumReaders, -MaxReaderCount) +
                                     MaxReaderCount;
                if (lngNumReaders != 0 &&
                    Interlocked.Add(ref _lngPendingCountForWriter, lngNumReaders) != 0)
                {
                    // Wait for existing readers to exit
                    objLoopSemaphore = PendingWriterSemaphore;
                    try
                    {
                        await objLoopSemaphore.WaitAsync(token).ConfigureAwait(false);
                    }
                    catch
                    {
                        Interlocked.Add(ref _lngPendingCountForWriter, -lngNumReaders);
                        Interlocked.Add(ref _lngNumReaders, MaxReaderCount);
                        throw;
                    }
                }
            }
            catch
            {
                while (stkLockedSemaphores.Count > 0)
                {
                    try
                    {
                        stkLockedSemaphores.Pop().Release();
                    }
                    catch (SemaphoreFullException)
                    {
                        // swallow this
                    }
                }

                throw;
            }
            finally
            {
                s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
            }
        }

        public void ReleaseSingleWriteLock()
        {
            ReleaseWriteLock(null, null);
        }

        public void ReleaseWriteLock(LinkedAsyncRWLockHelper objTopMostHeldUReader, LinkedAsyncRWLockHelper objTopMostHeldWriter)
        {
            // There are upgradeable readers into which we have re-entered, so go up the chain
            if (objTopMostHeldUReader != null)
            {
                Stack<LinkedAsyncRWLockHelper> stkLockedUReaderHelpers = s_objHelperStackPool.Get();
                try
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

                        // Use locals for thread safety
                        // Release our reader lock, allowing waiting readers to pass through
                        DebuggableSemaphoreSlim objLoopSemaphore2 = objLoopHelper.ReaderSemaphore;
                        try
                        {
                            objLoopSemaphore2?.Release();
                        }
                        catch (SemaphoreFullException)
                        {
                            // swallow this
                        }
                    }
                }
                finally
                {
                    s_objHelperStackPool.Return(ref stkLockedUReaderHelpers);
                }
            }

            // Announce to readers that we are no longer active
            Interlocked.Add(ref _lngNumReaders, MaxReaderCount);

            // Use locals for thread safety
            // Release our reader lock, allowing waiting readers to pass through
            DebuggableSemaphoreSlim objLoopSemaphore = ReaderSemaphore;
            try
            {
                objLoopSemaphore?.Release();
            }
            catch (SemaphoreFullException)
            {
                // swallow this
            }

            // Release our upgradeable read lock, allowing pending upgradeable reader locks and writer locks through
            objLoopSemaphore = UpgradeableReaderSemaphore;
            try
            {
                objLoopSemaphore?.Release();
            }
            catch (SemaphoreFullException)
            {
                // swallow this
            }
        }
    }
}
