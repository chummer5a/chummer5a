/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
