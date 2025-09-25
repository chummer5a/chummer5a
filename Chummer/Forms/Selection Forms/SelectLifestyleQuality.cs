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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Enums;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectLifestyleQuality : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain;
        private readonly Character _objCharacter;
        private string _strIgnoreQuality = string.Empty;
        private readonly Lifestyle _objParentLifestyle;
        private bool _blnFreeCost;

        private readonly XPathNavigator _objXPathDocument;

        private List<ListItem> _lstCategory;
        private static readonly ReadOnlyCollection<string> s_LifestylesSorted = Array.AsReadOnly(new[] { "Street", "Squatter", "Low", "Medium", "High", "Luxury" });
        private static readonly IReadOnlyCollection<string> s_LifestyleSpecific = new HashSet<string> { "Bolt Hole", "Traveler", "Commercial", "Hospitalized" };

        private static string _strSelectCategory = string.Empty;

        #region Control Events

        public SelectLifestyleQuality(Character objCharacter, Lifestyle objParentLifestyle)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objParentLifestyle = objParentLifestyle ?? throw new ArgumentNullException(nameof(objParentLifestyle));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            // Load the Quality information.
            _objXPathDocument = _objCharacter.LoadDataXPath("lifestyles.xml");
            _lstCategory = Utils.ListItemListPool.Get();
        }

        private async void SelectLifestyleQuality_Load(object sender, EventArgs e)
        {
            if (await _objParentLifestyle.GetStyleTypeAsync().ConfigureAwait(false) == LifestyleType.Standard)
            {
                await lblBP.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await lblBPLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await lblMinimum.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await lblMinimumLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            }
            else
            {
                await lblBP.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await lblBPLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await lblMinimum.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                await lblMinimumLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }

            // Populate the Quality Category list.
            foreach (XPathNavigator objXmlCategory in _objXPathDocument.SelectAndCacheExpression("/chummer/categories/category"))
            {
                string strCategory = objXmlCategory.Value;
                if (await AnyItemInList(strCategory).ConfigureAwait(false))
                {
                    _lstCategory.Add(new ListItem(strCategory, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strCategory));
                }
            }

            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }
            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);
            await cboCategory.DoThreadSafeAsync(x =>
            {
                x.Enabled = _lstCategory.Count > 1;

                if (!string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedValue = _strSelectCategory;

                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);

            // Change the BP Label to Karma if the character is being built with Karma instead (or is in Career Mode).
            if (await _objCharacter.GetCreatedAsync().ConfigureAwait(false) || !await _objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync().ConfigureAwait(false))
            {
                string strTemp = await LanguageManager.GetStringAsync("Label_LP").ConfigureAwait(false);
                await lblBPLabel.DoThreadSafeAsync(x => x.Text = strTemp).ConfigureAwait(false);
            }

            _blnLoading = false;

            await RefreshList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async void RefreshListControlWithCurrentCategory(object sender, EventArgs e)
        {
            await RefreshList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async void lstLifestyleQualities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strSelectedLifestyleId = await lstLifestyleQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedLifestyleId))
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            XPathNavigator objXmlQuality = _objXPathDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strSelectedLifestyleId);

            if (objXmlQuality == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                if (_objParentLifestyle.StyleType != LifestyleType.Standard)
                {
                    int intBP = 0;
                    objXmlQuality.TryGetInt32FieldQuickly("lp", ref intBP);
                    string strBP = await chkFree.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                        ? await LanguageManager.GetStringAsync("Checkbox_Free").ConfigureAwait(false)
                        : intBP.ToString(GlobalSettings.CultureInfo);
                    await lblBP.DoThreadSafeAsync(x => x.Text = strBP).ConfigureAwait(false);
                    await lblBPLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strBP))
                        .ConfigureAwait(false);
                }

                string strSource = objXmlQuality.SelectSingleNodeAndCacheExpression("source")?.Value
                                   ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                string strPage = objXmlQuality.SelectSingleNodeAndCacheExpression("altpage")?.Value
                                 ?? objXmlQuality.SelectSingleNodeAndCacheExpression("page")?.Value
                                 ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(
                        strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource, this).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await lblSource.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                    await lblSource.SetToolTipTextAsync(string.Empty).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }

                string strAllowed = string.Empty;
                if (await _objParentLifestyle.GetStyleTypeAsync().ConfigureAwait(false) != LifestyleType.Standard)
                {
                    strAllowed = objXmlQuality.SelectSingleNodeAndCacheExpression("allowed")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strAllowed))
                    {
                        await lblMinimum.DoThreadSafeAsync(x =>
                        {
                            x.Text = GetMinimumRequirement(strAllowed);
                            x.Visible = true;
                        }).ConfigureAwait(false);
                        await lblMinimumLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                    }
                    else
                    {
                        await lblMinimum.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                        await lblMinimumLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    }
                }

                string strCost = objXmlQuality.SelectSingleNodeAndCacheExpression("cost")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strCost))
                {
                    if (await chkFree.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        strCost = await LanguageManager.GetStringAsync("Checkbox_Free").ConfigureAwait(false);
                    }
                    else if (strAllowed.Contains(await _objParentLifestyle.GetBaseLifestyleAsync().ConfigureAwait(false)))
                    {
                        strCost = await LanguageManager.GetStringAsync("String_LifestyleFreeNuyen").ConfigureAwait(false);
                    }
                    else
                    {
                        if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decCost))
                        {
                            strCost = await _objCharacter.ProcessAttributesInXPathAsync(strCost).ConfigureAwait(false);
                            (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost).ConfigureAwait(false);
                            if (blnIsSuccess)
                                decCost = Convert.ToDecimal((double)objProcess);
                        }
                        strCost = decCost.ToString(
                                              await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).GetNuyenFormatAsync().ConfigureAwait(false),
                                              GlobalSettings.CultureInfo)
                                          + await LanguageManager.GetStringAsync("String_NuyenSymbol")
                                              .ConfigureAwait(false);
                    }

                    await lblCost.DoThreadSafeAsync(x =>
                    {
                        x.Text = strCost;
                        x.Visible = true;
                    }).ConfigureAwait(false);
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await lblCost.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    await lblCostLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }

                await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
            }
        }

        private static string GetMinimumRequirement(string strAllowedLifestyles)
        {
            if (s_LifestyleSpecific.Contains(strAllowedLifestyles))
            {
                return strAllowedLifestyles;
            }
            int intMin = int.MaxValue;
            foreach (string strLifestyle in strAllowedLifestyles.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (s_LifestylesSorted.Contains(strLifestyle) && s_LifestylesSorted.IndexOf(strLifestyle) < intMin)
                {
                    intMin = s_LifestylesSorted.IndexOf(strLifestyle);
                }
            }
            return s_LifestylesSorted[intMin];
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

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstLifestyleQualities.SelectedIndex + 1 < lstLifestyleQualities.Items.Count:
                    ++lstLifestyleQualities.SelectedIndex;
                    break;

                case Keys.Down:
                    {
                        if (lstLifestyleQualities.Items.Count > 0)
                        {
                            lstLifestyleQualities.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstLifestyleQualities.SelectedIndex >= 1:
                    --lstLifestyleQualities.SelectedIndex;
                    break;

                case Keys.Up:
                    {
                        if (lstLifestyleQualities.Items.Count > 0)
                        {
                            lstLifestyleQualities.SelectedIndex = lstLifestyleQualities.Items.Count - 1;
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
        /// Quality that was selected in the dialogue.
        /// </summary>
        public string SelectedQuality => _strSelectedQuality;

        /// <summary>
        /// Forcefully add a Category to the list.
        /// </summary>
        public string ForceCategory
        {
            set
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

        /// <summary>
        /// A Quality the character has that should be ignored for checking Fobidden requirements (which would prevent upgrading/downgrading a Quality).
        /// </summary>
        public string IgnoreQuality
        {
            set => _strIgnoreQuality = value;
        }

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether the item has no cost.
        /// </summary>
        public bool FreeCost => _blnFreeCost;

        #endregion Properties

        #region Methods

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
            if (_blnLoading && blnDoUIUpdate)
                return false;
            string strFilter = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                sbdFilter.Append('(').Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || string.IsNullOrWhiteSpace(strSearch)))
                {
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                }
                else
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
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

                if (_objParentLifestyle.StyleType == LifestyleType.Standard)
                {
                    sbdFilter.Append(" and (source = \"SR5\" or category = \"Contracts\")");
                }

                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = "[" + sbdFilter.Append(']').ToString();
            }

            List<ListItem> lstLifestyleQuality = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                foreach (XPathNavigator objXmlQuality in _objXPathDocument.Select("/chummer/qualities/quality" + strFilter))
                {
                    string strId = objXmlQuality.SelectSingleNodeAndCacheExpression("id", token)?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;
                    if (!blnDoUIUpdate)
                    {
                        return true;
                    }

                    if (blnLimitList && !await objXmlQuality.RequirementsMetAsync(_objCharacter, _objParentLifestyle, _strIgnoreQuality, token: token).ConfigureAwait(false))
                        continue;

                    lstLifestyleQuality.Add(
                        new ListItem(
                            strId,
                            objXmlQuality.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                            ?? objXmlQuality.SelectSingleNodeAndCacheExpression("name", token)?.Value
                            ?? await LanguageManager.GetStringAsync("String_Unknown", token: token)
                                                    .ConfigureAwait(false)));
                }

                if (blnDoUIUpdate)
                {
                    lstLifestyleQuality.Sort(CompareListItems.CompareNames);

                    string strOldSelectedQuality = await lstLifestyleQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    _blnLoading = true;
                    await lstLifestyleQualities.PopulateWithListItemsAsync(lstLifestyleQuality, token: token).ConfigureAwait(false);
                    _blnLoading = false;
                    await lstLifestyleQualities.DoThreadSafeAsync(x =>
                    {
                        if (string.IsNullOrEmpty(strOldSelectedQuality))
                            x.SelectedIndex = -1;
                        else
                            x.SelectedValue = strOldSelectedQuality;
                    }, token: token).ConfigureAwait(false);
                }

                return lstLifestyleQuality?.Count > 0;
            }
            finally
            {
                if (lstLifestyleQuality != null)
                    Utils.ListItemListPool.Return(ref lstLifestyleQuality);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strSelectedSourceIDString = await lstLifestyleQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedSourceIDString))
                return;
            XPathNavigator objNode = _objXPathDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strSelectedSourceIDString);
            if (objNode == null || !await objNode.RequirementsMetAsync(_objCharacter, _objParentLifestyle,
                                                                       objNode.SelectSingleNodeAndCacheExpression(
                                                                           "translate", token)?.Value
                                                                       ?? objNode.SelectSingleNodeAndCacheExpression(
                                                                           "name", token)?.Value, _strIgnoreQuality, token: token).ConfigureAwait(false))
                return;

            _strSelectedQuality = strSelectedSourceIDString;
            _strSelectCategory = GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0
                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false)
                : objNode.SelectSingleNodeAndCacheExpression("category", token)?.Value;
            _blnFreeCost = await chkFree.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);

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
