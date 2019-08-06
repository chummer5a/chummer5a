using Chummer.Plugins;
using Chummer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.UI;
using System.Diagnostics;

namespace ChummerHub.Client.Backend
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
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                if (_control == null) Application.UseWaitCursor = true;
                else _control.Cursor = Cursor.Current;
            });

            
        }

        public CursorWait(bool appStarting = false)
        {
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                Application.UseWaitCursor = true;
            });


        }

        public CursorWait(bool appStarting = false, Form form = null)
        {
            // Wait
            _form = form;
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                if (_form == null) Application.UseWaitCursor = true;
                else _form.Cursor = Cursor.Current;
            });

        }

        public void Dispose()
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
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
