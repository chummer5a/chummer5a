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
using System.Runtime.InteropServices;
using System.Text;

namespace ChummerHub.Client.Backend
{
    internal static class NativeMethods
    {
        [DllImport("wininet.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        private static extern bool InternetGetCookieEx(string url, string cookieName, StringBuilder cookieData, ref int size, int dwFlags, IntPtr lpReserved);

        [DllImport("wininet.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        private static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        private const int InternetCookieHttpOnly = 0x2000;

        /// <summary>
        /// Gets the actual Cookie data
        /// </summary>
        internal static string GetUriCookieData(Uri uri)
        {
            if (uri == null)
                return string.Empty;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            using (new Chummer.FetchSafelyFromPool<StringBuilder>(Chummer.Utils.StringBuilderPool,
                                                          out StringBuilder cookieData))
            {
                cookieData.Capacity = datasize;
                if (InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttpOnly, IntPtr.Zero))
                    return cookieData.Replace(';', ',').ToString();
                if (datasize < 0)
                    return null;
            }
            // Allocate StringBuilder large enough to hold the cookie
            using (new Chummer.FetchSafelyFromPool<StringBuilder>(Chummer.Utils.StringBuilderPool,
                                                                  out StringBuilder cookieData))
            {
                cookieData.Capacity = datasize;
                return InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttpOnly,
                                           IntPtr.Zero)
                    ? cookieData.Replace(';', ',').ToString()
                    : null;
            }
        }


        /// <summary>
        /// Gets the actual Cookie data
        /// </summary>
        internal static bool DeleteUriCookieData(Uri uri)
        {
            if (uri == null)
                return true;
            //Cookie temp1 = new Cookie("KEY1", "VALUE1", "/Path/To/My/App", "/");
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            using (new Chummer.FetchSafelyFromPool<StringBuilder>(Chummer.Utils.StringBuilderPool, out StringBuilder cookieData))
            {
                cookieData.Capacity = datasize;
                if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttpOnly, IntPtr.Zero))
                {
                    if (datasize < 0)
                        return false;
                    // Allocate StringBuilder large enough to hold the cookie
                    cookieData.Clear();
                    cookieData.Capacity = datasize;
                    if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttpOnly, IntPtr.Zero))
                        return false;
                }
                if (InternetSetCookie(uri.ToString(), null, string.Empty))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
