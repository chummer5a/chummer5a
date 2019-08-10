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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Attributes;
using NLog;
using Chummer.Classes;

namespace Chummer.Backend.Equipment
{
    public class Drug : IHasName, IHasXmlNode, ICanSort, IHasStolenProperty, ICanRemove
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = "";
        private string _strCategory = "";
        private string _strAvailability = "0";
        private string _strDuration;
        private string _strDescription = string.Empty;
        private string _strEffectDescription = string.Empty;
        private Dictionary<string, int> _dicCachedAttributes = new Dictionary<string, int>();
        private List<string> _lstCachedInfos = new List<string>();
        private Dictionary<string, int> _dicCachedLimits = new Dictionary<string, int>();
        private List<XmlNode> _lstCachedQualities = new List<XmlNode>();
        private string _strGrade = "";
        private decimal _decCost;
        private int _intAddictionThreshold;
        private int _intAddictionRating;
        private int _intSpeed = 9;
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
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objXmlData["sourceid"] == null || !objXmlData.TryGetField("sourceid", Guid.TryParse, out _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetField("id", Guid.TryParse, out _guiSourceID);
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
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
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

            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
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
            objXmlWriter.WriteStartElement("drug");
            objXmlWriter.WriteElementString("sourceid", SourceIDString);
            objXmlWriter.WriteElementString("guid", InternalId);
            objXmlWriter.WriteElementString("name", _strName);
            objXmlWriter.WriteElementString("category", _strCategory);
            objXmlWriter.WriteElementString("quantity", _decQty.ToString(GlobalOptions.InvariantCultureInfo));
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
                objXmlWriter.WriteElementString("cost", _decCost.ToString(GlobalOptions.InvariantCultureInfo));
            if (_intAddictionRating != 0)
                objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString());
            if (_intAddictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString());
            objXmlWriter.WriteElementString("grade", Grade.Name);
            objXmlWriter.WriteElementString("sortorder", _intSortOrder.ToString());
            objXmlWriter.WriteElementString("stolen", _blnStolen.ToString());
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
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("drug");

            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("grade", Grade.DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("qty", Quantity.ToString( "#,0.##", objCulture));
            objWriter.WriteElementString("addictionthreshold", AddictionThreshold.ToString(objCulture));
            objWriter.WriteElementString("addictionrating", AddictionRating.ToString(objCulture));
            objWriter.WriteElementString("initiative", Initiative.ToString(objCulture));
            objWriter.WriteElementString("initiativedice", InitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("speed", Speed.ToString(objCulture));
            objWriter.WriteElementString("duration", Duration.ToString(objCulture));
            objWriter.WriteElementString("crashdamage", CrashDamage.ToString(objCulture));
            objWriter.WriteElementString("avail", TotalAvail(GlobalOptions.CultureInfo, strLanguageToPrint));
            objWriter.WriteElementString("avail_english", TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));

            objWriter.WriteStartElement("attributes");
            foreach (KeyValuePair<string, int> objAttribute in Attributes)
            {
                if (objAttribute.Value != 0)
                {
                    objWriter.WriteStartElement("attribute");
                    objWriter.WriteElementString("name", LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short", strLanguageToPrint));
                    objWriter.WriteElementString("name_english", objAttribute.Key);
                    objWriter.WriteElementString("value", objAttribute.Value.ToString("+#;-#"));
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
                    objWriter.WriteElementString("value", objLimit.Value.ToString("+#;-#"));
                    objWriter.WriteEndElement();
                }
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("qualities");
            foreach (XmlNode nodQuality in Qualities)
            {
                objWriter.WriteStartElement("quality");
                objWriter.WriteElementString("name", LanguageManager.TranslateExtra(nodQuality.InnerText, strLanguageToPrint));
                objWriter.WriteElementString("name_english", nodQuality.InnerText);
                objWriter.WriteEndElement();
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("infos");
            foreach (string strInfo in Infos)
            {
                objWriter.WriteStartElement("info");
                objWriter.WriteElementString("name", LanguageManager.TranslateExtra(strInfo, strLanguageToPrint));
                objWriter.WriteElementString("name_english", strInfo);
                objWriter.WriteEndElement();
            }
            objWriter.WriteEndElement();

            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            objWriter.WriteEndElement();
        }
        #endregion
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
		public ObservableCollection<DrugComponent> Components { get; set; } = new ObservableCollection<DrugComponent>();

        /// <summary>
        /// Name of the Drug.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("gear.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
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
                _decCachedCost = Components.Sum(d => d.CostPerLevel);
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
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                /*
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }*/

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }
            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                foreach (DrugComponent objComponent in Components)
                {
                    AvailabilityValue objLoopAvail = objComponent.TotalAvailTuple();
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
                _intCachedAddictionThreshold = Components.Sum(d => d.AddictionThreshold);
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
                _intCachedAddictionRating = Components.Sum(d => d.AddictionRating);
                return _intCachedAddictionRating;
            }
        }

        private bool _blnCachedLimitFlag;
        public Dictionary<string, int> Limits
        {
            get
            {
                if (_blnCachedLimitFlag) return _dicCachedLimits;
                _dicCachedLimits = Components.Where(d => d.ActiveDrugEffect?.Limits.Count > 0)
                    .SelectMany(d => d.ActiveDrugEffect.Limits)
                    .GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
                _blnCachedLimitFlag = true;
                return _dicCachedLimits;

            }
        }

        private bool _blnCachedQualityFlag;
        public List<XmlNode> Qualities
	    {
	        get
	        {
	            if (_blnCachedQualityFlag) return _lstCachedQualities;
	            foreach (DrugComponent d in Components.Where(d => d.ActiveDrugEffect != null))
	            {
	                _lstCachedQualities.AddRange(d.ActiveDrugEffect.Qualities);
                }

	            _lstCachedQualities = _lstCachedQualities.Distinct().ToList();
	            _blnCachedQualityFlag = true;
                return _lstCachedQualities;
	        }
	    }

        private bool _blnCachedInfoFlag;
        public List<string> Infos
	    {
	        get
	        {
	            if (_blnCachedInfoFlag) return _lstCachedInfos;
	            foreach (DrugComponent d in Components.Where(d => d.ActiveDrugEffect != null))
	            {
	                _lstCachedInfos.AddRange(d.ActiveDrugEffect.Infos);
                }

	            _lstCachedInfos = _lstCachedInfos.Distinct().ToList();
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
                _intCachedInitiative = Components.Sum(d => d.ActiveDrugEffect?.Initiative ?? 0);
                return _intCachedInitiative;
            }
        }

        private int _intCachedInitiativeDice = int.MinValue;
        public int InitiativeDice
        {
            get
            {
                if (_intCachedInitiativeDice != int.MinValue) return _intCachedInitiativeDice;
                _intCachedInitiativeDice = Components.Sum(d => d.ActiveDrugEffect?.InitiativeDice ?? 0);
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
                _intCachedSpeed = Components.Sum(d => d.ActiveDrugEffect.Speed) + _intSpeed;
                return _intCachedSpeed;
            }
        }

        private int _intCachedDuration = int.MinValue;
        public int Duration
        {
            get
            {
                if (_intCachedDuration != int.MinValue) return _intCachedDuration;
                if (!string.IsNullOrWhiteSpace(_strDuration))
                {
                    StringBuilder strbldDrain = new StringBuilder(_strDuration);
                    foreach (string strAttribute in AttributeSection.AttributeStrings)
                    {
                        CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                        strbldDrain.CheapReplace(_strDuration, objAttrib.Abbrev,
                            () => objAttrib.TotalValue.ToString());
                    }

                    string strDuration = strbldDrain.ToString();
                    if (!int.TryParse(strDuration, out int intDuration))
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strDuration, out bool blnIsSuccess);
                        if (blnIsSuccess)
                            intDuration = Convert.ToInt32(objProcess);
                    }

                    _intCachedDuration = intDuration;
                }
                else
                {
                    _intCachedDuration = 0;
                }

                _intCachedDuration += Components.Sum(d => d.ActiveDrugEffect.Duration) + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DrugDuration);
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DrugDurationMultiplier) == 0)
                    return _intCachedDuration;
                decimal decMultiplier = 1;
                decMultiplier = _objCharacter.Improvements
                    .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DrugDurationMultiplier && objImprovement.Enabled)
                    .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                _intCachedDuration *= Convert.ToInt32(1.0m - decMultiplier);
                return _intCachedDuration;
            }
        }

        public CommonFunctions.Timescale DurationTimescale { get; private set; }

        private string _strCachedDisplayDuration;
        public string DisplayDuration
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_strCachedDisplayDuration)) return _strCachedDisplayDuration;
                StringBuilder sb = new StringBuilder();
                if (Duration > 0)
                {
                    sb.Append(Duration);
                    sb.Append(LanguageManager.GetString("String_Space"));
                    if (DurationDice > 0)
                    {
                        sb.Append($"x {DurationDice}{LanguageManager.GetString("String_D6")}");
                        sb.Append(LanguageManager.GetString("String_Space"));
                    }
                }

                sb.Append(CommonFunctions.GetTimescaleString(DurationTimescale, Duration > 1));
                _strCachedDisplayDuration = sb.ToString();

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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return LanguageManager.TranslateExtra(Name, strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            if (Quantity != 1)
                strReturn = Quantity.ToString("#,0.##", objCulture) + strSpaceCharacter + strReturn;
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

        public Dictionary<string, int> Attributes
        {
            get
            {
                if (_blnCachedAttributeFlag)
                    return _dicCachedAttributes;
                _dicCachedAttributes = new Dictionary<string, int>();
                foreach (DrugComponent objComponent in Components)
                {
                    foreach (DrugEffect objDrugEffect in objComponent.DrugEffects)
                    {
                        if (objDrugEffect.Level == objComponent.Level && objDrugEffect.Attributes.Count > 0)
                        {
                            foreach (KeyValuePair<string, int> objAttributeEntry in objDrugEffect.Attributes)
                            {
                                if (_dicCachedAttributes.ContainsKey(objAttributeEntry.Key))
                                    _dicCachedAttributes[objAttributeEntry.Key] += objAttributeEntry.Value;
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
        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }

                return SystemColors.WindowText;
            }
        }


        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D");

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        #endregion
        #region UI Methods
        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public TreeNode CreateTreeNode()
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language),
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;

            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }
        #endregion
        #region Methods
        public string GenerateDescription(int intLevel = -1, bool blnEffectsOnly = false, string strLanguage = "", CultureInfo objCulture = null, bool blnDoCache = true)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            if (objCulture == null)
                objCulture = GlobalOptions.CultureInfo;
            StringBuilder strbldDescription = new StringBuilder();
			bool blnNewLineFlag = false;
            string strSpaceString = LanguageManager.GetString("String_Space", strLanguage);
            string strColonString = LanguageManager.GetString("String_Colon", strLanguage);
            if (!blnEffectsOnly)
            {
                string strName = DisplayNameShort(strLanguage);
                if (!string.IsNullOrWhiteSpace(strName))
                    strbldDescription.AppendLine(strName);
            }

            if (intLevel != -1)
			{
                foreach (KeyValuePair<string, int> objAttribute in Attributes)
				{
                    if (objAttribute.Value != 0)
					{
					    if (blnNewLineFlag)
					    {
					        strbldDescription.Append(',').Append(strSpaceString);
					    }

                        strbldDescription.Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short", strLanguage))
                            .Append(strSpaceString).Append(objAttribute.Value.ToString("+#;-#"));
						blnNewLineFlag = true;
					}
				}
				if (blnNewLineFlag)
				{
					blnNewLineFlag = false;
					strbldDescription.AppendLine();
				}

				foreach (KeyValuePair<string, int> objLimit in Limits)
				{
                    if (objLimit.Value != 0)
					{
					    if (blnNewLineFlag)
					    {
					        strbldDescription.Append(',').Append(strSpaceString);
					    }

                        strbldDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key, strLanguage)).Append(strSpaceString).Append(LanguageManager.GetString("String_Limit", strLanguage)).Append(strSpaceString)
					        .Append(objLimit.Value.ToString(" +#;-#"));
                        blnNewLineFlag = true;
					}
				}
				if (blnNewLineFlag)
				{
					strbldDescription.AppendLine();
				}

				if (Initiative != 0 || InitiativeDice != 0)
				{
					strbldDescription.Append(LanguageManager.GetString("String_AttributeINILong", strLanguage)).Append(strSpaceString);
				    if (Initiative != 0)
				    {
				        strbldDescription.Append(Initiative.ToString("+#;-#"));
				        if (InitiativeDice != 0)
				            strbldDescription.Append(InitiativeDice.ToString("+#;-#")).Append(LanguageManager.GetString("String_D6", strLanguage));
                    }
				    else if (InitiativeDice != 0)
						strbldDescription.Append(InitiativeDice.ToString("+#;-#")).Append(LanguageManager.GetString("String_D6", strLanguage));
					strbldDescription.AppendLine();
				}

				foreach (XmlNode nodQuality in Qualities)
					strbldDescription.Append(LanguageManager.TranslateExtra(nodQuality.InnerText, strLanguage)).Append(strSpaceString).AppendLine(LanguageManager.GetString("String_Quality", strLanguage));
				foreach (string strInfo in Infos)
					strbldDescription.AppendLine(LanguageManager.TranslateExtra(strInfo, strLanguage));

				if (Category == "Custom Drug" || Duration != 0)
					strbldDescription.Append(LanguageManager.GetString("Label_Duration", strLanguage)).AppendLine(DisplayDuration);

				if (Category == "Custom Drug" || Speed != 0)
				{
				    strbldDescription.Append(LanguageManager.GetString("Label_Speed")).Append(strColonString).Append(strSpaceString);
				    if (Speed <= 0)
				        strbldDescription.AppendLine(LanguageManager.GetString("String_Immediate"));
				    else if (Speed <= 60)
				        strbldDescription.AppendLine((Speed / 3).ToString(GlobalOptions.CultureInfo) + strSpaceString + LanguageManager.GetString("String_CombatTurns"));
				    else
				        strbldDescription.AppendLine((Speed).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Seconds"));
                }

			    if (CrashDamage != 0)
			        strbldDescription.Append(LanguageManager.GetString("Label_CrashEffect", strLanguage)).Append(strSpaceString)
			            .Append(CrashDamage.ToString(objCulture)).Append(LanguageManager.GetString("String_DamageStun", strLanguage)).Append(strSpaceString)
			            .AppendLine(LanguageManager.GetString("String_DamageUnresisted", strLanguage));
			    if (!blnEffectsOnly)
			    {
			        strbldDescription.Append(LanguageManager.GetString("Label_AddictionRating", strLanguage)).Append(strSpaceString).AppendLine((AddictionRating * (intLevel + 1)).ToString(objCulture));
			        strbldDescription.Append(LanguageManager.GetString("Label_AddictionThreshold", strLanguage)).Append(strSpaceString).AppendLine((AddictionThreshold * (intLevel + 1)).ToString(objCulture));
			        strbldDescription.Append(LanguageManager.GetString("Label_Cost", strLanguage)).Append(strSpaceString).Append((Cost * (intLevel + 1)).ToString(_objCharacter.Options.NuyenFormat, objCulture)).AppendLine("¥");
			        strbldDescription.Append(LanguageManager.GetString("Label_Avail", strLanguage)).Append(strSpaceString).AppendLine(TotalAvail(objCulture, strLanguage));
			    }
			}
			else if (!blnEffectsOnly)
            {
                strbldDescription.Append(LanguageManager.GetString("Label_AddictionRating", strLanguage)).Append(strSpaceString).AppendLine((AddictionRating * (intLevel + 1)).ToString(objCulture));
                strbldDescription.Append(LanguageManager.GetString("Label_AddictionThreshold", strLanguage)).Append(strSpaceString).AppendLine((AddictionThreshold * (intLevel + 1)).ToString(objCulture));
                strbldDescription.Append(LanguageManager.GetString("Label_Cost", strLanguage)).Append(strSpaceString).Append((Cost * (intLevel + 1)).ToString(_objCharacter.Options.NuyenFormat, objCulture)).AppendLine("¥");
                strbldDescription.Append(LanguageManager.GetString("Label_Avail", strLanguage)).Append(strSpaceString).AppendLine(TotalAvail(objCulture, strLanguage));
            }

            string strReturn = strbldDescription.ToString();
            if (blnDoCache)
                _strDescription = strReturn;
			return strReturn;
		}

        /// <summary>
        /// Creates the improvements necessary to to 'activate' a given drug.
        /// TODO: I'm really not happy with the lack of extensibility on this.
        /// TODO: Refactor drug effects to just use XML nodes, which can then be passed to Improvement Manager?
        /// TODO: Refactor Improvement Manager to automatically collapse improvements of the same type into a single improvement?
        /// </summary>
        public void GenerateImprovement()
        {
            if (_objCharacter.Improvements.Any(ig => ig.SourceName == InternalId)) return;
            _objCharacter.ImprovementGroups.Add(Name);
            List<Improvement> lstImprovements = new List<Improvement>();

            foreach (KeyValuePair<string, int> objAttribute in Attributes)
            {
                if (objAttribute.Value == 0) continue;
                var i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    ImproveType = Improvement.ImprovementType.Attribute,
                    SourceName = InternalId,
                    Value = objAttribute.Value,
                    ImprovedName = objAttribute.Key
                };
                lstImprovements.Add(i);
            }

            foreach (KeyValuePair<string, int> objLimit in Limits)
            {
                if (objLimit.Value == 0) continue;
                var i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    Value = objLimit.Value
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
                var i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.Initiative,
                    Value = Initiative
                };
                lstImprovements.Add(i);
            }

            if (InitiativeDice != 0)
            {
                var i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.InitiativeDice,
                    Value = InitiativeDice
                };
                lstImprovements.Add(i);
            }

            if (Qualities.Count > 0)
            {
                XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
                foreach (XmlNode objXmlAddQuality in Qualities)
                {
                    XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlAddQuality.InnerText + "\"]");
                    string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerText ?? string.Empty;

                    string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerText;
                    int intCount = string.IsNullOrEmpty(strRating) ? 1 : ImprovementManager.ValueToInt(_objCharacter, strRating, 1);
                    bool blnDoesNotContributeToBP = !string.Equals(objXmlAddQuality.Attributes?["contributetobp"]?.InnerText, bool.TrueString, StringComparison.CurrentCultureIgnoreCase);

                    for (int i = 0; i < intCount; ++i)
                    {
                        // Makes sure we aren't over our limits for this particular quality from this overall source
                        if (objXmlAddQuality.Attributes?["forced"]?.InnerText == bool.TrueString ||
                            objXmlSelectedQuality.RequirementsMet(_objCharacter, LanguageManager.GetString("String_Quality", GlobalOptions.Language), string.Empty, Name))
                        {
                            List<Weapon> lstWeapons = new List<Weapon>();
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
                            ImprovementManager.CreateImprovement(_objCharacter, objAddQuality.InternalId, Improvement.ImprovementSource.Drug, InternalId, Improvement.ImprovementType.SpecificQuality,"");
                        }
                        else
                        {
                            throw new AbortedException();
                        }
                    }
                }
            }
            _objCharacter.Improvements.AddRange(lstImprovements);
            foreach (Improvement i in _objCharacter.Improvements.Where(imp => imp.ImprovedName == InternalId))
            {
                i.CustomName = "Test";
                i.CustomGroup = Name;
                i.Custom = true;
            }
        }
        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = SourceID == Guid.Empty
                    ? XmlManager.Load("drugcomponents.xml", strLanguage)
                        .SelectSingleNode($"/chummer/drugcomponents/drugcomponent[name = \"{Name}\"]")
                    : XmlManager.Load("drugcomponents.xml", strLanguage)
                        .SelectSingleNode($"/chummer/drugcomponents/drugcomponent[id = \"{SourceIDString}\" or id = \"{SourceIDString}\"]");

                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public bool Remove(Character characterObject, bool blnConfirmDelete)
        {
            if (blnConfirmDelete && !characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteDrug",
                    GlobalOptions.Language)))
            {
                return false;
            }
            characterObject.Drugs.Remove(this);
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Drug, InternalId);
            return true;
        }
        #endregion

    }
	/// <summary>
	/// Drug Component.
	/// </summary>
	public class DrugComponent : IHasName, IHasInternalId, IHasXmlNode
	{
        private Logger Log = LogManager.GetCurrentClassLogger();
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
	        _guidId = new Guid();
            _objCharacter = objCharacter;
		}

		#region Constructor, Create, Save, Load, and Print Methods
		public void Load(XmlNode objXmlData)
		{
		    objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
		    {
		        XmlNode node = GetNode(GlobalOptions.Language);
		        node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
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
                                    Log.Warn($"Unknown drug effect {objXmlEffect.Name} in component {strEffectName}");
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
            objXmlWriter.WriteElementString("sourceid", SourceIDString);
            objXmlWriter.WriteElementString("guid", InternalId);
            objXmlWriter.WriteElementString("name", _strName);
			objXmlWriter.WriteElementString("category", _strCategory);

			objXmlWriter.WriteStartElement("effects");
			foreach (DrugEffect objDrugEffect in DrugEffects)
			{
				objXmlWriter.WriteStartElement("effect");
				foreach (KeyValuePair<string, int> objAttribute in objDrugEffect.Attributes)
				{
					objXmlWriter.WriteStartElement("attribute");
					objXmlWriter.WriteElementString("name", objAttribute.Key);
					objXmlWriter.WriteElementString("value", objAttribute.Value.ToString());
					objXmlWriter.WriteEndElement();
				}
				foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
				{
					objXmlWriter.WriteStartElement("limit");
					objXmlWriter.WriteElementString("name", objLimit.Key);
					objXmlWriter.WriteElementString("value", objLimit.Value.ToString());
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
					objXmlWriter.WriteElementString("initiative", objDrugEffect.Initiative.ToString());
				if (objDrugEffect.InitiativeDice != 0)
					objXmlWriter.WriteElementString("initiativedice", objDrugEffect.InitiativeDice.ToString());
				if (objDrugEffect.Duration != 0)
					objXmlWriter.WriteElementString("duration", objDrugEffect.Duration.ToString());
				if (objDrugEffect.Speed != 0)
					objXmlWriter.WriteElementString("speed", objDrugEffect.Speed.ToString());
				if (objDrugEffect.CrashDamage != 0)
					objXmlWriter.WriteElementString("crashdamage", objDrugEffect.CrashDamage.ToString());
				objXmlWriter.WriteEndElement();
			}
			objXmlWriter.WriteEndElement();

		    objXmlWriter.WriteElementString("availability", _strAvailability);
            objXmlWriter.WriteElementString("cost", _strCost);
            objXmlWriter.WriteElementString("level", _intLevel.ToString());
            objXmlWriter.WriteElementString("limit", _intLimit.ToString());
            if (_intAddictionRating != 0)
				objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString());
			if (_intAddictionThreshold != 0)
				objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString());
            objXmlWriter.WriteElementString("source", _strSource);
            objXmlWriter.WriteElementString("page", _strPage);
        }
		#endregion
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
	        if (strLanguage == GlobalOptions.DefaultLanguage)
	            return Name;

	        XmlNode xmlGearDataNode = GetNode(strLanguage);
	        if (xmlGearDataNode?["name"]?.InnerText == "Custom Item")
	        {
	            return LanguageManager.TranslateExtra(Name, strLanguage);
	        }

	        return xmlGearDataNode?["translate"]?.InnerText ?? Name;
	    }

	    /// <summary>
	    /// The name of the object as it should be displayed in lists. Name (Level X).
	    /// </summary>
	    public string DisplayName(CultureInfo objCulture, string strLanguage)
	    {
	        string strReturn = DisplayNameShort(strLanguage);
	        if (Level != 0)
	        {
	            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
	            strReturn += strSpaceCharacter + '(' + LanguageManager.GetString("String_Level", strLanguage) + strSpaceCharacter + Level.ToString(objCulture) + ')';
	        }

	        return strReturn;
	    }

	    public string CurrentDisplayName => DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

	    /// <summary>
	    /// Translated Category.
	    /// </summary>
	    public string DisplayCategory(string strLanguage)
	    {
	        if (strLanguage == GlobalOptions.DefaultLanguage)
	            return Category;

	        return XmlManager.Load("drugcomponents.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
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
	    public string Page(string strLanguage)
	    {
	        if (strLanguage == GlobalOptions.DefaultLanguage)
	            return _strPage;

	        return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
	    }

		public List<DrugEffect> DrugEffects { get; } = new List<DrugEffect>();

        public DrugEffect ActiveDrugEffect => DrugEffects.FirstOrDefault(effect => effect.Level == Level);

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

                if (strCostExpression.StartsWith("FixedValues("))
                {
                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCostExpression = strValues[Math.Max(Math.Min(Level, strValues.Length) - 1, 0)].Trim('[', ']');
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                StringBuilder objCost = new StringBuilder(strCostExpression.TrimStart('+'));
                objCost.Replace("Level", Level.ToString());
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
            }
        }

        public string Availability
		{
			get => _strAvailability;
		    set => _strAvailability = value;
        }

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
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                /*
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }*/

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
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
	    public string SourceIDString => _guiSourceID.ToString("D");

        public string InternalId => _guidId.ToString("D");
        #endregion
        #region Methods
        public string GenerateDescription(int intLevel = -1)
		{
			if (intLevel >= DrugEffects.Count)
				return null;

			StringBuilder strbldDescription = new StringBuilder();
			bool blnNewLineFlag = false;
		    string strSpaceString = LanguageManager.GetString("String_Space");
		    string strColonString = LanguageManager.GetString("String_Colon");
            strbldDescription.Append(DisplayCategory(GlobalOptions.Language)).Append(strColonString).Append(strSpaceString).Append(CurrentDisplayName).AppendLine();

            if (intLevel != -1)
			{
				DrugEffect objDrugEffect = DrugEffects[intLevel];

				foreach (KeyValuePair<string, int> objAttribute in objDrugEffect.Attributes)
				{
                    if (objAttribute.Value != 0)
					{
					    if (blnNewLineFlag)
					    {
					        strbldDescription.Append(',').Append(strSpaceString);
					    }

                        strbldDescription.Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short"))
					        .Append(strSpaceString).Append(objAttribute.Value.ToString("+#;-#"));
                        blnNewLineFlag = true;
					}
                }
				if (blnNewLineFlag)
				{
					blnNewLineFlag = false;
					strbldDescription.AppendLine();
				}

				foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
				{
					if (objLimit.Value != 0)
					{
					    if (blnNewLineFlag)
					    {
					        strbldDescription.Append(',').Append(strSpaceString);
					    }

                        strbldDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key)).Append(strSpaceString).Append(LanguageManager.GetString("String_Limit")).Append(strSpaceString)
                            .Append(objLimit.Value.ToString(" +#;-#"));
                        blnNewLineFlag = true;
					}
				}
				if (blnNewLineFlag)
				{
					strbldDescription.AppendLine();
				}

				if (objDrugEffect.Initiative != 0 || objDrugEffect.InitiativeDice != 0)
				{
				    strbldDescription.Append(LanguageManager.GetString("String_AttributeINILong")).Append(strSpaceString);
				    if (objDrugEffect.Initiative != 0)
				    {
				        strbldDescription.Append(objDrugEffect.Initiative.ToString("+#;-#"));
				        if (objDrugEffect.InitiativeDice != 0)
				            strbldDescription.Append(objDrugEffect.InitiativeDice.ToString("+#;-#")).Append(LanguageManager.GetString("String_D6"));
				    }
				    else if (objDrugEffect.InitiativeDice != 0)
				        strbldDescription.Append(objDrugEffect.InitiativeDice.ToString("+#;-#")).Append(LanguageManager.GetString("String_D6"));
				    strbldDescription.AppendLine();
                }

			    foreach (XmlNode strQuality in objDrugEffect.Qualities)
			        strbldDescription.Append(LanguageManager.TranslateExtra(strQuality.InnerText, GlobalOptions.Language)).Append(strSpaceString).AppendLine(LanguageManager.GetString("String_Quality"));
			    foreach (string strInfo in objDrugEffect.Infos)
			        strbldDescription.AppendLine(LanguageManager.TranslateExtra(strInfo, GlobalOptions.Language));

				if (Category == "Custom Drug" || objDrugEffect.Duration != 0)
				    strbldDescription.Append(LanguageManager.GetString("Label_Duration")).Append(strColonString).Append(strSpaceString)
				        .Append("10 ⨯ ").Append((objDrugEffect.Duration + 1).ToString(GlobalOptions.CultureInfo)).Append(LanguageManager.GetString("String_D6")).Append(strSpaceString).AppendLine(LanguageManager.GetString("String_Minutes"));

                if (Category == "Custom Drug" || objDrugEffect.Speed != 0)
				{
				    strbldDescription.Append(LanguageManager.GetString("Label_Speed")).Append(strColonString).Append(strSpaceString);
				    if (objDrugEffect.Speed <= 0)
				        strbldDescription.AppendLine(LanguageManager.GetString("String_Immediate"));
				    else if (objDrugEffect.Speed <= 60)
				        strbldDescription.AppendLine((objDrugEffect.Speed / 3).ToString(GlobalOptions.CultureInfo) + strSpaceString + LanguageManager.GetString("String_CombatTurns"));
                    else
				        strbldDescription.AppendLine((objDrugEffect.Speed).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Seconds"));
                }

			    if (objDrugEffect.CrashDamage != 0)
			        strbldDescription.Append(LanguageManager.GetString("Label_CrashEffect")).Append(strSpaceString)
			            .Append(objDrugEffect.CrashDamage.ToString(GlobalOptions.CultureInfo)).Append(LanguageManager.GetString("String_DamageStun")).Append(strSpaceString)
			            .AppendLine(LanguageManager.GetString("String_DamageUnresisted"));

			    strbldDescription.Append(LanguageManager.GetString("Label_AddictionRating")).Append(strSpaceString).AppendLine((AddictionRating * (intLevel + 1)).ToString(GlobalOptions.CultureInfo));
			    strbldDescription.Append(LanguageManager.GetString("Label_AddictionThreshold")).Append(strSpaceString).AppendLine((AddictionThreshold * (intLevel + 1)).ToString(GlobalOptions.CultureInfo));
			    strbldDescription.Append(LanguageManager.GetString("Label_Cost")).Append(strSpaceString).Append((CostPerLevel * (intLevel + 1)).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)).AppendLine("¥");
			    strbldDescription.Append(LanguageManager.GetString("Label_Avail")).Append(strSpaceString).AppendLine(TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language));
			}
			else
            {
                string strPerLevel = LanguageManager.GetString("String_PerLevel");
                strbldDescription.Append(LanguageManager.GetString("Label_AddictionRating")).Append(strSpaceString).Append((AddictionRating * (intLevel + 1)).ToString(GlobalOptions.CultureInfo))
			        .Append(strSpaceString).AppendLine(strPerLevel);
			    strbldDescription.Append(LanguageManager.GetString("Label_AddictionThreshold")).Append(strSpaceString).Append((AddictionThreshold * (intLevel + 1)).ToString(GlobalOptions.CultureInfo))
                    .Append(strSpaceString).AppendLine(strPerLevel);
			    strbldDescription.Append(LanguageManager.GetString("Label_Cost")).Append(strSpaceString).Append((CostPerLevel * (intLevel + 1)).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo))
			        .Append("¥").Append(strSpaceString).AppendLine(strPerLevel);
			    strbldDescription.Append(LanguageManager.GetString("Label_Avail")).Append(strSpaceString).AppendLine(TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language));
			}

			return strbldDescription.ToString();
		}

	    public XmlNode GetNode()
	    {
	        return GetNode(GlobalOptions.Language);
	    }

	    public XmlNode GetNode(string strLanguage)
	    {
	        if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
	        {
	            _objCachedMyXmlNode = SourceID == Guid.Empty
	                ? XmlManager.Load("drugcomponents.xml", strLanguage)
	                    .SelectSingleNode($"/chummer/drugcomponents/drugcomponent[name = \"{Name}\"]")
	                : XmlManager.Load("drugcomponents.xml", strLanguage)
	                    .SelectSingleNode($"/chummer/drugcomponents/drugcomponent[id = \"{SourceIDString}\" or id = \"{SourceIDString}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
	        }
	        return _objCachedMyXmlNode;
	    }
        #endregion
    }
	/// <summary>
	/// Drug Effect
	/// </summary>
	public class DrugEffect : Object
	{
	    public DrugEffect()
		{
			Attributes = new Dictionary<string, int>();
			Limits = new Dictionary<string, int>();
			Qualities = new List<XmlNode>();
			Infos = new List<string>();
		}

		public Dictionary<string, int> Attributes { get; }

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
