using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;

namespace Chummer.Backend.StaticMethods
{
    /// <summary>
    /// This class holds different calculations about the character, mainly regarding build points
    /// </summary>
    public static class CharacterCalculations
    {
        public static int CalculateAttributeBP(IEnumerable<CharacterAttrib> attribs, IEnumerable<CharacterAttrib> extraAttribs = null)
        {
            // Primary and Special Attributes are calculated separately since you can only spend a maximum of 1/2 your BP allotment on Primary Attributes.
            // Special Attributes are not subject to the 1/2 of max BP rule.
            int intBP = attribs.Sum(att => att.TotalKarmaCost);
            if (extraAttribs != null)
            {
                intBP += extraAttribs.Sum(att => att.TotalKarmaCost);
            }
            return intBP;
        }

        public static int CalculateAttributePriorityPoints(Character objCharacter, IEnumerable<CharacterAttrib> attribs,
            IEnumerable<CharacterAttrib> extraAttribs = null)
        {
            int intAtt = 0;
            if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
            {
                // Get the total of "free points" spent
                intAtt += attribs.Sum(att => att.SpentPriorityPoints);
                if (extraAttribs != null)
                {
                    // Get the total of "free points" spent
                    intAtt += extraAttribs.Sum(att => att.SpentPriorityPoints);
                }
            }
            return intAtt;
        }

        public static int CalculateBPUsedByInitiation(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            // Calculate the BP used by Initiation.
            int intInitiationPoints = 0;
            foreach (InitiationGrade objGrade in objCharacter.InitiationGrades)
            {
                intInitiationPoints += objGrade.KarmaCost;
                // Add the Karma cost of extra Metamagic/Echoes to the Initiation cost.
                int metamagicKarma = Math.Max(objCharacter.Metamagics.Count(x => x.Grade == objGrade.Grade) - 1, 0);
                intInitiationPoints += objCharacterSettings.KarmaMetamagic * metamagicKarma;
            }

            // Add the Karma cost of extra Metamagic/Echoes to the Initiation cost.
            intInitiationPoints += objCharacter.Enhancements.Count * 2;
            /*
            foreach (Enhancement objEnhancement in objCharacter.Enhancements)
            {
                intInitiationPoints += 2;
            }
            */
            foreach (Power objPower in objCharacter.Powers)
            {
                intInitiationPoints += objPower.Enhancements.Count * 2;
                /*
                foreach (Enhancement objEnhancement in objPower.Enhancements)
                    intInitiationPoints += 2;
                    */
            }

            // Joining a Network does not cost Karma for Technomancers, so this only applies to Magicians/Adepts.
            // Check to see if the character is a member of a Group.
            if (objCharacter.GroupMember && objCharacter.MAGEnabled)
                intInitiationPoints += objCharacterSettings.KarmaJoinGroup;
            return intInitiationPoints;
        }

        public static void CalculateBPUsedByPrograms(Character objCharacter, ref int intKarmaPointsRemain, ref int intFreestyleBP, out int intAINormalProgramPointsUsed, out int intAIAdvancedProgramPointsUsed)
        {
            //TODO: Split this thing apart!
            intAINormalProgramPointsUsed = 0;
            intAIAdvancedProgramPointsUsed = 0;
            foreach (AIProgram objProgram in objCharacter.AIPrograms)
            {
                if (objProgram.CanDelete)
                {
                    if (objProgram.IsAdvancedProgram)
                        intAIAdvancedProgramPointsUsed += 1;
                    else
                        intAINormalProgramPointsUsed += 1;
                }
            }
            int intKarmaCost = 0;
            int intNumAdvancedProgramPointsAsNormalPrograms = 0;
            if (intAINormalProgramPointsUsed > objCharacter.AINormalProgramLimit)
            {
                if (intAIAdvancedProgramPointsUsed < objCharacter.AIAdvancedProgramLimit)
                {
                    intNumAdvancedProgramPointsAsNormalPrograms = Math.Min(intAINormalProgramPointsUsed - objCharacter.AINormalProgramLimit, objCharacter.AIAdvancedProgramLimit - intAIAdvancedProgramPointsUsed);
                    intAINormalProgramPointsUsed -= intNumAdvancedProgramPointsAsNormalPrograms;
                }
                if (intAINormalProgramPointsUsed > objCharacter.AINormalProgramLimit)
                    intKarmaCost += (intAINormalProgramPointsUsed - objCharacter.AINormalProgramLimit) * objCharacter.AIProgramKarmaCost;
            }
            if (intAIAdvancedProgramPointsUsed > objCharacter.AIAdvancedProgramLimit)
            {
                intKarmaCost += (intAIAdvancedProgramPointsUsed - objCharacter.AIAdvancedProgramLimit) * objCharacter.AIAdvancedProgramKarmaCost;
            }
            intKarmaPointsRemain -= intKarmaCost;
            intFreestyleBP += intAIAdvancedProgramPointsUsed + intAINormalProgramPointsUsed + intNumAdvancedProgramPointsAsNormalPrograms;
        }

