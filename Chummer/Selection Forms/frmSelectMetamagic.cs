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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    // TODO: Review naming schema
    // ReSharper disable once InconsistentNaming
    public partial class frmSelectMetamagic : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedMetamagic = string.Empty;

        private readonly string _strType = string.Empty;
        private readonly string _strRootXPath = "/chummer/metamagics/metamagic";

        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument;

        private readonly List<KeyValuePair<string, int>> _lstMetamagicLimits = new List<KeyValuePair<string, int>>();
        private readonly Mode _objMode;

        public enum Mode
        {
            Metamagic = 0,
            Echo,
        }

        #region Control Events
        public frmSelectMetamagic(Character objCharacter, Mode objMode)
        {
            InitializeComponent();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objMode = objMode;

            if (_objCharacter.Improvements.Any(imp =>
                imp.ImproveType == Improvement.ImprovementType.MetamagicLimit))
            {
                foreach (Improvement imp in _objCharacter.Improvements.Where(imp =>
                    imp.ImproveType == Improvement.ImprovementType.MetamagicLimit && imp.Enabled))
                {
                    _lstMetamagicLimits.Add(new KeyValuePair<string, int>(imp.ImprovedName, imp.Rating));
                }
            }

            // Load the Metamagic information.
            switch (objMode)
            {
                case Mode.Metamagic:
                    _strRootXPath = "/chummer/metamagics/metamagic";
                    _objXmlDocument = XmlManager.Load("metamagic.xml");
                    _strType = LanguageManager.GetString("String_Metamagic");
                    break;
                case Mode.Echo:
                    _strRootXPath = "/chummer/echoes/echo";
                    _objXmlDocument = XmlManager.Load("echoes.xml");
                    _strType = LanguageManager.GetString("String_Echo");
                    break;
            }
        }

        private void frmSelectMetamagic_Load(object sender, EventArgs e)
        {
            Text = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Title_SelectGeneric"), _strType);
            chkLimitList.Text = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Checkbox_SelectGeneric_LimitList"), _strType);

            _blnLoading = false;
            BuildMetamagicList();
        }

        private void lstMetamagic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstMetamagic.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected piece of Cyberware.
                XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strRootXPath + "[id = \"" + strSelectedId + "\"]");

                if (objXmlMetamagic != null)
                {
                    string strSource = objXmlMetamagic["source"]?.InnerText;
                    string strPage = objXmlMetamagic["altpage"]?.InnerText ?? objXmlMetamagic["page"]?.InnerText;
                    string strSpace = LanguageManager.GetString("String_Space");
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource) + strSpace + strPage;
                    lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
                }
                else
                {
                    lblSource.Text = string.Empty;
                    lblSource.SetToolTip(string.Empty);
                }
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstMetamagic_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildMetamagicList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildMetamagicList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Id of Metamagic that was selected in the dialogue.
        /// </summary>
        public string SelectedMetamagic => _strSelectedMetamagic;

        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Metamagics.
        /// </summary>
        private void BuildMetamagicList()
        {
            string strFilter = _objCharacter.Options.BookXPath();
            // If the character has MAG enabled, filter the list based on Adept/Magician availability.
            if (_objCharacter.MAGEnabled)
            {
                bool blnIsMagician = _objCharacter.MagicianEnabled;
                if (blnIsMagician != _objCharacter.AdeptEnabled)
                {
                    if (blnIsMagician)
                        strFilter = "magician = 'True' and (" + strFilter + ')';
                    else
                        strFilter = "adept = 'True' and (" + strFilter + ')';
                }
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);
            List<ListItem> lstMetamagics = new List<ListItem>();
            using (XmlNodeList objXmlMetamagicList = _objXmlDocument.SelectNodes(_strRootXPath + '[' + strFilter + ']'))
                if (objXmlMetamagicList?.Count > 0)
                    foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
                    {
                        string strId = objXmlMetamagic["id"]?.InnerText;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            if (_lstMetamagicLimits.Count > 0 && (_objMode == Mode.Metamagic && !_lstMetamagicLimits.Any(item => item.Key == objXmlMetamagic["name"]?.InnerText && (item.Value == -1 || item.Value == _objCharacter.InitiateGrade)) ||
                                                                  _objMode == Mode.Echo && !_lstMetamagicLimits.Any(item => item.Key == objXmlMetamagic["name"]?.InnerText && (item.Value == -1 || item.Value == _objCharacter.SubmersionGrade))))
                            {
                                continue;
                            }
                            if (!chkLimitList.Checked || objXmlMetamagic.CreateNavigator().RequirementsMet(_objCharacter))
                            {
                                lstMetamagics.Add(new ListItem(strId,
                                    objXmlMetamagic["translate"]?.InnerText ?? objXmlMetamagic["name"]?.InnerText ??
                                    LanguageManager.GetString("String_Unknown")));
                            }
                        }
                    }
            lstMetamagics.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstMetamagic.SelectedValue?.ToString();
            _blnLoading = true;
            lstMetamagic.BeginUpdate();
            lstMetamagic.ValueMember = "Value";
            lstMetamagic.DisplayMember = "Name";
            lstMetamagic.DataSource = lstMetamagics;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstMetamagic.SelectedValue = strOldSelected;
            else
                lstMetamagic.SelectedIndex = -1;
            lstMetamagic.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstMetamagic.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Make sure the selected Metamagic or Echo meets its requirements.
                XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strRootXPath + "[id = \"" + strSelectedId + "\"]");

                if (objXmlMetamagic?.CreateNavigator().RequirementsMet(_objCharacter, _strType) != true)
                    return;

                _strSelectedMetamagic = strSelectedId;

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
