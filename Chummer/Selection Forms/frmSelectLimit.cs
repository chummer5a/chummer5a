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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectLimit : Form
    {
        private string _strReturnValue = string.Empty;

        private List<ListItem> _lstLimits = new List<ListItem>();

        #region Control Events
        public frmSelectLimit()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            // Build the list of Limits.
            ListItem objPhysical = new ListItem();
            ListItem objMental = new ListItem();
            ListItem objSocial = new ListItem();
            objPhysical.Value = "Physical";
            objPhysical.Name = LanguageManager.Instance.GetString("Node_Physical");
            objMental.Value = "Mental";
            objMental.Name = LanguageManager.Instance.GetString("Node_Mental");
            objSocial.Value = "Social";
            objSocial.Name = LanguageManager.Instance.GetString("Node_Social");
            _lstLimits.Add(objPhysical);
            _lstLimits.Add(objMental);
            _lstLimits.Add(objSocial);

            cboLimit.BeginUpdate();
            cboLimit.ValueMember = "Value";
            cboLimit.DisplayMember = "Name";
            cboLimit.DataSource = _lstLimits;
            cboLimit.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboLimit.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void frmSelectLimit_Load(object sender, EventArgs e)
        {
            // Select the first Limit in the list.
            cboLimit.SelectedIndex = 0;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void frmSelectLimit_Shown(object sender, EventArgs e)
        {
            // If only a single Limit is in the list when the form is shown,
            // click the OK button since the user really doesn't have a choice.
            if (cboLimit.Items.Count == 1)
                cmdOK_Click(sender, e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Limit that was selected in the dialogue.
        /// </summary>
        public string SelectedLimit
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
        /// Limit the list to a single Limit.
        /// </summary>
        /// <param name="strValue">Single Limit to display.</param>
        public void SingleLimit(string strValue)
        {
            List<ListItem> lstItems = new List<ListItem>();
            ListItem objItem = new ListItem();
            objItem.Value = strValue;
            objItem.Name = strValue;
            lstItems.Add(objItem);
            cboLimit.BeginUpdate();
            cboLimit.DataSource = null;
            cboLimit.ValueMember = "Value";
            cboLimit.DisplayMember = "Name";
            cboLimit.DataSource = lstItems;
            cboLimit.EndUpdate();
        }

        /// <summary>
        /// Limit the list to a few Limits.
        /// </summary>
        /// <param name="strValue">List of Limits.</param>
        public void LimitToList(List<string> strValue)
        {
            _lstLimits.Clear();
            foreach (string strLimit in strValue)
            {
                ListItem objItem = new ListItem();
                objItem.Value = strLimit;
                objItem.Name = LanguageManager.Instance.GetString("String_Limit" + strLimit + "Short");
                _lstLimits.Add(objItem);
            }
            cboLimit.BeginUpdate();
            cboLimit.DataSource = null;
            cboLimit.ValueMember = "Value";
            cboLimit.DisplayMember = "Name";
            cboLimit.DataSource = _lstLimits;
            cboLimit.EndUpdate();
        }

        /// <summary>
        /// Exclude the list of Limits.
        /// </summary>
        /// <param name="strValue">List of Limits.</param>
        public void RemoveFromList(List<string> strValue)
        {
            foreach (string strLimit in strValue)
            {
                foreach (ListItem objItem in _lstLimits)
                {
                    if (objItem.Value == strLimit)
                    {
                        _lstLimits.Remove(objItem);
                        break;
                    }
                }
            }
            cboLimit.BeginUpdate();
            cboLimit.DataSource = null;
            cboLimit.ValueMember = "Value";
            cboLimit.DisplayMember = "Name";
            cboLimit.DataSource = _lstLimits;
            cboLimit.EndUpdate();
        }
        #endregion
    }
}
