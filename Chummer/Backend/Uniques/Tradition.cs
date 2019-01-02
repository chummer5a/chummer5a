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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.helpers;

namespace Chummer.Backend.Uniques
{
    public enum TraditionType
    {
        None,
        MAG,
        RES
    }
    /// <summary>
    /// A Tradition
    /// </summary>
    [HubClassTag("SourceID", true, "Name")]
    public class Tradition : IHasInternalId, IHasName, IHasXmlNode, IHasSource, INotifyMultiplePropertyChanged
    {
        private Guid _guiID;
        private string _strSourceGuid;
        private string _strName = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strDrainExpression = string.Empty;
        private string _strSpiritForm = "Materialization";
        private string _strSpiritCombat = string.Empty;
        private string _strSpiritDetection = string.Empty;
        private string _strSpiritHealth = string.Empty;
        private string _strSpiritIllusion = string.Empty;
        private string _strSpiritManipulation = string.Empty;
        private readonly List<string> _lstAvailableSpirits = new List<string>();
        private XmlNode _nodBonus;
        private TraditionType _eTraditionType = TraditionType.None;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public Tradition(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _objCharacter.PropertyChanged += RefreshDrainExpression;
        }

        public void UnbindTradition()
        {
            _objCharacter.PropertyChanged -= RefreshDrainExpression;
        }

        public void ResetTradition()
        {
            Bonus = null;
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Tradition, InternalId);
            Name = string.Empty;
            Extra = string.Empty;
            Source = string.Empty;
            _strPage = string.Empty;
            DrainExpression = string.Empty;
            SpiritForm = "Materialization";
            AvailableSpirits.Clear();
            Type = TraditionType.None;
            _objCachedSourceDetail = null;
        }

