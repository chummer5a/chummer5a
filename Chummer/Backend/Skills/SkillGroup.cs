using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Chummer.Annotations;

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
						skillGroup._affectedSkills.Add(skill);
					return skillGroup;

				}
			}

			if (string.IsNullOrWhiteSpace(skill.SkillGroup)) return null;

			SkillGroup newGroup = new SkillGroup(skill.CharacterObject, skill.SkillGroup);
			skill.CharacterObject.SkillGroups.Add(newGroup);
			newGroup._affectedSkills.Add(skill);

			//BindingList don't have sort, so we have to play dirty
			List<SkillGroup> g = new List<SkillGroup>(skill.CharacterObject.SkillGroups.OrderBy(x => x.DisplayName));
			skill.CharacterObject.SkillGroups.Clear();
			foreach (SkillGroup skillGroup in g)
			{
				skill.CharacterObject.SkillGroups.Add(skillGroup);
			}

			return newGroup;
		}

		private List<Skill> _affectedSkills = new List<Skill>(); 
		private int _skillFromSp;
		private int _skillFromKarma;
		private string _groupName;
		private readonly Character _character;
		private SkillGroup(Character character, string groupName)
		{
			_character = character;
			_groupName = groupName;
		}
		
		//TODO CALCULATIONS STUFF HERE
		public int Base
		{
			get
			{
				return _skillFromSp + FreeLevels;

			}
			set
			{
				int tempval = value - FreeLevels;

				if (tempval != _skillFromSp)
				{
					_skillFromSp = Math.Max(tempval, 0);


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

		public int Rating
		{
			get { return Karma + Base; }
		}

		public int FreeLevels
		{
			get;
			set; //TODO REFACTOR AWAY
		}
		public int RatingMaximum
		{
			get
			{
				return (_character.Created ? 12 : 6);
			}
		}

		public bool Broken { get; set; }

		//TODO READ FROM IMPROVEMENT
		public int RatingModifiers
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

		public string Name
		{
			get { return _groupName; }
		}

		public string DisplayName
		{
			get { return Name; } //TODO TRANSLATE
		}

		public bool HasCombatSkills { get { return false; } }
		public bool HasPhysicalSkills { get { return false; } }
		public bool HasSocialSkills { get { return false; } }
		public bool HasTechnicalSkills { get { return false; } }
		public bool HasVehicleSkills { get { return false; } }
		public bool HasMagicalSkills { get { return false; } }
		public bool HasResonanceSkills { get { return false; } }

		public Character Character
		{
			get { return _character; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
