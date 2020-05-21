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
 using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectComplexForm : Form
    {
        private string _strSelectedComplexForm = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBaseComplexFormsNode;
        private readonly XPathNavigator _xmlOptionalComplexFormNode;

        //private bool _blnBiowireEnabled = false;

        #region Control Events
        public frmSelectComplexForm(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            // Load the Complex Form information.
            _xmlBaseComplexFormsNode = XmlManager.Load("complexforms.xml").GetFastNavigator().SelectSingleNode("/chummer/complexforms");

            _xmlOptionalComplexFormNode = _objCharacter.GetNode();
            if (_xmlOptionalComplexFormNode == null) return;
            if (_objCharacter.MetavariantGuid != Guid.Empty)
            {
                XPathNavigator xmlMetavariantNode = _xmlOptionalComplexFormNode.SelectSingleNode($"metavariants/metavariant[id = \"{_objCharacter.MetavariantGuid}\"]");
                if (xmlMetavariantNode != null)
                    _xmlOptionalComplexFormNode = xmlMetavariantNode;
            }

            _xmlOptionalComplexFormNode = _xmlOptionalComplexFormNode.SelectSingleNode("optionalcomplexforms");
        }

        private void frmSelectComplexForm_Load(object sender, EventArgs e)
        {
            _blnLoading = false;
            BuildComplexFormList();
        }

        private void lstComplexForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedComplexFormId = lstComplexForms.SelectedValue?.ToString();
            if (_blnLoading || string.IsNullOrEmpty(strSelectedComplexFormId))
            {
                lblDuration.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblFV.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                return;
            }

            // Display the Complex Form information.
            XPathNavigator xmlComplexForm = _xmlBaseComplexFormsNode.SelectSingleNode("complexform[id = \"" + strSelectedComplexFormId + "\"]");
            if (xmlComplexForm != null)
            {
                switch (xmlComplexForm.SelectSingleNode("duration")?.Value)
                {
                    case "P":
                        lblDuration.Text = LanguageManager.GetString("String_SpellDurationPermanent", GlobalOptions.Language);
                        break;
                    case "S":
                        lblDuration.Text = LanguageManager.GetString("String_SpellDurationSustained", GlobalOptions.Language);
                        break;
                    case "Special":
                        lblDuration.Text = LanguageManager.GetString("String_SpellDurationSpecial", GlobalOptions.Language);
                        break;
                    default:
                        lblDuration.Text = LanguageManager.GetString("String_SpellDurationInstant", GlobalOptions.Language);
                        break;
                }

                switch (xmlComplexForm.SelectSingleNode("target")?.Value)
                {
                    case "Persona":
                        lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetPersona", GlobalOptions.Language);
                        break;
                    case "Device":
                        lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetDevice", GlobalOptions.Language);
                        break;
                    case "File":
                        lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetFile", GlobalOptions.Language);
                        break;
                    case "Self":
                        lblTarget.Text = LanguageManager.GetString("String_SpellRangeSelf", GlobalOptions.Language);
                        break;
                    case "Sprite":
                        lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetSprite", GlobalOptions.Language);
                        break;
                    case "Host":
                        lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetHost", GlobalOptions.Language);
                        break;
                    case "IC":
                        lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetIC", GlobalOptions.Language);
                        break;
                    default:
                        lblTarget.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                        break;
                }

                string strFV = xmlComplexForm.SelectSingleNode("fv")?.Value.Replace('/', '÷') ?? string.Empty;
                if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    strFV = strFV.CheapReplace("L", () => LanguageManager.GetString("String_ComplexFormLevel", GlobalOptions.Language))
                        .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", GlobalOptions.Language))
                        .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", GlobalOptions.Language))
                        .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", GlobalOptions.Language))
                        .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", GlobalOptions.Language))
                        .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", GlobalOptions.Language));
                }

                lblFV.Text = strFV;

                string strSource = xmlComplexForm.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strPage = xmlComplexForm.SelectSingleNode("altpage")?.Value ?? xmlComplexForm.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;

                lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            }
            else
            {
                lblDuration.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblFV.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblDurationLabel.Visible = !string.IsNullOrEmpty(lblDuration.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            lblFVLabel.Visible = !string.IsNullOrEmpty(lblFV.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void lstComplexForms_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildComplexFormList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstComplexForms.SelectedIndex == -1)
            {
                if (lstComplexForms.Items.Count > 0)
                    lstComplexForms.SelectedIndex = 0;
            }
            if (e.KeyCode == Keys.Down)
            {
                int intNewIndex = lstComplexForms.SelectedIndex + 1;
                if (intNewIndex >= lstComplexForms.Items.Count)
                    intNewIndex = 0;
                if (lstComplexForms.Items.Count > 0)
                    lstComplexForms.SelectedIndex = intNewIndex;
            }
            if (e.KeyCode == Keys.Up)
            {
                int intNewIndex = lstComplexForms.SelectedIndex - 1;
                if (intNewIndex <= 0)
                    intNewIndex = lstComplexForms.Items.Count - 1;
                if (lstComplexForms.Items.Count > 0)
                    lstComplexForms.SelectedIndex = intNewIndex;
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
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Complex Form that was selected in the dialogue.
        /// </summary>
        public string SelectedComplexForm => _strSelectedComplexForm;

        #endregion

        #region Methods
        private void BuildComplexFormList()
        {
            if (_blnLoading)
                return;

            string strFilter = "(" + _objCharacter.Options.BookXPath() + ')';

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Populate the Complex Form list.
            List<ListItem> lstComplexFormItems = new List<ListItem>();
            foreach (XPathNavigator xmlComplexForm in _xmlBaseComplexFormsNode.Select("complexform[" + strFilter + ']'))
            {
                string strId = xmlComplexForm.SelectSingleNode("id")?.Value;
                if (string.IsNullOrEmpty(strId))
                    continue;

                if (!xmlComplexForm.RequirementsMet(_objCharacter))
                    continue;

                string strName = xmlComplexForm.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                // If this is a Sprite with Optional Complex Forms, see if this Complex Form is allowed.
                if (_xmlOptionalComplexFormNode?.SelectSingleNode("complexform") != null)
                {
                    if (_xmlOptionalComplexFormNode.SelectSingleNode("complexform[text() = \"" + strName + "\"]") == null)
                        continue;
                }

                lstComplexFormItems.Add(new ListItem(strId, xmlComplexForm.SelectSingleNode("translate")?.Value ?? strName));
            }

            lstComplexFormItems.Sort(CompareListItems.CompareNames);
            _blnLoading = true;
            string strOldSelected = lstComplexForms.SelectedValue?.ToString();
            lstComplexForms.BeginUpdate();
            lstComplexForms.ValueMember = "Value";
            lstComplexForms.DisplayMember = "Name";
            lstComplexForms.DataSource = lstComplexFormItems;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstComplexForms.SelectedValue = strOldSelected;
            else
                lstComplexForms.SelectedIndex = -1;
            lstComplexForms.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstComplexForms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                _strSelectedComplexForm = strSelectedItem;
                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
