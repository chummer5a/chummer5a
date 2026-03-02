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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// A Focus.
    /// </summary>
    [DebuggerDisplay("{GearObject?.DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class Focus : IHasInternalId, IHasCharacterObject
    {
        private Guid _guiID;
        private readonly Character _objCharacter;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

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
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("focus");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("gearid", GearObject?.InternalId ?? string.Empty);
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
                GearObject = _objCharacter.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGearId)
                             ?? _objCharacter.Armor.FindArmorGear(strGearId)
                             ?? _objCharacter.Weapons.FindWeaponGear(strGearId)
                             ?? _objCharacter.Cyberware.FindCyberwareGear(strGearId)
                             ?? _objCharacter.Vehicles.FindVehicleGear(strGearId);
            }
        }

        /// <summary>
        /// Load the Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task LoadAsync(XmlNode objNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objNode == null)
                return;
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            string strGearId = string.Empty;
            if (objNode.TryGetStringFieldQuickly("gearid", ref strGearId))
            {
                GearObject = await _objCharacter.Gear
                                 .DeepFirstOrDefaultAsync(x => x.Children, x => x.InternalId == strGearId, token: token)
                                 .ConfigureAwait(false)
                             ?? (await _objCharacter.Armor.FindArmorGearAsync(strGearId, token: token)
                                 .ConfigureAwait(false)).Item1
                             ?? (await _objCharacter.Weapons.FindWeaponGearAsync(strGearId, token: token)
                                 .ConfigureAwait(false)).Item1
                             ?? (await _objCharacter.Cyberware.FindCyberwareGearAsync(strGearId, token: token)
                                 .ConfigureAwait(false)).Item1
                             ?? (await _objCharacter.Vehicles.FindVehicleGearAsync(strGearId, token: token)
                                 .ConfigureAwait(false)).Item1;
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

        /// <summary>
        /// Rating of the Foci.
        /// </summary>
        public Task<int> GetRatingAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            return GearObject != null ? GearObject.GetRatingAsync(token) : Task.FromResult(0);
        }

        public int BindingKarmaCost()
        {
            Gear objFocusGear = GearObject;
            // Each Focus costs an amount of Karma equal to their Force x specific Karma cost.
            string strFocusName = objFocusGear?.Name ?? string.Empty;
            string strFocusExtra = objFocusGear?.Extra ?? string.Empty;
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
            CharacterSettings objSettings = _objCharacter.Settings;
            switch (strFocusName)
            {
                case "Qi Focus":
                    decKarmaMultiplier = objSettings.KarmaQiFocus;
                    break;

                case "Sustaining Focus":
                    decKarmaMultiplier = objSettings.KarmaSustainingFocus;
                    break;

                case "Counterspelling Focus":
                    decKarmaMultiplier = objSettings.KarmaCounterspellingFocus;
                    break;

                case "Banishing Focus":
                    decKarmaMultiplier = objSettings.KarmaBanishingFocus;
                    break;

                case "Binding Focus":
                    decKarmaMultiplier = objSettings.KarmaBindingFocus;
                    break;

                case "Weapon Focus":
                    decKarmaMultiplier = objSettings.KarmaWeaponFocus;
                    break;

                case "Spellcasting Focus":
                    decKarmaMultiplier = objSettings.KarmaSpellcastingFocus;
                    break;

                case "Ritual Spellcasting Focus":
                    decKarmaMultiplier = objSettings.KarmaRitualSpellcastingFocus;
                    break;

                case "Spell Shaping Focus":
                    decKarmaMultiplier = objSettings.KarmaSpellShapingFocus;
                    break;

                case "Summoning Focus":
                    decKarmaMultiplier = objSettings.KarmaSummoningFocus;
                    break;

                case "Alchemical Focus":
                    decKarmaMultiplier = objSettings.KarmaAlchemicalFocus;
                    break;

                case "Centering Focus":
                    decKarmaMultiplier = objSettings.KarmaCenteringFocus;
                    break;

                case "Masking Focus":
                    decKarmaMultiplier = objSettings.KarmaMaskingFocus;
                    break;

                case "Disenchanting Focus":
                    decKarmaMultiplier = objSettings.KarmaDisenchantingFocus;
                    break;

                case "Power Focus":
                    decKarmaMultiplier = objSettings.KarmaPowerFocus;
                    break;

                case "Flexible Signature Focus":
                    decKarmaMultiplier = objSettings.KarmaFlexibleSignatureFocus;
                    break;
            }

            if (string.IsNullOrWhiteSpace(strFocusExtra))
            {
                decExtraKarmaCost += _objCharacter.Improvements.Sum(objLoopImprovement =>
                {
                    if (objLoopImprovement.ImprovedName != strFocusName ||
                        !string.IsNullOrEmpty(objLoopImprovement.Target) || !objLoopImprovement.Enabled)
                    {
                        return 0;
                    }

                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.FocusBindingKarmaCost:
                            return objLoopImprovement.Value;

                        case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                            decKarmaMultiplier += objLoopImprovement.Value;
                            break;
                    }

                    return 0;
                });
            }
            else
            {
                decExtraKarmaCost += _objCharacter.Improvements.Sum(objLoopImprovement =>
                {
                    if (objLoopImprovement.ImprovedName != strFocusName ||
                        (!string.IsNullOrEmpty(objLoopImprovement.Target) &&
                         !objLoopImprovement.Target.Contains(strFocusExtra)) || !objLoopImprovement.Enabled)
                    {
                        return 0;
                    }

                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.FocusBindingKarmaCost:
                            return objLoopImprovement.Value;

                        case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                            decKarmaMultiplier += objLoopImprovement.Value;
                            break;
                    }

                    return 0;
                });
            }

            return (Rating * decKarmaMultiplier + decExtraKarmaCost).StandardRound();
        }

        public async Task<int> BindingKarmaCostAsync(CancellationToken token = default)
        {
            Gear objFocusGear = GearObject;
            // Each Focus costs an amount of Karma equal to their Force x specific Karma cost.
            string strFocusName = objFocusGear?.Name ?? string.Empty;
            string strFocusExtra = objFocusGear?.Extra ?? string.Empty;
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
            CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
            switch (strFocusName)
            {
                case "Qi Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaQiFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Sustaining Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaSustainingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Counterspelling Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaCounterspellingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Banishing Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaBanishingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Binding Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaBindingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Weapon Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaWeaponFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Spellcasting Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaSpellcastingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Ritual Spellcasting Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaRitualSpellcastingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Spell Shaping Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaSpellShapingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Summoning Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaSummoningFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Alchemical Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaAlchemicalFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Centering Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaCenteringFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Masking Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaMaskingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Disenchanting Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaDisenchantingFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Power Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaPowerFocusAsync(token).ConfigureAwait(false);
                    break;

                case "Flexible Signature Focus":
                    decKarmaMultiplier = await objSettings.GetKarmaFlexibleSignatureFocusAsync(token).ConfigureAwait(false);
                    break;
            }

            ThreadSafeObservableCollection<Improvement> lstImprovements = await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(strFocusExtra))
            {
                decExtraKarmaCost += await lstImprovements.SumAsync(objLoopImprovement =>
                {
                    if (objLoopImprovement.ImprovedName != strFocusName)
                        return 0;
                    if (!string.IsNullOrEmpty(objLoopImprovement.Target))
                        return 0;
                    if (!objLoopImprovement.Enabled)
                        return 0;
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.FocusBindingKarmaCost:
                            return objLoopImprovement.Value;

                        case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                            decKarmaMultiplier += objLoopImprovement.Value;
                            break;
                    }
                    return 0;
                }, token).ConfigureAwait(false);
            }
            else
            {
                decExtraKarmaCost += await lstImprovements.SumAsync(objLoopImprovement =>
                {
                    if (objLoopImprovement.ImprovedName != strFocusName)
                        return 0;
                    if (!string.IsNullOrEmpty(objLoopImprovement.Target) && !objLoopImprovement.Target.Contains(strFocusExtra))
                        return 0;
                    if (!objLoopImprovement.Enabled)
                        return 0;
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.FocusBindingKarmaCost:
                            return objLoopImprovement.Value;

                        case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                            decKarmaMultiplier += objLoopImprovement.Value;
                            break;
                    }

                    return 0;
                }, token).ConfigureAwait(false);
            }

            return (await GetRatingAsync(token).ConfigureAwait(false) * decKarmaMultiplier + decExtraKarmaCost).StandardRound();
        }

        #endregion Properties
    }
}
