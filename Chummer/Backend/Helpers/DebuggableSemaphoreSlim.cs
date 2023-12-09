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

// Uncomment this define to control whether or not stacktraces should be saved every time a semaphore is successfully acquired or disposed.
#if DEBUG
//#define SEMAPHOREDEBUG
#endif

using System;
#if SEMAPHOREDEBUG
using System.Diagnostics;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Version of SemaphoreSlim(1, 1) with a surrounding wrapper that can help with debugging by saving the current stacktrace every time the sole lock is acquired or the semaphore is disposed
    /// </summary>
    public sealed class DebuggableSemaphoreSlim : IDisposable
    {
        private readonly bool _blnDisposeSemaphore;
        private readonly SemaphoreSlim _objSemaphoreSlim;

#if SEMAPHOREDEBUG
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string LastHolderStackTrace { get; set; }
#endif
        // ReSharper disable once UnusedMember.Local
        //private readonly string _strGuid = Guid.NewGuid().ToString("D");

        public DebuggableSemaphoreSlim()
        {
            _objSemaphoreSlim = new SemaphoreSlim(1, 1);
            _blnDisposeSemaphore = true;
        }

        public DebuggableSemaphoreSlim(SemaphoreSlim objSemaphoreSlim)
        {
            _objSemaphoreSlim = objSemaphoreSlim;
        }

        /// <inheritdoc cref="SemaphoreSlim.Wait()"/>
        public void Wait()
        {
            _objSemaphoreSlim.Wait();
#if SEMAPHOREDEBUG
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
#endif
        }

        /// <inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/>
        public void Wait(CancellationToken token)
        {
            _objSemaphoreSlim.Wait(token);
#if SEMAPHOREDEBUG
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
#endif
        }

        /// <inheritdoc cref="SemaphoreSlim.Wait(TimeSpan)"/>
        public bool Wait(TimeSpan timeout)
        {
#if SEMAPHOREDEBUG
            if (!_objSemaphoreSlim.Wait(timeout))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;

#else
            return _objSemaphoreSlim.Wait(timeout);
#endif
        }

        /// <inheritdoc cref="SemaphoreSlim.Wait(TimeSpan, CancellationToken)"/>
        public bool Wait(TimeSpan timeout, CancellationToken token)
        {
#if SEMAPHOREDEBUG
            if (!_objSemaphoreSlim.Wait(timeout, token))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;

#else
            return _objSemaphoreSlim.Wait(timeout, token);
#endif
        }

        /// <inheritdoc cref="SemaphoreSlim.Wait(int)"/>
        public bool Wait(int millisecondsTimeout)
        {
#if SEMAPHOREDEBUG
            if (!_objSemaphoreSlim.Wait(millisecondsTimeout))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;

#else
            return _objSemaphoreSlim.Wait(millisecondsTimeout);
#endif
        }

        /// <inheritdoc cref="SemaphoreSlim.Wait(int, CancellationToken)"/>
        public bool Wait(int millisecondsTimeout, CancellationToken token)
        {
#if SEMAPHOREDEBUG
            if (!_objSemaphoreSlim.Wait(millisecondsTimeout, token))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;

#else
            return _objSemaphoreSlim.Wait(millisecondsTimeout, token);
#endif
        }

#if SEMAPHOREDEBUG
        /// <inheritdoc cref="SemaphoreSlim.WaitAsync()"/>
        public async Task WaitAsync()
        {
            if (Utils.IsUnitTest)
            {
                if (!await _objSemaphoreSlim.WaitAsync(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration)
                        .ConfigureAwait(false))
                    throw new TimeoutException();
            }
            else
                await _objSemaphoreSlim.WaitAsync().ConfigureAwait(false);
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(CancellationToken)"/>
        public async Task WaitAsync(CancellationToken token)
        {
            if (Utils.IsUnitTest)
            {
                if (!await _objSemaphoreSlim.WaitAsync(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration, token)
                        .ConfigureAwait(false) && !token.IsCancellationRequested)
                    throw new TimeoutException();
            }
            else
                await _objSemaphoreSlim.WaitAsync(token).ConfigureAwait(false);
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan)"/>
        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            if (!await _objSemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan, CancellationToken)"/>
        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken token)
        {
            if (!await _objSemaphoreSlim.WaitAsync(timeout, token).ConfigureAwait(false))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(int)"/>
        public async Task<bool> WaitAsync(int millisecondsTimeout)
        {
            if (!await _objSemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)"/>
        public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken token)
        {
            if (!await _objSemaphoreSlim.WaitAsync(millisecondsTimeout, token).ConfigureAwait(false))
                return false;
            LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
            return true;
        }
#else

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync()"/>
        public async Task WaitAsync()
        {
            if (Utils.IsUnitTest)
            {
                if (!await _objSemaphoreSlim.WaitAsync(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration)
                        .ConfigureAwait(false))
                    throw new TimeoutException();
            }
            else
                await _objSemaphoreSlim.WaitAsync().ConfigureAwait(false);
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(CancellationToken)"/>
        public async Task WaitAsync(CancellationToken token)
        {
            if (Utils.IsUnitTest)
            {
                if (!await _objSemaphoreSlim
                        .WaitAsync(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration, token)
                        .ConfigureAwait(false) && !token.IsCancellationRequested)
                    throw new TimeoutException();
            }
            else
                await _objSemaphoreSlim.WaitAsync(token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan)"/>
        public Task<bool> WaitAsync(TimeSpan timeout)
        {
            return _objSemaphoreSlim.WaitAsync(timeout);
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan, CancellationToken)"/>
        public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken token)
        {
            return _objSemaphoreSlim.WaitAsync(timeout, token);
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(int)"/>
        public Task<bool> WaitAsync(int millisecondsTimeout)
        {
            return _objSemaphoreSlim.WaitAsync(millisecondsTimeout);
        }

        /// <inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)"/>
        public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken token)
        {
            return _objSemaphoreSlim.WaitAsync(millisecondsTimeout, token);
        }

#endif

        /// <summary>
        /// Version of SemaphoreSlim::Wait() that also processes application events if this is called on the UI thread
        /// </summary>
        public void SafeWait(bool blnForceDoEvents = false)
        {
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!Wait(Utils.DefaultSleepDuration))
                    {
                        if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }
                }
                else if (!Wait(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration))
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!Wait(Utils.DefaultSleepDuration))
                    Utils.DoEventsSafe(blnForceDoEvents);
            }
            else
                Wait();
        }

        /// <summary>
        /// Version of SemaphoreSlim::Wait() that also processes application events if this is called on the UI thread
        /// </summary>
        public void SafeWait(CancellationToken token, bool blnForceDoEvents = false)
        {
            if (Utils.IsUnitTest)
            {
                if (Utils.EverDoEvents)
                {
                    int intLoopCount = 0;
                    while (!Wait(Utils.DefaultSleepDuration, token))
                    {
                        if (++intLoopCount > Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration)
                            throw new TimeoutException();
                        Utils.DoEventsSafe(blnForceDoEvents);
                    }
                }
                else if (!Wait(Utils.WaitEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration, token) &&
                         !token.IsCancellationRequested)
                    throw new TimeoutException();
            }
            else if (Utils.EverDoEvents)
            {
                while (!Wait(Utils.DefaultSleepDuration, token))
                    Utils.DoEventsSafe(blnForceDoEvents);
            }
            else
                Wait(token);
        }

        /// <summary>
        /// Version of SemaphoreSlim::Wait() that also processes application events if this is called on the UI thread
        /// </summary>
        public bool SafeWait(TimeSpan timeout, bool blnForceDoEvents = false)
        {
            if (!Utils.EverDoEvents)
                return Wait(timeout);

            for (int i = timeout.Milliseconds; i >= 0; i -= Utils.DefaultSleepDuration)
            {
                if (Wait(Math.Min(Utils.DefaultSleepDuration, i)))
                    return true;
                Utils.DoEventsSafe(blnForceDoEvents);
            }

            return false;
        }

        /// <summary>
        /// Version of SemaphoreSlim::Wait() that also processes application events if this is called on the UI thread
        /// </summary>
        public bool SafeWait(TimeSpan timeout, CancellationToken token, bool blnForceDoEvents = false)
        {
            if (!Utils.EverDoEvents)
                return Wait(timeout, token);

            for (int i = timeout.Milliseconds; i >= 0; i -= Utils.DefaultSleepDuration)
            {
                if (Wait(Math.Min(Utils.DefaultSleepDuration, i), token))
                    return true;
                Utils.DoEventsSafe(blnForceDoEvents);
            }

            return false;
        }

        /// <summary>
        /// Version of SemaphoreSlim::Wait() that also processes application events if this is called on the UI thread
        /// </summary>
        public bool SafeWait(int millisecondsTimeout, bool blnForceDoEvents = false)
        {
            if (!Utils.EverDoEvents)
                return Wait(millisecondsTimeout);

            for (int i = millisecondsTimeout; i >= 0; i -= Utils.DefaultSleepDuration)
            {
                if (Wait(Math.Min(Utils.DefaultSleepDuration, i)))
                    return true;
                Utils.DoEventsSafe(blnForceDoEvents);
            }

            return false;
        }

        /// <summary>
        /// Version of SemaphoreSlim::Wait() that also processes application events if this is called on the UI thread
        /// </summary>
        public bool SafeWait(int millisecondsTimeout, CancellationToken token, bool blnForceDoEvents = false)
        {
            if (!Utils.EverDoEvents)
                return Wait(millisecondsTimeout, token);

            for (int i = millisecondsTimeout; i >= 0; i -= Utils.DefaultSleepDuration)
            {
                if (Wait(Math.Min(Utils.DefaultSleepDuration, i), token))
                    return true;
                Utils.DoEventsSafe(blnForceDoEvents);
            }

            return false;
        }

        /// <inheritdoc cref="SemaphoreSlim.Release()"/>
        public void Release()
        {
            _objSemaphoreSlim.Release();
#if SEMAPHOREDEBUG
            LastHolderStackTrace = string.Empty;
#endif
        }

        /// <inheritdoc cref="SemaphoreSlim.Release(int)"/>
        public void Release(int releaseCount)
        {
            _objSemaphoreSlim.Release(releaseCount);
#if SEMAPHOREDEBUG
            LastHolderStackTrace = string.Empty;
#endif
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_blnDisposeSemaphore)
            {
                _objSemaphoreSlim.Dispose();
#if SEMAPHOREDEBUG
                LastHolderStackTrace = EnhancedStackTrace.Current().ToString();
#endif
            }
        }

        /// <inheritdoc cref="SemaphoreSlim.CurrentCount"/>
        public int CurrentCount => _objSemaphoreSlim.CurrentCount;

        /// <inheritdoc cref="SemaphoreSlim.AvailableWaitHandle"/>
        public WaitHandle AvailableWaitHandle => _objSemaphoreSlim.AvailableWaitHandle;
    }
}
