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

using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;

namespace Chummer
{
    public class CursorWait : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly object _intApplicationWaitCursorsLock = new object();
        private static int _intApplicationWaitCursors;
        private static readonly ConcurrentDictionary<Control, ConcurrentStack<CursorWait>> _dicWaitingControls = new ConcurrentDictionary<Control, ConcurrentStack<CursorWait>>();
        private readonly Control _objControl;
        private readonly Form _objControlTopParent;
        private readonly Stopwatch objTimer = new Stopwatch();
        private readonly Guid instance = Guid.NewGuid();

        public CursorWait(Control objControl = null, bool blnAppStarting = false)
        {
            if (objControl?.Disposing != false || objControl.IsDisposed)
            {
                _objControl = null;
                lock (_intApplicationWaitCursorsLock)
                {
                    _intApplicationWaitCursors += 1;
                    if (_intApplicationWaitCursors > 0)
                        Application.UseWaitCursor = true;
                }
                return;
            }
            objTimer.Start();
            Log.Trace("CursorWait for Control \"" + objControl + "\" started with Guid \"" + instance.ToString() + "\".");
            _objControl = objControl;
            Form frmControl = objControl as Form;
            CursorToUse = blnAppStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            if (frmControl?.IsMdiChild != false)
            {
                if (frmControl != null)
                {
                    _objControlTopParent = frmControl.MdiParent;
                }
                else if (_objControl is UserControl objUserControl)
                {
                    _objControlTopParent = objUserControl.ParentForm;
                }
                else
                {
                    for (Control objLoop = _objControl.Parent; objLoop != null; objLoop = objLoop.Parent)
                    {
                        if (objLoop is Form objLoopForm)
                        {
                            _objControlTopParent = objLoopForm;
                            break;
                        }
                    }
                }
            }
            ConcurrentStack<CursorWait> stkNew = new ConcurrentStack<CursorWait>();
            while (!_dicWaitingControls.TryAdd(objControl, stkNew))
            {
                if (!_dicWaitingControls.TryGetValue(objControl, out ConcurrentStack<CursorWait> stkExisting))
                    continue;
                CursorWait objLastCursorWait = stkExisting.Peek();
                stkExisting.Push(this);
                if (blnAppStarting)
                {
                    if (objLastCursorWait == null)
                        SetControlCursor(CursorToUse);
                    else if (objLastCursorWait.CursorToUse == Cursors.WaitCursor)
                        CursorToUse = Cursors.WaitCursor;
                }
                else if (objLastCursorWait == null || objLastCursorWait.CursorToUse == Cursors.AppStarting)
                    SetControlCursor(CursorToUse);
                return;
            }
            stkNew.Push(this);
            SetControlCursor(CursorToUse);
        }

        private void SetControlCursor(Cursor objCursor)
        {
            if (objCursor != null)
            {
                _objControl.DoThreadSafe(() => _objControl.Cursor = objCursor);
                _objControlTopParent?.DoThreadSafe(() => _objControlTopParent.Cursor = objCursor);
            }
            else
            {
                _objControl.DoThreadSafe(() => _objControl.ResetCursor());
                _objControlTopParent?.DoThreadSafe(() => _objControlTopParent.ResetCursor());
            }
        }

        public Cursor CursorToUse { get; }

        private bool _blnDisposed;

        public void Dispose()
        {
            if (_blnDisposed)
                return;
            _blnDisposed = true;
            if (_objControl == null)
            {
                lock (_intApplicationWaitCursorsLock)
                {
                    _intApplicationWaitCursors -= 1;
                    if (_intApplicationWaitCursors <= 0)
                        Application.UseWaitCursor = false;
                }
                return;
            }
            Log.Trace("CursorWait for Control \"" + _objControl + "\" disposing with Guid \"" + instance.ToString() + "\" after " + objTimer.ElapsedMilliseconds + "ms.");
            objTimer.Stop();
            if (!_dicWaitingControls.TryGetValue(_objControl, out ConcurrentStack<CursorWait> stkCursorWaits) || stkCursorWaits == null || stkCursorWaits.Count <= 0)
            {
                Utils.BreakIfDebug();
                Log.Error("CursorWait for Control \"" + _objControl + "\" with Guid \"" + instance.ToString() + "\" somehow does not have a CursorWait stack defined for it");
                throw new ArgumentNullException(nameof(stkCursorWaits));
            }
            CursorWait objPoppedCursorWait = stkCursorWaits.Pop();
            if (objPoppedCursorWait != this)
            {
                Utils.BreakIfDebug();
                Log.Error("CursorWait for Control \"" + _objControl + "\" with Guid \"" + instance.ToString() + "\" somehow does not have a CursorWait stack defined for it");
                throw new ArgumentNullException(nameof(objPoppedCursorWait));
            }
            SetControlCursor(stkCursorWaits.Peek()?.CursorToUse);
        }
    }
}
