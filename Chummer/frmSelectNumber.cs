using System;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmSelectNumber : Form
	{
		private int _intReturnValue = 0;

		#region Control Events
		public frmSelectNumber()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		private void frmSelectNumber_Shown(object sender, EventArgs e)
		{
			nudNumber.Focus();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			_intReturnValue = Convert.ToInt32(nudNumber.Value);
			this.DialogResult = DialogResult.OK;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Value that was entered in the dialogue.
		/// </summary>
		public int SelectedValue
		{
			get
			{
				return _intReturnValue;
			}
			set
			{
				nudNumber.Value = value;
			}
		}

		/// <summary>
		/// Minimum number.
		/// </summary>
		public int Minimum
		{
			set
			{
				nudNumber.Minimum = value;
			}
		}

		/// <summary>
		/// Maximum number.
		/// </summary>
		public int Maximum
		{
			set
			{
				nudNumber.Maximum = value;
			}
		}

		/// <summary>
		/// Description to display in the dialogue.
		/// </summary>
		public string Description
		{
			set
			{
				lblDescription.Text = value;
			}
		}

		/// <summary>
		/// Whether or not the Cancel button is enabled.
		/// </summary>
		public bool AllowCancel
		{
			set
			{
				cmdCancel.Enabled = value;
				if (!value)
					this.ControlBox = false;
			}
		}
		#endregion
	}
}