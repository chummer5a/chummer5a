using System;
using System.Linq;
using System.Windows.Forms;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
	public partial class SetupForm : Form
	{
		public string Id => txtId.Text;
		public string Key => txtKey.Text;

		public SetupForm()
		{
			InitializeComponent();
		}

		private void SetupForm_Load(object sender, EventArgs e)
		{
			lblDb.Text = $"{PersistentState.FolderPath}";
		}

		private void AnyTextChanged(object sender, EventArgs e)
		{
			lblStatus.Visible = true;
			btnOK.Enabled = false;

			if (!txtId.Text.All(x => char.IsUpper(x) || char.IsDigit(x)))
			{
				lblStatus.Text = "Invalid Character in Access Id";
				return;
			}

			if (txtId.Text.Length != 20)
			{
				lblStatus.Text = "Access Id too " + (txtId.Text.Length > 20 ? "long" : "short");
				return;
			}

			if (txtKey.Text.Any(x => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVXYZ0123456789+/".IndexOf(x) == -1))
			{
				lblStatus.Text = "Invalid Character in Access Key";
				return;
			}

			if (txtKey.Text.Length != 40)
			{
				lblStatus.Text = "Access Key too " + (txtKey.Text.Length > 40 ? "long" : "short");
				return;
			}


			lblStatus.Visible = false;
			btnOK.Enabled = true;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void btnCanel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
