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
 using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class frmSelectExoticSkill : Form
    {
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectExoticSkill(Character objCharacter)
        {
            InitializeComponent();
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

        private void frmSelectExoticSkill_Load(object sender, EventArgs e)
        {
            List<ListItem> lstSkills = new List<ListItem>();

            // Build the list of Exotic Active Skills from the Skills file.
            using (XmlNodeList objXmlSkillList = XmlManager.Load("skills.xml").SelectNodes("/chummer/skills/skill[exotic = \"True\"]"))
                if (objXmlSkillList?.Count > 0)
                    foreach (XmlNode objXmlSkill in objXmlSkillList)
                    {
                        string strName = objXmlSkill["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                            lstSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
                    }
            lstSkills.Sort(CompareListItems.CompareNames);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstSkills;

            // Select the first Skill in the list.
            if (lstSkills.Count > 0)
                cboCategory.SelectedIndex = 0;
            else
                cmdOK.Enabled = false;

            cboCategory.EndUpdate();

            BuildList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Skill that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkill => cboCategory.SelectedValue?.ToString() ?? string.Empty;

        /// <summary>
        /// Skill specialisation that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkillSpecialisation => cboSkillSpecialisations.SelectedValue?.ToString() ?? LanguageManager.ReverseTranslateExtra(cboSkillSpecialisations.Text);

        #endregion

        private void BuildList()
        {
            string strSelectedCategory = cboCategory.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedCategory)) return;
            List<ListItem> lstSkillSpecializations = new List<ListItem>();

            using (XmlNodeList xmlWeaponList = XmlManager.Load("weapons.xml")
                .SelectNodes(string.Format(GlobalOptions.InvariantCultureInfo, "/chummer/weapons/weapon[(category = \"{0}s\" or useskill = \"{0}\") and ({1})]",
                    strSelectedCategory, _objCharacter.Options.BookXPath(false))))
            {
                if (xmlWeaponList?.Count > 0)
                {
                    foreach (XmlNode xmlWeapon in xmlWeaponList)
                    {
                        string strName = xmlWeapon["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            lstSkillSpecializations.Add(new ListItem(strName, xmlWeapon["translate"]?.InnerText ?? strName));
                        }
                    }
                }
            }

            using (XmlNodeList xmlSpecializationList = XmlManager.Load("skills.xml")
                .SelectNodes("/chummer/skills/skill[name = \"" + strSelectedCategory + "\" and (" + _objCharacter.Options.BookXPath() + ")]/specs/spec"))
            {
                if (xmlSpecializationList?.Count > 0)
                {
                    foreach (XmlNode xmlSpec in xmlSpecializationList)
                    {
                        string strName = xmlSpec.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            lstSkillSpecializations.Add(new ListItem(strName, xmlSpec.Attributes?["translate"]?.InnerText ?? strName));
                        }
                    }
                }
            }

            List<string> lstExistingExoticSkills = _objCharacter.SkillsSection.Skills
                .Where(x => x.Name == strSelectedCategory).Select(x => ((ExoticSkill) x).Specific).ToList();
            lstSkillSpecializations.RemoveAll(x => lstExistingExoticSkills.Contains(x.Value));
            lstSkillSpecializations.Sort(Comparer<ListItem>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal)));
            cboSkillSpecialisations.BeginUpdate();
            cboSkillSpecialisations.ValueMember = "Value";
            cboSkillSpecialisations.DisplayMember = "Name";
            cboSkillSpecialisations.DataSource = lstSkillSpecializations;

            // Select the first Skill in the list.
            if (lstSkillSpecializations.Count > 0)
                cboSkillSpecialisations.SelectedIndex = 0;
            cboSkillSpecialisations.EndUpdate();
        }
    }
}
