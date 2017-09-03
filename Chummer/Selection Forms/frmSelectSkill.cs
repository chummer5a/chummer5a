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
using Chummer.Skills;
using System.Linq;

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
        private string _strLimitToCategories = string.Empty;
        private string _strForceSkill = string.Empty;
        private bool _blnKnowledgeSkill = false;

        public string LinkedAttribute { get; set; } = string.Empty;

        private XmlDocument _objXmlDocument = new XmlDocument();
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectSkill(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
        }

        private void frmSelectSkill_Load(object sender, EventArgs e)
        {
            List<ListItem> lstSkills = new List<ListItem>();
            if (!_blnKnowledgeSkill)
            {
                _objXmlDocument = XmlManager.Instance.Load("skills.xml");
                // Build the list of non-Exotic Skills from the Skills file.
                XmlNodeList objXmlSkillList;
                if (!string.IsNullOrEmpty(_strForceSkill))
                {
                    objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[name = \"" + _strForceSkill + "\" and not(exotic)]");
                }
                else
                {
                    if (!string.IsNullOrEmpty(_strIncludeCategory))
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[category = \"" + _strIncludeCategory + "\" and not(exotic)]");
                    else if (!string.IsNullOrEmpty(_strLimitToCategories))
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[category = " + _strLimitToCategories + "]");
                    else if (!string.IsNullOrEmpty(_strExcludeCategory))
                    {
                        string[] strExcludes = _strExcludeCategory.Split(',');
                        string strExclude = string.Empty;
                        for (int i = 0; i <= strExcludes.Length - 1; i++)
                            strExclude += "category != \"" + strExcludes[i].Trim() + "\" and ";
                        // Remove the trailing " and ";
                        strExclude = strExclude.Substring(0, strExclude.Length - 5);
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strExclude + " and not(exotic)]");
                    }
                    else if (!string.IsNullOrEmpty(_strIncludeSkillGroup))
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[skillgroup = \"" + _strIncludeSkillGroup + "\" and not(exotic)]");
                    else if (!string.IsNullOrEmpty(_strExcludeSkillGroup))
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[skillgroup != \"" + _strExcludeSkillGroup + "\" and not(exotic)]");
                    else if (!string.IsNullOrEmpty(LinkedAttribute))
                    {
                        string[] strExcludes = LinkedAttribute.Split(',');
                        string strExclude = "not(exotic) and (";
                        for (int i = 0; i <= strExcludes.Length - 1; i++)
                            strExclude += "attribute = \"" + strExcludes[i].Trim() + "\" or ";
                        // Remove the trailing " and ";
                        strExclude = strExclude.Substring(0, strExclude.Length - 4) + ")";
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strExclude + "]");
                    }
                    else if (!string.IsNullOrEmpty(_strLimitToSkill))
                    {
                        string strFilter = "not(exotic) and (";
                        string[] strValue = _strLimitToSkill.Split(',');
                        foreach (string strSkill in strValue)
                            strFilter += "name = \"" + strSkill.Trim() + "\" or ";
                        // Remove the trailing " or ".
                        strFilter = strFilter.Substring(0, strFilter.Length - 4);
                        strFilter += ")";
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strFilter + "]");
                    }
                    else
                        objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[not(exotic)]");
                }

                // Add the Skills to the list.
                foreach (XmlNode objXmlSkill in objXmlSkillList)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlSkill["name"].InnerText;
                    objItem.Name = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
                    lstSkills.Add(objItem);
                }

                // Add in any Exotic Skills the character has.
                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (objSkill.IsExoticSkill)
                    {
                        ExoticSkill objExoticSkill = objSkill as ExoticSkill;
                        bool blnAddSkill = true;
                        if (!string.IsNullOrEmpty(_strForceSkill))
                            blnAddSkill = _strForceSkill == objExoticSkill.Name + " (" + objExoticSkill.Specific + ")";
                        else
                        {
                            if (!string.IsNullOrEmpty(_strIncludeCategory))
                                blnAddSkill = _strIncludeCategory == objExoticSkill.SkillCategory;
                            else if (!string.IsNullOrEmpty(_strExcludeCategory))
                                blnAddSkill = !_strExcludeCategory.Contains(objExoticSkill.SkillCategory);
                            else if (!string.IsNullOrEmpty(_strIncludeSkillGroup))
                                blnAddSkill = _strIncludeSkillGroup == objExoticSkill.SkillGroup;
                            else if (!string.IsNullOrEmpty(_strExcludeSkillGroup))
                                blnAddSkill = _strExcludeSkillGroup != objExoticSkill.SkillGroup;
                            else if (!string.IsNullOrEmpty(_strLimitToSkill))
                                blnAddSkill = _strLimitToSkill.Contains(objExoticSkill.Name);
                        }

                        if (blnAddSkill)
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objExoticSkill.Name + " (" + objExoticSkill.Specific + ")";
                            // Use the translated Exotic Skill name if available.
                            XmlNode objXmlSkill =
                                _objXmlDocument.SelectSingleNode("/chummer/skills/skill[exotic = \"Yes\" and name = \"" + objExoticSkill.Name + "\"]");
                            objItem.Name = objXmlSkill["translate"] != null
                                ? objXmlSkill["translate"].InnerText + " (" + objExoticSkill.DisplaySpecialization + ")"
                                : objExoticSkill.Name + " (" + objExoticSkill.DisplaySpecialization + ")";
                            lstSkills.Add(objItem);
                        }
                    }
                }
            }
            else
            {
                //TODO: This is less robust than it should be. Should be refactored to support the rest of the entries.
                if (!string.IsNullOrWhiteSpace(_strLimitToSkill))
                {
                    _objXmlDocument = XmlManager.Instance.Load("skills.xml");
                    string strFilter = string.Empty;
                    string[] strValue = _strLimitToSkill.Split(',');
                    strFilter = strValue.Aggregate(strFilter, (current, strSkill) => current + "name = \"" + strSkill.Trim() + "\" or ");
                    // Remove the trailing " or ".
                    strFilter = strFilter.Substring(0, strFilter.Length - 4);
                    XmlNodeList objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill[" + strFilter + "]");

                    // Add the Skills to the list.
                    foreach (XmlNode objXmlSkill in objXmlSkillList)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objXmlSkill["name"].InnerText;
                        if (objXmlSkill.Attributes != null)
                        {
                            objItem.Name = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
                        }
                        else
                            objItem.Name = objXmlSkill["name"].InnerXml;
                        lstSkills.Add(objItem);
                    }
                }
                else
                {
                    // Instead of showing all available Active Skills, show a list of Knowledge Skills that the character currently has.
                    foreach (KnowledgeSkill objKnow in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        ListItem objSkill = new ListItem();
                        objSkill.Value = objKnow.Name;
                        objSkill.Name = objKnow.DisplayName;
                        lstSkills.Add(objSkill);
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            lstSkills.Sort(objSort.Compare);
            cboSkill.BeginUpdate();
            cboSkill.ValueMember = "Value";
            cboSkill.DisplayMember = "Name";
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
            set
            {
                _strIncludeCategory = value;
            }
        }

        /// <summary>
        /// Only Skills from the selected Categories should be in the list.
        /// </summary>
        public XmlNode LimitToCategories
        {
            set
            {
                IEnumerable<string> lstCategories = value?.SelectNodes("category")?
                    .Cast<XmlNode>()
                    .Select(n => "\"" + n.InnerText + "\"");
                if (lstCategories != null)
                    _strLimitToCategories = string.Join(" or category = ", lstCategories);
            }
        }

        /// <summary>
        /// Only Skills not in the selected Category should be in the list.
        /// </summary>
        public string ExcludeCategory
        {
            set
            {
                _strExcludeCategory = value;
            }
        }

        /// <summary>
        /// Only Skills in the selected Skill Group should be in the list.
        /// </summary>
        public string OnlySkillGroup
        {
            set
            {
                _strIncludeSkillGroup = value;
            }
        }

        /// <summary>
        /// Restrict the list to only a single Skill.
        /// </summary>
        public string OnlySkill
        {
            set
            {
                _strForceSkill = value.Replace(", " + LanguageManager.Instance.GetString("Label_SelectGear_Hacked"), string.Empty);
            }
        }

        /// <summary>
        /// Only Skills not in the selected Skill Group should be in the list.
        /// </summary>
        public string ExcludeSkillGroup
        {
            set
            {
                _strExcludeSkillGroup = value;
            }
        }

        /// <summary>
        /// Only the provided Skills should be shown in the list.
        /// </summary>
        public string LimitToSkill
        {
            set
            {
                _strLimitToSkill = value;
            }
        }

        /// <summary>
        /// Skill that was selected in the dialogue.
        /// </summary>
        public string SelectedSkill
        {
            get
            {
                return _strReturnValue;
            }
        }

        /// <summary>
        /// Description to show in the window.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        /// <summary>
        /// Whether or not Knowledge Skills should be shown instead.
        /// </summary>
        public bool ShowKnowledgeSkills
        {
            set
            {
                _blnKnowledgeSkill = value;
            }
        }
        #endregion
    
public  Character objCharacter { get; set; }
    }
}