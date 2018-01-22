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
    public partial class frmSelectMartialArt : Form
    {
        private string _strSelectedMartialArt = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain = false;
        private string _strForcedValue = string.Empty;
        private bool _blnShowQualities = false;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArt(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;

            // Load the Martial Arts information.
            _objXmlDocument = XmlManager.Load("martialarts.xml");
        }

        private void frmSelectMartialArt_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            XmlNodeList objArtList = null;
            // Populate the Martial Arts list.
            if (!string.IsNullOrEmpty(_strForcedValue))
            {
                objArtList = _objXmlDocument.SelectNodes("/chummer/martialarts/martialart[name = \"" + _strForcedValue + "\"]");
            }
            if (objArtList == null || objArtList.Count == 0)
                objArtList = _objXmlDocument.SelectNodes("/chummer/martialarts/martialart[" + _objCharacter.Options.BookXPath() + "]");

            List<ListItem> lstMartialArt = new List<ListItem>();
            foreach (XmlNode objXmlArt in objArtList)
            {
                if (_blnShowQualities == (objXmlArt["quality"] != null) && objXmlArt.RequirementsMet(_objCharacter))
                {
                    lstMartialArt.Add(new ListItem(objXmlArt["id"].InnerText, objXmlArt["translate"]?.InnerText ?? objXmlArt["name"].InnerText));
                }
            }
            lstMartialArt.Sort(CompareListItems.CompareNames);
            lstMartialArts.BeginUpdate();
            lstMartialArts.ValueMember = "Value";
            lstMartialArts.DisplayMember = "Name";
            lstMartialArts.DataSource = lstMartialArt;

            if (lstMartialArts.Items.Count == 1)
            {
                lstMartialArts.SelectedIndex = 0;
                AcceptForm();
            }
            lstMartialArts.EndUpdate();

            _blnLoading = false;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
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
                XmlNode objXmlArt = _objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[id = \"" + strSelectedId + "\"]");

                if (objXmlArt != null)
                {
                    string strSource = objXmlArt["source"].InnerText;
                    string strPage = objXmlArt["altpage"]?.InnerText ?? objXmlArt["page"].InnerText;
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

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
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
        /// Martial Art that was selected in the dialogue.
        /// </summary>
        public string SelectedMartialArt
        {
            get
            {
                return _strSelectedMartialArt;
            }
        }

        /// <summary>
        /// Only show Martial Arts that are provided by a quality
        /// </summary>
        public bool ShowQualities
        {
            get
            {
                return _blnShowQualities;
            }
            set
            {
                _blnShowQualities = value;
            }
        }

        /// <summary>
        /// Force a Martial Art to be selected.
        /// </summary>
        public string ForcedValue
        {
            set
            {
                _strForcedValue = value;
            }
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
        #endregion
    }
}
