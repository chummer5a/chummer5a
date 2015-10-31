using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Annotations;


namespace Chummer
{
	public class Skill : INotifyPropertyChanged
	{
		#region REMOVELATERANDPLACEINCHILD
		
		public bool Absorb(Skill s)
		{
			return false;
		}
		public void Free(Skill s) { }

		public ReadOnlyCollection<Skill> Fold
		{
			get { return null; }
		}

		#endregion

		#region REFACTORAWAY_NOTANYMORE_RENAME_MEANING
		
		//TODO STILL REMOVE THOSE
		public bool IdImprovement;
		public bool LockKnowledge;
		
		public void Save(XmlWriter xw) { }
		public void Print(XmlWriter xw) { } //Not this one, due grouping, interface?
		#endregion

		#region Factory

		/// <summary>
		/// Load a skill from a xml node from a saved .chum5 file
		/// </summary>
		/// <param name="n">The XML node describing the skill</param>
		/// <param name="character">The character this skill belongs to</param>
		/// <returns></returns>
		public static Skill Load(XmlNode n, Character character)
		{
			return new Skill(n, character);
		}



		protected static Dictionary<string, bool> SkillTypeCache = new Dictionary<string, bool>();  //TODO CACHE INVALIDATE

		/// <summary>
		/// Load a skill from a data file describing said skill
		/// </summary>
		/// <param name="n">The XML node describing the skill</param>
		/// <param name="character">The character the skill belongs to</param>
		/// <returns></returns>
		public static Skill FromData(XmlNode n, Character character)
		{
			Skill s;
			if (n["exotic"] != null && n["exotic"].InnerText == "Yes")
			{
				//load exotic skill
				//TODO FINISH THIS
				if (Debugger.IsAttached)
					Debugger.Break();

				ExoticSkill s2 = new ExoticSkill(character, n["name"].InnerText);

				s = s2;
			}
			else
			{

				string category = n["category"].InnerText;  //if missing we have bigger problems, and a nullref is probably prefered
				bool knoSkill;

				if (SkillTypeCache != null && SkillTypeCache.ContainsKey(category))
				{
					knoSkill = SkillTypeCache[category];  //Simple cache, no need to be sloppy
				}
				else
				{
					XmlDocument document = XmlManager.Instance.Load("skills.xml");
					XmlNode knoNode = document.SelectSingleNode($"/chummer/categories/category[. = '{category}'");
					knoSkill = knoNode.Attributes["type"].InnerText != "active";
					SkillTypeCache[category] = knoSkill;
				}


				if (knoSkill)
				{
					//TODO INIT SKILL
					if (Debugger.IsAttached) Debugger.Break();

					KnowledgeSkill s2 = new KnowledgeSkill(character);

					s = s2;
				}
				else
				{
					Skill s2 = new Skill(character, n);
					//TODO INIT SKILL

					s = s2;
				}
			}

			s.TypeId = Guid.Parse(n["id"].InnerText);

			return s;
		}

		public Skill(Character character)
		{
			Character = character;  //INIT FROM HERE?

			ObjId = Guid.NewGuid();

			if (Debugger.IsAttached)
				Debugger.Break();

		}

		//load from data
		public Skill(Character character, XmlNode n) : this(character)
		{
			_name = n["name"].InnerText; //No need to catch errors (for now), if missing we are fsked anyway
			_usedAttribute = CharacterObject.GetAttribute(n["attribute"].InnerText);
			_category = n["category"].InnerText;
			_group = n["group"].InnerText;
			_default = n["default"].InnerText.ToLower() == "yes";
			Source = n["source"].InnerText;
			Page = n["page"].InnerText;

		}

		//Load from .chum5
		private Skill(XmlNode n, Character character) : this(character)
		{

		}

		#endregion

		protected CharacterAttrib _usedAttribute;

