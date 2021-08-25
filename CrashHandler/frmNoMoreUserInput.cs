using System.Windows.Forms;

namespace CrashHandler
{
    public sealed partial class frmNoMoreUserInput : Form
    {
        private delegate void ChangeDesc(CrashDumperProgress progress, string desc);

        private readonly CrashDumper _objCrashDumper;

        public frmNoMoreUserInput(CrashDumper objCrashDumper)
        {
            _objCrashDumper = objCrashDumper;
            InitializeComponent();

            if (_objCrashDumper != null)
            {
                lblProgress.Text = _objCrashDumper.Progress.GetDescription();

                _objCrashDumper.CrashDumperProgressChanged += CrashDumperProgressChanged;
            }
        }

        private void frmNoMoreUserInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_objCrashDumper != null)
                _objCrashDumper.CrashDumperProgressChanged -= CrashDumperProgressChanged;
        }

        private void CrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
        {
            Invoke(new ChangeDesc(ChangeProgress), args.Progress, args.Progress.GetDescription());
        }

        private void ChangeProgress(CrashDumperProgress progress, string desc)
        {
            lblProgress.Text = desc;
            if (progress == CrashDumperProgress.FinishedSending)
            {
                Close();
            }
        }
    }
}
