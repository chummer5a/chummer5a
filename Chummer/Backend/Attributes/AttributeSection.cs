using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace Chummer.Backend.Attributes
{
	public class AttributeSection : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static string[] AttributeStrings = { "BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "RES", "ESS", "DEP" };
		private List<CharacterAttrib> _attributes = new List<CharacterAttrib>();
		private List<CharacterAttrib> _specialAttributes = new List<CharacterAttrib>();
		private readonly Character _character;
		public CharacterAttrib.AttributeCategory AttributeCategory = CharacterAttrib.AttributeCategory.Standard;

		public AttributeSection(Character character)
		{
			_character = character;
		}

		/// <summary>
		/// Character's Attributes.
		/// </summary>
		public List<CharacterAttrib> AttributeList
		{
			get
			{
				return _attributes;
			}
			set
			{
				_attributes = value;
			}
		}
		/// <summary>
		/// Character's Attributes.
		/// </summary>
		public List<CharacterAttrib> SpecialAttributeList
		{
			get
			{
				return _specialAttributes;
			}
			set
			{
				_specialAttributes = value;
			}
		}

		internal void ForceAttributePropertyChangedNotificationAll(string name)
		{
			foreach (CharacterAttrib att in _attributes)
			{
				att.ForceEvent(name);
			}
		}

		public void Load(XmlDocument xmlDoc)
		{
			Timekeeper.Start("load_char_attrib");
			XmlNodeList nodes = xmlDoc.SelectNodes("/character/attributes/attribute");
			if (nodes == null) Utils.BreakIfDebug();
			AttributeList.Clear();
			SpecialAttributeList.Clear();
			foreach (XmlNode node in nodes)
			{
				CharacterAttrib att = new CharacterAttrib(_character,node["name"].InnerText);
				att.Load(node);
				switch (att.ConvertToAttributeCategory(att.Abbrev))
				{
					case CharacterAttrib.AttributeCategory.Special:
						SpecialAttributeList.Add(att);
						break;
					case CharacterAttrib.AttributeCategory.Standard:
						AttributeList.Add(att);
						break;
				}
			}
			Timekeeper.Finish("load_char_attrib");
		}

		public CharacterAttrib GetAttributeByName(string abbrev)
		{
			if (_attributes.Any(att => att.Abbrev == abbrev))
			{
				return _character.MetatypeCategory == "Shapeshifter" && _character.Created && _character.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter
				? _attributes.First(att => att.Abbrev == abbrev && att.MetatypeCategory == CharacterAttrib.AttributeCategory.Shapeshifter) 
				: _attributes.First(att => att.Abbrev == abbrev);
			}
			if (_specialAttributes.Any(att => att.Abbrev == abbrev))
			{
				return _specialAttributes.First(att => att.Abbrev == abbrev);
			}
			return null;
		}

		internal void Print(XmlTextWriter objWriter)
		{
			foreach (CharacterAttrib att in _attributes)
			{
				att.Print(objWriter);
			}
			foreach (CharacterAttrib att in _specialAttributes)
			{
				att.Print(objWriter);
			}
		}

		public void CopyAttribute(CharacterAttrib source, CharacterAttrib target, string mv, XmlDocument xmlDoc)
		{
			XmlNode node = xmlDoc.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{mv}\"]");
			target.MetatypeMinimum = Convert.ToInt32(node[$"{source.Abbrev.ToLower()}min"].InnerText);
			target.MetatypeMaximum = Convert.ToInt32(node[$"{source.Abbrev.ToLower()}max"].InnerText);
			target.MetatypeAugmentedMaximum = Convert.ToInt32(node[$"{source.Abbrev.ToLower()}aug"].InnerText);
			target.Base = source.Base;
			target.Karma = source.Karma;
		}

		internal void Reset()
		{
			AttributeList.Clear();
			SpecialAttributeList.Clear();
			foreach (string strAttribute in AttributeStrings)
			{
				CharacterAttrib att = new CharacterAttrib(_character, strAttribute);
				switch (att.ConvertToAttributeCategory(att.Abbrev))
				{
					case CharacterAttrib.AttributeCategory.Special:
						SpecialAttributeList.Add(att);
						break;
					case CharacterAttrib.AttributeCategory.Standard:
						AttributeList.Add(att);
						break;
				}
			}
		}

		internal void Save(XmlTextWriter objWriter)
		{
			foreach (CharacterAttrib objAttribute in _attributes)
			{
				objAttribute.Save(objWriter);
			}
			foreach (CharacterAttrib objAttribute in _specialAttributes)
			{
				objAttribute.Save(objWriter);
			}
		}

		public CharacterAttrib.AttributeCategory ConvertAttributeCategory(string s)
		{
			switch (s)
			{
				case "Shapeshifter":
					return CharacterAttrib.AttributeCategory.Shapeshifter;
				case "Special":
					return CharacterAttrib.AttributeCategory.Special;
				case "Metahuman":
				case "Standard":
					return CharacterAttrib.AttributeCategory.Standard;
			}
			return CharacterAttrib.AttributeCategory.Standard;
		}
	}
}