        /// Create a Tradition from an XmlNode.
        /// <param name="xmlTraditionNode">XmlNode to create the object from.</param>
        /// <param name="blnIsTechnomancerTradition">Whether or not this tradition is for a technomancer.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public bool Create(XmlNode xmlTraditionNode, bool blnIsTechnomancerTradition = false, string strForcedValue = "")
        {
            ResetTradition();
            Type = blnIsTechnomancerTradition ? TraditionType.RES : TraditionType.MAG;
            if (xmlTraditionNode.TryGetStringFieldQuickly("id", ref _strSourceGuid))
                _xmlCachedMyXmlNode = null;
            xmlTraditionNode.TryGetStringFieldQuickly("name", ref _strName);
            xmlTraditionNode.TryGetStringFieldQuickly("source", ref _strSource);
            xmlTraditionNode.TryGetStringFieldQuickly("page", ref _strPage);
            string strTemp = string.Empty;
            if (xmlTraditionNode.TryGetStringFieldQuickly("drain", ref strTemp))
                DrainExpression = strTemp;
            if (xmlTraditionNode.TryGetStringFieldQuickly("spiritform", ref strTemp))
                SpiritForm = strTemp;
            _nodBonus = xmlTraditionNode["bonus"];
            if (_nodBonus != null)
            {
                int intRating = _objCharacter.SubmersionGrade > 0 ? _objCharacter.SubmersionGrade : _objCharacter.InitiateGrade;

                string strOldFocedValue = ImprovementManager.ForcedValue;
                string strOldSelectedValue = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForcedValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Tradition, _guiID.ToString("D"), _nodBonus, true, intRating, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    ImprovementManager.ForcedValue = strOldFocedValue;
                    return false;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                ImprovementManager.ForcedValue = strOldFocedValue;
                ImprovementManager.SelectedValue = strOldSelectedValue;
            }
            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetTextFromPDF($"{_strSource} {_strPage}", _strName);
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPDF($"{Source} {Page(GlobalOptions.Language)}", DisplayName(GlobalOptions.Language));
                }
            }*/
            RebuildSpiritList();
            OnMultiplePropertyChanged(nameof(Name), nameof(Extra), nameof(Source), nameof(Page));
            return true;
        }

        

        public void RebuildSpiritList(bool blnDoOnPropertyChanged = true)
        {
            _lstAvailableSpirits.Clear();
            _strSpiritCombat = string.Empty;
            _strSpiritDetection = string.Empty;
            _strSpiritHealth = string.Empty;
            _strSpiritIllusion = string.Empty;
            _strSpiritManipulation = string.Empty;
            if (Type != TraditionType.None)
            {
                XmlNode xmlSpiritListNode = GetNode()?["spirits"];
                if (xmlSpiritListNode != null)
                {
                    using (XmlNodeList xmlAlwaysAccessSpirits = xmlSpiritListNode.SelectNodes("spirit"))
                    {
                        if (xmlAlwaysAccessSpirits?.Count > 0)
                        {
                            foreach (XmlNode xmlSpiritNode in xmlAlwaysAccessSpirits)
                            {
                                _lstAvailableSpirits.Add(xmlSpiritNode.InnerText);
                            }
                        }
                    }

                    XmlNode xmlCombatSpiritNode = xmlSpiritListNode.SelectSingleNode("spiritcombat");
                    if (xmlCombatSpiritNode != null)
                        _strSpiritCombat = xmlCombatSpiritNode.InnerText;
                    XmlNode xmlDetectionSpiritNode = xmlSpiritListNode.SelectSingleNode("spiritdetection");
                    if (xmlDetectionSpiritNode != null)
                        _strSpiritDetection = xmlDetectionSpiritNode.InnerText;
                    XmlNode xmlHealthSpiritNode = xmlSpiritListNode.SelectSingleNode("spirithealth");
                    if (xmlHealthSpiritNode != null)
                        _strSpiritHealth = xmlHealthSpiritNode.InnerText;
                    XmlNode xmlIllusionSpiritNode = xmlSpiritListNode.SelectSingleNode("spiritillusion");
                    if (xmlIllusionSpiritNode != null)
                        _strSpiritIllusion = xmlIllusionSpiritNode.InnerText;
                    XmlNode xmlManipulationSpiritNode = xmlSpiritListNode.SelectSingleNode("spiritmanipulation");
                    if (xmlManipulationSpiritNode != null)
                        _strSpiritManipulation = xmlManipulationSpiritNode.InnerText;
                }
            }
            if (blnDoOnPropertyChanged)
            {
                OnMultiplePropertyChanged(nameof(AvailableSpirits), nameof(SpiritCombat), nameof(SpiritDetection), nameof(SpiritHealth), nameof(SpiritIllusion), nameof(SpiritManipulation));
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (_eTraditionType == TraditionType.None)
                return;
            objWriter.WriteStartElement("tradition");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("traditiontype", _eTraditionType.ToString());
            objWriter.WriteElementString("id", _strSourceGuid);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("spiritform", _strSpiritForm);
            objWriter.WriteElementString("drain", _strDrainExpression);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("spiritcombat", _strSpiritCombat);
            objWriter.WriteElementString("spiritdetection", _strSpiritDetection);
            objWriter.WriteElementString("spirithealth", _strSpiritHealth);
            objWriter.WriteElementString("spiritillusion", _strSpiritIllusion);
            objWriter.WriteElementString("spiritmanipulation", _strSpiritManipulation);
            objWriter.WriteStartElement("spirits");
            foreach (string strSpirit in _lstAvailableSpirits)
            {
                objWriter.WriteElementString("spirit", strSpirit);
            }
            objWriter.WriteEndElement();
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Tradition from the XmlNode.
        /// </summary>
        /// <param name="xmlNode">XmlNode to load.</param>
        public void Load(XmlNode xmlNode)
        {
            string strTemp = string.Empty;
            if (!xmlNode.TryGetStringFieldQuickly("traditiontype", ref strTemp) || !Enum.TryParse(strTemp, out _eTraditionType))
            {
                _eTraditionType = TraditionType.None;
                return;
            }
            xmlNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (xmlNode.TryGetStringFieldQuickly("id", ref _strSourceGuid))
                _xmlCachedMyXmlNode = null;
            xmlNode.TryGetStringFieldQuickly("name", ref _strName);
            xmlNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            xmlNode.TryGetStringFieldQuickly("spiritform", ref _strSpiritForm);
            xmlNode.TryGetStringFieldQuickly("drain", ref _strDrainExpression);
            xmlNode.TryGetStringFieldQuickly("source", ref _strSource);
            xmlNode.TryGetStringFieldQuickly("page", ref _strPage);
            xmlNode.TryGetStringFieldQuickly("spiritcombat", ref _strSpiritCombat);
            xmlNode.TryGetStringFieldQuickly("spiritdetection", ref _strSpiritDetection);
            xmlNode.TryGetStringFieldQuickly("spirithealth", ref _strSpiritHealth);
            xmlNode.TryGetStringFieldQuickly("spiritillusion", ref _strSpiritIllusion);
            xmlNode.TryGetStringFieldQuickly("spiritmanipulation", ref _strSpiritManipulation);
            using (XmlNodeList xmlSpiritList = xmlNode.SelectNodes("spirits/spirit"))
            {
                if (xmlSpiritList?.Count > 0)
                {
                    foreach (XmlNode xmlSpiritNode in xmlSpiritList)
                    {
                        _lstAvailableSpirits.Add(xmlSpiritNode.InnerText);
                    }
                }
            }
            _nodBonus = xmlNode["bonus"];
        }

        /// <summary>
        /// Load the Tradition from the XmlNode using old data saved before traditions had their own class.
        /// </summary>
        /// <param name="xpathCharacterNode">XPathNavigator of the Character from which to load.</param>
        public void LegacyLoad(XPathNavigator xpathCharacterNode)
        {
            if (_eTraditionType == TraditionType.RES)
            {
                xpathCharacterNode.TryGetStringFieldQuickly("stream", ref _strName);
                xpathCharacterNode.TryGetStringFieldQuickly("streamfading", ref _strDrainExpression);
            }
            else
            {
                if (IsCustomTradition)
                {
                    xpathCharacterNode.TryGetStringFieldQuickly("traditionname", ref _strName);
                    xpathCharacterNode.TryGetStringFieldQuickly("spiritcombat", ref _strSpiritCombat);
                    xpathCharacterNode.TryGetStringFieldQuickly("spiritdetection", ref _strSpiritDetection);
                    xpathCharacterNode.TryGetStringFieldQuickly("spirithealth", ref _strSpiritHealth);
                    xpathCharacterNode.TryGetStringFieldQuickly("spiritillusion", ref _strSpiritIllusion);
                    xpathCharacterNode.TryGetStringFieldQuickly("spiritmanipulation", ref _strSpiritManipulation);
                }
                else
                    xpathCharacterNode.TryGetStringFieldQuickly("tradition", ref _strName);
                xpathCharacterNode.TryGetStringFieldQuickly("traditiondrain", ref _strDrainExpression);
            }
        }

        public void LoadFromHeroLab(XmlNode xmlHeroLabNode)
        {
            _eTraditionType = TraditionType.MAG;
            _strName = xmlHeroLabNode.SelectSingleNode("@name")?.InnerText;
            XmlNode xmlTraditionDataNode = !string.IsNullOrEmpty(_strName) ? XmlManager.Load("traditions.xml").SelectSingleNode("/chummer/traditions/tradition[name = \"" + _strName + "\"]") : null;
            if (xmlTraditionDataNode?.TryGetStringFieldQuickly("id", ref _strSourceGuid) != true)
            {
                _strSourceGuid = CustomMagicalTraditionGuid;
                xmlTraditionDataNode = GetNode();
            }
            Create(xmlTraditionDataNode);
            if (IsCustomTradition)
            {
                _strSpiritCombat = xmlHeroLabNode.SelectSingleNode("@combatspirits")?.InnerText;
                _strSpiritDetection = xmlHeroLabNode.SelectSingleNode("@detectionspirits")?.InnerText;
                _strSpiritHealth = xmlHeroLabNode.SelectSingleNode("@healthspirits")?.InnerText;
                _strSpiritIllusion = xmlHeroLabNode.SelectSingleNode("@illusionspirits")?.InnerText;
                _strSpiritManipulation = xmlHeroLabNode.SelectSingleNode("@manipulationspirits")?.InnerText;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("tradition");
            objWriter.WriteElementString("istechnomancertradition", (Type == TraditionType.RES).ToString());
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            if (Type == TraditionType.MAG)
            {
                objWriter.WriteElementString("spiritcombat", DisplaySpiritCombatMethod(strLanguageToPrint));
                objWriter.WriteElementString("spiritdetection", DisplaySpiritDetectionMethod(strLanguageToPrint));
                objWriter.WriteElementString("spirithealth", DisplaySpiritHealthMethod(strLanguageToPrint));
                objWriter.WriteElementString("spiritillusion", DisplaySpiritIllusionMethod(strLanguageToPrint));
                objWriter.WriteElementString("spiritmanipulation", DisplaySpiritManipulationMethod(strLanguageToPrint));
                objWriter.WriteElementString("spiritform", DisplaySpiritForm(strLanguageToPrint));
            }
            objWriter.WriteElementString("drainattributes", DisplayDrainExpressionMethod(strLanguageToPrint));
            objWriter.WriteElementString("drainvalue", DrainValue.ToString(objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties

        public Guid SourceID { get { return Guid.Parse(_strSourceGuid); } }

        /// <summary>
        /// Internal identifier which will be used to identify this Tradition in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// GUID used to identify the original data node of this tradition
        /// </summary>
        /// <remarks>So why not make it an Guid?</remarks>
        public string strSourceID => Type == TraditionType.None ? string.Empty : _strSourceGuid;

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == null)
                {
                    string strSource = Source;
                    string strPage = Page(GlobalOptions.Language);
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        _objCachedSourceDetail = new SourceString(strSource, strPage, GlobalOptions.Language);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                    }
                }

                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public TraditionType Type
        {
            get => _eTraditionType;
            set
            {
                if (_eTraditionType != value)
                {
                    _xmlCachedMyXmlNode = null;
                    _eTraditionType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The GUID of the Custom entry in the Magical Tradition file
        /// </summary>
        public const string CustomMagicalTraditionGuid = "616ba093-306c-45fc-8f41-0b98c8cccb46";

        /// <summary>
        /// Whether or not a Tradition is a custom one (i.e. it has a custom name and custom spirit settings)
        /// </summary>
        public bool IsCustomTradition => strSourceID == CustomMagicalTraditionGuid; // TODO: If Custom Technomancer Tradition added to streams.xml, check for that GUID as well

        /// <summary>
        /// Tradition name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (IsCustomTradition)
            {
                if (GlobalOptions.Language != strLanguage)
                {
                    string strReturnEnglish = strLanguage == GlobalOptions.DefaultLanguage ? Name : LanguageManager.ReverseTranslateExtra(Name, GlobalOptions.Language);
                    return LanguageManager.TranslateExtra(strReturnEnglish, strLanguage);
                }

                return LanguageManager.TranslateExtra(Name, strLanguage);
            }

            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';

            return strReturn;
        }

        /// <summary>
        /// What type of forms do spirits of these traditions come in? Defaults to Materialization.
        /// </summary>
        public string SpiritForm
        {
            get => _strSpiritForm;
            set => _strSpiritForm = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// The spirit form of the tradition as it should be displayed in printouts and the UI.
        /// </summary>
        public string DisplaySpiritForm(string strLanguage)
        {
            return LanguageManager.TranslateExtra(SpiritForm, strLanguage);
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes.
        /// </summary>
        public string DrainExpression
        {
            get
            {
                if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                {
                    return "BOD + WIL";
                }

                return _strDrainExpression;
            }
            set
            {
                if (_strDrainExpression != value)
                {
                    foreach (string strOldDrainAttribute in AttributeSection.AttributeStrings)
                    {
                        if (_strDrainExpression.Contains(strOldDrainAttribute))
                            _objCharacter.GetAttribute(strOldDrainAttribute).PropertyChanged -= RefreshDrainValue;
                    }

                    _strDrainExpression = value;
                    foreach (string strNewDrainAttribute in AttributeSection.AttributeStrings)
                    {
                        if (value.Contains(strNewDrainAttribute))
                            _objCharacter.GetAttribute(strNewDrainAttribute).PropertyChanged += RefreshDrainValue;
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public string DisplayDrainExpression => DisplayDrainExpressionMethod(GlobalOptions.Language);

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public string DisplayDrainExpressionMethod(string strLanguage)
        {
            string strDrain = DrainExpression;
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                strDrain = strDrain.CheapReplace(strAttribute, () =>
                {
                    if (strAttribute == "MAGAdept")
                        return LanguageManager.GetString("String_AttributeMAGShort", strLanguage) +
                               LanguageManager.GetString("String_Space", strLanguage) + '(' +
                               LanguageManager.GetString("String_DescAdept", strLanguage) + ')';

                    return LanguageManager.GetString($"String_Attribute{strAttribute}Short", strLanguage);
                });
            }

            return strDrain;
        }

        /// <summary>
        /// Magician's total amount of dice for resisting drain.
        /// </summary>
        public int DrainValue
        {
            get
            {
                if (Type == TraditionType.None)
                    return 0;
                string strDrainAttributes = DrainExpression;
                StringBuilder strbldDrain = new StringBuilder(strDrainAttributes);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                    strbldDrain.CheapReplace(strDrainAttributes, objAttrib.Abbrev, () => objAttrib.TotalValue.ToString());
                }

                string strDrain = strbldDrain.ToString();
                if (!int.TryParse(strDrain, out int intDrain))
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strDrain, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intDrain = Convert.ToInt32(objProcess);
                }

                // Add any Improvements for Drain Resistance.
                if (Type == TraditionType.RES)
                    intDrain += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.FadingResistance);
                else
                    intDrain += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DrainResistance);

                return intDrain;
            }
        }

        public string DrainValueToolTip
        {
            get
            {
                if (Type == TraditionType.None)
                    return string.Empty;
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(DrainExpression);

                // Update the Fading CharacterAttribute Value.
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    objToolTip.CheapReplace(strAttribute, () =>
                    {
                        CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                        return objAttrib.DisplayAbbrev + strSpaceCharacter + '(' +
                               objAttrib.TotalValue.ToString(GlobalOptions.CultureInfo) + ')';
                    });
                }

                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if ((Type == TraditionType.RES && objLoopImprovement.ImproveType == Improvement.ImprovementType.FadingResistance ||
                        Type == TraditionType.MAG && objLoopImprovement.ImproveType == Improvement.ImprovementType.DrainResistance) &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          _objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
            }
        }

        public void RefreshDrainExpression(object sender, PropertyChangedEventArgs e)
        {
            if (Type == TraditionType.MAG && (e.PropertyName == nameof(Character.AdeptEnabled) || e.PropertyName == nameof(Character.MagicianEnabled)))
                OnPropertyChanged(nameof(DrainExpression));
        }

        public void RefreshDrainValue(object sender, PropertyChangedEventArgs e)
        {
            if (Type != TraditionType.None && e.PropertyName == nameof(CharacterAttrib.TotalValue))
                OnPropertyChanged(nameof(DrainValue));
        }

        public IList<string> AvailableSpirits => _lstAvailableSpirits;

        /// <summary>
        /// Magician's Combat Spirit (for Custom Traditions) in English.
        /// </summary>
        public string SpiritCombat
        {
            get
            {
                if (Type == TraditionType.None)
                    return string.Empty;
                return _strSpiritCombat;
            }
            set
            {
                if (Type != TraditionType.None && _strSpiritCombat != value)
                {
                    _strSpiritCombat = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Combat Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritCombatMethod(string strLanguage)
        {
            if (string.IsNullOrEmpty(SpiritCombat))
                return LanguageManager.GetString("String_None", strLanguage);
            return LanguageManager.TranslateExtra(SpiritCombat, strLanguage);
        }

        /// <summary>
        /// Magician's Combat Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritCombat
        {
            get => DisplaySpiritCombatMethod(GlobalOptions.Language);
            set
            {
                if (Type != TraditionType.None)
                    SpiritCombat = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Magician's Detection Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritDetection
        {
            get
            {
                if (Type == TraditionType.None)
                    return string.Empty;
                return _strSpiritDetection;
            }
            set
            {
                if (Type != TraditionType.None && _strSpiritDetection != value)
                {
                    _strSpiritDetection = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Detection Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritDetectionMethod(string strLanguage)
        {
            if (string.IsNullOrEmpty(SpiritDetection))
                return LanguageManager.GetString("String_None", strLanguage);
            return LanguageManager.TranslateExtra(SpiritDetection, strLanguage);
        }

        /// <summary>
        /// Magician's Detection Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritDetection
        {
            get => DisplaySpiritDetectionMethod(GlobalOptions.Language);
            set
            {
                if (Type != TraditionType.None)
                    SpiritDetection = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Magician's Health Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritHealth
        {
            get
            {
                if (Type == TraditionType.None)
                    return string.Empty;
                return _strSpiritHealth;
            }
            set
            {
                if (Type != TraditionType.None && _strSpiritHealth != value)
                {
                    _strSpiritHealth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Health Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritHealthMethod(string strLanguage)
        {
            if (string.IsNullOrEmpty(SpiritHealth))
                return LanguageManager.GetString("String_None", strLanguage);
            return LanguageManager.TranslateExtra(SpiritHealth, strLanguage);
        }

        /// <summary>
        /// Magician's Health Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritHealth
        {
            get => DisplaySpiritHealthMethod(GlobalOptions.Language);
            set
            {
                if (Type != TraditionType.None)
                    SpiritHealth = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Magician's Illusion Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritIllusion
        {
            get
            {
                if (Type == TraditionType.None)
                    return string.Empty;
                return _strSpiritIllusion;
            }
            set
            {
                if (Type != TraditionType.None && _strSpiritIllusion != value)
                {
                    _strSpiritIllusion = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Illusion Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritIllusionMethod(string strLanguage)
        {
            if (string.IsNullOrEmpty(SpiritIllusion))
                return LanguageManager.GetString("String_None", strLanguage);
            return LanguageManager.TranslateExtra(SpiritIllusion, strLanguage);
        }

        /// <summary>
        /// Magician's Illusion Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritIllusion
        {
            get => DisplaySpiritIllusionMethod(GlobalOptions.Language);
            set
            {
                if (Type != TraditionType.None)
                    SpiritIllusion = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Magician's Manipulation Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritManipulation
        {
            get
            {
                if (Type == TraditionType.None)
                    return string.Empty;
                return _strSpiritManipulation;
            }
            set
            {
                if (Type != TraditionType.None && _strSpiritManipulation != value)
                {
                    _strSpiritManipulation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Manipulation Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritManipulationMethod(string strLanguage)
        {
            if (string.IsNullOrEmpty(SpiritManipulation))
                return LanguageManager.GetString("String_None", strLanguage);
            return LanguageManager.TranslateExtra(SpiritManipulation, strLanguage);
        }

        /// <summary>
        /// Magician's Manipulation Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritManipulation
        {
            get => DisplaySpiritManipulationMethod(GlobalOptions.Language);
            set
            {
                if (Type != TraditionType.None)
                    SpiritManipulation = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }
        
        private XmlNode _xmlCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (Type == TraditionType.None)
                return null;
            if (_xmlCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _xmlCachedMyXmlNode = GetTraditionDocument(strLanguage).SelectSingleNode("/chummer/traditions/tradition[id = \"" + strSourceID + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _xmlCachedMyXmlNode;
        }

        public XmlDocument GetTraditionDocument(string strLanguage)
        {
            switch (Type)
            {
                case TraditionType.MAG:
                    return XmlManager.Load("traditions.xml", strLanguage);
                case TraditionType.RES:
                    return XmlManager.Load("streams.xml", strLanguage);
                default:
                    return null;
            }
        }
        #endregion

        #region static
        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly DependancyGraph<string> AttributeDependancyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(DisplayName),
                    new DependancyGraphNode<string>(nameof(DisplayNameShort),
                        new DependancyGraphNode<string>(nameof(Name))
                    ),
                    new DependancyGraphNode<string>(nameof(Extra))
                ),
                new DependancyGraphNode<string>(nameof(DrainValueToolTip),
                    new DependancyGraphNode<string>(nameof(DrainValue),
                        new DependancyGraphNode<string>(nameof(DrainExpression))
                    )
                ),
                new DependancyGraphNode<string>(nameof(DisplayDrainExpression),
                    new DependancyGraphNode<string>(nameof(DrainExpression))
                ),
                new DependancyGraphNode<string>(nameof(AvailableSpirits),
                    new DependancyGraphNode<string>(nameof(SpiritCombat)),
                    new DependancyGraphNode<string>(nameof(SpiritDetection)),
                    new DependancyGraphNode<string>(nameof(SpiritHealth)),
                    new DependancyGraphNode<string>(nameof(SpiritIllusion)),
                    new DependancyGraphNode<string>(nameof(SpiritManipulation))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpiritCombat),
                    new DependancyGraphNode<string>(nameof(SpiritCombat))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpiritDetection),
                    new DependancyGraphNode<string>(nameof(SpiritDetection))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpiritHealth),
                    new DependancyGraphNode<string>(nameof(SpiritHealth))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpiritIllusion),
                    new DependancyGraphNode<string>(nameof(SpiritIllusion))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpiritManipulation),
                    new DependancyGraphNode<string>(nameof(SpiritManipulation))
                )
            );

        public static List<Tradition> GetTraditions(Character character)
        {
            List<Tradition> result = new List<Tradition>();
            XmlNodeList xmlTraditions = XmlManager.Load("traditions.xml").SelectNodes("/chummer/traditions/tradition");
            foreach(XmlNode node in xmlTraditions)
            {
                Tradition tradition = new Tradition(character);
                tradition.Create(node);
                result.Add(tradition);
            }
            return result;

        }

        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }

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
                    lstNamesOfChangedProperties = AttributeDependancyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in AttributeDependancyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }

            _objCharacter?.OnPropertyChanged(nameof(Character.MagicTradition));
        }
    }
}
