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

        #region Control Events

        public SelectExoticSkill(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SelectExoticSkill_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
            {
                // Build the list of Exotic Active Skills from the Skills file.
                using (XmlNodeList objXmlSkillList = _objCharacter.LoadData("skills.xml")
                                                                  .SelectNodes(
                                                                      "/chummer/skills/skill[exotic = \"True\"]"))
                {
                    if (objXmlSkillList?.Count > 0)
                    {
                        foreach (XmlNode objXmlSkill in objXmlSkillList)
                        {
                            string strName = objXmlSkill["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strName) && (string.IsNullOrEmpty(_strForceSkill)
                                                                   || strName.Equals(
                                                                       _strForceSkill,
                                                                       StringComparison.OrdinalIgnoreCase)))
                                lstSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                lstSkills.Sort(CompareListItems.CompareNames);
                cboCategory.BeginUpdate();
                cboCategory.PopulateWithListItems(lstSkills);

                // Select the first Skill in the list.
                if (lstSkills.Count > 0)
                {
                    cboCategory.SelectedIndex = 0;
                    cboCategory.Enabled = lstSkills.Count > 1;
                }
                else
                    cmdOK.Enabled = false;

                cboCategory.EndUpdate();
            }

            BuildList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildList();
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
        public string SelectedExoticSkill => cboCategory.SelectedValue?.ToString() ?? string.Empty;

        /// <summary>
        /// Skill specialization that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkillSpecialisation => cboSkillSpecialisations.SelectedValue?.ToString()
                                                           ?? _objCharacter.ReverseTranslateExtra(cboSkillSpecialisations.Text);

        #endregion Properties

        private void BuildList()
        {
            string strSelectedCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(strSelectedCategory))
                return;
            XPathNodeIterator xmlWeaponList = _objCharacter.LoadDataXPath("weapons.xml")
                                                           .Select("/chummer/weapons/weapon[(category = "
                                                                   + (strSelectedCategory + 's').CleanXPath()
                                                                   + " or useskill = "
                                                                   + strSelectedCategory.CleanXPath() + ") and ("
                                                                   + _objCharacter.Settings.BookXPath(false) + ")]");
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkillSpecializations))
            {
                if (xmlWeaponList.Count > 0)
                {
                    foreach (XPathNavigator xmlWeapon in xmlWeaponList)
                    {
                        string strName = xmlWeapon.SelectSingleNode("name")?.Value;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            lstSkillSpecializations.Add(
                                new ListItem(
                                    strName,
                                    xmlWeapon.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                        }
                    }
                }

                foreach (XPathNavigator xmlSpec in _objCharacter.LoadDataXPath("skills.xml")
                                                                .Select("/chummer/skills/skill[name = "
                                                                        + strSelectedCategory.CleanXPath() + " and ("
                                                                        + _objCharacter.Settings.BookXPath()
                                                                        + ")]/specs/spec"))
                {
                    string strName = xmlSpec.Value;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        lstSkillSpecializations.Add(new ListItem(
                                                        strName,
                                                        xmlSpec.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                        ?? strName));
                    }
                }

                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setExistingExoticSkills))
                {
                    setExistingExoticSkills.AddRange(_objCharacter.SkillsSection.Skills
                                                         .Where(
                                                             x => x.Name
                                                                  == strSelectedCategory)
                                                         .Select(x => ((ExoticSkill) x)
                                                                     .Specific));
                    lstSkillSpecializations.RemoveAll(x => setExistingExoticSkills.Contains(x.Value));
                }

                lstSkillSpecializations.Sort(
                    Comparer<ListItem>.Create((a, b) => string.CompareOrdinal(a.Name, b.Name)));
                string strOldText = cboSkillSpecialisations.Text;
                string strOldSelectedValue = cboSkillSpecialisations.SelectedValue?.ToString() ?? string.Empty;
                cboSkillSpecialisations.BeginUpdate();
                cboSkillSpecialisations.PopulateWithListItems(lstSkillSpecializations);
                if (!string.IsNullOrEmpty(strOldSelectedValue))
                    cboSkillSpecialisations.SelectedValue = strOldSelectedValue;
                if (cboSkillSpecialisations.SelectedIndex == -1)
                {
                    if (!string.IsNullOrEmpty(strOldText))
                        cboSkillSpecialisations.Text = strOldText;
                    // Select the first Skill in the list.
                    else if (lstSkillSpecializations.Count > 0)
                        cboSkillSpecialisations.SelectedIndex = 0;
                }

                cboSkillSpecialisations.EndUpdate();
            }
        }
    }
}
