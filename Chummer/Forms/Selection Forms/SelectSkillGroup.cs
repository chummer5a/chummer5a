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
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectSkillGroup : Form
    {
        private string _strReturnValue = string.Empty;
        private string _strForceValue = string.Empty;
        private string _strExcludeCategory = string.Empty;

        private readonly XPathNavigator _objXmlDocument;

        #region Control Events

        public frmSelectSkillGroup(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = XmlManager.LoadXPath("skills.xml", objCharacter?.Settings.EnabledCustomDataDirectoryPaths);
        }

        private void frmSelectSkillGroup_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGroups))
            {
                if (string.IsNullOrEmpty(_strForceValue))
                {
                    // Build the list of Skill Groups found in the Skills file.
                    foreach (XPathNavigator objXmlSkill in _objXmlDocument.SelectAndCacheExpression(
                                 "/chummer/skillgroups/name"))
                    {
                        if (!string.IsNullOrEmpty(_strExcludeCategory))
                        {
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdExclude))
                            {
                                string strExclude = string.Empty;
                                foreach (string strCategory in _strExcludeCategory.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                    sbdExclude.Append("category != ").Append(strCategory.CleanXPath()).Append(" and ");
                                // Remove the trailing " and ";
                                if (sbdExclude.Length > 0)
                                {
                                    sbdExclude.Length -= 5;
                                    strExclude = '(' + sbdExclude.ToString() + ") and ";
                                }
                                if (_objXmlDocument.SelectSingleNode(
                                        "/chummer/skills/skill[" + strExclude + "skillgroup = "
                                        + objXmlSkill.Value.CleanXPath() + ']') == null)
                                    continue;
                            }
                        }

                        string strInnerText = objXmlSkill.Value;
                        lstGroups.Add(new ListItem(strInnerText,
                                                   objXmlSkill.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                   ?? strInnerText));
                    }
                }
                else
                {
                    lstGroups.Add(new ListItem(_strForceValue, _strForceValue));
                }

                lstGroups.Sort(CompareListItems.CompareNames);
                cboSkillGroup.BeginUpdate();
                cboSkillGroup.PopulateWithListItems(lstGroups);
                // Select the first Skill in the list.
                cboSkillGroup.SelectedIndex = 0;
                cboSkillGroup.EndUpdate();
            }

            if (cboSkillGroup.Items.Count == 1)
                cmdOK_Click(sender, e);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboSkillGroup.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        #region Properties

        // Skill Group that was selected in the dialogue.
        public string SelectedSkillGroup => _strReturnValue;

        // Description to show in the window.
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Force a specific SkillGroup to be selected.
        /// </summary>
        public string OnlyGroup
        {
            set => _strForceValue = value;
        }

        /// <summary>
        /// Only Skills not in the selected Category should be in the list.
        /// </summary>
        public string ExcludeCategory
        {
            set => _strExcludeCategory = value;
        }

        #endregion Properties
    }
}
