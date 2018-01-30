using System;
using System.Threading;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]
namespace Translator
{
    static class Program
    {
        private static frmTranslatorMain s_FrmMain = null;
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            s_FrmMain = new frmTranslatorMain();
            Application.Run(s_FrmMain);
        }

        public static frmTranslatorMain MainForm
        {
            get
            {
                return s_FrmMain;
            }
        }
    }
}
