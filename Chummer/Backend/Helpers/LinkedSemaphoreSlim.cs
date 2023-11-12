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
//#define LINKEDSEMAPHOREDISPOSEDEBUG
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public sealed class LinkedSemaphoreSlim : IDisposable, IAsyncDisposable
    {
        private int _intDisposedStatus;
        private readonly bool _blnSemaphoreIsPooled;
        private DebuggableSemaphoreSlim _objMySemaphore;
        private LinkedSemaphoreSlim _objParentLinkedSemaphore;

        public DebuggableSemaphoreSlim MySemaphore => _objMySemaphore;

        public LinkedSemaphoreSlim ParentLinkedSemaphore => _objParentLinkedSemaphore;

        private static readonly SafeObjectPool<Stack<LinkedSemaphoreSlim>> s_objSemaphoreStackPool =
            new SafeObjectPool<Stack<LinkedSemaphoreSlim>>(() => new Stack<LinkedSemaphoreSlim>(8));

        public LinkedSemaphoreSlim(LinkedSemaphoreSlim objParent, bool blnGetFromPool = false)
        {
            if (blnGetFromPool)
            {
                _blnSemaphoreIsPooled = true;
                _objMySemaphore = Utils.SemaphorePool.Get();
            }
            else
                _objMySemaphore = new DebuggableSemaphoreSlim();

            _objParentLinkedSemaphore = objParent;
        }

        public LinkedSemaphoreSlim(LinkedSemaphoreSlim objParent, DebuggableSemaphoreSlim objMySemaphore, bool blnSemaphoreIsPooled)
        {
            _objMySemaphore = objMySemaphore ?? throw new ArgumentNullException(nameof(objMySemaphore));
            _blnSemaphoreIsPooled = blnSemaphoreIsPooled;
            _objParentLinkedSemaphore = objParent;
        }

#if LINKEDSEMAPHOREDISPOSEDEBUG
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string DisposeStackTrace { get; set; }
#endif

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;
            DebuggableSemaphoreSlim objMySemaphore = _objMySemaphore;
            if (objMySemaphore == null)
                return;
#if LINKEDSEMAPHOREDISPOSEDEBUG
            DisposeStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            objMySemaphore.SafeWait();
            Interlocked.Increment(ref _intDisposedStatus);
            _objMySemaphore = null;
            _objParentLinkedSemaphore = null;
            objMySemaphore.Release();
            if (_blnSemaphoreIsPooled)
                Utils.SemaphorePool.Return(ref objMySemaphore);
            else
                objMySemaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _intDisposedStatus, 1, 0) > 0)
                return;
            DebuggableSemaphoreSlim objMySemaphore = _objMySemaphore;
            if (objMySemaphore == null)
                return;
