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
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectMartialArtTechnique : Form
    {
        private string _strSelectedTechnique = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;

        private readonly MartialArt _objMartialArt;
        private readonly XPathNavigator _xmlBaseChummerNode;
        private readonly Character _objCharacter;
        private readonly HashSet<string> _setAllowedTechniques = new HashSet<string>();

        #region Control Events
        public frmSelectMartialArtTechnique(Character objCharacter, MartialArt objMartialArt)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objMartialArt = objMartialArt ?? throw new ArgumentNullException(nameof(objMartialArt));
            // Load the Martial Art information.
            _xmlBaseChummerNode = XmlManager.Load("martialarts.xml").GetFastNavigator().SelectSingleNode("/chummer");
            // Populate the Martial Art Tecnnique list.
            XPathNavigator xmlMartialArtNode = _xmlBaseChummerNode?.SelectSingleNode("martialarts/martialart[name = \"" + _objMartialArt.Name + "\"]");
            if (xmlMartialArtNode != null)
            {
                if (!xmlMartialArtNode.NodeExists("alltechniques"))
                {
                    foreach (XPathNavigator xmlTechnique in xmlMartialArtNode.Select("techniques/technique"))
                    {
                        string strTechniqueName = xmlTechnique.Value;
                        if (_objMartialArt.Techniques.All(x => x.Name != strTechniqueName))
                        {
                            _setAllowedTechniques.Add(strTechniqueName);
                        }
                    }
                }
                else if (_objMartialArt.Techniques.Count == 0)
                {
                    //TODO: Support for allowing all techniques  > 0.
                    string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
                    XPathNodeIterator objTechniquesList = _xmlBaseChummerNode.Select("techniques/technique[" + strFilter + "]");

                    foreach (XPathNavigator xmlTechnique in objTechniquesList)
                    {
                        if (_objMartialArt.Techniques.Any(x => x.Name == xmlTechnique.Value) || xmlTechnique.SelectSingleNode("name") == null) continue;
                            _setAllowedTechniques.Add(xmlTechnique.SelectSingleNode("name")?.Value);
                    }
                }
            }

        }

        private void frmSelectMartialArtTechnique_Load(object sender, EventArgs e)
        {
            _blnLoading = false;
            RefreshTechniquesList();
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

        private void lstTechniques_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void lstTechniques_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstTechniques.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XPathNavigator xmlTechnique = _xmlBaseChummerNode.SelectSingleNode("/chummer/techniques/technique[id = \"" + strSelectedId + "\"]");

                if (xmlTechnique != null)
                {
                    string strSource = xmlTechnique.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strPage = xmlTechnique.SelectSingleNode("altpage")?.Value ?? xmlTechnique.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                    lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                }
                else
                {
                    lblSource.Text = string.Empty;
                    lblSource.SetToolTip(string.Empty);
                    lblSourceLabel.Visible = false;
                }
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                lblSourceLabel.Visible = false;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshTechniquesList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Martial Art Technique that was selected in the dialogue.
        /// </summary>
        public string SelectedTechnique => _strSelectedTechnique;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstTechniques.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedTechnique = strSelectedId;
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Populate the Martial Arts Techniques list.
        /// </summary>
        private void RefreshTechniquesList()
        {
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')' + CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            XPathNodeIterator objTechniquesList = _xmlBaseChummerNode.Select("techniques/technique[" + strFilter + "]");

            List<ListItem> lstTechniqueItems = new List<ListItem>();
            foreach (XPathNavigator xmlTechnique in objTechniquesList)
            {
                string strId = xmlTechnique.SelectSingleNode("id")?.Value;
                if (!string.IsNullOrEmpty(strId))
                {
                    string strTechniqueName = xmlTechnique.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);

                    if (_setAllowedTechniques?.Contains(strTechniqueName) == false)
                        continue;

                    if (xmlTechnique.RequirementsMet(_objCharacter, _objMartialArt))
                    {
                        lstTechniqueItems.Add(new ListItem(strId, xmlTechnique.SelectSingleNode("translate")?.Value ?? strTechniqueName));
                    }
                }
            }
            lstTechniqueItems.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstTechniques.SelectedValue?.ToString();
            _blnLoading = true;
            lstTechniques.BeginUpdate();
            lstTechniques.ValueMember = "Value";
            lstTechniques.DisplayMember = "Name";
            lstTechniques.DataSource = lstTechniqueItems;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstTechniques.SelectedValue = strOldSelected;
            else
                lstTechniques.SelectedIndex = -1;
            lstTechniques.EndUpdate();
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
