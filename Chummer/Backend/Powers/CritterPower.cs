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
        public CritterPower(Character objCharacter) : base(objCharacter)
        {
            _improvementSource = Improvement.ImprovementSource.CritterPower;
            _freeLevelImprovementType = Improvement.ImprovementType.CritterPowerLevel;
            _freePointImprovementType = Improvement.ImprovementType.AdeptPowerFreePoints;

            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
            CharacterObject.PropertyChanged += OnCharacterChanged;
            if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
            {
                MAGAttributeObject = CharacterObject.MAGAdept;
            }
            else
            {
                MAGAttributeObject = CharacterObject.MAG;
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.IsMysticAdept))
            {
                if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                {
                    MAGAttributeObject = CharacterObject.MAGAdept;
                }
                else
                {
                    MAGAttributeObject = CharacterObject.MAG;
                }
            }
        }

        #region Methods

        #endregion
        #region Properties
        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("critterpowers.xml", strLanguage).SelectSingleNode("/chummer/powers/power[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion
    }
}