#if LINKEDSEMAPHOREDISPOSEDEBUG
            DisposeStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            await objMySemaphore.WaitAsync().ConfigureAwait(false);
            Interlocked.Increment(ref _intDisposedStatus);
            _objMySemaphore = null;
            _objParentLinkedSemaphore = null;
            objMySemaphore.Release();
            if (_blnSemaphoreIsPooled)
                Utils.SemaphorePool.Return(ref objMySemaphore);
            else
                objMySemaphore.Dispose();
        }

        public void WaitAll(LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            MySemaphore.Wait();
            LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
            while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
            {
                objLoopSemaphore.MySemaphore.Wait();
                objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
            }
        }

        public void WaitAll(CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            MySemaphore.Wait(token);
            try
            {
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                {
                    Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                    try
                    {
                        try
                        {
                            while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                            {
                                objLoopSemaphore.MySemaphore.Wait(token);
                                stkLockedSemaphores.Push(objLoopSemaphore);
                                objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                            }
                        }
                        catch
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            throw;
                        }

                        stkLockedSemaphores.Clear();
                    }
                    finally
                    {
                        s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                    }
                }
            }
            catch
            {
                MySemaphore.Release();
                throw;
            }
        }

        public bool WaitAll(TimeSpan timeout, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.Wait(timeout);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!MySemaphore.Wait(timeout))
                    return false;
                TimeSpan elapsed = stpTimer.Elapsed;
                if (elapsed > timeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                timeout -= elapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        stpTimer.Restart();
                        if (!objLoopSemaphore.MySemaphore.Wait(timeout))
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        elapsed = stpTimer.Elapsed;
                        if (elapsed > timeout)
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        timeout -= elapsed;
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public bool WaitAll(TimeSpan timeout, CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.Wait(timeout, token);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!MySemaphore.Wait(timeout, token))
                    return false;
                TimeSpan elapsed = stpTimer.Elapsed;
                if (elapsed > timeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                timeout -= elapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    try
                    {
                        while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            stpTimer.Restart();
                            if (!objLoopSemaphore.MySemaphore.Wait(timeout, token))
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            elapsed = stpTimer.Elapsed;
                            if (elapsed > timeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            timeout -= elapsed;
                            stkLockedSemaphores.Push(objLoopSemaphore);
                            objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                        }
                    }
                    catch
                    {
                        // Release back-to-front to prevent weird race conditions as much as possible
                        while (stkLockedSemaphores.Count > 0)
                            stkLockedSemaphores.Pop().MySemaphore.Release();
                        MySemaphore.Release();
                        throw;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public bool WaitAll(int millisecondsTimeout, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.Wait(millisecondsTimeout);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!MySemaphore.Wait(millisecondsTimeout))
                    return false;
                long millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                if (millisecondsElapsed > millisecondsTimeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                millisecondsTimeout -= (int)millisecondsElapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        stpTimer.Restart();
                        if (!objLoopSemaphore.MySemaphore.Wait(millisecondsTimeout))
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                        if (millisecondsElapsed > millisecondsTimeout)
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        millisecondsTimeout -= (int)millisecondsElapsed;
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public bool WaitAll(int millisecondsTimeout, CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.Wait(millisecondsTimeout, token);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!MySemaphore.Wait(millisecondsTimeout, token))
                    return false;
                long millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                if (millisecondsElapsed > millisecondsTimeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                millisecondsTimeout -= (int)millisecondsElapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    try
                    {
                        while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            stpTimer.Restart();
                            if (!objLoopSemaphore.MySemaphore.Wait(millisecondsTimeout, token))
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                            if (millisecondsElapsed > millisecondsTimeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            millisecondsTimeout -= (int)millisecondsElapsed;
                            stkLockedSemaphores.Push(objLoopSemaphore);
                            objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                        }
                    }
                    catch
                    {
                        // Release back-to-front to prevent weird race conditions as much as possible
                        while (stkLockedSemaphores.Count > 0)
                            stkLockedSemaphores.Pop().MySemaphore.Release();
                        MySemaphore.Release();
                        throw;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public async Task WaitAllAsync(LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            await MySemaphore.WaitAsync().ConfigureAwait(false);
            LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
            while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
            {
                await objLoopSemaphore.MySemaphore.WaitAsync().ConfigureAwait(false);
                objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
            }
        }

        public async Task WaitAllAsync(CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            await MySemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                {
                    Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                    try
                    {
                        try
                        {
                            while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                            {
                                await objLoopSemaphore.MySemaphore.WaitAsync(token).ConfigureAwait(false);
                                stkLockedSemaphores.Push(objLoopSemaphore);
                                objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                            }
                        }
                        catch
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            throw;
                        }

                        stkLockedSemaphores.Clear();
                    }
                    finally
                    {
                        s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                    }
                }
            }
            catch
            {
                MySemaphore.Release();
                throw;
            }
        }

        public async Task<bool> WaitAllAsync(TimeSpan timeout, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return await MySemaphore.WaitAsync(timeout).ConfigureAwait(false);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!await MySemaphore.WaitAsync(timeout).ConfigureAwait(false))
                    return false;
                TimeSpan elapsed = stpTimer.Elapsed;
                if (elapsed > timeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                timeout -= elapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        stpTimer.Restart();
                        if (!await objLoopSemaphore.MySemaphore.WaitAsync(timeout).ConfigureAwait(false))
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        elapsed = stpTimer.Elapsed;
                        if (elapsed > timeout)
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        timeout -= elapsed;
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public async Task<bool> WaitAllAsync(TimeSpan timeout, CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return await MySemaphore.WaitAsync(timeout, token).ConfigureAwait(false);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!await MySemaphore.WaitAsync(timeout, token).ConfigureAwait(false))
                    return false;
                TimeSpan elapsed = stpTimer.Elapsed;
                if (elapsed > timeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                timeout -= elapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    try
                    {
                        while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            stpTimer.Restart();
                            if (!await objLoopSemaphore.MySemaphore.WaitAsync(timeout, token).ConfigureAwait(false))
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            elapsed = stpTimer.Elapsed;
                            if (elapsed > timeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            timeout -= elapsed;
                            stkLockedSemaphores.Push(objLoopSemaphore);
                            objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                        }
                    }
                    catch
                    {
                        // Release back-to-front to prevent weird race conditions as much as possible
                        while (stkLockedSemaphores.Count > 0)
                            stkLockedSemaphores.Pop().MySemaphore.Release();
                        MySemaphore.Release();
                        throw;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public async Task<bool> WaitAllAsync(int millisecondsTimeout, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return await MySemaphore.WaitAsync(millisecondsTimeout).ConfigureAwait(false);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!await MySemaphore.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                    return false;
                long millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                if (millisecondsElapsed > millisecondsTimeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                millisecondsTimeout -= (int)millisecondsElapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        stpTimer.Restart();
                        if (!await objLoopSemaphore.MySemaphore.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                        if (millisecondsElapsed > millisecondsTimeout)
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        millisecondsTimeout -= (int)millisecondsElapsed;
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public async Task<bool> WaitAllAsync(int millisecondsTimeout, CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return await MySemaphore.WaitAsync(millisecondsTimeout, token).ConfigureAwait(false);
            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                if (!await MySemaphore.WaitAsync(millisecondsTimeout, token).ConfigureAwait(false))
                    return false;
                long millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                if (millisecondsElapsed > millisecondsTimeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                millisecondsTimeout -= (int)millisecondsElapsed;
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    try
                    {
                        while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            stpTimer.Restart();
                            if (!await objLoopSemaphore.MySemaphore.WaitAsync(millisecondsTimeout, token)
                                    .ConfigureAwait(false))
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                            if (millisecondsElapsed > millisecondsTimeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            millisecondsTimeout -= (int)millisecondsElapsed;
                            stkLockedSemaphores.Push(objLoopSemaphore);
                            objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                        }
                    }
                    catch
                    {
                        // Release back-to-front to prevent weird race conditions as much as possible
                        while (stkLockedSemaphores.Count > 0)
                            stkLockedSemaphores.Pop().MySemaphore.Release();
                        MySemaphore.Release();
                        throw;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public void SafeWaitAll(LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!MySemaphore.Wait(Utils.DefaultSleepDuration))
                    {
                        if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }

                    try
                    {
                        LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                        if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                            try
                            {
                                try
                                {
                                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                                    {
                                        while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration))
                                        {
                                            if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                                                throw new TimeoutException();
                                            Utils.DoEventsSafe(blnForceDoEvents);
                                        }

                                        stkLockedSemaphores.Push(objLoopSemaphore);
                                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                                    }
                                }
                                catch
                                {
                                    // Release back-to-front to prevent weird race conditions as much as possible
                                    while (stkLockedSemaphores.Count > 0)
                                        stkLockedSemaphores.Pop().MySemaphore.Release();
                                    throw;
                                }

                                stkLockedSemaphores.Clear();
                            }
                            finally
                            {
                                s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                            }
                        }
                    }
                    catch
                    {
                        MySemaphore.Release();
                        throw;
                    }
                }
                else if (!WaitAll(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration, objUntilSemaphore))
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!MySemaphore.Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe(blnForceDoEvents);
                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                {
                    while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe(blnForceDoEvents);
                    objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                }
            }
            else
                WaitAll(objUntilSemaphore);
        }

        public void SafeWaitAll(CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                    {
                        if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }

                    try
                    {
                        LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                        if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                            try
                            {
                                try
                                {
                                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                                    {
                                        while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                                        {
                                            if (intLoopCount++ > Utils.WaitEmergencyReleaseMaxTicks)
                                                throw new TimeoutException();
                                            Utils.DoEventsSafe(blnForceDoEvents);
                                        }

                                        stkLockedSemaphores.Push(objLoopSemaphore);
                                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                                    }
                                }
                                catch
                                {
                                    // Release back-to-front to prevent weird race conditions as much as possible
                                    while (stkLockedSemaphores.Count > 0)
                                        stkLockedSemaphores.Pop().MySemaphore.Release();
                                    throw;
                                }

                                stkLockedSemaphores.Clear();
                            }
                            finally
                            {
                                s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                            }
                        }
                    }
                    catch
                    {
                        MySemaphore.Release();
                        throw;
                    }
                }
                else if (!WaitAll(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration, token, objUntilSemaphore))
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                    Utils.DoEventsSafe(blnForceDoEvents);
                try
                {
                    LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                    if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                        try
                        {
                            try
                            {
                                while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                                {
                                    while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                                        Utils.DoEventsSafe(blnForceDoEvents);
                                    stkLockedSemaphores.Push(objLoopSemaphore);
                                    objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                                }
                            }
                            catch
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                throw;
                            }

                            stkLockedSemaphores.Clear();
                        }
                        finally
                        {
                            s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                        }
                    }
                }
                catch
                {
                    MySemaphore.Release();
                    throw;
                }
            }
            else
                WaitAll(token, objUntilSemaphore);
        }

        public bool SafeWaitAll(TimeSpan timeout, LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));
            if (!Utils.EverDoEvents)
                return WaitAll(timeout, objUntilSemaphore);

            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.SafeWait(timeout, blnForceDoEvents);

            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                while (!MySemaphore.Wait(Utils.DefaultSleepDuration))
                {
                    if (stpTimer.Elapsed > timeout)
                        return false;
                    Utils.DoEventsSafe(blnForceDoEvents);
                }

                TimeSpan elapsed = stpTimer.Elapsed;
                if (elapsed > timeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                timeout -= elapsed;

                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        stpTimer.Restart();
                        while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration))
                        {
                            if (stpTimer.Elapsed > timeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            Utils.DoEventsSafe(blnForceDoEvents);
                        }

                        elapsed = stpTimer.Elapsed;
                        if (elapsed > timeout)
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        timeout -= elapsed;
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public bool SafeWaitAll(TimeSpan timeout, CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));

            if (!Utils.EverDoEvents)
                return WaitAll(timeout, token, objUntilSemaphore);

            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.SafeWait(timeout, token, blnForceDoEvents);

            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                while (!MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                {
                    if (stpTimer.Elapsed > timeout)
                        return false;
                    Utils.DoEventsSafe(blnForceDoEvents);
                }

                TimeSpan elapsed = stpTimer.Elapsed;
                if (elapsed > timeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                timeout -= elapsed;

                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    try
                    {
                        while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            stpTimer.Restart();
                            while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                            {
                                if (stpTimer.Elapsed > timeout)
                                {
                                    // Release back-to-front to prevent weird race conditions as much as possible
                                    while (stkLockedSemaphores.Count > 0)
                                        stkLockedSemaphores.Pop().MySemaphore.Release();
                                    MySemaphore.Release();
                                    return false;
                                }

                                Utils.DoEventsSafe(blnForceDoEvents);
                            }

                            elapsed = stpTimer.Elapsed;
                            if (elapsed > timeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            timeout -= elapsed;
                            stkLockedSemaphores.Push(objLoopSemaphore);
                            objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                        }
                    }
                    catch
                    {
                        // Release back-to-front to prevent weird race conditions as much as possible
                        while (stkLockedSemaphores.Count > 0)
                            stkLockedSemaphores.Pop().MySemaphore.Release();
                        MySemaphore.Release();
                        throw;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public bool SafeWaitAll(int millisecondsTimeout, LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));

            if (!Utils.EverDoEvents)
                return WaitAll(millisecondsTimeout, objUntilSemaphore);

            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.SafeWait(millisecondsTimeout, blnForceDoEvents);

            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                while (!MySemaphore.Wait(Utils.DefaultSleepDuration))
                {
                    if (stpTimer.ElapsedMilliseconds > millisecondsTimeout)
                        return false;
                    Utils.DoEventsSafe(blnForceDoEvents);
                }

                long millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                if (millisecondsElapsed > millisecondsTimeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                millisecondsTimeout -= (int)millisecondsElapsed;

                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                    {
                        stpTimer.Restart();
                        while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration))
                        {
                            if (stpTimer.ElapsedMilliseconds > millisecondsTimeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            Utils.DoEventsSafe(blnForceDoEvents);
                        }

                        millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                        if (millisecondsElapsed > millisecondsTimeout)
                        {
                            // Release back-to-front to prevent weird race conditions as much as possible
                            while (stkLockedSemaphores.Count > 0)
                                stkLockedSemaphores.Pop().MySemaphore.Release();
                            MySemaphore.Release();
                            return false;
                        }

                        millisecondsTimeout -= (int)millisecondsElapsed;
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public bool SafeWaitAll(int millisecondsTimeout, CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
            if (_intDisposedStatus != 0)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));

            if (!Utils.EverDoEvents)
                return WaitAll(millisecondsTimeout, token, objUntilSemaphore);

            if (ParentLinkedSemaphore == null || ParentLinkedSemaphore == objUntilSemaphore)
                return MySemaphore.SafeWait(millisecondsTimeout, token, blnForceDoEvents);

            Stopwatch stpTimer = Utils.StopwatchPool.Get();
            try
            {
                stpTimer.Start();
                while (!MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                {
                    if (stpTimer.ElapsedMilliseconds > millisecondsTimeout)
                        return false;
                    Utils.DoEventsSafe(blnForceDoEvents);
                }

                long millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                if (millisecondsElapsed > millisecondsTimeout)
                {
                    MySemaphore.Release();
                    return false;
                }

                millisecondsTimeout -= (int)millisecondsElapsed;

                LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    try
                    {
                        while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                        {
                            stpTimer.Restart();
                            while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration, token))
                            {
                                if (stpTimer.ElapsedMilliseconds > millisecondsTimeout)
                                {
                                    // Release back-to-front to prevent weird race conditions as much as possible
                                    while (stkLockedSemaphores.Count > 0)
                                        stkLockedSemaphores.Pop().MySemaphore.Release();
                                    MySemaphore.Release();
                                    return false;
                                }

                                Utils.DoEventsSafe(blnForceDoEvents);
                            }

                            millisecondsElapsed = stpTimer.ElapsedMilliseconds;
                            if (millisecondsElapsed > millisecondsTimeout)
                            {
                                // Release back-to-front to prevent weird race conditions as much as possible
                                while (stkLockedSemaphores.Count > 0)
                                    stkLockedSemaphores.Pop().MySemaphore.Release();
                                MySemaphore.Release();
                                return false;
                            }

                            millisecondsTimeout -= (int)millisecondsElapsed;
                            stkLockedSemaphores.Push(objLoopSemaphore);
                            objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                        }
                    }
                    catch
                    {
                        // Release back-to-front to prevent weird race conditions as much as possible
                        while (stkLockedSemaphores.Count > 0)
                            stkLockedSemaphores.Pop().MySemaphore.Release();
                        MySemaphore.Release();
                        throw;
                    }

                    stkLockedSemaphores.Clear();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }

                return true;
            }
            finally
            {
                Utils.StopwatchPool.Return(ref stpTimer);
            }
        }

        public void ReleaseAll(LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            if (_intDisposedStatus > 1)
                throw new ObjectDisposedException(nameof(LinkedSemaphoreSlim));

            LinkedSemaphoreSlim objLoopSemaphore = ParentLinkedSemaphore;
            if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
            {
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore && objLoopSemaphore.MySemaphore.CurrentCount == 0)
                    {
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentLinkedSemaphore;
                    }

                    // Release back-to-front to prevent weird race conditions as much as possible
                    while (stkLockedSemaphores.Count > 0)
                        stkLockedSemaphores.Pop().MySemaphore.Release();
                }
                finally
                {
                    s_objSemaphoreStackPool.Return(ref stkLockedSemaphores);
                }
            }

            if (MySemaphore.CurrentCount == 0)
                MySemaphore.Release();
        }
    }
}
