using Chummer.Backend;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chummer.Backend.Equipment;
using Chummer.Backend.Attributes;
using System.Threading.Tasks;

namespace Chummer.Skills
{
    partial class Skill
    {
        private int _base;
        private int _karma;
        private bool _buyWithKarma;

        /// <summary>
        /// Base for internal use. No calling into other classes, making recursive loops impossible
        /// </summary>
        internal int Ibase
        {
            get { return _base + FreeBase(); }
        }

        /// <summary>
        /// Karma for internal use. No calling into other classes, making recursive loops impossible
        /// </summary>
        internal int Ikarma
        {
            get { return _karma + FreeKarma(); }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better that subclasses calculating Base - FreeBase()
        /// </summary>
        protected int BasePoints { get { return _base; } }

        /// <summary>
        /// How many points REALLY are in _karma Better htan subclasses calculating Karma - FreeKarma()
        /// </summary>
        protected int KarmaPoints { get { return _karma; } }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented? (Build method or skill group)
        /// </summary>
        public bool BaseUnlocked
        {
            get { return _character.BuildMethod.HaveSkillPoints() && 
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
                    _base = 0;
                    return SkillGroupObject.Base;
                }
                else
                {
                    return _base + FreeBase();
                }
            }
            set
            {
                if (SkillGroupObject != null && SkillGroupObject.Base != 0) return;

                int max = 0;
                int old = _base; // old value, don't fire too many events...

                //Calculate how far above maximum we are. 
                int overMax = (-1) * (RatingMaximum - (value + Ikarma));

                if (overMax > 0) //Too much
                {
                    //Get the smaller value, how far above, how much we can reduce
                    max = Math.Min(overMax, _karma);

                    Karma -= max; //reduce both by that amount
                    overMax -= max;

                    value -= overMax; //reduce value by leftovers, later prevents it going belov 0
                }

                _base = Math.Max(0, value - FreeBase());

                //if old != base, base changed
                if (old != _base) OnPropertyChanged(nameof(Base));

                //if max is changed, karma was too
                if(max != 0) OnPropertyChanged(nameof(Karma));
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
                    _karma = 0;
                    Specializations.RemoveAll(x => !x.Free);
                    return SkillGroupObject?.Karma ?? 0;

                }
                return _karma + FreeKarma() + (SkillGroupObject?.Karma ?? 0);
            }
            set
            {
                int old = _karma;

                //Calculate how far above maximum we are. 
                int overMax = (-1)*(RatingMaximum - (value + Base));

                value -= Math.Max(0, overMax); //reduce by 0 or points over. 

                //Handle free levels, don,t go below 0
                _karma = Math.Max(0, value - (FreeKarma() + (SkillGroupObject?.Karma ?? 0))); 

                if(old != _karma) OnPropertyChanged(nameof(Karma));

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
                int skillWire = CyberwareRating();

                return Math.Max(skillWire, TotalBaseRating);
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
                return _buyWithKarma;
            }
            set
            {
                _buyWithKarma = (value || ForceBuyWithKarma()) && !UnForceBuyWithKarma();
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
                int otherbonus = _character.Improvements.Where(x =>
                    x.ImprovedName == Name &&
                    x.Enabled &&
                    x.ImproveType == Improvement.ImprovementType.Skill).Sum(x => x.Maximum);
                return (_character.Created  || _character.IgnoreRules
                    ? 12
                    : (IsKnowledgeSkill && _character.BuildMethod == CharacterBuildMethod.LifeModule ? 9 : 6)) + otherbonus;
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

                if (SkillGroupObject?.GetEnumerable().Any(x => x.Name == ware.Extra) == true) return 0;

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
            return RelevantImprovements(x => x.AddToRating == AddToRating)?.Sum(x => x.Value) ?? 0;
        }

