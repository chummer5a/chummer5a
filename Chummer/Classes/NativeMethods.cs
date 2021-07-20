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
using System.Diagnostics;
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

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        internal static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32")]
        internal static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern bool ReleaseCapture();

        /// <summary>
        /// Modifies the User Interface Privilege Isolation (UIPI) message filter for a specified window
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window whose UIPI message filter is to be modified.</param>
        /// <param name="msg">The message that the message filter allows through or blocks.</param>
        /// <param name="action">The action to be performed, and can take one of the following values
        /// <see cref="MessageFilterInfo"/></param>
        /// <param name="changeInfo">Optional pointer to a
        /// <see cref="CHANGEFILTERSTRUCT"/> structure.</param>
        /// <returns>If the function succeeds, it returns TRUE; otherwise, it returns FALSE.
        /// To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, ChangeWindowMessageFilterExAction action, ref CHANGEFILTERSTRUCT changeInfo);

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

        internal enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11
        };

        /// <summary>
        /// Contains data to be passed to another application by the WM_COPYDATA message.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct COPYDATASTRUCT
        {
            /// <summary>
            /// User defined data to be passed to the receiving application.
            /// </summary>
            internal IntPtr dwData;

            /// <summary>
            /// The size, in bytes, of the data pointed to by the lpData member.
            /// </summary>
            internal int cbData;

            /// <summary>
            /// The data to be passed to the receiving application. This member can be IntPtr.Zero.
            /// </summary>
            internal IntPtr lpData;
        }

        internal static COPYDATASTRUCT CopyDataFromString(IntPtr intId, string strData)
        {
            return new COPYDATASTRUCT
            {
                dwData = intId,
                cbData = strData.Length * 2 + 1,
                lpData = Marshal.StringToHGlobalUni(strData)
            };
        }

        /// <summary>
        /// Values used in the struct CHANGEFILTERSTRUCT
        /// </summary>
        internal enum MessageFilterInfo : uint
        {
            /// <summary>
            /// Certain messages whose value is smaller than WM_USER are required to pass
            /// through the filter, regardless of the filter setting.
            /// There will be no effect when you attempt to use this function to
            /// allow or block such messages.
            /// </summary>
            None = 0,

            /// <summary>
            /// The message has already been allowed by this window's message filter,
            /// and the function thus succeeded with no change to the window's message filter.
            /// Applies to MSGFLT_ALLOW.
            /// </summary>
            AlreadyAllowed = 1,

            /// <summary>
            /// The message has already been blocked by this window's message filter,
            /// and the function thus succeeded with no change to the window's message filter.
            /// Applies to MSGFLT_DISALLOW.
            /// </summary>
            AlreadyDisAllowed = 2,

            /// <summary>
            /// The message is allowed at a scope higher than the window.
            /// Applies to MSGFLT_DISALLOW.
            /// </summary>
            AllowedHigher = 3
        }

        /// <summary>
        /// Values used by ChangeWindowMessageFilterEx
        /// </summary>
        internal enum ChangeWindowMessageFilterExAction : uint
        {
            /// <summary>
            /// Resets the window message filter for hWnd to the default.
            /// Any message allowed globally or process-wide will get through,
            /// but any message not included in those two categories,
            /// and which comes from a lower privileged process, will be blocked.
            /// </summary>
            Reset = 0,

            /// <summary>
            /// Allows the message through the filter.
            /// This enables the message to be received by hWnd,
            /// regardless of the source of the message,
            /// even it comes from a lower privileged process.
            /// </summary>
            Allow = 1,

            /// <summary>
            /// Blocks the message to be delivered to hWnd if it comes from
            /// a lower privileged process, unless the message is allowed process-wide
            /// by using the ChangeWindowMessageFilter function or globally.
            /// </summary>
            DisAllow = 2
        }

        /// <summary>
        /// Contains extended result information obtained by calling
        /// the ChangeWindowMessageFilterEx function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct CHANGEFILTERSTRUCT
        {
            /// <summary>
            /// The size of the structure, in bytes. Must be set to sizeof(CHANGEFILTERSTRUCT),
            /// otherwise the function fails with ERROR_INVALID_PARAMETER.
            /// </summary>
            internal uint size;

            /// <summary>
            /// If the function succeeds, this field contains one of the following values,
            /// <see cref="MessageFilterInfo"/>
            /// </summary>
            internal MessageFilterInfo info;
        }

        /// <summary>
        /// Handle used to send the message to all windows
        /// </summary>
        internal const int HWND_BROADCAST = 0xffff;

        internal const int WM_SETTEXT = 0X000C;

        /// <summary>
        /// An application sends the WM_COPYDATA message to pass data to another application.
        /// </summary>
        internal const int WM_COPYDATA = 0x004A;

        /// <summary>
        /// Message type that tells an instance of Chummer to show itself (and pop into the foreground)
        /// </summary>
        internal static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

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

        internal static void ShowProcessWindow(Process objProcess)
        {
            if (objProcess == null)
                throw new ArgumentNullException(nameof(objProcess));
            if (objProcess.MainWindowHandle == IntPtr.Zero)
            {
                // the window is hidden so try to restore it before setting focus.
                ShowWindow(objProcess.Handle, ShowWindowEnum.Restore);
            }
            // set user the focus to the window
            SetForegroundWindow(objProcess.MainWindowHandle);
        }
    }
}
