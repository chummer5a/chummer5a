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
using System.Text;
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
            _strSourceName = strSource;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = _objCharacter.LoadDataXPath("skills.xml");
        }

        private async void SelectSkill_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
            {
                // Build the list of non-Exotic Skills from the Skills file.
                XPathNodeIterator objXmlSkillList;
                if (!string.IsNullOrEmpty(_strForceSkill))
                {
                    objXmlSkillList = _objXmlDocument.Select("/chummer/skills/skill[name = "
                                                             + _strForceSkill.CleanXPath() + " and not(exotic) and ("
                                                             + _objCharacter.Settings.BookXPath() + ")]");
                }
                else if (!string.IsNullOrEmpty(_strLimitToCategories))
                    objXmlSkillList = _objXmlDocument.Select("/chummer/skills/skill[" + _strLimitToCategories + " and ("
                                                             + _objCharacter.Settings.BookXPath() + ")]");
                else
                {
                    string strFilter = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append("not(exotic) and (").Append(_objCharacter.Settings.BookXPath()).Append(')');
                        if (!string.IsNullOrEmpty(_strIncludeCategory))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillCategory in _strIncludeCategory.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("category = ").Append(strSkillCategory.Trim().CleanXPath())
                                         .Append(" or ");
                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strExcludeCategory))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillCategory in _strExcludeCategory.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("category != ").Append(strSkillCategory.Trim().CleanXPath())
                                         .Append(" and ");
                            // Remove the trailing " and ".
                            sbdFilter.Length -= 5;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strIncludeSkillGroup))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillGroup in _strIncludeSkillGroup.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("skillgroup = ").Append(strSkillGroup.Trim().CleanXPath())
                                         .Append(" or ");
                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strExcludeSkillGroup))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkillGroup in _strExcludeSkillGroup.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("skillgroup != ").Append(strSkillGroup.Trim().CleanXPath())
                                         .Append(" and ");
                            // Remove the trailing " and ".
                            sbdFilter.Length -= 5;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(LinkedAttribute))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strAttribute in LinkedAttribute.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("attribute = ").Append(strAttribute.Trim().CleanXPath())
                                         .Append(" or ");
                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strLimitToSkill))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkill in _strLimitToSkill.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("name = ").Append(strSkill.Trim().CleanXPath()).Append(" or ");
                            // Remove the trailing " or ".
                            sbdFilter.Length -= 4;
                            sbdFilter.Append(')');
                        }

                        if (!string.IsNullOrEmpty(_strExcludeSkill))
                        {
                            sbdFilter.Append(" and (");
                            foreach (string strSkill in _strExcludeSkill.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                sbdFilter.Append("name != ").Append(strSkill.Trim().CleanXPath()).Append(" and ");
                            // Remove the trailing " and ".
                            sbdFilter.Length -= 5;
                            sbdFilter.Append(')');
                        }

                        if (sbdFilter.Length > 0)
                            strFilter = '[' + sbdFilter.ToString() + ']';
                    }

                    objXmlSkillList = _objXmlDocument.Select("/chummer/skills/skill" + strFilter);
                }

                // Add the Skills to the list.
                if (objXmlSkillList.Count > 0)
                {
                    foreach (XPathNavigator objXmlSkill in objXmlSkillList)
                    {
                        string strXmlSkillName = objXmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value;
                        Skill objExistingSkill = _objCharacter.SkillsSection.GetActiveSkill(strXmlSkillName);
                        if (objExistingSkill == null)
                        {
                            if (_intMinimumRating > 0)
                            {
                                continue;
                            }
                        }
                        else if (objExistingSkill.Rating < _intMinimumRating
                                 || objExistingSkill.Rating > _intMaximumRating)
                        {
                            continue;
                        }

                        lstSkills.Add(new ListItem(strXmlSkillName,
                                                   objXmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                   ?? strXmlSkillName));
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
                            blnAddSkill = _strForceSkill == objExoticSkill.DictionaryKey;
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
                            XPathNavigator objXmlSkill = _objXmlDocument.SelectSingleNode(
                                "/chummer/skills/skill[exotic = " + bool.TrueString.CleanXPath()
                                                                  + " and name = " + objExoticSkill.Name.CleanXPath()
                                                                  + ']');
                            lstSkills.Add(new ListItem(objExoticSkill.DictionaryKey,
                                                       (objXmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                        ?? objExoticSkill.CurrentDisplayName)
                                                       + await LanguageManager.GetStringAsync("String_Space") + '('
                                                       + objExoticSkill.CurrentDisplaySpecialization + ')'));
                        }
                    }
                }

                if (lstSkills.Count == 0)
                {
                    Program.MainForm.ShowMessageBox(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                                      await LanguageManager.GetStringAsync("Message_Improvement_EmptySelectionListNamed"),
                                      _strSourceName));
                    DialogResult = DialogResult.Cancel;
                    return;
                }

                lstSkills.Sort(CompareListItems.CompareNames);
                cboSkill.BeginUpdate();
                cboSkill.PopulateWithListItems(lstSkills);
                // Select the first Skill in the list.
                cboSkill.SelectedIndex = 0;
                cboSkill.EndUpdate();
            }

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
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdLimitToCategories))
                    {
                        foreach (XmlNode objNode in xmlCategoryList)
                        {
                            sbdLimitToCategories.Append("category = ").Append(objNode.InnerText.CleanXPath())
                                                .Append(" or ");
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
    }
}
