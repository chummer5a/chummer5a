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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using NLog;

namespace Chummer
{
    public partial class SelectCyberware : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly Character _objCharacter;
        private readonly List<Grade> _lstGrades;
        private readonly string _strNoneGradeId;

        private decimal _decCostMultiplier = 1.0m;
        private decimal _decESSMultiplier = 1.0m;
        private int _intAvailModifier;
        private decimal _decMarkup;
        private bool _blnFreeCost;

        private Grade _objForcedGrade;
        private string _strSubsystems = string.Empty;
        private string _strDisallowedMounts = string.Empty;
        private string _strHasModularMounts = string.Empty;
        private decimal _decMaximumCapacity = -1;
        private bool _blnLockGrade;
        private int _intLoading = 1;

        private readonly Mode _eMode = Mode.Cyberware;
        private readonly string _strNodeXPath = "cyberwares/cyberware";
        private static string s_strSelectCategory = string.Empty;
        private static string _sStrSelectGrade = string.Empty;
        private string _strSelectedCategory = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private HashSet<string> _setDisallowedGrades;
        private string _strForceGrade = string.Empty;
        private readonly object _objParentObject;
        private readonly XPathNavigator _objParentNode;
        private HashSet<string> _setBlackMarketMaps;
        private readonly XPathNavigator _xmlBaseCyberwareDataNode;
        private CancellationTokenSource _objUpdateCyberwareInfoCancellationTokenSource;
        private CancellationTokenSource _objProcessGradeChangedCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshSelectedCyberwareCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource;
        private readonly CancellationToken _objGenericToken;

        private enum Mode
        {
            Cyberware = 0,
            Bioware
        }

        #region Control Events

        public SelectCyberware(Character objCharacter, Improvement.ImprovementSource objWareSource, object objParentNode = null)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();

            _objParentObject = objParentNode;
            CyberwareParent = objParentNode as Cyberware;
            _objParentNode = (_objParentObject as IHasXmlDataNode)?.GetNodeXPath();

            switch (objWareSource)
            {
                case Improvement.ImprovementSource.Cyberware:
                    _eMode = Mode.Cyberware;
                    _xmlBaseCyberwareDataNode = objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNodeAndCacheExpression("/chummer");
                    _strNodeXPath = "cyberwares/cyberware";
                    Tag = "Title_SelectCyberware";
                    break;

                case Improvement.ImprovementSource.Bioware:
                    _eMode = Mode.Bioware;
                    _xmlBaseCyberwareDataNode = objCharacter.LoadDataXPath("bioware.xml").SelectSingleNodeAndCacheExpression("/chummer");
                    _strNodeXPath = "biowares/bioware";
                    Tag = "Title_SelectCyberware_Bioware";
                    break;
            }

            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            _lstGrades = _objCharacter.GetGradesList(objWareSource);
            _strNoneGradeId = _lstGrades.Find(x => x.Name == "None")?.SourceIDString;

            _strCachedParentCost = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(async () =>
            {
                if (CyberwareParent != null)
                {
                    return await CyberwareParent.ProcessCostExpressionAsync(CyberwareParent.Cost,
                        () => CyberwareParent.GetRatingAsync(),
                        () => CyberwareParent.GetGradeAsync()).ConfigureAwait(false);
                }
                else if (ParentVehicleMod != null)
                {
                    return (await ParentVehicleMod.GetOwnCostAsync().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                }
                return "0";
            }, Utils.JoinableTaskFactory);
            _strCachedParentGearCost = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(async () =>
            {
                if (CyberwareParent != null)
                {
                    return (await (await CyberwareParent.GetGearChildrenAsync().ConfigureAwait(false))
                        .SumAsync(x => x.GetCalculatedCostAsync()).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                }
                return "0";
            }, Utils.JoinableTaskFactory);

            _objGenericCancellationTokenSource = new CancellationTokenSource();
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            _setBlackMarketMaps = Utils.StringHashSetPool.Get();
            _setDisallowedGrades = Utils.StringHashSetPool.Get();
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseCyberwareDataNode));

