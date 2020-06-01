using System;
using System.Linq;
using System.Windows.Forms;
using ChummerDataViewer.Model;
using System.Globalization;

namespace ChummerDataViewer
{
	public sealed partial class CrashReportView : UserControl
	{
		private readonly CrashReport _report;
		private readonly DownloaderWorker _worker;

		internal CrashReportView(CrashReport report, DownloaderWorker worker)
		{
			_report = report;
			_worker = worker;
			InitializeComponent();

			lblBuildType.Text = report.BuildType;
			lblGuid.Text = report.Guid.ToString("D");
			lblVersion.Text = report.Version.ToString(3);
			lblDate.Text = report.Timestamp.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

			if (report.StackTrace != null)
			{
				lblExceptionGuess.Text = GuessStack(_report.StackTrace);
				lblExceptionGuess.Visible = true;
			}
			report.ProgressChanged += (s, e) => OnProgressChanged(_report.Progress);
			OnProgressChanged(_report.Progress);
		}

		//TODO: move this to a better place
		private static string GuessStack(string stacktrace)
		{
			string exception = stacktrace.Split(':')[0];

			string location = stacktrace.Split(new [] {" at "}, StringSplitOptions.None).Skip(1).FirstOrDefault(x => x.StartsWith("Chummer.", StringComparison.Ordinal));

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
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (_report.StackTrace != null)
			{
				lblExceptionGuess.Text = GuessStack(_report.StackTrace);
				lblExceptionGuess.Visible = true;
			}

		}

		private void btnAction_Click(object sender, EventArgs e)
		{
			switch (_report.Progress)
			{
				case CrashReportProcessingProgress.NotStarted:
					_report.StartDownload(_worker);
					break;
				case CrashReportProcessingProgress.Downloaded:
					break;
				case CrashReportProcessingProgress.Unpacked:
					break;
			}
		}
	}
}
