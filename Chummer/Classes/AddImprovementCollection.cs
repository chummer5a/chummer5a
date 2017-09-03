using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Skills;
// ReSharper disable InconsistentNaming

namespace Chummer.Classes
{
    class AddImprovementCollection
    {
        private Character _objCharacter;
        private readonly ImprovementManager _manager;

        public AddImprovementCollection(Character character, ImprovementManager manager, Improvement.ImprovementSource objImprovementSource, string sourceName, string strUnique, string forcedValue, string limitSelection, string selectedValue, bool blnConcatSelectedValue, string strFriendlyName, int intRating, Func<string, int, int> valueToInt, Action rollback)
        {
            _objCharacter = character;
            _manager = manager;
            _objImprovementSource = objImprovementSource;
            SourceName = sourceName;
            _strUnique = strUnique;
            ForcedValue = forcedValue;
            LimitSelection = limitSelection;
            SelectedValue = selectedValue;
            _blnConcatSelectedValue = blnConcatSelectedValue;
            _strFriendlyName = strFriendlyName;
            _intRating = intRating;
            ValueToInt = valueToInt;
            Rollback = rollback;
            Commit = manager.Commit;
        }

        public string SourceName;
        public string ForcedValue;
        public string LimitSelection;
        public string SelectedValue;
        public string SelectedTarget = string.Empty;

        private readonly Improvement.ImprovementSource _objImprovementSource;
        private readonly string _strUnique;
        private readonly bool _blnConcatSelectedValue;
        private readonly string _strFriendlyName;
        private readonly int _intRating;


        //Transplanted functions, delegate values to make reflection grabbing all methods less fool proof...
        private readonly Func<string, int, int> ValueToInt;
        private readonly Action Commit;
        private readonly Action Rollback;

        private void CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
            string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
            int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0,
            int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false, string strTarget = "")
        {
            _manager.CreateImprovement(strImprovedName, objImprovementSource,
                strSourceName, objImprovementType, strUnique,
                intValue, intRating, intMinimum, intMaximum, intAugmented,
                intAugmentedMaximum, strExclude, blnAddToRating, strTarget);
        }
        private bool CreateImprovements(Improvement.ImprovementSource objImprovementSource, string strSourceName,
            XmlNode nodBonus, bool blnConcatSelectedValue = false, int intRating = 1, string strFriendlyName = "")
        {
            return _manager.CreateImprovements(objImprovementSource, strSourceName, nodBonus, blnConcatSelectedValue, intRating,
                strFriendlyName);
        }


        #region

