using System;
using System.Linq;
using System.Xml;

namespace Chummer.Backend.BuySellIncreaseDecreaseMethods
{
    /// <summary>
    /// Handles all changes to Metamagic, Echoes, Initiation and Submersion Grade
    /// </summary>
    public static class ChangeMetamagicOrEcho
    {
        #region Public Methods

        /// <summary>
        /// Checks if the character can increase it's initiation or submersion grade
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="objCharacterSettings"></param>
        /// <returns></returns>
        public static bool CanIncreaseInitiateGrade(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            if (objCharacter == null)
                return false;

            if (objCharacter.RESEnabled)
            {
                return objCharacter.SubmersionGrade + 1 > objCharacter.RES.TotalValue;
            }

            return objCharacter.InitiateGrade + 1 > objCharacter.MAG.TotalValue ||
                   objCharacterSettings.MysAdeptSecondMAGAttribute && objCharacter.IsMysticAdept &&
                   objCharacter.InitiateGrade + 1 > objCharacter.MAGAdept.TotalValue;
        }

        /// <summary>
        /// Creates a InitiationGradeObject and adds it to the InitiationGrades ObservableCollection in Character
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="groupInitiation">Is it a Group Initiation?</param>
        /// <param name="initiationOrdeal">Is it a initiation with ordeal?</param>
        /// <param name="initiationSchooling">Is it a initiation with schooling?</param>
        public static void CreateInitiateGradeObject(Character objCharacter, bool groupInitiation, bool initiationOrdeal,
            bool initiationSchooling)
        {
            int grade = objCharacter.InitiateGrade;
            bool isTechno = false;

            if (objCharacter.RESEnabled)
            {
                grade = objCharacter.SubmersionGrade;
                isTechno = true;
            }


            InitiationGrade objGrade = new InitiationGrade(objCharacter);
            objGrade.Create(grade + 1, isTechno, groupInitiation, initiationOrdeal,
                initiationSchooling);
            objCharacter.InitiationGrades.AddWithSort(objGrade);
        }



        /// <summary>
        /// Determines if another Metamagic needs to be payed with Karma. Probably deprecated / not needed at all!
        /// </summary>
        /// <param name="objGrade"></param>
        /// <param name="characterObject"></param>
        /// <returns></returns>
        public static bool BlnPayWithKarma(InitiationGrade objGrade, Character characterObject)
        {
            // Evaluate each object
            bool blnPayWithKarma = characterObject.Metamagics.Any(objMetamagic => objMetamagic.Grade == objGrade.Grade) ||
                                   characterObject.Spells.Any(objSpell => objSpell.Grade == objGrade.Grade);
            return blnPayWithKarma;
        }

        /// <summary>
        /// Initializes and creates a complete Metamagic, then adds it to the ObservableCollection Character.MetaMagic.
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="frmPickMetamagic"></param>
        /// <param name="objGrade"></param>
        /// <param name="objCharacterSettings"></param>
        /// <returns></returns>
        public static bool InitialiseCompleteMetamagic(Character objCharacter, frmSelectMetamagic frmPickMetamagic, InitiationGrade objGrade, CharacterSettings objCharacterSettings)
        {
            Metamagic objNewMetamagic = new Metamagic(objCharacter);

            var objSource = DetermineImprovementSource(objCharacter);
            var objXmlMetamagic = DetermineMetamagicXmlNode(objCharacter, frmPickMetamagic);

            //I think this can go, but not sure though
            if (BlnPayWithKarma(objGrade, objCharacter))
            {
                Program.MainForm.ShowMessageBox("You found some Code Joschi thought was unnecessary, you may continue. Please inform a Dev about this.");
                PayMetaMagicWithKarma(objNewMetamagic, objCharacter, objCharacterSettings);
            }
            return CreateAndAddMetaMagic(objCharacter, objNewMetamagic, objSource, objXmlMetamagic, objGrade);
        }


        public static bool InitialiseCompleteArt(Character objCharacter, frmSelectArt frmPickArt, InitiationGrade objGrade)
        {
            XmlNode objXmlArt = objCharacter.LoadData("metamagic.xml").SelectSingleNode("/chummer/arts/art[id = " + frmPickArt.SelectedItem.CleanXPath() + "]");

            Art objArt = new Art(objCharacter);

            objArt.Create(objXmlArt, Improvement.ImprovementSource.Metamagic);
            objArt.Grade = objGrade.Grade;
            if (objArt.InternalId.IsEmptyGuid())
                return false;

            objCharacter.Arts.Add(objArt);
            return true;
        }

        /// <summary>
        /// Loads the needed spelldata from xml and adds the created object to the correct List.
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="frmPickArt"></param>
        /// <param name="objGrade"></param>
        /// <returns></returns>
        public static bool InitialiseCompleteEnchantmentOrRitual(Character objCharacter, frmSelectArt frmPickArt, InitiationGrade objGrade)
        {
            XmlNode objXmlArt = objCharacter.LoadData("spells.xml").SelectSingleNode("/chummer/spells/spell[id = " + frmPickArt.SelectedItem.CleanXPath() + "]");

            Spell objNewSpell = new Spell(objCharacter);
            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, Improvement.ImprovementSource.Initiation);
            objNewSpell.Grade = objGrade.Grade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return false;

            objCharacter.Spells.Add(objNewSpell);

            return true;
        }

