/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
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
		/// <param name="lstValue">List of Powers.</param>
		public void LimitToList(List<KeyValuePair<string, string>> lstValue)
		{
			_lstPowers.Clear();
			foreach (KeyValuePair<string, string> lstObject in lstValue)
            {
				ListItem objItem = new ListItem();
				objItem.Value = lstObject.Key;
				if (lstObject.Value != "")
				{
					objItem.Name = lstObject.Key + " (" + lstObject.Value + ")";
				}
				else
				{
					objItem.Name = lstObject.Key;
                }
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