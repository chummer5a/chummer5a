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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class CreateNaturalWeapon : Form, IHasCharacterObject
    {
        private readonly XPathNavigator _objXmlPowersDocument;
        private readonly XPathNavigator _objXmlSkillsDocument;

        private readonly Character _objCharacter;
        private Weapon _objWeapon;

        public Character CharacterObject => _objCharacter;

        #region Control Events

        public CreateNaturalWeapon(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
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
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDVBase))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDVType))
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

                CharacterAttrib objAttribute = await _objCharacter.GetAttributeAsync("STR").ConfigureAwait(false);
                string strAbbrev = await objAttribute.GetCurrentDisplayAbbrevAsync().ConfigureAwait(false);
                lstDVBase.Add(new ListItem("({STR}/2)", "(" + strAbbrev + "/2)"));
                lstDVBase.Add(new ListItem("({STR})", "(" + strAbbrev + ")"));
                for (int i = 1; i <= 20; ++i)
                {
                    lstDVBase.Add(new ListItem(i.ToString(GlobalSettings.InvariantCultureInfo),
                                               i.ToString(GlobalSettings.CultureInfo)));
                }

                lstDVType.Add(new ListItem("P", await LanguageManager.GetStringAsync("String_DamagePhysical").ConfigureAwait(false)));
                lstDVType.Add(new ListItem("S", await LanguageManager.GetStringAsync("String_DamageStun").ConfigureAwait(false)));

                // Bind the Lists to the ComboBoxes.
                await cboSkill.PopulateWithListItemsAsync(lstSkills).ConfigureAwait(false);
                await cboSkill.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);

                await cboDVBase.PopulateWithListItemsAsync(lstDVBase).ConfigureAwait(false);
                await cboDVBase.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);

                await cboDVType.PopulateWithListItemsAsync(lstDVType).ConfigureAwait(false);
                await cboDVType.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm().ConfigureAwait(false);
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

        private async Task AcceptForm(CancellationToken token = default)
        {
            // Assemble the DV from the fields.
            string strDamage = await cboDVBase.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
            int intDVMod = await nudDVMod.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            if (intDVMod != 0)
            {
                if (intDVMod < 0)
                    strDamage += intDVMod.ToString(GlobalSettings.InvariantCultureInfo);
                else
                    strDamage += "+" + intDVMod.ToString(GlobalSettings.InvariantCultureInfo);
            }
            strDamage += await cboDVType.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);

            // Create the AP value.
            string strAP;
            int intAP = await nudAP.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            if (intAP == 0)
                strAP = "0";
            else if (intAP > 0)
                strAP = "+" + intAP.ToString(GlobalSettings.InvariantCultureInfo);
            else
                strAP = intAP.ToString(GlobalSettings.InvariantCultureInfo);

            // Get the information for the Natural Weapon Critter Power.
            XPathNavigator objPower = _objXmlPowersDocument.SelectSingleNodeAndCacheExpression("powers/power[name = \"Natural Weapon\"]", token);

            if (objPower != null)
            {
                // Create the Weapon.
                _objWeapon = new Weapon(_objCharacter);
                try
                {
                    _objWeapon.Name = await txtName.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                    _objWeapon.Category = await LanguageManager.GetStringAsync("Tab_Critter", GlobalSettings.DefaultLanguage, token: token).ConfigureAwait(false);
                    _objWeapon.RangeType = "Melee";
                    _objWeapon.Reach = (await nudReach.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                    _objWeapon.Accuracy = "Physical";
                    _objWeapon.Damage = strDamage;
                    _objWeapon.AP = strAP;
                    _objWeapon.Mode = "0";
                    _objWeapon.RC = "0";
                    _objWeapon.Concealability = "0";
                    _objWeapon.Avail = "0";
                    _objWeapon.Cost = "0";
                    _objWeapon.Ammo = "0";
                    _objWeapon.UseSkill = await cboSkill.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
                    _objWeapon.Source = objPower.SelectSingleNodeAndCacheExpression("source", token: token)?.Value ?? "SR5";
                    _objWeapon.Page = objPower.SelectSingleNodeAndCacheExpression("page", token: token)?.Value ?? "0";
                    await _objWeapon.CreateClipsAsync(token).ConfigureAwait(false);
                }
                catch
                {
                    await _objWeapon.DeleteWeaponAsync(token: CancellationToken.None).ConfigureAwait(false);
                    throw;
                }

                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, token: token).ConfigureAwait(false);
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
