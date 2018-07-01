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
using System.Xml;

namespace Chummer
{
    public partial class frmSelectProgramOption : Form
    {
        private string _strSelectedOption = string.Empty;
        private string _strProgramName = string.Empty;
        private string _strProgramCategory = string.Empty;
        private readonly List<string> _lstTags = new List<string>();

        private bool _blnAddAgain;

        private readonly XmlDocument _objXmlDocument;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectProgramOption(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Programs information.
            _objXmlDocument = XmlManager.Load("complexforms.xml");
        }

        private void frmSelectProgramOption_Load(object sender, EventArgs e)
        {
            List<ListItem> lstOption = new List<ListItem>();

            // Populate the Program list.
            XmlNodeList objXmlOptionList = _objXmlDocument.SelectNodes("/chummer/options/option[" + _objCharacter.Options.BookXPath() + "]");

            foreach (XmlNode objXmlOption in objXmlOptionList)
            {
                // If the Option has Category requirements, make sure they are met before adding the item to the list.
                if (objXmlOption["programtypes"] != null)
                {
                    bool blnAdd = false;
                    foreach (XmlNode objXmlCategory in objXmlOption.SelectNodes("programtypes/programtype"))
                    {
                        if (objXmlCategory.InnerText == _strProgramCategory)
                            blnAdd = true;
                    }
                    if (!blnAdd)
                        continue;
                }

                string strName = objXmlOption["name"].InnerText;
                lstOption.Add(new ListItem(strName, objXmlOption["translate"]?.InnerText ?? strName));
            }
            lstOption.Sort(CompareListItems.CompareNames);
            lstOptions.BeginUpdate();
            lstOptions.ValueMember = "Value";
            lstOptions.DisplayMember = "Name";
            lstOptions.DataSource = lstOption;
            lstOptions.EndUpdate();
        }

        private void lstOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedId = lstOptions.SelectedValue?.ToString();
            XmlNode xmlOption = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlOption = _objXmlDocument.SelectSingleNode("/chummer/options/option[name = \"" + strSelectedId + "\"]");

            if (xmlOption != null)
            {
                string strSource = xmlOption["source"].InnerText;
                string strPage = xmlOption["altpage"]?.InnerText ?? xmlOption["page"].InnerText;
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            }
            else
            {
                lblSource.Text = string.Empty;

                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, string.Empty);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstOptions.Text))
            {
                _blnAddAgain = false;
                AcceptForm();
            }
        }

        private void lstOptions_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstOptions.Text))
            {
                _blnAddAgain = false;
                AcceptForm();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstOptions.Text))
            {
                _blnAddAgain = true;
                AcceptForm();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Name of the Program the Option will be added to.
        /// </summary>
        public string ProgramName
        {
            set => _strProgramName = value;
        }

        /// <summary>
        /// Category of the Program the Option will be added to.
        /// </summary>
        public string ProgramCategory
        {
            set => _strProgramCategory = value;
        }

        /// <summary>
        /// Tags associated with the Program.
        /// </summary>
        public IList<string> ProgramTags => _lstTags;

        /// <summary>
        /// Program Option that was selected in the dialogue.
        /// </summary>
        public string SelectedOption => _strSelectedOption;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedOption = lstOptions.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            lblCommonSkill.Left = lblCommonSkillLabel.Left + lblCommonSkillLabel.Width + 6;
            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
        }
        #endregion
    }
}
