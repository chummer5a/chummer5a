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
 using System.Diagnostics;
 using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    // TODO: Review naming schema
    // ReSharper disable once InconsistentNaming
    public partial class frmSelectMetamagic : Form
    {
        private string _strSelectedMetamagic = string.Empty;

        private Mode _objMode = Mode.Metamagic;
        private string _strNode = "metamagic";
        private string _strRoot = "metamagics";
        private bool _blnAddAgain = false;

        private readonly Character _objCharacter;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private readonly XmlDocument _objMetatypeDocument;
        private readonly XmlDocument _objCritterDocument;
        private readonly XmlDocument _objQualityDocument;

        public enum Mode
        {
            Metamagic = 0,
            Echo = 1,
        }

        #region Control Events
        public frmSelectMetamagic(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;

            _objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
            _objCritterDocument = XmlManager.Instance.Load("critters.xml");
            _objQualityDocument = XmlManager.Instance.Load("qualities.xml");
        }

        private void frmSelectMetamagic_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            string s = LanguageManager.Instance.GetString(_strNode == "echo" ? "String_Echo" : "String_Metamagic");
            Text = LanguageManager.Instance.GetString("Title_SelectGeneric").Replace("{0}", s);
            chkLimitList.Text = LanguageManager.Instance.GetString("Checkbox_SelectGeneric_LimitList").Replace("{0}", s);

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            BuildMetamagicList();
        }

        private void lstMetamagic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstMetamagic.Text))
                return;

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/" + _strRoot + "/" + _strNode + "[name = \"" + lstMetamagic.SelectedValue + "\"]");

            if (objXmlMetamagic != null)
            {
                string strBook = _objCharacter.Options.LanguageBookShort(objXmlMetamagic["source"]?.InnerText);
                string strPage = objXmlMetamagic["altpage"]?.InnerText ?? objXmlMetamagic["page"]?.InnerText;
                lblSource.Text = $"{strBook} {strPage}";

                tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMetamagic["source"]?.InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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
            if (!string.IsNullOrEmpty(lstMetamagic.Text))
                AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildMetamagicList();
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
        /// Set the window's Mode to Cyberware or Bioware.
        /// </summary>
        public Mode WindowMode
        {
            get
            {
                return _objMode;
            }
            set
            {
                _objMode = value;
                switch (_objMode)
                {
                    case Mode.Metamagic:
                        _strNode = "metamagic";
                        _strRoot = "metamagics";
                        break;
                    case Mode.Echo:
                        _strNode = "echo";
                        _strRoot = "echoes";
                        break;
                }
            }
        }

        /// <summary>
        /// Name of Metamagic that was selected in the dialogue.
        /// </summary>
        public string SelectedMetamagic
        {
            get
            {
                return _strSelectedMetamagic;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Metamagics.
        /// </summary>
        private void BuildMetamagicList()
        {
            XmlNodeList objXmlMetamagicList;
            List<ListItem> lstMetamagics = new List<ListItem>();

            // Load the Metamagic information.
            switch (_objMode)
            {
                case Mode.Metamagic:
                    _objXmlDocument = XmlManager.Instance.Load("metamagic.xml");
                    break;
                case Mode.Echo:
                    _objXmlDocument = XmlManager.Instance.Load("echoes.xml");
                    break;
            }

            // If the character has MAG enabled, filter the list based on Adept/Magician availability.
            if (_objCharacter.MAGEnabled)
            {
                if (_objCharacter.MagicianEnabled && !_objCharacter.AdeptEnabled)
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[magician = 'yes' and (" + _objCharacter.Options.BookXPath() + ")]");
                else if (!_objCharacter.MagicianEnabled && _objCharacter.AdeptEnabled)
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[adept = 'yes' and (" + _objCharacter.Options.BookXPath() + ")]");
                else
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[" + _objCharacter.Options.BookXPath() + "]");
            }
            else
                objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[" + _objCharacter.Options.BookXPath() + "]");
            string s = LanguageManager.Instance.GetString(_strNode == "echo" ? "String_Echo" : "String_Metamagic");

            if (objXmlMetamagicList != null)
            foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
                {
                    bool add = !chkLimitList.Checked ||
                                  (chkLimitList.Checked &&
                                   Backend.Shared_Methods.SelectionShared.RequirementsMet(objXmlMetamagic, false, _objCharacter,
                                       _objMetatypeDocument, _objCritterDocument, _objQualityDocument, "", s));
                    if (!add) continue;
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlMetamagic["name"]?.InnerText;
                    objItem.Name = objXmlMetamagic["translate"]?.InnerText ?? objXmlMetamagic["name"]?.InnerText;
                    lstMetamagics.Add(objItem);
                }
            else
            {
                Utils.BreakIfDebug();
            }
            SortListItem objSort = new SortListItem();
            lstMetamagics.Sort(objSort.Compare);
            lstMetamagic.BeginUpdate();
            lstMetamagic.DataSource = null;
            lstMetamagic.ValueMember = "Value";
            lstMetamagic.DisplayMember = "Name";
            lstMetamagic.DataSource = lstMetamagics;
            lstMetamagic.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(lstMetamagic.Text))
                return;

            _strSelectedMetamagic = lstMetamagic.SelectedValue.ToString();

            // Make sure the selected Metamagic or Echo meets its requirements.
            XmlNode objXmlMetamagic = _objMode == Mode.Metamagic
                ? _objXmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + lstMetamagic.SelectedValue + "\"]")
                : _objXmlDocument.SelectSingleNode("/chummer/echoes/echo[name = \"" + lstMetamagic.SelectedValue + "\"]");

            string s = LanguageManager.Instance.GetString(_strNode == "echo" ? "String_Echo" : "String_Metamagic");
            if (!Backend.Shared_Methods.SelectionShared.RequirementsMet(objXmlMetamagic, true, _objCharacter, _objMetatypeDocument, _objCritterDocument, _objQualityDocument, "", s))
                return;
            DialogResult = DialogResult.OK;
        }

        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}