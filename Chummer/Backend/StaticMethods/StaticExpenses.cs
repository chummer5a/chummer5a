using System;

namespace Chummer.Backend.StaticMethods
{
    public static class StaticExpenses
    {
        public static void ExpenseLogStartingKarma(Character character)
        {
            // If the character was built with Karma, record their staring Karma amount (if any).
            if (character.Karma > 0)
            {
                ExpenseLogEntry objKarma = new ExpenseLogEntry(character);
                objKarma.Create(character.Karma, LanguageManager.GetString("Label_SelectBP_StartingKarma"),
                    ExpenseType.Karma, DateTime.Now);
                character.ExpenseEntries.AddWithSort(objKarma);

                // Create an Undo entry so that the starting Karma amount can be modified if needed.
                ExpenseUndo objKarmaUndo = new ExpenseUndo();
                objKarmaUndo.CreateKarma(KarmaExpenseType.ManualAdd, string.Empty);
                objKarma.Undo = objKarmaUndo;
            }
        }
        public static void ExpenseLogStartingNuyen(Character character)
        {
            // Create an Expense Entry for Starting Nuyen.
            ExpenseLogEntry objNuyen = new ExpenseLogEntry(character);
            objNuyen.Create(character.Nuyen, LanguageManager.GetString("Title_LifestyleNuyen"), ExpenseType.Nuyen,
                DateTime.Now);
            character.ExpenseEntries.AddWithSort(objNuyen);

            // Create an Undo entry so that the Starting Nuyen amount can be modified if needed.
            ExpenseUndo objNuyenUndo = new ExpenseUndo();
            objNuyenUndo.CreateNuyen(NuyenExpenseType.ManualAdd, string.Empty);
            objNuyen.Undo = objNuyenUndo;
        }
    }
}