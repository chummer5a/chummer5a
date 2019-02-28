using System;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]
namespace Translator
{
    static class Program
    {
        private static frmTranslatorMain s_FrmMain;
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            s_FrmMain = new frmTranslatorMain();
            Application.Run(s_FrmMain);
        }

        public static frmTranslatorMain MainForm => s_FrmMain;
    }
}
