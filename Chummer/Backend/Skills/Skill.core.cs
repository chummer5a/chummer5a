using System;
using System.Diagnostics;
using System.Linq;

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
				int skillWire = WireRating();

				return skillWire > 0 ? skillWire : LearnedRating;
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
					x.Enabled &&
					x.ImproveType == Improvement.ImprovementType.Skill &&
					x.ImprovedName == Name).Sum(x => x.Maximum);
									
				return (_character.Created 
					? 12
					: (KnowledgeSkill && _character.BuildMethod == CharacterBuildMethod.LifeModule ? 9 : 6)) + otherbonus;
			}
		}

		/// <summary>
		/// Things that modify the dicepool of the skill
		/// </summary>
		public virtual int PoolModifiers
		{
			get
			{
				ImprovementManager manager = new ImprovementManager(_character);
				
				int intModifier = 0;
				int condition = manager.ValueOf(Improvement.ImprovementType.ConditionMonitor); 
				
				//Dump loop looking at all improvements
				foreach (Improvement objImprovement in CharacterObject.Improvements)
				{
					if (objImprovement.AddToRating && objImprovement.Enabled)
					{
						// Improvement for an individual Skill.
						//TODO NOT WORKING FOR EXOTIC SKILLS (when is that needed?)
						if (objImprovement.ImproveType == Improvement.ImprovementType.Skill && objImprovement.ImprovedName == Name)
							intModifier += objImprovement.Value;


						// Improvement for a Skill Group.
						if (objImprovement.ImproveType == Improvement.ImprovementType.SkillGroup && objImprovement.ImprovedName == _group)
						{
							if (!objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(Category))
								intModifier += objImprovement.Value;
						}
						// Improvement for a Skill Category.
						if (objImprovement.ImproveType == Improvement.ImprovementType.SkillCategory && objImprovement.ImprovedName == Category)
						{
							if (!objImprovement.Exclude.Contains(Name))
								intModifier += objImprovement.Value;
						}
						// Improvement for a Skill linked to an CharacterAttribute.
						if (objImprovement.ImproveType == Improvement.ImprovementType.SkillAttribute && objImprovement.ImprovedName == AttributeObject.Abbrev)
						{
							if (!objImprovement.Exclude.Contains(Name))
								intModifier += objImprovement.Value;
						}
						// Improvement for Enhanced Articulation
						if (Category == "Physical Active" && (AttributeObject.Abbrev == "BOD" || AttributeObject.Abbrev == "AGI" || AttributeObject.Abbrev == "REA" || AttributeObject.Abbrev == "STR"))
						{
							if (objImprovement.ImproveType == Improvement.ImprovementType.EnhancedArticulation)
								intModifier += objImprovement.Value;
						}
					}
					
				}
				
				return intModifier + Math.Min(0, condition);
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
				return Rating + attribute + PoolModifiers;
			}
			if (Default)
			{
				return attribute + PoolModifiers - 1;
			}
			return 0;
		}

		/// <summary>
		/// How much Sp this costs. Price during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public virtual int CurrentSpCost()
		{

			return _base + 
				((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);
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
			

			int cost = 0;
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

			cost /= 2;
			cost *= CharacterObject.Options.KarmaImproveActiveSkill;

			//Don't think this is going to happen, but if it happens i want to know
			if (cost < 0 && Debugger.IsAttached)
				Debugger.Break();

			cost = Math.Max(0, cost); //Don't give karma back...

			cost +=  //Spec
					(!string.IsNullOrWhiteSpace(Specialization) && BuyWithKarma) ?
					_character.Options.KarmaSpecialization : 0;


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
			if (CharacterObject.JackOfAllTrades && CharacterObject.Created)
			{
				masterAdjustment = LearnedRating > 5 ? 2 : -1;
			}

			if (LearnedRating >= RatingMaximum)
			{
				return -1;
			}
			else if (LearnedRating == 0)
			{
				return _character.Options.KarmaNewActiveSkill + masterAdjustment;
			}
			else
			{
				return  (LearnedRating + 1)*_character.Options.KarmaImproveActiveSkill + masterAdjustment;
			}
		}

		public void Upgrade()
		{
			if (!CanUpgradeCareer) return;

			int price = UpgradeKarmaCost();

			//If data file contains {4} this crashes but...
			string upgradetext =
				$"{LanguageManager.Instance.GetString("String_ExpenseSkillGroup")} {DisplayName} {LearnedRating} -> {(LearnedRating + 1)}";

			ExpenseLogEntry entry = new	ExpenseLogEntry();
			entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
			entry.Undo = new ExpenseUndo().CreateKarma(LearnedRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, Id.ToString());
			
			CharacterObject.ExpenseEntries.Add(entry);

			Karma =+ 1;
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
		protected int FreeKarma()
		{
			return (from improvement in CharacterObject.Improvements
				   where improvement.ImproveType == Improvement.ImprovementType.SkillLevel
					  && improvement.ImprovedName == _name
				  select improvement.Value).Sum();  //TODO change to ImpManager.ValueOf?
		}

		/// <summary>
		/// How many free points this skill have gotten during some parts of character creation
		/// </summary>
		/// <returns></returns>
		protected int FreeBase()
		{
			return (from improvement in CharacterObject.Improvements
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
			return !string.IsNullOrWhiteSpace(Specialization) && (_karma > 0 || SkillGroupObject?.Karma > 0 || SkillGroupObject?.Base > 0);
		}

		/// <summary>
		/// Do circumstances force the Specialization to not be bought with karma?
		/// </summary>
		/// <returns></returns>
		private bool UnForceBuyWithKarma()
		{
			return LearnedRating == 0;
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
