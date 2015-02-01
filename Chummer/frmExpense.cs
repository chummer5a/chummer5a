using System;
using System.Globalization;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmExpense : Form
	{
		private ExpenseType _objMode = ExpenseType.Karma;

		#region Control Events
		public frmExpense()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

			// Determine the DateTime format and use that to display the date field (removing seconds since they're not important).
			DateTimeFormatInfo objDateTimeInfo = CultureInfo.CurrentCulture.DateTimeFormat;
			string strDatePattern = objDateTimeInfo.FullDateTimePattern.Replace(":ss", string.Empty);
			if (!GlobalOptions.Instance.DatesIncludeTime)
				strDatePattern = objDateTimeInfo.LongDatePattern;
			datDate.CustomFormat = strDatePattern;

			datDate.Value = DateTime.Now;

			txtDescription.Text = LanguageManager.Instance.GetString("String_ExpenseDefault");
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

		#region Properties
		/// <summary>
		/// Amount gained or spent.
		/// </summary>
		public int Amount
		{
			get
			{
				decimal decReturn = nudAmount.Value;
				if (_objMode == ExpenseType.Nuyen)
					decReturn *= (nudPercent.Value / 100);
				return Convert.ToInt32(Math.Round(decReturn, 0));
			}
			set
			{
				if (value < 0)
					nudAmount.Minimum = value;
				if (value == 0)
					nudAmount.Minimum = 0;
				nudAmount.Value = value;
			}
		}

		/// <summary>
		/// Reason for the Karma change.
		/// </summary>
		public string strReason
		{
			get
			{
				return txtDescription.Text;
			}
			set
			{
				txtDescription.Text = value;
			}
		}

		/// <summary>
		/// Whether or not this is a Karma refund.
		/// </summary>
		public bool Refund
		{
			get
			{
				return chkRefund.Checked;
			}
			set
			{
				chkRefund.Checked = value;
			}
		}

		/// <summary>
		/// Date and Time that was selected.
		/// </summary>
		public DateTime SelectedDate
		{
			get
			{
				return datDate.Value;
			}
			set
			{
				datDate.Value = value;
			}
		}

		/// <summary>
		/// The Expense's mode (either Karma or Nuyen).
		/// </summary>
		public ExpenseType Mode
		{
			set
			{
				if (value == ExpenseType.Nuyen)
				{
					lblKarma.Text = LanguageManager.Instance.GetString("Label_Expense_NuyenAmount");
					this.Text = LanguageManager.Instance.GetString("Title_Expense_Nuyen");
					chkRefund.Text = LanguageManager.Instance.GetString("Checkbox_Expense_RefundNuyen");
					nudPercent.Visible = true;
					lblPercent.Visible = true;
				}
				else
				{
					lblKarma.Text = LanguageManager.Instance.GetString("Label_Expense_KarmaAmount");
					this.Text = LanguageManager.Instance.GetString("Title_Expense_Karma");
					nudPercent.Visible = false;
					lblPercent.Visible = false;
				}
				_objMode = value;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Lock fields on the Form so that only the Date and Reason fields are editable.
		/// </summary>
		public void LockFields(bool blnEditAmount = false)
		{
			nudAmount.Enabled = blnEditAmount;
			nudPercent.Enabled = false;
			chkRefund.Enabled = false;

			if (blnEditAmount && nudAmount.Minimum < 0)
				nudAmount.Minimum = nudAmount.Maximum * -1;
		}
		#endregion
	}
}