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
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Chummer;

[assembly: CLSCompliant(true)]

namespace CrashHandler
{
    internal static class Program
    {
        private static readonly Dictionary<string, Action<string[]>> s_DictionaryFunctions =
            new Dictionary<string, Action<string[]>>
            {
                {"crash", ShowCrashReport}
            };

        private static void ShowCrashReport(string[] args)
        {
            using (CrashDumper objDumper = new CrashDumper(args[0], args[1]))
            {
                Application.Run(new CrashReporter(objDumper));
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Chummer.Program.SetProcessDPI(GlobalSettings.DpiScalingMethodSetting);
            if (Chummer.Program.IsMainThread)
                Chummer.Program.SetThreadDPI(GlobalSettings.DpiScalingMethodSetting);

            Action<string[]> funcToRun = null;
            List<string> lstArgsForFunc = new List<string>(args.Length - 1);
            foreach (string strArg in args)
            {
                if (strArg.Equals("--debug", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Debugger.IsAttached)
                        Debugger.Launch();
                }
                else if (funcToRun == null)
                {
                    s_DictionaryFunctions.TryGetValue(strArg, out funcToRun);
                }
                else
                    lstArgsForFunc.Add(strArg.Trim('\"'));
            }

            funcToRun?.Invoke(lstArgsForFunc.ToArray());
        }
    }
}
