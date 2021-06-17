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
        private static readonly ConcurrentDictionary<Control, ThreadSafeList<CursorWait>> s_dicWaitingControls = new ConcurrentDictionary<Control, ThreadSafeList<CursorWait>>();
        private readonly Control _objControl;
        private readonly Form _frmControlTopParent;
        private readonly Stopwatch objTimer = new Stopwatch();
        private readonly Guid instance = Guid.NewGuid();

        public CursorWait(Control objControl = null, bool blnAppStarting = false)
        {
            if (objControl.IsNullOrDisposed())
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
            Form frmControl = _objControl as Form;
            CursorToUse = blnAppStarting ? Cursors.AppStarting : Cursors.WaitCursor;
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
            ThreadSafeList<CursorWait> lstNew = new ThreadSafeList<CursorWait>();
            while (_objControl != null && !s_dicWaitingControls.TryAdd(_objControl, lstNew))
            {
                if (!s_dicWaitingControls.TryGetValue(_objControl, out ThreadSafeList<CursorWait> lstExisting))
                    continue;
                CursorWait objLastCursorWait = null;
                // Need this pattern because the size of lstExisting might change in between fetching lstExisting.Count and lstExisting[]
                do
                {
                    int intIndex = lstExisting.Count - 1;
                    if (intIndex >= 0)
                    {
                        try
                        {
                            objLastCursorWait = lstExisting[intIndex];
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            continue;
                        }
                    }
                    break;
                } while (true);
                lstExisting.Add(this);
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
            // Here for safety purposes
            if (_objControl.IsNullOrDisposed())
            {
                _objControl = null;
                _frmControlTopParent = null;
                lock (_intApplicationWaitCursorsLock)
                {
                    _intApplicationWaitCursors += 1;
                    if (_intApplicationWaitCursors > 0)
                        Application.UseWaitCursor = true;
                }
                return;
            }
            lstNew.Add(this);
            SetControlCursor(CursorToUse);
        }

        private void SetControlCursor(Cursor objCursor)
        {
            if (objCursor != null)
            {
                // Only wait for the cursor change if we're changing to or from a full waiting cursor
                if (objCursor == Cursors.WaitCursor || _objControl.Cursor == Cursors.WaitCursor)
                {
                    _objControl.DoThreadSafe(() => _objControl.Cursor = objCursor);
                    _frmControlTopParent?.DoThreadSafe(() => _frmControlTopParent.Cursor = objCursor);
                }
                else
                {
                    _objControl.QueueThreadSafe(() => _objControl.Cursor = objCursor);
                    _frmControlTopParent?.QueueThreadSafe(() => _frmControlTopParent.Cursor = objCursor);
                }
            }
            else
            {
                _objControl.DoThreadSafe(() => _objControl.ResetCursor());
                _frmControlTopParent?.DoThreadSafe(() => _frmControlTopParent.ResetCursor());
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
            if (!s_dicWaitingControls.TryGetValue(_objControl, out ThreadSafeList<CursorWait> lstCursorWaits) || lstCursorWaits == null || lstCursorWaits.Count <= 0)
            {
                Utils.BreakIfDebug();
                Log.Error("CursorWait for Control \"" + _objControl + "\" with Guid \"" + instance.ToString() + "\" somehow does not have a CursorWait list defined for it");
                throw new ArgumentNullException(nameof(lstCursorWaits));
            }
            
            int intMyIndex = lstCursorWaits.FindLastIndex(x => x.Equals(this));
            if (intMyIndex < 0 || !lstCursorWaits.Remove(this))
            {
                Utils.BreakIfDebug();
                Log.Error("CursorWait for Control \"" + _objControl + "\" with Guid \"" + instance.ToString() + "\" somehow is not in the CursorWait list defined for it");
                throw new ArgumentNullException(nameof(intMyIndex));
            }
            if (intMyIndex >= lstCursorWaits.Count)
            {
                CursorWait objPreviousCursorWait = null;
                // Need this pattern because the size of lstExisting might change in between fetching lstExisting.Count and lstExisting[]
                do
                {
                    int intIndex = lstCursorWaits.Count - 1;
                    if (intIndex >= 0)
                    {
                        try
                        {
                            objPreviousCursorWait = lstCursorWaits[intIndex];
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            continue;
                        }
                    }
                    break;
                } while (true);
                SetControlCursor(objPreviousCursorWait?.CursorToUse);
            }
            if (lstCursorWaits.Count == 0)
                s_dicWaitingControls.TryRemove(_objControl, out ThreadSafeList<CursorWait> _);
        }
    }
}
