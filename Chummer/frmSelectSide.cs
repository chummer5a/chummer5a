using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Chummer
{
	public partial class frmSelectSide : Form
	{
		private string _strSelectedSide = "";

		#region Control Events
		public frmSelectSide()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

			// Create a list for the sides.
			List<ListItem> lstSides = new List<ListItem>();
			ListItem objLeft = new ListItem();
			objLeft.Value = "Left";
			objLeft.Name = LanguageManager.Instance.GetString("String_Improvement_SideLeft");

			ListItem objRight = new ListItem();
			objRight.Value = "Right";
			objRight.Name = LanguageManager.Instance.GetString("String_Improvement_SideRight");

			lstSides.Add(objLeft);
			lstSides.Add(objRight);

			cboSide.DataSource = lstSides;
			cboSide.ValueMember = "Value";
			cboSide.DisplayMember = "Name";
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			_strSelectedSide = cboSide.Text;
			this.DialogResult = DialogResult.OK;
		}

		private void frmSelectSide_Load(object sender, EventArgs e)
		{
			// Select the first item in the list.
			cboSide.SelectedIndex = 0;
		}
		#endregion

		#region Properties
		// Description to show in the window.
		public string Description
		{
			set
			{
				lblDescription.Text = value;
			}
		}

		/// <summary>
		/// Side that was selected in the dialogue.
		/// </summary>
		public string SelectedSide
		{
			get
			{
				return _strSelectedSide;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Force a particular value to be selected in the window.
		/// </summary>
		/// <param name="strSide">Value to force.</param>
		public void ForceValue(string strSide)
		{
			cboSide.SelectedValue = strSide;
			cboSide.Text = strSide;
			cmdOK_Click(this, null);
		}
		#endregion
	}
}