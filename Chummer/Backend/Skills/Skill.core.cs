using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer.Skills
{
	partial class Skill
	{
		private int _base;
		private int _karma;

		internal int IBase
		{
			get { return _base + FreeBase(); }
		}

		internal int IKarma
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
					var old = _base; // old value, not needed, don't fire too many events...

					//Calculate how far above maximum we are. 
					int overMax = (-1) * (RatingMaximum - (value + IKarma));

					if (overMax > 0) //Too much
					{
						//Get the smaller value, how far above, how much we can reduce
						max = Math.Min(overMax, _karma);

						Karma -= max; //reduce both by that amount
						overMax -= max;

						_base = Math.Max(0, value - (overMax + FreeBase())); //reduce value by leftovers
					}
					else
					{
						_base = Math.Max(value - FreeBase(), 0);
					}
					
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
				var old = _karma;
				_karma = Math.Max(0, Math.Min(value - ( FreeKarma() + _skillGroup?.Karma ?? 0), RatingMaximum - (IBase + FreeKarma() + _skillGroup?.Karma ?? 0))); 
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
			get; set;
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
					: (this.KnowledgeSkill && _character.BuildMethod == CharacterBuildMethod.LifeModule ? 9 : 6)) + otherbonus;
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
	}
}
