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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// MessageBox "Extension" that also centers itself based on the location of its parent, since the built-in MessageBox cannot do that
    /// https://stackoverflow.com/questions/1732443/center-messagebox-in-parent-form
    /// </summary>
    public static class CenterableMessageBox
    {
        private static IWin32Window _owner;
        private static readonly NativeMethods.HookProc s_HookProc = MessageBoxHookProc;
        private static IntPtr _hHook = IntPtr.Zero;

        public static DialogResult Show(string text)
        {
            Initialize();
            return MessageBox.Show(text);
        }

        public static DialogResult Show(string text, string caption)
        {
            Initialize();
            return MessageBox.Show(text, caption);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defButton);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defButton, options);
        }

        public static DialogResult Show(IWin32Window owner, string text)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon, defButton);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon, defButton, options);
        }

        public const int WH_CALLWNDPROCRET = 12;

        public enum CbtHookAction
        {
            HCBT_MOVESIZE = 0,
            HCBT_MINMAX = 1,
            HCBT_QS = 2,
            HCBT_CREATEWND = 3,
            HCBT_DESTROYWND = 4,
            HCBT_ACTIVATE = 5,
            HCBT_CLICKSKIPPED = 6,
            HCBT_KEYSKIPPED = 7,
            HCBT_SYSCOMMAND = 8,
            HCBT_SETFOCUS = 9
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct CwpRetStruct : IEquatable<CwpRetStruct>
        {
            public readonly IntPtr lResult;
            public readonly IntPtr lParam;
            public readonly IntPtr wParam;
            [CLSCompliant(false)]
            public readonly uint message;
            public readonly IntPtr hwnd;

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = lResult.GetHashCode();
                    hashCode = (hashCode * 397) ^ lParam.GetHashCode();
                    hashCode = (hashCode * 397) ^ wParam.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int) message;
                    hashCode = (hashCode * 397) ^ hwnd.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(CwpRetStruct left, CwpRetStruct right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(CwpRetStruct left, CwpRetStruct right)
            {
                return !(left == right);
            }

            public override bool Equals(object obj)
            {
                return obj != null && Equals((CwpRetStruct)obj);
            }

            /// <inheritdoc />
            public bool Equals(CwpRetStruct other)
            {
                return lResult.Equals(other.lResult) && lParam.Equals(other.lParam) && wParam.Equals(other.wParam)
                       && message == other.message && hwnd.Equals(other.hwnd);
            }
        }

        private static void Initialize()
        {
            if (_hHook != IntPtr.Zero)
            {
                throw new NotSupportedException("multiple calls are not supported");
            }

            if (_owner != null)
            {
                _hHook = NativeMethods.SetWindowsHookEx(WH_CALLWNDPROCRET, s_HookProc, IntPtr.Zero, Environment.CurrentManagedThreadId);
            }
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return NativeMethods.CallNextHookEx(_hHook, nCode, wParam, lParam);
            }

            CwpRetStruct msg = (CwpRetStruct)Marshal.PtrToStructure(lParam, typeof(CwpRetStruct));
            IntPtr hook = _hHook;

            if (msg.message == (int)CbtHookAction.HCBT_ACTIVATE)
            {
                try
                {
                    CenterWindow(msg.hwnd);
                }
                finally
                {
                    NativeMethods.UnhookWindowsHookEx(_hHook);
                    _hHook = IntPtr.Zero;
                }
            }

            return NativeMethods.CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static void CenterWindow(IntPtr hChildWnd)
        {
            Rectangle recChild = new Rectangle(0, 0, 0, 0);
            if (!NativeMethods.GetWindowRect(hChildWnd, ref recChild))
                return;

            int width = recChild.Width - recChild.X;
            int height = recChild.Height - recChild.Y;

            Rectangle recParent = new Rectangle(0, 0, 0, 0);
            if (!NativeMethods.GetWindowRect(_owner.Handle, ref recParent))
                return;

            Point ptCenter = new Point(
                recParent.X + (recParent.Width - recParent.X) / 2,
                recParent.Y + (recParent.Height - recParent.Y) / 2);

            Point ptStart = new Point(
                ptCenter.X - width / 2,
                ptCenter.Y - height / 2);

            ptStart.X = ptStart.X < 0 ? 0 : ptStart.X;
            ptStart.Y = ptStart.Y < 0 ? 0 : ptStart.Y;

            NativeMethods.MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
        }
    }
}
