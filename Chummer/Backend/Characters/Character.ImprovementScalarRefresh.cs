using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class Character
    {
        /// <summary>
        /// Recreates improvements whose bonus XML references a numeric character scalar whose backing property just changed.
        /// Definitions live in <see cref="CharacterXPathSubstitution"/> (<c>s_numericScalars</c>).
        /// Skips entirely unless the changed-property set overlaps scalar backing properties.
        /// </summary>
        private void RefreshImprovementsForNumericScalarDependencies(HashSet<string> changedCharacterProperties,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!CharacterXPathSubstitution.ChangedPropertiesOverlapNumericScalarDependencies(changedCharacterProperties))
                return;

            foreach (Power objPower in Powers)
            {
                XmlNode xmlBonus = objPower.Bonus;
                if (!CharacterXPathSubstitution.BonusXmlNeedsNumericScalarRefresh(xmlBonus, changedCharacterProperties,
                        token))
                    continue;

                ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Power, objPower.InternalId,
                    token);
                int intTotalRating = objPower.TotalRating;
                if (intTotalRating > 0)
                {
                    ImprovementManager.SetForcedValue(objPower.Extra, this);
                    ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Power,
                        objPower.InternalId, xmlBonus, intTotalRating,
                        objPower.CurrentDisplayNameShort, token: token);
                }
            }

            foreach (Quality objQuality in Qualities)
            {
                if (!CharacterXPathSubstitution.AnyBonusXmlNeedsNumericScalarRefresh(changedCharacterProperties, token,
                        objQuality.Bonus, objQuality.FirstLevelBonus, objQuality.NaturalWeaponsNode))
                    continue;
                objQuality.RegenerateBonusImprovementsForStaleXPathScalars(token);
            }

            foreach (Spell objSpell in Spells)
            {
                XmlNode xmlSpellNode = objSpell.GetNode(token);
                XmlNode xmlSpellBonus = xmlSpellNode?["bonus"];
                if (!CharacterXPathSubstitution.BonusXmlNeedsNumericScalarRefresh(xmlSpellBonus,
                        changedCharacterProperties, token))
                    continue;
                objSpell.RegenerateBonusImprovementsForStaleXPathScalars(token);
            }

            foreach (ComplexForm objComplexForm in ComplexForms)
            {
                XmlNode xmlFormNode = objComplexForm.GetNode(token);
                XmlNode xmlBonus = xmlFormNode?["bonus"];
                if (!CharacterXPathSubstitution.BonusXmlNeedsNumericScalarRefresh(xmlBonus, changedCharacterProperties,
                        token))
                    continue;
                objComplexForm.RegenerateBonusImprovementsForStaleXPathScalars(token);
            }

            foreach (Cyberware objCyberware in Cyberware.AsEnumerableWithSideEffects())
                RefreshCyberwareTreeForStaleXPathScalars(objCyberware, changedCharacterProperties, token);

            foreach (Gear objGear in Gear.AsEnumerableWithSideEffects())
                RefreshGearTreeForStaleXPathScalars(objGear, changedCharacterProperties, token);

            foreach (Armor objArmor in Armor.AsEnumerableWithSideEffects())
                objArmor.RefreshBonusImprovementsForStaleXPathScalars(changedCharacterProperties, token);
        }

        private async Task RefreshImprovementsForNumericScalarDependenciesAsync(HashSet<string> changedCharacterProperties,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!CharacterXPathSubstitution.ChangedPropertiesOverlapNumericScalarDependencies(changedCharacterProperties))
                return;

            await Powers.ForEachAsync(async objPower =>
            {
                token.ThrowIfCancellationRequested();
                XmlNode xmlBonus = await objPower.GetBonusAsync(token).ConfigureAwait(false);
                if (!CharacterXPathSubstitution.BonusXmlNeedsNumericScalarRefresh(xmlBonus, changedCharacterProperties,
                        token))
                    return;

                ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Power, objPower.InternalId,
                    token);
                int intTotalRating = await objPower.GetTotalRatingAsync(token).ConfigureAwait(false);
                if (intTotalRating > 0)
                {
                    ImprovementManager.SetForcedValue(await objPower.GetExtraAsync(token).ConfigureAwait(false), this);
                    await ImprovementManager.CreateImprovementsAsync(this, Improvement.ImprovementSource.Power,
                        objPower.InternalId, xmlBonus, intTotalRating,
                        await objPower.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);

            await Qualities.ForEachAsync(async objQuality =>
            {
                token.ThrowIfCancellationRequested();
                if (!CharacterXPathSubstitution.AnyBonusXmlNeedsNumericScalarRefresh(changedCharacterProperties, token,
                        objQuality.Bonus, objQuality.FirstLevelBonus, objQuality.NaturalWeaponsNode))
                    return;
                await objQuality.RegenerateBonusImprovementsForStaleXPathScalarsAsync(token).ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await Spells.ForEachAsync(async objSpell =>
            {
                token.ThrowIfCancellationRequested();
                XmlNode xmlSpellNode = await objSpell.GetNodeAsync(GlobalSettings.Language, token).ConfigureAwait(false);
                XmlNode xmlSpellBonus = xmlSpellNode?["bonus"];
                if (!CharacterXPathSubstitution.BonusXmlNeedsNumericScalarRefresh(xmlSpellBonus,
                        changedCharacterProperties, token))
                    return;
                await objSpell.RegenerateBonusImprovementsForStaleXPathScalarsAsync(token).ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await ComplexForms.ForEachAsync(async objComplexForm =>
            {
                token.ThrowIfCancellationRequested();
                XmlNode xmlFormNode =
                    await objComplexForm.GetNodeAsync(GlobalSettings.Language, token).ConfigureAwait(false);
                XmlNode xmlBonus = xmlFormNode?["bonus"];
                if (!CharacterXPathSubstitution.BonusXmlNeedsNumericScalarRefresh(xmlBonus, changedCharacterProperties,
                        token))
                    return;
                await objComplexForm.RegenerateBonusImprovementsForStaleXPathScalarsAsync(token).ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await Cyberware.ForEachAsync(async objCyberware =>
            {
                token.ThrowIfCancellationRequested();
                await RefreshCyberwareTreeForStaleXPathScalarsAsync(objCyberware, changedCharacterProperties, token)
                    .ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await Gear.ForEachAsync(async objGear =>
            {
                token.ThrowIfCancellationRequested();
                await RefreshGearTreeForStaleXPathScalarsAsync(objGear, changedCharacterProperties, token)
                    .ConfigureAwait(false);
            }, token).ConfigureAwait(false);

            await Armor.ForEachAsync(async objArmor =>
            {
                token.ThrowIfCancellationRequested();
                await objArmor.RefreshBonusImprovementsForStaleXPathScalarsAsync(changedCharacterProperties, token)
                    .ConfigureAwait(false);
            }, token).ConfigureAwait(false);
        }

        private static void RefreshCyberwareTreeForStaleXPathScalars(Cyberware objCyberware,
            HashSet<string> changedCharacterProperties, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            objCyberware.RefreshBonusImprovementsForStaleXPathScalars(changedCharacterProperties, token);
            foreach (Cyberware objChild in objCyberware.Children.AsEnumerableWithSideEffects())
                RefreshCyberwareTreeForStaleXPathScalars(objChild, changedCharacterProperties, token);
            foreach (Gear objGear in objCyberware.GearChildren.AsEnumerableWithSideEffects())
                RefreshGearTreeForStaleXPathScalars(objGear, changedCharacterProperties, token);
        }

        private async Task RefreshCyberwareTreeForStaleXPathScalarsAsync(Cyberware objCyberware,
            HashSet<string> changedCharacterProperties, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await objCyberware.RefreshBonusImprovementsForStaleXPathScalarsAsync(changedCharacterProperties, token)
                .ConfigureAwait(false);
            await (await objCyberware.GetChildrenAsync(token).ConfigureAwait(false)).ForEachAsync(async objChild =>
            {
                await RefreshCyberwareTreeForStaleXPathScalarsAsync(objChild, changedCharacterProperties, token)
                    .ConfigureAwait(false);
            }, token).ConfigureAwait(false);
            await objCyberware.GearChildren.ForEachAsync(async objGear =>
            {
                await RefreshGearTreeForStaleXPathScalarsAsync(objGear, changedCharacterProperties, token)
                    .ConfigureAwait(false);
            }, token).ConfigureAwait(false);
        }

        private static void RefreshGearTreeForStaleXPathScalars(Gear objGear,
            HashSet<string> changedCharacterProperties, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            objGear.RefreshBonusImprovementsForStaleXPathScalars(changedCharacterProperties, token);
            foreach (Gear objChild in objGear.Children.AsEnumerableWithSideEffects())
                RefreshGearTreeForStaleXPathScalars(objChild, changedCharacterProperties, token);
        }

        private async Task RefreshGearTreeForStaleXPathScalarsAsync(Gear objGear,
            HashSet<string> changedCharacterProperties, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await objGear.RefreshBonusImprovementsForStaleXPathScalarsAsync(changedCharacterProperties, token)
                .ConfigureAwait(false);
            await objGear.Children.ForEachAsync(async objChild =>
            {
                await RefreshGearTreeForStaleXPathScalarsAsync(objChild, changedCharacterProperties, token)
                    .ConfigureAwait(false);
            }, token).ConfigureAwait(false);
        }
    }
}
