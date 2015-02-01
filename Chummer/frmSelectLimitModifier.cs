using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectLimitModifier : Form
    {
        private string _strReturnName = "";
        private int _intBonus = 1;
        private string _strCondition = "";

        #region Control Events
        public frmSelectLimitModifier()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

		private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnName = txtName.Text;
            _intBonus = Convert.ToInt32(nudBonus.Value);
            _strCondition = txtCondition.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

		private void frmSelectText_Shown(object sender, EventArgs e)
		{
			// If the field is pre-populated, immediately click OK.
			if (txtName.Text != "")
				cmdOK_Click(sender, e);
		}		
		#endregion

		#region Properties
		/// <summary>
		/// Modifier name that was entered in the dialogue.
		/// </summary>
		public string SelectedName
        {
            get
            {
                return _strReturnName;
            }
			set
			{
				txtName.Text = value;
			}
        }

        /// <summary>
        /// Modifier condition that was entered in the dialogue.
        /// </summary>
        public string SelectedCondition
        {
            get
            {
                return _strCondition;
            }
            set
            {
                txtCondition.Text = value;
            }
        }

        /// <summary>
        /// Modifier Bonus that was entered in the dialogue.
        /// </summary>
        public int SelectedBonus
        {
            get
            {
                return _intBonus;
            }
            set
            {
                nudBonus.Value = value;
            }
        }

        #endregion

    }
}
