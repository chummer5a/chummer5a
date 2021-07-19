using System.Windows.Forms;

namespace CrashHandler
{
    public sealed partial class frmNoMoreUserInput : Form
    {
        private delegate void ChangeDesc(CrashDumperProgress progress, string desc);

        public frmNoMoreUserInput(CrashDumper dmper)
        {
            InitializeComponent();

            if (dmper != null)
            {
                lblProgress.Text = dmper.Progress.GetDescription();

                dmper.CrashDumperProgressChanged += Dmper_CrashDumperProgressChanged;
            }
        }

        private void Dmper_CrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
        {
            Invoke(new ChangeDesc(ChangeProgress), args.Progress, args.Progress.GetDescription());
        }

        private void ChangeProgress(CrashDumperProgress progress, string desc)
        {
            if (progress == CrashDumperProgress.FinishedSending)
            {
                Close();
            }

            lblProgress.Text = desc;
        }
    }
}