using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend;
using Chummer.Backend.Equipment;
using Chummer.Datastructures;

namespace Chummer.Skills
{
	[DebuggerDisplay("{_name} {_base} {_karma}")]
	public partial class Skill : INotifyPropertyChanged
	{
		/// <summary>
		/// Called during save to allow derived classes to save additional infomation required to rebuild state
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void SaveExtendedData(XmlTextWriter writer)
		{
		}

		public CharacterAttrib AttributeObject { get; protected set; } //Attribute this skill primarily depends on
		private readonly Character _character; //The Character (parent) to this skill
		protected readonly string Category; //Name of the skill category it belongs to
		protected readonly string _group; //Name of the skill group this skill belongs to (remove?)
		protected string _name; //English name of this skill
		private string _strNotes; //Text of any notes that were entered by the user
		protected List<ListItem> SuggestedSpecializations; //List of suggested specializations for this skill
		private readonly string _translatedName = null;
		private string _translatedCategory = null;



		public void WriteTo(XmlTextWriter writer)
		{
			writer.WriteStartElement("skill");
			writer.WriteElementString("guid", Id.ToString());
			writer.WriteElementString("suid", SkillId.ToString());
			writer.WriteElementString("karma", _karma.ToString());
			writer.WriteElementString("base", _base.ToString()); //this could acctually be saved in karma too during career
			writer.WriteElementString("notes", _strNotes);
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

		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("skill");

			int rating = PoolOtherAttribute(AttributeObject.TotalValue);
			int specRating = Specializations.Count == 0
				? rating
				: (!IsKnowledgeSkill && Name == "Artisan" &&
				   CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired")
					? rating + 3
					: rating + 2);

			int ratingModifiers = RatingModifiers, dicePoolModifiers = PoolModifiers;

			objWriter.WriteElementString("name", DisplayName);
			objWriter.WriteElementString("skillgroup", SkillGroupObject?.DisplayName ?? LanguageManager.Instance.GetString("String_None"));
			objWriter.WriteElementString("skillgroup_english", SkillGroupObject?.Name ?? LanguageManager.Instance.GetString("String_None"));
			objWriter.WriteElementString("skillcategory", DisplayCategory);
			objWriter.WriteElementString("skillcategory_english", SkillCategory);  //Might exist legacy but not existing atm, will see if stuff breaks
			objWriter.WriteElementString("grouped", (SkillGroupObject?.CareerIncrease).ToString());
			objWriter.WriteElementString("default", Default.ToString());
			objWriter.WriteElementString("rating", Rating.ToString());
			objWriter.WriteElementString("ratingmax", RatingMaximum.ToString());
			objWriter.WriteElementString("specializedrating",specRating.ToString());
			objWriter.WriteElementString("total", PoolOtherAttribute(AttributeObject.TotalValue).ToString());
			objWriter.WriteElementString("knowledge", IsKnowledgeSkill.ToString());
			objWriter.WriteElementString("exotic", IsExoticSkill.ToString());
			objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString());
			objWriter.WriteElementString("base", Base.ToString());
			objWriter.WriteElementString("karma", Karma.ToString());
			objWriter.WriteElementString("spec", Specialization);
			objWriter.WriteElementString("attribute", Attribute);
			objWriter.WriteElementString("displayattribute", DisplayAttribute);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("source", CharacterObject.Options.LanguageBookShort(Source));
			objWriter.WriteElementString("page", Page);
			if (Attribute == "MAG" && CharacterObject.AdeptEnabled && CharacterObject.MagicianEnabled)
				objWriter.WriteElementString("attributemod", CharacterObject.MAGMagician.ToString());
			else
				objWriter.WriteElementString("attributemod", CharacterObject.GetAttribute(Attribute).TotalValue.ToString());
			objWriter.WriteElementString("ratingmod", (ratingModifiers + dicePoolModifiers).ToString());
			objWriter.WriteElementString("poolmod", dicePoolModifiers.ToString());
			objWriter.WriteElementString("islanguage", (SkillCategory == "Language").ToString());
			objWriter.WriteElementString("bp", CurrentKarmaCost().ToString());
			objWriter.WriteStartElement("skillspecializations");
			foreach (SkillSpecialization objSpec in Specializations)
			{
				objSpec.Print(objWriter);
			}
			objWriter.WriteEndElement();

