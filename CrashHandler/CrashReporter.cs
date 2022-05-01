using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CrashHandler
{
    public sealed partial class CrashReporter : Form
    {
        private delegate void ChangeDesc(CrashDumperProgress progress, string desc);

        private readonly CrashDumper _objDumper;

        public CrashReporter(CrashDumper objDumper)
        {
            _objDumper = objDumper ?? throw new ArgumentNullException(nameof(objDumper));
            InitializeComponent();
            lblDesc.Text = _objDumper.Attributes["visible-error-friendly"];
            txtIdSelectable.Text = "Crash followup Id = " + _objDumper.Attributes["visible-crash-id"];
            txtIdSelectable2.Text = "Installation Id = " + _objDumper.Attributes["installation-id"];
            _objDumper.CrashDumperProgressChanged += DumperOnCrashDumperProgressChanged;
        }

        private void frmCrashReporter_Load(object sender, EventArgs e)
        {
            _objDumper.StartCollecting();
        }

        private void frmCrashReporter_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objDumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
        }

        private void DumperOnCrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
        {
            Invoke(new ChangeDesc(ChangeProgress), args.Progress, args.Progress.GetDescription());
        }

        private void ChangeProgress(CrashDumperProgress progress, string desc)
        {
            tslStatusCollectionProgess.Text = desc;
            if (progress == CrashDumperProgress.Finished)
                lblContents.Enabled = true;
        }

        private void lblContents_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", Chummer.Utils.GetStartupPath);
        }

        private void rtbUserStory_TextChanged(object sender, EventArgs e)
        {
            cmdSubmitIssue.Enabled = lblDescriptionWarning.Visible
                = rtbUserStory.TextLength > 0 && !rtbUserStory.Text.Contains("(Enter text here)");
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Application.Exit();
        }

        private async void cmdRestart_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Application.Exit();
            await Chummer.Utils.RestartApplication();
        }

        private void cmdSubmitIssue_Click(object sender, EventArgs e)
        {
            string strTitle = HtmlWrap("Auto-Generated Issue from Crash: ID " + _objDumper.Attributes["visible-crash-id"]);
            string strBody = HtmlWrap("### Environment"
                                      + Environment.NewLine + "Crash ID: " + _objDumper.Attributes["visible-crash-id"]
                                      + Environment.NewLine + "Chummer Version: " +
                                      _objDumper.Attributes["visible-version"]
                                      + Environment.NewLine + "Environment: " + _objDumper.Attributes["os-name"]
                                      + Environment.NewLine + "Runtime: " + Environment.Version
                                      + Environment.NewLine + "Option upload logs set: " +
                                      _objDumper.Attributes["option-upload-logs-set"]
                                      + Environment.NewLine + "Installation ID: " +
                                      _objDumper.Attributes["installation-id"]
                                      + Environment.NewLine + Environment.NewLine + rtbUserStory.Text
                                      + Environment.NewLine + Environment.NewLine + "### Crash Description"
                                      + Environment.NewLine + _objDumper.Attributes["visible-error-friendly"]
                                      + Environment.NewLine + _objDumper.Attributes["visible-stacktrace"]);
            string strSend = "https://github.com/chummer5a/chummer5a/issues/new?labels=new&title=" + strTitle + "&body="
                             + strBody;

            string HtmlWrap(string strInput)
            {
                return System.Net.WebUtility.HtmlEncode(strInput)
                    .Replace(" ", "%20")
                    .Replace("#", "%23")
                    .Replace("\r\n", "%0D%0A")
                    .Replace("\r", "%0D%0A")
                    .Replace("\n", "%0D%0A");
            }

            Process.Start(strSend);
        }
    }
}
