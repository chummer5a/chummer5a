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
		/// <summary>
		/// Called during save to allow derived classes to save additional infomation required to rebuild state
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void SaveExtendedData(XmlTextWriter writer) { }

		protected CharacterAttrib AttributeObject; //Attribute this skill primarily depends on
		private readonly Character _character; //The Character (parent) to this skill
		protected readonly string Category; //Name of the skill category it belongs to
		protected readonly string _group;  //Name of the skill group this skill belongs to (remove?)
		protected string _name;  //English name of this skill
		protected List<ListItem> _spec;  //List of suggested specializations for this skill

		#region REMOVELATERANDPLACEINCHILDORNEVER?

		public bool Absorb(Skill s)
		{
			return false;
		}
		public void Free(Skill s) { }

		public ReadOnlyCollection<Skill> Fold
		{
			get { return null; }
		}

		
		public bool IdImprovement;
		public bool LockKnowledge;

		#endregion

		#region REFACTORAWAY_NOTANYMORE_RENAME_MEANING



		public void Print(XmlWriter xw) { } //Not this one, due grouping, interface?
		#endregion

		#region Factory

		public void WriteTo(XmlTextWriter writer)
		{
			writer.WriteStartElement("skill");
			writer.WriteElementString("guid", Id.ToString());
			writer.WriteElementString("suid", SkillId.ToString());
			writer.WriteElementString("karma", _karma.ToString());
			writer.WriteElementString("base", _base.ToString());  //this could acctually be saved in karma too during career

			if (!CharacterObject.Created)
			{
				writer.WriteElementString("buywithkarma", _buyWithKarma.ToString());
			}

			if (Specializations.Count != 0)
			{
				writer.WriteStartElement("specs");
				foreach (SkillSpecialization specialization in Specializations)
				{
					specialization.Save(writer);
				}
				writer.WriteEndElement();
			}

			SaveExtendedData(writer);

			writer.WriteEndElement();

		}


		/// <summary>
		/// Load a skill from a xml node from a saved .chum5 file
		/// </summary>
		/// <param name="n">The XML node describing the skill</param>
		/// <param name="character">The character this skill belongs to</param>
		/// <returns></returns>
		public static Skill Load(Character character, XmlNode n)
		{
			if (n["suid"] == null) return null;


			Guid suid;
			if (!Guid.TryParse(n["suid"].InnerText, out suid))
			{
				return null;
			}
			Skill skill;
			if (suid != Guid.Empty)
			{
				XmlDocument skills = XmlManager.Instance.Load("skills.xml");
				XmlNode node = skills.SelectSingleNode($"/chummer/skills/skill[id = '{n["suid"].InnerText}']");

				if (node == null) return null;

				skill = node["exotic"]?.InnerText == "Yes" ? new ExoticSkill(character, node) : new Skill(character, node);
			}
			else  //This is ugly but i'm not sure how to make it pretty
			{
				KnowledgeSkill knoSkill = new KnowledgeSkill(character);
				knoSkill.Load(n);

				skill = knoSkill;

			}

			XmlElement element = n["guid"];
			if (element != null) skill.Id = Guid.Parse(element.InnerText);

			n.TryGetField("karma", out skill._karma);
			n.TryGetField("base", out skill._base);
			n.TryGetField("buywithkarma", out skill._buyWithKarma);

			foreach (XmlNode spec in n.SelectNodes("specs/spec"))
			{
				skill.Specializations.Add(SkillSpecialization.Load(spec));
			}

			return skill;
		}

		/// <summary>
		/// Loads skill saved in legacy format
		/// </summary>
		/// <param name="character"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Skill LegacyLoad(Character character, XmlNode n)
		{
			Guid suid;
			if (!n.TryGetField("id", Guid.TryParse, out suid))
				return null;

			if (n.TryCheckValue("knowledge", "True"))
			{
				return null;
			}
			else
			{
				XmlNode data = XmlManager.Instance.Load("skills.xml").SelectSingleNode($"/chummer/skills/skill[id = '{n["id"].InnerText}']");

				//Some stuff apparently have a guid of 0000-000... (only exotic?)
				if (data == null)
				{
					data = XmlManager.Instance.Load("skills.xml")
						.SelectSingleNode($"/chummer/skills/skill[name = '{n["name"].InnerText}']");
				}


				Skill skill = Skill.FromData(data, character);

				n.TryGetField("base", out skill._base);
				n.TryGetField("karma", out skill._karma);

				skill._buyWithKarma = n.TryCheckValue("buywithkarma", "True");

				//TODO Specs

				return skill;
			}
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
			_character = character;
			_group = group;
			
			_character.PropertyChanged += OnCharacterChanged;
			SkillGroupObject = Skills.SkillGroup.Get(this);
			if (SkillGroupObject != null)
			{
				SkillGroupObject.PropertyChanged += OnSkillGroupChanged;
			}

			ImprovementEvent += OnImprovementEvent;
		}

		

		[Obsolete]
		public Skill(Character character)
		{
			//TODO REMOVE, keept because LOTS of places require this
			//Refactor still underway
			_character = character;  //INIT FROM HERE?
			
			Id = Guid.NewGuid();

			if (Debugger.IsAttached)
				Debugger.Break();

		}

		//load from data
		protected Skill(Character character, XmlNode n) : this(character, n["skillgroup"].InnerText) //Ugly hack, needs by then
		{
			_name = n["name"].InnerText; //No need to catch errors (for now), if missing we are fsked anyway
			AttributeObject = CharacterObject.GetAttribute(n["attribute"].InnerText);
			Category = n["category"].InnerText;
			Default = n["default"].InnerText.ToLower() == "yes";
			Source = n["source"].InnerText;
			Page = n["page"].InnerText;
			SkillId = Guid.Parse(n["id"].InnerText);
			AttributeObject.PropertyChanged += OnLinkedAttributeChanged;

			_spec = new List<ListItem>();
			foreach (XmlNode node in n["specs"].ChildNodes)
			{
				_spec.Add(ListItem.AutoXml(node.InnerText, node));
			}
		}

		#endregion

		/// <summary>
		/// The total, general pourpose dice pool for this skill
		/// </summary>
		public int Pool
		{
			get { return PoolOtherAttribute(AttributeObject.Augmented); }
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
		public string Attribute
		{
			get { return AttributeObject.Abbrev; }
		}


		private bool _oldEnable = true; //For OnPropertyChanged 
		
		//TODO handle aspected/adepts who cannot (always) get magic skills
		public bool Enabled  
		{
			get { return AttributeObject.Value != 0; }
		}

		private bool _oldUpgrade = false;
		public bool CanUpgradeCareer
		{
			get { return CharacterObject.Karma >= UpgradeKarmaCost(); }
		}

		public virtual bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		public bool Default { get; private set; }

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
		} //I

		//TODO RENAME DESCRIPTIVE
		/// <summary>
		/// The Unique ID for this skill. This is unique and never repeating
		/// </summary>
		public Guid Id { get; private set; } = Guid.NewGuid();

		/// <summary>
		/// The ID for this skill. This is persistent for active skills over 
		/// multiple characters, ?and predefined knowledge skills,? but not
		/// for skills where the user supplies a name (Exotic and Knowledge)
		/// </summary>
		public Guid SkillId { get; private set; } = Guid.Empty;

		public string SkillGroup { get { return _group; } }

		public string SkillCategory
		{
			get { return Category; }
			set
			{
				if (Debugger.IsAttached && false) //TODO REMOVE AGAIN
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
				if (Rating == 0) return ""; //Unleveled skills cannot have a specialization;

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
						OnPropertyChanged();
						KarmaSpecForcedMightChange();
					}
					else if (Specializations[0].Name != value)
					{
						Specializations[0] = new SkillSpecialization(value, true);
						OnPropertyChanged();
						KarmaSpecForcedMightChange();
					}
				}
				else
				{
					if (!String.IsNullOrWhiteSpace(value))
					{
						Specializations.Add(new SkillSpecialization(value, true));
						OnPropertyChanged();
						KarmaSpecForcedMightChange();
					}
				}

			}
		}

		public string DicePoolModifiersTooltip { get; }
		
		public SkillGroup SkillGroupObject { get; }

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
				var v = new PropertyChangedEventArgs(s);
				PropertyChanged?.Invoke(this, v);
			}
		}

		private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs propertyChangedEventArg)
		{
			if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Base))
			{
				OnPropertyChanged(propertyChangedEventArg.PropertyName);
				KarmaSpecForcedMightChange();
			}
			else if(propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Karma))
			{
				OnPropertyChanged(propertyChangedEventArg.PropertyName);
				KarmaSpecForcedMightChange();
			}
		}

		private void OnCharacterChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == nameof(Character.Karma))
			{
				if (_oldUpgrade != CanUpgradeCareer)
				{
					_oldUpgrade = CanUpgradeCareer;
					OnPropertyChanged(nameof(CanUpgradeCareer));
				}

			}
		}

		private void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			OnPropertyChanged(nameof(AttributeModifiers));
			if (Enabled != _oldEnable)
			{
				OnPropertyChanged(nameof(Enabled));
				_oldEnable = Enabled;
			}

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
