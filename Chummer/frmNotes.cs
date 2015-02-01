using System;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmNotes : Form
	{
		private static int _intWidth = 534;
		private static int _intHeight = 278;
		private readonly bool _blnLoading = false;

		#region Control Events
		public frmNotes()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_blnLoading = true;
			this.Width = _intWidth;
			this.Height = _intHeight;
			_blnLoading = false;
		}

		private void frmNotes_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void txtNotes_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				this.DialogResult = DialogResult.OK;
		}

		private void frmNotes_Resize(object sender, EventArgs e)
		{
			if (_blnLoading)
				return;

			_intWidth = this.Width;
			_intHeight = this.Height;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return txtNotes.Text;
			}
			set
			{
				txtNotes.Text = value;
				txtNotes.Select(txtNotes.Text.Length, 0);
			}
		}
		#endregion
	}
}