        public static int CalculateIntFormsPointsUsed(Character objCharacter)
        {
            return objCharacter.ComplexForms.Count(objComplexForm => objComplexForm.Grade == 0);
        }

        public static int SpiritPointsUsed(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            int intSpiritPointsUsed = 0;
            foreach (Spirit objSpirit in objCharacter.Spirits)
            {
                if (objSpirit.EntityType == SpiritType.Spirit)
                {
                    intSpiritPointsUsed += objSpirit.ServicesOwed * objCharacterSettings.KarmaSpirit;
                    // Each Sprite costs KarmaSpirit x Services Owed.
                }
            }

            return intSpiritPointsUsed;
        }

        public static int SpritePointsUsed(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            int intSpritePointsUsed = 0;
            foreach (var sprite in objCharacter.Spirits)
            {
                if (sprite.EntityType == SpiritType.Sprite)
                {
                    intSpritePointsUsed += sprite.ServicesOwed * objCharacterSettings.KarmaSpirit;
                }
            }

            return intSpritePointsUsed;
        }

        private static int PointsUsedByFettered(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            return objCharacter.Spirits.Where(spirit => spirit.Fettered && (spirit.EntityType == SpiritType.Spirit))
                .Sum(spirit => spirit.Force * objCharacterSettings.KarmaSpiritFettering);
        }

        private static int StackedFocusBindingCost(Character objCharacter)
        {
            return objCharacter.StackedFoci.Where(stackedFocus => stackedFocus.Bonded).Sum(stackedFocus => stackedFocus.BindingCost);
        }

        private static int FocusBindingCost(Character objCharacter)
        {
            return objCharacter.Foci.Sum(focus => focus.BindingKarmaCost());
        }


