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
    public partial class frmSelectArt : Form
    {
        private string _strSelectedItem = string.Empty;

        private bool _blnLoading = true;
        private readonly string _strBaseXPath = "art";
        private readonly string _strXPathFilter = string.Empty;
        private readonly string _strLocalName = string.Empty;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _objXmlDocument;

        public enum Mode
        {
            Art = 0,
            Enhancement,
            Enchantment,
            Ritual,
        }

        public frmSelectArt(Character objCharacter, Mode objWindowMode)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;

            // Load the Metamagic information.
            switch (objWindowMode)
            {
                case Mode.Art:
                    _objXmlDocument = XmlManager.Load("metamagic.xml").GetFastNavigator().SelectSingleNode("/chummer/arts");
                    _strLocalName = LanguageManager.GetString("String_Art", GlobalOptions.Language);
                    _strBaseXPath = "art";
                    _strXPathFilter = _objCharacter.Options.BookXPath();
                    break;
                case Mode.Enhancement:
                    _objXmlDocument = XmlManager.Load("powers.xml").GetFastNavigator().SelectSingleNode("/chummer/enhancements");
                    _strLocalName = LanguageManager.GetString("String_Enhancement", GlobalOptions.Language);
                    _strBaseXPath = "enhancement";
                    _strXPathFilter = _objCharacter.Options.BookXPath();
                    break;
                case Mode.Enchantment:
                    _strLocalName = LanguageManager.GetString("String_Enchantment", GlobalOptions.Language);
                    _objXmlDocument = XmlManager.Load("spells.xml").GetFastNavigator().SelectSingleNode("/chummer/spells");
                    _strBaseXPath = "spell";
                    _strXPathFilter = "category = 'Enchantments' and (" + _objCharacter.Options.BookXPath() + ")";
                    break;
                case Mode.Ritual:
                    _strLocalName = LanguageManager.GetString("String_Ritual", GlobalOptions.Language);
                    _objXmlDocument = XmlManager.Load("spells.xml").GetFastNavigator().SelectSingleNode("/chummer/spells");
                    _strBaseXPath = "spell";
                    _strXPathFilter = "category = 'Rituals' and (" + _objCharacter.Options.BookXPath() + ")";
                    break;
            }
        }

        private void frmSelectArt_Load(object sender, EventArgs e)
        {
            Text = LanguageManager.GetString("Title_SelectGeneric", GlobalOptions.Language).Replace("{0}", _strLocalName);
            chkLimitList.Text = LanguageManager.GetString("Checkbox_SelectGeneric_LimitList", GlobalOptions.Language).Replace("{0}", _strLocalName);

            _blnLoading = false;

            BuildList();
        }

        private void lstArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelected = lstArt.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelected))
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                return;
            }

            // Retrieve the information for the selected art
            XPathNavigator objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strBaseXPath + "[id = \"" + strSelected + "\"]");

            if (objXmlMetamagic == null)
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                return;
            }

            string strSource = objXmlMetamagic.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objXmlMetamagic.SelectSingleNode("altpage")?.Value ?? objXmlMetamagic.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstArt_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildList();
        }

        #region Properties
        /// <summary>
        /// Id of the Art that was selected in the dialogue.
        /// </summary>
        public string SelectedItem => _strSelectedItem;

        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Arts.
        /// </summary>
        private void BuildList()
        {
            if (_blnLoading)
                return;

            List<ListItem> lstArts = new List<ListItem>();
            foreach (XPathNavigator objXmlMetamagic in _objXmlDocument.Select(_strBaseXPath + '[' + _strXPathFilter + CommonFunctions.GenerateSearchXPath(txtSearch.Text) + ']'))
            {
                string strId = objXmlMetamagic.SelectSingleNode("id")?.Value;
                if (!string.IsNullOrEmpty(strId))
                {
                    if (!chkLimitList.Checked || objXmlMetamagic.RequirementsMet(_objCharacter))
                    {
                        lstArts.Add(new ListItem(objXmlMetamagic.SelectSingleNode("id")?.Value, objXmlMetamagic.SelectSingleNode("translate")?.Value ?? objXmlMetamagic.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language)));
                    }
                }
            }
            lstArts.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstArt.SelectedValue?.ToString();
            _blnLoading = true;
            lstArt.BeginUpdate();
            lstArt.ValueMember = "Value";
            lstArt.DisplayMember = "Name";
            lstArt.DataSource = lstArts;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstArt.SelectedValue = strOldSelected;
            else
                lstArt.SelectedIndex = -1;
            lstArt.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstArt.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                // Make sure the selected Metamagic or Echo meets its requirements.
                XPathNavigator objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strBaseXPath + "[id = \"" + strSelectedItem + "\"]");

                if (objXmlMetamagic != null)
                {
                    if (!objXmlMetamagic.RequirementsMet(_objCharacter, null, _strLocalName))
                        return;

                    _strSelectedItem = strSelectedItem;

                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
