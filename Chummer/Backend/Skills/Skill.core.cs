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

		internal int Ibase
		{
			get { return _base + FreeBase(); }
		}

		internal int Ikarma
		{
			get { return _karma + FreeKarma(); }
		}

		public bool BaseUnlocked
		{
			get { return _character.BuildMethod.HaveSkillPoints() && 
					(_skillGroup == null || _skillGroup.Base <= 0); }
		}

		/// <summary>
		/// The amount of points this skill have from skill points and bonuses
		/// to the skill rating
		/// </summary>
		public int Base
		{
			get
			{
				if (_skillGroup?.Base > 0)
				{
					_base = 0;
					return _skillGroup.Base;
				}
				else
				{
					return _base + FreeBase();
				}
			}
			set
			{
				if (_skillGroup == null || _skillGroup.Base == 0)
				{
					int max = 0;
					int old = _base; // old value, not needed, don't fire too many events...

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
				}
			}
		}

		
		/// <summary>
		/// Amount of skill points bought with karma
		/// </summary>
		public int Karma
		{
			get
			{
				return _karma + FreeKarma() + _skillGroup?.Karma ?? 0;
			}
			set
			{
				int old = _karma;

				//Calculate how far above maximum we are. 
				int overMax = (-1)*(RatingMaximum - (value + Base));

				value -= Math.Max(0, overMax); //reduce by 0 or points over. 

				//Handle free levels, don,t go below 0
				_karma = Math.Max(0, value - (FreeKarma() + _skillGroup?.Karma ?? 0)); 

				if(old != _karma) OnPropertyChanged();
				
			}
		}

		/// <summary>
		/// Levels in this skill. Read only. You probably want to increase
		/// Karma instead
		/// </summary>
		public int Rating
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
				_buyWithKarma |= ForceBuyWithKarma();


				return _buyWithKarma;
			}
			set
			{
				_buyWithKarma = value;
			}
		}

		/// <summary>
		/// Maximum possible rating
		/// </summary>
		public int RatingMaximum
		{
			get
			{
				int otherbonus = 0; //TODO READ FROM IMPMANAGER
									//TODO: Disallow street sams magic skills, etc (ASPECTED!!)
				return (_character.Created
					? 12
					: (KnowledgeSkill && _character.BuildMethod == CharacterBuildMethod.LifeModule ? 9 : 6)) + otherbonus;
			}
		}

		/// <summary>
		/// Things that modify the dicepool of the skill
		/// </summary>
		public int PoolModifiers
		{
			get
			{
				int intModifier = 0;
				//Code copied from old skill. It seems to do something, but don't want to touch it with a 10 foot pole
				#region Smelly
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
						if (objImprovement.ImproveType == Improvement.ImprovementType.SkillAttribute && objImprovement.ImprovedName == UsedAttribute.Abbrev)
						{
							if (!objImprovement.Exclude.Contains(Name))
								intModifier += objImprovement.Value;
						}
						// Improvement for Enhanced Articulation
						if (Category == "Physical Active" && (UsedAttribute.Abbrev == "BOD" || UsedAttribute.Abbrev == "AGI" || UsedAttribute.Abbrev == "REA" || UsedAttribute.Abbrev == "STR"))
						{
							if (objImprovement.ImproveType == Improvement.ImprovementType.EnhancedArticulation)
								intModifier += objImprovement.Value;
						}
					}
				}
				#endregion
				return intModifier;
			}
		}

		/// <summary>
		/// The total, general pourpose dice pool for this skill, using another
		/// value for the linked attribute. This allows calculation of dice pools
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
			if (_default)
			{
				return attribute + PoolModifiers - 1;
			}
			return 0;
		}

		/// <summary>
		/// How much Sp this costs. Price during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public int CurrentSpCost()
		{
			return _base + 
				((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);
		}

		/// <summary>
		/// How much karma this costs. Return value during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public int CurrentKarmaCost()
		{
			//No rating can obv not cost anything
			//Makes debugging easier as we often only care about value calculation
			if (Rating == 0) return 0; 
			

			int cost = 0;
			if (_skillGroup?.Karma > 0)
			{
				int groupUpper = _skillGroup.GetEnumerable().Min(x => x.Base + x.Karma);
				int groupLower = groupUpper - _skillGroup.Karma;

				int lower = Base + FreeKarma(); //Might be an error here

				cost = RangeCost(lower, groupLower) + RangeCost(groupUpper, Rating);
			}
			else
			{
				int lower = Base + FreeKarma();

				cost = RangeCost(lower, Rating);
			}

			//Don't think this is going to happen, but if it happens i want to know
			if (cost < 0 && Debugger.IsAttached)
				Debugger.Break();

			cost = Math.Max(0, cost); //Don't give karma back...

			cost +=  //Spec
					(!string.IsNullOrWhiteSpace(Specialization) && BuyWithKarma) ?
					_character.Options.KarmaSpecialization : 0;


			return cost;

		}

		private int RangeCost(int lower, int upper)
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
		public int UpgradeKarmaCost()
		{
			if (Rating <= RatingMaximum)
			{
				return -1;
			}
			else if (Rating == 0)
			{
				return _character.Options.KarmaNewActiveSkill;
			}
			else
			{
				return Rating == 0
					? _character.Options.KarmaNewActiveSkill
					: (Rating + 1)*_character.Options.KarmaImproveActiveSkill;
			}
		}
		
		private int FreeKarma()
		{
			return (from improvement in CharacterObject.Improvements
				   where improvement.ImproveType == Improvement.ImprovementType.SkillLevel
					  && improvement.ImprovedName == _name
				  select improvement.Value).Sum();
		}

		private int FreeBase()
		{
			return (from improvement in CharacterObject.Improvements
				   where improvement.ImproveType == Improvement.ImprovementType.SkillBase
					  && improvement.ImprovedName == _name
				  select improvement.Value).Sum();
		}

		private bool ForceBuyWithKarma()
		{
			return _karma > 0 || _skillGroup?.Karma > 0 || _skillGroup?.Base > 0;
		}
	}
}
