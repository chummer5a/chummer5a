using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrashHandler
{
	public partial class frmNoMoreUserInput : Form
	{
		delegate void ChangeDesc(CrashDumperProgress progress, string desc);


		public frmNoMoreUserInput(CrashDumper dmper)
		{
			InitializeComponent();

			lblProgress.Text = dmper.Progress.GetDescription();

			dmper.CrashDumperProgressChanged += Dmper_CrashDumperProgressChanged;
		}

		private void Dmper_CrashDumperProgressChanged(object sender, CrashDumperProgressChangedEventArgs args)
		{
			Invoke(new ChangeDesc(ChangeProgress), args.Progress, args.Progress.GetDescription());
		}

		private void ChangeProgress(CrashDumperProgress progress, string desc)
		{
			if (progress == CrashDumperProgress.FinishedSending)
			{
				Close();
			}

			lblProgress.Text = desc;
		}
	}
}
