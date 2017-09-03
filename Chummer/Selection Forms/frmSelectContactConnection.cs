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
using System.Drawing;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectContactConnection : Form
    {
        private int _intMembership = 0;
        private int _intAreaOfInfluence = 0;
        private int _intMagicalResources = 0;
        private int _intMatrixResources = 0;
        private string _strGroupName = string.Empty;
        private Color _objColour;
        private bool _blnFree = false;
        private bool _blnSkipUpdate = false;

        #region Control Events
        public frmSelectContactConnection()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void cboMembership_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cboAreaOfInfluence_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cboMagicalResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cboMatrixResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void chkFreeContact_CheckedChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void frmSelectContactConnection_Load(object sender, EventArgs e)
        {
            // Populate the fields with their data.
            // Membership.
            cboMembership.Items.Add("+0: " + LanguageManager.Instance.GetString("String_None"));
            cboMembership.Items.Add("+1: " + LanguageManager.Instance.GetString("String_SelectContactConnection_Members").Replace("{0}", "2-19"));
            cboMembership.Items.Add("+2: " + LanguageManager.Instance.GetString("String_SelectContactConnection_Members").Replace("{0}", "20-99"));
            cboMembership.Items.Add("+4: " + LanguageManager.Instance.GetString("String_SelectContactConnection_Members").Replace("{0}", "100-1000"));
            cboMembership.Items.Add("+6: " + LanguageManager.Instance.GetString("String_SelectContactConnection_Members").Replace("{0}", "1000+"));

            // Area of Influence.
            cboAreaOfInfluence.Items.Add("+0: " + LanguageManager.Instance.GetString("String_None"));
            cboAreaOfInfluence.Items.Add("+1: " + LanguageManager.Instance.GetString("String_SelectContactConnection_AreaDistrict"));
            cboAreaOfInfluence.Items.Add("+2: " + LanguageManager.Instance.GetString("String_SelectContactConnection_AreaSprawlwide"));
            cboAreaOfInfluence.Items.Add("+4: " + LanguageManager.Instance.GetString("String_SelectContactConnection_AreaNational"));
            cboAreaOfInfluence.Items.Add("+6: " + LanguageManager.Instance.GetString("String_SelectContactConnection_AreaGlobal"));

            // Magical Resources.
            cboMagicalResources.Items.Add("+0: " + LanguageManager.Instance.GetString("String_None"));
            cboMagicalResources.Items.Add("+1: " + LanguageManager.Instance.GetString("String_SelectContactConnection_MagicalMinority"));
            cboMagicalResources.Items.Add("+4: " + LanguageManager.Instance.GetString("String_SelectContactConnection_MagicalMost"));
            cboMagicalResources.Items.Add("+6: " + LanguageManager.Instance.GetString("String_SelectContactConnection_MagicalVast"));

            // Matrix Resources.
            cboMatrixResources.Items.Add("+0: " + LanguageManager.Instance.GetString("String_None"));
            cboMatrixResources.Items.Add("+1: " + LanguageManager.Instance.GetString("String_SelectContactConnection_MatrixActive"));
            cboMatrixResources.Items.Add("+2: " + LanguageManager.Instance.GetString("String_SelectContactConnection_MatrixBroad"));
            cboMatrixResources.Items.Add("+4: " + LanguageManager.Instance.GetString("String_SelectContactConnection_MatrixPervasive"));

            // Select the appropriate field values.
            _blnSkipUpdate = true;
            cboMembership.SelectedIndex = cboMembership.FindString("+" + _intMembership.ToString());
            cboAreaOfInfluence.SelectedIndex = cboAreaOfInfluence.FindString("+" + _intAreaOfInfluence.ToString());
            cboMagicalResources.SelectedIndex = cboMagicalResources.FindString("+" + _intMagicalResources.ToString());
            cboMatrixResources.SelectedIndex = cboMatrixResources.FindString("+" + _intMatrixResources.ToString());
            txtGroupName.Text = _strGroupName;
            cmdChangeColour.BackColor = _objColour;
            chkFreeContact.Checked = _blnFree;
            _blnSkipUpdate = false;

            lblTotalConnectionModifier.Text = (_intMembership + _intAreaOfInfluence + _intMagicalResources + _intMatrixResources).ToString();
        }

        private void cmdChangeColour_Click(object sender, EventArgs e)
        {
            ColorDialog dlgColour = new ColorDialog();
            dlgColour.ShowDialog(this);

            if (dlgColour.Color.Name == "White" || dlgColour.Color.Name == "Black")
            {
                cmdChangeColour.BackColor = SystemColors.Control;
                _objColour = SystemColors.Control;
            }
            else
            {
                cmdChangeColour.BackColor = dlgColour.Color;
                _objColour = dlgColour.Color;
            }
        }

        private void cboField_DropDown(object sender, EventArgs e)
        {
            // Resize the width of the DropDown so that the longest name fits.
            ComboBox objSender = (ComboBox)sender;
            int intWidth = objSender.DropDownWidth;
            Graphics objGraphics = objSender.CreateGraphics();
            Font objFont = objSender.Font;
            int intScrollWidth = (objSender.Items.Count > objSender.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;
            int intNewWidth;
            foreach (string strItem in ((ComboBox)sender).Items)
            {
                intNewWidth = (int)objGraphics.MeasureString(strItem, objFont).Width + intScrollWidth;
                if (intWidth < intNewWidth)
                {
                    intWidth = intNewWidth;
                }
            }
            objSender.DropDownWidth = intWidth;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Membership.
        /// </summary>
        public int Membership
        {
            get
            {
                return _intMembership;
            }
            set
            {
                _intMembership = value;
            }
        }

        /// <summary>
        /// Area of Influence.
        /// </summary>
        public int AreaOfInfluence
        {
            get
            {
                return _intAreaOfInfluence;
            }
            set
            {
                _intAreaOfInfluence = value;
            }
        }

        /// <summary>
        /// Magical Resources.
        /// </summary>
        public int MagicalResources
        {
            get
            {
                return _intMagicalResources;
            }
            set
            {
                _intMagicalResources = value;
            }
        }

        /// <summary>
        /// Matrix Resources.
        /// </summary>
        public int MatrixResources
        {
            get
            {
                return _intMatrixResources;
            }
            set
            {
                _intMatrixResources = value;
            }
        }

        /// <summary>
        /// Group Name.
        /// </summary>
        public string GroupName
        {
            get
            {
                return _strGroupName;
            }
            set
            {
                _strGroupName = value;
            }
        }

        /// <summary>
        /// Contact Colour.
        /// </summary>
        public Color Colour
        {
            get
            {
                return _objColour;
            }
            set
            {
                _objColour = value;
            }
        }

        /// <summary>
        /// Whether or not this is a free contact.
        /// </summary>
        public bool Free
        {
            get
            {
                return _blnFree;
            }
            set
            {
                _blnFree = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Refresh the Connection Group information.
        /// </summary>
        private void ValuesChanged()
        {
            if (_blnSkipUpdate)
                return;

            _intMembership = Convert.ToInt32(cboMembership.Text.Substring(0, 2));
            _intAreaOfInfluence = Convert.ToInt32(cboAreaOfInfluence.Text.Substring(0, 2));
            _intMagicalResources = Convert.ToInt32(cboMagicalResources.Text.Substring(0, 2));
            _intMatrixResources = Convert.ToInt32(cboMatrixResources.Text.Substring(0, 2));
            _strGroupName = txtGroupName.Text;
            _blnFree = chkFreeContact.Checked;

            lblTotalConnectionModifier.Text = (_intMembership + _intAreaOfInfluence + _intMagicalResources + _intMatrixResources).ToString();
        }

        private void MoveControls()
        {
            lblTotalConnectionModifier.Left = lblTotalConnectionModifierLabel.Left + lblTotalConnectionModifierLabel.Width + 6;
        }
        #endregion
    }
}