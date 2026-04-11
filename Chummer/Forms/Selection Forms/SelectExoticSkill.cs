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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class SelectExoticSkill : Form
    {
        private readonly Character _objCharacter;
        private string _strForceSkill;
        private string _strSelectedExoticSkill;
        private string _strSelectedExoticSkillSpecialization;

        #region Control Events

        public SelectExoticSkill(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            _strSelectedExoticSkill = (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue).ConfigureAwait(false))?.ToString() ?? string.Empty;
            _strSelectedExoticSkillSpecialization = (await cboSkillSpecialisations.DoThreadSafeFuncAsync(x => x.SelectedValue).ConfigureAwait(false))?.ToString()
                ?? await _objCharacter.ReverseTranslateExtraAsync(await cboSkillSpecialisations.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false);
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void SelectExoticSkill_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
            {
                // Build the list of Exotic Active Skills from the Skills file.
                using (XmlNodeList objXmlSkillList = (await _objCharacter.LoadDataAsync("skills.xml").ConfigureAwait(false))
                                                                  .SelectNodes(
                                                                      "/chummer/skills/skill[exotic = " + bool.TrueString.CleanXPath() + "]"))
                {
                    if (objXmlSkillList?.Count > 0)
                    {
                        foreach (XmlNode objXmlSkill in objXmlSkillList)
                        {
                            string strName = objXmlSkill["name"]?.InnerTextViaPool();
                            if (!string.IsNullOrEmpty(strName) && (string.IsNullOrEmpty(_strForceSkill)
                                                                   || strName.Equals(
                                                                       _strForceSkill,
                                                                       StringComparison.OrdinalIgnoreCase)))
                                lstSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerTextViaPool() ?? strName));
                        }
                    }
                }

                lstSkills.Sort(CompareListItems.CompareNames);
                await cboCategory.PopulateWithListItemsAsync(lstSkills).ConfigureAwait(false);

                // Select the first Skill in the list.
                if (lstSkills.Count > 0)
                {
                    await cboCategory.DoThreadSafeAsync(x =>
                    {
                        x.SelectedIndex = 0;
                        x.Enabled = lstSkills.Count > 1;
                    }).ConfigureAwait(false);
                }
                else
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
            }

            await BuildList().ConfigureAwait(false);
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await BuildList().ConfigureAwait(false);
        }

        public void ForceSkill(string strSkill)
        {
            _strForceSkill = strSkill;
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Skill that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkill => _strSelectedExoticSkill;

        /// <summary>
        /// Skill specialization that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkillSpecialisation => _strSelectedExoticSkillSpecialization;

        /// <summary>
        /// Skill specialization that was selected in the dialogue.
        /// </summary>
        public async Task<string> GetSelectedExoticSkillSpecialisationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strText = await cboSkillSpecialisations.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strText))
                return strText;
            return await _objCharacter.ReverseTranslateExtraAsync(
                    await cboSkillSpecialisations.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        #endregion Properties

        private async Task BuildList(CancellationToken token = default)
        {
            string strSelectedCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
            if (string.IsNullOrEmpty(strSelectedCategory))
                return;
            CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
            XPathNodeIterator xmlWeaponList = (await _objCharacter.LoadDataXPathAsync("weapons.xml", token: token).ConfigureAwait(false))
                                                           .Select("/chummer/weapons/weapon[(category = "
                                                                   + (strSelectedCategory + "s").CleanXPath()
                                                                   + " or useskill = "
                                                                   + strSelectedCategory.CleanXPath() + ") and "
                                                                   + await objSettings.BookXPathAsync(false, token).ConfigureAwait(false) + "]");
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkillSpecializations))
            {
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

                foreach (XPathNavigator xmlSpec in (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                                                                .Select("/chummer/skills/skill[name = "
                                                                        + strSelectedCategory.CleanXPath() + " and "
                                                                        + await objSettings.BookXPathAsync(token: token).ConfigureAwait(false)
                                                                        + "]/specs/spec"))
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

                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setExistingExoticSkills))
                {
                    foreach (Skill objSkill in await (await _objCharacter.GetSkillsSectionAsync(token)
                                                                         .ConfigureAwait(false)).GetSkillsAsync(token)
                                 .ConfigureAwait(false))
                    {
                        if (await objSkill.GetNameAsync(token).ConfigureAwait(false) != strSelectedCategory)
                            continue;
                        setExistingExoticSkills.Add(await ((ExoticSkill)objSkill).GetSpecificAsync(token)
                                                        .ConfigureAwait(false));
                    }
                    lstSkillSpecializations.RemoveAll(x => setExistingExoticSkills.Contains(x.Value));
                }

                lstSkillSpecializations.Sort(
                    Comparer<ListItem>.Create((a, b) => string.CompareOrdinal(a.Name, b.Name)));
                string strOldText = await cboSkillSpecialisations.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                string strOldSelectedValue = await cboSkillSpecialisations.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
                await cboSkillSpecialisations.PopulateWithListItemsAsync(lstSkillSpecializations, token: token).ConfigureAwait(false);
                await cboSkillSpecialisations.DoThreadSafeAsync(x =>
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
