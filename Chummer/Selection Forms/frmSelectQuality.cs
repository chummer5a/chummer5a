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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectQuality : Form
    {
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain;
        private bool _blnLoading = true;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XPathNavigator _xmlMetatypeQualityRestrictionNode;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        private static string _strSelectCategory = string.Empty;

        #region Control Events

        public frmSelectQuality(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));

            // Load the Quality information.
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNode("/chummer");
            _xmlMetatypeQualityRestrictionNode = _objCharacter.GetNode().SelectSingleNode("qualityrestriction");
        }

        private void frmSelectQuality_Load(object sender, EventArgs e)
        {
            // Populate the Quality Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseQualityDataNode.Select("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
            }

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
            {
                cboCategory.SelectedValue = _strSelectCategory;

                if (cboCategory.SelectedIndex == -1)
                    cboCategory.SelectedIndex = 0;
            }
            cboCategory.Enabled = _lstCategory.Count > 1;
            cboCategory.EndUpdate();

            if (_objCharacter.MetagenicLimit == 0)
                chkNotMetagenic.Checked = true;

            lblBPLabel.Text = LanguageManager.GetString("Label_Karma");
            _blnLoading = false;
            BuildQualityList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void lstQualities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _blnLoading = true;

            XPathNavigator xmlQuality = null;
            string strSelectedQuality = lstQualities.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedQuality))
            {
                xmlQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[id = " + strSelectedQuality.CleanXPath() + "]");
            }

            if (xmlQuality != null)
            {
                nudRating.ValueAsInt = nudRating.MinimumAsInt;
                int intMaxRating = int.MaxValue;
                if (xmlQuality.TryGetInt32FieldQuickly("limit", ref intMaxRating) && xmlQuality.SelectSingleNode("nolevels") == null)
                {
                    lblRatingNALabel.Visible = false;
                    nudRating.MaximumAsInt = intMaxRating;
                    nudRating.Visible = true;
                }
                else
                {
                    lblRatingNALabel.Visible = true;
                    nudRating.MaximumAsInt = 1;
                    nudRating.ValueAsInt = 1;
                    nudRating.Visible = false;
                }

                UpdateCostLabel(xmlQuality);

                string strSource = xmlQuality.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
                string strPage = xmlQuality.SelectSingleNode("altpage")?.Value ?? xmlQuality.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
                SourceString objSource = new SourceString(strSource, strPage, GlobalOptions.Language,
                    GlobalOptions.CultureInfo, _objCharacter);
                lblSource.Text = objSource.ToString();
                lblSource.SetToolTip(objSource.LanguageBookTooltip);
                lblSourceLabel.Visible = lblSource.Visible = !string.IsNullOrEmpty(lblSource.Text);
                tlpRight.Visible = true;
            }
            else
            {
                tlpRight.Visible = false;
            }

            _blnLoading = false;
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

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void CostControl_Changed(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _blnLoading = true;

            XPathNavigator xmlQuality = null;
            string strSelectedQuality = lstQualities.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedQuality))
            {
                xmlQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[id = " + strSelectedQuality.CleanXPath() + "]");
            }

            if (xmlQuality != null)
                UpdateCostLabel(xmlQuality);

            _blnLoading = false;
        }

        private void chkMetagenic_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMetagenic.Checked)
                chkNotMetagenic.Checked = false;
            BuildQualityList();
        }

        private void chkNotMetagenic_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNotMetagenic.Checked)
                chkMetagenic.Checked = false;
            BuildQualityList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstQualities.SelectedIndex + 1 < lstQualities.Items.Count:
                    lstQualities.SelectedIndex += 1;
                    break;

                case Keys.Down:
                    {
                        if (lstQualities.Items.Count > 0)
                        {
                            lstQualities.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstQualities.SelectedIndex - 1 >= 0:
                    lstQualities.SelectedIndex -= 1;
                    break;

                case Keys.Up:
                    {
                        if (lstQualities.Items.Count > 0)
                        {
                            lstQualities.SelectedIndex = lstQualities.Items.Count - 1;
                        }

                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void KarmaFilter(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _blnLoading = true;
            if (string.IsNullOrWhiteSpace(nudMinimumBP.Text))
            {
                nudMinimumBP.Value = 0;
            }
            if (string.IsNullOrWhiteSpace(nudValueBP.Text))
            {
                nudValueBP.Value = 0;
            }
            if (string.IsNullOrWhiteSpace(nudMaximumBP.Text))
            {
                nudMaximumBP.Value = 0;
            }

            if (nudMaximumBP.Value < nudMinimumBP.Value)
            {
                if (sender == nudMaximumBP)
                    nudMinimumBP.Value = nudMaximumBP.Value;
                else
                    nudMaximumBP.Value = nudMinimumBP.Value;
            }

            _blnLoading = false;

            BuildQualityList();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Quality that was selected in the dialogue.
        /// </summary>
        public string SelectedQuality => _strSelectedQuality;

        public int SelectedRating => nudRating.ValueAsInt;

        /// <summary>
        /// Forcefully add a Category to the list.
        /// </summary>
        public string ForceCategory
        {
            set
            {
                if (_lstCategory.Any(x => x.Value.ToString() == value))
                {
                    cboCategory.BeginUpdate();
                    cboCategory.SelectedValue = value;
                    cboCategory.Enabled = false;
                    cboCategory.EndUpdate();
                }
            }
        }

        /// <summary>
        /// A Quality the character has that should be ignored for checking Forbidden requirements (which would prevent upgrading/downgrading a Quality).
        /// </summary>
        public string IgnoreQuality { get; set; } = string.Empty;

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

        #endregion Properties

        #region Methods

        private void UpdateCostLabel(XPathNavigator xmlQuality)
        {
            if (xmlQuality != null)
            {
                if (chkFree.Checked)
                    lblBP.Text = 0.ToString(GlobalOptions.CultureInfo);
                else
                {
                    string strKarma = xmlQuality.SelectSingleNode("karma")?.Value ?? string.Empty;
                    if (strKarma.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        int intMin;
                        int intMax = int.MaxValue;
                        string strCost = strKarma.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            int.TryParse(strValues[0], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                                out intMin);
                            int.TryParse(strValues[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                                out intMax);
                        }
                        else
                            int.TryParse(strCost.FastEscape('+'), NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                                out intMin);

                        lblBP.Text = intMax == int.MaxValue
                            ? intMin.ToString(GlobalOptions.CultureInfo)
                            : string.Format(GlobalOptions.CultureInfo, "{0}{1}-{1}{2}", intMin,
                                LanguageManager.GetString("String_Space"), intMax);
                    }
                    else
                    {
                        int.TryParse(strKarma, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intBP);

                        if (xmlQuality.SelectSingleNode("costdiscount").RequirementsMet(_objCharacter))
                        {
                            XPathNavigator xmlValueNode = xmlQuality.SelectSingleNode("costdiscount/value");
                            if (xmlValueNode != null)
                            {
                                int intValue = xmlValueNode.ValueAsInt;
                                switch (xmlQuality.SelectSingleNode("category")?.Value)
                                {
                                    case "Positive":
                                        intBP += intValue;
                                        break;

                                    case "Negative":
                                        intBP -= intValue;
                                        break;
                                }
                            }
                        }

                        if (_objCharacter.Created && !_objCharacter.Options.DontDoubleQualityPurchases)
                        {
                            string strDoubleCostCareer = xmlQuality.SelectSingleNode("doublecareer")?.Value;
                            if (string.IsNullOrEmpty(strDoubleCostCareer) || strDoubleCostCareer != bool.FalseString)
                            {
                                intBP *= 2;
                            }
                        }

                        intBP *= nudRating.ValueAsInt;

                        lblBP.Text = (intBP * _objCharacter.Options.KarmaQuality).ToString(GlobalOptions.CultureInfo);
                        if (!_objCharacter.Created && _objCharacter.FreeSpells > 0 && Convert.ToBoolean(
                            xmlQuality.SelectSingleNode("canbuywithspellpoints")?.Value,
                            GlobalOptions.InvariantCultureInfo))
                        {
                            int i = (intBP * _objCharacter.Options.KarmaQuality);
                            int spellPoints = 0;
                            while (i > 0)
                            {
                                i -= 5;
                                spellPoints++;
                            }

                            lblBP.Text += string.Format(GlobalOptions.CultureInfo, "{0}/{0}{1}{0}{2}",
                                LanguageManager.GetString("String_Space"), spellPoints,
                                LanguageManager.GetString("String_SpellPoints"));
                            lblBP.ToolTipText = LanguageManager.GetString("Tip_SelectSpell_MasteryQuality");
                        }
                        else
                        {
                            lblBP.ToolTipText = string.Empty;
                        }
                    }
                }
            }

            lblBPLabel.Visible = lblBP.Visible = !string.IsNullOrEmpty(lblBP.Text);
        }

        /// <summary>
        /// Build the list of Qualities.
        /// </summary>
        private void BuildQualityList()
        {
            if (_blnLoading)
                return;

            string strCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
            StringBuilder sbdFilter = new StringBuilder('(' + _objCharacter.Options.BookXPath() + ')');
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0))
            {
                sbdFilter.Append(" and category = " + strCategory.CleanXPath());
            }
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = " + strItem.CleanXPath() + " or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    objCategoryFilter.Length -= 4;
                    sbdFilter.Append(" and (" + objCategoryFilter + ')');
                }
            }
            if (chkMetagenic.Checked)
            {
                sbdFilter.Append(" and (metagenic = 'True' or required/oneof[contains(., 'Changeling')])");
            }
            else if (chkNotMetagenic.Checked)
            {
                sbdFilter.Append(" and not(metagenic = 'True') and not(required/oneof[contains(., 'Changeling')])");
            }
            if (nudValueBP.Value != 0)
            {
                string strValueBP = nudValueBP.Value.ToString(GlobalOptions.InvariantCultureInfo);
                if (_objCharacter.Created && !_objCharacter.Options.DontDoubleQualityPurchases && nudValueBP.Value > 0)
                {
                    string strValueBPHalved = (nudValueBP.Value / 2).ToString(GlobalOptions.InvariantCultureInfo);
                    sbdFilter.Append(" and ((doublecareer = 'False' and (karma = " + strValueBP +
                                     " or (not(nolevels) and limit != 'False' and (karma mod " + strValueBP +
                                     ") = 0 and karma * karma * limit <= karma * " + strValueBP +
                                     "))) or (not(doublecareer = 'False') and (karma = " + strValueBPHalved +
                                     " or (not(nolevels) and limit != 'False' and (karma mod " + strValueBPHalved +
                                     ") = 0 and karma * karma * limit <= karma * " + strValueBPHalved + "))))");
                }
                else
                {
                    sbdFilter.Append(" and (karma = " + strValueBP +
                                     " or (not(nolevels) and limit != 'False' and (karma mod " + strValueBP +
                                     ") = 0 and karma * karma * limit <= karma * " + strValueBP + "))");
                }
            }
            else if (nudMinimumBP.Value != 0 || nudMaximumBP.Value != 0)
            {
                if (nudMinimumBP.Value < 0 == nudMaximumBP.Value < 0)
                {
                    sbdFilter.Append(" and (" + GetKarmaRangeString(nudMaximumBP.ValueAsInt, nudMinimumBP.ValueAsInt) + ")");
                }
                else
                {
                    sbdFilter.Append("and ((" + GetKarmaRangeString(nudMaximumBP.ValueAsInt, 0)
                                              + ") or (" + GetKarmaRangeString(-1, nudMinimumBP.ValueAsInt) + "))");
                }

                string GetKarmaRangeString(int intMax, int intMin)
                {
                    string strMax = intMax.ToString(GlobalOptions.InvariantCultureInfo);
                    string strMin = intMin.ToString(GlobalOptions.InvariantCultureInfo);
                    string strMostExtremeValue = (intMax > 0 ? intMax : intMin).ToString(GlobalOptions.InvariantCultureInfo);
                    string strValueDiff = (intMax > 0 ? intMax - intMin : intMin - intMax).ToString(GlobalOptions.InvariantCultureInfo);
                    if (_objCharacter.Created && !_objCharacter.Options.DontDoubleQualityPurchases)
                    {
                        return "((doublecareer = 'False' or karma < 0) and ((karma >= " + strMin + " and karma <= " +
                               strMax + ") or (not(nolevels) and limit != 'False' and karma * karma <= karma * " +
                               strMostExtremeValue + " and (karma * (" + strMostExtremeValue +
                               " mod karma) <= karma * " + strValueDiff + ") and ((karma >= 0 and karma * limit >= " +
                               strMin + ") or (karma < 0 and karma * limit <= " + strMax +
                               "))))) or (not(doublecareer = 'False' or karma < 0) and ((2 * karma >= " + strMin +
                               " and 2 * karma <= " + strMax +
                               ") or (not(nolevels) and limit != 'False' and 2 * karma * karma <= 2 * karma * " +
                               strMostExtremeValue + " and (2 * karma * (" + strMostExtremeValue +
                               " mod (2 * karma)) <= 2 * karma * " + strValueDiff +
                               ") and ((karma >= 0 and 2 * karma * limit >= " + strMin +
                               ") or (karma < 0 and 2 * karma * limit <= " + strMax + ")))))";
                    }

                    return "(karma >= " + strMin + " and karma <= " + strMax +
                           ") or (not(nolevels) and limit != 'False' and karma * karma <= karma * " +
                           strMostExtremeValue + " and (karma * (" + strMostExtremeValue + " mod karma) <= karma * " +
                           strValueDiff + ") and ((karma >= 0 and karma * limit >= " + strMin +
                           ") or (karma < 0 and karma * limit <= " + strMax + ")))";
                }
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
                sbdFilter.Append(" and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text));

            string strCategoryLower = strCategory == "Show All" ? "*" : strCategory.ToLowerInvariant();
            List<ListItem> lstQuality = new List<ListItem>();
            foreach (XPathNavigator objXmlQuality in _xmlBaseQualityDataNode.Select("qualities/quality[" + sbdFilter + "]"))
            {
                string strLoopName = objXmlQuality.SelectSingleNode("name")?.Value;
                if (string.IsNullOrEmpty(strLoopName))
                    continue;
                if (_xmlMetatypeQualityRestrictionNode != null && _xmlMetatypeQualityRestrictionNode.SelectSingleNode(strCategoryLower + "/quality[. = " + strLoopName.CleanXPath() + "]") == null)
                    continue;
                if (!chkLimitList.Checked || objXmlQuality.RequirementsMet(_objCharacter, string.Empty, string.Empty, IgnoreQuality))
                {
                    lstQuality.Add(new ListItem(objXmlQuality.SelectSingleNode("id")?.Value ?? string.Empty, objXmlQuality.SelectSingleNode("translate")?.Value ?? strLoopName));
                }
            }
            lstQuality.Sort(CompareListItems.CompareNames);

            string strOldSelectedQuality = lstQualities.SelectedValue?.ToString();
            _blnLoading = true;
            lstQualities.BeginUpdate();
            lstQualities.PopulateWithListItems(lstQuality);
            _blnLoading = false;
            if (string.IsNullOrEmpty(strOldSelectedQuality))
                lstQualities.SelectedIndex = -1;
            else
                lstQualities.SelectedValue = strOldSelectedQuality;
            lstQualities.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedQuality = lstQualities.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedQuality))
                return;

            XPathNavigator objNode = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[id = " + strSelectedQuality.CleanXPath() + "]");

            if (objNode == null || !objNode.RequirementsMet(_objCharacter, null, LanguageManager.GetString("String_Quality"), IgnoreQuality))
                return;

            _strSelectedQuality = strSelectedQuality;
            _strSelectCategory = (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode.SelectSingleNode("category")?.Value;
            DialogResult = DialogResult.OK;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
