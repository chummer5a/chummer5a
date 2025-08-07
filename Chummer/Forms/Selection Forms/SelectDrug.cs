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
using System.Buffers;
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
        private int _intLoading = 1;

        private const string _strNodeXPath = "Drugs/Drug";
        private static string _sStrSelectGrade = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private HashSet<string> _setDisallowedGrades;
        private string _strForceGrade = string.Empty;
        private HashSet<string> _setBlackMarketMaps;
        private readonly XPathNavigator _xmlBaseDrugDataNode;

        #region Control Events

        public SelectDrug(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _xmlBaseDrugDataNode = objCharacter.LoadDataXPath("drugcomponents.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _lstGrades = _objCharacter.GetGradesList(Improvement.ImprovementSource.Drug);
            _strNoneGradeId = _lstGrades.Find(x => x.Name == "None")?.SourceIDString;
            _setDisallowedGrades = Utils.StringHashSetPool.Get();
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            Disposed += (sender, args) =>
            {
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
                Utils.StringHashSetPool.Return(ref _setDisallowedGrades);
            };
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseDrugDataNode));
        }

        private async void SelectDrug_Load(object sender, EventArgs e)
        {
            bool blnBlackMarketDiscount = await _objCharacter.GetBlackMarketDiscountAsync().ConfigureAwait(false);
            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = blnBlackMarketDiscount).ConfigureAwait(false);

            if (await _objCharacter.GetCreatedAsync().ConfigureAwait(false))
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                }).ConfigureAwait(false);
            }
            else
            {
                await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await nudMarkup.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                int intMaxAvail = await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).GetMaximumAvailabilityAsync().ConfigureAwait(false);
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                    x.Visible = true;
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                }).ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                await txtSearch.DoThreadSafeAsync(x =>
                {
                    x.Text = DefaultSearchText;
                    x.Enabled = false;
                }).ConfigureAwait(false);
            }

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            await PopulateGrades(null, true, _objForcedGrade?.SourceIDString ?? string.Empty).ConfigureAwait(false);

            await cboGrade.DoThreadSafeAsync(x =>
            {
                if (_objForcedGrade != null)
                    x.SelectedValue = _objForcedGrade.SourceIDString;
                else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                    x.SelectedValue = _sStrSelectGrade;
                if (x.SelectedIndex == -1 && x.Items.Count > 0)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);

            Interlocked.Decrement(ref _intLoading);
            await RefreshList().ConfigureAwait(false);
        }

        private async void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            await ProcessGradeChanged().ConfigureAwait(false);
        }

        private async Task ProcessGradeChanged(CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref _intLoading, 1, 0) > 0)
                return;
            XPathNavigator xmlGrade = null;
            try
            {
                // Retrieve the information for the selected Grade.
                string strSelectedGrade = await cboGrade
                                                .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                                .ConfigureAwait(false);
                if (await cboGrade.DoThreadSafeFuncAsync(x => x.Enabled, token: token).ConfigureAwait(false)
                    && strSelectedGrade != null)
                    _strOldSelectedGrade = strSelectedGrade;
                if (!string.IsNullOrEmpty(strSelectedGrade))
                {
                    xmlGrade = _xmlBaseDrugDataNode.TryGetNodeByNameOrId("grades/grade", strSelectedGrade);
                }

                // Update the Cost multipliers based on the Grade that has been selected.
                if (xmlGrade != null)
                {
                    decimal.TryParse(xmlGrade.SelectSingleNodeAndCacheExpression("cost", token)?.Value,
                            System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _decCostMultiplier);
                    _intAvailModifier
                        = xmlGrade.SelectSingleNodeAndCacheExpression("avail", token)?.ValueAsInt ?? 0;

                    await RefreshList(token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _intLoading);
            }

            if (xmlGrade != null)
                await RefreshList(token: token).ConfigureAwait(false);
            else
                await UpdateDrugInfo(token).ConfigureAwait(false);
        }

        private async void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (await cboGrade.DoThreadSafeFuncAsync(x => x.Enabled).ConfigureAwait(false) != _blnOldGradeEnabled)
            {
                await cboGrade.DoThreadSafeAsync(x =>
                {
                    _blnOldGradeEnabled = x.Enabled;
                    if (_blnOldGradeEnabled)
                    {
                        x.SelectedValue = _strOldSelectedGrade;
                    }
                }).ConfigureAwait(false);
                await ProcessGradeChanged().ConfigureAwait(false);
            }
        }

        private async void lstDrug_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _intLoading, 1, 0) > 0)
                return;
            try
            {
                XPathNavigator xmlDrug = null;
                string strSelectedId = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                                    .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    // Retrieve the information for the selected piece of Drug.
                    xmlDrug = _xmlBaseDrugDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
                }

                string strForceGrade;
                if (xmlDrug != null)
                {
                    Dictionary<string, int> dicVehicleValues = null;
                    if (ParentVehicle != null)
                    {
                        int intVehicleBody = await ParentVehicle.GetTotalBodyAsync().ConfigureAwait(false);
                        int intVehiclePilot = await ParentVehicle.GetPilotAsync().ConfigureAwait(false);
                        dicVehicleValues = new Dictionary<string, int>(4)
                        {
                            { "STRMaximum", Math.Max(1, intVehicleBody * 2) },
                            { "AGIMaximum", Math.Max(1, intVehiclePilot * 2) },
                            { "STRMinimum", Math.Max(1, intVehicleBody) },
                            { "AGIMinimum", Math.Max(1, intVehiclePilot) }
                        };
                    }

                    strForceGrade = xmlDrug.SelectSingleNodeAndCacheExpression("forcegrade")?.Value;
                    // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
                    XPathNavigator xmlRatingNode = xmlDrug.SelectSingleNodeAndCacheExpression("rating");
                    if (xmlRatingNode != null)
                    {
                        string strMinRating = xmlDrug.SelectSingleNodeAndCacheExpression("minrating")?.Value;
                        int intMinRating = 1;
                        // Not a simple integer, so we need to start mucking around with strings
                        if (strMinRating.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                        {
                            strMinRating = await _objCharacter.ProcessAttributesInXPathAsync(strMinRating, dicVehicleValues).ConfigureAwait(false);
                            (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                           .EvaluateInvariantXPathAsync(strMinRating)
                                                                           .ConfigureAwait(false);
                            intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                        }
                        else
                            intMinRating = decValue.StandardRound();

                        await nudRating.DoThreadSafeAsync(x => x.Minimum = intMinRating).ConfigureAwait(false);

                        string strMaxRating = xmlRatingNode.Value;
                        int intMaxRating = 0;
                        // Not a simple integer, so we need to start mucking around with strings
                        if (strMaxRating.DoesNeedXPathProcessingToBeConvertedToNumber(out decValue))
                        {
                            strMaxRating = await _objCharacter.ProcessAttributesInXPathAsync(strMaxRating, dicVehicleValues).ConfigureAwait(false);
                            (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                           .EvaluateInvariantXPathAsync(strMaxRating)
                                                                           .ConfigureAwait(false);
                            intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                        }
                        else
                            intMaxRating = decValue.StandardRound();

                        if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            int intAvailModifier = strForceGrade == "None" ? 0 : _intAvailModifier;
                            while (intMaxRating > intMinRating
                                   && !await xmlDrug.CheckAvailRestrictionAsync(
                                       _objCharacter, intMaxRating, intAvailModifier).ConfigureAwait(false))
                            {
                                --intMaxRating;
                            }
                        }

                        if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                            && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            decimal decCostMultiplier = 1 + nudMarkup.Value / 100.0m;
                            if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked)
                                                            .ConfigureAwait(false))
                                decCostMultiplier *= 0.9m;
                            decimal decNuyen = await _objCharacter.GetAvailableNuyenAsync().ConfigureAwait(false);
                            while (intMaxRating > intMinRating
                                   && !await xmlDrug.CheckNuyenRestrictionAsync(
                                       _objCharacter, decNuyen, decCostMultiplier, intMaxRating).ConfigureAwait(false))
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
                        }).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = 0;
                            x.Value = 0;
                            x.Visible = false;
                        }).ConfigureAwait(false);
                    }

                    string strSource
                        = xmlDrug.SelectSingleNodeAndCacheExpression("source")?.Value
                          ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strPage
                        = xmlDrug.SelectSingleNodeAndCacheExpression("altpage")
                        ?.Value ?? xmlDrug.SelectSingleNodeAndCacheExpression("page")
                        ?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    SourceString objSource = await SourceString.GetSourceStringAsync(
                        strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSource.SetControlAsync(lblSource).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()))
                                        .ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(strForceGrade))
                    {
                        // Force the Drug to be a particular Grade.
                        await cboGrade.DoThreadSafeAsync(x =>
                        {
                            if (x.Enabled)
                                x.Enabled = false;
                        }).ConfigureAwait(false);
                        Grade objForcedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
                        strForceGrade = objForcedGrade?.SourceIDString;
                    }
                    else
                    {
                        await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade).ConfigureAwait(false);
                        if (_blnLockGrade)
                        {
                            strForceGrade = _objForcedGrade?.SourceIDString ?? cboGrade.SelectedValue?.ToString();
                        }
                    }

                    bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(
                        xmlDrug.SelectSingleNodeAndCacheExpression("category")
                        ?.Value);
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
                    }).ConfigureAwait(false);

                    // We will need to rebuild the Grade list since certain categories of 'ware disallow certain grades (e.g. Used for cultured bioware) and ForceGrades can change.
                    HashSet<string> setDisallowedGrades = null;
                    if (xmlDrug.SelectSingleNodeAndCacheExpression("bannedgrades") != null)
                    {
                        setDisallowedGrades = new HashSet<string>();
                        foreach (XPathNavigator objNode in xmlDrug.SelectAndCacheExpression("bannedgrades/grade"))
                        {
                            setDisallowedGrades.Add(objNode.Value);
                        }
                    }

                    await PopulateGrades(setDisallowedGrades, false, strForceGrade).ConfigureAwait(false);
                    /*
                    string strNotes = xmlDrug.SelectSingleNodeAndCacheExpression("altnotes")?.Value ?? xmlDrug.SelectSingleNodeAndCacheExpression("notes")?.Value;
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
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade).ConfigureAwait(false);
                    strForceGrade = string.Empty;
                    if (_blnLockGrade)
                    {
                        strForceGrade = _objForcedGrade?.SourceIDString
                                        ?? await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                                         .ConfigureAwait(false);
                    }

                    await PopulateGrades(null, false, strForceGrade).ConfigureAwait(false);
                    await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _intLoading);
            }

            await UpdateDrugInfo().ConfigureAwait(false);
        }

        private async void nudRating_ValueChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            await UpdateDrugInfo().ConfigureAwait(false);
        }

        private async void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false) && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                await RefreshList().ConfigureAwait(false);
            }
            await UpdateDrugInfo().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void chkHideBannedGrades_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            _lstGrades.Clear();
            _lstGrades.AddRange(await _objCharacter.GetGradesListAsync(Improvement.ImprovementSource.Drug, await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)).ConfigureAwait(false));
            await PopulateGrades().ConfigureAwait(false);
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            await AcceptForm().ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList().ConfigureAwait(false);
        }

        private async void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                await RefreshList().ConfigureAwait(false);
            }
            await UpdateDrugInfo().ConfigureAwait(false);
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            await UpdateDrugInfo().ConfigureAwait(false);
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
                case Keys.Up when lstDrug.SelectedIndex >= 1:
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
                txtSearch.Select(txtSearch.TextLength, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Whether the item has no cost.
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
        /// Whether the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public Vehicle ParentVehicle { get; set; }

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
        private async Task UpdateDrugInfo(CancellationToken token = default)
        {
            XPathNavigator objXmlDrug = null;
            string strSelectedId = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Drug.
                objXmlDrug = _xmlBaseDrugDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
            }
            if (objXmlDrug == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                return;
            }

            // Extract the Avail and Cost values from the Drug info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail = objXmlDrug.SelectSingleNodeAndCacheExpression("avail", token)?.Value;
            if (!string.IsNullOrEmpty(strAvail))
            {
                string strAvailExpr = strAvail.ProcessFixedValuesString(intRating);
                string strSuffix = string.Empty;
                char chrSuffix = strAvailExpr[strAvailExpr.Length - 1];
                switch (chrSuffix)
                {
                    case 'R':
                        strSuffix = await LanguageManager.GetStringAsync("String_AvailRestricted", token: token).ConfigureAwait(false);
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        break;

                    case 'F':
                        strSuffix = await LanguageManager.GetStringAsync("String_AvailForbidden", token: token).ConfigureAwait(false);
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        break;
                }

                string strPrefix = string.Empty;
                char chrPrefix = strAvailExpr[0];
                if (chrPrefix == '+' || chrPrefix == '-')
                {
                    strPrefix = chrPrefix.ToString(GlobalSettings.InvariantCultureInfo);
                    strAvailExpr = strAvailExpr.Substring(1);
                }

                if (strAvailExpr.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strAvailExpr = await strAvailExpr.CheapReplaceAsync("MinRating",
                                                                        async () =>
                                                                            (await nudRating.DoThreadSafeFuncAsync(
                                                                                x => x.MinimumAsInt, token: token).ConfigureAwait(false))
                                                                            .ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                     .CheapReplaceAsync(
                                                         "Rating",
                                                         () => intRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strAvailExpr, token).ConfigureAwait(false);
                    if (blnIsSuccess)
                    {
                        int intAvail = ((double)objProcess).StandardRound() + _intAvailModifier;
                        // Avail cannot go below 0.
                        if (intAvail < 0)
                            intAvail = 0;
                        strAvail = strPrefix + intAvail.ToString(GlobalSettings.CultureInfo) + strSuffix;
                    }
                }
                else
                {
                    int intAvail = decValue.StandardRound() + _intAvailModifier;
                    // Avail cannot go below 0.
                    if (intAvail < 0)
                        intAvail = 0;
                    strAvail = strPrefix + intAvail.ToString(GlobalSettings.CultureInfo) + strSuffix;
                }
            }
            else
            {
                strAvail = 0.ToString(GlobalSettings.CultureInfo);
            }

            bool blnShowAvail = !string.IsNullOrEmpty(strAvail);
            await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);
            await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = blnShowAvail, token: token).ConfigureAwait(false);

            // Cost.
            decimal decItemCost = 0;
            string strNuyen = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
            {
                await lblCost.DoThreadSafeAsync(x => x.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + strNuyen, token: token).ConfigureAwait(false);
            }
            else
            {
                string strCost = objXmlDrug.SelectSingleNodeAndCacheExpression("cost", token)?.Value;
                if (!string.IsNullOrEmpty(strCost))
                {
                    strCost = strCost.ProcessFixedValuesString(intRating);
                    // Check for a Variable Cost.
                    if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.SplitFixedSizePooledArray('-', 2);
                            try
                            {
                                decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                                decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                            }
                            finally
                            {
                                ArrayPool<string>.Shared.Return(strValues);
                            }
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        await lblCost.DoThreadSafeAsync(x => x.Text = decMax == decimal.MaxValue
                                                            ? decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo) + strNuyen + '+'
                                                            : decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo) + " - "
                                                            + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo) + strNuyen, token: token).ConfigureAwait(false);

                        decItemCost = decMin;
                    }
                    else if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decItemCost))
                    {
                        strCost = await (await strCost.CheapReplaceAsync("MinRating", async () => (await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false))
                                        .CheapReplaceAsync("Rating", () => intRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token).ConfigureAwait(false);
                        if (blnIsSuccess)
                        {
                            decItemCost = Convert.ToDecimal((double)objProcess) * _decCostMultiplier;
                            decItemCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;

                            if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            {
                                decItemCost *= 0.9m;
                            }

                            string strText = decItemCost.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo) + strNuyen;
                            await lblCost.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblCost.DoThreadSafeAsync(x => x.Text = strCost + strNuyen, token: token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        decItemCost *= _decCostMultiplier;
                        decItemCost *= 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;

                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            decItemCost *= 0.9m;
                        }

                        string strText = decItemCost.ToString(await _objCharacter.Settings.GetNuyenFormatAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo) + strNuyen;
                        await lblCost.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
                    }
                }
                else
                    await lblCost.DoThreadSafeAsync(x => x.Text = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + strNuyen, token: token).ConfigureAwait(false);
            }

            bool blnShowCost = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
            await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);

            // Test required to find the item.
            string strTest = await _objCharacter.AvailTestAsync(decItemCost, strAvail, token).ConfigureAwait(false);
            await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
            await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token).ConfigureAwait(false);
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
        }

        private int _intSkipListRefresh;

        private Task<bool> AnyItemInList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, false, token);
        }

        private Task<bool> RefreshList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, true, token);
        }

        private async Task<bool> RefreshList(string strCategory, bool blnDoUIUpdate, CancellationToken token = default)
        {
            if ((_intLoading > 0 || _intSkipListRefresh > 0) && blnDoUIUpdate)
                return false;
            string strCurrentGradeId = await cboGrade
                                             .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                             .ConfigureAwait(false);
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId)
                ? null
                : _lstGrades.Find(x => x.SourceIDString == strCurrentGradeId);
            string strFilter = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(')
                         .Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false))
                         .Append(')');
                if (objCurrentGrade != null)
                {
                    string strGradeNameCleaned = objCurrentGrade.Name.CleanXPath();
                    sbdFilter.Append(" and (not(forcegrade) or forcegrade = \"None\" or forcegrade = ")
                             .Append(strGradeNameCleaned).Append(") and (not(bannedgrades[grade = ")
                             .Append(strGradeNameCleaned).Append("]))");
                }

                string strSearch
                    = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            int intOverLimit = 0;
            List<ListItem> lstDrugs = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit
                                                   .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                   .ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems
                                                    .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                    .ConfigureAwait(false);
                bool blnFree = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                decimal decBaseCostMultiplier
                    = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false)
                    / 100.0m;
                decimal decNuyen = blnFree || !blnShowOnlyAffordItems ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                foreach (XPathNavigator xmlDrug in _xmlBaseDrugDataNode.Select(_strNodeXPath + strFilter))
                {
                    bool blnIsForceGrade = xmlDrug.SelectSingleNodeAndCacheExpression("forcegrade", token) == null;
                    if (objCurrentGrade != null && blnIsForceGrade && objCurrentGrade.Name.ContainsAny(
                            (await ImprovementManager
                                   .GetCachedImprovementListForValueOfAsync(
                                       _objCharacter,
                                       Improvement.ImprovementType
                                                  .DisableDrugGrade, token: token)
                                   .ConfigureAwait(false))
                            .Select(x => x.ImprovedName)))
                        continue;

                    string strMaxRating = xmlDrug.SelectSingleNodeAndCacheExpression("rating", token)?.Value;
                    string strMinRating = xmlDrug.SelectSingleNodeAndCacheExpression("minrating", token)?.Value;
                    int intMinRating = 1;
                    // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                    if (strMaxRating.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decMaxRating)
                        || strMinRating.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decMinRating))
                    {
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                       .EvaluateInvariantXPathAsync(strMinRating, token)
                                                                       .ConfigureAwait(false);
                        intMinRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                        (blnIsSuccess, objProcess) = await CommonFunctions
                                                           .EvaluateInvariantXPathAsync(strMaxRating, token)
                                                           .ConfigureAwait(false);
                        int intMaxRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                        if (intMaxRating < intMinRating)
                            continue;
                    }
                    else if (decMaxRating.StandardRound() < decMinRating.StandardRound())
                    {
                        continue;
                    }

                    if (ParentVehicle == null && !await xmlDrug.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                        continue;

                    if (!blnDoUIUpdate)
                    {
                        return true;
                    }

                    if (blnHideOverAvailLimit
                        && !await xmlDrug.CheckAvailRestrictionAsync(_objCharacter, intMinRating,
                                                                     blnIsForceGrade ? 0 : _intAvailModifier, token)
                                         .ConfigureAwait(false))
                    {
                        ++intOverLimit;
                        continue;
                    }

                    if (blnShowOnlyAffordItems && !blnFree)
                    {
                        decimal decCostMultiplier = decBaseCostMultiplier;
                        if (_setBlackMarketMaps.Contains(xmlDrug.SelectSingleNodeAndCacheExpression("category", token)?.Value))
                            decCostMultiplier *= 0.9m;
                        if (!await xmlDrug
                                   .CheckNuyenRestrictionAsync(_objCharacter, decNuyen, decCostMultiplier, token: token)
                                   .ConfigureAwait(false))
                        {
                            ++intOverLimit;
                            continue;
                        }
                    }

                    lstDrugs.Add(new ListItem(xmlDrug.SelectSingleNodeAndCacheExpression("id", token)?.Value,
                                              xmlDrug.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                              ?? xmlDrug.SelectSingleNodeAndCacheExpression("name", token)?.Value));
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
                                                                        "String_RestrictedItemsHidden", token: token)
                                                                    .ConfigureAwait(false),
                                                                intOverLimit)));
                    }

                    string strOldSelected = await lstDrug
                                                  .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                                  .ConfigureAwait(false);
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await lstDrug.PopulateWithListItemsAsync(lstDrugs, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }

                    await lstDrug.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        else
                            x.SelectedIndex = -1;
                    }, token: token).ConfigureAwait(false);
                }

                return lstDrugs?.Count > 0;
            }
            finally
            {
                if (lstDrugs != null)
                    Utils.ListItemListPool.Return(ref lstDrugs);
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
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strSelectedId = await lstDrug.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if (await cboGrade.DoThreadSafeFuncAsync(x => x.Text.StartsWith('*'), token: token).ConfigureAwait(false))
            {
                await Program.ShowScrollableMessageBoxAsync(this,
                    await LanguageManager.GetStringAsync("Message_BannedGrade", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_BannedGrade", token: token).ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                return;
            }
            XPathNavigator objDrugNode = _xmlBaseDrugDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
            if (objDrugNode == null)
                return;

            if (!await objDrugNode.RequirementsMetAsync(_objCharacter, strLocalName: await LanguageManager.GetStringAsync("String_SelectPACKSKit_Drug", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                return;

            string strForceGrade = objDrugNode.SelectSingleNodeAndCacheExpression("forcegrade", token)?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.Find(x => x.SourceIDString == strForceGrade);
                else
                    return;
            }

            _sStrSelectGrade = SelectedGrade?.SourceIDString;
            SelectedDrug = strSelectedId;
            SelectedRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            BlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            Markup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        private bool _blnPopulatingGrades;

        /// <summary>
        /// Populate the list of Drug Grades.
        /// </summary>
        /// <param name="setDisallowedGrades">Set of all grades that should not be shown.</param>
        /// <param name="blnForce">Force grades to be repopulated.</param>
        /// <param name="strForceGrade">If not empty, force this grade to be selected.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task PopulateGrades(ICollection<string> setDisallowedGrades = null, bool blnForce = false, string strForceGrade = "", CancellationToken token = default)
        {
            if (_blnPopulatingGrades)
                return;
            _blnPopulatingGrades = true;
            if (setDisallowedGrades == null)
                setDisallowedGrades = Array.Empty<string>();
            if (blnForce || !_setDisallowedGrades.SetEquals(setDisallowedGrades) || _strForceGrade != strForceGrade || await cboGrade.DoThreadSafeFuncAsync(x => x.Items.Count, token: token).ConfigureAwait(false) == 0)
            {
                _setDisallowedGrades.Clear();
                _setDisallowedGrades.AddRange(setDisallowedGrades);
                _strForceGrade = strForceGrade;
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGrade))
                {
                    foreach (Grade objWareGrade in _lstGrades)
                    {
                        if (objWareGrade.SourceIDString == _strNoneGradeId
                            && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != _strNoneGradeId))
                            continue;
                        //if (ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.DisableDrugGrade).Any(x => objWareGrade.Name.Contains(x.ImprovedName)))
                        //    continue;
                        if (_setDisallowedGrades.Contains(objWareGrade.Name))
                            continue;
                        /*
                        if (blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.BannedDrugGrades.Any(s => objWareGrade.Name.Contains(s)))
                            continue;
                        if (!blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.BannedDrugGrades.Any(s => objWareGrade.Name.Contains(s)))
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceID.ToString("D", GlobalSettings.InvariantCultureInfo), '*' + await objWareGrade.GetCurrentDisplayNameAsync(token)));
                        }
                        else
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceID.ToString("D", GlobalSettings.InvariantCultureInfo), await objWareGrade.GetCurrentDisplayNameAsync(token)));
                        }*/
                    }

                    string strOldSelected = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    bool blnDoSkipRefresh = strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId
                                                                             || lstGrade.Exists(
                                                                                 x => x.Value.ToString()
                                                                                     == strOldSelected);
                    if (blnDoSkipRefresh)
                        Interlocked.Increment(ref _intSkipListRefresh);
                    try
                    {
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await cboGrade.PopulateWithListItemsAsync(lstGrade, token: token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }

                        await cboGrade.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strForceGrade))
                                x.SelectedValue = strForceGrade;
                            else if (x.SelectedIndex <= 0 && !string.IsNullOrEmpty(strOldSelected))
                                x.SelectedValue = strOldSelected;
                            if (x.SelectedIndex == -1 && lstGrade.Count > 0)
                                x.SelectedIndex = 0;
                        }, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intSkipListRefresh);
                    }
                }
            }
            _blnPopulatingGrades = false;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
