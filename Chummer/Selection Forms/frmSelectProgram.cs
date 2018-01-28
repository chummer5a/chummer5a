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
using System.Linq;
 using System.Runtime.Serialization;
 using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectProgram : Form
    {
        private string _strSelectedComplexForm = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain = false;
        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument = null;

        //private bool _blnBiowireEnabled = false;

        #region Control Events
        public frmSelectProgram(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Programs information.
            _objXmlDocument = XmlManager.Load("complexforms.xml");
        }

        private void frmSelectProgram_Load(object sender, EventArgs e)
        {
            _blnLoading = false;
            BuildComplexFormList();
        }

        private void lstPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedComplexFormId = lstPrograms.SelectedValue?.ToString();
            if (_blnLoading || string.IsNullOrEmpty(strSelectedComplexFormId))
            {
                lblDuration.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblFV.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
                return;
            }

            // Display the Program information.
            XmlNode objXmlProgram = _objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[id = \"" + strSelectedComplexFormId + "\"]");
            if (objXmlProgram != null)
            {
                string strDuration = objXmlProgram["duration"].InnerText;
                string strTarget = objXmlProgram["target"].InnerText;
                string strFV = objXmlProgram["fv"].InnerText;

                lblDuration.Text = strDuration;
                lblTarget.Text = strTarget;
                lblFV.Text = strFV;

                string strSource = objXmlProgram["source"].InnerText;
                string strPage = objXmlProgram["altpage"]?.InnerText ?? objXmlProgram["page"].InnerText;
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

                tipTooltip.SetToolTip(lblSource,
                    CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            }
            else
            {
                lblDuration.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblFV.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void lstPrograms_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildComplexFormList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstPrograms.SelectedIndex == -1)
            {
                if (lstPrograms.Items.Count > 0)
                    lstPrograms.SelectedIndex = 0;
            }
            if (e.KeyCode == Keys.Down)
            {
                int intNewIndex = lstPrograms.SelectedIndex + 1;
                if (intNewIndex >= lstPrograms.Items.Count)
                    intNewIndex = 0;
                if (lstPrograms.Items.Count > 0)
                    lstPrograms.SelectedIndex = intNewIndex;
            }
            if (e.KeyCode == Keys.Up)
            {
                int intNewIndex = lstPrograms.SelectedIndex - 1;
                if (intNewIndex <= 0)
                    intNewIndex = lstPrograms.Items.Count - 1;
                if (lstPrograms.Items.Count > 0)
                    lstPrograms.SelectedIndex = intNewIndex;
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Program that was selected in the dialogue.
        /// </summary>
        public string SelectedComplexForm
        {
            get
            {
                return _strSelectedComplexForm;
            }
        }
        #endregion

        #region Methods
        private void BuildComplexFormList()
        {
            if (_blnLoading)
                return;

            XmlNodeList xmlOptionalComplexFormList = null;
            if (_objCharacter.IsCritter)
            {
                xmlOptionalComplexFormList = XmlManager.Load("critters.xml").SelectNodes("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]/optionalcomplexforms/complexform");
            }

            string strFilter = "(" + _objCharacter.Options.BookXPath() + ')';

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Populate the Complex Form list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/complexforms/complexform[" + strFilter + ']');

            List<ListItem> lstComplexFormItems = new List<ListItem>();
            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                string strName = objXmlProgram["name"]?.InnerText ?? string.Empty;
                // If this is a Sprite with Optional Complex Forms, see if this Complex Form is allowed.
                if (xmlOptionalComplexFormList?.Count > 0)
                {
                    bool blnAdd = false;
                    foreach (XmlNode objXmlForm in xmlOptionalComplexFormList)
                    {
                        if (objXmlForm.InnerText == strName)
                        {
                            blnAdd = true;
                            break;
                        }
                    }
                    if (!blnAdd)
                        continue;
                }

                lstComplexFormItems.Add(new ListItem(objXmlProgram["id"].InnerText, objXmlProgram["translate"]?.InnerText ?? strName));
            }

            lstComplexFormItems.Sort(CompareListItems.CompareNames);
            _blnLoading = true;
            string strOldSelected = lstPrograms.SelectedValue?.ToString();
            lstPrograms.BeginUpdate();
            lstPrograms.ValueMember = "Value";
            lstPrograms.DisplayMember = "Name";
            lstPrograms.DataSource = lstComplexFormItems;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstPrograms.SelectedValue = strOldSelected;
            else
                lstPrograms.SelectedIndex = -1;
            lstPrograms.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstPrograms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                _strSelectedComplexForm = strSelectedItem;
                DialogResult = DialogResult.OK;
            }
        }

        private void MoveControls()
        {
            int intLeft = lblDurationLabel.Width;
            intLeft = Math.Max(intLeft, lblTargetLabel.Width);
            intLeft = Math.Max(intLeft, lblFV.Width);
            intLeft = Math.Max(intLeft, lblSourceLabel.Width);

            lblTarget.Left = lblTargetLabel.Left + intLeft + 6;
            lblDuration.Left = lblDurationLabel.Left + intLeft + 6;
            lblFV.Left = lblFVLabel.Left + intLeft + 6;
            lblSource.Left = lblSourceLabel.Left + intLeft + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion
    }
}
