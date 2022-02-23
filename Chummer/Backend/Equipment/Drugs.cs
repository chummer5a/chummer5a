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
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer.Backend.Equipment
{
    public class Drug : IHasName, IHasXmlDataNode, ICanSort, IHasStolenProperty, ICanRemove
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strAvailability = "0";
        private string _strDuration;
        private string _strDescription = string.Empty;
        private string _strEffectDescription = string.Empty;
        private readonly Dictionary<string, decimal> _dicCachedAttributes = new Dictionary<string, decimal>();
        private readonly List<string> _lstCachedInfos = new List<string>();
        private readonly Dictionary<string, int> _dicCachedLimits = new Dictionary<string, int>();
        private readonly List<XmlNode> _lstCachedQualities = new List<XmlNode>();
        private string _strGrade = string.Empty;
        private decimal _decCost;
        private int _intAddictionThreshold;
        private int _intAddictionRating;
        private const int _intSpeed = 9;
        private decimal _decQty;
        private int _intSortOrder;
        private readonly Character _objCharacter;
        private bool _blnStolen;
        private bool _blnCachedAttributeFlag;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage;
        private string _strSource;
        private string _strPage;
        private int _intDurationDice;

        #region Constructor, Create, Save, Load, and Print Methods

        public Drug(Character objCharacter)
        {
            _objCharacter = objCharacter;
            // Create the GUID for the new Drug.
            _guiID = Guid.NewGuid();
            Components.CollectionChanged += ComponentsChanged;
        }

        private void ComponentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _intCachedCrashDamage = int.MinValue;
            _intCachedDuration = int.MinValue;
            _intCachedInitiative = int.MinValue;
            _intCachedInitiativeDice = int.MinValue;
            _intCachedSpeed = int.MinValue;
            _blnCachedQualityFlag = false;
            _blnCachedLimitFlag = false;
            _blnCachedAttributeFlag = false;
            _strDescription = string.Empty;
        }

        public void Create(XmlNode objXmlData)
        {
            objXmlData.TryGetField("guid", Guid.TryParse, out _guiID);
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objXmlData["sourceid"] == null || !objXmlData.TryGetField("sourceid", Guid.TryParse, out _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetField("id", Guid.TryParse, out _guiSourceID);
            }
            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlData.TryGetDecFieldQuickly("quantity", ref _decQty);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            objXmlData.TryGetStringFieldQuickly("grade", ref _strGrade);
            objXmlData.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objXmlData.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objXmlData.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlData.TryGetInt32FieldQuickly("durationdice", ref _intDurationDice);
            DurationTimescale = CommonFunctions.ConvertStringToTimescale(objXmlData["timescale"]?.InnerText);

            objXmlData.TryGetField("source", out _strSource);
            objXmlData.TryGetField("page", out _strPage);
        }

        public void Load(XmlNode objXmlData)
        {
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            Grade = Grade.ConvertToCyberwareGrade(objXmlData["grade"]?.InnerText, Improvement.ImprovementSource.Drug, _objCharacter);

            XmlNodeList xmlComponentsNodeList = objXmlData.SelectNodes("drugcomponents/drugcomponent");
            if (xmlComponentsNodeList?.Count > 0)
            {
                foreach (XmlNode objXmlLevel in xmlComponentsNodeList)
                {
                    DrugComponent c = new DrugComponent(_objCharacter);
                    c.Load(objXmlLevel);
                    Components.Add(c);
                }
            }

            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlData.TryGetDecFieldQuickly("quantity", ref _decQty);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            objXmlData.TryGetStringFieldQuickly("grade", ref _strGrade);
            objXmlData.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objXmlData.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objXmlData.TryGetField("source", out _strSource);
            objXmlData.TryGetField("page", out _strPage);
        }

        public void Save(XmlWriter objXmlWriter)
        {
            if (objXmlWriter == null)
                return;
            objXmlWriter.WriteStartElement("drug");
            objXmlWriter.WriteElementString("sourceid", SourceIDString);
            objXmlWriter.WriteElementString("guid", InternalId);
            objXmlWriter.WriteElementString("name", _strName);
            objXmlWriter.WriteElementString("category", _strCategory);
            objXmlWriter.WriteElementString("quantity", _decQty.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteStartElement("drugcomponents");
            foreach (DrugComponent objDrugComponent in Components)
            {
                objXmlWriter.WriteStartElement("drugcomponent");
                objDrugComponent.Save(objXmlWriter);
                objXmlWriter.WriteEndElement();
            }
            objXmlWriter.WriteEndElement();
            objXmlWriter.WriteElementString("availability", _strAvailability);
            if (_decCost != 0)
                objXmlWriter.WriteElementString("cost", _decCost.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionRating != 0)
                objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString(GlobalSettings.InvariantCultureInfo));
            if (Grade != null)
                objXmlWriter.WriteElementString("grade", Grade.Name);
            objXmlWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("source", _strSource);
            objXmlWriter.WriteElementString("page", _strPage);
            objXmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("drug");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            if (Grade != null)
                objWriter.WriteElementString("grade", Grade.DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("qty", Quantity.ToString("#,0.##", objCulture));
            objWriter.WriteElementString("addictionthreshold", AddictionThreshold.ToString(objCulture));
            objWriter.WriteElementString("addictionrating", AddictionRating.ToString(objCulture));
            objWriter.WriteElementString("initiative", Initiative.ToString(objCulture));
            objWriter.WriteElementString("initiativedice", InitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("speed", Speed.ToString(objCulture));
            objWriter.WriteElementString("duration", Duration.ToString(objCulture));
            objWriter.WriteElementString("crashdamage", CrashDamage.ToString(objCulture));
            objWriter.WriteElementString("avail", TotalAvail(GlobalSettings.CultureInfo, strLanguageToPrint));
            objWriter.WriteElementString("avail_english", TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.DefaultLanguage));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));

            objWriter.WriteStartElement("attributes");
            foreach (KeyValuePair<string, decimal> objAttribute in Attributes)
            {
                if (objAttribute.Value != 0)
                {
                    objWriter.WriteStartElement("attribute");
                    objWriter.WriteElementString("name", LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short", strLanguageToPrint));
                    objWriter.WriteElementString("name_english", objAttribute.Key);
                    objWriter.WriteElementString("value", objAttribute.Value.ToString("+#.#;-#.#", objCulture));
                    objWriter.WriteEndElement();
                }
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("limits");
            foreach (KeyValuePair<string, int> objLimit in Limits)
            {
                if (objLimit.Value != 0)
                {
                    objWriter.WriteStartElement("limit");
                    objWriter.WriteElementString("name", LanguageManager.GetString("Node_" + objLimit.Key, strLanguageToPrint));
                    objWriter.WriteElementString("name_english", objLimit.Key);
                    objWriter.WriteElementString("value", objLimit.Value.ToString("+#;-#", objCulture));
                    objWriter.WriteEndElement();
                }
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("qualities");
            foreach (string strQualityText in Qualities.Select(x => x.InnerText))
            {
                objWriter.WriteStartElement("quality");
                objWriter.WriteElementString("name", _objCharacter.TranslateExtra(strQualityText, strLanguageToPrint));
                objWriter.WriteElementString("name_english", strQualityText);
                objWriter.WriteEndElement();
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("infos");
            foreach (string strInfo in Infos)
            {
                objWriter.WriteStartElement("info");
                objWriter.WriteElementString("name", _objCharacter.TranslateExtra(strInfo, strLanguageToPrint));
                objWriter.WriteElementString("name_english", strInfo);
                objWriter.WriteEndElement();
            }
            objWriter.WriteEndElement();

            if (GlobalSettings.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            objWriter.WriteEndElement();
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this item.
        /// </summary>
        public string InternalId => _guiID.ToString();

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade { get; set; }

        /// <summary>
        /// Compiled description of the drug.
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_strDescription))
                    _strDescription = GenerateDescription(0);
                return _strDescription;
            }
            set => _strDescription = value;
        }

        /// <summary>
        /// Compiled description of the drug's Effects.
        /// </summary>
        public string EffectDescription
        {
            get
            {
                if (string.IsNullOrEmpty(_strEffectDescription))
                    _strEffectDescription = GenerateDescription(0, true);
                return _strEffectDescription;
            }
            set => _strEffectDescription = value;
        }

        /// <summary>
        /// Components of the Drug.
        /// </summary>
        public EnhancedObservableCollection<DrugComponent> Components { get; } = new EnhancedObservableCollection<DrugComponent>();

        /// <summary>
        /// Name of the Drug.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("gear.xml").SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Category of the Drug.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        private decimal _decCachedCost = decimal.MinValue;

        /// <summary>
        /// Base cost of the Drug.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (_decCachedCost != decimal.MinValue) return _decCachedCost;
                _decCachedCost = Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.CostPerLevel);
                return _decCachedCost;
            }
        }

        /// <summary>
        /// Total cost of the Drug.
        /// </summary>
        public decimal TotalCost => Cost * Quantity;

        /// <summary>
        /// Total cost of the Drug.
        /// </summary>
        public decimal StolenTotalCost => Stolen ? TotalCost : 0;

        /// <summary>
        /// Total amount of the Drug held by the character.
        /// </summary>
        public decimal Quantity
        {
            get => _decQty;
            set => _decQty = value;
        }

        /// <summary>
        /// Availability of the Drug.
        /// </summary>
        public string Availability => _strAvailability;

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Availability;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));

                    /*
                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                    }
                    */

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString(), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intAvail += ((double) objProcess).StandardRound();
                }
            }
            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                foreach (AvailabilityValue objLoopAvail in Components.Select(x => x.TotalAvailTuple))
                {
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        private int _intCachedAddictionThreshold = int.MinValue;

        /// <summary>
        /// Addiction Threshold of the Drug.
        /// </summary>
        public int AddictionThreshold
        {
            get
            {
                if (_intCachedAddictionThreshold != int.MinValue) return _intCachedAddictionThreshold;
                _intCachedAddictionThreshold = Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.AddictionThreshold);
                return _intCachedAddictionThreshold;
            }
        }

        private int _intCachedAddictionRating = int.MinValue;

        /// <summary>
        /// Addiction Rating of the Drug.
        /// </summary>
        public int AddictionRating
        {
            get
            {
                if (_intCachedAddictionRating != int.MinValue) return _intCachedAddictionRating;
                _intCachedAddictionRating = Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.AddictionRating);
                return _intCachedAddictionRating;
            }
        }

        private bool _blnCachedLimitFlag;

        public Dictionary<string, int> Limits
        {
            get
            {
                if (_blnCachedLimitFlag)
                    return _dicCachedLimits;
                _dicCachedLimits.Clear();
                foreach (KeyValuePair<string, int> kvpLimit in Components.Where(d => d.ActiveDrugEffect?.Limits.Count > 0).SelectMany(d => d.ActiveDrugEffect.Limits))
                {
                    if (_dicCachedLimits.TryGetValue(kvpLimit.Key, out int intExistingValue))
                        _dicCachedLimits[kvpLimit.Key] = intExistingValue + kvpLimit.Value;
                    else
                        _dicCachedLimits.Add(kvpLimit.Key, kvpLimit.Value);
                }
                _blnCachedLimitFlag = true;
                return _dicCachedLimits;
            }
        }

        private bool _blnCachedQualityFlag;

        public List<XmlNode> Qualities
        {
            get
            {
                if (_blnCachedQualityFlag)
                    return _lstCachedQualities;
                foreach (XmlNode objEffect in Components.Where(d => d.ActiveDrugEffect != null).SelectMany(d => d.ActiveDrugEffect.Qualities))
                {
                    if (!_lstCachedQualities.Contains(objEffect))
                        _lstCachedQualities.Add(objEffect);
                }

                _blnCachedQualityFlag = true;
                return _lstCachedQualities;
            }
        }

        private bool _blnCachedInfoFlag;

        public List<string> Infos
        {
            get
            {
                if (_blnCachedInfoFlag)
                    return _lstCachedInfos;
                foreach (string strInfo in Components.Where(d => d.ActiveDrugEffect != null).SelectMany(d => d.ActiveDrugEffect.Infos))
                {
                    if (!_lstCachedInfos.Contains(strInfo))
                        _lstCachedInfos.Add(strInfo);
                }

                _blnCachedInfoFlag = true;
                return _lstCachedInfos;
            }
        }

        private int _intCachedInitiative = int.MinValue;

        public int Initiative
        {
            get
            {
                if (_intCachedInitiative != int.MinValue) return _intCachedInitiative;
                _intCachedInitiative = Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.ActiveDrugEffect.Initiative);
                return _intCachedInitiative;
            }
        }

        private int _intCachedInitiativeDice = int.MinValue;

        public int InitiativeDice
        {
            get
            {
                if (_intCachedInitiativeDice != int.MinValue) return _intCachedInitiativeDice;
                _intCachedInitiativeDice = Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.ActiveDrugEffect.InitiativeDice);
                return _intCachedInitiativeDice;
            }
        }

        private int _intCachedSpeed = int.MinValue;

        /// <summary>
        /// How quickly the Drug takes effect, in seconds. A Combat Turn is considered
        /// to be 3 seconds, so anything with a Speed below 3 is considered to be Immediate.
        /// </summary>
        public int Speed
        {
            get
            {
                if (_intCachedSpeed != int.MinValue) return _intCachedSpeed;
                _intCachedSpeed = Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.ActiveDrugEffect.Speed) + _intSpeed;
                return _intCachedSpeed;
            }
        }

        private int _intCachedDuration = int.MinValue;

        public int Duration
        {
            get
            {
                if (_intCachedDuration != int.MinValue)
                    return _intCachedDuration;
                if (string.IsNullOrWhiteSpace(_strDuration))
                    return _intCachedDuration = 0;

                string strDuration;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDrain))
                {
                    sbdDrain.Append(_strDuration);
                    // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdDrain, _strDuration);
                    strDuration = sbdDrain.ToString();
                }

                if (!decimal.TryParse(strDuration, out decimal decDuration))
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strDuration, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        decDuration = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                decDuration += Components.Where(d => d.ActiveDrugEffect != null).Sum(d => d.ActiveDrugEffect.Duration) +
                               ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DrugDuration);
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DrugDurationMultiplier) == 0)
                    return _intCachedDuration = decDuration.StandardRound();
                decimal decMultiplier = 1;
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.DrugDurationMultiplier))
                {
                    decMultiplier -= 1.0m - objImprovement.Value / 100m;
                }

                return _intCachedDuration = (decDuration * (1.0m - decMultiplier)).StandardRound();
            }
        }

        public CommonFunctions.Timescale DurationTimescale { get; private set; }

        private string _strCachedDisplayDuration;

        public string DisplayDuration
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_strCachedDisplayDuration))
                    return _strCachedDisplayDuration;
                string strDisplayDuration = string.Empty;
                if (Duration > 0)
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    strDisplayDuration += Duration.ToString(GlobalSettings.CultureInfo) + strSpace;
                    if (DurationDice > 0)
                    {
                        strDisplayDuration += 'x' + strSpace + DurationDice.ToString(GlobalSettings.CultureInfo) +
                                              LanguageManager.GetString("String_D6") + strSpace;
                    }
                }

                strDisplayDuration += CommonFunctions.GetTimescaleString(DurationTimescale, Duration > 1);
                _strCachedDisplayDuration = strDisplayDuration;

                return _strCachedDisplayDuration;
            }
        }

        public int DurationDice { get; set; }

        private int _intCachedCrashDamage = int.MinValue;

        public int CrashDamage
        {
            get
            {
                if (_intCachedCrashDamage != int.MinValue) return _intCachedCrashDamage;
                _intCachedCrashDamage = Components.Sum(d => d.ActiveDrugEffect?.CrashDamage ?? 0);
                return _intCachedCrashDamage;
            }
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        public string Notes { get; internal set; }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                ? Name
                : _objCharacter.TranslateExtra(Name, strLanguage);
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            if (Quantity != 1)
                strReturn = Quantity.ToString("#,0.##", objCulture) + LanguageManager.GetString("String_Space", strLanguage) + strReturn;
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Dictionary<string, decimal> Attributes
        {
            get
            {
                if (_blnCachedAttributeFlag)
                    return _dicCachedAttributes;
                _dicCachedAttributes.Clear();
                foreach (DrugComponent objComponent in Components)
                {
                    foreach (DrugEffect objDrugEffect in objComponent.DrugEffects)
                    {
                        if (objDrugEffect.Level == objComponent.Level && objDrugEffect.Attributes.Count > 0)
                        {
                            foreach (KeyValuePair<string, decimal> objAttributeEntry in objDrugEffect.Attributes)
                            {
                                if (_dicCachedAttributes.TryGetValue(objAttributeEntry.Key, out decimal decExistingValue))
                                    _dicCachedAttributes[objAttributeEntry.Key] = decExistingValue + objAttributeEntry.Value;
                                else
                                    _dicCachedAttributes.Add(objAttributeEntry.Key, objAttributeEntry.Value);
                            }
                        }
                    }
                }
                _blnCachedAttributeFlag = true;
                return _dicCachedAttributes;
            }
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.HasNotesColor
                : ColorManager.WindowText;

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        #endregion Properties

        #region UI Methods

        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public TreeNode CreateTreeNode()
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;

            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }

        #endregion UI Methods

        #region Methods

        public string GenerateDescription(int intLevel = -1, bool blnEffectsOnly = false, string strLanguage = "", CultureInfo objCulture = null, bool blnDoCache = true)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            if (objCulture == null)
                objCulture = GlobalSettings.CultureInfo;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDescription))
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                if (!blnEffectsOnly)
                {
                    string strName = DisplayNameShort(strLanguage);
                    if (!string.IsNullOrWhiteSpace(strName))
                        sbdDescription.AppendLine(strName);
                }

                if (intLevel != -1)
                {
                    bool blnNewLineFlag = false;
                    foreach (KeyValuePair<string, decimal> objAttribute in Attributes)
                    {
                        if (objAttribute.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription
                                .Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short",
                                                                  strLanguage)).Append(strSpace)
                                .Append(objAttribute.Value.ToString("+#.#;-#.#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        blnNewLineFlag = false;
                        sbdDescription.AppendLine();
                    }

                    foreach (KeyValuePair<string, int> objLimit in Limits)
                    {
                        if (objLimit.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key, strLanguage))
                                          .Append(strSpace)
                                          .Append(LanguageManager.GetString("String_Limit", strLanguage))
                                          .Append(strSpace)
                                          .Append(objLimit.Value.ToString(" +#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        sbdDescription.AppendLine();
                    }

                    if (Initiative != 0 || InitiativeDice != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("String_AttributeINILong", strLanguage))
                                      .Append(strSpace);
                        if (Initiative != 0)
                        {
                            sbdDescription.Append(Initiative.ToString("+#;-#", GlobalSettings.CultureInfo));
                            if (InitiativeDice != 0)
                                sbdDescription.Append(InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                              .Append(LanguageManager.GetString("String_D6", strLanguage));
                        }
                        else if (InitiativeDice != 0)
                            sbdDescription.Append(InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                          .Append(LanguageManager.GetString("String_D6", strLanguage));

                        sbdDescription.AppendLine();
                    }

                    foreach (XmlNode nodQuality in Qualities)
                    {
                        sbdDescription.Append(_objCharacter.TranslateExtra(nodQuality.InnerText, strLanguage))
                                      .Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Quality", strLanguage));
                    }

                    foreach (string strInfo in Infos)
                        sbdDescription.AppendLine(_objCharacter.TranslateExtra(strInfo, strLanguage));

                    if (Category == "Custom Drug" || Duration != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_Duration", strLanguage))
                                      .AppendLine(DisplayDuration);

                    if (Category == "Custom Drug" || Speed != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("Label_Speed"))
                                      .Append(LanguageManager.GetString("String_Colon", strLanguage)).Append(strSpace);
                        if (Speed <= 0)
                            sbdDescription.AppendLine(LanguageManager.GetString("String_Immediate"));
                        else if (Speed <= 60)
                            sbdDescription.Append((Speed / 3).ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                          .AppendLine(LanguageManager.GetString("String_CombatTurns"));
                        else
                            sbdDescription.Append(Speed.ToString(GlobalSettings.CultureInfo))
                                          .AppendLine(LanguageManager.GetString("String_Seconds"));
                    }

                    if (CrashDamage != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_CrashEffect", strLanguage))
                                      .Append(strSpace)
                                      .Append(CrashDamage.ToString(objCulture))
                                      .Append(LanguageManager.GetString("String_DamageStun", strLanguage))
                                      .Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_DamageUnresisted", strLanguage));
                    if (!blnEffectsOnly)
                    {
                        sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating", strLanguage))
                                      .Append(strSpace)
                                      .AppendLine((AddictionRating * (intLevel + 1)).ToString(objCulture))
                                      .Append(LanguageManager.GetString("Label_AddictionThreshold", strLanguage))
                                      .Append(strSpace)
                                      .AppendLine((AddictionThreshold * (intLevel + 1)).ToString(objCulture))
                                      .Append(LanguageManager.GetString("Label_Cost", strLanguage)).Append(strSpace)
                                      .Append((Cost * (intLevel + 1)).ToString(
                                                  _objCharacter.Settings.NuyenFormat, objCulture)).AppendLine('¥')
                                      .Append(LanguageManager.GetString("Label_Avail", strLanguage)).Append(strSpace)
                                      .AppendLine(TotalAvail(objCulture, strLanguage));
                    }
                }
                else if (!blnEffectsOnly)
                {
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating", strLanguage))
                                  .Append(strSpace)
                                  .AppendLine(0.ToString(objCulture))
                                  .Append(LanguageManager.GetString("Label_AddictionThreshold", strLanguage))
                                  .Append(strSpace).AppendLine(0.ToString(objCulture))
                                  .Append(LanguageManager.GetString("Label_Cost", strLanguage)).Append(strSpace)
                                  .Append((Cost * (intLevel + 1)).ToString(
                                              _objCharacter.Settings.NuyenFormat, objCulture))
                                  .AppendLine('¥')
                                  .Append(LanguageManager.GetString("Label_Avail", strLanguage)).Append(strSpace)
                                  .AppendLine(TotalAvail(objCulture, strLanguage));
                }

                string strReturn = sbdDescription.ToString();
                if (blnDoCache)
                    _strDescription = strReturn;
                return strReturn;
            }
        }

        /// <summary>
        /// Creates the improvements necessary to 'activate' a given drug.
        /// TODO: I'm really not happy with the lack of extensibility on this.
        /// TODO: Refactor drug effects to just use XML nodes, which can then be passed to Improvement Manager?
        /// TODO: Refactor Improvement Manager to automatically collapse improvements of the same type into a single improvement?
        /// </summary>
        public void GenerateImprovement()
        {
            if (_objCharacter.Improvements.Any(ig => ig.SourceName == InternalId))
                return;
            _objCharacter.ImprovementGroups.Add(Name);
            string strSpace = LanguageManager.GetString("String_Space");
            string strNamePrefix = CurrentDisplayNameShort + strSpace + '-' + strSpace;
            List<Improvement> lstImprovements = Attributes.Where(objAttribute => objAttribute.Value != 0)
                .Select(objAttribute => new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    ImproveType = Improvement.ImprovementType.Attribute,
                    SourceName = InternalId,
                    Augmented = objAttribute.Value,
                    ImprovedName = objAttribute.Key,
                    CustomName = strNamePrefix + LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short")
                                               + strSpace + objAttribute.Value.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo)
                }).ToList();

            foreach (KeyValuePair<string, int> objLimit in Limits)
            {
                if (objLimit.Value == 0) continue;
                Improvement i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    Value = objLimit.Value,
                    CustomName = strNamePrefix + LanguageManager.GetString("Node_" + objLimit.Key)
                                               + strSpace + objLimit.Value.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo)
                };
                switch (objLimit.Key)
                {
                    case "Physical":
                        i.ImproveType = Improvement.ImprovementType.PhysicalLimit;
                        break;

                    case "Mental":
                        i.ImproveType = Improvement.ImprovementType.MentalLimit;
                        break;

                    case "Social":
                        i.ImproveType = Improvement.ImprovementType.SocialLimit;
                        break;
                }
                lstImprovements.Add(i);
            }

            if (Initiative != 0)
            {
                Improvement i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.Initiative,
                    Value = Initiative,
                    CustomName = strNamePrefix + LanguageManager.GetString("String_Initiative")
                                               + strSpace + Initiative.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo)
                };
                lstImprovements.Add(i);
            }

            if (InitiativeDice != 0)
            {
                Improvement i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.InitiativeDice,
                    Value = InitiativeDice,
                    CustomName = strNamePrefix + LanguageManager.GetString("String_InitiativeDice")
                                               + strSpace + InitiativeDice.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo)
                };
                lstImprovements.Add(i);
            }

            if (Qualities.Count > 0)
            {
                XmlDocument objXmlDocument = _objCharacter.LoadData("qualities.xml");
                foreach (XmlNode objXmlAddQuality in Qualities)
                {
                    XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = " + objXmlAddQuality.InnerText.CleanXPath() + ']');
                    if (objXmlSelectedQuality == null)
                        continue;
                    XPathNavigator xpnSelectedQuality = objXmlSelectedQuality.CreateNavigator();
                    string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerText ?? string.Empty;

                    string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerText;
                    int intCount = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, 1);
                    bool blnDoesNotContributeToBP = !string.Equals(objXmlAddQuality.Attributes?["contributetobp"]?.InnerText, bool.TrueString, StringComparison.CurrentCultureIgnoreCase);

                    for (int i = 0; i < intCount; ++i)
                    {
                        // Makes sure we aren't over our limits for this particular quality from this overall source
                        if (objXmlAddQuality.Attributes?["forced"]?.InnerText == bool.TrueString ||
                            xpnSelectedQuality.RequirementsMet(_objCharacter, LanguageManager.GetString("String_Quality"), string.Empty, Name))
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality objAddQuality = new Quality(_objCharacter);
                            objAddQuality.Create(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons, strForceValue, Name);

                            if (blnDoesNotContributeToBP)
                            {
                                objAddQuality.BP = 0;
                                objAddQuality.ContributeToLimit = false;
                            }

                            _objCharacter.Qualities.Add(objAddQuality);
                            foreach (Weapon objWeapon in lstWeapons)
                                _objCharacter.Weapons.Add(objWeapon);
                            Improvement objImprovement = new Improvement(_objCharacter)
                            {
                                ImprovedName = objAddQuality.InternalId,
                                ImproveSource = Improvement.ImprovementSource.Drug,
                                SourceName = InternalId,
                                ImproveType = Improvement.ImprovementType.SpecificQuality,
                                CustomName = strNamePrefix + LanguageManager.GetString("String_InitiativeDice")
                                                           + strSpace + objAddQuality.Name
                            };
                            lstImprovements.Add(objImprovement);
                        }
                        else
                        {
                            throw new AbortedException();
                        }
                    }
                }
            }
            foreach (Improvement i in lstImprovements)
            {
                i.CustomGroup = Name;
                i.Custom = true;
                i.Enabled = false;
                // This is initially set to false make sure no property changers are triggered
                i.SetupComplete = true;
            }
            _objCharacter.Improvements.AddRange(lstImprovements);
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                            && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("drugcomponents.xml", strLanguage)
                    : await _objCharacter.LoadDataAsync("drugcomponents.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/drugcomponents/drugcomponent[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/drugcomponents/drugcomponent[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant()
                                                        .CleanXPath()
                                        + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("drugcomponents.xml", strLanguage)
                    : await _objCharacter.LoadDataXPathAsync("drugcomponents.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/drugcomponents/drugcomponent[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/drugcomponents/drugcomponent[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant()
                                                        .CleanXPath()
                                        + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteDrug")))
            {
                return false;
            }
            _objCharacter.Drugs.Remove(this);
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Drug, InternalId);
            return true;
        }

        #endregion Methods
    }

    /// <summary>
    /// Drug Component.
    /// </summary>
    public class DrugComponent : IHasName, IHasInternalId, IHasXmlDataNode
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guidId;
        private Guid _guiSourceID;
        private string _strName;
        private string _strCategory;
        private string _strAvailability = "0";
        private int _intLevel;
        private int _intLimit = 1;
        private string _strSource;
        private string _strPage;
        private string _strCost;
        private int _intAddictionThreshold;
        private int _intAddictionRating;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage;
        private readonly Character _objCharacter;

        public DrugComponent(Character objCharacter)
        {
            _guidId = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        #region Constructor, Create, Save, Load, and Print Methods

        public void Load(XmlNode objXmlData)
        {
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objXmlData.TryGetField("internalid", Guid.TryParse, out _guidId);
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            XmlNodeList xmlEffectsList = objXmlData.SelectNodes("effects/effect");
            if (xmlEffectsList?.Count > 0)
            {
                foreach (XmlNode objXmlLevel in xmlEffectsList)
                {
                    DrugEffect objDrugEffect = new DrugEffect();
                    objXmlLevel.TryGetField("level", out int effectLevel);
                    objDrugEffect.Level = effectLevel;
                    XmlNodeList xmlEffectChildNodeList = objXmlLevel.SelectNodes("*");
                    if (xmlEffectChildNodeList?.Count > 0)
                    {
                        foreach (XmlNode objXmlEffect in xmlEffectChildNodeList)
                        {
                            string strEffectName = string.Empty;
                            objXmlEffect.TryGetStringFieldQuickly("name", ref strEffectName);
                            switch (objXmlEffect.Name)
                            {
                                case "attribute":
                                    {
                                        int intEffectValue = 0;
                                        if (!string.IsNullOrEmpty(strEffectName) && objXmlEffect.TryGetInt32FieldQuickly("value", ref intEffectValue))
                                            objDrugEffect.Attributes[strEffectName] = intEffectValue;
                                    }
                                    break;

                                case "limit":
                                    {
                                        int intEffectValue = 0;
                                        if (!string.IsNullOrEmpty(strEffectName) && objXmlEffect.TryGetInt32FieldQuickly("value", ref intEffectValue))
                                            objDrugEffect.Limits[strEffectName] = intEffectValue;
                                        break;
                                    }
                                case "quality":
                                    objDrugEffect.Qualities.Add(objXmlEffect);
                                    break;

                                case "info":
                                    objDrugEffect.Infos.Add(objXmlEffect.InnerText);
                                    break;

                                case "initiative":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.Initiative = intInnerText;
                                        break;
                                    }
                                case "initiativedice":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.InitiativeDice = intInnerText;
                                        break;
                                    }
                                case "crashdamage":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.CrashDamage = intInnerText;
                                        break;
                                    }
                                case "speed":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.Speed = intInnerText;
                                        break;
                                    }
                                case "duration":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.Duration = intInnerText;
                                        break;
                                    }
                                default:
                                    Log.Warn("Unknown drug effect " + objXmlEffect.Name + " in component " + strEffectName);
                                    break;
                            }
                        }
                    }

                    DrugEffects.Add(objDrugEffect);
                }
            }

            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlData.TryGetInt32FieldQuickly("level", ref _intLevel);
            objXmlData.TryGetInt32FieldQuickly("limit", ref _intLimit);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            objXmlData.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlData.TryGetStringFieldQuickly("page", ref _strPage);
        }

        public void Save(XmlWriter objXmlWriter)
        {
            if (objXmlWriter == null)
                return;
            objXmlWriter.WriteElementString("sourceid", SourceIDString);
            objXmlWriter.WriteElementString("guid", InternalId);
            objXmlWriter.WriteElementString("name", _strName);
            objXmlWriter.WriteElementString("category", _strCategory);

            objXmlWriter.WriteStartElement("effects");
            foreach (DrugEffect objDrugEffect in DrugEffects)
            {
                objXmlWriter.WriteStartElement("effect");
                foreach (KeyValuePair<string, decimal> objAttribute in objDrugEffect.Attributes)
                {
                    objXmlWriter.WriteStartElement("attribute");
                    objXmlWriter.WriteElementString("name", objAttribute.Key);
                    objXmlWriter.WriteElementString("value", objAttribute.Value.ToString(GlobalSettings.InvariantCultureInfo));
                    objXmlWriter.WriteEndElement();
                }
                foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
                {
                    objXmlWriter.WriteStartElement("limit");
                    objXmlWriter.WriteElementString("name", objLimit.Key);
                    objXmlWriter.WriteElementString("value", objLimit.Value.ToString(GlobalSettings.InvariantCultureInfo));
                    objXmlWriter.WriteEndElement();
                }
                foreach (XmlNode nodQuality in objDrugEffect.Qualities)
                {
                    objXmlWriter.WriteRaw("<quality>" + nodQuality.InnerXml + "</quality>");
                }
                foreach (string strInfo in objDrugEffect.Infos)
                {
                    objXmlWriter.WriteElementString("info", strInfo);
                }
                if (objDrugEffect.Initiative != 0)
                    objXmlWriter.WriteElementString("initiative", objDrugEffect.Initiative.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.InitiativeDice != 0)
                    objXmlWriter.WriteElementString("initiativedice", objDrugEffect.InitiativeDice.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.Duration != 0)
                    objXmlWriter.WriteElementString("duration", objDrugEffect.Duration.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.Speed != 0)
                    objXmlWriter.WriteElementString("speed", objDrugEffect.Speed.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.CrashDamage != 0)
                    objXmlWriter.WriteElementString("crashdamage", objDrugEffect.CrashDamage.ToString(GlobalSettings.InvariantCultureInfo));
                objXmlWriter.WriteEndElement();
            }
            objXmlWriter.WriteEndElement();

            objXmlWriter.WriteElementString("availability", _strAvailability);
            objXmlWriter.WriteElementString("cost", _strCost);
            objXmlWriter.WriteElementString("level", _intLevel.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("limit", _intLimit.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionRating != 0)
                objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("source", _strSource);
            objXmlWriter.WriteElementString("page", _strPage);
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Drug Component's English Name
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator xmlGearDataNode = this.GetNodeXPath(strLanguage);
            if (xmlGearDataNode?.SelectSingleNode("name")?.Value == "Custom Item")
            {
                return _objCharacter.TranslateExtra(Name, strLanguage);
            }

            return xmlGearDataNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Level X).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            if (Level != 0)
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                strReturn += strSpace + '(' + LanguageManager.GetString("String_Level", strLanguage) + strSpace + Level.ToString(objCulture) + ')';
            }
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("drugcomponents.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Category
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
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
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        public List<DrugEffect> DrugEffects { get; } = new List<DrugEffect>();

        public DrugEffect ActiveDrugEffect => DrugEffects.Find(effect => effect.Level == Level);

        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Cost of the drug component per level
        /// </summary>
        public decimal CostPerLevel
        {
            get
            {
                string strCostExpression = Cost;
                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                if (strCostExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCostExpression = strValues[Math.Max(Math.Min(Level, strValues.Length) - 1, 0)].Trim('[', ']');
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    sbdCost.Replace("Level", Level.ToString(GlobalSettings.InvariantCultureInfo));
                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                                 _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev,
                                             () => objLoopAttribute.TotalValue.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                             () => objLoopAttribute.TotalBase.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                    }

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString(), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        public string Availability
        {
            get => _strAvailability;
            set => _strAvailability = value;
        }

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple.ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple
        {
            get
            {
                bool blnModifyParentAvail = false;
                string strAvail = Availability;
                char chrLastAvailChar = ' ';
                int intAvail = 0;
                if (strAvail.Length > 0)
                {
                    chrLastAvailChar = strAvail[strAvail.Length - 1];
                    if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                    {
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }

                    blnModifyParentAvail = strAvail.StartsWith('+', '-');
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));

                        /*
                        foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                            sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                        }
                        */

                        object objProcess
                            = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString(), out bool blnIsSuccess);
                        if (blnIsSuccess)
                            intAvail += ((double) objProcess).StandardRound();
                    }
                }

                if (intAvail < 0)
                    intAvail = 0;

                return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
            }
        }

        public int AddictionThreshold
        {
            get => _intAddictionThreshold;
            set => _intAddictionThreshold = value;
        }

        public int AddictionRating
        {
            get => _intAddictionRating;
            set => _intAddictionRating = value;
        }

        public int Level
        {
            get => _intLevel;
            set => _intLevel = value;
        }

        /// <summary>
        /// Amount of this drug component that is allowed to be in a complete drug recipe. If 0, assume unlimited.
        /// </summary>
        public int Limit
        {
            get => _intLimit;
            set => _intLimit = value;
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public string InternalId => _guidId.ToString("D", GlobalSettings.InvariantCultureInfo);

        #endregion Properties

        #region Methods

        public string GenerateDescription(int intLevel = -1)
        {
            if (intLevel >= DrugEffects.Count)
                return null;

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDescription))
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strColon = LanguageManager.GetString("String_Colon");
                sbdDescription.Append(DisplayCategory(GlobalSettings.Language)).Append(strColon).Append(strSpace)
                              .AppendLine(CurrentDisplayName);

                if (intLevel != -1)
                {
                    DrugEffect objDrugEffect = DrugEffects[intLevel];
                    bool blnNewLineFlag = false;
                    foreach (KeyValuePair<string, decimal> objAttribute in objDrugEffect.Attributes)
                    {
                        if (objAttribute.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription
                                .Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short"))
                                .Append(strSpace)
                                .Append(objAttribute.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        blnNewLineFlag = false;
                        sbdDescription.AppendLine();
                    }

                    foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
                    {
                        if (objLimit.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key)).Append(strSpace)
                                          .Append(LanguageManager.GetString("String_Limit")).Append(strSpace)
                                          .Append(objLimit.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        sbdDescription.AppendLine();
                    }

                    if (objDrugEffect.Initiative != 0 || objDrugEffect.InitiativeDice != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("String_AttributeINILong")).Append(strSpace);
                        if (objDrugEffect.Initiative != 0)
                        {
                            sbdDescription.Append(
                                objDrugEffect.Initiative.ToString("+#;-#", GlobalSettings.CultureInfo));
                            if (objDrugEffect.InitiativeDice != 0)
                                sbdDescription
                                    .Append(objDrugEffect.InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                    .Append(LanguageManager.GetString("String_D6"));
                        }
                        else if (objDrugEffect.InitiativeDice != 0)
                            sbdDescription
                                .Append(objDrugEffect.InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                .Append(LanguageManager.GetString("String_D6"));

                        sbdDescription.AppendLine();
                    }

                    foreach (XmlNode strQuality in objDrugEffect.Qualities)
                        sbdDescription.Append(_objCharacter.TranslateExtra(strQuality.InnerText)).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Quality"));
                    foreach (string strInfo in objDrugEffect.Infos)
                        sbdDescription.AppendLine(_objCharacter.TranslateExtra(strInfo));

                    if (Category == "Custom Drug" || objDrugEffect.Duration != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_Duration")).Append(strColon)
                                      .Append(strSpace)
                                      .Append("10 ⨯ ")
                                      .Append((objDrugEffect.Duration + 1).ToString(GlobalSettings.CultureInfo))
                                      .Append(LanguageManager.GetString("String_D6")).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Minutes"));

                    if (Category == "Custom Drug" || objDrugEffect.Speed != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("Label_Speed")).Append(strColon)
                                      .Append(strSpace);
                        if (objDrugEffect.Speed <= 0)
                            sbdDescription.AppendLine(LanguageManager.GetString("String_Immediate"));
                        else if (objDrugEffect.Speed <= 60)
                            sbdDescription.Append((objDrugEffect.Speed / 3).ToString(GlobalSettings.CultureInfo))
                                          .Append(strSpace).AppendLine(LanguageManager.GetString("String_CombatTurns"));
                        else
                            sbdDescription.Append((objDrugEffect.Speed).ToString(GlobalSettings.CultureInfo))
                                          .AppendLine(LanguageManager.GetString("String_Seconds"));
                    }

                    if (objDrugEffect.CrashDamage != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_CrashEffect")).Append(strSpace)
                                      .Append(objDrugEffect.CrashDamage.ToString(GlobalSettings.CultureInfo))
                                      .Append(LanguageManager.GetString("String_DamageStun")).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_DamageUnresisted"));

                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating")).Append(strSpace)
                                  .AppendLine((AddictionRating * (intLevel + 1)).ToString(GlobalSettings.CultureInfo));
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionThreshold")).Append(strSpace)
                                  .AppendLine(
                                      (AddictionThreshold * (intLevel + 1)).ToString(GlobalSettings.CultureInfo));
                    sbdDescription.Append(LanguageManager.GetString("Label_Cost")).Append(strSpace)
                                  .Append((CostPerLevel * (intLevel + 1)).ToString(
                                              _objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo))
                                  .AppendLine('¥');
                    sbdDescription.Append(LanguageManager.GetString("Label_Avail")).Append(strSpace)
                                  .AppendLine(DisplayTotalAvail);
                }
                else
                {
                    string strPerLevel = LanguageManager.GetString("String_PerLevel");
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating")).Append(strSpace)
                                  .Append(0.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                  .AppendLine(strPerLevel);
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionThreshold")).Append(strSpace)
                                  .Append(0.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                  .AppendLine(strPerLevel);
                    sbdDescription.Append(LanguageManager.GetString("Label_Cost")).Append(strSpace)
                                  .Append((CostPerLevel * (intLevel + 1)).ToString(
                                              _objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo))
                                  .Append('¥')
                                  .Append(strSpace).AppendLine(strPerLevel);
                    sbdDescription.Append(LanguageManager.GetString("Label_Avail")).Append(strSpace)
                                  .AppendLine(DisplayTotalAvail);
                }

                return sbdDescription.ToString();
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                            && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("drugcomponents.xml", strLanguage)
                    : await _objCharacter.LoadDataAsync("drugcomponents.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/drugcomponents/drugcomponent[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/drugcomponents/drugcomponent[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant()
                                                        .CleanXPath()
                                        + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("drugcomponents.xml", strLanguage)
                    : await _objCharacter.LoadDataXPathAsync("drugcomponents.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/drugcomponents/drugcomponent[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/drugcomponents/drugcomponent[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant()
                                                        .CleanXPath()
                                        + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        #endregion Methods
    }

    /// <summary>
    /// Drug Effect
    /// </summary>
    public class DrugEffect
    {
        public DrugEffect()
        {
            Attributes = new Dictionary<string, decimal>();
            Limits = new Dictionary<string, int>();
            Qualities = new List<XmlNode>();
            Infos = new List<string>();
        }

        public Dictionary<string, decimal> Attributes { get; }

        public Dictionary<string, int> Limits { get; }

        public List<XmlNode> Qualities { get; }

        public List<string> Infos { get; }

        public int Initiative { get; set; }

        public int InitiativeDice { get; set; }

        public int CrashDamage { get; set; }

        public int Speed { get; set; }

        public int Duration { get; set; }

        public int Level { get; set; }
    }
}
