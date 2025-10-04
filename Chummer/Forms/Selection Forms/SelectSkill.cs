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
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class SelectSkill : Form
    {
        private string _strReturnValue = string.Empty;
        private string _strIncludeCategory = string.Empty;
        private string _strExcludeCategory = string.Empty;
        private string _strIncludeSkillGroup = string.Empty;
        private string _strExcludeSkillGroup = string.Empty;
        private string _strLimitToSkill = string.Empty;
        private string _strExcludeSkill = string.Empty;
        private string _strLimitToCategories = string.Empty;
        private string _strForceSkill = string.Empty;
        private readonly string _strSourceName;
        private int _intMinimumRating;
        private int _intMaximumRating = int.MaxValue;

        public string LinkedAttribute { get; set; } = string.Empty;

        private readonly XPathNavigator _objXmlDocument;
        private readonly Character _objCharacter;

        #region Control Events

        public SelectSkill(Character objCharacter, string strSource = "")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _strSourceName = strSource ?? string.Empty;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _objXmlDocument = _objCharacter.LoadDataXPath("skills.xml");
        }

        private async void SelectSkill_Load(object sender, EventArgs e)
        {
            bool blnForcedExotic = false;
            string strForcedExoticSkillName = string.Empty;
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
            {
                // Build the list of non-Exotic Skills from the Skills file.
                XPathNodeIterator objXmlSkillList;
                if (!string.IsNullOrEmpty(_strForceSkill))
                {
                    (blnForcedExotic, strForcedExoticSkillName)
                        = await ExoticSkill.IsExoticSkillNameTupleAsync(_objCharacter, _strForceSkill).ConfigureAwait(false);
                    if (blnForcedExotic)
                    {
                        objXmlSkillList = _objXmlDocument.Select("/chummer/skills/skill[name = "
                                                                 + strForcedExoticSkillName.CleanXPath()
                                                                 + " and exotic = 'True' and ("
                                                                 + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync()
                                                                     .ConfigureAwait(false) + ")]");
                    }
                    else
                    {
                        objXmlSkillList = _objXmlDocument.Select("/chummer/skills/skill[name = "
                                                                 + _strForceSkill.CleanXPath()
                                                                 + " and not(exotic = 'True') and ("
                                                                 + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync()
                                                                     .ConfigureAwait(false) + ")]");
                    }
                }
                else if (!string.IsNullOrEmpty(_strLimitToCategories))
                {
                    objXmlSkillList = _objXmlDocument.Select(
                        "/chummer/skills/skill["
                        + _strLimitToCategories + " and ("
                        + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync()
                                             .ConfigureAwait(false) + ")]");
                }
                else
                {
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        // If we don't have a minimum rating, include exotic skills as normal because they'll just make the second dropdown appear when selected
                        if (_intMinimumRating > 0)
                            sbdFilter.Append("not(exotic = 'True') and ");
                        sbdFilter.Append('(', await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync().ConfigureAwait(false), ')');
                        if (!string.IsNullOrEmpty(_strIncludeCategory))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillCategory in _strIncludeCategory.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                sbdFilter.Append("category = ", strSkillCategory.Trim().CleanXPath(), " or ");
                            }

                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strExcludeCategory))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillCategory in _strExcludeCategory.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                sbdFilter.Append("category != ", strSkillCategory.Trim().CleanXPath(), " and ");
                            }

                            // Remove the trailing " and ".
                            sbdFilter.Length -= 5;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strIncludeSkillGroup))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillGroup in _strIncludeSkillGroup.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                sbdFilter.Append("skillgroup = ", strSkillGroup.Trim().CleanXPath(), " or ");
                            }

                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strExcludeSkillGroup))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillGroup in _strExcludeSkillGroup.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                sbdFilter.Append("skillgroup != ", strSkillGroup.Trim().CleanXPath(), " and ");
                            }

                            // Remove the trailing " and ".
                            sbdFilter.Length -= 5;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(LinkedAttribute))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strAttribute in LinkedAttribute.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                sbdFilter.Append("attribute = ", strAttribute.Trim().CleanXPath(), " or ");
                            }

                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strLimitToSkill))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkill in _strLimitToSkill.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("name = ", strSkill.Trim().CleanXPath(), " or ");
                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strExcludeSkill))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkill in _strExcludeSkill.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("name != ", strSkill.Trim().CleanXPath(), " and ");
                            // Remove the trailing " and ".
                            sbdFilter.Length -= 5;
                            sbdFilter.Append(')');
                        }

                        if (sbdFilter.Length > 0)
                            strFilter = sbdFilter.Insert(0, '[').Append(']').ToString();
                    }

                    objXmlSkillList = _objXmlDocument.Select("/chummer/skills/skill" + strFilter);
                }

                // Add the Skills to the list.
                if (objXmlSkillList.Count > 0)
                {
                    foreach (XPathNavigator objXmlSkill in objXmlSkillList)
                    {
                        string strXmlSkillName = objXmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value;
                        Skill objExistingSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync(strXmlSkillName)
                                                                    .ConfigureAwait(false);
                        if (objExistingSkill == null)
                        {
                            if (_intMinimumRating > 0)
                            {
                                continue;
                            }
                        }
                        else if (objExistingSkill.TotalBaseRating < _intMinimumRating
                                 || objExistingSkill.TotalBaseRating > _intMaximumRating)
                        {
                            continue;
                        }

                        lstSkills.Add(new ListItem(
                                          new ValueTuple<string, bool>(strXmlSkillName, objXmlSkill.SelectSingleNodeAndCacheExpression("exotic")?.Value == bool.TrueString),
                                          objXmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value
                                          ?? strXmlSkillName));
                    }
                }

                // Add in any Exotic Skills the character has if we need a minimum rating.
                if (_intMinimumRating > 0)
                {
                    await cboExtra.DoThreadSafeAsync(x =>
                    {
                        x.AutoCompleteMode = AutoCompleteMode.None;
                        x.DropDownStyle = ComboBoxStyle.DropDownList;
                    }).ConfigureAwait(false);
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setAddedExotics))
                    {
                        await _objCharacter.SkillsSection.Skills.ForEachAsync(async objSkill =>
                        {
                            if (!objSkill.IsExoticSkill)
                                return;
                            string strLoopName = await objSkill.GetNameAsync().ConfigureAwait(false);
                            if (setAddedExotics.Contains(strLoopName))
                                return;
                            if (!(objSkill is ExoticSkill objExoticSkill))
                                return;
                            if (!string.IsNullOrEmpty(_strForceSkill) && _strForceSkill
                                != await objExoticSkill.GetDictionaryKeyAsync().ConfigureAwait(false))
                                return;
                            int intBaseRating = await objSkill.GetTotalBaseRatingAsync().ConfigureAwait(false);
                            if (intBaseRating < _intMinimumRating || intBaseRating > _intMaximumRating)
                                return;
                            if (!string.IsNullOrEmpty(_strIncludeCategory)
                                && !_strIncludeCategory.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                       .Contains(objSkill.SkillCategory))
                            {
                                return;
                            }

                            if (!string.IsNullOrEmpty(_strExcludeCategory)
                                && _strExcludeCategory.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Contains(objSkill.SkillCategory))
                            {
                                return;
                            }

                            if (!string.IsNullOrEmpty(_strIncludeSkillGroup)
                                && !_strIncludeSkillGroup.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                         .Contains(objSkill.SkillGroup))
                            {
                                return;
                            }

                            if (!string.IsNullOrEmpty(_strExcludeSkillGroup)
                                && _strExcludeSkillGroup.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                        .Contains(objSkill.SkillGroup))
                            {
                                return;
                            }

                            if (!string.IsNullOrEmpty(_strLimitToSkill)
                                && !_strLimitToSkill.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                    .Contains(strLoopName))
                            {
                                return;
                            }

                            if (!string.IsNullOrEmpty(_strExcludeSkill)
                                && _strExcludeSkill.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Contains(strLoopName))
                            {
                                return;
                            }

                            lstSkills.Add(new ListItem(new ValueTuple<string, bool>(strLoopName, true),
                                                       await objExoticSkill.GetCurrentDisplayNameAsync()
                                                                           .ConfigureAwait(false)));
                            setAddedExotics.Add(strLoopName);
                        }).ConfigureAwait(false);
                    }
                }

                if (lstSkills.Count == 0)
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager
                                .GetStringAsync("Message_Improvement_EmptySelectionListNamed")
                                .ConfigureAwait(false),
                            _strSourceName)).ConfigureAwait(false);
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.DialogResult = DialogResult.Cancel;
                        x.Close();
                    }).ConfigureAwait(false);
                    return;
                }

                lstSkills.Sort(CompareListItems.CompareNames);
                await cboSkill.PopulateWithListItemsAsync(lstSkills).ConfigureAwait(false);
                // Select the first Skill in the list.
                await cboSkill.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);
            }

            if (await cboSkill.DoThreadSafeFuncAsync(x => x.Items.Count).ConfigureAwait(false) == 1)
            {
                ValueTuple<string, bool> tupSelected
                    = blnForcedExotic
                        ? new ValueTuple<string, bool>(strForcedExoticSkillName, true)
                        : (ValueTuple<string, bool>) await cboSkill.DoThreadSafeFuncAsync(x => x.SelectedValue)
                                                              .ConfigureAwait(false);
                if (!tupSelected.Item2)
                {
                    _strReturnValue = tupSelected.Item1;
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.DialogResult = DialogResult.OK;
                        x.Close();
                    }).ConfigureAwait(false);
                }
                else if (blnForcedExotic || await cboExtra.DoThreadSafeFuncAsync(x => x.DropDownStyle == ComboBoxStyle.DropDownList).ConfigureAwait(false))
                {
                    await BuildExtraList(tupSelected.Item1).ConfigureAwait(false);
                    int intCount = await cboExtra.DoThreadSafeFuncAsync(x => x.Items.Count).ConfigureAwait(false);
                    if (intCount == 0)
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager
                                    .GetStringAsync("Message_Improvement_EmptySelectionListNamed")
                                    .ConfigureAwait(false),
                                _strSourceName)).ConfigureAwait(false);
                        await this.DoThreadSafeAsync(x =>
                        {
                            x.DialogResult = DialogResult.Cancel;
                            x.Close();
                        }).ConfigureAwait(false);
                        return;
                    }

                    if (blnForcedExotic)
                    {
                        _strReturnValue = _strForceSkill;
                        await this.DoThreadSafeAsync(x =>
                        {
                            x.DialogResult = DialogResult.OK;
                            x.Close();
                        }).ConfigureAwait(false);
                    }

                    if (intCount == 1)
                    {
                        _strReturnValue = tupSelected.Item1 + " ("
                                                            + await cboExtra.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? string.Empty)
                                                                            .ConfigureAwait(false) + ")";
                        await this.DoThreadSafeAsync(x =>
                        {
                            x.DialogResult = DialogResult.OK;
                            x.Close();
                        }).ConfigureAwait(false);
                    }
                }
                else
                    await cboSkill.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            ValueTuple<string, bool> tupSelected = (ValueTuple<string, bool>)cboSkill.SelectedValue;
            if (tupSelected.Item2)
                _strReturnValue = tupSelected.Item1 + " (" + (cboExtra.SelectedValue?.ToString() ?? string.Empty) + ")";
            else
                _strReturnValue = tupSelected.Item1;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Only Skills of the selected Category should be in the list.
        /// </summary>
        public string OnlyCategory
        {
            set => _strIncludeCategory = value;
        }

        /// <summary>
        /// Only Skills from the selected Categories should be in the list.
        /// </summary>
        public XmlNode LimitToCategories
        {
            set
            {
                using (XmlNodeList xmlCategoryList = value?.SelectNodes("category"))
                {
                    if (xmlCategoryList == null)
                        return;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdLimitToCategories))
                    {
                        foreach (XmlNode objNode in xmlCategoryList)
                        {
                            sbdLimitToCategories.Append("category = ", objNode.InnerTextViaPool().CleanXPath(), " or ");
                        }

                        // Remove the last " or "
                        if (sbdLimitToCategories.Length > 0)
                            sbdLimitToCategories.Length -= 4;
                        _strLimitToCategories = sbdLimitToCategories.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Only Skills not in the selected Category should be in the list.
        /// </summary>
        public string ExcludeCategory
        {
            set => _strExcludeCategory = value;
        }

        /// <summary>
        /// Only Skills in the selected Skill Group should be in the list.
        /// </summary>
        public string OnlySkillGroup
        {
            set => _strIncludeSkillGroup = value;
        }

        /// <summary>
        /// Restrict the list to only a single Skill.
        /// </summary>
        public string OnlySkill
        {
            set => _strForceSkill = value;
        }

        /// <summary>
        /// Only Skills not in the selected Skill Group should be in the list.
        /// </summary>
        public string ExcludeSkillGroup
        {
            set => _strExcludeSkillGroup = value;
        }

        /// <summary>
        /// Only the provided Skills should be shown in the list.
        /// </summary>
        public string LimitToSkill
        {
            set => _strLimitToSkill = value;
        }

        /// <summary>
        /// Only Skills not among the selected should be in the list.
        /// </summary>
        public string ExcludeSkill
        {
            set => _strExcludeSkill = value;
        }

        /// <summary>
        /// Skill that was selected in the dialogue.
        /// </summary>
        public string SelectedSkill => _strReturnValue;

        /// <summary>
        /// Description to show in the window.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Only show skills with a rating greater than or equal to this
        /// </summary>
        public int MinimumRating
        {
            set => _intMinimumRating = value;
        }

        /// <summary>
        /// Only show skills with a rating less than or equal to this
        /// </summary>
        public int MaximumRating
        {
            set => _intMaximumRating = value;
        }

        #endregion Properties

        private async void cboSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueTuple<string, bool> tupSelected = (ValueTuple<string, bool>)await cboSkill.DoThreadSafeFuncAsync(x => x.SelectedValue).ConfigureAwait(false);
            if (tupSelected.Item2)
            {
                await BuildExtraList(tupSelected.Item1).ConfigureAwait(false);
                await cboExtra.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }
            else
                await cboExtra.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
        }

        private async Task BuildExtraList(string strSelectedCategory, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strSelectedCategory))
                return;
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkillSpecializations))
            {
                if (_intMinimumRating <= 0)
                {
                    CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                    XPathNodeIterator xmlWeaponList = (await _objCharacter.LoadDataXPathAsync("weapons.xml", token: token).ConfigureAwait(false))
                        .Select("/chummer/weapons/weapon[(category = "
                                + (strSelectedCategory + "s").CleanXPath()
                                + " or useskill = "
                                + strSelectedCategory.CleanXPath() + ") and ("
                                + await objSettings.BookXPathAsync(false, token).ConfigureAwait(false) + ")]");
                    if (xmlWeaponList.Count > 0)
                    {
                        foreach (XPathNavigator xmlWeapon in xmlWeaponList)
                        {
                            string strName = xmlWeapon.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                lstSkillSpecializations.Add(
                                    new ListItem(
                                        strName,
                                        xmlWeapon.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? strName));
                            }
                        }
                    }

                    foreach (XPathNavigator xmlSpec in (await _objCharacter
                                                              .LoadDataXPathAsync("skills.xml", token: token)
                                                              .ConfigureAwait(false))
                             .Select("/chummer/skills/skill[name = "
                                     + strSelectedCategory.CleanXPath() + " and ("
                                     + await objSettings.BookXPathAsync(token: token).ConfigureAwait(false)
                                     + ")]/specs/spec"))
                    {
                        string strName = xmlSpec.Value;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            lstSkillSpecializations.Add(new ListItem(
                                                            strName,
                                                            xmlSpec.SelectSingleNodeAndCacheExpression("@translate", token: token)?.Value
                                                            ?? strName));
                        }
                    }
                }

                foreach (Skill objSkill in await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetSkillsAsync(token).ConfigureAwait(false))
                {
                    if (await objSkill.GetNameAsync(token).ConfigureAwait(false) != strSelectedCategory)
                        continue;
                    ExoticSkill objExoticSkill = (ExoticSkill) objSkill;
                    string strSpecific = await objExoticSkill.GetSpecificAsync(token).ConfigureAwait(false);
                    if (_intMinimumRating > 0 || _intMaximumRating < int.MaxValue)
                    {
                        int intTotalBaseRating
                            = await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                        if (intTotalBaseRating < _intMinimumRating)
                            continue;
                        if (intTotalBaseRating > _intMaximumRating)
                        {
                            lstSkillSpecializations.RemoveAll(x => x.Value.ToString() == strSpecific);
                            continue;
                        }
                    }
                    lstSkillSpecializations.Add(new ListItem(strSpecific, await objExoticSkill.GetCurrentDisplaySpecificAsync(token).ConfigureAwait(false)));
                }

                lstSkillSpecializations.Sort(
                    Comparer<ListItem>.Create((a, b) => string.CompareOrdinal(a.Name, b.Name)));
                string strOldText = await cboExtra.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                string strOldSelectedValue = await cboExtra.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
                await cboExtra.PopulateWithListItemsAsync(lstSkillSpecializations, token: token).ConfigureAwait(false);
                await cboExtra.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelectedValue))
                        x.SelectedValue = strOldSelectedValue;
                    if (x.SelectedIndex == -1)
                    {
                        if (!string.IsNullOrEmpty(strOldText))
                            x.Text = strOldText;
                        // Select the first Skill in the list.
                        else if (lstSkillSpecializations.Count > 0)
                            x.SelectedIndex = 0;
                    }
                }, token: token).ConfigureAwait(false);
            }
        }
    }
}
