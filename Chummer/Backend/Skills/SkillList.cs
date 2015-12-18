using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Chummer.Skills
{
	/// <summary>
	/// Overengineered shit kept temporarily
	/// TODO: REMOVE THIS
	/// </summary>
	public class SkillList : BindingList<Skill>
	{
		private readonly BindingList<Skill> _skills;
		private readonly BindingList<KnowledgeSkill> _knowledgeSkills;  
		private readonly Character _character;
		public SkillList(Character character)
		{
			_character = character;

			BuildList();

			_skills = new BindingList<Skill>((from Skill in this 
											 where !(Skill is KnowledgeSkill)
											 select Skill).ToList());
			_knowledgeSkills = new BindingList<KnowledgeSkill>((from Skill in this
																where Skill is KnowledgeSkill
																select (KnowledgeSkill)Skill).ToList());
			ListChanged += OnParrentChanging;
		}

		private void OnParrentChanging(object sender, ListChangedEventArgs listChangedEventArgs)
		{
			if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
			{
				KnowledgeSkill item = this[listChangedEventArgs.NewIndex] as KnowledgeSkill;
				if (item != null)
				{
					_knowledgeSkills.Add(item);
				}
				else
				{
					_skills.Add(this[listChangedEventArgs.NewIndex]);
				}
			}
		}

		private void BuildList()
		{
			// Load the Skills information.
			XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

			// Populate the Skills list.
			XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[not(exotic) and (" + _character.Options.BookXPath() + ")]");

			// First pass, build up a list of all of the Skills so we can sort them in alphabetical order for the current language.
			List<ListItem> lstSkillOrder = new List<ListItem>();
			foreach (XmlNode objXmlSkill in objXmlSkillList)
			{
				ListItem objSkill = new ListItem();
				objSkill.Value = objXmlSkill["name"].InnerText;
				if (objXmlSkill["translate"] != null)
					objSkill.Name = objXmlSkill["translate"].InnerText;
				else
					objSkill.Name = objXmlSkill["name"].InnerText;
				lstSkillOrder.Add(objSkill);
			}
			SortListItem objSort = new SortListItem();
			lstSkillOrder.Sort(objSort.Compare);

			// Second pass, retrieve the Skills in the order they're presented in the list.
			foreach (ListItem objItem in lstSkillOrder)
			{
				XmlNode objXmlSkill = objXmlDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + objItem.Value + "\"]");
				Skill objSkill = Skill.FromData(objXmlSkill, _character);
				Add(objSkill);
			}
		}
	}
}