        public static int OverallSpells(Character objCharacter, CharacterSettings objCharacterSettings,
            int intPPBought)
        {
            int limit = objCharacter.FreeSpells;


            // Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
            int spells = objCharacter.Spells.Count(spell =>
                spell.Grade == 0 && !spell.Alchemical && spell.Category != "Rituals" && !spell.FreeBonus);

            spells += RegainMasteryQualitySpells();

            //Spell reduction by specializations
            spells -= SpecializationReduction();

            //Handle Spells As Adept Powers
            if (intPPBought > 0 && objCharacterSettings.PrioritySpellsAsAdeptPowers)
            {
                spells += Math.Min(limit, intPPBought);
            }

            spells -= TouchOnlyReduction();

            return spells;

            #region Local Functions

            int RegainMasteryQualitySpells()
            {
                // Regain spell points for mastery Qualities
                int spellCost = objCharacter.SpellKarmaCost("Spells");
                // It is only karma-efficient to use spell points for Mastery qualities if real spell karma cost is not greater than unmodified spell karma cost
                if (spellCost <= objCharacterSettings.KarmaSpell && objCharacter.FreeSpells > 0)
                {
                    // Assume that every [spell cost] karma spent on a Mastery quality is paid for with a priority-given spell point instead, as that is the most karma-efficient.
                    int intQualityKarmaToSpellPoints = objCharacterSettings.KarmaSpell;
                    if (objCharacterSettings.KarmaSpell != 0)
                        intQualityKarmaToSpellPoints = Math.Min(limit,
                            objCharacter.Qualities
                                .Where(objQuality => objQuality.CanBuyWithSpellPoints && objQuality.ContributeToBP)
                                .Sum(objQuality => objQuality.BP) * objCharacterSettings.KarmaQuality /
                            objCharacterSettings.KarmaSpell);
                    return intQualityKarmaToSpellPoints;
                }

                return 0;
            }

            int SpecializationReduction()
            {
                int spellToLoose = 0;
                foreach (Improvement imp in objCharacter.Improvements.Where(i =>
                             i.ImproveType == Improvement.ImprovementType.FreeSpellsSkill && i.Enabled))
                {
                    Skill skill = objCharacter.SkillsSection.GetActiveSkill(imp.ImprovedName);
                    if (skill == null) continue;

                    //TODO: I don't like this being hardcoded, even though I know full well CGL are never going to reuse this.
                    foreach (SkillSpecialization spec in skill.Specializations)
                    {
                        if (objCharacter.Spells.Any(spell => spell.Category == spec.Name && !spell.FreeBonus))
                        {
                            spellToLoose++;
                        }
                    }
                }

                return spellToLoose;
            }

            int TouchOnlyReduction()
            {
                //Deduct touch only spells from spells
                var intLimitModTouchOnly = CalcLimitModTouchOnly(objCharacter);
                int intTouchOnlySpells = objCharacter.Spells.Count(spell =>
                    spell.Grade == 0 && !spell.Alchemical && spell.Category != "Rituals" &&
                    (spell.Range == "T (A)" || spell.Range == "T") && !spell.FreeBonus);
                int touchOnlyReduction = intTouchOnlySpells - Math.Max(0, intTouchOnlySpells - intLimitModTouchOnly);
                return touchOnlyReduction;
            }

            #endregion
        }

        /// <summary>
        /// Calculates the reductions that need to be applied to the points and count
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="objCharacterSettings"></param>
        /// <param name="intLimitMod"></param>
        /// <param name="intPPBought"></param>
        /// <returns>(spellReduction, ritualReduction, preperationsReduction)</returns>
        public static (int, int, int) SpellVariantReductions(Character objCharacter, CharacterSettings objCharacterSettings, int intLimitMod, int intPPBought)
        {

            //TODO: This reads shitty, but that's where I got stuck deconstructing this whole Method into 3 instead of returning a tuple
            //The problem is this overarching loop variable that needs to be carried over.
            int limit = objCharacter.FreeSpells;

            var availableFreeSpells = limit + intLimitMod;

            var ritualReduction = 0;
            var preperationsReduction = 0;
            var spellReduction = 0;

            var spells = OverallSpells(objCharacter, objCharacterSettings, intPPBought);
            var rituals = objCharacter.Spells.Count(spell =>
                spell.Grade == 0 && !spell.Alchemical && spell.Category == "Rituals" && !spell.FreeBonus);
            var preperations = objCharacter.Spells.Count(spell => spell.Grade == 0 && spell.Alchemical && !spell.FreeBonus);


            for (int i = availableFreeSpells; i > 0; i--)
            {
                if (spells - spellReduction > 0)
                {
                    spellReduction++;
                }
                else if (rituals - ritualReduction > 0)
                {
                    ritualReduction++;
                }
                else if (preperations - preperationsReduction > 0)
                {
                    preperationsReduction++;
                }
                else
                {
                    break;
                }
            }

            return (spellReduction, ritualReduction, preperationsReduction);

        }

