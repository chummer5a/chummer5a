using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;

namespace Chummer.Backend.BuySellIncreaseDecreaseMethods
{
    /// <summary>
    /// This class holds all kinds of different methods that are used to buy/sell or increase/decrease equipment or attributes.
    /// </summary>
    public static class ChangeAttributes
    {
        public static void IncreasePowerpoint(Character characterObj)
        {
            // Make sure the character has enough Karma to improve the CharacterAttribute.
            int intKarmaCost = characterObj.Settings.KarmaMysticAdeptPowerPoint;
            if (intKarmaCost > characterObj.Karma)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (characterObj.MysticAdeptPowerPoints + 1 > characterObj.MAG.TotalValue)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughMagic"), LanguageManager.GetString("MessageTitle_NotEnoughMagic"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend")
                , LanguageManager.GetString("String_PowerPoint")
                , intKarmaCost.ToString(GlobalSettings.CultureInfo))))
                return;

            // Create the Karma expense.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(characterObj);
            objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_PowerPoint"), ExpenseType.Karma, DateTime.Now);
            characterObj.ExpenseEntries.AddWithSort(objExpense);
            characterObj.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddPowerPoint, string.Empty);
            objExpense.Undo = objUndo;

            characterObj.MysticAdeptPowerPoints += 1;
        }
    }
}
