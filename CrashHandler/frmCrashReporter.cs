using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrashHandler
{
	public partial class frmCrashReporter : Form
	{

		delegate void ChangeDesc(CrashDumperProgress progress, string desc);
		private readonly CrashDumper _dumper;

		public frmCrashReporter(CrashDumper dumper)
		{
			_dumper = dumper;
			InitializeComponent();
			lblDesc.Text = _dumper.Attributes["visible-error-friendly"];
			txtIdSelectable.Text = "Crash followup id = " + _dumper.Attributes["visible-crash-id"];
			
			_dumper.CrashDumperProgressChanged += DumperOnCrashDumperProgressChanged;
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
		}

		private void btnNo_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			_dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
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
			byte[] bytes = Encoding.UTF8.GetBytes(txtDesc.Text);
			fs.Write(bytes, 0, bytes.Length);
			fs.Flush(true);
		}

		private void frmCrashReporter_FormClosing(object sender, FormClosingEventArgs e)
		{
			_dumper.CrashDumperProgressChanged -= DumperOnCrashDumperProgressChanged;
		}
	}
}
