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
using System.Text;
using System.Windows.Forms;
 using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectCritterPower : Form
    {
        private string _strSelectedPower = string.Empty;
        private int _intSelectedRating;
        private static string s_StrSelectCategory = string.Empty;
        private decimal _decPowerPoints;
        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseCritterPowerDataNode;
        private readonly XPathNavigator _xmlMetatypeDataNode;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectCritterPower(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _xmlBaseCritterPowerDataNode = _objCharacter.LoadDataXPath("critterpowers.xml").SelectSingleNode("/chummer");
            _xmlMetatypeDataNode = _objCharacter.GetNode();

            if (_xmlMetatypeDataNode == null || _objCharacter.MetavariantGuid == Guid.Empty) return;
            XPathNavigator xmlMetavariantNode = _xmlMetatypeDataNode.SelectSingleNode("metavariants/metavariant[id = "
                                                                                      + _objCharacter.MetavariantGuid.ToString("D", GlobalOptions.InvariantCultureInfo).CleanXPath()
                                                                                      + "]");
            if (xmlMetavariantNode != null)
                _xmlMetatypeDataNode = xmlMetavariantNode;
        }

        private void frmSelectCritterPower_Load(object sender, EventArgs e)
        {
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseCritterPowerDataNode.Select("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if (_objCharacter.Improvements.Any(imp =>
                        imp.ImproveType == Improvement.ImprovementType.AllowCritterPowerCategory &&
                        strInnerText.Contains(imp.ImprovedName)) &&
                    objXmlCategory.SelectSingleNode("@whitelist")?.Value == bool.TrueString ||
                    _objCharacter.Improvements.Any(imp =>
                        imp.ImproveType == Improvement.ImprovementType.LimitCritterPowerCategory &&
                        strInnerText.Contains(imp.ImprovedName)))
                {
                    _lstCategory.Add(new ListItem(strInnerText,
                        objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
                    continue;
                }
                if (_objCharacter.Improvements.Any(imp =>
                        imp.ImproveType == Improvement.ImprovementType.LimitCritterPowerCategory &&
                        !strInnerText.Contains(imp.ImprovedName)))
                {
                    continue;
                }
                _lstCategory.Add(new ListItem(strInnerText,
                    objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
            }

            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            cboCategory.EndUpdate();

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else if (cboCategory.Items.Contains(s_StrSelectCategory))
            {
                cboCategory.SelectedValue = s_StrSelectCategory;
            }

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void trePowers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            lblPowerPoints.Visible = false;
            lblPowerPointsLabel.Visible = false;
            string strSelectedPower = trePowers.SelectedNode.Tag?.ToString();
            if (!string.IsNullOrEmpty(strSelectedPower))
            {
                XPathNavigator objXmlPower = _xmlBaseCritterPowerDataNode.SelectSingleNode("powers/power[id = " + strSelectedPower.CleanXPath() + "]");
                if (objXmlPower != null)
                {
                    lblCritterPowerCategory.Text = objXmlPower.SelectSingleNode("category")?.Value ?? string.Empty;

                    switch (objXmlPower.SelectSingleNode("type")?.Value)
                    {
                        case "M":
                            lblCritterPowerType.Text = LanguageManager.GetString("String_SpellTypeMana");
                            break;
                        case "P":
                            lblCritterPowerType.Text = LanguageManager.GetString("String_SpellTypePhysical");
                            break;
                        default:
                            lblCritterPowerType.Text = string.Empty;
                            break;
                    }

                    switch (objXmlPower.SelectSingleNode("action")?.Value)
                    {
                        case "Auto":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionAutomatic");
                            break;
                        case "Free":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionFree");
                            break;
                        case "Simple":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionSimple");
                            break;
                        case "Complex":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionComplex");
                            break;
                        case "Special":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_SpellDurationSpecial");
                            break;
                        default:
                            lblCritterPowerAction.Text = string.Empty;
                            break;
                    }

                    string strRange = objXmlPower.SelectSingleNode("range")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strRange))
                    {
                        strRange = strRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf"))
                            .CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial"))
                            .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight"))
                            .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence"))
                            .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch"))
                            .CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea") + ')')
                            .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort"));
                    }
                    lblCritterPowerRange.Text = strRange;

                    string strDuration = objXmlPower.SelectSingleNode("duration")?.Value ?? string.Empty;
                    switch (strDuration)
                    {
                        case "Instant":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationInstantLong");
                            break;
                        case "Sustained":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationSustained");
                            break;
                        case "Always":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationAlways");
                            break;
                        case "Special":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationSpecial");
                            break;
                        default:
                            lblCritterPowerDuration.Text = strDuration;
                            break;
                    }

                    string strSource = objXmlPower.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
                    string strPage = objXmlPower.SelectSingleNode("altpage")?.Value ?? objXmlPower.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
                    SourceString objSource = new SourceString(strSource, strPage, GlobalOptions.Language,
                        GlobalOptions.CultureInfo, _objCharacter);
                    lblCritterPowerSource.Text = objSource.ToString();
                    lblCritterPowerSource.SetToolTip(objSource.LanguageBookTooltip);

                    nudCritterPowerRating.Visible = objXmlPower.SelectSingleNode("rating") != null;

                    lblKarma.Text = objXmlPower.SelectSingleNode("karma")?.Value ?? "0";

                    // If the character is a Free Spirit, populate the Power Points Cost as well.
                    if (_objCharacter.Metatype == "Free Spirit")
                    {
                        XPathNavigator xmlOptionalPowerCostNode = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers/power[. = " + objXmlPower.SelectSingleNode("name")?.Value.CleanXPath() + "]/@cost");
                        if (xmlOptionalPowerCostNode != null)
                        {
                            lblPowerPoints.Text = xmlOptionalPowerCostNode.Value;
                            lblPowerPoints.Visible = true;
                            lblPowerPointsLabel.Visible = true;
                        }
                    }
                }
                lblCritterPowerTypeLabel.Visible = !string.IsNullOrEmpty(lblCritterPowerType.Text);
                lblCritterPowerActionLabel.Visible = !string.IsNullOrEmpty(lblCritterPowerAction.Text);
                lblCritterPowerRangeLabel.Visible = !string.IsNullOrEmpty(lblCritterPowerRange.Text);
                lblCritterPowerDurationLabel.Visible = !string.IsNullOrEmpty(lblCritterPowerDuration.Text);
                lblCritterPowerSourceLabel.Visible = !string.IsNullOrEmpty(lblCritterPowerSource.Text);
                lblKarmaLabel.Visible = !string.IsNullOrEmpty(lblKarma.Text);
                tlpRight.Visible = true;
            }
            else
            {
                tlpRight.Visible = false;
            }
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            trePowers.Nodes.Clear();

            string strCategory = cboCategory.SelectedValue?.ToString();

            List<string> lstPowerWhitelist = new List<string>();

            // If the Critter is only allowed certain Powers, display only those.
            XPathNavigator xmlOptionalPowers = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers");
            if (xmlOptionalPowers != null)
            {
                foreach (XPathNavigator xmlNode in xmlOptionalPowers.Select("power"))
                    lstPowerWhitelist.Add(xmlNode.Value);

                // Determine if the Critter has a physical presence Power (Materialization, Possession, or Inhabitation).
                bool blnPhysicalPresence = _objCharacter.CritterPowers.Any(x => x.Name == "Materialization" || x.Name == "Possession" || x.Name == "Inhabitation");

                // Add any Critter Powers the Critter comes with that have been manually deleted so they can be re-added.
                foreach (XPathNavigator objXmlCritterPower in _xmlMetatypeDataNode.Select("powers/power"))
                {
                    bool blnAddPower = true;
                    // Make sure the Critter doesn't already have the Power.
                    foreach (CritterPower objCheckPower in _objCharacter.CritterPowers)
                    {
                        if (objCheckPower.Name == objXmlCritterPower.Value)
                        {
                            blnAddPower = false;
                            break;
                        }
                        if ((objCheckPower.Name == "Materialization" || objCheckPower.Name == "Possession" || objCheckPower.Name == "Inhabitation") && blnPhysicalPresence)
                        {
                            blnAddPower = false;
                            break;
                        }
                    }

                    if (blnAddPower)
                    {
                        lstPowerWhitelist.Add(objXmlCritterPower.Value);

                        // If Manifestation is one of the Powers, also include Inhabitation and Possess if they're not already in the list.
                        if (!blnPhysicalPresence && objXmlCritterPower.Value == "Materialization")
                        {
                            bool blnFoundPossession = false;
                            bool blnFoundInhabitation = false;
                            foreach (string strCheckPower in lstPowerWhitelist)
                            {
                                switch (strCheckPower)
                                {
                                    case "Possession":
                                        blnFoundPossession = true;
                                        break;
                                    case "Inhabitation":
                                        blnFoundInhabitation = true;
                                        break;
                                }

                                if (blnFoundInhabitation && blnFoundPossession)
                                    break;
                            }
                            if (!blnFoundPossession)
                            {
                                lstPowerWhitelist.Add("Possession");
                            }
                            if (!blnFoundInhabitation)
                            {
                                lstPowerWhitelist.Add("Inhabitation");
                            }
                        }
                    }
                }
            }

            StringBuilder sbdFilter = new StringBuilder('(' + _objCharacter.Options.BookXPath() + ')');
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All")
            {
                sbdFilter.Append(" and (contains(category," + strCategory.CleanXPath() + "))");
            }
            else
            {
                bool blnHasToxic = false;
                StringBuilder sbdCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                    {
                        sbdCategoryFilter.Append("(contains(category," + strItem.CleanXPath() + ")) or ");
                        if (strItem == "Toxic Critter Powers")
                        {
                            sbdCategoryFilter.Append("toxic = \"True\" or ");
                            blnHasToxic = true;
                        }
                    }
                }
                if (sbdCategoryFilter.Length > 0)
                {
                    sbdCategoryFilter.Length -= 4;
                    sbdFilter.Append(" and (" + sbdCategoryFilter + ')');
                }
                if (!blnHasToxic)
                    sbdFilter.Append(" and (not(toxic) or toxic != \"True\")");
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
                sbdFilter.Append(" and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text));
            foreach (XPathNavigator objXmlPower in _xmlBaseCritterPowerDataNode.Select("powers/power[" + sbdFilter + "]"))
            {
                string strPowerName = objXmlPower.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown");
                if (!lstPowerWhitelist.Contains(strPowerName) && lstPowerWhitelist.Count != 0) continue;
                if (!objXmlPower.RequirementsMet(_objCharacter, string.Empty, string.Empty)) continue;
                TreeNode objNode = new TreeNode
                {
                    Tag = objXmlPower.SelectSingleNode("id")?.Value ?? string.Empty,
                    Text = objXmlPower.SelectSingleNode("translate")?.Value ?? strPowerName
                };
                trePowers.Nodes.Add(objNode);
            }
            trePowers.Sort();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            cboCategory_SelectedIndexChanged(sender, e);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedPower = trePowers.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedPower))
                return;

            XPathNavigator objXmlPower = _xmlBaseCritterPowerDataNode.SelectSingleNode("powers/power[id = " + strSelectedPower.CleanXPath() + "]");
            if (objXmlPower == null)
                return;

            if (nudCritterPowerRating.Visible)
                _intSelectedRating = nudCritterPowerRating.ValueAsInt;

            s_StrSelectCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
            _strSelectedPower = strSelectedPower;

            // If the character is a Free Spirit (PC, not the Critter version), populate the Power Points Cost as well.
            if (_objCharacter.Metatype == "Free Spirit" && !_objCharacter.IsCritter)
            {
                string strName = objXmlPower.SelectSingleNode("name")?.Value;
                if (!string.IsNullOrEmpty(strName))
                {
                    XPathNavigator objXmlOptionalPowerCost = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers/power[. = " + strName.CleanXPath() + "]/@cost");
                    if (objXmlOptionalPowerCost != null)
                        _decPowerPoints = Convert.ToDecimal(objXmlOptionalPowerCost.Value, GlobalOptions.InvariantCultureInfo);
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Criter Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower => _strSelectedPower;

        /// <summary>
        /// Rating for the Critter Power that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => _intSelectedRating;

        /// <summary>
        /// Power Point cost for the Critter Power (only applies to Free Spirits).
        /// </summary>
        public decimal PowerPoints => _decPowerPoints;

        #endregion
    }
}
