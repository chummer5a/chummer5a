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
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Skills;
using System.Text;

namespace Chummer
{
    public partial class frmSelectSkill : Form
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

        private readonly XmlDocument _objXmlDocument;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectSkill(Character objCharacter, string strSource = "")
        {
            _objCharacter = objCharacter;
            _strSourceName = strSource;
            InitializeComponent();
            this.TranslateWinForm();
            _objXmlDocument = XmlManager.Load("skills.xml");
        }

        private void frmSelectSkill_Load(object sender, EventArgs e)
        {
            List<ListItem> lstSkills = new List<ListItem>();
            // Build the list of non-Exotic Skills from the Skills file.
            XmlNodeList objXmlSkillList;
            if (!string.IsNullOrEmpty(_strForceSkill))
            {
                objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[name = \"" + _strForceSkill + "\" and not(exotic) and (" + _objCharacter.Options.BookXPath() + ")]");
            }
            else if (!string.IsNullOrEmpty(_strLimitToCategories))
                objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + _strLimitToCategories + " and (" + _objCharacter.Options.BookXPath() + ")]");
            else
            {
                string strFilter = "not(exotic)";
                if (!string.IsNullOrEmpty(_strIncludeCategory))
                {
                    strFilter += " and (";
                    string[] strValue = _strIncludeCategory.Split(',');
                    foreach (string strSkillCategory in strValue)
                        strFilter += "category = \"" + strSkillCategory.Trim() + "\" or ";
                    // Remove the trailing " or ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 4);
                    strFilter += ')';
                }
                if (!string.IsNullOrEmpty(_strExcludeCategory))
                {
                    strFilter += " and (";
                    string[] strValue = _strExcludeCategory.Split(',');
                    foreach (string strSkillCategory in strValue)
                        strFilter += "category != \"" + strSkillCategory.Trim() + "\" and ";
                    // Remove the trailing " and ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 5);
                    strFilter += ')';
                }
                if (!string.IsNullOrEmpty(_strIncludeSkillGroup))
                {
                    strFilter += " and (";
                    string[] strValue = _strIncludeSkillGroup.Split(',');
                    foreach (string strSkillGroup in strValue)
                        strFilter += "skillgroup = \"" + strSkillGroup.Trim() + "\" or ";
                    // Remove the trailing " or ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 4);
                    strFilter += ')';
                }
                if (!string.IsNullOrEmpty(_strExcludeSkillGroup))
                {
                    strFilter += " and (";
                    string[] strValue = _strExcludeSkillGroup.Split(',');
                    foreach (string strSkillGroup in strValue)
                        strFilter += "skillgroup != \"" + strSkillGroup.Trim() + "\" and ";
                    // Remove the trailing " and ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 5);
                    strFilter += ')';
                }
                if (!string.IsNullOrEmpty(LinkedAttribute))
                {
                    strFilter += " and (";
                    string[] strValue = LinkedAttribute.Split(',');
                    foreach (string strAttribute in strValue)
                        strFilter += "attribute = \"" + strAttribute.Trim() + "\" or ";
                    // Remove the trailing " or ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 4);
                    strFilter += ')';
                }
                if (!string.IsNullOrEmpty(_strLimitToSkill))
                {
                    strFilter += " and (";
                    string[] strValue = _strLimitToSkill.Split(',');
                    foreach (string strSkill in strValue)
                        strFilter += "name = \"" + strSkill.Trim() + "\" or ";
                    // Remove the trailing " or ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 4);
                    strFilter += ')';
                }
                if (!string.IsNullOrEmpty(_strExcludeSkill))
                {
                    strFilter += " and (";
                    string[] strValue = _strExcludeSkill.Split(',');
                    foreach (string strSkill in strValue)
                        strFilter += "name != \"" + strSkill.Trim() + "\" and ";
                    // Remove the trailing " or ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 4);
                    strFilter += ')';
                }
                objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strFilter + " and (" + _objCharacter.Options.BookXPath() + ")]");
            }

            // Add the Skills to the list.
            if (objXmlSkillList?.Count > 0)
            {
                foreach (XmlNode objXmlSkill in objXmlSkillList)
                {
                    string strXmlSkillName = objXmlSkill["name"]?.InnerText;
                    Skill objExistingSkill = _objCharacter.SkillsSection.GetActiveSkill(strXmlSkillName);
                    if (objExistingSkill == null)
                    {
                        if (_intMinimumRating > 0)
                        {
                            continue;
                        }
                    }
                    else if (objExistingSkill.Rating < _intMinimumRating || objExistingSkill.Rating > _intMaximumRating)
                    {
                        continue;
                    }

                    lstSkills.Add(new ListItem(strXmlSkillName, objXmlSkill["translate"]?.InnerText ?? strXmlSkillName));
                }
            }

            // Add in any Exotic Skills the character has.
            foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
            {
                if (objSkill.IsExoticSkill)
                {
                    ExoticSkill objExoticSkill = objSkill as ExoticSkill;
                    bool blnAddSkill = true;
                    if (objSkill.Rating < _intMinimumRating || objSkill.Rating > _intMaximumRating)
                        blnAddSkill = false;
                    else if (!string.IsNullOrEmpty(_strForceSkill))
                        blnAddSkill = _strForceSkill == objExoticSkill.Name + " (" + objExoticSkill.Specific + ')';
                    else if (!string.IsNullOrEmpty(_strIncludeCategory))
                        blnAddSkill = _strIncludeCategory.Contains(objExoticSkill.SkillCategory);
                    else if (!string.IsNullOrEmpty(_strExcludeCategory))
                        blnAddSkill = !_strExcludeCategory.Contains(objExoticSkill.SkillCategory);
                    else if (!string.IsNullOrEmpty(_strIncludeSkillGroup))
                        blnAddSkill = _strIncludeSkillGroup.Contains(objExoticSkill.SkillGroup);
                    else if (!string.IsNullOrEmpty(_strExcludeSkillGroup))
                        blnAddSkill = !_strExcludeSkillGroup.Contains(objExoticSkill.SkillGroup);
                    else if (!string.IsNullOrEmpty(_strLimitToSkill))
                        blnAddSkill = _strLimitToSkill.Contains(objExoticSkill.Name);
                    else if (!string.IsNullOrEmpty(_strExcludeSkill))
                        blnAddSkill = !_strExcludeSkill.Contains(objExoticSkill.Name);

                    if (blnAddSkill)
                    {
                        // Use the translated Exotic Skill name if available.
                        XmlNode objXmlSkill = _objXmlDocument.SelectSingleNode("/chummer/skills/skill[exotic = \"True\" and name = \"" + objExoticSkill.Name + "\"]");
                        lstSkills.Add(new ListItem(objExoticSkill.Name + " (" + objExoticSkill.Specific + ')',
                            (objXmlSkill["translate"]?.InnerText ?? objExoticSkill.Name) + LanguageManager.GetString("String_Space") + '(' + objExoticSkill.CurrentDisplaySpecialization + ')'));
                    }
                }
            }

            if (lstSkills.Count <= 0)
            {
                Program.MainForm.ShowMessageBox(this, string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_Improvement_EmptySelectionListNamed"), _strSourceName));
                DialogResult = DialogResult.Cancel;
                return;
            }

            lstSkills.Sort(CompareListItems.CompareNames);
            cboSkill.BeginUpdate();
            cboSkill.ValueMember = nameof(ListItem.Value);
            cboSkill.DisplayMember = nameof(ListItem.Name);
            cboSkill.DataSource = lstSkills;

            // Select the first Skill in the list.
            cboSkill.SelectedIndex = 0;
            cboSkill.EndUpdate();

            if (cboSkill.Items.Count == 1)
                cmdOK_Click(sender, e);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboSkill.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

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
                    StringBuilder objLimitToCategories = new StringBuilder();
                    foreach (XmlNode objNode in xmlCategoryList)
                    {
                        objLimitToCategories.Append("category = ");
                        objLimitToCategories.Append('\"' + objNode.InnerText + '\"');
                        objLimitToCategories.Append(" or ");
                    }

                    // Remove the last " or "
                    if (objLimitToCategories.Length > 0)
                        objLimitToCategories.Length -= 4;
                    _strLimitToCategories = objLimitToCategories.ToString();
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
        #endregion
    }
}