        public void qualitylevel(XmlNode bonusNode)
        {
            //List of qualities to work with
            Guid[] all =
            {
                Guid.Parse("9ac85feb-ae1e-4996-8514-3570d411e1d5"), //national
                Guid.Parse("d9479e5c-d44a-45b9-8fb4-d1e08a9487b2"), //dirty criminal
                Guid.Parse("318d2edd-833b-48c5-a3e1-343bf03848a5"), //Limited
                Guid.Parse("e00623e1-54b0-4a91-b234-3c7e141deef4") //Corp
            };

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
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SpellResistance, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        public void enableattribute(XmlNode bonusNode)
        {
            Log.Info("enableattribute");
            if (bonusNode["name"].InnerText == "MAG")
            {
                _objCharacter.MAGEnabled = true;
                Log.Info("Calling CreateImprovement for MAG");
                CreateImprovement("MAG", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                    "enableattribute", 0, 0);
            }
            else if (bonusNode["name"].InnerText == "RES")
            {
                _objCharacter.RESEnabled = true;
                Log.Info("Calling CreateImprovement for RES");
                CreateImprovement("RES", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                    "enableattribute", 0, 0);
            }
            else if (bonusNode["name"].InnerText == "DEP")
            {
                _objCharacter.DEPEnabled = true;
                Log.Info("Calling CreateImprovement for DEP");
                CreateImprovement("DEP", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                    "enableattribute", 0, 0);
            }
        }

        // Add an Attribute Replacement.
        public void replaceattributes(XmlNode bonusNode)
        {
            XmlNodeList objXmlAttributes = bonusNode.SelectNodes("replaceattribute");
            if (objXmlAttributes != null)
                foreach (XmlNode objXmlAttribute in objXmlAttributes)
                {
                    Log.Info("replaceattribute");
                    Log.Info("replaceattribute = " + bonusNode.OuterXml.ToString());
                    // Record the improvement.
                    int intMin = 0;
                    int intMax = 0;

                    // Extract the modifiers.
                    if (objXmlAttribute.InnerXml.Contains("min"))
                        intMin = Convert.ToInt32(objXmlAttribute["min"].InnerText);
                    if (objXmlAttribute.InnerXml.Contains("max"))
                        intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText);
                    string strAttribute = objXmlAttribute["name"].InnerText;

                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.ReplaceAttribute,
                        _strUnique,
                        0, 1, intMin, intMax, 0, 0);
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
            frmSelectItem frmPickItem = new frmSelectItem();
            frmPickItem.Character = _objCharacter;
            if (!string.IsNullOrEmpty(ForcedValue))
                frmPickItem.ForceItem = ForcedValue;
            frmPickItem.AllowAutoSelect = false;
            frmPickItem.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedItem;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Restricted, _strUnique);
        }

        public void cyberseeker(XmlNode bonusNode)
        {
            //Check if valid attrib
            if (new string[] { "BOD", "AGI", "STR", "REA", "LOG", "CHA", "INT", "WIL", "BOX" }.Any(x => x == bonusNode.InnerText))
            {
                CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.Seeker,
                    _strUnique, 0, 0, 0, 0, 0, 0);

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

            frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
            if (!string.IsNullOrEmpty(_strFriendlyName))
                frmPickSkillGroup.Description =
                    LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroupName").Replace("{0}", _strFriendlyName);
            else
                frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroup");

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

            // Display the Select Skill window and record which Skill was selected.
            frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
            if (!string.IsNullOrEmpty(_strFriendlyName))
                frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
                    .Replace("{0}", _strFriendlyName);
            else
                frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");


            Log.Info("selectskill = " + bonusNode.OuterXml);
            if (bonusNode.OuterXml.Contains("skillgroup"))
                frmPickSkill.OnlySkillGroup = bonusNode.Attributes?["skillgroup"].InnerText;
            else if (bonusNode.OuterXml.Contains("skillcategory"))
                frmPickSkill.OnlyCategory = bonusNode.Attributes?["skillcategory"].InnerText;
            else if (bonusNode.OuterXml.Contains("skillcategories"))
                frmPickSkill.LimitToCategories = bonusNode["skillcategories"];
            else if (bonusNode.OuterXml.Contains("excludecategory"))
                frmPickSkill.ExcludeCategory = bonusNode.Attributes?["excludecategory"].InnerText;
            else if (bonusNode.OuterXml.Contains("limittoskill"))
                frmPickSkill.LimitToSkill = bonusNode.Attributes?["limittoskill"].InnerText;
            else if (bonusNode.OuterXml.Contains("limittoattribute"))
                frmPickSkill.LinkedAttribute = bonusNode.Attributes?["limittoattribute"].InnerText;

            bool useKnowledge = Convert.ToBoolean(bonusNode.Attributes?["knowledgeskills"]?.InnerText);
            frmPickSkill.ShowKnowledgeSkills = useKnowledge;

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickSkill.OnlySkill = ForcedValue;
                frmPickSkill.Opacity = 0;
            }
            frmPickSkill.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSkill.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            bool blnAddToRating = false;
            if (bonusNode["applytorating"] != null)
            {
                if (bonusNode["applytorating"].InnerText == "yes")
                    blnAddToRating = true;
            }

            SelectedValue = frmPickSkill.SelectedSkill;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Find the selected Skill.
            if (useKnowledge)
            {
                if (_objCharacter.SkillsSection.KnowledgeSkills.Any(k => k.Name == frmPickSkill.SelectedSkill))
                {
                    foreach (KnowledgeSkill k in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (k.Name != frmPickSkill.SelectedSkill) continue;
                        // We've found the selected Skill.
                        if (bonusNode.InnerXml.Contains("val"))
                        {
                            Log.Info("Calling CreateImprovement");
                            CreateImprovement(k.Name, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                ValueToInt(bonusNode["val"].InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty,
                                blnAddToRating);
                        }

                        if (!bonusNode.InnerXml.Contains("max")) continue;
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(k.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            0, 1, 0, ValueToInt(bonusNode["max"].InnerText, _intRating), 0, 0, string.Empty,
                            blnAddToRating);
                    }
                }
                else
                {
                    KnowledgeSkill k = new KnowledgeSkill(_objCharacter) {WriteableName = frmPickSkill.SelectedSkill};
                    _objCharacter.SkillsSection.KnowledgeSkills.Add(k);
                    // We've found the selected Skill.
                    if (bonusNode.InnerXml.Contains("val"))
                    {
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(k.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            ValueToInt(bonusNode["val"].InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty,
                            blnAddToRating);
                    }

                    if (!bonusNode.InnerXml.Contains("max")) return;
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(k.Name, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Skill,
                        _strUnique,
                        0, 1, 0, ValueToInt(bonusNode["max"].InnerText, _intRating), 0, 0, string.Empty,
                        blnAddToRating);
                }
            }
            else
                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (frmPickSkill.SelectedSkill.Contains("Exotic Melee Weapon") ||
                        frmPickSkill.SelectedSkill.Contains("Exotic Ranged Weapon") ||
                        frmPickSkill.SelectedSkill.Contains("Pilot Exotic Vehicle"))
                    {
                        if ($"{objSkill.Name} ({objSkill.Specialization})" != frmPickSkill.SelectedSkill) continue;
                        // We've found the selected Skill.
                        if (bonusNode.InnerXml.Contains("val"))
                        {
                            Log.Info("Calling CreateImprovement");
                            CreateImprovement($"{objSkill.Name} ({objSkill.Specialization})", _objImprovementSource,
                                SourceName,
                                Improvement.ImprovementType.Skill, _strUnique,
                                ValueToInt(bonusNode["val"].InnerText, _intRating), 1,
                                0, 0, 0, 0, string.Empty, blnAddToRating);
                        }

                        if (!bonusNode.InnerXml.Contains("max")) continue;
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement($"{objSkill.Name} ({objSkill.Specialization})", _objImprovementSource,
                            SourceName,
                            Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
                            ValueToInt(bonusNode["max"].InnerText, _intRating), 0, 0, string.Empty, blnAddToRating);
                    }
                    else
                    {
                        if (objSkill.Name != frmPickSkill.SelectedSkill) continue;
                        // We've found the selected Skill.
                        if (bonusNode.InnerXml.Contains("val"))
                        {
                            Log.Info("Calling CreateImprovement");
                            CreateImprovement(objSkill.Name, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                ValueToInt(bonusNode["val"].InnerText, _intRating), 1, 0, 0, 0, 0, string.Empty,
                                blnAddToRating);
                        }

                        if (!bonusNode.InnerXml.Contains("max")) continue;
                        Log.Info("Calling CreateImprovement");
                        CreateImprovement(objSkill.Name, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Skill,
                            _strUnique,
                            0, 1, 0, ValueToInt(bonusNode["max"].InnerText, _intRating), 0, 0, string.Empty,
                            blnAddToRating);
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

            frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
            if (!string.IsNullOrEmpty(_strFriendlyName))
                frmPickSkillGroup.Description =
                    LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroupName").Replace("{0}", _strFriendlyName);
            else
                frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroup");

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

            bool blnAddToRating = false;
            if (bonusNode["applytorating"] != null)
            {
                if (bonusNode["applytorating"].InnerText == "yes")
                    blnAddToRating = true;
            }

            SelectedValue = frmPickSkillGroup.SelectedSkillGroup;

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            if (bonusNode.SelectSingleNode("bonus") != null)
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, ValueToInt(bonusNode["bonus"].InnerText, _intRating), 1, 0, 0, 0, 0, strExclude,
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
            foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("selectattribute"))
            {
                Log.Info("selectattribute");
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickAttribute.Description =
                        LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
                else
                    frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

                // Add MAG and/or RES to the list of Attributes if they are enabled on the form.
                if (_objCharacter.MAGEnabled)
                    frmPickAttribute.AddMAG();
                if (_objCharacter.RESEnabled)
                    frmPickAttribute.AddRES();
                if (_objCharacter.DEPEnabled)
                    frmPickAttribute.AddDEP();

                Log.Info("selectattribute = " + bonusNode.OuterXml.ToString());

                if (objXmlAttribute.InnerXml.Contains("<attribute>"))
                {
                    List<string> strValue = new List<string>();
                    foreach (XmlNode objSubNode in objXmlAttribute.SelectNodes("attribute"))
                        strValue.Add(objSubNode.InnerText);
                    frmPickAttribute.LimitToList(strValue);
                }

                if (bonusNode.InnerXml.Contains("<excludeattribute>"))
                {
                    List<string> strValue = new List<string>();
                    foreach (XmlNode objSubNode in objXmlAttribute.SelectNodes("excludeattribute"))
                        strValue.Add(objSubNode.InnerText);
                    frmPickAttribute.RemoveFromList(strValue);
                }

                // Check to see if there is only one possible selection because of _strLimitSelection.
                if (!string.IsNullOrEmpty(ForcedValue))
                    LimitSelection = ForcedValue;

                Log.Info("_strForcedValue = " + ForcedValue);
                Log.Info("_strLimitSelection = " + LimitSelection);

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    frmPickAttribute.SingleAttribute(LimitSelection);
                    frmPickAttribute.Opacity = 0;
                }

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                /*SelectedValue = frmPickAttribute.SelectedAttribute;
                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ")";
                */
                Log.Info("_strSelectedValue = " + frmPickAttribute.SelectedAttribute);
                Log.Info("SourceName = " + SourceName);

                // Record the improvement.
                int intMin = 0;
                int intAug = 0;
                int intMax = 0;
                int intAugMax = 0;

                // Extract the modifiers.
                if (objXmlAttribute.InnerXml.Contains("min"))
                    intMin = Convert.ToInt32(objXmlAttribute["min"].InnerText);
                if (objXmlAttribute.InnerXml.Contains("val"))
                    intAug = Convert.ToInt32(objXmlAttribute["val"].InnerText);
                if (objXmlAttribute.InnerXml.Contains("max"))
                    intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText);
                if (objXmlAttribute.InnerXml.Contains("aug"))
                    intAugMax = Convert.ToInt32(objXmlAttribute["aug"].InnerText);

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
            // Display the Select Attribute window and record which Skill was selected.
            frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
            if (!string.IsNullOrEmpty(_strFriendlyName))
                frmPickAttribute.Description =
                    LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
            else
                frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

            // Add MAG and/or RES to the list of Attributes if they are enabled on the form.
            if (_objCharacter.MAGEnabled)
                frmPickAttribute.AddMAG();
            if (_objCharacter.RESEnabled)
                frmPickAttribute.AddRES();
            if (_objCharacter.DEPEnabled)
                frmPickAttribute.AddDEP();

            Log.Info("selectattribute = " + bonusNode.OuterXml.ToString());

            if (bonusNode.InnerXml.Contains("<attribute>"))
            {
                List<string> strValue = new List<string>();
                foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
                    strValue.Add(objXmlAttribute.InnerText);
                frmPickAttribute.LimitToList(strValue);
            }

            if (bonusNode.InnerXml.Contains("<excludeattribute>"))
            {
                List<string> strValue = new List<string>();
                foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("excludeattribute"))
                    strValue.Add(objXmlAttribute.InnerText);
                frmPickAttribute.RemoveFromList(strValue);
            }

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                frmPickAttribute.SingleAttribute(LimitSelection);
                frmPickAttribute.Opacity = 0;
            }

            frmPickAttribute.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickAttribute.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickAttribute.SelectedAttribute;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Record the improvement.
            int intMin = 0;
            int intAug = 0;
            int intMax = 0;
            int intAugMax = 0;

            // Extract the modifiers.
            if (bonusNode.InnerXml.Contains("min"))
                intMin = ValueToInt(bonusNode["min"].InnerXml, _intRating);
            if (bonusNode.InnerXml.Contains("val"))
                intAug = ValueToInt(bonusNode["val"].InnerXml, _intRating);
            if (bonusNode.InnerXml.Contains("max"))
                intMax = ValueToInt(bonusNode["max"].InnerXml, _intRating);
            if (bonusNode.InnerXml.Contains("aug"))
                intAugMax = ValueToInt(bonusNode["aug"].InnerXml, _intRating);

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
            // Display the Select Limit window and record which Limit was selected.
            frmSelectLimit frmPickLimit = new frmSelectLimit();
            if (!string.IsNullOrEmpty(_strFriendlyName))
                frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
                    .Replace("{0}", _strFriendlyName);
            else
                frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

            Log.Info("selectlimit = " + bonusNode.OuterXml);

            if (bonusNode.InnerXml.Contains("<limit>"))
            {
                List<string> strValue = new List<string>();
                foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("limit"))
                    strValue.Add(objXmlAttribute.InnerText);
                frmPickLimit.LimitToList(strValue);
            }

            if (bonusNode.InnerXml.Contains("<excludelimit>"))
            {
                List<string> strValue = new List<string>();
                foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("excludelimit"))
                    strValue.Add(objXmlAttribute.InnerText);
                frmPickLimit.RemoveFromList(strValue);
            }

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                frmPickLimit.SingleLimit(LimitSelection);
                frmPickLimit.Opacity = 0;
            }

            frmPickLimit.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickLimit.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickLimit.SelectedLimit;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            // Record the improvement.
            int intMin = 0;
            int intAug = 0;
            int intMax = 0;
            int intAugMax = 0;

            // Extract the modifiers.
            if (bonusNode.InnerXml.Contains("min"))
                intMin = ValueToInt(bonusNode["min"].InnerXml, _intRating);
            if (bonusNode.InnerXml.Contains("val"))
                intAug = ValueToInt(bonusNode["val"].InnerXml, _intRating);
            if (bonusNode.InnerXml.Contains("max"))
                intMax = ValueToInt(bonusNode["max"].InnerXml, _intRating);
            if (bonusNode.InnerXml.Contains("aug"))
                intAugMax = ValueToInt(bonusNode["aug"].InnerXml, _intRating);

            string strLimit = frmPickLimit.SelectedLimit;

            if (bonusNode["affectbase"] != null)
                strLimit += "Base";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            LimitModifier objLimitMod = new LimitModifier(_objCharacter);
            // string strBonus = bonusNode["value"].InnerText;
            int intBonus = intAug;
            string strName = _strFriendlyName;
            TreeNode nodTemp = new TreeNode();
            Improvement.ImprovementType objType = Improvement.ImprovementType.PhysicalLimit;

            switch (strLimit)
            {
                case "Mental":
                    {
                        objType = Improvement.ImprovementType.MentalLimit;
                        break;
                    }
                case "Social":
                    {
                        objType = Improvement.ImprovementType.SocialLimit;
                        break;
                    }
                default:
                    {
                        objType = Improvement.ImprovementType.PhysicalLimit;
                        break;
                    }
            }

            Log.Info("Calling CreateImprovement");
            CreateImprovement(strLimit, _objImprovementSource, SourceName, objType, _strFriendlyName, intBonus, 0, intMin,
                intMax,
                intAug, intAugMax);
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public void swapskillattribute(XmlNode bonusNode)
        {
            Log.Info("swapskillattribute");
            List<string> strLimitValue = new List<string>();
            if (bonusNode.InnerXml.Contains("<attribute>"))
            {
                foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
                    strLimitValue.Add(objXmlAttribute.InnerText);
            }
            if (strLimitValue.Count == 1)
                LimitSelection = strLimitValue.First();

            Log.Info("swapskillattribute = " + bonusNode.OuterXml.ToString());

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
                frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickAttribute.Description =
                        LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
                else
                    frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

                if (strLimitValue.Count > 0)
                    frmPickAttribute.LimitToList(strLimitValue);

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickAttribute.SelectedAttribute;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ")";
            }

            strLimitValue.Clear();
            if (bonusNode.InnerXml.Contains("<limittoskill>"))
            {
                SelectedTarget = bonusNode.SelectSingleNode("limittoskill").InnerText;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
                        .Replace("{0}", _strFriendlyName);
                else
                    frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

                if (bonusNode.OuterXml.Contains("<skillgroup>"))
                    frmPickSkill.OnlySkillGroup = bonusNode.SelectSingleNode("skillgroup").InnerText;
                else if (bonusNode.OuterXml.Contains("<skillcategory>"))
                    frmPickSkill.OnlyCategory = bonusNode.SelectSingleNode("skillcategory").InnerText;
                else if (bonusNode.OuterXml.Contains("<excludecategory>"))
                    frmPickSkill.ExcludeCategory = bonusNode.SelectSingleNode("excludecategory").InnerText;
                else if (bonusNode.OuterXml.Contains("<limittoattribute>"))
                    frmPickSkill.LinkedAttribute = bonusNode.SelectSingleNode("limittoattribute").InnerText;

                frmPickSkill.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedTarget = frmPickSkill.SelectedSkill;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedTarget + ")";
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
            List<string> strLimitValue = new List<string>();
            if (bonusNode.InnerXml.Contains("<attribute>"))
            {
                foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
                    strLimitValue.Add(objXmlAttribute.InnerText);
            }
            if (strLimitValue.Count == 1)
                LimitSelection = strLimitValue.First();

            Log.Info("swapskillspecattribute = " + bonusNode.OuterXml.ToString());

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
                frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickAttribute.Description =
                        LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
                else
                    frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

                if (strLimitValue.Count > 0)
                    frmPickAttribute.LimitToList(strLimitValue);

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickAttribute.SelectedAttribute;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedValue + ")";
            }

            strLimitValue.Clear();
            if (bonusNode.InnerXml.Contains("<limittoskill>"))
            {
                SelectedTarget = bonusNode.SelectSingleNode("limittoskill").InnerText;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
                        .Replace("{0}", _strFriendlyName);
                else
                    frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

                if (bonusNode.OuterXml.Contains("<skillgroup>"))
                    frmPickSkill.OnlySkillGroup = bonusNode.SelectSingleNode("skillgroup").InnerText;
                else if (bonusNode.OuterXml.Contains("<skillcategory>"))
                    frmPickSkill.OnlyCategory = bonusNode.SelectSingleNode("skillcategory").InnerText;
                else if (bonusNode.OuterXml.Contains("<excludecategory>"))
                    frmPickSkill.ExcludeCategory = bonusNode.SelectSingleNode("excludecategory").InnerText;
                else if (bonusNode.OuterXml.Contains("<limittoattribute>"))
                    frmPickSkill.LinkedAttribute = bonusNode.SelectSingleNode("limittoattribute").InnerText;

                frmPickSkill.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedTarget = frmPickSkill.SelectedSkill;

                if (_blnConcatSelectedValue)
                    SourceName += " (" + SelectedTarget + ")";
            }

            // TODO: Allow selection of specializations through frmSelectSkillSpec
            string strSpec = string.Empty;
            try
            {
                strSpec = bonusNode["spec"].InnerText;
            }
            catch
            {
            }

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

            if (bonusNode.Attributes?["category"] != null)
                frmPickSpell.LimitCategory = bonusNode.Attributes["category"].InnerText;

            Log.Info("selectspell = " + bonusNode.OuterXml.ToString());
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickSpell.ForceSpellName = ForcedValue;
                frmPickSpell.Opacity = 0;
            }

            if (bonusNode.Attributes["ignorerequirements"] != null)
            {
                frmPickSpell.IgnoreRequirements = Convert.ToBoolean(bonusNode.Attributes["ignorerequirements"].InnerText);
            }

            frmPickSpell.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSpell.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            // Open the Spells XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Instance.Load("spells.xml");

            XmlNode objXmlSpell = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickSpell.SelectedSpell + "\"]");
            SelectedValue = objXmlSpell["name"].InnerText;

            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text,
                _strUnique);
        }

        // Add a specific Spell to the Character.
        public void addspell(XmlNode bonusNode)
        {
            Log.Info("addspell");

            Log.Info("addspell = " + bonusNode.OuterXml.ToString());
            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            XmlDocument objXmlSpellDocument = XmlManager.Instance.Load("spells.xml");

            XmlNode node = objXmlSpellDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + bonusNode.InnerText + "\"]");

            if (node == null) return;
            // Check for SelectText.
            string strExtra = string.Empty;
            if (node["bonus"]?["selecttext"] != null)
            {
                
                frmSelectText frmPickText = new frmSelectText();
                frmPickText.Description =
                    LanguageManager.Instance.GetString("String_Improvement_SelectText")
                        .Replace("{0}", node["translate"]?.InnerText ?? node["name"].InnerText);
                frmPickText.ShowDialog();
                strExtra = frmPickText.SelectedValue;
            }

            Spell spell = new Spell(_objCharacter);
            spell.Create(node, _objCharacter, new TreeNode(), strExtra);
            if (spell.InternalId == Guid.Empty.ToString())
                return;

            _objCharacter.Spells.Add(spell);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(spell.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Spell,
                _strUnique);
        }

        // Select an AI program.
        public void selectaiprogram(XmlNode bonusNode)
        {
            Log.Info("selectaiprogram");
            // Display the Select Spell window.
            frmSelectAIProgram frmPickProgram = new frmSelectAIProgram(_objCharacter);

            Log.Info("selectaiprogram = " + bonusNode.OuterXml);

            Log.Info("_strForcedValue = " + ForcedValue);

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickProgram.SelectedProgram = ForcedValue;
            }
            else
            {
                frmPickProgram.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickProgram.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
            }

            SelectedValue = frmPickProgram.SelectedProgram;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            XmlDocument objXmlDocument = XmlManager.Instance.Load("programs.xml");

            XmlNode objXmlProgram = objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + frmPickProgram.SelectedProgram + "\"]");

            if (objXmlProgram != null)
            {
                // Check for SelectText.
                string strExtra = string.Empty;
                if (objXmlProgram["bonus"] != null)
                {
                    if (objXmlProgram["bonus"]["selecttext"] != null)
                    {
                        frmSelectText frmPickText = new frmSelectText();
                        frmPickText.Description =
                            LanguageManager.Instance.GetString("String_Improvement_SelectText")
                                .Replace("{0}", frmPickProgram.SelectedProgram);
                        frmPickText.ShowDialog();
                        strExtra = frmPickText.SelectedValue;
                    }
                }

                TreeNode objNode = new TreeNode();
                AIProgram objProgram = new AIProgram(_objCharacter);
                objProgram.Create(objXmlProgram, _objCharacter, objNode,
                    objXmlProgram["category"]?.InnerText == "Advanced Programs", strExtra, false);
                if (objProgram.InternalId == Guid.Empty.ToString())
                    return;

                _objCharacter.AIPrograms.Add(objProgram);

                Log.Info("Calling CreateImprovement");
                CreateImprovement(objProgram.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.AIProgram,
                    _strUnique);
            }
        }

        // Select an AI program.
        public void selectinherentaiprogram(XmlNode bonusNode)
        {
            Log.Info("selectaiprogram");
            // Display the Select Spell window.
            frmSelectAIProgram frmPickProgram = new frmSelectAIProgram(_objCharacter, false, true);

            Log.Info("selectaiprogram = " + bonusNode.OuterXml.ToString());

            Log.Info("_strForcedValue = " + ForcedValue);

            if (!string.IsNullOrEmpty(ForcedValue))
            {
                frmPickProgram.SelectedProgram = ForcedValue;
            }
            else
            {
                frmPickProgram.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickProgram.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
            }

            SelectedValue = frmPickProgram.SelectedProgram;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            XmlDocument objXmlDocument = XmlManager.Instance.Load("programs.xml");

            XmlNode objXmlProgram = objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + frmPickProgram.SelectedProgram + "\"]");

            if (objXmlProgram != null)
            {
                // Check for SelectText.
                string strExtra = string.Empty;
                if (objXmlProgram["bonus"] != null)
                {
                    if (objXmlProgram["bonus"]["selecttext"] != null)
                    {
                        frmSelectText frmPickText = new frmSelectText();
                        frmPickText.Description =
                            LanguageManager.Instance.GetString("String_Improvement_SelectText")
                                .Replace("{0}", frmPickProgram.SelectedProgram);
                        frmPickText.ShowDialog();
                        strExtra = frmPickText.SelectedValue;
                    }
                }

                TreeNode objNode = new TreeNode();
                AIProgram objProgram = new AIProgram(_objCharacter);
                objProgram.Create(objXmlProgram, _objCharacter, objNode,
                    objXmlProgram["category"]?.InnerText == "Advanced Programs", strExtra, false);
                if (objProgram.InternalId == Guid.Empty.ToString())
                    return;

                _objCharacter.AIPrograms.Add(objProgram);

                Log.Info("Calling CreateImprovement");
                CreateImprovement(objProgram.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.AIProgram,
                    _strUnique);
            }
        }

        // Select a Contact
        public void selectcontact(XmlNode bonusNode)
        {
            Log.Info("selectcontact");
            XmlNode nodSelect = bonusNode;

            frmSelectItem frmSelect = new frmSelectItem();

            String strMode = nodSelect["type"]?.InnerText ?? "all";

            List<Contact> selectedContactsList;
            if (strMode == "all")
            {
                selectedContactsList = new List<Contact>(_objCharacter.Contacts);
            }
            else if (strMode == "group" || strMode == "nongroup")
            {
                bool blnGroup = strMode == "group";


                //Select any contact where IsGroup equals blnGroup
                //and add to a list
                selectedContactsList =
                    new List<Contact>(from contact in _objCharacter.Contacts
                                      where contact.IsGroup == blnGroup
                                      select contact);
            }
            else
            {
                throw new AbortedException();
            }

            if (selectedContactsList.Count == 0)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_NoContactFound"),
                    LanguageManager.Instance.GetString("MessageTitle_NoContactFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new AbortedException();
            }

            int count = 0;
            //Black magic LINQ to cast content of list to another type
            List<ListItem> contacts = new List<ListItem>(from x in selectedContactsList
                                                         select new ListItem() { Name = x.Name, Value = (count++).ToString() });

            String strPrice = nodSelect?.InnerText ?? string.Empty;

            frmSelect.GeneralItems = contacts;
            frmSelect.ShowDialog();

            int index = int.Parse(frmSelect.SelectedItem);
            if (frmSelect.DialogResult != DialogResult.Cancel)
            {
                Contact selectedContact = selectedContactsList[index];

                if (nodSelect["mademan"] != null)
                {
                    selectedContact.MadeMan = true;
                    CreateImprovement(selectedContact.GUID, Improvement.ImprovementSource.Quality, SourceName,
                        Improvement.ImprovementType.ContactMadeMan, selectedContact.GUID);
                }

                if (String.IsNullOrWhiteSpace(SelectedValue))
                {
                    SelectedValue = selectedContact.Name;
                }
                else
                {
                    SelectedValue += (", " + selectedContact.Name);
                }
            }
            else
            {
                throw new AbortedException();
            }
        }

        public void addcontact(XmlNode bonusNode)
        {
            Log.Info("addcontact");

            int loyalty = 1;
            int connection = 1;

            bonusNode.TryGetInt32FieldQuickly("loyalty", ref loyalty);
            bonusNode.TryGetInt32FieldQuickly("connection", ref connection);
            bool group = bonusNode["group"] != null;
            bool free = bonusNode["free"] != null;
            bool canwrite = bonusNode["canwrite"] != null;

            Contact contact = new Contact(_objCharacter);
            contact.Free = free;
            contact.IsGroup = group;
            contact.Loyalty = loyalty;
            contact.Connection = connection;
            contact.ReadOnly = !canwrite;
            _objCharacter.Contacts.Add(contact);

            CreateImprovement(contact.GUID, Improvement.ImprovementSource.Quality, SourceName,
                Improvement.ImprovementType.AddContact, contact.GUID);
        }

        // Affect a Specific CharacterAttribute.
        public void specificattribute(XmlNode bonusNode)
        {
            Log.Info("specificattribute");

            if (bonusNode["name"].InnerText != "ESS")
            {
                // Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
                // Record the improvement.
                int intMin = 0;
                int intAug = 0;
                int intMax = 0;
                int intAugMax = 0;

                // Extract the modifiers.
                if (bonusNode.InnerXml.Contains("min"))
                    intMin = ValueToInt(bonusNode["min"].InnerXml, _intRating);
                if (bonusNode.InnerXml.Contains("val"))
                    intAug = ValueToInt(bonusNode["val"].InnerXml, _intRating);
                if (bonusNode.InnerXml.Contains("max"))
                {
                    if (bonusNode["max"].InnerText.Contains("-natural"))
                    {
                        intMax = Convert.ToInt32(bonusNode["max"].InnerText.Replace("-natural", string.Empty)) -
                                 _objCharacter.GetAttribute(bonusNode["name"].InnerText).MetatypeMaximum;
                    }
                    else
                        intMax = ValueToInt(bonusNode["max"].InnerXml, _intRating);
                }
                if (bonusNode.InnerXml.Contains("aug"))
                    intAugMax = ValueToInt(bonusNode["aug"].InnerXml, _intRating);

                string strUseUnique = _strUnique;
                if (bonusNode["name"].Attributes["precedence"] != null)
                    strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

                string strAttribute = bonusNode["name"].InnerText;

                if (bonusNode["affectbase"] != null)
                    strAttribute += "Base";

                CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                    strUseUnique, 0, 1, intMin, intMax, intAug, intAugMax);
            }
            else
            {
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Essence, string.Empty,
                    Convert.ToInt32(bonusNode["val"].InnerText));
            }
        }

        // Add a paid increase to an attribute
        public void attributelevel(XmlNode bonusNode)
        {
            Log.Info(new object[] { "attributelevel", bonusNode.OuterXml });
            String strAttrib = string.Empty;
            int value = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref value);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strAttrib))
            {
                CreateImprovement(strAttrib, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Attributelevel, string.Empty, value);
            }
            else if (bonusNode["options"] != null)
            {
                frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickAttribute.Description =
                        LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
                else
                    frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

                // Add MAG and/or RES to the list of Attributes if they are enabled on the form.
                if (_objCharacter.MAGEnabled)
                    frmPickAttribute.AddMAG();
                if (_objCharacter.RESEnabled)
                    frmPickAttribute.AddRES();
                if (_objCharacter.DEPEnabled)
                    frmPickAttribute.AddDEP();

                Log.Info("attributelevel = " + bonusNode.OuterXml.ToString());

                List<string> strValue = new List<string>();
                foreach (XmlNode objSubNode in bonusNode["options"])
                    strValue.Add(objSubNode.InnerText);
                frmPickAttribute.LimitToList(strValue);

                // Check to see if there is only one possible selection because of _strLimitSelection.
                if (!string.IsNullOrEmpty(ForcedValue))
                    LimitSelection = ForcedValue;

                Log.Info("_strForcedValue = " + ForcedValue);
                Log.Info("_strLimitSelection = " + LimitSelection);

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    frmPickAttribute.SingleAttribute(LimitSelection);
                    frmPickAttribute.Opacity = 0;
                }

                frmPickAttribute.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                Log.Info("_strSelectedValue = " + frmPickAttribute.SelectedAttribute);
                Log.Info("SourceName = " + SourceName);

                CreateImprovement(frmPickAttribute.SelectedAttribute, _objImprovementSource, SourceName,
    Improvement.ImprovementType.Attributelevel, string.Empty, value);
            }
            else
            {
                Log.Error(new object[] { "attributelevel", bonusNode.OuterXml });
            }
        }

        public void skilllevel(XmlNode bonusNode)
        {
            Log.Info(new object[] { "skilllevel", bonusNode.OuterXml });
            String strSkill = string.Empty;
            int value = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref value);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkill))
            {
                CreateImprovement(strSkill, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillLevel, string.Empty, value);
            }
            else
            {
                Log.Error(new object[] { "skilllevel", bonusNode.OuterXml });
            }
        }

        public void pushtext(XmlNode bonusNode)
        {

            String push = bonusNode.InnerText;
            if (!String.IsNullOrWhiteSpace(push))
            {
                _objCharacter.Pushtext.Push(push);
            }
        }

        public void knowsoft(XmlNode bonusNode)
        {
            int val = bonusNode["val"] != null ? ValueToInt(bonusNode["val"].InnerText, _intRating) : 1;

            string name;
            if (!string.IsNullOrWhiteSpace(ForcedValue))
            {
                name = ForcedValue;
            }
            else if (bonusNode["pick"] != null)
            {
                List<ListItem> types;
                if (bonusNode["group"] != null)
                {
                    var v = bonusNode.SelectNodes($"./group");
                    types =
                        KnowledgeSkill.KnowledgeTypes.Where(x => bonusNode.SelectNodes($"group[. = '{x.Value}']").Count > 0).ToList();

                }
                else if (bonusNode["notgroup"] != null)
                {
                    types =
                        KnowledgeSkill.KnowledgeTypes.Where(x => bonusNode.SelectNodes($"notgroup[. = '{x.Value}']").Count == 0).ToList();
                }
                else
                {
                    types = KnowledgeSkill.KnowledgeTypes;
                }

                frmSelectItem select = new frmSelectItem();
                select.DropdownItems = KnowledgeSkill.KnowledgeSkillsWithCategory(types.Select(x => x.Value).ToArray());

                select.ShowDialog();
                if (select.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                name = select.SelectedItem;
            }
            else if (bonusNode["name"] != null)
            {
                name = bonusNode["name"].InnerText;
            }
            else
            {
                //TODO some kind of error handling
                Log.Error(new[] { bonusNode.OuterXml, "Missing pick or name" });
                throw new AbortedException();
            }
            SelectedValue = name;


            KnowledgeSkill skill = new KnowledgeSkill(_objCharacter, name);

            string strTemp = string.Empty;
            if (bonusNode.TryGetStringFieldQuickly("require", ref strTemp) && strTemp == "skilljack")
            {
                _objCharacter.SkillsSection.KnowsoftSkills.Add(skill);
                if (_objCharacter.SkillsoftAccess)
                {
                    _objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
                }
            }
            else
            {
                _objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
            }

            CreateImprovement(name, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillBase, _strUnique, val);
            CreateImprovement(skill.Id.ToString(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillKnowledgeForced, _strUnique);

        }

        public void knowledgeskilllevel(XmlNode bonusNode)
        {
            //Theoretically life modules, right now we just give out free points and let people sort it out themselves.
            //Going to be fun to do the real way, from a computer science perspective, but i don't feel like using 2 weeks on that now

            int val = bonusNode["val"] != null ? ValueToInt(bonusNode["val"].InnerText, _intRating) : 1;
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, string.Empty, val);
        }

        public void knowledgeskillpoints(XmlNode bonusNode)
        {
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, string.Empty,
                ValueToInt(bonusNode.InnerText, Convert.ToInt32(bonusNode.Value)));
        }

        public void skillgrouplevel(XmlNode bonusNode)
        {
            Log.Info(new object[] { "skillgrouplevel", bonusNode.OuterXml });
            String strSkillGroup = String.Empty;
            int value = 1;
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkillGroup) &&
                bonusNode.TryGetInt32FieldQuickly("val", ref value))
            {
                CreateImprovement(strSkillGroup, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroupLevel, string.Empty, value);
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
            Log.Info("nuyenmaxbp = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.NuyenMaxBP, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Apply a bonus/penalty to physical limit.
        public void physicallimit(XmlNode bonusNode)
        {
            Log.Info("physicallimit");
            Log.Info("physicallimit = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("Physical", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalLimit,
                "",
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Apply a bonus/penalty to mental limit.
        public void mentallimit(XmlNode bonusNode)
        {
            Log.Info("mentallimit");
            Log.Info("mentallimit = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("Mental", _objImprovementSource, SourceName, Improvement.ImprovementType.MentalLimit,
                "",
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Apply a bonus/penalty to social limit.
        public void sociallimit(XmlNode bonusNode)
        {
            Log.Info("sociallimit");
            Log.Info("sociallimit = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("Social", _objImprovementSource, SourceName, Improvement.ImprovementType.SocialLimit,
                "",
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
        public void nuyenamt(XmlNode bonusNode)
        {
            Log.Info("nuyenamt");
            Log.Info("nuyenamt = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Nuyen, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Improve Condition Monitors.
        public void conditionmonitor(XmlNode bonusNode)
        {
            Log.Info("conditionmonitor");
            Log.Info("conditionmonitor = " + bonusNode.OuterXml.ToString());
            // Physical Condition.
            if (bonusNode.InnerXml.Contains("physical"))
            {
                Log.Info("Calling CreateImprovement for Physical");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCM, _strUnique,
                    ValueToInt(bonusNode["physical"].InnerText, _intRating));
            }

            // Stun Condition.
            if (bonusNode.InnerXml.Contains("stun"))
            {
                Log.Info("Calling CreateImprovement for Stun");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.StunCM, _strUnique,
                    ValueToInt(bonusNode["stun"].InnerText, _intRating));
            }

            // Condition Monitor Threshold.
            if (bonusNode["threshold"] != null)
            {
                string strUseUnique = _strUnique;
                if (bonusNode["threshold"].Attributes["precedence"] != null)
                    strUseUnique = "precedence" + bonusNode["threshold"].Attributes["precedence"].InnerText;

                Log.Info("Calling CreateImprovement for Threshold");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
                    ValueToInt(bonusNode["threshold"].InnerText, _intRating));
            }

            // Condition Monitor Threshold Offset. (Additioal boxes appear before the FIRST Condition Monitor penalty)
            if (bonusNode["thresholdoffset"] != null)
            {
                string strUseUnique = _strUnique;
                if (bonusNode["thresholdoffset"].Attributes["precedence"] != null)
                    strUseUnique = "precedence" + bonusNode["thresholdoffset"].Attributes["precedence"].InnerText;

                Log.Info("Calling CreateImprovement for Threshold Offset");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CMThresholdOffset,
                    strUseUnique, ValueToInt(bonusNode["thresholdoffset"].InnerText, _intRating));
            }

            // Condition Monitor Overflow.
            if (bonusNode["overflow"] != null)
            {
                Log.Info("Calling CreateImprovement for Overflow");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CMOverflow, _strUnique,
                    ValueToInt(bonusNode["overflow"].InnerText, _intRating));
            }
        }

        // Improve Living Personal Attributes.
        public void livingpersona(XmlNode bonusNode)
        {
            Log.Info("livingpersona");
            Log.Info("livingpersona = " + bonusNode.OuterXml.ToString());
            // Response.
            if (bonusNode.InnerXml.Contains("response"))
            {
                Log.Info("Calling CreateImprovement for response");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaResponse,
                    _strUnique, ValueToInt(bonusNode["response"].InnerText, _intRating));
            }

            // Signal.
            if (bonusNode.InnerXml.Contains("signal"))
            {
                Log.Info("Calling CreateImprovement for signal");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSignal,
                    _strUnique,
                    ValueToInt(bonusNode["signal"].InnerText, _intRating));
            }

            // Firewall.
            if (bonusNode.InnerXml.Contains("firewall"))
            {
                Log.Info("Calling CreateImprovement for firewall");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaFirewall,
                    _strUnique, ValueToInt(bonusNode["firewall"].InnerText, _intRating));
            }

            // System.
            if (bonusNode.InnerXml.Contains("system"))
            {
                Log.Info("Calling CreateImprovement for system");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSystem,
                    _strUnique,
                    ValueToInt(bonusNode["system"].InnerText, _intRating));
            }

            // Biofeedback Filter.
            if (bonusNode.InnerXml.Contains("biofeedback"))
            {
                Log.Info("Calling CreateImprovement for biofeedback");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaBiofeedback,
                    _strUnique, ValueToInt(bonusNode["biofeedback"].InnerText, _intRating));
            }
        }

        // The Improvement adjusts a specific Skill.
        public void specificskill(XmlNode bonusNode)
        {
            Log.Info("specificskill");
            Log.Info("specificskill = " + bonusNode.OuterXml.ToString());
            bool blnAddToRating = false;
            if (bonusNode["applytorating"] != null)
            {
                if (bonusNode["applytorating"].InnerText == "yes")
                    blnAddToRating = true;
            }

            string strUseUnique = _strUnique;
            if (bonusNode.Attributes["precedence"] != null)
                strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;

            // Record the improvement.
            if (bonusNode["bonus"] != null)
            {
                Log.Info("Calling CreateImprovement for bonus");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0, 0, 0,
                    0, string.Empty, blnAddToRating);
            }
            if (bonusNode["max"] != null)
            {
                Log.Info("Calling CreateImprovement for max");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ValueToInt(bonusNode["max"].InnerText, _intRating), 0,
                    0,
                    "", blnAddToRating);
            }
        }

        public void reflexrecorderoptimization(XmlNode bonusNode)
        {
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ReflexRecorderOptimization,
                _strUnique);
        }

        // The Improvement adds a martial art
        public void martialart(XmlNode bonusNode)
        {
            Log.Info("martialart");
            Log.Info("martialart = " + bonusNode.OuterXml.ToString());
            XmlDocument _objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
            XmlNode objXmlArt =
                _objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + bonusNode.InnerText +
                                                 "\"]");

            TreeNode objNode = new TreeNode();
            MartialArt objMartialArt = new MartialArt(_objCharacter);
            objMartialArt.Create(objXmlArt, objNode, _objCharacter);
            objMartialArt.IsQuality = true;
            _objCharacter.MartialArts.Add(objMartialArt);
        }

        // The Improvement adds a limit modifier
        public void limitmodifier(XmlNode bonusNode)
        {
            Log.Info("limitmodifier");
            Log.Info("limitmodifier = " + bonusNode.OuterXml.ToString());
            LimitModifier objLimitMod = new LimitModifier(_objCharacter);
            string strLimit = bonusNode["limit"].InnerText;
            string strBonus = bonusNode["value"].InnerText;
            if (strBonus == "Rating")
            {
                strBonus = _intRating.ToString();
            }
            string strCondition = string.Empty;
            try
            {
                strCondition = bonusNode["condition"].InnerText;
            }
            catch
            {
            }
            int intBonus = 0;
            if (strBonus == "Rating")
                intBonus = _intRating;
            else
                intBonus = Convert.ToInt32(strBonus);
            string strName = _strFriendlyName;
            TreeNode nodTemp = new TreeNode();
            Log.Info("Calling CreateImprovement");
            CreateImprovement(strLimit, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitModifier,
                _strFriendlyName, intBonus, 0, 0, 0, 0, 0, strCondition);
        }

        // The Improvement adjusts a Skill Category.
        public void skillcategory(XmlNode bonusNode)
        {
            Log.Info("skillcategory");
            Log.Info("skillcategory = " + bonusNode.OuterXml.ToString());

            bool blnAddToRating = false;
            if (bonusNode["applytorating"] != null)
            {
                if (bonusNode["applytorating"].InnerText == "yes")
                    blnAddToRating = true;
            }
            if (bonusNode.InnerXml.Contains("exclude"))
            {
                Log.Info("Calling CreateImprovement - exclude");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillCategory, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0,
                    0,
                    0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
            }
            else
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillCategory, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0,
                    0,
                    0, 0, string.Empty, blnAddToRating);
            }
        }

        // The Improvement adjusts a Skill Group.
        public void skillgroup(XmlNode bonusNode)
        {
            Log.Info("skillgroup");
            Log.Info("skillgroup = " + bonusNode.OuterXml.ToString());

            bool blnAddToRating = false;
            if (bonusNode["applytorating"] != null)
            {
                if (bonusNode["applytorating"].InnerText == "yes")
                    blnAddToRating = true;
            }
            if (bonusNode.InnerXml.Contains("exclude"))
            {
                Log.Info("Calling CreateImprovement - exclude");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroup, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0, 0, 0,
                    0, bonusNode["exclude"].InnerText, blnAddToRating);
            }
            else
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroup, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0, 0, 0,
                    0, string.Empty, blnAddToRating);
            }
        }

        // The Improvement adjust Skills with the given CharacterAttribute.
        public void skillattribute(XmlNode bonusNode)
        {
            Log.Info("skillattribute");
            Log.Info("skillattribute = " + bonusNode.OuterXml.ToString());

            string strUseUnique = _strUnique;
            if (bonusNode["name"].Attributes["precedence"] != null)
                strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

            bool blnAddToRating = false;
            if (bonusNode["applytorating"] != null)
            {
                if (bonusNode["applytorating"].InnerText == "yes")
                    blnAddToRating = true;
            }
            if (bonusNode.InnerXml.Contains("exclude"))
            {
                Log.Info("Calling CreateImprovement - exclude");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1,
                    0, 0, 0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
            }
            else
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1,
                    0, 0, 0, 0, string.Empty, blnAddToRating);
            }
        }

        // The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical CharacterAttribute).
        public void skillarticulation(XmlNode bonusNode)
        {
            Log.Info("skillarticulation");
            Log.Info("skillarticulation = " + bonusNode.OuterXml.ToString());

            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.EnhancedArticulation,
                _strUnique,
                ValueToInt(bonusNode["bonus"].InnerText, _intRating));
        }

        // Check for Armor modifiers.
        public void armor(XmlNode bonusNode)
        {
            Log.Info("armor");
            Log.Info("armor = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            string strUseUnique = _strUnique;
            if (bonusNode.Attributes["precedence"] != null)
            {
                strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;
            }
            else if (bonusNode.Attributes["group"] != null)
            {
                strUseUnique = "group" + bonusNode.Attributes["group"].InnerText;
            }
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Armor, strUseUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Reach modifiers.
        public void reach(XmlNode bonusNode)
        {
            Log.Info("reach");
            Log.Info("reach = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Reach, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Unarmed Damage Value modifiers.
        public void unarmeddv(XmlNode bonusNode)
        {
            Log.Info("unarmeddv");
            Log.Info("unarmeddv = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDV, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Unarmed Damage Value Physical.
        public void unarmeddvphysical(XmlNode bonusNode)
        {
            Log.Info("unarmeddvphysical");
            Log.Info("unarmeddvphysical = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDVPhysical, string.Empty);
        }

        // Check for Unarmed Armor Penetration.
        public void unarmedap(XmlNode bonusNode)
        {
            Log.Info("unarmedap");
            Log.Info("unarmedap = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedAP, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Unarmed Armor Penetration.
        public void unarmedreach(XmlNode bonusNode)
        {
            Log.Info("unarmedreach");
            Log.Info("unarmedreach = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedReach, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Initiative modifiers.
        public void initiative(XmlNode bonusNode)
        {
            Log.Info("initiative");
            Log.Info("initiative = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Initiative, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
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
            Log.Info("initiativedice = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");

            string strUseUnique = bonusNode.Name;
            if (bonusNode.Attributes["precedence"] != null)
                strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;

            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDice,
                strUseUnique, ValueToInt(bonusNode.InnerText, _intRating));
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
            Log.Info("initiativediceadd = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDiceAdd, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Matrix Initiative modifiers.
        public void matrixinitiative(XmlNode bonusNode)
        {
            Log.Info("matrixinitiative");
            Log.Info("matrixinitiative = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiative, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
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
            Log.Info("matrixinitiativedice = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                "matrixinitiativepass", ValueToInt(bonusNode.InnerText, _intRating));
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
            Log.Info("matrixinitiativediceadd = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Lifestyle cost modifiers.
        public void lifestylecost(XmlNode bonusNode)
        {
            Log.Info("lifestylecost");
            Log.Info("lifestylecost = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LifestyleCost, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for basic Lifestyle cost modifiers.
        public void basiclifestylecost(XmlNode bonusNode)
        {
            Log.Info("basiclifestylecost");
            Log.Info("basiclifestylecost = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BasicLifestyleCost, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Genetech Cost modifiers.
        public void genetechcostmultiplier(XmlNode bonusNode)
        {
            Log.Info("genetechcostmultiplier");
            Log.Info("genetechcostmultiplier = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechCostMultiplier,
                _strUnique, ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Genetech: Transgenics Cost modifiers.
        public void transgenicsgenetechcost(XmlNode bonusNode)
        {
            Log.Info("transgenicsgenetechcost");
            Log.Info("transgenicsgenetechcost = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.TransgenicsBiowareCost,
                _strUnique, ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Basic Bioware Essence Cost modifiers.
        public void basicbiowareessmultiplier(XmlNode bonusNode)
        {
            Log.Info("basicbiowareessmultiplier");
            Log.Info("basicbiowareessmultiplier = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BasicBiowareEssCost,
                _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Bioware Essence Cost modifiers.
        public void biowareessmultiplier(XmlNode bonusNode)
        {
            Log.Info("biowareessmultiplier");
            Log.Info("biowareessmultiplier = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCost, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Cybeware Essence Cost modifiers.
        public void cyberwareessmultiplier(XmlNode bonusNode)
        {
            Log.Info("cyberwareessmultiplier");
            Log.Info("cyberwareessmultiplier = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCost, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Uneducated modifiers.
        public void uneducated(XmlNode bonusNode)
        {
            Log.Info("uneducated");
            Log.Info("uneducated = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Uneducated, _strUnique);
            _objCharacter.SkillsSection.Uneducated = true;
        }

        // Check for College Education modifiers.
        public void collegeeducation(XmlNode bonusNode)
        {
            Log.Info("collegeeducation");
            Log.Info("collegeeducation = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CollegeEducation, _strUnique);
            _objCharacter.SkillsSection.CollegeEducation = true;
        }

        // Check for Jack Of All Trades modifiers.
        public void jackofalltrades(XmlNode bonusNode)
        {
            Log.Info("jackofalltrades");
            Log.Info("jackofalltrades = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.JackOfAllTrades, _strUnique);
            _objCharacter.SkillsSection.JackOfAllTrades = true;
        }

        // Check for Prototype Transhuman modifiers.
        public void prototypetranshuman(XmlNode bonusNode)
        {
            Log.Info("prototypetranshuman");
            Log.Info("prototypetranshuman = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");

            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.PrototypeTranshuman, _strUnique);
            _objCharacter.PrototypeTranshuman = Convert.ToDecimal(bonusNode.InnerText);

        }

        // Check for Uncouth modifiers.
        public void uncouth(XmlNode bonusNode)
        {
            Log.Info("uncouth");
            Log.Info("uncouth = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Uncouth, _strUnique);
            _objCharacter.SkillsSection.Uncouth = true;
        }

        // Check for Friends In High Places modifiers.
        public void friendsinhighplaces(XmlNode bonusNode)
        {
            Log.Info("friendsinhighplaces");
            Log.Info("friendsinhighplaces = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FriendsInHighPlaces,
                _strUnique);
            _objCharacter.FriendsInHighPlaces = true;
        }

        // Check for School of Hard Knocks modifiers.
        public void schoolofhardknocks(XmlNode bonusNode)
        {
            Log.Info("schoolofhardknocks");
            Log.Info("schoolofhardknocks = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SchoolOfHardKnocks, _strUnique);
            _objCharacter.SkillsSection.SchoolOfHardKnocks = true;
        }

        // Check for ExCon modifiers.
        public void excon(XmlNode bonusNode)
        {
            Log.Info("ExCon");
            Log.Info("ExCon = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ExCon, _strUnique);
            _objCharacter.ExCon = true;
        }

        // Check for TrustFund modifiers.
        public void trustfund(XmlNode bonusNode)
        {
            Log.Info("TrustFund");
            Log.Info("TrustFund = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.TrustFund,
                _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
            _objCharacter.TrustFund = ValueToInt(bonusNode.InnerText, _intRating);
        }

        // Check for Tech School modifiers.
        public void techschool(XmlNode bonusNode)
        {
            Log.Info("techschool");
            Log.Info("techschool = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.TechSchool, _strUnique);
            _objCharacter.SkillsSection.TechSchool = true;
        }

        // Check for MadeMan modifiers.
        public void mademan(XmlNode bonusNode)
        {
            Log.Info("MadeMan");
            Log.Info("MadeMan = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MadeMan, _strUnique);
            _objCharacter.MadeMan = true;
        }

        // Check for Linguist modifiers.
        public void linguist(XmlNode bonusNode)
        {
            Log.Info("Linguist");
            Log.Info("Linguist = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Linguist, _strUnique);
            _objCharacter.SkillsSection.Linguist = true;
        }

        // Check for LightningReflexes modifiers.
        public void lightningreflexes(XmlNode bonusNode)
        {
            Log.Info("LightningReflexes");
            Log.Info("LightningReflexes = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LightningReflexes, _strUnique);
            _objCharacter.LightningReflexes = true;
        }

        // Check for Fame modifiers.
        public void fame(XmlNode bonusNode)
        {
            Log.Info("Fame");
            Log.Info("Fame = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Fame, _strUnique);
            _objCharacter.Fame = true;
        }

        // Check for BornRich modifiers.
        public void bornrich(XmlNode bonusNode)
        {
            Log.Info("BornRich");
            Log.Info("BornRich = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BornRich, _strUnique);
            _objCharacter.BornRich = true;
        }

        // Check for Erased modifiers.
        public void erased(XmlNode bonusNode)
        {
            Log.Info("Erased");
            Log.Info("Erased = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Erased, _strUnique);
            _objCharacter.Erased = true;
        }

        // Check for Erased modifiers.
        public void overclocker(XmlNode bonusNode)
        {
            Log.Info("OverClocker");
            Log.Info("Overclocker = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Overclocker, _strUnique);
            _objCharacter.Overclocker = true;
        }

        // Check for Restricted Gear modifiers.
        public void restrictedgear(XmlNode bonusNode)
        {
            Log.Info("restrictedgear");
            Log.Info("restrictedgear = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedGear, _strUnique);
            _objCharacter.RestrictedGear = true;
        }

        // Check for Improvements that grant bonuses to the maximum amount of Native languages a user can have.
        public void nativelanguagelimit(XmlNode bonusNode)
        {
            Log.Info("nativelanguagelimit");
            Log.Info("nativelanguagelimit = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.NativeLanguageLimit,
                _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Ambidextrous modifiers.
        public void ambidextrous(XmlNode bonusNode)
        {
            Log.Info("Ambidextrous");
            Log.Info("Ambidextrous = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Ambidextrous, _strUnique);
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
            Log.Info("weaponcategorydv = " + bonusNode.OuterXml.ToString());
            XmlNodeList objXmlCategoryList = bonusNode.SelectNodes("weaponcategorydv");
            XmlNode nodWeapon = bonusNode;

            if (nodWeapon["selectskill"] != null)
            {
                // Display the Select Skill window and record which Skill was selected.
                frmSelectItem frmPickCategory = new frmSelectItem();
                List<ListItem> lstGeneralItems = new List<ListItem>();

                ListItem liBlades = new ListItem();
                liBlades.Name = "Blades";
                liBlades.Value = "Blades";

                ListItem liClubs = new ListItem();
                liClubs.Name = "Clubs";
                liClubs.Value = "Clubs";

                ListItem liUnarmed = new ListItem();
                liUnarmed.Name = "Unarmed";
                liUnarmed.Value = "Unarmed";

                ListItem liAstral = new ListItem();
                liAstral.Name = "Astral Combat";
                liAstral.Value = "Astral Combat";

                ListItem liExotic = new ListItem();
                liExotic.Name = "Exotic Melee Weapons";
                liExotic.Value = "Exotic Melee Weapons";

                lstGeneralItems.Add(liAstral);
                lstGeneralItems.Add(liBlades);
                lstGeneralItems.Add(liClubs);
                lstGeneralItems.Add(liExotic);
                lstGeneralItems.Add(liUnarmed);
                frmPickCategory.GeneralItems = lstGeneralItems;

                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickCategory.Description =
                        LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", _strFriendlyName);
                else
                    frmPickCategory.Description = LanguageManager.Instance.GetString("Title_SelectWeaponCategory");

                Log.Info("_strForcedValue = " + ForcedValue);

                if (ForcedValue.StartsWith("Adept:") || ForcedValue.StartsWith("Magician:"))
                    ForcedValue = string.Empty;

                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    frmPickCategory.Opacity = 0;
                }
                frmPickCategory.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickCategory.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickCategory.SelectedItem;

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
                    Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ValueToInt(nodWeapon["bonus"].InnerXml, _intRating));
            }
            else
            {
                // Run through each of the Skill Groups since there may be more than one affected.
                foreach (XmlNode objXmlCategory in objXmlCategoryList)
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(objXmlCategory["name"].InnerText, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ValueToInt(objXmlCategory["bonus"].InnerXml, _intRating));
                }
            }
        }

        // Check for Mentor Spirit bonuses.
        public void selectmentorspirit(XmlNode bonusNode)
        {
            Log.Info("selectmentorspirit");
            Log.Info("selectmentorspirit = " + bonusNode.OuterXml.ToString());
            frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
            frmPickMentorSpirit.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            XmlDocument doc = XmlManager.Instance.Load("mentors.xml");
            XmlNode mentorDoc = doc.SelectSingleNode("/chummer/mentors/mentor[id = \"" + frmPickMentorSpirit.SelectedMentor + "\"]");
            SelectedValue = mentorDoc["name"].InnerText;

            string strHoldValue = SelectedValue;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            if (frmPickMentorSpirit.BonusNode != null)
            {
                if (!CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName))
                {
                    throw new AbortedException();
                }
            }

            if (frmPickMentorSpirit.Choice1BonusNode != null)
            {
                Log.Info("frmPickMentorSpirit.Choice1BonusNode = " + frmPickMentorSpirit.Choice1BonusNode.OuterXml.ToString());
                string strForce = ForcedValue;
                if (!frmPickMentorSpirit.Choice1.StartsWith("Adept:") && !frmPickMentorSpirit.Choice1.StartsWith("Magician:"))
                    ForcedValue = frmPickMentorSpirit.Choice1;
                else
                    ForcedValue = string.Empty;
                Log.Info("Calling CreateImprovement");
                bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice1BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName);
                if (!blnSuccess)
                {
                    throw new AbortedException();
                }
                ForcedValue = strForce;
                _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
            }

            if (frmPickMentorSpirit.Choice2BonusNode != null)
            {
                Log.Info("frmPickMentorSpirit.Choice2BonusNode = " + frmPickMentorSpirit.Choice2BonusNode.OuterXml.ToString());
                string strForce = ForcedValue;
                if (!frmPickMentorSpirit.Choice2.StartsWith("Adept:") && !frmPickMentorSpirit.Choice2.StartsWith("Magician:"))
                    ForcedValue = frmPickMentorSpirit.Choice2;
                else
                    ForcedValue = string.Empty;
                Log.Info("Calling CreateImprovement");
                bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice2BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName);
                if (!blnSuccess)
                {
                    throw new AbortedException();
                }
                ForcedValue = strForce;
                _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
            }

            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MentorSpirit, frmPickMentorSpirit.SelectedMentor);

            if (frmPickMentorSpirit.MentorsMask)
            {
                Log.Info("frmPickMentorSpirit.MentorsMask = " + frmPickMentorSpirit.MentorsMask);
                Log.Info("Calling CreateImprovement");
                bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice2BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName);
                CreateImprovement(_strFriendlyName, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerPoints, string.Empty, 1);
                CreateImprovement(_strFriendlyName, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainValue, string.Empty, -1);
                if (!blnSuccess)
                {
                    throw new AbortedException();
                }
            }

            SelectedValue = strHoldValue;
            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("_strForcedValue = " + ForcedValue);
        }

        // Check for Paragon bonuses.
        public void selectparagon(XmlNode bonusNode)
        {
            Log.Info("selectparagon");
            Log.Info("selectparagon = " + bonusNode.OuterXml.ToString());
            frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
            frmPickMentorSpirit.XmlFile = "paragons.xml";
            frmPickMentorSpirit.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            
            XmlDocument doc = XmlManager.Instance.Load("paragons.xml");
            XmlNode mentorDoc = doc.SelectSingleNode("/chummer/mentors/mentor[id = \"" + frmPickMentorSpirit.SelectedMentor + "\"]");
            SelectedValue = mentorDoc["name"].InnerText;

            string strHoldValue = SelectedValue;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Paragon, frmPickMentorSpirit.SelectedMentor);

            if (frmPickMentorSpirit.BonusNode != null)
            {
                bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName);
                if (!blnSuccess)
                {
                    throw new AbortedException();
                }
            }

            if (frmPickMentorSpirit.Choice1BonusNode != null)
            {
                string strForce = ForcedValue;
                ForcedValue = frmPickMentorSpirit.Choice1;
                bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice1BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName);
                if (!blnSuccess)
                {
                    throw new AbortedException();
                }
                ForcedValue = strForce;
                _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
            }

            if (frmPickMentorSpirit.Choice2BonusNode != null)
            {
                string strForce = ForcedValue;
                ForcedValue = frmPickMentorSpirit.Choice2;
                bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice2BonusNode,
                    _blnConcatSelectedValue, _intRating, _strFriendlyName);
                if (!blnSuccess)
                {
                    throw new AbortedException();
                }
                ForcedValue = strForce;
                _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
            }

            SelectedValue = strHoldValue;
        }

        // Check for Smartlink bonus.
        public void smartlink(XmlNode bonusNode)
        {
            Log.Info("smartlink");
            Log.Info("smartlink = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Smartlink, "smartlink");
        }

        // Check for Adapsin bonus.
        public void adapsin(XmlNode bonusNode)
        {
            Log.Info("adapsin");
            Log.Info("adapsin = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Adapsin, "adapsin");
        }

        // Check for SoftWeave bonus.
        public void softweave(XmlNode bonusNode)
        {
            Log.Info("softweave");
            Log.Info("softweave = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SoftWeave, "softweave");
        }

        // Check for Sensitive System.
        public void sensitivesystem(XmlNode bonusNode)
        {
            Log.Info("sensitivesystem");
            Log.Info("sensitivesystem = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SensitiveSystem,
                "sensitivesystem");
        }

        // Check for Movement Percent.
        public void movementmultiplier(XmlNode bonusNode)
        {
            Log.Info("movementmultiplier");
            Log.Info("movementmultiplier = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MovementMultiplier, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Movement Percent.
        public void movementpercent(XmlNode bonusNode)
        {
            Log.Info("movementpercent");
            Log.Info("movementpercent = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MovementPercent, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Swim Percent.
        public void swimpercent(XmlNode bonusNode)
        {
            Log.Info("swimpercent");
            Log.Info("swimpercent = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SwimPercent, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Fly Percent.
        public void flypercent(XmlNode bonusNode)
        {
            Log.Info("flypercent");
            Log.Info("flypercent = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FlyPercent, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Fly Speed.
        public void flyspeed(XmlNode bonusNode)
        {
            Log.Info("flyspeed");
            Log.Info("flyspeed = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FlySpeed, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for free Positive Qualities.
        public void freepositivequalities(XmlNode bonusNode)
        {
            Log.Info("freepositivequalities");
            Log.Info("freepositivequalities = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreePositiveQualities, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for free Negative Qualities.
        public void freenegativequalities(XmlNode bonusNode)
        {
            Log.Info("freenegativequalities");
            Log.Info("freenegativequalities = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeNegativeQualities, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Select Side.
        public void selectside(XmlNode bonusNode)
        {
            Log.Info("selectside");
            Log.Info("selectside = " + bonusNode.OuterXml.ToString());
            frmSelectSide frmPickSide = new frmSelectSide();
            frmPickSide.Description = LanguageManager.Instance.GetString("Label_SelectSide").Replace("{0}", _strFriendlyName);
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
            Log.Info("freespiritpowerpoints = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Adept Power Points.
        public void adeptpowerpoints(XmlNode bonusNode)
        {
            Log.Info("adeptpowerpoints");
            Log.Info("adeptpowerpoints = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerPoints, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Adept Powers
        public void specificpower(XmlNode bonusNode)
        {
            Log.Info("specificpower");
            Log.Info("specificpower = " + bonusNode.OuterXml.ToString());
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (_objCharacter.AdeptEnabled)
            {
                string strSelection = string.Empty;
                ForcedValue = string.Empty;


                Log.Info("objXmlSpecificPower = " + bonusNode.OuterXml.ToString());

                string strPowerName = bonusNode["name"].InnerText;
                int intLevels = 0;
                if (bonusNode["val"] != null)
                    intLevels = Convert.ToInt32(bonusNode["val"].InnerText);

                string strPowerNameLimit = strPowerName;

                // Check if the character already has this power
                    Log.Info("strSelection = " + strSelection);
                Power objNewPower = new Power(_objCharacter);
                XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
                XmlNode objXmlPower =
                    objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerNameLimit + "\"]");
                objNewPower.Create(objXmlPower, _manager, 0);

                bool blnHasPower = _objCharacter.Powers.Any(objPower => objPower.Name == objNewPower.Name && objPower.Extra == objNewPower.Extra);
                if (!blnHasPower)
                {
                    _objCharacter.Powers.Add(objNewPower);
                    }

                Log.Info("blnHasPower = " + blnHasPower);
                Log.Info("Calling CreateImprovement");
                CreateImprovement(objNewPower.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerFreeLevels, objNewPower.Extra, 0, intLevels);
                    }
                    }

        // Select a Power.
        public void selectpowers(XmlNode bonusNode)
        {
            XmlNodeList objXmlPowerList = bonusNode.SelectNodes("selectpower");
            foreach (XmlNode objNode in objXmlPowerList)
            {
                Log.Info("selectpower");
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("_strForcedValue = " + ForcedValue);

                // Display the Select Power window and record which Power was selected.
                    frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
                    Log.Info("selectpower = " + objNode.OuterXml.ToString());

                int intLevels = 0;
                if (objNode["ignorerating"] != null)
                    frmPickPower.IgnoreLimits = Convert.ToBoolean(objNode["ignorerating"].InnerText);
                if (objNode["val"] != null)
                    intLevels = Convert.ToInt32(objNode["val"].InnerText.Replace("Rating", _intRating.ToString()));
                if (objNode["pointsperlevel"] != null)
                    frmPickPower.PointsPerLevel = Convert.ToDouble(objNode["pointsperlevel"].InnerText, GlobalOptions.InvariantCultureInfo);
                if (objNode["limit"] != null)
                    frmPickPower.LimitToRating = Convert.ToInt32(objNode["limit"].InnerText.Replace("Rating",_intRating.ToString()));
                    if (objNode.OuterXml.Contains("limittopowers"))
                        frmPickPower.LimitToPowers = objNode.Attributes["limittopowers"].InnerText;
                    frmPickPower.ShowDialog();

                    // Make sure the dialogue window was not canceled.
                    if (frmPickPower.DialogResult == DialogResult.Cancel)
                    {
                    //TODO: Rollback is unimplemented.
                    Rollback();
                    }
                else
                {
                    SelectedValue = frmPickPower.SelectedPower;
                    if (_blnConcatSelectedValue)
                        SourceName += " (" + SelectedValue + ")";

                    XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
                    XmlNode objXmlPower =
                        objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + SelectedValue + "\"]");

                    // If no, add the power and mark it free or give it free levels
                    Power objNewPower = new Power(_objCharacter);
                    objNewPower.Create(objXmlPower, _manager,0);

                    bool blnHasPower = _objCharacter.Powers.Any(objPower => objPower.Name == objNewPower.Name && objPower.Extra == objNewPower.Extra);

                    Log.Info("blnHasPower = " + blnHasPower);

                    if (!blnHasPower)
                        {
                        _objCharacter.Powers.Add(objNewPower);
                        }

                    Log.Info("blnHasPower = " + blnHasPower);
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(objNewPower.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerFreePoints, objNewPower.Extra, 0, intLevels);

                    if (blnHasPower)
                    {
                        foreach (
                            Power objPower in
                            _objCharacter.Powers.Where(
                                objPower => objPower.Name == objNewPower.Name && objPower.Extra == objNewPower.Extra))
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
            Log.Info("armorencumbrancepenalty = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Initiation.
        public void initiation(XmlNode bonusNode)
        {
            Log.Info("initiation");
            Log.Info("initiation = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Initiation, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
            _objCharacter.InitiateGrade += ValueToInt(bonusNode.InnerText, _intRating);
        }

        // Check for Submersion.
        public void submersion(XmlNode bonusNode)
        {
            Log.Info("submersion");
            Log.Info("submersion = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Submersion, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
            _objCharacter.SubmersionGrade += ValueToInt(bonusNode.InnerText, _intRating);
        }

        // Check for Skillwires.
        public void skillwire(XmlNode bonusNode)
        {
            Log.Info("skillwire");
            Log.Info("skillwire = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Skillwire, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Hardwires.
        public void hardwires(XmlNode bonusNode)
        {
            Log.Info("hardwire");
            Log.Info("hardwire = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            Cyberware objCyberware = CommonFunctions.DeepFindById(SourceName, _objCharacter.Cyberware);
            if (objCyberware == null)
            {
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("_strForcedValue = " + ForcedValue);

                // Display the Select Skill window and record which Skill was selected.
                frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                if (!string.IsNullOrEmpty(_strFriendlyName))
                    frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
                        .Replace("{0}", _strFriendlyName);
                else
                    frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

                Log.Info("selectskill = " + bonusNode.OuterXml.ToString());
                if (bonusNode.OuterXml.Contains("skillgroup"))
                    frmPickSkill.OnlySkillGroup = bonusNode.Attributes["skillgroup"].InnerText;
                else if (bonusNode.OuterXml.Contains("skillcategory"))
                    frmPickSkill.OnlyCategory = bonusNode.Attributes["skillcategory"].InnerText;
                else if (bonusNode.OuterXml.Contains("excludecategory"))
                    frmPickSkill.ExcludeCategory = bonusNode.Attributes["excludecategory"].InnerText;
                else if (bonusNode.OuterXml.Contains("limittoskill"))
                    frmPickSkill.LimitToSkill = bonusNode.Attributes["limittoskill"].InnerText;
                else if (bonusNode.OuterXml.Contains("limittoattribute"))
                    frmPickSkill.LinkedAttribute = bonusNode.Attributes["limittoattribute"].InnerText;

                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    frmPickSkill.OnlySkill = ForcedValue;
                    frmPickSkill.Opacity = 0;
                }
                frmPickSkill.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickSkill.SelectedSkill;
            }
            else
            {
                SelectedValue = objCyberware.Location;
            }
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Hardwire,
                SelectedValue,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Damage Resistance.
        public void damageresistance(XmlNode bonusNode)
        {
            Log.Info("damageresistance");
            Log.Info("damageresistance = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.DamageResistance, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Restricted Item Count.
        public void restricteditemcount(XmlNode bonusNode)
        {
            Log.Info("restricteditemcount");
            Log.Info("restricteditemcount = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedItemCount, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Judge Intentions.
        public void judgeintentions(XmlNode bonusNode)
        {
            Log.Info("judgeintentions");
            Log.Info("judgeintentions = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentions, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Composure.
        public void composure(XmlNode bonusNode)
        {
            Log.Info("composure");
            Log.Info("composure = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Composure, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Lift and Carry.
        public void liftandcarry(XmlNode bonusNode)
        {
            Log.Info("liftandcarry");
            Log.Info("liftandcarry = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LiftAndCarry, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Memory.
        public void memory(XmlNode bonusNode)
        {
            Log.Info("memory");
            Log.Info("memory = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Memory, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Concealability.
        public void concealability(XmlNode bonusNode)
        {
            Log.Info("concealability");
            Log.Info("concealability = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Concealability, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Drain Resistance.
        public void drainresist(XmlNode bonusNode)
        {
            Log.Info("drainresist");
            Log.Info("drainresist = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.DrainResistance, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Fading Resistance.
        public void fadingresist(XmlNode bonusNode)
        {
            Log.Info("fadingresist");
            Log.Info("fadingresist = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FadingResistance, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Notoriety.
        public void notoriety(XmlNode bonusNode)
        {
            Log.Info("notoriety");
            Log.Info("notoriety = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Notoriety, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Complex Form Limit.
        public void complexformlimit(XmlNode bonusNode)
        {
            Log.Info("complexformlimit");
            Log.Info("complexformlimit = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ComplexFormLimit, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Spell Limit.
        public void spelllimit(XmlNode bonusNode)
        {
            Log.Info("spelllimit");
            Log.Info("spelllimit = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SpellLimit, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Free Spells.
        public void freespells(XmlNode bonusNode)
        {
            Log.Info("freespells");
            Log.Info("freespells = " + bonusNode.OuterXml.ToString());
            if (bonusNode.Attributes?["attribute"] != null)
            {
                Log.Info("attribute");
                CharacterAttrib att = _objCharacter.GetAttribute(bonusNode.Attributes?["attribute"].InnerText);
                if (att != null)
                {
                    Log.Info(att.Abbrev);
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(att.Abbrev, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsATT, string.Empty);
                }
            }
            else if (bonusNode.Attributes?["skill"] != null)
            {
                Log.Info("skill");
                Skill objSkill = _objCharacter.SkillsSection.Skills.First(x => x.Name == bonusNode.Attributes["skill"].InnerText);
                Log.Info(bonusNode.Attributes["skill"].InnerText);
                if (objSkill != null)
                {
                    Log.Info("Calling CreateImprovement");
                    CreateImprovement(objSkill.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsSkill,string.Empty);
                }
            }
            else
            {
                Log.Info("Calling CreateImprovement");
                CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, string.Empty,
                    ValueToInt(bonusNode.InnerText, _intRating));
            }
        }

        // Check for Spell Category bonuses.
        public void spellcategory(XmlNode bonusNode)
        {
            Log.Info("spellcategory");
            Log.Info("spellcategory = " + bonusNode.OuterXml.ToString());

            string strUseUnique = _strUnique;
            if (bonusNode["name"].Attributes["precedence"] != null)
                strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

            Log.Info("Calling CreateImprovement");
            CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategory, strUseUnique, ValueToInt(bonusNode["val"].InnerText, _intRating));
        }

        // Check for Throwing Range bonuses.
        public void throwrange(XmlNode bonusNode)
        {
            Log.Info("throwrange");
            Log.Info("throwrange = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRange, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Throwing STR bonuses.
        public void throwstr(XmlNode bonusNode)
        {
            Log.Info("throwstr");
            Log.Info("throwstr = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowSTR, _strUnique,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Skillsoft access.
        public void skillsoftaccess(XmlNode bonusNode)
        {
            Log.Info("skillsoftaccess");
            Log.Info("skillsoftaccess = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SkillsoftAccess, string.Empty);
            _objCharacter.SkillsSection.KnowledgeSkills.AddRange(_objCharacter.SkillsSection.KnowsoftSkills);
        }

        // Check for Quickening Metamagic.
        public void quickeningmetamagic(XmlNode bonusNode)
        {
            Log.Info("quickeningmetamagic");
            Log.Info("quickeningmetamagic = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.QuickeningMetamagic, string.Empty);
        }

        // Check for ignore Stun CM Penalty.
        public void ignorecmpenaltystun(XmlNode bonusNode)
        {
            Log.Info("ignorecmpenaltystun");
            Log.Info("ignorecmpenaltystun = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, string.Empty);
        }

        // Check for ignore Physical CM Penalty.
        public void ignorecmpenaltyphysical(XmlNode bonusNode)
        {
            Log.Info("ignorecmpenaltyphysical");
            Log.Info("ignorecmpenaltyphysical = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, string.Empty);
        }

        // Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
        public void cyborgessence(XmlNode bonusNode)
        {
            Log.Info("cyborgessence");
            Log.Info("cyborgessence = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CyborgEssence, string.Empty);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public void essencepenalty(XmlNode bonusNode)
        {
            Log.Info("essencepenalty");
            Log.Info("essencepenalty = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenalty, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public void essencemax(XmlNode bonusNode)
        {
            Log.Info("essencemax");
            Log.Info("essencemax = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.EssenceMax, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        // Check for Select Sprite.
        public void selectsprite(XmlNode bonusNode)
        {
            Log.Info("selectsprite");
            Log.Info("selectsprite = " + bonusNode.OuterXml.ToString());
            XmlDocument objXmlDocument = XmlManager.Instance.Load("critters.xml");
            XmlNodeList objXmlNodeList =
                objXmlDocument.SelectNodes("/chummer/metatypes/metatype[contains(category, \"Sprites\")]");
            List<ListItem> lstCritters = new List<ListItem>();
            foreach (XmlNode objXmlNode in objXmlNodeList)
            {
                ListItem objItem = new ListItem();
                objItem.Name = objXmlNode["translate"]?.InnerText ?? objXmlNode["name"].InnerText;
                objItem.Value = objItem.Name;
                lstCritters.Add(objItem);
            }

            frmSelectItem frmPickItem = new frmSelectItem();
            frmPickItem.GeneralItems = lstCritters;
            frmPickItem.ShowDialog();

            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedItem;

            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
                Improvement.ImprovementType.AddSprite,
                "");
        }

        // Check for Black Market Discount.
        public void blackmarketdiscount(XmlNode bonusNode)
        {
            Log.Info("blackmarketdiscount");
            Log.Info("blackmarketdiscount = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BlackMarketDiscount,
                _strUnique);
            _objCharacter.BlackMarketDiscount = true;
        }

        // Select Armor (Mostly used for Custom Fit (Stack)).
        public void selectarmor(XmlNode bonusNode)
        {
            Log.Info("selectarmor");
            Log.Info("selectarmor = " + bonusNode.OuterXml.ToString());
            string strSelectedValue = string.Empty;
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            XmlDocument objXmlDocument = XmlManager.Instance.Load("armor.xml");
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

            //.SelectNodes("/chummer/skills/skill[not(exotic) and (" + _objCharacter.Options.BookXPath() + ")" + SkillFilter(filter) + "]");

            List<ListItem> lstArmors = new List<ListItem>();
            foreach (XmlNode objNode in objXmlNodeList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objNode["name"]?.InnerText;
                objItem.Name = objNode.Attributes?["translate"]?.InnerText ?? objNode["name"]?.InnerText;
                lstArmors.Add(objItem);
            }

            if (lstArmors.Count > 0)
            {

                frmSelectItem frmPickItem = new frmSelectItem();
                frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
                    .Replace("{0}", _strFriendlyName);
                frmPickItem.GeneralItems = lstArmors;

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
                    SourceName += " (" + SelectedValue + ")";

                strSelectedValue = frmPickItem.SelectedItem;
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + strSelectedValue);
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
            XmlDocument objXmlDocument = XmlManager.Instance.Load("cyberware.xml");
            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes(bonusNode["category"] != null 
            ? $"/chummer/cyberwares/cyberware[(category = '{bonusNode["category"].InnerText}') and ({_objCharacter.Options.BookXPath()})]" 
            : $"/chummer/cyberwares/cyberware[({_objCharacter.Options.BookXPath()})]");

            List<ListItem> list = new List<ListItem>();
            foreach (XmlNode objNode in objXmlNodeList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objNode["name"]?.InnerText;
                objItem.Name = objNode.Attributes?["translate"]?.InnerText ?? objNode["name"]?.InnerText;
                list.Add(objItem);
            }

            if (list.Count <= 0) return;
            frmSelectItem frmPickItem = new frmSelectItem();
            frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
                .Replace("{0}", _strFriendlyName);
            frmPickItem.GeneralItems = list;

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
                SourceName += " (" + SelectedValue + ")";

            string strSelectedValue = frmPickItem.SelectedItem;
            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SelectedValue = " + strSelectedValue);
        }

        // Select Weapon (custom entry for things like Spare Clip).
        public void selectweapon(XmlNode bonusNode)
        {
            Log.Info("selectweapon");
            Log.Info("selectweapon = " + bonusNode.OuterXml.ToString());
            string strSelectedValue = string.Empty;
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (_objCharacter == null)
            {
                // If the character is null (this is a Vehicle), the user must enter their own string.
                // Display the Select Item window and record the value that was entered.
                frmSelectText frmPickText = new frmSelectText();
                frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
                    .Replace("{0}", _strFriendlyName);

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
                    SourceName += " (" + SelectedValue + ")";

                strSelectedValue = frmPickText.SelectedValue;
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + strSelectedValue);
            }
            else
            {
                string strExclude = string.Empty;
                List <ListItem> lstWeapons = new List<ListItem>();
                bool blnIncludeUnarmed = bonusNode.Attributes["excludecategory"]?.InnerText == "true";
                strExclude = bonusNode.Attributes["excludecategory"]?.InnerText;
                foreach (Weapon objWeapon in _objCharacter.Weapons)
                {
                    bool blnAdd = !(!string.IsNullOrEmpty(strExclude) && objWeapon.WeaponType == strExclude || !blnIncludeUnarmed && objWeapon.Name == "Unarmed Attack");
                    if (blnAdd)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objWeapon.Name;
                        objItem.Name = objWeapon.DisplayName;
                        lstWeapons.Add(objItem);
                    }
                }

                frmSelectItem frmPickItem = new frmSelectItem();
                frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
                    .Replace("{0}", _strFriendlyName);
                frmPickItem.GeneralItems = lstWeapons;

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
                    SourceName += " (" + SelectedValue + ")";

                strSelectedValue = frmPickItem.SelectedItem;
                Log.Info("_strSelectedValue = " + SelectedValue);
                Log.Info("SelectedValue = " + strSelectedValue);
            }

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(strSelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique);
        }

        // Select an Optional Power.
        public void optionalpowers(XmlNode bonusNode)
        {
            XmlNodeList objXmlPowerList = bonusNode.SelectNodes("optionalpower");
            Log.Info("selectoptionalpower");
            // Display the Select Attribute window and record which Skill was selected.
            frmSelectOptionalPower frmPickPower = new frmSelectOptionalPower();
            frmPickPower.Description = LanguageManager.Instance.GetString("String_Improvement_SelectOptionalPower");
            string strForcedValue = string.Empty;

            List<KeyValuePair<string, string>> lstValue = new List<KeyValuePair<string, string>>();
            foreach (XmlNode objXmlOptionalPower in objXmlPowerList)
            {
                string strQuality = objXmlOptionalPower.InnerText;
                if (objXmlOptionalPower.Attributes["select"] != null)
                {
                    strForcedValue = objXmlOptionalPower.Attributes["select"].InnerText;
                }
                lstValue.Add(new KeyValuePair<string, string>(strQuality, strForcedValue));
            }
            frmPickPower.LimitToList(lstValue);


            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            Log.Info("_strForcedValue = " + ForcedValue);
            Log.Info("_strLimitSelection = " + LimitSelection);

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                frmPickPower.SinglePower(LimitSelection);
                frmPickPower.Opacity = 0;
            }

            frmPickPower.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickPower.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            strForcedValue = frmPickPower.SelectedPowerExtra;
            // Record the improvement.
            XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
            XmlNode objXmlPowerNode =
                objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + frmPickPower.SelectedPower + "\"]");
            TreeNode objPowerNode = new TreeNode();
            CritterPower objPower = new CritterPower(_objCharacter);

            objPower.Create(objXmlPowerNode, _objCharacter, objPowerNode, 0, strForcedValue);
            _objCharacter.CritterPowers.Add(objPower);
        }

        public void critterpowers(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
            foreach (XmlNode objXmlPower in bonusNode.SelectNodes("power"))
            {
                XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                TreeNode objPowerNode = new TreeNode();
                CritterPower objPower = new CritterPower(_objCharacter);
                string strForcedValue = string.Empty;
                int intRating = 0;
                if (objXmlPower.Attributes != null && objXmlPower.Attributes.Count > 0)
                {
                    intRating = Convert.ToInt32(objXmlPower.Attributes["rating"]?.InnerText);
                    strForcedValue = objXmlPower.Attributes["select"]?.InnerText;
                }

                objPower.Create(objXmlCritterPower, _objCharacter, objPowerNode, intRating, strForcedValue);
                _objCharacter.CritterPowers.Add(objPower);
            }
        }

        // Check for Adept Power Points.
        public void critterpowerlevels(XmlNode bonusNode)
        {
            foreach (XmlNode objXmlPower in bonusNode.SelectNodes("power"))
            {
                Log.Info("critterpowerlevels");
                Log.Info("critterpowerlevels = " + bonusNode.OuterXml.ToString());
                Log.Info("Calling CreateImprovement");
                CreateImprovement(objXmlPower["name"].InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPowerLevel,
                    string.Empty,
                    ValueToInt(objXmlPower["val"].InnerText, _intRating));
            }
        }

        public void publicawareness(XmlNode bonusNode)
        {
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.PublicAwareness, _strUnique,
                ValueToInt(bonusNode.InnerText, 1));
        }

        public void dealerconnection(XmlNode bonusNode)
        {
            Log.Info("dealerconnection");
            frmSelectItem frmPickItem = new frmSelectItem();
            List<ListItem> lstItems = new List<ListItem>();
            XmlNodeList objXmlList = bonusNode.SelectNodes("category");
            foreach (XmlNode objNode in objXmlList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objNode.InnerText;
                objItem.Name = objNode.InnerText;
                lstItems.Add(objItem);
            }
            frmPickItem.GeneralItems = lstItems;
            frmPickItem.AllowAutoSelect = false;
            frmPickItem.ShowDialog();
            // Make sure the dialogue window was not canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickItem.SelectedItem;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            // Create the Improvement.
            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
                Improvement.ImprovementType.DealerConnection, _strUnique);
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

            SkillsSection.FilterOptions skills;
            string strName = string.Empty;
            if (Enum.TryParse(final, out skills))
            {
                bool blnAdd = true;
                if (bonusNode.Attributes["name"] != null)
                {
                    strName = bonusNode.Attributes["name"].InnerText;
                    blnAdd = _objCharacter.SkillsSection.Skills.All(objSkill => objSkill.Name != strName);
                }

                if (blnAdd)
                {
                    _objCharacter.SkillsSection.AddSkills(skills, strName);
                    CreateImprovement(skills.ToString(), Improvement.ImprovementSource.Quality, SourceName,
                        Improvement.ImprovementType.SpecialSkills, _strUnique);
                }
            }
            else
            {
                Utils.BreakIfDebug();
                Log.Info(new[] { "Failed to parse", "specialskills", bonusNode.OuterXml });
            }
        }

        public void addqualities(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
            foreach (XmlNode objXmlAddQuality in bonusNode.SelectNodes("addquality"))
            {
                XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlAddQuality.InnerText + "\"]");
                string strForceValue = string.Empty;
                if (objXmlAddQuality.Attributes["select"] != null)
                    strForceValue = objXmlAddQuality.Attributes["select"].InnerText;
                bool blnAddQuality = _objCharacter.Qualities.All(objCharacterQuality => objCharacterQuality.Name != objXmlAddQuality.InnerText || objCharacterQuality.Extra != strForceValue);

                // Make sure the character does not yet have this Quality.

                if (blnAddQuality)
                {
                    TreeNode objAddQualityNode = new TreeNode();
                    Quality objAddQuality = new Quality(_objCharacter);
                    objAddQuality.Create(objXmlSelectedQuality, _objCharacter, QualitySource.Selected, objAddQualityNode, null, null, strForceValue);

                    bool blnFree = (objXmlAddQuality.Attributes["contributetobp"] == null ||
                                    (objXmlAddQuality.Attributes["contributetobp"]?.InnerText.ToLower() != "true"));
                    if (blnFree)
                    {
                        objAddQuality.BP = 0;
                        objAddQuality.ContributeToLimit = false;
                    }
                    _objCharacter.Qualities.Add(objAddQuality);
                    CreateImprovement(objAddQuality.InternalId, Improvement.ImprovementSource.Quality, SourceName,
                    Improvement.ImprovementType.SpecificQuality, _strUnique);
                }
            }
        }



        public void selectquality(XmlNode bonusNode)
        {
            XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
            List<ListItem> lstQualities = new List<ListItem>();
            string strForceValue = string.Empty;
            frmSelectItem frmPickItem = new frmSelectItem();
            foreach (XmlNode objXmlAddQuality in bonusNode.SelectNodes("quality"))
            {
                if (objXmlAddQuality.Attributes["select"] != null)
                    strForceValue = objXmlAddQuality.Attributes["select"].InnerText;
                bool blnAddQuality = _objCharacter.Qualities.All(objCharacterQuality => objCharacterQuality.Name != objXmlAddQuality.InnerText || objCharacterQuality.Extra != strForceValue);

                // Make sure the character does not yet have this Quality.

                if (blnAddQuality)
                {
                    XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlAddQuality.InnerText + "\"]");
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlQuality["name"].InnerText;
                    objItem.Name = objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"].InnerText;
                    lstQualities.Add(objItem);
                }
            }
            frmPickItem.GeneralItems = lstQualities;
            frmPickItem.ShowDialog();

            // Don't do anything else if the form was canceled.
            if (frmPickItem.DialogResult == DialogResult.Cancel)
                return;
            XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmPickItem.SelectedItem + "\"]");
            TreeNode objAddQualityNode = new TreeNode();
            Quality objAddQuality = new Quality(_objCharacter);

            bool blnFree = bonusNode.SelectSingleNode("quality[name = \"" + frmPickItem.SelectedItem + "\"]")?.Attributes?["contributetobp"] == null ||
                            (bonusNode.SelectSingleNode("quality[name = \"" + frmPickItem.SelectedItem + "\"]")?.Attributes?["contributetobp"]?.InnerText.ToLower() != "true");

            strForceValue = bonusNode.SelectSingleNode("quality[name = \"" + frmPickItem.SelectedItem + "\"]")?.Attributes?["select"].InnerText;
            objAddQuality.Create(objXmlSelectedQuality, _objCharacter, QualitySource.Selected, objAddQualityNode, null, null, strForceValue);
            if (blnFree)
            {
                objAddQuality.BP = 0;
                objAddQuality.ContributeToLimit = false;
            }
            _objCharacter.Qualities.Add(objAddQuality);
            CreateImprovement(objAddQuality.InternalId, Improvement.ImprovementSource.Quality, SourceName,
            Improvement.ImprovementType.SpecificQuality, _strUnique);
        }

        public void addskillspecialization(XmlNode bonusNode)
        {
            
            Skill objSkill = _objCharacter.SkillsSection.Skills.First(x => x.Name == bonusNode["skill"].InnerText);
            if (objSkill != null)
            {
                // Create the Improvement.
                Log.Info("Calling CreateImprovement");
                CreateImprovement(bonusNode["skill"].InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, bonusNode["spec"].InnerText);
                SkillSpecialization nspec = new SkillSpecialization(bonusNode["spec"].InnerText, true);
                objSkill.Specializations.Add(nspec);
            }
        }

        public void addskillspecializationoption(XmlNode bonusNode)
        {
            if (!(_objCharacter.Options.FreeMartialArtSpecialization && _objImprovementSource == Improvement.ImprovementSource.MartialArt)) return;
            var lstSkills = new List<Skill>();
            if (bonusNode["skills"] != null)
            {
                foreach (XmlNode objNode in bonusNode["skills"])
                {
                    var objSkill = _objCharacter.SkillsSection.Skills.First(x => x.Name == objNode.InnerText);
                    if (objSkill != null)
                    {
                        lstSkills.Add(objSkill);
                    }
                }
            }
            else
            {
                var objSkill = _objCharacter.SkillsSection.Skills.First(x => x.Name == bonusNode["skill"].InnerText);
                if (objSkill != null)
                {
                    lstSkills.Add(objSkill);
                }
            }
                
            if (lstSkills.Count == 0) return;
            foreach (var objSkill in lstSkills)
            {
                // Create the Improvement.
                Log.Info("Calling CreateImprovement");
                CreateImprovement(objSkill.Name, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillSpecialization, bonusNode["spec"].InnerText);
                SkillSpecialization nspec = new SkillSpecialization(bonusNode["spec"].InnerText, true);
                objSkill.Specializations.Add(nspec);
            }
        }
        
        public void spellkarmadiscount(XmlNode bonusNode)
        {
            Log.Info("spellkarmadiscount");
            Log.Info("spellkarmadiscount = " + bonusNode.OuterXml.ToString());
            Log.Info("Calling CreateImprovement");
            CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SpellKarmaDiscount, string.Empty,
                ValueToInt(bonusNode.InnerText, _intRating));
        }

        public void limitspellcategory(XmlNode bonusNode)
        {
            Log.Info("limitspellcategory");
            // Display the Select Spell window.
            frmSelectSpellCategory frmPickSpellCategory = new frmSelectSpellCategory();
            frmPickSpellCategory.Description = LanguageManager.Instance.GetString("Title_SelectSpellCategory");
            frmPickSpellCategory.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickSpellCategory.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }

            SelectedValue = frmPickSpellCategory.SelectedCategory;
            if (_blnConcatSelectedValue)
                SourceName += " (" + SelectedValue + ")";

            Log.Info("_strSelectedValue = " + SelectedValue);
            Log.Info("SourceName = " + SourceName);

            Log.Info("Calling CreateImprovement");
            CreateImprovement(frmPickSpellCategory.SelectedCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitSpellCategory,
                _strUnique);
        }

        public void limitspiritcategory(XmlNode bonusNode)
        {
            Log.Info("limitspiritcategory");
            XmlDocument spiritDoc = XmlManager.Instance.Load("traditions.xml");
            XmlNodeList xmlSpirits = spiritDoc.SelectNodes("/chummer/spirits/spirit");

            var spirits = (from XmlNode spirit in xmlSpirits
                select new ListItem
                {
                    Value = spirit["name"].InnerText, 
                    Name = spirit["translate"]?.InnerText ?? spirit["name"].InnerText
                }).ToList();

            frmSelectItem frmSelect = new frmSelectItem {GeneralItems = spirits};
            frmSelect.ShowDialog();
            
            if (frmSelect.DialogResult == DialogResult.Cancel)
            {
                throw new AbortedException();
            }
            CreateImprovement(frmSelect.SelectedItem, Improvement.ImprovementSource.Quality, SourceName,
    Improvement.ImprovementType.LimitSpiritCategory,"");
        }
        public void movementreplace(XmlNode bonusNode)
        {
            Log.Info("movementreplace");
            Log.Info("movementreplace = " + bonusNode.OuterXml);
            Log.Info("Calling CreateImprovement");

            Improvement.ImprovementType imp = Improvement.ImprovementType.WalkSpeed;
            switch (bonusNode["speed"].InnerText.ToLower())
            {
                case "walk":
                    imp = Improvement.ImprovementType.WalkSpeed;
                    break;
                case "run":
                    imp = Improvement.ImprovementType.RunSpeed;
                    break;
                case "sprint":
                    imp = Improvement.ImprovementType.SprintSpeed;
                    break;
            }

            CreateImprovement(bonusNode["category"].InnerText, _objImprovementSource, SourceName, imp, string.Empty,
                ValueToInt(bonusNode["val"].InnerText, _intRating));
        }
        #endregion
    }

    internal class AbortedException : Exception
    {
    }
}
