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
using System.Xml;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class SelectLifestyleQuality : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain;
        private readonly Character _objCharacter;
        private string _strIgnoreQuality = string.Empty;
        private readonly string _strSelectedLifestyle;
        private readonly IReadOnlyCollection<LifestyleQuality> _lstExistingQualities;

        private readonly XmlDocument _objXmlDocument;

        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private static readonly ReadOnlyCollection<string> s_LifestylesSorted = Array.AsReadOnly(new[] { "Street", "Squatter", "Low", "Medium", "High", "Luxury" });
        private static readonly IReadOnlyCollection<string> s_LifestyleSpecific = new HashSet<string> { "Bolt Hole", "Traveler", "Commercial", "Hospitalized" };

        private static string _strSelectCategory = string.Empty;

        private readonly XmlDocument _objMetatypeDocument;
        private readonly XmlDocument _objCritterDocument;

        #region Control Events

        public SelectLifestyleQuality(Character objCharacter, string strSelectedLifestyle, IReadOnlyCollection<LifestyleQuality> lstExistingQualities)
        {
            Disposed += (sender, args) => Utils.ListItemListPool.Return(_lstCategory);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _strSelectedLifestyle = strSelectedLifestyle;
            _lstExistingQualities = lstExistingQualities;

            // Load the Quality information.
            _objXmlDocument = _objCharacter.LoadData("lifestyles.xml");
            _objMetatypeDocument = _objCharacter.LoadData("metatypes.xml");
            _objCritterDocument = _objCharacter.LoadData("critters.xml");
        }

        private async void SelectLifestyleQuality_Load(object sender, EventArgs e)
        {
            // Populate the Quality Category list.
            using (XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category"))
            {
                if (objXmlCategoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlCategory in objXmlCategoryList)
                    {
                        string strCategory = objXmlCategory.InnerText;
                        if (await AnyItemInList(strCategory).ConfigureAwait(false))
                        {
                            _lstCategory.Add(new ListItem(strCategory, objXmlCategory.Attributes?["translate"]?.InnerText ?? strCategory));
                        }
                    }
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
            if (_objCharacter.Created || !_objCharacter.EffectiveBuildMethodUsesPriorityTables)
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

            XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + strSelectedLifestyleId.CleanXPath() + ']');
            if (objXmlQuality == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                int intBP = 0;
                objXmlQuality.TryGetInt32FieldQuickly("lp", ref intBP);
                string strBP = await chkFree.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                    ? await LanguageManager.GetStringAsync("Checkbox_Free").ConfigureAwait(false)
                    : intBP.ToString(GlobalSettings.CultureInfo);
                await lblBP.DoThreadSafeAsync(x => x.Text = strBP).ConfigureAwait(false);
                await lblBPLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strBP)).ConfigureAwait(false);

                string strSource = objXmlQuality["source"]?.InnerText
                                   ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                string strPage = objXmlQuality["altpage"]?.InnerText ?? objXmlQuality["page"]?.InnerText
                    ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(
                        strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await lblSource.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                    await lblSource.SetToolTipAsync(string.Empty).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }

                if (objXmlQuality["allowed"] != null)
                {
                    await lblMinimum.DoThreadSafeAsync(x =>
                    {
                        x.Text = GetMinimumRequirement(objXmlQuality["allowed"].InnerText);
                        x.Visible = true;
                    }).ConfigureAwait(false);
                    await lblMinimumLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await lblMinimum.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    await lblMinimumLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }

                if (objXmlQuality["cost"] != null)
                {
                    string strCost;
                    if (chkFree.Checked)
                    {
                        strCost = await LanguageManager.GetStringAsync("Checkbox_Free").ConfigureAwait(false);
                    }
                    else if (objXmlQuality["allowed"]?.InnerText.Contains(_strSelectedLifestyle) == true)
                    {
                        strCost = await LanguageManager.GetStringAsync("String_LifestyleFreeNuyen").ConfigureAwait(false);
                    }
                    else
                    {
                        strCost = objXmlQuality["cost"]?.InnerText;
                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost).ConfigureAwait(false);
                        decimal decCost = blnIsSuccess ? Convert.ToDecimal((double) objProcess) : 0;
                        strCost = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                  + await LanguageManager.GetStringAsync("String_NuyenSymbol").ConfigureAwait(false);
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
                case Keys.Up when lstLifestyleQualities.SelectedIndex - 1 >= 0:
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
                txtSearch.Select(txtSearch.Text.Length, 0);
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
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

        #endregion Properties

        #region Methods

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
            if (_blnLoading && blnDoUIUpdate)
                return false;
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || string.IsNullOrWhiteSpace(strSearch)))
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

                if (_strSelectedLifestyle != "Bolt Hole")
                {
                    sbdFilter.Append(" and (name != \"Dug a Hole\")");
                }

                if (!string.IsNullOrEmpty(strSearch))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            List<ListItem> lstLifestyleQuality = blnDoUIUpdate ? Utils.ListItemListPool.Get() : null;
            try
            {
                using (XmlNodeList objXmlQualityList
                       = _objXmlDocument.SelectNodes("/chummer/qualities/quality" + strFilter))
                {
                    if (objXmlQualityList?.Count > 0)
                    {
                        bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                        foreach (XmlNode objXmlQuality in objXmlQualityList)
                        {
                            string strId = objXmlQuality["id"]?.InnerText;
                            if (string.IsNullOrEmpty(strId))
                                continue;
                            if (!blnDoUIUpdate)
                            {
                                return true;
                            }

                            if (blnLimitList && !await RequirementMet(objXmlQuality, false, token).ConfigureAwait(false))
                                continue;

                            lstLifestyleQuality.Add(
                                new ListItem(
                                    strId,
                                    objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"]?.InnerText
                                    ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                        }
                    }
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
                    Utils.ListItemListPool.Return(lstLifestyleQuality);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strSelectedSourceIDString = await lstLifestyleQualities.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedSourceIDString))
                return;
            XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + strSelectedSourceIDString.CleanXPath() + ']');
            if (objNode == null || !await RequirementMet(objNode, true, token: token).ConfigureAwait(false))
                return;

            _strSelectedQuality = strSelectedSourceIDString;
            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0)
                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false)
                : objNode["category"]?.InnerText;

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Check if the Quality's requirements/restrictions are being met.
        /// </summary>
        /// <param name="objXmlQuality">XmlNode of the Quality.</param>
        /// <param name="blnShowMessage">Whether or not a message should be shown if the requirements are not met.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async ValueTask<bool> RequirementMet(XmlNode objXmlQuality, bool blnShowMessage, CancellationToken token = default)
        {
            // Ignore the rules.
            if (await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                return true;

            // See if the character already has this Quality and whether or not multiple copies are allowed.
            if (objXmlQuality["limit"]?.InnerText != bool.FalseString)
            {
                // Multiples aren't allowed, so make sure the character does not already have it.
                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                {
                    if (objXmlQuality["allowmultiple"] == null && objQuality.Name == objXmlQuality["name"].InnerText)
                    {
                        if (blnShowMessage)
                            Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectQuality_QualityLimit", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SelectQuality_QualityLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
            }

            if (objXmlQuality.InnerXml.Contains("forbidden"))
            {
                bool blnRequirementForbidden = false;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdForbidden))
                {
                    // Loop through the oneof requirements.
                    using (XmlNodeList objXmlForbiddenList = objXmlQuality.SelectNodes("forbidden/oneof"))
                    {
                        foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
                        {
                            using (XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes)
                            {
                                foreach (XmlNode objXmlForbidden in objXmlOneOfList)
                                {
                                    switch (objXmlForbidden.Name)
                                    {
                                        case "quality":
                                            // Run through all of the Qualities the character has and see if the current forbidden item exists.
                                            // If so, turn on the RequirementForbidden flag so it cannot be selected.
                                            foreach (LifestyleQuality objQuality in _lstExistingQualities)
                                            {
                                                if (objQuality.Name == objXmlForbidden.InnerText
                                                    && objQuality.Name != _strIgnoreQuality)
                                                {
                                                    blnRequirementForbidden = true;
                                                    sbdForbidden.AppendLine().Append('\t')
                                                                .Append(objQuality.CurrentDisplayNameShort);
                                                }
                                            }

                                            break;

                                        case "characterquality":
                                            // Run through all of the Qualities the character has and see if the current forbidden item exists.
                                            // If so, turn on the RequirementForbidden flag so it cannot be selected.
                                            foreach (Quality objQuality in _objCharacter.Qualities)
                                            {
                                                if (objQuality.Name == objXmlForbidden.InnerText
                                                    && objQuality.Name != _strIgnoreQuality)
                                                {
                                                    blnRequirementForbidden = true;
                                                    sbdForbidden.AppendLine().Append('\t')
                                                                .Append(objQuality.CurrentDisplayName);
                                                }
                                            }

                                            break;

                                        case "metatype":
                                            // Check the Metatype restriction.
                                            if (objXmlForbidden.InnerText == _objCharacter.Metatype)
                                            {
                                                blnRequirementForbidden = true;
                                                XmlNode objNode
                                                    = _objMetatypeDocument.SelectSingleNode(
                                                          "/chummer/metatypes/metatype[name = "
                                                          + objXmlForbidden.InnerText.CleanXPath() + ']') ??
                                                      _objCritterDocument.SelectSingleNode(
                                                          "/chummer/metatypes/metatype[name = "
                                                          + objXmlForbidden.InnerText.CleanXPath() + ']');
                                                sbdForbidden.AppendLine().Append('\t')
                                                            .Append(
                                                                objNode["translate"]?.InnerText
                                                                ?? objXmlForbidden.InnerText);
                                            }

                                            break;

                                        case "metatypecategory":
                                            // Check the Metatype Category restriction.
                                            if (objXmlForbidden.InnerText == _objCharacter.MetatypeCategory)
                                            {
                                                blnRequirementForbidden = true;
                                                XmlNode objNode
                                                    = _objMetatypeDocument.SelectSingleNode(
                                                          "/chummer/categories/category[. = "
                                                          + objXmlForbidden.InnerText.CleanXPath() + ']') ??
                                                      _objCritterDocument.SelectSingleNode(
                                                          "/chummer/categories/category[. = "
                                                          + objXmlForbidden.InnerText.CleanXPath() + ']');
                                                sbdForbidden.AppendLine().Append('\t').Append(
                                                    objNode.Attributes["translate"]?.InnerText
                                                    ?? objXmlForbidden.InnerText);
                                            }

                                            break;

                                        case "metavariant":
                                            // Check the Metavariant restriction.
                                            if (objXmlForbidden.InnerText == _objCharacter.Metavariant)
                                            {
                                                blnRequirementForbidden = true;
                                                XmlNode objNode
                                                    = _objMetatypeDocument.SelectSingleNode(
                                                          "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                          + objXmlForbidden.InnerText.CleanXPath() + ']') ??
                                                      _objCritterDocument.SelectSingleNode(
                                                          "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                          + objXmlForbidden.InnerText.CleanXPath() + ']');
                                                sbdForbidden.AppendLine().Append('\t')
                                                            .Append(
                                                                objNode["translate"]?.InnerText
                                                                ?? objXmlForbidden.InnerText);
                                            }

                                            break;

                                        case "metagenic":
                                            // Check to see if the character has a Metagenic Quality.
                                            foreach (Quality objQuality in _objCharacter.Qualities)
                                            {
                                                if ((await objQuality.GetNodeXPathAsync(token: token).ConfigureAwait(false))
                                                    ?.SelectSingleNode("metagenic")
                                                    ?.Value == bool.TrueString)
                                                {
                                                    blnRequirementForbidden = true;
                                                    sbdForbidden.AppendLine().Append('\t')
                                                                .Append(objQuality.CurrentDisplayName);
                                                    break;
                                                }
                                            }

                                            break;
                                    }
                                }
                            }
                        }
                    }

                    // The character is not allowed to take the Quality, so display a message and uncheck the item.
                    if (blnRequirementForbidden)
                    {
                        if (blnShowMessage)
                            Program.ShowMessageBox(this,
                                                            await LanguageManager.GetStringAsync(
                                                                "Message_SelectQuality_QualityRestriction", token: token).ConfigureAwait(false)
                                                            + sbdForbidden,
                                                            await LanguageManager.GetStringAsync(
                                                                "MessageTitle_SelectQuality_QualityRestriction", token: token).ConfigureAwait(false),
                                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
            }

            if (objXmlQuality.InnerXml.Contains("required"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdRequirement))
                {
                    bool blnRequirementMet = true;

                    XmlDocument objXmlQualityDocument = await _objCharacter.LoadDataAsync("qualities.xml", token: token).ConfigureAwait(false);
                    // Loop through the oneof requirements.
                    using (XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/oneof"))
                    {
                        foreach (XmlNode objXmlOneOf in objXmlRequiredList)
                        {
                            bool blnOneOfMet = false;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdThisRequirement))
                            {
                                sbdThisRequirement.AppendLine()
                                                  .Append(await LanguageManager.GetStringAsync(
                                                              "Message_SelectQuality_OneOf", token: token).ConfigureAwait(false));
                                using (XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes)
                                {
                                    foreach (XmlNode objXmlRequired in objXmlOneOfList)
                                    {
                                        switch (objXmlRequired.Name)
                                        {
                                            case "quality":
                                                // Run through all of the Qualities the character has and see if the current required item exists.
                                                // If so, turn on the RequirementMet flag so it can be selected.
                                                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                                                {
                                                    if (objQuality.Name == objXmlRequired.InnerText)
                                                        blnOneOfMet = true;
                                                }

                                                if (!blnOneOfMet)
                                                {
                                                    XmlNode objNode = _objXmlDocument.SelectSingleNode(
                                                        "/chummer/qualities/quality[name = "
                                                        + objXmlRequired.InnerText.CleanXPath()
                                                        +
                                                        "]");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText
                                                                              ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "characterquality":

                                                // Run through all of the Qualities the character has and see if the current required item exists.
                                                // If so, turn on the RequirementMet flag so it can be selected.
                                                foreach (Quality objQuality in _objCharacter.Qualities)
                                                {
                                                    if (objQuality.Name == objXmlRequired.InnerText)
                                                        blnOneOfMet = true;
                                                }

                                                if (!blnOneOfMet)
                                                {
                                                    XmlNode objNode = objXmlQualityDocument.SelectSingleNode(
                                                        "/chummer/qualities/quality[name = "
                                                        + objXmlRequired.InnerText.CleanXPath()
                                                        +
                                                        "]");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText
                                                                              ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "metatype":
                                                // Check the Metatype requirement.
                                                if (objXmlRequired.InnerText == _objCharacter.Metatype)
                                                    blnOneOfMet = true;
                                                else
                                                {
                                                    XmlNode objNode =
                                                        _objMetatypeDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype[name = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]") ??
                                                        _objCritterDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype[name = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText
                                                                              ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "metatypecategory":
                                                // Check the Metatype Category requirement.
                                                if (objXmlRequired.InnerText == _objCharacter.MetatypeCategory)
                                                    blnOneOfMet = true;
                                                else
                                                {
                                                    XmlNode objNode =
                                                        _objMetatypeDocument.SelectSingleNode(
                                                            "/chummer/categories/category[. = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]") ??
                                                        _objCritterDocument.SelectSingleNode(
                                                            "/chummer/categories/category[. = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText
                                                                              ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "metavariant":
                                                // Check the Metavariant requirement.
                                                if (objXmlRequired.InnerText == _objCharacter.Metavariant)
                                                    blnOneOfMet = true;
                                                else
                                                {
                                                    XmlNode objNode =
                                                        _objMetatypeDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                            +
                                                            objXmlRequired.InnerText.CleanXPath() + ']') ??
                                                        _objCritterDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                            +
                                                            objXmlRequired.InnerText.CleanXPath() + ']');
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText
                                                                              ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "inherited":
                                                sbdThisRequirement.AppendLine().Append('\t')
                                                                  .Append(
                                                                      await LanguageManager.GetStringAsync(
                                                                          "Message_SelectQuality_Inherit", token: token).ConfigureAwait(false));
                                                break;

                                            case "careerkarma":
                                                // Check Career Karma requirement.
                                                if (_objCharacter.CareerKarma >= Convert.ToInt32(
                                                        objXmlRequired.InnerText,
                                                        GlobalSettings.InvariantCultureInfo))
                                                    blnOneOfMet = true;
                                                else
                                                    sbdThisRequirement.AppendLine().Append('\t').AppendFormat(
                                                        GlobalSettings.CultureInfo,
                                                        await LanguageManager.GetStringAsync(
                                                            "Message_SelectQuality_RequireKarma", token: token).ConfigureAwait(false),
                                                        objXmlRequired.InnerText);
                                                break;

                                            case "ess":
                                                // Check Essence requirement.
                                                if (objXmlRequired.InnerText.StartsWith('-'))
                                                {
                                                    // Essence must be less than the value.
                                                    if (await _objCharacter.EssenceAsync(token: token).ConfigureAwait(false) < Convert.ToDecimal(
                                                            objXmlRequired.InnerText.TrimStart('-'),
                                                            GlobalSettings.InvariantCultureInfo))
                                                        blnOneOfMet = true;
                                                }
                                                else
                                                {
                                                    // Essence must be equal to or greater than the value.
                                                    if (await _objCharacter.EssenceAsync(token: token).ConfigureAwait(false)
                                                        >= Convert.ToDecimal(objXmlRequired.InnerText,
                                                                             GlobalSettings.InvariantCultureInfo))
                                                        blnOneOfMet = true;
                                                }

                                                break;

                                            case "skill":
                                                // Check if the character has the required Skill.
                                                Skill objSkill
                                                    = await _objCharacter.SkillsSection.GetActiveSkillAsync(
                                                        objXmlRequired["name"].InnerText, token).ConfigureAwait(false);
                                                if ((objSkill?.Rating ?? 0)
                                                    >= Convert.ToInt32(objXmlRequired["val"].InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                {
                                                    blnOneOfMet = true;
                                                }

                                                break;

                                            case "attribute":
                                                // Check to see if an Attribute meets a requirement.
                                                CharacterAttrib objAttribute
                                                    = await _objCharacter.GetAttributeAsync(objXmlRequired["name"].InnerText, token: token).ConfigureAwait(false);
                                                // Make sure the Attribute's total value meets the requirement.
                                                if (objXmlRequired["total"] != null && await objAttribute.GetTotalValueAsync(token).ConfigureAwait(false)
                                                    >= Convert.ToInt32(objXmlRequired["total"].InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                {
                                                    blnOneOfMet = true;
                                                }

                                                break;

                                            case "attributetotal":
                                                // Check if the character's Attributes add up to a particular total.
                                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                                strAttributes
                                                    = await _objCharacter.AttributeSection
                                                                         .ProcessAttributesInXPathAsync(strAttributes, token: token).ConfigureAwait(false);
                                                (bool blnIsSuccess, object objProcess)
                                                    = await CommonFunctions.EvaluateInvariantXPathAsync(
                                                        strAttributes, token).ConfigureAwait(false);
                                                if ((blnIsSuccess ? ((double) objProcess).StandardRound() : 0)
                                                    >= Convert.ToInt32(objXmlRequired["val"].InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                    blnOneOfMet = true;
                                                break;

                                            case "skillgrouptotal":
                                            {
                                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                                int intTotal = 0;
                                                foreach (string strGroup in objXmlRequired["skillgroups"].InnerText
                                                             .SplitNoAlloc('+', StringSplitOptions.RemoveEmptyEntries))
                                                {
                                                    foreach (SkillGroup objGroup in _objCharacter.SkillsSection
                                                                 .SkillGroups)
                                                    {
                                                        if (objGroup.Name == strGroup)
                                                        {
                                                            intTotal += objGroup.Rating;
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText,
                                                        GlobalSettings.InvariantCultureInfo))
                                                    blnOneOfMet = true;
                                            }
                                                break;

                                            case "lifestyle":
                                                if (_strSelectedLifestyle == objXmlRequired.InnerText)
                                                    blnOneOfMet = true;
                                                break;

                                            case "cyberwares":
                                            {
                                                // Check to see if the character has a number of the required Cyberware/Bioware items.
                                                int intTotal = 0;

                                                // Check Cyberware.
                                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                             "cyberware"))
                                                {
                                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware.Where(
                                                                 objCyberware =>
                                                                     objCyberware.Name == objXmlCyberware.InnerText))
                                                    {
                                                        string strSelect = objXmlCyberware.Attributes["select"]
                                                            ?.InnerText;
                                                        if (string.IsNullOrEmpty(strSelect)
                                                            || strSelect == objCyberware.Extra)
                                                        {
                                                            intTotal++;
                                                            break;
                                                        }
                                                    }
                                                }

                                                // Check Bioware.
                                                foreach (XmlNode objXmlBioware in objXmlRequired.SelectNodes("bioware"))
                                                {
                                                    if (await _objCharacter.Cyberware.AnyAsync(
                                                            objCyberware =>
                                                                objCyberware.Name == objXmlBioware.InnerText, token: token).ConfigureAwait(false))
                                                    {
                                                        intTotal++;
                                                    }
                                                }

                                                // Check Cyberware name that contain a straing.
                                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                             "cyberwarecontains"))
                                                {
                                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware.Where(
                                                                 objCyberware =>
                                                                     objCyberware.Name.Contains(
                                                                         objXmlCyberware.InnerText)))
                                                    {
                                                        string strSelect = objXmlCyberware.Attributes["select"]
                                                            ?.InnerText;
                                                        if (string.IsNullOrEmpty(strSelect)
                                                            || strSelect == objCyberware.Extra)
                                                        {
                                                            intTotal++;
                                                            break;
                                                        }
                                                    }
                                                }

                                                // Check Bioware name that contain a straing.
                                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                             "biowarecontains"))
                                                {
                                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                                    {
                                                        if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                                        {
                                                            string strSelect = objXmlCyberware.Attributes["select"]
                                                                ?.InnerText;
                                                            if (string.IsNullOrEmpty(strSelect)
                                                                || strSelect == objCyberware.Extra)
                                                            {
                                                                intTotal++;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                // Check for Cyberware Plugins.
                                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                             "cyberwareplugin"))
                                                {
                                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                                    {
                                                        if (await objCyberware.Children.AnyAsync(
                                                                objPlugin =>
                                                                    objPlugin.Name == objXmlCyberware.InnerText, token: token).ConfigureAwait(false))
                                                        {
                                                            intTotal++;
                                                        }
                                                    }
                                                }

                                                // Check for Cyberware Categories.
                                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                             "cyberwarecategory"))
                                                {
                                                    intTotal += await _objCharacter.Cyberware.CountAsync(objCyberware =>
                                                        objCyberware.Category == objXmlCyberware.InnerText, token: token).ConfigureAwait(false);
                                                }

                                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText,
                                                        GlobalSettings.InvariantCultureInfo))
                                                    blnOneOfMet = true;
                                            }
                                                break;

                                            case "streetcredvsnotoriety":
                                                // Street Cred must be higher than Notoriety.
                                                if (_objCharacter.StreetCred >= _objCharacter.Notoriety)
                                                    blnOneOfMet = true;
                                                break;

                                            case "damageresistance":
                                                // Damage Resistance must be a particular value.
                                                if (await _objCharacter.BOD.GetTotalValueAsync(token).ConfigureAwait(false)
                                                    + await ImprovementManager.ValueOfAsync(_objCharacter,
                                                        Improvement.ImprovementType
                                                                   .DamageResistance, token: token).ConfigureAwait(false)
                                                    >= Convert.ToInt32(objXmlRequired.InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                    blnOneOfMet = true;
                                                break;
                                        }
                                    }
                                }

                                // Update the flag for requirements met.
                                blnRequirementMet = blnRequirementMet && blnOneOfMet;
                                sbdRequirement.Append(sbdThisRequirement);
                            }
                        }
                    }

                    // Loop through the allof requirements.
                    using (XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/allof"))
                    {
                        foreach (XmlNode objXmlAllOf in objXmlRequiredList)
                        {
                            bool blnAllOfMet = true;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdThisRequirement))
                            {
                                sbdThisRequirement.AppendLine()
                                                  .Append(await LanguageManager.GetStringAsync(
                                                              "Message_SelectQuality_AllOf", token: token).ConfigureAwait(false));
                                using (XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes)
                                {
                                    foreach (XmlNode objXmlRequired in objXmlAllOfList)
                                    {
                                        bool blnFound = false;
                                        switch (objXmlRequired.Name)
                                        {
                                            case "quality":

                                                // Run through all of the Qualities the character has and see if the current required item exists.
                                                // If so, turn on the RequirementMet flag so it can be selected.
                                                foreach (LifestyleQuality objQuality in _lstExistingQualities)
                                                {
                                                    if (objQuality.Name == objXmlRequired.InnerText)
                                                        blnFound = true;
                                                }

                                                if (!blnFound)
                                                {
                                                    XmlNode objNode = _objXmlDocument.SelectSingleNode(
                                                        "/chummer/qualities/quality[name = "
                                                        + objXmlRequired.InnerText.CleanXPath()
                                                        +
                                                        "]/translate");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(
                                                                          objNode?.InnerText
                                                                          ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "metatype":
                                                // Check the Metatype requirement.
                                                if (objXmlRequired.InnerText == _objCharacter.Metatype)
                                                    blnFound = true;
                                                else
                                                {
                                                    XmlNode objNode =
                                                        _objMetatypeDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype[name = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]") ??
                                                        _objCritterDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype[name = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText ??
                                                                              objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "metatypecategory":
                                                // Check the Metatype Category requirement.
                                                if (objXmlRequired.InnerText == _objCharacter.MetatypeCategory)
                                                    blnFound = true;
                                                else
                                                {
                                                    XmlNode objNode =
                                                        _objMetatypeDocument.SelectSingleNode(
                                                            "/chummer/categories/category[. = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]") ??
                                                        _objCritterDocument.SelectSingleNode(
                                                            "/chummer/categories/category[. = " +
                                                            objXmlRequired.InnerText.CleanXPath() +
                                                            "]");
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText ??
                                                                              objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "metavariant":
                                                // Check the Metavariant requirement.
                                                if (objXmlRequired.InnerText == _objCharacter.Metavariant)
                                                    blnFound = true;
                                                else
                                                {
                                                    XmlNode objNode =
                                                        _objMetatypeDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                            +
                                                            objXmlRequired.InnerText.CleanXPath() + ']') ??
                                                        _objCritterDocument.SelectSingleNode(
                                                            "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                            +
                                                            objXmlRequired.InnerText.CleanXPath() + ']');
                                                    sbdThisRequirement.AppendLine().Append('\t')
                                                                      .Append(objNode["translate"]?.InnerText
                                                                              ?? objXmlRequired.InnerText);
                                                }

                                                break;

                                            case "inherited":
                                                sbdThisRequirement.AppendLine().Append('\t')
                                                                  .Append(
                                                                      await LanguageManager.GetStringAsync(
                                                                          "Message_SelectQuality_Inherit", token: token).ConfigureAwait(false));
                                                break;

                                            case "careerkarma":
                                                // Check Career Karma requirement.
                                                if (_objCharacter.CareerKarma >= Convert.ToInt32(
                                                        objXmlRequired.InnerText,
                                                        GlobalSettings.InvariantCultureInfo))
                                                    blnFound = true;
                                                else
                                                    sbdThisRequirement.AppendLine().Append('\t').AppendFormat(
                                                        GlobalSettings.CultureInfo,
                                                        await LanguageManager.GetStringAsync(
                                                            "Message_SelectQuality_RequireKarma", token: token).ConfigureAwait(false),
                                                        objXmlRequired.InnerText);
                                                break;

                                            case "ess":
                                                // Check Essence requirement.
                                                if (objXmlRequired.InnerText.StartsWith('-'))
                                                {
                                                    // Essence must be less than the value.
                                                    if (await _objCharacter.EssenceAsync(token: token).ConfigureAwait(false) < Convert.ToDecimal(
                                                            objXmlRequired.InnerText.TrimStart('-'),
                                                            GlobalSettings.InvariantCultureInfo))
                                                        blnFound = true;
                                                }
                                                else
                                                {
                                                    // Essence must be equal to or greater than the value.
                                                    if (await _objCharacter.EssenceAsync(token: token).ConfigureAwait(false)
                                                        >= Convert.ToDecimal(objXmlRequired.InnerText,
                                                                             GlobalSettings.InvariantCultureInfo))
                                                        blnFound = true;
                                                }

                                                break;

                                            case "skill":
                                                // Check if the character has the required Skill.
                                                Skill objSkill
                                                    = await _objCharacter.SkillsSection.GetActiveSkillAsync(
                                                        objXmlRequired["name"].InnerText, token).ConfigureAwait(false);
                                                if ((objSkill?.Rating ?? 0)
                                                    >= Convert.ToInt32(objXmlRequired["val"].InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                {
                                                    blnFound = true;
                                                }

                                                break;

                                            case "attribute":
                                                // Check to see if an Attribute meets a requirement.
                                                CharacterAttrib objAttribute
                                                    = await _objCharacter.GetAttributeAsync(objXmlRequired["name"].InnerText, token: token).ConfigureAwait(false);
                                                // Make sure the Attribute's total value meets the requirement.
                                                if (objXmlRequired["total"] != null && await objAttribute.GetTotalValueAsync(token).ConfigureAwait(false)
                                                    >= Convert.ToInt32(objXmlRequired["total"].InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                {
                                                    blnFound = true;
                                                }

                                                break;

                                            case "attributetotal":
                                                // Check if the character's Attributes add up to a particular total.
                                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                                strAttributes
                                                    = await _objCharacter.AttributeSection
                                                                         .ProcessAttributesInXPathAsync(strAttributes, token: token).ConfigureAwait(false);
                                                (bool blnIsSuccess, object objProcess)
                                                    = await CommonFunctions.EvaluateInvariantXPathAsync(
                                                        strAttributes, token).ConfigureAwait(false);
                                                if ((blnIsSuccess ? ((double)objProcess).StandardRound() : 0)
                                                    >= Convert.ToInt32(objXmlRequired["val"].InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                    blnFound = true;
                                                break;

                                            case "skillgrouptotal":
                                                {
                                                    // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                                    int intTotal = 0;
                                                    foreach (string strGroup in objXmlRequired["skillgroups"].InnerText
                                                                 .SplitNoAlloc('+', StringSplitOptions.RemoveEmptyEntries))
                                                    {
                                                        foreach (SkillGroup objGroup in _objCharacter.SkillsSection
                                                                     .SkillGroups)
                                                        {
                                                            if (objGroup.Name == strGroup)
                                                            {
                                                                intTotal += objGroup.Rating;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText,
                                                            GlobalSettings.InvariantCultureInfo))
                                                        blnFound = true;
                                                }
                                                break;

                                            case "lifestyle":
                                                if (_strSelectedLifestyle == objXmlRequired.InnerText)
                                                    blnFound = true;
                                                break;

                                            case "cyberwares":
                                                {
                                                    // Check to see if the character has a number of the required Cyberware/Bioware items.
                                                    int intTotal = 0;

                                                    // Check Cyberware.
                                                    foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                                 "cyberware"))
                                                    {
                                                        foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                                        {
                                                            if (objCyberware.Name == objXmlCyberware.InnerText)
                                                            {
                                                                string strSelect = objXmlCyberware.Attributes["select"]
                                                                    ?.InnerText;
                                                                if (string.IsNullOrEmpty(strSelect)
                                                                    || strSelect == objCyberware.Extra)
                                                                {
                                                                    intTotal++;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    // Check Bioware.
                                                    foreach (XmlNode objXmlBioware in objXmlRequired.SelectNodes("bioware"))
                                                    {
                                                        if (await _objCharacter.Cyberware.AnyAsync(
                                                                objCyberware =>
                                                                    objCyberware.Name == objXmlBioware.InnerText, token: token).ConfigureAwait(false))
                                                        {
                                                            intTotal++;
                                                        }
                                                    }

                                                    // Check Cyberware name that contain a string.
                                                    foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                                 "cyberwarecontains"))
                                                    {
                                                        foreach (Cyberware objCyberware in await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                                                        {
                                                            if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                                            {
                                                                string strSelect = objXmlCyberware.Attributes["select"]
                                                                    ?.InnerText;
                                                                if (string.IsNullOrEmpty(strSelect)
                                                                    || strSelect == objCyberware.Extra)
                                                                {
                                                                    intTotal++;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    // Check Bioware name that contain a string.
                                                    foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                                 "biowarecontains"))
                                                    {
                                                        foreach (Cyberware objCyberware in await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                                                        {
                                                            if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                                            {
                                                                string strSelect = objXmlCyberware.Attributes["select"]
                                                                    ?.InnerText;
                                                                if (string.IsNullOrEmpty(strSelect)
                                                                    || strSelect == objCyberware.Extra)
                                                                {
                                                                    intTotal++;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    // Check for Cyberware Plugins.
                                                    foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                                 "cyberwareplugin"))
                                                    {
                                                        foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                                        {
                                                            if (await objCyberware.Children.AnyAsync(
                                                                    objPlugin =>
                                                                        objPlugin.Name == objXmlCyberware.InnerText, token: token).ConfigureAwait(false))
                                                            {
                                                                intTotal++;
                                                            }
                                                        }
                                                    }

                                                    // Check for Cyberware Categories.
                                                    foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes(
                                                                 "cyberwarecategory"))
                                                    {
                                                        intTotal += await _objCharacter.Cyberware.CountAsync(
                                                            objCyberware =>
                                                                objCyberware.Category == objXmlCyberware.InnerText, token: token).ConfigureAwait(false);
                                                    }

                                                    if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText,
                                                            GlobalSettings.InvariantCultureInfo))
                                                        blnFound = true;
                                                }
                                                break;

                                            case "streetcredvsnotoriety":
                                                // Street Cred must be higher than Notoriety.
                                                if (_objCharacter.StreetCred >= _objCharacter.Notoriety)
                                                    blnFound = true;
                                                break;

                                            case "damageresistance":
                                                // Damage Resistance must be a particular value.
                                                if (await _objCharacter.BOD.GetTotalValueAsync(token).ConfigureAwait(false)
                                                    + await ImprovementManager.ValueOfAsync(_objCharacter,
                                                        Improvement.ImprovementType
                                                                   .DamageResistance, token: token).ConfigureAwait(false)
                                                    >= Convert.ToInt32(objXmlRequired.InnerText,
                                                                       GlobalSettings.InvariantCultureInfo))
                                                    blnFound = true;
                                                break;
                                        }

                                        // If this item was not found, fail the AllOfMet condition.
                                        if (!blnFound)
                                            blnAllOfMet = false;
                                    }
                                }

                                // Update the flag for requirements met.
                                blnRequirementMet = blnRequirementMet && blnAllOfMet;
                                sbdRequirement.Append(sbdThisRequirement);
                            }
                        }
                    }

                    // The character has not met the requirements, so display a message and uncheck the item.
                    if (!blnRequirementMet)
                    {
                        if (blnShowMessage)
                        {
                            string strMessage = await LanguageManager.GetStringAsync("Message_SelectQuality_QualityRequirement", token: token).ConfigureAwait(false);
                            strMessage += sbdRequirement.ToString();
                            Program.ShowMessageBox(this, strMessage,
                                                            await LanguageManager.GetStringAsync(
                                                                "MessageTitle_SelectQuality_QualityRequirement", token: token).ConfigureAwait(false),
                                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
