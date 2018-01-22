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

        private bool _blnLoading = true;
        private Mode _objMode = Mode.Art;
        private string _strNode = "art";
        private string _strRoot = "arts";
        private string _strCategory = string.Empty;
        private string _strLocalName = string.Empty;
        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument = null;

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
            WindowMode = objWindowMode;
            switch (_objMode)
            {
                case Mode.Art:
                    _objXmlDocument = XmlManager.Load("metamagic.xml");
                    _strLocalName = LanguageManager.GetString("String_Art", GlobalOptions.Language);
                    break;
                case Mode.Enhancement:
                    _objXmlDocument = XmlManager.Load("powers.xml");
                    _strLocalName = LanguageManager.GetString("String_Enhancement", GlobalOptions.Language);
                    break;
                case Mode.Enchantment:
                    _strLocalName = LanguageManager.GetString("String_Enchantment", GlobalOptions.Language);
                    _objXmlDocument = XmlManager.Load("spells.xml");
                    break;
                case Mode.Ritual:
                    _strLocalName = LanguageManager.GetString("String_Ritual", GlobalOptions.Language);
                    _objXmlDocument = XmlManager.Load("spells.xml");
                    break;
            }
        }

        private void frmSelectArt_Load(object sender, EventArgs e)
        {
            Text = LanguageManager.GetString("Title_SelectGeneric", GlobalOptions.Language).Replace("{0}", _strLocalName);
            chkLimitList.Text = LanguageManager.GetString("Checkbox_SelectGeneric_LimitList", GlobalOptions.Language).Replace("{0}", _strLocalName);

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

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
                tipTooltip.SetToolTip(lblSource, string.Empty);
                return;
            }

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/" + _strRoot + '/' + _strNode + "[name = \"" + strSelected + "\"]");

            if (objXmlMetamagic == null)
            {
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
                return;
            }

            string strSource = objXmlMetamagic["source"].InnerText;
            string strPage = objXmlMetamagic["altpage"]?.InnerText ?? objXmlMetamagic["page"]?.InnerText;
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;
            tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
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
            if (_blnLoading)
                return;

            XmlNodeList objXmlMetamagicList;
            List<ListItem> lstArts = new List<ListItem>();

            // Load the Metamagic information.
            switch (_objMode)
            {
                case Mode.Art:
                case Mode.Enhancement:
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + '/' + _strNode + '[' + _objCharacter.Options.BookXPath() + ']');
                    break;
                default:
                    objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + '/' + _strNode + "[category = '" + _strCategory + "' and (" + _objCharacter.Options.BookXPath() + ")]");
                    break;
            }

            foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
            {
                if (!chkLimitList.Checked || objXmlMetamagic.RequirementsMet(_objCharacter))
                {
                    string strName = objXmlMetamagic["name"].InnerText;
                    lstArts.Add(new ListItem(strName, objXmlMetamagic["translate"]?.InnerText ?? strName));
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
            if (string.IsNullOrEmpty(strSelectedItem))
                return;

            // Make sure the selected Metamagic or Echo meets its requirements.
            XmlNode objXmlMetamagic;
            if (_objMode == Mode.Art)
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/arts/art[name = \"" + strSelectedItem + "\"]");
            else if (_objMode == Mode.Enchantment)
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[category = \"Enchantments\" and name = \"" + strSelectedItem + "\"]");
            else if (_objMode == Mode.Enhancement)
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/enhancements/enhancement[name = \"" + strSelectedItem + "\"]");
            else
                objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[category = \"Rituals\" and name = \"" + strSelectedItem + "\"]");

            if (!objXmlMetamagic.RequirementsMet(_objCharacter))
                return;

            _strSelectedItem = strSelectedItem;

            DialogResult = DialogResult.OK;
        }


        #endregion
    }
}
