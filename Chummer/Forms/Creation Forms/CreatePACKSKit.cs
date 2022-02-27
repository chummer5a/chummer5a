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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class CreatePACKSKit : Form
    {
        [NotNull] private readonly Character _objCharacter;

        #region Control Events

        public CreatePACKSKit(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the kit and file name fields are populated.
            if (string.IsNullOrEmpty(txtName.Text))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_KitName"), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_KitName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_FileName"), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_FileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the file name starts with custom and ends with _packs.xml.
            if (!txtFileName.Text.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) || !txtFileName.Text.EndsWith("_packs.xml", StringComparison.OrdinalIgnoreCase))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_InvalidFileName"), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_InvalidFileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // See if a Kit with this name already exists for the Custom category.
            // This was originally done without the XmlManager, but because amends and overrides and toggling custom data directories can change names, we need to use it.
            string strName = txtName.Text;
            if ((await XmlManager.LoadXPathAsync("packs.xml", _objCharacter.Settings.EnabledCustomDataDirectoryPaths))
                .SelectSingleNode("/chummer/packs/pack[name = " + strName.CleanXPath() + " and category = \"Custom\"]") != null)
            {
                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_DuplicateName"), strName),
                    await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_DuplicateName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strPath = Path.Combine(Utils.GetStartupPath, "data", txtFileName.Text);

            // If this is not a new file, read in the existing contents.
            XmlDocument objXmlCurrentDocument = null;
            if (File.Exists(strPath))
            {
                try
                {
                    objXmlCurrentDocument = new XmlDocument { XmlResolver = null };
                    objXmlCurrentDocument.LoadStandard(strPath);
                }
                catch (IOException ex)
                {
                    Program.ShowMessageBox(this, ex.ToString());
                    return;
                }
                catch (XmlException ex)
                {
                    Program.ShowMessageBox(this, ex.ToString());
                    return;
                }
            }

            using (FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    await objWriter.WriteStartDocumentAsync();

                    // <chummer>
                    await objWriter.WriteStartElementAsync("chummer");
                    // <packs>
                    await objWriter.WriteStartElementAsync("packs");

                    // If this is not a new file, write out the current contents.
                    if (objXmlCurrentDocument != null)
                    {
                        XmlNode xmlExistingPacksNode = objXmlCurrentDocument.SelectSingleNode("/chummer/packs");
                        xmlExistingPacksNode?.WriteContentTo(objWriter);
                    }

                    // <pack>
                    await objWriter.WriteStartElementAsync("pack");
                    // <name />
                    await objWriter.WriteElementStringAsync("name", txtName.Text);
                    // <category />
                    await objWriter.WriteElementStringAsync("category", "Custom");

                    // Export Attributes.
                    if (chkAttributes.Checked)
                    {
                        int intBOD = _objCharacter.BOD.Value - (_objCharacter.BOD.MetatypeMinimum - 1);
                        int intAGI = _objCharacter.AGI.Value - (_objCharacter.AGI.MetatypeMinimum - 1);
                        int intREA = _objCharacter.REA.Value - (_objCharacter.REA.MetatypeMinimum - 1);
                        int intSTR = _objCharacter.STR.Value - (_objCharacter.STR.MetatypeMinimum - 1);
                        int intCHA = _objCharacter.CHA.Value - (_objCharacter.CHA.MetatypeMinimum - 1);
                        int intINT = _objCharacter.INT.Value - (_objCharacter.INT.MetatypeMinimum - 1);
                        int intLOG = _objCharacter.LOG.Value - (_objCharacter.LOG.MetatypeMinimum - 1);
                        int intWIL = _objCharacter.WIL.Value - (_objCharacter.WIL.MetatypeMinimum - 1);
                        int intEDG = _objCharacter.EDG.Value - (_objCharacter.EDG.MetatypeMinimum - 1);
                        int intMAG = _objCharacter.MAG.Value - (_objCharacter.MAG.MetatypeMinimum - 1);
                        int intMAGAdept = _objCharacter.MAGAdept.Value - (_objCharacter.MAGAdept.MetatypeMinimum - 1);
                        int intDEP = _objCharacter.DEP.Value - (_objCharacter.DEP.MetatypeMinimum - 1);
                        int intRES = _objCharacter.RES.Value - (_objCharacter.RES.MetatypeMinimum - 1);
                        // <attributes>
                        await objWriter.WriteStartElementAsync("attributes");
                        await objWriter.WriteElementStringAsync("bod", intBOD.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("agi", intAGI.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("rea", intREA.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("str", intSTR.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("cha", intCHA.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("int", intINT.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("log", intLOG.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("wil", intWIL.ToString(GlobalSettings.InvariantCultureInfo));
                        await objWriter.WriteElementStringAsync("edg", intEDG.ToString(GlobalSettings.InvariantCultureInfo));
                        if (_objCharacter.MAGEnabled)
                        {
                            await objWriter.WriteElementStringAsync("mag", intMAG.ToString(GlobalSettings.InvariantCultureInfo));
                            if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                                await objWriter.WriteElementStringAsync("magadept", intMAGAdept.ToString(GlobalSettings.InvariantCultureInfo));
                        }

                        if (_objCharacter.RESEnabled)
                            await objWriter.WriteElementStringAsync("res", intRES.ToString(GlobalSettings.InvariantCultureInfo));
                        if (_objCharacter.DEPEnabled)
                            await objWriter.WriteElementStringAsync("dep", intDEP.ToString(GlobalSettings.InvariantCultureInfo));
                        // </attributes>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Qualities.
                    if (chkQualities.Checked)
                    {
                        bool blnPositive = false;
                        bool blnNegative = false;
                        // Determine if Positive or Negative Qualities exist.
                        foreach (Quality objQuality in _objCharacter.Qualities)
                        {
                            switch (objQuality.Type)
                            {
                                case QualityType.Positive:
                                    blnPositive = true;
                                    break;

                                case QualityType.Negative:
                                    blnNegative = true;
                                    break;
                            }

                            if (blnPositive && blnNegative)
                                break;
                        }

                        // <qualities>
                        await objWriter.WriteStartElementAsync("qualities");

                        // Positive Qualities.
                        if (blnPositive)
                        {
                            // <positive>
                            await objWriter.WriteStartElementAsync("positive");
                            foreach (Quality objQuality in _objCharacter.Qualities)
                            {
                                if (objQuality.Type == QualityType.Positive)
                                {
                                    await objWriter.WriteStartElementAsync("quality");
                                    if (!string.IsNullOrEmpty(objQuality.Extra))
                                        await objWriter.WriteAttributeStringAsync("select", objQuality.Extra);
                                    objWriter.WriteValue(objQuality.Name);
                                    await objWriter.WriteEndElementAsync();
                                }
                            }

                            // </positive>
                            await objWriter.WriteEndElementAsync();
                        }

                        // Negative Qualities.
                        if (blnPositive)
                        {
                            // <negative>
                            await objWriter.WriteStartElementAsync("negative");
                            foreach (Quality objQuality in _objCharacter.Qualities)
                            {
                                if (objQuality.Type == QualityType.Negative)
                                {
                                    await objWriter.WriteStartElementAsync("quality");
                                    if (!string.IsNullOrEmpty(objQuality.Extra))
                                        await objWriter.WriteAttributeStringAsync("select", objQuality.Extra);
                                    objWriter.WriteValue(objQuality.Name);
                                    await objWriter.WriteEndElementAsync();
                                }
                            }

                            // </negative>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </qualities>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Starting Nuyen.
                    if (chkStartingNuyen.Checked)
                    {
                        decimal decNuyenBP = _objCharacter.NuyenBP;
                        if (!_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                            decNuyenBP /= 2.0m;
                        await objWriter.WriteElementStringAsync("nuyenbp", decNuyenBP.ToString(GlobalSettings.InvariantCultureInfo));
                    }

                    /* TODO: Add support for active and knowledge skills and skill groups
                    // Export Active Skills.
                    if (chkActiveSkills.Checked)
                    {
                        // <skills>
                        await objWriter.WriteStartElementAsync("skills");

                        // Active Skills.
                        foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (!objSkill.IsKnowledgeSkill && objSkill.Rating > 0)
                            {
                                // <skill>
                                await objWriter.WriteStartElementAsync("skill");
                                await objWriter.WriteElementStringAsync("name", objSkill.Name);
                                await objWriter.WriteElementStringAsync("rating", objSkill.Rating.ToString());
                                if (!string.IsNullOrEmpty(objSkill.Specialization))
                                    await objWriter.WriteElementStringAsync("spec", objSkill.Specialization);
                                // </skill>
                                await objWriter.WriteEndElementAsync();
                            }
                        }

                        // Skill Groups.
                        foreach (SkillGroup objSkillGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objSkillGroup.BaseUnbroken && objSkillGroup.Rating > 0)
                            {
                                // <skillgroup>
                                await objWriter.WriteStartElementAsync("skillgroup");
                                await objWriter.WriteElementStringAsync("name", objSkillGroup.Name);
                                await objWriter.WriteElementStringAsync("rating", objSkillGroup.Rating.ToString());
                                // </skillgroup>
                                await objWriter.WriteEndElementAsync();
                            }
                        }
                        // </skills>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Knowledge Skills.
                    if (chkKnowledgeSkills.Checked)
                    {
                        // <knowledgeskills>
                        await objWriter.WriteStartElementAsync("knowledgeskills");
                        foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.Skills.OfType<KnowledgeSkill>())
                        {
                            // <skill>
                            await objWriter.WriteStartElementAsync("skill");
                            await objWriter.WriteElementStringAsync("name", objSkill.Name);
                            await objWriter.WriteElementStringAsync("rating", objSkill.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            if (!string.IsNullOrEmpty(objSkill.Specialization))
                                await objWriter.WriteElementStringAsync("spec", objSkill.Specialization);
                            await objWriter.WriteElementStringAsync("category", objSkill.SkillCategory);
                            // </skill>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </knowledgeskills>
                        await objWriter.WriteEndElementAsync();
                    }
                    */

                    // Export Martial Arts.
                    if (chkMartialArts.Checked)
                    {
                        // <martialarts>
                        await objWriter.WriteStartElementAsync("martialarts");
                        foreach (MartialArt objArt in _objCharacter.MartialArts)
                        {
                            // <martialart>
                            await objWriter.WriteStartElementAsync("martialart");
                            await objWriter.WriteElementStringAsync("name", objArt.Name);
                            if (objArt.Techniques.Count > 0)
                            {
                                // <techniques>
                                await objWriter.WriteStartElementAsync("techniques");
                                foreach (MartialArtTechnique objTechnique in objArt.Techniques)
                                    await objWriter.WriteElementStringAsync("technique", objTechnique.Name);
                                // </techniques>
                                await objWriter.WriteEndElementAsync();
                            }

                            // </martialart>
                            await objWriter.WriteEndElementAsync();
                        }
                        // </martialarts>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Spells.
                    if (chkSpells.Checked)
                    {
                        // <spells>
                        await objWriter.WriteStartElementAsync("spells");
                        foreach (Spell objSpell in _objCharacter.Spells)
                        {
                            await objWriter.WriteStartElementAsync("spell");
                            await objWriter.WriteStartElementAsync("name");
                            if (!string.IsNullOrEmpty(objSpell.Extra))
                                await objWriter.WriteAttributeStringAsync("select", objSpell.Extra);
                            objWriter.WriteValue(objSpell.Name);
                            await objWriter.WriteEndElementAsync();
                            await objWriter.WriteElementStringAsync("category", objSpell.Category);
                            await objWriter.WriteEndElementAsync();
                        }

                        // </spells>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Complex Forms.
                    if (chkComplexForms.Checked)
                    {
                        // <programs>
                        await objWriter.WriteStartElementAsync("complexforms");
                        foreach (ComplexForm objComplexForm in _objCharacter.ComplexForms)
                        {
                            // <program>
                            await objWriter.WriteStartElementAsync("complexform");
                            await objWriter.WriteStartElementAsync("name");
                            if (!string.IsNullOrEmpty(objComplexForm.Extra))
                                await objWriter.WriteAttributeStringAsync("select", objComplexForm.Extra);
                            objWriter.WriteValue(objComplexForm.Name);
                            await objWriter.WriteEndElementAsync();
                            await objWriter.WriteEndElementAsync();
                            // </program>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </programs>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Cyberware/Bioware.
                    if (chkCyberware.Checked)
                    {
                        bool blnCyberware = false;
                        bool blnBioware = false;
                        foreach (Cyberware objCharacterCyberware in _objCharacter.Cyberware)
                        {
                            switch (objCharacterCyberware.SourceType)
                            {
                                case Improvement.ImprovementSource.Bioware:
                                    blnBioware = true;
                                    break;

                                case Improvement.ImprovementSource.Cyberware:
                                    blnCyberware = true;
                                    break;
                            }

                            if (blnCyberware && blnBioware)
                                break;
                        }

                        if (blnCyberware)
                        {
                            // <cyberwares>
                            await objWriter.WriteStartElementAsync("cyberwares");
                            foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                            {
                                if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                                {
                                    // <cyberware>
                                    await objWriter.WriteStartElementAsync("cyberware");
                                    await objWriter.WriteElementStringAsync("name", objCyberware.Name);
                                    if (objCyberware.Rating > 0)
                                        await objWriter.WriteElementStringAsync("rating", objCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    await objWriter.WriteElementStringAsync("grade", objCyberware.Grade.Name);
                                    if (objCyberware.Children.Count > 0)
                                    {
                                        // <cyberwares>
                                        await objWriter.WriteStartElementAsync("cyberwares");
                                        foreach (Cyberware objChildCyberware in objCyberware.Children)
                                        {
                                            if (objChildCyberware.Capacity != "[*]")
                                            {
                                                // <cyberware>
                                                await objWriter.WriteStartElementAsync("cyberware");
                                                await objWriter.WriteElementStringAsync("name", objChildCyberware.Name);
                                                if (objChildCyberware.Rating > 0)
                                                    await objWriter.WriteElementStringAsync("rating", objChildCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));

                                                if (objChildCyberware.GearChildren.Count > 0)
                                                    await WriteGear(objWriter, objChildCyberware.GearChildren);
                                                // </cyberware>
                                                await objWriter.WriteEndElementAsync();
                                            }
                                        }

                                        // </cyberwares>
                                        await objWriter.WriteEndElementAsync();
                                    }

                                    if (objCyberware.GearChildren.Count > 0)
                                        await WriteGear(objWriter, objCyberware.GearChildren);

                                    // </cyberware>
                                    await objWriter.WriteEndElementAsync();
                                }
                            }

                            // </cyberwares>
                            await objWriter.WriteEndElementAsync();
                        }

                        if (blnBioware)
                        {
                            // <biowares>
                            await objWriter.WriteStartElementAsync("biowares");
                            foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                            {
                                if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                                {
                                    // <bioware>
                                    await objWriter.WriteStartElementAsync("bioware");
                                    await objWriter.WriteElementStringAsync("name", objCyberware.Name);
                                    if (objCyberware.Rating > 0)
                                        await objWriter.WriteElementStringAsync("rating", objCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    await objWriter.WriteElementStringAsync("grade", objCyberware.Grade.ToString());

                                    if (objCyberware.GearChildren.Count > 0)
                                        await WriteGear(objWriter, objCyberware.GearChildren);
                                    // </bioware>
                                    await objWriter.WriteEndElementAsync();
                                }
                            }

                            // </biowares>
                            await objWriter.WriteEndElementAsync();
                        }
                    }

                    // Export Lifestyle.
                    if (chkLifestyle.Checked)
                    {
                        // <lifestyles>
                        await objWriter.WriteStartElementAsync("lifestyles");
                        foreach (Lifestyle objLifestyle in _objCharacter.Lifestyles)
                        {
                            // <lifestyle>
                            await objWriter.WriteStartElementAsync("lifestyle");
                            await objWriter.WriteElementStringAsync("name", objLifestyle.Name);
                            await objWriter.WriteElementStringAsync("months", objLifestyle.Increments.ToString(GlobalSettings.InvariantCultureInfo));
                            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
                            {
                                // This is an Advanced Lifestyle, so write out its properties.
                                await objWriter.WriteElementStringAsync("cost", objLifestyle.Cost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo));
                                await objWriter.WriteElementStringAsync("dice", objLifestyle.Dice.ToString(GlobalSettings.InvariantCultureInfo));
                                await objWriter.WriteElementStringAsync("multiplier", objLifestyle.Multiplier.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo));
                                await objWriter.WriteElementStringAsync("baselifestyle", objLifestyle.BaseLifestyle);
                                if (objLifestyle.LifestyleQualities.Count > 0)
                                {
                                    // <qualities>
                                    await objWriter.WriteStartElementAsync("qualities");
                                    foreach (LifestyleQuality objQuality in objLifestyle.LifestyleQualities)
                                        await objWriter.WriteElementStringAsync("quality", objQuality.Name);
                                    // </qualities>
                                    await objWriter.WriteEndElementAsync();
                                }
                            }

                            // </lifestyle>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </lifestyles>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Armor.
                    if (chkArmor.Checked)
                    {
                        // <armors>
                        await objWriter.WriteStartElementAsync("armors");
                        foreach (Armor objArmor in _objCharacter.Armor)
                        {
                            // <armor>
                            await objWriter.WriteStartElementAsync("armor");
                            await objWriter.WriteElementStringAsync("name", objArmor.Name);
                            if (objArmor.ArmorMods.Count > 0)
                            {
                                // <mods>
                                await objWriter.WriteStartElementAsync("mods");
                                foreach (ArmorMod objMod in objArmor.ArmorMods)
                                {
                                    // <mod>
                                    await objWriter.WriteStartElementAsync("mod");
                                    await objWriter.WriteElementStringAsync("name", objMod.Name);
                                    if (objMod.Rating > 0)
                                        await objWriter.WriteElementStringAsync("rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    // </mod>
                                    await objWriter.WriteEndElementAsync();
                                }

                                // </mods>
                                await objWriter.WriteEndElementAsync();
                            }

                            if (objArmor.GearChildren.Count > 0)
                                await WriteGear(objWriter, objArmor.GearChildren);

                            // </armor>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </armors>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Weapons.
                    if (chkWeapons.Checked)
                    {
                        // <weapons>
                        await objWriter.WriteStartElementAsync("weapons");
                        foreach (Weapon objWeapon in _objCharacter.Weapons)
                        {
                            // Don't attempt to export Cyberware and Gear Weapons since those are handled by those object types. The default Unarmed Attack Weapon should also not be exported.
                            if (objWeapon.Category != "Cyberware" && objWeapon.Category != "Gear" && objWeapon.Name != "Unarmed Attack")
                            {
                                // <weapon>
                                await objWriter.WriteStartElementAsync("weapon");
                                await objWriter.WriteElementStringAsync("name", objWeapon.Name);

                                // Weapon Accessories.
                                if (objWeapon.WeaponAccessories.Count > 0)
                                {
                                    // <accessories>
                                    await objWriter.WriteStartElementAsync("accessories");
                                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                    {
                                        // Don't attempt to export items included in the Weapon.
                                        if (!objAccessory.IncludedInWeapon)
                                        {
                                            // <accessory>
                                            await objWriter.WriteStartElementAsync("accessory");
                                            await objWriter.WriteElementStringAsync("name", objAccessory.Name);
                                            await objWriter.WriteElementStringAsync("mount", objAccessory.Mount);
                                            await objWriter.WriteElementStringAsync("extramount", objAccessory.ExtraMount);

                                            if (objAccessory.GearChildren.Count > 0)
                                                await WriteGear(objWriter, objAccessory.GearChildren);

                                            // </accessory>
                                            await objWriter.WriteEndElementAsync();
                                        }
                                    }

                                    // </accessories>
                                    await objWriter.WriteEndElementAsync();
                                }

                                // Underbarrel Weapon.
                                if (objWeapon.UnderbarrelWeapons.Count > 0)
                                {
                                    foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                                    {
                                        if (!objUnderbarrelWeapon.IncludedInWeapon)
                                            await objWriter.WriteElementStringAsync("underbarrel", objUnderbarrelWeapon.Name);
                                    }
                                }

                                // </weapon>
                                await objWriter.WriteEndElementAsync();
                            }
                        }

                        // </weapons>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Gear.
                    if (chkGear.Checked)
                    {
                        await WriteGear(objWriter, _objCharacter.Gear);
                    }

                    // Export Vehicles.
                    if (chkVehicles.Checked)
                    {
                        // <vehicles>
                        await objWriter.WriteStartElementAsync("vehicles");
                        foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                        {
                            bool blnWeapons = false;
                            // <vehicle>
                            await objWriter.WriteStartElementAsync("vehicle");
                            await objWriter.WriteElementStringAsync("name", objVehicle.Name);
                            if (objVehicle.Mods.Count > 0)
                            {
                                // <mods>
                                await objWriter.WriteStartElementAsync("mods");
                                foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                                {
                                    // Only write out the Mods that are not part of the base vehicle.
                                    if (!objVehicleMod.IncludedInVehicle)
                                    {
                                        // <mod>
                                        await objWriter.WriteStartElementAsync("mod");
                                        await objWriter.WriteElementStringAsync("name", objVehicleMod.Name);
                                        if (objVehicleMod.Rating > 0)
                                            await objWriter.WriteElementStringAsync("rating", objVehicleMod.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                        // </mod>
                                        await objWriter.WriteEndElementAsync();

                                        // See if this is a Weapon Mount with Weapons.
                                        if (objVehicleMod.Weapons.Count > 0)
                                            blnWeapons = true;
                                    }
                                    else
                                    {
                                        // See if this is a Weapon Mount with Weapons.
                                        if (objVehicleMod.Weapons.Count > 0)
                                            blnWeapons = true;
                                    }
                                }

                                // </mods>
                                await objWriter.WriteEndElementAsync();
                            }

                            // If there are Weapons, add them.
                            if (blnWeapons)
                            {
                                // <weapons>
                                await objWriter.WriteStartElementAsync("weapons");
                                foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                                {
                                    foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                    {
                                        // <weapon>
                                        await objWriter.WriteStartElementAsync("weapon");
                                        await objWriter.WriteElementStringAsync("name", objWeapon.Name);

                                        // Weapon Accessories.
                                        if (objWeapon.WeaponAccessories.Count > 0)
                                        {
                                            // <accessories>
                                            await objWriter.WriteStartElementAsync("accessories");
                                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                            {
                                                // Don't attempt to export items included in the Weapon.
                                                if (!objAccessory.IncludedInWeapon)
                                                {
                                                    // <accessory>
                                                    await objWriter.WriteStartElementAsync("accessory");
                                                    await objWriter.WriteElementStringAsync("name", objAccessory.Name);
                                                    await objWriter.WriteElementStringAsync("mount", objAccessory.Mount);
                                                    await objWriter.WriteElementStringAsync("extramount", objAccessory.ExtraMount);
                                                    // </accessory>
                                                    await objWriter.WriteEndElementAsync();
                                                }
                                            }

                                            // </accessories>
                                            await objWriter.WriteEndElementAsync();
                                        }

                                        // Underbarrel Weapon.
                                        if (objWeapon.UnderbarrelWeapons.Count > 0)
                                        {
                                            foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                                                await objWriter.WriteElementStringAsync("underbarrel", objUnderbarrelWeapon.Name);
                                        }

                                        // </weapon>
                                        await objWriter.WriteEndElementAsync();
                                    }
                                }

                                // </weapons>
                                await objWriter.WriteEndElementAsync();
                            }

                            // Gear.
                            if (objVehicle.GearChildren.Count > 0)
                            {
                                await WriteGear(objWriter, objVehicle.GearChildren);
                            }

                            // </vehicle>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </vehicles>
                        await objWriter.WriteEndElementAsync();
                    }

                    // </pack>
                    await objWriter.WriteEndElementAsync();
                    // </packs>
                    await objWriter.WriteEndElementAsync();
                    // </chummer>
                    await objWriter.WriteEndElementAsync();

                    await objWriter.WriteEndDocumentAsync();
                }
            }

            Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_SuiteCreated"), txtName.Text),
                await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_SuiteCreated"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Recursively write out all Gear information since these can be nested pretty deep.
        /// </summary>
        /// <param name="objWriter">XmlWriter to use.</param>
        /// <param name="lstGear">List of Gear to write.</param>
        private static async ValueTask WriteGear(XmlWriter objWriter, IEnumerable<Gear> lstGear)
        {
            // <gears>
            await objWriter.WriteStartElementAsync("gears");
            foreach (Gear objGear in lstGear)
            {
                if (objGear.IncludedInParent)
                    continue;
                // <gear>
                await objWriter.WriteStartElementAsync("gear");
                await objWriter.WriteStartElementAsync("name");
                if (!string.IsNullOrEmpty(objGear.Extra))
                    await objWriter.WriteAttributeStringAsync("select", objGear.Extra);
                objWriter.WriteValue(objGear.Name);
                await objWriter.WriteEndElementAsync();
                await objWriter.WriteElementStringAsync("category", objGear.Category);
                if (objGear.Rating > 0)
                    await objWriter.WriteElementStringAsync("rating", objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                if (objGear.Quantity != 1)
                    await objWriter.WriteElementStringAsync("qty", objGear.Quantity.ToString(GlobalSettings.InvariantCultureInfo));
                if (objGear.Children.Count > 0)
                    await WriteGear(objWriter, objGear.Children);
                // </gear>
                await objWriter.WriteEndElementAsync();
            }
            // </gears>
            await objWriter.WriteEndElementAsync();
        }

        #endregion Methods
    }
}
