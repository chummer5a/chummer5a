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
 using System.Runtime;
 using System.Runtime.InteropServices;
﻿using System.Threading;
﻿using System.Windows.Forms;
﻿using Chummer.Backend;

[assembly: CLSCompliant(true)]
namespace Chummer
{
    static class Program
    {
        private const string strChummerGuid = "eb0759c1-3599-495e-8bc5-57c8b3e1b31c";
        private static Mutex s_MutexGlobal;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (s_MutexGlobal = new Mutex(false, @"Global\" + strChummerGuid))
            {
                ProfileOptimization.SetProfileRoot(Application.StartupPath);
                ProfileOptimization.StartProfile("chummerprofile");
                Stopwatch sw = Stopwatch.StartNew();
                //If debuging and launched from other place (Bootstrap), launch debugger
                if (Environment.GetCommandLineArgs().Contains("/debug") && !Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
                sw.TaskEnd("dbgchk");
                //Various init stuff (that mostly "can" be removed as they serve 
                //debugging more than function


                //Needs to be called before Log is setup, as it moves where log might be.
                FixCwd();


                sw.TaskEnd("fixcwd");
                //Log exceptions that is caught. Wanting to know about this cause of performance
                AppDomain.CurrentDomain.FirstChanceException += Log.FirstChanceException;
                AppDomain.CurrentDomain.FirstChanceException += s_Heatmap.OnException;

                sw.TaskEnd("appdomain 2");

                string info =
                    $"Application Chummer5a build {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} started at {DateTime.UtcNow} with command line arguments {Environment.CommandLine}";
                sw.TaskEnd("infogen");

                Log.Info(info);
                sw.TaskEnd("infoprnt");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Make sure the default language has been loaded before attempting to open the Main Form.
                LanguageManager.TranslateWinForm(GlobalOptions.Language, null);
                sw.TaskEnd("languagefreestartup");
//#if !DEBUG
                AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                        CrashHandler.WebMiniDumpHandler(ex);

                    //main.Hide();
                    //main.ShowInTaskbar = false;
                };
//#endif

                sw.TaskEnd("Startup");

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                s_FrmMainForm = new frmChummerMain();
                Application.Run(s_FrmMainForm);

                Log.Info(s_Heatmap.GenerateInfo());
            }
        }

        private static frmChummerMain s_FrmMainForm;
        /// <summary>
        /// Main application form.
        /// </summary>
        public static frmChummerMain MainForm
        {
            get
            {
                return s_FrmMainForm;
            }
            set
            {
                s_FrmMainForm = value;
            }
        }

        static readonly ExceptionHeatMap s_Heatmap = new ExceptionHeatMap();

        static void FixCwd()
        {
            //If launched by file assiocation, the cwd is file location. 
            //Chummer looks for data in cwd, to be able to move exe (legacy+bootstraper uses this)

            if (Directory.Exists(Path.Combine(Application.StartupPath, "data"))
                && Directory.Exists(Path.Combine(Application.StartupPath, "lang")))
            {
                //both normally used data dirs present (add file loading abstraction to the list)
                //so do nothing

                return;
            }

            Environment.CurrentDirectory = Application.StartupPath;
        }

        public static Mutex GlobalChummerMutex
        {
            get { return s_MutexGlobal; }
        }
    }
}
