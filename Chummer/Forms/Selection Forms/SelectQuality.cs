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

namespace Chummer
{
    public partial class SelectQuality : Form
    {
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain;
        private bool _blnLoading = true;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XPathNavigator _xmlMetatypeQualityRestrictionNode;

        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();

        private static string _strSelectCategory = string.Empty;

        #region Control Events

        public SelectQuality(Character objCharacter)
        {
            Disposed += (sender, args) => Utils.ListItemListPool.Return(_lstCategory);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));

            // Load the Quality information.
            _xmlBaseQualityDataNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlMetatypeQualityRestrictionNode = _objCharacter.GetNodeXPath().SelectSingleNodeAndCacheExpression("qualityrestriction");
        }

        private async void SelectQuality_Load(object sender, EventArgs e)
        {
            // Populate the Quality Category list.
            foreach (XPathNavigator objXmlCategory in await _xmlBaseQualityDataNode.SelectAndCacheExpressionAsync("categories/category").ConfigureAwait(false))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate").ConfigureAwait(false))?.Value ?? strInnerText));
            }

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }
            
            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);
            // Select the first Category in the list.
            await cboCategory.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedIndex = 0;
                else
                {
                    x.SelectedValue = _strSelectCategory;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }

                x.Enabled = _lstCategory.Count > 1;
            }).ConfigureAwait(false);

            if (_objCharacter.MetagenicLimit == 0)
                await chkNotMetagenic.DoThreadSafeAsync(x => x.Checked = true).ConfigureAwait(false);

            string strKarma = await LanguageManager.GetStringAsync("Label_Karma").ConfigureAwait(false);
            await lblBPLabel.DoThreadSafeAsync(x => x.Text = strKarma).ConfigureAwait(false);
            _blnLoading = false;
            await BuildQualityList().ConfigureAwait(false);
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await BuildQualityList().ConfigureAwait(false);
        }

        private async void lstQualities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _blnLoading = true;

            try
            {
                XPathNavigator xmlQuality = null;
                string strSelectedQuality = await lstQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedQuality))
                {
                    xmlQuality = _xmlBaseQualityDataNode.SelectSingleNode(
                        "qualities/quality[id = " + strSelectedQuality.CleanXPath() + ']');
                }

                if (xmlQuality != null)
                {
                    await nudRating.DoThreadSafeAsync(x => x.ValueAsInt = x.MinimumAsInt).ConfigureAwait(false);
                    int intMaxRating = int.MaxValue;
                    if (xmlQuality.TryGetInt32FieldQuickly("limit", ref intMaxRating)
                        && await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("nolevels").ConfigureAwait(false) == null)
                    {
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.MaximumAsInt = intMaxRating;
                            x.Visible = true;
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblRatingNALabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                        await nudRating.DoThreadSafeAsync(x =>
                        {
                            x.MaximumAsInt = 1;
                            x.ValueAsInt = 1;
                            x.Visible = false;
                        }).ConfigureAwait(false);
                    }

                    await UpdateCostLabel(xmlQuality).ConfigureAwait(false);

                    string strSource = (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("source").ConfigureAwait(false))?.Value
                                       ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strPage = (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("altpage").ConfigureAwait(false))?.Value
                                     ?? (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("page").ConfigureAwait(false))?.Value
                                     ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    SourceString objSource = await SourceString.GetSourceStringAsync(
                        strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSource.SetControlAsync(lblSource).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(
                        x => x.Visible = !string.IsNullOrEmpty(objSource.ToString())).ConfigureAwait(false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }
            }
            finally
            {
                _blnLoading = false;
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            await BuildQualityList().ConfigureAwait(false);
        }

        private async void CostControl_Changed(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _blnLoading = true;

            XPathNavigator xmlQuality = null;
            string strSelectedQuality = await lstQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedQuality))
            {
                xmlQuality = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[id = " + strSelectedQuality.CleanXPath() + ']');
            }

            if (xmlQuality != null)
                await UpdateCostLabel(xmlQuality).ConfigureAwait(false);

            _blnLoading = false;
        }

        private async void chkMetagenic_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkMetagenic.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await chkNotMetagenic.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
            await BuildQualityList().ConfigureAwait(false);
        }

        private async void chkNotMetagenic_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkNotMetagenic.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                await chkMetagenic.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
            await BuildQualityList().ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildQualityList().ConfigureAwait(false);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstQualities.SelectedIndex + 1 < lstQualities.Items.Count:
                    ++lstQualities.SelectedIndex;
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
                    --lstQualities.SelectedIndex;
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

        private async void KarmaFilter(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _blnLoading = true;
            await nudMinimumBP.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Text))
                {
                    x.Value = 0;
                }
            }).ConfigureAwait(false);
            await nudValueBP.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Text))
                {
                    x.Value = 0;
                }
            }).ConfigureAwait(false);
            await nudMaximumBP.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Text))
                {
                    x.Value = 0;
                }
            }).ConfigureAwait(false);

            decimal decMaximumBP = await nudMaximumBP.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false);
            decimal decMinimumBP = await nudMinimumBP.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false);
            if (decMaximumBP < decMinimumBP)
            {
                if (sender == nudMaximumBP)
                    await nudMinimumBP.DoThreadSafeAsync(x => x.Value = decMaximumBP).ConfigureAwait(false);
                else
                    await nudMaximumBP.DoThreadSafeAsync(x => x.Value = decMinimumBP).ConfigureAwait(false);
            }

            _blnLoading = false;

            await BuildQualityList().ConfigureAwait(false);
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
                    try
                    {
                        cboCategory.SelectedValue = value;
                        cboCategory.Enabled = false;
                    }
                    finally
                    {
                        cboCategory.EndUpdate();
                    }
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

        private async ValueTask UpdateCostLabel(XPathNavigator xmlQuality, CancellationToken token = default)
        {
            if (xmlQuality != null)
            {
                if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    await lblBP.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                else
                {
                    string strKarma = (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("karma", token).ConfigureAwait(false))?.Value ?? string.Empty;
                    if (strKarma.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        int intMin;
                        int intMax = int.MaxValue;
                        string strCost = strKarma.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            int.TryParse(strValues[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                out intMin);
                            int.TryParse(strValues[1], NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                out intMax);
                        }
                        else
                            int.TryParse(strCost.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                out intMin);

                        string strBP = intMax == int.MaxValue
                            ? intMin.ToString(GlobalSettings.CultureInfo)
                            : string.Format(GlobalSettings.CultureInfo, "{0}{1}-{1}{2}", intMin,
                                            await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false), intMax);
                        await lblBP.DoThreadSafeAsync(x => x.Text = strBP, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        int.TryParse(strKarma, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intBP);

                        if ((await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("costdiscount", token).ConfigureAwait(false)).RequirementsMet(_objCharacter))
                        {
                            XPathNavigator xmlValueNode = await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("costdiscount/value", token).ConfigureAwait(false);
                            if (xmlValueNode != null)
                            {
                                int intValue = xmlValueNode.ValueAsInt;
                                switch ((await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("category", token).ConfigureAwait(false))?.Value)
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

                        if (_objCharacter.Created && !_objCharacter.Settings.DontDoubleQualityPurchases)
                        {
                            string strDoubleCostCareer = (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("doublecareer", token).ConfigureAwait(false))?.Value;
                            if (string.IsNullOrEmpty(strDoubleCostCareer) || strDoubleCostCareer != bool.FalseString)
                            {
                                intBP *= 2;
                            }
                        }

                        intBP *= await nudRating.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);

                        await lblBP.DoThreadSafeAsync(x => x.Text = (intBP * _objCharacter.Settings.KarmaQuality).ToString(GlobalSettings.CultureInfo), token: token).ConfigureAwait(false);
                        if (!_objCharacter.Created && _objCharacter.FreeSpells > 0 && Convert.ToBoolean(
                            (await xmlQuality.SelectSingleNodeAndCacheExpressionAsync("canbuywithspellpoints", token).ConfigureAwait(false))?.Value,
                            GlobalSettings.InvariantCultureInfo))
                        {
                            int i = (intBP * _objCharacter.Settings.KarmaQuality);
                            int spellPoints = 0;
                            while (i > 0)
                            {
                                i -= 5;
                                spellPoints++;
                            }

                            string strBP = string.Format(GlobalSettings.CultureInfo, "{0}/{0}{1}{0}{2}",
                                                         await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                                                         spellPoints,
                                                         await LanguageManager.GetStringAsync("String_SpellPoints", token: token).ConfigureAwait(false));
                            string strBPTooltip
                                = await LanguageManager.GetStringAsync("Tip_SelectSpell_MasteryQuality", token: token).ConfigureAwait(false);
                            await lblBP.DoThreadSafeAsync(x =>
                            {
                                x.Text += strBP;
                                x.ToolTipText = strBPTooltip;
                            }, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await lblBP.DoThreadSafeAsync(x => x.ToolTipText = string.Empty, token: token).ConfigureAwait(false);
                        }
                    }
                }
            }

            bool blnShowBP = !string.IsNullOrEmpty(await lblBP.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
            await lblBPLabel.DoThreadSafeAsync(x => x.Visible = blnShowBP, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Build the list of Qualities.
        /// </summary>
        private async ValueTask BuildQualityList(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                                  .ConfigureAwait(false) ?? string.Empty;
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(')
                         .Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false))
                         .Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || await txtSearch
                                                                    .DoThreadSafeFuncAsync(
                                                                        x => x.TextLength, token: token)
                                                                    .ConfigureAwait(false) == 0))
                {
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                }
                else
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value))
                        {
                            if (!string.IsNullOrEmpty(strItem))
                                sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdCategoryFilter.Length -= 4;
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                        }
                    }
                }

                if (await chkMetagenic.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                {
                    sbdFilter.Append(" and (metagenic = 'True' or required/oneof[contains(., 'Changeling')])");
                }
                else if (await chkNotMetagenic.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                              .ConfigureAwait(false))
                {
                    sbdFilter.Append(" and not(metagenic = 'True') and not(required/oneof[contains(., 'Changeling')])");
                }

                decimal decValueBP = await nudValueBP.DoThreadSafeFuncAsync(x => x.Value, token: token)
                                                     .ConfigureAwait(false);
                if (decValueBP != 0)
                {
                    string strValueBP = decValueBP.ToString(GlobalSettings.InvariantCultureInfo);
                    if (_objCharacter.Created && !_objCharacter.Settings.DontDoubleQualityPurchases
                                              && decValueBP > 0)
                    {
                        string strValueBPHalved = (decValueBP / 2).ToString(GlobalSettings.InvariantCultureInfo);
                        sbdFilter.Append(" and ((doublecareer = 'False' and (karma = ").Append(strValueBP)
                                 .Append(" or (not(nolevels) and limit != 'False' and (karma mod ").Append(strValueBP)
                                 .Append(") = 0 and karma * karma * limit <= karma * ").Append(strValueBP)
                                 .Append("))) or (not(doublecareer = 'False') and (karma = ").Append(strValueBPHalved)
                                 .Append(" or (not(nolevels) and limit != 'False' and (karma mod ")
                                 .Append(strValueBPHalved)
                                 .Append(") = 0 and karma * karma * limit <= karma * ").Append(strValueBPHalved)
                                 .Append("))))");
                    }
                    else
                    {
                        sbdFilter.Append(" and (karma = ").Append(strValueBP)
                                 .Append(" or (not(nolevels) and limit != 'False' and (karma mod ").Append(strValueBP)
                                 .Append(") = 0 and karma * karma * limit <= karma * ").Append(strValueBP).Append("))");
                    }
                }
                else
                {
                    int intMinimumBP = await nudMinimumBP.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token)
                                                         .ConfigureAwait(false);
                    int intMaximumBP = await nudMaximumBP.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token)
                                                         .ConfigureAwait(false);
                    if (intMinimumBP != 0 || intMaximumBP != 0)
                    {
                        if (intMinimumBP < 0 == intMaximumBP < 0)
                        {
                            sbdFilter.Append(" and (")
                                     .Append(GetKarmaRangeString(intMaximumBP, intMinimumBP))
                                     .Append(')');
                        }
                        else
                        {
                            sbdFilter.Append("and ((").Append(GetKarmaRangeString(intMaximumBP, 0))
                                     .Append(") or (")
                                     .Append(GetKarmaRangeString(-1, intMinimumBP)).Append("))");
                        }

                        string GetKarmaRangeString(int intMax, int intMin)
                        {
                            string strMax = intMax.ToString(GlobalSettings.InvariantCultureInfo);
                            string strMin = intMin.ToString(GlobalSettings.InvariantCultureInfo);
                            string strMostExtremeValue
                                = (intMax > 0 ? intMax : intMin).ToString(GlobalSettings.InvariantCultureInfo);
                            string strValueDiff
                                = (intMax > 0 ? intMax - intMin : intMin - intMax).ToString(
                                    GlobalSettings.InvariantCultureInfo);
                            if (_objCharacter.Created && !_objCharacter.Settings.DontDoubleQualityPurchases)
                            {
                                return "((doublecareer = 'False' or karma < 0) and ((karma >= " + strMin
                                    + " and karma <= "
                                    +
                                    strMax + ") or (not(nolevels) and limit != 'False' and karma * karma <= karma * " +
                                    strMostExtremeValue + " and (karma * (" + strMostExtremeValue +
                                    " mod karma) <= karma * " + strValueDiff
                                    + ") and ((karma >= 0 and karma * limit >= "
                                    +
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
                                   strMostExtremeValue + " and (karma * (" + strMostExtremeValue
                                   + " mod karma) <= karma * "
                                   +
                                   strValueDiff + ") and ((karma >= 0 and karma * limit >= " + strMin +
                                   ") or (karma < 0 and karma * limit <= " + strMax + ")))";
                        }
                    }
                }

                string strSearch
                    = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            string strCategoryLower = strCategory == "Show All" ? "*" : strCategory.ToLowerInvariant();
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstQuality))
            {
                bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                      .ConfigureAwait(false);
                foreach (XPathNavigator objXmlQuality in
                         _xmlBaseQualityDataNode.Select("qualities/quality" + strFilter))
                {
                    string strLoopName = (await objXmlQuality
                                                .SelectSingleNodeAndCacheExpressionAsync("name", token: token)
                                                .ConfigureAwait(false))?.Value;
                    if (string.IsNullOrEmpty(strLoopName))
                        continue;
                    if (_xmlMetatypeQualityRestrictionNode != null
                        && _xmlMetatypeQualityRestrictionNode.SelectSingleNode(
                            strCategoryLower + "/quality[. = " + strLoopName.CleanXPath() + ']') == null)
                        continue;
                    if (!blnLimitList
                        || objXmlQuality.RequirementsMet(_objCharacter, string.Empty, string.Empty, IgnoreQuality))
                    {
                        lstQuality.Add(new ListItem(
                                           (await objXmlQuality
                                                  .SelectSingleNodeAndCacheExpressionAsync("id", token: token)
                                                  .ConfigureAwait(false))?.Value
                                           ?? string.Empty,
                                           (await objXmlQuality
                                                  .SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                                  .ConfigureAwait(false))?.Value
                                           ?? strLoopName));
                    }
                }

                lstQuality.Sort(CompareListItems.CompareNames);

                string strOldSelectedQuality = await lstQualities
                                                     .DoThreadSafeFuncAsync(
                                                         x => x.SelectedValue?.ToString(), token: token)
                                                     .ConfigureAwait(false);
                _blnLoading = true;
                await lstQualities.PopulateWithListItemsAsync(lstQuality, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstQualities.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrEmpty(strOldSelectedQuality))
                        x.SelectedIndex = -1;
                    else
                        x.SelectedValue = strOldSelectedQuality;
                }, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strSelectedQuality = await lstQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedQuality))
                return;

            XPathNavigator objNode = _xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[id = " + strSelectedQuality.CleanXPath() + ']');

            if (objNode?.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync("String_Quality", token: token).ConfigureAwait(false), IgnoreQuality) != true)
                return;

            _strSelectedQuality = strSelectedQuality;
            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0)
                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false)
                : (await objNode.SelectSingleNodeAndCacheExpressionAsync("category", token).ConfigureAwait(false))?.Value;
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
