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
 using System.Xml.XPath;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmNaturalWeapon : Form
    {
        private readonly XPathNavigator _objXmlPowersDocument;
        private readonly XPathNavigator _objXmlSkillsDocument;

        private readonly Character _objCharacter;
        private Weapon _objWeapon;

        #region Control Events
        public frmNaturalWeapon(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _objXmlPowersDocument = XmlManager.Load("critterpowers.xml").GetFastNavigator().SelectSingleNode("/chummer");
            _objXmlSkillsDocument = XmlManager.Load("skills.xml").GetFastNavigator().SelectSingleNode("/chummer");

            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void frmNaturalWeapon_Load(object sender, EventArgs e)
        {
            // Load the list of Combat Active Skills and populate the Skills list.
            List<ListItem> lstSkills = new List<ListItem>();
            foreach (XPathNavigator objXmlSkill in _objXmlSkillsDocument.Select("skills/skill[category = \"Combat Active\"]"))
            {
                string strName = objXmlSkill.SelectSingleNode("name")?.Value;
                if (!string.IsNullOrEmpty(strName))
                    lstSkills.Add(new ListItem(strName, objXmlSkill.SelectSingleNode("translate")?.Value ?? strName));
            }

            List<ListItem> lstDVBase = new List<ListItem>
            {
                new ListItem("(STR/2)", '(' + _objCharacter.STR.DisplayAbbrev + "/2)"),
                new ListItem("(STR)", '(' + _objCharacter.STR.DisplayAbbrev + ')')
            };
            for (int i = 1; i <= 20; ++i)
            {
                lstDVBase.Add(new ListItem(i.ToString(), i.ToString()));
            }

            List<ListItem> lstDVType = new List<ListItem>
            {
                new ListItem("P", LanguageManager.GetString("String_DamagePhysical", GlobalOptions.Instance.Language)),
                new ListItem("S", LanguageManager.GetString("String_DamageStun", GlobalOptions.Instance.Language))
            };

            // Bind the Lists to the ComboBoxes.
            cboSkill.BeginUpdate();
            cboSkill.ValueMember = "Value";
            cboSkill.DisplayMember = "Name";
            cboSkill.DataSource = lstSkills;
            cboSkill.SelectedIndex = 0;
            cboSkill.EndUpdate();

            cboDVBase.BeginUpdate();
            cboDVBase.ValueMember = "Value";
            cboDVBase.DisplayMember = "Name";
            cboDVBase.DataSource = lstDVBase;
            cboDVBase.SelectedIndex = 0;
            cboDVBase.EndUpdate();

            cboDVType.BeginUpdate();
            cboDVType.ValueMember = "Value";
            cboDVType.DisplayMember = "Name";
            cboDVType.DataSource = lstDVType;
            cboDVType.SelectedIndex = 0;
            cboDVType.EndUpdate();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }
        #endregion

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

        private void AcceptForm()
        {
            // Assemble the DV from the fields.
            string strDamage = cboDVBase.SelectedValue.ToString();
            if (decimal.ToInt32(nudDVMod.Value) != 0)
            {
                if (nudDVMod.Value < 0)
                    strDamage += nudDVMod.Value.ToString(GlobalOptions.Instance.InvariantCultureInfo);
                else
                    strDamage += '+' + nudDVMod.Value.ToString(GlobalOptions.Instance.InvariantCultureInfo);
            }
            strDamage += cboDVType.SelectedValue.ToString();

            // Create the AP value.
            string strAP;
            if (nudAP.Value == 0)
                strAP = "0";
            else if (nudAP.Value > 0)
                strAP = '+' + nudAP.Value.ToString(GlobalOptions.Instance.InvariantCultureInfo);
            else
                strAP = nudAP.Value.ToString(GlobalOptions.Instance.InvariantCultureInfo);

            // Get the information for the Natural Weapon Critter Power.
            XPathNavigator objPower = _objXmlPowersDocument.SelectSingleNode("powers/power[name = \"Natural Weapon\"]");

            if (objPower != null)
            {
                // Create the Weapon.
                _objWeapon = new Weapon(_objCharacter)
                {
                    Name = txtName.Text,
                    Category = LanguageManager.GetString("Tab_Critter", GlobalOptions.Instance.Language),
                    WeaponType = "Melee",
                    Reach = decimal.ToInt32(nudReach.Value),
                    Damage = strDamage,
                    AP = strAP,
                    Mode = "0",
                    RC = "0",
                    Concealability = 0,
                    Avail = "0",
                    Cost = "0",
                    UseSkill = cboSkill.SelectedValue.ToString(),
                    Source = objPower.SelectSingleNode("source")?.Value,
                    Page = objPower.SelectSingleNode("page")?.Value
                };

                DialogResult = DialogResult.OK;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapon that was created as a result of the dialogue.
        /// </summary>
        public Weapon SelectedWeapon => _objWeapon;

        #endregion
    }
}
