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
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectMartialArt : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedMartialArt = string.Empty;

        private bool _blnAddAgain;
        private string _strForcedValue = string.Empty;

        private readonly XPathNavigator _xmlBaseMartialArtsNode;
        private readonly XPathNavigator _xmlBaseMartialArtsTechniquesNode;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArt(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;

            // Load the Martial Arts information.
            XPathNavigator xmlBaseMartialArtsDocumentNode = XmlManager.Load("martialarts.xml").GetFastNavigator();
            _xmlBaseMartialArtsNode = xmlBaseMartialArtsDocumentNode.SelectSingleNode("/chummer/martialarts");
            _xmlBaseMartialArtsTechniquesNode = xmlBaseMartialArtsDocumentNode.SelectSingleNode("/chummer/techniques");
        }

        private void frmSelectMartialArt_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_strForcedValue))
            {
                _blnAddAgain = false;
                string strSelectedId = _xmlBaseMartialArtsNode.SelectSingleNode("martialart[name = \"" + _strForcedValue + "\"]/id")?.Value;
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    _strSelectedMartialArt = strSelectedId;
                    DialogResult = DialogResult.OK;
                }
            }

            _blnLoading = false;

            RefreshArtList();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstMartialArts_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
        }

        private void lstMartialArts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstMartialArts.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Populate the Martial Arts list.
                XPathNavigator objXmlArt = _xmlBaseMartialArtsNode.SelectSingleNode("martialart[id = \"" + strSelectedId + "\"]");

                if (objXmlArt != null)
                {
                    lblKarmaCost.Text = objXmlArt.SelectSingleNode("cost")?.Value ?? 7.ToString(GlobalOptions.CultureInfo);
                    lblKarmaCostLabel.Visible = !string.IsNullOrEmpty(lblKarmaCost.Text);

                    StringBuilder objTechniqueStringBuilder = new StringBuilder();
                    foreach (XPathNavigator xmlMartialArtsTechnique in objXmlArt.Select("techniques/technique"))
                    {
                        string strLoopTechniqueName = xmlMartialArtsTechnique.SelectSingleNode("name")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strLoopTechniqueName))
                        {
                            XPathNavigator xmlTechniqueNode = _xmlBaseMartialArtsTechniquesNode.SelectSingleNode("technique[name = \"" + strLoopTechniqueName + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
                            if (xmlTechniqueNode != null)
                            {
                                if (objTechniqueStringBuilder.Length > 0)
                                    objTechniqueStringBuilder.AppendLine(",");
                                
                                objTechniqueStringBuilder.Append(GlobalOptions.Language != GlobalOptions.DefaultLanguage ? xmlTechniqueNode.SelectSingleNode("translate")?.Value ?? strLoopTechniqueName: strLoopTechniqueName);
                            }
                        }
                    }
                    lblIncludedTechniques.Text = objTechniqueStringBuilder.ToString();
                    lblIncludedTechniquesLabel.Visible = !string.IsNullOrEmpty(lblIncludedTechniques.Text);

                    string strSource = objXmlArt.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strPage = objXmlArt.SelectSingleNode("altpage")?.Value ?? objXmlArt.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                    lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                }
                else
                {
                    lblKarmaCostLabel.Visible = false;
                    lblKarmaCost.Text = string.Empty;
                    lblIncludedTechniquesLabel.Visible = false;
                    lblIncludedTechniques.Text = string.Empty;
                    lblSourceLabel.Visible = false;
                    lblSource.Text = string.Empty;
                    lblSource.SetToolTip(string.Empty);
                }
            }
            else
            {
                lblKarmaCostLabel.Visible = false;
                lblKarmaCost.Text = string.Empty;
                lblIncludedTechniquesLabel.Visible = false;
                lblIncludedTechniques.Text = string.Empty;
                lblSourceLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshArtList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Martial Art that was selected in the dialogue.
        /// </summary>
        public string SelectedMartialArt => _strSelectedMartialArt;

        /// <summary>
        /// Only show Martial Arts that are provided by a quality
        /// </summary>
        public bool ShowQualities { get; set; }

        /// <summary>
        /// Force a Martial Art to be selected.
        /// </summary>
        public string ForcedValue
        {
            set => _strForcedValue = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstMartialArts.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedMartialArt = lstMartialArts.SelectedValue.ToString();
                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }

        /// <summary>
        /// Populate the Martial Arts list.
        /// </summary>
        private void RefreshArtList()
        {
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (ShowQualities)
                strFilter += " and isquality = \"" + bool.TrueString + "\"";
            else
                strFilter += " and not(isquality = \"" + bool.TrueString + "\")";
            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            XPathNodeIterator objArtList = _xmlBaseMartialArtsNode.Select("martialart[" + strFilter + "]");
            
            List<ListItem> lstMartialArt = new List<ListItem>();
            foreach (XPathNavigator objXmlArt in objArtList)
            {
                string strId = objXmlArt.SelectSingleNode("id")?.Value;
                if (!string.IsNullOrEmpty(strId) && objXmlArt.RequirementsMet(_objCharacter))
                {
                    lstMartialArt.Add(new ListItem(strId, objXmlArt.SelectSingleNode("translate")?.Value ?? objXmlArt.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language)));
                }
            }
            lstMartialArt.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstMartialArts.SelectedValue?.ToString();
            _blnLoading = true;
            lstMartialArts.BeginUpdate();
            lstMartialArts.ValueMember = "Value";
            lstMartialArts.DisplayMember = "Name";
            lstMartialArts.DataSource = lstMartialArt;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstMartialArts.SelectedValue = strOldSelected;
            else
                lstMartialArts.SelectedIndex = -1;
            lstMartialArts.EndUpdate();
        }
        #endregion
    }
}