        /// <summary>
        /// Initializes, creates and pays the karma cost for an Enhancement, then saves it to the Character.Enhancements ObservableCollection
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="frmPickArt"></param>
        /// <param name="objGrade"></param>
        /// <param name="objCharacterSettings"></param>
        /// <returns></returns>
        public static bool InitializeCompleteEnhancement(Character objCharacter, frmSelectArt frmPickArt, InitiationGrade objGrade, CharacterSettings objCharacterSettings)
        {
            XmlNode objXmlArt = objCharacter.LoadData("powers.xml")
                .SelectSingleNode("/chummer/enhancements/enhancement[id = " + frmPickArt.SelectedItem.CleanXPath() + "]");

            if (objXmlArt == null)
                return true;

            Enhancement objEnhancement = new Enhancement(objCharacter);
            objEnhancement.Create(objXmlArt, Improvement.ImprovementSource.Initiation);
            objEnhancement.Grade = objGrade.Grade;
            if (objEnhancement.InternalId.IsEmptyGuid())
                return true;

            // Find the associated Power
            string strPower = objXmlArt["power"]?.InnerText;
            bool blnPowerFound = false;
            foreach (Power objPower in objCharacter.Powers)
            {
                if (objPower.Name != strPower) continue;

                objPower.Enhancements.Add(objEnhancement);
                blnPowerFound = true;
                break;
            }

            if (!blnPowerFound)
            {
                // Add it to the character instead
                objCharacter.Enhancements.Add(objEnhancement);
            }

            EnhancementExpendKarma(objCharacter, objCharacterSettings, objEnhancement);

            return true;
        }



        #endregion
        #region Private Methods

        private static void EnhancementExpendKarma(Character objCharacter, CharacterSettings objCharacterSettings, Enhancement objEnhancement)
        {
            string strType = LanguageManager.GetString("String_Enhancement");
            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(objCharacter);
            objExpense.Create(objCharacterSettings.KarmaEnhancement * -1,
                strType + LanguageManager.GetString("String_Space") + objEnhancement.DisplayNameShort(GlobalSettings.Language),
                ExpenseType.Karma, DateTime.Now);
            objCharacter.ExpenseEntries.AddWithSort(objExpense);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddSpell, objEnhancement.InternalId);
            objExpense.Undo = objUndo;

            // Adjust the character's Karma total.
            objCharacter.Karma -= objCharacterSettings.KarmaEnhancement;
        }


        /// <summary>
        /// Creates and adds a specified Metamagic to the MetaMagic Observable Collection
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="objNewMetamagic"></param>
        /// <param name="objSource"></param>
        /// <param name="selectedMetaMagicNode"></param>
        /// <param name="objInitiationGrade"></param>
        private static bool CreateAndAddMetaMagic(Character objCharacter, Metamagic objNewMetamagic,
            Improvement.ImprovementSource objSource, XmlNode selectedMetaMagicNode, InitiationGrade objInitiationGrade)
        {
            objNewMetamagic.Create(selectedMetaMagicNode, objSource);
            objNewMetamagic.Grade = objInitiationGrade.Grade;
            if (objNewMetamagic.InternalId.IsEmptyGuid())
                return false;

            objCharacter.Metamagics.Add(objNewMetamagic);

            return true;
        }

        /// <summary>
        /// Pays a Metamagic with Karma. Probably deprecated / not needed at all!
        /// </summary>
        /// <param name="objNewMetamagic"></param>
        /// <param name="objCharacter"></param>
        /// <param name="objCharacterSettings"></param>
        /// <returns></returns>
        private static void PayMetaMagicWithKarma(Metamagic objNewMetamagic, Character objCharacter, CharacterSettings objCharacterSettings)
        {
            string strType = LanguageManager.GetString(objNewMetamagic.SourceType == Improvement.ImprovementSource.Echo
                ? "String_Echo"
                : "String_Metamagic");
            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(objCharacter);
            objExpense.Create(objCharacterSettings.KarmaMetamagic * -1,
                strType + LanguageManager.GetString("String_Space") + objNewMetamagic.DisplayNameShort(GlobalSettings.Language),
                ExpenseType.Karma, DateTime.Now);
            objCharacter.ExpenseEntries.AddWithSort(objExpense);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddMetamagic, objNewMetamagic.InternalId);
            objExpense.Undo = objUndo;

            // Adjust the character's Karma total.
            objCharacter.Karma -= objCharacterSettings.KarmaMetamagic;
        }

        /// <summary>
        /// Sets the correct Improvement source between Echo and Metamagic
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <returns></returns>
        private static Improvement.ImprovementSource DetermineImprovementSource(Character objCharacter)
        {
            if (objCharacter.RESEnabled)
            {
                return Improvement.ImprovementSource.Echo;
            }
            return Improvement.ImprovementSource.Metamagic;
        }

        /// <summary>
        /// Returns the XMLNode of an chosen Metamagic
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="frmPickMetamagic"></param>
        /// <returns></returns>
        private static XmlNode DetermineMetamagicXmlNode(Character objCharacter, frmSelectMetamagic frmPickMetamagic)
        {
            if (objCharacter.RESEnabled)
            {
                return objCharacter.LoadData("echoes.xml").SelectSingleNode("/chummer/echoes/echo[id = " + frmPickMetamagic.SelectedMetamagic.CleanXPath() + "]");
            }
            return objCharacter.LoadData("metamagic.xml").SelectSingleNode("/chummer/metamagics/metamagic[id = " + frmPickMetamagic.SelectedMetamagic.CleanXPath() + "]");
        }
        #endregion













    }
}
