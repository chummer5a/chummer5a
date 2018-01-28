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
using Chummer.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectMartialArtTechnique : Form
    {
        private string _strSelectedTechnique = string.Empty;

        private bool _blnAddAgain = false;

        private readonly MartialArt _objMartialArt;
        private readonly XmlDocument _xmlDocument;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArtTechnique(Character objCharacter, MartialArt objMartialArt)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objMartialArt = objMartialArt;
            // Load the Martial Art information.
            _xmlDocument = XmlManager.Load("martialarts.xml");
        }

        private void frmSelectMartialArtTechnique_Load(object sender, EventArgs e)
        {
            HashSet<string> setAllowedTechniques = null;
            // Populate the Martial Art Tecnnique list.
            XmlNode xmlMartialArtNode = _objMartialArt.GetNode();
            if (xmlMartialArtNode != null && xmlMartialArtNode["alltechniques"] == null)
            {
                setAllowedTechniques = new HashSet<string>();
                foreach (XmlNode xmlTechnique in xmlMartialArtNode.SelectNodes("techniques/technique"))
                {
                    string strTechniqueName = xmlTechnique.InnerText;
                    if (!_objMartialArt.Techniques.Any(x => x.Name == strTechniqueName))
                    {
                        setAllowedTechniques.Add(strTechniqueName);
                    }
                }
            }

            List<ListItem> lstTechniqueItems = new List<ListItem>();
            foreach (XmlNode xmlTechnique in _xmlDocument.SelectNodes("/chummer/techniques/technique"))
            {
                string strTechniqueName = xmlTechnique["name"].InnerText;

                if (setAllowedTechniques?.Contains(strTechniqueName) == false)
                    continue;

                if (xmlTechnique.RequirementsMet(_objCharacter))
                {
                    lstTechniqueItems.Add(new ListItem(xmlTechnique["id"].InnerText, xmlTechnique["translate"]?.InnerText ?? strTechniqueName));
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
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstTechniques_DoubleClick(object sender, EventArgs e)
        {
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
                XmlNode xmlTechnique = _xmlDocument.SelectSingleNode("/chummer/techniques/technique[id = \"" + strSelectedId + "\"]");

                if (xmlTechnique != null)
                {
                    string strSource = xmlTechnique["source"].InnerText;
                    string strPage = xmlTechnique["altpage"]?.InnerText ?? xmlTechnique["page"].InnerText;
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

                    tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                }
            }
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
        /// Martial Art Technique that was selected in the dialogue.
        /// </summary>
        public string SelectedTechnique
        {
            get
            {
                return _strSelectedTechnique;
            }
        }
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
