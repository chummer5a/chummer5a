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
		private string _specific;
		
		public ExoticSkill(Character character, XmlNode node) : base(character, node)
		{
			//SuggestedSpecializations.Clear();

			//// Look through the Weapons file and grab the names of items that are part of the appropriate Exotic Category or use the matching Exoctic Skill.
			//XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");
			//XmlNodeList objXmlWeaponList = objXmlWeaponDocument.SelectNodes($"/chummer/weapons/weapon[category = \"{node["name"].InnerText}s\" or useskill = \"{node["name"].InnerText}\"]");
			//foreach (XmlNode objXmlWeapon in objXmlWeaponList)
			//	SuggestedSpecializations.Add(new ListItem(objXmlWeapon["name"].InnerText, 
			//		objXmlWeapon.Attributes["translate"]?.InnerText ?? objXmlWeapon["name"].InnerText));

			//_allowVisible = !CharacterObject.Created;
		}

		public override bool AllowDelete
		{
			get { return !CharacterObject.Created; }
		}

		public override int CurrentSpCost()
		{
			return BasePoints;
		}

		/// <summary>
		/// How much karma this costs. Return value during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public override int CurrentKarmaCost()
		{
			return RangeCost(Base + FreeKarma(), LearnedRating);
		}

		public override bool IsExoticSkill
		{
			get { return true; }
		}

		/// <summary>
		/// Called during save to allow derived classes to save additional infomation required to rebuild state
		/// </summary>
		/// <param name="writer"></param>
		protected override void SaveExtendedData(XmlTextWriter writer)
		{
			writer.WriteElementString("specific", _specific);
		}

		public string Specific {
			get { return _specific; }
			set
			{
				_specific = value;
				OnPropertyChanged();
			}
		}

		public override string DisplaySpecialization
		{
			get { return Specific; }
		}
	}
}
