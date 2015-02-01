using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using Chummer.OmaeService;

namespace Chummer
{
	public partial class frmOmaeCompress : Form
	{
		private readonly OmaeHelper _objOmaeHelper = new OmaeHelper();
		private List<string> _lstFiles = new List<string>();

		#region Control Events
		public frmOmaeCompress()
		{
			InitializeComponent();
		}

		private void cmdCompress_Click(object sender, EventArgs e)
		{
			_lstFiles.Clear();
			GetDirectories(txtFilePath.Text);
			_objOmaeHelper.CompressMutipleToFile(_lstFiles, txtDestination.Text);
			MessageBox.Show("Done");
		}
		#endregion

		#region Methods
		private void GetDirectories(string strDirectory)
		{
			foreach (string strFound in Directory.GetDirectories(strDirectory))
			{
				GetDirectories(strFound);
			}
			foreach (string strFile in Directory.GetFiles(strDirectory, "*.chum5"))
				_lstFiles.Add(strFile);
		}
		#endregion
	}
}