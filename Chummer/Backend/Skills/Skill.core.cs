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
using System.Collections.Generic;
using System.Linq;
using Chummer.Backend.Attributes;

namespace Chummer.Backend.Skills
{

    partial class Skill : IHasInternalId
    {
        private int _intBase;
        private int _intKarma;
        private bool _blnBuyWithKarma;

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public int BasePoints
        {
            get => _intBase;
            set
            {
                if (_intBase != value)
                {
                    _intBase = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public int KarmaPoints
        {
            get => _intKarma;
            set
            {
                if (_intKarma != value)
                {
                    _intKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented? (Build method or skill group)
        /// </summary>
        public bool BaseUnlocked => CharacterObject.EffectiveBuildMethodUsesPriorityTables
                                    && (SkillGroupObject == null
                                        || SkillGroupObject.Base <= 0
                                        || (CharacterObject.Options.UsePointsOnBrokenGroups
                                            && (!CharacterObject.Options.StrictSkillGroupsInCreateMode
                                                || CharacterObject.Created)));

        /// <summary>
        /// Is it possible to place points in Karma or is it prevented a stricter interpretation of the rules
        /// </summary>
        public bool KarmaUnlocked
        {
            get
            {
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                {
                    return (SkillGroupObject == null || SkillGroupObject.Rating <= 0);
                }

                return true;
            }
        }

        /// <summary>
        /// The amount of points this skill have from skill points and bonuses
        /// to the skill rating that would be obtained in some points of character creation
        /// </summary>
        public int Base
        {
            get
            {
                if (SkillGroupObject?.Base > 0)
                {
                    if ((CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                        || !CharacterObject.Options.UsePointsOnBrokenGroups)
                        BasePoints = 0;
                    return Math.Min(SkillGroupObject.Base + BasePoints + FreeBase, RatingMaximum);
                }

                return Math.Min(BasePoints + FreeBase, RatingMaximum);
            }
            set
            {
                if (SkillGroupObject?.Base > 0
                    && ((CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                        || !CharacterObject.Options.UsePointsOnBrokenGroups))
                    return;

                //Calculate how far above maximum we are.
                int intOverMax = value + Karma - RatingMaximum + RatingModifiers(Attribute);

                if (intOverMax > 0) //Too much
                {
                    //Get the smaller value, how far above, how much we can reduce
                    int intMax = Math.Min(intOverMax, KarmaPoints);

                    KarmaPoints -= intMax; //reduce both by that amount
                    intOverMax -= intMax;
                }

                value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                BasePoints = Math.Max(0, value - (FreeBase + (SkillGroupObject?.Base ?? 0)));
            }
        }


        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public int Karma
        {
            get
            {
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created && SkillGroupObject?.Karma > 0)
                {
                    _intKarma = 0;
                    Specializations.RemoveAll(x => !x.Free);
                    return SkillGroupObject.Karma;
                }
                return Math.Min(KarmaPoints + FreeKarma + (SkillGroupObject?.Karma ?? 0), RatingMaximum);
            }
            set
            {
                //Calculate how far above maximum we are.
                int intOverMax = value + Base - RatingMaximum + RatingModifiers(Attribute);

                if (intOverMax > 0) //Too much
                {
                    //Get the smaller value, how far above, how much we can reduce
                    int intMax = Math.Min(intOverMax, BasePoints);

                    BasePoints -= intMax; //reduce both by that amount
                    intOverMax -= intMax;
                }

                value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                //Handle free levels, don,t go below 0
                KarmaPoints = Math.Max(0, value - (FreeKarma + (SkillGroupObject?.Karma ?? 0)));
            }
        }

        /// <summary>
        /// Levels in this skill. Read only. You probably want to increase
        /// Karma instead
        /// </summary>
        public int Rating => Math.Max(CyberwareRating, TotalBaseRating);

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public int TotalBaseRating => LearnedRating + RatingModifiers(Attribute);

        /// <summary>
        /// The rating the character have actually paid for, not including skillwires
        /// or other overrides for skill Rating. Read only, you probably want to
        /// increase Karma instead.
        /// </summary>
        public int LearnedRating => Karma + Base;

        /// <summary>
        /// Is the specialization bought with karma. During career mode this is undefined
        /// </summary>
        public bool BuyWithKarma
        {
            get => (_blnBuyWithKarma || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma && !string.IsNullOrEmpty(Specialization);
            set
            {
                bool blnNewValue = (value || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma && !string.IsNullOrEmpty(Specialization);
                if (_blnBuyWithKarma != blnNewValue)
                {
                    _blnBuyWithKarma = blnNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum possible rating
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                int intOtherBonus = RelevantImprovements(x => x.ImproveType == Improvement.ImprovementType.Skill && x.Enabled).Sum(x => x.Maximum);
                if (CharacterObject.Created || CharacterObject.IgnoreRules)
                    return 12 + intOtherBonus;
                return (IsKnowledgeSkill && CharacterObject.EffectiveBuildMethodIsLifeModule ? 9 : 6) + intOtherBonus;
            }
        }

        /// <summary>
        /// The total, general purpose dice pool for this skill, using another
        /// value for the attribute part of the test. This allows calculation of dice pools
        /// while using cyberlimbs or while rigging
        /// </summary>
        /// <param name="intAttributeOverrideValue">The value to be used for the attribute if it's not the default value. int.MinValue is equivalent to not overriding.</param>
        /// <param name="strAttribute">The English abbreviation of the used attribute.</param>
        /// <param name="blnIncludeConditionals">Whether to include improvements that don't apply under all circumstances.</param>
        /// <returns></returns>
        public int PoolOtherAttribute(string strAttribute, bool blnIncludeConditionals = false, int intAttributeOverrideValue = int.MinValue)
        {
            if (!Enabled)
                return 0;
            int intRating = Rating;
            int intValue = intAttributeOverrideValue > int.MinValue
                ? intAttributeOverrideValue
                : CharacterObject.AttributeSection.GetAttributeByName(strAttribute).TotalValue;
            if (intRating > 0)
            {
                return Math.Max(0, intRating + intValue + PoolModifiers(strAttribute, blnIncludeConditionals) + CharacterObject.WoundModifier);
            }
            if (Default)
            {
                return Math.Max(0, intValue + PoolModifiers(strAttribute, blnIncludeConditionals) + DefaultModifier + CharacterObject.WoundModifier);
            }
            return 0;
        }

        private static readonly Guid s_GuiReflexRecorderId = new Guid("17a6ba49-c21c-461b-9830-3beae8a237fc");

        public int DefaultModifier
        {
            get
            {
                if (SkillGroupObject != null && CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization && x.Enabled))
                {
                    HashSet<string> setSkillNames = SkillGroupObject.SkillList.Select(x => x.Name).ToHashSet();
                    if (CharacterObject.Cyberware.Where(x => x.SourceID == s_GuiReflexRecorderId)
                        .Any(x => setSkillNames.Contains(x.Extra)))
                    {
                        return 0;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int PoolModifiers(string strUseAttribute, bool blnIncludeConditionals = false) => Bonus(false, strUseAttribute, blnIncludeConditionals);

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int RatingModifiers(string strUseAttribute, bool blnIncludeConditionals = false) => Bonus(true, strUseAttribute, blnIncludeConditionals);

        protected int Bonus(bool blnAddToRating, string strUseAttribute, bool blnIncludeConditionals = false)
        {
            //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
            return (RelevantImprovements(x => x.AddToRating == blnAddToRating, strUseAttribute, blnIncludeConditionals).Sum(x => x.Value)).StandardRound();
        }

        private IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null, string strUseAttribute = "", bool blnIncludeConditionals = false, bool blnExistAfterFirst = false)
        {
            string strNameToUse = Name;
            if (!string.IsNullOrWhiteSpace(strNameToUse))
            {
                if (this is ExoticSkill objThisAsExoticSkill)
                    strNameToUse += " (" + objThisAsExoticSkill.Specific + ')';
                if (string.IsNullOrEmpty(strUseAttribute))
                    strUseAttribute = Attribute;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (!objImprovement.Enabled || funcWherePredicate?.Invoke(objImprovement) == false) continue;
                    if (!blnIncludeConditionals && !string.IsNullOrWhiteSpace(objImprovement.Condition)) continue;
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.Skill:
                        case Improvement.ImprovementType.SwapSkillAttribute:
                        case Improvement.ImprovementType.SwapSkillSpecAttribute:
                        case Improvement.ImprovementType.SkillDisable:
                            if ((objImprovement.ImprovedName == Name || objImprovement.ImprovedName == strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                        case Improvement.ImprovementType.SkillGroup:
                        case Improvement.ImprovementType.SkillGroupDisable:
                            if (objImprovement.ImprovedName == SkillGroup && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(strNameToUse) && !objImprovement.Exclude.Contains(SkillCategory))
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                        case Improvement.ImprovementType.SkillCategory:
                            if (objImprovement.ImprovedName == SkillCategory && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                        case Improvement.ImprovementType.SkillAttribute:
                            if (objImprovement.ImprovedName == strUseAttribute && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                        case Improvement.ImprovementType.SkillLinkedAttribute:
                            if (objImprovement.ImprovedName == Attribute && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                        case Improvement.ImprovementType.BlockSkillDefault:
                            if (objImprovement.ImprovedName == SkillGroup)
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                        case Improvement.ImprovementType.EnhancedArticulation:
                            if (SkillCategory == "Physical Active" && AttributeSection.PhysicalAttributes.Contains(Attribute))
                            {
                                yield return objImprovement;
                                if (blnExistAfterFirst)
                                    yield break;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentSpCost
        {
            get
            {
                int cost = BasePoints + ((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                ExoticSkill objThisAsExoticSkill = this as ExoticSkill;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= BasePoints &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName) ||
                            (objThisAsExoticSkill != null && objLoopImprovement.ImprovedName == Name + " (" + objThisAsExoticSkill.Specific + ')'))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillPointCost)
                                decExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCost)
                                decExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    cost = (cost * decMultiplier + decExtra).StandardRound();
                else
                    cost += decExtra.StandardRound();

                return Math.Max(cost, 0);
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentKarmaCost
        {
            get
            {
                //No rating can obv not cost anything
                //Makes debugging easier as we often only care about value calculation
                int intTotalBaseRating = TotalBaseRating;
                if (intTotalBaseRating == 0) return 0;

                int intCost;
                int intLower;
                if (SkillGroupObject?.Karma > 0)
                {
                    int intGroupUpper = SkillGroupObject.SkillList.Min(x => x.Base + x.Karma + x.RatingModifiers(x.Attribute));
                    int intGroupLower = intGroupUpper - SkillGroupObject.Karma;

                    intLower = Base + FreeKarma + RatingModifiers(Attribute); //Might be an error here

                    intCost = RangeCost(intLower, intGroupLower) + RangeCost(intGroupUpper, intTotalBaseRating);
                }
                else
                {
                    intLower = Base + FreeKarma + RatingModifiers(Attribute);

                    intCost = RangeCost(intLower, intTotalBaseRating);
                }

                //Don't think this is going to happen, but if it happens i want to know
                if (intCost < 0)
                    Utils.BreakIfDebug();

                int intSpecCount = 0;
                foreach (SkillSpecialization objSpec in Specializations)
                {
                    if (!objSpec.Free && (BuyWithKarma || !CharacterObject.EffectiveBuildMethodUsesPriorityTables))
                        intSpecCount += 1;
                }
                int intSpecCost = intSpecCount * CharacterObject.Options.KarmaSpecialization;
                decimal decExtraSpecCost = 0;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName != SkillCategory)
                            continue;
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            decExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decSpecCostMultiplier != 1.0m)
                    intSpecCost = (intSpecCost * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                else
                    intSpecCost += decExtraSpecCost.StandardRound(); //Spec
                intCost += intSpecCost;

                return Math.Max(0, intCost);
            }
        }

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        /// <returns></returns>
        protected int RangeCost(int lower, int upper)
        {
            if (lower >= upper)
                return 0;

            int intLevelsModded = upper * (upper + 1); //cost if nothing else was there
            intLevelsModded -= lower*(lower + 1); //remove "karma" costs from base + free

            intLevelsModded /= 2; //we get square, we need triangle

            int cost;
            if (lower == 0)
                cost = (intLevelsModded - 1) * CharacterObject.Options.KarmaImproveActiveSkill + CharacterObject.Options.KarmaNewActiveSkill;
            else
                cost = intLevelsModded * CharacterObject.Options.KarmaImproveActiveSkill;

            if (CharacterObject.Options.CompensateSkillGroupKarmaDifference)
            {
                SkillGroup objMySkillGroup = SkillGroupObject;
                if (objMySkillGroup != null)
                {
                    int intSkillGroupUpper = int.MaxValue;
                    foreach (Skill objSkillGroupMember in objMySkillGroup.SkillList)
                    {
                        if (objSkillGroupMember != this)
                        {
                            int intLoopTotalBaseRating = objSkillGroupMember.TotalBaseRating;
                            if (intLoopTotalBaseRating < intSkillGroupUpper)
                                intSkillGroupUpper = intLoopTotalBaseRating;
                        }
                    }
                    if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > lower)
                    {
                        if (intSkillGroupUpper > upper)
                            intSkillGroupUpper = upper;
                        int intGroupLevelsModded = intSkillGroupUpper * (intSkillGroupUpper + 1); //cost if nothing else was there
                        intGroupLevelsModded -= lower * (lower + 1); //remove "karma" costs from base + free

                        intGroupLevelsModded /= 2; //we get square, we need triangle

                        int intGroupCost;
                        int intNakedSkillCost = objMySkillGroup.SkillList.Count;
                        if (lower == 0)
                        {
                            intGroupCost = (intGroupLevelsModded - 1) * CharacterObject.Options.KarmaImproveSkillGroup + CharacterObject.Options.KarmaNewSkillGroup;
                            intNakedSkillCost *= (intGroupLevelsModded - 1) * CharacterObject.Options.KarmaImproveActiveSkill + CharacterObject.Options.KarmaNewActiveSkill;
                        }
                        else
                        {
                            intGroupCost = intGroupLevelsModded * CharacterObject.Options.KarmaImproveSkillGroup;
                            intNakedSkillCost *= intGroupLevelsModded * CharacterObject.Options.KarmaImproveActiveSkill;
                        }

                        cost += (intGroupCost - intNakedSkillCost);
                    }
                }
            }

            decimal decMultiplier = 1.0m;
            decimal decExtra = 0;
            ExoticSkill objThisAsExoticSkill = this as ExoticSkill;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= lower &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName) ||
                        (objThisAsExoticSkill != null && objLoopImprovement.ImprovedName == Name + " (" + objThisAsExoticSkill.Specific + ')'))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost)
                            decExtra += objLoopImprovement.Value * (Math.Min(upper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                    else if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                            decExtra += objLoopImprovement.Value * (Math.Min(upper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decMultiplier != 1.0m)
                cost = (cost * decMultiplier + decExtra).StandardRound();
            else
                cost += decExtra.StandardRound();

            if (cost < 0 && !CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                cost = 0;
            return cost;
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual int UpgradeKarmaCost
        {
            get
            {
                int intTotalBaseRating = TotalBaseRating;
                if (intTotalBaseRating >= RatingMaximum)
                {
                    return -1;
                }
                int upgrade = 0;
                int intOptionsCost;
                if (intTotalBaseRating == 0)
                {
                    intOptionsCost = CharacterObject.Options.KarmaNewActiveSkill;
                    upgrade += intOptionsCost;
                }
                else
                {
                    intOptionsCost = CharacterObject.Options.KarmaImproveActiveSkill;
                    upgrade += (intTotalBaseRating + 1) * intOptionsCost;
                }

                if (CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                {
                    SkillGroup objMySkillGroup = SkillGroupObject;
                    if (objMySkillGroup != null)
                    {
                        int intSkillGroupUpper = int.MaxValue;
                        foreach (Skill objSkillGroupMember in objMySkillGroup.SkillList)
                        {
                            if (objSkillGroupMember != this)
                            {
                                int intLoopTotalBaseRating = objSkillGroupMember.TotalBaseRating;
                                if (intLoopTotalBaseRating < intSkillGroupUpper)
                                    intSkillGroupUpper = intLoopTotalBaseRating;
                            }
                        }
                        if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > intTotalBaseRating)
                        {
                            int intGroupCost;
                            int intNakedSkillCost = objMySkillGroup.SkillList.Count;
                            if (intTotalBaseRating == 0)
                            {
                                intGroupCost = CharacterObject.Options.KarmaNewSkillGroup;
                                intNakedSkillCost *= CharacterObject.Options.KarmaNewActiveSkill;
                            }
                            else
                            {
                                intGroupCost = (intTotalBaseRating + 1) * CharacterObject.Options.KarmaImproveSkillGroup;
                                intNakedSkillCost *= (intTotalBaseRating + 1) * CharacterObject.Options.KarmaImproveActiveSkill;
                            }

                            upgrade += (intGroupCost - intNakedSkillCost);
                        }
                    }
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                ExoticSkill objThisAsExoticSkill = this as ExoticSkill;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating + 1 &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName) ||
                            (objThisAsExoticSkill != null && objLoopImprovement.ImprovedName == Name + " (" + objThisAsExoticSkill.Specific + ')'))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost)
                                decExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                                decExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    upgrade = (upgrade * decMultiplier + decExtra).StandardRound();
                else
                    upgrade += decExtra.StandardRound();

                int intMinCost = Math.Min(1, intOptionsCost);
                if (upgrade < intMinCost && !CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                    upgrade = intMinCost;
                return upgrade;
            }
        }

        public void Upgrade()
        {
            if (CharacterObject.Created)
            {
                if (!CanUpgradeCareer)
                    return;

                int price = UpgradeKarmaCost;
                int intTotalBaseRating = TotalBaseRating;
                //If data file contains {4} this crashes but...
                string upgradetext =
                    string.Format(GlobalOptions.CultureInfo, "{0}{4}{1}{4}{2}{4}->{4}{3}",
                        LanguageManager.GetString(IsKnowledgeSkill ? "String_ExpenseKnowledgeSkill" : "String_ExpenseActiveSkill"),
                        CurrentDisplayName,
                        intTotalBaseRating,
                        intTotalBaseRating + 1,
                        LanguageManager.GetString("String_Space"));

                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
                objExpense.Undo = new ExpenseUndo().CreateKarma(intTotalBaseRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, InternalId);

                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                CharacterObject.Karma -= price;
            }

            Karma += 1;
        }

        private int _intCachedCanAffordSpecialization = -1;

        public bool CanAffordSpecialization
        {
            get
            {
                if (_intCachedCanAffordSpecialization < 0)
                {
                    if (!CanHaveSpecs)
                        _intCachedCanAffordSpecialization = 0;
                    else
                    {
                        int intPrice = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                        decimal decExtraSpecCost = 0;
                        int intTotalBaseRating = TotalBaseRating;
                        decimal decSpecCostMultiplier = 1.0m;
                        foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                        {
                            if (objLoopImprovement.Minimum <= intTotalBaseRating
                                && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                    || (objLoopImprovement.Condition == "career") == CharacterObject.Created
                                    || (objLoopImprovement.Condition == "create") != CharacterObject.Created)
                                && objLoopImprovement.Enabled
                                && objLoopImprovement.ImprovedName == SkillCategory)
                            {
                                if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                                    decExtraSpecCost += objLoopImprovement.Value;
                                else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                            }
                        }
                        if (decSpecCostMultiplier != 1.0m)
                            intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                        else
                            intPrice += decExtraSpecCost.StandardRound(); //Spec

                        _intCachedCanAffordSpecialization = intPrice <= CharacterObject.Karma ? 1 : 0;
                    }
                }

                return _intCachedCanAffordSpecialization > 0;
            }
        }

        public void AddSpecialization(string strName)
        {
            SkillSpecialization nspec = new SkillSpecialization(CharacterObject, strName);
            if (CharacterObject.Created)
            {
                int intPrice = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                decimal decExtraSpecCost = 0;
                int intTotalBaseRating = TotalBaseRating;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating
                        && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                            || (objLoopImprovement.Condition == "career") == CharacterObject.Created
                            || (objLoopImprovement.Condition == "create") != CharacterObject.Created)
                        && objLoopImprovement.Enabled && objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            decExtraSpecCost += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decSpecCostMultiplier != 1.0m)
                    intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                else
                    intPrice += decExtraSpecCost.StandardRound(); //Spec

                if (intPrice > CharacterObject.Karma)
                    return;

                //If data file contains {4} this crashes but...
                string upgradetext = //TODO WRONG
                    string.Format(GlobalOptions.CultureInfo, "{0}{3}{1}{3}({2})",
                        LanguageManager.GetString("String_ExpenseLearnSpecialization"),
                        CurrentDisplayName,
                        strName,
                        LanguageManager.GetString("String_Space"));

                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intPrice * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
                objExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.AddSpecialization, nspec.InternalId);

                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                CharacterObject.Karma -= intPrice;
            }

            Specializations.Add(nspec);
        }

        /// <summary>
        /// How many free points of this skill have gotten, with the exception of some things during character creation
        /// </summary>
        /// <returns></returns>
        private int _intCachedFreeKarma = int.MinValue;
        public int FreeKarma
        {
            get
            {
                if (_intCachedFreeKarma != int.MinValue)
                    return _intCachedFreeKarma;

                return _intCachedFreeKarma = string.IsNullOrEmpty(Name)
                    ? 0
                    : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillLevel, false, Name).StandardRound();
            }
        }

        /// <summary>
        /// How many free points this skill have gotten during some parts of character creation
        /// </summary>
        /// <returns></returns>
        private int _intCachedFreeBase = int.MinValue;

        public int FreeBase
        {
            get
            {
                if (_intCachedFreeBase != int.MinValue)
                    return _intCachedFreeBase;

                return _intCachedFreeBase = string.IsNullOrEmpty(Name)
                    ? 0
                    : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillBase, false, Name).StandardRound();
            }
        }

        private int _intCachedForcedBuyWithKarma = -1;

        /// <summary>
        /// Do circumstances force the Specialization to be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool ForcedBuyWithKarma
        {
            get
            {
                if (_intCachedForcedBuyWithKarma < 0)
                {
                    _intCachedForcedBuyWithKarma = !string.IsNullOrWhiteSpace(Specialization)
                                                   && ((KarmaPoints > 0
                                                        && BasePoints + FreeBase == 0
                                                        && !CharacterObject.Options.AllowPointBuySpecializationsOnKarmaSkills)
                                                       || SkillGroupObject?.Karma > 0
                                                       || SkillGroupObject?.Base > 0)
                        ? 1
                        : 0;
                }

                return _intCachedForcedBuyWithKarma > 0;
            }
        }

        private int _intCachedForcedNotBuyWithKarma = -1;

        /// <summary>
        /// Do circumstances force the Specialization to not be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool ForcedNotBuyWithKarma
        {
            get
            {
                if (_intCachedForcedNotBuyWithKarma < 0)
                {
                    _intCachedForcedNotBuyWithKarma = TotalBaseRating == 0
                                                      || (CharacterObject.Options.StrictSkillGroupsInCreateMode
                                                          && !CharacterObject.Created
                                                          && SkillGroupObject?.Karma > 0)
                        ? 1
                        : 0;
                }

                return _intCachedForcedNotBuyWithKarma > 0;
            }
        }

        /// <summary>
        /// Dicepool of the skill, formatted for use in tooltips by other objects.
        /// </summary>
        /// <param name="intPool">Dicepool to use. In most </param>
        /// <param name="strValidSpec">A specialization to check for. If not empty, will be checked for and added to the string.</param>
        /// <returns></returns>
        public string FormattedDicePool(int intPool, string strValidSpec = "")
        {
            string strSpace = LanguageManager.GetString("String_Space");
            string strReturn = string.Format(GlobalOptions.CultureInfo, "{0}{1}({2})", CurrentDisplayName, strSpace, intPool);
            // Add any Specialization bonus if applicable.
            if (!string.IsNullOrWhiteSpace(strValidSpec))
            {
                int intSpecBonus = GetSpecializationBonus(strValidSpec);
                if (intSpecBonus != 0)
                    strReturn +=
                        string.Format(GlobalOptions.CultureInfo, "{0}{1}{0}{2}{3}{0}{4}{0}({5})", strSpace, '+', LanguageManager.GetString("String_ExpenseSpecialization"),
                            LanguageManager.GetString("String_Colon"), DisplayCategory(GlobalOptions.Language), intSpecBonus);
            }

            return strReturn;
        }
    }
}
