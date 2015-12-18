using System;
using System.Collections.Generic;
using System.Xml;

namespace Chummer.Skills
{ 
	class KnowledgeSkill : Skill
	{
		public KnowledgeSkill(Character character) : base(character)
		{
			
		}

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

	}
}
