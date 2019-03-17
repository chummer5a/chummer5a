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
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                if (control == null) Application.UseWaitCursor = true;
                else control.Cursor = Cursor.Current;
            });

            
        }

        public CursorWait(bool appStarting = false, Form form = null)
        {
            // Wait
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                if (form == null) Application.UseWaitCursor = true;
                else form.Cursor = Cursor.Current;
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
