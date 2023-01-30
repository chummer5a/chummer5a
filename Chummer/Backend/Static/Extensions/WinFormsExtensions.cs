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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Annotations;
using NLog;

namespace Chummer
{
    public static class WinFormsExtensions
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        #region Forms Extensions

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="owner"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static DialogResult ShowDialogSafe(this Form frmForm, IWin32Window owner = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (frmForm == null)
                throw new ArgumentNullException(nameof(frmForm));
            if (frmForm.IsDisposed)
                throw new ObjectDisposedException(nameof(frmForm));
            if (!Utils.IsUnitTest)
                return Utils.RunOnMainThread(() => frmForm.ShowDialog(owner), token);

            // Unit tests cannot use ShowDialog because that will stall them out
            bool blnDoClose = false;
            void FormOnShown(object sender, EventArgs args) => blnDoClose = true;
            frmForm.DoThreadSafe(x =>
            {
                x.Shown += FormOnShown;
                x.ShowInTaskbar = false;
                x.Show(owner);
            });
            while (!blnDoClose)
            {
                token.ThrowIfCancellationRequested();
                Utils.SafeSleep(token, true);
            }

            return frmForm.DoThreadSafeFunc(x =>
            {
                x.Close();
                return x.DialogResult;
            });
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="objCharacter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static DialogResult ShowDialogSafe(this Form frmForm, Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return frmForm.ShowDialogSafe(Program.GetFormForDialog(objCharacter), token);
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="owner"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogSafeAsync(this Form frmForm, IWin32Window owner = null, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<DialogResult>(token);
            // Unit tests cannot use ShowDialog because that will stall them out
            if (frmForm == null)
                return Task.FromException<DialogResult>(new ArgumentNullException(nameof(frmForm)));
            if (frmForm.IsDisposed)
                return Task.FromException<DialogResult>(new ObjectDisposedException(nameof(frmForm)));
            if (!Utils.IsUnitTest)
                return Utils.RunOnMainThreadAsync(() => frmForm.ShowDialog(owner), token);

            TaskCompletionSource<DialogResult> objCompletionSource = new TaskCompletionSource<DialogResult>();
            using (token.Register(() => objCompletionSource.TrySetCanceled(token)))
            {
                void BeginShow(Form frmInner)
                {
                    frmInner.Shown += FormOnShown;
                    frmInner.Show(owner);

                    void FormOnShown(object sender, EventArgs args)
                    {
                        frmForm.DoThreadSafe(x => x.Close());
                        objCompletionSource.SetResult(frmForm.DoThreadSafeFunc(x => x.DialogResult));
                    }
                }

                Action<Form> funcBegin = BeginShow;
                frmForm.BeginInvoke(funcBegin, frmForm);
                return objCompletionSource.Task;
            }
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="objCharacter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<DialogResult> ShowDialogSafeAsync(this Form frmForm, Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Form frmFormForDialog = await Program.GetFormForDialogAsync(objCharacter, token).ConfigureAwait(false);
            return await frmForm.ShowDialogSafeAsync(frmFormForDialog, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Non-blocking version of ShowDialog() based on the following blog post:
        /// https://sriramsakthivel.wordpress.com/2015/04/19/asynchronous-showdialog/
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="owner"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogNonBlockingAsync(this Form frmForm, IWin32Window owner = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (frmForm == null)
                return Task.FromException<DialogResult>(new ArgumentNullException(nameof(frmForm)));
            if (frmForm.IsDisposed)
                return Task.FromException<DialogResult>(new ObjectDisposedException(nameof(frmForm)));
            if (!frmForm.IsHandleCreated)
            {
                IntPtr _ = frmForm.Handle; // accessing Handle forces its creation
            }

            TaskCompletionSource<DialogResult> objCompletionSource = new TaskCompletionSource<DialogResult>();
            CancellationTokenRegistration objCancelRegistration
                = token.Register(() => objCompletionSource.TrySetCanceled(token));
            frmForm.BeginInvoke(new Action(() =>
            {
                objCompletionSource.SetResult(frmForm.ShowDialog(owner));
                objCancelRegistration.Dispose();
            }));
            return objCompletionSource.Task;
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="owner"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogNonBlockingSafeAsync(this Form frmForm, IWin32Window owner = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Unit tests cannot use ShowDialog because that will stall them out
            if (!Utils.IsUnitTest)
                return frmForm.ShowDialogNonBlockingAsync(owner, token);
            if (frmForm == null)
                return Task.FromException<DialogResult>(new ArgumentNullException(nameof(frmForm)));
            if (frmForm.IsDisposed)
                return Task.FromException<DialogResult>(new ObjectDisposedException(nameof(frmForm)));
            if (!frmForm.IsHandleCreated)
            {
                IntPtr _ = frmForm.Handle; // accessing Handle forces its creation
            }

            TaskCompletionSource<DialogResult> objCompletionSource = new TaskCompletionSource<DialogResult>();
            CancellationTokenRegistration objCancelRegistration
                = token.Register(() => objCompletionSource.TrySetCanceled(token));
            void BeginShow(Form frmInner)
            {
                frmInner.Shown += FormOnShown;
                frmInner.Show(owner);
                void FormOnShown(object sender, EventArgs args)
                {
                    frmForm.DoThreadSafe(x => x.Close());
                    objCompletionSource.SetResult(frmForm.DoThreadSafeFunc(x => x.DialogResult));
                    objCancelRegistration.Dispose();
                }
            }

            Action<Form> funcBegin = BeginShow;
            frmForm.BeginInvoke(funcBegin, frmForm);
            return objCompletionSource.Task;
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="objCharacter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogNonBlockingSafeAsync(this Form frmForm, Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return frmForm.ShowDialogNonBlockingSafeAsync(Program.GetFormForDialog(objCharacter), token);
        }

        #endregion Forms Extensions

        #region Controls Extensions

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objControl, Action funcToRun) where T : Control
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                    funcToRun.Invoke();
                else
                    Utils.RunOnMainThread(funcToRun);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objControl, Action<T> funcToRun) where T : Control
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                    funcToRun.Invoke(null);
                else
                    Utils.RunOnMainThread(() => funcToRun(objControl));
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objControl, Action<CancellationToken> funcToRun, CancellationToken token = default) where T : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                    funcToRun.Invoke(token);
                else
                    Utils.RunOnMainThread(() => funcToRun(token), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static void DoThreadSafe<T>(this T objControl, Action<T, CancellationToken> funcToRun, CancellationToken token = default) where T : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                    funcToRun.Invoke(null, token);
                else
                    Utils.RunOnMainThread(() => funcToRun(objControl, token), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objControl, Action funcToRun, CancellationToken token = default) where T : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objControl == null ? Task.Run(funcToRun, token) : Utils.RunOnMainThreadAsync(funcToRun, token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objControl, Action<T> funcToRun, CancellationToken token = default) where T : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objControl == null
                    ? Task.Run(() => funcToRun(null), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objControl), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objControl, Action<CancellationToken> funcToRun, CancellationToken token = default) where T : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objControl == null
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task DoThreadSafeAsync<T>(this T objControl, Action<T, CancellationToken> funcToRun, CancellationToken token = default) where T : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (funcToRun == null)
                return Task.CompletedTask;
            try
            {
                return objControl == null
                    ? Task.Run(() => funcToRun(null, token), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objControl, token), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException(e);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objControl, Func<T2> funcToRun) where T1 : Control
        {
            if (funcToRun == null)
                return default;
            try
            {
                return objControl == null ? funcToRun.Invoke() : Utils.RunOnMainThread(funcToRun);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return default;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objControl, Func<T1, T2> funcToRun) where T1 : Control
        {
            if (funcToRun == null)
                return default;
            try
            {
                return objControl == null ? funcToRun.Invoke(null) : Utils.RunOnMainThread(() => funcToRun(objControl));
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return default;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objControl, Func<CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            try
            {
                return objControl == null
                    ? funcToRun.Invoke(token)
                    : Utils.RunOnMainThread(() => funcToRun(token), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return default;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static T2 DoThreadSafeFunc<T1, T2>(this T1 objControl, Func<T1, CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            try
            {
                return objControl == null
                    ? funcToRun.Invoke(null, token)
                    : Utils.RunOnMainThread(() => funcToRun(objControl, token), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return default;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objControl == null
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<T1, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objControl == null
                    ? Task.Run(() => funcToRun(null), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objControl), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objControl == null
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<T1, CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<T2>(token);
            if (funcToRun == null)
                return Task.FromResult<T2>(default);
            try
            {
                return objControl == null
                    ? Task.Run(() => funcToRun(null, token), token)
                    : Utils.RunOnMainThreadAsync(() => funcToRun(objControl, token), token);
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
                Program.ShowScrollableMessageBox(objControl, e.ToString());
#endif
                return Task.FromException<T2>(e);
            }

            return Task.FromResult<T2>(default);
        }

        /// <summary>
        /// Bind a control's property to a property such that only the control's property is ever updated (when the source has OnPropertyChanged)
        /// Faster than DoDataBinding both on startup and on processing, so should be used for properties where the control's property is never set manually.
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        public static void DoOneWayDataBinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (objControl == null)
                return;
            Utils.RunOnMainThread(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false,
                                            DataSourceUpdateMode.Never);
            });
        }

        /// <summary>
        /// Bind a control's property to a property such that only the control's property is ever updated (when the source has OnPropertyChanged)
        /// Faster than DoDataBinding both on startup and on processing, so should be used for properties where the control's property is never set manually.
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task DoOneWayDataBindingAsync(this Control objControl, string strPropertyName, object objDataSource, string strDataMember, CancellationToken token = default)
        {
            if (objControl == null)
                return Task.CompletedTask;
            return Utils.RunOnMainThreadAsync(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false,
                                            DataSourceUpdateMode.Never);
            }, token);
        }

        /// <summary>
        /// Bind a control's property to a data property with an async getter in one direction. Similar to a one-way databinding, but the processing is done
        /// with async tasks, thus bypassing potential synchronous locking issues.
        /// </summary>
        /// <typeparam name="T1">Control type of <paramref name="objControl"/>.</typeparam>
        /// <typeparam name="T2">Source for the data property.</typeparam>
        /// <typeparam name="T3">Type of the data property that will be bound to the control</typeparam>
        /// <param name="objControl">Control to bind.</param>
        /// <param name="funcControlSetter">Setter function to use to set the appropriate property of <paramref name="objControl"/>.</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/>.</param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/> through the <paramref name="funcControlSetter"/> setter.</param>
        /// <param name="funcAsyncDataGetter">Asynchronous getter function of <paramref name="strDataMember"/>.</param>
        /// <param name="objGetterToken">Cancellation to use in any asynchronous getting or updating of </param>
        /// <param name="token">Cancellation token to listen to for this assignment.</param>
        public static void RegisterOneWayAsyncDataBinding<T1, T2, T3>(
            this T1 objControl, Action<T1, T3> funcControlSetter, T2 objDataSource, string strDataMember,
            Func<T2, Task<T3>> funcAsyncDataGetter, CancellationToken objGetterToken = default,
            CancellationToken token = default)
            where T1 : Control where T2 : INotifyPropertyChanged
        {
            if (objControl == null)
                return;
            Utils.RunOnMainThread(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }
            }, token);
            T3 objData = Utils.SafelyRunSynchronously(() => funcAsyncDataGetter.Invoke(objDataSource), token);
            objControl.DoThreadSafe((x, y) => funcControlSetter.Invoke(x, objData), objGetterToken);
            if (objDataSource is IHasLockObject objHasLock)
            {
                try
                {
                    using (objHasLock.LockObject.EnterWriteLock(token))
                        objDataSource.PropertyChanged += OnPropertyChangedAsync;
                }
                catch (ObjectDisposedException)
                {
                    // swallow this
                }
            }
            else
                objDataSource.PropertyChanged += OnPropertyChangedAsync;
            Utils.RunOnMainThread(() => objControl.Disposed += (o, args) =>
            {
                if (objDataSource is IHasLockObject objHasLock2)
                {
                    try
                    {
                        using (objHasLock2.LockObject.EnterWriteLock(CancellationToken.None))
                            objDataSource.PropertyChanged -= OnPropertyChangedAsync;
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this
                    }
                }
                else
                    objDataSource.PropertyChanged -= OnPropertyChangedAsync;
            }, token);
            async void OnPropertyChangedAsync(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == strDataMember && !objGetterToken.IsCancellationRequested)
                {
                    T3 objInnerData = await funcAsyncDataGetter.Invoke(objDataSource).ConfigureAwait(false);
                    await objControl.DoThreadSafeAsync(y => funcControlSetter.Invoke(y, objInnerData), token: objGetterToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Bind a control's property to a data property with an async getter in one direction. Similar to a one-way databinding, but the processing is done
        /// with async tasks, thus bypassing potential synchronous locking issues.
        /// </summary>
        /// <typeparam name="T1">Control type of <paramref name="objControl"/>.</typeparam>
        /// <typeparam name="T2">Source for the data property.</typeparam>
        /// <typeparam name="T3">Type of the data property that will be bound to the control</typeparam>
        /// <param name="objControl">Control to bind.</param>
        /// <param name="funcControlSetter">Setter function to use to set the appropriate property of <paramref name="objControl"/>.</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/>.</param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/> through the <paramref name="funcControlSetter"/> setter.</param>
        /// <param name="funcAsyncDataGetter">Asynchronous getter function of <paramref name="strDataMember"/>.</param>
        /// <param name="objGetterToken">Cancellation to use in any asynchronous getting or updating of </param>
        /// <param name="token">Cancellation token to listen to for this assignment.</param>
        public static async ValueTask RegisterOneWayAsyncDataBindingAsync<T1, T2, T3>(
            this T1 objControl, Action<T1, T3> funcControlSetter, T2 objDataSource, string strDataMember,
            Func<T2, Task<T3>> funcAsyncDataGetter, CancellationToken objGetterToken = default,
            CancellationToken token = default)
            where T1 : Control where T2 : INotifyPropertyChanged
        {
            if (objControl == null)
                return;
            await Utils.RunOnMainThreadAsync(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }
            }, token).ConfigureAwait(false);
            T3 objData = await funcAsyncDataGetter.Invoke(objDataSource).ConfigureAwait(false);
            await objControl.DoThreadSafeAsync(x => funcControlSetter.Invoke(x, objData), objGetterToken).ConfigureAwait(false);
            if (objDataSource is IHasLockObject objHasLock)
            {
                try
                {
                    IAsyncDisposable objLocker = await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        objDataSource.PropertyChanged += OnPropertyChangedAsync;
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // swallow this
                }
            }
            else
                objDataSource.PropertyChanged += OnPropertyChangedAsync;
            await Utils.RunOnMainThreadAsync(() => objControl.Disposed += (o, args) =>
            {
                if (objDataSource is IHasLockObject objHasLock2)
                {
                    try
                    {
                        using (objHasLock2.LockObject.EnterWriteLock(CancellationToken.None))
                            objDataSource.PropertyChanged -= OnPropertyChangedAsync;
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this
                    }
                }
                else
                    objDataSource.PropertyChanged -= OnPropertyChangedAsync;
            }, token).ConfigureAwait(false);
            async void OnPropertyChangedAsync(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == strDataMember && !objGetterToken.IsCancellationRequested)
                {
                    T3 objInnerData = await funcAsyncDataGetter.Invoke(objDataSource).ConfigureAwait(false);
                    await objControl.DoThreadSafeAsync(y => funcControlSetter.Invoke(y, objInnerData), token: objGetterToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Bind a control's property to a property via OnPropertyChanged
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void DoDataBinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objControl == null)
                return;
            Utils.RunOnMainThread(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false,
                                            DataSourceUpdateMode.OnPropertyChanged);
            }, token);
        }

        /// <summary>
        /// Bind a control's property to a property via OnPropertyChanged
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task DoDataBindingAsync(this Control objControl, string strPropertyName, object objDataSource, string strDataMember, CancellationToken token = default)
        {
            if (objControl == null)
                return Task.CompletedTask;
            return Utils.RunOnMainThreadAsync(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false,
                                            DataSourceUpdateMode.OnPropertyChanged);
            }, token);
        }

        /// <summary>
        /// Bind a control's property to the OPPOSITE of property such that only the control's property is ever updated (when the source has OnPropertyChanged). Expected to be used exclusively by boolean bindings, other attributes have not been tested.
        /// Faster than DoDataBinding both on startup and on processing, so should be used for properties where the control's property is never set manually.
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        public static void DoOneWayNegatableDataBinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (objControl == null)
                return;
            Utils.RunOnMainThread(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, false,
                                                                 DataSourceUpdateMode.Never));
            });
        }

        /// <summary>
        /// Bind a control's property to the OPPOSITE of property such that only the control's property is ever updated (when the source has OnPropertyChanged). Expected to be used exclusively by boolean bindings, other attributes have not been tested.
        /// Faster than DoDataBinding both on startup and on processing, so should be used for properties where the control's property is never set manually.
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task DoOneWayNegatableDataBindingAsync(this Control objControl, string strPropertyName, object objDataSource, string strDataMember, CancellationToken token = default)
        {
            if (objControl == null)
                return Task.CompletedTask;
            return Utils.RunOnMainThreadAsync(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, false,
                                                                 DataSourceUpdateMode.Never));
            }, token);
        }

        /// <summary>
        /// Bind a control's property to the OPPOSITE of property via OnPropertyChanged. Expected to be used exclusively by boolean bindings, other attributes have not been tested.
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        public static void DoNegatableDataBinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (objControl == null)
                return;
            Utils.RunOnMainThread(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, false,
                                                                 DataSourceUpdateMode.OnPropertyChanged));
            });
        }

        /// <summary>
        /// Bind a control's property to the OPPOSITE of property via OnPropertyChanged. Expected to be used exclusively by boolean bindings, other attributes have not been tested.
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task DoNegatableDataBindingAsync(this Control objControl, string strPropertyName, object objDataSource, string strDataMember, CancellationToken token = default)
        {
            if (objControl == null)
                return Task.CompletedTask;
            return Utils.RunOnMainThreadAsync(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, false,
                                                                 DataSourceUpdateMode.OnPropertyChanged));
            }, token);
        }

        /// <summary>
        /// Syntactic sugar for what is effectively a null check for disposable WinForms controls.
        /// </summary>
        /// <param name="objControl"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed([CanBeNull] this Control objControl)
        {
            try
            {
                return objControl == null || objControl.Disposing || objControl.IsDisposed;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }

        #endregion Controls Extensions

        #region ComboBox Extensions

        /// <summary>
        /// We use this dictionary to keep track of which List{ListItem} types assigned to WinForms controls need to be returned to pools
        /// when the controls dispose. It's really hacky, but because controls' DataSource property gets muddied during the control's disposal,
        /// some external dictionary like this is kind of the only safe way to handle this issue without relying solely on custom control types.
        /// It's also a ConcurrentDictionary and not LockingDictionary because it will overwhelmingly be used for adding and/or updating values,
        /// not any for just getting/reading them.
        /// </summary>
        private static readonly ConcurrentDictionary<Control, List<ListItem>> s_dicListItemListAssignments
            = new ConcurrentDictionary<Control, List<ListItem>>();

        public static void PopulateWithListItems(this ListBox lsbThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            lsbThis?.DoThreadSafe(x => PopulateWithListItemsCore(x, lstItems, token));
        }

        public static void PopulateWithListItems(this ComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            cboThis?.DoThreadSafe(x => PopulateWithListItemsCore(x, lstItems, token));
        }

        public static void PopulateWithListItems(this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            cboThis?.DoThreadSafe(x => PopulateWithListItemsCore(x, lstItems, token));
        }

        public static Task PopulateWithListItemsAsync([NotNull] this ListBox lsbThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            return lsbThis.DoThreadSafeAsync(x => PopulateWithListItemsCore(x, lstItems, token), token);
        }

        public static Task PopulateWithListItemsAsync([NotNull] this ComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            return cboThis.DoThreadSafeAsync(x => PopulateWithListItemsCore(x, lstItems, token), token);
        }

        public static Task PopulateWithListItemsAsync([NotNull] this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            return cboThis.DoThreadSafeAsync(x => PopulateWithListItemsCore(x, lstItems, token), token);
        }

        private static void PopulateWithListItemsCore(this ListBox lsbThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(lsbThis.DataSource, lstItems))
                return;
            token.ThrowIfCancellationRequested();
            // Binding multiple ComboBoxes to the same DataSource will also cause selected values to sync up between them.
            // Resetting bindings to prevent this though will also reset bindings to other properties, so that's not really an option
            // This means the code we use has to set the DataSources to new lists instead of the same one.
            List<ListItem> lstItemsToSet = Utils.ListItemListPool.Get();
            bool blnDoReturnList = true;
            try
            {
                if (lstItems != null)
                {
                    lstItemsToSet.AddRange(lstItems);
                }

                token.ThrowIfCancellationRequested();
                lsbThis.BeginUpdate();
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!(lsbThis.DataSource is IEnumerable<ListItem> lstCurrentList))
                    {
                        lsbThis.ValueMember = nameof(ListItem.Value);
                        lsbThis.DisplayMember = nameof(ListItem.Name);
                    }
                    // Setting DataSource is slow because WinForms is old, so let's make sure we definitely need to do it
                    else if (lstCurrentList.SequenceEqual(lstItemsToSet))
                        return;

                    token.ThrowIfCancellationRequested();
                    if (lsbThis.DataSource != null)
                    {
                        // Assign new binding context because to avoid weirdness when switching DataSource
                        lsbThis.BindingContext = new BindingContext();
                    }
                    else
                    {
                        lsbThis.Disposed += (sender, args) =>
                        {
                            if (s_dicListItemListAssignments.TryRemove(lsbThis, out List<ListItem> lstInnerToReturn))
                            {
                                Utils.ListItemListPool.Return(ref lstInnerToReturn);
                            }
                        };
                    }

                    blnDoReturnList = false;
                    s_dicListItemListAssignments.AddOrUpdate(lsbThis, lstItemsToSet, (x, y) =>
                    {
                        Utils.ListItemListPool.Return(ref y);
                        return lstItemsToSet;
                    });
                    lsbThis.DataSource = lstItemsToSet;
                }
                finally
                {
                    lsbThis.EndUpdate();
                }
            }
            finally
            {
                if (blnDoReturnList)
                    Utils.ListItemListPool.Return(ref lstItemsToSet);
            }
        }

        private static void PopulateWithListItemsCore(this ComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(cboThis.DataSource, lstItems))
                return;
            token.ThrowIfCancellationRequested();
            // Binding multiple ComboBoxes to the same DataSource will also cause selected values to sync up between them.
            // Resetting bindings to prevent this though will also reset bindings to other properties, so that's not really an option
            // This means the code we use has to set the DataSources to new lists instead of the same one.
            List<ListItem> lstItemsToSet = Utils.ListItemListPool.Get();
            bool blnDoReturnList = true;
            try
            {
                if (lstItems != null)
                {
                    lstItemsToSet.AddRange(lstItems);
                }

                token.ThrowIfCancellationRequested();
                cboThis.BeginUpdate();
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!(cboThis.DataSource is IEnumerable<ListItem> lstCurrentList))
                    {
                        cboThis.ValueMember = nameof(ListItem.Value);
                        cboThis.DisplayMember = nameof(ListItem.Name);
                    }
                    // Setting DataSource is slow because WinForms is old, so let's make sure we definitely need to do it
                    else if (lstCurrentList.SequenceEqual(lstItemsToSet))
                        return;

                    token.ThrowIfCancellationRequested();
                    if (cboThis.DataSource != null)
                    {
                        // Assign new binding context because to avoid weirdness when switching DataSource
                        cboThis.BindingContext = new BindingContext();
                    }
                    else
                    {
                        cboThis.Disposed += (sender, args) =>
                        {
                            if (s_dicListItemListAssignments.TryRemove(cboThis, out List<ListItem> lstInnerToReturn))
                            {
                                Utils.ListItemListPool.Return(ref lstInnerToReturn);
                            }
                        };
                    }

                    blnDoReturnList = false;
                    s_dicListItemListAssignments.AddOrUpdate(cboThis, lstItemsToSet, (x, y) =>
                    {
                        Utils.ListItemListPool.Return(ref y);
                        return lstItemsToSet;
                    });
                    cboThis.DataSource = lstItemsToSet;
                }
                finally
                {
                    cboThis.EndUpdate();
                }
            }
            finally
            {
                if (blnDoReturnList)
                    Utils.ListItemListPool.Return(ref lstItemsToSet);
            }
        }

        private static void PopulateWithListItemsCore(this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(cboThis.DataSource, lstItems))
                return;
            token.ThrowIfCancellationRequested();
            // Binding multiple ComboBoxes to the same DataSource will also cause selected values to sync up between them.
            // Resetting bindings to prevent this though will also reset bindings to other properties, so that's not really an option
            // This means the code we use has to set the DataSources to new lists instead of the same one.
            List<ListItem> lstItemsToSet = Utils.ListItemListPool.Get();
            bool blnDoReturnList = true;
            try
            {
                if (lstItems != null)
                {
                    lstItemsToSet.AddRange(lstItems);
                }

                token.ThrowIfCancellationRequested();
                cboThis.BeginUpdate();
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!(cboThis.DataSource is IEnumerable<ListItem> lstCurrentList))
                    {
                        cboThis.ValueMember = nameof(ListItem.Value);
                        cboThis.DisplayMember = nameof(ListItem.Name);
                    }
                    // Setting DataSource is slow because WinForms is old, so let's make sure we definitely need to do it
                    else if (lstCurrentList.SequenceEqual(lstItemsToSet))
                        return;

                    token.ThrowIfCancellationRequested();
                    if (cboThis.DataSource != null)
                    {
                        // Assign new binding context because to avoid weirdness when switching DataSource
                        cboThis.BindingContext = new BindingContext();
                    }
                    else
                    {
                        cboThis.Disposed += (sender, args) =>
                        {
                            if (s_dicListItemListAssignments.TryRemove(cboThis, out List<ListItem> lstInnerToReturn))
                            {
                                Utils.ListItemListPool.Return(ref lstInnerToReturn);
                            }
                        };
                    }

                    blnDoReturnList = false;
                    s_dicListItemListAssignments.AddOrUpdate(cboThis, lstItemsToSet, (x, y) =>
                    {
                        Utils.ListItemListPool.Return(ref y);
                        return lstItemsToSet;
                    });
                    cboThis.DataSource = lstItemsToSet;
                }
                finally
                {
                    cboThis.EndUpdate();
                }
            }
            finally
            {
                if (blnDoReturnList)
                    Utils.ListItemListPool.Return(ref lstItemsToSet);
            }
        }

        #endregion ComboBox Extensions

        #region TreeNode Extensions

        public static TreeNode GetTopParent(this TreeNode objThis)
        {
            if (objThis == null)
                return null;
            TreeNode objReturn = objThis;
            while (objReturn.Parent != null)
                objReturn = objReturn.Parent;
            return objReturn;
        }

        /// <summary>
        /// Find a TreeNode in a TreeNode based on its Tag.
        /// </summary>
        /// <param name="strGuid">InternalId of the Node to find.</param>
        /// <param name="objNode">TreeNode to search.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNode(this TreeNode objNode, string strGuid, bool blnDeep = true)
        {
            if (objNode == null || string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid()) return null;
            foreach (TreeNode objChild in objNode.Nodes)
            {
                if (objChild.Tag is IHasInternalId idNode && idNode.InternalId == strGuid || objChild.Tag is string s && s == strGuid)
                    return objChild;

                if (!blnDeep) continue;
                TreeNode objFound = objChild.FindNode(strGuid);
                if (objFound != null)
                    return objFound;
            }
            return null;
        }

        /// <summary>
        /// Find a TreeNode in a TreeNode based on its Tag.
        /// </summary>
        /// <param name="objNode">TreeNode to search.</param>
        /// <param name="objTag">Tag to look for.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNodeByTag(this TreeNode objNode, object objTag, bool blnDeep = true)
        {
            if (objNode != null && objTag != null)
            {
                foreach (TreeNode objChild in objNode.Nodes)
                {
                    if (objChild.Tag == objTag)
                        return objChild;

                    if (blnDeep)
                    {
                        TreeNode objFound = objChild.FindNodeByTag(objTag);
                        if (objFound != null)
                            return objFound;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the rightmost edge of the node or any of its descendents.
        /// </summary>
        /// <returns></returns>
        public static int GetRightMostEdge(this TreeNode objNode)
        {
            if (objNode == null)
                return 0;
            if (objNode.Nodes.Count == 0)
                return objNode.Bounds.Right;
            int intReturn = 0;
            foreach (TreeNode objChild in objNode.Nodes)
            {
                int intLoopEdge = objChild.GetRightMostEdge();
                if (intLoopEdge > intReturn)
                    intReturn = intLoopEdge;
            }
            return intReturn;
        }

        #endregion TreeNode Extensions

        #region TreeView Extensions

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treView">TreeView to sort.</param>
        /// <param name="strSelectedNodeTag">String of the tag to select after sorting.</param>
        public static void SortCustomAlphabetically(this TreeView treView, string strSelectedNodeTag = "")
        {
            TreeNodeCollection lstTreeViewNodes = treView?.Nodes;
            if (lstTreeViewNodes == null)
                return;
            if (string.IsNullOrEmpty(strSelectedNodeTag))
                strSelectedNodeTag = (treView.SelectedNode?.Tag as IHasInternalId)?.InternalId;
            for (int i = 0; i < lstTreeViewNodes.Count; ++i)
            {
                TreeNode objLoopNode = lstTreeViewNodes[i];
                TreeNodeCollection objLoopNodeChildren = objLoopNode.Nodes;
                int intChildrenCount = objLoopNodeChildren.Count;
                if (intChildrenCount > 0)
                {
                    TreeNode[] aobjNodes = new TreeNode[intChildrenCount];
                    objLoopNodeChildren.CopyTo(aobjNodes, 0);
                    objLoopNodeChildren.Clear();
                    Array.Sort(aobjNodes, CompareTreeNodes.CompareText);
                    objLoopNodeChildren.AddRange(aobjNodes);

                    objLoopNode.Expand();
                }
            }

            TreeNode objSelectedNode = treView.FindNode(strSelectedNodeTag);
            if (objSelectedNode != null)
                treView.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treView">TreeView to sort.</param>
        /// <param name="objSelectedNodeTag">String of the tag to select after sorting.</param>
        public static void SortCustomAlphabetically(this TreeView treView, object objSelectedNodeTag = null)
        {
            TreeNodeCollection lstTreeViewNodes = treView?.Nodes;
            if (lstTreeViewNodes == null)
                return;
            if (objSelectedNodeTag == null)
                objSelectedNodeTag = treView.SelectedNode?.Tag;
            for (int i = 0; i < lstTreeViewNodes.Count; ++i)
            {
                TreeNode objLoopNode = lstTreeViewNodes[i];
                TreeNodeCollection objLoopNodeChildren = objLoopNode.Nodes;
                int intChildrenCount = objLoopNodeChildren.Count;
                if (intChildrenCount > 0)
                {
                    TreeNode[] aobjNodes = new TreeNode[intChildrenCount];
                    objLoopNodeChildren.CopyTo(aobjNodes, 0);
                    objLoopNodeChildren.Clear();
                    Array.Sort(aobjNodes, CompareTreeNodes.CompareText);
                    objLoopNodeChildren.AddRange(aobjNodes);

                    objLoopNode.Expand();
                }
            }

            TreeNode objSelectedNode = treView.FindNodeByTag(objSelectedNodeTag);
            if (objSelectedNode != null)
                treView.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Sort the contents of a TreeView based on the sorting property of any
        /// ICanSorts in the tree
        /// </summary>
        /// <param name="treView">The tree to sort</param>
        /// <param name="blnRetainTopLevelOrder">Whether or not to retain the order of the top level nodes.</param>
        public static void SortCustomOrder(this TreeView treView, bool blnRetainTopLevelOrder = false)
        {
            if (treView == null)
                return;

            string strSelectedNodeTag = (treView.SelectedNode?.Tag as IHasInternalId)?.InternalId;

            List<TreeNode> lstOriginalTopLevelOrder = null;
            if (blnRetainTopLevelOrder)
            {
                lstOriginalTopLevelOrder = new List<TreeNode>(treView.Nodes.Count);
                foreach (TreeNode objNode in treView.Nodes)
                    lstOriginalTopLevelOrder.Add(objNode);
            }

            bool blnOldSorted = treView.Sorted;
            try
            {
                IComparer currentSorter = treView.TreeViewNodeSorter;
                try
                {
                    treView.TreeViewNodeSorter = new CustomNodeSorter();
                    treView.Sort();
                }
                finally
                {
                    treView.TreeViewNodeSorter = currentSorter;
                }
            }
            finally
            {
                treView.Sorted = blnOldSorted;
            }

            if (lstOriginalTopLevelOrder != null)
            {
                treView.Nodes.Clear();
                foreach (TreeNode objNode in lstOriginalTopLevelOrder)
                    treView.Nodes.Add(objNode);
            }

            // Reselect whatever was selected before
            TreeNode objSelectedNode = treView.FindNode(strSelectedNodeTag);
            if (objSelectedNode != null)
                treView.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Custom comparer used by SortCustomOrder
        /// </summary>
        private sealed class CustomNodeSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                // Sort any non-sortables first
                object objLeft = (x as TreeNode)?.Tag;
                if (objLeft == null)
                    return (y as TreeNode)?.Tag == null ? 0 : -1;
                object objRight = (y as TreeNode)?.Tag;
                if (objRight == null)
                    return 1;
                // Sort by SortOrder, but always put Locations after all non-Locations
                if (objLeft is ICanSort objLeftCanSort)
                {
                    if (!(objRight is ICanSort objRightCanSort))
                        return 1;
                    if (objLeft is Location)
                    {
                        return objRight is Location
                            ? objLeftCanSort.SortOrder.CompareTo(objRightCanSort.SortOrder)
                            : 1;
                    }
                    return objRight is Location
                        ? -1
                        : objLeftCanSort.SortOrder.CompareTo(objRightCanSort.SortOrder);
                }
                return objRight is ICanSort ? -1 : 0;
            }
        }

        /// <summary>
        /// Iterates through a TreeView and stores the sorting order on any
        /// ICanSort objects, allowing them to retain the order after a load
        /// </summary>
        /// <param name="treView"></param>
        public static void CacheSortOrder(this TreeView treView)
        {
            CacheSortOrderRecursive(treView?.Nodes);
        }

        /// <summary>
        /// Does a breadth-first recursion to set the sorting property of any ICanSorts in the tree
        /// </summary>
        /// <param name="lstNodes">The list of TreeNodes to iterate over</param>
        private static void CacheSortOrderRecursive(TreeNodeCollection lstNodes)
        {
            // Do this as two steps because non-sortables can own sortables
            foreach (TreeNode objNode in lstNodes)
            {
                if (objNode.Tag is ICanSort objSortable)
                    objSortable.SortOrder = objNode.Index;
                CacheSortOrderRecursive(objNode.Nodes);
            }
        }

        /// <summary>
        /// Clear the background color for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="treView">Base TreeView whose nodes should get their background color cleared.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public static void ClearNodeBackground(this TreeView treView, TreeNode objHighlighted)
        {
            treView?.Nodes.ClearNodeBackground(objHighlighted);
        }

        /// <summary>
        /// Find a TreeNode in a TreeView based on its Tag.
        /// </summary>
        /// <param name="strGuid">InternalId of the Node to find.</param>
        /// <param name="treTree">TreeView to search.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNode(this TreeView treTree, string strGuid, bool blnDeep = true)
        {
            if (treTree == null || string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid()) return null;
            foreach (TreeNode objNode in treTree.Nodes)
            {
                if (objNode?.Tag is IHasInternalId node && node.InternalId == strGuid || objNode?.Tag?.ToString() == strGuid)
                    return objNode;

                if (!blnDeep) continue;
                TreeNode objFound = objNode.FindNode(strGuid);
                if (objFound != null)
                    return objFound;
            }
            return null;
        }

        /// <summary>
        /// Find a TreeNode in a TreeView based on its Tag.
        /// </summary>
        /// <param name="treTree">TreeView to search.</param>
        /// <param name="objTag">Tag to look for.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNodeByTag(this TreeView treTree, object objTag, bool blnDeep = true)
        {
            if (treTree != null && objTag != null)
            {
                foreach (TreeNode objNode in treTree.Nodes)
                {
                    if (objNode.Tag == objTag)
                        return objNode;

                    if (blnDeep)
                    {
                        TreeNode objFound = objNode.FindNodeByTag(objTag);
                        if (objFound != null)
                            return objFound;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the rightmost edge of the tree or any of its descendents.
        /// </summary>
        /// <param name="treTree"></param>
        /// <returns></returns>
        public static int GetRightMostEdge(this TreeView treTree)
        {
            if (treTree == null)
                return 0;
            if (treTree.Nodes.Count == 0)
                return treTree.Bounds.Right;
            int intReturn = 0;
            foreach (TreeNode objChild in treTree.Nodes)
            {
                int intLoopEdge = objChild.GetRightMostEdge();
                if (intLoopEdge > intReturn)
                    intReturn = intLoopEdge;
            }
            return intReturn;
        }

        #endregion TreeView Extensions

        #region TreeNodeCollection Extensions

        /// <summary>
        /// Recursive method to clear the background color for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="objNodes">Parent TreeNodeCollection to check.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public static void ClearNodeBackground(this TreeNodeCollection objNodes, TreeNode objHighlighted)
        {
            if (objNodes == null)
                return;
            foreach (TreeNode objChild in objNodes)
            {
                if (objChild != objHighlighted)
                    objChild.BackColor = ColorManager.Window;
                objChild.Nodes.ClearNodeBackground(objHighlighted);
            }
        }

        #endregion TreeNodeCollection Extensions

        #region TextBox Extensions

        /// <summary>
        /// Get the number of preferred shown lines for a TextBox control and the maximum height of these lines. Multiply the two to get the preferred height of the control.
        /// </summary>
        /// <param name="txtTextBox">Control to analyze</param>
        public static Tuple<int, int> MeasureLineHeights(this TextBox txtTextBox)
        {
            string[] astrLines = txtTextBox.Lines;
            int intNumDisplayedLines = 0;
            int intMaxLineHeight = 0;
            if (astrLines != null)
            {
                foreach (string strLine in astrLines)
                {
                    Size objTextSize = TextRenderer.MeasureText(strLine, txtTextBox.Font);
                    intNumDisplayedLines += ((decimal)objTextSize.Width / txtTextBox.Width).StandardRound();
                    intMaxLineHeight = Math.Max(intMaxLineHeight, objTextSize.Height);
                }
            }
            return new Tuple<int, int>(intNumDisplayedLines, intMaxLineHeight);
        }

        /// <summary>
        /// Automatically (un)set vertical scrollbars for a TextBox based on whether or not it needs them.
        /// </summary>
        /// <param name="txtTextBox">Control to analyze and potentially (un)set scrollbars</param>
        public static void AutoSetScrollbars(this TextBox txtTextBox)
        {
            // Set textbox vertical scrollbar based on whether it's needed or not
            (int intNumDisplayedLines, int intMaxLineHeight) = MeasureLineHeights(txtTextBox);

            // Search the height of the biggest line and set scrollbars based on that
            ScrollBars eOldScrollBars = txtTextBox.ScrollBars;
            ScrollBars eNewScrollBars
                = intNumDisplayedLines * intMaxLineHeight >= Math.Max(txtTextBox.Height, txtTextBox.PreferredHeight)
                    ? ScrollBars.Vertical
                    : ScrollBars.None;
            if (eOldScrollBars == eNewScrollBars)
                return;
            txtTextBox.SuspendLayout();
            try
            {
                txtTextBox.ScrollBars = eNewScrollBars;
            }
            finally
            {
                txtTextBox.ResumeLayout();
            }
        }

        #endregion TextBox Extensions
    }
}
