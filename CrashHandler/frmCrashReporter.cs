using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrashHandler
{
	public partial class frmCrashReporter : Form
	{

		delegate void ChangeDesc(CrashDumperProgress progress, string desc);
		private readonly CrashDumper _dumper;
		private string _strDefaultUserStory = "";

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
		}

		private void btnNo_Click(object sender, EventArgs e)
		{
			//TODO: Convert to restart, collect previously loaded character files from application and relaunch?
			DialogResult = DialogResult.Cancel;
			_dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
		    Environment.Exit(-1);
			Close();
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			if (timerRefreshTextFile.Enabled)
			{
				timerRefreshTextFile.Enabled = false;
				timerRefreshTextFile_Tick(null, null);
				fs.Close();
			}
			DialogResult = DialogResult.OK;
			_dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
			_dumper.AllowSending();
			
			Close();
		}

		private FileStream fs = null;
		private void timerRefreshTextFile_Tick(object sender, EventArgs e)
		{
			timerRefreshTextFile.Stop();

			if (fs == null)
			{
				fs = File.OpenWrite(Path.Combine(_dumper.WorkingDirectory, "userstory.txt"));
			}
			fs.Seek(0, SeekOrigin.Begin);
			byte[] bytes = Encoding.UTF8.GetBytes("");
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
			string strSend = "https://github.com/chummer5a/chummer5a/issues/new?labels=new&title={0}&body={1}";
			strSend = strSend.Replace("{0}",$" Issue: - PLEASE ENTER DESCRIPTION HERE");
			string strBody = "";
			strBody += "### Environment\n";
			strBody += $"Crash ID: {_dumper.Attributes["visible-crash-id"]}\n";
			strBody += $"Chummer Version: {_dumper.Attributes["visible-version"]}\n";
			strBody += $"Environment: {_dumper.Attributes["os-name"]}\n";
			strBody += $"Runtime: {Environment.Version}\n";
			strBody += txtUserStory.Text;
			strBody = System.Net.WebUtility.HtmlEncode(strBody);
			strBody = strBody.Replace(" ", "%20");
			strBody = strBody.Replace("#", "%23");
			strBody = strBody.Replace("\n", "%0D%0A");
			strSend = strSend.Replace("{1}", strBody);

			Process.Start(strSend);
			btnSend_Click(sender, e);
		}
		private static string Md5Hash(string input)
		{
			StringBuilder hash = new StringBuilder();
			MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
			byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

			for (int i = 0; i < bytes.Length; i++)
			{
				hash.Append(bytes[i].ToString("x2"));
			}
			return hash.ToString();
		}
	}
}
