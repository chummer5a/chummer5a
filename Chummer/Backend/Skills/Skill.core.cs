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
					var old = _base; //TODO: SWARP MIN MAX ORDER
					_base = Math.Min(Math.Max(0, value - FreeBase()), RatingMaximum - (IKarma + FreeBase()));
					if(old != _base) OnPropertyChanged();
				}
			}
		}

		public bool BaseUnlocked
		{
			get { return _skillGroup == null || _skillGroup.Base <= 0; }
		}

		/// <summary>
		/// Amount of skill points bought with karma
		/// </summary>
		public int Karma
		{
			get
			{
				return _karma;
				
			}
			set
			{
				_karma = value; 
				
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
	}
}
