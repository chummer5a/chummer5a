using System;
using System.Windows.Forms;
using Chummer.OmaeService;

namespace Chummer
{
	public partial class frmOmaeAccount : Form
	{
		private string _strUserName = "";
		private readonly OmaeHelper _objOmaeHelper = new OmaeHelper();

		#region Control Events
		public frmOmaeAccount(string strUserName)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_strUserName = strUserName;
			MoveControls();
		}

		private void frmOmaeAccount_Load(object sender, EventArgs e)
		{
			omaeSoapClient objService = _objOmaeHelper.GetOmaeService();
			txtEmail.Text = objService.GetEmailAddress(_strUserName);
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			omaeSoapClient objService = _objOmaeHelper.GetOmaeService();
			objService.SetEmailAddress(_strUserName, txtEmail.Text);
			this.DialogResult = DialogResult.OK;
		}
		#endregion

		#region Methods
		private void MoveControls()
		{
			txtEmail.Left = lblEmail.Left + lblEmail.Width + 6;
			txtEmail.Width = this.Width - txtEmail.Left - 19;
		}
		#endregion
	}
}