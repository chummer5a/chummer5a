using System;
using System.Collections.Generic;
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

        public static string BuildAttributes(Character objCharacter, ICollection<CharacterAttrib> attribs, ICollection<CharacterAttrib> extraAttribs = null, bool special = false)
        {
            int bp = CharacterCalculations.CalculateAttributeBP(attribs, extraAttribs);
            string s = bp.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");
            int att = CharacterCalculations.CalculateAttributePriorityPoints(objCharacter, attribs, extraAttribs);
            int total = special ? objCharacter.TotalSpecial : objCharacter.TotalAttributes;
            if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
            {
                if (bp > 0)
                {
                    s = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_OverPriorityPoints"),
                        total - att, total, bp);
                }
                else
                {
                    s = (total - att).ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Of") + total.ToString(GlobalSettings.CultureInfo);
                }
            }
            return s;
        }
    }
}
