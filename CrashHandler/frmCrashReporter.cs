using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrashHandler
{
	public partial class frmCrashReporter : Form
	{
		delegate void ChangeDesc(string desc);
		private readonly CrashDumper _dumper;

		public frmCrashReporter(CrashDumper dumper)
		{
			_dumper = dumper;
			InitializeComponent();
			lblDesc.Text = _dumper.Attributes["visible-error-friendly"];
			lblId.Text = "Crash followup id = " + _dumper.Attributes["visible-crash-id"];
			
			_dumper.CrashDumperProgressChanged += DumperOnCrashDumperProgressChanged;
		}

		private void frmCrashReporter_Load(object sender, EventArgs e)
		{
			_dumper.StartCollecting();
		}

		private void DumperOnCrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
		{
			Invoke(new ChangeDesc(ChangeProgress), args.Progress.GetDescription());
		}

		private void ChangeProgress(string desc)
		{
			statusCollectionProgess.Text = desc;
		}

		private void llblContents_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("explorer.exe", _dumper.WorkingDirectory);
		}

		private void txtDesc_TextChanged(object sender, EventArgs e)
		{

		}

		private void btnNo_Click(object sender, EventArgs e)
		{

		}

		private void btnSend_Click(object sender, EventArgs e)
		{

		}

		
	}
}
