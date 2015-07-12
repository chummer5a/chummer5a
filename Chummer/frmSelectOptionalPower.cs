using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectOptionalPower : Form
    {
        private string _strReturnValue = "";

		private List<ListItem> _lstPowers = new List<ListItem>();

		#region Control Events
		public frmSelectOptionalPower()
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

		private void cmdOK_Click(object sender, EventArgs e)
        {
			_strReturnValue = cboPower.SelectedValue.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void frmSelectOptionalPower_Load(object sender, EventArgs e)
        {
            // Select the first Power in the list.
            cboPower.SelectedIndex = 0;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

		private void frmSelectOptionalPower_Shown(object sender, EventArgs e)
		{
			// If only a single Power is in the list when the form is shown,
			// click the OK button since the user really doesn't have a choice.
			if (cboPower.Items.Count == 1)
				cmdOK_Click(sender, e);
		}
		#endregion

		#region Properties
		/// <summary>
        /// Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower
        {
            get
            {
                return _strReturnValue;
            }
        }

        /// <summary>
        /// Description to display on the form.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }
		#endregion

		#region Methods
		/// <summary>
		/// Limit the list to a single Power.
		/// </summary>
		/// <param name="strValue">Single Power to display.</param>
		public void SinglePower(string strValue)
		{
			List<ListItem> lstItems = new List<ListItem>();
			ListItem objItem = new ListItem();
			objItem.Value = strValue;
			objItem.Name = strValue;
			lstItems.Add(objItem);
			cboPower.DataSource = null;
			cboPower.ValueMember = "Value";
			cboPower.DisplayMember = "Name";
			cboPower.DataSource = lstItems;
		}

		/// <summary>
		/// Limit the list to a few Powers.
		/// </summary>
		/// <param name="strValue">List of Powers.</param>
		public void LimitToList(List<string> strValue)
		{
			_lstPowers.Clear();
			foreach (string strPower in strValue)
			{
				ListItem objItem = new ListItem();
				objItem.Value = strPower;
				objItem.Name = strPower;
                _lstPowers.Add(objItem);
			}
			cboPower.DataSource = null;
			cboPower.DataSource = _lstPowers;
			cboPower.ValueMember = "Value";
			cboPower.DisplayMember = "Name";
		}

		/// <summary>
		/// Exclude the list of Powers.
		/// </summary>
		/// <param name="strValue">List of Powers.</param>
		public void RemoveFromList(List<string> strValue)
		{
			foreach (string strPower in strValue)
			{
				foreach (ListItem objItem in _lstPowers)
				{
					if (objItem.Value == strPower)
					{
						_lstPowers.Remove(objItem);
						break;
					}
				}
			}
			cboPower.DataSource = null;
			cboPower.DataSource = _lstPowers;
			cboPower.ValueMember = "Value";
			cboPower.DisplayMember = "Name";
		}
		#endregion
    }
}