using System;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmLifestyleNuyen : Form
	{
		private int _intDice = 0;
		private int _intExtra = 0;
		private int _intMultiplier = 0;

		#region Control Events
		public frmLifestyleNuyen()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			MoveControls();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void frmLifestyleNuyen_Load(object sender, EventArgs e)
		{
			lblDice.Text = LanguageManager.Instance.GetString("Label_LifestyleNuyen_ResultOf") + " " + _intDice.ToString() + "D6: (";
			nudDiceResult.Maximum = _intDice * 6;
			nudDiceResult.Minimum = _intDice;
			lblResult.Text = " + " + _intExtra.ToString() + ") x " + _intMultiplier.ToString() + " = " + string.Format("{0:###,###,##0¥}", (nudDiceResult.Value + _intExtra) * _intMultiplier);
			MoveControls();
		}

		private void nudDiceResult_ValueChanged(object sender, EventArgs e)
		{
			lblResult.Text = " + " + _intExtra.ToString() + ") x " + _intMultiplier.ToString() + " = " + string.Format("{0:###,###,##0¥}", (nudDiceResult.Value + _intExtra) * _intMultiplier);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Number of dice that are rolled for the lifestyle.
		/// </summary>
		public int Dice
		{
			get{
				return _intDice;
			}
			set
			{
				_intDice = value;
			}
		}

		/// <summary>
		/// Extra number that is added to the dice roll.
		/// </summary>
		public int Extra
		{
			get
			{
				return _intExtra;
			}
			set
			{
				_intExtra = value;
			}
		}

		/// <summary>
		/// D6 multiplier for the Lifestyle.
		/// </summary>
		public int Multiplier
		{
			get
			{
				return _intMultiplier;
			}
			set
			{
				_intMultiplier = value;
			}
		}

		/// <summary>
		/// The total amount of Nuyen resulting from the dice roll.
		/// </summary>
		public int StartingNuyen
		{
			get
			{
				return Convert.ToInt32((nudDiceResult.Value + _intExtra) * _intMultiplier);
			}
		}
		#endregion

		#region Methods
		private void MoveControls()
		{
			nudDiceResult.Left = lblDice.Left + lblDice.Width + 6;
			lblResult.Left = nudDiceResult.Left + nudDiceResult.Width + 6;
		}
		#endregion
	}
}