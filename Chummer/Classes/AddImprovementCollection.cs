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
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
// ReSharper disable InconsistentNaming

namespace Chummer.Classes
{
    public class AddImprovementCollection
    {
        private readonly Character _objCharacter;

        public AddImprovementCollection(Character character, Improvement.ImprovementSource objImprovementSource, string sourceName, string strUnique, string forcedValue, string limitSelection, string selectedValue, bool blnConcatSelectedValue, string strFriendlyName, int intRating)
        {
            _objCharacter = character;
            _objImprovementSource = objImprovementSource;
            SourceName = sourceName;
            _strUnique = strUnique;
            ForcedValue = forcedValue;
            LimitSelection = limitSelection;
            SelectedValue = selectedValue;
            _blnConcatSelectedValue = blnConcatSelectedValue;
            _strFriendlyName = strFriendlyName;
            _intRating = intRating;
        }

        public string SourceName { get; set; }
        public string ForcedValue { get; set; }
        public string LimitSelection { get; set; }
        public string SelectedValue { get; set; }
        public string SelectedTarget { get; set; } = string.Empty;

        private readonly Improvement.ImprovementSource _objImprovementSource;
        private readonly string _strUnique;
        private readonly bool _blnConcatSelectedValue;
        private readonly string _strFriendlyName;
        private readonly int _intRating;
        
        private void CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
            string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
            int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0,
            int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false, string strTarget = "", string strCondition = "")
        {
            ImprovementManager.CreateImprovement(_objCharacter, strImprovedName, objImprovementSource,
                strSourceName, objImprovementType, strUnique,
                intValue, intRating, intMinimum, intMaximum, intAugmented,
                intAugmentedMaximum, strExclude, blnAddToRating, strTarget, strCondition);
        }

        #region
        public void qualitylevel(XmlNode bonusNode)
        {
            /*
            //List of qualities to work with
            Guid[] all =
            {
                new Guid("9ac85feb-ae1e-4996-8514-3570d411e1d5"), //national
                new Guid("d9479e5c-d44a-45b9-8fb4-d1e08a9487b2"), //dirty criminal
                new Guid("318d2edd-833b-48c5-a3e1-343bf03848a5"), //Limited
                new Guid("e00623e1-54b0-4a91-b234-3c7e141deef4") //Corp
            };
            */

            //Add to list
            //retrive list
            //sort list
            //find active instance
            //if active = list[top]
            //    return
            //else
            //    remove active
            //  add list[top]
            //    set list[top] active
        }

        // Dummy Method for SelectText
        public void selecttext(XmlNode bonusNode)
        {
            Log.Info("selecttext: " + SelectedValue);
        }

        public void spellresistance(XmlNode bonusNode)
        {
            Log.Info("spellresistance");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpellResistance, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void mentalmanipulationresist(XmlNode bonusNode)
        {
            Log.Info("mentalmanipulationresist");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MentalManipulationResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void physicalmanipulationresist(XmlNode bonusNode)
        {
            Log.Info("physicalmanipulationresist");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalManipulationResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void manaillusionresist(XmlNode bonusNode)
        {
            Log.Info("manaillusionresist");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ManaIllusionResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void physicalillusionresist(XmlNode bonusNode)
        {
            Log.Info("physicalillusionresist");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalIllusionResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void detectionspellresist(XmlNode bonusNode)
        {
            Log.Info("detectionspellresist");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DetectionSpellResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void enableattribute(XmlNode bonusNode)
        {
            Log.Info("enableattribute");
            switch (bonusNode["name"]?.InnerText)
            {
                case "MAG":
                    Log.Info("Calling CreateImprovement for MAG");
                    CreateImprovement("MAG", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0);
                    _objCharacter.MAGEnabled = true;
                    break;
                case "RES":
                    Log.Info("Calling CreateImprovement for RES");
                    CreateImprovement("RES", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0);
                    _objCharacter.RESEnabled = true;
                    break;
                case "DEP":
                    Log.Info("Calling CreateImprovement for DEP");
                    CreateImprovement("DEP", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0);
                    _objCharacter.DEPEnabled = true;
                    break;
            }
        }

        // Add an Attribute Replacement.
        public void replaceattributes(XmlNode bonusNode)
        {
            using (XmlNodeList objXmlAttributes = bonusNode.SelectNodes("replaceattribute"))
                if (objXmlAttributes != null)
                    foreach (XmlNode objXmlAttribute in objXmlAttributes)
                    {
                        Log.Info("replaceattribute");
                        Log.Info("replaceattribute = " + bonusNode.OuterXml);
                        // Record the improvement.
                        int intMin = 0;
                        int intMax = 0;

                        // Extract the modifiers.
                        string strTemp = objXmlAttribute["min"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, out intMin);
                        strTemp = objXmlAttribute["max"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, out intMax);
                        string strAttribute = objXmlAttribute["name"]?.InnerText;

                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.ReplaceAttribute,
                            _strUnique,
                            0, 1, intMin, intMax);
                    }
        }

        // Enable a special tab.
        public void enabletab(XmlNode bonusNode)
        {
            Log.Info("enabletab");
            foreach (XmlNode objXmlEnable in bonusNode.ChildNodes)
            {
                switch (objXmlEnable.InnerText)
                {
                    case "magician":
                        _objCharacter.MagicianEnabled = true;
                        Log.Info("magician");
                        CreateImprovement("Magician", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "enabletab", 0, 0);
                        break;
                    case "adept":
                        _objCharacter.AdeptEnabled = true;
                        Log.Info("adept");
                        CreateImprovement("Adept", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "enabletab",
                            0, 0);
                        break;
                    case "technomancer":
                        _objCharacter.TechnomancerEnabled = true;
                        Log.Info("technomancer");
                        CreateImprovement("Technomancer", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "enabletab", 0, 0);
                        break;
                    case "advanced programs":
                        _objCharacter.AdvancedProgramsEnabled = true;
                        Log.Info("advanced programs");
                        CreateImprovement("Advanced Programs", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "enabletab", 0, 0);
                        break;
                    case "critter":
                        _objCharacter.CritterEnabled = true;
                        Log.Info("critter");
                        CreateImprovement("Critter", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "enabletab", 0, 0);
                        break;
                    case "initiation":
                        _objCharacter.InitiationEnabled = true;
                        Log.Info("initiation");
                        CreateImprovement("Initiation", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "enabletab", 0, 0);
                        break;
                }
            }
        }

        // Disable a  tab.
        public void disabletab(XmlNode bonusNode)
        {
            Log.Info("disabletab");
            foreach (XmlNode objXmlEnable in bonusNode.ChildNodes)
            {
                switch (objXmlEnable.InnerText)
                {
                    case "cyberware":
                        _objCharacter.CyberwareDisabled = true;
                        Log.Info("cyberware");
                        CreateImprovement("Cyberware", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                            "disabletab", 0, 0);
                        break;
                }
            }
        }

        // Select Restricted (select Restricted items for Fake Licenses).
        public void selectrestricted(XmlNode bonusNode)
        {
            Log.Info("selectrestricted");
            frmSelectItem frmPickItem = new frmSelectItem
            {
                Character = _objCharacter
            };
            if (!string.IsNullOrEmpty(ForcedValue))
                frmPickItem.ForceItem = ForcedValue;
            frmPickItem.AllowAutoSelect = false;
            frmPickItem.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedName;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Restricted, _strUnique);
        }
        
        public void selecttradition(XmlNode bonusNode)
        {
            Log.Info("selecttradition");

            // Populate the Magician Traditions list.
            XPathNavigator xmlTraditionsBaseChummerNode = XmlManager.Load("traditions.xml").GetFastNavigator().SelectSingleNode("/chummer");
            List<ListItem> lstTraditions = new List<ListItem>();
            if (xmlTraditionsBaseChummerNode != null)
            {
                foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select("traditions/tradition[" + _objCharacter.Options.BookXPath() + "]"))
                {
                    string strName = xmlTradition.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstTraditions.Add(new ListItem(strName, xmlTradition.SelectSingleNode("translate")?.Value ?? strName));
                }
            }

            if (lstTraditions.Count > 1)
            {
                lstTraditions.Sort(CompareListItems.CompareNames);
            }

            frmSelectItem frmPickItem = new frmSelectItem
            {
                DropdownItems = lstTraditions,
                SelectedItem = _objCharacter.MagicTradition
            };
            if (!string.IsNullOrEmpty(ForcedValue))
                frmPickItem.ForceItem = ForcedValue;
            frmPickItem.AllowAutoSelect = false;
            frmPickItem.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedName;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName, Improvement.ImprovementType.Tradition, _strUnique);
        }

        public void cyberseeker(XmlNode bonusNode)
        {
            //Check if valid attrib
            string strBonusNodeText = bonusNode.InnerText;
            if (strBonusNodeText == "BOX" || AttributeSection.AttributeStrings.Any(x => x == strBonusNodeText))
            {
                CreateImprovement(strBonusNodeText, _objImprovementSource, SourceName, Improvement.ImprovementType.Seeker, _strUnique, 0, 0);
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        public void blockskillgroupdefaulting(XmlNode bonusNode)
        {
            Log.Info("blockskillgroupdefaulting");
            string strExclude = string.Empty;
            if (bonusNode.Attributes?["excludecategory"] != null)
                strExclude = bonusNode.Attributes["excludecategory"].InnerText;

            frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup
            {
                Description = !string.IsNullOrEmpty(_strFriendlyName)
                    ? LanguageManager.GetString("String_Improvement_SelectSkillGroupName", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                    : LanguageManager.GetString("String_Improvement_SelectSkillGroup", GlobalOptions.Language)
            };

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickSkillGroup.OnlyGroup = ForcedValue;
                frmPickSkillGroup.Opacity = 0;
            }

            if (!string.IsNullOrEmpty(strExclude))
                frmPickSkillGroup.ExcludeCategory = strExclude;

            frmPickSkillGroup.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSkillGroup.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickSkillGroup.SelectedSkillGroup;

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillDefault,
                _strUnique, 0, 0, 0, 1, 0, 0, strExclude);
        }

        // Select a Skill.
        public void selectskill(XmlNode bonusNode)
        {
            //TODO this don't work
            Log.Info("selectskill");
            if (ForcedValue == "+2 to a Combat Skill")
                ForcedValue = string.Empty;

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("_strForcedValue = " + ForcedValue);

            bool blnIsKnowledgeSkill = false;
            string strSelectedSkill = ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName, ref blnIsKnowledgeSkill);
            
            bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;

            SelectedValue = strSelectedSkill;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);
            
            string strVal = bonusNode["val"]?.InnerText;
            string strMax = bonusNode["max"]?.InnerText;
            bool blnDisableSpec = bonusNode.InnerXml.Contains("disablespecializationeffects");
            // Find the selected Skill.
            if (blnIsKnowledgeSkill)
            {
                if (_objCharacter.SkillsSection.KnowledgeSkills.Any(k => k.Name == strSelectedSkill))
                {
                    foreach (KnowledgeSkill k in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (k.Name != strSelectedSkill)
                            continue;
                        // We've found the selected Skill.
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            Log.Info("Calling CreateImprovement");
                            CreateImprovement(k.Name, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating), 1, 0, 0, 0, 0, string.Empty,
                                blnAddToRating);
                        }

                        if (blnDisableSpec)
                        {
                            Log.Info("Calling CreateImprovement");
                            CreateImprovement(k.Name, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.DisableSpecializationEffects,
                                _strUnique);
                        }

                        if (!string.IsNullOrEmpty(strMax))
                        {
                            Log.Info("Calling CreateImprovement");
                            CreateImprovement(k.Name, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                                blnAddToRating);
                        }
                    }
                }
                else
                {
                    KnowledgeSkill k = new KnowledgeSkill(_objCharacter, strSelectedSkill);
                    _objCharacter.SkillsSection.KnowledgeSkills.Add(k);
                    // We've found the selected Skill.
                    if (!string.IsNullOrEmpty(strVal))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(k.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating), 1, 0, 0, 0, 0, string.Empty,
                            blnAddToRating);
                    }

