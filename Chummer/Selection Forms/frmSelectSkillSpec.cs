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

namespace Chummer
{
    public partial class frmSelectSpec : Form
    {
        private readonly Skill _objSkill;
        private readonly Character _objCharacter;
        private readonly string _strForceItem = string.Empty;
        private readonly XmlDocument _objXmlDocument;

        #region Control Events
        public frmSelectSpec(Skill skill)
        {
            _objSkill = skill ?? throw new ArgumentNullException(nameof(skill));
            _objCharacter = skill.CharacterObject;
            InitializeComponent();
            this.TranslateWinForm();
            _objXmlDocument = XmlManager.Load("skills.xml");
        }

        private void frmSelectSpec_Load(object sender, EventArgs e)
        {
            List<ListItem> lstItems = new List<ListItem>
            {
                new ListItem("Custom", string.Empty)
            };

            if (_objCharacter.Created || _objCharacter.BuildMethod == CharacterBuildMethod.Karma || _objCharacter.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                chkKarma.Checked = true;
                chkKarma.Visible = false;
            }
            XmlNode xmlParentSkill;
            if (Mode == "Knowledge")
                xmlParentSkill = _objXmlDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + _objSkill.Name + "\"]") ??
                    _objXmlDocument.SelectSingleNode("/chummer/knowledgeskills/skill[translate = \"" + _objSkill.Name + "\"]");
            else
                xmlParentSkill = _objXmlDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + _objSkill.Name + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
            // Populate the Skill's Specializations (if any).
            using (XmlNodeList xmlSpecList = xmlParentSkill?.SelectNodes("specs/spec"))
            {
                if (xmlSpecList != null)
                {
                    foreach (XmlNode objXmlSpecialization in xmlSpecList)
                    {
                        string strInnerText = objXmlSpecialization.InnerText;
                        lstItems.Add(new ListItem(strInnerText, objXmlSpecialization.Attributes?["translate"]?.InnerText ?? strInnerText));

                        if (_objSkill.SkillCategory != "Combat Active")
                            continue;
                        // Look through the Weapons file and grab the names of items that are part of the appropriate Category or use the matching Skill.
                        XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");
                        //Might need to include skill name or might miss some values?
                        XmlNodeList objXmlWeaponList = objXmlWeaponDocument.SelectNodes("/chummer/weapons/weapon[(spec = \"" + strInnerText + "\" or spec2 = \"" + strInnerText + "\") and (" + _objCharacter.Options.BookXPath() + ")]");
                        if (objXmlWeaponList == null)
                            continue;
                        foreach (XmlNode objXmlWeapon in objXmlWeaponList)
                        {
                            string strName = objXmlWeapon["name"]?.InnerText;
                            lstItems.Add(new ListItem(strName, objXmlWeapon["translate"]?.InnerText ?? strName));
                        }
                    }
                }
            }

            // Populate the lists.
            cboSpec.BeginUpdate();
            cboSpec.ValueMember = "Value";
            cboSpec.DisplayMember = "Name";
            cboSpec.DataSource = lstItems;

            // If there's only 1 value in the list, the character doesn't have a choice, so just accept it.
            if (cboSpec.Items.Count == 1 && cboSpec.DropDownStyle == ComboBoxStyle.DropDownList && AllowAutoSelect)
                AcceptForm();

            if (!string.IsNullOrEmpty(_strForceItem))
            {
                cboSpec.SelectedIndex = cboSpec.FindStringExact(_strForceItem);
                if (cboSpec.SelectedIndex != -1)
                    AcceptForm();
                else
                {
                    cboSpec.DataSource = null;
                    List<ListItem> lstSingle = new List<ListItem>
                    {
                        new ListItem(_strForceItem, _strForceItem)
                    };
                    cboSpec.ValueMember = "Value";
                    cboSpec.DisplayMember = "Name";
                    cboSpec.DataSource = lstSingle;
                    cboSpec.SelectedIndex = 0;
                    AcceptForm();
                }
            }
            cboSpec.EndUpdate();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cboSpec_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSpec.DropDownStyle = cboSpec.SelectedValue?.ToString() == "Custom" ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Name of the item that was selected.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                if (cboSpec.SelectedValue != null && cboSpec.SelectedValue.ToString() != "Custom")
                {
                    return cboSpec.SelectedValue.ToString();
                }
                else
                {
                    return cboSpec.Text;
                }
            }
        }

        /// <summary>
        /// Whether or not the Form should be accepted if there is only one item left in the list.
        /// </summary>
        public bool AllowAutoSelect { get; set; } = true;

        /// <summary>
        /// Type of skill that we're selecting. Used to differentiate knowledge skills.
        /// </summary>
        public string Mode { get; set; }


        /// <summary>
        /// Whether or not to force the .
        /// </summary>
        public bool BuyWithKarma
        {
            get => chkKarma.Checked;
            set => chkKarma.Checked = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (!string.IsNullOrEmpty(SelectedItem))
                DialogResult = DialogResult.OK;
        }
        #endregion
    }
}
