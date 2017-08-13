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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using Chummer.Skills;
using Chummer.Backend.Attributes;

namespace Chummer
{
    public partial class frmSelectLifestyleQuality : Form
    {
        public int buildPos = 0;
        public int buildNeg = 0;
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain = false;
        private readonly Character _objCharacter;
        private string _strIgnoreQuality = string.Empty;
        private string _strSelectedLifestyle = string.Empty;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private List<ListItem> _lstCategory = new List<ListItem>();
        private List<string> _lstLifestylesSorted = new List<string>(new string[] {"Street", "Squatter", "Low", "Medium", "High", "Luxury"});
        private string[] _strLifestyleSpecific = { "Bolt Hole", "Traveler", "Commercial", "Hospitalized" };

        private static string _strSelectCategory = string.Empty;

        private readonly XmlDocument _objMetatypeDocument = new XmlDocument();
        private readonly XmlDocument _objCritterDocument = new XmlDocument();

        #region Control Events
        public frmSelectLifestyleQuality(Character objCharacter, string strSelectedLifestyle)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _strSelectedLifestyle = strSelectedLifestyle;

            _objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
            _objCritterDocument = XmlManager.Instance.Load("critters.xml");

            MoveControls();
        }

        private void frmSelectLifestyleQuality_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Load the Quality information.
            _objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

