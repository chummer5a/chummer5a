using System;
using System.Collections.Generic;
using System.Xml;
using Chummer.Backend.Attributes;

namespace Chummer.Backend.StaticMethods
{
    /// <summary>
    /// This class holds all kinds of methods that are used to increase/decrease attributes.
    /// </summary>
    public static class StaticAttributes
    {
        /// <summary>
        /// Increases the powerpoints of an mystic adept in career mode and creates the Karma expense. Only accessable as a house rule.
        /// </summary>
        /// <param name="characterObj"></param>
        public static bool IncreasePowerpoint(Character characterObj)
        {
            //This could have been passed as a variable, but this makes it more independent from frmCareer
            int intKarmaCost = characterObj.Settings.KarmaMysticAdeptPowerPoint;


            // Create the Karma expense.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(characterObj);
            objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_PowerPoint"), ExpenseType.Karma, DateTime.Now);
            characterObj.ExpenseEntries.AddWithSort(objExpense);
            characterObj.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddPowerPoint, string.Empty);
            objExpense.Undo = objUndo;

            characterObj.MysticAdeptPowerPoints += 1;

            return true;
        }


        /// <summary>
        /// Creates a list of Attributes to add for ShapeShifters to save the Character as created.
        /// </summary>
        /// <returns></returns>
        public static List<CharacterAttrib> ShapeshifterAttributesToAdd(Character character)
        {
            List<CharacterAttrib> lstAttributesToAdd = null;
            if (character.MetatypeCategory == "Shapeshifter")
            {
                lstAttributesToAdd = new List<CharacterAttrib>(AttributeSection.AttributeStrings.Count);
                XmlDocument xmlDoc = character.LoadData("metatypes.xml");
                string strMetavariantXPath = "/chummer/metatypes/metatype[id = "
                                             + character.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo)
                                                 .CleanXPath()
                                             + "]/metavariants/metavariant[id = "
                                             + character.MetavariantGuid
                                                 .ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath()
                                             + "]";
                foreach (CharacterAttrib objOldAttribute in character.AttributeSection.AttributeList)
                {
                    CharacterAttrib objNewAttribute = new CharacterAttrib(character, objOldAttribute.Abbrev,
                        CharacterAttrib.AttributeCategory.Shapeshifter);
                    AttributeSection.CopyAttribute(objOldAttribute, objNewAttribute, strMetavariantXPath, xmlDoc);
                    lstAttributesToAdd.Add(objNewAttribute);
                }

                foreach (CharacterAttrib objAttributeToAdd in lstAttributesToAdd)
                {
                    character.AttributeSection.AttributeList.Add(objAttributeToAdd);
                }
            }

            return lstAttributesToAdd;
        }
    }
}