			objWriter.WriteEndElement();
		}
		
		#region Factory
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
			XmlDocument skills = XmlManager.Instance.Load("skills.xml");
			if (!Guid.TryParse(n["suid"].InnerText, out suid))
			{
				return null;
			}
			Skill skill;
			if (suid != Guid.Empty)
			{
				XmlNode node = skills.SelectSingleNode($"/chummer/skills/skill[id = '{n["suid"].InnerText}']");

				if (node == null) return null;

				if (node["exotic"]?.InnerText == "Yes")
				{
					ExoticSkill exotic = new ExoticSkill(character, node);
					exotic.Load(n);
					skill = exotic;
				}
				else
				{
					skill = new Skill(character, node);
				}
			}
			else //This is ugly but i'm not sure how to make it pretty
			{
				if (n["forced"] != null)
				{
					skill = new KnowledgeSkill(character, n["name"].InnerText);
				}
				else
				{
					KnowledgeSkill knoSkill = new KnowledgeSkill(character);
					knoSkill.Load(n);
					skill = knoSkill;
				}
			}

			XmlElement element = n["guid"];
			if (element != null) skill.Id = Guid.Parse(element.InnerText);

			n.TryGetField("karma", out skill._karma);
			n.TryGetField("base", out skill._base);
			n.TryGetField("buywithkarma", out skill._buyWithKarma);
			n.TryGetField("notes", out skill._strNotes);

			foreach (XmlNode spec in n.SelectNodes("specs/spec"))
			{
				skill.Specializations.Add(SkillSpecialization.Load(spec));
			}
			XmlNode objCategoryNode = skills.SelectSingleNode($"/chummer/categories/category[. = '{skill.SkillCategory}']");

			if (objCategoryNode != null && objCategoryNode.Attributes?["translate"] != null)
			{
				skill.DisplayCategory = objCategoryNode.Attributes["translate"].InnerText;
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
			Skill skill;

			n.TryGetField("id", Guid.TryParse, out suid, Guid.NewGuid());

			int baseRating = int.Parse(n["base"].InnerText);
			int fullRating = int.Parse(n["rating"].InnerText);
			int karmaRating = fullRating - baseRating;  //Not reading karma directly as career only increases rating

			if (n.TryCheckValue("knowledge", "True"))
			{
				Skills.KnowledgeSkill kno = new KnowledgeSkill(character);
				kno.WriteableName = n["name"].InnerText;
				kno.Base = baseRating;
				kno.Karma = karmaRating;

				kno.Type = n["skillcategory"].InnerText;

				skill = kno;
			}
			else
			{
				XmlNode data =
					XmlManager.Instance.Load("skills.xml").SelectSingleNode($"/chummer/skills/skill[id = '{suid}']");

				//Some stuff apparently have a guid of 0000-000... (only exotic?)
				if (data == null)
				{
					data = XmlManager.Instance.Load("skills.xml")
						.SelectSingleNode($"/chummer/skills/skill[name = '{n["name"].InnerText}']");
				}


				skill = Skill.FromData(data, character);
				skill._base = baseRating;
				skill._karma = karmaRating;

				ExoticSkill exoticSkill = skill as ExoticSkill;
				if (exoticSkill != null)
				{
					string name = n.SelectSingleNode("skillspecializations/skillspecialization/name")?.InnerText ?? "";
					//don't need to do more load then.
					
					exoticSkill.Specific = name;
					return skill;
				}

				skill._buyWithKarma = n.TryCheckValue("buywithkarma", "True");
			}

			var v = from XmlNode node
				in n.SelectNodes("skillspecializations/skillspecialization")
				select SkillSpecialization.Load(node);
			var q = v.ToList();
			if (q.Count != 0)
			{
				skill.Specializations.AddRange(q);
			}

			return skill;
		}

