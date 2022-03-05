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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectDrug : Form
    {
        private readonly Character _objCharacter;
        private readonly List<Grade> _lstGrades;
        private readonly string _strNoneGradeId;

        private decimal _decCostMultiplier = 1.0m;
        private int _intAvailModifier;

        private Grade _objForcedGrade;
        private bool _blnLockGrade;
        private bool _blnLoading = true;

        private const string _strNodeXPath = "Drugs/Drug";
        private static string _sStrSelectGrade = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private bool _blnIgnoreSecondHand;
        private string _strForceGrade = string.Empty;
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private readonly XPathNavigator _xmlBaseDrugDataNode;

        #region Control Events

        public SelectDrug(Character objCharacter)
        {
            InitializeComponent();

            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));

            _xmlBaseDrugDataNode = objCharacter.LoadDataXPath("drugcomponents.xml").SelectSingleNodeAndCacheExpression("/chummer");

            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _lstGrades = _objCharacter.GetGradeList(Improvement.ImprovementSource.Drug).ToList();
            _strNoneGradeId = _lstGrades.Find(x => x.Name == "None")?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseDrugDataNode));
        }

        private async void SelectDrug_Load(object sender, EventArgs e)
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
                chkHideOverAvailLimit.Text = string.Format(GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
            }

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                txtSearch.Text = DefaultSearchText;
                txtSearch.Enabled = false;
            }

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            PopulateGrades(false, true, _objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) ?? string.Empty);

            if (_objForcedGrade != null)
                cboGrade.SelectedValue = _objForcedGrade.SourceId.ToString();
            else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                cboGrade.SelectedValue = _sStrSelectGrade;
            if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                cboGrade.SelectedIndex = 0;

            _blnLoading = false;
            await RefreshList();
        }

        private async void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            await ProcessGradeChanged();
        }

        private async ValueTask ProcessGradeChanged()
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
                xmlGrade = _xmlBaseDrugDataNode.SelectSingleNode("grades/grade[id = " + strSelectedGrade.CleanXPath() + ']');

            // Update the Cost multipliers based on the Grade that has been selected.
            if (xmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                _intAvailModifier = xmlGrade.SelectSingleNode("avail")?.ValueAsInt ?? 0;

                _blnLoading = false;
                await RefreshList();
            }
            else
            {
                _blnLoading = false;
                await UpdateDrugInfo();
            }
        }

        private async void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (cboGrade.Enabled != _blnOldGradeEnabled)
            {
                _blnOldGradeEnabled = cboGrade.Enabled;
                if (_blnOldGradeEnabled)
                {
                    cboGrade.SelectedValue = _strOldSelectedGrade;
                }
                await ProcessGradeChanged();
            }
        }

        private async void lstDrug_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            XPathNavigator xmlDrug = null;
            string strSelectedId = lstDrug.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Drug.
                xmlDrug = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
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
                        strMinRating = await (await (await (await strMinRating.CheapReplaceAsync("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo)))
                                    .CheapReplaceAsync("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo)))
                                .CheapReplaceAsync("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo)))
                            .CheapReplaceAsync("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnIsSuccess);
                        intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    nudRating.Minimum = intMinRating;

                    string strMaxRating = xmlRatingNode.Value;
                    int intMaxRating = 0;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                    {
                        strMaxRating = await (await (await (await strMaxRating.CheapReplaceAsync("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo)))
                                    .CheapReplaceAsync("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo)))
                                .CheapReplaceAsync("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo)))
                            .CheapReplaceAsync("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMaxRating, out bool blnIsSuccess);
                        intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    nudRating.Maximum = intMaxRating;
                    if (chkHideOverAvailLimit.Checked)
                    {
                        int intAvailModifier = strForceGrade == "None" ? 0 : _intAvailModifier;
                        while (nudRating.Maximum > intMinRating && !xmlDrug.CheckAvailRestriction(_objCharacter, nudRating.MaximumAsInt, intAvailModifier))
                        {
                            --nudRating.Maximum;
                        }
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
                    {
                        decimal decCostMultiplier = 1 + nudMarkup.Value / 100.0m;
                        if (chkBlackMarketDiscount.Checked)
                            decCostMultiplier *= 0.9m;
                        while (nudRating.Maximum > intMinRating && !xmlDrug.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                        {
                            --nudRating.Maximum;
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

                string strSource = xmlDrug.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                string strPage = xmlDrug.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? xmlDrug.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                SourceString objSource = SourceString.GetSourceString(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                lblSource.Text = objSource.ToString();
                lblSource.SetToolTip(objSource.LanguageBookTooltip);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                Grade objForcedGrade = null;
                if (!string.IsNullOrEmpty(strForceGrade))
                {
                    // Force the Drug to be a particular Grade.
                    if (cboGrade.Enabled)
                        cboGrade.Enabled = false;
                    objForcedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
                    strForceGrade = objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
                }
                else
                {
                    cboGrade.Enabled = !_blnLockGrade;
                    if (_blnLockGrade)
                    {
                        strForceGrade = _objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) ?? cboGrade.SelectedValue?.ToString();
                        objForcedGrade = _objForcedGrade ?? _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == strForceGrade);
                    }
                }

                bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")?.Value);
                chkBlackMarketDiscount.Enabled = blnCanBlackMarketDiscount;
                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                }
                else if (!blnCanBlackMarketDiscount)
                {
                    //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
                PopulateGrades(xmlDrug.SelectSingleNode("nosecondhand") != null || !cboGrade.Enabled && objForcedGrade?.SecondHand != true, false, strForceGrade);
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
                tlpRight.Visible = true;
            }
            else
            {
                tlpRight.Visible = false;
                cboGrade.Enabled = !_blnLockGrade;
                strForceGrade = string.Empty;
                Grade objForcedGrade = null;
                if (_blnLockGrade)
                {
                    strForceGrade = _objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) ?? cboGrade.SelectedValue?.ToString();
                    objForcedGrade = _objForcedGrade ?? _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == strForceGrade);
                }
                PopulateGrades(_blnLockGrade && objForcedGrade?.SecondHand != true, false, strForceGrade);
                chkBlackMarketDiscount.Checked = false;
            }
            _blnLoading = false;
            await UpdateDrugInfo();
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            await UpdateDrugInfo();
        }

        private async void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
            {
                await RefreshList();
            }
            await UpdateDrugInfo();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkHideBannedGrades_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _lstGrades.Clear();
            _lstGrades.AddRange(_objCharacter.GetGradeList(Improvement.ImprovementSource.Drug, chkHideBannedGrades.Checked));
            PopulateGrades();
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            await AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private async void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkShowOnlyAffordItems.Checked)
            {
                await RefreshList();
            }
            await UpdateDrugInfo();
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            await UpdateDrugInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstDrug.SelectedIndex + 1 < lstDrug.Items.Count:
                    lstDrug.SelectedIndex++;
                    break;

                case Keys.Down:
                    {
                        if (lstDrug.Items.Count > 0)
                        {
                            lstDrug.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstDrug.SelectedIndex - 1 >= 0:
                    lstDrug.SelectedIndex--;
                    break;

                case Keys.Up:
                    {
                        if (lstDrug.Items.Count > 0)
                        {
                            lstDrug.SelectedIndex = lstDrug.Items.Count - 1;
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

        #endregion Control Events

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the Drug's information based on the Drug selected and current Rating.
        /// </summary>
        private async ValueTask UpdateDrugInfo()
        {
            XPathNavigator objXmlDrug = null;
            string strSelectedId = lstDrug.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Drug.
                objXmlDrug = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlDrug == null)
            {
                tlpRight.Visible = false;
                return;
            }

            // Extract the Avail and Cost values from the Drug info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            int intRating = nudRating.ValueAsInt;
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail = objXmlDrug.SelectSingleNode("avail")?.Value;
            if (!string.IsNullOrEmpty(strAvail))
            {
                string strAvailExpr = strAvail;
                if (strAvailExpr.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strAvailExpr = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                }

                string strSuffix = string.Empty;
                char chrSuffix = strAvailExpr[strAvailExpr.Length - 1];
                switch (chrSuffix)
                {
                    case 'R':
                        strSuffix = await LanguageManager.GetStringAsync("String_AvailRestricted");
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        break;

                    case 'F':
                        strSuffix = await LanguageManager.GetStringAsync("String_AvailForbidden");
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        break;
                }

                string strPrefix = string.Empty;
                char chrPrefix = strAvailExpr[0];
                if (chrPrefix == '+' || chrPrefix == '-')
                {
                    strPrefix = chrPrefix.ToString(GlobalSettings.InvariantCultureInfo);
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }

                strAvailExpr = await strAvailExpr.CheapReplaceAsync("MinRating",
                        () => nudRating.Minimum.ToString(GlobalSettings.InvariantCultureInfo))
                    .CheapReplaceAsync("Rating", () => nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo));

                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr, out bool blnIsSuccess);
                if (blnIsSuccess)
                {
                    int intAvail = ((double)objProcess).StandardRound() + _intAvailModifier;
                    // Avail cannot go below 0.
                    if (intAvail < 0)
                        intAvail = 0;
                    lblAvail.Text = strPrefix + intAvail.ToString(GlobalSettings.CultureInfo) + strSuffix;
                }
                else
                {
                    lblAvail.Text = strAvail;
                }
            }
            else
            {
                lblAvail.Text = 0.ToString(GlobalSettings.CultureInfo);
            }

            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

            // Cost.
            decimal decItemCost = 0;
            if (chkFree.Checked)
            {
                lblCost.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            }
            else
            {
                string strCost = objXmlDrug.SelectSingleNode("cost")?.Value;
                if (!string.IsNullOrEmpty(strCost))
                {
                    if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
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
                            decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        lblCost.Text = decMax == decimal.MaxValue ?
                            decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+" :
                            decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + " - " + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';

                        decItemCost = decMin;
                    }
                    else
                    {
                        strCost = await (await strCost.CheapReplaceAsync("MinRating", () => nudRating.Minimum.ToString(GlobalSettings.InvariantCultureInfo)))
                            .CheapReplaceAsync("Rating", () => nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                        if (blnIsSuccess)
                        {
                            decItemCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) * _decCostMultiplier;
                            decItemCost *= 1 + nudMarkup.Value / 100.0m;

                            if (chkBlackMarketDiscount.Checked)
                            {
                                decItemCost *= 0.9m;
                            }

                            lblCost.Text = decItemCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        }
                        else
                        {
                            lblCost.Text = strCost + '¥';
                        }
                    }
                }
                else
                    lblCost.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            }

            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);

            // Test required to find the item.
            lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);
            lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);
            tlpRight.Visible = true;
        }

        private bool _blnSkipListRefresh;

        private ValueTask<bool> AnyItemInList(string strCategory = "")
        {
            return RefreshList(strCategory, false);
        }

        private ValueTask<bool> RefreshList(string strCategory = "")
        {
            return RefreshList(strCategory, true);
        }

        private async ValueTask<bool> RefreshList(string strCategory, bool blnDoUIUpdate)
        {
            if ((_blnLoading || _blnSkipListRefresh) && blnDoUIUpdate)
                return false;
            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId)
                ? null
                : _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo)
                                       == strCurrentGradeId);
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                if (objCurrentGrade != null)
                {
                    sbdFilter.Append(" and (not(forcegrade) or forcegrade = \"None\" or forcegrade = ")
                             .Append(objCurrentGrade.Name.CleanXPath()).Append(')');
                    if (objCurrentGrade.SecondHand)
                        sbdFilter.Append(" and not(nosecondhand)");
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            int intOverLimit = 0;
            List<ListItem> lstDrugs = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                foreach (XPathNavigator xmlDrug in _xmlBaseDrugDataNode.Select(_strNodeXPath + strFilter))
                {
                    bool blnIsForceGrade = xmlDrug.SelectSingleNode("forcegrade") == null;
                    if (objCurrentGrade != null && blnIsForceGrade && ImprovementManager
                                                                      .GetCachedImprovementListForValueOf(
                                                                          _objCharacter,
                                                                          Improvement.ImprovementType.DisableDrugGrade)
                                                                      .Any(x => objCurrentGrade.Name.Contains(
                                                                               x.ImprovedName)))
                        continue;

                    string strMaxRating = xmlDrug.SelectSingleNode("rating")?.Value;
                    string strMinRating = xmlDrug.SelectSingleNode("minrating")?.Value;
                    int intMinRating = 1;
                    // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                    if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out int intMaxRating)
                        || !string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnIsSuccess);
                        intMinRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                        objProcess = CommonFunctions.EvaluateInvariantXPath(strMaxRating, out blnIsSuccess);
                        intMaxRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                        if (intMaxRating < intMinRating)
                            continue;
                    }

                    if (ParentVehicle == null && !xmlDrug.RequirementsMet(_objCharacter))
                        continue;

                    if (!blnDoUIUpdate)
                    {
                        return true;
                    }

                    if (chkHideOverAvailLimit.Checked
                        && !xmlDrug.CheckAvailRestriction(_objCharacter, intMinRating,
                                                          blnIsForceGrade ? 0 : _intAvailModifier))
                    {
                        ++intOverLimit;
                        continue;
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
                    {
                        decimal decCostMultiplier = 1 + nudMarkup.Value / 100.0m;
                        if (_setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")?.Value))
                            decCostMultiplier *= 0.9m;
                        if (!xmlDrug.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier))
                        {
                            ++intOverLimit;
                            continue;
                        }
                    }
                    
                    lstDrugs.Add(new ListItem(xmlDrug.SelectSingleNode("id")?.Value,
                                              xmlDrug.SelectSingleNodeAndCacheExpression("translate")?.Value
                                              ?? xmlDrug.SelectSingleNode("name")?.Value));
                }

                if (blnDoUIUpdate)
                {
                    lstDrugs.Sort(CompareListItems.CompareNames);
                    if (intOverLimit > 0)
                    {
                        // Add after sort so that it's always at the end
                        lstDrugs.Add(new ListItem(string.Empty,
                                                  string.Format(GlobalSettings.CultureInfo,
                                                                await LanguageManager.GetStringAsync(
                                                                    "String_RestrictedItemsHidden"),
                                                                intOverLimit)));
                    }

                    string strOldSelected = lstDrug.SelectedValue?.ToString();
                    _blnLoading = true;
                    lstDrug.BeginUpdate();
                    lstDrug.PopulateWithListItems(lstDrugs);
                    _blnLoading = false;
                    if (!string.IsNullOrEmpty(strOldSelected))
                        lstDrug.SelectedValue = strOldSelected;
                    else
                        lstDrug.SelectedIndex = -1;

                    lstDrug.EndUpdate();
                }

                return lstDrugs?.Count > 0;
            }
            finally
            {
                if (lstDrugs != null)
                    Utils.ListItemListPool.Return(lstDrugs);
            }
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
        private async ValueTask AcceptForm()
        {
            string strSelectedId = lstDrug.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if (cboGrade.Text.StartsWith('*'))
            {
                Program.ShowMessageBox(this,
                    await LanguageManager.GetStringAsync("Message_BannedGrade"),
                    await LanguageManager.GetStringAsync("MessageTitle_BannedGrade"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XPathNavigator objDrugNode = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            if (objDrugNode == null)
                return;

            if (!objDrugNode.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync("String_SelectPACKSKit_Drug")))
                return;

            string strForceGrade = objDrugNode.SelectSingleNode("forcegrade")?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = cboGrade.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == strForceGrade);
                else
                    return;
            }

            _sStrSelectGrade = SelectedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
            SelectedDrug = strSelectedId;
            SelectedRating = nudRating.ValueAsInt;
            BlackMarketDiscount = chkBlackMarketDiscount.Checked;
            Markup = nudMarkup.Value;

            DialogResult = DialogResult.OK;
        }

        private bool _blnPopulatingGrades;

        /// <summary>
        /// Populate the list of Drug Grades.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Second-Hand Grades should be added to the list.</param>
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
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGrade))
                {
                    foreach (Grade objWareGrade in _lstGrades)
                    {
                        if (objWareGrade.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == _strNoneGradeId
                            && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != _strNoneGradeId))
                            continue;
                        //if (ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.DisableDrugGrade).Any(x => objWareGrade.Name.Contains(x.ImprovedName)))
                        //    continue;
                        if (blnIgnoreSecondHand && objWareGrade.SecondHand)
                            continue;
                        /*
                        if (blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.BannedDrugGrades.Any(s => objWareGrade.Name.Contains(s)))
                            continue;
                        if (!blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.BannedDrugGrades.Any(s => objWareGrade.Name.Contains(s)))
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceId.ToString("D"), '*' + objWareGrade.CurrentDisplayName));
                        }
                        else
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceId.ToString("D"), objWareGrade.CurrentDisplayName));
                        }*/
                    }

                    string strOldSelected = cboGrade.SelectedValue?.ToString();
                    bool blnOldSkipListRefresh = _blnSkipListRefresh;
                    if (strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId
                                                         || lstGrade.Any(x => x.Value.ToString() == strOldSelected))
                        _blnSkipListRefresh = true;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    cboGrade.BeginUpdate();
                    cboGrade.PopulateWithListItems(lstGrade);
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
            }
            _blnPopulatingGrades = false;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
