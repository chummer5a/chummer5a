using System;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmSellItem : Form
	{
		#region Control Events
		public frmSellItem()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			MoveControls();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
		#endregion

		#region Methods
		private void MoveControls()
		{
			nudPercent.Left = lblSellForLabel.Left + lblSellForLabel.Width + 6;
			lblPercentLabel.Left = nudPercent.Left + nudPercent.Width + 6;
			this.Width = lblPercentLabel.Left + lblPercentLabel.Width + 19;
			if (this.Width < 185)
				this.Width = 185;
		}
		#endregion

		#region Properties
		/// <summary>
		/// The percentage the item will be sold at.
		/// </summary>
		public double SellPercent
		{
			get
			{
				return Convert.ToDouble(nudPercent.Value / 100, GlobalOptions.Instance.CultureInfo);
			}
		}
		#endregion
	}
}