using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public class CritterPower : Power
    {
        #region Constructor, Print, Save and Load Methods
        public CritterPower(Character objCharacter) : base(objCharacter)
        {
            _improvementSource = Improvement.ImprovementSource.CritterPower;
            _freeLevelImprovementType = Improvement.ImprovementType.CritterPowerLevel;
            _freePointImprovementType = Improvement.ImprovementType.AdeptPowerFreePoints;

            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
        }
        #endregion
        #region Properties
        public string Category { get; set; }
        public bool CountTowardsLimit { get; set; }
        public int Karma { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public string DisplayType(string strLanguage)
        {
            string strReturn = Type;

            switch (strReturn)
            {
                case "M":
                    strReturn = LanguageManager.GetString("String_SpellTypeMana", strLanguage);
                    break;
                case "P":
                    strReturn = LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_None", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("critterpowers.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        #endregion
    }
}
