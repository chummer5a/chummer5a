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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public sealed class LinkedSemaphoreSlim : IDisposable, IAsyncDisposable
    {
        private readonly bool _blnSemaphoreIsPooled;
        private DebuggableSemaphoreSlim _objMySemaphore;

        public DebuggableSemaphoreSlim MySemaphore => _objMySemaphore;
        public LinkedSemaphoreSlim ParentSemaphore { get; }

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

            ParentSemaphore = objParent;
        }

        public LinkedSemaphoreSlim(LinkedSemaphoreSlim objParent, DebuggableSemaphoreSlim objMySemaphore, bool blnSemaphoreIsPooled)
        {
            _objMySemaphore = objMySemaphore ?? throw new ArgumentNullException(nameof(objMySemaphore));
            _blnSemaphoreIsPooled = blnSemaphoreIsPooled;
            ParentSemaphore = objParent;
        }

        public void Dispose()
        {
            MySemaphore.SafeWait();
            MySemaphore.Release();
            if (_blnSemaphoreIsPooled)
                Utils.SemaphorePool.Return(ref _objMySemaphore);
            else
                MySemaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await MySemaphore.WaitAsync().ConfigureAwait(false);
            MySemaphore.Release();
            if (_blnSemaphoreIsPooled)
                Utils.SemaphorePool.Return(ref _objMySemaphore);
            else
                MySemaphore.Dispose();
        }

        public void WaitAll(LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            MySemaphore.Wait();
            LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
            while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
            {
                objLoopSemaphore.MySemaphore.Wait();
                objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
            }
        }

        public void WaitAll(CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            MySemaphore.Wait(token);
            try
            {
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                                objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                            objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                            objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            await MySemaphore.WaitAsync().ConfigureAwait(false);
            LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
            while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
            {
                await objLoopSemaphore.MySemaphore.WaitAsync().ConfigureAwait(false);
                objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
            }
        }

        public async Task WaitAllAsync(CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null)
        {
            await MySemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                                objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                            objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                            objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
                        LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
                while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
                {
                    while (!objLoopSemaphore.MySemaphore.Wait(Utils.DefaultSleepDuration))
                        Utils.DoEventsSafe(blnForceDoEvents);
                    objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
                }
            }
            else
                WaitAll(objUntilSemaphore);
        }

        public void SafeWaitAll(CancellationToken token, LinkedSemaphoreSlim objUntilSemaphore = null, bool blnForceDoEvents = false)
        {
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
                        LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
                    LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                                    objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (!Utils.EverDoEvents)
                return WaitAll(timeout, objUntilSemaphore);

            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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

                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (!Utils.EverDoEvents)
                return WaitAll(timeout, token, objUntilSemaphore);

            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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

                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                            objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (!Utils.EverDoEvents)
                return WaitAll(millisecondsTimeout, objUntilSemaphore);

            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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

                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            if (!Utils.EverDoEvents)
                return WaitAll(millisecondsTimeout, token, objUntilSemaphore);

            if (ParentSemaphore == null || ParentSemaphore == objUntilSemaphore)
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

                LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
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
                            objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
            LinkedSemaphoreSlim objLoopSemaphore = ParentSemaphore;
            if (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore)
            {
                Stack<LinkedSemaphoreSlim> stkLockedSemaphores = s_objSemaphoreStackPool.Get();
                try
                {
                    while (objLoopSemaphore != null && objLoopSemaphore != objUntilSemaphore && objLoopSemaphore.MySemaphore.CurrentCount == 0)
                    {
                        stkLockedSemaphores.Push(objLoopSemaphore);
                        objLoopSemaphore = objLoopSemaphore.ParentSemaphore;
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
