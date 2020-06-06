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
 using System;
using System.Collections.Generic;
 using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectAttribute : Form
    {
        private string _strReturnValue = string.Empty;

        private readonly List<ListItem> _lstAttributes;

        #region Control Events
        public frmSelectAttribute(params string[] lstAttributeAbbrevs)
        {
            InitializeComponent();
            this.TranslateWinForm();

            // Build the list of Attributes.
            _lstAttributes = new List<ListItem>(lstAttributeAbbrevs.Length);
            foreach (string strAbbrev in lstAttributeAbbrevs)
            {
                string strAttributeDisplayName = strAbbrev == "MAGAdept"
                    ? LanguageManager.GetString("String_AttributeMAGShort") + " (" + LanguageManager.GetString("String_DescAdept") + ')'
                    : LanguageManager.GetString("String_Attribute" + strAbbrev + "Short");
                _lstAttributes.Add(new ListItem(strAbbrev, strAttributeDisplayName));
            }

            cboAttribute.BeginUpdate();
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            if (_lstAttributes.Count >= 1)
                cboAttribute.SelectedIndex = 0;
            else
                cmdOK.Enabled = false;
            cboAttribute.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboAttribute.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void frmSelectAttribute_Load(object sender, EventArgs e)
        {
            if (_lstAttributes.Count == 1)
            {
                cmdOK_Click(sender, e);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Attribute that was selected in the dialogue.
        /// </summary>
        public string SelectedAttribute => _strReturnValue;

        /// <summary>
        /// Description to display on the form.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Whether or not the Do not affect Metatype Maximum checkbox should be shown on the form.
        /// </summary>
        public bool ShowMetatypeMaximum
        {
            set => chkDoNotAffectMetatypeMaximum.Visible = value;
        }

        /// <summary>
        /// Whether or not the Metatype Maximum value should be affected as well.
        /// </summary>
        public bool DoNotAffectMetatypeMaximum => chkDoNotAffectMetatypeMaximum.Checked;

        #endregion
    }
}
