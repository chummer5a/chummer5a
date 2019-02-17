using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace Chummer.Backend.Attributes
{
	public class AttributeSection : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static string[] AttributeStrings = { "BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "RES", "ESS", "DEP" };
	    private List<KeyValuePair<string,BindingSource>> _bindings = new List<KeyValuePair<string, BindingSource>>();
		private readonly Character _character;
		private CharacterAttrib.AttributeCategory _attributeCategory = CharacterAttrib.AttributeCategory.Standard;
	    public Action<object> AttributeCategoryChanged;

        #region Constructor, Save, Load, Print Methods
        public AttributeSection(Character character)
		{
			_character = character;
		}

		private void BuildBindingList()
		{
			_bindings.Clear();
			_bindings.Add(new KeyValuePair<string, BindingSource>("BOD", BODBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("AGI", AGIBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("REA", RESBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("STR", STRBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("CHA", CHABinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("INT", INTBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("LOG", LOGBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("WIL", WILBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("EDG", EDGBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("MAG", MAGBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("RES", RESBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("ESS", ESSBinding));
			_bindings.Add(new KeyValuePair<string, BindingSource>("DEP", DEPBinding));
		}

		internal void Save(XmlTextWriter objWriter)
		{
			foreach (CharacterAttrib objAttribute in AttributeList)
			{
				objAttribute.Save(objWriter);
			}
			foreach (CharacterAttrib objAttribute in SpecialAttributeList)
			{
				objAttribute.Save(objWriter);
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

		internal void Print(XmlTextWriter objWriter)
		{
			foreach (CharacterAttrib att in AttributeList)
			{
				att.Print(objWriter);
			}
			foreach (CharacterAttrib att in SpecialAttributeList)
			{
				att.Print(objWriter);
			}
		}
		#endregion

		#region Methods
		public CharacterAttrib GetAttributeByName(string abbrev)
		{
			if (AttributeList.Any(att => att.Abbrev == abbrev))
			{
				return _character.MetatypeCategory == "Shapeshifter" && _character.Created && _character.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter
				? AttributeList.First(att => att.Abbrev == abbrev && att.MetatypeCategory == CharacterAttrib.AttributeCategory.Shapeshifter) 
				: AttributeList.First(att => att.Abbrev == abbrev);
			}
			if (SpecialAttributeList.Any(att => att.Abbrev == abbrev))
			{
				return SpecialAttributeList.First(att => att.Abbrev == abbrev);
			}
			return null;
		}

		public BindingSource GetAttributeBindingByName(string abbrev)
		{
			return _bindings.FirstOrDefault(b => b.Key == abbrev).Value;
		}

		internal void ForceAttributePropertyChangedNotificationAll(string name)
		{
			foreach (CharacterAttrib att in AttributeList)
			{
				att.ForceEvent(name);
			}
		}

		public void CopyAttribute(CharacterAttrib source, CharacterAttrib target, string mv, XmlDocument xmlDoc)
		{
			XmlNode node = xmlDoc.SelectSingleNode($"{mv}");
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
			ResetBindings();
			BuildBindingList();
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

		/// <summary>
		/// Reset the databindings for all character attributes. 
		/// This method is used to support hot-swapping attributes for shapeshifters.
		/// </summary>
		public void ResetBindings()
		{
			BODBinding.DataSource = _character.BOD;
			AGIBinding.DataSource = _character.AGI;
			REABinding.DataSource = _character.REA;
			STRBinding.DataSource = _character.STR;
			CHABinding.DataSource = _character.CHA;
			INTBinding.DataSource = _character.INT;
			LOGBinding.DataSource = _character.LOG;
			WILBinding.DataSource = _character.WIL;
			EDGBinding.DataSource = _character.EDG;
			MAGBinding.DataSource = _character.MAG;
			RESBinding.DataSource = _character.RES;
			DEPBinding.DataSource = _character.DEP;
			ESSBinding.DataSource = _character.ESS;
		}
		#endregion

		#region Bindings
		public BindingSource BODBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource AGIBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource REABinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource STRBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource CHABinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource INTBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource LOGBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource WILBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource EDGBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource MAGBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource RESBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource DEPBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		public BindingSource ESSBinding = new BindingSource { DataSource = typeof(CharacterAttrib) };
		#endregion

		#region Properties
		/// <summary>
		/// Character's Attributes.
		/// </summary>
		public List<CharacterAttrib> AttributeList { get; set; } = new List<CharacterAttrib>();

	    /// <summary>
		/// Character's Attributes.
		/// </summary>
		public List<CharacterAttrib> SpecialAttributeList { get; set; } = new List<CharacterAttrib>();

	    public CharacterAttrib.AttributeCategory AttributeCategory
	    {
	        get => _attributeCategory;
	        set
	        {
	            _attributeCategory = value;
	            AttributeCategoryChanged?.Invoke(this);
            }
	    }

        internal void ForceAttributePropertyChangedNotificationAll(string v, string improvedName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