		private Guid TypeId;
		private readonly Guid ObjId;
		protected Character Character;
		protected readonly bool _default;
		protected readonly string _category;
		protected readonly string _group;
		protected int _skillFromSp = 0;
		protected int _skillFromKarma = 0; //This is also used for career advances


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
					OnPropertyChanged(nameof(Rating));
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
				OnPropertyChanged(nameof(Rating));
			}
		}

		public int Rating
		{
			get { return Karma + Base; }
			set
			{
				int diff = (Karma + Base) - value;
				//Play with karma first, cause



			}
		}

		public int FreeLevels
		{
			get;
			set; //TODO REFACTOR AWAY
		}
		public int RatingMaximum
		{
			get {
				int otherbonus = 0; //TODO READ FROM IMPMANAGER
				return (Character.Created
					? 12
					: (this.KnowledgeSkill && Character.BuildMethod == CharacterBuildMethod.LifeModule ? 9 : 6)) + otherbonus;

			}
			
		}

		//TODO READ FROM IMPROVEMENT
		public int RatingModifiers
		{
			get
			{
				int intModifier = 0;
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
							if (!objImprovement.Exclude.Contains(Name) && !objImprovement.Exclude.Contains(_category))
								intModifier += objImprovement.Value;
						}
						// Improvement for a Skill Category.
						if (objImprovement.ImproveType == Improvement.ImprovementType.SkillCategory && objImprovement.ImprovedName == _category)
						{
							if (!objImprovement.Exclude.Contains(Name))
								intModifier += objImprovement.Value;
						}
						// Improvement for a Skill linked to an CharacterAttribute.
						if (objImprovement.ImproveType == Improvement.ImprovementType.SkillAttribute && objImprovement.ImprovedName == _usedAttribute.Abbrev)
						{
							if (!objImprovement.Exclude.Contains(Name))
								intModifier += objImprovement.Value;
						}
						// Improvement for Enhanced Articulation
						if (_category == "Physical Active" && (_usedAttribute.Abbrev == "BOD" || _usedAttribute.Abbrev == "AGI" || _usedAttribute.Abbrev == "REA" || _usedAttribute.Abbrev == "STR"))
						{
							if (objImprovement.ImproveType == Improvement.ImprovementType.EnhancedArticulation)
								intModifier += objImprovement.Value;
						}
					}
				}

				return intModifier;
			}
		}

		//TODO CALCULATE
		//TODO RENAME TO POOL
		/// <summary>
		/// The total, general pourpose dice pool for this skill
		/// </summary>
		public int TotalRating
		{
			get { return PoolOtherAttribute(_usedAttribute.Augmented); }
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
			return Rating + attribute + RatingModifiers;
		}

		public Character CharacterObject
		{
			get { return Character; }
		}

		public virtual string Attribute
		{
			get { return _usedAttribute.Abbrev; }
			set { } //TODO REFACTOR AWAY
		}

		//TODO OVERRIDE IN CHILD CLASS
		public virtual bool AllowDelete
		{
			get
			{
				return false;
			}
			set { } //TODO REFACTOR AWAY
		}

		public bool Default
		{
			get { return false; }  //TODO THIS
			set { } //TODO REFACTOR AWAY
		}

		public bool IsGrouped
		{
			get { return false; }
			set {} //TODO REFACTOR AWAY
		}

		//TODO OVERRIDE IN CHILD CLASS
		public virtual bool ExoticSkill
		{
			get { return false; }
			set { } //TODO REFACTOR AWAY
		}

		public virtual bool KnowledgeSkill
		{
			get { return false; }
			set { } //TODO REFACTOR AWAY
		}


		private string _name;

		public virtual string Name
		{
			get { return _name; }
			set
			{
				throw new InvalidOperationException("Name cannot be set on a standart skill");
				_name = value;
			}
		} //I

		//TODO RENAME DESCRIPTIVE
		/// <summary>
		/// The Unique ID for this skill. This is unique and never repeating
		/// </summary>
		public Guid Id
		{
			get { return ObjId; }
			set { } //TODO REFACTOR AWAY
		}

		/// <summary>
		/// The ID for this skill. This is persistent for active skills over 
		/// multiple characters, ?and predefined knowledge skills,? but not
		/// for skills where the user supplies a name (Exotic and Knowledge)
		/// </summary>
		public Guid SkillId
		{
			get { return TypeId; }
		}

		public string SkillGroup { get; set; } //TODO REFACTOR TO SKILLGROUPOBJ

		public string Specialization
		{
			get; set;
		}

		public string DicePoolModifiersTooltip { get; }
		
		//REFACTOR HOW?
		public bool BuyWithKarma
		{
			get; set;
            
		}

		public List<SkillSpecialization> Specializations;

		public SkillGroup SkillGroupObject { get; set; }  //TODO REWRITE/UNFUCK

		public string SkillCategory { get; set; } //TODO REFACTOR?

		public string Page
		{
			get;
			set;  //TODO REFACTOR AWAY
		}

		public string Source
		{
			get;
			set; //TODO REFACTOR AWAY
		}

		

		
		//Stuff that is RO, that is simply calculated from other things
		//Move to extension method?
		#region Calculations

		public int AttributeModifiers
		{
			get { return Character.GetAttribute(Attribute).TotalValue; }
		}

		#endregion

		#region Static
		private List<String> _knowledgeSkillCatagories;
		public List<String> KnowledgeSkillCatagories
		{
			//TODO RESHARPER COMPLAIN, TEST, NOT SURE I UNDERSTAND WORKINGS
			get { return _knowledgeSkillCatagories == null ? DefaultKnowledgeSkillCatagories : _knowledgeSkillCatagories; }
			set { _knowledgeSkillCatagories = value; }
		}

		private static List<String> _defaultKnowledgeSkillCatagories;  //TODO CACHE INVALIDATION
		public static List<String> DefaultKnowledgeSkillCatagories
		{
			get
			{
				if (_defaultKnowledgeSkillCatagories == null)
				{

					XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");
					XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill");
					_defaultKnowledgeSkillCatagories = new List<String>();
					foreach (XmlNode objXmlSkill in objXmlSkillList)
					{
						if (objXmlSkill["translate"] != null)
							_defaultKnowledgeSkillCatagories.Add(objXmlSkill["translate"].InnerText);
						else
							_defaultKnowledgeSkillCatagories.Add(objXmlSkill["name"].InnerText);
					}
				}
				return _defaultKnowledgeSkillCatagories;
			}
		}
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
