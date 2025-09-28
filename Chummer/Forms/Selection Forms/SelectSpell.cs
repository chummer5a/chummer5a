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
        private bool _blnLimited;
        private bool _blnExtended;
        private bool _blnAlchemical;
        private bool _blnFreeBonus;
        private bool _blnIgnoreRequirements;
        private string _strLimitCategory = string.Empty;
        private string _strForceSpell = string.Empty;
        private bool _blnCanGenericSpellBeFree;
        private bool _blnCanTouchOnlySpellBeFree;
        private static string _strSelectCategory = string.Empty;

        private readonly XPathNavigator _xmlBaseSpellDataNode;
        private readonly Character _objCharacter;
        private List<ListItem> _lstCategory;
        private bool _blnRefresh;

        #region Control Events

        public SelectSpell(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Load the Spells information.
            _xmlBaseSpellDataNode = _objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _lstCategory = Utils.ListItemListPool.Get();
        }

        private async void SelectSpell_Load(object sender, EventArgs e)
        {
            await chkLimited.SetToolTipTextAsync(await LanguageManager.GetStringAsync("Tip_SelectSpell_LimitedSpell").ConfigureAwait(false)).ConfigureAwait(false);
            await chkExtended.SetToolTipTextAsync(await LanguageManager.GetStringAsync("Tip_SelectSpell_ExtendedSpell").ConfigureAwait(false)).ConfigureAwait(false);
            // If a value is forced, set the name of the spell and accept the form.
            if (!string.IsNullOrEmpty(_strForceSpell))
            {
                _strSelectedSpell = _strForceSpell;
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }).ConfigureAwait(false);
            }
            (bool blnCanTouchOnlySpellBeFree, bool blnCanGenericSpellBeFree) = await _objCharacter.AllowFreeSpellsAsync().ConfigureAwait(false);
            _blnCanTouchOnlySpellBeFree = blnCanTouchOnlySpellBeFree;
            _blnCanGenericSpellBeFree = blnCanGenericSpellBeFree;
            
            await txtSearch.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseSpellDataNode.SelectAndCacheExpression(
                         "categories/category"))
            {
                string strCategory = objXmlCategory.Value;
                if (!string.IsNullOrEmpty(_strLimitCategory) && strCategory != _strLimitCategory)
                    continue;
                if (!await AnyItemInList(strCategory).ConfigureAwait(false))
                    continue;
                _lstCategory.Add(new ListItem(strCategory,
                                              objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                              ?? strCategory));
            }

            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count != 1)
            {
                _lstCategory.Insert(0,
                    new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }

            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);
            // Select the first Category in the list.
            await cboCategory.DoThreadSafeAsync(x =>
            {
                if (string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedIndex = 0;
                else
                    x.SelectedValue = _strSelectCategory;
                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);

            // Don't show the Extended Spell checkbox if the option to Extend any Detection Spell is disabled.
            bool blnExtendedVisible = await (await _objCharacter.GetSettingsAsync()).GetExtendAnyDetectionSpellAsync().ConfigureAwait(false);
            await chkExtended.DoThreadSafeAsync(x => x.Visible = blnExtendedVisible).ConfigureAwait(false);
            
            _blnLoading = false;
            await BuildSpellList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async void lstSpells_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateSpellInfo().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await BuildSpellList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildSpellList(await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm().ConfigureAwait(false);
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
                txtSearch.Select(txtSearch.TextLength, 0);
        }

        private async void chkExtended_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh)
                return;
            await UpdateSpellInfo().ConfigureAwait(false);
        }

        private async void chkLimited_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnRefresh)
                return;
            await UpdateSpellInfo().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether a Limited version of the Spell was selected.
        /// </summary>
        public bool Limited => _blnLimited;

        /// <summary>
        /// Whether an Extended version of the Spell was selected.
        /// </summary>
        public bool Extended => _blnExtended;

        /// <summary>
        /// Whether a Alchemical version of the Spell was selected.
        /// </summary>
        public bool Alchemical => _blnAlchemical;

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

        public bool FreeBonus => _blnFreeBonus;

        #endregion Properties

        #region Methods

        private Task<bool> AnyItemInList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, false, token);
        }

        private Task<bool> BuildSpellList(string strCategory = "", CancellationToken token = default)
        {
            return RefreshList(strCategory, true, token);
        }

        private async Task<bool> RefreshList(string strCategory, bool blnDoUIUpdate, CancellationToken token = default)
        {
            if (_blnLoading && blnDoUIUpdate)
                return false;
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    await lstSpells.PopulateWithListItemAsync(ListItem.Blank, token: token).ConfigureAwait(false);
                }
                return false;
            }

            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            bool blnHasSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength != 0, token: token).ConfigureAwait(false);

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSpellItems))
            {
                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> limitDescriptors))
                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> blockDescriptors))
                {
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append('(').Append(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');
                        if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                               && (GlobalSettings.SearchInCategoryOnly
                                                                   || !blnHasSearch))
                            sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                        else
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdCategoryFilter))
                            {
                                foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                                {
                                    if (!string.IsNullOrEmpty(strItem))
                                    {
                                        sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath())
                                                         .Append(" or ");
                                    }
                                }

                                if (sbdCategoryFilter.Length > 0)
                                {
                                    sbdCategoryFilter.Length -= 4;
                                    sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                                }
                            }
                        }

                        if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetExtendAnyDetectionSpellAsync(token).ConfigureAwait(false))
                            sbdFilter.Append(" and ((not(contains(name, \", Extended\"))))");
                        if (!string.IsNullOrEmpty(strSearch))
                            sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(strSearch));

                        // Populate the Spell list.
                        if (!_blnIgnoreRequirements)
                        {
                            foreach (Improvement improvement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                         _objCharacter, Improvement.ImprovementType.LimitSpellDescriptor, token: token).ConfigureAwait(false))
                            {
                                limitDescriptors.Add(improvement.ImprovedName);
                            }

                            foreach (Improvement improvement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                         _objCharacter, Improvement.ImprovementType.BlockSpellDescriptor, token: token).ConfigureAwait(false))
                            {
                                blockDescriptors.Add(improvement.ImprovedName);
                            }
                        }

                        if (sbdFilter.Length > 0)
                            strFilter = "[" + sbdFilter.Append(']').ToString();
                    }

                    foreach (XPathNavigator objXmlSpell in _xmlBaseSpellDataNode.Select("spells/spell" + strFilter))
                    {
                        string strSpellCategory = objXmlSpell.SelectSingleNodeAndCacheExpression("category", token)?.Value ?? string.Empty;
                        if (!_blnIgnoreRequirements)
                        {
                            if (!await objXmlSpell.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                                continue;

                            if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.AllowSpellCategory,
                                    strSpellCategory, token: token).ConfigureAwait(false)).Count != 0)
                            {
                                if (!blnDoUIUpdate)
                                    return true;
                                await AddSpell(objXmlSpell, strSpellCategory).ConfigureAwait(false);
                                continue;
                            }

                            string strRange = objXmlSpell.SelectSingleNodeAndCacheExpression("range", token)?.Value ?? string.Empty;
                            if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.AllowSpellRange,
                                    strRange, token: token).ConfigureAwait(false)).Count != 0)
                            {
                                if (!blnDoUIUpdate)
                                    return true;
                                await AddSpell(objXmlSpell, strSpellCategory).ConfigureAwait(false);
                                continue;
                            }

                            string strDescriptor
                                = objXmlSpell.SelectSingleNodeAndCacheExpression("descriptor", token)?.Value ?? string.Empty;

                            if (limitDescriptors.Count != 0
                                && !limitDescriptors.Any(l => strDescriptor.Contains(l)))
                                continue;

                            if (blockDescriptors.Count != 0 && blockDescriptors.Any(l => strDescriptor.Contains(l)))
                                continue;

                            if ((await ImprovementManager
                                       .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                           Improvement.ImprovementType.LimitSpellCategory, token: token).ConfigureAwait(false))
                                .Exists(x => x.ImprovedName != strSpellCategory))
                            {
                                continue;
                            }
                        }

                        if (!blnDoUIUpdate)
                            return true;
                        await AddSpell(objXmlSpell, strSpellCategory).ConfigureAwait(false);
                    }
                }

                if (blnDoUIUpdate)
                {
                    lstSpellItems.Sort(CompareListItems.CompareNames);
                    string strOldSelected = await lstSpells.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    _blnLoading = true;
                    await lstSpells.PopulateWithListItemsAsync(lstSpellItems, token: token).ConfigureAwait(false);
                    _blnLoading = false;
                    await lstSpells.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strOldSelected))
                            x.SelectedValue = strOldSelected;
                        else
                            x.SelectedIndex = -1;
                    }, token: token).ConfigureAwait(false);
                }

                async ValueTask AddSpell(XPathNavigator objXmlSpell, string strSpellCategory)
                {
                    string strDisplayName = objXmlSpell.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ??
                                            objXmlSpell.SelectSingleNodeAndCacheExpression("name", token)?.Value ??
                                            await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    if (!GlobalSettings.SearchInCategoryOnly && blnHasSearch
                                                             && !string.IsNullOrEmpty(strSpellCategory))
                    {
                        ListItem objFoundItem
                            = _lstCategory.Find(objFind => objFind.Value.ToString() == strSpellCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += strSpace + "[" + objFoundItem.Name + "]";
                        }
                    }

                    lstSpellItems.Add(new ListItem(objXmlSpell.SelectSingleNodeAndCacheExpression("id", token)?.Value ?? string.Empty,
                                                   strDisplayName));
                }

                return lstSpellItems.Count > 0;
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strSelectedItem = await lstSpells.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedItem))
                return;

            // Display the Spell information.
            
            // Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
            // The maximum number of Spells a character can start with is 2 x (highest of Spellcasting or Ritual Spellcasting Skill).
            int intSpellCount = 0;
            int intRitualCount = 0;
            int intAlchPrepCount = 0;

            await _objCharacter.Spells.ForEachAsync(objSpell =>
            {
                if (objSpell.Alchemical)
                {
                    intAlchPrepCount++;
                }
                else if (objSpell.Category == "Rituals")
                {
                    intRitualCount++;
                }
                else
                {
                    intSpellCount++;
                }
            }, token).ConfigureAwait(false);
            if (!await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
            {
                XPathNavigator objXmlSpell = _xmlBaseSpellDataNode.TryGetNodeByNameOrId("spells/spell", strSelectedItem);
                if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    int intSpellLimit = await (await _objCharacter.GetAttributeAsync("MAG", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false) * 2;
                    if (await chkAlchemical.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    {
                        if (intAlchPrepCount >= intSpellLimit)
                        {
                            await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_SpellLimit", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                            return;
                        }
                    }
                    else if (objXmlSpell?.SelectSingleNodeAndCacheExpression("category", token)?.Value == "Rituals")
                    {
                        if (intRitualCount >= intSpellLimit)
                        {
                            await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_SpellLimit", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                            return;
                        }
                    }
                    else if (intSpellCount >= intSpellLimit)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_SpellLimit", token: token).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageTitle_SpellLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        return;
                    }
                }
                if (!await objXmlSpell.RequirementsMetAsync(_objCharacter, strLocalName: await LanguageManager.GetStringAsync("String_DescSpell", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                {
                    return;
                }
            }

            _strSelectedSpell = strSelectedItem;
            if (GlobalSettings.SearchInCategoryOnly
                || await txtSearch.DoThreadSafeFuncAsync(x => x.TextLength, token: token).ConfigureAwait(false) == 0)
            {
                _strSelectCategory = await cboCategory
                                           .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                           .ConfigureAwait(false);
            }
            else
            {
                XPathNavigator xmlSpellNode
                    = _xmlBaseSpellDataNode.TryGetNodeByNameOrId("/chummer/spells/spell", _strSelectedSpell);
                _strSelectCategory = xmlSpellNode != null
                    ? xmlSpellNode.SelectSingleNodeAndCacheExpression("category", token)?.Value ?? string.Empty
                    : string.Empty;
            }

            _blnLimited = await chkLimited.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            _blnExtended = await chkExtended.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            _blnAlchemical = await chkAlchemical.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);

            _blnFreeBonus = await chkFreeBonus.DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                          .ConfigureAwait(false);

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

        private async Task UpdateSpellInfo(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            XPathNavigator xmlSpell = null;
            string strSelectedSpellId = await lstSpells.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            _blnRefresh = true;
            await chkExtended.DoThreadSafeAsync(x =>
            {
                if (!x.Visible && x.Checked)
                {
                    x.Checked = false;
                }
            }, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedSpellId))
            {
                xmlSpell = _xmlBaseSpellDataNode.TryGetNodeByNameOrId("/chummer/spells/spell", strSelectedSpellId);
            }
            if (xmlSpell == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                return;
            }

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);

            bool blnExtendedFound = false;
            bool blnAlchemicalFound = false;
            string strDescriptors = xmlSpell.SelectSingleNodeAndCacheExpression("descriptor", token)?.Value;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDescriptors))
            {
                if (!string.IsNullOrEmpty(strDescriptors))
                {
                    foreach (string strDescriptor in strDescriptors.SplitNoAlloc(
                                 ',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string strTrimmedDescriptor = strDescriptor.Trim();
                        switch (strTrimmedDescriptor.ToUpperInvariant())
                        {
                            case "ALCHEMICAL PREPARATION":
                                blnAlchemicalFound = true;
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescAlchemicalPreparation", token: token).ConfigureAwait(false));
                                break;

                            case "EXTENDED AREA":
                                blnExtendedFound = true;
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescExtendedArea", token: token).ConfigureAwait(false));
                                break;

                            case "MATERIAL LINK":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescMaterialLink", token: token).ConfigureAwait(false));
                                break;

                            case "MULTI-SENSE":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescMultiSense", token: token).ConfigureAwait(false));
                                break;

                            case "ORGANIC LINK":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescOrganicLink", token: token).ConfigureAwait(false));
                                break;

                            case "SINGLE-SENSE":
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescSingleSense", token: token).ConfigureAwait(false));
                                break;

                            default:
                                sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_Desc" + strTrimmedDescriptor, token: token).ConfigureAwait(false));
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
                    }, token: token).ConfigureAwait(false);
                }
                else if (xmlSpell.SelectSingleNodeAndCacheExpression("category", token)?.Value == "Rituals")
                {
                    await chkAlchemical.DoThreadSafeAsync(x =>
                    {
                        x.Visible = false;
                        x.Checked = false;
                    }, token: token).ConfigureAwait(false);
                }
                else
                {
                    await chkAlchemical.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Enabled = true;
                    }, token: token).ConfigureAwait(false);
                }

                if (blnExtendedFound)
                {
                    await chkExtended.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Checked = true;
                        x.Enabled = false;
                    }, token: token).ConfigureAwait(false);
                }
                else if (xmlSpell.SelectSingleNodeAndCacheExpression("category", token)?.Value == "Detection"
                         && await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetExtendAnyDetectionSpellAsync(token).ConfigureAwait(false))
                {
                    // If Extended Area was not found and the Extended checkbox is checked, add Extended Area to the list of Descriptors.
                    if (await chkExtended.DoThreadSafeFuncAsync(x =>
                        {
                            x.Visible = true;
                            if (!x.Enabled) // Resets this checkbox if we just selected an Extended Area spell
                                x.Checked = false;
                            x.Enabled = true;
                            return x.Checked;
                        }, token: token).ConfigureAwait(false))
                    {
                        sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescExtendedArea", token: token).ConfigureAwait(false)).Append(',')
                            .Append(strSpace);
                    }
                }
                else
                {
                    await chkExtended.DoThreadSafeAsync(x =>
                    {
                        x.Checked = false;
                        x.Visible = false;
                    }, token: token).ConfigureAwait(false);
                }

                if (await chkAlchemical.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !blnAlchemicalFound)
                {
                    sbdDescriptors.Append(await LanguageManager.GetStringAsync("String_DescAlchemicalPreparation", token: token).ConfigureAwait(false)).Append(',')
                                  .Append(strSpace);
                }

                // Remove the trailing comma.
                if (sbdDescriptors.Length > 2)
                    sbdDescriptors.Length -= 2;
                strDescriptors = sbdDescriptors.ToString();
            }

            string strSelectedSpellName = xmlSpell.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
            string strSelectedSpellCategory = xmlSpell.SelectSingleNodeAndCacheExpression("category", token)?.Value ?? string.Empty;
            List<Improvement> lstDrainRelevantImprovements = new List<Improvement>(await _objCharacter.Improvements.GetCountAsync(token).ConfigureAwait(false));
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                              out HashSet<string> setDescriptors))
            {
                foreach (string strDescriptor in strDescriptors.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    setDescriptors.Add(strDescriptor);
                await _objCharacter.Improvements.ForEachWithBreakAsync(objImprovement =>
                {
                    if (!objImprovement.Enabled)
                        return true;

                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SpellCategoryDrain:
                            if (objImprovement.ImprovedName == strSelectedSpellCategory)
                            {
                                lstDrainRelevantImprovements.Add(objImprovement);
                            }

                            break;

                        case Improvement.ImprovementType.SpellDescriptorDrain:
                            if (setDescriptors.Count > 0)
                            {
                                bool blnAllow = false;
                                foreach (string strDescriptor in objImprovement.ImprovedName.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (strDescriptor.StartsWith("NOT", StringComparison.Ordinal))
                                    {
                                        if (setDescriptors.Contains(
                                                strDescriptor.TrimStartOnce("NOT(").TrimEndOnce(')')))
                                        {
                                            blnAllow = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        blnAllow = setDescriptors.Contains(strDescriptor);
                                    }
                                }

                                if (blnAllow)
                                {
                                    lstDrainRelevantImprovements.Add(objImprovement);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.DrainValue:
                            {
                                if (string.IsNullOrEmpty(objImprovement.ImprovedName)
                                    || objImprovement.ImprovedName == strSelectedSpellName)
                                {
                                    lstDrainRelevantImprovements.Add(objImprovement);
                                }
                            }
                            break;
                    }

                    return true;
                }, token: token).ConfigureAwait(false);
            }
            if (string.IsNullOrEmpty(strDescriptors))
                strDescriptors = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
            await lblDescriptors.DoThreadSafeAsync(x => x.Text = strDescriptors, token: token).ConfigureAwait(false);
            await lblDescriptorsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDescriptors), token: token).ConfigureAwait(false);

            string strType;
            switch (xmlSpell.SelectSingleNodeAndCacheExpression("type", token)?.Value.ToUpperInvariant())
            {
                case "M":
                    strType = await LanguageManager.GetStringAsync("String_SpellTypeMana", token: token).ConfigureAwait(false);
                    break;

                default:
                    strType = await LanguageManager.GetStringAsync("String_SpellTypePhysical", token: token).ConfigureAwait(false);
                    break;
            }
            await lblType.DoThreadSafeAsync(x => x.Text = strType, token: token).ConfigureAwait(false);
            await lblTypeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strType), token: token).ConfigureAwait(false);

            string strDuration;
            switch (xmlSpell.SelectSingleNodeAndCacheExpression("duration", token)?.Value.ToUpperInvariant())
            {
                case "P":
                    strDuration = await LanguageManager.GetStringAsync("String_SpellDurationPermanent", token: token).ConfigureAwait(false);
                    break;

                case "S":
                    strDuration = await LanguageManager.GetStringAsync("String_SpellDurationSustained", token: token).ConfigureAwait(false);
                    break;

                default:
                    strDuration = await LanguageManager.GetStringAsync("String_SpellDurationInstant", token: token).ConfigureAwait(false);
                    break;
            }

            await lblDuration.DoThreadSafeAsync(x => x.Text = strDuration, token: token).ConfigureAwait(false);
            await lblDurationLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDuration), token: token).ConfigureAwait(false);

            string strRange = xmlSpell.SelectSingleNodeAndCacheExpression("range", token)?.Value ?? string.Empty;
            bool blnBarehandedAdept;
            if (await _objCharacter.GetAdeptEnabledAsync(token).ConfigureAwait(false) && !await _objCharacter.GetMagicianEnabledAsync(token).ConfigureAwait(false)
                && _blnCanTouchOnlySpellBeFree && (strRange == "T" || strRange == "T (A)"))
            {
                await chkFreeBonus.DoThreadSafeAsync(x =>
                {
                    x.Checked = true;
                    x.Visible = true;
                    x.Enabled = false;
                }, token: token).ConfigureAwait(false);
                blnBarehandedAdept = true;

                // Show/hide Limited checkbox based on settings
                bool blnLimitedVisible = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetAllowLimitedSpellsForBareHandedAdeptAsync(token).ConfigureAwait(false);

                await chkLimited.DoThreadSafeAsync(x =>
                {
                    x.Enabled = blnLimitedVisible;
                    if (!x.Enabled)
                        x.Checked = false;
                }, token: token).ConfigureAwait(false);
            }
            else
            {
                bool blnVisible = _blnCanGenericSpellBeFree || (_blnCanTouchOnlySpellBeFree && (strRange == "T" || strRange == "T (A)"));
                await chkFreeBonus.DoThreadSafeAsync(x =>
                {
                    x.Checked = FreeOnly;
                    x.Visible = blnVisible;
                    x.Enabled = FreeOnly;
                }, token: token).ConfigureAwait(false);
                blnBarehandedAdept = false;
                await chkLimited.DoThreadSafeAsync(x =>
                {
                    x.Enabled = true;
                }, token: token).ConfigureAwait(false);
            }

            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strRange = await strRange
                                 .CheapReplaceAsync("Self", () => LanguageManager.GetStringAsync("String_SpellRangeSelf", token: token), token: token)
                                 .CheapReplaceAsync("LOS", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfSight", token: token), token: token)
                                 .CheapReplaceAsync("LOI", () => LanguageManager.GetStringAsync("String_SpellRangeLineOfInfluence", token: token), token: token)
                                 .CheapReplaceAsync("Touch", () => LanguageManager.GetStringAsync("String_SpellRangeTouchLong", token: token), token: token)
                                 .CheapReplaceAsync("T", () => LanguageManager.GetStringAsync("String_SpellRangeTouch", token: token), token: token)
                                 .CheapReplaceAsync("(A)", async () => "(" + await LanguageManager.GetStringAsync("String_SpellRangeArea", token: token).ConfigureAwait(false) + ")", token: token)
                                 .CheapReplaceAsync("MAG", () => LanguageManager.GetStringAsync("String_AttributeMAGShort", token: token), token: token).ConfigureAwait(false);
            }
            await lblRange.DoThreadSafeAsync(x => x.Text = strRange, token: token).ConfigureAwait(false);
            await lblRangeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strRange), token: token).ConfigureAwait(false);

            switch (xmlSpell.SelectSingleNodeAndCacheExpression("damage", token)?.Value.ToUpperInvariant())
            {
                case "P":
                {
                    await lblDamageLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                    string strDamage = await LanguageManager.GetStringAsync("String_DamagePhysical", token: token).ConfigureAwait(false);
                    await lblDamage.DoThreadSafeAsync(x => x.Text = strDamage, token: token).ConfigureAwait(false);
                }
                    break;

                case "S":
                {
                    await lblDamageLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                    string strDamage = await LanguageManager.GetStringAsync("String_DamageStun", token: token).ConfigureAwait(false);
                    await lblDamage.DoThreadSafeAsync(x => x.Text = strDamage, token: token).ConfigureAwait(false);
                }
                    break;

                default:
                    await lblDamageLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                    await lblDamage.DoThreadSafeAsync(x => x.Text = string.Empty, token: token).ConfigureAwait(false);
                    break;
            }

            string strDv = xmlSpell.SelectSingleNodeAndCacheExpression("dv", token)?.Value.Replace('/', '÷').Replace('*', '×') ?? string.Empty;
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strDv = await strDv.CheapReplaceAsync("F", () => LanguageManager.GetStringAsync("String_SpellForce", token: token), token: token)
                                   .CheapReplaceAsync("Overflow damage", () => LanguageManager.GetStringAsync("String_SpellOverflowDamage", token: token), token: token)
                                   .CheapReplaceAsync("Damage Value", () => LanguageManager.GetStringAsync("String_SpellDamageValue", token: token), token: token)
                                   .CheapReplaceAsync("Toxin DV", () => LanguageManager.GetStringAsync("String_SpellToxinDV", token: token), token: token)
                                   .CheapReplaceAsync("Disease DV", () => LanguageManager.GetStringAsync("String_SpellDiseaseDV", token: token), token: token)
                                   .CheapReplaceAsync("Radiation Power",
                                                      () => LanguageManager.GetStringAsync("String_SpellRadiationPower", token: token), token: token).ConfigureAwait(false);
            }

            bool blnForce = strDv.StartsWith('F');
            strDv = blnForce ? strDv.TrimStartOnce("F", true) : strDv;
            //Navigator can't do math on a single value, so inject a mathable value.
            if (string.IsNullOrEmpty(strDv))
            {
                strDv = "0";
            }
            else
            {
                int intPos = strDv.IndexOf('-');
                if (intPos != -1)
                {
                    strDv = strDv.Substring(intPos);
                }
                else
                {
                    intPos = strDv.IndexOf('+');
                    if (intPos != -1)
                    {
                        strDv = strDv.Substring(intPos);
                    }
                }
            }

            string strToAppend = string.Empty;
            int intDrainDv = 0;
            if (strDv.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDv))
                {
                    sbdDv.Append('(').Append(strDv).Append(')');
                    foreach (Improvement objImprovement in lstDrainRelevantImprovements)
                    {
                        sbdDv.Append(" + (").Append(objImprovement.Value.ToString(GlobalSettings.InvariantCultureInfo)).Append(')');
                    }

                    if (await chkLimited.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                    {
                        sbdDv.Append("-2");
                    }
                    if (!blnExtendedFound && await chkExtended.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                    {
                        sbdDv.Append("+2");
                    }

                    if (blnBarehandedAdept && !blnForce)
                    {
                        sbdDv.Insert(0, "2 * (").Append(')');
                    }

                    await _objCharacter.ProcessAttributesInXPathAsync(sbdDv, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object xprResult) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdDv.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intDrainDv = ((double)xprResult).StandardRound();
                    else
                        strToAppend = sbdDv.ToString();
                }
            }
            else
            {
                foreach (Improvement objImprovement in lstDrainRelevantImprovements)
                {
                    decValue += objImprovement.Value;
                }
                if (await chkLimited.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                {
                    decValue -= 2;
                }
                if (!blnExtendedFound && await chkExtended.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                {
                    decValue += 2;
                }
                if (blnBarehandedAdept && !blnForce)
                {
                    decValue *= 2;
                }
                intDrainDv = decValue.StandardRound();
            }

            // Drain always minimum 2
            if (!blnForce && string.IsNullOrEmpty(strToAppend))
                intDrainDv = Math.Max(intDrainDv, 2);

            if (blnForce)
            {
                if (!string.IsNullOrEmpty(strToAppend))
                {
                    strDv += "F" + strToAppend;
                    if (blnBarehandedAdept)
                        strDv = "2 * (" + strDv + ")";
                }
                else
                    strDv = string.Format(GlobalSettings.CultureInfo, blnBarehandedAdept ? "2 * (F{0:+0;-0;})" : "F{0:+0;-0;}", intDrainDv);
            }
            else if (!string.IsNullOrEmpty(strToAppend))
            {
                strDv += strToAppend;
                if (blnBarehandedAdept)
                    strDv = "2 * (" + strDv + ")";
            }
            else
                // Drain always minimum 2 (doubled for Barehanded Adept)
                strDv = Math.Max(intDrainDv, blnBarehandedAdept ? 4 : 2).ToString(GlobalSettings.CultureInfo);

            await lblDV.DoThreadSafeAsync(x => x.Text = strDv, token: token).ConfigureAwait(false);
            await lblDVLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDv), token: token).ConfigureAwait(false);

            string strSource = xmlSpell.SelectSingleNodeAndCacheExpression("source", token)?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
            string strPage = xmlSpell.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? xmlSpell.SelectSingleNodeAndCacheExpression("page", token)?.Value ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                             GlobalSettings.CultureInfo, _objCharacter, token: token).ConfigureAwait(false);
            await objSource.SetControlAsync(lblSource, this, token: token).ConfigureAwait(false);
            await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString()), token: token).ConfigureAwait(false);
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
            _blnRefresh = false;
        }

        #endregion Methods
    }
}
