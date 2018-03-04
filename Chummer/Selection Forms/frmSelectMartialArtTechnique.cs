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

        private bool _blnAddAgain;

        private readonly MartialArt _objMartialArt;
        private readonly XPathNavigator _xmlBaseChummerNode;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArtTechnique(Character objCharacter, MartialArt objMartialArt)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objMartialArt = objMartialArt;
            // Load the Martial Art information.
            _xmlBaseChummerNode = XmlManager.Load("martialarts.xml").GetFastNavigator().SelectSingleNode("/chummer");
        }

        private void frmSelectMartialArtTechnique_Load(object sender, EventArgs e)
        {
            HashSet<string> setAllowedTechniques = null;
            // Populate the Martial Art Tecnnique list.
            XPathNavigator xmlMartialArtNode = _xmlBaseChummerNode.SelectSingleNode("martialarts/martialart[name = \"" + _objMartialArt.Name + "\"]");
            if (xmlMartialArtNode != null && !xmlMartialArtNode.NodeExists("alltechniques"))
            {
                setAllowedTechniques = new HashSet<string>();
                foreach (XPathNavigator xmlTechnique in xmlMartialArtNode.Select("techniques/technique"))
                {
                    string strTechniqueName = xmlTechnique.Value;
                    if (_objMartialArt.Techniques.All(x => x.Name != strTechniqueName))
                    {
                        setAllowedTechniques.Add(strTechniqueName);
                    }
                }
            }

            List<ListItem> lstTechniqueItems = new List<ListItem>();
            foreach (XPathNavigator xmlTechnique in _xmlBaseChummerNode.Select("techniques/technique"))
            {
                string strId = xmlTechnique.SelectSingleNode("id")?.Value;
                if (!string.IsNullOrEmpty(strId))
                {
                    string strTechniqueName = xmlTechnique.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);

                    if (setAllowedTechniques?.Contains(strTechniqueName) == false)
                        continue;

                    if (xmlTechnique.RequirementsMet(_objCharacter))
                    {
                        lstTechniqueItems.Add(new ListItem(strId, xmlTechnique.SelectSingleNode("translate")?.Value ?? strTechniqueName));
                    }
                }
            }
            lstTechniqueItems.Sort(CompareListItems.CompareNames);
            lstTechniques.BeginUpdate();
            lstTechniques.ValueMember = "Value";
            lstTechniques.DisplayMember = "Name";
            lstTechniques.DataSource = lstTechniqueItems;
            lstTechniques.EndUpdate();
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
            string strSelectedId = lstTechniques.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XPathNavigator xmlTechnique = _xmlBaseChummerNode.SelectSingleNode("/chummer/techniques/technique[id = \"" + strSelectedId + "\"]");

                if (xmlTechnique != null)
                {
                    string strSource = xmlTechnique.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strPage = xmlTechnique.SelectSingleNode("altpage")?.Value ?? xmlTechnique.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

                    tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                }
                else
                {
                    lblSource.Text = string.Empty;

                    tipTooltip.SetToolTip(lblSource, string.Empty);
                }
            }
            else
            {
                lblSource.Text = string.Empty;

                tipTooltip.SetToolTip(lblSource, string.Empty);
            }
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
        #endregion
    }
}
