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

	    private ObservableCollection<CharacterAttrib> _colAttributes;
        public ObservableCollection<CharacterAttrib> Attributes {
            get
            {
                if (_colAttributes == null)
                {
                    Utils.BreakIfDebug();
                }
                else
                {
                    return _colAttributes;
                }

                _colAttributes = new ObservableCollection<CharacterAttrib>
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
                    _colAttributes.Add(_objCharacter.MAG);
                    if (_objCharacter.Options.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                        _colAttributes.Add(_objCharacter.MAGAdept);
                }
                if (_objCharacter.RESEnabled)
                {
                    _colAttributes.Add(_objCharacter.RES);
                }
                if (_objCharacter.DEPEnabled)
                {
                    _colAttributes.Add(_objCharacter.DEP);
                }

                return _colAttributes;
            }
            internal set => _colAttributes = value;
        }

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

	    public async void Create(XmlNode charNode, int intValue, int intMinModifier = 0, int intMaxModifier = 0)
        {
            using (var op_create_char_attrib = Timekeeper.StartSyncron("create_char_attrib", null, CustomActivity.OperationType.RequestOperation, charNode?.InnerText))
            {
                int intOldBODBase = _objCharacter.BOD.Base;
                int intOldBODKarma = _objCharacter.BOD.Karma;
                int intOldAGIBase= _objCharacter.AGI.Base;
                int intOldAGIKarma = _objCharacter.AGI.Karma;
                int intOldREABase = _objCharacter.REA.Base;
                int intOldREAKarma = _objCharacter.REA.Karma;
                int intOldSTRBase = _objCharacter.STR.Base;
                int intOldSTRKarma = _objCharacter.STR.Karma;
                int intOldCHABase = _objCharacter.CHA.Base;
                int intOldCHAKarma = _objCharacter.CHA.Karma;
                int intOldINTBase = _objCharacter.INT.Base;
                int intOldINTKarma = _objCharacter.INT.Karma;
                int intOldLOGBase = _objCharacter.LOG.Base;
                int intOldLOGKarma = _objCharacter.LOG.Karma;
                int intOldWILBase = _objCharacter.WIL.Base;
                int intOldWILKarma = _objCharacter.WIL.Karma;
                int intOldEDGBase = _objCharacter.EDG.Base;
                int intOldEDGKarma = _objCharacter.EDG.Karma;
                int intOldMAGBase = _objCharacter.MAG.Base;
                int intOldMAGKarma = _objCharacter.MAG.Karma;
                int intOldMAGAdeptBase = _objCharacter.MAGAdept.Base;
                int intOldMAGAdeptKarma = _objCharacter.MAGAdept.Karma;
                int intOldRESBase = _objCharacter.RES.Base;
                int intOldRESKarma = _objCharacter.RES.Karma;
                int intOldDEPBase = _objCharacter.DEP.Base;
                int intOldDEPKarma = _objCharacter.DEP.Karma;
                foreach (CharacterAttrib objAttribute in AttributeList.Concat(SpecialAttributeList))
                    objAttribute.UnbindAttribute();
                AttributeList.Clear();
                SpecialAttributeList.Clear();

                foreach (string strAttribute in AttributeStrings)
                {
                    CharacterAttrib objAttribute;
                    switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                    {
                        case CharacterAttrib.AttributeCategory.Special:
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                CharacterAttrib.AttributeCategory.Special);
                            SpecialAttributeList.Add(objAttribute);
                            break;
                        case CharacterAttrib.AttributeCategory.Standard:
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                CharacterAttrib.AttributeCategory.Standard);
                            AttributeList.Add(objAttribute);
                            break;
                    }
                }

                _objCharacter.BOD.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["bodmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["bodmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["bodaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.AGI.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["agimin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["agimax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["agiaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.REA.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["reamin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["reamax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["reaaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.STR.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["strmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["strmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["straug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.CHA.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["chamin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["chamax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["chaaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.INT.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["intmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["intmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["intaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.LOG.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["logmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["logmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["logaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.WIL.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["wilmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["wilmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["wilaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.MAG.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["magmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["magmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.RES.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["resmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["resmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["resaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.EDG.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["edgmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["edgmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["edgaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.DEP.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["depmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["depmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["depaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.MAGAdept.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["magmin"]?.InnerText, intValue, intMinModifier),
                    CommonFunctions.ExpressionToString(charNode["magmax"]?.InnerText, intValue, intMaxModifier),
                    CommonFunctions.ExpressionToString(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
                _objCharacter.ESS.AssignLimits(
                    CommonFunctions.ExpressionToString(charNode["essmin"]?.InnerText, intValue, 0),
                    CommonFunctions.ExpressionToString(charNode["essmax"]?.InnerText, intValue, 0),
                    CommonFunctions.ExpressionToString(charNode["essaug"]?.InnerText, intValue, 0));

                _objCharacter.BOD.Base = Math.Min(intOldBODBase, _objCharacter.BOD.PriorityMaximum);
                _objCharacter.BOD.Karma = Math.Min(intOldBODKarma, _objCharacter.BOD.KarmaMaximum);
                _objCharacter.AGI.Base = Math.Min(intOldAGIBase, _objCharacter.AGI.PriorityMaximum);
                _objCharacter.AGI.Karma = Math.Min(intOldAGIKarma, _objCharacter.AGI.KarmaMaximum);
                _objCharacter.REA.Base = Math.Min(intOldREABase, _objCharacter.REA.PriorityMaximum);
                _objCharacter.REA.Karma = Math.Min(intOldREAKarma, _objCharacter.REA.KarmaMaximum);
                _objCharacter.STR.Base = Math.Min(intOldSTRBase, _objCharacter.STR.PriorityMaximum);
                _objCharacter.STR.Karma = Math.Min(intOldSTRKarma, _objCharacter.STR.KarmaMaximum);
                _objCharacter.CHA.Base = Math.Min(intOldCHABase, _objCharacter.CHA.PriorityMaximum);
                _objCharacter.CHA.Karma = Math.Min(intOldCHAKarma, _objCharacter.CHA.KarmaMaximum);
                _objCharacter.INT.Base = Math.Min(intOldINTBase, _objCharacter.INT.PriorityMaximum);
                _objCharacter.INT.Karma = Math.Min(intOldINTKarma, _objCharacter.INT.KarmaMaximum);
                _objCharacter.LOG.Base = Math.Min(intOldLOGBase, _objCharacter.LOG.PriorityMaximum);
                _objCharacter.LOG.Karma = Math.Min(intOldLOGKarma, _objCharacter.LOG.KarmaMaximum);
                _objCharacter.WIL.Base = Math.Min(intOldWILBase, _objCharacter.WIL.PriorityMaximum);
                _objCharacter.WIL.Karma = Math.Min(intOldWILKarma, _objCharacter.WIL.KarmaMaximum);
                _objCharacter.EDG.Base = Math.Min(intOldEDGBase, _objCharacter.EDG.PriorityMaximum);
                _objCharacter.EDG.Karma = Math.Min(intOldEDGKarma, _objCharacter.EDG.KarmaMaximum);

                if (Attributes == null)
                {
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
                }
                else
                {
                    // Not creating a new collection here so that CollectionChanged events from previous list are kept
                    Attributes.Clear();
                    Attributes.Add(_objCharacter.BOD);
                    Attributes.Add(_objCharacter.AGI);
                    Attributes.Add(_objCharacter.REA);
                    Attributes.Add(_objCharacter.STR);
                    Attributes.Add(_objCharacter.CHA);
                    Attributes.Add(_objCharacter.INT);
                    Attributes.Add(_objCharacter.LOG);
                    Attributes.Add(_objCharacter.WIL);
                    Attributes.Add(_objCharacter.EDG);
                }
                if (_objCharacter.MAGEnabled)
                {
                    _objCharacter.MAG.Base = Math.Min(intOldMAGBase, _objCharacter.MAG.PriorityMaximum);
                    _objCharacter.MAG.Karma = Math.Min(intOldMAGKarma, _objCharacter.MAG.KarmaMaximum);
                    Attributes.Add(_objCharacter.MAG);
                    if (_objCharacter.Options.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                    {
                        _objCharacter.MAGAdept.Base = Math.Min(intOldMAGAdeptBase, _objCharacter.MAGAdept.PriorityMaximum);
                        _objCharacter.MAGAdept.Karma = Math.Min(intOldMAGAdeptKarma, _objCharacter.MAGAdept.KarmaMaximum);
                        Attributes.Add(_objCharacter.MAGAdept);
                    }
                }

                if (_objCharacter.RESEnabled)
                {
                    _objCharacter.RES.Base = Math.Min(intOldRESBase, _objCharacter.RES.PriorityMaximum);
                    _objCharacter.RES.Karma = Math.Min(intOldRESKarma, _objCharacter.RES.KarmaMaximum);
                    Attributes.Add(_objCharacter.RES);
                }

                if (_objCharacter.DEPEnabled)
                {
                    _objCharacter.DEP.Base = Math.Min(intOldDEPBase, _objCharacter.DEP.PriorityMaximum);
                    _objCharacter.DEP.Karma = Math.Min(intOldDEPKarma, _objCharacter.DEP.KarmaMaximum);
                    Attributes.Add(_objCharacter.DEP);
                }

                ResetBindings();
                _objCharacter.RefreshAttributeBindings();
                //Timekeeper.Finish("create_char_attrib");
            }
        }

		public void Load(XmlNode xmlSavedCharacterNode)
		{
			//Timekeeper.Start("load_char_attrib");
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
                    CharacterAttrib objAttribute;
                    switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                    {
                        case CharacterAttrib.AttributeCategory.Special:
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Special);
                            objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                            SpecialAttributeList.Add(objAttribute);
                            break;
                        case CharacterAttrib.AttributeCategory.Standard:
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Standard);
                            objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                            AttributeList.Add(objAttribute);
                            break;
                    }

                    if (xmlCharNodeAnimalForm == null) continue;
                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Shapeshifter);
                    objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm);
                    switch (CharacterAttrib.ConvertToAttributeCategory(objAttribute.Abbrev))
                    {
                        case CharacterAttrib.AttributeCategory.Special:
                            SpecialAttributeList.Add(objAttribute);
                            break;
                        case CharacterAttrib.AttributeCategory.Shapeshifter:
                            AttributeList.Add(objAttribute);
                            break;
                    }
                }
                else
                {
                    foreach (XmlNode xmlAttributeNode in lstAttributeNodes)
                    {
                        CharacterAttrib objAttribute;
                        switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                        {
                            case CharacterAttrib.AttributeCategory.Special:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Special);
                                objAttribute.Load(xmlAttributeNode);
                                SpecialAttributeList.Add(objAttribute);
                                break;
                            case CharacterAttrib.AttributeCategory.Standard:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Standard);
                                objAttribute.Load(xmlAttributeNode);
                                AttributeList.Add(objAttribute);
                                break;
                        }
                    }
                }
            }
            ResetBindings();
		    _objCharacter.RefreshAttributeBindings();
            //Timekeeper.Finish("load_char_attrib");
		}

	    public async void LoadFromHeroLab(XmlNode xmlStatBlockBaseNode, CustomActivity parentActivity)
	    {
            using (var op_load_char_attrib = Timekeeper.StartSyncron("load_char_attrib", parentActivity))
            {
                foreach (CharacterAttrib objAttribute in AttributeList.Concat(SpecialAttributeList))
                    objAttribute.UnbindAttribute();
                AttributeList.Clear();
                SpecialAttributeList.Clear();
                XmlDocument objXmlDocument =
                    XmlManager.Load(_objCharacter.IsCritter ? "critters.xml" : "metatypes.xml");
                XmlNode xmlMetatypeNode =
                    objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype +
                                                    "\"]");
                XmlNode xmlCharNode =
                    xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + _objCharacter.Metavariant +
                                                      "\"]") ?? xmlMetatypeNode;
                // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
                XmlNode xmlCharNodeAnimalForm =
                    _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created ? xmlMetatypeNode : null;
                foreach (string strAttribute in AttributeStrings)
                {
                    // First, remake the attribute

                    CharacterAttrib objAttribute = null;
                    switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                    {
                        case CharacterAttrib.AttributeCategory.Special:
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                CharacterAttrib.AttributeCategory.Special);
                            objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                            SpecialAttributeList.Add(objAttribute);
                            break;
                        case CharacterAttrib.AttributeCategory.Standard:
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                CharacterAttrib.AttributeCategory.Standard);
                            objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                            AttributeList.Add(objAttribute);
                            break;
                    }

                    if (xmlCharNodeAnimalForm != null)
                    {
                        switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                        {
                            case CharacterAttrib.AttributeCategory.Special:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                    CharacterAttrib.AttributeCategory.Special);
                                objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm);
                                SpecialAttributeList.Add(objAttribute);
                                break;
                            case CharacterAttrib.AttributeCategory.Shapeshifter:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                    CharacterAttrib.AttributeCategory.Shapeshifter);
                                objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm);
                                AttributeList.Add(objAttribute);
                                break;
                        }
                    }

                    // Then load in attribute karma levels (we'll adjust these later if the character is in Create mode)
                    if (strAttribute == "ESS"
                    ) // Not Essence though, this will get modified automatically instead of having its value set to the one HeroLab displays
                        continue;
                    XmlNode xmlHeroLabAttributeNode =
                        xmlStatBlockBaseNode.SelectSingleNode(
                            "attributes/attribute[@name = \"" + GetAttributeEnglishName(strAttribute) + "\"]");
                    XmlNode xmlAttributeBaseNode = xmlHeroLabAttributeNode?.SelectSingleNode("@base");
                    if (xmlAttributeBaseNode != null &&
                        int.TryParse(xmlAttributeBaseNode.InnerText, out int intHeroLabAttributeBaseValue))
                    {
                        int intAttributeMinimumValue = GetAttributeByName(strAttribute).MetatypeMinimum;
                        if (intHeroLabAttributeBaseValue == intAttributeMinimumValue) continue;
                        if (objAttribute != null)
                            objAttribute.Karma = intHeroLabAttributeBaseValue - intAttributeMinimumValue;
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
                            if (objAttributeToPutPointsInto == null ||
                                (objLoopAttribute.Karma <= intAttributePointCount &&
                                 (intLoopTotalKarmaCost > intAttributeToPutPointsIntoTotalKarmaCost ||
                                  (intLoopTotalKarmaCost == intAttributeToPutPointsIntoTotalKarmaCost &&
                                   objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))))
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
                                (intLoopTotalKarmaCost == intHighestTotalKarmaCost &&
                                 objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))
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
                            if (objAttributeToPutPointsInto == null ||
                                (objLoopAttribute.Karma <= intAttributePointCount &&
                                 (intLoopTotalKarmaCost > intAttributeToPutPointsIntoTotalKarmaCost ||
                                  (intLoopTotalKarmaCost == intAttributeToPutPointsIntoTotalKarmaCost &&
                                   objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))))
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
                                (intLoopTotalKarmaCost == intHighestTotalKarmaCost &&
                                 objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))
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
                //Timekeeper.Finish("load_char_attrib");
            }
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

		        if (AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
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
			    CharacterAttrib objAttribute;
				switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
				{
				    case CharacterAttrib.AttributeCategory.Special:
				        objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Special);
				        SpecialAttributeList.Add(objAttribute);
				        break;
				    case CharacterAttrib.AttributeCategory.Standard:
				        objAttribute = new CharacterAttrib(_objCharacter, strAttribute, CharacterAttrib.AttributeCategory.Standard);
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
        [HubTag(true)]
        public List<CharacterAttrib> AttributeList { get; } = new List<CharacterAttrib>();

        /// <summary>
        /// Character's Attributes.
        /// </summary>
        public List<CharacterAttrib> SpecialAttributeList { get; } = new List<CharacterAttrib>();

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
