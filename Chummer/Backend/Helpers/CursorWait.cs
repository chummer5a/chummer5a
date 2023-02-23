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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public sealed class CursorWait : IDisposable, IAsyncDisposable
    {
        private static int _intApplicationWaitCursors;
        private static readonly ConcurrentDictionary<Control, int> s_DicCursorControls = new ConcurrentDictionary<Control, int>();
        private readonly bool _blnAppStartingCursor;
        private readonly Control _objControl;
        private Form _frmControlTopParent;
        private bool _blnDoUnsetCursorOnDispose;

        public static CursorWait New(Control objControl = null, bool blnAppStarting = false)
        {
            CursorWait objReturn = new CursorWait(objControl, blnAppStarting);
            if (objReturn._objControl == null)
            {
                if (Interlocked.Increment(ref _intApplicationWaitCursors) == 1)
                    Application.UseWaitCursor = true;
                objReturn._blnDoUnsetCursorOnDispose = true;
                return objReturn;
            }
            Form frmControl = objReturn._objControl as Form;
            if (frmControl?.DoThreadSafeFunc(x => x.IsMdiChild) != false)
            {
                if (frmControl != null)
                {
                    objReturn._frmControlTopParent = frmControl.DoThreadSafeFunc(x => x.MdiParent);
                }
                else if (objReturn._objControl is UserControl objUserControl)
                {
                    objReturn._frmControlTopParent = objUserControl.DoThreadSafeFunc(x => x.ParentForm);
                }
                else if (objReturn._objControl != null)
                {
                    for (Control objLoop = objReturn._objControl.DoThreadSafeFunc(x => x.Parent); objLoop != null; objLoop = objLoop.DoThreadSafeFunc(x => x.Parent))
                    {
                        if (objLoop is Form objLoopForm)
                        {
                            objReturn._frmControlTopParent = objLoopForm;
                            break;
                        }
                    }
                }
            }

            if (objReturn._objControl != null)
            {
                if (objReturn._blnAppStartingCursor)
                {
                    int intNewValue = s_DicCursorControls.AddOrUpdate(objReturn._objControl, 1, (x, y) => Interlocked.Increment(ref y));
                    objReturn.SetControlCursor(intNewValue < short.MaxValue ? Cursors.AppStarting : Cursors.WaitCursor);
                    objReturn._blnDoUnsetCursorOnDispose = true;
                }
                else
                {
                    s_DicCursorControls.AddOrUpdate(objReturn._objControl, short.MaxValue, (x, y) => Interlocked.Add(ref y, short.MaxValue));
                    objReturn.SetControlCursor(Cursors.WaitCursor);
                    objReturn._blnDoUnsetCursorOnDispose = true;
                }
            }
            return objReturn;
        }

        public static async Task<CursorWait> NewAsync(Control objControl = null, bool blnAppStarting = false, CancellationToken token = default)
        {
            CursorWait objReturn = new CursorWait(objControl, blnAppStarting);
            if (objReturn._objControl == null)
            {
                if (Interlocked.Increment(ref _intApplicationWaitCursors) == 1)
                    Application.UseWaitCursor = true;
                objReturn._blnDoUnsetCursorOnDispose = true;
                return objReturn;
            }
            Form frmControl = objReturn._objControl as Form;
            try
            {
                if (frmControl == null || await frmControl.DoThreadSafeFuncAsync(x => x.IsMdiChild, token).ConfigureAwait(false))
                {
                    if (frmControl != null)
                    {
                        objReturn._frmControlTopParent = await frmControl.DoThreadSafeFuncAsync(x => x.MdiParent, token).ConfigureAwait(false);
                    }
                    else if (objReturn._objControl is UserControl objUserControl)
                    {
                        objReturn._frmControlTopParent = await objUserControl.DoThreadSafeFuncAsync(x => x.ParentForm, token).ConfigureAwait(false);
                    }
                    else if (objReturn._objControl != null)
                    {
                        for (Control objLoop = await objReturn._objControl.DoThreadSafeFuncAsync(x => x.Parent, token).ConfigureAwait(false); objLoop != null; objLoop = await objLoop.DoThreadSafeFuncAsync(x => x.Parent, token).ConfigureAwait(false))
                        {
                            if (objLoop is Form objLoopForm)
                            {
                                objReturn._frmControlTopParent = objLoopForm;
                                break;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                await objReturn.DisposeAsync().ConfigureAwait(false);
                throw;
            }

            if (objReturn._objControl != null)
            {
                if (objReturn._blnAppStartingCursor)
                {
                    int intNewValue = s_DicCursorControls.AddOrUpdate(objReturn._objControl, 1, (x, y) => Interlocked.Increment(ref y));
                    try
                    {
                        await objReturn.SetControlCursorAsync(intNewValue < short.MaxValue ? Cursors.AppStarting : Cursors.WaitCursor, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        await objReturn.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                    objReturn._blnDoUnsetCursorOnDispose = true;
                }
                else
                {
                    s_DicCursorControls.AddOrUpdate(objReturn._objControl, short.MaxValue, (x, y) => Interlocked.Add(ref y, short.MaxValue));
                    try
                    {
                        await objReturn.SetControlCursorAsync(Cursors.WaitCursor, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        await objReturn.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                    objReturn._blnDoUnsetCursorOnDispose = true;
                }
            }
            return objReturn;
        }

        // If you are about to make this not-private, STOP! Use CursorWait.New() or CursorWait.NewAsync() instead
        private CursorWait(Control objControl = null, bool blnAppStarting = false)
        {
            _objControl = objControl;
            _blnAppStartingCursor = blnAppStarting;
        }

        private void SetControlCursor(Cursor objCursor)
        {
            if (objCursor != null)
            {
                _objControl.DoThreadSafe(x => x.Cursor = objCursor);
                _frmControlTopParent?.DoThreadSafe(x => x.Cursor = objCursor);
            }
            else
            {
                _objControl.DoThreadSafe(x => x.ResetCursor());
                _frmControlTopParent?.DoThreadSafe(x => x.ResetCursor());
            }
        }

        private async Task SetControlCursorAsync(Cursor objCursor, CancellationToken token = default)
        {
            if (objCursor != null)
            {
                await _objControl.DoThreadSafeAsync(x => x.Cursor = objCursor, token).ConfigureAwait(false);
                if (_frmControlTopParent != null)
                    await _frmControlTopParent.DoThreadSafeAsync(x => x.Cursor = objCursor, token).ConfigureAwait(false);
            }
            else
            {
                await _objControl.DoThreadSafeAsync(x => x.ResetCursor(), token).ConfigureAwait(false);
                if (_frmControlTopParent != null)
                    await _frmControlTopParent.DoThreadSafeAsync(x => x.ResetCursor(), token).ConfigureAwait(false);
            }
        }

        private int _intDisposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _intDisposed, 1) != 0)
                return;
            if (_objControl == null)
            {
                if (Interlocked.Decrement(ref _intApplicationWaitCursors) == 0 && _blnDoUnsetCursorOnDispose)
                {
                    Application.UseWaitCursor = false;
                }
                return;
            }
            if (_blnAppStartingCursor)
            {
                if (s_DicCursorControls.TryRemove(_objControl, out int intCurrentValue))
                {
                    int intDecrementedValue = Interlocked.Decrement(ref intCurrentValue);
                    if (intDecrementedValue > 0)
                        s_DicCursorControls.AddOrUpdate(_objControl, intDecrementedValue, (x, y) => y + intDecrementedValue);
                    else if (_blnDoUnsetCursorOnDispose)
                        SetControlCursor(null);
                }
            }
            else if (s_DicCursorControls.TryRemove(_objControl, out int intCurrentValue))
            {
                int intDecrementedValue = Interlocked.Add(ref intCurrentValue, -short.MaxValue);
                if (intDecrementedValue > 0)
                {
                    s_DicCursorControls.AddOrUpdate(_objControl, intDecrementedValue, (x, y) => y + intDecrementedValue);
                    if (_blnDoUnsetCursorOnDispose)
                        SetControlCursor(intDecrementedValue < short.MaxValue ? Cursors.AppStarting : Cursors.WaitCursor);
                }
                else if (_blnDoUnsetCursorOnDispose)
                    SetControlCursor(null);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _intDisposed, 1) != 0)
                return;
            if (_objControl == null)
            {
                if (Interlocked.Decrement(ref _intApplicationWaitCursors) == 0 && _blnDoUnsetCursorOnDispose)
                {
                    Application.UseWaitCursor = false;
                }
                return;
            }
            if (_blnAppStartingCursor)
            {
                if (s_DicCursorControls.TryRemove(_objControl, out int intCurrentValue))
                {
                    int intDecrementedValue = Interlocked.Decrement(ref intCurrentValue);
                    if (intDecrementedValue > 0)
                        s_DicCursorControls.AddOrUpdate(_objControl, intDecrementedValue, (x, y) => y + intDecrementedValue);
                    else if (_blnDoUnsetCursorOnDispose)
                        await SetControlCursorAsync(null).ConfigureAwait(false);
                }
            }
            else if (s_DicCursorControls.TryRemove(_objControl, out int intCurrentValue))
            {
                int intDecrementedValue = Interlocked.Add(ref intCurrentValue, -short.MaxValue);
                if (intDecrementedValue > 0)
                {
                    s_DicCursorControls.AddOrUpdate(_objControl, intDecrementedValue, (x, y) => y + intDecrementedValue);
                    if (_blnDoUnsetCursorOnDispose)
                        await SetControlCursorAsync(intDecrementedValue < short.MaxValue ? Cursors.AppStarting : Cursors.WaitCursor).ConfigureAwait(false);
                }
                else if (_blnDoUnsetCursorOnDispose)
                    await SetControlCursorAsync(null).ConfigureAwait(false);
            }
        }
    }
}
