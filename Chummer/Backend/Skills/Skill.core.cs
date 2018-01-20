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
using System.Diagnostics;
using System.Linq;
using Chummer.Backend.Equipment;
using Chummer.Backend.Attributes;

namespace Chummer.Backend.Skills
{
    partial class Skill : IHasName, IHasInternalId
    {
        private int _intBase;
        private int _intKarma;
        private bool _blnBuyWithKarma;

        /// <summary>
        /// Base for internal use. No calling into other classes, making recursive loops impossible
        /// </summary>
        internal int Ibase
        {
            get { return _intBase + FreeBase; }
        }

        /// <summary>
        /// Karma for internal use. No calling into other classes, making recursive loops impossible
        /// </summary>
        internal int Ikarma
        {
            get { return _intKarma + FreeKarma; }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better that subclasses calculating Base - FreeBase()
        /// </summary>
        protected int BasePoints { get { return _intBase; } }

        /// <summary>
        /// How many points REALLY are in _karma Better htan subclasses calculating Karma - FreeKarma()
        /// </summary>
        protected int KarmaPoints { get { return _intKarma; } }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented? (Build method or skill group)
        /// </summary>
        public bool BaseUnlocked
        {
            get { return _objCharacter.BuildMethod.HaveSkillPoints() && 
                    (SkillGroupObject == null || SkillGroupObject.Base <= 0); }
        }

        /// <summary>
        /// Is it possible to place points in Karma or is it prevented a stricter interprentation of the rules
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
        /// to the skill rating that would be optained in some points of character creation
        /// </summary>
        public int Base
        {
            get
            {
                if (SkillGroupObject?.Base > 0)
                {
                    _intBase = 0;
                    return SkillGroupObject.Base;
                }
                else
                {
                    return _intBase + FreeBase;
                }
            }
            set
            {
                if (SkillGroupObject != null && SkillGroupObject.Base != 0) return;

                int intMax = 0;
                int intOld = _intBase; // old value, don't fire too many events...

                //Calculate how far above maximum we are. 
                int intOverMax = (-1) * (RatingMaximum - (value + Ikarma));

                if (intOverMax > 0) //Too much
                {
                    //Get the smaller value, how far above, how much we can reduce
                    intMax = Math.Min(intOverMax, _intKarma);

                    Karma -= intMax; //reduce both by that amount
                    intOverMax -= intMax;

                    value -= intOverMax; //reduce value by leftovers, later prevents it going belov 0
                }

                _intBase = Math.Max(0, value - FreeBase);

                //if old != base, base changed
                if (intOld != _intBase)
                    OnPropertyChanged(nameof(Base));

                //if max is changed, karma was too
                if (intMax != 0)
                    OnPropertyChanged(nameof(Karma));
                KarmaSpecForcedMightChange();
            }
        }

        
        /// <summary>
        /// Amount of skill points bought with karma and bonues to the skills rating
        /// </summary>
        public int Karma
        {
            get
            {
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created && ((SkillGroupObject?.Karma ?? 0) > 0))
                {
                    _intKarma = 0;
                    Specializations.RemoveAll(x => !x.Free);
                    return SkillGroupObject?.Karma ?? 0;
                }
                return _intKarma + FreeKarma + (SkillGroupObject?.Karma ?? 0);
            }
            set
            {
                int intOld = _intKarma;

                //Calculate how far above maximum we are. 
                int intOverMax = (-1)*(RatingMaximum - (value + Base));

                value -= Math.Max(0, intOverMax); //reduce by 0 or points over. 

                //Handle free levels, don,t go below 0
                _intKarma = Math.Max(0, value - (FreeKarma + (SkillGroupObject?.Karma ?? 0))); 

                if (intOld != _intKarma)
                    OnPropertyChanged(nameof(Karma));

                KarmaSpecForcedMightChange();
            }
        }

        /// <summary>
        /// Levels in this skill. Read only. You probably want to increase
        /// Karma instead
        /// </summary>
        public int Rating
        {
            get
            {
                return Math.Max(CyberwareRating, TotalBaseRating);
            }
        }

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public int TotalBaseRating
        {
            get
            {
                if (CharacterObject.Created)
                {
                  return LearnedRating + RatingModifiers;
                }
                return LearnedRating;
            }
        }

        /// <summary>
        /// The rating the character have acctually paid for, not including skillwires
        /// or other overrides for skill Rating. Read only, you probably want to 
        /// increase Karma instead.
        /// </summary>
        public int LearnedRating
        {
            get { return Karma + Base; }
        }

        /// <summary>
        /// Is the specialization bought with karma. During career mode this is undefined
        /// </summary>
        public bool BuyWithKarma
        {
            get
            {
                return _blnBuyWithKarma;
            }
            set
            {
                _blnBuyWithKarma = (value || ForceBuyWithKarma()) && !UnForceBuyWithKarma();
                OnPropertyChanged(nameof(BuyWithKarma));
            }
        }

