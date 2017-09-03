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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectMartialArt : Form
    {
        private string _strSelectedMartialArt = string.Empty;

        private bool _blnAddAgain = false;
        private string _strForcedValue = string.Empty;
        private bool _blnShowQualities = false;

        private XmlDocument _objXmlDocument = new XmlDocument();
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArt(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
        }

        private void frmSelectMartialArt_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            XmlNodeList objArtList;
            List<ListItem> lstMartialArt = new List<ListItem>();

            // Load the Martial Arts information.
            _objXmlDocument = XmlManager.Instance.Load("martialarts.xml");

            // Populate the Martial Arts list.
            if (string.IsNullOrEmpty(_strForcedValue))
                objArtList = _objXmlDocument.SelectNodes("/chummer/martialarts/martialart[" + _objCharacter.Options.BookXPath() + "]");
            else
                objArtList = _objXmlDocument.SelectNodes("/chummer/martialarts/martialart[name = \"" + _strForcedValue + "\"]");
            foreach (XmlNode objXmlArt in objArtList)
            {
                XmlNode objXmlQuality = objXmlArt["quality"];
                if ((_blnShowQualities && objXmlQuality != null) || (!_blnShowQualities && objXmlQuality == null))
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlArt["name"].InnerText;
                    objItem.Name = objXmlArt["translate"]?.InnerText ?? objXmlArt["name"].InnerText;
                    lstMartialArt.Add(objItem);
                }
            }
            SortListItem objSort = new SortListItem();
            lstMartialArt.Sort(objSort.Compare);
            lstMartialArts.BeginUpdate();
            lstMartialArts.DataSource = null;
            lstMartialArts.ValueMember = "Value";
            lstMartialArts.DisplayMember = "Name";
            lstMartialArts.DataSource = lstMartialArt;

            if (lstMartialArts.Items.Count == 1)
            {
                lstMartialArts.SelectedIndex = 0;
                AcceptForm();
            }
            lstMartialArts.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstMartialArts.Text))
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
            // Populate the Martial Arts list.
            XmlNode objXmlArt = _objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + lstMartialArts.SelectedValue + "\"]");

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlArt["source"].InnerText);
            string strPage = objXmlArt["page"].InnerText;
            if (objXmlArt["altpage"] != null)
                strPage = objXmlArt["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlArt["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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
            _strSelectedMartialArt = lstMartialArts.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}