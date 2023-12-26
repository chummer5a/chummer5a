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
        private HashSet<string> _setDisallowedGrades = Utils.StringHashSetPool.Get();
        private string _strForceGrade = string.Empty;
        private readonly object _objParentObject;
        private readonly XPathNavigator _objParentNode;
        private HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private readonly XPathNavigator _xmlBaseCyberwareDataNode;
        private CancellationTokenSource _objUpdateCyberwareInfoCancellationTokenSource;
        private CancellationTokenSource _objProcessGradeChangedCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshSelectedCyberwareCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        private enum Mode
        {
            Cyberware = 0,
            Bioware
        }

        #region Control Events

        public SelectCyberware(Character objCharacter, Improvement.ImprovementSource objWareSource, object objParentNode = null)
        {
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
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
                _objGenericCancellationTokenSource.Dispose();
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
                Utils.StringHashSetPool.Return(ref _setDisallowedGrades);
            };
            InitializeComponent();

            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objParentObject = objParentNode;
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

            _lstGrades = _objCharacter.GetGradesList(objWareSource);
            _strNoneGradeId = _lstGrades.Find(x => x.Name == "None")?.SourceIDString;
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_xmlBaseCyberwareDataNode));
        }

        private async void SelectCyberware_Load(object sender, EventArgs e)
        {
            try
            {
                if (_objCharacter.Created)
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
                }
                else
                {
                    await lblMarkupLabel.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                        .ConfigureAwait(false);
                    await nudMarkup.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                   .ConfigureAwait(false);
                    await lblMarkupPercentLabel.DoThreadSafeAsync(x => x.Visible = false, token: _objGenericToken)
                                               .ConfigureAwait(false);
                    await chkHideBannedGrades
                          .DoThreadSafeAsync(x => x.Visible = !_objCharacter.IgnoreRules, token: _objGenericToken)
                          .ConfigureAwait(false);
                    await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                    {
                        x.Text = string.Format(
                            GlobalSettings.CultureInfo, x.Text,
                            _objCharacter.Settings.MaximumAvailability);
                        x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                    }, token: _objGenericToken).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(DefaultSearchText))
                {
                    await txtSearch.DoThreadSafeAsync(x =>
                    {
                        x.Text = DefaultSearchText;
                        x.Enabled = false;
                    }, token: _objGenericToken).ConfigureAwait(false);
                }

                await chkPrototypeTranshuman
                      .DoThreadSafeAsync(
                          x => x.Visible = _objCharacter.IsPrototypeTranshuman && _eMode == Mode.Bioware
                                                                               && !_objCharacter.Created,
                          token: _objGenericToken).ConfigureAwait(false);

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

                await chkBlackMarketDiscount
                      .DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount,
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
                        _decCostMultiplier = Convert.ToDecimal(
                            xmlGrade.SelectSingleNodeAndCacheExpression("cost", token: _objGenericToken)?.Value, GlobalSettings.InvariantCultureInfo);
                        _decESSMultiplier = Convert.ToDecimal(
                            xmlGrade.SelectSingleNodeAndCacheExpression("ess", token: _objGenericToken)?.Value, GlobalSettings.InvariantCultureInfo);
                        _intAvailModifier
                            = xmlGrade.SelectSingleNodeAndCacheExpression("avail", token: _objGenericToken)?.ValueAsInt ?? 0;
                    }
                }

                await lblESSDiscountLabel
                      .DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts,
                                         token: _objGenericToken).ConfigureAwait(false);
                await lblESSDiscountPercentLabel
                      .DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts,
                                         token: _objGenericToken).ConfigureAwait(false);
                await nudESSDiscount
                      .DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts,
                                         token: _objGenericToken).ConfigureAwait(false);
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
                        _decCostMultiplier
                            = Convert.ToDecimal(
                                xmlGrade.SelectSingleNodeAndCacheExpression("cost", token)?.Value, GlobalSettings.InvariantCultureInfo);
                        _decESSMultiplier
                            = Convert.ToDecimal(
                                xmlGrade.SelectSingleNodeAndCacheExpression("ess", token)
                                ?.Value, GlobalSettings.InvariantCultureInfo);
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
                            string strMinRating = xmlCyberware.SelectSingleNodeAndCacheExpression("minrating", token)?.Value;
                            int intMinRating = 1;
                            // Not a simple integer, so we need to start mucking around with strings
                            if (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                            {
                                strMinRating = await strMinRating.CheapReplaceAsync("MaximumSTR",
                                                                     async () => (ParentVehicle != null
                                                                         ? Math.Max(1, await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) * 2)
                                                                         : await _objCharacter.STR.GetTotalMaximumAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                                 .CheapReplaceAsync("MaximumAGI",
                                                                     async () => (ParentVehicle != null
                                                                         ? Math.Max(1, await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false) * 2)
                                                                         : await _objCharacter.AGI.GetTotalMaximumAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                                 .CheapReplaceAsync("MinimumSTR",
                                                                     async () => (ParentVehicle != null ? await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                                 .CheapReplaceAsync("MinimumAGI",
                                                                     async () => (ParentVehicle != null ? await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating, token).ConfigureAwait(false);
                                intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                            }
                            await nudRating.DoThreadSafeAsync(x => x.Minimum = intMinRating, token: token).ConfigureAwait(false);

                            string strMaxRating = xmlRatingNode.Value;
                            int intMaxRating = 0;
                            // Not a simple integer, so we need to start mucking around with strings
                            if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                            {
                                strMaxRating = await strMaxRating.CheapReplaceAsync("MaximumSTR",
                                                                     async () => (ParentVehicle != null
                                                                         ? Math.Max(1, await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) * 2)
                                                                         : await _objCharacter.STR.GetTotalMaximumAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                                 .CheapReplaceAsync("MaximumAGI",
                                                                     async () => (ParentVehicle != null
                                                                         ? Math.Max(1, await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false) * 2)
                                                                         : await _objCharacter.AGI.GetTotalMaximumAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                                 .CheapReplaceAsync("MinimumSTR",
                                                                     async () => (ParentVehicle != null ? await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                                                 .CheapReplaceAsync("MinimumAGI",
                                                                     async () => (ParentVehicle != null ? await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMaxRating, token).ConfigureAwait(false);
                                intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                            }
                            if (await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            {
                                while (intMaxRating > intMinRating
                                       && !await xmlCyberware.CheckAvailRestrictionAsync(
                                           _objCharacter, intMaxRating, _intAvailModifier, token: token).ConfigureAwait(false))
                                {
                                    --intMaxRating;
                                }
                            }
                            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            {
                                decimal decCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                                decCostMultiplier *= _decCostMultiplier;
                                if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                                    decCostMultiplier *= 0.9m;
                                while (intMaxRating > intMinRating && !await xmlCyberware.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier, intMaxRating, token).ConfigureAwait(false))
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
                        await objSource.SetControlAsync(lblSource, token: token).ConfigureAwait(false);
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

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: _objGenericToken).ConfigureAwait(false)
                    && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
                }

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
                                        _eMode == Mode.Bioware
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
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(_strSelectedCategory, _objGenericToken).ConfigureAwait(false);
                }

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
                case Keys.Up when lstCyberware.SelectedIndex - 1 >= 0:
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
        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
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
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

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
                                          + _decMaximumCapacity.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
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
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public Vehicle ParentVehicle { get; set; }

        public decimal Markup { get; set; }

        /// <summary>
        /// Whether the bioware should be discounted by Prototype Transhuman.
        /// </summary>
        public bool PrototypeTranshuman => chkPrototypeTranshuman.Checked && _eMode == Mode.Bioware && !_objCharacter.Created;

        /// <summary>
        /// Parent cyberware that the current selection will be added to.
        /// </summary>
        public Cyberware CyberwareParent { get; set; }

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
                        string strAvailExpression = objXmlCyberware
                            .SelectSingleNodeAndCacheExpression("avail", token)?.Value ?? string.Empty;
                        AvailabilityValue objTotalAvail = new AvailabilityValue(
                            intRating,
                            await strAvailExpression.CheapReplaceAsync("MinRating",
                                                                       () => nudRating.DoThreadSafeFuncAsync(
                                                                           y => y.Minimum.ToString(
                                                                               GlobalSettings.InvariantCultureInfo), token: token),
                                                                       token: token).ConfigureAwait(false), _intAvailModifier);
                        await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        string strAvail = await objTotalAvail.ToStringAsync(token).ConfigureAwait(false);
                        await lblAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token).ConfigureAwait(false);

                        // Cost.
                        decimal decItemCost = 0;
                        string strCost;
                        if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            strCost = 0.0m.ToString(_objCharacter.Settings.NuyenFormat,
                                                              GlobalSettings.CultureInfo)
                                              + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            strCost = objXmlCyberware.SelectSingleNodeAndCacheExpression("cost", token)?.Value;
                            if (!string.IsNullOrEmpty(strCost))
                            {
                                if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                                {
                                    string strSuffix = string.Empty;
                                    if (!strCost.EndsWith(')'))
                                    {
                                        strSuffix = strCost.Substring(strCost.LastIndexOf(')') + 1);
                                        strCost = strCost.TrimEndOnce(strSuffix);
                                    }

                                    string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                                .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    strCost = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                                    strCost += strSuffix;
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
                                    {
                                        decMin = Convert.ToDecimal(strCost.FastEscape('+'),
                                                                   GlobalSettings.InvariantCultureInfo);
                                    }

                                    strCost = decMax == decimal.MaxValue
                                        ? decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                          GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                 .ConfigureAwait(false) + '+'
                                        : decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                          GlobalSettings.CultureInfo)
                                          + " - " + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                                    GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                                 .ConfigureAwait(false);

                                    decItemCost = decMin;
                                }
                                else
                                {
                                    strCost = (await strCost
                                                     .CheapReplaceAsync("Parent Cost", () => CyberwareParent?.Cost ?? "0",
                                                                        token: token)
                                                     .CheapReplaceAsync("Parent Gear Cost",
                                                                        async () => CyberwareParent != null
                                                                            ? (await CyberwareParent.GearChildren
                                                                                .SumParallelAsync(
                                                                                    x => x.GetTotalCostAsync(token), token: token)
                                                                                .ConfigureAwait(false))
                                                                            .ToString(GlobalSettings.InvariantCultureInfo)
                                                                            : "0", token: token)
                                                     .CheapReplaceAsync("MinRating",
                                                                        () => nudRating.DoThreadSafeFuncAsync(
                                                                            x => x.Minimum.ToString(
                                                                                GlobalSettings.InvariantCultureInfo),
                                                                            token: token), token: token)
                                                     .ConfigureAwait(false))
                                        .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

                                    (bool blnIsSuccess, object objProcess)
                                        = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token).ConfigureAwait(false);
                                    if (blnIsSuccess)
                                    {
                                        decItemCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo)
                                                      * _decCostMultiplier * decGenetechCostModifier;
                                        decItemCost *= 1 + nudMarkup.Value / 100.0m;

                                        if (chkBlackMarketDiscount.Checked)
                                        {
                                            decItemCost *= 0.9m;
                                        }

                                        strCost = decItemCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                            + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        strCost += await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                                    }
                                }
                            }
                            else
                            {
                                strCost = 0.0m.ToString(_objCharacter.Settings.NuyenFormat,
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
                        await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts, token: token).ConfigureAwait(false);
                        await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts, token: token).ConfigureAwait(false);
                        await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts, token: token).ConfigureAwait(false);

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
                                    if (strSelectCategory == "Basic" && _eMode == Mode.Bioware)
                                        decCharacterESSModifier -= 1 - BasicBiowareESSMultiplier;
                                    if (blnIsGeneware)
                                        decCharacterESSModifier -= 1 - GenetechEssMultiplier;

                                    if ((await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).AllowCyberwareESSDiscounts)
                                    {
                                        decimal decDiscountModifier = await nudESSDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                                        decCharacterESSModifier *= 1.0m - decDiscountModifier;
                                    }

                                    decCharacterESSModifier -= 1 - _decESSMultiplier;

                                    decCharacterESSModifier *= CharacterTotalESSMultiplier;

                                    string strPostModifierExpression
                                        = (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                                        .EssenceModifierPostExpression;
                                    if (!string.IsNullOrEmpty(strPostModifierExpression) && strPostModifierExpression != "{Modifier}")
                                    {
                                        (bool blnIsSuccess2, object objProcess2)
                                            = await CommonFunctions.EvaluateInvariantXPathAsync(
                                                strPostModifierExpression.Replace(
                                                    "{Modifier}",
                                                    decCharacterESSModifier.ToString(GlobalSettings.InvariantCultureInfo)),
                                                token).ConfigureAwait(false);
                                        if (blnIsSuccess2)
                                            decCharacterESSModifier = Convert.ToDecimal(objProcess2, GlobalSettings.InvariantCultureInfo);
                                    }
                                }

                                string strEss = objXmlCyberware.SelectSingleNodeAndCacheExpression("ess", token)?.Value ?? string.Empty;
                                if (strEss.StartsWith("FixedValues(", StringComparison.Ordinal))
                                {
                                    string strSuffix = string.Empty;
                                    if (!strEss.EndsWith(')'))
                                    {
                                        strSuffix = strEss.Substring(strEss.LastIndexOf(')') + 1);
                                        strEss = strEss.TrimEndOnce(strSuffix);
                                    }

                                    string[] strValues = strEss.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                               .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    strEss = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                                    strEss += strSuffix;
                                }

                                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                    strEss.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                                if (blnIsSuccess)
                                {
                                    decESS = decCharacterESSModifier
                                             * Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                                    if (!_objCharacter.Settings.DontRoundEssenceInternally)
                                    {
                                        decESS = decimal.Round(decESS, _objCharacter.Settings.EssenceDecimals,
                                                               MidpointRounding.AwayFromZero);
                                    }
                                }
                            }

                            await lblEssence.DoThreadSafeAsync(x =>
                            {
                                x.Text = decESS.ToString(_objCharacter.Settings.EssenceFormat, GlobalSettings.CultureInfo);
                                if (blnAddToParentESS)
                                    x.Text = '+' + x.Text;
                            }, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblEssence.DoThreadSafeAsync(x => x.Text
                                                                   = 0.0m.ToString(
                                                                       _objCharacter.Settings.EssenceFormat,
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
                            if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                            {
                                string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                                .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                strCapacity = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                            }

                            if (strCapacity == "[*]" || strCapacity == "*")
                                await lblCapacity.DoThreadSafeAsync(x => x.Text = "*", token: token).ConfigureAwait(false);
                            else
                            {
                                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                if (intPos != -1)
                                {
                                    string strFirstHalf = strCapacity.Substring(0, intPos);
                                    string strSecondHalf
                                        = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);

                                    blnSquareBrackets = strFirstHalf.StartsWith('[');
                                    if (blnSquareBrackets && strFirstHalf.Length > 1)
                                        strFirstHalf = strFirstHalf.Substring(1, strCapacity.Length - 2);

                                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                        strFirstHalf.Replace(
                                            "Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);

                                    await lblCapacity.DoThreadSafeAsync(x =>
                                    {
                                        x.Text = blnIsSuccess
                                            ? objProcess.ToString()
                                            : strFirstHalf;
                                        if (blnSquareBrackets)
                                            x.Text = '[' + x.Text + ']';
                                    }, token: token).ConfigureAwait(false);

                                    strSecondHalf = strSecondHalf.Trim('[', ']');
                                    (bool blnIsSuccess2, object objProcess2) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                        strSecondHalf.Replace(
                                            "Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                                    strSecondHalf = (blnAddToParentCapacity ? "+[" : "[")
                                                    + (blnIsSuccess2 ? objProcess2.ToString() : strSecondHalf) + ']';

                                    await lblCapacity.DoThreadSafeAsync(x => x.Text += '/' + strSecondHalf, token: token).ConfigureAwait(false);
                                }
                                else
                                {
                                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                        strCapacity.Replace(
                                            "Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                                    await lblCapacity.DoThreadSafeAsync(x =>
                                    {
                                        x.Text = blnIsSuccess
                                            ? objProcess.ToString()
                                            : strCapacity;
                                        if (blnSquareBrackets)
                                        {
                                            x.Text = blnAddToParentCapacity
                                                ? "+[" + x.Text + ']'
                                                : '[' + x.Text + ']';
                                        }
                                    }, token: token).ConfigureAwait(false);
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
                    await lstCyberware.PopulateWithListItemsAsync(ListItem.Blank.Yield(), token: token)
                                      .ConfigureAwait(false);
                }

                return false;
            }

            string strCurrentGradeId = await cboGrade
                                             .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                             .ConfigureAwait(false);
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId)
                ? null
                : _lstGrades.Find(x => string.Equals(x.SourceIDString, strCurrentGradeId, StringComparison.OrdinalIgnoreCase));

            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(')
                         .Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false))
                         .Append(')');
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdCategoryFilter))
                {
                    if (strCategory != "Show All" && !Upgrading
                                                  && (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0))
                    {
                        sbdCategoryFilter.Append("category = ").Append(strCategory.CleanXPath())
                                         .Append(" or category = \"None\"");
                    }
                    else
                    {
                        foreach (ListItem objItem in cboCategory.Items)
                        {
                            string strItem = objItem.Value.ToString();
                            if (!string.IsNullOrEmpty(strItem))
                                sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Append("category = \"None\"");
                        }
                    }

                    if (sbdCategoryFilter.Length > 0)
                        sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                }

                if (_objParentNode != null)
                    sbdFilter.Append(" and (requireparent or contains(capacity, \"[\")) and not(mountsto)");
                else if (ParentVehicle == null && ((!_objCharacter.AddCyberwareEnabled && _eMode == Mode.Cyberware)
                                                   || (!_objCharacter.AddBiowareEnabled && _eMode == Mode.Bioware)))
                {
                    sbdFilter.Append(" and (id = ").Append(Cyberware.EssenceHoleGUID.ToString().CleanXPath())
                             .Append(" or id = ").Append(Cyberware.EssenceAntiHoleGUID.ToString().CleanXPath())
                             .Append(" or mountsto)");
                }
                else
                    sbdFilter.Append(" and not(requireparent)");
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

            bool blnCyberwareDisabled = !_objCharacter.AddCyberwareEnabled;
            bool blnBiowareDisabled = !_objCharacter.AddBiowareEnabled;
            int intOverLimit = 0;
            List<ListItem> lstCyberwares = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                if (xmlIterator.Count > 0)
                {
                    bool blnHideOverAvailLimit = await chkHideOverAvailLimit
                                                       .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                       .ConfigureAwait(false);
                    bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems
                                                        .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                        .ConfigureAwait(false);
                    bool blnFree = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                .ConfigureAwait(false);
                    decimal decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                                       .ConfigureAwait(false);
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
                                    lstWareListToCheck = CyberwareParent.Children;
                                else if (ParentVehicle == null)
                                    lstWareListToCheck = _objCharacter.Cyberware;
                                if (xmlCyberware
                                          .SelectSingleNodeAndCacheExpression("selectside", token: token) == null
                                    || !string.IsNullOrEmpty(CyberwareParent?.Location) ||
                                    lstWareListToCheck?.Any(x => x.Location == "Left") == true
                                    && lstWareListToCheck.Any(x => x.Location == "Right"))
                                {
                                    string[] astrBlockedMounts
                                        = strBlocksMounts.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    if (_strHasModularMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                            .Any(strLoop => astrBlockedMounts.Contains(strLoop)))
                                        continue;
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

                        string strMaxRating = xmlCyberware
                            .SelectSingleNodeAndCacheExpression("rating", token: token)?.Value;
                        string strMinRating = xmlCyberware
                            .SelectSingleNodeAndCacheExpression("minrating", token: token)?.Value;
                        int intMinRating = 1;
                        // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                        if ((!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out int intMaxRating))
                            || (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating)))
                        {
                            strMinRating = await strMinRating.CheapReplaceAsync("MaximumSTR",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetTotalBodyAsync(
                                                                                 token).ConfigureAwait(false) * 2)
                                                                         : await _objCharacter.STR.GetTotalMaximumAsync(
                                                                             token).ConfigureAwait(false))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MaximumAGI",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetPilotAsync(token)
                                                                                 .ConfigureAwait(false)
                                                                             * 2)
                                                                         : await _objCharacter.AGI.GetTotalMaximumAsync(
                                                                             token).ConfigureAwait(false))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MinimumSTR",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetTotalBodyAsync(token)
                                                                         .ConfigureAwait(false)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo), token: token)
                                                             .CheapReplaceAsync("MinimumAGI",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetPilotAsync(token)
                                                                         .ConfigureAwait(false)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo),
                                                                 token: token).ConfigureAwait(false);

                            (bool blnIsSuccess, object objProcess)
                                = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating, token)
                                                       .ConfigureAwait(false);
                            intMinRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;

                            strMaxRating = await strMaxRating.CheapReplaceAsync("MaximumSTR",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetTotalBodyAsync(
                                                                                 token).ConfigureAwait(false) * 2)
                                                                         : await _objCharacter.STR.GetTotalMaximumAsync(
                                                                             token).ConfigureAwait(false))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MaximumAGI",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetPilotAsync(token)
                                                                                 .ConfigureAwait(false)
                                                                             * 2)
                                                                         : await _objCharacter.AGI.GetTotalMaximumAsync(
                                                                             token).ConfigureAwait(false))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MinimumSTR",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetTotalBodyAsync(token)
                                                                         .ConfigureAwait(false)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo), token: token)
                                                             .CheapReplaceAsync("MinimumAGI",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetPilotAsync(token)
                                                                         .ConfigureAwait(false)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo),
                                                                 token: token).ConfigureAwait(false);

                            (blnIsSuccess, objProcess)
                                = await CommonFunctions.EvaluateInvariantXPathAsync(strMaxRating, token)
                                                       .ConfigureAwait(false);
                            intMaxRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                            if (intMaxRating < intMinRating)
                                continue;
                        }

                        // Ex-Cons cannot have forbidden or restricted 'ware
                        if (_objCharacter.ExCon && ParentVehicle == null
                                                && xmlCyberware
                                                         .SelectSingleNodeAndCacheExpression(
                                                             "mountsto", token: token) == null)
                        {
                            Cyberware objParent = CyberwareParent;
                            bool blnAnyParentIsModular = !string.IsNullOrEmpty(objParent?.PlugsIntoModularMount);
                            while (objParent != null && !blnAnyParentIsModular)
                            {
                                objParent = objParent.Parent;
                                blnAnyParentIsModular = !string.IsNullOrEmpty(objParent?.PlugsIntoModularMount);
                            }

                            if (!blnAnyParentIsModular)
                            {
                                string strAvailExpr
                                    = xmlCyberware.SelectSingleNodeAndCacheExpression("avail", token: token)?.Value
                                      ?? string.Empty;
                                if (strAvailExpr.StartsWith("FixedValues(", StringComparison.Ordinal))
                                {
                                    string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true)
                                                                     .TrimEndOnce(')')
                                                                     .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    strAvailExpr
                                        = strValues[Math.Max(Math.Min(intMinRating - 1, strValues.Length - 1), 0)];
                                }

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
                                blnIsForceGrade ? 0 : _intAvailModifier, token).ConfigureAwait(false))
                        {
                            ++intOverLimit;
                            continue;
                        }

                        if (blnShowOnlyAffordItems && !blnFree)
                        {
                            decimal decCostMultiplier = 1 + decMarkup / 100.0m;
                            if (_setBlackMarketMaps.Contains(
                                    xmlCyberware
                                        .SelectSingleNodeAndCacheExpression("category", token: token)?.Value))
                                decCostMultiplier *= 0.9m;
                            if (!await xmlCyberware
                                       .CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier, token: token)
                                       .ConfigureAwait(false))
                            {
                                ++intOverLimit;
                                continue;
                            }
                        }

                        lstCyberwares.Add(new ListItem(
                                              xmlCyberware.SelectSingleNodeAndCacheExpression("id", token: token)?.Value,
                                              xmlCyberware.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                              ?? xmlCyberware.SelectSingleNodeAndCacheExpression("name", token: token)?.Value));
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
                                                                             "String_RestrictedItemsHidden",
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
                Program.ShowScrollableMessageBox(this,
                                                 await LanguageManager.GetStringAsync("Message_BannedGrade", token: token).ConfigureAwait(false),
                                                 await LanguageManager.GetStringAsync("MessageTitle_BannedGrade", token: token).ConfigureAwait(false),
                                                 MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XPathNavigator objCyberwareNode = _xmlBaseCyberwareDataNode.TryGetNodeByNameOrId(_strNodeXPath, strSelectedId);
            if (objCyberwareNode == null)
                return;

            if (_objCharacter.Settings.EnforceCapacity && _objParentObject != null)
            {
                // Capacity.
                bool blnAddToParentCapacity = objCyberwareNode.SelectSingleNodeAndCacheExpression("addtoparentcapacity", token: token) != null;
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objCyberwareNode.SelectSingleNodeAndCacheExpression("capacity", token: token)?.Value;
                if (strCapacity?.Contains('[') == true)
                {
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strCapacity = strValues[Math.Max(Math.Min(nudRating.ValueAsInt, strValues.Length) - 1, 0)];
                    }

                    decimal decCapacity = 0;

                    if (strCapacity != "*")
                    {
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token: token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            decCapacity = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                    }
                    
                    decimal decMaximumCapacityUsed = blnAddToParentCapacity ? (_objParentObject as Cyberware)?.Parent?.CapacityRemaining ?? decimal.MaxValue : MaximumCapacity;

                    if (decMaximumCapacityUsed - decCapacity < 0)
                    {
                        Program.ShowScrollableMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_OverCapacityLimit", token: token).ConfigureAwait(false),
                                                                             decMaximumCapacityUsed.ToString("#,0.##", GlobalSettings.CultureInfo),
                                                                             decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo)),
                                                         await LanguageManager.GetStringAsync("MessageTitle_OverCapacityLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            if (!Upgrading && ParentVehicle == null && !await objCyberwareNode.RequirementsMetAsync(_objCharacter, null, await LanguageManager.GetStringAsync(_eMode == Mode.Cyberware ? "String_SelectPACKSKit_Cyberware" : "String_SelectPACKSKit_Bioware", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false))
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
                : objCyberwareNode.SelectSingleNodeAndCacheExpression("category", token: token)?.Value;
            _sStrSelectGrade = SelectedGrade?.SourceIDString;
            SelectedCyberware = strSelectedId;
            SelectedRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            BlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            Markup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
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
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGrade))
                {
                    foreach (Grade objWareGrade in _lstGrades.Where(objWareGrade =>
                                 objWareGrade.SourceIDString != _strNoneGradeId ||
                                 (!string.IsNullOrEmpty(strForceGrade) && strForceGrade == _strNoneGradeId)))
                    {
                        if (string.IsNullOrEmpty(strForceGrade))
                        {
                            if (_setDisallowedGrades.Contains(objWareGrade.Name))
                                continue;
                            if (objWareGrade.Name.ContainsAny((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                        WindowMode == Mode.Bioware
                                            ? Improvement.ImprovementType.DisableBiowareGrade
                                            : Improvement.ImprovementType.DisableCyberwareGrade, token: token)
                                    .ConfigureAwait(false)).Select(x => x.ImprovedName)))
                            {
                                continue;
                            }

                            if (_objCharacter.AdapsinEnabled && WindowMode == Mode.Cyberware)
                            {
                                if (!objWareGrade.Adapsin
                                    && objWareGrade.Name.ContainsAny(
                                        _lstGrades.Where(x => x.Adapsin).Select(x => x.Name)))
                                    continue;
                            }
                            else if (objWareGrade.Adapsin)
                                continue;

                            if (_objCharacter.BurnoutEnabled)
                            {
                                if (!objWareGrade.Burnout
                                    && objWareGrade.Name.ContainsAny(
                                        _lstGrades.Where(x => x.Burnout).Select(x => x.Name)))
                                    continue;
                            }
                            else if (objWareGrade.Burnout)
                                continue;

                            if (blnHideBannedGrades
                                && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                                && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                                && objWareGrade.Name.ContainsAny(_objCharacter.Settings.BannedWareGrades))
                                continue;
                        }

                        if (!await (await objWareGrade.GetNodeXPathAsync(token: token).ConfigureAwait(false)).RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                        {
                            continue;
                        }

                        if (blnHideBannedGrades
                            && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                            && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                            && objWareGrade.Name.ContainsAny(_objCharacter.Settings.BannedWareGrades))
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceIDString,
                                '*' + await objWareGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                        }
                        else
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceIDString, await objWareGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                        }
                    }

                    string strOldSelected = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    bool blnDoSkipRefresh = strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId
                                                                             || lstGrade.Any(
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
                            else if (x.SelectedIndex <= 0 && !string.IsNullOrWhiteSpace(strOldSelected))
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
                string strSubsystem = "categories/category[. = " + _strSubsystems.CleanXPath().Replace(",", "\" or . = \"") + ']';
                objXmlCategoryList = _xmlBaseCyberwareDataNode.Select(strSubsystem);
            }
            else
            {
                objXmlCategoryList = _xmlBaseCyberwareDataNode.SelectAndCacheExpression("categories/category", token: token);
            }

            string strOldSelectedCyberware = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
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

        #endregion Methods
    }
}
