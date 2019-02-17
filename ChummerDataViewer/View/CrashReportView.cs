using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
	public partial class CrashReportView : UserControl
	{
		private readonly CrashReport _report;
		private readonly DownloaderWorker _worker;

		internal CrashReportView(CrashReport report, DownloaderWorker worker)
		{
			_report = report;
			_worker = worker;
			InitializeComponent();

			lblBuildType.Text = report.BuildType;
			lblGuid.Text = report.Guid.ToString();
			lblVersion.Text = report.Version.ToString(3);
			lblDate.Text = report.Timestamp.ToString("d MMM yy");

			if (report.StackTrace != null)
			{
				lblExceptionGuess.Text = GuessStack(_report.StackTrace);
				lblExceptionGuess.Visible = true;
			}
			report.ProgressChanged += (s, e) => OnProgressChanged(_report.Progress);
			OnProgressChanged(_report.Progress);
		}

		//TODO: move this to a better place
		private string GuessStack(string stacktrace)
		{
			string exception = stacktrace.Split(':')[0];

			string location = stacktrace.Split(new string[] {" at "}, StringSplitOptions.None).Skip(1).FirstOrDefault(x => x.StartsWith("Chummer."));

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
