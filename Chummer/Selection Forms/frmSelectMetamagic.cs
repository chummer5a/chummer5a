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

        private readonly Mode _objMode = Mode.Metamagic;
        private readonly string _strRootXPath = "/chummer/metamagics/metamagic";

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

            _objMode = objMode;
            // Load the Metamagic information.
            switch (_objMode)
            {
                case Mode.Metamagic:
                    _strRootXPath = "/chummer/metamagics/metamagic";
                    _objXmlDocument = XmlManager.Load("metamagic.xml");
                    break;
                case Mode.Echo:
                    _strRootXPath = "/chummer/echoes/echo";
                    _objXmlDocument = XmlManager.Load("echoes.xml");
                    break;
            }
        }

        private void frmSelectMetamagic_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            string strType = LanguageManager.GetString(_objMode == Mode.Echo ? "String_Echo" : "String_Metamagic", GlobalOptions.Language);
            Text = LanguageManager.GetString("Title_SelectGeneric", GlobalOptions.Language).Replace("{0}", strType);
            chkLimitList.Text = LanguageManager.GetString("Checkbox_SelectGeneric_LimitList", GlobalOptions.Language).Replace("{0}", strType);

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            BuildMetamagicList();
        }

        private void lstMetamagic_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedId = lstMetamagic.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected piece of Cyberware.
                XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strRootXPath + "[id = \"" + strSelectedId + "\"]");

                if (objXmlMetamagic != null)
                {
                    string strSource = objXmlMetamagic["source"]?.InnerText;
                    string strPage = objXmlMetamagic["altpage"]?.InnerText ?? objXmlMetamagic["page"]?.InnerText;
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
            AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildMetamagicList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Id of Metamagic that was selected in the dialogue.
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
            string strFilter = _objCharacter.Options.BookXPath();
            // If the character has MAG enabled, filter the list based on Adept/Magician availability.
            if (_objCharacter.MAGEnabled)
            {
                bool blnIsMagician = _objCharacter.MagicianEnabled;
                if (blnIsMagician != _objCharacter.AdeptEnabled)
                {
                    if (blnIsMagician)
                        strFilter = "magician = 'True' and (" + strFilter + ')';
                    else
                        strFilter = "adept = 'True' and (" + strFilter + ')';
                }
            }
            XmlNodeList objXmlMetamagicList = _objXmlDocument.SelectNodes(_strRootXPath + '[' + strFilter + ']');
            List<ListItem> lstMetamagics = new List<ListItem>();
            foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
            {
                if (!chkLimitList.Checked || objXmlMetamagic.RequirementsMet(_objCharacter))
                {
                    lstMetamagics.Add(new ListItem(objXmlMetamagic["id"].InnerText, objXmlMetamagic["translate"]?.InnerText ?? objXmlMetamagic["name"].InnerText));
                }
            }
            lstMetamagics.Sort(CompareListItems.CompareNames);
            lstMetamagic.BeginUpdate();
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
            string strSelectedId = lstMetamagic.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Make sure the selected Metamagic or Echo meets its requirements.
                XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strRootXPath + "[id = \"" + strSelectedId + "\"]");

                if (!objXmlMetamagic.RequirementsMet(_objCharacter, LanguageManager.GetString(_objMode == Mode.Metamagic ? "String_Metamagic" : "String_Echo", GlobalOptions.Language)))
                    return;

                _strSelectedMetamagic = strSelectedId;

                DialogResult = DialogResult.OK;
            }
        }

        #endregion
    }
}
