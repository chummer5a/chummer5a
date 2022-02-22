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
using System.Text;
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
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_KitName"), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_KitName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_FileName"), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_FileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the file name starts with custom and ends with _packs.xml.
            if (!txtFileName.Text.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) || !txtFileName.Text.EndsWith("_packs.xml", StringComparison.OrdinalIgnoreCase))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_InvalidFileName"), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_InvalidFileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // See if a Kit with this name already exists for the Custom category.
            // This was originally done without the XmlManager, but because amends and overrides and toggling custom data directories can change names, we need to use it.
            string strName = txtName.Text;
            if ((await XmlManager.LoadXPathAsync("packs.xml", _objCharacter.Settings.EnabledCustomDataDirectoryPaths))
                .SelectSingleNode("/chummer/packs/pack[name = " + strName.CleanXPath() + " and category = \"Custom\"]") != null)
            {
                Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_DuplicateName"), strName),
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
                    Program.MainForm.ShowMessageBox(this, ex.ToString());
                    return;
                }
                catch (XmlException ex)
                {
                    Program.MainForm.ShowMessageBox(this, ex.ToString());
                    return;
                }
            }

            using (FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 1,
                    IndentChar = '\t'
                })
                {
                    objWriter.WriteStartDocument();

                    // <chummer>
                    objWriter.WriteStartElement("chummer");
                    // <packs>
                    objWriter.WriteStartElement("packs");

                    // If this is not a new file, write out the current contents.
                    if (objXmlCurrentDocument != null)
                    {
                        using (XmlNodeList objXmlNodeList = objXmlCurrentDocument.SelectNodes("/chummer/*"))
                            if (objXmlNodeList?.Count > 0)
                                foreach (XmlNode objXmlNode in objXmlNodeList)
                                    objXmlNode.WriteContentTo(objWriter);
                    }

                    // <pack>
                    objWriter.WriteStartElement("pack");
                    // <name />
                    objWriter.WriteElementString("name", txtName.Text);
                    // <category />
                    objWriter.WriteElementString("category", "Custom");

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
                        objWriter.WriteStartElement("attributes");
                        objWriter.WriteElementString("bod", intBOD.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("agi", intAGI.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("rea", intREA.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("str", intSTR.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("cha", intCHA.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("int", intINT.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("log", intLOG.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("wil", intWIL.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("edg", intEDG.ToString(GlobalSettings.InvariantCultureInfo));
                        if (_objCharacter.MAGEnabled)
                        {
                            objWriter.WriteElementString("mag", intMAG.ToString(GlobalSettings.InvariantCultureInfo));
                            if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                                objWriter.WriteElementString("magadept", intMAGAdept.ToString(GlobalSettings.InvariantCultureInfo));
                        }

                        if (_objCharacter.RESEnabled)
                            objWriter.WriteElementString("res", intRES.ToString(GlobalSettings.InvariantCultureInfo));
                        if (_objCharacter.DEPEnabled)
                            objWriter.WriteElementString("dep", intDEP.ToString(GlobalSettings.InvariantCultureInfo));
                        // </attributes>
                        objWriter.WriteEndElement();
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
                        objWriter.WriteStartElement("qualities");

                        // Positive Qualities.
                        if (blnPositive)
                        {
                            // <positive>
                            objWriter.WriteStartElement("positive");
                            foreach (Quality objQuality in _objCharacter.Qualities)
                            {
                                if (objQuality.Type == QualityType.Positive)
                                {
                                    objWriter.WriteStartElement("quality");
                                    if (!string.IsNullOrEmpty(objQuality.Extra))
                                        objWriter.WriteAttributeString("select", objQuality.Extra);
                                    objWriter.WriteValue(objQuality.Name);
                                    objWriter.WriteEndElement();
                                }
                            }

                            // </positive>
                            objWriter.WriteEndElement();
                        }

                        // Negative Qualities.
                        if (blnPositive)
                        {
                            // <negative>
                            objWriter.WriteStartElement("negative");
                            foreach (Quality objQuality in _objCharacter.Qualities)
                            {
                                if (objQuality.Type == QualityType.Negative)
                                {
                                    objWriter.WriteStartElement("quality");
                                    if (!string.IsNullOrEmpty(objQuality.Extra))
                                        objWriter.WriteAttributeString("select", objQuality.Extra);
                                    objWriter.WriteValue(objQuality.Name);
                                    objWriter.WriteEndElement();
                                }
                            }

                            // </negative>
                            objWriter.WriteEndElement();
                        }

                        // </qualities>
                        objWriter.WriteEndElement();
                    }

                    // Export Starting Nuyen.
                    if (chkStartingNuyen.Checked)
                    {
                        decimal decNuyenBP = _objCharacter.NuyenBP;
                        if (!_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                            decNuyenBP /= 2.0m;
                        objWriter.WriteElementString("nuyenbp", decNuyenBP.ToString(GlobalSettings.InvariantCultureInfo));
                    }

                    /* TODO: Add support for active and knowledge skills and skill groups
                    // Export Active Skills.
                    if (chkActiveSkills.Checked)
                    {
                        // <skills>
                        objWriter.WriteStartElement("skills");

                        // Active Skills.
                        foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (!objSkill.IsKnowledgeSkill && objSkill.Rating > 0)
                            {
                                // <skill>
                                objWriter.WriteStartElement("skill");
                                objWriter.WriteElementString("name", objSkill.Name);
                                objWriter.WriteElementString("rating", objSkill.Rating.ToString());
                                if (!string.IsNullOrEmpty(objSkill.Specialization))
                                    objWriter.WriteElementString("spec", objSkill.Specialization);
                                // </skill>
                                objWriter.WriteEndElement();
                            }
                        }

                        // Skill Groups.
                        foreach (SkillGroup objSkillGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objSkillGroup.BaseUnbroken && objSkillGroup.Rating > 0)
                            {
                                // <skillgroup>
                                objWriter.WriteStartElement("skillgroup");
                                objWriter.WriteElementString("name", objSkillGroup.Name);
                                objWriter.WriteElementString("rating", objSkillGroup.Rating.ToString());
                                // </skillgroup>
                                objWriter.WriteEndElement();
                            }
                        }
                        // </skills>
                        objWriter.WriteEndElement();
                    }

                    // Export Knowledge Skills.
                    if (chkKnowledgeSkills.Checked)
                    {
                        // <knowledgeskills>
                        objWriter.WriteStartElement("knowledgeskills");
                        foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.Skills.OfType<KnowledgeSkill>())
                        {
                            // <skill>
                            objWriter.WriteStartElement("skill");
                            objWriter.WriteElementString("name", objSkill.Name);
                            objWriter.WriteElementString("rating", objSkill.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            if (!string.IsNullOrEmpty(objSkill.Specialization))
                                objWriter.WriteElementString("spec", objSkill.Specialization);
                            objWriter.WriteElementString("category", objSkill.SkillCategory);
                            // </skill>
                            objWriter.WriteEndElement();
                        }

                        // </knowledgeskills>
                        objWriter.WriteEndElement();
                    }
                    */

                    // Export Martial Arts.
                    if (chkMartialArts.Checked)
                    {
                        // <martialarts>
                        objWriter.WriteStartElement("martialarts");
                        foreach (MartialArt objArt in _objCharacter.MartialArts)
                        {
                            // <martialart>
                            objWriter.WriteStartElement("martialart");
                            objWriter.WriteElementString("name", objArt.Name);
                            if (objArt.Techniques.Count > 0)
                            {
                                // <techniques>
                                objWriter.WriteStartElement("techniques");
                                foreach (MartialArtTechnique objTechnique in objArt.Techniques)
                                    objWriter.WriteElementString("technique", objTechnique.Name);
                                // </techniques>
                                objWriter.WriteEndElement();
                            }

                            // </martialart>
                            objWriter.WriteEndElement();
                        }
                        // </martialarts>
                        objWriter.WriteEndElement();
                    }

                    // Export Spells.
                    if (chkSpells.Checked)
                    {
                        // <spells>
                        objWriter.WriteStartElement("spells");
                        foreach (Spell objSpell in _objCharacter.Spells)
                        {
                            objWriter.WriteStartElement("spell");
                            objWriter.WriteStartElement("name");
                            if (!string.IsNullOrEmpty(objSpell.Extra))
                                objWriter.WriteAttributeString("select", objSpell.Extra);
                            objWriter.WriteValue(objSpell.Name);
                            objWriter.WriteEndElement();
                            objWriter.WriteElementString("category", objSpell.Category);
                            objWriter.WriteEndElement();
                        }

                        // </spells>
                        objWriter.WriteEndElement();
                    }

                    // Export Complex Forms.
                    if (chkComplexForms.Checked)
                    {
                        // <programs>
                        objWriter.WriteStartElement("complexforms");
                        foreach (ComplexForm objComplexForm in _objCharacter.ComplexForms)
                        {
                            // <program>
                            objWriter.WriteStartElement("complexform");
                            objWriter.WriteStartElement("name");
                            if (!string.IsNullOrEmpty(objComplexForm.Extra))
                                objWriter.WriteAttributeString("select", objComplexForm.Extra);
                            objWriter.WriteValue(objComplexForm.Name);
                            objWriter.WriteEndElement();
                            objWriter.WriteEndElement();
                            // </program>
                            objWriter.WriteEndElement();
                        }

                        // </programs>
                        objWriter.WriteEndElement();
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
                            objWriter.WriteStartElement("cyberwares");
                            foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                            {
                                if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                                {
                                    // <cyberware>
                                    objWriter.WriteStartElement("cyberware");
                                    objWriter.WriteElementString("name", objCyberware.Name);
                                    if (objCyberware.Rating > 0)
                                        objWriter.WriteElementString("rating", objCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    objWriter.WriteElementString("grade", objCyberware.Grade.Name);
                                    if (objCyberware.Children.Count > 0)
                                    {
                                        // <cyberwares>
                                        objWriter.WriteStartElement("cyberwares");
                                        foreach (Cyberware objChildCyberware in objCyberware.Children)
                                        {
                                            if (objChildCyberware.Capacity != "[*]")
                                            {
                                                // <cyberware>
                                                objWriter.WriteStartElement("cyberware");
                                                objWriter.WriteElementString("name", objChildCyberware.Name);
                                                if (objChildCyberware.Rating > 0)
                                                    objWriter.WriteElementString("rating", objChildCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));

                                                if (objChildCyberware.GearChildren.Count > 0)
                                                    WriteGear(objWriter, objChildCyberware.GearChildren);
                                                // </cyberware>
                                                objWriter.WriteEndElement();
                                            }
                                        }

                                        // </cyberwares>
                                        objWriter.WriteEndElement();
                                    }

                                    if (objCyberware.GearChildren.Count > 0)
                                        WriteGear(objWriter, objCyberware.GearChildren);

                                    // </cyberware>
                                    objWriter.WriteEndElement();
                                }
                            }

                            // </cyberwares>
                            objWriter.WriteEndElement();
                        }

                        if (blnBioware)
                        {
                            // <biowares>
                            objWriter.WriteStartElement("biowares");
                            foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                            {
                                if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                                {
                                    // <bioware>
                                    objWriter.WriteStartElement("bioware");
                                    objWriter.WriteElementString("name", objCyberware.Name);
                                    if (objCyberware.Rating > 0)
                                        objWriter.WriteElementString("rating", objCyberware.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    objWriter.WriteElementString("grade", objCyberware.Grade.ToString());

                                    if (objCyberware.GearChildren.Count > 0)
                                        WriteGear(objWriter, objCyberware.GearChildren);
                                    // </bioware>
                                    objWriter.WriteEndElement();
                                }
                            }

                            // </biowares>
                            objWriter.WriteEndElement();
                        }
                    }

                    // Export Lifestyle.
                    if (chkLifestyle.Checked)
                    {
                        // <lifestyles>
                        objWriter.WriteStartElement("lifestyles");
                        foreach (Lifestyle objLifestyle in _objCharacter.Lifestyles)
                        {
                            // <lifestyle>
                            objWriter.WriteStartElement("lifestyle");
                            objWriter.WriteElementString("name", objLifestyle.Name);
                            objWriter.WriteElementString("months", objLifestyle.Increments.ToString(GlobalSettings.InvariantCultureInfo));
                            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
                            {
                                // This is an Advanced Lifestyle, so write out its properties.
                                objWriter.WriteElementString("cost", objLifestyle.Cost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo));
                                objWriter.WriteElementString("dice", objLifestyle.Dice.ToString(GlobalSettings.InvariantCultureInfo));
                                objWriter.WriteElementString("multiplier", objLifestyle.Multiplier.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo));
                                objWriter.WriteElementString("baselifestyle", objLifestyle.BaseLifestyle);
                                if (objLifestyle.LifestyleQualities.Count > 0)
                                {
                                    // <qualities>
                                    objWriter.WriteStartElement("qualities");
                                    foreach (LifestyleQuality objQuality in objLifestyle.LifestyleQualities)
                                        objWriter.WriteElementString("quality", objQuality.Name);
                                    // </qualities>
                                    objWriter.WriteEndElement();
                                }
                            }

                            // </lifestyle>
                            objWriter.WriteEndElement();
                        }

                        // </lifestyles>
                        objWriter.WriteEndElement();
                    }

                    // Export Armor.
                    if (chkArmor.Checked)
                    {
                        // <armors>
                        objWriter.WriteStartElement("armors");
                        foreach (Armor objArmor in _objCharacter.Armor)
                        {
                            // <armor>
                            objWriter.WriteStartElement("armor");
                            objWriter.WriteElementString("name", objArmor.Name);
                            if (objArmor.ArmorMods.Count > 0)
                            {
                                // <mods>
                                objWriter.WriteStartElement("mods");
                                foreach (ArmorMod objMod in objArmor.ArmorMods)
                                {
                                    // <mod>
                                    objWriter.WriteStartElement("mod");
                                    objWriter.WriteElementString("name", objMod.Name);
                                    if (objMod.Rating > 0)
                                        objWriter.WriteElementString("rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    // </mod>
                                    objWriter.WriteEndElement();
                                }

                                // </mods>
                                objWriter.WriteEndElement();
                            }

                            if (objArmor.GearChildren.Count > 0)
                                WriteGear(objWriter, objArmor.GearChildren);

                            // </armor>
                            objWriter.WriteEndElement();
                        }

                        // </armors>
                        objWriter.WriteEndElement();
                    }

                    // Export Weapons.
                    if (chkWeapons.Checked)
                    {
                        // <weapons>
                        objWriter.WriteStartElement("weapons");
                        foreach (Weapon objWeapon in _objCharacter.Weapons)
                        {
                            // Don't attempt to export Cyberware and Gear Weapons since those are handled by those object types. The default Unarmed Attack Weapon should also not be exported.
                            if (objWeapon.Category != "Cyberware" && objWeapon.Category != "Gear" && objWeapon.Name != "Unarmed Attack")
                            {
                                // <weapon>
                                objWriter.WriteStartElement("weapon");
                                objWriter.WriteElementString("name", objWeapon.Name);

                                // Weapon Accessories.
                                if (objWeapon.WeaponAccessories.Count > 0)
                                {
                                    // <accessories>
                                    objWriter.WriteStartElement("accessories");
                                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                    {
                                        // Don't attempt to export items included in the Weapon.
                                        if (!objAccessory.IncludedInWeapon)
                                        {
                                            // <accessory>
                                            objWriter.WriteStartElement("accessory");
                                            objWriter.WriteElementString("name", objAccessory.Name);
                                            objWriter.WriteElementString("mount", objAccessory.Mount);
                                            objWriter.WriteElementString("extramount", objAccessory.ExtraMount);

                                            if (objAccessory.GearChildren.Count > 0)
                                                WriteGear(objWriter, objAccessory.GearChildren);

                                            // </accessory>
                                            objWriter.WriteEndElement();
                                        }
                                    }

                                    // </accessories>
                                    objWriter.WriteEndElement();
                                }

                                // Underbarrel Weapon.
                                if (objWeapon.UnderbarrelWeapons.Count > 0)
                                {
                                    foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                                    {
                                        if (!objUnderbarrelWeapon.IncludedInWeapon)
                                            objWriter.WriteElementString("underbarrel", objUnderbarrelWeapon.Name);
                                    }
                                }

                                // </weapon>
                                objWriter.WriteEndElement();
                            }
                        }

                        // </weapons>
                        objWriter.WriteEndElement();
                    }

                    // Export Gear.
                    if (chkGear.Checked)
                    {
                        WriteGear(objWriter, _objCharacter.Gear);
                    }

                    // Export Vehicles.
                    if (chkVehicles.Checked)
                    {
                        // <vehicles>
                        objWriter.WriteStartElement("vehicles");
                        foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                        {
                            bool blnWeapons = false;
                            // <vehicle>
                            objWriter.WriteStartElement("vehicle");
                            objWriter.WriteElementString("name", objVehicle.Name);
                            if (objVehicle.Mods.Count > 0)
                            {
                                // <mods>
                                objWriter.WriteStartElement("mods");
                                foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                                {
                                    // Only write out the Mods that are not part of the base vehicle.
                                    if (!objVehicleMod.IncludedInVehicle)
                                    {
                                        // <mod>
                                        objWriter.WriteStartElement("mod");
                                        objWriter.WriteElementString("name", objVehicleMod.Name);
                                        if (objVehicleMod.Rating > 0)
                                            objWriter.WriteElementString("rating", objVehicleMod.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                        // </mod>
                                        objWriter.WriteEndElement();

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
                                objWriter.WriteEndElement();
                            }

                            // If there are Weapons, add them.
                            if (blnWeapons)
                            {
                                // <weapons>
                                objWriter.WriteStartElement("weapons");
                                foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                                {
                                    foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                    {
                                        // <weapon>
                                        objWriter.WriteStartElement("weapon");
                                        objWriter.WriteElementString("name", objWeapon.Name);

                                        // Weapon Accessories.
                                        if (objWeapon.WeaponAccessories.Count > 0)
                                        {
                                            // <accessories>
                                            objWriter.WriteStartElement("accessories");
                                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                            {
                                                // Don't attempt to export items included in the Weapon.
                                                if (!objAccessory.IncludedInWeapon)
                                                {
                                                    // <accessory>
                                                    objWriter.WriteStartElement("accessory");
                                                    objWriter.WriteElementString("name", objAccessory.Name);
                                                    objWriter.WriteElementString("mount", objAccessory.Mount);
                                                    objWriter.WriteElementString("extramount", objAccessory.ExtraMount);
                                                    // </accessory>
                                                    objWriter.WriteEndElement();
                                                }
                                            }

                                            // </accessories>
                                            objWriter.WriteEndElement();
                                        }

                                        // Underbarrel Weapon.
                                        if (objWeapon.UnderbarrelWeapons.Count > 0)
                                        {
                                            foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                                                objWriter.WriteElementString("underbarrel", objUnderbarrelWeapon.Name);
                                        }

                                        // </weapon>
                                        objWriter.WriteEndElement();
                                    }
                                }

                                // </weapons>
                                objWriter.WriteEndElement();
                            }

                            // Gear.
                            if (objVehicle.GearChildren.Count > 0)
                            {
                                WriteGear(objWriter, objVehicle.GearChildren);
                            }

                            // </vehicle>
                            objWriter.WriteEndElement();
                        }

                        // </vehicles>
                        objWriter.WriteEndElement();
                    }

                    // </pack>
                    objWriter.WriteEndElement();
                    // </packs>
                    objWriter.WriteEndElement();
                    // </chummer>
                    objWriter.WriteEndElement();

                    objWriter.WriteEndDocument();
                }
            }

            Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_SuiteCreated"), txtName.Text),
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
        private static void WriteGear(XmlWriter objWriter, IEnumerable<Gear> lstGear)
        {
            // <gears>
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in lstGear)
            {
                if (objGear.IncludedInParent)
                    continue;
                // <gear>
                objWriter.WriteStartElement("gear");
                objWriter.WriteStartElement("name");
                if (!string.IsNullOrEmpty(objGear.Extra))
                    objWriter.WriteAttributeString("select", objGear.Extra);
                objWriter.WriteValue(objGear.Name);
                objWriter.WriteEndElement();
                objWriter.WriteElementString("category", objGear.Category);
                if (objGear.Rating > 0)
                    objWriter.WriteElementString("rating", objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                if (objGear.Quantity != 1)
                    objWriter.WriteElementString("qty", objGear.Quantity.ToString(GlobalSettings.InvariantCultureInfo));
                if (objGear.Children.Count > 0)
                    WriteGear(objWriter, objGear.Children);
                // </gear>
                objWriter.WriteEndElement();
            }
            // </gears>
            objWriter.WriteEndElement();
        }

        #endregion Methods
    }
}
