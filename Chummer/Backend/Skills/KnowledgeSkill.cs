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
		/// How much karma this costs. Return value during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public override int CurrentKarmaCost()
		{
			int cost = Rating*(Rating + 1);
			int lower = Base + FreeKarma();
			cost -= lower*(lower + 1);

			cost +=  //Spec
					(!string.IsNullOrWhiteSpace(Specialization) && BuyWithKarma) ?
					CharacterObject.Options.KarmaSpecialization : 0;

			return cost;
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
