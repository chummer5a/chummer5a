using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chummer;

namespace Chummer.Skills
{ 
	class ExoticSkill : Skill
	{
		public ExoticSkill(Character character, XmlNode node) : base(character, node)
		{
			_spec.Clear();

			// Look through the Weapons file and grab the names of items that are part of the appropriate Exotic Category or use the matching Exoctic Skill.
			XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");
			XmlNodeList objXmlWeaponList = objXmlWeaponDocument.SelectNodes($"/chummer/weapons/weapon[category = \"{node["name"].InnerText}s\" or useskill = \"{node["name"].InnerText}\"]");
			foreach (XmlNode objXmlWeapon in objXmlWeaponList)
				_spec.Add(new ListItem(objXmlWeapon["name"].InnerText, 
					objXmlWeapon.Attributes["translate"]?.InnerText ?? objXmlWeapon["name"].InnerText));

		}

		public override bool AllowDelete
		{
			get { return !CharacterObject.Created; }
		}
	}
}
