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
        /// <returns></returns>
        public static DialogResult ShowDialogSafe(this Form frmForm, IWin32Window owner = null)
        {
            if (!Utils.IsUnitTest)
                return frmForm.ShowDialog(owner);
            // Unit tests cannot use ShowDialog because that will stall them out
            bool blnDoClose = false;
            void FormOnShown(object sender, EventArgs args) => blnDoClose = true;
            frmForm.Shown += FormOnShown;
            frmForm.ShowInTaskbar = false;
            frmForm.Show(owner);
            while (!blnDoClose)
                Utils.SafeSleep(true);
            frmForm.Close();
            return frmForm.DialogResult;
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="objCharacter"></param>
        /// <returns></returns>
        public static DialogResult ShowDialogSafe(this Form frmForm, Character objCharacter)
        {
            return frmForm.ShowDialogSafe(Program.GetFormForDialog(objCharacter));
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogSafeAsync(this Form frmForm, IWin32Window owner = null)
        {
            // Unit tests cannot use ShowDialog because that will stall them out
            return !Utils.IsUnitTest ? Task.FromResult(frmForm.ShowDialog(owner)) : ShowDialogSafeUnitTestAsync();

            async Task<DialogResult> ShowDialogSafeUnitTestAsync()
            {
                bool blnDoClose = false;
                void FormOnShown(object sender, EventArgs args) => blnDoClose = true;
                frmForm.Shown += FormOnShown;
                frmForm.Show(owner);
                while (!blnDoClose)
                    await Utils.SafeSleepAsync();
                frmForm.Close();
                return frmForm.DialogResult;
            }
        }

        /// <summary>
        /// Alternative to Form.ShowDialog() that will not stall out unit tests.
        /// </summary>
        /// <param name="frmForm"></param>
        /// <param name="objCharacter"></param>
        /// <returns></returns>
        public static Task<DialogResult> ShowDialogSafeAsync(this Form frmForm, Character objCharacter)
        {
            return frmForm.ShowDialogSafeAsync(Program.GetFormForDialog(objCharacter));
        }

        #endregion

        #region Controls Extensions

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoThreadSafe(this Control objControl, Action funcToRun)
        {
            objControl.DoThreadSafeCore(true, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoThreadSafe(this Control objControl, Action<Control> funcToRun)
        {
            objControl.DoThreadSafeCore(true, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoThreadSafe(this Control objControl, Func<Task> funcToRun)
        {
            objControl.DoThreadSafeCore(true, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and waits for it to complete.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoThreadSafe(this Control objControl, Func<Control, Task> funcToRun)
        {
            objControl.DoThreadSafeCore(true, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner without waiting for the code to complete before continuing.
        /// If you want to wait for the code to complete, use DoThreadSafe or DoThreadSafeAsync instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueThreadSafe(this Control objControl, Action funcToRun)
        {
            objControl.DoThreadSafeCore(false, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner without waiting for the code to complete before continuing.
        /// If you want to wait for the code to complete, use DoThreadSafe or DoThreadSafeAsync instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueThreadSafe(this Control objControl, Action<Control> funcToRun)
        {
            objControl.DoThreadSafeCore(false, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner without waiting for the code to complete before continuing.
        /// If you want to wait for the code to complete, use DoThreadSafe or DoThreadSafeAsync instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueThreadSafe(this Control objControl, Func<Task> funcToRun)
        {
            objControl.DoThreadSafeCore(false, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner without waiting for the code to complete before continuing.
        /// If you want to wait for the code to complete, use DoThreadSafe or DoThreadSafeAsync instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QueueThreadSafe(this Control objControl, Func<Control, Task> funcToRun)
        {
            objControl.DoThreadSafeCore(false, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner, but using a void means that this method is not awaitable.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="blnSync">Whether to wait for the invocation to complete (True) or to keep going without waiting (False).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DoThreadSafeCore(this Control objControl, bool blnSync, Action funcToRun)
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl.IsNullOrDisposed())
                {
                    if (blnSync)
                        funcToRun.Invoke();
                    else
                        Task.Run(funcToRun);
                }
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        if (blnSync)
                        {
                            IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun);
                            // Next two commands ensure easier debugging, prevent spamming of invokes to the UI thread that would cause lock-ups, and ensure safe invoke handle disposal
                            objResult.AsyncWaitHandle.WaitOne();
                            objResult.AsyncWaitHandle.Close();
                        }
                        else
                            myControlCopy.BeginInvoke(funcToRun);
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner, but using a void means that this method is not awaitable.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="blnSync">Whether to wait for the invocation to complete (True) or to keep going without waiting (False).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DoThreadSafeCore(this Control objControl, bool blnSync, Action<Control> funcToRun)
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl.IsNullOrDisposed())
                {
                    if (blnSync)
                        funcToRun.Invoke(objControl);
                    else
                        Task.Run(() => funcToRun(objControl));
                }
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        if (blnSync)
                        {
                            IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                            // Next two commands ensure easier debugging, prevent spamming of invokes to the UI thread that would cause lock-ups, and ensure safe invoke handle disposal
                            objResult.AsyncWaitHandle.WaitOne();
                            objResult.AsyncWaitHandle.Close();
                        }
                        else
                            myControlCopy.BeginInvoke(funcToRun, myControlCopy);
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner, but using a void means that this method is not awaitable.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="blnSync">Whether to wait for the invocation to complete (True) or to keep going without waiting (False).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DoThreadSafeCore(this Control objControl, bool blnSync, Func<Task> funcToRun)
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl.IsNullOrDisposed())
                {
                    if (blnSync)
                    {
                        Task tskRunning = funcToRun.Invoke();
                        if (tskRunning.Status == TaskStatus.Created)
                            tskRunning.RunSynchronously();
                        while (!tskRunning.IsCompleted)
                            Utils.SafeSleep();
                        if (tskRunning.Exception != null)
                            throw tskRunning.Exception;
                    }
                    else
                    {
                        Task.Run(() => funcToRun.Invoke().ContinueWith(x =>
                        {
                            if (x.Exception != null)
                                throw x.Exception;
                        }));
                    }
                }
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun);
                        // funcToRun actually creates a Task that performs what is being run, so we need to get that task and then work with the task instead of the IAsyncResult
                        objResult.AsyncWaitHandle.WaitOne();
                        object objReturnRaw = myControlCopy.EndInvoke(objResult);
                        if (objReturnRaw is Task tskRunning)
                        {
                            if (blnSync)
                            {
                                if (tskRunning.Status == TaskStatus.Created)
                                    tskRunning.RunSynchronously();
                                while (!tskRunning.IsCompleted)
                                    Utils.SafeSleep();
                                if (tskRunning.Exception != null)
                                    throw tskRunning.Exception;
                            }
                            else
                            {
                                Task.Run(() => tskRunning.ContinueWith(x =>
                                {
                                    if (x.Exception != null)
                                        throw x.Exception;
                                }));
                            }
                        }

                        objResult.AsyncWaitHandle.Close();
                    }
                    else
                    {
                        Task tskRunning = funcToRun.Invoke();
                        if (tskRunning.Status == TaskStatus.Created)
                            tskRunning.RunSynchronously();
                        while (!tskRunning.IsCompleted)
                            Utils.SafeSleep();
                        if (tskRunning.Exception != null)
                            throw tskRunning.Exception;
                    }
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner, but using a void means that this method is not awaitable.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="blnSync">Whether to wait for the invocation to complete (True) or to keep going without waiting (False).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DoThreadSafeCore(this Control objControl, bool blnSync, Func<Control, Task> funcToRun)
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl.IsNullOrDisposed())
                {
                    if (blnSync)
                    {
                        Task tskRunning = funcToRun.Invoke(objControl);
                        if (tskRunning.Status == TaskStatus.Created)
                            tskRunning.RunSynchronously();
                        while (!tskRunning.IsCompleted)
                            Utils.SafeSleep();
                        if (tskRunning.Exception != null)
                            throw tskRunning.Exception;
                    }
                    else
                    {
                        Task.Run(() => funcToRun.Invoke(objControl).ContinueWith(x =>
                        {
                            if (x.Exception != null)
                                throw x.Exception;
                        }));
                    }
                }
                else
                {
                    // ReSharper disable once InlineTemporaryVariable
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                        // funcToRun actually creates a Task that performs what is being run, so we need to get that task and then work with the task instead of the IAsyncResult
                        objResult.AsyncWaitHandle.WaitOne();
                        object objReturnRaw = myControlCopy.EndInvoke(objResult);
                        if (objReturnRaw is Task tskRunning)
                        {
                            if (blnSync)
                            {
                                if (tskRunning.Status == TaskStatus.Created)
                                    tskRunning.RunSynchronously();
                                while (!tskRunning.IsCompleted)
                                    Utils.SafeSleep();
                                if (tskRunning.Exception != null)
                                    throw tskRunning.Exception;
                            }
                            else
                            {
                                Task.Run(() => tskRunning.ContinueWith(x =>
                                {
                                    if (x.Exception != null)
                                        throw x.Exception;
                                }));
                            }
                        }

                        objResult.AsyncWaitHandle.Close();
                    }
                    else
                    {
                        Task tskRunning = funcToRun.Invoke(myControlCopy);
                        if (tskRunning.Status == TaskStatus.Created)
                            tskRunning.RunSynchronously();
                        while (!tskRunning.IsCompleted)
                            Utils.SafeSleep();
                        if (tskRunning.Exception != null)
                            throw tskRunning.Exception;
                    }
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DoThreadSafeAsync(this Control objControl, Action funcToRun)
        {
            if (funcToRun == null)
                return;
            if (objControl.IsNullOrDisposed())
                await Task.Run(funcToRun);
            else
            {
                try
                {
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun);
                        await Task.Factory.FromAsync(objResult, x => myControlCopy.EndInvoke(x));
                    }
                    else
                        funcToRun.Invoke();
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
                catch (System.Threading.ThreadAbortException)
                {
                    //no need to do anything here - actually we can't anyway...
                }
                catch (Exception e)
                {
                    Log.Error(e);
#if DEBUG
                    Program.ShowMessageBox(objControl, e.ToString());
#endif
                }
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DoThreadSafeAsync(this Control objControl, Action<Control> funcToRun)
        {
            if (funcToRun == null)
                return;
            if (objControl.IsNullOrDisposed())
                await Task.Run(() => funcToRun(objControl));
            else
            {
                try
                {
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                        await Task.Factory.FromAsync(objResult, x => myControlCopy.EndInvoke(x));
                    }
                    else
                        funcToRun.Invoke(myControlCopy);
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
                catch (System.Threading.ThreadAbortException)
                {
                    //no need to do anything here - actually we can't anyway...
                }
                catch (Exception e)
                {
                    Log.Error(e);
#if DEBUG
                    Program.ShowMessageBox(objControl, e.ToString());
#endif
                }
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DoThreadSafeAsync(this Control objControl, Func<Task> funcToRun)
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl.IsNullOrDisposed())
                    await funcToRun.Invoke();
                else
                {
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun);
                        await Task.Factory.FromAsync(objResult, x => myControlCopy.EndInvoke(x));
                    }
                    else
                        await funcToRun.Invoke();
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner and in a way where it can get awaited.
        /// If you do not want to wait for the code to complete before moving on, use QueueThreadSafe instead.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DoThreadSafeAsync(this Control objControl, Func<Control, Task> funcToRun)
        {
            if (funcToRun == null)
                return;
            try
            {
                if (objControl.IsNullOrDisposed())
                    await funcToRun.Invoke(objControl);
                else
                {
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                        await Task.Factory.FromAsync(objResult, x => myControlCopy.EndInvoke(x));
                    }
                    else
                        await funcToRun.Invoke(myControlCopy);
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DoThreadSafeFunc<T>(this Control objControl, Func<T> funcToRun)
        {
            Task<T> objTask = objControl.DoThreadSafeFuncCoreAsync(true, funcToRun);
            if (objTask.Status == TaskStatus.Created)
                objTask.RunSynchronously();
            if (objTask.Exception != null)
                throw objTask.Exception;
            return objTask.Result;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DoThreadSafeFunc<T>(this Control objControl, Func<Control, T> funcToRun)
        {
            Task<T> objTask = objControl.DoThreadSafeFuncCoreAsync(true, funcToRun);
            if (objTask.Status == TaskStatus.Created)
                objTask.RunSynchronously();
            if (objTask.Exception != null)
                throw objTask.Exception;
            return objTask.Result;
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> DoThreadSafeFuncAsync<T>(this Control objControl, Func<T> funcToRun)
        {
            return objControl.DoThreadSafeFuncCoreAsync(false, funcToRun);
        }

        /// <summary>
        /// Runs code that returns a value on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> DoThreadSafeFuncAsync<T>(this Control objControl, Func<Control, T> funcToRun)
        {
            return objControl.DoThreadSafeFuncCoreAsync(false, funcToRun);
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="blnSync">Whether to wait for the invocation to complete (True) or to keep going without waiting (False).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<T> DoThreadSafeFuncCoreAsync<T>(this Control objControl, bool blnSync, Func<T> funcToRun)
        {
            if (funcToRun == null)
                return default;
            T objReturn = default;
            try
            {
                if (objControl.IsNullOrDisposed())
                    objReturn = blnSync ? funcToRun.Invoke() : await Task.Run(funcToRun);
                else
                {
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun);
                        if (blnSync)
                        {
                            // Next two commands ensure easier debugging, prevent spamming of invokes to the UI thread that would cause lock-ups, and ensure safe invoke handle disposal
                            objResult.AsyncWaitHandle.WaitOne();
                            object objReturnRaw = myControlCopy.EndInvoke(objResult);
                            if (objReturnRaw is T objReturnRawCast)
                                objReturn = objReturnRawCast;
                            objResult.AsyncWaitHandle.Close();
                        }
                        else
                        {
                            await Task.Factory.FromAsync(objResult, x =>
                            {
                                object objReturnRaw = myControlCopy.EndInvoke(objResult);
                                if (objReturnRaw is T objReturnRawCast)
                                    objReturn = objReturnRawCast;
                            });
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
            }

            return objReturn;
        }

        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        /// <param name="blnSync">Whether to wait for the invocation to complete (True) or to keep going without waiting (False).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<T> DoThreadSafeFuncCoreAsync<T>(this Control objControl, bool blnSync, Func<Control, T> funcToRun)
        {
            if (funcToRun == null)
                return default;
            T objReturn = default;
            try
            {
                if (objControl.IsNullOrDisposed())
                    objReturn = blnSync ? funcToRun.Invoke(objControl) : await Task.Run(() => funcToRun(objControl));
                else
                {
                    Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                    if (myControlCopy.InvokeRequired)
                    {
                        IAsyncResult objResult = myControlCopy.BeginInvoke(funcToRun, myControlCopy);
                        if (blnSync)
                        {
                            // Next two commands ensure easier debugging, prevent spamming of invokes to the UI thread that would cause lock-ups, and ensure safe invoke handle disposal
                            objResult.AsyncWaitHandle.WaitOne();
                            object objReturnRaw = myControlCopy.EndInvoke(objResult);
                            if (objReturnRaw is T objReturnRawCast)
                                objReturn = objReturnRawCast;
                            objResult.AsyncWaitHandle.Close();
                        }
                        else
                        {
                            await Task.Factory.FromAsync(objResult, x =>
                            {
                                object objReturnRaw = myControlCopy.EndInvoke(objResult);
                                if (objReturnRaw is T objReturnRawCast)
                                    objReturn = objReturnRawCast;
                            });
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
            catch (System.Threading.ThreadAbortException)
            {
                //no need to do anything here - actually we can't anyway...
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.ShowMessageBox(objControl, e.ToString());
#endif
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
            if (!objControl.IsHandleCreated)
            {
                IntPtr _ = objControl.Handle; // accessing Handle forces its creation
            }

            objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false, DataSourceUpdateMode.Never);
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
            if (!objControl.IsHandleCreated)
            {
                IntPtr _ = objControl.Handle; // accessing Handle forces its creation
            }
            objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false, DataSourceUpdateMode.OnPropertyChanged);
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
            if (!objControl.IsHandleCreated)
            {
                IntPtr _ = objControl.Handle; // accessing Handle forces its creation
            }
            objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, false, DataSourceUpdateMode.Never));
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
            if (!objControl.IsHandleCreated)
            {
                IntPtr _ = objControl.Handle; // accessing Handle forces its creation
            }
            objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, false, DataSourceUpdateMode.OnPropertyChanged));
        }

        /// <summary>
        /// Syntactic sugar for what is effectively a null check for disposable WinForms controls.
        /// </summary>
        /// <param name="objControl"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed([CanBeNull] this Control objControl)
        {
            return objControl?.Disposing != false || objControl.IsDisposed;
        }

        #endregion Controls Extensions

        #region ComboBox Extensions

        public static void PopulateWithListItems(this ListBox lsbThis, IEnumerable<ListItem> lstItems)
        {
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
            if (!(lsbThis.DataSource is IEnumerable<ListItem> lstCurrentList))
            {
                lsbThis.ValueMember = nameof(ListItem.Value);
                lsbThis.DisplayMember = nameof(ListItem.Name);
            }
            // Setting DataSource is slow because WinForms is old, so let's make sure we definitely need to do it
            else if (lstItemsToSet != null && lstCurrentList.SequenceEqual(lstItemsToSet))
                return;
            List<ListItem> lstOldItems = null;
            if (lsbThis.DataSource != null)
            {
                lstOldItems = lsbThis.DataSource as List<ListItem>; // If the old DataSource is a List<ListItem>, make sure we can return it to the pool
                lsbThis.BindingContext = new BindingContext();
            }
            lsbThis.DataSource = lstItemsToSet;
            if (lstOldItems != null)
                Utils.ListItemListPool.Return(lstOldItems);
        }

        public static void PopulateWithListItems(this ComboBox cboThis, IEnumerable<ListItem> lstItems)
        {
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
            if (!(cboThis.DataSource is IEnumerable<ListItem> lstCurrentList))
            {
                cboThis.ValueMember = nameof(ListItem.Value);
                cboThis.DisplayMember = nameof(ListItem.Name);
            }
            // Setting DataSource is slow because WinForms is old, so let's make sure we definitely need to do it
            else if (lstItemsToSet != null && lstCurrentList.SequenceEqual(lstItemsToSet))
                return;
            List<ListItem> lstOldItems = null;
            if (cboThis.DataSource != null)
            {
                lstOldItems = cboThis.DataSource as List<ListItem>; // If the old DataSource is a List<ListItem>, make sure we can return it to the pool
                cboThis.BindingContext = new BindingContext();
            }
            cboThis.DataSource = lstItemsToSet;
            if (lstOldItems != null)
                Utils.ListItemListPool.Return(lstOldItems);
        }

        public static void PopulateWithListItems(this ElasticComboBox cboThis, IEnumerable<ListItem> lstItems)
        {
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
            if (!(cboThis.DataSource is IEnumerable<ListItem> lstCurrentList))
            {
                cboThis.ValueMember = nameof(ListItem.Value);
                cboThis.DisplayMember = nameof(ListItem.Name);
            }
            // Setting DataSource is slow because WinForms is old, so let's make sure we definitely need to do it
            else if (lstItemsToSet != null && lstCurrentList.SequenceEqual(lstItemsToSet))
                return;
            List<ListItem> lstOldItems = null;
            if (cboThis.DataSource != null)
            {
                lstOldItems = cboThis.DataSource as List<ListItem>; // If the old DataSource is a List<ListItem>, make sure we can return it to the pool
                cboThis.BindingContext = new BindingContext();
            }
            cboThis.DataSource = lstItemsToSet;
            if (lstOldItems != null)
                Utils.ListItemListPool.Return(lstOldItems);
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
