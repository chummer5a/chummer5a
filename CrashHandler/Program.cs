using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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

        internal static HttpClient UploadClient { get; } = new HttpClient();

        private static void ShowCrashReport(string[] args)
        {
            if (args.Contains("--debug") && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            CrashDumper dmper = null;
            try
            {
                dmper = new CrashDumper(args[0]);
                CrashReporter reporter = new CrashReporter(dmper);

                Application.Run(reporter);

                if (reporter.DialogResult == DialogResult.OK)
                {
                    Application.Run(new NoMoreUserInput(dmper));
                }
            }
            finally
            {
                //Last ditch attempt at closing chummer if not done yet
                try
                {
                    dmper?.Process?.Kill();
                }
                catch
                {
                    // ignored
                }
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
