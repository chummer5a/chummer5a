using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Annotations;
using Chummer.Skills;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

// ReSharper disable once CheckNamespace
namespace Chummer
{
    /// <summary>
    /// An Adept Power.
    /// </summary>
    public class Power : INotifyPropertyChanged, INamedItemWithGuidAndNode
    {
        private Guid _guiID;
        private Guid _sourceID = new Guid();
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strPointsPerLevel = "0";
        private string _strAction = string.Empty;
        private decimal _decExtraPointCost = 0;
        private int _intMaxLevel = 0;
        private bool _blnDiscountedAdeptWay = false;
        private bool _blnDiscountedGeas = false;
        private XmlNode _nodAdeptWayRequirements;
        private string _strNotes = string.Empty;
        private bool _blnFree = false;
        private int _intFreeLevels = 0;
        private string _strAdeptWayDiscount = "0";
        private string _strBonusSource = string.Empty;
        private decimal _decFreePoints = 0;
        private string _strDisplayName = string.Empty;
        private string _displayPoints = string.Empty;

        #region Constructor, Create, Save, Load, and Print Methods
        public Power(Character objCharacter)
        {
            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("power");
            objWriter.WriteElementString("id", _sourceID.ToString());
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", Name);
            objWriter.WriteElementString("extra", Extra);
            objWriter.WriteElementString("pointsperlevel", _strPointsPerLevel);
            objWriter.WriteElementString("adeptway", _strAdeptWayDiscount);
            objWriter.WriteElementString("action", _strAction);
            objWriter.WriteElementString("rating", Rating.ToString());
            objWriter.WriteElementString("extrapointcost", _decExtraPointCost.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("levels", LevelsEnabled.ToString());
            objWriter.WriteElementString("maxlevel", _intMaxLevel.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("discounted", _blnDiscountedAdeptWay.ToString());
            objWriter.WriteElementString("discountedgeas", _blnDiscountedGeas.ToString());
            objWriter.WriteElementString("bonussource", _strBonusSource);
            objWriter.WriteElementString("freepoints", _decFreePoints.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("freelevels", _intFreeLevels.ToString(CultureInfo.InvariantCulture));
            if (Bonus != null)
                objWriter.WriteRaw("<bonus>" + Bonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodAdeptWayRequirements != null)
                objWriter.WriteRaw("<adeptwayrequires>" + _nodAdeptWayRequirements.InnerXml + "</adeptwayrequires>");
            else
                objWriter.WriteElementString("adeptwayrequires", string.Empty);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            CharacterObject.SourceProcess(_strSource);
        }

        public bool Create(XmlNode objNode, int intRating = 1, XmlNode objBonusNodeOverride = null)
        {
            Name = objNode["name"].InnerText;
            _sourceID = Guid.Parse(objNode["id"].InnerText);
            objNode.TryGetStringFieldQuickly("points", ref _strPointsPerLevel);
            objNode.TryGetStringFieldQuickly("adeptway", ref _strAdeptWayDiscount);
            LevelsEnabled = Convert.ToBoolean(objNode["levels"].InnerText);
            Rating = intRating;
            objNode.TryGetInt32FieldQuickly("maxlevels", ref _intMaxLevel);
            objNode.TryGetBoolFieldQuickly("discounted", ref _blnDiscountedAdeptWay);
            objNode.TryGetBoolFieldQuickly("discountedgeas", ref _blnDiscountedGeas);
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetDecFieldQuickly("extrapointcost", ref _decExtraPointCost);
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            Bonus = objNode["bonus"];
            if (objBonusNodeOverride != null)
                Bonus = objBonusNodeOverride;
            _nodAdeptWayRequirements = objNode["adeptwayrequires"];
            if (objNode.InnerXml.Contains("enhancements"))
            {
                XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
                if (nodEnhancements != null)
                    foreach (XmlNode nodEnhancement in nodEnhancements)
                    {
                        Enhancement objEnhancement = new Enhancement(CharacterObject);
                        objEnhancement.Load(nodEnhancement);
                        objEnhancement.Parent = this;
                        Enhancements.Add(objEnhancement);
                    }
            }
            if (Bonus != null && Bonus.HasChildNodes)
            {
                if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId, Bonus, false, TotalRating, DisplayNameShort))
                {
                    this.Deleting = true;
                    CharacterObject.Powers.Remove(this);
                    OnPropertyChanged(nameof(TotalRating));
                    return false;
                }
                Extra = ImprovementManager.SelectedValue;
            }
            if (TotalMaximumLevels < Rating)
            {
                Rating = TotalMaximumLevels;
                OnPropertyChanged(nameof(TotalRating));
            }
            return true;
        }

        /// <summary>
        /// Load the Power from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            Name = objNode["name"].InnerText;
            if (objNode["id"] != null)
            {
                _sourceID = Guid.Parse(objNode["id"].InnerText);
            }
            else
            {
                string strPowerName = Name;
                if (strPowerName.Contains('('))
                    strPowerName = strPowerName.Substring(0, strPowerName.IndexOf('(') - 1);
                XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                if (objXmlPower != null) _sourceID = Guid.Parse(objXmlPower["id"].InnerText);
            }
            Extra = objNode["extra"].InnerText ?? string.Empty;
            _strPointsPerLevel = objNode["pointsperlevel"]?.InnerText;
            objNode.TryGetField("action", out _strAction);
            if (objNode["adeptway"] != null)
                _strAdeptWayDiscount = objNode["adeptway"].InnerText;
            else
            {
                string strPowerName = Name;
                if (strPowerName.Contains('('))
                    strPowerName = strPowerName.Substring(0, strPowerName.IndexOf('(') - 1);
                XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                if (objXmlPower?["adeptway"] != null)
                    _strAdeptWayDiscount = objXmlPower["adeptway"].InnerText;
            }
            Rating = Convert.ToInt32(objNode["rating"]?.InnerText);
            LevelsEnabled = Convert.ToBoolean(objNode["levels"]?.InnerText);
            objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
            objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevel);
            objNode.TryGetInt32FieldQuickly("freelevels", ref _intFreeLevels);
            objNode.TryGetBoolFieldQuickly("discounted", ref _blnDiscountedAdeptWay);
            objNode.TryGetBoolFieldQuickly("discountedgeas", ref _blnDiscountedGeas);
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetDecFieldQuickly("extrapointcost", ref _decExtraPointCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            Bonus = objNode["bonus"];
            if (objNode["adeptway"] != null)
            {
                if (objNode["adeptwayrequires"] == null)
                {
                    XmlNode objXmlPower = MyXmlNode;
                    if (objXmlPower != null)
                        _nodAdeptWayRequirements = objXmlPower["adeptwayrequires"];
                }
                else
                {
                    _nodAdeptWayRequirements = objNode["adeptwayrequires"];
                }
            } 
            if (!objNode.InnerXml.Contains("enhancements")) return;
            XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
            if (nodEnhancements == null) return;
            foreach (XmlNode nodEnhancement in nodEnhancements)
            {
                Enhancement objEnhancement = new Enhancement(CharacterObject);
                objEnhancement.Load(nodEnhancement);
                objEnhancement.Parent = this;
                Enhancements.Add(objEnhancement);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("power");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra));
            objWriter.WriteElementString("pointsperlevel", PointsPerLevel.ToString());
            objWriter.WriteElementString("adeptway", AdeptWayDiscount.ToString());
            objWriter.WriteElementString("rating", LevelsEnabled ? Rating.ToString() : "0");
            objWriter.WriteElementString("totalpoints", PowerPoints.ToString());
            objWriter.WriteElementString("action", DisplayAction);
            objWriter.WriteElementString("source", CharacterObject.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Print(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Character object being used by the Power.
        /// </summary>
        public Character CharacterObject { get; }

        /// <summary>
        /// Internal identifier which will be used to identify this Power in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString();

        /// <summary>
        /// Power's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra { get; set; } = string.Empty;

        /// <summary>
        /// The Enhancements currently applied to the Power.
        /// </summary>
        public List<Enhancement> Enhancements { get; } = new List<Enhancement>();

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                string strReturn = Name;
                //Cache the displayname the first time it's called. 
                //TODO: Obsolete if the program ever switches to dynamically changing languages. 
                if (!string.IsNullOrWhiteSpace(_strDisplayName))
                {
                    strReturn = _strDisplayName;
                }
                // Get the translated name if applicable.
                else if (GlobalOptions.Language != "en-us")
                {
                    _strDisplayName = MyXmlNode?["translate"]?.InnerText ?? strReturn;
                    strReturn = _strDisplayName;
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The translated name of the Power (Name + any Extra text).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                if (string.IsNullOrEmpty(Extra)) return strReturn;
                LanguageManager.Load(GlobalOptions.Language, this);
                // Attempt to retrieve the CharacterAttribute name.
                string strTranslateName = LanguageManager.GetString("String_Attribute" + Extra + "Short", false);
                if (!string.IsNullOrEmpty(strTranslateName))
                    strReturn += " (" + strTranslateName + ")";
                else
                    strReturn += " (" + LanguageManager.TranslateExtra(Extra) + ")";

                return strReturn;
            }
        }

        /// <summary>
        /// Power Point cost per level of the Power.
        /// </summary>
        public decimal PointsPerLevel
        {
            get
            {
                decimal decReturn = Convert.ToDecimal(_strPointsPerLevel,GlobalOptions.InvariantCultureInfo);
                return decReturn;
            }
            set
            {
                _strPointsPerLevel = value.ToString();
            }
        }

        /// <summary>
        /// An additional cost on top of the power's PointsPerLevel. 
        /// Example: Improved Reflexes is properly speaking Rating + 0.5, but the math for that gets weird. 
        /// </summary>
        public decimal ExtraPointCost
        {
            get { return _decExtraPointCost; }
            set { _decExtraPointCost = value; }
        }

        /// <summary>
        /// Power Point discount for an Adept Way.
        /// </summary>
        public decimal AdeptWayDiscount
        {
            get
            {
                decimal decReturn = Convert.ToDecimal(_strAdeptWayDiscount, GlobalOptions.InvariantCultureInfo);
                return decReturn;
            }
            set
            {
                _strAdeptWayDiscount = value.ToString();
            }
        }

        /// <summary>
        /// Calculated Power Point cost per level of the Power (including discounts).
        /// </summary>
        public decimal CalculatedPointsPerLevel => PointsPerLevel;

        /// <summary>
        /// Calculate the discount that is applied to the Power.
        /// </summary>
        private decimal Discount => _blnDiscountedAdeptWay ? AdeptWayDiscount : 0;

        /// <summary>
        /// The current Rating of the Power.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// The current Rating of the Power, including any Free Levels. 
        /// </summary>
        public int TotalRating
        {
            get { return Math.Min(Rating + FreeLevels, TotalMaximumLevels); }
            set
            {
                Rating = Math.Max(value - FreeLevels, 0);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Free levels of the power.
        /// </summary>
        public int FreeLevels
        {
            get
            {
                decimal decExtraCost = FreePoints;
                // Rating does not include free levels from improvements, and those free levels can be used to buy the first level of a power so that Qi Foci, so need to check for those first
                int intReturn = CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra).Sum(objImprovement => objImprovement.Rating);
                // The power has an extra cost, so free PP from things like Qi Foci have to be charged first. 
                if (Rating + intReturn == 0 && ExtraPointCost > 0)
                {
                    decExtraCost -= (PointsPerLevel + ExtraPointCost);
                    if (decExtraCost >= 0)
                    {
                        intReturn += 1;
                    }
                    for (decimal i = decExtraCost; (i - 1 >= 0); i--)
                    {
                        intReturn += 1;
                    }
                }
                //Either the first level of the power has been paid for with PP, or the power doesn't have an extra cost.
                else
                {
                    for (decimal i = decExtraCost; i - PointsPerLevel >= 0; i-= PointsPerLevel)
                    {
                        intReturn += 1;
                    }

                }
                return Math.Min(intReturn, CharacterObject.MAG.TotalValue);
            }
        }

        /// <summary>
        /// Total number of Power Points the Power costs.
        /// </summary>
        public decimal PowerPoints
        {
            get
            {
                decimal decReturn = 0;
                if (_blnFree || Rating == 0 || !LevelsEnabled && FreeLevels > 0)
                {
                    return decReturn;
                }

                if (FreeLevels * PointsPerLevel >= FreePoints)
                {
                    decReturn = Rating * PointsPerLevel;
                    decReturn += ExtraPointCost;
                }
                else
                {
                    decReturn = TotalRating * PointsPerLevel + ExtraPointCost;
                    decReturn -= FreePoints;
                }
                decReturn -= Discount;
                return Math.Max(decReturn, 0);
            }            
        }

        public string DisplayPoints
        {
            get
            {
                if (!string.IsNullOrEmpty(_displayPoints))
                    return _displayPoints;
                else
                    return PowerPoints.ToString("G29");
            }
            set { _displayPoints = value; }
        }

        /// <summary>
        /// Bonus source.
        /// </summary>
        public string BonusSource
        {
            get
            {
                return _strBonusSource;
            }
            set
            {
                _strBonusSource = value;
            }
        }

        /// <summary>
        /// Free Power Points that apply to the Power. Calculated as Improvement Rating * 0.25.
        /// Typically used for Qi Foci. 
        /// </summary>
        public decimal FreePoints
        {
            get
            {
                int intRating = CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreePoints && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra).Sum(objImprovement => objImprovement.Rating);
                decimal decReturn = (decimal) (intRating * 0.25);
                return decReturn;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        /// <summary>
        /// Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                // Get the translated name if applicable.
                if (GlobalOptions.Language == "en-us")
                    return _strPage;
                return MyXmlNode?["altpage"]?.InnerText ?? _strPage;
            }
            set
            {
                _strPage = value;
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus { get; set; }

        /// <summary>
        /// Whether or not Levels enabled for the Power.
        /// </summary>
        public bool LevelsEnabled { get; set; }

        /// <summary>
        /// Maximum Level for the Power.
        /// </summary>
        public int MaxLevels
        {
            get
            {
                return _intMaxLevel;
            }
            set
            {
                _intMaxLevel = value;
            }
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 25% from Adept Way.
        /// </summary>
        public bool DiscountedAdeptWay
        {
            get
            {
                return _blnDiscountedAdeptWay;
            }
            set
            {
                _blnDiscountedAdeptWay = value;
            }
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 25% from Geas.
        /// </summary>
        public bool DiscountedGeas
        {
            get
            {
                return _blnDiscountedGeas;
            }
            set
            {
                _blnDiscountedGeas = value;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
        /// Action.
        /// </summary>
        public string Action
        {
            get
            {
                return _strAction;
            }
            set
            {
                _strAction = value;
            }
        }

        /// <summary>
        /// Translated Action.
        /// </summary>
        public string DisplayAction
        {
            get
            {
                string strReturn = string.Empty;

                switch (_strAction)
                {
                    case "Auto":
                        strReturn = LanguageManager.GetString("String_ActionAutomatic");
                        break;
                    case "Free":
                        strReturn = LanguageManager.GetString("String_ActionFree");
                        break;
                    case "Simple":
                        strReturn = LanguageManager.GetString("String_ActionSimple");
                        break;
                    case "Complex":
                        strReturn = LanguageManager.GetString("String_ActionComplex");
                        break;
                    case "Interrupt":
                        strReturn = LanguageManager.GetString("String_ActionInterrupt");
                        break;
                    case "Special":
                        strReturn = LanguageManager.GetString("String_SpellDurationSpecial");
                        break;
                }

                return strReturn;
            }
        }

        #endregion

        #region Complex Properties 

        public int TotalMaximumLevels
        {
            get
            {
                if (!LevelsEnabled)
                    return 1;
                int intReturn = MaxLevels;
                if (intReturn == 0)
                {
                    intReturn = Math.Max(intReturn, CharacterObject.MAG.TotalValue);
                }
                if (Name == "Improved Ability (skill)")
                {
                    Skill objBoostedSkill = CharacterObject.SkillsSection.GetActiveSkill(Extra);
                    if (objBoostedSkill != null)
                    {
                        // +1 at the end so that division of 2 always rounds up, and integer division by 2 is significantly less expensive than decimal/double division
                        intReturn = Math.Min(intReturn, (objBoostedSkill.Base + objBoostedSkill.Karma + 1) / 2);
                    }
                }
                if (!CharacterObject.IgnoreRules)
                {
                    intReturn = Math.Min(intReturn, CharacterObject.MAG.TotalValue);
                }
                return intReturn;
            }
        }

        /// <summary>
        /// Whether the power can be discounted due to presence of an Adept Way. 
        /// </summary>
        public bool AdeptWayDiscountEnabled
        {
            get
            {
                bool blnReturn = false;
                if (AdeptWayDiscount == 0)
                {
                    return false;
                }
                if (_nodAdeptWayRequirements?["magicianswayforbids"] == null)
                {
                    blnReturn = CharacterObject.Qualities.Any(objQuality => objQuality.Name == "The Magician's Way");
                }
                if (!blnReturn)
                {
                    XmlNodeList objXmlRequiredList = _nodAdeptWayRequirements?.SelectNodes("required/oneof/quality");
                    if (objXmlRequiredList != null)
                    {
                        foreach (XmlNode objNode in objXmlRequiredList)
                        {
                            if (objNode.Attributes?["extra"] != null)
                            {
                                blnReturn = CharacterObject.Qualities.Any(objQuality => objQuality.Name == objNode.InnerText && LanguageManager.TranslateExtra(objQuality.Extra) == objNode.Attributes["extra"].InnerText);
                                if (blnReturn)
                                    break;
                            }
                            else
                            {
                                blnReturn = CharacterObject.Qualities.Any(objQuality => objQuality.Name == objNode.InnerText);
                                if (blnReturn)
                                    break;
                            }
                        }
                    }
                }
                if (blnReturn == false && DiscountedAdeptWay)
                {
                    DiscountedAdeptWay = false;
                }
                return blnReturn;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
            if (Bonus?.InnerXml.Contains("Rating") == true && propertyName == nameof(TotalRating))
            {
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId);
                if (!Deleting)
                {
                    ImprovementManager.ForcedValue = Extra;
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId, Bonus, false, TotalRating, DisplayNameShort);
                }
            }
        }

        /// <summary>
        /// Is the currently power being deleted? 
        /// Ugly hack to prevent powers with Ratings recreating their improvments when they're being deleted. TODO: FIX THIS BETTER
        /// </summary>
        public bool Deleting { internal get; set; }

        public string Category { get; set; }

        public XmlNode MyXmlNode
        {
            get
            {
                return XmlManager.Load("powers.xml")?.SelectSingleNode("/chummer/powers/power[id = \"" + _sourceID.ToString() + "\"]");
            }
        }

        /// <summary>
        /// ToolTip that shows how the Power is calculating its Modified Rating.
        /// </summary>
        public string ToolTip()
        {
            string strReturn = $"Rating ({Rating} x {PointsPerLevel})";
            string strModifier = CharacterObject.Improvements.Where(objImprovement => objImprovement.Enabled)
                .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra)
                .Aggregate("", (current, objImprovement) => current + $" + {CharacterObject.GetObjectName(objImprovement)} ({objImprovement.Rating})");

            return strReturn + strModifier;
        }

        /// <summary>
        /// Forces a particular event. Currently used for forcing Qi Foci to update Single-Level powers. //TODO: Better way to implement this?
        /// </summary>
        /// <param name="property"></param>
        public void ForceEvent(string property)
        {
            var v = new PropertyChangedEventArgs(property);
            PropertyChanged?.Invoke(this, v);
        }
        #endregion
    }
}
