using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Chummer.Annotations;

namespace Chummer.Skills
{
	public class SkillGroup : INotifyPropertyChanged
	{
		#region Core calculations
		private int _skillFromSp;
		private int _skillFromKarma;

		public int Base
		{
			get
			{
				if (Character.Uncouth && HasSocialSkills) return 0;
				return _skillFromSp + FreeBase();
			}
			set
			{
				if (BaseUnbroken)
				{
					int old = _skillFromSp;

					//Calculate how far above maximum we are. 
					int overMax = (-1) * (RatingMaximum - (value + _skillFromKarma + FreeLevels()));

					//reduce value by max or 0
					//TODO karma from skill, karma other stuff might be reduced
					value -= Math.Max(0, overMax);

					//and save back, cannot go under 0
					_skillFromSp = Math.Max(0, value - FreeBase());

					if (old != _skillFromSp) OnPropertyChanged();
				}
			}
		}

		public int Karma
		{
			get
			{
				if (Character.Uncouth && HasSocialSkills) return 0;
				return _skillFromKarma + FreeLevels();
			}
			set
			{
				if (KarmaUnbroken)
				{
					int old = _skillFromKarma;

					//Calculate how far above maximum we are. 
					int overMax = (-1) * (RatingMaximum - (value + _skillFromSp + FreeBase()));

					//reduce value by max or 0
					//TODO can remove karma from skills
					value -= Math.Max(0, overMax);

					//and save back, cannot go under 0
					_skillFromKarma = Math.Max(0, value - FreeLevels());

					if (old != _skillFromKarma) OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Is it possible to increment this skill group from points
		/// Inverted to simplifly databinding
		/// </summary>
		public bool BaseUnbroken
		{
			get
			{
				if (Character.Uncouth && HasSocialSkills) return false;
				return _character.BuildMethod.HaveSkillPoints() && !_affectedSkills.Any(x => x.Ibase > 0);
			}
		}

		/// <summary>
		/// Is it possible to increment this skill group from karma
		/// Inverted to simplifly databinding
		/// </summary>
		public bool KarmaUnbroken
		{
			get
			{
				if (Character.Uncouth && HasSocialSkills) return false;
				int high = _affectedSkills.Max(x => x.Ibase);
				bool ret = _affectedSkills.Any(x => x.Ibase + x.Ikarma < high);

				return !ret;
			}
		}

		/// <summary>
		/// Can this skillgroup be increaced in career mode?
		/// </summary>
		public bool CareerIncrease
		{
			get
			{
				if (_affectedSkills.Count == 0) return false;

				if (_affectedSkills.Any(x => x.LearnedRating != _affectedSkills[0].LearnedRating))
				{
					return false;
				}

				if (_affectedSkills.Any(x => x.Specializations.Count != 0))
				{
					return false;
				}

				if (Character.Uncouth && HasSocialSkills) return false;

				return _affectedSkills.Max(x => x.LearnedRating) < RatingMaximum;
			}
		}

		public int Rating
		{
			get { return Karma + Base; }
		}

		internal int FreeBase()
		{
			return (from improvement in _character.Improvements
					where improvement.ImproveType == Improvement.ImprovementType.SkillGroupBase
					   && improvement.ImprovedName == _groupName
					select improvement.Value).Sum();
		}

		int FreeLevels()
		{
			return (from improvement in _character.Improvements
					where improvement.ImproveType == Improvement.ImprovementType.SkillGroupLevel
					   && improvement.ImprovedName == _groupName
					select improvement.Value).Sum();
		}

		public int RatingMaximum
		{
			get
			{
				return (_character.Created ? 12 : 6);
			}
		}

		public void Upgrade()
		{
			if (!CareerIncrease) return;

			int price = UpgradeKarmaCost();

			//If data file contains {4} this crashes but...
			string upgradetext =
				$"{LanguageManager.Instance.GetString("String_ExpenseActiveSkill")} {DisplayName} {Rating} -> {(Rating + 1)}";

			ExpenseLogEntry entry = new ExpenseLogEntry();
			entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
			entry.Undo = new ExpenseUndo().CreateKarma(Rating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, Name);

			Character.ExpenseEntries.Add(entry);

			Karma = +1;
			Character.Karma -= price;
		}

		#endregion

		#region All the other stuff that is required
		internal static SkillGroup Get(Skill skill)
		{
			if(skill.SkillGroupObject != null) return skill.SkillGroupObject;

			if (skill.SkillGroup == null) return null;

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
			_toolTip = null;
			OnPropertyChanged(nameof(ToolTip));
			skill.PropertyChanged += SkillOnPropertyChanged;
		}

		internal void WriteTo(XmlWriter writer)
		{
			writer.WriteStartElement("group");

			writer.WriteElementString("karma", _skillFromKarma.ToString());
			writer.WriteElementString("base", _skillFromSp.ToString());
			writer.WriteElementString("id", Id.ToString());
			writer.WriteElementString("name", _groupName);
			
			writer.WriteEndElement();
		}

		internal static SkillGroup Load(Character character, XmlNode saved)
		{
			Guid g;

			SkillGroup group = new SkillGroup(character, saved["name"].InnerText);

			saved.TryGetField("karma", out group._skillFromKarma);
			saved.TryGetField("base", out group._skillFromSp);
			saved.TryGetField("id", Guid.TryParse, out g);

			group.Id = g;


			return group;
		}

		private void SkillOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == nameof(Skill.Base))
			{
				if (_baseBrokenOldValue != BaseUnbroken)
					OnPropertyChanged(nameof(BaseUnbroken));

				_baseBrokenOldValue = BaseUnbroken;
			}

			if (propertyChangedEventArgs.PropertyName == nameof(Skill.Base) ||
			    propertyChangedEventArgs.PropertyName == nameof(Skill.Karma))
			{
				if (!KarmaUnbroken && _skillFromKarma > 0)
				{
					_skillFromKarma = 0;
					OnPropertyChanged(nameof(Karma));
				}

				if (_karmaBrokenOldValue != KarmaUnbroken) { 
					OnPropertyChanged(nameof(KarmaUnbroken));
}
				_karmaBrokenOldValue = KarmaUnbroken;
			}

			if (_careerIncreaseOldValue != CareerIncrease)
			{
				_careerIncreaseOldValue = CareerIncrease;
				OnPropertyChanged(nameof(CareerIncrease));
			}
		}

		private bool _baseBrokenOldValue;
		private bool _karmaBrokenOldValue;
		private bool _careerIncreaseOldValue;
		private readonly List<Skill> _affectedSkills = new List<Skill>(); 
		private readonly string _groupName;
		private readonly Character _character;
		
		private SkillGroup(Character character, string groupName)
		{
			_character = character;
			_groupName = groupName;
			_baseBrokenOldValue = BaseUnbroken;

			// ReSharper disable once ExplicitCallerInfoArgument
			Character.UncouthChanged += sender =>
			{
				if(HasSocialSkills)
				{
					OnPropertyChanged(nameof(Rating));
					OnPropertyChanged(nameof(BaseUnbroken));
					OnPropertyChanged(nameof(KarmaUnbroken));
				}
			};

			ImprovementEvent += OnImprovementEvent;
		}

		public Character Character
		{
			get { return _character; }
		}

		public string Name
		{
			get { return _groupName; }
		}

		private string _cachedDisplayName = null;
		public string DisplayName
		{
			get
			{
				if(_cachedDisplayName != null) return _cachedDisplayName;
				 
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/skillgroups/name[. = \"" + Name + "\"]");
					if (objNode != null)
					{
						if (objNode.Attributes["translate"] != null)
							return _cachedDisplayName = objNode.Attributes["translate"].InnerText;
					}
				}
				return _cachedDisplayName = Name;
			} 
		}