		protected static readonly Dictionary<string, bool> SkillTypeCache = new Dictionary<string, bool>();
			//TODO CACHE INVALIDATE

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
				ExoticSkill s2 = new ExoticSkill(character, n);
				s = s2;
			}
			else
			{
				XmlDocument document = XmlManager.Instance.Load("skills.xml");
				XmlNode knoNode = null;
				string category = n["category"].InnerText; //if missing we have bigger problems, and a nullref is probably prefered
				bool knoSkill;

				if (SkillTypeCache != null && SkillTypeCache.ContainsKey(category))
				{
					knoSkill = SkillTypeCache[category]; //Simple cache, no need to be sloppy
				}
				else
				{
					knoNode = document.SelectSingleNode($"/chummer/categories/category[. = '{category}']");
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
				if (knoNode != null)
				{
					if (knoNode.Attributes["translate"] != null)
					{
						s.DisplayCategory = knoNode.Attributes["translate"].InnerText;
					}
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

			character.ImprovementEvent += OnImprovementEvent;
			Specializations.ListChanged += SpecializationsOnListChanged;
		}


		//load from data
		protected Skill(Character character, XmlNode n) : this(character, n["skillgroup"].InnerText)
			//Ugly hack, needs by then
		{
			_name = n["name"].InnerText; //No need to catch errors (for now), if missing we are fsked anyway
			AttributeObject = CharacterObject.GetAttribute(n["attribute"].InnerText);
			Category = n["category"].InnerText;
			Default = n["default"].InnerText.ToLower() == "yes";
			Source = n["source"].InnerText;
			Page = n["page"].InnerText;
			SkillId = Guid.Parse(n["id"].InnerText);

			_translatedName = n["translate"]?.InnerText;

			AttributeObject.PropertyChanged += OnLinkedAttributeChanged;

			SuggestedSpecializations = new List<ListItem>();
			foreach (XmlNode node in n["specs"].ChildNodes)
			{
				SuggestedSpecializations.Add(ListItem.AutoXml(node.InnerText, node));
			}
		}

		#endregion

		/// <summary>
		/// The total, general pourpose dice pool for this skill
		/// </summary>
		public int Pool
		{
			get { return PoolOtherAttribute(AttributeObject.TotalValue); }
		}

		public bool Leveled
		{
			get { return Rating > 0; }
		}

		public bool CanHaveSpecs => Leveled && KarmaUnlocked;

		public Character CharacterObject
		{
			get { return _character; }
		}

		//TODO change to the acctual characterattribute object
		/// <summary>
		/// The Abbreviation of the linke attribute. Not the object due legacy
		/// </summary>
		public string Attribute
		{
			get
			{
					return AttributeObject.Abbrev;
			}
		}
		/// <summary>
		/// The translated abbreviation of the linked attribute.
		/// </summary>
		public string DisplayAttribute
		{
			get
			{
				if (GlobalOptions.Instance.Language != "en-us")
				{
					return LanguageManager.Instance.GetString($"String_Attribute{AttributeObject.Abbrev}Short");
				}
				else
				{
					return AttributeObject.Abbrev;
				}
			}
		}

		private bool _oldEnable = true; //For OnPropertyChanged 

		//TODO handle aspected/adepts who cannot (always) get magic skills
		public bool Enabled
		{
			get
			{
				//TODO: This is a temporary workaround until proper support for selectively enabling or disabling skills works, as above.
				if (CharacterObject.Metatype == "A.I.")
				{
					return !(AttributeObject.Abbrev == "MAG" || AttributeObject.Abbrev == "RES");
				}
				else
				{
					return AttributeObject.Value != 0;
				}
			}
		}

		private bool _oldUpgrade = false;

		public bool CanUpgradeCareer
		{
			get { return CharacterObject.Karma >= UpgradeKarmaCost() && RatingMaximum > LearnedRating; }
		}

		public virtual bool AllowDelete
		{
			get { return false; }
		}

		public bool Default { get; private set; }

		public virtual bool IsExoticSkill
		{
			get { return false; }
		}

		public virtual bool IsKnowledgeSkill
		{
			get { return false; }
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

		public string SkillGroup
		{
			get { return _group; }
		}

		public virtual string SkillCategory
		{
			get { return Category; }
		}

		public IReadOnlyList<ListItem> CGLSpecializations
		{
			get { return SuggestedSpecializations; }
		}

		private string _cachedStringSpec = null;
		public virtual string DisplaySpecialization
		{
			get { return _cachedStringSpec = _cachedStringSpec ?? string.Join(", ", Specializations.Select(x => x.Name)); }
		}

		//TODO A unit test here?, I know we don't have them, but this would be improved by some
		//Or just ignore support for multiple specizalizations even if the rules say it is possible?
		public BindingList<SkillSpecialization> Specializations { get; } = new BindingList<SkillSpecialization>();

		public string Specialization
		{
			get
			{
				if (LearnedRating == 0)
				{
					return ""; //Unleveled skills cannot have a specialization;
				}

				if (Specializations.Count > 0)
				{
					return Specializations[0].Name;
				}

				return "";
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					int index = -1;
					for (int i = 0; i < Specializations.Count; i++)
					{
						if (Specializations[i].Free) continue;
						index = i;
						break;
					}

					if(index >= 0) Specializations.RemoveAt(index);
				}
				else if (Specializations.Count == 0 && !string.IsNullOrWhiteSpace(value))
				{
					Specializations.Add(new SkillSpecialization(value, false));
				}
				else
				{
					if (Specializations[0].Free)
					{
						Specializations.MergeInto(new SkillSpecialization(value, false), (x, y) => x.Free == y.Free ? 0 : (x.Free ? 1 : -1));
					}
					else
					{
						Specializations[0] = new SkillSpecialization(value, false);
					}
				}
			}
		}

		public string PoolToolTip
		{
			get
			{
				if (!Default && !Leveled)
				{
					return "You cannot default in this skill";
						//TODO translate (could not find it in lang file, did not check old source)
				}

				StringBuilder s;
				if (CyberwareRating() > LearnedRating)
				{
					s = new StringBuilder($"{LanguageManager.Instance.GetString("Tip_Skill_SkillsoftRating")} ({CyberwareRating()})");
				}
				else
				{
					s = new StringBuilder($"{LanguageManager.Instance.GetString("Tip_Skill_SkillRating")} ({Rating}");


					bool first = true;
					foreach (Improvement source in RelevantImprovements().Where(x => x.AddToRating))
					{
						if (first)
						{
							first = false;

							s.Append(" (Base (");
							s.Append(LearnedRating);
							s.Append(")");
						}

						s.Append(" + ");
						s.Append(GetName(source));
						s.Append(" (");
						s.Append(source.Value);
						s.Append(")");
					}
					if (!first) s.Append(")");

					s.Append(")");
				}

				s.Append($" + {Attribute} ({AttributeModifiers})");

				if (Default && !Leveled)
				{
					if (DefaultModifier == 0)
					{
						s.Append(" Reflex Recorder Optimization ");
					}
					else
					{
						s.Append($" - {LanguageManager.Instance.GetString("Tip_Skill_Defaulting")} (1)");
					}

				}

				foreach (Improvement source in RelevantImprovements().Where(x => !x.AddToRating))
				{
					s.Append(" + ");
					s.Append(GetName(source));
					s.Append(" (");
					s.Append(source.Value);
					s.Append(")");
				}


				int wound = WoundModifier;
				if (wound != 0)
				{
					s.Append(" - " + LanguageManager.Instance.GetString("Tip_Skill_Wounds") + " (" + wound + ")");
				}

				if (AttributeObject.Abbrev == "STR" || AttributeObject.Abbrev == "AGI")
				{
					foreach (Cyberware cyberware in _character.Cyberware.Where(x => x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
					{
						s.Append("\n");
						s.AppendFormat("{0} {1} ", cyberware.Location, cyberware.DisplayNameShort);
						if (cyberware.Grade != GlobalOptions.CyberwareGrades.GetGrade("Standard"))
						{
							s.AppendFormat("({0}) ", cyberware.Grade.DisplayName);
						}

						int pool = PoolOtherAttribute(Attribute == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility);

						if (cyberware.Location == CharacterObject.PrimaryArm)
						{
							s.Append(pool);
						}
						else
						{
							s.AppendFormat("{0} (-2 Off Hand)", pool - 2);
						}
					}
				}

				return s.ToString();
			}
		}

		private string GetName(Improvement source)
		{
			string value = null;
			switch (source.ImproveSource)
			{
				case Improvement.ImprovementSource.Bioware:
				{
					Cyberware q = _character.Cyberware.FirstOrDefault(x => x.InternalId == source.SourceName);
					value = q?.DisplayNameShort;
				}
					break;
				case Improvement.ImprovementSource.Quality:
				{
					Quality q = _character.Qualities.FirstOrDefault(x => x.InternalId == source.SourceName);
					value = q?.DisplayName;
				}
					break;
				case Improvement.ImprovementSource.Power:
				{
					Power power = _character.Powers.FirstOrDefault(x => x.InternalId == source.SourceName);
					value = power?.DisplayNameShort;
				}
					break;
				case Improvement.ImprovementSource.Custom:
				{
					return source.CustomName;
				}

				case Improvement.ImprovementSource.Gear:
				{
					value = _character.Gear.FirstOrDefault(x => x.InternalId == source.SourceName)?.DisplayName;
				}
					break;
				default:
					return source.SourceName;
			}

			if (value == null)
			{
				Log.Warning(new object[]{"Skill Tooltip GetName value = null", source.SourceName, source.ImproveSource, source.ImproveType, source.ImprovedName});
			}

			return value ?? source.ImproveSource + " source not found";

		}

		public string UpgradeToolTip
		{
			get
			{
				return string.Format(LanguageManager.Instance.GetString("Tip_ImproveItem"), (Rating + 1), UpgradeKarmaCost());
				;
			}
		}

		public string AddSpecToolTip
		{
			get
			{
				return string.Format(LanguageManager.Instance.GetString("Tip_Skill_AddSpecialization"),
					CharacterObject.Options.KarmaSpecialization);
			}
		}

		public string SkillToolTip
		{
			get
			{
				//v-- hack i guess
				string strReturn = "";
				string middle = "";
				if (!string.IsNullOrWhiteSpace(SkillGroup))
				{
					middle = $"{SkillGroup} {LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}\n";
				}
				if (!String.IsNullOrEmpty(_strNotes))
				{
					_strNotes = CommonFunctions.WordWrap(_strNotes, 100);
					strReturn = LanguageManager.Instance.GetString("Label_Notes") + " " +_strNotes + "\n\n";
				}

				strReturn += $"{this.GetDisplayCategory()}\n{middle}{CharacterObject.Options.LanguageBookLong(Source)} {LanguageManager.Instance.GetString("String_Page")} {Page}";

				return strReturn;
					
			}
		}

		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		public SkillGroup SkillGroupObject { get; }

		public string Page { get; private set; }

		public string Source { get; private set; }

		//Stuff that is RO, that is simply calculated from other things
		//Move to extension method?

		#region Calculations

		public int AttributeModifiers
		{
			get { return AttributeObject.TotalValue; }
		}

		//TODO: Add translation support
		public string DisplayName
		{
			get { return _translatedName ?? Name; }
		}

		public string DisplayCategory
		{
			get { return _translatedCategory ?? Category; }
			set { _translatedCategory = value; }
		}
		public virtual string DisplayPool
		{
			get {
				return DisplayOtherAttribue(AttributeObject.TotalValue);
			}
		}

		public string DisplayOtherAttribue(int attributeValue)
		{
			int pool = PoolOtherAttribute(attributeValue);

			if (string.IsNullOrWhiteSpace(Specialization))
			{
				return pool.ToString();
			}
			else
			{
				//Handler for the Inspired Quality. 
				if (!IsKnowledgeSkill && Name == "Artisan")
				{
					if (CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired"))
					{
						return $"{pool} ({pool + 3})";
					}
				}
				else if (IsExoticSkill)
				{
					return $"{pool}";
				}
				return $"{pool} ({pool + 2})";
			}
		}

		protected int _cachedWareRating = int.MinValue;
		/// <summary>
		/// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
		/// </summary>
		/// <returns>Artificial skill attributeValue</returns>
		public virtual int CyberwareRating()
		{

			if (_cachedWareRating != int.MinValue) return _cachedWareRating;

			//TODO: method is here, but not used in any form, needs testing (worried about child items...)
			//this might do hardwires if i understand how they works correctly
			var hardwire = CharacterObject.Improvements.Where(
				improvement =>
					improvement.ImproveType == Improvement.ImprovementType.Hardwire && improvement.ImprovedName == Name &&
					improvement.Enabled).ToList();

			if (hardwire.Any())
			{
				return _cachedWareRating = hardwire.Max(x => x.Value);
			}


			ImprovementManager manager = new ImprovementManager(CharacterObject);

			int skillWireRating = manager.ValueOf(Improvement.ImprovementType.Skillwire);
			if ((skillWireRating > 0 || IsKnowledgeSkill) && CharacterObject.SkillsoftAccess)
			{
				Func<Gear, int> recusivestuff = null;
				recusivestuff = (gear) =>
				{
					//TODO this works with translate?
					if (gear.Equipped && gear.Category == "Skillsofts" &&
					    (gear.Extra == Name ||
					     gear.Extra == Name + ", " + LanguageManager.Instance.GetString("Label_SelectGear_Hacked")))
					{
						return gear.Name == "Activesoft"
							? Math.Min(gear.Rating, skillWireRating)
							: gear.Rating;

					}
					return gear.Children.Select(child => recusivestuff(child)).FirstOrDefault(returned => returned > 0);
				};

				return _cachedWareRating =  CharacterObject.Gear.Select(child => recusivestuff(child)).FirstOrDefault(val => val > 0);

			}

			return _cachedWareRating = 0;
		}


		
		#endregion

		#region Static

		//A tree of dependencies. Once some of the properties are changed, 
		//anything they depend on, also needs to raise OnChanged
		//This tree keeps track of dependencies
		private static readonly ReverseTree<string> DependencyTree =
			new ReverseTree<string>(nameof(PoolToolTip),
				new ReverseTree<string>(nameof(DisplayPool),
					new ReverseTree<string>(nameof(Pool),
						new ReverseTree<string>(nameof(PoolModifiers)),
						new ReverseTree<string>(nameof(AttributeModifiers)),
						new ReverseTree<string>(nameof(CanHaveSpecs),
							new ReverseTree<string>(nameof(Leveled),
								new ReverseTree<string>(nameof(Rating),
									new ReverseTree<string>(nameof(Karma)),
									new ReverseTree<string>(nameof(BaseUnlocked),
										new ReverseTree<string>(nameof(Base))
									)
								)
							)
						)
					)			
				)
			);
		#endregion

		internal void ForceEvent(string property)
		{
			foreach (string s in DependencyTree.Find(property))
			{
				var v = new PropertyChangedEventArgs(s);
				PropertyChanged?.Invoke(this, v);
			}
		}

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
			else if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Karma))
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

		protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			OnPropertyChanged(nameof(AttributeModifiers));
			if (Enabled != _oldEnable)
			{
				OnPropertyChanged(nameof(Enabled));
				_oldEnable = Enabled;
			}

		}

		private void SpecializationsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
		{
			_cachedStringSpec = null;
			OnPropertyChanged(nameof(Specialization));
			OnPropertyChanged(nameof(DisplaySpecialization));
		}

		[Obsolete("Refactor this method away once improvementmanager gets outbound events")]
		private void OnImprovementEvent(List<Improvement> improvements, ImprovementManager improvementManager)
		{
			_cachedFreeBase = int.MinValue;
			_cachedFreeKarma = int.MinValue;
			_cachedWareRating = int.MinValue;
			if (improvements.Any(imp => 
				(imp.ImproveType == Improvement.ImprovementType.SkillLevel || imp.ImproveType == Improvement.ImprovementType.Skill) && 
				imp.ImprovedName == _name))
			{
				OnPropertyChanged(nameof(PoolModifiers));
			}
			else if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization))
			{
				OnPropertyChanged(nameof(PoolModifiers));
			}

			if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.Attribute && imp.ImprovedName == Attribute && imp.Enabled))
			{
				OnPropertyChanged(nameof(AttributeModifiers));
			}
			else if (improvements.Any(imp => imp.ImproveSource == Improvement.ImprovementSource.Cyberware))
			{
				OnPropertyChanged(nameof(AttributeModifiers));
			}
			
		}

	}
}
