using System;
using System.Collections.Generic;
using System.Xml;

namespace Chummer.Skills
{
	public class KnowledgeSkill : Skill
	{
		private static readonly Dictionary<string, string> CategoriesSkillMap;  //Categories to their attribtue
		private static readonly Dictionary<string, string> NameCategoryMap;  //names to their category

		public static List<ListItem> DefaultKnowledgeSkillCatagories { get; }
		public static List<ListItem> KnowledgeTypes { get; }

		static KnowledgeSkill()
		{
			KnowledgeTypes = new List<ListItem>(); //Load the (possible translated) types of kno skills (Academic, Street...)
			XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

			XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");
			foreach (XmlNode objXmlCategory in objXmlCategoryList)
			{
				string display = objXmlCategory.Attributes["translate"]?.InnerText ?? objXmlCategory.InnerText;
				KnowledgeTypes.Add(new ListItem(objXmlCategory.InnerText, display));
			}


			NameCategoryMap = new Dictionary<string, string>(); 
			CategoriesSkillMap = new Dictionary<string, string>();
			XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill");
			DefaultKnowledgeSkillCatagories = new List<ListItem>();
			foreach (XmlNode objXmlSkill in objXmlSkillList)
			{
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

		private List<ListItem> _knowledgeSkillCatagories;
		private string _type;
		
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
			get { return _name; }
			set
			{
				_name = value;
				if (NameCategoryMap.ContainsKey(value))
				{
					_type = NameCategoryMap[value];
					_spec.Clear();

					XmlNodeList list =
						XmlManager.Instance.Load("skills.xml").SelectNodes($"chummer/knowledgeskills/skill[name = '{_name}']/specs/spec");
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
				return base.PoolModifiers + adj;
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

			if (LearnedRating <= RatingMaximum)
			{
				return -1;
			}
			else if (LearnedRating == 0)
			{
				return CharacterObject.Options.KarmaNewKnowledgeSkill + adjustment;
			}
			else
			{
				return (LearnedRating == 0
					? CharacterObject.Options.KarmaNewActiveSkill
					: (LearnedRating + 1) * CharacterObject.Options.KarmaImproveKnowledgeSkill)
					   + adjustment;
			}
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

		protected override void SaveExtendedData(XmlTextWriter writer)
		{
			writer.WriteElementString("name", _name);
			writer.WriteElementString("type", _type);
		}

		public void Load(XmlNode node)
		{
			node.TryGetField("name", out _name);
			node.TryGetField("type", out _type);
		}
	}
}