        private List<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return null;
            List<Improvement> lstReturn = new List<Improvement>();
            foreach (Improvement objImprovement in CharacterObject.Improvements)
            {
                if (objImprovement.Enabled && funcWherePredicate?.Invoke(objImprovement) != false)
                {
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.Skill:
                            if (objImprovement.ImprovedName == Name)
                                lstReturn.Add(objImprovement);
                            break;
                        case Improvement.ImprovementType.SkillGroup:
                            if (objImprovement.ImprovedName == _group && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(SkillCategory))
                                lstReturn.Add(objImprovement);
                            break;
                        case Improvement.ImprovementType.SkillCategory:
                            if (objImprovement.ImprovedName == SkillCategory && !objImprovement.Exclude.Contains(Name))
                                lstReturn.Add(objImprovement);
                            break;
                        case Improvement.ImprovementType.SkillAttribute:
                            if (objImprovement.ImprovedName == AttributeObject.Abbrev && !objImprovement.Exclude.Contains(Name))
                                lstReturn.Add(objImprovement);
                            break;
                        case Improvement.ImprovementType.BlockSkillDefault:
                            if (objImprovement.ImprovedName == SkillGroup)
                                lstReturn.Add(objImprovement);
                            break;
                        case Improvement.ImprovementType.SwapSkillAttribute:
                        case Improvement.ImprovementType.SwapSkillSpecAttribute:
                            if (objImprovement.Target == Name)
                                lstReturn.Add(objImprovement);
                            break;
                        case Improvement.ImprovementType.EnhancedArticulation:
                            if (Category == "Physical Active" && CharacterAttrib.PhysicalAttributes.Contains(AttributeObject.Abbrev))
                                lstReturn.Add(objImprovement);
                            break;
                    }
                }
            }
            return lstReturn;
        }

        public int WoundModifier
        {
            get
            {
                return Math.Min(0, ImprovementManager.ValueOf(_character, Improvement.ImprovementType.ConditionMonitor));
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentSpCost()
        {

            int cost = _base + ((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);
            
            int intExtra = 0;
            decimal decMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= BasePoints &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _character.Created || (objLoopImprovement.Condition == "create") != _character.Created) && objLoopImprovement.Enabled)
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

            int cost;
            int lower;
            if (SkillGroupObject?.Karma > 0)
            {
                int groupUpper = SkillGroupObject.GetEnumerable().Min(x => x.Base + x.Karma);
                int groupLower = groupUpper - SkillGroupObject.Karma;

                lower = Base + FreeKarma(); //Might be an error here

                cost = RangeCost(lower, groupLower) + RangeCost(groupUpper, intTotalBaseRating);
            }
            else
            {
                lower = Base + FreeKarma();

                cost = RangeCost(lower, intTotalBaseRating);
            }

            //Don't think this is going to happen, but if it happens i want to know
            if (cost < 0 && Debugger.IsAttached)
                Debugger.Break();

            int intSpecCount = 0;
            foreach (SkillSpecialization objSpec in Specializations)
            {
                if (!objSpec.Free && (BuyWithKarma || _character.BuildMethod == CharacterBuildMethod.Karma || _character.BuildMethod == CharacterBuildMethod.LifeModule))
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
            cost += intSpecCost + intExtraSpecCost; //Spec

            return Math.Max(0, cost);

        }

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        /// <returns></returns>
        protected int RangeCost(int lower, int upper)
        {
            if (lower == upper)
                return 0;
            //TODO: this don't handle buying new skills if new skill price != upgrade price
            int cost = upper * (upper + 1); //cost if nothing else was there
            cost -= lower*(lower + 1); //remove "karma" costs from base + free

            cost /= 2; //we get square, we need triangle

            if (cost == 1)
                cost *= _character.Options.KarmaNewActiveSkill;
            else
                cost *= _character.Options.KarmaImproveActiveSkill;

            decimal decMultiplier = 1.0m;
            int intExtra = 0;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= lower &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _character.Created || (objLoopImprovement.Condition == "create") != _character.Created) && objLoopImprovement.Enabled)
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

            return Math.Max(cost, 0);
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
                intOptionsCost = _character.Options.KarmaNewActiveSkill;
                upgrade += intOptionsCost;
            }
            else
            {
                intOptionsCost = _character.Options.KarmaImproveActiveSkill;
                upgrade += (intTotalBaseRating + 1)* intOptionsCost;
            }
            
            decimal decMultiplier = 1.0m;
            int intExtra = 0;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _character.Created || (objLoopImprovement.Condition == "create") != _character.Created) && objLoopImprovement.Enabled)
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

            return Math.Max(upgrade, Math.Min(1, intOptionsCost));
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
                $"{LanguageManager.GetString(strSkillType)} {DisplayName} {intTotalBaseRating} -> {(intTotalBaseRating + 1)}";

            ExpenseLogEntry entry = new ExpenseLogEntry(CharacterObject);
            entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
            entry.Undo = new ExpenseUndo().CreateKarma(intTotalBaseRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, Id.ToString());
            
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
                $"{LanguageManager.GetString("String_ExpenseLearnSpecialization")} {DisplayName} ({name})";

            SkillSpecialization nspec = new SkillSpecialization(name, false);

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
        private int _cachedFreeKarma = int.MinValue;
        protected int FreeKarma()
        {
            if (_cachedFreeKarma != int.MinValue) return _cachedFreeKarma;

            return _cachedFreeKarma = (from improvement in CharacterObject.Improvements
                where improvement.ImproveType == Improvement.ImprovementType.SkillLevel
                      && improvement.ImprovedName == _name
                select improvement.Value).Sum(); //TODO change to ImpManager.ValueOf?
        }

        /// <summary>
        /// How many free points this skill have gotten during some parts of character creation
        /// </summary>
        /// <returns></returns>
        private int _cachedFreeBase = int.MinValue;

        protected int FreeBase()
        {
            if (_cachedFreeBase != int.MinValue) return _cachedFreeBase;
            return _cachedFreeBase = (from improvement in CharacterObject.Improvements
                where improvement.ImproveType == Improvement.ImprovementType.SkillBase
                      && improvement.ImprovedName == _name
                select improvement.Value).Sum();
        }

        /// <summary>
        /// Do circumstances force the Specialization to be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool ForceBuyWithKarma()
        {
            return !string.IsNullOrWhiteSpace(Specialization) && ((_karma > 0 && Ibase == 0 && !CharacterObject.Options.AllowPointBuySpecializationsOnKarmaSkills)  || SkillGroupObject?.Karma > 0 || SkillGroupObject?.Base > 0);
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
                _buyWithKarma = true;
                OnPropertyChanged(nameof(BuyWithKarma));
            }
            else if (BuyWithKarma && UnForceBuyWithKarma())
            {
                _buyWithKarma = false;
                OnPropertyChanged(nameof(BuyWithKarma));
            }
            if (!CanHaveSpecs && Specializations.Count > 0)
            {
                Specializations.Clear();
            }
        }
    }
}
