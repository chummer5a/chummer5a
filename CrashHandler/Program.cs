using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

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
            if (args.Contains("--debug") && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            using (CrashDumper objDumper = new CrashDumper(args[0]))
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
            for (int i = 0; i < args.Length - 1; ++i)
            {
                if (s_DictionaryFunctions.TryGetValue(args[i], out Action<string[]> actCachedAction))
                {
                    actCachedAction(args.Skip(i + 1).ToArray());
                    break;
                }
            }

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new frmCrashReporter());
        }
    }
}
