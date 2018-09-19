using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public class AdeptPower : Power
    {
        public AdeptPower(Character objCharacter) : base(objCharacter)
        {
            _improvementSource = Improvement.ImprovementSource.Power;
            _freeLevelImprovementType = Improvement.ImprovementType.AdeptPowerFreeLevels;
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
    }
}
