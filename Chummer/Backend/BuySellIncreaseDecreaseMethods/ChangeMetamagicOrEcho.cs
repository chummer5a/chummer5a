using System.Xml;

namespace Chummer.Backend.BuySellIncreaseDecreaseMethods
{
    /// <summary>
    /// Handles all changes to Metamagic, Echoes, Initiation and Submersion Grade
    /// </summary>
    public static class ChangeMetamagicOrEcho
    {
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

        public static void CreateAndAddMetaMagic(Character objCharacter, Metamagic objNewMetamagic,
            Improvement.ImprovementSource objSource, XmlNode selectedMetaMagicNode, int initiationGrade)
        {
            objNewMetamagic.Create(selectedMetaMagicNode, objSource);
            objNewMetamagic.Grade = initiationGrade;
            if (objNewMetamagic.InternalId.IsEmptyGuid())
                return;

            objCharacter.Metamagics.Add(objNewMetamagic);
        }
    }
}
