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
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using Chummer.Backend.Attributes;
using System.Text;

namespace Chummer
{
    public partial class frmSelectLifestyleQuality : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain;
        private readonly Character _objCharacter;
        private string _strIgnoreQuality = string.Empty;
        private readonly string _strSelectedLifestyle;
        private readonly IReadOnlyCollection<LifestyleQuality> _lstExistingQualities;

        private readonly XmlDocument _objXmlDocument;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private static readonly List<string> s_LstLifestylesSorted = new List<string>(new [] {"Street", "Squatter", "Low", "Medium", "High", "Luxury"});
        private static readonly string[] s_StrLifestyleSpecific = { "Bolt Hole", "Traveler", "Commercial", "Hospitalized" };

        private static string s_StrSelectCategory = string.Empty;

        private readonly XmlDocument _objMetatypeDocument;
        private readonly XmlDocument _objCritterDocument;

        #region Control Events
        public frmSelectLifestyleQuality(Character objCharacter, string strSelectedLifestyle, IReadOnlyCollection<LifestyleQuality> lstExistingQualities)
        {
            InitializeComponent();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _strSelectedLifestyle = strSelectedLifestyle;
            _lstExistingQualities = lstExistingQualities;

            // Load the Quality information.
            _objXmlDocument = XmlManager.Load("lifestyles.xml");
            _objMetatypeDocument = XmlManager.Load("metatypes.xml");
            _objCritterDocument = XmlManager.Load("critters.xml");
        }

