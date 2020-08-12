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
using System.Windows.Forms;

namespace Chummer
{
    public class CursorWait : IDisposable
    {
        private static bool _blnTopMostWaitCursor;
        private readonly bool _blnOldUseWaitCursor;
        private readonly bool _blnControlIsForm;
        private readonly Control _objControl;
        private readonly Cursor _objOldCursor;
        private readonly Form _objControlTopParent;
        private readonly Cursor _objOldCursorTopParent;
        public CursorWait(Control objControl = null, bool blnAppStarting = false)
        {
            // Wait
            _objControl = objControl;
            if (_objControl?.IsDisposed != false)
            {
                if (!_blnTopMostWaitCursor)
                {
                    _blnTopMostWaitCursor = true;
                    _blnOldUseWaitCursor = Application.UseWaitCursor;
                    Application.UseWaitCursor = true;
                }
            }
            else
            {
                _blnControlIsForm = objControl is Form;
                Cursor objNewCursor = blnAppStarting ? Cursors.AppStarting : Cursors.WaitCursor;
                if (objNewCursor != Cursors.AppStarting && !_blnControlIsForm)
                {
                    if (_objControl is UserControl objUserControl)
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

                    _objOldCursorTopParent = _objControlTopParent?.Cursor ?? Cursors.Default;
                }

                _objOldCursor = _objControl.Cursor;
                _objControl.DoThreadSafe(() => _objControl.Cursor = objNewCursor);
                _objControlTopParent.DoThreadSafe(() => _objControlTopParent.Cursor = objNewCursor);
                if (_blnControlIsForm)
                    _objControl.SuspendLayout();
            }
        }

        private bool _blnDisposed;

        public void Dispose()
        {
            if (_blnDisposed)
                return;

            if (_objControl?.IsDisposed != false)
            {
                if (_blnTopMostWaitCursor)
                {
                    _blnTopMostWaitCursor = false;
                    Application.UseWaitCursor = _blnOldUseWaitCursor;
                }
            }
            else
            {
                _objControl.DoThreadSafe(() => _objControl.Cursor = _objOldCursor);
                _objControlTopParent?.DoThreadSafe(() => _objControlTopParent.Cursor = _objOldCursorTopParent);
                if (_blnControlIsForm)
                    _objControl.ResumeLayout();
            }

            _blnDisposed = true;
        }
    }
}
