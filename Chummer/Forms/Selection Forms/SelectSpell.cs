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

namespace Chummer
{
    public partial class SelectSpell : Form
    {
        private string _strSelectedSpell = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private bool _blnIgnoreRequirements;
        private string _strLimitCategory = string.Empty;
        private string _strForceSpell = string.Empty;
        private bool _blnCanGenericSpellBeFree;
        private bool _blnCanTouchOnlySpellBeFree;
        private static string _strSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseSpellDataNode;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private bool _blnRefresh;

        #region Control Events

        public SelectSpell(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            // Load the Spells information.
            _xmlBaseSpellDataNode = _objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer");
        }

        private async void SelectSpell_Load(object sender, EventArgs e)
        {
            await chkLimited.SetToolTipAsync(await LanguageManager.GetStringAsync("Tip_SelectSpell_LimitedSpell"));
            await chkExtended.SetToolTipAsync(await LanguageManager.GetStringAsync("Tip_SelectSpell_ExtendedSpell"));
            // If a value is forced, set the name of the spell and accept the form.
            if (!string.IsNullOrEmpty(_strForceSpell))
            {
                _strSelectedSpell = _strForceSpell;
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                });
            }
            (bool blnCanTouchOnlySpellBeFree, bool blnCanGenericSpellBeFree) = await _objCharacter.AllowFreeSpellsAsync();
            _blnCanTouchOnlySpellBeFree = blnCanTouchOnlySpellBeFree;
            _blnCanGenericSpellBeFree = blnCanGenericSpellBeFree;
            await txtSearch.DoThreadSafeAsync(x => x.Text = string.Empty);
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in await _xmlBaseSpellDataNode.SelectAndCacheExpressionAsync(
                         "categories/category"))
            {
                string strCategory = objXmlCategory.Value;
                if (!string.IsNullOrEmpty(_strLimitCategory) && strCategory != _strLimitCategory)
                    continue;
                if (!await AnyItemInList(strCategory))
                    continue;
                _lstCategory.Add(new ListItem(strCategory,
                                              (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value
                                              ?? strCategory));
            }

            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count != 1)
            {
                _lstCategory.Insert(0,
                    new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll")));
            }
            
            await cboCategory.PopulateWithListItemsAsync(_lstCategory);
            // Select the first Category in the list.
            await cboCategory.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedIndex = 0;
                else
                    x.SelectedValue = _strSelectCategory;
                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            });

            // Don't show the Extended Spell checkbox if the option to Extend any Detection Spell is disabled.
            await chkExtended.DoThreadSafeAsync(x => x.Visible = _objCharacter.Settings.ExtendAnyDetectionSpell);
            _blnLoading = false;
            await BuildSpellList(cboCategory.SelectedValue?.ToString());
        }

        private async void lstSpells_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSpellInfo();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await BuildSpellList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()));
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildSpellList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()));
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstSpells.SelectedIndex == -1 && lstSpells.Items.Count > 0)
            {
                lstSpells.SelectedIndex = 0;
            }
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        int intNewIndex = lstSpells.SelectedIndex + 1;
                        if (intNewIndex >= lstSpells.Items.Count)
                            intNewIndex = 0;
                        if (lstSpells.Items.Count > 0)
                            lstSpells.SelectedIndex = intNewIndex;
                        break;
                    }
                case Keys.Up:
                    {
                        int intNewIndex = lstSpells.SelectedIndex - 1;
                        if (intNewIndex <= 0)
                            intNewIndex = lstSpells.Items.Count - 1;
                        if (lstSpells.Items.Count > 0)
                            lstSpells.SelectedIndex = intNewIndex;
                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private async void chkExtended_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh)
                return;
            await UpdateSpellInfo();
        }

        private async void chkLimited_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh)
                return;
            await UpdateSpellInfo();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not a Limited version of the Spell was selected.
        /// </summary>
        public bool Limited => chkLimited.Checked;

        /// <summary>
        /// Whether or not an Extended version of the Spell was selected.
        /// </summary>
        public bool Extended => chkExtended.Checked;

        /// <summary>
        /// Whether or not a Alchemical version of the Spell was selected.
        /// </summary>
        public bool Alchemical => chkAlchemical.Checked;

        public bool FreeOnly { get; set; }

        /// <summary>
        /// Limit the Spell list to a particular Category.
        /// </summary>
        public string LimitCategory
        {
            set => _strLimitCategory = value;
        }

        /// <summary>
        /// Force a particular Spell to be selected.
        /// </summary>
        public string ForceSpellName
        {
            set => _strForceSpell = value;
        }

        /// <summary>
        /// Spell that was selected in the dialogue.
        /// </summary>
        public string SelectedSpell => _strSelectedSpell;

        public bool IgnoreRequirements
        {
            get => _blnIgnoreRequirements;
            set => _blnIgnoreRequirements = value;
        }

        public bool FreeBonus { get; set; }

        #endregion Properties

        #region Methods

        private ValueTask<bool> AnyItemInList(string strCategory = "")
        {
            return RefreshList(strCategory, false);
        }

        private ValueTask<bool> BuildSpellList(string strCategory = "")
        {
            return RefreshList(strCategory, true);
        }

        private async ValueTask<bool> RefreshList(string strCategory, bool blnDoUIUpdate)
        {
            if (_blnLoading && blnDoUIUpdate)
                return false;
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    await lstSpells.PopulateWithListItemsAsync(ListItem.Blank.Yield());
                }
                return false;
            }

            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text);
            bool blnHasSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength != 0);

            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSpellItems))
            {
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> limitDescriptors))
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> blockDescriptors))
                {
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                        if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                               && (GlobalSettings.SearchInCategoryOnly
                                                                   || !blnHasSearch))
                            sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                        else
                        {
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdCategoryFilter))
                            {
                                foreach (string strItem in _lstCategory.Select(x => x.Value))
                                {
                                    if (!string.IsNullOrEmpty(strItem))
                                        sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath())
                                                         .Append(" or ");
                                }

                                if (sbdCategoryFilter.Length > 0)
                                {
                                    sbdCategoryFilter.Length -= 4;
                                    sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                                }
                            }
                        }

                        if (_objCharacter.Settings.ExtendAnyDetectionSpell)
                            sbdFilter.Append(" and ((not(contains(name, \", Extended\"))))");
                        if (!string.IsNullOrEmpty(strSearch))
                            sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                        // Populate the Spell list.
                        if (!_blnIgnoreRequirements)
                        {
                            foreach (Improvement improvement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                         _objCharacter, Improvement.ImprovementType.LimitSpellDescriptor))
                            {
                                limitDescriptors.Add(improvement.ImprovedName);
                            }

                            foreach (Improvement improvement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                         _objCharacter, Improvement.ImprovementType.BlockSpellDescriptor))
                            {
                                blockDescriptors.Add(improvement.ImprovedName);
                            }
                        }

                        if (sbdFilter.Length > 0)
                            strFilter = '[' + sbdFilter.ToString() + ']';
                    }

                    foreach (XPathNavigator objXmlSpell in _xmlBaseSpellDataNode.Select("spells/spell" + strFilter))
                    {
                        string strSpellCategory = objXmlSpell.SelectSingleNode("category")?.Value ?? string.Empty;
                        if (!_blnIgnoreRequirements)
                        {
                            if (!objXmlSpell.RequirementsMet(_objCharacter))
                                continue;
                            
                            if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.AllowSpellCategory,
                                    strSpellCategory)).Count != 0)
                            {
                                if (!blnDoUIUpdate)
                                    return true;
                                await AddSpell(objXmlSpell, strSpellCategory);
                                continue;
                            }

                            string strRange = objXmlSpell.SelectSingleNode("range")?.Value ?? string.Empty;
                            if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.AllowSpellRange,
                                    strRange)).Count != 0)
                            {
                                if (!blnDoUIUpdate)
                                    return true;
                                await AddSpell(objXmlSpell, strSpellCategory);
                                continue;
                            }

                            string strDescriptor
                                = objXmlSpell.SelectSingleNode("descriptor")?.Value ?? string.Empty;

                            if (limitDescriptors.Count != 0
                                && !limitDescriptors.Any(l => strDescriptor.Contains(l)))
                                continue;

                            if (blockDescriptors.Count != 0 && blockDescriptors.Any(l => strDescriptor.Contains(l)))
                                continue;

                            if ((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                                             Improvement.ImprovementType.LimitSpellCategory))
                                .Any(x => x.ImprovedName != strSpellCategory))
                            {
                                continue;
                            }
                        }
                        
                        if (!blnDoUIUpdate)
                            return true;
                        await AddSpell(objXmlSpell, strSpellCategory);
                    }
                }

                if (blnDoUIUpdate)
                {
                    lstSpellItems.Sort(CompareListItems.CompareNames);
                    string strOldSelected = await lstSpells.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                    _blnLoading = true;
                    await lstSpells.PopulateWithListItemsAsync(lstSpellItems);
                    _blnLoading = false;
                    await lstSpells.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        else
                            x.SelectedIndex = -1;
                    });
                }

                async ValueTask AddSpell(XPathNavigator objXmlSpell, string strSpellCategory)
                {
                    string strDisplayName = (await objXmlSpell.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ??
                                            objXmlSpell.SelectSingleNode("name")?.Value ??
                                            await LanguageManager.GetStringAsync("String_Unknown");
                    if (!GlobalSettings.SearchInCategoryOnly && blnHasSearch
                                                             && !string.IsNullOrEmpty(strSpellCategory))
                    {
                        ListItem objFoundItem
                            = _lstCategory.Find(objFind => objFind.Value.ToString() == strSpellCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += strSpace + '[' + objFoundItem.Name + ']';
                        }
                    }

                    lstSpellItems.Add(new ListItem(objXmlSpell.SelectSingleNode("id")?.Value ?? string.Empty,
                                                   strDisplayName));
                }

                return lstSpellItems.Count > 0;
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strSelectedItem = await lstSpells.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            if (string.IsNullOrEmpty(strSelectedItem))
                return;

            // Display the Spell information.
            XPathNavigator objXmlSpell = _xmlBaseSpellDataNode.SelectSingleNode("spells/spell[id = " + strSelectedItem.CleanXPath() + ']');
            // Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
            // The maximum number of Spells a character can start with is 2 x (highest of Spellcasting or Ritual Spellcasting Skill).
            int intSpellCount = 0;
            int intRitualCount = 0;
            int intAlchPrepCount = 0;

            foreach (Spell objspell in _objCharacter.Spells)
            {
                if (objspell.Alchemical)
                {
                    intAlchPrepCount++;
                }
                else if (objspell.Category == "Rituals")
                {
                    intRitualCount++;
                }
                else
                {
                    intSpellCount++;
                }
            }
            if (!await _objCharacter.GetIgnoreRulesAsync(token))
            {
                if (!await _objCharacter.GetCreatedAsync(token))
                {
                    int intSpellLimit = await (await _objCharacter.GetAttributeAsync("MAG", token: token)).GetTotalValueAsync(token) * 2;
                    if (await chkAlchemical.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        if (intAlchPrepCount >= intSpellLimit)
                        {
                            Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit", token: token), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit", token: token), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (objXmlSpell?.SelectSingleNode("category")?.Value == "Rituals")
                    {
                        if (intRitualCount >= intSpellLimit)
                        {
                            Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit", token: token), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit", token: token), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (intSpellCount >= intSpellLimit)
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit", token: token),
                            await LanguageManager.GetStringAsync("MessageTitle_SpellLimit", token: token), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                if (!objXmlSpell.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync("String_DescSpell", token: token)))
                {
                    return;
                }
            }

            _strSelectedSpell = strSelectedItem;
            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token) == 0)
                ? await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                : _xmlBaseSpellDataNode.SelectSingleNode("/chummer/spells/spell[id = " + _strSelectedSpell.CleanXPath() + "]/category")?.Value ?? string.Empty;
            FreeBonus = await chkFreeBonus.DoThreadSafeFuncAsync(x => x.Checked, token: token);
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token);
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        private async ValueTask UpdateSpellInfo(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            XPathNavigator xmlSpell = null;
            string strSelectedSpellId = await lstSpells.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
            _blnRefresh = true;
            await chkExtended.DoThreadSafeAsync(x =>
            {
                if (!x.Visible && x.Checked)
                {
                    x.Checked = false;
                }
            }, token: token);
            if (!string.IsNullOrEmpty(strSelectedSpellId))
            {
                xmlSpell = _xmlBaseSpellDataNode.SelectSingleNode("/chummer/spells/spell[id = " + strSelectedSpellId.CleanXPath() + ']');
            }
            if (xmlSpell == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token);
                return;
            }

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);

            bool blnExtendedFound = false;
            bool blnAlchemicalFound = false;
            string strDescriptors = xmlSpell.SelectSingleNode("descriptor")?.Value;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDescriptors))
            {
                if (!string.IsNullOrEmpty(strDescriptors))
                {
                    foreach (string strDescriptor in strDescriptors.SplitNoAlloc(
                                 ',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        switch (strDescriptor.Trim())
                        {
                            case "Alchemical Preparation":
                                blnAlchemicalFound = true;
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescAlchemicalPreparation", token: token));
                                break;

                            case "Extended Area":
                                blnExtendedFound = true;
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescExtendedArea", token: token));
                                break;

                            case "Material Link":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescMaterialLink", token: token));
                                break;

                            case "Multi-Sense":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescMultiSense", token: token));
                                break;

                            case "Organic Link":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescOrganicLink", token: token));
                                break;

                            case "Single-Sense":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescSingleSense", token: token));
                                break;

                            default:
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_Desc" + strDescriptor.Trim(), token: token));
                                break;
                        }

                        sbdDescriptors.Append(',').Append(strSpace);
                    }
                }

                if (blnAlchemicalFound)
                {
                    await chkAlchemical.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Enabled = false;
                        x.Checked = true;
                    }, token: token);
                }
                else if (xmlSpell.SelectSingleNode("category")?.Value == "Rituals")
                {
                    await chkAlchemical.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, token: token);
                }
                else
                {
                    await chkAlchemical.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Enabled = true;
                    }, token: token);
                }

                // If Extended Area was not found and the Extended checkbox is checked, add Extended Area to the list of Descriptors.
                if (await chkExtended.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !blnExtendedFound)
                {
                    sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescExtendedArea", token: token)).Append(',')
                                  .Append(strSpace);
                }

                if (await chkAlchemical.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !blnAlchemicalFound)
                {
                    sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescAlchemicalPreparation", token: token)).Append(',')
                                  .Append(strSpace);
                }

                // Remove the trailing comma.
                if (sbdDescriptors.Length > 2)
                    sbdDescriptors.Length -= 2;
                strDescriptors = sbdDescriptors.ToString();
            }

            if (string.IsNullOrEmpty(strDescriptors))
                strDescriptors = await LanguageManager.GetStringAsync("String_None", token: token);
            await lblDescriptors.DoThreadSafeAsync(x => x.Text = strDescriptors, token: token);
            await lblDescriptorsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDescriptors), token: token);

            string strType;
            switch (xmlSpell.SelectSingleNode("type")?.Value)
            {
                case "M":
                    strType = await LanguageManager.GetStringAsync("String_SpellTypeMana", token: token);
                    break;

                default:
                    strType = await LanguageManager.GetStringAsync("String_SpellTypePhysical", token: token);
                    break;
            }
            await lblType.DoThreadSafeAsync(x => x.Text = strType, token: token);
            await lblTypeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strType), token: token);

            string strDuration;
            switch (xmlSpell.SelectSingleNode("duration")?.Value)
            {
                case "P":
                    strDuration = await LanguageManager.GetStringAsync("String_SpellDurationPermanent", token: token);
                    break;

                case "S":
                    strDuration = await LanguageManager.GetStringAsync("String_SpellDurationSustained", token: token);
                    break;

                default:
                    strDuration = await LanguageManager.GetStringAsync("String_SpellDurationInstant", token: token);
                    break;
            }

            await lblDuration.DoThreadSafeAsync(x => x.Text = strDuration, token: token);
            await lblDurationLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDuration), token: token);

            if (blnExtendedFound)
            {
                await chkExtended.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.Checked = true;
                    x.Enabled = false;
                }, token: token);
            }
            else if (_objCharacter.Settings.ExtendAnyDetectionSpell && xmlSpell.SelectSingleNode("category")?.Value == "Detection")
            {
                await chkExtended.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    if (!x.Enabled) // Resets this checkbox if we just selected an Extended Area spell
                        x.Checked = false;
                    x.Enabled = true;
                }, token: token);
            }
            else
            {
                await chkExtended.DoThreadSafeAsync(x =>
                {
                    x.Checked = false;
                    x.Visible = false;
                }, token: token);
            }

            string strRange = xmlSpell.SelectSingleNode("range")?.Value ?? string.Empty;
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strRange = await strRange
                    .CheapReplaceAsync("Self", () => LanguageManager.GetStringAsync("String_SpellRangeSelf", token: token), token: token)
                    .CheapReplaceAsync("LOS", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfSight", token: token), token: token)
                    .CheapReplaceAsync("LOI", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfInfluence", token: token), token: token)
                    .CheapReplaceAsync("Touch", () => LanguageManager.GetStringAsync("String_SpellRangeTouchLong", token: token), token: token)
                    .CheapReplaceAsync("T", () => LanguageManager.GetStringAsync("String_SpellRangeTouch", token: token), token: token)
                    .CheapReplaceAsync("(A)", async () => '(' + await LanguageManager.GetStringAsync("String_SpellRangeArea", token: token) + ')', token: token)
                    .CheapReplaceAsync("MAG", () => LanguageManager.GetStringAsync("String_AttributeMAGShort", token: token), token: token);
            }
            await lblRange.DoThreadSafeAsync(x => x.Text = strRange, token: token);
            await lblRangeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strRange), token: token);

            switch (xmlSpell.SelectSingleNode("damage")?.Value)
            {
                case "P":
                    await lblDamageLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    await LanguageManager.GetStringAsync("String_DamagePhysical", token: token)
                                         .ContinueWith(y => lblDamage.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    break;

                case "S":
                    await lblDamageLabel.DoThreadSafeAsync(x => x.Visible = true, token: token);
                    await LanguageManager.GetStringAsync("String_DamageStun", token: token)
                                         .ContinueWith(y => lblDamage.DoThreadSafeAsync(x => x.Text = y.Result, token: token), token)
                                         .Unwrap();
                    break;

                default:
                    await lblDamageLabel.DoThreadSafeAsync(x => x.Visible = false, token: token);
                    await lblDamage.DoThreadSafeAsync(x => x.Text = string.Empty, token: token);
                    break;
            }

            string strDV = xmlSpell.SelectSingleNode("dv")?.Value.Replace('/', '÷').Replace('*', '×') ?? string.Empty;
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strDV = await strDV.CheapReplaceAsync("F", () => LanguageManager.GetStringAsync("String_SpellForce", token: token), token: token)
                    .CheapReplaceAsync("Overflow damage", () => LanguageManager.GetStringAsync("String_SpellOverflowDamage", token: token), token: token)
                    .CheapReplaceAsync("Damage Value", () => LanguageManager.GetStringAsync("String_SpellDamageValue", token: token), token: token)
                    .CheapReplaceAsync("Toxin DV", () => LanguageManager.GetStringAsync("String_SpellToxinDV", token: token), token: token)
                    .CheapReplaceAsync("Disease DV", () => LanguageManager.GetStringAsync("String_SpellDiseaseDV", token: token), token: token)
                    .CheapReplaceAsync("Radiation Power",
                        () => LanguageManager.GetStringAsync("String_SpellRadiationPower", token: token), token: token);
            }

            bool force = strDV.StartsWith('F');
            strDV = strDV.TrimStartOnce('F');
            //Navigator can't do math on a single value, so inject a mathable value.
            if (string.IsNullOrEmpty(strDV))
            {
                strDV = "0";
            }
            else
            {
                int intPos = strDV.IndexOf('-');
                if (intPos != -1)
                {
                    strDV = strDV.Substring(intPos);
                }
                else
                {
                    intPos = strDV.IndexOf('+');
                    if (intPos != -1)
                    {
                        strDV = strDV.Substring(intPos);
                    }
                }
            }

            if (Limited)
            {
                strDV += " + -2";
            }
            if (Extended && !blnExtendedFound)
            {
                strDV += " + 2";
            }
            object xprResult = CommonFunctions.EvaluateInvariantXPath(strDV.TrimStart('+'), out bool blnIsSuccess);
            if (blnIsSuccess)
            {
                if (force)
                {
                    strDV = string.Format(GlobalSettings.CultureInfo, "F{0:+0;-0;}", xprResult);
                }
                else if (xprResult.ToString() != "0")
                {
                    strDV += xprResult;
                }
            }

            await lblDV.DoThreadSafeAsync(x => x.Text = strDV, token: token);
            await lblDVLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDV), token: token);

            if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled && _blnCanTouchOnlySpellBeFree && xmlSpell.SelectSingleNode("range")?.Value == "T")
            {
                await chkFreeBonus.DoThreadSafeAsync(x =>
                {
                    x.Checked = true;
                    x.Visible = true;
                    x.Enabled = false;
                }, token: token);
            }
            else
            {
                await chkFreeBonus.DoThreadSafeAsync(x =>
                {
                    x.Checked = FreeOnly;
                    x.Visible = _blnCanGenericSpellBeFree
                                || (_blnCanTouchOnlySpellBeFree && xmlSpell.SelectSingleNode("range")?.Value == "T");
                    x.Enabled = FreeOnly;
                }, token: token);
            }

            string strSource = xmlSpell.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
            string strPage = (await xmlSpell.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token))?.Value ?? xmlSpell.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                             GlobalSettings.CultureInfo, _objCharacter, token: token);
            await objSource.SetControlAsync(lblSource, token: token);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()), token: token);
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token);
            _blnRefresh = false;
        }

        #endregion Methods
    }
}