        private static int RegainMasteryQualityKarma(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            int spellCost = objCharacter.SpellKarmaCost("Spells");
            // Regain Karma for efficient usage in mastery qualities
            // It is only karma-efficient to use spell points for Mastery qualities if real spell karma cost is not greater than unmodified spell karma cost
            if (spellCost > objCharacterSettings.KarmaSpell || objCharacter.FreeSpells <= 0) return 0;


            int limit = objCharacter.FreeSpells;
            // Assume that every [spell cost] karma spent on a Mastery quality is paid for with a priority-given spell point instead, as that is the most karma-efficient.
            int intQualityKarmaToSpellPoints = objCharacterSettings.KarmaSpell;
            if (objCharacterSettings.KarmaSpell != 0)
                intQualityKarmaToSpellPoints = Math.Min(limit,
                    objCharacter.Qualities
                        .Where(objQuality => objQuality.CanBuyWithSpellPoints && objQuality.ContributeToBP)
                        .Sum(objQuality => objQuality.BP) * objCharacterSettings.KarmaQuality /
                    objCharacterSettings.KarmaSpell);

            // Add the karma paid for by spell points back into the available karma pool.
            var karmaRegained = intQualityKarmaToSpellPoints * objCharacterSettings.KarmaSpell;

            return karmaRegained;
        }

        private static int KarmaForMysAdeptPowerPoints(Character objCharacter, CharacterSettings objCharacterSettings, int powerPointsBought)
        {
            int limit = objCharacter.FreeSpells;
            if (objCharacterSettings.PrioritySpellsAsAdeptPowers)
            {
                powerPointsBought = Math.Max(0, powerPointsBought - limit);
            }

            int intAttributePointsUsed = powerPointsBought * objCharacter.Settings.KarmaMysticAdeptPowerPoint;
            return intAttributePointsUsed;
        }

        private static int CalcLimitModTouchOnly(Character objCharacter)
        {
            //Touch Only
            int intLimitModTouchOnly = 0;
            foreach (Improvement imp in objCharacter.Improvements.Where(i =>
                         i.ImproveType == Improvement.ImprovementType.FreeSpellsATT && i.Enabled))
            {
                int intAttValue = objCharacter.GetAttribute(imp.ImprovedName).TotalValue;
                if (imp.UniqueName.Contains("half"))
                    intAttValue = (intAttValue + 1) / 2;

                if (imp.UniqueName.Contains("touchonly"))
                    intLimitModTouchOnly += intAttValue;
            }

            foreach (Improvement imp in objCharacter.Improvements.Where(i =>
                         i.ImproveType == Improvement.ImprovementType.FreeSpellsSkill && i.Enabled))
            {
                Skill skill = objCharacter.SkillsSection.GetActiveSkill(imp.ImprovedName);
                if (skill == null) continue;
                int intSkillValue = skill.TotalBaseRating;

                if (imp.UniqueName.Contains("half"))
                    intSkillValue = (intSkillValue + 1) / 2;

                if (imp.UniqueName.Contains("touchonly"))
                    intLimitModTouchOnly += intSkillValue;
            }

            return intLimitModTouchOnly;
        }

        public static int CalcLimitMod(Character objCharacter)
        {
            //Normal Spells
            var intLimitMod = (ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.SpellLimit)
                               + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.FreeSpells))
                .StandardRound();

            foreach (Improvement imp in objCharacter.Improvements.Where(i =>
                         i.ImproveType == Improvement.ImprovementType.FreeSpellsATT && i.Enabled))
            {
                int intAttValue = objCharacter.GetAttribute(imp.ImprovedName).TotalValue;
                if (imp.UniqueName.Contains("half"))
                    intAttValue = (intAttValue + 1) / 2;

                if (!imp.UniqueName.Contains("touchonly"))
                    intLimitMod += intAttValue;
            }

            foreach (Improvement imp in objCharacter.Improvements.Where(i =>
                         i.ImproveType == Improvement.ImprovementType.FreeSpellsSkill && i.Enabled))
            {
                Skill skill = objCharacter.SkillsSection.GetActiveSkill(imp.ImprovedName);
                if (skill == null) continue;
                int intSkillValue = skill.TotalBaseRating;

                if (imp.UniqueName.Contains("half"))
                    intSkillValue = (intSkillValue + 1) / 2;

                if (!imp.UniqueName.Contains("touchonly"))
                    intLimitMod += intSkillValue;
            }

