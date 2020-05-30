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
    public partial class frmSelectMartialArtManeuver : Form
    {
        private string _strSelectedManeuver = string.Empty;

        private bool _blnAddAgain = false;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArtManeuver(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            // Load the Martial Art information.
            _objXmlDocument = XmlManager.Load("martialarts.xml");
        }

        private void frmSelectMartialArtManeuver_Load(object sender, EventArgs e)
        {
            List<ListItem> lstManeuver = new List<ListItem>();

            // Populate the Martial Art Maneuver list.
            XmlNodeList objManeuverList = _objXmlDocument.SelectNodes("/chummer/maneuvers/maneuver[" + _objCharacter.Options.BookXPath() + "]");
            foreach (XmlNode objXmlManeuver in objManeuverList)
            {
                string strName = objXmlManeuver["name"].InnerText;
                lstManeuver.Add(new ListItem(strName, objXmlManeuver["translate"]?.InnerText ?? strName));
            }
            lstManeuver.Sort(CompareListItems.CompareNames);
            lstManeuvers.BeginUpdate();
            lstManeuvers.DataSource = null;
            lstManeuvers.ValueMember = "Value";
            lstManeuvers.DisplayMember = "Name";
            lstManeuvers.DataSource = lstManeuver;
            lstManeuvers.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstManeuvers.Text))
            {
                _blnAddAgain = false;
                AcceptForm();
            }
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
            string strSelectedId = lstManeuvers.SelectedValue?.ToString();
            XmlNode xmlManeuver = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlManeuver = _objXmlDocument.SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + strSelectedId + "\"]");

            if (xmlManeuver != null)
            {
                string strSource = xmlManeuver["source"].InnerText;
                string strPage = xmlManeuver["altpage"]?.InnerText ?? xmlManeuver["page"].InnerText;
                string strSpace = LanguageManager.GetString("String_Space");
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource) + strSpace + strPage;
                ToolTipFactory.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
            }
            else
            {
                lblSource.Text = string.Empty;

                ToolTipFactory.SetToolTip(lblSource, string.Empty);
            }
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstManeuvers.Text))
            {
                _blnAddAgain = true;
                AcceptForm();
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
        /// Maneuver that was selected in the dialogue.
        /// </summary>
        public string SelectedManeuver
        {
            get
            {
                return _strSelectedManeuver;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedManeuver = lstManeuvers.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
