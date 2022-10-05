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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NLog;

namespace Chummer
{
    public static class DispatcherExtensions
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objDispatcher, Action funcToRun) where T : DispatcherObject
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke();
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                        funcToRun.Invoke();
                    else
                        objDispatcherCopy.Dispatcher.Invoke(funcToRun);
                }
#else
                else
                    Utils.RunOnMainThread(() => funcToRun);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objDispatcher, Action<T> funcToRun) where T : DispatcherObject
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke(null);
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                        funcToRun.Invoke(objDispatcherCopy);
                    else
                        objDispatcherCopy.Dispatcher.Invoke(funcToRun, objDispatcherCopy);
                }
#else
                else
                    Utils.RunOnMainThread(() => funcToRun(objDispatcher));
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objDispatcher, Action<CancellationToken> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke(token);
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        funcToRun.Invoke(token);
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        objDispatcherCopy.Dispatcher.Invoke(funcToRun, token);
                    }
                }
#else
                else
                    Utils.RunOnMainThread(() => funcToRun(token), token);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objDispatcher, Action<T, CancellationToken> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke(null, token);
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        funcToRun.Invoke(objDispatcherCopy, token);
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        objDispatcherCopy.Dispatcher.Invoke(funcToRun, objDispatcherCopy, token);
                    }
                }
#else
                else
                    Utils.RunOnMainThread(() => funcToRun(objDispatcher, token), token);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task DoThreadSafeAsync<T>(this T objDispatcher, Action funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke();
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        funcToRun.Invoke();
                    }
                    else
                    {
                        await objDispatcherCopy.Dispatcher.InvokeAsync(funcToRun, DispatcherPriority.Normal, token).Task.ConfigureAwait(false);
                    }
                }
#else
                else
                    await Utils.RunOnMainThreadAsync(funcToRun, token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task DoThreadSafeAsync<T>(this T objDispatcher, Action<T> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke(null);
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        funcToRun.Invoke(objDispatcherCopy);
                    }
                    else if (token != default)
                    {
                        DispatcherOperation objOperation = objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, objDispatcherCopy);
                        using (token.Register(() => objOperation.Abort()))
                            await objOperation.Task.ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        await objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, objDispatcherCopy).Task.ConfigureAwait(false);
                    }
                }
#else
                else
                    await Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher), token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task DoThreadSafeAsync<T>(this T objDispatcher, Action<CancellationToken> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke(token);
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        funcToRun.Invoke(token);
                    }
                    else if (token != default)
                    {
                        DispatcherOperation objOperation = objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, token);
                        using (token.Register(() => objOperation.Abort()))
                            await objOperation.Task.ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        await objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, token).Task.ConfigureAwait(false);
                    }
                }
#else
                else
                    await Utils.RunOnMainThreadAsync(() => funcToRun(token), token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task DoThreadSafeAsync<T>(this T objDispatcher, Action<T, CancellationToken> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objDispatcher == null)
                {
                    funcToRun.Invoke(null, token);
                }
#if USE_INVOKE
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        funcToRun.Invoke(objDispatcherCopy, token);
                    }
                    else if (token != default)
                    {
                        DispatcherOperation objOperation = objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, objDispatcherCopy, token);
                        using (token.Register(() => objOperation.Abort()))
                            await objOperation.Task.ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        await objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, objDispatcherCopy, token).Task.ConfigureAwait(false);
                    }
                }
#else
                else
                    await Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher, token), token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objDispatcher, Func<T2> funcToRun) where T1 : DispatcherObject
        {
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke();
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    objReturn = objDispatcherCopy.CheckAccess()
                        ? funcToRun.Invoke()
                        : objDispatcherCopy.Dispatcher.Invoke(funcToRun);
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke() : Utils.RunOnMainThread(funcToRun);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objDispatcher, Func<T1, T2> funcToRun) where T1 : DispatcherObject
        {
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke(null);
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                        objReturn = funcToRun.Invoke(objDispatcherCopy);
                    else
                    {
                        switch (objDispatcherCopy.Dispatcher.Invoke(funcToRun, objDispatcherCopy))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke(null) : Utils.RunOnMainThread(() => funcToRun(objDispatcher));
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objDispatcher, Func<CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke(token);
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = funcToRun.Invoke(token);
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        switch (objDispatcherCopy.Dispatcher.Invoke(funcToRun, token))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke(token) : Utils.RunOnMainThread(() => funcToRun(token));
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objDispatcher, Func<T1, CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke(null, token);
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = funcToRun.Invoke(objDispatcherCopy, token);
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        switch (objDispatcherCopy.Dispatcher.Invoke(funcToRun, objDispatcherCopy, token))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke(null, token) : Utils.RunOnMainThread(() => funcToRun(objDispatcher, token));
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke();
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = funcToRun.Invoke();
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = await objDispatcherCopy.Dispatcher.InvokeAsync(funcToRun, DispatcherPriority.Normal, token).Task.ConfigureAwait(false);
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke() : await Utils.RunOnMainThreadAsync(funcToRun, token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<T1, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke(null);
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = funcToRun.Invoke(objDispatcherCopy);
                    }
                    else
                    {
                        DispatcherOperation objOperation = objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, objDispatcherCopy);
                        if (token != default)
                        {
                            using (token.Register(() => objOperation.Abort()))
                                await objOperation.Task.ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            await objOperation.Task.ConfigureAwait(false);
                        }
                        switch (objOperation.Result)
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke(null) : await Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher), token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke(token);
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = funcToRun.Invoke(token);
                    }
                    else
                    {
                        DispatcherOperation objOperation = objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, token);
                        if (token != default)
                        {
                            using (token.Register(() => objOperation.Abort()))
                                await objOperation.Task.ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            await objOperation.Task.ConfigureAwait(false);
                        }
                        switch (objOperation.Result)
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke(token) : await Utils.RunOnMainThreadAsync(() => funcToRun(token), token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<T1, CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
#if USE_INVOKE
                if (objDispatcher == null)
                    objReturn = funcToRun.Invoke(null, token);
                else
                {
                    T1 objDispatcherCopy = objDispatcher; //to have the Object for sure, regardless of other threads
                    if (objDispatcherCopy.CheckAccess())
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = funcToRun.Invoke(objDispatcherCopy, token);
                    }
                    else
                    {
                        DispatcherOperation objOperation = objDispatcherCopy.Dispatcher.BeginInvoke(funcToRun, objDispatcherCopy, token);
                        if (token != default)
                        {
                            using (token.Register(() => objOperation.Abort()))
                                await objOperation.Task.ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            await objOperation.Task.ConfigureAwait(false);
                        }
                        switch (objOperation.Result)
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                }
#else
                objReturn = objDispatcher == null ? funcToRun.Invoke(null, token) : await Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher, token), token).ConfigureAwait(false);
#endif
            }
            catch (ObjectDisposedException) // e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(e.ToString());
#endif
                throw;
            }

            return objReturn;
        }
    }
}