            // Prevent Enter key from closing the form when NumericUpDown controls have focus
            nudMinimumEssence.KeyDown += NumericUpDown_KeyDown;
            nudMaximumEssence.KeyDown += NumericUpDown_KeyDown;
            nudExactEssence.KeyDown += NumericUpDown_KeyDown;
            nudMinimumCost.KeyDown += NumericUpDown_KeyDown;
            nudMaximumCost.KeyDown += NumericUpDown_KeyDown;
            nudExactCost.KeyDown += NumericUpDown_KeyDown;
        }

        private async void SelectCyberware_Load(object sender, EventArgs e)
        {
            try
            {
                if (await _objCharacter.GetCreatedAsync(_objGenericToken).ConfigureAwait(false))
                {
                    await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = true, token: _objGenericToken)
                                        .ConfigureAwait(false);
                    await nudMarkup.DoThreadSafeAsync(x => x.Visible = true, token: _objGenericToken)
                                   .ConfigureAwait(false);
                    await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = true, token: _objGenericToken)
                                               .ConfigureAwait(false);
                    await chkHideBannedGrades.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                             .ConfigureAwait(false);
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, token: _objGenericToken).ConfigureAwait(false);
                    await chkPrototypeTranshuman.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                        .ConfigureAwait(false);
                    await nudMarkup.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                   .ConfigureAwait(false);
                    await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                               .ConfigureAwait(false);
                    bool blnIgnoreRules = await _objCharacter.GetIgnoreRulesAsync(_objGenericToken).ConfigureAwait(false);
                    await chkHideBannedGrades
                          .DoThreadSafeAsync(x => x.Visible = !blnIgnoreRules, token: _objGenericToken)
                          .ConfigureAwait(false);
                    int intMaxAvail = await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).GetMaximumAvailabilityAsync(_objGenericToken).ConfigureAwait(false);
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Text = string.Format(GlobalSettings.CultureInfo, x.Text, intMaxAvail);
                        x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                    }, token: _objGenericToken).ConfigureAwait(false);
                    if (WindowMode == Mode.Bioware)
                    {
                        _blnPrototypeTranshumanAllowed = await _objCharacter.GetIsPrototypeTranshumanAsync(_objGenericToken).ConfigureAwait(false);
                        await chkPrototypeTranshuman.DoThreadSafeAsync(x => x.Visible = _blnPrototypeTranshumanAllowed, token: _objGenericToken).ConfigureAwait(false);
                    }
                    else
                        await chkPrototypeTranshuman.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(DefaultSearchText))
                {
                    await txtSearch.DoThreadSafeAsync(x =>
                    {
                        x.Text = DefaultSearchText;
                        x.Enabled = false;
                    }, token: _objGenericToken).ConfigureAwait(false);
                }

                await PopulateCategories(_objGenericToken).ConfigureAwait(false);

                await cboCategory.DoThreadSafeAsync(x =>
                {
                    // Select the first Category in the list.
                    if (!string.IsNullOrEmpty(s_strSelectCategory))
                        x.SelectedValue = s_strSelectCategory;
                    if (x.SelectedIndex == -1 && x.Items.Count > 0)
                        x.SelectedIndex = 0;
                    _strSelectedCategory = x.SelectedValue?.ToString();
                }, token: _objGenericToken).ConfigureAwait(false);

                bool blnBlackMarketDiscount = await _objCharacter.GetBlackMarketDiscountAsync(_objGenericToken).ConfigureAwait(false);
                await chkBlackMarketDiscount
                      .DoThreadSafeAsync(x => x.Visible = blnBlackMarketDiscount,
                                         token: _objGenericToken).ConfigureAwait(false);

                // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
                await PopulateGrades(null, true, _objForcedGrade?.SourceIDString ?? string.Empty,
                                     await chkHideBannedGrades
                                           .DoThreadSafeFuncAsync(x => x.Checked, token: _objGenericToken)
                                           .ConfigureAwait(false), _objGenericToken).ConfigureAwait(false);

                await cboGrade.DoThreadSafeAsync(x =>
                {
                    if (_objForcedGrade != null)
                        x.SelectedValue = _objForcedGrade.SourceIDString;
                    else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                        x.SelectedValue = _sStrSelectGrade;
                    if (x.SelectedIndex == -1 && x.Items.Count > 0)
                        x.SelectedIndex = 0;
                }, token: _objGenericToken).ConfigureAwait(false);

                // Retrieve the information for the selected Grade.
                string strSelectedGrade = await cboGrade
                                                .DoThreadSafeFuncAsync(
                                                    x => x.SelectedValue?.ToString(), token: _objGenericToken)
                                                .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedGrade))
                {
                    XPathNavigator xmlGrade
                        = _xmlBaseCyberwareDataNode.TryGetNodeByNameOrId("grades/grade", strSelectedGrade);

                    // Update the Essence and Cost multipliers based on the Grade that has been selected.
                    if (xmlGrade != null)
                    {
                        decimal.TryParse(xmlGrade.SelectSingleNodeAndCacheExpression("cost", token: _objGenericToken)?.Value,
                            NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _decCostMultiplier);
                        decimal.TryParse(xmlGrade.SelectSingleNodeAndCacheExpression("ess", token: _objGenericToken)?.Value,
                            NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _decESSMultiplier);
                        _intAvailModifier
                            = xmlGrade.SelectSingleNodeAndCacheExpression("avail", token: _objGenericToken)?.ValueAsInt ?? 0;
                    }
                }

                if (await (await _objCharacter.GetSettingsAsync(_objGenericToken).ConfigureAwait(false)).GetAllowCyberwareESSDiscountsAsync(_objGenericToken).ConfigureAwait(false))
                {
                    await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = true, token: _objGenericToken).ConfigureAwait(false);
                    await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = true, token: _objGenericToken).ConfigureAwait(false);
                    await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = true, token: _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken).ConfigureAwait(false);
                    await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken).ConfigureAwait(false);
                    await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken).ConfigureAwait(false);
                }
                Interlocked.Decrement(ref _intLoading);

                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SelectCyberware_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessGradeChangedCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCyberwareInfoCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshSelectedCyberwareCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await ProcessGradeChanged(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task ProcessGradeChanged(CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objProcessGradeChangedCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                if (Interlocked.CompareExchange(ref _intLoading, 1, 0) > 0)
                    return;
                token = objJoinedCancellationTokenSource.Token;
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
                        xmlGrade =
                            _xmlBaseCyberwareDataNode.TryGetNodeByNameOrId("grades/grade", strSelectedGrade);
                    }

                    // Update the Essence and Cost multipliers based on the Grade that has been selected.
                    if (xmlGrade != null)
                    {
                        decimal.TryParse(xmlGrade.SelectSingleNodeAndCacheExpression("cost", token: _objGenericToken)?.Value,
                            NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _decCostMultiplier);
                        decimal.TryParse(xmlGrade.SelectSingleNodeAndCacheExpression("ess", token: _objGenericToken)?.Value,
                            NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _decESSMultiplier);
                        _intAvailModifier
                            = xmlGrade.SelectSingleNodeAndCacheExpression("avail", token)
                            ?.ValueAsInt ?? 0;

                        await PopulateCategories(token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                    return;
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
                try
                {
                    if (xmlGrade != null)
                    {
                        await RefreshList(_strSelectedCategory, token).ConfigureAwait(false);
                        await DoRefreshSelectedCyberware(token).ConfigureAwait(false);
                    }
                    else
                    {
                        await UpdateCyberwareInfo(token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private async void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            try
            {
                if (await cboGrade.DoThreadSafeFuncAsync(x => x.Enabled, token: _objGenericToken).ConfigureAwait(false)
                    != _blnOldGradeEnabled)
                {
                    await cboGrade.DoThreadSafeAsync(x =>
                    {
                        _blnOldGradeEnabled = x.Enabled;
                        if (_blnOldGradeEnabled)
                        {
                            x.SelectedValue = _strOldSelectedGrade;
                        }
                    }, token: _objGenericToken).ConfigureAwait(false);
                    await ProcessGradeChanged(_objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _intLoading, 1, 0) > 0)
                return;
            try
            {
                _strSelectedCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: _objGenericToken)
                                                        .ConfigureAwait(false);
                string strForceGrade = string.Empty;
                // Update the list of Cyberware based on the selected Category.
                await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade, token: _objGenericToken).ConfigureAwait(false);
                if (_blnLockGrade)
                {
                    strForceGrade = await cboGrade
                                          .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(),
                                                                 token: _objGenericToken)
                                          .ConfigureAwait(false);
                }

                // We will need to rebuild the Grade list since certain categories of 'ware disallow certain grades (e.g. Used for cultured bioware) and ForceGrades can change.
                await PopulateGrades(null, false, strForceGrade,
                                     await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked, token: _objGenericToken)
                                                              .ConfigureAwait(false), _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
                return;
            }
            finally
            {
                Interlocked.Decrement(ref _intLoading);
            }
            try
            {
                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await DoRefreshSelectedCyberware(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoRefreshSelectedCyberware(CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshSelectedCyberwareCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                if (Interlocked.CompareExchange(ref _intLoading, 1, 0) > 0)
                    return;
                token = objJoinedCancellationTokenSource.Token;
                try
                {
                    XPathNavigator xmlCyberware = null;
                    string strSelectedId = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        // Retrieve the information for the selected piece of Cyberware.
                        xmlCyberware = _xmlBaseCyberwareDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
                    }
                    string strForceGrade;
                    if (xmlCyberware != null)
                    {
                        strForceGrade = xmlCyberware.SelectSingleNodeAndCacheExpression("forcegrade", token)?.Value;
                        if (!string.IsNullOrEmpty(strForceGrade))
                        {
                            // Force the Cyberware to be a particular Grade.
                            await cboGrade.DoThreadSafeAsync(x =>
                            {
                                if (x.Enabled)
                                    x.Enabled = false;
                            }, token: token).ConfigureAwait(false);
                            Grade objForcedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
                            strForceGrade = objForcedGrade?.SourceIDString;
                        }
                        else
                        {
                            await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade, token: token)
                                          .ConfigureAwait(false);
                            if (_blnLockGrade)
                            {
                                strForceGrade = _objForcedGrade?.SourceIDString ?? await cboGrade
                                    .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                    .ConfigureAwait(false);
                            }
                        }

                        // We will need to rebuild the Grade list since certain categories of 'ware disallow certain grades (e.g. Used for cultured bioware) and ForceGrades can change.
                        HashSet<string> setDisallowedGrades = null;
                        if (xmlCyberware.SelectSingleNodeAndCacheExpression("bannedgrades", token) != null)
                        {
                            setDisallowedGrades = new HashSet<string>();
                            foreach (XPathNavigator objNode in xmlCyberware.SelectAndCacheExpression("bannedgrades/grade", token))
                            {
                                setDisallowedGrades.Add(objNode.Value);
                            }
                        }
                        await PopulateGrades(setDisallowedGrades, false, strForceGrade,
                                             await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false), token).ConfigureAwait(false);

                        // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
                        XPathNavigator xmlRatingNode = xmlCyberware.SelectSingleNodeAndCacheExpression("rating", token);
                        if (xmlRatingNode != null)
                        {
                            string strMinRating = xmlCyberware.SelectSingleNodeAndCacheExpression("minrating", token)?.Value ?? string.Empty;
                            string strMaxRating = xmlRatingNode.Value;
                            // Not a simple integer, so we need to start mucking around with strings
                            (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(xmlCyberware, strMinRating, 1, 0, token).ConfigureAwait(false);
                            int intMinRating = blnIsSuccess ? decValue.StandardRound() : 1;
                            await nudRating.DoThreadSafeAsync(x => x.Minimum = intMinRating, token: token).ConfigureAwait(false);
                            (decValue, blnIsSuccess) = await ProcessInvariantXPathExpression(xmlCyberware, strMaxRating, intMinRating, intMinRating, token).ConfigureAwait(false);
                            int intMaxRating = blnIsSuccess ? decValue.StandardRound() : 1;
                            if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            {
                                while (intMaxRating > intMinRating
                                       && !await xmlCyberware.CheckAvailRestrictionAsync(
                                           _objCharacter, intMaxRating, _intAvailModifier + (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: xmlCyberware.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token: token).ConfigureAwait(false))
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
                            }, token: token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                            await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await nudRating.DoThreadSafeAsync(x =>
                            {
                                x.Minimum = 0;
                                x.Value = 0;
                                x.Visible = false;
                            }, token: token).ConfigureAwait(false);
                        }

                        string strRatingLabel = xmlCyberware.SelectSingleNodeAndCacheExpression("ratinglabel", token)?.Value;
                        strRatingLabel = !string.IsNullOrEmpty(strRatingLabel)
                            ? string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("Label_RatingFormat", token: token).ConfigureAwait(false),
                                            await LanguageManager.GetStringAsync(strRatingLabel, token: token).ConfigureAwait(false))
                            : await LanguageManager.GetStringAsync("Label_Rating", token: token).ConfigureAwait(false);
                        await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token).ConfigureAwait(false);

                        string strSource = xmlCyberware.SelectSingleNodeAndCacheExpression("source", token)?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        string strPage = xmlCyberware.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? xmlCyberware.SelectSingleNodeAndCacheExpression("page", token)?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                        SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                            GlobalSettings.CultureInfo, _objCharacter, token: token).ConfigureAwait(false);
                        await objSource.SetControlAsync(lblSource, this, token: token).ConfigureAwait(false);
                        bool blnShowSource = !string.IsNullOrEmpty(await lblSource.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnShowSource, token: token).ConfigureAwait(false);

                        bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlCyberware.SelectSingleNodeAndCacheExpression("category", token)?.Value);
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
                        }, token: token).ConfigureAwait(false);

                        string strNotes = xmlCyberware.SelectSingleNodeAndCacheExpression("altnotes", token)?.Value ?? xmlCyberware.SelectSingleNodeAndCacheExpression("notes", token)?.Value;
                        if (!string.IsNullOrEmpty(strNotes))
                        {
                            await lblCyberwareNotesLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await lblCyberwareNotes.DoThreadSafeAsync(x =>
                            {
                                x.Visible = true;
                                x.Text = strNotes;
                            }, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblCyberwareNotes.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                            await lblCyberwareNotesLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                        }
                        await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                        await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade, token: token).ConfigureAwait(false);
                        strForceGrade = _blnLockGrade
                            ? _objForcedGrade?.SourceIDString
                              ?? await cboGrade.DoThreadSafeFuncAsync(
                                  x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false)
                            : string.Empty;
                        await PopulateGrades(null, false, strForceGrade, await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                        await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Checked = false, token: token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
                await UpdateCyberwareInfo(token).ConfigureAwait(false);
            }
        }

        private async void ProcessCyberwareInfoChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                await UpdateCyberwareInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void EssenceCostFilter(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;

            try
            {
                _intLoading = 1;
                await nudMinimumEssence.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudMaximumEssence.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudExactEssence.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudMinimumCost.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudMaximumCost.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);
                await nudExactCost.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                    {
                        x.Value = 0;
                    }
                }, _objGenericToken).ConfigureAwait(false);

                decimal decMaximumEssence = await nudMaximumEssence.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decMinimumEssence = await nudMinimumEssence.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decExactEssence = await nudExactEssence.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                
                // If exact essence is specified, clear range values
                if (decExactEssence > 0)
                {
                    await nudMinimumEssence.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    await nudMaximumEssence.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                }
                // If range values are specified, clear exact essence
                else if (decMinimumEssence > 0 || decMaximumEssence > 0)
                {
                    await nudExactEssence.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    
                    // Ensure maximum is not less than minimum
                    if (decMaximumEssence < decMinimumEssence)
                    {
                        if (sender == nudMaximumEssence)
                            await nudMinimumEssence.DoThreadSafeAsync(x => x.Value = decMaximumEssence, _objGenericToken).ConfigureAwait(false);
                        else
                            await nudMaximumEssence.DoThreadSafeAsync(x => x.Value = decMinimumEssence, _objGenericToken).ConfigureAwait(false);
                    }
                }

                decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, _objGenericToken).ConfigureAwait(false);
                
                // If exact cost is specified, clear range values
                if (decExactCost > 0)
                {
                    await nudMinimumCost.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    await nudMaximumCost.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                }
                // If range values are specified, clear exact cost
                else if (decMinimumCost > 0 || decMaximumCost > 0)
                {
                    await nudExactCost.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken).ConfigureAwait(false);
                    
                    // Ensure maximum is not less than minimum (only if maximum is actually set)
                    if (decMaximumCost > 0 && decMaximumCost < decMinimumCost)
                    {
                        if (sender == nudMaximumCost)
                            await nudMinimumCost.DoThreadSafeAsync(x => x.Value = decMaximumCost, _objGenericToken).ConfigureAwait(false);
                        else
                            await nudMaximumCost.DoThreadSafeAsync(x => x.Value = decMinimumCost, _objGenericToken).ConfigureAwait(false);
                    }
                }

                _intLoading = 0;

                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Form is being closed or operation was cancelled, ignore
                _intLoading = 0;
            }
        }


        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {

                await UpdateCyberwareInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            try
            {
                await AcceptForm(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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
            try
            {
                bool blnHideBannedGrades = await chkHideBannedGrades
                                                 .DoThreadSafeFuncAsync(x => x.Checked, token: _objGenericToken)
                                                 .ConfigureAwait(false);
                _lstGrades.Clear();
                _lstGrades.AddRange(await _objCharacter.GetGradesListAsync(
                                        WindowMode == Mode.Bioware
                                            ? Improvement.ImprovementSource.Bioware
                                            : Improvement.ImprovementSource.Cyberware,
                                        blnHideBannedGrades, _objGenericToken).ConfigureAwait(false));
                await PopulateGrades(blnHideBannedGrades: blnHideBannedGrades, token: _objGenericToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            try
            {
                await AcceptForm(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {

                await UpdateCyberwareInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstCyberware.SelectedIndex + 1 < lstCyberware.Items.Count:
                    lstCyberware.SelectedIndex++;
                    break;

                case Keys.Down:
                    {
                        if (lstCyberware.Items.Count > 0)
                        {
                            lstCyberware.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstCyberware.SelectedIndex >= 1:
                    lstCyberware.SelectedIndex--;
                    break;

                case Keys.Up:
                    {
                        if (lstCyberware.Items.Count > 0)
                        {
                            lstCyberware.SelectedIndex = lstCyberware.Items.Count - 1;
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

        private async void chkUseCurrentEssence_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                if (await chkUseCurrentEssence.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    decimal decCurrentEssence = await _objCharacter.EssenceAsync(token: _objGenericToken).ConfigureAwait(false);
                    await nudMaximumEssence.DoThreadSafeAsync(x => 
                    {
                        x.Value = decCurrentEssence;
                        x.Enabled = false;
                    }, _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await nudMaximumEssence.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken).ConfigureAwait(false);
                }
                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void chkUseCurrentNuyen_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                if (await chkUseCurrentNuyen.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    decimal decCurrentNuyen = await _objCharacter.GetAvailableNuyenAsync(token: _objGenericToken).ConfigureAwait(false);
                    await nudMaximumCost.DoThreadSafeAsync(x => 
                    {
                        x.Value = decCurrentNuyen;
                        x.Enabled = false;
                    }, _objGenericToken).ConfigureAwait(false);
                }
                else
                {
                    await nudMaximumCost.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken).ConfigureAwait(false);
                }
                await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }
        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Essence cost multiplier from the character.
        /// </summary>
        public decimal CharacterESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Total Essence cost multiplier from the character (stacks multiplicatively at the very last step.
        /// </summary>
        public decimal CharacterTotalESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Cost multiplier for Genetech.
        /// </summary>
        public decimal GenetechCostMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Essence cost multiplier for Genetech.
        /// </summary>
        public decimal GenetechEssMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Essence cost multiplier for Basic Bioware.
        /// </summary>
        public decimal BasicBiowareESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Whether the item has no cost.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        /// <summary>
        /// Set the window's Mode to Cyberware or Bioware.
        /// </summary>
        private Mode WindowMode => _eMode;

        /// <summary>
        /// Set the maximum Capacity the piece of Cyberware is allowed to be.
        /// </summary>
        public decimal MaximumCapacity
        {
            get => _decMaximumCapacity;
            set
            {
                _decMaximumCapacity = value;
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed")
                                          + LanguageManager.GetString("String_Space")
                                          + value.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// Set the maximum Capacity the piece of Gear is allowed to be.
        /// </summary>
        public async Task SetMaximumCapacityAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _decMaximumCapacity = value;
            string strText = await LanguageManager.GetStringAsync("Label_MaximumCapacityAllowed", token: token).ConfigureAwait(false)
                                + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false)
                                + value.ToString("#,0.##", GlobalSettings.CultureInfo);
            await lblMaximumCapacity.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Comma-separate list of Categories to show for Subsystems.
        /// </summary>
        public string Subsystems
        {
            set => _strSubsystems = value;
        }

        /// <summary>
        /// Comma-separate list of mount locations that are disallowed.
        /// </summary>
        public string DisallowedMounts
        {
            set => _strDisallowedMounts = value;
        }

        /// <summary>
        /// Comma-separate list of mount locations that already exist on the parent.
        /// </summary>
        public string HasModularMounts
        {
            set => _strHasModularMounts = value;
        }

        /// <summary>
        /// Manually set the Grade of the piece of Cyberware.
        /// </summary>
        public Grade ForcedGrade
        {
            get => _objForcedGrade;
            set => _objForcedGrade = value;
        }

        /// <summary>
        /// Name of Cyberware that was selected in the dialogue.
        /// </summary>
        public string SelectedCyberware { get; private set; } = string.Empty;

        /// <summary>
        /// Grade of the selected piece of Cyberware.
        /// </summary>
        public Grade SelectedGrade { get; private set; }

        /// <summary>
        /// Rating of the selected piece of Cyberware (0 if not applicable).
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

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public VehicleMod ParentVehicleMod { get; set; }

        public decimal Markup => _decMarkup;

        private bool _blnPrototypeTranshumanAllowed;

        /// <summary>
        /// Whether the bioware should be discounted by Prototype Transhuman.
        /// </summary>
        public bool PrototypeTranshuman => _blnPrototypeTranshumanAllowed && chkPrototypeTranshuman.Checked;

        /// <summary>
        /// Parent cyberware that the current selection will be added to.
        /// </summary>
        public Cyberware CyberwareParent { get; }

        /// <summary>
        /// Default text string to filter by.
        /// </summary>
        public string DefaultSearchText { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the Cyberware's information based on the Cyberware selected and current Rating.
        /// </summary>
        private async Task UpdateCyberwareInfo(CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCyberwareInfoCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                XPathNavigator objXmlCyberware = null;
                string strSelectedId = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    // Retrieve the information for the selected piece of Cyberware.
                    objXmlCyberware = _xmlBaseCyberwareDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
                }
                if (objXmlCyberware == null)
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    return;
                }

                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                try
                {
                    await tlpRight.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.SuspendLayout();
                    }, token: token).ConfigureAwait(false);
                    try
                    {
                        string strSelectCategory = objXmlCyberware.SelectSingleNodeAndCacheExpression("category", token)?.Value ?? string.Empty;
                        bool blnForceNoESSModifier = objXmlCyberware.SelectSingleNodeAndCacheExpression("forcegrade", token)?.Value == "None";
                        bool blnIsGeneware = objXmlCyberware.SelectSingleNodeAndCacheExpression("isgeneware", token) != null && objXmlCyberware.SelectSingleNodeAndCacheExpression("isgeneware", token)?.Value != bool.FalseString;

                        // Place the Genetech cost multiplier in a variable that can be safely modified.
                        decimal decGenetechCostModifier = 1;
                        // Genetech cost modifier only applies to Genetech.
                        if (blnIsGeneware)
                            decGenetechCostModifier = GenetechCostMultiplier;

                        // Extract the Avail and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
                        // This is done using XPathExpression.

                        int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
                        int intMinRating = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token).ConfigureAwait(false);
                        string strAvailExpression = objXmlCyberware
                            .SelectSingleNodeAndCacheExpression("avail", token)?.Value ?? string.Empty;
                        bool blnOrGear = strAvailExpression.EndsWith(" or Gear", StringComparison.Ordinal);
                        if (blnOrGear)
                            strAvailExpression = strAvailExpression.TrimEndOnce(" or Gear", true);

                        AvailabilityValue objTotalAvail = new AvailabilityValue(
                            intRating,
                            await strAvailExpression.CheapReplaceAsync("MinRating",
                                                                       () => nudRating.DoThreadSafeFuncAsync(
                                                                           y => y.Minimum.ToString(
                                                                               GlobalSettings.InvariantCultureInfo), token: token),
                                                                       token: token).ConfigureAwait(false),
                            _intAvailModifier + (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: objXmlCyberware.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound());
                        string strAvail = await objTotalAvail.ToStringAsync(token).ConfigureAwait(false);
                        await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);

                        // Cost.
                        decimal decItemCost = 0;
                        string strCost;
                        string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                        if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            strCost = 0.0m.ToString(strNuyenFormat,
                                                              GlobalSettings.CultureInfo)
                                              + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            strCost = objXmlCyberware.SelectSingleNodeAndCacheExpression("cost", token)?.Value;
                            if (!string.IsNullOrEmpty(strCost))
                            {
                                strCost = strCost.ProcessFixedValuesString(intRating);
                                // Check for a Variable Cost.
                                if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                                {
                                    string strFirstHalf = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                                    string strSecondHalf = string.Empty;
                                    int intHyphenIndex = strFirstHalf.IndexOf('-');
                                    if (intHyphenIndex != -1)
                                    {
                                        if (intHyphenIndex + 1 < strFirstHalf.Length)
                                            strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                                        strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                                    }
                                    decimal decMin;
                                    decimal decMax = decimal.MaxValue;
                                    if (intHyphenIndex != -1)
                                    {
                                        decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                                        decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
                                    }
                                    else
                                        decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

                                    strCost = decMax == decimal.MaxValue
                                        ? decMin.ToString(strNuyenFormat,
                                                          GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                 .ConfigureAwait(false) + "+"
                                        : decMin.ToString(strNuyenFormat,
                                                          GlobalSettings.CultureInfo)
                                          + " - " + decMax.ToString(strNuyenFormat,
                                                                    GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                 .ConfigureAwait(false);

                                    decItemCost = decMin;
                                }
                                else if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decItemCost))
                                {
                                    bool blnIsSuccess;
                                    (decItemCost, blnIsSuccess)
                                        = await ProcessInvariantXPathExpression(objXmlCyberware, strCost, intMinRating, intRating, token).ConfigureAwait(false);
                                    if (blnIsSuccess)
                                    {
                                        decItemCost *= _decCostMultiplier * decGenetechCostModifier * (1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false) / 100.0m);

                                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                                        {
                                            decItemCost *= 0.9m;
                                        }

                                        strCost = decItemCost.ToString(strNuyenFormat, GlobalSettings.CultureInfo)
                                            + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        strCost += await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    decItemCost *= _decCostMultiplier * decGenetechCostModifier * (1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false) / 100.0m);

                                    if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                                    {
                                        decItemCost *= 0.9m;
                                    }

                                    strCost = decItemCost.ToString(strNuyenFormat, GlobalSettings.CultureInfo)
                                        + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                strCost = 0.0m.ToString(strNuyenFormat,
                                                          GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                 .ConfigureAwait(false);
                            }
                        }
                        await lblCost.DoThreadSafeAsync(x => x.Text = strCost, token: token).ConfigureAwait(false);

                        bool blnShowCost = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                        await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);

                        // Test required to find the item.
                        string strTest = await _objCharacter.AvailTestAsync(decItemCost, objTotalAvail, token).ConfigureAwait(false);
                        await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                        await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token).ConfigureAwait(false);

                        // Essence.
                        bool blnAllowEssDiscounts = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetAllowCyberwareESSDiscountsAsync(token).ConfigureAwait(false);
                        if (blnAllowEssDiscounts)
                        {
                            await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                            await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                            await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                            await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                        }

                        string strEssenceFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetEssenceFormatAsync(token).ConfigureAwait(false);
                        bool blnAddToParentESS = objXmlCyberware.SelectSingleNodeAndCacheExpression("addtoparentess", token) != null;
                        if (_objParentNode == null || blnAddToParentESS)
                        {
                            decimal decESS = 0;
                            if (!await chkPrototypeTranshuman.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            {
                                // Place the Essence cost multiplier in a variable that can be safely modified.
                                decimal decCharacterESSModifier = 1.0m;

                                if (!blnForceNoESSModifier)
                                {
                                    decCharacterESSModifier = CharacterESSMultiplier;
                                    // If Basic Bioware is selected, apply the Basic Bioware ESS Multiplier.
                                    if (strSelectCategory == "Basic" && WindowMode == Mode.Bioware)
                                        decCharacterESSModifier -= 1 - BasicBiowareESSMultiplier;
                                    if (blnIsGeneware)
                                        decCharacterESSModifier -= 1 - GenetechEssMultiplier;

                                    if (blnAllowEssDiscounts)
                                    {
                                        decimal decDiscountModifier = await nudESSDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                                        decCharacterESSModifier *= 1.0m - decDiscountModifier;
                                    }

                                    decCharacterESSModifier -= 1 - _decESSMultiplier;

                                    decCharacterESSModifier *= CharacterTotalESSMultiplier;

                                    string strPostModifierExpression
                                        = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                                        .GetEssenceModifierPostExpressionAsync(token).ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(strPostModifierExpression) && strPostModifierExpression != "{Modifier}")
                                    {
                                        strPostModifierExpression = strPostModifierExpression.Replace("{Modifier}",
                                                        decCharacterESSModifier.ToString(GlobalSettings.InvariantCultureInfo));
                                        (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(objXmlCyberware, strPostModifierExpression, intMinRating, intRating, token).ConfigureAwait(false);
                                        if (blnIsSuccess)
                                            decCharacterESSModifier = decValue;
                                    }
                                }

                                string strEss = objXmlCyberware.SelectSingleNodeAndCacheExpression("ess", token)?.Value ?? string.Empty;
                                strEss = strEss.ProcessFixedValuesString(intRating);
                                decESS = (await ProcessInvariantXPathExpression(objXmlCyberware, strEss, intMinRating, intRating, token).ConfigureAwait(false)).Item1;
                                decESS *= decCharacterESSModifier;
                                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                                if (!await objSettings.GetDontRoundEssenceInternallyAsync(token).ConfigureAwait(false))
                                {
                                    decESS = decimal.Round(decESS, await objSettings.GetEssenceDecimalsAsync(token).ConfigureAwait(false),
                                                            MidpointRounding.AwayFromZero);
                                }
                            }

                            await lblEssence.DoThreadSafeAsync(x =>
                            {
                                if (blnAddToParentESS)
                                    x.Text = "+" + decESS.ToString(strEssenceFormat, GlobalSettings.CultureInfo);
                                else
                                    x.Text = decESS.ToString(strEssenceFormat, GlobalSettings.CultureInfo);
                            }, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblEssence.DoThreadSafeAsync(x => x.Text
                                                                   = 0.0m.ToString(
                                                                       strEssenceFormat,
                                                                       GlobalSettings.CultureInfo), token: token)
                                            .ConfigureAwait(false);
                        }

                        bool blnShowEssence = !string.IsNullOrEmpty(await lblEssence.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                        await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = blnShowEssence, token: token).ConfigureAwait(false);

                        // Capacity.
                        bool blnAddToParentCapacity = objXmlCyberware.SelectSingleNodeAndCacheExpression("addtoparentcapacity", token) != null;
                        // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                        string strCapacity = objXmlCyberware.SelectSingleNodeAndCacheExpression("capacity", token)?.Value ?? string.Empty;
                        bool blnSquareBrackets = strCapacity.StartsWith('[');
                        if (string.IsNullOrEmpty(strCapacity))
                        {
                            await lblCapacity.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            if (blnSquareBrackets)
                                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                            strCapacity = strCapacity.ProcessFixedValuesString(intRating);
                            if (strCapacity == "[*]" || strCapacity == "*")
                                await lblCapacity.DoThreadSafeAsync(x => x.Text = "*", token: token).ConfigureAwait(false);
                            else
                            {
                                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                if (intPos != -1)
                                {
                                    string strFirstHalf = strCapacity.Substring(0, intPos);
                                    string strSecondHalf = strCapacity.Substring(intPos + 1);

                                    blnSquareBrackets = strFirstHalf.StartsWith('[');
                                    if (blnSquareBrackets && strFirstHalf.Length > 1)
                                        strFirstHalf = strFirstHalf.Substring(1, strCapacity.Length - 2);
                                    string strText;
                                    if (strFirstHalf.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                                    {
                                        bool blnIsSuccess;
                                        (decValue, blnIsSuccess) = await ProcessInvariantXPathExpression(objXmlCyberware, strFirstHalf, intMinRating, intRating, token).ConfigureAwait(false);
                                        strText = blnIsSuccess ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo) : strFirstHalf;
                                    }
                                    else
                                    {
                                        strText = decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
                                    }
                                    if (blnSquareBrackets)
                                        strText = "[" + strText + "]";
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);

                                    strSecondHalf = strSecondHalf.Trim('[', ']');
                                    if (strSecondHalf.DoesNeedXPathProcessingToBeConvertedToNumber(out decValue))
                                    {
                                        bool blnIsSuccess;
                                        (decValue, blnIsSuccess) = await ProcessInvariantXPathExpression(objXmlCyberware, strSecondHalf, intMinRating, intRating, token).ConfigureAwait(false);
                                        strSecondHalf = (blnAddToParentCapacity ? "+[" : "[") + (blnIsSuccess ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo) : strSecondHalf) + "]";
                                    }
                                    else
                                        strSecondHalf = (blnAddToParentCapacity ? "+[" : "[")
                                                        + decValue.ToString("#,0.##", GlobalSettings.InvariantCultureInfo) + "]";
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text += "/" + strSecondHalf, token: token).ConfigureAwait(false);
                                }
                                else
                                {
                                    string strText;
                                    if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                                    {
                                        bool blnIsSuccess;
                                        (decValue, blnIsSuccess) = await ProcessInvariantXPathExpression(objXmlCyberware, strCapacity, intMinRating, intRating, token).ConfigureAwait(false);
                                        strText = blnIsSuccess ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo) : strCapacity;
                                    }
                                    else
                                    {
                                        strText = decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
                                    }
                                    if (blnSquareBrackets)
                                    {
                                        strText = blnAddToParentCapacity
                                            ? "+[" + strText + "]"
                                            : "[" + strText + "]";
                                    }
                                    await lblCapacity.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
                                }
                            }
                        }

                        bool blnShowCapacity = !string.IsNullOrEmpty(await lblCapacity.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                        await lblCapacityLabel.DoThreadSafeAsync(x => x.Visible = blnShowCapacity, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await tlpRight.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                }
            }
        }

        private int _intSkipListRefresh;

        private Task<bool> AnyItemInList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, false, token);
        }

        private async Task<bool> RefreshList(string strCategory = "", CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
                return await RefreshList(strCategory, true, objJoinedCancellationTokenSource.Token).ConfigureAwait(false);
        }

        private async Task<bool> RefreshList(string strCategory, bool blnDoUIUpdate,
                                                  CancellationToken token = default)
        {
            if ((_intLoading > 0 || _intSkipListRefresh > 0) && blnDoUIUpdate)
                return false;
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    await lstCyberware.PopulateWithListItemAsync(ListItem.Blank, token: token)
                                      .ConfigureAwait(false);
                }

                return false;
            }

            decimal decMinimumCost = await nudMinimumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
            decimal decMaximumCost = await nudMaximumCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
            decimal decExactCost = await nudExactCost.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);

            string strCurrentGradeId = await cboGrade
                                             .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                             .ConfigureAwait(false);
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId)
                ? null
                : _lstGrades.Find(x => string.Equals(x.SourceIDString, strCurrentGradeId, StringComparison.OrdinalIgnoreCase));

            string strFilter = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(',
                    await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false),
                    ')');
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdCategoryFilter))
                {
                    if (strCategory != "Show All" && !Upgrading
                                                  && (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0))
                    {
                        sbdCategoryFilter.Append("category = ", strCategory.CleanXPath(), " or category = \"None\"");
                    }
                    else
                    {
                        foreach (ListItem objItem in cboCategory.Items)
                        {
                            string strItem = objItem.Value.ToString();
                            if (!string.IsNullOrEmpty(strItem))
                                sbdCategoryFilter.Append("category = ", strItem.CleanXPath(), " or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Append("category = \"None\"");
                        }
                    }

                    if (sbdCategoryFilter.Length > 0)
                        sbdFilter.Append(" and (", sbdCategoryFilter.ToString(), ')');
                }

                if (_objParentNode != null)
                    sbdFilter.Append(" and (requireparent or contains(capacity, \"[\")) and not(mountsto)");
                else if (ParentVehicle == null && ((!await _objCharacter.GetAddCyberwareEnabledAsync(token).ConfigureAwait(false) && WindowMode == Mode.Cyberware)
                                                   || (!await _objCharacter.GetAddBiowareEnabledAsync(token).ConfigureAwait(false) && WindowMode == Mode.Bioware)))
                {
                    sbdFilter.Append(" and (id = ", Cyberware.EssenceHoleGuidString.CleanXPath(), " or id = ", Cyberware.EssenceAntiHoleGuidString.CleanXPath(), " or mountsto)");
                }
                else
                    sbdFilter.Append(" and not(requireparent)");
                if (objCurrentGrade != null)
                {
                    string strGradeNameCleaned = objCurrentGrade.Name.CleanXPath();
                    sbdFilter.Append(" and (not(forcegrade) or forcegrade = \"None\" or forcegrade = ", strGradeNameCleaned, ") and (not(bannedgrades[grade = ", strGradeNameCleaned, "]))");
                }

                string strSearch
                    = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ", CommonFunctions.GenerateSearchXPath(strSearch));

                // Note: Essence filtering is handled in post-processing due to dynamic expressions like Rating * 0.1 and FixedValues()

                if (sbdFilter.Length > 0)
                    strFilter = sbdFilter.Insert(0, '[').Append(']').ToString();
            }

            XPathNodeIterator xmlIterator;
            try
            {
                xmlIterator = _xmlBaseCyberwareDataNode.Select(_strNodeXPath + strFilter);
            }
            catch (XPathException e)
            {
                Log.Warn(e);
                return false;
            }

            bool blnCyberwareDisabled = !await _objCharacter.GetAddCyberwareEnabledAsync(token).ConfigureAwait(false);
            bool blnBiowareDisabled = !await _objCharacter.GetAddBiowareEnabledAsync(token).ConfigureAwait(false);
            int intOverLimit = 0;
            List<ListItem> lstCyberwares = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                if (xmlIterator.Count > 0)
                {
                    bool blnHideOverAvailLimit = await chkHideOverAvailLimit
                                                       .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                       .ConfigureAwait(false);
                    bool blnFree = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                .ConfigureAwait(false);
                    decimal decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                                       .ConfigureAwait(false);
                    decimal decNuyen = blnFree ? decimal.MaxValue : await _objCharacter.GetAvailableNuyenAsync(token: token).ConfigureAwait(false);
                    foreach (XPathNavigator xmlCyberware in xmlIterator)
                    {
                        bool blnIsForceGrade
                            = xmlCyberware.SelectSingleNodeAndCacheExpression("forcegrade", token: token) != null;
                        if (objCurrentGrade != null && blnIsForceGrade)
                        {
                            if (WindowMode == Mode.Bioware)
                            {
                                if (objCurrentGrade.Name.ContainsAny((await ImprovementManager
                                                                         .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                                             Improvement.ImprovementType.DisableBiowareGrade, token: token)
                                                                         .ConfigureAwait(false))
                                                                  .Select(x => x.ImprovedName)))
                                    continue;
                            }
                            else if (objCurrentGrade.Name.ContainsAny((await ImprovementManager
                                                                          .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                                              Improvement.ImprovementType
                                                                                  .DisableCyberwareGrade, token: token).ConfigureAwait(false))
                                                                   .Select(x => x.ImprovedName)))
                                continue;
                        }

                        if (xmlCyberware.SelectSingleNodeAndCacheExpression("mountsto", token: token) == null)
                        {
                            if (blnCyberwareDisabled
                                && xmlCyberware
                                    .SelectSingleNodeAndCacheExpression("subsystems/cyberware", token: token) != null)
                            {
                                continue;
                            }

                            if (blnBiowareDisabled
                                && xmlCyberware
                                    .SelectSingleNodeAndCacheExpression("subsystems/bioware", token: token) != null)
                            {
                                continue;
                            }
                        }

                        XPathNavigator xmlTestNode
                            = xmlCyberware
                                    .SelectSingleNodeAndCacheExpression("forbidden/parentdetails", token: token);
                        if (xmlTestNode != null && await _objParentNode
                                                         .ProcessFilterOperationNodeAsync(
                                                             xmlTestNode, false, token: token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = xmlCyberware
                                            .SelectSingleNodeAndCacheExpression(
                                                "required/parentdetails", token: token);
                        if (xmlTestNode != null && !await _objParentNode
                                                          .ProcessFilterOperationNodeAsync(
                                                              xmlTestNode, false, token: token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        if (!string.IsNullOrEmpty(_strHasModularMounts))
                        {
                            string strBlocksMounts
                                = xmlCyberware
                                    .SelectSingleNodeAndCacheExpression("blocksmounts", token: token)?.Value;
                            if (!string.IsNullOrEmpty(strBlocksMounts))
                            {
                                ICollection<Cyberware> lstWareListToCheck = null;
                                if (CyberwareParent != null)
                                    lstWareListToCheck = await CyberwareParent.GetChildrenAsync(token).ConfigureAwait(false);
                                else if (ParentVehicle == null)
                                    lstWareListToCheck = await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false);
                                if (xmlCyberware
                                          .SelectSingleNodeAndCacheExpression("selectside", token: token) == null
                                    || !string.IsNullOrEmpty(CyberwareParent?.Location) ||
                                    lstWareListToCheck?.Any(x => x.Location == "Left") == true
                                    && lstWareListToCheck.Any(x => x.Location == "Right"))
                                {
                                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string>
                                                                                setBlockedMounts))
                                    {
                                        foreach (string strBlockedMount in strBlocksMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                                            setBlockedMounts.Add(strBlockedMount);
                                        if (_strHasModularMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                                .Any(strLoop => setBlockedMounts.Contains(strLoop)))
                                            continue;
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(_strDisallowedMounts))
                        {
                            string strLoopMount
                                = xmlCyberware
                                    .SelectSingleNodeAndCacheExpression("modularmount", token: token)?.Value;
                            if (!string.IsNullOrEmpty(strLoopMount) && _strDisallowedMounts
                                                                       .SplitNoAlloc(
                                                                           ',', StringSplitOptions.RemoveEmptyEntries)
                                                                       .Contains(strLoopMount))
                                continue;
                        }

                        string strMinRating = xmlCyberware
                            .SelectSingleNodeAndCacheExpression("minrating", token: token)?.Value ?? string.Empty;
                        int intMinRating = 1;
                        int intMaxRating = 1;
                        // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                        if (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                        {
                            (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(xmlCyberware, strMinRating, 1, 0, token).ConfigureAwait(false);
                            intMinRating = blnIsSuccess ? decValue.StandardRound() : 1;
                        }
                        string strMaxRating = xmlCyberware
                            .SelectSingleNodeAndCacheExpression("rating", token: token)?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                        {
                            (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(xmlCyberware, strMaxRating, intMinRating, intMinRating, token).ConfigureAwait(false);
                            intMaxRating = blnIsSuccess ? decValue.StandardRound() : 1;
                        }
                        if (intMaxRating < intMinRating)
                            continue;

                        // Ex-Cons cannot have forbidden or restricted 'ware
                        if (await _objCharacter.GetExConAsync(token).ConfigureAwait(false) && ParentVehicle == null
                                                && xmlCyberware
                                                         .SelectSingleNodeAndCacheExpression(
                                                             "mountsto", token: token) == null)
                        {
                            Cyberware objParent = CyberwareParent;
                            bool blnAnyParentIsModular = objParent != null && !string.IsNullOrEmpty(await objParent.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false));
                            while (objParent != null && !blnAnyParentIsModular)
                            {
                                objParent = await objParent.GetParentAsync(token).ConfigureAwait(false);
                                blnAnyParentIsModular = objParent != null && !string.IsNullOrEmpty(await objParent.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false));
                            }

                            if (!blnAnyParentIsModular)
                            {
                                string strAvailExpr
                                    = xmlCyberware.SelectSingleNodeAndCacheExpression("avail", token: token)?.Value
                                      ?? string.Empty;
                                strAvailExpr = strAvailExpr.ProcessFixedValuesString(intMinRating);

                                if (strAvailExpr.EndsWith('F', 'R'))
                                {
                                    continue;
                                }
                            }
                        }

                        if (!Upgrading && ParentVehicle == null && !await xmlCyberware.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                            continue;

                        if (!blnDoUIUpdate)
                        {
                            return true;
                        }

                        if (blnHideOverAvailLimit && !await xmlCyberware.CheckAvailRestrictionAsync(
                                _objCharacter, intMinRating,
                                (blnIsForceGrade ? 0 : _intAvailModifier) + (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: xmlCyberware.SelectSingleNodeAndCacheExpression("id", token)?.Value, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound(), token).ConfigureAwait(false))
                        {
                            ++intOverLimit;
                            continue;
                        }



                        // Apply essence filtering
                        decimal decMinimumEssence = await nudMinimumEssence.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                        decimal decMaximumEssence = await nudMaximumEssence.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                        decimal decExactEssence = await nudExactEssence.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
                        
                        if (decExactEssence > 0)
                        {
                            // Exact essence filtering
                            string strEssenceExpr = xmlCyberware.SelectSingleNodeAndCacheExpression("ess", token: token)?.Value ?? "0";
                            strEssenceExpr = strEssenceExpr.ProcessFixedValuesString(intMinRating);
                            
                            decimal decEssenceCost = 0;
                            if (!string.IsNullOrEmpty(strEssenceExpr) && !decimal.TryParse(strEssenceExpr, out decEssenceCost))
                            {
                                (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(xmlCyberware, strEssenceExpr, intMinRating, intMinRating, token).ConfigureAwait(false);
                                decEssenceCost = blnIsSuccess ? decValue : 0;
                            }
                            
                            // Apply essence discount if applicable
                            if (decEssenceCost > 0 && _decESSMultiplier != 1.0m)
                            {
                                decEssenceCost *= _decESSMultiplier;
                            }
                            
                            // Check if essence cost matches exactly
                            if (Math.Abs(decEssenceCost - decExactEssence) > 0.001m) // Use small tolerance for floating point comparison
                            {
                                continue;
                            }
                        }
                        else if (decMinimumEssence != 0 || decMaximumEssence != 0)
                        {
                            // Range essence filtering
                            string strEssenceExpr = xmlCyberware.SelectSingleNodeAndCacheExpression("ess", token: token)?.Value ?? "0";
                            strEssenceExpr = strEssenceExpr.ProcessFixedValuesString(intMinRating);
                            
                            decimal decEssenceCost = 0;
                            if (!string.IsNullOrEmpty(strEssenceExpr) && !decimal.TryParse(strEssenceExpr, out decEssenceCost))
                            {
                                (decimal decValue, bool blnIsSuccess) = await ProcessInvariantXPathExpression(xmlCyberware, strEssenceExpr, intMinRating, intMinRating, token).ConfigureAwait(false);
                                decEssenceCost = blnIsSuccess ? decValue : 0;
                            }
                            
                            // Apply essence discount if applicable
                            if (decEssenceCost > 0 && _decESSMultiplier != 1.0m)
                            {
                                decEssenceCost *= _decESSMultiplier;
                            }
                            
                            // Check if essence cost is within the specified range
                            if (decEssenceCost < decMinimumEssence || decEssenceCost > decMaximumEssence)
                            {
                                continue;
                            }
                        }

                        bool blnPassesCostFilter = true;
                        blnPassesCostFilter = await CommonFunctions.CheckCostFilterAsync(xmlCyberware, _objCharacter, 
                            CyberwareParent, 1.0m, 1, decMinimumCost, decMaximumCost, decExactCost, token).ConfigureAwait(false);

                        if (blnPassesCostFilter)
                        {
                            string strId = xmlCyberware.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                            if (!string.IsNullOrEmpty(strId))
                                lstCyberwares.Add(new ListItem(
                                                      strId,
                                                      xmlCyberware.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                      ?? xmlCyberware.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                      ?? strId));
                        }
                    }
                }

                if (blnDoUIUpdate)
                {
                    lstCyberwares.Sort(CompareListItems.CompareNames);
                    if (intOverLimit > 0)
                    {
                        // Add after sort so that it's always at the end
                        lstCyberwares.Add(new ListItem(string.Empty,
                                                       string.Format(GlobalSettings.CultureInfo,
                                                                     await LanguageManager.GetStringAsync(
                                                                             "String_RestrictedItemsHiddenEssence",
                                                                             token: token)
                                                                         .ConfigureAwait(false),
                                                                     intOverLimit)));
                    }

                    string strOldSelected = await lstCyberware
                                                  .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                                  .ConfigureAwait(false);
                    Interlocked.Increment(ref _intLoading);
                    try
                    {
                        await lstCyberware.PopulateWithListItemsAsync(lstCyberwares, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                    await lstCyberware.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        else
                            x.SelectedIndex = -1;
                    }, token: token).ConfigureAwait(false);
                }

                return lstCyberwares?.Count > 0;
            }
            finally
            {
                if (lstCyberwares != null)
                    Utils.ListItemListPool.Return(ref lstCyberwares);
            }
        }

        /// <summary>
        /// Is a given piece of ware being Upgraded?
        /// </summary>
        public bool Upgrading { get; set; }

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
            string strSelectedId = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if ((await cboGrade.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false)).StartsWith('*'))
            {
                await Program.ShowScrollableMessageBoxAsync(this,
                    await LanguageManager.GetStringAsync("Message_BannedGrade", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_BannedGrade", token: token).ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                return;
            }
            XPathNavigator objCyberwareNode = _xmlBaseCyberwareDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
            if (objCyberwareNode == null)
                return;

            int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetEnforceCapacityAsync(token).ConfigureAwait(false) && _objParentObject != null)
            {
                // Capacity.
                bool blnAddToParentCapacity = objCyberwareNode.SelectSingleNodeAndCacheExpression("addtoparentcapacity", token: token) != null;
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objCyberwareNode.SelectSingleNodeAndCacheExpression("capacity", token: token)?.Value;
                if (strCapacity?.Contains('[') == true)
                {
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    strCapacity = strCapacity.ProcessFixedValuesString(intRating);

                    decimal decCapacity = 0;
                    if (strCapacity != "*" && strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decCapacity))
                    {
                        int intMinRating = await nudRating.DoThreadSafeFuncAsync(x => x.MinimumAsInt, token: token).ConfigureAwait(false);
                        decCapacity = (await ProcessInvariantXPathExpression(objCyberwareNode, strCapacity, intMinRating, intRating, token).ConfigureAwait(false)).Item1;
                    }

                    Cyberware objGrandparent = _objParentObject is Cyberware objCyberwareParent
                        ? await objCyberwareParent.GetParentAsync(token).ConfigureAwait(false)
                        : null;
                    decimal decMaximumCapacityUsed = blnAddToParentCapacity
                        ? objGrandparent != null
                            ? await objGrandparent.GetCapacityRemainingAsync(token).ConfigureAwait(false)
                            : decimal.MaxValue
                        : MaximumCapacity;

                    if (decMaximumCapacityUsed < decCapacity)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_OverCapacityLimit", token: token).ConfigureAwait(false),
                                decMaximumCapacityUsed.ToString("#,0.##", GlobalSettings.CultureInfo),
                                decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo)),
                            await LanguageManager.GetStringAsync("MessageTitle_OverCapacityLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        return;
                    }
                }
            }
            if (!Upgrading && ParentVehicle == null && !await objCyberwareNode.RequirementsMetAsync(_objCharacter, strLocalName: await LanguageManager.GetStringAsync(WindowMode == Mode.Cyberware ? "String_SelectPACKSKit_Cyberware" : "String_SelectPACKSKit_Bioware", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                return;

            string strForceGrade = objCyberwareNode.SelectSingleNodeAndCacheExpression("forcegrade", token: token)?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.Find(x => string.Equals(x.SourceIDString, strForceGrade, StringComparison.OrdinalIgnoreCase));
                else
                    return;
            }

            s_strSelectCategory = GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0
                ? _strSelectedCategory
                : objCyberwareNode.SelectSingleNodeAndCacheExpression("category", token: token)?.Value ?? string.Empty;
            _sStrSelectGrade = SelectedGrade?.SourceIDString;
            SelectedCyberware = strSelectedId;
            SelectedRating = intRating;
            BlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            _decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
            _blnFreeCost = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            await nudESSDiscount.DoThreadSafeAsync(x =>
            {
                if (x.Visible)
                    SelectedESSDiscount = x.ValueAsInt;
            }, token: token).ConfigureAwait(false);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        private bool _blnPopulatingGrades;

        /// <summary>
        /// Populate the list of Cyberware Grades.
        /// </summary>
        /// <param name="setDisallowedGrades">Set of all grades that should not be shown.</param>
        /// <param name="blnForce">Force grades to be repopulated.</param>
        /// <param name="strForceGrade">If not empty, force this grade to be selected.</param>
        /// <param name="blnHideBannedGrades">Whether to hide grades banned by the character's gameplay options.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task PopulateGrades(ICollection<string> setDisallowedGrades = null, bool blnForce = false, string strForceGrade = "", bool blnHideBannedGrades = true, CancellationToken token = default)
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
                    bool blnSkipCheck = !string.IsNullOrEmpty(strForceGrade) && strForceGrade == _strNoneGradeId;
                    HashSet<string> setBannedWareGrades = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetBannedWareGradesAsync(token).ConfigureAwait(false);
                    foreach (Grade objWareGrade in _lstGrades)
                    {
                        if (!blnSkipCheck && objWareGrade.SourceIDString == _strNoneGradeId)
                            continue;
                        if (string.IsNullOrEmpty(strForceGrade))
                        {
                            if (_setDisallowedGrades.Contains(objWareGrade.Name)) continue;
                            if (objWareGrade.Name.ContainsAny((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(_objCharacter, WindowMode == Mode.Bioware
                                        ? Improvement.ImprovementType.DisableBiowareGrade
                                        : Improvement.ImprovementType.DisableCyberwareGrade, token: token)
                                    .ConfigureAwait(false)).Select(x => x.ImprovedName)))
                            {
                                continue;
                            }

                            if (await _objCharacter.GetAdapsinEnabledAsync(token).ConfigureAwait(false) && WindowMode == Mode.Cyberware)
                            {
                                if (!objWareGrade.Adapsin &&
                                    objWareGrade.Name.ContainsAny(_lstGrades.Where(x => x.Adapsin).Select(x => x.Name)))
                                    continue;
                            }
                            else if (objWareGrade.Adapsin)
                                continue;

                            if (await _objCharacter.GetBurnoutsWayEnabledAsync(token).ConfigureAwait(false))
                            {
                                if (!objWareGrade.Burnout &&
                                    objWareGrade.Name.ContainsAny(_lstGrades.Where(x => x.Burnout).Select(x => x.Name)))
                                    continue;
                            }
                            else if (objWareGrade.Burnout) continue;

                            if (blnHideBannedGrades &&
                                !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false) &&
                                !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                            {
                                string strGradeName = objWareGrade.Name;
                                if (setBannedWareGrades.Contains(strGradeName) || strGradeName.ContainsAny(setBannedWareGrades))
                                    continue;
                            }
                        }

                        if (!await (await objWareGrade.GetNodeXPathAsync(token: token).ConfigureAwait(false)).RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                        {
                            continue;
                        }

                        string strGradeDisplayName = await objWareGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        if (blnHideBannedGrades && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false) && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                        {
                            string strGradeName = objWareGrade.Name;
                            if (setBannedWareGrades.Contains(strGradeName) || strGradeName.ContainsAny(setBannedWareGrades))
                                lstGrade.Add(new ListItem(objWareGrade.SourceIDString, "*" + strGradeDisplayName));
                            else
                                lstGrade.Add(new ListItem(objWareGrade.SourceIDString, strGradeDisplayName));
                        }
                        else
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceIDString, strGradeDisplayName));
                        }
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
                        if (blnDoSkipRefresh)
                            Interlocked.Decrement(ref _intSkipListRefresh);
                    }
                }
            }
            _blnPopulatingGrades = false;
        }

        private bool _blnPopulatingCategories;

        private async Task PopulateCategories(CancellationToken token = default)
        {
            if (_blnPopulatingCategories)
                return;
            _blnPopulatingCategories = true;
            XPathNodeIterator objXmlCategoryList;
            if (_strSubsystems.Length > 0)
            {
                // Populate the Cyberware Category list.
                string strSubsystem = "categories/category[. = " + _strSubsystems.CleanXPath().Replace(",", "\" or . = \"") + "]";
                objXmlCategoryList = _xmlBaseCyberwareDataNode.Select(strSubsystem);
            }
            else
            {
                objXmlCategoryList = _xmlBaseCyberwareDataNode.SelectAndCacheExpression("categories/category", token: token);
            }

            string strOldSelectedCyberware = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
            {
                foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
                {
                    // Make sure the category contains items that we can actually display
                    if (await AnyItemInList(objXmlCategory.Value, token: token).ConfigureAwait(false))
                    {
                        string strInnerText = objXmlCategory.Value;
                        lstCategory.Add(new ListItem(strInnerText,
                                                     objXmlCategory.SelectSingleNodeAndCacheExpression("@translate", token: token)
                                                                   ?.Value ?? strInnerText));
                    }
                }

                lstCategory.Sort(CompareListItems.CompareNames);

                if (lstCategory.Count > 0)
                {
                    lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: token).ConfigureAwait(false)));
                }

                string strOldSelected = _strSelectedCategory;
                Interlocked.Increment(ref _intLoading);
                try
                {
                    await cboCategory.PopulateWithListItemsAsync(lstCategory, token: token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
                await cboCategory.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    if (x.SelectedIndex == -1 && lstCategory.Count > 0)
                        x.SelectedIndex = 0;
                }, token: token).ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(strOldSelectedCyberware))
                await lstCyberware.DoThreadSafeAsync(x => x.SelectedValue = strOldSelectedCyberware, token: token).ConfigureAwait(false);

            _blnPopulatingCategories = false;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            try
            {
                await CommonFunctions.OpenPdfFromControl(sender, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private readonly Microsoft.VisualStudio.Threading.AsyncLazy<string> _strCachedParentCost;
        private readonly Microsoft.VisualStudio.Threading.AsyncLazy<string> _strCachedParentGearCost;

        private async Task<ValueTuple<decimal, bool>> ProcessInvariantXPathExpression(XPathNavigator xmlCyberware, string strExpression, int intMinRating, int intRating, CancellationToken token = default)
        {
            // Use the unified method for basic pattern replacement
            var (decValue, blnSuccess) = await CommonFunctions.ProcessInvariantXPathExpressionAsync(
                strExpression, 
                intRating, 
                _objCharacter, 
                CyberwareParent, 
                xmlCyberware, 
                intMinRating, 
                token: token).ConfigureAwait(false);
            
            // If the unified method couldn't handle it, fall back to the complex cyberlimb logic
            if (!blnSuccess && strExpression.HasValuesNeedingReplacementForXPathProcessing())
            {
                return await ProcessComplexCyberlimbExpression(xmlCyberware, strExpression, intMinRating, intRating, token).ConfigureAwait(false);
            }
            
            return new ValueTuple<decimal, bool>(decValue, blnSuccess);
        }
        
        private async Task<ValueTuple<decimal, bool>> ProcessComplexCyberlimbExpression(XPathNavigator xmlCyberware, string strExpression, int intMinRating, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = true;
            strExpression = strExpression.ProcessFixedValuesString(intRating).TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnSuccess = false;
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Cost}",
                            () => _strCachedParentCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Parent Cost",
                            () => _strCachedParentCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Gear Cost}",
                            () => _strCachedParentGearCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Parent Gear Cost",
                            () => _strCachedParentGearCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        // Deliberately don't replace "Gear Cost" and "Children Cost" so that they get properly displayed in the UI

                        await sbdValue.CheapReplaceAsync(strExpression, "{MinRating}",
                            () => intMinRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "MinRating",
                            () => intMinRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Rating}",
                            () => intRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Rating",
                            () => intRating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                        Dictionary<string, int> dicVehicleValues = null;
                        if (strExpression.Contains("{STR") || strExpression.Contains("{AGI"))
                        {
                            Cyberware objCyberlimbParent = null;
                            if (strExpression.Contains("imum}"))
                            {
                                string strCategory = xmlCyberware.SelectSingleNodeAndCacheExpression("category", token)?.Value;
                                string strLimbSlot = xmlCyberware.SelectSingleNodeAndCacheExpression("limbslot", token)?.Value;
                                string strMountsTo = xmlCyberware.SelectSingleNodeAndCacheExpression("mountsto", token)?.Value;
                                if (strCategory == "Cyberlimb" || !string.IsNullOrEmpty(strLimbSlot) || !string.IsNullOrEmpty(strMountsTo))
                                {
                                    int intMinAgility = 3;
                                    int intMinStrength = 3;
                                    xmlCyberware.TryGetInt32FieldQuickly("minagility", ref intMinAgility);
                                    xmlCyberware.TryGetInt32FieldQuickly("minstrength", ref intMinStrength);
                                    dicVehicleValues = new Dictionary<string, int>(2)
                                {
                                    { "STRMinimum", intMinStrength },
                                    { "AGIMinimum", intMinAgility }
                                };
                                }
                                else
                                {
                                    objCyberlimbParent = CyberwareParent;
                                    while (objCyberlimbParent != null && objCyberlimbParent.Category != "Cyberlimb" && !await objCyberlimbParent.GetIsLimbAsync(token).ConfigureAwait(false))
                                        objCyberlimbParent = objCyberlimbParent.Parent;
                                    if (objCyberlimbParent != null)
                                    {
                                        dicVehicleValues = new Dictionary<string, int>(2)
                                    {
                                        { "STRMinimum", await objCyberlimbParent.GetMinStrengthAsync(token).ConfigureAwait(false) },
                                        { "AGIMinimum", await objCyberlimbParent.GetMinAgilityAsync(token).ConfigureAwait(false) }
                                    };
                                    }
                                    else if (ParentVehicle != null)
                                    {
                                        int intTotalBody = await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                                        dicVehicleValues = new Dictionary<string, int>(4)
                                {
                                    { "STRMaximum", Math.Max(1, intTotalBody * 2) },
                                    { "AGIMaximum", Math.Max(1, await ParentVehicle.GetMaxPilotAsync(token).ConfigureAwait(false)) },
                                    { "STRMinimum", Math.Max(1, intTotalBody) },
                                    { "AGIMinimum", Math.Max(1, await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false)) }
                                };
                                    }
                                }
                            }
                            if (strExpression.Contains("{STR}") || strExpression.Contains("{AGI}")
                                || strExpression.Contains("{STRUnaug}") || strExpression.Contains("{AGIUnaug}")
                                || strExpression.Contains("{STRBase}") || strExpression.Contains("{AGIBase}"))
                            {
                                int intSTR = -1;
                                int intSTRValue = -1;
                                int intSTRBase = -1;
                                int intAGI = -1;
                                int intAGIValue = -1;
                                int intAGIBase = -1;
                                string strCategory = xmlCyberware.SelectSingleNodeAndCacheExpression("category", token)?.Value;
                                string strLimbSlot = xmlCyberware.SelectSingleNodeAndCacheExpression("limbslot", token)?.Value;
                                string strMountsTo = xmlCyberware.SelectSingleNodeAndCacheExpression("mountsto", token)?.Value;
                                if (strCategory == "Cyberlimb" || !string.IsNullOrEmpty(strLimbSlot) || !string.IsNullOrEmpty(strMountsTo))
                                {
                                    int intMinAgility = 3;
                                    int intMinStrength = 3;
                                    xmlCyberware.TryGetInt32FieldQuickly("minagility", ref intMinAgility);
                                    xmlCyberware.TryGetInt32FieldQuickly("minstrength", ref intMinStrength);
                                    int intAgilityValue;
                                    int intStrengthValue;
                                    int intAgilityTotalValue;
                                    int intStrengthTotalValue;
                                    if (ParentVehicle == null)
                                    {
                                        CharacterAttrib objAgi = await _objCharacter.GetAttributeAsync("AGI", token: token)
                                                                                          .ConfigureAwait(false);
                                        CharacterAttrib objStr = await _objCharacter.GetAttributeAsync("STR", token: token)
                                                                                          .ConfigureAwait(false);
                                        intAgilityValue = Math.Min(
                                            intMinAgility,
                                            objAgi != null
                                                ? await objAgi.GetTotalMaximumAsync(token).ConfigureAwait(false)
                                                : 0);
                                        intStrengthValue = Math.Min(
                                            intMinStrength,
                                            objStr != null
                                                ? await objStr.GetTotalMaximumAsync(token).ConfigureAwait(false)
                                                : 0);

                                        intStrengthTotalValue = intAgilityTotalValue = Math.Min(await _objCharacter.GetRedlinerBonusAsync(token).ConfigureAwait(false),
                                            await (await _objCharacter.GetSettingsAsync(token)).GetCyberlimbAttributeBonusCapAsync(token).ConfigureAwait(false));

                                        intAgilityTotalValue = Math.Min(
                                            intAgilityValue + intAgilityTotalValue,
                                            objAgi != null
                                                ? await objAgi.GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false)
                                                : 0);
                                        intStrengthTotalValue = Math.Min(
                                            intStrengthValue + intStrengthTotalValue,
                                            objStr != null
                                                ? await objStr.GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false)
                                                : 0);
                                    }
                                    else
                                    {
                                        intAgilityTotalValue = intAgilityValue = Math.Min(intMinAgility, Math.Max(await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) * 2, 1));
                                        intStrengthTotalValue = intStrengthValue = Math.Min(intMinStrength, Math.Max(await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) * 2, 1));
                                    }
                                    intSTR = intStrengthTotalValue;
                                    intSTRValue = intStrengthValue;
                                    intSTRBase = intMinStrength;
                                    intAGI = intAgilityTotalValue;
                                    intAGIValue = intAgilityValue;
                                    intAGIBase = intMinAgility;
                                }
                                else
                                {
                                    if (objCyberlimbParent == null)
                                    {
                                        objCyberlimbParent = CyberwareParent;
                                        while (objCyberlimbParent != null && objCyberlimbParent.Category != "Cyberlimb" && !await objCyberlimbParent.GetIsLimbAsync(token).ConfigureAwait(false))
                                            objCyberlimbParent = objCyberlimbParent.Parent;
                                    }
                                    if (objCyberlimbParent != null)
                                    {
                                        intSTR = await objCyberlimbParent.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                        intSTRValue = await objCyberlimbParent.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                        intSTRBase = await objCyberlimbParent.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                        intAGI = await objCyberlimbParent.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                        intAGIValue = await objCyberlimbParent.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                        intAGIBase = await objCyberlimbParent.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                    }
                                    else if (ParentVehicle != null)
                                    {
                                        if (ParentVehicleMod != null)
                                        {
                                            intSTRValue = intSTR = await ParentVehicleMod.GetTotalStrengthAsync(token).ConfigureAwait(false);
                                            intAGIValue = intAGI = await ParentVehicleMod.GetTotalAgilityAsync(token).ConfigureAwait(false);
                                            intSTRBase = await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                                            intAGIBase = await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            int intVehicleBody = await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                                            int intVehiclePilot = await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                                            intSTRBase = intSTRValue = intSTR = intVehicleBody;
                                            intAGIBase = intAGIValue = intAGI = intVehiclePilot;
                                        }
                                    }
                                }

                                if (intSTR >= 0 || intAGI >= 0)
                                {
                                    if (dicVehicleValues == null)
                                    {
                                        dicVehicleValues = new Dictionary<string, int>(6)
                                    {
                                        {"STR", intSTR},
                                        {"STRUnaug", intSTRValue},
                                        {"STRBase", intSTRBase},
                                        {"AGI", intAGI},
                                        {"AGIUnaug", intAGIValue},
                                        {"AGIBase", intAGIBase}
                                    };
                                    }
                                    else
                                    {
                                        dicVehicleValues["STR"] = intSTR;
                                        dicVehicleValues["STRUnaug"] = intSTRValue;
                                        dicVehicleValues["STRBase"] = intSTRBase;
                                        dicVehicleValues["AGI"] = intAGI;
                                        dicVehicleValues["AGIUnaug"] = intAGIValue;
                                        dicVehicleValues["AGIBase"] = intAGIBase;
                                    }
                                }
                            }
                        }

                        if (ParentVehicle != null)
                            await ParentVehicle.ProcessAttributesInXPathAsync(sbdValue, strExpression, dicValueOverrides: dicVehicleValues, token: token).ConfigureAwait(false);
                        else
                        {
                            Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                            await _objCharacter.ProcessAttributesInXPathAsync(sbdValue, strExpression, dicVehicleValues, token: token).ConfigureAwait(false);
                        }
                        strExpression = sbdValue.ToString();
                    }
                }
                (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(strExpression, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    return new ValueTuple<decimal, bool>(Convert.ToDecimal((double)objProcess), true);
            }

            return new ValueTuple<decimal, bool>(decValue, blnSuccess);
        }

        private void NumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        #endregion Methods
    }
}
