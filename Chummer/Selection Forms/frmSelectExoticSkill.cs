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
﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectExoticSkill : Form
    {
        #region Control Events
        public frmSelectExoticSkill()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
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
            XmlDocument objXmlDocument = new XmlDocument();
            objXmlDocument = XmlManager.Instance.Load("skills.xml");

            List<ListItem> lstSkills = new List<ListItem>();
            

            // Build the list of Exotic Active Skills from the Skills file.
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[exotic = \"Yes\"]");
            foreach (XmlNode objXmlSkill in objXmlSkillList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlSkill["name"].InnerText;
                objItem.Name = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
                lstSkills.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            lstSkills.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstSkills;

            // Select the first Skill in the list.
            cboCategory.SelectedIndex = 0;

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
        public string SelectedExoticSkill
        {
            get
            {
                return cboCategory.SelectedValue.ToString();
            }
        }
        /// <summary>
        /// Skill specialisation that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkillSpecialisation
        {
            get
            {
                if (cboSkillSpecialisations.SelectedValue == null)
                {
                    return cboSkillSpecialisations.Text;
                }
                else
                {
                    return cboSkillSpecialisations.SelectedValue.ToString();
                }
            }
        }


        #endregion

        private void BuildList()
        {
            List<ListItem> lstSkillSpecialisations = new List<ListItem>();
            XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

            XmlNodeList objXmlSelectedSkill =
                objXmlDocument.SelectNodes("/chummer/skills/skill[name = \"" + cboCategory.SelectedValue.ToString() + "\"]/specs/spec");
            XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");
            XmlNodeList objXmlWeaponList =
                objXmlWeaponDocument.SelectNodes("/chummer/weapons/weapon[category = \"" + cboCategory.SelectedValue.ToString() +
                                                 "s\" or useskill = \"" + cboCategory.SelectedValue.ToString() + "\"]");
            foreach (XmlNode objXmlWeapon in objXmlWeaponList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlWeapon["name"].InnerText;
                objItem.Name = objXmlWeapon["translate"]?.InnerText ?? objXmlWeapon["name"].InnerText;

                lstSkillSpecialisations.Add(objItem);
            }
            foreach (XmlNode objXmlSpecialization in objXmlSelectedSkill)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlSpecialization.InnerText;
                objItem.Name = objXmlSpecialization["translate"]?.InnerText ?? objXmlSpecialization.InnerText;
                lstSkillSpecialisations.Add(objItem);
            }
            cboSkillSpecialisations.BeginUpdate();
            cboSkillSpecialisations.ValueMember = "Value";
            cboSkillSpecialisations.DisplayMember = "Name";
            cboSkillSpecialisations.DataSource = lstSkillSpecialisations;

            // Select the first Skill in the list.
            cboSkillSpecialisations.SelectedIndex = 0;
            cboSkillSpecialisations.EndUpdate();
        }
    }
}