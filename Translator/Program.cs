using System;
using System.Threading;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]
namespace Translator
{
    internal static class Program
    {
        private static frmMain _frmMain = null;
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _frmMain = new frmMain();
            Application.Run(_frmMain);
        }

        public static frmMain MainForm
        {
            get
            {
                return _frmMain;
            }
        }
    }
}
