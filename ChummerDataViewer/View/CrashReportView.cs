using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
    public sealed partial class CrashReportView : UserControl
    {
        private readonly CrashReport _objReport;
        private readonly DownloaderWorker _worker;

        internal CrashReportView(CrashReport objReport, DownloaderWorker worker)
        {
            _objReport = objReport;
            _worker = worker;
            InitializeComponent();

            lblBuildType.Text = objReport.BuildType;
            lblGuid.Text = objReport.Guid.ToString("D");
            lblVersion.Text = objReport.Version.ToString(3);
            lblDate.Text = objReport.Timestamp.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

            if (objReport.StackTrace != null)
            {
                lblExceptionGuess.Text = GuessStack(_objReport.StackTrace);
                lblExceptionGuess.Visible = true;
            }
            objReport.ProgressChanged += (s, e) => OnProgressChanged(_objReport.Progress);
            OnProgressChanged(_objReport.Progress);
        }

        //TODO: move this to a better place
        private static string GuessStack(string stacktrace)
        {
            string exception = stacktrace.Split(':')[0];

            string location = stacktrace.Split(new[] { " at " }, StringSplitOptions.None).Skip(1).FirstOrDefault(x => x.StartsWith("Chummer.", StringComparison.Ordinal));

            if (location != null) return exception + " : " + location;

            return exception;
        }

        private void OnProgressChanged(CrashReportProcessingProgress progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnProgressChanged(progress)));
                return;
            }

            switch (progress)
            {
                case CrashReportProcessingProgress.NotStarted:
                    btnAction.Text = "Download";
                    btnAction.Enabled = true;
                    break;

                case CrashReportProcessingProgress.Downloading:
                    btnAction.Text = "Downloading";
                    btnAction.Enabled = false;
                    break;

                case CrashReportProcessingProgress.Downloaded:
                    btnAction.Text = "Unpack";
                    btnAction.Enabled = true;
                    break;

                case CrashReportProcessingProgress.Unpacking:
                    btnAction.Text = "Unpacking";
                    btnAction.Enabled = false;
                    break;

                case CrashReportProcessingProgress.Unpacked:
                    btnAction.Text = "Open folder";
                    btnAction.Enabled = true;
                    break;
            }

            if (_objReport.StackTrace != null)
            {
                lblExceptionGuess.Text = GuessStack(_objReport.StackTrace);
                lblExceptionGuess.Visible = true;
            }
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            switch (_objReport.Progress)
            {
                case CrashReportProcessingProgress.NotStarted:
                    _objReport.StartDownload(_worker);
                    break;

                case CrashReportProcessingProgress.Downloaded:
                    break;

                case CrashReportProcessingProgress.Unpacked:
                    break;
                case CrashReportProcessingProgress.Downloading:
                    break;
                case CrashReportProcessingProgress.Unpacking:
                    break;
            }
        }
    }
}