		private string _toolTip = null;
		public string ToolTip
		{
			get
			{
				if (_toolTip != null) return _toolTip;

				_toolTip = LanguageManager.Instance.GetString("Tip_SkillGroup_Skills");
				_toolTip += " ";
				_toolTip += string.Join(", ", _affectedSkills.Select(x => x.DisplayName));

				return _toolTip;
			}
		}

		public Guid Id { get; private set; } = Guid.NewGuid();


		#region HasWhateverSkills
		public bool HasCombatSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Combat Active"); }
		}

		public bool HasPhysicalSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Physical Active"); }
		}

		public bool HasSocialSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Social Active"); }
		}

		public bool HasTechnicalSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Technical Active"); }
		}

		public bool HasVehicleSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Vehicle Active"); }
		}

		public bool HasMagicalSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Magical Active"); }
		}

		public bool HasResonanceSkills
		{
			get { return _affectedSkills.Any(x => x.SkillCategory == "Resonance Active"); }
		}
		#endregion

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

		//TODO static, but per character? This is probably a bug
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

		public int CurrentSpCost()
		{
			return _skillFromSp;
		}

		public int CurrentKarmaCost()
		{
			if (_skillFromKarma == 0) return 0;

			int upper = _affectedSkills.Min(x => x.LearnedRating);
			int lower = upper - _skillFromKarma;

			int cost = upper*(upper + 1);
			cost -= lower*(lower - 1);
			cost /= 2; //We get sqre, need triangle
			
			return cost * _character.Options.KarmaImproveSkillGroup; //todo handle KarmaNewSkillGrup 
		}

		public int UpgradeKarmaCost()
		{
			if (Rating == 0)
			{
				return Character.Options.KarmaNewSkillGroup;
			}
			else if (RatingMaximum > Rating)
			{
				return (Rating + 1)*Character.Options.KarmaImproveSkillGroup;
			}
			else
			{
				return -1;
			}
		}
		
		public IEnumerable<Skill> GetEnumerable() //Databinding shits itself if this implements IEnumerable
		{
			return _affectedSkills;
		}

		#endregion
	}
}
