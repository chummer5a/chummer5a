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
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmNaturalWeapon : Form
    {
        private readonly XmlDocument _objXmlPowersDocument = XmlManager.Load("critterpowers.xml");
        private readonly XmlDocument _objXmlSkillsDocument = XmlManager.Load("skills.xml");

        private readonly Character _objCharacter;
        private Weapon _objWeapon;

        #region Control Events
        public frmNaturalWeapon(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmNaturalWeapon_Load(object sender, EventArgs e)
        {
            // Load the list of Combat Active Skills and populate the Skills list.
            List<ListItem> lstSkills = new List<ListItem>();
            foreach (XmlNode objXmlSkill in _objXmlSkillsDocument.SelectNodes("/chummer/skills/skill[category = \"Combat Active\"]"))
            {
                string strName = objXmlSkill["name"].InnerText;
                lstSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
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
                new ListItem("P", LanguageManager.GetString("String_DamagePhysical", GlobalOptions.Language)),
                new ListItem("S", LanguageManager.GetString("String_DamageStun", GlobalOptions.Language))
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
                    strDamage += nudDVMod.Value.ToString(GlobalOptions.InvariantCultureInfo);
                else
                    strDamage += '+' + nudDVMod.Value.ToString(GlobalOptions.InvariantCultureInfo);
            }
            strDamage += cboDVType.SelectedValue.ToString();

            // Create the AP value.
            string strAP = string.Empty;
            if (nudAP.Value == 0)
                strAP = "0";
            else if (nudAP.Value > 0)
                strAP = '+' + nudAP.Value.ToString(GlobalOptions.InvariantCultureInfo);
            else
                strAP = nudAP.Value.ToString(GlobalOptions.InvariantCultureInfo);

            // Get the information for the Natural Weapon Critter Power.
            XmlNode objPower = _objXmlPowersDocument.SelectSingleNode("/chummer/powers/power[name = \"Natural Weapon\"]");

            // Create the Weapon.
            _objWeapon = new Weapon(_objCharacter)
            {
                Name = txtName.Text,
                Category = LanguageManager.GetString("Tab_Critter", GlobalOptions.Language),
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
                Source = objPower["source"].InnerText,
                Page = objPower["page"].InnerText
            };

            DialogResult = DialogResult.OK;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapon that was created as a result of the dialogue.
        /// </summary>
        public Weapon SelectedWeapon
        {
            get
            {
                return _objWeapon;
            }
        }
        #endregion
    }
}
