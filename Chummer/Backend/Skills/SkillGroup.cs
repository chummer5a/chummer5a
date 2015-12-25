using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Chummer.Annotations;
using Chummer.Datastructures;

namespace Chummer.Skills
{
	public class SkillGroup : INotifyPropertyChanged
	{
		internal static SkillGroup Get(Skill skill)
		{
			if(skill.SkillGroupObject != null) return skill.SkillGroupObject;

			foreach (SkillGroup skillGroup in skill.CharacterObject.SkillGroups)
			{
				if (skillGroup._groupName == skill.SkillGroup)
				{
					if(!skillGroup._affectedSkills.Contains(skill))
						skillGroup.Add(skill);
					return skillGroup;

				}
			}

			if (string.IsNullOrWhiteSpace(skill.SkillGroup)) return null;

			SkillGroup newGroup = new SkillGroup(skill.CharacterObject, skill.SkillGroup);
			skill.CharacterObject.SkillGroups.Add(newGroup);
			newGroup.Add(skill);

			//BindingList don't have sort, so we have to play dirty
			List<SkillGroup> g = new List<SkillGroup>(skill.CharacterObject.SkillGroups.OrderBy(x => x.DisplayName));
			skill.CharacterObject.SkillGroups.Clear();
			foreach (SkillGroup skillGroup in g)
			{
				skill.CharacterObject.SkillGroups.Add(skillGroup);
			}

			return newGroup;
		}

		private void Add(Skill skill)
		{
			_affectedSkills.Add(skill);
			skill.PropertyChanged += SkillOnPropertyChanged;
		}

		private void SkillOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == nameof(Skill.Base))
			{
				if(_baseBrokenOldValue != BaseUnbroken)
					OnPropertyChanged(nameof(BaseUnbroken));

				_baseBrokenOldValue = BaseUnbroken;
			}
		}

		private bool _baseBrokenOldValue = true;
		private List<Skill> _affectedSkills = new List<Skill>(); 
		private int _skillFromSp;
		private int _skillFromKarma;
		private readonly string _groupName;
		private readonly Character _character;
		private SkillGroup(Character character, string groupName)
		{
			_character = character;
			_groupName = groupName;

			ImprovementEvent += OnImprovementEvent;
		}
		
		//TODO CALCULATIONS STUFF HERE
		public int Base
		{
			get
			{
				return _skillFromSp + FreeBase;

			}
			set
			{
				if (this.BaseUnbroken)
				{
					int tempval = value - FreeBase;

					_skillFromSp = Math.Min(Math.Max(tempval, 0), RatingMaximum - (Karma + FreeBase));
					
					OnPropertyChanged();
				}
			}
		}

		public int Karma
		{
			get { return _skillFromKarma; }
			set
			{
				_skillFromKarma = value;
				OnPropertyChanged();
			}
		}

		public bool BaseUnbroken
		{
			get
			{
				bool ret = _affectedSkills.Any(x => x.IBase > 0);
				return !ret;
			}
		}

		public int Rating
		{
			get { return Karma + Base; }
		}

		internal int FreeBase
		{
			get
			{
				return (from improvement in _character.Improvements
					   where improvement.ImproveType == Improvement.ImprovementType.SkillGroupBase
					      && improvement.ImprovedName == _groupName
					  select improvement.Value).Sum();
			} 
		}

		int FreeLevels
		{
			get
			{
				return (from improvement in _character.Improvements
					   where improvement.ImproveType == Improvement.ImprovementType.SkillGroupLevel
						  && improvement.ImprovedName == _groupName
					  select improvement.Value).Sum();
			}
			
		}

		public int RatingMaximum
		{
			get
			{
				return (_character.Created ? 12 : 6);
			}
		}

		public int PoolModifiers
		{
			get
			{
				int intModifier = 0;
				//Code copied from old skill. It seems to do something, but don't want to touch it with a 10 foot pole
				#region Smelly
				//TODO this ever gets used, refactor thing away?
				//foreach (Improvement objImprovement in _character.Improvements)
				//{
				//	if (objImprovement.AddToRating && objImprovement.Enabled)
				//	{
				//		// Improvement for an individual Skill.
				//		//TODO NOT WORKING FOR EXOTIC SKILLS (when is that needed?)
				//		if (objImprovement.ImproveType == Improvement.ImprovementType.Skill && objImprovement.ImprovedName == Name)
				//			intModifier += objImprovement.Value;


				//		// Improvement for a Skill Group.
				//		if (objImprovement.ImproveType == Improvement.ImprovementType.SkillGroup && objImprovement.ImprovedName == _group)
				//		{
				//			if (!objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(_category))
				//				intModifier += objImprovement.Value;
				//		}
				//		// Improvement for a Skill Category.
				//		if (objImprovement.ImproveType == Improvement.ImprovementType.SkillCategory && objImprovement.ImprovedName == _category)
				//		{
				//			if (!objImprovement.Exclude.Contains(Name))
				//				intModifier += objImprovement.Value;
				//		}
				//		// Improvement for a Skill linked to an CharacterAttribute.
				//		if (objImprovement.ImproveType == Improvement.ImprovementType.SkillAttribute && objImprovement.ImprovedName == _usedAttribute.Abbrev)
				//		{
				//			if (!objImprovement.Exclude.Contains(Name))
				//				intModifier += objImprovement.Value;
				//		}
				//		// Improvement for Enhanced Articulation
				//		if (_category == "Physical Active" && (_usedAttribute.Abbrev == "BOD" || _usedAttribute.Abbrev == "AGI" || _usedAttribute.Abbrev == "REA" || _usedAttribute.Abbrev == "STR"))
				//		{
				//			if (objImprovement.ImproveType == Improvement.ImprovementType.EnhancedArticulation)
				//				intModifier += objImprovement.Value;
				//		}
				//	}
				//}
				#endregion
				return intModifier;
			}
		}

		public Character Character
		{
			get { return _character; }
		}

		public string Name
		{
			get { return _groupName; }
		}

		public string DisplayName
		{
			get { return Name; } //TODO TRANSLATE
		}

		public bool HasCombatSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Combat Active"); }
		}

		public bool HasPhysicalSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Physical Active"); ; }
		}

		public bool HasSocialSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Social Active"); ; }
		}

		public bool HasTechnicalSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Technical Active"); ; }
		}

		public bool HasVehicleSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Vehicle Active"); ; }
		}

		public bool HasMagicalSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Magical Active"); ; }
		}

		public bool HasResonanceSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Resonance Active"); ; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		[Obsolete("Refactor this method away once improvementmanager gets outbound events")]
		private void OnImprovementEvent(List<Improvement> improvements, ImprovementManager improvementManager)
		{
			if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.SkillGroupLevel
			                            && imp.ImprovedName == _groupName))
			{
				OnPropertyChanged(nameof(FreeLevels));
				OnPropertyChanged(nameof(Base));
				//OnPropertyChanged(nameof(Base));
			}

		}

		//I also think this prevents GC. But there is no good way to do it short of rewriting improvements
		private static event Action<List<Improvement>, ImprovementManager> ImprovementEvent;
		//To get when things change in improvementmanager
		//Ugly, ugly done, but we cannot get events out of it today
		// FUTURE REFACTOR HERE
		[Obsolete("Refactor this method away once improvementmanager gets outbound events")]
		internal static void ImprovementHook(List<Improvement> _lstTransaction, ImprovementManager improvementManager)
		{
			ImprovementEvent?.Invoke(_lstTransaction, improvementManager);
		}

		[Obsolete("Only here as old code depends on it, remove in time")]
		public bool Broken { get; set; }
	}
}