            // Populate the Quality Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                _lstCategory.Add(objItem);
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            // Change the BP Label to Karma if the character is being built with Karma instead (or is in Career Mode).
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma || _objCharacter.BuildMethod == CharacterBuildMethod.Priority || _objCharacter.Created)
                lblBPLabel.Text = LanguageManager.Instance.GetString("Label_LP");

            BuildQualityList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void lstLifestyleQualities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstLifestyleQualities.Text))
                return;

            XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + lstLifestyleQualities.SelectedValue + "\"]");
            int intBP = Convert.ToInt32(objXmlQuality["lp"].InnerText);
            lblBP.Text = intBP.ToString();
            if (chkFree.Checked)
                lblBP.Text = LanguageManager.Instance.GetString("Checkbox_Free");

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlQuality["source"].InnerText);
            string strPage = objXmlQuality["page"].InnerText;
            if (objXmlQuality["altpage"] != null)
                strPage = objXmlQuality["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;
            if (objXmlQuality["allowed"] != null)
            {
                lblMinimum.Text = GetMinimumRequirement(objXmlQuality["allowed"].InnerText);
                lblMinimum.Visible = true;
                lblMinimumLabel.Visible = true;
            }
            else
            {
                lblMinimum.Visible = false;
                lblMinimumLabel.Visible = false;
            }
            if (objXmlQuality["cost"] != null)
            {
                if (chkFree.Checked)
                {
                    lblCost.Text = LanguageManager.Instance.GetString("Checkbox_Free");
                }
                else if (objXmlQuality["allowed"]?.InnerText.Contains(_strSelectedLifestyle) == true)
                {
                    lblCost.Text = LanguageManager.Instance.GetString("String_LifestyleFreeNuyen");
                }
                else
                {
                lblCost.Text = String.Format("{0:###,###,##0¥}", Convert.ToInt32(objXmlQuality["cost"].InnerText));
                }
                lblCost.Visible = true;
                lblCostLabel.Visible = true;
            }
            else
            {
                lblCost.Visible = false;
                lblCostLabel.Visible = false;
            }
            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlQuality["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }

        private string GetMinimumRequirement(string strAllowedLifestyles)
        {
            if (_strLifestyleSpecific.Contains(strAllowedLifestyles))
            {
                return strAllowedLifestyles;
            }
            int intMin = int.MaxValue;
            foreach (string strLifesytle in strAllowedLifestyles.Split(','))
            {
                if (_lstLifestylesSorted.Contains(strLifesytle) && _lstLifestylesSorted.IndexOf(strLifesytle) < intMin)
                {
                    intMin = _lstLifestylesSorted.IndexOf(strLifesytle);
                }
            }
            return _lstLifestylesSorted[intMin];
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstLifestyleQualities_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            lstLifestyleQualities_SelectedIndexChanged(sender, e);
        }

        private void chkMetagenetic_CheckedChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstLifestyleQualities.SelectedIndex + 1 < lstLifestyleQualities.Items.Count)
                {
                    lstLifestyleQualities.SelectedIndex++;
                }
                else if (lstLifestyleQualities.Items.Count > 0)
                    {
                        lstLifestyleQualities.SelectedIndex = 0;
                    }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstLifestyleQualities.SelectedIndex - 1 >= 0)
                {
                    lstLifestyleQualities.SelectedIndex--;
                }
                else if (lstLifestyleQualities.Items.Count > 0)
                    {
                        lstLifestyleQualities.SelectedIndex = lstLifestyleQualities.Items.Count - 1;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Quality that was selected in the dialogue.
        /// </summary>
        public string SelectedQuality
        {
            get
            {
                return _strSelectedQuality;
            }
        }

        /// <summary>
        /// Forcefully add a Category to the list.
        /// </summary>
        public string ForceCategory
        {
            set
            {
                cboCategory.BeginUpdate();
                cboCategory.DataSource = null;
                cboCategory.Items.Add(value);
                cboCategory.EndUpdate();
            }
        }

        /// <summary>
        /// A Quality the character has that should be ignored for checking Fobidden requirements (which would prevent upgrading/downgrading a Quality).
        /// </summary>
        public string IgnoreQuality
        {
            set
            {
                _strIgnoreQuality = value;
            }
        }

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
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost
        {
            get
            {
                return chkFree.Checked;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Qualities.
        /// </summary>
        private void BuildQualityList()
        {
            List<ListItem> lstLifestyleQuality = new List<ListItem>();
            if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                // Treat everything as being uppercase so the search is case-insensitive.
                string strSearch = "/chummer/qualities/quality[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
                strSearch += "]";

                XmlNodeList objXmlQualityList = _objXmlDocument.SelectNodes(strSearch);
                foreach (XmlNode objXmlQuality in objXmlQualityList)
                {
                    if (_strSelectedLifestyle != "Bolt Hole" && objXmlQuality["name"].InnerText == "Dug a Hole")
                    {
                        continue;
                    }
                    if (objXmlQuality["hide"] == null)
                    {
                        if (!chkLimitList.Checked || (chkLimitList.Checked && RequirementMet(objXmlQuality, false)))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlQuality["name"].InnerText;
                            objItem.Name = objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"].InnerText;

                            if (objXmlQuality["category"] != null)
                            {
                                ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == objXmlQuality["category"].InnerText);

                                if (objFoundItem != null)
                                {
                                    objItem.Name += " [" + objFoundItem.Name + "]";
                            }
                            }

                            lstLifestyleQuality.Add(objItem);
                        }
                    }
                }
            }
            else
            {
                string strXPath = "category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")";

                foreach (XmlNode objXmlQuality in _objXmlDocument.SelectNodes("/chummer/qualities/quality[" + strXPath + "]"))
                {
                    if (_strSelectedLifestyle != "Bolt Hole" && objXmlQuality["name"].InnerText == "Dug a Hole")
                    {
                        continue;
                    }
                            if (!chkLimitList.Checked || (chkLimitList.Checked && RequirementMet(objXmlQuality, false)))
                            {
                                if (objXmlQuality["hide"] == null)
                                {
                                    ListItem objItem = new ListItem();
                                    objItem.Value = objXmlQuality["name"].InnerText;
                                    objItem.Name = objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"].InnerText;

                                    lstLifestyleQuality.Add(objItem);
                                }
                            }
                        }
                    }
            SortListItem objSort = new SortListItem();
            lstLifestyleQuality.Sort(objSort.Compare);
            lstLifestyleQualities.BeginUpdate();
            lstLifestyleQualities.DataSource = null;
            lstLifestyleQualities.ValueMember = "Value";
            lstLifestyleQualities.DisplayMember = "Name";
            lstLifestyleQualities.DataSource = lstLifestyleQuality;
            lstLifestyleQualities.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(lstLifestyleQualities.Text))
                return;
            _objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
            XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + lstLifestyleQualities.SelectedValue + "\"]");
            _strSelectedQuality = objNode["name"].InnerText;
            _strSelectCategory = objNode["category"].InnerText;

            if (!RequirementMet(objNode, true))
                return;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Check if the Quality's requirements/restrictions are being met.
        /// </summary>
        /// <param name="objXmlQuality">XmlNode of the Quality.</param>
        /// <param name="blnShowMessage">Whether or not a message should be shown if the requirements are not met.</param>
        private bool RequirementMet(XmlNode objXmlQuality, bool blnShowMessage)
        {
            // Ignore the rules.
            if (_objCharacter.IgnoreRules)
                return true;

            // See if the character already has this Quality and whether or not multiple copies are allowed.
            bool blnAllowMultiple = false;
            if (objXmlQuality["limit"] != null)
            {
                if (objXmlQuality["limit"].InnerText == "no")
                    blnAllowMultiple = true;
            }
            if (!blnAllowMultiple)
            {
                // Multiples aren't allowed, so make sure the character does not already have it.
                foreach (LifestyleQuality objQuality in _objCharacter.LifestyleQualities)
                {
                    if (objQuality.Name == objXmlQuality["name"].InnerText)
                    {
                        if (blnShowMessage)
                            MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectQuality_QualityLimit"), LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
            }

            if (objXmlQuality.InnerXml.Contains("forbidden"))
            {
                bool blnRequirementForbidden = false;
                string strForbidden = string.Empty;

                // Loop through the oneof requirements.
                XmlNodeList objXmlForbiddenList = objXmlQuality.SelectNodes("forbidden/oneof");
                foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
                {
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

                    foreach (XmlNode objXmlForbidden in objXmlOneOfList)
                    {
                        switch (objXmlForbidden.Name)
                        {
                            case "quality":
                                // Run through all of the Qualities the character has and see if the current forbidden item exists.
                                // If so, turn on the RequirementForbidden flag so it cannot be selected.
                                foreach (LifestyleQuality objQuality in _objCharacter.LifestyleQualities)
                                {
                                    if (objQuality.Name == objXmlForbidden.InnerText && objQuality.Name != _strIgnoreQuality)
                                    {
                                        blnRequirementForbidden = true;
                                        strForbidden += "\n\t" + objQuality.DisplayNameShort;
                                    }
                                }
                                break;
                            case "characterquality":
                                // Run through all of the Qualities the character has and see if the current forbidden item exists.
                                // If so, turn on the RequirementForbidden flag so it cannot be selected.
                                foreach (Quality objQuality in _objCharacter.Qualities)
                                {
                                    if (objQuality.Name == objXmlForbidden.InnerText && objQuality.Name != _strIgnoreQuality)
                                    {
                                        blnRequirementForbidden = true;
                                        strForbidden += "\n\t" + objQuality.DisplayNameShort;
                                    }
                                }
                                break;
                            case "metatype":
                                // Check the Metatype restriction.
                                if (objXmlForbidden.InnerText == _objCharacter.Metatype)
                                {
                                    blnRequirementForbidden = true;
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlForbidden.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlForbidden.InnerText + "\"]");
                                    strForbidden += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlForbidden.InnerText;
                                }
                                break;
                            case "metatypecategory":
                                // Check the Metatype Category restriction.
                                if (objXmlForbidden.InnerText == _objCharacter.MetatypeCategory)
                                {
                                    blnRequirementForbidden = true;
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlForbidden.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlForbidden.InnerText + "\"]");
                                    strForbidden += objNode.Attributes["translate"] != null
                                        ? "\n\t" + objNode.Attributes["translate"].InnerText
                                        : "\n\t" + objXmlForbidden.InnerText;
                                }
                                break;
                            case "metavariant":
                                // Check the Metavariant restriction.
                                if (objXmlForbidden.InnerText == _objCharacter.Metavariant)
                                {
                                    blnRequirementForbidden = true;
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlForbidden.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlForbidden.InnerText + "\"]");
                                    strForbidden += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlForbidden.InnerText;
                                }
                                break;
                            case "metagenetic":
                                // Check to see if the character has a Metagenetic Quality.
                                foreach (Quality objQuality in _objCharacter.Qualities)
                                {
                                    XmlNode objXmlCheck = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objQuality.Name + "\"]");
                                    if (objXmlCheck["metagenetic"] != null)
                                    {
                                        if (objXmlCheck["metagenetic"].InnerText.ToLower() == "yes")
                                        {
                                            blnRequirementForbidden = true;
                                            strForbidden += "\n\t" + objQuality.DisplayName;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                // The character is not allowed to take the Quality, so display a message and uncheck the item.
                if (blnRequirementForbidden)
                {
                    if (blnShowMessage)
                        MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectQuality_QualityRestriction") + strForbidden, LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityRestriction"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            if (objXmlQuality.InnerXml.Contains("required"))
            {
                string strRequirement = string.Empty;
                bool blnRequirementMet = true;

                // Loop through the oneof requirements.
                XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/oneof");
                XmlDocument _objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");
                foreach (XmlNode objXmlOneOf in objXmlRequiredList)
                {
                    bool blnOneOfMet = false;
                    string strThisRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_OneOf");
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlOneOfList)
                    {
                        switch (objXmlRequired.Name)
                        {
                            case "quality":
                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (LifestyleQuality objQuality in _objCharacter.LifestyleQualities)
                                {
                                    if (objQuality.Name == objXmlRequired.InnerText)
                                        blnOneOfMet = true;
                                }

                                if (!blnOneOfMet)
                                {
                                    XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "characterquality":

                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (Quality objQuality in _objCharacter.Qualities)
                                {
                                    if (objQuality.Name == objXmlRequired.InnerText)
                                        blnOneOfMet = true;
                                }

                                if (!blnOneOfMet)
                                {
                                    XmlNode objNode = _objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "metatype":
                                // Check the Metatype requirement.
                                if (objXmlRequired.InnerText == _objCharacter.Metatype)
                                    blnOneOfMet = true;
                                else
                                {
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "metatypecategory":
                                // Check the Metatype Category requirement.
                                if (objXmlRequired.InnerText == _objCharacter.MetatypeCategory)
                                    blnOneOfMet = true;
                                else
                                {
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlRequired.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "metavariant":
                                // Check the Metavariant requirement.
                                if (objXmlRequired.InnerText == _objCharacter.Metavariant)
                                    blnOneOfMet = true;
                                else
                                {
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "inherited":
                                strThisRequirement += "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_Inherit");
                                break;
                            case "careerkarma":
                                // Check Career Karma requirement.
                                if (_objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText))
                                    blnOneOfMet = true;
                                else
                                    strThisRequirement = "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_RequireKarma").Replace("{0}", objXmlRequired.InnerText);
                                break;
                            case "ess":
                                // Check Essence requirement.
                                if (objXmlRequired.InnerText.StartsWith("-"))
                                {
                                    // Essence must be less than the value.
                                    if (_objCharacter.Essence < Convert.ToDecimal(objXmlRequired.InnerText.Replace("-", string.Empty), GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                }
                                else
                                {
                                    // Essence must be equal to or greater than the value.
                                    if (_objCharacter.Essence >= Convert.ToDecimal(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                }
                                break;
                            case "skill":
                                // Check if the character has the required Skill.
                                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                                {
                                    if (objSkill.Name == objXmlRequired["name"].InnerText)
                                    {
                                        if (objSkill.Rating >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                        {
                                            blnOneOfMet = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "attribute":
                                // Check to see if an Attribute meets a requirement.
                                CharacterAttrib objAttribute = _objCharacter.GetAttribute(objXmlRequired["name"].InnerText);

                                if (objXmlRequired["total"] != null)
                                {
                                    // Make sure the Attribute's total value meets the requirement.
                                    if (objAttribute.TotalValue >= Convert.ToInt32(objXmlRequired["total"].InnerText))
                                        blnOneOfMet = true;
                                }
                                break;
                            case "attributetotal":
                                // Check if the character's Attributes add up to a particular total.
                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                foreach (string strAttribute in Character.AttributeStrings)
                                {
                                    strAttributes = strAttributes.Replace(strAttribute, _objCharacter.GetAttribute(strAttribute).Value.ToString());
                                }

                                XmlDocument objXmlDocument = new XmlDocument();
                                XPathNavigator nav = objXmlDocument.CreateNavigator();
                                XPathExpression xprAttributes = nav.Compile(strAttributes);
                                if (Convert.ToInt32(nav.Evaluate(xprAttributes)) >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnOneOfMet = true;
                                break;
                            case "skillgrouptotal":
                            {
                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                int intTotal = 0;
                                string[] strGroups = objXmlRequired["skillgroups"].InnerText.Split('+');
                                for (int i = 0; i <= strGroups.Length - 1; i++)
                                {
                                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroups[i])
                                        {
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnOneOfMet = true;
                            }
                                break;
                            case "lifestyle":
                                if (_strSelectedLifestyle == objXmlRequired.InnerText)
                                    blnOneOfMet = true;
                                break;
                            case "cyberwares":
                            {
                                // Check to see if the character has a number of the required Cyberware/Bioware items.
                                int intTotal = 0;

                                // Check Cyberware.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberware"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware.Where(objCyberware => (objCyberware.Name == objXmlCyberware.InnerText)))
                                    {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                    }
                                }

                                // Check Bioware.
                                foreach (XmlNode objXmlBioware in objXmlRequired.SelectNodes("bioware"))
                                {
                                    if (_objCharacter.Cyberware.Any(objCyberware => objCyberware.Name == objXmlBioware.InnerText))
                                    {
                                        intTotal++;
                                    }
                                }

                                // Check Cyberware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecontains"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware.Where(objCyberware => objCyberware.Name.Contains(objXmlCyberware.InnerText)))
                                    {
                                        if (objXmlCyberware.Attributes["select"] == null)
                                        {
                                            intTotal++;
                                            break;
                                        }
                                        if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
                                        {
                                            intTotal++;
                                            break;
                                        }
                                    }
                                }

                                // Check Bioware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("biowarecontains"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                        {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Check for Cyberware Plugins.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwareplugin"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Children.Any(objPlugin => objPlugin.Name == objXmlCyberware.InnerText))
                                        {
                                            intTotal++;
                                        }
                                    }
                                }

                                // Check for Cyberware Categories.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecategory"))
                                {
                                    intTotal += _objCharacter.Cyberware.Count(objCyberware => objCyberware.Category == objXmlCyberware.InnerText);
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText))
                                    blnOneOfMet = true;
                            }
                                break;
                            case "streetcredvsnotoriety":
                                // Street Cred must be higher than Notoriety.
                                if (_objCharacter.StreetCred >= _objCharacter.Notoriety)
                                    blnOneOfMet = true;
                                break;
                            case "damageresistance":
                                // Damage Resistance must be a particular value.
                                ImprovementManager _objImprovementManager = new ImprovementManager(_objCharacter);
                                if (_objCharacter.BOD.TotalValue + _objImprovementManager.ValueOf(Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(objXmlRequired.InnerText))
                                    blnOneOfMet = true;
                                break;
                        }
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnOneOfMet;
                    strRequirement += strThisRequirement;
                }

                // Loop through the allof requirements.
                objXmlRequiredList = objXmlQuality.SelectNodes("required/allof");
                foreach (XmlNode objXmlAllOf in objXmlRequiredList)
                {
                    bool blnAllOfMet = true;
                    string strThisRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_AllOf");
                    XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlAllOfList)
                    {
                        bool blnFound = false;
                        switch (objXmlRequired.Name)
                        {
                            case "quality":

                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (LifestyleQuality objQuality in _objCharacter.LifestyleQualities)
                                {
                                    if (objQuality.Name == objXmlRequired.InnerText)
                                        blnFound = true;
                                }

                                if (!blnFound)
                                {
                                    XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "metatype":
                                // Check the Metatype requirement.
                                if (objXmlRequired.InnerText == _objCharacter.Metatype)
                                    blnFound = true;
                                else
                                {
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "metatypecategory":
                                // Check the Metatype Category requirement.
                                if (objXmlRequired.InnerText == _objCharacter.MetatypeCategory)
                                    blnFound = true;
                                else
                                {
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlRequired.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "metavariant":
                                // Check the Metavariant requirement.
                                if (objXmlRequired.InnerText == _objCharacter.Metavariant)
                                    blnFound = true;
                                else
                                {
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;
                            case "inherited":
                                strThisRequirement += "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_Inherit");
                                break;
                            case "careerkarma":
                                // Check Career Karma requirement.
                                if (_objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText))
                                    blnFound = true;
                                else
                                    strThisRequirement = "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_RequireKarma").Replace("{0}", objXmlRequired.InnerText);
                                break;
                            case "ess":
                                // Check Essence requirement.
                                if (objXmlRequired.InnerText.StartsWith("-"))
                                {
                                    // Essence must be less than the value.
                                    if (_objCharacter.Essence < Convert.ToDecimal(objXmlRequired.InnerText.Replace("-", string.Empty), GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                else
                                {
                                    // Essence must be equal to or greater than the value.
                                    if (_objCharacter.Essence >= Convert.ToDecimal(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                break;
                            case "skill":
                                // Check if the character has the required Skill.
                                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills.Where(objSkill => objSkill.Name == objXmlRequired["name"].InnerText))
                                {
                                    if (objSkill.Rating >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    {
                                        blnFound = true;
                                        break;
                                    }
                                }
                                break;
                            case "attribute":
                                // Check to see if an Attribute meets a requirement.
                                CharacterAttrib objAttribute = _objCharacter.GetAttribute(objXmlRequired["name"].InnerText);

                                if (objXmlRequired["total"] != null)
                                {
                                    // Make sure the Attribute's total value meets the requirement.
                                    if (objAttribute.TotalValue >= Convert.ToInt32(objXmlRequired["total"].InnerText))
                                        blnFound = true;
                                }
                                break;
                            case "attributetotal":
                                // Check if the character's Attributes add up to a particular total.
                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                foreach (string strAttribute in Character.AttributeStrings)
                                {
                                    strAttributes = strAttributes.Replace(strAttribute, _objCharacter.GetAttribute(strAttribute).Value.ToString());
                                }

                                XmlDocument objXmlDocument = new XmlDocument();
                                XPathNavigator nav = objXmlDocument.CreateNavigator();
                                XPathExpression xprAttributes = nav.Compile(strAttributes);
                                if (Convert.ToInt32(nav.Evaluate(xprAttributes)) >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnFound = true;
                                break;
                            case "skillgrouptotal":
                            {
                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                int intTotal = 0;
                                string[] strGroups = objXmlRequired["skillgroups"].InnerText.Split('+');
                                for (int i = 0; i <= strGroups.Length - 1; i++)
                                {
                                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroups[i])
                                        {
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnFound = true;
                            }
                                break;
                            case "lifestyle":
                                if (_strSelectedLifestyle == objXmlRequired.InnerText)
                                    blnFound = true;
                                break;
                            case "cyberwares":
                            {
                                // Check to see if the character has a number of the required Cyberware/Bioware items.
                                int intTotal = 0;

                                // Check Cyberware.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberware"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Name == objXmlCyberware.InnerText)
                                        {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Check Bioware.
                                foreach (XmlNode objXmlBioware in objXmlRequired.SelectNodes("bioware"))
                                {
                                    if (_objCharacter.Cyberware.Any(objCyberware => objCyberware.Name == objXmlBioware.InnerText))
                                    {
                                        intTotal++;
                                    }
                                }

                                // Check Cyberware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecontains"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                        {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Check Bioware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("biowarecontains"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                        {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Check for Cyberware Plugins.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwareplugin"))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Children.Any(objPlugin => objPlugin.Name == objXmlCyberware.InnerText))
                                        {
                                            intTotal++;
                                        }
                                    }
                                }

                                // Check for Cyberware Categories.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecategory"))
                                {
                                    intTotal += _objCharacter.Cyberware.Count(objCyberware => objCyberware.Category == objXmlCyberware.InnerText);
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText))
                                    blnFound = true;
                            }
                                break;
                            case "streetcredvsnotoriety":
                                // Street Cred must be higher than Notoriety.
                                if (_objCharacter.StreetCred >= _objCharacter.Notoriety)
                                    blnFound = true;
                                break;
                            case "damageresistance":
                                // Damage Resistance must be a particular value.
                                ImprovementManager _objImprovementManager = new ImprovementManager(_objCharacter);
                                if (_objCharacter.BOD.TotalValue + _objImprovementManager.ValueOf(Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(objXmlRequired.InnerText))
                                    blnFound = true;
                                break;
                        }

                        // If this item was not found, fail the AllOfMet condition.
                        if (!blnFound)
                            blnAllOfMet = false;
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnAllOfMet;
                    strRequirement += strThisRequirement;
                }

                // The character has not met the requirements, so display a message and uncheck the item.
                if (!blnRequirementMet)
                {
                    string strMessage = LanguageManager.Instance.GetString("Message_SelectQuality_QualityRequirement");
                    strMessage += strRequirement;

                    if (blnShowMessage)
                        MessageBox.Show(strMessage, LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityRequirement"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            return true;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblBPLabel.Width, lblSourceLabel.Width);
            lblBP.Left = lblBPLabel.Left + intWidth + 6;
            lblSource.Left = lblSourceLabel.Left + intWidth + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
