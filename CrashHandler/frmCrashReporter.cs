using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CrashHandler
{
    public sealed partial class frmCrashReporter : Form
    {
        private delegate void ChangeDesc(CrashDumperProgress progress, string desc);

        private readonly CrashDumper _dumper;
        private readonly string _strDefaultUserStory;
        private static readonly MD5CryptoServiceProvider _md5provider = new MD5CryptoServiceProvider();

        public frmCrashReporter(CrashDumper dumper)
        {
            _dumper = dumper;
            InitializeComponent();
            lblDesc.Text = _dumper.Attributes["visible-error-friendly"];
            txtIdSelectable.Text = "Crash followup id = " + _dumper.Attributes["visible-crash-id"];
            _dumper.CrashDumperProgressChanged += DumperOnCrashDumperProgressChanged;
            _strDefaultUserStory = Md5Hash(txtUserStory.Text);
        }

        private void frmCrashReporter_Load(object sender, EventArgs e)
        {
            _dumper.StartCollecting();
        }

        private void DumperOnCrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
        {
            Invoke(new ChangeDesc(ChangeProgress), args.Progress, args.Progress.GetDescription());
        }

        private void ChangeProgress(CrashDumperProgress progress, string desc)
        {
            if (progress == CrashDumperProgress.FinishedSending)
            {
                Close();
            }

            statusCollectionProgess.Text = desc;
        }

        private void llblContents_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _dumper.DoCleanUp = false;
            Process.Start("explorer.exe", _dumper.WorkingDirectory);
        }

        private void txtDesc_TextChanged(object sender, EventArgs e)
        {
            timerRefreshTextFile.Stop();
            timerRefreshTextFile.Start();
            lblDescriptionWarning.Visible = txtUserStory.Text.Length == 0;
            btnSend.Enabled = Md5Hash(txtUserStory.Text) != _strDefaultUserStory;
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            //TODO: Convert to restart, collect previously loaded character files from application and relaunch?
            DialogResult = DialogResult.Cancel;
            _dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
            Environment.Exit(-1);
            // Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (timerRefreshTextFile.Enabled)
            {
                timerRefreshTextFile.Enabled = false;
                timerRefreshTextFile_Tick(null, null);
                fs.Close();
            }

            _dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
            _dumper.AllowSending();

            DialogResult = DialogResult.OK;
            Close();
        }

        private FileStream fs;

        private void timerRefreshTextFile_Tick(object sender, EventArgs e)
        {
            timerRefreshTextFile.Stop();

            if (fs == null)
            {
                fs = File.OpenWrite(Path.Combine(_dumper.WorkingDirectory, "userstory.txt"));
            }
            fs.Seek(0, SeekOrigin.Begin);
            byte[] bytes = Encoding.UTF8.GetBytes(string.Empty);
            if (Md5Hash(txtUserStory.Text) != _strDefaultUserStory)
            {
                bytes = Encoding.UTF8.GetBytes(txtUserStory.Text);
            }
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush(true);
        }

        private void frmCrashReporter_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
        }

        private void cmdSubmitIssue_Click(object sender, EventArgs e)
        {
            string strTitle = System.Net.WebUtility.HtmlEncode("Issue: - PLEASE ENTER DESCRIPTION HERE");
            strTitle = strTitle
                .Replace(" ", "%20")
                .Replace("#", "%23")
                .Replace("\r\n", "%0D%0A")
                .Replace("\r", "%0D%0A")
                .Replace("\n", "%0D%0A");

            StringBuilder sbdBody = new StringBuilder()
                .AppendLine("### Environment")
                .Append("Crash ID: ").AppendLine(_dumper.Attributes["visible-crash-id"])
                .Append("Chummer Version: ").AppendLine(_dumper.Attributes["visible-version"])
                .Append("Environment: ").AppendLine(_dumper.Attributes["os-name"])
                .Append("Runtime: ").AppendLine(Environment.Version.ToString())
                .Append("Option upload logs set: ").AppendLine(_dumper.Attributes["option-upload-logs-set"])
                .Append("Installation ID: ").AppendLine(_dumper.Attributes["installation-id"])
                .Append(txtUserStory.Text);
            string strBody = System.Net.WebUtility.HtmlEncode(sbdBody.ToString());
            strBody = strBody
                .Replace(" ", "%20")
                .Replace("#", "%23")
                .Replace("\r\n", "%0D%0A")
                .Replace("\r", "%0D%0A")
                .Replace("\n", "%0D%0A");

            string strSend = $"https://github.com/chummer5a/chummer5a/issues/new?labels=new&title={strTitle}&body={strBody}";

            Process.Start(strSend);
            btnSend_Click(sender, e);
        }

        private static string Md5Hash(string strInput)
        {
            StringBuilder sbdHash = new StringBuilder();
            byte[] achrBytes = _md5provider.ComputeHash(new UTF8Encoding().GetBytes(strInput));
            foreach (var chrByte in achrBytes)
            {
                sbdHash.Append(chrByte.ToString("x2"));
            }
            return sbdHash.ToString();
        }
    }
}