                    if (blnDisableSpec)
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(k.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.DisableSpecializationEffects,
                            _strUnique);
                    }

                    if (!string.IsNullOrEmpty(strMax))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(k.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                            blnAddToRating);
                    }
                }
            }
            else if (strSelectedSkill.Contains("Exotic Melee Weapon") ||
                     strSelectedSkill.Contains("Exotic Ranged Weapon") ||
                     strSelectedSkill.Contains("Pilot Exotic Vehicle"))
            {
                foreach (Skill objLoopSkill in _objCharacter.SkillsSection.Skills.Where(s => s.IsExoticSkill))
                {
                    ExoticSkill objSkill = (ExoticSkill) objLoopSkill;
                    if ($"{objSkill.Name} ({objSkill.Specific})" != strSelectedSkill)
                        continue;
                    // We've found the selected Skill.
                    if (!string.IsNullOrEmpty(strVal))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement($"{objSkill.Name} ({objSkill.Specific})", _objImprovementSource,
                            SourceName,
                            Improvement.ImprovementType.Skill, _strUnique,
                            ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating), 1,
                            0, 0, 0, 0, string.Empty, blnAddToRating);
                    }

                    if (blnDisableSpec)
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement($"{objSkill.Name} ({objSkill.Specific})", _objImprovementSource,
                            SourceName,
                            Improvement.ImprovementType.DisableSpecializationEffects,
                            _strUnique);
                    }

                    if (!string.IsNullOrEmpty(strMax))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement($"{objSkill.Name} ({objSkill.Specific})", _objImprovementSource,
                            SourceName,
                            Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
                            ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                            blnAddToRating);
                    }
                }
            }
            else
            {
                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (objSkill.Name != strSelectedSkill)
                        continue;
                    // We've found the selected Skill.
                    if (!string.IsNullOrEmpty(strVal))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(objSkill.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating), 1, 0, 0, 0, 0,
                            string.Empty,
                            blnAddToRating);
                    }

                    if (blnDisableSpec)
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(objSkill.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.DisableSpecializationEffects,
                            _strUnique);
                    }

                    if (!string.IsNullOrEmpty(strMax))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(objSkill.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                            blnAddToRating);
                    }
                }
            }
        }

        // Select a Skill Group.
        public void selectskillgroup(XmlNode bonusNode)
        {
            Log.Info("selectskillgroup");
            string strExclude = string.Empty;
            if (bonusNode.Attributes?["excludecategory"] != null)
                strExclude = bonusNode.Attributes["excludecategory"].InnerText;

            frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup
            {
                Description = !string.IsNullOrEmpty(_strFriendlyName)
                    ? LanguageManager.GetString("String_Improvement_SelectSkillGroupName", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                    : LanguageManager.GetString("String_Improvement_SelectSkillGroup", GlobalOptions.Language)
            };

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickSkillGroup.OnlyGroup = ForcedValue;
                frmPickSkillGroup.Opacity = 0;
            }

            if (!string.IsNullOrEmpty(strExclude))
                frmPickSkillGroup.ExcludeCategory = strExclude;

            frmPickSkillGroup.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSkillGroup.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;

            SelectedValue = frmPickSkillGroup.SelectedSkillGroup;

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            string strBonus = bonusNode["bonus"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, ImprovementManager.ValueToInt(_objCharacter, strBonus, _intRating), 1, 0, 0, 0, 0, strExclude,
                    blnAddToRating);
            }
            else
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, 0, 0, 0, 1, 0, 0, strExclude,
                    blnAddToRating);
            }
        }

        public void selectattributes(XmlNode bonusNode)
        {
            using (XmlNodeList xmlSelectAttributeList = bonusNode.SelectNodes("selectattribute"))
                if (xmlSelectAttributeList != null)
                    foreach (XmlNode objXmlAttribute in xmlSelectAttributeList)
                    {
                        Log.Info("selectattribute");

                        Log.Info("selectattribute = " + bonusNode.OuterXml);

                        List<string> lstAbbrevs = new List<string>();
                        XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute");
                        if (xmlAttributeList?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList)
                                lstAbbrevs.Add(objSubNode.InnerText);
                        }
                        else
                        {
                            lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                            xmlAttributeList = bonusNode.SelectNodes("excludeattribute");
                            if (xmlAttributeList?.Count > 0)
                            {
                                foreach (XmlNode objSubNode in xmlAttributeList)
                                    lstAbbrevs.Remove(objSubNode.InnerText);
                            }
                        }

                        lstAbbrevs.Remove("ESS");
                        if (!_objCharacter.MAGEnabled)
                        {
                            lstAbbrevs.Remove("MAG");
                            lstAbbrevs.Remove("MAGAdept");
                        }
                        else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                            lstAbbrevs.Remove("MAGAdept");

                        if (!_objCharacter.RESEnabled)
                            lstAbbrevs.Remove("RES");
                        if (!_objCharacter.DEPEnabled)
                            lstAbbrevs.Remove("DEP");

                        Log.Info("_strForcedValue = " + ForcedValue);
                        Log.Info("_strLimitSelection = " + LimitSelection);

                        // Check to see if there is only one possible selection because of _strLimitSelection.
                        if (!string.IsNullOrEmpty(ForcedValue))
                            LimitSelection = ForcedValue;

                        if (!string.IsNullOrEmpty(LimitSelection))
                        {
                            lstAbbrevs.RemoveAll(x => x != LimitSelection);
                        }

                        // Display the Select Attribute window and record which Skill was selected.
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                        {
                            Description = !string.IsNullOrEmpty(_strFriendlyName)
                                ? LanguageManager.GetString("String_Improvement_SelectAttributeNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                                : LanguageManager.GetString("String_Improvement_SelectAttribute", GlobalOptions.Language)
                        };

                        frmPickAttribute.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = frmPickAttribute.SelectedAttribute;
                        if (_blnConcatSelectedValue)
                            SourceName += " (" + SelectedValue + ')';

                        Log.Info("_strSelectedValue = " + frmPickAttribute.SelectedAttribute);
                        Log.Info("SourceName = " + SourceName);

                        // Record the improvement.
                        int intMin = 0;
                        int intAug = 0;
                        int intMax = 0;
                        int intAugMax = 0;

                        // Extract the modifiers.
                        string strTemp = objXmlAttribute["min"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, out intMin);
                        strTemp = objXmlAttribute["val"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, out intAug);
                        strTemp = objXmlAttribute["max"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, out intMax);
                        strTemp = objXmlAttribute["aug"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, out intAugMax);

                        string strAttribute = frmPickAttribute.SelectedAttribute;

                        if (objXmlAttribute["affectbase"] != null)
                            strAttribute += "Base";

                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                            _strUnique,
                            0, 1, intMin, intMax, intAug, intAugMax);
                    }
        }

        // Select an CharacterAttribute.
        public void selectattribute(XmlNode bonusNode)
        {
            Log.Info("selectattribute");
            
            List<string> lstAbbrevs = new List<string>();
            XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute");
            if (xmlAttributeList?.Count > 0)
            {
                foreach (XmlNode objSubNode in xmlAttributeList)
                    lstAbbrevs.Add(objSubNode.InnerText);
            }
            else
            {
                lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                xmlAttributeList = bonusNode.SelectNodes("excludeattribute");
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Remove(objSubNode.InnerText);
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!_objCharacter.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!_objCharacter.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!_objCharacter.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                lstAbbrevs.RemoveAll(x => x != LimitSelection);
            }

            // Display the Select Attribute window and record which Skill was selected.
            frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
            {
                Description = !string.IsNullOrEmpty(_strFriendlyName)
                    ? LanguageManager.GetString("String_Improvement_SelectAttributeNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                    : LanguageManager.GetString("String_Improvement_SelectAttribute", GlobalOptions.Language)
            };

            Log.Info("selectattribute = " + bonusNode.OuterXml);

            frmPickAttribute.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickAttribute.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickAttribute.SelectedAttribute;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Record the improvement.
            int intMin = 0;
            int intAug = 0;
            int intMax = 0;
            int intAugMax = 0;

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intMin = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["val"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intAug = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["max"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["aug"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);

            string strAttribute = frmPickAttribute.SelectedAttribute;

            if (bonusNode["affectbase"] != null)
                strAttribute += "Base";

            Log.Info("Calling CreateImprovement");
            CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                _strUnique,
                0, 1, intMin, intMax, intAug, intAugMax);
        }

        // Select a Limit.
        public void selectlimit(XmlNode bonusNode)
        {
            Log.Info("selectlimit");
            
            Log.Info("selectlimit = " + bonusNode.OuterXml);
            
            List<string> strLimits = new List<string>();
            XmlNodeList xmlDefinedLimits = bonusNode.SelectNodes("limit");
            if (xmlDefinedLimits != null && xmlDefinedLimits.Count > 0)
            {
                foreach (XmlNode objXmlAttribute in xmlDefinedLimits)
                    strLimits.Add(objXmlAttribute.InnerText);
            }
            else
            {
                strLimits.Add("Physical");
                strLimits.Add("Mental");
                strLimits.Add("Social");
            }

            XmlNodeList xmlExcludeLimits = bonusNode.SelectNodes("excludelimit");
            if (xmlExcludeLimits != null && xmlExcludeLimits.Count > 0)
            {
                foreach (XmlNode objXmlAttribute in xmlExcludeLimits)
                {
                    strLimits.Remove(objXmlAttribute.InnerText);
                }
            }

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                strLimits.RemoveAll(x => x != LimitSelection);
            }

            // Display the Select Limit window and record which Limit was selected.
            frmSelectLimit frmPickLimit = new frmSelectLimit(strLimits.ToArray())
            {
                Description = !string.IsNullOrEmpty(_strFriendlyName)
                    ? LanguageManager.GetString("String_Improvement_SelectLimitNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                    : LanguageManager.GetString("String_Improvement_SelectLimit", GlobalOptions.Language)
            };

            frmPickLimit.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickLimit.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            
            // Record the improvement.
            int intMin = 0;
            int intAug = 0;
            int intMax = 0;
            int intAugMax = 0;

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intMin = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["val"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intAug = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["max"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["aug"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            
            string strLimit = frmPickLimit.SelectedLimit;
            
            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);
            
            // string strBonus = bonusNode["value"].InnerText;
            int intBonus = intAug;
            Improvement.ImprovementType eType;

            switch (strLimit)
            {
                case "Mental":
                    {
                        eType = Improvement.ImprovementType.MentalLimit;
                        break;
                    }
                case "Social":
                    {
                        eType = Improvement.ImprovementType.SocialLimit;
                        break;
                    }
                case "Physical":
                    {
                        eType = Improvement.ImprovementType.SocialLimit;
                        break;
                    }
                default:
                    throw new AbortedException();
            }

            SelectedValue = frmPickLimit.SelectedDisplayLimit;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            if (bonusNode["affectbase"] != null)
                strLimit += "Base";

            Log.Info("Calling CreateImprovement");
            CreateImprovement(strLimit, _objImprovementSource, SourceName, eType, _strUnique, intBonus, 0, intMin,
                intMax,
                intAug, intAugMax);
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public void swapskillattribute(XmlNode bonusNode)
        {
            Log.Info("swapskillattribute");
            
            List<string> lstAbbrevs = new List<string>();
            XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute");
            if (xmlAttributeList?.Count > 0)
            {
                foreach (XmlNode objSubNode in xmlAttributeList)
                    lstAbbrevs.Add(objSubNode.InnerText);
            }
            else
            {
                lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                xmlAttributeList = bonusNode.SelectNodes("excludeattribute");
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Remove(objSubNode.InnerText);
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!_objCharacter.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!_objCharacter.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!_objCharacter.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            if (lstAbbrevs.Count == 1)
                LimitSelection = lstAbbrevs[0];

            Log.Info("swapskillattribute = " + bonusNode.OuterXml);

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                SelectedValue = LimitSelection;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                {
                    Description = !string.IsNullOrEmpty(_strFriendlyName)
                        ? LanguageManager.GetString("String_Improvement_SelectAttributeNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                        : LanguageManager.GetString("String_Improvement_SelectAttribute", GlobalOptions.Language)
                };

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickAttribute.SelectedAttribute;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';
            }

            string strLimitToSkill = bonusNode.SelectSingleNode("limittoskill")?.InnerText;
            if (!string.IsNullOrEmpty(strLimitToSkill))
            {
                SelectedTarget = strLimitToSkill;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickSkill.Description = LanguageManager.GetString("String_Improvement_SelectSkillNamed", GlobalOptions.Language)
                        .Replace("{0}", _strFriendlyName);
                else
                    frmPickSkill.Description = LanguageManager.GetString("String_Improvement_SelectSkill", GlobalOptions.Language);

                string strTemp = bonusNode.SelectSingleNode("skillgroup")?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    frmPickSkill.OnlySkillGroup = strTemp;
                else
                {
                    XmlNode xmlSkillCategories = bonusNode.SelectSingleNode("skillcategories");
                    if (xmlSkillCategories != null)
                        frmPickSkill.LimitToCategories = xmlSkillCategories;
                    else
                    {
                        strTemp = bonusNode.SelectSingleNode("skillcategory")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.OnlyCategory = strTemp;
                        else
                        {
                            strTemp = bonusNode.SelectSingleNode("excludecategory")?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                                frmPickSkill.ExcludeCategory = strTemp;
                            else
                            {
                                strTemp = bonusNode.SelectSingleNode("limittoattribute")?.InnerText;
                                if (!string.IsNullOrEmpty(strTemp))
                                    frmPickSkill.LinkedAttribute = strTemp;
                            }
                        }
                    }
                }

                frmPickSkill.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedTarget = frmPickSkill.SelectedSkill;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedTarget + ')';
            }

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SwapSkillAttribute, _strUnique,
                0, 1, 0, 0, 0, 0, string.Empty, false, SelectedTarget);
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public void swapskillspecattribute(XmlNode bonusNode)
        {
            Log.Info("swapskillspecattribute");
            
            List<string> lstAbbrevs = new List<string>();
            XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute");
            if (xmlAttributeList?.Count > 0)
            {
                foreach (XmlNode objSubNode in xmlAttributeList)
                    lstAbbrevs.Add(objSubNode.InnerText);
            }
            else
            {
                lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                xmlAttributeList = bonusNode.SelectNodes("excludeattribute");
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Remove(objSubNode.InnerText);
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!_objCharacter.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!_objCharacter.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!_objCharacter.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            if (lstAbbrevs.Count == 1)
                LimitSelection = lstAbbrevs[0];

            Log.Info("swapskillspecattribute = " + bonusNode.OuterXml);

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                SelectedValue = LimitSelection;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                {
                    Description = !string.IsNullOrEmpty(_strFriendlyName)
                        ? LanguageManager.GetString("String_Improvement_SelectAttributeNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                        : LanguageManager.GetString("String_Improvement_SelectAttribute", GlobalOptions.Language)
                };

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickAttribute.SelectedAttribute;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';
            }

            string strLimitToSkill = bonusNode.SelectSingleNode("limittoskill")?.InnerText;
            if (!string.IsNullOrEmpty(strLimitToSkill))
            {
                SelectedTarget = strLimitToSkill;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickSkill.Description = LanguageManager.GetString("String_Improvement_SelectSkillNamed", GlobalOptions.Language)
                        .Replace("{0}", _strFriendlyName);
                else
                    frmPickSkill.Description = LanguageManager.GetString("String_Improvement_SelectSkill", GlobalOptions.Language);

                string strTemp = bonusNode.SelectSingleNode("skillgroup")?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    frmPickSkill.OnlySkillGroup = strTemp;
                else
                {
                    XmlNode xmlSkillCategories = bonusNode.SelectSingleNode("skillcategories");
                    if (xmlSkillCategories != null)
                        frmPickSkill.LimitToCategories = xmlSkillCategories;
                    else
                    {
                        strTemp = bonusNode.SelectSingleNode("skillcategory")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.OnlyCategory = strTemp;
                        else
                        {
                            strTemp = bonusNode.SelectSingleNode("excludecategory")?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                                frmPickSkill.ExcludeCategory = strTemp;
                            else
                            {
                                strTemp = bonusNode.SelectSingleNode("limittoattribute")?.InnerText;
                                if (!string.IsNullOrEmpty(strTemp))
                                    frmPickSkill.LinkedAttribute = strTemp;
                            }
                        }
                    }
                }

                frmPickSkill.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedTarget = frmPickSkill.SelectedSkill;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedTarget + ')';
            }

            // TODO: Allow selection of specializations through frmSelectSkillSpec
            string strSpec = bonusNode["spec"]?.InnerText ?? string.Empty;

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SwapSkillSpecAttribute, _strUnique,
                0, 1, 0, 0, 0, 0, strSpec, false, SelectedTarget);
        }

        // Select a Spell.
        public void selectspell(XmlNode bonusNode)
        {
            Log.Info("selectspell");
            // Display the Select Spell window.
            frmSelectSpell frmPickSpell = new frmSelectSpell(_objCharacter);

            string strCategory = bonusNode.Attributes?["category"]?.InnerText;
            if (!string.IsNullOrEmpty(strCategory))
                frmPickSpell.LimitCategory = strCategory;

            Log.Info("selectspell = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickSpell.ForceSpellName = ForcedValue;
                frmPickSpell.Opacity = 0;
            }

            frmPickSpell.IgnoreRequirements = bonusNode.Attributes?["ignorerequirements"]?.InnerText == bool.TrueString;

            frmPickSpell.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSpell.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            // Open the Spells XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("spells.xml");

            XmlNode node = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickSpell.SelectedSpell + "\"]") ??
                           objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + frmPickSpell.SelectedSpell + "\"]");
            if (node == null)
                throw new AbortedException();

            SelectedValue = node["name"]?.InnerText;

            Spell spell = new Spell(_objCharacter);
            // Check for SelectText.
            string strExtra = string.Empty;
            XmlNode xmlSelectText = node.SelectSingleNode("bonus/selecttext");
            if (xmlSelectText != null)
            {
                frmSelectText frmPickText = new frmSelectText
                {
                    Description =
                        LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                            .Replace("{0}", node["translate"]?.InnerText ?? node["name"]?.InnerText)
                };
                frmPickText.ShowDialog();
                // Make sure the dialogue window was not canceled.
                if (frmPickText.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                strExtra = frmPickText.SelectedValue;
            }
            spell.Create(node, strExtra);
            if (spell.InternalId.IsEmptyGuid())
                throw new AbortedException();
            spell.Grade = -1;
            _objCharacter.Spells.Add(spell);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(spell.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Spell,
                _strUnique);
        }

        // Add a specific Spell to the Character.
        public void addspell(XmlNode bonusNode)
        {
            Log.Info("addspell");

            Log.Info("addspell = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            XmlDocument objXmlSpellDocument = XmlManager.Load("spells.xml");

            XmlNode node = objXmlSpellDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + bonusNode.InnerText + "\"]");

            if (node == null)
                throw new AbortedException();
            // Check for SelectText.
            string strExtra = string.Empty;
            XmlNode xmlSelectText = node.SelectSingleNode("bonus/selecttext");
            if (xmlSelectText != null)
            {
                frmSelectText frmPickText = new frmSelectText
                {
                    Description =
                    LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                        .Replace("{0}", node["translate"]?.InnerText ?? node["name"]?.InnerText)
                };
                frmPickText.ShowDialog();
                // Make sure the dialogue window was not canceled.
                if (frmPickText.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                strExtra = frmPickText.SelectedValue;
            }

            Spell spell = new Spell(_objCharacter);
            spell.Create(node, strExtra);
            if (spell.InternalId.IsEmptyGuid())
                throw new AbortedException();
            spell.Grade = -1;
            _objCharacter.Spells.Add(spell);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(spell.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Spell,
                _strUnique);
        }

        // Select a Complex Form.
        public void selectcomplexform(XmlNode bonusNode)
        {
            Log.Info("selectcomplexform");
            
            Log.Info("selectcomplexform = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            string strSelectedComplexForm = ForcedValue;

            if (string.IsNullOrEmpty(strSelectedComplexForm))
            {
                // Display the Select ComplexForm window.
                frmSelectComplexForm frmPickComplexForm = new frmSelectComplexForm(_objCharacter);
                frmPickComplexForm.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickComplexForm.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                strSelectedComplexForm = frmPickComplexForm.SelectedComplexForm;
            }

            // Open the ComplexForms XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("complexforms.xml");

            XmlNode node = objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[id = \"" + strSelectedComplexForm + "\"]") ??
                           objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + strSelectedComplexForm + "\"]");
            if (node == null)
                throw new AbortedException();

            SelectedValue = node["name"]?.InnerText;

            ComplexForm objComplexform = new ComplexForm(_objCharacter);
            // Check for SelectText.
            string strExtra = string.Empty;
            XmlNode xmlSelectText = node.SelectSingleNode("bonus/selecttext");
            if (xmlSelectText != null)
            {
                frmSelectText frmPickText = new frmSelectText
                {
                    Description =
                        LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                            .Replace("{0}", node["translate"]?.InnerText ?? node["name"]?.InnerText)
                };
                frmPickText.ShowDialog();
                // Make sure the dialogue window was not canceled.
                if (frmPickText.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                strExtra = frmPickText.SelectedValue;
            }
            objComplexform.Create(node, strExtra);
            if (objComplexform.InternalId.IsEmptyGuid())
                throw new AbortedException();
            objComplexform.Grade = -1;

            _objCharacter.ComplexForms.Add(objComplexform);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objComplexform.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.ComplexForm,
                _strUnique);
        }

        // Add a specific ComplexForm to the Character.
        public void addcomplexform(XmlNode bonusNode)
        {
            Log.Info("addcomplexform");

            Log.Info("addcomplexform = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            XmlDocument objXmlComplexFormDocument = XmlManager.Load("complexforms.xml");

            XmlNode node = objXmlComplexFormDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + bonusNode.InnerText + "\"]");

            if (node == null)
                throw new AbortedException();
            // Check for SelectText.
            string strExtra = string.Empty;
            XmlNode xmlSelectText = node.SelectSingleNode("bonus/selecttext");
            if (xmlSelectText != null)
            {
                frmSelectText frmPickText = new frmSelectText
                {
                    Description =
                    LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                        .Replace("{0}", node["translate"]?.InnerText ?? node["name"]?.InnerText)
                };
                frmPickText.ShowDialog();
                // Make sure the dialogue window was not canceled.
                if (frmPickText.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                strExtra = frmPickText.SelectedValue;
            }

            ComplexForm objComplexform = new ComplexForm(_objCharacter);
            objComplexform.Create(node, strExtra);
            if (objComplexform.InternalId.IsEmptyGuid())
                throw new AbortedException();
            objComplexform.Grade = -1;

            _objCharacter.ComplexForms.Add(objComplexform);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objComplexform.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.ComplexForm,
                _strUnique);
        }

        // Add a specific Gear to the Character.
        public void addgear(XmlNode bonusNode)
        {
            Log.Info("addgear");

            Log.Info("addgear = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Adding Gear");
            string strName = bonusNode["name"]?.InnerText ?? string.Empty;
            string strCategory = bonusNode["category"]?.InnerText ?? string.Empty;
            XmlNode node = XmlManager.Load("gear.xml").SelectSingleNode("/chummer/gears/gear[name = \"" + strName + "\" and category = \"" + strCategory + "\"]");

            if (node == null)
                throw new AbortedException();
            int intRating = 1;
            string strTemp = string.Empty;
            if (bonusNode.TryGetStringFieldQuickly("rating", ref strTemp))
                intRating = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            decimal decQty = 1.0m;
            if (bonusNode["quantity"] != null)
                decQty = Convert.ToDecimal(bonusNode["quantity"].InnerText, GlobalOptions.InvariantCultureInfo);

            // Create the new piece of Gear.
            List<Weapon> lstWeapons = new List<Weapon>();

            Gear objNewGear = new Gear(_objCharacter);
            objNewGear.Create(node, intRating, lstWeapons, ForcedValue);

            if (objNewGear.InternalId.IsEmptyGuid())
                throw new AbortedException();

            objNewGear.Quantity = decQty;

            // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
            if (_objCharacter.ActiveCommlink == null && objNewGear.IsCommlink)
            {
                objNewGear.SetActiveCommlink(_objCharacter, true);
            }

            if (bonusNode["fullcost"] == null)
                objNewGear.Cost = "0";
            // Create any Weapons that came with this Gear.
            foreach (Weapon objWeapon in lstWeapons)
                _objCharacter.Weapons.Add(objWeapon);

            objNewGear.ParentID = SourceName;

            _objCharacter.Gear.Add(objNewGear);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objNewGear.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Gear,
                _strUnique);
        }

        // Add a specific Gear to the Character.
        public void addweapon(XmlNode bonusNode)
        {
            Log.Info("addweapon");

            Log.Info("addweapon = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Adding Weapon");
            string strName = bonusNode["name"]?.InnerText ?? throw new AbortedException();
            XmlNode node = XmlManager.Load("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"" + strName + "\"]") ?? throw new AbortedException();

            // Create the new piece of Gear.
            List<Weapon> lstWeapons = new List<Weapon>();

            Weapon objNewWeapon = new Weapon(_objCharacter);
            objNewWeapon.Create(node, lstWeapons);

            if (objNewWeapon.InternalId.IsEmptyGuid())
                throw new AbortedException();

            // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
            if (_objCharacter.ActiveCommlink == null && objNewWeapon.IsCommlink)
            {
                objNewWeapon.SetActiveCommlink(_objCharacter, true);
            }

            if (bonusNode["fullcost"] == null)
                objNewWeapon.Cost = "0";

            // Create any Weapons that came with this Gear.
            foreach (Weapon objWeapon in lstWeapons)
                _objCharacter.Weapons.Add(objWeapon);

            objNewWeapon.ParentID = SourceName;

            _objCharacter.Weapons.Add(objNewWeapon);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objNewWeapon.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Weapon,
                _strUnique);
        }

        // Select an AI program.
        public void selectaiprogram(XmlNode bonusNode)
        {
            Log.Info("selectaiprogram");
            
            Log.Info("selectaiprogram = " + bonusNode.OuterXml);

            Log.Info("_strForcedValue = " + ForcedValue);

            XmlNode xmlProgram = null;
            XmlDocument xmlDocument = XmlManager.Load("programs.xml");
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                xmlProgram = xmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + ForcedValue + "\"]") ??
                    xmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + ForcedValue + "\"]");
            }

            if (xmlProgram == null)
            {
                // Display the Select Program window.
                frmSelectAIProgram frmPickProgram = new frmSelectAIProgram(_objCharacter);
                frmPickProgram.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickProgram.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                xmlProgram = xmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + frmPickProgram.SelectedProgram + "\"]");
            }
            
            if (xmlProgram != null)
            {
                // Check for SelectText.
                string strExtra = string.Empty;
                XmlNode xmlSelectText = xmlProgram.SelectSingleNode("bonus/selecttext");
                if (xmlSelectText != null)
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        Description =
                            LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                                .Replace("{0}", xmlProgram["translate"]?.InnerText ?? xmlProgram["name"]?.InnerText)
                    };
                    frmPickText.ShowDialog();
                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.DialogResult == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }
                    strExtra = frmPickText.SelectedValue;
                }
                
                AIProgram objProgram = new AIProgram(_objCharacter);
                objProgram.Create(xmlProgram, strExtra, false);
                if (objProgram.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                _objCharacter.AIPrograms.Add(objProgram);

                SelectedValue = objProgram.DisplayNameShort(GlobalOptions.Language);
                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';

                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SourceName = " + SourceName);

                Log.Info("Calling CreateImprovement");
                CreateImprovement(objProgram.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.AIProgram,
                    _strUnique);
            }
            else
                throw new AbortedException();
        }

        // Select an AI program.
        public void selectinherentaiprogram(XmlNode bonusNode)
        {
            Log.Info("selectaiprogram");
            // Display the Select Spell window.
            frmSelectAIProgram frmPickProgram = new frmSelectAIProgram(_objCharacter, false, true);

            Log.Info("selectaiprogram = " + bonusNode.OuterXml);

            Log.Info("_strForcedValue = " + ForcedValue);

            XmlNode xmlProgram = null;
            XmlDocument xmlDocument = XmlManager.Load("programs.xml");
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                xmlProgram = xmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + ForcedValue + "\"]") ??
                    xmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + ForcedValue + "\"]");
            }

            if (xmlProgram == null)
            {
                frmPickProgram.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickProgram.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                xmlProgram = xmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + frmPickProgram.SelectedProgram + "\"]");
            }
            
            if (xmlProgram != null)
            {
                // Check for SelectText.
                string strExtra = string.Empty;
                XmlNode xmlSelectText = xmlProgram.SelectSingleNode("bonus/selecttext");
                if (xmlSelectText != null)
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        Description =
                            LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                                .Replace("{0}", xmlProgram["translate"]?.InnerText ?? xmlProgram["name"]?.InnerText)
                    };
                    frmPickText.ShowDialog();
                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.DialogResult == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }
                    strExtra = frmPickText.SelectedValue;
                }
                
                AIProgram objProgram = new AIProgram(_objCharacter);
                objProgram.Create(xmlProgram, strExtra, false);
                if (objProgram.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                SelectedValue = objProgram.DisplayNameShort(GlobalOptions.Language);
                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';

                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SourceName = " + SourceName);

                _objCharacter.AIPrograms.Add(objProgram);

                Log.Info("Calling CreateImprovement");
                CreateImprovement(objProgram.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.AIProgram,
                    _strUnique);
            }
            else
                throw new AbortedException();
        }

        // Select a Contact
        public void selectcontact(XmlNode bonusNode)
        {
            Log.Info("selectcontact");
            XmlNode nodSelect = bonusNode;

            frmSelectItem frmSelect = new frmSelectItem();

            string strMode = nodSelect["type"]?.InnerText ?? "all";

            List<Contact> lstSelectedContacts;
            if (strMode == "all")
            {
                lstSelectedContacts = new List<Contact>(_objCharacter.Contacts);
            }
            else if (strMode == "group" || strMode == "nongroup")
            {
                bool blnGroup = strMode == "group";


                //Select any contact where IsGroup equals blnGroup
                //and add to a list
                lstSelectedContacts = _objCharacter.Contacts.Where(x => x.IsGroup == blnGroup).ToList();
            }
            else
            {
                throw new AbortedException();
            }

            if (lstSelectedContacts.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NoContactFound", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_NoContactFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new AbortedException();
            }

            int count = 0;
            //Black magic LINQ to cast content of list to another type
            List<ListItem> contacts = new List<ListItem>(from x in lstSelectedContacts select new ListItem(count++.ToString(), x.Name));

            frmSelect.GeneralItems = contacts;
            frmSelect.ShowDialog();
            
            if (frmSelect.DialogResult == DialogResult.Cancel)
                throw new AbortedException();
            
            Contact objSelectedContact = int.TryParse(frmSelect.SelectedItem, out int intIndex) ? lstSelectedContacts[intIndex] : throw new AbortedException();
            
            string strTemp = string.Empty;
            if (nodSelect.TryGetStringFieldQuickly("forcedloyalty", ref strTemp))
            {
                int intForcedLoyalty = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
                objSelectedContact.ForcedLoyalty = Math.Max(intForcedLoyalty, objSelectedContact.ForcedLoyalty);
                CreateImprovement(objSelectedContact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForcedLoyalty, _strUnique, intForcedLoyalty);
            }
            if (nodSelect["free"] != null)
            {
                objSelectedContact.Free = true;
                CreateImprovement(objSelectedContact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactMakeFree, _strUnique);
            }
            if (nodSelect["forcegroup"] != null)
            {
                objSelectedContact.GroupEnabled = false;
                CreateImprovement(objSelectedContact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForceGroup, _strUnique);
            }
            if (string.IsNullOrWhiteSpace(SelectedValue))
            {
                SelectedValue = objSelectedContact.Name;
            }
            else
            {
                SelectedValue += (", " + objSelectedContact.Name);
            }
        }

        public void addcontact(XmlNode bonusNode)
        {
            Log.Info("addcontact");

            int intLoyalty = 1;
            int intConnection = 1;

            string strTemp = string.Empty;
            if (bonusNode.TryGetStringFieldQuickly("loyalty", ref strTemp))
                intLoyalty = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            if (bonusNode.TryGetStringFieldQuickly("connection", ref strTemp))
                intConnection = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            bool group = bonusNode["group"] != null;
            bool canwrite = bonusNode["canwrite"] != null;
            Contact contact = new Contact(_objCharacter)
            {
                IsGroup = group,
                Loyalty = intLoyalty,
                Connection = intConnection,
                ReadOnly = !canwrite
            };
            _objCharacter.Contacts.Add(contact);

            CreateImprovement(contact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.AddContact, contact.GUID);
            
            if (bonusNode.TryGetStringFieldQuickly("forcedloyalty", ref strTemp))
            {
                int intForcedLoyalty = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
                contact.ForcedLoyalty = Math.Max(intForcedLoyalty, contact.ForcedLoyalty);
                CreateImprovement(contact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForcedLoyalty, _strUnique, intForcedLoyalty);
            }
            if (bonusNode["free"] != null)
            {
                contact.Free = true;
                CreateImprovement(contact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactMakeFree, _strUnique);
            }
            if (bonusNode["forcegroup"] != null)
            {
                contact.GroupEnabled = false;
                CreateImprovement(contact.GUID, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForceGroup, _strUnique);
            }
        }

        // Affect a Specific CharacterAttribute.
        public void specificattribute(XmlNode bonusNode)
        {
            Log.Info("specificattribute");

            // Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
            // Record the improvement.
            int intMin = 0;
            int intAug = 0;
            int intMax = 0;
            int intAugMax = 0;
            string strAttribute = bonusNode["name"]?.InnerText;

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intMin = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["val"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intAug = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["max"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.EndsWith("-natural"))
                {
                    intMax = Convert.ToInt32(strTemp.TrimEndOnce("-natural", true)) -
                             _objCharacter.GetAttribute(strAttribute).MetatypeMaximum;
                }
                else
                    intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            }
            strTemp = bonusNode["aug"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);

            string strUseUnique = _strUnique;
            XmlNode xmlPrecedenceNode = bonusNode.SelectSingleNode("name/@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.InnerText;

            if (bonusNode["affectbase"] != null)
                strAttribute += "Base";

            CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                strUseUnique, 0, 1, intMin, intMax, intAug, intAugMax);
        }

        // Add a paid increase to an attribute
        public void attributelevel(XmlNode bonusNode)
        {
            Log.Info(new object[] { "attributelevel", bonusNode.OuterXml });
            string strAttrib = string.Empty;
            int value = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref value);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strAttrib))
            {
                CreateImprovement(strAttrib, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Attributelevel, _strUnique, value);
            }
            else if (bonusNode["options"] != null)
            {
                List<string> lstAbbrevs = new List<string>();
                foreach (XmlNode objSubNode in bonusNode["options"])
                    lstAbbrevs.Add(objSubNode.InnerText);

                lstAbbrevs.Remove("ESS");
                if (!_objCharacter.MAGEnabled)
                {
                    lstAbbrevs.Remove("MAG");
                    lstAbbrevs.Remove("MAGAdept");
                }
                else if (!_objCharacter.IsMysticAdept || !_objCharacter.Options.MysAdeptSecondMAGAttribute)
                    lstAbbrevs.Remove("MAGAdept");

                if (!_objCharacter.RESEnabled)
                    lstAbbrevs.Remove("RES");
                if (!_objCharacter.DEPEnabled)
                    lstAbbrevs.Remove("DEP");

                Log.Info("_strForcedValue = " + ForcedValue);
                Log.Info("_strLimitSelection = " + LimitSelection);

                // Check to see if there is only one possible selection because of _strLimitSelection.
                if (!string.IsNullOrEmpty(ForcedValue))
                    LimitSelection = ForcedValue;

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    lstAbbrevs.RemoveAll(x => x != LimitSelection);
                }

                frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
                {
                    Description = !string.IsNullOrEmpty(_strFriendlyName)
                        ? LanguageManager.GetString("String_Improvement_SelectAttributeNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                        : LanguageManager.GetString("String_Improvement_SelectAttribute", GlobalOptions.Language)
                };

                Log.Info("attributelevel = " + bonusNode.OuterXml);

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickAttribute.SelectedAttribute;
                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';

                Log.Info("_strSelectedValue = " + frmPickAttribute.SelectedAttribute);
                Log.Info("SourceName = " + SourceName);

                CreateImprovement(frmPickAttribute.SelectedAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attributelevel, _strUnique, value);
            }
            else
            {
                Log.Error(new object[] { "attributelevel", bonusNode.OuterXml });
            }
        }

        public void skilllevel(XmlNode bonusNode)
        {
            Log.Info(new object[] { "skilllevel", bonusNode.OuterXml });
            string strSkill = string.Empty;
            int intValue = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref intValue);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkill))
            {
                CreateImprovement(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillLevel, _strUnique, intValue);
            }
            else
            {
                Log.Error(new object[] { "skilllevel", bonusNode.OuterXml });
            }
        }

        public void pushtext(XmlNode bonusNode)
        {
            string strPush = bonusNode.InnerText;
            if (!string.IsNullOrWhiteSpace(strPush))
            {
                _objCharacter.Pushtext.Push(strPush);
            }
        }

        public void activesoft(XmlNode bonusNode)
        {
            Log.Info("activesoft");
            string strNodeOuterXml = bonusNode.OuterXml;
            Log.Info("activesoft = " + strNodeOuterXml);
            string strForcedValue = ForcedValue;
            Log.Info("_strForcedValue = " + strForcedValue);

            bool blnDummy = false;
            SelectedValue = string.IsNullOrEmpty(strForcedValue) ? ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName, ref blnDummy) : strForcedValue;
            if (blnDummy)
                throw new AbortedException();

            string strVal = bonusNode["val"]?.InnerText;

            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            if (SelectedValue.Contains("Exotic Melee Weapon") ||
                SelectedValue.Contains("Exotic Ranged Weapon") ||
                SelectedValue.Contains("Pilot Exotic Vehicle"))
            {
                foreach (Skill objLoopSkill in _objCharacter.SkillsSection.Skills.Where(s => s.IsExoticSkill))
                {
                    ExoticSkill objExoticSkill = (ExoticSkill)objLoopSkill;
                    if ($"{objExoticSkill.Name} ({objExoticSkill.Specific})" != SelectedValue)
                        continue;
                    // We've found the selected Skill.
                    if (!string.IsNullOrEmpty(strVal))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Activesoft,
                            _strUnique,
                            ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating));
                    }
                }
            }
            else
            {
                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (objSkill.Name != SelectedValue)
                        continue;
                    // We've found the selected Skill.
                    if (!string.IsNullOrEmpty(strVal))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Activesoft,
                            _strUnique,
                            ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating));
                    }
                }
            }

            if (bonusNode["addknowledge"] != null)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(_objCharacter, SelectedValue);

                _objCharacter.SkillsSection.KnowsoftSkills.Add(objKnowledgeSkill);
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillsoftAccess) > 0)
                {
                    _objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                }

                Log.Info("Calling CreateImprovement");
                CreateImprovement(objKnowledgeSkill.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillsoft, _strUnique, ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating));
            }
        }

        public void skillsoft(XmlNode bonusNode)
        {
            string strNodeOuterXml = bonusNode.OuterXml;
            Log.Info("skillsoft = " + strNodeOuterXml);
            string strForcedValue = ForcedValue;
            Log.Info("_strForcedValue = " + strForcedValue);

            bool blnIsKnowledgeSkill = true;
            SelectedValue = string.IsNullOrEmpty(strForcedValue) ? ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName, ref blnIsKnowledgeSkill) : strForcedValue;

            string strVal = bonusNode["val"]?.InnerText;
            
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);
            
            KnowledgeSkill objSkill = new KnowledgeSkill(_objCharacter, SelectedValue);

            _objCharacter.SkillsSection.KnowsoftSkills.Add(objSkill);
            if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillsoftAccess) > 0)
            {
                _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
            }

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objSkill.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillsoft, _strUnique, ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating));
        }

        public void knowledgeskilllevel(XmlNode bonusNode)
        {
            //Theoretically life modules, right now we just give out free points and let people sort it out themselves.
            //Going to be fun to do the real way, from a computer science perspective, but i don't feel like using 2 weeks on that now

            int val = bonusNode["val"] != null ? ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"].InnerText, _intRating) : 1;
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, _strUnique, val);
        }

        public void knowledgeskillpoints(XmlNode bonusNode)
        {
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, Convert.ToInt32(bonusNode.Value)));
        }

        public void skillgrouplevel(XmlNode bonusNode)
        {
            Log.Info(new object[] { "skillgrouplevel", bonusNode.OuterXml });
            string strSkillGroup = string.Empty;
            int value = 1;
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkillGroup) &&
                bonusNode.TryGetInt32FieldQuickly("val", ref value))
            {
                CreateImprovement(strSkillGroup, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroupLevel, _strUnique, value);
            }
            else
            {
                Log.Error(new object[] { "skillgrouplevel", bonusNode.OuterXml });
            }
        }

        // Change the maximum number of BP that can be spent on Nuyen.
        public void nuyenmaxbp(XmlNode bonusNode)
        {
            Log.Info("nuyenmaxbp");
            Log.Info("nuyenmaxbp = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NuyenMaxBP, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Apply a bonus/penalty to physical limit.
        public void physicallimit(XmlNode bonusNode)
        {
            Log.Info("physicallimit");
            Log.Info("physicallimit = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement("Physical", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalLimit,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Apply a bonus/penalty to mental limit.
        public void mentallimit(XmlNode bonusNode)
        {
            Log.Info("mentallimit");
            Log.Info("mentallimit = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement("Mental", _objImprovementSource, SourceName, Improvement.ImprovementType.MentalLimit,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Apply a bonus/penalty to social limit.
        public void sociallimit(XmlNode bonusNode)
        {
            Log.Info("sociallimit");
            Log.Info("sociallimit = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement("Social", _objImprovementSource, SourceName, Improvement.ImprovementType.SocialLimit,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
        public void nuyenamt(XmlNode bonusNode)
        {
            Log.Info("nuyenamt");
            Log.Info("nuyenamt = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Nuyen, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Improve Condition Monitors.
        public void conditionmonitor(XmlNode bonusNode)
        {
            Log.Info("conditionmonitor");
            Log.Info("conditionmonitor = " + bonusNode.OuterXml);
            string strTemp = bonusNode["physical"]?.InnerText;
            // Physical Condition.
            if (!string.IsNullOrEmpty(strTemp))
            {
                Log.Info("Calling CreateImprovement for Physical");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCM, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
            }
            strTemp = bonusNode["stun"]?.InnerText;
            // Stun Condition.
            if (!string.IsNullOrEmpty(strTemp))
            {
                Log.Info("Calling CreateImprovement for Stun");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StunCM, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
            }

            // Condition Monitor Threshold.
            XmlNode objNode = bonusNode["threshold"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes?["precedence"]?.InnerText;
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                Log.Info("Calling CreateImprovement for Threshold");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
                    ImprovementManager.ValueToInt(_objCharacter, objNode.InnerText, _intRating));
            }

            // Condition Monitor Threshold Offset. (Additioal boxes appear before the FIRST Condition Monitor penalty)
            objNode = bonusNode["thresholdoffset"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes?["precedence"]?.InnerText;
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                Log.Info("Calling CreateImprovement for Threshold Offset");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMThresholdOffset,
                    strUseUnique, ImprovementManager.ValueToInt(_objCharacter, objNode.InnerText, _intRating));
            }
            // Condition Monitor Threshold Offset that must be shared between the two. (Additioal boxes appear before the FIRST Condition Monitor penalty)
            objNode = bonusNode["sharedthresholdoffset"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes?["precedence"]?.InnerText;
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                Log.Info("Calling CreateImprovement for Threshold Offset");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMSharedThresholdOffset,
                    strUseUnique, ImprovementManager.ValueToInt(_objCharacter, objNode.InnerText, _intRating));
            }

            // Condition Monitor Overflow.
            objNode = bonusNode["overflow"];
            if (objNode != null)
            {
                Log.Info("Calling CreateImprovement for Overflow");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMOverflow, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, objNode.InnerText, _intRating));
            }
        }

        // Improve Living Personal Attributes.
        public void livingpersona(XmlNode bonusNode)
        {
            Log.Info("livingpersona");
            Log.Info("livingpersona = " + bonusNode.OuterXml);

            // Device Rating.
            string strBonus = bonusNode["devicerating"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                if (strBonus.StartsWith("FixedValues("))
                {
                    string[] strValues = strBonus.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strBonus = strValues[Math.Max(Math.Min(_intRating, strValues.Length) - 1, 0)];
                }
                strBonus = strBonus.Replace("Rating", _intRating.ToString());
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = '+' + strBonus;
                Log.Info("Calling CreateImprovement for device rating");
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaDeviceRating, _strUnique);
            }

            // Attack.
            strBonus = bonusNode["programlimit"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                if (strBonus.StartsWith("FixedValues("))
                {
                    string[] strValues = strBonus.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strBonus = strValues[Math.Max(Math.Min(_intRating, strValues.Length) - 1, 0)];
                }
                strBonus = strBonus.Replace("Rating", _intRating.ToString());
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = '+' + strBonus;
                Log.Info("Calling CreateImprovement for program limit");
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaProgramLimit, _strUnique);
            }

            // Attack.
            strBonus = bonusNode["attack"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                if (strBonus.StartsWith("FixedValues("))
                {
                    string[] strValues = strBonus.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strBonus = strValues[Math.Max(Math.Min(_intRating, strValues.Length) - 1, 0)];
                }
                strBonus = strBonus.Replace("Rating", _intRating.ToString());
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = '+' + strBonus;
                Log.Info("Calling CreateImprovement for attack");
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaAttack, _strUnique);
            }

            // Sleaze.
            strBonus = bonusNode["sleaze"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                if (strBonus.StartsWith("FixedValues("))
                {
                    string[] strValues = strBonus.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strBonus = strValues[Math.Max(Math.Min(_intRating, strValues.Length) - 1, 0)];
                }
                strBonus = strBonus.Replace("Rating", _intRating.ToString());
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = '+' + strBonus;
                Log.Info("Calling CreateImprovement for sleaze");
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSleaze, _strUnique);
            }

            // Data Processing.
            strBonus = bonusNode["dataprocessing"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                if (strBonus.StartsWith("FixedValues("))
                {
                    string[] strValues = strBonus.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strBonus = strValues[Math.Max(Math.Min(_intRating, strValues.Length) - 1, 0)];
                }
                strBonus = strBonus.Replace("Rating", _intRating.ToString());
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = '+' + strBonus;
                Log.Info("Calling CreateImprovement for data processing");
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaDataProcessing, _strUnique);
            }

            // Firewall.
            strBonus = bonusNode["firewall"]?.InnerText;
            if (!string.IsNullOrEmpty(strBonus))
            {
                if (strBonus.StartsWith("FixedValues("))
                {
                    string[] strValues = strBonus.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strBonus = strValues[Math.Max(Math.Min(_intRating, strValues.Length) - 1, 0)];
                }
                strBonus = strBonus.Replace("Rating", _intRating.ToString());
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = '+' + strBonus;
                Log.Info("Calling CreateImprovement for firewall");
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaFirewall, _strUnique);
            }
        }

        // The Improvement adjusts a specific Skill.
        public void specificskill(XmlNode bonusNode)
        {
            Log.Info("specificskill");
            Log.Info("specificskill = " + bonusNode.OuterXml);
            bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;
            string strCondition = bonusNode["condition"]?.InnerText ?? string.Empty;

            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;

            string strBonusNodeName = bonusNode["name"]?.InnerText;
            // Record the improvement.
            string strTemp = bonusNode["bonus"]?.InnerXml;
            if (!string.IsNullOrEmpty(strTemp))
            {
                Log.Info("Calling CreateImprovement for bonus");
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating), 1, 0, 0, 0,
                    0, string.Empty, blnAddToRating, string.Empty, strCondition);
            }
            if (bonusNode["disablespecializationeffects"] != null)
            {
                Log.Info("Calling CreateImprovement for disabling specializtion effects");
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.DisableSpecializationEffects,
                    strUseUnique, 0, 1, 0, 0, 0, 0, string.Empty, false, string.Empty, strCondition);
            }

            strTemp = bonusNode["max"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                Log.Info("Calling CreateImprovement for max");
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating), 0,
                    0,
                    string.Empty, blnAddToRating, string.Empty, strCondition);
            }
            strTemp = bonusNode["misceffect"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                Log.Info("Calling CreateImprovement for misc effect");
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, 0, 0,
                    0, string.Empty, false, strTemp, strCondition);
            }
        }

        public void reflexrecorderoptimization(XmlNode bonusNode)
        {
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ReflexRecorderOptimization, _strUnique);
        }

        // The Improvement adds a martial art
        public void martialart(XmlNode bonusNode)
        {
            Log.Info("martialart");
            Log.Info("martialart = " + bonusNode.OuterXml);
            XmlDocument _objXmlDocument = XmlManager.Load("martialarts.xml");
            XmlNode objXmlArt = _objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + bonusNode.InnerText + "\"]");
            
            MartialArt objMartialArt = new MartialArt(_objCharacter);
            objMartialArt.Create(objXmlArt);
            objMartialArt.IsQuality = true;
            _objCharacter.MartialArts.Add(objMartialArt);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objMartialArt.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.MartialArt,
                _strUnique);
        }

        // The Improvement adds a limit modifier
        public void limitmodifier(XmlNode bonusNode)
        {
            Log.Info("limitmodifier");
            Log.Info("limitmodifier = " + bonusNode.OuterXml);
            string strLimit = bonusNode["limit"]?.InnerText;
            string strBonus = bonusNode["value"]?.InnerText;
            if (strBonus == "Rating")
            {
                strBonus = _intRating.ToString();
            }
            string strCondition = bonusNode["condition"]?.InnerText ?? string.Empty;
            int intBonus = strBonus == "Rating" ? _intRating : Convert.ToInt32(strBonus);
            Log.Info("Calling CreateImprovement");
            LimitModifier lm = new LimitModifier(_objCharacter);
            lm.Create(_strFriendlyName,intBonus,strLimit,strCondition);
            _objCharacter.LimitModifiers.Add(lm);
            CreateImprovement(lm.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitModifier,
                _strFriendlyName, intBonus, 0, 0, 0, 0, 0, string.Empty, false, string.Empty, strCondition);
        }

        // The Improvement adjusts a Skill Category.
        public void skillcategory(XmlNode bonusNode)
        {
            Log.Info("skillcategory");
            Log.Info("skillcategory = " + bonusNode.OuterXml);
            
            string strName = bonusNode["name"]?.InnerText;
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;
                string strExclude = bonusNode["exclude"]?.InnerText;
                if (!string.IsNullOrEmpty(strExclude))
                {
                    Log.Info("Calling CreateImprovement - exclude");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillCategory, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1, 0,
                        0,
                        0, 0, strExclude, blnAddToRating);
                }
                else
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillCategory, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1, 0,
                        0,
                        0, 0, string.Empty, blnAddToRating);
                }
            }
        }

        // The Improvement adjusts a Skill Group.
        public void skillgroup(XmlNode bonusNode)
        {
            Log.Info("skillgroup");
            Log.Info("skillgroup = " + bonusNode.OuterXml);
            
            string strName = bonusNode["name"]?.InnerText;
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;
                string strExclude = bonusNode["exclude"]?.InnerText;
                if (!string.IsNullOrEmpty(strExclude))
                {
                    Log.Info("Calling CreateImprovement - exclude");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillGroup, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1, 0, 0, 0,
                        0, strExclude, blnAddToRating);
                }
                else
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillGroup, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1, 0, 0, 0,
                        0, string.Empty, blnAddToRating);
                }
            }
        }

        // The Improvement adjust Skills when used with the given CharacterAttribute.
        public void skillattribute(XmlNode bonusNode)
        {
            Log.Info("skillattribute");
            Log.Info("skillattribute = " + bonusNode.OuterXml);

            string strUseUnique = _strUnique;
            XmlNode xmlPrecedenceNode = bonusNode.SelectSingleNode("name/@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.InnerText;
            
            string strName = bonusNode["name"]?.InnerText;
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;
                string strExclude = bonusNode["exclude"]?.InnerText;
                if (!string.IsNullOrEmpty(strExclude))
                {
                    Log.Info("Calling CreateImprovement - exclude");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillAttribute, strUseUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1,
                        0, 0, 0, 0, strExclude, blnAddToRating);
                }
                else
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillAttribute, strUseUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1,
                        0, 0, 0, 0, string.Empty, blnAddToRating);
                }
            }
        }

        // The Improvement adjust Skills whose linked attribute is the given CharacterAttribute.
        public void skilllinkedattribute(XmlNode bonusNode)
        {
            Log.Info("skilllinkedattribute");
            Log.Info("skilllinkedattribute = " + bonusNode.OuterXml);

            string strUseUnique = _strUnique;
            XmlNode xmlPrecedenceNode = bonusNode.SelectSingleNode("name/@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.InnerText;

            string strName = bonusNode["name"]?.InnerText;
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerText == bool.TrueString;
                string strExclude = bonusNode["exclude"]?.InnerText;
                if (!string.IsNullOrEmpty(strExclude))
                {
                    Log.Info("Calling CreateImprovement - exclude");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillLinkedAttribute, strUseUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1,
                        0, 0, 0, 0, strExclude, blnAddToRating);
                }
                else
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(strName, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.SkillLinkedAttribute, strUseUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerXml, _intRating), 1,
                        0, 0, 0, 0, string.Empty, blnAddToRating);
                }
            }
        }

        // The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical CharacterAttribute).
        public void skillarticulation(XmlNode bonusNode)
        {
            Log.Info("skillarticulation");
            Log.Info("skillarticulation = " + bonusNode.OuterXml);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EnhancedArticulation,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["bonus"]?.InnerText, _intRating));
        }

        // Check for Armor modifiers.
        public void armor(XmlNode bonusNode)
        {
            Log.Info("armor");
            Log.Info("armor = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Armor, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Fire Armor modifiers.
        public void firearmor(XmlNode bonusNode)
        {
            Log.Info("firearmor");
            Log.Info("firearmor = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FireArmor, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Cold Armor modifiers.
        public void coldarmor(XmlNode bonusNode)
        {
            Log.Info("coldarmor");
            Log.Info("coldarmor = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ColdArmor, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Electricity Armor modifiers.
        public void electricityarmor(XmlNode bonusNode)
        {
            Log.Info("electricityarmor");
            Log.Info("electricityarmor = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ElectricityArmor, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Acid Armor modifiers.
        public void acidarmor(XmlNode bonusNode)
        {
            Log.Info("acidarmor");
            Log.Info("acidarmor = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AcidArmor, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Falling Armor modifiers.
        public void fallingarmor(XmlNode bonusNode)
        {
            Log.Info("fallingarmor");
            Log.Info("fallingarmor = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FallingArmor, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Dodge modifiers.
        public void dodge(XmlNode bonusNode)
        {
            Log.Info("dodge");
            Log.Info("dodge = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerText;
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Dodge, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Reach modifiers.
        public void reach(XmlNode bonusNode)
        {
            Log.Info("reach");
            Log.Info("reach = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Reach, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Unarmed Damage Value modifiers.
        public void unarmeddv(XmlNode bonusNode)
        {
            Log.Info("unarmeddv");
            Log.Info("unarmeddv = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDV, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Unarmed Damage Value Physical.
        public void unarmeddvphysical(XmlNode bonusNode)
        {
            Log.Info("unarmeddvphysical");
            Log.Info("unarmeddvphysical = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDVPhysical, _strUnique);
        }

        // Check for Unarmed Armor Penetration.
        public void unarmedap(XmlNode bonusNode)
        {
            Log.Info("unarmedap");
            Log.Info("unarmedap = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedAP, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Unarmed Armor Penetration.
        public void unarmedreach(XmlNode bonusNode)
        {
            Log.Info("unarmedreach");
            Log.Info("unarmedreach = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedReach, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Initiative modifiers.
        public void initiative(XmlNode bonusNode)
        {
            Log.Info("initiative");
            Log.Info("initiative = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Initiative, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Initiative Pass modifiers. Only the highest one ever applies. Legacy method for old characters.
        public void initiativepass(XmlNode bonusNode)
        {
            initiativedice(bonusNode);
        }

        // Check for Initiative Pass modifiers. Only the highest one ever applies.
        public void initiativedice(XmlNode bonusNode)
        {
            Log.Info("initiativedice");
            Log.Info("initiativedice = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");

            string strUseUnique = bonusNode.Name;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecedence))
                strUseUnique = "precedence" + strPrecedence;

            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDice,
                strUseUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Initiative Dice modifiers. Only the highest one ever applies. Legacy method for old characters.
        public void initiativepassadd(XmlNode bonusNode)
        {
            initiativediceadd(bonusNode);
        }

        // Check for Initiative Dice modifiers. Only the highest one ever applies.
        public void initiativediceadd(XmlNode bonusNode)
        {
            Log.Info("initiativediceadd");
            Log.Info("initiativediceadd = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDiceAdd, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Matrix Initiative modifiers.
        public void matrixinitiative(XmlNode bonusNode)
        {
            Log.Info("matrixinitiative");
            Log.Info("matrixinitiative = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiative, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public void matrixinitiativepass(XmlNode bonusNode)
        {
            matrixinitiativedice(bonusNode);
        }

        // Check for Matrix Initiative Pass modifiers.
        public void matrixinitiativedice(XmlNode bonusNode)
        {
            Log.Info("matrixinitiativedice");
            Log.Info("matrixinitiativedice = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                "matrixinitiativepass", ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public void matrixinitiativepassadd(XmlNode bonusNode)
        {
            matrixinitiativediceadd(bonusNode);
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public void matrixinitiativediceadd(XmlNode bonusNode)
        {
            Log.Info("matrixinitiativediceadd");
            Log.Info("matrixinitiativediceadd = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Lifestyle cost modifiers.
        public void lifestylecost(XmlNode bonusNode)
        {
            Log.Info("lifestylecost");
            Log.Info("lifestylecost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.LifestyleCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for basic Lifestyle cost modifiers.
        public void basiclifestylecost(XmlNode bonusNode)
        {
            Log.Info("basiclifestylecost");
            Log.Info("basiclifestylecost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BasicLifestyleCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Genetech Cost modifiers.
        public void genetechcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("genetechcostmultiplier");
            Log.Info("genetechcostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechCostMultiplier,
                _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }
        
        // Check for Basic Bioware Essence Cost modifiers.
        public void basicbiowareessmultiplier(XmlNode bonusNode)
        {
            Log.Info("basicbiowareessmultiplier");
            Log.Info("basicbiowareessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BasicBiowareEssCost,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Bioware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void biowareessmultiplier(XmlNode bonusNode)
        {
            Log.Info("biowareessmultiplier");
            Log.Info("biowareessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Bioware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void biowaretotalessmultiplier(XmlNode bonusNode)
        {
            Log.Info("biowaretotalessmultiplier");
            Log.Info("biowaretotalessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareTotalEssMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Cybeware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void cyberwareessmultiplier(XmlNode bonusNode)
        {
            Log.Info("cyberwareessmultiplier");
            Log.Info("cyberwareessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Cyberware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void cyberwaretotalessmultiplier(XmlNode bonusNode)
        {
            Log.Info("cyberwaretotalessmultiplier");
            Log.Info("cyberwaretotalessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareTotalEssMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Non-Retroactive Bioware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void biowareessmultipliernonretroactive(XmlNode bonusNode)
        {
            Log.Info("biowareessmultiplier");
            Log.Info("biowareessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Non-Retroactive Bioware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void biowaretotalessmultipliernonretroactive(XmlNode bonusNode)
        {
            Log.Info("biowaretotalessmultiplier");
            Log.Info("biowaretotalessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareTotalEssMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Non-Retroactive Cybeware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void cyberwareessmultipliernonretroactive(XmlNode bonusNode)
        {
            Log.Info("cyberwareessmultiplier");
            Log.Info("cyberwareessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Non-Retroactive Cyberware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void cyberwaretotalessmultipliernonretroactive(XmlNode bonusNode)
        {
            Log.Info("cyberwaretotalessmultiplier");
            Log.Info("cyberwaretotalessmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareTotalEssMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Prototype Transhuman modifiers.
        public void prototypetranshuman(XmlNode bonusNode)
        {
            Log.Info("prototypetranshuman");
            Log.Info("prototypetranshuman = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");

            _objCharacter.PrototypeTranshuman += Convert.ToDecimal(bonusNode.InnerText);
            CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.PrototypeTranshuman, _strUnique);
        }

        // Check for Friends In High Places modifiers.
        public void friendsinhighplaces(XmlNode bonusNode)
        {
            Log.Info("friendsinhighplaces");
            Log.Info("friendsinhighplaces = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FriendsInHighPlaces,
                _strUnique);
            _objCharacter.FriendsInHighPlaces = true;
        }

        // Check for ExCon modifiers.
        public void excon(XmlNode bonusNode)
        {
            Log.Info("ExCon");
            Log.Info("ExCon = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ExCon, _strUnique);
            _objCharacter.ExCon = true;
        }

        // Check for TrustFund modifiers.
        public void trustfund(XmlNode bonusNode)
        {
            Log.Info("TrustFund");
            Log.Info("TrustFund = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.TrustFund,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
            _objCharacter.TrustFund = ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating);
        }

        // Check for MadeMan modifiers.
        public void mademan(XmlNode bonusNode)
        {
            Log.Info("MadeMan");
            Log.Info("MadeMan = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MadeMan, _strUnique);
            _objCharacter.MadeMan = true;
        }

        // Check for Fame modifiers.
        public void fame(XmlNode bonusNode)
        {
            Log.Info("Fame");
            Log.Info("Fame = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Fame, _strUnique);
            _objCharacter.Fame = true;
        }

        // Check for BornRich modifiers.
        public void bornrich(XmlNode bonusNode)
        {
            Log.Info("BornRich");
            Log.Info("BornRich = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BornRich, _strUnique);
            _objCharacter.BornRich = true;
        }

        // Check for Erased modifiers.
        public void erased(XmlNode bonusNode)
        {
            Log.Info("Erased");
            Log.Info("Erased = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Erased, _strUnique);
            _objCharacter.Erased = true;
        }

        // Check for Erased modifiers.
        public void overclocker(XmlNode bonusNode)
        {
            Log.Info("OverClocker");
            Log.Info("Overclocker = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Overclocker, _strUnique);
            _objCharacter.Overclocker = true;
        }

        // Check for Restricted Gear modifiers.
        public void restrictedgear(XmlNode bonusNode)
        {
            Log.Info("restrictedgear");
            Log.Info("restrictedgear = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedGear, _strUnique);
            _objCharacter.RestrictedGear = true;
        }

        // Check for Improvements that grant bonuses to the maximum amount of Native languages a user can have.
        public void nativelanguagelimit(XmlNode bonusNode)
        {
            Log.Info("nativelanguagelimit");
            Log.Info("nativelanguagelimit = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NativeLanguageLimit,
                _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Ambidextrous modifiers.
        public void ambidextrous(XmlNode bonusNode)
        {
            Log.Info("Ambidextrous");
            Log.Info("Ambidextrous = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Ambidextrous, _strUnique);
            _objCharacter.Ambidextrous = true;
        }

        // Check for Weapon Category DV modifiers.
        public void weaponcategorydv(XmlNode bonusNode)
        {
            //TODO: FIX THIS
            /*
             * I feel like talking a little bit about improvementmanager at
             * this point. It is an intresting class. First of all, it 
             * manages to throw out everything we ever learned about OOP
             * and create a class based on functional programming.
             * 
             * That is true, it is a class, based on manipulating a single
             * list on another class.
             * 
             * But atleast there is a reference to it somewhere right?
             * 
             * No, you create one wherever you need it, meaning there are
             * tens of instances of this class, all operating on the same 
             * list
             * 
             * After that, it is just plain stupid.
             * If you have an list of xmlNodes and some might be the same
             * it checks if a specific node exists (sometimes even by text
             * comparison on .OuterXml) and then runs specific code for 
             * each. If it is there multiple times either of those 2 things
             * happen.
             * 
             * 1. Sad, nothing we can do, guess you have to survive
             * 2. Lets create a foreach in that specific part of the code
             * 
             * Fuck ImprovementManager, kill it with fire, burn the ashes
             * and feed what remains to a dragon that eats unholy 
             * abominations
             */


            Log.Info("weaponcategorydv");
            Log.Info("weaponcategorydv = " + bonusNode.OuterXml);
            XmlNodeList objXmlCategoryList = bonusNode.SelectNodes("weaponcategorydv");
            XmlNode nodWeapon = bonusNode;

            if (nodWeapon["selectskill"] != null)
            {
                bool blnDummy = false;
                SelectedValue = ImprovementManager.DoSelectSkill(nodWeapon["selectskill"], _objCharacter, _intRating, _strFriendlyName, ref blnDummy);

                if (blnDummy)
                {
                    throw new AbortedException();
                }

                Log.Info("strSelected = " + SelectedValue);

                foreach (Power objPower in _objCharacter.Powers)
                {
                    if (objPower.InternalId == SourceName)
                    {
                        objPower.Extra = SelectedValue;
                    }
                }

                Log.Info("Calling CreateImprovement");
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ImprovementManager.ValueToInt(_objCharacter, nodWeapon["bonus"]?.InnerXml, _intRating));
            }
            else if (objXmlCategoryList != null)
            {
                // Run through each of the Skill Groups since there may be more than one affected.
                foreach (XmlNode objXmlCategory in objXmlCategoryList)
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(objXmlCategory["name"]?.InnerText, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ImprovementManager.ValueToInt(_objCharacter, objXmlCategory["bonus"]?.InnerXml, _intRating));
                }
            }
        }

        public void weaponcategorydice(XmlNode bonusNode)
        {
            Log.Info("WeaponCategoryDice");
            Log.Info("WeaponCategoryDice = " + bonusNode.OuterXml);
            using (XmlNodeList xmlSelectCategoryList = bonusNode.SelectNodes("selectcategory"))
            {
                if (xmlSelectCategoryList?.Count > 0)
                {
                    foreach (XmlNode xmlSelectCategory in xmlSelectCategoryList)
                    {
                        // Display the Select Category window and record which Category was selected.
                        List<ListItem> lstGeneralItems = new List<ListItem>();

                        XmlNodeList xmlCategoryList = xmlSelectCategory.SelectNodes("category");
                        if (xmlCategoryList?.Count > 0)
                        {
                            foreach (XmlNode objXmlCategory in xmlCategoryList)
                            {
                                string strInnerText = objXmlCategory.InnerText;
                                lstGeneralItems.Add(new ListItem(strInnerText, LanguageManager.TranslateExtra(strInnerText, GlobalOptions.Language)));
                            }
                        }

                        frmSelectItem frmPickCategory = new frmSelectItem
                        {
                            GeneralItems = lstGeneralItems,
                            Description = !string.IsNullOrEmpty(_strFriendlyName)
                                ? LanguageManager.GetString("String_Improvement_SelectSkillNamed", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
                                : LanguageManager.GetString("Title_SelectWeaponCategory", GlobalOptions.Language)
                        };

                        Log.Info("_strForcedValue = " + ForcedValue);

                        if (ForcedValue.StartsWith("Adept:") || ForcedValue.StartsWith("Magician:"))
                            ForcedValue = string.Empty;

                        if (!string.IsNullOrEmpty(ForcedValue))
                        {
                            frmPickCategory.Opacity = 0;
                            frmPickCategory.ForceItem = ForcedValue;
                        }
                        frmPickCategory.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickCategory.DialogResult == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        Log.Info("strSelected = " + SelectedValue);

                        foreach (Power objPower in _objCharacter.Powers)
                        {
                            if (objPower.InternalId == SourceName)
                            {
                                objPower.Extra = SelectedValue;
                            }
                        }

                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.WeaponCategoryDice, _strUnique, ImprovementManager.ValueToInt(_objCharacter, xmlSelectCategory["value"]?.InnerText, _intRating));
                    }
                }
            }

            using (XmlNodeList xmlCategoryList = bonusNode.SelectNodes("category"))
            {
                if (xmlCategoryList?.Count > 0)
                {
                    foreach (XmlNode xmlCategory in xmlCategoryList)
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(xmlCategory["name"]?.InnerText, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.WeaponCategoryDice, _strUnique, ImprovementManager.ValueToInt(_objCharacter, xmlCategory["value"]?.InnerText, _intRating));
                    }
                }
            }
        }

        // Check for Mentor Spirit bonuses.
        public void selectmentorspirit(XmlNode bonusNode)
        {
            Log.Info("selectmentorspirit");
            Log.Info("selectmentorspirit = " + bonusNode.OuterXml);
            frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter)
            {
                ForcedMentor = ForcedValue
            };
            frmPickMentorSpirit.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            
            XmlNode xmlMentor = XmlManager.Load("mentors.xml").SelectSingleNode("/chummer/mentors/mentor[id = \"" + frmPickMentorSpirit.SelectedMentor + "\"]");
            SelectedValue = xmlMentor?["name"]?.InnerText ?? string.Empty;

            string strHoldValue = SelectedValue;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            string strForce = ForcedValue;
            MentorSpirit objMentor = new MentorSpirit(_objCharacter);
            _objCharacter.MentorSpirits.Add(objMentor);
            objMentor.Create(xmlMentor, Improvement.ImprovementType.MentorSpirit, ForcedValue, frmPickMentorSpirit.Choice1, frmPickMentorSpirit.Choice2, frmPickMentorSpirit.MentorsMask);

            ForcedValue = strForce;
            SelectedValue = strHoldValue;
            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("_strForcedValue = " + ForcedValue);
            CreateImprovement(objMentor.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.MentorSpirit, frmPickMentorSpirit.SelectedMentor);
        }

        // Check for Paragon bonuses.
        public void selectparagon(XmlNode bonusNode)
        {
            Log.Info("selectparagon");
            Log.Info("selectparagon = " + bonusNode.OuterXml);
            frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter, "paragons.xml")
            {
                ForcedMentor = ForcedValue
            };
            frmPickMentorSpirit.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            
            XmlNode xmlMentor = XmlManager.Load("paragons.xml").SelectSingleNode("/chummer/mentors/mentor[id = \"" + frmPickMentorSpirit.SelectedMentor + "\"]");
            SelectedValue = xmlMentor?["name"]?.InnerText ?? string.Empty;

            string strHoldValue = SelectedValue;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            string strForce = ForcedValue;
            MentorSpirit objMentor = new MentorSpirit(_objCharacter);
            _objCharacter.MentorSpirits.Add(objMentor);
            objMentor.Create(xmlMentor, Improvement.ImprovementType.Paragon, ForcedValue, frmPickMentorSpirit.Choice1, frmPickMentorSpirit.Choice2, frmPickMentorSpirit.MentorsMask);

            ForcedValue = strForce;
            SelectedValue = strHoldValue;
            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("_strForcedValue = " + ForcedValue);
            CreateImprovement(objMentor.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Paragon, frmPickMentorSpirit.SelectedMentor);
        }

        // Check for Smartlink bonus.
        public void smartlink(XmlNode bonusNode)
        {
            Log.Info("smartlink");
            Log.Info("smartlink = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Smartlink, "smartlink");
        }

        // Check for Adapsin bonus.
        public void adapsin(XmlNode bonusNode)
        {
            Log.Info("adapsin");
            Log.Info("adapsin = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Adapsin, "adapsin");
        }

        // Check for SoftWeave bonus.
        public void softweave(XmlNode bonusNode)
        {
            Log.Info("softweave");
            Log.Info("softweave = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SoftWeave, "softweave");
        }

        // Check for bonus that removes the ability to take any bioware (e.g. Sensitive System)
        public void disablebioware(XmlNode bonusNode)
        {
            Log.Info("disablebioware");
            Log.Info("disablebioware = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableBioware,
                "disablebioware");
        }

        // Check for bonus that removes the ability to take any cyberware.
        public void disablecyberware(XmlNode bonusNode)
        {
            Log.Info("disablecyberware");
            Log.Info("disablecyberware = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableCyberware,
                "disablecyberware");
        }

        // Check for bonus that removes access to certain bioware grades (e.g. Cyber-Snob)
        public void disablebiowaregrade(XmlNode bonusNode)
        {
            Log.Info("disablebiowaregrade");
            Log.Info("disablebiowaregrade = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strGradeName = bonusNode.InnerText;
            CreateImprovement(strGradeName, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableBiowareGrade,
                "disablebiowaregrade");
        }

        // Check for bonus that removes access to certain cyberware grades (e.g. Regeneration critter power).
        public void disablecyberwaregrade(XmlNode bonusNode)
        {
            Log.Info("disablecyberwaregrade");
            Log.Info("disablecyberwaregrade = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strGradeName = bonusNode.InnerText;
            CreateImprovement(strGradeName, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableCyberwareGrade,
                "disablecyberwaregrade");
        }

        // Check for increases to walk multiplier.
        public void walkmultiplier(XmlNode bonusNode)
        {
            Log.Info("walkmultiplier");
            Log.Info("walkmultiplier = " + bonusNode.OuterXml);

            Log.Info("Calling CreateImprovement");
            string strCategory = bonusNode["category"]?.InnerText;
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.WalkMultiplier, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
                strTemp = bonusNode["percent"]?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.WalkMultiplierPercent, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
            }
        }

        // Check for increases to run multiplier.
        public void runmultiplier(XmlNode bonusNode)
        {
            Log.Info("runmultiplier");
            Log.Info("runmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strCategory = bonusNode["category"]?.InnerText;
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.RunMultiplier, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
                strTemp = bonusNode["percent"]?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.RunMultiplierPercent, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
            }
        }

        // Check for increases to distance sprinted per hit.
        public void sprintbonus(XmlNode bonusNode)
        {
            Log.Info("sprintbonus");
            Log.Info("sprintbonus = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strCategory = bonusNode["category"]?.InnerText;
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.SprintBonus, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
                strTemp = bonusNode["percent"]?.InnerText;
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.SprintBonusPercent, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating));
            }
        }

        // Check for free Positive Qualities.
        public void freepositivequalities(XmlNode bonusNode)
        {
            Log.Info("freepositivequalities");
            Log.Info("freepositivequalities = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreePositiveQualities, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for free Negative Qualities.
        public void freenegativequalities(XmlNode bonusNode)
        {
            Log.Info("freenegativequalities");
            Log.Info("freenegativequalities = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeNegativeQualities, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Select Side.
        public void selectside(XmlNode bonusNode)
        {
            Log.Info("selectside");
            Log.Info("selectside = " + bonusNode.OuterXml);
            frmSelectSide frmPickSide = new frmSelectSide
            {
                Description = LanguageManager.GetString("Label_SelectSide", GlobalOptions.Language).Replace("{0}", _strFriendlyName)
            };
            if (!string.IsNullOrEmpty(ForcedValue))
                frmPickSide.ForceValue(ForcedValue);
            else
                frmPickSide.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSide.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickSide.SelectedSide;
            Log.Info("_strSelectedValue = " + SelectedValue);
        }

        // Check for Free Spirit Power Points.
        public void freespiritpowerpoints(XmlNode bonusNode)
        {
            Log.Info("freespiritpowerpoints");
            Log.Info("freespiritpowerpoints = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Adept Power Points.
        public void adeptpowerpoints(XmlNode bonusNode)
        {
            Log.Info("adeptpowerpoints");
            Log.Info("adeptpowerpoints = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerPoints, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Adept Powers
        public void specificpower(XmlNode bonusNode)
        {
            Log.Info("specificpower");
            Log.Info("specificpower = " + bonusNode.OuterXml);
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (_objCharacter.AdeptEnabled)
            {
                string strSelection = string.Empty;
                ForcedValue = string.Empty;
                
                Log.Info("objXmlSpecificPower = " + bonusNode.OuterXml);

                string strPowerName = bonusNode["name"]?.InnerText;

                if (!string.IsNullOrEmpty(strPowerName))
                {
                    // Check if the character already has this power
                    Log.Info("strSelection = " + strSelection);
                    Power objNewPower = new Power(_objCharacter);
                    XmlNode objXmlPower = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]");
                    if (!objNewPower.Create(objXmlPower, 0, bonusNode["bonusoverride"]))
                        throw new AbortedException();

                    Power objBoostedPower = _objCharacter.Powers.FirstOrDefault(objPower => objPower.Name == objNewPower.Name && objPower.Extra == objNewPower.Extra);
                    if (objBoostedPower == null)
                    {
                        _objCharacter.Powers.Add(objNewPower);
                        objBoostedPower = objNewPower;
                        Log.Info("blnHasPower = false");
                    }
                    else
                        Log.Info("blnHasPower = true");

                    Log.Info("Calling CreateImprovement");
                    int intLevels = 0;
                    if (bonusNode["val"] != null)
                        intLevels = Convert.ToInt32(bonusNode["val"].InnerText);
                    if (!objBoostedPower.LevelsEnabled)
                        intLevels = 1;
                    CreateImprovement(objNewPower.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerFreeLevels, objNewPower.Extra, 0, intLevels);

                    // fix: refund power points, if bonus would make power exceede maximum
                    if (objBoostedPower.TotalMaximumLevels < objBoostedPower.Rating + intLevels)
                    {
                        objBoostedPower.Rating = Math.Max(objBoostedPower.TotalMaximumLevels - intLevels, 0);
                    }

                    objBoostedPower.OnPropertyChanged(nameof(objBoostedPower.TotalRating));
                    objBoostedPower.OnPropertyChanged(nameof(objBoostedPower.FreeLevels));
                }
            }
        }

        // Select a Power.
        public void selectpowers(XmlNode bonusNode)
        {
            Log.Info("selectpower");
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (_objCharacter.AdeptEnabled)
            {
                using (XmlNodeList objXmlPowerList = bonusNode.SelectNodes("selectpower"))
                    if (objXmlPowerList != null)
                        foreach (XmlNode objNode in objXmlPowerList)
                        {
                            Log.Info("_strSelectedValue = " + SelectedValue);
                            Log.Info("_strForcedValue = " + ForcedValue);

                            // Display the Select Power window and record which Power was selected.
                            frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
                            Log.Info("selectpower = " + objNode.OuterXml);

                            int intLevels = Convert.ToInt32(objNode["val"]?.InnerText.Replace("Rating", _intRating.ToString()));
                            frmPickPower.IgnoreLimits = objNode["ignorerating"]?.InnerText == bool.TrueString;
                            string strPointsPerLevel = objNode["pointsperlevel"]?.InnerText;
                            if (!string.IsNullOrEmpty(strPointsPerLevel))
                                frmPickPower.PointsPerLevel = Convert.ToDecimal(strPointsPerLevel, GlobalOptions.InvariantCultureInfo);
                            string strLimit = objNode["limit"]?.InnerText.Replace("Rating", _intRating.ToString());
                            if (!string.IsNullOrEmpty(strLimit))
                                frmPickPower.LimitToRating = Convert.ToInt32(strLimit);
                            string strLimitToPowers = objNode.Attributes?["limittopowers"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLimitToPowers))
                                frmPickPower.LimitToPowers = strLimitToPowers;
                            frmPickPower.ShowDialog();

                            // Make sure the dialogue window was not canceled.
                            if (frmPickPower.DialogResult == DialogResult.Cancel)
                            {
                                throw new AbortedException();
                            }
                            else
                            {
                                XmlNode objXmlPower = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[id = \"" + frmPickPower.SelectedPower + "\"]");

                                // If no, add the power and mark it free or give it free levels
                                Power objNewPower = new Power(_objCharacter);
                                if (!objNewPower.Create(objXmlPower))
                                    throw new AbortedException();

                                SelectedValue = objNewPower.DisplayName;
                                if (_blnConcatSelectedValue)
                                    SourceName += " (" + SelectedValue + ')';

                                List<Power> lstExistingPowersList = _objCharacter.Powers.Where(objPower => objPower.Name == objNewPower.Name && objPower.Extra == objNewPower.Extra).ToList();

                                Log.Info("blnHasPower = " + (lstExistingPowersList.Count > 0).ToString());

                                if (lstExistingPowersList.Count == 0)
                                {
                                    _objCharacter.Powers.Add(objNewPower);
                                }

                                Log.Info("Calling CreateImprovement");
                                CreateImprovement(objNewPower.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerFreePoints, objNewPower.Extra, 0, intLevels);

                                foreach (Power objPower in lstExistingPowersList)
                                {
                                    objPower.ForceEvent(nameof(Power.FreeLevels));
                                }
                            }
                        }
            }
        }

        // Check for Armor Encumbrance Penalty.
        public void armorencumbrancepenalty(XmlNode bonusNode)
        {
            Log.Info("armorencumbrancepenalty");
            Log.Info("armorencumbrancepenalty = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Initiation.
        public void initiation(XmlNode bonusNode)
        {
            Log.Info("initiation");
            Log.Info("initiation = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Initiation, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
            _objCharacter.InitiateGrade += ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating);
        }

        // Check for Submersion.
        public void submersion(XmlNode bonusNode)
        {
            Log.Info("submersion");
            Log.Info("submersion = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Submersion, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
            _objCharacter.SubmersionGrade += ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating);
        }

        public void addmetamagic(XmlNode bonusNode)
        {
            XmlNode objXmlSelectedMetamagic = XmlManager.Load("metamagic.xml").SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + bonusNode.InnerText + "\"]");
            string strForcedValue = bonusNode.Attributes?["select"]?.InnerText ?? string.Empty;

            // Makes sure we aren't over our limits for this particular metamagic from this overall source
            if (objXmlSelectedMetamagic.RequirementsMet(_objCharacter, LanguageManager.GetString("String_Metamagic", GlobalOptions.Language), string.Empty, _strFriendlyName))
            {
                Metamagic objAddMetamagic = new Metamagic(_objCharacter);
                objAddMetamagic.Create(objXmlSelectedMetamagic, Improvement.ImprovementSource.Metamagic, strForcedValue);
                objAddMetamagic.Grade = -1;
                if (objAddMetamagic.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                _objCharacter.Metamagics.Add(objAddMetamagic);
                CreateImprovement(objAddMetamagic.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Metamagic, _strUnique);
            }
            else
            {
                throw new AbortedException();
            }
        }

        public void selectmetamagic(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Load("metamagic.xml");
            string strForceValue = string.Empty;
            XmlNode objXmlSelectedMetamagic;
            XmlNodeList xmlMetamagicList = bonusNode.SelectNodes("metamagic");
            if (xmlMetamagicList?.Count > 0)
            {
                List<ListItem> lstMetamagics = new List<ListItem>();
                frmSelectItem frmPickItem = new frmSelectItem();
                foreach (XmlNode objXmlAddMetamagic in xmlMetamagicList)
                {
                    string strLoopName = objXmlAddMetamagic.InnerText;
                    XmlNode objXmlMetamagic = objXmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + strLoopName + "\"]");
                    // Makes sure we aren't over our limits for this particular metamagic from this overall source
                    if (objXmlMetamagic != null && objXmlAddMetamagic.RequirementsMet(_objCharacter, string.Empty, string.Empty, _strFriendlyName))
                    {
                        lstMetamagics.Add(new ListItem(objXmlMetamagic["id"]?.InnerText, objXmlMetamagic["translate"]?.InnerText ?? strLoopName));
                    }
                }
                if (lstMetamagics.Count == 0)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_Improvement_EmptySelectionListNamed", GlobalOptions.Language).Replace("{0}", SourceName));
                    throw new AbortedException();
                }
                frmPickItem.GeneralItems = lstMetamagics;
                frmPickItem.ShowDialog();
                // Don't do anything else if the form was canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                    throw new AbortedException();
                
                objXmlSelectedMetamagic = objXmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[id = \"" + frmPickItem.SelectedItem + "\"]");
                
                string strSelectedName = objXmlSelectedMetamagic?["name"]?.InnerText;
                if (string.IsNullOrEmpty(strSelectedName))
                    throw new AbortedException();
                foreach (XmlNode objXmlAddMetamagic in xmlMetamagicList)
                {
                    if (strSelectedName == objXmlAddMetamagic.InnerText)
                    {
                        strForceValue = objXmlAddMetamagic.Attributes?["select"]?.InnerText ?? string.Empty;
                        break;
                    }
                }
            }
            else
            {
                frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(_objCharacter, frmSelectMetamagic.Mode.Metamagic);
                frmPickMetamagic.ShowDialog();
                // Don't do anything else if the form was canceled.
                if (frmPickMetamagic.DialogResult == DialogResult.Cancel)
                    throw new AbortedException();

                objXmlSelectedMetamagic = objXmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[id = \"" + frmPickMetamagic.SelectedMetamagic + "\"]");
            }

            Metamagic objAddMetamagic = new Metamagic(_objCharacter);
            objAddMetamagic.Create(objXmlSelectedMetamagic, Improvement.ImprovementSource.Metamagic, strForceValue);
            objAddMetamagic.Grade = -1;
            if (objAddMetamagic.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = objAddMetamagic.DisplayName(GlobalOptions.Language);

            _objCharacter.Metamagics.Add(objAddMetamagic);
            CreateImprovement(objAddMetamagic.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Metamagic, _strUnique);
        }

        public void addecho(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Load("echoes.xml");
            XmlNode objXmlSelectedEcho = objXmlDocument.SelectSingleNode("/chummer/echoes/echo[name = \"" + bonusNode.InnerText + "\"]");
            string strForceValue = bonusNode.Attributes?["select"]?.InnerText ?? string.Empty;

            // Makes sure we aren't over our limits for this particular echo from this overall source
            if (objXmlSelectedEcho.RequirementsMet(_objCharacter, LanguageManager.GetString("String_Echo", GlobalOptions.Language), string.Empty, _strFriendlyName))
            {
                Metamagic objAddEcho = new Metamagic(_objCharacter);
                objAddEcho.Create(objXmlSelectedEcho, Improvement.ImprovementSource.Echo, strForceValue);
                objAddEcho.Grade = -1;
                if (objAddEcho.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                _objCharacter.Metamagics.Add(objAddEcho);
                CreateImprovement(objAddEcho.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Echo, _strUnique);
            }
            else
            {
                throw new AbortedException();
            }
        }

        public void selectecho(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Load("echoes.xml");
            string strForceValue = string.Empty;
            XmlNode xmlSelectedEcho;
            XmlNodeList xmlEchoList = bonusNode.SelectNodes("echo");
            if (xmlEchoList?.Count > 0)
            {
                List<ListItem> lstEchoes = new List<ListItem>();
                frmSelectItem frmPickItem = new frmSelectItem();
                foreach (XmlNode objXmlAddEcho in xmlEchoList)
                {
                    string strLoopName = objXmlAddEcho.InnerText;
                    XmlNode objXmlEcho = objXmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + strLoopName + "\"]");
                    // Makes sure we aren't over our limits for this particular metamagic from this overall source
                    if (objXmlEcho != null && objXmlAddEcho.RequirementsMet(_objCharacter, string.Empty, string.Empty, _strFriendlyName))
                    {
                        lstEchoes.Add(new ListItem(objXmlEcho["id"]?.InnerText, objXmlEcho["translate"]?.InnerText ?? strLoopName));
                    }
                }
                if (lstEchoes.Count == 0)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_Improvement_EmptySelectionListNamed", GlobalOptions.Language).Replace("{0}", SourceName));
                    throw new AbortedException();
                }
                frmPickItem.GeneralItems = lstEchoes;
                frmPickItem.ShowDialog();
                // Don't do anything else if the form was canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                    throw new AbortedException();
                
                xmlSelectedEcho = objXmlDocument.SelectSingleNode("/chummer/echoes/echo[id = \"" + frmPickItem.SelectedItem + "\"]");
                string strSelectedName = xmlSelectedEcho?["name"]?.InnerText;
                if (string.IsNullOrEmpty(strSelectedName))
                    throw new AbortedException();
                foreach (XmlNode objXmlAddEcho in xmlEchoList)
                {
                    if (strSelectedName == objXmlAddEcho.InnerText)
                    {
                        strForceValue = objXmlAddEcho.Attributes?["select"]?.InnerText ?? string.Empty;
                        break;
                    }
                }
            }
            else
            {
                frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(_objCharacter, frmSelectMetamagic.Mode.Echo);
                frmPickMetamagic.ShowDialog();
                // Don't do anything else if the form was canceled.
                if (frmPickMetamagic.DialogResult == DialogResult.Cancel)
                    throw new AbortedException();

                xmlSelectedEcho = objXmlDocument.SelectSingleNode("/chummer/echoes/echo[id = \"" + frmPickMetamagic.SelectedMetamagic + "\"]");
            }
            
            Metamagic objAddEcho = new Metamagic(_objCharacter);
            objAddEcho.Create(xmlSelectedEcho, Improvement.ImprovementSource.Echo, strForceValue);
            objAddEcho.Grade = -1;
            if (objAddEcho.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = objAddEcho.DisplayName(GlobalOptions.Language);

            _objCharacter.Metamagics.Add(objAddEcho);
            CreateImprovement(objAddEcho.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Echo, _strUnique);
        }

        // Check for Skillwires.
        public void skillwire(XmlNode bonusNode)
        {
            Log.Info("skillwire");
            Log.Info("skillwire = " + bonusNode.OuterXml);
            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillwire, strUseUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Hardwires.
        public void hardwires(XmlNode bonusNode)
        {
            Log.Info("hardwire");
            string strNodeOuterXml = bonusNode.OuterXml;
            Log.Info("hardwire = " + strNodeOuterXml);
            Log.Info("Calling CreateImprovement");
            string strForcedValue = ForcedValue;
            Log.Info("_strForcedValue = " + strForcedValue);

            bool blnDummy = false;
            SelectedValue = string.IsNullOrEmpty(strForcedValue) ? ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName, ref blnDummy) : strForcedValue;

            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Hardwire,
                SelectedValue,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Damage Resistance.
        public void damageresistance(XmlNode bonusNode)
        {
            Log.Info("damageresistance");
            Log.Info("damageresistance = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DamageResistance, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Restricted Item Count.
        public void restricteditemcount(XmlNode bonusNode)
        {
            Log.Info("restricteditemcount");
            Log.Info("restricteditemcount = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedItemCount, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Judge Intentions.
        public void judgeintentions(XmlNode bonusNode)
        {
            Log.Info("judgeintentions");
            Log.Info("judgeintentions = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentions, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Judge Intentions (offense only, i.e. doing the judging).
        public void judgeintentionsoffense(XmlNode bonusNode)
        {
            Log.Info("judgeintentionsoffense");
            Log.Info("judgeintentionsoffense = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentionsOffense, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Judge Intentions (defense only, i.e. being judged).
        public void judgeintentionsdefense(XmlNode bonusNode)
        {
            Log.Info("judgeintentionsdefense");
            Log.Info("judgeintentionsdefense = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentionsDefense, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Composure.
        public void composure(XmlNode bonusNode)
        {
            Log.Info("composure");
            Log.Info("composure = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Composure, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Lift and Carry.
        public void liftandcarry(XmlNode bonusNode)
        {
            Log.Info("liftandcarry");
            Log.Info("liftandcarry = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.LiftAndCarry, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Memory.
        public void memory(XmlNode bonusNode)
        {
            Log.Info("memory");
            Log.Info("memory = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Memory, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Fatigue Resist.
        public void fatigueresist(XmlNode bonusNode)
        {
            Log.Info("fatigueresist");
            Log.Info("fatigueresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FatigueResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Radiation Resist.
        public void radiationresist(XmlNode bonusNode)
        {
            Log.Info("radiationresist");
            Log.Info("radiationresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.RadiationResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Sonic Attacks Resist.
        public void sonicresist(XmlNode bonusNode)
        {
            Log.Info("sonicresist");
            Log.Info("sonicresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SonicResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Contact-vector Toxins Resist.
        public void toxincontactresist(XmlNode bonusNode)
        {
            Log.Info("toxincontactresist");
            Log.Info("toxincontactresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinContactResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Ingestion-vector Toxins Resist.
        public void toxiningestionresist(XmlNode bonusNode)
        {
            Log.Info("toxiningestionresist");
            Log.Info("toxiningestionresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinIngestionResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Inhalation-vector Toxins Resist.
        public void toxininhalationresist(XmlNode bonusNode)
        {
            Log.Info("toxininhalationresist");
            Log.Info("toxininhalationresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInhalationResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Injection-vector Toxins Resist.
        public void toxininjectionresist(XmlNode bonusNode)
        {
            Log.Info("toxininjectionresist");
            Log.Info("toxininjectionresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInjectionResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Contact-vector Pathogens Resist.
        public void pathogencontactresist(XmlNode bonusNode)
        {
            Log.Info("pathogencontactresist");
            Log.Info("pathogencontactresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenContactResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Ingestion-vector Pathogens Resist.
        public void pathogeningestionresist(XmlNode bonusNode)
        {
            Log.Info("pathogeningestionresist");
            Log.Info("pathogeningestionresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenIngestionResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Inhalation-vector Pathogens Resist.
        public void pathogeninhalationresist(XmlNode bonusNode)
        {
            Log.Info("pathogeninhalationresist");
            Log.Info("pathogeninhalationresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInhalationResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Injection-vector Pathogens Resist.
        public void pathogeninjectionresist(XmlNode bonusNode)
        {
            Log.Info("pathogeninjectionresist");
            Log.Info("pathogeninjectionresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInjectionResist, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Contact-vector Toxins Immunity.
        public void toxincontactimmune(XmlNode bonusNode)
        {
            Log.Info("toxincontactimmune");
            Log.Info("toxincontactimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinContactImmune, _strUnique);
        }

        // Check for Ingestion-vector Toxins Immunity.
        public void toxiningestionimmune(XmlNode bonusNode)
        {
            Log.Info("toxiningestionimmune");
            Log.Info("toxiningestionimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinIngestionImmune, _strUnique);
        }

        // Check for Inhalation-vector Toxins Immunity.
        public void toxininhalationimmune(XmlNode bonusNode)
        {
            Log.Info("toxininhalationimmune");
            Log.Info("toxininhalationimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInhalationImmune, _strUnique);
        }

        // Check for Injection-vector Toxins Immunity.
        public void toxininjectionimmune(XmlNode bonusNode)
        {
            Log.Info("toxininjectionimmune");
            Log.Info("toxininjectionimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInjectionImmune, _strUnique);
        }

        // Check for Contact-vector Pathogens Immunity.
        public void pathogencontactimmune(XmlNode bonusNode)
        {
            Log.Info("pathogencontactimmune");
            Log.Info("pathogencontactimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenContactImmune, _strUnique);
        }

        // Check for Ingestion-vector Pathogens Immunity.
        public void pathogeningestionimmune(XmlNode bonusNode)
        {
            Log.Info("pathogeningestionimmune");
            Log.Info("pathogeningestionimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenIngestionImmune, _strUnique);
        }

        // Check for Inhalation-vector Pathogens Immunity.
        public void pathogeninhalationimmune(XmlNode bonusNode)
        {
            Log.Info("pathogeninhalationimmune");
            Log.Info("pathogeninhalationimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInhalationImmune, _strUnique);
        }

        // Check for Injection-vector Pathogens Immunity.
        public void pathogeninjectionimmune(XmlNode bonusNode)
        {
            Log.Info("pathogeninjectionimmune");
            Log.Info("pathogeninjectionimmune = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInjectionImmune, _strUnique);
        }

        // Check for Physiological Addiction Resist if you are not addicted.
        public void physiologicaladdictionfirsttime(XmlNode bonusNode)
        {
            Log.Info("physiologicaladdictionfirsttime");
            Log.Info("physiologicaladdictionfirsttime = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysiologicalAddictionFirstTime, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Psychological Addiction if you are not addicted.
        public void psychologicaladdictionfirsttime(XmlNode bonusNode)
        {
            Log.Info("psychologicaladdictionfirsttime");
            Log.Info("psychologicaladdictionfirsttime = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PsychologicalAddictionFirstTime, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Physiological Addiction Resist if you are addicted.
        public void physiologicaladdictionalreadyaddicted(XmlNode bonusNode)
        {
            Log.Info("physiologicaladdictionalreadyaddicted");
            Log.Info("physiologicaladdictionalreadyaddicted = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Psychological Addiction if you are addicted.
        public void psychologicaladdictionalreadyaddicted(XmlNode bonusNode)
        {
            Log.Info("psychologicaladdictionalreadyaddicted");
            Log.Info("psychologicaladdictionalreadyaddicted = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Recovery Dice from Stun CM Damage.
        public void stuncmrecovery(XmlNode bonusNode)
        {
            Log.Info("stuncmrecovery");
            Log.Info("stuncmrecovery = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StunCMRecovery, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Recovery Dice from Physical CM Damage.
        public void physicalcmrecovery(XmlNode bonusNode)
        {
            Log.Info("physicalcmrecovery");
            Log.Info("physicalcmrecovery = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCMRecovery, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Whether Essence is added to Recovery Dice from Stun CM Damage.
        public void addesstostuncmrecovery(XmlNode bonusNode)
        {
            Log.Info("addesstostuncmrecovery");
            Log.Info("addesstostuncmrecovery = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AddESStoStunCMRecovery, _strUnique);
        }

        // Check for Whether Essence is added to Recovery Dice from Physical CM Damage.
        public void addesstophysicalcmrecovery(XmlNode bonusNode)
        {
            Log.Info("addesstophysicalcmrecovery");
            Log.Info("addesstophysicalcmrecovery = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AddESStoPhysicalCMRecovery, _strUnique);
        }

        // Check for Concealability.
        public void concealability(XmlNode bonusNode)
        {
            Log.Info("concealability");
            Log.Info("concealability = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Concealability, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Drain Resistance.
        public void drainresist(XmlNode bonusNode)
        {
            Log.Info("drainresist");
            Log.Info("drainresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainResistance, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Drain Value.
        public void drainvalue(XmlNode bonusNode)
        {
            Log.Info("drainvalue");
            Log.Info("drainvalue = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainValue, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Fading Resistance.
        public void fadingresist(XmlNode bonusNode)
        {
            Log.Info("fadingresist");
            Log.Info("fadingresist = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FadingResistance, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Fading Value.
        public void fadingvalue(XmlNode bonusNode)
        {
            Log.Info("fadingvalue");
            Log.Info("fadingvalue = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FadingValue, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Notoriety.
        public void notoriety(XmlNode bonusNode)
        {
            Log.Info("notoriety");
            Log.Info("notoriety = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Notoriety, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Street Cred bonuses.
        public void streetcred(XmlNode bonusNode)
        {
            Log.Info("streetcred");
            Log.Info("streetcred = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StreetCred, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Street Cred Multiplier bonuses.
        public void streetcredmultiplier(XmlNode bonusNode)
        {
            Log.Info("streetcredmultiplier");
            Log.Info("streetcredmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StreetCredMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Complex Form Limit.
        public void complexformlimit(XmlNode bonusNode)
        {
            Log.Info("complexformlimit");
            Log.Info("complexformlimit = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ComplexFormLimit, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Spell Limit.
        public void spelllimit(XmlNode bonusNode)
        {
            Log.Info("spelllimit");
            Log.Info("spelllimit = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpellLimit, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Free Spells.
        public void freespells(XmlNode bonusNode)
        {
            Log.Info("freespells");
            Log.Info("freespells = " + bonusNode.OuterXml);
            string strSpellTypeLimit = string.Empty;
            XmlAttributeCollection objNodeAttributes = bonusNode.Attributes;
            if (objNodeAttributes != null)
            {
                if (!string.IsNullOrWhiteSpace(objNodeAttributes["limit"]?.InnerText))
                    strSpellTypeLimit = objNodeAttributes["limit"].InnerText;
                if (objNodeAttributes["attribute"] != null)
                {
                    Log.Info("attribute");
                    CharacterAttrib att = _objCharacter.GetAttribute(objNodeAttributes["attribute"].InnerText);
                    if (att != null)
                    {
                        Log.Info(att.Abbrev);
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(att.Abbrev, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsATT, strSpellTypeLimit);
                    }
                }
                else if (objNodeAttributes["skill"] != null)
                {
                    Log.Info("skill");
                    string strKey = objNodeAttributes["skill"].InnerText;
                    Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strKey);
                    Log.Info(strKey);
                    if (objSkill != null)
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(objSkill.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsSkill, strSpellTypeLimit);
                    }
                }
                else
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, _strUnique,
                        ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
                }
            }
            else
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
            }
        }

        // Check for Spell Category bonuses.
        public void spellcategory(XmlNode bonusNode)
        {
            Log.Info("spellcategory");
            Log.Info("spellcategory = " + bonusNode.OuterXml);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategory, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating));
        }

        // Check for Spell Category Drain bonuses.
        public void spellcategorydrain(XmlNode bonusNode)
        {
            Log.Info("spellcategorydrain");
            Log.Info("spellcategorydrain = " + bonusNode.OuterXml);
            string s = bonusNode["category"]?.InnerText ?? SelectedValue;
            if (string.IsNullOrWhiteSpace(s)) throw new AbortedException();
            Log.Info("Calling CreateImprovement");
            CreateImprovement(s, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategoryDrain, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating));
        }

        // Check for Spell Category Damage bonuses.
        public void spellcategorydamage(XmlNode bonusNode)
        {
            Log.Info("spellcategorydamage");
            Log.Info("spellcategorydamage = " + bonusNode.OuterXml);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["category"]?.InnerText, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategoryDamage, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating));
        }

        // Check for Throwing Range bonuses.
        public void throwrange(XmlNode bonusNode)
        {
            Log.Info("throwrange");
            Log.Info("throwrange = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRange, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Throwing STR bonuses.
        public void throwstr(XmlNode bonusNode)
        {
            Log.Info("throwstr");
            Log.Info("throwstr = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowSTR, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Skillsoft access.
        public void skillsoftaccess(XmlNode bonusNode)
        {
            Log.Info("skillsoftaccess");
            Log.Info("skillsoftaccess = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerText;
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillsoftAccess, strUseUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
            _objCharacter.SkillsSection.KnowledgeSkills.AddRange(_objCharacter.SkillsSection.KnowsoftSkills);
        }

        // Check for Quickening Metamagic.
        public void quickeningmetamagic(XmlNode bonusNode)
        {
            Log.Info("quickeningmetamagic");
            Log.Info("quickeningmetamagic = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.QuickeningMetamagic, _strUnique);
        }

        // Check for ignore Stun CM Penalty.
        public void ignorecmpenaltystun(XmlNode bonusNode)
        {
            Log.Info("ignorecmpenaltystun");
            Log.Info("ignorecmpenaltystun = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, _strUnique);
        }

        // Check for ignore Physical CM Penalty.
        public void ignorecmpenaltyphysical(XmlNode bonusNode)
        {
            Log.Info("ignorecmpenaltyphysical");
            Log.Info("ignorecmpenaltyphysical = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, _strUnique);
        }

        // Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
        public void cyborgessence(XmlNode bonusNode)
        {
            Log.Info("cyborgessence");
            Log.Info("cyborgessence = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyborgEssence, _strUnique);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public void essencepenalty(XmlNode bonusNode)
        {
            Log.Info("essencepenalty");
            Log.Info("essencepenalty = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenalty, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public void essencepenaltyt100(XmlNode bonusNode)
        {
            Log.Info("essencepenaltyt100");
            Log.Info("essencepenaltyt100 = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyT100, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting MAG rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public void essencepenaltymagonlyt100(XmlNode bonusNode)
        {
            Log.Info("essencepenaltymagonlyt100");
            Log.Info("essencepenaltymagonlyt100 = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyMAGOnlyT100, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public void essencemax(XmlNode bonusNode)
        {
            Log.Info("essencemax");
            Log.Info("essencemax = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssenceMax, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        // Check for Select Sprite.
        public void selectsprite(XmlNode bonusNode)
        {
            Log.Info("selectsprite");
            Log.Info("selectsprite = " + bonusNode.OuterXml);
            List<ListItem> lstCritters = new List<ListItem>();
            using (XmlNodeList objXmlNodeList = XmlManager.Load("critters.xml").SelectNodes("/chummer/metatypes/metatype[contains(category, \"Sprites\")]"))
                if (objXmlNodeList != null)
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strName = objXmlNode["name"]?.InnerText;
                        lstCritters.Add(new ListItem(strName, objXmlNode["translate"]?.InnerText ?? strName));
                    }

            frmSelectItem frmPickItem = new frmSelectItem
            {
                GeneralItems = lstCritters
            };
            frmPickItem.ShowDialog();

            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedItem;

            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
                Improvement.ImprovementType.AddSprite,
                _strUnique);
        }

        // Check for Black Market Discount.
        public void blackmarketdiscount(XmlNode bonusNode)
        {
            Log.Info("blackmarketdiscount");
            Log.Info("blackmarketdiscount = " + bonusNode.OuterXml);
            XmlNodeList nodeList = XmlManager.Load("options.xml").SelectNodes("/chummer/blackmarketpipelinecategories/category");
            SelectedValue = string.Empty;
            if (nodeList != null)
            {
                List<ListItem> itemList = (from XmlNode objNode in nodeList
                    select new ListItem(objNode.InnerText,
                        objNode.Attributes?["translate"]?.InnerText ?? objNode.InnerText)).ToList();

                frmSelectItem frmPickItem = new frmSelectItem
                {
                    Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                        .Replace("{0}", _strFriendlyName),
                    GeneralItems = itemList
                };

                Log.Info("_strLimitSelection = " + LimitSelection);
                Log.Info("_strForcedValue = " + ForcedValue);

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    frmPickItem.ForceItem = LimitSelection;
                    frmPickItem.Opacity = 0;
                }

                frmPickItem.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                SelectedValue = frmPickItem.SelectedName;
                if (_blnConcatSelectedValue)
                    SourceName += " (" + frmPickItem.SelectedName + ')';

                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + SelectedValue);
            }
            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.BlackMarketDiscount,
                _strUnique);
            _objCharacter.BlackMarketDiscount = true;
        }

        // Select Armor (Mostly used for Custom Fit (Stack)).
        public void selectarmor(XmlNode bonusNode)
        {
            Log.Info("selectarmor");
            Log.Info("selectarmor = " + bonusNode.OuterXml);
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            XmlDocument objXmlDocument = XmlManager.Load("armor.xml");
            XmlNodeList objXmlNodeList;
            if (!string.IsNullOrEmpty(bonusNode.InnerText))
            {
                objXmlNodeList = objXmlDocument.SelectNodes("/chummer/armors/armor[name starts-with " + bonusNode.InnerText + "(" + _objCharacter.Options.BookXPath() +
                                                            ") and category = 'High-Fashion Armor Clothing' and mods[name = 'Custom Fit']]");
            }
            else
            {
                objXmlNodeList =
                    objXmlDocument.SelectNodes("/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() +
                                               ") and category = 'High-Fashion Armor Clothing' and mods[name = 'Custom Fit']]");
            }

            //.SelectNodes("/chummer/skills/skill[not(exotic) and (" + _objCharacter.Options.BookXPath() + ')' + SkillFilter(filter) + "]");

            List<ListItem> lstArmors = new List<ListItem>();
            if (objXmlNodeList != null)
            {
                foreach (XmlNode objNode in objXmlNodeList)
                {
                    string strName = objNode["name"]?.InnerText ?? string.Empty;
                    lstArmors.Add(new ListItem(strName, objNode.Attributes?["translate"]?.InnerText ?? strName));
                }
            }

            if (lstArmors.Count > 0)
            {

                frmSelectItem frmPickItem = new frmSelectItem
                {
                    Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                    .Replace("{0}", _strFriendlyName),
                    GeneralItems = lstArmors
                };

                Log.Info("_strLimitSelection = " + LimitSelection);
                Log.Info("_strForcedValue = " + ForcedValue);

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    frmPickItem.ForceItem = LimitSelection;
                    frmPickItem.Opacity = 0;
                }

                frmPickItem.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickItem.SelectedItem;
                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';
                
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + frmPickItem.SelectedItem);
            }

        }

        // Select a specific piece of Cyberware.
        public void selectcyberware(XmlNode bonusNode)
        {
            Log.Info("selectcyberware");
            Log.Info("selectcyberware = " + bonusNode.OuterXml);
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            string strCategory = bonusNode["category"]?.InnerText;
            XmlNodeList objXmlNodeList = XmlManager.Load("cyberware.xml").SelectNodes(!string.IsNullOrEmpty(strCategory) 
            ? $"/chummer/cyberwares/cyberware[(category = '{strCategory}') and ({_objCharacter.Options.BookXPath()})]" 
            : $"/chummer/cyberwares/cyberware[({_objCharacter.Options.BookXPath()})]");

            List<ListItem> list = new List<ListItem>();
            if (objXmlNodeList != null)
            {
                foreach (XmlNode objNode in objXmlNodeList)
                {
                    string strName = objNode["name"]?.InnerText ?? string.Empty;
                    list.Add(new ListItem(strName, objNode.Attributes?["translate"]?.InnerText ?? strName));
                }
            }

            if (list.Count <= 0)
                throw new AbortedException();
            frmSelectItem frmPickItem = new frmSelectItem
            {
                Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                .Replace("{0}", _strFriendlyName),
                GeneralItems = list
            };

            Log.Info("_strLimitSelection = " + LimitSelection);
            Log.Info("_strForcedValue = " + ForcedValue);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                frmPickItem.ForceItem = LimitSelection;
                frmPickItem.Opacity = 0;
            }

            frmPickItem.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedItem;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            string strSelectedValue = frmPickItem.SelectedItem;
            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SelectedValue = " + strSelectedValue);
        }

        // Select Weapon (custom entry for things like Spare Clip).
        public void selectweapon(XmlNode bonusNode)
        {
            Log.Info("selectweapon");
            Log.Info("selectweapon = " + bonusNode.OuterXml);
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (_objCharacter == null)
            {
                // If the character is null (this is a Vehicle), the user must enter their own string.
                // Display the Select Item window and record the value that was entered.
                frmSelectText frmPickText = new frmSelectText
                {
                    Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                    .Replace("{0}", _strFriendlyName)
                };

                Log.Info("_strLimitSelection = " + LimitSelection);
                Log.Info("_strForcedValue = " + ForcedValue);

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    frmPickText.SelectedValue = LimitSelection;
                    frmPickText.Opacity = 0;
                }

                frmPickText.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickText.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickText.SelectedValue;
                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ')';
                
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + frmPickText.SelectedValue);
            }
            else
            {
                List <ListItem> lstWeapons = new List<ListItem>();
                bool blnIncludeUnarmed = bonusNode.Attributes?["includeunarmed"]?.InnerText == bool.TrueString;
                string strExclude = bonusNode.Attributes?["excludecategory"]?.InnerText ?? string.Empty;
                foreach (Weapon objWeapon in _objCharacter.Weapons.GetAllDescendants(x => x.Children))
                {
                    if ((string.IsNullOrEmpty(strExclude) || objWeapon.WeaponType != strExclude) && (blnIncludeUnarmed || objWeapon.Name != "Unarmed Attack"))
                    {
                        lstWeapons.Add(new ListItem(objWeapon.InternalId, objWeapon.DisplayName(GlobalOptions.Language)));
                    }
                }

                frmSelectItem frmPickItem = new frmSelectItem
                {
                    Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language)
                    .Replace("{0}", _strFriendlyName),
                    GeneralItems = lstWeapons
                };

                Log.Info("_strLimitSelection = " + LimitSelection);
                Log.Info("_strForcedValue = " + ForcedValue);

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    frmPickItem.ForceItem = LimitSelection;
                    frmPickItem.Opacity = 0;
                }

                frmPickItem.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                SelectedValue = frmPickItem.SelectedName;
                if (_blnConcatSelectedValue)
                    SourceName += " (" + frmPickItem.SelectedName + ')';
                
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + SelectedValue);
            }

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique);
        }

        // Select an Optional Power.
        public void optionalpowers(XmlNode bonusNode)
        {
            Log.Info("selectoptionalpower");
            
            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            string strForcePower = !string.IsNullOrEmpty(LimitSelection) ? LimitSelection : string.Empty;

            List<Tuple<string, string>> lstPowerExtraPairs = new List<Tuple<string, string>>();
            using (XmlNodeList xmlOptionalPowerList = bonusNode.SelectNodes("optionalpower"))
                if (xmlOptionalPowerList?.Count > 0)
                    foreach (XmlNode objXmlOptionalPower in xmlOptionalPowerList)
                    {
                        string strPower = objXmlOptionalPower.InnerText;
                        if (string.IsNullOrEmpty(strForcePower) || strForcePower == strPower)
                        {
                            lstPowerExtraPairs.Add(new Tuple<string, string>(strPower, objXmlOptionalPower.Attributes?["select"]?.InnerText));
                        }
                    }

            // Display the Select Critter Power window and record which power was selected.
            frmSelectOptionalPower frmPickPower = new frmSelectOptionalPower(lstPowerExtraPairs.ToArray())
            {
                Description = LanguageManager.GetString("String_Improvement_SelectOptionalPower", GlobalOptions.Language)
            };

            frmPickPower.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickPower.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            
            // Record the improvement.
            XmlNode objXmlPowerNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers/power[name = \"" + frmPickPower.SelectedPower + "\"]");
            CritterPower objPower = new CritterPower(_objCharacter);
            objPower.Create(objXmlPowerNode, 0, frmPickPower.SelectedPowerExtra);
            if (objPower.InternalId.IsEmptyGuid())
                throw new AbortedException();

            objPower.Grade = -1;
            _objCharacter.CritterPowers.Add(objPower);
            CreateImprovement(objPower.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPower, _strUnique);
        }

        public void critterpowers(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Load("critterpowers.xml");
            using (XmlNodeList xmlPowerList = bonusNode.SelectNodes("power"))
                if (xmlPowerList?.Count > 0)
                    foreach (XmlNode objXmlPower in xmlPowerList)
                    {
                        XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                        CritterPower objPower = new CritterPower(_objCharacter);
                        string strForcedValue = string.Empty;
                        int intRating = 0;
                        if (objXmlPower.Attributes?.Count > 0)
                        {
                            string strRating = objXmlPower.Attributes["rating"]?.InnerText;
                            if (!string.IsNullOrEmpty(strRating))
                                intRating = ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);
                            strForcedValue = objXmlPower.Attributes["select"]?.InnerText;
                        }

                        objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                        objPower.Grade = -1;
                        _objCharacter.CritterPowers.Add(objPower);
                        CreateImprovement(objPower.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPower, _strUnique);
                    }
        }

        // Check for Adept Power Points.
        public void critterpowerlevels(XmlNode bonusNode)
        {
            using (XmlNodeList xmlPowerList = bonusNode.SelectNodes("power"))
                if (xmlPowerList?.Count > 0)
                    foreach (XmlNode objXmlPower in xmlPowerList)
                    {
                        Log.Info("critterpowerlevels");
                        Log.Info("critterpowerlevels = " + bonusNode.OuterXml);
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(objXmlPower["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPowerLevel,
                            _strUnique,
                            ImprovementManager.ValueToInt(_objCharacter, objXmlPower["val"]?.InnerText, _intRating));
                    }
        }

        public void publicawareness(XmlNode bonusNode)
        {
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PublicAwareness, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }

        public void dealerconnection(XmlNode bonusNode)
        {
            Log.Info("dealerconnection");
            List<ListItem> lstItems = new List<ListItem>();
            using (XmlNodeList objXmlList = bonusNode.SelectNodes("category"))
                if (objXmlList?.Count > 0)
                    foreach (XmlNode objNode in objXmlList)
                    {
                        if (!_objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DealerConnection && objImprovement.UniqueName == objNode.InnerText))
                        {
                            lstItems.Add(new ListItem(objNode.InnerText, LanguageManager.GetString("String_DealerConnection_" + objNode.InnerText, GlobalOptions.Language)));
                        }
                    }
            if (lstItems.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Improvement_EmptySelectionListNamed", GlobalOptions.Language).Replace("{0}", SourceName));
                throw new AbortedException();
            }
            frmSelectItem frmPickItem = new frmSelectItem
            {
                GeneralItems = lstItems,
                AllowAutoSelect = false
            };
            frmPickItem.ShowDialog();
            // Make sure the dialogue window was not canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = LanguageManager.GetString("String_DealerConnection_" + frmPickItem.SelectedItem, GlobalOptions.Language);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + frmPickItem.SelectedItem);
            Log.Info("SourceName = " + SourceName);

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.DealerConnection, frmPickItem.SelectedItem);
        }

        public void unlockskills(XmlNode bonusNode)
        {
            List<string> options = bonusNode.InnerText.Split(',').Select(x => x.Trim()).ToList();
            string final;
            if (options.Count == 0)
            {
                Utils.BreakIfDebug();
                throw new AbortedException();
            }
            else if (options.Count == 1)
            {
                final = options[0];
            }
            else
            {
                frmSelectItem frmSelect = new frmSelectItem
                {
                    AllowAutoSelect = true,
                    GeneralItems = options.Select(x => new ListItem(x, x)).ToList()
                };

                if (_objCharacter.Pushtext.Count > 0)
                {
                    frmSelect.ForceItem = _objCharacter.Pushtext.Pop();
                }

                if (frmSelect.ShowDialog() == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                final = frmSelect.SelectedItem;
            }

            string strName = bonusNode.Attributes?["name"]?.InnerText;
            if (Enum.TryParse(final, out SkillsSection.FilterOptions skills))
            {
                if (string.IsNullOrEmpty(strName) || !_objCharacter.SkillsSection.SkillsDictionary.ContainsKey(strName))
                {
                    _objCharacter.SkillsSection.AddSkills(skills, strName);
                    CreateImprovement(skills.ToString(), _objImprovementSource, SourceName,  Improvement.ImprovementType.SpecialSkills, _strUnique);
                }
            }
            else
            {
                Utils.BreakIfDebug();
                Log.Info(new object[] { "Failed to parse", "specialskills", bonusNode.OuterXml });
            }
        }

        public void addqualities(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
            using (XmlNodeList xmlQualityList = bonusNode.SelectNodes("addquality"))
                if (xmlQualityList?.Count > 0)
                    foreach (XmlNode objXmlAddQuality in xmlQualityList)
                    {
                        XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlAddQuality.InnerText + "\"]");
                        string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                        
                        string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerText;
                        int intCount = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);
                        bool blnDoesNotContributeToBP = objXmlAddQuality.Attributes?["contributetobp"]?.InnerText.ToLower() != bool.TrueString.ToLower();

                        for (int i = 0; i < intCount; ++i)
                        {
                            // Makes sure we aren't over our limits for this particular quality from this overall source
                            if (objXmlSelectedQuality.RequirementsMet(_objCharacter, LanguageManager.GetString("String_Quality", GlobalOptions.Language), string.Empty, _strFriendlyName))
                            {
                                List<Weapon> lstWeapons = new List<Weapon>();
                                Quality objAddQuality = new Quality(_objCharacter);
                                objAddQuality.Create(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons, strForceValue, _strFriendlyName);

                                if (blnDoesNotContributeToBP)
                                {
                                    objAddQuality.BP = 0;
                                    objAddQuality.ContributeToLimit = false;
                                }

                                _objCharacter.Qualities.Add(objAddQuality);
                                foreach (Weapon objWeapon in lstWeapons)
                                    _objCharacter.Weapons.Add(objWeapon);
                                CreateImprovement(objAddQuality.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecificQuality, _strUnique);
                            }
                            else
                            {
                                throw new AbortedException();
                            }
                        }
                    }
        }

        public void selectquality(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
            List<ListItem> lstQualities = new List<ListItem>();
            using (XmlNodeList xmlQualityList = bonusNode.SelectNodes("quality"))
                if (xmlQualityList?.Count > 0)
                    foreach (XmlNode objXmlAddQuality in xmlQualityList)
                    {
                        // Makes sure we aren't over our limits for this particular quality from this overall source
                        if (objXmlAddQuality.RequirementsMet(_objCharacter, string.Empty, string.Empty, _strFriendlyName))
                        {
                            string strName = objXmlAddQuality.InnerText;
                            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + strName + "\"]");
                            if (objXmlQuality != null)
                            {
                                lstQualities.Add(new ListItem(strName, objXmlQuality["translate"]?.InnerText ?? strName));
                            }
                        }
                    }
            if (lstQualities.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Improvement_EmptySelectionListNamed", GlobalOptions.Language).Replace("{0}", SourceName));
                throw new AbortedException();
            }

            frmSelectItem frmPickItem = new frmSelectItem {GeneralItems = lstQualities};
            frmPickItem.ShowDialog();

            // Don't do anything else if the form was canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
                throw new AbortedException();
            XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmPickItem.SelectedItem + "\"]");
            XmlNode objXmlBonusQuality = bonusNode.SelectSingleNode("quality[\"" + frmPickItem.SelectedItem + "\"]");
            Quality objAddQuality = new Quality(_objCharacter);
            List<Weapon> lstWeapons = new List<Weapon>();

            string strForceValue = objXmlBonusQuality?.Attributes?["select"]?.InnerText;
            objAddQuality.Create(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons, strForceValue, _strFriendlyName);
            if (objXmlBonusQuality?.Attributes?["contributetobp"]?.InnerText != bool.TrueString)
            {
                objAddQuality.BP = 0;
                objAddQuality.ContributeToLimit = false;
            }
            if (bonusNode["discountqualities"] != null)
            {
                lstQualities.Clear();
                lstQualities.Add(new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language)));
                using (XmlNodeList xmlQualityNodeList = bonusNode.SelectNodes("discountqualities/quality"))
                    if (xmlQualityNodeList?.Count > 0)
                        foreach (XmlNode objXmlAddQuality in xmlQualityNodeList)
                        {
                            strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                            string strName = objXmlAddQuality.InnerText;

                            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlAddQuality.InnerText + "\"]");
                            if (objXmlQuality != null)
                            {
                                string strDisplayName = objXmlQuality["translate"]?.InnerText ?? strName;
                                if (!string.IsNullOrWhiteSpace(strForceValue))
                                    strDisplayName += " (" + strForceValue + ')';
                                lstQualities.Add(new ListItem(strName, strDisplayName));
                            }
                        }
                if (lstQualities.Count == 0)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_Improvement_EmptySelectionListNamed", GlobalOptions.Language).Replace("{0}", SourceName));
                    throw new AbortedException();
                }

                frmPickItem = new frmSelectItem {GeneralItems = lstQualities};
                frmPickItem.ShowDialog();

                // Don't do anything else if the form was canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                    throw new AbortedException();
                if (frmPickItem.SelectedItem != "None")
                {
                    objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmPickItem.SelectedItem + "\"]");
                    objXmlBonusQuality = bonusNode.SelectSingleNode("discountqualities/quality[\"" + frmPickItem.SelectedItem + "\"]");
                    int qualityDiscount = Convert.ToInt32(objXmlBonusQuality?.Attributes?["discount"].InnerText);
                    Quality discountQuality = new Quality(_objCharacter)
                    {
                        BP = 0
                    };
                    strForceValue = objXmlBonusQuality?.Attributes?["select"]?.InnerText;
                    discountQuality.Create(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons, strForceValue, _strFriendlyName);
                    _objCharacter.Qualities.Add(discountQuality);
                    objAddQuality.BP = Math.Max(objAddQuality.BP + qualityDiscount, 1);
                    CreateImprovement(discountQuality.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecificQuality, _strUnique);
                }
            }

            _objCharacter.Qualities.Add(objAddQuality);
            foreach (Weapon objWeapon in lstWeapons)
                _objCharacter.Weapons.Add(objWeapon);
            CreateImprovement(objAddQuality.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecificQuality, _strUnique);
        }

        public void addskillspecialization(XmlNode bonusNode)
        {
            string strSkill = bonusNode["skill"]?.InnerText ?? string.Empty;
            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strSkill);
            if (objSkill != null)
            {
                // Create the Improvement.
                Log.Info("Calling CreateImprovement");
                string strSpec = bonusNode["spec"]?.InnerText ?? string.Empty;
                CreateImprovement(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, strSpec);
                SkillSpecialization nspec = new SkillSpecialization(strSpec, true, objSkill);
                objSkill.Specializations.Add(nspec);
            }
        }

        public void addskillspecializationoption(XmlNode bonusNode)
        {
            if (_objCharacter.Options.FreeMartialArtSpecialization && _objImprovementSource == Improvement.ImprovementSource.MartialArt)
            {
                List<Skill> lstSkills = new List<Skill>();
                XmlNodeList xmlSkillsList = bonusNode.SelectNodes("skills");
                if (xmlSkillsList?.Count > 0)
                {
                    foreach (XmlNode objNode in xmlSkillsList)
                    {
                        Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(objNode.InnerText);
                        if (objSkill != null)
                        {
                            lstSkills.Add(objSkill);
                        }
                    }
                }
                else
                {
                    Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(bonusNode["skill"]?.InnerText ?? string.Empty);
                    if (objSkill != null)
                    {
                        lstSkills.Add(objSkill);
                    }
                }

                if (lstSkills.Count > 0)
                {
                    foreach (Skill objSkill in lstSkills)
                    {
                        // Create the Improvement.
                        Log.Info("Calling CreateImprovement");
                        string strSpec = bonusNode["spec"]?.InnerText;
                        CreateImprovement(objSkill.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, strSpec);
                        SkillSpecialization nspec = new SkillSpecialization(strSpec, true, objSkill);
                        objSkill.Specializations.Add(nspec);
                    }
                }
            }
        }

        public void limitspellcategory(XmlNode bonusNode)
        {
            Log.Info("limitspellcategory");
            // Display the Select Spell window.
            frmSelectSpellCategory frmPickSpellCategory = new frmSelectSpellCategory
            {
                Description = LanguageManager.GetString("Title_SelectSpellCategory", GlobalOptions.Language)
            };
            frmPickSpellCategory.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSpellCategory.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = frmPickSpellCategory.SelectedCategory;
            else
                SelectedValue += ", " + frmPickSpellCategory.SelectedCategory;
            if (_blnConcatSelectedValue)
                SourceName += " (" + frmPickSpellCategory.SelectedCategory + ')';

            Log.Info("_strSelectedValue = " + frmPickSpellCategory.SelectedCategory);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickSpellCategory.SelectedCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitSpellCategory, _strUnique);
        }

        public void limitspelldescriptor(XmlNode bonusNode)
        {
            Log.Info("limitspelldescriptor");
            // Display the Select Spell window.
            string strSelected;
            if (!string.IsNullOrWhiteSpace(bonusNode.InnerText))
            {
                strSelected = bonusNode.InnerText;
            }
            else
            {
                frmSelectItem frmPickItem = new frmSelectItem
                {
                    Description = LanguageManager.GetString("Title_SelectSpellDescriptor", GlobalOptions.Language)
                };
                frmPickItem.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickItem.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                strSelected = frmPickItem.SelectedItem;
            }

            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = strSelected;
            else
                SelectedValue += ", " + strSelected;
            if (_blnConcatSelectedValue)
                SourceName += " (" + strSelected + ')';

            Log.Info("_strSelectedValue = " + strSelected);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(strSelected, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitSpellDescriptor, _strUnique);
        }

        public void limitspiritcategory(XmlNode bonusNode)
        {
            Log.Info("limitspiritcategory");
            XmlNodeList xmlLimitedSpirits = bonusNode.SelectNodes("spirit");
            HashSet<string> limit = new HashSet<string>();
            if (xmlLimitedSpirits != null)
            {
                foreach (XmlNode n in xmlLimitedSpirits)
                {
                    limit.Add(n.InnerText);
                }
            }
            List<ListItem> lstSpirits = new List<ListItem>();
            using (XmlNodeList xmlSpirits = XmlManager.Load("traditions.xml").SelectNodes("/chummer/spirits/spirit"))
                if (xmlSpirits?.Count > 0)
                    foreach (XmlNode xmlSpirit in xmlSpirits)
                    {
                        string strSpiritName = xmlSpirit["name"]?.InnerText;
                        if (limit.All(l => strSpiritName == l))
                        {
                            lstSpirits.Add(new ListItem(strSpiritName, xmlSpirit["translate"]?.InnerText ?? strSpiritName));
                        }
                    }

            frmSelectItem frmSelect = new frmSelectItem {GeneralItems = lstSpirits};
            frmSelect.ShowDialog();
            
            if (frmSelect.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = frmSelect.SelectedItem;
            else
                SelectedValue += ", " + frmSelect.SelectedItem;
            if (_blnConcatSelectedValue)
                SourceName += " (" + frmSelect.SelectedItem + ')';
            
            Log.Info("_strSelectedValue = " + frmSelect.SelectedItem);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmSelect.SelectedItem, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitSpiritCategory, _strUnique);
        }
        public void movementreplace(XmlNode bonusNode)
        {
            Log.Info("movementreplace");
            Log.Info("movementreplace = " + bonusNode.OuterXml);

            Improvement.ImprovementType imp = Improvement.ImprovementType.WalkSpeed;
            string strSpeed = bonusNode["speed"]?.InnerText;
            if (!string.IsNullOrEmpty(strSpeed))
            {
                switch (strSpeed.ToLower())
                {
                    case "run":
                        imp = Improvement.ImprovementType.RunSpeed;
                        break;
                    case "sprint":
                        imp = Improvement.ImprovementType.SprintSpeed;
                        break;
                }
            }

            string strNodeValText = bonusNode["val"]?.InnerText;
            Log.Info("Calling CreateImprovement");
            string strCategory = bonusNode["category"]?.InnerText;
            if (!string.IsNullOrEmpty(strCategory))
            {
                CreateImprovement(strCategory, _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, strNodeValText, _intRating));
            }
            else
            {
                CreateImprovement("Ground", _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, strNodeValText, _intRating));
                CreateImprovement("Swim", _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, strNodeValText, _intRating));
                CreateImprovement("Fly", _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToInt(_objCharacter, strNodeValText, _intRating));
            }
        }
        public void addlimb(XmlNode bonusNode)
        {
            Log.Info("addlimb");
            Log.Info("addlimb = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["limbslot"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.AddLimb, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating));
        }

        public void attributekarmacost(XmlNode bonusNode)
        {
            Log.Info("attributekarmacost");
            Log.Info("attributekarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void activeskillkarmacost(XmlNode bonusNode)
        {
            Log.Info("activeskillkarmacost");
            Log.Info("activeskillkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgroupkarmacost(XmlNode bonusNode)
        {
            Log.Info("skillgroupkarmacost");
            Log.Info("skillgroupkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void knowledgeskillkarmacost(XmlNode bonusNode)
        {
            Log.Info("knowledgeskillkarmacost");
            Log.Info("knowledgeskillkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgroupdisable(XmlNode bonusNode)
        {
            Log.Info("skillgroupdisable");
            Log.Info("skillgroupdisable = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupDisable, _strUnique);
        }

        public void skillgroupcategorydisable(XmlNode bonusNode)
        {
            Log.Info("skillgroupcategorydisable");
            Log.Info("skillgroupcategorydisable = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryDisable, _strUnique);
        }

        public void skillgroupcategorykarmacost(XmlNode bonusNode)
        {
            Log.Info("skillgroupcategorykarmacost");
            Log.Info("skillgroupcategorykarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillcategorykarmacost(XmlNode bonusNode)
        {
            Log.Info("skillcategorykarmacost");
            Log.Info("skillcategorykarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillcategoryspecializationkarmacost(XmlNode bonusNode)
        {
            Log.Info("skillcategoryspecializationkarmacost");
            Log.Info("skillcategoryspecializationkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategorySpecializationKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void attributepointcost(XmlNode bonusNode)
        {
            Log.Info("attributepointcost");
            Log.Info("attributepointcost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.AttributePointCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void activeskillpointcost(XmlNode bonusNode)
        {
            Log.Info("activeskillpointcost");
            Log.Info("activeskillpointcost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillPointCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgrouppointcost(XmlNode bonusNode)
        {
            Log.Info("skillgrouppointcost");
            Log.Info("skillgrouppointcost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupPointCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void knowledgeskillpointcost(XmlNode bonusNode)
        {
            Log.Info("knowledgeskillpointcost");
            Log.Info("knowledgeskillpointcost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillPointCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgroupcategorypointcost(XmlNode bonusNode)
        {
            Log.Info("skillgroupcategorypointcost");
            Log.Info("skillgroupcategorypointcost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryPointCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillcategorypointcost(XmlNode bonusNode)
        {
            Log.Info("skillcategorypointcost");
            Log.Info("skillcategorypointcost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryPointCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void newspellkarmacost(XmlNode bonusNode)
        {
            Log.Info("newspellkarmacost");
            Log.Info("newspellkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode.Attributes?["type"]?.InnerText ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewSpellKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void newcomplexformkarmacost(XmlNode bonusNode)
        {
            Log.Info("newcomplexformkarmacost");
            Log.Info("newcomplexformkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewComplexFormKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void newaiprogramkarmacost(XmlNode bonusNode)
        {
            Log.Info("newaiprogramkarmacost");
            Log.Info("newaiprogramkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIProgramKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void newaiadvancedprogramkarmacost(XmlNode bonusNode)
        {
            Log.Info("newaiadvancedprogramkarmacost");
            Log.Info("newaiadvancedprogramkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIAdvancedProgramKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void attributekarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("attributekarmacostmultiplier");
            Log.Info("attributekarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void activeskillkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("activeskillkarmacostmultiplier");
            Log.Info("activeskillkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgroupkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillgroupkarmacostmultiplier");
            Log.Info("skillgroupkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void knowledgeskillkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("knowledgeskillkarmacostmultiplier");
            Log.Info("knowledgeskillkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgroupcategorykarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillgroupcategorykarmacostmultiplier");
            Log.Info("skillgroupcategorykarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillcategorykarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillcategorykarmacostmultiplier");
            Log.Info("skillcategorykarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillcategoryspecializationkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillcategoryspecializationkarmacostmultiplier");
            Log.Info("skillcategoryspecializationkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void attributepointcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("attributepointcostmultiplier");
            Log.Info("attributepointcostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.AttributePointCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void activeskillpointcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillpointcostmultiplier");
            Log.Info("skillpointcosmultipliert = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgrouppointcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillgrouppointcostmultiplier");
            Log.Info("skillgrouppointcostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void knowledgeskillpointcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillpointcostmultiplier");
            Log.Info("skillpointcosmultipliert = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillgroupcategorypointcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillgroupcategorypointcostmultiplier");
            Log.Info("skillgroupcategorypointcostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void skillcategorypointcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("skillcategorypointcostmultiplier");
            Log.Info("skillcategorypointcostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerText, _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerText, _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerText ?? string.Empty);
        }

        public void newspellkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("newspellkarmacostmultiplier");
            Log.Info("newspellkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode.Attributes?["type"]?.InnerText ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewSpellKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void newcomplexformkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("newcomplexformkarmacostmultiplier");
            Log.Info("newcomplexformkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewComplexFormKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void newaiprogramkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("newaiprogramkarmacostmultiplier");
            Log.Info("newaiprogramkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIProgramKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void newaiadvancedprogramkarmacostmultiplier(XmlNode bonusNode)
        {
            Log.Info("newaiadvancedprogramkarmacostmultiplier");
            Log.Info("newaiadvancedprogramkarmacostmultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerText ?? string.Empty);
        }

        public void blockskillspecializations(XmlNode bonusNode)
        {
            Log.Info("blockskillspecializations");
            Log.Info("blockskillspecializations = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillSpecializations, _strUnique);
        }

        public void blockskillcategoryspecializations(XmlNode bonusNode)
        {
            Log.Info("blockskillcategoryspecializations");
            Log.Info("blockskillcategoryspecializations = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillCategorySpecializations, _strUnique);
        }

        // Flat modifier to cost of binding a focus
        public void focusbindingkarmacost(XmlNode bonusNode)
        {
            Log.Info("focusbindingkarmacost");
            Log.Info("focusbindingkarmacost = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.FocusBindingKarmaCost, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, bonusNode["extracontains"]?.InnerText ?? string.Empty);
        }

        // Flat modifier to the number that is multiplied by a focus' rating to get the focus' binding karma cost
        public void focusbindingkarmamultiplier(XmlNode bonusNode)
        {
            Log.Info("focusbindingkarmamultiplier");
            Log.Info("focusbindingkarmamultiplier = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.FocusBindingKarmaMultiplier, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode["val"]?.InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty, false, bonusNode["extracontains"]?.InnerText ?? string.Empty);
        }

        public void magicianswaydiscount(XmlNode bonusNode)
        {
            Log.Info("magicianswaydiscount");
            Log.Info("magicianswaydiscount = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MagiciansWayDiscount, _strUnique);
        }

        public void burnoutsway(XmlNode bonusNode)
        {
            Log.Info("burnoutsway");
            Log.Info("burnoutsway = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BurnoutsWay, _strUnique);
        }

        // Add a specific Cyber/Bioware to the Character.
        public void addware(XmlNode bonusNode)
        {
            Log.Info("addware");

            Log.Info("addware = " + bonusNode.OuterXml);
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ')';

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Adding ware");
            XmlNode node;
            Improvement.ImprovementSource eSource;
            if (bonusNode["type"]?.InnerText == "bioware")
            {
                node = XmlManager.Load("bioware.xml").SelectSingleNode("/chummer/biowares/bioware[name = \"" + bonusNode["name"]?.InnerText + "\"]");
                eSource = Improvement.ImprovementSource.Bioware;
            }
            else
            {
                node = XmlManager.Load("cyberware.xml").SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + bonusNode["name"]?.InnerText + "\"]");
                eSource = Improvement.ImprovementSource.Cyberware;
            }

            if (node == null)
                throw new AbortedException();
            string strRating = bonusNode["rating"]?.InnerText;
            int intRating = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);

            // Create the new piece of ware.
            Cyberware objCyberware = new Cyberware(_objCharacter);
            List<Weapon> lstWeapons = new List<Weapon>();
            List<Vehicle> lstVehicles = new List<Vehicle>();

            Grade objGrade = Cyberware.ConvertToCyberwareGrade(bonusNode["grade"]?.InnerText, _objImprovementSource, _objCharacter);
            objCyberware.Create(node, _objCharacter, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, true, ForcedValue);

            if (objCyberware.InternalId.IsEmptyGuid())
                throw new AbortedException();

            objCyberware.Cost = "0";
            // Create any Weapons that came with this ware.
            foreach (Weapon objWeapon in lstWeapons)
                _objCharacter.Weapons.Add(objWeapon);
            // Create any Vehicles that came with this ware.
            foreach (Vehicle objVehicle in lstVehicles)
                _objCharacter.Vehicles.Add(objVehicle);

            objCyberware.ParentID = SourceName;

            _objCharacter.Cyberware.Add(objCyberware);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(objCyberware.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.FreeWare,
                _strUnique);
        }

        public void weaponaccuracy(XmlNode bonusNode)
        {
            Log.Info("weaponaccuracy");
            Log.Info("weaponaccuracy = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"]?.InnerText ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponAccuracy, _strUnique, ImprovementManager.ValueToInt(_objCharacter, bonusNode["value"]?.InnerText, _intRating));
        }

        public void weaponskillaccuracy(XmlNode bonusNode)
        {
            Log.Info("weaponskillaccuracy");
            Log.Info("weaponskillaccuracy = " + bonusNode.OuterXml);

            string strForcedValue = ForcedValue;
            Log.Info("_strForcedValue = " + strForcedValue);

            XmlNode xmlSelectSkillNode = bonusNode["selectskill"];
            if (xmlSelectSkillNode != null)
            {
                bool blnDummy = false;
                SelectedValue = string.IsNullOrEmpty(strForcedValue) ? ImprovementManager.DoSelectSkill(xmlSelectSkillNode, _objCharacter, _intRating, _strFriendlyName, ref blnDummy) : strForcedValue;
                if (blnDummy)
                    throw new AbortedException();
            }
            else
            {
                SelectedValue = string.IsNullOrEmpty(strForcedValue) ? (bonusNode["name"]?.InnerText ?? string.Empty) : strForcedValue;
            }

            string strVal = bonusNode["value"]?.InnerText;

            if (_blnConcatSelectedValue && !string.IsNullOrEmpty(SelectedValue))
                SourceName += " (" + SelectedValue + ')';
            
            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponSkillAccuracy, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, strVal, _intRating));
        }

        public void metageneticlimit(XmlNode bonusNode)
        {
            Log.Info("spellresistance");
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MetageneticLimit, _strUnique,
                ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerText, _intRating));
        }
        #endregion
    }

    [Serializable]
    public sealed class AbortedException : Exception
    {
    }
}
