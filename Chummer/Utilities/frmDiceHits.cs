using System;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmDiceHits : Form
	{
		private int _intDice = 0;

		#region Control Events
		public frmDiceHits()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			MoveControls();
		}

		private void frmDiceHits_Load(object sender, EventArgs e)
		{
			lblDice.Text = LanguageManager.Instance.GetString("String_DiceHits_HitsOn") + " " + _intDice.ToString() + "D6: ";
			nudDiceResult.Maximum = _intDice;
			nudDiceResult.Minimum = 0;
			lblResult.Text = "";
			MoveControls();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Number of dice that are rolled for the lifestyle.
		/// </summary>
		public int Dice
		{
			get
			{
				return _intDice;
			}
			set
			{
				_intDice = value;
			}
		}

		/// <summary>
		/// Window title.
		/// </summary>
		public string Title
		{
			set
			{
				this.Text = value;
			}
		}

		/// <summary>
		/// Description text.
		/// </summary>
		public string Description
		{
			set
			{
				lblDescription.Text = value;
			}
		}

		/// <summary>
		/// Dice roll result.
		/// </summary>
		public int Result
		{
			get
			{
				return Convert.ToInt32(nudDiceResult.Value);
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