        private void frmSelectLifestyleQuality_Load(object sender, EventArgs e)
        {
            // Populate the Quality Category list.
            using (XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category"))
                if (objXmlCategoryList?.Count > 0)
                    foreach (XmlNode objXmlCategory in objXmlCategoryList)
                    {
                        string strCategory = objXmlCategory.InnerText;
                        if (BuildQualityList(strCategory, false, true).Count > 0)
                        {
                            _lstCategory.Add(new ListItem(strCategory, objXmlCategory.Attributes?["translate"]?.InnerText ?? strCategory));
                        }
                    }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = nameof(ListItem.Value);
            cboCategory.DisplayMember = nameof(ListItem.Name);
            cboCategory.DataSource = _lstCategory;
            cboCategory.Enabled = _lstCategory.Count > 1;

            if (!string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            // Change the BP Label to Karma if the character is being built with Karma instead (or is in Career Mode).
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma || _objCharacter.BuildMethod == CharacterBuildMethod.LifeModule || _objCharacter.Created)
                lblBPLabel.Text = LanguageManager.GetString("Label_LP");

            _blnLoading = false;

            BuildQualityList(cboCategory.SelectedValue?.ToString());
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                BuildQualityList(cboCategory.SelectedValue?.ToString());
        }

        private void lstLifestyleQualities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strSelectedLifestyleId = lstLifestyleQualities.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedLifestyleId))
            {
                lblMinimum.Visible = false;
                lblMinimumLabel.Visible = false;
                lblCost.Visible = false;
                lblCostLabel.Visible = false;
                lblBP.Text = string.Empty;
                lblBPLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                lblSourceLabel.Visible = false;
                return;
            }

            XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + strSelectedLifestyleId + "\"]");
            if (objXmlQuality == null)
            {
                lblMinimum.Visible = false;
                lblMinimumLabel.Visible = false;
                lblCost.Visible = false;
                lblCostLabel.Visible = false;
                lblBP.Text = string.Empty;
                lblBPLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                lblSourceLabel.Visible = false;
                return;
            }

            int intBP = 0;
            objXmlQuality.TryGetInt32FieldQuickly("lp", ref intBP);
            lblBP.Text = chkFree.Checked ? LanguageManager.GetString("Checkbox_Free") : intBP.ToString(GlobalOptions.CultureInfo);
            lblBPLabel.Visible = !string.IsNullOrEmpty(lblBP.Text);

            string strSource = objXmlQuality["source"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
            string strPage = objXmlQuality["altpage"]?.InnerText ?? objXmlQuality["page"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
            if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
            {
                string strSpace = LanguageManager.GetString("String_Space");
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource) + strSpace + strPage;
                lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
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
                    lblCost.Text = LanguageManager.GetString("Checkbox_Free");
                }
                else if (objXmlQuality["allowed"]?.InnerText.Contains(_strSelectedLifestyle) == true)
                {
                    lblCost.Text = LanguageManager.GetString("String_LifestyleFreeNuyen");
                }
                else
                {
                    string strCost = objXmlQuality["cost"]?.InnerText;
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                    decimal decCost = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
                    lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                }
                lblCost.Visible = true;
                lblCostLabel.Visible = true;
            }
            else
            {
                lblCost.Visible = false;
                lblCostLabel.Visible = false;
            }
        }

        private static string GetMinimumRequirement(string strAllowedLifestyles)
        {
            if (s_StrLifestyleSpecific.Contains(strAllowedLifestyles))
            {
                return strAllowedLifestyles;
            }
            int intMin = int.MaxValue;
            foreach (string strLifesytle in strAllowedLifestyles.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (s_LstLifestylesSorted.Contains(strLifesytle) && s_LstLifestylesSorted.IndexOf(strLifesytle) < intMin)
                {
                    intMin = s_LstLifestylesSorted.IndexOf(strLifesytle);
                }
            }
            return s_LstLifestylesSorted[intMin];
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
            if (!_blnLoading)
                BuildQualityList(cboCategory.SelectedValue?.ToString());
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                BuildQualityList(cboCategory.SelectedValue?.ToString());
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                BuildQualityList(cboCategory.SelectedValue?.ToString());
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstLifestyleQualities.SelectedIndex + 1 < lstLifestyleQualities.Items.Count)
                {
                    lstLifestyleQualities.SelectedIndex += 1;
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
                    lstLifestyleQualities.SelectedIndex -= 1;
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
        public string SelectedQuality => _strSelectedQuality;

        /// <summary>
        /// Forcefully add a Category to the list.
        /// </summary>
        public string ForceCategory
        {
            set
            {
                cboCategory.BeginUpdate();
                cboCategory.SelectedValue = value;
                cboCategory.Enabled = false;
                cboCategory.EndUpdate();
            }
        }

        /// <summary>
        /// A Quality the character has that should be ignored for checking Fobidden requirements (which would prevent upgrading/downgrading a Quality).
        /// </summary>
        public string IgnoreQuality
        {
            set => _strIgnoreQuality = value;
        }

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Qualities.
        /// </summary>
        private List<ListItem> BuildQualityList(string strCategory, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            string strFilter = "(" + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || string.IsNullOrWhiteSpace(txtSearch.Text)))
            {
                strFilter += " and category = \"" + strCategory + '\"';
            }
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                }
            }

            if (_strSelectedLifestyle != "Bolt Hole")
            {
                strFilter += " and (name != \"Dug a Hole\")";
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            List<ListItem> lstLifestyleQuality = new List<ListItem>();
            using (XmlNodeList objXmlQualityList = _objXmlDocument.SelectNodes("/chummer/qualities/quality[" + strFilter + "]"))
                if (objXmlQualityList?.Count > 0)
                    foreach (XmlNode objXmlQuality in objXmlQualityList)
                    {
                        string strId = objXmlQuality["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strId))
                            continue;
                        if (!blnDoUIUpdate || !chkLimitList.Checked || RequirementMet(objXmlQuality, false))
                        {
                            lstLifestyleQuality.Add(new ListItem(strId, objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown")));
                            if (blnTerminateAfterFirst)
                                break;
                        }
                    }
            if (blnDoUIUpdate)
            {
                lstLifestyleQuality.Sort(CompareListItems.CompareNames);

                string strOldSelectedQuality = lstLifestyleQualities.SelectedValue?.ToString();
                _blnLoading = true;
                lstLifestyleQualities.BeginUpdate();
                lstLifestyleQualities.ValueMember = nameof(ListItem.Value);
                lstLifestyleQualities.DisplayMember = nameof(ListItem.Name);
                lstLifestyleQualities.DataSource = lstLifestyleQuality;
                _blnLoading = false;
                if (string.IsNullOrEmpty(strOldSelectedQuality))
                    lstLifestyleQualities.SelectedIndex = -1;
                else
                    lstLifestyleQualities.SelectedValue = strOldSelectedQuality;

                lstLifestyleQualities.EndUpdate();
            }

            return lstLifestyleQuality;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedSourceIDString = lstLifestyleQualities.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedSourceIDString))
                return;
            XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + strSelectedSourceIDString + "\"]");
            if (objNode == null || !RequirementMet(objNode, true))
                return;

            _strSelectedQuality = strSelectedSourceIDString;
            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;

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
            if (objXmlQuality["limit"]?.InnerText != bool.FalseString)
            {
                // Multiples aren't allowed, so make sure the character does not already have it.
                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                {
                    if (objXmlQuality["allowmultiple"] == null && objQuality.Name == objXmlQuality["name"].InnerText)
                    {
                        if (blnShowMessage)
                            Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_SelectQuality_QualityLimit"), LanguageManager.GetString("MessageTitle_SelectQuality_QualityLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                                {
                                    if (objQuality.Name == objXmlForbidden.InnerText && objQuality.Name != _strIgnoreQuality)
                                    {
                                        blnRequirementForbidden = true;
                                        strForbidden += Environment.NewLine + '\t' + objQuality.DisplayNameShort(GlobalOptions.Language);
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
                                        strForbidden += Environment.NewLine + '\t' + objQuality.DisplayNameShort(GlobalOptions.Language);
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
                                    strForbidden += Environment.NewLine + '\t' + (objNode["translate"]?.InnerText ?? objXmlForbidden.InnerText);
                                }
                                break;
                            case "metatypecategory":
                                // Check the Metatype Category restriction.
                                if (objXmlForbidden.InnerText == _objCharacter.MetatypeCategory)
                                {
                                    blnRequirementForbidden = true;
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlForbidden.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlForbidden.InnerText + "\"]");
                                    strForbidden += Environment.NewLine + '\t' + (objNode.Attributes["translate"]?.InnerText ?? objXmlForbidden.InnerText);
                                }
                                break;
                            case "metavariant":
                                // Check the Metavariant restriction.
                                if (objXmlForbidden.InnerText == _objCharacter.Metavariant)
                                {
                                    blnRequirementForbidden = true;
                                    XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlForbidden.InnerText + "\"]") ??
                                                      _objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlForbidden.InnerText + "\"]");
                                    strForbidden += Environment.NewLine + '\t' + (objNode["translate"]?.InnerText ?? objXmlForbidden.InnerText);
                                }
                                break;
                            case "metagenic":
                                // Check to see if the character has a Metagenic Quality.
                                foreach (Quality objQuality in _objCharacter.Qualities)
                                {
                                    XmlNode objXmlCheck = objQuality.GetNode();
                                    if (objXmlCheck["metagenic"]?.InnerText == bool.TrueString)
                                    {
                                        blnRequirementForbidden = true;
                                        strForbidden += Environment.NewLine + '\t' + objQuality.CurrentDisplayName;
                                        break;
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
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_SelectQuality_QualityRestriction") + strForbidden, LanguageManager.GetString("MessageTitle_SelectQuality_QualityRestriction"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            if (objXmlQuality.InnerXml.Contains("required"))
            {
                string strRequirement = string.Empty;
                bool blnRequirementMet = true;

                // Loop through the oneof requirements.
                XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/oneof");
                XmlDocument _objXmlQualityDocument = XmlManager.Load("qualities.xml");
                foreach (XmlNode objXmlOneOf in objXmlRequiredList)
                {
                    bool blnOneOfMet = false;
                    string strThisRequirement = Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_OneOf");
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlOneOfList)
                    {
                        switch (objXmlRequired.Name)
                        {
                            case "quality":
                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                                {
                                    if (objQuality.Name == objXmlRequired.InnerText)
                                        blnOneOfMet = true;
                                }

                                if (!blnOneOfMet)
                                {
                                    XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? Environment.NewLine + '\t' + objNode["translate"].InnerText
                                        : Environment.NewLine + '\t' + objXmlRequired.InnerText;
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
                                        ? Environment.NewLine + '\t' + objNode["translate"].InnerText
                                        : Environment.NewLine + '\t' + objXmlRequired.InnerText;
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
                                        ? Environment.NewLine + '\t' + objNode["translate"].InnerText
                                        : Environment.NewLine + '\t' + objXmlRequired.InnerText;
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
                                        ? Environment.NewLine + '\t' + objNode["translate"].InnerText
                                        : Environment.NewLine + '\t' + objXmlRequired.InnerText;
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
                                        ? Environment.NewLine + '\t' + objNode["translate"].InnerText
                                        : Environment.NewLine + '\t' + objXmlRequired.InnerText;
                                }
                                break;
                            case "inherited":
                                strThisRequirement += Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectQuality_Inherit");
                                break;
                            case "careerkarma":
                                // Check Career Karma requirement.
                                if (_objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
                                    blnOneOfMet = true;
                                else
                                    strThisRequirement = Environment.NewLine + '\t' + string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_SelectQuality_RequireKarma"), objXmlRequired.InnerText);
                                break;
                            case "ess":
                                // Check Essence requirement.
                                if (objXmlRequired.InnerText.StartsWith('-'))
                                {
                                    // Essence must be less than the value.
                                    if (_objCharacter.Essence() < Convert.ToDecimal(objXmlRequired.InnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                }
                                else
                                {
                                    // Essence must be equal to or greater than the value.
                                    if (_objCharacter.Essence() >= Convert.ToDecimal(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                }
                                break;
                            case "skill":
                                // Check if the character has the required Skill.
                                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(objXmlRequired["name"].InnerText);
                                if ((objSkill?.Rating ?? 0) >= Convert.ToInt32(objXmlRequired["val"].InnerText, GlobalOptions.InvariantCultureInfo))
                                {
                                    blnOneOfMet = true;
                                }
                                break;
                            case "attribute":
                                // Check to see if an Attribute meets a requirement.
                                CharacterAttrib objAttribute = _objCharacter.GetAttribute(objXmlRequired["name"].InnerText);

                                if (objXmlRequired["total"] != null)
                                {
                                    // Make sure the Attribute's total value meets the requirement.
                                    if (objAttribute.TotalValue >= Convert.ToInt32(objXmlRequired["total"].InnerText, GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                }
                                break;
                            case "attributetotal":
                                // Check if the character's Attributes add up to a particular total.
                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                foreach (string strAttribute in AttributeSection.AttributeStrings)
                                {
                                    strAttributes = strAttributes.CheapReplace(strAttribute, () => _objCharacter.GetAttribute(strAttribute).Value.ToString(GlobalOptions.InvariantCultureInfo));
                                }

                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAttributes, out bool blnIsSuccess);
                                if ((blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) : 0) >= Convert.ToInt32(objXmlRequired["val"].InnerText, GlobalOptions.InvariantCultureInfo))
                                    blnOneOfMet = true;
                                break;
                            case "skillgrouptotal":
                            {
                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                int intTotal = 0;
                                foreach (string strGroup in objXmlRequired["skillgroups"].InnerText.SplitNoAlloc('+', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroup)
                                        {
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText, GlobalOptions.InvariantCultureInfo))
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
                                        if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Extra)
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
                                        if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Extra)
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
                                            if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Extra)
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

                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText, GlobalOptions.InvariantCultureInfo))
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
                                if (_objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
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
                    string strThisRequirement = Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_AllOf");
                    XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlAllOfList)
                    {
                        bool blnFound = false;
                        switch (objXmlRequired.Name)
                        {
                            case "quality":

                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                                {
                                    if (objQuality.Name == objXmlRequired.InnerText)
                                        blnFound = true;
                                }

                                if (!blnFound)
                                {
                                    XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlRequired.InnerText + "\"]/translate");
                                    strThisRequirement += Environment.NewLine + '\t' + (objNode?.InnerText ?? objXmlRequired.InnerText);
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
                                    strThisRequirement += Environment.NewLine + '\t' + (objNode["translate"]?.InnerText ?? objXmlRequired.InnerText);
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
                                    strThisRequirement += Environment.NewLine + '\t' + (objNode["translate"]?.InnerText ?? objXmlRequired.InnerText);
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
                                    strThisRequirement += Environment.NewLine + '\t' + (objNode["translate"]?.InnerText ?? objXmlRequired.InnerText);
                                }
                                break;
                            case "inherited":
                                strThisRequirement += Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectQuality_Inherit");
                                break;
                            case "careerkarma":
                                // Check Career Karma requirement.
                                if (_objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
                                    blnFound = true;
                                else
                                    strThisRequirement = Environment.NewLine + '\t' + string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_SelectQuality_RequireKarma"), objXmlRequired.InnerText);
                                break;
                            case "ess":
                                // Check Essence requirement.
                                if (objXmlRequired.InnerText.StartsWith('-'))
                                {
                                    // Essence must be less than the value.
                                    if (_objCharacter.Essence() < Convert.ToDecimal(objXmlRequired.InnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                else
                                {
                                    // Essence must be equal to or greater than the value.
                                    if (_objCharacter.Essence() >= Convert.ToDecimal(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                break;
                            case "skill":
                                // Check if the character has the required Skill.
                                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(objXmlRequired["name"].InnerText);
                                if ((objSkill?.Rating ?? 0) >= Convert.ToInt32(objXmlRequired["val"].InnerText, GlobalOptions.InvariantCultureInfo))
                                {
                                    blnFound = true;
                                }
                                break;
                            case "attribute":
                                // Check to see if an Attribute meets a requirement.
                                CharacterAttrib objAttribute = _objCharacter.GetAttribute(objXmlRequired["name"].InnerText);

                                if (objXmlRequired["total"] != null)
                                {
                                    // Make sure the Attribute's total value meets the requirement.
                                    if (objAttribute.TotalValue >= Convert.ToInt32(objXmlRequired["total"].InnerText, GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                break;
                            case "attributetotal":
                                // Check if the character's Attributes add up to a particular total.
                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                foreach (string strAttribute in AttributeSection.AttributeStrings)
                                {
                                    strAttributes = strAttributes.CheapReplace(strAttribute, () => _objCharacter.GetAttribute(strAttribute).Value.ToString(GlobalOptions.InvariantCultureInfo));
                                }

                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAttributes, out bool blnIsSuccess);
                                if ((blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) : 0) >= Convert.ToInt32(objXmlRequired["val"].InnerText, GlobalOptions.InvariantCultureInfo))
                                    blnFound = true;
                                break;
                            case "skillgrouptotal":
                            {
                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                int intTotal = 0;
                                foreach (string strGroup in objXmlRequired["skillgroups"].InnerText.SplitNoAlloc('+', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroup)
                                        {
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText, GlobalOptions.InvariantCultureInfo))
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
                                                string strSelect = objXmlCyberware.Attributes["select"]?.InnerText;
                                                if (string.IsNullOrEmpty(strSelect) || strSelect == objCyberware.Extra)
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
                                                string strSelect = objXmlCyberware.Attributes["select"]?.InnerText;
                                                if (string.IsNullOrEmpty(strSelect) || strSelect == objCyberware.Extra)
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
                                                string strSelect = objXmlCyberware.Attributes["select"]?.InnerText;
                                                if (string.IsNullOrEmpty(strSelect) || strSelect == objCyberware.Extra)
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

                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText, GlobalOptions.InvariantCultureInfo))
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
                                if (_objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(objXmlRequired.InnerText, GlobalOptions.InvariantCultureInfo))
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
                    string strMessage = LanguageManager.GetString("Message_SelectQuality_QualityRequirement");
                    strMessage += strRequirement;

                    if (blnShowMessage)
                        Program.MainForm.ShowMessageBox(this, strMessage, LanguageManager.GetString("MessageTitle_SelectQuality_QualityRequirement"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            return true;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
