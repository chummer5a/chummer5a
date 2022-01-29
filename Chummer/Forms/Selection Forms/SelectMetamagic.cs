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
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectMetamagic : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedMetamagic = string.Empty;

        private readonly string _strType;
        private readonly string _strRootXPath;

        private readonly Character _objCharacter;

        private readonly XPathNavigator _objXmlDocument;

        private readonly List<string> _lstMetamagicLimits = new List<string>();

        #region Control Events

        public SelectMetamagic(Character objCharacter, InitiationGrade objGrade)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            if (objGrade == null)
                throw new ArgumentNullException(nameof(objGrade));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            int intMyGrade = objGrade.Grade;
            foreach (Improvement imp in ImprovementManager
                                        .GetCachedImprovementListForValueOf(
                                            _objCharacter, Improvement.ImprovementType.MetamagicLimit)
                                        .Where(imp => imp.Rating == intMyGrade))
            {
                _lstMetamagicLimits.Add(imp.ImprovedName);
            }

            // Load the Metamagic information.
            if (objGrade.Technomancer)
            {
                _strRootXPath = "/chummer/echoes/echo";
                _objXmlDocument = _objCharacter.LoadDataXPath("echoes.xml");
                _strType = LanguageManager.GetString("String_Echo");
            }
            else
            {
                _strRootXPath = "/chummer/metamagics/metamagic";
                _objXmlDocument = _objCharacter.LoadDataXPath("metamagic.xml");
                _strType = LanguageManager.GetString("String_Metamagic");
            }
        }

        private void SelectMetamagic_Load(object sender, EventArgs e)
        {
            Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Title_SelectGeneric"), _strType);
            chkLimitList.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Checkbox_SelectGeneric_LimitList"), _strType);

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
                XPathNavigator objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strRootXPath + "[id = " + strSelectedId.CleanXPath() + ']');

                if (objXmlMetamagic != null)
                {
                    string strSource = objXmlMetamagic.SelectSingleNode("source")?.Value;
                    string strPage = objXmlMetamagic.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetamagic.SelectSingleNode("page")?.Value;
                    SourceString objSourceString = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                    objSourceString.SetControl(lblSource);
                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                    tlpRight.Visible = true;
                }
                else
                {
                    tlpRight.Visible = false;
                }
            }
            else
            {
                tlpRight.Visible = false;
            }
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

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Id of Metamagic that was selected in the dialogue.
        /// </summary>
        public string SelectedMetamagic => _strSelectedMetamagic;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Build the list of Metamagics.
        /// </summary>
        private void BuildMetamagicList()
        {
            string strFilter = '(' + _objCharacter.Settings.BookXPath() + ')';
            // If the character has MAG enabled, filter the list based on Adept/Magician availability.
            if (_objCharacter.MAGEnabled)
            {
                bool blnIsMagician = _objCharacter.MagicianEnabled;
                if (blnIsMagician != _objCharacter.AdeptEnabled)
                {
                    if (blnIsMagician)
                        strFilter += "and magician = " + bool.TrueString.CleanXPath();
                    else
                        strFilter += "and adept = " + bool.TrueString.CleanXPath();
                }
            }

            if (_lstMetamagicLimits.Count > 0)
            {
                strFilter += " and (";
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    foreach (string strMetamagic in _lstMetamagicLimits)
                        sbdFilter.Append("name = ").Append(strMetamagic.CleanXPath()).Append(" or ");
                    sbdFilter.Length -= 4;
                    strFilter += sbdFilter.ToString() + ')';
                }
            }

            if (!string.IsNullOrEmpty(txtSearch.Text))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text);
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetamagics))
            {
                foreach (XPathNavigator objXmlMetamagic in
                         _objXmlDocument.Select(_strRootXPath + '[' + strFilter + ']'))
                {
                    string strId = objXmlMetamagic.SelectSingleNodeAndCacheExpression("id")?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;
                    if (!chkLimitList.Checked || objXmlMetamagic.CreateNavigator().RequirementsMet(_objCharacter))
                    {
                        lstMetamagics.Add(new ListItem(strId,
                                                       objXmlMetamagic.SelectSingleNodeAndCacheExpression("translate")
                                                                      ?.Value ?? objXmlMetamagic
                                                           .SelectSingleNodeAndCacheExpression("name")?.Value ??
                                                       LanguageManager.GetString("String_Unknown")));
                    }
                }

                lstMetamagics.Sort(CompareListItems.CompareNames);
                string strOldSelected = lstMetamagic.SelectedValue?.ToString();
                _blnLoading = true;
                lstMetamagic.BeginUpdate();
                lstMetamagic.PopulateWithListItems(lstMetamagics);
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    lstMetamagic.SelectedValue = strOldSelected;
                else
                    lstMetamagic.SelectedIndex = -1;
                lstMetamagic.EndUpdate();
            }
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
                XPathNavigator objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strRootXPath + "[id = " + strSelectedId.CleanXPath() + ']');

                if (objXmlMetamagic?.RequirementsMet(_objCharacter, _strType) != true)
                    return;

                _strSelectedMetamagic = strSelectedId;

                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
