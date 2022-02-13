using System;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]

namespace Translator
{
    internal static class Program
    {
        private static TranslatorMain _frmMain;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _frmMain = new TranslatorMain();
            Application.Run(_frmMain);
        }

        public static TranslatorMain MainForm => _frmMain;
    }
}
