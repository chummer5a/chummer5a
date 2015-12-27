using System;
using System.Collections.Generic;
using System.Xml;

namespace Chummer.Skills
{
	public class KnowledgeSkill : Skill
	{
		public KnowledgeSkill(Character character) : base(character, (string)null)
		{
			UsedAttribute = character.GetAttribute("LOG");
			_spec = new List<ListItem>();
		}

		private List<ListItem> _knowledgeSkillCatagories;
		
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

		private static Dictionary<string, string> _categoriesSkillMap; 
		private static Dictionary<string, string> _nameCategoryMap; 
		private static List<ListItem> _defaultKnowledgeSkillCatagories;  //TODO CACHE INVALIDATION
		public static List<ListItem> DefaultKnowledgeSkillCatagories
		{
			get
			{
				if (_defaultKnowledgeSkillCatagories == null)
				{
					_nameCategoryMap = new Dictionary<string, string>();
					_categoriesSkillMap = new Dictionary<string, string>();
					XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");
					XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill");
					_defaultKnowledgeSkillCatagories = new List<ListItem>();
					foreach (XmlNode objXmlSkill in objXmlSkillList)
					{
						string display = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;

						_defaultKnowledgeSkillCatagories.Add(new ListItem(objXmlSkill["name"].InnerText, display));

						_nameCategoryMap[objXmlSkill["name"].InnerText] = objXmlSkill["category"].InnerText;
						_categoriesSkillMap[objXmlSkill["category"].InnerText] = objXmlSkill["attribute"].InnerText;
					}
				}
				return _defaultKnowledgeSkillCatagories;
			}
		}

		private static List<ListItem> _knowledgeTypes; 
		public static List<ListItem> KnowledgeTypes
		{
			get
			{
				if (_knowledgeTypes == null)
				{
					_knowledgeTypes = new List<ListItem>();

					
					XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

					XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");
					foreach (XmlNode objXmlCategory in objXmlSkillList)
					{
						string display = objXmlCategory.Attributes["translate"]?.InnerText ?? objXmlCategory.InnerText;
						
						_knowledgeTypes.Add(new ListItem(objXmlCategory.InnerText, display));
					}
				}

				return _knowledgeTypes;
			}
		}

		public string WriteableName
		{
			get { return _name; }
			set
			{
				_name = value;
				if (_nameCategoryMap.ContainsKey(value))
				{
					_type = _nameCategoryMap[value];
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

		private string _type;

		public string Type
		{
			get { return _type; }
			set
			{
				if (!_categoriesSkillMap.ContainsKey(value)) return;
				UsedAttribute = CharacterObject.GetAttribute(_categoriesSkillMap[value]);
				_type = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(AttributeModifiers));
			}
		}
	}
}
