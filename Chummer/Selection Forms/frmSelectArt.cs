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
        private string _strSelectedItem = "";

        private Mode _objMode = Mode.Art;
        private string _strNode = "art";
        private string _strRoot = "arts";
        private string _strCategory = "";

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
            if (_objMode == Mode.Enhancement)
            {
                this.Text = LanguageManager.Instance.GetString("Title_SelectEnhancement");
                chkLimitList.Text = LanguageManager.Instance.GetString("Checkbox_SelectEnhancement_LimitList");
            }
            else if (_objMode == Mode.Enchantment)
            {
                this.Text = LanguageManager.Instance.GetString("Title_SelectEnchantment");
                chkLimitList.Text = LanguageManager.Instance.GetString("Checkbox_SelectEnchantment_LimitList");
            }
            else if (_objMode == Mode.Ritual)
            {
                this.Text = LanguageManager.Instance.GetString("Title_SelectRitual");
                chkLimitList.Text = LanguageManager.Instance.GetString("Checkbox_SelectRitual_LimitList");
            }

            foreach (Label objLabel in this.Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = "";
            }

            BuildList();
        }

        private void lstArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstArt.Text == "")
                return;

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/" + _strRoot + "/" + _strNode + "[name = \"" + lstArt.SelectedValue + "\"]");

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlMetamagic["source"].InnerText);
            string strPage = objXmlMetamagic["page"].InnerText;
            if (objXmlMetamagic["altpage"] != null)
                strPage = objXmlMetamagic["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMetamagic["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void lstArt_DoubleClick(object sender, System.EventArgs e)
        {
            if (lstArt.Text != "")
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
                if (!chkLimitList.Checked || (chkLimitList.Checked && RequirementMet(objXmlMetamagic, false)))
                {
                    bool blnNew = true;
                    switch (_objMode)
                    {
                        case Mode.Art:
                            foreach(Art objArt in _objCharacter.Arts)
                            {
                                if (objArt.Name == objXmlMetamagic["name"].InnerText)
                                {
                                    blnNew = false;
                                    break;
                                }
                            }
                            break;
                        case Mode.Enhancement:
                            foreach(Enhancement objEnhancement in _objCharacter.Enhancements)
                            {
                                if (objEnhancement.Name == objXmlMetamagic["name"].InnerText)
                                {
                                    blnNew = false;
                                    break;
                                }
                            }
                            foreach(Power objPower in _objCharacter.Powers)
                            {
                                foreach (Enhancement objEnhancement in objPower.Enhancements)
                                {
                                    if (objEnhancement.Name == objXmlMetamagic["name"].InnerText)
                                    {
                                        blnNew = false;
                                        break;
                                    }
                                }
                            }
                            break;
                        case Mode.Enchantment:
                        case Mode.Ritual:
                            foreach (Spell objSpell in _objCharacter.Spells)
                            {
                                if (objSpell.Name == objXmlMetamagic["name"].InnerText)
                                {
                                    blnNew = false;
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    if (blnNew)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objXmlMetamagic["name"].InnerText;
                        if (objXmlMetamagic["translate"] != null)
                            objItem.Name = objXmlMetamagic["translate"].InnerText;
                        else
                            objItem.Name = objXmlMetamagic["name"].InnerText;
                        lstArts.Add(objItem);
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            lstArts.Sort(objSort.Compare);
            lstArt.DataSource = null;
            lstArt.ValueMember = "Value";
            lstArt.DisplayMember = "Name";
            lstArt.DataSource = lstArts;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (lstArt.Text == "")
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

            if (!RequirementMet(objXmlMetamagic, true))
                return;

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Check if the Metamagic's requirements/restrictions are being met.
        /// </summary>
        /// <param name="objXmlCheckMetamagic">XmlNode of the Metamagic.</param>
        /// <param name="blnShowMessage">Whether or not a message should be shown if the requirements are not met.</param>
        private bool RequirementMet(XmlNode objXmlCheckMetamagic, bool blnShowMessage)
        {
            // Ignore the rules.
            if (_objCharacter.IgnoreRules)
                return true;

            string strParent = "";
            string strChild = "";
            if (_objMode == Mode.Art)
            {
                strParent = "arts";
                strChild = "art";
            }
            else if (_objMode == Mode.Enhancement)
            {
                strParent = "enhancements";
                strChild = "enhancement";
            }
            else
            {
                strParent = "spells";
                strChild = "spell";
            }

            if (objXmlCheckMetamagic.InnerXml.Contains("<required>"))
            {
                bool blnRequirementMet = true;
                string strRequirement = "";
                if (objXmlCheckMetamagic.InnerXml.Contains("<allof>"))
                {
                    strRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_AllOf");
                }

                // Metamagic requirements.
                foreach (XmlNode objXmlMetamagic in objXmlCheckMetamagic.SelectNodes("required/allof/metamagic"))
                {
                    bool blnFound = false;
                    foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
                    {
                        if (objMetamagic.Name == objXmlMetamagic.InnerText)
                        {
                            blnFound = true;
                            break;
                        }
                    }

                    if (!blnFound)
                    {
                        blnRequirementMet = false;
                        XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + strParent + "/" + strChild + "[name = \"" + objXmlMetamagic.InnerText + "\"]");
                        if (objNode["translate"] != null)
                            strRequirement += "\n\t" + objNode["translate"].InnerText;
                        else
                            strRequirement += "\n\t" + objXmlMetamagic.InnerText;
                    }
                }

                // Power requirements.
                foreach (XmlNode objXmlPower in objXmlCheckMetamagic.SelectNodes("required/allof/power"))
                {
                    bool blnFound = false;
                    foreach (Power objPower in _objCharacter.Powers)
                    {
                        if (objPower.Name == objXmlPower.InnerText)
                        {
                            blnFound = true;
                            break;
                        }
                    }

                    if (!blnFound)
                    {
                        blnRequirementMet = false;
                        strRequirement += "\n\t" + objXmlPower.InnerText;
                    }
                }

                // Art requirements.
                bool blnStreetGrimoire = (_objCharacter.Options.Books["SG"]);
                if (blnStreetGrimoire && !_objCharacter.Options.IgnoreArtRequirements)
                {
                    foreach (XmlNode objXmlArt in objXmlCheckMetamagic.SelectNodes("required/allof/art"))
                    {
                        bool blnFound = false;
                        foreach (Art objArt in _objCharacter.Arts)
                        {
                            if (objArt.Name == objXmlArt.InnerText)
                            {
                                blnFound = true;
                                break;
                            }
                        }

                        if (!blnFound)
                        {
                            blnRequirementMet = false;
                            strRequirement += "\n\t" + objXmlArt.InnerText;
                        }
                    }
                }

                // Echo requirements.
                foreach (XmlNode objXmlEcho in objXmlCheckMetamagic.SelectNodes("required/allof/echo"))
                {
                    bool blnFound = false;
                    foreach (Metamagic objEcho in _objCharacter.Metamagics)
                    {
                        if (objEcho.Name == objXmlEcho.InnerText)
                        {
                            blnFound = true;
                            break;
                        }
                    }

                    if (!blnFound)
                    {
                        blnRequirementMet = false;
                        XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + strParent + "/" + strChild + "[name = \"" + objXmlEcho.InnerText + "\"]");
                        if (objNode["translate"] != null)
                            strRequirement += "\n\t" + objNode["translate"].InnerText;
                        else
                            strRequirement += "\n\t" + objXmlEcho.InnerText;
                    }
                }

                // Quality requirements.
                bool blnQualityFound = false;
                string strQualityRequirement = "";
                if (objXmlCheckMetamagic.SelectNodes("required/allof/quality").Count == 0)
                    blnQualityFound = true;
                else
                {
                    foreach (XmlNode objXmlQuality in objXmlCheckMetamagic.SelectNodes("required/allof/quality"))
                    {
                        foreach (Quality objQuality in _objCharacter.Qualities)
                        {
                            if (objQuality.Name == objXmlQuality.InnerText)
                            {
                                blnQualityFound = true;
                                break;
                            }

                            XmlNode objNode =
                                _objQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objNode["translate"] != null)
                                strQualityRequirement += "\n\t" + objNode["translate"].InnerText;
                            else
                                strQualityRequirement += "\n\t" + objXmlQuality.InnerText;
                        }
                    }
                    if (!blnQualityFound)
                    {
                        blnRequirementMet = false;
                        strRequirement += strQualityRequirement;
                    }
                }

                // Check OneOf requirements
                string strOneOfRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_OneOf");
                bool blnOneOfRequirementMet = false;

                if (!objXmlCheckMetamagic.InnerXml.Contains("<oneof>"))
                {
                    blnOneOfRequirementMet = true;
                }

                foreach (XmlNode objXmlQuality in objXmlCheckMetamagic.SelectNodes("required/oneof/quality"))
                {
                    foreach (Quality objQuality in _objCharacter.Qualities)
                    {
                        if (objQuality.Name == objXmlQuality.InnerText)
                        {
                            blnOneOfRequirementMet = true;
                            break;
                        }
                    }

                    if (!blnOneOfRequirementMet)
                        strOneOfRequirement += "\n\t" + objXmlQuality.InnerText;
                    else
                        break;
                }

                if (blnStreetGrimoire && !_objCharacter.Options.IgnoreArtRequirements)
                {
                    foreach (XmlNode objXmlArt in objXmlCheckMetamagic.SelectNodes("required/oneof/art"))
                    {
                        foreach (Art objArt in _objCharacter.Arts)
                        {
                            if (objArt.Name == objXmlArt.InnerText)
                            {
                                blnOneOfRequirementMet = true;
                                break;
                            }
                        }

                        if (!blnOneOfRequirementMet)
                            strOneOfRequirement += "\n\t" + objXmlArt.InnerText;
                        else
                            break;
                    }
                }

                if (!blnOneOfRequirementMet)
                {
                    blnRequirementMet = false;
                    strRequirement += strOneOfRequirement;
                }

                if (!blnRequirementMet)
                {
                    string strMessage = "";
                    string strTitle = "";

                    if (_objMode == Mode.Art)
                    {
                        strMessage = LanguageManager.Instance.GetString("Message_SelectArt_ArtRequirement");
                        strTitle = LanguageManager.Instance.GetString("MessageTitle_SelectArt_ArtRequirement");
                    }
                    else if (_objMode == Mode.Enchantment)
                    {
                        strMessage = LanguageManager.Instance.GetString("Message_SelectArt_EnchantmentRequirement");
                        strTitle = LanguageManager.Instance.GetString("MessageTitle_SelectArt_EnchantmentRequirement");
                    }
                    else if (_objMode == Mode.Enhancement)
                    {
                        strMessage = LanguageManager.Instance.GetString("Message_SelectArt_EnhancementRequirement");
                        strTitle = LanguageManager.Instance.GetString("MessageTitle_SelectArt_EnhancementRequirement");
                    }
                    else
                    {
                        strMessage = LanguageManager.Instance.GetString("Message_SelectArt_RitualRequirement");
                        strTitle = LanguageManager.Instance.GetString("MessageTitle_SelectArt_RitualRequirement");
                    }
                    strMessage += strRequirement;

                    if (blnShowMessage)
                        MessageBox.Show(strMessage, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            return true;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }

    }
}
