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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using NLog;

// ReSharper disable InconsistentNaming

namespace Chummer
{
    public class AddImprovementCollection
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly Character _objCharacter;

        public AddImprovementCollection(Character character, Improvement.ImprovementSource objImprovementSource, string sourceName, string strUnique, string forcedValue, string limitSelection, string selectedValue, string strFriendlyName, int intRating)
        {
            _objCharacter = character;
            _objImprovementSource = objImprovementSource;
            SourceName = sourceName;
            _strUnique = strUnique;
            ForcedValue = forcedValue;
            LimitSelection = limitSelection;
            SelectedValue = selectedValue;
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
        private readonly string _strFriendlyName;
        private readonly int _intRating;

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
        /// <param name="strImprovedName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
        /// <param name="strSourceName">Name of the item that grants this Improvement.</param>
        /// <param name="objImprovementType">Type of object the Improvement applies to.</param>
        /// <param name="strUnique">Name of the pool this Improvement should be added to - only the single highest value in the pool will be applied to the character.</param>
        /// <param name="decValue">Set a Value for the Improvement.</param>
        /// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
        /// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
        /// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
        /// <param name="decAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
        /// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
        /// <param name="blnAddToRating">Whether the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        private Improvement CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
            string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
            decimal decValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, decimal decAugmented = 0,
            int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false, string strTarget = "", string strCondition = "")
        {
            return ImprovementManager.CreateImprovement(_objCharacter, strImprovedName, objImprovementSource,
                strSourceName, objImprovementType, strUnique,
                decValue, intRating, intMinimum, intMaximum, decAugmented,
                intAugmentedMaximum, strExclude, blnAddToRating, strTarget, strCondition);
        }

        /// <summary>
        /// CreateImprovement overload method that takes only ImprovedName and ImprovementType, using default properties otherwise.
        /// </summary>
        /// <param name="selectedValue">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="improvementType">Type of object the Improvement applies to.</param>
        /// <returns></returns>
        private Improvement CreateImprovement(string selectedValue, Improvement.ImprovementType improvementType)
        {
            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = selectedValue;
            else
                SelectedValue += ", " + selectedValue; // Do not use localized value for space because we expect the selected value to be in English
            return CreateImprovement(selectedValue, _objImprovementSource, SourceName, improvementType, _strUnique);
        }

        /// <summary>
        /// CreateImprovement overload method that takes only ImprovedName, Target and ImprovementType, using default properties otherwise.
        /// </summary>
        /// <param name="strImprovementName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="improvementType">Type of object the Improvement applies to.</param>
        /// <returns></returns>
        private Improvement CreateImprovement(string strImprovementName, string strTarget,
            Improvement.ImprovementType improvementType)
        {
            return CreateImprovement(strImprovementName, _objImprovementSource, SourceName, improvementType, _strUnique,
                0, 1, 0, 0, 0, 0, string.Empty, false, strTarget);
        }

        #region Improvement Methods

#pragma warning disable IDE1006 // Naming Styles

        public void qualitylevel(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Get the level value from the bonus node
            int intLevel = ImprovementManager.ValueToInt(_objCharacter, bonusNode.InnerTextViaPool(), _intRating);

            // Get the quality group from the group attribute (e.g., "SINner", "TrustFund")
            string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool() ?? string.Empty;

            // Create an improvement that tracks this quality level
            CreateImprovement(strGroup, _objImprovementSource, SourceName,
                Improvement.ImprovementType.QualityLevel, _strUnique,
                intLevel, 1, 0, 0, 0, 0);
        }
        // Dummy Method for SelectText
        public static void selecttext(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
        }

        public void surprise(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Surprise, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void spellresistance(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpellResistance, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void mentalmanipulationresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MentalManipulationResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void physicalmanipulationresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalManipulationResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void manaillusionresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ManaIllusionResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void physicalillusionresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalIllusionResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void detectionspellresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DetectionSpellResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void directmanaspellresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DirectManaSpellResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void directphysicalspellresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DirectPhysicalSpellResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreasebodresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseBODResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreaseagiresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseAGIResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreaserearesist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseREAResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreasestrresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseSTRResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreasecharesist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseCHAResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreaseintresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseINTResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreaselogresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseLOGResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void decreasewilresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseWILResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void enableattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            switch (bonusNode["name"]?.InnerTextViaPool().ToUpperInvariant())
            {
                case "MAG":
                    CreateImprovement("MAG", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0);
                    _objCharacter.MAGEnabled = true;
                    break;

                case "RES":
                    CreateImprovement("RES", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0);
                    _objCharacter.RESEnabled = true;
                    break;

                case "DEP":
                    CreateImprovement("DEP", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0);
                    _objCharacter.DEPEnabled = true;
                    break;
            }
        }

        // Add an Attribute Replacement.
        public void replaceattributes(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList objXmlAttributes = bonusNode.SelectNodes("replaceattribute"))
            {
                if (objXmlAttributes == null)
                    return;
                foreach (XmlNode objXmlAttribute in objXmlAttributes)
                {
                    // Record the improvement.
                    string strAttribute = string.Empty;
                    if (objXmlAttribute.TryGetStringFieldQuickly("name", ref strAttribute))
                    {
                        // Extract the modifiers.
                        int intMin = 0;
                        int intMax = 0;
                        int intAugMax = 0;
                        objXmlAttribute.TryGetInt32FieldQuickly("min", ref intMin);
                        objXmlAttribute.TryGetInt32FieldQuickly("max", ref intMax);
                        objXmlAttribute.TryGetInt32FieldQuickly("aug", ref intAugMax);

                        CreateImprovement(strAttribute, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.ReplaceAttribute,
                            _strUnique, 0, 1, intMin, intMax, 0, intAugMax);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                    }
                }
            }
        }

        // Enable a special tab.
        public void enabletab(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            using (XmlNodeList xmlEnableList = bonusNode.SelectNodes("name"))
            {
                if (xmlEnableList?.Count > 0)
                {
                    foreach (XmlNode xmlEnable in xmlEnableList)
                    {
                        switch (xmlEnable.InnerTextViaPool().ToUpperInvariant())
                        {
                            case "MAGICIAN":
                                _objCharacter.MagicianEnabled = true;
                                CreateImprovement("Magician", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0);
                                break;

                            case "ADEPT":
                                _objCharacter.AdeptEnabled = true;
                                CreateImprovement("Adept", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab",
                                    0, 0);
                                break;

                            case "TECHNOMANCER":
                                _objCharacter.TechnomancerEnabled = true;
                                CreateImprovement("Technomancer", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0);
                                break;

                            case "ADVANCED PROGRAMS":
                                _objCharacter.AdvancedProgramsEnabled = true;
                                CreateImprovement("Advanced Programs", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0);
                                break;

                            case "CRITTER":
                                _objCharacter.CritterEnabled = true;
                                CreateImprovement("Critter", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0);
                                break;
                        }
                    }
                }
            }
        }

