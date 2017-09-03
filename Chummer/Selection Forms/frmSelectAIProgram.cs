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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectAIProgram : Form
    {
        private string _strSelectedAIProgram = string.Empty;
        private string _strSelectedCategory = string.Empty;

        private bool _blnAddAgain = false;
        private bool _blnAdvancedProgramAllowed = true;
        private bool _blnInherentProgram = false;
        private readonly Character _objCharacter;
        private List<ListItem> _lstCategory = new List<ListItem>();

        private XmlDocument _objXmlDocument = new XmlDocument();

        #region Control Events
        public frmSelectAIProgram(Character objCharacter, bool blnAdvancedProgramAllowed = true, bool blnInherentProgram = false)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _blnAdvancedProgramAllowed = blnAdvancedProgramAllowed;
            _blnInherentProgram = blnInherentProgram;
            MoveControls();
        }

        private void frmSelectProgram_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Load the Programs information.
            _objXmlDocument = XmlManager.Instance.Load("programs.xml");

            // Populate the Category list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlNodeList)
            {
                if (_blnInherentProgram && objXmlCategory.InnerText != "Common Programs" && objXmlCategory.InnerText != "Hacking Programs")
                    continue;
                if (!_blnAdvancedProgramAllowed && objXmlCategory.InnerText == "Advanced Programs")
                    continue;
                bool blnAddItem = true;
                // Make sure it is not already in the Category list.
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == objXmlCategory.InnerText)
                    {
                        blnAddItem = false;
                        break;
                    }
                }
                if (blnAddItem)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlCategory.InnerText;
                    objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                    _lstCategory.Add(objItem);
                }
            }
            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            cboCategory.EndUpdate();

            // Select the first Category in the list.
            cboCategory.SelectedIndex = 0;

            txtSearch.Text = string.Empty;
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCategory.SelectedValue == null)
                return;

            txtSearch.Text = string.Empty;

            // Populate the Program list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/programs/program[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");

            UpdateProgramList(objXmlNodeList);
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            cboCategory_SelectedIndexChanged(sender, e);
        }

        private void lstAIPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstAIPrograms.SelectedValue == null)
                return;

            // Retrieve the information for the selected piece of Cyberware.
            XmlNode objXmlProgram = _objXmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + lstAIPrograms.SelectedValue + "\"]");
            UpdateProgramInfo(objXmlProgram);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstAIPrograms.Text))
                AcceptForm();
        }

        private void trePrograms_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstAIPrograms.Text))
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
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/programs/program[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";

            // Populate the Program list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes(strSearch);
            UpdateProgramList(objXmlNodeList);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstAIPrograms.SelectedIndex + 1 < lstAIPrograms.Items.Count)
                {
                    lstAIPrograms.SelectedIndex++;
                }
                else if (lstAIPrograms.Items.Count > 0)
                {
                    lstAIPrograms.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstAIPrograms.SelectedIndex - 1 >= 0)
                {
                    lstAIPrograms.SelectedIndex--;
                }
                else if (lstAIPrograms.Items.Count > 0)
                {
                    lstAIPrograms.SelectedIndex = lstAIPrograms.Items.Count - 1;
                }
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
        public string SelectedProgram
        {
            get
            {
                return _strSelectedAIProgram;
            }
            set
            {
                _strSelectedAIProgram = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Update the Program's information based on the Program selected.
        /// </summary>
        private void UpdateProgramInfo(XmlNode objXmlProgram)
        {
            if (!string.IsNullOrEmpty(lstAIPrograms.Text))
            {
                string strRequiresProgram = LanguageManager.Instance.GetString("String_None");
                if (objXmlProgram["require"] != null)
                    strRequiresProgram = objXmlProgram["require"].InnerText;

                lblRequiresProgram.Text = strRequiresProgram;

                string strBook = _objCharacter.Options.LanguageBookShort(objXmlProgram["source"].InnerText);
                string strPage = objXmlProgram["page"].InnerText;
                if (objXmlProgram["altpage"] != null)
                    strPage = objXmlProgram["altpage"].InnerText;
                lblSource.Text = strBook + " " + strPage;

                tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlProgram["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
            }
        }

        /// <summary>
        /// Update the Program List based on a base program node list.
        /// </summary>
        private void UpdateProgramList(XmlNodeList objXmlNodeList)
        {
            List<ListItem> lstPrograms = new List<ListItem>();
            bool blnCheckForOptional = false;
            XmlNode objXmlCritter = null;
            XmlDocument objXmlCritterDocument = new XmlDocument();
            if (_objCharacter.IsCritter)
            {
                objXmlCritterDocument = XmlManager.Instance.Load("critters.xml");
                objXmlCritter = objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                if (objXmlCritter.InnerXml.Contains("<optionalaiprograms>"))
                    blnCheckForOptional = true;
            }

            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                if (objXmlProgram["hidden"] != null)
                    continue;
                bool blnAdd = true;
                if (chkLimitList.Checked && objXmlProgram["require"] != null)
                {
                    blnAdd = false;
                    foreach (AIProgram objAIProgram in _objCharacter.AIPrograms)
                    {
                        if (objAIProgram.Name == objXmlProgram["require"].InnerText)
                        {
                            blnAdd = true;
                            break;
                        }
                    }
                    if (!blnAdd)
                        continue;
                }
                // If this is a critter with Optional Programs, see if this Program is allowed.
                if (blnCheckForOptional)
                {
                    blnAdd = false;
                    foreach (XmlNode objXmlForm in objXmlCritter.SelectNodes("optionalaiprograms/program"))
                    {
                        if (objXmlForm.InnerText == objXmlProgram["name"].InnerText)
                        {
                            blnAdd = true;
                            break;
                        }
                    }
                    if (!blnAdd)
                        continue;
                }
                ListItem objItem = new ListItem();
                objItem.Value = objXmlProgram["id"].InnerText;
                objItem.Name = objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"].InnerText;
                if (!string.IsNullOrEmpty(txtSearch.Text) && objXmlProgram["category"] != null && objXmlProgram["category"].InnerText != cboCategory.SelectedValue.ToString())
                {
                    ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == objXmlProgram["category"].InnerText);
                    if (objFoundItem != null)
                    {
                        objItem.Name += " [" + objFoundItem.Name + "]";
                    }
                }
                lstPrograms.Add(objItem);
            }

            SortListItem objSort = new SortListItem();
            lstPrograms.Sort(objSort.Compare);
            lstAIPrograms.BeginUpdate();
            lstAIPrograms.DataSource = null;
            lstAIPrograms.ValueMember = "Value";
            lstAIPrograms.DisplayMember = "Name";
            lstAIPrograms.DataSource = lstPrograms;
            lstAIPrograms.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (!string.IsNullOrEmpty(lstAIPrograms.Text))
            {
                XmlNode objXmlProgram;

                if (lstAIPrograms.SelectedValue.ToString().Contains('^'))
                {
                    // If the SelectedValue contains ^, then it also includes the English Category name which needs to be extracted.
                    int intIndexOf = lstAIPrograms.SelectedValue.ToString().IndexOf('^');
                    string strValue = lstAIPrograms.SelectedValue.ToString().Substring(0, intIndexOf);
                    string strCategory = lstAIPrograms.SelectedValue.ToString().Substring(intIndexOf + 1, lstAIPrograms.SelectedValue.ToString().Length - intIndexOf - 1);
                    objXmlProgram = _objXmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + strValue + "\" and category = \"" + strCategory + "\"]");
                }
                else
                    objXmlProgram = _objXmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + lstAIPrograms.SelectedValue + "\"]");

                _strSelectedAIProgram = objXmlProgram["name"].InnerText;
                _strSelectedCategory = objXmlProgram["category"].InnerText;

                // Check to make sure requirement is met
                string strRequiresProgram = string.Empty;
                bool boolRequirementMet = true;
                if (objXmlProgram["require"] != null)
                {
                    strRequiresProgram = objXmlProgram["require"].InnerText;
                    boolRequirementMet = false;
                    foreach (AIProgram objLoopAIProgram in _objCharacter.AIPrograms)
                    {
                        if (objLoopAIProgram.Name == strRequiresProgram)
                        {
                            boolRequirementMet = true;
                            break;
                        }
                    }
                }
                if (boolRequirementMet)
                    DialogResult = DialogResult.OK;
                else
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectAIProgram_AdvancedProgramRequirement") + strRequiresProgram,
                        LanguageManager.Instance.GetString("MessageTitle_SelectAIProgram_AdvancedProgramRequirement"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        private void MoveControls()
        {
            int intLeft = lblRequiresProgramLabel.Width;
            intLeft = Math.Max(intLeft, lblSourceLabel.Width);

            lblRequiresProgram.Left = lblRequiresProgramLabel.Left + intLeft + 6;
            lblSource.Left = lblSourceLabel.Left + intLeft + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
