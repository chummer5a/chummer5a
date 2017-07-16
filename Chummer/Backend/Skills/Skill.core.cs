using Chummer.Backend;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chummer.Backend.Equipment;
using Chummer.Backend.Attributes;

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
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode)
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
                if (old != _base) OnPropertyChanged();

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
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && ((SkillGroupObject?.Karma ?? 0) > 0))
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

                if(old != _karma) OnPropertyChanged();



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

                return Math.Max(skillWire, LearnedRating + Bonus(true));
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
                _buyWithKarma = (value ||  ForceBuyWithKarma()) && !UnForceBuyWithKarma();
                OnPropertyChanged();
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

                if (SkillGroupObject?.GetEnumerable().Any(x => x.Name == ware.Location) == true) return 0;

                return -1;
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public virtual int PoolModifiers
        {
            get {
                return Bonus(false);
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public virtual int RatingModifiers
        {
            get
            {
                return Bonus(true);
            }
        }

        private int Bonus(bool AddToRating)
        {
            //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
            return RelevantImprovements().Where(x => x.AddToRating == AddToRating).Sum(x => x.Value);
        }

        private IEnumerable<Improvement> RelevantImprovements()
        {
            if (string.IsNullOrWhiteSpace(Name)) yield break;
            foreach (Improvement objImprovement in CharacterObject.Improvements)
            {
                if(!objImprovement.Enabled) continue;

                switch (objImprovement.ImproveType)
                {
                    case Improvement.ImprovementType.Skill:
                        if (objImprovement.ImprovedName == Name)
                            yield return objImprovement;
                        break;
                    case Improvement.ImprovementType.SkillGroup:
                        if(objImprovement.ImprovedName == _group && !objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(SkillCategory))
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
                        if (Category == "Physical Active" && CharacterAttrib.PhysicalAttributes.Contains(AttributeObject.Abbrev))
                            yield return objImprovement;
                        break;
                }
            }
        }

        public int WoundModifier
        {
            get
            {
                return Math.Min(0, new ImprovementManager(_character).ValueOf(Improvement.ImprovementType.ConditionMonitor));
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentSpCost()
        {

            int cost = _base +
                       ((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);

            if (Unaware()) cost *= 2;

            return cost;
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentKarmaCost()
        {
            //No rating can obv not cost anything
            //Makes debugging easier as we often only care about value calculation
            if (LearnedRating == 0) return 0;

            int cost;
            if (SkillGroupObject?.Karma > 0)
            {
                int groupUpper = SkillGroupObject.GetEnumerable().Min(x => x.Base + x.Karma);
                int groupLower = groupUpper - SkillGroupObject.Karma;

                int lower = Base + FreeKarma(); //Might be an error here

                cost = RangeCost(lower, groupLower) + RangeCost(groupUpper, LearnedRating);
            }
            else
            {
                int lower = Base + FreeKarma();

                cost = RangeCost(lower, LearnedRating);
            }

            //Don't think this is going to happen, but if it happens i want to know
            if (cost < 0 && Debugger.IsAttached)
                Debugger.Break();

            cost = Math.Max(0, cost); //Don't give karma back...

            foreach (SkillSpecialization objSpec in Specializations)
            {
                if (!objSpec.Free)
                    cost += //Spec
                    (BuyWithKarma || _character.BuildMethod == CharacterBuildMethod.Karma ||
                      _character.BuildMethod == CharacterBuildMethod.LifeModule)
                        ? _character.Options.KarmaSpecialization
                        : 0;
            }

            if (Unaware()) cost *= 2;

            return cost;

        }

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        /// <returns></returns>
        protected int RangeCost(int lower, int upper)
        {
            //TODO: this don't handle buying new skills if new skill price != upgrade price
            int cost = upper * (upper + 1); //cost if nothing else was there
            cost -= lower*(lower + 1); //remove "karma" costs from base + free

            cost /= 2; //we get square, we need triangle

            cost *= _character.Options.KarmaImproveActiveSkill;
            return cost;
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual int UpgradeKarmaCost()
        {
            int masterAdjustment = 0;
            if (CharacterObject.SkillsSection.JackOfAllTrades && CharacterObject.Created)
            {
                masterAdjustment = LearnedRating >= 5 ? 2 : -1;
            }

            int upgrade;
            if (LearnedRating >= RatingMaximum)
            {
                upgrade = -1;
            }
            else if (LearnedRating == 0)
            {
                upgrade = _character.Options.KarmaNewActiveSkill + masterAdjustment;
            }
            else
            {
                upgrade = (LearnedRating + 1)*_character.Options.KarmaImproveActiveSkill + masterAdjustment;
            }

            if (Unaware()) upgrade *= 2;

            return upgrade;

        }

        //Character is really bad at this. Uncouth and a social skill or Uneducated and technical skill
        private bool Unaware()
        {
            if (CharacterObject.SkillsSection.Uncouth && Category == "Social Active")
            {
                return true;
            }

            if (CharacterObject.SkillsSection.Uneducated && Category == "Technical Active")
            {
                return true;
            }

            return false;
        }

        public void Upgrade()
        {
            if (!CanUpgradeCareer) return;

            int price = UpgradeKarmaCost();
            string strSkillType = "String_ExpenseActiveSkill";
            if (IsKnowledgeSkill)
            {
                strSkillType = "String_ExpenseKnowledgeSkill";
            }
            //If data file contains {4} this crashes but...
            string upgradetext =
                $"{LanguageManager.Instance.GetString(strSkillType)} {DisplayName} {LearnedRating} -> {(LearnedRating + 1)}";

            ExpenseLogEntry entry = new    ExpenseLogEntry();
            entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
            entry.Undo = new ExpenseUndo().CreateKarma(LearnedRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, Id.ToString());
            
            CharacterObject.ExpenseEntries.Add(entry);

            Karma += 1;
            CharacterObject.Karma -= price;
        }


        public void AddSpecialization(string name)
        {
            if (!CharacterObject.CanAffordSpecialization) return;
            

            int price = CharacterObject.Options.KarmaSpecialization;

            //If data file contains {4} this crashes but...
            string upgradetext = //TODO WRONG
                $"{LanguageManager.Instance.GetString("String_ExpenseLearnSpecialization")} {DisplayName} ({name})";

            SkillSpecialization nspec = new SkillSpecialization(name, false);

            ExpenseLogEntry entry = new ExpenseLogEntry();
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
            return !string.IsNullOrWhiteSpace(Specialization) && ((_karma > 0 && Ibase == 0)  || SkillGroupObject?.Karma > 0 || SkillGroupObject?.Base > 0);
        }

        /// <summary>
        /// Do circumstances force the Specialization to not be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool UnForceBuyWithKarma()
        {
            return LearnedRating == 0 || (CharacterObject.Options.StrictSkillGroupsInCreateMode && ((SkillGroupObject?.Karma ?? 0) > 0));
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
        }
    }
}
