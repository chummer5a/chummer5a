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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectArt : Form
    {
        private string _strSelectedItem = string.Empty;

        private Mode _objMode = Mode.Art;
        private string _strNode = "art";
        private string _strRoot = "arts";
        private string _strCategory = string.Empty;
        private string _strLocalName = string.Empty;
        private readonly Character _objCharacter;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private readonly XmlDocument _objMetamagicDocument = new XmlDocument();
        private readonly XmlDocument _objSpellDocument = new XmlDocument();
        private readonly XmlDocument _objPowerDocument = new XmlDocument();
        private readonly XmlDocument _objQualityDocument = new XmlDocument();

        public enum Mode
        {
            Art = 0,
            Enhancement = 1,
            Enchantment = 2,
            Ritual = 3,
        }

        public frmSelectArt(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;

            _objMetamagicDocument = XmlManager.Instance.Load("metamagic.xml");
            _objSpellDocument = XmlManager.Instance.Load("spells.xml");
            _objPowerDocument = XmlManager.Instance.Load("powers.xml");
            _objQualityDocument = XmlManager.Instance.Load("qualities.xml");
        }

        private void frmSelectArt_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            
            switch (_objMode)
            {
                case Mode.Enhancement:
                    _strLocalName = LanguageManager.Instance.GetString("String_Enhancement");
                    break;
                case Mode.Enchantment:
                    _strLocalName = LanguageManager.Instance.GetString("String_Enchantment");
                    break;
                case Mode.Ritual:
                    _strLocalName = LanguageManager.Instance.GetString("String_Ritual");
                    break;
                case Mode.Art:
                    _strLocalName = LanguageManager.Instance.GetString("String_Art");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Text = LanguageManager.Instance.GetString("Title_SelectGeneric").Replace("{0}", _strLocalName);
            chkLimitList.Text = LanguageManager.Instance.GetString("Checkbox_SelectGeneric_LimitList").Replace("{0}", _strLocalName);

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            BuildList();
        }

        private void lstArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstArt.Text))
                return;

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/" + _strRoot + "/" + _strNode + "[name = \"" + lstArt.SelectedValue + "\"]");

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlMetamagic["source"]?.InnerText);
            string strPage = objXmlMetamagic["altpage"]?.InnerText ?? objXmlMetamagic["page"]?.InnerText;
            lblSource.Text = $"{strBook} {strBook}";

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMetamagic["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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
            if (!string.IsNullOrEmpty(lstArt.Text))
                AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildList();
        }

        #region Properties
        /// <summary>
        /// Set the window's Mode to Art, Enchantment, Enhancement, or Ritual.
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
                    case Mode.Art:
                        _strNode = "art";
                        _strRoot = "arts";
                        break;
                    case Mode.Enchantment:
                        _strNode = "spell";
                        _strRoot = "spells";
                        _strCategory = "Enchantments";
                        break;
                    case Mode.Enhancement:
                        _strNode = "enhancement";
                        _strRoot = "enhancements";
                        break;
                    case Mode.Ritual:
                        _strNode = "spell";
                        _strRoot = "spells";
                        _strCategory = "Rituals";
                        break;
                }
            }
        }

        /// <summary>
        /// Name of Metamagic that was selected in the dialogue.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return _strSelectedItem;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Metamagics.
        /// </summary>
        private void BuildList()
        {
            XmlNodeList objXmlMetamagicList;
            List<ListItem> lstArts = new List<ListItem>();

            // Load the Metamagic information.
            switch (_objMode)
            {
                case Mode.Art:
                    _objXmlDocument = XmlManager.Instance.Load("metamagic.xml");
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[" + _objCharacter.Options.BookXPath() + "]");
                    break;
                case Mode.Enhancement:
                    _objXmlDocument = XmlManager.Instance.Load("powers.xml");
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[" + _objCharacter.Options.BookXPath() + "]");
                    break;
                case Mode.Enchantment:
                case Mode.Ritual:
                    _objXmlDocument = XmlManager.Instance.Load("spells.xml");
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[category = '" + _strCategory + "' and (" + _objCharacter.Options.BookXPath() + ")]");
                    break;
                default:
                    _objXmlDocument = XmlManager.Instance.Load("spells.xml");
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[category = '" + _strCategory + "' and (" + _objCharacter.Options.BookXPath() + ")]");
                    break;
            }

            foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
            {
                bool add = (!chkLimitList.Checked ||
                               (chkLimitList.Checked &&
                                Backend.Shared_Methods.SelectionShared.RequirementsMet(objXmlMetamagic, false,
                                    _objCharacter, null,
                                    null, _objQualityDocument, "", _strLocalName)));
                if (!add) continue;
                ListItem objItem = new ListItem();
                objItem.Value = objXmlMetamagic["name"].InnerText;
                objItem.Name = objXmlMetamagic["translate"]?.InnerText ?? objXmlMetamagic["name"].InnerText;
                lstArts.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            lstArts.Sort(objSort.Compare);
            lstArt.BeginUpdate();
            lstArt.DataSource = null;
            lstArt.ValueMember = "Value";
            lstArt.DisplayMember = "Name";
            lstArt.DataSource = lstArts;
            lstArt.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(lstArt.Text))
                return;

            _strSelectedItem = lstArt.SelectedValue.ToString();

            // Make sure the selected Metamagic or Echo meets its requirements.
            XmlNode objXmlMetamagic;
            if (_objMode == Mode.Art)
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/arts/art[name = \"" + lstArt.SelectedValue + "\"]");
            else if (_objMode == Mode.Enchantment)
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[category = \"Enchantments\" and name = \"" + lstArt.SelectedValue + "\"]");
            else if (_objMode == Mode.Enhancement)
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/enhancements/enhancement[name = \"" + lstArt.SelectedValue + "\"]");
            else
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[category = \"Rituals\" and name = \"" + lstArt.SelectedValue + "\"]");

            if (!Backend.Shared_Methods.SelectionShared.RequirementsMet(objXmlMetamagic, true, _objCharacter, null, null, _objQualityDocument, "", _strLocalName))
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
