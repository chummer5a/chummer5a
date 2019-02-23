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

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private static readonly DependancyGraph<string> AttributeSectionDependencyGraph =
            new DependancyGraph<string>(
            );


	    public ObservableCollection<CharacterAttrib> Attributes { get; set; }

	    private static readonly string[] s_LstAttributeStrings = { "BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "MAGAdept", "RES", "ESS", "DEP" };
        public static ReadOnlyCollection<string> AttributeStrings => Array.AsReadOnly(s_LstAttributeStrings);

	    private static readonly string[] s_LstPhysicalAttributes = { "BOD", "AGI", "REA", "STR" };
        public static ReadOnlyCollection<string> PhysicalAttributes => Array.AsReadOnly(s_LstPhysicalAttributes);

	    private static readonly string[] s_LstMentalAttributes = { "CHA", "INT", "LOG", "WIL" };
        public static ReadOnlyCollection<string> MentalAttributes => Array.AsReadOnly(s_LstMentalAttributes);

	    public static string GetAttributeEnglishName(string strAbbrev)
	    {
	        switch (strAbbrev)
	        {
                case "BOD":
                    return "Body";
                case "AGI":
                    return "Agility";
	            case "REA":
	                return "Reaction";
	            case "STR":
	                return "Strength";
	            case "CHA":
	                return "Charisma";
	            case "INT":
	                return "Intuition";
	            case "LOG":
	                return "Logic";
	            case "WIL":
	                return "Willpower";
	            case "EDG":
	                return "Edge";
	            case "MAG":
	                return "Magic";
	            case "MAGAdept":
	                return "Magic (Adept)";
	            case "RES":
	                return "Resonance";
	            case "ESS":
	                return "Essence";
	            case "DEP":
	                return "Depth";
                default:
                    return string.Empty;
            }
	    }

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

	    public void Create(XmlNode charNode, int intValue, int intMinModifier = 0, int intMaxModifier = 0)
        {
            Timekeeper.Start("create_char_attrib");
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

            _objCharacter.BOD.AssignLimits(CommonFunctions.ExpressionToString(charNode["bodmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["bodmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["bodaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.AGI.AssignLimits(CommonFunctions.ExpressionToString(charNode["agimin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["agimax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["agiaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.REA.AssignLimits(CommonFunctions.ExpressionToString(charNode["reamin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["reamax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["reaaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.STR.AssignLimits(CommonFunctions.ExpressionToString(charNode["strmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["strmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["straug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.CHA.AssignLimits(CommonFunctions.ExpressionToString(charNode["chamin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["chamax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["chaaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.INT.AssignLimits(CommonFunctions.ExpressionToString(charNode["intmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["intmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["intaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.LOG.AssignLimits(CommonFunctions.ExpressionToString(charNode["logmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["logmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["logaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.WIL.AssignLimits(CommonFunctions.ExpressionToString(charNode["wilmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["wilmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["wilaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.MAG.AssignLimits(CommonFunctions.ExpressionToString(charNode["magmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["magmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.RES.AssignLimits(CommonFunctions.ExpressionToString(charNode["resmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["resmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["resaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.EDG.AssignLimits(CommonFunctions.ExpressionToString(charNode["edgmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["edgmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["edgaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.DEP.AssignLimits(CommonFunctions.ExpressionToString(charNode["depmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["depmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["depaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.MAGAdept.AssignLimits(CommonFunctions.ExpressionToString(charNode["magmin"]?.InnerText, intValue, intMinModifier), CommonFunctions.ExpressionToString(charNode["magmax"]?.InnerText, intValue, intMaxModifier), CommonFunctions.ExpressionToString(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
            _objCharacter.ESS.AssignLimits(CommonFunctions.ExpressionToString(charNode["essmin"]?.InnerText, intValue, 0), CommonFunctions.ExpressionToString(charNode["essmax"]?.InnerText, intValue, 0), CommonFunctions.ExpressionToString(charNode["essaug"]?.InnerText, intValue, 0));

            Attributes = new ObservableCollection<CharacterAttrib>
            {
                _objCharacter.BOD,
                _objCharacter.AGI,
                _objCharacter.REA,
                _objCharacter.STR,
                _objCharacter.CHA,
                _objCharacter.INT,
                _objCharacter.LOG,
                _objCharacter.WIL,
                _objCharacter.EDG
            };
            if (_objCharacter.MAGEnabled)
            {
                Attributes.Add(_objCharacter.MAG);
                if (_objCharacter.Options.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                    Attributes.Add(_objCharacter.MAGAdept);
            }
            if (_objCharacter.RESEnabled)
            {
                Attributes.Add(_objCharacter.RES);
            }
            if (_objCharacter.DEPEnabled)
            {
                Attributes.Add(_objCharacter.DEP);
            }
            ResetBindings();
            _objCharacter.RefreshAttributeBindings();
            Timekeeper.Finish("create_char_attrib");
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

		    Attributes = new ObservableCollection<CharacterAttrib>
		    {
		        _objCharacter.BOD,
		        _objCharacter.AGI,
		        _objCharacter.REA,
		        _objCharacter.STR,
		        _objCharacter.CHA,
		        _objCharacter.INT,
		        _objCharacter.LOG,
		        _objCharacter.WIL,
		        _objCharacter.EDG
		    };
		    if (_objCharacter.MAGEnabled)
		    {
		        Attributes.Add(_objCharacter.MAG);
		        if (_objCharacter.Options.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
		            Attributes.Add(_objCharacter.MAGAdept);
		    }
		    if (_objCharacter.RESEnabled)
		    {
		        Attributes.Add(_objCharacter.RES);
		    }
		    if (_objCharacter.DEPEnabled)
		    {
		        Attributes.Add(_objCharacter.DEP);
		    }
            ResetBindings();
		    _objCharacter.RefreshAttributeBindings();
            Timekeeper.Finish("load_char_attrib");
		}

	    public void LoadFromHeroLab(XmlNode xmlStatBlockBaseNode)
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
                // First, remake the attribute
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

                // Then load in attribute karma levels (we'll adjust these later if the character is in Create mode)
                if (strAttribute == "ESS") // Not Essence though, this will get modified automatically instead of having its value set to the one HeroLab displays
                    continue;
                XmlNode xmlHeroLabAttributeNode = xmlStatBlockBaseNode.SelectSingleNode("attributes/attribute[@name = \"" + GetAttributeEnglishName(strAttribute) + "\"]");
                XmlNode xmlAttributeBaseNode = xmlHeroLabAttributeNode?.SelectSingleNode("@base");
                if (xmlAttributeBaseNode != null &&
                    int.TryParse(xmlAttributeBaseNode.InnerText, out int intHeroLabAttributeBaseValue))
                {
                    int intAttributeMinimumValue = GetAttributeByName(strAttribute).MetatypeMinimum;
                    if (intHeroLabAttributeBaseValue != intAttributeMinimumValue)
                    {
                        objAttribute.Karma = intHeroLabAttributeBaseValue - intAttributeMinimumValue;
                    }
                }
            }

	        if (!_objCharacter.Created && _objCharacter.BuildMethodHasSkillPoints)
	        {
                // Allocate Attribute Points
	            int intAttributePointCount = _objCharacter.TotalAttributes;
	            CharacterAttrib objAttributeToPutPointsInto;
                // First loop through attributes where costs can be 100% covered with points
	            do
	            {
	                objAttributeToPutPointsInto = null;
	                int intAttributeToPutPointsIntoTotalKarmaCost = 0;
                    foreach (CharacterAttrib objLoopAttribute in AttributeList)
	                {
                        if (objLoopAttribute.Karma == 0)
                            continue;
                        // Put points into the attribute with the highest total karma cost.
	                    // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
	                    int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                        if (objAttributeToPutPointsInto == null || (objLoopAttribute.Karma <= intAttributePointCount &&
                                                                    (intLoopTotalKarmaCost > intAttributeToPutPointsIntoTotalKarmaCost ||
                                                                     (intLoopTotalKarmaCost == intAttributeToPutPointsIntoTotalKarmaCost && objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))))
	                    {
	                        objAttributeToPutPointsInto = objLoopAttribute;
	                        intAttributeToPutPointsIntoTotalKarmaCost = intLoopTotalKarmaCost;
	                    }
	                }

	                if (objAttributeToPutPointsInto != null)
	                {
	                    objAttributeToPutPointsInto.Base = objAttributeToPutPointsInto.Karma;
                        intAttributePointCount -= objAttributeToPutPointsInto.Karma;
                        objAttributeToPutPointsInto.Karma = 0;
	                }
	            } while (objAttributeToPutPointsInto != null && intAttributePointCount > 0);

                // If any points left over, then put them all into the attribute with the highest karma cost
	            if (intAttributePointCount > 0 && AttributeList.Any(x => x.Karma != 0))
	            {
	                int intHighestTotalKarmaCost = 0;
	                foreach (CharacterAttrib objLoopAttribute in AttributeList)
	                {
	                    if (objLoopAttribute.Karma == 0)
	                        continue;
	                    // Put points into the attribute with the highest total karma cost.
	                    // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
	                    int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
	                    if (objAttributeToPutPointsInto == null ||
	                        intLoopTotalKarmaCost > intHighestTotalKarmaCost ||
	                        (intLoopTotalKarmaCost == intHighestTotalKarmaCost && objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))
	                    {
	                        objAttributeToPutPointsInto = objLoopAttribute;
	                        intHighestTotalKarmaCost = intLoopTotalKarmaCost;
	                    }
	                }

	                if (objAttributeToPutPointsInto != null)
	                {
	                    objAttributeToPutPointsInto.Base = intAttributePointCount;
	                    objAttributeToPutPointsInto.Karma -= intAttributePointCount;
	                }
                }

	            // Allocate Special Attribute Points
                intAttributePointCount = _objCharacter.TotalSpecial;
                // First loop through attributes where costs can be 100% covered with points
                do
                {
                    objAttributeToPutPointsInto = null;
                    int intAttributeToPutPointsIntoTotalKarmaCost = 0;
                    foreach (CharacterAttrib objLoopAttribute in SpecialAttributeList)
                    {
                        if (objLoopAttribute.Karma == 0)
                            continue;
                        // Put points into the attribute with the highest total karma cost.
                        // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                        int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                        if (objAttributeToPutPointsInto == null || (objLoopAttribute.Karma <= intAttributePointCount &&
                                                                    (intLoopTotalKarmaCost > intAttributeToPutPointsIntoTotalKarmaCost ||
                                                                     (intLoopTotalKarmaCost == intAttributeToPutPointsIntoTotalKarmaCost && objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))))
                        {
                            objAttributeToPutPointsInto = objLoopAttribute;
                            intAttributeToPutPointsIntoTotalKarmaCost = intLoopTotalKarmaCost;
                        }
                    }

                    if (objAttributeToPutPointsInto != null)
                    {
                        objAttributeToPutPointsInto.Base = objAttributeToPutPointsInto.Karma;
                        intAttributePointCount -= objAttributeToPutPointsInto.Karma;
                        objAttributeToPutPointsInto.Karma = 0;
                    }
                } while (objAttributeToPutPointsInto != null);

                // If any points left over, then put them all into the attribute with the highest karma cost
                if (intAttributePointCount > 0 && SpecialAttributeList.Any(x => x.Karma != 0))
                {
                    int intHighestTotalKarmaCost = 0;
                    foreach (CharacterAttrib objLoopAttribute in SpecialAttributeList)
                    {
                        if (objLoopAttribute.Karma == 0)
                            continue;
                        // Put points into the attribute with the highest total karma cost.
                        // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                        int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                        if (objAttributeToPutPointsInto == null ||
                            intLoopTotalKarmaCost > intHighestTotalKarmaCost ||
                            (intLoopTotalKarmaCost == intHighestTotalKarmaCost && objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))
                        {
                            objAttributeToPutPointsInto = objLoopAttribute;
                            intHighestTotalKarmaCost = intLoopTotalKarmaCost;
                        }
                    }

                    if (objAttributeToPutPointsInto != null)
                    {
                        objAttributeToPutPointsInto.Base = intAttributePointCount;
                        objAttributeToPutPointsInto.Karma -= intAttributePointCount;
                    }
                }
            }
            ResetBindings();
            _objCharacter.RefreshAttributeBindings();
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
		    if (_objCharacter.MetatypeCategory == "Shapeshifter")
		    {
		        XmlDocument xmlMetatypesDoc = XmlManager.Load("metatypes.xml", strLanguageToPrint);
		        XmlNode xmlNode = xmlMetatypesDoc.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{_objCharacter.Metatype}\"]");

		        xmlNode = xmlNode?.SelectSingleNode($"metavariants/metavariant[name = \"{_objCharacter.Metavariant}\"]/name/@translate");

		        if (AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter)
		        {
		            objWriter.WriteElementString("attributecategory", xmlNode?.SelectSingleNode("name/@translate")?.InnerText ?? _objCharacter.Metatype);
                }
		        else
		        {
		            xmlNode = xmlNode?.SelectSingleNode($"metavariants/metavariant[name = \"{_objCharacter.Metavariant}\"]/name/@translate");
                    objWriter.WriteElementString("attributecategory", xmlNode?.InnerText ?? _objCharacter.Metavariant);
                }
		    }
		    objWriter.WriteElementString("attributecategory_english", AttributeCategory.ToString());
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

		internal void ForceAttributePropertyChangedNotificationAll(params string[] lstNames)
		{
			foreach (CharacterAttrib att in AttributeList)
			{
				att.OnMultiplePropertyChanged(lstNames);
			}
		}

		public static void CopyAttribute(CharacterAttrib objSource, CharacterAttrib objTarget, string strMetavariantXPath, XmlDocument xmlDoc)
		{
            string strSourceAbbrev = objSource.Abbrev.ToLower();
            if (strSourceAbbrev == "magadept")
                strSourceAbbrev = "mag";
            XmlNode node = !string.IsNullOrEmpty(strMetavariantXPath) ? xmlDoc.SelectSingleNode(strMetavariantXPath) : null;
		    if (node != null)
		    {
		        objTarget.MetatypeMinimum = Convert.ToInt32(node[$"{strSourceAbbrev}min"]?.InnerText);
		        objTarget.MetatypeMaximum = Convert.ToInt32(node[$"{strSourceAbbrev}max"]?.InnerText);
		        objTarget.MetatypeAugmentedMaximum = Convert.ToInt32(node[$"{strSourceAbbrev}aug"]?.InnerText);
		    }

		    objTarget.Base = objSource.Base;
		    objTarget.Karma = objSource.Karma;
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
	            if (_eAttributeCategory == value) return;
	            _eAttributeCategory = value;
	            OnPropertyChanged();
	        }
	    }
        #endregion
    }
}
