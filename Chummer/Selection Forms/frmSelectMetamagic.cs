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

        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument = null;

        public enum Mode
        {
            Metamagic = 0,
            Echo,
        }

        #region Control Events
        public frmSelectMetamagic(Character objCharacter, Mode objMode)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            
            WindowMode = objMode;
            // Load the Metamagic information.
            switch (_objMode)
            {
                case Mode.Metamagic:
                    _objXmlDocument = XmlManager.Load("metamagic.xml");
                    break;
                case Mode.Echo:
                    _objXmlDocument = XmlManager.Load("echoes.xml");
                    break;
            }
        }

        private void frmSelectMetamagic_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            string s = LanguageManager.GetString(_strNode == "echo" ? "String_Echo" : "String_Metamagic", GlobalOptions.Language);
            Text = LanguageManager.GetString("Title_SelectGeneric", GlobalOptions.Language).Replace("{0}", s);
            chkLimitList.Text = LanguageManager.GetString("Checkbox_SelectGeneric_LimitList", GlobalOptions.Language).Replace("{0}", s);

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
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
                string strBook = CommonFunctions.LanguageBookShort(objXmlMetamagic["source"]?.InnerText, GlobalOptions.Language);
                string strPage = objXmlMetamagic["altpage"]?.InnerText ?? objXmlMetamagic["page"]?.InnerText;
                lblSource.Text = $"{strBook} {strPage}";

                tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(objXmlMetamagic["source"]?.InnerText, GlobalOptions.Language) + " " + LanguageManager.GetString("String_Page", GlobalOptions.Language) + " " + strPage);
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
            string s = LanguageManager.GetString(_strNode == "echo" ? "String_Echo" : "String_Metamagic", GlobalOptions.Language);

            if (objXmlMetamagicList != null)
            {
                foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
                {
                    if (!chkLimitList.Checked ||
                        Backend.SelectionShared.RequirementsMet(objXmlMetamagic, false, _objCharacter, string.Empty, s))
                    {
                        string strName = objXmlMetamagic["name"]?.InnerText ?? string.Empty;
                        lstMetamagics.Add(new ListItem(strName, objXmlMetamagic["translate"]?.InnerText ?? strName));
                    }
                }
            }
            else
            {
                Utils.BreakIfDebug();
            }
            lstMetamagics.Sort(CompareListItems.CompareNames);
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

            string s = LanguageManager.GetString(_strNode == "echo" ? "String_Echo" : "String_Metamagic", GlobalOptions.Language);
            if (!Backend.SelectionShared.RequirementsMet(objXmlMetamagic, true, _objCharacter, string.Empty, s))
                return;
            
            DialogResult = DialogResult.OK;
        }

        #endregion
    }
}
