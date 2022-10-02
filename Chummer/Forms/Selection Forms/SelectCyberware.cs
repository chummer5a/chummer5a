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
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
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
        private bool _blnLoading = true;

        private readonly Mode _eMode = Mode.Cyberware;
        private readonly string _strNodeXPath = "cyberwares/cyberware";
        private static string s_strSelectCategory = string.Empty;
        private static string _sStrSelectGrade = string.Empty;
        private string _strSelectedCategory = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private readonly HashSet<string> _setDisallowedGrades = Utils.StringHashSetPool.Get();
        private string _strForceGrade = string.Empty;
        private readonly object _objParentObject;
        private readonly XPathNavigator _objParentNode;
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private readonly XPathNavigator _xmlBaseCyberwareDataNode;

        private enum Mode
        {
            Cyberware = 0,
            Bioware
        }

        #region Control Events

        public SelectCyberware(Character objCharacter, Improvement.ImprovementSource objWareSource, object objParentNode = null)
        {
            Disposed += (sender, args) =>
            {
                Utils.StringHashSetPool.Return(_setBlackMarketMaps);
                Utils.StringHashSetPool.Return(_setDisallowedGrades);
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

            await chkPrototypeTranshuman.DoThreadSafeAsync(x => x.Visible = _objCharacter.IsPrototypeTranshuman && _eMode == Mode.Bioware && !_objCharacter.Created);

            await PopulateCategories();

            await cboCategory.DoThreadSafeAsync(x =>
            {
                // Select the first Category in the list.
                if (!string.IsNullOrEmpty(s_strSelectCategory))
                    x.SelectedValue = s_strSelectCategory;
                if (x.SelectedIndex == -1 && x.Items.Count > 0)
                    x.SelectedIndex = 0;
                _strSelectedCategory = x.SelectedValue?.ToString();
            });

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            await PopulateGrades(null, true, _objForcedGrade?.SourceIDString ?? string.Empty, await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked));

            await cboGrade.DoThreadSafeAsync(x =>
            {
                if (_objForcedGrade != null)
                    x.SelectedValue = _objForcedGrade.SourceIDString;
                else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                    x.SelectedValue = _sStrSelectGrade;
                if (x.SelectedIndex == -1 && x.Items.Count > 0)
                    x.SelectedIndex = 0;
            });

            // Retrieve the information for the selected Grade.
            string strSelectedGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (!string.IsNullOrEmpty(strSelectedGrade))
            {
                XPathNavigator xmlGrade = _xmlBaseCyberwareDataNode.SelectSingleNode("grades/grade[id = " + strSelectedGrade.CleanXPath() + ']');

                // Update the Essence and Cost multipliers based on the Grade that has been selected.
                if (xmlGrade != null)
                {
                    _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                    _decESSMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("ess")?.Value, GlobalSettings.InvariantCultureInfo);
                    _intAvailModifier = xmlGrade.SelectSingleNode("avail")?.ValueAsInt ?? 0;
                }
            }

            await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts);
            await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts);
            await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts);

            _blnLoading = false;
            await RefreshList(_strSelectedCategory);
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
                xmlGrade = _xmlBaseCyberwareDataNode.SelectSingleNode("grades/grade[id = " + strSelectedGrade.CleanXPath() + ']');

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
            if (xmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                _decESSMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("ess")?.Value, GlobalSettings.InvariantCultureInfo);
                _intAvailModifier = xmlGrade.SelectSingleNode("avail")?.ValueAsInt ?? 0;

                await PopulateCategories(token);
                _blnLoading = false;
                await RefreshList(_strSelectedCategory, token);
                await DoRefreshSelectedCyberware(token);
            }
            else
            {
                _blnLoading = false;
                await UpdateCyberwareInfo(token);
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

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            _strSelectedCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            string strForceGrade = string.Empty;
            // Update the list of Cyberware based on the selected Category.
            cboGrade.Enabled = !_blnLockGrade;
            if (_blnLockGrade)
                strForceGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            // We will need to rebuild the Grade list since certain categories of 'ware disallow certain grades (e.g. Used for cultured bioware) and ForceGrades can change.
            await PopulateGrades(null, false, strForceGrade, await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked));
            _blnLoading = false;
            await RefreshList(_strSelectedCategory);
        }

        private async void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            await DoRefreshSelectedCyberware();
        }

        private async ValueTask DoRefreshSelectedCyberware(CancellationToken token = default)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            XPathNavigator xmlCyberware = null;
            string strSelectedId = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                xmlCyberware = _xmlBaseCyberwareDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            }
            string strForceGrade;
            if (xmlCyberware != null)
            {
                strForceGrade = xmlCyberware.SelectSingleNode("forcegrade")?.Value;
                // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
                XPathNavigator xmlRatingNode = xmlCyberware.SelectSingleNode("rating");
                if (xmlRatingNode != null)
                {
                    string strMinRating = xmlCyberware.SelectSingleNode("minrating")?.Value;
                    int intMinRating = 1;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                    {
                        strMinRating = await strMinRating.CheapReplaceAsync("MaximumSTR",
                                async () => (ParentVehicle != null
                                    ? Math.Max(1, await ParentVehicle.GetTotalBodyAsync(token) * 2)
                                    : await _objCharacter.STR.GetTotalMaximumAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("MaximumAGI",
                                async () => (ParentVehicle != null
                                    ? Math.Max(1, await ParentVehicle.GetPilotAsync(token) * 2)
                                    : await _objCharacter.AGI.GetTotalMaximumAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("MinimumSTR",
                                async () => (ParentVehicle != null ? await ParentVehicle.GetTotalBodyAsync(token) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("MinimumAGI",
                                async () => (ParentVehicle != null ? await ParentVehicle.GetPilotAsync(token) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token);

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating, token);
                        intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    await nudRating.DoThreadSafeAsync(x => x.Minimum = intMinRating, token: token);

                    string strMaxRating = xmlRatingNode.Value;
                    int intMaxRating = 0;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                    {
                        strMaxRating = await strMaxRating.CheapReplaceAsync("MaximumSTR",
                                async () => (ParentVehicle != null
                                    ? Math.Max(1, await ParentVehicle.GetTotalBodyAsync(token) * 2)
                                    : await _objCharacter.STR.GetTotalMaximumAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("MaximumAGI",
                                async () => (ParentVehicle != null
                                    ? Math.Max(1, await ParentVehicle.GetPilotAsync(token) * 2)
                                    : await _objCharacter.AGI.GetTotalMaximumAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("MinimumSTR",
                                async () => (ParentVehicle != null ? await ParentVehicle.GetTotalBodyAsync(token) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("MinimumAGI",
                                async () => (ParentVehicle != null ? await ParentVehicle.GetPilotAsync(token) : 3).ToString(GlobalSettings.InvariantCultureInfo), token: token);

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strMaxRating, token);
                        intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    if (chkHideOverAvailLimit.Checked)
                    {
                        int intAvailModifier = strForceGrade == "None" ? 0 : _intAvailModifier;
                        while (intMaxRating > intMinRating
                               && !await xmlCyberware.CheckAvailRestrictionAsync(
                                   _objCharacter, intMaxRating, intAvailModifier, token: token))
                        {
                            --intMaxRating;
                        }
                    }
                    if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        decimal decCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m;
                        decCostMultiplier *= _decCostMultiplier;
                        if (await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                            decCostMultiplier *= 0.9m;
                        while (intMaxRating > intMinRating && !await xmlCyberware.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier, intMaxRating, token))
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
                    }, token: token);
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                    await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                }
                else
                {
                    await lblRatingLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    await nudRating.DoThreadSafeAsync(x =>
                    {
                        x.Minimum = 0;
                        x.Value = 0;
                        x.Visible = false;
                    }, token: token);
                }

                string strRatingLabel = xmlCyberware.SelectSingleNode("ratinglabel")?.Value;
                strRatingLabel = !string.IsNullOrEmpty(strRatingLabel)
                    ? string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Label_RatingFormat", token: token),
                                    await LanguageManager.GetStringAsync(strRatingLabel, token: token))
                    : await LanguageManager.GetStringAsync("Label_Rating", token: token);
                await lblRatingLabel.DoThreadSafeAsync(x => x.Text = strRatingLabel, token: token);

                string strSource = xmlCyberware.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
                string strPage = (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token))?.Value ?? xmlCyberware.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
                SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter, token: token);
                await objSource.SetControlAsync(lblSource, token: token);
                await lblSource.DoThreadSafeFuncAsync(x => x.Text, token: token)
                               .ContinueWith(
                                   y => lblSourceLabel.DoThreadSafeAsync(
                                       x => x.Visible = !string.IsNullOrEmpty(y.Result), token: token), token).Unwrap();
                
                if (!string.IsNullOrEmpty(strForceGrade))
                {
                    // Force the Cyberware to be a particular Grade.
                    await cboGrade.DoThreadSafeAsync(x =>
                    {
                        if (x.Enabled)
                            x.Enabled = false;
                    }, token: token);
                    Grade objForcedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
                    strForceGrade = objForcedGrade?.SourceIDString;
                }
                else
                {
                    await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade, token: token);
                    if (_blnLockGrade)
                    {
                        strForceGrade = _objForcedGrade?.SourceIDString ?? await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                    }
                }

                bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlCyberware.SelectSingleNode("category")?.Value);
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
                }, token: token);

                // We will need to rebuild the Grade list since certain categories of 'ware disallow certain grades (e.g. Used for cultured bioware) and ForceGrades can change.
                HashSet<string> setDisallowedGrades = null;
                if (xmlCyberware.SelectSingleNode("bannedgrades") != null)
                {
                    setDisallowedGrades = new HashSet<string>();
                    foreach (XPathNavigator objNode in xmlCyberware.Select("bannedgrades/grade"))
                    {
                        setDisallowedGrades.Add(objNode.Value);
                    }
                }
                await PopulateGrades(setDisallowedGrades, false, strForceGrade,
                                     await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked, token: token), token);

                string strNotes = xmlCyberware.SelectSingleNode("altnotes")?.Value ?? xmlCyberware.SelectSingleNode("notes")?.Value;
                if (!string.IsNullOrEmpty(strNotes))
                {
                    await lblCyberwareNotesLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    await lblCyberwareNotes.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Text = strNotes;
                    }, token: token);
                }
                else
                {
                    await lblCyberwareNotes.DoThreadSafeAsync(x => x.Visible = false, token: token);
                    await lblCyberwareNotesLabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                }
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token);
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token);
                await cboGrade.DoThreadSafeAsync(x => x.Enabled = !_blnLockGrade, token: token);
                strForceGrade = _blnLockGrade
                    ? _objForcedGrade?.SourceIDString
                      ?? await cboGrade.DoThreadSafeFuncAsync(
                          x => x.SelectedValue?.ToString(), token: token)
                    : string.Empty;
                await PopulateGrades(null, false, strForceGrade, await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked, token), token: token);
                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Checked = false, token: token);
            }
            _blnLoading = false;
            await UpdateCyberwareInfo(token);
        }

        private async void ProcessCyberwareInfoChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            await UpdateCyberwareInfo();
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList(_strSelectedCategory);
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFree.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await RefreshList(_strSelectedCategory);
            }
            await UpdateCyberwareInfo();
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
            bool blnHideBannedGrades = await chkHideBannedGrades.DoThreadSafeFuncAsync(x => x.Checked);
            _lstGrades.Clear();
            _lstGrades.AddRange(await _objCharacter.GetGradesListAsync(
                                    _eMode == Mode.Bioware
                                        ? Improvement.ImprovementSource.Bioware
                                        : Improvement.ImprovementSource.Cyberware,
                                    blnHideBannedGrades));
            await PopulateGrades(blnHideBannedGrades: blnHideBannedGrades);
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            await AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList(_strSelectedCategory);
        }

        private async void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await RefreshList(_strSelectedCategory);
            }
            await UpdateCyberwareInfo();
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
                txtSearch.Select(txtSearch.Text.Length, 0);
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
        public Vehicle ParentVehicle { set; get; }

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
        private async ValueTask UpdateCyberwareInfo(CancellationToken token = default)
        {
            XPathNavigator objXmlCyberware = null;
            string strSelectedId = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                objXmlCyberware = _xmlBaseCyberwareDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            }
            if (objXmlCyberware == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token);
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token);
            try
            {
                await tlpRight.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.SuspendLayout();
                }, token: token);
                try
                {
                    string strSelectCategory = objXmlCyberware.SelectSingleNode("category")?.Value ?? string.Empty;
                    bool blnForceNoESSModifier = objXmlCyberware.SelectSingleNode("forcegrade")?.Value == "None";
                    bool blnIsGeneware = objXmlCyberware.SelectSingleNode("isgeneware") != null && objXmlCyberware.SelectSingleNode("isgeneware")?.Value != bool.FalseString;

                    // Place the Genetech cost multiplier in a variable that can be safely modified.
                    decimal decGenetechCostModifier = 1;
                    // Genetech cost modifier only applies to Genetech.
                    if (blnIsGeneware)
                        decGenetechCostModifier = GenetechCostMultiplier;

                    // Extract the Avail and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
                    // This is done using XPathExpression.

                    int intRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token);
                    AvailabilityValue objTotalAvail = new AvailabilityValue(
                        intRating, objXmlCyberware.SelectSingleNode("avail")?.Value, _intAvailModifier);
                    await lblAvailLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    await lblAvail.DoThreadSafeAsync(x => x.Text = objTotalAvail.ToString(), token: token);

                    // Cost.
                    decimal decItemCost = 0;
                    if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        await lblCost.DoThreadSafeAsync(
                            x => x.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat,
                                                          GlobalSettings.CultureInfo)
                                          + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                    }
                    else
                    {
                        string strCost = objXmlCyberware.SelectSingleNode("cost")?.Value;
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
                                    decMin = Convert.ToDecimal(strCost.FastEscape('+'),
                                                               GlobalSettings.InvariantCultureInfo);

                                await lblCost.DoThreadSafeAsync(x => x.Text = decMax == decimal.MaxValue
                                                                    ? decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                          GlobalSettings.CultureInfo)
                                                                      + LanguageManager.GetString("String_NuyenSymbol") + '+'
                                                                    : decMin.ToString(_objCharacter.Settings.NuyenFormat,
                                                                          GlobalSettings.CultureInfo)
                                                                      + " - " + decMax.ToString(_objCharacter.Settings.NuyenFormat,
                                                                          GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);

                                decItemCost = decMin;
                            }
                            else
                            {
                                strCost = await strCost
                                                .CheapReplaceAsync("Parent Cost", () => CyberwareParent?.Cost ?? "0", token: token)
                                                .CheapReplaceAsync("Parent Gear Cost",
                                                                   async () => CyberwareParent != null
                                                                       ? (await CyberwareParent.GearChildren
                                                                           .SumParallelAsync(x => x.TotalCost, token: token))
                                                                       .ToString(GlobalSettings.InvariantCultureInfo)
                                                                       : "0", token: token)
                                                .CheapReplaceAsync("MinRating",
                                                                   () => nudRating.Minimum.ToString(
                                                                       GlobalSettings.InvariantCultureInfo), token: token)
                                                .CheapReplaceAsync("Rating",
                                                                   () => nudRating.Value.ToString(
                                                                       GlobalSettings.InvariantCultureInfo), token: token);

                                (bool blnIsSuccess, object objProcess)
                                    = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token);
                                if (blnIsSuccess)
                                {
                                    decItemCost = (Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo)
                                                   * _decCostMultiplier * decGenetechCostModifier);
                                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                                    if (chkBlackMarketDiscount.Checked)
                                    {
                                        decItemCost *= 0.9m;
                                    }

                                    await lblCost.DoThreadSafeAsync(x => x.Text
                                                                        = decItemCost.ToString(_objCharacter.Settings.NuyenFormat,
                                                                            GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                                }
                                else
                                {
                                    await lblCost.DoThreadSafeAsync(x => x.Text = strCost + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                                }
                            }
                        }
                        else
                            await lblCost.DoThreadSafeAsync(x => x.Text
                                                                = (0.0m).ToString(_objCharacter.Settings.NuyenFormat,
                                                                      GlobalSettings.CultureInfo)
                                                                  + LanguageManager.GetString("String_NuyenSymbol"), token: token);
                    }

                    await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                 .ContinueWith(
                                     y => lblCostLabel.DoThreadSafeAsync(
                                         x => x.Visible = !string.IsNullOrEmpty(y.Result), token: token), token).Unwrap();

                    // Test required to find the item.
                    string strTest = await _objCharacter.AvailTestAsync(decItemCost, objTotalAvail, token);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token);

                    // Essence.
                    await lblESSDiscountLabel.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts, token: token);
                    await lblESSDiscountPercentLabel.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts, token: token);
                    await nudESSDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts, token: token);

                    bool blnAddToParentESS = objXmlCyberware.SelectSingleNode("addtoparentess") != null;
                    if (_objParentNode == null || blnAddToParentESS)
                    {
                        decimal decESS = 0;
                        if (!await chkPrototypeTranshuman.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            // Place the Essence cost multiplier in a variable that can be safely modified.
                            decimal decCharacterESSModifier = 1.0m;

                            if (!blnForceNoESSModifier)
                            {
                                decCharacterESSModifier = CharacterESSMultiplier;
                                // If Basic Bioware is selected, apply the Basic Bioware ESS Multiplier.
                                if (strSelectCategory == "Basic" && _eMode == Mode.Bioware)
                                    decCharacterESSModifier -= (1 - BasicBiowareESSMultiplier);
                                if (blnIsGeneware)
                                    decCharacterESSModifier -= (1 - GenetechEssMultiplier);

                                if (_objCharacter.Settings.AllowCyberwareESSDiscounts)
                                {
                                    decimal decDiscountModifier = await nudESSDiscount.DoThreadSafeFuncAsync(x => x.Value, token: token) / 100.0m;
                                    decCharacterESSModifier *= (1.0m - decDiscountModifier);
                                }

                                decCharacterESSModifier -= (1 - _decESSMultiplier);

                                decCharacterESSModifier *= CharacterTotalESSMultiplier;
                            }

                            string strEss = objXmlCyberware.SelectSingleNode("ess")?.Value ?? string.Empty;
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
                                strEss.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token);
                            if (blnIsSuccess)
                            {
                                decESS = decCharacterESSModifier
                                         * Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                                if (!_objCharacter.Settings.DontRoundEssenceInternally)
                                    decESS = decimal.Round(decESS, _objCharacter.Settings.EssenceDecimals,
                                                           MidpointRounding.AwayFromZero);
                            }
                        }

                        await lblEssence.DoThreadSafeAsync(x =>
                        {
                            x.Text = decESS.ToString(_objCharacter.Settings.EssenceFormat, GlobalSettings.CultureInfo);
                            if (blnAddToParentESS)
                                x.Text = '+' + x.Text;
                        }, token: token);
                    }
                    else
                        await lblEssence.DoThreadSafeAsync(x => x.Text
                                                               = (0.0m).ToString(_objCharacter.Settings.EssenceFormat,
                                                                   GlobalSettings.CultureInfo), token: token);

                    await lblEssence.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                    .ContinueWith(
                                        y => lblEssenceLabel.DoThreadSafeAsync(
                                            x => x.Visible = !string.IsNullOrEmpty(y.Result), token: token), token).Unwrap();

                    // Capacity.
                    bool blnAddToParentCapacity = objXmlCyberware.SelectSingleNode("addtoparentcapacity") != null;
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    string strCapacity = objXmlCyberware.SelectSingleNode("capacity")?.Value ?? string.Empty;
                    bool blnSquareBrackets = strCapacity.StartsWith('[');
                    if (string.IsNullOrEmpty(strCapacity))
                    {
                        await lblCapacity.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo), token: token);
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
                            await lblCapacity.DoThreadSafeAsync(x => x.Text = "*", token: token);
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
                                        "Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token);

                                await lblCapacity.DoThreadSafeAsync(x =>
                                {
                                    x.Text = blnIsSuccess
                                        ? objProcess.ToString()
                                        : strFirstHalf;
                                    if (blnSquareBrackets)
                                        x.Text = '[' + x.Text + ']';
                                }, token: token);

                                strSecondHalf = strSecondHalf.Trim('[', ']');
                                (blnIsSuccess, objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                    strSecondHalf.Replace(
                                        "Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token);
                                strSecondHalf = (blnAddToParentCapacity ? "+[" : "[")
                                                + (blnIsSuccess ? objProcess.ToString() : strSecondHalf) + ']';

                                await lblCapacity.DoThreadSafeAsync(x => x.Text += '/' + strSecondHalf, token: token);
                            }
                            else
                            {
                                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                                    strCapacity.Replace(
                                        "Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token);
                                await lblCapacity.DoThreadSafeAsync(x =>
                                {
                                    x.Text = blnIsSuccess
                                        ? objProcess.ToString()
                                        : strCapacity;
                                    if (blnSquareBrackets)
                                        x.Text = blnAddToParentCapacity
                                            ? "+[" + x.Text + ']'
                                            : '[' + x.Text + ']';
                                }, token: token);
                            }
                        }
                    }

                    await lblCapacity.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                     .ContinueWith(
                                         y => lblCapacityLabel.DoThreadSafeAsync(
                                             x => x.Visible = !string.IsNullOrEmpty(y.Result), token: token), token).Unwrap();
                }
                finally
                {
                    await tlpRight.DoThreadSafeAsync(x => x.ResumeLayout(), token: token);
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout(), token: token);
            }
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
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    await lstCyberware.PopulateWithListItemsAsync(ListItem.Blank.Yield(), token: token);
                }
                return false;
            }

            string strCurrentGradeId = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId)
                ? null
                : _lstGrades.Find(x => x.SourceIDString == strCurrentGradeId);

            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token)).Append(')');
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdCategoryFilter))
                {
                    if (strCategory != "Show All" && !Upgrading
                                                  && (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0))
                        sbdCategoryFilter.Append("category = ").Append(strCategory.CleanXPath())
                                         .Append(" or category = \"None\"");
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
                else if (ParentVehicle == null && ((!_objCharacter.AddCyberwareEnabled && _eMode == Mode.Cyberware) || (!_objCharacter.AddBiowareEnabled && _eMode == Mode.Bioware)))
                    sbdFilter.Append(" and (id = ").Append(Cyberware.EssenceHoleGUID.ToString().CleanXPath())
                             .Append(" or id = ").Append(Cyberware.EssenceAntiHoleGUID.ToString().CleanXPath())
                             .Append(" or mountsto)");
                else
                    sbdFilter.Append(" and not(requireparent)");
                if (objCurrentGrade != null)
                {
                    string strGradeNameCleaned = objCurrentGrade.Name.CleanXPath();
                    sbdFilter.Append(" and (not(forcegrade) or forcegrade = \"None\" or forcegrade = ")
                             .Append(strGradeNameCleaned).Append(") and (not(bannedgrades[grade = ")
                             .Append(strGradeNameCleaned).Append("]))");
                }

                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token);
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
                    bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                    bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                    bool blnFree = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token);
                    decimal decMarkup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token);
                    foreach (XPathNavigator xmlCyberware in xmlIterator)
                    {
                        bool blnIsForceGrade = await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("forcegrade", token: token) != null;
                        if (objCurrentGrade != null && blnIsForceGrade)
                        {
                            if (WindowMode == Mode.Bioware)
                            {
                                if ((await ImprovementManager
                                        .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                            Improvement.ImprovementType.DisableBiowareGrade, token: token))
                                    .Any(x => objCurrentGrade.Name.Contains(x.ImprovedName)))
                                    continue;
                            }
                            else if ((await ImprovementManager
                                         .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                             Improvement.ImprovementType
                                                        .DisableCyberwareGrade, token: token))
                                     .Any(x => objCurrentGrade.Name.Contains(x.ImprovedName)))
                                continue;
                        }

                        if (blnCyberwareDisabled
                            && await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("subsystems/cyberware", token: token) != null
                            && await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("mountsto", token: token) == null)
                        {
                            continue;
                        }

                        if (blnBiowareDisabled
                            && await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("subsystems/bioware", token: token) != null
                            && await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("mountsto", token: token) == null)
                        {
                            continue;
                        }

                        XPathNavigator xmlTestNode
                            = await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("forbidden/parentdetails", token: token);
                        if (xmlTestNode != null && await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("required/parentdetails", token: token);
                        if (xmlTestNode != null && !await _objParentNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token: token))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        if (!string.IsNullOrEmpty(_strHasModularMounts))
                        {
                            string strBlocksMounts
                                = (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("blocksmounts", token: token))?.Value;
                            if (!string.IsNullOrEmpty(strBlocksMounts))
                            {
                                ICollection<Cyberware> lstWareListToCheck = null;
                                if (CyberwareParent != null)
                                    lstWareListToCheck = CyberwareParent.Children;
                                else if (ParentVehicle == null)
                                    lstWareListToCheck = _objCharacter.Cyberware;
                                if (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("selectside", token: token) == null
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
                                = (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("modularmount", token: token))?.Value;
                            if (!string.IsNullOrEmpty(strLoopMount) && _strDisallowedMounts
                                                                       .SplitNoAlloc(
                                                                           ',', StringSplitOptions.RemoveEmptyEntries)
                                                                       .Contains(strLoopMount))
                                continue;
                        }

                        string strMaxRating = (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("rating", token: token))?.Value;
                        string strMinRating = (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("minrating", token: token))?.Value;
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
                                                                                 token) * 2)
                                                                         : await _objCharacter.STR.GetTotalMaximumAsync(
                                                                             token))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MaximumAGI",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetPilotAsync(token)
                                                                             * 2)
                                                                         : await _objCharacter.AGI.GetTotalMaximumAsync(
                                                                             token))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MinimumSTR",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetTotalBodyAsync(token)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo), token: token)
                                                             .CheapReplaceAsync("MinimumAGI",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetPilotAsync(token)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo),
                                                                 token: token);

                            (bool blnIsSuccess, object objProcess)
                                = await CommonFunctions.EvaluateInvariantXPathAsync(strMinRating, token);
                            intMinRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;

                            strMaxRating = await strMaxRating.CheapReplaceAsync("MaximumSTR",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetTotalBodyAsync(
                                                                                 token) * 2)
                                                                         : await _objCharacter.STR.GetTotalMaximumAsync(
                                                                             token))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MaximumAGI",
                                                                 async () => (ParentVehicle != null
                                                                         ? Math.Max(
                                                                             1,
                                                                             await ParentVehicle.GetPilotAsync(token)
                                                                             * 2)
                                                                         : await _objCharacter.AGI.GetTotalMaximumAsync(
                                                                             token))
                                                                     .ToString(GlobalSettings.InvariantCultureInfo),
                                                                 token: token)
                                                             .CheapReplaceAsync("MinimumSTR",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetTotalBodyAsync(token)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo), token: token)
                                                             .CheapReplaceAsync("MinimumAGI",
                                                                 async () => (ParentVehicle != null
                                                                     ? await ParentVehicle.GetPilotAsync(token)
                                                                     : 3).ToString(
                                                                     GlobalSettings.InvariantCultureInfo),
                                                                 token: token);

                            (blnIsSuccess, objProcess)
                                = await CommonFunctions.EvaluateInvariantXPathAsync(strMaxRating, token);
                            intMaxRating = blnIsSuccess ? ((double) objProcess).StandardRound() : 1;
                            if (intMaxRating < intMinRating)
                                continue;
                        }

                        // Ex-Cons cannot have forbidden or restricted 'ware
                        if (_objCharacter.ExCon && ParentVehicle == null
                                                && await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("mountsto", token: token) == null)
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
                                string strAvailExpr = (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("avail", token: token))?.Value
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

                        if (!Upgrading && ParentVehicle == null && !xmlCyberware.RequirementsMet(_objCharacter))
                            continue;

                        if (!blnDoUIUpdate)
                        {
                            return true;
                        }

                        if (blnHideOverAvailLimit && !await xmlCyberware.CheckAvailRestrictionAsync(_objCharacter, intMinRating,
                                blnIsForceGrade ? 0 : _intAvailModifier, token))
                        {
                            ++intOverLimit;
                            continue;
                        }

                        if (blnShowOnlyAffordItems && !blnFree)
                        {
                            decimal decCostMultiplier = 1 + (decMarkup / 100.0m);
                            if (_setBlackMarketMaps.Contains(
                                    (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("category", token: token))?.Value))
                                decCostMultiplier *= 0.9m;
                            if (!await xmlCyberware.CheckNuyenRestrictionAsync(_objCharacter.Nuyen, decCostMultiplier, token: token))
                            {
                                ++intOverLimit;
                                continue;
                            }
                        }

                        lstCyberwares.Add(new ListItem((await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("id", token: token))?.Value,
                                                       (await xmlCyberware.SelectSingleNodeAndCacheExpressionAsync("translate", token: token))
                                                                   ?.Value ?? (await xmlCyberware
                                                           .SelectSingleNodeAndCacheExpressionAsync(
                                                               "name", token: token))?.Value));
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
                                                                         "String_RestrictedItemsHidden", token: token),
                                                                     intOverLimit)));
                    }

                    string strOldSelected = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                    _blnLoading = true;
                    await lstCyberware.PopulateWithListItemsAsync(lstCyberwares, token: token);
                    _blnLoading = false;
                    await lstCyberware.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        else
                            x.SelectedIndex = -1;
                    }, token: token);
                }

                return lstCyberwares?.Count > 0;
            }
            finally
            {
                if (lstCyberwares != null)
                    Utils.ListItemListPool.Return(lstCyberwares);
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
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strSelectedId = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if ((await cboGrade.DoThreadSafeFuncAsync(x => x.Text, token: token)).StartsWith('*'))
            {
                Program.ShowMessageBox(this,
                    await LanguageManager.GetStringAsync("Message_BannedGrade", token: token),
                    await LanguageManager.GetStringAsync("MessageTitle_BannedGrade", token: token),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XPathNavigator objCyberwareNode = _xmlBaseCyberwareDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + ']');
            if (objCyberwareNode == null)
                return;

            if (_objCharacter.Settings.EnforceCapacity && _objParentObject != null)
            {
                // Capacity.
                bool blnAddToParentCapacity = await objCyberwareNode.SelectSingleNodeAndCacheExpressionAsync("addtoparentcapacity", token: token) != null;
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = (await objCyberwareNode.SelectSingleNodeAndCacheExpressionAsync("capacity", token: token))?.Value;
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
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), token: token);
                        if (blnIsSuccess)
                            decCapacity = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                    }

                    decimal decMaximumCapacityUsed = blnAddToParentCapacity ? (_objParentObject as Cyberware)?.Parent?.CapacityRemaining ?? decimal.MaxValue : MaximumCapacity;

                    if (decMaximumCapacityUsed - decCapacity < 0)
                    {
                        Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_OverCapacityLimit", token: token)
                                , decMaximumCapacityUsed.ToString("#,0.##", GlobalSettings.CultureInfo)
                                , decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo)),
                            await LanguageManager.GetStringAsync("MessageTitle_OverCapacityLimit", token: token), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            if (!Upgrading && ParentVehicle == null && !objCyberwareNode.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync(_eMode == Mode.Cyberware ? "String_SelectPACKSKit_Cyberware" : "String_SelectPACKSKit_Bioware", token: token)))
                return;

            string strForceGrade = (await objCyberwareNode.SelectSingleNodeAndCacheExpressionAsync("forcegrade", token: token))?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.Find(x => x.SourceIDString == strForceGrade);
                else
                    return;
            }

            s_strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token) == 0)
                ? _strSelectedCategory
                : (await objCyberwareNode.SelectSingleNodeAndCacheExpressionAsync("category", token: token))?.Value;
            _sStrSelectGrade = SelectedGrade?.SourceIDString;
            SelectedCyberware = strSelectedId;
            SelectedRating = await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token);
            BlackMarketDiscount = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x => x.Checked, token: token);
            Markup = await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token);
            await nudESSDiscount.DoThreadSafeAsync(x =>
            {
                if (x.Visible)
                    SelectedESSDiscount = x.ValueAsInt;
            }, token: token);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token);
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
        private async ValueTask PopulateGrades(ICollection<string> setDisallowedGrades = null, bool blnForce = false, string strForceGrade = "", bool blnHideBannedGrades = true, CancellationToken token = default)
        {
            if (_blnPopulatingGrades)
                return;
            _blnPopulatingGrades = true;
            if (setDisallowedGrades == null)
                setDisallowedGrades = Array.Empty<string>();
            if (blnForce || !_setDisallowedGrades.SetEquals(setDisallowedGrades) || _strForceGrade != strForceGrade || await cboGrade.DoThreadSafeFuncAsync(x => x.Items.Count, token: token) == 0)
            {
                _setDisallowedGrades.Clear();
                _setDisallowedGrades.AddRange(setDisallowedGrades);
                _strForceGrade = strForceGrade;
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGrade))
                {
                    foreach (Grade objWareGrade in _lstGrades)
                    {
                        if (objWareGrade.SourceIDString == _strNoneGradeId
                            && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != _strNoneGradeId))
                            continue;
                        if (string.IsNullOrEmpty(strForceGrade))
                        {
                            if (_setDisallowedGrades.Contains(objWareGrade.Name))
                                continue;
                            if (WindowMode == Mode.Bioware)
                            {
                                if ((await ImprovementManager
                                        .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                            Improvement.ImprovementType.DisableBiowareGrade, token: token))
                                    .Any(x => objWareGrade.Name.Contains(x.ImprovedName)))
                                    continue;
                                if (objWareGrade.Adapsin)
                                    continue;
                            }
                            else
                            {
                                if ((await ImprovementManager
                                        .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                            Improvement.ImprovementType
                                                       .DisableCyberwareGrade, token: token))
                                    .Any(x => objWareGrade.Name.Contains(x.ImprovedName)))
                                    continue;
                                if (_objCharacter.AdapsinEnabled)
                                {
                                    if (!objWareGrade.Adapsin && _lstGrades.Any(x => x.Adapsin && objWareGrade.Name.Contains(x.Name)))
                                        continue;
                                }
                                else if (objWareGrade.Adapsin)
                                    continue;
                            }

                            if (_objCharacter.BurnoutEnabled)
                            {
                                if (!objWareGrade.Burnout && _lstGrades.Any(x => x.Burnout && objWareGrade.Name.Contains(x.Name)))
                                    continue;
                            }
                            else if (objWareGrade.Burnout)
                                continue;

                            if (blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules &&
                                _objCharacter.Settings.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s)))
                                continue;
                        }

                        if (!(await objWareGrade.GetNodeXPathAsync(token: token)).RequirementsMet(_objCharacter))
                        {
                            continue;
                        }

                        if (!blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules
                            && _objCharacter.Settings.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s)))
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceIDString,
                                                      '*' + objWareGrade.CurrentDisplayName));
                        }
                        else
                        {
                            lstGrade.Add(new ListItem(objWareGrade.SourceIDString, objWareGrade.CurrentDisplayName));
                        }
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

        private bool _blnPopulatingCategories;

        private async ValueTask PopulateCategories(CancellationToken token = default)
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
                objXmlCategoryList = await _xmlBaseCyberwareDataNode.SelectAndCacheExpressionAsync("categories/category", token: token);
            }

            string strOldSelectedCyberware = await lstCyberware.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCategory))
            {
                foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
                {
                    // Make sure the category contains items that we can actually display
                    if (await AnyItemInList(objXmlCategory.Value, token: token))
                    {
                        string strInnerText = objXmlCategory.Value;
                        lstCategory.Add(new ListItem(strInnerText,
                                                     (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate", token: token))
                                                                   ?.Value ?? strInnerText));
                    }
                }

                lstCategory.Sort(CompareListItems.CompareNames);

                if (lstCategory.Count > 0)
                {
                    lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: token)));
                }

                string strOldSelected = _strSelectedCategory;
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                await cboCategory.PopulateWithListItemsAsync(lstCategory, token: token);
                _blnLoading = blnOldLoading;
                await cboCategory.DoThreadSafeAsync(x =>
                {
                    x.SelectedValue = strOldSelected;
                    if (x.SelectedIndex == -1 && lstCategory.Count > 0)
                        x.SelectedIndex = 0;
                }, token: token);
            }

            if (!string.IsNullOrEmpty(strOldSelectedCyberware))
                await lstCyberware.DoThreadSafeAsync(x => x.SelectedValue = strOldSelectedCyberware, token: token);

            _blnPopulatingCategories = false;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
