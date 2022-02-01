using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CrashHandler
{
    public sealed partial class CrashReporter : Form
    {
        private delegate void ChangeDesc(CrashDumperProgress progress, string desc);

        private readonly CrashDumper _dumper;

        public CrashReporter(CrashDumper dumper)
        {
            _dumper = dumper ?? throw new ArgumentNullException(nameof(dumper));
            InitializeComponent();
            lblDesc.Text = _dumper.Attributes["visible-error-friendly"];
            txtIdSelectable.Text = "Crash followup Id = " + _dumper.Attributes["visible-crash-id"];
            txtIdSelectable2.Text = "Installation Id = " + _dumper.Attributes["installation-id"];
            _dumper.CrashDumperProgressChanged += DumperOnCrashDumperProgressChanged;
        }

        private void frmCrashReporter_Load(object sender, EventArgs e)
        {
            _dumper.StartCollecting();
        }

        private void frmCrashReporter_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
        }

        private void DumperOnCrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
        {
            Invoke(new ChangeDesc(ChangeProgress), args.Progress, args.Progress.GetDescription());
        }

        private void ChangeProgress(CrashDumperProgress progress, string desc)
        {
            statusCollectionProgess.Text = desc;
            if (progress == CrashDumperProgress.FinishedSending)
            {
                Close();
            }
        }

        private void lblContents_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _dumper.DoCleanUp = false;
            Process.Start("explorer.exe", _dumper.WorkingDirectory);
        }

        private void txtDesc_TextChanged(object sender, EventArgs e)
        {
            btnSend.Enabled = cmdSubmitIssue.Enabled = lblDescriptionWarning.Visible =
                txtUserStory.TextLength > 0 && !txtUserStory.Text.Contains("(Enter text here)");
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            //TODO: Convert to restart, collect previously loaded character files from application and relaunch?
            DialogResult = DialogResult.Cancel;
            _dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
            Environment.Exit(-1);
            // Close();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            using (FileStream fs = File.OpenWrite(Path.Combine(_dumper.WorkingDirectory, "userstory.txt")))
            {
                fs.Seek(0, SeekOrigin.Begin);
                byte[] bytes = Encoding.UTF8.GetBytes(txtUserStory.Text);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                fs.Flush(true);
            }

            _dumper.AllowSending();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdSubmitIssue_Click(object sender, EventArgs e)
        {
            string strTitle = HtmlWrap("Auto-Generated Issue from Crash: ID " + _dumper.Attributes["visible-crash-id"]);
            string strBody = HtmlWrap("### Environment"
                                      + Environment.NewLine + "Crash ID: " + _dumper.Attributes["visible-crash-id"]
                                      + Environment.NewLine + "Chummer Version: " +
                                      _dumper.Attributes["visible-version"]
                                      + Environment.NewLine + "Environment: " + _dumper.Attributes["os-name"]
                                      + Environment.NewLine + "Runtime: " + Environment.Version
                                      + Environment.NewLine + "Option upload logs set: " +
                                      _dumper.Attributes["option-upload-logs-set"]
                                      + Environment.NewLine + "Installation ID: " +
                                      _dumper.Attributes["installation-id"]
                                      + Environment.NewLine + Environment.NewLine + txtUserStory.Text
                                      + Environment.NewLine + Environment.NewLine + "### Crash Description"
                                      + Environment.NewLine + _dumper.Attributes["visible-error-friendly"]
                                      + Environment.NewLine + _dumper.Attributes["visible-stacktrace"]);
            string strSend =
                $"https://github.com/chummer5a/chummer5a/issues/new?labels=new&title={strTitle}&body={strBody}";

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
