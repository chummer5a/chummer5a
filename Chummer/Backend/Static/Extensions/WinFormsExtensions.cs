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
using System.Collections.Generic;
using System.ComponentModel;
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
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

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
            {
                DialogResult FuncToRun(Form x, IWin32Window y = null) => x.ShowDialog(y);

                if (!frmForm.InvokeRequired)
                {
                    token.ThrowIfCancellationRequested();
                    if (!(owner is Control objOwner) || !objOwner.InvokeRequired)
                        return frmForm.ShowDialog(owner);
                    return (DialogResult) objOwner.Invoke((Func<Form, IWin32Window, DialogResult>) FuncToRun, frmForm,
                                                          owner);
                }
                token.ThrowIfCancellationRequested();
                return (DialogResult) frmForm.Invoke((Func<Form, IWin32Window, DialogResult>) FuncToRun, frmForm,
                                                     owner);
            }
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
        public static async Task<DialogResult> ShowDialogSafeAsync(this Form frmForm, IWin32Window owner = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Unit tests cannot use ShowDialog because that will stall them out
            if (frmForm == null)
                throw new ArgumentNullException(nameof(frmForm));
            if (frmForm.IsDisposed)
                throw new ObjectDisposedException(nameof(frmForm));
            if (!Utils.IsUnitTest)
            {
                DialogResult FuncToRun(Form x, IWin32Window y = null) => x.ShowDialog(y);

                if (frmForm.InvokeRequired)
                {
                    using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        // ReSharper disable once AccessToDisposedClosure
                    using (token.Register(() => objInnerTokenSource.Cancel(false)))
                    {
                        void MyControlCopyOnDisposed(object sender, EventArgs e)
                        {
                            try
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                objInnerTokenSource.Cancel(false);
                            }
                            catch (ObjectDisposedException)
                            {
                                //swallow this
                            }
                        }

                        CancellationToken objInnerToken = objInnerTokenSource.Token;
                        frmForm.Disposed += MyControlCopyOnDisposed;
                        try
                        {
                            IAsyncResult objInvoke
                                = frmForm.BeginInvoke((Func<Form, IWin32Window, DialogResult>) FuncToRun, frmForm,
                                                      owner);
                            using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                await objHandle.WaitOneAsync(objInnerToken);
                            token.ThrowIfCancellationRequested();
                            objInnerToken.ThrowIfCancellationRequested();
                            if (frmForm.IsNullOrDisposed())
                                return default;
                            object objReturn = frmForm.EndInvoke(objInvoke);
                            if (objReturn is Exception ex)
                                throw ex;
                            return (DialogResult)objReturn;
                        }
                        finally
                        {
                            frmForm.Disposed -= MyControlCopyOnDisposed;
                        }
                    }
                }

                if (owner is Control objOwner && objOwner.InvokeRequired)
                {
                    using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        // ReSharper disable once AccessToDisposedClosure
                    using (token.Register(() => objInnerTokenSource.Cancel(false)))
                    {
                        void MyControlCopyOnDisposed(object sender, EventArgs e)
                        {
                            try
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                objInnerTokenSource.Cancel(false);
                            }
                            catch (ObjectDisposedException)
                            {
                                //swallow this
                            }
                        }

                        CancellationToken objInnerToken = objInnerTokenSource.Token;
                        objOwner.Disposed += MyControlCopyOnDisposed;
                        try
                        {
                            IAsyncResult objInvoke
                                = objOwner.BeginInvoke((Func<Form, IWin32Window, DialogResult>)FuncToRun, frmForm,
                                                       owner);
                            using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                await objHandle.WaitOneAsync(objInnerToken);
                            token.ThrowIfCancellationRequested();
                            objInnerToken.ThrowIfCancellationRequested();
                            if (objOwner.IsNullOrDisposed())
                                return default;
                            object objReturn = objOwner.EndInvoke(objInvoke);
                            if (objReturn is Exception ex)
                                throw ex;
                            return (DialogResult)objReturn;
                        }
                        finally
                        {
                            objOwner.Disposed -= MyControlCopyOnDisposed;
                        }
                    }
                }

                return frmForm.ShowDialog(owner);
            }

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
                return await objCompletionSource.Task;
            }
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="objCharacter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogSafeAsync(this Form frmForm, Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Program.GetFormForDialogAsync(objCharacter).ContinueWith(async x => await frmForm.ShowDialogSafeAsync(await x, token), token).Unwrap();
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

        #endregion

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
                {
                    funcToRun.Invoke();
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        if (myControlCopy.Invoke(funcToRun) is Exception ex)
                            throw ex;
                    }
                    else
                        funcToRun.Invoke();
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
                {
                    funcToRun.Invoke(null);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        if (myControlCopy.Invoke(funcToRun, myControlCopy) is Exception ex)
                            throw ex;
                    }
                    else
                        funcToRun.Invoke(myControlCopy);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
                {
                    funcToRun.Invoke(token);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        if (myControlCopy.Invoke(funcToRun, token) is Exception ex)
                            throw ex;
                    }
                    else
                        funcToRun.Invoke(token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
                {
                    funcToRun.Invoke(null, token);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        if (myControlCopy.Invoke(funcToRun, myControlCopy, token) is Exception ex)
                            throw ex;
                    }
                    else
                        funcToRun.Invoke(myControlCopy, token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
        public static async Task DoThreadSafeAsync<T>(this T objControl, Action funcToRun, CancellationToken token = default) where T : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                {
                    await Task.Run(funcToRun, token);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (myControlCopy.IsNullOrDisposed())
                                        return;
                                    if (myControlCopy.EndInvoke(objInvoke) is Exception ex)
                                        throw ex;
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        funcToRun.Invoke();
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
        public static async Task DoThreadSafeAsync<T>(this T objControl, Action<T> funcToRun, CancellationToken token = default) where T : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                {
                    await Task.Run(() => funcToRun(null), token);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (myControlCopy.IsNullOrDisposed())
                                        return;
                                    if (myControlCopy.EndInvoke(objInvoke) is Exception ex)
                                        throw ex;
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        funcToRun.Invoke(myControlCopy);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
        public static async Task DoThreadSafeAsync<T>(this T objControl, Action<CancellationToken> funcToRun, CancellationToken token = default) where T : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                {
                    await Task.Run(() => funcToRun(token), token);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                            // ReSharper disable once AccessToDisposedClosure
                        using (token.Register(() => objInnerTokenSource.Cancel(false)))
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun, objInnerToken);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (myControlCopy.IsNullOrDisposed())
                                        return;
                                    if (myControlCopy.EndInvoke(objInvoke) is Exception ex)
                                        throw ex;
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        funcToRun.Invoke(token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
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
        public static async Task DoThreadSafeAsync<T>(this T objControl, Action<T, CancellationToken> funcToRun, CancellationToken token = default) where T : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return;
            try
            {
                if (objControl == null)
                {
                    await Task.Run(() => funcToRun(null, token), token);
                }
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    // ReSharper disable once InlineTemporaryVariable
                    T myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                            // ReSharper disable once AccessToDisposedClosure
                        using (token.Register(() => objInnerTokenSource.Cancel(false)))
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun, myControlCopy, objInnerToken);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (myControlCopy.IsNullOrDisposed())
                                        return;
                                    if (myControlCopy.EndInvoke(objInvoke) is Exception ex)
                                        throw ex;
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        funcToRun.Invoke(myControlCopy, token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }
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
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = funcToRun.Invoke();
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        switch (myControlCopy.Invoke(funcToRun))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke();
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
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
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = funcToRun.Invoke(null);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        switch (myControlCopy.Invoke(funcToRun, myControlCopy))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke(myControlCopy);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
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
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = funcToRun.Invoke(token);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        switch (myControlCopy.Invoke(funcToRun, token))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke(token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
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
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = funcToRun.Invoke(null, token);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        switch (myControlCopy.Invoke(funcToRun, myControlCopy, token))
                        {
                            case Exception ex:
                                throw ex;
                            case T2 objReturnRawCast:
                                objReturn = objReturnRawCast;
                                break;
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke(myControlCopy, token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = await Task.Run(funcToRun, token);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (!myControlCopy.IsNullOrDisposed())
                                    {
                                        switch (myControlCopy.EndInvoke(objInvoke))
                                        {
                                            case Exception ex:
                                                throw ex;
                                            case T2 objReturnRawCast:
                                                objReturn = objReturnRawCast;
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke();
                }
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<T1, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = await Task.Run(() => funcToRun(null), token);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (!myControlCopy.IsNullOrDisposed())
                                    {
                                        switch (myControlCopy.EndInvoke(objInvoke))
                                        {
                                            case Exception ex:
                                                throw ex;
                                            case T2 objReturnRawCast:
                                                objReturn = objReturnRawCast;
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke(myControlCopy);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = await Task.Run(() => funcToRun(token), token);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                            // ReSharper disable once AccessToDisposedClosure
                        using (token.Register(() => objInnerTokenSource.Cancel(false)))
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun, objInnerToken);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (!myControlCopy.IsNullOrDisposed())
                                    {
                                        switch (myControlCopy.EndInvoke(objInvoke))
                                        {
                                            case Exception ex:
                                                throw ex;
                                            case T2 objReturnRawCast:
                                                objReturn = objReturnRawCast;
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke(token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Note: we cannot do a flag hack here because .GetAwaiter().GetResult() can run into object disposed issues for this special case.
        public static async Task<T2> DoThreadSafeFuncAsync<T1, T2>(this T1 objControl, Func<T1, CancellationToken, T2> funcToRun, CancellationToken token = default) where T1 : Control
        {
            token.ThrowIfCancellationRequested();
            if (funcToRun == null)
                return default;
            T2 objReturn = default;
            try
            {
                if (objControl == null)
                    objReturn = await Task.Run(() => funcToRun(null, token), token);
                else if (!objControl.Disposing && !objControl.IsDisposed)
                {
                    T1 myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CancellationTokenSource objInnerTokenSource = new CancellationTokenSource())
                        // ReSharper disable once AccessToDisposedClosure
                        using (token.Register(() => objInnerTokenSource.Cancel(false)))
                        {
                            void MyControlCopyOnDisposed(object sender, EventArgs e)
                            {
                                try
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    objInnerTokenSource.Cancel(false);
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            CancellationToken objInnerToken = objInnerTokenSource.Token;
                            myControlCopy.Disposed += MyControlCopyOnDisposed;
                            try
                            {
                                // Second check needed just in case form got disposed in the meantime
                                if (!objControl.Disposing && !objControl.IsDisposed)
                                {
                                    IAsyncResult objInvoke = myControlCopy.BeginInvoke(funcToRun, myControlCopy, objInnerToken);
                                    using (WaitHandle objHandle = objInvoke.AsyncWaitHandle)
                                        await objHandle.WaitOneAsync(objInnerToken);
                                    token.ThrowIfCancellationRequested();
                                    objInnerToken.ThrowIfCancellationRequested();
                                    if (!myControlCopy.IsNullOrDisposed())
                                    {
                                        switch (myControlCopy.EndInvoke(objInvoke))
                                        {
                                            case Exception ex:
                                                throw ex;
                                            case T2 objReturnRawCast:
                                                objReturn = objReturnRawCast;
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                myControlCopy.Disposed -= MyControlCopyOnDisposed;
                            }
                        }
                    }
                    else
                        objReturn = funcToRun.Invoke(myControlCopy, token);
                }
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
                Program.ShowMessageBox(objControl, e.ToString());
#endif
                throw;
            }

            return objReturn;
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
            Control objThreadReferrer = Program.MainForm ?? objControl;
            objThreadReferrer.DoThreadSafe(() =>
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
        /// Bind a control's property to a property via OnPropertyChanged
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        public static void DoDataBinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (objControl == null)
                return;
            Control objThreadReferrer = Program.MainForm ?? objControl;
            objThreadReferrer.DoThreadSafe(() =>
            {
                if (!objControl.IsHandleCreated)
                {
                    IntPtr _ = objControl.Handle; // accessing Handle forces its creation
                }

                objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false,
                                            DataSourceUpdateMode.OnPropertyChanged);
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
        public static void DoOneWayNegatableDataBinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (objControl == null)
                return;
            Control objThreadReferrer = Program.MainForm ?? objControl;
            objThreadReferrer.DoThreadSafe(() =>
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
            Control objThreadReferrer = Program.MainForm ?? objControl;
            objThreadReferrer.DoThreadSafe(() =>
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

        public static void PopulateWithListItems(this ListBox lsbThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            lsbThis?.DoThreadSafe(x => PopulateWithListItemsCore(x, lstItems, token));
        }

        public static Task PopulateWithListItemsAsync(this ListBox lsbThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            return lsbThis?.DoThreadSafeAsync(x => PopulateWithListItemsCore(x, lstItems, token), token) ?? Task.CompletedTask;
        }

        private static void PopulateWithListItemsCore(this ListBox lsbThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(lsbThis.DataSource, lstItems))
                return;
            // Binding multiple ComboBoxes to the same DataSource will also cause selected values to sync up between them.
            // Resetting bindings to prevent this though will also reset bindings to other properties, so that's not really an option
            // This means the code we use has to set the DataSources to new lists instead of the same one.
            List<ListItem> lstItemsToSet = null;
            if (lstItems != null)
            {
                lstItemsToSet = Utils.ListItemListPool.Get();
                lstItemsToSet.AddRange(lstItems);
            }
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
                else if (lstItemsToSet != null && lstCurrentList.SequenceEqual(lstItemsToSet))
                    return;

                token.ThrowIfCancellationRequested();
                List<ListItem> lstOldItems = null;
                if (lsbThis.DataSource != null)
                {
                    lstOldItems
                        = lsbThis
                                .DataSource as
                            List<ListItem>; // If the old DataSource is a List<ListItem>, make sure we can return it to the pool
                    lsbThis.BindingContext = new BindingContext();
                }
                token.ThrowIfCancellationRequested();
                lsbThis.DataSource = lstItemsToSet;
                token.ThrowIfCancellationRequested();
                if (lstOldItems != null)
                    Utils.ListItemListPool.Return(lstOldItems);
            }
            finally
            {
                lsbThis.EndUpdate();
            }
        }

        public static void PopulateWithListItems(this ComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            cboThis?.DoThreadSafe(x => PopulateWithListItemsCore(x, lstItems, token));
        }

        public static Task PopulateWithListItemsAsync(this ComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            return cboThis?.DoThreadSafeAsync(x => PopulateWithListItemsCore(x, lstItems, token), token) ?? Task.CompletedTask;
        }

        private static void PopulateWithListItemsCore(this ComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(cboThis.DataSource, lstItems))
                return;
            // Binding multiple ComboBoxes to the same DataSource will also cause selected values to sync up between them.
            // Resetting bindings to prevent this though will also reset bindings to other properties, so that's not really an option
            // This means the code we use has to set the DataSources to new lists instead of the same one.
            List<ListItem> lstItemsToSet = null;
            if (lstItems != null)
            {
                lstItemsToSet = Utils.ListItemListPool.Get();
                lstItemsToSet.AddRange(lstItems);
            }
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
                else if (lstItemsToSet != null && lstCurrentList.SequenceEqual(lstItemsToSet))
                    return;

                token.ThrowIfCancellationRequested();
                List<ListItem> lstOldItems = null;
                if (cboThis.DataSource != null)
                {
                    lstOldItems
                        = cboThis
                                .DataSource as
                            List<ListItem>; // If the old DataSource is a List<ListItem>, make sure we can return it to the pool
                    cboThis.BindingContext = new BindingContext();
                }
                token.ThrowIfCancellationRequested();
                cboThis.DataSource = lstItemsToSet;
                token.ThrowIfCancellationRequested();
                if (lstOldItems != null)
                    Utils.ListItemListPool.Return(lstOldItems);
            }
            finally
            {
                cboThis.EndUpdate();
            }
        }

        public static void PopulateWithListItems(this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            cboThis?.DoThreadSafe(x => PopulateWithListItemsCore(x, lstItems, token));
        }

        public static Task PopulateWithListItemsAsync(this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            return cboThis?.DoThreadSafeAsync(x => PopulateWithListItemsCore(x, lstItems, token), token) ?? Task.CompletedTask;
        }

        private static void PopulateWithListItemsCore(this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ReferenceEquals(cboThis.DataSource, lstItems))
                return;
            // Binding multiple ComboBoxes to the same DataSource will also cause selected values to sync up between them.
            // Resetting bindings to prevent this though will also reset bindings to other properties, so that's not really an option
            // This means the code we use has to set the DataSources to new lists instead of the same one.
            List<ListItem> lstItemsToSet = null;
            if (lstItems != null)
            {
                lstItemsToSet = Utils.ListItemListPool.Get();
                lstItemsToSet.AddRange(lstItems);
            }
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
                else if (lstItemsToSet != null && lstCurrentList.SequenceEqual(lstItemsToSet))
                    return;

                token.ThrowIfCancellationRequested();
                List<ListItem> lstOldItems = null;
                if (cboThis.DataSource != null)
                {
                    lstOldItems
                        = cboThis
                                .DataSource as
                            List<ListItem>; // If the old DataSource is a List<ListItem>, make sure we can return it to the pool
                    cboThis.BindingContext = new BindingContext();
                }
                token.ThrowIfCancellationRequested();
                cboThis.DataSource = lstItemsToSet;
                token.ThrowIfCancellationRequested();
                if (lstOldItems != null)
                    Utils.ListItemListPool.Return(lstOldItems);
            }
            finally
            {
                cboThis.EndUpdate();
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
        public static void SortCustomOrder(this TreeView treView)
        {
            if (treView == null)
                return;
            string strSelectedNodeTag = (treView.SelectedNode?.Tag as IHasInternalId)?.InternalId;

            IComparer currentSorter = treView.TreeViewNodeSorter;
            treView.TreeViewNodeSorter = new CustomNodeSorter();
            treView.Sort();
            treView.TreeViewNodeSorter = currentSorter;

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
                if (!((x as TreeNode)?.Tag is ICanSort lhs))
                    return -1;
                return !((y as TreeNode)?.Tag is ICanSort rhs) ? 1 : lhs.SortOrder.CompareTo(rhs.SortOrder);
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
    }
}
