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
using Chummer.Plugins;
using Chummer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Chummer
{
    public class CursorWait : IDisposable
    {
        UserControl _control = null;
        Form _form = null;
        public CursorWait(bool appStarting = false, UserControl control = null)
        {
            // Wait
            _control = control;
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            Program.MainForm.DoThreadSafe(() =>
            {
                if (_control == null) Application.UseWaitCursor = true;
                else _control.Cursor = Cursor.Current;
            });

            
        }

        public CursorWait(bool appStarting = false)
        {
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            Program.MainForm.DoThreadSafe(() =>
            {
                Application.UseWaitCursor = true;
            });


        }

        public CursorWait(bool appStarting = false, Form form = null)
        {
            // Wait
            _form = form;
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            Program.MainForm.DoThreadSafe(() =>
            {
                if (_form == null) Application.UseWaitCursor = true;
                else _form.Cursor = Cursor.Current;
            });

        }

        public void Dispose()
        {
            Program.MainForm.DoThreadSafe(() =>
            {
                // Reset
                Cursor.Current = Cursors.Default;
                Application.UseWaitCursor = false;
                if (_control != null)
                    _control.Cursor = Cursors.Default;
                if (_form != null)
                    _form.Cursor = Cursors.Default;
            });

        }
    }
}
