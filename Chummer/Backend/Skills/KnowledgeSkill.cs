using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chummer.Backend.Equipment;
using Chummer.Datastructures;

namespace Chummer.Skills
{
	public class KnowledgeSkill : Skill
	{
		private static readonly TranslatedField<string> _translator = new TranslatedField<string>(); 
		private static readonly Dictionary<string, string> CategoriesSkillMap = new Dictionary<string, string>();  //Categories to their attribtue
		private static readonly Dictionary<string, string> NameCategoryMap = new Dictionary<string, string>();  //names to their category

		public static List<ListItem> DefaultKnowledgeSkillCatagories { get; }
		public static List<ListItem> KnowledgeTypes { get; }  //Load the (possible translated) types of kno skills (Academic, Street...)

		static KnowledgeSkill()
		{
			XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

			XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");

            KnowledgeTypes = (from XmlNode objXmlCategory in objXmlCategoryList
		        let display = objXmlCategory.Attributes["translate"]?.InnerText ?? objXmlCategory.InnerText
		        orderby display
		        select new ListItem(objXmlCategory.InnerText, display)).ToList();


		    XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill");
			DefaultKnowledgeSkillCatagories = new List<ListItem>();
			foreach (XmlNode objXmlSkill in objXmlSkillList)
			{
				string display = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;

				if (GlobalOptions.Instance.Language != "en-us")
				{
					_translator.Add(objXmlSkill["name"].InnerText, display);
				}

				DefaultKnowledgeSkillCatagories.Add(new ListItem(objXmlSkill["name"].InnerText, display));

				NameCategoryMap[objXmlSkill["name"].InnerText] = objXmlSkill["category"].InnerText;
				CategoriesSkillMap[objXmlSkill["category"].InnerText] = objXmlSkill["attribute"].InnerText;
			}
		}

		public static List<ListItem> KnowledgeSkillsWithCategory(params string[] categories)
		{
			HashSet<string> set = new HashSet<string>(categories);

			return DefaultKnowledgeSkillCatagories.Where(x => set.Contains(NameCategoryMap[x.Value])).ToList();
		} 

		public override bool AllowDelete
		{
			get { return true; } //TODO LM
		}

		protected string _translated; //non english name, if present
		private List<ListItem> _knowledgeSkillCatagories;
		private string _type;
		private string _skillCategory;
		public bool ForcedName { get; }

		public KnowledgeSkill(Character character) : base(character, (string)null)
		{
			AttributeObject = character.GetAttribute("LOG");
			AttributeObject.PropertyChanged += OnLinkedAttributeChanged;
			_type = "";
			SuggestedSpecializations = new List<ListItem>();
		}

		public KnowledgeSkill(Character character, string forcedName) : this(character)
		{
			WriteableName = forcedName;
			ForcedName = true;
		}
		
