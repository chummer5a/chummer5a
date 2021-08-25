using System;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]

namespace Translator
{
    internal static class Program
    {
        private static frmTranslatorMain _frmMain;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _frmMain = new frmTranslatorMain();
            Application.Run(_frmMain);
        }

        public static frmTranslatorMain MainForm => _frmMain;
    }
}
