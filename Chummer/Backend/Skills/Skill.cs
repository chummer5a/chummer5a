using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Annotations;
using Chummer.Datastructures;

/*
 * When comming back:
 * Karma can exceed ratingMaximum
 * 
 * Base = Group + FreeLevels
 * Should be
 * Base = Group > FreeLevels ? Group : FreeLevels 
 */


namespace Chummer.Skills
{
	[DebuggerDisplay("{_name} {_base} {_karma}")]
	public partial class Skill : INotifyPropertyChanged
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
			if (Debugger.IsAttached)
				Debugger.Break();

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

				ExoticSkill s2 = new ExoticSkill(character, n);

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
					XmlNode knoNode = document.SelectSingleNode($"/chummer/categories/category[. = '{category}']");
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

			
			return s;
		}

		protected Skill(Character character, string group)
		{
			//DEBUGGING .CTOR, remove before using
			_character = character;  //INIT FROM HERE?
			_objId = Guid.NewGuid();
			_group = group;
			
			_skillGroup = Skills.SkillGroup.Get(this);
			if (_skillGroup != null)
			{
				_skillGroup.PropertyChanged += OnSkillGroupChanged;
					
					//+= OnSkillGroupChanged();
			}

			ImprovementEvent += OnImprovementEvent;
		}

		[Obsolete]
		public Skill(Character character)
		{
			//TODO REMOVE, keept because LOTS of places require this
			//Refactor still underway
			_character = character;  //INIT FROM HERE?
			
			_objId = Guid.NewGuid();

			if (Debugger.IsAttached)
				Debugger.Break();

		}

		//load from data
		protected Skill(Character character, XmlNode n) : this(character, n["skillgroup"].InnerText) //Ugly hack, needs by then
		{
			_name = n["name"].InnerText; //No need to catch errors (for now), if missing we are fsked anyway
			UsedAttribute = CharacterObject.GetAttribute(n["attribute"].InnerText);
			Category = n["category"].InnerText;
			_default = n["default"].InnerText.ToLower() == "yes";
			Source = n["source"].InnerText;
			Page = n["page"].InnerText;

			UsedAttribute.PropertyChanged += OnLinkedAttributeChanged;

			_spec = new List<ListItem>();
			foreach (XmlNode node in n["specs"].ChildNodes)
			{
				_spec.Add(ListItem.AutoXml(node.InnerText, node));
			}
		}

		//Load from .chum5
		private Skill(XmlNode n, Character character) : this(character, "FIX LATER")
		{

		}

		#endregion

		private readonly SkillGroup _skillGroup;
		protected bool _skillGroupLinked = true; //Auto broken and 'kind' of true
 		protected readonly CharacterAttrib UsedAttribute;
		private readonly Guid _objId;
		private readonly Character _character;
		protected readonly bool _default;
		protected readonly string Category;
		protected readonly string _group;
		protected int _skillFromSp = 0;
		protected int _skillFromKarma = 0; //This is also used for career advances
		protected string _name;
		protected List<ListItem> _spec;
		
		
		/// <summary>
		/// The total, general pourpose dice pool for this skill
		/// </summary>
		public int Pool
		{
			get { return PoolOtherAttribute(UsedAttribute.Augmented); }
		}

		public bool Leveled
		{
			get { return Rating > 0; }
		}

		
		public Character CharacterObject
		{
			get { return _character; }
		}

		//TODO change to the acctual characterattribute object
		/// <summary>
		/// The Abbreviation of the linke attribute. This way due legacy
		/// </summary>
		public virtual string Attribute
		{
			get { return UsedAttribute.Abbrev; }
			set
			{
				if (Debugger.IsAttached)
					Debugger.Break();
			} //TODO REFACTOR AWAY
		}

		//TODO OVERRIDE IN CHILD CLASS
		public virtual bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		public bool Default
		{
			get { return _default; }
			set { } //TODO REFACTOR AWAY
		}

		public bool IsGrouped
		{
			get { return _skillGroupLinked && _skillGroup != null; }
			set
			{
				_skillGroupLinked = value;
				if (_skillGroupLinked && _skillGroup != null)
				{
					_skillFromKarma = SkillGroupObject.Karma;
					_skillFromSp = SkillGroupObject.Base;
				}
				OnPropertyChanged();
			}
		}

		public bool IsUnlocked
		{
			get { return !(_skillGroup != null && IsGrouped && (SkillGroupObject.Karma + SkillGroupObject.Base) > 0); }
		}
		
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
			get { return _objId; }
			set
			{
				if (Debugger.IsAttached)
					Debugger.Break();
			} //TODO REFACTOR AWAY
		}

		/// <summary>
		/// The ID for this skill. This is persistent for active skills over 
		/// multiple characters, ?and predefined knowledge skills,? but not
		/// for skills where the user supplies a name (Exotic and Knowledge)
		/// </summary>
		public Guid SkillId { get; }

		public string SkillGroup { get { return _group; } }

		public string SkillCategory
		{
			get { return Category; }
			set
			{
				if (Debugger.IsAttached)
					Debugger.Break();
			}
		} //TODO REFACTOR?

		public IReadOnlyList<ListItem> CGLSpecializations { get { return _spec; } } 

		//TODO A unit test here?, I know we don't have them, but this would be improved by some
		//Or just ignore support for multiple specizalizations even if the rules say it is possible?
		public List<SkillSpecialization> Specializations { get; } = new List<SkillSpecialization>();
		public string Specialization
		{
			get
			{
				Specializations.Sort((x, y) => x.Free == y.Free ? 0 : (x.Free ? -1 : 1));
				if (Specializations.Count > 0)
				{
					return Specializations[0].Name;
				}
				return null;
			}
			set
			{
				Specializations.Sort((x, y) => x.Free == y.Free ? 0 : (x.Free ? -1 : 1));
				
				if (Specializations.Count >= 1 && Specializations[0].Free)
				{
					if (string.IsNullOrWhiteSpace(value))
					{
						Specializations.RemoveAt(0);
					}
					else if (Specializations[0].Name != value)
					{
						Specializations[0] = new SkillSpecialization(value, true);
					}
				}
				else
				{
					if (!String.IsNullOrWhiteSpace(value))
					{
						Specializations.Add(new SkillSpecialization(value, true));
					}
				}

			}
		}

		public string DicePoolModifiersTooltip { get; }
		
		

		public SkillGroup SkillGroupObject { get { return _skillGroup; } } 
		
		public string Page { get; private set; }

		public string Source { get; private set; }
		
		//Stuff that is RO, that is simply calculated from other things
		//Move to extension method?
		#region Calculations

		public int AttributeModifiers
		{
			get { return _character.GetAttribute(Attribute).TotalValue; }  //TODO: Totalhere, argumented elsewhere, investigate
		}

		//TODO: Add translation support
		public string DisplayName
		{
			get { return Name; }
		}

		public string DisplayPool
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Specialization))
				{
					return Pool.ToString();
				}
				else
				{
					return $"{Pool} ({Pool + 2})"; //TODO: Artisian handle
				}
			}

		}

		#endregion

		#region Static

		//A tree of dependencies. Once some of the properties are changed, 
		//anything they depend on, also needs to raise OnChanged
		//This tree keeps track of dependencies
		private static readonly ReverseTree<string> DependencyTree =
		
		new ReverseTree<string>(nameof(DisplayPool),
			new ReverseTree<string>(nameof(Pool),
				new ReverseTree<string>(nameof(PoolModifiers)),
				new ReverseTree<string>(nameof(AttributeModifiers)),
				new ReverseTree<string>(nameof(Rating),
					new ReverseTree<string>(nameof(Karma)),
					new ReverseTree<string>(nameof(BaseUnlocked),
						new ReverseTree<string>(nameof(Base))
					)
				)
			)
		);
		
		
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			foreach (string s in DependencyTree.Find(propertyName))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
			}
		}

		private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs propertyChangedEventArg)
		{
			if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Base))
			{
				OnPropertyChanged(propertyChangedEventArg.PropertyName);
			}
			else if(propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Karma))
			{
				OnPropertyChanged(propertyChangedEventArg.PropertyName);
			}
		}

		private void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			OnPropertyChanged(nameof(AttributeModifiers));
		}

		[Obsolete("Refactor this method away once improvementmanager gets outbound events")]
		private void OnImprovementEvent(List<Improvement> improvements, ImprovementManager improvementManager)
		{
			if(improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.SkillLevel 
			&& imp.ImprovedName == _name))
				OnPropertyChanged(nameof(PoolModifiers));
		}
		//I also think this prevents GC. But there is no good way to do it...
		private static event Action<List<Improvement>, ImprovementManager> ImprovementEvent;
		//To get when things change in improvementmanager
		//Ugly, ugly done, but we cannot get events out of it today
		// FUTURE REFACTOR HERE
		[Obsolete("Refactor this method away once improvementmanager gets outbound events")]
		internal static void ImprovementHook(List<Improvement> _lstTransaction, ImprovementManager improvementManager)
		{
			ImprovementEvent?.Invoke(_lstTransaction, improvementManager);
		}
	}
}