        // Disable a  tab.
        public void disabletab(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList xmlDisableList = bonusNode.SelectNodes("name"))
            {
                if (xmlDisableList?.Count > 0)
                {
                    foreach (XmlNode xmlDisable in xmlDisableList)
                    {
                        switch (xmlDisable.InnerTextViaPool().ToUpperInvariant())
                        {
                            case "CYBERWARE":
                                _objCharacter.CyberwareDisabled = true;
                                CreateImprovement("Cyberware", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "disabletab", 0, 0);
                                break;

                            case "INITIATION":
                                _objCharacter.InitiationForceDisabled = true;
                                CreateImprovement("Initiation", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "disabletab", 0, 0);
                                break;
                        }
                    }
                }
            }
        }

        // Select Restricted (select Restricted items for Fake Licenses).
        public void selectrestricted(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else
            {
                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                {
                    frmPickItem.MyForm.SetRestrictedMode(_objCharacter);
                    if (!string.IsNullOrEmpty(ForcedValue))
                        frmPickItem.MyForm.ForceItem(ForcedValue);

                    frmPickItem.MyForm.AllowAutoSelect = !string.IsNullOrEmpty(ForcedValue);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickItem.MyForm.SelectedName;
                }
            }

            // Create the Improvement.
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Restricted, _strUnique);
        }

        public void selecttradition(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else
            {
                // Populate the Magician Traditions list.
                XPathNavigator xmlTraditionsBaseChummerNode =
                    _objCharacter.LoadDataXPath("traditions.xml").SelectSingleNodeAndCacheExpression("/chummer");
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstTraditions))
                {
                    if (xmlTraditionsBaseChummerNode != null)
                    {
                        foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                     "traditions/tradition[" + _objCharacter.Settings.BookXPath() + "]"))
                        {
                            string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstTraditions.Add(new ListItem(
                                                      xmlTradition.SelectSingleNodeAndCacheExpression("id")?.Value
                                                      ?? strName,
                                                      xmlTradition.SelectSingleNodeAndCacheExpression("translate")
                                                                  ?.Value ?? strName));
                        }
                    }

                    if (lstTraditions.Count > 1)
                    {
                        lstTraditions.Sort(CompareListItems.CompareNames);
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                    {
                        AllowAutoSelect = false
                    }))
                    {
                        frmPickItem.MyForm.SetDropdownItemsMode(lstTraditions);
                        frmPickItem.MyForm.SelectedItem = _objCharacter.MagicTradition.SourceIDString;

                        // Make sure the dialogue window was not canceled.
                        if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = frmPickItem.MyForm.SelectedName;
                    }
                }
            }

            // Create the Improvement.
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Tradition,
                _strUnique);
        }

        public void cyberseeker(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            //Check if valid attrib
            string strBonusNodeText = bonusNode.InnerTextViaPool();
            if (strBonusNodeText == "BOX" || AttributeSection.AttributeStrings.Contains(strBonusNodeText))
            {
                CreateImprovement(strBonusNodeText, _objImprovementSource, SourceName, Improvement.ImprovementType.Seeker, _strUnique, 0, 0);
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        public void cyberlimbattributebonus(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            
            string strAttribute = bonusNode["name"]?.InnerTextViaPool() ?? string.Empty;
            decimal decValue = ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating);

            CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberlimbAttributeBonus, _strUnique, decValue, 0);
        }

        public void blockskillcategorydefaulting(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Expected values are either a Skill Name or an empty string.
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName,
                              Improvement.ImprovementType.BlockSkillCategoryDefault, _strUnique);
        }

        public void blockskillgroupdefaulting(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strExclude = bonusNode.Attributes?["excludecategory"]?.InnerTextViaPool() ?? string.Empty;
            string strSelect = bonusNode.InnerTextViaPool();
            if (string.IsNullOrEmpty(strSelect))
            {
                using (ThreadSafeForm<SelectSkillGroup> frmPickSkillGroup = ThreadSafeForm<SelectSkillGroup>.Get(
                           () => new SelectSkillGroup(_objCharacter)
                           {
                               Description = !string.IsNullOrEmpty(_strFriendlyName)
                                   ? string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString("String_Improvement_SelectSkillGroupName"),
                                                   _strFriendlyName)
                                   : LanguageManager.GetString("String_Improvement_SelectSkillGroup")
                           }))
                {
                    if (!string.IsNullOrEmpty(ForcedValue))
                    {
                        frmPickSkillGroup.MyForm.OnlyGroup = ForcedValue;
                        frmPickSkillGroup.MyForm.Opacity = 0;
                    }

                    if (!string.IsNullOrEmpty(strExclude))
                        frmPickSkillGroup.MyForm.ExcludeCategory = strExclude;

                    // Make sure the dialogue window was not canceled.
                    if (frmPickSkillGroup.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelect = frmPickSkillGroup.MyForm.SelectedSkillGroup;
                }
            }

            SelectedValue = strSelect;

            CreateImprovement(strSelect, _objImprovementSource, SourceName,
                              Improvement.ImprovementType.BlockSkillGroupDefault, _strUnique, 0, 0, 0, 1, 0, 0, strExclude);
        }

        public void blockskilldefaulting(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSelectedSkill = bonusNode.InnerTextViaPool();
            if (string.IsNullOrEmpty(strSelectedSkill))
            {
                string strForcedValue = ForcedValue;
                strSelectedSkill = string.IsNullOrEmpty(strForcedValue)
                    ? ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName).Item1
                    : strForcedValue;
            }
            // Expected values are either a Skill Name or an empty string.
            CreateImprovement(strSelectedSkill, _objImprovementSource, SourceName,
                              Improvement.ImprovementType.BlockSkillDefault, _strUnique);
        }

        public void allowskilldefaulting(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Expected values are either a Skill Name or an empty string.
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.AllowSkillDefault, _strUnique);
        }

        // Select a Skill.
        public void selectskill(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            if (ForcedValue == "+2 to a Combat Skill")
                ForcedValue = string.Empty;

            (string strSelectedSkill, bool blnIsKnowledgeSkill) = ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName);

            bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;

            SelectedValue = strSelectedSkill;

            string strVal = bonusNode["val"]?.InnerTextViaPool();
            string strMax = bonusNode["max"]?.InnerTextViaPool();
            bool blnDisableSpec = false;
            bonusNode.TryGetBoolFieldQuickly("disablespecializationeffects", ref blnDisableSpec);
            // Find the selected Skill.
            if (blnIsKnowledgeSkill)
            {
                if (_objCharacter.SkillsSection.KnowledgeSkills.Any(k => k.DictionaryKey == strSelectedSkill))
                {
                    _objCharacter.SkillsSection.KnowledgeSkills.ForEach(objKnowledgeSkill =>
                    {
                        string strName = objKnowledgeSkill.DictionaryKey;
                        if (strName != strSelectedSkill)
                            return;
                        // We've found the selected Skill.
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            CreateImprovement(strName, _objImprovementSource, SourceName,
                                              Improvement.ImprovementType.Skill, _strUnique,
                                              ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating), 1, 0, 0,
                                              0, 0, string.Empty, blnAddToRating);
                        }

                        if (blnDisableSpec)
                        {
                            CreateImprovement(strName, _objImprovementSource, SourceName,
                                              Improvement.ImprovementType.DisableSpecializationEffects, _strUnique);
                        }

                        if (!string.IsNullOrEmpty(strMax))
                        {
                            CreateImprovement(strName, _objImprovementSource, SourceName,
                                              Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
                                              ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0,
                                              string.Empty, blnAddToRating);
                        }
                    });
                }
                else
                {
                    bool blnAllowUpgrade = true;
                    bonusNode.TryGetBoolFieldQuickly("disableupgrades", ref blnAllowUpgrade);
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(_objCharacter, strSelectedSkill, blnAllowUpgrade);
                    try
                    {
                        _objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                        // We've found the selected Skill.
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            CreateImprovement(objKnowledgeSkill.DictionaryKey, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating), 1, 0, 0, 0, 0, string.Empty,
                                blnAddToRating);
                        }

                        if (blnDisableSpec)
                        {
                            CreateImprovement(objKnowledgeSkill.DictionaryKey, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.DisableSpecializationEffects,
                                _strUnique);
                        }

                        if (!string.IsNullOrEmpty(strMax))
                        {
                            CreateImprovement(objKnowledgeSkill.DictionaryKey, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                                blnAddToRating);
                        }
                    }
                    catch
                    {
                        objKnowledgeSkill.Remove();
                        throw;
                    }
                }
            }
            else if (ExoticSkill.IsExoticSkillName(_objCharacter, strSelectedSkill))
            {
                if (!string.IsNullOrEmpty(strVal))
                {
                    // Make sure we have the exotic skill in the list if we're altering any values
                    Skill objExistingSkill = _objCharacter.SkillsSection.GetActiveSkill(strSelectedSkill);
                    if (objExistingSkill?.IsExoticSkill != true)
                    {
                        string strSkillName = strSelectedSkill;
                        int intParenthesesIndex = strSkillName.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex > 0)
                        {
                            string strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                            strSkillName = strSkillName.Substring(0, intParenthesesIndex);
                            _objCharacter.SkillsSection.AddExoticSkill(strSkillName, strSkillSpecific);
                        }
                    }
                    CreateImprovement(strSelectedSkill, _objImprovementSource,
                        SourceName,
                        Improvement.ImprovementType.Skill, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating), 1,
                        0, 0, 0, 0, string.Empty, blnAddToRating);
                }

                if (blnDisableSpec)
                {
                    CreateImprovement(strSelectedSkill, _objImprovementSource,
                        SourceName,
                        Improvement.ImprovementType.DisableSpecializationEffects,
                        _strUnique);
                }

                if (!string.IsNullOrEmpty(strMax))
                {
                    CreateImprovement(strSelectedSkill, _objImprovementSource,
                        SourceName,
                        Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
                        ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                        blnAddToRating);
                }
            }
            else
            {
                // Add in improvements even if we have matching skill in case the matching skill gets added later.
                if (!string.IsNullOrEmpty(strVal))
                {
                    CreateImprovement(strSelectedSkill, _objImprovementSource, SourceName,
                                      Improvement.ImprovementType.Skill,
                                      _strUnique,
                                      ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating), 1, 0, 0, 0, 0,
                                      string.Empty,
                                      blnAddToRating);
                }

                if (blnDisableSpec)
                {
                    CreateImprovement(strSelectedSkill, _objImprovementSource, SourceName,
                                      Improvement.ImprovementType.DisableSpecializationEffects,
                                      _strUnique);
                }

                if (!string.IsNullOrEmpty(strMax))
                {
                    CreateImprovement(strSelectedSkill, _objImprovementSource, SourceName,
                                      Improvement.ImprovementType.Skill,
                                      _strUnique,
                                      0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strMax, _intRating), 0, 0, string.Empty,
                                      blnAddToRating);
                }
            }
        }

        // Select a Skill Group.
        public void selectskillgroup(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strExclude = string.Empty;
            if (bonusNode.Attributes?["excludecategory"] != null)
                strExclude = bonusNode.Attributes["excludecategory"].InnerTextViaPool();

            using (ThreadSafeForm<SelectSkillGroup> frmPickSkillGroup = ThreadSafeForm<SelectSkillGroup>.Get(() => new SelectSkillGroup(_objCharacter)
            {
                Description = !string.IsNullOrEmpty(_strFriendlyName)
                           ? string.Format(GlobalSettings.CultureInfo,
                               LanguageManager.GetString("String_Improvement_SelectSkillGroupName"), _strFriendlyName)
                           : LanguageManager.GetString("String_Improvement_SelectSkillGroup")
            }))
            {
                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    frmPickSkillGroup.MyForm.OnlyGroup = ForcedValue;
                    frmPickSkillGroup.MyForm.Opacity = 0;
                }

                if (!string.IsNullOrEmpty(strExclude))
                    frmPickSkillGroup.MyForm.ExcludeCategory = strExclude;

                // Make sure the dialogue window was not canceled.
                if (frmPickSkillGroup.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickSkillGroup.MyForm.SelectedSkillGroup;
            }

            bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;

            string strBonus = bonusNode["bonus"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, ImprovementManager.ValueToDec(_objCharacter, strBonus, _intRating), 1, 0, 0, 0, 0, strExclude,
                    blnAddToRating);
            }
            else
            {
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, 0, 0, 0, 1, 0, 0, strExclude,
                    blnAddToRating);
            }
        }

        public void selectattributes(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            bool blnSingleSelected = true;
            List<string> selectedValues = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlSelectAttributeList = bonusNode.SelectNodes("selectattribute"))
            {
                if (xmlSelectAttributeList != null)
                {
                    foreach (XmlNode objXmlAttribute in xmlSelectAttributeList)
                    {
                        List<string> lstAbbrevs = new List<string>(xmlSelectAttributeList.Count);
                        using (XmlNodeList xmlAttributeList = objXmlAttribute.SelectNodes("attribute"))
                        {
                            if (xmlAttributeList?.Count > 0)
                            {
                                foreach (XmlNode objSubNode in xmlAttributeList)
                                    lstAbbrevs.Add(objSubNode.InnerTextViaPool());
                            }
                            else
                            {
                                lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                                using (XmlNodeList xmlAttributeList2 = objXmlAttribute.SelectNodes("excludeattribute"))
                                {
                                    if (xmlAttributeList2?.Count > 0)
                                    {
                                        foreach (XmlNode objSubNode in xmlAttributeList2)
                                            lstAbbrevs.Remove(objSubNode.InnerTextViaPool());
                                    }
                                }
                            }
                        }

                        lstAbbrevs.Remove("ESS");
                        if (!_objCharacter.MAGEnabled)
                        {
                            lstAbbrevs.Remove("MAG");
                            lstAbbrevs.Remove("MAGAdept");
                        }
                        else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
                            lstAbbrevs.Remove("MAGAdept");

                        if (!_objCharacter.RESEnabled)
                            lstAbbrevs.Remove("RES");
                        if (!_objCharacter.DEPEnabled)
                            lstAbbrevs.Remove("DEP");

                        // Check to see if there is only one possible selection because of _strLimitSelection.
                        if (!string.IsNullOrEmpty(ForcedValue))
                            LimitSelection = ForcedValue;

                        if (!string.IsNullOrEmpty(LimitSelection))
                        {
                            lstAbbrevs.RemoveAll(x => x != LimitSelection);
                        }

                        // Display the Select Attribute window and record which Skill was selected.
                        string strSelected;
                        switch (lstAbbrevs.Count)
                        {
                            case 0:
                                throw new AbortedException();
                            case 1:
                                strSelected = lstAbbrevs[0];
                                break;

                            default:
                            {
                                using (ThreadSafeForm<SelectAttribute> frmPickAttribute
                                       = ThreadSafeForm<SelectAttribute>.Get(
                                           () => new SelectAttribute(lstAbbrevs.ToArray())
                                           {
                                               Description = !string.IsNullOrEmpty(_strFriendlyName)
                                                   ? string.Format(GlobalSettings.CultureInfo,
                                                                   LanguageManager.GetString(
                                                                       "String_Improvement_SelectAttributeNamed"),
                                                                   _strFriendlyName)
                                                   : LanguageManager.GetString("String_Improvement_SelectAttribute")
                                           }))
                                {
                                    // Make sure the dialogue window was not canceled.
                                    if (frmPickAttribute.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                    {
                                        throw new AbortedException();
                                    }

                                    strSelected = frmPickAttribute.MyForm.SelectedAttribute;

                                    break;
                                }
                            }
                        }

                        if (blnSingleSelected && selectedValues.Count > 0 && !selectedValues.Contains(strSelected))
                            blnSingleSelected = false;
                        selectedValues.Add(strSelected);

                        // Record the improvement.
                        int intMin = 0;
                        int intAug = 0;
                        int intMax = 0;
                        int intAugMax = 0;

                        // Extract the modifiers.
                        string strTemp = objXmlAttribute["min"]?.InnerTextViaPool();
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intMin);
                        strTemp = objXmlAttribute["val"]?.InnerTextViaPool();
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intAug);
                        strTemp = objXmlAttribute["max"]?.InnerTextViaPool();
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intMax);
                        strTemp = objXmlAttribute["aug"]?.InnerTextViaPool();
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intAugMax);

                        string strAttribute = strSelected;

                        if (objXmlAttribute["affectbase"] != null)
                            strAttribute += "Base";

                        CreateImprovement(strAttribute, _objImprovementSource, SourceName,
                                          Improvement.ImprovementType.Attribute,
                                          _strUnique,
                                          0, 1, intMin, intMax, intAug, intAugMax);
                    }
                }
            }

            if (blnSingleSelected)
            {
                SelectedValue = selectedValues.FirstOrDefault();
            }
            else
            {
                string strSpace = LanguageManager.GetString("String_Space");
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sdbValue))
                {
                    foreach (string s in AttributeSection.AttributeStrings)
                    {
                        int i = selectedValues.Count(c => c == s);
                        if (i <= 0)
                            continue;
                        if (sdbValue.Length > 0)
                        {
                            sdbValue.Append(',', strSpace);
                        }

                        sdbValue.AppendFormat(GlobalSettings.CultureInfo, "{0}{1}({2})", s, strSpace, i);
                    }

                    SelectedValue = sdbValue.ToString();
                }
            }
        }

        // Select an CharacterAttribute.
        public void selectattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute"))
            {
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Add(objSubNode.InnerTextViaPool());
                }
                else
                {
                    lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                    using (XmlNodeList xmlAttributeList2 = bonusNode.SelectNodes("excludeattribute"))
                    {
                        if (xmlAttributeList2?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList2)
                                lstAbbrevs.Remove(objSubNode.InnerTextViaPool());
                        }
                    }
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!_objCharacter.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!_objCharacter.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!_objCharacter.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                lstAbbrevs.RemoveAll(x => x != LimitSelection);
            }

            string strSelected;
            switch (lstAbbrevs.Count)
            {
                case 0:
                    throw new AbortedException();
                case 1:
                    strSelected = lstAbbrevs[0];
                    break;

                default:
                {
                    // Display the Select Attribute window and record which Skill was selected.
                    using (ThreadSafeForm<SelectAttribute> frmPickAttribute = ThreadSafeForm<SelectAttribute>.Get(
                               () => new SelectAttribute(lstAbbrevs.ToArray())
                               {
                                   Description = !string.IsNullOrEmpty(_strFriendlyName)
                                       ? string.Format(GlobalSettings.CultureInfo,
                                                       LanguageManager.GetString("String_Improvement_SelectAttributeNamed"),
                                                       _strFriendlyName)
                                       : LanguageManager.GetString("String_Improvement_SelectAttribute")
                               }))
                    {
                        // Make sure the dialogue window was not canceled.
                        if (frmPickAttribute.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        strSelected = frmPickAttribute.MyForm.SelectedAttribute;
                    }

                    break;
                }
            }

            SelectedValue = strSelected;

            // Record the improvement.
            int intMin = 0;
            decimal decAug = 0;
            int intMax = 0;
            int intAugMax = 0;

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                intMin = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["val"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                decAug = ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["max"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["aug"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);

            string strAttribute = strSelected;

            if (bonusNode["affectbase"] != null)
                strAttribute += "Base";

            CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                _strUnique,
                0, 1, intMin, intMax, decAug, intAugMax);
        }

        // Select a Limit.
        public void selectlimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> strLimits = new List<string>(4);
            using (XmlNodeList xmlDefinedLimits = bonusNode.SelectNodes("limit"))
            {
                if (xmlDefinedLimits?.Count > 0)
                {
                    foreach (XmlNode objXmlAttribute in xmlDefinedLimits)
                        strLimits.Add(objXmlAttribute.InnerTextViaPool());
                }
                else
                {
                    strLimits.Add("Physical");
                    strLimits.Add("Mental");
                    strLimits.Add("Social");
                }
            }

            using (XmlNodeList xmlExcludeLimits = bonusNode.SelectNodes("excludelimit"))
            {
                if (xmlExcludeLimits?.Count > 0)
                {
                    foreach (XmlNode objXmlAttribute in xmlExcludeLimits)
                    {
                        strLimits.Remove(objXmlAttribute.InnerTextViaPool());
                    }
                }
            }

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                strLimits.RemoveAll(x => x != LimitSelection);
            }

            // Display the Select Limit window and record which Limit was selected.
            using (ThreadSafeForm<SelectLimit> frmPickLimit = ThreadSafeForm<SelectLimit>.Get(() => new SelectLimit(strLimits.ToArray())
            {
                Description = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_Improvement_SelectLimitNamed"), _strFriendlyName)
                    : LanguageManager.GetString("String_Improvement_SelectLimit")
            }))
            {
                // Make sure the dialogue window was not canceled.
                if (frmPickLimit.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                // Record the improvement.
                int intMin = 0;
                decimal decAug = 0;
                int intMax = 0;
                int intAugMax = 0;

                // Extract the modifiers.
                string strTemp = bonusNode["min"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    intMin = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
                strTemp = bonusNode["val"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    decAug = ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating);
                strTemp = bonusNode["max"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
                strTemp = bonusNode["aug"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    intAugMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);

                string strLimit = frmPickLimit.MyForm.SelectedLimit;

                // string strBonus = bonusNode["value"].InnerTextViaPool();
                Improvement.ImprovementType eType;

                switch (strLimit.ToUpperInvariant())
                {
                    case "MENTAL":
                        {
                            eType = Improvement.ImprovementType.MentalLimit;
                            break;
                        }
                    case "SOCIAL":
                        {
                            eType = Improvement.ImprovementType.SocialLimit;
                            break;
                        }
                    case "PHYSICAL":
                        {
                            eType = Improvement.ImprovementType.PhysicalLimit;
                            break;
                        }
                    default:
                        throw new AbortedException();
                }

                SelectedValue = frmPickLimit.MyForm.SelectedDisplayLimit;

                if (bonusNode["affectbase"] != null)
                    strLimit += "Base";

                CreateImprovement(strLimit, _objImprovementSource, SourceName, eType, _strUnique, decAug, 0, intMin,
                    intMax,
                    decAug, intAugMax);
            }
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public void swapskillattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute"))
            {
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Add(objSubNode.InnerTextViaPool());
                }
                else
                {
                    lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                    using (XmlNodeList xmlAttributeList2 = bonusNode.SelectNodes("excludeattribute"))
                    {
                        if (xmlAttributeList2?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList2)
                                lstAbbrevs.Remove(objSubNode.InnerTextViaPool());
                        }
                    }
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!_objCharacter.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!_objCharacter.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!_objCharacter.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            if (lstAbbrevs.Count == 1)
                LimitSelection = lstAbbrevs[0];

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                SelectedValue = LimitSelection;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                using (ThreadSafeForm<SelectAttribute> frmPickAttribute = ThreadSafeForm<SelectAttribute>.Get(() => new SelectAttribute(lstAbbrevs.ToArray())
                {
                    Description = !string.IsNullOrEmpty(_strFriendlyName)
                        ? string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_Improvement_SelectAttributeNamed"), _strFriendlyName)
                        : LanguageManager.GetString("String_Improvement_SelectAttribute")
                }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickAttribute.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickAttribute.MyForm.SelectedAttribute;
                }
            }

            string strLimitToSkill = bonusNode["limittoskill"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strLimitToSkill))
            {
                SelectedTarget = strLimitToSkill;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                using (ThreadSafeForm<SelectSkill> frmPickSkill = ThreadSafeForm<SelectSkill>.Get(() =>
                           new SelectSkill(_objCharacter)
                           {
                               Description = !string.IsNullOrEmpty(_strFriendlyName)
                                   ? string.Format(GlobalSettings.CultureInfo,
                                       LanguageManager.GetString("String_Improvement_SelectSkillNamed"),
                                       _strFriendlyName)
                                   : LanguageManager.GetString("String_Improvement_SelectSkill")
                           }))
                {
                    string strTemp = bonusNode["skillgroup"]?.InnerTextViaPool();
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.OnlySkillGroup = strTemp;
                    else
                    {
                        XmlElement xmlSkillCategories = bonusNode["skillcategories"];
                        if (xmlSkillCategories != null)
                            frmPickSkill.MyForm.LimitToCategories = xmlSkillCategories;
                        else
                        {
                            strTemp = bonusNode["skillcategory"]?.InnerTextViaPool();
                            if (!string.IsNullOrEmpty(strTemp))
                                frmPickSkill.MyForm.OnlyCategory = strTemp;
                            else
                            {
                                strTemp = bonusNode["excludecategory"]?.InnerTextViaPool();
                                if (!string.IsNullOrEmpty(strTemp))
                                    frmPickSkill.MyForm.ExcludeCategory = strTemp;
                                else
                                {
                                    strTemp = bonusNode["limittoattribute"]?.InnerTextViaPool();
                                    if (!string.IsNullOrEmpty(strTemp))
                                        frmPickSkill.MyForm.LinkedAttribute = strTemp;
                                }
                            }
                        }
                    }

                    // Make sure the dialogue window was not canceled.
                    if (frmPickSkill.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedTarget = frmPickSkill.MyForm.SelectedSkill;
                }
            }

            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SwapSkillAttribute, _strUnique,
                0, 1, 0, 0, 0, 0, string.Empty, false, SelectedTarget);
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public void swapskillspecattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute"))
            {
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Add(objSubNode.InnerTextViaPool());
                }
                else
                {
                    lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                    using (XmlNodeList xmlAttributeList2 = bonusNode.SelectNodes("excludeattribute"))
                    {
                        if (xmlAttributeList2?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList2)
                                lstAbbrevs.Remove(objSubNode.InnerTextViaPool());
                        }
                    }
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!_objCharacter.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!_objCharacter.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!_objCharacter.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            if (lstAbbrevs.Count == 1)
                LimitSelection = lstAbbrevs[0];

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                SelectedValue = LimitSelection;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                using (ThreadSafeForm<SelectAttribute> frmPickAttribute = ThreadSafeForm<SelectAttribute>.Get(() =>
                           new SelectAttribute(lstAbbrevs.ToArray())
                           {
                               Description = !string.IsNullOrEmpty(_strFriendlyName)
                                   ? string.Format(GlobalSettings.CultureInfo,
                                       LanguageManager.GetString("String_Improvement_SelectAttributeNamed"),
                                       _strFriendlyName)
                                   : LanguageManager.GetString("String_Improvement_SelectAttribute")
                           }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickAttribute.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickAttribute.MyForm.SelectedAttribute;
                }
            }

            string strLimitToSkill = bonusNode["limittoskill"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strLimitToSkill))
            {
                SelectedTarget = strLimitToSkill;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                using (ThreadSafeForm<SelectSkill> frmPickSkill = ThreadSafeForm<SelectSkill>.Get(() =>
                           new SelectSkill(_objCharacter)
                           {
                               Description = !string.IsNullOrEmpty(_strFriendlyName)
                                   ? string.Format(GlobalSettings.CultureInfo,
                                       LanguageManager.GetString("String_Improvement_SelectSkillNamed"),
                                       _strFriendlyName)
                                   : LanguageManager.GetString("String_Improvement_SelectSkill")
                           }))
                {
                    string strTemp = bonusNode["skillgroup"]?.InnerTextViaPool();
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.OnlySkillGroup = strTemp;
                    else
                    {
                        XmlElement xmlSkillCategories = bonusNode["skillcategories"];
                        if (xmlSkillCategories != null)
                            frmPickSkill.MyForm.LimitToCategories = xmlSkillCategories;
                        else
                        {
                            strTemp = bonusNode["skillcategory"]?.InnerTextViaPool();
                            if (!string.IsNullOrEmpty(strTemp))
                                frmPickSkill.MyForm.OnlyCategory = strTemp;
                            else
                            {
                                strTemp = bonusNode["excludecategory"]?.InnerTextViaPool();
                                if (!string.IsNullOrEmpty(strTemp))
                                    frmPickSkill.MyForm.ExcludeCategory = strTemp;
                                else
                                {
                                    strTemp = bonusNode["limittoattribute"]?.InnerTextViaPool();
                                    if (!string.IsNullOrEmpty(strTemp))
                                        frmPickSkill.MyForm.LinkedAttribute = strTemp;
                                }
                            }
                        }
                    }

                    // Make sure the dialogue window was not canceled.
                    if (frmPickSkill.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedTarget = frmPickSkill.MyForm.SelectedSkill;
                }
            }

            // TODO: Allow selection of specializations through frmSelectSkillSpec
            string strSpec = bonusNode["spec"]?.InnerTextViaPool() ?? string.Empty;

            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SwapSkillSpecAttribute, _strUnique,
                0, 1, 0, 0, 0, 0, strSpec, false, SelectedTarget);
        }

        // Select a Spell.
        public void selectspell(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlNode node;
            // Display the Select Spell window.
            using (ThreadSafeForm<SelectSpell> frmPickSpell = ThreadSafeForm<SelectSpell>.Get(() => new SelectSpell(_objCharacter)))
            {
                string strCategory = bonusNode.Attributes?["category"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strCategory))
                    frmPickSpell.MyForm.LimitCategory = strCategory;

                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    frmPickSpell.MyForm.ForceSpellName = ForcedValue;
                    frmPickSpell.MyForm.Opacity = 0;
                }

                frmPickSpell.MyForm.IgnoreRequirements = bonusNode.Attributes?["ignorerequirements"]?.InnerTextIsTrueString() == true;

                // Make sure the dialogue window was not canceled.
                if (frmPickSpell.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                // Open the Spells XML file and locate the selected piece.
                XmlDocument objXmlDocument = _objCharacter.LoadData("spells.xml");

                node = objXmlDocument.TryGetNodeByNameOrId("/chummer/spells/spell",
                    frmPickSpell.MyForm.SelectedSpell);
            }

            if (node == null)
                throw new AbortedException();

            SelectedValue = node["name"]?.InnerTextViaPool();

            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = node.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext");
            if (xmlSelectText != null)
            {
                using (ThreadSafeForm<SelectText> frmPickText = ThreadSafeForm<SelectText>.Get(() => new SelectText
                       {
                           Description = string.Format(GlobalSettings.CultureInfo,
                               LanguageManager.GetString("String_Improvement_SelectText"),
                               node["translate"]?.InnerTextViaPool() ?? node["name"]?.InnerTextViaPool())
                       }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        throw new AbortedException();

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            Spell spell = new Spell(_objCharacter);
            try
            {
                spell.Create(node, strExtra);
                if (spell.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                spell.Grade = -1;
                _objCharacter.Spells.Add(spell);

                CreateImprovement(spell.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Spell,
                    _strUnique);
            }
            catch
            {
                spell.Remove(false);
                throw;
            }
        }

        // Add a specific Spell to the Character.
        public void addspell(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlDocument objXmlSpellDocument = _objCharacter.LoadData("spells.xml");

            XmlNode node = objXmlSpellDocument.TryGetNodeByNameOrId("/chummer/spells/spell", bonusNode.InnerTextViaPool()) ?? throw new AbortedException();
            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = node.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext");
            if (xmlSelectText != null)
            {
                using (ThreadSafeForm<SelectText> frmPickText = ThreadSafeForm<SelectText>.Get(() => new SelectText
                       {
                           Description = string.Format(GlobalSettings.CultureInfo,
                               LanguageManager.GetString("String_Improvement_SelectText"),
                               node["translate"]?.InnerTextViaPool() ?? node["name"]?.InnerTextViaPool())
                       }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            Spell spell = new Spell(_objCharacter);
            try
            {
                spell.Create(node, strExtra);
                if (spell.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                spell.Alchemical = bonusNode.Attributes?["alchemical"]?.InnerTextIsTrueString() == true;
                spell.Extended = bonusNode.Attributes?["extended"]?.InnerTextIsTrueString() == true;
                spell.Limited = bonusNode.Attributes?["limited"]?.InnerTextIsTrueString() == true;
                spell.BarehandedAdept = bonusNode.Attributes?["barehandedadept"]?.InnerTextIsTrueString() == true || bonusNode.Attributes?["usesunarmed"]?.InnerTextIsTrueString() == true;
                spell.Grade = -1;
                _objCharacter.Spells.Add(spell);

                CreateImprovement(spell.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Spell,
                    _strUnique);
            }
            catch
            {
                spell.Remove(false);
                throw;
            }
        }

        // Select a Complex Form.
        public void selectcomplexform(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedComplexForm = ForcedValue;

            if (string.IsNullOrEmpty(strSelectedComplexForm))
            {
                // Display the Select ComplexForm window.
                using (ThreadSafeForm<SelectComplexForm> frmPickComplexForm = ThreadSafeForm<SelectComplexForm>.Get(() => new SelectComplexForm(_objCharacter)))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickComplexForm.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelectedComplexForm = frmPickComplexForm.MyForm.SelectedComplexForm;
                }
            }

            // Open the ComplexForms XML file and locate the selected piece.
            XmlDocument objXmlDocument = _objCharacter.LoadData("complexforms.xml");

            XmlNode node = objXmlDocument.TryGetNodeByNameOrId("/chummer/complexforms/complexforms", strSelectedComplexForm)
                           ?? throw new AbortedException();

            SelectedValue = node["name"]?.InnerTextViaPool();

            ComplexForm objComplexform = new ComplexForm(_objCharacter);
            try
            {
                objComplexform.Create(node);
                if (objComplexform.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                objComplexform.Grade = -1;

                _objCharacter.ComplexForms.Add(objComplexform);

                CreateImprovement(objComplexform.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.ComplexForm,
                    _strUnique);
            }
            catch
            {
                objComplexform.Remove(false);
                throw;
            }
        }

        // Add a specific ComplexForm to the Character.
        public void addcomplexform(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlDocument objXmlComplexFormDocument = _objCharacter.LoadData("complexforms.xml");

            XmlNode node = objXmlComplexFormDocument.TryGetNodeByNameOrId("/chummer/complexforms/complexform", bonusNode.InnerTextViaPool()) ?? throw new AbortedException();

            ComplexForm objComplexform = new ComplexForm(_objCharacter);
            try
            {
                objComplexform.Create(node);
                if (objComplexform.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                objComplexform.Grade = -1;

                _objCharacter.ComplexForms.Add(objComplexform);

                CreateImprovement(objComplexform.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.ComplexForm,
                    _strUnique);
            }
            catch
            {
                objComplexform.Remove(false);
                throw;
            }
        }

        // Add a specific Gear to the Character.
        public void addgear(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            Gear objNewGear = Purchase(bonusNode);
            using (XmlNodeList xmlChildren = bonusNode["children"]?.ChildNodes)
            {
                if (xmlChildren?.Count > 0)
                {
                    foreach (XmlNode xmlChildNode in xmlChildren)
                    {
                        Purchase(xmlChildNode, objNewGear);
                    }
                }
            }

            Gear Purchase(XmlNode xmlGearNode, Gear objParent = null)
            {
                string strName = xmlGearNode["name"]?.InnerTextViaPool() ?? string.Empty;
                string strCategory = xmlGearNode["category"]?.InnerTextViaPool() ?? string.Empty;
                string strFilter = "/chummer/gears/gear";
                if (!string.IsNullOrEmpty(strName) || !string.IsNullOrEmpty(strCategory))
                {
                    strFilter += "[";
                    if (!string.IsNullOrEmpty(strName))
                    {
                        strFilter += "name = " + strName.CleanXPath();
                        if (!string.IsNullOrEmpty(strCategory))
                            strFilter += " and category = " + strCategory.CleanXPath();
                    }
                    else
                        strFilter += "category = " + strCategory.CleanXPath();
                    strFilter += "]";
                }

                XmlNode xmlGearDataNode = _objCharacter.LoadData("gear.xml").SelectSingleNode(strFilter) ?? throw new AbortedException();
                int intRating = 0;
                string strTemp = string.Empty;
                if (xmlGearNode.TryGetStringFieldQuickly("rating", ref strTemp))
                    intRating = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
                decimal decQty = 1.0m;
                if (xmlGearNode["quantity"] != null)
                    decQty = ImprovementManager.ValueToDec(_objCharacter, xmlGearNode["quantity"].InnerTextViaPool(), _intRating);

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>(1);

                Gear objNewGearToCreate = new Gear(_objCharacter);
                try
                {
                    objNewGearToCreate.Create(xmlGearDataNode, intRating, lstWeapons, ForcedValue);

                    if (objNewGearToCreate.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    objNewGearToCreate.Quantity = decQty;

                    // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                    if (_objCharacter.ActiveCommlink == null && objNewGearToCreate.IsCommlink)
                    {
                        objNewGearToCreate.SetActiveCommlink(_objCharacter, true);
                    }

                    if (xmlGearNode["fullcost"] == null)
                        objNewGearToCreate.Cost = "0";
                    // Create any Weapons that came with this Gear.
                    foreach (Weapon objWeapon in lstWeapons)
                        _objCharacter.Weapons.Add(objWeapon);

                    objNewGearToCreate.ParentID = SourceName;
                    if (objParent != null)
                    {
                        objParent.Children.Add(objNewGearToCreate);
                        objNewGearToCreate.Parent = objParent;
                    }
                    else
                    {
                        _objCharacter.Gear.Add(objNewGearToCreate);
                    }

                    CreateImprovement(objNewGearToCreate.InternalId, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Gear,
                        _strUnique);
                    return objNewGearToCreate;
                }
                catch
                {
                    objNewGearToCreate.DeleteGear();
                    throw;
                }
            }
        }

        // Add a specific Gear to the Character.
        public void addweapon(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strName = bonusNode["name"]?.InnerTextViaPool() ?? throw new AbortedException();
            XmlNode node = _objCharacter.LoadData("weapons.xml").TryGetNodeByNameOrId("/chummer/weapons/weapon", strName) ?? throw new AbortedException();

            // Create the new piece of Gear.
            List<Weapon> lstWeapons = new List<Weapon>(1);

            Weapon objNewWeapon = new Weapon(_objCharacter);
            try
            {
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

                CreateImprovement(objNewWeapon.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Weapon,
                    _strUnique);
            }
            catch
            {
                objNewWeapon.DeleteWeapon();
                throw;
            }
        }

        // Add a specific Gear to the Character.
        public void naturalweapon(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            Weapon objWeapon = new Weapon(_objCharacter);
            try
            {
                objWeapon.Name = bonusNode["name"]?.InnerTextViaPool() ?? _strFriendlyName;
                objWeapon.Category = LanguageManager.GetString("Tab_Critter", GlobalSettings.DefaultLanguage);
                objWeapon.RangeType = "Melee";
                objWeapon.Reach = bonusNode["reach"]?.InnerTextViaPool() ?? "0";
                objWeapon.Accuracy = bonusNode["accuracy"]?.InnerTextViaPool() ?? "Physical";
                objWeapon.Damage = bonusNode["damage"]?.InnerTextViaPool() ?? "({STR})S";
                objWeapon.AP = bonusNode["ap"]?.InnerTextViaPool() ?? "0";
                objWeapon.Mode = "0";
                objWeapon.RC = "0";
                objWeapon.Concealability = "0";
                objWeapon.Avail = "0";
                objWeapon.Cost = "0";
                objWeapon.Ammo = "0";
                objWeapon.UseSkill = bonusNode["useskill"]?.InnerTextViaPool() ?? string.Empty;
                objWeapon.Source = bonusNode["source"]?.InnerTextViaPool() ?? "SR5";
                objWeapon.Page = bonusNode["page"]?.InnerTextViaPool() ?? "0";
                objWeapon.ParentID = SourceName;
                objWeapon.CreateClips();

                _objCharacter.Weapons.Add(objWeapon);

                CreateImprovement(objWeapon.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Weapon,
                    _strUnique);
            }
            catch
            {
                objWeapon.DeleteWeapon();
                throw;
            }
        }

        // Select an AI program.
        public void selectaiprogram(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlNode xmlProgram = null;
            XmlDocument xmlDocument = _objCharacter.LoadData("programs.xml");
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", ForcedValue)
                             ?? throw new AbortedException();
            }

            if (xmlProgram == null)
            {
                // Display the Select Program window.
                using (ThreadSafeForm<SelectAIProgram> frmPickProgram = ThreadSafeForm<SelectAIProgram>.Get(() => new SelectAIProgram(_objCharacter)))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickProgram.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", frmPickProgram.MyForm.SelectedProgram)
                                 ?? throw new AbortedException();
                }
            }

            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = xmlProgram.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext");
            if (xmlSelectText != null)
            {
                using (ThreadSafeForm<SelectText> frmPickText = ThreadSafeForm<SelectText>.Get(() => new SelectText
                       {
                           Description = string.Format(GlobalSettings.CultureInfo,
                               LanguageManager.GetString("String_Improvement_SelectText"),
                               xmlProgram["translate"]?.InnerTextViaPool() ?? xmlProgram["name"]?.InnerTextViaPool())
                       }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            AIProgram objProgram = new AIProgram(_objCharacter);
            objProgram.Create(xmlProgram, strExtra, false);
            if (objProgram.InternalId.IsEmptyGuid())
                throw new AbortedException();

            _objCharacter.AIPrograms.Add(objProgram);

            SelectedValue = objProgram.CurrentDisplayNameShort;

            CreateImprovement(objProgram.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.AIProgram,
                _strUnique);
        }

        // Select an AI program.
        public void selectinherentaiprogram(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlNode xmlProgram = null;
            XmlDocument xmlDocument = _objCharacter.LoadData("programs.xml");
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", ForcedValue)
                    ?? throw new AbortedException();
            }

            if (xmlProgram == null)
            {
                // Display the Select Spell window.
                using (ThreadSafeForm<SelectAIProgram> frmPickProgram = ThreadSafeForm<SelectAIProgram>.Get(() => new SelectAIProgram(_objCharacter, false, true)))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickProgram.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", frmPickProgram.MyForm.SelectedProgram)
                                 ?? throw new AbortedException();
                }
            }

            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = xmlProgram.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext");
            if (xmlSelectText != null)
            {
                using (ThreadSafeForm<SelectText> frmPickText = ThreadSafeForm<SelectText>.Get(() => new SelectText
                       {
                           Description = string.Format(GlobalSettings.CultureInfo,
                               LanguageManager.GetString("String_Improvement_SelectText"),
                               xmlProgram["translate"]?.InnerTextViaPool() ?? xmlProgram["name"]?.InnerTextViaPool())
                       }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            AIProgram objProgram = new AIProgram(_objCharacter);
            objProgram.Create(xmlProgram, strExtra, false);
            if (objProgram.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = objProgram.CurrentDisplayNameShort;

            _objCharacter.AIPrograms.Add(objProgram);

            CreateImprovement(objProgram.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.AIProgram,
                _strUnique);
        }

        // Select a Contact
        public void selectcontact(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strMode = bonusNode["type"]?.InnerTextViaPool() ?? "all";

            IList<Contact> lstSelectedContacts;
            switch (strMode.ToUpperInvariant())
            {
                case "ALL":
                    lstSelectedContacts = _objCharacter.Contacts;
                    break;

                case "GROUP":
                case "NONGROUP":
                {
                    bool blnGroup = strMode == "group";
                    //Select any contact where IsGroup equals blnGroup
                    //and add to a list
                    lstSelectedContacts = _objCharacter.Contacts.Where(x => x.IsGroup == blnGroup).ToList();
                    break;
                }
                default:
                    throw new AbortedException();
            }

            if (lstSelectedContacts.Count == 0)
            {
                Program.ShowScrollableMessageBox(LanguageManager.GetString("Message_NoContactFound"),
                    LanguageManager.GetString("MessageTitle_NoContactFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new AbortedException();
            }

            using (ThreadSafeForm<SelectItem> frmSelect = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
            {
                int count = 0;
                //Black magic LINQ to cast content of list to another type
                frmSelect.MyForm.SetGeneralItemsMode(lstSelectedContacts.Select(x => new ListItem(count++.ToString(GlobalSettings.InvariantCultureInfo), x.Name)));

                if (frmSelect.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    throw new AbortedException();

                Contact objSelectedContact = int.TryParse(frmSelect.MyForm.SelectedItem, out int intIndex)
                    ? lstSelectedContacts[intIndex]
                    : throw new AbortedException();

                string strTemp = string.Empty;
                if (bonusNode.TryGetStringFieldQuickly("forcedloyalty", ref strTemp))
                {
                    decimal decForcedLoyalty = ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating);
                    CreateImprovement(objSelectedContact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForcedLoyalty, _strUnique, decForcedLoyalty);
                }

                if (bonusNode["free"] != null)
                {
                    CreateImprovement(objSelectedContact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactMakeFree, _strUnique);
                }

                if (bonusNode["forcegroup"] != null)
                {
                    CreateImprovement(objSelectedContact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForceGroup, _strUnique);
                }

                if (string.IsNullOrWhiteSpace(SelectedValue))
                {
                    SelectedValue = objSelectedContact.Name;
                }
                else
                {
                    SelectedValue += "," + LanguageManager.GetString("String_Space") + objSelectedContact.Name;
                }
            }
        }

        public void addcontact(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            int intLoyalty = 1;
            int intConnection = 1;

            string strTemp = string.Empty;
            if (bonusNode.TryGetStringFieldQuickly("loyalty", ref strTemp))
                intLoyalty = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            if (bonusNode.TryGetStringFieldQuickly("connection", ref strTemp))
                intConnection = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            bool group = bonusNode["group"] != null;
            bool canwrite = bonusNode["canwrite"] != null;
            Contact contact = new Contact(_objCharacter, !canwrite);
            try
            {
                contact.IsGroup = group;
                contact.Loyalty = intLoyalty;
                contact.Connection = intConnection;
                _objCharacter.Contacts.Add(contact);

                CreateImprovement(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.AddContact, contact.UniqueId);

                if (bonusNode.TryGetStringFieldQuickly("forcedloyalty", ref strTemp))
                {
                    decimal decForcedLoyalty = ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating);
                    CreateImprovement(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForcedLoyalty, _strUnique, decForcedLoyalty);
                }
                if (bonusNode["free"] != null)
                {
                    CreateImprovement(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactMakeFree, _strUnique);
                }
                if (bonusNode["forcegroup"] != null)
                {
                    CreateImprovement(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForceGroup, _strUnique);
                }
            }
            catch
            {
                _objCharacter.Contacts.Remove(contact);
                contact.Dispose();
                throw;
            }
        }

        // Affect a Specific CharacterAttribute.
        public void specificattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
            // Record the improvement.
            int intMin = 0;
            decimal decAug = 0;
            int intMax = 0;
            int intAugMax = 0;
            string strAttribute = bonusNode["name"]?.InnerTextViaPool();

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                intMin = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["val"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                decAug = ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating);
            strTemp = bonusNode["max"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.EndsWith("-natural", StringComparison.Ordinal))
                {
                    intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp.TrimEndOnce("-natural", true), _intRating)
                        - _objCharacter.GetAttribute(strAttribute).MetatypeMaximum;
                }
                else
                    intMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);
            }
            strTemp = bonusNode["aug"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating);

            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence") ?? bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("name/@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            if (bonusNode["affectbase"] != null)
                strAttribute += "Base";

            CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                strUseUnique, 0, 1, intMin, intMax, decAug, intAugMax);
        }

        // Add a paid increase to an attribute
        public void attributelevel(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
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
                List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
                foreach (XmlNode objSubNode in bonusNode["options"])
                    lstAbbrevs.Add(objSubNode.InnerTextViaPool());

                lstAbbrevs.Remove("ESS");
                if (!_objCharacter.MAGEnabled)
                {
                    lstAbbrevs.Remove("MAG");
                    lstAbbrevs.Remove("MAGAdept");
                }
                else if (!_objCharacter.IsMysticAdept || !_objCharacter.Settings.MysAdeptSecondMAGAttribute)
                    lstAbbrevs.Remove("MAGAdept");

                if (!_objCharacter.RESEnabled)
                    lstAbbrevs.Remove("RES");
                if (!_objCharacter.DEPEnabled)
                    lstAbbrevs.Remove("DEP");

                // Check to see if there is only one possible selection because of _strLimitSelection.
                if (!string.IsNullOrEmpty(ForcedValue))
                    LimitSelection = ForcedValue;

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    lstAbbrevs.RemoveAll(x => x != LimitSelection);
                }

                string strSelected;
                switch (lstAbbrevs.Count)
                {
                    case 0:
                        throw new AbortedException();
                    case 1:
                        strSelected = lstAbbrevs[0];
                        break;

                    default:
                    {
                        using (ThreadSafeForm<SelectAttribute> frmPickAttribute = ThreadSafeForm<SelectAttribute>.Get(
                                   () =>
                                       new SelectAttribute(lstAbbrevs.ToArray())
                                       {
                                           Description = !string.IsNullOrEmpty(_strFriendlyName)
                                               ? string.Format(GlobalSettings.CultureInfo,
                                                               LanguageManager.GetString(
                                                                   "String_Improvement_SelectAttributeNamed"),
                                                               _strFriendlyName)
                                               : LanguageManager.GetString("String_Improvement_SelectAttribute")
                                       }))
                        {
                            // Make sure the dialogue window was not canceled.
                            if (frmPickAttribute.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            {
                                throw new AbortedException();
                            }

                            strSelected = frmPickAttribute.MyForm.SelectedAttribute;
                        }

                        break;
                    }
                }

                SelectedValue = strSelected;

                CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.Attributelevel, _strUnique, value);
            }
            else
            {
                Log.Error(new object[] { "attributelevel", bonusNode.OuterXmlViaPool() });
            }
        }

        public void skilllevel(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSkill = string.Empty;
            int intValue = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref intValue);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkill))
            {
                CreateImprovement(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillLevel, _strUnique, intValue);
            }
            else
            {
                Log.Error(new object[] { "skilllevel", bonusNode.OuterXmlViaPool() });
            }
        }

        public void pushtext(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strPush = bonusNode.InnerTextViaPool();
            if (!string.IsNullOrWhiteSpace(strPush))
            {
                _objCharacter.PushText.Push(strPush);
            }
        }

        public void pushtextforqualitygroup(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            
            string strPush = bonusNode.InnerTextViaPool();
            string strQualityGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool() ?? string.Empty;
            
            if (!string.IsNullOrWhiteSpace(strPush) && !string.IsNullOrEmpty(strQualityGroup))
            {
                _objCharacter.PushTextForQualityGroup(strQualityGroup, strPush);
            }
        }

        public void activesoft(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = ForcedValue;
            bool blnKnowledgeSkill = false;
            if (string.IsNullOrEmpty(strSelectedValue))
                (strSelectedValue, blnKnowledgeSkill) = ImprovementManager
                    .DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName);
            if (blnKnowledgeSkill)
                throw new AbortedException();
            SelectedValue = strSelectedValue;
            string strVal = bonusNode["val"]?.InnerTextViaPool();
            (bool blnIsExotic, string strExoticSkillName)
                = ExoticSkill.IsExoticSkillNameTuple(_objCharacter, strSelectedValue);
            if (blnIsExotic)
            {
                if (!string.IsNullOrEmpty(strVal))
                {
                    // Make sure we have the exotic skill in the list if we're adding an activesoft
                    Skill objExistingSkill = _objCharacter.SkillsSection.GetActiveSkill(strSelectedValue);
                    if (objExistingSkill?.IsExoticSkill != true)
                    {
                        string strSkillName = strSelectedValue;
                        int intParenthesesIndex = strExoticSkillName.Length - 1 + strSkillName.TrimStartOnce(strExoticSkillName, true).IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex >= strExoticSkillName.Length)
                        {
                            string strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                            _objCharacter.SkillsSection.AddExoticSkill(strExoticSkillName, strSkillSpecific);
                        }
                    }
                    CreateImprovement(strSelectedValue, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Activesoft,
                        _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating));
                }
            }
            else if (!string.IsNullOrEmpty(strVal))
            {
                CreateImprovement(strSelectedValue, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.Activesoft,
                                  _strUnique,
                                  ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating));
            }

            if (bonusNode["addknowledge"] != null)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(_objCharacter, strSelectedValue, false);
                try
                {
                    _objCharacter.SkillsSection.KnowsoftSkills.Add(objKnowledgeSkill);
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillsoftAccess) > 0)
                    {
                        _objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                    }

                    CreateImprovement(objKnowledgeSkill.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillsoft, _strUnique, ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating));
                }
                catch
                {
                    objKnowledgeSkill.Remove();
                    throw;
                }
            }
        }

        public void skillsoft(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strForcedValue = ForcedValue;

            SelectedValue = string.IsNullOrEmpty(strForcedValue)
                ? ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName, true).Item1
                : strForcedValue;

            string strVal = bonusNode["val"]?.InnerTextViaPool();

            KnowledgeSkill objSkill = new KnowledgeSkill(_objCharacter, SelectedValue, false);
            try
            {
                _objCharacter.SkillsSection.KnowsoftSkills.Add(objSkill);
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillsoftAccess) > 0)
                {
                    _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                }

                CreateImprovement(objSkill.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillsoft, _strUnique, ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating));
            }
            catch
            {
                objSkill.Remove();
                throw;
            }
        }

        public void knowledgeskilllevel(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            //Theoretically life modules, right now we just give out free points and let people sort it out themselves.
            //Going to be fun to do the real way, from a computer science perspective, but i don't feel like using 2 weeks on that now

            decimal decVal = bonusNode["val"] != null ? ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"].InnerTextViaPool(), _intRating) : 1;
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, _strUnique, decVal);
        }

        public void knowledgeskillpoints(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), ImprovementManager.ValueToInt(_objCharacter, bonusNode.Value, _intRating)));
        }

        public void skillgrouplevel(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
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
                Log.Error(new object[] { "skillgrouplevel", bonusNode.OuterXmlViaPool() });
            }
        }

        // Change the maximum number of BP that can be spent on Nuyen.
        public void nuyenmaxbp(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NuyenMaxBP, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Apply a bonus/penalty to physical limit.
        public void physicallimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement("Physical", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalLimit,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Apply a bonus/penalty to mental limit.
        public void mentallimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement("Mental", _objImprovementSource, SourceName, Improvement.ImprovementType.MentalLimit,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Apply a bonus/penalty to social limit.
        public void sociallimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement("Social", _objImprovementSource, SourceName, Improvement.ImprovementType.SocialLimit,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
        public void nuyenamt(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty;
            CreateImprovement(strCondition, _objImprovementSource, SourceName, Improvement.ImprovementType.Nuyen, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Improve Condition Monitors.
        public void conditionmonitor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strTemp = bonusNode["physical"]?.InnerTextViaPool();
            // Physical Condition.
            if (!string.IsNullOrEmpty(strTemp))
            {
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCM, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
            }
            strTemp = bonusNode["stun"]?.InnerTextViaPool();
            // Stun Condition.
            if (!string.IsNullOrEmpty(strTemp))
            {
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StunCM, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
            }

            // Condition Monitor Threshold.
            XmlElement objNode = bonusNode["threshold"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes["precedence"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
                    ImprovementManager.ValueToDec(_objCharacter, objNode.InnerTextViaPool(), _intRating));
            }

            // Condition Monitor Threshold Offset. (Additional boxes appear before the FIRST Condition Monitor penalty)
            objNode = bonusNode["thresholdoffset"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes["precedence"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMThresholdOffset,
                    strUseUnique, ImprovementManager.ValueToDec(_objCharacter, objNode.InnerTextViaPool(), _intRating));
            }
            // Condition Monitor Threshold Offset that must be shared between the two. (Additional boxes appear before the FIRST Condition Monitor penalty)
            objNode = bonusNode["sharedthresholdoffset"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes["precedence"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMSharedThresholdOffset,
                    strUseUnique, ImprovementManager.ValueToDec(_objCharacter, objNode.InnerTextViaPool(), _intRating));
            }

            // Condition Monitor Overflow.
            objNode = bonusNode["overflow"];
            if (objNode != null)
            {
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMOverflow, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, objNode.InnerTextViaPool(), _intRating));
            }
        }

        // Improve Living Personal Attributes.
        public void livingpersona(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Device Rating.
            string strBonus = bonusNode["devicerating"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaDeviceRating, _strUnique);
            }

            // Program Limit.
            strBonus = bonusNode["programlimit"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaProgramLimit, _strUnique);
            }

            // Attack.
            strBonus = bonusNode["attack"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaAttack, _strUnique);
            }

            // Sleaze.
            strBonus = bonusNode["sleaze"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSleaze, _strUnique);
            }

            // Data Processing.
            strBonus = bonusNode["dataprocessing"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaDataProcessing, _strUnique);
            }

            // Firewall.
            strBonus = bonusNode["firewall"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaFirewall, _strUnique);
            }

            // Matrix CM.
            strBonus = bonusNode["matrixcm"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                CreateImprovement(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaMatrixCM, _strUnique);
            }
        }

        // The Improvement adjusts a specific Skill.
        public void specificskill(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
            string strCondition = bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty;

            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;

            string strBonusNodeName = bonusNode["name"]?.InnerTextViaPool();
            // Record the improvement.
            string strTemp = bonusNode["bonus"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
            {
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating), 1, 0, 0, 0,
                    0, string.Empty, blnAddToRating, string.Empty, strCondition);
            }
            if (bonusNode["disablespecializationeffects"] != null)
            {
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.DisableSpecializationEffects,
                    strUseUnique, 0, 1, 0, 0, 0, 0, string.Empty, false, string.Empty, strCondition);
            }

            strTemp = bonusNode["max"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
            {
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ImprovementManager.ValueToInt(_objCharacter, strTemp, _intRating), 0,
                    0,
                    string.Empty, blnAddToRating, string.Empty, strCondition);
            }
            strTemp = bonusNode["misceffect"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strTemp))
            {
                CreateImprovement(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, 0, 0,
                    0, string.Empty, false, strTemp, strCondition);
            }
        }

        public void reflexrecorderoptimization(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ReflexRecorderOptimization, _strUnique);
        }

        public void removeskillcategorydefaultpenalty(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Expected values are either a Skill Name or an empty string.
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.RemoveSkillCategoryDefaultPenalty, _strUnique);
        }

        public void removeskillgroupdefaultpenalty(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Expected values are either a Skill Name or an empty string.
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.RemoveSkillGroupDefaultPenalty, _strUnique);
        }

        public void removeskilldefaultpenalty(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Expected values are either a Skill Name or an empty string.
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.RemoveSkillDefaultPenalty, _strUnique);
        }

        // The Improvement adds a martial art
        public void martialart(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode objXmlArt = _objCharacter.LoadData("martialarts.xml").TryGetNodeByNameOrId("/chummer/martialarts/martialart", bonusNode.InnerTextViaPool());

            MartialArt objMartialArt = new MartialArt(_objCharacter);
            try
            {
                objMartialArt.Create(objXmlArt);
                objMartialArt.IsQuality = true;
                _objCharacter.MartialArts.Add(objMartialArt);

                CreateImprovement(objMartialArt.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.MartialArt,
                    _strUnique);
            }
            catch
            {
                objMartialArt.DeleteMartialArt();
                throw;
            }
        }

        // The Improvement adds a limit modifier
        public void limitmodifier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strLimit = bonusNode["limit"]?.InnerTextViaPool();
            decimal decBonus = ImprovementManager.ValueToDec(_objCharacter, bonusNode["value"]?.InnerTextViaPool(), _intRating);
            string strCondition = bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty;

            LimitModifier objLimitModifier = new LimitModifier(_objCharacter);
            objLimitModifier.Create(_strFriendlyName, decBonus.StandardRound(), strLimit, strCondition, false);
            _objCharacter.LimitModifiers.Add(objLimitModifier);

            CreateImprovement(objLimitModifier.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitModifier,
                _strUnique, decBonus, 0, 0, 0, 0, 0, string.Empty, strCondition: strCondition);
        }

        // The Improvement adjusts a Skill Category.
        public void skillcategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strName = bonusNode["name"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool() ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty;
                CreateImprovement(strName, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.SkillCategory, _strUnique,
                                  ImprovementManager.ValueToDec(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(),
                                                                _intRating), 1, 0,
                                  0,
                                  0, 0, !string.IsNullOrEmpty(strExclude) ? strExclude : string.Empty, blnAddToRating, strCondition: strCondition);
            }
        }

        // The Improvement adjusts a Skill Group.
        public void skillgroup(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strName = bonusNode["name"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool() ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty;
                CreateImprovement(strName, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.SkillGroup, _strUnique,
                                  ImprovementManager.ValueToDec(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(),
                                                                _intRating), 1, 0, 0, 0,
                                  0, !string.IsNullOrEmpty(strExclude) ? strExclude : string.Empty, blnAddToRating, strCondition: strCondition);
            }
        }

        // The Improvement adjust Skills when used with the given CharacterAttribute.
        public void skillattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence") ?? bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("name/@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            string strName = bonusNode["name"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool() ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty;
                CreateImprovement(strName, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.SkillAttribute, strUseUnique,
                                  ImprovementManager.ValueToDec(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(),
                                                                _intRating), 1,
                                  0, 0, 0, 0, strExclude,
                                  blnAddToRating, strCondition: strCondition);
            }
        }

        // The Improvement adjust Skills whose linked attribute is the given CharacterAttribute.
        public void skilllinkedattribute(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence") ?? bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("name/@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            string strName = bonusNode["name"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool() ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty;
                CreateImprovement(strName, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.SkillLinkedAttribute, strUseUnique,
                                  ImprovementManager.ValueToDec(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(),
                                                                _intRating), 1,
                                  0, 0, 0, 0, strExclude,
                                  blnAddToRating, strCondition: strCondition);
            }
        }

        // The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical CharacterAttribute).
        public void skillarticulation(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EnhancedArticulation,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(), _intRating));
        }

        // Check for Armor modifiers.
        public void armor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Armor, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Fire Armor modifiers.
        public void firearmor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FireArmor, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Cold Armor modifiers.
        public void coldarmor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ColdArmor, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Electricity Armor modifiers.
        public void electricityarmor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ElectricityArmor, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Acid Armor modifiers.
        public void acidarmor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AcidArmor, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Falling Armor modifiers.
        public void fallingarmor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FallingArmor, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Dodge modifiers.
        public void dodge(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Dodge, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Reach modifiers.
        public void reach(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strWeapon = bonusNode.Attributes?["name"]?.InnerTextViaPool() ?? string.Empty;
            CreateImprovement(strWeapon, _objImprovementSource, SourceName, Improvement.ImprovementType.Reach,
                _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Unarmed Damage Value modifiers.
        public void unarmeddv(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDV, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Unarmed Damage Value Physical.
        public void unarmeddvphysical(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDVPhysical, _strUnique);
        }

        // Check for Unarmed Armor Penetration.
        public void unarmedap(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedAP, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Unarmed Armor Penetration.
        public void unarmedreach(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedReach, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Initiative modifiers.
        public void initiative(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Initiative, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Initiative Pass modifiers. Only the highest one ever applies. Legacy method for old characters.
        public void initiativepass(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            initiativedice(bonusNode);
        }

        // Check for Initiative Pass modifiers. Only the highest one ever applies.
        public void initiativedice(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = bonusNode.Name;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecedence))
                strUseUnique = "precedence" + strPrecedence;

            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDice,
                strUseUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Initiative Dice modifiers. Only the highest one ever applies. Legacy method for old characters.
        public void initiativepassadd(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            initiativediceadd(bonusNode);
        }

        // Check for Initiative Dice modifiers. Only the highest one ever applies.
        public void initiativediceadd(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDiceAdd, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Matrix Initiative modifiers.
        public void matrixinitiative(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiative, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public void matrixinitiativepass(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            matrixinitiativedice(bonusNode);
        }

        // Check for Matrix Initiative Pass modifiers.
        public void matrixinitiativedice(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                "matrixinitiativepass", ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public void matrixinitiativepassadd(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            matrixinitiativediceadd(bonusNode);
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public void matrixinitiativediceadd(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void availability(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the Lifestyle node is present, we restrict to a specific lifestyle type.
            string strForId = bonusNode.Attributes?["id"]?.InnerTextViaPool() ?? string.Empty;
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty;
            CreateImprovement(strForId, _objImprovementSource, SourceName, Improvement.ImprovementType.Availability, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), strCondition: strCondition);
        }

        // Check for Lifestyle cost modifiers.
        public void lifestylecost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the Lifestyle node is present, we restrict to a specific lifestyle type.
            string strBaseLifestyle = bonusNode.Attributes?["lifestyle"]?.InnerTextViaPool() ?? string.Empty;
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty;
            CreateImprovement(strBaseLifestyle, _objImprovementSource, SourceName, Improvement.ImprovementType.LifestyleCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), strCondition: strCondition);
        }

        // Check for basic Lifestyle cost modifiers.
        public void basiclifestylecost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the Lifestyle node is present, we restrict to a specific lifestyle type.
            string strBaseLifestyle = bonusNode.Attributes?["lifestyle"]?.InnerTextViaPool() ?? string.Empty;
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty;
            CreateImprovement(strBaseLifestyle, _objImprovementSource, SourceName, Improvement.ImprovementType.BasicLifestyleCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), strCondition: strCondition);
        }

        // Check for Genetech Cost modifiers.
        public void genetechcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechCostMultiplier,
                _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Genetech Cost modifiers.
        public void genetechessmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechEssMultiplier,
                _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Basic Bioware Essence Cost modifiers.
        public void basicbiowareessmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BasicBiowareEssCost,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Bioware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void biowareessmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Bioware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void biowaretotalessmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareTotalEssMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Cyberware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void cyberwareessmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Cyberware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void cyberwaretotalessmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareTotalEssMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Non-Retroactive Bioware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void biowareessmultipliernonretroactive(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCostNonRetroactive, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Non-Retroactive Bioware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void biowaretotalessmultipliernonretroactive(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Non-Retroactive Cyberware Essence Cost modifiers that stack additively with base modifiers like grade.
        public void cyberwareessmultipliernonretroactive(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCostNonRetroactive, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Non-Retroactive Cyberware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public void cyberwaretotalessmultipliernonretroactive(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Prototype Transhuman modifiers.
        public void prototypetranshuman(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            decimal decValue = ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating);
            _objCharacter.PrototypeTranshuman += decValue;
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.PrototypeTranshuman, _strUnique, intRating: _intRating);
        }

        // Check for Friends In High Places modifiers.
        public void friendsinhighplaces(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FriendsInHighPlaces,
                _strUnique);
        }

        // Check for ExCon modifiers.
        public void excon(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ExCon, _strUnique);
        }

        // Check for TrustFund modifiers.
        public void trustfund(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.TrustFund,
                _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for MadeMan modifiers.
        public void mademan(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MadeMan, _strUnique);
        }

        // Check for Fame modifiers.
        public void fame(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Fame, _strUnique);
        }

        // Check for Erased modifiers.
        public void erased(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Erased, _strUnique);
        }

        // Check for Erased modifiers.
        public void overclocker(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Overclocker, _strUnique);
        }

        // Check for Restricted Gear modifiers.
        public void restrictedgear(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strValue = bonusNode["availability"]?.InnerTextViaPool();
            string strCount = bonusNode["amount"]?.InnerTextViaPool();
            if (string.IsNullOrEmpty(strCount))
            {
                strCount = "1";
                // Needed for legacy purposes when re-applying improvements
                if (string.IsNullOrEmpty(strValue))
                    strValue = bonusNode.InnerTextViaPool();
            }

            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedGear, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, strValue, _intRating), ImprovementManager.ValueToInt(_objCharacter, strCount, _intRating));
        }

        // Check for Improvements that grant bonuses to the maximum amount of Native languages a user can have.
        public void nativelanguagelimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NativeLanguageLimit, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Ambidextrous modifiers.
        public void ambidextrous(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Ambidextrous, _strUnique);
        }

        // Check for Weapon Category DV modifiers.
        public void weaponcategorydv(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            //TODO: FIX THIS
            /*
             * I feel like talking a little bit about improvementmanager at
             * this point. It is an interesting class. First of all, it
             * manages to throw out everything we ever learned about OOP
             * and create a class based on functional programming.
             *
             * That is true, it is a class, based on manipulating a single
             * list on another class.
             *
             * But at least there is a reference to it somewhere right?
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

            string strSelectedValue = string.Empty;
            if (bonusNode["selectskill"] != null)
            {
                bool blnKnowledgeSkill;
                (strSelectedValue, blnKnowledgeSkill) = ImprovementManager.DoSelectSkill(bonusNode["selectskill"], _objCharacter, _intRating, _strFriendlyName);

                if (blnKnowledgeSkill)
                {
                    throw new AbortedException();
                }

                Power objPower = _objCharacter.Powers.FirstOrDefault(p => p.InternalId == SourceName);
                if (objPower != null)
                    objPower.Extra = strSelectedValue;
            }
            else if (bonusNode["selectcategories"] != null)
            {
                using (XmlNodeList xmlSelectCategoryList = bonusNode.SelectNodes("selectcategories"))
                {
                    if (xmlSelectCategoryList?.Count > 0)
                    {
                        foreach (XmlNode xmlSelectCategory in xmlSelectCategoryList)
                        {
                            // Display the Select Category window and record which Category was selected.
                            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                           out List<ListItem> lstGeneralItems))
                            {
                                using (XmlNodeList xmlCategoryList = xmlSelectCategory.SelectNodes("category"))
                                {
                                    if (xmlCategoryList?.Count > 0)
                                    {
                                        foreach (XmlNode objXmlCategory in xmlCategoryList)
                                        {
                                            string strInnerText = objXmlCategory.InnerTextViaPool();
                                            lstGeneralItems.Add(new ListItem(strInnerText,
                                                                             _objCharacter.TranslateExtra(
                                                                                 strInnerText, GlobalSettings.Language,
                                                                                 "weapons.xml")));
                                        }
                                    }
                                }

                                using (ThreadSafeForm<SelectItem> frmPickCategory = ThreadSafeForm<SelectItem>.Get(() =>
                                           new SelectItem
                                           {
                                               Description = !string.IsNullOrEmpty(_strFriendlyName)
                                                   ? string.Format(GlobalSettings.CultureInfo,
                                                       LanguageManager.GetString(
                                                           "String_Improvement_SelectSkillNamed"), _strFriendlyName)
                                                   : LanguageManager.GetString("Title_SelectWeaponCategory")
                                           }))
                                {
                                    frmPickCategory.MyForm.SetGeneralItemsMode(lstGeneralItems);

                                    if (ForcedValue.StartsWith("Adept:", StringComparison.Ordinal)
                                        || ForcedValue.StartsWith("Magician:", StringComparison.Ordinal))
                                        ForcedValue = string.Empty;

                                    if (!string.IsNullOrEmpty(ForcedValue))
                                    {
                                        frmPickCategory.MyForm.Opacity = 0;
                                        frmPickCategory.MyForm.ForceItem(ForcedValue);
                                    }

                                    // Make sure the dialogue window was not canceled.
                                    if (frmPickCategory.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                    {
                                        throw new AbortedException();
                                    }

                                    strSelectedValue = frmPickCategory.MyForm.SelectedItem;
                                }
                            }

                            _objCharacter.Powers.ForEach(objPower =>
                            {
                                if (objPower.InternalId == SourceName)
                                {
                                    objPower.Extra = strSelectedValue;
                                }
                            });
                        }
                    }
                }
            }
            else if (bonusNode["name"] != null)
            {
                strSelectedValue = bonusNode["name"].InnerTextViaPool();
            }
            else
            {
                Utils.BreakIfDebug();
            }
            SelectedValue = strSelectedValue;
            CreateImprovement(strSelectedValue, _objImprovementSource, SourceName,
                Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(), _intRating));
        }

        public void weaponcategorydice(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList xmlSelectCategoryList = bonusNode.SelectNodes("selectcategory"))
            {
                if (xmlSelectCategoryList?.Count > 0)
                {
                    foreach (XmlNode xmlSelectCategory in xmlSelectCategoryList)
                    {
                        // Display the Select Category window and record which Category was selected.
                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstGeneralItems))
                        {
                            using (XmlNodeList xmlCategoryList = xmlSelectCategory.SelectNodes("category"))
                            {
                                if (xmlCategoryList?.Count > 0)
                                {
                                    foreach (XmlNode objXmlCategory in xmlCategoryList)
                                    {
                                        string strInnerText = objXmlCategory.InnerTextViaPool();
                                        lstGeneralItems.Add(new ListItem(strInnerText,
                                                                         _objCharacter.TranslateExtra(
                                                                             strInnerText, GlobalSettings.Language,
                                                                             "weapons.xml")));
                                    }
                                }
                            }

                            using (ThreadSafeForm<SelectItem> frmPickCategory = ThreadSafeForm<SelectItem>.Get(() =>
                                       new SelectItem
                                       {
                                           Description = !string.IsNullOrEmpty(_strFriendlyName)
                                               ? string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString(
                                                       "String_Improvement_SelectSkillNamed"), _strFriendlyName)
                                               : LanguageManager.GetString("Title_SelectWeaponCategory")
                                       }))
                            {
                                frmPickCategory.MyForm.SetGeneralItemsMode(lstGeneralItems);

                                if (ForcedValue.StartsWith("Adept:", StringComparison.Ordinal)
                                    || ForcedValue.StartsWith("Magician:", StringComparison.Ordinal))
                                    ForcedValue = string.Empty;

                                if (!string.IsNullOrEmpty(ForcedValue))
                                {
                                    frmPickCategory.MyForm.Opacity = 0;
                                    frmPickCategory.MyForm.ForceItem(ForcedValue);
                                }

                                // Make sure the dialogue window was not canceled.
                                if (frmPickCategory.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                {
                                    throw new AbortedException();
                                }

                                SelectedValue = frmPickCategory.MyForm.SelectedItem;
                            }
                        }

                        _objCharacter.Powers.ForEach(objPower =>
                        {
                            if (objPower.InternalId == SourceName)
                            {
                                objPower.Extra = SelectedValue;
                            }
                        });

                        CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.WeaponCategoryDice, _strUnique, ImprovementManager.ValueToDec(_objCharacter, xmlSelectCategory["value"]?.InnerTextViaPool(), _intRating));
                    }
                }
            }

            using (XmlNodeList xmlCategoryList = bonusNode.SelectNodes("category"))
            {
                if (xmlCategoryList?.Count > 0)
                {
                    foreach (XmlNode xmlCategory in xmlCategoryList)
                    {
                        CreateImprovement(xmlCategory["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName,
                            Improvement.ImprovementType.WeaponCategoryDice, _strUnique, ImprovementManager.ValueToDec(_objCharacter, xmlCategory["value"]?.InnerTextViaPool(), _intRating));
                    }
                }
            }
        }

        public void weaponspecificdice(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGeneralItems))
            {
                string strType = bonusNode.Attributes?["type"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strType))
                {
                    _objCharacter.Weapons.ForEach(objWeapon =>
                    {
                        if (objWeapon.RangeType == strType)
                        {
                            lstGeneralItems.Add(new ListItem(objWeapon.InternalId, objWeapon.CurrentDisplayName));
                        }
                    });
                }
                else
                {
                    _objCharacter.Weapons.ForEach(objWeapon => lstGeneralItems.Add(new ListItem(objWeapon.InternalId, objWeapon.CurrentDisplayName)));
                }

                Weapon objSelectedWeapon;
                using (ThreadSafeForm<SelectItem> frmPickWeapon = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                       {
                           Description = !string.IsNullOrEmpty(_strFriendlyName)
                               ? string.Format(GlobalSettings.CultureInfo,
                                   LanguageManager.GetString("String_Improvement_SelectSkillNamed"),
                                   _strFriendlyName)
                               : LanguageManager.GetString("Title_SelectWeapon")
                       }))
                {
                    frmPickWeapon.MyForm.SetGeneralItemsMode(lstGeneralItems);
                    if (!string.IsNullOrEmpty(ForcedValue))
                    {
                        frmPickWeapon.MyForm.Opacity = 0;
                    }

                    frmPickWeapon.MyForm.ForceItem(ForcedValue);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickWeapon.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    string strSelected = frmPickWeapon.MyForm.SelectedItem;

                    objSelectedWeapon = _objCharacter.Weapons.FirstOrDefault(x => x.InternalId == strSelected);
                    if (objSelectedWeapon == null)
                    {
                        throw new AbortedException();
                    }
                }

                SelectedValue = objSelectedWeapon.Name;
                CreateImprovement(objSelectedWeapon.InternalId, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.WeaponSpecificDice, _strUnique,
                                  ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
            }
        }

        // Check for Mentor Spirit bonuses.
        public void selectmentorspirit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (ThreadSafeForm<SelectMentorSpirit> frmPickMentorSpirit = ThreadSafeForm<SelectMentorSpirit>.Get(() => new SelectMentorSpirit(_objCharacter)
            {
                ForcedMentor = ForcedValue
            }))
            {
                // Make sure the dialogue window was not canceled.
                if (frmPickMentorSpirit.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                XmlNode xmlMentor = _objCharacter.LoadData("mentors.xml").TryGetNodeByNameOrId("/chummer/mentors/mentor", frmPickMentorSpirit.MyForm.SelectedMentor)
                                    ?? throw new AbortedException();
                SelectedValue = xmlMentor["name"]?.InnerTextViaPool() ?? string.Empty;

                string strHoldValue = SelectedValue;

                string strForce = ForcedValue;
                MentorSpirit objMentor = new MentorSpirit(_objCharacter);
                try
                {
                    objMentor.Create(xmlMentor, Improvement.ImprovementType.MentorSpirit, string.Empty,
                                     frmPickMentorSpirit.MyForm.Choice1, frmPickMentorSpirit.MyForm.Choice2);
                    if (objMentor.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    _objCharacter.MentorSpirits.Add(objMentor);
                }
                catch
                {
                    objMentor.Dispose();
                    throw;
                }

                ForcedValue = strForce;
                SelectedValue = strHoldValue;
                CreateImprovement(objMentor.InternalId, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.MentorSpirit,
                                  frmPickMentorSpirit.MyForm.SelectedMentor);
            }
        }

        // Check for Paragon bonuses.
        public void selectparagon(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (ThreadSafeForm<SelectMentorSpirit> frmPickMentorSpirit = ThreadSafeForm<SelectMentorSpirit>.Get(() => new SelectMentorSpirit(_objCharacter, "paragons.xml")
            {
                ForcedMentor = ForcedValue
            }))
            {
                // Make sure the dialogue window was not canceled.
                if (frmPickMentorSpirit.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                XmlNode xmlMentor = _objCharacter.LoadData("paragons.xml").TryGetNodeByNameOrId("/chummer/mentors/mentor", frmPickMentorSpirit.MyForm.SelectedMentor)
                                    ?? throw new AbortedException();
                SelectedValue = xmlMentor["name"]?.InnerTextViaPool() ?? string.Empty;

                string strHoldValue = SelectedValue;

                string strForce = ForcedValue;
                MentorSpirit objMentor = new MentorSpirit(_objCharacter);
                try
                {
                    objMentor.Create(xmlMentor, Improvement.ImprovementType.Paragon, string.Empty,
                                     frmPickMentorSpirit.MyForm.Choice1, frmPickMentorSpirit.MyForm.Choice2);
                    if (objMentor.InternalId.IsEmptyGuid())
                        throw new AbortedException();
                    _objCharacter.MentorSpirits.Add(objMentor);
                }
                catch
                {
                    objMentor.Dispose();
                    throw;
                }

                ForcedValue = strForce;
                SelectedValue = strHoldValue;
                CreateImprovement(objMentor.InternalId, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.Paragon, frmPickMentorSpirit.MyForm.SelectedMentor);
            }
        }

        // Check for Smartlink bonus.
        public void smartlink(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Smartlink,
                "smartlink", ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Adapsin bonus.
        public void adapsin(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Adapsin, "adapsin");
        }

        // Check for SoftWeave bonus.
        public void softweave(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SoftWeave, "softweave");
        }

        // Check for bonus that removes the ability to take any bioware (e.g. Sensitive System)
        public void disablebioware(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableBioware,
                "disablebioware");
        }

        // Check for bonus that removes the ability to take any cyberware.
        public void disablecyberware(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableCyberware,
                "disablecyberware");
        }

        // Check for bonus that removes access to certain bioware grades (e.g. Cyber-Snob)
        public void disablebiowaregrade(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strGradeName = bonusNode.InnerTextViaPool();
            CreateImprovement(strGradeName, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableBiowareGrade,
                "disablebiowaregrade");
        }

        // Check for bonus that removes access to certain cyberware grades (e.g. Regeneration critter power).
        public void disablecyberwaregrade(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strGradeName = bonusNode.InnerTextViaPool();
            CreateImprovement(strGradeName, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableCyberwareGrade,
                "disablecyberwaregrade");
        }

        // Check for increases to walk multiplier.
        public void walkmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCategory = bonusNode["category"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.WalkMultiplier, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
                strTemp = bonusNode["percent"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.WalkMultiplierPercent, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
            }
        }

        // Check for increases to run multiplier.
        public void runmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCategory = bonusNode["category"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.RunMultiplier, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
                strTemp = bonusNode["percent"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.RunMultiplierPercent, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
            }
        }

        // Check for increases to distance sprinted per hit.
        public void sprintbonus(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCategory = bonusNode["category"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.SprintBonus, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
                strTemp = bonusNode["percent"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strTemp))
                    CreateImprovement(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.SprintBonusPercent, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, strTemp, _intRating));
            }
        }

        // Check for free Positive Qualities.
        public void freepositivequalities(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreePositiveQualities, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for free Negative Qualities.
        public void freenegativequalities(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeNegativeQualities, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Select Side.
        public void selectside(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (ThreadSafeForm<SelectSide> frmPickSide = ThreadSafeForm<SelectSide>.Get(() => new SelectSide
            {
                Description = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_SelectSide"), _strFriendlyName)
            }))
            {
                if (!string.IsNullOrEmpty(ForcedValue))
                    frmPickSide.MyForm.ForceValue(ForcedValue);
                // Make sure the dialogue window was not canceled.
                else if (frmPickSide.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickSide.MyForm.SelectedSide;
            }
        }

        // Check for Free Spirit Power Points.
        public void freespiritpowerpoints(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Adept Power Points.
        public void adeptpowerpoints(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerPoints, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Adept Powers
        public void specificpower(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (_objCharacter.AdeptEnabled)
            {
                ForcedValue = string.Empty;

                string strPowerName = bonusNode["name"]?.InnerTextViaPool();

                if (!string.IsNullOrEmpty(strPowerName))
                {
                    Power objBoostedPower;
                    XmlNode objXmlPower = _objCharacter.LoadData("powers.xml").TryGetNodeByNameOrId("/chummer/powers/power", strPowerName);
                    Power objNewPower = new Power(_objCharacter);
                    try
                    {
                        if (!objNewPower.Create(objXmlPower, 0, bonusNode["bonusoverride"]))
                            throw new AbortedException();

                        // Check if the character already has this power
                        objBoostedPower = _objCharacter.Powers.FirstOrDefault(objPower => objPower.Name == objNewPower.Name && objPower.Extra == objNewPower.Extra);
                    }
                    catch
                    {
                        objNewPower.DeletePower();
                        throw;
                    }
                    if (objBoostedPower == null)
                    {
                        _objCharacter.Powers.Add(objNewPower);
                        objBoostedPower = objNewPower;
                    }
                    else
                        objNewPower.DeletePower();

                    int.TryParse(bonusNode["val"]?.InnerTextViaPool(), NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intLevels);
                    if (!objBoostedPower.LevelsEnabled)
                        intLevels = 1;
                    CreateImprovement(objBoostedPower.Name, _objImprovementSource, SourceName,
                        !string.IsNullOrWhiteSpace(bonusNode["pointsperlevel"]?.InnerTextViaPool())
                            ? Improvement.ImprovementType.AdeptPowerFreePoints
                            : Improvement.ImprovementType.AdeptPowerFreeLevels, objBoostedPower.Extra, 0,
                        intLevels);

                    // fix: refund power points, if bonus would make power exceed maximum
                    if (objBoostedPower.TotalMaximumLevels < objBoostedPower.Rating + intLevels)
                    {
                        objBoostedPower.Rating = Math.Max(objBoostedPower.TotalMaximumLevels - intLevels, 0);
                    }

                    objBoostedPower.OnPropertyChanged(nameof(Power.FreeLevels));
                }
            }
        }

        // Select a Power.
        public void selectpowers(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (_objCharacter.AdeptEnabled)
            {
                using (XmlNodeList objXmlPowerList = bonusNode.SelectNodes("selectpower"))
                {
                    if (objXmlPowerList != null)
                    {
                        XmlDocument xmlDocument = _objCharacter.LoadData("powers.xml");
                        foreach (XmlNode objNode in objXmlPowerList)
                        {
                            XmlNode objXmlPower;
                            int intLevels = ImprovementManager.ValueToInt(_objCharacter, objNode["val"]?.InnerTextViaPool(), _intRating);
                            string strPointsPerLevel = objNode["pointsperlevel"]?.InnerTextViaPool();
                            // Display the Select Power window and record which Power was selected.
                            using (ThreadSafeForm<SelectPower> frmPickPower = ThreadSafeForm<SelectPower>.Get(() => new SelectPower(_objCharacter)))
                            {
                                frmPickPower.MyForm.ForBonus = true;
                                frmPickPower.MyForm.IgnoreLimits = objNode["ignorerating"]?.InnerTextIsTrueString() == true;

                                if (!string.IsNullOrEmpty(strPointsPerLevel))
                                    frmPickPower.MyForm.PointsPerLevel = ImprovementManager.ValueToDec(_objCharacter, strPointsPerLevel, _intRating);
                                string strLimit = objNode["limit"]?.InnerTextViaPool().Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                                if (!string.IsNullOrEmpty(strLimit))
                                    frmPickPower.MyForm.LimitToRating = ImprovementManager.ValueToInt(_objCharacter, strLimit, _intRating);
                                string strLimitToPowers = objNode.Attributes?["limittopowers"]?.InnerTextViaPool();
                                if (!string.IsNullOrEmpty(strLimitToPowers))
                                    frmPickPower.MyForm.LimitToPowers = strLimitToPowers;

                                // Make sure the dialogue window was not canceled.
                                if (frmPickPower.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                    throw new AbortedException();

                                objXmlPower = xmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", frmPickPower.MyForm.SelectedPower)
                                              ?? throw new AbortedException();
                            }

                            bool blnHasPower;
                            string strName;
                            string strExtra;
                            // If no, add the power and mark it free or give it free levels
                            Power objNewPower = new Power(_objCharacter);
                            try
                            {
                                if (!objNewPower.Create(objXmlPower, 0, bonusNode["bonusoverride"]))
                                    throw new AbortedException();

                                SelectedValue = objNewPower.CurrentDisplayName;
                                strName = objNewPower.Name;
                                strExtra = objNewPower.Extra;
                                blnHasPower = _objCharacter.Powers.Any(
                                    objPower => objPower.Name == strName && objPower.Extra == strExtra);
                            }
                            catch
                            {
                                objNewPower.DeletePower();
                                throw;
                            }

                            if (!blnHasPower)
                            {
                                _objCharacter.Powers.Add(objNewPower);
                            }
                            else
                            {
                                // Another copy of the power already exists, so we ensure that we remove any improvements created by the power because we're discarding it.
                                objNewPower.DeletePower();
                            }

                            CreateImprovement(strName, _objImprovementSource, SourceName,
                                    !string.IsNullOrWhiteSpace(strPointsPerLevel)
                                        ? Improvement.ImprovementType.AdeptPowerFreePoints
                                        : Improvement.ImprovementType.AdeptPowerFreeLevels, strExtra, 0,
                                    intLevels);
                        }
                    }
                }
            }
        }

        // Check for Armor Encumbrance Penalty.
        public void armorencumbrancepenalty(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void addart(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode objXmlSelectedArt = _objCharacter.LoadData("metamagic.xml").TryGetNodeByNameOrId("/chummer/arts/art", bonusNode.InnerTextViaPool());

            // Makes sure we aren't over our limits for this particular metamagic from this overall source
            if (bonusNode.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                objXmlSelectedArt?.CreateNavigator().RequirementsMet(_objCharacter, strLocalName: _strFriendlyName) == true)
            {
                Art objAddArt = new Art(_objCharacter);
                objAddArt.Create(objXmlSelectedArt, Improvement.ImprovementSource.Metamagic);
                objAddArt.Grade = -1;
                if (objAddArt.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                _objCharacter.Arts.Add(objAddArt);
                CreateImprovement(objAddArt.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Art, _strUnique);
            }
            else
            {
                throw new AbortedException();
            }
        }

        public void selectart(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("metamagic.xml");
            XmlNode objXmlSelectedArt;
            using (XmlNodeList xmlArtList = bonusNode.SelectNodes("art"))
            {
                if (xmlArtList?.Count > 0)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstArts))
                    {
                        foreach (XmlNode objXmlAddArt in xmlArtList)
                        {
                            string strLoopName = objXmlAddArt.InnerTextViaPool();
                            XmlNode objXmlArt = objXmlDocument.TryGetNodeByNameOrId("/chummer/arts/art", strLoopName);
                            // Makes sure we aren't over our limits for this particular metamagic from this overall source
                            if (objXmlArt != null && objXmlAddArt.CreateNavigator().RequirementsMet(_objCharacter))
                            {
                                lstArts.Add(new ListItem(objXmlArt["id"]?.InnerTextViaPool(),
                                    objXmlArt["translate"]?.InnerTextViaPool() ?? strLoopName));
                            }
                        }

                        if (lstArts.Count == 0)
                        {
                            Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                LanguageManager.GetString(
                                    "Message_Improvement_EmptySelectionListNamed"),
                                SourceName));
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstArts);
                            // Don't do anything else if the form was canceled.
                            if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                throw new AbortedException();

                            objXmlSelectedArt = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", frmPickItem.MyForm.SelectedItem)
                                                ?? throw new AbortedException();
                        }
                    }

                    string strSelectedName = objXmlSelectedArt["name"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strSelectedName))
                        throw new AbortedException();
                }
                else
                {
                    using (ThreadSafeForm<SelectArt> frmPickArt = ThreadSafeForm<SelectArt>.Get(() => new SelectArt(_objCharacter, SelectArt.Mode.Art)))
                    {
                        // Don't do anything else if the form was canceled.
                        if (frmPickArt.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            throw new AbortedException();

                        objXmlSelectedArt = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", frmPickArt.MyForm.SelectedItem)
                                            ?? throw new AbortedException();
                    }
                }
            }

            Art objAddArt = new Art(_objCharacter);
            objAddArt.Create(objXmlSelectedArt, Improvement.ImprovementSource.Metamagic);
            objAddArt.Grade = -1;
            if (objAddArt.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = objAddArt.CurrentDisplayName;

            _objCharacter.Arts.Add(objAddArt);
            CreateImprovement(objAddArt.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Art, _strUnique);
        }

        public void addmetamagic(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode objXmlSelectedMetamagic = _objCharacter.LoadData("metamagic.xml").TryGetNodeByNameOrId("/chummer/metamagics/metamagic", bonusNode.InnerTextViaPool());
            // Makes sure we aren't over our limits for this particular metamagic from this overall source
            if (bonusNode.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                objXmlSelectedMetamagic?.CreateNavigator().RequirementsMet(_objCharacter, strLocalName: _strFriendlyName) == true)
            {
                string strForcedValue = bonusNode.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
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
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("metamagic.xml");
            string strForceValue = string.Empty;
            XmlNode objXmlSelectedMetamagic;
            using (XmlNodeList xmlMetamagicList = bonusNode.SelectNodes("metamagic"))
            {
                if (xmlMetamagicList?.Count > 0)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstMetamagics))
                    {
                        foreach (XmlNode objXmlAddMetamagic in xmlMetamagicList)
                        {
                            string strLoopName = objXmlAddMetamagic.InnerTextViaPool();
                            XmlNode objXmlMetamagic
                                = objXmlDocument.TryGetNodeByNameOrId("/chummer/metamagics/metamagic", strLoopName);
                            // Makes sure we aren't over our limits for this particular metamagic from this overall source
                            if (objXmlMetamagic != null && objXmlAddMetamagic.CreateNavigator().RequirementsMet(_objCharacter))
                            {
                                lstMetamagics.Add(new ListItem(objXmlMetamagic["id"]?.InnerTextViaPool(),
                                    objXmlMetamagic["translate"]?.InnerTextViaPool()
                                    ?? strLoopName));
                            }
                        }

                        if (lstMetamagics.Count == 0)
                        {
                            Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                LanguageManager.GetString(
                                    "Message_Improvement_EmptySelectionListNamed"),
                                SourceName));
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstMetamagics);
                            // Don't do anything else if the form was canceled.
                            if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                throw new AbortedException();

                            objXmlSelectedMetamagic = objXmlDocument.TryGetNodeByNameOrId("/chummer/metamagics/metamagic", frmPickItem.MyForm.SelectedItem)
                                                      ?? throw new AbortedException();
                        }
                    }

                    string strSelectedName = objXmlSelectedMetamagic["name"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strSelectedName))
                        throw new AbortedException();
                    foreach (XmlNode objXmlAddMetamagic in xmlMetamagicList)
                    {
                        if (strSelectedName == objXmlAddMetamagic.InnerTextViaPool())
                        {
                            strForceValue = objXmlAddMetamagic.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
                            break;
                        }
                    }
                }
                else
                {
                    InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1 };
                    using (ThreadSafeForm<SelectMetamagic> frmPickMetamagic = ThreadSafeForm<SelectMetamagic>.Get(() => new SelectMetamagic(_objCharacter, objGrade)))
                    {
                        // Don't do anything else if the form was canceled.
                        if (frmPickMetamagic.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            throw new AbortedException();

                        objXmlSelectedMetamagic = objXmlDocument.TryGetNodeByNameOrId("/chummer/metamagics/metamagic", frmPickMetamagic.MyForm.SelectedMetamagic)
                                                  ?? throw new AbortedException();
                    }
                }
            }

            Metamagic objAddMetamagic = new Metamagic(_objCharacter);
            objAddMetamagic.Create(objXmlSelectedMetamagic, Improvement.ImprovementSource.Metamagic, strForceValue);
            objAddMetamagic.Grade = -1;
            if (objAddMetamagic.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = objAddMetamagic.CurrentDisplayName;

            _objCharacter.Metamagics.Add(objAddMetamagic);
            CreateImprovement(objAddMetamagic.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Metamagic, _strUnique);
        }

        public void addecho(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("echoes.xml");
            XmlNode objXmlSelectedEcho = objXmlDocument.TryGetNodeByNameOrId("/chummer/echoes/echo", bonusNode.InnerTextViaPool());

            // Makes sure we aren't over our limits for this particular echo from this overall source
            if (bonusNode.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                objXmlSelectedEcho?.CreateNavigator().RequirementsMet(_objCharacter, strLocalName: _strFriendlyName) == true)
            {
                string strForceValue = bonusNode.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
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
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("echoes.xml");
            string strForceValue = string.Empty;
            XmlNode xmlSelectedEcho;
            using (XmlNodeList xmlEchoList = bonusNode.SelectNodes("echo"))
            {
                if (xmlEchoList?.Count > 0)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstEchoes))
                    {
                        foreach (XmlNode objXmlAddEcho in xmlEchoList)
                        {
                            string strLoopName = objXmlAddEcho.InnerTextViaPool();
                            XmlNode objXmlEcho = objXmlDocument.TryGetNodeByNameOrId(
                                "/chummer/metamagics/metamagic", strLoopName);
                            // Makes sure we aren't over our limits for this particular metamagic from this overall source
                            if (objXmlEcho != null && objXmlAddEcho.CreateNavigator().RequirementsMet(_objCharacter))
                            {
                                lstEchoes.Add(new ListItem(objXmlEcho["id"]?.InnerTextViaPool(),
                                    objXmlEcho["translate"]?.InnerTextViaPool() ?? strLoopName));
                            }
                        }

                        if (lstEchoes.Count == 0)
                        {
                            Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                LanguageManager.GetString(
                                    "Message_Improvement_EmptySelectionListNamed"),
                                SourceName));
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstEchoes);
                            // Don't do anything else if the form was canceled.
                            if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                throw new AbortedException();

                            xmlSelectedEcho = objXmlDocument.TryGetNodeByNameOrId("/chummer/echoes/echo", frmPickItem.MyForm.SelectedItem)
                                              ?? throw new AbortedException();
                        }
                    }

                    string strSelectedName = xmlSelectedEcho["name"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strSelectedName))
                        throw new AbortedException();
                    foreach (XmlNode objXmlAddEcho in xmlEchoList)
                    {
                        if (strSelectedName == objXmlAddEcho.InnerTextViaPool())
                        {
                            strForceValue = objXmlAddEcho.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
                            break;
                        }
                    }
                }
                else
                {
                    InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1, Technomancer = true };
                    using (ThreadSafeForm<SelectMetamagic> frmPickMetamagic = ThreadSafeForm<SelectMetamagic>.Get(() => new SelectMetamagic(_objCharacter, objGrade)))
                    {
                        // Don't do anything else if the form was canceled.
                        if (frmPickMetamagic.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            throw new AbortedException();

                        xmlSelectedEcho = objXmlDocument.TryGetNodeByNameOrId("/chummer/echoes/echo", frmPickMetamagic.MyForm.SelectedMetamagic)
                                          ?? throw new AbortedException();
                    }
                }
            }

            Metamagic objAddEcho = new Metamagic(_objCharacter);
            objAddEcho.Create(xmlSelectedEcho, Improvement.ImprovementSource.Echo, strForceValue);
            objAddEcho.Grade = -1;
            if (objAddEcho.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = objAddEcho.CurrentDisplayName;

            _objCharacter.Metamagics.Add(objAddEcho);
            CreateImprovement(objAddEcho.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Echo, _strUnique);
        }

        // Check for Skillwires.
        public void skillwire(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillwire,
                              strUseUnique,
                              ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Hardwires.
        public void hardwires(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = ForcedValue;
            if (string.IsNullOrEmpty(strSelectedValue))
                strSelectedValue = ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName).Item1;
            SelectedValue = strSelectedValue;
            (bool blnIsExotic, string strExoticSkillName)
                = ExoticSkill.IsExoticSkillNameTuple(_objCharacter, strSelectedValue);
            if (blnIsExotic)
            {
                if (!bonusNode.IsNullOrInnerTextIsEmpty())
                {
                    // Make sure we have the exotic skill in the list if we're adding an activesoft
                    Skill objExistingSkill = _objCharacter.SkillsSection.GetActiveSkill(strSelectedValue);
                    if (objExistingSkill?.IsExoticSkill != true)
                    {
                        string strSkillName = strSelectedValue;
                        int intParenthesesIndex = strExoticSkillName.Length - 1 + strSkillName.TrimStartOnce(strExoticSkillName, true).IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex >= strExoticSkillName.Length)
                        {
                            string strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                            _objCharacter.SkillsSection.AddExoticSkill(strExoticSkillName, strSkillSpecific);
                        }
                    }
                    CreateImprovement(strSelectedValue, _objImprovementSource, SourceName,
                                      Improvement.ImprovementType.Hardwire,
                                      _strUnique,
                                      ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
                }
            }
            else
            {
                CreateImprovement(strSelectedValue, _objImprovementSource, SourceName,
                                  Improvement.ImprovementType.Hardwire,
                                  strSelectedValue,
                                  ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
            }
        }

        // Check for Damage Resistance.
        public void damageresistance(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DamageResistance, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Judge Intentions.
        public void judgeintentions(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentions, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Judge Intentions (offense only, i.e. doing the judging).
        public void judgeintentionsoffense(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentionsOffense, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Judge Intentions (defense only, i.e. being judged).
        public void judgeintentionsdefense(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentionsDefense, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Composure.
        public void composure(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Composure, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Lift and Carry.
        public void liftandcarry(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.LiftAndCarry, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Memory.
        public void memory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Memory, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Fatigue Resist.
        public void fatigueresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FatigueResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Radiation Resist.
        public void radiationresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.RadiationResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Sonic Attacks Resist.
        public void sonicresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SonicResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Contact-vector Toxins Resist.
        public void toxincontactresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinContactResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Ingestion-vector Toxins Resist.
        public void toxiningestionresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinIngestionResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Inhalation-vector Toxins Resist.
        public void toxininhalationresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInhalationResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Injection-vector Toxins Resist.
        public void toxininjectionresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInjectionResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Contact-vector Pathogens Resist.
        public void pathogencontactresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenContactResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Ingestion-vector Pathogens Resist.
        public void pathogeningestionresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenIngestionResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Inhalation-vector Pathogens Resist.
        public void pathogeninhalationresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInhalationResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Injection-vector Pathogens Resist.
        public void pathogeninjectionresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInjectionResist, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Contact-vector Toxins Immunity.
        public void toxincontactimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinContactImmune, _strUnique);
        }

        // Check for Ingestion-vector Toxins Immunity.
        public void toxiningestionimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinIngestionImmune, _strUnique);
        }

        // Check for Inhalation-vector Toxins Immunity.
        public void toxininhalationimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInhalationImmune, _strUnique);
        }

        // Check for Injection-vector Toxins Immunity.
        public void toxininjectionimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInjectionImmune, _strUnique);
        }

        // Check for Contact-vector Pathogens Immunity.
        public void pathogencontactimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenContactImmune, _strUnique);
        }

        // Check for Ingestion-vector Pathogens Immunity.
        public void pathogeningestionimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenIngestionImmune, _strUnique);
        }

        // Check for Inhalation-vector Pathogens Immunity.
        public void pathogeninhalationimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInhalationImmune, _strUnique);
        }

        // Check for Injection-vector Pathogens Immunity.
        public void pathogeninjectionimmune(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInjectionImmune, _strUnique);
        }

        // Check for Physiological Addiction Resist if you are not addicted.
        public void physiologicaladdictionfirsttime(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysiologicalAddictionFirstTime, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Psychological Addiction if you are not addicted.
        public void psychologicaladdictionfirsttime(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PsychologicalAddictionFirstTime, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Physiological Addiction Resist if you are addicted.
        public void physiologicaladdictionalreadyaddicted(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Psychological Addiction if you are addicted.
        public void psychologicaladdictionalreadyaddicted(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Recovery Dice from Stun CM Damage.
        public void stuncmrecovery(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StunCMRecovery, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Recovery Dice from Physical CM Damage.
        public void physicalcmrecovery(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCMRecovery, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Whether Essence is added to Recovery Dice from Stun CM Damage.
        public void addesstostuncmrecovery(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AddESStoStunCMRecovery, _strUnique);
        }

        // Check for Whether Essence is added to Recovery Dice from Physical CM Damage.
        public void addesstophysicalcmrecovery(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AddESStoPhysicalCMRecovery, _strUnique);
        }

        // Check for Concealability.
        public void concealability(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Concealability, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Drain Resistance.
        public void drainresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainResistance, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Drain Value.
        public void drainvalue(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.Attributes?["specific"]?.InnerTextViaPool() ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainValue, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Fading Resistance.
        public void fadingresist(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FadingResistance, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Fading Value.
        public void fadingvalue(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.Attributes?["specific"]?.InnerTextViaPool() ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FadingValue, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Notoriety.
        public void notoriety(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Notoriety, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Street Cred bonuses.
        public void streetcred(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StreetCred, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Street Cred Multiplier bonuses.
        public void streetcredmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StreetCredMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Complex Form Limit.
        public void complexformlimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ComplexFormLimit, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Spell Limit.
        public void spelllimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpellLimit, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Free Spells.
        public void freespells(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlAttributeCollection objNodeAttributes = bonusNode.Attributes;
            if (objNodeAttributes != null)
            {
                string strSpellTypeLimit = objNodeAttributes["limit"]?.InnerTextViaPool() ?? string.Empty;
                if (objNodeAttributes["attribute"] != null)
                {
                    CharacterAttrib att = _objCharacter.GetAttribute(objNodeAttributes["attribute"].InnerTextViaPool());
                    if (att != null)
                    {
                        CreateImprovement(att.Abbrev, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsATT, strSpellTypeLimit);
                    }
                }
                else if (objNodeAttributes["skill"] != null)
                {
                    string strKey = objNodeAttributes["skill"].InnerTextViaPool();
                    CreateImprovement(strKey, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsSkill, strSpellTypeLimit);
                }
                else
                {
                    CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, _strUnique,
                        ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
                }
            }
            else
            {
                CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
            }
        }

        // Check for Spell Category bonuses.
        public void spellcategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            spellcategorydicepool(bonusNode);
        }

        public void spellcategorydicepool(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategory, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        // Check for dicepool bonuses for a specific Spell.
        public void spelldicepool(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["id"]?.InnerTextViaPool() ?? bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellDicePool, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        // Check for Spell Category Drain bonuses.
        public void spellcategorydrain(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string s = bonusNode["category"]?.InnerTextViaPool() ?? SelectedValue;
            if (string.IsNullOrWhiteSpace(s))
                throw new AbortedException();
            CreateImprovement(s, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategoryDrain, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        // Check for Spell Category Damage bonuses.
        public void spellcategorydamage(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["category"]?.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategoryDamage, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        // Check for Spell descriptor Damage bonuses.
        public void spelldescriptordamage(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["descriptor"]?.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellDescriptorDamage, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        // Check for Spell descriptor drain bonuses.
        public void spelldescriptordrain(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["descriptor"]?.InnerTextViaPool(), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellDescriptorDrain, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        // Check for Throwing Range bonuses.
        public void throwrange(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRange, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Throwing Range bonuses.
        public void throwrangestr(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRangeSTR, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Throwing STR bonuses.
        public void throwstr(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowSTR, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Skillsoft access.
        public void skillsoftaccess(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;
            CreateImprovement(string.Empty, _objImprovementSource, SourceName,
                              Improvement.ImprovementType.SkillsoftAccess, strUseUnique,
                              ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
            _objCharacter.SkillsSection.KnowledgeSkills.AddRange(_objCharacter.SkillsSection.KnowsoftSkills);
        }

        // Check for Quickening Metamagic.
        public void quickeningmetamagic(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.QuickeningMetamagic, _strUnique);
        }

        // Check for ignore Stun CM Penalty.
        public void ignorecmpenaltystun(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, _strUnique);
        }

        // Check for ignore Physical CM Penalty.
        public void ignorecmpenaltyphysical(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, _strUnique);
        }

        // Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
        public void cyborgessence(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyborgEssence, _strUnique);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public void essencepenalty(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenalty, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public void essencepenaltyt100(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyT100, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting MAG rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public void essencepenaltymagonlyt100(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyMAGOnlyT100, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting RES rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public void essencepenaltyresonlyt100(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyRESOnlyT100, _strUnique,
                              ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting DEP rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public void essencepenaltydeponlyt100(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyDEPOnlyT100, _strUnique,
                              ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for special attribute burn modifiers that stack additively.
        public void specialattburnmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialAttBurn, _strUnique,
                              ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for special attribute burn modifiers that stack multiplicatively.
        public void specialatttotalburnmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialAttTotalBurnMultiplier, _strUnique,
                              ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public void essencemax(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssenceMax, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Check for Select Sprite.
        public void selectsprite(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCritters))
            {
                using (XmlNodeList objXmlNodeList = _objCharacter.LoadData("critters.xml")
                                                                 .SelectNodes(
                                                                     "/chummer/metatypes/metatype[contains(category, \"Sprites\")]"))
                {
                    if (objXmlNodeList != null)
                    {
                        foreach (XmlNode objXmlNode in objXmlNodeList)
                        {
                            string strName = objXmlNode["name"]?.InnerTextViaPool();
                            lstCritters.Add(new ListItem(strName, objXmlNode["translate"]?.InnerTextViaPool() ?? strName));
                        }
                    }
                }

                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(lstCritters);

                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickItem.MyForm.SelectedItem;
                }

                CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.AddSprite,
                    _strUnique);
            }
        }

        // Check for Black Market Discount.
        public void blackmarketdiscount(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XPathNodeIterator nodeList = _objCharacter.LoadDataXPath("options.xml").SelectAndCacheExpression("/chummer/blackmarketpipelinecategories/category");
            SelectedValue = string.Empty;
            if (nodeList.Count > 0)
            {
                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                {
                    Description = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_Improvement_SelectText"), _strFriendlyName)
                }))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(nodeList.OfType<XPathNavigator>().Select(objNode =>
                        new ListItem(objNode.Value, objNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? objNode.Value)));

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        frmPickItem.MyForm.ForceItem(LimitSelection);
                        frmPickItem.MyForm.Opacity = 0;
                    }

                    // Make sure the dialogue window was not canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickItem.MyForm.SelectedName;
                }
            }

            // Create the Improvement.
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.BlackMarketDiscount,
                _strUnique);
        }

        // Select Armor (Mostly used for Custom Fit (Stack)).
        public void selectarmor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("armor.xml");
            XPathNodeIterator objXmlNodeList;
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                objXmlNodeList
                    = objXmlDocument.Select(
                        "/chummer/armors/armor[starts-with(name, " + bonusNode.InnerTextViaPool().CleanXPath() + ") and ("
                        + _objCharacter.Settings.BookXPath() + ") and mods[name = 'Custom Fit']]");
            }
            else
            {
                objXmlNodeList =
                    objXmlDocument.Select("/chummer/armors/armor[(" + _objCharacter.Settings.BookXPath() + ") and mods[name = 'Custom Fit']]");
            }

            //.SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" + _objCharacter.Settings.BookXPath() + ")" + SkillFilter(filter) + "]");

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstArmors))
            {
                if (objXmlNodeList.Count > 0)
                {
                    foreach (XPathNavigator objNode in objXmlNodeList)
                    {
                        string strName = objNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strName))
                            lstArmors.Add(new ListItem(
                                              strName,
                                              objNode.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                              ?? strName));
                    }
                }

                if (lstArmors.Count > 0)
                {
                    using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                    {
                        Description = string.Format(GlobalSettings.CultureInfo,
                                                           LanguageManager.GetString("String_Improvement_SelectText"),
                                                           _strFriendlyName)
                    }))
                    {
                        frmPickItem.MyForm.SetGeneralItemsMode(lstArmors);

                        if (!string.IsNullOrEmpty(LimitSelection))
                        {
                            frmPickItem.MyForm.ForceItem(LimitSelection);
                            frmPickItem.MyForm.Opacity = 0;
                        }

                        // Make sure the dialogue window was not canceled.
                        if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = frmPickItem.MyForm.SelectedItem;
                    }
                }
            }

            // Create the Improvement.
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique);
        }

        // Select a specific piece of Cyberware.
        public void selectcyberware(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            string strCategory = bonusNode["category"]?.InnerTextViaPool();
            XPathNodeIterator objXmlNodeList = _objCharacter.LoadDataXPath("cyberware.xml").Select(!string.IsNullOrEmpty(strCategory)
                ? "/chummer/cyberwares/cyberware[(category = " + strCategory.CleanXPath() + ") and (" + _objCharacter.Settings.BookXPath() + ")]"
                : "/chummer/cyberwares/cyberware[(" + _objCharacter.Settings.BookXPath() + ")]");

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> list))
            {
                if (objXmlNodeList.Count > 0)
                {
                    foreach (XPathNavigator objNode in objXmlNodeList)
                    {
                        string strName = objNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strName))
                            list.Add(new ListItem(
                                         strName,
                                         objNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
                    }
                }

                if (list.Count == 0)
                    throw new AbortedException();
                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                {
                    Description = string.Format(GlobalSettings.CultureInfo,
                                                       LanguageManager.GetString("String_Improvement_SelectText"),
                                                       _strFriendlyName)
                }))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(list);

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        frmPickItem.MyForm.ForceItem(LimitSelection);
                        frmPickItem.MyForm.Opacity = 0;
                    }

                    // Make sure the dialogue window was not canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickItem.MyForm.SelectedItem;
                }
            }

            // Create the Improvement.
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique);
        }

        // Select Weapon (custom entry for things like Spare Clip).
        public void selectweapon(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (_objCharacter == null)
            {
                // If the character is null (this is a Vehicle), the user must enter their own string.
                // Display the Select Item window and record the value that was entered.
                using (ThreadSafeForm<SelectText> frmPickText = ThreadSafeForm<SelectText>.Get(() => new SelectText
                {
                    Description = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_Improvement_SelectText"), _strFriendlyName)
                }))
                {
                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        frmPickText.MyForm.SelectedValue = LimitSelection;
                        frmPickText.MyForm.Opacity = 0;
                    }

                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickText.MyForm.SelectedValue;
                }
            }
            else
            {
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstWeapons))
                {
                    bool blnIncludeUnarmed = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@includeunarmed")?.Value == bool.TrueString;
                    string strExclude = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory")?.Value ?? string.Empty;
                    string strWeaponDetails = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@weapondetails")?.Value ?? string.Empty;
                    foreach (Weapon objWeapon in _objCharacter.Weapons.GetAllDescendants(x => x.Children))
                    {
                        if (!string.IsNullOrEmpty(strExclude) && objWeapon.RangeType == strExclude)
                            continue;
                        if (!blnIncludeUnarmed && objWeapon.Name == "Unarmed Attack")
                            continue;
                        if (!string.IsNullOrEmpty(strWeaponDetails)
                            && objWeapon.GetNodeXPath()?.SelectSingleNode("self::node()[" + strWeaponDetails + "]") == null)
                            continue;
                        lstWeapons.Add(new ListItem(objWeapon.InternalId,
                                                    objWeapon.CurrentDisplayNameShort));
                    }

                    if (string.IsNullOrWhiteSpace(LimitSelection)
                        || lstWeapons.Exists(item => item.Name == LimitSelection))
                    {
                        if (lstWeapons.Count == 0)
                        {
                            Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                                                 LanguageManager.GetString(
                                                                     "Message_Improvement_EmptySelectionListNamed"),
                                                                 SourceName));
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                        {
                            Description = string.Format(GlobalSettings.CultureInfo,
                                                               LanguageManager.GetString(
                                                                   "String_Improvement_SelectText"), _strFriendlyName)
                        }))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstWeapons);

                            if (!string.IsNullOrEmpty(LimitSelection))
                            {
                                frmPickItem.MyForm.ForceItem(LimitSelection);
                                frmPickItem.MyForm.Opacity = 0;
                            }

                            // Make sure the dialogue window was not canceled.
                            if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            {
                                throw new AbortedException();
                            }

                            SelectedValue = frmPickItem.MyForm.SelectedName;
                        }
                    }
                    else
                    {
                        SelectedValue = LimitSelection;
                    }
                }
            }

            // Create the Improvement.
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique);
        }

        // Select an Optional Power.
        public void optionalpowers(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            string strForcePower = !string.IsNullOrEmpty(LimitSelection) ? LimitSelection : string.Empty;
            int powerCount = 1;
            if (string.IsNullOrEmpty(strForcePower) && bonusNode.Attributes?["count"] != null)
            {
                string strCount = bonusNode.Attributes?["count"]?.InnerTextViaPool();
                if (strCount.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strCount = _objCharacter.ProcessAttributesInXPath(strCount);
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strCount);
                    powerCount = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                }
                else
                    powerCount = Math.Max(1, decValue.StandardRound());
            }

            for (int i = 0; i < powerCount; i++)
            {
                List<ValueTuple<string, string>> lstPowerExtraPairs;
                using (XmlNodeList xmlOptionalPowerList = bonusNode.SelectNodes("optionalpower"))
                {
                    lstPowerExtraPairs = new List<ValueTuple<string, string>>(xmlOptionalPowerList?.Count ?? 0);
                    if (xmlOptionalPowerList?.Count > 0)
                    {
                        foreach (XmlNode objXmlOptionalPower in xmlOptionalPowerList)
                        {
                            string strPower = objXmlOptionalPower.InnerTextViaPool();
                            if (string.IsNullOrEmpty(strForcePower) || strForcePower == strPower)
                            {
                                lstPowerExtraPairs.Add(new ValueTuple<string, string>(strPower,
                                    objXmlOptionalPower.Attributes?["select"]?.InnerTextViaPool()));
                            }
                        }
                    }
                }

                // Display the Select Critter Power window and record which power was selected.
                using (ThreadSafeForm<SelectOptionalPower> frmPickPower = ThreadSafeForm<SelectOptionalPower>.Get(() => new SelectOptionalPower(_objCharacter, lstPowerExtraPairs.ToArray())
                {
                    Description = LanguageManager.GetString("String_Improvement_SelectOptionalPower")
                }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickPower.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    // Record the improvement.
                    XmlNode objXmlPowerNode = _objCharacter.LoadData("critterpowers.xml")
                        .TryGetNodeByNameOrId("/chummer/powers/power", frmPickPower.MyForm.SelectedPower);
                    CritterPower objPower = new CritterPower(_objCharacter);
                    objPower.Create(objXmlPowerNode, 0, frmPickPower.MyForm.SelectedPowerExtra);
                    if (objPower.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    objPower.Grade = -1;
                    _objCharacter.CritterPowers.Add(objPower);
                    CreateImprovement(objPower.InternalId, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.CritterPower, _strUnique);
                }
            }
        }

        public void critterpowers(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("critterpowers.xml");
            using (XmlNodeList xmlPowerList = bonusNode.SelectNodes("power"))
            {
                if (xmlPowerList?.Count > 0)
                {
                    foreach (XmlNode objXmlPower in xmlPowerList)
                    {
                        XmlNode objXmlCritterPower = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", objXmlPower.InnerTextViaPool());
                        CritterPower objPower = new CritterPower(_objCharacter);
                        string strForcedValue = string.Empty;
                        int intRating = 0;
                        if (objXmlPower.Attributes?.Count > 0)
                        {
                            string strRating = objXmlPower.Attributes["rating"]?.InnerTextViaPool();
                            if (!string.IsNullOrEmpty(strRating))
                                intRating = ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);
                            strForcedValue = objXmlPower.Attributes["select"]?.InnerTextViaPool();
                        }

                        objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                        objPower.Grade = -1;
                        _objCharacter.CritterPowers.Add(objPower);
                        CreateImprovement(objPower.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPower, _strUnique);
                    }
                }
            }
        }

        // Check for Adept Power Points.
        public void critterpowerlevels(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList xmlPowerList = bonusNode.SelectNodes("power"))
            {
                if (xmlPowerList?.Count > 0)
                {
                    foreach (XmlNode objXmlPower in xmlPowerList)
                    {
                        CreateImprovement(objXmlPower["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPowerLevel,
                            _strUnique,
                            ImprovementManager.ValueToDec(_objCharacter, objXmlPower["val"]?.InnerTextViaPool(), _intRating));
                    }
                }
            }
        }

        public void publicawareness(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PublicAwareness, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void dealerconnection(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstItems))
            {
                using (XmlNodeList objXmlList = bonusNode.SelectNodes("category"))
                {
                    if (objXmlList?.Count > 0)
                    {
                        foreach (XmlNode objNode in objXmlList)
                        {
                            string strText = objNode.InnerTextViaPool();
                            if (!string.IsNullOrEmpty(strText) && ImprovementManager
                                .GetCachedImprovementListForValueOf(_objCharacter,
                                                                    Improvement.ImprovementType.DealerConnection)
                                .TrueForAll(x => x.UniqueName != strText))
                            {
                                lstItems.Add(new ListItem(strText,
                                                          LanguageManager.GetString(
                                                              "String_DealerConnection_" + strText)));
                            }
                        }
                    }
                }

                if (lstItems.Count == 0)
                {
                    Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                                                  LanguageManager.GetString(
                                                                      "Message_Improvement_EmptySelectionListNamed"),
                                                                  SourceName));
                    throw new AbortedException();
                }

                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                {
                    AllowAutoSelect = false
                }))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(lstItems);
                    // Make sure the dialogue window was not canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickItem.MyForm.SelectedItem;
                }

                // Create the Improvement.
                CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.DealerConnection, SelectedValue);
            }
        }

        public void unlockskills(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            List<string> options = bonusNode.InnerTextViaPool().SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            string final;
            switch (options.Count)
            {
                case 0:
                    Utils.BreakIfDebug();
                    throw new AbortedException();
                case 1:
                    final = options[0];
                    break;

                default:
                {
                    using (ThreadSafeForm<SelectItem> frmSelect = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                    {
                        AllowAutoSelect = true
                    }))
                    {
                        frmSelect.MyForm.SetGeneralItemsMode(options.Select(x => new ListItem(x, x)));

                        if (_objCharacter.PushText.TryPop(out string strText))
                        {
                            frmSelect.MyForm.ForceItem(strText);
                        }

                        if (frmSelect.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        final = frmSelect.MyForm.SelectedItem;
                    }

                    break;
                }
            }

            if (Enum.TryParse(final, out SkillsSection.FilterOption skills))
            {
                string strName = bonusNode.Attributes?["name"]?.InnerTextViaPool() ?? string.Empty;
                _objCharacter.SkillsSection.AddSkills(skills, strName);
                CreateImprovement(skills.ToString(), _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialSkills, _strUnique, strTarget: strName);
            }
            else
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Failed to parse", "specialskills", bonusNode.OuterXmlViaPool() });
            }
        }

        public void addqualities(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("qualities.xml");
            using (XmlNodeList xmlQualityList = bonusNode.SelectNodes("addquality"))
            {
                if (xmlQualityList?.Count > 0)
                {
                    foreach (XmlNode objXmlAddQuality in xmlQualityList)
                    {
                        if (objXmlAddQuality.NodeType == XmlNodeType.Comment) continue;
                        XmlNode objXmlSelectedQuality = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", objXmlAddQuality.InnerTextViaPool());
                        if (objXmlSelectedQuality == null)
                        {
                            Utils.BreakIfDebug();
                        }
                        string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;

                        string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerTextViaPool();
                        int intCount = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);
                        bool blnDoesNotContributeToBP = !string.Equals(objXmlAddQuality.Attributes?["contributetobp"]?.InnerTextViaPool(), bool.TrueString, StringComparison.OrdinalIgnoreCase);
                        bool blnForced = objXmlAddQuality.Attributes?["forced"]?.InnerTextIsTrueString() == true;
                        AddQuality(intCount, blnForced, objXmlSelectedQuality, strForceValue, blnDoesNotContributeToBP);
                    }
                }
            }
        }

        public void selectquality(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = _objCharacter.LoadData("qualities.xml");
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstQualities))
            {
                using (XmlNodeList xmlQualityList = bonusNode.SelectNodes("quality"))
                {
                    if (xmlQualityList?.Count > 0)
                    {
                        foreach (XmlNode objXmlAddQuality in xmlQualityList)
                        {
                            string strName = objXmlAddQuality.InnerTextViaPool();
                            XmlNode objXmlQuality
                                = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strName);
                            // Makes sure we aren't over our limits for this particular quality from this overall source
                            if (objXmlAddQuality.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                                objXmlQuality?.CreateNavigator().RequirementsMet(_objCharacter) == true)
                            {
                                lstQualities.Add(
                                    new ListItem(strName, objXmlQuality?["translate"]?.InnerTextViaPool() ?? strName));
                            }
                        }
                    }
                }

                if (lstQualities.Count == 0)
                {
                    Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                                                  LanguageManager.GetString(
                                                                      "Message_Improvement_EmptySelectionListNamed"),
                                                                  SourceName));
                    throw new AbortedException();
                }

                XmlNode objXmlSelectedQuality;
                XmlNode objXmlBonusQuality;
                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(lstQualities);

                    // Don't do anything else if the form was canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        throw new AbortedException();
                    objXmlSelectedQuality = objXmlDocument.TryGetNodeByNameOrId(
                        "/chummer/qualities/quality", frmPickItem.MyForm.SelectedItem);
                    objXmlBonusQuality
                        = bonusNode.SelectSingleNode("quality[. = " + frmPickItem.MyForm.SelectedItem.CleanXPath() + "]");
                }

                int intQualityDiscount = 0;

                if (bonusNode["discountqualities"] != null)
                {
                    string strForceDiscountValue;
                    lstQualities.Clear();
                    lstQualities.Add(new ListItem("None", LanguageManager.GetString("String_None")));
                    using (XmlNodeList xmlQualityNodeList = bonusNode.SelectNodes("discountqualities/quality"))
                    {
                        if (xmlQualityNodeList?.Count > 0)
                        {
                            foreach (XmlNode objXmlAddQuality in xmlQualityNodeList)
                            {
                                strForceDiscountValue = objXmlAddQuality.SelectSingleNodeAndCacheExpressionAsNavigator("@select")?.Value ?? string.Empty;
                                string strName = objXmlAddQuality.InnerTextViaPool();

                                XmlNode objXmlQuality
                                    = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strName);
                                if (objXmlQuality != null)
                                {
                                    string strDisplayName = objXmlQuality["translate"]?.InnerTextViaPool() ?? strName;
                                    if (!string.IsNullOrWhiteSpace(strForceDiscountValue))
                                        strDisplayName += " (" + strForceDiscountValue + ")";
                                    lstQualities.Add(new ListItem(strName, strDisplayName));
                                }
                            }
                        }
                    }

                    if (lstQualities.Count == 0)
                    {
                        Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                                             LanguageManager.GetString(
                                                                 "Message_Improvement_EmptySelectionListNamed"),
                                                             SourceName));
                        throw new AbortedException();
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem
                           = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                    {
                        frmPickItem.MyForm.SetGeneralItemsMode(lstQualities);

                        // Don't do anything else if the form was canceled.
                        if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            throw new AbortedException();
                        if (frmPickItem.MyForm.SelectedItem != "None")
                        {
                            objXmlSelectedQuality
                                = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality",
                                                                      frmPickItem.MyForm.SelectedItem);
                            objXmlBonusQuality
                                = bonusNode.SelectSingleNode("discountqualities/quality[. = "
                                                             + frmPickItem.MyForm.SelectedItem.CleanXPath() + "]");
                            intQualityDiscount
                                = ImprovementManager.ValueToInt(_objCharacter, objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@discount")?.Value, _intRating);
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality discountQuality = new Quality(_objCharacter);
                            try
                            {
                                discountQuality.BP = 0;
                                strForceDiscountValue = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@select")?.Value;
                                discountQuality.Create(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons,
                                    strForceDiscountValue, _strFriendlyName);
                                CreateImprovement(discountQuality.InternalId, _objImprovementSource, SourceName,
                                                  Improvement.ImprovementType.SpecificQuality, _strUnique);
                            }
                            catch
                            {
                                discountQuality.DeleteQuality();
                                throw;
                            }

                            _objCharacter.Qualities.Add(discountQuality);
                            // Create any Weapons that came with this ware.
                            foreach (Weapon objWeapon in lstWeapons)
                                _objCharacter.Weapons.Add(objWeapon);
                        }
                    }
                }
                string strForceValue = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@select")?.Value;
                bool blnDoesNotContributeToBP = !string.Equals(objXmlSelectedQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@contributetobp")?.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
                bool blnForced = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@forced")?.Value == bool.TrueString;
                string strRating = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@rating")?.Value;
                int intCount = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);
                AddQuality(intCount, blnForced, objXmlSelectedQuality, strForceValue, blnDoesNotContributeToBP,
                    intQualityDiscount);
            }
        }

        public void addskillspecialization(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSkill = bonusNode["skill"]?.InnerTextViaPool() ?? string.Empty;
            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strSkill);
            if (objSkill != null)
            {
                // Create the Improvement.
                string strSpec = bonusNode["spec"]?.InnerTextViaPool() ?? string.Empty;
                SkillSpecialization objSpec = new SkillSpecialization(_objCharacter, objSkill, strSpec);
                try
                {
                    objSkill.Specializations.Add(objSpec);
                    CreateImprovement(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, objSpec.InternalId);
                }
                catch
                {
                    objSpec.Dispose();
                    throw;
                }
            }
        }

        public void addskillspecializationoption(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlSkillsList = bonusNode.SelectNodes("skills/skill");
            List<Skill> lstSkills = new List<Skill>(xmlSkillsList?.Count ?? 0);
            if (xmlSkillsList?.Count > 0)
            {
                foreach (XmlNode objNode in xmlSkillsList)
                {
                    Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(objNode.InnerTextViaPool());
                    if (objSkill != null)
                    {
                        lstSkills.Add(objSkill);
                    }
                }
            }
            else
            {
                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(bonusNode["skill"]?.InnerTextViaPool() ?? string.Empty);
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
                    string strSpec = bonusNode["spec"]?.InnerTextViaPool();
                    CreateImprovement(objSkill.DictionaryKey, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecializationOption, strSpec);
                    if (_objCharacter.Settings.FreeMartialArtSpecialization && _objImprovementSource == Improvement.ImprovementSource.MartialArt)
                    {
                        SkillSpecialization objSpec = new SkillSpecialization(_objCharacter, objSkill, strSpec);
                        try
                        {
                            objSkill.Specializations.Add(objSpec);
                            CreateImprovement(objSkill.DictionaryKey, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, objSpec.InternalId);
                        }
                        catch
                        {
                            objSpec.Dispose();
                            throw;
                        }
                    }
                }
            }
        }

        public void allowspellrange(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), Improvement.ImprovementType.AllowSpellRange);
        }

        public void allowspellcategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                CreateImprovement(bonusNode.InnerTextViaPool(), Improvement.ImprovementType.AllowSpellCategory);
            }
            else
            {
                // Display the Select Spell window.
                using (ThreadSafeForm<SelectSpellCategory> frmPickSpellCategory = ThreadSafeForm<SelectSpellCategory>.Get(() => new SelectSpellCategory(_objCharacter)
                {
                    Description = LanguageManager.GetString("Title_SelectSpellCategory")
                }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickSpellCategory.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    CreateImprovement(frmPickSpellCategory.MyForm.SelectedCategory, Improvement.ImprovementType.AllowSpellCategory);
                }
            }
        }

        public void limitspellrange(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), Improvement.ImprovementType.LimitSpellRange);
        }

        public void limitspellcategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                CreateImprovement(bonusNode.InnerTextViaPool(), Improvement.ImprovementType.LimitSpellCategory);
            }
            else
            {
                // Display the Select Spell window.
                using (ThreadSafeForm<SelectSpellCategory> frmPickSpellCategory = ThreadSafeForm<SelectSpellCategory>.Get(() => new SelectSpellCategory(_objCharacter)
                {
                    Description = LanguageManager.GetString("Title_SelectSpellCategory")
                }))
                {
                    frmPickSpellCategory.MyForm.SetExcludeCategories(bonusNode.Attributes?["exclude"]?.InnerTextViaPool().SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries));

                    // Make sure the dialogue window was not canceled.
                    if (frmPickSpellCategory.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    CreateImprovement(frmPickSpellCategory.MyForm.SelectedCategory, Improvement.ImprovementType.LimitSpellCategory);
                }
            }
        }

        public void limitspelldescriptor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Display the Select Spell window.
            string strSelected;
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                strSelected = bonusNode.InnerTextViaPool();
            }
            else
            {
                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                {
                    Description = LanguageManager.GetString("Title_SelectSpellDescriptor")
                }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelected = frmPickItem.MyForm.SelectedItem;
                }
            }
            CreateImprovement(strSelected, Improvement.ImprovementType.LimitSpellDescriptor);
        }

        public void blockspelldescriptor(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Display the Select Spell window.
            string strSelected;
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                strSelected = bonusNode.InnerTextViaPool();
            }
            else
            {
                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                {
                    Description = LanguageManager.GetString("Title_SelectSpellDescriptor")
                }))
                {
                    // Make sure the dialogue window was not canceled.
                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelected = frmPickItem.MyForm.SelectedItem;
                }
            }
            CreateImprovement(strSelected, Improvement.ImprovementType.BlockSpellDescriptor);
        }

        #region addspiritorsprite

        /// <summary>
        /// Improvement type that adds to the available sprite types a character can summon.
        /// </summary>
        /// <param name="bonusNode"></param>
        public void addsprite(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlAllowedSpirits = bonusNode.SelectNodes("spirit");
            bool blnAddToSelected = true;
            string strAddToSelected = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("addtoselected")?.Value;
            if (!string.IsNullOrEmpty(strAddToSelected))
            {
                blnAddToSelected = Convert.ToBoolean(strAddToSelected, GlobalSettings.InvariantCultureInfo);
            }
            AddSpiritOrSprite("streams.xml", xmlAllowedSpirits, Improvement.ImprovementType.AddSprite, blnAddToSelected, "Sprites");
        }

        /// <summary>
        /// Improvement type that adds to the available spirit types a character can summon.
        /// </summary>
        /// <param name="bonusNode"></param>
        public void addspirit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlAllowedSpirits = bonusNode.SelectNodes("spirit");
            bool blnAddToSelected = true;
            string strAddToSelected = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("addtoselected")?.Value;
            if (!string.IsNullOrEmpty(strAddToSelected))
            {
                blnAddToSelected = Convert.ToBoolean(strAddToSelected, GlobalSettings.InvariantCultureInfo);
            }
            AddSpiritOrSprite("traditions.xml", xmlAllowedSpirits, Improvement.ImprovementType.AddSpirit, blnAddToSelected, "Spirits");
        }

        /// <summary>
        /// Improvement type that limits the spirits a character can summon to a particular category.
        /// </summary>
        /// <param name="bonusNode"></param>
        public void limitspiritcategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlAllowedSpirits = bonusNode.SelectNodes("spirit");
            bool blnAddToSelected = true;
            string strAddToSelected = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("addtoselected")?.Value;
            if (!string.IsNullOrEmpty(strAddToSelected))
            {
                blnAddToSelected = Convert.ToBoolean(strAddToSelected, GlobalSettings.InvariantCultureInfo);
            }
            AddSpiritOrSprite("traditions.xml", xmlAllowedSpirits, Improvement.ImprovementType.LimitSpiritCategory, blnAddToSelected);
        }

        private void AddSpiritOrSprite(string strXmlDoc, XmlNodeList xmlAllowedSpirits, Improvement.ImprovementType impType, bool addToSelectedValue = true, string strCritterCategory = "")
        {
            if (xmlAllowedSpirits == null)
                throw new ArgumentNullException(nameof(xmlAllowedSpirits));
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setAllowed))
            {
                foreach (XmlNode n in xmlAllowedSpirits)
                {
                    setAllowed.Add(n.InnerTextViaPool());
                }

                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSpirits))
                {
                    foreach (XPathNavigator xmlSpirit in _objCharacter.LoadDataXPath(strXmlDoc)
                                                                      .SelectAndCacheExpression(
                                                                          "/chummer/spirits/spirit"))
                    {
                        string strSpiritName = xmlSpirit.SelectSingleNodeAndCacheExpression("name")?.Value;
                        if (setAllowed.All(l => strSpiritName != l) && setAllowed.Count != 0)
                            continue;
                        lstSpirits.Add(new ListItem(strSpiritName,
                                                    xmlSpirit.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                    ?? strSpiritName));
                    }

                    if (!string.IsNullOrEmpty(strCritterCategory))
                    {
                        foreach (XPathNavigator xmlSpirit in _objCharacter.LoadDataXPath("critters.xml")
                                                                          .Select(
                                                                              "/chummer/critters/critter[category = "
                                                                              + strCritterCategory.CleanXPath() + "]"))
                        {
                            string strSpiritName = xmlSpirit.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (setAllowed.All(l => strSpiritName != l) && setAllowed.Count != 0)
                                continue;
                            lstSpirits.Add(new ListItem(strSpiritName,
                                                        xmlSpirit.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                        ?? strSpiritName));
                        }
                    }

                    using (ThreadSafeForm<SelectItem> frmSelect = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
                    {
                        frmSelect.MyForm.SetGeneralItemsMode(lstSpirits);
                        frmSelect.MyForm.ForceItem(ForcedValue);
                        if (frmSelect.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        if (addToSelectedValue)
                        {
                            if (string.IsNullOrEmpty(SelectedValue))
                                SelectedValue = frmSelect.MyForm.SelectedItem;
                            else
                                SelectedValue += ", " + frmSelect.MyForm.SelectedItem; // Do not use localized value for space because we expect the selected value to be in English
                        }

                        CreateImprovement(frmSelect.MyForm.SelectedItem, _objImprovementSource, SourceName, impType,
                                          _strUnique);
                    }
                }
            }
        }

        #endregion addspiritorsprite

        public void movementreplace(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            Improvement.ImprovementType imp = Improvement.ImprovementType.WalkSpeed;
            string strSpeed = bonusNode["speed"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strSpeed))
            {
                switch (strSpeed.ToUpperInvariant())
                {
                    case "RUN":
                        imp = Improvement.ImprovementType.RunSpeed;
                        break;

                    case "SPRINT":
                        imp = Improvement.ImprovementType.SprintSpeed;
                        break;
                }
            }

            string strNodeValText = bonusNode["val"]?.InnerTextViaPool();
            string strCategory = bonusNode["category"]?.InnerTextViaPool();
            if (!string.IsNullOrEmpty(strCategory))
            {
                CreateImprovement(strCategory, _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, strNodeValText, _intRating));
            }
            else
            {
                CreateImprovement("Ground", _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, strNodeValText, _intRating));
                CreateImprovement("Swim", _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, strNodeValText, _intRating));
                CreateImprovement("Fly", _objImprovementSource, SourceName, imp, _strUnique,
                    ImprovementManager.ValueToDec(_objCharacter, strNodeValText, _intRating));
            }
        }

        public void addlimb(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence");
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            CreateImprovement(bonusNode["limbslot"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AddLimb, strUseUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        public void attributekarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void activeskillkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgroupkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void knowledgeskillkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void knowledgeskillkarmacostmin(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillKarmaCostMinimum, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skilldisable(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillDisable, _strUnique);
        }

        public void skillenablemovement(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillEnableMovement, _strUnique);
        }

        public void skillgroupdisable(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupDisable, _strUnique);
        }

        public void skillgroupdisablechoice(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else
            {
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
                {
                    using (XmlNodeList objXmlGroups = bonusNode.SelectNodes("skillgroup"))
                    {
                        if (objXmlGroups?.Count > 0)
                        {
                            foreach (XmlNode objXmlGroup in objXmlGroups)
                            {
                                string strInnerText = objXmlGroup.InnerTextViaPool();
                                lstSkills.Add(new ListItem(strInnerText,
                                                           _objCharacter.TranslateExtra(
                                                               strInnerText, GlobalSettings.Language,
                                                               "skills.xml")));
                            }
                        }
                        else
                        {
                            foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
                            {
                                if (!objGroup.IsDisabled)
                                    lstSkills.Add(new ListItem(objGroup.Name, objGroup.CurrentDisplayName));
                            }
                        }
                    }

                    if (lstSkills.Count > 1)
                    {
                        lstSkills.Sort(CompareListItems.CompareNames);
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                    {
                        Description = LanguageManager.GetString("String_DisableSkillGroupPrompt"),
                        AllowAutoSelect = false
                    }))
                    {
                        frmPickItem.MyForm.SetGeneralItemsMode(lstSkills);

                        // Make sure the dialogue window was not canceled.
                        if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = frmPickItem.MyForm.SelectedName;
                    }
                }
            }

            CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupDisable, _strUnique);
        }

        public void skillgroupcategorydisable(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryDisable, _strUnique);
        }

        public void skillgroupcategorykarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillcategorykarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillcategoryspecializationkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategorySpecializationKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void attributepointcost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributePointCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void activeskillpointcost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillPointCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgrouppointcost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupPointCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void knowledgeskillpointcost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillPointCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgroupcategorypointcost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryPointCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillcategorypointcost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryPointCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newspellkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.Attributes?["type"]?.InnerTextViaPool() ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewSpellKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newcomplexformkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewComplexFormKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newaiprogramkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIProgramKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newaiadvancedprogramkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIAdvancedProgramKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void attributekarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void activeskillkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgroupkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void knowledgeskillkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgroupcategorykarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillcategorykarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillcategoryspecializationkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void attributepointcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributePointCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void activeskillpointcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgrouppointcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void knowledgeskillpointcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillgroupcategorypointcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void skillcategorypointcostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryPointCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating),
                1, ImprovementManager.ValueToInt(_objCharacter, bonusNode["min"]?.InnerTextViaPool(), _intRating), ImprovementManager.ValueToInt(_objCharacter, bonusNode["max"]?.InnerTextViaPool(), _intRating),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newspellkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.Attributes?["type"]?.InnerTextViaPool() ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewSpellKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newcomplexformkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewComplexFormKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newaiprogramkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIProgramKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void newaiadvancedprogramkarmacostmultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void blockskillspecializations(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillSpecializations, _strUnique);
        }

        public void blockskillcategoryspecializations(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillCategorySpecializations, _strUnique);
        }

        // Flat modifier to cost of binding a focus
        public void focusbindingkarmacost(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.FocusBindingKarmaCost, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, bonusNode["extracontains"]?.InnerTextViaPool() ?? string.Empty);
        }

        // Flat modifier to the number that is multiplied by a focus' rating to get the focus' binding karma cost
        public void focusbindingkarmamultiplier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.FocusBindingKarmaMultiplier, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating), 1, 0, 0, 0, 0, string.Empty, false, bonusNode["extracontains"]?.InnerTextViaPool() ?? string.Empty);
        }

        public void magicianswaydiscount(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MagiciansWayDiscount, _strUnique);
        }

        public void burnoutsway(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BurnoutsWay, _strUnique);
        }

        // Add a specific Cyber/Bioware to the Character.
        public void addware(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode node;
            Improvement.ImprovementSource eSource;
            string strName = bonusNode["name"]?.InnerTextViaPool();
            if (string.IsNullOrEmpty(strName))
                throw new AbortedException();
            if (bonusNode["type"]?.InnerTextViaPool() == "bioware")
            {
                node = _objCharacter.LoadData("bioware.xml").TryGetNodeByNameOrId("/chummer/biowares/bioware", strName);
                eSource = Improvement.ImprovementSource.Bioware;
            }
            else
            {
                node = _objCharacter.LoadData("cyberware.xml").TryGetNodeByNameOrId("/chummer/cyberwares/cyberware", strName);
                eSource = Improvement.ImprovementSource.Cyberware;
            }

            if (node == null)
                throw new AbortedException();
            string strRating = bonusNode["rating"]?.InnerTextViaPool();
            int intRating = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, _intRating);
            string strSourceId = node["id"]?.InnerTextViaPool();
            string strImprovedName = strSourceId;
            if (string.Equals(strSourceId, Cyberware.EssenceAntiHoleGuidString, StringComparison.OrdinalIgnoreCase))
            {
                _objCharacter.DecreaseEssenceHole(intRating);
            }
            else if (string.Equals(strSourceId, Cyberware.EssenceHoleGuidString, StringComparison.OrdinalIgnoreCase))
            {
                _objCharacter.IncreaseEssenceHole(intRating);
            }
            else
            {
                // Create the new piece of ware.
                Grade objGrade = Grade.ConvertToCyberwareGrade(bonusNode["grade"]?.InnerTextViaPool(), _objImprovementSource,
                    _objCharacter);
                List<Weapon> lstWeapons = new List<Weapon>(1);
                List<Vehicle> lstVehicles = new List<Vehicle>(1);
                Cyberware objCyberware = new Cyberware(_objCharacter);
                try
                {
                    objCyberware.Create(node, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, true,
                        ForcedValue);

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
                    strImprovedName = objCyberware.InternalId;
                }
                catch
                {
                    if (lstWeapons.Count > 0)
                    {
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            objWeapon.DeleteWeapon();
                        }
                    }

                    if (lstVehicles.Count > 0)
                    {
                        foreach (Vehicle objVehicle in lstVehicles)
                        {
                            objVehicle.DeleteVehicle();
                        }
                    }
                    objCyberware.DeleteCyberware();
                    throw;
                }
            }

            CreateImprovement(strImprovedName, _objImprovementSource, SourceName,
                Improvement.ImprovementType.FreeWare,
                _strUnique, intRating: intRating);
        }

        public void weaponaccuracy(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool() ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponAccuracy, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["value"]?.InnerTextViaPool(), _intRating));
        }

        public void weaponrangemodifier(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode["name"]?.InnerTextViaPool() ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponRangeModifier, _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["value"]?.InnerTextViaPool(), _intRating));
        }

        public void weaponskillaccuracy(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = ForcedValue;
            if (string.IsNullOrEmpty(strSelectedValue))
            {
                XmlElement xmlSelectSkillNode = bonusNode["selectskill"];
                if (xmlSelectSkillNode != null)
                {
                    bool blnKnowledgeSkill;
                    (strSelectedValue, blnKnowledgeSkill) = ImprovementManager.DoSelectSkill(xmlSelectSkillNode, _objCharacter, _intRating,
                            _strFriendlyName);
                    if (blnKnowledgeSkill)
                        throw new AbortedException();
                }
                else
                    strSelectedValue = bonusNode["name"]?.InnerTextViaPool() ?? string.Empty;
            }

            SelectedValue = strSelectedValue;
            string strVal = bonusNode["value"]?.InnerTextViaPool();

            CreateImprovement(strSelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponSkillAccuracy, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, strVal, _intRating));
        }

        public void metageniclimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MetageneticLimit, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void specialmodificationlimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialModificationLimit, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void cyberadeptdaemon(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberadeptDaemon, _strUnique);
        }

        /// <summary>
        /// Improvement increases the Dice Pool for a specific named Action.
        /// TODO: Link to actions.xml when we implement that.
        /// </summary>
        /// <param name="bonusNode"></param>
        public void actiondicepool(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else if (bonusNode["name"] != null)
            {
                SelectedValue = bonusNode["name"].InnerTextViaPool();
            }
            else
            {
                // Populate the Magician Traditions list.
                XPathNavigator xmlActionsBaseNode =
                    _objCharacter.LoadDataXPath("actions.xml").SelectSingleNodeAndCacheExpression("/chummer");
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstActions))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdXPath))
                    {
                        sbdXPath.Append("actions/action[", _objCharacter.Settings.BookXPath());
                        string strCategory = bonusNode.Attributes?["category"]?.InnerTextViaPool() ?? string.Empty;
                        if (!string.IsNullOrEmpty(strCategory))
                        {
                            sbdXPath.Append(" and category = ", strCategory.CleanXPath());
                        }
                        sbdXPath.Append(']');
                        if (xmlActionsBaseNode != null)
                        {
                            foreach (XPathNavigator xmlAction in xmlActionsBaseNode.Select(sbdXPath.ToString()))
                            {
                                string strName = xmlAction.SelectSingleNodeAndCacheExpression("name")?.Value;
                                if (!string.IsNullOrEmpty(strName))
                                    lstActions.Add(new ListItem(
                                                       xmlAction.SelectSingleNodeAndCacheExpression("id")?.Value
                                                       ?? strName,
                                                       xmlAction.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                       ?? strName));
                            }
                        }
                    }

                    if (lstActions.Count > 1)
                    {
                        lstActions.Sort(CompareListItems.CompareNames);
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                    {
                        AllowAutoSelect = false
                    }))
                    {
                        frmPickItem.MyForm.SetDropdownItemsMode(lstActions);

                        // Make sure the dialogue window was not canceled.
                        if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = frmPickItem.MyForm.SelectedName;
                    }
                }
            }
            CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.ActionDicePool,
                _strUnique, ImprovementManager.ValueToDec(_objCharacter, bonusNode["val"]?.InnerTextViaPool(), _intRating));
        }

        public void contactkarma(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactKarmaDiscount, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        public void contactkarmaminimum(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactKarmaMinimum, _strUnique,
                ImprovementManager.ValueToDec(_objCharacter, bonusNode.InnerTextViaPool(), _intRating));
        }

        // Enable Sprite Fettering.
        public void allowspritefettering(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AllowSpriteFettering, _strUnique);
        }

        // Enable the Convert to Cyberzombie methods.
        public void enablecyberzombie(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EnableCyberzombie, _strUnique);
        }

        public void allowcritterpowercategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AllowCritterPowerCategory, _strUnique);
        }

        public void limitcritterpowercategory(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.LimitCritterPowerCategory, _strUnique);
        }

        public void attributemaxclamp(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeMaxClamp, _strUnique);
        }

        public void metamagiclimit(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlMetamagicsList = bonusNode.SelectNodes("metamagic");
            if (xmlMetamagicsList != null)
            {
                foreach (XmlNode child in xmlMetamagicsList)
                {
                    int intRating = -1;
                    string strGrade = child.Attributes?["grade"]?.InnerTextViaPool();
                    if (!string.IsNullOrEmpty(strGrade))
                        intRating = ImprovementManager.ValueToInt(_objCharacter, strGrade, _intRating);
                    CreateImprovement(child.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.MetamagicLimit, _strUnique, 0, intRating);
                }
            }
        }

        public void disablequality(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.DisableQuality, _strUnique);
        }

        public void freequality(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.FreeQuality, _strUnique);
        }

        public void selectexpertise(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Select the skill to get the expertise
            string strForcedValue = ForcedValue;
            ForcedValue = string.Empty; // Temporarily clear Forced Value because the Forced Value should be for the specialization name, not the skill
            string strSkill = ImprovementManager.DoSelectSkill(bonusNode, _objCharacter, _intRating, _strFriendlyName).Item1;
            ForcedValue = strForcedValue;
            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strSkill) ?? throw new AbortedException();
            // Select the actual specialization to add as an expertise
            using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(() => new SelectItem()))
            {
                string strLimitToSpecialization = bonusNode.Attributes?["limittospecialization"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strLimitToSpecialization))
                    frmPickItem.MyForm.SetDropdownItemsMode(strLimitToSpecialization.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                        .Where(x => objSkill.Specializations.All(y => y.Name != x)).Select(x => new ListItem(x, _objCharacter.TranslateExtra(x, GlobalSettings.Language, "skills.xml"))));
                else
                    frmPickItem.MyForm.SetGeneralItemsMode(objSkill.CGLSpecializations);
                if (!string.IsNullOrEmpty(ForcedValue))
                    frmPickItem.MyForm.ForceItem(ForcedValue);

                frmPickItem.MyForm.AllowAutoSelect = !string.IsNullOrEmpty(ForcedValue);

                // Make sure the dialogue window was not canceled.
                string strSelected = frmPickItem.MyForm.SelectedName;
                if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel || string.IsNullOrEmpty(strSelected))
                {
                    throw new AbortedException();
                }

                SelectedValue = strSelected;
            }
            // Create the Improvement.
            SkillSpecialization objExpertise = new SkillSpecialization(_objCharacter, objSkill, SelectedValue, true, true);
            try
            {
                objSkill.Specializations.Add(objExpertise);
                CreateImprovement(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, objExpertise.InternalId);
            }
            catch
            {
                objExpertise.Dispose();
                throw;
            }
        }

        public void penaltyfreesustain(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strDummy = string.Empty;
            int intCount = 1;
            if (bonusNode.TryGetStringFieldQuickly("count", ref strDummy))
                intCount = ImprovementManager.ValueToInt(_objCharacter, strDummy, _intRating);
            int intMaxForce = int.MaxValue;
            if (bonusNode.TryGetStringFieldQuickly("force", ref strDummy))
                intMaxForce = ImprovementManager.ValueToInt(_objCharacter, strDummy, _intRating);
            CreateImprovement(bonusNode.InnerTextViaPool(), _objImprovementSource, SourceName, Improvement.ImprovementType.PenaltyFreeSustain, _strUnique, intMaxForce, intCount);
        }

        /// <summary>
        /// Replaces the Active Skill used for one or all spells with another. Example:
        /// <replacespellskill spell="Mana Net">Blades</replacespellskill> Replace the Spellcasting skill with Blades for the spell Mana Net.
        /// <replacespellskill>Blades</replacespellskill> Replace the Spellcasting skill with Blades for ALL spells that are not marked as Alchemical or BareHandedAdept.
        /// </summary>
        public void replaceskillspell(XmlNode bonusNode)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            CreateImprovement(bonusNode.Attributes?["spell"]?.InnerTextViaPool() ?? string.Empty, bonusNode.InnerTextViaPool(), Improvement.ImprovementType.ReplaceSkillSpell);
        }

#pragma warning restore IDE1006 // Naming Styles

        #endregion Improvement Methods
        #region Helper Methods
        /// <summary>
        /// Create and add a named Quality to the character.
        /// </summary>
        /// <param name="intCount">Total number of the instances of the quality to attempt to add. Used to proxy giving a quality of a given Rating.</param>
        /// <param name="blnForced">Whether to ignore the required/forbidden nodes of the quality</param>
        /// <param name="objXmlSelectedQuality">XmlNode of the selected quality</param>
        /// <param name="strForceValue">Forced Extra value for the quality</param>
        /// <param name="blnDoesNotContributeToBP">Whether the quality contributes to BP or not. If false, BP will be set to 0.</param>
        /// <param name="intQualityDiscount">Total reduced karma cost of the quality as provided by costdiscount nodes or similar. Incompatible with blnDoesNotContributeToBP. Minimum of 1.</param>
        /// <exception cref="AbortedException"></exception>
        private void AddQuality(int intCount, bool blnForced, XmlNode objXmlSelectedQuality, string strForceValue, bool blnDoesNotContributeToBP, int intQualityDiscount = 0)
        {
            for (int i = 0; i < intCount; ++i)
            {
                // Makes sure we aren't over our limits for this particular quality from this overall source
                if (blnForced || objXmlSelectedQuality?.CreateNavigator().RequirementsMet(_objCharacter, strLocalName: _strFriendlyName) == true)
                {
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Quality objAddQuality = new Quality(_objCharacter);
                    try
                    {
                        objAddQuality.Create(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons,
                            strForceValue, _strFriendlyName);

                        if (blnDoesNotContributeToBP)
                        {
                            objAddQuality.BP = 0;
                            objAddQuality.ContributeToLimit = false;
                        }
                        else if (intQualityDiscount != 0)
                        {
                            objAddQuality.BP = Math.Max(objAddQuality.BP + intQualityDiscount, 1);
                        }

                        _objCharacter.Qualities.Add(objAddQuality);
                        foreach (Weapon objWeapon in lstWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                        CreateImprovement(objAddQuality.InternalId, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.SpecificQuality, _strUnique);
                    }
                    catch
                    {
                        objAddQuality.DeleteQuality();
                        throw;
                    }
                }
                else
                {
                    throw new AbortedException();
                }
            }
        }
        #endregion
    }

    [Serializable]
    public sealed class AbortedException : Exception
    {
        public AbortedException()
        {
        }

        public AbortedException(string message) : base(message)
        {
        }

        public AbortedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
