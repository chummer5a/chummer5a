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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Chummer
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern uint GetCurrentThreadId();

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string name);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetDefaultPrinter(string strName);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetDefaultPrinter(StringBuilder sbdBuffer, ref int ptrBuffer);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetThreadDpiAwarenessContext();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetAwarenessFromDpiAwarenessContext(IntPtr DPI_AWARENESS_CONTEXT);

        [DllImport("SHCore.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwareness(ProcessDPIAwareness awareness);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwarenessContext(ContextDPIAwareness awareness);

        [DllImport("user32.dll")]
        internal static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll")]
        internal static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        internal static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        internal static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        internal static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

        /// <summary>
        /// The SendMessage function sends a message to a window or windows.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        /// <summary>
        /// ReleaseCapture releases a mouse capture
        /// </summary>
        /// <returns></returns>
        [DllImportAttribute("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern bool ReleaseCapture();

        internal enum ProcessDPIAwareness
        {
            Unaware = 0,
            System = 1,
            PerMonitor = 2
        }

        internal enum ContextDPIAwareness
        {
            Undefined = 0,
            Unaware = -1,
            System = -2,
            PerMonitor = -3,
            PerMonitorV2 = -4,
            UnawareGdiScaled = -5
        }

        internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        internal delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIDEvent, uint dwTime);

        internal static string GetDefaultPrinter()
        {

            int ptrBuffer = 0;
            if (GetDefaultPrinter(null, ref ptrBuffer))
            {
                return null;
            }
            int intLastWin32Error = Marshal.GetLastWin32Error();
            if (intLastWin32Error == 122) // ERROR_INSUFFICIENT_BUFFER
            {
                StringBuilder sbdBuffer = new StringBuilder(ptrBuffer);
                if (GetDefaultPrinter(sbdBuffer, ref ptrBuffer))
                {
                    return sbdBuffer.ToString();
                }
                intLastWin32Error = Marshal.GetLastWin32Error();
            }
            if (intLastWin32Error == 2) // ERROR_FILE_NOT_FOUND
            {
                return null;
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