		public List<ListItem> KnowledgeSkillCatagories
		{
			get { return _knowledgeSkillCatagories ?? DefaultKnowledgeSkillCatagories; }
			set
			{
				_knowledgeSkillCatagories = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CustomSkillCatagories));
			}
		}

		public bool CustomSkillCatagories
		{
			get { return _knowledgeSkillCatagories != null; }
		}
		
		public string WriteableName
		{
			get { return _translator.Read(_name, ref _translated); }
			set
			{
				if (ForcedName) return;
				_translator.Write(value,ref _name, ref _translated);
				LoadSuggestedSpecializations(_name);

				OnPropertyChanged();
			}
		}

		private void LoadSuggestedSpecializations(string name)
		{
			if (NameCategoryMap.ContainsKey(name))
			{
				Type = NameCategoryMap[name];
				SuggestedSpecializations.Clear();

				XmlNodeList list =
					XmlManager.Instance.Load("skills.xml").SelectNodes($"chummer/knowledgeskills/skill[name = \"{name}\"]/specs/spec");
				foreach (XmlNode node in list)
				{
					SuggestedSpecializations.Add(ListItem.AutoXml(node.InnerText, node));
				}
				OnPropertyChanged(nameof(CGLSpecializations));
				OnPropertyChanged(nameof(Type));
			}
		}

		public override string SkillCategory
		{
			get { return Type; }
		}

		public override string DisplayPool
		{
			get
			{
				if (Rating == 0 && _type == "Language")
				{
					return "N";
				}
				else
				{
					return base.DisplayPool;
				}
			}
		}

        /// <summary>
        /// The attributeValue this skill have from Skilljacks + Knowsoft
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public override int CyberwareRating()
        {

            if (_cachedWareRating != int.MinValue) return _cachedWareRating;

            if (IsKnowledgeSkill && CharacterObject.SkillsoftAccess)
            {
                Func<Gear, int> recusivestuff = null;
                recusivestuff = (gear) =>
                {
                    //TODO this works with translate?
                    if (gear.Equipped && gear.Category == "Skillsofts" &&
                        (gear.Extra == Name ||
                         gear.Extra == Name + ", " + LanguageManager.Instance.GetString("Label_SelectGear_Hacked")))
                    {
                        return gear.Rating;
                    }
                    return gear.Children.Select(child => recusivestuff(child)).FirstOrDefault(returned => returned > 0);
                };

                return _cachedWareRating = CharacterObject.Gear.Select(child => recusivestuff(child)).FirstOrDefault(val => val > 0);

            }

            return _cachedWareRating = 0;
        }

        public string Type
		{
			get { return _type; }
			set
			{
				if (!CategoriesSkillMap.ContainsKey(value)) return;
				AttributeObject.PropertyChanged -= OnLinkedAttributeChanged;
				AttributeObject = CharacterObject.GetAttribute(CategoriesSkillMap[value]);

				AttributeObject.PropertyChanged += OnLinkedAttributeChanged;
				_type = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(AttributeModifiers));
			}
		}

		/// <summary>
		/// Things that modify the dicepool of the skill
		/// </summary>
		public override int PoolModifiers
		{
			get
			{
				int adj = _type == "Language" && CharacterObject.SkillsSection.Linguist ? 1 : 0;
				return base.PoolModifiers + adj + WoundModifier;
			}
		}

		/// <summary>
		/// How much karma this costs. Return value during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public override int CurrentKarmaCost()
		{
			int cost = 0;
			if (HasRelatedBoost())
			{
				int lower = Base + FreeKarma();

				for (int i = lower; i < LearnedRating; i += 2) //TODO: this is probably fucked
				{
					cost += (i+1)*CharacterObject.Options.KarmaImproveKnowledgeSkill;
				}
			}
			else
			{
				cost = LearnedRating * (LearnedRating + 1);
				int lower = Base + FreeKarma();
				cost -= lower * (lower + 1);

				cost /= 2;
				cost *= CharacterObject.Options.KarmaImproveKnowledgeSkill;
			}

			

			cost +=  //Spec
					(!string.IsNullOrWhiteSpace(Specialization) && BuyWithKarma) ?
					CharacterObject.Options.KarmaSpecialization : 0;

			if (UneducatedEffect()) cost *= 2;

			return cost;
		}


		/// <summary>
		/// Karma price to upgrade. Returns negative if impossible
		/// </summary>
		/// <returns>Price in karma</returns>
		public override int UpgradeKarmaCost()
		{
			int adjustment = 0;
			if (CharacterObject.SkillsSection.JackOfAllTrades && CharacterObject.Created)
			{
				adjustment = LearnedRating > 5 ? 2 : -1;
			}

			if (HasRelatedBoost() && CharacterObject.Created && LearnedRating >= 3)
			{
				adjustment -= 1;
			}


			int value;
			if (LearnedRating >= RatingMaximum)
			{
				return -1;
			}
			else if (LearnedRating == 0)
			{
				value =  CharacterObject.Options.KarmaNewKnowledgeSkill + adjustment;
			}
			else
			{
				value = (LearnedRating == 0
					? CharacterObject.Options.KarmaNewActiveSkill
					: (LearnedRating + 1) * CharacterObject.Options.KarmaImproveKnowledgeSkill)
					   + adjustment;
			}

			if (UneducatedEffect()) value *= 2;
			return value;
		}

		/// <summary>
		/// How much Sp this costs. Price during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public override int CurrentSpCost()
		{
			if (HasRelatedBoost())
			{
				return (BasePoints + (string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma || 
                    (CharacterObject.BuildMethod == CharacterBuildMethod.Karma || 
                    CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule) 
                    && !CharacterObject.Options.FreeKarmaKnowledge ? 0 : 1) + 1)/2;
			}
			else
			{
                return BasePoints + (string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma ||
                    (CharacterObject.BuildMethod == CharacterBuildMethod.Karma ||
                    CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule) 
                    && !CharacterObject.Options.FreeKarmaKnowledge ? 0 : 1);
            }

		}

		/// <summary>
		/// This method checks if the character have the related knowledge skill
		/// quality.
		/// 
		/// Eg. If it is a language skill, it returns Character.Linguistic, if
		/// it is technical it returns Character.TechSchool
		/// </summary>
		/// <returns></returns>
		public bool HasRelatedBoost()
		{
			switch (_type)
			{
				case "Language":
					return CharacterObject.SkillsSection.Linguist;
				case "Professional":
					return CharacterObject.SkillsSection.TechSchool;
				case "Street":
					return CharacterObject.SkillsSection.SchoolOfHardKnocks;
				case "Academic":
					return CharacterObject.SkillsSection.CollegeEducation;
				case "Interest":
					return false; 


				default:
					return false;
			}
		}

		private bool UneducatedEffect()
		{
			switch (_type)
			{
				case "Professional":
					return CharacterObject.SkillsSection.Uneducated;
				case "Academic":
					return CharacterObject.SkillsSection.Uneducated;

				default:
					return false;
			}
		}

		protected override void SaveExtendedData(XmlTextWriter writer)
		{
			writer.WriteElementString("name", _name);
			writer.WriteElementString("type", _type);
			if(_translated != null) writer.WriteElementString(GlobalOptions.Instance.Language, _translated);
			if(ForcedName) writer.WriteElementString("forced", null);
		}

		public void Load(XmlNode node)
		{
			node.TryGetField("name", out _name);
			node.TryGetField(GlobalOptions.Instance.Language, out _translated);

			LoadSuggestedSpecializations(_name);
			Type = node["type"].InnerText;
		}

        public override bool IsKnowledgeSkill
        {
            get { return true; }
        }
    }
}
