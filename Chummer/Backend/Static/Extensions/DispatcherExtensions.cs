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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NLog;

namespace Chummer
{
    public static class DispatcherExtensions
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

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
                    funcToRun.Invoke();
                else
                    Utils.RunOnMainThread(() => funcToRun);
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
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
                    funcToRun.Invoke(null);
                else
                    Utils.RunOnMainThread(() => funcToRun(objDispatcher));
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
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
                    funcToRun.Invoke(token);
                else
                    Utils.RunOnMainThread(() => funcToRun(token), token: token);
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
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
                    funcToRun.Invoke(null, token);
                else
                    Utils.RunOnMainThread(() => funcToRun(objDispatcher, token), token: token);
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
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
        public static Task DoThreadSafeAsync<T>(this T objDispatcher, Action funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objDispatcher == null
                    ? Task.Run(funcToRun, token)
                    : Utils.RunOnMainThreadAsync(funcToRun, token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objDispatcher, Action<T> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objDispatcher == null
                    ? Task.Run(() => funcToRun(null), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher), token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objDispatcher, Action<CancellationToken> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objDispatcher == null
                    ? Task.Run(() => funcToRun(token), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(token), token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code on a DispatcherObject (i.e. any WPF control) in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objDispatcher, Action<T, CancellationToken> funcToRun, CancellationToken token = default) where T : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objDispatcher == null
                    ? Task.Run(() => funcToRun(null, token), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher, token), token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
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
            try
            {
                return objDispatcher == null ? funcToRun.Invoke() : Utils.RunOnMainThread(funcToRun);
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                throw;
            }

            return default;
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
            try
            {
                return objDispatcher == null
                    ? funcToRun.Invoke(null)
                    : Utils.RunOnMainThread(() => funcToRun(objDispatcher));
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                throw;
            }

            return default;
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
            try
            {
                return objDispatcher == null ? funcToRun.Invoke(token) : Utils.RunOnMainThread(() => funcToRun(token), token: token);
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                throw;
            }

            return default;
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
            try
            {
                return objDispatcher == null
                    ? funcToRun.Invoke(null, token)
                    : Utils.RunOnMainThread(() => funcToRun(objDispatcher, token), token: token);
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
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                e = e.Demystify();
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                throw;
            }

            return default;
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objDispatcher == null
                    ? Task.Run(funcToRun, token)
                    : Utils.RunOnMainThreadAsync(funcToRun, token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException<T2>(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<T1, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objDispatcher == null
                    ? Task.Run(() => funcToRun(null), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher), token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException<T2>(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objDispatcher == null
                    ? Task.Run(() => funcToRun(token), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(token), token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException<T2>(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Runs code that returns a value on a DispatcherObject (i.e. any WPF control) in a thread-safe manner.
        /// </summary>
        /// <param name="objDispatcher">Dispatcher object whose dispatcher's Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objDispatcher, Func<T1, CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : DispatcherObject
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objDispatcher == null
                    ? Task.Run(() => funcToRun(null, token), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objDispatcher, token), token);
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
            catch (OperationCanceledException e)
            {
                return Task.FromException<T2>(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowScrollableMessageBox(e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }
    }
}
