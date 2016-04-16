using System;
using System.Collections.Generic;
using System.Xml;
using Chummer.Datastructures;

namespace Chummer.Skills
{
	public class KnowledgeSkill : Skill
	{
		private static readonly TranslatedField<string> _translator = new TranslatedField<string>(); 
		private static readonly Dictionary<string, string> CategoriesSkillMap = new Dictionary<string, string>();  //Categories to their attribtue
		private static readonly Dictionary<string, string> NameCategoryMap = new Dictionary<string, string>();  //names to their category

		public static List<ListItem> DefaultKnowledgeSkillCatagories { get; }
		public static List<ListItem> KnowledgeTypes { get; } = new List<ListItem>(); //Load the (possible translated) types of kno skills (Academic, Street...)

		static KnowledgeSkill()
		{
			XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

			XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");
			foreach (XmlNode objXmlCategory in objXmlCategoryList)
			{
				string display = objXmlCategory.Attributes["translate"]?.InnerText ?? objXmlCategory.InnerText;
				KnowledgeTypes.Add(new ListItem(objXmlCategory.InnerText, display));
			}

			

			XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill");
			DefaultKnowledgeSkillCatagories = new List<ListItem>();
			foreach (XmlNode objXmlSkill in objXmlSkillList)
			{
				if (GlobalOptions.Instance.Language != "en-us")
				{
					_translator.Add(objXmlSkill["name"].InnerText, objXmlSkill["translate"]?.InnerText);
				}

				string display = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;

				DefaultKnowledgeSkillCatagories.Add(new ListItem(objXmlSkill["name"].InnerText, display));

				NameCategoryMap[objXmlSkill["name"].InnerText] = objXmlSkill["category"].InnerText;
				CategoriesSkillMap[objXmlSkill["category"].InnerText] = objXmlSkill["attribute"].InnerText;
			}
		}

		public override bool AllowDelete
		{
			get { return true; } //TODO LM
		}

		protected string _translated; //non english name, if present
		private List<ListItem> _knowledgeSkillCatagories;
		private string _type;
		private string _skillCategory;

		public KnowledgeSkill(Character character) : base(character, (string)null)
		{
			AttributeObject = character.GetAttribute("LOG");
			_type = "";
			_spec = new List<ListItem>();
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
				_translator.Write(value,ref _name, ref _translated);
				if (NameCategoryMap.ContainsKey(_name))
				{
					Type = NameCategoryMap[_name];
					_spec.Clear();

					XmlNodeList list =
						XmlManager.Instance.Load("skills.xml").SelectNodes($"chummer/knowledgeskills/skill[name = \"{_name}\"]/specs/spec");
					foreach (XmlNode node in list)
					{
						_spec.Add(ListItem.AutoXml(node.InnerText, node));
					}
					OnPropertyChanged(nameof(CGLSpecializations));
					OnPropertyChanged(nameof(Type));
				}

				OnPropertyChanged();
			}
		}

		public override string SkillCategory
		{
			get { return Type; }
		}

		public string Type
		{
			get { return _type; }
			set
			{
				if (!CategoriesSkillMap.ContainsKey(value)) return;
				AttributeObject = CharacterObject.GetAttribute(CategoriesSkillMap[value]);
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
				int adj = _type == "Language" && CharacterObject.Linguist ? 1 : 0;
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
			if (CharacterObject.JackOfAllTrades && CharacterObject.Created)
			{
				adjustment = LearnedRating > 5 ? 2 : -1;
			}

			if (HasRelatedBoost() && CharacterObject.Created && LearnedRating >= 3)
			{
				adjustment -= 1;
			}


			int value;
			if (LearnedRating <= RatingMaximum)
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
				return (Ibase - 1)/2 + 1;
			}
			else
			{
				return Ibase;
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
					return CharacterObject.Linguist;
				case "Professional":
					return CharacterObject.TechSchool;
				case "Street":
					return CharacterObject.SchoolOfHardKnocks;
				case "Academic":
					return CharacterObject.CollegeEducation;
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
					return CharacterObject.Uneducated;
				case "Academic":
					return CharacterObject.Uneducated;

				default:
					return false;
			}
		}

		protected override void SaveExtendedData(XmlTextWriter writer)
		{
			writer.WriteElementString("name", _name);
			writer.WriteElementString("type", _type);
			if(_translated != null) writer.WriteElementString(GlobalOptions.Instance.Language, _translated);
		}

		public void Load(XmlNode node)
		{
			node.TryGetField("name", out _name);
			node.TryGetField("type", out _type);
			node.TryGetField(GlobalOptions.Instance.Language, out _translated);
		}
	}
}
