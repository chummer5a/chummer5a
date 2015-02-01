using System;
using System.IO;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmHistory : Form
	{
		#region Control Events
		public frmHistory()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		private void frmHistory_Load(object sender, EventArgs e)
		{
			// Display the contents of the changelog.txt file in the TextBox.
			try
			{
				txtRevisionHistory.Text = File.ReadAllText(Application.StartupPath + Path.DirectorySeparatorChar + "changelog.txt");
				txtRevisionHistory.SelectionStart = 0;
				txtRevisionHistory.SelectionLength = 0;
			}
			catch
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_History_FileNotFound"), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.Close();
			}
		}
		#endregion
	}
}