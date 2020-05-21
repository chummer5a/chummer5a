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
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectDrug : Form
    {
        private readonly Character _objCharacter;
        private IList<Grade> _lstGrades;
        private readonly string _strNoneGradeId;

        private decimal _decCostMultiplier = 1.0m;
        private int _intAvailModifier;

        private Grade _objForcedGrade;
        private bool _blnLockGrade;
        private bool _blnLoading = true;

        private readonly string _strNodeXPath = "Drugs/Drug";
        private static string _sStrSelectGrade = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private bool _blnIgnoreSecondHand;
        private string _strForceGrade = string.Empty;
        private readonly HashSet<string> _setBlackMarketMaps;
        private readonly XPathNavigator _xmlBaseDrugDataNode;

        private enum Mode
        {
            Drug = 0,
            Bioware,
        }

        #region Control Events
        public frmSelectDrug(Character objCharacter)
        {
            InitializeComponent();

            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));

            _xmlBaseDrugDataNode = XmlManager.Load("drugcomponents.xml").GetFastNavigator().SelectSingleNode("/chummer");

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _lstGrades = _objCharacter.GetGradeList(Improvement.ImprovementSource.Drug);
            _strNoneGradeId = _lstGrades.FirstOrDefault(x => x.Name == "None")?.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo);
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseDrugDataNode);
        }

        private void frmSelectDrug_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
                chkHideBannedGrades.Visible = false;
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
                chkHideBannedGrades.Visible = !_objCharacter.IgnoreRules;
                chkHideOverAvailLimit.Text = string.Format(GlobalOptions.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.MaximumAvailability.ToString(GlobalOptions.CultureInfo));
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                txtSearch.Text = DefaultSearchText;
                txtSearch.Enabled = false;
            }

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            PopulateGrades(false, true, _objForcedGrade?.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) ?? string.Empty);

            if (_objForcedGrade != null)
                cboGrade.SelectedValue = _objForcedGrade.SourceId.ToString();
            else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                cboGrade.SelectedValue = _sStrSelectGrade;
            if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                cboGrade.SelectedIndex = 0;

            _blnLoading = false;
            RefreshList();
        }

        private void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;

            XPathNavigator xmlGrade = null;
            // Retrieve the information for the selected Grade.
            string strSelectedGrade = cboGrade.SelectedValue?.ToString();
            if (cboGrade.Enabled && strSelectedGrade != null)
                _strOldSelectedGrade = strSelectedGrade;
            if (!string.IsNullOrEmpty(strSelectedGrade))
                xmlGrade = _xmlBaseDrugDataNode.SelectSingleNode("grades/grade[id = \"" + strSelectedGrade + "\"]");

            // Update the Cost multipliers based on the Grade that has been selected.
            if (xmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalOptions.InvariantCultureInfo);
                _intAvailModifier = Convert.ToInt32(xmlGrade.SelectSingleNode("avail")?.Value, GlobalOptions.InvariantCultureInfo);

                _blnLoading = false;
                RefreshList();
            }
            else
            {
                _blnLoading = false;
                UpdateDrugInfo();
            }
        }
        private void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (cboGrade.Enabled != _blnOldGradeEnabled)
            {
                _blnOldGradeEnabled = cboGrade.Enabled;
                if (_blnOldGradeEnabled)
                {
                    cboGrade.SelectedValue = _strOldSelectedGrade;
                }
                cboGrade_SelectedIndexChanged(sender, e);
            }
        }

        private void lstDrug_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            XPathNavigator xmlDrug = null;
            string strSelectedId = lstDrug.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Drug.
                xmlDrug = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = \"" + strSelectedId + "\"]");
            }
            string strForceGrade;
            if (xmlDrug != null)
            {
                strForceGrade = xmlDrug.SelectSingleNode("forcegrade")?.Value;
                // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
                XPathNavigator xmlRatingNode = xmlDrug.SelectSingleNode("rating");
                if (xmlRatingNode != null)
                {
                    string strMinRating = xmlDrug.SelectSingleNode("minrating")?.Value;
                    int intMinRating = 1;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                    {
                        strMinRating = strMinRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalOptions.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnIsSuccess);
                        intMinRating = blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) : 1;
                    }
                    nudRating.Minimum = intMinRating;

                    string strMaxRating = xmlRatingNode.Value;
                    int intMaxRating = 0;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                    {
                        strMaxRating = strMaxRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalOptions.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMaxRating, out bool blnIsSuccess);
                        intMaxRating = blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) : 1;
                    }
                    nudRating.Maximum = intMaxRating;
                    if (chkHideOverAvailLimit.Checked)
                    {
                        int intAvailModifier = strForceGrade == "None" ? 0 : _intAvailModifier;
                        while (nudRating.Maximum > intMinRating && !SelectionShared.CheckAvailRestriction(xmlDrug, _objCharacter, decimal.ToInt32(nudRating.Maximum), intAvailModifier))
                        {
                            nudRating.Maximum -= 1;
                        }
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (chkBlackMarketDiscount.Checked)
                            decCostMultiplier *= 0.9m;
                        while (nudRating.Maximum > intMinRating && !SelectionShared.CheckNuyenRestriction(xmlDrug, _objCharacter.Nuyen, decCostMultiplier, decimal.ToInt32(nudRating.Maximum)))
                        {
                            nudRating.Maximum -= 1;
                        }
                    }
                    nudRating.Value = nudRating.Minimum;
                    nudRating.Enabled = nudRating.Minimum != nudRating.Maximum;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                    lblRatingLabel.Visible = true;
                }
                else
                {
                    lblRatingLabel.Visible = true;
                    lblRatingNALabel.Visible = true;
                    nudRating.Minimum = 0;
                    nudRating.Value = 0;
                    nudRating.Visible = false;
                }

                string strSource = xmlDrug.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strPage = xmlDrug.SelectSingleNode("altpage")?.Value ?? xmlDrug.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                Grade objForcedGrade = null;
                if (!string.IsNullOrEmpty(strForceGrade))
                {
                    // Force the Drug to be a particular Grade.
                    if (cboGrade.Enabled)
                        cboGrade.Enabled = false;
                    objForcedGrade = _lstGrades.FirstOrDefault(x => x.Name == strForceGrade);
                    strForceGrade = objForcedGrade?.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo);
                }
                else
                {
                    cboGrade.Enabled = !_blnLockGrade;
                    if (_blnLockGrade)
                    {
                        strForceGrade = _objForcedGrade?.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) ?? cboGrade.SelectedValue?.ToString();
                        objForcedGrade = _objForcedGrade ?? _lstGrades.FirstOrDefault(x => x.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) == strForceGrade);
                    }
                }

                chkBlackMarketDiscount.Enabled = _objCharacter.BlackMarketDiscount;

                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalOptions.AssumeBlackMarket &&
                                                     _setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")
                                                         ?.Value);
                }
                else if (!_setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")?.Value))
                {
                    //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
                PopulateGrades(xmlDrug.SelectSingleNode("nosecondhand") != null || (!cboGrade.Enabled && objForcedGrade?.SecondHand != true), false, strForceGrade);
                /*
                string strNotes = xmlDrug.SelectSingleNode("altnotes")?.Value ?? xmlDrug.SelectSingleNode("notes")?.Value;
                if (!string.IsNullOrEmpty(strNotes))
                {
                    lblDrugNotes.Visible = true;
                    lblDrugNotesLabel.Visible = true;
                    lblDrugNotes.Text = strNotes;
                }
                else
                {
                    lblDrugNotes.Visible = false;
                    lblDrugNotesLabel.Visible = false;
                }*/
            }
            else
            {
                lblRatingLabel.Visible = false;
                lblRatingNALabel.Visible = false;
                nudRating.Minimum = 0;
                nudRating.Value = 0;
                nudRating.Visible = false;
                cboGrade.Enabled = !_blnLockGrade;
                strForceGrade = string.Empty;
                Grade objForcedGrade = null;
                if (_blnLockGrade)
                {
                    strForceGrade = _objForcedGrade?.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) ?? cboGrade.SelectedValue?.ToString();
                    objForcedGrade = _objForcedGrade ?? _lstGrades.FirstOrDefault(x => x.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) == strForceGrade);
                }
                PopulateGrades(_blnLockGrade && objForcedGrade?.SecondHand != true, false, strForceGrade);
                chkBlackMarketDiscount.Checked = false;
                lblSourceLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }
            _blnLoading = false;
            UpdateDrugInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            UpdateDrugInfo();
        }

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
            {
                RefreshList();
            }
            UpdateDrugInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkHideBannedGrades_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _lstGrades = _objCharacter.GetGradeList(Improvement.ImprovementSource.Drug, chkHideBannedGrades.Checked);
            PopulateGrades();
        }

        private void lstDrug_DoubleClick(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList();
            }
            UpdateDrugInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            UpdateDrugInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstDrug.SelectedIndex + 1 < lstDrug.Items.Count)
                {
                    lstDrug.SelectedIndex++;
                }
                else if (lstDrug.Items.Count > 0)
                {
                    lstDrug.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstDrug.SelectedIndex - 1 >= 0)
                {
                    lstDrug.SelectedIndex--;
                }
                else if (lstDrug.Items.Count > 0)
                {
                    lstDrug.SelectedIndex = lstDrug.Items.Count - 1;
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
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

        /// <summary>
        /// Manually set the Grade of the piece of Drug.
        /// </summary>
        public Grade SetGrade
        {
            set => _objForcedGrade = value;
        }

        /// <summary>
        /// Name of Drug that was selected in the dialogue.
        /// </summary>
        public string SelectedDrug { get; private set; } = string.Empty;

        /// <summary>
        /// Grade of the selected piece of Drug.
        /// </summary>
        public Grade SelectedGrade { get; private set; }

        /// <summary>
        /// Rating of the selected piece of Drug (0 if not applicable).
        /// </summary>
        public int SelectedRating { get; private set; }

        /// <summary>
        /// Selected Essence cost discount.
        /// </summary>
        public int SelectedESSDiscount { get; private set; }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public Vehicle ParentVehicle { set; get; }

        public decimal Markup { get; set; }

        /// <summary>
        /// Parent Drug that the current selection will be added to.
        /// </summary>
        public Drug DrugParent { get; set; }

        /// <summary>
        /// Default text string to filter by.
        /// </summary>
        public string DefaultSearchText { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Update the Drug's information based on the Drug selected and current Rating.
        /// </summary>
        private void UpdateDrugInfo()
        {
            XPathNavigator objXmlDrug = null;
            string strSelectedId = lstDrug.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected piece of Drug.
                objXmlDrug = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = \"" + strSelectedId + "\"]");
            }
            if (objXmlDrug == null)
            {
                lblCostLabel.Visible = false;
                lblAvailLabel.Visible = false;
                lblTestLabel.Visible = false;
                lblCost.Text = string.Empty;
                lblAvail.Text = string.Empty;
                lblTest.Text = string.Empty;
                return;
            }

            // Extract the Avil and Cost values from the Drug info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            int intRating = decimal.ToInt32(nudRating.Value);
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail = objXmlDrug.SelectSingleNode("avail")?.Value;
            if (!string.IsNullOrEmpty(strAvail))
            {
                string strAvailExpr = strAvail;
                if (strAvailExpr.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvailExpr = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                }

                string strSuffix = string.Empty;
                char chrSuffix = strAvailExpr[strAvailExpr.Length - 1];
                if (chrSuffix == 'R')
                {
                    strSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                else if (chrSuffix == 'F')
                {
                    strSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }

                string strPrefix = string.Empty;
                char chrPrefix = strAvailExpr[0];
                if (chrPrefix == '+' || chrPrefix == '-')
                {
                    strPrefix = chrPrefix.ToString(GlobalOptions.InvariantCultureInfo);
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }

                strAvailExpr = strAvailExpr.CheapReplace("MinRating", () => nudRating.Minimum.ToString(GlobalOptions.InvariantCultureInfo))
                    .CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));

                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr, out bool blnIsSuccess);
                if (blnIsSuccess)
                {
                    int intAvail = Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) + _intAvailModifier;
                    // Avail cannot go below 0.
                    if (intAvail < 0)
                        intAvail = 0;
                    lblAvail.Text = strPrefix + intAvail.ToString(GlobalOptions.CultureInfo) + strSuffix;
                }
                else
                {
                    lblAvail.Text = strAvail;
                }
            }
            else
            {
                lblAvail.Text = 0.ToString(GlobalOptions.CultureInfo);
            }

            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

            // Cost.
            decimal decItemCost = 0;
            if (chkFree.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            }
            else
            {
                string strCost = objXmlDrug.SelectSingleNode("cost")?.Value;
                if (!string.IsNullOrEmpty(strCost))
                {
                    if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                        strCost = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                    }
                    // Check for a Variable Cost.
                    if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                        lblCost.Text = decMax == decimal.MaxValue ?
                            decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+" :
                            decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                        decItemCost = decMin;
                    }
                    else
                    {
                        strCost = strCost.CheapReplace("MinRating", () => nudRating.Minimum.ToString(GlobalOptions.InvariantCultureInfo))
                            .CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                        if (blnIsSuccess)
                        {
                            decItemCost = Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) * _decCostMultiplier;
                            decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                            if (chkBlackMarketDiscount.Checked)
                            {
                                decItemCost *= 0.9m;
                            }

                            lblCost.Text = decItemCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                        }
                        else
                        {
                            lblCost.Text = strCost + '¥';
                        }
                    }
                }
                else
                    lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            }

            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);

            // Test required to find the item.
            lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);
            lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);
        }

        private bool _blnSkipListRefresh;
        private IList<ListItem> RefreshList(bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            if ((_blnLoading || _blnSkipListRefresh) && blnDoUIUpdate)
                return null;
            string strFilter = "(" + _objCharacter.Options.BookXPath() +')';
            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId) ? null : _lstGrades.FirstOrDefault(x => x.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) == strCurrentGradeId);
            if (objCurrentGrade != null)
            {
                strFilter += " and (not(forcegrade) or forcegrade = \"None\" or forcegrade = \"" + objCurrentGrade.Name + "\")";
                if (objCurrentGrade.SecondHand)
                    strFilter += " and not(nosecondhand)";
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            return BuildDrugList(_xmlBaseDrugDataNode.Select(_strNodeXPath + '[' + strFilter + ']'), blnDoUIUpdate, blnTerminateAfterFirst);
        }

        private IList<ListItem> BuildDrugList(XPathNodeIterator objXmlDrugList, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            if (_blnLoading && blnDoUIUpdate)
                return null;

            List<ListItem> lstDrugs = new List<ListItem>();

            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId) ? null : _lstGrades.FirstOrDefault(x => x.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) == strCurrentGradeId);
            foreach (XPathNavigator xmlDrug in objXmlDrugList)
            {
                bool blnIsForceGrade = xmlDrug.SelectSingleNode("forcegrade") == null;
                if (objCurrentGrade != null && blnIsForceGrade)
                {
                    if (_objCharacter.Improvements.Any(x => (x.ImproveType == Improvement.ImprovementType.DisableDrugGrade && objCurrentGrade.Name.Contains(x.ImprovedName) && x.Enabled)))
                        continue;
                }

                string strMaxRating = xmlDrug.SelectSingleNode("rating")?.Value;
                string strMinRating = xmlDrug.SelectSingleNode("minrating")?.Value;
                int intMinRating = 1;
                // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                if ((!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out int intMaxRating)) || (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating)))
                {
                    var objProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnIsSuccess);
                    intMinRating = blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) : 1;
                    objProcess = CommonFunctions.EvaluateInvariantXPath(strMaxRating, out blnIsSuccess);
                    intMaxRating = blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo) : 1;
                    if (intMaxRating < intMinRating)
                        continue;
                }

                if (chkHideOverAvailLimit.Checked && !SelectionShared.CheckAvailRestriction(xmlDrug, _objCharacter, intMinRating, blnIsForceGrade ? 0 : _intAvailModifier))
                    continue;
                if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
                {
                    decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    if (!SelectionShared.CheckNuyenRestriction(xmlDrug, _objCharacter.Nuyen, decCostMultiplier))
                        continue;
                }
                if (ParentVehicle == null && !xmlDrug.RequirementsMet(_objCharacter))
                    continue;
                lstDrugs.Add(new ListItem(xmlDrug.SelectSingleNode("id")?.Value, xmlDrug.SelectSingleNode("translate")?.Value ?? xmlDrug.SelectSingleNode("name")?.Value));
                if (blnTerminateAfterFirst)
                    break;
            }

            if (!blnDoUIUpdate) return lstDrugs;
            lstDrugs.Sort(CompareListItems.CompareNames);

            string strOldSelected = lstDrug.SelectedValue?.ToString();
            _blnLoading = true;
            lstDrug.BeginUpdate();
            lstDrug.ValueMember = "Value";
            lstDrug.DisplayMember = "Name";
            lstDrug.DataSource = lstDrug;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstDrug.SelectedValue = strOldSelected;
            else
                lstDrug.SelectedIndex = -1;

            lstDrug.EndUpdate();

            return lstDrugs;
        }

        /// <summary>
        /// Lock the Grade so it cannot be changed.
        /// </summary>
        public void LockGrade()
        {
            cboGrade.Enabled = false;
            _blnLockGrade = true;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstDrug.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if (cboGrade.Text.StartsWith('*'))
            {
                MessageBox.Show(
                    LanguageManager.GetString("Message_BannedGrade", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_BannedGrade", GlobalOptions.Language),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XPathNavigator objDrugNode = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = \"" + strSelectedId + "\"]");
            if (objDrugNode == null)
                return;

            if (!objDrugNode.RequirementsMet(_objCharacter, null, LanguageManager.GetString("String_SelectPACKSKit_Drug", GlobalOptions.Language)))
                return;

            string strForceGrade = objDrugNode.SelectSingleNode("forcegrade")?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.FirstOrDefault(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = cboGrade.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.FirstOrDefault(x => x.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) == cboGrade.SelectedValue?.ToString());
                else
                    return;
            }

            _sStrSelectGrade = SelectedGrade?.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo);
            SelectedDrug = strSelectedId;
            SelectedRating = decimal.ToInt32(nudRating.Value);
            BlackMarketDiscount = chkBlackMarketDiscount.Checked;
            Markup = nudMarkup.Value;

            DialogResult = DialogResult.OK;
        }

        private bool _blnPopulatingGrades;
        /// <summary>
        /// Populate the list of Drug Grades.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Secon-Hand Grades should be added to the list.</param>
        /// <param name="blnForce">Force grades to be repopulated.</param>
        /// <param name="strForceGrade">If not empty, force this grade to be selected.</param>
        private void PopulateGrades(bool blnIgnoreSecondHand = false, bool blnForce = false, string strForceGrade = "")
        {
            if (_blnPopulatingGrades)
                return;
            _blnPopulatingGrades = true;
            if (blnForce || blnIgnoreSecondHand != _blnIgnoreSecondHand || _strForceGrade != strForceGrade || cboGrade.Items.Count == 0)
            {
                _blnIgnoreSecondHand = blnIgnoreSecondHand;
                _strForceGrade = strForceGrade;
                List<ListItem> lstGrade = new List<ListItem>(5);
                foreach (Grade objWareGrade in _lstGrades)
                {
                    if (objWareGrade.SourceId.ToString("D", GlobalOptions.InvariantCultureInfo) == _strNoneGradeId && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != _strNoneGradeId))
                        continue;
                    //if (_objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableDrugGrade && objWareGrade.Name.Contains(x.ImprovedName) && x.Enabled))
                    //    continue;
                    if (blnIgnoreSecondHand && objWareGrade.SecondHand)
                        continue;
                    /*
                    if (blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.BannedDrugGrades.Any(s => objWareGrade.Name.Contains(s)))
                        continue;
                    if (!blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.BannedDrugGrades.Any(s => objWareGrade.Name.Contains(s)))
                    {
                        lstGrade.Add(new ListItem(objWareGrade.SourceId.ToString("D"), $"*{objWareGrade.DisplayName(GlobalOptions.Language)}"));
                    }
                    else
                    {
                        lstGrade.Add(new ListItem(objWareGrade.SourceId.ToString("D"), objWareGrade.DisplayName(GlobalOptions.Language)));
                    }*/
                }

                string strOldSelected = cboGrade.SelectedValue?.ToString();
                bool blnOldSkipListRefresh = _blnSkipListRefresh;
                if (strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId || lstGrade.Any(x => x.Value.ToString() == strOldSelected))
                    _blnSkipListRefresh = true;
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                cboGrade.BeginUpdate();
                cboGrade.ValueMember = "Value";
                cboGrade.DisplayMember = "Name";
                cboGrade.DataSource = lstGrade;
                _blnLoading = blnOldLoading;
                if (!string.IsNullOrEmpty(strForceGrade))
                    cboGrade.SelectedValue = strForceGrade;
                else if (cboGrade.SelectedIndex <= 0 && !string.IsNullOrWhiteSpace(strOldSelected))
                    cboGrade.SelectedValue = strOldSelected;
                if (cboGrade.SelectedIndex == -1 && lstGrade.Count > 0)
                    cboGrade.SelectedIndex = 0;

                cboGrade.EndUpdate();

                _blnSkipListRefresh = blnOldSkipListRefresh;
            }
            _blnPopulatingGrades = false;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
