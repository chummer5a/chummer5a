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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class CreateNaturalWeapon : Form
    {
        private readonly XPathNavigator _objXmlPowersDocument;
        private readonly XPathNavigator _objXmlSkillsDocument;

        private readonly Character _objCharacter;
        private Weapon _objWeapon;

        #region Control Events

        public CreateNaturalWeapon(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _objXmlPowersDocument = _objCharacter.LoadDataXPath("critterpowers.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _objXmlSkillsDocument = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNodeAndCacheExpression("/chummer");

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            MoveControls();
        }

        private async void CreateNaturalWeapon_Load(object sender, EventArgs e)
        {
            // Load the list of Combat Active Skills and populate the Skills list.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDVBase))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDVType))
            {
                foreach (XPathNavigator objXmlSkill in _objXmlSkillsDocument.SelectAndCacheExpression(
                             "skills/skill[category = \"Combat Active\"]"))
                {
                    string strName = objXmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstSkills.Add(new ListItem(
                                          strName,
                                          objXmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value
                                          ?? strName));
                }

                lstDVBase.Add(new ListItem("(STR/2)", '(' + _objCharacter.STR.DisplayAbbrev + "/2)"));
                lstDVBase.Add(new ListItem("(STR)", '(' + _objCharacter.STR.DisplayAbbrev + ')'));
                for (int i = 1; i <= 20; ++i)
                {
                    lstDVBase.Add(new ListItem(i.ToString(GlobalSettings.InvariantCultureInfo),
                                               i.ToString(GlobalSettings.CultureInfo)));
                }

                lstDVType.Add(new ListItem("P", await LanguageManager.GetStringAsync("String_DamagePhysical")));
                lstDVType.Add(new ListItem("S", await LanguageManager.GetStringAsync("String_DamageStun")));

                // Bind the Lists to the ComboBoxes.
                cboSkill.BeginUpdate();
                cboSkill.PopulateWithListItems(lstSkills);
                cboSkill.SelectedIndex = 0;
                cboSkill.EndUpdate();

                cboDVBase.BeginUpdate();
                cboDVBase.PopulateWithListItems(lstDVBase);
                cboDVBase.SelectedIndex = 0;
                cboDVBase.EndUpdate();

                cboDVType.BeginUpdate();
                cboDVType.PopulateWithListItems(lstDVType);
                cboDVType.SelectedIndex = 0;
                cboDVType.EndUpdate();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm();
        }

        #endregion Control Events

        #region Methods

        private void MoveControls()
        {
            int intWidth = Math.Max(lblName.Width, lblDV.Width);
            intWidth = Math.Max(intWidth, lblAP.Width);
            intWidth = Math.Max(intWidth, lblActiveSkill.Width);
            intWidth = Math.Max(intWidth, lblReach.Width);
            txtName.Left = lblName.Left + intWidth + 6;
            cboSkill.Left = lblActiveSkill.Left + intWidth + 6;
            cboDVBase.Left = lblDV.Left + intWidth + 6;
            lblDVPlus.Left = cboDVBase.Left + cboDVBase.Width + 6;
            nudDVMod.Left = lblDVPlus.Left + lblDVPlus.Width + 6;
            cboDVType.Left = nudDVMod.Left + nudDVMod.Width + 6;
            nudAP.Left = lblAP.Left + intWidth + 6;
            nudReach.Left = lblReach.Left + intWidth + 6;
        }

        private async ValueTask AcceptForm()
        {
            // Assemble the DV from the fields.
            string strDamage = cboDVBase.SelectedValue.ToString();
            if (nudDVMod.ValueAsInt != 0)
            {
                if (nudDVMod.Value < 0)
                    strDamage += nudDVMod.Value.ToString(GlobalSettings.InvariantCultureInfo);
                else
                    strDamage += '+' + nudDVMod.Value.ToString(GlobalSettings.InvariantCultureInfo);
            }
            strDamage += cboDVType.SelectedValue.ToString();

            // Create the AP value.
            string strAP;
            if (nudAP.Value == 0)
                strAP = "0";
            else if (nudAP.Value > 0)
                strAP = '+' + nudAP.Value.ToString(GlobalSettings.InvariantCultureInfo);
            else
                strAP = nudAP.Value.ToString(GlobalSettings.InvariantCultureInfo);

            // Get the information for the Natural Weapon Critter Power.
            XPathNavigator objPower = _objXmlPowersDocument.SelectSingleNode("powers/power[name = \"Natural Weapon\"]");

            if (objPower != null)
            {
                // Create the Weapon.
                _objWeapon = new Weapon(_objCharacter)
                {
                    Name = txtName.Text,
                    Category = await LanguageManager.GetStringAsync("Tab_Critter"),
                    RangeType = "Melee",
                    Reach = nudReach.ValueAsInt,
                    Damage = strDamage,
                    AP = strAP,
                    Mode = "0",
                    RC = "0",
                    Concealability = 0,
                    Avail = "0",
                    Cost = "0",
                    UseSkill = cboSkill.SelectedValue.ToString(),
                    Source = objPower.SelectSingleNodeAndCacheExpression("source")?.Value,
                    Page = objPower.SelectSingleNodeAndCacheExpression("page")?.Value
                };

                DialogResult = DialogResult.OK;
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Weapon that was created as a result of the dialogue.
        /// </summary>
        public Weapon SelectedWeapon => _objWeapon;

        #endregion Properties
    }
}
