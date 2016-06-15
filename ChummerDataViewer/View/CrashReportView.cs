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
		public CrashReportView(CrashReport report)
		{
			InitializeComponent();

			lblBuildType.Text = report.BuildType;
			lblGuid.Text = report.Guid.ToString();
			lblVersion.Text = report.Version.ToString(3);
			lblDate.Text = report.Timestamp.ToString("d MMM yy");

		}

		private void btnAction_Click(object sender, EventArgs e)
		{

		}
	}
}
