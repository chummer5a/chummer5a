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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chummer;
using NLog;

namespace CrashHandler
{
    public sealed partial class CrashReporter : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private delegate void ChangeDesc(CrashDumperProgress progress, string desc);

        private readonly CrashDumper _objDumper;

        public CrashReporter(CrashDumper objDumper)
        {
            _objDumper = objDumper ?? throw new ArgumentNullException(nameof(objDumper));
            InitializeComponent();
            if (_objDumper.Attributes.TryGetValue("visible-error-friendly", out string strDesc))
                lblDesc.Text = strDesc;
            else
                lblDesc.Text = "Error deserializing friendly crash description.";
            if (_objDumper.Attributes.TryGetValue("visible-crash-id", out string strCrashId))
                txtIdSelectable.Text = "Crash followup Id = " + strCrashId;
            else
                txtIdSelectable.Text = "Error deserializing crash identifier.";
            if (_objDumper.Attributes.TryGetValue("installation-id", out string strInstallationId))
                txtIdSelectable2.Text = "Installation Id = " + strInstallationId;
            else
                txtIdSelectable2.Text = "Error deserializing installation identifier.";
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
            Process.Start("explorer.exe", Utils.GetStartupPath);
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

        private void cmdRestart_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Application.UseWaitCursor = true;
            string strArguments = string.Empty;
            // Get the parameters/arguments passed to program if any
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdArguments))
            {
                try
                {
                    ThreadSafeObservableCollection<CharacterShared> lstForms = Chummer.Program.MainForm
                        .OpenCharacterEditorForms;
                    if (lstForms != null)
                    {
                        foreach (CharacterShared objOpenCharacterForm in lstForms)
                        {
                            try
                            {
                                string strLoopFileName = objOpenCharacterForm.CharacterObject?.FileName ?? string.Empty;
                                if (!string.IsNullOrEmpty(strLoopFileName))
                                    sbdArguments.Append('\"').Append(strLoopFileName).Append("\" ");
                            }
                            catch (Exception ex)
                            {
                                // Swallow any exceptions
                                Log.Info(ex);
                                Utils.BreakIfDebug();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Swallow any exceptions
                    Log.Info(ex);
                    Utils.BreakIfDebug();
                }

                if (sbdArguments.Length > 0)
                {
                    --sbdArguments.Length;
                    strArguments = sbdArguments.ToString();
                }
            }
            ProcessStartInfo objStartInfo = new ProcessStartInfo
            {
                FileName = Utils.GetStartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName,
                Arguments = strArguments
            };
            try
            {
                Application.Exit();
            }
            finally
            {
                objStartInfo.Start();
            }
        }

        private void cmdSubmitIssue_Click(object sender, EventArgs e)
        {
            if (!_objDumper.Attributes.TryGetValue("visible-crash-id", out string strCrashId))
                strCrashId = "Error encountered while deserializing";
            if (!_objDumper.Attributes.TryGetValue("visible-version", out string strVersion))
                strVersion = "Error encountered while deserializing";
            if (!_objDumper.Attributes.TryGetValue("os-name", out string strOsName))
                strOsName = "Error encountered while deserializing";
            if (!_objDumper.Attributes.TryGetValue("option-upload-logs-set", out string strOptionUploadLogsSet))
                strOptionUploadLogsSet = "Error encountered while deserializing";
            if (!_objDumper.Attributes.TryGetValue("installation-id", out string strInstallationId))
                strInstallationId = "Error encountered while deserializing";
            if (!_objDumper.Attributes.TryGetValue("visible-error-friendly", out string strErrorFriendly))
                strErrorFriendly = "Error encountered while deserializing crash description.";
            if (!_objDumper.Attributes.TryGetValue("visible-stacktrace", out string strStacktrace))
                strStacktrace = "Error encountered while deserializing stacktrace.";
            string strTitle = HtmlWrap("Auto-Generated Issue from Crash: ID " + strCrashId);
            string strBody = HtmlWrap("### Environment"
                                      + Environment.NewLine + "Crash ID: " + strCrashId
                                      + Environment.NewLine + "Chummer Version: " + strVersion
                                      + Environment.NewLine + "Environment: " + strOsName
                                      + Environment.NewLine + "Runtime: " + Environment.Version
                                      + Environment.NewLine + "Option upload logs set: " + strOptionUploadLogsSet
                                      + Environment.NewLine + "Installation ID: " + strInstallationId
                                      + Environment.NewLine + Environment.NewLine + rtbUserStory.Text
                                      + Environment.NewLine + Environment.NewLine + "### Crash Description"
                                      + Environment.NewLine + strErrorFriendly
                                      + Environment.NewLine + strStacktrace);
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

            Process.Start(new ProcessStartInfo(strSend) { UseShellExecute = true });
        }
    }
}
