/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Attributes
{
	public class AttributeSection : INotifyMultiplePropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = AttributeSectionDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in AttributeSectionDependencyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if (PropertyChanged != null)
            {
                foreach (string strPropertyToChange in lstNamesOfChangedProperties)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                }
            }
        }

        private static readonly DependancyGraph<string> AttributeSectionDependencyGraph =
            new DependancyGraph<string>(
            );

        private static readonly string[] s_LstAttributeStrings = { "BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "MAGAdept", "RES", "ESS", "DEP" };
        public static ReadOnlyCollection<string> AttributeStrings => Array.AsReadOnly(s_LstAttributeStrings);

	    private static readonly string[] s_LstPhysicalAttributes = { "BOD", "AGI", "REA", "STR" };
        public static ReadOnlyCollection<string> PhysicalAttributes => Array.AsReadOnly(s_LstPhysicalAttributes);

	    private static readonly string[] s_LstMentalAttributes = { "CHA", "INT", "LOG", "WIL" };
        public static ReadOnlyCollection<string> MentalAttributes => Array.AsReadOnly(s_LstMentalAttributes);

	    private readonly Dictionary<string, BindingSource> _dicBindings = new Dictionary<string, BindingSource>(AttributeStrings.Count);
		private readonly Character _objCharacter;
		private CharacterAttrib.AttributeCategory _eAttributeCategory = CharacterAttrib.AttributeCategory.Standard;

        #region Constructor, Save, Load, Print Methods
        public AttributeSection(Character character)
		{
			_objCharacter = character;
		}

		private void BuildBindingList()
		{
			_dicBindings.Clear();
            foreach (string strAttributeString in AttributeStrings)
            {
                _dicBindings.Add(strAttributeString, new BindingSource { DataSource = GetAttributeByName(strAttributeString) });
            }
		}

	    public void UnbindAttributeSection()
	    {
	        _dicBindings.Clear();
	        foreach (CharacterAttrib objAttribute in AttributeList.Concat(SpecialAttributeList))
	            objAttribute.UnbindAttribute();
	        AttributeList.Clear();
	        SpecialAttributeList.Clear();
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

		public void Load(XmlNode xmlSavedCharacterNode)
		{
			Timekeeper.Start("load_char_attrib");
            foreach (CharacterAttrib objAttribute in AttributeList.Concat(SpecialAttributeList))
                objAttribute.UnbindAttribute();
            AttributeList.Clear();
			SpecialAttributeList.Clear();
            XmlDocument objXmlDocument = XmlManager.Load(_objCharacter.IsCritter ? "critters.xml" : "metatypes.xml");
            XmlNode xmlMetatypeNode = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
            XmlNode xmlCharNode = xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + _objCharacter.Metavariant + "\"]") ?? xmlMetatypeNode;
            // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
            XmlNode xmlCharNodeAnimalForm = _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created ? xmlMetatypeNode : null;
            foreach (string strAttribute in AttributeStrings)
            {
                XmlNodeList lstAttributeNodes = xmlSavedCharacterNode.SelectNodes("attributes/attribute[name = \"" + strAttribute + "\"]");
                // Couldn't find the appopriate attribute in the loaded file, so regenerate it from scratch.
                if (lstAttributeNodes == null || lstAttributeNodes.Count == 0 || xmlCharNodeAnimalForm != null && _objCharacter.LastSavedVersion < new Version("5.200.25"))
                {
                    CharacterAttrib objAttribute = new CharacterAttrib(_objCharacter, strAttribute);
                    objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                    switch (CharacterAttrib.ConvertToAttributeCategory(objAttribute.Abbrev))
                    {
                        case CharacterAttrib.AttributeCategory.Special:
                            SpecialAttributeList.Add(objAttribute);
                            break;
                        case CharacterAttrib.AttributeCategory.Standard:
                            AttributeList.Add(objAttribute);
                            break;
                    }
                    if (xmlCharNodeAnimalForm != null)
                    {
                        objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Shapeshifter);
                        objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm);
                        switch (CharacterAttrib.ConvertToAttributeCategory(objAttribute.Abbrev))
                        {
                            case CharacterAttrib.AttributeCategory.Special:
                                SpecialAttributeList.Add(objAttribute);
                                break;
                            case CharacterAttrib.AttributeCategory.Standard:
                                AttributeList.Add(objAttribute);
                                break;
                        }
                    }
                }
                else
                {
                    foreach (XmlNode xmlAttributeNode in lstAttributeNodes)
                    {
                        CharacterAttrib att = new CharacterAttrib(_objCharacter, strAttribute);
                        att.Load(xmlAttributeNode);
                        switch (CharacterAttrib.ConvertToAttributeCategory(att.Abbrev))
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
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCharacterNode[strAttributeLower + "min"]?.InnerText.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1",
                    out bool blnIsSuccess);
                if (blnIsSuccess)
                    intMinValue = Convert.ToInt32(Math.Ceiling((double)objProcess));
            }
            catch (XPathException) { intMinValue = 1; }
            catch (OverflowException) { intMinValue = 1; }
            catch (InvalidCastException) { intMinValue = 1; }
            try
            {
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCharacterNode[strAttributeLower + "max"]?.InnerText.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1",
                    out bool blnIsSuccess);
                if (blnIsSuccess)
                    intMaxValue = Convert.ToInt32(Math.Ceiling((double)objProcess));
            }
            catch (XPathException) { intMaxValue = 1; }
            catch (OverflowException) { intMaxValue = 1; }
            catch (InvalidCastException) { intMaxValue = 1; }
            try
            {
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCharacterNode[strAttributeLower + "aug"]?.InnerText.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1",
                    out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAugValue = Convert.ToInt32(Math.Ceiling((double)objProcess));
            }
            catch (XPathException) { intAugValue = 1; }
            catch (OverflowException) { intAugValue = 1; }
            catch (InvalidCastException) { intAugValue = 1; }

            objNewAttribute.Base = Convert.ToInt32(objCharacterNode["base"]?.InnerText);
            objNewAttribute.Karma = Convert.ToInt32(objCharacterNode["base"]?.InnerText);
            objNewAttribute.AssignLimits(intMinValue.ToString(), intMaxValue.ToString(), intAugValue.ToString());
            return objNewAttribute;
        }

		internal void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
		{
			foreach (CharacterAttrib att in AttributeList)
			{
				att.Print(objWriter, objCulture, strLanguageToPrint);
			}
			foreach (CharacterAttrib att in SpecialAttributeList)
			{
				att.Print(objWriter, objCulture, strLanguageToPrint);
			}
		}
		#endregion

		#region Methods
		public CharacterAttrib GetAttributeByName(string abbrev)
		{
            bool blnGetShifterAttribute = _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created && _objCharacter.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter;
            CharacterAttrib objReturn = AttributeList.FirstOrDefault(att => att.Abbrev == abbrev && (att.MetatypeCategory == CharacterAttrib.AttributeCategory.Shapeshifter) == blnGetShifterAttribute) ?? SpecialAttributeList.FirstOrDefault(att => att.Abbrev == abbrev);
		    return objReturn;
		}

		public BindingSource GetAttributeBindingByName(string abbrev)
		{
            if (_dicBindings.TryGetValue(abbrev, out BindingSource objAttributeBinding))
                return objAttributeBinding;
            return null;
		}

		internal void ForceAttributePropertyChangedNotificationAll(string name)
		{
			foreach (CharacterAttrib att in AttributeList)
			{
				att.OnPropertyChanged(name);
			}
		}

		public static void CopyAttribute(CharacterAttrib source, CharacterAttrib target, string mv, XmlDocument xmlDoc)
		{
            string strSourceAbbrev = source.Abbrev.ToLower();
            if (strSourceAbbrev == "magadept")
                strSourceAbbrev = "mag";
            XmlNode node = xmlDoc.SelectSingleNode($"{mv}");
		    if (node != null)
		    {
		        target.MetatypeMinimum = Convert.ToInt32(node[$"{strSourceAbbrev}min"]?.InnerText);
		        target.MetatypeMaximum = Convert.ToInt32(node[$"{strSourceAbbrev}max"]?.InnerText);
		        target.MetatypeAugmentedMaximum = Convert.ToInt32(node[$"{strSourceAbbrev}aug"]?.InnerText);
		    }

		    target.Base = source.Base;
			target.Karma = source.Karma;
        }

		internal void Reset()
		{
            foreach (CharacterAttrib objAttribute in AttributeList.Concat(SpecialAttributeList))
                objAttribute.UnbindAttribute();
			AttributeList.Clear();
			SpecialAttributeList.Clear();
			foreach (string strAttribute in AttributeStrings)
			{
				CharacterAttrib objAttribute = new CharacterAttrib(_objCharacter, strAttribute);
				switch (CharacterAttrib.ConvertToAttributeCategory(objAttribute.Abbrev))
				{
					case CharacterAttrib.AttributeCategory.Special:
						SpecialAttributeList.Add(objAttribute);
						break;
					case CharacterAttrib.AttributeCategory.Standard:
						AttributeList.Add(objAttribute);
						break;
				}
			}
			BuildBindingList();
		}

		public static CharacterAttrib.AttributeCategory ConvertAttributeCategory(string s)
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
            foreach (KeyValuePair<string, BindingSource> objBindingEntry in _dicBindings)
            {
                objBindingEntry.Value.DataSource = GetAttributeByName(objBindingEntry.Key);
            }
		}
		#endregion

		#region Properties
		/// <summary>
		/// Character's Attributes.
		/// </summary>
		public IList<CharacterAttrib> AttributeList { get; } = new List<CharacterAttrib>();

	    /// <summary>
		/// Character's Attributes.
		/// </summary>
		public IList<CharacterAttrib> SpecialAttributeList { get; } = new List<CharacterAttrib>();

	    public CharacterAttrib.AttributeCategory AttributeCategory
	    {
	        get => _eAttributeCategory;
	        set
	        {
	            if (_eAttributeCategory != value)
	            {
	                _eAttributeCategory = value;
	                OnPropertyChanged();
	            }
	        }
	    }
        #endregion
    }
}
