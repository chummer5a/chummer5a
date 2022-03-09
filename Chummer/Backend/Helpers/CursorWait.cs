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
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace Chummer
{
    public sealed class CursorWait : IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private static int _intApplicationWaitCursors;
        private static readonly ConcurrentDictionary<Control, int> s_DicWaitCursorControls = new ConcurrentDictionary<Control, int>();
        private static readonly ConcurrentDictionary<Control, int> s_DicApplicationStartingControls = new ConcurrentDictionary<Control, int>();
        private readonly bool _blnAppStartingCursor;
        private bool _blnDisposed;
        private readonly Control _objControl;
        private readonly Form _frmControlTopParent;
        private readonly Stopwatch _objTimer = new Stopwatch();
        private readonly Guid _guidInstance = Guid.NewGuid();

        public static CursorWait New(Control objControl = null, bool blnAppStarting = false)
        {
            CursorWait objReturn = new CursorWait(objControl, blnAppStarting);
            if (objReturn._objControl.IsNullOrDisposed())
            {
                if (Interlocked.Increment(ref _intApplicationWaitCursors) == 1)
                {
                    Application.UseWaitCursor = true;
                }
                return objReturn;
            }
            objReturn._objTimer.Start();
            Log.Trace("CursorWait for Control \"" + objControl + "\" started with Guid \"" + objReturn._guidInstance + "\".");

            if (objReturn._objControl != null)
            {
                if (objReturn._blnAppStartingCursor)
                {

                    s_DicApplicationStartingControls.AddOrUpdate(objReturn._objControl, 1,
                                                                 (x, y) => Interlocked.Increment(ref y));
                    if (!s_DicWaitCursorControls.TryGetValue(objReturn._objControl, out int intExitingWaits)
                        || intExitingWaits == 0)
                        objReturn.SetControlCursor(Cursors.AppStarting);
                }
                else
                {
                    s_DicWaitCursorControls.AddOrUpdate(objReturn._objControl, 1, (x, y) => Interlocked.Increment(ref y));
                    objReturn.SetControlCursor(Cursors.WaitCursor);
                }
            }
            return objReturn;
        }

        private CursorWait(Control objControl = null, bool blnAppStarting = false)
        {
            _objControl = objControl;
            Form frmControl = _objControl as Form;
            if (frmControl?.IsMdiChild != false)
            {
                if (frmControl != null)
                {
                    _frmControlTopParent = frmControl.MdiParent;
                }
                else if (_objControl is UserControl objUserControl)
                {
                    _frmControlTopParent = objUserControl.ParentForm;
                }
                else
                {
                    for (Control objLoop = _objControl?.Parent; objLoop != null; objLoop = objLoop.Parent)
                    {
                        if (objLoop is Form objLoopForm)
                        {
                            _frmControlTopParent = objLoopForm;
                            break;
                        }
                    }
                }
            }

            _blnAppStartingCursor = blnAppStarting;
        }

        private void SetControlCursor(Cursor objCursor)
        {
            if (objCursor != null)
            {
                // Only wait for the cursor change if we're changing to or from a full waiting cursor
                if (objCursor == Cursors.WaitCursor || _objControl.Cursor == Cursors.WaitCursor)
                {
                    _objControl.DoThreadSafe(x => x.Cursor = objCursor);
                    _frmControlTopParent?.DoThreadSafe(x => x.Cursor = objCursor);
                }
                else
                {
                    _objControl.QueueThreadSafe(x => x.Cursor = objCursor);
                    _frmControlTopParent?.QueueThreadSafe(x => x.Cursor = objCursor);
                }
            }
            else
            {
                _objControl.DoThreadSafe(x => x.ResetCursor());
                _frmControlTopParent?.DoThreadSafe(x => x.ResetCursor());
            }
        }

        public void Dispose()
        {
            if (_blnDisposed)
                return;
            _blnDisposed = true;
            if (_objControl == null)
            {
                if (Interlocked.Decrement(ref _intApplicationWaitCursors) == 0)
                {
                    Application.UseWaitCursor = false;
                }
                return;
            }
            Log.Trace("CursorWait for Control \"" + _objControl + "\" disposing with Guid \"" + _guidInstance + "\" after " + _objTimer.ElapsedMilliseconds + "ms.");
            _objTimer.Stop();
            if (_blnAppStartingCursor)
            {
                if (s_DicApplicationStartingControls.TryRemove(_objControl, out int intCurrentValue))
                {
                    if (Interlocked.Decrement(ref intCurrentValue) > 0)
                        s_DicApplicationStartingControls.AddOrUpdate(_objControl, intCurrentValue,
                                                                     (x, y) => y + intCurrentValue);
                    else if (!s_DicWaitCursorControls.TryGetValue(_objControl, out int intExitingWaits) || intExitingWaits == 0)
                        SetControlCursor(null);
                }
            }
            else if (s_DicWaitCursorControls.TryRemove(_objControl, out int intCurrentValue))
            {
                if (Interlocked.Decrement(ref intCurrentValue) > 0)
                    s_DicWaitCursorControls.AddOrUpdate(_objControl, intCurrentValue,
                                                        (x, y) => y + intCurrentValue);
                else if (s_DicApplicationStartingControls.TryGetValue(_objControl, out int intExitingWaits) && intExitingWaits > 0)
                    SetControlCursor(Cursors.AppStarting);
                else
                    SetControlCursor(null);
            }
        }
    }
}