        /// <summary>
        /// Maximum possible rating
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                int otherbonus = _objCharacter.Improvements.Where(x =>
                    x.ImprovedName == Name &&
                    x.Enabled &&
                    x.ImproveType == Improvement.ImprovementType.Skill).Sum(x => x.Maximum);
                return (_objCharacter.Created  || _objCharacter.IgnoreRules
                    ? 12
                    : (IsKnowledgeSkill && _objCharacter.BuildMethod == CharacterBuildMethod.LifeModule ? 9 : 6)) + otherbonus;
            }
        }

        /// <summary>
        /// The total, general pourpose dice pool for this skill, using another
        /// value for the attribute part of the test. This allows calculation of dice pools
        /// while using cyberlimbs or while rigging
        /// </summary>
        /// <param name="attribute">The value of the used attribute</param>
        /// <returns></returns>
        public int PoolOtherAttribute(int attribute)
        {
            if (Rating > 0)
            {
                return Rating + attribute + PoolModifiers + WoundModifier;
            }
            if (Default)
            {
                return attribute + PoolModifiers + DefaultModifier + WoundModifier;
            }
            return 0;
        }

        public int DefaultModifier
        {
            get
            {
                if (CharacterObject.Improvements.All(x => x.ImproveType != Improvement.ImprovementType.ReflexRecorderOptimization))
                    return -1;

                Guid reflexrecorderid = Guid.Parse("17a6ba49-c21c-461b-9830-3beae8a237fc");
                Cyberware ware = CharacterObject.Cyberware.FirstOrDefault(x => x.SourceID == reflexrecorderid);

                if (ware == null) return -1;

                if (SkillGroupObject?.SkillList.Any(x => x.Name == ware.Extra) == true) return 0;

                return -1;
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int PoolModifiers
        {
            get {
                return Bonus(false);
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int RatingModifiers
        {
            get
            {
                return Bonus(true);
            }
        }

        protected int Bonus(bool AddToRating)
        {
            //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
            return RelevantImprovements(x => x.AddToRating == AddToRating).Sum(x => x.Value);
        }

        private IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.Enabled && funcWherePredicate?.Invoke(objImprovement) != false)
                    {
                        switch (objImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.Skill:

                                if (objImprovement.ImprovedName == Name)
                                {
                                    yield return objImprovement;
                                    break;
                                }
                                if (IsExoticSkill)
                                {
                                    ExoticSkill s = (ExoticSkill)this;
                                    if (objImprovement.ImprovedName == $"{Name} ({s.Specific})")
                                    {
                                        yield return objImprovement;
                                    }
                                }
                                break;
                            case Improvement.ImprovementType.SkillGroup:
                                if (objImprovement.ImprovedName == _strGroup && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(SkillCategory))
                                    yield return objImprovement;
                                break;
                            case Improvement.ImprovementType.SkillCategory:
                                if (objImprovement.ImprovedName == SkillCategory && !objImprovement.Exclude.Contains(Name))
                                    yield return objImprovement;
                                break;
                            case Improvement.ImprovementType.SkillAttribute:
                                if (objImprovement.ImprovedName == AttributeObject.Abbrev && !objImprovement.Exclude.Contains(Name))
                                    yield return objImprovement;
                                break;
                            case Improvement.ImprovementType.BlockSkillDefault:
                                if (objImprovement.ImprovedName == SkillGroup)
                                    yield return objImprovement;
                                break;
                            case Improvement.ImprovementType.SwapSkillAttribute:
                            case Improvement.ImprovementType.SwapSkillSpecAttribute:
                                if (objImprovement.Target == Name)
                                    yield return objImprovement;
                                break;
                            case Improvement.ImprovementType.EnhancedArticulation:
                                if (_strCategory == "Physical Active" && AttributeSection.PhysicalAttributes.Contains(AttributeObject.Abbrev))
                                    yield return objImprovement;
                                break;
                        }
                    }
                }
            }
        }

        public int WoundModifier
        {
            get
            {
                return Math.Min(0, ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ConditionMonitor));
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentSpCost()
        {

            int cost = _intBase + ((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);
            
            int intExtra = 0;
            decimal decMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= BasePoints &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillPointCost)
                            intExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillPointCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                    else if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCost)
                            intExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decMultiplier != 1.0m)
                cost = decimal.ToInt32(decimal.Ceiling(cost * decMultiplier));
            cost += intExtra;

            return Math.Max(cost, 0);
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentKarmaCost()
        {
            //No rating can obv not cost anything
            //Makes debugging easier as we often only care about value calculation
            int intTotalBaseRating = TotalBaseRating;
            if (intTotalBaseRating == 0) return 0;

            int intCost;
            int intLower;
            if (SkillGroupObject?.Karma > 0)
            {
                int intGroupUpper = SkillGroupObject.SkillList.Min(x => x.Base + x.Karma);
                int intGroupLower = intGroupUpper - SkillGroupObject.Karma;

                intLower = Base + FreeKarma; //Might be an error here

                intCost = RangeCost(intLower, intGroupLower) + RangeCost(intGroupUpper, intTotalBaseRating);
            }
            else
            {
                intLower = Base + FreeKarma;

                intCost = RangeCost(intLower, intTotalBaseRating);
            }

            //Don't think this is going to happen, but if it happens i want to know
            if (intCost < 0)
                Utils.BreakIfDebug();

            int intSpecCount = 0;
            foreach (SkillSpecialization objSpec in Specializations)
            {
                if (!objSpec.Free && (BuyWithKarma || _objCharacter.BuildMethod == CharacterBuildMethod.Karma || _objCharacter.BuildMethod == CharacterBuildMethod.LifeModule))
                    intSpecCount += 1;
            }
            int intSpecCost = intSpecCount * (IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization);
            int intExtraSpecCost = 0;
            decimal decSpecCostMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            intExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decSpecCostMultiplier != 1.0m)
                intSpecCost = decimal.ToInt32(decimal.Ceiling(intSpecCost * decSpecCostMultiplier));
            intCost += intSpecCost + intExtraSpecCost; //Spec

            return Math.Max(0, intCost);

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

            int cost = 0;
            if (lower == 0)
                cost = (intLevelsModded - 1) * _objCharacter.Options.KarmaImproveActiveSkill + _objCharacter.Options.KarmaNewActiveSkill;
            else
                cost = intLevelsModded * _objCharacter.Options.KarmaImproveActiveSkill;

            if (_objCharacter.Options.CompensateSkillGroupKarmaDifference)
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

                        int intGroupCost = 0;
                        int intNakedSkillCost = objMySkillGroup.SkillList.Count;
                        if (lower == 0)
                        {
                            intGroupCost = (intGroupLevelsModded - 1) * _objCharacter.Options.KarmaImproveSkillGroup + _objCharacter.Options.KarmaNewSkillGroup;
                            intNakedSkillCost *= (intGroupLevelsModded - 1) * _objCharacter.Options.KarmaImproveActiveSkill + _objCharacter.Options.KarmaNewActiveSkill;
                        }
                        else
                        {
                            intGroupCost = intGroupLevelsModded * _objCharacter.Options.KarmaImproveSkillGroup;
                            intNakedSkillCost *= intGroupLevelsModded * _objCharacter.Options.KarmaImproveActiveSkill;
                        }

                        cost += (intGroupCost - intNakedSkillCost);
                    }
                }
            }

            decimal decMultiplier = 1.0m;
            int intExtra = 0;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= lower &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost)
                            intExtra += objLoopImprovement.Value * (Math.Min(upper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                    else if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                            intExtra += objLoopImprovement.Value * (Math.Min(upper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decMultiplier != 1.0m)
                cost = decimal.ToInt32(decimal.Ceiling(cost * decMultiplier));
            cost += intExtra;

            if (cost < 0 && !_objCharacter.Options.CompensateSkillGroupKarmaDifference)
                cost = 0;
            return cost;
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual int UpgradeKarmaCost()
        {
            int intTotalBaseRating = TotalBaseRating;
            if (intTotalBaseRating >= RatingMaximum)
            {
                return -1;
            }
            int upgrade = 0;
            int intOptionsCost = 1;
            if (intTotalBaseRating == 0)
            {
                intOptionsCost = _objCharacter.Options.KarmaNewActiveSkill;
                upgrade += intOptionsCost;
            }
            else
            {
                intOptionsCost = _objCharacter.Options.KarmaImproveActiveSkill;
                upgrade += (intTotalBaseRating + 1) * intOptionsCost;
            }

            if (_objCharacter.Options.CompensateSkillGroupKarmaDifference)
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
                        int intGroupCost = 0;
                        int intNakedSkillCost = objMySkillGroup.SkillList.Count;
                        if (intTotalBaseRating == 0)
                        {
                            intGroupCost = _objCharacter.Options.KarmaNewSkillGroup;
                            intNakedSkillCost *= _objCharacter.Options.KarmaNewActiveSkill;
                        }
                        else
                        {
                            intGroupCost = (intTotalBaseRating + 1) * _objCharacter.Options.KarmaImproveSkillGroup;
                            intNakedSkillCost *= (intTotalBaseRating + 1) * _objCharacter.Options.KarmaImproveActiveSkill;
                        }

                        upgrade += (intGroupCost - intNakedSkillCost);
                    }
                }
            }

            decimal decMultiplier = 1.0m;
            int intExtra = 0;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost)
                            intExtra += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                    else if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                            intExtra += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decMultiplier != 1.0m)
                upgrade = decimal.ToInt32(decimal.Ceiling(upgrade * decMultiplier));
            upgrade += intExtra;

            int intMinCost = Math.Min(1, intOptionsCost);
            if (upgrade < intMinCost && !_objCharacter.Options.CompensateSkillGroupKarmaDifference)
                upgrade = intMinCost;
            return upgrade;
        }

        public void Upgrade()
        {
            if (!CanUpgradeCareer) return;

            int price = UpgradeKarmaCost();
            int intTotalBaseRating = TotalBaseRating;
            string strSkillType = "String_ExpenseActiveSkill";
            if (IsKnowledgeSkill)
            {
                strSkillType = "String_ExpenseKnowledgeSkill";
            }
            //If data file contains {4} this crashes but...
            string upgradetext =
                $"{LanguageManager.GetString(strSkillType, GlobalOptions.Language)} {DisplayNameMethod(GlobalOptions.Language)} {intTotalBaseRating} 🡒 {(intTotalBaseRating + 1)}";

            ExpenseLogEntry entry = new ExpenseLogEntry(CharacterObject);
            entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
            entry.Undo = new ExpenseUndo().CreateKarma(intTotalBaseRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, InternalId);
            
            CharacterObject.ExpenseEntries.Add(entry);

            Karma += 1;
            CharacterObject.Karma -= price;
        }

        private bool _oldCanAffordSpecialization = false;
        public bool CanAffordSpecialization
        {
            get
            {
                if (!CanHaveSpecs)
                    return false;
                int price = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                int intExtraSpecCost = 0;
                int intTotalBaseRating = TotalBaseRating;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                                intExtraSpecCost += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decSpecCostMultiplier != 1.0m)
                    price = decimal.ToInt32(decimal.Ceiling(price * decSpecCostMultiplier));
                price += intExtraSpecCost; //Spec

                return price <= CharacterObject.Karma;
            }
        }

        public void AddSpecialization(string name)
        {
            int price = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

            int intExtraSpecCost = 0;
            int intTotalBaseRating = TotalBaseRating;
            decimal decSpecCostMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            intExtraSpecCost += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decSpecCostMultiplier != 1.0m)
                price = decimal.ToInt32(decimal.Ceiling(price * decSpecCostMultiplier));
            price += intExtraSpecCost; //Spec

            if (price > CharacterObject.Karma)
                return;

            //If data file contains {4} this crashes but...
            string upgradetext = //TODO WRONG
                $"{LanguageManager.GetString("String_ExpenseLearnSpecialization", GlobalOptions.Language)} {DisplayNameMethod(GlobalOptions.Language)} ({name})";

            SkillSpecialization nspec = new SkillSpecialization(name, false, this);

            ExpenseLogEntry entry = new ExpenseLogEntry(CharacterObject);
            entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
            entry.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.AddSpecialization, nspec.InternalId);

            CharacterObject.ExpenseEntries.Add(entry);

            Specializations.Add(nspec);
            CharacterObject.Karma -= price;
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

                return _intCachedFreeKarma = string.IsNullOrEmpty(Name) ? 0 : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillLevel, false, Name);
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

                return _intCachedFreeBase = string.IsNullOrEmpty(Name) ? 0 : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillBase, false, Name);
            }
        }

        /// <summary>
        /// Do circumstances force the Specialization to be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool ForceBuyWithKarma()
        {
            return !string.IsNullOrWhiteSpace(Specialization) && ((_intKarma > 0 && Ibase == 0 && !CharacterObject.Options.AllowPointBuySpecializationsOnKarmaSkills)  || SkillGroupObject?.Karma > 0 || SkillGroupObject?.Base > 0);
        }

        /// <summary>
        /// Do circumstances force the Specialization to not be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool UnForceBuyWithKarma()
        {
            return TotalBaseRating == 0 || (CharacterObject.Options.StrictSkillGroupsInCreateMode && ((SkillGroupObject?.Karma ?? 0) > 0));
        }

        /// <summary>
        /// This checks if BuyWithKarma have been force changed and fires an event if so
        /// </summary>
        private void KarmaSpecForcedMightChange()
        {
            if (!BuyWithKarma && ForceBuyWithKarma())
            {
                _blnBuyWithKarma = true;
                OnPropertyChanged(nameof(BuyWithKarma));
            }
            else if (BuyWithKarma && UnForceBuyWithKarma())
            {
                _blnBuyWithKarma = false;
                OnPropertyChanged(nameof(BuyWithKarma));
            }
            if (!CanHaveSpecs && Specializations.Count > 0)
            {
                Specializations.Clear();
            }
        }
    }
}
