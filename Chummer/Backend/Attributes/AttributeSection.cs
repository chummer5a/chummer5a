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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Attributes
{
    public sealed class AttributeSection : INotifyMultiplePropertyChanged, IDisposable
    {
        private bool _blnLoading = true;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            HashSet<string> setNamesOfChangedProperties = null;
            try
            {
                foreach (string strPropertyName in lstPropertyNames)
                {
                    if (setNamesOfChangedProperties == null)
                        setNamesOfChangedProperties
                            = s_AttributeSectionDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                    else
                    {
                        foreach (string strLoopChangedProperty in s_AttributeSectionDependencyGraph
                                     .GetWithAllDependentsEnumerable(this, strPropertyName))
                            setNamesOfChangedProperties.Add(strLoopChangedProperty);
                    }
                }

                if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                    return;

                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                }
            }
            finally
            {
                if (setNamesOfChangedProperties != null)
                    Utils.StringHashSetPool.Return(setNamesOfChangedProperties);
            }
        }

        private static readonly PropertyDependencyGraph<AttributeSection> s_AttributeSectionDependencyGraph =
            new PropertyDependencyGraph<AttributeSection>(
            );

        private EnhancedObservableCollection<CharacterAttrib> _colAttributes;

        public EnhancedObservableCollection<CharacterAttrib> Attributes
        {
            get
            {
                if (_colAttributes != null)
                {
                    return _colAttributes;
                }

                _colAttributes = new EnhancedObservableCollection<CharacterAttrib>
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
                    if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
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

        public static readonly ReadOnlyCollection<string> AttributeStrings = Array.AsReadOnly(new[]
            {"BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "MAGAdept", "RES", "ESS", "DEP"});

        public static readonly ReadOnlyCollection<string> PhysicalAttributes = Array.AsReadOnly(new[]
            {"BOD", "AGI", "REA", "STR"});

        public static readonly ReadOnlyCollection<string> MentalAttributes = Array.AsReadOnly(new[]
            {"CHA", "INT", "LOG", "WIL"});

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

        private readonly LockingDictionary<string, BindingSource> _dicBindings = new LockingDictionary<string, BindingSource>(AttributeStrings.Count);
        private readonly Character _objCharacter;
        private CharacterAttrib.AttributeCategory _eAttributeCategory = CharacterAttrib.AttributeCategory.Standard;

        #region Constructor, Save, Load, Print Methods

        public AttributeSection(Character character)
        {
            _objCharacter = character;
            AttributeList.CollectionChanged += AttributeListOnCollectionChanged;
            AttributeList.BeforeClearCollectionChanged += AttributeListOnBeforeClearCollectionChanged;
            SpecialAttributeList.CollectionChanged += SpecialAttributeListOnCollectionChanged;
            SpecialAttributeList.BeforeClearCollectionChanged += SpecialAttributeListOnBeforeClearCollectionChanged;
        }

        private void SpecialAttributeListOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_blnLoading)
                return;
            foreach (CharacterAttrib objAttribute in e.OldItems)
            {
                objAttribute.UnbindAttribute();
                if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                    objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
            }
        }

        private void AttributeListOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_blnLoading)
                return;
            foreach (CharacterAttrib objAttribute in e.OldItems)
            {
                objAttribute.UnbindAttribute();
                if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                    objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
            }
        }

        private void AttributeListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_blnLoading)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterAttrib objAttribute in e.NewItems)
                    {
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        objAttribute.UnbindAttribute();
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    HashSet<CharacterAttrib> setNewAttribs = e.NewItems.OfType<CharacterAttrib>().ToHashSet();
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        if (setNewAttribs.Contains(objAttribute))
                            continue;
                        objAttribute.UnbindAttribute();
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    foreach (CharacterAttrib objAttribute in setNewAttribs)
                    {
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    break;
            }
        }

        private void SpecialAttributeListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_blnLoading)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterAttrib objAttribute in e.NewItems)
                    {
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        objAttribute.UnbindAttribute();
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    HashSet<CharacterAttrib> setNewAttribs = e.NewItems.OfType<CharacterAttrib>().ToHashSet();
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        if (setNewAttribs.Contains(objAttribute))
                            continue;
                        objAttribute.UnbindAttribute();
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    foreach (CharacterAttrib objAttribute in setNewAttribs)
                    {
                        if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                            objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                    }
                    break;
            }
        }

        public void UnbindAttributeSection()
        {
            AttributeList.Clear();
            SpecialAttributeList.Clear();
        }

        public void Dispose()
        {
            foreach (BindingSource objSource in _dicBindings.Values)
                objSource.Dispose();
            _dicBindings.Dispose();
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
            if (charNode == null)
                return;
            bool blnOldLoading = _blnLoading;
            try
            {
                _blnLoading = true;
                using (_ = Timekeeper.StartSyncron("create_char_attrib", null,
                           CustomActivity.OperationType.RequestOperation, charNode.InnerText))
                {
                    int intOldBODBase = _objCharacter.BOD.Base;
                    int intOldBODKarma = _objCharacter.BOD.Karma;
                    int intOldAGIBase = _objCharacter.AGI.Base;
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
                        CommonFunctions.ExpressionToInt(charNode["bodmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["bodmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["bodaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.AGI.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["agimin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["agimax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["agiaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.REA.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["reamin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["reamax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["reaaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.STR.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["strmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["strmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["straug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.CHA.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["chamin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["chamax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["chaaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.INT.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["intmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["intmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["intaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.LOG.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["logmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["logmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["logaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.WIL.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["wilmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["wilmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["wilaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.MAG.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["magmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.RES.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["resmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["resmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["resaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.EDG.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["edgmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["edgmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["edgaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.DEP.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["depmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["depmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["depaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.MAGAdept.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["magmin"]?.InnerText, intValue, intMinModifier),
                        CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intValue, intMaxModifier),
                        CommonFunctions.ExpressionToInt(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
                    _objCharacter.ESS.AssignLimits(
                        CommonFunctions.ExpressionToInt(charNode["essmin"]?.InnerText, intValue),
                        CommonFunctions.ExpressionToInt(charNode["essmax"]?.InnerText, intValue),
                        CommonFunctions.ExpressionToInt(charNode["essaug"]?.InnerText, intValue));

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
                        Attributes = new EnhancedObservableCollection<CharacterAttrib>
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
                        if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                        {
                            _objCharacter.MAGAdept.Base =
                                Math.Min(intOldMAGAdeptBase, _objCharacter.MAGAdept.PriorityMaximum);
                            _objCharacter.MAGAdept.Karma =
                                Math.Min(intOldMAGAdeptKarma, _objCharacter.MAGAdept.KarmaMaximum);
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
                    //Timekeeper.Finish("create_char_attrib");
                }
            }
            finally
            {
                _blnLoading = blnOldLoading;
            }
        }

        public void Load(XmlNode xmlSavedCharacterNode)
        {
            if (xmlSavedCharacterNode == null)
                return;
            bool blnOldLoading = _blnLoading;
            try
            {
                _blnLoading = true;
                //Timekeeper.Start("load_char_attrib");
                AttributeList.Clear();
                SpecialAttributeList.Clear();
                XPathNavigator xmlCharNode = _objCharacter.GetNodeXPath();
                // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
                XPathNavigator xmlCharNodeAnimalForm =
                    _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created
                        ? _objCharacter.GetNodeXPath(true)
                        : null;
                foreach (string strAttribute in AttributeStrings)
                {
                    XmlNodeList lstAttributeNodes =
                        xmlSavedCharacterNode.SelectNodes("attributes/attribute[name = " + strAttribute.CleanXPath() +
                                                          ']');
                    // Couldn't find the appropriate attribute in the loaded file, so regenerate it from scratch.
                    if (lstAttributeNodes == null || lstAttributeNodes.Count == 0 || xmlCharNodeAnimalForm != null &&
                        _objCharacter.LastSavedVersion < new Version(5, 200, 25))
                    {
                        CharacterAttrib objAttribute;
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

                        if (xmlCharNodeAnimalForm == null) continue;
                        objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                            CharacterAttrib.AttributeCategory.Shapeshifter);
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
                    else
                    {
                        foreach (XmlNode xmlAttributeNode in lstAttributeNodes)
                        {
                            CharacterAttrib objAttribute;
                            switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                        CharacterAttrib.AttributeCategory.Special);
                                    objAttribute.Load(xmlAttributeNode);
                                    SpecialAttributeList.Add(objAttribute);
                                    break;

                                case CharacterAttrib.AttributeCategory.Standard:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                        CharacterAttrib.AttributeCategory.Standard);
                                    objAttribute.Load(xmlAttributeNode);
                                    AttributeList.Add(objAttribute);
                                    break;
                            }
                        }
                    }
                }
                ResetBindings();
            }
            finally
            {
                _blnLoading = blnOldLoading;
            }
            //Timekeeper.Finish("load_char_attrib");
        }

        [CLSCompliant(false)]
        public void LoadFromHeroLab(XPathNavigator xmlStatBlockBaseNode, CustomActivity parentActivity)
        {
            if (xmlStatBlockBaseNode == null)
                return;
            using (_ = Timekeeper.StartSyncron("load_char_attrib", parentActivity))
            {
                bool blnOldLoading = _blnLoading;
                try
                {
                    _blnLoading = true;
                    AttributeList.Clear();
                    SpecialAttributeList.Clear();
                    XPathNavigator xmlCharNode = _objCharacter.GetNodeXPath();
                    // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
                    XPathNavigator xmlCharNodeAnimalForm =
                        _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created
                            ? _objCharacter.GetNodeXPath(true)
                            : null;
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
                        XPathNavigator xmlHeroLabAttributeNode =
                            xmlStatBlockBaseNode.SelectSingleNode(
                                "attributes/attribute[@name = " + GetAttributeEnglishName(strAttribute).CleanXPath() +
                                ']');
                        XPathNavigator xmlAttributeBaseNode = xmlHeroLabAttributeNode?.SelectSingleNode("@base");
                        if (xmlAttributeBaseNode != null &&
                            int.TryParse(xmlAttributeBaseNode.Value, out int intHeroLabAttributeBaseValue))
                        {
                            int intAttributeMinimumValue = GetAttributeByName(strAttribute).MetatypeMinimum;
                            if (intHeroLabAttributeBaseValue == intAttributeMinimumValue) continue;
                            if (objAttribute != null)
                                objAttribute.Karma = intHeroLabAttributeBaseValue - intAttributeMinimumValue;
                        }
                    }

                    if (!_objCharacter.Created && _objCharacter.EffectiveBuildMethodUsesPriorityTables)
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
                }
                finally
                {
                    _blnLoading = blnOldLoading;
                }
                //Timekeeper.Finish("load_char_attrib");
            }
        }

        private static CharacterAttrib RemakeAttribute(CharacterAttrib objNewAttribute, XPathNavigator objCharacterNode)
        {
            if (objNewAttribute == null)
                return null;
            if (objCharacterNode == null)
                throw new ArgumentNullException(nameof(objCharacterNode));
            string strAttributeLower = objNewAttribute.Abbrev.ToLowerInvariant();
            if (strAttributeLower == "magadept")
                strAttributeLower = "mag";
            int intMinValue = 1;
            int intMaxValue = 1;
            int intAugValue = 1;

            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCharacterNode.SelectSingleNode(strAttributeLower + "min")?.Value.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1",
                    out bool blnIsSuccess);
                if (blnIsSuccess)
                    intMinValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intMinValue = 1; }
            catch (OverflowException) { intMinValue = 1; }
            catch (InvalidCastException) { intMinValue = 1; }
            try
            {
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCharacterNode.SelectSingleNode(strAttributeLower + "max")?.Value.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1",
                    out bool blnIsSuccess);
                if (blnIsSuccess)
                    intMaxValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intMaxValue = 1; }
            catch (OverflowException) { intMaxValue = 1; }
            catch (InvalidCastException) { intMaxValue = 1; }
            try
            {
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCharacterNode.SelectSingleNode(strAttributeLower + "aug")?.Value.Replace("/", " div ").Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1",
                    out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAugValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intAugValue = 1; }
            catch (OverflowException) { intAugValue = 1; }
            catch (InvalidCastException) { intAugValue = 1; }

            objNewAttribute.Base = objCharacterNode.SelectSingleNodeAndCacheExpression("base")?.ValueAsInt ?? 0;
            objNewAttribute.Karma = objCharacterNode.SelectSingleNodeAndCacheExpression("base")?.ValueAsInt ?? 0;
            objNewAttribute.AssignLimits(intMinValue, intMaxValue, intAugValue);
            return objNewAttribute;
        }

        internal void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (_objCharacter.MetatypeCategory == "Shapeshifter")
            {
                XPathNavigator xmlNode = _objCharacter.GetNodeXPath(true);

                if (AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
                {
                    objWriter.WriteElementString("attributecategory", xmlNode?.SelectSingleNodeAndCacheExpression("name/@translate")?.Value ?? _objCharacter.Metatype);
                }
                else
                {
                    xmlNode = xmlNode?.SelectSingleNode("metavariants/metavariant[id = " + _objCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath() + ']');
                    objWriter.WriteElementString("attributecategory", xmlNode?.Value ?? _objCharacter.Metavariant);
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

        #endregion Constructor, Save, Load, Print Methods

        #region Methods

        public CharacterAttrib GetAttributeByName(string abbrev)
        {
            bool blnGetShifterAttribute = _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created
                && AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter;
            CharacterAttrib objReturn
                = AttributeList.Find(att => att.Abbrev == abbrev
                                            && (att.MetatypeCategory == CharacterAttrib.AttributeCategory.Shapeshifter)
                                            == blnGetShifterAttribute)
                  ?? SpecialAttributeList.Find(att => att.Abbrev == abbrev);
            return objReturn;
        }

        public BindingSource GetAttributeBindingByName(string abbrev)
        {
            return _dicBindings.TryGetValue(abbrev, out BindingSource objAttributeBinding) ? objAttributeBinding : null;
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
            if (objSource == null || objTarget == null)
                return;
            string strSourceAbbrev = objSource.Abbrev.ToLowerInvariant();
            if (strSourceAbbrev == "magadept")
                strSourceAbbrev = "mag";
            XmlNode node = !string.IsNullOrEmpty(strMetavariantXPath) ? xmlDoc?.SelectSingleNode(strMetavariantXPath) : null;
            if (node != null)
            {
                int.TryParse(node[strSourceAbbrev + "min"]?.InnerText, NumberStyles.Any,
                             GlobalSettings.InvariantCultureInfo, out int intMinimum);
                int.TryParse(node[strSourceAbbrev + "max"]?.InnerText, NumberStyles.Any,
                             GlobalSettings.InvariantCultureInfo, out int intMaximum);
                int.TryParse(node[strSourceAbbrev + "aug"]?.InnerText, NumberStyles.Any,
                             GlobalSettings.InvariantCultureInfo, out int intAugmentedMaximum);
                intMaximum = Math.Max(intMaximum, intMinimum);
                intAugmentedMaximum = Math.Max(intAugmentedMaximum, intMaximum);
                objTarget.AssignLimits(intMinimum, intMaximum, intAugmentedMaximum);
            }

            objTarget.Base = objSource.Base;
            objTarget.Karma = objSource.Karma;
        }

        public string ProcessAttributesInXPath(string strInput, IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            string strReturn = strInput;
            foreach (string strCharAttributeName in AttributeStrings)
            {
                strReturn = strReturn
                    .CheapReplace('{' + strCharAttributeName + '}', () => (dicValueOverrides?.ContainsKey(strCharAttributeName) == true
                            ? dicValueOverrides[strCharAttributeName]
                            : _objCharacter.GetAttribute(strCharAttributeName).TotalValue).ToString(GlobalSettings.InvariantCultureInfo))
                    .CheapReplace('{' + strCharAttributeName + "Unaug}", () => (dicValueOverrides?.ContainsKey(strCharAttributeName + "Unaug") == true
                        ? dicValueOverrides[strCharAttributeName + "Unaug"]
                        : _objCharacter.GetAttribute(strCharAttributeName).Value).ToString(GlobalSettings.InvariantCultureInfo))
                    .CheapReplace('{' + strCharAttributeName + "Base}", () => (dicValueOverrides?.ContainsKey(strCharAttributeName + "Base") == true
                        ? dicValueOverrides[strCharAttributeName + "Base"]
                        : _objCharacter.GetAttribute(strCharAttributeName).TotalBase).ToString(GlobalSettings.InvariantCultureInfo));
            }
            return strReturn;
        }

        public void ProcessAttributesInXPath(StringBuilder sbdInput, string strOriginal = "", IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            foreach (string strCharAttributeName in AttributeStrings)
            {
                sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + '}', () => (dicValueOverrides?.ContainsKey(strCharAttributeName) == true
                    ? dicValueOverrides[strCharAttributeName]
                    : _objCharacter.GetAttribute(strCharAttributeName).TotalValue).ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Unaug}", () => (dicValueOverrides?.ContainsKey(strCharAttributeName + "Unaug") == true
                    ? dicValueOverrides[strCharAttributeName + "Unaug"]
                    : _objCharacter.GetAttribute(strCharAttributeName).Value).ToString(GlobalSettings.InvariantCultureInfo));
                sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Base}", () => (dicValueOverrides?.ContainsKey(strCharAttributeName + "Base") == true
                    ? dicValueOverrides[strCharAttributeName + "Base"]
                    : _objCharacter.GetAttribute(strCharAttributeName).TotalBase).ToString(GlobalSettings.InvariantCultureInfo));
            }
        }

        public string ProcessAttributesInXPathForTooltip(string strInput, CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            string strReturn = strInput;
            foreach (string strCharAttributeName in AttributeStrings)
            {
                strReturn = strReturn
                    .CheapReplace('{' + strCharAttributeName + '}', () =>
                    {
                        string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                            .DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null || !dicValueOverrides.TryGetValue(strCharAttributeName, out int intAttributeValue))
                                intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).TotalValue;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }
                        return strInnerReturn;
                    })
                    .CheapReplace('{' + strCharAttributeName + "Unaug}", () =>
                    {
                        string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                            .DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null || !dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug", out int intAttributeValue))
                                intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).Value;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            LanguageManager.GetString("String_NaturalAttribute", strLanguage), strInnerReturn);
                    })
                    .CheapReplace('{' + strCharAttributeName + "Base}", () =>
                    {
                        string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                            .DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null || !dicValueOverrides.TryGetValue(strCharAttributeName + "Base", out int intAttributeValue))
                                intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).TotalBase;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            LanguageManager.GetString("String_BaseAttribute", strLanguage), strInnerReturn);
                    });
            }
            return strReturn;
        }

        public void ProcessAttributesInXPathForTooltip(StringBuilder sbdInput, string strOriginal = "", CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            foreach (string strCharAttributeName in AttributeStrings)
            {
                sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + '}', () =>
                {
                    string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                        .DisplayNameShort(strLanguage);
                    if (blnShowValues)
                    {
                        if (dicValueOverrides == null || !dicValueOverrides.TryGetValue(strCharAttributeName, out int intAttributeValue))
                            intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).TotalValue;
                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                    }
                    return strInnerReturn;
                });
                sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Unaug}", () =>
                {
                    string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                        .DisplayNameShort(strLanguage);
                    if (blnShowValues)
                    {
                        if (dicValueOverrides == null || !dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug", out int intAttributeValue))
                            intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).Value;
                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                    }

                    return string.Format(objCultureInfo,
                        LanguageManager.GetString("String_NaturalAttribute", strLanguage), strInnerReturn);
                });
                sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Base}", () =>
                {
                    string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                        .DisplayNameShort(strLanguage);
                    if (blnShowValues)
                    {
                        if (dicValueOverrides == null || !dicValueOverrides.TryGetValue(strCharAttributeName + "Base", out int intAttributeValue))
                            intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).TotalBase;
                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                    }

                    return string.Format(objCultureInfo,
                        LanguageManager.GetString("String_BaseAttribute", strLanguage), strInnerReturn);
                });
            }
        }

        internal void Reset(bool blnFirstTime = false)
        {
            bool blnOldLoading = !blnFirstTime && _blnLoading;
            try
            {
                _blnLoading = true;
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

                if (blnFirstTime)
                {
                    foreach (BindingSource objSource in _dicBindings.Values)
                        objSource.Dispose();
                    _dicBindings.Clear();
                    foreach (string strAttributeString in AttributeStrings)
                    {
                        _dicBindings.Add(strAttributeString, new BindingSource());
                    }
                }

                ResetBindings();
            }
            finally
            {
                _blnLoading = blnOldLoading;
            }
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
            _objCharacter.RefreshAttributeBindings();
            foreach (KeyValuePair<string, BindingSource> objBindingEntry in _dicBindings)
            {
                objBindingEntry.Value.ResetBindings(false);
            }
        }

        /// <summary>
        /// Can an attribute be increased to its metatype maximum?
        /// Note: if the attribute is already at its metatype maximum, this method assumes that we just raised it and will decrease it if it's illegal.
        /// </summary>
        /// <param name="objAttribute">Attribute to raise to its metatype maximum.</param>
        /// <returns></returns>
        public bool CanRaiseAttributeToMetatypeMax(CharacterAttrib objAttribute)
        {
            if (_objCharacter.Created || _objCharacter.IgnoreRules
                                      || objAttribute.MetatypeCategory == CharacterAttrib.AttributeCategory.Special
                                      || _objCharacter.Settings.MaxNumberMaxAttributesCreate >= AttributeList.Count)
                return true;
            return AttributeList.Count(x => x.MetatypeCategory == objAttribute.MetatypeCategory && x != objAttribute && x.AtMetatypeMaximum)
                   < _objCharacter.Settings.MaxNumberMaxAttributesCreate;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Character's Attributes.
        /// </summary>
        [HubTag(true)]
        public EnhancedObservableCollection<CharacterAttrib> AttributeList { get; } = new EnhancedObservableCollection<CharacterAttrib>();

        /// <summary>
        /// Character's Attributes.
        /// </summary>
        public EnhancedObservableCollection<CharacterAttrib> SpecialAttributeList { get; } = new EnhancedObservableCollection<CharacterAttrib>();

        public CharacterAttrib.AttributeCategory AttributeCategory
        {
            get => _eAttributeCategory;
            set
            {
                if (_eAttributeCategory == value)
                    return;
                _eAttributeCategory = value;
                if (_objCharacter.Created)
                {
                    ResetBindings();
                    ForceAttributePropertyChangedNotificationAll(nameof(CharacterAttrib.MetatypeMaximum),
                                                                 nameof(CharacterAttrib.MetatypeMinimum));
                }
                OnPropertyChanged();
            }
        }

        #endregion Properties
    }
}
