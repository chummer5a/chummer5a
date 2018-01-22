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
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    static class Log
    {
        private static readonly StreamWriter s_LogWriter;
        private static readonly StringBuilder s_TimeStamper;  //This will break in case of multithreading
        private static bool s_BlnLogEnabled = false;
        static Log()
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (GlobalOptions.UseLogging)
            {
                //TODO: Add listner to UseLogging to be able to start it mid run
                string strFile = Path.Combine(Application.StartupPath, "chummerlog.txt");
                s_LogWriter = new StreamWriter(strFile);
                s_TimeStamper = new StringBuilder();
                s_BlnLogEnabled = true;
            }
            sw.TaskEnd("log open");
        }

        /// <summary>
        /// This will disabled logging and free any resources used by it
        /// </summary>
        public static void Kill()
        {
            s_LogWriter.Flush();
            s_LogWriter.Close();
            s_BlnLogEnabled = false;
        }

        /// <summary>
        /// Log that the execution path is entering a method
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Enter
        (
            object[] info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
        )
        {
            writeLog(info, file, method, line, "Entering ");
        }

        /// <summary>
        /// Log that the execution path is entering a method
        /// </summary>
        /// <param name="info">An optional object providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Enter
        (
            string info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
        )
        {
            writeLog(new object[] {info}, file, method, line, "Entering ");
        }

        /// <summary>
        /// Log that the execution path is entering a method
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Exit
        (
            string info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
        )
        {
            writeLog(new object[]{info},file, method, line, "Exiting   ");
        }

        /// <summary>
        /// Log that an error occoured
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Error
            (
            object[] info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
            )
        {
            writeLog(info,file, method, line, "Error     ");
        }

        /// <summary>
        /// Log that an error occoured
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Error
            (
            object info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
            )
        {
            writeLog(new object[]{info},file, method, line, "Error     ");
        }

        /// <summary>
        /// Log an exception has occoured
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Exception
        (
            Exception exception
        )
        {
            if(!s_BlnLogEnabled)
                return;

            writeLog(
                new object[]{exception, exception.StackTrace},
                exception.Source, 
                exception.TargetSite.Name, 
                (new StackTrace(exception, true)).GetFrame(0).GetFileLineNumber(), 
                "Exception ");
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Warning
            (
            object[] info= null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
            )
        {
            writeLog(info, file, method, line, "Warning   ");
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        /// <param name="info">An optional object providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Warning
            (
            object info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
            )
        {
            writeLog(new object[]{info},file, method, line, "Warning   ");
        }

        /// <summary>
        /// Log some info
        /// </summary>
        /// <param name="info">An optional object providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Info
            (
            object[] info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
            )
        {
            writeLog(info,file, method, line, "Info      ");
        }

        /// <summary>
        /// Log some info
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        public static void Info
            (
            String info = null,
#if LEGACY
            string file = "LEGACY",
            string method = "LEGACY",
            int line = 0
#else
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
#endif
            )
        {
            writeLog(new object[]{info},file, method, line, "Info      ");
        }

        private static void writeLog(object[] info, string file, string method, int line, string pre)
        {
            if (!s_BlnLogEnabled)
                return;

            Stopwatch sw = Stopwatch.StartNew();
            //TODO: Add timestamp to logs

            s_TimeStamper.Clear();
            s_TimeStamper.Append(pre);
            string[] classPath = file.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            s_TimeStamper.Append(classPath[classPath.Length - 1]);
            s_TimeStamper.Append('.');
            s_TimeStamper.Append(method);
            s_TimeStamper.Append(':');
            s_TimeStamper.Append(line);

            if (info != null)
            {
                s_TimeStamper.Append(' ');
                foreach (object o in info)
                {
                    s_TimeStamper.Append(o);
                    s_TimeStamper.Append(", ");
                }

                s_TimeStamper.Length -= 2;
            }

            sw.TaskEnd("makeentry");

            s_LogWriter.WriteLine(s_TimeStamper.ToString());
            sw.TaskEnd("filewrite");
            Trace.WriteLine(s_TimeStamper.ToString());
            sw.TaskEnd("screenwrite");
        }

        public static void FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (s_BlnLogEnabled)
            {
                s_LogWriter?.WriteLine("First chance exception: " + e?.Exception);
            }
        }
    }
}
