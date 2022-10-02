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
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool MiniDumpWriteDump
        (
            IntPtr hProcess,
            uint ProcessId,
            SafeHandle hFile,
            MINIDUMP_TYPE DumpType,
            ref MiniDumpExceptionInformation ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallbackParam
        );

        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool MiniDumpWriteDump
        (
            IntPtr hProcess,
            uint ProcessId,
            SafeHandle hFile,
            MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallbackParam
        );

        [StructLayout(LayoutKind.Sequential, Pack = 4)]  // Pack=4 is important! So it works also for x64!
        internal struct MiniDumpExceptionInformation
        {
            internal readonly uint ThreadId;
            internal IntPtr ExceptionPointers;

            [MarshalAs(UnmanagedType.Bool)]
            internal readonly bool ClientPointers;
        }

        [Flags]
        internal enum MINIDUMP_TYPE
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000,
            MiniDumpWithoutAuxiliaryState = 0x00004000,
            MiniDumpWithFullAuxiliaryState = 0x00008000,
            MiniDumpWithPrivateWriteCopyMemory = 0x00010000,
            MiniDumpIgnoreInaccessibleMemory = 0x00020000,
            MiniDumpWithTokenInformation = 0x00040000,
            MiniDumpWithModuleHeaders = 0x00080000,
            MiniDumpFilterTriage = 0x00100000,
            MiniDumpValidTypeFlags = 0x001fffff
        }

        [DllImport("kernel32.dll", EntryPoint = "DebugActiveProcess", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool DebugActiveProcess(IntPtr hProcess);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", CharSet = CharSet.Auto, ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess", CharSet = CharSet.Auto, ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId", CharSet = CharSet.Auto, ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern uint GetCurrentProcessId();

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string name);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool SetDefaultPrinter(string strName);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool GetDefaultPrinter(StringBuilder sbdBuffer, ref int ptrBuffer);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr GetThreadDpiAwarenessContext();

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int GetAwarenessFromDpiAwarenessContext(IntPtr dpiAwarenessContext);

        [DllImport("SHCore.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool SetProcessDpiAwarenessContext(ContextDpiAwareness awareness);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool SetThreadDpiAwarenessContext(ContextDpiAwareness awareness);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, StringBuilder lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref IntPtr lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing;
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref CopyDataStruct lParam);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowMode flags);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
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
        /// <see cref="ChangeFilterStruct"/> structure.</param>
        /// <returns>If the function succeeds, it returns TRUE; otherwise, it returns FALSE.
        /// To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, ChangeWindowMessageFilterExAction action, ref ChangeFilterStruct changeInfo);

        internal enum ProcessDpiAwareness
        {
            Unaware = 0,
            System = 1,
            PerMonitor = 2
        }

        internal enum ContextDpiAwareness
        {
            Undefined = 0,
            Unaware = -1,
            System = -2,
            PerMonitor = -3,
            PerMonitorV2 = -4,
            UnawareGdiScaled = -5
        }

        internal enum ShowWindowMode
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
        }

        /// <summary>
        /// Contains data to be passed to another application by the WM_COPYDATA message.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct CopyDataStruct
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

        internal static CopyDataStruct CopyDataFromString(IntPtr intId, string strData)
        {
            return new CopyDataStruct
            {
                dwData = intId,
                cbData = strData.Length * 2 + 1,
                lpData = Marshal.StringToHGlobalUni(strData)
            };
        }

        /// <summary>
        /// Values used in the struct ChangeFilterStruct
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
        internal struct ChangeFilterStruct
        {
            /// <summary>
            /// The size of the structure, in bytes. Must be set to sizeof(ChangeFilterStruct),
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
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdBuffer))
                {
                    if (GetDefaultPrinter(sbdBuffer, ref ptrBuffer))
                    {
                        return sbdBuffer.ToString();
                    }
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
                ShowWindow(objProcess.Handle, ShowWindowMode.Restore);
            }
            // set user the focus to the window
            SetForegroundWindow(objProcess.MainWindowHandle);
        }

        internal enum SystemString
        {
            OK = 0,
            Cancel = 1,
            Abort = 2,
            Retry = 3,
            Ignore = 4,
            Yes = 5,
            No = 6,
            Close = 7,
            Help = 8,
            TryAgain = 9,
            Continue = 10
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern IntPtr MB_GetString(int strId);

        /// <summary>
        /// Get a system string that is localized to the user's currently-set Windows language.
        /// </summary>
        /// <param name="intSystemStringId">Id of the system string to use.</param>
        internal static string GetSystemString(int intSystemStringId)
        {
            if (intSystemStringId < 0 || intSystemStringId > 10)
                throw new ArgumentOutOfRangeException(nameof(intSystemStringId));
            return Marshal.PtrToStringAuto(MB_GetString(intSystemStringId));
        }

        /// <summary>
        /// Get a system string that is localized to the user's currently-set Windows language.
        /// </summary>
        /// <param name="eSystemStringId">Id of the system string to use.</param>
        internal static string GetSystemString(SystemString eSystemStringId)
        {
            return Marshal.PtrToStringAuto(MB_GetString((int)eSystemStringId));
        }

        internal enum SHSTOCKICONID : uint
        {
            /// <summary>Document of a type with no associated application.</summary>
            SIID_DOCNOASSOC = 0,

            /// <summary>Document of a type with an associated application.</summary>
            SIID_DOCASSOC = 1,

            /// <summary>Generic application with no custom icon.</summary>
            SIID_APPLICATION = 2,

            /// <summary>Folder (generic, unspecified state).</summary>
            SIID_FOLDER = 3,

            /// <summary>Folder (open).</summary>
            SIID_FOLDEROPEN = 4,

            /// <summary>5.25-inch disk drive.</summary>
            SIID_DRIVE525 = 5,

            /// <summary>3.5-inch disk drive.</summary>
            SIID_DRIVE35 = 6,

            /// <summary>Removable drive.</summary>
            SIID_DRIVEREMOVE = 7,

            /// <summary>Fixed drive (hard disk).</summary>
            SIID_DRIVEFIXED = 8,

            /// <summary>Network drive (connected).</summary>
            SIID_DRIVENET = 9,

            /// <summary>Network drive (disconnected).</summary>
            SIID_DRIVENETDISABLED = 10,

            /// <summary>CD drive.</summary>
            SIID_DRIVECD = 11,

            /// <summary>RAM disk drive.</summary>
            SIID_DRIVERAM = 12,

            /// <summary>The entire network.</summary>
            SIID_WORLD = 13,

            /// <summary>A computer on the network.</summary>
            SIID_SERVER = 15,

            /// <summary>A local printer or print destination.</summary>
            SIID_PRINTER = 16,

            /// <summary>The Network virtual folder (FOLDERID_NetworkFolder/CSIDL_NETWORK).</summary>
            SIID_MYNETWORK = 17,

            /// <summary>The Search feature.</summary>
            SIID_FIND = 22,

            /// <summary>The Help and Support feature.</summary>
            SIID_HELP = 23,

            /// <summary>Overlay for a shared item.</summary>
            SIID_SHARE = 28,

            /// <summary>Overlay for a shortcut.</summary>
            SIID_LINK = 29,

            /// <summary>Overlay for items that are expected to be slow to access.</summary>
            SIID_SLOWFILE = 30,

            /// <summary>The Recycle Bin (empty).</summary>
            SIID_RECYCLER = 31,

            /// <summary>The Recycle Bin (not empty).</summary>
            SIID_RECYCLERFULL = 32,

            /// <summary>Audio CD media.</summary>
            SIID_MEDIACDAUDIO = 40,

            /// <summary>Security lock.</summary>
            SIID_LOCK = 47,

            /// <summary>A virtual folder that contains the results of a search.</summary>
            SIID_AUTOLIST = 49,

            /// <summary>A network printer.</summary>
            SIID_PRINTERNET = 50,

            /// <summary>A server shared on a network.</summary>
            SIID_SERVERSHARE = 51,

            /// <summary>A local fax printer.</summary>
            SIID_PRINTERFAX = 52,

            /// <summary>A network fax printer.</summary>
            SIID_PRINTERFAXNET = 53,

            /// <summary>A file that receives the output of a Print to file operation.</summary>
            SIID_PRINTERFILE = 54,

            /// <summary>A category that results from a Stack by command to organize the contents of a folder.</summary>
            SIID_STACK = 55,

            /// <summary>Super Video CD (SVCD) media.</summary>
            SIID_MEDIASVCD = 56,

            /// <summary>A folder that contains only subfolders as child items.</summary>
            SIID_STUFFEDFOLDER = 57,

            /// <summary>Unknown drive type.</summary>
            SIID_DRIVEUNKNOWN = 58,

            /// <summary>DVD drive.</summary>
            SIID_DRIVEDVD = 59,

            /// <summary>DVD media.</summary>
            SIID_MEDIADVD = 60,

            /// <summary>DVD-RAM media.</summary>
            SIID_MEDIADVDRAM = 61,

            /// <summary>DVD-RW media.</summary>
            SIID_MEDIADVDRW = 62,

            /// <summary>DVD-R media.</summary>
            SIID_MEDIADVDR = 63,

            /// <summary>DVD-ROM media.</summary>
            SIID_MEDIADVDROM = 64,

            /// <summary>CD+ (enhanced audio CD) media.</summary>
            SIID_MEDIACDAUDIOPLUS = 65,

            /// <summary>CD-RW media.</summary>
            SIID_MEDIACDRW = 66,

            /// <summary>CD-R media.</summary>
            SIID_MEDIACDR = 67,

            /// <summary>A writable CD in the process of being burned.</summary>
            SIID_MEDIACDBURN = 68,

            /// <summary>Blank writable CD media.</summary>
            SIID_MEDIABLANKCD = 69,

            /// <summary>CD-ROM media.</summary>
            SIID_MEDIACDROM = 70,

            /// <summary>An audio file.</summary>
            SIID_AUDIOFILES = 71,

            /// <summary>An image file.</summary>
            SIID_IMAGEFILES = 72,

            /// <summary>A video file.</summary>
            SIID_VIDEOFILES = 73,

            /// <summary>A mixed file.</summary>
            SIID_MIXEDFILES = 74,

            /// <summary>Folder back.</summary>
            SIID_FOLDERBACK = 75,

            /// <summary>Folder front.</summary>
            SIID_FOLDERFRONT = 76,

            /// <summary>Security shield. Use for UAC prompts only.</summary>
            SIID_SHIELD = 77,

            /// <summary>Warning.</summary>
            SIID_WARNING = 78,

            /// <summary>Informational.</summary>
            SIID_INFO = 79,

            /// <summary>Error.</summary>
            SIID_ERROR = 80,

            /// <summary>Key.</summary>
            SIID_KEY = 81,

            /// <summary>Software.</summary>
            SIID_SOFTWARE = 82,

            /// <summary>A UI item, such as a button, that issues a rename command.</summary>
            SIID_RENAME = 83,

            /// <summary>A UI item, such as a button, that issues a delete command.</summary>
            SIID_DELETE = 84,

            /// <summary>Audio DVD media.</summary>
            SIID_MEDIAAUDIODVD = 85,

            /// <summary>Movie DVD media.</summary>
            SIID_MEDIAMOVIEDVD = 86,

            /// <summary>Enhanced CD media.</summary>
            SIID_MEDIAENHANCEDCD = 87,

            /// <summary>Enhanced DVD media.</summary>
            SIID_MEDIAENHANCEDDVD = 88,

            /// <summary>High definition DVD media in the HD DVD format.</summary>
            SIID_MEDIAHDDVD = 89,

            /// <summary>High definition DVD media in the Blu-ray Disc™ format.</summary>
            SIID_MEDIABLURAY = 90,

            /// <summary>Video CD (VCD) media.</summary>
            SIID_MEDIAVCD = 91,

            /// <summary>DVD+R media.</summary>
            SIID_MEDIADVDPLUSR = 92,

            /// <summary>DVD+RW media.</summary>
            SIID_MEDIADVDPLUSRW = 93,

            /// <summary>A desktop computer.</summary>
            SIID_DESKTOPPC = 94,

            /// <summary>A mobile computer (laptop).</summary>
            SIID_MOBILEPC = 95,

            /// <summary>The User Accounts Control Panel item.</summary>
            SIID_USERS = 96,

            /// <summary>Smart media.</summary>
            SIID_MEDIASMARTMEDIA = 97,

            /// <summary>CompactFlash media.</summary>
            SIID_MEDIACOMPACTFLASH = 98,

            /// <summary>A cell phone.</summary>
            SIID_DEVICECELLPHONE = 99,

            /// <summary>A digital camera.</summary>
            SIID_DEVICECAMERA = 100,

            /// <summary>A digital video camera.</summary>
            SIID_DEVICEVIDEOCAMERA = 101,

            /// <summary>An audio player.</summary>
            SIID_DEVICEAUDIOPLAYER = 102,

            /// <summary>Connect to network.</summary>
            SIID_NETWORKCONNECT = 103,

            /// <summary>The Network and Internet Control Panel item.</summary>
            SIID_INTERNET = 104,

            /// <summary>A compressed file with a .zip file name extension.</summary>
            SIID_ZIPFILE = 105,

            /// <summary>The Additional Options Control Panel item.</summary>
            SIID_SETTINGS = 106,

            /// <summary>High definition DVD drive (any type - HD DVD-ROM, HD DVD-R, HD-DVD-RAM) that uses the HD DVD format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_DRIVEHDDVD = 132,

            /// <summary>High definition DVD drive (any type - BD-ROM, BD-R, BD-RE) that uses the Blu-ray Disc format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_DRIVEBD = 133,

            /// <summary>High definition DVD-ROM media in the HD DVD-ROM format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_MEDIAHDDVDROM = 134,

            /// <summary>High definition DVD-R media in the HD DVD-R format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_MEDIAHDDVDR = 135,

            /// <summary>High definition DVD-RAM media in the HD DVD-RAM format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_MEDIAHDDVDRAM = 136,

            /// <summary>High definition DVD-ROM media in the Blu-ray Disc BD-ROM format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_MEDIABDROM = 137,

            /// <summary>High definition write-once media in the Blu-ray Disc BD-R format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_MEDIABDR = 138,

            /// <summary>High definition read/write media in the Blu-ray Disc BD-RE format.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_MEDIABDRE = 139,

            /// <summary>A cluster disk array.</summary>
            /// <remarks>Windows Vista with SP1 and later.</remarks>
            SIID_CLUSTEREDDRIVE = 140,

            /// <summary>The highest valid value in the enumeration.</summary>
            /// <remarks>Values over 160 are Windows 7-only icons.</remarks>
            SIID_MAX_ICONS = 175
        }

        [Flags]
        internal enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SHSTOCKICONINFO
        {
            internal uint cbSize;
            internal IntPtr hIcon;
            internal readonly int iSysIconIndex;
            internal readonly int iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            internal readonly string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        internal static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        /// <summary>
        /// Gets a Windows stock icon. Useful as an alternative to the SystemIcons class.
        /// </summary>
        /// <param name="eIconId">Id to indicate which stock icon to fetch.</param>
        internal static Icon GetStockIcon(SHSTOCKICONID eIconId)
        {
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO))
            };
            Marshal.ThrowExceptionForHR(SHGetStockIconInfo(eIconId, SHGSI.SHGSI_ICON, ref sii));
            return Icon.FromHandle(sii.hIcon);
        }
    }
}
