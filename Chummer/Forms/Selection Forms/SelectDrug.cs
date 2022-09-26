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
using System.Threading;
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

            _lstGrades = _objCharacter.GetGradesList(Improvement.ImprovementSource.Drug);
            _strNoneGradeId = _lstGrades.Find(x => x.Name == "None")?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseDrugDataNode));
        }

        private async void SelectDrug_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = true);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true);
                await chkHideBannedGrades.DoThreadSafeAsync(x => x.Visible = false);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                });
            }
            else
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false);
                await chkHideBannedGrades.DoThreadSafeAsync(x => x.Visible = !_objCharacter.IgnoreRules);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(
                        GlobalSettings.CultureInfo, x.Text,
                        _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                });
            }

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                await txtSearch.DoThreadSafeAsync(x =>
                {
                    x.Text = DefaultSearchText;
                    x.Enabled = false;
                });
            }

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            await PopulateGrades(false, true, _objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) ?? string.Empty);

            await cboGrade.DoThreadSafeAsync(x =>
            {
                if (_objForcedGrade != null)
                    x.SelectedValue = _objForcedGrade.SourceId.ToString();
                else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                    x.SelectedValue = _sStrSelectGrade;
                if (x.SelectedIndex == -1 && x.Items.Count > 0)
                    x.SelectedIndex = 0;
            });

            _blnLoading = false;
            await RefreshList();
        }

        private async void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            await ProcessGradeChanged();
        }

        private async ValueTask ProcessGradeChanged(CancellationToken token = default)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;

            XPathNavigator xmlGrade = null;
            // Retrieve the information for the selected Grade.
            string strSelectedGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (await cboGrade.DoThreadSafeFuncAsync(x => x.Enabled, token: token) && strSelectedGrade != null)
                _strOldSelectedGrade = strSelectedGrade;
            if (!string.IsNullOrEmpty(strSelectedGrade))
                xmlGrade = _xmlBaseDrugDataNode.SelectSingleNode("grades/grade[id = " + strSelectedGrade.CleanXPath() + ']');

            // Update the Cost multipliers based on the Grade that has been selected.
            if (xmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                _intAvailModifier = xmlGrade.SelectSingleNode("avail")?.ValueAsInt ?? 0;

                _blnLoading = false;
                await RefreshList(token: token);
            }
            else
            {
                _blnLoading = false;
                await UpdateDrugInfo(token);
            }
        }

        private async void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (await cboGrade.DoThreadSafeFuncAsync(x => x.Enabled) != _blnOldGradeEnabled)
            {
                await cboGrade.DoThreadSafeAsync(x =>
                {
                    _blnOldGradeEnabled = x.Enabled;
                    if (_blnOldGradeEnabled)
                    {
                        x.SelectedValue = _strOldSelectedGrade;
                    }
                });
                await ProcessGradeChanged();
            }
        }

        private async void lstDrug_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            XPathNavigator xmlDrug = null;
            string strSelectedId = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
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
                        strMinRating = await strMinRating
                                             .CheapReplaceAsync("MaximumSTR",
                                                                () => (ParentVehicle != null
                                                                        ? Math.Max(1, ParentVehicle.TotalBody * 2)
                                                                        : _objCharacter.STR.TotalMaximum)
                                                                    .ToString(GlobalSettings.InvariantCultureInfo))
                                             .CheapReplaceAsync("MaximumAGI",
                                                                () => (ParentVehicle != null
                                                                        ? Math.Max(1, ParentVehicle.Pilot * 2)
                                                                        : _objCharacter.AGI.TotalMaximum)
                                                                    .ToString(GlobalSettings.InvariantCultureInfo))
                                             .CheapReplaceAsync("MinimumSTR",
                                                                () => (ParentVehicle?.TotalBody ?? 3).ToString(
                                                                    GlobalSettings.InvariantCultureInfo))
                                             .CheapReplaceAsync("MinimumAGI",
                                                                () => (ParentVehicle?.Pilot ?? 3).ToString(
                                                                    GlobalSettings.InvariantCultureInfo));

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating);
                        intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    await nudRating.DoThreadSafeAsync(x => x.Minimum = intMinRating);

                    string strMaxRating = xmlRatingNode.Value;
                    int intMaxRating = 0;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                    {
                        strMaxRating = await strMaxRating
                                             .CheapReplaceAsync("MaximumSTR",
                                                                () => (ParentVehicle != null
                                                                        ? Math.Max(1, ParentVehicle.TotalBody * 2)
                                                                        : _objCharacter.STR.TotalMaximum)
                                                                    .ToString(GlobalSettings.InvariantCultureInfo))
                                             .CheapReplaceAsync("MaximumAGI",
                                                                () => (ParentVehicle != null
                                                                        ? Math.Max(1, ParentVehicle.Pilot * 2)
                                                                        : _objCharacter.AGI.TotalMaximum)
                                                                    .ToString(GlobalSettings.InvariantCultureInfo))
                                             .CheapReplaceAsync("MinimumSTR",
                                                                () => (ParentVehicle?.TotalBody ?? 3).ToString(
                                                                    GlobalSettings.InvariantCultureInfo))
                                             .CheapReplaceAsync("MinimumAGI",
                                                                () => (ParentVehicle?.Pilot ?? 3).ToString(
                                                                    GlobalSettings.InvariantCultureInfo));

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMaxRating);
                        intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked))
                    {
                        int intAvailModifier = strForceGrade == "None" ? 0 : _intAvailModifier;
                        while (intMaxRating > intMinRating
                               && !await xmlDrug.CheckAvailRestrictionAsync(
                                   _objCharacter, intMaxRating, intAvailModifier))
                        {
                            --intMaxRating;
                        }
                    }
                    if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked)
                        && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked))
                    {
                        decimal decCostMultiplier = 1 + nudMarkup.Value / 100.0m;
                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked))
                            decCostMultiplier *= 0.9m;
                        while (intMaxRating > intMinRating
                               && !await xmlDrug.CheckNuyenRestrictionAsync(
                                   _objCharacter.Nuyen, decCostMultiplier, intMaxRating))
                        {
                            --intMaxRating;
                        }
                    }
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Maximum = intMaxRating;
                        x.Value = x.Minimum;
                        x.Enabled = x.Minimum != x.Maximum;
                        x.Visible = true;
                    });
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false);
                    await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true);
                }
                else
                {
                    await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true);
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true);
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Minimum = 0;
                        x.Value = 0;
                        x.Visible = false;
                    });
                }

                string strSource = xmlDrug.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                string strPage = (await xmlDrug.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? xmlDrug.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                await objSource.SetControlAsync(lblSource);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()));

                Grade objForcedGrade = null;
                if (!string.IsNullOrEmpty(strForceGrade))
                {
                    // Force the Drug to be a particular Grade.
                    await cboGrade.DoThreadSafeAsync(x =>
                    {
                        if (x.Enabled)
                            x.Enabled = false;
                    });
                    objForcedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
                    strForceGrade = objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
                }
                else
                {
                    await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade);
                    if (_blnLockGrade)
                    {
                        strForceGrade = _objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) ?? cboGrade.SelectedValue?.ToString();
                        objForcedGrade = _objForcedGrade ?? _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == strForceGrade);
                    }
                }

                bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")?.Value);
                await chkBlackMarketDiscount.DoThreadSafeAsync(x =>
                {
                    x.Enabled = blnCanBlackMarketDiscount;
                    if (!x.Checked)
                    {
                        x.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                    }
                    else if (!blnCanBlackMarketDiscount)
                    {
                        //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                        x.Checked = false;
                    }
                });

                // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
                await PopulateGrades(xmlDrug.SelectSingleNode("nosecondhand") != null || !await cboGrade.DoThreadSafeFuncAsync(x => x.Enabled) && objForcedGrade?.SecondHand != true, false, strForceGrade);
                /*
                string strNotes = xmlDrug.SelectSingleNode("altnotes")?.Value ?? xmlDrug.SelectSingleNode("notes")?.Value;
                if (!string.IsNullOrEmpty(strNotes))
                {
                    await lblDrugNotesLabel.DoThreadSafeAsync(x => x.Visible = true);
                    await lblDrugNotes.DoThreadSafeAsync(x =>
                    {
                        x.Text = strNotes;
                        x.Visible = true;
                    });
                }
                else
                {
                    await lblDrugNotes.DoThreadSafeAsync(x => x.Visible = false);
                    await lblDrugNotesLabel.DoThreadSafeAsync(x => x.Visible = false);
                }*/
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true);
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade);
                strForceGrade = string.Empty;
                Grade objForcedGrade = null;
                if (_blnLockGrade)
                {
                    strForceGrade = _objForcedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) ?? await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                    objForcedGrade = _objForcedGrade ?? _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == strForceGrade);
                }
                await PopulateGrades(_blnLockGrade && objForcedGrade?.SecondHand != true, false, strForceGrade);
                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Checked = false);
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
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked))
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
            Close();
        }

        private async void chkHideBannedGrades_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _lstGrades.Clear();
            _lstGrades.AddRange(await _objCharacter.GetGradesListAsync(Improvement.ImprovementSource.Drug, await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked)));
            await PopulateGrades();
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
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked))
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
        private async ValueTask UpdateDrugInfo(CancellationToken token = default)
        {
            XPathNavigator objXmlDrug = null;
            string strSelectedId = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Drug.
                objXmlDrug = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlDrug == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token);
                return;
            }

            // Extract the Avail and Cost values from the Drug info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token);
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
                        strSuffix = await LanguageManager.GetStringAsync("String_AvailRestricted", token: token);
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        break;

                    case 'F':
                        strSuffix = await LanguageManager.GetStringAsync("String_AvailForbidden", token: token);
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
                                                                    async () =>
                                                                        (await nudRating.DoThreadSafeFuncAsync(
                                                                            x => x.Minimum, token: token))
                                                                        .ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                 .CheapReplaceAsync(
                                                     "Rating",
                                                     () => intRating.ToString(GlobalSettings.InvariantCultureInfo), token: token);

                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strAvailExpr, token);
                if (blnIsSuccess)
                {
                    int intAvail = ((double)objProcess).StandardRound() + _intAvailModifier;
                    // Avail cannot go below 0.
                    if (intAvail < 0)
                        intAvail = 0;
                    await lblAvail.DoThreadSafeAsync(x => x.Text = strPrefix + intAvail.ToString(GlobalSettings.CultureInfo) + strSuffix, token: token);
                }
                else
                {
                    await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token);
                }
            }
            else
            {
                await lblAvail.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo), token: token);
            }

            strAvail = await lblAvail.DoThreadSafeFuncAsync(x => x.Text, token: token);
            await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token);

            // Cost.
            decimal decItemCost = 0;
            if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token))
            {
                await lblCost.DoThreadSafeAsync(x => x.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
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

                        await lblCost.DoThreadSafeAsync(x => x.Text = decMax == decimal.MaxValue
                                                            ? decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol") + '+'
                                                            : decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo) + " - "
                                                            + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);

                        decItemCost = decMin;
                    }
                    else
                    {
                        strCost = await (await strCost.CheapReplaceAsync("MinRating", () => nudRating.Minimum.ToString(GlobalSettings.InvariantCultureInfo), token: token))
                            .CheapReplaceAsync("Rating", () => nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo), token: token);

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token);
                        if (blnIsSuccess)
                        {
                            decItemCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) * _decCostMultiplier;
                            decItemCost *= 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m);

                            if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                            {
                                decItemCost *= 0.9m;
                            }

                            await lblCost.DoThreadSafeAsync(x => x.Text = decItemCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                        }
                        else
                        {
                            await lblCost.DoThreadSafeAsync(x => x.Text = strCost + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                        }
                    }
                }
                else
                    await lblCost.DoThreadSafeAsync(x => x.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
            }

            await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token)
                         .ContinueWith(
                             y => lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result), token: token), token)
                         .Unwrap();

            // Test required to find the item.
            string strTest = await _objCharacter.AvailTestAsync(decItemCost, strAvail, token);
            await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token);
            await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token);
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token);
        }

        private bool _blnSkipListRefresh;

        private ValueTask<bool> AnyItemInList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, false, token);
        }

        private ValueTask<bool> RefreshList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, true, token);
        }

        private async ValueTask<bool> RefreshList(string strCategory, bool blnDoUIUpdate, CancellationToken token = default)
        {
            if ((_blnLoading || _blnSkipListRefresh) && blnDoUIUpdate)
                return false;
            string strCurrentGradeId = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId)
                ? null
                : _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo)
                                       == strCurrentGradeId);
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token)).Append(')');
                if (objCurrentGrade != null)
                {
                    sbdFilter.Append(" and (not(forcegrade) or forcegrade = \"None\" or forcegrade = ")
                             .Append(objCurrentGrade.Name.CleanXPath()).Append(')');
                    if (objCurrentGrade.SecondHand)
                        sbdFilter.Append(" and not(nosecondhand)");
                }

                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            int intOverLimit = 0;
            List<ListItem> lstDrugs = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                bool blnFree = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                decimal decBaseCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m;
                foreach (XPathNavigator xmlDrug in _xmlBaseDrugDataNode.Select(_strNodeXPath + strFilter))
                {
                    bool blnIsForceGrade = xmlDrug.SelectSingleNode("forcegrade") == null;
                    if (objCurrentGrade != null && blnIsForceGrade && (await ImprovementManager
                            .GetCachedImprovementListForValueOfAsync(
                                _objCharacter,
                                Improvement.ImprovementType.DisableDrugGrade, token: token))
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
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating, token);
                        intMinRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                        (blnIsSuccess, objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMaxRating, token);
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

                    if (blnHideOverAvailLimit
                        && !await xmlDrug.CheckAvailRestrictionAsync(_objCharacter, intMinRating,
                                                                     blnIsForceGrade ? 0 : _intAvailModifier, token))
                    {
                        ++intOverLimit;
                        continue;
                    }

                    if (blnShowOnlyAffordItems && !blnFree)
                    {
                        decimal decCostMultiplier = decBaseCostMultiplier;
                        if (_setBlackMarketMaps.Contains(xmlDrug.SelectSingleNode("category")?.Value))
                            decCostMultiplier *= 0.9m;
                        if (!await xmlDrug.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier, token: token))
                        {
                            ++intOverLimit;
                            continue;
                        }
                    }
                    
                    lstDrugs.Add(new ListItem(xmlDrug.SelectSingleNode("id")?.Value,
                                              (await xmlDrug.SelectSingleNodeAndCacheExpressionAsync("translate", token: token))?.Value
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
                                                                    "String_RestrictedItemsHidden", token: token),
                                                                intOverLimit)));
                    }

                    string strOldSelected = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                    _blnLoading = true;
                    await lstDrug.PopulateWithListItemsAsync(lstDrugs, token: token);
                    _blnLoading = false;
                    await lstDrug.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        else
                            x.SelectedIndex = -1;
                    }, token: token);
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
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strSelectedId = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if (await cboGrade.DoThreadSafeFuncAsync(x => x.Text.StartsWith('*'), token: token))
            {
                Program.ShowMessageBox(this,
                    await LanguageManager.GetStringAsync("Message_BannedGrade", token: token),
                    await LanguageManager.GetStringAsync("MessageTitle_BannedGrade", token: token),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XPathNavigator objDrugNode = _xmlBaseDrugDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            if (objDrugNode == null)
                return;

            if (!objDrugNode.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync("String_SelectPACKSKit_Drug", token: token)))
                return;

            string strForceGrade = objDrugNode.SelectSingleNode("forcegrade")?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.Find(x => x.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo) == strForceGrade);
                else
                    return;
            }

            _sStrSelectGrade = SelectedGrade?.SourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
            SelectedDrug = strSelectedId;
            SelectedRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token);
            BlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token);
            Markup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token);
        }

        private bool _blnPopulatingGrades;

        /// <summary>
        /// Populate the list of Drug Grades.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Second-Hand Grades should be added to the list.</param>
        /// <param name="blnForce">Force grades to be repopulated.</param>
        /// <param name="strForceGrade">If not empty, force this grade to be selected.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async ValueTask PopulateGrades(bool blnIgnoreSecondHand = false, bool blnForce = false, string strForceGrade = "", CancellationToken token = default)
        {
            if (_blnPopulatingGrades)
                return;
            _blnPopulatingGrades = true;
            if (blnForce || blnIgnoreSecondHand != _blnIgnoreSecondHand || _strForceGrade != strForceGrade || await cboGrade.DoThreadSafeFuncAsync(x => x.Items.Count, token: token) == 0)
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

                    string strOldSelected = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                    bool blnOldSkipListRefresh = _blnSkipListRefresh;
                    if (strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId
                                                         || lstGrade.Any(x => x.Value.ToString() == strOldSelected))
                        _blnSkipListRefresh = true;
                    bool blnOldLoading = _blnLoading;
                    _blnLoading = true;
                    await cboGrade.PopulateWithListItemsAsync(lstGrade, token: token);
                    _blnLoading = blnOldLoading;
                    await cboGrade.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strForceGrade))
                            x.SelectedValue = strForceGrade;
                        else if (x.SelectedIndex <= 0 && !string.IsNullOrWhiteSpace(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        if (x.SelectedIndex == -1 && lstGrade.Count > 0)
                            x.SelectedIndex = 0;
                    }, token: token);

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
