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
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class SelectSpec : Form
    {
        private readonly Skill _objSkill;
        private readonly Character _objCharacter;
        private readonly string _strForceItem = string.Empty;
        private readonly XPathNavigator _objXmlDocument;

        #region Control Events

        public SelectSpec(Skill objSkill)
        {
            _objSkill = objSkill ?? throw new ArgumentNullException(nameof(objSkill));
            _objCharacter = objSkill.CharacterObject;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = XmlManager.LoadXPath("skills.xml", _objCharacter?.Settings.EnabledCustomDataDirectoryPaths);
        }

        private async void SelectSpec_Load(object sender, EventArgs e)
        {
            string strSkillName = await _objSkill.GetNameAsync().ConfigureAwait(false);
            XPathNavigator xmlParentSkill;
            if (Mode == "Knowledge")
            {
                xmlParentSkill
                    = _objXmlDocument.TryGetNodeByNameOrId("/chummer/knowledgeskills/skill", strSkillName)
                      ?? _objXmlDocument.SelectSingleNode(
                          "/chummer/knowledgeskills/skill[translate = " + strSkillName.CleanXPath() + ']');
            }
            else
            {
                xmlParentSkill = _objXmlDocument.TryGetNodeByNameOrId("/chummer/skills/skill", strSkillName,
                                                                      await (await _objCharacter.GetSettingsAsync()
                                                                              .ConfigureAwait(false)).BookXPathAsync()
                                                                          .ConfigureAwait(false));
            }

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstItems))
            {
                // Populate the Skill's Specializations (if any).
                XPathNodeIterator xmlSpecList = xmlParentSkill?.SelectAndCacheExpression("specs/spec");
                if (xmlSpecList?.Count > 0)
                {
                    foreach (XPathNavigator objXmlSpecialization in xmlSpecList)
                    {
                        string strInnerText = objXmlSpecialization.Value;
                        lstItems.Add(new ListItem(strInnerText,
                                                  objXmlSpecialization.SelectSingleNodeAndCacheExpression("@translate")
                                                                      ?.Value ?? strInnerText));
                    }
                }

                lstItems.Sort();

                if (_objSkill.SkillCategory == "Combat Active")
                {
                    // Look through the Weapons file and grab the names of items that are part of the appropriate Category or use the matching Skill.
                    XPathNavigator objXmlWeaponDocument = await _objCharacter.LoadDataXPathAsync("weapons.xml").ConfigureAwait(false);
                    string strXPathFilter;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                    {
                        sbdFilter.Append("category = ").Append(strSkillName.CleanXPath());
                        foreach (ListItem objSpec in lstItems)
                        {
                            string strLoopValue = objSpec.Value.ToString().CleanXPath();
                            sbdFilter.Append(" or spec = ").Append(strLoopValue)
                                     .Append(" or spec2 = ").Append(strLoopValue);
                        }
                        strXPathFilter = sbdFilter.ToString();
                    }

                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstWeaponItems))
                    {
                        //Might need to include skill name or might miss some values?
                        foreach (XPathNavigator objXmlWeapon in objXmlWeaponDocument.Select(
                                     "/chummer/weapons/weapon[(" + strXPathFilter + ") and ("
                                     + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false))
                                             .BookXPathAsync().ConfigureAwait(false)
                                     + ")]"))
                        {
                            string strName = objXmlWeapon.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                lstWeaponItems.Add(new ListItem(
                                                       strName,
                                                       objXmlWeapon
                                                           .SelectSingleNodeAndCacheExpression("translate")?.Value
                                                       ?? strName));
                            }
                        }

                        lstWeaponItems.Sort();
                        lstItems.AddRange(lstWeaponItems);
                    }
                }

                lstItems.Insert(0, new ListItem("Custom", string.Empty));

                // Populate the lists.
                await cboSpec.PopulateWithListItemsAsync(lstItems).ConfigureAwait(false);
            }

            // If there's only 1 value in the list, the character doesn't have a choice, so just accept it.
            if (await cboSpec.DoThreadSafeFuncAsync(x => x.Items.Count == 1 && x.DropDownStyle == ComboBoxStyle.DropDownList).ConfigureAwait(false) && AllowAutoSelect)
                await this.DoThreadSafeAsync(x => x.AcceptForm()).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_strForceItem))
            {
                await cboSpec.DoThreadSafeAsync(x => x.SelectedIndex = x.FindStringExact(_strForceItem)).ConfigureAwait(false);
                if (await cboSpec.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false) == -1)
                {
                    await cboSpec.PopulateWithListItemAsync(new ListItem(_strForceItem, _strForceItem)).ConfigureAwait(false);
                    await cboSpec.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x => x.AcceptForm()).ConfigureAwait(false);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cboSpec_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSpec.DropDownStyle = cboSpec.SelectedValue?.ToString() == "Custom" ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Name of the item that was selected.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                string strSelected = cboSpec.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strSelected) && strSelected != "Custom")
                {
                    return strSelected;
                }

                return cboSpec.Text;
            }
        }

        /// <summary>
        /// Whether the Form should be accepted if there is only one item left in the list.
        /// </summary>
        public bool AllowAutoSelect { get; } = true;

        /// <summary>
        /// Type of skill that we're selecting. Used to differentiate knowledge skills.
        /// </summary>
        public string Mode { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (!string.IsNullOrEmpty(SelectedItem))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        #endregion Methods
    }
}
