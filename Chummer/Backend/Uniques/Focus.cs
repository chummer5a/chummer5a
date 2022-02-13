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
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// A Focus.
    /// </summary>
    [DebuggerDisplay("{GearObject?.DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class Focus : IHasInternalId
    {
        private Guid _guiID;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, and Load Methods

        public Focus(Character objCharacter)
        {
            // Create the GUID for the new Focus.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("focus");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("gearid", GearObject?.InternalId);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            string strGearId = string.Empty;
            if (objNode.TryGetStringFieldQuickly("gearid", ref strGearId))
            {
                GearObject = _objCharacter.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGearId) ??
                             (_objCharacter.Armor.FindArmorGear(strGearId) ?? (_objCharacter.Weapons.FindWeaponGear(strGearId) ??
                                                                               (_objCharacter.Cyberware.FindCyberwareGear(strGearId) ??
                                                                                _objCharacter.Vehicles.FindVehicleGear(strGearId))));
            }
        }

        #endregion Constructor, Create, Save, and Load Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Focus in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Foci's name.
        /// </summary>
        public Gear GearObject { get; set; }

        /// <summary>
        /// Rating of the Foci.
        /// </summary>
        public int Rating => GearObject?.Rating ?? 0;

        public int BindingKarmaCost()
        {
            Gear objFocusGear = GearObject;
            // Each Focus costs an amount of Karma equal to their Force x speicific Karma cost.
            string strFocusName = objFocusGear.Name;
            string strFocusExtra = objFocusGear.Extra;
            decimal decExtraKarmaCost = 0;
            //TODO: Oh god I hate putting in this kind of behaviour but we don't have anything else handy that supports altering focus cost.
            if (strFocusName.EndsWith(", Individualized, Complete", StringComparison.Ordinal))
            {
                decExtraKarmaCost = -2;
                strFocusName = strFocusName.Replace(", Individualized, Complete", string.Empty);
            }
            else if (strFocusName.EndsWith(", Individualized, Partial", StringComparison.Ordinal))
            {
                decExtraKarmaCost = -1;
                strFocusName = strFocusName.Replace(", Individualized, Partial", string.Empty);
            }
            int intPosition = strFocusName.IndexOf('(');
            if (intPosition > -1)
                strFocusName = strFocusName.Substring(0, intPosition - 1);
            intPosition = strFocusName.IndexOf(',');
            if (intPosition > -1)
                strFocusName = strFocusName.Substring(0, intPosition);
            decimal decKarmaMultiplier = 1;
            CharacterSettings characterObjectSettings = GearObject.CharacterObject.Settings;
            switch (strFocusName)
            {
                case "Qi Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaQiFocus;
                    break;

                case "Sustaining Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaSustainingFocus;
                    break;

                case "Counterspelling Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaCounterspellingFocus;
                    break;

                case "Banishing Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaBanishingFocus;
                    break;

                case "Binding Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaBindingFocus;
                    break;

                case "Weapon Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaWeaponFocus;
                    break;

                case "Spellcasting Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaSpellcastingFocus;
                    break;

                case "Ritual Spellcasting Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaRitualSpellcastingFocus;
                    break;

                case "Spell Shaping Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaSpellShapingFocus;
                    break;

                case "Summoning Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaSummoningFocus;
                    break;

                case "Alchemical Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaAlchemicalFocus;
                    break;

                case "Centering Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaCenteringFocus;
                    break;

                case "Masking Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaMaskingFocus;
                    break;

                case "Disenchanting Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaDisenchantingFocus;
                    break;

                case "Power Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaPowerFocus;
                    break;

                case "Flexible Signature Focus":
                    decKarmaMultiplier = characterObjectSettings.KarmaFlexibleSignatureFocus;
                    break;
            }

            if (string.IsNullOrWhiteSpace(strFocusExtra))
            {
                foreach (Improvement objLoopImprovement in GearObject.CharacterObject.Improvements.Where(
                             x => x.ImprovedName == strFocusName && string.IsNullOrEmpty(x.Target) && x.Enabled))
                {
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.FocusBindingKarmaCost:
                            decExtraKarmaCost += objLoopImprovement.Value;
                            break;

                        case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                            decKarmaMultiplier += objLoopImprovement.Value;
                            break;
                    }
                }
            }
            else
            {
                foreach (Improvement objLoopImprovement in GearObject.CharacterObject.Improvements.Where(
                             x => x.ImprovedName == strFocusName
                                  && (string.IsNullOrEmpty(x.Target) || x.Target.Contains(strFocusExtra)) && x.Enabled))
                {
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.FocusBindingKarmaCost:
                            decExtraKarmaCost += objLoopImprovement.Value;
                            break;

                        case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                            decKarmaMultiplier += objLoopImprovement.Value;
                            break;
                    }
                }
            }

            return (Rating * decKarmaMultiplier + decExtraKarmaCost).StandardRound();
        }

        #endregion Properties
    }
}
