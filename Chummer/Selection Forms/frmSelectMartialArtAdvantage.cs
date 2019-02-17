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
    public partial class frmSelectMartialArtAdvantage : Form
    {
        private string _strSelectedAdvantage = string.Empty;

        private bool _blnAddAgain = false;
        private string _strMartialArt = string.Empty;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMartialArtAdvantage(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            // Load the Martial Art information.
            _objXmlDocument = XmlManager.Load("martialarts.xml");
        }

        private void frmSelectMartialArtAdvantage_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            List<ListItem> lstAdvantage = new List<ListItem>();

            // Populate the Martial Art Advantage list.
            XmlNode objMartialArtNode = _objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + _strMartialArt + "\"]");
            if (objMartialArtNode["alltechniques"] != null)
                objMartialArtNode = _objXmlDocument.SelectSingleNode("/chummer");
            XmlNodeList objXmlAdvantageList = objMartialArtNode.SelectNodes("techniques/technique");
            foreach (XmlNode objXmlAdvantage in objXmlAdvantageList)
            {
                string strAdvantageName = objXmlAdvantage["name"].InnerText;
                foreach (MartialArt objMartialArt in _objCharacter.MartialArts.Where(objMartialArt => objMartialArt.Name == _strMartialArt))
                {
                    if (objMartialArt.Advantages.Any(advantage => advantage.Name == strAdvantageName))
                    {
                        goto NotNewAdvantage;
                    }
                }

                ListItem objItem = new ListItem();
                objItem.Value = strAdvantageName;
                objItem.Name = objXmlAdvantage.Attributes?["translate"]?.InnerText ?? strAdvantageName;
                lstAdvantage.Add(objItem);
                NotNewAdvantage:;
            }
            SortListItem objSort = new SortListItem();
            lstAdvantage.Sort(objSort.Compare);
            lstAdvantages.BeginUpdate();
            lstAdvantages.DataSource = null;
            lstAdvantages.ValueMember = "Value";
            lstAdvantages.DisplayMember = "Name";
            lstAdvantages.DataSource = lstAdvantage;
            lstAdvantages.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstAdvantages.Text))
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstAdvantages_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
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
        /// Martial Art to display Advantages for.
        /// </summary>
        public string MartialArt
        {
            set
            {
                _strMartialArt = value;
            }
        }

        /// <summary>
        /// Martial Art Advantage that was selected in the dialogue.
        /// </summary>
        public string SelectedAdvantage
        {
            get
            {
                return _strSelectedAdvantage;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedAdvantage = lstAdvantages.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }
        #endregion
    }
}
