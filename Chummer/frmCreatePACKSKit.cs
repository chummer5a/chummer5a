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
 using Chummer.Backend.Equipment;
 using Chummer.Skills;

namespace Chummer
{
    public partial class frmCreatePACKSKit : Form
    {
        private Character _objCharacter;

        #region Control Events
        public frmCreatePACKSKit(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the kit and file name fields are populated.
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_CreatePACKSKit_KitName"), LanguageManager.Instance.GetString("MessageTitle_CreatePACKSKit_KitName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_CreatePACKSKit_FileName"), LanguageManager.Instance.GetString("MessageTitle_CreatePACKSKit_FileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Make sure the file name starts with custom and ends with _packs.xml.
            if (!txtFileName.Text.StartsWith("custom") || !txtFileName.Text.EndsWith("_packs.xml"))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_CreatePACKSKit_InvalidFileName"), LanguageManager.Instance.GetString("MessageTitle_CreatePACKSKit_InvalidFileName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // See if a Kit with this name already exists for the Custom category. This is done without the XmlManager since we need to check each file individually.
            XmlDocument objXmlDocument = new XmlDocument();
            string strCustomPath = Path.Combine(Application.StartupPath, "data");
            foreach (string strFile in Directory.GetFiles(strCustomPath, "custom*_packs.xml"))
            {
                objXmlDocument.Load(strFile);
                XmlNodeList objXmlPACKSList = objXmlDocument.SelectNodes("/chummer/packs/pack[name = \"" + txtName.Text + "\" and category = \"Custom\"]");
                if (objXmlPACKSList.Count > 0)
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("Message_CreatePACKSKit_DuplicateName").Replace("{0}", txtName.Text).Replace("{1}", strFile.Replace(strCustomPath + Path.DirectorySeparatorChar, string.Empty)), LanguageManager.Instance.GetString("MessageTitle_CreatePACKSKit_DuplicateName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            string strPath = Path.Combine(strCustomPath, txtFileName.Text);
            bool blnNewFile = !File.Exists(strPath);

            // If this is not a new file, read in the existing contents.
            XmlDocument objXmlCurrentDocument = new XmlDocument();
            if (!blnNewFile)
                objXmlCurrentDocument.Load(strPath);

            FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.Unicode);
            objWriter.Formatting = Formatting.Indented;
            objWriter.Indentation = 1;
            objWriter.IndentChar = '\t';
            objWriter.WriteStartDocument();

            // <chummer>
            objWriter.WriteStartElement("chummer");
            // <packs>
            objWriter.WriteStartElement("packs");

            // If this is not a new file, write out the current contents.
            if (!blnNewFile)
            {
                XmlNodeList objXmlNodeList = objXmlCurrentDocument.SelectNodes("/chummer/*");
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
                int intDEP = _objCharacter.DEP.Value - (_objCharacter.DEP.MetatypeMinimum - 1);
                int intRES = _objCharacter.RES.Value - (_objCharacter.RES.MetatypeMinimum - 1);
                // <attributes>
                objWriter.WriteStartElement("attributes");
                objWriter.WriteElementString("bod", intBOD.ToString());
                objWriter.WriteElementString("agi", intAGI.ToString());
                objWriter.WriteElementString("rea", intREA.ToString());
                objWriter.WriteElementString("str", intSTR.ToString());
                objWriter.WriteElementString("cha", intCHA.ToString());
                objWriter.WriteElementString("int", intINT.ToString());
                objWriter.WriteElementString("log", intLOG.ToString());
                objWriter.WriteElementString("wil", intWIL.ToString());
                objWriter.WriteElementString("edg", intEDG.ToString());
                if (_objCharacter.MAGEnabled)
                    objWriter.WriteElementString("mag", intMAG.ToString());
                if (_objCharacter.RESEnabled)
                    objWriter.WriteElementString("res", intRES.ToString());
                if (_objCharacter.DEPEnabled)
                    objWriter.WriteElementString("dep", intDEP.ToString());
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
                    if (objQuality.Type == QualityType.Positive)
                        blnPositive = true;
                    if (objQuality.Type == QualityType.Negative)
                        blnNegative = true;
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
                int intNuyenBP = Convert.ToInt32(_objCharacter.NuyenBP);
                if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma)
                    intNuyenBP = Convert.ToInt32(Convert.ToDouble(intNuyenBP, GlobalOptions.InvariantCultureInfo) / 2.0);
                objWriter.WriteElementString("nuyenbp", intNuyenBP.ToString());
            }

            // Export Active Skills.
            if (chkActiveSkills.Checked)
            {
                // <skills>
                objWriter.WriteStartElement("skills");

                //TODO: Figure out what this did?
                // Active Skills.
                //foreach (Skill objSkill in _objCharacter.Skills)
                //{
                //    if (!objSkill.KnowledgeSkill && !objSkill.IsGrouped && objSkill.Rating > 0)
                //    {
                //        // <skill>
                //        objWriter.WriteStartElement("skill");
                //        objWriter.WriteElementString("name", objSkill.Name);
                //        objWriter.WriteElementString("rating", objSkill.Rating.ToString());
                //        if (!string.IsNullOrEmpty(objSkill.Specialization))
                //            objWriter.WriteElementString("spec", objSkill.Specialization);
                //        // </skill>
                //        objWriter.WriteEndElement();
                //    }
                //}  

                // Skill Groups.
                //foreach (SkillGroup objSkillGroup in _objCharacter.SkillGroups)
                //{
                //    if (!objSkillGroup.Broken && objSkillGroup.Rating > 0)
                //    {
                //        // <skillgroup>
                //        objWriter.WriteStartElement("skillgroup");
                //        objWriter.WriteElementString("name", objSkillGroup.Name);
                //        objWriter.WriteElementString("rating", objSkillGroup.Rating.ToString());
                //        // </skillgroup>
                //        objWriter.WriteEndElement();
                //    }
                //}
                // </skills>
                objWriter.WriteEndElement();
            }

            // Export Knowledge Skills.
            if (chkKnowledgeSkills.Checked)
            {
                // <knowledgeskills>
                objWriter.WriteStartElement("knowledgeskills");
                // Active Skills.
                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (objSkill.IsKnowledgeSkill)
                    {
                        // <skill>
                        objWriter.WriteStartElement("skill");
                        objWriter.WriteElementString("name", objSkill.Name);
                        objWriter.WriteElementString("rating", objSkill.Rating.ToString());
                        if (!string.IsNullOrEmpty(objSkill.Specialization))
                            objWriter.WriteElementString("spec", objSkill.Specialization);
                        objWriter.WriteElementString("category", objSkill.SkillCategory);
                        // </skill>
                        objWriter.WriteEndElement();
                    }
                }
                // </knowledgeskills>
                objWriter.WriteEndElement();
            }

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
                    objWriter.WriteElementString("rating", objArt.Rating.ToString());
                    if (objArt.Advantages.Count > 0)
                    {
                        // <advantages>
                        objWriter.WriteStartElement("advantages");
                        foreach (MartialArtAdvantage objAdvantage in objArt.Advantages)
                            objWriter.WriteElementString("advantage", objAdvantage.Name);
                        // </advantages>
                        objWriter.WriteEndElement();
                    }
                    // </martialart>
                    objWriter.WriteEndElement();
                }
                foreach (MartialArtManeuver objManeuver in _objCharacter.MartialArtManeuvers)
                    objWriter.WriteElementString("maneuver", objManeuver.Name);
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
                    if (!string.IsNullOrEmpty(objSpell.Extra))
                        objWriter.WriteAttributeString("select", objSpell.Extra);
                    objWriter.WriteValue(objSpell.Name);
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
                foreach (ComplexForm objProgram in _objCharacter.ComplexForms)
                {
                    // <program>
                    objWriter.WriteStartElement("complexform");
                    objWriter.WriteStartElement("name");
                    objWriter.WriteValue(objProgram.Name);
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
                    if (objCharacterCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                        blnBioware = true;
                    if (objCharacterCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                        blnCyberware = true;
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
                                objWriter.WriteElementString("rating", objCyberware.Rating.ToString());
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
                                            objWriter.WriteElementString("rating", objChildCyberware.Rating.ToString());

                                        if (objChildCyberware.Gear.Count > 0)
                                            WriteGear(objWriter, objChildCyberware.Gear);
                                        // </cyberware>
                                        objWriter.WriteEndElement();
                                    }
                                }
                                // </cyberwares>
                                objWriter.WriteEndElement();
                            }

