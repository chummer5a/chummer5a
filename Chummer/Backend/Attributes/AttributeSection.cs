using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Attributes
{
	public class AttributeSection : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static string[] AttributeStrings = { "BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "MAGAdept", "RES", "ESS", "DEP" };
	    private Dictionary<string, BindingSource> _bindings = new Dictionary<string, BindingSource>(AttributeStrings.Length);
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
            foreach (string strAttributeString in AttributeStrings)
            {
                _bindings.Add(strAttributeString, new BindingSource { DataSource = GetAttributeByName(strAttributeString) });
            }
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
			AttributeList.Clear();
			SpecialAttributeList.Clear();
            XmlDocument objXmlDocument = XmlManager.Load(_character.IsCritter ? "critters.xml" : "metatypes.xml");
            XmlNode objCharNode = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _character.Metatype + "\"]/metavariants/metavariant[name = \"" + _character.Metavariant + "\"]")
                        ?? objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _character.Metatype + "\"]");
            XmlNode objCharNodeAnimalForm = null;
            // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
            if (_character.MetatypeCategory == "Shapeshifter" && _character.Created)
            {
                objCharNodeAnimalForm = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _character.Metatype + "\"]");
            }
            foreach (string s in AttributeStrings)
            {
                XmlNodeList attNodeList = xmlDoc.SelectNodes("/character/attributes/attribute[name = \"" + s + "\"]");
                // Couldn't find the appopriate attribute in the loaded file, so regenerate it from scratch. 
                if (attNodeList.Count == 0)
                {
                    CharacterAttrib att = new CharacterAttrib(_character, s);
                    att = RemakeAttribute(att, objCharNode);
                    switch (att.ConvertToAttributeCategory(att.Abbrev))
                    {
                        case CharacterAttrib.AttributeCategory.Special:
                            SpecialAttributeList.Add(att);
                            break;
                        case CharacterAttrib.AttributeCategory.Standard:
                            AttributeList.Add(att);
                            break;
                    }
                    if (objCharNodeAnimalForm != null)
                    {
                        att = new CharacterAttrib(_character, s, CharacterAttrib.AttributeCategory.Shapeshifter);
                        att = RemakeAttribute(att, objCharNodeAnimalForm);
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
                else
                {
                    foreach (XmlNode attNode in attNodeList)
                    {
                        CharacterAttrib att = new CharacterAttrib(_character, s);
                        att.Load(attNode);
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
            }
            ResetBindings();
			Timekeeper.Finish("load_char_attrib");
		}

        private static CharacterAttrib RemakeAttribute(CharacterAttrib objNewAttribute, XmlNode objCharacterNode)
        {
            string strAttributeLower = objNewAttribute.Abbrev.ToLowerInvariant();
            if (strAttributeLower == "magadept")
                strAttributeLower = "mag";
            int intMinValue = 1;
            int intMaxValue = 1;
            int intAugValue = 1;

            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                intMinValue = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objCharacterNode[strAttributeLower + "min"]?.InnerText.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1")));
            }
            catch (XPathException) { intMinValue = 1; }
            catch (OverflowException) { intMinValue = 1; }
            catch (InvalidCastException) { intMinValue = 1; }
            try
            {
                intMaxValue = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objCharacterNode[strAttributeLower + "max"]?.InnerText.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1")));
            }
            catch (XPathException) { intMaxValue = 1; }
            catch (OverflowException) { intMaxValue = 1; }
            catch (InvalidCastException) { intMaxValue = 1; }
            try
            {
                intAugValue = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objCharacterNode[strAttributeLower + "aug"]?.InnerText.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1")));
            }
            catch (XPathException) { intAugValue = 1; }
            catch (OverflowException) { intAugValue = 1; }
            catch (InvalidCastException) { intAugValue = 1; }

            objNewAttribute.AssignLimits(intMinValue.ToString(), intMaxValue.ToString(), intAugValue.ToString());
            return objNewAttribute;
        }

		internal void Print(XmlTextWriter objWriter, CultureInfo objCulture)
		{
			foreach (CharacterAttrib att in AttributeList)
			{
				att.Print(objWriter, objCulture);
			}
			foreach (CharacterAttrib att in SpecialAttributeList)
			{
				att.Print(objWriter, objCulture);
			}
		}
		#endregion

		#region Methods
		public CharacterAttrib GetAttributeByName(string abbrev)
		{
            bool blnGetShifterAttribute = _character.MetatypeCategory == "Shapeshifter" && _character.Created && _character.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter;
            CharacterAttrib objReturn = AttributeList.FirstOrDefault(att => att.Abbrev == abbrev && (att.MetatypeCategory == CharacterAttrib.AttributeCategory.Shapeshifter) == blnGetShifterAttribute);
            if (objReturn == null)
			{
                objReturn = SpecialAttributeList.FirstOrDefault(att => att.Abbrev == abbrev);
            }
			return objReturn;
		}

		public BindingSource GetAttributeBindingByName(string abbrev)
		{
            BindingSource objAttributeBinding;
            if (_bindings.TryGetValue(abbrev, out objAttributeBinding))
                return objAttributeBinding;
            return null;
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
            string strSourceAbbrev = source.Abbrev.ToLower();
            if (strSourceAbbrev == "magadept")
                strSourceAbbrev = "mag";
            XmlNode node = xmlDoc.SelectSingleNode($"{mv}");
			target.MetatypeMinimum = Convert.ToInt32(node[$"{strSourceAbbrev}min"].InnerText);
			target.MetatypeMaximum = Convert.ToInt32(node[$"{strSourceAbbrev}max"].InnerText);
			target.MetatypeAugmentedMaximum = Convert.ToInt32(node[$"{strSourceAbbrev}aug"].InnerText);
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
            foreach (KeyValuePair<string, BindingSource> objBindingEntry in _bindings)
            {
                objBindingEntry.Value.DataSource = GetAttributeByName(objBindingEntry.Key);
            }
		}
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
        #endregion
    }
}
