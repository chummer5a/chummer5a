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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectAIProgram : Form
    {
        private string _strSelectedAIProgram = string.Empty;
        private static string s_StrSelectedCategory = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain = false;
        private readonly bool _blnAdvancedProgramAllowed = true;
        private readonly bool _blnInherentProgram = false;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        private readonly XmlDocument _objXmlDocument = null;

        #region Control Events
        public frmSelectAIProgram(Character objCharacter, bool blnAdvancedProgramAllowed = true, bool blnInherentProgram = false)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _blnAdvancedProgramAllowed = blnAdvancedProgramAllowed;
            _blnInherentProgram = blnInherentProgram;
            MoveControls();
            // Load the Programs information.
            _objXmlDocument = XmlManager.Load("programs.xml");
        }

        private void frmSelectProgram_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            // Populate the Category list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlNodeList)
            {
                string strInnerText = objXmlCategory.InnerText;
                if (_blnInherentProgram && strInnerText != "Common Programs" && strInnerText != "Hacking Programs")
                    continue;
                if (!_blnAdvancedProgramAllowed && strInnerText == "Advanced Programs")
                    continue;
                // Make sure it is not already in the Category list.
                if (!_lstCategory.Any(objItem => objItem.Value.ToString() == strInnerText))
                {
                    _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            cboCategory.EndUpdate();
            
            if (!string.IsNullOrEmpty(s_StrSelectedCategory))
                cboCategory.SelectedValue = s_StrSelectedCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;

            _blnLoading = false;

            RefreshList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstAIPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProgramInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void trePrograms_DoubleClick(object sender, EventArgs e)
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
            RefreshList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstAIPrograms.SelectedIndex + 1 < lstAIPrograms.Items.Count)
                {
                    lstAIPrograms.SelectedIndex += 1;
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
                    lstAIPrograms.SelectedIndex -= 1;
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
        private void UpdateProgramInfo()
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstAIPrograms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                XmlNode objXmlProgram = _objXmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + strSelectedId + "\"]");
                if (objXmlProgram != null)
                {
                    string strRequiresProgram = objXmlProgram["require"]?.InnerText;
                    if (string.IsNullOrEmpty(strRequiresProgram))
                    {
                        strRequiresProgram = LanguageManager.GetString("String_None", GlobalOptions.Language);
                    }
                    else
                    {
                        strRequiresProgram = _objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + strRequiresProgram + "\"]/@translate")?.InnerText ?? strRequiresProgram;
                    }

                    lblRequiresProgram.Text = strRequiresProgram;

                    string strSource = objXmlProgram["source"]?.InnerText;
                    if (!string.IsNullOrEmpty(strSource))
                    {
                        string strPage = objXmlProgram["altpage"]?.InnerText ?? objXmlProgram["page"].InnerText;
                        if (!string.IsNullOrEmpty(strPage))
                        {
                            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;
                            tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + " " + strPage);
                        }
                        else
                        {
                            lblRequiresProgram.Text = string.Empty;
                            lblSource.Text = string.Empty;
                            tipTooltip.SetToolTip(lblSource, string.Empty);
                        }
                    }
                    else
                    {
                        lblRequiresProgram.Text = string.Empty;
                        lblSource.Text = string.Empty;
                        tipTooltip.SetToolTip(lblSource, string.Empty);
                    }
                }
                else
                {
                    lblRequiresProgram.Text = string.Empty;
                    lblSource.Text = string.Empty;
                    tipTooltip.SetToolTip(lblSource, string.Empty);
                }
            }
            else
            {
                lblRequiresProgram.Text = string.Empty;
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
            }
        }

        /// <summary>
        /// Refreshes the displayed lists
        /// </summary>
        private void RefreshList()
        {
            if (_blnLoading)
                return;

            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';

            string strCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + '\"';
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ')';
                }
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Populate the Program list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/programs/program[" + strFilter + ']');

            UpdateProgramList(objXmlNodeList);
        }

        /// <summary>
        /// Update the Program List based on a base program node list.
        /// </summary>
        private void UpdateProgramList(XmlNodeList objXmlNodeList)
        {
            List<ListItem> lstPrograms = new List<ListItem>();
            XmlNodeList xmlCritterOptionalPrograms = null;
            if (_objCharacter.IsCritter)
            {
                xmlCritterOptionalPrograms = XmlManager.Load("critters.xml").SelectNodes("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]/optionalaiprograms/program");
            }

            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                bool blnAdd = true;
                if (chkLimitList.Checked)
                {
                    string strRequire = objXmlProgram["require"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRequire))
                    {
                        blnAdd = false;
                        foreach (AIProgram objAIProgram in _objCharacter.AIPrograms)
                        {
                            if (objAIProgram.Name == strRequire)
                            {
                                blnAdd = true;
                                break;
                            }
                        }
                        if (!blnAdd)
                            continue;
                    }
                }
                string strName = objXmlProgram["name"].InnerText;
                // If this is a critter with Optional Programs, see if this Program is allowed.
                if (xmlCritterOptionalPrograms?.Count > 0)
                {
                    blnAdd = false;
                    foreach (XmlNode objXmlForm in xmlCritterOptionalPrograms)
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
                string strDisplayName = objXmlProgram["translate"]?.InnerText ?? strName;
                if (!_objCharacter.Options.SearchInCategoryOnly && txtSearch.TextLength != 0)
                {
                    string strCategory = objXmlProgram["category"]?.InnerText;
                    if (!string.IsNullOrEmpty(strCategory))
                    {
                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += " [" + objFoundItem.Name + "]";
                        }
                    }
                }
                lstPrograms.Add(new ListItem(objXmlProgram["id"].InnerText, strDisplayName));
            }
            
            lstPrograms.Sort(CompareListItems.CompareNames);
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
            string strSelectedId = lstAIPrograms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode xmlProgram = _objXmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + strSelectedId + "\"]");

                // Check to make sure requirement is met
                string strRequiresProgram = xmlProgram["require"]?.InnerText;
                if (!string.IsNullOrEmpty(strRequiresProgram))
                {
                    bool blnRequirementsMet = false;
                    foreach (AIProgram objLoopAIProgram in _objCharacter.AIPrograms)
                    {
                        if (objLoopAIProgram.Name == strRequiresProgram)
                        {
                            blnRequirementsMet = true;
                            break;
                        }
                    }
                    if (!blnRequirementsMet)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_SelectAIProgram_AdvancedProgramRequirement", GlobalOptions.Language) + strRequiresProgram,
                            LanguageManager.GetString("MessageTitle_SelectAIProgram_AdvancedProgramRequirement", GlobalOptions.Language),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                _strSelectedAIProgram = strSelectedId;
                s_StrSelectedCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlProgram["category"].InnerText;

                DialogResult = DialogResult.OK;
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
    }
}