            return intLimitMod;
        }

        static int CalculateMartialArtsPoints(this Character objCharacter, CharacterSettings objCharacterSettings)
        {
            int intMartialArtsPoints = 0;
            foreach (MartialArt objectMartialArt in objCharacter.MartialArts)
            {
                if (objectMartialArt.IsQuality)
                    continue;

                intMartialArtsPoints += objectMartialArt.Cost;
                intMartialArtsPoints +=
                    Math.Max(objectMartialArt.Techniques.Count - 1, 0) * objCharacterSettings.KarmaTechnique;
            }

            return intMartialArtsPoints;
        }

        /// <summary>
        /// Calculates the BP that are used by LifeModule Qualities
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="objCharacterSettings"></param>
        /// <returns></returns>
        private static int LifeModuleQualities(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            int intLifeModuleQualities = 0;
            foreach (Quality objLoopQuality in objCharacter.Qualities.Where(q => q.ContributeToBP))
            {
                if (objLoopQuality.Type == QualityType.LifeModule)
                {
                    intLifeModuleQualities += objLoopQuality.BP * objCharacterSettings.KarmaQuality;
                }
            }
            return intLifeModuleQualities;
        }

        public static int PointsInContacts(Character objCharacter)
        {
            int intPointsInContacts = 0;
            int intContactPoints = objCharacter.ContactPoints;
            int intContactPointsLeft = intContactPoints;
            foreach (Contact objContact in objCharacter.Contacts)
            {
                // Don't care about free contacts or groups
                if (objContact.EntityType != ContactType.Contact || objContact.Free || objContact.IsGroup)
                    continue;

                int over = intContactPointsLeft - objContact.ContactPoints;

                //Prefers to eat 0, we went over
                if (over < 0)
                {
                    //over is negative so to add we substract
                    //instead of +abs(over)
                    intPointsInContacts -= over;
                    intContactPointsLeft = 0; //we went over so we know none are left
                }
                else
                {
                    //otherwise just set;
                    intContactPointsLeft = over;
                }
            }
            objCharacter.ContactPointsUsed = intContactPointsLeft;
            return intPointsInContacts;
        }

        private static int PointsInHighPlacesFriend(Character objCharacter, int intPointsInContacts)
        {
            var intHighPlacesFriends = NumberHighPlacesFriends(objCharacter);

            int pointsInHighPlacesFriend = 0;
            if (intPointsInContacts > 0 || objCharacter.CHA.Value * 4 < intHighPlacesFriends)
            {
                pointsInHighPlacesFriend = Math.Max(0, intHighPlacesFriends - objCharacter.CHA.Value * 4);
            }

            return pointsInHighPlacesFriend;
        }

        public static int NumberHighPlacesFriends(Character objCharacter)
        {
            int intHighPlacesFriends = 0;
            foreach (Contact objContact in objCharacter.Contacts)
            {
                // Don't care about free contacts
                if (objContact.EntityType != ContactType.Contact || objContact.Free)
                    continue;


                if (objContact.Connection >= 8 && objCharacter.FriendsInHighPlaces)
                {
                    intHighPlacesFriends += objContact.Connection + objContact.Loyalty;
                }
            }

            return intHighPlacesFriends;
        }

        private static int PointsUsedByMetaType(Character objCharacter, CharacterSettings objCharacterSettings)
        {
            int pointsUsedByMetaType;

            // Metatype/Metavariant only cost points when working with BP (or when the Metatype Costs Karma option is enabled when working with Karma).
            if (!objCharacter.EffectiveBuildMethodUsesPriorityTables)
            {
                // Subtract the BP used for Metatype.
                pointsUsedByMetaType = objCharacter.MetatypeBP * objCharacterSettings.MetatypeCostsKarmaMultiplier;
            }
            else
            {
                pointsUsedByMetaType = objCharacter.MetatypeBP;
            }

            return pointsUsedByMetaType;
        }

        public static (int, int) CalculateRemainingBP(Character character, CharacterSettings characterSettings, int powerPointsBought)
        {
            int intKarmaPointsRemain = characterSettings.BuildKarma;
            //int intPointsUsed = 0; // used as a running total for each section

            int intFreestyleBP = 0;

            // ------------------------------------------------------------------------------
            var pointsUsedByMetaType = PointsUsedByMetaType(character, characterSettings);
            intKarmaPointsRemain -= pointsUsedByMetaType;

            // ------------------------------------------------------------------------------
            // Calculate the points used by Contacts.

            var intPointsInContacts = PointsInContacts(character);

            var pointsInHighPlacesFriend = PointsInHighPlacesFriend(character, intPointsInContacts);


            intPointsInContacts += pointsInHighPlacesFriend;

            intKarmaPointsRemain -= intPointsInContacts;
            // ------------------------------------------------------------------------------
            // Calculate the BP used by Qualities.

            var intLifeModuleQualities = LifeModuleQualities(character, characterSettings);

            //Sum of all qualities used. Negative ones give Karma, thats why they are substracted.
            int intQualityPointsUsed = intLifeModuleQualities - character.NegativeQualityKarma + character.PositiveQualityKarmaTotal;

            intKarmaPointsRemain -= intQualityPointsUsed;
            intFreestyleBP += intQualityPointsUsed;
            // Changelings must either have a balanced negative and positive number of metagenic qualities, or have 1 more point of positive than negative.
            // If the latter, karma is used to balance them out.
            if (character.MetagenicPositiveQualityKarma + character.MetagenicNegativeQualityKarma == 1)
                intKarmaPointsRemain--;

            // ------------------------------------------------------------------------------
            // Update Primary Attributes and Special Attributes values.
            int intAttributePointsUsed = CalculateAttributeBP(character.AttributeSection.AttributeList);
            //Add Special Attributes
            intAttributePointsUsed += CalculateAttributeBP(character.AttributeSection.SpecialAttributeList);
            intKarmaPointsRemain -= intAttributePointsUsed;

            // ------------------------------------------------------------------------------
            // Include the BP used by Martial Arts.

            var intMartialArtsPoints = CalculateMartialArtsPoints(character, characterSettings);
            intKarmaPointsRemain -= intMartialArtsPoints;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Skill Groups.
            int intSkillGroupsPoints = character.SkillsSection.SkillGroups.TotalCostKarma();
            intKarmaPointsRemain -= intSkillGroupsPoints;
            // ------------------------------------------------------------------------------
            // Calculate the BP used by Active Skills.
            int skillPointsKarma = character.SkillsSection.Skills.TotalCostKarma();
            intKarmaPointsRemain -= skillPointsKarma;

            // ------------------------------------------------------------------------------
            // Calculate the points used by Knowledge Skills.
            int knowledgeKarmaUsed = character.SkillsSection.KnowledgeSkills.TotalCostKarma();

            //TODO: Remaining is named USED?
            intKarmaPointsRemain -= knowledgeKarmaUsed;

            intFreestyleBP += knowledgeKarmaUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Resources/Nuyen.
            int intNuyenBP = character.NuyenBP.StandardRound();

            intKarmaPointsRemain -= intNuyenBP;

            intFreestyleBP += intNuyenBP;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Spells.

            int intSpellPointsUsed = 0;
            int intRitualPointsUsed = 0;
            int intPrepPointsUsed = 0;


            if (character.MagicianEnabled
                || character.AdeptEnabled
                || character.Improvements.Any(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.FreeSpells
                                                                       || objImprovement.ImproveType == Improvement.ImprovementType.FreeSpellsATT
                                                                       || objImprovement.ImproveType == Improvement.ImprovementType.FreeSpellsSkill)
                                                                      && objImprovement.Enabled))
            {
                int limit = character.FreeSpells;

                int intLimitMod = CalcLimitMod(character);


                //Calculates the reduction that needs to be applied to all points and counts
                var reductionTuple = SpellVariantReductions(character, characterSettings, intLimitMod, powerPointsBought);

                //Deconstructs the tuple
                var (spellReduction, ritualReduction, preperationsReduction) = reductionTuple;

                //Calcs the points
                var ritualPoints = limit + intLimitMod - ritualReduction;
                var spellPoints = limit + intLimitMod - spellReduction;
                var prepPoints = limit + intLimitMod - preperationsReduction;

                //calcs how much are left
                var spells = OverallSpells(character, characterSettings, powerPointsBought) -
                             spellReduction;
                var alchemicalPreparations = character.Spells.Count(spell => spell.Grade == 0 && spell.Alchemical && !spell.FreeBonus) - preperationsReduction;
                var rituals = character.Spells.Count(spell =>
                    spell.Grade == 0 && !spell.Alchemical && spell.Category == "Rituals" && !spell.FreeBonus) - ritualReduction;


                int spellCost = character.SpellKarmaCost("Spells");
                intKarmaPointsRemain -= Math.Max(0, spells) * spellCost;

                int ritualCost = character.SpellKarmaCost("Rituals");
                intKarmaPointsRemain -= Math.Max(0, rituals) * ritualCost;

                int prepCost = character.SpellKarmaCost("Preparations");
                intKarmaPointsRemain -= Math.Max(0, alchemicalPreparations) * prepCost;

                if (powerPointsBought > 0)
                {
                    var attributePointsUsed = KarmaForMysAdeptPowerPoints(character, characterSettings, powerPointsBought);
                    intKarmaPointsRemain -= attributePointsUsed;
                }

                //Regained Karma for smart usage of BP in mastery qualities
                int karmaRegained = RegainMasteryQualityKarma(character, characterSettings);
                intKarmaPointsRemain += karmaRegained;

                intSpellPointsUsed += Math.Max(Math.Max(0, spells) * spellCost, 0);
                intRitualPointsUsed += Math.Max(Math.Max(0, rituals) * ritualCost, 0);
                intPrepPointsUsed += Math.Max(Math.Max(0, alchemicalPreparations) * prepCost, 0);

                intFreestyleBP += intSpellPointsUsed + intRitualPointsUsed + intPrepPointsUsed;
            }



            // ------------------------------------------------------------------------------

            var intFociPointsUsed = FociPointsUsed(character);

            intKarmaPointsRemain -= intFociPointsUsed;
            intFreestyleBP += intFociPointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Spirits and Sprites.


            var intSpiritPointsUsed = SpiritPointsUsed(character, characterSettings);

            var intSpritePointsUsed = SpritePointsUsed(character, characterSettings);

            var pointsUsedByFettered = PointsUsedByFettered(character, characterSettings);


            intKarmaPointsRemain -= (intSpiritPointsUsed + intSpritePointsUsed + pointsUsedByFettered);
            intFreestyleBP += intSpiritPointsUsed + pointsUsedByFettered + intSpritePointsUsed;

            // ------------------------------------------------------------------------------

            // Calculate the BP used by Complex Forms.
            var intFormsPointsUsed = CalculateIntFormsPointsUsed(character);

            if (intFormsPointsUsed > character.CFPLimit)
                intKarmaPointsRemain -= (intFormsPointsUsed - character.CFPLimit) * character.ComplexFormKarmaCost;
            intFreestyleBP += intFormsPointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Programs and Advanced Programs.
            //TODO: Untangle this into multiple Methods to not pass stuff as ref or use out

            int intAINormalProgramPointsUsed, intAIAdvancedProgramPointsUsed;
            CalculateBPUsedByPrograms(character, ref intKarmaPointsRemain, ref intFreestyleBP, out intAINormalProgramPointsUsed, out intAIAdvancedProgramPointsUsed);

            // ------------------------------------------------------------------------------
            int intInitiationPoints = CalculateBPUsedByInitiation(character, characterSettings);
            intKarmaPointsRemain -= intInitiationPoints;
            intFreestyleBP += intInitiationPoints;

            // Add the Karma cost of any Critter Powers.
            intKarmaPointsRemain = character.CritterPowers.Aggregate(intKarmaPointsRemain, (current, objPower) => current - objPower.Karma);

            character.Karma = intKarmaPointsRemain;

            return (intKarmaPointsRemain, intFreestyleBP);
        }

        public static int FociPointsUsed(Character character)
        {
            //Calculate the BP used by Foci
            var focusBindingCost = FocusBindingCost(character);
            // Calculate the BP used by Stacked Foci.
            var stackedFocusBindingCost = StackedFocusBindingCost(character);
            int intFociPointsUsed = stackedFocusBindingCost + focusBindingCost;
            return intFociPointsUsed;
        }


        /// <summary>
        /// Calculate the amount of Nuyen the character has remaining.
        /// </summary>
        public static decimal CalculateNuyen(Character character)
        {
            decimal decDeductions = 0;
            decimal decStolenDeductions = 0;
            //If the character has the Stolen Gear quality or something similar, we need to handle the nuyen a little differently.
            if (character.Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.Nuyen && i.ImprovedName == "Stolen"))
            {
                // Cyberware/Bioware cost.
                decDeductions += character.Cyberware.Where(c => !c.Stolen).AsParallel().Sum(x => x.CurrentTotalCost);
                decStolenDeductions += character.Cyberware.Where(c => c.Stolen).AsParallel().Sum(x => x.StolenTotalCost);

                // Initiation Grade cost.
                foreach (InitiationGrade objGrade in character.InitiationGrades)
                {
                    if (objGrade.Schooling)
                        decDeductions += 10000;
                }

                // Armor cost.
                decDeductions += character.Armor.Where(c => !c.Stolen).AsParallel().Sum(x => x.TotalCost);
                decStolenDeductions += character.Armor.Where(c => c.Stolen).AsParallel().Sum(x => x.StolenTotalCost);

                // Weapon cost.
                decDeductions += character.Weapons.Where(c => !c.Stolen).AsParallel().Sum(x => x.TotalCost);
                decStolenDeductions += character.Weapons.Where(c => c.Stolen).AsParallel().Sum(x => x.TotalCost);

                // Gear cost.
                decDeductions += character.Gear.Where(c => !c.Stolen).AsParallel().Sum(x => x.TotalCost);
                decStolenDeductions += character.Gear.Where(c => c.Stolen).AsParallel().Sum(x => x.StolenTotalCost);

                // Lifestyle cost.
                decDeductions += character.Lifestyles.AsParallel().Sum(x => x.TotalCost);

                // Vehicle cost.
                decDeductions += character.Vehicles.Where(c => !c.Stolen).AsParallel().Sum(x => x.TotalCost);
                decStolenDeductions += character.Vehicles.Where(c => c.Stolen).AsParallel().Sum(x => x.StolenTotalCost);

                // Drug cost.
                decDeductions += character.Drugs.Where(c => !c.Stolen).AsParallel().Sum(x => x.TotalCost);
                decStolenDeductions += character.Drugs.Where(c => c.Stolen).AsParallel().Sum(x => x.StolenTotalCost);
            }
            else
            {
                // Cyberware/Bioware cost.
                decDeductions += character.Cyberware.AsParallel().Sum(x => x.CurrentTotalCost);

                // Initiation Grade cost.
                decDeductions += 10000 * character.InitiationGrades.Count(x => x.Schooling);

                // Armor cost.
                decDeductions += character.Armor.AsParallel().Sum(x => x.TotalCost);

                // Weapon cost.
                decDeductions += character.Weapons.AsParallel().Sum(x => x.TotalCost);

                // Gear cost.
                decDeductions += character.Gear.AsParallel().Sum(x => x.TotalCost);

                // Lifestyle cost.
                decDeductions += character.Lifestyles.AsParallel().Sum(x => x.TotalCost);

                // Vehicle cost.
                decDeductions += character.Vehicles.AsParallel().Sum(x => x.TotalCost);

                // Drug cost.
                decDeductions += character.Drugs.AsParallel().Sum(x => x.TotalCost);
            }

            character.StolenNuyen =
                ImprovementManager.ValueOf(character, Improvement.ImprovementType.Nuyen, false, "Stolen") -
                decStolenDeductions;
            return character.Nuyen = character.TotalStartingNuyen - decDeductions;
        }
    }
}