                            if (objCyberware.Gear.Count > 0)
                                WriteGear(objWriter, objCyberware.Gear);

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
                                objWriter.WriteElementString("rating", objCyberware.Rating.ToString());
                            objWriter.WriteElementString("grade", objCyberware.Grade.ToString());

                            if (objCyberware.Gear.Count > 0)
                                WriteGear(objWriter, objCyberware.Gear);
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
                    objWriter.WriteElementString("months", objLifestyle.Months.ToString());
                    if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
                    {
                        // This is an Advanced Lifestyle, so write out its properties.
                        objWriter.WriteElementString("cost", objLifestyle.Cost.ToString());
                        objWriter.WriteElementString("dice", objLifestyle.Dice.ToString());
                        objWriter.WriteElementString("multiplier", objLifestyle.Multiplier.ToString());
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
                                objWriter.WriteElementString("rating", objMod.Rating.ToString());
                            // </mod>
                            objWriter.WriteEndElement();
                        }
                        // </mods>
                        objWriter.WriteEndElement();
                    }

                    if (objArmor.Gear.Count > 0)
                        WriteGear(objWriter, objArmor.Gear);

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

                                    if (objAccessory.Gear.Count > 0)
                                        WriteGear(objWriter, objAccessory.Gear);

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
                                    objWriter.WriteElementString("rating", objVehicleMod.Rating.ToString());
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
                    if (objVehicle.Gear.Count > 0)
                    {
                        WriteGear(objWriter, objVehicle.Gear);
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
            objWriter.Close();
            objStream.Close();

            MessageBox.Show(LanguageManager.Instance.GetString("Message_CreatePACKSKit_SuiteCreated").Replace("{0}", txtName.Text), LanguageManager.Instance.GetString("MessageTitle_CreatePACKSKit_SuiteCreated"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Recursively write out all Gear information since these can be nested pretty deep.
        /// </summary>
        /// <param name="objWriter">XmlWriter to use.</param>
        /// <param name="lstGear">List of Gear to write.</param>
        private void WriteGear(XmlWriter objWriter, List<Gear> lstGear)
        {
            // <gears>
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in lstGear)
            {
                // Do not attempt to export Nexi since they're completely custom objects.
                if (!objGear.Name.StartsWith("Nexus") && !objGear.IncludedInParent)
                {
                    // <gear>
                    objWriter.WriteStartElement("gear");
                    objWriter.WriteStartElement("name");
                    if (!string.IsNullOrEmpty(objGear.Extra))
                        objWriter.WriteAttributeString("select", objGear.Extra);
                    objWriter.WriteValue(objGear.Name);
                    objWriter.WriteEndElement();
                    objWriter.WriteElementString("category", objGear.Category);
                    if (objGear.Rating > 0)
                        objWriter.WriteElementString("rating", objGear.Rating.ToString());
                    if (objGear.Quantity > 1)
                        objWriter.WriteElementString("qty", objGear.Quantity.ToString());
                    if (objGear.Children.Count > 0)
                        WriteGear(objWriter, objGear.Children);
                    // </gear>
                    objWriter.WriteEndElement();
                }
            }
            // </gears>
            objWriter.WriteEndElement();
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblNameLabel.Width, lblFileNameLabel.Width);
            txtName.Left = lblNameLabel.Left + intWidth + 6;
            txtName.Width = Width - txtName.Left - 19;
            txtFileName.Left = lblFileNameLabel.Left + intWidth + 6;
            txtFileName.Width = Width - txtFileName.Left - 19;
        }
        #endregion
    }
}